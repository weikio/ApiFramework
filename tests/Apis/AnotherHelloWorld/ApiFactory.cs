using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AnotherHelloWorld
{
    public static class ApiFactory
    {
        public static Task<IEnumerable<Type>> Create()
        {
            var result = new List<Type>(){typeof(TestFunctionality)};

            return Task.FromResult<IEnumerable<Type>>(result);
        }
    }
}
