using DCMS.Core.Configuration;

namespace DCMS.Core.Domain.Security
{
    /// <summary>
    /// 验证码设置
    /// </summary>
    public class CaptchaSettings : ISettings
    {
        /// <summary>
        /// Is CAPTCHA enabled?
        /// </summary>
        //[Column(TypeName = "BIT(1)")]
        public bool Enabled { get; set; }
        /// <summary>
        /// A value indicating whether CAPTCHA should be displayed on the login page
        /// </summary>
        //[Column(TypeName = "BIT(1)")]
        public bool ShowOnLoginPage { get; set; }
        /// <summary>
        /// A value indicating whether CAPTCHA should be displayed on the registration page
        /// </summary>
        //[Column(TypeName = "BIT(1)")]
        public bool ShowOnRegistrationPage { get; set; }
        /// <summary>
        /// A value indicating whether CAPTCHA should be displayed on the contacts page
        /// </summary>
        //[Column(TypeName = "BIT(1)")]
        public bool ShowOnContactUsPage { get; set; }
        /// <summary>
        /// A value indicating whether CAPTCHA should be displayed on the wishlist page
        /// </summary>
        //[Column(TypeName = "BIT(1)")]
        public bool ShowOnEmailWishlistToFriendPage { get; set; }
        /// <summary>
        /// A value indicating whether CAPTCHA should be displayed on the "email a friend" page
        /// </summary>
        //[Column(TypeName = "BIT(1)")]
        public bool ShowOnEmailProductToFriendPage { get; set; }
        /// <summary>
        /// A value indicating whether CAPTCHA should be displayed on the "comment blog" page
        /// </summary>
        //[Column(TypeName = "BIT(1)")]
        public bool ShowOnBlogCommentPage { get; set; }
        /// <summary>
        /// A value indicating whether CAPTCHA should be displayed on the "comment news" page
        /// </summary>
        //[Column(TypeName = "BIT(1)")]
        public bool ShowOnNewsCommentPage { get; set; }
        /// <summary>
        /// A value indicating whether CAPTCHA should be displayed on the product reviews page
        /// </summary>
        //[Column(TypeName = "BIT(1)")]
        public bool ShowOnProductReviewPage { get; set; }
        /// <summary>
        /// A value indicating whether CAPTCHA should be displayed on the "Apply for vendor account" page
        /// </summary>
        //[Column(TypeName = "BIT(1)")]
        public bool ShowOnApplyVendorPage { get; set; }
        /// <summary>
        /// A value indicating whether CAPTCHA should be displayed on the "forgot password" page
        /// </summary>
        //[Column(TypeName = "BIT(1)")]
        public bool ShowOnForgotPasswordPage { get; set; }
        /// <summary>
        /// reCAPTCHA public key
        /// </summary>
        public string ReCaptchaPublicKey { get; set; }
        /// <summary>
        /// reCAPTCHA private key
        /// </summary>
        public string ReCaptchaPrivateKey { get; set; }
        /// <summary>
        /// reCAPTCHA theme
        /// </summary>
        public string ReCaptchaTheme { get; set; }
        /// <summary>
        /// reCAPTCHA default language
        /// </summary>
        public string ReCaptchaDefaultLanguage { get; set; }
        /// <summary>
        /// A value indicating whether reCAPTCHA language should be set automatically
        /// </summary>
        //[Column(TypeName = "BIT(1)")]
        public bool AutomaticallyChooseLanguage { get; set; }
    }
}