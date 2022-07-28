using DCMS.ViewModel.Models.Users;
using System.Collections.Generic;


namespace DCMS.ViewModel.Models.Common
{

    public class OptSelectIDS
    {
        public OptSelectIDS()
        {
            mIds = new List<int>();
            pIds = new List<int>();
        }

        public List<int> mIds { get; set; }
        public List<int> pIds { get; set; }
    }


    public class MenuModel
    {
        public List<ModuleTree<ModuleModel>> MenuTrees { get; set; } = new List<ModuleTree<ModuleModel>>();
    }


    public class BaseModuleTree<T>
    {
        public bool Visible { get; set; }
        public T Module { get; set; }
        public int MaxItems { get; set; } = 0;
        public List<BaseModuleTree<T>> Children { get; set; }
    }

    public class ModuleTree<T> : BaseModuleTree<T>
    {
        public IList<PermissionRecordModel> PermissionRecords { get; set; }
        public new List<ModuleTree<T>> Children { get; set; }
    }

}