/*
 * Mark Diedericks
 * 20/07/2018
 * Version 1.0.5
 * Theme abstraction interface
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Macro_Editor.Themes
{
    public interface ITheme
    {
        /// <summary>
        /// Basic Functions for Theme
        /// </summary>
        IList<Uri> UriList { get; }
        string Name { get; }

        void SetAccent(Uri resource);
    }
}
