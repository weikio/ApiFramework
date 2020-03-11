using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Weikio.ApiFramework.Samples.DependencyConflict
{
    public class HostLoggerApi
    {
        private readonly ILogger<HostLoggerApi> _logger;

        public HostLoggerApi(ILogger<HostLoggerApi> logger)
        {
            _logger = logger;
        }

        public string Log()
        {
            _logger.LogInformation("Running host logger");

            var assembly = _logger.GetType().Assembly;
            var location = assembly.Location;

            var versionInfo = FileVersionInfo.GetVersionInfo(location);
            return location + " " + versionInfo.ToString();
        }
    }
}
