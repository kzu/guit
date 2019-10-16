using System.Composition;

namespace Guit
{
    public class SingletonAttribute : ExportAttribute
    {
        public SingletonAttribute() : base("Singleton") { }
    }
}