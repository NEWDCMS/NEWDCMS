using DCMS.Core.Domain.Products;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;

namespace DCMS.Services.Products
{
    /// <summary>
    /// 商品属性解析
    /// </summary>
    public partial class ProductAttributeParser : IProductAttributeParser
    {
        #region 字段

        private readonly IProductAttributeService _productAttributeService;

        #endregion

        #region 构造

        public ProductAttributeParser(IProductAttributeService productAttributeService)
        {
            _productAttributeService = productAttributeService;
        }

        #endregion

        #region 商品属性

        /// <summary>
        /// 获取变体商品属性标识
        /// </summary>
        /// <param name="attributes">属性</param>
        /// <returns></returns>
        public virtual IList<int> ParseProductVariantAttributeIds(string attributes)
        {
            var ids = new List<int>();
            if (string.IsNullOrEmpty(attributes))
            {
                return ids;
            }

            try
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(attributes);

                var nodeList1 = xmlDoc.SelectNodes(@"//Attributes/ProductVariantAttribute");
                foreach (XmlNode node1 in nodeList1)
                {
                    if (node1.Attributes != null && node1.Attributes["ID"] != null)
                    {
                        string str1 = node1.Attributes["ID"].InnerText.Trim();
                        if (int.TryParse(str1, out int id))
                        {
                            ids.Add(id);
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                Debug.Write(exc.ToString());
            }
            return ids;
        }

        /// <summary>
        /// 获取选择的变体商品属性
        /// </summary>
        /// <param name="attributes">属性</param>
        /// <returns></returns>
        public virtual IList<ProductVariantAttribute> ParseProductVariantAttributes(string attributes)
        {
            var pvaCollection = new List<ProductVariantAttribute>();
            var ids = ParseProductVariantAttributeIds(attributes);
            foreach (int id in ids)
            {
                var pva = _productAttributeService.GetProductVariantAttributeById(0, id);
                if (pva != null)
                {
                    pvaCollection.Add(pva);
                }
            }
            return pvaCollection;
        }

        /// <summary>
        ///  获取变体商品属性值
        /// </summary>
        /// <param name="attributes">Attributes</param>
        /// <returns>Product variant attribute values</returns>
        public virtual IList<ProductVariantAttributeValue> ParseProductVariantAttributeValues(string attributes)
        {
            var pvaValues = new List<ProductVariantAttributeValue>();
            var pvaCollection = ParseProductVariantAttributes(attributes);
            foreach (var pva in pvaCollection)
            {
                if (!pva.ShouldHaveValues())
                {
                    continue;
                }

                var pvaValuesStr = ParseValues(attributes, pva.Id);
                foreach (string pvaValueStr in pvaValuesStr)
                {
                    if (!string.IsNullOrEmpty(pvaValueStr))
                    {
                        if (int.TryParse(pvaValueStr, out int pvaValueId))
                        {
                            var pvaValue = _productAttributeService.GetProductVariantAttributeValueById(0, pvaValueId);
                            if (pvaValue != null)
                            {
                                pvaValues.Add(pvaValue);
                            }
                        }
                    }
                }
            }
            return pvaValues;
        }


        /// <summary>
        /// 获取选择商品属性值
        /// </summary>
        /// <param name="attributes">属性</param>
        /// <param name="productVariantAttributeId">变体属性ID</param>
        /// <returns></returns>
        public virtual IList<string> ParseValues(string attributes, int productVariantAttributeId)
        {
            var selectedProductVariantAttributeValues = new List<string>();
            try
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(attributes);

                var nodeList1 = xmlDoc.SelectNodes(@"//Attributes/ProductVariantAttribute");
                foreach (XmlNode node1 in nodeList1)
                {
                    if (node1.Attributes != null && node1.Attributes["ID"] != null)
                    {
                        string str1 = node1.Attributes["ID"].InnerText.Trim();
                        if (int.TryParse(str1, out int id))
                        {
                            if (id == productVariantAttributeId)
                            {
                                var nodeList2 = node1.SelectNodes(@"ProductVariantAttributeValue/Value");
                                foreach (XmlNode node2 in nodeList2)
                                {
                                    string value = node2.InnerText.Trim();
                                    selectedProductVariantAttributeValues.Add(value);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                Debug.Write(exc.ToString());
            }
            return selectedProductVariantAttributeValues;
        }


        /// <summary>
        /// 添加商品属性
        /// </summary>
        /// <param name="attributes"></param>
        /// <param name="pva"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public virtual string AddProductAttribute(string attributes, ProductVariantAttribute pva, string value)
        {
            string result = string.Empty;
            try
            {
                var xmlDoc = new XmlDocument();
                if (string.IsNullOrEmpty(attributes))
                {
                    var element1 = xmlDoc.CreateElement("Attributes");
                    xmlDoc.AppendChild(element1);
                }
                else
                {
                    xmlDoc.LoadXml(attributes);
                }
                var rootElement = (XmlElement)xmlDoc.SelectSingleNode(@"//Attributes");

                XmlElement pvaElement = null;
                //find existing
                var nodeList1 = xmlDoc.SelectNodes(@"//Attributes/ProductVariantAttribute");
                foreach (XmlNode node1 in nodeList1)
                {
                    if (node1.Attributes != null && node1.Attributes["ID"] != null)
                    {
                        string str1 = node1.Attributes["ID"].InnerText.Trim();
                        if (int.TryParse(str1, out int id))
                        {
                            if (id == pva.Id)
                            {
                                pvaElement = (XmlElement)node1;
                                break;
                            }
                        }
                    }
                }


                if (pvaElement == null)
                {
                    pvaElement = xmlDoc.CreateElement("ProductVariantAttribute");
                    pvaElement.SetAttribute("ID", pva.Id.ToString());
                    rootElement.AppendChild(pvaElement);
                }

                var pvavElement = xmlDoc.CreateElement("ProductVariantAttributeValue");
                pvaElement.AppendChild(pvavElement);

                var pvavVElement = xmlDoc.CreateElement("Value");
                pvavVElement.InnerText = value;
                pvavElement.AppendChild(pvavVElement);

                result = xmlDoc.OuterXml;
            }
            catch (Exception exc)
            {
                Debug.Write(exc.ToString());
            }
            return result;
        }

        /// <summary>
        /// 判断属性是否相等
        /// </summary>
        /// <param name="attributes1">第1个属性</param>
        /// <param name="attributes2">第2个属性</param>
        /// <returns></returns>
        public virtual bool AreProductAttributesEqual(string attributes1, string attributes2)
        {
            bool attributesEqual = true;
            if (ParseProductVariantAttributeIds(attributes1).Count == ParseProductVariantAttributeIds(attributes2).Count)
            {
                var pva1Collection = ParseProductVariantAttributes(attributes1);
                var pva2Collection = ParseProductVariantAttributes(attributes2);
                foreach (var pva1 in pva1Collection)
                {
                    bool hasAttribute = false;
                    foreach (var pva2 in pva2Collection)
                    {
                        if (pva1.Id == pva2.Id)
                        {
                            hasAttribute = true;
                            var pvaValues1Str = ParseValues(attributes1, pva1.Id);
                            var pvaValues2Str = ParseValues(attributes2, pva2.Id);
                            if (pvaValues1Str.Count == pvaValues2Str.Count)
                            {
                                foreach (string str1 in pvaValues1Str)
                                {
                                    bool hasValue = false;
                                    foreach (string str2 in pvaValues2Str)
                                    {
                                        if (str1.Trim().ToLower() == str2.Trim().ToLower())
                                        {
                                            hasValue = true;
                                            break;
                                        }
                                    }

                                    if (!hasValue)
                                    {
                                        attributesEqual = false;
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                attributesEqual = false;
                                break;
                            }
                        }
                    }

                    if (hasAttribute == false)
                    {
                        attributesEqual = false;
                        break;
                    }
                }
            }
            else
            {
                attributesEqual = false;
            }

            return attributesEqual;
        }

        /// <summary>
        /// 查找属性组合
        /// </summary>
        public virtual ProductVariantAttributeCombination FindProductVariantAttributeCombination(Product product,
            string attributesXml)
        {
            if (product == null)
            {
                throw new ArgumentNullException("product");
            }

            //existing combinations
            var combinations = _productAttributeService.GetAllProductVariantAttributeCombinations(product.Id);
            if (combinations.Count == 0)
            {
                return null;
            }

            foreach (var combination in combinations)
            {
                bool attributesEqual = AreProductAttributesEqual(combination.AttributesXml, attributesXml);
                if (attributesEqual)
                {
                    return combination;
                }
            }

            return null;
        }

        /// <summary>
        /// 生成组合
        /// </summary>
        /// <param name="product"></param>
        public virtual IList<string> GenerateAllCombinations(Product product)
        {
            if (product == null)
            {
                throw new ArgumentNullException("product");
            }

            var allProductVariantAttributes = _productAttributeService.GetProductVariantAttributesByProductId(product.StoreId, product.Id);
            var allPossibleAttributeCombinations = new List<List<ProductVariantAttribute>>();
            for (int counter = 0; counter < (1 << allProductVariantAttributes.Count); ++counter)
            {
                var combination = new List<ProductVariantAttribute>();
                for (int i = 0; i < allProductVariantAttributes.Count; ++i)
                {
                    if ((counter & (1 << i)) == 0)
                    {
                        combination.Add(allProductVariantAttributes[i]);
                    }
                }

                allPossibleAttributeCombinations.Add(combination);
            }

            var allAttributesXml = new List<string>();
            foreach (var combination in allPossibleAttributeCombinations)
            {
                var attributesXml = new List<string>();
                foreach (var pva in combination)
                {
                    if (!pva.ShouldHaveValues())
                    {
                        continue;
                    }

                    var pvaValues = _productAttributeService.GetProductVariantAttributeValues(0, pva.Id);
                    if (pvaValues.Count == 0)
                    {
                        continue;
                    }

                    //checkboxes could have several values ticked
                    var allPossibleCheckboxCombinations = new List<List<ProductVariantAttributeValue>>();
                    if (pva.AttributeControlType == AttributeControlType.Checkboxes)
                    {
                        for (int counter = 0; counter < (1 << pvaValues.Count); ++counter)
                        {
                            var checkboxCombination = new List<ProductVariantAttributeValue>();
                            for (int i = 0; i < pvaValues.Count; ++i)
                            {
                                if ((counter & (1 << i)) == 0)
                                {
                                    checkboxCombination.Add(pvaValues[i]);
                                }
                            }

                            allPossibleCheckboxCombinations.Add(checkboxCombination);
                        }
                    }

                    if (attributesXml.Count == 0)
                    {
                        //first set of values
                        if (pva.AttributeControlType == AttributeControlType.Checkboxes)
                        {
                            //checkboxes could have several values ticked
                            foreach (var checkboxCombination in allPossibleCheckboxCombinations)
                            {
                                var tmp1 = "";
                                foreach (var checkboxValue in checkboxCombination)
                                {
                                    tmp1 = AddProductAttribute(tmp1, pva, checkboxValue.Id.ToString());
                                }
                                if (!string.IsNullOrEmpty(tmp1))
                                {
                                    attributesXml.Add(tmp1);
                                }
                            }
                        }
                        else
                        {
                            //other attribute types (dropdownlist, radiobutton, color squares)
                            foreach (var pvaValue in pvaValues)
                            {
                                var tmp1 = AddProductAttribute("", pva, pvaValue.Id.ToString());
                                attributesXml.Add(tmp1);
                            }
                        }
                    }
                    else
                    {
                        //next values. let's "append" them to already generated attribute combinations in XML format
                        var attributesXmlTmp = new List<string>();
                        if (pva.AttributeControlType == AttributeControlType.Checkboxes)
                        {
                            //checkboxes could have several values ticked
                            foreach (var str1 in attributesXml)
                            {
                                foreach (var checkboxCombination in allPossibleCheckboxCombinations)
                                {
                                    var tmp1 = str1;
                                    foreach (var checkboxValue in checkboxCombination)
                                    {
                                        tmp1 = AddProductAttribute(tmp1, pva, checkboxValue.Id.ToString());
                                    }
                                    if (!string.IsNullOrEmpty(tmp1))
                                    {
                                        attributesXmlTmp.Add(tmp1);
                                    }
                                }
                            }
                        }
                        else
                        {
                            //other attribute types (dropdownlist, radiobutton, color squares)
                            foreach (var pvaValue in pvaValues)
                            {
                                foreach (var str1 in attributesXml)
                                {
                                    var tmp1 = AddProductAttribute(str1, pva, pvaValue.Id.ToString());
                                    attributesXmlTmp.Add(tmp1);
                                }
                            }
                        }
                        attributesXml.Clear();
                        attributesXml.AddRange(attributesXmlTmp);
                    }
                }
                allAttributesXml.AddRange(attributesXml);
            }

            return allAttributesXml;
        }

        #endregion


    }
}
