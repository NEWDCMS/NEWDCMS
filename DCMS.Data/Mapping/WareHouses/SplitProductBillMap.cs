using DCMS.Core.Domain.WareHouses;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace DCMS.Data.Mapping.WareHouses
{

    /// <summary>
    /// 用于表示库存商品拆分单
    /// </summary>
    public class SplitProductBillMap : DCMSEntityTypeConfiguration<SplitProductBill>
    {
        public override void Configure(EntityTypeBuilder<SplitProductBill> builder)
        {
            builder.ToTable("SplitProductBills");
            builder.HasKey(b => b.Id);
            builder.Ignore(c => c.Operations);
            builder.Ignore(c => c.BillType);
            builder.Ignore(c => c.BillTypeId);
            builder.Ignore(c => c.Deleted);

            base.Configure(builder);
        }
    }


    /// <summary>
    /// 用于表示库存商品拆分单项目
    /// </summary>
    public partial class SplitProductItemMap : DCMSEntityTypeConfiguration<SplitProductItem>
    {

        //public SplitProductItemMap()
        //{
        //    ToTable("SplitProductItems");
        //    HasKey(o => o.Id);

        //    HasRequired(sao => sao.SplitProductBill)
        //    .WithMany(sa => sa.SplitProductItems)
        //    .HasForeignKey(sao => sao.SplitProductBillId);
        //}

        public override void Configure(EntityTypeBuilder<SplitProductItem> builder)
        {
            builder.ToTable("SplitProductItems");
            builder.HasKey(b => b.Id);

            builder.HasOne(sao => sao.SplitProductBill)
            .WithMany(sa => sa.Items)
            .HasForeignKey(sao => sao.SplitProductBillId).IsRequired();

            base.Configure(builder);
        }
    }


}
