using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Data;
using DCMS.Core.Domain.Census;
using DCMS.Core.Domain.Tasks;
using DCMS.Core.Infrastructure.DependencyManagement;
using DCMS.Services.Events;
using DCMS.Services.Sales;
using DCMS.Services.Tasks;
using DCMS.Services.Terminals;
using DCMS.Services.Users;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using DbF = Microsoft.EntityFrameworkCore.EF;


namespace DCMS.Services.Census
{
    public class VisitStoreService : BaseService, IVisitStoreService
    {
        private readonly IDistrictService _districtService;
        private readonly IUserService _userService;
        private readonly IQueuedMessageService _queuedMessageService;
        private readonly ISaleBillService _saleBillService;
        private readonly ITerminalService _terminalService;
        public VisitStoreService(IServiceGetter getter,
            IStaticCacheManager cacheManager,
            IEventPublisher eventPublisher,
            IDistrictService districtService,
            IUserService userService,
            IQueuedMessageService queuedMessageService,
            ISaleBillService saleBillService
            , ITerminalService terminalService
            ) : base(getter, cacheManager, eventPublisher)
        {
            _districtService = districtService;
            _userService = userService;
            _queuedMessageService = queuedMessageService;
            _saleBillService = saleBillService;
            _terminalService = terminalService; ;
        }


        /// <summary>
        /// 业务员拜访记录
        /// </summary>
        /// <param name="store"></param>
        /// <param name="businessUserId"></param>
        /// <param name="terminalId"></param>
        /// <param name="terminalName"></param>
        /// <param name="districtId"></param>
        /// <param name="channelId"></param>
        /// <param name="visitTypeId"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public IList<VisitStore> GetVisitRecords(int? store, int? businessUserId, int? terminalId, string terminalName, int? districtId, int? channelId, int? visitTypeId, DateTime? start = null, DateTime? end = null, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = VisitStoreRepository_RO.Table;

            if (store.HasValue)
            {
                query = query.Where(c => c.StoreId == store);
            }

            if (businessUserId.HasValue && businessUserId.Value != 0)
            {
                query = query.Where(c => c.BusinessUserId == businessUserId);
            }

            if (terminalId.HasValue && terminalId.Value != 0)
            {
                query = query.Where(c => c.TerminalId == terminalId);
            }
            //客户名称检索
            if (!string.IsNullOrEmpty(terminalName))
            {
                var terminalIds = _terminalService.GetTerminalIds(store, terminalName);
                query = query.Where(a => terminalIds.Contains(a.TerminalId));
            }
            if (districtId.HasValue && districtId.Value != 0)
            {
                //递归片区查询
                var distinctIds = _districtService.GetSubDistrictIds(store ?? 0, districtId ?? 0);
                if (distinctIds != null && distinctIds.Count > 0)
                {
                    string inDistinctIds = string.Join("','", distinctIds);
                    query = query.Where(c => distinctIds.Contains(c.DistrictId ?? 0));
                }
                else
                {
                    query = query.Where(c => c.DistrictId == districtId);
                }
            }

            if (channelId.HasValue && channelId.Value != 0)
            {
                query = query.Where(c => c.ChannelId == channelId);
            }

            if (visitTypeId.HasValue && visitTypeId.Value != 0)
            {
                query = query.Where(c => c.VisitTypeId == visitTypeId);
            }

            if (start.HasValue)
            {
                var startTime = DateTime.Parse(start.Value.ToString("yyyy-MM-dd 00:00:00"));
                query = query.Where(o => o.SigninDateTime >= startTime);
            }

            if (end.HasValue)
            {
                var endTime = DateTime.Parse(end.Value.ToString("yyyy-MM-dd 23:59:59"));
                query = query.Where(o => o.SigninDateTime <= endTime);
            }

            query = query.OrderByDescending(c => c.Id);


            var result = query.Skip(pageIndex * pageSize).Take(pageSize).ToList();

            return result;
        }


        /// <summary>
        /// 业务员拜访记录
        /// </summary>
        /// <param name="store"></param>
        /// <param name="businessUserId"></param>
        /// <param name="terminalId"></param>
        /// <param name="terminalName"></param>
        /// <param name="districtId"></param>
        /// <param name="channelId"></param>
        /// <param name="visitTypeId"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public IPagedList<VisitStore> GetAllVisitRecords(int? store, int? businessUserId, int? terminalId, string terminalName, int? districtId, int? channelId, int? visitTypeId, DateTime? start = null, DateTime? end = null, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = VisitStoreRepository_RO.Table;

            if (store.HasValue)
            {
                query = query.Where(c => c.StoreId == store);
            }

            if (businessUserId.HasValue && businessUserId.Value != 0)
            {
                query = query.Where(c => c.BusinessUserId == businessUserId);
            }

            if (terminalId.HasValue && terminalId.Value != 0)
            {
                query = query.Where(c => c.TerminalId == terminalId);
            }
            //客户名称检索
            if (!string.IsNullOrEmpty(terminalName))
            {
                var terminalIds = _terminalService.GetTerminalIds(store, terminalName);
                query = query.Where(a => terminalIds.Contains(a.TerminalId));
            }
            if (districtId.HasValue && districtId.Value != 0)
            {
                //递归片区查询
                var distinctIds = _districtService.GetSubDistrictIds(store ?? 0, districtId ?? 0);
                if (distinctIds != null && distinctIds.Count > 0)
                {
                    string inDistinctIds = string.Join("','", distinctIds);
                    query = query.Where(c => distinctIds.Contains(c.DistrictId ?? 0));
                }
                else
                {
                    query = query.Where(c => c.DistrictId == districtId);
                }
            }

            if (channelId.HasValue && channelId.Value != 0)
            {
                query = query.Where(c => c.ChannelId == channelId);
            }

            if (visitTypeId.HasValue && visitTypeId.Value != 0)
            {
                query = query.Where(c => c.VisitTypeId == visitTypeId);
            }

            if (start.HasValue)
            {
                var startTime = DateTime.Parse(start.Value.ToString("yyyy-MM-dd 00:00:00"));
                query = query.Where(o => o.SigninDateTime >= startTime);
            }

            if (end.HasValue)
            {
                var endTime = DateTime.Parse(end.Value.ToString("yyyy-MM-dd 23:59:59"));
                query = query.Where(o => o.SigninDateTime <= endTime);
            }

            query = query.OrderByDescending(c => c.Id);

            var lists = new PagedList<VisitStore>(query, pageIndex, pageSize);
            return lists;

        }


        /// <summary>
        /// 业务员拜访达成表
        /// </summary>
        /// <param name="store"></param>
        /// <param name="businessUserId"></param>
        /// <param name="lineId"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public IList<VisitStore> GetVisitReacheds(int? store, int? businessUserId, int? lineId, DateTime? start = null, DateTime? end = null)
        {
            var query = VisitStoreRepository_RO.Table;

            //查询进行了交易的拜访记录
            query = query.Where(c => (c.SaleBillId != null && c.SaleBillId != 0) || (c.SaleReservationBillId != null && c.SaleReservationBillId != 0) || (c.ReturnBillId != null && c.ReturnBillId != 0) || (c.ReturnReservationBillId != null && c.ReturnReservationBillId != 0));

            if (store.HasValue)
            {
                query = query.Where(c => c.StoreId == store);
            }

            if (businessUserId.HasValue && businessUserId.Value != 0)
            {
                query = query.Where(c => c.BusinessUserId == businessUserId);
            }

            if (lineId.HasValue && lineId.Value != 0)
            {
                query = query.Where(c => c.LineId == lineId);
            }

            if (start.HasValue)
            {
                var startTime = DateTime.Parse(start.Value.ToString("yyyy-MM-dd 00:00:00"));
                query = query.Where(o => o.SigninDateTime >= startTime);
            }

            if (end.HasValue)
            {
                var endTime = DateTime.Parse(end.Value.ToString("yyyy-MM-dd 23:59:59"));
                query = query.Where(o => o.SigninDateTime <= endTime);
            }

            query = query.OrderByDescending(c => c.SigninDateTime);

            return query.ToList();
        }

        /// <summary>
        /// 业务员拜访达成表
        /// </summary>
        /// <param name="store"></param>
        /// <param name="businessUserId"></param>
        /// <param name="lineId"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public IPagedList<VisitStore> GetAllVisitReacheds(int? store, int? businessUserId, IList<int> lineIds, DateTime? start = null, DateTime? end = null, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = VisitStoreRepository_RO.Table;

            //查询进行了交易的拜访记录
            //query = query.Where(c => (c.SaleBillId != null && c.SaleBillId != 0) || (c.SaleReservationBillId != null && c.SaleReservationBillId != 0) || (c.ReturnBillId != null && c.ReturnBillId != 0) || (c.ReturnReservationBillId != null && c.ReturnReservationBillId != 0));

            if (store.HasValue)
            {
                query = query.Where(c => c.StoreId == store);
            }

            if (businessUserId.HasValue && businessUserId.Value > 0)
            {
                query = query.Where(c => c.BusinessUserId == businessUserId);
            }

            if (lineIds?.Count > 0)
            {
                var query_terminal = from t in LineTierOptionsRepository_RO.Table
                                     where lineIds.Contains(t.LineTierId) && t.StoreId == store
                                     select t.TerminalId;
                query = query.Where(c => query_terminal.ToList().Contains(c.TerminalId));
            }

            if (start.HasValue)
            {
                var startTime = DateTime.Parse(start.Value.ToString("yyyy-MM-dd 00:00:00"));
                query = query.Where(o => o.SigninDateTime >= startTime);
            }

            if (end.HasValue)
            {
                var endTime = DateTime.Parse(end.Value.ToString("yyyy-MM-dd 23:59:59"));
                query = query.Where(o => o.SigninDateTime <= endTime);
            }

            query = query.OrderByDescending(c => c.SigninDateTime);

            var lists = new PagedList<VisitStore>(query, pageIndex, pageSize);
            return lists;
        }


        public virtual VisitStore GetLastRecord(int? store, int? visitId, int? terminalId, int? businessUserId)
        {
            var query = VisitStoreRepository_RO.Table;

            if (store.HasValue)
            {
                query = query.Where(c => c.StoreId == store);
            }

            if (visitId != 0)
            {
                var currentData = GetRecordById(store, visitId);

                if (currentData != null)
                {
                    query = query.Where(c => c.Id != visitId);
                    query = query.Where(c => c.TerminalId == currentData.TerminalId);
                    query = query.Where(c => c.SigninDateTime < currentData.SigninDateTime);
                }
            }
            else
            {
                query = query.Where(c => c.BusinessUserId == businessUserId && c.TerminalId == terminalId && c.SigninDateTime < DateTime.Now);
            }

            query.OrderByDescending(x => x.SigninDateTime);

            return query.FirstOrDefault();
        }

        /// <summary>
        /// 获取业务员最近签到过(已经签退的）
        /// </summary>
        /// <param name="store"></param>
        /// <param name="terminalId"></param>
        /// <param name="businessUserId"></param>
        /// <returns></returns>
        public virtual VisitStore GetUserLastRecord(int? store, int? terminalId, int? businessUserId)
        {
            var query = from r in VisitStoreRepository_RO.Table
                        where r.StoreId == store && r.TerminalId == terminalId && r.BusinessUserId == businessUserId && r.SignTypeId == 2
                        orderby r.SigninDateTime descending
                        select r;
            return query.ToList().FirstOrDefault();
        }

       /// <summary>
       /// 获取最后一次签到未签退记录
       /// </summary>
       /// <param name="store"></param>
       /// <param name="businessUserId"></param>
       /// <returns></returns>
        public virtual VisitStore GetUserLastOutRecord(int? store, int? businessUserId)
        {
            var query = from r in VisitStoreRepository_RO.Table
                        where r.StoreId == store && r.BusinessUserId == businessUserId && r.SignTypeId == 1
                        orderby r.SigninDateTime descending
                        select r;
            return query.ToList().FirstOrDefault();
        }


        /// <summary>
        /// 检查是否签退
        /// </summary>
        /// <param name="store"></param>
        /// <param name="terminalId"></param>
        /// <param name="businessUserId"></param>
        /// <returns></returns>
        public virtual VisitStore CheckOut(int? store, int? terminalId, int? businessUserId)
        {
            var query = from r in VisitStoreRepository_RO.Table
                        where r.StoreId == store && r.TerminalId == terminalId && r.BusinessUserId == businessUserId && r.SignTypeId == 1
                        orderby r.SigninDateTime descending
                        select r;
            return query.ToList().FirstOrDefault();
        }


        public VisitStore GetRecordById(int? store, int? visitId)
        {
            var query = VisitStoreRepository_RO.Table;

            if (store.HasValue)
            {
                query = query.Where(c => c.StoreId == store);
            }

            if (visitId != 0)
            {
                var currentData = query.Where(x => x.Id == visitId).FirstOrDefault();

                query = from c in query
                        where c.Id == visitId
                        select c;
            }

            return query.FirstOrDefault();
        }

        public IList<VisitStore> GetVisitRecordsByUserid(int? store, int? businessUserId)
        {
            var query = VisitStoreRepository_RO.Table;

            if (store.HasValue)
            {
                query = query.Where(c => c.StoreId == store);
            }

            if (businessUserId.HasValue && businessUserId.Value != 0)
            {
                query = query.Where(c => c.BusinessUserId == businessUserId);
            }

            return query.ToList();
        }


        public IList<VisitStore> GetLastVisitRecordsByTerminalId(int? store, int? terminalId)
        {
            var query = VisitStoreRepository_RO.Table;

            if (store.HasValue)
            {
                query = query.Where(c => c.StoreId == store);
            }

            if (terminalId.HasValue && terminalId.Value != 0)
            {
                query = query.Where(c => c.TerminalId == terminalId);
            }

            return query.OrderByDescending(s => s.SigninDateTime).ToList();
        }


        public IList<VisitStore> GetVisitRecordsByUserid(int? store, int? businessUserId, DateTime? date)
        {
            var query = VisitStoreRepository_RO.TableNoTracking;

            if (store.HasValue)
            {
                query = query.Where(c => c.StoreId == store);
            }

            if (businessUserId.HasValue && businessUserId.Value != 0)
            {
                query = query.Where(c => c.BusinessUserId == businessUserId);
            }

            var result = query.ToList();

            if (date.HasValue)
            {
                //Expression<Func<VisitStore, bool>> where = p => System.Data.Objects.EntityFunctions.DiffDays(p.SigninDateTime, date) == 0;
                //return query.Where(where.Compile()).ToList();

                //result = result.Where(c => MySqlDbFunctionsExtensions.DateDiffDay(c.SigninDateTime, date) == 0).ToList();
                //var ss = MySqlDbFunctionsExtensions.DateDiffDay(date, date);
                //MySqlDbFunctionsExtensions.DateDiffDay(DbF.Functions, date, date);


                result = result.Where(c => MySqlDbFunctionsExtensions.DateDiffDay(DbF.Functions, c.SigninDateTime, date) == 0).ToList();
            }

            return result;
        }

        public IList<VisitStore> GetVisitRecordsByUserid(int? store, int? businessUserId, DateTime? start, DateTime? end)
        {
            var query = VisitStoreRepository_RO.Table;

            if (store.HasValue)
            {
                query = query.Where(c => c.StoreId == store);
            }

            if (businessUserId.HasValue && businessUserId.Value != 0)
            {
                query = query.Where(c => c.BusinessUserId == businessUserId);
            }

            if (start.HasValue)
            {
                var startTime = DateTime.Parse(start.Value.ToString("yyyy-MM-dd 00:00:00"));
                query = query.Where(o => o.SigninDateTime >= startTime);
            }

            if (end.HasValue)
            {
                var endTime = DateTime.Parse(end.Value.ToString("yyyy-MM-dd 23:59:59"));
                query = query.Where(o => o.SigninDateTime <= endTime);
            }

            return query.ToList();
        }


        public virtual IList<DoorheadPhoto> GetDoorheadPhotoByVisitId(int visitId = 0)
        {
            if (visitId != 0)
            {
                var query = DoorheadPhotoRepository_RO.Table;
                query = query.Where(t => t.VisitStoreId.Equals(visitId));
                query = query.OrderByDescending(c => c.VisitStoreId);
                var photos = query.ToList();
                return photos;
            }
            else
            {
                return null;
            }
        }

        public virtual IList<DisplayPhoto> GetDisplayPhotoByVisitId(int visitId = 0)
        {
            if (visitId != 0)
            {
                var query = DisplayPhotoRepository_RO.Table;
                query = query.Where(t => t.VisitStoreId.Equals(visitId));
                query = query.OrderByDescending(c => c.VisitStoreId);
                var photos = query.ToList();
                return photos;
            }
            else
            {
                return null;
            }
        }

        public int InsertDoorheadPhoto(DoorheadPhoto photo)
        {
            try
            {
                if (photo != null)
                {
                    var uow = DoorheadPhotoRepository.UnitOfWork;
                    DoorheadPhotoRepository.Insert(photo);
                    uow.SaveChanges();
                }
                return photo.Id;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public int InsertDisplayPhoto(DisplayPhoto photo)
        {
            try
            {
                if (photo != null)
                {
                    var uow = DisplayPhotoRepository.UnitOfWork;
                    DisplayPhotoRepository.Insert(photo);
                    uow.SaveChanges();
                }
                return photo.Id;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }
        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="record"></param>
        public virtual void InsertVisitStore(VisitStore record)
        {
            if (record == null)
            {
                throw new ArgumentNullException("visitStore");
            }

            var uow = VisitStoreRepository.UnitOfWork;
            VisitStoreRepository.Insert(record);
            uow.SaveChanges();

            _eventPublisher.EntityInserted(record);
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="record"></param>
        public virtual void UpdateVisitStore(VisitStore record)
        {
            if (record == null)
            {
                throw new ArgumentNullException("news");
            }

            var uow = VisitStoreRepository.UnitOfWork;
            VisitStoreRepository.Update(record);
            uow.SaveChanges();

            //事件通知
            _eventPublisher.EntityUpdated(record);
        }



        /// <summary>
        /// 连表合并查询业务员轨迹
        /// </summary>
        /// <param name="store"></param>
        /// <param name="businessUserId"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public IQueryable<QueryVisitStoreAndTracking> GetQueryVisitStoreAndTracking(int? store = 0, int? businessUserId = 0, DateTime? start = null, DateTime? end = null)
        {
            //MSSQL/MYSQL
            var sqlString1 = $"select {store ?? 0} StoreId,BusinessUserId, BusinessUserName,  '1' Ctype, Latitude, Longitude, TerminalName, SigninDateTime as CreateDateTime from VisitStore where StoreId=" + store + " and BusinessUserId =" + businessUserId + " and SigninDateTime >= '" + start.Value.ToString("yyyy-MM-dd 00:00:00") + "' and SigninDateTime <= '" + end.Value.ToString("yyyy-MM-dd 23:59:59") + "'";

            var sqlString2 = $"select {store ?? 0} StoreId,BusinessUserId,BusinessUserName,'0' Ctype,Latitude,Longitude,Address as TerminalName,CreateDateTime from Tracking where StoreId=" + store + " and BusinessUserId =" + businessUserId + "  and CreateDateTime >= '" + start.Value.ToString("yyyy-MM-dd 00:00:00") + "' and CreateDateTime <= '" + end.Value.ToString("yyyy-MM-dd 23:59:59") + "'";

            var lists1 = VisitStoreRepository_RO.QueryFromSql<QueryVisitStoreAndTracking>(sqlString1).ToList();
            var lists2 = TrackingRepository_RO.QueryFromSql<QueryVisitStoreAndTracking>(sqlString2).ToList();

            var query = lists1.Union(lists2);

            return query.OrderBy(s => s.CreateDateTime).AsQueryable();
        }

        /// <summary>
        /// 客户拜访活跃度排行
        /// </summary>
        /// <param name="store"></param>
        /// <param name="businessUserId"></param>
        /// <param name="terminalId"></param>
        /// <returns></returns>
        public IList<VisitStore> GetCustomerActivityRanking(int? store, int? businessUserId = 0, int? terminalId = 0)
        {

            //MSSQL/MYSQL
            var sqlString = "select * from(select *, ROW_NUMBER() over(partition by TerminalId order by SigninDateTime desc) as rn from VisitStore) a where a.StoreId = " + store + " and a.rn <= 1 ";

            if (businessUserId.HasValue && businessUserId.Value != 0)
            {
                sqlString += " and BusinessUserId = " + businessUserId + "";
            }

            if (terminalId.HasValue && terminalId.Value != 0)
            {
                sqlString += " and TerminalId = " + terminalId + "";
            }

            var query = VisitStoreRepository_RO.EntityFromSql<VisitStore>(sqlString);
            return query.ToList();
        }

        /// <summary>
        /// 连表合并查询业务员轨迹(WEB端使用)
        /// </summary>
        /// <param name="store"></param>
        /// <param name="businessUserId"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public IQueryable<QueryVisitStoreAndTracking> GetQueryVisitStoreAndTrackingForWeb(int? store = 0, int? businessUserId = 0, DateTime? start = null, DateTime? end = null)
        {
            //MSSQL
            //var sqlString = "select * from ((select storeId??0,BusinessUserId,BusinessUserName,Ctype=N'0',Latitude,Longitude,[Address] as TerminalName,CreateDateTime from [Census].[dbo].Tracking) union all(select storeId??0,BusinessUserId, BusinessUserName, Ctype = N'1', Latitude, Longitude, TerminalName, SigninDateTime as CreateDateTime from VisitStore)) as Query where StoreId=" + store + " and BusinessUserId =" + businessUserId + " ";
            //if (start.HasValue)
            //    sqlString += " and datediff(day,CreateDateTime,'" + start.Value.ToString("yyyy-MM-dd 00:00:00") + "')=0";
            //if (end.HasValue)
            //    sqlString += " and CreateDateTime <= '" + end.Value.ToString("yyyy-MM-dd 23:59:59") + "'";
            //sqlString += "  order by  CreateDateTime";
            //var query = VisitStoreRepository.QueryFromSql<QueryVisitStoreAndTracking>(sqlString);
            //return query.AsQueryable();

            //MYSQL
            var sqlString1 = $"select storeId,BusinessUserId, BusinessUserName, '1' Ctype, Latitude, Longitude, TerminalName, SigninDateTime as CreateDateTime from dcms.VisitStore where StoreId=" + store + " and BusinessUserId =" + businessUserId + "";

            var sqlString2 = $"select storeId,BusinessUserId,BusinessUserName,'0' Ctype,Latitude,Longitude,Address as TerminalName,CreateDateTime from census.Tracking where StoreId=" + store + " and BusinessUserId =" + businessUserId + "";

            if (start.HasValue)
            {
                sqlString1 += $" and datediff(SigninDateTime,date_format('" + start.Value.ToString("yyyy-MM-dd 00:00:00") + "','%Y%m%d')) = 0";
                sqlString2 += $" and datediff(CreateDateTime,date_format('" + start.Value.ToString("yyyy-MM-dd 00:00:00") + "','%Y%m%d')) = 0";
            }

            if (end.HasValue)
            {
                sqlString1 += $" and SigninDateTime <= '" + end.Value.ToString("yyyy-MM-dd 23:59:59") + "'";
                sqlString2 += $" and CreateDateTime <= '" + end.Value.ToString("yyyy-MM-dd 23:59:59") + "'";
            }

            var lists1 = VisitStoreRepository_RO.QueryFromSql<QueryVisitStoreAndTracking>(sqlString1).ToList();
            var lists2 = TrackingRepository_RO.QueryFromSql<QueryVisitStoreAndTracking>(sqlString2).ToList();

            var query = lists1.Union(lists2);
            return query.OrderBy(s => s.CreateDateTime).AsQueryable();

        }


        public BaseResult SignInVisitStore(int? storeId, int userId, VisitStore visitStore, VisitStore data, bool isAdmin = false)
        {
            var uow = AdvanceReceiptBillsRepository.UnitOfWork;

            ITransaction transaction = null;
            try
            {

                transaction = uow.BeginOrUseTransaction();


                var lastVisited = new VisitStore();
                {
                    #region 拜访签到

                    visitStore.SignTypeId = 1;
                    visitStore.SignType = SignEnum.CheckIn;
                    visitStore.DistrictId = data.DistrictId ?? 0;
                    visitStore.ChannelId = data.ChannelId ?? 0;
                    visitStore.NextOrderDays = data.NextOrderDays ?? 0;
                    visitStore.LastSigninDateTime = 0;
                    visitStore.LastPurchaseDateTime = 0;
                    visitStore.OnStoreStopSeconds = data.OnStoreStopSeconds ?? 0;
                    visitStore.LineId = data.LineId ?? 0;
                    visitStore.Latitude = data.Latitude ?? 0;
                    visitStore.Longitude = data.Longitude ?? 0;
                    visitStore.SaleBillId = data.SaleBillId ?? 0;
                    visitStore.SaleReservationBillId = data.SaleReservationBillId ?? 0;
                    visitStore.ReturnBillId = data.ReturnBillId ?? 0;
                    visitStore.ReturnReservationBillId = data.ReturnReservationBillId ?? 0;
                    visitStore.SaleAmount = data.SaleAmount ?? 0;
                    visitStore.SaleOrderAmount = data.SaleOrderAmount ?? 0;
                    visitStore.ReturnAmount = data.ReturnAmount ?? 0;
                    visitStore.ReturnOrderAmount = data.ReturnOrderAmount ?? 0;
                    visitStore.NoVisitedDays = data.NoVisitedDays ?? 0;
                    InsertVisitStore(visitStore);
                    //InsertAdvanceReceiptBill(advanceReceiptBill);

                    #endregion
                }

                if (visitStore.Id > 0)
                {
                    //这里获取上一次 签到记录  
                    lastVisited = GetLastRecord(storeId, 0, data.TerminalId, data.BusinessUserId);

                    //上次签到时间
                    lastVisited.LastSigninDateTime = data.SigninDateTime.Subtract(lastVisited.SigninDateTime).Days;

                    //这里获取上一次开单记录 SaleBillId
                    // 条件为： TerminalId BusinessUserId
                    var sales = _saleBillService.GetLastSaleBill(storeId, data.TerminalId,
                    data.BusinessUserId);

                    if (sales != null)
                    {
                        //上次采购时间
                        lastVisited.LastPurchaseDateTime = data.SigninDateTime.Subtract(sales.CreatedOnUtc).Days;
                    }


                }



                //发送通知
                try
                {
                    //获取当前用户管理员用户 电话号码 多个用"|"分隔
                    var adminNumbers = _userService.GetAllAdminUserMobileNumbersByStore(storeId ?? 0).ToList();
                    QueuedMessage queuedMessage = new QueuedMessage()
                    {
                        StoreId = storeId ?? 0,
                        MType = MTypeEnum.CheckException,
                        Title = CommonHelper.GetEnumDescription<MTypeEnum>(MTypeEnum.CheckException),
                        BillType = BillTypeEnum.Other,
                        BillId = visitStore.Id,
                        CreatedOnUtc = DateTime.Now,
                        TerminalName = data.TerminalName,
                    };
                    _queuedMessageService.InsertQueuedMessage(adminNumbers,queuedMessage);


                }
                catch (Exception ex)
                {
                    _queuedMessageService.WriteLogs(ex.Message);
                }




                //保存事务
                transaction.Commit();

                return new BaseResult { Success = true, Message = Resources.Bill_CreateOrUpdateSuccessful };
            }
            catch (Exception)
            {
                //如果事务不存在或者为控则回滚
                transaction?.Rollback();
                return new BaseResult { Success = false, Message = Resources.Bill_CreateOrUpdateFailed };
            }
            finally
            {
                //不管怎样最后都会关闭掉这个事务
                using (transaction) { }
            }
        }




        public IList<ReachQuery> GetReachs(int? store, int? userId = 0, DateTime? start = null, DateTime? end = null)
        {
            var sqlString = @" SELECT 
                                u.Id AS `UserId`,
                                u.UserRealName AS `UserName`,
                                IFNULL(lm.LineTierId, 0) AS LineId,
                                IFNULL(l.Name, 0) AS LineName,
                                v.TerminalId AS TerminalId,
                                v.TerminalName AS TerminalName,
                                (CASE v.SignTypeId
                                    WHEN 2 THEN 1
                                    ELSE 0
                                END) AS `Status`,
                                v.OnStoreStopSeconds AS OnStoreSeconds,
                                v.SigninDateTime AS SigninTime
                            FROM
                                auth.Users AS u
                                    INNER JOIN
                                auth.User_UserRole_Mapping AS urm ON u.id = urm.UserId
                                    INNER JOIN
                                auth.UserRoles AS ur ON urm.UserRoleId = ur.id
                                    AND ur.SystemName = 'Salesmans'
                                    LEFT JOIN
                                dcms.LineTierUserMapping AS lm ON u.Id = lm.UserId
                                    LEFT JOIN
                                dcms.LineTiers AS l ON l.id = lm.LineTierId
                                    LEFT JOIN
                                dcms.VisitStore AS v ON u.id = v.BusinessUserId
                            WHERE
                                u.StoreId = "+ store??0 + "  AND lm.LineTierId > 0";

            if (userId.HasValue && userId > 0)
                sqlString += @" AND u.Id = "+ userId + "";

            if (start.HasValue)
                sqlString += @" AND v.SigninDateTime >= '" + start.Value.ToString("yyyy-MM-dd 00:00:00") + "'";

            if (end.HasValue)
                sqlString += @" AND v.SigninDateTime <= '" + end.Value.ToString("yyyy-MM-dd 23:59:59") + "'";


            sqlString += @" ORDER BY u.Id , lm.LineTierId , v.SigninDateTime DESC;";

            var query = VisitStoreRepository_RO.QueryFromSql<ReachQuery>(sqlString);

            return query.ToList();
        }


        public IList<ReachOnlineQuery> GetLineReachs(int? store, int? userId = 0)
        {
            try
            {
                var sqlString = @"SELECT 
                                u.Id AS `UserId`,
                                u.UserRealName AS `UserName`,
                                IFNULL(lm.LineTierId, 0) AS LineId,
                                IFNULL(l.Name, 0) AS LineName,
                                IFNULL(ct.Id, 0) AS TerminalId,
                                IFNULL(ct.Name, '') AS TerminalName
                            FROM
                                auth.Users AS u
                                    INNER JOIN
                                auth.User_UserRole_Mapping AS urm ON u.id = urm.UserId
                                    INNER JOIN
                                auth.UserRoles AS ur ON urm.UserRoleId = ur.id
                                    AND ur.SystemName = 'Salesmans'
                                    LEFT JOIN
                                dcms.LineTierUserMapping AS lm ON u.Id = lm.UserId
                                    LEFT JOIN
                                dcms.LineTiers AS l ON l.id = lm.LineTierId
                                    LEFT JOIN
                                dcms.LineTierOptions AS lo ON lm.LineTierId = lo.LineTierId
                                    LEFT JOIN
                                dcms_crm.CRM_Terminals AS ct ON lo.TerminalId = ct.Id
                            WHERE
                                u.StoreId = " + store ?? 0 + "  AND lm.LineTierId > 0";

                if (userId.HasValue && userId > 0)
                    sqlString += @" AND u.Id = " + userId + "";


                sqlString += @" ORDER BY u.Id , lm.LineTierId;";

                var query = VisitStoreRepository_RO.QueryFromSql<ReachOnlineQuery>(sqlString);

                return query.ToList();
            }
            catch(Exception ex)
            {
                return new List<ReachOnlineQuery>();
            }
        }





    }
}
