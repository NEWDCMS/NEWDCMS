using DCMS.Core;
using DCMS.Core.Domain.Products;
using DCMS.Core.Domain.Users;
using DCMS.Core.Html;
using System.Text;
using System.Web;


namespace DCMS.Services.Products
{
    /// <summary>
    /// 格式商品属性
    /// </summary>
    public partial class ProductAttributeFormatter : IProductAttributeFormatter
    {
        private readonly IWorkContext _workContext;
        private readonly IProductAttributeService _productAttributeService;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly IWebHelper _webHelper;


        public ProductAttributeFormatter(IWorkContext workContext,
            IProductAttributeService productAttributeService,
            IProductAttributeParser productAttributeParser,
            IWebHelper webHelper)
        {
            _workContext = workContext;
            _productAttributeService = productAttributeService;
            _productAttributeParser = productAttributeParser;
            _webHelper = webHelper;
        }


        public string FormatAttributes(Product product, string attributes)
        {
            var customer = _workContext.CurrentUser;
            return FormatAttributes(product, attributes, customer);
        }


        public string FormatAttributes(Product product, string attributes,
            User user, string serapator = "<br />", bool htmlEncode = true, bool renderPrices = true,
            bool renderProductAttributes = true, bool renderGiftCardAttributes = true,
            bool allowHyperlinks = true)
        {
            var result = new StringBuilder();

            //attributes
            if (renderProductAttributes)
            {
                var pvaCollection = _productAttributeParser.ParseProductVariantAttributes(attributes);
                for (int i = 0; i < pvaCollection.Count; i++)
                {
                    var pva = pvaCollection[i];
                    var valuesStr = _productAttributeParser.ParseValues(attributes, pva.Id);
                    for (int j = 0; j < valuesStr.Count; j++)
                    {
                        string valueStr = valuesStr[j];
                        string pvaAttribute = string.Empty;
                        if (!pva.ShouldHaveValues())
                        {
                            //no values
                            if (pva.AttributeControlType == AttributeControlType.MultilineTextbox)
                            {
                                //multiline textbox
                                var attributeName = pva.ProductAttribute.Name;
                                //encode (if required)
                                if (htmlEncode)
                                {
                                    attributeName = HttpUtility.HtmlEncode(attributeName);
                                }

                                pvaAttribute = string.Format("{0}: {1}", attributeName, HtmlHelper.FormatText(valueStr, false, true, false, false, false, false));
                                //we never encode multiline textbox input
                            }

                            else
                            {
                                //other attributes (textbox, datepicker)
                                pvaAttribute = string.Format("{0}: {1}", pva.ProductAttribute.Name, valueStr);
                                //encode (if required)
                                if (htmlEncode)
                                {
                                    pvaAttribute = HttpUtility.HtmlEncode(pvaAttribute);
                                }
                            }
                        }
                        else
                        {
                            //attributes with values
                            if (int.TryParse(valueStr, out int pvaId))
                            {
                                var pvaValue = _productAttributeService.GetProductVariantAttributeValueById(0, pvaId);
                                if (pvaValue != null)
                                {
                                    pvaAttribute = string.Format("{0}: {1}", pva.ProductAttribute.Name, pvaValue.Name);
                                    if (renderPrices)
                                    {
                                        //价格计算服务，验证这这里实现....(需要完善)

                                        //decimal taxRate = decimal.Zero;
                                        decimal pvaValuePriceAdjustment = pvaValue.PriceAdjustment;
                                        decimal priceAdjustmentBase = pvaValuePriceAdjustment;
                                        decimal priceAdjustment = priceAdjustmentBase;
                                        if (priceAdjustmentBase > 0)
                                        {
                                            string priceAdjustmentStr = priceAdjustment.ToString();
                                            pvaAttribute += string.Format(" [+{0}]", priceAdjustmentStr);
                                        }
                                        else if (priceAdjustmentBase < decimal.Zero)
                                        {
                                            string priceAdjustmentStr = (-priceAdjustment).ToString();
                                            pvaAttribute += string.Format(" [-{0}]", priceAdjustmentStr);
                                        }
                                    }
                                }
                                //encode (if required)
                                if (htmlEncode)
                                {
                                    pvaAttribute = HttpUtility.HtmlEncode(pvaAttribute);
                                }
                            }
                        }

                        if (!string.IsNullOrEmpty(pvaAttribute))
                        {
                            if (i != 0 || j != 0)
                            {
                                result.Append(serapator);
                            }

                            result.Append(pvaAttribute);
                        }
                    }
                }
            }

            return result.ToString();
        }
    }
}
