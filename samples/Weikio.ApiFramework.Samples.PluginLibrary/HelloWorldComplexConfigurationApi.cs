﻿namespace Weikio.ApiFramework.Samples.PluginLibrary
{
    public class HelloWorldComplexConfigurationApi
    {
        public Complex Configuration { get; set; }
        
        public string SayHello()
        {
            return $"{Configuration.Name}-{Configuration.Country}-{Configuration.Age}";
        }
    }
}