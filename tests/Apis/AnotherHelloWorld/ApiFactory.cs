using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AnotherHelloWorld;
using Weikio.ApiFramework.SDK;

namespace AnotherHelloWorld
{
    public static class ApiFactory
    {
        public static Task<IEnumerable<Type>> Create()
        {
            var result = new List<Type>() { typeof(TestFunctionality) };

            return Task.FromResult<IEnumerable<Type>>(result);
        }
    }
}

namespace AnotherHelloWorld.SingleNonTask
{
    public class ApiFactory
    {
        public ApiFactory()
        {
        }

        public Type Create()
        {
            return typeof(TestFunctionality);
        }
    }
}

namespace AnotherHelloWorld.SingleTask
{
    public class ApiFactory
    {
        public ApiFactory()
        {
        }

        public async Task<Type> Create()
        {
            await Task.Delay(TimeSpan.FromMilliseconds(10));
            
            return typeof(TestFunctionality);
        }
    }
}

namespace AnotherHelloWorld.MultiNonTask
{
    public class ApiFactory
    {
        public ApiFactory()
        {
        }

        public  List<Type> Create()
        {
            return new List<Type>() { typeof(TestFunctionality) };
        }
    }
}

namespace AnotherHelloWorld.MultiTask
{
    public class ApiFactory
    {
        public async Task<List<Type>> Create()
        {
            await Task.Delay(TimeSpan.FromMilliseconds(10));
            return new List<Type>() { typeof(TestFunctionality) };
        }
    }
}


namespace HelloWorldContext
{
    public class ApiFactory
    {
        private readonly ApiEndpointFactoryContext _context;

        public ApiFactory(ApiEndpointFactoryContext context)
        {
            _context = context;
        }

        public Task<Type> Create()
        {
            var result = typeof(TestFunctionality);

            return Task.FromResult(result);
        }
    }
}
