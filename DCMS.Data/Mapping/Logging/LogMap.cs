using DCMS.Core.Domain.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace DCMS.Data.Mapping.Logging
{
    public partial class LogMap : DCMSEntityTypeConfiguration<Log>
    {
        //public LogMap()
        //{
        //    ToTable("Log");
        //    HasKey(l => l.Id);
        //    Property(l => l.ShortMessage).IsRequired();
        //    Property(l => l.IpAddress).HasMaxLength(200);

        //    Ignore(l => l.LogLevel);

        //    HasOptional(l => l.User)
        //        .WithMany()
        //        .HasForeignKey(l => l.UserId)
        //    .WillCascadeOnDelete(true);

        //}


        public override void Configure(EntityTypeBuilder<Log> builder)
        {
            builder.ToTable("Log");
            builder.HasKey(b => b.Id);
            builder.Property(l => l.ShortMessage).IsRequired();
            builder.Property(l => l.IpAddress).HasMaxLength(200);
            builder.Ignore(l => l.LogLevel);

            builder.HasOne(logItem => logItem.User)
                      .WithMany()
                      .HasForeignKey(logItem => logItem.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

            base.Configure(builder);
        }
    }
}