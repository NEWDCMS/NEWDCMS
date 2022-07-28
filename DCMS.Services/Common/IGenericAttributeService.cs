using DCMS.Core;
using DCMS.Core.Domain.Common;
using System.Collections.Generic;


namespace DCMS.Services.Common
{
    /// <summary>
    /// 泛型属性服务接口
    /// </summary>
    public partial interface IGenericAttributeService
    {

        void DeleteAttribute(GenericAttribute attribute);

        GenericAttribute GetAttributeById(int attributeId);

        void InsertAttribute(GenericAttribute attribute);

        void UpdateAttribute(GenericAttribute attribute);


        IList<GenericAttribute> GetAttributesForEntity(int? store, int entityId, string keyGroup);


        void SaveAttribute<TPropType>(BaseEntity entity, string key, TPropType value, int storeId = 0);


        TPropType GetAttribute<TPropType>(BaseEntity entity, string key, int storeId = 0, TPropType defaultValue = default(TPropType));
    }
}