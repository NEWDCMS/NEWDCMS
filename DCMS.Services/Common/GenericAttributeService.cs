using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Domain.Common;
using DCMS.Core.Infrastructure.DependencyManagement;
using DCMS.Data.Extensions;
using DCMS.Services.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using DCMS.Services.Caching;

namespace DCMS.Services.Common
{
    public partial class GenericAttributeService : BaseService, IGenericAttributeService
    {
        public GenericAttributeService(IServiceGetter getter,
            IStaticCacheManager cacheManager,
            IEventPublisher eventPublisher) : base(getter, cacheManager, eventPublisher)
        {
        }

        #region Methods

        /// <summary>
        /// Deletes an attribute
        /// </summary>
        /// <param name="attribute">Attribute</param>
        public virtual void DeleteAttribute(GenericAttribute attribute)
        {
            if (attribute == null)
            {
                throw new ArgumentNullException("attribute");
            }

            GenericAttributesRepository.Delete(attribute);

            _eventPublisher.EntityDeleted(attribute);
        }

        /// <summary>
        /// Gets an attribute
        /// </summary>
        /// <param name="attributeId">Attribute identifier</param>
        /// <returns>An attribute</returns>
        public virtual GenericAttribute GetAttributeById(int attributeId)
        {
            if (attributeId == 0)
            {
                return null;
            }

            var attribute = GenericAttributesRepository.ToCachedGetById(attributeId);
            return attribute;
        }

        /// <summary>
        /// Inserts an attribute
        /// </summary>
        /// <param name="attribute">attribute</param>
        public virtual void InsertAttribute(GenericAttribute attribute)
        {
            if (attribute == null)
            {
                throw new ArgumentNullException("attribute");
            }

            GenericAttributesRepository.Insert(attribute);

            //event notification
            //_eventPublisher.EntityInserted(attribute);
        }

        /// <summary>
        /// Updates the attribute
        /// </summary>
        /// <param name="attribute">Attribute</param>
        public virtual void UpdateAttribute(GenericAttribute attribute)
        {
            if (attribute == null)
            {
                throw new ArgumentNullException("attribute");
            }

            var uow = GenericAttributesRepository.UnitOfWork;
            GenericAttributesRepository.Update(attribute);
            uow.SaveChanges();

            //event notification
            //_eventPublisher.EntityUpdated(attribute);
        }

        /// <summary>
        /// Get attributes
        /// </summary>
        /// <param name="entityId">Entity identifier</param>
        /// <param name="keyGroup">Key group</param>
        /// <returns>Get attributes</returns>
        public virtual IList<GenericAttribute> GetAttributesForEntity(int? store, int entityId, string keyGroup)
        {
            var query = from ga in GenericAttributesRepository.Table
                        where ga.EntityId == entityId && ga.StoreId == store && ga.KeyGroup == keyGroup
                        select ga;

            var key = DCMSDefaults.GENERICATTRIBUTE_KEY.FillCacheKey(store ?? 0, entityId, keyGroup);
            return _cacheManager.Get(key, () => query.ToList());
        }


        /// <summary>
        /// Save attribute value
        /// </summary>
        /// <typeparam name="TPropType">Property type</typeparam>
        /// <param name="entity">Entity</param>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        public virtual void SaveAttribute<TPropType>(BaseEntity entity, string key, TPropType value, int storeId = 0)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }

            string keyGroup = entity.GetUnproxiedEntityType().Name;

            var props = GetAttributesForEntity(storeId, entity.Id, keyGroup)
                .Where(x => x.StoreId == storeId)
                .ToList();
            var prop = props.FirstOrDefault(ga =>
                ga.Key.Equals(key, StringComparison.InvariantCultureIgnoreCase)); //should be culture invariant

            string valueStr = CommonHelper.To<string>(value);

            if (prop != null)
            {
                if (string.IsNullOrWhiteSpace(valueStr))
                {
                    //delete
                    DeleteAttribute(prop);
                }
                else
                {
                    //update
                    prop.Value = valueStr;
                    UpdateAttribute(prop);
                }
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(valueStr))
                {
                    //insert
                    prop = new GenericAttribute()
                    {
                        EntityId = entity.Id,
                        Key = key,
                        KeyGroup = keyGroup,
                        Value = valueStr,
                        StoreId = storeId

                    };
                    InsertAttribute(prop);
                }
            }
        }
        public virtual TPropType GetAttribute<TPropType>(BaseEntity entity, string key, int storeId = 0, TPropType defaultValue = default(TPropType))
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            var keyGroup = entity.GetUnproxiedEntityType().Name;

            var props = GetAttributesForEntity(storeId, entity.Id, keyGroup);

            //little hack here (only for unit testing). we should write expect-return rules in unit tests for such cases
            if (props == null)
            {
                return defaultValue;
            }

            props = props.Where(x => x.StoreId == storeId).ToList();
            if (!props.Any())
            {
                return defaultValue;
            }

            var prop = props.FirstOrDefault(ga =>
                ga.Key.Equals(key, StringComparison.InvariantCultureIgnoreCase)); //should be culture invariant

            if (prop == null || string.IsNullOrEmpty(prop.Value))
            {
                return defaultValue;
            }

            return CommonHelper.To<TPropType>(prop.Value);
        }

        #endregion
    }
}