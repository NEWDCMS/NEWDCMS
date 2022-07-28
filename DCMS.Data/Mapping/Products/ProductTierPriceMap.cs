using DCMS.Core.Domain.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace DCMS.Data.Mapping
{

    public partial class ProductTierPriceMap : DCMSEntityTypeConfiguration<ProductTierPrice>
    {
        //public ProductTierPriceMap()
        //{
        //    ToTable("ProductTierPrices");
        //    HasKey(o => o.Id);

        //    Property(tp => tp.SmallUnitPrice).HasPrecision(18, 4);
        //    Property(tp => tp.StrokeUnitPrice).HasPrecision(18, 4);
        //    Property(tp => tp.BigUnitPrice).HasPrecision(18, 4);

        //    Ignore(c => c.PriceType);

        //    HasRequired(tp => tp.Product)
        //        .WithMany(p => p.ProductTierPrices)
        //        .HasForeignKey(tp => tp.ProductId);

        //}

        public override void Configure(EntityTypeBuilder<ProductTierPrice> builder)
        {
            builder.ToTable("ProductTierPrices");
            builder.HasKey(b => b.Id);

            builder.Property(tp => tp.SmallUnitPrice).HasColumnType("decimal(18,4)");
            builder.Property(tp => tp.StrokeUnitPrice).HasColumnType("decimal(18,4)");
            builder.Property(tp => tp.BigUnitPrice).HasColumnType("decimal(18,4)");

            builder.Ignore(c => c.PriceType);

            builder.HasOne(tp => tp.Product)
                .WithMany(p => p.ProductTierPrices)
                .HasForeignKey(tp => tp.ProductId).IsRequired();

            base.Configure(builder);
        }
    }




}

