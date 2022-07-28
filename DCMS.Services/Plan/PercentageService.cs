using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Data;
using DCMS.Core.Domain.Plan;
using DCMS.Core.Infrastructure.DependencyManagement;
using DCMS.Services.Events;
using DCMS.Services.Products;
using System;
using System.Collections.Generic;
using System.Linq;
using DCMS.Services.Caching;


namespace DCMS.Services.plan
{

    /// <summary>
    /// 表示员工提成计算
    /// </summary>
    public partial class PercentageService : BaseService, IPercentageService
    {

        private readonly ICategoryService _categoryService;
        private readonly IProductService _productService;

        public PercentageService(IStaticCacheManager cacheManager,
            IServiceGetter getter,
            IEventPublisher eventPublisher,
            ICategoryService categoryService,
            IProductService productService
            ) : base(getter, cacheManager, eventPublisher)
        {
            _categoryService = categoryService;
            _productService = productService;
        }


        #region 方法


        /// <summary>
        ///  删除
        /// </summary>
        /// <param name="percentages"></param>
        public virtual void DeletePercentage(Percentage percentages)
        {
            if (percentages == null)
            {
                throw new ArgumentNullException("percentages");
            }

            var uow = PercentageRepository.UnitOfWork;
            PercentageRepository.Delete(percentages);
            uow.SaveChanges();

            //event notification
            _eventPublisher.EntityDeleted(percentages);
        }

        /// <summary>
        /// 获取全部
        /// </summary>
        /// <param name="name"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public virtual IPagedList<Percentage> GetAllPercentages(string name = null, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            if (pageSize >= 50)
                pageSize = 50;
            var query = PercentageRepository.Table;

            if (!string.IsNullOrWhiteSpace(name))
            {
                query = query.Where(c => c.Name.Contains(name));
            }

            query = query.OrderByDescending(c => c.CreatedOnUtc);
            //var percentages = new PagedList<Percentage>(query.ToList(), pageIndex, pageSize);
            //return percentages;

            //总页数
            var totalCount = query.Count();
            var plists = query.Skip(pageIndex * pageSize).Take(pageSize).ToList();
            return new PagedList<Percentage>(plists, pageIndex, pageSize, totalCount);

        }


        /// <summary>
        /// 获取全部
        /// </summary>
        /// <returns></returns>
        public virtual IList<Percentage> GetAllPercentages(int? store)
        {
            var key = DCMSDefaults.PERCENTAGE_ALL_KEY.FillCacheKey(store ?? 0);
            return _cacheManager.Get(key, () =>
            {
                var query = from s in PercentageRepository.Table
                            where s.StoreId == store.Value
                            orderby s.CreatedOnUtc, s.Name
                            select s;
                var percentage = query.ToList();
                return percentage;
            });
        }

        /// <summary>
        /// 获取全部
        /// </summary>
        /// <returns></returns>
        public virtual IList<Percentage> GetAllPercentages()
        {
            var key = DCMSDefaults.PERCENTAGE_ALL_KEY.FillCacheKey(0);
            return _cacheManager.Get(key, () =>
            {
                var query = from s in PercentageRepository.Table
                            orderby s.CreatedOnUtc, s.Name
                            select s;
                var percentage = query.ToList();
                return percentage;
            });
        }

        /// <summary>
        /// 获取
        /// </summary>
        /// <param name="percentagesId"></param>
        /// <returns></returns>
        public virtual Percentage GetPercentageById(int? store, int percentagesId)
        {
            if (percentagesId == 0)
            {
                return null;
            }

            return PercentageRepository.ToCachedGetById(percentagesId);
        }


        /// <summary>
        /// 获取商品提成方案
        /// </summary>
        /// <param name="plan"></param>
        /// <param name="product"></param>
        /// <returns></returns>
        public virtual Percentage GetPercentageByProduct(int store, int plan, int productId)
        {
            var key = DCMSDefaults.GETPERCENTAGE_BY_BYPRODUCT_KEY.FillCacheKey(store, plan, productId);
            return _cacheManager.Get(key, () =>
            {
                var query = from c in PercentageRepository.Table
                            where c.StoreId == store && c.PercentagePlanId == plan && c.ProductId == productId && productId > 0
                            select c;
                var percentage = query.FirstOrDefault();
                return percentage;
            });
        }


        /// <summary>
        /// 获取商品分类提成方案
        /// </summary>
        /// <param name="plan"></param>
        /// <param name="product"></param>
        /// <returns></returns>
        public virtual Percentage GetPercentageByCatagory(int store, int plan, int catagoryId)
        {
            var key = DCMSDefaults.GETPERCENTAGE_BY_BYCATAGORY_KEY.FillCacheKey(store, plan, catagoryId);
            return _cacheManager.Get(key, () =>
            {
                var query = from c in PercentageRepository.Table
                            where c.StoreId == store && c.PercentagePlanId == plan && c.CatagoryId == catagoryId && catagoryId > 0
                            select c;
                var percentage = query.FirstOrDefault();
                return percentage;
            });
        }

        public virtual IList<Percentage> GetPercentageByIds(int? store, int[] ids)
        {
            if (ids == null || ids.Length == 0)
            {
                return new List<Percentage>();
            }

            var key = DCMSDefaults.PERCENTAGES_BY_IDS_KEY.FillCacheKey(store ?? 0, ids.OrderBy(a => a));
            return _cacheManager.Get(key, () =>
            {

                var query = from c in PercentageRepository.Table
                            where ids.Contains(c.Id)
                            select c;
                var percentages = query.ToList();
                return percentages;
            });

        }

        public virtual Percentage GetPercentageByCatagoryId(int store, int catagoryId)
        {
            if (catagoryId == 0)
            {
                return null;
            }

            var query = from s in PercentageRepository.Table
                        where s.StoreId == store && s.CatagoryId == catagoryId
                        orderby s.CreatedOnUtc, s.Name
                        select s;
            var percentage = query.FirstOrDefault();
            return percentage;
        }
        public virtual Percentage GetPercentageByProductId(int store, int productId)
        {
            if (productId == 0)
            {
                return null;
            }

            var query = from s in PercentageRepository.Table
                        where s.StoreId == store && s.ProductId == productId
                        orderby s.CreatedOnUtc, s.Name
                        select s;
            var percentage = query.FirstOrDefault();
            return percentage;
        }

        public virtual Percentage GetPercentageByCatagoryId(int store, int percentagePlanId, int catagoryId)
        {
            if (catagoryId == 0 || percentagePlanId == 0)
            {
                return null;
            }

            var query = from s in PercentageRepository.Table
                        where s.StoreId == store && s.CatagoryId == catagoryId && s.PercentagePlanId == percentagePlanId
                        orderby s.CreatedOnUtc, s.Name
                        select s;
            var percentage = query.FirstOrDefault();
            return percentage;
        }
        public virtual Percentage GetPercentageByProductId(int store, int percentagePlanId, int productId)
        {
            if (productId == 0 || percentagePlanId == 0)
            {
                return null;
            }

            var query = from s in PercentageRepository.Table
                        where s.StoreId == store && s.ProductId == productId && s.PercentagePlanId == percentagePlanId
                        orderby s.CreatedOnUtc, s.Name
                        select s;
            var percentage = query.FirstOrDefault();
            return percentage;
        }



        public virtual IList<Percentage> GetPercentagesByPlanId(int pid)
        {
            var query = from c in PercentageRepository.Table
                        where c.PercentagePlanId == pid
                        select c;
            var percentage = query.ToList();
            return percentage;
        }


        public virtual IList<Percentage> GetPercentagesByIds(int[] sIds)
        {
            if (sIds == null || sIds.Length == 0)
            {
                return new List<Percentage>();
            }

            var query = from c in PercentageRepository.Table
                        where sIds.Contains(c.Id)
                        select c;
            var percentage = query.ToList();

            var sortedPercentages = new List<Percentage>();
            foreach (int id in sIds)
            {
                var percentages = percentage.Find(x => x.Id == id);
                if (percentages != null)
                {
                    sortedPercentages.Add(percentages);
                }
            }
            return sortedPercentages;
        }

        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="percentages"></param>
        public virtual void InsertPercentage(Percentage percentages)
        {
            if (percentages == null)
            {
                throw new ArgumentNullException("percentages");
            }

            var uow = PercentageRepository.UnitOfWork;
            PercentageRepository.Insert(percentages);
            uow.SaveChanges();

            //event notification
            _eventPublisher.EntityInserted(percentages);
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="percentages"></param>
        public virtual void UpdatePercentage(Percentage percentages)
        {
            if (percentages == null)
            {
                throw new ArgumentNullException("percentages");
            }

            var uow = PercentageRepository.UnitOfWork;
            PercentageRepository.Update(percentages);
            uow.SaveChanges();

            //event notification
            _eventPublisher.EntityUpdated(percentages);
        }

        #endregion


        #region 区间范围


        /// <summary>
        ///  删除
        /// </summary>
        /// <param name="percentageRangeOptions"></param>
        public virtual void DeletePercentageRangeOption(PercentageRangeOption percentageRangeOptions)
        {
            if (percentageRangeOptions == null)
            {
                throw new ArgumentNullException("percentageRangeOptions");
            }

            var uow = PercentageRangeOptionRepository.UnitOfWork;
            PercentageRangeOptionRepository.Delete(percentageRangeOptions);
            uow.SaveChanges();

            //event notification
            _eventPublisher.EntityDeleted(percentageRangeOptions);
        }


        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="percentageId"></param>
        public virtual void DeletePercentageRangeOptionByPercentageId(int? percentageId)
        {
            var query = PercentageRangeOptionRepository.Table;

            if (percentageId.HasValue)
            {
                query = query.Where(c => c.PercentageId == percentageId);
            }

            query = query.OrderByDescending(c => c.Id);

            var percentageRangeOptions = query.ToList();

            foreach (var option in percentageRangeOptions)
            {
                DeletePercentageRangeOption(option);
            }
        }



        /// <summary>
        /// 获取全部
        /// </summary>
        /// <param name="name"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public virtual IPagedList<PercentageRangeOption> GetAllPercentageRangeOptions(int? percentageId, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            if (pageSize >= 50)
                pageSize = 50;
            var query = PercentageRangeOptionRepository.Table;

            if (percentageId.HasValue)
            {
                query = query.Where(c => c.PercentageId == percentageId);
            }

            query = query.OrderByDescending(c => c.Id);
            //var percentageRangeOptions = new PagedList<PercentageRangeOption>(query.ToList(), pageIndex, pageSize);
            //return percentageRangeOptions;

            //总页数
            var totalCount = query.Count();
            var plists = query.Skip(pageIndex * pageSize).Take(pageSize).ToList();
            return new PagedList<PercentageRangeOption>(plists, pageIndex, pageSize, totalCount);

        }


        /// <summary>
        /// 获取全部
        /// </summary>
        /// <returns></returns>
        public virtual IList<PercentageRangeOption> GetAllPercentageRangeOptionsByPercentageId(int? store, int? percentageId)
        {
            var key = DCMSDefaults.PERCENTAGE_OPTION_ALL_KEY.FillCacheKey(store ?? 0, percentageId);
            return _cacheManager.Get(key, () =>
            {
                var query = from s in PercentageRangeOptionRepository.Table
                            where s.PercentageId == percentageId.Value
                            orderby s.Id
                            select s;
                var percentageRangeOption = query.ToList();
                return percentageRangeOption;
            });
        }

        public virtual IList<PercentageRangeOption> GetAllPercentageRangeOptionsByPercentageIds(int? store, int[] ids)
        {
            if (ids == null || ids.Length == 0)
            {
                return new List<PercentageRangeOption>();
            }

            var key = DCMSDefaults.PERCENTAGERANGEOPTIONS_BY_IDS_KEY.FillCacheKey(store ?? 0, ids.OrderBy(a => a));
            return _cacheManager.Get(key, () =>
            {

                var query = from s in PercentageRangeOptionRepository.Table
                            where ids.Contains(s.PercentageId)
                            orderby s.Id
                            select s;
                var percentageRangeOption = query.ToList();
                return percentageRangeOption;
            });

        }

        /// <summary>
        /// 获取全部
        /// </summary>
        /// <returns></returns>
        public virtual IList<PercentageRangeOption> GetAllPercentageRangeOptions()
        {
            var key = DCMSDefaults.PERCENTAGE_OPTION_ALL_KEY.FillCacheKey(0, 0);
            return _cacheManager.Get(key, () =>
            {
                var query = from s in PercentageRangeOptionRepository.Table
                            orderby s.Id
                            select s;
                var percentageRangeOption = query.ToList();
                return percentageRangeOption;
            });
        }

        /// <summary>
        /// 获取
        /// </summary>
        /// <param name="percentageRangeOptionsId"></param>
        /// <returns></returns>
        public virtual PercentageRangeOption GetPercentageRangeOptionById(int? store, int percentageRangeOptionsId)
        {
            if (percentageRangeOptionsId == 0)
            {
                return null;
            }

            return PercentageRangeOptionRepository.ToCachedGetById(percentageRangeOptionsId);
        }


        /// <summary>
        /// 获取
        /// </summary>
        /// <param name="sIds"></param>
        /// <returns></returns>
        public virtual IList<PercentageRangeOption> GetPercentageRangeOptionsByIds(int[] sIds)
        {
            if (sIds == null || sIds.Length == 0)
            {
                return new List<PercentageRangeOption>();
            }

            var query = from c in PercentageRangeOptionRepository.Table
                        where sIds.Contains(c.Id)
                        select c;
            var percentageRangeOption = query.ToList();

            var sortedPercentageRangeOptions = new List<PercentageRangeOption>();
            foreach (int id in sIds)
            {
                var percentageRangeOptions = percentageRangeOption.Find(x => x.Id == id);
                if (percentageRangeOptions != null)
                {
                    sortedPercentageRangeOptions.Add(percentageRangeOptions);
                }
            }
            return sortedPercentageRangeOptions;
        }

        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="percentageRangeOptions"></param>
        public virtual void InsertPercentageRangeOption(PercentageRangeOption percentageRangeOptions)
        {
            if (percentageRangeOptions == null)
            {
                throw new ArgumentNullException("percentageRangeOptions");
            }

            var uow = PercentageRangeOptionRepository.UnitOfWork;
            PercentageRangeOptionRepository.Insert(percentageRangeOptions);
            uow.SaveChanges();

            //event notification
            _eventPublisher.EntityInserted(percentageRangeOptions);
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="percentageRangeOptions"></param>
        public virtual void UpdatePercentageRangeOption(PercentageRangeOption percentageRangeOptions)
        {
            if (percentageRangeOptions == null)
            {
                throw new ArgumentNullException("percentageRangeOptions");
            }

            var uow = PercentageRangeOptionRepository.UnitOfWork;
            PercentageRangeOptionRepository.Update(percentageRangeOptions);
            uow.SaveChanges();

            //event notification
            _eventPublisher.EntityUpdated(percentageRangeOptions);
        }

        #endregion



        public BaseResult CreateOrUpdate(int storeId, int userId, int? percentageId, Percentage data, List<PercentageRangeOption> percentageRangeOptions)
        {
            var uow = PercentageRepository.UnitOfWork;

            ITransaction transaction = null;
            try
            {

                transaction = uow.BeginOrUseTransaction();



                Percentage percentage = new Percentage();

                List<PercentageRangeOption> newOption = percentageRangeOptions;
                List<PercentageRangeOption> oldOption = new List<PercentageRangeOption>();

                if (percentageId.HasValue && percentageId != 0)
                {
                    percentage = GetPercentageById(storeId, percentageId.Value);
                    oldOption = GetAllPercentageRangeOptionsByPercentageId(storeId, percentageId ?? 0).ToList();
                }
                if (data != null)
                {
                    percentage.PercentagePlanId = data.PercentagePlanId;
                    percentage.StoreId = storeId;
                    percentage.CatagoryId = data.CatagoryId;
                    percentage.ProductId = data.ProductId;
                    percentage.CalCulateMethodId = data.CalCulateMethodId;
                    percentage.Name = data.Name;
                    percentage.IsReturnCalCulated = data.IsReturnCalCulated;

                    switch (percentage.CalCulateMethodId)
                    {
                        //销售额百分比
                        case 1:
                            {
                                percentage.IsGiftCalCulated = false;
                                percentage.SalesPercent = data.SalesPercent;
                                percentage.ReturnPercent = data.ReturnPercent;
                                percentage.SalesAmount = 0;
                                percentage.ReturnAmount = 0;
                                percentage.QuantityCalCulateMethodId = 0;
                                percentage.CostingCalCulateMethodId = 0;
                                percentage.CalCulateMethodId = data.CalCulateMethodId;
                            }
                            break;
                        //销售额变化百分比
                        case 2:
                            {
                                percentage.IsGiftCalCulated = false;
                                percentage.SalesPercent = 0;
                                percentage.ReturnPercent = 0;
                                percentage.SalesAmount = 0;
                                percentage.ReturnAmount = 0;
                                percentage.QuantityCalCulateMethodId = 0;
                                percentage.CostingCalCulateMethodId = 0;
                                percentage.CalCulateMethodId = data.CalCulateMethodId;
                            }
                            break;
                        //销售额分段变化百分比
                        case 3:
                            {
                                percentage.IsGiftCalCulated = false;
                                percentage.SalesPercent = 0;
                                percentage.ReturnPercent = 0;
                                percentage.SalesAmount = 0;
                                percentage.ReturnAmount = 0;
                                percentage.QuantityCalCulateMethodId = 0;
                                percentage.CostingCalCulateMethodId = 0;
                                percentage.CalCulateMethodId = data.CalCulateMethodId;
                            }
                            break;
                        //销售数量每件固定额
                        case 4:
                            {
                                percentage.IsGiftCalCulated = data.IsGiftCalCulated;
                                percentage.SalesPercent = 0;
                                percentage.ReturnPercent = 0;
                                percentage.SalesAmount = data.SalesAmount;
                                percentage.ReturnAmount = data.ReturnAmount;
                                percentage.CalCulateMethodId = data.CalCulateMethodId;
                                percentage.QuantityCalCulateMethodId = data.QuantityCalCulateMethodId;
                                percentage.CostingCalCulateMethodId = 0;
                            }
                            break;
                        //按销售数量变化每件提成金额
                        case 5:
                            {
                                percentage.IsGiftCalCulated = data.IsGiftCalCulated;
                                percentage.SalesPercent = 0;
                                percentage.ReturnPercent = 0;
                                percentage.SalesAmount = 0;
                                percentage.ReturnAmount = 0;
                                percentage.QuantityCalCulateMethodId = data.QuantityCalCulateMethodId;
                                percentage.CostingCalCulateMethodId = 0;
                                percentage.CalCulateMethodId = data.CalCulateMethodId;
                            }
                            break;
                        //按销售数量分段变化每件提成金额
                        case 6:
                            {
                                percentage.IsGiftCalCulated = data.IsGiftCalCulated;
                                percentage.SalesPercent = 0;
                                percentage.ReturnPercent = 0;
                                percentage.SalesAmount = 0;
                                percentage.ReturnAmount = 0;
                                percentage.QuantityCalCulateMethodId = data.QuantityCalCulateMethodId;
                                percentage.CostingCalCulateMethodId = 0;
                                percentage.CalCulateMethodId = data.CalCulateMethodId;
                            }
                            break;
                        //利润额百分比
                        case 7:
                            {
                                percentage.IsGiftCalCulated = data.IsGiftCalCulated;
                                percentage.SalesPercent = data.SalesPercent;
                                percentage.ReturnPercent = data.ReturnPercent;
                                percentage.SalesAmount = 0;
                                percentage.ReturnAmount = 0;
                                percentage.QuantityCalCulateMethodId = 0;
                                percentage.CostingCalCulateMethodId = data.CostingCalCulateMethodId;
                                percentage.CalCulateMethodId = data.CalCulateMethodId;
                            }
                            break;
                        //利润额变化百分比
                        case 8:
                            {
                                percentage.IsGiftCalCulated = data.IsGiftCalCulated;
                                percentage.SalesPercent = 0;
                                percentage.ReturnPercent = 0;
                                percentage.SalesAmount = 0;
                                percentage.ReturnAmount = 0;
                                percentage.QuantityCalCulateMethodId = 0;
                                percentage.CostingCalCulateMethodId = data.CostingCalCulateMethodId;
                                percentage.CalCulateMethodId = data.CalCulateMethodId;
                            }
                            break;
                        //利润额分段变化百分比
                        case 9:
                            {
                                percentage.IsGiftCalCulated = data.IsGiftCalCulated;
                                percentage.SalesPercent = 0;
                                percentage.ReturnPercent = 0;
                                percentage.SalesAmount = 0;
                                percentage.ReturnAmount = 0;
                                percentage.QuantityCalCulateMethodId = 0;
                                percentage.CostingCalCulateMethodId = data.CostingCalCulateMethodId;
                                percentage.CalCulateMethodId = data.CalCulateMethodId;
                            }
                            break;
                    }
                }

                if (percentageId.HasValue && percentageId != 0)
                {
                    UpdatePercentage(percentage);

                    newOption.ForEach(n =>
                    {
                        var option = GetPercentageRangeOptionById(storeId, n.Id);
                        //不存在插入
                        if (option == null)
                        {
                            n.PercentageId = percentage.Id;
                            n.StoreId = storeId;
                            InsertPercentageRangeOption(n);
                        }
                        //存在更新
                        else
                        {
                            option.NetSalesRange = n.NetSalesRange;
                            option.SalesPercent = n.SalesPercent;
                            option.ReturnPercent = n.ReturnPercent;
                            UpdatePercentageRangeOption(option);
                        }
                    });

                    //删除
                    oldOption.ForEach(o =>
                    {
                        if (newOption.Count(n => n.Id == o.Id) == 0)
                        {
                            var sd = GetPercentageRangeOptionById(storeId, o.Id);
                            if (sd != null)
                            {
                                DeletePercentageRangeOption(sd);
                            }
                        }
                    });

                }
                else
                {
                    percentage.CreatedOnUtc = DateTime.Now;

                    if (percentage.CatagoryId.HasValue && percentage.CatagoryId != 0)
                    {
                        percentage.ProductId = 0;
                    }

                    if (percentage.ProductId.HasValue && percentage.ProductId != 0)
                    {
                        percentage.CatagoryId = 0;
                    }

                    InsertPercentage(percentage);

                    //添加分段
                    if (newOption != null && newOption.Count > 0)
                    {
                        foreach (var option in newOption)
                        {
                            option.StoreId = storeId;
                            option.PercentageId = percentage.Id;
                            InsertPercentageRangeOption(option);
                        }
                    }

                    if (percentage.CatagoryId.HasValue && percentage.CatagoryId != 0)
                    {
                        var catagory = _categoryService.GetCategoryById(storeId, percentage.CatagoryId ?? 0);
                        if (catagory != null)
                        {
                            catagory.PercentageId = percentage.Id;
                            _categoryService.UpdateCategory(catagory);
                        }
                    }

                    if (percentage.ProductId.HasValue && percentage.ProductId != 0)
                    {
                        var product = _productService.GetProductById(storeId, percentage.ProductId ?? 0);
                        if (product != null)
                        {
                            product.PercentageId = percentage.Id;
                            _productService.UpdateProduct(product);
                        }
                    }

                }


                //保存事务
                transaction.Commit();

                return new BaseResult { Success = true, Message = "配置成功" };
            }
            catch (Exception)
            {
                //如果事务不存在或者为控则回滚
                transaction?.Rollback();
                return new BaseResult { Success = false, Message = "配置失败" };
            }
            finally
            {
                //不管怎样最后都会关闭掉这个事务
                using (transaction) { }
            }
        }

        public BaseResult Reset(int storeId, int userId, Percentage percentage)
        {
            var uow = PercentageRepository.UnitOfWork;

            ITransaction transaction = null;
            try
            {

                transaction = uow.BeginOrUseTransaction();



                percentage.CostingCalCulateMethodId = 0;
                percentage.CalCulateMethodId = 0;
                percentage.QuantityCalCulateMethodId = 0;
                percentage.IsReturnCalCulated = true;
                percentage.IsGiftCalCulated = true;
                percentage.SalesPercent = 0;
                percentage.ReturnPercent = 0;
                percentage.SalesAmount = 0;
                percentage.ReturnAmount = 0;
                UpdatePercentage(percentage);

                var options = GetAllPercentageRangeOptionsByPercentageId(storeId, percentage.Id).ToList();

                foreach (var option in options)
                {
                    option.NetSalesRange = 0;
                    option.SalesPercent = 0;
                    option.ReturnPercent = 0;
                    UpdatePercentageRangeOption(option);
                }


                //保存事务
                transaction.Commit();

                return new BaseResult { Success = true, Message = "重置成功" };
            }
            catch (Exception)
            {
                //如果事务不存在或者为控则回滚
                transaction?.Rollback();
                return new BaseResult { Success = false, Message = "重置失败" };
            }
            finally
            {
                //不管怎样最后都会关闭掉这个事务
                using (transaction) { }
            }
        }

    }
}