using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Weikio.ApiFramework.Samples.AsyncStream
{
    public class WeatherApi
    {
        private readonly Random _rng = new Random();

        public async IAsyncEnumerable<Weather> GetStream()
        {
            for (var i = 1; i <= 10000; i++)
            {
                var w = new Weather
                {
                    Date = DateTime.Now.AddMinutes(i), TemperatureC = _rng.Next(-20, 55)
                };

                if ((double) i % 1000 == 0)
                {
                    await Task.Delay(TimeSpan.FromSeconds(1));
                }

                yield return w;
            }
        }
    }
}