using DCMS.ViewModel.Models.Products;
using DCMS.Web.Framework.Validators;
using FluentValidation;


namespace DCMS.ViewModel.Validators.Products
{
    public class StatisticalTypeValidator : BaseDCMSValidator<StatisticalTypeModel>
    {
        public StatisticalTypeValidator()
        {
            RuleFor(x => x.Name).NotNull().WithMessage("名称不能为空");
            RuleFor(x => x.Value).NotNull().WithMessage("值不能为空");
        }
    }
}