using DCMS.Core.Domain.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace DCMS.Data.Mapping.Products
{
    /// <summary>
    /// 规格属性映射
    /// </summary>
    public partial class SpecificationAttributeMap : DCMSEntityTypeConfiguration<SpecificationAttribute>
    {
        //public SpecificationAttributeMap()
        //{
        //    ToTable("SpecificationAttributes");
        //    HasKey(sa => sa.Id);
        //    Property(sa => sa.Name).IsRequired();
        //}

        public override void Configure(EntityTypeBuilder<SpecificationAttribute> builder)
        {
            builder.ToTable("SpecificationAttributes");
            builder.HasKey(b => b.Id);
            builder.Property(sa => sa.Name).IsRequired();
            base.Configure(builder);
        }
    }
}