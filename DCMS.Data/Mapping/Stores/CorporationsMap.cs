using DCMS.Core.Domain.Stores;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DCMS.Data.Mapping.Stores
{
    public class CorporationsMap : DCMSEntityTypeConfiguration<Corporations>
    {
        public override void Configure(EntityTypeBuilder<Corporations> builder)
        {
            builder.ToTable("Corporations");
            builder.HasKey(b => b.Id);

            //store表没有StoreId字段，暂时忽略
            builder.Ignore(s => s.StoreId);

            base.Configure(builder);
        }
    }
}
