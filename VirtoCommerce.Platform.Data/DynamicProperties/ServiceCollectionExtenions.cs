using Microsoft.Extensions.DependencyInjection;
using VirtoCommerce.Platform.Core.DynamicProperties;

namespace VirtoCommerce.Platform.Data.DynamicProperties
{
    public static class ServiceCollectionExtenions
    {
        public static IServiceCollection AddDynamicProperties(this IServiceCollection services)
        {
            services.AddSingleton<IDynamicPropertyService, DynamicPropertyService>();
            services.AddSingleton<IDynamicPropertySearchService, DynamicPropertySearchService>();
            services.AddSingleton<IDynamicPropertyRegistrar, DynamicPropertyService>();
            services.AddSingleton<IDynamicPropertyDictionaryItemsService, DynamicPropertyDictionaryItemsService>();
            services.AddSingleton<IDynamicPropertyMetaInfoService, DynamicPropertyMetaInfoService>();

            return services;
        }
    }
}
