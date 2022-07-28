using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DCMS.Data.Mapping
{
    /// <summary>
    /// 表示基本查询类型映射配置
    /// </summary>
    /// <typeparam name="TQuery"></typeparam>
    public partial class DCMSQueryTypeConfiguration<TQuery> : IMappingConfiguration, IEntityTypeConfiguration<TQuery> where TQuery : class
    {

        protected virtual void PostConfigure(EntityTypeBuilder<TQuery> builder)
        {
        }



        public virtual void Configure(EntityTypeBuilder<TQuery> builder)
        {
            //add custom configuration
            PostConfigure(builder);
        }

        public virtual void ApplyConfiguration(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(this);
        }

    }
}