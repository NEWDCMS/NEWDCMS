using DCMS.Core.Domain.WareHouses;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace DCMS.Data.Mapping.WareHouses
{
    public class StockMap : DCMSEntityTypeConfiguration<Stock>
    {

        //public StockMap()
        //{
        //    ToTable("Stocks");
        //    HasKey(o => o.Id);

        //    HasRequired(s => s.Product)
        //        .WithMany(s => s.Stocks)
        //        .HasForeignKey(s => s.ProductId);


        //    HasRequired(s => s.WareHouse)
        //        .WithMany(s => s.Stocks)
        //        .HasForeignKey(s => s.WareHouseId);
        //}

        public override void Configure(EntityTypeBuilder<Stock> builder)
        {
            builder.ToTable("Stocks");
            builder.HasKey(b => b.Id);

            builder.HasOne(s => s.Product)
                .WithMany(s => s.Stocks)
                .HasForeignKey(s => s.ProductId).IsRequired();


            //builder.HasOne(s => s.WareHouse)
            //    .WithMany(s => s.Stocks)
            //    .HasForeignKey(s => s.WareHouseId).IsRequired();

            base.Configure(builder);
        }
    }

    public class StockInOutRecordMap : DCMSEntityTypeConfiguration<StockInOutRecord>
    {


        //public StockInOutRecordMap()
        //{
        //    Ignore(c => c.DirectionEnum);

        //    ToTable("StockInOutRecords");
        //    HasKey(o => o.Id);

        //}

        public override void Configure(EntityTypeBuilder<StockInOutRecord> builder)
        {
            builder.ToTable("StockInOutRecords");
            builder.HasKey(b => b.Id);
            builder.Ignore(c => c.DirectionEnum);
            base.Configure(builder);
        }
    }


    public class StockFlowMap : DCMSEntityTypeConfiguration<StockFlow>
    {

        //public StockFlowMap()
        //{
        //    ToTable("StockFlows");
        //    HasKey(o => o.Id);

        //    HasRequired(s => s.Stock)
        //        .WithMany(s => s.StockFlows)
        //        .HasForeignKey(s => s.StockId);
        //}


        public override void Configure(EntityTypeBuilder<StockFlow> builder)
        {
            builder.ToTable("StockFlows");
            builder.HasKey(b => b.Id);
            builder.HasOne(s => s.Stock)
              .WithMany(s => s.StockFlows)
              .HasForeignKey(s => s.StockId).IsRequired();
            base.Configure(builder);
        }
    }


    /// <summary>
    /// 出入库流水映射
    /// </summary>
    public partial class StockInOutRecordStockFlowMap : DCMSEntityTypeConfiguration<StockInOutRecordStockFlow>
    {
        //public StockInOutRecordStockFlowMap()
        //{
        //    ToTable("StockInOutRecords_StockFlows_Mapping");

        //    HasRequired(o => o.StockFlow)
        //         .WithMany()
        //         .HasForeignKey(o => o.StockFlowId);

        //    HasRequired(o => o.StockInOutRecord)
        //        .WithMany(m => m.StockInOutRecordStockFlows)
        //        .HasForeignKey(o => o.StockInOutRecordId);

        //}

        public override void Configure(EntityTypeBuilder<StockInOutRecordStockFlow> builder)
        {

            builder.ToTable("StockInOutRecords_StockFlows_Mapping");
            builder.HasKey(mapping => new { mapping.StockInOutRecordId, mapping.StockFlowId });

            builder.Property(mapping => mapping.StockInOutRecordId).HasColumnName("StockInOutRecordId");
            builder.Property(mapping => mapping.StockFlowId).HasColumnName("StockFlowId");

            builder.HasOne(mapping => mapping.StockFlow)
                .WithMany()
                .HasForeignKey(mapping => mapping.StockFlowId)
                .IsRequired();

            builder.HasOne(mapping => mapping.StockInOutRecord)
               .WithMany(customer => customer.StockInOutRecordStockFlows)
                .HasForeignKey(mapping => mapping.StockInOutRecordId)
                .IsRequired();



            base.Configure(builder);
        }
    }



    /// <summary>
    /// 用于商品出入库明细记录
    /// </summary>
    public class StockInOutDetailsMap : DCMSEntityTypeConfiguration<StockInOutDetails>
    {
        public override void Configure(EntityTypeBuilder<StockInOutDetails> builder)
        {
            builder.ToTable("StockInOutDetails");
            builder.HasKey(b => b.Id);
            base.Configure(builder);
        }
    }

}
