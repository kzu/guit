using System;
using System.Composition;
using System.Globalization;
using System.Resources;
using DotNetGit.Properties;
using Terminal.Gui;

namespace DotNetGit
{
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class MenuCommandAttribute : ExportAttribute
    {
        public MenuCommandAttribute(string displayNameResource, Key hotKey, double order) : base(typeof(IMenuCommand))
        {
            var resourceManager = new ResourceManager(typeof(Resources));
            try
            {
                DisplayName = resourceManager.GetString(displayNameResource, CultureInfo.CurrentUICulture);
            }
            catch (MissingManifestResourceException)
            {
                DisplayName = displayNameResource;
            }

            HotKey = hotKey;
            Order = order;
        }

        public string DisplayName { get; private set; }

        public Key HotKey { get; private set; }

        public double Order { get; private set; }
    }
}