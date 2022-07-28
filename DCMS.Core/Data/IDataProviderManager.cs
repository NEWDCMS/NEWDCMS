namespace DCMS.Core.Data
{
    /// <summary>
    /// 数据提供管理器接口
    /// </summary>
    public partial interface IDataProviderManager
    {
        #region Properties
        IDataProvider DataProvider { get; }

        //IDBProvider DBProvider { get; }
        #endregion
    }
}