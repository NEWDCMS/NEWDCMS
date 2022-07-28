using DCMS.Core.Domain.Users;
using System;


namespace DCMS.Core.Domain.Logging
{

    //public partial class ActivityLog : BaseEntity
    //{
    //    public int ActivityLogTypeId { get; set; }
    //    public int UserId { get; set; }
    //    public string Comment { get; set; }
    //    public DateTime CreatedOnUtc { get; set; }
    //    public virtual ActivityLogType ActivityLogType { get; set; }
    //    public virtual User User { get; set; }
    //}

    public partial class ActivityLog : BaseEntity
    {
        /// <summary>
        /// Gets or sets the activity log type identifier
        /// </summary>
        public int ActivityLogTypeId { get; set; }

        /// <summary>
        /// Gets or sets the entity identifier
        /// </summary>
        //public int? EntityId { get; set; }

        /// <summary>
        /// Gets or sets the entity name
        /// </summary>
        //public string EntityName { get; set; }

        /// <summary>
        /// Gets or sets the user identifier
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Gets or sets the activity comment
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        /// Gets or sets the date and time of instance creation
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }

        /// <summary>
        /// Gets the activity log type
        /// </summary>
        public virtual ActivityLogType ActivityLogType { get; set; }

        /// <summary>
        /// Gets the user
        /// </summary>
        public virtual User User { get; set; }

        /// <summary>
        /// Gets or sets the IP address
        /// </summary>
        //public virtual string IpAddress { get; set; }
    }
}
