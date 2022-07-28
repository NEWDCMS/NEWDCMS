using DCMS.Core.Domain.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace DCMS.Data.Mapping.BILLs
{

    public partial class PickingMap : DCMSEntityTypeConfiguration<PickingBill>
    {
        public override void Configure(EntityTypeBuilder<PickingBill> builder)
        {
            builder.ToTable("Pickings");
            builder.HasKey(b => b.Id);
            builder.Ignore(c => c.BillType);
            builder.Ignore(c => c.BillTypeId);
            base.Configure(builder);
        }
    }

    public partial class PickingItemMap : DCMSEntityTypeConfiguration<PickingItem>
    {

        //public PickingItemMap()
        //{
        //    ToTable("PickingItems");
        //    HasKey(o => o.Id);
        //}
        public override void Configure(EntityTypeBuilder<PickingItem> builder)
        {
            builder.ToTable("PickingItems");
            builder.HasKey(b => b.Id);
            base.Configure(builder);
        }
    }

}
