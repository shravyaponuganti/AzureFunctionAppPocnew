using System;
using System.Threading.Tasks;
using iPaaS.Transformation;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Data.SqlClient;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using iPaas.Transformation.Models;
using SqlConnection = System.Data.SqlClient.SqlConnection;
using Azure.Security.KeyVault.Secrets;
using Azure.Identity;
using iPaas.Transformation.Interfaces;

namespace iPaas.Transformation.Function
{
    public class FunReadPayloadSentToAEAPI
    {
        private readonly ITelemetryHelper telemetryHelper;
        private readonly ITransformAEAsset transformAEAsset;
        public FunReadPayloadSentToAEAPI(ITelemetryHelper telemetryHelper, ITransformAEAsset transformAEAsset)
        {
            this.telemetryHelper = telemetryHelper;
            this.transformAEAsset = transformAEAsset;
        }

        [FunctionName("FunReadPayloadSentToAEAPI")]
        public async Task Run([ServiceBusTrigger("az-jsonqueue", Connection = "ConnectionQueue")] string myQueueItem, ILogger log)
        {
            try
            {
                log.LogInformation($"Api Service started, ServiceBus queue trigger function processed message: {myQueueItem}");
                ApiInfo apiInfo = GetApiRequiredInfo(log);
                var requestMessage = new HttpRequestMessage(HttpMethod.Post, apiInfo.APIUrl);
                requestMessage.Headers.Add("environment_id", apiInfo.EnvironmentId);
                string TokenUrl = apiInfo.TokenUrl + apiInfo.EnvironmentId;
                dynamic tokenObj = JsonConvert.DeserializeObject<object>(GetToken(TokenUrl, log).Result);
                string accessToken = tokenObj["access_token"];
                string tokenType = tokenObj["token_type"];
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue(tokenType, accessToken);
                requestMessage.Headers.Add("X-Core-Target-Type", "stage");
                requestMessage.Content = new StringContent(myQueueItem, Encoding.UTF8, "application/json");
                string responseBody;
                using (var apiClient = new HttpClient())
                {
                    HttpResponseMessage apiResponse = await apiClient.SendAsync(requestMessage);
                    responseBody = await apiResponse.Content.ReadAsStringAsync();
                }
                log.LogInformation($"C# ServiceBus queue trigger function processed message: {responseBody}");
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message + ex.StackTrace);
            }
            finally
            {
                log.LogInformation($"Api Service ended");
            }
        }

        private ApiInfo GetApiRequiredInfo(ILogger log)
        {
            ApiInfo apiInfo = new ApiInfo();
            try
            {
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
                string SQLConnectionString = System.Environment.GetEnvironmentVariable("SQLConnectionString");
                using (SqlConnection connection = new SqlConnection(SQLConnectionString))
                {
                    connection.Open();
                    string sql = @"SELECT [EnvironmentId],[TokenUrl],[APIUrl] FROM [dbo].[ApiConfig] WHERE IsActive = 1";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        using (SqlDataReader oReader = command.ExecuteReader())
                        {
                            while (oReader.Read())
                            {
                                apiInfo.EnvironmentId = oReader["EnvironmentId"].ToString();
                                apiInfo.APIUrl = oReader["APIUrl"].ToString();
                                apiInfo.TokenUrl = oReader["TokenUrl"].ToString();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message + ex.StackTrace);
                return apiInfo;
            }
            return apiInfo;
        }

        public async Task<string> GetToken(string TokenUrl, ILogger log)
        {
            var secretClient = new SecretClient(new Uri(Environment.GetEnvironmentVariable("KeyVaultUri")), new DefaultAzureCredential());
            KeyVaultSecret secretToken = secretClient.GetSecret("AuthorizationToken");
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, TokenUrl);
            httpRequestMessage.Headers.Add("data-raw", "");
            httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Basic", secretToken.Value);

            string responseString = string.Empty;
            using (var client = new HttpClient())
            {
                HttpResponseMessage response = client.SendAsync(httpRequestMessage).Result;
                responseString = await response.Content.ReadAsStringAsync();
            }
            return responseString;
        }
    }
}
