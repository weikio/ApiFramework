using System;
using System.Collections.Generic;
using System.Text;

namespace Weikio.ApiFramework.Core.Cache
{
    public class ApiCacheOptions
    {
        public int ExpirationTimeInSeconds { get; set; } = 60;
    }
}
