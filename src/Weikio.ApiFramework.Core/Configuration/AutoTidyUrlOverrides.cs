using Weikio.ApiFramework.Abstractions;

namespace Weikio.ApiFramework.Core.Configuration
{
    public class AutoTidyUrlAPIOverrides
    {
        private AutoTidyUrlModeEnum _autoTidyUrls = AutoTidyUrlModeEnum.Automatic;

        public AutoTidyUrlModeEnum AutoTidyUrls
        {
            get
            {
                return _autoTidyUrls;
            }
            set
            {
                _autoTidyUrls = value;
                IsSet = true;
            }
        }

        public bool IsSet { get; private set; } = false;
    }
}
