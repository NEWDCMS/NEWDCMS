using DCMS.Core.Domain.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace DCMS.Data.Mapping
{

    /// <summary>
    /// 商品映射
    /// </summary>
    public partial class ProductMap : DCMSEntityTypeConfiguration<Product>
    {
        //public ProductMap()
        //{
        //    ToTable("Products");
        //    HasKey(o => o.Id);

        //    Property(o => o.ManageInventoryMethodId).IsRequired();

        //    //忽略
        //    Ignore(o => o.ManageInventoryMethod);
        //    Ignore(o => o.LowStockActivity);

        //}

        public override void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.ToTable("Products");
            builder.HasKey(b => b.Id);
            builder.Property(o => o.ManageInventoryMethodId).IsRequired();
            builder.Ignore(o => o.ManageInventoryMethod);
            builder.Ignore(o => o.LowStockActivity);
            builder.Ignore(o => o.Brand);
            base.Configure(builder);
        }
    }



    public partial class ProductPictureMap : DCMSEntityTypeConfiguration<ProductPicture>
    {
        //public ProductPictureMap()
        //{
        //    ToTable("Products_Pictures_Mapping");
        //    HasKey(pp => pp.Id);

        //    HasRequired(pp => pp.Picture)
        //        .WithMany(p => p.ProductPictures)
        //        .HasForeignKey(pp => pp.PictureId);


        //    HasRequired(pp => pp.Product)
        //        .WithMany(p => p.ProductPictures)
        //        .HasForeignKey(pp => pp.ProductId);
        //}
        public override void Configure(EntityTypeBuilder<ProductPicture> builder)
        {

            builder.ToTable(DCMSMappingDefaults.ProductPictureTable);
            builder.HasKey(mapping => new { mapping.ProductId, mapping.PictureId });

            builder.Property(mapping => mapping.ProductId).HasColumnName("ProductId");
            builder.Property(mapping => mapping.PictureId).HasColumnName("PictureId");

            builder.HasOne(mapping => mapping.Picture)
                .WithMany()
                .HasForeignKey(mapping => mapping.PictureId)
                .IsRequired();

            builder.HasOne(mapping => mapping.Product)
               .WithMany(customer => customer.ProductPictures)
                .HasForeignKey(mapping => mapping.ProductId)
                .IsRequired();



            base.Configure(builder);
        }
    }

}
