using DCMS.Core.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace DCMS.Data.Mapping.Users
{
    public partial class UserGroupMap : DCMSEntityTypeConfiguration<UserGroup>
    {
        //public UserGroupMap()
        //{
        //    ToTable("UserGroup");
        //    HasKey(cr => cr.Id);

        //    //用户组用户
        //    HasMany(g => g.Users)
        //        .WithMany(u => u.UserGroups)
        //        .Map(m => m.MapLeftKey("UserGroup_Id").MapRightKey("User_Id").ToTable("UserGroup_Users_Mapping"));

        //    //用户组用户角色
        //    HasMany(g => g.UserRoles)
        //        .WithMany(r => r.UserGroups)
        //        .Map(m => m.MapLeftKey("UserGroup_Id").MapRightKey("UserRole_Id").ToTable("UserGroup_UserRoles_Mapping"));
        //}

        public override void Configure(EntityTypeBuilder<UserGroup> builder)
        {
            builder.ToTable("UserGroup");
            builder.HasKey(b => b.Id);
            //
            builder.Ignore(mapping => mapping.Users);
            builder.Ignore(mapping => mapping.UserRoles);
            base.Configure(builder);
        }

    }

    /// <summary>
    /// Mapping
    /// </summary>
    public partial class UserGroupUserMap : DCMSEntityTypeConfiguration<UserGroupUser>
    {
        public override void Configure(EntityTypeBuilder<UserGroupUser> builder)
        {

            builder.ToTable(DCMSMappingDefaults.UserGroupUsersTable);
            builder.HasKey(mapping => new { mapping.UserGroup_Id, mapping.User_Id });

            builder.Property(mapping => mapping.UserGroup_Id).HasColumnName("UserGroup_Id");
            builder.Property(mapping => mapping.User_Id).HasColumnName("User_Id");

            builder.HasOne(mapping => mapping.User)
                .WithMany(customer => customer.UserGroupUsers)
                .HasForeignKey(mapping => mapping.User_Id)
                .IsRequired();

            builder.HasOne(mapping => mapping.UserGroup)
               .WithMany(customer => customer.UserGroupUsers)
                .HasForeignKey(mapping => mapping.UserGroup_Id)
                .IsRequired();




            base.Configure(builder);
        }

    }


    /// <summary>
    /// Mapping
    /// </summary>
    public partial class UserGroupUserRoleMap : DCMSEntityTypeConfiguration<UserGroupUserRole>
    {
        public override void Configure(EntityTypeBuilder<UserGroupUserRole> builder)
        {

            builder.ToTable(DCMSMappingDefaults.UserGroupUserRolesTable);
            builder.HasKey(mapping => new { mapping.UserGroup_Id, mapping.UserRole_Id });

            builder.Property(mapping => mapping.UserGroup_Id).HasColumnName("UserGroup_Id");
            builder.Property(mapping => mapping.UserRole_Id).HasColumnName("UserRole_Id");

            builder.HasOne(mapping => mapping.UserGroup)
                .WithMany(customer => customer.UserGroupUserRoles)
                .HasForeignKey(mapping => mapping.UserGroup_Id)
                .IsRequired();

            builder.HasOne(mapping => mapping.UserRole)
               .WithMany(customer => customer.UserGroupUserRoles)
                .HasForeignKey(mapping => mapping.UserRole_Id)
                .IsRequired();

            builder.Ignore(m => m.Id);

            base.Configure(builder);
        }

    }
}
