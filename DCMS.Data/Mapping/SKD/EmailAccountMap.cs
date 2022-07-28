using DCMS.Core.Domain.Messages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DCMS.Data.Mapping.Tasks
{
    public partial class EmailAccountMap : DCMSEntityTypeConfiguration<EmailAccount>
    {
        //public EmailAccountMap()
        //{
        //    this.ToTable("EmailAccount");
        //    this.HasKey(ea => ea.Id);

        //    this.Property(ea => ea.Email).IsRequired().HasMaxLength(255);
        //    this.Property(ea => ea.DisplayName).HasMaxLength(255);
        //    this.Property(ea => ea.Host).IsRequired().HasMaxLength(255);
        //    this.Property(ea => ea.Username).IsRequired().HasMaxLength(255);
        //    this.Property(ea => ea.Password).IsRequired().HasMaxLength(255);

        //    this.Ignore(ea => ea.FriendlyName);
        //}

        public override void Configure(EntityTypeBuilder<EmailAccount> builder)
        {
            builder.ToTable("EmailAccount");
            builder.HasKey(b => b.Id);

            builder.Property(ea => ea.Email).IsRequired().HasMaxLength(255);
            builder.Property(ea => ea.DisplayName).HasMaxLength(255);
            builder.Property(ea => ea.Host).IsRequired().HasMaxLength(255);
            builder.Property(ea => ea.Username).IsRequired().HasMaxLength(255);
            builder.Property(ea => ea.Password).IsRequired().HasMaxLength(255);

            builder.Ignore(ea => ea.FriendlyName);


            base.Configure(builder);
        }
    }
}