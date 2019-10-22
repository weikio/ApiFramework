using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace Weikio.AspNetCore.Common
{
    public static class IServiceCollectionExtensions
    {
        public static List<Type> GetByInterface(this IServiceCollection services, Type interfaceType)
        {
            var result = new List<Type>();

            foreach (var serviceRegistration in services)
            {
                if (!interfaceType.IsAssignableFrom(serviceRegistration.ImplementationType))
                {
                    continue;
                }

                result.Add(serviceRegistration.ImplementationType);
            }

            return result;
        }
    }

    namespace Adafy.Common.BackgroundTasks
    {
    }
}
