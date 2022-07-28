using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Domain.Terminals;
using DCMS.Core.Infrastructure.DependencyManagement;
using DCMS.Services.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using DCMS.Services.Caching;

namespace DCMS.Services.Finances
{
    /// <summary>
    /// 应收款明细服务
    /// </summary>
    public partial class ReceivableDetailService : BaseService, IReceivableDetailService
    {
        #region 构造

        public ReceivableDetailService(IServiceGetter getter,
            IStaticCacheManager cacheManager,
            IEventPublisher eventPublisher) : base(getter, cacheManager, eventPublisher)
        {

        }
        #endregion

        #region 方法
        /// <summary>
        /// 获取所有应收款明细信息
        /// </summary>
        /// <returns></returns>
        public virtual IList<ReceivableDetail> GetAll(int? storeId)
        {
            return _cacheManager.Get(DCMSDefaults.RECEIVABLEDETAIL_ALL_STORE_KEY.FillCacheKey(storeId), () =>
            {
                var query = from c in FinanceReceivableDetailRepository.Table
                            select c;

                if (storeId != null)
                {
                    query = query.Where(c => c.StoreId == storeId);
                }

                query = query.OrderByDescending(c => c.CreatedOnUtc);
                var list = query.ToList();
                return list;
            });
        }
        /// <summary>
        /// 分页获取应收款明细信息
        /// </summary>
        /// <param name="searchStr"></param>
        /// <param name="totalCount"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public virtual IPagedList<ReceivableDetail> GetReceivableDetails(string searchStr, int? storeId, int pageIndex, int pageSize)
        {
            if (pageSize >= 50)
                pageSize = 50;
            var query = from p in FinanceReceivableDetailRepository.Table
                        select p;
            //if (!string.IsNullOrEmpty(searchStr))
            //    query = query.Where(t => t.Name.Contains(searchStr));
            if (storeId != null)
            {
                query = query.Where(c => c.StoreId == storeId);
            }

            query = query.OrderByDescending(c => c.CreatedOnUtc);

            //return new PagedList<ReceivableDetail>(query.ToList(), pageIndex, pageSize);
            //总页数
            var totalCount = query.Count();
            var plists = query.Skip(pageIndex * pageSize).Take(pageSize).ToList();
            return new PagedList<ReceivableDetail>(plists, pageIndex, pageSize, totalCount);

        }
        /// <summary>
        /// 根据主键Id获取应收款明细信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual ReceivableDetail GetReceivableDetailById(int? store, int id)
        {
            if (id == 0)
            {
                return null;
            }

            return FinanceReceivableDetailRepository.ToCachedGetById(id);
        }
        /// <summary>
        /// 根据应收款Id获取所有收款记录信息
        /// </summary>
        /// <param name="financeReceivableId"></param>
        /// <returns></returns>
        public virtual IList<ReceivableDetail> GetReceivableDetailsByFinanceReceivableId(int financeReceivableId)
        {
            var query = from c in FinanceReceivableDetailRepository.Table
                        where c.ReceivableId == financeReceivableId
                        select c;
            return query.ToList();
        }
        public virtual IList<ReceivableDetail> GetReceivableDetailsByIds(int[] ids)
        {
            if (ids == null || ids.Length == 0)
            {
                return new List<ReceivableDetail>();
            }

            var query = from c in FinanceReceivableDetailRepository.Table
                        where ids.Contains(c.Id)
                        select c;
            var list = query.ToList();

            var result = new List<ReceivableDetail>();
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
        /// 新增应收款明细信息
        /// </summary>
        /// <param name="financeReceivableDetail"></param>
        public virtual void InsertReceivableDetail(ReceivableDetail financeReceivableDetail)
        {
            if (financeReceivableDetail == null)
            {
                throw new ArgumentNullException("financeReceivableDetail");
            }

            var uow = FinanceReceivableDetailRepository.UnitOfWork;
            FinanceReceivableDetailRepository.Insert(financeReceivableDetail);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityInserted(financeReceivableDetail);
        }
        /// <summary>
        /// 删除应收款明细信息
        /// </summary>
        /// <param name="financeReceivableDetail"></param>
        public virtual void DeleteReceivableDetail(ReceivableDetail financeReceivableDetail)
        {
            if (financeReceivableDetail == null)
            {
                throw new ArgumentNullException("financeReceivableDetail");
            }

            var uow = FinanceReceivableDetailRepository.UnitOfWork;
            FinanceReceivableDetailRepository.Delete(financeReceivableDetail);
            uow.SaveChanges();

            //event notification
            _eventPublisher.EntityDeleted(financeReceivableDetail);
        }
        /// <summary>
        /// 修改应收款明细信息
        /// </summary>
        /// <param name="financeReceivableDetail"></param>
        public virtual void UpdateReceivableDetail(ReceivableDetail financeReceivableDetail)
        {
            if (financeReceivableDetail == null)
            {
                throw new ArgumentNullException("financeReceivableDetail");
            }

            var uow = FinanceReceivableDetailRepository.UnitOfWork;
            FinanceReceivableDetailRepository.Update(financeReceivableDetail);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityUpdated(financeReceivableDetail);
        }
        #endregion
    }
}
