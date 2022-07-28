using DCMS.Core.Domain.Finances;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace DCMS.Data.Mapping.Finances
{

    /// <summary>
    /// 收款单据
    /// </summary>
    public partial class CashReceiptBillMap : DCMSEntityTypeConfiguration<CashReceiptBill>
    {
        public override void Configure(EntityTypeBuilder<CashReceiptBill> builder)
        {
            builder.ToTable("CashReceiptBills");
            builder.HasKey(b => b.Id);
            builder.Ignore(c => c.Operations);
            builder.Ignore(c => c.BillType);
            builder.Ignore(c => c.BillTypeId);
            builder.Ignore(c => c.Remark);
            base.Configure(builder);
        }
    }


    /// <summary>
    /// 收款单据项目
    /// </summary>
    public partial class CashReceiptItemMap : DCMSEntityTypeConfiguration<CashReceiptItem>
    {

        //public CashReceiptItemMap()
        //{
        //    ToTable("CashReceiptItems");
        //    HasKey(o => o.Id);

        //    Ignore(c => c.BillTypeEnum);

        //    HasRequired(sao => sao.CashReceiptBill)
        //    .WithMany(sa => sa.CashReceiptItems)
        //    .HasForeignKey(sao => sao.CashReceiptBillId);
        //}

        public override void Configure(EntityTypeBuilder<CashReceiptItem> builder)
        {
            builder.ToTable("CashReceiptItems");
            builder.HasKey(b => b.Id);
            builder.Ignore(c => c.BillTypeEnum);

            builder.HasOne(sao => sao.CashReceiptBill)
            .WithMany(sa => sa.Items)
            .HasForeignKey(sao => sao.CashReceiptBillId).IsRequired();

            base.Configure(builder);
        }
    }


    /// <summary>
    ///  收款账户（收款单据科目映射表）
    /// </summary>
    public partial class CashReceiptBillAccountingMap : DCMSEntityTypeConfiguration<CashReceiptBillAccounting>
    {
        //public CashReceiptBillAccountingMap()
        //{
        //    ToTable("CashReceiptBill_Accounting_Mapping");

        //    HasRequired(o => o.AccountingOption)
        //         .WithMany()
        //         .HasForeignKey(o => o.AccountingOptionId);

        //    HasRequired(o => o.CashReceiptBill)
        //        .WithMany(m => m.CashReceiptBillAccountings)
        //        .HasForeignKey(o => o.CashReceiptBillId);

        //}

        public override void Configure(EntityTypeBuilder<CashReceiptBillAccounting> builder)
        {
            //CashReceiptBill_Accounting_Mapping
            // :“No field was found backing property 'CashReceiptBillAccountings' of entity type 'CashReceiptBill'.Lazy - loaded navigation properties must have backing fields. Either name the backing field so that it is picked up by convention or configure the backing field to use.”

            builder.ToTable("CashReceiptBill_Accounting_Mapping")
               .Property(entity => entity.BillId)
               .HasField("CashReceiptBillId");

            builder.Property(p => p.BillId).HasField("CashReceiptBillId");

            builder.HasKey(mapping => new { mapping.BillId, mapping.AccountingOptionId });

            builder.Property(mapping => mapping.BillId).HasColumnName("CashReceiptBillId");
            builder.Property(mapping => mapping.AccountingOptionId).HasColumnName("AccountingOptionId");

            builder.HasOne(mapping => mapping.AccountingOption)
                .WithMany()
                .HasForeignKey(mapping => mapping.AccountingOptionId)
                .IsRequired();

            builder.HasOne(mapping => mapping.CashReceiptBill)
               .WithMany(customer => customer.CashReceiptBillAccountings)
               .HasForeignKey(mapping => mapping.BillId)
               .IsRequired();



            base.Configure(builder);
        }
    }

}
