using DCMS.Core.Domain.Users;
using FluentValidation.Validators;
using System.Linq;
using System.Text.RegularExpressions;
using System;
using FluentValidation;


namespace DCMS.Web.Framework.Validators
{


    public class UsernamePropertyValidator<T, TProperty> : PropertyValidator<T, TProperty>
    {
        private readonly UserSettings _userSettings;

        public override string Name => "UsernamePropertyValidator";

        /// <summary>
        /// Ctor
        /// </summary>
        public UsernamePropertyValidator(UserSettings userSettings)
        {
            _userSettings = userSettings;
        }


        public override bool IsValid(ValidationContext<T> context, TProperty value)
        {
            return IsValid(value as string, _userSettings);
        }

        /// <summary>
        /// Is valid?
        /// </summary>
        /// <param name="username">Username</param>
        /// <param name="customerSettings">Customer settings</param>
        /// <returns>Result</returns>
        public static bool IsValid(string username, UserSettings userSettings)
        {
            if (!userSettings.UsernameValidationEnabled || string.IsNullOrEmpty(userSettings.UsernameValidationRule))
                return true;

            if (string.IsNullOrEmpty(username))
                return false;

            return userSettings.UsernameValidationUseRegex
                ? Regex.IsMatch(username, userSettings.UsernameValidationRule, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase)
                : username.All(l => userSettings.UsernameValidationRule.Contains(l));
        }

        protected override string GetDefaultMessageTemplate(string errorCode) => "Username is not valid";
    }


    //public class UsernamePropertyValidator : PropertyValidator
    //{
    //    private readonly UserSettings _userSettings;

    //    /// <summary>
    //    /// Ctor
    //    /// </summary>
    //    public UsernamePropertyValidator(UserSettings userSettings)
    //        : base("Username is not valid")
    //    {
    //        _userSettings = userSettings;
    //    }

    //    /// <summary>
    //    /// Is valid?
    //    /// </summary>
    //    /// <param name="context">Validation context</param>
    //    /// <returns>Result</returns>
    //    protected override bool IsValid(PropertyValidatorContext context)
    //    {
    //        return IsValid(context.PropertyValue as string, _userSettings);
    //    }

    //    /// <summary>
    //    /// Is valid?
    //    /// </summary>
    //    /// <param name="username">Username</param>
    //    /// <param name="userSettings">User settings</param>
    //    /// <returns>Result</returns>
    //    public static bool IsValid(string username, UserSettings userSettings)
    //    {
    //        if (!userSettings.UsernameValidationEnabled || string.IsNullOrEmpty(userSettings.UsernameValidationRule))
    //        {
    //            return true;
    //        }

    //        if (string.IsNullOrEmpty(username))
    //        {
    //            return false;
    //        }

    //        return userSettings.UsernameValidationUseRegex
    //            ? Regex.IsMatch(username, userSettings.UsernameValidationRule, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase)
    //            : username.All(l => userSettings.UsernameValidationRule.Contains(l));
    //    }
    //}
}
