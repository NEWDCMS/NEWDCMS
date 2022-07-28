namespace DCMS.Core.Domain.Common
{
    public partial class SearchTerm : BaseEntity
    {

        public string Keyword { get; set; }



        public int Count { get; set; }
    }
}
