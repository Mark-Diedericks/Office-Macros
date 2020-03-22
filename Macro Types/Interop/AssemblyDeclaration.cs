using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Macro_Engine.Interop
{
    /// <summary>
    /// Data structure containing info on assemblies, serializable data structure for saving
    /// </summary>
    [TypeConverter(typeof(AssemblyDeclarationConverter))]
    [SettingsSerializeAs(SettingsSerializeAs.String)]
    public class AssemblyDeclaration
    {
        public string displayname;
        public string filepath;
        public bool enabled;

        /// <summary>
        /// Initialize new instance of the data structure
        /// </summary>
        /// <param name="dn">Display name of the assembly</param>
        /// <param name="ln">Full file path of the assembly</param>
        /// <param name="enabled">If usage of the assembly is enabled</param>
        public AssemblyDeclaration(string dn, string ln, bool enabled)
        {
            displayname = dn;
            filepath = ln;
        }
    }

    /// <summary>
    /// Converter to serialize AssemblyDeclaration instances
    /// </summary>
    public class AssemblyDeclarationConverter : TypeConverter
    {
        /// <summary>
        /// Interface method, ensures that the source can be deserialized into an AssemblyDeclaration
        /// </summary>
        /// <param name="context"></param>
        /// <param name="sourceType"></param>
        /// <returns>Bool identifying if it can be converted</returns>
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string);
        }

        /// <summary>
        /// Interface method, deserializes a string into an AssemblyDeclaration instance
        /// </summary>
        /// <param name="context"></param>
        /// <param name="culture"></param>
        /// <param name="value"></param>
        /// <returns>The AssemblyDeclaration that has been deserialized</returns>
        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            if (value is string)
            {
                string[] parts = ((string)value).Split(new char[] { ',' });
                AssemblyDeclaration assembly = new AssemblyDeclaration(parts.Length > 0 ? parts[0] : "", parts.Length > 2 ? parts[2] : "", parts.Length > 2 ? bool.Parse(parts[2]) : false);
                return assembly;
            }

            return base.ConvertFrom(context, culture, value);
        }

        /// <summary>
        /// Interface method, serializes AssemblyDeclaration as string
        /// </summary>
        /// <param name="context"></param>
        /// <param name="culture"></param>
        /// <param name="value"></param>
        /// <param name="destinationType"></param>
        /// <returns>The string of the serialized AssemblyDeclaration</returns>
        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                AssemblyDeclaration assembly = value as AssemblyDeclaration;
                return string.Format("{0},{1},{2}", assembly.displayname, assembly.filepath, assembly.enabled);
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }

    }
}
