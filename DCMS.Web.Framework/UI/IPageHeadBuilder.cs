namespace DCMS.Web.Framework.UI
{
    /// <summary>
    /// 页头构建器
    /// </summary>
    public partial interface IPageHeadBuilder
    {

        void AddTitleParts(string part);

        void AppendTitleParts(string part);

        string GenerateTitle(bool addDefaultTitle);


        void AddMetaDescriptionParts(string part);

        void AppendMetaDescriptionParts(string part);

        string GenerateMetaDescription();


        void AddMetaKeywordParts(string part);

        void AppendMetaKeywordParts(string part);

        string GenerateMetaKeywords();


        void AddScriptParts(ResourceLocation location, string src, string debugSrc, bool excludeFromBundle, bool isAsync, int order = 0);

        void AppendScriptParts(ResourceLocation location, string src, string debugSrc, bool excludeFromBundle, bool isAsync, int order = 0);

        string GenerateScripts(ResourceLocation location, bool? bundleFiles = null);

        void AddInlineScriptParts(ResourceLocation location, string script, int order = 0);

        void AppendInlineScriptParts(ResourceLocation location, string script, int order = 0);

        string GenerateInlineScripts(ResourceLocation location);


        void AddCssFileParts(ResourceLocation location, string src, string debugSrc, bool excludeFromBundle = false);

        void AppendCssFileParts(ResourceLocation location, string src, string debugSrc, bool excludeFromBundle = false);

        string GenerateCssFiles(ResourceLocation location, bool? bundleFiles = null);

        void AddCanonicalUrlParts(string part);

        void AppendCanonicalUrlParts(string part);

        string GenerateCanonicalUrls();


        void AddHeadCustomParts(string part);

        void AppendHeadCustomParts(string part);

        string GenerateHeadCustom();

        void AddPageCssClassParts(string part);

        void AppendPageCssClassParts(string part);

        string GeneratePageCssClasses();

        void AddEditPageUrl(string url);

        string GetEditPageUrl();

        void SetActiveMenuItemSystemName(string systemName);

        string GetActiveMenuItemSystemName();
    }
}
