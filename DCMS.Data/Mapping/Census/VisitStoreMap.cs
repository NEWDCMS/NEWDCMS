using DCMS.Core.Domain.Census;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace DCMS.Data.Mapping.Census
{
    //门店拜访记录表
    public partial class VisitStoreMap : DCMSEntityTypeConfiguration<VisitStore>
    {
        //public VisitStoreMap()
        //{
        //    ToTable("VisitStore");
        //    HasKey(o => o.Id);

        //    //忽略
        //    Ignore(c => c.VisitType);
        //    Ignore(c => c.SignType);
        //    Ignore(c => c.LastPurchaseDate);

        //}

        public override void Configure(EntityTypeBuilder<VisitStore> builder)
        {
            builder.ToTable("VisitStore");
            builder.HasKey(b => b.Id);

            builder.Ignore(c => c.VisitType);
            builder.Ignore(c => c.SignType);
            builder.Ignore(c => c.LastPurchaseDate);


            base.Configure(builder);
        }
    }
}
