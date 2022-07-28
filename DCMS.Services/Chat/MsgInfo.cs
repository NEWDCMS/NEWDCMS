using System;
using System.Collections.Generic;
using System.Text;

namespace DCMS.Services.Chat
{
    /// <summary>
    /// 通讯协议
    /// </summary>
    public class MsgInfo
    {
        /// <summary>
        /// 接收人
        /// </summary>
        public string to_id { get; set; }
        /// <summary>
        /// 发送人
        /// </summary>
        public string from_id { get; set; }
        /// <summary>
        /// 发送人昵称
        /// </summary>
        public string from_username { get; set; }
        /// <summary>
        /// 发送人头像
        /// </summary>
        public string from_userpic { get; set; }
        /// <summary>
        /// 发送类型 text,voice等等
        /// </summary>
        public string type { get; set; }
        /// <summary>
        /// 发送内容 
        /// </summary>
        public string data { get; set; }
        /// <summary>
        /// 接收到的时间 
        /// </summary>
        public long time { get; set; }
    }
}
