using DCMS.Core.Domain.Stores;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace DCMS.Data.Mapping.Stores
{
    public partial class StoreMap : DCMSEntityTypeConfiguration<Store>
    {
        public override void Configure(EntityTypeBuilder<Store> builder)
        {
            builder.ToTable("CRM_Stores");
            builder.HasKey(b => b.Id);

            builder.Property(s => s.Name).IsRequired().HasMaxLength(400);
            builder.Property(s => s.Url).IsRequired().HasMaxLength(400);
            builder.Property(s => s.SecureUrl).HasMaxLength(400);
            builder.Property(s => s.Hosts).HasMaxLength(1000);

            //store表没有StoreId字段，暂时忽略
            builder.Ignore(s => s.StoreId);

            base.Configure(builder);
        }
    }
}