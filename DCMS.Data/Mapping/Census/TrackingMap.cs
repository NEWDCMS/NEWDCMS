using DCMS.Core.Domain.Census;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace DCMS.Data.Mapping.Census
{
    public partial class TrackingMap : DCMSEntityTypeConfiguration<Tracking>
    {
        //public TrackingMap()
        //{
        //    ToTable("Tracking");
        //    HasKey(o => o.Id);
        //}

        public override void Configure(EntityTypeBuilder<Tracking> builder)
        {
            builder.ToTable("Tracking");
            builder.HasKey(b => b.Id);
            base.Configure(builder);
        }
    }
}
