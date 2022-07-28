using DCMS.Core.Domain.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace DCMS.Data.Mapping
{
    public partial class StockEarlyWarningMap : DCMSEntityTypeConfiguration<StockEarlyWarning>
    {
        //public StockEarlyWarningMap()
        //{
        //    ToTable("StockEarlyWarnings");
        //    HasKey(o => o.Id);

        //    HasRequired(sw => sw.WareHouse)
        //    .WithMany(sw => sw.StockEarlyWarnings)
        //    .HasForeignKey(sw => sw.WareHouseId);
        //}

        public override void Configure(EntityTypeBuilder<StockEarlyWarning> builder)
        {
            builder.ToTable("StockEarlyWarnings");
            builder.HasKey(b => b.Id);


            builder.HasOne(sw => sw.WareHouse)
            .WithMany(sw => sw.StockEarlyWarnings)
            .HasForeignKey(sw => sw.WareHouseId).IsRequired();

            base.Configure(builder);
        }
    }
}
