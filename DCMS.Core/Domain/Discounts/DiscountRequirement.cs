using System.Collections.Generic;

namespace DCMS.Core.Domain.Discounts
{

    public partial class DiscountRequirement : BaseEntity
    {
        private ICollection<DiscountRequirement> _childRequirements;


        public int DiscountId { get; set; }

        public string DiscountRequirementRuleSystemName { get; set; }


        public int? ParentId { get; set; }


        public int? InteractionTypeId { get; set; }


        public bool IsGroup { get; set; }


        public RequirementGroupInteractionType? InteractionType
        {
            get => (RequirementGroupInteractionType?)InteractionTypeId;
            set => InteractionTypeId = (int?)value;
        }

        public virtual Discount Discount { get; set; }

        public virtual ICollection<DiscountRequirement> ChildRequirements
        {
            get => _childRequirements ?? (_childRequirements = new List<DiscountRequirement>());
            protected set => _childRequirements = value;
        }
    }
}