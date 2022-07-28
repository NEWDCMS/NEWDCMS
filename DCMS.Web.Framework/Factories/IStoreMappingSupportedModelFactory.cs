using DCMS.Core;
using DCMS.Core.Domain.Stores;
using DCMS.Web.Framework.Models;

namespace DCMS.Web.Framework.Factories
{

    public partial interface IStoreMappingSupportedModelFactory
    {

        void PrepareModelStores<TModel>(TModel model) where TModel : IStoreMappingSupportedModel;


        void PrepareModelStores<TModel, TEntity>(TModel model, TEntity entity, bool ignoreStoreMappings)
            where TModel : IStoreMappingSupportedModel where TEntity : BaseEntity, IStoreMappingSupported;
    }
}