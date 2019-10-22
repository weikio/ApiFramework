using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Weikio.ApiFramework.Plugins.HealthCheck
{
    public class SometimesWorkingApi
    {
        public bool IsWorking()
        {
            var isBroken = IsBroken();

            if (isBroken)
            {
                throw new Exception("Currently broken");
            }

            return true;
        }

        public static bool IsBroken()
        {
            var currentMinute = DateTime.Now.Minute.ToString();

            var brokenMinutes = new char[] { '1', '2', '4', '5', '6', '7' };

            var lastMinute = currentMinute.Last();

            if (brokenMinutes.Contains(lastMinute))
            {
                return true;
            }

            return false;
        }
    }
}
