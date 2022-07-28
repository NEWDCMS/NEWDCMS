using DCMS.Core.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DCMS.Data.Mapping.Users
{
    public partial class UserRoleMap : DCMSEntityTypeConfiguration<UserRole>
    {
        //public UserRoleMap()
        //{
        //    ToTable("UserRoles");
        //    HasKey(cr => cr.Id);
        //    Property(cr => cr.Name).IsRequired().HasMaxLength(255);
        //    Property(cr => cr.SystemName).HasMaxLength(255);
        //}

        public override void Configure(EntityTypeBuilder<UserRole> builder)
        {
            builder.ToTable("UserRoles");
            builder.HasKey(b => b.Id);
            builder.Property(cr => cr.Name).IsRequired().HasMaxLength(255);
            builder.Property(cr => cr.SystemName).HasMaxLength(255);

            builder.Ignore(user => user.Modules);
            base.Configure(builder);
        }
    }
}