/*
 * Mark Diedericks
 * 30/07/2018
 * Version 1.0.3
 * Base tool model
 */
 
using Macro_Editor.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Macro_Editor.Model.Base
{
    public class ToolModel : Model
    {
        public ToolModel()
        {
            IsVisible = true;
            Title = "";
            ContentId = "";
        }

        #region IsVisible

        private bool m_IsVisible;
        public bool IsVisible
        {
            get
            {
                return m_IsVisible;
            }
            set
            {
                if (m_IsVisible != value)
                {
                    m_IsVisible = value;
                    OnPropertyChanged(nameof(IsVisible));
                }
            }
        }

        #endregion
        #region Title

        private string m_Title;
        public string Title
        {
            get
            {
                return m_Title;
            }

            set
            {
                if (m_Title != value)
                {
                    m_Title = value;
                    OnPropertyChanged(nameof(Title));
                }
            }
        }

        #endregion

        #region ContentId

        private string m_ContentId;
        public string ContentId
        {
            get
            {
                return m_ContentId;
            }

            set
            {
                if (m_ContentId != value)
                {
                    m_ContentId = value;
                    OnPropertyChanged(nameof(ContentId));
                }
            }
        }

        #endregion   

    }
}
