using DCMS.Core.Domain.Terminals;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace DCMS.Data.Mapping.Terminals
{

    /// <summary>
    /// 片区映射
    /// </summary>
    public partial class DistrictMap : DCMSEntityTypeConfiguration<District>
    {
        //public DistrictMap()
        //{
        //    ToTable("Districts");
        //    HasKey(o => o.Id);
        //}

        public override void Configure(EntityTypeBuilder<District> builder)
        {
            builder.ToTable("Districts");
            builder.HasKey(b => b.Id);
            base.Configure(builder);
        }
    }
}
