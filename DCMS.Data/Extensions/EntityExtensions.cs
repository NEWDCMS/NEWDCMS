using DCMS.Core;
using DCMS.Core.Caching;
using System;

namespace DCMS.Data.Extensions
{
    /// <summary>
    /// Entity 扩展方法
    /// </summary>
    public static class EntityExtensions
    {
        /// <summary>
        /// 检查是否代理
        /// </summary>
        /// <param name="entity">Entity</param>
        /// <returns>Result</returns>
        private static bool IsProxy(this BaseEntity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            //in EF 6 we could use ObjectContext.GetObjectType. Now it's not available. Here is a workaround:

            var type = entity.GetType();
            //e.g. "CustomerProxy" will be derived from "Customer". And "Customer" is derived from BaseEntity
            return type.BaseType != null && type.BaseType.BaseType != null && type.BaseType.BaseType == typeof(BaseEntity);
        }

        /// <summary>
        /// 获取未代理实体类型
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="Exception"></exception>
        public static Type GetUnproxiedEntityType(this BaseEntity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            Type type = null;
            //cachable entity (get the base entity type)
            if (entity is IEntityForCaching)
            {
                type = ((IEntityForCaching)entity).GetType().BaseType;
            }
            //EF proxy
            else if (entity.IsProxy())
            {
                type = entity.GetType().BaseType;
            }
            //not proxied entity
            else
            {
                type = entity.GetType();
            }

            if (type == null)
            {
                throw new Exception("Original entity type cannot be loaded");
            }

            return type;
        }



    }
}