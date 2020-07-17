/*
 * Mark Diedericks
 * 30/07/2018
 * Version 1.0.7
 * Tool window view model
 */

using Macro_Editor.Model.Base;
using Macro_Editor.Routing;
using Macro_Editor.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace Macro_Editor.ViewModel.Base
{
    public class ToolViewModel : ViewModel
    {
        /// <summary>
        /// Instantiate ToolViewModel
        /// </summary>
        public ToolViewModel()
        {

        }

        #region Model

        private ToolModel m_Model;
        public ToolModel Model
        {
            get
            {
                return m_Model;
            }

            set
            {
                if (m_Model != value)
                {
                    m_Model = value;
                    OnPropertyChanged(nameof(Model));
                }
            }
        }

        #endregion

        #region IsVisible

        public bool IsVisible
        {
            get
            {
                return Model.IsVisible;
            }

            set
            {
                if (Model.IsVisible != value)
                {
                    Model.IsVisible = value;
                    OnPropertyChanged(nameof(IsVisible));
                }
            }
        }

        #endregion

        #region Title

        public string Title
        {
            get
            {
                return Model.Title;
            }

            set
            {
                if (Model.Title != value)
                {
                    Model.Title = value;
                    OnPropertyChanged(nameof(Title));
                }
            }
        }

        #endregion

        #region ContentId
        
        public string ContentId
        {
            get
            {
                return Model.ContentId;
            }

            set
            {
                if (Model.ContentId != value)
                {
                    Model.ContentId = value;
                    OnPropertyChanged(nameof(ContentId));
                }
            }
        }

        #endregion

    }
}
