using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
using Microsoft.WindowsAzure.Storage;
using Newtonsoft.Json;
using TransformFunction.Models;
using TransformFunction.Services;

namespace TransformFunction
{
    public class CSVtoJSONConverter
    {
        private readonly ICSVConverter csvConverter;
        public CSVtoJSONConverter(ICSVConverter csvConverter)
        {
            this.csvConverter = csvConverter;
        }

        [FunctionName("CSVtoJSONConverter")]
        //[return: ServiceBus("az-jsonqueue", Connection = "ConnectionQueue")]
        public async Task Run([ServiceBusTrigger("az-queue", Connection = "ConnectionQueue")] ServiceBusMessageDetails myQueueItem, [ServiceBus(queueOrTopicName: "az-jsonqueue", Connection = "ConnectionQueue")] IAsyncCollector<string> dispatcher, ILogger log)
        {
            log.LogInformation($"CSVtoJSONConverter function execution started at {DateTime.Now}");
            try
            {
                var jsonPayloadData = new List<string>();
                log.LogInformation($"Input service bus message : {JsonConvert.SerializeObject(myQueueItem)}");
                jsonPayloadData = await ConverttoJsonAsync(myQueueItem, log);
                log.LogInformation($"CSV Record(s) count is : {jsonPayloadData?.Count ?? 0}");
                if (jsonPayloadData != null && jsonPayloadData.Any())
                {
                    foreach (var jsonObj in jsonPayloadData)
                    {
                        await dispatcher.AddAsync(jsonObj);
                        log.LogInformation($"Converted payload is : {jsonObj}");
                    }
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message + ex.StackTrace);
            }
            finally
            {
                log.LogInformation($"CSVtoJSONConverter function execution ended at {DateTime.Now}");
            }
        }

        public async Task<List<string>> ConverttoJsonAsync(ServiceBusMessageDetails message, ILogger log)
        {
            try
            {
                log.LogInformation($"Entered into method ConverttoJsonAsync");
                if (message is null)
                {
                    log.LogInformation($"ServiceBusMessageDetails : Message is empty or null");
                    return null;
                }

                var storageAccount = CloudStorageAccount.Parse(message.StorageAccountConnectionString);
                var serviceClient = storageAccount.CreateCloudBlobClient();
                var container = serviceClient.GetContainerReference(message.BlobContainerName);
                var blob = container.GetBlockBlobReference(message.FileName);
                using (var memoryStream = new MemoryStream())
                {
                    await blob.DownloadToStreamAsync(memoryStream);
                    memoryStream.Position = 0;
                    return await csvConverter.GetJsonListAsync(memoryStream, message.ConfigJson);
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message + ex.StackTrace);
                return null;
            }
            finally
            {
                log.LogInformation($"Exit from method ConverttoJsonAsync");
            }
        }

    }
}
