using DCMS.Web.Framework.Mvc.ModelBinding;


namespace DCMS.Web.Framework
{
    public class HintDisplayName : System.ComponentModel.DisplayNameAttribute, IModelAttribute
    {
        private string _resourceValue = string.Empty;


        public HintDisplayName(string resourceKey, string hit)
            : base(resourceKey)
        {
            ResourceKey = resourceKey + "|" + hit;
        }

        public string ResourceKey { get; set; }

        public override string DisplayName
        {
            get
            {
                _resourceValue = ResourceKey;
                return _resourceValue;
            }
        }

        public string Name
        {
            get { return "HintDisplayName"; }
        }
    }
}
