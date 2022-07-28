using DCMS.Core.Domain.CRM;
using DCMS.Core.Domain.OCMS;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DCMS.Data.Mapping.CRM
{

    public partial class CRM_RELATIONMap : DCMSEntityTypeConfiguration<CRM_RELATION>
    {
        public override void Configure(EntityTypeBuilder<CRM_RELATION> builder)
        {
            builder.ToTable("CRM_Relations");
            builder.HasKey(b => b.Id);
            base.Configure(builder);
        }
    }

    public partial class CRM_RETURNMap : DCMSEntityTypeConfiguration<CRM_RETURN>
    {
        public override void Configure(EntityTypeBuilder<CRM_RETURN> builder)
        {
            builder.ToTable("CRM_Returns");
            builder.HasKey(b => b.Id);
            builder.Ignore(b => b.StoreId);
            base.Configure(builder);
        }
    }

    public partial class CRM_ORGMap : DCMSEntityTypeConfiguration<CRM_ORG>
    {
        public override void Configure(EntityTypeBuilder<CRM_ORG> builder)
        {
            builder.ToTable("CRM_Orgs");
            builder.HasKey(b => b.Id);
            builder.Ignore(b => b.StoreId);
            base.Configure(builder);
        }
    }

    public partial class CRM_BPMap : DCMSEntityTypeConfiguration<CRM_BP>
    {
        public override void Configure(EntityTypeBuilder<CRM_BP> builder)
        {
            builder.ToTable("CRM_Bps");
            builder.HasKey(b => b.Id);
            builder.Ignore(b => b.StoreId);
            base.Configure(builder);
        }
    }

    public partial class CRM_ZSNTM0040Map : DCMSEntityTypeConfiguration<CRM_ZSNTM0040>
    {
        public override void Configure(EntityTypeBuilder<CRM_ZSNTM0040> builder)
        {
            builder.ToTable("CRM_Zsntm0040");
            builder.HasKey(b => b.Id);
            builder.Ignore(b => b.StoreId);
            base.Configure(builder);
        }
    }

    public partial class CRM_HEIGHT_CONFMap : DCMSEntityTypeConfiguration<CRM_HEIGHT_CONF>
    {
        public override void Configure(EntityTypeBuilder<CRM_HEIGHT_CONF> builder)
        {
            builder.ToTable("CRM_HeightConfs");
            builder.HasKey(b => b.Id);
            builder.Ignore(b => b.StoreId);
            base.Configure(builder);
        }
    }

    public partial class CRM_BUSTATyMap : DCMSEntityTypeConfiguration<CRM_BUSTAT>
    {
        public override void Configure(EntityTypeBuilder<CRM_BUSTAT> builder)
        {
            builder.ToTable("CRM_Bustats");
            builder.HasKey(b => b.Id);
            builder.Ignore(b => b.StoreId);
            base.Configure(builder);
        }
    }

    public partial class OCMS_CharacterSettingMap : DCMSEntityTypeConfiguration<OCMS_CharacterSetting>
    {
        public override void Configure(EntityTypeBuilder<OCMS_CharacterSetting> builder)
        {
            builder.ToTable("OCMS_CharacterSetting");
            builder.HasKey(b => b.Id);
            builder.Ignore(b => b.StoreId);
            base.Configure(builder);
        }
    }

    public partial class OCMS_ProductsMap : DCMSEntityTypeConfiguration<OCMS_Products>
    {
        public override void Configure(EntityTypeBuilder<OCMS_Products> builder)
        {
            builder.ToTable("OCMS_Products");
            builder.HasKey(b => b.Id);
            builder.Ignore(b => b.StoreId);
            base.Configure(builder);
        }
    }
}
