using DCMS.Core.Domain.Census;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DCMS.Data.Mapping
{
    public partial class DoorheadPhotoMap : DCMSEntityTypeConfiguration<DoorheadPhoto>
    {
        //public DoorheadPhotoMap()
        //{
        //    ToTable("DoorheadPhoto");
        //    HasKey(c => c.Id);
        //    //this.Property(t => t.Id).HasColumnName("Id").HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

        //    HasRequired(o => o.Tradition)
        //       .WithMany(c => c.DoorheadPhotos)
        //       .HasForeignKey(o => o.TraditionId);


        //    HasRequired(o => o.Restaurant)
        //        .WithMany(c => c.DoorheadPhotos)
        //        .HasForeignKey(o => o.RestaurantId);


        //    //HasRequired(o => o.VisitStore)
        //    //   .WithMany(c => c.DoorheadPhotos)
        //    //   .HasForeignKey(o => o.VisitStoreId);
        //}

        public override void Configure(EntityTypeBuilder<DoorheadPhoto> builder)
        {
            builder.ToTable("DoorheadPhoto");
            builder.HasKey(b => b.Id);

            //builder.HasOne(o => o.Tradition)
            // .WithMany(c => c.DoorheadPhotos)
            // .HasForeignKey(o => o.TraditionId).IsRequired();


            //builder.HasOne(o => o.Restaurant)
            //    .WithMany(c => c.DoorheadPhotos)
            //    .HasForeignKey(o => o.RestaurantId).IsRequired();


            base.Configure(builder);
        }
    }

}
