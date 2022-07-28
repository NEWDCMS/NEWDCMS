using DCMS.Core.Domain.Plan;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DCMS.Data.Mapping.Plan
{
    public partial class PercentagePlanMap : DCMSEntityTypeConfiguration<PercentagePlan>
    {
        //public PercentagePlanMap()
        //{
        //    ToTable("PercentagePlan");
        //    HasKey(pr => pr.Id);
        //    Ignore(o => o.PlanType);
        //}

        public override void Configure(EntityTypeBuilder<PercentagePlan> builder)
        {
            builder.ToTable("PercentagePlan");
            builder.HasKey(b => b.Id);
            builder.Ignore(o => o.PlanType);
            base.Configure(builder);
        }
    }


    public partial class PercentageMap : DCMSEntityTypeConfiguration<Percentage>
    {
        //public PercentageMap()
        //{
        //    ToTable("Percentage");
        //    HasKey(pr => pr.Id);
        //    Ignore(o => o.CalCulateMethod);
        //    Ignore(o => o.QuantityCalCulateMethod);
        //    Ignore(o => o.CostingCalCulateMethod);

        //    HasRequired(pr => pr.PercentagePlan)
        //             .WithMany(pro => pro.Percentages)
        //             .HasForeignKey(pro => pro.PercentagePlanId);

        //}

        public override void Configure(EntityTypeBuilder<Percentage> builder)
        {
            builder.ToTable("Percentage");
            builder.HasKey(b => b.Id);

            builder.HasOne(pr => pr.PercentagePlan)
                     .WithMany(pro => pro.Percentages)
                     .HasForeignKey(pro => pro.PercentagePlanId);

            builder.Ignore(o => o.CalCulateMethod);
            builder.Ignore(o => o.QuantityCalCulateMethod);
            builder.Ignore(o => o.CostingCalCulateMethod);

            base.Configure(builder);
        }
    }


    public partial class PercentageRangeOptionMap : DCMSEntityTypeConfiguration<PercentageRangeOption>
    {
        //public PercentageRangeOptionMap()
        //{
        //    ToTable("PercentageRangeOption");
        //    HasKey(pr => pr.Id);

        //    HasRequired(pr => pr.Percentage)
        //        .WithMany(pro => pro.PercentageRangeOptions)
        //        .HasForeignKey(pro => pro.PercentageId);
        //}

        public override void Configure(EntityTypeBuilder<PercentageRangeOption> builder)
        {
            builder.ToTable("PercentageRangeOption");
            builder.HasKey(b => b.Id);

            builder.HasOne(pr => pr.Percentage)
                .WithMany(pro => pro.PercentageRangeOptions)
                .HasForeignKey(pro => pro.PercentageId);

            base.Configure(builder);
        }
    }
}
