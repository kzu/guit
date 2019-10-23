using System;
using System.Composition;
using System.Globalization;
using System.Resources;
using Terminal.Gui;

namespace Guit
{
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = false)]
    public class MenuCommandAttribute : ExportAttribute
    {
        public MenuCommandAttribute(string id, Key hotKey, string? context = null, Type? resources = null) :
            this(id, hotKey, (double)hotKey, context, resources)
        { }

        public MenuCommandAttribute(string id, int key, string? context = null, Type? resources = null) :
            this(id, key, (double)key, context, resources)
        { }

        public MenuCommandAttribute(string id, Key hotKey, double order, string? context = null, Type? resources = null) :
            this(id, (int)hotKey, order, context, resources)
        { }

        public MenuCommandAttribute(string id, int key, double order, string? context = null, Type? resources = null)
            : base(typeof(IMenuCommand))
        {
            if (resources != null)
            {
                var resourceManager = new ResourceManager(resources);
                try
                {
                    DisplayName = resourceManager.GetString(id, CultureInfo.CurrentUICulture) ?? id;
                    Description = resourceManager.GetString($"{id}.Description", CultureInfo.CurrentUICulture);
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
            Context = context;
            Key = key;
            Order = order;
        }

        public string Id { get; set; }

        public string DisplayName { get; }

        public int Key { get; }

        public double Order { get; }

        public string? Context { get; }

        public bool DefaultVisible { get; set; } = true;

        public bool ReportProgress { get; set; } = true;

        public bool IsDynamic { get; set; }

        public string Description { get; set; }
    }
}