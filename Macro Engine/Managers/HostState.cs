using Macro_Engine.Interop;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace Macro_Engine
{
    [TypeConverter(typeof(HostStateConverter))]
    [SettingsSerializeAs(SettingsSerializeAs.Xml)]
    public class HostState
    {
        public string[] Workspaces { get; }
        public string[] RibbonMacros { get; }
        public string ActiveMacro { get; }
        public AssemblyDeclaration[] Assemblies { get; }

        public HostState(string[] workspaces, string[] ribbonMacros, string activeMacro, AssemblyDeclaration[] assemblies)
        {
            Workspaces = workspaces;
            RibbonMacros = ribbonMacros;
            ActiveMacro = activeMacro;
            Assemblies = assemblies;
        }

        public HostState() : this(new string[] { }, new string[] { }, "", new Macro_Engine.Interop.AssemblyDeclaration[] { })
        { }
    }

    class HostStateConverter : TypeConverter
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
                return Deserialize((string)value);
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
                HostState state = value as HostState;
                return Serialize(state);
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }

        private HostState Deserialize(string value)
        {
            XmlSerializer ser = new XmlSerializer(typeof(HostState));
            return (HostState)ser.Deserialize(new StringReader(value));
        }

        private string Serialize(HostState value)
        {
            if (value == null)
                return string.Empty;

            XmlSerializer ser = new XmlSerializer(typeof(HostState));
            using (StringWriter sw = new StringWriter())
            {
                using(XmlWriter xw = XmlWriter.Create(sw, new XmlWriterSettings { Indent = true }))
                {
                    ser.Serialize(xw, value);
                    return sw.ToString();
                }
            }
        }
    }
}
