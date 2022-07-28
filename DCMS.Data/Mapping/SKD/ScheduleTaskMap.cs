using DCMS.Core.Domain.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DCMS.Data.Mapping.Tasks
{
    public partial class ScheduleTaskMap : DCMSEntityTypeConfiguration<ScheduleTask>
    {
        //public ScheduleTaskMap()
        //{
        //    ToTable("ScheduleTask");
        //    HasKey(t => t.Id);
        //    Property(t => t.Name).IsRequired();
        //    Property(t => t.Type).IsRequired();
        //}

        public override void Configure(EntityTypeBuilder<ScheduleTask> builder)
        {
            builder.ToTable("ScheduleTask");
            builder.HasKey(b => b.Id);
            builder.Property(t => t.Name).IsRequired();
            //builder.Property(t => t.Type).IsRequired();
            builder.Property(t => t.Enabled).HasColumnType("BIT(1)").HasDefaultValue(false);
            builder.Property(t => t.StopOnError).HasColumnType("BIT(1)").HasDefaultValue(false);
            base.Configure(builder);
        }
    }



}