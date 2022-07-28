using DCMS.Core.Domain.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace DCMS.Data.Mapping.Products
{
    /// <summary>
    /// 商品变体组合映射
    /// </summary>
    public partial class ProductVariantAttributeCombinationMap : DCMSEntityTypeConfiguration<ProductVariantAttributeCombination>
    {
        //public ProductVariantAttributeCombinationMap()
        //{
        //    ToTable("ProductVariantAttributeCombination");
        //    HasKey(pvac => pvac.Id);

        //    Property(pvac => pvac.Sku).HasMaxLength(400);
        //    Property(pvac => pvac.ManufacturerPartNumber).HasMaxLength(400);
        //    Property(pvac => pvac.Gtin).HasMaxLength(400);

        //    HasRequired(pvac => pvac.Product)
        //        .WithMany(p => p.ProductVariantAttributeCombinations)
        //        .HasForeignKey(pvac => pvac.ProductId);
        //}


        public override void Configure(EntityTypeBuilder<ProductVariantAttributeCombination> builder)
        {
            builder.ToTable("ProductVariantAttributeCombination");
            builder.HasKey(b => b.Id);


            builder.Property(pvac => pvac.Sku).HasMaxLength(400);
            builder.Property(pvac => pvac.ManufacturerPartNumber).HasMaxLength(400);
            builder.Property(pvac => pvac.Gtin).HasMaxLength(400);

            builder.HasOne(pvac => pvac.Product)
                .WithMany(p => p.ProductVariantAttributeCombinations)
                .HasForeignKey(pvac => pvac.ProductId)
                .IsRequired();

            base.Configure(builder);
        }
    }
}