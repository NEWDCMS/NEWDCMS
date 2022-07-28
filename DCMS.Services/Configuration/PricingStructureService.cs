using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Data;
using DCMS.Core.Domain.Configuration;
using DCMS.Core.Infrastructure.DependencyManagement;
using DCMS.Services.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DCMS.Services.Caching;

namespace DCMS.Services.Configuration
{

    public partial class PricingStructureService : BaseService, IPricingStructureService
    {

        public PricingStructureService(IStaticCacheManager cacheManager,
            IServiceGetter getter,
            IEventPublisher eventPublisher) : base(getter, cacheManager, eventPublisher)
        {

        }


        #region 方法


        public virtual void DeletePricingStructure(PricingStructure pricingStructures)
        {
            if (pricingStructures == null)
            {
                throw new ArgumentNullException("pricingStructures");
            }

            var uow = PricingStructuresRepository.UnitOfWork;
            PricingStructuresRepository.Delete(pricingStructures);
            uow.SaveChanges();

            //event notification
            _eventPublisher.EntityDeleted(pricingStructures);
        }


        public virtual IPagedList<PricingStructure> GetAllPricingStructures(int? store, int? type, string name = null, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            if (pageSize >= 50)
                pageSize = 50;
            var query = PricingStructuresRepository.Table;

            if (!string.IsNullOrWhiteSpace(name))
            {
                query = query.Where(c => c.CustomerName.Contains(name));
            }

            if (store.HasValue && store.Value != 0)
            {
                query = query.Where(c => c.StoreId == store);
            }

            if (type.HasValue)
            {
                query = query.Where(c => c.PriceType == type);
            }

            query = query.OrderByDescending(c => c.CreatedOnUtc);
            //var pricingStructures = new PagedList<PricingStructure>(query.ToList(), pageIndex, pageSize);
            //return pricingStructures;
            //总页数
            var totalCount = query.Count();
            var plists = query.Skip(pageIndex * pageSize).Take(pageSize).ToList();
            return new PagedList<PricingStructure>(plists, pageIndex, pageSize, totalCount);
        }


        public virtual IList<PricingStructure> GetAllPricingStructures(int? store)
        {
            var key = DCMSDefaults.PRICESTRUCTURE_ALL_KEY.FillCacheKey(store);
            return _cacheManager.Get(key, () =>
            {
                var query = from s in PricingStructuresRepository.Table
                            where s.StoreId == store.Value
                            orderby s.CreatedOnUtc, s.Order
                            select s;
                var pricingStructure = query.ToList();
                return pricingStructure;
            });
        }

        public virtual PricingStructure GetPricingStructureById(int? store, int pricingStructuresId)
        {
            if (pricingStructuresId == 0)
            {
                return null;
            }

            return PricingStructuresRepository.ToCachedGetById(pricingStructuresId);
        }


        public virtual IList<PricingStructure> GetPricingStructuresByIds(int[] sIds)
        {
            if (sIds == null || sIds.Length == 0)
            {
                return new List<PricingStructure>();
            }

            var query = from c in PricingStructuresRepository.Table
                        where sIds.Contains(c.Id)
                        select c;
            var pricingStructure = query.ToList();

            var sortedPricingStructures = new List<PricingStructure>();
            foreach (int id in sIds)
            {
                var pricingStructures = pricingStructure.Find(x => x.Id == id);
                if (pricingStructures != null)
                {
                    sortedPricingStructures.Add(pricingStructures);
                }
            }
            return sortedPricingStructures;
        }



        public virtual void InsertPricingStructure(PricingStructure pricingStructures)
        {
            if (pricingStructures == null)
            {
                throw new ArgumentNullException("pricingStructures");
            }

            var uow = PricingStructuresRepository.UnitOfWork;
            PricingStructuresRepository.Insert(pricingStructures);
            uow.SaveChanges();

            //event notification
            _eventPublisher.EntityInserted(pricingStructures);
        }


        public virtual void UpdatePricingStructure(PricingStructure pricingStructures)
        {
            if (pricingStructures == null)
            {
                throw new ArgumentNullException("pricingStructures");
            }

            var uow = PricingStructuresRepository.UnitOfWork;
            PricingStructuresRepository.Update(pricingStructures);
            uow.SaveChanges();

            //event notification
            _eventPublisher.EntityUpdated(pricingStructures);
        }


        public virtual async Task<IPagedList<PricingStructure>> AsyncGetAllPricingStructures(int? store, int? type, string name = null, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            if (pageSize >= 50)
                pageSize = 50;
            return await Task.Run(() =>
            {
                return GetAllPricingStructures(store, type, name, pageIndex, pageSize);
            });
        }
        public virtual async Task<IList<PricingStructure>> AsyncGetAllPricingStructures(int? store)
        {
            return await Task.Run(() =>
            {
                return GetAllPricingStructures(store);
            });
        }
        public virtual async Task<PricingStructure> AsyncGetPricingStructureById(int pricingStructuresId)
        {
            return await Task.Run(() =>
            {
                return GetPricingStructureById(0, pricingStructuresId);
            });
        }
        public virtual async Task<IList<PricingStructure>> AsyncGetPricingStructuresByIds(int[] sIds)
        {
            return await Task.Run(() =>
            {
                return GetPricingStructuresByIds(sIds);
            });
        }


        public BaseResult CreateOrUpdate(int storeId, int userId, List<PricingStructure> pricingStructures)
        {
            var uow = PricingStructuresRepository.UnitOfWork;

            ITransaction transaction = null;
            try
            {

                transaction = uow.BeginOrUseTransaction();



                #region 插入、修改
                var allPrices = GetAllPricingStructures(storeId);
                pricingStructures.ForEach(p =>
                {
                    var pricingStructure = GetPricingStructureById(storeId, p.Id);
                    if (pricingStructure == null)
                    {
                        if (allPrices.Count(cp => cp.Id == p.Id) == 0)
                        {
                            pricingStructure = p;
                            pricingStructure.StoreId = storeId;
                            pricingStructure.CreatedOnUtc = DateTime.Now;
                            InsertPricingStructure(pricingStructure);
                            //不排除
                            p.Id = pricingStructure.Id;
                            //allPrices.Add(price);
                            if (!allPrices.Select(s => s.Id).Contains(pricingStructure.Id))
                            {
                                allPrices.Add(pricingStructure);
                            }
                        }
                    }
                    else
                    {
                        pricingStructure.PriceType = p.PriceType;
                        pricingStructure.CustomerId = p.CustomerId;
                        pricingStructure.CustomerName = p.CustomerName;
                        pricingStructure.ChannelId = p.ChannelId;
                        pricingStructure.DistrictName = p.DistrictName;
                        pricingStructure.DistrictIds = p.DistrictIds;
                        pricingStructure.EndPointLevel = p.EndPointLevel;
                        pricingStructure.PreferredPrice = p.PreferredPrice;
                        pricingStructure.SecondaryPrice = p.SecondaryPrice;
                        pricingStructure.FinalPrice = p.FinalPrice;
                        pricingStructure.Order = p.Order;
                        UpdatePricingStructure(pricingStructure);
                    }
                });

                #endregion

                #region Grid 移除则从库移除删除项

                allPrices.ToList().ForEach(p =>
                {
                    if (pricingStructures.Count(cp => cp.Id == p.Id) == 0)
                    {
                        allPrices.Remove(p);
                        var pricingStructure = GetPricingStructureById(storeId, p.Id);
                        if (pricingStructure != null)
                        {
                            DeletePricingStructure(pricingStructure);
                        }
                    }
                });

                #endregion


                //保存事务
                transaction.Commit();

                return new BaseResult { Success = true, Message = "价格体系创建/更新成功" };
            }
            catch (Exception)
            {
                //如果事务不存在或者为控则回滚
                transaction?.Rollback();
                return new BaseResult { Success = false, Message = "价格体系创建/更新失败" };
            }
            finally
            {
                //不管怎样最后都会关闭掉这个事务
                using (transaction) { }
            }
        }


        #endregion
    }
}