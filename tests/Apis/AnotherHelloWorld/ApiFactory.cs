using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
