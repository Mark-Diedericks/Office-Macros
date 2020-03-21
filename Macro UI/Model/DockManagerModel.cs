/*
 * Mark Diedericks
 * 31/07/2018
 * Version 1.0.1
 * Handles the data of the dock view model
 */

using Macro_UI.ViewModel;
using Macro_UI.ViewModel.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Macro_UI.Model
{
    public class DockManagerModel : Base.Model
    {
        /// <summary>
        /// Instantiation of DockManagerModel
        /// </summary>
        public DockManagerModel()
        {
            Documents = new ObservableCollection<DocumentViewModel>();
            Tools = new ObservableCollection<ToolViewModel>();
        }

        #region Documents

        private ObservableCollection<DocumentViewModel> m_Documents;
        public ObservableCollection<DocumentViewModel> Documents
        {
            get
            {
                return m_Documents;
            }
            set
            {
                if(m_Documents != value)
                {
                    m_Documents = value;
                    OnPropertyChanged(nameof(Documents));
                }
            }
        }

        #endregion
        
        #region Tools

        private ObservableCollection<ToolViewModel> m_Tools;
        public ObservableCollection<ToolViewModel> Tools
        {
            get
            {
                return m_Tools;
            }
            set
            {
                if (m_Tools != value)
                {
                    m_Tools = value;
                    OnPropertyChanged(nameof(Tools));
                }
            }
        }

        #endregion

        #region ActiveDocument

        private DocumentViewModel m_ActiveDocument;
        public DocumentViewModel ActiveDocument
        {
            get
            {
                return m_ActiveDocument;
            }
            set
            {
                if (m_ActiveDocument != value)
                {
                    m_ActiveDocument = value;
                    OnPropertyChanged(nameof(ActiveDocument));
                }
            }
        }

        #endregion

        #region ActiveContent

        private object m_ActiveContent;
        public object ActiveContent
        {
            get
            {
                return m_ActiveContent;
            }
            set
            {
                if (m_ActiveContent != value)
                {
                    m_ActiveContent = value;
                    OnPropertyChanged(nameof(ActiveContent));
                }
            }
        }

        #endregion

        #region Explorer

        private ExplorerViewModel m_Explorer;
        public ExplorerViewModel Explorer
        {
            get
            {
                return m_Explorer;
            }
            set
            {
                if(m_Explorer != value)
                {
                    m_Explorer = value;
                    OnPropertyChanged(nameof(Explorer));
                }
            }
        }

        #endregion

        #region Console

        private ConsoleViewModel m_Console;
        public ConsoleViewModel Console
        {
            get
            {
                return m_Console;
            }
            set
            {
                if(m_Console != value)
                {
                    m_Console = value;
                    OnPropertyChanged(nameof(Console));
                }
            }
        }

        #endregion
    }
}
