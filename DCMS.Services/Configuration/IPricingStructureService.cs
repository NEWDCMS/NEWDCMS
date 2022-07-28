using DCMS.Core;
using DCMS.Core.Domain.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DCMS.Services.Configuration
{
    public interface IPricingStructureService
    {
        void DeletePricingStructure(PricingStructure pricingStructures);
        IList<PricingStructure> GetAllPricingStructures(int? store);
        IPagedList<PricingStructure> GetAllPricingStructures(int? store, int? type, string name = null, int pageIndex = 0, int pageSize = int.MaxValue);
        PricingStructure GetPricingStructureById(int? store, int pricingStructuresId);
        IList<PricingStructure> GetPricingStructuresByIds(int[] sIds);
        void InsertPricingStructure(PricingStructure pricingStructures);
        void UpdatePricingStructure(PricingStructure pricingStructures);

        Task<IPagedList<PricingStructure>> AsyncGetAllPricingStructures(int? store, int? type, string name = null, int pageIndex = 0, int pageSize = int.MaxValue);
        Task<IList<PricingStructure>> AsyncGetAllPricingStructures(int? store);
        Task<PricingStructure> AsyncGetPricingStructureById(int pricingStructuresId);
        Task<IList<PricingStructure>> AsyncGetPricingStructuresByIds(int[] sIds);

        BaseResult CreateOrUpdate(int storeId, int userId, List<PricingStructure> pricingStructures);

    }
}