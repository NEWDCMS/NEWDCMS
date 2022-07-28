using DCMS.Core.Domain.Finances;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace DCMS.Data.Mapping.Finances
{

    /// <summary>
    /// 期末结账
    /// </summary>
    public partial class ClosingAccountsMap : DCMSEntityTypeConfiguration<ClosingAccounts>
    {
        public override void Configure(EntityTypeBuilder<ClosingAccounts> builder)
        {
            builder.ToTable("ClosingAccounts");
            builder.HasKey(b => b.Id);
            base.Configure(builder);
        }
    }

    /// <summary>
    /// 成本变化明细汇总
    /// </summary>
    public partial class CostPriceSummeryMap : DCMSEntityTypeConfiguration<CostPriceSummery>
    {
        public override void Configure(EntityTypeBuilder<CostPriceSummery> builder)
        {
            builder.ToTable("CostPriceSummeries");
            builder.HasKey(b => b.Id);
            builder.Ignore(c => c.Records);
            base.Configure(builder);
        }
    }


    /// <summary>
    /// 成本变化明细记录
    /// </summary>
    public partial class CostPriceChangeRecordsMap : DCMSEntityTypeConfiguration<CostPriceChangeRecords>
    {
        public override void Configure(EntityTypeBuilder<CostPriceChangeRecords> builder)
        {
            builder.ToTable("CostPriceChangeRecords");
            builder.HasKey(b => b.Id);
            base.Configure(builder);
        }
    }

}
