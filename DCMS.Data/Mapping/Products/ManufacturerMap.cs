using DCMS.Core.Domain.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace DCMS.Data.Mapping.Products
{
    /// <summary>
    /// 提供商映射
    /// </summary>
    public partial class ManufacturerMap : DCMSEntityTypeConfiguration<Manufacturer>
    {
        //public ManufacturerMap()
        //{
        //    ToTable("Manufacturer");
        //    HasKey(m => m.Id);
        //    Property(m => m.Name).IsRequired().HasMaxLength(400);
        //}

        public override void Configure(EntityTypeBuilder<Manufacturer> builder)
        {
            builder.ToTable("Manufacturer");
            builder.HasKey(b => b.Id);
            builder.Property(b => b.Name).IsRequired().HasMaxLength(400);
            base.Configure(builder);
        }
    }
}