using DCMS.Core.Domain.Products;
using DCMS.Core.Html;
using System.Net;
using System.Text;


namespace DCMS.Services.Users
{

    public partial class UserAttributeFormatter : IUserAttributeFormatter
    {
        #region Fields

        private readonly IUserAttributeParser _userAttributeParser;
        private readonly IUserAttributeService _userAttributeService;

        #endregion

        #region Ctor

        public UserAttributeFormatter(IUserAttributeParser userAttributeParser,
            IUserAttributeService userAttributeService
)
        {
            _userAttributeParser = userAttributeParser;
            _userAttributeService = userAttributeService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// ∏Ò Ω Ù–‘
        /// </summary>
        /// <param name="attributesXml"></param>
        /// <param name="separator"></param>
        /// <param name="htmlEncode"></param>
        /// <returns></returns>
        public virtual string FormatAttributes(string attributesXml, string separator = "<br />", bool htmlEncode = true)
        {
            var result = new StringBuilder();

            var attributes = _userAttributeParser.ParseUserAttributes(attributesXml);
            for (var i = 0; i < attributes.Count; i++)
            {
                var attribute = attributes[i];
                var valuesStr = _userAttributeParser.ParseValues(attributesXml, attribute.Id);
                for (var j = 0; j < valuesStr.Count; j++)
                {
                    var valueStr = valuesStr[j];
                    var formattedAttribute = string.Empty;
                    if (!attribute.ShouldHaveValues())
                    {
                        //no values
                        if (attribute.AttributeControlType == AttributeControlType.MultilineTextbox)
                        {
                            //multiline textbox
                            var attributeName = attribute.Name;
                            //encode (if required)
                            if (htmlEncode)
                            {
                                attributeName = WebUtility.HtmlEncode(attributeName);
                            }

                            formattedAttribute = $"{attributeName}: {HtmlHelper.FormatText(valueStr, false, true, false, false, false, false)}";
                            //we never encode multiline textbox input
                        }
                        else if (attribute.AttributeControlType == AttributeControlType.FileUpload)
                        {
                            //file upload
                            //not supported for user attributes
                        }
                        else
                        {
                            //other attributes (textbox, datepicker)
                            formattedAttribute = $"{attribute.Name}: {valueStr}";
                            //encode (if required)
                            if (htmlEncode)
                            {
                                formattedAttribute = WebUtility.HtmlEncode(formattedAttribute);
                            }
                        }
                    }
                    else
                    {
                        if (int.TryParse(valueStr, out var attributeValueId))
                        {
                            var attributeValue = _userAttributeService.GetUserAttributeValueById(attributeValueId);
                            if (attributeValue != null)
                            {
                                formattedAttribute = $"{attribute.Name}: {attributeValue}";
                            }
                            //encode (if required)
                            if (htmlEncode)
                            {
                                formattedAttribute = WebUtility.HtmlEncode(formattedAttribute);
                            }
                        }
                    }

                    if (string.IsNullOrEmpty(formattedAttribute))
                    {
                        continue;
                    }

                    if (i != 0 || j != 0)
                    {
                        result.Append(separator);
                    }

                    result.Append(formattedAttribute);
                }
            }

            return result.ToString();
        }

        #endregion
    }
}