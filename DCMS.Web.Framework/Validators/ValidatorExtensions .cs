using DCMS.Core.Domain.Users;
using FluentValidation;


namespace DCMS.Web.Framework.Validators
{

	public static class ValidatorExtensions
	{

		//3.1
		//  public static IRuleBuilderOptions<T, decimal> IsDecimal<T>(this IRuleBuilder<T, decimal> ruleBuilder, decimal maxValue)
		//{
		//    return ruleBuilder.SetValidator(new DecimalPropertyValidator(maxValue));
		//}

		//6.0
		public static IRuleBuilderOptions<TModel, decimal> IsDecimal<TModel>(this IRuleBuilder<TModel, decimal> ruleBuilder, decimal maxValue)
		{
			return ruleBuilder.SetValidator(new DecimalPropertyValidator<TModel, decimal>(maxValue));
		}



		//3.1
		//public static IRuleBuilderOptions<T, string> IsUsername<T>(this IRuleBuilder<T, string> ruleBuilder, UserSettings userSettings)
		//{
		//    return ruleBuilder.SetValidator(new UsernamePropertyValidator(userSettings));
		//}

		//6.0
		public static IRuleBuilderOptions<TModel, string> IsUsername<TModel>(this IRuleBuilder<TModel, string> ruleBuilder,
	   UserSettings userSettings)
		{
			return ruleBuilder.SetValidator(new UsernamePropertyValidator<TModel, string>(userSettings));
		}



		public static IRuleBuilder<T, string> IsPassword<T>(this IRuleBuilder<T, string> ruleBuilder, UserSettings userSettings)
		{
			var regExp = "^";
			//Passwords must be at least X characters and contain the following: upper case (A-Z), lower case (a-z), number (0-9) and special character (e.g. !@#$%^&*-)
			regExp += userSettings.PasswordRequireUppercase ? "(?=.*?[A-Z])" : "";
			regExp += userSettings.PasswordRequireLowercase ? "(?=.*?[a-z])" : "";
			regExp += userSettings.PasswordRequireDigit ? "(?=.*?[0-9])" : "";
			regExp += userSettings.PasswordRequireNonAlphanumeric ? "(?=.*?[#?!@$%^&*-])" : "";
			regExp += $".{{{userSettings.PasswordMinLength},}}$";

			var message = string.Format("Validation.Password.Rule",
				string.Format("Validation.Password.LengthValidation", userSettings.PasswordMinLength),
				userSettings.PasswordRequireUppercase ? "Validation.Password.RequireUppercase" : "",
				userSettings.PasswordRequireLowercase ? "Validation.Password.RequireLowercase" : "",
				userSettings.PasswordRequireDigit ? "Validation.Password.RequireDigit" : "",
				userSettings.PasswordRequireNonAlphanumeric ? "Validation.Password.RequireNonAlphanumeric" : "");

			var options = ruleBuilder
				.NotEmpty().WithMessage("Validation.Password.IsNotEmpty")
				.Matches(regExp).WithMessage(message);

			return options;
		}

	}
}
