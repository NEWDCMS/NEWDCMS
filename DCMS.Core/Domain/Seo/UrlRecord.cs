using System.ComponentModel.DataAnnotations.Schema;
namespace DCMS.Core.Domain.Seo
{

    /// <summary>
    /// Represents an URL record
    /// </summary>
    public partial class UrlRecord : BaseEntity
    {

        public int EntityId { get; set; }

        public string EntityName { get; set; }

        public string Slug { get; set; }

        [Column(TypeName = "BIT(1)")]
        public bool IsActive { get; set; }
    }
}
