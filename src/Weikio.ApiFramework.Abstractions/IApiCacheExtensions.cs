using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Weikio.ApiFramework.Abstractions
{
    public static class IApiCacheExtensions
    {
        public static void Set(this IApiCache cache, Endpoint endpoint, string key, byte[] value, TimeSpan absoluteExpirationRelativeToNow)
        {
            cache.Set(endpoint, key, value, new ApiCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow
            });
        }

        public static async Task SetAsync(this IApiCache cache, Endpoint endpoint, string key, byte[] value, TimeSpan absoluteExpirationRelativeToNow, CancellationToken token = default)
        {
            await cache.SetAsync(endpoint, key, value, new ApiCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow
            }, token);
        }

        public static byte[] GetOrCreate(this IApiCache cache, Endpoint endpoint, string key, ApiCacheEntryOptions options, Func<byte[]> getValue)
        {
            var data = cache.Get(endpoint, key);
            if (data == null)
            {
                data = getValue();
                cache.Set(endpoint, key, data, options);
            }

            return data;
        }

        public static byte[] GetOrCreate(this IApiCache cache, Endpoint endpoint, string key, TimeSpan absoluteExpirationRelativeToNow, Func<byte[]> getValue)
        {
            var options = new ApiCacheEntryOptions() { AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow };
            
            return cache.GetOrCreate(endpoint, key, options, getValue);    
        }

        public static async Task<byte[]> GetOrCreateAsync(this IApiCache cache, Endpoint endpoint, string key, ApiCacheEntryOptions options, Func<Task<byte[]>> getValue, CancellationToken token = default)
        {
            var data = await cache.GetAsync(endpoint, key, token);
            if (data == null)
            {
                data = await getValue();
                await cache.SetAsync(endpoint, key, data, options, token);
            }

            return data;
        }

        public static async Task<byte[]> GetOrCreateAsync(this IApiCache cache, Endpoint endpoint, string key, TimeSpan absoluteExpirationRelativeToNow, Func<Task<byte[]>> getValue, CancellationToken token = default)
        {
            var options = new ApiCacheEntryOptions() { AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow };
           
            return await cache.GetOrCreateAsync(endpoint, key, options, getValue, token);
        }

        public static string GetString(this IApiCache cache, Endpoint endpoint, string key)
        {
            var data = cache.Get(endpoint, key);
            if (data == null)
            {
                return null;
            }

            return Encoding.UTF8.GetString(data, 0, data.Length);
        }

        public static async Task<string> GetStringAsync(this IApiCache cache, Endpoint endpoint, string key, CancellationToken token = default)
        {
            var data = await cache.GetAsync(endpoint, key, token);
            if (data == null)
            {
                return null;
            }

            return Encoding.UTF8.GetString(data, 0, data.Length);
        }

        public static void SetString(this IApiCache cache, Endpoint endpoint, string key, string value, ApiCacheEntryOptions options)
        {
            cache.Set(endpoint, key, Encoding.UTF8.GetBytes(value), options);
        }

        public static void SetString(this IApiCache cache, Endpoint endpoint, string key, string value, TimeSpan absoluteExpirationRelativeToNow)
        {
            cache.Set(endpoint, key, Encoding.UTF8.GetBytes(value), absoluteExpirationRelativeToNow);
        }

        public static async Task SetStringAsync(this IApiCache cache, Endpoint endpoint, string key, string value, ApiCacheEntryOptions options, CancellationToken token = default)
        {
            await cache.SetAsync(endpoint, key, Encoding.UTF8.GetBytes(value), options, token);
        }

        public static async Task SetStringAsync(this IApiCache cache, Endpoint endpoint, string key, string value, TimeSpan absoluteExpirationRelativeToNow, CancellationToken token = default)
        {
            await cache.SetAsync(endpoint, key, Encoding.UTF8.GetBytes(value), absoluteExpirationRelativeToNow, token);
        }

        public static string GetOrCreateString(this IApiCache cache, Endpoint endpoint, string key, ApiCacheEntryOptions options, Func<string> getValue)
        {
            var data = cache.GetOrCreate(endpoint, key, options, () =>
            {
                return Encoding.UTF8.GetBytes(getValue());
            });

            return Encoding.UTF8.GetString(data, 0, data.Length);
        }

        public static string GetOrCreateString(this IApiCache cache, Endpoint endpoint, string key, TimeSpan absoluteExpirationRelativeToNow, Func<string> getValue)
        {
            var options = new ApiCacheEntryOptions() { AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow };
            
            return cache.GetOrCreateString(endpoint, key, options, getValue);    
        }

        public static async Task<string> GetOrCreateStringAsync(this IApiCache cache, Endpoint endpoint, string key, ApiCacheEntryOptions options, Func<Task<string>> getValue, CancellationToken token = default)
        {
            var data = await cache.GetOrCreateAsync(endpoint, key, options, async () =>
            {
                return Encoding.UTF8.GetBytes(await getValue());
            }, token);

            return Encoding.UTF8.GetString(data, 0, data.Length);
        }

        public static async Task<string> GetOrCreateStringAsync(this IApiCache cache, Endpoint endpoint, string key, TimeSpan absoluteExpirationRelativeToNow, Func<Task<string>> getValue, CancellationToken token = default)
        {
            var options = new ApiCacheEntryOptions() { AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow };

            return await cache.GetOrCreateStringAsync(endpoint, key, options, getValue, token);
        }
    }
}
