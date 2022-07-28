using DCMS.ViewModel.Models.Products;
using DCMS.Web.Framework.Validators;
using FluentValidation;

namespace DCMS.ViewModel.Validators.Products
{
    public class SpecificationAttributeOptionValidator : BaseDCMSValidator<SpecificationAttributeOptionModel>
    {
        public SpecificationAttributeOptionValidator()
        {
            RuleFor(x => x.Name).NotNull().WithMessage("名称不能为空");
        }
    }
}