using DCMS.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DCMS.Data.Mapping
{
    /// <summary>
    /// 表示基本实体映射配置
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public partial class DCMSEntityTypeConfiguration<TEntity> : IMappingConfiguration, IEntityTypeConfiguration<TEntity> where TEntity : BaseEntity
    {

        protected virtual void PostConfigure(EntityTypeBuilder<TEntity> builder)
        {
        }

        public virtual void Configure(EntityTypeBuilder<TEntity> builder)
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