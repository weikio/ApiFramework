namespace Weikio.ApiFramework.Plugins.HelloWorld
{
    public class ReturnMessage
    {
        public ReturnMessage(string result)
        {
            Result = result;
        }

        public string Result { get; set; }
    }
}
