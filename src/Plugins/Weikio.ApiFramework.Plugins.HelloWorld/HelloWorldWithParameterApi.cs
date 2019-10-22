namespace Weikio.ApiFramework.Plugins.HelloWorld
{
    public class HelloWorldWithParameterApi
    {
        public MyConfiguration Configuration { get; set; }

        public HelloWorldWithParameterApi()
        {
        }

        public string HelloThere(string name)
        {
            if (Configuration == null)
            {
                return $"Configuration null: {name}";
            }

            if (string.IsNullOrWhiteSpace(Configuration?.HelloString))
            {
                return $"Configuration string missing: {name}";
            }

            return $"{Configuration.HelloString} {name}";
        }
    }
}
