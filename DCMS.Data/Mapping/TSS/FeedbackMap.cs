using DCMS.Core.Domain.TSS;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DCMS.Data.Mapping.TSS
{
    public class FeedbackMap : DCMSEntityTypeConfiguration<Feedback>
    {
        public override void Configure(EntityTypeBuilder<Feedback> builder)
        {
            builder.ToTable("Feedback");
            builder.HasKey(b => b.Id);

            builder.Property(mapping => mapping.FeedbackTyoe).HasColumnName("Type");

            base.Configure(builder);
        }
    }

    public class MarketFeedbackMap : DCMSEntityTypeConfiguration<MarketFeedback>
    {
        public override void Configure(EntityTypeBuilder<MarketFeedback> builder)
        {
            builder.ToTable("MarketFeedback");
            builder.HasKey(b => b.Id);
            base.Configure(builder);
        }
    }
}
