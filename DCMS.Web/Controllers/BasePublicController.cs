using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Domain.Configuration;
using DCMS.Core.Domain.Plan;
using DCMS.Core.Domain.Products;
using DCMS.Core.Domain.Stores;
using DCMS.Core.Domain.Terminals;
using DCMS.Core.Domain.Users;
using DCMS.Core.Domain.Visit;
using DCMS.Core.Domain.WareHouses;
using DCMS.Core.Infrastructure;
using DCMS.Services.Finances;
using DCMS.Services.Logging;
using DCMS.Services.Messages;
using DCMS.Services.Sales;
using DCMS.Services.Security;
using DCMS.Services.WareHouses;
using DCMS.ViewModel.Models;
using DCMS.ViewModel.Models.Stores;
using DCMS.Web.Framework.Controllers;
using DCMS.Web.Framework.Extensions;
using DCMS.Web.Framework.Models;
using DCMS.Web.Framework.Mvc.Filters;
using DCMS.Web.Framework.Security;
using DCMS.Web.Infrastructure.Mapper.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DCMS.Web.Controllers
{
    [HttpsRequirement(SslRequirement.NoMatter)]
    [WwwRequirement]
    [CheckAccessPublicStore]
    public abstract partial class BasePublicController : BaseController
    {
        protected readonly IWorkContext _workContext;
        protected readonly ILogger _loggerService;
        protected readonly IStoreContext _storeContext;
        protected readonly INotificationService _notificationService;

        public BasePublicController(
            IWorkContext workContext,
            ILogger loggerService,
            IStoreContext storeContext,
            INotificationService notificationService
            )
        {
            _workContext = workContext;
            _loggerService = loggerService;
            _storeContext = storeContext;
            _notificationService = notificationService;
        }

        protected virtual IActionResult InvokeHttp404()
        {
            Response.StatusCode = 404;
            return new EmptyResult();
        }

        protected virtual IActionResult InvokeHttp401()
        {
            Response.StatusCode = 401;
            return new EmptyResult();
        }


        public Store curStore => _storeContext.CurrentStore;
        public User curUser => _workContext.CurrentUser;

        public List<StoreModel> Stores => _storeContext.Stores.Where(s => (!string.IsNullOrEmpty(s.Code) ? s.Code.Trim() : "") == "SYSTEM").Select(s =>
         {
             return s.ToModel<StoreModel>();
         }).ToList();


        /// <summary>
        /// 日志异常
        /// </summary>
        /// <param name="exc"></param>
        private void LogException(Exception exc)
        {
            _loggerService.Error(exc.Message, exc, curUser);
        }

        /// <summary>
        /// 用于组织机构递归下拉树
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <param name="callback"></param>
        /// <param name="store"></param>
        /// <param name="parentId"></param>
        /// <returns></returns>
        [NonAction]
        protected T BindDropDownList<T>(T model, Func<int?, int, List<Branch>> callback, int? store, int parentId = 0) where T : IParentList
        {
            List<SelectListItem> parentList = new List<SelectListItem>();
            var nodes = callback(store ?? 0, parentId);
            if (nodes.ToList().Count > 0)
            {
                foreach (var b in nodes)
                {
                    string pname = b.DeptName;
                    string pid = b.Id.ToString();
                    parentList.Add(new SelectListItem
                    {
                        Text = " ┝  " + pname,
                        Value = pid
                    });
                    int sonpid = int.Parse(pid);
                    string blank = "    ┝━━ ";
                    BindNode(callback, sonpid, store, blank, parentList);
                }
            }
            else
            {
                if (parentId == 0)
                {
                    parentList.Add(new SelectListItem
                    {
                        Text = " ┝  根目录",
                        Value = "0"
                    });
                }
            }



            model.ParentList = new SelectList(parentList, "Value", "Text");
            return model;
        }
        protected void BindNode(Func<int?, int, List<Branch>> callback, int sonpid, int? store, string blank, List<SelectListItem> selectList)
        {
            var nodes = callback(store ?? 0, sonpid);
            foreach (var c in nodes)
            {
                string sname = blank + c.DeptName;
                string sid = c.Id.ToString();
                selectList.Add(new SelectListItem
                {
                    Text = sname,
                    Value = sid
                });
                int sonpids = int.Parse(sid);
                string blank2 = blank + "━";
                BindNode(callback, sonpids, store, blank2, selectList);
            }
        }



        /// <summary>
        /// 用于商品类别构递归下拉树
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <param name="callback"></param>
        /// <param name="store"></param>
        /// <param name="parentId"></param>
        /// <returns></returns>
        [NonAction]
        protected T BindCategoryDropDownList<T>(T model, Func<int?, int, bool, IList<Category>> callback, int? store, int parentId = 0) where T : IParentList
        {
            List<SelectListItem> parentList = new List<SelectListItem> { new SelectListItem() { Text = "-请选择-", Value = "0" } };
            var nodes = callback(store.Value, parentId, false);
            foreach (var b in nodes)
            {
                string pname = b.Name;
                string pid = b.Id.ToString();
                parentList.Add(new SelectListItem
                {
                    Text = " ┝  " + pname,
                    Value = pid
                });
                int sonpid = int.Parse(pid);
                string blank = "    ┝━";
                BindCategoryNode(callback, sonpid, store.Value, blank, parentList);
            }
            model.ParentList = new SelectList(parentList, "Value", "Text");
            return model;
        }
        [NonAction]
        protected void BindCategoryNode(Func<int?, int, bool, IList<Category>> callback, int sonpid, int? store, string blank, List<SelectListItem> selectList)
        {
            var nodes = callback(store.Value, sonpid, false);
            foreach (var c in nodes)
            {
                string sname = blank + c.Name;
                string sid = c.Id.ToString();
                selectList.Add(new SelectListItem
                {
                    Text = sname,
                    Value = sid
                });
                int sonpids = int.Parse(sid);
                string blank2 = blank + "━";
                BindCategoryNode(callback, sonpids, store.Value, blank2, selectList);
            }
        }


        /// <summary>
        /// 用户下拉绑定
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="storeId"></param>
        /// <param name="userRoleIds"></param>
        /// <returns></returns>
        [NonAction]
        protected SelectList BindUserSelection(Func<int, string,int, bool,bool, List<Core.Domain.Users.User>> callback, Store curStore, string userRoleIds, int defaultValue = 0,bool selectSubordinate = false,bool isAdmin=false)
        {
            var items = callback(curStore?.Id ?? 0, userRoleIds, defaultValue, selectSubordinate,isAdmin).Select(u =>
              {
                  return new SelectListItem() { Text = u?.UserRealName, Value = u?.Id.ToString(), Selected = (u?.Id.Equals(defaultValue) ?? false) };
              }).ToList();
            //items.Add(new SelectListItem { Text = "请选择", Value = "");
            return new SelectList(items, "Value", "Text");
        }
        /// <summary>
        /// 用户下拉绑定
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="storeId"></param>
        /// <param name="userRoleIds"></param>
        /// <returns></returns>
        [NonAction]
        protected SelectList BindUserSelection(List<Core.Domain.Users.User> list, int defaultValue = 0)
        {
            var items = list.Select(u =>
             {
                 return new SelectListItem() { Text = u.UserRealName, Value = u.Id.ToString(), Selected = (u.Id.Equals(defaultValue)) };
             }).ToList();
            //items.Add(new SelectListItem { Text = "请选择仓库", Value = "");
            return new SelectList(items, "Value", "Text");
        }



        /// <summary>
        /// 仓库下拉绑定（不分仓库类型）
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="curStore"></param>
        /// <returns></returns>
        [NonAction]
        protected SelectList BindWareHouseSelection(Func<int?, WHAEnum?, int, IList<WareHouse>> callback, Store curStore, WHAEnum? wHA, int userId = 0, int defaultValue = 0)
        {
            var items = callback(curStore?.Id ?? 0, wHA, userId).Select(u =>
               {
                   return new SelectListItem()
                   {
                       Text = u.Name,
                       Value = u.Id.ToString(),
                       Selected = (u.Id.Equals(defaultValue))
                   };
               }).ToList();

            return new SelectList(items, "Value", "Text");
        }

        /// <summary>
        /// 仓库下拉绑定（分仓库类型，仓库、车辆）
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="curStore"></param>
        /// <param name="type"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        [NonAction]
        protected SelectList BindWareHouseByTypeSelection(Func<int?, int?, WHAEnum?, int, IList<WareHouse>> callback, Store curStore, int type, WHAEnum? wHA, int userId = 0, int defaultValue = 0)
        {
            var items = callback(curStore?.Id ?? 0, type, wHA, userId).Select(u =>
             {
                 return new SelectListItem()
                 {
                     Text = u.Name,
                     Value = u.Id.ToString(),
                     Selected = (u.Id.Equals(defaultValue))
                 };
             }).ToList();

            //items.Add(new SelectListItem { Text = "请选择仓库", Value = "");
            return new SelectList(items, "Value", "Text");
        }


        /// <summary>
        /// 供应商下拉绑定
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="curStore"></param>
        /// <returns></returns>
        [NonAction]
        protected SelectList BindManufacturerSelection(Func<int?, IList<Manufacturer>> callback, Store curStore, int defaultValue = 0)
        {
            var items = callback(curStore?.Id ?? 0).Select(u =>
               {
                   return new SelectListItem() { Text = u.Name, Value = u.Id.ToString(), Selected = (u.Id.Equals(defaultValue)) };
               }).ToList();
            //items.Add(new SelectListItem { Text = "请选择供应商", Value = "");
            return new SelectList(items, "Value", "Text");
        }
        [NonAction]
        protected SelectList BindManufacturerSelection(IList<Manufacturer> manufacturers, int defaultValue = 0)
        {
            var items = manufacturers.Select(u =>
            {
                return new SelectListItem() { Text = u.Name, Value = u.Id.ToString(), Selected = (u.Id.Equals(defaultValue)) };
            }).ToList();
            //items.Add(new SelectListItem { Text = "请选择供应商", Value = "");
            return new SelectList(items, "Value", "Text");
        }


        /// <summary>
        /// 品牌下拉绑定
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="curStore"></param>
        /// <returns></returns>
        [NonAction]
        protected SelectList BindBrandSelection(Func<int?, IList<Brand>> callback, Store curStore, int defaultValue = 0)
        {
            var items = callback(curStore?.Id ?? 0).Select(u =>
               {
                   return new SelectListItem() { Text = u.Name, Value = u.Id.ToString(), Selected = (u.Id.Equals(defaultValue)) };
               }).ToList();
            //items.Add(new SelectListItem { Text = "请选择品牌", Value = "");
            return new SelectList(items, "Value", "Text");
        }
        [NonAction]
        protected SelectList BindBrandSelection(IList<Brand> brands, int defaultValue = 0)
        {
            var items = brands.Select(u =>
            {
                return new SelectListItem() { Text = u.Name, Value = u.Id.ToString(), Selected = (u.Id.Equals(defaultValue)) };
            }).ToList();
            //items.Add(new SelectListItem { Text = "请选择品牌", Value = "");
            return new SelectList(items, "Value", "Text");
        }

        /// <summary>
        /// 商品类别下拉绑定
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="curStore"></param>
        /// <returns></returns>
        [NonAction]
        protected SelectList BindCategorySelection(Func<int?, IList<Category>> callback, Store curStore, int defaultValue = 0)
        {
            var items = callback(curStore?.Id ?? 0).Select(u =>
              {
                  return new SelectListItem() { Text = u.Name, Value = u.Id.ToString(), Selected = (u.Id.Equals(defaultValue)) };
              }).ToList();
            //items.Add(new SelectListItem { Text = "请选择类别", Value = "");
            return new SelectList(items, "Value", "Text");
        }
        [NonAction]
        protected SelectList BindCategorySelection(IList<Category> categories, int defaultValue = 0)
        {
            var items = categories.Select(u =>
            {
                return new SelectListItem() { Text = u.Name, Value = u.Id.ToString(), Selected = (u.Id.Equals(defaultValue)) };
            }).ToList();
            //items.Add(new SelectListItem { Text = "请选择类别", Value = "");
            return new SelectList(items, "Value", "Text");
        }

        /// <summary>
        /// 片区下拉绑定
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="curStore"></param>
        /// <returns></returns>
        [NonAction]
        protected SelectList BindDistrictSelection(Func<int?, IList<District>> callback, Store curStore, int defaultValue = 0)
        {
            var items = callback(curStore?.Id ?? 0).Select(u =>
               {
                   return new SelectListItem() { Text = u.Name, Value = u.Id.ToString(), Selected = (u.Id.Equals(defaultValue)) };
               }).ToList();
            //items.Add(new SelectListItem { Text = "请选择片区", Value = "");
            return new SelectList(items, "Value", "Text");
        }
        [NonAction]
        protected SelectList BindDistrictSelection(IList<District> districts, Store curStore, int defaultValue = 0)
        {
            var items = districts.Select(u =>
            {
                return new SelectListItem() { Text = u.Name, Value = u.Id.ToString(), Selected = (u.Id.Equals(defaultValue)) };
            }).ToList();
            //items.Add(new SelectListItem { Text = "请选择片区", Value = "");
            return new SelectList(items, "Value", "Text");
        }



        /// <summary>
        /// 客户渠道
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="curStore"></param>
        /// <returns></returns>
        [NonAction]
        protected SelectList BindChanneSelection(Func<int?, IList<Channel>> callback, Store curStore, int defaultValue = 0)
        {
            var items = callback(curStore?.Id ?? 0).Select(u =>
               {
                   return new SelectListItem() { Text = u.Name, Value = u.Id.ToString(), Selected = (u.Id.Equals(defaultValue)) };
               }).ToList();
            //items.Add(new SelectListItem { Text = "请选择渠道", Value = "");
            return new SelectList(items, "Value", "Text");
        }
        [NonAction]
        protected SelectList BindChanneSelection(IList<Channel> channels, Store curStore, int defaultValue = 0)
        {
            var items = channels.Select(u =>
            {
                return new SelectListItem() { Text = u.Name, Value = u.Id.ToString(), Selected = (u.Id.Equals(defaultValue)) };
            }).ToList();
            //items.Add(new SelectListItem { Text = "请选择渠道", Value = "");
            return new SelectList(items, "Value", "Text");
        }


        /// <summary>
        /// 客户等级
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="curStore"></param>
        /// <returns></returns>
        [NonAction]
        protected SelectList BindRankSelection(Func<int?, IList<Rank>> callback, Store curStore, int defaultValue = 0)
        {
            var items = callback(curStore?.Id ?? 0).Select(u =>
               {
                   return new SelectListItem() { Text = u.Name, Value = u.Id.ToString(), Selected = (u.Id.Equals(defaultValue)) };
               }).ToList();
            //items.Add(new SelectListItem { Text = "请选择等级", Value = "");
            return new SelectList(items, "Value", "Text");
        }
        [NonAction]
        protected SelectList BindRankSelection(IList<Rank> ranks, Store curStore, int defaultValue = 0)
        {
            var items = ranks.Select(u =>
            {
                return new SelectListItem() { Text = u.Name, Value = u.Id.ToString(), Selected = (u.Id.Equals(defaultValue)) };
            }).ToList();
            //items.Add(new SelectListItem { Text = "请选择等级", Value = "");
            return new SelectList(items, "Value", "Text");
        }

        /// <summary>
        /// 线路
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="curStore"></param>
        /// <returns></returns>
        [NonAction]
        protected SelectList BindLineSelection(Func<int?, IList<LineTier>> callback, Store curStore, int defaultValue = 0)
        {
            var items = callback(curStore?.Id ?? 0).Select(u =>
               {
                   return new SelectListItem() { Text = u.Name, Value = u.Id.ToString(), Selected = (u.Id.Equals(defaultValue)) };
               }).ToList();
            //items.Add(new SelectListItem { Text = "请选择线路", Value = "");
            return new SelectList(items, "Value", "Text");
        }
        [NonAction]
        protected SelectList BindLineSelection(IList<LineTier> lineTiers, int defaultValue = 0)
        {
            var items = lineTiers.Select(u =>
            {
                return new SelectListItem() { Text = u.Name, Value = u.Id.ToString(), Selected = (u.Id.Equals(defaultValue)) };
            }).ToList();
            //items.Add(new SelectListItem { Text = "请选择线路", Value = "");
            return new SelectList(items, "Value", "Text");
        }



        /// <summary>
        /// 提成方案下拉绑定
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="curStore"></param>
        /// <returns></returns>
        [NonAction]
        protected SelectList BindPercentagePlanSelection(Func<int?, List<PercentagePlan>> callback, Store curStore, int defaultValue = 0)
        {
            var items = callback(curStore?.Id ?? 0).Select(u =>
               {
                   return new SelectListItem() { Text = u.Name, Value = u.Id.ToString(), Selected = (u.Id.Equals(defaultValue)) };
               }).ToList();
            //items.Add(new SelectListItem { Text = "请选择方案", Value = "");
            return new SelectList(items, "Value", "Text");
        }
        [NonAction]
        protected SelectList BindPercentagePlanSelection(List<PercentagePlan> percentagePlans, int defaultValue = 0)
        {
            var items = percentagePlans.Select(u =>
            {
                return new SelectListItem() { Text = u.Name, Value = u.Id.ToString(), Selected = (u.Id.Equals(defaultValue)) };
            }).ToList();
            //items.Add(new SelectListItem { Text = "请选择方案", Value = "");
            return new SelectList(items, "Value", "Text");
        }

        /// <summary>
        /// 配置项
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="curStore"></param>
        /// <returns></returns>
        [NonAction]
        protected SelectList BindOptionSelection(Dictionary<string, int> options, int defaultValue = 0)
        {
            var items = options.Select(u =>
             {
                 return new SelectListItem() { Text = u.Key, Value = u.Value.ToString(), Selected = (u.Value.Equals(defaultValue)) };
             }).ToList();
            //items.Add(new SelectListItem { Text = "请选择", Value = "");
            return new SelectList(items, "Value", "Text");
        }



        /// <summary>
        /// 报表单据类型下拉绑定 BillTypeReportEnum / BillTypeEnum
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="curStore"></param>
        /// <returns></returns>
        [NonAction]
        protected SelectList BindBillTypeSelection<T>(int defaultValue = 0, T[] ids = null) where T : Enum
        {
            var types = Enum.GetValues(typeof(T)).Cast<T>();
            if (ids != null)
            {
                types = types.Where(s => ids.Contains(s));
            }
            var items = types.Select(u =>
             {
                 if (u is BillTypeEnum u1)
                 {
                     return new SelectListItem() { Text = CommonHelper.GetEnumDescription(u), Value = ((int)u1).ToString(), Selected = (((int)u1).Equals(defaultValue)) };
                 }
                 else if (u is BillTypeReportEnum u2)
                 {
                     return new SelectListItem() { Text = CommonHelper.GetEnumDescription(u), Value = ((int)u2).ToString(), Selected = (((int)u2).Equals(defaultValue)) };
                 }
                 else
                 {
                     return new SelectListItem() { Text = "全部", Value = "", Selected = (u.Equals(defaultValue)) };
                 }
             }).ToList();
            return new SelectList(items, "Value", "Text");
        }

        /// <summary>
        /// 会计科目
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="curStore"></param>
        /// <returns></returns>
        [NonAction]
        protected SelectList BindAccountSelection(Func<int?, int, IList<AccountingOption>> callback, Store curStore, int typeid, int defaultValue = 0)
        {
            var items = callback(curStore?.Id ?? 0, typeid).Select(u =>
               {
                   return new SelectListItem() { Text = u.Name, Value = u.Number.ToString(), Selected = (u.Id.Equals(defaultValue)) };
               }).ToList();
            // items.Add(new SelectListItem { Text = "选择科目", Value = "");
            return new SelectList(items, "Value", "Text");
        }
        [NonAction]
        protected SelectList BindAccountSelection(IList<AccountingOption> accountingOptions, int typeid, int defaultValue = 0)
        {
            var items = accountingOptions.Select(u =>
            {
                return new SelectListItem() { Text = u.Name, Value = u.Number.ToString(), Selected = (u.Id.Equals(defaultValue)) };
            }).ToList();
            // items.Add(new SelectListItem { Text = "选择科目", Value = "");
            return new SelectList(items, "Value", "Text");
        }

        /// <summary>
        /// 默认售价类型
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="curStore"></param>
        /// <returns></returns>
        [NonAction]
        protected SelectList BindPricePlanSelection(Func<int?, IList<ProductPricePlan>> callback, Store curStore)
        {
            return new SelectList(callback(curStore?.Id ?? 0).Select(a =>
               {
                   return new SelectListItem() { Text = a.Name, Value = a.PricesPlanId.ToString() + "_" + a.PriceTypeId.ToString() };

               }).ToList(), "Value", "Text");
        }
        [NonAction]
        protected SelectList BindPricePlanSelection(IList<ProductPricePlan> productPricePlans)
        {
            return new SelectList(productPricePlans.Select(a =>
            {
                return new SelectListItem() { Text = a.Name, Value = a.PricesPlanId.ToString() + "_" + a.PriceTypeId.ToString() };

            }).ToList(), "Value", "Text");
        }

        /// <summary>
        /// 备注下拉绑定
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="curStore"></param>
        /// <returns></returns>
        [NonAction]
        protected SelectList BindRemarkConfigSelection(Func<int?, IList<RemarkConfig>> callback, Store curStore, int defaultValue = 0)
        {
            var items = callback(curStore?.Id ?? 0).Select(u =>
              {
                  return new SelectListItem()
                  {
                      Text = u.Name,
                      Value = u.Id.ToString(),
                      Selected = (u.Id.Equals(defaultValue))
                  };
              }).ToList();

            //items.Add(new SelectListItem { Text = "请选择备注", Value = "");
            return new SelectList(items, "Value", "Text");
        }

        /// <summary>
        /// 备注下拉绑定
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="curStore"></param>
        /// <returns></returns>
        [NonAction]
        protected Dictionary<int, string> BindRemarkConfigDic(Func<int?, IList<RemarkConfig>> callback, Store curStore, int defaultValue = 0)
        {
            var items = callback(curStore?.Id ?? 0);
            Dictionary<int, string> dic = new Dictionary<int, string>();
            if (items != null && items.Count() > 0)
            {
                items.ToList().ForEach(it =>
                {
                    dic.Add(it.Id, it.Name);
                });
            }

            return dic;
        }


        /// <summary>
        /// 会计期间是否冻结
        /// </summary>
        /// <param name="period"></param>
        /// <returns></returns>
        [NonAction]
        protected bool PeriodClosed(DateTime period)
        {
            var _closingAccountsService = EngineContext.Current.Resolve<IClosingAccountsService>();
            return _closingAccountsService.IsClosed(curStore.Id, DateTime.Now);
        }

        /// <summary>
        /// 流程下个节点是否已红冲
        /// </summary>
        /// <param name="period"></param>
        /// <returns></returns>
        [NonAction]
        protected bool CheckNextNodeReversed(Store curStore, int? curBillId, BillTypeEnum curBilleType)
        {
            if (BillTypeEnum.SaleReservationBill == curBilleType)
            {
                var _billService = EngineContext.Current.Resolve<IDispatchBillService>();
                return _billService.CheckReversed(curBillId);
            }
            else if (BillTypeEnum.AllocationBill == curBilleType)
            {
                var _billService = EngineContext.Current.Resolve<ISaleBillService>();
                return _billService.CheckReversed(curBillId);
            }
            return false;
        }



        /// <summary>
        /// 会计期间是否锁定
        /// </summary>
        /// <param name="period"></param>
        /// <returns></returns>
        [NonAction]
        protected bool PeriodLocked(DateTime period)
        {
            var _closingAccountsService = EngineContext.Current.Resolve<IClosingAccountsService>();
            return _closingAccountsService.IsLocked(curStore.Id, DateTime.Now);
        }


        /// <summary>
        /// Warning
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        [NonAction]
        protected JsonResult Warning(string message)
        {
            return Warning(message, null);
        }
        [NonAction]
        protected JsonResult Warning(string message, object data)
        {
            return Json(new { Success = false, Icon = "fa-exclamation-circle", Message = message, Data = data });
        }


        /// <summary>
        /// Successful
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        [NonAction]
        protected JsonResult Successful(string message)
        {
            return Successful(message, null);
        }
        [NonAction]
        protected JsonResult Successful(string message, object data)
        {
            return Successful(true, message, data);
        }
        [NonAction]
        protected JsonResult Successful(bool status, string message, object data = null)
        {
            return Json(new { Success = status, Icon = "fa-check-circle", Message = message, Data = data });
        }

        /// <summary>
        /// Error
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        [NonAction]
        protected JsonResult Error(string message)
        {
            return Error(message, null);
        }
        [NonAction]
        protected JsonResult Error(string message, object data)
        {
            return Json(new { Success = false, Icon = "fa-times-circle", Message = message, Data = data });
        }

        [NonAction]
        protected override JsonResult BillChecking<T, Items>(T bill, BillStates operation,string authCode = "")
        {
            if (bill == null)
            {
                return Warning("单据信息不存在.");
            }

            if (bill.StoreId != curStore.Id)
            {
                return Warning("非法操作.");
            }

            switch (operation)
            {
                case BillStates.Draft:
                    {
                        if (bill.AuditedStatus || bill.ReversedStatus)
                        {
                            return Warning("非法操作.");
                        }
                    }
                    break;
                case BillStates.Audited: //审核
                    {
                        var _permissionService = EngineContext.Current.Resolve<IPermissionService>();
                        var authorizeCodes = _permissionService.GetUserAuthorizeCodesByUserId(curUser.StoreId, curUser != null ? curUser.Id : 0, false);
                        var auth = string.IsNullOrWhiteSpace(authCode) ? false: authorizeCodes.Where(a => a == authCode).ToList().Count > 0;
                        if (!(curUser.IsAdmin() || auth))
                        {
                            return Warning("权限不足.");
                        }

                        if (bill.AuditedStatus || bill.ReversedStatus)
                        {
                            return Warning("重复操作.");
                        }

                        if (bill.Items == null || !bill.Items.Any())
                        {
                            return Warning("单据没有明细.");
                        }

                    }
                    break;
                case BillStates.Reversed: //红冲
                    {
                        if (!bill.AuditedStatus || bill.ReversedStatus)
                        {
                            return Warning("非法操作.");
                        }

                        if (bill.Items == null || !bill.Items.Any())
                        {
                            return Warning("单据没有明细.");
                        }

                        if (DateTime.Now.Subtract(bill.AuditedDate ?? DateTime.Now).TotalSeconds > 86400)
                        {
                            return Warning("已经审核的单据超过24小时，不允许红冲.");
                        }
                    }
                    break;
            }

            if (bill.Deleted)
            {
                return Warning("单据已作废.");
            }

            //成功返回null
            return new JsonResult(null);
        }


        [NonAction]
        protected SelectList BindDatesSelection(Func<int?, IList<Core.Domain.Finances.ClosingAccounts>> callback, int? curStoreId, DateTime? defaultValue = null)
        {
            var items = callback(curStoreId ?? 0).Where(s => s.LockStatus == true && s.CheckStatus == true).OrderByDescending(s => s.ClosingAccountDate).Select(u =>
                  {
                      return new SelectListItem()
                      {
                          Text = $"{u.ClosingAccountDate.ToString("yyyy 年第 MM 期")}",
                          Value = u.ClosingAccountDate.ToString("yyyy-MM-dd"),
                          Selected = (u.ClosingAccountDate.Equals(defaultValue))
                      };
                  }).ToList();

            if (items == null || !items.Any())
            {
                items.Add(new SelectListItem { Text = "无结账期间", Value = DateTime.Now.ToString("yyyy-MM-dd") });
            }

            return new SelectList(items, "Value", "Text");
        }


        [NonAction]
        protected string LockKey<T>(T data) where T : BaseEntityModel
        {
            return string.Format(DCMSCachingDefaults.RedisDataReSubmitKey, Request.GetUrl(), curStore.Id, curUser.Id, CommonHelper.MD5(JsonConvert.SerializeObject(data)));
        }

        [NonAction]
        protected string RedLockKey<T>(T data) where T : BaseEntity
        {
            return string.Format(DCMSCachingDefaults.AuditingSubmitKey, Request.GetUrl(), curStore.Id, curUser.Id, CommonHelper.MD5(JsonConvert.SerializeObject(data)));
        }

    }
}