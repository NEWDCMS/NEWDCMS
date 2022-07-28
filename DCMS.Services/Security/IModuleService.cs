using DCMS.Core;
using DCMS.Core.Domain.Security;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace DCMS.Services.Security
{
	public interface IModuleService
    {
        void DeleteModule(Module module);
        IPagedList<Module> GetAllModules(string modulename = null, int? store = null, bool isShowEnabled = false, int pageIndex = 0, int pageSize = int.MaxValue);

        List<Module> GetModulesByParentId(int? store, int pid, bool isShowEnabled = false);
        List<Module> GetModules();
        List<Module> GetModulesByStore(int? store, bool isShowEnabled = false, Expression<Func<Module, bool>> selector = null);

        List<Module> GetNotPaltformModulesByParentId(List<Module> allModules, int? store, int pid, int? postion, bool isShowEnabled = false);

        Module GetModuleById(int moduleId);
        Module GetModuleByName(string name);
        IList<Module> GetModulesByIds(int[] moduleIds);
        void InsertModule(Module module);
        void UpdateModule(Module module);

        string GetFormattedBreadCrumb(Module module, IList<Module> allModules = null, string separator = "-");
        IList<Module> GetModuleBreadCrumb(Module module, IList<Module> allModules = null);

        List<Module> GetModulesWithPaltform(bool IsPaltform = false);
        List<QueryModule> GetModulePermissionRecords(bool IsPaltform = false);
    }
}