using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Weikio.ApiFramework.SDK;

namespace Weikio.ApiFramework.Samples.Admin
{
    public class HelloWorld
    {
        private readonly ILogger<HelloWorld> _logger;
        private readonly HelloSayer _sayer;

        public HelloWorld(ILogger<HelloWorld> logger, HelloSayer sayer)
        {
            _logger = logger;
            _sayer = sayer;
        }

        public string SayHello()
        {
            return _sayer.SayHello();
        }
    }

    public class HelloSayer
    {
        public string SayHello()
        {
            return "Hello There";
        } 
    }

    public class HelloWorldApiFactory
    {
        private readonly IServiceCollection _services;

        public HelloWorldApiFactory(IServiceCollection services)
        {
            _services = services;
        }

        public ApiFactoryResult Create()
        {
            _services.AddSingleton<HelloSayer>();

            return new ApiFactoryResult(typeof(HelloWorld), _services);
        }
    }
}
