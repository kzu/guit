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
        public MenuCommandAttribute(string id, Key hotKey, string context = null) :
            this(id, hotKey, (double)hotKey, context)
        { }

        public MenuCommandAttribute(string id, Key hotKey, double order, string context = null)
            : base(typeof(IMenuCommand))
        {
            var resourceManager = new ResourceManager(typeof(Resources));
            try
            {
                DisplayName = resourceManager.GetString(id, CultureInfo.CurrentUICulture) ?? id;
            }
            catch (MissingManifestResourceException)
            {
                DisplayName = id;
            }

            Context = context;
            HotKey = hotKey;
            Order = order;
        }

        public string DisplayName { get; }

        public Key HotKey { get; }

        public double Order { get; }

        public string Context { get; }
    }
}