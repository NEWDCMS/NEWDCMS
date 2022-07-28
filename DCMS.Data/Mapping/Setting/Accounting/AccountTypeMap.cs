using DCMS.Core.Domain.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace DCMS.Data.Mapping.Setting
{
    public partial class AccountingTypeMap : DCMSEntityTypeConfiguration<AccountingType>
    {
        //public AccountingTypeMap()
        //{
        //    ToTable("AccountingTypes");
        //    HasKey(o => o.Id);
        //}

        public override void Configure(EntityTypeBuilder<AccountingType> builder)
        {
            builder.ToTable("AccountingTypes");
            builder.HasKey(b => b.Id);
            base.Configure(builder);
        }
    }

    public partial class AccountingOptionMap : DCMSEntityTypeConfiguration<AccountingOption>
    {
        //public AccountingOptionMap()
        //{
        //    ToTable("AccountingOptions");
        //    HasKey(ao => ao.Id);
        //    Property(ao => ao.Name).IsRequired();

        //    Ignore(ao => ao.Balance);

        //    HasRequired(sao => sao.AccountingType)
        //        .WithMany(sa => sa.AccountingOptions)
        //        .HasForeignKey(sao => sao.AccountingTypeId);
        //}

        public override void Configure(EntityTypeBuilder<AccountingOption> builder)
        {
            builder.ToTable("AccountingOptions");
            builder.HasKey(b => b.Id);
            //builder.Ignore(ao => ao.Level);

            builder.Property(ao => ao.Name).IsRequired();

            builder.HasOne(sao => sao.AccountingType)
                .WithMany(sa => sa.AccountingOptions)
                .HasForeignKey(sao => sao.AccountingTypeId)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
