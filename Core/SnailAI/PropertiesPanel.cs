using System.Drawing;
using System.IO;
using Rhino.UI;
using SnailAI;

namespace SnailAI
{
    public class ViewportPropertiesPage : ObjectPropertiesPage
    {
        public override string EnglishPageTitle => "SnailAI";

        public override PropertyPageType PageType => PropertyPageType.View;

        public sealed override object PageControl
        {
            get
            {
                if (SnailPropertiesUiPanel == null || SnailPropertiesUiPanel.IsDisposed)
                    SnailPropertiesUiPanel = NewPanel();
                return SnailPropertiesUiPanel;
            }
        }

        public SnailPropertiesUiPanel SnailPropertiesUiPanel { get; set; }

        //public override Icon PageIcon(Size sizeInPixels)
        //{
        //    Icon icon = null;
        //    var byteArray = (byte[])Cyclops.Properties.Resources.ResourceManager.GetObject("CyclopsLogo");
        //    using (var ms = new MemoryStream(byteArray))
        //        icon = new Icon(ms, sizeInPixels.Width, sizeInPixels.Height);
        //    return icon;
        //}

        public override bool ShouldDisplay(ObjectPropertiesPageEventArgs e)
        {
            return e.Viewport != null && e.Viewport.DisplayMode.EnglishName == "SnailAI";
        }

        public override void UpdatePage(ObjectPropertiesPageEventArgs e)
        {
            //SnailPropertiesUiPanel?.UpdateInfo();
        }

        private static SnailPropertiesUiPanel NewPanel()
        {
            return new SnailPropertiesUiPanel();
        }
    }
}