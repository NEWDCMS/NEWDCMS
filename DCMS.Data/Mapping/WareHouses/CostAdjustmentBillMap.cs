using DCMS.Core.Domain.WareHouses;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace DCMS.Data.Mapping.WareHouses
{

    /// <summary>
    /// 用于表示成本调价单
    /// </summary>
    public class CostAdjustmentBillMap : DCMSEntityTypeConfiguration<CostAdjustmentBill>
    {
        public override void Configure(EntityTypeBuilder<CostAdjustmentBill> builder)
        {
            builder.ToTable("CostAdjustmentBills");
            builder.HasKey(b => b.Id);
            builder.Ignore(c => c.Operations);
            builder.Ignore(c => c.BillType);
            builder.Ignore(c => c.BillTypeId);
            builder.Ignore(c => c.Deleted);

            base.Configure(builder);
        }
    }


    /// <summary>
    /// 用于表示成本调价单项目
    /// </summary>
    public partial class CostAdjustmentItemMap : DCMSEntityTypeConfiguration<CostAdjustmentItem>
    {

        //public CostAdjustmentItemMap()
        //{
        //    ToTable("CostAdjustmentItems");
        //    HasKey(o => o.Id);

        //    HasRequired(sao => sao.CostAdjustmentBill)
        //    .WithMany(sa => sa.CostAdjustmentItems)
        //    .HasForeignKey(sao => sao.CostAdjustmentBillId);
        //}

        public override void Configure(EntityTypeBuilder<CostAdjustmentItem> builder)
        {
            builder.ToTable("CostAdjustmentItems");
            builder.HasKey(b => b.Id);

            builder.HasOne(sao => sao.CostAdjustmentBill)
            .WithMany(sa => sa.Items)
            .HasForeignKey(sao => sao.CostAdjustmentBillId).IsRequired();

            base.Configure(builder);
        }
    }


}
