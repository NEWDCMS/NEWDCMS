namespace DCMS.Core.Domain.Chat
{
    public class User : BaseEntity
    {
        public string Name { get; set; }
        public string Avatar { get; set; }
        public string MobileNumber { get; set; }
        public int UserId { get; set; }
        public string OpenId { get; set; }
    }
}
