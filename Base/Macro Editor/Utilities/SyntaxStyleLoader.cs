/*
 * Mark Diedericks
 * 30107/2018
 * Version 1.0.3
 * Syntax style loading and parsing utility
 */
using Macro_Editor.View;
using Macro_Editor.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Macro_Editor.Utilities
{
    public class SyntaxStyleLoader
    {

        //Style Changed Event
        public delegate void StyleChangeEvent();
        public static event StyleChangeEvent OnStyleChanged;

        //constants
        private const string DIGIT = "#COLOR_DIGIT";        //#202020 - DFDFDF
        private const string COMMENT = "#COLOR_COMMENT";      //#57a64a
        private const string STRING = "#COLOR_STRING";       //#ff22ff
        private const string PAIR = "#COLOR_PAIR";         //#569cd6
        private const string CLASS = "#COLOR_CLASS";        //#4ec9b0
        private const string STATEMENT = "#COLOR_STATEMENT";    //#70b0e0
        private const string FUNCTION = "#COLOR_FUNCTION";     //#404040 - BFBFBF
        private const string BOOLEAN = "#COLOR_BOOLEAN";      //#569cd6

        //Values
        private static SyntaxStyleValues s_ColorValues;

        /// <summary>
        /// Produces a stream of data representing the syntax style
        /// </summary>
        /// <returns>The stream of syntax style data</returns>
        public static Stream GetStyleStream()
        {
            string style = Properties.Resources.DefaultSyntax;

            style = style.Replace(DIGIT, s_ColorValues.DIGIT);
            style = style.Replace(COMMENT, s_ColorValues.COMMENT);
            style = style.Replace(STRING, s_ColorValues.STRING);
            style = style.Replace(PAIR, s_ColorValues.PAIR);
            style = style.Replace(CLASS, s_ColorValues.CLASS);
            style = style.Replace(STATEMENT, s_ColorValues.STATEMENT);
            style = style.Replace(FUNCTION, s_ColorValues.FUNCTION);
            style = style.Replace(BOOLEAN, s_ColorValues.BOOLEAN);

            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);

            writer.Write(style);
            writer.Flush();
            stream.Position = 0;

            return stream;
        }

        /// <summary>
        /// Serializes the colours of the syntax style
        /// </summary>
        /// <param name="values">Colour values</param>
        /// <returns>Serialized value</returns>
        private static string CreateSyntaxStyleString(SyntaxStyleValues values)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(values.DIGIT + ';');
            sb.Append(values.COMMENT + ';');
            sb.Append(values.STRING + ';');
            sb.Append(values.PAIR + ';');
            sb.Append(values.CLASS + ';');
            sb.Append(values.STATEMENT + ';');
            sb.Append(values.FUNCTION + ';');
            sb.Append(values.BOOLEAN + ';');

            return sb.ToString();
        }

        /// <summary>
        /// Deserializes the colours of the syntax style
        /// </summary>
        /// <param name="value">Serialized value</param>
        /// <returns>Colour values</returns>
        private static SyntaxStyleValues ParseSyntaxStyleString(string value)
        {
            SyntaxStyleValues ssv = new SyntaxStyleValues();
            string[] values = value.Split(';');

            if (values.Length < 8)
                return ssv;

            ssv.DIGIT = values[0];
            ssv.COMMENT = values[1];
            ssv.STRING = values[2];
            ssv.PAIR = values[3];
            ssv.CLASS = values[4];
            ssv.STATEMENT = values[5];
            ssv.FUNCTION = values[6];
            ssv.BOOLEAN = values[7];

            return ssv;
        }

        /// <summary>
        /// Fires OnStyleChanged event
        /// </summary>
        public static void UpdateSyntaxStyle()
        {
            OnStyleChanged?.Invoke();
        }

        /// <summary>
        /// Sets the colours of the syntax style
        /// </summary>
        /// <param name="values"></param>
        public static void SetSyntaxStyle(SyntaxStyleValues values)
        {
            if(values != null)
                s_ColorValues = values;

            SaveSyntaxStyle();
            UpdateSyntaxStyle();
        }

        /// <summary>
        /// Saves the serialized colours of the syntax style
        /// </summary>
        public static void SaveSyntaxStyle()
        {
            if (MainWindowViewModel.GetInstance().ActiveTheme.Name == "Dark")
                Properties.Settings.Default.SyntaxStyleDark = CreateSyntaxStyleString(s_ColorValues);
            else
                Properties.Settings.Default.SyntaxStyleLight = CreateSyntaxStyleString(s_ColorValues);

            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// Loads serialized colour values to use in the syntax style
        /// </summary>
        public static void LoadColorValues()
        {
            if (MainWindowViewModel.GetInstance().ActiveTheme.Name == "Dark")
                s_ColorValues = ParseSyntaxStyleString(Properties.Settings.Default.SyntaxStyleDark);
                //s_ColorValues = ParseSyntaxStyleString("#dfdfdf;#57a64a;#ff22ff;#569cd6;#4ec9b0;#70b0e0;#bfbfbf;#569cd6;");
            else
                s_ColorValues = ParseSyntaxStyleString(Properties.Settings.Default.SyntaxStyleLight);
                //s_ColorValues = ParseSyntaxStyleString("#202020;#57a64a;#ff22ff;#569cd6;#4ec9b0;#70b0e0;#404040;#569cd6;");

            UpdateSyntaxStyle();
        }

        /// <summary>
        /// Gets the syntax style colour values
        /// </summary>
        /// <returns>Colour values</returns>
        public static SyntaxStyleValues GetValues()
        {
            if (s_ColorValues == null)
                LoadColorValues();

            return s_ColorValues;
        }
    }
}
