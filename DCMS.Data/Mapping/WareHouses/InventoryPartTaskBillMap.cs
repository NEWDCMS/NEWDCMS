using DCMS.Core.Domain.WareHouses;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace DCMS.Data.Mapping.WareHouses
{

    /// <summary>
    /// 用于表示盘点任务(部分)
    /// </summary>
    public class InventoryPartTaskBillMap : DCMSEntityTypeConfiguration<InventoryPartTaskBill>
    {
        public override void Configure(EntityTypeBuilder<InventoryPartTaskBill> builder)
        {
            builder.ToTable("InventoryPartTaskBills");
            builder.HasKey(b => b.Id);
            builder.Ignore(c => c.Operations);
            builder.Ignore(c => c.BillType);
            builder.Ignore(c => c.BillTypeId);
            builder.Ignore(c => c.Remark);

            base.Configure(builder);
        }
    }


    /// <summary>
    /// 用于表示盘点任务(部分)项目
    /// </summary>
    public partial class InventoryPartTaskItemMap : DCMSEntityTypeConfiguration<InventoryPartTaskItem>
    {

        //public InventoryPartTaskItemMap()
        //{
        //    ToTable("InventoryPartTaskItems");
        //    HasKey(o => o.Id);

        //    HasRequired(sao => sao.InventoryPartTaskBill)
        //    .WithMany(sa => sa.InventoryPartTaskItems)
        //    .HasForeignKey(sao => sao.InventoryPartTaskBillId);
        //}

        public override void Configure(EntityTypeBuilder<InventoryPartTaskItem> builder)
        {
            builder.ToTable("InventoryPartTaskItems");
            builder.HasKey(b => b.Id);

            builder.HasOne(sao => sao.InventoryPartTaskBill)
            .WithMany(sa => sa.Items)
            .HasForeignKey(sao => sao.InventoryPartTaskBillId).IsRequired();
            base.Configure(builder);
        }
    }


}
