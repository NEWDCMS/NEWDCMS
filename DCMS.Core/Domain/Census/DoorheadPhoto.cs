using System;

namespace DCMS.Core.Domain.Census
{
    /// <summary>
    /// 门头照片
    /// </summary>
    public class DoorheadPhoto : BaseEntity
    {
        public string StoragePath { get; set; }
        public int TraditionId { get; set; }
        public virtual Tradition Tradition { get; set; }
        public int RestaurantId { get; set; }
        public virtual Restaurant Restaurant { get; set; }
        public int VisitStoreId { get; set; }
        public DateTime UpdateDate { get; set; }
    }

}
