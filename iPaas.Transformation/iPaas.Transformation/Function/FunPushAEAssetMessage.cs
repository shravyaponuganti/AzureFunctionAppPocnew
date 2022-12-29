using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using iPaas.Transformation.Models;
using Azure.Storage.Blobs;
using System.Web;
using System.Text.RegularExpressions;
using iPaas.Transformation.Interfaces;
using System.Data.SqlClient;

namespace iPaas.Transformation.Function
{
    public class FunPushAEAssetMessage
    {
        private readonly ISqlConnection sqlConnection;
        private readonly IStorageHelper storageConnection;
        private readonly IServiceBusHelper serviceBusHelper;
        private readonly ITelemetryHelper telemetryHelper;

        public FunPushAEAssetMessage(ISqlConnection sqlConnection, IStorageHelper storageConnection, IServiceBusHelper serviceBusHelper, ITelemetryHelper telemetryHelper)
        {
            this.sqlConnection = sqlConnection;
            this.storageConnection = storageConnection;
            this.serviceBusHelper = serviceBusHelper;
            this.telemetryHelper = telemetryHelper;
        }

        [FunctionName("FunPushAEAssetMessage")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req, ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var fMetaData = JsonConvert.DeserializeObject<ServiceBusQueueInfo>(requestBody);
            BlobClient account = new BlobClient(fMetaData.StorageAccountConnectionString, fMetaData.BlobContainerName, fMetaData.FileName);
            var bMetaData = new FileMetadata();
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
            await serviceBusHelper.SendMessageAsync(Json, fMetaData.QueueName);
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
            catch (Exception ex)
            {
                log.LogError(ex.Message + ex.StackTrace);
                return jsonTemplateString;
            }
            return jsonTemplateString;
        }
    }
}
