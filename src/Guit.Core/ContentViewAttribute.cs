using System;
using System.Composition;
using System.Globalization;
using System.Resources;
using Terminal.Gui;

namespace Guit
{
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ContentViewAttribute : ExportAttribute
    {
        public ContentViewAttribute(string id, int key, double order = 100, string? context = null, Type? resources = null)
            : base(typeof(ContentView))
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

            Id = id;
            Context = context ?? id;
            Key = key;
            Order = order;
        }

        public string Id { get; set; }

        public string DisplayName { get; }

        public int Key { get; }

        public double Order { get; }

        public string Context { get; }

        public bool Visible { get; set; } = true;
    }
}