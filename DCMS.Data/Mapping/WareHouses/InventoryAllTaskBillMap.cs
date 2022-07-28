using DCMS.Core.Domain.WareHouses;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace DCMS.Data.Mapping.WareHouses
{

    /// <summary>
    /// 用于表示盘点任务(整仓)
    /// </summary>
    public class InventoryAllTaskBillMap : DCMSEntityTypeConfiguration<InventoryAllTaskBill>
    {
        public override void Configure(EntityTypeBuilder<InventoryAllTaskBill> builder)
        {
            builder.ToTable("InventoryAllTaskBills");
            builder.HasKey(b => b.Id);
            builder.Ignore(c => c.Operations);
            builder.Ignore(c => c.BillType);
            builder.Ignore(c => c.BillTypeId);
            builder.Ignore(c => c.Remark);
            builder.Ignore(c => c.Deleted);

            base.Configure(builder);
        }
    }


    /// <summary>
    /// 用于表示盘点任务(整仓)项目
    /// </summary>
    public partial class InventoryAllTaskItemMap : DCMSEntityTypeConfiguration<InventoryAllTaskItem>
    {

        //public InventoryAllTaskItemMap()
        //{
        //    ToTable("InventoryAllTaskItems");
        //    HasKey(o => o.Id);

        //    HasRequired(sao => sao.InventoryAllTaskBill)
        //    .WithMany(sa => sa.InventoryAllTaskItems)
        //    .HasForeignKey(sao => sao.InventoryAllTaskBillId);
        //}

        public override void Configure(EntityTypeBuilder<InventoryAllTaskItem> builder)
        {
            builder.ToTable("InventoryAllTaskItems");
            builder.HasKey(b => b.Id);

            builder.HasOne(sao => sao.InventoryAllTaskBill)
            .WithMany(sa => sa.Items)
            .HasForeignKey(sao => sao.InventoryAllTaskBillId).IsRequired();

            base.Configure(builder);
        }
    }


}
