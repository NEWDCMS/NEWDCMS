using DCMS.Core;
using DCMS.Core.Domain.Common;
using DCMS.Core.Domain.Users;
using System.Collections.Generic;

namespace DCMS.Services.Users
{
    public interface IBranchService
    {
        void DeleteBranch(Branch branch);
        Branch GetBranchsByDeptId(int deptCode);
        IList<Branch> GetBranchs();
        IList<Branch> GetBranchs(string branchname = null, int store = 0, int offset = 0, int limit = 0);
        IPagedList<Branch> GetAllBranchs(string branchname = null, int store = 0, int pageIndex = 0, int pageSize = int.MaxValue);
        Branch GetBranchById(int branchId);
        Branch GetBranchByName(string name);
        IList<Branch> GetBranchsByIds(int[] branchIds);
        List<Branch> GetBranchsByParentId(int? store, int pid);
        /// <summary>
        /// 绑定部门信息
        /// </summary>
        /// <param name="store"></param>
        /// <param name="pid"></param>
        /// <returns></returns>
        List<Branch> BindBranchsByParentId(int? store, int pid);
        void InsertBranch(Branch branch);
        void UpdateBranch(Branch branch);
        bool HasChilds(Branch branch);
        IList<ZTree> GetBranchZTree(int? storeId);

    }
}