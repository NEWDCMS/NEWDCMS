using DCMS.Core.Domain.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DCMS.Data.Mapping.Tasks
{
    public partial class QueuedMessageMap : DCMSEntityTypeConfiguration<QueuedMessage>
    {
        //public QueuedMessageMap()
        //{
        //    this.ToTable("QueuedMessage");
        //    this.HasKey(qe => qe.Id);
        //    this.Ignore(c => c.IsRead);
        //    this.Ignore(c => c.MType);
        //    this.Ignore(c => c.BillType);
        //}

        public override void Configure(EntityTypeBuilder<QueuedMessage> builder)
        {
            builder.ToTable("QueuedMessage");
            builder.HasKey(b => b.Id);

            builder.Ignore(c => c.IsRead);
            builder.Ignore(c => c.MType);
            builder.Ignore(c => c.BillType);

            base.Configure(builder);
        }
    }
}