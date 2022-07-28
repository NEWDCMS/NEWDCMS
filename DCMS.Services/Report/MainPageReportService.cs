using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Domain.Common;
using DCMS.Core.Domain.Report;
using DCMS.Core.Domain.Sales;
using DCMS.Core.Infrastructure.DependencyManagement;
using DCMS.Services.Events;
using DCMS.Services.Stores;
using DCMS.Services.Users;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using DbF = Microsoft.EntityFrameworkCore.EF;
using DCMS.Services.Caching;
using System.Globalization;

namespace DCMS.Services.Report
{
    /// <summary>
    /// 主页报表
    /// </summary>
    public class MainPageReportService : BaseService, IMainPageReportService
    {
        #region 构造

        private readonly IStoreService _storeService;
        private readonly ISaleReportService _saleReportService;
        private readonly IUserService _userService;
        private readonly ICacheKeyService _cacheKeyService;

        public MainPageReportService(IServiceGetter getter,
            IStaticCacheManager cacheManager,
            ISaleReportService saleReportService,
            IStoreService storeService,
            IUserService userService,
            ICacheKeyService cacheKeyService,
            IEventPublisher eventPublisher) : base(getter, cacheManager, eventPublisher)
        {
            _saleReportService = saleReportService;
            _storeService = storeService;
            _userService = userService;
            _cacheKeyService = cacheKeyService;
        }


        #endregion

        /// <summary>
        /// 仪表盘
        /// </summary>
        /// <param name="storeId">经销商Id</param>
        /// <param name="businessUserIds">业务员Ids</param>
        /// <returns></returns>
        public DashboardReport GetDashboardReport(int? storeId, int[] businessUserIds, bool include = true)
        {
            try
            {
                #region MYSQL

                var reporting = new DashboardReport();

                var subs = new List<int>();

                if (include)
                {
                    businessUserIds.ToList().ForEach(b =>
                    {
                        var ids = _userService.GetSubordinate(storeId, b);
                        subs.AddRange(ids);
                    });

                    if (subs?.Count() == 0)
                    {
                        subs.AddRange(businessUserIds);
                    }
                }
                else
                {
                    subs = businessUserIds.ToList();

                }

                var distincts = subs.Distinct();

                string mySqlString = $"select (select ifnull(sum(sb.ReceivableAmount),0) as TodaySaleAmount from SaleBills  as sb where sb.StoreId = {storeId ?? 0} and sb.AuditedStatus = true and sb.ReversedStatus = false and  (sb.BusinessUserId in ({string.Join(",", distincts)}) and sb.BusinessUserId <> 0 ) and  DATEDIFF(now(),sb.CreatedOnUtc) = 0) as TodaySaleAmount,(select ifnull(sum(sb.ReceivableAmount),0) as TodaySaleAmount from SaleBills  as sb where sb.StoreId = {storeId ?? 0} and sb.AuditedStatus = true and sb.ReversedStatus = false and  (sb.BusinessUserId in ({string.Join(",", distincts)}) and sb.BusinessUserId <> 0 ) and  DATEDIFF(now(),sb.CreatedOnUtc) = 1) as YesterdaySaleAmount,(select ifnull(count(sb.ReceivableAmount), 0) as TodaySaleAmount from SaleReservationBills  as sb where sb.StoreId = {storeId ?? 0} and sb.AuditedStatus = true and sb.ReversedStatus = false and(sb.BusinessUserId in ({string.Join(",", distincts)}) and sb.BusinessUserId <> 0) and  DATEDIFF(now(), sb.CreatedOnUtc) = 0) as TodayOrderQuantity,(select ifnull(count(sb.ReceivableAmount), 0) as TodaySaleAmount from SaleReservationBills  as sb where sb.StoreId = {storeId ?? 0} and sb.AuditedStatus = true and sb.ReversedStatus = false and(sb.BusinessUserId in ({string.Join(",", distincts)}) and sb.BusinessUserId <> 0) and  DATEDIFF(now(), sb.CreatedOnUtc) = 1) as YesterdayOrderQuantity,(select count(*) as TodayAddTerminalQuantity from dcms_crm.CRM_Terminals  as sb where sb.StoreId ={storeId ?? 0}  and (sb.CreatedUserId in ({string.Join(",", distincts)}) and sb.CreatedUserId <> 0) and DATEDIFF(now(), sb.CreatedOnUtc) = 0) as TodayAddTerminalQuantity,(select count(*) as YesterdayAddTerminalQuantity from dcms_crm.CRM_Terminals  as sb where sb.StoreId = {storeId ?? 0} and(sb.CreatedUserId in ({string.Join(",", distincts)}) and sb.CreatedUserId <> 0) and DATEDIFF(now(), sb.CreatedOnUtc) = 1) as YesterdayAddTerminalQuantity,(select count(*) as TodayVisitQuantity from VisitStore  as sb where sb.StoreId = {storeId ?? 0}  and (sb.BusinessUserId in ({string.Join(",", distincts)}) and sb.BusinessUserId <> 0) and DATEDIFF(now(), sb.SigninDateTime) = 0) as TodayVisitQuantity,(select count(*) as YesterdayVisitQuantity from VisitStore  as sb where sb.StoreId = {storeId ?? 0} and (sb.BusinessUserId in ({string.Join(",", distincts)}) and sb.BusinessUserId <> 0) and DATEDIFF(now(), sb.SigninDateTime) = 1) as YesterdayVisitQuantity";

                reporting = SaleBillsRepository_RO.QueryFromSql<DashboardReport>(mySqlString).FirstOrDefault();

                return reporting;

                #endregion


            }
            catch (Exception)
            {
                return new DashboardReport();
            }
        }

        /// <summary>
        /// 当月销量
        /// </summary>
        /// <param name="store"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="brandIds"></param>
        /// <param name="businessUserIds"></param>
        /// <returns></returns>
        public IList<MonthSaleReport> GetMonthSaleReport(int? storeId, DateTime? startTime, DateTime? endTime, int[] brandIds, int[] businessUserIds)
        {
            try
            {
                var reporting = new List<MonthSaleReport>();

                string whereQuery = $" a.StoreId= {storeId ?? 0}";

                if (brandIds != null && brandIds.Length > 0)
                {
                    whereQuery += $" and p.BrandId in ({string.Join(",", brandIds)}) ";
                }

                if (businessUserIds.Length > 0 && !businessUserIds.Contains(0))
                {
                    whereQuery += $" and a.BusinessUserId in ({string.Join(",", businessUserIds)}) ";
                }

                whereQuery += $" and year(a.CreatedOnUtc) >= { DateTime.Now.Year}";

                //本年销量按月统计
                var mySqlString = $"select rr.*, b1.Name BrandName,p1.BigUnitId,p1.StrokeUnitId,p1.SmallUnitId , p1.BigQuantity, p1.StrokeQuantity from (select date_format(a.TransactionDate, '%Y-%m') SaleDate, sum(b.Amount) as SaleAmount,sum(b.Quantity) as SaleQuantity,p.BrandId,substring_index(group_concat(b.ProductId), ',', 1) ProductId from SaleBills a inner join SaleItems b on a.Id = b.SaleBillId inner join Products p on b.ProductId = p.Id where {whereQuery} and a.AuditedStatus='1' and a.ReversedStatus='0' and PERIOD_DIFF(date_format(current_date,'%Y%m'),date_format(a.TransactionDate,'%Y%m')) <=12 group by date_format(a.TransactionDate, '%Y-%m'), p.BrandId) rr inner join Brands b1 on rr.BrandId = b1.Id inner join Products p1 on rr.ProductId = p1.Id order by rr.SaleDate";

                reporting = SaleBillsRepository_RO.QueryFromSql<MonthSaleReport>(mySqlString).ToList();


                return reporting;
            }
            catch (Exception)
            {
                return new List<MonthSaleReport>();
            }
        }

        public IList<SalePercentReport> GetSalePercentReport(int? storeId, int type, int[] businessUserIds)
        {
            try
            {
                var reporting = new List<SalePercentReport>();

                string whereQuery = $" and a.StoreId= {storeId ?? 0}";

                if (businessUserIds.Length > 0 && !businessUserIds.Contains(0))
                {
                    whereQuery += $" and a.BusinessUserId in ({string.Join(",", businessUserIds)}) ";
                }

                switch (type)
                {
                    //今日
                    case 1:
                        {
                            //MYQL
                            whereQuery += $" and to_days(a.CreatedOnUtc) = to_days(now()) ";
                        }
                        break;
                    //今日上周同期
                    case 2:
                        {
                            whereQuery += $" and to_days(now()) - to_days(a.CreatedOnUtc) = 7 ";
                        }
                        break;
                    //昨天
                    case 3:
                        {
                            whereQuery += $" and to_days(now()) - to_days(a.CreatedOnUtc) = 1 ";
                        }
                        break;
                    //前天
                    case 4:
                        {
                            whereQuery += $" and to_days(now()) - to_days(a.CreatedOnUtc) = 2 ";
                        }
                        break;
                    //上周
                    case 5:
                        {
                            //MYQL
                            whereQuery += $" and YEARWEEK(date_format(a.CreatedOnUtc,'%Y-%m-%d')) = YEARWEEK(now())-1 ";
                        }
                        break;
                    //本周
                    case 6:
                        {
                            //MYQL
                            whereQuery += $" and YEARWEEK(date_format(a.CreatedOnUtc,'%Y-%m-%d')) = YEARWEEK(now()) ";
                        }
                        break;
                    //上月
                    case 7:
                        {
                            //MYQL
                            whereQuery += $" and date_format(a.CreatedOnUtc,'%Y-%m')=date_format(DATE_SUB(curdate(), INTERVAL 1 MONTH),'%Y-%m') ";
                        }
                        break;
                    //本月
                    case 8:
                        {
                            //MYQL
                            whereQuery += $" and date_format(a.CreatedOnUtc,'%Y-%m')=date_format(now(),'%Y-%m') ";
                        }
                        break;
                    //本季
                    case 9:
                        {
                            //MYQL
                            whereQuery += $" and QUARTER(a.CreatedOnUtc)=QUARTER(now()) ";
                        }
                        break;
                    //本年
                    case 10:
                        {
                            //MYQL
                            whereQuery += $" and YEAR(a.CreatedOnUtc)=YEAR(NOW()) ";
                        }
                        break;
                }
                //本年销量按月统计
                var mySqlString = $"select rr.*,u.UserRealName as BussinessUserName from (select sum(b.Amount) as SaleAmount,sum(b.Quantity) as SaleQuantity,BusinessUserId from dcms.SaleBills a inner join dcms.SaleItems b on a.Id = b.SaleBillId { whereQuery} and a.AuditedStatus='1' and a.ReversedStatus='0' group by BusinessUserId ) rr inner join auth.Users u on rr.BusinessUserId = u.Id";
                reporting = SaleBillsRepository_RO.QueryFromSql<SalePercentReport>(mySqlString).ToList();
                return reporting;
            }
            catch (Exception ex)
            {
                return new List<SalePercentReport>();
            }

        }

        public IList<BussinessVisitStoreReport> GetBussinessVisitStoreReport(int? storeId, DateTime? startTime, DateTime? endTime, int[] businessUserIds)
        {
            try
            {
                var reporting = new List<BussinessVisitStoreReport>();

                //string whereQuery = $" a.StoreId= {storeId ?? 0}";

                //if (businessUserIds.Length > 0 && !businessUserIds.Contains(0))
                //{
                //    whereQuery += $" and a.BusinessUserId in ({string.Join(",", businessUserIds)}) ";
                //}
                //if (startTime.HasValue)
                //{
                //    whereQuery += $" and a.CreatedOnUtc >= '{ startTime }'";
                //}
                //if (endTime.HasValue)
                //{
                //    whereQuery += $" and a.CreatedOnUtc <= '{ endTime }'";
                //}

                ////本年销量按月统计
                //var mySqlString = $"select rr.*,u.UserRealName as BussinessUserName from(select a.BusinessUserId, sum(b.Amount) as SaleAmount,sum(b.Quantity) as SaleQuantity from dcms.SaleBills a inner join dcms.SaleItems b on a.Id = b.SaleBillId  where {whereQuery} and a.AuditedStatus='1' and a.ReversedStatus='0' group by a.BusinessUserId) rr  inner join auth.Users u on rr.BusinessUserId = u.Id order by rr.BusinessUserId";

                string whereQuery = $" sb.StoreId = {storeId ?? 0}";

                if (businessUserIds.Length > 0 && !businessUserIds.Contains(0))
                {
                    whereQuery += $" and sb.BusinessUserId in ({string.Join(",", businessUserIds)}) ";
                }
                if (startTime.HasValue)
                {
                    whereQuery += $" and sb.SigninDateTime >= '{ startTime }'";
                }
                if (endTime.HasValue)
                {
                    whereQuery += $" and sb.SignOutDateTime <= '{ endTime }'";
                }

                //本年销量按月统计
                //var mySqlString = $"select rr.*,u.UserRealName as BussinessUserName from(select a.BusinessUserId, sum(b.Amount) as SaleAmount,sum(b.Quantity) as SaleQuantity from dcms.SaleBills a inner join dcms.SaleItems b on a.Id = b.SaleBillId  where {whereQuery} and a.AuditedStatus='1' and a.ReversedStatus='0' group by a.BusinessUserId) rr  inner join auth.Users u on rr.BusinessUserId = u.Id order by rr.BusinessUserId";

                var mySqlString = $"select t.*,u.UserRealName as BussinessUserName from (select BusinessUserId,count(*) as VisitStoreAmount from dcms.VisitStore  as sb where { whereQuery} group by BusinessUserId)t inner join auth.Users u on t.BusinessUserId = u.Id";
                //reporting = SaleBillsRepository_RO.QueryFromSql<BussinessSaleReport>(mySqlString).ToList();
                reporting = SaleBillsRepository_RO.QueryFromSql<BussinessVisitStoreReport>(mySqlString).ToList();

                return reporting;
            }
            catch (Exception ex)
            {
                return new List<BussinessVisitStoreReport>();
            }
        }

        /// <summary>
        /// 获取所有经销商销售信息
        /// </summary>
        /// <returns></returns>
        public AllStoreSaleInformation GetAllStoreSaleInformation()
        {

            AllStoreSaleInformation allStoreSaleInformation = new AllStoreSaleInformation();
            try
            {
                allStoreSaleInformation.AllStoreDashboard = GetAllStoreDashboard();

                //订单总计
                allStoreSaleInformation.AllStoreOrderTotals = GetAllStoreOrderTotal(0);
                allStoreSaleInformation.AllStoreUnfinishedOrders = GetAllStoreUnfinishedOrder(0, 5, 1);

                allStoreSaleInformation.AllStoreHotSaleQuantityRankings = GetAllStoreHotSaleRanking(5, 1);
                allStoreSaleInformation.AllStoreHotSaleAmountRankings = GetAllStoreHotSaleRanking(5, 2);

                allStoreSaleInformation.AllStoreHotOrderQuantityRankings = GetAllStoreHotOrderRanking(5, 1);
                allStoreSaleInformation.AllStoreHotOrderAmountRankings = GetAllStoreHotOrderRanking(5, 2);

                return allStoreSaleInformation;
            }
            catch (Exception)
            {
                return allStoreSaleInformation;
            }

        }

        public AllStoreDashboard GetAllStoreDashboard()
        {

            try
            {
                var tSaleQuantity = from s in SaleBillsRepository_RO.TableNoTracking
                                    join b in SaleItemsRepository_RO.TableNoTracking on s.Id equals b.SaleBillId
                                    where s.AuditedStatus == true && s.ReversedStatus == false
                                    select b.Quantity;

                var tOrderQuantity = SaleReservationBillsRepository_RO.TableNoTracking.Where(p => p.AuditedStatus == true && p.ReversedStatus == false).Count();

                var storeQuantity = StoreRepository_RO.TableNoTracking.Where(p => p.Published == true).Count();

                var storeProductQuantity = ProductsRepository_RO.TableNoTracking.Where(p => p.Published == true).Count();

                var allStoreDashboard = new AllStoreDashboard()
                {
                    /// <summary>
                    ///  所有订单总数
                    /// </summary>
                    TotalSumOrderQuantity = tOrderQuantity,

                    /// <summary>
                    ///  所有经销商总数
                    /// </summary>
                    TotalSumStoreQuantity = storeQuantity,

                    /// <summary>
                    /// 所有经销商商品总数
                    /// </summary>
                    TotalSumProductQuantity = storeProductQuantity,

                    /// <summary>
                    ///  所有商品销量总数
                    /// </summary>
                    TotalSumSaleQuantity = tSaleQuantity.Sum(),
                };

                return allStoreDashboard;
            }
            catch (Exception)
            {
                return new AllStoreDashboard();
            }

        }

        /// <summary>
        /// 获取经销商销售订单总计
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="orderStatus"></param>
        /// <param name="dateDiff"></param>
        /// <returns></returns>
        public int GetOrderCountByStore(int? storeId, int orderStatus, int dateDiff)
        {
            try
            {
                var query = SaleReservationBillsRepository_RO.TableNoTracking;

                if (storeId.HasValue && storeId != 0)
                {
                    query = query.Where(s => s.StoreId == storeId);
                }

                //1,未审核 2,已审核 3,已红冲 4,已收款 5,已调度
                switch (orderStatus)
                {
                    //未审核
                    case 1:
                        query = query.Where(s => s.AuditedStatus == false);
                        break;
                    //已审核
                    case 2:
                        query = query.Where(s => s.AuditedStatus == true);
                        break;
                    //已红冲
                    case 3:
                        query = query.Where(s => s.ReversedStatus == true);
                        break;
                    //已收款
                    case 4:
                        query = query.Where(s => s.Receipted == true);
                        break;
                    //已调度
                    case 5:
                        query = query.Where(s => s.DispatchedStatus == false);
                        break;
                }

                switch (dateDiff)
                {

                    //今天
                    case 1:
                        query = query.Where(s => MySqlDbFunctionsExtensions.DateDiffDay(DbF.Functions, DateTime.Now, s.CreatedOnUtc) == 0);
                        break;
                    //本周
                    case 2:
                        query = query.Where(s => s.CreatedOnUtc > DateTime.Now.AddDays(-Convert.ToInt32(DateTime.Now.Date.DayOfWeek)));
                        break;
                    //本月
                    case 3:
                        query = query.Where(s => s.CreatedOnUtc > DateTime.Now.AddDays(-Convert.ToInt32(DateTime.Now.Date.Day)));
                        break;
                    //本年
                    case 4:
                        query = query.Where(s => s.CreatedOnUtc.Year == DateTime.Now.Year);
                        break;
                    //全部
                    default:

                        break;
                }

                return query.Count();
            }
            catch (Exception)
            {
                return 0;
            }
        }


        /// <summary>
        /// 订单总计
        /// </summary>
        /// <param name="storeId"></param>
        /// <returns></returns>
        public List<AllStoreOrderTotal> GetAllStoreOrderTotal(int storeId)
        {
            try
            {
                var reporting = new List<AllStoreOrderTotal>()
            {
                new AllStoreOrderTotal
                {
                    OrderStatus = 1,
                    OrderStatusName = "未审核",
                    Today = GetOrderCountByStore(storeId, 1, 1),
                    ThisWeek = GetOrderCountByStore(storeId, 1, 2),
                    ThisMonth = GetOrderCountByStore(storeId, 1, 3),
                    ThisYear = GetOrderCountByStore(storeId, 1, 4),
                    Total = GetOrderCountByStore(storeId, 1, 5),
                },
                new AllStoreOrderTotal
                {
                    OrderStatus = 2,
                    OrderStatusName = "已审核",
                    Today = GetOrderCountByStore(storeId, 2, 1),
                    ThisWeek = GetOrderCountByStore(storeId, 2, 2),
                    ThisMonth = GetOrderCountByStore(storeId, 2, 3),
                    ThisYear = GetOrderCountByStore(storeId, 2, 4),
                    Total = GetOrderCountByStore(storeId, 2, 5),
                },
                new AllStoreOrderTotal
                {
                    OrderStatus = 3,
                    OrderStatusName = "已红冲",
                    Today = GetOrderCountByStore(storeId, 3, 1),
                    ThisWeek = GetOrderCountByStore(storeId, 3, 2),
                    ThisMonth = GetOrderCountByStore(storeId, 3, 3),
                    ThisYear = GetOrderCountByStore(storeId, 3, 4),
                    Total = GetOrderCountByStore(storeId, 3, 5),
                },
                new AllStoreOrderTotal
                {
                    OrderStatus = 4,
                    OrderStatusName = "已收款",
                    Today = GetOrderCountByStore(storeId, 4, 1),
                    ThisWeek = GetOrderCountByStore(storeId, 4, 2),
                    ThisMonth = GetOrderCountByStore(storeId, 4, 3),
                    ThisYear = GetOrderCountByStore(storeId,4, 4),
                    Total = GetOrderCountByStore(storeId, 4, 5),
                },
                new AllStoreOrderTotal
                {
                    OrderStatus = 5,
                    OrderStatusName = "已调度",
                    Today = GetOrderCountByStore(storeId, 5, 1),
                    ThisWeek = GetOrderCountByStore(storeId, 5, 2),
                    ThisMonth = GetOrderCountByStore(storeId, 5, 3),
                    ThisYear = GetOrderCountByStore(storeId, 5, 4),
                    Total = GetOrderCountByStore(storeId, 5, 5),
                },
            };
                return reporting;
            }
            catch (Exception)
            {
                return new List<AllStoreOrderTotal>();
            }
        }

        /// <summary>
        /// 未完成的订单
        /// </summary>
        /// <param name="storeId"></param>
        /// <returns></returns>
        public List<AllStoreUnfinishedOrder> GetAllStoreUnfinishedOrder(int storeId, int itop, int itype)
        {
            try
            {
                //1,未审核 2,未转单 3,未调度 4,未收款

                var query = SaleReservationBillsRepository_RO.TableNoTracking
                    .Where(s => s.AuditedStatus == false || s.ChangedStatus == false || s.DispatchedStatus == false || s.Receipted == false);
                var query1 = from p in query group p by p.StoreId into pGroup orderby pGroup.Key select pGroup;
                var reporting = query1.Take(5).ToList().Select(g =>
                {
                    var reasonStatus = 0;
                    var reasonStatusName = "";
                    var bills = g.ToList();
                    var bills1 = g.Where(s => s.AuditedStatus == false).ToList();
                    var bills2 = g.Where(s => s.ChangedStatus == false).ToList();
                    var bills3 = g.Where(s => s.DispatchedStatus == false).ToList();
                    var bills4 = g.Where(s => s.Receipted == false).ToList();

                    if (bills1.Count() > 0)
                    {
                        reasonStatus = 1;
                        reasonStatusName = "未审核";
                        bills = bills1;
                    }
                    else if (bills2.Count() > 0)
                    {
                        reasonStatus = 2;
                        reasonStatusName = "未转单";
                        bills = bills2;
                    }
                    else if (bills3.Count() > 0)
                    {
                        reasonStatus = 3;
                        reasonStatusName = "未调度";
                        bills = bills3;
                    }
                    else if (bills4.Count() > 0)
                    {
                        reasonStatus = 4;
                        reasonStatusName = "未收款";
                        bills = bills4;
                    }

                    return new AllStoreUnfinishedOrder
                    {
                        ReasonStatus = reasonStatus,
                        ReasonStatusName = reasonStatusName,
                        StoreId = g.Key,
                        StoreName = _storeService.GetStoreName(g.Key),
                        TotalSumSub = bills.Select(s => s.SumAmount).Sum(),
                        TotalSumOrderQuantity = bills.Count(),
                    };

                }).ToList();

                return reporting;
            }
            catch (Exception)
            {
                return new List<AllStoreUnfinishedOrder>();
            }
        }

        /// <summary>
        /// 销售
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="itop"></param>
        /// <param name="itype"></param>
        /// <returns></returns>
        public List<HotSaleRanking> GetAllStoreHotSaleRanking(int itop, int itype)
        {
            try
            {
                var orders = _saleReportService.GetHotSaleRanking(null, null, null, null, null, null, null).Take(itop).ToList();
                //量
                if (itype == 1)
                {
                    orders = orders.OrderByDescending(s => s.TotalSumNetQuantity).ToList();
                }
                //额
                else if (itype == 2)
                {
                    orders = orders.OrderByDescending(s => s.TotalSumNetAmount).ToList();
                }

                return orders;
            }
            catch (Exception)
            {
                return new List<HotSaleRanking>();
            }
        }

        /// <summary>
        /// 销订
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="itop"></param>
        /// <param name="itype"></param>
        /// <returns></returns>
        public List<HotSaleRanking> GetAllStoreHotOrderRanking(int itop, int itype)
        {
            try
            {
                var orders = _saleReportService.GetHotOrderRanking(null, null, null, null, null, null, null).Take(itop).ToList();
                //量
                if (itype == 1)
                {
                    orders = orders.OrderByDescending(s => s.TotalSumNetQuantity).ToList();
                }
                //额
                else if (itype == 2)
                {
                    orders = orders.OrderByDescending(s => s.TotalSumNetAmount).ToList();
                }

                return orders;
            }
            catch (Exception)
            {
                return new List<HotSaleRanking>();
            }
        }



        /// <summary>
        /// 获取读取用户所有待审核单据数
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="userIds"></param>
        /// <returns></returns>
        public int GetPendingCount(int storeId, int[] userIds)
        {
            try
            {
                string where = " b.StoreId = " + storeId + " and b.AuditedStatus = 0 and b.ReversedStatus = 0  ";
                if (userIds.Length > 0)
                {
                    where += " and b.MakeUserId in (" + string.Join(",", userIds) + ")";
                }

                string sqlString = $"select sum(bills.count) as `value` from ((select count(*) as count from AdvancePaymentBills as b where {where}) union all(select count(*) as count from AdvanceReceiptBills as b where  {where}) union all(select count(*) as count from AllocationBills as b where  {where}) union all(select count(*) as count from CashReceiptBills as b where  {where}) union all(select count(*) as count from CombinationProductBills as b where  {where})union all(select count(*) as count from CostAdjustmentBills as b where  {where}) union all(select count(*) as count from CostContractBills as b where  {where})union all(select count(*) as count from CostExpenditureBills as b where  {where})union all(select count(*) as count from FinancialIncomeBills as b where  {where})union all(select count(*) as count from InventoryAllTaskBills as b where  {where})union all(select count(*) as count from InventoryPartTaskBills as b where  {where})union all(select count(*) as count from InventoryProfitLossBills as b where  {where})union all(select count(*) as count from PaymentReceiptBills as b where  {where})union all(select count(*) as count from PurchaseBills as b where  {where})union all(select count(*) as count from PurchaseReturnBills as b where  {where})union all(select count(*) as count from ReturnBills as b where  {where})union all(select count(*) as count from ReturnReservationBills as b where  {where})union all(select count(*) as count from SaleBills as b where  {where})union all(select count(*) as count from SaleReservationBills as b where  {where})union all(select count(*) as count from SplitProductBills as b where  {where})) as bills";

                var query = SaleBillsRepository_RO.QueryFromSql<IntQueryType>(sqlString).ToList();
                return query.Select(s => s.Value ?? 0).FirstOrDefault();
            }
            catch (Exception)
            {
                return 0;
            }
        }

        /// <summary>
        /// 获取当前用户所有销售订单数
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="userIds"></param>
        /// <returns></returns>
        public int GetOrderCount(int storeId, int[] userIds)
        {
            try
            {
                string where = " b.StoreId = " + storeId + " and b.AuditedStatus = 1 and b.ReversedStatus = 0  ";
                if (userIds.Length > 0)
                {
                    where += " and b.MakeUserId in (" + string.Join(",", userIds) + ")";
                }

                string sqlString = $"select count(*) as `value`  from SaleReservationBills as b where  {where}";

                var query = SaleBillsRepository_RO.QueryFromSql<IntQueryType>(sqlString).ToList();
                return query.Select(s => s.Value ?? 0).FirstOrDefault();
            }
            catch (Exception)
            {
                return 0;
            }
        }


        /// <summary>
        /// 获取当前用户所有销售单数
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="userIds"></param>
        /// <returns></returns>
        public int GetSaleCount(int storeId, int[] userIds)
        {
            try
            {
                string where = " b.StoreId = " + storeId + " and b.AuditedStatus = 1 and b.ReversedStatus = 0  ";
                if (userIds.Length > 0)
                {
                    where += " and b.MakeUserId in (" + string.Join(",", userIds) + ")";
                }

                string sqlString = $"select count(*) as `value`  from SaleBills as b where  {where}";

                var query = SaleBillsRepository_RO.QueryFromSql<IntQueryType>(sqlString).ToList();
                return query.Select(s => s.Value ?? 0).FirstOrDefault();
            }
            catch (Exception)
            {
                return 0;
            }
        }


        /// <summary>
        /// 获取当前用户所有调拨单数
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="userIds"></param>
        /// <returns></returns>
        public int GetAllocationCount(int storeId, int[] userIds)
        {
            try
            {
                string where = " b.StoreId = " + storeId + " and b.AuditedStatus = 1 and b.ReversedStatus = 0  ";
                if (userIds.Length > 0)
                {
                    where += " and b.MakeUserId in (" + string.Join(",", userIds) + ")";
                }

                string sqlString = $"select count(*) as `value`  from AllocationBills as b where  {where}";

                var query = SaleBillsRepository_RO.QueryFromSql<IntQueryType>(sqlString).ToList();
                return query.Select(s => s.Value ?? 0).FirstOrDefault();
            }
            catch (Exception)
            {
                return 0;
            }
        }


        /// <summary>
        /// 获取当前用户所有待调度数
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="userIds"></param>
        /// <returns></returns>
        public int GetDispatchCount(int storeId, int[] userIds)
        {
            try
            {
                string where = " b.StoreId = " + storeId + " and b.AuditedStatus = 1 and b.ReversedStatus = 0  and b.BillStatus = 0 ";
                if (userIds.Length > 0)
                {
                    where += " and b.MakeUserId in (" + string.Join(",", userIds) + ")";
                }

                string sqlString = $"select count(*) as `value`  from DispatchBills as b where  {where}";

                var query = SaleBillsRepository_RO.QueryFromSql<IntQueryType>(sqlString).ToList();
                return query.Select(s => s.Value ?? 0).FirstOrDefault();
            }
            catch (Exception)
            {
                return 0;
            }
        }


        /// <summary>
        /// 获取当前用户所有退货单数
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="userIds"></param>
        /// <returns></returns>
        public int GetReturnCount(int storeId, int[] userIds)
        {
            try
            {
                string where = " b.StoreId = " + storeId + " and b.AuditedStatus = 1 and b.ReversedStatus = 0  ";
                if (userIds.Length > 0)
                {
                    where += " and b.MakeUserId in (" + string.Join(",", userIds) + ")";
                }

                string sqlString = $"select count(*) as `value`  from ReturnBills as b where  {where}";

                var query = SaleBillsRepository_RO.QueryFromSql<IntQueryType>(sqlString).ToList();
                return query.Select(s => s.Value ?? 0).FirstOrDefault();
            }
            catch (Exception)
            {
                return 0;
            }
        }

        /// <summary>
        /// 获取当前用户所有收款单数
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="userIds"></param>
        /// <returns></returns>
        public int GetCashReceiptCount(int storeId, int[] userIds)
        {
            try
            {
                string where = " b.StoreId = " + storeId + " and b.AuditedStatus = 1 and b.ReversedStatus = 0  ";
                if (userIds.Length > 0)
                {
                    where += " and b.MakeUserId in (" + string.Join(",", userIds) + ")";
                }

                string sqlString = $"select count(*) as `value`  from CashReceiptBills as b where  {where}";

                var query = SaleBillsRepository_RO.QueryFromSql<IntQueryType>(sqlString).ToList();
                return query.Select(s => s.Value ?? 0).FirstOrDefault();
            }
            catch (Exception)
            {
                return 0;
            }
        }

        /// <summary>
        /// 获取当前用户所有待转单数
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="userIds"></param>
        /// <returns></returns>
        public int GetChangeCount(int storeId, int[] userIds)
        {
            try
            {
                string where = " b.StoreId = " + storeId + " and b.AuditedStatus = 1 and b.ReversedStatus = 0  and b.ChangedStatus = 0";
                if (userIds.Length > 0)
                {
                    where += " and b.MakeUserId in (" + string.Join(",", userIds) + ")";
                }

                string sqlString = $"select sum(bills.count) as `value` from ((select count(*) as count from SaleReservationBills as b where  {where}) union all(select count(*) as count from ReturnReservationBills as b where  {where}) ) as bills";

                var query = SaleBillsRepository_RO.QueryFromSql<IntQueryType>(sqlString).ToList();
                return query.Select(s => s.Value ?? 0).FirstOrDefault();
            }
            catch (Exception)
            {
                return 0;
            }
        }
    }
}
