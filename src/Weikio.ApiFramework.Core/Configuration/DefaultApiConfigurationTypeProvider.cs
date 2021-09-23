using System;
using System.Collections.Generic;
using System.Linq;
using Weikio.ApiFramework.Abstractions;

namespace Weikio.ApiFramework.Core.Configuration
{
    public class DefaultApiConfigurationTypeProvider : List<ApiConfiguration>, IApiConfigurationTypeProvider
    {
        public ApiConfiguration GetByApi(ApiDefinition apiDefinition)
        {
            if (apiDefinition == null)
            {
                throw new ArgumentNullException(nameof(apiDefinition));
            }

            return this.FirstOrDefault(x => x.ApiDefinition == apiDefinition);
        }
    }
}
