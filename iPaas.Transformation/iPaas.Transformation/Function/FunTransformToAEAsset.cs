using System;
using iPaas.Transformation.Interfaces;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace iPaaS.Transformation
{
    public class FunTransformToAEAsset
    {
        private readonly ITelemetryHelper telemetryHelper;
        private readonly ITransformAEAsset transformAEAsset;
        public FunTransformToAEAsset(ITelemetryHelper telemetryHelper, ITransformAEAsset transformAEAsset)
        {
            this.telemetryHelper = telemetryHelper;
            this.transformAEAsset = transformAEAsset;
        }

        [FunctionName("FunReadAEAssetQueue")]
        public void Run([ServiceBusTrigger("az-queue", Connection = "ConnectionQueue")]string myQueueItem, ILogger log)
        {
            var corealtionId = Guid.NewGuid().ToString();
            log.LogInformation($"C# ServiceBus queue trigger function start processing message: {myQueueItem} and ColerationId : {corealtionId}");

            this.transformAEAsset.Transform(myQueueItem, corealtionId);

            log.LogInformation($"C# ServiceBus queue trigger function processed message: {myQueueItem} and ColerationId : {corealtionId}");
        }
    }
}
