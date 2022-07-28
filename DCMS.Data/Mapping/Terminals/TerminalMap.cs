using DCMS.Core.Domain.Terminals;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace DCMS.Data.Mapping.Terminals
{

    /// <summary>
    /// 终端映射
    /// </summary>
    public partial class TerminalMap : DCMSEntityTypeConfiguration<Terminal>
    {
        public override void Configure(EntityTypeBuilder<Terminal> builder)
        {
            builder.ToTable("CRM_Terminals");
            builder.HasKey(b => b.Id);
            builder.Ignore(c => c.DataTypeId);
            base.Configure(builder);
        }
    }

    public partial class NewTerminalsMap : DCMSEntityTypeConfiguration<NewTerminal>
    {
        public override void Configure(EntityTypeBuilder<NewTerminal> builder)
        {
            builder.ToTable("crm_newterminals");
            builder.HasKey(b => b.Id);
            base.Configure(builder);
        }
    }
}
