using System.ComponentModel.DataAnnotations.Schema;

namespace DCMS.Core.Domain.Stores
{
    public partial class Corporations : BaseEntity
    {

        public int? ParendId { get; set; } = 0;

        public int? FactoryId { get; set; } = 0;

        public int CorporationType { get; set; }

        public string CorporationTypeName { get; set; }
        public string ShortName { get; set; }

        public string FullName { get; set; }
        public string Code { get; set; }
        public string LevelCode { get; set; }
        [Column(TypeName = "BIT(1)")]
        public bool Actived { get; set; }
        public string FMS_ORG_CODE { get; set; }
        public string FMS_SYS_CODE { get; set; }
        public string FMS_MS_USER { get; set; }
        public string FMS_WS_PWD { get; set; }
        public string FMS_URL { get; set; }
        public string ERP_SELFMADE_ORG_CODE { get; set; }
        public string ERP_BOUGHT_ORG_CODE { get; set; }
    }
}
