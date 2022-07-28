using DCMS.Api.Infrastructure.Mapper.Extensions;
using DCMS.Core;
using DCMS.Services.WareHouses;
using DCMS.ViewModel.Models.WareHouses;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;
using Microsoft.AspNetCore.Authorization;
using DCMS.Core.Domain.WareHouses;

namespace DCMS.Api.Controllers
{
    /// <summary>
    /// 用于仓库管理
    /// </summary>
   [Authorize]
    [Route("api/v{version:apiVersion}/dcms/warehouse")]
    public class WareHouseController : BaseAPIController
    {
        private readonly IWareHouseService _wareHouseService;

        /// <summary>
        /// 用于仓库管理
        /// </summary>
        /// <param name="wareHouseService"></param>
        public WareHouseController(IWareHouseService wareHouseService, ILogger<BaseAPIController> logger) : base(logger)
        {
            _wareHouseService = wareHouseService;
        }


        /// <summary>
        /// 获取经销商仓库信息
        /// </summary>
        /// <param name="store">经销商标识</param>
        /// <param name="makeuserId">用户</param>
        /// <param name="btype">单据类型</param>
        /// <param name="searchStr"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet("wareHouse/getWareHouses/{store}")]
        [SwaggerOperation("getWareHouses")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<IList<WareHouseModel>>> GetWareHouses(int? store, int? makeuserId, int? btype, string searchStr, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error<WareHouseModel>(Resources.ParameterError);

            return await Task.Run(() =>
            {
                try
                {
                    var model = new List<WareHouseModel>();

                    //仓库
                    model = _wareHouseService.GetWareHouseList(searchStr, store != null ? store : 0, 0, pageIndex, pageSize)
                    .Select(r =>
                    {
                        r.WareHouseAccess = JsonConvert.DeserializeObject<List<WareHouseAccess>>(r.WareHouseAccessSettings);
                        if (btype != null && btype > 0 && makeuserId > 0)
                        {
                            var bs = r.WareHouseAccess?.Where(b => b.UserId == makeuserId).Select(b => b.BillTypes).FirstOrDefault();
                            if (bs?.Where(bts => bts.Selected)?.Select(bts => bts.BillTypeId).Contains(btype ?? 0) ?? false)
                            {
                                return r.ToModel<WareHouseModel>();
                            }
                            else
                            {
                                return null;
                            }
                        }
                        else if (btype == 0 && makeuserId > 0)
                        {
                            var bs = r.WareHouseAccess?.Where(b => b.UserId == makeuserId && b.StockQuery == true).FirstOrDefault();
                            if (bs != null)
                            {
                                return r.ToModel<WareHouseModel>();
                            }
                            return null;
                        }
                        else 
                        {
                            return null;
                        }
                    }).Where(s => s != null).ToList();

                    return this.Successful(Resources.Successful, model);
                }
                catch (Exception ex)
                {
                    return this.Error<WareHouseModel>(ex.Message);
                }
            });
        }
    }
}