namespace DCMS.Services.Messages
{
    /// <summary>
    /// 消息结构
    /// </summary>
    public struct NotifyData
    {
        /// <summary>
        /// 类型 (success/warning/error)
        /// </summary>
        public NotifyType Type { get; set; }

        /// <summary>
        /// 消息文本
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 是否对消息进行HTML编码
        /// </summary>
        public bool Encode { get; set; }
    }
}
