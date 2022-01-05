using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Weikio.ApiFramework.Abstractions
{
    public interface IEndpointCache
    {
        string GetOrCreateString(string key, Func<string> getString);
        Task<string> GetOrCreateStringAsync(string key, Func<Task<string>> getString);
        string GetString(string key);
        void SetString(string key, string value);

        byte[] GetOrCreateObject(string key, Func<byte[]> getObject);
        Task<byte[]> GetOrCreateObjectAsync(string key, Func<Task<byte[]>> getObject);
        byte[] GetObject(string key);
        void SetObject(string key, byte[] value);
    }
}
