using DCMS.Core.Domain.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace DCMS.Data.Mapping.Products
{

    /// <summary>
    /// 商品规格属性映射
    /// </summary>
    public partial class ProductSpecificationAttributeMap : DCMSEntityTypeConfiguration<ProductSpecificationAttribute>
    {
        //public ProductSpecificationAttributeMap()
        //{
        //    ToTable("Products_SpecificationAttribute_Mapping");
        //    HasKey(psa => psa.Id);

        //    Property(psa => psa.CustomValue).HasMaxLength(4000);

        //    HasRequired(psa => psa.SpecificationAttributeOption)
        //        .WithMany(sao => sao.ProductSpecificationAttributes)
        //        .HasForeignKey(psa => psa.SpecificationAttributeOptionId);


        //    HasRequired(psa => psa.Product)
        //        .WithMany(p => p.ProductSpecificationAttributes)
        //        .HasForeignKey(psa => psa.ProductId);
        //}

        public override void Configure(EntityTypeBuilder<ProductSpecificationAttribute> builder)
        {

            builder.ToTable(DCMSMappingDefaults.ProductSpecificationAttributeTable);

            builder.HasKey(mapping => new {mapping.Id, mapping.ProductId, mapping.SpecificationAttributeOptionId });

            builder.Property(psa => psa.CustomValue).HasMaxLength(4000);
            builder.Property(mapping => mapping.ProductId).HasColumnName("ProductId");
            builder.Property(mapping => mapping.SpecificationAttributeOptionId).HasColumnName("SpecificationAttributeOptionId");

            builder.HasOne(mapping => mapping.SpecificationAttributeOption)
                .WithMany(p => p.ProductSpecificationAttributes)
                .HasForeignKey(mapping => mapping.SpecificationAttributeOptionId)
                .IsRequired();

            builder.HasOne(mapping => mapping.Product)
               .WithMany(customer => customer.ProductSpecificationAttributes)
                .HasForeignKey(mapping => mapping.ProductId)
                .IsRequired();



            base.Configure(builder);
        }
    }
}