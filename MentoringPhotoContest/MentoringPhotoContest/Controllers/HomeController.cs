using Azure;
using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Azure.Storage.Queues;
using MentoringPhotoContest.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using MentoringPhotoContest.Thanks;
using MentoringPhotoContest.Infrastructure.Configuration;

namespace MentoringPhotoContest.Controllers
{


    public class HomeController : Controller
    {
        //reshareper
        private readonly ILogger<HomeController> _logger;
        private readonly Configuration _config;

        public HomeController(ILogger<HomeController> logger, Configuration configuration)
        {
            _logger = logger;
            _config = configuration;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UploadFile(IFormFile image)
        {
            string filePath = Path.GetRandomFileName();
            string connectionString = Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING");
            string containerName = Environment.GetEnvironmentVariable("AZURE_STORAGE_CONTAINER");
            //zrobic klase do pobierania wartosci ze srodowisk

            Thanks.Thanks message = new(
                "rowKey",
                DateTime.Now);
            string blobName = message.PartitionKey.ToString();
            string queueName = "queue";


            if (image.Length > 0)
            {
                using (var stream = System.IO.File.Create(filePath))
                {
                    //await  - wtf, its async method
                    await image.CopyToAsync(stream);
                }
                BlobContainerClient containerClient = new(connectionString,
                                                                           containerName);
                BlobClient blobClient = containerClient.GetBlobClient(blobName);
                Console.WriteLine("Uploading File");
                //await
                blobClient.UploadAsync(filePath, true).Wait(5000);
                // I may only start task when I am calling task constructor,
                // in this case class task already started the task
                //task.Start();
                var serviceClient = new TableServiceClient(connectionString);
                TableClient table = serviceClient.GetTableClient("table");

                table.CreateIfNotExists();
                Thanks.Thanks.CreateMessage(table, message);

                // Create the queue service client
                QueueClient queueClient = new(connectionString, queueName, new QueueClientOptions
                {
                    // this is required because otherwise exception that no base64 during taking message from queue
                    //System.Private.CoreLib: Exception while executing function: Functions.Function1. Microsoft.Azure.WebJobs.Host: Exception binding parameter 'myQueueItem'. System.Private.CoreLib: The input is not a valid Base-64 string as it contains a non-base 64 character, more than two padding characters, or an illegal character among the padding characters.
                    MessageEncoding = QueueMessageEncoding.Base64
                });
                if (queueClient.Exists())
                {
                    // Send a message to the queue
                    await queueClient.SendMessageAsync(blobName);
                    Console.WriteLine($"Inserted: {blobName}");
                }
                else
                {
                    Console.WriteLine($"Not inserted: {blobName}");
                }
            }
            return new OkResult();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
