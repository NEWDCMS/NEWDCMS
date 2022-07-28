using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Domain.Report;
using DCMS.Core.Infrastructure.DependencyManagement;
using DCMS.Services.Events;
using DCMS.Services.Products;
using DCMS.Services.Terminals;
using MathNet.Numerics.Statistics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DCMS.Services.Report
{
    /// <summary>
    /// 市场报表
    /// </summary>
    public class MarketReportService : BaseService, IMarketReportService
    {
        private readonly IDistrictService _districtService;
        private readonly ICategoryService _categoryService;

        public MarketReportService(IServiceGetter getter,
            IStaticCacheManager cacheManager,
            IEventPublisher eventPublisher,
            IDistrictService districtService,
            ICategoryService categoryService
            ) : base(getter, cacheManager, eventPublisher)
        {
            _districtService = districtService;
            _categoryService = categoryService;
        }

        /// <summary>
        /// 客户活跃度
        /// </summary>
        /// <param name="storeId">经销商Id</param>
        /// <param name="noVisitDayMore">无拜访天数大于</param>
        /// <param name="terminalId">客户Id</param>
        /// <param name="noSaleDayMore">无销售天数大于</param>
        /// <param name="districtId">客户片区Id</param>
        /// <param name="staffUserId">员工Id</param>
        /// <returns></returns>
        public IList<MarketReportTerminalActive> GetMarketReportTerminalActive(int? storeId, int? noVisitDayMore, int? terminalId, string terminalName, int? noSaleDayMore, int? districtId, int? staffUserId)
        {
            try
            {
                terminalName = CommonHelper.Filter(terminalName);

                var reporting = new List<MarketReportTerminalActive>();

                string whereQuery = $" a.StoreId = {storeId ?? 0} ";
                string whereQuery2 = $" a.StoreId = {storeId ?? 0} ";
                string whereQuery3 = $" 1=1 ";


                if (noSaleDayMore.HasValue && noSaleDayMore.Value != 0)
                {
                    whereQuery3 += $" and alls.NoSaleDays = '{noSaleDayMore}' ";
                }
                else
                {
                    whereQuery3 += $" or alls.NoSaleDays >0  ";
                }

                if (noVisitDayMore.HasValue && noVisitDayMore.Value != 0)
                {
                    whereQuery3 += $" and alls.NoVisitDays = '{noVisitDayMore}' ";
                }
                else
                {
                    whereQuery3 += $" or alls.NoVisitDays >0  ";
                }

                if (staffUserId.HasValue && staffUserId.Value != 0)
                {
                    whereQuery2 += $" and sb.BusinessUserId = '{staffUserId}' ";
                }

                if (terminalId.HasValue && terminalId.Value != 0)
                {
                    whereQuery += $" and a.TerminalId = '{terminalId}' ";
                }
                if (terminalName != null)
                {
                    whereQuery += $" and a.Name like '%{terminalName}%' ";
                }

                if (districtId.HasValue && districtId.Value != 0)
                {
                    //递归片区查询
                    var distinctIds = _districtService.GetSubDistrictIds(storeId ?? 0, districtId ?? 0);
                    if (distinctIds != null && distinctIds.Count > 0)
                    {
                        string inDistinctIds = string.Join("','", distinctIds);
                        whereQuery2 += $" and a.DistrictId in ('{inDistinctIds}') ";
                    }
                    else
                    {
                        whereQuery += $" and a.DistrictId = '{districtId}' ";
                    }
                }

                //MSSQL
                //string sqlString = $"select * from (select a.Id as TerminalId,a.Name as TerminalName,a.Code as TerminalCode,NoVisitDays = isnull( (select top 1 DATEDIFF(day,sb.SigninDateTime,GETDATE()) from VisitStore as sb where sb.TerminalId = a.Id order by sb.SigninDateTime desc),0),NoSaleDays = isnull( (select top 1 DATEDIFF(day,sb.CreatedOnUtc,GETDATE()) from SaleBills as sb where {whereQuery2} and sb.TerminalId = a.Id order by sb.CreatedOnUtc desc),0),a.DistrictId,d.Name DistrictName from dcms_crm.CRM_Terminals as a left join  Districts as d on a.DistrictId = d.Id where {whereQuery} ) as alls where {whereQuery3} order by alls.NoSaleDays desc";

                //MySQl
                string sqlString = $"select * from (select a.Id as TerminalId,a.Name as TerminalName,a.Code as TerminalCode, IFNULL( (select datediff(current_date,date_format(sb.SigninDateTime,'%Y%m%d')) from VisitStore as sb where sb.TerminalId = a.Id order by sb.SigninDateTime desc limit 1),0) as NoVisitDays, IFNULL( (select datediff(current_date,date_format(sb.CreatedOnUtc,'%Y%m%d')) from SaleBills as sb where {whereQuery2} and sb.TerminalId = a.Id order by sb.CreatedOnUtc desc limit 1),0) as NoSaleDays,a.DistrictId,d.Name DistrictName from dcms_crm.CRM_Terminals as a left join  Districts as d on a.DistrictId = d.Id where {whereQuery} ) as alls where {whereQuery3} order by alls.NoSaleDays desc";

                reporting = SaleBillsRepository_RO.QueryFromSql<MarketReportTerminalActive>(sqlString).ToList();
                return reporting;
            }
            catch (Exception)
            {
                return new List<MarketReportTerminalActive>();
            }
        }

        /// <summary>
        /// 客户价值分析
        /// </summary>
        /// <param name="storeId">经销商Id</param>
        /// <param name="terminalId">客户Id</param>
        /// <param name="districtId">客户片区Id</param>
        /// <returns></returns>
        public IList<MarketReportTerminalValueAnalysis> GetMarketReportTerminalValueAnalysis(int? storeId, int? terminalId, string terminalName, int? districtId)
        {
            terminalName = CommonHelper.Filter(terminalName);

            var reporting = new List<MarketReportTerminalValueAnalysis>();

            try
            {
                string whereQuery = $" a.StoreId = {storeId ?? 0} ";

                if (terminalId.HasValue && terminalId.Value != 0)
                {
                    whereQuery += $" and a.TerminalId = '{terminalId}' ";
                }
                if (terminalName != null)
                {
                    whereQuery += $" and a.Name like '%{terminalName}%' ";
                }
                if (districtId.HasValue && districtId.Value != 0)
                {
                    // 递归片区查询
                    var distinctIds = _districtService.GetSubDistrictIds(storeId ?? 0, districtId ?? 0);
                    if (distinctIds != null && distinctIds.Count > 0)
                    {
                        string inDistinctIds = string.Join("','", distinctIds);
                        whereQuery += $" and a.DistrictId in ('{inDistinctIds}') ";
                    }
                    else
                    {
                        whereQuery += $" and a.DistrictId = '{districtId}' ";
                    }
                }

                //MSSQL
                //string sqlString = $"select a.Id as TerminalId,a.Name as TerminalName,a.Code as TerminalCode,NoPurchaseDays = isnull( (select top 1 DATEDIFF(day,sb.CreatedOnUtc,GETDATE()) from SaleBills as sb where sb.StoreId = {storeId ?? 0}  and sb.TerminalId = a.Id and sb.CreatedOnUtc >= DATEADD(day,-90,GETDATE()) order by sb.CreatedOnUtc desc),case 0 when 0 then 90 end),PurchaseNumber = (select count(*) from SaleBills as sb where sb.StoreId = {storeId??0}  and sb.TerminalId = a.Id and sb.CreatedOnUtc >= DATEADD(day,-90,GETDATE())),PurchaseAmount = isnull( (select sum(sb.SumAmount) from SaleBills as sb where sb.StoreId = {storeId ?? 0}  and sb.TerminalId = a.Id and sb.CreatedOnUtc >= DATEADD(day,-90,GETDATE())),0),NoVisitDays = isnull( (select top 1 DATEDIFF(day,sb.SigninDateTime,GETDATE()) from VisitStore as sb where sb.TerminalId = a.Id and sb.SigninDateTime >= DATEADD(day,-90,GETDATE()) order by sb.SigninDateTime desc),case 0 when 0 then 90 end),d.Name as DistrictName from dcms_crm.CRM_Terminals as a left  join Districts as d on a.DistrictId = d.Id where {whereQuery}";

                //MYSQL
                string sqlString = $"select a.Id as TerminalId,a.Name as TerminalName,a.Code as TerminalCode, ifnull(d.Id,0) as DistrictId, IFNULL((select datediff(current_date,date_format(sb.CreatedOnUtc,'%Y%m%d')) from SaleBills as sb where sb.StoreId = {storeId ?? 0}  and sb.TerminalId = a.Id and sb.CreatedOnUtc >= date_add(current_date, interval - 90 day) order by sb.CreatedOnUtc desc limit 1),case 0 when 0 then 90 end) as NoPurchaseDays,(select count(*) from SaleBills as sb where sb.StoreId = {storeId ?? 0}  and sb.TerminalId = a.Id and sb.CreatedOnUtc >= date_add(current_date, interval - 90 day)) as PurchaseNumber,IFNULL((select sum(sb.SumAmount) from SaleBills as sb where sb.StoreId = {storeId ?? 0}  and sb.TerminalId = a.Id and sb.CreatedOnUtc >= date_add(current_date, interval - 90 day)),0) as PurchaseAmount,IFNULL((select  datediff(current_date, date_format(sb.SigninDateTime, '%Y%m%d')) from VisitStore as sb where sb.TerminalId = a.Id and sb.SigninDateTime >= date_add(current_date, interval - 90 day) order by sb.SigninDateTime desc limit 1),case 0 when 0 then 90 end) as NoVisitDays,d.Name as DistrictName,0 as R_S,0 as F_S,0 as M_S, 0 as RFMScore, 0 as TerminalTypeId, '' as TerminalTypeName from dcms_crm.CRM_Terminals as a left join Districts as d on a.DistrictId = d.Id where {whereQuery}";


                var datas = SaleBillsRepository_RO.QueryFromSql<MarketReportTerminalValueAnalysis>(sqlString).ToList();

                //关于DCMS 客户价值识别挖掘的模型与算法

                //5阶(RFM)模型
                //重要价值客户/重要保持客户/重要发展客户/重要挽留客户/一般价值客户

                //指标：
                //未采购天数(R)
                //采购次数(F)
                //采购额度(M)
                //1，评分
                //方法一：采用5分制为RFM三个维度的值赋予一个评分值。对于F、M变量来讲，值越大代表购买购买频率越高、订单金额越高；但对R来讲，值越小代表离截止时间点越近。然后将三个值拼接到一起，例如RFM得分为312、333、132。
                //方法二：将RFM三个维度的值进行加权后相加求得一个新的汇总值。
                //方法三：分别求每个用户的R,F,M与R平均值、F平均值、M平均值的差，将差值 > 1的数据赋值为‘1’，差值 <= 1赋值为‘0’。这个过程分别将RFM三个维度的数据分为两种结果（1或者0）。接着将RFM三个值组合拼接，得到8个数值：111、011、101、110、001、010、100、000；再将8个数值赋值为8类标签：'011':'重要保持客户'，'101':'重要发展客户'，'001':'重要挽留客户'，'110':'一般价值客户'，'010':'一般保持客户'，'100':'一般发展客户'，'000':'一般挽留客户'。

                //1.取 R，F，M，对应数据
                var rs = datas.Select(s => (double)s.NoPurchaseDays).OrderBy(s => s).ToList();
                var fs = datas.Select(s => (double)s.PurchaseNumber).OrderBy(s => s).ToList();
                var ms = datas.Select(s => (double)s.PurchaseAmount).OrderBy(s => s).ToList();

                //2.计算R，F，M，对应5分位分数
                var rQuantiles = rs.FiveNumberSummary();
                var fQuantiles = fs.FiveNumberSummary();
                var mQuantiles = ms.FiveNumberSummary();

                //3.确认R，F，M 区间，并打分
                foreach (var col in datas)
                {
                    //Rank范围 [0.0, 0.25, 0.50, 0.75, 1.0]
                    var r_rank = Statistics.QuantileRank(rQuantiles, col.NoPurchaseDays, col.NoPurchaseDays == 0 ? RankDefinition.Min : RankDefinition.Average);
                    var f_rank = Statistics.QuantileRank(fQuantiles, col.PurchaseNumber, col.PurchaseNumber == 0 ? RankDefinition.Min : RankDefinition.Average);
                    var m_rank = Statistics.QuantileRank(mQuantiles, (double)col.PurchaseAmount, col.PurchaseAmount == 0 ? RankDefinition.Min : RankDefinition.Average);

                    //打分范围 [1-5]
                    col.R_S = MatchRScore(r_rank);
                    col.F_S = MatchFMScore(f_rank);
                    col.M_S = MatchFMScore(m_rank);

                    //评分应用
                    col.RFMScore = 100 * (int)col.R_S + 10 * (int)col.F_S + (int)col.M_S;

                    var r = col.R_S;
                    var f = col.F_S;
                    var m = col.M_S;

                    if (r > 3 && f > 3 && m > 3)
                    {
                        col.TerminalTypeId = 8;
                        col.TerminalTypeName = "高价值客户";
                    }
                    else if (r <= 3 && f > 3 && m > 3)
                    {
                        col.TerminalTypeId = 7;
                        col.TerminalTypeName = "高重点保护客户";
                    }
                    else if (r > 3 && f <= 3 && m > 3)
                    {
                        col.TerminalTypeId = 6;
                        col.TerminalTypeName = "重点发展客户";
                    }
                    else if (r <= 3 && f <= 3 && m > 3)
                    {
                        col.TerminalTypeId = 5;
                        col.TerminalTypeName = "重点挽留客户";
                    }
                    else if (r > 3 && f > 3 && m <= 3)
                    {
                        col.TerminalTypeId = 4;
                        col.TerminalTypeName = "一般价值客户";
                    }
                    else if (r <= 3 && f > 3 && m <= 3)
                    {
                        col.TerminalTypeId = 3;
                        col.TerminalTypeName = "一般保持客户";
                    }
                    else if (r > 3 && f <= 3 && m <= 3)
                    {
                        col.TerminalTypeId = 2;
                        col.TerminalTypeName = "一般发展客户";
                    }
                    else if (r <= 3 && f <= 3 && m <= 3)
                    {
                        col.TerminalTypeId = 1;
                        col.TerminalTypeName = "潜在客户";
                    }
                }

                #region 

                //4.用K-means聚类算法将用户进行分组，从而找出最具有价值的用户群
                //聚族数
                //int numClusters = 5;
                //string[][] rawIndex = new string[datas.Count][];
                //double[][] rawData = new double[datas.Count][];
                //for (int i = 0; i < datas.Count; i++)
                //{
                //    rawIndex[i] = new string[]
                //    {
                //        i.ToString(),
                //        (datas[i].TerminalId ?? 0).ToString(),
                //        datas[i].TerminalName,
                //        datas[i].NoPurchaseDays.ToString(),
                //        datas[i].PurchaseNumber.ToString(),
                //        datas[i].PurchaseAmount.ToString(),
                //        datas[i].RFMScore.ToString(),
                //    };
                //    //未采购天数得分(R_S) / 采购次数得分(F_S) / 采购额度得分(M_S)
                //    rawData[i] = new double[] { datas[i].R_S, datas[i].F_S, datas[i].M_S };
                //}

                //Clusterer c = new Clusterer(numClusters);
                //int[] clustering = c.Cluster(rawData);

                //for (int k = 0; k < numClusters; ++k)
                //{
                //    for (int i = 0; i < rawData.Length; ++i)
                //    {
                //        var report = new MarketReportTerminalValueAnalysis();
                //        int clusterID = clustering[i];
                //        if (clusterID != k) continue;

                //        report.TerminalTypeId = k;
                //        report.TerminalId = int.Parse(rawIndex[i][1]);
                //        report.TerminalName = rawIndex[i][2];
                //        report.NoPurchaseDays = int.Parse(rawIndex[i][3]);
                //        report.PurchaseNumber = int.Parse(rawIndex[i][4]);
                //        report.PurchaseAmount = decimal.Parse(rawIndex[i][5]);
                //        report.RFMScore = int.Parse(rawIndex[i][6]);

                //        var r = rawData[i][0];
                //        var f = rawData[i][1];
                //        var m = rawData[i][2];

                //        //var median = Statistics.Median(new double[] { 1, 2, 3, 4, 5 });//=3

                //        //if (r > 3 && f > 3 && m > 3)
                //        //    report.TerminalTypeName = "高价值客户";
                //        //else if (r <= 3 && f > 3 && m > 3)
                //        //    report.TerminalTypeName = "高重点保护客户";
                //        //else if (r > 3 && f <= 3 && m > 3)
                //        //    report.TerminalTypeName = "重点发展客户";
                //        //else if (r <= 3 && f <= 3 && m > 3)
                //        //    report.TerminalTypeName = "重点挽留客户";
                //        //else if (r > 3 && f > 3 && m <= 3)
                //        //    report.TerminalTypeName = "一般价值客户";
                //        //else if (r <= 3 && f > 3 && m <= 3)
                //        //    report.TerminalTypeName = "一般保持客户";
                //        //else if (r > 3 && f <= 3 && m <= 3)
                //        //    report.TerminalTypeName = "一般发展客户";
                //        //else if (r <= 3 && f <= 3 && m <= 3)
                //        //    report.TerminalTypeName = "潜在客户";
                //        //else
                //        //    report.TerminalTypeName = "";

                //        reporting.Add(report);
                //    }
                //}

                #endregion

                reporting = datas;

                return reporting;
            }
            catch (Exception)
            {
                return reporting;
            }
        }


        /// <summary>
        /// 对于R日期间隔越小越好
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        static public int MatchRScore(double x)
        {
            if (x <= 0.0)
            {
                return 5;
            }
            else if (x <= 0.25)
            {
                return 4;
            }
            else if (x <= 0.50)
            {
                return 3;
            }
            else if (x <= 0.75)
            {
                return 2;
            }
            else
            {
                return 1;
            }
        }

        /// <summary>
        /// 对于FM消费频次和金额越大越好
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        private static int MatchFMScore(double x)
        {


            if (x <= 0.0)
            {
                return 1;
            }
            else if (x <= 0.25)
            {
                return 2;
            }
            else if (x <= 0.50)
            {
                return 3;
            }
            else if (x <= 0.75)
            {
                return 4;
            }
            else
            {
                return 5;
            }
        }


        /// <summary>
        /// 客户流失预警
        /// </summary>
        /// <param name="storeId">经销商Id</param>
        /// <param name="terminalId">客户Id</param>
        /// <param name="districtId">客户片区Id</param>
        /// <param name="terminalValueId">客户价值Id</param>
        /// <returns></returns>
        public IList<MarketReportTerminalLossWarning> GetMarketReportTerminalLossWarning(int? storeId, int? terminalId, string terminalName, int? districtId)
        {

            terminalName = CommonHelper.Filter(terminalName);

            var reporting = new List<MarketReportTerminalLossWarning>();

            try
            {
                string whereQuery = $" a.StoreId = {storeId ?? 0} ";

                if (terminalId.HasValue && terminalId.Value != 0)
                {
                    whereQuery += $" and a.TerminalId = '{terminalId}' ";
                }
                if (terminalName != null)
                {
                    whereQuery += $" and a.Name like '%{terminalName}%' ";
                }
                if (districtId.HasValue && districtId.Value != 0)
                {
                    //递归片区查询
                    var distinctIds = _districtService.GetSubDistrictIds(storeId ?? 0, districtId ?? 0);
                    if (distinctIds != null && distinctIds.Count > 0)
                    {
                        string inDistinctIds = string.Join("','", distinctIds);
                        whereQuery += $" and a.DistrictId in ('{inDistinctIds}') ";
                    }
                    else
                    {
                        whereQuery += $" and a.DistrictId = '{districtId}' ";
                    }
                }

                //MSSQL
                //string sqlString = $"select a.Id as TerminalId,a.Name as TerminalName,a.Code as TerminalCode,NoPurchaseDays = isnull( (select top 1 DATEDIFF(day,sb.CreatedOnUtc,GETDATE()) from SaleBills as sb where sb.StoreId = {storeId ?? 0}  and sb.TerminalId = a.Id and sb.CreatedOnUtc >= DATEADD(day,-90,GETDATE()) order by sb.CreatedOnUtc desc),case 0 when 0 then 90 end),PurchaseNumber = (select count(*) from SaleBills as sb where sb.StoreId = {storeId??0}  and sb.TerminalId = a.Id and sb.CreatedOnUtc >= DATEADD(day,-90,GETDATE())),PurchaseAmount = isnull( (select sum(sb.SumAmount) from SaleBills as sb where sb.StoreId = {storeId ?? 0}  and sb.TerminalId = a.Id and sb.CreatedOnUtc >= DATEADD(day,-90,GETDATE())),0),NoVisitDays = isnull( (select top 1 DATEDIFF(day,sb.SigninDateTime,GETDATE()) from VisitStore as sb where sb.TerminalId = a.Id and sb.SigninDateTime >= DATEADD(day,-90,GETDATE()) order by sb.SigninDateTime desc),case 0 when 0 then 90 end),d.Name as DistrictName from dcms_crm.CRM_Terminals as a left  join Districts as d on a.DistrictId = d.Id where {whereQuery}";

                //MYSQL
                string sqlString = $"select a.Id as TerminalId,a.Name as TerminalName,a.DistrictId,a.Code as TerminalCode,IFNULL((select datediff(current_date,date_format(sb.CreatedOnUtc,'%Y%m%d')) from SaleBills as sb where sb.StoreId = {storeId ?? 0}  and sb.TerminalId = a.Id and sb.CreatedOnUtc >= date_add(current_date, interval - 90 day) order by sb.CreatedOnUtc desc limit 1),case 0 when 0 then 90 end) as NoPurchaseDays,(select count(*) from SaleBills as sb where sb.StoreId = {storeId ?? 0}  and sb.TerminalId = a.Id and sb.CreatedOnUtc >= date_add(current_date, interval - 90 day)) as PurchaseNumber,IFNULL((select sum(sb.SumAmount) from SaleBills as sb where sb.StoreId = {storeId ?? 0}  and sb.TerminalId = a.Id and sb.CreatedOnUtc >= date_add(current_date, interval - 90 day)),0) as PurchaseAmount,IFNULL((select  datediff(current_date, date_format(sb.SigninDateTime, '%Y%m%d')) from VisitStore as sb where sb.TerminalId = a.Id and sb.SigninDateTime >= date_add(current_date, interval - 90 day) order by sb.SigninDateTime desc limit 1),case 0 when 0 then 90 end) as NoVisitDays,d.Name as DistrictName from dcms_crm.CRM_Terminals as a left join Districts as d on a.DistrictId = d.Id where {whereQuery}";

                var datas = SaleBillsRepository_RO.QueryFromSql<MarketReportTerminalLossWarning>(sqlString).ToList();

                //关于DCMS 客户价值识别挖掘的模型与算法

                //5阶(RFM)模型
                //重要价值客户/重要保持客户/重要发展客户/重要挽留客户/一般价值客户

                //指标：
                //未采购天数(R)
                //采购次数(F)
                //采购额度(M)
                //1，评分
                //方法一：采用5分制为RFM三个维度的值赋予一个评分值。对于F、M变量来讲，值越大代表购买购买频率越高、订单金额越高；但对R来讲，值越小代表离截止时间点越近。然后将三个值拼接到一起，例如RFM得分为312、333、132。
                //方法二：将RFM三个维度的值进行加权后相加求得一个新的汇总值。
                //方法三：分别求每个用户的R,F,M与R平均值、F平均值、M平均值的差，将差值 > 1的数据赋值为‘1’，差值 <= 1赋值为‘0’。这个过程分别将RFM三个维度的数据分为两种结果（1或者0）。接着将RFM三个值组合拼接，得到8个数值：111、011、101、110、001、010、100、000；再将8个数值赋值为8类标签：'011':'重要保持客户'，'101':'重要发展客户'，'001':'重要挽留客户'，'110':'一般价值客户'，'010':'一般保持客户'，'100':'一般发展客户'，'000':'一般挽留客户'。

                //1.取 R，F，M，对应数据
                var rs = datas.Select(s => (double)s.NoPurchaseDays).OrderBy(s => s).ToList();
                var fs = datas.Select(s => (double)s.PurchaseNumber).OrderBy(s => s).ToList();
                var ms = datas.Select(s => (double)s.PurchaseAmount).OrderBy(s => s).ToList();

                //2.计算R，F，M，对应5分位分数
                var rQuantiles = rs.FiveNumberSummary();
                var fQuantiles = fs.FiveNumberSummary();
                var mQuantiles = ms.FiveNumberSummary();

                //3.确认R，F，M 区间，并打分
                foreach (var col in datas)
                {
                    //Rank范围 [0.0, 0.25, 0.50, 0.75, 1.0]
                    var r_rank = Statistics.QuantileRank(rQuantiles, col.NoPurchaseDays, col.NoPurchaseDays == 0 ? RankDefinition.Min : RankDefinition.Average);
                    var f_rank = Statistics.QuantileRank(fQuantiles, col.PurchaseNumber, col.PurchaseNumber == 0 ? RankDefinition.Min : RankDefinition.Average);
                    var m_rank = Statistics.QuantileRank(mQuantiles, (double)col.PurchaseAmount, col.PurchaseAmount == 0 ? RankDefinition.Min : RankDefinition.Average);

                    //打分范围 [1-5]
                    col.R_S = MatchRScore(r_rank);
                    col.F_S = MatchFMScore(f_rank);
                    col.M_S = MatchFMScore(m_rank);

                    //评分应用
                    col.RFMScore = 100 * (int)col.R_S + 10 * (int)col.F_S + (int)col.M_S;

                    var r = col.R_S;
                    var f = col.F_S;
                    var m = col.M_S;

                    if (r > 3 && f > 3 && m > 3)
                    {
                        col.TerminalTypeId = 8;
                        col.TerminalTypeName = "高价值客户";
                    }
                    else if (r <= 3 && f > 3 && m > 3)
                    {
                        col.TerminalTypeId = 7;
                        col.TerminalTypeName = "高重点保护客户";
                    }
                    else if (r > 3 && f <= 3 && m > 3)
                    {
                        col.TerminalTypeId = 6;
                        col.TerminalTypeName = "重点发展客户";
                    }
                    else if (r <= 3 && f <= 3 && m > 3)
                    {
                        col.TerminalTypeId = 5;
                        col.TerminalTypeName = "重点挽留客户";
                    }
                    else if (r > 3 && f > 3 && m <= 3)
                    {
                        col.TerminalTypeId = 4;
                        col.TerminalTypeName = "一般价值客户";
                    }
                    else if (r <= 3 && f > 3 && m <= 3)
                    {
                        col.TerminalTypeId = 3;
                        col.TerminalTypeName = "一般保持客户";
                    }
                    else if (r > 3 && f <= 3 && m <= 3)
                    {
                        col.TerminalTypeId = 2;
                        col.TerminalTypeName = "一般发展客户";
                    }
                    else if (r <= 3 && f <= 3 && m <= 3)
                    {
                        col.TerminalTypeId = 1;
                        col.TerminalTypeName = "潜在客户";
                    }
                }

                reporting = datas;
                return reporting;
            }
            catch (Exception)
            {
                return reporting;
            }
        }

        /// <summary>
        /// 铺市率报表
        /// </summary>
        /// <param name="storeId">经销商Id</param>
        /// <param name="productId">商品Id</param>
        /// <param name="categoryId">商品类别Id</param>
        /// <param name="brandId">品牌Id</param>
        /// <param name="districtId">客户片区Id</param>
        /// <param name="startTime">开始日期</param>
        /// <param name="endTime">结束日期</param>
        /// <param name="bussinessUserId">业务员Id</param>
        /// <returns></returns>
        public IList<MarketReportShopRate> GetMarketReportShopRate(int? storeId, int? productId, string productName, int? categoryId, int? brandId, int? districtId, DateTime? startTime, DateTime? endTime, int? bussinessUserId)
        {
            productName = CommonHelper.Filter(productName);

            var reporting = new List<MarketReportShopRate>();

            try
            {
                string whereQuery = $" p.StoreId = {storeId ?? 0} ";
                string whereQuery2 = $" a.StoreId = {storeId ?? 0} ";
                string whereQuery3 = $" a.StoreId = {storeId ?? 0} ";

                if (productId.HasValue && productId.Value != 0)
                {
                    whereQuery += $" and p.Id = '{productId}' ";
                }
                if (productName != null)
                {
                    whereQuery += $" and p.Name like '%{productName}%' ";
                }

                if (categoryId.HasValue && categoryId.Value != 0)
                {
                    //递归商品类别查询
                    var categoryIds = _categoryService.GetSubCategoryIds(storeId ?? 0, categoryId ?? 0);
                    if (categoryIds != null && categoryIds.Count > 0)
                    {
                        string incategoryIds = string.Join("','", categoryIds);
                        whereQuery += $" and p.CategoryId in ('{incategoryIds}') ";
                    }
                    else
                    {
                        whereQuery += $" and p.CategoryId = '{categoryId}' ";
                    }
                }

                if (brandId.HasValue && brandId.Value != 0)
                {
                    whereQuery += $" and p.BrandId = '{brandId}' ";
                }

                if (bussinessUserId.HasValue && bussinessUserId.Value != 0)
                {
                    whereQuery2 += $" and a.BusinessUserId = '{bussinessUserId}' ";
                    whereQuery3 += $" and a.BusinessUserId = '{bussinessUserId}' ";
                }

                if (districtId.HasValue && districtId.Value != 0)
                {
                    //递归片区查询
                    var distinctIds = _districtService.GetSubDistrictIds(storeId ?? 0, districtId ?? 0);
                    if (distinctIds != null && distinctIds.Count > 0)
                    {
                        string inDistinctIds = string.Join("','", distinctIds);
                        whereQuery2 += $" and a.DistrictId in ('{inDistinctIds}') ";
                        whereQuery3 += $" and a.DistrictId in ('{inDistinctIds}') ";
                    }
                    else
                    {
                        whereQuery2 += $" and a.DistrictId = '{districtId}' ";
                        whereQuery3 += $" and a.DistrictId = '{districtId}' ";
                    }
                }

                if (startTime.HasValue)
                {
                    //startTime = DateTime.Parse(((DateTime)startTime).ToString("yyyy-MM-dd 00:00:00"));
                    whereQuery2 += $" and a.CreatedOnUtc >= '{startTime?.ToString("yyyy-MM-dd 00:00:00")}'";
                }

                if (endTime.HasValue)
                {
                    //endTime = DateTime.Parse(((DateTime)endTime).ToString("yyyy-MM-dd 23:59:59"));
                    whereQuery2 += $" and a.CreatedOnUtc <= '{endTime?.ToString("yyyy-MM-dd 23:59:59")}'";
                }


                //MSSQL
                //string sqlString = $"select p.Id ProductId ,isnull(p.ProductCode,'') as ProductCode,p.Name as ProductName,isnull(p.SmallBarCode,'') as SmallBarCode,isnull(p.StrokeBarCode,'') as StrokeBarCode,isnull(p.BigBarCode,'') as BigBarCode,UnitConversion = CONCAT('1', CAST(b.Name AS nvarchar(max)) ,'=', p.BigQuantity,CAST(s.Name AS nvarchar(max))),isnull(sale.Amount, 0.00) SaleAmount,isnull(retn.Amount, 0.00) ReturnAmount,DoorQuantity = (select count(*) from dcms_crm.CRM_Terminals as t where t.StoreId = {storeId ?? 0}),BeginQuantity = (select count(*) from(select a.TerminalId from SaleBills as a  inner join SaleItems as b on a.Id = b.SaleBillId  inner join  Terminals as t on a.TerminalId = t.Id where {whereQuery3} and a.AuditedStatus = '1' and a.ReversedStatus = '0' and b.ProductId = p.Id and a.CreatedOnUtc < '{startTime?.ToString("yyyy-MM-dd 00:00:00")}'  group by a.TerminalId) as ts),InsideQuantity = (select count(*) from(select a.TerminalId from SaleBills as a  inner join SaleItems as b on a.Id = b.SaleBillId  inner join  Terminals as t on a.TerminalId = t.Id where {whereQuery3} and a.AuditedStatus = '1' and a.ReversedStatus = '0' and b.ProductId = p.Id and ( a.CreatedOnUtc <= '{startTime?.ToString("yyyy-MM-dd 00:00:00")}' and a.CreatedOnUtc <= '{endTime?.ToString("yyyy-MM-dd 23:59:59")}')  group by a.TerminalId) as ts),EndQuantity = (select count(*) from(select a.TerminalId from SaleBills as a  inner join SaleItems as b on a.Id = b.SaleBillId  inner join  Terminals as t on a.TerminalId = t.Id where {whereQuery3} and  a.AuditedStatus = '1' and a.ReversedStatus = '0' and b.ProductId = p.Id and  a.CreatedOnUtc <= '{endTime?.ToString("yyyy-MM-dd 23:59:59")}'  group by a.TerminalId) as ts),0 DecreaseQuantity,0 AddQuantity from  Products as p left join SpecificationAttributeOptions as s on p.SmallUnitId = s.id left join SpecificationAttributeOptions as m on p.StrokeUnitId = m.id left join SpecificationAttributeOptions as b on p.BigUnitId = b.id left join(select b.ProductId, sum(b.Amount) Amount from SaleBills as a  inner join SaleItems as b on a.Id = b.SaleBillId  inner join  Terminals as t on a.TerminalId = t.Id where {whereQuery2} and a.AuditedStatus = '1' and a.ReversedStatus = '0'  group by b.ProductId) as sale  on p.Id = sale.ProductId left join(select b.ProductId, sum(b.Amount) Amount from ReturnBills as a  inner join ReturnItems as b on a.Id = b.ReturnBillId  inner join  Terminals as t on a.TerminalId = t.Id where {whereQuery2} and a.AuditedStatus = '1' and a.ReversedStatus = '0'  group by b.ProductId) as retn  on p.Id = retn.ProductId where {whereQuery}";

                //MYSQL
                string sqlString = $" SELECT p.Id AS ProductId, IFNULL(p.ProductCode, '') AS ProductCode, p.Name AS ProductName , 0.0 as Percent, " +
                                   $"   IFNULL(p.SmallBarCode, '') AS SmallBarCode" +
                                   $"  , IFNULL(p.StrokeBarCode, '') AS StrokeBarCode" +
                                   $"  , IFNULL(p.BigBarCode, '') AS BigBarCode" +
                                   $"  , CONCAT('1', CAST(b.Name AS char), '=', p.BigQuantity, CAST(s.Name AS char)) AS UnitConversion" +
                                   $"  , IFNULL(sale.Amount, 0.00) AS SaleAmount" +
                                   $"  , IFNULL(retn.Amount, 0.00) AS ReturnAmount" +
                                   $"  , (" +
                                   $"   SELECT COUNT(*)" +
                                   $"   FROM dcms_crm.CRM_Terminals t" +
                                   $"   WHERE t.StoreId = {storeId ?? 0}" +
                                   $"  ) AS DoorQuantity" +
                                   $"  , (" +
                                   $"   SELECT COUNT(*)" +
                                   $"   FROM (" +
                                   $"    SELECT a.TerminalId" +
                                   $"    FROM SaleBills a" +
                                   $"     INNER JOIN SaleItems b ON a.Id = b.SaleBillId" +
                                   $"     INNER JOIN dcms_crm.CRM_Terminals t ON a.TerminalId = t.Id" +
                                   $"    WHERE {whereQuery3}" +
                                   $"     AND a.AuditedStatus = '1'" +
                                   $"     AND a.ReversedStatus = '0'" +
                                   $"     AND b.ProductId = p.Id" +
                                   $"     AND a.CreatedOnUtc < '{startTime?.ToString("yyyy-MM-dd 00:00:00")}'" +
                                   $"    GROUP BY a.TerminalId" +
                                   $"   ) ts" +
                                   $"  ) AS BeginQuantity" +
                                   $"  , (" +
                                   $"   SELECT COUNT(*)" +
                                   $"   FROM (" +
                                   $"    SELECT a.TerminalId" +
                                   $"    FROM SaleBills a" +
                                   $"     INNER JOIN SaleItems b ON a.Id = b.SaleBillId" +
                                   $"     INNER JOIN dcms_crm.CRM_Terminals t ON a.TerminalId = t.Id" +
                                   $"    WHERE {whereQuery3}" +
                                   $"     AND a.AuditedStatus = '1'" +
                                   $"     AND a.ReversedStatus = '0'" +
                                   $"     AND b.ProductId = p.Id" +
                                   $"     AND (a.CreatedOnUtc <= '{startTime?.ToString("yyyy-MM-dd 00:00:00")}'" +
                                   $"      AND a.CreatedOnUtc <= '{endTime?.ToString("yyyy-MM-dd 00:00:00")}')" +
                                   $"    GROUP BY a.TerminalId" +
                                   $"   ) ts" +
                                   $"  ) AS InsideQuantity" +
                                   $"  , (" +
                                   $"   SELECT COUNT(*)" +
                                   $"   FROM (" +
                                   $"    SELECT a.TerminalId" +
                                   $"    FROM SaleBills a" +
                                   $"     INNER JOIN SaleItems b ON a.Id = b.SaleBillId" +
                                   $"     INNER JOIN dcms_crm.CRM_Terminals t ON a.TerminalId = t.Id" +
                                   $"    WHERE {whereQuery3}" +
                                   $"     AND a.AuditedStatus = '1'" +
                                   $"     AND a.ReversedStatus = '0'" +
                                   $"     AND b.ProductId = p.Id" +
                                   $"     AND a.CreatedOnUtc <= '{endTime?.ToString("yyyy-MM-dd 00:00:00")}'" +
                                   $"    GROUP BY a.TerminalId" +
                                   $"   ) ts" +
                                   $"  ) AS EndQuantity, 0 AS DecreaseQuantity, 0 AS AddQuantity" +
                                   $" FROM Products p" +
                                   $"  LEFT JOIN SpecificationAttributeOptions s ON p.SmallUnitId = s.id" +
                                   $"  LEFT JOIN SpecificationAttributeOptions m ON p.StrokeUnitId = m.id" +
                                   $"  LEFT JOIN SpecificationAttributeOptions b ON p.BigUnitId = b.id" +
                                   $"  LEFT JOIN (" +
                                   $"   SELECT b.ProductId, SUM(b.Amount) AS Amount" +
                                   $"   FROM SaleBills a" +
                                   $"    INNER JOIN SaleItems b ON a.Id = b.SaleBillId" +
                                   $"    INNER JOIN dcms_crm.CRM_Terminals t ON a.TerminalId = t.Id" +
                                   $"   WHERE {whereQuery2}" +
                                   $"    AND a.AuditedStatus = '1'" +
                                   $"    AND a.ReversedStatus = '0'" +
                                   $"   GROUP BY b.ProductId" +
                                   $"  ) sale" +
                                   $"  ON p.Id = sale.ProductId" +
                                   $"  LEFT JOIN (" +
                                   $"   SELECT b.ProductId, SUM(b.Amount) AS Amount" +
                                   $"   FROM ReturnBills a" +
                                   $"    INNER JOIN ReturnItems b ON a.Id = b.ReturnBillId" +
                                   $"    INNER JOIN dcms_crm.CRM_Terminals t ON a.TerminalId = t.Id" +
                                   $"   WHERE {whereQuery2}" +
                                   $"    AND a.AuditedStatus = '1'" +
                                   $"    AND a.ReversedStatus = '0'" +
                                   $"   GROUP BY b.ProductId" +
                                   $"  ) retn" +
                                   $"  ON p.Id = retn.ProductId" +
                                   $" WHERE {whereQuery}";

                var datas = SaleBillsRepository_RO.QueryFromSql<MarketReportShopRate>(sqlString).ToList();

                datas.ForEach(r =>
                {
                    r.DecreaseQuantity = r.EndQuantity - r.InsideQuantity;
                    r.AddQuantity = r.EndQuantity - r.BeginQuantity;
                    if (r.InsideQuantity.HasValue && r.InsideQuantity.Value != 0)
                    {
                        r.Percent = Convert.ToDouble(Convert.ToDecimal(r.InsideQuantity.Value) / Convert.ToDecimal(r.DoorQuantity ?? 0)) * 100;
                    }
                    else
                    {
                        r.Percent = 0.00;
                    }
                });

                reporting = datas;

                return reporting;
            }
            catch (Exception)
            {
                return reporting;
            }

        }


    }
}
