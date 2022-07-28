using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Domain.Common;
using DCMS.Core.Domain.Users;
using DCMS.Core.Infrastructure.DependencyManagement;
using DCMS.Services.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using DCMS.Services.Caching;

namespace DCMS.Services.Users
{
    public class BranchService : BaseService, IBranchService
    {
        
        public BranchService(IServiceGetter getter,
            IStaticCacheManager cacheManager,
         
            IEventPublisher eventPublisher) : base(getter, cacheManager, eventPublisher)
        {
            
        }

        public virtual IPagedList<Branch> GetAllBranchs(string branchname = null,
            int store = 0,
            int pageIndex = 0, int pageSize = int.MaxValue)
        {
            if (pageSize >= 50)
                pageSize = 50;
            var key = DCMSDefaults.BINDBRANCH_GETALLBRANCHS_STORE_KEY.FillCacheKey(branchname, store, pageIndex, pageSize);
            return _cacheManager.Get(key, () =>
            {
                var query = BranchRepository.Table;

                if (!string.IsNullOrWhiteSpace(branchname))
                {
                    query = query.Where(c => c.DeptName.Contains(branchname));
                }

                if (store != 0)
                {
                    query = query.Where(c => c.StoreId == store);
                }

                query = query.OrderBy(c => c.ParentId);
                //var branchs = new PagedList<Branch>(query.ToList(), pageIndex, pageSize);
                //return branchs;

                //总页数
                var totalCount = query.Count();
                var plists = query.Skip(pageIndex * pageSize).Take(pageSize).ToList();
                return new PagedList<Branch>(plists, pageIndex, pageSize, totalCount);

            });
        }


        public virtual IList<Branch> GetBranchs(string branchname = null, int store = 0, int offset = 0, int limit = 0)
        {
            var key = DCMSDefaults.BINDBRANCH_GETBRANCHS_STORE_KEY.FillCacheKey(branchname, store, offset, limit);
            return _cacheManager.Get(key, () =>
            {
                var query = BranchRepository.Table;
                if (!string.IsNullOrWhiteSpace(branchname))
                {
                    query = query.Where(c => c.DeptName.Contains(branchname));
                }

                if (store != 0)
                {
                    query = query.Where(c => c.StoreId == store);
                }

                query = query.OrderBy(c => c.ParentId);

                var branchs = query.Skip(offset).Take(limit).ToList();
                return branchs;
            });
        }

        public virtual IList<Branch> GetBranchs()
        {
            var query = from c in BranchRepository.Table select c;
            var branchs = query.ToList();
            return branchs;
        }

        public bool HasChilds(Branch branch)
        {
            var query = from c in BranchRepository.Table where c.ParentId == branch.Id select c;
            return query.ToList().Count > 0;
        }

        public List<Branch> GetBranchsByParentId(int pid)
        {
            var query = from c in BranchRepository.Table where c.ParentId == pid select c;
            return query.ToList();
        }



        public List<Branch> GetBranchsByParentId(int? store, int pid)
        {
            //var query = BranchRepository.Table;
            var query = BranchRepository_RO.TableNoTracking;

            if (store != 0)
            {
                query = query.Where(c => c.StoreId == store);
            }

            query = query.Where(c => c.ParentId == pid);

            return query.ToList();
        }

        /// <summary>
        /// 绑定部门信息
        /// </summary>
        /// <param name="store"></param>
        /// <param name="pid"></param>
        /// <returns></returns>
        public List<Branch> BindBranchsByParentId(int? store, int pid)
        {
            var key = DCMSDefaults.BINDBRANCH_ALL_STORE_KEY.FillCacheKey(store, pid);
            return _cacheManager.Get(key, () =>
             {
                 var query = BranchRepository.Table;

                 if (store != 0)
                 {
                     query = query.Where(c => c.StoreId == store);
                 }

                 query = query.Where(c => c.ParentId == pid);
                 var result = query.Select(q => new { Id = q.Id, DeptName = q.DeptName }).ToList().Select(x => new Branch { Id = x.Id, DeptName = x.DeptName }).ToList();
                 return result;
             });

        }


        public Branch GetBranchsByDeptId(int deptCode)
        {
            var query = from c in BranchRepository.Table where c.DeptCode == deptCode select c;
            return query.ToList().FirstOrDefault();
        }


        public virtual void DeleteBranch(Branch branch)
        {
            if (branch == null)
            {
                throw new ArgumentNullException("branch");
            }

            var uow = BranchRepository.UnitOfWork;
            BranchRepository.Delete(branch);
            uow.SaveChanges();

            //事件通知
            _eventPublisher.EntityDeleted(branch);
        }


        public virtual Branch GetBranchById(int branchId)
        {
            if (branchId == 0)
            {
                return null;
            }

            return BranchRepository.ToCachedGetById(branchId);
        }

        public virtual IList<Branch> GetBranchsByIds(int[] branchIds)
        {
            if (branchIds == null || branchIds.Length == 0)
            {
                return new List<Branch>();
            }

            var query = from c in BranchRepository.Table
                        where branchIds.Contains(c.Id)
                        select c;
            var branchs = query.ToList();
            //sort by passed identifiers
            var sortedBranchs = new List<Branch>();
            foreach (int id in branchIds)
            {
                var branch = branchs.Find(x => x.Id == id);
                if (branch != null)
                {
                    sortedBranchs.Add(branch);
                }
            }
            return sortedBranchs;
        }


        public virtual Branch GetBranchByName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            var query = from c in BranchRepository.Table
                        orderby c.Id
                        where c.DeptName == name
                        select c;
            var branch = query.FirstOrDefault();
            return branch;
        }

        public virtual void InsertBranch(Branch branch)
        {
            if (branch == null)
            {
                throw new ArgumentNullException("branch");
            }

            var uow = BranchRepository.UnitOfWork;
            BranchRepository.Insert(branch);
            uow.SaveChanges();

            //事件通知
            _eventPublisher.EntityInserted(branch);
        }


        public virtual void UpdateBranch(Branch branch)
        {
            if (branch == null)
            {
                throw new ArgumentNullException("branch");
            }

            var uow = BranchRepository.UnitOfWork;
            BranchRepository.Update(branch);
            uow.SaveChanges();

            //事件通知
            _eventPublisher.EntityUpdated(branch);
        }


        public IList<ZTree> GetBranchZTree(int? storeId)
        {
            var key = DCMSDefaults.BINDBRANCH_GETBRANCHZTREE_STORE_KEY.FillCacheKey(storeId.Value);
            return _cacheManager.Get(key, () =>
            {

                List<ZTree> branchs = BranchRepository.Table.Select(c => c).Where(c => (c.Status == true) && c.StoreId == storeId)
               .ToList().OrderBy(c => c.ParentId)
               .Select(c =>
               {
                   return new ZTree()
                   {
                       id = c.Id,
                       pId = c.ParentId,
                       name = c.DeptName,
                       isParent = BranchRepository.Table.Where(m => m.ParentId == c.Id).Count() > 0,
                       open = false
                   };
               }).ToList();
                return branchs;
            });
        }


    }
}

