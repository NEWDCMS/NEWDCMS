using DCMS.Core.Domain.Media;
using Newtonsoft.Json;


namespace DCMS.Core.Domain.Products
{

    /// <summary>
    /// 用于表示商品图片
    /// </summary>
    public partial class ProductPicture : BaseEntity
    {

        public int ProductId { get; set; } = 0;

        public int PictureId { get; set; } = 0;


        public int DisplayOrder { get; set; } = 0;

        [JsonIgnore]
        public virtual Picture Picture { get; set; }
        [JsonIgnore]
        public virtual Product Product { get; set; }
    }

}
