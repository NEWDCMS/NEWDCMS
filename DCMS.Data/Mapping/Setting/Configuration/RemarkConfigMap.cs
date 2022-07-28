using DCMS.Core.Domain.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace DCMS.Data.Mapping
{
    public partial class RemarkConfigMap : DCMSEntityTypeConfiguration<RemarkConfig>
    {
        //public RemarkConfigMap()
        //{
        //    ToTable("RemarkConfigs");
        //    HasKey(o => o.Id);
        //}

        public override void Configure(EntityTypeBuilder<RemarkConfig> builder)
        {
            builder.ToTable("RemarkConfigs");
            builder.HasKey(b => b.Id);
            base.Configure(builder);
        }
    }
}
