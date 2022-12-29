using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using System.Web;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace SendASBMessge
{
    public static class ServicebusMessage
    {
        //private static readonly string SQLConnectionString = "Server = tcp:sql-ipaas-sbx-euno-01.database.windows.net,1433;Initial Catalog = sqldb - ipaas; Persist Security Info=False;User ID = sqladmin; Password=password!@#456;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
        [FunctionName("ServicebusMessage")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");        

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            FileMetaData fMetaData = new FileMetaData();
            fMetaData = JsonConvert.DeserializeObject<FileMetaData>(requestBody);
            BlobClient account = new BlobClient(fMetaData.StorageAccountConnectionString, fMetaData.BlobContainerName, fMetaData.FileName);
            BlobMetaData bMetaData = new BlobMetaData();

            bMetaData.FileName = HttpUtility.UrlDecode(account.Name);
            bMetaData.AccountName = HttpUtility.UrlDecode(account.AccountName);
            bMetaData.AbsoluteUri = HttpUtility.UrlDecode(account.Uri.AbsoluteUri);
            bMetaData.BlobContainerName = HttpUtility.UrlDecode(account.BlobContainerName);
            bMetaData.Host = HttpUtility.UrlDecode(account.Uri.Host);
            bMetaData.LocalPath = HttpUtility.UrlDecode(account.Uri.LocalPath);
            log.LogInformation(fMetaData.ConnectionSQL);
            bMetaData.ConfigJson = Regex.Unescape(GetJsonTemplate(fMetaData.ConnectionSQL, log));
            bMetaData.StorageAccountConnectionString = fMetaData.StorageAccountConnectionString;
            var Json = JsonConvert.SerializeObject(bMetaData, Formatting.Indented);

            await using var client = new ServiceBusClient(fMetaData.ServiceBusConnectionString);
            ServiceBusSender sender = client.CreateSender(fMetaData.QueueName);

            ServiceBusMessage message = new ServiceBusMessage(Json);
            await sender.SendMessageAsync(message);

            var responseMessage = string.IsNullOrEmpty(requestBody)
                ? "Please pass a Json related to Csv file in the request body for a personalized response."
                : $"This " + Json + " HTTP triggered function executed successfully.";

            return new OkObjectResult(responseMessage);
        }
        private static string GetJsonTemplate(string SQLConnectionString, ILogger log)
        {
            string jsonTemplateString = string.Empty; try
            {
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
                SQLConnectionString = Environment.GetEnvironmentVariable("SQLConnectionString");

                using (SqlConnection connection = new SqlConnection(SQLConnectionString))
                {
                    connection.Open(); 
                    string sql = @"SELECT [ConfigurationJSON] FROM [dbo].[MappingConfiguration] WHERE IsActive = 1";

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        jsonTemplateString = (string)command.ExecuteScalar();
                    }
                }
            }
            catch(Exception ex) 
            {
                log.LogError(ex.Message + ex.StackTrace);
                return jsonTemplateString; 
            }
            return jsonTemplateString;
        }
    }
}
