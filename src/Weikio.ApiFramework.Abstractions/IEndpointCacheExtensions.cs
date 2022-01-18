using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Weikio.ApiFramework.Abstractions
{
    public static class IEndpointCacheExtensions
    {
        public static void SetData(this IEndpointCache cache, string key, byte[] value, TimeSpan absoluteExpirationRelativeToNow)
        {
            cache.Set(key, value, new ApiCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow
            });
        }

        public static async Task SetDataAsync(this IEndpointCache cache, string key, byte[] value, TimeSpan absoluteExpirationRelativeToNow, CancellationToken token = default)
        {
            await cache.SetAsync(key, value, new ApiCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow
            }, token);
        }

        public static byte[] GetOrCreateData(this IEndpointCache cache, string key, ApiCacheEntryOptions options, Func<byte[]> getValue)
        {
            var data = cache.Get(key);
            if (data == null)
            {
                data = getValue();
                cache.Set(key, data, options);
            }

            return data;
        }

        public static byte[] GetOrCreateData(this IEndpointCache cache, string key, TimeSpan absoluteExpirationRelativeToNow, Func<byte[]> getValue)
        {
            var options = new ApiCacheEntryOptions() { AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow };
            return cache.GetOrCreateData(key, options, getValue);    
        }

        public static async Task<byte[]> GetOrCreateDataAsync(this IEndpointCache cache, string key, ApiCacheEntryOptions options, Func<Task<byte[]>> getValue, CancellationToken token = default)
        {
            var data = await cache.GetAsync(key, token);
            if (data == null)
            {
                data = await getValue();
                await cache.SetAsync(key, data, options, token);
            }

            return data;
        }

        public static async Task<byte[]> GetOrCreateDataAsync(this IEndpointCache cache, string key, TimeSpan absoluteExpirationRelativeToNow, Func<Task<byte[]>> getValue, CancellationToken token = default)
        {
            var options = new ApiCacheEntryOptions() { AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow };
            return await cache.GetOrCreateDataAsync(key, options, getValue, token);
        }

        public static string GetString(this IEndpointCache cache, string key)
        {
            var data = cache.Get(key);
            if (data == null)
            {
                return null;
            }

            return Encoding.UTF8.GetString(data, 0, data.Length);
        }

        public static async Task<string> GetStringAsync(this IEndpointCache cache, string key, CancellationToken token = default)
        {
            var data = await cache.GetAsync(key, token);
            if (data == null)
            {
                return null;
            }

            return Encoding.UTF8.GetString(data, 0, data.Length);
        }

        public static void SetString(this IEndpointCache cache, string key, string value, ApiCacheEntryOptions options)
        {
            cache.Set(key, Encoding.UTF8.GetBytes(value), options);
        }

        public static void SetString(this IEndpointCache cache, string key, string value, TimeSpan absoluteExpirationRelativeToNow)
        {
            cache.SetData(key, Encoding.UTF8.GetBytes(value), absoluteExpirationRelativeToNow);
        }

        public static async Task SetStringAsync(this IEndpointCache cache, string key, string value, ApiCacheEntryOptions options, CancellationToken token = default)
        {
            await cache.SetAsync(key, Encoding.UTF8.GetBytes(value), options, token);
        }

        public static async Task SetStringAsync(this IEndpointCache cache, string key, string value, TimeSpan absoluteExpirationRelativeToNow, CancellationToken token = default)
        {
            await cache.SetDataAsync(key, Encoding.UTF8.GetBytes(value), absoluteExpirationRelativeToNow, token);
        }

        public static string GetOrCreateString(this IEndpointCache cache, string key, ApiCacheEntryOptions options, Func<string> getValue)
        {
            var data = cache.GetOrCreateData(key, options, () =>
            {
                return Encoding.UTF8.GetBytes(getValue());
            });

            return Encoding.UTF8.GetString(data, 0, data.Length);
        }

        public static string GetOrCreateString(this IEndpointCache cache, string key, TimeSpan absoluteExpirationRelativeToNow, Func<string> getValue)
        {
            var options = new ApiCacheEntryOptions() { AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow };
            return cache.GetOrCreateString(key, options, getValue);
        }

        public static async Task<string> GetOrCreateStringAsync(this IEndpointCache cache, string key, ApiCacheEntryOptions options, Func<Task<string>> getValue, CancellationToken token = default)
        {
            var data = await cache.GetOrCreateDataAsync(key, options, async () =>
            {
                return Encoding.UTF8.GetBytes(await getValue());
            }, token);

            return Encoding.UTF8.GetString(data, 0, data.Length);
        }

        public static async Task<string> GetOrCreateStringAsync(this IEndpointCache cache, string key, TimeSpan absoluteExpirationRelativeToNow, Func<Task<string>> getValue, CancellationToken token = default)
        {
            var options = new ApiCacheEntryOptions() { AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow };
            return await cache.GetOrCreateStringAsync(key, options, getValue, token);
        }
    }
}
