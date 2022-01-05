using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Weikio.ApiFramework.Abstractions
{
    public interface IApiCache
    {
        string GetOrCreateString(Endpoint endpoint, string key, Func<string> getString);
        Task<string> GetOrCreateStringAsync(Endpoint endpoint, string key, Func<Task<string>> getString);
        string GetString(Endpoint endpoint, string key);
        void SetString(Endpoint endpoint, string key, string value);

        byte[] GetOrCreateObject(Endpoint endpoint, string key, Func<byte[]> getObject);
        Task<byte[]> GetOrCreateObjectAsync(Endpoint endpoint, string key, Func<Task<byte[]>> getObject);
        byte[] GetObject(Endpoint endpoint, string key);
        void SetObject(Endpoint endpoint, string key, byte[] value);
    }
}
