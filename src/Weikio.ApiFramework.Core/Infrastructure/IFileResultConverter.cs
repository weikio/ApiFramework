using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Weikio.ApiFramework.Core.Infrastructure
{
    public interface IFileResultConverter
    {
        bool CanConvertType(Type type);
        Task<FileStreamResult> Convert(object obj);
    }
}
