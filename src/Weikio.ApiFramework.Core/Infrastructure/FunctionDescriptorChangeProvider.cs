using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Primitives;

namespace Weikio.ApiFramework.Core.Infrastructure
{
    public class ActionDescriptorChangeProvider : IActionDescriptorChangeProvider
    {
        public ActionDescriptorChangeProvider(FunctionChangeToken changeToken)
        {
            ChangeToken = changeToken;
        }

        public FunctionChangeToken ChangeToken { get; }

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

//    public class FunctionDescriptorChangeProvider : IActionDescriptorChangeProvider
//    {
//        public static FunctionDescriptorChangeProvider Instance { get; } = new FunctionDescriptorChangeProvider();
//
//        public CancellationTokenSource TokenSource { get; private set; }
//
//        public IChangeToken GetChangeToken()
//        {
//            TokenSource = new CancellationTokenSource();
//
//            return new CancellationChangeToken(TokenSource.Token);
//        }
//    }

//    public class FunctionDescriptorChangeProvider
//    {
//        public static FunctionDescriptorChangeProvider Instance { get; } = new FunctionDescriptorChangeProvider();
//
//        public CancellationTokenSource TokenSource { get; private set; }
//
//        public IChangeToken GetChangeToken()
//        {
//            TokenSource = new CancellationTokenSource();
//
//            return new CancellationChangeToken(TokenSource.Token);
//        }
//    }
}
