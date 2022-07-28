using DCMS.Core.Domain.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DCMS.Data.Mapping
{

    public partial class CombinationMap : DCMSEntityTypeConfiguration<Combination>
    {
        //public CombinationMap()
        //{
        //    ToTable("Combinations");
        //    HasKey(o => o.Id);

        //    HasRequired(tp => tp.Product)
        //        .WithMany()
        //        .HasForeignKey(tp => tp.ProductId);

        //}
        public override void Configure(EntityTypeBuilder<Combination> builder)
        {
            builder.ToTable("Combinations");
            builder.HasKey(b => b.Id);

            builder.HasOne(tp => tp.Product)
                .WithMany()
                .HasForeignKey(tp => tp.ProductId);

            base.Configure(builder);
        }
    }

    public partial class ProductCombinationMap : DCMSEntityTypeConfiguration<ProductCombination>
    {
        //public ProductCombinationMap()
        //{
        //    ToTable("ProductCombinations");
        //    HasKey(o => o.Id);

        //    HasRequired(tp => tp.Combination)
        //        .WithMany(p => p.ProductCombinations)
        //        .HasForeignKey(tp => tp.CombinationId);

        //    HasRequired(tp => tp.Product)
        //        .WithMany()
        //        .HasForeignKey(tp => tp.ProductId);
        //}

        public override void Configure(EntityTypeBuilder<ProductCombination> builder)
        {
            builder.ToTable("ProductCombinations");
            builder.HasKey(b => b.Id);

            builder.HasOne(tp => tp.Combination)
                .WithMany(p => p.ProductCombinations)
                .HasForeignKey(tp => tp.CombinationId);

            builder.HasOne(tp => tp.Product)
                .WithMany()
                .HasForeignKey(tp => tp.ProductId);

            base.Configure(builder);
        }
    }


}
