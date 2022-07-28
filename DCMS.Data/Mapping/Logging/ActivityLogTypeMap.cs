using DCMS.Core.Domain.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace DCMS.Data.Mapping.Logging
{
    public partial class ActivityLogTypeMap : DCMSEntityTypeConfiguration<ActivityLogType>
    {
        //public ActivityLogTypeMap()
        //{
        //    ToTable("ActivityLogType");
        //    HasKey(alt => alt.Id);

        //    Property(alt => alt.SystemKeyword).IsRequired().HasMaxLength(100);
        //    Property(alt => alt.Name).IsRequired().HasMaxLength(200);
        //}

        public override void Configure(EntityTypeBuilder<ActivityLogType> builder)
        {
            builder.ToTable("ActivityLogType");
            builder.HasKey(b => b.Id);
            builder.Property(alt => alt.SystemKeyword).IsRequired().HasMaxLength(100);
            builder.Property(alt => alt.Name).IsRequired().HasMaxLength(200);

            base.Configure(builder);
        }
    }
}
