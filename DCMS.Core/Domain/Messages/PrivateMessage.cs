using System;

namespace DCMS.Core.Domain.Messages
{
    /// <summary>
    /// 表示用户私信
    /// </summary>
    public partial class PrivateMessage : BaseEntity
    {

        public int FromUserId { get; set; }
        public int ToUserId { get; set; }
        public string Subject { get; set; }
        public string Text { get; set; }
        public bool IsRead { get; set; }
        public bool IsDeletedByAuthor { get; set; }
        public bool IsDeletedByRecipient { get; set; }
        public DateTime CreatedOnUtc { get; set; }
    }
}
