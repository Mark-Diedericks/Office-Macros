/*
 * Mark Diedericks
 * 30107/2018
 * Version 1.0.3
 * Syntax style loading and parsing utility
 */
using Macro_UI.View;
using Macro_UI.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Macro_UI.Utilities
{
    public class SyntaxStyleLoader
    {
        public enum SyntaxStyleColor
        {
            DIGIT = 0,
            COMMENT = 1,
            STRING = 2,
            PAIR = 3,
            CLASS = 4,
            STATEMENT = 5,
            FUNCTION = 6,
            BOOLEAN = 7
        }

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
        private static string[] s_ColorValues;

        /// <summary>
        /// Produces a stream of data representing the syntax style
        /// </summary>
        /// <returns>The stream of syntax style data</returns>
        public static Stream GetStyleStream()
        {
            throw new NotImplementedException();

            string style = Properties.Resources.IronPython;

            style = style.Replace(DIGIT, s_ColorValues[(int)SyntaxStyleColor.DIGIT]);
            style = style.Replace(COMMENT, s_ColorValues[(int)SyntaxStyleColor.COMMENT]);
            style = style.Replace(STRING, s_ColorValues[(int)SyntaxStyleColor.STRING]);
            style = style.Replace(PAIR, s_ColorValues[(int)SyntaxStyleColor.PAIR]);
            style = style.Replace(CLASS, s_ColorValues[(int)SyntaxStyleColor.CLASS]);
            style = style.Replace(STATEMENT, s_ColorValues[(int)SyntaxStyleColor.STATEMENT]);
            style = style.Replace(FUNCTION, s_ColorValues[(int)SyntaxStyleColor.FUNCTION]);
            style = style.Replace(BOOLEAN, s_ColorValues[(int)SyntaxStyleColor.BOOLEAN]);

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
        private static string CreateSyntaxStyleString(string[] values)
        {
            StringBuilder sb = new StringBuilder();

            foreach (string val in values)
                sb.Append(val + ';');

            return sb.ToString();
        }

        /// <summary>
        /// Deserializes the colours of the syntax style
        /// </summary>
        /// <param name="value">Serialized value</param>
        /// <returns>Colour values</returns>
        private static string[] ParseSyntaxStyleString(string value)
        {
            return value.Split(';');
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
        public static void SetSyntaxStyle(string[] values)
        {
            if (values.Length != 8)
                return;

            s_ColorValues = values;

            UpdateSyntaxStyle();
        }

        /// <summary>
        /// Saves the serialized colours of the syntax style
        /// </summary>
        /// <param name="DarkTheme">Is dark theme enabled</param>
        public static void SaveSyntaxStyle(bool DarkTheme)
        {
            if (DarkTheme)
                Properties.Settings.Default.SyntaxStyleDark = CreateSyntaxStyleString(s_ColorValues);
            else
                Properties.Settings.Default.SyntaxStyleLight = CreateSyntaxStyleString(s_ColorValues);

            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// Sets a specific colour of syntax style
        /// </summary>
        /// <param name="color">Which color id it should be applied to</param>
        /// <param name="value">The value to be applied</param>
        public static void SetSyntaxColor(SyntaxStyleColor color, string value)
        {
            s_ColorValues[(int)color] = value;
        }

        /// <summary>
        /// Loads serialized colour values to use in the syntax style
        /// </summary>
        public static void LoadColorValues()
        {
           if (MainWindowViewModel.GetInstance().ActiveTheme.Name == "Dark")
                s_ColorValues = ParseSyntaxStyleString(Properties.Settings.Default.SyntaxStyleDark);
            else
                s_ColorValues = ParseSyntaxStyleString(Properties.Settings.Default.SyntaxStyleLight);

            UpdateSyntaxStyle();
        }

        /// <summary>
        /// Gets the syntax style colour values
        /// </summary>
        /// <returns>Colour values</returns>
        public static string[] GetValues()
        {
            if (s_ColorValues == null)
                LoadColorValues();

            return s_ColorValues;
        }
    }
}
