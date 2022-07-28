using DCMS.Core.Domain.Finances;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace DCMS.Data.Mapping.Finances
{

    /// <summary>
    /// 用于表示费用支出单据
    /// </summary>
    public partial class CostExpenditureBillMap : DCMSEntityTypeConfiguration<CostExpenditureBill>
    {

        public override void Configure(EntityTypeBuilder<CostExpenditureBill> builder)
        {
            builder.ToTable("CostExpenditureBills");
            builder.HasKey(b => b.Id);
            builder.Ignore(c => c.Operations);
            builder.Ignore(c => c.BillType);
            builder.Ignore(c => c.BillTypeId);
            builder.Ignore(c => c.ReceivedStatus);
            base.Configure(builder);
        }
    }


    /// <summary>
    /// 费用支出单据项目
    /// </summary>
    public partial class CostExpenditureItemMap : DCMSEntityTypeConfiguration<CostExpenditureItem>
    {

        //public CostExpenditureItemMap()
        //{
        //    ToTable("CostExpenditureItems");
        //    HasKey(o => o.Id);

        //    HasRequired(sao => sao.CostExpenditureBill)
        //    .WithMany(sa => sa.CostExpenditureItems)
        //    .HasForeignKey(sao => sao.CostExpenditureBillId);
        //}

        public override void Configure(EntityTypeBuilder<CostExpenditureItem> builder)
        {
            builder.ToTable("CostExpenditureItems");
            builder.HasKey(b => b.Id);

            builder.HasOne(sao => sao.CostExpenditureBill)
            .WithMany(sa => sa.Items)
            .HasForeignKey(sao => sao.CostExpenditureBillId).IsRequired();

            base.Configure(builder);
        }
    }


    /// <summary>
    ///  费用支出账户（收款单据科目映射表）
    /// </summary>
    public partial class CostExpenditureBillAccountingMap : DCMSEntityTypeConfiguration<CostExpenditureBillAccounting>
    {
        //public CostExpenditureBillAccountingMap()
        //{
        //    ToTable("CostExpenditureBill_Accounting_Mapping");

        //    HasRequired(o => o.AccountingOption)
        //         .WithMany()
        //         .HasForeignKey(o => o.AccountingOptionId);

        //    HasRequired(o => o.CostExpenditureBill)
        //        .WithMany(m => m.CostExpenditureBillAccountings)
        //        .HasForeignKey(o => o.CostExpenditureBillId);

        //}

        public override void Configure(EntityTypeBuilder<CostExpenditureBillAccounting> builder)
        {
            builder.ToTable("CostExpenditureBill_Accounting_Mapping")
               .Property(entity => entity.BillId)
               .HasField("CostExpenditureBillId");
         

            builder.HasKey(mapping => new { mapping.BillId, mapping.AccountingOptionId });

            builder.Property(mapping => mapping.BillId).HasColumnName("CostExpenditureBillId");
            builder.Property(mapping => mapping.AccountingOptionId).HasColumnName("AccountingOptionId");

            builder.HasOne(mapping => mapping.AccountingOption)
                .WithMany()
                .HasForeignKey(mapping => mapping.AccountingOptionId)
                .IsRequired();

            builder.HasOne(mapping => mapping.CostExpenditureBill)
               .WithMany(customer => customer.CostExpenditureBillAccountings)
                .HasForeignKey(mapping => mapping.BillId)
                .IsRequired();



            base.Configure(builder);
        }

    }

}
