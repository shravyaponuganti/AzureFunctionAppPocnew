using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using TransformFunction.Models;

namespace TransformFunction.Services
{
    public interface ICSVConverter
    {
        Task<List<string>> GetJsonListAsync(Stream stream, string targetJsonFormat);
        List<string> GetJsonList(Stream stream, string targetJsonFormat);

        Task<string> GetJsonAsync(Stream stream, string targetJsonFormat);
        string GetJson(Stream stream, string targetJsonFormat);
    }
}
