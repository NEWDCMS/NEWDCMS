using DCMS.Core.Domain.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DCMS.Data.Mapping
{

    public partial class ProductTierPricePlanMap : DCMSEntityTypeConfiguration<ProductTierPricePlan>
    {
        //public ProductTierPricePlanMap()
        //{
        //    ToTable("ProductTierPricePlans");
        //    HasKey(o => o.Id);

        //}

        public override void Configure(EntityTypeBuilder<ProductTierPricePlan> builder)
        {
            builder.ToTable("ProductTierPricePlans");
            builder.HasKey(b => b.Id);
            base.Configure(builder);
        }
    }


}

