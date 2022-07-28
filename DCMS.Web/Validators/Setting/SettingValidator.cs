using DCMS.ViewModel.Models.Configuration;
using DCMS.Web.Framework.Validators;
using FluentValidation;

namespace DCMS.ViewModel.Validators.Setting
{
    public class SettingValidator : BaseDCMSValidator<SettingModel>
    {
        public SettingValidator()
        {
            RuleFor(x => x.Name).NotNull().WithMessage("配置名称不能为空");
        }
    }


    public class StockEarlyWarningValidator : BaseDCMSValidator<StockEarlyWarningModel>
    {
        public StockEarlyWarningValidator()
        {
            RuleFor(x => x.WareHouseId).NotNull().NotEqual(0).WithMessage("仓库不能为空");
            RuleFor(x => x.ProductId).NotNull().NotEqual(0).WithMessage("商品不能为空");
            RuleFor(x => x.UnitId).NotNull().NotEqual(0).WithMessage("单位不能为空");
        }
    }

    public class PrintTemplateValidator : BaseDCMSValidator<PrintTemplateModel>
    {
        public PrintTemplateValidator()
        {
            RuleFor(x => x.TemplateType).NotNull().NotEqual(-1).WithMessage("请选择模板类型");
            RuleFor(x => x.BillType).NotNull().NotEqual(-1).WithMessage("请选择单据类型");
            RuleFor(x => x.Title).NotNull().WithMessage("标题不能为空");
            RuleFor(x => x.Content).NotNull().WithMessage("内容不能为空");

        }

    }

    public class RemarkConfigValidator : BaseDCMSValidator<RemarkConfigModel>
    {
        public RemarkConfigValidator()
        {
            RuleFor(x => x.Name).NotNull().WithMessage("标题不能为空");
        }

    }

}