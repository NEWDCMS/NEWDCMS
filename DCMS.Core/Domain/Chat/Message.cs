using System;
using System.Collections.Generic;
using System.Text;

namespace DCMS.Core.Domain.Chat
{
    public class Message : BaseEntity
    {
        public int ChatId { get; set; }

        public string Content { get; set; }
        public string SenderName { get; set; }
        public string RecieverName { get; set; }

        public int SenderId { get; set; }
        public int RecieverId { get; set; }

        /// <summary>
        /// 发送人头像
        /// </summary>
        public string SenderAvatar { get; set; }

        /// <summary>
        ///  消息类型 text,voice,image
        /// </summary>
        public string Type { get; set; }

        public string Images { get; set; }

        /// <summary>
        /// 接收时间
        /// </summary>
        public DateTime Time { get; set; }

    }
}
