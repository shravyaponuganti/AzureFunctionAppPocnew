using System;
using System.Collections.Generic;
using System.Text;

namespace SendASBMessge
{
    public class FileMetaData
    {
        public string ServiceBusConnectionString
        {
            get;
            set;
        }
        public string QueueName
        {
            get;
            set;
        }
        public string FileName
        {
            get;
            set;
        }
        public string BlobContainerName
        {
            get;
            set;
        }
        public string StorageAccountConnectionString
        {
            get;
            set;
        }
        public string ConnectionSQL
        {
            get;
            set;
        }
    }
}
