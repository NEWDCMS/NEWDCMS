using DCMS.Core;
using DCMS.Core.Infrastructure;
using Microsoft.AspNetCore.Hosting;

namespace DCMS.Web.Helpers
{
    /// <summary>
    /// TinyMCE 编辑器
    /// </summary>
    public static class TinyMceHelper
    {
        public static string GetTinyMceLanguage()
        {
            var workContext = EngineContext.Current.Resolve<IWorkContext>();
            var hostingEnvironment = EngineContext.Current.Resolve<IWebHostEnvironment>();
            var fileProvider = EngineContext.Current.Resolve<IDCMSFileProvider>();

            var langFile = $"zh-CN.js";
            var directoryPath = fileProvider.Combine(hostingEnvironment.WebRootPath, @"lib\tinymce\langs");
            var fileExists = fileProvider.FileExists($"{directoryPath}\\{langFile}");

            if (!fileExists)
            {
                langFile = $"zhch.js";
                fileExists = fileProvider.FileExists($"{directoryPath}\\{langFile}");
            }

            if (!fileExists)
            {
                langFile = $"zhch.js";
                fileExists = fileProvider.FileExists($"{directoryPath}\\{langFile}");
            }

            return fileExists ? "" : string.Empty;
        }
    }
}