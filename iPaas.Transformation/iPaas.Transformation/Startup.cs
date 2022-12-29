using iPaas.Transformation.Helpers;
using iPaas.Transformation.Interfaces;
using iPaaS.Transformation.Transformers;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//[assembly: FunctionsStartup(typeof(iPaaS.Transformation.Startup))]
namespace iPaaS.Transformation
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            //ISqlConnection sqlConnection = new SqlConnection();
            //IStorageConnection storageConnection = new StorageConnection();
            //ITelemetryHelper telemetryHelper = new TelemetryHelper();
            //IServiceBusHelper serviceBusHelper = new ServiceBusHelper(sqlConnection, storageConnection, telemetryHelper);
            builder.Services.TryAddSingleton<ISqlConnection, SqlConnection>();
            builder.Services.TryAddSingleton<IStorageHelper, StorageHelper>();
            builder.Services.TryAddScoped<ITelemetryHelper, TelemetryHelper>();
            builder.Services.TryAddSingleton<IServiceBusHelper, ServiceBusHelper>();
            builder.Services.TryAddSingleton<ITransformAEAsset, TransformAEAsset>();
        }
    }
}
