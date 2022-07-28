using DCMS.Core.Domain.News;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace DCMS.Data.Mapping.News
{
    public partial class NewsNewsCategoryMap : DCMSEntityTypeConfiguration<NewsCategory>
    {
        //public NewsNewsCategoryMap()
        //{
        //    ToTable("NewsCategory");
        //    HasKey(pc => pc.Id);

        //    HasOptional(c => c.NewsCategories)
        //        .WithMany(c => c.ChildCategories)
        //        .HasForeignKey(d => d.ParentId);

        //    //this.HasRequired(pc => pc.NewsCategories)
        //    //    .WithMany()
        //    //    .HasForeignKey(pc => pc.NewsCategoryId);

        //    //this.HasRequired(pc => pc.NewsItems)
        //    //    .WithMany()
        //    //    .HasForeignKey(pc => pc.NewsItemId);
        //}

        public override void Configure(EntityTypeBuilder<NewsCategory> builder)
        {
            builder.ToTable("NewsCategory");
            builder.HasKey(b => b.Id);

            builder.HasOne(c => c.NewsCategories)
                .WithMany(c => c.ChildCategories)
                .HasForeignKey(d => d.ParentId).IsRequired();

            base.Configure(builder);
        }
    }
}