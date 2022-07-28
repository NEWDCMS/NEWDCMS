namespace DCMS.Core.Domain.Sales
{
    /// <summary>
    /// 留存凭证照片
    /// </summary>
    public class RetainPhoto : BaseEntity
    {
        public string DisplayPath { get; set; }
        public int DeliverySignId { get; set; }
        public virtual DeliverySign DeliverySign { get; set; }

    }
}
