using DCMS.Core.Domain.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DCMS.Data.Mapping
{
    public partial class BrandMap : DCMSEntityTypeConfiguration<Brand>
    {
        //public BrandMap()
        //{
        //    ToTable("Brands");
        //    HasKey(o => o.Id);

        //}

        public override void Configure(EntityTypeBuilder<Brand> builder)
        {
            builder.ToTable("Brands");
            builder.HasKey(b => b.Id);
            base.Configure(builder);
        }
    }


}
