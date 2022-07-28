using DCMS.Core.Domain.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DCMS.Data.Mapping
{
    public partial class ProductFlavorMap : DCMSEntityTypeConfiguration<ProductFlavor>
    {
        //public ProductFlavorMap()
        //{
        //    ToTable("ProductFlavors");
        //    HasKey(o => o.Id);
        //}

        public override void Configure(EntityTypeBuilder<ProductFlavor> builder)
        {
            builder.ToTable("ProductFlavors");
            builder.HasKey(b => b.Id);
            base.Configure(builder);
        }

    }
}
