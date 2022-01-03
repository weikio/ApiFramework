using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Weikio.ApiFramework.Abstractions
{
    public interface IEndpointCache
    {
        string GetOrCreateString(string key, Func<string> getString);
        Task<string> GetOrCreateStringAsync(string key, Func<Task<string>> getString);
        string GetString(string key);
        void SetString(string key, string value);

        object GetOrCreateObject(string key, Func<object> getObject);
        Task<object> GetOrCreateObjectAsync(string key, Func<Task<object>> getString);
        object GetObject(string key);
        void SetObject(string key, object value);
    }
}
