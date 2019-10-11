using System;
using System.Collections.Generic;
using System.Linq;

namespace Guit
{
    public static class ConfigurationExtensions
    {
        static readonly IDictionary<Type, Func<LibGit2Sharp.Configuration, string, object>> getValueOrDefault = new Dictionary<Type, Func<LibGit2Sharp.Configuration, string, object>>
        {
            { typeof(int), (config, key) => config.GetValueOrDefault<int>(key) },
            { typeof(long), (config, key) => config.GetValueOrDefault<long>(key) },
            { typeof(bool), (config, key) => config.GetValueOrDefault<bool>(key) },
            { typeof(string), (config, key) => config.GetValueOrDefault<string>(key) },
        };

        public static T Read<T>(this LibGit2Sharp.Configuration configuration, string section, string? subSection = null) where T : new()
        {
            var result = new T();
            var key = section;
            if (!string.IsNullOrEmpty(subSection))
                key += "." + subSection;

            foreach (var prop in typeof(T).GetProperties().Where(x => x.CanWrite))
            {
                if (getValueOrDefault.TryGetValue(prop.PropertyType, out var getter))
                {
                    prop.SetValue(result, getter(configuration, key + "." + prop.Name));
                }
            }

            return result;
        }
    }
}
