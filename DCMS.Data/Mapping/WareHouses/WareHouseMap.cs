using DCMS.Core.Domain.WareHouses;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DCMS.Data.Mapping.WareHouses
{
    public class WareHouseMap : DCMSEntityTypeConfiguration<WareHouse>
    {
        public override void Configure(EntityTypeBuilder<WareHouse> builder)
        {
            builder.ToTable("WareHouses");
            builder.HasKey(b => b.Id);
            builder.Ignore(b => b.WareHouseAccess);
            base.Configure(builder);
        }

    }

}
