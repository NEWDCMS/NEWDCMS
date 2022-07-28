using DCMS.Core.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DCMS.Data.Mapping.Users
{
    public partial class BranchMap : DCMSEntityTypeConfiguration<Branch>
    {
        //public BranchMap()
        //{
        //    ToTable("Branch");
        //    HasKey(pr => pr.Id);
        //}

        public override void Configure(EntityTypeBuilder<Branch> builder)
        {
            builder.ToTable("Branch");
            builder.HasKey(b => b.Id);
            base.Configure(builder);
        }
    }
}
