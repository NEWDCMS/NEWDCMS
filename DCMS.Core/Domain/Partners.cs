using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace DCMS.Core.Domain.Users
{
    /// <summary>
    /// （合作者账户）用于API 防伪造签名认证
    /// </summary>
    public partial class Partner : BaseEntity
    {
        public Partner()
        {
            UserGuid = Guid.NewGuid().ToString();
        }

        public string UserGuid { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public int PasswordFormatId { get; set; }
        public string PasswordSalt { get; set; }
        [Column(TypeName = "BIT(1)")]
        public bool Active { get; set; }

        [Column(TypeName = "BIT(1)")]
        public bool IsSystemAccount { get; set; }
        public string LastIpAddress { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public string AccessKey { get; set; }
    }
}
