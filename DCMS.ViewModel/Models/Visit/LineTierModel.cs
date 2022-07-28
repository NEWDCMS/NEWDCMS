using DCMS.Core.Domain.Visit;
using DCMS.ViewModel.Models.Terminals;
using DCMS.ViewModel.Models.Users;
using DCMS.Web.Framework.Models;
using System.Collections.Generic;


namespace DCMS.ViewModel.Models.Visit
{
    public partial class LineTierListModel : BaseModel
    {
        public LineTierListModel()
        {
            PagingFilteringContext = new PagingFilteringModel();
            LineTiers = new List<LineTierModel>();
            LineTierOptions = new List<LineTierOption>();

            UserLineTierAssigns = new List<UserLineTierAssignModel>();
            BusinessUsers = new List<UserModel>();
        }

        public PagingFilteringModel PagingFilteringContext { get; set; }
        public IList<LineTierModel> LineTiers { get; set; }
        public IList<LineTierOption> LineTierOptions { get; set; }

        public List<UserLineTierAssignModel> UserLineTierAssigns { get; set; }
        public List<UserModel> BusinessUsers { get; set; }
    }


    public partial class LineTierModel : BaseEntityModel
    {


        /// <summary>
        /// 线路类别名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 拜访终端数量
        /// </summary>
        public int Qty { get; set; } = 0;
        /// <summary>
        /// 是否启用
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }

        public List<TerminalModel> Terminals { get; set; } = new List<TerminalModel>();
    }

    public partial class LineTierOptionModel : BaseEntityModel
    {
        /// <summary>
        /// 类别Id
        /// </summary>
        public int LineTierId { get; set; } = 0;

        /// <summary>
        /// 拜访顺序
        /// </summary>
        public int Order { get; set; } = 0;

        /// <summary>
        /// 客户(终端)
        /// </summary>
        public int TerminalId { get; set; } = 0;
        public string TerminalName { get; set; }
        public string TerminalAddress { get; set; }
        public string BossName { get; set; }
        public string BossCall { get; set; }

    }

    public partial class UserLineTierAssignModel : BaseEntityModel
    {
        /// <summary>
        /// 线路顺序
        /// </summary>
        public int Order { get; set; } = 0;

        /// <summary>
        ///业务员
        /// </summary>
        public int UserId { get; set; } = 0;

        /// <summary>
        /// 线路
        /// </summary>
        public int LineTierId { get; set; } = 0;
        public string LineTierName { get; set; }

        /// <summary>
        /// 客户数量
        /// </summary>
        public int Quantity { get; set; } = 0;
    }

    public class LineTierUpdateModel : BaseEntityModel
    {

        /// <summary>
        /// 项目
        /// </summary>
        public List<LineTierOptionModel> Items { get; set; }

    }


}