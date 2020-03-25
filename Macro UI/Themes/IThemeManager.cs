/*
 * Mark Diedericks
 * 03/07/2018
 * Version 1.0.5
 * ThemeManager abstraction interface
 */

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Macro_UI.Themes
{
    public interface IThemeManager
    {
        Dictionary<string, Uri> Accents { get; }
        ObservableCollection<ITheme> Themes { get; }
        ITheme ActiveTheme { get; }
        string ActiveAccent { get; }

        bool AddTheme(ITheme theme);
        bool SetTheme(string name);

        bool AddAccent(string name, Uri resource);
        bool SetAccent(string name);
    }
}
