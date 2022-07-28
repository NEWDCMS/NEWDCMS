using System;
using System.Collections.Generic;
using System.Text;

namespace DCMS.Core.Domain.Chat
{
    public class ChatRoom : BaseEntity
    {
        /// <summary>
        /// 发送人
        /// </summary>
        public string SenderName { get; set; }
        /// <summary>
        /// 接收人
        /// </summary>
        public string RecieverName { get; set; }

        public int SenderId { get; set; }
        public int RecieverId { get; set; }

    }
}
