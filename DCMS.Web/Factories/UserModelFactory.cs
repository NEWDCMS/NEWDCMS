using DCMS.Core;
using DCMS.Core.Domain.Common;
using DCMS.Core.Domain.Media;
using DCMS.Core.Domain.Products;
using DCMS.Core.Domain.Security;
using DCMS.Core.Domain.Tax;
using DCMS.Core.Domain.Users;
using DCMS.Services.Common;
using DCMS.Services.Helpers;
using DCMS.Services.Stores;
using DCMS.Services.Users;
using DCMS.ViewModel.Models.Users;
using System;
using System.Collections.Generic;
using System.Linq;


namespace DCMS.Web.Factories
{
    public partial class UserModelFactory : IUserModelFactory
    {
        private readonly AddressSettings _addressSettings;
        private readonly CaptchaSettings _captchaSettings;
        private readonly CommonSettings _commonSettings;
        private readonly UserSettings _userSettings;
        private readonly DateTimeSettings _dateTimeSettings;
        private readonly ExternalAuthenticationSettings _externalAuthenticationSettings;
        private readonly IUserAttributeParser _userAttributeParser;
        private readonly IUserAttributeService _userAttributeService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IGenericAttributeService _genericAttributeService;
        //private readonly IPictureService _pictureService;
        private readonly IStoreContext _storeContext;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IWorkContext _workContext;
        private readonly MediaSettings _mediaSettings;
        private readonly SecuritySettings _securitySettings;
        private readonly TaxSettings _taxSettings;

        public UserModelFactory(AddressSettings addressSettings,
            CaptchaSettings captchaSettings,
            CommonSettings commonSettings,
            UserSettings userSettings,
            DateTimeSettings dateTimeSettings,
            ExternalAuthenticationSettings externalAuthenticationSettings,

            IUserAttributeParser userAttributeParser,
            IUserAttributeService userAttributeService,
            IDateTimeHelper dateTimeHelper,
            IGenericAttributeService genericAttributeService,
            //IPictureService pictureService,
            IStoreContext storeContext,
            IStoreMappingService storeMappingService,
            IWorkContext workContext,
            MediaSettings mediaSettings,

            SecuritySettings securitySettings,
            TaxSettings taxSettings)
        {
            _addressSettings = addressSettings;
            _captchaSettings = captchaSettings;

            _commonSettings = commonSettings;
            _userSettings = userSettings;
            _dateTimeSettings = dateTimeSettings;
            _externalAuthenticationSettings = externalAuthenticationSettings;
            _userAttributeParser = userAttributeParser;
            _userAttributeService = userAttributeService;
            _dateTimeHelper = dateTimeHelper;
            _genericAttributeService = genericAttributeService;
            //  _pictureService = pictureService;
            _storeContext = storeContext;
            _storeMappingService = storeMappingService;
            _workContext = workContext;
            _mediaSettings = mediaSettings;
            _securitySettings = securitySettings;
            _taxSettings = taxSettings;
        }

        public virtual IList<UserAttributeModel> PrepareCustomUserAttributes(User user, string overrideAttributesXml = "")
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            var result = new List<UserAttributeModel>();

            var userAttributes = _userAttributeService.GetAllUserAttributes();
            foreach (var attribute in userAttributes)
            {
                var attributeModel = new UserAttributeModel
                {
                    Id = attribute.Id,
                    Name = attribute.Name,
                    IsRequired = attribute.IsRequired,
                    AttributeControlType = attribute.AttributeControlType,
                };

                if (attribute.ShouldHaveValues())
                {
                    //values
                    var attributeValues = _userAttributeService.GetUserAttributeValues(attribute.Id);
                    foreach (var attributeValue in attributeValues)
                    {
                        var valueModel = new UserAttributeValueModel
                        {
                            Id = attributeValue.Id,
                            Name = attributeValue.Name,
                            IsPreSelected = attributeValue.IsPreSelected
                        };
                        attributeModel.Values.Add(valueModel);
                    }
                }

                //set already selected attributes
                var selectedAttributesXml = !string.IsNullOrEmpty(overrideAttributesXml) ?
                    overrideAttributesXml :
                    _genericAttributeService.GetAttribute<string>(user, DCMSDefaults.CustomUserAttributes);
                switch (attribute.AttributeControlType)
                {
                    case AttributeControlType.DropdownList:
                    case AttributeControlType.RadioList:
                    case AttributeControlType.Checkboxes:
                        {
                            if (!string.IsNullOrEmpty(selectedAttributesXml))
                            {
                                //clear default selection
                                foreach (var item in attributeModel.Values)
                                {
                                    item.IsPreSelected = false;
                                }

                                //select new values
                                var selectedValues = _userAttributeParser.ParseUserAttributeValues(selectedAttributesXml);
                                foreach (var attributeValue in selectedValues)
                                {
                                    foreach (var item in attributeModel.Values)
                                    {
                                        if (attributeValue.Id == item.Id)
                                        {
                                            item.IsPreSelected = true;
                                        }
                                    }
                                }
                            }
                        }
                        break;
                    case AttributeControlType.ReadonlyCheckboxes:
                        {
                            //do nothing
                            //values are already pre-set
                        }
                        break;
                    case AttributeControlType.TextBox:
                    case AttributeControlType.MultilineTextbox:
                        {
                            if (!string.IsNullOrEmpty(selectedAttributesXml))
                            {
                                var enteredText = _userAttributeParser.ParseValues(selectedAttributesXml, attribute.Id);
                                if (enteredText.Any())
                                {
                                    attributeModel.DefaultValue = enteredText[0];
                                }
                            }
                        }
                        break;
                    case AttributeControlType.ColorSquares:
                    case AttributeControlType.ImageSquares:
                    case AttributeControlType.Datepicker:
                    case AttributeControlType.FileUpload:
                    default:
                        //not supported attribute control types
                        break;
                }

                result.Add(attributeModel);
            }

            return result;
        }


        public virtual UserModel PrepareUserInfoModel(UserModel model, User user,
            bool excludeProperties, string overrideCustomUserAttributesXml = "")
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (!excludeProperties)
            {
                model.UserRealName = _genericAttributeService.GetAttribute<string>(user, DCMSDefaults.RealNameAttribute);
                model.Gender = _genericAttributeService.GetAttribute<string>(user, DCMSDefaults.GenderAttribute);
                var dateOfBirth = _genericAttributeService.GetAttribute<DateTime?>(user, DCMSDefaults.DateOfBirthAttribute);
                model.Email = user.Email;
                model.Username = user.Username;
            }
            else
            {
                if (_userSettings.UsernamesEnabled && !_userSettings.AllowUsersToChangeUsernames)
                {
                    model.Username = user.Username;
                }
            }

            //if (_userSettings.UserRegistrationType == UserRegistrationType.EmailValidation)
            //    model.EmailToRevalidate = user.EmailToRevalidate;


            model.GenderEnabled = _userSettings.GenderEnabled;
            model.DateOfBirthEnabled = _userSettings.DateOfBirthEnabled;

            model.StreetAddressEnabled = _userSettings.StreetAddressEnabled;

            model.CityEnabled = _userSettings.CityEnabled;

            model.CountryEnabled = _userSettings.CountryEnabled;

            model.StateProvinceEnabled = _userSettings.StateProvinceEnabled;

            model.PhoneEnabled = _userSettings.PhoneEnabled;
            model.UsernamesEnabled = _userSettings.UsernamesEnabled;
            model.AllowUsersToChangeUsernames = _userSettings.AllowUsersToChangeUsernames;

            //external authentication


            return model;
        }


        public virtual LoginModel PrepareLoginModel(bool? checkoutAsGuest)
        {
            var model = new LoginModel
            {
                //UsernamesEnabled = _userSettings.UsernamesEnabled,
                //是否开启验证码
                DisplayCaptcha = _captchaSettings.Enabled && _captchaSettings.ShowOnLoginPage
            };
            return model;
        }

        public virtual PasswordRecoveryModel PreparePasswordRecoveryModel()
        {
            var model = new PasswordRecoveryModel
            {
                DisplayCaptcha = _captchaSettings.Enabled && _captchaSettings.ShowOnForgotPasswordPage
            };
            return model;
        }


        public virtual PasswordRecoveryConfirmModel PreparePasswordRecoveryConfirmModel()
        {
            var model = new PasswordRecoveryConfirmModel();
            return model;
        }


        public virtual UserAvatarModel PrepareUserAvatarModel(UserAvatarModel model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            //model.AvatarUrl = _pictureService.GetPictureUrl(
            //    _genericAttributeService.GetAttribute<int>(_workContext.CurrentUser, DCMSDefaults.AvatarPictureIdAttribute),
            //    _mediaSettings.AvatarPictureSize,
            //    false);

            return model;
        }

    }
}