using DCMS.Web.Framework.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace DCMS.ViewModel.Models.Messages
{

    public partial class QueuedEmailSearchModel : BaseSearchModel
    {



        [UIHint("DateNullable")]
        public DateTime? SearchStartDate { get; set; }


        [UIHint("DateNullable")]
        public DateTime? SearchEndDate { get; set; }

        [DataType(DataType.EmailAddress)]
        public string SearchFromEmail { get; set; }

        [DataType(DataType.EmailAddress)]
        public string SearchToEmail { get; set; }

        public bool SearchLoadNotSent { get; set; }

        public int SearchMaxSentTries { get; set; }


        public int GoDirectlyToNumber { get; set; }


    }
}