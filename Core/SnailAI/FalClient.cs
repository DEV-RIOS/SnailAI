using System;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using Rhino;
using System.Diagnostics;

namespace FalAPI
{
    public class FalClient
    {
        /// <summary>
        /// Placeholder image URL for the FalAI API.
        /// </summary>
        public string DefaultImagePath = "https://raw.githubusercontent.com/comfyanonymous/ComfyUI/master/input/example.png";

        /// <summary>
        /// App ID for the FalAI API.
        /// </summary>
        public string appId { get; set; }

        /// <summary>
        /// Base URL for the FalAI API.
        /// </summary>
        private string _url;

        /// <summary>
        /// API key for the FalAI API.
        /// </summary>
        private string _apiKey;

        private string _name;
        /// <summary>
        /// .NET HTTP client for making requests to the FalAI API.
        /// </summary>
        private readonly HttpClient _apiClient;

        /// <summary>
        /// .NET HTTP client for handling image url, downloading and conversion
        /// </summary>
        private readonly HttpClient _imageClient;

        /// <summary>
        /// Header to use for sending image to the FalAI API.
        /// </summary>
        private string _imageHeader = "data:image/png;base64,";

        /// <summary>
        /// Constructor for the FalClient class.
        /// </summary>
        /// <param name="appId">Fal AI app id</param>
        /// <param name="apiKey">Fal AI API key</param>
        public FalClient(string appId, string apiKey, string name)
        {
            _name = name;
            this.appId = appId;
            _url = "https://queue.fal.run";
            _apiKey = apiKey;
            _apiClient = new HttpClient();
            _apiClient.DefaultRequestHeaders.Clear();
            _apiClient.DefaultRequestHeaders.Add("Authorization", $"Key {_apiKey}");
            _imageClient = new HttpClient();
        }

        /// <summary>
        /// Send a request to the FalAI API with the given prompt and image in bitmap format.
        /// </summary>
        /// <param name="prompt">String prompt</param>
        /// <param name="imageBmp">Image in BMP</param>
        /// <returns>Fal AI response</returns>
        public async Task<FalResponse?> SendRequestBitmap(string season, string timeofday, string imageBmp)
        {
            var apiPath = $"{_url}/{appId}";

            string? imageBase64 = null;
            if (imageBmp != null)
            {
                imageBase64 = $"{_imageHeader},{imageBmp}";
            }

            var tempPrompt = promt;
            tempPrompt = tempPrompt.Replace("SEASON", season);
            tempPrompt = tempPrompt.Replace("TIME", timeofday);


            var payload = new
            {
                cliptextencode_text = tempPrompt,
                loadimage_1 = imageBase64 ?? DefaultImagePath,
            };

            string jsonPayload = Newtonsoft.Json.JsonConvert.SerializeObject(payload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            try
            {
                // Make the POST request
                HttpResponseMessage response = await _apiClient.PostAsync(apiPath, content);
                response.EnsureSuccessStatusCode(); // Throw if the request failed

                // Get the response content as a string
                string result = await response.Content.ReadAsStringAsync();
                var data = Newtonsoft.Json.JsonConvert.DeserializeObject<FalResponse>(result);

                return data;
            }
            catch (HttpRequestException e)
            {
                RhinoApp.WriteLine($"Request error: {e.Message}");
                return null;
            }
        }

        private static readonly string promt = "Ultra-realistic architectural photography of a building, surrounded by a vibrant SEASON landscape. Landscape enliven the scene at TIME adding a sense of renewal and energy. The atmosphere is TIME and inviting, with a touch of whimsy highlighting the architectural features. The building's modern design is enhanced by 3D visualization techniques, with a color palette dominated by fresh SEASON, soft pastels, and crisp whites to reflect the rejuvenating spirit of SEASON.";

        /// <summary>
        /// Send a request to the FalAI API with the given prompt and image in bitmap format.
        /// </summary>
        /// <param name="prompt">String prompt</param>
        /// <param name="imageBmp">Image in BMP</param>
        /// <returns>Fal AI response</returns>
        public async Task<FalResponse?> SendRequestBitmap(string season, string timeofday, string imageBmp, string imageGuide)
        {
            var apiPath = $"{_url}/{appId}";

            string imageBase64 = null;
            imageBase64 = $"{_imageHeader},{imageBmp}";

            string imageGuide64 = null;
            imageGuide64 = $"{_imageHeader},{imageGuide}";

            var tempPrompt = promt;
            tempPrompt.Replace("SEASON", season);
            tempPrompt.Replace("TIME", timeofday);


            var payload = new
            {
                cliptextencode_text = tempPrompt,
                loadimage_1 = imageBase64 ?? DefaultImagePath,
                loadimage_2 = imageGuide64 ?? DefaultImagePath,
            };

            string jsonPayload = Newtonsoft.Json.JsonConvert.SerializeObject(payload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            try
            {
                // Make the POST request
                HttpResponseMessage response = await _apiClient.PostAsync(apiPath, content);
                response.EnsureSuccessStatusCode(); // Throw if the request failed

                // Get the response content as a string
                string result = await response.Content.ReadAsStringAsync();
                var data = Newtonsoft.Json.JsonConvert.DeserializeObject<FalResponse>(result);

                return data;
            }
            catch (HttpRequestException e)
            {
                RhinoApp.WriteLine($"Request error: {e.Message}");
                return null;
            }
        }


        /// <summary>
        /// Keep polling the FalAI API for resulting image in bitmap with the given ID.
        /// </summary>
        /// <param name="requestId">ID of the request previously sent to Fal AI</param>
        /// <param name="delayMs">Frequency of polling in milliseconds</param>
        /// <param name="maxAttempts">Maximum amount of attempts</param>
        /// <returns>Resulting image in bitmap</returns>
        /// <exception cref="TimeoutException"></exception>
        public async Task<Bitmap> PollForResultBitmap(FalResponse response, bool[] stop, int delayMs = 100, int maxAttempts = 10000)
        {
            var falOutput = await PollForResult(response, stop, delayMs, maxAttempts);
            if (falOutput == null) { return null; }
            var imageUrls = falOutput.GetImageUrls();
            var image = await FetchBitmap(imageUrls[0]);

            return image;
        }

        /// <summary>
        /// Keep polling the FalAI API for the result of the request with the given ID.
        /// </summary>
        /// <param name="requestId">ID of the request previously sent to Fal AI</param>
        /// <param name="delayMs">Frequency of polling in milliseconds</param>
        /// <param name="maxAttempts">Maximum amount of attempts</param>
        /// <returns>Fal AI output</returns>
        /// <exception cref="TimeoutException"></exception>
        public async Task<FalOutput> PollForResult(FalResponse sendResponse, bool[] stop, int delayMs, int maxAttempts)
        {
            var timer = new Stopwatch();
            timer.Start();
            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                if (stop[0])
                {
                    RhinoApp.WriteLine(_name + ": Job is killed...");
                    return null;
                }

                // Wait before the next check
                await Task.Delay(delayMs);

                // Send request to check job status
                var statusResponse = await GetStatus(sendResponse);

                if (statusResponse.status == "IN_QUEUE")
                {
                    RhinoApp.WriteLine(_name + ": Job is in queue... " + timer.Elapsed.TotalMilliseconds + " ms" + " queue:" + statusResponse.queue_position);
                }
                else if (statusResponse.status == "IN_PROGRESS")
                {
                    RhinoApp.WriteLine(_name + ": Job is in progress...");
                }
                else if (statusResponse.status == "COMPLETED")
                {
                    RhinoApp.WriteLine(_name + ": Job is completed...");
                    var response = await GetResponse(sendResponse);
                    var data = Newtonsoft.Json.JsonConvert.DeserializeObject<FalOutput>(response);
                    return data;
                    // return response;
                }
                else
                {
                    RhinoApp.WriteLine(_name + ": Job failed...");
                }
            }

            throw new TimeoutException("The job did not complete in the expected timeframe.");
        }

        public async Task<string> GetResponse(FalResponse sendResponse)
        {
            //var apiPath = $"{_url}/{appId}/requests/{requestId}";

            try
            {
                HttpResponseMessage response = await _apiClient.GetAsync(sendResponse.response_url);
                response.EnsureSuccessStatusCode(); // Throw if the request failed

                string result = await response.Content.ReadAsStringAsync();
                return result;
            }
            catch
            {
                RhinoApp.WriteLine("Failed to get a response from FalAI.");
                return null;
            }
        }

        /// <summary>
        /// Get the status of a request with the given ID.
        /// </summary>
        /// <param name="requestId">ID of the request previously sent to Fal AI</param>
        /// <returns>Fal AI response</returns>
        public async Task<FalResponse> GetStatus(FalResponse sendResponse)
        {
            //var apiPath = $"{_url}/{appId}/requests/{requestId}/status";

            try
            {
                HttpResponseMessage response = await _apiClient.GetAsync(sendResponse.status_url);
                response.EnsureSuccessStatusCode(); // Throw if the request failed

                string result = await response.Content.ReadAsStringAsync();

                var data = Newtonsoft.Json.JsonConvert.DeserializeObject<FalResponse>(result);
                return data;
            }
            catch
            {
                RhinoApp.WriteLine("Failed to get a respfonse from FalAI.");
                return null;
            }
        }

        /// <summary>
        /// Fetch a bitmap from the given URL.
        /// </summary>
        /// <param name="url">URL to image</param>
        /// <returns>Bitmap the image</returns>
        public async Task<Bitmap> FetchBitmap(string url)
        {
            using (var stream = await _imageClient.GetStreamAsync(url))
            {
                return new Bitmap(stream);
            }
        }
    }

    /// <summary>
    /// Represents the response from the FalAI API.
    /// </summary>
    public class FalResponse
    {
        public string status { get; set; }
        public int queue_position { get; set; }
        public string request_id { get; set; }
        public string response_url { get; set; }
        public string status_url { get; set; }
        public string cancel_url { get; set; }
    }

    /// <summary>
    /// Represents the output from the FalAI API.
    /// </summary>
    public class FalOutput
    {
        /// <summary>
        /// `outputs` field from the FalAI API.
        /// The key usually an integer (seems to be 12 all the time)
        /// </summary>
        public Dictionary<string, OutputDetail> outputs { get; set; }
        public Dictionary<string, object> fal_outputs { get; set; }
        public Dictionary<string, object> timings { get; set; }

        /// <summary>
        /// Get the image URLs from the FalAI output.
        /// </summary>
        /// <returns>A list of url strings</returns>
        public List<string> GetImageUrls()
        {
            var urls = new List<string>();
            foreach (var detail in outputs)
            {
                foreach (var image in detail.Value.images)
                {
                    urls.Add(image.url);
                }
            }

            return urls;
        }

        /// <summary>
        /// Represents the detail of the output from the FalAI API.
        /// </summary>
        public class OutputDetail
        {
            public List<Image> images { get; set; }
        }

        /// <summary>
        /// Represents an image returned by the FalAI API.
        /// </summary>
        public class Image
        {
            public string filename { get; set; }
            public string subfolder { get; set; }
            public string type { get; set; }
            public string url { get; set; }
        }
    }
}

