/*
 * Mark Diedericks
 * 01/08/2018
 * Version 1.0.5
 * Textual editor view model
 */

using Macro_Engine;
using Macro_UI.Model;
using Macro_UI.Routing;
using Macro_UI.View;
using Macro_UI.ViewModel.Base;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Macro_Engine.Macros;

namespace Macro_UI.ViewModel
{
    public class TextualEditorViewModel : DocumentViewModel
    {
        /// <summary>
        /// Instantiation of TextualEditorViewModel
        /// </summary>
        public TextualEditorViewModel()
        {
            Model = new TextualEditorModel(Guid.Empty);
            IsSaved = true;
        }

        /// <summary>
        /// Saves the macro associated with the document
        /// </summary>
        /// <param name="OnComplete">Action to be fired on the tasks completetion</param>
        public override void Save(Action OnComplete)
        {
            MacroUI.GetInstance().GetMacro(Macro).Source = Source.Text;
            MacroUI.GetInstance().GetMacro(Macro).Save();
            base.Save(OnComplete);
        }

        /// <summary>
        /// Executes the macro associated with the document
        /// </summary>
        /// <param name="OnComplete">Action to be fired on the tasks completetion</param>
        public override void Start(Action OnComplete)
        {
            IMacro m = MacroUI.GetInstance().GetMacro(Macro);
            string runtime = (string)MainWindowViewModel.GetInstance().SelectedRuntime.Tag;

            m.Source = Source.Text;
            m.Execute(MainWindowViewModel.GetInstance().AsyncExecution, runtime);
            base.Start(null);
        }

        /// <summary>
        /// Terminates the execution of the macro associated with the document
        /// </summary>
        /// <param name="OnComplete">Action to be fired on the tasks completetion</param>
        public override void Stop(Action OnComplete)
        {
            //Events.OnTerminateExecutionInvoke();
            Events.InvokeEvent("OnTerminateExecution");

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
