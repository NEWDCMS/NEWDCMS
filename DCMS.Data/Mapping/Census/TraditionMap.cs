using DCMS.Core.Domain.Census;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace DCMS.Data.Mapping
{

    public partial class TraditionMap : DCMSEntityTypeConfiguration<Tradition>
    {
        //public TraditionMap()
        //{
        //    ToTable("Tradition");
        //    HasKey(o => o.Id);
        //}


        public override void Configure(EntityTypeBuilder<Tradition> builder)
        {
            builder.ToTable("Tradition");
            builder.HasKey(b => b.Id);
            base.Configure(builder);
        }
    }
}