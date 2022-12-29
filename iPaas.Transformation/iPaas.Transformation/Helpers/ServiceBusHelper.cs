using Azure.Messaging.ServiceBus;
using iPaas.Transformation.Interfaces;
using iPaaS.Transformation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iPaas.Transformation.Helpers
{
    public class ServiceBusHelper : IServiceBusHelper
    {

        private ISqlConnection sqlConnection;
        private IStorageHelper storageConnection;
        private ITelemetryHelper telemetryHelper;

        public ServiceBusHelper(ISqlConnection sqlConnection, IStorageHelper storageConnection, ITelemetryHelper telemetryHelper)
        {
            this.sqlConnection = sqlConnection;
            this.storageConnection = storageConnection;
            this.telemetryHelper = telemetryHelper;
        }

        public async Task<bool> SendMessageAsync(string queueMessage, string queueName, string topicName = null)
        {
            telemetryHelper.LogTrace("Start:Sending Message");
            await using var client = new ServiceBusClient(storageConnection.ConnectionString);
            ServiceBusSender sender = client.CreateSender(queueName);
            ServiceBusMessage message = new ServiceBusMessage(queueMessage);
            await sender.SendMessageAsync(message);
            telemetryHelper.LogTrace("End:Sending Message");
            return true;
        }

        public async Task<string> ReadMessageAsync(string queueName, string topicName = null)
        {
            telemetryHelper.LogTrace("Start:Reading Message");
            await using var client = new ServiceBusClient(storageConnection.ConnectionString);
            ServiceBusReceiver receiver = client.CreateReceiver(queueName);
            ServiceBusReceivedMessage receivedMessage = await receiver.ReceiveMessageAsync();
            string body = receivedMessage.Body.ToString();
            telemetryHelper.LogTrace("End:Reading Message");
            return body;
        }
    }
}
