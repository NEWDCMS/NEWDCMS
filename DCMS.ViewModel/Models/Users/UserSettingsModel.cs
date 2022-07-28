using DCMS.Web.Framework.Models;

namespace DCMS.Core.Domain.Users
{

    /// <summary>
    /// 用户系统设置
    /// </summary>
    public class UserSettingsModel : BaseModel
    {

        public bool UsernamesEnabled { get; set; }

        public bool CheckUsernameAvailabilityEnabled { get; set; }

        public bool AllowUsersToChangeUsernames { get; set; }

        public bool UsernameValidationEnabled { get; set; }

        public bool UsernameValidationUseRegex { get; set; }

        public string UsernameValidationRule { get; set; }

        public PasswordFormat DefaultPasswordFormat { get; set; }

        public string HashedPasswordFormat { get; set; }

        public int PasswordMinLength { get; set; }

        public bool PasswordRequireLowercase { get; set; }

        public bool PasswordRequireUppercase { get; set; }

        public bool PasswordRequireNonAlphanumeric { get; set; }

        public bool PasswordRequireDigit { get; set; }

        public int UnduplicatedPasswordsNumber { get; set; }

        public int PasswordRecoveryLinkDaysValid { get; set; }
        public int PasswordLifetime { get; set; }
        public int FailedPasswordAllowedAttempts { get; set; }
        public int FailedPasswordLockoutMinutes { get; set; }
        public UserRegistrationType UserRegistrationType { get; set; }
        public bool AllowUsersToUploadAvatars { get; set; }
        public int AvatarMaximumSizeBytes { get; set; }
        public bool DefaultAvatarEnabled { get; set; }
        public bool ShowUsersLocation { get; set; }
        public bool ShowUsersJoinDate { get; set; }
        public bool AllowViewingProfiles { get; set; }
        public bool NotifyNewUserRegistration { get; set; }
        public bool HideDownloadableProductsTab { get; set; }
        public bool HideBackInStockSubscriptionsTab { get; set; }
        public bool DownloadableProductsValidateUser { get; set; }
        public UserNameFormat UserNameFormat { get; set; }
        public bool NewsletterEnabled { get; set; }
        public bool NewsletterTickedByDefault { get; set; }
        public bool HideNewsletterBlock { get; set; }
        public bool NewsletterBlockAllowToUnsubscribe { get; set; }
        public int OnlineUserMinutes { get; set; }
        public bool StoreLastVisitedPage { get; set; }
        public bool StoreIpAddresses { get; set; }
        public bool SuffixDeletedUsers { get; set; }
        public bool EnteringEmailTwice { get; set; }
        public bool RequireRegistrationForDownloadableProducts { get; set; }
        public bool AllowUsersToCheckGiftCardBalance { get; set; }
        public int DeleteGuestTaskOlderThanMinutes { get; set; }

        #region Form fields
        public bool GenderEnabled { get; set; }
        public bool DateOfBirthEnabled { get; set; }
        public bool DateOfBirthRequired { get; set; }
        public int? DateOfBirthMinimumAge { get; set; }
        public bool CompanyEnabled { get; set; }
        public bool CompanyRequired { get; set; }
        public bool StreetAddressEnabled { get; set; }
        public bool StreetAddressRequired { get; set; }
        public bool StreetAddress2Enabled { get; set; }
        public bool StreetAddress2Required { get; set; }
        public bool ZipPostalCodeEnabled { get; set; }
        public bool ZipPostalCodeRequired { get; set; }
        public bool CityEnabled { get; set; }
        public bool CityRequired { get; set; }
        public bool CountyEnabled { get; set; }
        public bool CountyRequired { get; set; }
        public bool CountryEnabled { get; set; }
        public bool CountryRequired { get; set; }
        public bool StateProvinceEnabled { get; set; }
        public bool StateProvinceRequired { get; set; }
        public bool PhoneEnabled { get; set; }
        public bool PhoneRequired { get; set; }
        public bool FaxEnabled { get; set; }
        public bool FaxRequired { get; set; }
        public bool AcceptPrivacyPolicyEnabled { get; set; }

        #endregion
    }
}
