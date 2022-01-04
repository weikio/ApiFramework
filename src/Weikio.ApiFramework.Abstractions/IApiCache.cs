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

        object GetOrCreateObject(Endpoint endpoint, string key, Func<object> getObject);
        Task<object> GetOrCreateObjectAsync(Endpoint endpoint, string key, Func<Task<object>> getString);
        object GetObject(Endpoint endpoint, string key);
        void SetObject(Endpoint endpoint, string key, object value);
    }
}