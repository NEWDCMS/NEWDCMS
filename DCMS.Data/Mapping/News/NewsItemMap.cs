using DCMS.Core.Domain.News;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace DCMS.Data.Mapping.News
{
    public partial class NewsItemMap : DCMSEntityTypeConfiguration<NewsItem>
    {
        //public NewsItemMap()
        //{
        //    ToTable("NewsItem");
        //    HasKey(bp => bp.Id);
        //    Property(bp => bp.Title).IsRequired();
        //    Property(bp => bp.Full).IsRequired();
        //    Property(bp => bp.MetaKeywords).HasMaxLength(400);
        //    Property(bp => bp.MetaTitle).HasMaxLength(400);

        //    //关系映射
        //    HasRequired(bp => bp.NewsCategory)
        //        .WithMany(p => p.NewsItems)
        //        .HasForeignKey(bp => bp.NewsCategoryId).WillCascadeOnDelete(true);
        //}

        public override void Configure(EntityTypeBuilder<NewsItem> builder)
        {
            builder.ToTable("NewsItem");
            builder.HasKey(b => b.Id);

            builder.Property(bp => bp.Title).IsRequired();
            builder.Property(bp => bp.Full).IsRequired();
            builder.Property(bp => bp.MetaKeywords).HasMaxLength(400);
            builder.Property(bp => bp.MetaTitle).HasMaxLength(400);

            //关系映射
            builder.HasOne(bp => bp.NewsCategory)
                .WithMany(p => p.NewsItems)
                .HasForeignKey(bp => bp.NewsCategoryId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            base.Configure(builder);
        }
    }
}