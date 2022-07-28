using DCMS.Web.Framework.Models;
using Newtonsoft.Json;

namespace DCMS.Core.Domain.Sales
{
    /// <summary>
    /// 留存凭证照片
    /// </summary>
    public class RetainPhotoModel : BaseEntityModel
    {
        public string DisplayPath { get; set; }
        public int DeliverySignId { get; set; } = 0;

        [JsonIgnore]
        public virtual DeliverySign DeliverySign { get; set; }
    }
}
