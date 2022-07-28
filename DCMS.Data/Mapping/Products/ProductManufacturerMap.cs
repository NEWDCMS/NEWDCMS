using DCMS.Core.Domain.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace DCMS.Data.Mapping.Products
{
    public partial class ProductManufacturerMap : DCMSEntityTypeConfiguration<ProductManufacturer>
    {
        //public ProductManufacturerMap()
        //{
        //    ToTable("Product_Manufacturer_Mapping");
        //    HasKey(pm => pm.Id);

        //    HasRequired(pm => pm.Manufacturer)
        //        .WithMany()
        //        .HasForeignKey(pm => pm.ManufacturerId);


        //    HasRequired(pm => pm.Product)
        //        .WithMany(p => p.ProductManufacturers)
        //        .HasForeignKey(pm => pm.ProductId);
        //}

        public override void Configure(EntityTypeBuilder<ProductManufacturer> builder)
        {

            builder.ToTable(DCMSMappingDefaults.ProductManufacturerTable);
            builder.HasKey(mapping => new { mapping.ProductId, mapping.ManufacturerId });

            builder.Property(mapping => mapping.ProductId).HasColumnName("ProductId");
            builder.Property(mapping => mapping.ManufacturerId).HasColumnName("ManufacturerId");

            builder.HasOne(mapping => mapping.Manufacturer)
                .WithMany()
                .HasForeignKey(mapping => mapping.ManufacturerId)
                .IsRequired();

            builder.HasOne(mapping => mapping.Product)
               .WithMany(customer => customer.ProductManufacturers)
                .HasForeignKey(mapping => mapping.ProductId)
                .IsRequired();

            base.Configure(builder);
        }

    }
}