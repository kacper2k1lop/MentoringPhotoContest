using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using FunctionApp.Consts;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;


namespace FunctionApp
{
    public static class TaggingPhoto
    {
        [Function("TaggingPhoto")]
        public static void Run([QueueTrigger("queue", Connection = ConfigurationKeys.BlobConnectionString)] string myQueueItem,
            FunctionContext context)
        {
            var logger = context.GetLogger("TaggingPhoto");
            logger.LogInformation($"C# Queue trigger function processed: {myQueueItem}");
            string subscriptionKey = Environment.GetEnvironmentVariable("COMPUTER_VISION_SUBSCRIPTION_KEY");
            string endpoint = Environment.GetEnvironmentVariable("COMPUTER_VISION_ENDPOINT");
            string connectionString = Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING");

            string containerName = "container";//I can make it env variable, like appsetting

            BlobContainerClient containerClient = new(
                connectionString,
                containerName);
            BlobClient blobClient = containerClient.GetBlobClient(myQueueItem);
            string remoteImageUrl = GetServiceSasUriForBlob(blobClient).ToString();

            var serviceClient = new TableServiceClient(connectionString);
            TableClient table = serviceClient.GetTableClient("table");

            try
            {
                Task<string> tag = AnalyzeImageSample.AnalyzeImageSample.RunAsync(endpoint, subscriptionKey, remoteImageUrl);
                tag.Wait(5000);
                Thanks.Thanks.UpdateEntity(table, myQueueItem, tag.Result);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            Console.WriteLine("\nPress ENTER to exit.");
            Console.ReadLine();
        }
        private static Uri GetServiceSasUriForBlob(BlobClient blobClient)
        {
            // Check whether this BlobClient object has been authorized with Shared Key.
            if (blobClient.CanGenerateSasUri)
            {
                // Create a SAS token that's valid for one hour.
                BlobSasBuilder sasBuilder = new(
                    BlobSasPermissions.Read,
                    DateTimeOffset.UtcNow.AddHours(1));
                Uri sasUri = blobClient.GenerateSasUri(sasBuilder);
                return sasUri;
            }
            else
            {
                Console.WriteLine(@"BlobClient must be authorized with Shared Key 
                          credentials to create a service SAS.");
                return null;
            }
        }
    }

}
