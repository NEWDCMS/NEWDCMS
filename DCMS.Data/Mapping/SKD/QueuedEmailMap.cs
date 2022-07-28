using DCMS.Core.Domain.Messages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DCMS.Data.Mapping.Tasks
{
    public partial class QueuedEmailMap : DCMSEntityTypeConfiguration<QueuedEmail>
    {
        //public QueuedEmailMap()
        //{
        //    this.ToTable("QueuedEmail");
        //    this.HasKey(qe => qe.Id);

        //    this.Property(qe => qe.From).IsRequired().HasMaxLength(500);
        //    this.Property(qe => qe.FromName).HasMaxLength(500);
        //    this.Property(qe => qe.To).IsRequired().HasMaxLength(500);
        //    this.Property(qe => qe.ToName).HasMaxLength(500);
        //    this.Property(qe => qe.CC).HasMaxLength(500);
        //    this.Property(qe => qe.Bcc).HasMaxLength(500);
        //    this.Property(qe => qe.Subject).HasMaxLength(1000);


        //    this.HasRequired(qe => qe.EmailAccount)
        //        .WithMany()
        //        .HasForeignKey(qe => qe.EmailAccountId).WillCascadeOnDelete(true);
        //}

        public override void Configure(EntityTypeBuilder<QueuedEmail> builder)
        {
            builder.ToTable("QueuedEmail");
            builder.HasKey(b => b.Id);

            builder.Property(qe => qe.From).IsRequired().HasMaxLength(500);
            builder.Property(qe => qe.FromName).HasMaxLength(500);
            builder.Property(qe => qe.To).IsRequired().HasMaxLength(500);
            builder.Property(qe => qe.ToName).HasMaxLength(500);
            builder.Property(qe => qe.CC).HasMaxLength(500);
            builder.Property(qe => qe.Bcc).HasMaxLength(500);
            builder.Property(qe => qe.Subject).HasMaxLength(1000);


            builder.HasOne(qe => qe.EmailAccount)
                .WithMany()
                .HasForeignKey(qe => qe.EmailAccountId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            builder.Ignore(email => email.Priority);
            base.Configure(builder);
        }
    }
}