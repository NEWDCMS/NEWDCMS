using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Domain.Visit;
using DCMS.Core.Infrastructure.DependencyManagement;
using DCMS.Services.Events;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using DCMS.Services.Caching;

namespace DCMS.Services.Visit
{
    /// <summary>
    /// 拜访线路服务
    /// </summary>
    public partial class LineTierService : BaseService, ILineTierService
    {
        public LineTierService(IServiceGetter getter,
            IStaticCacheManager cacheManager,
            IEventPublisher eventPublisher) : base(getter, cacheManager, eventPublisher)
        {
        }

        #region 拜访线路主表方法
        /// <summary>
        /// 获取所有拜访线路信息
        /// </summary>
        /// <returns></returns>
        public virtual IList<LineTier> GetAll(int? storeId)
        {
            return _cacheManager.Get(DCMSDefaults.LINETIER_ALL_STORE_KEY.FillCacheKey(storeId), () =>
            {
                var query = from c in LineTiersRepository.Table
                            select c;
                if (storeId != null)
                {
                    query = query.Where(c => c.StoreId == storeId);
                }

                var list = query.ToList();
                return list;
            });
        }

        /// <summary>
        /// 绑定线路信息
        /// </summary>
        /// <param name="storeId"></param>
        /// <returns></returns>
        public virtual IList<LineTier> BindLineTier(int? storeId)
        {
            return _cacheManager.Get(DCMSDefaults.BINDLINETIER_ALL_STORE_KEY.FillCacheKey(storeId), () =>
           {
               var query = from c in LineTiersRepository.Table
                           select c;
               if (storeId != null)
               {
                   query = query.Where(c => c.StoreId == storeId);
               }

               var result = query.Select(q => new { Id = q.Id, Name = q.Name }).ToList().Select(x => new LineTier { Id = x.Id, Name = x.Name }).ToList();
               return result;
           });
        }


        /// <summary>
        /// 分页获取拜访线路信息
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public virtual IPagedList<LineTier> GetLineTiers(int? storeId, int? userId, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            if (pageSize >= 50)
                pageSize = 50;
            var query = from p in LineTiersRepository.Table
                        select p;
            if (storeId != null)
            {
                query = query.Where(c => c.StoreId == storeId);
            }

            if (userId.HasValue && userId.Value > 0) 
            {
                //获取用户分配的线路
                var lst = GetUserLineTier(storeId.Value,userId.Value);
                query = query.Where(c=> lst.Contains(c.Id));
            }

            query = query.OrderByDescending(c => c.Id);
            //return new PagedList<LineTier>(query.ToList(), pageIndex, pageSize);
            //总页数
            var totalCount = query.Count();
            var plists = query.Skip(pageIndex * pageSize).Take(pageSize).ToList();
            return new PagedList<LineTier>(plists, pageIndex, pageSize, totalCount);
        }
        /// <summary>
        /// 根据主键Id获取拜访线路信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual LineTier GetLineTierById(int? store, int id, bool isInClude = false)
        {
            if (id == 0)
            {
                return null;
            }

            if (isInClude)
            {
                var query = LineTiersRepository_RO.Table.Include(ap => ap.LineTierOptions);
                return query.FirstOrDefault(l => l.Id == id);
            }
            else
            {
                var query = LineTiersRepository_RO.Table;
                return query.FirstOrDefault(l => l.Id == id);
            }
        }

        public virtual int GetLineTierByName(int store, string name)
        {
            var query = LineTiersRepository.Table;
            if (string.IsNullOrWhiteSpace(name))
            {
                return 0;
            }

            return query.Where(s => s.StoreId == store && s.Name == name).Select(s => s.Id).FirstOrDefault();
        }

        public virtual IList<LineTier> GetLineTiersByIds(int? store, int[] idArr)
        {
            if (idArr == null || idArr.Length == 0)
            {
                return new List<LineTier>();
            }

            var query = from c in LineTiersRepository.Table
                        where c.StoreId == store && idArr.Contains(c.Id)
                        select c;
            var list = query.ToList();

            var result = new List<LineTier>();
            foreach (int id in idArr)
            {
                var model = list.Find(x => x.Id == id);
                if (model != null)
                {
                    result.Add(model);
                }
            }
            return result;
        }

        /// <summary>
        /// 新增拜访线路信息
        /// </summary>
        /// <param name="lineTier"></param>
        public virtual void InsertLineTier(LineTier lineTier)
        {
            if (lineTier == null)
            {
                throw new ArgumentNullException("lineTier");
            }

            var uow = LineTiersRepository.UnitOfWork;
            LineTiersRepository.Insert(lineTier);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityInserted(lineTier);
        }
        /// <summary>
        /// 删除拜访线路信息
        /// </summary>
        /// <param name="lineTier"></param>
        public virtual void DeleteLineTier(LineTier lineTier)
        {
            if (lineTier == null)
            {
                throw new ArgumentNullException("lineTier");
            }

            var uow = LineTiersRepository.UnitOfWork;
            LineTiersRepository.Delete(lineTier);
            uow.SaveChanges();

            //event notification
            _eventPublisher.EntityDeleted(lineTier);
        }
        /// <summary>
        /// 修改拜访线路信息
        /// </summary>
        /// <param name="lineTier"></param>
        public virtual void UpdateLineTier(LineTier lineTier)
        {
            if (lineTier == null)
            {
                throw new ArgumentNullException("lineTier");
            }

            var uow = LineTiersRepository.UnitOfWork;
            LineTiersRepository.Update(lineTier);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityUpdated(lineTier);
        }
        #endregion

        #region 线路访问方法
        /// <summary>
        /// 分页获取线路访问信息
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public virtual IList<LineTierOption> GetLineTierOptions(int? store, int lineTierId)
        {
            var query = from p in LineTierOptionsRepository.TableNoTracking
                        where p.StoreId == store && p.LineTierId == lineTierId
                        select p;
            query = query.OrderBy(c => c.Order);
            return query.ToList();
        }

        public virtual IList<int> GetLineTierOptionsIDS(int? store, int lineTierId)
        {
            var query = from p in LineTierOptionsRepository.TableNoTracking
                        where p.StoreId == store && p.LineTierId == lineTierId
                        select p.Id;
            return query.ToList();
        }

        /// <summary>
        /// 根据主键Id获取线路访问信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual LineTierOption GetLineTierOptionById(int? store, int id)
        {
            if (id == 0)
            {
                return null;
            }

            var query = from p in LineTierOptionsRepository.TableNoTracking
                        where p.StoreId == store && p.Id == id
                        select p;

            return query.FirstOrDefault();
        }

        public virtual bool LineTierOptionExists(int? store, int lineTierId,int terminalId)
        {
            return LineTierOptionsRepository.Table.Where(s => s.StoreId == store && s.LineTierId == lineTierId && s.TerminalId == terminalId)?.Any() ?? false;
        }

        /// <summary>
        /// 查询终端是否分配线路（分配原则，一个终端只能分配一个线路）
        /// </summary>
        /// <param name="store"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual LineTierOption GetLineTierOptionOnlyOne(int? store, int Terminalid)
        {
            if (Terminalid == 0 || store == 0)
            {
                return null;
            }
            var query = from p in LineTierOptionsRepository.Table
                        where p.StoreId == store && p.TerminalId == Terminalid
                        select p;
            return query.FirstOrDefault();
        }
        public virtual IList<LineTierOption> GetLineTierOptionsByIds(int[] idArr)
        {
            if (idArr == null || idArr.Length == 0)
            {
                return new List<LineTierOption>();
            }

            var query = from c in LineTierOptionsRepository.Table
                        where idArr.Contains(c.Id)
                        select c;
            var list = query.ToList();

            var result = new List<LineTierOption>();
            foreach (int id in idArr)
            {
                var model = list.Find(x => x.Id == id);
                if (model != null)
                {
                    result.Add(model);
                }
            }
            return result;
        }
        /// <summary>
        /// 根据终端Id和线路Id获取线路访问信息
        /// </summary>
        /// <param name="terminalId"></param>
        /// <param name="lineTierId"></param>
        /// <returns></returns>
        public virtual LineTierOption GetLineTierOptionByLineTierIdAndTerminalId(int terminalId, int lineTierId)
        {
            var query = from c in LineTierOptionsRepository.Table
                        where c.TerminalId == terminalId && c.LineTierId == lineTierId
                        select c;
            return query.FirstOrDefault();
        }

        /// <summary>
        /// 新增线路访问信息
        /// </summary>
        public virtual void InsertLineTierOption(LineTierOption lineTierOption)
        {
            if (lineTierOption == null)
            {
                throw new ArgumentNullException("lineTierOption");
            }

            var uow = LineTierOptionsRepository.UnitOfWork;
            LineTierOptionsRepository.Insert(lineTierOption);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityInserted(lineTierOption);
        }

        public virtual void InsertLineTierOptions(List<LineTierOption> lineTierOptions)
        {
            if (lineTierOptions == null)
                throw new ArgumentNullException("lineTierOptions");

            var uow = LineTierOptionsRepository.UnitOfWork;
            LineTierOptionsRepository.Insert(lineTierOptions);
            uow.SaveChanges();
            lineTierOptions.ForEach(s => { _eventPublisher.EntityInserted(s); });
        }
        /// <summary>
        /// 删除线路访问信息
        /// </summary>
        public virtual void DeleteLineTierOption(LineTierOption lineTierOption)
        {
            if (lineTierOption == null)
            {
                throw new ArgumentNullException("lineTierOption");
            }

            var uow = LineTierOptionsRepository.UnitOfWork;
            LineTierOptionsRepository.Delete(lineTierOption);
            uow.SaveChanges();

            //event notification
            _eventPublisher.EntityDeleted(lineTierOption);
        }

        public virtual void DeleteLineTierOptions(List<LineTierOption> lineTierOptions)
        {
            if (lineTierOptions == null)
            {
                throw new ArgumentNullException("lineTierOptions");
            }

            var uow = LineTierOptionsRepository.UnitOfWork;
            lineTierOptions.ForEach(p =>
            {
                LineTierOptionsRepository.Delete(p);
            });
            uow.SaveChanges();

            //event notification
            lineTierOptions.ForEach(p =>
            {
                _eventPublisher.EntityDeleted(p);
            });
           
        }

        /// <summary>
        /// 修改线路访问信息
        /// </summary>
        public virtual void UpdateLineTierOption(LineTierOption lineTierOption)
        {
            if (lineTierOption == null)
            {
                throw new ArgumentNullException("lineTierOption");
            }

            var uow = LineTierOptionsRepository.UnitOfWork;
            LineTierOptionsRepository.Update(lineTierOption);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityUpdated(lineTierOption);
        }
        #endregion

        #region 业务员线路分配方法
        /// <summary>
        /// 分页获取业务员线路分配信息
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public virtual IList<UserLineTierAssign> GetUserLineTierAssigns(int userId)
        {
            var query = from p in UserLineTierAssignRepository_RO.Table
                        .Include(l => l.LineTier)
                        .ThenInclude(l => l.LineTierOptions)
                        where p.UserId == userId
                        select p;
            query = query.OrderBy(c => c.Order);
            return query.ToList();
        }
        /// <summary>
        /// 根据主键Id获取业务员线路分配信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual UserLineTierAssign GetUserLineTierAssignById(int? store, int id)
        {
            if (id == 0)
            {
                return null;
            }

            return UserLineTierAssignRepository.ToCachedGetById(id);
        }
        public virtual IList<UserLineTierAssign> GetUserLineTierAssignsByIds(int[] idArr)
        {
            if (idArr == null || idArr.Length == 0)
            {
                return new List<UserLineTierAssign>();
            }

            var query = from c in UserLineTierAssignRepository.Table
                        where idArr.Contains(c.Id)
                        select c;
            var list = query.ToList();

            var result = new List<UserLineTierAssign>();
            foreach (int id in idArr)
            {
                var model = list.Find(x => x.Id == id);
                if (model != null)
                {
                    result.Add(model);
                }
            }
            return result;
        }
        /// <summary>
        /// 根据业务员Id和线路Id获取业务员线路分配信息
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="lineTierId"></param>
        /// <returns></returns>
        public virtual UserLineTierAssign GetUserLineTierAssignByLineTierIdAndUserId(int userId, int lineTierId)
        {
            var query = from c in UserLineTierAssignRepository.Table
                        where c.UserId == userId && c.LineTierId == lineTierId
                        select c;
            return query.FirstOrDefault();
        }

        /// <summary>
        /// 新增业务员线路分配信息
        /// </summary>
        public virtual void InsertUserLineTierAssign(UserLineTierAssign userLineTierAssign)
        {
            if (userLineTierAssign == null)
            {
                throw new ArgumentNullException("userLineTierAssign");
            }

            var uow = UserLineTierAssignRepository.UnitOfWork;
            UserLineTierAssignRepository.Insert(userLineTierAssign);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityInserted(userLineTierAssign);
        }
        /// <summary>
        /// 删除业务员线路分配信息
        /// </summary>
        public virtual void DeleteUserLineTierAssign(UserLineTierAssign userLineTierAssign)
        {
            if (userLineTierAssign == null)
            {
                throw new ArgumentNullException("userLineTierAssign");
            }

            var uow = UserLineTierAssignRepository.UnitOfWork;
            UserLineTierAssignRepository.Delete(userLineTierAssign);
            uow.SaveChanges();

            //event notification
            _eventPublisher.EntityDeleted(userLineTierAssign);
        }
        /// <summary>
        /// 修改业务员线路分配信息
        /// </summary>
        public virtual void UpdateUserLineTierAssign(UserLineTierAssign userLineTierAssign)
        {
            try
            {
                if (userLineTierAssign == null)
                {
                    throw new ArgumentNullException("userLineTierAssign");
                }

                var uow = UserLineTierAssignRepository.UnitOfWork;
                UserLineTierAssignRepository.Update(userLineTierAssign);
                uow.SaveChanges();

                //通知
                _eventPublisher.EntityUpdated(userLineTierAssign);
            }
            catch (Exception)
            {

            }

        }
        #endregion

        #region 获取用户指定的线路
        /// <summary>
        /// 获取用户指定的线路ID
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public IList<int> GetUserLineTier(int storeId, int userId)
        {
            try
            {
                var query = from l in UserLineTierAssignRepository.Table
                            where l.StoreId == storeId && l.UserId == userId
                            select l.LineTierId;
                return query.ToList();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public int GetTerminalLineId(int userId, int terminalId)
        {
            try
            {
                var lst_userLine = from l in UserLineTierAssignRepository_RO.Table
                                   where l.UserId.Equals(userId)
                                   select l.LineTierId;
                var lst = from l in LineTierOptionsRepository_RO.Table
                          where l.TerminalId.Equals(terminalId) && lst_userLine.ToList().Contains(l.LineTierId)
                          select l.LineTierId;
                return lst.FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion
    }
}
