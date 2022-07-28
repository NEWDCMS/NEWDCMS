using System;
using System.Collections.Generic;
using System.Linq;

namespace DCMS.Services.Plugins
{

    public partial class PluginLoadedAssemblyInfo
    {

        public PluginLoadedAssemblyInfo(string shortName, string assemblyInMemory)
        {
            ShortName = shortName;
            References = new List<(string PluginName, string AssemblyName)>();
            AssemblyFullNameInMemory = assemblyInMemory;
        }


        /// <summary>
        /// Gets the short assembly name
        /// </summary>
        public string ShortName { get; }

        /// <summary>
        /// Gets the full assembly name loaded in memory
        /// </summary>
        public string AssemblyFullNameInMemory { get; }

        /// <summary>
        /// Gets a list of all mentioned plugin-assembly pairs
        /// </summary>
        public List<(string PluginName, string AssemblyName)> References { get; }

        /// <summary>
        /// Gets a list of plugins that conflict with the loaded assembly version
        /// </summary>
        public IList<(string PluginName, string AssemblyName)> Collisions =>
            References.Where(reference => !reference.AssemblyName.Equals(AssemblyFullNameInMemory, StringComparison.CurrentCultureIgnoreCase)).ToList();
    }
}