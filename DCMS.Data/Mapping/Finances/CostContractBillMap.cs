using DCMS.Core.Domain.Finances;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace DCMS.Data.Mapping.Finances
{

    /// <summary>
    /// 用于表示费用合同单据
    /// </summary>
    public partial class CostContractBillMap : DCMSEntityTypeConfiguration<CostContractBill>
    {
        public override void Configure(EntityTypeBuilder<CostContractBill> builder)
        {
            builder.ToTable("CostContractBills");
            builder.HasKey(b => b.Id);
            builder.Ignore(c => c.Operations);
            builder.Ignore(c => c.BillType);
            builder.Ignore(c => c.BillTypeId);
            base.Configure(builder);
        }
    }


    /// <summary>
    /// 费用合同单据项目
    /// </summary>
    public partial class CostContractItemMap : DCMSEntityTypeConfiguration<CostContractItem>
    {

        //public CostContractItemMap()
        ////{
        ////    ToTable("CostContractItems");
        ////    HasKey(o => o.Id);

        ////    HasRequired(sao => sao.CostContractBill)
        ////    .WithMany(sa => sa.CostContractItems)
        ////    .HasForeignKey(sao => sao.CostContractBillId);
        ////}

        public override void Configure(EntityTypeBuilder<CostContractItem> builder)
        {
            builder.ToTable("CostContractItems");
            builder.HasKey(b => b.Id);

            builder.HasOne(sao => sao.CostContractBill)
          .WithMany(sa => sa.Items)
          .HasForeignKey(sao => sao.CostContractBillId).IsRequired();

            base.Configure(builder);
        }
    }
}
