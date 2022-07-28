namespace DCMS.Core.Domain.Common
{

    /// <summary>
    /// 表示泛型属性
    /// </summary>
    public partial class GenericAttribute : BaseEntity
    {

        public int EntityId { get; set; }


        public string KeyGroup { get; set; }


        public string Key { get; set; }


        public string Value { get; set; }



    }
}
