using DCMS.Core.Domain.Finances;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace DCMS.Data.Mapping.Finances
{

    /// <summary>
    /// 科目余额(试算表)
    /// </summary>
    public partial class TrialBalanceMap : DCMSEntityTypeConfiguration<TrialBalance>
    {

        //public TrialBalanceMap()
        //{
        //    ToTable("TrialBalances");
        //    HasKey(o => o.Id);
        //}

        public override void Configure(EntityTypeBuilder<TrialBalance> builder)
        {
            builder.ToTable("TrialBalances");
            builder.HasKey(b => b.Id);

            builder.Ignore(b => b.BillId);
            builder.Ignore(b => b.CollectionAmount);

            base.Configure(builder);
        }
    }


    /// <summary>
    /// 利润表
    /// </summary>
    public partial class ProfitSheetMap : DCMSEntityTypeConfiguration<ProfitSheet>
    {

        public override void Configure(EntityTypeBuilder<ProfitSheet> builder)
        {
            builder.ToTable("ProfitSheets");
            builder.HasKey(b => b.Id);

            builder.Ignore(b => b.LineNum);
            builder.Ignore(b => b.BillId);
            builder.Ignore(b => b.CollectionAmount);

            base.Configure(builder);
        }
    }


    /// <summary>
    /// 资产负债表
    /// </summary>
    public partial class BalanceSheetMap : DCMSEntityTypeConfiguration<BalanceSheet>
    {

        public override void Configure(EntityTypeBuilder<BalanceSheet> builder)
        {
            builder.ToTable("BalanceSheets");
            builder.HasKey(b => b.Id);

            builder.Ignore(b => b.LineNum);
            builder.Ignore(b => b.BillId);
            builder.Ignore(b => b.CollectionAmount);

            base.Configure(builder);
        }
    }


}
