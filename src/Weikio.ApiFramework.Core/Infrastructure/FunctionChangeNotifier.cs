namespace Weikio.ApiFramework.Core.Infrastructure
{
    public class FunctionChangeNotifier
    {
        private readonly FunctionChangeToken _changeToken;

        public FunctionChangeNotifier(FunctionChangeToken changeToken)
        {
            _changeToken = changeToken;
        }

        public void Notify()
        {
            _changeToken.TokenSource.Cancel();
        }
    }
}
