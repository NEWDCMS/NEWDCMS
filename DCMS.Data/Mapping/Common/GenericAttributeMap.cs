using DCMS.Core.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DCMS.Data.Mapping
{

    public partial class GenericAttributeMap : DCMSEntityTypeConfiguration<GenericAttribute>
    {
        //public GenericAttributeMap()
        //{
        //    ToTable("GenericAttributes");
        //    HasKey(ga => ga.Id);

        //    Property(ga => ga.KeyGroup).IsRequired().HasMaxLength(400);
        //    Property(ga => ga.Key).IsRequired().HasMaxLength(400);
        //    Property(ga => ga.Value).IsRequired();
        //}

        public override void Configure(EntityTypeBuilder<GenericAttribute> builder)
        {
            builder.ToTable("GenericAttributes");
            builder.HasKey(b => b.Id);
            builder.Property(ga => ga.KeyGroup).IsRequired().HasMaxLength(400);
            builder.Property(ga => ga.Key).IsRequired().HasMaxLength(400);
            builder.Property(ga => ga.Value).IsRequired();
            base.Configure(builder);
        }
    }
}