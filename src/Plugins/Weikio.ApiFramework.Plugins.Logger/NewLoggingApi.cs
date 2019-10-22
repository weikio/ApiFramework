using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Weikio.ApiFramework.Plugins.Logger
{
    public class NewLoggingApi
    {
        private readonly ILogger<NewLoggingApi> _logger;

        public NewLoggingApi(ILogger<NewLoggingApi> logger)
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
