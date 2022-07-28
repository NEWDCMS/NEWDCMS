using DCMS.Core;
using DCMS.Core.Domain.Plan;
using System.Collections.Generic;

namespace DCMS.Services.plan
{
    public interface IPercentageService
    {
        void DeletePercentage(Percentage percentages);
        void DeletePercentageRangeOption(PercentageRangeOption percentageRangeOptions);
        void DeletePercentageRangeOptionByPercentageId(int? percentageId);
        IList<PercentageRangeOption> GetAllPercentageRangeOptions();
        IPagedList<PercentageRangeOption> GetAllPercentageRangeOptions(int? percentageId, int pageIndex = 0, int pageSize = int.MaxValue);
        IList<PercentageRangeOption> GetAllPercentageRangeOptionsByPercentageId(int? store, int? percentageId);
        IList<PercentageRangeOption> GetAllPercentageRangeOptionsByPercentageIds(int? store, int[] ids);

        IList<Percentage> GetAllPercentages();
        IPagedList<Percentage> GetAllPercentages(string name = null, int pageIndex = 0, int pageSize = int.MaxValue);
        IList<Percentage> GetAllPercentages(int? store);
        Percentage GetPercentageById(int? store, int percentagesId);
        IList<Percentage> GetPercentageByIds(int? store, int[] ids);
        IList<Percentage> GetPercentagesByPlanId(int pid);
        Percentage GetPercentageByCatagoryId(int store, int catagoryId);
        Percentage GetPercentageByProductId(int store, int productId);

        Percentage GetPercentageByCatagoryId(int store, int percentagePlanId, int catagoryId);
        Percentage GetPercentageByProductId(int store, int percentagePlanId, int productId);

        PercentageRangeOption GetPercentageRangeOptionById(int? store, int percentageRangeOptionsId);
        IList<PercentageRangeOption> GetPercentageRangeOptionsByIds(int[] sIds);
        IList<Percentage> GetPercentagesByIds(int[] sIds);
        void InsertPercentage(Percentage percentages);
        void InsertPercentageRangeOption(PercentageRangeOption percentageRangeOptions);
        void UpdatePercentage(Percentage percentages);
        void UpdatePercentageRangeOption(PercentageRangeOption percentageRangeOptions);

        Percentage GetPercentageByCatagory(int store, int plan, int catagoryId);
        Percentage GetPercentageByProduct(int store, int plan, int productId);

        BaseResult CreateOrUpdate(int storeId, int userId, int? percentageId, Percentage data, List<PercentageRangeOption> percentageRangeOptions);

        BaseResult Reset(int storeId, int userId, Percentage percentage);


    }
}