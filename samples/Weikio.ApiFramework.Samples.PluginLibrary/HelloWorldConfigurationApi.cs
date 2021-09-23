namespace Weikio.ApiFramework.Samples.PluginLibrary
{
    public class HelloWorldConfigurationApi
    {
        public string Configuration { get; set; }
        
        public string SayHello()
        {
            return Configuration;
        }
    }
}
