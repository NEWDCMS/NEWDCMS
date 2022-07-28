using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Domain.Security;
using DCMS.Core.Infrastructure.DependencyManagement;
using DCMS.Services.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using DCMS.Services.Caching;
using System.Linq.Expressions;

namespace DCMS.Services.Security
{
    public class ModuleService : BaseService, IModuleService
    {
        
        public ModuleService(IServiceGetter getter,
            IStaticCacheManager cacheManager,
           
            IEventPublisher eventPublisher) : base(getter, cacheManager, eventPublisher)
        {
            
        }


        public virtual IPagedList<Module> GetAllModules(string modulename = null,
            int? store = null,
            bool isShowEnabled = false,
            int pageIndex = 0, 
            int pageSize = int.MaxValue)
        {
            if (pageSize >= 50)
                pageSize = 50;
            var key = DCMSDefaults.MODULES_ALLOWED_KEY.FillCacheKey(store, modulename, isShowEnabled, pageIndex, pageSize);
            return _cacheManager.Get(key, () =>
            {
                var query = ModuleRepository.Table;

                if (!string.IsNullOrWhiteSpace(modulename))
                {
                    query = query.Where(c => c.Name.Contains(modulename));
                }

                if (store.HasValue)
                {
                    query = query.Where(c => c.StoreId == store);
                }

                if (isShowEnabled)
                {
                    query = query.Where(c => c.Enabled == true);
                }

                query = query.OrderByDescending(c => c.CreatedOnUtc);

                //总页数
                var totalCount = query.Count();
                var plists = query.Skip(pageIndex * pageSize).Take(pageSize).ToList();
                return new PagedList<Module>(plists, pageIndex, pageSize, totalCount);
            });
        }

        public List<Module> GetModules()
        {
            var key = DCMSDefaults.MODULES_ALLS_KEY.FillCacheKey(0);
            return _cacheManager.Get(key, () =>
            {
                var query = from c in ModuleRepository.Table select c;
                query = query.OrderByDescending(c => c.CreatedOnUtc);
                return query.ToList();
            });
        }




        public List<Module> GetModulesByParentId(int? store, int pid, bool isShowEnabled = false)
        {

            var query = ModuleRepository.Table;
            query = query.Where(c => c.ParentId == pid && c.StoreId == store.Value);

            if (isShowEnabled)
            {
                query = query.Where(c => c.Enabled == true);
            }

            return query.ToList();
        }


        /// <summary>
        /// 根据父类获取非平台创建模块
        /// </summary>
        /// <param name="allModules"></param>
        /// <param name="store"></param>
        /// <param name="pid"></param>
        /// <param name="postion"></param>
        /// <param name="isShowEnabled"></param>
        /// <returns></returns>
        public List<Module> GetNotPaltformModulesByParentId(List<Module> allModules, int? store, int pid, int? postion, bool isShowEnabled = false)
        {
            if (allModules == null)
            {
                return new List<Module>();
            }

            var query = allModules?.AsQueryable();

            query = query.Where(c => c.ParentId == pid);

            if (isShowEnabled)
            {
                query = query.Where(c => c.Enabled == true);
            }

            query = query.Where(c => c.IsPaltform == false);

            if (postion.HasValue)
            {
                query = query.Where(c => c.LayoutPositionId == postion);
            }

            return query.ToList();
        }

		public List<Module> GetModulesByStore(int? store, bool isShowEnabled = false,Expression<Func<Module, bool>> selector = null)
		{
			var query = ModuleRepository.TableNoTracking
                .Where(c => c.StoreId == 0);

            if (isShowEnabled)
            {
                query = query.Where(c => c.Enabled == true);
            }

			if (selector != null)
				query = query.Where(selector);

			var moduls = query.Select(s => new Module
            {
                Name = s.Name,
                Id = s.Id,
                Code = s.Code,
                LinkUrl = s.LinkUrl,
                Enabled = s.Enabled,
                ShowMobile = s.ShowMobile,
                IsPaltform = s.IsPaltform,
                LayoutPositionId = s.LayoutPositionId,
                ParentId = s.ParentId,
                Controller = s.Controller,
                Icon = s.Icon,
                Action = s.Action
            }).OrderBy(s => s.Id);

            var key = DCMSDefaults.MODULES_GETMODULES_BYSTORE_KEY.FillCacheKey(store, isShowEnabled);
            return _cacheManager.Get(key, () => moduls.ToList());
        }


        public virtual void DeleteModule(Module module)
        {
            if (module == null)
            {
                throw new ArgumentNullException("module");
            }

            var uow = ModuleRepository.UnitOfWork;
            ModuleRepository.Delete(module);
            uow.SaveChanges();

            //事件通知
            _eventPublisher.EntityDeleted(module);
        }


        public virtual Module GetModuleById(int moduleId)
        {
            return ModuleRepository.ToCachedGetById(moduleId);
        }


        public virtual IList<Module> GetModulesByIds(int[] moduleIds)
        {
            if (moduleIds == null || moduleIds.Length == 0)
            {
                return new List<Module>();
            }

            var query = from c in ModuleRepository.Table
                        where moduleIds.Contains(c.Id)
                        select c;
            var modules = query.ToList();
            //sort by passed identifiers
            var sortedModules = new List<Module>();
            foreach (int id in moduleIds)
            {
                var module = modules.Find(x => x.Id == id);
                if (module != null)
                {
                    sortedModules.Add(module);
                }
            }
            return sortedModules;
        }


        public virtual Module GetModuleByName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            var query = from c in ModuleRepository.Table
                        orderby c.Id
                        where c.Name == name
                        select c;
            var module = query.FirstOrDefault();
            return module;
        }

        public virtual void InsertModule(Module module)
        {
            if (module == null)
            {
                throw new ArgumentNullException("module");
            }

            var uow = ModuleRepository.UnitOfWork;
            ModuleRepository.Insert(module);
            uow.SaveChanges();

            //事件通知
            _eventPublisher.EntityInserted(module);
        }



        public virtual void UpdateModule(Module module)
        {
            if (module == null)
            {
                throw new ArgumentNullException("module");
            }

            var uow = ModuleRepository.UnitOfWork;
            ModuleRepository.Update(module);
            uow.SaveChanges();

            //事件通知
            _eventPublisher.EntityUpdated(module);
        }



        public virtual string GetFormattedBreadCrumb(Module module, IList<Module> allModules = null, string separator = "-")
        {
            var result = "0";

            var breadcrumb = GetModuleBreadCrumb(module, allModules);
            if (breadcrumb.Count == 0)
            {
                return result;
            }

            for (var i = 0; i <= breadcrumb.Count - 1; i++)
            {
                var mId = breadcrumb[i].Id.ToString();
                result = string.IsNullOrEmpty(result) ? mId : $"{result}{separator}{mId}";
            }
            return result;
        }

        public virtual IList<Module> GetModuleBreadCrumb(Module module, IList<Module> allModules = null)
        {
            if (module == null)
            {
                throw new ArgumentNullException(nameof(module));
            }

            var result = new List<Module>();

            var alreadyProcessedModuleIds = new List<int>();

            while (module != null &&
                module.Enabled &&
                !alreadyProcessedModuleIds.Contains(module.Id))
            {
                result.Add(module);

                alreadyProcessedModuleIds.Add(module.Id);

                module = allModules != null ? allModules.FirstOrDefault(c => c.Id == module.ParentId)
                    : GetModuleById(module.ParentId ?? 0);
            }

            result.Reverse();
            return result;
        }

        public List<Module> GetModulesWithPaltform(bool IsPaltform = false)
        {
            var key = DCMSDefaults.MODULES_ALLOWED_ISPALTFORM_KEY.FillCacheKey(IsPaltform);
            return _cacheManager.Get(key, () =>
            {
                var query = from c in ModuleRepository.Table where c.IsPaltform == IsPaltform select c;
                return query.ToList();
            });
        }

        public List<QueryModule> GetModulePermissionRecords(bool IsPaltform = false)
        {
            var key = DCMSDefaults.MODULES_GETMODULEPERMISSIONRECORDS_ISPALTFORM_KEY.FillCacheKey(IsPaltform);
            return _cacheManager.Get(key, () =>
            {
                var lists = new List<QueryModule>();
                try
                {
                    var modules = GetModulesWithPaltform(IsPaltform);
                    lists = modules.Select(s =>
                    {
                        return new QueryModule()
                        {
                            Id = s.Id,
                            Codes = s.Code.Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries).Select(c => int.Parse(c)).ToList(),
                            Name = s.Name,
                            Permissions = s.Permissions.Select(p => p.Id).ToList(),
                            ModuleRoles = s.ModuleRoles.Select(p => p.UserRole_Id).ToList(),
                        };
                    }).ToList();
                    return lists;
                }
                catch (Exception)
                {
                    return lists;
                }
            });
        }

    }
}

