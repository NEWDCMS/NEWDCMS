using DCMS.Core.Domain.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace DCMS.Data.Mapping
{

    /// <summary>
    ///  最近售价
    /// </summary>
    public partial class RecentPriceMap : DCMSEntityTypeConfiguration<RecentPrice>
    {
        //public RecentPriceMap()
        //{
        //    ToTable("RecentPrices");
        //    HasKey(o => o.Id);

        //    HasRequired(p => p.Product)
        //        .WithMany()
        //        .HasForeignKey(p => p.ProductId);
        //}


        public override void Configure(EntityTypeBuilder<RecentPrice> builder)
        {
            builder.ToTable("RecentPrices");
            builder.HasKey(b => b.Id);

            builder.HasOne(p => p.Product)
                .WithMany()
                .HasForeignKey(p => p.ProductId)
                .IsRequired();

            base.Configure(builder);
        }
    }


}
