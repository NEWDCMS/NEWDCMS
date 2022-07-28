using DCMS.Core.Domain.Terminals;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace DCMS.Data.Mapping.Terminals
{
    /// <summary>
    /// 渠道映射
    /// </summary>
    public partial class ChannelMap : DCMSEntityTypeConfiguration<Channel>
    {
        //public ChannelMap()
        //{
        //    ToTable("Channels");
        //    HasKey(o => o.Id);
        //}

        public override void Configure(EntityTypeBuilder<Channel> builder)
        {
            builder.ToTable("Channels");
            builder.HasKey(b => b.Id);
            base.Configure(builder);
        }
    }
}
