using DCMS.ViewModel.Models.Products;
using DCMS.Web.Framework.Validators;
using FluentValidation;

namespace DCMS.ViewModel.Validators.Products
{

    public class ProductValidator : BaseDCMSValidator<ProductModel>
    {
        public ProductValidator()
        {
            RuleFor(x => x.Name).NotNull().WithMessage("名称不能为空");
            RuleFor(x => x.CategoryId).NotNull().WithMessage("类别不能为空");
            RuleFor(x => x.SmallUnitId).NotNull().WithMessage("最小单位不能为空");
            RuleFor(x => x.MnemonicCode).NotNull().WithMessage("助记码不能为空");
            RuleFor(x => x.BrandId).NotNull().WithMessage("品牌不能为空");
        }
    }


    public class ProductCombinationValidator : BaseDCMSValidator<ProductCombinationModel>
    {
        public ProductCombinationValidator()
        {
            RuleFor(x => x.ProductId).NotNull().WithMessage("子商品不能为空");
            RuleFor(x => x.Quantity).NotNull().WithMessage("数量不能为空");
            RuleFor(x => x.UnitId).NotNull().WithMessage("最小单位不能为空");
        }
    }

    public class ProductFlavorValidator : BaseDCMSValidator<ProductFlavorModel>
    {
        public ProductFlavorValidator()
        {
            RuleFor(x => x.Name).NotNull().WithMessage("口味名称不能为空");
        }
    }


    public class ManufacturerValidator : BaseDCMSValidator<ManufacturerModel>
    {
        public ManufacturerValidator()
        {
            RuleFor(x => x.Name).NotNull().WithMessage("供应商名称不能为空");
        }
    }


    public class ProductTierPricePlanValidator : BaseDCMSValidator<ProductTierPricePlanModel>
    {
        public ProductTierPricePlanValidator()
        {
            RuleFor(x => x.Name).NotNull().WithMessage("名称不能为空");
        }
    }
}