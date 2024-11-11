using Eto.Drawing;
using Rhino;
using Rhino.Display;
using System;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using FalAPI;
using Newtonsoft.Json;
using Rhino.UI;
using SnailAI;
using SnailAI.Properties;

namespace SnailAI
{
    ///<summary>
    /// <para>Every RhinoCommon .rhp assembly must have one and only one PlugIn-derived
    /// class. DO NOT create instances of this class yourself. It is the
    /// responsibility of Rhino to create an instance of this class.</para>
    /// <para>To complete plug-in information, please also see all PlugInDescription
    /// attributes in AssemblyInfo.cs (you might need to click "Project" ->
    /// "Show All Files" to see it in the "Solution Explorer" window).</para>
    ///</summary>
    public class SnailAIPlugin : Rhino.PlugIns.PlugIn
    {

        public static string season = "Summer";
        public static string time = "Morning";

        private readonly ViewportPropertiesPage _mPage;

        public Lazy<FalAPI.FalClient> clientLowQuality;
        public Lazy<FalAPI.FalClient> clientMediumQuality;
        public Lazy<FalAPI.FalClient> clientHighQuality;
        public Stopwatch timer = new Stopwatch();
        public readonly bool[] queCapture = new[] { false };
        public SemaphoreSlim mutex = new SemaphoreSlim(1);


        public SnailAIPlugin()
        {
            clientLowQuality = new Lazy<FalAPI.FalClient>(() => new FalAPI.FalClient(FalClientSettings.LowAPIEndpoint, FalClientSettings.APIKey, "Low"));
            clientMediumQuality = new Lazy<FalAPI.FalClient>(() => new FalAPI.FalClient(FalClientSettings.MedAPIEndpoint, FalClientSettings.APIKey, "Medium"));
            clientHighQuality = new Lazy<FalAPI.FalClient>(() => new FalAPI.FalClient(FalClientSettings.HighAPIEndpoint, FalClientSettings.APIKey, "High"));

            _mPage = new ViewportPropertiesPage();
            RhinoView.Modified += QueCaptureView;
            Rhino.RhinoDoc.ReplaceRhinoObject += QueCaptureModelChange;
            Rhino.RhinoDoc.AddRhinoObject += QueCaptureModelChange;
            Rhino.RhinoDoc.DeleteRhinoObject += QueCaptureModelChange;
            Rhino.RhinoDoc.UndeleteRhinoObject += QueCaptureModelChange;

            Rhino.Display.DisplayPipeline.DrawForeground += DrawForeground;
            Instance = this;
            Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        await RequestImage();
                        Thread.Sleep(10);
                    }
                    catch
                    { }
                }
            });
        }

        public async Task<bool> RequestImage()
        {

            if (timer.Elapsed < TimeSpan.FromMilliseconds(50) || !queCapture[0])
                return false;

            var internaltimer = new Stopwatch();
            internaltimer.Start();

            queCapture[0] = false;

            // Viewport
            var view = RhinoDoc.ActiveDoc.Views.ActiveView;
            if (view == null)
                return false;
            var size = view.Size;

            System.Drawing.Bitmap viewportBitmap = null;
            RhinoApp.InvokeAndWait(() =>
            {
                isCapture = true;
                viewportBitmap = view.CaptureToBitmap(size/2, false, false, false);
                isCapture = false;
            });

            if (queCapture[0])
                return false;

            //string season = _mPage.SnailPropertiesUiPanel?._section._seasonValue.SelectedValue.ToString();
            //string time = _mPage.SnailPropertiesUiPanel?._section._timeOfDayValue.SelectedValue.ToString();


            // Low
            RhinoApp.WriteLine("View Capture: " + internaltimer.Elapsed.TotalMilliseconds + "ms");
            internaltimer.Restart();
            string bitmapViewportString = ConvertBitmapToBase64String(viewportBitmap);
            var lowLesponse = await clientLowQuality.Value.SendRequestBitmap(
                season,
                time, 
                bitmapViewportString);
            var lowBitmap = await clientLowQuality.Value.PollForResultBitmap(lowLesponse, queCapture);
            if (lowBitmap == null) return false;
            mutex.Wait();
            bitmap = new DisplayBitmap(new System.Drawing.Bitmap(lowBitmap, size));
            mutex.Release();

            if (queCapture[0])
                return false;

            RhinoApp.WriteLine("Low quality: " + internaltimer.Elapsed.TotalMilliseconds + "ms");

            // Medium 
            internaltimer.Restart();

            RhinoDoc.ActiveDoc.Views.ActiveView?.Redraw();
            string bitmapLowString = ConvertBitmapToBase64String(lowBitmap);

            var mediumResponse = await clientMediumQuality.Value.SendRequestBitmap(
                season,
                time, 
                bitmapLowString, 
                bitmapViewportString);
            var mediumBitmap = await clientMediumQuality.Value.PollForResultBitmap(mediumResponse, queCapture);
            if (mediumBitmap == null) return false;
            mutex.Wait();
            bitmap = new DisplayBitmap(new System.Drawing.Bitmap(mediumBitmap, size));
            mutex.Release();

            if (queCapture[0])
                return false;
            RhinoApp.WriteLine("Medium quality: " + internaltimer.Elapsed.TotalMilliseconds + "ms");
            internaltimer.Restart();

            RhinoDoc.ActiveDoc.Views.ActiveView?.Redraw();

            // High 
            internaltimer.Restart();

            RhinoDoc.ActiveDoc.Views.ActiveView?.Redraw();
            string bitmapMediumString = ConvertBitmapToBase64String(mediumBitmap);

            var highResponse = await clientHighQuality.Value.SendRequestBitmap(
                
                season,
                time, 
                bitmapMediumString, bitmapViewportString);
            var highBitmap = await clientHighQuality.Value.PollForResultBitmap(highResponse, queCapture);
            if (highBitmap == null) return false;
            mutex.Wait();
            bitmap = new DisplayBitmap(new System.Drawing.Bitmap(highBitmap, size));
            mutex.Release();

            if (queCapture[0])
                return false;
            RhinoApp.WriteLine("High quality: " + internaltimer.Elapsed.TotalMilliseconds + "ms");
            internaltimer.Restart();

            RhinoDoc.ActiveDoc.Views.ActiveView?.Redraw();

            return true;
        }

        /// <summary>
        /// Convert the image bitmap to a Base64 string.
        /// </summary>
        /// <param name="bitmap">Bitmap of the image</param>
        /// <returns>Base64string of the image</returns>
        public static string ConvertBitmapToBase64String(System.Drawing.Bitmap bitmap)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                // Save the bitmap to the memory stream in BMP format
                bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);

                // Convert the byte array to a Base64 string
                byte[] byteArray = ms.ToArray();

                return Convert.ToBase64String(byteArray);
            }
        }

        protected override void ObjectPropertiesPages(ObjectPropertiesPageCollection collection)
        {
            collection.Add(_mPage);
        }

        ///<summary>Gets the only instance of the SnailAIPlugin plug-in.</summary>
        public static SnailAIPlugin Instance { get; private set; }

        public override Rhino.PlugIns.PlugInLoadTime LoadTime => Rhino.PlugIns.PlugInLoadTime.AtStartup;

        // You can override methods here to change the plug-in behavior on
        // loading and shut down, add options pages to the Rhino _Option command
        // and maintain plug-in wide options in a document.

        bool isCapture = false;

        DisplayBitmap bitmap;

        private void QueCaptureView(object sender, ViewEventArgs e)
        {
            if (e.View.ActiveViewport?.DisplayMode?.EnglishName == "SnailAI" && !isCapture)
            {
                mutex.Wait();
                bitmap = null;
                mutex.Release();
                timer.Restart();
                queCapture[0] = true;
            }
        }

        private void QueCaptureModelChange(object sender, EventArgs e)
        {
            mutex.Wait();
            bitmap = null;
            mutex.Release();
            timer.Restart();
            queCapture[0] = true;
        }

        private void DrawForeground(object sender, DrawEventArgs e)
        {
            if (e.Viewport?.DisplayMode?.EnglishName == "SnailAI")
            {
                mutex.Wait();
                if (!isCapture && bitmap != null)
                {
                    e.Display.DrawBitmap(bitmap, 0, 0);
                }
                mutex.Release();
            }
        }   
    
    }

}