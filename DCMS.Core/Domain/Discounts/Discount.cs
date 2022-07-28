using System;
using System.Collections.Generic;

namespace DCMS.Core.Domain.Discounts
{

    public partial class Discount : BaseEntity
    {
        private ICollection<DiscountRequirement> _discountRequirements;
        private ICollection<DiscountCategoryMapping> _discountCategoryMappings;
        private ICollection<DiscountManufacturerMapping> _discountManufacturerMappings;
        private ICollection<DiscountProductMapping> _discountProductMappings;


        public string Name { get; set; }
        public int DiscountTypeId { get; set; }

        public bool UsePercentage { get; set; }

        public decimal DiscountPercentage { get; set; }

        public decimal DiscountAmount { get; set; }

        public decimal? MaximumDiscountAmount { get; set; }

        public DateTime? StartDateUtc { get; set; }

        public DateTime? EndDateUtc { get; set; }

        public bool RequiresCouponCode { get; set; }

        public string CouponCode { get; set; }

        public bool IsCumulative { get; set; }

        public int DiscountLimitationId { get; set; }

        public int LimitationTimes { get; set; }

        public int? MaximumDiscountedQuantity { get; set; }

        public bool AppliedToSubCategories { get; set; }

        public DiscountType DiscountType
        {
            get => (DiscountType)DiscountTypeId;
            set => DiscountTypeId = (int)value;
        }

        public DiscountLimitationType DiscountLimitation
        {
            get => (DiscountLimitationType)DiscountLimitationId;
            set => DiscountLimitationId = (int)value;
        }

        public virtual ICollection<DiscountRequirement> DiscountRequirements
        {
            get => _discountRequirements ?? (_discountRequirements = new List<DiscountRequirement>());
            protected set => _discountRequirements = value;
        }

        public virtual ICollection<DiscountCategoryMapping> DiscountCategoryMappings
        {
            get => _discountCategoryMappings ?? (_discountCategoryMappings = new List<DiscountCategoryMapping>());
            protected set => _discountCategoryMappings = value;
        }

        public virtual ICollection<DiscountManufacturerMapping> DiscountManufacturerMappings
        {
            get => _discountManufacturerMappings ?? (_discountManufacturerMappings = new List<DiscountManufacturerMapping>());
            protected set => _discountManufacturerMappings = value;
        }

        public virtual ICollection<DiscountProductMapping> DiscountProductMappings
        {
            get => _discountProductMappings ?? (_discountProductMappings = new List<DiscountProductMapping>());
            protected set => _discountProductMappings = value;
        }
    }
}
