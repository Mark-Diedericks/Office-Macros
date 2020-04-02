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
                new Uri("pack://application:,,,/MahApps.Metro;component/Styles/Accents/BaseLight.xaml"),
                new Uri("pack://application:,,,/Macro UI;component/Themes/BaseAccent.xaml"),
                new Uri("pack://application:,,,/AvalonDock.Themes.VS2013;component/LightBrushs.xaml"),
            };
        }

        public void SetAccent(Uri resource)
        {
            UriList = new List<Uri>
            {
                new Uri("pack://application:,,,/MahApps.Metro;component/Styles/Accents/BaseLight.xaml"),
                resource,
                new Uri("pack://application:,,,/AvalonDock.Themes.VS2013;component/LightBrushs.xaml"),
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
