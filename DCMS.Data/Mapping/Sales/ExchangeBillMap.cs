using DCMS.Core.Domain.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace DCMS.Data.Mapping.BILLs
{
    /// <summary>
    /// 换货单
    /// </summary>
    public partial class ExchangeBillMap : DCMSEntityTypeConfiguration<ExchangeBill>
    {

        public override void Configure(EntityTypeBuilder<ExchangeBill> builder)
        {
            builder.ToTable("ExchangeBills");
            builder.HasKey(b => b.Id);

            builder.Ignore(o => o.Operations);
            builder.Ignore(c => c.BillType);
            builder.Ignore(c => c.BillTypeId);

            base.Configure(builder);
        }
    }
    public partial class ExchangeItemMap : DCMSEntityTypeConfiguration<ExchangeItem>
    {
        public override void Configure(EntityTypeBuilder<ExchangeItem> builder)
        {
            builder.ToTable("ExchangeItems");
            builder.HasKey(b => b.Id);

            builder.HasOne(o => o.ExchangeBill)
                .WithMany(m => m.Items)
                .HasForeignKey(o => o.ExchangeBillId).IsRequired();

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


}
