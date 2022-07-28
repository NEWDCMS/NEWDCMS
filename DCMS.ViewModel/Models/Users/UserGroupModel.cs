//
using DCMS.Web.Framework;
using DCMS.Web.Framework.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;


namespace DCMS.ViewModel.Models.Users
{

    //[Validator(typeof(UserGroupValidator))]
    public class UserGroupModel : BaseEntityModel
    {

        public int StoreName { get; set; }
        public SelectList Stores { get; set; }


        [HintDisplayName("组名", "组名")]
        public string GroupName { get; set; }

        [HintDisplayName("描述", "描述")]
        public string Description { set; get; }

        [HintDisplayName("排序", "排序")]
        public int OrderSort { get; set; }

        [HintDisplayName("是否启用", "是否启用")]
        public bool Enabled { get; set; }

        [HintDisplayName("创建时间", "创建时间")]
        public DateTime CreatedOnUtc { get; set; }


    }
}