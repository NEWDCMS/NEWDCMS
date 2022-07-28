using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Domain.Configuration;
using DCMS.Core.Domain.Products;
using DCMS.Core.Domain.Stores;
using DCMS.Core.Domain.Terminals;
using DCMS.Core.Domain.Users;
using DCMS.Core.Domain.WareHouses;
using DCMS.Core.Infrastructure;
using DCMS.Core.Infrastructure.DependencyManagement;
using DCMS.Services.Configuration;
using DCMS.Services.Events;
using DCMS.Services.Products;
using DCMS.Services.Security;
using DCMS.Services.Terminals;
using DCMS.Services.Users;
using DCMS.Services.WareHouses;
using System;
using System.Collections.Generic;
using System.Linq;
using DCMS.Services.Caching;

namespace DCMS.Services.Stores
{
    /// <summary>
    /// 经销商服务
    /// </summary>
    public partial class StoreService : BaseService, IStoreService
    {
        //这里使用_cacheManager 替代 cacheManager
        
        public StoreService(IServiceGetter getter,
            IStaticCacheManager cacheManager,
            IEventPublisher eventPublisher) : base(getter, cacheManager, eventPublisher)
        {
            
        }

        #region Methods


        /// <summary>
        ///  删除
        /// </summary>
        /// <param name="store"></param>
        public virtual void DeleteStore(Store store)
        {
            if (store == null)
            {
                throw new ArgumentNullException(nameof(store));
            }

            if (store is IEntityForCaching)
            {
                throw new ArgumentException("Cacheable entities are not supported by Entity Framework");
            }

            var allStores = GetAllStores(true);
            if (allStores.Count == 1)
            {
                throw new Exception("You cannot delete the only configured store");
            }

            var uow = StoreRepository.UnitOfWork;
            StoreRepository.Delete(store);
            uow.SaveChanges();

            //event notification
            _eventPublisher.EntityDeleted(store);
        }

        /// <summary>
        /// 获取全部经销商
        /// </summary>
        /// <param name="name"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public virtual IPagedList<Store> GetAllStores(string name = null, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            if (pageSize >= 50)
                pageSize = 50;
            var query = StoreRepository_RO.Table;

            if (!string.IsNullOrWhiteSpace(name))
            {
                query = query.Where(c => c.Name.Contains(name));
            }

            query = query.OrderByDescending(c => c.DisplayOrder);

            //总页数
            var totalCount = query.Count();
            var plists = query.Skip(pageIndex * pageSize).Take(pageSize).ToList();
            return new PagedList<Store>(plists, pageIndex, pageSize, totalCount);

        }

        /// <summary>
        /// 获取全部
        /// </summary>
        /// <returns></returns>
        public virtual IList<Store> GetAllStores(bool loadCacheableCopy = true)
        {
            IList<Store> LoadStoresFunc()
            {
                var query = from s in StoreRepository_RO.Table orderby s.DisplayOrder, s.Id select s;
                return query.ToList();
            }

            if (loadCacheableCopy)
            {
                return _cacheManager.Get(DCMSStoreDefaults.StoresByIdCacheKey.FillCacheKey(loadCacheableCopy), () =>
                {
                    var result = new List<Store>();
                    foreach (var store in LoadStoresFunc())
                    {
                        result.Add(new StoreForCaching(store));
                    }
                    return result;
                });
            }

            return LoadStoresFunc();
        }
        /// <summary>
        /// 获取全部
        /// </summary>
        /// <returns></returns>
        public virtual IList<Corporations> GetAllfactory()
        {
            var query = from s in CorporationsRepository_RO.Table
                        where s.FactoryId != 3 && s.ShortName.Contains("分公司")
                        orderby s.FactoryId ascending
                        select s;
            return query.ToList();
        }
        /// <summary>
        /// 绑定经销商信息
        /// </summary>
        /// <returns></returns>
        public virtual IList<Store> BindStoreList()
        {
            return _cacheManager.Get(DCMSDefaults.BINDSTORE_ALLLIST.FillCacheKey(0), () =>
             {
                 var query = from s in StoreRepository_RO.TableNoTracking
                             orderby s.DisplayOrder, s.Name
                             select s;
                 var result = query.Select(q => new { Id = q.Id, Name = q.Name }).ToList().Select(x => new Store { Id = x.Id, Name = x.Name }).ToList();
                 return result;
             });
        }

        /// <summary>
        /// 获取
        /// </summary>
        /// <param name="storeId"></param>
        /// <returns></returns>
        public virtual Store GetStoreById(int storeId)
        {
            if (storeId == 0)
            {
                return null;
            }

            return StoreRepository_RO.GetById(storeId);
        }


        public virtual Store GetStoreByUserId(int userId)
        {
            if (userId == 0)
            {
                return null;
            }

            var user = UserRepository_RO.TableNoTracking.Where(u => u.Id == userId).FirstOrDefault();

            return GetStoreById(user?.StoreId ?? 0);
        }


        public virtual Store GetManageStore()
        {
            var store = StoreRepository_RO.TableNoTracking.Where(u => u.Code == "SYSTEM").FirstOrDefault();
            return store;
        }


        public virtual string GetStoreName(int storeId)
        {
            if (storeId == 0)
            {
                return "";
            }
            //var store = GetStoreById(storeId);
            //return store != null ? store.Name : "";
            var key = DCMSDefaults.STORE_NAME_BY_ID_KEY.FillCacheKey(storeId);
            return _cacheManager.Get(key, () =>
            {
                return StoreRepository_RO.Table.Where(a => a.Id == storeId).Select(a => a.Name).FirstOrDefault();
            });
        }

        public virtual IList<Store> GetStoresByIds(int[] sIds)
        {
            if (sIds == null || sIds.Length == 0)
            {
                return new List<Store>();
            }

            var query = from c in StoreRepository_RO.Table
                        where sIds.Contains(c.Id)
                        select c;
            var stores = query.ToList();

            var sortedStores = new List<Store>();
            foreach (int id in sIds)
            {
                var store = stores.Find(x => x.Id == id);
                if (store != null)
                {
                    sortedStores.Add(store);
                }
            }

            return sortedStores;
        }



        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="store"></param>
        public virtual void InsertStore(Store store)
        {
            if (store == null)
            {
                throw new ArgumentNullException("store");
            }

            var uow = StoreRepository.UnitOfWork;
            StoreRepository.Insert(store);
            uow.SaveChanges();

            //event notification
            _eventPublisher.EntityInserted(store);
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="store"></param>
        public virtual void UpdateStore(Store store)
        {
            if (store == null)
            {
                throw new ArgumentNullException("store");
            }

            var uow = StoreRepository.UnitOfWork;
            StoreRepository.Update(store);
            uow.SaveChanges();

            _cacheManager.RemoveByPrefix(DCMSDefaults.STORES_PK);
            //event notification
            _eventPublisher.EntityUpdated(store);
        }


        /// <summary>
        /// 验证编码是否存在(新增验证)
        /// </summary>
        /// <param name="storeCode"></param>
        /// <returns></returns>
        public bool CheckStoreCode(string storeCode)
        {
            var query = from s in StoreRepository_RO.Table
                        where s.Code == storeCode
                        orderby s.Id
                        select s;
            return query.ToList().Count() > 0;
        }




        #endregion


        #region 终端


        /// <summary>
        /// 获取经销商终端
        /// </summary>
        /// <returns></returns>
        public virtual IList<Terminal> GetTerminals(int? storeId)
        {
            if (storeId.HasValue)
            {
                var ids = new List<int>();
                var tids = from s in CRM_RELATIONRepository_RO.Table
                           where s.StoreId == storeId
                           orderby s.Id ascending
                           select s.TerminalId;

                if (tids != null && tids.Any())
                    ids = tids.ToList();

                var query = from s in TerminalsRepository_RO.Table
                            where ids.Contains(s.Id)
                            orderby s.Id ascending
                            select s;

                return query.ToList();
            }
            else
                return null;
        }

        #endregion


        /// <summary>
        /// 脚本新增经销商
        /// </summary>
        /// <param name="store"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public bool AddStoreScript(Store store, User user)
        {

            bool fg = true;
            try
            {
                if (fg)
                {

                    var userService = EngineContext.Current.Resolve<IUserService>();

                    int storeId = store.Id;
                    int userId = user.Id;
                    int userRoleId = 0;
                    //step1:
                    #region 插入角色

                    //超级管理员
                    UserRole userRole;
                    if (user.UserRoles.ToList().Where(u => u.SystemName == "Administrators").Count() == 0)
                    {
                        userRole = new UserRole
                        {
                            Name = "超级管理员",
                            StoreId = storeId,
                            Active = true,
                            IsSystemRole = false,
                            SystemName = "Administrators",
                            Description = "超级管理员"
                        };

                        userService.InsertUserRole(userRole);
                        userRoleId = userRole.Id;
                        user.UserRoles.Add(userRole);
                    }
                    else
                    {
                        userRole = user.UserRoles.ToList().Where(u => u.SystemName == "Administrators").FirstOrDefault();
                        userRoleId = userRole.Id;
                    }

                    //员工
                    if (user.UserRoles.ToList().Where(u => u.SystemName == "Employees").Count() == 0)
                    {
                        var userRole2 = new UserRole
                        {
                            Name = "员工",
                            StoreId = storeId,
                            Active = true,
                            IsSystemRole = false,
                            SystemName = "Employees",
                            Description = "员工"
                        };

                        userService.InsertUserRole(userRole2);
                        user.UserRoles.Add(userRole2);
                    }

                    //业务员
                    if (user.UserRoles.ToList().Where(u => u.SystemName == "Salesmans").Count() == 0)
                    {
                        var userRole3 = new UserRole
                        {
                            Name = "业务员",
                            StoreId = storeId,
                            Active = true,
                            IsSystemRole = false,
                            SystemName = "Salesmans",
                            Description = "业务员"
                        };

                        userService.InsertUserRole(userRole3);
                        user.UserRoles.Add(userRole3);
                    }

                    //送货员
                    if (user.UserRoles.ToList().Where(u => u.SystemName == "Delivers").Count() == 0)
                    {
                        var userRole4 = new UserRole
                        {
                            Name = "送货员",
                            StoreId = storeId,
                            Active = true,
                            IsSystemRole = false,
                            SystemName = "Delivers",
                            Description = "送货员"
                        };

                        userService.InsertUserRole(userRole4);
                        user.UserRoles.Add(userRole4);
                    }

                    //经销商管理员
                    if (user.UserRoles.ToList().Where(u => u.SystemName == "Distributors").Count() == 0)
                    {
                        var userRole5 = new UserRole
                        {
                            Name = "经销商管理员",
                            StoreId = storeId,
                            Active = true,
                            IsSystemRole = false,
                            SystemName = "Distributors",
                            Description = "经销商管理员"
                        };

                        userService.InsertUserRole(userRole5);
                        user.UserRoles.Add(userRole5);
                    }

                    #endregion

                    //step2:
                    #region 用户角色关系
                    //user.UserRoles.Add(userRole);
                    //user.UserRoles.Add(userRole2);
                    //user.UserRoles.Add(userRole3);
                    //user.UserRoles.Add(userRole4);
                    //user.UserRoles.Add(userRole5);
                    userService.UpdateUser(user);
                    #endregion

                    //step3:
                    #region 角色模块(脚本只给超级管理员默认所有权限)
                    var moduleService = EngineContext.Current.Resolve<IModuleService>();
                    var modules = moduleService.GetAllModules();
                    if (modules != null && modules.Count > 0)
                    {
                        modules.ToList().ForEach(m =>
                        {
                            if (userRole.ModuleRoles.Select(s => s.Module).Where(um => um.Id == m.Id).Count() == 0)
                            {
                                userRole.ModuleRoles.Add(new Core.Domain.Security.ModuleRole
                                {
                                    Module_Id = m.Id,
                                    Module = m,
                                    UserRole_Id = userRole.Id,
                                    UserRole = userRole
                                });
                            }
                        });
                    }
                    userService.UpdateUserRole(userRole);

                    #endregion

                    //step5:
                    #region 默认基本数据表

                    #region 品牌档案 Brand
                    var _brandService = EngineContext.Current.Resolve<IBrandService>();

                    List<Brand> brands = _brandService.GetAllBrands(storeId).ToList();

                    if (brands == null || brands.Where(b => b.Name == "雪花").Count() == 0)
                    {
                        Brand brand = new Brand()
                        {
                            StoreId = storeId,
                            Name = "雪花",
                            Status = true,
                            DisplayOrder = 0,
                            CreatedOnUtc = DateTime.Now
                        };
                        _brandService.InsertBrand(brand);
                    }

                    #endregion

                    #region 商品类别 Category
                    var _categoryService = EngineContext.Current.Resolve<ICategoryService>();
                    List<Category> categorys = _categoryService.GetAllCategories(storeId).ToList();

                    Category category;
                    if (categorys == null || categorys.Where(c => c.Name == "全部").Count() == 0)
                    {
                        category = new Category()
                        {
                            StoreId = storeId,
                            Name = "全部",
                            ParentId = 0,
                            PathCode = "1",
                            StatisticalType = 1,
                            Status = 0,
                            OrderNo = 0,
                            BrandId = null,
                            BrandName = "",
                            Deleted = false,
                            Published = true,
                            PercentageId = null
                        };
                        _categoryService.InsertCategory(category);

                    }
                    else
                    {
                        category = categorys.Where(c => c.Name == "全部").FirstOrDefault();
                    }

                    if (categorys == null || categorys.Where(c => c.Name == "啤酒").Count() == 0)
                    {
                        Category category2 = new Category()
                        {
                            StoreId = storeId,
                            Name = "啤酒",
                            ParentId = category.Id,
                            PathCode = null,
                            StatisticalType = 0,
                            Status = 0,
                            OrderNo = 0,
                            BrandId = null,
                            BrandName = "",
                            Deleted = false,
                            Published = true,
                            PercentageId = null
                        };
                        _categoryService.InsertCategory(category2);
                    }

                    #endregion

                    #region 渠道 Channel
                    var _channelService = EngineContext.Current.Resolve<IChannelService>();
                    List<Channel> channels = _channelService.GetAll(storeId).ToList();
                    if (channels == null || channels.Where(c => c.Name == "商超").Count() == 0)
                    {
                        Channel channel1 = new Channel()
                        {
                            StoreId = storeId,
                            OrderNo = 0,
                            Name = "商超",
                            Describe = "商超",
                            Attribute = 4,
                            Deleted = false,
                            CreateDate = DateTime.Now
                        };
                        _channelService.InsertChannel(channel1);
                    }

                    if (channels == null || channels.Where(c => c.Name == "批发").Count() == 0)
                    {
                        Channel channel2 = new Channel()
                        {
                            StoreId = storeId,
                            OrderNo = 0,
                            Name = "批发",
                            Describe = "批发",
                            Attribute = 5,
                            Deleted = false,
                            CreateDate = DateTime.Now
                        };
                        _channelService.InsertChannel(channel2);
                    }

                    if (channels == null || channels.Where(c => c.Name == "餐饮").Count() == 0)
                    {
                        Channel channel3 = new Channel()
                        {
                            StoreId = storeId,
                            OrderNo = 0,
                            Name = "餐饮",
                            Describe = "餐饮",
                            Attribute = 2,
                            Deleted = false,
                            CreateDate = DateTime.Now
                        };
                        _channelService.InsertChannel(channel3);
                    }

                    if (channels == null || channels.Where(c => c.Name == "特殊渠道").Count() == 0)
                    {
                        Channel channel4 = new Channel()
                        {
                            StoreId = storeId,
                            OrderNo = 0,
                            Name = "特殊渠道",
                            Describe = "特殊渠道",
                            Attribute = 1,
                            Deleted = false,
                            CreateDate = DateTime.Now
                        };
                        _channelService.InsertChannel(channel4);
                    }

                    #endregion

                    #region 片区 District
                    var _districtService = EngineContext.Current.Resolve<IDistrictService>();
                    List<District> districts = _districtService.GetAll(storeId).ToList();

                    if (districts == null || districts.Where(b => b.Name == "全部").Count() == 0)
                    {
                        District district = new District()
                        {
                            StoreId = storeId,
                            Name = "全部",
                            ParentId = 0,
                            OrderNo = 0,
                            Describe = "",
                            Deleted = false
                        };
                        _districtService.InsertDistrict(district);
                    }

                    #endregion

                    #region 打印模板 PrintTemplate

                    var _printTemplateService = EngineContext.Current.Resolve<IPrintTemplateService>();
                    List<PrintTemplate> printTemplates = _printTemplateService.GetAllPrintTemplates(storeId).ToList();

                    #region 销售订单

                    if (printTemplates == null || printTemplates.Where(p => p.BillType == (int)BillTypeEnum.SaleReservationBill).Count() == 0)
                    {
                        string content1 = "<div id=\"theadid\">  <p style=\"text - align: center; \"><span style=\"font - size: 24pt; font - family: 楷体, 华文楷体; \">@商铺名称</span></p>    <p style=\"text - align: left; \"><span style=\"font - family: 仿宋, 华文仿宋; font - size: 10pt; \">客户：<strong>@客户名称</strong> &nbsp; &nbsp;客户电话：@客户电话 &nbsp; 客户地址：@客户地址 &nbsp;</span></p>    <p style=\"text - align: left; \"><span style=\"font - family: 仿宋, 华文仿宋; font - size: 10pt; \">单据编号：@单据编号 制单：@制单&nbsp; &nbsp; 日期：@日期&nbsp;业务员：@业务员 &nbsp;业务电话：@业务电话</span></p>    </div>    <div id=\"tbodyid\">  <table style=\"width: 729px; \" class=\"table table-bordered\">    <thead>  <tr style=\"height: 20px; \">    <td style=\"width: 35px; height: 20px; text - align: center; \"><span style=\"font - family: 仿宋, 华文仿宋; \"><strong>&nbsp;</strong></span></td>    <td style=\"width: 162px; height: 20px; text - align: center; \"><span style=\"font - family: 仿宋, 华文仿宋; \"><strong>商品名称</strong></span></td>    <td style=\"width: 123px; height: 20px; text - align: center; \"><span style=\"font - family: 仿宋, 华文仿宋; \"><strong>条形码</strong></span></td>    <td style=\"width: 101px; height: 20px; text - align: center; \"><span style=\"font - family: 仿宋, 华文仿宋; \"><strong>单位</strong></span></td>    <td style=\"width: 98px; height: 20px; text - align: center; \"><span style=\"font - family: 仿宋, 华文仿宋; \"><strong>单位换算</strong></span></td>    <td style=\"width: 65px; height: 20px; text - align: center; \"><span style=\"font - family: 仿宋, 华文仿宋; \"><strong>数量</strong></span></td>    <td style=\"width: 61px; height: 20px; text - align: center; \"><span style=\"font - family: 仿宋, 华文仿宋; \"><strong>价格</strong></span></td>    <td style=\"width: 78px; height: 20px; text - align: center; \"><span style=\"font - family: 仿宋, 华文仿宋; \"><strong>金额</strong></span></td>    <td style=\"width: 88px; height: 20px; text - align: center; \"><span style=\"font - family: 仿宋, 华文仿宋; \"><strong>备注</strong></span></td>    </tr>    </thead>    <tbody>    <tr style=\"height: 17px; \">    <td style=\"width: 35px; height: 17px; text - align: center; \"><span style=\"font - family: 仿宋, 华文仿宋; \">&nbsp;#序号</span></td>    <td style=\"width: 162px; height: 17px; \"><strong><span style=\"font - family: 仿宋, 华文仿宋; \">#商品名称</span></strong></td>    <td style=\"width: 123px; height: 17px; \"><span style=\"font - family: 仿宋, 华文仿宋; \">#条形码</span></td>    <td style=\"width: 101px; height: 17px; text - align: center; \"><span style=\"font - family: 仿宋, 华文仿宋; \">#商品单位</span></td>    <td style=\"width: 98px; height: 17px; text - align: center; \"><span style=\"font - family: 仿宋, 华文仿宋; \">#单位换算</span></td>    <td style=\"width: 65px; height: 17px; text - align: right; \"><strong><span style=\"font - family: 仿宋, 华文仿宋; \">#数量</span></strong></td>    <td style=\"width: 61px; height: 17px; text - align: right; \"><span style=\"font - family: 仿宋, 华文仿宋; \">#价格</span></td>    <td style=\"width: 78px; height: 17px; text - align: right; \"><strong><span style=\"font - family: 仿宋, 华文仿宋; \">#金额</span></strong></td>    <td style=\"width: 88px; height: 17px; text - align: center; \"><span style=\"font - family: 仿宋, 华文仿宋; \">#备注</span></td>    </tr>    </tbody>    <tfoot>  <tr style=\"height: 24px; \">    <td style=\"width: 35px; height: 24px; \"><span style=\"font - family: 仿宋, 华文仿宋; \">&nbsp;</span></td>    <td style=\"width: 162px; height: 24px; \"><span style=\"font - family: 仿宋, 华文仿宋; \"><strong>总计</strong></span></td>    <td style=\"width: 123px; height: 24px; \"><span style=\"font - family: 仿宋, 华文仿宋; \">&nbsp;</span></td>    <td style=\"width: 101px; height: 24px; \"><span style=\"font - family: 仿宋, 华文仿宋; \">&nbsp;</span></td>    <td style=\"width: 98px; height: 24px; text - align: center; \"><span style=\"font - family: 仿宋, 华文仿宋; \">&nbsp;</span></td>    <td style=\"width: 65px; height: 24px; text - align: right; \"><strong><span style=\"font - family: 仿宋, 华文仿宋; \">&nbsp;数量:###</span></strong></td>    <td style=\"width: 61px; height: 24px; text - align: center; \"><span style=\"font - family: 仿宋, 华文仿宋; \">&nbsp;</span></td>    <td style=\"width: 78px; height: 24px; text - align: right; \"><strong><span style=\"font - family: 仿宋, 华文仿宋; \">金额:###</span></strong></td>    <td style=\"width: 88px; height: 24px; \"><span style=\"font - family: 仿宋, 华文仿宋; \">&nbsp;</span></td>    </tr>    </tfoot>  </table>    </div>    <div id=\"tfootid\">  <p style=\"text - align: left; \"><span style=\"font - family: 仿宋, 华文仿宋; font - size: 10pt; \">公司地址：@公司地址监督电话：15802908655</span><span style=\"font - family: 仿宋, 华文仿宋; font - size: 10pt; \">&nbsp;</span></p>    <p><span style=\"font - family: 仿宋, 华文仿宋; \"><strong>送货人：&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;收货人:</strong>&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;<strong>收款方式</strong>：现金【】微信【】支付宝【】转账【】&nbsp;</span></p>    </div>";

                        PrintTemplate printTemplate1 = new PrintTemplate()
                        {
                            StoreId = storeId,
                            TemplateType = 0,
                            BillType = (int)BillTypeEnum.SaleReservationBill,
                            Title = CommonHelper.GetEnumDescription(BillTypeEnum.SaleReservationBill),
                            Content = content1
                        };
                        _printTemplateService.InsertPrintTemplate(printTemplate1);
                    }

                    #endregion

                    #region 销售单

                    if (printTemplates == null || printTemplates.Where(p => p.BillType == (int)BillTypeEnum.SaleBill).Count() == 0)
                    {
                        string content2 = "<div id=\"theadid\"><p style=\"text - align: center; \"><span style=\"font - size: 24pt; font - family: 楷体, 华文楷体; \">@商铺名称</span></p><p style=\"text - align: left; \"><span style=\"font - family: 仿宋, 华文仿宋; \">客户：@客户名称 &nbsp;客户电话：@客户电话 &nbsp; 客户地址：@客户地址 &nbsp; &nbsp; &nbsp;</span></p><p style=\"text - align: left; \"><span style=\"font - family: 仿宋, 华文仿宋; \">单据编号：@单据编号制单：@制单&nbsp; &nbsp;日期：@交易日期&nbsp;业务员：@业务员 &nbsp; &nbsp; 业务电话：@业务电话</span></p></div><div id=\"tbodyid\"><table style=\"width: 659px; \" cellpadding=\"0\" class=\"table table-bordered\"><thead><tr style=\"height: 1px; \"><td style=\"width: 36px; height: 1px; \"><span style=\"font - size: 10pt; font - family: 仿宋, 华文仿宋; \">&nbsp;</span></td><td style=\"width: 141px; height: 1px; text - align: center; \"><span style=\"font - size: 8pt; font - family: 仿宋, 华文仿宋; \"><strong>商品名称</strong></span></td><td style=\"width: 67px; height: 1px; text - align: center; \"><span style=\"font - size: 8pt; font - family: 仿宋, 华文仿宋; \"><strong>条形码</strong></span></td><td style=\"width: 80px; height: 1px; text - align: center; \"><span style=\"font - size: 8pt; font - family: 仿宋, 华文仿宋; \"><strong>单位</strong></span></td><td style=\"width: 118px; height: 1px; text - align: center; \"><span style=\"font - size: 8pt; font - family: 仿宋, 华文仿宋; \"><strong>单位换算</strong></span></td><td style=\"width: 93px; height: 1px; text - align: center; \"><span style=\"font - size: 8pt; font - family: 仿宋, 华文仿宋; \"><strong>数量</strong></span></td><td style=\"width: 75px; height: 1px; text - align: center; \"><span style=\"font - size: 8pt; font - family: 仿宋, 华文仿宋; \"><strong>价格</strong></span></td><td style=\"width: 91px; height: 1px; text - align: center; \"><span style=\"font - size: 8pt; font - family: 仿宋, 华文仿宋; \"><strong>金额</strong></span></td><td style=\"width: 100px; height: 1px; text - align: center; \"><span style=\"font - size: 8pt; font - family: 仿宋, 华文仿宋; \"><strong>备注</strong></span></td></tr></thead><tbody><tr style=\"height: 5px; \"><td style=\"width: 36px; height: 5px; text - align: center; \"><span style=\"font - family: 仿宋, 华文仿宋; \">#序号</span></td><td style=\"width: 141px; height: 5px; text - align: left; \"><p><span style=\"font - family: 仿宋, 华文仿宋; \">#商品名称</span></p><p>&nbsp;</p></td><td style=\"width: 67px; height: 5px; \"><span style=\"font - size: 10pt; font - family: 仿宋, 华文仿宋; \">#条形码</span></td><td style=\"width: 80px; height: 5px; text - align: left; \">&nbsp;<span style=\"font - size: 8pt; font - family: 仿宋, 华文仿宋; \">#商品单位</span></td><td style=\"width: 118px; height: 5px; text - align: left; \"><span style=\"font - size: 10pt; font - family: 仿宋, 华文仿宋; \">#单位换算</span></td><td style=\"width: 93px; height: 5px; text - align: right; \"><span style=\"font - size: 10pt; font - family: 仿宋, 华文仿宋; \">#数量</span></td><td style=\"width: 75px; height: 5px; text - align: right; \"><span style=\"font - size: 10pt; font - family: 仿宋, 华文仿宋; \">#价格</span></td><td style=\"width: 91px; height: 5px; text - align: right; \"><span style=\"font - size: 10pt; font - family: 仿宋, 华文仿宋; \">#金额</span></td><td style=\"width: 100px; height: 5px; \"><span style=\"font - size: 10pt; font - family: 仿宋, 华文仿宋; \">#备注</span></td></tr></tbody><tfoot><tr style=\"height: 24px; \"><td style=\"width: 36px; height: 24px; \"><span style=\"font - family: 仿宋, 华文仿宋; \">&nbsp;</span></td><td style=\"width: 141px; height: 24px; \"><span style=\"font - family: 仿宋, 华文仿宋; \"><strong>总计</strong></span></td><td style=\"width: 67px; height: 24px; \"><span style=\"font - family: 仿宋, 华文仿宋; \">&nbsp;</span></td><td style=\"width: 80px; height: 24px; \">&nbsp;</td><td style=\"width: 118px; height: 24px; \"><span style=\"font - family: 仿宋, 华文仿宋; \">&nbsp;</span></td><td style=\"width: 93px; height: 24px; text - align: right; \"><span style=\"font - family: 仿宋, 华文仿宋; \">&nbsp;数量:###</span></td><td style=\"width: 75px; height: 24px; \"><span style=\"font - family: 仿宋, 华文仿宋; \">&nbsp;</span></td><td style=\"width: 91px; height: 24px; text - align: right; \"><span style=\"font - family: 仿宋, 华文仿宋; \">金额:###</span></td><td style=\"width: 100px; height: 24px; \"><span style=\"font - family: 仿宋, 华文仿宋; \">&nbsp;</span></td></tr></tfoot></table></div><div id=\"tfootid\"><p>&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;&nbsp;</p><p>地址：@公司地址 &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;监督电话：15802908655&nbsp; &nbsp;</p><p>送货人：送货人：</p><p>&nbsp;</p><div class=\"entry - mod - catalogue\">&nbsp;</div></div>";

                        PrintTemplate printTemplate2 = new PrintTemplate()
                        {
                            StoreId = storeId,
                            TemplateType = 0,
                            BillType = (int)BillTypeEnum.SaleBill,
                            Title = CommonHelper.GetEnumDescription(BillTypeEnum.SaleBill),
                            Content = content2
                        };
                        _printTemplateService.InsertPrintTemplate(printTemplate2);
                    }

                    #endregion

                    #region 退货订单

                    if (printTemplates == null || printTemplates.Where(p => p.BillType == (int)BillTypeEnum.ReturnReservationBill).Count() == 0)
                    {
                        string content3 = "<div id=\"theadid\">  <p style=\"text - align: center; \"><span style=\"font - size: 24pt; \">@商铺名称</span></p>    <p style=\"text - align: left; \">客户：@客户名称 &nbsp; 仓库：@仓库 &nbsp; &nbsp;业务员：@业务员 &nbsp; &nbsp; 业务电话：@业务电话 &nbsp; &nbsp;</p>    <p style=\"text - align: left; \">单据编号：@单据编号 &nbsp; &nbsp;&nbsp;交易日期：@交易日期</p>    </div>    <div id=\"tbodyid\">  <table style=\"width: 729px; \" class=\"table table-bordered\">    <thead>  <tr>    <td style=\"width: 32px; height: 20px; text - align: center; \"><strong>&nbsp;</strong></td>    <td style=\"width: 256px; height: 20px; text - align: center; \"><strong>商品名称</strong></td>    <td style=\"width: 94px; height: 20px; text - align: center; \"><strong>条形码</strong></td>    <td style=\"width: 50px; height: 20px; text - align: center; \"><strong>单位</strong></td>    <td style=\"width: 98px; height: 20px; text - align: center; \"><strong>单位换算</strong></td>    <td style=\"width: 58px; height: 20px; text - align: center; \"><strong>数量</strong></td>    <td style=\"width: 64px; height: 20px; text - align: center; \"><strong>价格</strong></td>    <td style=\"width: 79px; height: 20px; text - align: center; \"><strong>金额</strong></td>    <td style=\"width: 80px; height: 20px; text - align: center; \"><strong>备注</strong></td>    </tr>    </thead>    <tbody>    <tr>    <td style=\"width: 32px; height: 17px; \">&nbsp;#序号</td>    <td style=\"width: 256px; height: 17px; \">#商品名称</td>    <td style=\"width: 94px; height: 17px; \">#条形码</td>    <td style=\"width: 50px; height: 17px; text - align: center; \">#商品单位</td>    <td style=\"width: 98px; height: 17px; \">#单位换算</td>    <td style=\"width: 58px; height: 17px; text - align: right; \">#数量</td>    <td style=\"width: 64px; height: 17px; text - align: right; \">#价格</td>    <td style=\"width: 79px; height: 17px; text - align: right; \">#金额</td>    <td style=\"width: 80px; height: 17px; \">#备注</td>    </tr>    </tbody>    <tfoot>  <tr>    <td style=\"width: 32px; height: 24px; \">&nbsp;</td>    <td style=\"width: 256px; height: 24px; \"><strong>总计</strong></td>    <td style=\"width: 94px; height: 24px; \">&nbsp;</td>    <td style=\"width: 50px; height: 24px; \">&nbsp;</td>    <td style=\"width: 98px; height: 24px; \">&nbsp;</td>    <td style=\"width: 58px; height: 24px; text - align: right; \">&nbsp;数量:###</td>    <td style=\"width: 64px; height: 24px; \">&nbsp;</td>    <td style=\"width: 79px; height: 24px; text - align: right; \">金额:###</td>    <td style=\"width: 80px; height: 24px; \">&nbsp;</td>    </tr>    </tfoot>  </table>    </div>    <div id=\"tfootid\">  <p>制单：@制单 &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;日期：@日期 &nbsp; &nbsp; &nbsp; &nbsp;</p>    <p>地址：@公司地址 &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;订货电话：@订货电话 &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;&nbsp;</p>    <p>备注：@备注</p>    </div>";

                        PrintTemplate printTemplate3 = new PrintTemplate()
                        {
                            StoreId = storeId,
                            TemplateType = 0,
                            BillType = (int)BillTypeEnum.ReturnReservationBill,
                            Title = CommonHelper.GetEnumDescription(BillTypeEnum.ReturnReservationBill),
                            Content = content3
                        };
                        _printTemplateService.InsertPrintTemplate(printTemplate3);
                    }

                    #endregion

                    #region 退货单

                    if (printTemplates == null || printTemplates.Where(p => p.BillType == (int)BillTypeEnum.ReturnBill).Count() == 0)
                    {
                        string content4 = "<div id=\"theadid\"><p style=\"text - align: center; \"><span style=\"font - size: 24pt; \">@商铺名称</span></p>  <p style=\"text - align: left; \">客户：@客户名称 &nbsp; 仓库：@仓库 &nbsp; &nbsp;业务员：@业务员 &nbsp; &nbsp; 业务电话：@业务电话 &nbsp; &nbsp;</p>  <p style=\"text - align: left; \">单据编号：@单据编号 &nbsp;&nbsp;交易日期：@交易日期</p>  </div>  <div id=\"tbodyid\"><table style=\"width: 720px; \" class=\"table table-bordered\">  <thead><tr>  <td style=\"width: 32px; height: 20px; \"><strong>&nbsp;</strong></td>  <td style=\"width: 211.656px; height: 20px; text - align: center; \"><strong>商品名称</strong></td>  <td style=\"width: 101.344px; height: 20px; text - align: center; \"><strong>条形码</strong></td>  <td style=\"width: 43px; height: 20px; text - align: center; \"><strong>单位</strong></td>  <td style=\"width: 74px; height: 20px; text - align: center; \"><strong>单位换算</strong></td>  <td style=\"width: 49px; height: 20px; text - align: center; \"><strong>数量</strong></td>  <td style=\"width: 67px; height: 20px; text - align: center; \"><strong>价格</strong></td>  <td style=\"width: 71px; height: 20px; text - align: center; \"><strong>金额</strong></td>  <td style=\"width: 89px; height: 20px; text - align: center; \"><strong>备注</strong></td>  </tr>  </thead>  <tbody>  <tr>  <td style=\"width: 32px; height: 17px; \">&nbsp;#序号</td>  <td style=\"width: 211.656px; height: 17px; \"><p>#商品名称</p>  <p>#生产日期</p>  </td>  <td style=\"width: 101.344px; height: 17px; \">#条形码</td>  <td style=\"width: 43px; height: 17px; text - align: center; \">#商品单位</td>  <td style=\"width: 74px; height: 17px; \">#单位换算</td>  <td style=\"width: 49px; height: 17px; text - align: right; \">#数量</td>  <td style=\"width: 67px; height: 17px; text - align: right; \">#价格</td>  <td style=\"width: 71px; height: 17px; text - align: right; \">#金额</td>  <td style=\"width: 89px; height: 17px; \">#备注</td>  </tr>  </tbody>  <tfoot><tr>  <td style=\"width: 32px; height: 24px; \">&nbsp;</td>  <td style=\"width: 211.656px; height: 24px; \"><strong>总计</strong></td>  <td style=\"width: 101.344px; height: 24px; \">&nbsp;</td>  <td style=\"width: 43px; height: 24px; \">&nbsp;</td>  <td style=\"width: 74px; height: 24px; \">&nbsp;</td>  <td style=\"width: 49px; height: 24px; text - align: right; \">&nbsp;数量:###</td>  <td style=\"width: 67px; height: 24px; \">&nbsp;</td>  <td style=\"width: 71px; height: 24px; text - align: right; \">金额:###</td>  <td style=\"width: 89px; height: 24px; \">&nbsp;</td>  </tr>  </tfoot></table>  </div>  <div id=\"tfootid\"><p>制单：@制单 &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;日期：@日期 &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;</p>  <p>地址：@公司地址 &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;订货电话：@订货电话 &nbsp;</p>  <p>备注：@备注 &nbsp;</p>  </div>";

                        PrintTemplate printTemplate4 = new PrintTemplate()
                        {
                            StoreId = storeId,
                            TemplateType = 0,
                            BillType = (int)BillTypeEnum.ReturnBill,
                            Title = CommonHelper.GetEnumDescription(BillTypeEnum.ReturnBill),
                            Content = content4
                        };
                        _printTemplateService.InsertPrintTemplate(printTemplate4);
                    }

                    #endregion

                    #region 车辆对货单

                    if (printTemplates == null || printTemplates.Where(p => p.BillType == (int)BillTypeEnum.CarGoodBill).Count() == 0)
                    {
                        string content5 = "<div id=\"theadid\">    <p style=\"text - align: center; \"><span style=\"font - size: 24pt; \">@商铺名称</span></p>       </div>       <div id=\"tbodyid\">    <table style=\"width: 729px; \" class=\"table table-bordered\">         <thead>            <tr style=\"height: 20px; \">           <td style=\"width: 32px; height: 20px; \"><strong>&nbsp;</strong></td>           <td style=\"width: 50px; height: 20px; \"><strong>单据编号</strong></td>           <td style=\"width: 50px; height: 20px; \"><strong>单据类型</strong></td>           <td style=\"width: 50px; height: 20px; \"><strong>客户</strong></td>           <td style=\"width: 50px; height: 20px; \"><strong>转单时间</strong></td>           <td style=\"width: 50px; height: 20px; \"><strong>仓库</strong></td>           <td style=\"width: 50px; height: 20px; \"><strong>商品名称</strong></td>           <td style=\"width: 50px; height: 20px; \"><strong>销订数量</strong></td>           <td style=\"width: 50px; height: 20px; \"><strong>退订数量</strong></td>           <td style=\"width: 50px; height: 20px; \"><strong>拒收</strong></td>           <td style=\"width: 50px; height: 20px; \"><strong>退货</strong></td>          </tr>         </thead>         <tbody>          <tr style=\"height: 17px; \">           <td style=\"width: 32px; height: 17px; \">&nbsp;#序号</td>           <td style=\"width: 100px; height: 17px; \">#单据编号</td>           <td style=\"width: 100px; height: 17px; \">#单据类型</td>           <td style=\"width: 100px; height: 17px; \">#客户</td>           <td style=\"width: 100px; height: 17px; \">#转单时间</td>           <td style=\"width: 100px; height: 17px; \">#仓库</td>           <td style=\"width: 150px; height: 17px; \">#商品名称</td>           <td style=\"width: 50px; height: 17px; \">#销订数量</td>           <td style=\"width: 50px; height: 17px; \">#退订数量</td>           <td style=\"width: 50px; height: 17px; \">#拒收</td>           <td style=\"width: 50px; height: 17px; \">#退货</td>          </tr>         </tbody>         <tfoot>      <tr style=\"height: 17px; \">           <td style=\"width: 32px; height: 17px; \"><strong>&nbsp;</strong></td>           <td style=\"width: 50px; height: 17px; \"><strong>合计</strong></td>           <td style=\"width: 50px; height: 17px; \"></td>           <td style=\"width: 50px; height: 17px; \"></td>           <td style=\"width: 50px; height: 17px; \"></td>           <td style=\"width: 50px; height: 17px; \"></td>           <td style=\"width: 50px; height: 17px; \"></td>           <td style=\"width: 50px; height: 17px; \">销订数量:###</td>           <td style=\"width: 50px; height: 17px; \">退订数量:###</td>           <td style=\"width: 50px; height: 17px; \">拒收:###</td>           <td style=\"width: 50px; height: 17px; \">退货:###</td>          </tr>         </tfoot>    </table>       </div>       <div id=\"tfootid\">   &nbsp;  </div>";

                        PrintTemplate printTemplate5 = new PrintTemplate()
                        {
                            StoreId = storeId,
                            TemplateType = 0,
                            BillType = (int)BillTypeEnum.CarGoodBill,
                            Title = CommonHelper.GetEnumDescription(BillTypeEnum.CarGoodBill),
                            Content = content5
                        };
                        _printTemplateService.InsertPrintTemplate(printTemplate5);
                    }

                    #endregion

                    #region 采购订单

                    if (printTemplates == null || printTemplates.Where(p => p.BillType == (int)BillTypeEnum.PurchaseReservationBill).Count() == 0)
                    {
                        string content6 = "<div id=\"theadid\">  <p style=\"text - align: center; \"><span style=\"font - size: 24pt; \">@商铺名称</span></p>    <p style=\"text - align: left; \">供应商：@供应商 &nbsp;&nbsp; &nbsp;业务员：@业务员 &nbsp; &nbsp; 业务电话：@业务电话 &nbsp; &nbsp;</p>    <p style=\"text - align: left; \">单据编号：@单据编号 &nbsp; &nbsp;&nbsp;交易日期：@交易日期</p>    </div>    <div id=\"tbodyid\">  <table style=\"width: 720px; \"  class=\"table table-bordered\">  <thead>  <tr>    <td style=\"width: 32px; height: 20px; \"><strong>&nbsp;</strong></td>    <td style=\"width: 214px; height: 20px; text - align: center; \"><strong>商品名称</strong></td>    <td style=\"width: 121px; height: 20px; text - align: center; \"><strong>条形码</strong></td>    <td style=\"width: 62px; height: 20px; text - align: center; \"><strong>单位</strong></td>    <td style=\"width: 101px; height: 20px; text - align: center; \"><strong>单位换算</strong></td>    <td style=\"width: 74px; height: 20px; text - align: center; \"><strong>数量</strong></td>    <td style=\"width: 61px; height: 20px; text - align: center; \"><strong>价格</strong></td>    <td style=\"width: 73px; height: 20px; text - align: center; \"><strong>金额</strong></td>    <td style=\"width: 63px; height: 20px; text - align: center; \"><strong>备注</strong></td>    </tr>    </thead>    <tbody>    <tr>    <td style=\"width: 32px; height: 17px; \">&nbsp;#序号</td>    <td style=\"width: 214px; height: 17px; \">#商品名称</td>    <td style=\"width: 121px; height: 17px; \">#条形码</td>    <td style=\"width: 62px; height: 17px; text - align: center; \">#商品单位</td>    <td style=\"width: 101px; height: 17px; text - align: center; \">#单位换算</td>    <td style=\"width: 74px; height: 17px; text - align: right; \">#数量</td>    <td style=\"width: 61px; height: 17px; text - align: right; \">#价格</td>    <td style=\"width: 73px; height: 17px; text - align: right; \">#金额</td>    <td style=\"width: 63px; height: 17px; \">#备注</td>    </tr>    </tbody>    <tfoot>  <tr>    <td style=\"width: 32px; height: 24px; \">&nbsp;</td>    <td style=\"width: 214px; height: 24px; \"><strong>总计</strong></td>    <td style=\"width: 121px; height: 24px; \">&nbsp;</td>    <td style=\"width: 62px; height: 24px; \">&nbsp;</td>    <td style=\"width: 101px; height: 24px; \">&nbsp;</td>    <td style=\"width: 74px; height: 24px; text - align: right; \">&nbsp;数量:###</td>    <td style=\"width: 61px; height: 24px; \">&nbsp;</td>    <td style=\"width: 73px; height: 24px; text - align: right; \">金额:###</td>    <td style=\"width: 63px; height: 24px; \">&nbsp;</td>    </tr>    </tfoot>  </table>    </div>    <div id=\"tfootid\">  <p>制单：@制单 &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;日期：@日期 &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;&nbsp;</p>    <p>地址：@公司地址 &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;订货电话：@订货电话 &nbsp; &nbsp;</p>    <p>备注：@备注 &nbsp;</p>    </div>";

                        PrintTemplate printTemplate6 = new PrintTemplate()
                        {
                            StoreId = storeId,
                            TemplateType = 0,
                            BillType = (int)BillTypeEnum.PurchaseReservationBill,
                            Title = CommonHelper.GetEnumDescription(BillTypeEnum.PurchaseReservationBill),
                            Content = content6
                        };
                        _printTemplateService.InsertPrintTemplate(printTemplate6);
                    }

                    #endregion

                    #region 采购单

                    if (printTemplates == null || printTemplates.Where(p => p.BillType == (int)BillTypeEnum.PurchaseBill).Count() == 0)
                    {
                        string content7 = "<!DOCTYPE html>  <html>  <head>  </head>  <body>  <div id=\"theadid\">  <p style=\"text - align: center; \"><span style=\"font - size: 24pt; \">@商铺名称</span></p>  <p style=\"text - align: left; \">供应商：@供应商 &nbsp; 业务员：@业务员 &nbsp; &nbsp; 业务电话：@业务电话 &nbsp; &nbsp;</p>  <p style=\"text - align: left; \">单据编号：@单据编号 &nbsp; &nbsp; 交易日期：@交易日期 &nbsp; &nbsp; &nbsp;仓库：@仓库</p>  </div>  <div id=\"tbodyid\">  <table style=\"width: 720px; \">  <thead>  <tr style=\"height: 20.179px; \">  <td style=\"width: 32px; height: 20.179px; \"><strong>&nbsp;</strong></td>  <td style=\"width: 216.474px; height: 20.179px; \"><strong>商品名称</strong></td>  <td style=\"width: 118.526px; height: 20.179px; \"><strong>条形码</strong></td>  <td style=\"width: 54px; height: 20.179px; \"><strong>单位</strong></td>  <td style=\"width: 81px; height: 20.179px; \"><strong>单位换算</strong></td>  <td style=\"width: 57px; height: 20.179px; \"><strong>数量</strong></td>  <td style=\"width: 75px; height: 20.179px; \"><strong>价格</strong></td>  <td style=\"width: 79px; height: 20.179px; \"><strong>金额</strong></td>  <td style=\"width: 87px; height: 20.179px; \"><strong>备注</strong></td>  </tr>  </thead>  <tbody>  <tr style=\"height: 17px; \">  <td style=\"width: 32px; height: 17px; \">&nbsp;#序号</td>  <td style=\"width: 216.474px; height: 17px; \">  <p>#商品名称</p>  <p>#生产日期</p>  </td>  <td style=\"width: 118.526px; height: 17px; \">#条形码</td>  <td style=\"width: 54px; height: 17px; text - align: center; \">#商品单位</td>  <td style=\"width: 81px; height: 17px; text - align: center; \">#单位换算</td>  <td style=\"width: 57px; height: 17px; text - align: right; \">#数量</td>  <td style=\"width: 75px; height: 17px; text - align: right; \">#价格</td>  <td style=\"width: 79px; height: 17px; text - align: right; \">#金额</td>  <td style=\"width: 87px; height: 17px; \">#备注</td>  </tr>  </tbody>  <tfoot>  <tr style=\"height: 24px; \">  <td style=\"width: 32px; height: 24px; \">&nbsp;</td>  <td style=\"width: 216.474px; height: 24px; \"><strong>总计</strong></td>  <td style=\"width: 118.526px; height: 24px; \">&nbsp;</td>  <td style=\"width: 54px; height: 24px; \">&nbsp;</td>  <td style=\"width: 81px; height: 24px; \">&nbsp;</td>  <td style=\"width: 57px; height: 24px; text - align: right; \">数量:###&nbsp;</td>  <td style=\"width: 75px; height: 24px; \">&nbsp;</td>  <td style=\"width: 79px; height: 24px; text - align: right; \">金额:###</td>  <td style=\"width: 87px; height: 24px; \">&nbsp;</td>  </tr>  </tfoot>  </table>  </div>  <div id=\"tfootid\">  <p>制单：@制单 &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;日期：@日期 &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;&nbsp;</p>  <p>地址：@公司地址 &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;订货电话：@订货电话 &nbsp;</p>  <p>备注：@备注</p>  </div>  </body>  </html>";

                        PrintTemplate printTemplate7 = new PrintTemplate()
                        {
                            StoreId = storeId,
                            TemplateType = 0,
                            BillType = (int)BillTypeEnum.PurchaseBill,
                            Title = CommonHelper.GetEnumDescription(BillTypeEnum.PurchaseBill),
                            Content = content7
                        };
                        _printTemplateService.InsertPrintTemplate(printTemplate7);
                    }

                    #endregion

                    #region 采购退货单

                    if (printTemplates == null || printTemplates.Where(p => p.BillType == (int)BillTypeEnum.PurchaseReturnBill).Count() == 0)
                    {
                        string content8 = "<div id=\"theadid\">  <p style=\"text - align: center; \"><span style=\"font - size: 24pt; \">@商铺名称</span></p>    <p style=\"text - align: left; \">供应商：@供应商 &nbsp; 业务员：@业务员 &nbsp; &nbsp; 业务电话：@业务电话 &nbsp;</p>    <p style=\"text - align: left; \">&nbsp;单据编号：@单据编号 &nbsp; &nbsp;交易日期：@交易日期 &nbsp; &nbsp;仓库：@仓库</p>    </div>    <div id=\"tbodyid\">  <table style=\"width: 720px; \"  class=\"table table-bordered\">  <thead>  <tr>    <td style=\"width: 32px; height: 20px; \"><strong>&nbsp;</strong></td>    <td style=\"width: 213.489px; height: 20px; text - align: center; \"><strong>商品名称</strong></td>    <td style=\"width: 95.5114px; height: 20px; text - align: center; \"><strong>条形码</strong></td>    <td style=\"width: 43px; height: 20px; text - align: center; \"><strong>单位</strong></td>    <td style=\"width: 89px; height: 20px; text - align: center; \"><strong>单位换算</strong></td>    <td style=\"width: 54px; height: 20px; text - align: center; \"><strong>数量</strong></td>    <td style=\"width: 51px; height: 20px; text - align: center; \"><strong>价格</strong></td>    <td style=\"width: 63px; height: 20px; text - align: center; \"><strong>金额</strong></td>    <td style=\"width: 85px; height: 20px; text - align: center; \"><strong>备注</strong></td>    </tr>    </thead>    <tbody>    <tr>    <td style=\"width: 32px; height: 17px; \">&nbsp;#序号</td>    <td style=\"width: 213.489px; height: 17px; \">  <p>#商品名称</p>    <p>#生产日期</p>    </td>    <td style=\"width: 95.5114px; height: 17px; \">#条形码</td>    <td style=\"width: 43px; height: 17px; text - align: center; \">#商品单位</td>    <td style=\"width: 89px; height: 17px; text - align: center; \">#单位换算</td>    <td style=\"width: 54px; height: 17px; text - align: right; \">#数量</td>    <td style=\"width: 51px; height: 17px; text - align: right; \">#价格</td>    <td style=\"width: 63px; height: 17px; text - align: right; \">#金额</td>    <td style=\"width: 85px; height: 17px; \">#备注</td>    </tr>    </tbody>    <tfoot>  <tr>    <td style=\"width: 32px; height: 24px; \">&nbsp;</td>    <td style=\"width: 213.489px; height: 24px; \"><strong>总计</strong></td>    <td style=\"width: 95.5114px; height: 24px; \">&nbsp;</td>    <td style=\"width: 43px; height: 24px; \">&nbsp;</td>    <td style=\"width: 89px; height: 24px; \">&nbsp;</td>    <td style=\"width: 54px; height: 24px; text - align: right; \">&nbsp;数量:###</td>    <td style=\"width: 51px; height: 24px; \">&nbsp;</td>    <td style=\"width: 63px; height: 24px; text - align: right; \">金额:###</td>    <td style=\"width: 85px; height: 24px; \">&nbsp;</td>    </tr>    </tfoot>  </table>    </div>    <div id=\"tfootid\">  <p>制单：@制单 &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;日期：@日期 &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;&nbsp;</p>    <p>备注：@备注&nbsp;</p>    </div>";

                        PrintTemplate printTemplate8 = new PrintTemplate()
                        {
                            StoreId = storeId,
                            TemplateType = 0,
                            BillType = (int)BillTypeEnum.PurchaseReturnBill,
                            Title = CommonHelper.GetEnumDescription(BillTypeEnum.PurchaseReturnBill),
                            Content = content8
                        };
                        _printTemplateService.InsertPrintTemplate(printTemplate8);
                    }

                    #endregion

                    #region 调拨单

                    if (printTemplates == null || printTemplates.Where(p => p.BillType == (int)BillTypeEnum.AllocationBill).Count() == 0)
                    {
                        string content9 = "<div id=\"theadid\">  <p style=\"text - align: center; \"><span style=\"font - size: 24pt; \">@商铺名称</span></p>    <p style=\"text - align: left; \">出货仓库：@出货仓库 &nbsp; &nbsp; 入货仓库：@入货仓库 &nbsp;&nbsp;调拨日期：@调拨日期</p>    <p style=\"text - align: left; \">业务员：@业务员 &nbsp; &nbsp; 业务电话：@业务电话 &nbsp; &nbsp;单据编号：@单据编号 &nbsp;&nbsp;</p>    </div>    <div id=\"tbodyid\">  <table style=\"width: 720px; \"  class=\"table table-bordered\">  <thead>  <tr style=\"height: 20px; \">    <td style=\"width: 32px; height: 20px; text - align: center; \"><strong>&nbsp;</strong></td>    <td style=\"width: 260.6px; height: 20px; text - align: center; \"><strong>商品名称</strong></td>    <td style=\"width: 96.4px; height: 20px; text - align: center; \"><strong>数量</strong></td>    <td style=\"width: 112px; height: 20px; text - align: center; \"><strong>条形码</strong></td>    <td style=\"width: 44px; height: 20px; text - align: center; \"><strong>单位</strong></td>    <td style=\"width: 85px; height: 20px; text - align: center; \"><strong>单位换算</strong></td>    <td style=\"width: 70px; height: 20px; text - align: center; \"><strong>单价</strong></td>    </tr>    </thead>    <tbody>    <tr style=\"height: 17px; \">    <td style=\"width: 32px; height: 17px; \">&nbsp;#序号</td>    <td style=\"width: 260.6px; height: 17px; \">  <p>#商品名称</p>    <p>#生产日期</p>    </td>    <td style=\"width: 96.4px; height: 17px; \">  <p style=\"text - align: right; \">#数量</p>    </td>    <td style=\"width: 112px; height: 17px; \">#条形码</td>    <td style=\"width: 44px; height: 17px; text - align: center; \">#商品单位</td>    <td style=\"width: 85px; height: 17px; text - align: center; \">#单位换算</td>    <td style=\"width: 70px; height: 17px; text - align: right; \">#批发价</td>    </tr>    </tbody>    <tfoot>  <tr style=\"height: 17px; \">    <td style=\"width: 32px; height: 17px; \">&nbsp;</td>    <td style=\"width: 260.6px; height: 17px; \">  <p>合计</p>    </td>    <td style=\"width: 96.4px; height: 17px; \">  <p style=\"text - align: right; \">数量:###</p>    </td>    <td style=\"width: 112px; height: 17px; \">&nbsp;</td>    <td style=\"width: 44px; height: 17px; text - align: center; \">&nbsp;</td>    <td style=\"width: 85px; height: 17px; text - align: center; \">&nbsp;</td>    <td style=\"width: 70px; height: 17px; text - align: right; \">&nbsp;</td>    </tr>    </tfoot>  </table>    </div>    <div id=\"tfootid\">  <p>制单：@制单 &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;日期：@日期 &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; 备注：@备注 &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;</p>    <p>地址：@公司地址 &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;</p>    <p>订货电话：@订货电话 &nbsp;</p>    </div>";

                        PrintTemplate printTemplate9 = new PrintTemplate()
                        {
                            StoreId = storeId,
                            TemplateType = 0,
                            BillType = (int)BillTypeEnum.AllocationBill,
                            Title = CommonHelper.GetEnumDescription(BillTypeEnum.AllocationBill),
                            Content = content9
                        };
                        _printTemplateService.InsertPrintTemplate(printTemplate9);
                    }

                    #endregion

                    #region 盘点盈亏单

                    if (printTemplates == null || printTemplates.Where(p => p.BillType == (int)BillTypeEnum.InventoryProfitLossBill).Count() == 0)
                    {
                        string content10 = "<div id=\"theadid\">  <p style=\"text - align: center; \"><span style=\"font - size: 24pt; \">@商铺名称</span></p>    <p style=\"text - align: left; \">仓库：@仓库 &nbsp; &nbsp; 经办人：@经办人 &nbsp; &nbsp;单据编号：@单据编号</p>    </div>    <div id=\"tbodyid\">  <table style=\"width: 720px; \" class=\"table table-bordered\">    <thead>  <tr>    <td style=\"width: 32px; height: 20px; text - align: center; \"><strong>&nbsp;</strong></td>    <td style=\"width: 201.29px; height: 20px; text - align: center; \"><strong>商品名称</strong></td>    <td style=\"width: 70.7102px; height: 20px; text - align: center; \"><strong>单位</strong></td>    <td style=\"width: 94px; height: 20px; text - align: center; \"><strong>单位换算</strong></td>    <td style=\"width: 56px; height: 20px; text - align: center; \"><strong>数量</strong></td>    <td style=\"width: 54px; height: 20px; text - align: center; \"><strong>成本价</strong></td>    <td style=\"width: 130px; height: 20px; text - align: center; \"><strong>成本金额</strong></td>    </tr>    </thead>    <tbody>    <tr>    <td style=\"width: 32px; height: 17px; \">&nbsp;#序号</td>    <td style=\"width: 201.29px; height: 17px; \">  <p>#商品名称</p>    <p>#生产日期</p>    </td>    <td style=\"width: 70.7102px; height: 17px; text - align: center; \">#商品单位</td>    <td style=\"width: 94px; height: 17px; \">#单位换算</td>    <td style=\"width: 56px; height: 17px; text - align: right; \">#数量</td>    <td style=\"width: 54px; height: 17px; text - align: right; \">#成本价</td>    <td style=\"width: 130px; height: 17px; text - align: right; \">#成本金额</td>    </tr>    </tbody>    <tfoot>  <tr>    <td style=\"width: 32px; height: 24px; \">&nbsp;</td>    <td style=\"width: 201.29px; height: 24px; \"><strong>总计</strong></td>    <td style=\"width: 70.7102px; height: 24px; \">&nbsp;</td>    <td style=\"width: 94px; height: 24px; \">&nbsp;</td>    <td style=\"width: 56px; height: 24px; \">&nbsp;</td>    <td style=\"width: 54px; height: 24px; \">&nbsp;</td>    <td style=\"width: 130px; height: 24px; text - align: right; \">&nbsp;金额:###</td>    </tr>    </tfoot>  </table>    </div>    <div id=\"tfootid\">  <p>制单：@制单 &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;日期：@日期 &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;</p>    <p>地址：@公司地址 &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;&nbsp;订货电话：@订货电话 &nbsp;</p>    <p>备注：@备注 &nbsp; &nbsp; &nbsp;</p>    </div>";

                        PrintTemplate printTemplate10 = new PrintTemplate()
                        {
                            StoreId = storeId,
                            TemplateType = 0,
                            BillType = (int)BillTypeEnum.InventoryProfitLossBill,
                            Title = CommonHelper.GetEnumDescription(BillTypeEnum.InventoryProfitLossBill),
                            Content = content10
                        };
                        _printTemplateService.InsertPrintTemplate(printTemplate10);
                    }

                    #endregion

                    #region 成本调价单

                    if (printTemplates == null || printTemplates.Where(p => p.BillType == (int)BillTypeEnum.CostAdjustmentBill).Count() == 0)
                    {
                        string content11 = "<div id=\"theadid\">  <p style=\"text - align: center; \"><span style=\"font - size: 24pt; \">@商铺名称</span></p>    <p style=\"text - align: left; \">仓库：@仓库 &nbsp; &nbsp; 经办人：@经办人 &nbsp; &nbsp;单据编号：@单据编号</p>    </div>    <div id=\"tbodyid\">  <table style=\"width: 720px; \" class=\"table table-bordered\">    <thead>  <tr>    <td style=\"width: 32px; height: 20px; text - align: center; \"><strong>&nbsp;</strong></td>    <td style=\"width: 201.29px; height: 20px; text - align: center; \"><strong>商品名称</strong></td>    <td style=\"width: 70.7102px; height: 20px; text - align: center; \"><strong>单位</strong></td>    <td style=\"width: 94px; height: 20px; text - align: center; \"><strong>单位换算</strong></td>    <td style=\"width: 56px; height: 20px; text - align: center; \"><strong>数量</strong></td>    <td style=\"width: 54px; height: 20px; text - align: center; \"><strong>成本价</strong></td>    <td style=\"width: 130px; height: 20px; text - align: center; \"><strong>成本金额</strong></td>    </tr>    </thead>    <tbody>    <tr>    <td style=\"width: 32px; height: 17px; \">&nbsp;#序号</td>    <td style=\"width: 201.29px; height: 17px; \">  <p>#商品名称</p>    <p>#生产日期</p>    </td>    <td style=\"width: 70.7102px; height: 17px; text - align: center; \">#商品单位</td>    <td style=\"width: 94px; height: 17px; \">#单位换算</td>    <td style=\"width: 56px; height: 17px; text - align: right; \">#数量</td>    <td style=\"width: 54px; height: 17px; text - align: right; \">#成本价</td>    <td style=\"width: 130px; height: 17px; text - align: right; \">#成本金额</td>    </tr>    </tbody>    <tfoot>  <tr>    <td style=\"width: 32px; height: 24px; \">&nbsp;</td>    <td style=\"width: 201.29px; height: 24px; \"><strong>总计</strong></td>    <td style=\"width: 70.7102px; height: 24px; \">&nbsp;</td>    <td style=\"width: 94px; height: 24px; \">&nbsp;</td>    <td style=\"width: 56px; height: 24px; \">&nbsp;</td>    <td style=\"width: 54px; height: 24px; \">&nbsp;</td>    <td style=\"width: 130px; height: 24px; text - align: right; \">&nbsp;金额:###</td>    </tr>    </tfoot>  </table>    </div>    <div id=\"tfootid\">  <p>制单：@制单 &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;日期：@日期 &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;</p>    <p>地址：@公司地址 &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;&nbsp;订货电话：@订货电话 &nbsp;</p>    <p>备注：@备注 &nbsp; &nbsp; &nbsp;</p>    </div>";

                        PrintTemplate printTemplate11 = new PrintTemplate()
                        {
                            StoreId = storeId,
                            TemplateType = 0,
                            BillType = (int)BillTypeEnum.CostAdjustmentBill,
                            Title = CommonHelper.GetEnumDescription(BillTypeEnum.CostAdjustmentBill),
                            Content = content11
                        };
                        _printTemplateService.InsertPrintTemplate(printTemplate11);
                    }

                    #endregion

                    #region 报损单

                    if (printTemplates == null || printTemplates.Where(p => p.BillType == (int)BillTypeEnum.ScrapProductBill).Count() == 0)
                    {
                        string content12 = "<div id=\"theadid\">  <p style=\"text - align: center; \"><span style=\"font - size: 24pt; \">@商铺名称</span></p>    <p style=\"text - align: left; \">仓库：@仓库 &nbsp; &nbsp; 经办人：@经办人 &nbsp; &nbsp;单据编号：@单据编号</p>    </div>    <div id=\"tbodyid\">  <table style=\"width: 722px; \" class=\"table table-bordered\">    <thead>  <tr>    <td style=\"width: 32px; height: 20px; \"><strong>&nbsp;</strong></td>    <td style=\"width: 214px; height: 20px; \"><strong>商品名称</strong></td>    <td style=\"width: 52px; height: 20px; \"><strong>单位</strong></td>    <td style=\"width: 91px; height: 20px; \"><strong>单位换算</strong></td>    <td style=\"width: 66px; height: 20px; \"><strong>数量</strong></td>    <td style=\"width: 69px; height: 20px; \"><strong>成本价</strong></td>    <td style=\"width: 115px; height: 20px; \"><strong>成本金额</strong></td>    </tr>    </thead>    <tbody>    <tr>    <td style=\"width: 32px; height: 17px; \">&nbsp;#序号</td>    <td style=\"width: 214px; height: 17px; \">  <p>#商品名称</p>    <p>#生产日期</p>    </td>    <td style=\"width: 52px; height: 17px; text - align: center; \">#商品单位</td>    <td style=\"width: 91px; height: 17px; text - align: center; \">#单位换算</td>    <td style=\"width: 66px; height: 17px; text - align: right; \">#数量</td>    <td style=\"width: 69px; height: 17px; text - align: right; \">#成本价</td>    <td style=\"width: 115px; height: 17px; text - align: right; \">#成本金额</td>    </tr>    </tbody>    <tfoot>  <tr>    <td style=\"width: 32px; height: 24px; \">&nbsp;</td>    <td style=\"width: 214px; height: 24px; \"><strong>总计</strong></td>    <td style=\"width: 52px; height: 24px; \">&nbsp;</td>    <td style=\"width: 91px; height: 24px; \">&nbsp;</td>    <td style=\"width: 66px; height: 24px; text - align: right; \">数量:###&nbsp;</td>    <td style=\"width: 69px; height: 24px; \">&nbsp;</td>    <td style=\"width: 115px; height: 24px; text - align: right; \">&nbsp;金额:###</td>    </tr>    </tfoot>  </table>    </div>    <div id=\"tfootid\">  <p>制单：@制单 &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;日期：@日期 &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; 备注：@备注 &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;</p>    <p>地址：@公司地址 &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;</p>    <p>订货电话：@订货电话 &nbsp;</p>    </div>";

                        PrintTemplate printTemplate12 = new PrintTemplate()
                        {
                            StoreId = storeId,
                            TemplateType = 0,
                            BillType = (int)BillTypeEnum.ScrapProductBill,
                            Title = CommonHelper.GetEnumDescription(BillTypeEnum.ScrapProductBill),
                            Content = content12
                        };
                        _printTemplateService.InsertPrintTemplate(printTemplate12);
                    }

                    #endregion

                    #region 盘点任务（整仓）

                    if (printTemplates == null || printTemplates.Where(p => p.BillType == (int)BillTypeEnum.InventoryAllTaskBill).Count() == 0)
                    {
                        string content13 = "<div id=\"theadid\">  <p style=\"text - align: center; \"><span style=\"font - size: 24pt; \">@商铺名称</span></p>    <p style=\"text - align: left; \">仓库：@仓库 &nbsp; &nbsp;&nbsp;盘点人员：@盘点人员</p>    <p style=\"text - align: left; \">业务员：@业务员 &nbsp; &nbsp; 业务电话：@业务电话 &nbsp; &nbsp;单据编号：@单据编号</p>    </div>    <div id=\"tbodyid\">  <table style=\"width: 727px; \"  class=\"table table-bordered\">  <thead>  <tr>    <td style=\"width: 32px; height: 20px; text - align: center; \">&nbsp;</td>    <td style=\"width: 187px; height: 20px; text - align: center; \">商品名称</td>    <td style=\"width: 91px; height: 20px; text - align: center; \">单位换算</td>    <td style=\"width: 94px; height: 20px; text - align: center; \">当前库存数量</td>    <td style=\"width: 102px; height: 20px; text - align: center; \">盘点库存数量量</td>    <td style=\"width: 68px; height: 20px; text - align: center; \">盘盈数量</td>    <td style=\"width: 84px; height: 20px; text - align: center; \">盘亏数量</td>    </tr>    </thead>    <tbody>    <tr>    <td style=\"width: 32px; height: 17px; \">&nbsp;#序号</td>    <td style=\"width: 187px; height: 17px; \">  <p>#商品名称</p>    <p>#生产日期</p>    </td>    <td style=\"width: 91px; height: 17px; text - align: center; \">#单位换算</td>    <td style=\"width: 94px; height: 17px; text - align: right; \">#当前库存数量</td>    <td style=\"width: 102px; height: 17px; text - align: right; \">#盘点库存数量</td>    <td style=\"width: 68px; height: 17px; text - align: right; \">#盘盈数量</td>    <td style=\"width: 84px; height: 17px; text - align: right; \">#盘亏数量</td>    </tr>    </tbody>    </table>    </div>    <div id=\"tfootid\">  <p>制单：@制单 &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;日期：@日期 &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; 备注：@备注 &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;</p>    <p>地址：@公司地址 &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;</p>    <p>订货电话：@订货电话 &nbsp;</p>    </div>";

                        PrintTemplate printTemplate13 = new PrintTemplate()
                        {
                            StoreId = storeId,
                            TemplateType = 0,
                            BillType = (int)BillTypeEnum.InventoryAllTaskBill,
                            Title = CommonHelper.GetEnumDescription(BillTypeEnum.InventoryAllTaskBill),
                            Content = content13
                        };
                        _printTemplateService.InsertPrintTemplate(printTemplate13);
                    }

                    #endregion

                    #region 盘点任务（部分）

                    if (printTemplates == null || printTemplates.Where(p => p.BillType == (int)BillTypeEnum.InventoryPartTaskBill).Count() == 0)
                    {
                        string content14 = "<div id=\"theadid\">  <p style=\"text - align: center; \"><span style=\"font - size: 24pt; \">@商铺名称</span></p>    <p style=\"text - align: left; \">仓库：@仓库 &nbsp; &nbsp;&nbsp;盘点人员：@盘点人员</p>    <p style=\"text - align: left; \">业务员：@业务员 &nbsp; &nbsp; 业务电话：@业务电话 &nbsp; &nbsp;单据编号：@单据编号</p>    </div>    <div id=\"tbodyid\">  <table style=\"width: 727px; \"  class=\"table table-bordered\">  <thead>  <tr>    <td style=\"width: 32px; height: 20px; text - align: center; \">&nbsp;</td>    <td style=\"width: 187px; height: 20px; text - align: center; \">商品名称</td>    <td style=\"width: 91px; height: 20px; text - align: center; \">单位换算</td>    <td style=\"width: 94px; height: 20px; text - align: center; \">当前库存数量</td>    <td style=\"width: 102px; height: 20px; text - align: center; \">盘点库存数量量</td>    <td style=\"width: 68px; height: 20px; text - align: center; \">盘盈数量</td>    <td style=\"width: 84px; height: 20px; text - align: center; \">盘亏数量</td>    </tr>    </thead>    <tbody>    <tr>    <td style=\"width: 32px; height: 17px; \">&nbsp;#序号</td>    <td style=\"width: 187px; height: 17px; \">  <p>#商品名称</p>    <p>#生产日期</p>    </td>    <td style=\"width: 91px; height: 17px; text - align: center; \">#单位换算</td>    <td style=\"width: 94px; height: 17px; text - align: right; \">#当前库存数量</td>    <td style=\"width: 102px; height: 17px; text - align: right; \">#盘点库存数量</td>    <td style=\"width: 68px; height: 17px; text - align: right; \">#盘盈数量</td>    <td style=\"width: 84px; height: 17px; text - align: right; \">#盘亏数量</td>    </tr>    </tbody>    </table>    </div>    <div id=\"tfootid\">  <p>制单：@制单 &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;日期：@日期 &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; 备注：@备注 &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;</p>    <p>地址：@公司地址 &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;</p>    <p>订货电话：@订货电话 &nbsp;</p>    </div>";

                        PrintTemplate printTemplate14 = new PrintTemplate()
                        {
                            StoreId = storeId,
                            TemplateType = 0,
                            BillType = (int)BillTypeEnum.InventoryPartTaskBill,
                            Title = CommonHelper.GetEnumDescription(BillTypeEnum.InventoryPartTaskBill),
                            Content = content14
                        };
                        _printTemplateService.InsertPrintTemplate(printTemplate14);
                    }

                    #endregion

                    #region 组合单

                    if (printTemplates == null || printTemplates.Where(p => p.BillType == (int)BillTypeEnum.CombinationProductBill).Count() == 0)
                    {
                        string content15 = "<div id=\"theadid\">  <p style=\"text - align: center; \"><span style=\"font - size: 24pt; \">@商铺名称</span></p>    <p style=\"text - align: left; \">仓库：@仓库 &nbsp; 业务员：@业务员 &nbsp; &nbsp; 主商品：@主商品 &nbsp; 数量：@数量 &nbsp;单据编号：@单据编号</p>    </div>    <div id=\"tbodyid\">  <table style=\"width: 729px; \" class=\"table table-bordered\">    <thead>  <tr>    <td style=\"width: 32px; height: 20px; \"><strong>&nbsp;</strong></td>    <td style=\"width: 153px; height: 20px; \"><strong>商品名称</strong></td>    <td style=\"width: 105px; height: 20px; \"><strong>子商品/主商品</strong></td>    <td style=\"width: 58px; height: 20px; \"><strong>生产日期</strong></td>    <td style=\"width: 56px; height: 20px; \"><strong>数量</strong></td>    </tr>    </thead>    <tbody>    <tr>    <td style=\"width: 32px; height: 17px; \">&nbsp;#序号</td>    <td style=\"width: 153px; height: 17px; \">#商品名称</td>    <td style=\"width: 105px; height: 17px; \">#子商品/主商品</td>    <td style=\"width: 58px; height: 17px; \">#生产日期</td>    <td style=\"width: 56px; height: 17px; \">#数量</td>    </tr>    </tbody>    </table>    </div>    <div id=\"tfootid\">  <p>制单：@制单 &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;日期：@日期 &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;</p>    <p>地址：@公司地址 &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;订货电话：@订货电话 &nbsp;&nbsp;</p>    <p>备注：@备注</p>    <p>&nbsp;</p>    </div>";

                        PrintTemplate printTemplate15 = new PrintTemplate()
                        {
                            StoreId = storeId,
                            TemplateType = 0,
                            BillType = (int)BillTypeEnum.CombinationProductBill,
                            Title = CommonHelper.GetEnumDescription(BillTypeEnum.CombinationProductBill),
                            Content = content15
                        };
                        _printTemplateService.InsertPrintTemplate(printTemplate15);
                    }

                    #endregion

                    #region 拆分单

                    if (printTemplates == null || printTemplates.Where(p => p.BillType == (int)BillTypeEnum.SplitProductBill).Count() == 0)
                    {
                        string content16 = "<div id=\"theadid\">  <p style=\"text - align: center; \"><span style=\"font - size: 24pt; \">@商铺名称</span></p>    <p style=\"text - align: left; \">仓库：@仓库 &nbsp; 业务员：@业务员 &nbsp; &nbsp; 主商品：@主商品 &nbsp; 数量：@数量 &nbsp;单据编号：@单据编号</p>    </div>    <div id=\"tbodyid\">  <table style=\"width: 729px; \" class=\"table table-bordered\">    <thead>  <tr>    <td style=\"width: 32px; height: 20px; \"><strong>&nbsp;</strong></td>    <td style=\"width: 153px; height: 20px; \"><strong>商品名称</strong></td>    <td style=\"width: 105px; height: 20px; \"><strong>子商品/主商品</strong></td>    <td style=\"width: 58px; height: 20px; \"><strong>生产日期</strong></td>    <td style=\"width: 56px; height: 20px; \"><strong>数量</strong></td>    </tr>    </thead>    <tbody>    <tr>    <td style=\"width: 32px; height: 17px; \">&nbsp;#序号</td>    <td style=\"width: 153px; height: 17px; \">#商品名称</td>    <td style=\"width: 105px; height: 17px; \">#子商品/主商品</td>    <td style=\"width: 58px; height: 17px; \">#生产日期</td>    <td style=\"width: 56px; height: 17px; \">#数量</td>    </tr>    </tbody>    </table>    </div>    <div id=\"tfootid\">  <p>制单：@制单 &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;日期：@日期 &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;</p>    <p>地址：@公司地址 &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;订货电话：@订货电话 &nbsp;&nbsp;</p>    <p>备注：@备注</p>    <p>&nbsp;</p>    </div>";

                        PrintTemplate printTemplate16 = new PrintTemplate()
                        {
                            StoreId = storeId,
                            TemplateType = 0,
                            BillType = (int)BillTypeEnum.SplitProductBill,
                            Title = CommonHelper.GetEnumDescription(BillTypeEnum.SplitProductBill),
                            Content = content16
                        };
                        _printTemplateService.InsertPrintTemplate(printTemplate16);
                    }

                    #endregion

                    #region 收款单

                    if (printTemplates == null || printTemplates.Where(p => p.BillType == (int)BillTypeEnum.CashReceiptBill).Count() == 0)
                    {
                        string content17 = "<div id=\"theadid\">  <p style=\"text - align: center; \"><span style=\"font - size: 24pt; \">@商铺名称</span></p>    <p style=\"text - align: left; \">客户：@客户名称 &nbsp; 业务员：@业务员 &nbsp; &nbsp; 业务电话：@业务电话 &nbsp; &nbsp;单据编号：@单据编号</p>    </div>    <div id=\"tbodyid\">  <table style=\"width: 728px; \" class=\"table table-bordered\">    <thead>  <tr style=\"height: 20px; \">    <td style=\"width: 32px; height: 20px; \"><strong>&nbsp;</strong></td>    <td style=\"width: 125px; height: 20px; text - align: center; \"><strong>单据编号</strong></td>    <td style=\"width: 82px; height: 20px; text - align: center; \"><strong>单据类型</strong></td>    <td style=\"width: 110px; height: 20px; text - align: center; \"><strong>开单时间</strong></td>    <td style=\"width: 122px; height: 20px; text - align: center; \"><strong>单据金额</strong></td>    <td style=\"width: 77px; height: 20px; text - align: center; \"><strong>已收</strong></td>    <td style=\"width: 88px; height: 20px; text - align: center; \"><strong>尚欠</strong></td>    <td style=\"width: 93px; height: 20px; text - align: center; \"><strong>本次收款</strong></td>    </tr>    </thead>    <tbody>    <tr style=\"height: 17px; \">    <td style=\"width: 32px; height: 17px; \">&nbsp;#序号</td>    <td style=\"width: 125px; height: 17px; \">#单据编号</td>    <td style=\"width: 82px; height: 17px; \">#单据类型</td>    <td style=\"width: 110px; height: 17px; \">#开单时间</td>    <td style=\"width: 122px; height: 17px; text - align: right; \">#单据金额</td>    <td style=\"width: 77px; height: 17px; text - align: right; \">#已收金额</td>    <td style=\"width: 88px; height: 17px; text - align: right; \">#尚欠金额</td>    <td style=\"width: 93px; height: 17px; text - align: right; \">#本次收款金额</td>    </tr>    </tbody>    <tfoot>  <tr style=\"height: 24px; \">    <td style=\"width: 32px; height: 24px; \">&nbsp;</td>    <td style=\"width: 125px; height: 24px; \"><strong>总计</strong></td>    <td style=\"width: 82px; height: 24px; \">&nbsp;</td>    <td style=\"width: 110px; height: 24px; \">&nbsp;</td>    <td style=\"width: 122px; height: 24px; text - align: right; \">&nbsp;单据金额:###</td>    <td style=\"width: 77px; height: 24px; text - align: right; \">&nbsp;已收金额:###</td>    <td style=\"width: 88px; height: 24px; text - align: right; \">尚欠金额:###</td>    <td style=\"width: 93px; height: 24px; text - align: right; \">本次收款金额:###</td>    </tr>    </tfoot>  </table>    </div>    <div id=\"tfootid\">  <p>制单：@制单 &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;日期：@日期 &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; 备注：@备注 &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;</p>    <p>地址：@公司地址 &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;</p>    <p>订货电话：@订货电话 &nbsp;</p>    </div>";

                        PrintTemplate printTemplate17 = new PrintTemplate()
                        {
                            StoreId = storeId,
                            TemplateType = 0,
                            BillType = (int)BillTypeEnum.CashReceiptBill,
                            Title = CommonHelper.GetEnumDescription(BillTypeEnum.CashReceiptBill),
                            Content = content17
                        };
                        _printTemplateService.InsertPrintTemplate(printTemplate17);
                    }

                    #endregion

                    #region 付款单

                    if (printTemplates == null || printTemplates.Where(p => p.BillType == (int)BillTypeEnum.PaymentReceiptBill).Count() == 0)
                    {
                        string content18 = "<!DOCTYPE html>  <html>  <head>  </head>  <body>  <div id=\"theadid\">  <p style=\"text - align: center; \"><span style=\"font - size: 24pt; \">@商铺名称</span></p>  <p style=\"text - align: left; \">客户：@客户名称 &nbsp; 业务员：@业务员 &nbsp; &nbsp; 业务电话：@业务电话 &nbsp; &nbsp;单据编号：@单据编号</p>  </div>  <div id=\"tbodyid\">  <table style=\"width: 720px; \">  <thead>  <tr>  <td style=\"width: 32px; height: 20px; text - align: center; \"><strong>&nbsp;</strong></td>  <td style=\"width: 123px; height: 20px; text - align: center; \"><strong>单据编号</strong></td>  <td style=\"width: 78px; height: 20px; text - align: center; \"><strong>单据类型</strong></td>  <td style=\"width: 104.23px; height: 20px; text - align: center; \"><strong>开单时间</strong></td>  <td style=\"width: 82.7699px; height: 20px; text - align: center; \"><strong>单据金额</strong></td>  <td style=\"width: 77px; height: 20px; text - align: center; \"><strong>已付</strong></td>  <td style=\"width: 85px; height: 20px; text - align: center; \"><strong>尚欠</strong></td>  <td style=\"width: 134px; height: 20px; text - align: center; \"><strong>本次付款</strong></td>  </tr>  </thead>  <tbody>  <tr>  <td style=\"width: 32px; height: 17px; \">&nbsp;#序号</td>  <td style=\"width: 123px; height: 17px; \">#单据编号</td>  <td style=\"width: 78px; height: 17px; \">#单据类型</td>  <td style=\"width: 104.23px; height: 17px; \">#开单时间</td>  <td style=\"width: 82.7699px; height: 17px; text - align: right; \">#单据金额</td>  <td style=\"width: 77px; height: 17px; text - align: right; \">#已收金额</td>  <td style=\"width: 85px; height: 17px; text - align: right; \">#尚欠金额</td>  <td style=\"width: 134px; height: 17px; text - align: right; \">#本次收款金额</td>  </tr>  </tbody>  <tfoot>  <tr>  <td style=\"width: 32px; height: 24px; \">&nbsp;</td>  <td style=\"width: 123px; height: 24px; \"><strong>总计</strong></td>  <td style=\"width: 78px; height: 24px; \">&nbsp;</td>  <td style=\"width: 104.23px; height: 24px; \">&nbsp;</td>  <td style=\"width: 82.7699px; height: 24px; \">单据金额:###</td>  <td style=\"width: 77px; height: 24px; text - align: right; \">已收金额:###</td>  <td style=\"width: 85px; height: 24px; text - align: right; \">尚欠金额:###</td>  <td style=\"width: 134px; height: 24px; text - align: right; \">本次收款金额:###</td>  </tr>  </tfoot>  </table>  </div>  <div id=\"tfootid\">  <p>制单：@制单 &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;日期：@日期 &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; 备注：@备注 &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;</p>  <p>地址：@公司地址 &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;</p>  <p>订货电话：@订货电话 &nbsp;</p>  </div>  </body>  </html>";

                        PrintTemplate printTemplate18 = new PrintTemplate()
                        {
                            StoreId = storeId,
                            TemplateType = 0,
                            BillType = (int)BillTypeEnum.PaymentReceiptBill,
                            Title = CommonHelper.GetEnumDescription(BillTypeEnum.PaymentReceiptBill),
                            Content = content18
                        };
                        _printTemplateService.InsertPrintTemplate(printTemplate18);
                    }

                    #endregion

                    #region 预收款

                    if (printTemplates == null || printTemplates.Where(p => p.BillType == (int)BillTypeEnum.AdvanceReceiptBill).Count() == 0)
                    {
                        string content19 = "<div id=\"theadid\">  <p style=\"text - align: center; \"><span style=\"font - size: 24pt; \">@商铺名称</span></p>    </div>    <div id=\"tbodyid\">  <table style=\"width: 720px; \" class=\"table table-bordered\">    <thead>  <tr>    <td style=\"width: 63px; height: 20px; \">客户</td>    <td style=\"width: 93px; height: 20px; \">#客户</td>    <td style=\"width: 69px; height: 20px; \">业务员</td>    <td style=\"width: 106.011px; height: 20px; \">#业务员</td>    <td style=\"width: 96.9886px; height: 20px; \">收款日期</td>    <td style=\"width: 112.727px; height: 20px; \" colspan=\"2\">#收款日期</td>    <td style=\"width: 65px; height: 20px; \">预收款</td>    <td style=\"width: 125px; height: 20px; \">#预收款账户</td>    </tr>    </thead>    <tbody>    <tr>    <td style=\"width: 63px; height: 17px; \">&nbsp;</td>    <td style=\"width: 93px; height: 17px; \">&nbsp;</td>    <td style=\"width: 69px; height: 17px; \">预收款金额</td>    <td style=\"width: 106.011px; height: 17px; \">#预收款金额</td>    <td style=\"width: 96.9886px; height: 17px; \">备注</td>    <td style=\"width: 302.727px; height: 17px; \" colspan=\"4\">&nbsp;#备注</td>    </tr>    </tbody>    </table>    </div>    <div id=\"tfootid\">  <p>&nbsp;</p>    [优惠：@优惠金额 ]&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;支付信息：@支付信息&nbsp;</div>";

                        PrintTemplate printTemplate19 = new PrintTemplate()
                        {
                            StoreId = storeId,
                            TemplateType = 0,
                            BillType = (int)BillTypeEnum.AdvanceReceiptBill,
                            Title = CommonHelper.GetEnumDescription(BillTypeEnum.AdvanceReceiptBill),
                            Content = content19
                        };
                        _printTemplateService.InsertPrintTemplate(printTemplate19);
                    }

                    #endregion

                    #region 预付款

                    if (printTemplates == null || printTemplates.Where(p => p.BillType == (int)BillTypeEnum.AdvancePaymentBill).Count() == 0)
                    {
                        string content20 = "<!DOCTYPE html>  <html>  <head>  </head>  <body>  <div id=\"theadid\">  <p style=\"text - align: center; \"><span style=\"font - size: 24pt; \">@商铺名称</span></p>  </div>  <div id=\"tbodyid\">  <table style=\"width: 720px; \">  <thead>  <tr style=\"height: 20.0881px; \">  <td style=\"width: 56.821px; height: 20.0881px; \">&nbsp;供应商</td>  <td style=\"width: 127.179px; height: 20.0881px; \">#供应商</td>  <td style=\"width: 62px; height: 20.0881px; \">业务员</td>  <td style=\"width: 102px; height: 20.0881px; \">#业务员</td>  <td style=\"width: 87px; height: 20.0881px; \">付款日期</td>  <td style=\"width: 114.545px; height: 20.0881px; \" colspan=\"2\">#付款日期</td>  <td style=\"width: 50px; height: 20.0881px; \">预付款</td>  <td style=\"width: 131px; height: 20.0881px; \">#预付款</td>  </tr>  </thead>  <tbody>  <tr style=\"height: 17px; \">  <td style=\"width: 56.821px; height: 17px; \">付款账户</td>  <td style=\"width: 127.179px; height: 17px; \">#付款账户</td>  <td style=\"width: 62px; height: 17px; \">付款金额</td>  <td style=\"width: 102px; height: 17px; \">#付款金额</td>  <td style=\"width: 87px; height: 17px; \">备注</td>  <td style=\"width: 295.545px; height: 17px; \" colspan=\"4\">&nbsp;#备注</td>  </tr>  </tbody>  </table>  </div>  <div id=\"tfootid\">&nbsp;</div>  </body>  </html>";

                        PrintTemplate printTemplate20 = new PrintTemplate()
                        {
                            StoreId = storeId,
                            TemplateType = 0,
                            BillType = (int)BillTypeEnum.AdvancePaymentBill,
                            Title = CommonHelper.GetEnumDescription(BillTypeEnum.AdvancePaymentBill),
                            Content = content20
                        };
                        _printTemplateService.InsertPrintTemplate(printTemplate20);
                    }

                    #endregion

                    #region 费用支出单

                    if (printTemplates == null || printTemplates.Where(p => p.BillType == (int)BillTypeEnum.CostExpenditureBill).Count() == 0)
                    {
                        string content21 = "<div id=\"theadid\">  <p style=\"text - align: center; \"><span style=\"font - size: 32px; \">费用支出单</span></p>    <p style=\"text - align: left; \">&nbsp;业务员：@业务员 &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;打印时间：@打印时间 &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;单据编号：@单据编号 &nbsp; &nbsp;&nbsp;</p>    </div>    <div id=\"tbodyid\">  <table style=\"width: 728px; \" class=\"table table-bordered\">    <thead>  <tr style=\"height: 20.2px; \">    <td style=\"width: 32px; height: 20.2px; text - align: center; \"><strong>&nbsp;</strong></td>    <td style=\"width: 256px; height: 20.2px; text - align: center; \">费用类别</td>    <td style=\"width: 150px; height: 20.2px; text - align: center; \">金额</td>    <td style=\"width: 147px; height: 20.2px; text - align: center; \">客户</td>    <td style=\"width: 121px; height: 20.2px; text - align: center; \">备注</td>    </tr>    </thead>    <tbody>    <tr style=\"height: 17px; \">    <td style=\"width: 32px; height: 17px; text - align: left; \">&nbsp;#序号</td>    <td style=\"width: 256px; height: 17px; text - align: center; \">#费用类别</td>    <td style=\"width: 150px; height: 17px; text - align: right; \">#金额</td>    <td style=\"width: 147px; height: 17px; text - align: center; \">#客户</td>    <td style=\"width: 121px; height: 17px; \">#备注</td>    </tr>    </tbody>    <tfoot>  <tr style=\"height: 24px; \">    <td style=\"width: 32px; height: 24px; \">&nbsp;</td>    <td style=\"width: 256px; height: 24px; \">总计</td>    <td style=\"width: 150px; height: 24px; text - align: right; \">&nbsp;@合计</td>    <td style=\"width: 147px; height: 24px; text - align: right; \">&nbsp;</td>    <td style=\"width: 121px; height: 24px; \">&nbsp;</td>    </tr>    </tfoot>  </table>    </div>    <div id=\"tfootid\">  <p>付款日期：@付款日期 &nbsp; &nbsp; &nbsp; &nbsp; 付款方式：@付款方式 &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;支出金额：@支出金额 &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; 日期：@日期</p>    <p>制单：@制单 &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; 备注：@备注 &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;审核人：@审核人</p>    </div>";

                        PrintTemplate printTemplate21 = new PrintTemplate()
                        {
                            StoreId = storeId,
                            TemplateType = 0,
                            BillType = (int)BillTypeEnum.CostExpenditureBill,
                            Title = CommonHelper.GetEnumDescription(BillTypeEnum.CostExpenditureBill),
                            Content = content21
                        };
                        _printTemplateService.InsertPrintTemplate(printTemplate21);
                    }

                    #endregion

                    #region 其他收入

                    if (printTemplates == null || printTemplates.Where(p => p.BillType == (int)BillTypeEnum.FinancialIncomeBill).Count() == 0)
                    {
                        string content22 = "<div id=\"theadid\">  <p style=\"text - align: center; \"><span style=\"font - size: 32px; \">其他收入</span></p>    <p style=\"text - align: left; \">&nbsp;业务员：@业务员 &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;打印时间：@打印时间 &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;单据编号：@单据编号 &nbsp; &nbsp;&nbsp;</p>    </div>    <div id=\"tbodyid\">  <table style=\"width: 728px; \" class=\"table table-bordered\">    <thead>  <tr style=\"height: 20.2px; \">    <td style=\"width: 32px; height: 20.2px; text - align: center; \"><strong>&nbsp;</strong></td>    <td style=\"width: 256px; height: 20.2px; text - align: center; \">收入类别</td>    <td style=\"width: 150px; height: 20.2px; text - align: center; \">金额</td>    <td style=\"width: 147px; height: 20.2px; text - align: center; \">客户/供应商</td>    <td style=\"width: 121px; height: 20.2px; text - align: center; \">备注</td>    </tr>    </thead>    <tbody>    <tr style=\"height: 17px; \">    <td style=\"width: 32px; height: 17px; text - align: left; \">&nbsp;#序号</td>    <td style=\"width: 256px; height: 17px; text - align: center; \">#收入类别</td>    <td style=\"width: 150px; height: 17px; text - align: right; \">#金额</td>    <td style=\"width: 147px; height: 17px; text - align: center; \">#客户/供应商</td>    <td style=\"width: 121px; height: 17px; \">#备注</td>    </tr>    </tbody>    <tfoot>  <tr style=\"height: 24px; \">    <td style=\"width: 32px; height: 24px; \">&nbsp;</td>    <td style=\"width: 256px; height: 24px; \">总计</td>    <td style=\"width: 150px; height: 24px; text - align: right; \">&nbsp;@合计</td>    <td style=\"width: 147px; height: 24px; text - align: right; \">&nbsp;</td>    <td style=\"width: 121px; height: 24px; \">&nbsp;</td>    </tr>    </tfoot>  </table>    </div>    <div id=\"tfootid\">  <p>付款日期：@付款日期 &nbsp; &nbsp; &nbsp; &nbsp; 付款方式：@付款方式 &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;收入金额：@收入金额 &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; 日期：@日期</p>    <p>制单：@制单 &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; 备注：@备注 &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;审核人：@审核人</p>    </div>";

                        PrintTemplate printTemplate22 = new PrintTemplate()
                        {
                            StoreId = storeId,
                            TemplateType = 0,
                            BillType = (int)BillTypeEnum.FinancialIncomeBill,
                            Title = CommonHelper.GetEnumDescription(BillTypeEnum.FinancialIncomeBill),
                            Content = content22
                        };
                        _printTemplateService.InsertPrintTemplate(printTemplate22);
                    }

                    #endregion

                    #region 整车装车单

                    if (printTemplates == null || printTemplates.Where(p => p.BillType == (int)BillTypeEnum.AllLoadBill).Count() == 0)
                    {
                        string content23 = "<div id=\"theadid\">  <p style=\"text - align: center; \"><span style=\"font - size: 24pt; \">@商铺名称</span></p>    <p style=\"text - align: left; \">仓库：@仓库 &nbsp; 车辆：@车辆 &nbsp; &nbsp; 业务员：@业务员</p>    <p style=\"text - align: left; \">订单编号：@订单编号</p>    </div>    <div id=\"tbodyid\">  <table style=\"width: 729px; \" class=\"table table-bordered\">    <thead>  <tr style=\"height: 20px; \">    <td style=\"width: 32px; height: 20px; \"><strong>&nbsp;</strong></td>    <td style=\"width: 300px; height: 20px; \"><strong>商品名称</strong></td>    <td style=\"width: 105px; height: 20px; \"><strong>数量</strong></td>    </tr>    </thead>    <tbody>    <tr style=\"height: 17px; \">    <td style=\"width: 32px; height: 17px; \">&nbsp;#序号</td>    <td style=\"width: 300px; height: 17px; \">#商品名称</td>    <td style=\"width: 105px; height: 17px; \">#数量</td>    </tr>    </tbody>    </table>    </div>    <div id=\"tfootid\">&nbsp;</div>";

                        PrintTemplate printTemplate23 = new PrintTemplate()
                        {
                            StoreId = storeId,
                            TemplateType = 0,
                            BillType = (int)BillTypeEnum.AllLoadBill,
                            Title = CommonHelper.GetEnumDescription(BillTypeEnum.AllLoadBill),
                            Content = content23
                        };
                        _printTemplateService.InsertPrintTemplate(printTemplate23);
                    }

                    #endregion

                    #region 拆零装车单

                    if (printTemplates == null || printTemplates.Where(p => p.BillType == (int)BillTypeEnum.ZeroLoadBill).Count() == 0)
                    {
                        string content24 = "<div id=\"theadid\">  <p style=\"text - align: center; \"><span style=\"font - size: 24pt; \">@商铺名称</span></p>    <p style=\"text - align: left; \">仓库：@仓库 &nbsp; 车辆：@车辆 &nbsp; &nbsp; 业务员：@业务员</p>    </div>    <div id=\"tbodyid\">  <table style=\"width: 729px; \" class=\"table table-bordered\">    <thead>  <tr style=\"height: 20px; \">    <td style=\"width: 32px; height: 20px; \"><strong>&nbsp;</strong></td>    <td style=\"width: 153px; height: 20px; \"><strong>商品名称</strong></td>    <td style=\"width: 105px; height: 20px; \"><strong>数量</strong></td>    </tr>    </thead>    <tbody>    <tr style=\"height: 17px; \">    <td style=\"width: 32px; height: 17px; \">&nbsp;#序号</td>    <td style=\"width: 153px; height: 17px; \">#商品名称</td>    <td style=\"width: 105px; height: 17px; \">#数量</td>    </tr>    </tbody>    <tfoot>  <tr style=\"height: 17px; \">    <td style=\"width: 32px; height: 17px; \"><strong>&nbsp;</strong></td>    <td style=\"width: 153px; height: 17px; \"><strong>合计</strong></td>    <td style=\"width: 105px; height: 17px; \">数量(大中小):###</td>    </tr>    </tfoot>  </table>    </div>    <div id=\"tfootid\">&nbsp;</div>";

                        PrintTemplate printTemplate24 = new PrintTemplate()
                        {
                            StoreId = storeId,
                            TemplateType = 0,
                            BillType = (int)BillTypeEnum.ZeroLoadBill,
                            Title = CommonHelper.GetEnumDescription(BillTypeEnum.ZeroLoadBill),
                            Content = content24
                        };
                        _printTemplateService.InsertPrintTemplate(printTemplate24);
                    }

                    #endregion

                    #region 整箱拆零合并单

                    if (printTemplates == null || printTemplates.Where(p => p.BillType == (int)BillTypeEnum.ZeroLoadBill).Count() == 0)
                    {
                        string content25 = "<div id=\"theadid\">  <p style=\"text - align: center; \"><span style=\"font - size: 24pt; \">@商铺名称</span></p>    <p style=\"text - align: left; \">仓库：@仓库 &nbsp; 车辆：@车辆 &nbsp; &nbsp; 业务员：@业务员</p>    <p style=\"text - align: left; \">订单编号：@订单编号</p>    </div>    <div id=\"tbodyid\">  <table style=\"width: 729px; \" class=\"table table-bordered\">    <thead>  <tr style=\"height: 20px; \">    <td style=\"width: 32px; height: 20px; \"><strong>&nbsp;</strong></td>    <td style=\"width: 300px; height: 20px; \"><strong>商品名称</strong></td>    <td style=\"width: 105px; height: 20px; \"><strong>数量</strong></td>    </tr>    </thead>    <tbody>    <tr style=\"height: 17px; \">    <td style=\"width: 32px; height: 17px; \">&nbsp;#序号</td>    <td style=\"width: 300px; height: 17px; \">#商品名称</td>    <td style=\"width: 105px; height: 17px; \">#数量</td>    </tr>    </tbody>    <tfoot>  <tr style=\"height: 17px; \">    <td style=\"width: 32px; height: 17px; \"><strong>&nbsp;</strong></td>    <td style=\"width: 153px; height: 17px; \"><strong>合计</strong></td>    <td style=\"width: 105px; height: 17px; \">数量(大中小):###</td>    </tr>    </tfoot>  </table>    </div>    <div id=\"tfootid\">&nbsp;</div>";

                        PrintTemplate printTemplate25 = new PrintTemplate()
                        {
                            StoreId = storeId,
                            TemplateType = 0,
                            BillType = (int)BillTypeEnum.AllZeroMergerBill,
                            Title = CommonHelper.GetEnumDescription(BillTypeEnum.AllZeroMergerBill),
                            Content = content25
                        };
                        _printTemplateService.InsertPrintTemplate(printTemplate25);
                    }

                    #endregion

                    #region 记账凭证

                    if (printTemplates == null || printTemplates.Where(p => p.BillType == (int)BillTypeEnum.AccountingVoucher).Count() == 0)
                    {
                        string content26 = "<div id=\"theadid\">  <p style=\"text - align: center; \"><span style=\"font - size: 24pt; \">@商铺名称</span></p>    <p style=\"text - align: left; \">日期：@日期 &nbsp; &nbsp;单据编号：@单据编号 &nbsp; &nbsp; 生成方式：@生成方式 &nbsp; &nbsp; 凭证号：@凭证号</p>    </div>    <div id=\"tbodyid\">  <table style=\"width: 728px; \" class=\"table table-bordered\">    <thead>  <tr style=\"height: 20px; \">    <td style=\"width: 32px; height: 20px; \"><strong>&nbsp;</strong></td>    <td style=\"width: 125px; height: 20px; text - align: center; \"><strong>摘要</strong></td>    <td style=\"width: 82px; height: 20px; text - align: center; \"><strong>科目</strong></td>    <td style=\"width: 110px; height: 20px; text - align: center; \"><strong>借方金额</strong></td>    <td style=\"width: 122px; height: 20px; text - align: center; \"><strong>贷方金额</strong></td>    </tr>    </thead>    <tbody>    <tr style=\"height: 17px; \">    <td style=\"width: 32px; height: 17px; \">&nbsp;#序号</td>    <td style=\"width: 125px; height: 17px; \">#摘要</td>    <td style=\"width: 82px; height: 17px; \">#科目</td>    <td style=\"width: 110px; height: 17px; text - align: right; \">#借方金额</td>    <td style=\"width: 122px; height: 17px; text - align: right; \">#贷方金额</td>    </tr>    </tbody>    <tfoot>  <tr style=\"height: 24px; \">    <td style=\"width: 32px; height: 24px; \">&nbsp;</td>    <td style=\"width: 125px; height: 24px; text - align:center\" colspan=\"2\"><strong>合计:&nbsp;合计总金额:###</strong></td>    <td style=\"width: 122px; height: 24px; text - align: right; \">&nbsp;借方总金额:###</td>    <td style=\"width: 77px; height: 24px; text - align: right; \">&nbsp;贷方总金额:###</td>    </tr>    </tfoot>  </table>    </div>    <div id=\"tfootid\">  <p>制单人：@制单人 &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;审核人：@审核人</p>    </div>";

                        PrintTemplate printTemplate26 = new PrintTemplate()
                        {
                            StoreId = storeId,
                            TemplateType = 0,
                            BillType = (int)BillTypeEnum.AccountingVoucher,
                            Title = CommonHelper.GetEnumDescription(BillTypeEnum.AccountingVoucher),
                            Content = content26
                        };
                        _printTemplateService.InsertPrintTemplate(printTemplate26);
                    }

                    #endregion

                    #region 库存表

                    if (printTemplates == null || printTemplates.Where(p => p.BillType == (int)BillTypeEnum.StockReport).Count() == 0)
                    {
                        string content27 = "<div id=\"theadid\">  <p style=\"text - align: center; \"><span style=\"font - size: 20pt; \">库存表</span></p>    <p style=\"text - align: left; \">仓库：仓库@&nbsp;&nbsp;商品名称：商品名称@&nbsp;&nbsp;商品类别：商品类别@&nbsp;&nbsp;</p>    </div>    <div id=\"tbodyid\">  <table style=\"width: 729px; \" cellpadding=\"0\" class=\"table table-bordered\">    <thead>  <tr style=\"height: 1px; \">    <td style=\"width: 36px; height: 1px; \"><strong>序号</strong></td>    <td style=\"width: 100px; height: 1px; text - align: center; \"><strong>商品类别</strong></td>    <td style=\"width: 100px; height: 1px; text - align: center; \"><strong>商品编号</strong></td>    <td style=\"width: 100px; height: 1px; text - align: center; \"><strong>商品名称</strong></td>    <td style=\"width: 100px; height: 1px; text - align: center; \"><strong>条形码</strong></td>    <td style=\"width: 100px; height: 1px; text - align: center; \"><strong>实际库存数量</strong></td>    <td style=\"width: 100px; height: 1px; text - align: center; \"><strong>单位换算</strong></td>    </tr>    </thead>    <tbody>    <tr style=\"height: 5px; \">    <td style=\"width: 36px; height: 1px; \">序号#</td>    <td style=\"width: 100px; height: 1px; text - align: center; \">商品类别#</td>    <td style=\"width: 100px; height: 1px; text - align: center; \">商品编号#</td>    <td style=\"width: 100px; height: 1px; text - align: center; \">商品名称#</td>    <td style=\"width: 100px; height: 1px; text - align: center; \">条形码#</td>    <td style=\"width: 100px; height: 1px; text - align: center; \">实际库存数量#</td>    <td style=\"width: 100px; height: 1px; text - align: center; \">单位换算#</td>    </tr>    </tbody>    <tfoot>  <tr style=\"height: 24px; \">    <td style=\"width: 36px; height: 1px; \">&nbsp;</td>    <td style=\"width: 100px; height: 1px; text - align: center; \"><strong>合计</strong></td>    <td style=\"width: 100px; height: 1px; text - align: center; \">&nbsp;</td>    <td style=\"width: 100px; height: 1px; text - align: center; \">&nbsp;</td>    <td style=\"width: 100px; height: 1px; text - align: center; \">&nbsp;</td>    <td style=\"width: 100px; height: 1px; text - align: center; \">实际库存数量##</td>    <td style=\"width: 100px; height: 1px; text - align: center; \">&nbsp;</td>    </tr>    </tfoot>  </table>    </div>";

                        PrintTemplate printTemplate27 = new PrintTemplate()
                        {
                            StoreId = storeId,
                            TemplateType = 1,
                            BillType = (int)BillTypeEnum.StockReport,
                            Title = CommonHelper.GetEnumDescription(BillTypeEnum.StockReport),
                            Content = content27
                        };
                        _printTemplateService.InsertPrintTemplate(printTemplate27);
                    }

                    #endregion

                    #region 销售汇总(客户/商品)

                    if (printTemplates == null || printTemplates.Where(p => p.BillType == (int)BillTypeEnum.SaleSummeryReport).Count() == 0)
                    {
                        string content28 = "<div id=\"theadid\">  <p style=\"text - align: center; \"><span style=\"font - size: 20pt; \">销售汇总(客户/商品)</span></p>    <p style=\"text - align: left; \">交易日期：交易日期@&nbsp;&nbsp;仓库：仓库@&nbsp;&nbsp;商品名称：商品名称@&nbsp;&nbsp;商品类别：商品类别@&nbsp;&nbsp;</p>    <p style=\"text - align: left; \">业务员：业务员@&nbsp;&nbsp;客户：客户@</p>    </div>    <div id=\"tbodyid\">  <table style=\"width: 729px; \" cellpadding=\"0\" class=\"table table-bordered\">    <thead>  <tr style=\"height: 1px; \">    <td style=\"width: 36px; height: 1px; \"><strong>序号</strong></td>    <td style=\"width: 100px; height: 1px; text - align: center; \"><strong>客户名称</strong></td>    <td style=\"width: 100px; height: 1px; text - align: center; \"><strong>商品名称</strong></td>    <td style=\"width: 100px; height: 1px; text - align: center; \"><strong>销售数量</strong></td>    <td style=\"width: 100px; height: 1px; text - align: center; \"><strong>销售金额</strong></td>    <td style=\"width: 100px; height: 1px; text - align: center; \"><strong>退货数量</strong></td>    <td style=\"width: 100px; height: 1px; text - align: center; \"><strong>退货金额</strong></td>    <td style=\"width: 100px; height: 1px; text - align: center; \"><strong>还货数量</strong></td>    <td style=\"width: 100px; height: 1px; text - align: center; \"><strong>还货金额</strong></td>    <td style=\"width: 100px; height: 1px; text - align: center; \"><strong>总数量</strong></td>    <td style=\"width: 100px; height: 1px; text - align: center; \"><strong>总金额</strong></td>    </tr>    </thead>    <tbody>    <tr style=\"height: 5px; \">    <td style=\"width: 36px; height: 1px; \">序号#</td>    <td style=\"width: 100px; height: 1px; text - align: center; \">客户名称#</td>    <td style=\"width: 100px; height: 1px; text - align: center; \">商品名称#</td>    <td style=\"width: 100px; height: 1px; text - align: center; \">销售数量#</td>    <td style=\"width: 100px; height: 1px; text - align: center; \">销售金额#</td>    <td style=\"width: 100px; height: 1px; text - align: center; \">退货数量#</td>    <td style=\"width: 100px; height: 1px; text - align: center; \">退货金额#</td>    <td style=\"width: 100px; height: 1px; text - align: center; \">还货数量#</td>    <td style=\"width: 100px; height: 1px; text - align: center; \">还货金额#</td>    <td style=\"width: 100px; height: 1px; text - align: center; \">总数量#</td>    <td style=\"width: 100px; height: 1px; text - align: center; \">总金额#</td>    </tr>    </tbody>    <tfoot>  <tr style=\"height: 24px; \">    <td style=\"width: 36px; height: 1px; \">&nbsp;</td>    <td style=\"width: 100px; height: 1px; text - align: center; \"><strong>总计</strong></td>    <td style=\"width: 100px; height: 1px; text - align: center; \">&nbsp;</td>    <td style=\"width: 100px; height: 1px; text - align: center; \">销售数量##</td>    <td style=\"width: 100px; height: 1px; text - align: center; \">销售金额##</td>    <td style=\"width: 100px; height: 1px; text - align: center; \">退货数量##</td>    <td style=\"width: 100px; height: 1px; text - align: center; \">退货金额##</td>    <td style=\"width: 100px; height: 1px; text - align: center; \">还货数量##</td>    <td style=\"width: 100px; height: 1px; text - align: center; \">还货金额##</td>    <td style=\"width: 100px; height: 1px; text - align: center; \">总数量##</td>    <td style=\"width: 100px; height: 1px; text - align: center; \">总金额##</td>    </tr>    </tfoot>  </table>    </div>";

                        PrintTemplate printTemplate28 = new PrintTemplate()
                        {
                            StoreId = storeId,
                            TemplateType = 1,
                            BillType = (int)BillTypeEnum.SaleSummeryReport,
                            Title = CommonHelper.GetEnumDescription(BillTypeEnum.SaleSummeryReport),
                            Content = content28
                        };
                        _printTemplateService.InsertPrintTemplate(printTemplate28);
                    }

                    #endregion

                    #region 调拨汇总表(按商品)

                    if (printTemplates == null || printTemplates.Where(p => p.BillType == (int)BillTypeEnum.TransferSummaryReport).Count() == 0)
                    {
                        string content29 = "<div id=\"theadid\">  <p style=\"text - align: center; \"><span style=\"font - size: 20pt; \">调拨汇总表(按商品)</span></p>    <p style=\"text - align: left; \">出货仓库：出货仓库@&nbsp;&nbsp;入货仓库：入货仓库@&nbsp;&nbsp;商品名称：商品名称@&nbsp;&nbsp;商品类别：商品类别@&nbsp;&nbsp;</p>    <p style=\"text - align: left; \">交易日期：交易日期@&nbsp;&nbsp;</p>    </div>    <div id=\"tbodyid\">  <table style=\"width: 729px; \" cellpadding=\"0\" class=\"table table-bordered\"><thead>  <tr style=\"height: 1px; \">    <td style=\"width: 36px; height: 1px; \"><strong>序号</strong></td>    <td style=\"width: 100px; height: 1px; text - align: center; \"><strong>商品名称</strong></td>    <td style=\"width: 100px; height: 1px; text - align: center; \"><strong>商品条码</strong></td>    <td style=\"width: 100px; height: 1px; text - align: center; \"><strong>单位换算</strong></td>    <td style=\"width: 100px; height: 1px; text - align: center; \"><strong>出货仓库</strong></td>    <td style=\"width: 100px; height: 1px; text - align: center; \"><strong>入货仓库</strong></td>    <td style=\"width: 100px; height: 1px; text - align: center; \"><strong>数量</strong></td>    </tr>    </thead>    <tbody>    <tr style=\"height: 5px; \">    <td style=\"width: 36px; height: 1px; \">序号#</td>    <td style=\"width: 100px; height: 1px; text - align: center; \">商品名称#</td>    <td style=\"width: 100px; height: 1px; text - align: center; \">商品条码#</td>    <td style=\"width: 100px; height: 1px; text - align: center; \">单位换算#</td>    <td style=\"width: 100px; height: 1px; text - align: center; \">出货仓库#</td>    <td style=\"width: 100px; height: 1px; text - align: center; \">入货仓库#</td>    <td style=\"width: 100px; height: 1px; text - align: center; \">数量#</td>    </tr>    </tbody>    <tfoot>  <tr style=\"height: 24px; \">    <td style=\"width: 36px; height: 1px; \">&nbsp;</td>    <td style=\"width: 100px; height: 1px; text - align: center; \"><strong>合计</strong></td>    <td style=\"width: 100px; height: 1px; text - align: center; \">&nbsp;</td>    <td style=\"width: 100px; height: 1px; text - align: center; \">&nbsp;</td>    <td style=\"width: 100px; height: 1px; text - align: center; \">&nbsp;</td>    <td style=\"width: 100px; height: 1px; text - align: center; \">&nbsp;</td>    <td style=\"width: 100px; height: 1px; text - align: center; \">数量##</td>    </tr>    </tfoot>  </table>    </div>";

                        PrintTemplate printTemplate29 = new PrintTemplate()
                        {
                            StoreId = storeId,
                            TemplateType = 1,
                            BillType = (int)BillTypeEnum.TransferSummaryReport,
                            Title = CommonHelper.GetEnumDescription(BillTypeEnum.TransferSummaryReport),
                            Content = content29
                        };
                        _printTemplateService.InsertPrintTemplate(printTemplate29);
                    }

                    #endregion

                    #region 销售汇总(按商品)

                    if (printTemplates == null || printTemplates.Where(p => p.BillType == (int)BillTypeEnum.SaleSummeryProductReport).Count() == 0)
                    {
                        string content30 = "<!DOCTYPE html>  <html>  <head>  </head>  <body>  <div id=\"theadid\">  <p style=\"text - align: center; \"><span style=\"font - size: 20pt; \">销售汇总(按商品)</span></p>  <p style=\"text - align: left; \">交易日期：交易日期@&nbsp;&nbsp;仓库：仓库@&nbsp;&nbsp;商品名称：商品名称@&nbsp;&nbsp;商品类别：商品类别@&nbsp;&nbsp;</p>  <p style=\"text - align: left; \">业务员：业务员@&nbsp;&nbsp;客户：客户@</p>  </div>  <div id=\"tbodyid\">  <table style=\"width: 729px; \" cellpadding=\"0\" class=\"table table-bordered\"><thead>  <tr style=\"height: 1px; \">  <td style=\"width: 36px; height: 1px; \"><strong>序号</strong></td>  <td style=\"width: 100px; height: 1px; text - align: center; \"><strong>商品名称</strong></td>  <td style=\"width: 100px; height: 1px; text - align: center; \"><strong>条形码</strong></td>  <td style=\"width: 100px; height: 1px; text - align: center; \"><strong>销售数量</strong></td>  <td style=\"width: 100px; height: 1px; text - align: center; \"><strong>销售金额</strong></td>  <td style=\"width: 100px; height: 1px; text - align: center; \"><strong>赠送数量</strong></td>  <td style=\"width: 100px; height: 1px; text - align: center; \"><strong>退货数量</strong></td>  <td style=\"width: 100px; height: 1px; text - align: center; \"><strong>退货金额</strong></td>  <td style=\"width: 100px; height: 1px; text - align: center; \"><strong>净销售量</strong></td>  <td style=\"width: 100px; height: 1px; text - align: center; \"><strong>净销售额</strong></td>  </tr>  </thead>  <tbody>  <tr style=\"height: 5px; \">  <td style=\"width: 36px; height: 1px; \">序号#</td>  <td style=\"width: 100px; height: 1px; text - align: center; \">商品名称#</td>  <td style=\"width: 100px; height: 1px; text - align: center; \">条形码#</td>  <td style=\"width: 100px; height: 1px; text - align: center; \">销售数量#</td>  <td style=\"width: 100px; height: 1px; text - align: center; \">销售金额#</td>  <td style=\"width: 100px; height: 1px; text - align: center; \">赠送数量#</td>  <td style=\"width: 100px; height: 1px; text - align: center; \">退货数量#</td>  <td style=\"width: 100px; height: 1px; text - align: center; \">退货金额#</td>  <td style=\"width: 100px; height: 1px; text - align: center; \">净销售量#</td>  <td style=\"width: 100px; height: 1px; text - align: center; \">销售净额#</td>  </tr>  </tbody>  <tfoot>  <tr style=\"height: 24px; \">  <td style=\"width: 36px; height: 1px; \">&nbsp;</td>  <td style=\"width: 100px; height: 1px; text - align: center; \"><strong>合计</strong></td>  <td style=\"width: 100px; height: 1px; text - align: center; \">&nbsp;</td>  <td style=\"width: 100px; height: 1px; text - align: center; \">销售数量##</td>  <td style=\"width: 100px; height: 1px; text - align: center; \">销售金额##</td>  <td style=\"width: 100px; height: 1px; text - align: center; \">赠送数量##</td>  <td style=\"width: 100px; height: 1px; text - align: center; \">退货数量##</td>  <td style=\"width: 100px; height: 1px; text - align: center; \">退货金额##</td>  <td style=\"width: 100px; height: 1px; text - align: center; \">净销售量##</td>  <td style=\"width: 100px; height: 1px; text - align: center; \">销售净额##</td>  </tr>  </tfoot>  </table>  </div>  </body>  </html>";

                        PrintTemplate printTemplate30 = new PrintTemplate()
                        {
                            StoreId = storeId,
                            TemplateType = 1,
                            BillType = (int)BillTypeEnum.SaleSummeryProductReport,
                            Title = CommonHelper.GetEnumDescription(BillTypeEnum.SaleSummeryProductReport),
                            Content = content30
                        };
                        _printTemplateService.InsertPrintTemplate(printTemplate30);
                    }

                    #endregion

                    #endregion

                    #region 等级 Rank
                    var _rankService = EngineContext.Current.Resolve<IRankService>();
                    List<Rank> ranks = _rankService.GetAll(storeId).ToList();

                    if (ranks == null || ranks.Where(r => r.Name == "A级").Count() == 0)
                    {
                        Rank rank1 = new Rank()
                        {
                            StoreId = storeId,
                            Name = "A级",
                            Deleted = false,
                            CreateDate = DateTime.Now
                        };
                        _rankService.InsertRank(rank1);
                    }

                    if (ranks == null || ranks.Where(r => r.Name == "B级").Count() == 0)
                    {
                        Rank rank2 = new Rank()
                        {
                            StoreId = storeId,
                            Name = "B级",
                            Deleted = false,
                            CreateDate = DateTime.Now
                        };
                        _rankService.InsertRank(rank2);
                    }

                    if (ranks == null || ranks.Where(r => r.Name == "C级").Count() == 0)
                    {
                        Rank rank3 = new Rank()
                        {
                            StoreId = storeId,
                            Name = "C级",
                            Deleted = false,
                            CreateDate = DateTime.Now
                        };
                        _rankService.InsertRank(rank3);
                    }

                    if (ranks == null || ranks.Where(r => r.Name == "D级").Count() == 0)
                    {
                        Rank rank4 = new Rank()
                        {
                            StoreId = storeId,
                            Name = "D级",
                            Deleted = false,
                            CreateDate = DateTime.Now
                        };
                        _rankService.InsertRank(rank4);
                    }

                    if (ranks == null || ranks.Where(r => r.Name == "E级").Count() == 0)
                    {
                        Rank rank5 = new Rank()
                        {
                            StoreId = storeId,
                            Name = "E级",
                            Deleted = false,
                            CreateDate = DateTime.Now
                        };
                        _rankService.InsertRank(rank5);
                    }

                    #endregion

                    #region 规格属性选项 SpecificationAttributeOptions,表示规格属性 SpecificationAttributes
                    var _specificationAttributeService = EngineContext.Current.Resolve<ISpecificationAttributeService>();
                    List<SpecificationAttribute> specificationAttributes = _specificationAttributeService.GetSpecificationAttributesBtStore(storeId).ToList();

                    SpecificationAttribute specificationAttribute1;
                    if (specificationAttributes == null || specificationAttributes.Where(s => s.Name == "包装").Count() == 0)
                    {
                        specificationAttribute1 = new SpecificationAttribute()
                        {
                            StoreId = storeId,
                            Name = "包装",
                            DisplayOrder = 0
                        };
                        _specificationAttributeService.InsertSpecificationAttribute(specificationAttribute1);
                    }
                    else
                    {
                        specificationAttribute1 = specificationAttributes.Where(s => s.Name == "包装").FirstOrDefault();
                    }

                    SpecificationAttribute specificationAttribute2;
                    if (specificationAttributes == null || specificationAttributes.Where(s => s.Name == "容量").Count() == 0)
                    {
                        specificationAttribute2 = new SpecificationAttribute()
                        {
                            StoreId = storeId,
                            Name = "容量",
                            DisplayOrder = 0
                        };
                        _specificationAttributeService.InsertSpecificationAttribute(specificationAttribute2);
                    }
                    else
                    {
                        specificationAttribute2 = specificationAttributes.Where(s => s.Name == "容量").FirstOrDefault();
                    }

                    SpecificationAttribute specificationAttribute3;
                    if (specificationAttributes == null || specificationAttributes.Where(s => s.Name == "小单位").Count() == 0)
                    {
                        specificationAttribute3 = new SpecificationAttribute()
                        {
                            StoreId = storeId,
                            Name = "小单位",
                            DisplayOrder = 0
                        };
                        _specificationAttributeService.InsertSpecificationAttribute(specificationAttribute3);
                    }
                    else
                    {
                        specificationAttribute3 = specificationAttributes.Where(s => s.Name == "小单位").FirstOrDefault();
                    }

                    SpecificationAttribute specificationAttribute4;
                    if (specificationAttributes == null || specificationAttributes.Where(s => s.Name == "大单位").Count() == 0)
                    {
                        specificationAttribute4 = new SpecificationAttribute()
                        {
                            StoreId = storeId,
                            Name = "大单位",
                            DisplayOrder = 0
                        };
                        _specificationAttributeService.InsertSpecificationAttribute(specificationAttribute4);
                    }
                    else
                    {
                        specificationAttribute4 = specificationAttributes.Where(s => s.Name == "大单位").FirstOrDefault();
                    }

                    SpecificationAttribute specificationAttribute5;
                    if (specificationAttributes == null || specificationAttributes.Where(s => s.Name == "中单位").Count() == 0)
                    {
                        specificationAttribute5 = new SpecificationAttribute()
                        {
                            StoreId = storeId,
                            Name = "中单位",
                            DisplayOrder = 0
                        };
                        _specificationAttributeService.InsertSpecificationAttribute(specificationAttribute5);
                    }
                    else
                    {
                        specificationAttribute5 = specificationAttributes.Where(s => s.Name == "中单位").FirstOrDefault();
                    }

                    List<SpecificationAttributeOption> specificationAttributeOptions = _specificationAttributeService.GetSpecificationAttributeOptionsByStore(storeId).ToList();

                    if (specificationAttributeOptions == null || specificationAttributeOptions.Where(s => s.Name == "礼盒装" && s.SpecificationAttributeId == specificationAttribute1.Id).Count() == 0)
                    {
                        SpecificationAttributeOption specificationAttributeOption1 = new SpecificationAttributeOption()
                        {
                            SpecificationAttributeId = specificationAttribute1.Id,
                            Name = "礼盒装",
                            DisplayOrder = 0
                        };
                        _specificationAttributeService.InsertSpecificationAttributeOption(specificationAttributeOption1);
                    }

                    if (specificationAttributeOptions == null || specificationAttributeOptions.Where(s => s.Name == "组合装" && s.SpecificationAttributeId == specificationAttribute1.Id).Count() == 0)
                    {
                        SpecificationAttributeOption specificationAttributeOption2 = new SpecificationAttributeOption()
                        {
                            SpecificationAttributeId = specificationAttribute1.Id,
                            Name = "组合装",
                            DisplayOrder = 0
                        };
                        _specificationAttributeService.InsertSpecificationAttributeOption(specificationAttributeOption2);
                    }

                    if (specificationAttributeOptions == null || specificationAttributeOptions.Where(s => s.Name == "瓶装" && s.SpecificationAttributeId == specificationAttribute1.Id).Count() == 0)
                    {
                        SpecificationAttributeOption specificationAttributeOption3 = new SpecificationAttributeOption()
                        {
                            SpecificationAttributeId = specificationAttribute1.Id,
                            Name = "瓶装",
                            DisplayOrder = 0
                        };
                        _specificationAttributeService.InsertSpecificationAttributeOption(specificationAttributeOption3);
                    }

                    if (specificationAttributeOptions == null || specificationAttributeOptions.Where(s => s.Name == "听装" && s.SpecificationAttributeId == specificationAttribute1.Id).Count() == 0)
                    {
                        SpecificationAttributeOption specificationAttributeOption4 = new SpecificationAttributeOption()
                        {
                            SpecificationAttributeId = specificationAttribute1.Id,
                            Name = "听装",
                            DisplayOrder = 0
                        };
                        _specificationAttributeService.InsertSpecificationAttributeOption(specificationAttributeOption4);
                    }

                    if (specificationAttributeOptions == null || specificationAttributeOptions.Where(s => s.Name == "200ML" && s.SpecificationAttributeId == specificationAttribute2.Id).Count() == 0)
                    {
                        SpecificationAttributeOption specificationAttributeOption5 = new SpecificationAttributeOption()
                        {
                            SpecificationAttributeId = specificationAttribute2.Id,
                            Name = "200ML",
                            DisplayOrder = 0
                        };
                        _specificationAttributeService.InsertSpecificationAttributeOption(specificationAttributeOption5);
                    }

                    if (specificationAttributeOptions == null || specificationAttributeOptions.Where(s => s.Name == "300ML" && s.SpecificationAttributeId == specificationAttribute2.Id).Count() == 0)
                    {
                        SpecificationAttributeOption specificationAttributeOption6 = new SpecificationAttributeOption()
                        {
                            SpecificationAttributeId = specificationAttribute2.Id,
                            Name = "300ML",
                            DisplayOrder = 0
                        };
                        _specificationAttributeService.InsertSpecificationAttributeOption(specificationAttributeOption6);
                    }

                    if (specificationAttributeOptions == null || specificationAttributeOptions.Where(s => s.Name == "400ML" && s.SpecificationAttributeId == specificationAttribute2.Id).Count() == 0)
                    {
                        SpecificationAttributeOption specificationAttributeOption7 = new SpecificationAttributeOption()
                        {
                            SpecificationAttributeId = specificationAttribute2.Id,
                            Name = "400ML",
                            DisplayOrder = 0
                        };
                        _specificationAttributeService.InsertSpecificationAttributeOption(specificationAttributeOption7);
                    }

                    if (specificationAttributeOptions == null || specificationAttributeOptions.Where(s => s.Name == "500ML" && s.SpecificationAttributeId == specificationAttribute2.Id).Count() == 0)
                    {
                        SpecificationAttributeOption specificationAttributeOption8 = new SpecificationAttributeOption()
                        {
                            SpecificationAttributeId = specificationAttribute2.Id,
                            Name = "500ML",
                            DisplayOrder = 0
                        };
                        _specificationAttributeService.InsertSpecificationAttributeOption(specificationAttributeOption8);
                    }

                    if (specificationAttributeOptions == null || specificationAttributeOptions.Where(s => s.Name == "瓶" && s.SpecificationAttributeId == specificationAttribute3.Id).Count() == 0)
                    {
                        SpecificationAttributeOption specificationAttributeOption9 = new SpecificationAttributeOption()
                        {
                            SpecificationAttributeId = specificationAttribute3.Id,
                            Name = "瓶",
                            DisplayOrder = 0
                        };
                        _specificationAttributeService.InsertSpecificationAttributeOption(specificationAttributeOption9);
                    }

                    if (specificationAttributeOptions == null || specificationAttributeOptions.Where(s => s.Name == "包" && s.SpecificationAttributeId == specificationAttribute3.Id).Count() == 0)
                    {
                        SpecificationAttributeOption specificationAttributeOption10 = new SpecificationAttributeOption()
                        {
                            SpecificationAttributeId = specificationAttribute3.Id,
                            Name = "包",
                            DisplayOrder = 0
                        };
                        _specificationAttributeService.InsertSpecificationAttributeOption(specificationAttributeOption10);
                    }

                    if (specificationAttributeOptions == null || specificationAttributeOptions.Where(s => s.Name == "箱" && s.SpecificationAttributeId == specificationAttribute3.Id).Count() == 0)
                    {
                        SpecificationAttributeOption specificationAttributeOption11 = new SpecificationAttributeOption()
                        {
                            SpecificationAttributeId = specificationAttribute3.Id,
                            Name = "箱",
                            DisplayOrder = 0
                        };
                        _specificationAttributeService.InsertSpecificationAttributeOption(specificationAttributeOption11);
                    }

                    if (specificationAttributeOptions == null || specificationAttributeOptions.Where(s => s.Name == "听" && s.SpecificationAttributeId == specificationAttribute3.Id).Count() == 0)
                    {
                        SpecificationAttributeOption specificationAttributeOption12 = new SpecificationAttributeOption()
                        {
                            SpecificationAttributeId = specificationAttribute3.Id,
                            Name = "听",
                            DisplayOrder = 0
                        };
                        _specificationAttributeService.InsertSpecificationAttributeOption(specificationAttributeOption12);
                    }

                    if (specificationAttributeOptions == null || specificationAttributeOptions.Where(s => s.Name == "袋" && s.SpecificationAttributeId == specificationAttribute3.Id).Count() == 0)
                    {
                        SpecificationAttributeOption specificationAttributeOption13 = new SpecificationAttributeOption()
                        {
                            SpecificationAttributeId = specificationAttribute3.Id,
                            Name = "袋",
                            DisplayOrder = 0
                        };
                        _specificationAttributeService.InsertSpecificationAttributeOption(specificationAttributeOption13);
                    }

                    if (specificationAttributeOptions == null || specificationAttributeOptions.Where(s => s.Name == "盒" && s.SpecificationAttributeId == specificationAttribute3.Id).Count() == 0)
                    {
                        SpecificationAttributeOption specificationAttributeOption14 = new SpecificationAttributeOption()
                        {
                            SpecificationAttributeId = specificationAttribute3.Id,
                            Name = "盒",
                            DisplayOrder = 0
                        };
                        _specificationAttributeService.InsertSpecificationAttributeOption(specificationAttributeOption14);
                    }

                    if (specificationAttributeOptions == null || specificationAttributeOptions.Where(s => s.Name == "箱" && s.SpecificationAttributeId == specificationAttribute4.Id).Count() == 0)
                    {
                        SpecificationAttributeOption specificationAttributeOption15 = new SpecificationAttributeOption()
                        {
                            SpecificationAttributeId = specificationAttribute4.Id,
                            Name = "箱",
                            DisplayOrder = 0
                        };
                        _specificationAttributeService.InsertSpecificationAttributeOption(specificationAttributeOption15);
                    }

                    if (specificationAttributeOptions == null || specificationAttributeOptions.Where(s => s.Name == "包" && s.SpecificationAttributeId == specificationAttribute4.Id).Count() == 0)
                    {
                        SpecificationAttributeOption specificationAttributeOption16 = new SpecificationAttributeOption()
                        {
                            SpecificationAttributeId = specificationAttribute4.Id,
                            Name = "包",
                            DisplayOrder = 0
                        };
                        _specificationAttributeService.InsertSpecificationAttributeOption(specificationAttributeOption16);
                    }

                    if (specificationAttributeOptions == null || specificationAttributeOptions.Where(s => s.Name == "件" && s.SpecificationAttributeId == specificationAttribute4.Id).Count() == 0)
                    {
                        SpecificationAttributeOption specificationAttributeOption17 = new SpecificationAttributeOption()
                        {
                            SpecificationAttributeId = specificationAttribute4.Id,
                            Name = "件",
                            DisplayOrder = 0
                        };
                        _specificationAttributeService.InsertSpecificationAttributeOption(specificationAttributeOption17);
                    }

                    if (specificationAttributeOptions == null || specificationAttributeOptions.Where(s => s.Name == "包" && s.SpecificationAttributeId == specificationAttribute5.Id).Count() == 0)
                    {
                        SpecificationAttributeOption specificationAttributeOption18 = new SpecificationAttributeOption()
                        {
                            SpecificationAttributeId = specificationAttribute5.Id,
                            Name = "包",
                            DisplayOrder = 0
                        };
                        _specificationAttributeService.InsertSpecificationAttributeOption(specificationAttributeOption18);
                    }

                    if (specificationAttributeOptions == null || specificationAttributeOptions.Where(s => s.Name == "袋" && s.SpecificationAttributeId == specificationAttribute5.Id).Count() == 0)
                    {
                        SpecificationAttributeOption specificationAttributeOption19 = new SpecificationAttributeOption()
                        {
                            SpecificationAttributeId = specificationAttribute5.Id,
                            Name = "袋",
                            DisplayOrder = 0
                        };
                        _specificationAttributeService.InsertSpecificationAttributeOption(specificationAttributeOption19);
                    }

                    if (specificationAttributeOptions == null || specificationAttributeOptions.Where(s => s.Name == "盒" && s.SpecificationAttributeId == specificationAttribute5.Id).Count() == 0)
                    {
                        SpecificationAttributeOption specificationAttributeOption20 = new SpecificationAttributeOption()
                        {
                            SpecificationAttributeId = specificationAttribute5.Id,
                            Name = "盒",
                            DisplayOrder = 0
                        };
                        _specificationAttributeService.InsertSpecificationAttributeOption(specificationAttributeOption20);
                    }

                    if (specificationAttributeOptions == null || specificationAttributeOptions.Where(s => s.Name == "板" && s.SpecificationAttributeId == specificationAttribute5.Id).Count() == 0)
                    {
                        SpecificationAttributeOption specificationAttributeOption21 = new SpecificationAttributeOption()
                        {
                            SpecificationAttributeId = specificationAttribute5.Id,
                            Name = "板",
                            DisplayOrder = 0
                        };
                        _specificationAttributeService.InsertSpecificationAttributeOption(specificationAttributeOption21);
                    }

                    #endregion

                    #region 统计类别 StatisticalTypes

                    var _statisticalTypeService = EngineContext.Current.Resolve<IStatisticalTypeService>();
                    List<StatisticalTypes> statisticalTypess = _statisticalTypeService.GetAllStatisticalTypess(storeId).ToList();

                    if (statisticalTypess == null || statisticalTypess.Where(s => s.Name == "红牛").Count() == 0)
                    {
                        StatisticalTypes statisticalTypes1 = new StatisticalTypes()
                        {
                            StoreId = storeId,
                            Name = "红牛",
                            Value = "0",
                            CreatedOnUtc = DateTime.Now
                        };
                        _statisticalTypeService.InsertStatisticalTypes(statisticalTypes1);
                    }

                    if (statisticalTypess == null || statisticalTypess.Where(s => s.Name == "百事").Count() == 0)
                    {
                        StatisticalTypes statisticalTypes2 = new StatisticalTypes()
                        {
                            StoreId = storeId,
                            Name = "百事",
                            Value = "1",
                            CreatedOnUtc = DateTime.Now
                        };
                        _statisticalTypeService.InsertStatisticalTypes(statisticalTypes2);
                    }

                    if (statisticalTypess == null || statisticalTypess.Where(s => s.Name == "雪花").Count() == 0)
                    {
                        StatisticalTypes statisticalTypes3 = new StatisticalTypes()
                        {
                            StoreId = storeId,
                            Name = "雪花",
                            Value = "2",
                            CreatedOnUtc = DateTime.Now
                        };
                        _statisticalTypeService.InsertStatisticalTypes(statisticalTypes3);
                    }

                    if (statisticalTypess == null || statisticalTypess.Where(s => s.Name == "饮料").Count() == 0)
                    {
                        StatisticalTypes statisticalTypes4 = new StatisticalTypes()
                        {
                            StoreId = storeId,
                            Name = "饮料",
                            Value = "3",
                            CreatedOnUtc = DateTime.Now
                        };
                        _statisticalTypeService.InsertStatisticalTypes(statisticalTypes4);
                    }

                    if (statisticalTypess == null || statisticalTypess.Where(s => s.Name == "西凤").Count() == 0)
                    {
                        StatisticalTypes statisticalTypes5 = new StatisticalTypes()
                        {
                            StoreId = storeId,
                            Name = "西凤",
                            Value = "4",
                            CreatedOnUtc = DateTime.Now
                        };
                        _statisticalTypeService.InsertStatisticalTypes(statisticalTypes5);
                    }

                    #endregion

                    #region 仓库 WareHouse

                    var _wareHouseService = EngineContext.Current.Resolve<IWareHouseService>();
                    List<WareHouse> wareHouses = _wareHouseService.GetWareHouseList(storeId).ToList();
                    if (wareHouses == null || wareHouses.Where(w => w.Name == "主仓库").Count() == 0)
                    {
                        WareHouse wareHouse1 = new WareHouse()
                        {
                            StoreId = storeId,
                            Code = "zck",
                            Name = "主仓库",
                            Type = 1,
                            AllowNegativeInventory = true,
                            Status = true,
                            Deleted = false,
                            CreatedUserId = userId,
                            CreatedOnUtc = DateTime.Now
                        };
                        _wareHouseService.InsertWareHouse(wareHouse1);
                    }


                    #endregion

                    #endregion

                    //step6:
                    #region 设置

                    var _settingService = EngineContext.Current.Resolve<ISettingService>();

                    _settingService.ClearCache(store.Id);
                    #region APP打印设置

                    var aPPPrintSetting = _settingService.LoadSetting<APPPrintSetting>(storeId);
                    aPPPrintSetting.AllowPrintPackPrice = false;
                    aPPPrintSetting.PrintMode = 1;
                    aPPPrintSetting.PrintingNumber = 1;
                    aPPPrintSetting.AllowAutoPrintSalesAndReturn = false;
                    aPPPrintSetting.AllowAutoPrintOrderAndReturn = false;
                    aPPPrintSetting.AllowAutoPrintAdvanceReceipt = false;
                    aPPPrintSetting.AllowAutoPrintArrears = false;
                    aPPPrintSetting.AllowPrintOnePass = false;
                    aPPPrintSetting.AllowPrintProductSummary = false;
                    aPPPrintSetting.AllowPringMobile = false;
                    aPPPrintSetting.AllowPrintingTimeAndNumber = false;
                    aPPPrintSetting.AllowPrintCustomerBalance = false;
                    aPPPrintSetting.PageHeaderText = "";
                    aPPPrintSetting.PageFooterText1 = "";
                    aPPPrintSetting.PageFooterText2 = "";
                    aPPPrintSetting.PageHeaderImage = "";
                    aPPPrintSetting.PageFooterImage = "";

                    _settingService.SaveSetting(aPPPrintSetting, x => x.AllowPrintPackPrice, storeId, false);
                    _settingService.SaveSetting(aPPPrintSetting, x => x.PrintMode, storeId, false);
                    _settingService.SaveSetting(aPPPrintSetting, x => x.PrintingNumber, storeId, false);
                    _settingService.SaveSetting(aPPPrintSetting, x => x.AllowAutoPrintSalesAndReturn, storeId, false);
                    _settingService.SaveSetting(aPPPrintSetting, x => x.AllowAutoPrintOrderAndReturn, storeId, false);
                    _settingService.SaveSetting(aPPPrintSetting, x => x.AllowAutoPrintAdvanceReceipt, storeId, false);
                    _settingService.SaveSetting(aPPPrintSetting, x => x.AllowAutoPrintArrears, storeId, false);
                    _settingService.SaveSetting(aPPPrintSetting, x => x.AllowPrintOnePass, storeId, false);
                    _settingService.SaveSetting(aPPPrintSetting, x => x.AllowPrintProductSummary, storeId, false);
                    _settingService.SaveSetting(aPPPrintSetting, x => x.AllowPringMobile, storeId, false);
                    _settingService.SaveSetting(aPPPrintSetting, x => x.AllowPrintingTimeAndNumber, storeId, false);
                    _settingService.SaveSetting(aPPPrintSetting, x => x.AllowPrintCustomerBalance, storeId, false);
                    _settingService.SaveSetting(aPPPrintSetting, x => x.PageHeaderText, storeId, false);
                    _settingService.SaveSetting(aPPPrintSetting, x => x.PageFooterText1, storeId, false);
                    _settingService.SaveSetting(aPPPrintSetting, x => x.PageFooterText2, storeId, false);
                    _settingService.SaveSetting(aPPPrintSetting, x => x.PageHeaderImage, storeId, false);
                    _settingService.SaveSetting(aPPPrintSetting, x => x.PageFooterImage, storeId, false);
                    #endregion

                    #region 电脑打印设置

                    var pcPrintSetting = _settingService.LoadSetting<PCPrintSetting>(storeId);
                    pcPrintSetting.StoreName = store.Name;
                    pcPrintSetting.Address = "";
                    pcPrintSetting.PlaceOrderTelphone = "";
                    pcPrintSetting.PrintMethod = 1;
                    pcPrintSetting.PaperType = 1;
                    pcPrintSetting.PaperWidth = 100;
                    pcPrintSetting.PaperHeight = 100;
                    pcPrintSetting.BorderType = 1;
                    pcPrintSetting.MarginTop = 1;
                    pcPrintSetting.MarginBottom = 1;
                    pcPrintSetting.MarginLeft = 1;
                    pcPrintSetting.MarginRight = 1;
                    pcPrintSetting.IsPrintPageNumber = false;
                    pcPrintSetting.PrintHeader = false;
                    pcPrintSetting.PrintFooter = false;
                    pcPrintSetting.IsFixedRowNumber = false;
                    pcPrintSetting.FixedRowNumber = 30;
                    pcPrintSetting.PrintSubtotal = false;
                    pcPrintSetting.PrintPort = 8000;

                    _settingService.SaveSetting(pcPrintSetting, x => x.StoreName, storeId, false);
                    _settingService.SaveSetting(pcPrintSetting, x => x.Address, storeId, false);
                    _settingService.SaveSetting(pcPrintSetting, x => x.PlaceOrderTelphone, storeId, false);
                    _settingService.SaveSetting(pcPrintSetting, x => x.PrintMethod, storeId, false);
                    _settingService.SaveSetting(pcPrintSetting, x => x.PaperType, storeId, false);
                    _settingService.SaveSetting(pcPrintSetting, x => x.PaperWidth, storeId, false);
                    _settingService.SaveSetting(pcPrintSetting, x => x.PaperHeight, storeId, false);
                    _settingService.SaveSetting(pcPrintSetting, x => x.BorderType, storeId, false);
                    _settingService.SaveSetting(pcPrintSetting, x => x.MarginTop, storeId, false);
                    _settingService.SaveSetting(pcPrintSetting, x => x.MarginBottom, storeId, false);
                    _settingService.SaveSetting(pcPrintSetting, x => x.MarginLeft, storeId, false);
                    _settingService.SaveSetting(pcPrintSetting, x => x.MarginRight, storeId, false);
                    _settingService.SaveSetting(pcPrintSetting, x => x.IsPrintPageNumber, storeId, false);
                    _settingService.SaveSetting(pcPrintSetting, x => x.PrintHeader, storeId, false);
                    _settingService.SaveSetting(pcPrintSetting, x => x.PrintFooter, storeId, false);
                    _settingService.SaveSetting(pcPrintSetting, x => x.IsFixedRowNumber, storeId, false);
                    _settingService.SaveSetting(pcPrintSetting, x => x.FixedRowNumber, storeId, false);
                    _settingService.SaveSetting(pcPrintSetting, x => x.PrintSubtotal, storeId, false);
                    _settingService.SaveSetting(pcPrintSetting, x => x.PrintPort, storeId, false);

                    #endregion

                    #region 会计科目
                    //var accountingOptionService = EngineContext.Current.Resolve<IAccountingService>();
                    //List<AccountingOption> accountingOptions = accountingOptionService.GetAllAccountingOptionsByStore(storeId);

                    //#region 1资产类
                    ////库存现金
                    //AccountingOption accountingOption1;
                    //if (accountingOptions == null || accountingOptions.Where(a => a.AccountingTypeId == 1 && a.ParentId == 0 && a.Name == "库存现金").Count() == 0)
                    //{
                    //    accountingOption1 = new AccountingOption()
                    //    {
                    //        StoreId = storeId,
                    //        AccountingTypeId = 1,
                    //        ParentId = 0,
                    //        Name = "库存现金",
                    //        Code = "1001",
                    //        DisplayOrder = 0,
                    //        Enabled = true,
                    //        IsDefault = false
                    //    };
                    //    accountingOptionService.InsertAccountingOption(accountingOption1);
                    //}
                    //else
                    //{
                    //    accountingOption1 = accountingOptions.Where(a => a.AccountingTypeId == 1 && a.ParentId == 0 && a.Name == "库存现金").FirstOrDefault();
                    //}

                    ////银行存款
                    //AccountingOption accountingOption2;
                    //if (accountingOptions == null || accountingOptions.Where(a => a.AccountingTypeId == 1 && a.ParentId == 0 && a.Name == "银行存款").Count() == 0)
                    //{
                    //    accountingOption2 = new AccountingOption()
                    //    {
                    //        StoreId = storeId,
                    //        AccountingTypeId = 1,
                    //        ParentId = 0,
                    //        Name = "银行存款",
                    //        Code = "1002",
                    //        DisplayOrder = 0,
                    //        Enabled = true,
                    //        IsDefault = false
                    //    };
                    //    accountingOptionService.InsertAccountingOption(accountingOption2);
                    //}
                    //else
                    //{
                    //    accountingOption2 = accountingOptions.Where(a => a.AccountingTypeId == 1 && a.ParentId == 0 && a.Name == "银行存款").FirstOrDefault();
                    //}

                    ////应收账款
                    //AccountingOption accountingOption3;
                    //if (accountingOptions == null || accountingOptions.Where(a => a.AccountingTypeId == 1 && a.ParentId == 0 && a.Name == "应收账款").Count() == 0)
                    //{
                    //    accountingOption3 = new AccountingOption()
                    //    {
                    //        StoreId = storeId,
                    //        AccountingTypeId = 1,
                    //        ParentId = 0,
                    //        Name = "应收账款",
                    //        Code = "1004",
                    //        DisplayOrder = 0,
                    //        Enabled = true,
                    //        IsDefault = false,
                    //        AccountCodeTypeId = int.Parse(AccountingCodeEnum.AccountsReceivable.GetTypeCode().ToString())
                    //    };
                    //    accountingOptionService.InsertAccountingOption(accountingOption3);
                    //}
                    //else
                    //{
                    //    accountingOption3 = accountingOptions.Where(a => a.AccountingTypeId == 1 && a.ParentId == 0 && a.Name == "应收账款").FirstOrDefault();
                    //}

                    ////预付账款
                    //AccountingOption accountingOption4;
                    //if (accountingOptions == null || accountingOptions.Where(a => a.AccountingTypeId == 1 && a.ParentId == 0 && a.Name == "预付账款").Count() == 0)
                    //{
                    //    accountingOption4 = new AccountingOption()
                    //    {
                    //        StoreId = storeId,
                    //        AccountingTypeId = 1,
                    //        ParentId = 0,
                    //        Name = "预付账款",
                    //        Code = "1005",
                    //        DisplayOrder = 0,
                    //        Enabled = true,
                    //        IsDefault = false
                    //    };
                    //    accountingOptionService.InsertAccountingOption(accountingOption4);
                    //}
                    //else
                    //{
                    //    accountingOption4 = accountingOptions.Where(a => a.AccountingTypeId == 1 && a.ParentId == 0 && a.Name == "预付账款").FirstOrDefault();
                    //}

                    ////应收利息
                    //AccountingOption accountingOption5;
                    //if (accountingOptions == null || accountingOptions.Where(a => a.AccountingTypeId == 1 && a.ParentId == 0 && a.Name == "应收利息").Count() == 0)
                    //{
                    //    accountingOption5 = new AccountingOption()
                    //    {
                    //        StoreId = storeId,
                    //        AccountingTypeId = 1,
                    //        ParentId = 0,
                    //        Name = "应收利息",
                    //        Code = "1006",
                    //        DisplayOrder = 0,
                    //        Enabled = true,
                    //        IsDefault = false,
                    //        AccountCodeTypeId = int.Parse(AccountingCodeEnum.InterestReceivable.GetTypeCode().ToString())
                    //    };
                    //    accountingOptionService.InsertAccountingOption(accountingOption5);
                    //}
                    //else
                    //{
                    //    accountingOption5 = accountingOptions.Where(a => a.AccountingTypeId == 1 && a.ParentId == 0 && a.Name == "应收利息").FirstOrDefault();
                    //}

                    ////库存商品
                    //AccountingOption accountingOption6;
                    //if (accountingOptions == null || accountingOptions.Where(a => a.AccountingTypeId == 1 && a.ParentId == 0 && a.Name == "库存商品").Count() == 0)
                    //{
                    //    accountingOption6 = new AccountingOption()
                    //    {
                    //        StoreId = storeId,
                    //        AccountingTypeId = 1,
                    //        ParentId = 0,
                    //        Name = "库存商品",
                    //        Code = "1007",
                    //        DisplayOrder = 0,
                    //        Enabled = true,
                    //        IsDefault = false,
                    //        AccountCodeTypeId = int.Parse(AccountingCodeEnum.InventoryGoods.GetTypeCode().ToString())
                    //    };
                    //    accountingOptionService.InsertAccountingOption(accountingOption6);
                    //}
                    //else
                    //{
                    //    accountingOption6 = accountingOptions.Where(a => a.AccountingTypeId == 1 && a.ParentId == 0 && a.Name == "库存商品").FirstOrDefault();
                    //}

                    ////固定资产
                    //AccountingOption accountingOption7;
                    //if (accountingOptions == null || accountingOptions.Where(a => a.AccountingTypeId == 1 && a.ParentId == 0 && a.Name == "固定资产").Count() == 0)
                    //{
                    //    accountingOption7 = new AccountingOption()
                    //    {
                    //        StoreId = storeId,
                    //        AccountingTypeId = 1,
                    //        ParentId = 0,
                    //        Name = "固定资产",
                    //        Code = "1008",
                    //        DisplayOrder = 0,
                    //        Enabled = true,
                    //        IsDefault = false,
                    //        AccountCodeTypeId = int.Parse(AccountingCodeEnum.FixedAssets.GetTypeCode().ToString())
                    //    };
                    //    accountingOptionService.InsertAccountingOption(accountingOption7);
                    //}
                    //else
                    //{
                    //    accountingOption7 = accountingOptions.Where(a => a.AccountingTypeId == 1 && a.ParentId == 0 && a.Name == "固定资产").FirstOrDefault();
                    //}

                    ////累计折旧
                    //AccountingOption accountingOption8;
                    //if (accountingOptions == null || accountingOptions.Where(a => a.AccountingTypeId == 1 && a.ParentId == 0 && a.Name == "累计折旧").Count() == 0)
                    //{
                    //    accountingOption8 = new AccountingOption()
                    //    {
                    //        StoreId = storeId,
                    //        AccountingTypeId = 1,
                    //        ParentId = 0,
                    //        Name = "累计折旧",
                    //        Code = "1009",
                    //        DisplayOrder = 0,
                    //        Enabled = true,
                    //        IsDefault = false,
                    //        AccountCodeTypeId = int.Parse(AccountingCodeEnum.AccumulatedDepreciation.GetTypeCode().ToString())
                    //    };
                    //    accountingOptionService.InsertAccountingOption(accountingOption8);
                    //}
                    //else
                    //{
                    //    accountingOption8 = accountingOptions.Where(a => a.AccountingTypeId == 1 && a.ParentId == 0 && a.Name == "累计折旧").FirstOrDefault();
                    //}

                    ////固定资产清理
                    //AccountingOption accountingOption9;
                    //if (accountingOptions == null || accountingOptions.Where(a => a.AccountingTypeId == 1 && a.ParentId == 0 && a.Name == "固定资产清理").Count() == 0)
                    //{
                    //    accountingOption9 = new AccountingOption()
                    //    {
                    //        StoreId = storeId,
                    //        AccountingTypeId = 1,
                    //        ParentId = 0,
                    //        Name = "固定资产清理",
                    //        Code = "1010",
                    //        DisplayOrder = 0,
                    //        Enabled = true,
                    //        IsDefault = false,
                    //        AccountCodeTypeId = int.Parse(AccountingCodeEnum.LiquidationFixedAssets.GetTypeCode().ToString())
                    //    };
                    //    accountingOptionService.InsertAccountingOption(accountingOption9);
                    //}
                    //else
                    //{
                    //    accountingOption9 = accountingOptions.Where(a => a.AccountingTypeId == 1 && a.ParentId == 0 && a.Name == "固定资产清理").FirstOrDefault();
                    //}

                    ////其他账户
                    //AccountingOption accountingOption10;
                    //if (accountingOptions == null || accountingOptions.Where(a => a.AccountingTypeId == 1 && a.ParentId == 0 && a.Name == "其他账户").Count() == 0)
                    //{
                    //    accountingOption10 = new AccountingOption()
                    //    {
                    //        StoreId = storeId,
                    //        AccountingTypeId = 1,
                    //        ParentId = 0,
                    //        Name = "其他账户",
                    //        Code = "1003",
                    //        DisplayOrder = 0,
                    //        Enabled = true,
                    //        IsDefault = false,
                    //        AccountCodeTypeId = int.Parse(AccountingCodeEnum.OtherAccount.GetTypeCode().ToString())
                    //    };
                    //    accountingOptionService.InsertAccountingOption(accountingOption10);
                    //}
                    //else
                    //{
                    //    accountingOption10 = accountingOptions.Where(a => a.AccountingTypeId == 1 && a.ParentId == 0 && a.Name == "其他账户").FirstOrDefault();
                    //}

                    ////现金
                    //AccountingOption accountingOption11;
                    //if (accountingOptions == null || accountingOptions.Where(a => a.AccountingTypeId == 1 && a.ParentId == accountingOption1.Id && a.Name == "现金").Count() == 0)
                    //{
                    //    accountingOption11 = new AccountingOption()
                    //    {
                    //        StoreId = storeId,
                    //        AccountingTypeId = 1,
                    //        ParentId = accountingOption1.Id,
                    //        Name = "现金",
                    //        Code = "100101",
                    //        DisplayOrder = 0,
                    //        Enabled = true,
                    //        IsDefault = true,
                    //        AccountCodeTypeId = int.Parse(AccountingCodeEnum.Cash.GetTypeCode().ToString())
                    //    };
                    //    accountingOptionService.InsertAccountingOption(accountingOption11);
                    //}
                    //else
                    //{
                    //    accountingOption11 = accountingOptions.Where(a => a.AccountingTypeId == 1 && a.ParentId == accountingOption1.Id && a.Name == "现金").FirstOrDefault();
                    //}

                    ////银行
                    //AccountingOption accountingOption12;
                    //if (accountingOptions == null || accountingOptions.Where(a => a.AccountingTypeId == 1 && a.ParentId == accountingOption2.Id && a.Name == "银行").Count() == 0)
                    //{
                    //    accountingOption12 = new AccountingOption()
                    //    {
                    //        StoreId = storeId,
                    //        AccountingTypeId = 1,
                    //        ParentId = accountingOption2.Id,
                    //        Name = "银行",
                    //        Code = "100201",
                    //        DisplayOrder = 0,
                    //        Enabled = true,
                    //        IsDefault = true,
                    //        AccountCodeTypeId = int.Parse(AccountingCodeEnum.Bank.GetTypeCode().ToString())
                    //    };
                    //    accountingOptionService.InsertAccountingOption(accountingOption12);
                    //}
                    //else
                    //{
                    //    accountingOption12 = accountingOptions.Where(a => a.AccountingTypeId == 1 && a.ParentId == accountingOption2.Id && a.Name == "银行").FirstOrDefault();
                    //}

                    ////微信
                    //AccountingOption accountingOption13;
                    //if (accountingOptions == null || accountingOptions.Where(a => a.AccountingTypeId == 1 && a.ParentId == accountingOption2.Id && a.Name == "微信").Count() == 0)
                    //{
                    //    accountingOption13 = new AccountingOption()
                    //    {
                    //        StoreId = storeId,
                    //        AccountingTypeId = 1,
                    //        ParentId = accountingOption2.Id,
                    //        Name = "微信",
                    //        Code = "100202",
                    //        DisplayOrder = 0,
                    //        Enabled = true,
                    //        IsDefault = true,
                    //        AccountCodeTypeId = int.Parse(AccountingCodeEnum.WChat.GetTypeCode().ToString())
                    //    };
                    //    accountingOptionService.InsertAccountingOption(accountingOption13);
                    //}
                    //else
                    //{
                    //    accountingOption13 = accountingOptions.Where(a => a.AccountingTypeId == 1 && a.ParentId == accountingOption2.Id && a.Name == "微信").FirstOrDefault();
                    //}

                    ////支付宝
                    //AccountingOption accountingOption14;
                    //if (accountingOptions == null || accountingOptions.Where(a => a.AccountingTypeId == 1 && a.ParentId == accountingOption2.Id && a.Name == "支付宝").Count() == 0)
                    //{
                    //    accountingOption14 = new AccountingOption()
                    //    {
                    //        StoreId = storeId,
                    //        AccountingTypeId = 1,
                    //        ParentId = accountingOption2.Id,
                    //        Name = "支付宝",
                    //        Code = "100203",
                    //        DisplayOrder = 0,
                    //        Enabled = true,
                    //        IsDefault = true,
                    //        AccountCodeTypeId = int.Parse(AccountingCodeEnum.PayTreasure.GetTypeCode().ToString())
                    //    };
                    //    accountingOptionService.InsertAccountingOption(accountingOption14);
                    //}
                    //else
                    //{
                    //    accountingOption14 = accountingOptions.Where(a => a.AccountingTypeId == 1 && a.ParentId == accountingOption2.Id && a.Name == "支付宝").FirstOrDefault();
                    //}

                    ////预付款
                    //AccountingOption accountingOption15;
                    //if (accountingOptions == null || accountingOptions.Where(a => a.AccountingTypeId == 1 && a.ParentId == accountingOption4.Id && a.Name == "预付款").Count() == 0)
                    //{
                    //    accountingOption15 = new AccountingOption()
                    //    {
                    //        StoreId = storeId,
                    //        AccountingTypeId = 1,
                    //        ParentId = accountingOption4.Id,
                    //        Name = "预付款",
                    //        Code = "100501",
                    //        DisplayOrder = 0,
                    //        Enabled = true,
                    //        IsDefault = true,
                    //        AccountCodeTypeId = int.Parse(AccountingCodeEnum.AdvancePayment.GetTypeCode().ToString())
                    //    };
                    //    accountingOptionService.InsertAccountingOption(accountingOption15);
                    //}
                    //else
                    //{
                    //    accountingOption15 = accountingOptions.Where(a => a.AccountingTypeId == 1 && a.ParentId == accountingOption4.Id && a.Name == "预付款").FirstOrDefault();
                    //}

                    //#endregion

                    //#region 2负债类
                    ////短期借款
                    //AccountingOption accountingOption16;
                    //if (accountingOptions == null || accountingOptions.Where(a => a.AccountingTypeId == 2 && a.ParentId == 0 && a.Name == "短期借款").Count() == 0)
                    //{
                    //    accountingOption16 = new AccountingOption()
                    //    {
                    //        StoreId = storeId,
                    //        AccountingTypeId = 2,
                    //        ParentId = 0,
                    //        Name = "短期借款",
                    //        Code = "2001",
                    //        DisplayOrder = 0,
                    //        Enabled = true,
                    //        IsDefault = false,
                    //        AccountCodeTypeId = int.Parse(AccountingCodeEnum.ShortBorrowing.GetTypeCode().ToString())
                    //    };
                    //    accountingOptionService.InsertAccountingOption(accountingOption16);
                    //}
                    //else
                    //{
                    //    accountingOption16 = accountingOptions.Where(a => a.AccountingTypeId == 2 && a.ParentId == 0 && a.Name == "短期借款").FirstOrDefault();
                    //}

                    ////应付账款
                    //AccountingOption accountingOption17;
                    //if (accountingOptions == null || accountingOptions.Where(a => a.AccountingTypeId == 2 && a.ParentId == 0 && a.Name == "应付账款").Count() == 0)
                    //{
                    //    accountingOption17 = new AccountingOption()
                    //    {
                    //        StoreId = storeId,
                    //        AccountingTypeId = 2,
                    //        ParentId = 0,
                    //        Name = "应付账款",
                    //        Code = "2002",
                    //        DisplayOrder = 0,
                    //        Enabled = true,
                    //        IsDefault = false,
                    //        AccountCodeTypeId = int.Parse(AccountingCodeEnum.AccountsPayable.GetTypeCode().ToString())
                    //    };
                    //    accountingOptionService.InsertAccountingOption(accountingOption17);
                    //}
                    //else
                    //{
                    //    accountingOption17 = accountingOptions.Where(a => a.AccountingTypeId == 2 && a.ParentId == 0 && a.Name == "应付账款").FirstOrDefault();
                    //}

                    ////预收账款
                    //AccountingOption accountingOption18;
                    //if (accountingOptions == null || accountingOptions.Where(a => a.AccountingTypeId == 2 && a.ParentId == 0 && a.Name == "预收账款").Count() == 0)
                    //{
                    //    accountingOption18 = new AccountingOption()
                    //    {
                    //        StoreId = storeId,
                    //        AccountingTypeId = 2,
                    //        ParentId = 0,
                    //        Name = "预收账款",
                    //        Code = "2003",
                    //        DisplayOrder = 0,
                    //        Enabled = true,
                    //        IsDefault = false
                    //    };
                    //    accountingOptionService.InsertAccountingOption(accountingOption18);
                    //}
                    //else
                    //{
                    //    accountingOption18 = accountingOptions.Where(a => a.AccountingTypeId == 2 && a.ParentId == 0 && a.Name == "预收账款").FirstOrDefault();
                    //}

                    ////订货款
                    //AccountingOption accountingOption19;
                    //if (accountingOptions == null || accountingOptions.Where(a => a.AccountingTypeId == 2 && a.ParentId == 0 && a.Name == "订货款").Count() == 0)
                    //{
                    //    accountingOption19 = new AccountingOption()
                    //    {
                    //        StoreId = storeId,
                    //        AccountingTypeId = 2,
                    //        ParentId = 0,
                    //        Name = "订货款",
                    //        Code = "2004",
                    //        DisplayOrder = 0,
                    //        Enabled = true,
                    //        IsDefault = false,
                    //        AccountCodeTypeId = int.Parse(AccountingCodeEnum.Order.GetTypeCode().ToString())
                    //    };
                    //    accountingOptionService.InsertAccountingOption(accountingOption19);
                    //}
                    //else
                    //{
                    //    accountingOption19 = accountingOptions.Where(a => a.AccountingTypeId == 2 && a.ParentId == 0 && a.Name == "订货款").FirstOrDefault();
                    //}

                    ////应付职工薪酬
                    //AccountingOption accountingOption20;
                    //if (accountingOptions == null || accountingOptions.Where(a => a.AccountingTypeId == 2 && a.ParentId == 0 && a.Name == "应付职工薪酬").Count() == 0)
                    //{
                    //    accountingOption20 = new AccountingOption()
                    //    {
                    //        StoreId = storeId,
                    //        AccountingTypeId = 2,
                    //        ParentId = 0,
                    //        Name = "应付职工薪酬",
                    //        Code = "2005",
                    //        DisplayOrder = 0,
                    //        Enabled = true,
                    //        IsDefault = false,
                    //        AccountCodeTypeId = int.Parse(AccountingCodeEnum.EmployeePayable.GetTypeCode().ToString())
                    //    };
                    //    accountingOptionService.InsertAccountingOption(accountingOption20);
                    //}
                    //else
                    //{
                    //    accountingOption20 = accountingOptions.Where(a => a.AccountingTypeId == 2 && a.ParentId == 0 && a.Name == "应付职工薪酬").FirstOrDefault();
                    //}

                    ////应交税费
                    //AccountingOption accountingOption21;
                    //if (accountingOptions == null || accountingOptions.Where(a => a.AccountingTypeId == 2 && a.ParentId == 0 && a.Name == "应交税费").Count() == 0)
                    //{
                    //    accountingOption21 = new AccountingOption()
                    //    {
                    //        StoreId = storeId,
                    //        AccountingTypeId = 2,
                    //        ParentId = 0,
                    //        Name = "应交税费",
                    //        Code = "2006",
                    //        DisplayOrder = 0,
                    //        Enabled = true,
                    //        IsDefault = false
                    //    };
                    //    accountingOptionService.InsertAccountingOption(accountingOption21);
                    //}
                    //else
                    //{
                    //    accountingOption21 = accountingOptions.Where(a => a.AccountingTypeId == 2 && a.ParentId == 0 && a.Name == "应交税费").FirstOrDefault();
                    //}

                    ////应付利息
                    //AccountingOption accountingOption22;
                    //if (accountingOptions == null || accountingOptions.Where(a => a.AccountingTypeId == 2 && a.ParentId == 0 && a.Name == "应付利息").Count() == 0)
                    //{
                    //    accountingOption22 = new AccountingOption()
                    //    {
                    //        StoreId = storeId,
                    //        AccountingTypeId = 2,
                    //        ParentId = 0,
                    //        Name = "应付利息",
                    //        Code = "2007",
                    //        DisplayOrder = 0,
                    //        Enabled = true,
                    //        IsDefault = false,
                    //        AccountCodeTypeId = int.Parse(AccountingCodeEnum.InterestPayable.GetTypeCode().ToString())
                    //    };
                    //    accountingOptionService.InsertAccountingOption(accountingOption22);
                    //}
                    //else
                    //{
                    //    accountingOption22 = accountingOptions.Where(a => a.AccountingTypeId == 2 && a.ParentId == 0 && a.Name == "应付利息").FirstOrDefault();
                    //}

                    ////其他应付款
                    //AccountingOption accountingOption23;
                    //if (accountingOptions == null || accountingOptions.Where(a => a.AccountingTypeId == 2 && a.ParentId == 0 && a.Name == "其他应付款").Count() == 0)
                    //{
                    //    accountingOption23 = new AccountingOption()
                    //    {
                    //        StoreId = storeId,
                    //        AccountingTypeId = 2,
                    //        ParentId = 0,
                    //        Name = "其他应付款",
                    //        Code = "2008",
                    //        DisplayOrder = 0,
                    //        Enabled = true,
                    //        IsDefault = false,
                    //        AccountCodeTypeId = int.Parse(AccountingCodeEnum.OtherPayables.GetTypeCode().ToString())
                    //    };
                    //    accountingOptionService.InsertAccountingOption(accountingOption23);
                    //}
                    //else
                    //{
                    //    accountingOption23 = accountingOptions.Where(a => a.AccountingTypeId == 2 && a.ParentId == 0 && a.Name == "其他应付款").FirstOrDefault();
                    //}

                    ////长期借款
                    //AccountingOption accountingOption24;
                    //if (accountingOptions == null || accountingOptions.Where(a => a.AccountingTypeId == 2 && a.ParentId == 0 && a.Name == "长期借款").Count() == 0)
                    //{
                    //    accountingOption24 = new AccountingOption()
                    //    {
                    //        StoreId = storeId,
                    //        AccountingTypeId = 2,
                    //        ParentId = 0,
                    //        Name = "长期借款",
                    //        Code = "2009",
                    //        DisplayOrder = 0,
                    //        Enabled = true,
                    //        IsDefault = false,
                    //        AccountCodeTypeId = int.Parse(AccountingCodeEnum.LongBorrowing.GetTypeCode().ToString())
                    //    };
                    //    accountingOptionService.InsertAccountingOption(accountingOption24);
                    //}
                    //else
                    //{
                    //    accountingOption24 = accountingOptions.Where(a => a.AccountingTypeId == 2 && a.ParentId == 0 && a.Name == "长期借款").FirstOrDefault();
                    //}

                    ////预收款
                    //AccountingOption accountingOption25;
                    //if (accountingOptions == null || accountingOptions.Where(a => a.AccountingTypeId == 2 && a.ParentId == accountingOption18.Id && a.Name == "预收款").Count() == 0)
                    //{
                    //    accountingOption25 = new AccountingOption()
                    //    {
                    //        StoreId = storeId,
                    //        AccountingTypeId = 2,
                    //        ParentId = accountingOption18.Id,
                    //        Name = "预收款",
                    //        Code = "200301",
                    //        DisplayOrder = 0,
                    //        Enabled = true,
                    //        IsDefault = false,
                    //        AccountCodeTypeId = int.Parse(AccountingCodeEnum.AdvanceReceipt.GetTypeCode().ToString())
                    //    };
                    //    accountingOptionService.InsertAccountingOption(accountingOption25);
                    //}
                    //else
                    //{
                    //    accountingOption25 = accountingOptions.Where(a => a.AccountingTypeId == 2 && a.ParentId == accountingOption18.Id && a.Name == "预收款").FirstOrDefault();
                    //}

                    ////应交增值税
                    //AccountingOption accountingOption26;
                    //if (accountingOptions == null || accountingOptions.Where(a => a.AccountingTypeId == 2 && a.ParentId == accountingOption21.Id && a.Name == "应交增值税").Count() == 0)
                    //{
                    //    accountingOption26 = new AccountingOption()
                    //    {
                    //        StoreId = storeId,
                    //        AccountingTypeId = 2,
                    //        ParentId = accountingOption21.Id,
                    //        Name = "应交增值税",
                    //        Code = "200601",
                    //        DisplayOrder = 0,
                    //        Enabled = true,
                    //        IsDefault = false
                    //    };
                    //    accountingOptionService.InsertAccountingOption(accountingOption26);
                    //}
                    //else
                    //{
                    //    accountingOption26 = accountingOptions.Where(a => a.AccountingTypeId == 2 && a.ParentId == accountingOption21.Id && a.Name == "应交增值税").FirstOrDefault();
                    //}

                    ////进项税额
                    //AccountingOption accountingOption27;
                    //if (accountingOptions == null || accountingOptions.Where(a => a.AccountingTypeId == 2 && a.ParentId == accountingOption26.Id && a.Name == "进项税额").Count() == 0)
                    //{
                    //    accountingOption27 = new AccountingOption()
                    //    {
                    //        StoreId = storeId,
                    //        AccountingTypeId = 2,
                    //        ParentId = accountingOption26.Id,
                    //        Name = "进项税额",
                    //        Code = "20060101",
                    //        DisplayOrder = 0,
                    //        Enabled = true,
                    //        IsDefault = false,
                    //        AccountCodeTypeId = int.Parse(AccountingCodeEnum.InputTax.GetTypeCode().ToString())
                    //    };
                    //    accountingOptionService.InsertAccountingOption(accountingOption27);
                    //}
                    //else
                    //{
                    //    accountingOption27 = accountingOptions.Where(a => a.AccountingTypeId == 2 && a.ParentId == accountingOption26.Id && a.Name == "进项税额").FirstOrDefault();
                    //}

                    ////已交税金
                    //AccountingOption accountingOption28;
                    //if (accountingOptions == null || accountingOptions.Where(a => a.AccountingTypeId == 2 && a.ParentId == accountingOption26.Id && a.Name == "已交税金").Count() == 0)
                    //{
                    //    accountingOption28 = new AccountingOption()
                    //    {
                    //        StoreId = storeId,
                    //        AccountingTypeId = 2,
                    //        ParentId = accountingOption26.Id,
                    //        Name = "已交税金",
                    //        Code = "20060102",
                    //        DisplayOrder = 0,
                    //        Enabled = true,
                    //        IsDefault = false,
                    //        AccountCodeTypeId = int.Parse(AccountingCodeEnum.PayTaxes.GetTypeCode().ToString())
                    //    };
                    //    accountingOptionService.InsertAccountingOption(accountingOption28);
                    //}
                    //else
                    //{
                    //    accountingOption28 = accountingOptions.Where(a => a.AccountingTypeId == 2 && a.ParentId == accountingOption26.Id && a.Name == "已交税金").FirstOrDefault();
                    //}

                    ////转出未交增值税
                    //AccountingOption accountingOption29;
                    //if (accountingOptions == null || accountingOptions.Where(a => a.AccountingTypeId == 2 && a.ParentId == accountingOption26.Id && a.Name == "转出未交增值税").Count() == 0)
                    //{
                    //    accountingOption29 = new AccountingOption()
                    //    {
                    //        StoreId = storeId,
                    //        AccountingTypeId = 2,
                    //        ParentId = accountingOption26.Id,
                    //        Name = "转出未交增值税",
                    //        Code = "20060103",
                    //        DisplayOrder = 0,
                    //        Enabled = true,
                    //        IsDefault = false,
                    //        AccountCodeTypeId = int.Parse(AccountingCodeEnum.TransferTaxes.GetTypeCode().ToString())
                    //    };
                    //    accountingOptionService.InsertAccountingOption(accountingOption29);
                    //}
                    //else
                    //{
                    //    accountingOption29 = accountingOptions.Where(a => a.AccountingTypeId == 2 && a.ParentId == accountingOption26.Id && a.Name == "转出未交增值税").FirstOrDefault();
                    //}

                    ////销项税额
                    //AccountingOption accountingOption30;
                    //if (accountingOptions == null || accountingOptions.Where(a => a.AccountingTypeId == 2 && a.ParentId == accountingOption26.Id && a.Name == "销项税额").Count() == 0)
                    //{
                    //    accountingOption30 = new AccountingOption()
                    //    {
                    //        StoreId = storeId,
                    //        AccountingTypeId = 2,
                    //        ParentId = accountingOption26.Id,
                    //        Name = "销项税额",
                    //        Code = "20060104",
                    //        DisplayOrder = 0,
                    //        Enabled = true,
                    //        IsDefault = false,
                    //        AccountCodeTypeId = int.Parse(AccountingCodeEnum.OutputTax.GetTypeCode().ToString())
                    //    };
                    //    accountingOptionService.InsertAccountingOption(accountingOption30);
                    //}
                    //else
                    //{
                    //    accountingOption30 = accountingOptions.Where(a => a.AccountingTypeId == 2 && a.ParentId == accountingOption26.Id && a.Name == "销项税额").FirstOrDefault();
                    //}

                    ////未交增值税
                    //AccountingOption accountingOption31;
                    //if (accountingOptions == null || accountingOptions.Where(a => a.AccountingTypeId == 2 && a.ParentId == accountingOption21.Id && a.Name == "未交增值税").Count() == 0)
                    //{
                    //    accountingOption31 = new AccountingOption()
                    //    {
                    //        StoreId = storeId,
                    //        AccountingTypeId = 2,
                    //        ParentId = accountingOption21.Id,
                    //        Name = "未交增值税",
                    //        Code = "200602",
                    //        DisplayOrder = 0,
                    //        Enabled = true,
                    //        IsDefault = false,
                    //        AccountCodeTypeId = int.Parse(AccountingCodeEnum.UnpaidVAT.GetTypeCode().ToString())
                    //    };
                    //    accountingOptionService.InsertAccountingOption(accountingOption31);
                    //}
                    //else
                    //{
                    //    accountingOption31 = accountingOptions.Where(a => a.AccountingTypeId == 2 && a.ParentId == accountingOption21.Id && a.Name == "未交增值税").FirstOrDefault();
                    //}

                    //#endregion

                    //#region 3权益类
                    ////实收资本
                    //AccountingOption accountingOption32;
                    //if (accountingOptions == null || accountingOptions.Where(a => a.AccountingTypeId == 3 && a.ParentId == 0 && a.Name == "实收资本").Count() == 0)
                    //{
                    //    accountingOption32 = new AccountingOption()
                    //    {
                    //        StoreId = storeId,
                    //        AccountingTypeId = 3,
                    //        ParentId = 0,
                    //        Name = "实收资本",
                    //        Code = "3001",
                    //        DisplayOrder = 0,
                    //        Enabled = true,
                    //        IsDefault = false,
                    //        AccountCodeTypeId = int.Parse(AccountingCodeEnum.PaidCapital.GetTypeCode().ToString())
                    //    };
                    //    accountingOptionService.InsertAccountingOption(accountingOption32);
                    //}
                    //else
                    //{
                    //    accountingOption32 = accountingOptions.Where(a => a.AccountingTypeId == 3 && a.ParentId == 0 && a.Name == "实收资本").FirstOrDefault();
                    //}

                    ////资本公积
                    //AccountingOption accountingOption33;
                    //if (accountingOptions == null || accountingOptions.Where(a => a.AccountingTypeId == 3 && a.ParentId == 0 && a.Name == "资本公积").Count() == 0)
                    //{
                    //    accountingOption33 = new AccountingOption()
                    //    {
                    //        StoreId = storeId,
                    //        AccountingTypeId = 3,
                    //        ParentId = 0,
                    //        Name = "资本公积",
                    //        Code = "3002",
                    //        DisplayOrder = 0,
                    //        Enabled = true,
                    //        IsDefault = false,
                    //        AccountCodeTypeId = int.Parse(AccountingCodeEnum.CapitalReserves.GetTypeCode().ToString())
                    //    };
                    //    accountingOptionService.InsertAccountingOption(accountingOption33);
                    //}
                    //else
                    //{
                    //    accountingOption33 = accountingOptions.Where(a => a.AccountingTypeId == 3 && a.ParentId == 0 && a.Name == "资本公积").FirstOrDefault();
                    //}


                    ////盈余公积
                    //AccountingOption accountingOption34;
                    //if (accountingOptions == null || accountingOptions.Where(a => a.AccountingTypeId == 3 && a.ParentId == 0 && a.Name == "盈余公积").Count() == 0)
                    //{
                    //    accountingOption34 = new AccountingOption()
                    //    {
                    //        StoreId = storeId,
                    //        AccountingTypeId = 3,
                    //        ParentId = 0,
                    //        Name = "盈余公积",
                    //        Code = "3003",
                    //        DisplayOrder = 0,
                    //        Enabled = true,
                    //        IsDefault = false
                    //    };
                    //    accountingOptionService.InsertAccountingOption(accountingOption34);
                    //}
                    //else
                    //{
                    //    accountingOption34 = accountingOptions.Where(a => a.AccountingTypeId == 3 && a.ParentId == 0 && a.Name == "盈余公积").FirstOrDefault();
                    //}

                    ////本年利润
                    //AccountingOption accountingOption35;
                    //if (accountingOptions == null || accountingOptions.Where(a => a.AccountingTypeId == 3 && a.ParentId == 0 && a.Name == "本年利润").Count() == 0)
                    //{
                    //    accountingOption35 = new AccountingOption()
                    //    {
                    //        StoreId = storeId,
                    //        AccountingTypeId = 3,
                    //        ParentId = 0,
                    //        Name = "本年利润",
                    //        Code = "3004",
                    //        DisplayOrder = 0,
                    //        Enabled = true,
                    //        IsDefault = false,
                    //        AccountCodeTypeId = int.Parse(AccountingCodeEnum.ThisYearProfits.GetTypeCode().ToString())
                    //    };
                    //    accountingOptionService.InsertAccountingOption(accountingOption35);
                    //}
                    //else
                    //{
                    //    accountingOption35 = accountingOptions.Where(a => a.AccountingTypeId == 3 && a.ParentId == 0 && a.Name == "本年利润").FirstOrDefault();
                    //}

                    ////利润分配
                    //AccountingOption accountingOption36;
                    //if (accountingOptions == null || accountingOptions.Where(a => a.AccountingTypeId == 3 && a.ParentId == 0 && a.Name == "利润分配").Count() == 0)
                    //{
                    //    accountingOption36 = new AccountingOption()
                    //    {
                    //        StoreId = storeId,
                    //        AccountingTypeId = 3,
                    //        ParentId = 0,
                    //        Name = "利润分配",
                    //        Code = "3005",
                    //        DisplayOrder = 0,
                    //        Enabled = true,
                    //        IsDefault = false
                    //    };
                    //    accountingOptionService.InsertAccountingOption(accountingOption36);
                    //}
                    //else
                    //{
                    //    accountingOption36 = accountingOptions.Where(a => a.AccountingTypeId == 3 && a.ParentId == 0 && a.Name == "利润分配").FirstOrDefault();
                    //}

                    ////法定盈余公积
                    //AccountingOption accountingOption37;
                    //if (accountingOptions == null || accountingOptions.Where(a => a.AccountingTypeId == 3 && a.ParentId == accountingOption34.Id && a.Name == "法定盈余公积").Count() == 0)
                    //{
                    //    accountingOption37 = new AccountingOption()
                    //    {
                    //        StoreId = storeId,
                    //        AccountingTypeId = 3,
                    //        ParentId = accountingOption34.Id,
                    //        Name = "法定盈余公积",
                    //        Code = "300301",
                    //        DisplayOrder = 0,
                    //        Enabled = true,
                    //        IsDefault = false,
                    //        AccountCodeTypeId = int.Parse(AccountingCodeEnum.LegalSurplus.GetTypeCode().ToString())
                    //    };
                    //    accountingOptionService.InsertAccountingOption(accountingOption37);
                    //}
                    //else
                    //{
                    //    accountingOption37 = accountingOptions.Where(a => a.AccountingTypeId == 3 && a.ParentId == accountingOption34.Id && a.Name == "法定盈余公积").FirstOrDefault();
                    //}

                    ////任意盈余公积
                    //AccountingOption accountingOption38;
                    //if (accountingOptions == null || accountingOptions.Where(a => a.AccountingTypeId == 3 && a.ParentId == accountingOption34.Id && a.Name == "任意盈余公积").Count() == 0)
                    //{
                    //    accountingOption38 = new AccountingOption()
                    //    {
                    //        StoreId = storeId,
                    //        AccountingTypeId = 3,
                    //        ParentId = accountingOption34.Id,
                    //        Name = "任意盈余公积",
                    //        Code = "300302",
                    //        DisplayOrder = 0,
                    //        Enabled = true,
                    //        IsDefault = false,
                    //        AccountCodeTypeId = int.Parse(AccountingCodeEnum.ArbitrarySurplus.GetTypeCode().ToString())
                    //    };
                    //    accountingOptionService.InsertAccountingOption(accountingOption38);
                    //}
                    //else
                    //{
                    //    accountingOption38 = accountingOptions.Where(a => a.AccountingTypeId == 3 && a.ParentId == accountingOption34.Id && a.Name == "任意盈余公积").FirstOrDefault();
                    //}

                    ////未分配利润
                    //AccountingOption accountingOption39;
                    //if (accountingOptions == null || accountingOptions.Where(a => a.AccountingTypeId == 3 && a.ParentId == accountingOption36.Id && a.Name == "未分配利润").Count() == 0)
                    //{
                    //    accountingOption39 = new AccountingOption()
                    //    {
                    //        StoreId = storeId,
                    //        AccountingTypeId = 3,
                    //        ParentId = accountingOption36.Id,
                    //        Name = "未分配利润",
                    //        Code = "300501",
                    //        DisplayOrder = 0,
                    //        Enabled = true,
                    //        IsDefault = false,
                    //        AccountCodeTypeId = int.Parse(AccountingCodeEnum.UndistributedProfit.GetTypeCode().ToString())
                    //    };
                    //    accountingOptionService.InsertAccountingOption(accountingOption39);
                    //}
                    //else
                    //{
                    //    accountingOption39 = accountingOptions.Where(a => a.AccountingTypeId == 3 && a.ParentId == accountingOption36.Id && a.Name == "未分配利润").FirstOrDefault();
                    //}

                    //#endregion

                    //#region 4损益类（收入）

                    ////主营业务收入
                    //AccountingOption accountingOption40;
                    //if (accountingOptions == null || accountingOptions.Where(a => a.AccountingTypeId == 4 && a.ParentId == 0 && a.Name == "主营业务收入").Count() == 0)
                    //{
                    //    accountingOption40 = new AccountingOption()
                    //    {
                    //        StoreId = storeId,
                    //        AccountingTypeId = 4,
                    //        ParentId = 0,
                    //        Name = "主营业务收入",
                    //        Code = "4001",
                    //        DisplayOrder = 0,
                    //        Enabled = true,
                    //        IsDefault = false,
                    //        AccountCodeTypeId = int.Parse(AccountingCodeEnum.MainIncome.GetTypeCode().ToString())
                    //    };
                    //    accountingOptionService.InsertAccountingOption(accountingOption40);
                    //}
                    //else
                    //{
                    //    accountingOption40 = accountingOptions.Where(a => a.AccountingTypeId == 4 && a.ParentId == 0 && a.Name == "主营业务收入").FirstOrDefault();
                    //}

                    ////其他业务收入
                    //AccountingOption accountingOption41;
                    //if (accountingOptions == null || accountingOptions.Where(a => a.AccountingTypeId == 4 && a.ParentId == 0 && a.Name == "其他业务收入").Count() == 0)
                    //{
                    //    accountingOption41 = new AccountingOption()
                    //    {
                    //        StoreId = storeId,
                    //        AccountingTypeId = 4,
                    //        ParentId = 0,
                    //        Name = "其他业务收入",
                    //        Code = "4002",
                    //        DisplayOrder = 0,
                    //        Enabled = true,
                    //        IsDefault = false
                    //    };
                    //    accountingOptionService.InsertAccountingOption(accountingOption41);
                    //}
                    //else
                    //{
                    //    accountingOption41 = accountingOptions.Where(a => a.AccountingTypeId == 4 && a.ParentId == 0 && a.Name == "其他业务收入").FirstOrDefault();
                    //}

                    ////盘点报溢收入
                    //AccountingOption accountingOption42;
                    //if (accountingOptions == null || accountingOptions.Where(a => a.AccountingTypeId == 4 && a.ParentId == accountingOption41.Id && a.Name == "盘点报溢收入").Count() == 0)
                    //{
                    //    accountingOption42 = new AccountingOption()
                    //    {
                    //        StoreId = storeId,
                    //        AccountingTypeId = 4,
                    //        ParentId = accountingOption41.Id,
                    //        Name = "盘点报溢收入",
                    //        Code = "400201",
                    //        DisplayOrder = 0,
                    //        Enabled = true,
                    //        IsDefault = false,
                    //        AccountCodeTypeId = int.Parse(AccountingCodeEnum.TakeStockIncome.GetTypeCode().ToString())
                    //    };
                    //    accountingOptionService.InsertAccountingOption(accountingOption42);
                    //}
                    //else
                    //{
                    //    accountingOption42 = accountingOptions.Where(a => a.AccountingTypeId == 4 && a.ParentId == accountingOption41.Id && a.Name == "盘点报溢收入").FirstOrDefault();
                    //}

                    ////成本调价收入
                    //AccountingOption accountingOption43;
                    //if (accountingOptions == null || accountingOptions.Where(a => a.AccountingTypeId == 4 && a.ParentId == accountingOption41.Id && a.Name == "成本调价收入").Count() == 0)
                    //{
                    //    accountingOption43 = new AccountingOption()
                    //    {
                    //        StoreId = storeId,
                    //        AccountingTypeId = 4,
                    //        ParentId = accountingOption41.Id,
                    //        Name = "成本调价收入",
                    //        Code = "400202",
                    //        DisplayOrder = 0,
                    //        Enabled = true,
                    //        IsDefault = false,
                    //        AccountCodeTypeId = int.Parse(AccountingCodeEnum.CostIncome.GetTypeCode().ToString())
                    //    };
                    //    accountingOptionService.InsertAccountingOption(accountingOption43);
                    //}
                    //else
                    //{
                    //    accountingOption43 = accountingOptions.Where(a => a.AccountingTypeId == 4 && a.ParentId == accountingOption41.Id && a.Name == "成本调价收入").FirstOrDefault();
                    //}

                    ////厂家返点
                    //AccountingOption accountingOption44;
                    //if (accountingOptions == null || accountingOptions.Where(a => a.AccountingTypeId == 4 && a.ParentId == accountingOption41.Id && a.Name == "厂家返点").Count() == 0)
                    //{
                    //    accountingOption44 = new AccountingOption()
                    //    {
                    //        StoreId = storeId,
                    //        AccountingTypeId = 4,
                    //        ParentId = accountingOption41.Id,
                    //        Name = "厂家返点",
                    //        Code = "400203",
                    //        DisplayOrder = 0,
                    //        Enabled = true,
                    //        IsDefault = false,
                    //        AccountCodeTypeId = int.Parse(AccountingCodeEnum.ManufacturerRebates.GetTypeCode().ToString())
                    //    };
                    //    accountingOptionService.InsertAccountingOption(accountingOption44);
                    //}
                    //else
                    //{
                    //    accountingOption44 = accountingOptions.Where(a => a.AccountingTypeId == 4 && a.ParentId == accountingOption41.Id && a.Name == "厂家返点").FirstOrDefault();
                    //}

                    ////商品拆装收入
                    //AccountingOption accountingOption45;
                    //if (accountingOptions == null || accountingOptions.Where(a => a.AccountingTypeId == 4 && a.ParentId == accountingOption41.Id && a.Name == "商品拆装收入").Count() == 0)
                    //{
                    //    accountingOption45 = new AccountingOption()
                    //    {
                    //        StoreId = storeId,
                    //        AccountingTypeId = 4,
                    //        ParentId = accountingOption41.Id,
                    //        Name = "商品拆装收入",
                    //        Code = "400204",
                    //        DisplayOrder = 0,
                    //        Enabled = true,
                    //        IsDefault = false,
                    //        AccountCodeTypeId = int.Parse(AccountingCodeEnum.GoodsIncome.GetTypeCode().ToString())
                    //    };
                    //    accountingOptionService.InsertAccountingOption(accountingOption45);
                    //}
                    //else
                    //{
                    //    accountingOption45 = accountingOptions.Where(a => a.AccountingTypeId == 4 && a.ParentId == accountingOption41.Id && a.Name == "商品拆装收入").FirstOrDefault();
                    //}

                    ////采购退货收入
                    //AccountingOption accountingOption46;
                    //if (accountingOptions == null || accountingOptions.Where(a => a.AccountingTypeId == 4 && a.ParentId == accountingOption41.Id && a.Name == "采购退货收入").Count() == 0)
                    //{
                    //    accountingOption46 = new AccountingOption()
                    //    {
                    //        StoreId = storeId,
                    //        AccountingTypeId = 4,
                    //        ParentId = accountingOption41.Id,
                    //        Name = "采购退货收入",
                    //        Code = "400205",
                    //        DisplayOrder = 0,
                    //        Enabled = true,
                    //        IsDefault = false,
                    //        AccountCodeTypeId = int.Parse(AccountingCodeEnum.PurchaseIncome.GetTypeCode().ToString())
                    //    };
                    //    accountingOptionService.InsertAccountingOption(accountingOption46);
                    //}
                    //else
                    //{
                    //    accountingOption46 = accountingOptions.Where(a => a.AccountingTypeId == 4 && a.ParentId == accountingOption41.Id && a.Name == "采购退货收入").FirstOrDefault();
                    //}

                    //#endregion

                    //#region 5损益类（支出）
                    ////主营业务成本
                    //AccountingOption accountingOption47;
                    //if (accountingOptions == null || accountingOptions.Where(a => a.AccountingTypeId == 5 && a.ParentId == 0 && a.Name == "主营业务成本").Count() == 0)
                    //{
                    //    accountingOption47 = new AccountingOption()
                    //    {
                    //        StoreId = storeId,
                    //        AccountingTypeId = 5,
                    //        ParentId = 0,
                    //        Name = "主营业务成本",
                    //        Code = "5001",
                    //        DisplayOrder = 0,
                    //        Enabled = true,
                    //        IsDefault = false,
                    //        AccountCodeTypeId = int.Parse(AccountingCodeEnum.MainCost.GetTypeCode().ToString())
                    //    };
                    //    accountingOptionService.InsertAccountingOption(accountingOption47);
                    //}
                    //else
                    //{
                    //    accountingOption47 = accountingOptions.Where(a => a.AccountingTypeId == 5 && a.ParentId == 0 && a.Name == "主营业务成本").FirstOrDefault();
                    //}

                    ////其他业务成本
                    //AccountingOption accountingOption48;
                    //if (accountingOptions == null || accountingOptions.Where(a => a.AccountingTypeId == 5 && a.ParentId == 0 && a.Name == "其他业务成本").Count() == 0)
                    //{
                    //    accountingOption48 = new AccountingOption()
                    //    {
                    //        StoreId = storeId,
                    //        AccountingTypeId = 5,
                    //        ParentId = 0,
                    //        Name = "其他业务成本",
                    //        Code = "5002",
                    //        DisplayOrder = 0,
                    //        Enabled = true,
                    //        IsDefault = false
                    //    };
                    //    accountingOptionService.InsertAccountingOption(accountingOption48);
                    //}
                    //else
                    //{
                    //    accountingOption48 = accountingOptions.Where(a => a.AccountingTypeId == 5 && a.ParentId == 0 && a.Name == "其他业务成本").FirstOrDefault();
                    //}

                    ////销售费用
                    //AccountingOption accountingOption49;
                    //if (accountingOptions == null || accountingOptions.Where(a => a.AccountingTypeId == 5 && a.ParentId == 0 && a.Name == "销售费用").Count() == 0)
                    //{
                    //    accountingOption49 = new AccountingOption()
                    //    {
                    //        StoreId = storeId,
                    //        AccountingTypeId = 5,
                    //        ParentId = 0,
                    //        Name = "销售费用",
                    //        Code = "5003",
                    //        DisplayOrder = 0,
                    //        Enabled = true,
                    //        IsDefault = false
                    //    };
                    //    accountingOptionService.InsertAccountingOption(accountingOption49);
                    //}
                    //else
                    //{
                    //    accountingOption49 = accountingOptions.Where(a => a.AccountingTypeId == 5 && a.ParentId == 0 && a.Name == "销售费用").FirstOrDefault();
                    //}

                    ////管理费用
                    //AccountingOption accountingOption50;
                    //if (accountingOptions == null || accountingOptions.Where(a => a.AccountingTypeId == 5 && a.ParentId == 0 && a.Name == "管理费用").Count() == 0)
                    //{
                    //    accountingOption50 = new AccountingOption()
                    //    {
                    //        StoreId = storeId,
                    //        AccountingTypeId = 5,
                    //        ParentId = 0,
                    //        Name = "管理费用",
                    //        Code = "5004",
                    //        DisplayOrder = 0,
                    //        Enabled = true,
                    //        IsDefault = false
                    //    };
                    //    accountingOptionService.InsertAccountingOption(accountingOption50);
                    //}
                    //else
                    //{
                    //    accountingOption50 = accountingOptions.Where(a => a.AccountingTypeId == 5 && a.ParentId == 0 && a.Name == "管理费用").FirstOrDefault();
                    //}

                    ////财务费用
                    //AccountingOption accountingOption51;
                    //if (accountingOptions == null || accountingOptions.Where(a => a.AccountingTypeId == 5 && a.ParentId == 0 && a.Name == "财务费用").Count() == 0)
                    //{
                    //    accountingOption51 = new AccountingOption()
                    //    {
                    //        StoreId = storeId,
                    //        AccountingTypeId = 5,
                    //        ParentId = 0,
                    //        Name = "财务费用",
                    //        Code = "5005",
                    //        DisplayOrder = 0,
                    //        Enabled = true,
                    //        IsDefault = false
                    //    };
                    //    accountingOptionService.InsertAccountingOption(accountingOption51);
                    //}
                    //else
                    //{
                    //    accountingOption51 = accountingOptions.Where(a => a.AccountingTypeId == 5 && a.ParentId == 0 && a.Name == "财务费用").FirstOrDefault();
                    //}

                    ////盘点亏损
                    //AccountingOption accountingOption52;
                    //if (accountingOptions == null || accountingOptions.Where(a => a.AccountingTypeId == 5 && a.ParentId == accountingOption48.Id && a.Name == "盘点亏损").Count() == 0)
                    //{
                    //    accountingOption52 = new AccountingOption()
                    //    {
                    //        StoreId = storeId,
                    //        AccountingTypeId = 5,
                    //        ParentId = accountingOption48.Id,
                    //        Name = "盘点亏损",
                    //        Code = "500201",
                    //        DisplayOrder = 0,
                    //        Enabled = true,
                    //        IsDefault = false,
                    //        AccountCodeTypeId = int.Parse(AccountingCodeEnum.InventoryLoss.GetTypeCode().ToString())
                    //    };
                    //    accountingOptionService.InsertAccountingOption(accountingOption52);
                    //}
                    //else
                    //{
                    //    accountingOption52 = accountingOptions.Where(a => a.AccountingTypeId == 5 && a.ParentId == accountingOption48.Id && a.Name == "盘点亏损").FirstOrDefault();
                    //}

                    ////成本调价损失
                    //AccountingOption accountingOption53;
                    //if (accountingOptions == null || accountingOptions.Where(a => a.AccountingTypeId == 5 && a.ParentId == accountingOption48.Id && a.Name == "成本调价损失").Count() == 0)
                    //{
                    //    accountingOption53 = new AccountingOption()
                    //    {
                    //        StoreId = storeId,
                    //        AccountingTypeId = 5,
                    //        ParentId = accountingOption48.Id,
                    //        Name = "成本调价损失",
                    //        Code = "500202",
                    //        DisplayOrder = 0,
                    //        Enabled = true,
                    //        IsDefault = false,
                    //        AccountCodeTypeId = int.Parse(AccountingCodeEnum.CostLoss.GetTypeCode().ToString())
                    //    };
                    //    accountingOptionService.InsertAccountingOption(accountingOption53);
                    //}
                    //else
                    //{
                    //    accountingOption53 = accountingOptions.Where(a => a.AccountingTypeId == 5 && a.ParentId == accountingOption48.Id && a.Name == "成本调价损失").FirstOrDefault();
                    //}

                    ////采购退货损失
                    //AccountingOption accountingOption54;
                    //if (accountingOptions == null || accountingOptions.Where(a => a.AccountingTypeId == 5 && a.ParentId == accountingOption48.Id && a.Name == "采购退货损失").Count() == 0)
                    //{
                    //    accountingOption54 = new AccountingOption()
                    //    {
                    //        StoreId = storeId,
                    //        AccountingTypeId = 5,
                    //        ParentId = accountingOption48.Id,
                    //        Name = "采购退货损失",
                    //        Code = "500203",
                    //        DisplayOrder = 0,
                    //        Enabled = true,
                    //        IsDefault = false,
                    //        AccountCodeTypeId = int.Parse(AccountingCodeEnum.PurchaseLoss.GetTypeCode().ToString())
                    //    };
                    //    accountingOptionService.InsertAccountingOption(accountingOption54);
                    //}
                    //else
                    //{
                    //    accountingOption54 = accountingOptions.Where(a => a.AccountingTypeId == 5 && a.ParentId == accountingOption48.Id && a.Name == "采购退货损失").FirstOrDefault();
                    //}

                    ////优惠
                    //AccountingOption accountingOption55;
                    //if (accountingOptions == null || accountingOptions.Where(a => a.AccountingTypeId == 5 && a.ParentId == accountingOption49.Id && a.Name == "优惠").Count() == 0)
                    //{
                    //    accountingOption55 = new AccountingOption()
                    //    {
                    //        StoreId = storeId,
                    //        AccountingTypeId = 5,
                    //        ParentId = accountingOption49.Id,
                    //        Name = "优惠",
                    //        Code = "500301",
                    //        DisplayOrder = 0,
                    //        Enabled = true,
                    //        IsDefault = false,
                    //        AccountCodeTypeId = int.Parse(AccountingCodeEnum.Preferential.GetTypeCode().ToString())
                    //    };
                    //    accountingOptionService.InsertAccountingOption(accountingOption55);
                    //}
                    //else
                    //{
                    //    accountingOption55 = accountingOptions.Where(a => a.AccountingTypeId == 5 && a.ParentId == accountingOption49.Id && a.Name == "优惠").FirstOrDefault();
                    //}

                    ////刷卡手续费
                    //AccountingOption accountingOption56;
                    //if (accountingOptions == null || accountingOptions.Where(a => a.AccountingTypeId == 5 && a.ParentId == accountingOption49.Id && a.Name == "刷卡手续费").Count() == 0)
                    //{
                    //    accountingOption56 = new AccountingOption()
                    //    {
                    //        StoreId = storeId,
                    //        AccountingTypeId = 5,
                    //        ParentId = accountingOption49.Id,
                    //        Name = "刷卡手续费",
                    //        Code = "500302",
                    //        DisplayOrder = 0,
                    //        Enabled = true,
                    //        IsDefault = false,
                    //        AccountCodeTypeId = int.Parse(AccountingCodeEnum.CardFees.GetTypeCode().ToString())
                    //    };
                    //    accountingOptionService.InsertAccountingOption(accountingOption56);
                    //}
                    //else
                    //{
                    //    accountingOption56 = accountingOptions.Where(a => a.AccountingTypeId == 5 && a.ParentId == accountingOption49.Id && a.Name == "刷卡手续费").FirstOrDefault();
                    //}

                    ////陈列费
                    //AccountingOption accountingOption57;
                    //if (accountingOptions == null || accountingOptions.Where(a => a.AccountingTypeId == 5 && a.ParentId == accountingOption49.Id && a.Name == "陈列费").Count() == 0)
                    //{
                    //    accountingOption57 = new AccountingOption()
                    //    {
                    //        StoreId = storeId,
                    //        AccountingTypeId = 5,
                    //        ParentId = accountingOption49.Id,
                    //        Name = "陈列费",
                    //        Code = "500303",
                    //        DisplayOrder = 0,
                    //        Enabled = true,
                    //        IsDefault = false,
                    //        AccountCodeTypeId = int.Parse(AccountingCodeEnum.DisplayFees.GetTypeCode().ToString())
                    //    };
                    //    accountingOptionService.InsertAccountingOption(accountingOption57);
                    //}
                    //else
                    //{
                    //    accountingOption57 = accountingOptions.Where(a => a.AccountingTypeId == 5 && a.ParentId == accountingOption49.Id && a.Name == "陈列费").FirstOrDefault();
                    //}

                    ////油费
                    //AccountingOption accountingOption58;
                    //if (accountingOptions == null || accountingOptions.Where(a => a.AccountingTypeId == 5 && a.ParentId == accountingOption49.Id && a.Name == "油费").Count() == 0)
                    //{
                    //    accountingOption58 = new AccountingOption()
                    //    {
                    //        StoreId = storeId,
                    //        AccountingTypeId = 5,
                    //        ParentId = accountingOption49.Id,
                    //        Name = "油费",
                    //        Code = "500304",
                    //        DisplayOrder = 0,
                    //        Enabled = true,
                    //        IsDefault = false,
                    //        AccountCodeTypeId = int.Parse(AccountingCodeEnum.OilFees.GetTypeCode().ToString())
                    //    };
                    //    accountingOptionService.InsertAccountingOption(accountingOption58);
                    //}
                    //else
                    //{
                    //    accountingOption58 = accountingOptions.Where(a => a.AccountingTypeId == 5 && a.ParentId == accountingOption49.Id && a.Name == "油费").FirstOrDefault();
                    //}

                    ////车辆费
                    //AccountingOption accountingOption59;
                    //if (accountingOptions == null || accountingOptions.Where(a => a.AccountingTypeId == 5 && a.ParentId == accountingOption49.Id && a.Name == "车辆费").Count() == 0)
                    //{
                    //    accountingOption59 = new AccountingOption()
                    //    {
                    //        StoreId = storeId,
                    //        AccountingTypeId = 5,
                    //        ParentId = accountingOption49.Id,
                    //        Name = "车辆费",
                    //        Code = "500305",
                    //        DisplayOrder = 0,
                    //        Enabled = true,
                    //        IsDefault = false,
                    //        AccountCodeTypeId = int.Parse(AccountingCodeEnum.CarFees.GetTypeCode().ToString())
                    //    };
                    //    accountingOptionService.InsertAccountingOption(accountingOption59);
                    //}
                    //else
                    //{
                    //    accountingOption59 = accountingOptions.Where(a => a.AccountingTypeId == 5 && a.ParentId == accountingOption49.Id && a.Name == "车辆费").FirstOrDefault();
                    //}

                    ////用餐费
                    //AccountingOption accountingOption60;
                    //if (accountingOptions == null || accountingOptions.Where(a => a.AccountingTypeId == 5 && a.ParentId == accountingOption49.Id && a.Name == "用餐费").Count() == 0)
                    //{
                    //    accountingOption60 = new AccountingOption()
                    //    {
                    //        StoreId = storeId,
                    //        AccountingTypeId = 5,
                    //        ParentId = accountingOption49.Id,
                    //        Name = "用餐费",
                    //        Code = "500306",
                    //        DisplayOrder = 0,
                    //        Enabled = true,
                    //        IsDefault = false,
                    //        AccountCodeTypeId = int.Parse(AccountingCodeEnum.MealsFees.GetTypeCode().ToString())
                    //    };
                    //    accountingOptionService.InsertAccountingOption(accountingOption60);
                    //}
                    //else
                    //{
                    //    accountingOption60 = accountingOptions.Where(a => a.AccountingTypeId == 5 && a.ParentId == accountingOption49.Id && a.Name == "用餐费").FirstOrDefault();
                    //}

                    ////运费
                    //AccountingOption accountingOption61;
                    //if (accountingOptions == null || accountingOptions.Where(a => a.AccountingTypeId == 5 && a.ParentId == accountingOption49.Id && a.Name == "运费").Count() == 0)
                    //{
                    //    accountingOption61 = new AccountingOption()
                    //    {
                    //        StoreId = storeId,
                    //        AccountingTypeId = 5,
                    //        ParentId = accountingOption49.Id,
                    //        Name = "运费",
                    //        Code = "500307",
                    //        DisplayOrder = 0,
                    //        Enabled = true,
                    //        IsDefault = false,
                    //        AccountCodeTypeId = int.Parse(AccountingCodeEnum.TransferFees.GetTypeCode().ToString())
                    //    };
                    //    accountingOptionService.InsertAccountingOption(accountingOption61);
                    //}
                    //else
                    //{
                    //    accountingOption61 = accountingOptions.Where(a => a.AccountingTypeId == 5 && a.ParentId == accountingOption49.Id && a.Name == "运费").FirstOrDefault();
                    //}

                    ////折旧费用
                    //AccountingOption accountingOption62;
                    //if (accountingOptions == null || accountingOptions.Where(a => a.AccountingTypeId == 5 && a.ParentId == accountingOption49.Id && a.Name == "折旧费用").Count() == 0)
                    //{
                    //    accountingOption62 = new AccountingOption()
                    //    {
                    //        StoreId = storeId,
                    //        AccountingTypeId = 5,
                    //        ParentId = accountingOption49.Id,
                    //        Name = "折旧费用",
                    //        Code = "500308",
                    //        DisplayOrder = 0,
                    //        Enabled = true,
                    //        IsDefault = false,
                    //        AccountCodeTypeId = int.Parse(AccountingCodeEnum.OldFees.GetTypeCode().ToString())
                    //    };
                    //    accountingOptionService.InsertAccountingOption(accountingOption62);
                    //}
                    //else
                    //{
                    //    accountingOption62 = accountingOptions.Where(a => a.AccountingTypeId == 5 && a.ParentId == accountingOption49.Id && a.Name == "折旧费用").FirstOrDefault();
                    //}

                    ////0.5元奖盖
                    //AccountingOption accountingOption63;
                    //if (accountingOptions == null || accountingOptions.Where(a => a.AccountingTypeId == 5 && a.ParentId == accountingOption49.Id && a.Name == "0.5元奖盖").Count() == 0)
                    //{
                    //    accountingOption63 = new AccountingOption()
                    //    {
                    //        StoreId = storeId,
                    //        AccountingTypeId = 5,
                    //        ParentId = accountingOption49.Id,
                    //        Name = "0.5元奖盖",
                    //        Code = "500309",
                    //        DisplayOrder = 0,
                    //        Enabled = true,
                    //        IsDefault = false,
                    //        AccountCodeTypeId = int.Parse(AccountingCodeEnum.BottleCapsFees.GetTypeCode().ToString())
                    //    };
                    //    accountingOptionService.InsertAccountingOption(accountingOption63);
                    //}
                    //else
                    //{
                    //    accountingOption63 = accountingOptions.Where(a => a.AccountingTypeId == 5 && a.ParentId == accountingOption49.Id && a.Name == "0.5元奖盖").FirstOrDefault();
                    //}

                    ////2元瓶盖
                    //AccountingOption accountingOption64;
                    //if (accountingOptions == null || accountingOptions.Where(a => a.AccountingTypeId == 5 && a.ParentId == accountingOption49.Id && a.Name == "2元瓶盖").Count() == 0)
                    //{
                    //    accountingOption64 = new AccountingOption()
                    //    {
                    //        StoreId = storeId,
                    //        AccountingTypeId = 5,
                    //        ParentId = accountingOption49.Id,
                    //        Name = "2元瓶盖",
                    //        Code = "500310",
                    //        DisplayOrder = 0,
                    //        Enabled = true,
                    //        IsDefault = false,
                    //        AccountCodeTypeId = int.Parse(AccountingCodeEnum.TwoCapsFees.GetTypeCode().ToString())
                    //    };
                    //    accountingOptionService.InsertAccountingOption(accountingOption64);
                    //}
                    //else
                    //{
                    //    accountingOption64 = accountingOptions.Where(a => a.AccountingTypeId == 5 && a.ParentId == accountingOption49.Id && a.Name == "2元瓶盖").FirstOrDefault();
                    //}

                    ////50元瓶盖
                    //AccountingOption accountingOption65;
                    //if (accountingOptions == null || accountingOptions.Where(a => a.AccountingTypeId == 5 && a.ParentId == accountingOption49.Id && a.Name == "50元瓶盖").Count() == 0)
                    //{
                    //    accountingOption65 = new AccountingOption()
                    //    {
                    //        StoreId = storeId,
                    //        AccountingTypeId = 5,
                    //        ParentId = accountingOption49.Id,
                    //        Name = "50元瓶盖",
                    //        Code = "500311",
                    //        DisplayOrder = 0,
                    //        Enabled = true,
                    //        IsDefault = false,
                    //        AccountCodeTypeId = int.Parse(AccountingCodeEnum.FiftyCapsFees.GetTypeCode().ToString())
                    //    };
                    //    accountingOptionService.InsertAccountingOption(accountingOption65);
                    //}
                    //else
                    //{
                    //    accountingOption65 = accountingOptions.Where(a => a.AccountingTypeId == 5 && a.ParentId == accountingOption49.Id && a.Name == "50元瓶盖").FirstOrDefault();
                    //}

                    ////办公费
                    //AccountingOption accountingOption66;
                    //if (accountingOptions == null || accountingOptions.Where(a => a.AccountingTypeId == 5 && a.ParentId == accountingOption50.Id && a.Name == "办公费").Count() == 0)
                    //{
                    //    accountingOption66 = new AccountingOption()
                    //    {
                    //        StoreId = storeId,
                    //        AccountingTypeId = 5,
                    //        ParentId = accountingOption50.Id,
                    //        Name = "办公费",
                    //        Code = "500401",
                    //        DisplayOrder = 0,
                    //        Enabled = true,
                    //        IsDefault = false,
                    //        AccountCodeTypeId = int.Parse(AccountingCodeEnum.OfficeFees.GetTypeCode().ToString())
                    //    };
                    //    accountingOptionService.InsertAccountingOption(accountingOption66);
                    //}
                    //else
                    //{
                    //    accountingOption66 = accountingOptions.Where(a => a.AccountingTypeId == 5 && a.ParentId == accountingOption50.Id && a.Name == "办公费").FirstOrDefault();
                    //}

                    ////房租
                    //AccountingOption accountingOption67;
                    //if (accountingOptions == null || accountingOptions.Where(a => a.AccountingTypeId == 5 && a.ParentId == accountingOption50.Id && a.Name == "房租").Count() == 0)
                    //{
                    //    accountingOption67 = new AccountingOption()
                    //    {
                    //        StoreId = storeId,
                    //        AccountingTypeId = 5,
                    //        ParentId = accountingOption50.Id,
                    //        Name = "房租",
                    //        Code = "500402",
                    //        DisplayOrder = 0,
                    //        Enabled = true,
                    //        IsDefault = false,
                    //        AccountCodeTypeId = int.Parse(AccountingCodeEnum.HouseFees.GetTypeCode().ToString())
                    //    };
                    //    accountingOptionService.InsertAccountingOption(accountingOption67);
                    //}
                    //else
                    //{
                    //    accountingOption67 = accountingOptions.Where(a => a.AccountingTypeId == 5 && a.ParentId == accountingOption50.Id && a.Name == "房租").FirstOrDefault();
                    //}

                    ////物业管理费
                    //AccountingOption accountingOption68;
                    //if (accountingOptions == null || accountingOptions.Where(a => a.AccountingTypeId == 5 && a.ParentId == accountingOption50.Id && a.Name == "物业管理费").Count() == 0)
                    //{
                    //    accountingOption68 = new AccountingOption()
                    //    {
                    //        StoreId = storeId,
                    //        AccountingTypeId = 5,
                    //        ParentId = accountingOption50.Id,
                    //        Name = "物业管理费",
                    //        Code = "500403",
                    //        DisplayOrder = 0,
                    //        Enabled = true,
                    //        IsDefault = false,
                    //        AccountCodeTypeId = int.Parse(AccountingCodeEnum.ManagementFees.GetTypeCode().ToString())
                    //    };
                    //    accountingOptionService.InsertAccountingOption(accountingOption68);
                    //}
                    //else
                    //{
                    //    accountingOption68 = accountingOptions.Where(a => a.AccountingTypeId == 5 && a.ParentId == accountingOption50.Id && a.Name == "物业管理费").FirstOrDefault();
                    //}

                    ////水电费
                    //AccountingOption accountingOption69;
                    //if (accountingOptions == null || accountingOptions.Where(a => a.AccountingTypeId == 5 && a.ParentId == accountingOption50.Id && a.Name == "水电费").Count() == 0)
                    //{
                    //    accountingOption69 = new AccountingOption()
                    //    {
                    //        StoreId = storeId,
                    //        AccountingTypeId = 5,
                    //        ParentId = accountingOption50.Id,
                    //        Name = "水电费",
                    //        Code = "500404",
                    //        DisplayOrder = 0,
                    //        Enabled = true,
                    //        IsDefault = false,
                    //        AccountCodeTypeId = int.Parse(AccountingCodeEnum.WaterFees.GetTypeCode().ToString())
                    //    };
                    //    accountingOptionService.InsertAccountingOption(accountingOption69);
                    //}
                    //else
                    //{
                    //    accountingOption69 = accountingOptions.Where(a => a.AccountingTypeId == 5 && a.ParentId == accountingOption50.Id && a.Name == "水电费").FirstOrDefault();
                    //}

                    ////累计折旧
                    //AccountingOption accountingOption70;
                    //if (accountingOptions == null || accountingOptions.Where(a => a.AccountingTypeId == 5 && a.ParentId == accountingOption50.Id && a.Name == "累计折旧").Count() == 0)
                    //{
                    //    accountingOption70 = new AccountingOption()
                    //    {
                    //        StoreId = storeId,
                    //        AccountingTypeId = 5,
                    //        ParentId = accountingOption50.Id,
                    //        Name = "累计折旧",
                    //        Code = "500405",
                    //        DisplayOrder = 0,
                    //        Enabled = true,
                    //        IsDefault = false,
                    //        AccountCodeTypeId = int.Parse(AccountingCodeEnum.AccumulatedFees.GetTypeCode().ToString())
                    //    };
                    //    accountingOptionService.InsertAccountingOption(accountingOption70);
                    //}
                    //else
                    //{
                    //    accountingOption70 = accountingOptions.Where(a => a.AccountingTypeId == 5 && a.ParentId == accountingOption50.Id && a.Name == "累计折旧").FirstOrDefault();
                    //}

                    ////汇兑损益
                    //AccountingOption accountingOption71;
                    //if (accountingOptions == null || accountingOptions.Where(a => a.AccountingTypeId == 5 && a.ParentId == accountingOption51.Id && a.Name == "汇兑损益").Count() == 0)
                    //{
                    //    accountingOption71 = new AccountingOption()
                    //    {
                    //        StoreId = storeId,
                    //        AccountingTypeId = 5,
                    //        ParentId = accountingOption51.Id,
                    //        Name = "汇兑损益",
                    //        Code = "500501",
                    //        DisplayOrder = 0,
                    //        Enabled = true,
                    //        IsDefault = false,
                    //        AccountCodeTypeId = int.Parse(AccountingCodeEnum.ExchangeLoss.GetTypeCode().ToString())
                    //    };
                    //    accountingOptionService.InsertAccountingOption(accountingOption71);
                    //}
                    //else
                    //{
                    //    accountingOption71 = accountingOptions.Where(a => a.AccountingTypeId == 5 && a.ParentId == accountingOption51.Id && a.Name == "汇兑损益").FirstOrDefault();
                    //}

                    ////利息
                    //AccountingOption accountingOption72;
                    //if (accountingOptions == null || accountingOptions.Where(a => a.AccountingTypeId == 5 && a.ParentId == accountingOption51.Id && a.Name == "利息").Count() == 0)
                    //{
                    //    accountingOption72 = new AccountingOption()
                    //    {
                    //        StoreId = storeId,
                    //        AccountingTypeId = 5,
                    //        ParentId = accountingOption51.Id,
                    //        Name = "利息",
                    //        Code = "500502",
                    //        DisplayOrder = 0,
                    //        Enabled = true,
                    //        IsDefault = false,
                    //        AccountCodeTypeId = int.Parse(AccountingCodeEnum.Interest.GetTypeCode().ToString())
                    //    };
                    //    accountingOptionService.InsertAccountingOption(accountingOption72);
                    //}
                    //else
                    //{
                    //    accountingOption72 = accountingOptions.Where(a => a.AccountingTypeId == 5 && a.ParentId == accountingOption51.Id && a.Name == "利息").FirstOrDefault();
                    //}

                    ////手续费
                    //AccountingOption accountingOption73;
                    //if (accountingOptions == null || accountingOptions.Where(a => a.AccountingTypeId == 5 && a.ParentId == accountingOption51.Id && a.Name == "手续费").Count() == 0)
                    //{
                    //    accountingOption73 = new AccountingOption()
                    //    {
                    //        StoreId = storeId,
                    //        AccountingTypeId = 5,
                    //        ParentId = accountingOption51.Id,
                    //        Name = "手续费",
                    //        Code = "500503",
                    //        DisplayOrder = 0,
                    //        Enabled = true,
                    //        IsDefault = false,
                    //        AccountCodeTypeId = int.Parse(AccountingCodeEnum.PoundageFees.GetTypeCode().ToString())
                    //    };
                    //    accountingOptionService.InsertAccountingOption(accountingOption73);
                    //}
                    //else
                    //{
                    //    accountingOption73 = accountingOptions.Where(a => a.AccountingTypeId == 5 && a.ParentId == accountingOption51.Id && a.Name == "手续费").FirstOrDefault();
                    //}

                    //#endregion

                    //6其他类


                    #endregion

                    #region 公司设置

                    var companySetting = _settingService.LoadSetting<CompanySetting>(storeId);

                    //商品精细化
                    companySetting.OpenBillMakeDate = 0;
                    companySetting.MulProductPriceUnit = 0;
                    companySetting.AllowCreateMulSameBarcode = false;

                    //开单选项
                    companySetting.DefaultPurchasePrice = 0;
                    companySetting.VariablePriceCommodity = 0;
                    companySetting.AccuracyRounding = 0;
                    companySetting.MakeBillDisplayBarCode = 0;
                    companySetting.AllowSelectionDateRange = 0;
                    companySetting.DockingTicketPassSystem = false;
                    companySetting.AllowReturnInSalesAndOrders = false;
                    companySetting.AppMaybeDeliveryPersonnel = false;
                    companySetting.AppSubmitOrderAutoAudits = false;
                    companySetting.AppSubmitTransferAutoAudits = false;
                    companySetting.AppSubmitExpenseAutoAudits = false;
                    companySetting.AppSubmitBillReturnAutoAudits = false;
                    companySetting.AppAllowWriteBack = false;
                    companySetting.AllowAdvancePaymentsNegative = false;
                    companySetting.ShowOnlyPrepaidAccountsWithPrepaidReceipts = false;
                    companySetting.TasteByTasteAccountingOnlyPrintMainProduct = false;
                    companySetting.AutoApproveConsumerPaidBill = true;

                    //库存管理
                    companySetting.APPOnlyShowHasStockProduct = false;
                    companySetting.APPShowOrderStock = false;

                    //业务员管理
                    companySetting.OnStoreStopSeconds = 0;
                    companySetting.EnableSalesmanTrack = false;
                    companySetting.Start = "7:00";
                    companySetting.End = "19:00";
                    companySetting.FrequencyTimer = 1;
                    companySetting.SalesmanOnlySeeHisCustomer = false;
                    companySetting.SalesmanVisitStoreBefore = false;
                    companySetting.SalesmanVisitMustPhotographed = false;
                    companySetting.DoorheadPhotoNum = 1;
                    companySetting.DisplayPhotoNum = 4;
                    companySetting.EnableBusinessTime = false;
                    companySetting.BusinessStart = "7:00";
                    companySetting.BusinessEnd = "19:00";
                    companySetting.EnableBusinessVisitLine = false;

                    //财务管理
                    companySetting.ReferenceCostPrice = 0;
                    companySetting.AveragePurchasePriceCalcNumber = 0;
                    companySetting.AllowNegativeInventoryMonthlyClosure = false;

                    //其他设置
                    companySetting.EnableTaxRate = false;
                    companySetting.TaxRate = 0;
                    companySetting.PhotographedWater = "";

                    //清除数据
                    companySetting.ClearArchiveDatas = false;
                    companySetting.ClearBillDatas = false;

                    _settingService.SaveSetting(companySetting, x => x.OpenBillMakeDate, storeId, false);
                    _settingService.SaveSetting(companySetting, x => x.MulProductPriceUnit, storeId, false);
                    _settingService.SaveSetting(companySetting, x => x.AllowCreateMulSameBarcode, storeId, false);
                    _settingService.SaveSetting(companySetting, x => x.DefaultPurchasePrice, storeId, false);
                    _settingService.SaveSetting(companySetting, x => x.VariablePriceCommodity, storeId, false);
                    _settingService.SaveSetting(companySetting, x => x.AccuracyRounding, storeId, false);
                    _settingService.SaveSetting(companySetting, x => x.MakeBillDisplayBarCode, storeId, false);
                    _settingService.SaveSetting(companySetting, x => x.AllowSelectionDateRange, storeId, false);
                    _settingService.SaveSetting(companySetting, x => x.DockingTicketPassSystem, storeId, false);
                    _settingService.SaveSetting(companySetting, x => x.AllowReturnInSalesAndOrders, storeId, false);
                    _settingService.SaveSetting(companySetting, x => x.AppMaybeDeliveryPersonnel, storeId, false);
                    _settingService.SaveSetting(companySetting, x => x.AppSubmitOrderAutoAudits, storeId, false);
                    _settingService.SaveSetting(companySetting, x => x.AppSubmitTransferAutoAudits, storeId, false);
                    _settingService.SaveSetting(companySetting, x => x.AppSubmitExpenseAutoAudits, storeId, false);
                    _settingService.SaveSetting(companySetting, x => x.AppSubmitBillReturnAutoAudits, storeId, false);
                    _settingService.SaveSetting(companySetting, x => x.AppAllowWriteBack, storeId, false);
                    _settingService.SaveSetting(companySetting, x => x.AllowAdvancePaymentsNegative, storeId, false);
                    _settingService.SaveSetting(companySetting, x => x.ShowOnlyPrepaidAccountsWithPrepaidReceipts, storeId, false);
                    _settingService.SaveSetting(companySetting, x => x.TasteByTasteAccountingOnlyPrintMainProduct, storeId, false);
                    _settingService.SaveSetting(companySetting, x => x.APPOnlyShowHasStockProduct, storeId, false);
                    _settingService.SaveSetting(companySetting, x => x.APPShowOrderStock, storeId, false);
                    _settingService.SaveSetting(companySetting, x => x.OnStoreStopSeconds, storeId, false);
                    _settingService.SaveSetting(companySetting, x => x.EnableSalesmanTrack, storeId, false);
                    _settingService.SaveSetting(companySetting, x => x.Start, storeId, false);
                    _settingService.SaveSetting(companySetting, x => x.End, storeId, false);
                    _settingService.SaveSetting(companySetting, x => x.FrequencyTimer, storeId, false);
                    _settingService.SaveSetting(companySetting, x => x.SalesmanOnlySeeHisCustomer, storeId, false);
                    _settingService.SaveSetting(companySetting, x => x.SalesmanVisitStoreBefore, storeId, false);
                    _settingService.SaveSetting(companySetting, x => x.SalesmanVisitMustPhotographed, storeId, false);
                    _settingService.SaveSetting(companySetting, x => x.ReferenceCostPrice, storeId, false);
                    _settingService.SaveSetting(companySetting, x => x.AveragePurchasePriceCalcNumber, storeId, false);
                    _settingService.SaveSetting(companySetting, x => x.AllowNegativeInventoryMonthlyClosure, storeId, false);
                    _settingService.SaveSetting(companySetting, x => x.EnableTaxRate, storeId, false);
                    _settingService.SaveSetting(companySetting, x => x.TaxRate, storeId, false);
                    _settingService.SaveSetting(companySetting, x => x.PhotographedWater, storeId, false);
                    _settingService.SaveSetting(companySetting, x => x.ClearArchiveDatas, storeId, false);
                    _settingService.SaveSetting(companySetting, x => x.ClearBillDatas, storeId, false);
                    _settingService.SaveSetting(companySetting, x => x.DisplayPhotoNum, storeId, false);
                    _settingService.SaveSetting(companySetting, x => x.DoorheadPhotoNum, storeId, false);

                    _settingService.SaveSetting(companySetting, x => x.EnableBusinessTime, storeId, false);
                    _settingService.SaveSetting(companySetting, x => x.BusinessStart, storeId, false);
                    _settingService.SaveSetting(companySetting, x => x.BusinessEnd, storeId, false);
                    _settingService.SaveSetting(companySetting, x => x.EnableBusinessVisitLine, storeId, false);
                    #endregion

                    #region 备注配置 RemarkConfig
                    var _remarkConfigService = EngineContext.Current.Resolve<IRemarkConfigService>();
                    List<RemarkConfig> remarkConfigs = _remarkConfigService.GetAllRemarkConfigs(storeId).ToList();

                    if (remarkConfigs == null || remarkConfigs.Where(r => r.Name == "搭赠").Count() == 0)
                    {
                        RemarkConfig remarkConfig1 = new RemarkConfig()
                        {
                            StoreId = storeId,
                            Name = "搭赠",
                            RemberPrice = true
                        };
                        _remarkConfigService.InsertRemarkConfig(remarkConfig1);
                    }

                    if (remarkConfigs == null || remarkConfigs.Where(r => r.Name == "陈列").Count() == 0)
                    {
                        RemarkConfig remarkConfig2 = new RemarkConfig()
                        {
                            StoreId = storeId,
                            Name = "陈列",
                            RemberPrice = true
                        };
                        _remarkConfigService.InsertRemarkConfig(remarkConfig2);
                    }

                    if (remarkConfigs == null || remarkConfigs.Where(r => r.Name == "打折").Count() == 0)
                    {
                        RemarkConfig remarkConfig3 = new RemarkConfig()
                        {
                            StoreId = storeId,
                            Name = "打折",
                            RemberPrice = true
                        };
                        _remarkConfigService.InsertRemarkConfig(remarkConfig3);
                    }

                    if (remarkConfigs == null || remarkConfigs.Where(r => r.Name == "兑瓶盖").Count() == 0)
                    {
                        RemarkConfig remarkConfig4 = new RemarkConfig()
                        {
                            StoreId = storeId,
                            Name = "兑瓶盖",
                            RemberPrice = true
                        };
                        _remarkConfigService.InsertRemarkConfig(remarkConfig4);
                    }

                    if (remarkConfigs == null || remarkConfigs.Where(r => r.Name == "兑拉环").Count() == 0)
                    {
                        RemarkConfig remarkConfig5 = new RemarkConfig()
                        {
                            StoreId = storeId,
                            Name = "兑拉环",
                            RemberPrice = true
                        };
                        _remarkConfigService.InsertRemarkConfig(remarkConfig5);
                    }

                    if (remarkConfigs == null || remarkConfigs.Where(r => r.Name == "换货").Count() == 0)
                    {
                        RemarkConfig remarkConfig6 = new RemarkConfig()
                        {
                            StoreId = storeId,
                            Name = "换货",
                            RemberPrice = true
                        };
                        _remarkConfigService.InsertRemarkConfig(remarkConfig6);
                    }

                    if (remarkConfigs == null || remarkConfigs.Where(r => r.Name == "冰冻化赠酒").Count() == 0)
                    {
                        RemarkConfig remarkConfig7 = new RemarkConfig()
                        {
                            StoreId = storeId,
                            Name = "冰冻化赠酒",
                            RemberPrice = true
                        };
                        _remarkConfigService.InsertRemarkConfig(remarkConfig7);
                    }

                    if (remarkConfigs == null || remarkConfigs.Where(r => r.Name == "专销").Count() == 0)
                    {
                        RemarkConfig remarkConfig8 = new RemarkConfig()
                        {
                            StoreId = storeId,
                            Name = "专销",
                            RemberPrice = true
                        };
                        _remarkConfigService.InsertRemarkConfig(remarkConfig8);
                    }

                    if (remarkConfigs == null || remarkConfigs.Where(r => r.Name == "消费者促销").Count() == 0)
                    {
                        RemarkConfig remarkConfig9 = new RemarkConfig()
                        {
                            StoreId = storeId,
                            Name = "消费者促销",
                            RemberPrice = true
                        };
                        _remarkConfigService.InsertRemarkConfig(remarkConfig9);
                    }



                    #endregion

                    #region 商品设置
                    var productSetting = _settingService.LoadSetting<ProductSetting>(storeId);
                    productSetting.SmallUnitSpecificationAttributeOptionsMapping = specificationAttribute3.Id;
                    productSetting.StrokeUnitSpecificationAttributeOptionsMapping = specificationAttribute5.Id;
                    productSetting.BigUnitSpecificationAttributeOptionsMapping = specificationAttribute4.Id;

                    _settingService.SaveSetting(productSetting, x => x.SmallUnitSpecificationAttributeOptionsMapping, storeId, false);
                    _settingService.SaveSetting(productSetting, x => x.StrokeUnitSpecificationAttributeOptionsMapping, storeId, false);
                    _settingService.SaveSetting(productSetting, x => x.BigUnitSpecificationAttributeOptionsMapping, storeId, false);

                    #endregion

                    //#region 财务设置

                    //var financeSetting = _settingService.LoadSetting<FinanceSetting>(storeId);

                    ////销售单(收款账户)
                    //FinanceAccountingMap saleFinanceAccountingMap = new FinanceAccountingMap();
                    ////现金
                    //if (saleFinanceAccountingMap.Options == null || saleFinanceAccountingMap.Options.Where(so => so.AccountingTypeId == 1 && so.ParentId == accountingOption1.Id && so.Name == "现金").Count() == 0)
                    //{
                    //    saleFinanceAccountingMap.Options.Add(new AccountingOption()
                    //    {
                    //        StoreId = storeId,
                    //        AccountingTypeId = 1,
                    //        ParentId = accountingOption1.Id,
                    //        Name = "现金",
                    //        Code = "100101",
                    //        DisplayOrder = 0,
                    //        Enabled = true,
                    //        IsDefault = true
                    //    });
                    //}

                    ////银行
                    //if (saleFinanceAccountingMap.Options == null || saleFinanceAccountingMap.Options.Where(so => so.AccountingTypeId == 1 && so.ParentId == accountingOption2.Id && so.Name == "银行").Count() == 0)
                    //{
                    //    saleFinanceAccountingMap.Options.Add(new AccountingOption()
                    //    {
                    //        StoreId = storeId,
                    //        AccountingTypeId = 1,
                    //        ParentId = accountingOption2.Id,
                    //        Name = "银行",
                    //        Code = "100201",
                    //        DisplayOrder = 0,
                    //        Enabled = true,
                    //        IsDefault = true
                    //    });
                    //}

                    //saleFinanceAccountingMap.DefaultOption = accountingOption11.Id;
                    //saleFinanceAccountingMap.DebitOption = accountingOption11.Id;
                    //saleFinanceAccountingMap.CreditOption = accountingOption11.Id;
                    //financeSetting.SaleBillAccountingOptionConfiguration = JsonConvert.SerializeObject(saleFinanceAccountingMap);

                    ////销售订单(收款账户)
                    //FinanceAccountingMap saleReservationBillFinanceAccountingMap = new FinanceAccountingMap();
                    ////现金
                    //if (saleReservationBillFinanceAccountingMap.Options == null || saleReservationBillFinanceAccountingMap.Options.Where(so => so.AccountingTypeId == 1 && so.ParentId == accountingOption1.Id && so.Name == "现金").Count() == 0)
                    //{
                    //    saleReservationBillFinanceAccountingMap.Options.Add(new AccountingOption()
                    //    {
                    //        StoreId = storeId,
                    //        AccountingTypeId = 1,
                    //        ParentId = accountingOption1.Id,
                    //        Name = "现金",
                    //        Code = "100101",
                    //        DisplayOrder = 0,
                    //        Enabled = true,
                    //        IsDefault = true
                    //    });
                    //}

                    ////银行
                    //if (saleReservationBillFinanceAccountingMap.Options == null || saleReservationBillFinanceAccountingMap.Options.Where(so => so.AccountingTypeId == 1 && so.ParentId == accountingOption2.Id && so.Name == "银行").Count() == 0)
                    //{
                    //    saleReservationBillFinanceAccountingMap.Options.Add(new AccountingOption()
                    //    {
                    //        StoreId = storeId,
                    //        AccountingTypeId = 1,
                    //        ParentId = accountingOption2.Id,
                    //        Name = "银行",
                    //        Code = "100201",
                    //        DisplayOrder = 0,
                    //        Enabled = true,
                    //        IsDefault = true
                    //    });
                    //}

                    //saleReservationBillFinanceAccountingMap.DefaultOption = accountingOption11.Id;
                    //saleReservationBillFinanceAccountingMap.DebitOption = accountingOption11.Id;
                    //saleReservationBillFinanceAccountingMap.CreditOption = accountingOption11.Id;
                    //financeSetting.SaleReservationBillAccountingOptionConfiguration = JsonConvert.SerializeObject(saleReservationBillFinanceAccountingMap);

                    ////退货单(收款账户)
                    //FinanceAccountingMap returnBillAccountingOptionConfiguration = new FinanceAccountingMap();
                    ////现金
                    //if (returnBillAccountingOptionConfiguration.Options == null || returnBillAccountingOptionConfiguration.Options.Where(so => so.AccountingTypeId == 1 && so.ParentId == accountingOption1.Id && so.Name == "现金").Count() == 0)
                    //{
                    //    returnBillAccountingOptionConfiguration.Options.Add(new AccountingOption()
                    //    {
                    //        StoreId = storeId,
                    //        AccountingTypeId = 1,
                    //        ParentId = accountingOption1.Id,
                    //        Name = "现金",
                    //        Code = "100101",
                    //        DisplayOrder = 0,
                    //        Enabled = true,
                    //        IsDefault = true
                    //    });
                    //}

                    ////银行
                    //if (returnBillAccountingOptionConfiguration.Options == null || returnBillAccountingOptionConfiguration.Options.Where(so => so.AccountingTypeId == 1 && so.ParentId == accountingOption2.Id && so.Name == "银行").Count() == 0)
                    //{
                    //    returnBillAccountingOptionConfiguration.Options.Add(new AccountingOption()
                    //    {
                    //        StoreId = storeId,
                    //        AccountingTypeId = 1,
                    //        ParentId = accountingOption2.Id,
                    //        Name = "银行",
                    //        Code = "100201",
                    //        DisplayOrder = 0,
                    //        Enabled = true,
                    //        IsDefault = true
                    //    });
                    //}

                    //returnBillAccountingOptionConfiguration.DefaultOption = accountingOption11.Id;
                    //returnBillAccountingOptionConfiguration.DebitOption = accountingOption11.Id;
                    //returnBillAccountingOptionConfiguration.CreditOption = accountingOption11.Id;
                    //financeSetting.ReturnBillAccountingOptionConfiguration = JsonConvert.SerializeObject(returnBillAccountingOptionConfiguration);

                    ////退货订单(收款账户)
                    //FinanceAccountingMap returnReservationBillAccountingOptionConfiguration = new FinanceAccountingMap();
                    ////现金
                    //if (returnReservationBillAccountingOptionConfiguration.Options == null || returnReservationBillAccountingOptionConfiguration.Options.Where(so => so.AccountingTypeId == 1 && so.ParentId == accountingOption1.Id && so.Name == "现金").Count() == 0)
                    //{
                    //    returnReservationBillAccountingOptionConfiguration.Options.Add(new AccountingOption()
                    //    {
                    //        StoreId = storeId,
                    //        AccountingTypeId = 1,
                    //        ParentId = accountingOption1.Id,
                    //        Name = "现金",
                    //        Code = "100101",
                    //        DisplayOrder = 0,
                    //        Enabled = true,
                    //        IsDefault = true
                    //    });
                    //}

                    ////银行
                    //if (returnReservationBillAccountingOptionConfiguration.Options == null || returnReservationBillAccountingOptionConfiguration.Options.Where(so => so.AccountingTypeId == 1 && so.ParentId == accountingOption2.Id && so.Name == "银行").Count() == 0)
                    //{
                    //    returnReservationBillAccountingOptionConfiguration.Options.Add(new AccountingOption()
                    //    {
                    //        StoreId = storeId,
                    //        AccountingTypeId = 1,
                    //        ParentId = accountingOption2.Id,
                    //        Name = "银行",
                    //        Code = "100201",
                    //        DisplayOrder = 0,
                    //        Enabled = true,
                    //        IsDefault = true
                    //    });
                    //}

                    //returnReservationBillAccountingOptionConfiguration.DefaultOption = accountingOption11.Id;
                    //returnReservationBillAccountingOptionConfiguration.DebitOption = accountingOption11.Id;
                    //returnReservationBillAccountingOptionConfiguration.CreditOption = accountingOption11.Id;
                    //financeSetting.ReturnReservationBillAccountingOptionConfiguration = JsonConvert.SerializeObject(returnReservationBillAccountingOptionConfiguration);

                    ////收款单(收款账户)
                    //FinanceAccountingMap receiptAccountingOptionConfiguration = new FinanceAccountingMap();
                    ////现金
                    //if (receiptAccountingOptionConfiguration.Options == null || receiptAccountingOptionConfiguration.Options.Where(so => so.AccountingTypeId == 1 && so.ParentId == accountingOption1.Id && so.Name == "现金").Count() == 0)
                    //{
                    //    receiptAccountingOptionConfiguration.Options.Add(new AccountingOption()
                    //    {
                    //        StoreId = storeId,
                    //        AccountingTypeId = 1,
                    //        ParentId = accountingOption1.Id,
                    //        Name = "现金",
                    //        Code = "100101",
                    //        DisplayOrder = 0,
                    //        Enabled = true,
                    //        IsDefault = true
                    //    });
                    //}

                    ////银行
                    //if (receiptAccountingOptionConfiguration.Options == null || receiptAccountingOptionConfiguration.Options.Where(so => so.AccountingTypeId == 1 && so.ParentId == accountingOption2.Id && so.Name == "银行").Count() == 0)
                    //{
                    //    receiptAccountingOptionConfiguration.Options.Add(new AccountingOption()
                    //    {
                    //        StoreId = storeId,
                    //        AccountingTypeId = 1,
                    //        ParentId = accountingOption2.Id,
                    //        Name = "银行",
                    //        Code = "100201",
                    //        DisplayOrder = 0,
                    //        Enabled = true,
                    //        IsDefault = true
                    //    });
                    //}

                    //receiptAccountingOptionConfiguration.DefaultOption = accountingOption11.Id;
                    //receiptAccountingOptionConfiguration.DebitOption = accountingOption11.Id;
                    //receiptAccountingOptionConfiguration.CreditOption = accountingOption11.Id;
                    //financeSetting.ReceiptAccountingOptionConfiguration = JsonConvert.SerializeObject(receiptAccountingOptionConfiguration);

                    ////付款单(付款账户)
                    //FinanceAccountingMap paymentAccountingOptionConfiguration = new FinanceAccountingMap();
                    ////现金
                    //if (paymentAccountingOptionConfiguration.Options == null || paymentAccountingOptionConfiguration.Options.Where(so => so.AccountingTypeId == 1 && so.ParentId == accountingOption1.Id && so.Name == "现金").Count() == 0)
                    //{
                    //    paymentAccountingOptionConfiguration.Options.Add(new AccountingOption()
                    //    {
                    //        StoreId = storeId,
                    //        AccountingTypeId = 1,
                    //        ParentId = accountingOption1.Id,
                    //        Name = "现金",
                    //        Code = "100101",
                    //        DisplayOrder = 0,
                    //        Enabled = true,
                    //        IsDefault = true
                    //    });
                    //}

                    ////银行
                    //if (paymentAccountingOptionConfiguration.Options == null || paymentAccountingOptionConfiguration.Options.Where(so => so.AccountingTypeId == 1 && so.ParentId == accountingOption2.Id && so.Name == "银行").Count() == 0)
                    //{
                    //    paymentAccountingOptionConfiguration.Options.Add(new AccountingOption()
                    //    {
                    //        StoreId = storeId,
                    //        AccountingTypeId = 1,
                    //        ParentId = accountingOption2.Id,
                    //        Name = "银行",
                    //        Code = "100201",
                    //        DisplayOrder = 0,
                    //        Enabled = true,
                    //        IsDefault = true
                    //    });
                    //}

                    //paymentAccountingOptionConfiguration.DefaultOption = accountingOption11.Id;
                    //paymentAccountingOptionConfiguration.DebitOption = accountingOption11.Id;
                    //paymentAccountingOptionConfiguration.CreditOption = accountingOption11.Id;
                    //financeSetting.PaymentAccountingOptionConfiguration = JsonConvert.SerializeObject(paymentAccountingOptionConfiguration);

                    ////预收款单(收款账户)
                    //FinanceAccountingMap advanceReceiptAccountingOptionConfiguration = new FinanceAccountingMap();
                    ////现金
                    //if (advanceReceiptAccountingOptionConfiguration.Options == null || advanceReceiptAccountingOptionConfiguration.Options.Where(so => so.AccountingTypeId == 1 && so.ParentId == accountingOption1.Id && so.Name == "现金").Count() == 0)
                    //{
                    //    advanceReceiptAccountingOptionConfiguration.Options.Add(new AccountingOption()
                    //    {
                    //        StoreId = storeId,
                    //        AccountingTypeId = 1,
                    //        ParentId = accountingOption1.Id,
                    //        Name = "现金",
                    //        Code = "100101",
                    //        DisplayOrder = 0,
                    //        Enabled = true,
                    //        IsDefault = true
                    //    });
                    //}

                    ////银行
                    //if (advanceReceiptAccountingOptionConfiguration.Options == null || advanceReceiptAccountingOptionConfiguration.Options.Where(so => so.AccountingTypeId == 1 && so.ParentId == accountingOption2.Id && so.Name == "银行").Count() == 0)
                    //{
                    //    advanceReceiptAccountingOptionConfiguration.Options.Add(new AccountingOption()
                    //    {
                    //        StoreId = storeId,
                    //        AccountingTypeId = 1,
                    //        ParentId = accountingOption2.Id,
                    //        Name = "银行",
                    //        Code = "100201",
                    //        DisplayOrder = 0,
                    //        Enabled = true,
                    //        IsDefault = true
                    //    });
                    //}

                    //advanceReceiptAccountingOptionConfiguration.DefaultOption = accountingOption11.Id;
                    //advanceReceiptAccountingOptionConfiguration.DebitOption = accountingOption11.Id;
                    //advanceReceiptAccountingOptionConfiguration.CreditOption = accountingOption11.Id;
                    //financeSetting.AdvanceReceiptAccountingOptionConfiguration = JsonConvert.SerializeObject(advanceReceiptAccountingOptionConfiguration);

                    ////预付款单(付款账户)
                    //FinanceAccountingMap advancePaymentAccountingOptionConfiguration = new FinanceAccountingMap();
                    ////现金
                    //if (advancePaymentAccountingOptionConfiguration.Options == null || advancePaymentAccountingOptionConfiguration.Options.Where(so => so.AccountingTypeId == 1 && so.ParentId == accountingOption1.Id && so.Name == "现金").Count() == 0)
                    //{
                    //    advancePaymentAccountingOptionConfiguration.Options.Add(new AccountingOption()
                    //    {
                    //        StoreId = storeId,
                    //        AccountingTypeId = 1,
                    //        ParentId = accountingOption1.Id,
                    //        Name = "现金",
                    //        Code = "100101",
                    //        DisplayOrder = 0,
                    //        Enabled = true,
                    //        IsDefault = true
                    //    });
                    //}

                    ////银行
                    //if (advancePaymentAccountingOptionConfiguration.Options == null || advancePaymentAccountingOptionConfiguration.Options.Where(so => so.AccountingTypeId == 1 && so.ParentId == accountingOption2.Id && so.Name == "银行").Count() == 0)
                    //{
                    //    advancePaymentAccountingOptionConfiguration.Options.Add(new AccountingOption()
                    //    {
                    //        StoreId = storeId,
                    //        AccountingTypeId = 1,
                    //        ParentId = accountingOption2.Id,
                    //        Name = "银行",
                    //        Code = "100201",
                    //        DisplayOrder = 0,
                    //        Enabled = true,
                    //        IsDefault = true
                    //    });
                    //}

                    //advancePaymentAccountingOptionConfiguration.DefaultOption = accountingOption11.Id;
                    //advancePaymentAccountingOptionConfiguration.DebitOption = accountingOption11.Id;
                    //advancePaymentAccountingOptionConfiguration.CreditOption = accountingOption11.Id;
                    //financeSetting.AdvancePaymentAccountingOptionConfiguration = JsonConvert.SerializeObject(advancePaymentAccountingOptionConfiguration);

                    ////采购单(付款账户)
                    //FinanceAccountingMap purchaseBillAccountingOptionConfiguration = new FinanceAccountingMap();
                    ////现金
                    //if (purchaseBillAccountingOptionConfiguration.Options == null || purchaseBillAccountingOptionConfiguration.Options.Where(so => so.AccountingTypeId == 1 && so.ParentId == accountingOption1.Id && so.Name == "现金").Count() == 0)
                    //{
                    //    purchaseBillAccountingOptionConfiguration.Options.Add(new AccountingOption()
                    //    {
                    //        StoreId = storeId,
                    //        AccountingTypeId = 1,
                    //        ParentId = accountingOption1.Id,
                    //        Name = "现金",
                    //        Code = "100101",
                    //        DisplayOrder = 0,
                    //        Enabled = true,
                    //        IsDefault = true
                    //    });
                    //}

                    ////银行
                    //if (purchaseBillAccountingOptionConfiguration.Options == null || purchaseBillAccountingOptionConfiguration.Options.Where(so => so.AccountingTypeId == 1 && so.ParentId == accountingOption2.Id && so.Name == "银行").Count() == 0)
                    //{
                    //    purchaseBillAccountingOptionConfiguration.Options.Add(new AccountingOption()
                    //    {
                    //        StoreId = storeId,
                    //        AccountingTypeId = 1,
                    //        ParentId = accountingOption2.Id,
                    //        Name = "银行",
                    //        Code = "100201",
                    //        DisplayOrder = 0,
                    //        Enabled = true,
                    //        IsDefault = true
                    //    });
                    //}

                    //purchaseBillAccountingOptionConfiguration.DefaultOption = accountingOption11.Id;
                    //purchaseBillAccountingOptionConfiguration.DebitOption = accountingOption11.Id;
                    //purchaseBillAccountingOptionConfiguration.CreditOption = accountingOption11.Id;
                    //financeSetting.PurchaseBillAccountingOptionConfiguration = JsonConvert.SerializeObject(purchaseBillAccountingOptionConfiguration);

                    ////采购退货单(付款账户)
                    //FinanceAccountingMap purchaseReturnBillAccountingOptionConfiguration = new FinanceAccountingMap();
                    ////现金
                    //if (purchaseReturnBillAccountingOptionConfiguration.Options == null || purchaseReturnBillAccountingOptionConfiguration.Options.Where(so => so.AccountingTypeId == 1 && so.ParentId == accountingOption1.Id && so.Name == "现金").Count() == 0)
                    //{
                    //    purchaseReturnBillAccountingOptionConfiguration.Options.Add(new AccountingOption()
                    //    {
                    //        StoreId = storeId,
                    //        AccountingTypeId = 1,
                    //        ParentId = accountingOption1.Id,
                    //        Name = "现金",
                    //        Code = "100101",
                    //        DisplayOrder = 0,
                    //        Enabled = true,
                    //        IsDefault = true
                    //    });
                    //}

                    ////银行
                    //if (purchaseReturnBillAccountingOptionConfiguration.Options == null || purchaseReturnBillAccountingOptionConfiguration.Options.Where(so => so.AccountingTypeId == 1 && so.ParentId == accountingOption2.Id && so.Name == "银行").Count() == 0)
                    //{
                    //    purchaseReturnBillAccountingOptionConfiguration.Options.Add(new AccountingOption()
                    //    {
                    //        StoreId = storeId,
                    //        AccountingTypeId = 1,
                    //        ParentId = accountingOption2.Id,
                    //        Name = "银行",
                    //        Code = "100201",
                    //        DisplayOrder = 0,
                    //        Enabled = true,
                    //        IsDefault = true
                    //    });
                    //}

                    //purchaseReturnBillAccountingOptionConfiguration.DefaultOption = accountingOption11.Id;
                    //purchaseReturnBillAccountingOptionConfiguration.DebitOption = accountingOption11.Id;
                    //purchaseReturnBillAccountingOptionConfiguration.CreditOption = accountingOption11.Id;
                    //financeSetting.PurchaseReturnBillAccountingOptionConfiguration = JsonConvert.SerializeObject(purchaseReturnBillAccountingOptionConfiguration);

                    ////费用支出(支出账户)
                    //FinanceAccountingMap costExpenditureAccountingOptionConfiguration = new FinanceAccountingMap();
                    ////现金
                    //if (costExpenditureAccountingOptionConfiguration.Options == null || costExpenditureAccountingOptionConfiguration.Options.Where(so => so.AccountingTypeId == 1 && so.ParentId == accountingOption1.Id && so.Name == "现金").Count() == 0)
                    //{
                    //    costExpenditureAccountingOptionConfiguration.Options.Add(new AccountingOption()
                    //    {
                    //        StoreId = storeId,
                    //        AccountingTypeId = 1,
                    //        ParentId = accountingOption1.Id,
                    //        Name = "现金",
                    //        Code = "100101",
                    //        DisplayOrder = 0,
                    //        Enabled = true,
                    //        IsDefault = true
                    //    });
                    //}

                    ////银行
                    //if (costExpenditureAccountingOptionConfiguration.Options == null || costExpenditureAccountingOptionConfiguration.Options.Where(so => so.AccountingTypeId == 1 && so.ParentId == accountingOption2.Id && so.Name == "银行").Count() == 0)
                    //{
                    //    costExpenditureAccountingOptionConfiguration.Options.Add(new AccountingOption()
                    //    {
                    //        StoreId = storeId,
                    //        AccountingTypeId = 1,
                    //        ParentId = accountingOption2.Id,
                    //        Name = "银行",
                    //        Code = "100201",
                    //        DisplayOrder = 0,
                    //        Enabled = true,
                    //        IsDefault = true
                    //    });
                    //}

                    //costExpenditureAccountingOptionConfiguration.DefaultOption = accountingOption11.Id;
                    //costExpenditureAccountingOptionConfiguration.DebitOption = accountingOption11.Id;
                    //costExpenditureAccountingOptionConfiguration.CreditOption = accountingOption11.Id;
                    //financeSetting.CostExpenditureAccountingOptionConfiguration = JsonConvert.SerializeObject(costExpenditureAccountingOptionConfiguration);

                    ////财务收入（收款账户）
                    //FinanceAccountingMap financialIncomeAccountingOptionConfiguration = new FinanceAccountingMap();
                    ////现金
                    //if (financialIncomeAccountingOptionConfiguration.Options == null || financialIncomeAccountingOptionConfiguration.Options.Where(so => so.AccountingTypeId == 1 && so.ParentId == accountingOption1.Id && so.Name == "现金").Count() == 0)
                    //{
                    //    financialIncomeAccountingOptionConfiguration.Options.Add(new AccountingOption()
                    //    {
                    //        StoreId = storeId,
                    //        AccountingTypeId = 1,
                    //        ParentId = accountingOption1.Id,
                    //        Name = "现金",
                    //        Code = "100101",
                    //        DisplayOrder = 0,
                    //        Enabled = true,
                    //        IsDefault = true
                    //    });
                    //}

                    ////银行
                    //if (financialIncomeAccountingOptionConfiguration.Options == null || financialIncomeAccountingOptionConfiguration.Options.Where(so => so.AccountingTypeId == 1 && so.ParentId == accountingOption2.Id && so.Name == "银行").Count() == 0)
                    //{
                    //    financialIncomeAccountingOptionConfiguration.Options.Add(new AccountingOption()
                    //    {
                    //        StoreId = storeId,
                    //        AccountingTypeId = 1,
                    //        ParentId = accountingOption2.Id,
                    //        Name = "银行",
                    //        Code = "100201",
                    //        DisplayOrder = 0,
                    //        Enabled = true,
                    //        IsDefault = true
                    //    });
                    //}

                    //financialIncomeAccountingOptionConfiguration.DefaultOption = accountingOption11.Id;
                    //financialIncomeAccountingOptionConfiguration.DebitOption = accountingOption11.Id;
                    //financialIncomeAccountingOptionConfiguration.CreditOption = accountingOption11.Id;
                    //financeSetting.FinancialIncomeAccountingOptionConfiguration = JsonConvert.SerializeObject(financialIncomeAccountingOptionConfiguration);

                    //_settingService.SaveSetting(financeSetting, x => x.SaleBillAccountingOptionConfiguration, storeId, false);
                    //_settingService.SaveSetting(financeSetting, x => x.SaleReservationBillAccountingOptionConfiguration, storeId, false);

                    //_settingService.SaveSetting(financeSetting, x => x.ReturnBillAccountingOptionConfiguration, storeId, false);
                    //_settingService.SaveSetting(financeSetting, x => x.ReturnReservationBillAccountingOptionConfiguration, storeId, false);

                    //_settingService.SaveSetting(financeSetting, x => x.ReceiptAccountingOptionConfiguration, storeId, false);
                    //_settingService.SaveSetting(financeSetting, x => x.PaymentAccountingOptionConfiguration, storeId, false);

                    //_settingService.SaveSetting(financeSetting, x => x.AdvanceReceiptAccountingOptionConfiguration, storeId, false);
                    //_settingService.SaveSetting(financeSetting, x => x.AdvancePaymentAccountingOptionConfiguration, storeId, false);

                    //_settingService.SaveSetting(financeSetting, x => x.PurchaseBillAccountingOptionConfiguration, storeId, false);
                    //_settingService.SaveSetting(financeSetting, x => x.PurchaseReturnBillAccountingOptionConfiguration, storeId, false);

                    //_settingService.SaveSetting(financeSetting, x => x.CostExpenditureAccountingOptionConfiguration, storeId, false);
                    //_settingService.SaveSetting(financeSetting, x => x.FinancialIncomeAccountingOptionConfiguration, storeId, false);

                    //#endregion

                    #endregion

                    //scope.Complete();
                }

            }
            catch (Exception)
            {
                fg = false;
            }
            return fg;

        }

        public string[] GetNotExistingStores(string[] storeIdsNames)
        {
            if (storeIdsNames == null)
            {
                throw new ArgumentNullException(nameof(storeIdsNames));
            }

            var query = StoreRepository_RO.Table;
            var queryFilter = storeIdsNames.Distinct().ToArray();
            //filtering by name
            var filter = query.Select(store => store.Name).Where(store => queryFilter.Contains(store)).ToList();
            queryFilter = queryFilter.Except(filter).ToArray();

            //if some names not found
            if (!queryFilter.Any())
            {
                return queryFilter.ToArray();
            }

            //filtering by IDs
            filter = query.Select(store => store.Id.ToString()).Where(store => queryFilter.Contains(store)).ToList();
            queryFilter = queryFilter.Except(filter).ToArray();

            return queryFilter.ToArray();
        }

    }
}