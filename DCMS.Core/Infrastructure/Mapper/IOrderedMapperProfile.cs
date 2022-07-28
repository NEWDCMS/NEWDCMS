namespace DCMS.Core.Infrastructure.Mapper
{
    /// <summary>
    /// Mapper profile 注册接口
    /// </summary>
    public interface IOrderedMapperProfile
    {
        int Order { get; }
    }
}
