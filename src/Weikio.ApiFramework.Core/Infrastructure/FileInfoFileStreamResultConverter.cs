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

            var result = new FileStreamResult(stream, "application/octet-stream");
            var fileName = Path.GetFileName(info.FullName);
            result.FileDownloadName = fileName;

            return Task.FromResult(result);
        }
    }
}
