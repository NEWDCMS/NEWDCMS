using DCMS.Core.Domain.Stores;
using System.Collections.Generic;

namespace DCMS.Core
{
    public interface IStoreContext
    {
        Store CurrentStore { get; }
        IList<Store> Stores { get; }
        int ActiveStoreScopeConfiguration { get; }
    }
}
