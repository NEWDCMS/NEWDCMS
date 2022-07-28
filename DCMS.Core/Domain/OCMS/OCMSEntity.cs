using System;
using System.Collections.Generic;
using System.Text;

namespace DCMS.Core.Domain.OCMS
{
    public class OCMS_CharacterSetting: BaseEntity
    {

        public int CUSTOMER_ID { get; set; }

        public int CORPORATION_ID { get; set; }

        public int FACTORY_ID { get; set; }

        public int MD_PRODUCT_ID { get; set; }

        public int SALES_REGION_ID { get; set; }

        public int SALES_SITE_ID { get; set; }

        public decimal PRICE { get; set; }

        public DateTime UPDATED_DATE { get; set; }

        public string RegionCode { get; set; }

        public string CUSTOMER_CODE { get; set; }
    }

    public class OCMS_Products : BaseEntity 
    {
        public string PRODUCT_CODE { get; set; }

        public string PRODUCT_NAME { get; set; }

        public string PRODUCT_SHORT_NAME { get; set; }
        
        public string PRODUCT_UNIT_NAME { get; set; }

        public string IS_PACKING_SALES { get; set; }

        public string INNER_PACKING_Code { get; set; }

        public string OUTER_PACKING_Code { get; set; }

        public string PRODUCT_BRAND_NAME { get; set; }

        public string PRODUCT_STYLE_NAME { get; set; }

        public string PRODUCT_CLASS_NAME { get; set; }

        public string MALT_SUGAR_RATE { get; set; }

        public string SHELF_LIFE { get; set; }

        public string BALANCE_BOTTLE_LEVEL_ID { get; set; }
        
        public int IS_ACTIVED { get; set; }

        public string FMS_CODE { get; set; }

        public string PRODUCT_TYPE { get; set; }

        public string BOTTLE_BOX { get; set; }

        public string LITER_CONVERSION_RATE { get; set; }

        public decimal WEIGHT { get; set; }

        public string CAPACITY { get; set; }

        public string CAPACITY_UNIT { get; set; }

        public string REMARK { get; set; }

        public string BALANCE_BOTTLE_LEVEL_NAME { get; set; }

        public DateTime UPDATE_TIME { get; set; }

        public string RegionCode { get; set; }

        public int OCMS_PRODUCTID { get; set; }
    }
}
