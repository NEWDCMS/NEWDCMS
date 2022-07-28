using DCMS.Core.Data;
using Microsoft.EntityFrameworkCore;
using System;

namespace DCMS.Data
{
    //public class UnitOfWorkFactory : IUnitOfWorkFactory
    //{
    //    public IUnitOfWork GetUnitOfWork(IServiceProvider serviceProvider, IDBContext context)
    //    {
    //        //Check.NotNull(serviceProvider, nameof(serviceProvider));
    //        //Check.NotNull(context, nameof(context));
    //        return new UnitOfWork(serviceProvider, context);
    //    }
    //}

    public class UnitOfWorkFactory : IUnitOfWorkFactory
    {
        //public IUnitOfWork GetUnitOfWork(DbContext context)
        //{
        //    return new UnitOfWork<DbContext>(context);
        //}
        public IUnitOfWork GetUnitOfWork(IServiceProvider serviceProvider, DbContext context)
        {
            return new UnitOfWork(serviceProvider, context);
        }
    }
}
