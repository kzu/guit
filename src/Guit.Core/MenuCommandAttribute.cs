using System;
using System.Composition;
using System.Globalization;
using System.Resources;
using Terminal.Gui;

namespace Guit
{
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class MenuCommandAttribute : ExportAttribute
    {
        public MenuCommandAttribute(string id, Key hotKey, string context = null, Type resources = null) :
            this(id, hotKey, (double)hotKey, context, resources)
        { }

        public MenuCommandAttribute(string id, Key hotKey, double order, string context = null, Type resources = null)
            : base(typeof(IMenuCommand))
        {
            if (resources != null)
            {
                var resourceManager = new ResourceManager(resources);
                try
                {
                    DisplayName = resourceManager.GetString(id, CultureInfo.CurrentUICulture) ?? id;
                }
                catch (MissingManifestResourceException)
                {
                    DisplayName = id;
                }
            }
            else
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