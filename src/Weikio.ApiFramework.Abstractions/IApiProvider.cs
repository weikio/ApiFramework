using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Weikio.ApiFramework.Abstractions
{
    public interface IApiProvider
    {
        Task Initialize(CancellationToken cancellationToken);
        List<ApiDefinition> List();
        List<IApiCatalog> ListCatalogs();
        Api Get(ApiDefinition definition);
        void Add(IApiCatalog catalog);
        void Remove(IApiCatalog catalog);
    }

    public class ApiCatalogNotInitializedException : Exception
    {
        public ApiCatalogNotInitializedException():base("Api Catalog is not initialized. Initialize catalog before adding it to Api Provider")
        {
        }
    }
}
