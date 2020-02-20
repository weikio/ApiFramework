using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Weikio.ApiFramework.Core.Infrastructure
{
    public class FileInfoFileStreamResultConverter : IFileStreamResultConverter
    {
        public bool CanConvertType(Type type)
        {
            return type == typeof(FileInfo);
        }

        public Task<FileStreamResult> Convert(object obj)
        {
            var info = (FileInfo) obj;
            var stream = File.OpenRead(info.FullName);

            return Task.FromResult(new FileStreamResult(stream, "application/octet-stream")); 
        }
    }
}
