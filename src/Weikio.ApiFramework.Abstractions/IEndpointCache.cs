using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Weikio.ApiFramework.Abstractions
{
    public interface IEndpointCache
    {
        byte[] GetData(string key);
        Task<byte[]> GetDataAsync(string key, CancellationToken token = default);

        void SetData(string key, byte[] value, ApiCacheEntryOptions options);
        Task SetDataAsync(string key, byte[] value, ApiCacheEntryOptions options, CancellationToken token = default);
    }
}
