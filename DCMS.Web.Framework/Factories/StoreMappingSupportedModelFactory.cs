using DCMS.Core;
using DCMS.Core.Domain.Stores;
using DCMS.Services.Stores;
using DCMS.Web.Framework.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Linq;

namespace DCMS.Web.Framework.Factories
{
    /// <summary>
    /// 表示支持基本存储映射的模型工厂实现
    /// </summary>
    public partial class StoreMappingSupportedModelFactory : IStoreMappingSupportedModelFactory
    {
        private readonly IStoreMappingService _storeMappingService;
        private readonly IStoreService _storeService;


        public StoreMappingSupportedModelFactory(IStoreMappingService storeMappingService,
            IStoreService storeService)
        {
            _storeMappingService = storeMappingService;
            _storeService = storeService;
        }



        public virtual void PrepareModelStores<TModel>(TModel model) where TModel : IStoreMappingSupportedModel
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var availableStores = _storeService.GetAllStores(true);
            model.AvailableStores = availableStores.Select(store => new SelectListItem
            {
                Text = store.Name,
                Value = store.Id.ToString(),
                Selected = model.SelectedStoreIds.Contains(store.Id)
            }).ToList();
        }


        public virtual void PrepareModelStores<TModel, TEntity>(TModel model, TEntity entity, bool ignoreStoreMappings)
            where TModel : IStoreMappingSupportedModel where TEntity : BaseEntity, IStoreMappingSupported
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            if (!ignoreStoreMappings && entity != null)
            {
                model.SelectedStoreIds = _storeMappingService.GetStoresIdsWithAccess(entity).ToList();
            }

            PrepareModelStores(model);
        }


    }
}