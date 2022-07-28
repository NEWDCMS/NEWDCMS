using DCMS.Core.Domain.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace DCMS.Data.Mapping.BILLs
{

    /// <summary>
    /// 收款对账单
    /// </summary>
    public partial class FinanceReceiveAccountBillMap : DCMSEntityTypeConfiguration<FinanceReceiveAccountBill>
    {
        public override void Configure(EntityTypeBuilder<FinanceReceiveAccountBill> builder)
        {
            builder.ToTable("FinanceReceiveAccountBills");
            builder.HasKey(b => b.Id);

            builder.Ignore(c => c.FinanceReceiveAccountStatus);
            builder.Ignore(c => c.BillType);
            builder.Ignore(c => c.Accounts);
            
            base.Configure(builder);
        }
    }

    /// <summary>
    ///  收款账户（收款单据科目映射表）
    /// </summary>
    public partial class FinanceReceiveAccountBillAccountingMap : DCMSEntityTypeConfiguration<FinanceReceiveAccountBillAccounting>
    {
        public override void Configure(EntityTypeBuilder<FinanceReceiveAccountBillAccounting> builder)
        {

            builder.ToTable("FinanceReceiveAccountBill_Accounting_Mapping")
                .Property(entity => entity.BillId)
                .HasField("FinanceReceiveAccountBillId");

            builder.HasKey(mapping => new { mapping.BillId, mapping.AccountingOptionId });

            builder.Property(mapping => mapping.BillId).HasColumnName("FinanceReceiveAccountBillId");

            builder.Property(mapping => mapping.AccountingOptionId).HasColumnName("AccountingOptionId");

            builder.HasOne(mapping => mapping.AccountingOption)
                .WithMany()
                .HasForeignKey(mapping => mapping.AccountingOptionId)
                .IsRequired();

            builder.HasOne(mapping => mapping.FinanceReceiveAccountBill)
               .WithMany(customer => customer.FinanceReceiveAccountBillAccountings)
                .HasForeignKey(mapping => mapping.BillId)
                .IsRequired();



            base.Configure(builder);
        }
    }
}
