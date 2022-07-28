using DCMS.ViewModel.Models.Plan;
using DCMS.Web.Framework.Validators;
using FluentValidation;

namespace DCMS.ViewModel.Validators.Plan
{
    public class PercentagePlanValidator : BaseDCMSValidator<PercentagePlanModel>
    {
        public PercentagePlanValidator()
        {
            RuleFor(x => x.Name).NotNull().WithMessage("名称不能为空");
        }
    }
}