using DCMS.Core.Domain.Security;
//using System.Data.Entity.ModelConfiguration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace DCMS.Data.Mapping.Security
{
    public partial class AclRecordMap : DCMSEntityTypeConfiguration<AclRecord>
    {
        //public AclRecordMap()
        //{
        //    ToTable("AclRecord");
        //    HasKey(lp => lp.Id);
        //    Property(lp => lp.EntityName).IsRequired().HasMaxLength(400);
        //}

        public override void Configure(EntityTypeBuilder<AclRecord> builder)
        {
            builder.ToTable("AclRecord");
            builder.HasKey(affiliate => affiliate.Id);
            builder.Property(lp => lp.EntityName).IsRequired().HasMaxLength(400);
            base.Configure(builder);
        }


    }
}