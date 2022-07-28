using DCMS.Core.Domain.Users;
using System;
namespace DCMS.Core.Domain.Logging
{

    public partial class Log : BaseEntity
    {

        public int LogLevelId { get; set; }

        public string ShortMessage { get; set; }


        public string FullMessage { get; set; }

        public string IpAddress { get; set; }

        public int? UserId { get; set; }

        public string PageUrl { get; set; }


        public string ReferrerUrl { get; set; }

        public DateTime CreatedOnUtc { get; set; }


        public LogLevel LogLevel
        {
            get
            {
                return (LogLevel)LogLevelId;
            }
            set
            {
                LogLevelId = (int)value;
            }
        }

        public virtual User User { get; set; }
    }
}
