/*
 * Mark Diedericks
 * 22/07/2018
 * Version 1.0.1
 * Console view model
 */
 
using Macro_UI.Model;
using Macro_UI.Utilities;
using Macro_UI.ViewModel.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Macro_UI.ViewModel
{
    public class ConsoleViewModel : ToolViewModel
    {
        /// <summary>
        /// Instantiate ConsoleViewModel
        /// </summary>
        public ConsoleViewModel()
        {
            Model = new ConsoleModel();
        }


        public void BeginInput()
        {
            InputStart = TextLines.Length;
            InputText = "";

            TextReadOnly = false;

            FocusEvent?.Invoke();
        }

        public void EndInput(string text)
        {
            int textLen = text.Length;
            InputText = (textLen > InputStart) ? text.Substring(InputStart, textLen - InputStart) : "";

            TextReadOnly = true;
        }

        #region Model

        public new ConsoleModel Model
        {
            get
            {
                return (ConsoleModel)base.Model;
            }

            set
            {
                if (((ConsoleModel)base.Model) != value)
                {
                    base.Model = value;
                    OnPropertyChanged(nameof(Model));
                }
            }
        }

        public string TextLines
        {
            get
            {
                return Model.TextLines;
            }
            set
            {
                Model.TextLines = value;
                OnPropertyChanged(nameof(TextLines));
            }
        }

        public bool TextReadOnly
        {
            get
            {
                return Model.TextReadOnly;
            }
            set
            {
                Model.TextReadOnly = value;
                OnPropertyChanged(nameof(TextReadOnly));
            }
        }

        public int InputStart
        {
            get
            {
                return Model.InputStart;
            }
            set
            {
                Model.InputStart = value;
                OnPropertyChanged(nameof(InputStart));
            }
        }

        public string InputText
        {
            get
            {
                return Model.InputText;
            }
            set
            {
                Model.InputText = value;
                OnPropertyChanged(nameof(InputText));
            }
        }

        #endregion

        #region FocusEvent
        private Action m_FocusEvent;
        public Action FocusEvent
        {
            get
            {
                return m_FocusEvent;
            }
            set
            {
                if (m_FocusEvent != value)
                {
                    m_FocusEvent = value;
                    OnPropertyChanged(nameof(FocusEvent));
                }
            }
        }
        #endregion
    }
}
