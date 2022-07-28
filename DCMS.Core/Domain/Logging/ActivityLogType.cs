namespace DCMS.Core.Domain.Logging
{
    using System.ComponentModel.DataAnnotations.Schema;
    public partial class ActivityLogType : BaseEntity
    {

        public string SystemKeyword { get; set; }


        public string Name { get; set; }

        [Column(TypeName = "BIT(1)")]
        public bool Enabled { get; set; }
    }
}
