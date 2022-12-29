using iPaas.Transformation.Interfaces;
using Microsoft.WindowsAzure.Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iPaas.Transformation.Helpers
{
    public class StorageHelper : IStorageHelper
    {
        public string ConnectionString { get => null; set => value = null; }

        public  async Task<Stream> ReadBlobAsync(string fileName, string containerName)
        {
            var storageAccount = CloudStorageAccount.Parse(ConnectionString);
            var serviceClient = storageAccount.CreateCloudBlobClient();
            var container = serviceClient.GetContainerReference(containerName);
            var blob = container.GetBlockBlobReference(fileName);
            using (var memoryStream = new MemoryStream())
            {
                await blob.DownloadToStreamAsync(memoryStream);
                memoryStream.Position = 0;
                return memoryStream;
            }
        }
    }
}
