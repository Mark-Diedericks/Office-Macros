/*
 * Mark Diedericks
 * 20/07/2018
 * Version 1.0.5
 * Dark Theme For UI
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Macro_UI.Themes
{
    public sealed class DarkTheme : ITheme
    {
        /// <summary>
        /// Instantiate a new DarkTheme
        /// </summary>
        public DarkTheme()
        {
            UriList = new List<Uri>
            {
                new Uri("pack://application:,,,/MahApps.Metro;component/Styles/Accents/BaseDark.xaml"),
                new Uri("pack://application:,,,/Macro UI;component/Themes/BaseAccent.xaml"),
                new Uri("pack://application:,,,/AvalonDock.Themes.VS2012;component/DarkTheme.xaml"),
            };
        }

        public void SetAccent(Uri resource)
        {
            UriList = new List<Uri>
            {
                new Uri("pack://application:,,,/MahApps.Metro;component/Styles/Accents/BaseDark.xaml"),
                resource,
                new Uri("pack://application:,,,/AvalonDock.Themes.VS2012;component/DarkTheme.xaml"),
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
                return "Dark";
            }
        }
    }
}
