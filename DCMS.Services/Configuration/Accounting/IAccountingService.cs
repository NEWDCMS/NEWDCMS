using DCMS.Core;
using DCMS.Core.Domain.Configuration;
using System;
using System.Collections.Generic;

namespace DCMS.Services.Settings
{
    public interface IAccountingService
    {
        void DeleteAccountingOption(AccountingOption accountingOption);
        void DeleteAccountingType(AccountingType accountingType);
        string GetAccountingOptionName(int? store, int accountingOptionId);
        string GetAccountingOptionNameByCodeType(int? store, int accountCodeTypeId);
        AccountingOption GetAccountingOptionById(int store, int typeId, int accountingOptionId);
        AccountingOption GetAccountingOptionById(int accountingOptionId);

        bool CodeExist(int? store, string code);

        IList<AccountingOption> GetAccountingOptionsByIds(int? store, int[] idArr);

        AccountingOption GetAccountingOptionByName(int? store, string accountingOptionName);
        AccountingOption GetAccountingOptionByAccountCodeTypeId(int? store, int? accountCodeTypeId);
        List<AccountingOption> GetAccountingOptionsByAccountCodeTypeIds(int? store, int[] accountCodeTypeIds);
        AccountingOption GetAccountingOptionByCode(int? store, string code);
        AccountingOption Parse(int? store, AccountingCodeEnum ace);
        AccountingOption ParseChilds(int? store, AccountingCodeEnum ace, int aopId);
        AccountingCodeEnum ReserveParse(int? store, int accountingOptionId);
        IPagedList<AccountingType> GetAllAccountingTypes(int? store, string name = null, int pageIndex = 0, int pageSize = int.MaxValue);
        List<AccountingOption> GetAccountingOptionsByAccountingType(int? store, int accountingTypeId);
        List<AccountingOption> GetAccountingOptionsByParentId(int? store, int[] pids, bool isShowEnabled = false);
        List<AccountingOption> GetSubAccountingOptionsByAccountCodeTypeIds(int? store, int[] accountCodeTypeIds, bool includeAccountCodeTypeIds = false);

        AccountingOption GetAccountingOptionsByEnum(int? store, int codeTypeId);

        AccountingType GetAccountingTypeById(int accountingTypeId);
        AccountingType GetAccountingTypeByName(string name);
        IList<AccountingType> GetAccountingTypes();
        IList<AccountingOption> GetDefaultAccounts(int? store);
        List<AccountingOption> GetAllAccountingOptions(int? store, int type = 0, bool isShowEnabled = false);
        List<AccountingOption> GetAllAccountingOptionsByStore(int? store);
        List<AccountingOption> GetAccountingOptions(int? storeId);
        IList<AccountingOption> GetAllAccounts(int? store, int? typeId = 0, int[] codeTypeIds = null);
        void InsertAccountingOption(AccountingOption accountingOption);
        void InsertAccountingType(AccountingType accountingType);
        void UpdateAccountingOption(AccountingOption accountingOption);
        void UpdateAccountingType(AccountingType accountingType);
        bool HasChilds(int store, AccountingOption accountingOption);
        int ChildCount(int store, int typeId, int pid);
        List<AccountingOption> GetAccountingOptionsByParentId(int? store, int pid);
        List<AccountingOption> GetAccountingOptionsByParentId(int? store, int pid, int type);
        List<AccountingOption> GetAccountingOptionsByParentId(int? store, int pid, int[] types);
        bool HasChilds(int accountingOptionId);
        /// <summary>
        /// 获取会计科目叶子节点
        /// </summary>
        /// <param name="store"></param>
        /// <param name="accountingIds"></param>
        /// <returns></returns>
        List<AccountingOption> GetAccountingOptionsByAccountingIds(int? storeId, string accountingIds);

        BaseResult CreateOrUpdate(int storeId, int userId, int typeid, int? accountingOptionid, AccountingOption from, AccountingOption accountingOption);
        List<int> GetSubAccountingOptionIds(int storeId, int accountingOptionId);
        List<AccountingOption> GetAllAccountingOptions(int? store);

        List<AccountingOptionTree> GetAccountingOptionTree(int? store, int parentId, List<int> options);


        List<AccountingTree> GetAccountingTree(int? store, int parentId, int type);
        List<AccountingTree> PaseAccountingTree(int? store, int parentId);
        List<AccountingTree> PaseAccountingTree(int? store, int parentId, int type);

        List<AccountingOptionTree<T>> GetOptionsList<T>(int? store, int Id, int type, List<AccountingOption> options, List<T> sheets) where T : BaseAccount;
        List<AccountingOptionTree<T>> PaseOptionsTree<T>(int? store, DateTime dateTime, List<T> sheets) where T : BaseAccount;
        List<AccountingOptionTree<T>> PaseOptionsTree<T>(int? store, int type, DateTime dateTime, List<T> sheets) where T : BaseAccount;
        List<AccountingTree> GetAccountingTree(int? store, int parentId, int type, int[] typeCodeIds, IList<AccountingOption> allAccountingOptions = null);
        List<AccountingOption> GetDefaultAccounting(int storeId, BillTypeEnum billType, int typeId, List<AccountingOption> alls);
        Tuple<AccountingOption, List<AccountingOption>, List<AccountingOption>, Dictionary<int, string>> GetDefaultAccounting(int storeId, BillTypeEnum billType);
        Tuple<List<AccountingOption>, Dictionary<int, string>> GetReceiptAccounting(int storeId, BillTypeEnum billType, int typeId, List<AccountingOption> alls);

        Dictionary<int, string> GetAccountingOptionNames(int storeId, int[] accountingOptionIds);
        Dictionary<int, string> GetAccountingOptionNameByCodeTypes(int storeId, int[] accountCodeTypeIds);
    }
}