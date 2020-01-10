namespace HelloWorld
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
