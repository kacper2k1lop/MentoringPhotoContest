using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;

namespace FunctionApp.AnalyzeImageSample
{
    public static class AnalyzeImageSample
    {
        public static async Task<string> RunAsync(string endpoint, string key, string remoteImageUrl)
        {
            ComputerVisionClient computerVision = new(new ApiKeyServiceClientCredentials(key))
            {
                Endpoint = endpoint
            };


            Console.WriteLine("Images being analyzed ...");
            return await AnalyzeFromUrlAsync(computerVision, remoteImageUrl);
        }

        // Analyze a remote image
        private static async Task<string> AnalyzeFromUrlAsync(ComputerVisionClient computerVision, string imageUrl)
        {
            if (!Uri.IsWellFormedUriString(imageUrl, UriKind.Absolute))
            {
                Console.WriteLine("\nInvalid remote image url:\n{0} \n", imageUrl);
                return null;
            }

            ImageAnalysis analysis = await computerVision.AnalyzeImageAsync(imageUrl, visualFeatures: new List<VisualFeatureTypes?>() { VisualFeatureTypes.Tags });
            return DisplayTagResults(analysis, imageUrl);
        }

        // Analyze a local image

        private static string DisplayTagResults(ImageAnalysis analysis, string imagePath)
        {
            Console.WriteLine(imagePath);
            //image tags
            string tagedPhoto = null;
            foreach (var tag in analysis.Tags)
            {
                tagedPhoto += tag.Name + ":" + tag.Confidence + ";";
            }
            return tagedPhoto;
        }
    }
}
