using DCMS.Web.Framework.Models;
using DCMS.Web.Framework.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace DCMS.ViewModel.Models.Messages
{

    public partial class EmailAccountModel : BaseEntityModel
    {
        #region Properties

        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        public string DisplayName { get; set; }


        public string Host { get; set; }


        public int Port { get; set; }


        public string Username { get; set; }


        [DataType(DataType.Password)]
        [DCMSTrim]
        public string Password { get; set; }


        public bool EnableSsl { get; set; }


        public bool UseDefaultCredentials { get; set; }


        public bool IsDefaultEmailAccount { get; set; }


        public string SendTestEmailTo { get; set; }

        #endregion
    }
}