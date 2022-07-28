using DCMS.Core.Domain.CSMS;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DCMS.Data.Mapping.CSMS
{

    public partial class TerminalSignReportMap : DCMSEntityTypeConfiguration<TerminalSignReport>
    {
        public override void Configure(EntityTypeBuilder<TerminalSignReport> builder)
        {
            builder.ToTable("TerminalSignReport");
            builder.HasKey(b => b.Id);
            base.Configure(builder);
        }
    }

    public partial class OrderDetailMap : DCMSEntityTypeConfiguration<OrderDetail>
    {
        public override void Configure(EntityTypeBuilder<OrderDetail> builder)
        {
            builder.ToTable("OrderDetail");
            builder.HasKey(b => b.Id);
            base.Configure(builder);
        }
    }
}
