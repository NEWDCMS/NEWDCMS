using DCMS.Core.Domain.Finances;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace DCMS.Data.Mapping.Finances
{

    /// <summary>
    /// 用于表示预收款单据
    /// </summary>
    public partial class AdvanceReceiptBillMap : DCMSEntityTypeConfiguration<AdvanceReceiptBill>
    {
        public override void Configure(EntityTypeBuilder<AdvanceReceiptBill> builder)
        {
            builder.ToTable("AdvanceReceiptBills");
            builder.HasKey(b => b.Id);
            builder.Ignore(c => c.Operations);
            builder.Ignore(c => c.BillType);
            builder.Ignore(c => c.BillTypeId);
            builder.Ignore(c => c.ReceivedStatus);

            base.Configure(builder);
        }
    }

    /// <summary>
    ///  预收款账户（收款单据科目映射表）
    /// </summary>
    public partial class AdvanceReceiptBillAccountingMap : DCMSEntityTypeConfiguration<AdvanceReceiptBillAccounting>
    {

        public override void Configure(EntityTypeBuilder<AdvanceReceiptBillAccounting> builder)
        {
            builder.ToTable("AdvanceReceiptBill_Accounting_Mapping")
               .Property(entity => entity.BillId)
               .HasField("AdvanceReceiptBillId");
     

            builder.HasKey(mapping => new { mapping.BillId, mapping.AccountingOptionId });

            builder.Property(mapping => mapping.BillId).HasColumnName("AdvanceReceiptBillId");
            builder.Property(mapping => mapping.AccountingOptionId).HasColumnName("AccountingOptionId");

            builder.HasOne(mapping => mapping.AccountingOption)
                .WithMany()
                .HasForeignKey(mapping => mapping.AccountingOptionId)
                .IsRequired();

            builder.HasOne(mapping => mapping.AdvanceReceiptBill)
               .WithMany(customer => customer.Items)
                .HasForeignKey(mapping => mapping.BillId)
                .IsRequired();



            base.Configure(builder);
        }
    }

}
