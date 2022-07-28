using DCMS.Core.Domain.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace DCMS.Data.Mapping.Products
{

    /// <summary>
    /// 商品属性映射
    /// </summary>
    public partial class ProductAttributeMap : DCMSEntityTypeConfiguration<ProductAttribute>
    {
        //public ProductAttributeMap()
        //{
        //    ToTable("ProductAttribute");
        //    HasKey(pa => pa.Id);
        //    Property(pa => pa.Name).IsRequired();
        //}

        public override void Configure(EntityTypeBuilder<ProductAttribute> builder)
        {
            builder.ToTable("ProductAttribute");
            builder.HasKey(b => b.Id);
            builder.Property(b => b.Name).IsRequired();
            base.Configure(builder);
        }
    }
}