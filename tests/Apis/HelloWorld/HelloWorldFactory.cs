using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HelloWorld
{
    public static class ApiFactory
    {
        public static Task<IEnumerable<Type>> Create()
        {
            var result = new List<Type>(){typeof(HelloWorldApi)};

            return Task.FromResult<IEnumerable<Type>>(result);
        }
    }
}
