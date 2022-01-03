using System;
using System.Collections.Generic;
using System.Text;

namespace Weikio.ApiFramework.Core.Cache
{
    public interface ICacheOptions
    {
        public Func<string, string> KeyFunc { get; }
    }
}
