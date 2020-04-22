/*
 * Mark Diedericks
 * 30/07/2018
 * Version 1.0.6
 * TextualEditor mdeol
 */

using Macro_Engine;
using Macro_Engine.Macros;
using Macro_UI.Model.Base;
using ICSharpCode.AvalonEdit.Document;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Macro_UI.Model
{
    public class TextualEditorModel : DocumentModel
    {
        /// <summary>
        /// Instantiation of TextualEditorModel
        /// </summary>
        /// <param name="id">The id of the editor's macro</param>
        public TextualEditorModel(FileDeclaration d)
        {
            if (d != null)
            {
                Title = d.Info.Name;
                ToolTip = d.Info.Name;
                ContentId = d.Info.FullName;
                Declaration = d;
                IsClosed = false;
                Source = new TextDocument(d.Content);
                IsSaved = true;
                return;
            }

            Source = new TextDocument();
            IsSaved = false;
        }

        #region Source

        private TextDocument m_Source;
        public TextDocument Source
        {
            get
            {
                return m_Source;
            }

            set
            {
                if (m_Source != value)
                {
                    m_Source = value;
                    m_Source.TextChanged += (s, e) => { IsSaved = false; };
                    OnPropertyChanged(nameof(Source));
                    IsSaved = false;
                }
            }
        }

        #endregion
    }
}
