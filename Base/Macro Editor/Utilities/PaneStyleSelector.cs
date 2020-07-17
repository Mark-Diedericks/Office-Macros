/*
 * Mark Diedericks
 * 24/07/2018
 * Version 1.0.0
 * Handles the view models style selection for the docking manager
 */

using Macro_Editor.ViewModel.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Macro_Editor.Utilities
{
    internal class PaneStyleSelector : StyleSelector
    {
        public Style ToolStyle { get; set; }
        public Style DocumentStyle { get; set; }

        /// <summary>
        /// Override method
        /// </summary>
        /// <param name="item">ViewModel of the view</param>
        /// <param name="container">UI element (view)</param>
        /// <returns></returns>
        public override Style SelectStyle(object item, DependencyObject container)
        {
            if (item is ToolViewModel)
                return ToolStyle;

            if (item is DocumentViewModel)
                return DocumentStyle;

            return base.SelectStyle(item, container);
        }
    }
}
