using DCMS.Core.Domain.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DCMS.Data.Mapping.BILLs
{

    public partial class DispatchBillMap : DCMSEntityTypeConfiguration<DispatchBill>
    {

        public override void Configure(EntityTypeBuilder<DispatchBill> builder)
        {
            builder.ToTable("DispatchBills");
            builder.HasKey(b => b.Id);
            builder.Ignore(c => c.Operations);
            builder.Ignore(c => c.BillType);
            builder.Ignore(c => c.BillTypeId);
            builder.Ignore(c => c.Remark);
            builder.Ignore(c => c.Deleted);

            base.Configure(builder);
        }

    }

    public partial class DispatchItemMap : DCMSEntityTypeConfiguration<DispatchItem>
    {

        //public DispatchItemMap()
        //{
        //    ToTable("DispatchItems");
        //    HasKey(o => o.Id);

        //    HasRequired(o => o.DispatchBill)
        //        .WithMany(m => m.DispatchItems)
        //        .HasForeignKey(o => o.DispatchBillId);

        //}

        public override void Configure(EntityTypeBuilder<DispatchItem> builder)
        {
            builder.ToTable("DispatchItems");
            builder.HasKey(b => b.Id);

            builder.HasOne(o => o.DispatchBill)
                .WithMany(m => m.Items)
                .HasForeignKey(o => o.DispatchBillId).IsRequired();

            base.Configure(builder);
        }

    }

}
