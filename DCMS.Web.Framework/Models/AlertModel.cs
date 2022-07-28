namespace DCMS.Web.Framework.Models
{
    /// <summary>
    /// Alert模拟窗体
    /// </summary>
    public class ActionAlertModel : BaseEntityModel
    {
        /// <summary>
        /// Window ID
        /// </summary>
        public string WindowId { get; set; }
        /// <summary>
        /// Alert ID
        /// </summary>
        public string AlertId { get; set; }
        /// <summary>
        /// Alert message
        /// </summary>
        public string AlertMessage { get; set; }
    }
}
