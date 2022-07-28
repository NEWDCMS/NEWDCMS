using DCMS.ViewModel.Models.Campaigns;
using DCMS.Web.Framework.Validators;
using FluentValidation;


namespace DCMS.ViewModel.Validators.Campaigns
{
    public class CampaignValidator : BaseDCMSValidator<CampaignModel>
    {
        public CampaignValidator()
        {
            RuleFor(x => x.Name).NotNull().WithMessage("活动名称不能为空");
            RuleFor(x => x.StartTime).NotNull().WithMessage("开始时间不能为空");
            RuleFor(x => x.EndTime).NotNull().WithMessage("结束时间不能为空");
        }
    }
}