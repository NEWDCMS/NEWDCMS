using DCMS.Core.Domain.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace DCMS.Data.Mapping.BILLs
{
    /// <summary>
    /// 销售订单
    /// </summary>
    public partial class SaleReservationBillMap : DCMSEntityTypeConfiguration<SaleReservationBill>
    {

        public override void Configure(EntityTypeBuilder<SaleReservationBill> builder)
        {
            builder.ToTable("SaleReservationBills");
            builder.HasKey(b => b.Id);

            builder.Ignore(o => o.Operations);
            //builder.Ignore(c => c.TaxAmount);
            builder.Ignore(c => c.BillType);
            builder.Ignore(c => c.BillTypeId);

            base.Configure(builder);
        }
    }

    /// <summary>
    /// 销售订单明细
    /// </summary>
    public partial class SaleReservationItemMap : DCMSEntityTypeConfiguration<SaleReservationItem>
    {
        public override void Configure(EntityTypeBuilder<SaleReservationItem> builder)
        {
            builder.ToTable("SaleReservationItems");
            builder.HasKey(b => b.Id);

            builder.HasOne(o => o.SaleReservationBill)
                .WithMany(m => m.Items)
                .HasForeignKey(o => o.SaleReservationBillId).IsRequired();

            builder.Ignore(c => c.BusinessUserId);
            builder.Ignore(c => c.DeliveryUserId);
            builder.Ignore(c => c.MakeUserId);
            builder.Ignore(c => c.AuditedUserId);
            builder.Ignore(c => c.ReversedUserId);
            builder.Ignore(c => c.ProductName);
            builder.Ignore(c => c.CategoryId);
            builder.Ignore(c => c.PercentageType);

            base.Configure(builder);
        }
    }

    /// <summary>
    ///  收款账户（收款单据科目映射表）
    /// </summary>
    public partial class SaleReservationBillAccountingMap : DCMSEntityTypeConfiguration<SaleReservationBillAccounting>
    {
        //public SaleReservationBillAccountingMap()
        //{
        //    ToTable("SaleReservationBill_Accounting_Mapping");

        //    HasRequired(o => o.AccountingOption)
        //         .WithMany()
        //         .HasForeignKey(o => o.AccountingOptionId);

        //    HasRequired(o => o.SaleReservationBill)
        //        .WithMany(m => m.SaleReservationBillAccountings)
        //        .HasForeignKey(o => o.SaleReservationBillId);

        //}

        public override void Configure(EntityTypeBuilder<SaleReservationBillAccounting> builder)
        {
            builder.ToTable("SaleReservationBill_Accounting_Mapping")
               .Property(entity => entity.BillId)
               .HasField("SaleReservationBillId");

            builder.HasKey(mapping => new { mapping.BillId, mapping.AccountingOptionId });

            builder.Property(mapping => mapping.BillId).HasColumnName("SaleReservationBillId");
            builder.Property(mapping => mapping.AccountingOptionId).HasColumnName("AccountingOptionId");

            builder.HasOne(mapping => mapping.AccountingOption)
                .WithMany()
                .HasForeignKey(mapping => mapping.AccountingOptionId)
                .IsRequired();

            builder.HasOne(mapping => mapping.SaleReservationBill)
               .WithMany(customer => customer.SaleReservationBillAccountings)
                .HasForeignKey(mapping => mapping.BillId)
                .IsRequired();



            base.Configure(builder);
        }
    }

}
