using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Refit;
using System;

namespace MicroRabbit.Infra.IoC.Extensions.Refit
{
    public static class RefitClientExtension
    {
        public static void RegisterRefitClientServices<T>(this IServiceCollection services, string baseAddress)
            where T : class
        {
            var refitSettings = new RefitSettings()
            {
                ContentSerializer = new JsonContentSerializer(new Newtonsoft.Json.JsonSerializerSettings()
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                    Converters = { new StringEnumConverter() },
                    NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore,
                    ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
                })
            };

            services
                .AddRefitClient<T>(refitSettings)
                .ConfigureHttpClient(c => c.BaseAddress = new Uri(baseAddress));
        }
    }
}
