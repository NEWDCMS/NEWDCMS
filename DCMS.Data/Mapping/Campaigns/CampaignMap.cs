using DCMS.Core.Domain.Campaigns;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DCMS.Data.Mapping.Campaigns
{
    /// <summary>
    /// 促销活动
    /// </summary>
    public partial class CampaignMap : DCMSEntityTypeConfiguration<Campaign>
    {
        //public CampaignMap()
        //{
        //    ToTable("Campaigns");
        //    HasKey(o => o.Id);
        //}
        public override void Configure(EntityTypeBuilder<Campaign> builder)
        {
            builder.ToTable("Campaigns");
            //builder.ToTable(nameof(Campaign));
            builder.HasKey(b => b.Id);
            base.Configure(builder);
        }
    }


    /// <summary>
    ///  促销活动渠道映射
    /// </summary>
    public partial class CampaignChannelMap : DCMSEntityTypeConfiguration<CampaignChannel>
    {

        public override void Configure(EntityTypeBuilder<CampaignChannel> builder)
        {
            builder.ToTable(DCMSMappingDefaults.CampaignChannelsTable);

            builder.HasKey(mapping => new { mapping.CampaignId, mapping.ChannelId });
            builder.Property(mapping => mapping.CampaignId).HasColumnName("CampaignId");
            builder.Property(mapping => mapping.ChannelId).HasColumnName("ChannelId");

            builder.HasOne(mapping => mapping.Campaign)
                .WithMany(customer => customer.CampaignChannels)
                .HasForeignKey(mapping => mapping.CampaignId)
                .IsRequired();

            builder.HasOne(mapping => mapping.Channel)
                .WithMany()
                .HasForeignKey(mapping => mapping.ChannelId)
                .IsRequired();

            base.Configure(builder);
        }
    }


    /// <summary>
    ///  活动购买商品
    /// </summary>
    public partial class CampaignBuyProductMap : DCMSEntityTypeConfiguration<CampaignBuyProduct>
    {
        //public CampaignBuyProductMap()
        //{
        //    ToTable("CampaignBuyProducts");
        //    HasKey(o => o.Id);

        //    //活动和活动商品一对多
        //    HasRequired(o => o.Campaign)
        //        .WithMany(o => o.BuyProducts)
        //        .HasForeignKey(o => o.CampaignId).WillCascadeOnDelete(true);
        //}

        public override void Configure(EntityTypeBuilder<CampaignBuyProduct> builder)
        {
            builder.ToTable("CampaignBuyProducts");
            builder.HasKey(b => b.Id);

            builder.HasOne(b => b.Campaign)
             .WithMany(b => b.BuyProducts)
             .HasForeignKey(b => b.CampaignId)
             .OnDelete(DeleteBehavior.Cascade);

            base.Configure(builder);
        }
    }


    /// <summary>
    /// 活动赠送商品
    /// </summary>
    public partial class CampaignGiveProductMap : DCMSEntityTypeConfiguration<CampaignGiveProduct>
    {
        //public CampaignGiveProductMap()
        //{
        //    ToTable("CampaignGiveProducts");
        //    HasKey(o => o.Id);

        //    //活动和活动商品一对多
        //    HasRequired(o => o.Campaign)
        //        .WithMany(o => o.GiveProducts)
        //        .HasForeignKey(o => o.CampaignId).WillCascadeOnDelete(true);

        //    ////活动商品和商品一对一
        //    //this.HasRequired(o => o.Product)
        //    //    .WithMany()
        //    //    .HasForeignKey(o => o.ProductId);
        //}

        public override void Configure(EntityTypeBuilder<CampaignGiveProduct> builder)
        {
            builder.ToTable("CampaignGiveProducts");
            builder.HasKey(b => b.Id);

            builder.HasOne(b => b.Campaign)
             .WithMany(b => b.GiveProducts)
             .HasForeignKey(b => b.CampaignId)
             .OnDelete(DeleteBehavior.Cascade);

            base.Configure(builder);
        }
    }


}
