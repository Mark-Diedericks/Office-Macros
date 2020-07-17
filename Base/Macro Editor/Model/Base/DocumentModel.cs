/*
 * Mark Diedericks
 * 02/08/2018
 * Version 1.0.2
 * Base document model
 */
 
using Macro_Engine;
using Macro_Engine.Macros;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Macro_Editor.Model.Base
{
    public class DocumentModel : Model
    {
        public DocumentModel()
        {
            IsClosed = false;
            Title = "";
            ToolTip = "";
            ContentId = "";
            Declaration = null;
        }

        /// <summary>
        /// Identifies the type of macro and creates the respective document model
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static DocumentModel Create(FileDeclaration d)
        {
            if (d == null)
                return null;

            return new TextualEditorModel(d);
        }

        #region IsClosed

        private bool m_IsClosed;
        public bool IsClosed
        {
            get
            {
                return m_IsClosed;
            }

            set
            {
                if (m_IsClosed != value)
                {
                    m_IsClosed = value;
                    OnPropertyChanged(nameof(IsClosed));
                }
            }
        }

        #endregion

        #region IsSaved

        private bool m_IsSaved;
        public bool IsSaved
        {
            get
            {
                return m_IsSaved;
            }

            set
            {
                if (m_IsSaved != value)
                {
                    m_IsSaved = value;
                    OnPropertyChanged(nameof(IsSaved));
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

        #region ToolTip

        private string m_ToolTip;
        public string ToolTip
        {
            get
            {
                return m_ToolTip;
            }

            set
            {
                if (m_ToolTip != value)
                {
                    m_ToolTip = value;
                    OnPropertyChanged(nameof(ToolTip));
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

        #region Declaration

        private FileDeclaration m_Declaration;
        public FileDeclaration Declaration
        {
            get
            {
                return m_Declaration;
            }

            set
            {
                if (m_Declaration != value)
                {
                    m_Declaration = value;
                    OnPropertyChanged(nameof(Declaration));
                }
            }
        }

        #endregion

    }
}
