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

namespace Macro_Editor.Themes
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
                new Uri("pack://application:,,,/Macro Editor;component/Themes/BaseAccent.xaml"),
                new Uri("pack://application:,,,/Macro Editor;component/Themes/ThemeLight.xaml"),
            };
        }

        public void SetAccent(Uri resource)
        {
            UriList = new List<Uri>
            {
                resource,
                new Uri("pack://application:,,,/Macro Editor;component/Themes/ThemeLight.xaml"),
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
