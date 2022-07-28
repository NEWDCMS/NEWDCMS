using DCMS.Core;
using DCMS.Core.Domain.Plan;
using System.Collections.Generic;

namespace DCMS.Services.plan
{
    public interface IPercentagePlanService
    {


        void DeletePercentagePlan(PercentagePlan percentagePlans);
        IList<PercentagePlan> GetAllPercentagePlans();
        IPagedList<PercentagePlan> GetAllPercentagePlans(int? store, string name = null, int pageIndex = 0, int pageSize = int.MaxValue);
        IList<PercentagePlan> GetAllPercentagePlans(int? store);
        PercentagePlan GetPercentagePlanById(int? store, int percentagePlansId);
        IList<PercentagePlan> GetPercentagePlansByIds(int[] sIds);
        IList<Percentage> GetPercentagePlans(int PercentagePlansid);

        /// <summary>
        /// 绑定提成方案信息
        /// </summary>
        /// <param name="store"></param>
        /// <returns></returns>
        List<PercentagePlan> BindPercentagePlanList(int? store);

        /// <summary>
        /// 绑定业务员提成方案信息
        /// </summary>
        /// <param name="store"></param>
        /// <returns></returns>
        List<PercentagePlan> BindBusinessPercentagePlanList(int? store);

        /// <summary>
        /// 绑定送货员提成方案信息
        /// </summary>
        /// <param name="store"></param>
        /// <returns></returns>
        List<PercentagePlan> BindDeliverPercentagePlanList(int? store);


        void InsertPercentagePlan(PercentagePlan percentagePlans);
        void UpdatePercentagePlan(PercentagePlan percentagePlans);

        BaseResult BillCreateOrUpdate(int storeId, int userId, int? billId, PercentagePlan percentagePlan, PercentagePlan data, bool isAdmin = false);
        int PercentagePlanId(int store, string Name);
    }
}