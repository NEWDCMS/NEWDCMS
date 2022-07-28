using DCMS.Core.Domain.Visit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace DCMS.Data.Mapping.Visit
{
    public partial class UserLineTierAssignMap : DCMSEntityTypeConfiguration<UserLineTierAssign>
    {
        //public UserLineTierAssignMap()
        //{
        //    ToTable("LineTierUserMapping");
        //    HasKey(pc => pc.Id);


        //    HasRequired(pc => pc.LineTier)
        //        .WithMany()
        //        .HasForeignKey(pc => pc.LineTierId);

        //}

        public override void Configure(EntityTypeBuilder<UserLineTierAssign> builder)
        {
            builder.ToTable("LineTierUserMapping");
            builder.HasKey(b => b.Id);

            builder.HasOne(pc => pc.LineTier)
                .WithMany()
                .HasForeignKey(pc => pc.LineTierId).IsRequired();

            base.Configure(builder);
        }
    }
}
