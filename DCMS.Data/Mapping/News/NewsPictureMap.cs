using DCMS.Core.Domain.News;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace DCMS.Data.Mapping.News
{
    public partial class NewsPictureMap : DCMSEntityTypeConfiguration<NewsPicture>
    {
        //public NewsPictureMap()
        //{
        //    ToTable("NewsPicture");
        //    HasKey(pp => pp.Id);
        //}

        public override void Configure(EntityTypeBuilder<NewsPicture> builder)
        {
            builder.ToTable("NewsPicture");
            builder.HasKey(b => b.Id);
            base.Configure(builder);
        }
    }
}