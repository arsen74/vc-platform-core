using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.DynamicProperties;
using VirtoCommerce.Platform.Data.Infrastructure;
using VirtoCommerce.Platform.Data.Repositories;

namespace VirtoCommerce.Platform.Data.DynamicProperties
{
    public class DynamicPropertySearchService : IDynamicPropertySearchService
    {
        private readonly Func<IPlatformRepository> _repositoryFactory;
        private readonly IDynamicPropertyService _dynamicPropertyService;
        private readonly IPlatformMemoryCache _memoryCache;
        private readonly IDynamicPropertyDictionaryItemsService _dynamicPropertyDictionaryItemsService;

        public DynamicPropertySearchService(Func<IPlatformRepository> repositoryFactory, IDynamicPropertyService dynamicPropertyService, IPlatformMemoryCache memoryCache, IDynamicPropertyDictionaryItemsService dynamicPropertyDictionaryItemsService)
        {
            _repositoryFactory = repositoryFactory;
            _dynamicPropertyService = dynamicPropertyService;
            _memoryCache = memoryCache;
            _dynamicPropertyDictionaryItemsService = dynamicPropertyDictionaryItemsService;
        }


        #region IDynamicPropertySearchService members
        public virtual async Task<DynamicPropertyDictionaryItemSearchResult> SearchDictionaryItemsAsync(DynamicPropertyDictionaryItemSearchCriteria criteria)
        {
            var cacheKey = CacheKey.With(GetType(), "SearchDictionaryItemsAsync", criteria.GetHashCode().ToString());
            return await _memoryCache.GetOrCreateExclusiveAsync(cacheKey, async (cacheEntry) =>
            {
                cacheEntry.AddExpirationToken(DynamicPropertiesCacheRegion.CreateChangeToken());
                var result = AbstractTypeFactory<DynamicPropertyDictionaryItemSearchResult>.TryCreateInstance();
                using (var repository = _repositoryFactory())
                {
                    //Optimize performance and CPU usage
                    repository.DisableChangesTracking();

                    var query = repository.DynamicPropertyDictionaryItems;

                    if (!string.IsNullOrEmpty(criteria.PropertyId))
                    {
                        query = query.Where(x => x.PropertyId == criteria.PropertyId);
                    }
                    if (!string.IsNullOrEmpty(criteria.Keyword))
                    {
                        query = query.Where(x => x.Name.Contains(criteria.Keyword));
                    }

                    var sortInfos = criteria.SortInfos;
                    if (sortInfos.IsNullOrEmpty())
                    {
                        sortInfos = new[] { new SortInfo { SortColumn = "Name" } };
                    }
                    query = query.OrderBySortInfos(sortInfos);
                    result.TotalCount = await query.CountAsync();
                    var ids = await query.Skip(criteria.Skip)
                                         .Take(criteria.Take)
                                         .Select(x => x.Id)
                                         .ToListAsync();

                    var properties = await _dynamicPropertyDictionaryItemsService.GetDynamicPropertyDictionaryItemsAsync(ids.ToArray());
                    result.Results = properties.OrderBy(x => ids.IndexOf(x.Id))
                                               .ToList();
                }
                return result;
            });
        }

        public virtual async Task<DynamicPropertySearchResult> SearchDynamicPropertiesAsync(DynamicPropertySearchCriteria criteria)
        {
            var cacheKey = CacheKey.With(GetType(), "SearchDynamicPropertiesAsync", criteria.GetCacheKey());
            return await _memoryCache.GetOrCreateExclusiveAsync(cacheKey, async (cacheEntry) =>
            {
                cacheEntry.AddExpirationToken(DynamicPropertiesCacheRegion.CreateChangeToken());
                var result = AbstractTypeFactory<DynamicPropertySearchResult>.TryCreateInstance();
                using (var repository = _repositoryFactory())
                {
                    //Optimize performance and CPU usage
                    repository.DisableChangesTracking();

                    var query = repository.DynamicProperties;

                    if (!string.IsNullOrEmpty(criteria.TypeName))
                    {
                        query = query.Where(x => x.ObjectType == criteria.TypeName);
                    }

                    if (!string.IsNullOrEmpty(criteria.Keyword))
                    {
                        query = query.Where(x => x.Name.Contains(criteria.Keyword));
                    }

                    if (!criteria.ObjectTypes.IsNullOrEmpty())
                    {
                        query = query.Where(m => criteria.ObjectTypes.Contains(m.ObjectType));
                    }

                    var sortInfos = criteria.SortInfos;
                    if (sortInfos.IsNullOrEmpty())
                    {
                        sortInfos = new[] { new SortInfo { SortColumn = "Name" } };
                    }
                    query = query.OrderBySortInfos(sortInfos);
                    result.TotalCount = await query.CountAsync();
                    var ids = await query.Skip(criteria.Skip)
                                         .Take(criteria.Take)
                                         .Select(x => x.Id)
                                         .ToListAsync();

                    var properties = await _dynamicPropertyService.GetDynamicPropertiesAsync(ids.ToArray());
                    result.Results = properties.OrderBy(x => ids.IndexOf(x.Id))
                                               .ToList();
                }
                return result;
            });
        }
        #endregion
    }
}
