using DCMS.Core.Domain.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DCMS.Data.Mapping
{
    /// <summary>
    /// 类别映射
    /// </summary>
    public partial class CategoryMap : DCMSEntityTypeConfiguration<Category>
    {
        //public CategoryMap()
        //{
        //    ToTable("Categories");
        //    HasKey(o => o.Id);
        //}

        public override void Configure(EntityTypeBuilder<Category> builder)
        {
            builder.ToTable("Categories");
            builder.HasKey(b => b.Id);
            base.Configure(builder);
        }
    }


    /// <summary>
    ///  商品类别映射
    /// </summary>
    public partial class ProductCategoryMap : DCMSEntityTypeConfiguration<ProductCategory>
    {
        //public ProductCategoryMap()
        //{
        //    ToTable("Products_Category_Mapping");

        //    HasRequired(o => o.Category)
        //         .WithMany()
        //         .HasForeignKey(o => o.CategoryId);

        //    HasRequired(o => o.Product)
        //        .WithMany(m => m.ProductCategories)
        //        .HasForeignKey(o => o.ProductId);

        //}

        public override void Configure(EntityTypeBuilder<ProductCategory> builder)
        {

            builder.ToTable(DCMSMappingDefaults.ProductCategoryTable);
            builder.HasKey(mapping => new { mapping.CategoryId, mapping.ProductId });

            builder.Property(mapping => mapping.CategoryId).HasColumnName("CategoryId");
            builder.Property(mapping => mapping.ProductId).HasColumnName("ProductId");

            builder.HasOne(mapping => mapping.Category)
                .WithMany()
                .HasForeignKey(mapping => mapping.CategoryId)
                .IsRequired();

            builder.HasOne(mapping => mapping.Product)
               .WithMany(customer => customer.ProductCategories)
                .HasForeignKey(mapping => mapping.ProductId)
                .IsRequired();



            base.Configure(builder);
        }
    }

}
