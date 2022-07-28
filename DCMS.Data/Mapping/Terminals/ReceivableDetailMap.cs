using DCMS.Core.Domain.Terminals;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace DCMS.Data.Mapping.Terminals
{
    /// <summary>
    /// 应收款记录表映射
    /// </summary>
    public class ReceivableDetailMap : DCMSEntityTypeConfiguration<ReceivableDetail>
    {
        //public ReceivableDetailMap()
        //{
        //    ToTable("ReceivableDetails");
        //    HasKey(o => o.Id);
        //}

        public override void Configure(EntityTypeBuilder<ReceivableDetail> builder)
        {
            builder.ToTable("ReceivableDetails");
            builder.HasKey(b => b.Id);
            base.Configure(builder);
        }
    }
}
