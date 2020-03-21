/*
 * Mark Diedericks
 * 27/07/2018
 * Version 1.0.0
 * Focus utility manager UI element focus
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Macro_UI.Utilities
{
    public static class FocusUtility
    {
        public static readonly DependencyProperty IsFocusedProperty = DependencyProperty.RegisterAttached("IsFocused", typeof(bool), typeof(FocusUtility), new UIPropertyMetadata(false, OnIsFocusedPropertyChanged));

        /// <summary>
        /// Gets if an UI element is focused
        /// </summary>
        /// <param name="obj">UI Element</param>
        /// <returns>Bool representation of is focused</returns>
        public static bool GetIsFocused(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsFocusedProperty);
        }

        /// <summary>
        /// Sets whether an UI element is focused
        /// </summary>
        /// <param name="obj">UI element</param>
        /// <param name="value">Bool represenation of is focused</param>
        public static void SetIsFocused(DependencyObject obj, bool value)
        {
            obj.SetValue(IsFocusedProperty, value);
        }

        /// <summary>
        /// Focus Changed event callback
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="args"></param>
        private static void OnIsFocusedPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            UIElement uie = obj as UIElement;

            if ((bool)args.NewValue)
                uie.Focus();
        }
    }
}
