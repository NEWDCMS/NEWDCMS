using DCMS.Core.Domain.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace DCMS.Data.Mapping.Products
{
    /// <summary>
    /// 商品变体属性映射
    /// </summary>
    public partial class ProductVariantAttributeValueMap : DCMSEntityTypeConfiguration<ProductVariantAttributeValue>
    {
        //public ProductVariantAttributeValueMap()
        //{
        //    ToTable("ProductVariantAttributeValue");
        //    HasKey(pvav => pvav.Id);
        //    Property(pvav => pvav.Name).IsRequired().HasMaxLength(400);
        //    Property(pvav => pvav.ColorSquaresRgb).HasMaxLength(100);

        //    Property(pvav => pvav.PriceAdjustment).HasPrecision(18, 4);
        //    Property(pvav => pvav.WeightAdjustment).HasPrecision(18, 4);

        //    Ignore(pvav => pvav.AttributeValueType);

        //    HasRequired(pvav => pvav.ProductVariantAttribute)
        //        .WithMany(pva => pva.ProductVariantAttributeValues)
        //        .HasForeignKey(pvav => pvav.ProductVariantAttributeId);
        //}

        /*
The relationship from 'ProductVariantAttributeValue.ProductVariantAttribute' to 'ProductVariantAttribute.ProductVariantAttributeValues' with foreign key properties {'ProductVariantAttributeId' : int} cannot target the primary key {'ProductId' : int, 'ProductAttributeId' : int} because it is not compatible. Configure a principal key or a set of compatible foreign key properties for this relationship
*/

        public override void Configure(EntityTypeBuilder<ProductVariantAttributeValue> builder)
        {
            builder.ToTable("ProductVariantAttributeValue");
            builder.HasKey(b => b.Id);


            builder.Property(pvav => pvav.Name).IsRequired().HasMaxLength(400);
            builder.Property(pvav => pvav.ColorSquaresRgb).HasMaxLength(100);

            builder.Property(pvav => pvav.PriceAdjustment).HasColumnType("decimal(18,4)");
            builder.Property(pvav => pvav.WeightAdjustment).HasColumnType("decimal(18,4)");

            builder.Ignore(pvav => pvav.AttributeValueType);

            builder.HasOne(pvav => pvav.ProductVariantAttribute)
                .WithMany(pva => pva.ProductVariantAttributeValues)
                .HasForeignKey(pvav => pvav.ProductVariantAttributeId)
                .IsRequired();

            //builder.HasOne(value => value.ProductAttribute)
            //.WithMany()
            //.HasForeignKey(value => value.ProductAttributeId)
            //.IsRequired();

            base.Configure(builder);
        }
    }
}