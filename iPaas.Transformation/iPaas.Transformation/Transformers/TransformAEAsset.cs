using iPaas.Transformation.Common;
using iPaas.Transformation.Interfaces;
using iPaas.Transformation.Models;
using Microsoft.WindowsAzure.Storage;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iPaaS.Transformation.Transformers
{

    public class TransformAEAsset : ITransformAEAsset
    {
        private readonly ISqlConnection sqlConnection;
        private readonly IStorageHelper storageHelper;
        private readonly IServiceBusHelper serviceBusHelper;
        private readonly ITelemetryHelper telemetryHelper;

        public TransformAEAsset(ISqlConnection sqlConnection, IStorageHelper storageHelper, IServiceBusHelper serviceBusHelper, ITelemetryHelper telemetryHelper)
        {
            this.sqlConnection = sqlConnection;
            this.storageHelper = storageHelper;
            this.serviceBusHelper = serviceBusHelper;
            this.telemetryHelper = telemetryHelper;
        }

        public async Task<bool> Transform(string myQueueItem, string corealtionId)
        {
            try
            {
                telemetryHelper.LogTrace($"Entered into method ConverttoJsonAsync");
                var message = JsonConvert.DeserializeObject<FileMetadata>(myQueueItem);
                if (message is null)
                {
                    telemetryHelper.LogTrace($"FileMetadata : Message is empty or null");
                    return false;
                }

                //var storageAccount = CloudStorageAccount.Parse(message.StorageAccountConnectionString);
                //var serviceClient = storageAccount.CreateCloudBlobClient();
                //var container = serviceClient.GetContainerReference(message.BlobContainerName);
                //var blob = container.GetBlockBlobReference(message.FileName);
                //using (var memoryStream = new MemoryStream())
                //using()
                //{
                var memoryStream = await storageHelper.ReadBlobAsync(message.FileName, message.BlobContainerName);
                memoryStream.Position = 0;
                var jsonObjectList = await CsvJsonConverter.GetJsonListAsync(memoryStream, message.ConfigJson);
                foreach (var jObject in jsonObjectList)
                {
                    await serviceBusHelper.SendMessageAsync(jObject, "az-queue");
                }
                //}
                return true;
            }
            catch (Exception ex)
            {
                telemetryHelper.LogError(ex.Message + ex.StackTrace);
                return false;
            }
            finally
            {
                telemetryHelper.LogTrace($"Exit from method ConverttoJsonAsync");
            }
        }
    }
}
