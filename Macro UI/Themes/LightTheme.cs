/*
 * Mark Diedericks
 * 20/07/2018
 * Version 1.0.5
 * Light Theme for UI
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Macro_UI.Themes
{
    public sealed class LightTheme : ITheme
    {
        /// <summary>
        /// Instantiate a new LightTheme
        /// </summary>
        public LightTheme()
        {
            UriList = new List<Uri>
            {
                //new Uri("pack://application:,,,/MahApps.Metro;component/Styles/Controls.xaml"),
                //new Uri("pack://application:,,,/MahApps.Metro;component/Styles/Fonts.xaml"),
                //new Uri("pack://application:,,,/MahApps.Metro;component/Styles/Colors.xaml"),
                //new Uri("pack://application:,,,/MahApps.Metro;component/Styles/Accents/Blue.xaml"),
                //new Uri("pack://application:,,,/Macro UI;component/Themes/ExcelAccent.xaml"),
                new Uri("pack://application:,,,/MahApps.Metro;component/Styles/Accents/BaseLight.xaml"),
                new Uri("pack://application:,,,/Macro UI;component/Themes/ExcelAccent.xaml"),
                new Uri("pack://application:,,,/AvalonDock.Themes.VS2012;component/LightTheme.xaml"),
            };
        }

        /// <summary>
        /// Inherited Memebers
        /// </summary>

        public IList<Uri> UriList { get; internal set; }
        public string Name
        {
            get
            {
                return "Light";
            }
        }
    }
}
