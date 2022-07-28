using DCMS.Core.Domain.WareHouses;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace DCMS.Data.Mapping.WareHouses
{

    /// <summary>
    /// 用于表示商品报损单
    /// </summary>
    public class ScrapProductBillMap : DCMSEntityTypeConfiguration<ScrapProductBill>
    {
        public override void Configure(EntityTypeBuilder<ScrapProductBill> builder)
        {
            builder.ToTable("ScrapProductBills");
            builder.HasKey(b => b.Id);
            builder.Ignore(c => c.Operations);
            builder.Ignore(c => c.BillType);
            builder.Ignore(c => c.BillTypeId);
            builder.Ignore(c => c.Deleted);

            base.Configure(builder);
        }
    }


    /// <summary>
    /// 商品报损单项目
    /// </summary>
    public partial class ScrapProductItemMap : DCMSEntityTypeConfiguration<ScrapProductItem>
    {

        //public ScrapProductItemMap()
        //{
        //    ToTable("ScrapProductItems");
        //    HasKey(o => o.Id);

        //    HasRequired(sao => sao.ScrapProductBill)
        //    .WithMany(sa => sa.ScrapProductItems)
        //    .HasForeignKey(sao => sao.ScrapProductBillId);
        //}

        public override void Configure(EntityTypeBuilder<ScrapProductItem> builder)
        {
            builder.ToTable("ScrapProductItems");
            builder.HasKey(b => b.Id);

            builder.HasOne(sao => sao.ScrapProductBill)
             .WithMany(sa => sa.Items)
             .HasForeignKey(sao => sao.ScrapProductBillId).IsRequired();

            base.Configure(builder);
        }
    }


}
