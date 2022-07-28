using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Data;
using DCMS.Core.Domain.Configuration;
using DCMS.Core.Infrastructure.DependencyManagement;
using DCMS.Services.Configuration;
using DCMS.Services.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using DCMS.Services.Caching;
using DCMS.Core.Domain.Common;

namespace DCMS.Services.Settings
{
    /// <summary>
    /// 会计科目服务
    /// </summary>
    public partial class AccountingService : BaseService, IAccountingService
    {
        private readonly ICacheKeyService _cacheKeyService;

        public AccountingService(IServiceGetter getter,
            IStaticCacheManager cacheManager,
            ICacheKeyService cacheKeyService,
            IEventPublisher eventPublisher,
            ISettingService settingService
            ) : base(getter, cacheManager, eventPublisher)
        {
            _cacheKeyService = cacheKeyService;
        }

        #region 方法

        #region 科目类别

        /// <summary>
        /// 获取科目类别
        /// </summary>
        /// <param name="accountingTypeId"></param>
        /// <returns></returns>
        public virtual AccountingType GetAccountingTypeById(int accountingTypeId)
        {
            if (accountingTypeId == 0)
            {
                return null;
            }

            return AccountingTypesRepository.ToCachedGetById(accountingTypeId);
        }

        public virtual IPagedList<AccountingType> GetAllAccountingTypes(int? store, string name = null, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            if (pageSize >= 50)
                pageSize = 50;
            var query = AccountingTypesRepository.Table;

            if (!string.IsNullOrWhiteSpace(name))
            {
                query = query.Where(c => c.Name.Contains(name));
            }

            query = query.OrderBy(c => c.DisplayOrder);

            var plists = new PagedList<AccountingType>(query.ToList(), pageIndex, pageSize);

            var key = DCMSDefaults.ACCOUNTTYPES_GETALLACCOUNTINGTYPES_KEY.FillCacheKey(store, name, pageIndex, pageSize);
            return _cacheManager.Get(key, () => plists);
        }

        public List<AccountingOption> GetAccountingOptionsByParentId(int? store, int pid)
        {
            var query = AccountingOptionsRepository.Table;
            query = query.Where(c => c.ParentId == pid && c.StoreId == store.Value);
            return query.ToList();
        }

        public List<AccountingOption> GetAccountingOptionsByParentId(int? store, int pid, int type)
        {
            var query = AccountingOptionsRepository.Table;

            query = query.Where(c => c.ParentId == pid && c.StoreId == store.Value);

            if (type != 0)
            {
                query = query.Where(c => c.AccountingTypeId == type);
            }

            return query.ToList();
        }


        public List<AccountingOption> GetAccountingOptionsByParentId(int? store, int pid, int[] types)
        {
            var query = AccountingOptionsRepository.Table;

            query = query.Where(c => c.ParentId == pid && c.StoreId == store.Value);

            if (types != null && types.Length > 0)
            {
                query = query.Where(c => types.Contains(c.AccountingTypeId));
            }

            return query.ToList();
        }

        /// <summary>
        /// 获取科目类别
        /// </summary>
        /// <returns></returns>
        public virtual IList<AccountingType> GetAccountingTypes()
        {
            var query = from sa in AccountingTypesRepository.Table
                        orderby sa.DisplayOrder
                        select sa;
            var accountingTypes = query.ToList();
            return accountingTypes;
        }


        /// <summary>
        /// 删除科目类别
        /// </summary>
        /// <param name="accountingType"></param>
        public virtual void DeleteAccountingType(AccountingType accountingType)
        {
            if (accountingType == null)
            {
                throw new ArgumentNullException("accountingType");
            }

            var uow = AccountingTypesRepository.UnitOfWork;
            AccountingTypesRepository.Delete(accountingType);
            uow.SaveChanges();

            _eventPublisher.EntityDeleted(accountingType);
        }

        /// <summary>
        /// 添加科目类别
        /// </summary>
        /// <param name="accountingType"></param>
        public virtual void InsertAccountingType(AccountingType accountingType)
        {
            if (accountingType == null)
            {
                throw new ArgumentNullException("accountingType");
            }

            var uow = AccountingTypesRepository.UnitOfWork;
            AccountingTypesRepository.Insert(accountingType);
            uow.SaveChanges();

            _eventPublisher.EntityInserted(accountingType);
        }

        /// <summary>
        /// 更新科目类别
        /// </summary>
        /// <param name="accountingType"></param>
        public virtual void UpdateAccountingType(AccountingType accountingType)
        {
            if (accountingType == null)
            {
                throw new ArgumentNullException("accountingType");
            }

            var uow = AccountingTypesRepository.UnitOfWork;
            AccountingTypesRepository.Update(accountingType);
            uow.SaveChanges();

            _eventPublisher.EntityUpdated(accountingType);
        }

        #endregion

        #region 科目类别项

        /// <summary>
        /// 获取科目项
        /// </summary>
        /// <param name="accountingOptionId"></param>
        /// <returns></returns>
        public virtual AccountingOption GetAccountingOptionById(int store, int typeId, int accountingOptionId)
        {
            var query = from c in AccountingOptionsRepository.Table where c.Number == accountingOptionId && c.AccountingTypeId == typeId && c.StoreId == store select c;
            return query.FirstOrDefault();
        }
        /// <summary>
        /// 获取科目项
        /// </summary>
        /// <param name="accountingOptionId"></param>
        /// <returns></returns>
        public virtual AccountingOption GetAccountingOptionById(int accountingOptionId)
        {
            if (accountingOptionId == 0)
            {
                return null;
            }

            return AccountingOptionsRepository.ToCachedGetById(accountingOptionId);
        }

        /// <summary>
        /// 验证code是否存在
        /// </summary>
        /// <param name="store"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        public virtual bool CodeExist(int? store, string code)
        {
            return AccountingOptionsRepository.Table.Where(q => q.StoreId == store && q.Code == code).Count() > 0;
        }


        public virtual IList<AccountingOption> GetAccountingOptionsByIds(int? store, int[] idArr)
        {
            if (idArr == null || idArr.Length == 0)
            {
                return new List<AccountingOption>();
            }

            var query = from c in AccountingOptionsRepository.Table
                        where c.StoreId == store && idArr.Contains(c.Id)
                        select c;

            var key = DCMSDefaults.ACCOUNTINGOPTION_BY_IDS_KEY.FillCacheKey(store, idArr);
            return _cacheManager.Get(key, () => query.ToList());

        }


        public virtual AccountingOption GetAccountingOptionByName(int? store, string accountingOptionName)
        {
            if (string.IsNullOrEmpty(accountingOptionName))
            {
                return null;
            }

            var query = AccountingOptionsRepository.Table;
            query = query.Where(c => c.StoreId == store && c.Name == accountingOptionName);

            return query.FirstOrDefault();

        }

        public virtual AccountingType GetAccountingTypeByName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }

            var query = AccountingTypesRepository.Table;

            return query.FirstOrDefault(c => c.Name == name);
        }

        public virtual AccountingOption GetAccountingOptionByAccountCodeTypeId(int? store, int? accountCodeTypeId)
        {
            var query = AccountingOptionsRepository.Table;
            query = query.Where(c => c.StoreId == store && c.AccountCodeTypeId == accountCodeTypeId);
            return query.FirstOrDefault();
        }

        public virtual List<AccountingOption> GetAccountingOptionsByAccountCodeTypeIds(int? store, int[] accountCodeTypeIds)
        {
            var query = AccountingOptionsRepository.Table;
            query = query.Where(c => c.StoreId == store && accountCodeTypeIds.Contains(c.AccountCodeTypeId ?? 0));
            return query.ToList();
        }

        public virtual AccountingOption Parse(int? store, AccountingCodeEnum ace)
        {
            var query = AccountingOptionsRepository.Table;
            query = query.Where(c => c.StoreId == store && c.AccountCodeTypeId == (int)ace);
            return query.FirstOrDefault();
        }

        public virtual AccountingCodeEnum ReserveParse(int? store, int accountingOptionId)
        {
            if (accountingOptionId == 0)
            {
                throw new ArgumentNullException("科目不能为空");
            }

            var query = AccountingOptionsRepository.Table;
            query = query.Where(c => c.StoreId == store && c.Id == accountingOptionId);
            var aop = query.FirstOrDefault();
            return (AccountingCodeEnum)aop?.AccountCodeTypeId;
        }



        public virtual AccountingOption ParseChilds(int? store, AccountingCodeEnum ace, int aopId)
        {
            var lists = new List<AccountingOption>();
            var aop = Parse(store, ace);
            if (aop != null)
            {
                var query = AccountingOptionsRepository.TableNoTracking;
                query = query.Where(c => c.StoreId == store && c.ParentId == aop.Number);
                //query = query.Where(c => c.StoreId == store && c.Id == aop.Id);
                lists = query.ToList();
            }
            return lists.Where(s => s.Id == aopId).FirstOrDefault();
        }

        public virtual AccountingOption GetAccountingOptionByCode(int? store, string code)
        {
            var query = AccountingOptionsRepository.Table;
            query = query.Where(c => c.StoreId == store && c.Code == code);
            return query.FirstOrDefault();
        }


        public virtual string GetAccountingOptionName(int? store, int accountingOptionId)
        {
            if (accountingOptionId == 0)
                return null;

            var option = AccountingOptionsRepository.Table.Where(a => a.StoreId == store && a.Id == accountingOptionId).Select(a => a.Name).FirstOrDefault();

            var key = _cacheKeyService.PrepareKeyForDefaultCache(DCMSDefaults.ACCOUNTINGOPTION_NAME_BY_ID_KEY, store, accountingOptionId);
            return _cacheManager.Get(key, () => option);
        }
        public virtual string GetAccountingOptionNameByCodeType(int? store, int accountCodeTypeId)
        {
            if (accountCodeTypeId == 0)
                return null;

            var option = AccountingOptionsRepository.Table.Where(a => a.StoreId == store && a.AccountCodeTypeId == accountCodeTypeId).Select(a => a.Name).FirstOrDefault();

            var key = _cacheKeyService.PrepareKeyForDefaultCache(DCMSDefaults.ACCOUNTINGOPTION_NAME_BY_CODETYPEID_KEY, store, accountCodeTypeId);

            return _cacheManager.Get(key, () => option);
        }


        public virtual Dictionary<int, string> GetAccountingOptionNames(int storeId, int[] accountingOptionIds)
        {
            var dicts = new Dictionary<int, string>();
            if (accountingOptionIds.Count() > 0)
            {
                dicts = UserRepository_RO.QueryFromSql<DictType>($"SELECT Id,Name as Name FROM dcms.AccountingOptions where StoreId = " + storeId + " and id in(" + string.Join(",", accountingOptionIds) + ");").ToDictionary(k => k.Id, v => v.Name);
            }
            return dicts;
        }

        public virtual Dictionary<int, string> GetAccountingOptionNameByCodeTypes(int storeId, int[] accountCodeTypeIds)
        {
            var dicts = new Dictionary<int, string>();
            if (accountCodeTypeIds.Count() > 0)
            {
                dicts = UserRepository_RO.QueryFromSql<DictType>($"SELECT Id,Name as Name FROM dcms.AccountingOptions where StoreId = " + storeId + " and Number in(" + string.Join(",", accountCodeTypeIds) + ");").ToDictionary(k => k.Id, v => v.Name);
            }
            return dicts;
        }


        public List<AccountingOption> GetAccountingOptionsByParentId(int? store, int[] pids, bool isShowEnabled = false)
        {
            var query = AccountingOptionsRepository.Table;
            query = query.Where(c => pids.Contains(c.ParentId ?? 0) && c.StoreId == store.Value);

            if (isShowEnabled)
            {
                query = query.Where(c => c.Enabled == true);
            }

            return query.ToList();
        }

        public List<AccountingOption> GetSubAccountingOptionsByAccountCodeTypeIds(int? store, int[] accountCodeTypeIds, bool includeAccountCodeTypeIds = false)
        {
            //指定节点科目
            var accountingOptions = GetAccountingOptionsByAccountCodeTypeIds(store, accountCodeTypeIds);
            //所有科目
            var allOptions = GetAllAccountingOptionsByStore(store);
            var newOptions = new List<AccountingOption>();

            //递归子节点
            //panentid = id
            //GetSubOptions(allOptions, newOptions, accountingOptions.Select(ao => ao.Id).ToArray());
            //parentid = number
            //GetSubOptions(allOptions, newOptions, accountingOptions.Select(ao => ao.Number).ToArray());

            //包含根节点
            if (includeAccountCodeTypeIds && accountingOptions != null && accountingOptions.Count > 0)
            {
                accountingOptions.ForEach(ao =>
                {
                    if (newOptions.Where(no => no.Id == ao.Id).Count() == 0)
                    {
                        newOptions.Add(ao);
                    }
                });
            }

            return newOptions;

        }

        /// <summary>
        /// 根据枚举值获取科目
        /// </summary>
        /// <param name="store"></param>
        /// <param name="codeTypeId"></param>
        /// <returns></returns>
        public AccountingOption GetAccountingOptionsByEnum(int? store, int codeTypeId)
        {
            var query = AccountingOptionsRepository.Table;
            query = query.Where(c => c.AccountCodeTypeId == codeTypeId && c.StoreId == store);

            return query.FirstOrDefault();
        }


        /// <summary>
        /// 获取科目项
        /// </summary>
        /// <param name="store"></param>
        /// <param name="pid"></param>
        /// <param name="isShowEnabled"></param>
        /// <returns></returns>
        public List<AccountingOption> GetAllAccountingOptions(int? store, int type = 0, bool isShowEnabled = false)
        {
            var key = DCMSDefaults.ACCOUNTOPTIONS_ALL_BY_STORE_KEY.FillCacheKey(store, type);
            return _cacheManager.Get(key, () =>
            {
                var query = AccountingOptionsRepository.Table;
                query = query.Where(c => c.StoreId == store.Value);

                if (type != 0)
                {
                    query = query.Where(c => c.AccountingTypeId == type);
                }

                if (isShowEnabled)
                {
                    query = query.Where(c => c.Enabled == true);
                }

                return query.OrderBy(s => s.StoreId).OrderBy(s => s.AccountingTypeId).ToList();
            });

        }

        public List<AccountingOption> GetAllAccountingOptionsByStore(int? store)
        {
            var query = AccountingOptionsRepository.Table;
            query = query.Where(c => c.StoreId == store.Value);
            return query.ToList();
        }



        /// <summary>
        /// 获取科目项
        /// </summary>
        /// <param name="accountingTypeId"></param>
        /// <returns></returns>
        public virtual List<AccountingOption> GetAccountingOptionsByAccountingType(int? store, int accountingTypeId)
        {
            var query = from sao in AccountingOptionsRepository.Table
                        orderby sao.DisplayOrder
                        where sao.AccountingTypeId == accountingTypeId && sao.StoreId == store && sao.Enabled == true
                        select sao;
            var accountingOptions = query.ToList();
            return accountingOptions;
        }



        public virtual List<AccountingOption> GetAccountingOptions(int? storeId)
        {
            var accountingOptions = new List<AccountingOption>();
            //获取科目 注意：AccountingType  StoreId 一定要取 0
            var accTypeQuery = AccountingTypesRepository.TableNoTracking.Where(s => s.StoreId == 0 && s.DisplayOrder == 0).ToList();
            if (accTypeQuery != null)
            {
                var types = accTypeQuery.Select(s => s.Id).ToArray();
                if (types != null)
                {
                    accountingOptions = AccountingOptionsRepository.TableNoTracking.Where(c => c.StoreId == storeId.Value && types.Contains(c.AccountingTypeId)).ToList();
                }
            }
            return accountingOptions;
        }


        /// <summary>
        /// 获取默认收款账户
        /// </summary>
        /// <param name="store"></param>
        /// <returns></returns>
        public virtual IList<AccountingOption> GetDefaultAccounts(int? store)
        {
            var query = from sao in AccountingOptionsRepository.TableNoTracking
                        orderby sao.DisplayOrder
                        where sao.IsDefault == true && sao.StoreId == store
                        select sao;
            var accountingOptions = query.ToList();
            return accountingOptions;
        }


        /// <summary>
        /// 删除科目项
        /// </summary>
        /// <param name="accountingOption"></param>
        public virtual void DeleteAccountingOption(AccountingOption accountingOption)
        {
            if (accountingOption == null)
            {
                throw new ArgumentNullException("accountingOption");
            }

            var uow = AccountingOptionsRepository.UnitOfWork;
            AccountingOptionsRepository.Delete(accountingOption);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityDeleted(accountingOption);
        }

        /// <summary>
        /// 添加科目项
        /// </summary>
        /// <param name="accountingOption"></param>
        public virtual void InsertAccountingOption(AccountingOption accountingOption)
        {
            if (accountingOption == null)
            {
                throw new ArgumentNullException("accountingOption");
            }

            var uow = AccountingOptionsRepository.UnitOfWork;
            AccountingOptionsRepository.Insert(accountingOption);
            uow.SaveChanges();

            //经销商自定义的会计科目Number值为Id值
            //accountingOption.Number = accountingOption.Id;
            //UpdateAccountingOption(accountingOption);
            //通知
            _eventPublisher.EntityInserted(accountingOption);
        }

        /// <summary>
        /// 更新科目项
        /// </summary>
        /// <param name="accountingOption"></param>
        public virtual void UpdateAccountingOption(AccountingOption accountingOption)
        {
            if (accountingOption == null)
            {
                throw new ArgumentNullException("accountingOption");
            }

            var uow = AccountingOptionsRepository.UnitOfWork;
            AccountingOptionsRepository.Update(accountingOption);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityUpdated(accountingOption);
        }

        /// <summary>
        /// 是否有子目录
        /// </summary>
        /// <param name="accountingOption"></param>
        /// <returns></returns>
        public bool HasChilds(int store, AccountingOption accountingOption)
        {
            var query = from c in AccountingOptionsRepository.Table where c.ParentId == accountingOption.Id && c.StoreId == store select c;
            return query.ToList().Count > 0;
        }

        public bool HasChilds(int accountingOptionId)
        {
            var query = from c in AccountingOptionsRepository.Table where c.ParentId == accountingOptionId select c;
            return query.ToList().Count > 0;
        }



        /// <summary>
        /// 获取子目录数
        /// </summary>
        /// <param name="typeId"></param>
        /// <param name="pid"></param>
        /// <returns></returns>
        public int ChildCount(int store, int typeId, int pid)
        {
            var query = from c in AccountingOptionsRepository.Table where c.ParentId == pid && c.AccountingTypeId == typeId && c.StoreId == store select c;
            return query.ToList().Count;
        }


        public virtual IList<AccountingOption> GetAllAccounts(int? store, int? typeId = 0, int[] codeTypeIds = null)
        {
            var query = AccountingOptionsRepository.TableNoTracking.Where(s => s.StoreId == store);

            if (typeId.HasValue && typeId.Value > 0)
            {
                query = query.Where(s => s.AccountingTypeId == typeId);
            }

            //if (codeTypeIds?.Count() > 0)
            //    query = query.Where(s => codeTypeIds.Contains(s.AccountCodeTypeId ?? 0));

            var querys = from sao in query
                         orderby sao.Id
                         select new AccountingOption()
                         {
                             Id = sao.Id,
                             StoreId = store ?? 0,
                             Number = sao.Number,
                             ParentId = sao.ParentId,
                             Name = sao.Name,
                             Code = sao.Code,
                             Enabled = true,
                             IsLeaf = sao.IsLeaf,
                             AccountingTypeId = sao.AccountingTypeId,
                             AccountCodeTypeId = sao.AccountCodeTypeId
                         };

            var key = DCMSDefaults.GETALLACCOUNTS_KEY.FillCacheKey(store, typeId, codeTypeIds);
            var accountingOptions = _cacheManager.Get(key, () => querys.ToList());
            return accountingOptions;
        }


        #endregion

        /// <summary>
        /// 获取会计科目叶子节点
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="accountingIds"></param>
        /// <returns></returns>
        public List<AccountingOption> GetAccountingOptionsByAccountingIds(int? storeId, string accountingIds)
        {
            var query = from c in AccountingOptionsRepository.Table where c.StoreId == storeId && c.IsLeaf == true select c;
            return query.ToList();
        }

        private int GetOptionForMaxNumber(int store)
        {
            var query = from c in AccountingOptionsRepository.Table where c.StoreId == store select c;
            return query.Max(s => s.Number);
        }
        #endregion


        public BaseResult CreateOrUpdate(int storeId, int userId, int typeid, int? accountingOptionid, AccountingOption from, AccountingOption accountingOption)
        {
            var uow = AccountingOptionsRepository.UnitOfWork;

            ITransaction transaction = null;
            try
            {

                transaction = uow.BeginOrUseTransaction();



                if (accountingOptionid.HasValue && accountingOptionid.Value != 0)
                {
                    //父节点
                    var parentNode = GetAccountingOptionById(from.ParentId ?? 0);
                    //如果存在父节点，并且父节点为叶子节点
                    if (parentNode != null && parentNode.IsLeaf == true)
                    {
                        var hasChilds = HasChilds(storeId, parentNode);
                        if (hasChilds)
                        {
                            parentNode.IsLeaf = false;
                            UpdateAccountingOption(parentNode);
                        }
                    }

                    accountingOption.AccountingTypeId = typeid;
                    accountingOption.ParentId = from.ParentId;
                    accountingOption.Name = from.Name;
                    accountingOption.Code = from.Code;
                    accountingOption.DisplayOrder = from.DisplayOrder;
                    accountingOption.Enabled = from.Enabled;
                    accountingOption.IsDefault = from.IsDefault;
                    accountingOption.AccountCodeTypeId = from.AccountCodeTypeId;
                    accountingOption.InitBalance = from.InitBalance;

                    UpdateAccountingOption(accountingOption);
                }
                else
                {
                    from.Number = GetOptionForMaxNumber(storeId) + 1;
                    from.AccountingTypeId = typeid;
                    from.IsLeaf = true;
                    InsertAccountingOption(from);

                    //父节点
                    var parentNode = GetAccountingOptionById(from.ParentId ?? 0);
                    //如果存在父节点，并且父节点为叶子节点
                    if (parentNode != null && parentNode.IsLeaf == true)
                    {
                        var hasChilds = HasChilds(storeId, parentNode);
                        if (hasChilds)
                        {
                            parentNode.IsLeaf = false;
                            UpdateAccountingOption(parentNode);
                        }
                    }
                }


                //保存事务
                transaction.Commit();

                return new BaseResult { Success = true, Message = "科目创建/更新成功" };
            }
            catch (Exception)
            {
                //如果事务不存在或者为控则回滚
                transaction?.Rollback();
                return new BaseResult { Success = false, Message = "科目创建/更新失败" };
            }
            finally
            {
                //不管怎样最后都会关闭掉这个事务
                using (transaction) { }
            }
        }

        /// <summary>
        /// 获取全部可用类别
        /// </summary>
        /// <param name="store"></param>
        /// <returns></returns>
        public virtual List<AccountingOption> GetAllAccountingOptions(int? store)
        {
            var key = DCMSDefaults.ACCOUNTINGOPTION_ALL_BY_STOREID_KEY.FillCacheKey(store);
            return _cacheManager.Get(key, () =>
            {
                var query = AccountingOptionsRepository.Table;

                if (store.HasValue && store.Value != 0)
                {
                    query = query.Where(c => c.StoreId == store);
                }
                else
                {
                    return new List<AccountingOption>();
                }

                query = query.OrderBy(c => c.ParentId);
                return query.ToList();
            });
        }

        public List<int> GetSubAccountingOptionIds(int storeId, int accountingOptionId)
        {
            //递归获取会计科目
            List<int> accountingOptionIds = new List<int>();
            var allAccountingOptions = GetAllAccountingOptions(storeId);
            if (allAccountingOptions != null && allAccountingOptions.Count > 0)
            {
                //如果用户选择会计科目，则查询当前类别下所有子会计科目
                if (accountingOptionId > 0)
                {
                    var thisCategories = SortAccountingOptionsForTree(allAccountingOptions, accountingOptionId);
                    if (thisCategories != null && thisCategories.Count > 0)
                    {
                        accountingOptionIds = thisCategories.Select(cg => cg.Id).Distinct().ToList();
                    }
                    accountingOptionIds = accountingOptionIds.Distinct().ToList();
                }
                //如果用户没有选择会计科目，则查询所有可用会计科目
                else
                {
                    accountingOptionIds.AddRange(allAccountingOptions.Select(ac => ac.Id).Distinct().ToList());
                }
            }
            return accountingOptionIds;
        }

        /// <summary>
        /// 排序会计科目树
        /// </summary>
        /// <param name="source">源</param>
        /// <param name="parentId">父节点</param>
        /// <param name="ignoreCategoriesWithoutExistingParent">是否忽略不存在的父级</param>
        /// <returns></returns>
        public IList<AccountingOption> SortAccountingOptionsForTree(IList<AccountingOption> source, int parentId = 0, bool ignoreAccountingOptionsWithoutExistingParent = false)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            var result = new List<AccountingOption>();

            foreach (var cat in source.ToList().FindAll(c => c.ParentId == parentId))
            {
                result.Add(cat);
                result.AddRange(SortAccountingOptionsForTree(source, cat.Id, ignoreAccountingOptionsWithoutExistingParent));
            }
            if (!ignoreAccountingOptionsWithoutExistingParent && result.Count != source.Count)
            {
                foreach (var cat in source)
                {
                    if (result.FirstOrDefault(x => x.Id == cat.Id) == null)
                    {
                        if (cat.Id == parentId)
                        {
                            result.Add(cat);
                        }
                    }
                }
            }
            return result;
        }


        /// <summary>
        /// 递归获取科目树
        /// </summary>
        /// <param name="store"></param>
        /// <param name="parentId"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public List<AccountingTree> GetAccountingTree(int? store, int parentId, int type)
        {
            var trees = new List<AccountingTree>();
            var perentList = GetAccountingOptionsByParentId(store.Value, parentId, type);
            if (perentList != null && perentList.Count > 0)
            {
                foreach (var b in perentList)
                {
                    var tempList = GetAccountingTree(store.Value, b.Id, type);
                    var node = new AccountingTree
                    {
                        Id = b.Id,
                        StoreId = b.StoreId,
                        AccountingTypeId = b.AccountingTypeId,
                        AccountCodeTypeId = b.AccountCodeTypeId,
                        ParentId = b.ParentId,
                        Number = b.Number,
                        Name = b.Name,
                        Code = b.Code,
                        DisplayOrder = b.DisplayOrder,
                        Enabled = b.Enabled,
                        IsDefault = b.IsDefault,
                        IsLeaf = true,
                        IsCustom = b.IsCustom,
                        Children = new List<AccountingTree>()
                    };
                    if (tempList.Count > 0)
                    {
                        node.IsLeaf = false;
                        node.Children = tempList;
                    }
                    trees.Add(node);
                }
            }
            return trees;
        }


        /// <summary>
        /// 递归获取科目树
        /// </summary>
        /// <param name="store"></param>
        /// <param name="parentId"></param>
        /// <param name="type"></param>
        /// <param name="typeCodeIds"></param>
        /// <param name="allAccountingOptions"></param>
        /// <returns></returns>
        public List<AccountingTree> GetAccountingTree(int? store, int parentId, int type, int[] typeCodeIds, IList<AccountingOption> allAccountingOptions = null)
        {
            var trees = new List<AccountingTree>();

            var parentList = allAccountingOptions
                .Where(s => s.StoreId == store.Value && s.ParentId == parentId && s.AccountingTypeId == type)
                .ToList();

            if (parentList != null && parentList.Count > 0)
            {
                foreach (var b in parentList)
                {
                    var tempList = GetAccountingTree(store.Value, b.Number, type, typeCodeIds, allAccountingOptions);
                    var node = new AccountingTree
                    {
                        Id = b.Id,
                        StoreId = b.StoreId,
                        AccountingTypeId = b.AccountingTypeId,
                        AccountCodeTypeId = b.AccountCodeTypeId,
                        ParentId = b.ParentId,
                        Number = b.Number,
                        Name = b.Name,
                        Code = b.Code,
                        Children = new List<AccountingTree>()
                    };

                    if (tempList.Count > 0)
                    {
                        node.IsLeaf = false;
                        node.Children = tempList;
                    }
                    trees.Add(node);
                }
            }
            return trees;
        }


        /// <summary>
        /// 递归获取科目树
        /// </summary>
        /// <param name="store"></param>
        /// <param name="parentId"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public List<AccountingOptionTree> GetAccountingOptionTree(int? store, int parentId, List<int> options)
        {
            List<AccountingOptionTree> trees = new List<AccountingOptionTree>();
            var perentList = GetAccountingOptionsByParentId(store.Value, parentId);
            if (perentList != null && perentList.Count > 0)
            {
                foreach (var b in perentList)
                {
                    List<AccountingOptionTree> tempList = GetAccountingOptionTree(store.Value, b.Id, options);

                    b.Selected = options != null ? options.Contains(b.Id) : false;
                    var node = new AccountingOptionTree
                    {
                        Visible = options.Contains(b.Id),
                        Option = b,
                        Children = new List<AccountingOptionTree>()
                    };

                    if (tempList != null && tempList.Count > 0)
                    {
                        node.Children = tempList;
                    }

                    trees.Add(node);

                }
            }
            return trees;
        }


        public List<AccountingTree> PaseAccountingTree(int? store, int parentId)
        {
            return PaseAccountingTree(store, parentId, 0);
        }
        public List<AccountingTree> PaseAccountingTree(int? store, int parentId, int type)
        {
            var key = DCMSDefaults.ACCOUNTINGOPTION_PASEACCOUNTINGTREE_KEY.FillCacheKey(store, parentId, type);
            return _cacheManager.Get(key, () =>
             {
                 var options = GetAllAccountingOptions(store).Select(c => c.Id).ToList();
                 var ops = GetAccountingTree(store ?? 0, parentId, type);
                 return ops.OrderBy(s => s.AccountingTypeId).ToList();
             });
        }


        public List<AccountingOptionTree<T>> GetOptionsList<T>(int? store, int Id, int type, List<AccountingOption> options, List<T> sheets) where T : BaseAccount
        {
            var trees = new List<AccountingOptionTree<T>>();
            var perentList = GetAccountingOptionsByParentId(store.Value, options, Id, type);
            if (perentList != null && perentList.Count > 0)
            {
                foreach (var b in perentList)
                {
                    List<AccountingOptionTree<T>> tempList = GetOptionsList(store.Value, b.Number, type, options, sheets);
                    //b.Selected = options != null ? options.Select(s => s.Number).Contains(b.Number) : false;
                    var node = new AccountingOptionTree<T>
                    {
                        //Visible = b.Selected,
                        Option = b,
                        Children = new List<AccountingOptionTree<T>>(),
                        Bearer = sheets?.FirstOrDefault(x => x.AccountingOptionId == b.Id)
                    };

                    if (tempList != null && tempList.Count > 0)
                    {
                        node.Children = tempList;
                    }

                    trees.Add(node);
                }
            }
            return trees;
        }

        public List<AccountingOption> GetAccountingOptionsByParentId(int? store, List<AccountingOption> options, int pid, int type)
        {
            var query = options.Where(c => c.ParentId == pid);

            if (type != 0)
            {
                query = query.Where(c => c.AccountingTypeId == type);
            }

            return query.ToList();
        }


        /// <summary>
        /// 解析科目树
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="store"></param>
        /// <param name="dateTime"></param>
        /// <param name="sheets"></param>
        /// <returns></returns>
        public List<AccountingOptionTree<T>> PaseOptionsTree<T>(int? store, DateTime dateTime, List<T> sheets) where T : BaseAccount
        {
            var key = DCMSDefaults.ACCOUNTINGOPTION_PASEOPTIONSTREE_KEY.FillCacheKey(store, dateTime, nameof(T));
            return _cacheManager.Get(key, () =>
            {
                var allOptions = GetAllAccountingOptions(store).ToList();
                var ops = GetOptionsList<T>(store ?? 0, 0, 0, allOptions, sheets);
                return ops;
            });
        }

        /// <summary>
        /// 解析科目树
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="store"></param>
        /// <param name="type"></param>
        /// <param name="dateTime"></param>
        /// <param name="sheets"></param>
        /// <returns></returns>
        public List<AccountingOptionTree<T>> PaseOptionsTree<T>(int? store, int type, DateTime dateTime, List<T> sheets) where T : BaseAccount
        {
            var key = DCMSDefaults.ACCOUNTINGOPTION_PASEOPTIONSTREE_TYPE_KEY.FillCacheKey(store, type, dateTime, nameof(T));
            return _cacheManager.Get(key, () =>
            {
                var options = GetAllAccountingOptions(store, type).ToList();
                var ops = GetOptionsList<T>(store ?? 0, 0, type, options, sheets);
                return ops;
            });
        }


        /// <summary>
        /// 获取指定单据类型的初始科目账户
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="billTypeId"></param>
        /// <param name="typeId"></param>
        /// <param name="ids"></param>
        /// <returns></returns>
        public List<AccountingOption> GetDefaultAccounting(int storeId, BillTypeEnum billType, int typeId, List<AccountingOption> alls)
        {
            var codeTypeIds = new List<int>();
            switch (billType)
            {
                case BillTypeEnum.SaleReservationBill:
                    //资产类：预收账款
                    //type.(int)AccountingEnum.Assets;
                    codeTypeIds.Add((int)AccountingCodeEnum.AdvanceReceipt);
                    break;
                case BillTypeEnum.SaleBill:
                    //资产类：库存现金,银行存款,其他账户,预收账款
                    codeTypeIds.Add((int)AccountingCodeEnum.HandCash);
                    codeTypeIds.Add((int)AccountingCodeEnum.BankDeposits);
                    codeTypeIds.Add((int)AccountingCodeEnum.OtherAccount);
                    codeTypeIds.Add((int)AccountingCodeEnum.AdvanceReceipt);
                    break;
                case BillTypeEnum.ReturnReservationBill:
                    //资产类：预收账款
                    codeTypeIds.Add((int)AccountingCodeEnum.AdvanceReceipt);
                    break;
                case BillTypeEnum.ReturnBill:
                    //资产类：库存现金,银行存款,其他账户,预收账款
                    codeTypeIds.Add((int)AccountingCodeEnum.HandCash);
                    codeTypeIds.Add((int)AccountingCodeEnum.BankDeposits);
                    codeTypeIds.Add((int)AccountingCodeEnum.OtherAccount);
                    codeTypeIds.Add((int)AccountingCodeEnum.AdvanceReceipt);
                    break;
                case BillTypeEnum.PurchaseReturnBill:
                    //资产类：库存现金,银行存款,其他账户,预付账款
                    codeTypeIds.Add((int)AccountingCodeEnum.HandCash);
                    codeTypeIds.Add((int)AccountingCodeEnum.BankDeposits);
                    codeTypeIds.Add((int)AccountingCodeEnum.OtherAccount);
                    codeTypeIds.Add((int)AccountingCodeEnum.AdvancePayment);
                    break;
                case BillTypeEnum.PurchaseBill:
                    //资产类：库存现金,银行存款,其他账户,预付账款
                    codeTypeIds.Add((int)AccountingCodeEnum.HandCash);
                    codeTypeIds.Add((int)AccountingCodeEnum.BankDeposits);
                    codeTypeIds.Add((int)AccountingCodeEnum.OtherAccount);
                    codeTypeIds.Add((int)AccountingCodeEnum.AdvancePayment);
                    break;
                case BillTypeEnum.CashReceiptBill:
                    //资产类：库存现金,银行存款,其他账户,预收账款
                    codeTypeIds.Add((int)AccountingCodeEnum.HandCash);
                    codeTypeIds.Add((int)AccountingCodeEnum.BankDeposits);
                    codeTypeIds.Add((int)AccountingCodeEnum.OtherAccount);
                    codeTypeIds.Add((int)AccountingCodeEnum.AdvanceReceipt);
                    break;
                case BillTypeEnum.PaymentReceiptBill:
                    //资产类：库存现金,银行存款,其他账户,预付账款
                    codeTypeIds.Add((int)AccountingCodeEnum.HandCash);
                    codeTypeIds.Add((int)AccountingCodeEnum.BankDeposits);
                    codeTypeIds.Add((int)AccountingCodeEnum.OtherAccount);
                    codeTypeIds.Add((int)AccountingCodeEnum.AdvancePayment);
                    break;
                case BillTypeEnum.AdvanceReceiptBill:
                    //资产类：预收账款
                    codeTypeIds.Add((int)AccountingCodeEnum.AdvanceReceipt);
                    break;
                case BillTypeEnum.AdvancePaymentBill:
                    //资产类：预付账款
                    codeTypeIds.Add((int)AccountingCodeEnum.AdvancePayment);
                    break;
                case BillTypeEnum.CostExpenditureBill:
                    //资产类：库存现金,银行存款,其他账户,
                    codeTypeIds.Add((int)AccountingCodeEnum.HandCash);
                    codeTypeIds.Add((int)AccountingCodeEnum.BankDeposits);
                    codeTypeIds.Add((int)AccountingCodeEnum.OtherAccount);
                    break;
                case BillTypeEnum.CostContractBill:
                    //(费用类别)
                    //损益类（支出）：销售费用,管理费用,财务费用
                    codeTypeIds.Add((int)AccountingCodeEnum.SaleFees);
                    codeTypeIds.Add((int)AccountingCodeEnum.ManageFees);
                    codeTypeIds.Add((int)AccountingCodeEnum.FinanceFees);
                    break;
                case BillTypeEnum.FinancialIncomeBill:
                    //资产类：库存现金,银行存款,其他账户,
                    codeTypeIds.Add((int)AccountingCodeEnum.HandCash);
                    codeTypeIds.Add((int)AccountingCodeEnum.BankDeposits);
                    codeTypeIds.Add((int)AccountingCodeEnum.OtherAccount);
                    break;
                default:
                    break;
            }


            var numbers = alls
                .Where(s => codeTypeIds?.Contains(s.AccountCodeTypeId ?? 0) ?? true)
                .Select(s => s.Number).ToList();

            return alls
                .Where(m => (numbers?.Contains(m.ParentId ?? 0) ?? true) || (numbers?.Contains(m.Number) ?? true) && m.IsLeaf == true)
                .OrderBy(m => m.ParentId)
                .ThenBy(m => m.DisplayOrder).ToList();
        }


        /// <summary>
        /// 获取指定单据类型的收款/付款账户
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="billTypeId"></param>
        /// <param name="typeId"></param>
        /// <param name="ids"></param>
        /// <returns></returns>
        public Tuple<List<AccountingOption>, Dictionary<int, string>> GetReceiptAccounting(int storeId, BillTypeEnum billType, int typeId, List<AccountingOption> alls)
        {
            var codeTypeIds = new List<int>();
            var dynamicColumns = new Dictionary<int, string>();

            switch (billType)
            {
                case BillTypeEnum.SaleReservationBill:
                    //资产类：预收账款
                    codeTypeIds.Add((int)AccountingCodeEnum.AdvanceReceipt);
                    dynamicColumns.Add((int)AccountingCodeEnum.AdvanceReceipt, "预收款");
                    break;
                case BillTypeEnum.SaleBill:
                    //资产类：库存现金,银行存款,其他账户,预收账款
                    codeTypeIds.Add((int)AccountingCodeEnum.HandCash);
                    codeTypeIds.Add((int)AccountingCodeEnum.BankDeposits);
                    codeTypeIds.Add((int)AccountingCodeEnum.OtherAccount);
                    codeTypeIds.Add((int)AccountingCodeEnum.AdvanceReceipt);
                    dynamicColumns.Add((int)AccountingCodeEnum.HandCash, "现金");
                    dynamicColumns.Add((int)AccountingCodeEnum.BankDeposits, "银行");
                    dynamicColumns.Add((int)AccountingCodeEnum.OtherAccount, "其他");
                    dynamicColumns.Add((int)AccountingCodeEnum.AdvanceReceipt, "预收款");
                    break;
                case BillTypeEnum.ReturnReservationBill:
                    //资产类：预收账款
                    codeTypeIds.Add((int)AccountingCodeEnum.AdvanceReceipt);
                    dynamicColumns.Add((int)AccountingCodeEnum.AdvanceReceipt, "预收款");
                    break;
                case BillTypeEnum.ReturnBill:
                    //资产类：库存现金,银行存款,其他账户,预收账款
                    codeTypeIds.Add((int)AccountingCodeEnum.HandCash);
                    codeTypeIds.Add((int)AccountingCodeEnum.BankDeposits);
                    codeTypeIds.Add((int)AccountingCodeEnum.OtherAccount);
                    codeTypeIds.Add((int)AccountingCodeEnum.AdvanceReceipt);
                    dynamicColumns.Add((int)AccountingCodeEnum.HandCash, "现金");
                    dynamicColumns.Add((int)AccountingCodeEnum.BankDeposits, "银行");
                    dynamicColumns.Add((int)AccountingCodeEnum.OtherAccount, "其他");
                    dynamicColumns.Add((int)AccountingCodeEnum.AdvanceReceipt, "预收款");
                    break;
                case BillTypeEnum.PurchaseReturnBill:
                    //资产类：库存现金,银行存款,其他账户,预付账款
                    codeTypeIds.Add((int)AccountingCodeEnum.HandCash);
                    codeTypeIds.Add((int)AccountingCodeEnum.BankDeposits);
                    codeTypeIds.Add((int)AccountingCodeEnum.OtherAccount);
                    codeTypeIds.Add((int)AccountingCodeEnum.AdvancePayment);
                    dynamicColumns.Add((int)AccountingCodeEnum.HandCash, "现金");
                    dynamicColumns.Add((int)AccountingCodeEnum.BankDeposits, "银行");
                    dynamicColumns.Add((int)AccountingCodeEnum.OtherAccount, "其他");
                    dynamicColumns.Add((int)AccountingCodeEnum.AdvancePayment, "预付款");
                    break;
                case BillTypeEnum.PurchaseBill:
                    //资产类：库存现金,银行存款,其他账户,预付账款
                    codeTypeIds.Add((int)AccountingCodeEnum.HandCash);
                    codeTypeIds.Add((int)AccountingCodeEnum.BankDeposits);
                    codeTypeIds.Add((int)AccountingCodeEnum.OtherAccount);
                    codeTypeIds.Add((int)AccountingCodeEnum.AdvancePayment);
                    dynamicColumns.Add((int)AccountingCodeEnum.HandCash, "现金");
                    dynamicColumns.Add((int)AccountingCodeEnum.BankDeposits, "银行");
                    dynamicColumns.Add((int)AccountingCodeEnum.OtherAccount, "其他");
                    dynamicColumns.Add((int)AccountingCodeEnum.AdvancePayment, "预付款");
                    break;
                case BillTypeEnum.CashReceiptBill:
                    //资产类：库存现金,银行存款,其他账户,预收账款
                    codeTypeIds.Add((int)AccountingCodeEnum.HandCash);
                    codeTypeIds.Add((int)AccountingCodeEnum.BankDeposits);
                    codeTypeIds.Add((int)AccountingCodeEnum.OtherAccount);
                    codeTypeIds.Add((int)AccountingCodeEnum.AdvanceReceipt);
                    dynamicColumns.Add((int)AccountingCodeEnum.HandCash, "现金");
                    dynamicColumns.Add((int)AccountingCodeEnum.BankDeposits, "银行");
                    dynamicColumns.Add((int)AccountingCodeEnum.OtherAccount, "其他");
                    dynamicColumns.Add((int)AccountingCodeEnum.AdvanceReceipt, "预收款冲抵");
                    break;
                case BillTypeEnum.PaymentReceiptBill:
                    //资产类：库存现金,银行存款,其他账户,预付账款
                    codeTypeIds.Add((int)AccountingCodeEnum.HandCash);
                    codeTypeIds.Add((int)AccountingCodeEnum.BankDeposits);
                    codeTypeIds.Add((int)AccountingCodeEnum.OtherAccount);
                    codeTypeIds.Add((int)AccountingCodeEnum.AdvancePayment);
                    dynamicColumns.Add((int)AccountingCodeEnum.HandCash, "现金");
                    dynamicColumns.Add((int)AccountingCodeEnum.BankDeposits, "银行");
                    dynamicColumns.Add((int)AccountingCodeEnum.OtherAccount, "其他");
                    dynamicColumns.Add((int)AccountingCodeEnum.AdvancePayment, "预付款冲抵");
                    break;
                case BillTypeEnum.AdvanceReceiptBill:
                    //资产类：预收账款
                    codeTypeIds.Add((int)AccountingCodeEnum.HandCash);
                    codeTypeIds.Add((int)AccountingCodeEnum.BankDeposits);
                    codeTypeIds.Add((int)AccountingCodeEnum.OtherAccount);
                    dynamicColumns.Add((int)AccountingCodeEnum.HandCash, "现金");
                    dynamicColumns.Add((int)AccountingCodeEnum.BankDeposits, "银行");
                    dynamicColumns.Add((int)AccountingCodeEnum.OtherAccount, "其他");
                    break;
                case BillTypeEnum.AdvancePaymentBill:
                    //资产类：预付账款
                    codeTypeIds.Add((int)AccountingCodeEnum.HandCash);
                    codeTypeIds.Add((int)AccountingCodeEnum.BankDeposits);
                    codeTypeIds.Add((int)AccountingCodeEnum.OtherAccount);
                    dynamicColumns.Add((int)AccountingCodeEnum.HandCash, "现金");
                    dynamicColumns.Add((int)AccountingCodeEnum.BankDeposits, "银行");
                    dynamicColumns.Add((int)AccountingCodeEnum.OtherAccount, "其他");
                    break;
                case BillTypeEnum.CostExpenditureBill:
                    //资产类：库存现金,银行存款,其他账户,
                    codeTypeIds.Add((int)AccountingCodeEnum.HandCash);
                    codeTypeIds.Add((int)AccountingCodeEnum.BankDeposits);
                    codeTypeIds.Add((int)AccountingCodeEnum.OtherAccount);
                    dynamicColumns.Add((int)AccountingCodeEnum.HandCash, "现金");
                    dynamicColumns.Add((int)AccountingCodeEnum.BankDeposits, "银行");
                    dynamicColumns.Add((int)AccountingCodeEnum.OtherAccount, "其他");
                    break;
                case BillTypeEnum.CostContractBill:
                    //(费用类别)
                    //损益类（支出）：销售费用,管理费用,财务费用
                    codeTypeIds.Add((int)AccountingCodeEnum.SaleFees);
                    codeTypeIds.Add((int)AccountingCodeEnum.ManageFees);
                    codeTypeIds.Add((int)AccountingCodeEnum.FinanceFees);
                    dynamicColumns.Add((int)AccountingCodeEnum.SaleFees, "销售费");
                    dynamicColumns.Add((int)AccountingCodeEnum.ManageFees, "管理费");
                    dynamicColumns.Add((int)AccountingCodeEnum.FinanceFees, "财务费");
                    break;
                case BillTypeEnum.FinancialIncomeBill:
                    //资产类：库存现金,银行存款,其他账户,
                    codeTypeIds.Add((int)AccountingCodeEnum.HandCash);
                    codeTypeIds.Add((int)AccountingCodeEnum.BankDeposits);
                    codeTypeIds.Add((int)AccountingCodeEnum.OtherAccount);
                    dynamicColumns.Add((int)AccountingCodeEnum.HandCash, "现金");
                    dynamicColumns.Add((int)AccountingCodeEnum.BankDeposits, "银行");
                    dynamicColumns.Add((int)AccountingCodeEnum.OtherAccount, "其他");
                    break;
                case BillTypeEnum.FinanceReceiveAccount:
                    //资产类：库存现金,银行存款,其他账户,
                    codeTypeIds.Add((int)AccountingCodeEnum.HandCash);
                    codeTypeIds.Add((int)AccountingCodeEnum.BankDeposits);
                    codeTypeIds.Add((int)AccountingCodeEnum.OtherAccount);
                    dynamicColumns.Add((int)AccountingCodeEnum.HandCash, "现金");
                    dynamicColumns.Add((int)AccountingCodeEnum.BankDeposits, "银行");
                    dynamicColumns.Add((int)AccountingCodeEnum.OtherAccount, "其他");
                    break;
                default:
                    break;
            }
            //
            var numbers = alls
                .Where(s => codeTypeIds?.Contains(s.AccountCodeTypeId ?? 0) ?? true)
                .Select(s => s.Number).ToList();

            var opts = alls
                .Where(m => (numbers?.Contains(m.ParentId ?? 0) ?? true) || (numbers?.Contains(m.Number) ?? true) && m.IsLeaf == true)
                .OrderBy(m => m.ParentId)
                .ThenBy(m => m.DisplayOrder).ToList();

            return new Tuple<List<AccountingOption>, Dictionary<int, string>>(opts, dynamicColumns);
        }


        /// <summary>
        /// 获取指定单据类型的初始科目账户
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="billType"></param>
        /// <returns>
        /// item1:单据默认收付款科目
        /// item1:单据默认初始科目项目
        /// item3:单据的收付款账户
        /// </returns>
        public Tuple<AccountingOption, List<AccountingOption>, List<AccountingOption>, Dictionary<int, string>> GetDefaultAccounting(int storeId, BillTypeEnum billType)
        {
            var allOptions = GetAllAccounts(storeId).ToList();
            var accounts = GetDefaultAccounting(storeId, billType, 0, allOptions);
            var accountReceipts = GetReceiptAccounting(storeId, billType, 0, allOptions);

            int defaultCodeType = 0;
            switch (billType)
            {
                case BillTypeEnum.SaleReservationBill:
                    //默认：预收款
                    defaultCodeType = (int)AccountingCodeEnum.AdvancesReceived;
                    break;
                case BillTypeEnum.SaleBill:
                    //默认：现金
                    defaultCodeType = (int)AccountingCodeEnum.Cash;
                    break;
                case BillTypeEnum.ReturnReservationBill:
                    //默认：预收款
                    defaultCodeType = (int)AccountingCodeEnum.AdvancesReceived;
                    break;
                case BillTypeEnum.ReturnBill:
                    //默认：现金,
                    defaultCodeType = (int)AccountingCodeEnum.Cash;
                    break;
                case BillTypeEnum.PurchaseReturnBill:
                    //默认：现金,
                    //defaultCodeType = (int)AccountingCodeEnum.Cash;
                    //默认：预付款
                    defaultCodeType = (int)AccountingCodeEnum.Imprest;
                    break;
                case BillTypeEnum.PurchaseBill:
                    //默认：现金,
                    //defaultCodeType = (int)AccountingCodeEnum.Cash;
                    //默认：预付款
                    defaultCodeType = (int)AccountingCodeEnum.Imprest;
                    break;
                case BillTypeEnum.CashReceiptBill:
                    //默认：现金,
                    defaultCodeType = (int)AccountingCodeEnum.Cash;
                    break;
                case BillTypeEnum.PaymentReceiptBill:
                    //默认：现金,
                    defaultCodeType = (int)AccountingCodeEnum.Cash;
                    break;
                case BillTypeEnum.AdvanceReceiptBill:
                    //默认：现金,
                    defaultCodeType = (int)AccountingCodeEnum.Cash;
                    break;
                case BillTypeEnum.AdvancePaymentBill:
                    //默认：现金
                    defaultCodeType = (int)AccountingCodeEnum.Cash;
                    break;
                case BillTypeEnum.CostExpenditureBill:
                    //默认：现金,
                    defaultCodeType = (int)AccountingCodeEnum.Cash;
                    break;
                case BillTypeEnum.CostContractBill:
                    //默认（无）
                    defaultCodeType = 0;
                    break;
                case BillTypeEnum.FinancialIncomeBill:
                    //默认：现金,
                    defaultCodeType = (int)AccountingCodeEnum.Cash;
                    break;
                default:
                    break;
            }

            //默认科目
            var defaultAccount = accountReceipts.Item1
                .Select(s => s)
                .Where(s => s.AccountCodeTypeId == defaultCodeType)
                .FirstOrDefault();

            return new Tuple<AccountingOption, List<AccountingOption>, List<AccountingOption>, Dictionary<int, string>>(defaultAccount, accounts, accountReceipts.Item1, accountReceipts.Item2);
        }
    }

}
