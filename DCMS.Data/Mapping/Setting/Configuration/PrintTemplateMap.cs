using DCMS.Core.Domain.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DCMS.Data.Mapping
{
    public partial class PrintTemplateMap : DCMSEntityTypeConfiguration<PrintTemplate>
    {
        public override void Configure(EntityTypeBuilder<PrintTemplate> builder)
        {
            builder.ToTable("PrintTemplates");
            builder.HasKey(b => b.Id);
            builder.Ignore(password => password.EPaperTypes);
            base.Configure(builder);
        }
    }
}
