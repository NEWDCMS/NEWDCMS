using DCMS.Core.Domain.Products;
using DCMS.Web.Framework.Models;
using System.Collections.Generic;

namespace DCMS.ViewModel.Models.Users
{
    public partial class UserAttributeModel : BaseEntityModel
    {
        public UserAttributeModel()
        {
            Values = new List<UserAttributeValueModel>();
        }

        public string Name { get; set; }

        public bool IsRequired { get; set; }

        /// <summary>
        /// Default value for textboxes
        /// </summary>
        public string DefaultValue { get; set; }

        public AttributeControlType AttributeControlType { get; set; }

        public int AttributeControlTypeId { get; set; }

        public string AttributeControlTypeName { get; set; }

        public IList<UserAttributeValueModel> Values { get; set; }

        public int DisplayOrder { get; set; }
    }

    public partial class UserAttributeValueModel : BaseEntityModel
    {
        public string Name { get; set; }

        public bool IsPreSelected { get; set; }
    }
}