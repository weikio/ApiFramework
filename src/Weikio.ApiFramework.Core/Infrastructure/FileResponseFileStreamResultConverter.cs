using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;

namespace Weikio.ApiFramework.Core.Infrastructure
{
    /// <summary>
    /// Can convert NSwag's FileResponse-type into FileStreamResult. FileResponse is dynamically code generated by NSwag so we can't directly compare types.
    /// </summary>
    public class FileResponseFileStreamResultConverter : IFileStreamResultConverter
    {
        public bool CanConvertType(Type type)
        {
            if (type == null)
            {
                return false;
            }

            if (!string.Equals(type.Name, "FileResponse", StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }

            var streamProperty = type.GetProperty("Stream");

            if (streamProperty == null)
            {
                return false;
            }

            var result = streamProperty.PropertyType == typeof(Stream);

            return result;
        }

        public Task<FileStreamResult> Convert(object obj)
        {
            // Todo: Could we cache the reflections?
            var streamProperty = obj.GetType().GetProperty("Stream");

            if (streamProperty == null)
            {
                throw new Exception($"Couldn't locate Stream property from {obj.GetType().FullName}");
            }

            var stream = (Stream) streamProperty.GetValue(obj);

            var contentType = "application/octet-stream";

            var headersProperty = obj.GetType().GetProperty("Headers");

            if (headersProperty == null)
            {
                return Task.FromResult(new FileStreamResult(stream, contentType));
            }

            var headers = (IReadOnlyDictionary<string, IEnumerable<string>>) headersProperty.GetValue(obj);

            if (headers?.ContainsKey("Content-Type") == true && headers["Content-Type"]?.Any() == true)
            {
                contentType = headers["Content-Type"].First();
            }

            string fileName = null;

            if (headers?.ContainsKey("Content-Disposition") == true && headers["Content-Disposition"]?.Any() == true)
            {
                // Content-Disposition's format is like the following:
                //  content-disposition: attachment; filename=StorageExplorer.exe; filename*=UTF-8''StorageExplorer.exe 
                // Try to parse the first filename
                var attributes = headers["Content-Disposition"].First();

                if (attributes.Contains("filename="))
                {
                    var fileNameAttribute = attributes.Substring(attributes.IndexOf("filename=", StringComparison.Ordinal));
                    fileNameAttribute = fileNameAttribute.Substring(fileNameAttribute.IndexOf("=", StringComparison.Ordinal) + 1);
                    fileName = fileNameAttribute.Substring(0, fileNameAttribute.IndexOf(";", StringComparison.Ordinal));
                }
            }

            var result = new FileStreamResult(stream, contentType);

            if (fileName != null)
            {
                result.FileDownloadName = fileName;
            }

            return Task.FromResult(result);
        }
    }
}
