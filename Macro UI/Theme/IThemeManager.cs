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

        ObservableCollection<ITheme> Themes { get; }
        ITheme ActiveTheme { get; }

        bool AddTheme(ITheme theme);
        bool SetTheme(string name);

    }
}
