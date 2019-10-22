using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Weikio.ApiFramework.Plugins.OldLogger
{
    public class OldLoggingFunction
    {
        private readonly ILogger<OldLoggingFunction> _logger;

        public OldLoggingFunction(ILogger<OldLoggingFunction> logger)
        {
            _logger = logger;
        }

        public string Log()
        {
            _logger.LogInformation("Running new logger");

            var assembly = _logger.GetType().Assembly;
            var location = assembly.Location;

            var versionInfo = FileVersionInfo.GetVersionInfo(location);

            return location + " " + versionInfo.ToString();
        }
    }
}
