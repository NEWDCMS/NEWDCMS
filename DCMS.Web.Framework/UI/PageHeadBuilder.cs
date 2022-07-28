using BundlerMinifier;
using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Domain.Common;
using DCMS.Core.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace DCMS.Web.Framework.UI
{
    /// <summary>
    /// 页头构建器
    /// </summary>
    public partial class PageHeadBuilder : IPageHeadBuilder
    {
        #region Fields

        private static readonly object _lock = new object();

        private readonly BundleFileProcessor _processor;
        private readonly CommonSettings _commonSettings;
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IDCMSFileProvider _fileProvider;
        private readonly IStaticCacheManager _cacheManager;
        private readonly IUrlHelperFactory _urlHelperFactory;
        //private readonly IUrlRecordService _urlRecordService;

        private readonly List<string> _titleParts;
        private readonly List<string> _metaDescriptionParts;
        private readonly List<string> _metaKeywordParts;
        private readonly Dictionary<ResourceLocation, List<ScriptReferenceMeta>> _scriptParts;
        private readonly Dictionary<ResourceLocation, List<string>> _inlineScriptParts;
        private readonly Dictionary<ResourceLocation, List<CssReferenceMeta>> _cssParts;
        private readonly List<string> _canonicalUrlParts;
        private readonly List<string> _headCustomParts;
        private readonly List<string> _pageCssClassParts;
        private string _activeAdminMenuSystemName;
        private string _editPageUrl;

        //in minutes
        private const int RECHECK_BUNDLED_FILES_PERIOD = 120;

        #endregion

        #region Ctor

        public PageHeadBuilder(
            CommonSettings commonSettings,
            IActionContextAccessor actionContextAccessor,
            IWebHostEnvironment webHostEnvironment,
            IDCMSFileProvider fileProvider,
            IStaticCacheManager cacheManager,
            IUrlHelperFactory urlHelperFactory
            )
        {
            _processor = new BundleFileProcessor();
            _commonSettings = commonSettings;
            _actionContextAccessor = actionContextAccessor;
            _webHostEnvironment = webHostEnvironment;
            _fileProvider = fileProvider;
            _cacheManager = cacheManager;
            _urlHelperFactory = urlHelperFactory;

            _titleParts = new List<string>();
            _metaDescriptionParts = new List<string>();
            _metaKeywordParts = new List<string>();
            _scriptParts = new Dictionary<ResourceLocation, List<ScriptReferenceMeta>>();
            _inlineScriptParts = new Dictionary<ResourceLocation, List<string>>();
            _cssParts = new Dictionary<ResourceLocation, List<CssReferenceMeta>>();
            _canonicalUrlParts = new List<string>();
            _headCustomParts = new List<string>();
            _pageCssClassParts = new List<string>();
        }

        #endregion

        #region Utilities

        /// <summary>
        /// 获取捆绑文件名
        /// </summary>
        /// <param name="parts">Parts to bundle</param>
        /// <returns>File name</returns>
        protected virtual string GetBundleFileName(string[] parts)
        {
            if (parts == null || parts.Length == 0)
            {
                throw new ArgumentException("parts");
            }

            //calculate hash
            var hash = "";
            using (SHA256 sha = new SHA256Managed())
            {
                // string concatenation
                var hashInput = "";
                foreach (var part in parts)
                {
                    hashInput += part;
                    hashInput += ",";
                }

                var input = sha.ComputeHash(Encoding.Unicode.GetBytes(hashInput));
                hash = WebEncoders.Base64UrlEncode(input);
            }
            //ensure only valid chars
            //hash = _urlRecordService.GetSeName(hash, _seoSettings.ConvertNonWesternChars, _seoSettings.AllowUnicodeCharsInUrls);

            return hash;
        }

        #endregion

        #region Methods


        public virtual void AddTitleParts(string part)
        {
            if (string.IsNullOrEmpty(part))
            {
                return;
            }

            _titleParts.Add(part);
        }

        public virtual void AppendTitleParts(string part)
        {
            if (string.IsNullOrEmpty(part))
            {
                return;
            }

            _titleParts.Insert(0, part);
        }

        public virtual string GenerateTitle(bool addDefaultTitle)
        {
            var result = "";
            result = string.Join("_", _titleParts.AsEnumerable().Reverse().ToArray());
            return result;
        }


        public virtual void AddMetaDescriptionParts(string part)
        {
            if (string.IsNullOrEmpty(part))
            {
                return;
            }

            _metaDescriptionParts.Add(part);
        }

        public virtual void AppendMetaDescriptionParts(string part)
        {
            if (string.IsNullOrEmpty(part))
            {
                return;
            }

            _metaDescriptionParts.Insert(0, part);
        }

        public virtual string GenerateMetaDescription()
        {
            var metaDescription = string.Join(", ", _metaDescriptionParts.AsEnumerable().Reverse().ToArray());
            var result = !string.IsNullOrEmpty(metaDescription) ? metaDescription : "DCMS";
            return result;
        }


        public virtual void AddMetaKeywordParts(string part)
        {
            if (string.IsNullOrEmpty(part))
            {
                return;
            }

            _metaKeywordParts.Add(part);
        }

        public virtual void AppendMetaKeywordParts(string part)
        {
            if (string.IsNullOrEmpty(part))
            {
                return;
            }

            _metaKeywordParts.Insert(0, part);
        }

        public virtual string GenerateMetaKeywords()
        {
            var metaKeyword = string.Join(", ", _metaKeywordParts.AsEnumerable().Reverse().ToArray());
            var result = !string.IsNullOrEmpty(metaKeyword) ? metaKeyword : "DCMS";
            return result;
        }

        public virtual void AddScriptParts(ResourceLocation location, string src, string debugSrc, bool excludeFromBundle, bool isAsync, int order = 0)
        {
            if (!_scriptParts.ContainsKey(location))
            {
                _scriptParts.Add(location, new List<ScriptReferenceMeta>());
            }

            if (string.IsNullOrEmpty(src))
            {
                return;
            }

            if (string.IsNullOrEmpty(debugSrc))
            {
                debugSrc = src;
            }

            _scriptParts[location].Add(new ScriptReferenceMeta
            {
                ExcludeFromBundle = excludeFromBundle,
                IsAsync = isAsync,
                Src = src,
                Order = order,
                DebugSrc = debugSrc
            });
        }

        public virtual void AppendScriptParts(ResourceLocation location, string src, string debugSrc, bool excludeFromBundle, bool isAsync, int order = 0)
        {
            if (!_scriptParts.ContainsKey(location))
            {
                _scriptParts.Add(location, new List<ScriptReferenceMeta>());
            }

            if (string.IsNullOrEmpty(src))
            {
                return;
            }

            if (string.IsNullOrEmpty(debugSrc))
            {
                debugSrc = src;
            }

            _scriptParts[location].Insert(0, new ScriptReferenceMeta
            {
                ExcludeFromBundle = excludeFromBundle,
                IsAsync = isAsync,
                Src = src,
                Order = order,
                DebugSrc = debugSrc
            });
        }

        public virtual string GenerateScripts(ResourceLocation location, bool? bundleFiles = null)
        {
            if (!_scriptParts.ContainsKey(location) || _scriptParts[location] == null)
            {
                return "";
            }

            if (!_scriptParts.Any())
            {
                return "";
            }

            var server = EngineContext.GetStaticResourceServer;

            var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);

            var debugModel = _webHostEnvironment.IsDevelopment();

            if (!bundleFiles.HasValue)
            {
                //use setting if no value is specified
                bundleFiles = _commonSettings.EnableJsBundling;
            }

            if (bundleFiles.Value)
            {
                var partsToBundle = _scriptParts[location]
                    .Where(x => !x.ExcludeFromBundle)
                    .OrderByDescending(x => x.Order)
                    .Distinct()
                    .ToArray();

                var partsToDontBundle = _scriptParts[location]
                    .Where(x => x.ExcludeFromBundle)
                     .OrderByDescending(x => x.Order)
                    .Distinct()
                    .ToArray();

                var result = new StringBuilder();

                //parts to  bundle
                if (partsToBundle.Any())
                {
                    //ensure \bundles directory exists
                    _fileProvider.CreateDirectory(_fileProvider.GetAbsolutePath("bundles"));

                    var bundle = new Bundle();
                    foreach (var item in partsToBundle)
                    {
                        new PathString(urlHelper.Content(debugModel ? item.DebugSrc : item.Src))
                            .StartsWithSegments(urlHelper.ActionContext.HttpContext.Request.PathBase, out PathString path);
                        var src = path.Value.TrimStart('/');

                        //check whether this file exists, if not it should be stored into /wwwroot directory
                        if (!_fileProvider.FileExists(_fileProvider.MapPath(path)))
                        {
                            src = $"wwwroot/{src}";
                        }

                        bundle.InputFiles.Add(src);
                    }

                    //output file
                    var outputFileName = GetBundleFileName(partsToBundle.Select(x => debugModel ? x.DebugSrc : x.Src).ToArray());
                    bundle.OutputFileName = "wwwroot/bundles/" + outputFileName + ".js";
                    //save
                    var configFilePath = _webHostEnvironment.ContentRootPath + "\\" + outputFileName + ".json";
                    bundle.FileName = configFilePath;

                    //performance optimization. do not bundle and minify for each HTTP request
                    //we periodically re-check already bundles file
                    //so if we have minification enabled, it could take up to several minutes to see changes in updated resource files (or just reset the cache or restart the site)
                    var cacheKey = new CacheKey($"DCMS.minification.shouldrebuild.js-{outputFileName}")
                    {
                        CacheTime = RECHECK_BUNDLED_FILES_PERIOD
                    };
                    var shouldRebuild = _cacheManager.Get(cacheKey, () => true);

                    if (shouldRebuild)
                    {
                        lock (_lock)
                        {
                            //store json file to see a generated config file(for debugging purposes)
                            BundleHandler.AddBundle(configFilePath, bundle);

                            //process
                            _processor.Process(configFilePath, new List<Bundle> { bundle });
                        }

                        _cacheManager.Set(cacheKey, false);
                    }

                    //render
                    result.AppendFormat("<script src=\"{0}\"></script>", urlHelper.Content("~/bundles/" + outputFileName + ".min.js"));
                    result.Append(Environment.NewLine);
                }

                //parts to not bundle
                foreach (var item in partsToDontBundle)
                {
                    var src = debugModel ? item.DebugSrc : item.Src;

                    if (string.IsNullOrEmpty(server))
                    {
                        result.AppendFormat("<script {1}src=\"{0}\"></script>", urlHelper.Content(src), item.IsAsync ? "async " : "");
                    }
                    else
                    {
                        result.AppendFormat("<script {1}src=\"{2}{0}{3}\"></script>", urlHelper.Content(src), item.IsAsync ? "async " : "", server, $"?v={System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString()}");
                    }

                    result.Append(Environment.NewLine);
                }

                return result.ToString();
            }
            else
            {

                var scriptParts = _scriptParts[location]
                   .Where(x => !x.ExcludeFromBundle)
                   .OrderByDescending(x => x.Order)
                   .Distinct()
                   .ToArray();

                var result = new StringBuilder();
                foreach (var item in scriptParts)
                {
                    var src = debugModel ? item.DebugSrc : item.Src;
                    if (string.IsNullOrEmpty(server))
                    {
                        result.AppendFormat("<script {1}src=\"{0}\"></script>", urlHelper.Content(src), item.IsAsync ? "async " : "");
                    }
                    else
                    {
                        result.AppendFormat("<script {1}src=\"{2}{0}{3}\"></script>", urlHelper.Content(src), item.IsAsync ? "async " : "", server, $"?v={System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString()}");
                    }

                    result.Append(Environment.NewLine);
                }
                return result.ToString();
            }
        }


        public virtual void AddInlineScriptParts(ResourceLocation location, string script, int order = 0)
        {
            if (!_inlineScriptParts.ContainsKey(location))
            {
                _inlineScriptParts.Add(location, new List<string>());
            }

            if (string.IsNullOrEmpty(script))
            {
                return;
            }

            _inlineScriptParts[location].Add(script);
        }

        public virtual void AppendInlineScriptParts(ResourceLocation location, string script, int order = 0)
        {
            if (!_inlineScriptParts.ContainsKey(location))
            {
                _inlineScriptParts.Add(location, new List<string>());
            }

            if (string.IsNullOrEmpty(script))
            {
                return;
            }

            _inlineScriptParts[location].Insert(0, script);
        }

        public virtual string GenerateInlineScripts(ResourceLocation location)
        {
            if (!_inlineScriptParts.ContainsKey(location) || _inlineScriptParts[location] == null)
            {
                return "";
            }

            if (!_inlineScriptParts.Any())
            {
                return "";
            }

            var result = new StringBuilder();
            foreach (var item in _inlineScriptParts[location])
            {
                result.Append(item);
                result.Append(Environment.NewLine);
            }
            return result.ToString();
        }


        public virtual void AddCssFileParts(ResourceLocation location, string src, string debugSrc, bool excludeFromBundle = false)
        {
            if (!_cssParts.ContainsKey(location))
            {
                _cssParts.Add(location, new List<CssReferenceMeta>());
            }

            if (string.IsNullOrEmpty(src))
            {
                return;
            }

            if (string.IsNullOrEmpty(debugSrc))
            {
                debugSrc = src;
            }

            _cssParts[location].Add(new CssReferenceMeta
            {
                ExcludeFromBundle = excludeFromBundle,
                Src = src,
                DebugSrc = debugSrc
            });
        }

        public virtual void AppendCssFileParts(ResourceLocation location, string src, string debugSrc, bool excludeFromBundle = false)
        {
            if (!_cssParts.ContainsKey(location))
            {
                _cssParts.Add(location, new List<CssReferenceMeta>());
            }

            if (string.IsNullOrEmpty(src))
            {
                return;
            }

            if (string.IsNullOrEmpty(debugSrc))
            {
                debugSrc = src;
            }

            _cssParts[location].Insert(0, new CssReferenceMeta
            {
                ExcludeFromBundle = excludeFromBundle,
                Src = src,
                DebugSrc = debugSrc
            });
        }

        public virtual string GenerateCssFiles(ResourceLocation location, bool? bundleFiles = null)
        {
            if (!_cssParts.ContainsKey(location) || _cssParts[location] == null)
            {
                return "";
            }

            if (!_cssParts.Any())
            {
                return "";
            }

            var server = EngineContext.GetStaticResourceServer;

            var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);

            var debugModel = _webHostEnvironment.IsDevelopment();

            if (!bundleFiles.HasValue)
            {
                //use setting if no value is specified
                bundleFiles = _commonSettings.EnableCssBundling;
            }

            //CSS bundling is not allowed in virtual directories
            if (urlHelper.ActionContext.HttpContext.Request.PathBase.HasValue)
            {
                bundleFiles = false;
            }

            if (bundleFiles.Value)
            {
                var partsToBundle = _cssParts[location]
                    .Where(x => !x.ExcludeFromBundle)
                    .Distinct()
                    .ToArray();
                var partsToDontBundle = _cssParts[location]
                    .Where(x => x.ExcludeFromBundle)
                    .Distinct()
                    .ToArray();

                var result = new StringBuilder();


                //parts to  bundle
                if (partsToBundle.Any())
                {
                    //ensure \bundles directory exists
                    _fileProvider.CreateDirectory(_fileProvider.GetAbsolutePath("bundles"));

                    var bundle = new Bundle();
                    foreach (var item in partsToBundle)
                    {
                        var src = debugModel ? item.DebugSrc : item.Src;
                        src = urlHelper.Content(src);
                        //check whether this file exists 
                        var srcPath = _fileProvider.Combine(_webHostEnvironment.ContentRootPath, src.Remove(0, 1).Replace("/", "\\"));
                        if (_fileProvider.FileExists(srcPath))
                        {
                            //remove starting /
                            src = src.Remove(0, 1);
                        }
                        else
                        {
                            //if not, it should be stored into /wwwroot directory
                            src = "wwwroot/" + src;
                        }
                        bundle.InputFiles.Add(src);
                    }
                    //output file
                    var outputFileName = GetBundleFileName(partsToBundle.Select(x => { return debugModel ? x.DebugSrc : x.Src; }).ToArray());
                    bundle.OutputFileName = "wwwroot/bundles/" + outputFileName + ".css";
                    //save
                    var configFilePath = _webHostEnvironment.ContentRootPath + "\\" + outputFileName + ".json";
                    bundle.FileName = configFilePath;

                    var cacheKey = new CacheKey($"DCMS.minification.shouldrebuild.CSS-{outputFileName}")
                    {
                        CacheTime = RECHECK_BUNDLED_FILES_PERIOD
                    };
                    var shouldRebuild = _cacheManager.Get(cacheKey, () => true);

                    if (shouldRebuild)
                    {
                        lock (_lock)
                        {
                            //store json file to see a generated config file(for debugging purposes)
                            BundleHandler.AddBundle(configFilePath, bundle);

                            //process
                            _processor.Process(configFilePath, new List<Bundle> { bundle });
                        }

                        _cacheManager.Set(cacheKey, false);
                    }

                    //render
                    result.AppendFormat("<link href=\"{0}\" rel=\"stylesheet\" type=\"{1}\" />", urlHelper.Content("~/bundles/" + outputFileName + ".min.css"), MimeTypes.TextCss);

                    result.Append(Environment.NewLine);
                }

                //parts not to bundle
                foreach (var item in partsToDontBundle)
                {
                    var src = debugModel ? item.DebugSrc : item.Src;


                    if (string.IsNullOrEmpty(server))
                    {
                        result.AppendFormat("<link href=\"{0}\" rel=\"stylesheet\" type=\"{1}\" />", urlHelper.Content(src), MimeTypes.TextCss);
                    }
                    else
                    {
                        result.AppendFormat("<link href=\"{2}{0}{3}\" rel=\"stylesheet\" type=\"{1}\" />", urlHelper.Content(src), MimeTypes.TextCss, server, $"?v={System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString()}");
                    }

                    result.Append(Environment.NewLine);
                }

                return result.ToString();
            }
            else
            {
                //bundling is disabled
                var result = new StringBuilder();
                foreach (var item in _cssParts[location].Distinct())
                {
                    var src = debugModel ? item.DebugSrc : item.Src;
                    if (string.IsNullOrEmpty(server))
                    {
                        result.AppendFormat("<link href=\"{0}\" rel=\"stylesheet\" type=\"{1}\" />", urlHelper.Content(src), MimeTypes.TextCss);
                    }
                    else
                    {
                        result.AppendFormat("<link href=\"{2}{0}{3}\" rel=\"stylesheet\" type=\"{1}\" />", urlHelper.Content(src), MimeTypes.TextCss, server, $"?v={System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString()}");
                    }

                    result.AppendLine();
                }
                return result.ToString();
            }
        }


        public virtual void AddCanonicalUrlParts(string part)
        {
            if (string.IsNullOrEmpty(part))
            {
                return;
            }

            _canonicalUrlParts.Add(part);
        }

        public virtual void AppendCanonicalUrlParts(string part)
        {
            if (string.IsNullOrEmpty(part))
            {
                return;
            }

            _canonicalUrlParts.Insert(0, part);
        }

        public virtual string GenerateCanonicalUrls()
        {
            var result = new StringBuilder();
            foreach (var canonicalUrl in _canonicalUrlParts)
            {
                result.AppendFormat("<link rel=\"canonical\" href=\"{0}\" />", canonicalUrl);
                result.Append(Environment.NewLine);
            }
            return result.ToString();
        }


        public virtual void AddHeadCustomParts(string part)
        {
            if (string.IsNullOrEmpty(part))
            {
                return;
            }

            _headCustomParts.Add(part);
        }

        public virtual void AppendHeadCustomParts(string part)
        {
            if (string.IsNullOrEmpty(part))
            {
                return;
            }

            _headCustomParts.Insert(0, part);
        }

        public virtual string GenerateHeadCustom()
        {
            //use only distinct rows
            var distinctParts = _headCustomParts.Distinct().ToList();
            if (!distinctParts.Any())
            {
                return "";
            }

            var result = new StringBuilder();
            foreach (var path in distinctParts)
            {
                result.Append(path);
                result.Append(Environment.NewLine);
            }
            return result.ToString();
        }


        public virtual void AddPageCssClassParts(string part)
        {
            if (string.IsNullOrEmpty(part))
            {
                return;
            }

            _pageCssClassParts.Add(part);
        }

        public virtual void AppendPageCssClassParts(string part)
        {
            if (string.IsNullOrEmpty(part))
            {
                return;
            }

            _pageCssClassParts.Insert(0, part);
        }

        public virtual string GeneratePageCssClasses()
        {
            var result = string.Join(" ", _pageCssClassParts.AsEnumerable().Reverse().ToArray());
            return result;
        }


        public virtual void AddEditPageUrl(string url)
        {
            _editPageUrl = url;
        }

        public virtual string GetEditPageUrl()
        {
            return _editPageUrl;
        }

        public virtual void SetActiveMenuItemSystemName(string systemName)
        {
            _activeAdminMenuSystemName = systemName;
        }

        public virtual string GetActiveMenuItemSystemName()
        {
            return _activeAdminMenuSystemName;
        }

        #endregion

        #region Nested classes


        private class ScriptReferenceMeta : IEquatable<ScriptReferenceMeta>
        {
            public bool ExcludeFromBundle { get; set; }
            public bool IsAsync { get; set; }
            public string Src { get; set; }

            public int Order { get; set; } = 0;
            public string DebugSrc { get; set; }

            public bool Equals(ScriptReferenceMeta item)
            {
                if (item == null)
                {
                    return false;
                }

                return Src.Equals(item.Src) && DebugSrc.Equals(item.DebugSrc);
            }
            public override int GetHashCode()
            {
                return Src == null ? 0 : Src.GetHashCode();
            }
        }


        private class CssReferenceMeta : IEquatable<CssReferenceMeta>
        {
            public bool ExcludeFromBundle { get; set; }

            public string Src { get; set; }
            public string DebugSrc { get; set; }
            public int Order { get; set; } = 0;
            public bool Equals(CssReferenceMeta item)
            {
                if (item == null)
                {
                    return false;
                }

                return Src.Equals(item.Src) && DebugSrc.Equals(item.DebugSrc);
            }
            public override int GetHashCode()
            {
                return Src == null ? 0 : Src.GetHashCode();
            }
        }

        #endregion
    }
}