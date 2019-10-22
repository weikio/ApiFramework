namespace Weikio.ApiFramework.Plugins.HelloWorld
{
    public class HelloWorldWithMultipleParameterFunction
    {
        public MyConfiguration Configuration { get; set; }

        public string SayHello(string name, bool ready, int? age)
        {
            return $"Name {name}, Ready: {ready}, Age: {age.GetValueOrDefault()}";
        }
    }
}
