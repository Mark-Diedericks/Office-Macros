/*
 * Mark Diedericks
 * 26/07/2018
 * Version 1.0.7
 * Textual Editor UI Control
 */

using Macro_UI.Routing;
using Macro_UI.Utilities;
using Macro_UI.ViewModel;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;

namespace Macro_UI.View
{
    /// <summary>
    /// Interaction logic for TextualEditorView.xaml
    /// </summary>
    public partial class TextualEditorView : UserControl
    {
        private const double FONT_MAX_SIZE = 72d;
        private const double FONT_MIN_SIZE = 6d;

        /// <summary>
        /// Instantiation of TextualEditorView
        /// </summary>
        public TextualEditorView()
        {
            InitializeComponent();
            DataContextChanged += TextualEditorView_DataContextChanged;

            m_CodeEditor.SyntaxHighlighting = HighlightingLoader.Load(new XmlTextReader(SyntaxStyleLoader.GetStyleStream()), HighlightingManager.Instance);
            SyntaxStyleLoader.LoadColorValues();
            SyntaxStyleLoader.OnStyleChanged += delegate () 
            {
                m_CodeEditor.SyntaxHighlighting = HighlightingLoader.Load(new XmlTextReader(SyntaxStyleLoader.GetStyleStream()), HighlightingManager.Instance);
            };
        }

        /// <summary>
        /// Gets the AvalonEdit TextEditor Control
        /// </summary>
        /// <returns>AvalonEdit TextEditor</returns>
        public TextEditor GetAvalonTextEditor()
        {
            return m_CodeEditor;
        }

        /// <summary>
        /// DataContextChanged event callback, binds commands
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextualEditorView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!(DataContext is TextualEditorViewModel))
                return;

            ((TextualEditorViewModel)DataContext).UndoCommand = new RelayCommand(call => m_CodeEditor.Undo(), call => m_CodeEditor.CanUndo);
            ((TextualEditorViewModel)DataContext).RedoCommand = new RelayCommand(call => m_CodeEditor.Redo(), call => m_CodeEditor.CanRedo);
            ((TextualEditorViewModel)DataContext).CopyCommand = new RelayCommand(call => m_CodeEditor.Copy());
            ((TextualEditorViewModel)DataContext).CutCommand = new RelayCommand(call => m_CodeEditor.Cut());
            ((TextualEditorViewModel)DataContext).PasteCommand = new RelayCommand(call => m_CodeEditor.Paste());

            ((TextualEditorViewModel)DataContext).GetTextEditorEvent = () => { return GetAvalonTextEditor(); };
        }

        private void CodeEditor_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            bool isCTRL = Keyboard.Modifiers == ModifierKeys.Control;
            if(isCTRL)
            {
                double newSize = Properties.Settings.Default.EditorFontSize + (e.Delta > 0 ? 1 : -1);
                newSize = Math.Max(FONT_MIN_SIZE, Math.Min(FONT_MAX_SIZE, newSize));

                Properties.Settings.Default.EditorFontSize = newSize;
                Properties.Settings.Default.Save();

                e.Handled = true;
            }
        }
    }
}
