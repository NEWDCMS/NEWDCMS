using DCMS.Core.Domain.Purchases;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace DCMS.Data.Mapping.BILLs
{

    /// <summary>
    /// 采购单
    /// </summary>
    public partial class PurchaseBillMap : DCMSEntityTypeConfiguration<PurchaseBill>
    {

        public override void Configure(EntityTypeBuilder<PurchaseBill> builder)
        {
            builder.ToTable("PurchaseBills");
            builder.HasKey(b => b.Id);
            builder.Ignore(c => c.Operations);
            builder.Ignore(c => c.PaymentStatus);
            builder.Ignore(c => c.BillType);
            builder.Ignore(c => c.BillTypeId);
            base.Configure(builder);

        }
    }

    /// <summary>
    /// 采购单明细
    /// </summary>
    public partial class PurchaseItemMap : DCMSEntityTypeConfiguration<PurchaseItem>
    {
        public override void Configure(EntityTypeBuilder<PurchaseItem> builder)
        {
            builder.ToTable("PurchaseItems");
            builder.HasKey(b => b.Id);
            builder.HasOne(o => o.PurchaseBill)
                .WithMany(m => m.Items)
                .HasForeignKey(o => o.PurchaseBillId).IsRequired();

            builder.Ignore(c => c.BusinessUserId);
            builder.Ignore(c => c.DeliveryUserId);
            builder.Ignore(c => c.MakeUserId);
            builder.Ignore(c => c.AuditedUserId);
            builder.Ignore(c => c.ReversedUserId);
            builder.Ignore(c => c.ProductName);
            builder.Ignore(c => c.CategoryId);
            builder.Ignore(c => c.PercentageType);

            builder.Ignore(c => c.Profit);
            builder.Ignore(c => c.CostProfitRate);
            builder.Ignore(c => c.IsGifts);


            base.Configure(builder);
        }
    }

    /// <summary>
    ///  付款账户（付款单据科目映射表）
    /// </summary>
    public partial class PurchaseBillAccountingMap : DCMSEntityTypeConfiguration<PurchaseBillAccounting>
    {
        public override void Configure(EntityTypeBuilder<PurchaseBillAccounting> builder)
        {
            builder.ToTable("PurchaseBill_Accounting_Mapping")
               .Property(entity => entity.BillId)
               .HasField("PurchaseBillId");
    

            builder.HasKey(mapping => new { mapping.BillId, mapping.AccountingOptionId });

            builder.Property(mapping => mapping.BillId).HasColumnName("PurchaseBillId");
            builder.Property(mapping => mapping.AccountingOptionId).HasColumnName("AccountingOptionId");

            builder.HasOne(mapping => mapping.AccountingOption)
                .WithMany()
                .HasForeignKey(mapping => mapping.AccountingOptionId)
                .IsRequired();

            builder.HasOne(mapping => mapping.PurchaseBill)
               .WithMany(customer => customer.PurchaseBillAccountings)
                .HasForeignKey(mapping => mapping.BillId)
                .IsRequired();



            base.Configure(builder);
        }
    }
}
