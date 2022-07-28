using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace DCMS.Web.Factories
{
    public interface IBaseModelFactory
    {
        void PrepareActivityLogTypes(IList<SelectListItem> items, bool withSpecialDefaultItem = true, string defaultItemText = null);
        void PrepareEmailAccounts(IList<SelectListItem> items, bool withSpecialDefaultItem = true, string defaultItemText = null);
        void PrepareLogLevels(IList<SelectListItem> items, bool withSpecialDefaultItem = true, string defaultItemText = null);
        void PrepareStores(IList<SelectListItem> items, bool withSpecialDefaultItem = true, string defaultItemText = null);
        void PrepareUserRoles(IList<SelectListItem> items, bool withSpecialDefaultItem = true, string defaultItemText = null);
    }
}