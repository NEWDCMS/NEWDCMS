using DCMS.Core.Domain.Finances;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace DCMS.Data.Mapping.Finances
{

    /// <summary>
    /// 用于表示记账凭证
    /// </summary>
    public partial class RecordingVoucherMap : DCMSEntityTypeConfiguration<RecordingVoucher>
    {
        public override void Configure(EntityTypeBuilder<RecordingVoucher> builder)
        {
            builder.ToTable("RecordingVouchers");
            builder.HasKey(b => b.Id);
            builder.Ignore(b => b.Generate);
            builder.Ignore(c => c.BillType);
            builder.Ignore(c => c.CreatedOnUtc);
            builder.Ignore(c => c.Remark);

            builder.Property(mapping => mapping.BillTypeId).HasColumnName("BillTypeId");
            builder.Property(mapping => mapping.Deleted).HasColumnName("DeleteStatus");

            base.Configure(builder);
        }
    }


    /// <summary>
    /// 用于表示凭证项目
    /// </summary>
    public partial class VoucherItemMap : DCMSEntityTypeConfiguration<VoucherItem>
    {

        public override void Configure(EntityTypeBuilder<VoucherItem> builder)
        {
            builder.ToTable("VoucherItems");
            builder.HasKey(b => b.Id);
            //
            builder.Ignore(b => b.AccountingCode);

            builder.HasOne(sao => sao.RecordingVoucher)
            .WithMany(sa => sa.Items)
            .HasForeignKey(sao => sao.RecordingVoucherId).IsRequired();

            base.Configure(builder);
        }
    }



}
