using DCMS.Core.Domain.WareHouses;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace DCMS.Data.Mapping.WareHouses
{

    /// <summary>
    /// 用于表示调拨单
    /// </summary>
    public class AllocationBillMap : DCMSEntityTypeConfiguration<AllocationBill>
    {

        public override void Configure(EntityTypeBuilder<AllocationBill> builder)
        {
            builder.ToTable("AllocationBills");
            builder.HasKey(b => b.Id);
            builder.Ignore(c => c.Operations);
            builder.Ignore(c => c.BillType);
            builder.Ignore(c => c.BillTypeId);
            base.Configure(builder);
        }
    }


    /// <summary>
    /// 用于表示调拨单项目
    /// </summary>
    public partial class AllocationItemMap : DCMSEntityTypeConfiguration<AllocationItem>
    {

        //public AllocationItemMap()
        //{
        //    ToTable("AllocationItems");
        //    HasKey(o => o.Id);

        //    HasRequired(sao => sao.AllocationBill)
        //    .WithMany(sa => sa.AllocationItems)
        //    .HasForeignKey(sao => sao.AllocationBillId);
        //}

        public override void Configure(EntityTypeBuilder<AllocationItem> builder)
        {
            builder.ToTable("AllocationItems");
            builder.HasKey(b => b.Id);

            builder.HasOne(sao => sao.AllocationBill)
            .WithMany(sa => sa.Items)
            .HasForeignKey(sao => sao.AllocationBillId).IsRequired();

            base.Configure(builder);
        }
    }


}
