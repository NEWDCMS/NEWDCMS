using System;

namespace DCMS.Core.Data
{
    /// <summary>
    /// 无主键实体接口
    /// </summary>
    public interface IEntity
    {
    }

    /// <summary>
    /// 实体接口
    /// </summary>
    public interface IEntity<TKey> : IEntity where TKey : IEquatable<TKey>
    {
        /// <summary>
        /// 主键, 如果使用EF Core, 请将该属性设置为Ignore
        /// </summary>
        TKey Key { get; }
    }
}
