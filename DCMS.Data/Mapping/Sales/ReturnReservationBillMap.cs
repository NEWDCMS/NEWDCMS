using DCMS.Core.Domain.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace DCMS.Data.Mapping.BILLs
{
    /// <summary>
    /// 退货订单
    /// </summary>
    public partial class ReturnReservationBillMap : DCMSEntityTypeConfiguration<ReturnReservationBill>
    {

        public override void Configure(EntityTypeBuilder<ReturnReservationBill> builder)
        {
            builder.ToTable("ReturnReservationBills");
            builder.HasKey(b => b.Id);

            builder.Ignore(c => c.Operations);
            //builder.Ignore(c => c.TaxAmount);
            builder.Ignore(c => c.BillType);
            builder.Ignore(c => c.BillTypeId);

            base.Configure(builder);
        }
    }

    /// <summary>
    /// 退货订单明细
    /// </summary>
    public partial class ReturnReservationItemMap : DCMSEntityTypeConfiguration<ReturnReservationItem>
    {

        //public ReturnReservationItemMap()
        //{
        //    ToTable("ReturnReservationItems");
        //    HasKey(o => o.Id);

        //    HasRequired(o => o.ReturnReservationBill)
        //        .WithMany(m => m.ReturnReservationItems)
        //        .HasForeignKey(o => o.ReturnReservationBillId);

        //}
        public override void Configure(EntityTypeBuilder<ReturnReservationItem> builder)
        {
            builder.ToTable("ReturnReservationItems");
            builder.HasKey(b => b.Id);

            builder.HasOne(o => o.ReturnReservationBill)
                .WithMany(m => m.Items)
                .HasForeignKey(o => o.ReturnReservationBillId).IsRequired();

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
    ///  收款账户（退货订单据科目映射表）
    /// </summary>
    public partial class ReturnReservationBillAccountingMap : DCMSEntityTypeConfiguration<ReturnReservationBillAccounting>
    {
        //public ReturnReservationBillAccountingMap()
        //{
        //    ToTable("ReturnReservationBill_Accounting_Mapping");

        //    HasRequired(o => o.AccountingOption)
        //         .WithMany()
        //         .HasForeignKey(o => o.AccountingOptionId);

        //    HasRequired(o => o.ReturnReservationBill)
        //        .WithMany(m => m.ReturnReservationBillAccountings)
        //        .HasForeignKey(o => o.ReturnReservationBillId);

        //}

        public override void Configure(EntityTypeBuilder<ReturnReservationBillAccounting> builder)
        {
            builder.ToTable("ReturnReservationBill_Accounting_Mapping")
               .Property(entity => entity.BillId)
               .HasField("ReturnReservationBillId");


            builder.HasKey(mapping => new { mapping.BillId, mapping.AccountingOptionId });

            builder.Property(mapping => mapping.BillId).HasColumnName("ReturnReservationBillId");
            builder.Property(mapping => mapping.AccountingOptionId).HasColumnName("AccountingOptionId");

            builder.HasOne(mapping => mapping.AccountingOption)
                .WithMany()
                .HasForeignKey(mapping => mapping.AccountingOptionId)
                .IsRequired();

            builder.HasOne(mapping => mapping.ReturnReservationBill)
               .WithMany(customer => customer.ReturnReservationBillAccountings)
                .HasForeignKey(mapping => mapping.BillId)
                .IsRequired();



            base.Configure(builder);
        }
    }

}
