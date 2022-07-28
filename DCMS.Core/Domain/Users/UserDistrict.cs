namespace DCMS.Core.Domain.Users
{
    /// <summary>
    /// 用于表示用于片区映射
    /// </summary>
    public partial class UserDistricts : BaseEntity
    {
        public int UserId { get; set; }
        public int DistrictId { get; set; }
    }
}
