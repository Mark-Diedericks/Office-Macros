using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Macro_Engine.Macros
{
    /// <summary>
    /// Data strcture containing info on a macro, serializable data structure for saving
    /// </summary>
    [TypeConverter(typeof(MacroDeclarationConverter))]
    [SettingsSerializeAs(SettingsSerializeAs.String)]
    public class MacroDeclaration
    {
        public Guid ID { get; set; }
        public string Language { get; }
        public FileInfo Info { get; }
        public IMacro Macro { get; }

        public MacroDeclaration(string lang, FileInfo info, IMacro macro) : this(lang, info, macro, Guid.Empty)
        {}

        public MacroDeclaration(string lang, FileInfo info, IMacro macro, Guid id)
        {
            Language = lang;
            Info = info;
            Macro = macro;
            ID = id;
        }
    }

    /// <summary>
    /// Converter to serialize MacroDeclaration instances
    /// </summary>
    public class MacroDeclarationConverter : TypeConverter
    {
        /// <summary>
        /// Interface method, ensures that the source can be deserialized into an MacroDeclaration
        /// </summary>
        /// <param name="context"></param>
        /// <param name="sourceType"></param>
        /// <returns>Bool identifying if it can be converted</returns>
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string);
        }

        /// <summary>
        /// Interface method, deserializes a string into an MacroDeclaration instance
        /// </summary>
        /// <param name="context"></param>
        /// <param name="culture"></param>
        /// <param name="value"></param>
        /// <returns>The MacroDeclaration that has been deserialized</returns>
        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            if (value is string)
            {
                string[] parts = ((string)value).Split(new char[] { ',' });

                FileInfo info = new FileInfo(parts.Length > 1 ? parts[1] : "");
                MacroDeclaration macro = new MacroDeclaration(parts[0], info);

                return macro;
            }

            return base.ConvertFrom(context, culture, value);
        }

        /// <summary>
        /// Interface method, serializes MacroDeclaration as string
        /// </summary>
        /// <param name="context"></param>
        /// <param name="culture"></param>
        /// <param name="value"></param>
        /// <param name="destinationType"></param>
        /// <returns>The string of the serialized MacroDeclaration</returns>
        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                MacroDeclaration macro = value as MacroDeclaration;
                return string.Format("{0},{1}", macro.Language, macro.Info.FullName);
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }

    }
}
