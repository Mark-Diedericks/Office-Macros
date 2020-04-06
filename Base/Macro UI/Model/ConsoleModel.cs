/*
 * Mark Diedericks
 * 30/07/2018
 * Version 1.0.1
 * Console model
 */
 
using Macro_UI.Model.Base;
using Macro_UI.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Macro_UI.Model
{
    public class ConsoleModel : ToolModel
    {
        private static ConsoleModel s_Instance;

        /// <summary>
        /// Gets the instance of ConsoleModel
        /// </summary>
        /// <returns>ConsoleModel instance</returns>
        public static ConsoleModel GetInstance()
        {
            return s_Instance != null ? s_Instance : new ConsoleModel();
        }

        /// <summary>
        /// Console Model Instantiation
        /// </summary>
        public ConsoleModel()
        {
            s_Instance = this;
            Output = new TextBoxWriter(null);
            Error = new TextBoxWriter(null);
        }

        #region Output

        private TextBoxWriter m_Output;
        public TextBoxWriter Output
        {
            get
            {
                return m_Output;
            }
            set
            {
                m_Output = value;
                OnPropertyChanged(nameof(Output));
            }
        }

        #endregion

        #region Error

        private TextBoxWriter m_Error;
        public TextBoxWriter Error
        {
            get
            {
                return m_Error;
            }
            set
            {
                m_Error = value;
                OnPropertyChanged(nameof(Error));
            }
        }

        #endregion

        #region Input

        private TextBoxReader m_Input;
        public TextBoxReader Input
        {
            get
            {
                return m_Input;
            }
            set
            {
                m_Input = value;
                OnPropertyChanged(nameof(Input));
            }
        }

        #endregion
    }
}
