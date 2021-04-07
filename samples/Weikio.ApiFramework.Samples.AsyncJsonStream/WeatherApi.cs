using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Weikio.ApiFramework.Samples.AsyncJsonStream
{
    public class WeatherApi
    {
        private readonly Random _rng = new Random();

        public WeatherConfiguration Configuration { get; set; }

#pragma warning disable 1998
        public async IAsyncEnumerable<Weather> GetStream()
#pragma warning restore 1998
        {
            for (var i = 1; i <= 100000; i++)
            {
                var w = new Weather { Date = DateTime.Now.AddMinutes(i), TemperatureC = _rng.Next(-20, 55) };

                yield return w;
            }
        }
    }

    public class WeatherConfiguration
    {
        public TimeSpan Delay { get; set; } = TimeSpan.FromMilliseconds(300);
    }
}
