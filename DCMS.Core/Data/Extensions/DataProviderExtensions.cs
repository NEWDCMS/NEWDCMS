namespace DCMS.Core.Data.Extensions
{

    //public static class DataProviderExtensions
    //{
    //    #region Utilities

    //    /// <summary>
    //    /// 获取参数
    //    /// </summary>
    //    /// <param name="dataProvider">Data provider</param>
    //    /// <param name="dbType">Data type</param>
    //    /// <param name="parameterName">Parameter name</param>
    //    /// <param name="parameterValue">Parameter value</param>
    //    /// <returns>Parameter</returns>
    //    private static DbParameter GetParameter(this IDataProvider dataProvider, DbType dbType, string parameterName, object parameterValue)
    //    {
    //        var parameter = dataProvider.GetParameter();
    //        parameter.ParameterName = parameterName;
    //        parameter.Value = parameterValue;
    //        parameter.DbType = dbType;

    //        return parameter;
    //    }

    //    /// <summary>
    //    /// 获取输出参数
    //    /// </summary>
    //    /// <param name="dataProvider">Data provider</param>
    //    /// <param name="dbType">Data type</param>
    //    /// <param name="parameterName">Parameter name</param>
    //    /// <returns>Parameter</returns>
    //    private static DbParameter GetOutputParameter(this IDataProvider dataProvider, DbType dbType, string parameterName)
    //    {
    //        var parameter = dataProvider.GetParameter();
    //        parameter.ParameterName = parameterName;
    //        parameter.DbType = dbType;
    //        parameter.Direction = ParameterDirection.Output;

    //        return parameter;
    //    }

    //    #endregion

    //    #region Methods

    //    /// <summary>
    //    /// 获取字符串参数
    //    /// </summary>
    //    /// <param name="dataProvider">Data provider</param>
    //    /// <param name="parameterName">Parameter name</param>
    //    /// <param name="parameterValue">Parameter value</param>
    //    /// <returns>Parameter</returns>
    //    public static DbParameter GetStringParameter(this IDataProvider dataProvider, string parameterName, string parameterValue)
    //    {
    //        return dataProvider.GetParameter(DbType.String, parameterName, (object)parameterValue ?? DBNull.Value);
    //    }

    //    /// <summary>
    //    /// 获取输出字符串参数
    //    /// </summary>
    //    /// <param name="dataProvider">Data provider</param>
    //    /// <param name="parameterName">Parameter name</param>
    //    /// <returns>Parameter</returns>
    //    public static DbParameter GetOutputStringParameter(this IDataProvider dataProvider, string parameterName)
    //    {
    //        return dataProvider.GetOutputParameter(DbType.String, parameterName);
    //    }

    //    /// <summary>
    //    /// 获取整形参数
    //    /// </summary>
    //    /// <param name="dataProvider">Data provider</param>
    //    /// <param name="parameterName">Parameter name</param>
    //    /// <param name="parameterValue">Parameter value</param>
    //    /// <returns>Parameter</returns>
    //    public static DbParameter GetInt32Parameter(this IDataProvider dataProvider, string parameterName, int? parameterValue)
    //    {
    //        return dataProvider.GetParameter(DbType.Int32, parameterName, parameterValue.HasValue ? (object)parameterValue.Value : DBNull.Value);
    //    }

    //    /// <summary>
    //    /// 获取32位输出整形参数
    //    /// </summary>
    //    /// <param name="dataProvider">Data provider</param>
    //    /// <param name="parameterName">Parameter name</param>
    //    /// <returns>Parameter</returns>
    //    public static DbParameter GetOutputInt32Parameter(this IDataProvider dataProvider, string parameterName)
    //    {
    //        return dataProvider.GetOutputParameter(DbType.Int32, parameterName);
    //    }

    //    /// <summary>
    //    /// 获取布尔参数
    //    /// </summary>
    //    /// <param name="dataProvider">Data provider</param>
    //    /// <param name="parameterName">Parameter name</param>
    //    /// <param name="parameterValue">Parameter value</param>
    //    /// <returns>Parameter</returns>
    //    public static DbParameter GetBooleanParameter(this IDataProvider dataProvider, string parameterName, bool? parameterValue)
    //    {
    //        return dataProvider.GetParameter(DbType.Boolean, parameterName, parameterValue.HasValue ? (object)parameterValue.Value : DBNull.Value);
    //    }

    //    /// <summary>
    //    /// 获取浮点参数
    //    /// </summary>
    //    /// <param name="dataProvider">Data provider</param>
    //    /// <param name="parameterName">Parameter name</param>
    //    /// <param name="parameterValue">Parameter value</param>
    //    /// <returns>Parameter</returns>
    //    public static DbParameter GetDecimalParameter(this IDataProvider dataProvider, string parameterName, decimal? parameterValue)
    //    {
    //        return dataProvider.GetParameter(DbType.Decimal, parameterName, parameterValue.HasValue ? (object)parameterValue.Value : DBNull.Value);
    //    }

    //    /// <summary>
    //    /// 获取时间日期参数
    //    /// </summary>
    //    /// <param name="dataProvider">Data provider</param>
    //    /// <param name="parameterName">Parameter name</param>
    //    /// <param name="parameterValue">Parameter value</param>
    //    /// <returns>Parameter</returns>
    //    public static DbParameter GetDateTimeParameter(this IDataProvider dataProvider, string parameterName, DateTime? parameterValue)
    //    {
    //        return dataProvider.GetParameter(DbType.DateTime, parameterName, parameterValue.HasValue ? (object)parameterValue.Value : DBNull.Value);
    //    }

    //    #endregion
    //}
}