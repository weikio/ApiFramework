namespace Weikio.ApiFramework.Plugins.HelloWorld
{
    public class HelloWorldApi
    {
        public string SayHello()
        {
            return "Hello Api Framework!";
        }

        public ReturnMessage CreateMessage(string name)
        {
            return new ReturnMessage($"Hello {name}");
        }
    }
}
