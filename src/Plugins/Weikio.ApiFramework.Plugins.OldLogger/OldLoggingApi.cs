using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Weikio.ApiFramework.Plugins.OldLogger
{
    public class OldLoggingApi
    {
        private readonly ILogger<OldLoggingApi> _logger;

        public OldLoggingApi(ILogger<OldLoggingApi> logger)
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
