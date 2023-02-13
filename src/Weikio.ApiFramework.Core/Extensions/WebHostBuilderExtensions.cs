using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Weikio.ApiFramework.Abstractions;
using Weikio.ApiFramework.Core.Configuration;
using Weikio.ApiFramework.SDK;

// ReSharper disable once CheckNamespace
namespace Weikio.ApiFramework
{
    public static class WebHostBuilderExtensions
    {
        public static IWebHostBuilder AddApiFrameworkJsonConfigurationFile(this IWebHostBuilder webHostBuilder, string filePath = "apiframework.json")
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentNullException(filePath, nameof(filePath));
            }

            var filePaths = GetFilePaths(filePath);

            foreach (var path in filePaths)
            {
                webHostBuilder.ConfigureAppConfiguration((context, configBuilder) =>
                {
                    configBuilder.AddJsonFile(path, true, true);
                });
            }

            webHostBuilder.ConfigureServices(services =>
            {
                services.AddTransient(typeof(IEndpointConfigurationProvider), typeof(AppConfigurationEndpointConfigurationProvider));
            });

            return webHostBuilder;
        }

        private static List<string> GetFilePaths(string filePath)
        {
            var fileDirectory = new FileInfo(filePath).Directory?.FullName;

            if (string.IsNullOrWhiteSpace(fileDirectory))
            {
                return new List<string>();
            }

            var isFileName = File.Exists(filePath);

            if (isFileName)
            {
                return new List<string> { filePath };
            }

            var isDirectory = Directory.Exists(filePath);
            string searchPattern;
            var searchPath = fileDirectory;

            if (isDirectory)
            {
                searchPath = filePath; 
                searchPattern = "*";
            }
            else
            {
                searchPattern = Path.GetFileName(filePath);
            }

            if (string.IsNullOrWhiteSpace(searchPattern))
            {
                searchPattern = "*";
            }

            var jsonFilesInDirectory = Directory.GetFiles(searchPath, searchPattern);

            return new List<string>(jsonFilesInDirectory);
        }
    }
}
