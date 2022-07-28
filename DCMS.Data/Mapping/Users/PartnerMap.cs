using DCMS.Core.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DCMS.Data.Mapping.Account
{
    /// <summary>
    /// 
    /// </summary>
    public partial class PartnerMap : DCMSEntityTypeConfiguration<Partner>
    {

        //public PartnerMap()
        //{
        //    ToTable("Partners");
        //    HasKey(o => o.Id);
        //}

        public override void Configure(EntityTypeBuilder<Partner> builder)
        {
            builder.ToTable("Partners");
            builder.HasKey(b => b.Id);
            base.Configure(builder);
        }
    }


}
