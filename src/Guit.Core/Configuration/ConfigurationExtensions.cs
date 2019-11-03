using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Guit
{
    /// <summary>
    /// Usability overloads for <see cref="LibGit2Sharp.Configuration"/>.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class ConfigurationExtensions
    {
        static readonly IDictionary<Type, Func<LibGit2Sharp.Configuration, string, object>> getValueOrDefault = new Dictionary<Type, Func<LibGit2Sharp.Configuration, string, object>>
        {
            { typeof(int), (config, key) => config.GetValueOrDefault<int>(key) },
            { typeof(long), (config, key) => config.GetValueOrDefault<long>(key) },
            { typeof(bool), (config, key) => config.GetValueOrDefault<bool>(key) },
            { typeof(string), (config, key) => config.GetValueOrDefault<string>(key) },
        };

        /// <summary>
        /// Reads a strong-typed representation of a configuration section, by setting 
        /// all writable properties that match configuration settings in the given 
        /// section and optional subsection.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         Only <see cref="int"/>, <see cref="long"/>, <see cref="bool"/> 
        ///         and <see cref="string"/> typed properties are supported, since 
        ///         those are the supported value types in git configuration.
        ///     </para>
        /// </remarks>
        /// <typeparam name="T">The type of the strongly typed data object to hold the configuration values.</typeparam>
        /// <param name="configuration">The <see cref="LibGit2Sharp.Configuration"/> instance to use for reading values.</param>
        /// <param name="section">The configuration section to read.</param>
        /// <param name="subSection">Optional subsection to read.</param>
        /// <returns>A new instance of <typeparamref name="T"/> with properties set to their matching configuration values.</returns>
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
