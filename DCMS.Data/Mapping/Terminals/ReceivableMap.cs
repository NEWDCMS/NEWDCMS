using DCMS.Core.Domain.Terminals;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DCMS.Data.Mapping.Terminals
{

    /// <summary>
    /// 应收款映射
    /// </summary>
    public partial class ReceivableMap : DCMSEntityTypeConfiguration<Receivable>
    {
        //public ReceivableMap()
        //{
        //    ToTable("Receivables");
        //    HasKey(o => o.Id);
        //}

        public override void Configure(EntityTypeBuilder<Receivable> builder)
        {
            builder.ToTable("Receivables");
            builder.HasKey(b => b.Id);
            base.Configure(builder);
        }
    }
}
