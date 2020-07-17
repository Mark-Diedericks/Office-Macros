/*
 * Thomas Willwacher (Original), Mark Diedericks (Editor)
 * 18/08/2018
 * Version 1.0.0
 * Find/Replace dialog, basic logic
 */

using Macro_Editor.ViewModel;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Macro_Engine;

namespace Macro_Editor.View
{
    /// <summary>
    /// Interaction logic for FindReplaceDialog.xaml
    /// </summary>
    public partial class FindReplaceDialog : MetroWindow
    {
        private static string textToFind = "";
        private static bool caseSensitive = true;
        private static bool wholeWord = true;
        private static bool useRegex = false;
        private static bool useWildcards = false;
        private static bool searchUp = false;

        private TextEditor editor;

        /// <summary>
        /// Thomas Willwacher - FindReplaceDialog constructor
        /// </summary>
        /// <param name="editor"></param>
        public FindReplaceDialog(TextEditor editor)
        {
            InitializeComponent();

            this.editor = editor;

            txtFind.Text = txtFind2.Text = textToFind;
            cbCaseSensitive.IsChecked = caseSensitive;
            cbWholeWord.IsChecked = wholeWord;
            cbRegex.IsChecked = useRegex;
            cbWildcards.IsChecked = useWildcards;
            cbSearchUp.IsChecked = searchUp;

            Events.SubscribeEvent("ThemeChanged", (Action)ThemeChangedEvent);
            //Routing.EventManager.ThemeChangedEvent += ThemeChangedEvent;
            ThemeChangedEvent();
        }

        private ResourceDictionary ThemeDictionary
        {
            get
            {
                return Resources.MergedDictionaries[1];
            }
        }

        /// <summary>
        /// ThemeChanged event callback, changes the theme
        /// </summary>
        private void ThemeChangedEvent()
        {
            ThemeDictionary.MergedDictionaries.Clear();

            foreach (Uri uri in MainWindowViewModel.GetInstance().ActiveTheme.UriList)
            {
                try
                { 
                    ThemeDictionary.MergedDictionaries.Add(new ResourceDictionary() { Source = uri });
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine("Couldn't find: " + uri);
                    System.Diagnostics.Debug.WriteLine(e.Message);
                }
            }
        }

        /// <summary>
        /// Thomas Willwacher
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closed(object sender, System.EventArgs e)
        {
            textToFind = txtFind2.Text;
            caseSensitive = (cbCaseSensitive.IsChecked == true);
            wholeWord = (cbWholeWord.IsChecked == true);
            useRegex = (cbRegex.IsChecked == true);
            useWildcards = (cbWildcards.IsChecked == true);
            searchUp = (cbSearchUp.IsChecked == true);

            theDialog = null;
        }

        /// <summary>
        /// Thomas Willwacher
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FindNextClick(object sender, RoutedEventArgs e)
        {
            if (!FindNext(txtFind.Text))
                System.Media.SystemSounds.Beep.Play();
        }

        /// <summary>
        /// Thomas Willwacher
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FindNext2Click(object sender, RoutedEventArgs e)
        {
            if (!FindNext(txtFind2.Text))
                System.Media.SystemSounds.Beep.Play();
        }

        /// <summary>
        /// Thomas Willwacher
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ReplaceClick(object sender, RoutedEventArgs e)
        {
            Regex regex = GetRegEx(txtFind2.Text);
            string input = editor.Text.Substring(editor.SelectionStart, editor.SelectionLength);
            Match match = regex.Match(input);
            bool replaced = false;
            if (match.Success && match.Index == 0 && match.Length == input.Length)
            {
                editor.Document.Replace(editor.SelectionStart, editor.SelectionLength, txtReplace.Text);
                replaced = true;
            }
            
            if (!FindNext(txtFind2.Text) && !replaced)
                System.Media.SystemSounds.Beep.Play();
        }

        /// <summary>
        /// Thomas Willwacher
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ReplaceAllClick(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to Replace All occurences of \"" +
            txtFind2.Text + "\" with \"" + txtReplace.Text + "\"?",
                "Replace All", MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.OK)
            {
                Regex regex = GetRegEx(txtFind2.Text, true);
                int offset = 0;
                editor.BeginChange();
                foreach (Match match in regex.Matches(editor.Text))
                {
                    editor.Document.Replace(offset + match.Index, match.Length, txtReplace.Text);
                    offset += txtReplace.Text.Length - match.Length;
                }
                editor.EndChange();
            }
        }

        /// <summary>
        /// Thomas Willwacher
        /// </summary>
        /// <param name="textToFind"></param>
        /// <returns></returns>
        private bool FindNext(string textToFind)
        {
            Regex regex = GetRegEx(textToFind);
            int start = regex.Options.HasFlag(RegexOptions.RightToLeft) ?
            editor.SelectionStart : editor.SelectionStart + editor.SelectionLength;
            Match match = regex.Match(editor.Text, start);

            if (!match.Success)  // start again from beginning or end
            {
                if (regex.Options.HasFlag(RegexOptions.RightToLeft))
                    match = regex.Match(editor.Text, editor.Text.Length);
                else
                    match = regex.Match(editor.Text, 0);
            }

            if (match.Success)
            {
                editor.Select(match.Index, match.Length);
                TextLocation loc = editor.Document.GetLocation(match.Index);
                editor.ScrollTo(loc.Line, loc.Column);
            }

            return match.Success;
        }

        /// <summary>
        /// Thomas Willwacher
        /// </summary>
        /// <param name="textToFind"></param>
        /// <param name="leftToRight"></param>
        /// <returns></returns>
        private Regex GetRegEx(string textToFind, bool leftToRight = false)
        {
            RegexOptions options = RegexOptions.None;
            if (cbSearchUp.IsChecked == true && !leftToRight)
                options |= RegexOptions.RightToLeft;
            if (cbCaseSensitive.IsChecked == false)
                options |= RegexOptions.IgnoreCase;

            if (cbRegex.IsChecked == true)
            {
                return new Regex(textToFind, options);
            }
            else
            {
                string pattern = Regex.Escape(textToFind);
                if (cbWildcards.IsChecked == true)
                    pattern = pattern.Replace("\\*", ".*").Replace("\\?", ".");
                if (cbWholeWord.IsChecked == true)
                    pattern = "\\b" + pattern + "\\b";
                return new Regex(pattern, options);
            }
        }

        private static FindReplaceDialog theDialog = null;

        /// <summary>
        /// Thomas Willwacher
        /// </summary>
        /// <param name="editor"></param>
        /// <param name="index"></param>
        private static void ShowForType(TextEditor editor, int index)
        {
            if (theDialog == null)
            {
                theDialog = new FindReplaceDialog(editor);
                theDialog.tabMain.SelectedIndex = index;
                theDialog.Show();
                theDialog.Activate();
            }
            else
            {
                theDialog.tabMain.SelectedIndex = index;
                theDialog.Activate();
            }

            if (!editor.TextArea.Selection.IsMultiline)
            {
                theDialog.txtFind.Text = theDialog.txtFind2.Text = editor.TextArea.Selection.GetText();
                theDialog.txtFind.SelectAll();
                theDialog.txtFind2.SelectAll();
                theDialog.txtFind2.Focus();
            }
        }

        /// <summary>
        /// Thomas Willwacher
        /// </summary>
        /// <param name="editor"></param>
        public static void ShowForReplace(TextEditor editor)
        {
            ShowForType(editor, 1);
        }

        /// <summary>
        /// Thomas Willwacher
        /// </summary>
        /// <param name="editor"></param>
        public static void ShowForFind(TextEditor editor)
        {
            ShowForType(editor, 0);
        }
    }
}
