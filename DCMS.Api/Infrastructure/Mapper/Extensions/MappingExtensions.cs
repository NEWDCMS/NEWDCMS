using DCMS.Core;
using DCMS.Core.Configuration;
using DCMS.Core.Infrastructure.Mapper;
using DCMS.Web.Framework.Models;
using System;

namespace DCMS.Api.Infrastructure.Mapper.Extensions
{
    /// <summary>
    ///  AutoMapper 实体领域模型映射扩展V2.0 实现推断机制
    /// </summary>
    public static class MappingExtensions
    {

        private static TDestination Map<TDestination>(this object source)
        {
            return AutoMapperConfiguration.Mapper.Map<TDestination>(source);
        }
        private static TDestination MapTo<TSource, TDestination>(this TSource source, TDestination destination)
        {
            return AutoMapperConfiguration.Mapper.Map(source, destination);
        }


        public static TModel ToModel<TModel>(this BaseEntity entity) where TModel : BaseEntityModel
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            return entity.Map<TModel>();
        }
        public static TModel ToModel<TEntity, TModel>(this TEntity entity, TModel model) where TEntity : BaseEntity where TModel : BaseEntityModel
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            return entity.MapTo(model);
        }


        public static TEntity ToEntity<TEntity>(this BaseEntityModel model) where TEntity : BaseEntity
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            return model.Map<TEntity>();
        }
        public static TEntity ToEntity<TEntity, TModel>(this TModel model, TEntity entity) where TEntity : BaseEntity where TModel : BaseEntityModel
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            return model.MapTo(entity);
        }


        //
        public static TModel ToAccountModel<TModel>(this BaseAccount entity) where TModel : BaseAccountModel
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            return entity.Map<TModel>();
        }
        public static TModel ToAccountModel<TEntity, TModel>(this BaseAccount entity, TModel model) where TEntity : BaseAccount where TModel : BaseAccountModel
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            return entity.MapTo(model);
        }

        public static TEntity ToAccountEntity<TEntity>(this BaseAccountModel model) where TEntity : BaseAccount
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            return model.Map<TEntity>();
        }
        public static TEntity ToAccountEntity<TEntity, TModel>(this BaseAccountModel model, TEntity entity) where TEntity : BaseAccount where TModel : BaseAccountModel
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            return model.MapTo(entity);
        }



        public static TModel ToSettingsModel<TModel>(this ISettings settings) where TModel : BaseModel, ISettingsModel
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            return settings.Map<TModel>();
        }
        public static TSettings ToSettings<TSettings, TModel>(this TModel model, TSettings settings) where TSettings : class, ISettings where TModel : BaseModel, ISettingsModel
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            return model.MapTo(settings);
        }
    }
}