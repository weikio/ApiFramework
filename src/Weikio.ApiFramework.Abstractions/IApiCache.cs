using System;
using System.Collections.Generic;
using System.Text;

namespace Weikio.ApiFramework.Abstractions
{
    public interface IApiCache
    {
        string GetOrCreateString(string key, Func<string> getString);
        string GetString(string key);
        void SetString(string key, string value);

        object GetObject(string key);
        void SetObject(string key, object value);
    }
}
