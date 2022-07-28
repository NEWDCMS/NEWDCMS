using Microsoft.EntityFrameworkCore;
using System;


namespace DCMS.Core.Data
{
    /// <summary>
    /// UnitOfWork工厂接口
    /// </summary>
    public interface IUnitOfWorkFactory
    {
        //IUnitOfWork GetUnitOfWork(DbContext context);
        IUnitOfWork GetUnitOfWork(IServiceProvider serviceProvider, DbContext context);
    }



}
