using DCMS.Api.Infrastructure.Mapper.Extensions;
using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Domain.Configuration;
using DCMS.Core.Domain.Products;
using DCMS.Core.Domain.Terminals;
using DCMS.Core.Domain.Users;
using DCMS.Core.Domain.WareHouses;
using DCMS.Core.Infrastructure;
using DCMS.Services.Finances;
using DCMS.Services.Stores;
using DCMS.ViewModel.Models.Stores;
using DCMS.Web.Framework.Extensions;
using DCMS.Web.Framework.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DCMS.Api.Controllers
{


    /// <summary>
    /// API 基类
    /// 注意模型绑定：
    /// </summary>
    [ApiController]
    public abstract class BaseAPIController : ControllerBase
    {
        protected readonly ILogger<BaseAPIController> _logger;

        public BaseAPIController(ILogger<BaseAPIController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 获取经销商列表
        /// </summary>
        public List<StoreModel> Stores
        {
            get
            {
                var _storeService = EngineContext.Current.Resolve<IStoreService>();
                var stors = _storeService.GetAllStores(true).Select(s =>
                {
                    var m = s.ToModel<StoreModel>();
                    if ((!string.IsNullOrEmpty(m.Code) ? m.Code.Trim() : "") == "SYSTEM")
                    {
                        m.Id = 0;
                    }

                    return m;
                });
                return stors.ToList();
            }
        }

        /// <summary>
        /// 会计期间是否锁定
        /// </summary>
        /// <param name="period"></param>
        /// <returns></returns>
        [NonAction]
        protected bool PeriodLocked(DateTime period, int store)
        {
            var _closingAccountsService = EngineContext.Current.Resolve<IClosingAccountsService>();
            return _closingAccountsService.IsLocked(store, DateTime.Now);
        }

        /// <summary>
        /// 会计期间是否冻结
        /// </summary>
        /// <param name="period"></param>
        /// <param name="store"></param>
        /// <returns></returns>
        [NonAction]
        protected bool PeriodClosed(DateTime period, int store)
        {
            var _closingAccountsService = EngineContext.Current.Resolve<IClosingAccountsService>();
            return _closingAccountsService.IsClosed(store, DateTime.Now);
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
        protected T BindDropDownList<T>(T model, Func<int?, int, List<Branch>> callback, int? store, int parentId = 0) where T : BaseBill
        {
            List<SelectListItem> parentList = new List<SelectListItem>();
            var nodes = callback(store ?? 0, parentId);

            if (parentId == 0)
            {
                parentList.Add(new SelectListItem
                {
                    Text = " ┝  根目录",
                    Value = "0"
                });
            }

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
                string blank = "    ┝━ ";
                BindNode(callback, sonpid, store, blank, parentList);
            }
            //model.ParentList = new SelectList(parentList, "Value", "Text");
            return model;
        }

        /// <summary>
        /// 品牌下拉绑定
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="sonpid"></param>
        /// <param name="store"></param>
        /// <param name="blank"></param>
        /// <param name="selectList"></param>
        [NonAction]
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
        protected T BindCategoryDropDownList<T>(T model, Func<int?, int, bool, IList<Category>> callback, int? store, int parentId = 0) where T : BaseBill
        {
            List<SelectListItem> parentList = new List<SelectListItem>();
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
            //model.ParentList = new SelectList(parentList, "Value", "Text");
            return model;
        }

        /// <summary>
        /// 分类下拉绑定
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="sonpid"></param>
        /// <param name="store"></param>
        /// <param name="blank"></param>
        /// <param name="selectList"></param>
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
        /// <param name="store"></param>
        /// <param name="userRoleIds"></param>
        /// <returns></returns>
        [NonAction]
        protected SelectList BindUserSelection(Func<int, string,int, bool, bool, List<User>> callback,int? store, string userRoleIds,int curUserId = 0,bool selectSubordinate = false, bool isadmin = false)
        {
            return new SelectList(callback(store ?? 0, userRoleIds, curUserId, selectSubordinate, isadmin).Select(u =>
            {
                return new SelectListItem() { Text = u.UserRealName, Value = u.Id.ToString() };
            }).ToList(), "Value", "Text");
        }


        /// <summary>
        /// 仓库下拉绑定（不分仓库类型）
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="storeId"></param>
        /// <returns></returns>
        [NonAction]
        protected SelectList BindWareHouseSelection(Func<int?, WHAEnum?, int, IList<WareHouse>> callback, int? storeId, WHAEnum? wHA, int userId = 0, int defaultValue = 0)
        {
            var items = callback(storeId, wHA, userId).Select(u =>
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
        /// <param name="storeId"></param>
        /// <param name="type"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        [NonAction]
        protected SelectList BindWareHouseByTypeSelection(Func<int?, int?, WHAEnum?, int, IList<WareHouse>> callback, int? storeId, int type, WHAEnum? wHA, int userId = 0, int defaultValue = 0)
        {
            var items = callback(storeId, type, wHA, userId).Select(u =>
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
        /// <param name="store"></param>
        /// <returns></returns>
        [NonAction]
        protected SelectList BindManufacturerSelection(Func<int?, IList<Manufacturer>> callback, int? store)
        {
            return new SelectList(callback(store ?? 0).Select(u =>
            {
                return new SelectListItem() { Text = u.Name, Value = u.Id.ToString() };
            }).ToList(), "Value", "Text");
        }


        /// <summary>
        /// 品牌下拉绑定
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="store"></param>
        /// <returns></returns>
        [NonAction]
        protected SelectList BindBrandSelection(Func<int?, IList<Brand>> callback, int? store)
        {
            return new SelectList(callback(store ?? 0).Select(u =>
            {
                return new SelectListItem() { Text = u.Name, Value = u.Id.ToString() };
            }).ToList(), "Value", "Text");
        }


        /// <summary>
        /// 商品类别下拉绑定
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="store"></param>
        /// <returns></returns>
        [NonAction]
        protected SelectList BindCategorySelection(Func<int?, IList<Category>> callback, int? store)
        {
            return new SelectList(callback(store ?? 0).Select(u =>
            {
                return new SelectListItem() { Text = u.Name, Value = u.Id.ToString() };
            }).ToList(), "Value", "Text");
        }


        /// <summary>
        /// 片区下拉绑定
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="store"></param>
        /// <returns></returns>
        [NonAction]
        protected SelectList BindDistrictSelection(Func<int?, IList<District>> callback, int? store)
        {
            return new SelectList(callback(store ?? 0).Select(u =>
            {
                return new SelectListItem() { Text = u.Name, Value = u.Id.ToString() };
            }).ToList(), "Value", "Text");
        }


        /// <summary>
        /// 客户渠道
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="store"></param>
        /// <returns></returns>
        [NonAction]
        protected SelectList BindChanneSelection(Func<int?, IList<Channel>> callback, int? store)
        {
            return new SelectList(callback(store ?? 0).Select(u =>
            {
                return new SelectListItem() { Text = u.Name, Value = u.Id.ToString() };
            }).ToList(), "Value", "Text");
        }

        /// <summary>
        /// 客户等级
        /// </summary>
        /// <returns></returns>
        [NonAction]
        protected SelectList BindRankSelection(Func<int?, IList<Rank>> callback, int? store)
        {
            return new SelectList(callback(store ?? 0).Select(u =>
            {
                return new SelectListItem() { Text = u.Name, Value = u.Id.ToString() };
            }).ToList(), "Value", "Text");
        }



        /// <summary>
        /// 配置项
        /// </summary>
        /// <returns></returns>
        [NonAction]
        protected SelectList BindOptionSelection(Dictionary<string, int> options)
        {
            return new SelectList(options.Select(u =>
            {
                return new SelectListItem() { Text = u.Key, Value = u.Value.ToString() };
            }).ToList(), "Value", "Text");
        }



        /// <summary>
        /// 报表单据类型下拉绑定
        /// </summary>
        /// <returns></returns>
        [NonAction]
        protected SelectList BindBillTypeSelection()
        {
            // var types = new Dictionary<string, string>() { { "全部", "0" } };
            var types = Enum.GetValues(typeof(BillTypeReportEnum)).Cast<BillTypeReportEnum>();
            return new SelectList(types.Select(u =>
            {
                return new SelectListItem() { Text = CommonHelper.GetEnumDescription(u), Value = ((int)u).ToString() };
            }).ToList(), "Value", "Text");
        }

        /// <summary>
        /// 会计科目
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="store"></param>
        /// <param name="typeid"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        [NonAction]
        protected SelectList BindAccountSelection(Func<int?, int, IList<AccountingOption>> callback, int? store, int typeid, int defaultValue = 0)
        {
            var items = callback(store ?? 0, typeid).Select(u =>
              {
                  return new SelectListItem() { Text = u.Name, Value = u.Id.ToString(), Selected = (u.Id.Equals(defaultValue)) };
              }).ToList();
            return new SelectList(items, "Value", "Text");
        }


        /// <summary>
        /// 默认售价类型
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="curStore"></param>
        /// <returns></returns>
        [NonAction]
        protected SelectList BindPricePlanSelection(Func<int?, IList<ProductPricePlan>> callback, int curStore)
        {
            return new SelectList(callback(curStore).Where(s => s.PriceTypeId != 5).Select(a =>
            {
                return new SelectListItem() { Text = a.Name, Value = a.PricesPlanId.ToString() + "_" + a.PriceTypeId.ToString() };

            }).ToList(), "Value", "Text");
        }



        /// <summary>
        /// 会计期间是否冻结
        /// </summary>
        /// <param name="period"></param>
        /// <returns></returns>
        [NonAction]
        protected bool PeriodClosed(int store, DateTime period)
        {
            var _closingAccountsService = EngineContext.Current.Resolve<IClosingAccountsService>();
            return _closingAccountsService.IsClosed(store, DateTime.Now);
        }



        /// <summary>
        /// 会计期间是否锁定
        /// </summary>
        /// <param name="period"></param>
        /// <returns></returns>
        [NonAction]
        protected bool PeriodLocked(int store, DateTime period)
        {
            var _closingAccountsService = EngineContext.Current.Resolve<IClosingAccountsService>();
            return _closingAccountsService.IsLocked(store, DateTime.Now);
        }


        /// <summary>
        /// Warning
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        [NonAction]
        protected APIResult<object> Warning(string message)
        {
            return Warning(message, null);
        }
        [NonAction]
        protected APIResult<object> Warning(string message, object data)
        {
            return new APIResult<object>()
            {
                Code = (int)DCMSStatusCode.Failed,
                Message = string.IsNullOrEmpty(message) ? Resources.Failed : message,
                Data = data,
                Success = false
            };
        }

        /// <summary>
        /// Successful
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        [NonAction]
        protected APIResult<object> Successful(string message)
        {
            return Successful(message, null);
        }
        [NonAction]
        protected APIResult<object> Successful(string message, object data)
        {
            return Successful(true, message, data);
        }
        [NonAction]
        protected APIResult<object> Successful(bool status, string message, object data = null)
        {
            return new APIResult<object>()
            {
                Code = (int)DCMSStatusCode.Successful,
                Success = true,
                Message = string.IsNullOrEmpty(message) ? Resources.Successful : message,
                Data = data
            };
        }
        [NonAction]
        protected APIResult<IList<T>> Successful<T>(string message, IList<T> data) where T : BaseEntityModel
        {
            return new APIResult<IList<T>>()
            {
                Code = (int)DCMSStatusCode.Successful,
                Success = true,
                Message = string.IsNullOrEmpty(message) ? Resources.Successful : message,
                Data = data
            };
        }
        [NonAction]
        protected APIResult<IList<T>> Successful2<T>(string message, IList<T> data) where T : class
        {
            return new APIResult<IList<T>>()
            {
                Code = (int)DCMSStatusCode.Successful,
                Success = true,
                Message = string.IsNullOrEmpty(message) ? Resources.Successful : message,
                Data = data
            };
        }
        [NonAction]
        protected APIResult<T> Successful<T>(string message, T data = null) where T : BaseEntityModel
        {
            return new APIResult<T>()
            {
                Code = (int)DCMSStatusCode.Successful,
                Success = true,
                Message = string.IsNullOrEmpty(message) ? Resources.Successful : message,
                Data = data ?? null
            };
        }
        [NonAction]
        protected APIResult<T> Successful3<T>(string message, T data = null) where T : class
        {
            return new APIResult<T>()
            {
                Code = (int)DCMSStatusCode.Successful,
                Success = true,
                Message = string.IsNullOrEmpty(message) ? Resources.Successful : message,
                Data = data ?? null,
            };
        }

        [NonAction]
        protected APIResult<IPagedList<T>> Successful8<T>(string message, IPagedList<T> data) //where T : IPagedList
        {
            return new APIResult<IPagedList<T>>()
            {
                Code = (int)DCMSStatusCode.Successful,
                Success = true,
                Message = string.IsNullOrEmpty(message) ? Resources.Successful : message,
                Data = data,
                Rows = data.TotalCount,
                Pages = data.TotalPages
            };
        }

        /// <summary>
        /// Error
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        [NonAction]
        protected APIResult<object> Error(string message)
        {
            return Error(message, null);
        }
        [NonAction]
        protected APIResult<object> Error(string message, object data)
        {
            return new APIResult<object>()
            {
                Code = (int)DCMSStatusCode.Failed,
                Data = data,
                Message = string.IsNullOrEmpty(message) ? Resources.Failed : message,
                Success = false
            };
        }
        [NonAction]
        protected APIResult<IList<T>> Error<T>(string message) where T : BaseEntityModel
        {
            return new APIResult<IList<T>>()
            {
                Code = (int)DCMSStatusCode.Failed,
                Data = null,
                Message = string.IsNullOrEmpty(message) ? Resources.Failed : message,
                Success = false
            };
        }
        [NonAction]
        protected APIResult<IList<T>> Error2<T>(string message) where T : class
        {
            return new APIResult<IList<T>>()
            {
                Code = (int)DCMSStatusCode.Failed,
                Data = null,
                Message = string.IsNullOrEmpty(message) ? Resources.Failed : message,
                Success = false
            };
        }
        [NonAction]
        protected APIResult<T> Error<T>(bool failed, string message) where T : BaseEntityModel
        {
            return new APIResult<T>()
            {
                Code = (int)DCMSStatusCode.Failed,
                Data = null,
                Message = string.IsNullOrEmpty(message) ? Resources.Failed : message,
                Success = false
            };
        }
        [NonAction]
        protected APIResult<T> Error3<T>(string message) where T : class
        {
            return new APIResult<T>()
            {
                Code = (int)DCMSStatusCode.Failed,
                Data = null,
                Message = string.IsNullOrEmpty(message) ? Resources.Failed : message,
                Success = false
            };
        }


        [NonAction]
        protected string LockKey<T>(T data, int store, int userId) where T : BaseEntityModel
        {
            return string.Format(DCMSCachingDefaults.RedisDataReSubmitKey, Request.GetUrl(), store, userId, CommonHelper.MD5(JsonConvert.SerializeObject(data)));
        }
        [NonAction]
        protected string LockKeys<T>(IList<T> datas, int store, int userId) where T : BaseEntityModel
        {
            return string.Format(DCMSCachingDefaults.RedisDataReSubmitKey, Request.GetUrl(), store, userId, CommonHelper.MD5(JsonConvert.SerializeObject(datas)));
        }
        [NonAction]
        protected string LockKey<T>(IList<T> datas, int store, int userId) where T : BaseEntity
        {
            return string.Format(DCMSCachingDefaults.RedisDataReSubmitKey, Request.GetUrl(), store, userId, CommonHelper.MD5(JsonConvert.SerializeObject(datas)));
        }


        [NonAction]
        protected string RedLockKey<T>(T data, int store, int userId) where T : BaseEntity
        {
            return string.Format(DCMSCachingDefaults.AuditingSubmitKey, Request.GetUrl(), store, userId, CommonHelper.MD5(JsonConvert.SerializeObject(data)));
        }
        [NonAction]
        protected string RedLockKeys<T>(IList<T> datas, int store, int userId) where T : BaseEntity
        {
            return string.Format(DCMSCachingDefaults.AuditingSubmitKey, Request.GetUrl(), store, userId, CommonHelper.MD5(JsonConvert.SerializeObject(datas)));
        }


        [NonAction]
        protected APIResult<dynamic> BillChecking<T, Items>(T bill, int store, BillStates operation) where T : DCMS.Core.BaseBill<Items> where Items : BaseEntity
        {
            if (bill == null)
            {
                return this.Warning(Resources.Bill_NotExist);
            }
            if (bill.StoreId != store)
            {
                return this.Warning(Resources.IllegalOperation);
            }

            switch (operation)
            {
                case BillStates.Draft:
                    {
                        if (bill.AuditedStatus || bill.ReversedStatus)
                        {
                            return this.Warning("非法操作.");
                        }
                    }
                    break;
                case BillStates.Audited: //审核
                    {
                        if (bill.AuditedStatus || bill.ReversedStatus)
                        {
                            return this.Warning("重复操作.");
                        }

                        if (bill.Items == null || !bill.Items.Any())
                        {
                            return this.Warning("单据没有明细.");
                        }

                    }
                    break;
                case BillStates.Reversed: //红冲
                    {
                        if (!bill.AuditedStatus || bill.ReversedStatus)
                        {
                            return this.Warning("非法操作.");
                        }

                        if (bill.Items == null || !bill.Items.Any())
                        {
                            return this.Warning("单据没有明细.");
                        }

                        if (DateTime.Now.Subtract(bill.AuditedDate ?? DateTime.Now).TotalSeconds > 86400)
                        {
                            return this.Warning("已经审核的单据超过24小时，不允许红冲.");
                        }
                    }
                    break;
            }

            if (bill.Deleted)
            {
                return this.Warning("单据已作废.");
            }

            //成功返回null
            return new APIResult<dynamic>();
        }

    }
}

