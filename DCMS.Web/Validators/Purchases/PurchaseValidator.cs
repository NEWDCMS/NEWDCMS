using DCMS.ViewModel.Models.Purchases;
using DCMS.Web.Framework.Validators;
using FluentValidation;

namespace DCMS.ViewModel.Validators.Purchases
{


    public class PurchaseValidator : BaseDCMSValidator<PurchaseBillModel>
    {
        /// <summary>
        /// 采购单验证
        /// </summary>
        public PurchaseValidator()
        {

            RuleFor(x => x.ManufacturerName).NotNull().WithMessage("供应商名称不能为空");
            RuleFor(x => x.ManufacturerId).NotNull().WithMessage("供应商不能为空");

            RuleFor(x => x.BusinessUserName).NotNull().WithMessage("业务员名称不能为空");
            RuleFor(x => x.BusinessUserId).NotNull().WithMessage("业务员不能为空");

            RuleFor(x => x.WareHouseId).NotNull().WithMessage("仓库不能为空");
            RuleFor(x => x.TransactionDate).NotNull().WithMessage("交易日期不能为空");
            RuleFor(x => x.IsMinUnitPurchase).NotNull().WithMessage("按最小单位采购不能为空");
        }
    }

    public class PurchaseReturnValidator : BaseDCMSValidator<PurchaseReturnBillModel>
    {
        /// <summary>
        /// 采购退货单验证
        /// </summary>
        public PurchaseReturnValidator()
        {

            RuleFor(x => x.ManufacturerName).NotNull().WithMessage("供应商名称不能为空");
            RuleFor(x => x.ManufacturerId).NotNull().WithMessage("供应商不能为空");

            RuleFor(x => x.BusinessUserName).NotNull().WithMessage("业务员名称不能为空");
            RuleFor(x => x.BusinessUserId).NotNull().WithMessage("业务员不能为空");

            RuleFor(x => x.WareHouseId).NotNull().WithMessage("仓库不能为空");
            RuleFor(x => x.TransactionDate).NotNull().WithMessage("交易日期不能为空");
            RuleFor(x => x.IsMinUnitPurchase).NotNull().WithMessage("按最小单位采购不能为空");

        }
    }



}