using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Weikio.ApiFramework.Abstractions
{
    public interface IApiCache
    {
        byte[] GetData(Endpoint endpoint, string key);
        Task<byte[]> GetDataAsync(Endpoint endpoint, string key, CancellationToken token = default);

        void SetData(Endpoint endpoint, string key, byte[] value);
        Task SetDataAsync(Endpoint endpoint, string key, byte[] value, CancellationToken token = default);
    }
}
