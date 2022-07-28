using DCMS.Core;
using DCMS.Core.Domain.Security;
using DCMS.Web.Framework.Models;

namespace DCMS.Web.Framework.Factories
{

    public partial interface IAclSupportedModelFactory
    {

        void PrepareModelUserRoles<TModel>(TModel model) where TModel : IAclSupportedModel;

        void PrepareModelUserRoles<TModel, TEntity>(TModel model, TEntity entity, bool ignoreAclMappings)
            where TModel : IAclSupportedModel where TEntity : BaseEntity, IAclSupported;
    }
}