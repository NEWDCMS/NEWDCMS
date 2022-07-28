using DCMS.Core.Domain.Stores;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace DCMS.Data.Mapping.Stores
{
    public partial class StoreMappingMap : DCMSEntityTypeConfiguration<StoreMapping>
    {
        //public StoreMappingMap()
        //{
        //    ToTable("StoreMapping");
        //    HasKey(sm => sm.Id);

        //    Property(sm => sm.EntityName).IsRequired().HasMaxLength(400);
        //}

        public override void Configure(EntityTypeBuilder<StoreMapping> builder)
        {
            builder.ToTable("StoreMapping");
            builder.HasKey(b => b.Id);
            builder.Property(sm => sm.EntityName).IsRequired().HasMaxLength(400);
            base.Configure(builder);
        }
    }
}