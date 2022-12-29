using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iPaas.Transformation.Models
{
    public class FileMetadata
    {
        public string FileName { get; set; }
        public string AccountName { get; set; }
        public string BlobContainerName { get; set; }
        public string AbsoluteUri { get; set; }
        public string LocalPath { get; set; }
        public string Host { get; set; }
        public string StorageAccountConnectionString { get; set; }
        public string ConfigJson { get; set; }
    }
}
