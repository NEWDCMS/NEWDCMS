using DCMS.Core.Caching;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace DCMS.Core
{
    /// <summary>
    /// 实体基类
    /// </summary>
    public abstract partial class BaseEntity
    {
        /// <summary>
        /// 获取或设置实体标识符
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 经销商Id
        /// </summary>
        public int StoreId { get; set; } = 0;

        /// <summary>
        /// 用于判等比较的重写方法
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as BaseEntity);
        }

        /// <summary>
        /// 快速判断实体ID
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private static bool IsTransient(BaseEntity obj)
        {
            return obj != null && Equals(obj.Id, default(int));
        }

        /// <summary>
        /// 获取当前实例的 System.Type
        /// </summary>
        /// <returns></returns>
        private Type GetUnproxiedType()
        {
            return GetType();
        }

        /// <summary>
        /// 判断实体对象是否相同
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public virtual bool Equals(BaseEntity other)
        {
            if (other == null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            if (!IsTransient(this) &&
                !IsTransient(other) &&
                Equals(Id, other.Id))
            {
                var otherType = other.GetUnproxiedType();
                var thisType = GetUnproxiedType();
                return thisType.IsAssignableFrom(otherType) ||
                        otherType.IsAssignableFrom(thisType);
            }

            return false;
        }

        /// <summary>
        /// 获取哈希码
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            if (Equals(Id, default(int)))
            {
                return base.GetHashCode();
            }

            return Id.GetHashCode();
        }

        /// <summary>
        /// 运算符重载 判等
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static bool operator ==(BaseEntity x, BaseEntity y)
        {
            return Equals(x, y);
        }

        /// <summary>
        /// 运算符重载 不等
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static bool operator !=(BaseEntity x, BaseEntity y)
        {
            return !(x == y);
        }


        /// <summary>
        /// 获取用于缓存实体的KEY 
        /// </summary>
        [JsonIgnore]
        public string EntityCacheKey => GetEntityCacheKey(GetType(), Id);

        /// <summary>
        /// 获取用于缓存实体KEY 
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static string GetEntityCacheKey(Type entityType, object id)
        {
            return string.Format(DCMSCachingDefaults.DCMSEntityCacheKey, entityType.Name, id);
        }
    }


    public abstract class BaseBill : BaseEntity
    {
        /// <summary>
        /// 单据编号
        /// </summary>
        public string BillNumber { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 制单人
        /// </summary>
        public int MakeUserId { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }


        #region 用于逻辑关联（不存储）

        public int? BillTypeId { get; set; } = 0;

        public BillTypeEnum BillType
        {
            get { return (BillTypeEnum)BillTypeId; }
            set { BillTypeId = (int)value; }
        }

        #endregion

        /// <summary>
        /// 生成单号
        /// </summary>
        /// <returns></returns>
        public string GenerateNumber()
        {
            var number = CommonHelper.GetBillNumber(CommonHelper.GetEnumDescription(BillType).Split(',')[1], StoreId);
            BillNumber = number;
            return number;
        }
    }

    /// <summary>
    /// 单据基类
    /// </summary>
    public abstract class BaseBill<T> : BaseBill where T : BaseEntity
    {

        private ICollection<T> _Items;

        /// <summary>
        /// 审核人
        /// </summary>
        public int? AuditedUserId { get; set; } = 0;
        /// <summary>
        /// 状态(审核)
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool AuditedStatus { get; set; }

        /// <summary>
        /// 审核时间
        /// </summary>
        public DateTime? AuditedDate { get; set; }


        /// <summary>
        /// 红冲人
        /// </summary>
        public int? ReversedUserId { get; set; } = 0;

        /// <summary>
        /// 红冲状态
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool ReversedStatus { get; set; }

        /// <summary>
        /// 红冲时间
        /// </summary>
        public DateTime? ReversedDate { get; set; }

        /// <summary>
        /// 删除标志位
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool Deleted { get; set; } = false;


        public bool HasItems()
        {
            return _Items.Count > 0;
        }

        /// <summary>
        /// (导航)项目
        /// </summary>
        [JsonIgnore]
        public virtual ICollection<T> Items
        {
            get { return _Items ?? (_Items = new List<T>()); }
            protected set { _Items = value; }
        }
    }


    public abstract class BaseAccount : BaseEntity
    {

        /// 科目类别
        /// </summary>
        [NotMapped]
        public int AccountingTypeId { get; set; }
        /// <summary>
        /// 财务科目
        /// </summary>
        [NotMapped]
        public string AccountingOptionName { get; set; }


        /// <summary>
        /// 会计科目
        /// </summary>
        public int AccountingOptionId { get; set; }
        /// <summary>
        /// 单据Id
        /// </summary>
        public int BillId { get; set; }
        /// <summary>
        /// 收款金额
        /// </summary>
        public decimal CollectionAmount { get; set; }

    }



    /// <summary>
    /// 单据项目基类
    /// </summary>
    public class BaseItem : BaseEntity
    {

        #region 用于逻辑关联用户（不存储）

        /// <summary>
        /// 提成类型（0：业务提成，1：送货提成）
        /// </summary>
        public int PercentageType { get; set; }
        /// <summary>
        /// 业务员
        /// </summary>
        public int BusinessUserId { get; set; } = 0;
        /// <summary>
        /// 送货员
        /// </summary>
        public int DeliveryUserId { get; set; } = 0;
        /// <summary>
        /// 制单人
        /// </summary>
        public int MakeUserId { get; set; } = 0;

        /// <summary>
        /// 审核人
        /// </summary>
        public int? AuditedUserId { get; set; } = 0;
        /// <summary>
        /// 红冲人
        /// </summary>
        public int? ReversedUserId { get; set; } = 0;
        /// <summary>
        /// 商品名称
        /// </summary>
        public string ProductName { get; set; }
        /// <summary>
        /// 商品类型Id
        /// </summary>
        public int CategoryId { get; set; } = 0;

        #endregion


        /// <summary>
        /// 单位
        /// </summary>
        public int UnitId { get; set; }


        /// <summary>
        /// 商品Id
        /// </summary>
        public int ProductId { get; set; }
        /// <summary>
        /// 数量
        /// </summary>
        public int Quantity { get; set; }
        /// <summary>
        /// 价格
        /// </summary>
        public decimal Price { get; set; }
        /// <summary>
        /// 金额
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// 利润
        /// </summary>
        public decimal Profit { get; set; }

        /// <summary>
        /// 成本价(历史成本价)
        /// </summary>
        public decimal CostPrice { get; set; }


        //(注意：结转成本后，已审核业务单据中的成本价，将会被替换成结转后的全月平均价!)
        /// <summary>
        /// 成本金额
        /// </summary>
        public decimal CostAmount { get; set; }
        /// <summary>
        /// 成本利润率 =利润/成本费用×100%
        /// </summary>
        public decimal CostProfitRate { get; set; }

        /// <summary>
        /// 是否赠品 2019-07-24
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool IsGifts { get; set; } = false;



    }



}




