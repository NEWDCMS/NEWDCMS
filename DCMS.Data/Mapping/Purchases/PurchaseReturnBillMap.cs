using DCMS.Core.Domain.Purchases;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace DCMS.Data.Mapping.BILLs
{

    /// <summary>
    /// 采购退货单
    /// </summary>
    public partial class PurchaseReturnBillMap : DCMSEntityTypeConfiguration<PurchaseReturnBill>
    {
        public override void Configure(EntityTypeBuilder<PurchaseReturnBill> builder)
        {
            builder.ToTable("PurchaseReturnBills");
            builder.HasKey(b => b.Id);
            builder.Ignore(c => c.Operations);
            builder.Ignore(c => c.PaymentStatus);
            builder.Ignore(c => c.BillType);
            builder.Ignore(c => c.BillTypeId);
            builder.Ignore(c => c.Deleted);

            base.Configure(builder);
        }
    }

    /// <summary>
    /// 采购退货单明细
    /// </summary>
    public partial class PurchaseReturnItemMap : DCMSEntityTypeConfiguration<PurchaseReturnItem>
    {

        //public PurchaseReturnItemMap()
        //{
        //    ToTable("PurchaseReturnItems");
        //    HasKey(o => o.Id);
        //    HasRequired(o => o.PurchaseReturnBill)
        //        .WithMany(m => m.PurchaseReturnItems)
        //        .HasForeignKey(o => o.PurchaseReturnBillId);
        //}


        public override void Configure(EntityTypeBuilder<PurchaseReturnItem> builder)
        {
            builder.ToTable("PurchaseReturnItems");
            builder.HasKey(b => b.Id);

            builder.HasOne(o => o.PurchaseReturnBill)
                .WithMany(m => m.Items)
                .HasForeignKey(o => o.PurchaseReturnBillId).IsRequired();

            base.Configure(builder);
        }
    }

    /// <summary>
    ///  付款账户（付款单据科目映射表）
    /// </summary>
    public partial class PurchaseReturnBillAccountingMap : DCMSEntityTypeConfiguration<PurchaseReturnBillAccounting>
    {
        //public PurchaseReturnBillAccountingMap()
        //{
        //    ToTable("PurchaseReturnBill_Accounting_Mapping");

        //    HasRequired(o => o.AccountingOption)
        //         .WithMany()
        //         .HasForeignKey(o => o.AccountingOptionId);

        //    HasRequired(o => o.PurchaseReturnBill)
        //        .WithMany(m => m.PurchaseReturnBillAccountings)
        //        .HasForeignKey(o => o.PurchaseReturnBillId);

        //}

        public override void Configure(EntityTypeBuilder<PurchaseReturnBillAccounting> builder)
        {
            builder.ToTable("PurchaseReturnBill_Accounting_Mapping")
               .Property(entity => entity.BillId)
               .HasField("PurchaseReturnBillId");


            builder.HasKey(mapping => new { mapping.BillId, mapping.AccountingOptionId });

            builder.Property(mapping => mapping.BillId).HasColumnName("PurchaseReturnBillId");
            builder.Property(mapping => mapping.AccountingOptionId).HasColumnName("AccountingOptionId");

            builder.HasOne(mapping => mapping.AccountingOption)
                .WithMany()
                .HasForeignKey(mapping => mapping.AccountingOptionId)
                .IsRequired();

            builder.HasOne(mapping => mapping.PurchaseReturnBill)
               .WithMany(customer => customer.PurchaseReturnBillAccountings)
                .HasForeignKey(mapping => mapping.BillId)
                .IsRequired();


            base.Configure(builder);
        }
    }
}
