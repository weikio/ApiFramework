using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Weikio.ApiFramework.Abstractions
{
    public static class IEndpointCacheExtensions
    {
        public static byte[] GetOrCreateData(this IEndpointCache cache, string key, Func<byte[]> getValue)
        {
            var data = cache.GetData(key);
            if (data == null)
            {
                data = getValue();
                cache.SetData(key, data);
            }

            return data;
        }

        public static async Task<byte[]> GetOrCreateDataAsync(this IEndpointCache cache, string key, Func<Task<byte[]>> getValue, CancellationToken token = default)
        {
            var data = await cache.GetDataAsync(key, token);
            if (data == null)
            {
                data = await getValue();
                await cache.SetDataAsync(key, data, token);
            }

            return data;
        }

        public static string GetString(this IEndpointCache cache, string key)
        {
            var data = cache.GetData(key);
            if (data == null)
            {
                return null;
            }

            return Encoding.UTF8.GetString(data, 0, data.Length);
        }

        public static async Task<string> GetStringAsync(this IEndpointCache cache, string key, CancellationToken token = default)
        {
            var data = await cache.GetDataAsync(key, token);
            if (data == null)
            {
                return null;
            }

            return Encoding.UTF8.GetString(data, 0, data.Length);
        }

        public static void SetString(this IEndpointCache cache, string key, string value)
        {
            cache.SetData(key, Encoding.UTF8.GetBytes(value));
        }

        public static async Task SetStringAsync(this IEndpointCache cache, string key, string value, CancellationToken token = default)
        {
            await cache.SetDataAsync(key, Encoding.UTF8.GetBytes(value), token);
        }

        public static string GetOrCreateString(this IEndpointCache cache, string key, Func<string> getValue)
        {
            var data = cache.GetOrCreateData(key, () =>
            {
                return Encoding.UTF8.GetBytes(getValue());
            });

            return Encoding.UTF8.GetString(data, 0, data.Length);
        }

        public static async Task<string> GetOrCreateStringAsync(this IEndpointCache cache, string key, Func<Task<string>> getValue, CancellationToken token = default)
        {
            var data = await cache.GetOrCreateDataAsync(key, async () =>
            {
                return Encoding.UTF8.GetBytes(await getValue());
            }, token);

            return Encoding.UTF8.GetString(data, 0, data.Length);
        }
    }
}
