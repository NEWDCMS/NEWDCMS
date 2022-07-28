using DCMS.Core.Domain.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace DCMS.Data.Mapping
{

    /// <summary>
    /// 统计类别
    /// </summary>
    public partial class StatisticalTypeMap : DCMSEntityTypeConfiguration<StatisticalTypes>
    {
        //public StatisticalTypeMap()
        //{
        //    ToTable("StatisticalTypes");
        //    HasKey(o => o.Id);
        //}

        public override void Configure(EntityTypeBuilder<StatisticalTypes> builder)
        {
            builder.ToTable("StatisticalTypes");
            builder.HasKey(b => b.Id);
            base.Configure(builder);
        }
    }


}
