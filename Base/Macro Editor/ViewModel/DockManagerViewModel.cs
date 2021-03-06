﻿/*
 * Mark Diedericks
 * 02/08/2018
 * Version 1.0.5
 * Handles the interaction logic of the dock view
 */

using Macro_Engine;
using Macro_Engine.Macros;
using Macro_Editor.Model;
using Macro_Editor.Model.Base;
using Macro_Editor.ViewModel.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Macro_Editor.ViewModel
{
    public class DockManagerViewModel : Base.ViewModel
    {
        /// <summary>
        /// Instantiate DocumentViewModel
        /// </summary>
        /// <param name="DocumentViewModels">Documents</param>
        public DockManagerViewModel(IEnumerable<DocumentViewModel> DocumentViewModels)
        {
            Model = new DockManagerModel();

            Explorer = new ExplorerViewModel() { Model = new ExplorerModel() { Title = "Explorer", ContentId = "Explorer", IsVisible = true } };
            Console = new ConsoleViewModel() { Model = new ConsoleModel() { Title = "Console", ContentId = "Console", IsVisible = true } };

            Tools.Add(Explorer);
            Tools.Add(Console);

            foreach (DocumentViewModel document in DocumentViewModels)
            {
                document.PropertyChanged += Document_PropertyChanged;

                if (!document.IsClosed)
                    Documents.Add(document);
            }
        }

        /// <summary>
        /// Gets a list of all documents which are currently in an usaved state
        /// </summary>
        /// <returns>List of unsaved documents</returns>
        public List<DocumentViewModel> GetUnsavedDocuments()
        {
            List<DocumentViewModel> unsaved = new List<DocumentViewModel>();

            foreach (DocumentViewModel document in Documents)
                if (!document.IsSaved)
                    unsaved.Add(document);

            return unsaved;
        }

        /// <summary>
        /// Add document to the view
        /// </summary>
        /// <param name="document">The document to add</param>
        public void AddDocument(DocumentViewModel document)
        {
            document.PropertyChanged += Document_PropertyChanged;

            if (!document.IsClosed)
                Documents.Add(document);
        }

        /// <summary>
        /// Gets a document based on the macro id
        /// </summary>
        /// <param name="d">The file's declaration</param>
        /// <returns>DocumentViewModel of the macro</returns>
        public DocumentViewModel GetDocument(FileDeclaration d)
        {
            foreach (DocumentViewModel document in Documents)
                if (document.Declaration == d)
                    return document;

            return null;
        }

        /// <summary>
        /// Gets the FileDeclaration for the active document
        /// </summary>
        /// <returns>FileDeclaration of active document</returns>
        public FileDeclaration GetActiveDocumentDeclaration()
        {
            if (ActiveDocument == null)
                return null;

            return ActiveDocument.Declaration;
        }

        /// <summary>
        /// Instantiate DocumentViewModel
        /// </summary>
        /// <param name="docs">Serialized string of open documents</param>
        public DockManagerViewModel(string[] docs) : this(LoadVisibleDocuments(docs))
        {

        }

        /// <summary>
        /// PropertyChanged event callback, removes document if it has been closed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Document_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            DocumentViewModel document = (DocumentViewModel)sender;

            if(e.PropertyName == nameof(DocumentViewModel.IsClosed))
            {
                if (!document.IsClosed)
                    Documents.Add(document);
                else
                    Documents.Remove(document);
            }
        }

        /// <summary>
        /// Identifies documents that are open from a serialized list
        /// </summary>
        /// <param name="docs"></param>
        /// <returns>List of DocumentViewModels</returns>
        private static List<DocumentViewModel> LoadVisibleDocuments(string[] paths)
        {
            List<DocumentViewModel> documents = new List<DocumentViewModel>();

            if (paths == null)
                return documents;

            foreach (string s in paths)
            {
                FileDeclaration d = MacroUI.GetInstance().GetDeclarationFromFullname(s);
                if (d != null)
                {
                    DocumentModel model = DocumentModel.Create(d);

                    if (model != null)
                    {
                        DocumentViewModel viewModel = DocumentViewModel.Create(model);
                        documents.Add(viewModel);
                    }
                }
            }

            return documents;
        }

        /// <summary>
        /// Serializes a list of all open documents
        /// </summary>
        /// <returns>Serialized string list of documents</returns>
        public string[] GetVisibleDocuments()
        {
            List<string> visDocs = new List<string>();

            foreach (DocumentViewModel document in Documents)
                if (document.Model.Declaration != null)
                    visDocs.Add(document.Model.Declaration.Info.FullName);

            return visDocs.ToArray<string>();
        }

        #region Model

        private DockManagerModel m_Model;
        public DockManagerModel Model
        {
            get
            {
                return m_Model;
            }
            set
            {
                if(m_Model != value)
                {
                    m_Model = value;
                    OnPropertyChanged(nameof(Model));
                }
            }
        }

        #endregion

        #region Documents

        public ObservableCollection<DocumentViewModel> Documents
        {
            get
            {
                return Model.Documents;
            }
            set
            {
                if(Model.Documents != value)
                {
                    Model.Documents = value;
                    OnPropertyChanged(nameof(Documents));
                }
            }
        }

        #endregion

        #region Tools

        public ObservableCollection<ToolViewModel> Tools
        {
            get
            {
                return Model.Tools;
            }
            set
            {
                if (Model.Tools != value)
                {
                    Model.Tools = value;
                    OnPropertyChanged(nameof(Tools));
                }
            }
        }

        #endregion

        #region ActiveDocument

        public DocumentViewModel ActiveDocument
        {
            get
            {
                return Model.ActiveDocument;
            }
            set
            {
                if (Model.ActiveDocument != value)
                {
                    Model.ActiveDocument = value;
                    OnPropertyChanged(nameof(ActiveDocument));
                }
            }
        }

        #endregion

        #region ActiveContent

        public object ActiveContent
        {
            get
            {
                return Model.ActiveContent;
            }
            set
            {
                if (Model.ActiveContent != value)
                {
                    Model.ActiveContent = value;

                    if (ActiveContent is DocumentViewModel)
                        ActiveDocument = ActiveContent as DocumentViewModel;

                    OnPropertyChanged(nameof(ActiveContent));
                }
            }
        }

        #endregion

        #region Explorer

        public ExplorerViewModel Explorer
        {
            get
            {
                return Model.Explorer;
            }
            set
            {
                if (Model.Explorer != value)
                {
                    Model.Explorer = value;
                    OnPropertyChanged(nameof(Explorer));
                }
            }
        }

        #endregion

        #region Console

        public ConsoleViewModel Console
        {
            get
            {
                return Model.Console;
            }
            set
            {
                if (Model.Console != value)
                {
                    Model.Console = value;
                    OnPropertyChanged(nameof(Console));
                }
            }
        }

        #endregion
    }
}
