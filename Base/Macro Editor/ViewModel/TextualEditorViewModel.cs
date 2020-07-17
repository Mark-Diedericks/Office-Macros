/*
 * Mark Diedericks
 * 01/08/2018
 * Version 1.0.5
 * Textual editor view model
 */

using Macro_Engine;
using Macro_Editor.Model;
using Macro_Editor.Routing;
using Macro_Editor.View;
using Macro_Editor.ViewModel.Base;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Macro_Engine.Macros;

namespace Macro_Editor.ViewModel
{
    public class TextualEditorViewModel : DocumentViewModel
    {
        /// <summary>
        /// Instantiation of TextualEditorViewModel
        /// </summary>
        public TextualEditorViewModel()
        {
            Model = new TextualEditorModel(null);
            IsSaved = true;
        }

        /// <summary>
        /// Saves the macro associated with the document
        /// </summary>
        /// <param name="OnComplete">Action to be fired on the tasks completetion</param>
        public override void Save(Action OnComplete)
        {
            Declaration.Content = Source.Text;
            Declaration.Save();
            base.Save(OnComplete);
        }

        /// <summary>
        /// Executes the macro associated with the document
        /// </summary>
        /// <param name="OnComplete">Action to be fired on the tasks completetion</param>
        public async override void Start(Action OnComplete)
        {
            string runtime = (string)MainWindowViewModel.GetInstance().SelectedRuntime.Tag;
            Declaration.Content = Source.Text;

            //Execute(MainWindowViewModel.GetInstance().AsyncExecution, OnComplete, runtime);

            await MacroUI.GetInstance().TryExecuteFile(Declaration, MainWindowViewModel.GetInstance().AsyncExecution, runtime);
            base.Start(OnComplete);
        }

        /// <summary>
        /// Terminates the execution of the macro associated with the document
        /// </summary>
        /// <param name="OnComplete">Action to be fired on the tasks completetion</param>
        public override void Stop(Action OnComplete)
        {
            //Events.OnTerminateExecutionInvoke();
            Events.InvokeEvent("OnTerminateExecution", OnComplete);
            base.Stop(null);
        }

        /// <summary>
        /// Gets the AvalonEdit TextEditor Control
        /// </summary>
        /// <returns>AvalonEdit TextEditor</returns>
        public TextEditor GetTextEditor()
        {
            if (GetTextEditorEvent == null)
                return null;

            return GetTextEditorEvent?.Invoke();
        }

        #region Model

        public new TextualEditorModel Model
        {
            get
            {
                return (TextualEditorModel)base.Model;
            }

            set
            {
                if (((TextualEditorModel)base.Model) != value)
                {
                    base.Model = value;
                    OnPropertyChanged(nameof(Model));
                }
            }
        }

        #endregion

        #region Source

        public TextDocument Source
        {
            get
            {
                return Model.Source;
            }

            set
            {
                if (Model.Source != value)
                {
                    Model.Source = value;
                    OnPropertyChanged(nameof(Source));
                }
            }
        }

        #endregion

        #region GetTextEditorEvent

        private Func<TextEditor> m_GetTextEditorEvent;
        public Func<TextEditor> GetTextEditorEvent
        {
            get
            {
                return m_GetTextEditorEvent;
            }

            set
            {
                if (m_GetTextEditorEvent != value)
                {
                    m_GetTextEditorEvent = value;
                    OnPropertyChanged(nameof(GetTextEditorEvent));
                }
            }
        }

        #endregion
    }
}
