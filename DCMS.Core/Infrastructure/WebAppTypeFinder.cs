using System;
using System.Collections.Generic;
using System.Reflection;

namespace DCMS.Core.Infrastructure
{

    public class WebAppTypeFinder : AppDomainTypeFinder
    {
        #region Fields

        private bool _binFolderAssembliesLoaded;

        #endregion

        #region Ctor

        public WebAppTypeFinder(IDCMSFileProvider fileProvider = null) : base(fileProvider)
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets whether assemblies in the bin folder of the web application should be specifically checked for being loaded on application load. This is need in situations where plugins need to be loaded in the AppDomain after the application been reloaded.
        /// </summary>
        public bool EnsureBinFolderAssembliesLoaded { get; set; } = true;

        #endregion

        #region Methods

        /// <summary>
        /// Gets a physical disk path of \Bin directory
        /// </summary>
        /// <returns>The physical path. E.g. "c:\inetpub\wwwroot\bin"</returns>
        public virtual string GetBinDirectory()
        {
            return AppContext.BaseDirectory;
        }

        /// <summary>
        /// Get assemblies
        /// </summary>
        /// <returns>Result</returns>
        public override IList<Assembly> GetAssemblies()
        {
            if (!EnsureBinFolderAssembliesLoaded || _binFolderAssembliesLoaded)
            {
                return base.GetAssemblies();
            }

            _binFolderAssembliesLoaded = true;
            var binPath = GetBinDirectory();
            //binPath = _webHelper.MapPath("~/bin");
            LoadMatchingAssemblies(binPath);

            return base.GetAssemblies();
        }

        #endregion
    }

    //public class WebAppTypeFinder : AppDomainTypeFinder
    //{
    //    #region Fields

    //    private bool _binFolderAssembliesLoaded;

    //    #endregion

    //    #region Ctor

    //    public WebAppTypeFinder(IDCMSFileProvider fileProvider = null) : base(fileProvider)
    //    {
    //    }

    //    #endregion

    //    #region Properties

    //    /// <summary>
    //    /// 获取或设置是否应在应用程序加载时特别检查web应用程序的bin文件夹中的程序集。
    //    /// 在应用程序重新加载后需要在AppDomain中加载插件的情况下需要这样做。
    //    /// </summary>
    //    public bool EnsureBinFolderAssembliesLoaded { get; set; } = true;

    //    #endregion

    //    #region Methods

    //    /// <summary>
    //    /// Gets a physical disk path of \Bin directory
    //    /// </summary>
    //    /// <returns>The physical path. E.g. "c:\inetpub\wwwroot\bin"</returns>
    //    public virtual string GetBinDirectory()
    //    {
    //        return AppContext.BaseDirectory;
    //    }

    //    /// <summary>
    //    /// Get assemblies
    //    /// </summary>
    //    /// <returns>Result</returns>
    //    public override IList<Assembly> GetAssemblies()
    //    {
    //        if (!EnsureBinFolderAssembliesLoaded || _binFolderAssembliesLoaded)
    //            return base.GetAssemblies();

    //        _binFolderAssembliesLoaded = true;
    //        var binPath = GetBinDirectory();
    //        //"E:\\Git\\DCMS.Studio.Core\\DCMS.Core\\DCMS.Web\\bin\\Debug\\netcoreapp3.1\\"
    //        //binPath = _webHelper.MapPath("~/bin");
    //        LoadMatchingAssemblies(binPath);

    //        return base.GetAssemblies();
    //    }

    //    #endregion
    //}
}
