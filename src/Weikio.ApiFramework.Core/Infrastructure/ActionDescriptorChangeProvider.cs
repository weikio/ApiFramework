using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Primitives;

namespace Weikio.ApiFramework.Core.Infrastructure
{
    public class ActionDescriptorChangeProvider : IActionDescriptorChangeProvider
    {
        public ActionDescriptorChangeProvider(ApiChangeToken changeToken)
        {
            ChangeToken = changeToken;
        }

        public ApiChangeToken ChangeToken { get; }

        public IChangeToken GetChangeToken()
        {
            if (ChangeToken.TokenSource.IsCancellationRequested)
            {
                ChangeToken.Initialize();

                return new CancellationChangeToken(ChangeToken.TokenSource.Token);
            }

            return new CancellationChangeToken(ChangeToken.TokenSource.Token);
        }
    }
}
