using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iPaas.Transformation.Interfaces
{
    public interface IServiceBusHelper
    {
        Task<bool> SendMessageAsync(string queueMessage, string queueName, string topicName = null);
        Task<string> ReadMessageAsync(string queueName, string topicName = null);
    }
}
