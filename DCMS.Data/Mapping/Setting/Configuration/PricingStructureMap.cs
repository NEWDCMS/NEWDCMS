using DCMS.Core.Domain.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace DCMS.Data.Mapping
{
    public partial class PricingStructureMap : DCMSEntityTypeConfiguration<PricingStructure>
    {
        //public PricingStructureMap()
        //{
        //    ToTable("PricingStructures");
        //    HasKey(o => o.Id);

        //}

        public override void Configure(EntityTypeBuilder<PricingStructure> builder)
        {
            builder.ToTable("PricingStructures");
            builder.HasKey(b => b.Id);
            base.Configure(builder);
        }
    }

}
