namespace Weikio.ApiFramework.Plugins.HelloWorld
{
    public class HelloWorldFunction
    {
        public string SayHello()
        {
            return "Hello Function Framework!";
        }

        public ReturnMessage CreateMessage(string name)
        {
            return new ReturnMessage($"Hello {name}");
        }
    }
}
