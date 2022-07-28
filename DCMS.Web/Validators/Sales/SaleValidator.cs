using DCMS.ViewModel.Models.Sales;
using DCMS.Web.Framework.Validators;
using FluentValidation;

namespace DCMS.ViewModel.Validators.Sales
{

    public class SaleReservationBillValidator : BaseDCMSValidator<SaleReservationBillModel>
    {
        /// <summary>
        /// 销售订单验证
        /// </summary>
        public SaleReservationBillValidator()
        {

            RuleFor(x => x.TerminalName).NotNull().WithMessage("客户名称不能为空");
            RuleFor(x => x.BusinessUserId).NotNull().WithMessage("业务员不能为空");
            RuleFor(x => x.WareHouseId).NotNull().WithMessage("仓库不能为空");
            RuleFor(x => x.PayTypeId).NotNull().WithMessage("付款方式不能为空");
            RuleFor(x => x.TransactionDate).NotNull().WithMessage("交易日期不能为空");
            RuleFor(x => x.DefaultAmountId).NotNull().WithMessage("默认售价不能为空");

        }
    }

    public class SaleValidator : BaseDCMSValidator<SaleBillModel>
    {
        /// <summary>
        /// 销售单验证
        /// </summary>
        public SaleValidator()
        {

            RuleFor(x => x.TerminalName).NotNull().WithMessage("客户名称不能为空");
            RuleFor(x => x.BusinessUserId).NotNull().WithMessage("业务员不能为空");
            RuleFor(x => x.WareHouseId).NotNull().WithMessage("仓库不能为空");
            RuleFor(x => x.DeliveryUserId).NotNull().WithMessage("送货员不能为空");
            RuleFor(x => x.TransactionDate).NotNull().WithMessage("交易日期不能为空");
            RuleFor(x => x.DefaultAmountId).NotNull().WithMessage("默认售价不能为空");

        }
    }

    public class ReturnReservationValidator : BaseDCMSValidator<ReturnReservationBillModel>
    {
        /// <summary>
        /// 退货订单验证
        /// </summary>
        public ReturnReservationValidator()
        {

            RuleFor(x => x.TerminalName).NotNull().WithMessage("客户名称不能为空");
            RuleFor(x => x.BusinessUserId).NotNull().WithMessage("业务员不能为空");
            RuleFor(x => x.WareHouseId).NotNull().WithMessage("仓库不能为空");
            RuleFor(x => x.PayTypeId).NotNull().WithMessage("付款方式不能为空");
            RuleFor(x => x.TransactionDate).NotNull().WithMessage("交易日期不能为空");
            RuleFor(x => x.DefaultAmountId).NotNull().WithMessage("默认售价不能为空");

        }
    }

    public class ReturnValidator : BaseDCMSValidator<ReturnBillModel>
    {
        /// <summary>
        /// 退货单验证
        /// </summary>
        public ReturnValidator()
        {

            RuleFor(x => x.TerminalName).NotNull().WithMessage("客户名称不能为空");
            RuleFor(x => x.BusinessUserId).NotNull().WithMessage("业务员不能为空");
            RuleFor(x => x.WareHouseId).NotNull().WithMessage("仓库不能为空");
            RuleFor(x => x.DeliveryUserId).NotNull().WithMessage("送货员不能为空");
            RuleFor(x => x.TransactionDate).NotNull().WithMessage("交易日期不能为空");
            RuleFor(x => x.DefaultAmountId).NotNull().WithMessage("默认售价不能为空");

        }
    }

    public class ChangeReservationValidatorUpdate : BaseDCMSValidator<ChangeReservationUpdateModel>
    {
        /// <summary>
        /// 订单转销售单验证
        /// </summary>
        public ChangeReservationValidatorUpdate()
        {
            //销售订单（退货订单）为什么收款方式，则销售单（退货单）也相同
            //RuleFor(x => x.AccountingId).NotNull().WithMessage("收款方式不能为空");
            //仓库为空，则 销售订单（退货订单）的仓库 等于 销售单（退货单）的仓库
            //RuleFor(x => x.WareHouseId).NotNull().WithMessage("仓库不能为空");
            RuleFor(x => x.DeliveryUserId).NotNull().WithMessage("送货员不能为空");
            //交易日期为空，则 销售订单（退货订单）的交易日期 等于 销售单（退货单）的交易日期
            //RuleFor(x => x.TransactionDate).NotNull().WithMessage("交易日期不能为空");


        }
    }


}