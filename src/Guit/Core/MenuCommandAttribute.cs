using System;
using System.Composition;
using System.Globalization;
using System.Resources;
using Guit.Properties;
using Terminal.Gui;

namespace Guit
{
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class MenuCommandAttribute : ExportAttribute
    {
        public MenuCommandAttribute(string displayNameResource, Key hotKey) :
            this(displayNameResource, hotKey, (double)hotKey)
        {
        }

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

        public string DisplayName { get; }

        public Key HotKey { get; }

        public double Order { get; }
    }
}