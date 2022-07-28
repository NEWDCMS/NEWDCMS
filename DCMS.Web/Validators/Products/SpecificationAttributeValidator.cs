using DCMS.ViewModel.Models.Products;
using DCMS.Web.Framework.Validators;
using FluentValidation;

namespace DCMS.ViewModel.Validators.Products
{
    public class SpecificationAttributeValidator : BaseDCMSValidator<SpecificationAttributeModel>
    {
        public SpecificationAttributeValidator()
        {
            RuleFor(x => x.Name).NotNull().WithMessage("属性名不能为空");
        }
    }
}