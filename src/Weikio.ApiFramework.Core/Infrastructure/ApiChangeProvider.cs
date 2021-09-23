using System;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Primitives;

namespace Weikio.ApiFramework.Core.Infrastructure
{
    public class ApiChangeProvider : IActionDescriptorChangeProvider
    {
        private readonly Func<IChangeToken> _changeTokenProducer;

        public ApiChangeProvider(Func<IChangeToken> changeTokenProducer)
        {
            _changeTokenProducer = changeTokenProducer;
        }

        public IChangeToken GetChangeToken()
        {
            var result = _changeTokenProducer.Invoke();

            return result;
        }
    }
}
