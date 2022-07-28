using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DCMS.Data.Mapping
{

    public partial class SettingMap : DCMSEntityTypeConfiguration<DCMS.Core.Domain.Configuration.Setting>
    {
        //public SettingMap()
        //{
        //    ToTable("Settings");
        //    HasKey(o => o.Id);
        //}

        public override void Configure(EntityTypeBuilder<DCMS.Core.Domain.Configuration.Setting> builder)
        {
            builder.ToTable("Settings");
            builder.HasKey(b => b.Id);
            base.Configure(builder);
        }
    }
}