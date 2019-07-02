using CognitiveServicesTestApp.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Script.Serialization;

namespace CognitiveServicesTestApp.Services
{
    public class CognitiveService
    {
        private HttpClient client;

        private static string KEY = ConfigurationManager.AppSettings["faceapikey"];

        private static string VISIONKEY = ConfigurationManager.AppSettings["visionapikey"];

        private static string BASEURI = ConfigurationManager.AppSettings["baseuri"];

        private static string OCRBASEURI = ConfigurationManager.AppSettings["visionapibaseuri"];

        public CognitiveService()
        {

            client = new HttpClient();

        }

        public async Task<string> overallResultRequest(string licenseId, string selfieId)
        {
            
            // Request headers
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", KEY);

            var uri = BASEURI + "verify";

            HttpResponseMessage response;

            var cogPackage = new CogPackage() { faceId1 = licenseId, faceId2 = selfieId };

            JavaScriptSerializer serializer = new JavaScriptSerializer();


            var package = serializer.Serialize(cogPackage);

            // Request body
            byte[] byteData = Encoding.UTF8.GetBytes(package);

            using (var content = new ByteArrayContent(byteData))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                response = await client.PostAsync(uri, content);
            }

            var contentString = await response.Content.ReadAsStringAsync();

            return contentString;

        }



        public async Task<string> MakeAnalysisRequest(string imageFilePath)
        {

            // Request headers.
            client.DefaultRequestHeaders.Add(
                "Ocp-Apim-Subscription-Key", KEY);

            // Request parameters. A third optional parameter is "details".
            string requestParameters = "returnFaceId=true&returnFaceLandmarks=false";

            // Assemble the URI for the REST API Call.
            var uri = BASEURI + "detect?" + requestParameters;

            HttpResponseMessage response;

            var base64 = imageFilePath.Split(',')[1];

            // Request body. Posts a locally stored JPEG image.
            byte[] byteData = Convert.FromBase64String(base64);

            using (ByteArrayContent content = new ByteArrayContent(byteData))
            {
                // This example uses content type "application/octet-stream".
                // The other content types you can use are "application/json"
                // and "multipart/form-data".
                content.Headers.ContentType =
                    new MediaTypeHeaderValue("application/octet-stream");

                // Execute the REST API call.
                response = await client.PostAsync(uri, content);

                // Get the JSON response.
                var contentString = await response.Content.ReadAsStringAsync();

                var mystr = contentString.Substring(1, contentString.Length - 2);

                dynamic stuff = JsonConvert.DeserializeObject<CogResponse>(mystr);

                return stuff?.faceId;

            }
        }

        public async Task<string> MakeOCRAnalysisRequest(string imageFilePath)
        {

            // Request headers.
            client.DefaultRequestHeaders.Add(
                "Ocp-Apim-Subscription-Key", VISIONKEY);
    

            // Assemble the URI for the REST API Call.
            var uri = OCRBASEURI;

            HttpResponseMessage response;

            var base64 = imageFilePath.Split(',')[1];

            // Request body. Posts a locally stored JPEG image.
            byte[] byteData = Convert.FromBase64String(base64);

            using (ByteArrayContent content = new ByteArrayContent(byteData))
            {
                // This example uses content type "application/octet-stream".
                // The other content types you can use are "application/json"
                // and "multipart/form-data".
                content.Headers.ContentType =
                    new MediaTypeHeaderValue("application/octet-stream");

                // Execute the REST API call.
                response = await client.PostAsync(uri, content);

                // Get the JSON response.
                var contentString = await response.Content.ReadAsStringAsync();

                var result = JsonConvert.DeserializeObject<OCRResponse>(contentString);

                var lastname = result?.regions[2]?.lines[0]?.words[0].text;

                var firstname = result?.regions[2]?.lines[1]?.words[0].text;

                var middlename = result?.regions[2]?.lines[1]?.words[1].text;

                var paragraph = "FullName: " + firstname + " " + middlename + " " + lastname;

                //foreach (var line in regionTwo.lines)
                //{

                //    var fullline = "";

                //    foreach (var word in line?.words)
                //    {

                //        fullline += word.text + " ";

                //    }

                //    paragraph += fullline + Environment.NewLine;

                //}



                client.DefaultRequestHeaders.Clear();

                return paragraph;
            }
        }

    }// end of class
}// end of namespace