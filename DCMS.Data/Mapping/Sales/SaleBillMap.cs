using DCMS.Core.Domain.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace DCMS.Data.Mapping.BILLs
{

    /// <summary>
    /// 销售单
    /// </summary>
    public partial class SaleBillMap : DCMSEntityTypeConfiguration<SaleBill>
    {

        public override void Configure(EntityTypeBuilder<SaleBill> builder)
        {
            builder.ToTable("SaleBills");
            builder.HasKey(b => b.Id);

            builder.HasOne(s => s.SaleReservationBill)
                .WithMany()
                .HasForeignKey(s => s.SaleReservationBillId).IsRequired();

            builder.Ignore(c => c.Operations);
            //builder.Ignore(c => c.TaxAmount);
            builder.Ignore(c => c.BillType);
            builder.Ignore(c => c.BillTypeId);
            builder.Ignore(c => c.ReceivedStatus);

            base.Configure(builder);
        }
    }

    /// <summary>
    /// 销售单明细
    /// </summary>
    public partial class SaleItemMap : DCMSEntityTypeConfiguration<SaleItem>
    {

        public override void Configure(EntityTypeBuilder<SaleItem> builder)
        {
            builder.ToTable("SaleItems");
            builder.HasKey(b => b.Id);

            builder.HasOne(o => o.SaleBill)
                .WithMany(m => m.Items)
                .HasForeignKey(o => o.SaleBillId).IsRequired();

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
    public partial class SaleBillAccountingMap : DCMSEntityTypeConfiguration<SaleBillAccounting>
    {
        public override void Configure(EntityTypeBuilder<SaleBillAccounting> builder)
        {

            builder.ToTable("SaleBill_Accounting_Mapping")
                .Property(entity => entity.BillId)
                .HasField("SaleBillId");

            builder.HasKey(mapping => new {mapping.BillId, mapping.AccountingOptionId });
            builder.Property(mapping => mapping.BillId).HasColumnName("SaleBillId");
            builder.Property(mapping => mapping.AccountingOptionId).HasColumnName("AccountingOptionId");

            builder.HasOne(mapping => mapping.AccountingOption)
                .WithMany()
                .HasForeignKey(mapping => mapping.AccountingOptionId)
                .IsRequired();

            builder.HasOne(mapping => mapping.SaleBill)
               .WithMany(customer => customer.SaleBillAccountings)
                .HasForeignKey(mapping => mapping.BillId)
                .IsRequired();

            base.Configure(builder);
        }
    }



    /// <summary>
    /// 用于表示送货签收
    /// </summary>
    public partial class DeliverySignMap : DCMSEntityTypeConfiguration<DeliverySign>
    {

        //public DeliverySignMap()
        //{
        //    ToTable("DeliverySigns");
        //    HasKey(o => o.Id);

        //}

        public override void Configure(EntityTypeBuilder<DeliverySign> builder)
        {
            builder.ToTable("DeliverySigns");
            builder.HasKey(b => b.Id);
            base.Configure(builder);
        }
    }

    /// <summary>
    /// 留存凭证照片
    /// </summary>
    public partial class RetainPhotoMap : DCMSEntityTypeConfiguration<RetainPhoto>
    {

        //public RetainPhotoMap()
        //{
        //    ToTable("RetainPhotos");
        //    HasKey(o => o.Id);

        //    HasRequired(o => o.DeliverySign)
        //        .WithMany(m => m.RetainPhotos)
        //        .HasForeignKey(o => o.DeliverySignId);

        //}
        public override void Configure(EntityTypeBuilder<RetainPhoto> builder)
        {
            builder.ToTable("RetainPhotos");
            builder.HasKey(b => b.Id);

            builder.HasOne(o => o.DeliverySign)
                .WithMany(m => m.RetainPhotos)
                .HasForeignKey(o => o.DeliverySignId).IsRequired();

            base.Configure(builder);
        }
    }


}
