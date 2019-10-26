using System;

namespace Guit
{
    /// <summary>
    /// Used in codegen from targets to inject the known built-in plugins.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    class CorePluginAttribute : Attribute
    {
        public CorePluginAttribute(string assemblyFileName, string isVisible = "true")
        {
            AssemblyFileName = assemblyFileName;
            IsVisible = bool.TryParse(isVisible, out var result) && result;
        }

        public string AssemblyFileName { get; }

        /// <summary>
        /// Whether the plugin should be visible in the plugins view.
        /// </summary>
        public bool IsVisible { get; }
    }
}
