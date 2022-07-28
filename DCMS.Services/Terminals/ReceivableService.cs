using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Domain.Terminals;
using DCMS.Core.Infrastructure.DependencyManagement;
using DCMS.Services.Events;
using DCMS.Services.Terminals;
using System;
using System.Collections.Generic;
using System.Linq;
using DCMS.Services.Caching;

namespace DCMS.Services.Finances
{
    /// <summary>
    /// 应收款服务
    /// </summary>
    public partial class ReceivableService : BaseService, IReceivableService
    {
        #region 构造
        private readonly ITerminalService _terminalService;

        public ReceivableService(IServiceGetter getter,
            IStaticCacheManager cacheManager,
            IEventPublisher eventPublisher, ITerminalService terminalService) : base(getter, cacheManager, eventPublisher)
        {
            _terminalService = terminalService;
        }

        #endregion

        #region 方法
        /// <summary>
        /// 获取所有应收款信息
        /// </summary>
        /// <returns></returns>
        public virtual IList<Receivable> GetAll(int? storeId)
        {
            return _cacheManager.Get(DCMSDefaults.RECEIVABLE_ALL_STORE_KEY.FillCacheKey(storeId), () =>
            {
                var query = from c in ReceivablesRepository.Table
                            orderby c.CreatedOnUtc
                            where !c.Deleted
                            select c;
                if (storeId != null)
                {
                    query = query.Where(c => c.StoreId == storeId);
                }

                return query.ToList();
            });
        }
        /// <summary>
        /// 分页获取应收款信息
        /// </summary>
        /// <param name="searchStr"></param>
        /// <param name="totalCount"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public virtual IPagedList<Receivable> GetReceivables(int? terminalId, string terminalName, int? storeId, int pageIndex, int pageSize)
        {
            var query = from p in ReceivablesRepository.Table
                        orderby p.CreatedOnUtc descending
                        where !p.Deleted
                        select p;

            if (storeId != null)
            {
                query = query.Where(c => c.StoreId == storeId);
            }
            if (terminalId != null)
            {
                query = query.Where(c => c.TerminalId == terminalId);
            }
            //客户名称检索
            if (!string.IsNullOrEmpty(terminalName))
            {
                var terminalIds = _terminalService.GetTerminalIds(storeId, terminalName);
                query = query.Where(a => terminalIds.Contains(a.TerminalId));
            }
            //return new PagedList<Receivable>(query.ToList(), pageIndex, pageSize);
            //总页数
            var totalCount = query.Count();
            var plists = query.Skip(pageIndex * pageSize).Take(pageSize).ToList();
            return new PagedList<Receivable>(plists, pageIndex, pageSize, totalCount);
        }
        /// <summary>
        /// 根据主键Id获取应收款信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual Receivable GetReceivableById(int? store, int id)
        {
            if (id == 0)
            {
                return null;
            }

            return ReceivablesRepository.ToCachedGetById(id);
        }
        public virtual IList<Receivable> GetReceivablesByIds(int[] ids)
        {
            if (ids == null || ids.Length == 0)
            {
                return new List<Receivable>();
            }

            var query = from c in ReceivablesRepository.Table
                        where ids.Contains(c.Id)
                        select c;
            var list = query.ToList();

            var result = new List<Receivable>();
            foreach (int id in ids)
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
        /// 新增应收款信息
        /// </summary>
        /// <param name="financeReceivable"></param>
        public virtual void InsertReceivable(Receivable financeReceivable)
        {
            if (financeReceivable == null)
            {
                throw new ArgumentNullException("financeReceivable");
            }

            var uow = ReceivablesRepository.UnitOfWork;
            ReceivablesRepository.Insert(financeReceivable);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityInserted(financeReceivable);
        }
        /// <summary>
        /// 删除应收款信息
        /// </summary>
        /// <param name="financeReceivable"></param>
        public virtual void DeleteReceivable(Receivable financeReceivable)
        {
            if (financeReceivable == null)
            {
                throw new ArgumentNullException("financeReceivable");
            }

            var uow = ReceivablesRepository.UnitOfWork;
            ReceivablesRepository.Delete(financeReceivable);
            uow.SaveChanges();
            _cacheManager.RemoveByPrefix(BaseEntity.GetEntityCacheKey(typeof(Receivable), financeReceivable.Id));
            //event notification
            _eventPublisher.EntityDeleted(financeReceivable);
        }
        /// <summary>
        /// 修改应收款信息
        /// </summary>
        /// <param name="financeReceivable"></param>
        public virtual void UpdateReceivable(Receivable financeReceivable)
        {
            if (financeReceivable == null)
            {
                throw new ArgumentNullException("financeReceivable");
            }

            var uow = ReceivablesRepository.UnitOfWork;
            ReceivablesRepository.Update(financeReceivable);
            uow.SaveChanges();
            _cacheManager.RemoveByPrefix(BaseEntity.GetEntityCacheKey(typeof(Receivable), financeReceivable.Id));
             //通知
             _eventPublisher.EntityUpdated(financeReceivable);
        }
        #endregion
    }
}
