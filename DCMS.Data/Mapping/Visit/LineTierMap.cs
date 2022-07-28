using DCMS.Core.Domain.Visit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace DCMS.Data.Mapping.Visit
{
    public partial class LineTierMap : DCMSEntityTypeConfiguration<LineTier>
    {
        //public LineTierMap()
        //{
        //    ToTable("LineTiers");
        //    HasKey(pc => pc.Id);

        //}

        public override void Configure(EntityTypeBuilder<LineTier> builder)
        {
            builder.ToTable("LineTiers");
            builder.HasKey(b => b.Id);
            base.Configure(builder);
        }
    }

    public partial class LineTierOptionMap : DCMSEntityTypeConfiguration<LineTierOption>
    {
        //public LineTierOptionMap()
        //{
        //    ToTable("LineTierOptions");
        //    HasKey(pc => pc.Id);

        //    HasRequired(l => l.LineTier)
        //        .WithMany(lo => lo.LineTierOptions)
        //        .HasForeignKey(l => l.LineTierId);
        //}

        public override void Configure(EntityTypeBuilder<LineTierOption> builder)
        {
            builder.ToTable("LineTierOptions");
            builder.HasKey(b => b.Id);
            builder.HasOne(l => l.LineTier)
                .WithMany(lo => lo.LineTierOptions)
                .HasForeignKey(l => l.LineTierId)
                .IsRequired();
            base.Configure(builder);
        }
    }
}
