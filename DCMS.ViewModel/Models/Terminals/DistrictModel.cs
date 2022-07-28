using DCMS.Web.Framework.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DCMS.ViewModel.Models.Terminals
{
    public partial class DistrictModel : BaseEntityModel
    {

        /// <summary>
        /// 片区名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 父Id
        /// </summary>
        public int ParentId { get; set; } = 0;
        /// <summary>
        /// 排序号
        /// </summary>
        public int? OrderNo { get; set; } = 0;
        /// <summary>
        /// 描述
        /// </summary>
        public string Describe { get; set; }
        /// <summary>
        /// 是否删除
        /// </summary>
        public bool Deleted { get; set; }

        public SelectList DistrictList { get; set; }
    }

}