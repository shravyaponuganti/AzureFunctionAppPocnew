using iPaas.Transformation.Interfaces;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iPaas.Transformation.Helpers
{
    /// <summary>
    /// TelemetryHelper
    /// Ref: https://learn.microsoft.com/en-us/azure/azure-monitor/app/sdk-connection-string?tabs=net#set-a-connection-string
    /// </summary>
    public class TelemetryHelper : ITelemetryHelper
    {
        private readonly TelemetryClient telemetryClient;
        private readonly ILogger logger;

        public TelemetryHelper(ILogger logger)
        {
            this.logger = logger;
            var configuration = new TelemetryConfiguration
            {
                //TODO: Read from Config
                ConnectionString = "InstrumentationKey=00000000-0000-0000-0000-000000000000;"
            };
            telemetryClient = new TelemetryClient(configuration);
        }

        public void LogError(string message)
        {
            logger.LogError(message);
            //TODO: Need to modify the messagein a generic way
            telemetryClient.TrackEvent(message);

        }

        public void LogEvent(string message)
        {
            logger.LogInformation(message);
            telemetryClient.TrackEvent(message);
        }

        public void LogTrace(string message)
        {
            logger.LogTrace(message);
            telemetryClient.TrackTrace(message);
        }
    }
}
