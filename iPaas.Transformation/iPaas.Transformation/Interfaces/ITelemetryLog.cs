using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iPaas.Transformation.Interfaces
{
    public interface ITelemetryHelper
    {
        void LogTrace(string message);
        void LogEvent(string message);
        void LogError(string message);

    }
}
