using DCMS.Core.Domain.Terminals;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace DCMS.Data.Mapping.Terminals
{
    /// <summary>
    /// 渠道映射
    /// </summary>
    public partial class RankMap : DCMSEntityTypeConfiguration<Rank>
    {
        //public RankMap()
        //{
        //    ToTable("Ranks");
        //    HasKey(o => o.Id);
        //}

        public override void Configure(EntityTypeBuilder<Rank> builder)
        {
            builder.ToTable("Ranks");
            builder.HasKey(b => b.Id);
            base.Configure(builder);
        }
    }
}
