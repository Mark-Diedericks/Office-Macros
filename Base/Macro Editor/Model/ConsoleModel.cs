/*
 * Mark Diedericks
 * 30/07/2018
 * Version 1.0.1
 * Console model
 */

using Macro_Engine;
using Macro_Editor.Model.Base;
using Macro_Editor.Utilities;
using Macro_Editor.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Macro_Editor.Model
{
    public class ConsoleModel : ToolModel
    {
        private DispatcherTimer m_Timer;

        /// <summary>
        /// Console Model Instantiation
        /// </summary>
        public ConsoleModel()
        {
            TextLines = "";
            TextReadOnly = true;
        }

        #region TextLines

        private string m_Text;
        public string TextLines
        {
            get
            {
                return m_Text;
            }
            set
            {
                m_Text = value;
                OnPropertyChanged(nameof(TextLines));
            }
        }

        #endregion

        #region TextReadOnly

        private bool m_TextReadOnly;
        public bool TextReadOnly
        {
            get
            {
                return m_TextReadOnly;
            }
            set
            {
                m_TextReadOnly = value;
                OnPropertyChanged(nameof(TextReadOnly));
            }
        }

        #endregion

        #region InputStart

        private int m_InputStart;
        public int InputStart
        {
            get
            {
                return m_InputStart;
            }
            set
            {
                m_InputStart = value;
                OnPropertyChanged(nameof(m_InputStart));
            }
        }

        #endregion

        #region InputText

        private string m_InputText;
        public string InputText
        {
            get
            {
                return m_InputText;
            }
            set
            {
                m_InputText = value;
                OnPropertyChanged(nameof(InputText));
            }
        }

        #endregion
    }
}
