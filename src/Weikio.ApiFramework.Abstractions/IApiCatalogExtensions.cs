using System;

namespace Weikio.ApiFramework.Abstractions
{
    public static class IApiCatalogExtensions
    {
        public static Api Get(this IApiCatalog catalog, string name, Version version)
        {
            return catalog.Get(new ApiDefinition(name, version));
        }

        public static Api Get(this IApiCatalog catalog, string name)
        {
            return Get(catalog, name, new Version(1, 0, 0, 0));
        }
    }
}