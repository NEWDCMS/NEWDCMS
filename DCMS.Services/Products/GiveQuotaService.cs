using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Data;
using DCMS.Core.Domain.Products;
using DCMS.Core.Infrastructure.DependencyManagement;
using DCMS.Services.Events;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using DCMS.Services.Caching;
using DCMS.Core.Domain.Finances;

namespace DCMS.Services.Products
{

    public partial class GiveQuotaService : BaseService, IGiveQuotaService
    {

        private readonly IProductService _productService;
        private readonly ISpecificationAttributeService _specificationAttributeService;

        public GiveQuotaService(IStaticCacheManager cacheManager,
            IServiceGetter getter,
            IEventPublisher eventPublisher,
            IProductService productService,
            ISpecificationAttributeService specificationAttributeService
            ) : base(getter, cacheManager, eventPublisher)
        {
            _productService = productService;
            _specificationAttributeService = specificationAttributeService;
        }

        #region 方法

        /// <summary>
        ///  删除
        /// </summary>
        /// <param name="giveQuotas"></param>
        public virtual void DeleteGiveQuota(GiveQuota giveQuotas)
        {
            if (giveQuotas == null)
            {
                throw new ArgumentNullException("giveQuotas");
            }

            var uow = GiveQuotaRepository.UnitOfWork;
            GiveQuotaRepository.Delete(giveQuotas);
            uow.SaveChanges();

            //event notification
            _eventPublisher.EntityDeleted(giveQuotas);
        }

        /// <summary>
        /// 获取全部
        /// </summary>
        /// <param name="name"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public virtual IPagedList<GiveQuota> GetAllGiveQuotas(int? store, int? year, int? userId, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            if (pageSize >= 50)
                pageSize = 50;
            var query = GiveQuotaRepository.Table;

            if (store.HasValue && store.Value != 0)
            {
                query = query.Where(gq => gq.StoreId == store.Value);
            }

            if (userId.HasValue)
            {
                query = query.Where(gq => gq.UserId == userId.Value);
            }

            if (year.HasValue)
            {
                query = query.Where(gq => gq.Year == year.Value);
            }

            //var giveQuotas = new PagedList<GiveQuota>(query.ToList(), pageIndex, pageSize);
            //return giveQuotas;

            //总页数
            var totalCount = query.Count();
            var plists = query.Skip(pageIndex * pageSize).Take(pageSize).ToList();
            return new PagedList<GiveQuota>(plists, pageIndex, pageSize, totalCount);

        }

        /// <summary>
        /// 获取全部
        /// </summary>
        /// <returns></returns>
        public virtual IList<GiveQuota> GetAllGiveQuotas(int? store)
        {
            var key = DCMSDefaults.GIVEQUOTA_ALL_KEY.FillCacheKey(store);
            return _cacheManager.Get(key, () =>
            {
                var query = from s in GiveQuotaRepository.Table
                            where s.StoreId == store.Value
                            orderby s.Id
                            select s;
                var giveQuota = query.ToList();
                return giveQuota;
            });
        }

        /// <summary>
        /// 获取全部
        /// </summary>
        /// <returns></returns>
        public virtual IList<GiveQuota> GetAllGiveQuotas()
        {
            var key = DCMSDefaults.GIVEQUOTA_ALL_KEY.FillCacheKey(0);
            return _cacheManager.Get(key, () =>
            {
                var query = from s in GiveQuotaRepository.Table
                            orderby s.Id
                            select s;
                var giveQuota = query.ToList();
                return giveQuota;
            });
        }

        /// <summary>
        /// 获取赠品额度
        /// </summary>
        /// <param name="giveQuotasId"></param>
        /// <returns></returns>
        public virtual GiveQuota GetGiveQuotaById(int? store, int giveQuotasId)
        {
            if (giveQuotasId == 0)
            {
                return null;
            }
            return GiveQuotaRepository.ToCachedGetById(giveQuotasId);
        }

        /// <summary>
        /// 获取赠品额度
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="userId"></param>
        /// <param name="year"></param>
        /// <returns></returns>
        public virtual GiveQuota GetGiveQuotaByStoreIdUserIdYear(int storeId, int userId, int year)
        {
            if (storeId == 0 || userId == 0 || year == 0)
            {
                return null;
            }

            var key = DCMSDefaults.GIVEQUOTA_BY_STORE_USER_YEAR_KEY.FillCacheKey(storeId, userId, year);
            return _cacheManager.Get(key, () =>
            {
                var query = from a in GiveQuotaRepository.Table
                            where a.StoreId == storeId
                            && a.UserId == userId
                            && a.Year == year
                            select a;
                return query.FirstOrDefault();
            });
        }


        public virtual IList<GiveQuota> GetGiveQuotas(int? userId, int? year)
        {
            var query = from c in GiveQuotaRepository.Table
                        where c.UserId == userId && c.Year == year
                        select c;
            var giveQuotas = query.ToList();
            return giveQuotas;
        }


        public virtual IList<GiveQuota> GetGiveQuotasByIds(int[] sIds)
        {
            if (sIds == null || sIds.Length == 0)
            {
                return new List<GiveQuota>();
            }

            var query = from c in GiveQuotaRepository.Table
                        where sIds.Contains(c.Id)
                        select c;
            var giveQuota = query.ToList();

            var sortedGiveQuotas = new List<GiveQuota>();
            foreach (int id in sIds)
            {
                var giveQuotas = giveQuota.Find(x => x.Id == id);
                if (giveQuotas != null)
                {
                    sortedGiveQuotas.Add(giveQuotas);
                }
            }
            return sortedGiveQuotas;
        }

        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="giveQuotas"></param>
        public virtual void InsertGiveQuota(GiveQuota giveQuotas)
        {
            if (giveQuotas == null)
            {
                throw new ArgumentNullException("giveQuotas");
            }

            var uow = GiveQuotaRepository.UnitOfWork;
            GiveQuotaRepository.Insert(giveQuotas);
            uow.SaveChanges();

            //event notification
            _eventPublisher.EntityInserted(giveQuotas);
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="giveQuotas"></param>
        public virtual void UpdateGiveQuota(GiveQuota giveQuotas)
        {
            if (giveQuotas == null)
            {
                throw new ArgumentNullException("giveQuotas");
            }

            var uow = GiveQuotaRepository.UnitOfWork;
            GiveQuotaRepository.Update(giveQuotas);
            uow.SaveChanges();

            //event notification
            _eventPublisher.EntityUpdated(giveQuotas);
        }

        #endregion

        #region GiveQuotaOption

        /// <summary>
        /// 获取项目
        /// </summary>
        /// <param name="giveQuotaOptionId"></param>
        /// <returns></returns>
        public virtual GiveQuotaOption GetGiveQuotaOptionById(int giveQuotaOptionId)
        {
            if (giveQuotaOptionId == 0)
            {
                return null;
            }
            return GiveQuotaOptionRepository.GetById(giveQuotaOptionId);
            //return GiveQuotaOptionRepository.ToCachedGetById(giveQuotaOptionId);
        }

        /// <summary>
        /// 获取赠品额度 这里实时额度，不缓存
        /// </summary>
        /// <param name="idArr"></param>
        /// <returns></returns>
        public virtual IList<GiveQuotaOption> GetGiveQuotaOptionsByIds(int[] idArr)
        {
            if (idArr == null || idArr.Length == 0)
            {
                return new List<GiveQuotaOption>();
            }

            var query = from c in GiveQuotaOptionRepository.Table
                        where idArr.Contains(c.Id)
                        select c;
            var list = query.ToList();
            return list;
        }


        public virtual IList<GiveQuotaOption> GetGiveQuotaOptionByQuotaId(int? giveQuotaId)
        {
            var query = from gqo in GiveQuotaOptionRepository.Table
                        where gqo.GiveQuotaId == giveQuotaId
                        orderby gqo.Id
                        select gqo;
            var giveQuotaOptions = query.ToList();
            return giveQuotaOptions;
        }



        /// <summary>
        /// 获取全部项目
        /// </summary>
        /// <param name="giveId"></param>
        /// <param name="productId"></param>
        /// <returns></returns>
        public virtual IList<GiveQuotaOption> GetAllGiveQuotaOptions(int? giveId)
        {
            var query = from gqo in GiveQuotaOptionRepository.Table
                        orderby gqo.Id
                        select gqo;
            var giveQuotaOptions = query.ToList();
            return giveQuotaOptions;
        }

        /// <summary>
        /// 获取主管额度
        /// </summary>
        /// <param name="year">日期</param>
        /// <param name="leaderId">主管id</param>
        /// <returns></returns>
        public virtual IList<GiveQuotaOption> GetGiveQuotaOptions(int year, int leaderId)
        {
            var query = from a in GiveQuotaRepository.Table
                        join b in GiveQuotaOptionRepository.Table on a.Id equals b.GiveQuotaId
                        where a.Year == year
                        && a.UserId == leaderId
                        select b;
            return query.ToList();

        }

        /// <summary>
        /// 获取全部项目
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="year"></param>
        /// <returns></returns>
        public virtual IList<GiveQuotaOption> GetGiveQuotaOptions(int? userId, int? year)
        {
            var gqIds = GiveQuotaRepository.Table.Where(gq => gq.UserId == userId && gq.Year == year).Select(gq => gq.Id).ToList();
            var query1 = from gqo in GiveQuotaOptionRepository.Table
                         join gq in gqIds
                         on gqo.GiveQuotaId equals gq
                         select gqo;

            var options = query1.ToList();
            return options;
        }

        /// <summary>
        ///  移除项目
        /// </summary>
        /// <param name="giveQuotaOption"></param>
        public virtual void DeleteGiveQuotaOption(GiveQuotaOption giveQuotaOption)
        {
            if (giveQuotaOption == null)
            {
                throw new ArgumentNullException("giveQuotaOption");
            }

            var uow = GiveQuotaOptionRepository.UnitOfWork;
            GiveQuotaOptionRepository.Delete(giveQuotaOption);
            uow.SaveChanges();

            //event notification
            _eventPublisher.EntityDeleted(giveQuotaOption);
        }

        public virtual void InsertGiveQuotaOption(GiveQuotaOption giveQuotaOption)
        {
            if (giveQuotaOption == null)
            {
                throw new ArgumentNullException("giveQuotaOption");
            }

            var uow = GiveQuotaOptionRepository.UnitOfWork;
            GiveQuotaOptionRepository.Insert(giveQuotaOption);
            uow.SaveChanges();

            //event notification
            _eventPublisher.EntityInserted(giveQuotaOption);
        }

        public virtual void UpdateGiveQuotaOption(GiveQuotaOption giveQuotaOption)
        {
            if (giveQuotaOption == null)
            {
                throw new ArgumentNullException("giveQuotaOption");
            }

            var uow = GiveQuotaOptionRepository.UnitOfWork;
            GiveQuotaOptionRepository.Update(giveQuotaOption);
            uow.SaveChanges();

            //event notification
            _eventPublisher.EntityUpdated(giveQuotaOption);
        }


        #endregion

        #region 赠品记录

        /// <summary>
        ///  删除
        /// </summary>
        /// <param name="giveQuotas"></param>
        public virtual void DeleteGiveQuotaRecords(GiveQuotaRecords giveQuotaRecords)
        {
            if (giveQuotaRecords == null)
            {
                throw new ArgumentNullException("giveQuotaRecords");
            }

            var uow = GiveQuotaRecordsRepository.UnitOfWork;
            GiveQuotaRecordsRepository.Delete(giveQuotaRecords);
            uow.SaveChanges();

            _eventPublisher.EntityDeleted(giveQuotaRecords);
        }

        /// <summary>
        /// 获取全部
        /// </summary>
        /// <param name="store"></param>
        /// <param name="businessUserId"></param>
        /// <param name="productid"></param>
        /// <param name="customerId"></param>
        /// <param name="catagoryId"></param>
        /// <param name="costingCalCulateMethodId"></param>
        /// <param name="giveTypeId"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public virtual IPagedList<GiveQuotaRecords> GetAllGiveQuotaRecords(int? store, int? businessUserId, int? productid, int? customerId, int? catagoryId, int? costingCalCulateMethodId, int? giveTypeId, DateTime? start, DateTime? end, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            if (pageSize >= 50)
                pageSize = 50;
            var query = GiveQuotaRecordsRepository.Table;

            if (store.HasValue && store.Value != 0)
            {
                query = query.Where(gr => gr.StoreId == store.Value);
            }

            if (businessUserId.HasValue && businessUserId.Value != 0)
            {
                query = query.Where(gr => gr.BusinessUserId == businessUserId.Value);
            }

            if (productid.HasValue && productid.Value != 0)
            {
                query = query.Where(gr => gr.ProductId == productid.Value);
            }

            if (customerId.HasValue && customerId.Value != 0)
            {
                query = query.Where(gr => gr.TerminalId == customerId.Value);
            }

            if (catagoryId.HasValue && catagoryId.Value != 0)
            {
                query = query.Where(gr => gr.CategoryId == catagoryId.Value);
            }

            if (costingCalCulateMethodId.HasValue && costingCalCulateMethodId.Value != 0)
            {
                query = query.Where(gr => gr.CostingCalCulateMethodId == costingCalCulateMethodId.Value);
            }

            if (giveTypeId.HasValue && giveTypeId.Value != 0)
            {
                query = query.Where(gr => gr.GiveTypeId == giveTypeId.Value);
            }

            if (start.HasValue)
            {
                var startTime = DateTime.Parse(start.Value.ToString("yyyy-MM-dd 00:00:00"));
                query = query.Where(gr => gr.CreatedOnUtc >= startTime);
            }

            if (end.HasValue)
            {
                var endTime = DateTime.Parse(end.Value.ToString("yyyy-MM-dd 23:59:59"));
                query = query.Where(gr => gr.CreatedOnUtc <= endTime);
            }

            query = query.OrderByDescending(gr => gr.CreatedOnUtc);

            //总页数
            var totalCount = query.Count();
            var plists = query.Skip(pageIndex * pageSize).Take(pageSize).ToList();
            return new PagedList<GiveQuotaRecords>(plists, pageIndex, pageSize, totalCount);

        }


        /// <summary>
        /// 获取全部赠品汇总
        /// </summary>
        /// <param name="store"></param>
        /// <param name="businessUserId"></param>
        /// <param name="productid"></param>
        /// <param name="productName"></param>
        /// <param name="customerId"></param>
        /// <param name="terminalName"></param>
        /// <param name="catagoryId"></param>
        /// <param name="costingCalCulateMethodId"></param>
        /// <param name="giveTypeId"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public virtual IPagedList<GiveQuotaRecordsSummery> GetAllGiveQuotaRecordsSummeries(int? store, int? businessUserId, int? productid, string productName, int? customerId, string terminalName, int? catagoryId, int? costingCalCulateMethodId, int? giveTypeId, DateTime? start, DateTime? end, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var recordsSummeries = new List<GiveQuotaRecordsSummery>();
            try
            {

                productName = CommonHelper.FilterSQLChar(productName);

                if (pageSize >= 50)
                    pageSize = 50;

                //MYSQL
                string whereString1 = $"";
                string whereString2 = $"";


                //whereString1
                if (businessUserId.HasValue && businessUserId.Value > 0)
                {
                    whereString1 += $" and s.BusinessUserId = {businessUserId} ";
                }

                if (productid.HasValue && productid.Value > 0)
                {
                    whereString1 += $" and si.ProductId = {productid} ";
                }

                if (customerId.HasValue && customerId.Value > 0)
                {
                    whereString1 += $" and s.TerminalId = {customerId}";
                }

                if (start.HasValue)
                {
                    var startTime = start.Value.ToString("yyyy-MM-dd 00:00:00");
                    whereString1 += $" and s.CreatedOnUtc >= '{startTime}'";
                }

                if (end.HasValue)
                {
                    var endTime = end.Value.ToString("yyyy-MM-dd 23:59:59");
                    whereString1 += $" and s.CreatedOnUtc <='{endTime}' ";
                }

                //whereString2

                if (catagoryId.HasValue && catagoryId.Value > 0)
                {
                    whereString2 += $" and a.CategoryId = {catagoryId} ";
                }

                if (terminalName != null)
                {
                    whereString2 += $" and a.TerminalName like '%{terminalName}%' ";
                }
                if (productName != null)
                {
                    whereString2 += $" and a.ProductName like '%{productName}%' ";
                }

                //string mySqlString = @"SELECT 
                //                    a.*,
                //                    IFNULL((SELECT IFNULL(t.`Name`, a.SmallUnitId) FROM dcms.SpecificationAttributeOptions AS t WHERE id = a.SmallUnitId AND t.StoreId = {0} and a.SmallUnitId >0),'') AS SUnitName,
                //                    IFNULL((SELECT IFNULL(t.`Name`, a.StrokeUnitId) FROM dcms.SpecificationAttributeOptions AS t WHERE id = a.StrokeUnitId AND t.StoreId = {0} and a.StrokeUnitId >0),'') AS MUnitName,
                //                    IFNULL((SELECT IFNULL(t.`Name`, a.BigUnitId) FROM dcms.SpecificationAttributeOptions AS t WHERE id = a.BigUnitId AND t.StoreId = {0} and a.BigUnitId >0),'') AS BUnitName
                //                FROM
                //                    ((SELECT 
                //                            0 AS Gtype,
                //                            s.TerminalId,
                //                            (SELECT  IFNULL(t.`Name`, '未知') FROM dcms_crm.CRM_Terminals AS t WHERE id = s.TerminalId AND t.StoreId = {0}) AS TerminalName,
                //                            (SELECT  IFNULL(t.`Code`, '000') FROM dcms_crm.CRM_Terminals AS t WHERE id = s.TerminalId AND t.StoreId = {0}) AS TerminalCode,
                //                            s.BusinessUserId,
                //                            IFNULL((SELECT  IFNULL(t.`UserRealName`, s.BusinessUserId) FROM auth.Users AS t WHERE id = s.BusinessUserId AND t.StoreId = {0}), s.BusinessUserId) AS BusinessUserName,
                //                            si.ProductId,
                //                            (SELECT  IFNULL(t.`Name`, si.ProductId) FROM dcms.Products AS t WHERE id = si.ProductId AND t.StoreId = {0}) AS ProductName,
                //                            (SELECT  t.CategoryId FROM dcms.Products AS t WHERE id = si.ProductId AND t.StoreId = {0}) AS CategoryId,
                //                            (SELECT  t.BigQuantity FROM dcms.Products AS t WHERE id = si.ProductId AND t.StoreId = {0}) AS BigQuantity,
                //                            (SELECT  t.SmallUnitId FROM dcms.Products AS t WHERE id = si.ProductId AND t.StoreId = {0}) AS SmallUnitId,
                //                            (SELECT  t.StrokeUnitId FROM dcms.Products AS t WHERE id = si.ProductId AND t.StoreId = {0}) AS StrokeUnitId,
                //                            (SELECT  t.BigUnitId FROM dcms.Products AS t WHERE id = si.ProductId AND t.StoreId = {0}) AS BigUnitId,
                //                            si.UnitId,
                //                            (SELECT IFNULL(t.`Name`, si.UnitId) FROM dcms.SpecificationAttributeOptions AS t WHERE id = si.UnitId AND t.StoreId = {0}) AS UnitName,
                //                            si.Quantity,
                //                            si.CostAmount
                //                    FROM dcms.SaleBills AS s  INNER JOIN dcms.SaleItems AS si ON s.Id = si.SaleBillId
                //                    WHERE
                //                            s.StoreId = {0} 
                //                            AND si.IsGifts = TRUE AND si.CampaignId = 0  AND CostContractId = 0 {1}) UNION (SELECT 
                //                            1 AS Gtype,
                //                   s.TerminalId,
                //                            (SELECT  IFNULL(t.`Name`, '未知') FROM dcms_crm.CRM_Terminals AS t WHERE id = s.TerminalId AND t.StoreId = {0}) AS TerminalName,
                //                            (SELECT  IFNULL(t.`Code`, '000') FROM dcms_crm.CRM_Terminals AS t WHERE id = s.TerminalId AND t.StoreId = {0}) AS TerminalCode,
                //                            s.BusinessUserId,
                //                            IFNULL((SELECT  IFNULL(t.`UserRealName`, s.BusinessUserId) FROM auth.Users AS t WHERE id = s.BusinessUserId AND t.StoreId = {0}), s.BusinessUserId) AS BusinessUserName,
                //                            si.ProductId,
                //                            (SELECT  IFNULL(t.`Name`, si.ProductId) FROM dcms.Products AS t WHERE id = si.ProductId AND t.StoreId = {0}) AS ProductName,
                //                            (SELECT  t.CategoryId FROM dcms.Products AS t WHERE id = si.ProductId AND t.StoreId = {0}) AS CategoryId,
                //                            (SELECT  t.BigQuantity FROM dcms.Products AS t WHERE id = si.ProductId AND t.StoreId = {0}) AS BigQuantity,
                //                            (SELECT  t.SmallUnitId FROM dcms.Products AS t WHERE id = si.ProductId AND t.StoreId = {0}) AS SmallUnitId,
                //                            (SELECT  t.StrokeUnitId FROM dcms.Products AS t WHERE id = si.ProductId AND t.StoreId = {0}) AS StrokeUnitId,
                //                            (SELECT  t.BigUnitId FROM dcms.Products AS t WHERE id = si.ProductId AND t.StoreId = {0}) AS BigUnitId,
                //                            si.UnitId,
                //                            (SELECT IFNULL(t.`Name`, si.UnitId) FROM dcms.SpecificationAttributeOptions AS t WHERE id = si.UnitId AND t.StoreId = {0}) AS UnitName,
                //                            si.Quantity,
                //                            si.CostAmount
                //                    FROM dcms.SaleBills AS s  INNER JOIN dcms.SaleItems AS si ON s.Id = si.SaleBillId
                //                    WHERE
                //                            s.StoreId = {0} 
                //                            AND si.IsGifts = TRUE AND si.CampaignId > 0 AND CostContractId = 0 {1}) UNION (SELECT 
                //                            2 AS Gtype,
                //                            s.TerminalId,
                //                            (SELECT  IFNULL(t.`Name`, '未知') FROM dcms_crm.CRM_Terminals AS t WHERE id = s.TerminalId AND t.StoreId = {0}) AS TerminalName,
                //                            (SELECT  IFNULL(t.`Code`, '000') FROM dcms_crm.CRM_Terminals AS t WHERE id = s.TerminalId AND t.StoreId = {0}) AS TerminalCode,
                //                            s.BusinessUserId,
                //                            IFNULL((SELECT  IFNULL(t.`UserRealName`, s.BusinessUserId) FROM auth.Users AS t WHERE id = s.BusinessUserId AND t.StoreId = {0}), s.BusinessUserId) AS BusinessUserName,
                //                            si.ProductId,
                //                            (SELECT  IFNULL(t.`Name`, si.ProductId) FROM dcms.Products AS t WHERE id = si.ProductId AND t.StoreId = {0}) AS ProductName,
                //                            (SELECT  t.CategoryId FROM dcms.Products AS t WHERE id = si.ProductId AND t.StoreId = {0}) AS CategoryId,
                //                            (SELECT  t.BigQuantity FROM dcms.Products AS t WHERE id = si.ProductId AND t.StoreId = {0}) AS BigQuantity,
                //                            (SELECT  t.SmallUnitId FROM dcms.Products AS t WHERE id = si.ProductId AND t.StoreId = {0}) AS SmallUnitId,
                //                            (SELECT  t.StrokeUnitId FROM dcms.Products AS t WHERE id = si.ProductId AND t.StoreId = {0}) AS StrokeUnitId,
                //                            (SELECT  t.BigUnitId FROM dcms.Products AS t WHERE id = si.ProductId AND t.StoreId = {0}) AS BigUnitId,
                //                            si.UnitId,
                //                            (SELECT IFNULL(t.`Name`, si.UnitId) FROM dcms.SpecificationAttributeOptions AS t WHERE id = si.UnitId AND t.StoreId = {0}) AS UnitName,
                //                            si.Quantity,
                //                            si.CostAmount
                //                    FROM dcms.SaleBills AS s  INNER JOIN dcms.SaleItems AS si ON s.Id = si.SaleBillId
                //                    WHERE
                //                            s.StoreId = {0} 
                //                            AND si.IsGifts = TRUE AND si.CampaignId = 0 AND CostContractId > 0  {1})) AS a
                //                WHERE
                //                   a.Gtype >= 0  {2} ";
                var mySqlString = @"SELECT si.GiveTypeId AS Gtype,s.TerminalId,si.RemarkConfigId
		                                ,(
			                                SELECT IFNULL(t.`Name`, '未知')
			                                FROM dcms_crm.CRM_Terminals t
			                                WHERE id = s.TerminalId
				                                AND t.StoreId = {0}
		                                ) AS TerminalName
 		                                ,(
 			                                SELECT IFNULL(t.`Code`, '000')
			                                FROM dcms_crm.CRM_Terminals t
 			                                WHERE id = s.TerminalId
 				                                AND t.StoreId = {0}
 		                                ) AS TerminalCode
 		                                ,s.BusinessUserId
 		                                , IFNULL((
 			                                SELECT IFNULL(t.`UserRealName`, s.BusinessUserId)
 			                                FROM auth.Users t
 			                                WHERE id = s.BusinessUserId
 			                                AND t.StoreId = {0}
 		                                ),s.BusinessUserId) AS BusinessUserName
 		                                ,IFNULL(p.ERPProductId,0) AS ProductId
 		                                ,IFNULL(p.MnemonicCode,p.`Name`) AS ProductName
 		                                ,p.CategoryId
 		                                ,p.BigQuantity
 		                                ,p.SmallUnitId
 		                                ,p.StrokeUnitId
 		                                ,p.BigUnitId
 		                                ,si.UnitId
 		                                , (
 			                                SELECT IFNULL(t.`Name`, si.UnitId)
 			                                FROM dcms.SpecificationAttributeOptions t
 			                                WHERE id = si.UnitId
 			                                AND t.StoreId = {0}
 		                                ) AS UnitName
 		                                , si.Quantity
 		                                , si.CostAmount
                                        ,IFNULL(
                                            (SELECT IFNULL(t.`Name`, p.SmallUnitId) 
                                             FROM dcms.SpecificationAttributeOptions AS t 
                                             WHERE id = p.SmallUnitId 
                                             AND t.StoreId = {0} 
                                             AND p.SmallUnitId >0),'') AS SUnitName
                                        ,IFNULL(
                                            (SELECT IFNULL(t.`Name`, p.StrokeUnitId)
                                             FROM dcms.SpecificationAttributeOptions AS t 
                                             WHERE id = p.StrokeUnitId 
                                             AND t.StoreId = {0} 
                                             AND p.StrokeUnitId >0),'') AS MUnitName
                                        ,IFNULL(
                                            (SELECT IFNULL(t.`Name`, p.BigUnitId) 
                                             FROM dcms.SpecificationAttributeOptions AS t 
                                             WHERE id = p.BigUnitId 
                                             AND t.StoreId = {0} 
                                             AND p.BigUnitId >0),'') AS BUnitName
                                    FROM dcms.SaleBills s
		                            INNER JOIN dcms.SaleItems si ON s.Id = si.SaleBillId
		                            INNER JOIN dcms.Products p ON si.ProductId=p.Id
		                            WHERE s.StoreId = {0}
                                        AND s.AuditedStatus = TRUE
										AND s.ReversedStatus = FALSE
		                                AND si.IsGifts = true
		                                {1} {2}";
                string sqlString = string.Format(mySqlString, store ?? 0, whereString1, whereString2);
                var query = GiveQuotaRecordsRepository.QueryFromSql<GiveSummery>(sqlString).ToList();

                //分组
                if (query != null && query.Any())
                {
                    foreach (IGrouping<int, GiveSummery> tgroup in query.GroupBy(s => s.TerminalId))
                    {
                        foreach (IGrouping<int, GiveSummery> pgroup in tgroup.GroupBy(s => s.ProductId))
                        {
                            var t = tgroup.FirstOrDefault();
                            var p = pgroup.FirstOrDefault();

                            var grs = new GiveQuotaRecordsSummery
                            {
                                TerminalId = tgroup.Key,
                                ProductId = pgroup.Key,

                                TerminalName = t.TerminalName,
                                TerminalCode = t.TerminalCode,
                                ProductName = p.ProductName,
                                BarCode = pgroup.Key.ToString(),

                                //单位换算
                                UnitConversion = $"1{p.BUnitName} = {p.BigQuantity}{p.SUnitName}"
                            };

                            var giveType_lst = pgroup.Where(s => s.Gtype == 1 || s.Gtype == 0).DistinctBy(d=>d.RemarkConfigId).ToList();

                            var lst = new List<OrdinaryGiftSummery>();

                            foreach (var item in giveType_lst)
                            {
                                var giftQuantity = $"";
                                var rst = ParperUnit(pgroup.Where(s => s.Gtype == item.Gtype && s.RemarkConfigId == item.RemarkConfigId).ToList());
                                if (rst.Item1 > 0)
                                    giftQuantity += $"{rst.Item1}{p.BUnitName}";

                                if (rst.Item2 > 0)
                                    giftQuantity += $"{rst.Item2}{p.MUnitName}";

                                if (rst.Item3 > 0)
                                    giftQuantity += $"{rst.Item3}{p.SUnitName}";
                                var ordinaryGiftSummery = new OrdinaryGiftSummery();
                                ordinaryGiftSummery.RemarkConfigId = item.RemarkConfigId??0;
                                ordinaryGiftSummery.Quantity = giftQuantity;
                                ordinaryGiftSummery.CostAmount = rst.Item4;
                                lst.Add(new OrdinaryGiftSummery() { RemarkConfigId = item.RemarkConfigId ?? 0, Quantity = giftQuantity, CostAmount = rst.Item4, QuantityTuple = rst });
                                
                            }
                            grs.OrdinaryGiftSummerys = lst;

                            var pu1 = ParperUnit(pgroup.Where(s => s.Gtype == 1).ToList());
                            var pu2 = ParperUnit(pgroup.Where(s => s.Gtype == 3).ToList());
                            var pu3 = ParperUnit(pgroup.Where(s => s.Gtype == 4).ToList());


                            var gf = $"";
                            var pf = $"";
                            var cf = $"";

                            //普通赠品
                            grs.GeneralQuantity = 0;
                            grs.GeneralQuantityTuple = pu1;

                            if (pu1.Item1 > 0)
                                gf += $"{pu1.Item1}{p.BUnitName}";

                            if (pu1.Item2 > 0)
                                gf += $"{pu1.Item2}{p.MUnitName}";

                            if (pu1.Item3 > 0)
                                gf += $"{pu1.Item3}{p.SUnitName}";

                            grs.GeneralQuantityFormat = gf;
                            grs.GeneralCostAmount = pu1.Item4;

                            //促销赠品


                            grs.PromotionalQuantity = 0;
                            grs.PromotionalQuantityTuple = pu2;

                            if (pu2.Item1 > 0)
                                pf += $"{pu2.Item1}{p.BUnitName}";

                            if (pu2.Item2 > 0)
                                pf += $"{pu2.Item2}{p.MUnitName}";

                            if (pu2.Item3 > 0)
                                pf += $"{pu2.Item3}{p.SUnitName}";

                            grs.PromotionalQuantityFormat = pf;
                            grs.PromotionalCostAmount = pu2.Item4;

                            //费用合同
                            grs.ContractQuantity = 0;
                            grs.ContractQuantityTuple = pu3;

                            if (pu3.Item1 > 0)
                                cf += $"{pu3.Item1}{p.BUnitName}";

                            if (pu3.Item2 > 0)
                                cf += $"{pu3.Item2}{p.MUnitName}";

                            if (pu3.Item3 > 0)
                                cf += $"{pu3.Item3}{p.SUnitName}";


                            grs.ContractQuantityFormat = cf;
                            grs.ContractCostAmount = pu3.Item4;

                            //订货赠品
                            grs.OrderQuantity = 0;
                            grs.OrderQuantityTuple = new Tuple<int, int, int, decimal>(0, 0, 0, 0);
                            grs.OrderQuantityFormat = "";
                            grs.OrderCostAmount = 0;

                            recordsSummeries.Add(grs);
                        }
                    }
                }
                //总页数
                var totalCount = recordsSummeries.Count();
                var plists = recordsSummeries.Skip(pageIndex * pageSize).Take(pageSize)?.ToList();
                return new PagedList<GiveQuotaRecordsSummery>(plists, pageIndex, pageSize, totalCount);
            }
            catch (Exception ex)
            {
                return new PagedList<GiveQuotaRecordsSummery>(recordsSummeries, 0, 0);
            }
        }


        private Tuple<int, int, int, decimal> ParperUnit(List<GiveSummery> gqs)
        {
            var sQuantity_G = 0;
            var mQuantity_G = 0;
            var bQuantity_G = 0;
            decimal costAmount_G = 0;

            if (gqs != null && gqs.Any())
            {
                gqs.ForEach(s =>
                {
                    if (s.UnitId == s.BigUnitId)
                    {
                        bQuantity_G += s.Quantity;
                    }
                    else if (s.UnitId == s.StrokeUnitId)
                    {
                        mQuantity_G += s.Quantity;
                    }
                    else if (s.UnitId == s.SmallUnitId)
                    {
                        sQuantity_G += s.Quantity;
                    }
                    costAmount_G += s.CostAmount;
                });
            }

            return new Tuple<int, int, int, decimal>(bQuantity_G, mQuantity_G, sQuantity_G, costAmount_G);
        }

        public virtual IList<GiveQuotaRecordsSummery> GetAllGiveQuotaRecordsSummeries(int? store, int? businessUserId, int? productid, int? customerId, int? catagoryId, int? costingCalCulateMethodId, int? giveTypeId, DateTime? start, DateTime? end)
        {

            string sqlString = $"select row_number()over(order by getdate()) as Id,Records=(STUFF((select ',' + CONVERT(VARCHAR(10),t.Id) from GiveQuotaRecords as t where t.StoreId = alls.StoreId  and t.TerminalId = alls.TerminalId  and t.ProductId = alls.ProductId  and t.CreatedOnUtc = alls.CreatedOnUtc  FOR XML PATH('')), 1, 1, '')),storeId,TerminalId,TerminalName,ProductId,ProductName,TerminalCode,BarCode,UnitConversion,sum(case GiveTypeId when 1 then Quantity else 0 end) GeneralQuantity,sum(case GiveTypeId when 1 then CostAmount else 0 end) GeneralCostAmount,sum(case GiveTypeId when 2 then Quantity else 0 end) OrderQuantity,sum(case GiveTypeId when 2 then CostAmount else 0 end) OrderCostAmount,sum(case GiveTypeId when 3 then Quantity else 0 end) PromotionalQuantity,sum(case GiveTypeId when 3 then CostAmount else 0 end) PromotionalCostAmount,sum(case GiveTypeId when 4 then Quantity else 0 end) ContractQuantity,sum(case GiveTypeId when 4 then CostAmount else 0 end) ContractCostAmount from GiveQuotaRecords as alls where StoreId={store ?? 0} ";

            if (businessUserId.HasValue && businessUserId.Value != 0)
            {
                sqlString += $" and BusinessUserId = {businessUserId} ";
            }

            if (productid.HasValue && productid.Value != 0)
            {
                sqlString += $" and ProductId = {productid} ";
            }

            if (customerId.HasValue && customerId.Value != 0)
            {
                sqlString += $" and TerminalId = {customerId}";
            }

            if (catagoryId.HasValue && catagoryId.Value != 0)
            {
                sqlString += $" and CategoryId = {catagoryId} ";
            }

            if (costingCalCulateMethodId.HasValue && costingCalCulateMethodId.Value != 0)
            {
                sqlString += $" and CostingCalCulateMethodId = {costingCalCulateMethodId} ";
            }

            if (giveTypeId.HasValue && giveTypeId.Value != 0)
            {
                sqlString += $" and GiveTypeId = {giveTypeId} ";
            }

            if (start.HasValue)
            {
                var startTime = DateTime.Parse(start.Value.ToString("yyyy-MM-dd 00:00:00"));
                sqlString += " and CreatedOnUtc >= '{startTime}'";
            }

            if (end.HasValue)
            {
                var endTime = DateTime.Parse(end.Value.ToString("yyyy-MM-dd 23:59:59"));
                sqlString += " and CreatedOnUtc <='{endTime}' ";
            }

            sqlString += " group by storeId,TerminalId,TerminalName,ProductId,ProductName,TerminalCode,BarCode,UnitConversion,CreatedOnUtc";

            var query = GiveQuotaRecordsRepository.QueryFromSql<GiveQuotaRecordsSummery>(sqlString);
            query = query.OrderByDescending(gr => gr.CreatedOnUtc);
            return query.ToList();
        }


        /// <summary>
        /// 获取全部
        /// </summary>
        /// <returns></returns>
        public virtual IList<GiveQuotaRecords> GetAllGiveQuotaRecords(int? store)
        {
            var key = DCMSDefaults.GIVEQUOTARECORDS_ALL_KEY.FillCacheKey(store);
            return _cacheManager.Get(key, () =>
            {
                var query = from s in GiveQuotaRecordsRepository.Table
                            where s.StoreId == store.Value
                            orderby s.Id
                            select s;
                var giveQuota = query.ToList();
                return giveQuota;
            });
        }

        /// <summary>
        /// 获取
        /// </summary>
        /// <param name="giveQuotasId"></param>
        /// <returns></returns>
        public virtual GiveQuotaRecords GetQuotaRecordsRepositoryById(int? store, int giveQuotaRecordsId)
        {
            if (giveQuotaRecordsId == 0)
            {
                return null;
            }

            return GiveQuotaRecordsRepository.ToCachedGetById(giveQuotaRecordsId);
        }

        public virtual IList<GiveQuotaRecords> GetQuotaRecordsByBillId(int billId)
        {
            var query = from s in GiveQuotaRecordsRepository.Table
                        where s.BillId == billId
                        orderby s.Id
                        select s;
            var giveQuota = query.ToList();
            return giveQuota;
        }

        /// <summary>
        /// 获取每种类型的费用及赠品兑付情况
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="customerId"></param>
        /// <param name="giveTypeId"></param>
        /// <returns></returns>
        public IList<GiveQuotaRecords> GetQuotaRecordsByType(int? storeId, int customerId, int? giveTypeId, int? costContractId)
        {
            var query = GiveQuotaRecordsRepository_RO.Table;

            if (storeId.HasValue && storeId.Value > 0)
            {
                query = query.Where(gq => gq.TerminalId == customerId);
            }

            if (customerId > 0)
            {
                query = query.Where(gq => gq.TerminalId == customerId);
            }

            if (giveTypeId.HasValue && giveTypeId.Value > 0)
            {
                query = query.Where(gq => gq.GiveTypeId == giveTypeId);
            }

            if (costContractId.HasValue && costContractId.Value > 0)
            {
                query = query.Where(gq => gq.ContractId == costContractId);
            }

            var giveQuota = query.ToList();
            return giveQuota;
        }


        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="giveQuotaRecords"></param>
        public virtual void InsertGiveQuotaRecords(GiveQuotaRecords giveQuotaRecords)
        {
            if (giveQuotaRecords == null)
            {
                throw new ArgumentNullException("giveQuotaRecords");
            }

            switch (giveQuotaRecords.GiveType)
            {
                //普通
                case GiveTypeEnum.General:
                    giveQuotaRecords.CampaignId = 0;
                    giveQuotaRecords.ContractId = 0;
                    break;
                //订货赠品
                case GiveTypeEnum.Order:
                    giveQuotaRecords.CampaignId = 0;
                    giveQuotaRecords.ContractId = 0;
                    break;
                //促销赠品
                case GiveTypeEnum.Promotional:
                    giveQuotaRecords.ContractId = 0;
                    break;
                //费用合同
                case GiveTypeEnum.Contract:
                    giveQuotaRecords.CampaignId = 0;
                    break;
                default:
                    break;
            }

            var uow = GiveQuotaRecordsRepository.UnitOfWork;
            GiveQuotaRecordsRepository.Insert(giveQuotaRecords);
            uow.SaveChanges();

            _eventPublisher.EntityInserted(giveQuotaRecords);
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="giveQuotas"></param>
        public virtual void UpdateGiveQuotaRecords(GiveQuotaRecords giveQuotaRecords)
        {
            if (giveQuotaRecords == null)
            {
                throw new ArgumentNullException("giveQuotaRecords");
            }

            var uow = GiveQuotaRecordsRepository.UnitOfWork;
            GiveQuotaRecordsRepository.Update(giveQuotaRecords);
            uow.SaveChanges();

            _eventPublisher.EntityUpdated(giveQuotaRecords);
        }

        #endregion

        public BaseResult BillCreateOrUpdate(int storeId, int userId, int? giveQuotaId, GiveQuota giveQuota, GiveQuotaUpdate data, List<GiveQuotaOption> items, bool isAdmin = false)
        {
            var uow = GiveQuotaRepository.UnitOfWork;

            ITransaction transaction = null;
            try
            {

                transaction = uow.BeginOrUseTransaction();

                //修改
                if (giveQuotaId.HasValue && giveQuotaId.Value != 0 && giveQuota != null)
                {
                    giveQuota.Remark = data.Remark;
                    UpdateGiveQuota(giveQuota); //更新额度设置

                    #region 更新额度项目

                    var allProducts = _productService.GetProductsByIds(storeId, data.Items.Select(ci => ci.ProductId).Distinct().ToArray());
                    var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(storeId, allProducts.GetProductBigStrokeSmallUnitIds());

                    //原有额度配置
                    List<GiveQuotaOption> oldGiveQuotaOptions = GetGiveQuotaOptionByQuotaId(giveQuotaId).ToList();

                    data.Items.ForEach(p =>
                    {
                        if (p.ProductId != 0)
                        {
                            var giveQuotaOption = GetGiveQuotaOptionById(p.Id);
                            if (giveQuotaOption == null)
                            {
                                //追加项
                                if (oldGiveQuotaOptions.Count(cp => cp.Id == p.Id) == 0)
                                {
                                    var item = p;
                                    item.GiveQuotaId = giveQuota.Id;
                                    item.ProductId = p.ProductId;
                                    item.UnitId = p.UnitId;
                                    item.StoreId = storeId;
                                    item.Jan = p.Jan ?? 0;
                                    item.Feb = p.Feb ?? 0;
                                    item.Mar = p.Mar ?? 0;
                                    item.Apr = p.Apr ?? 0;
                                    item.May = p.May ?? 0;
                                    item.Jun = p.Jun ?? 0;
                                    item.Jul = p.Jul ?? 0;
                                    item.Aug = p.Aug ?? 0;
                                    item.Sep = p.Sep ?? 0;
                                    item.Oct = p.Oct ?? 0;
                                    item.Nov = p.Nov ?? 0;
                                    item.Dec = p.Dec ?? 0;

                                    item.Total = (p.Jan ?? 0) + (p.Feb ?? 0) + (p.Mar ?? 0) + (p.Apr ?? 0) + (p.May ?? 0) + (p.Jun ?? 0) + (p.Aug ?? 0) + (p.Sep ?? 0) + (p.Oct ?? 0) + (p.Nov ?? 0) + (p.Dec ?? 0);
                                    item.Remark = p.Remark == null ? "" : p.Remark;

                                    InsertGiveQuotaOption(item);
                                    //不排除
                                    p.Id = item.Id;
                                    if (!giveQuota.GiveQuotaOptions.Select(s => s.Id).Contains(item.Id))
                                    {
                                        giveQuota.GiveQuotaOptions.Add(item);
                                    }
                                }
                            }
                            else
                            {
                                //存在则更新
                                giveQuotaOption.ProductId = p.ProductId;
                                giveQuotaOption.UnitId = p.UnitId;
                                giveQuotaOption.Jan = p.Jan ?? 0;
                                giveQuotaOption.Feb = p.Feb ?? 0;
                                giveQuotaOption.Mar = p.Mar ?? 0;
                                giveQuotaOption.Apr = p.Apr ?? 0;
                                giveQuotaOption.May = p.May ?? 0;
                                giveQuotaOption.Jun = p.Jun ?? 0;
                                giveQuotaOption.Jul = p.Jul ?? 0;
                                giveQuotaOption.Aug = p.Aug ?? 0;
                                giveQuotaOption.Sep = p.Sep ?? 0;
                                giveQuotaOption.Oct = p.Oct ?? 0;
                                giveQuotaOption.Nov = p.Nov ?? 0;
                                giveQuotaOption.Dec = p.Dec ?? 0;

                                giveQuotaOption.Total = (p.Jan ?? 0) + (p.Feb ?? 0) + (p.Mar ?? 0) + (p.Apr ?? 0) + (p.May ?? 0) + (p.Jun ?? 0) + (p.Aug ?? 0) + (p.Sep ?? 0) + (p.Oct ?? 0) + (p.Nov ?? 0) + (p.Dec ?? 0);
                                giveQuotaOption.Remark = p.Remark == null ? "" : p.Remark;

                                UpdateGiveQuotaOption(giveQuotaOption);
                            }
                        }
                    });

                    #endregion

                    #region Grid 移除则从库移除删除项

                    giveQuota.GiveQuotaOptions.ToList().ForEach(p =>
                    {
                        if (data.Items.Count(cp => cp.Id == p.Id) == 0)
                        {
                            giveQuota.GiveQuotaOptions.Remove(p);
                            var sd = GetGiveQuotaOptionById(p.Id);
                            if (sd != null)
                            {
                                DeleteGiveQuotaOption(sd);
                            }
                        }
                    });

                    #endregion

                }
                //插入
                else
                {

                    giveQuota = new GiveQuota()
                    {
                        Year = data.Year.Value,
                        UserId = data.UserId.Value,
                        StoreId = storeId,
                        Remark = data.Remark == null ? "" : data.Remark
                    };

                    //添加
                    InsertGiveQuota(giveQuota);

                    var allProducts = _productService.GetProductsByIds(storeId, data.Items.Select(pr => pr.ProductId).Distinct().ToArray());
                    var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(storeId, allProducts.GetProductBigStrokeSmallUnitIds());

                    foreach (var option in data.Items)
                    {
                        var product = allProducts.Where(ap => ap.Id == option.ProductId).FirstOrDefault();
                        if (product != null && option.ProductId > 0)
                        {
                            option.StoreId = storeId;
                            option.GiveQuotaId = giveQuota.Id;

                            option.Total = ((option.Jan ?? 0) + (option.Feb ?? 0) + (option.Mar ?? 0) + (option.Apr ?? 0) + (option.May ?? 0) + (option.Jun ?? 0) + (option.Jul ?? 0) + (option.Aug ?? 0) + (option.Sep ?? 0) + (option.Oct ?? 0) + (option.Nov) + (option.Dec ?? 0));

                            option.Remark = option.Remark == null ? "" : option.Remark;

                            InsertGiveQuotaOption(option);
                        }
                    }

                }


                //保存事务
                transaction.Commit();

                return new BaseResult { Success = true, Return = giveQuotaId ?? 0, Message = "单据创建/更新成功" };
            }
            catch (Exception ex)
            {
                //如果事务不存在或者为控则回滚
                transaction?.Rollback();
                return new BaseResult { Success = false, Message = "单据创建/更新失败" };
            }
            finally
            {
                //不管怎样最后都会关闭掉这个事务
                using (transaction) { }
            }
        }

        /// <summary>
        /// 主管赠品额度余额（动态计算方式）--合同类型为从主管赠品扣减（2），主管、年份、月份
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="year"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public IList<GiveQuotaOption> GetGiveQuotaBalances(int? storeId, int year, int userId, int? giveQuotaId = 0)
        {
            var result = new List<GiveQuotaOption>();

            //获取主管赠品扣减费用合同
            var costs = from cc in CostContractBillsRepository.Table
                         join ci in CostContractItemsRepository.Table
                         .Include(c=>c.CostContractBill) 
                         on cc.Id equals ci.CostContractBillId
                         where cc.StoreId == storeId && cc.ContractType == 2 && cc.LeaderId == userId && cc.Year == year && cc.RejectedStatus==false && cc.AbandonedStatus==false
                        select ci;

            var options =GetGiveQuotaOptions(year, userId); //主管赠品明细

            if (giveQuotaId.HasValue && giveQuotaId.Value > 0)
                options = options.Where(g => g.GiveQuotaId == giveQuotaId).ToList();

            List<int> productIds = new List<int>();
            productIds.AddRange(costs.Select(x=>x.ProductId).Distinct().ToList());
            productIds.AddRange(options.Select(it => it.ProductId).Distinct().ToList());

            //所有商品
            List<Product> allProducts = _productService.GetProductsByIds(storeId??0, productIds.Distinct().ToArray()).ToList();
            var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(storeId, allProducts.GetProductBigStrokeSmallUnitIds());

            var costContractItems = costs.ToList();
            if (costContractItems?.Any() ?? false)
            {
                costContractItems.ForEach(s =>
                {
                    Product product = allProducts.Where(ap => ap.Id == s.ProductId).FirstOrDefault();
                    var conversionQuantity = product.GetConversionQuantity(allOptions, s.UnitId ?? 0); //转为小单位量再计算
                    if (product.SmallUnitId != s.UnitId)
                        s.Total = CalcDoubleQuality(s.Total.Value.ToString("0.0"), conversionQuantity);
                });
            }

            foreach (var item in options)
            {
                Product product = allProducts.Where(ap => ap.Id == item.ProductId).FirstOrDefault();
                var conversionQuantity = product.GetConversionQuantity(allOptions, item.UnitId ?? 0); //转为小单位量再计算

                decimal? jan = item.Jan, feb = item.Feb, mar = item.Mar, apr = item.Apr, may = item.May, jun = item.Jun, jul = item.Jul, aug = item.Aug, sep = item.Sep, oct = item.Oct, nov = item.Nov, dec = item.Dec;

                //在这里转换双精度量
                if (product.SmallUnitId != item.UnitId)
                {
                    jan = CalcDoubleQuality(item.Jan==null ? "0" : item.Jan.Value.ToString("0.0"), conversionQuantity);
                    feb = CalcDoubleQuality(item.Feb == null ? "0" : item.Feb.Value.ToString("0.0"), conversionQuantity);
                    mar = CalcDoubleQuality(item.Mar == null ? "0" : item.Mar.Value.ToString("0.0"), conversionQuantity);
                    apr = CalcDoubleQuality(item.Apr == null ? "0" : item.Apr.Value.ToString("0.0"), conversionQuantity);
                    may = CalcDoubleQuality(item.May == null ? "0" : item.May.Value.ToString("0.0"), conversionQuantity);
                    jun = CalcDoubleQuality(item.Jun == null ? "0" : item.Jun.Value.ToString("0.0"), conversionQuantity);
                    jul = CalcDoubleQuality(item.Jul == null ? "0" : item.Jul.Value.ToString("0.0"), conversionQuantity);
                    aug = CalcDoubleQuality(item.Aug == null ? "0" : item.Aug.Value.ToString("0.0"), conversionQuantity);
                    sep = CalcDoubleQuality(item.Sep == null ? "0" : item.Sep.Value.ToString("0.0"), conversionQuantity);
                    oct = CalcDoubleQuality(item.Oct == null ? "0" : item.Oct.Value.ToString("0.0"), conversionQuantity);
                    nov = CalcDoubleQuality(item.Nov == null ? "0" : item.Nov.Value.ToString("0.0"), conversionQuantity);
                    dec = CalcDoubleQuality(item.Dec == null ? "0" : item.Dec.Value.ToString("0.0"), conversionQuantity);
                }

                item.Jan_Balance = item.Jan > 0 ? jan - costContractItems?.Where(g => g.ProductId==item.ProductId && g.CostContractBill.Month == 1)?.Sum(s => s.Total) ?? 0 : 0;
                item.Feb_Balance = item.Feb > 0 ? feb - costContractItems?.Where(g => g.ProductId == item.ProductId && g.CostContractBill.Month == 2)?.Sum(s => s.Total) ?? 0 : 0;
                item.Mar_Balance = item.Mar > 0 ? mar - costContractItems?.Where(g => g.ProductId == item.ProductId && g.CostContractBill.Month == 3)?.Sum(s => s.Total) ?? 0 : 0;
                item.Apr_Balance = item.Apr > 0 ? apr - costContractItems?.Where(g => g.ProductId == item.ProductId && g.CostContractBill.Month == 4)?.Sum(s => s.Total) ?? 0 : 0;
                item.May_Balance = item.May > 0 ? may - costContractItems?.Where(g => g.ProductId == item.ProductId && g.CostContractBill.Month == 5)?.Sum(s => s.Total) ?? 0 : 0;
                item.Jun_Balance = item.Jun > 0 ? jun - costContractItems?.Where(g => g.ProductId == item.ProductId && g.CostContractBill.Month == 6)?.Sum(s => s.Total) ?? 0 : 0;
                item.Jul_Balance = item.Jul > 0 ? jul - costContractItems?.Where(g => g.ProductId == item.ProductId && g.CostContractBill.Month == 7)?.Sum(s => s.Total) ?? 0 : 0;
                item.Aug_Balance = item.Aug > 0 ? aug - costContractItems?.Where(g => g.ProductId == item.ProductId && g.CostContractBill.Month == 8)?.Sum(s => s.Total) ?? 0 : 0;
                item.Sep_Balance = item.Sep > 0 ? sep - costContractItems?.Where(g => g.ProductId == item.ProductId && g.CostContractBill.Month == 9)?.Sum(s => s.Total) ?? 0 : 0;
                item.Oct_Balance = item.Oct > 0 ? oct - costContractItems?.Where(g => g.ProductId == item.ProductId && g.CostContractBill.Month == 10)?.Sum(s => s.Total) ?? 0 : 0;
                item.Nov_Balance = item.Nov > 0 ? nov - costContractItems?.Where(g => g.ProductId == item.ProductId && g.CostContractBill.Month == 11)?.Sum(s => s.Total) ?? 0 : 0;
                item.Dec_Balance = item.Dec > 0 ? dec - costContractItems?.Where(g => g.ProductId == item.ProductId && g.CostContractBill.Month == 12)?.Sum(s => s.Total) ?? 0 : 0;

                result.Add(item);
            }

            return result;
        }

        /// <summary>
        /// 计算双精度商品数量转换
        /// </summary>
        /// <param name="num"></param>
        /// <param name="conversionQuantity"></param>
        /// <returns></returns>
        protected decimal CalcDoubleQuality(string num, int conversionQuantity)
        {
            if (num.IndexOf(".") > -1)
            {
                string[] idArray = num.Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries);
                double[] idsDoubles = Array.ConvertAll<string, double>(idArray, s => Convert.ToDouble(s));
                int[] idInts = Array.ConvertAll<double, int>(idsDoubles, s => Convert.ToInt32(s));

                return idInts[0] * conversionQuantity + idInts[1];
            }

            return int.Parse(num) * conversionQuantity;
        }

    }
}