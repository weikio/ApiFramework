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

        private object GetTestObject(string name)
        {
            var testObject = new TestObject()
            {
                Name = name
            };
            return testObject;
        }

        public string SetString(string name)
        {
            _cache.SetString("MyKey", name);
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
            var cacheValue = _cache.GetOrCreateString("MyKey", func);
            return $"Hello {cacheValue} from cache";
        }

        public async Task<string> CreateAsyncronousStringAsync(string name)
        {
            Func<Task<string>> func = () => GetTestStringAsync(name);
            var cacheValue = await _cache.GetOrCreateStringAsync("MyKey", func);
            return $"Hello {cacheValue} from cache";
        }

        public string Timeout(string name, int timeInSeconds)
        {
            _cache.SetString("MyKey", name);
            Thread.Sleep(timeInSeconds * 1000);
            var cacheValue = _cache.GetString("MyKey");
            if (string.IsNullOrEmpty(cacheValue))
            {
                return $"Hello. You were removed from cache";
            }
            return $"Hello {cacheValue}. You are still in cache";
        }

        public void SetObject(string name)
        {
            var obj = Encoding.ASCII.GetBytes(name);
            _cache.SetObject("MyKey", obj);
            return;
        }

        public string GetObject()
        {
            var cacheValue = _cache.GetObject("MyKey");
            if (cacheValue == null)
            {
                return $"Hello. Value not found from cache";
            }
            var obj = Encoding.ASCII.GetString(cacheValue);
            return $"Hello {obj} object from cache";
        }
    }

    internal class TestObject
    {
        public string Name { get; set; }
    }
}
