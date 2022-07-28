using DCMS.Core.Domain.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace DCMS.Data.Mapping
{
    public partial class GiveQuotaMap : DCMSEntityTypeConfiguration<GiveQuota>
    {
        //public GiveQuotaMap()
        //{
        //    ToTable("GiveQuota");
        //    HasKey(o => o.Id);

        //    //this.HasMany(g => g.GiveQuotaOptions)
        //    //    .WithRequired(g => g.GiveQuota);
        //}

        public override void Configure(EntityTypeBuilder<GiveQuota> builder)
        {
            builder.ToTable("GiveQuota");
            builder.HasKey(b => b.Id);
            base.Configure(builder);
        }
    }

    public partial class GiveQuotaOptionMap : DCMSEntityTypeConfiguration<GiveQuotaOption>
    {
        //public GiveQuotaOptionMap()
        //{
        //    ToTable("GiveQuotaOption");
        //    HasKey(o => o.Id);

        //    HasRequired(g => g.GiveQuota)
        //        .WithMany(g => g.GiveQuotaOptions)
        //        .HasForeignKey(q => q.GiveQuotaId);
        //}
        public override void Configure(EntityTypeBuilder<GiveQuotaOption> builder)
        {
            builder.ToTable("GiveQuotaOption");
            builder.HasKey(b => b.Id);

            builder.HasOne(g => g.GiveQuota)
                .WithMany(g => g.GiveQuotaOptions)
                .HasForeignKey(q => q.GiveQuotaId);

            base.Configure(builder);
        }
    }


    public partial class GiveQuotaRecordsMap : DCMSEntityTypeConfiguration<GiveQuotaRecords>
    {
        //public GiveQuotaRecordsMap()
        //{
        //    ToTable("GiveQuotaRecords");
        //    HasKey(o => o.Id);

        //    Ignore(o => o.GiveType);
        //}
        public override void Configure(EntityTypeBuilder<GiveQuotaRecords> builder)
        {
            builder.ToTable("GiveQuotaRecords");
            builder.HasKey(b => b.Id);
            builder.Ignore(b => b.GiveType);

            base.Configure(builder);
        }
    }

}
