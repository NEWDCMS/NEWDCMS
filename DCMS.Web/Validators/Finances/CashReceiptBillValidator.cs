using DCMS.ViewModel.Models.Finances;
using DCMS.Web.Framework.Validators;
using FluentValidation;

namespace DCMS.ViewModel.Validators.Finances
{
    public class CashReceiptBillValidator : BaseDCMSValidator<CashReceiptBillModel>
    {
        public CashReceiptBillValidator()
        {
            RuleFor(x => x.CustomerName).NotNull().WithMessage("客户名不能为空");
            RuleFor(x => x.Payeer).NotNull().WithMessage("收款人不能为空");
        }
    }
}