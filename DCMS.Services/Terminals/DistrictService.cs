using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Domain.Common;
using DCMS.Core.Domain.Terminals;
using DCMS.Core.Infrastructure.DependencyManagement;
using DCMS.Services.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using DCMS.Services.Caching;


namespace DCMS.Services.Terminals
{
    /// <summary>
    ///  片区信息服务
    /// </summary>
    public partial class DistrictService : BaseService, IDistrictService
    {
        public DistrictService(IServiceGetter getter,
            IStaticCacheManager cacheManager,
            IEventPublisher eventPublisher) : base(getter, cacheManager, eventPublisher)
        {
            
        }



        #region 方法

        /// <summary>
        /// 根据主键Id获取片区信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual District GetDistrictById(int? store, int id)
        {
            if (id == 0)
            {
                return null;
            }

            return DistrictsRepository.ToCachedGetById(id);
        }

        public virtual IList<District> GetDistrictByIds(int? store, int[] ids)
        {

            if (ids == null || ids.Length == 0)
            {
                return new List<District>();
            }

            var key = DCMSDefaults.DISTRICT_BY_IDS_KEY.FillCacheKey(store ?? 0, ids.OrderBy(a => a));
            return _cacheManager.Get(key, () =>
            {

                var query = from c in DistrictsRepository.Table
                            where ids.Contains(c.Id)
                            select c;
                var percentages = query.ToList();
                return percentages;
            });

        }


        public virtual int GetDistrictByName(int store, string name)
        {
            var query = DistrictsRepository.Table;
            if (string.IsNullOrWhiteSpace(name))
            {
                return 0;
            }

            return query.Where(s => s.StoreId == store && s.Name == name).Select(s => s.Id).FirstOrDefault();
        }

        /// <summary>
        /// 根据父Id获取片区信息
        /// </summary>
        /// <param name="parentId"></param>
        /// <returns></returns>
        public virtual IList<District> GetDistrictByParentId(int? storeId, int parentId)
        {
            //if (parentId == 0)
            //    return null;
            var query = from c in DistrictsRepository_RO.TableNoTracking
                        where !c.Deleted && c.ParentId == parentId && c.StoreId == storeId.Value
                        select c;
            return query.ToList();
        }


        /// <summary>
        /// 获取全部片区信息
        /// </summary>
        /// <returns></returns>
        public virtual IList<District> GetAll(int? storeId)
        {
            return GetAllDistrictByStoreId(storeId);
        }

        /// <summary>
        /// 绑定片区信息
        /// </summary>
        /// <param name="storeId"></param>
        /// <returns></returns>
        public virtual IList<District> BindDistrictList(int? storeId)
        {
            return _cacheManager.Get(DCMSDefaults.BINDDISTRICT_ALL_STORE_KEY.FillCacheKey(storeId), () =>
           {
               var query = from c in DistrictsRepository.Table
                           where !c.Deleted
                           select c;
               if (storeId != null)
               {
                   query = query.Where(c => c.StoreId == storeId);
               }

               var result = query.Select(q => new { Id = q.Id, Name = q.Name }).ToList().Select(x => new District { Id = x.Id, Name = x.Name }).ToList();
               return result;
           });
        }

        /// <summary>
        /// 获取全部经销商片区
        /// </summary>
        /// <param name="store"></param>
        /// <returns></returns>
        public virtual IList<District> GetAllDistrictByStoreId(int? storeId)
        {
            return _cacheManager.Get(DCMSDefaults.DISTRICT_ALL_STORE_KEY.FillCacheKey(storeId), () =>
           {
               var query = from c in DistrictsRepository.Table
                           where c.Deleted == false && c.StoreId == storeId.Value
                           select c;

               return query.ToList();
           });
        }

        /// <summary>
        /// 获取指定用户终端线路所包含的片区
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public virtual IList<District> GetAllDistrictByStoreId(int? storeId, int? userId)
        {
            if (userId.HasValue && userId.Value != 0)
            {
                ///////////////////////////////////////不查询自己创建的片区故注释//////////////////////////////////////////////////
                //var queryDistricts = from c in TerminalsRepository.Table
                //                     where c.StoreId == storeId.Value && c.CreatedUserId == userId.Value
                //                     group c by c.DistrictId into g
                //                     select g.Key;

                //var ids = queryDistricts.Where(s => s > 0).ToList();
                //////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                //如果创建人不是自己，获取当前用户所在片区
                var tids = UserDistrictsRepository.Table
               .Where(s => s.StoreId == storeId && s.UserId == userId)
               .OrderBy(s => s.DistrictId)
               .Select(s => s.DistrictId)
               .ToList();

                var tmpids = tids.Where(s => s > 0).ToList();
                ///////////////////////////////////////不查询自己创建的片区故注释//////////////////////////////////////////////////
                //if (ids != null && ids.Any())
                //{
                //    if (tmpids != null && tmpids.Any())
                //        ids.AddRange(tmpids);

                //    var query = from c in DistrictsRepository.Table
                //                where c.Deleted == false && c.StoreId == storeId.Value && ids.Contains(c.Id)
                //                select c;

                //    return query.ToList();
                //}
                //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                if (tmpids != null && tmpids.Any())
                {
                    var query = from c in DistrictsRepository.Table
                                where c.Deleted == false && c.StoreId == storeId.Value && tmpids.Contains(c.Id)
                                select c;
                    return query.ToList();
                }
                else
                {
                    var query = from c in DistrictsRepository.Table
                                where c.Deleted == false && c.StoreId == storeId.Value
                                select c;
                    return query.ToList();
                }
            }
            else
            {
                var query = from c in DistrictsRepository.Table
                            where c.Deleted == false && c.StoreId == storeId.Value
                            select c;

                return query.ToList();
            }
        }

        /// <summary>
        /// 添加片区信息
        /// </summary>
        /// <param name="district"></param>
        public virtual void InsertDistrict(District district)
        {
            if (district == null)
            {
                throw new ArgumentNullException("district");
            }

            var uow = DistrictsRepository.UnitOfWork;
            DistrictsRepository.Insert(district);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityInserted(district);
        }
        /// <summary>
        /// 删除片区信息
        /// </summary>
        /// <param name="district"></param>
        public virtual void DeleteDistrict(District district)
        {
            if (district == null)
            {
                throw new ArgumentNullException("district");
            }

            var uow = DistrictsRepository.UnitOfWork;
            DistrictsRepository.Delete(district);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityDeleted(district);
        }
        /// <summary>
        /// 修改片区信息
        /// </summary>
        /// <param name="district"></param>
        public virtual void UpdateDistrict(District district)
        {
            if (district == null)
            {
                throw new ArgumentNullException("district");
            }

            var uow = DistrictsRepository.UnitOfWork;
            DistrictsRepository.Update(district);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityUpdated(district);
        }

        /// <summary>
        /// 获取片区ZTree
        /// </summary>
        /// <param name="store"></param>
        /// <returns></returns>
        public IList<ZTree> GetListZTreeVM(int? store)
        {
            List<ZTree> districts = DistrictsRepository.Table
            .Select(c => c).Where(c => (c.Deleted == false) && c.StoreId == store)
            .ToList().Select(c =>
            {
                return new ZTree()
                {
                    open = true,
                    id = c.Id,
                    pId = c.ParentId,
                    name = c.Name
                };
            }).ToList();

            return districts;
        }



        public List<int> GetDistricts(int? store, int Id)
        {
            List<int> fancyTrees = new List<int>();
            var perentList = GetDistrictByParentId(store, Id);
            if (perentList != null && perentList.Count > 0)
            {
                foreach (var b in perentList)
                {
                    var tempList = GetDistricts(store.Value, b.Id);
                    if (tempList != null && tempList.Count > 0)
                    {
                        fancyTrees.Add(b.Id);
                    }
                }
            }
            return fancyTrees;
        }
        #endregion

        public List<int> GetSubDistrictIds(int storeId, int districtId)
        {
            if (districtId > 0)
            {
                //var districtIds = DistrictsRepository.QueryFromSql<IntQueryType>($"SELECT id `value` FROM (SELECT t1.id,IF ( find_in_set(ParentId, @pids ) > 0, @pids := concat(@pids, ',', id ), 0 ) AS ischild FROM ( SELECT  id, ParentId FROM dcms.Districts where StoreId = {storeId} and ParentId = {districtId} ) t1,( SELECT @pids := 1 ) t2 ) t3;").ToList();

                string sqlString = @"SELECT 
                               id as `value` 
                            FROM
                                (SELECT
                                    t1.id,
                                        t1.ParentId,
                                        IF(FIND_IN_SET(ParentId, @pids) > 0, @pids:= CONCAT(@pids, ',', id), 0) AS ischild
                                FROM
                                    (SELECT
                                    id, ParentId
                                FROM
                                    dcms.Districts t
                                WHERE ";
                sqlString += " t.StoreId = '" + storeId + "' ";

                sqlString += @" ORDER BY ParentId, id) t1, (SELECT @pids:= " + districtId + ") t2) t3 ";

                var districtIds = DistrictsRepository.QueryFromSql<IntQueryType>(sqlString).ToList();

                return districtIds.Select(s => s.Value ?? 0).ToList();
            }
            else
                return new List<int>();
        }

        /// <summary>
        /// 验证是否有子片区
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="districtId"></param>
        /// <returns></returns>
        public List<int> GetSonDistrictIds(int storeId, int districtId)
        {
            return GetAllDistrictByStoreId(storeId).Where(s => s.ParentId == districtId).Select(a => a.Id).ToList();
        }


        public bool CheckDistrictRoot(int storeId)
        {
            return DistrictsRepository.Table.Where(a => a.StoreId == storeId && a.ParentId == 0 && a.Deleted == false).Count() > 0;
        }

        public bool HadInstall(int storeId)
        {
            return GetAllDistrictByStoreId(storeId).Where(s => s.Name == "全部" && s.Deleted == false && s.ParentId == 0).Count() > 0;
        }

        public bool InstallDistrict(int storeId)
        {
            var uow = DistrictsRepository.UnitOfWork;
            DistrictsRepository.Insert(new District { StoreId = storeId, Name = "全部", Deleted = false, ParentId = 0, OrderNo = 0 });
            return uow.SaveChanges() > 0;
        }
        public IList<District> GetChildDistrict(int storeId, int districtId)
        {
            try
            {
                var query = DistrictsRepository.Table.Where(x => x.StoreId == storeId && x.ParentId == districtId && x.Deleted == false);
                return query.ToList().Concat(query.ToList().SelectMany(t => GetChildDistrict(storeId, t.Id))).ToList();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IList<District> GetParentDistrict(int storeId, int districtId)
        {
            try
            {
                var query = DistrictsRepository.Table.Where(x => x.StoreId == storeId && x.Id == districtId);
                return query.ToList().Concat(query.ToList().SelectMany(t => GetParentDistrict(storeId, t.ParentId))).ToList();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IList<District> GetUserDistrict(int storeId, int userId) 
        {
            var lst = new List<District>();
            var tids = UserDistrictsRepository.Table
               .Where(s => s.StoreId == storeId && s.UserId == userId)
               .OrderBy(s => s.DistrictId)
               .Select(s => s.DistrictId)
               .ToList();
            var tmpids = tids.Where(s => s > 0).ToList();
            if (tmpids != null && tmpids.Any())
            {
                var query = from c in DistrictsRepository.Table
                            where c.Deleted == false && c.StoreId == storeId && tmpids.Contains(c.Id)
                            select c;
                lst = query.ToList();
            }
            return lst;
        }
    }
}
