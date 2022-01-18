using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Weikio.ApiFramework.Abstractions;

namespace HelloWorld
{

    public class HelloWorldCacheApi
    {
        private readonly IEndpointCache _cache;
        private readonly TimeSpan _absoluteExpirationRelativeToNow = TimeSpan.FromSeconds(5);

        public HelloWorldCacheApi(IEndpointCache cache)
        {
            _cache = cache;
        }
        private string GetTestString(string name)
        {
            return $"{name} (function)";
        }

        private async Task<string> GetTestStringAsync(string name)
        {
            return await Task.FromResult($"{name} (async function)");
        }

        public string SetString(string name)
        {
            _cache.SetString("MyKey", name, _absoluteExpirationRelativeToNow);
            var cacheValue = _cache.GetString("MyKey");
            return $"Hello {cacheValue} from cache";
        }

        public string GetString()
        {
            var cacheValue = _cache.GetString("MyKey");
            if (string.IsNullOrEmpty(cacheValue))
            {
                return $"Hello. Value not found from cache";
            }
            return $"Hello {cacheValue} from cache";
        }

        public string CreateString(string name)
        {
            Func<string> func = () => GetTestString(name);
            var cacheValue = _cache.GetOrCreateString("MyKey", _absoluteExpirationRelativeToNow, func);
            return $"Hello {cacheValue} from cache";
        }

        public async Task<string> CreateAsyncronousStringAsync(string name)
        {
            Func<Task<string>> func = () => GetTestStringAsync(name);
            var cacheValue = await _cache.GetOrCreateStringAsync("MyKey", _absoluteExpirationRelativeToNow, func);
            return $"Hello {cacheValue} from cache";
        }

        public string Timeout(string name, int timeInSeconds)
        {
            _cache.SetString("MyKey", name, _absoluteExpirationRelativeToNow);
            Thread.Sleep(timeInSeconds * 1000);
            var cacheValue = _cache.GetString("MyKey");
            if (string.IsNullOrEmpty(cacheValue))
            {
                return $"Hello. You were removed from cache";
            }
            return $"Hello {cacheValue}. You are still in cache";
        }

        public void SetData(string name)
        {
            var obj = Encoding.ASCII.GetBytes(name);
            _cache.SetData("MyKey", obj, _absoluteExpirationRelativeToNow);
            return;
        }

        public string GetData()
        {
            var cacheValue = _cache.GetData("MyKey");
            if (cacheValue == null)
            {
                return $"Hello. Value not found from cache";
            }
            var value = Encoding.ASCII.GetString(cacheValue);
            return $"Hello {value} data from cache";
        }
    }
}
