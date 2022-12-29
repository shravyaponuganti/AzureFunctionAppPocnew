using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iPaas.Transformation.Interfaces
{
    public interface IStorageHelper
    {
        string ConnectionString { get; set; }
        Task<Stream> ReadBlobAsync(string fileName, string containerName);
    }
}
