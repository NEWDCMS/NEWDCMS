namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// 提供在服务类型已存在时的注入策略
    /// </summary>
    public enum InjectionPolicy
    {
        /// <summary>
        /// 追加
        /// </summary>
        Append = 0,
        /// <summary>
        /// 跳过
        /// </summary>
        Skip = 1,
        /// <summary>
        /// 替代
        /// </summary>
        Replace = 2
    }
}
