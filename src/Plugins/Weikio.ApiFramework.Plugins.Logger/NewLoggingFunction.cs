using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Weikio.ApiFramework.Plugins.Logger
{
    public class NewLoggingFunction
    {
        private readonly ILogger<NewLoggingFunction> _logger;

        public NewLoggingFunction(ILogger<NewLoggingFunction> logger)
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
