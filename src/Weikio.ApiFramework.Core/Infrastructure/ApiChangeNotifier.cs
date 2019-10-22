namespace Weikio.ApiFramework.Core.Infrastructure
{
    public class ApiChangeNotifier
    {
        private readonly ApiChangeToken _changeToken;

        public ApiChangeNotifier(ApiChangeToken changeToken)
        {
            _changeToken = changeToken;
        }

        public void Notify()
        {
            _changeToken.TokenSource.Cancel();
        }
    }
}
