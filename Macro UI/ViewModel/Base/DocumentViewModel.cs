/*
 * Mark Diedericks
 * 30/07/2018
 * Version 1.0.9
 * Document window view model
 */

using Macro_UI.Model;
using Macro_UI.Model.Base;
using Macro_UI.Routing;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace Macro_UI.ViewModel.Base
{
    public class DocumentViewModel : ViewModel
    {

        #region Close Command

        private ICommand m_CloseCommand;
        public ICommand CloseCommand
        {
            get
            {
                if (m_CloseCommand == null)
                    m_CloseCommand = new RelayCommand(call => Close());
                return m_CloseCommand;
            }
        }

        /// <summary>
        /// Closes the current document, prompts if unsaved
        /// </summary>
        public void Close()
        {
            if (!IsSaved)
            {
                MacroUI.GetInstance().DisplayYesNoCancelMessage(Title + " has unsaved changed. Do you want to save the changes?", "Confirmation", "Discard", new Action<MessageDialogResult>((result) =>
                {
                    if (result == MessageDialogResult.Affirmative)
                        SaveCommand.Execute(new Action(() => IsClosed = true));
                    else if (result == MessageDialogResult.FirstAuxiliary)
                        IsClosed = true;
                    else
                        IsClosed = false;
                }));
            }
            else
            {
                IsClosed = true;
            }
        }

        #endregion

        #region Save Command

        private ICommand m_SaveCommand;
        public ICommand SaveCommand
        {
            get
            {
                if (m_SaveCommand == null)
                    m_SaveCommand = new RelayCommand((OnComplete) => Save((Action)OnComplete));
                return m_SaveCommand;
            }
        }

        /// <summary>
        /// Saves the current document
        /// </summary>
        /// <param name="OnComplete"></param>
        public virtual void Save(Action OnComplete)
        {
            IsSaved = true;
            OnComplete?.Invoke();
        }

        #endregion

        #region Start Command

        private ICommand m_StartCommand;
        public ICommand StartCommand
        {
            get
            {
                if (m_StartCommand == null)
                    m_StartCommand = new RelayCommand((OnComplete) => Start((Action)OnComplete));
                return m_StartCommand;
            }
        }

        public virtual void Start(Action OnComplete)
        {
            OnComplete?.Invoke();
        }

        #endregion

        #region Stop Command

        private ICommand m_StopCommand;
        public ICommand StopCommand
        {
            get
            {
                if (m_StopCommand == null)
                    m_StopCommand = new RelayCommand((OnComplete) => Stop((Action)OnComplete));
                return m_StopCommand;
            }
        }

        public virtual void Stop(Action OnComplete)
        {
            OnComplete?.Invoke();
        }

        #endregion

        #region Undo Command

        private ICommand m_UndoCommand;
        public ICommand UndoCommand
        {
            get
            {
                return m_UndoCommand;
            }
            set
            {
                m_UndoCommand = value;
            }
        }

        #endregion

        #region Redo Command

        private ICommand m_RedoCommand;
        public ICommand RedoCommand
        {
            get
            {
                return m_RedoCommand;
            }
            set
            {
                m_RedoCommand = value;
            }
        }

        #endregion

        #region Copy Command

        private ICommand m_CopyCommand;
        public ICommand CopyCommand
        {
            get
            {
                return m_CopyCommand;
            }
            set
            {
                m_CopyCommand = value;
            }
        }

        #endregion

        #region Cut Command

        private ICommand m_CutCommand;
        public ICommand CutCommand
        {
            get
            {
                return m_CutCommand;
            }
            set
            {
                m_CutCommand = value;
            }
        }

        #endregion

        #region Paste Command

        private ICommand m_PasteCommand;
        public ICommand PasteCommand
        {
            get
            {
                return m_PasteCommand;
            }
            set
            {
                m_PasteCommand = value;
            }
        }

        #endregion

        /// <summary>
        /// Instantiate DocumentViewModel
        /// </summary>
        public DocumentViewModel()
        {
            CanClose = true;
            CanFloat = true;
        }

        /// <summary>
        /// Creates DocumentViewModel from DocumentModel
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static DocumentViewModel Create(DocumentModel model)
        {
            if(model is TextualEditorModel)
                return new TextualEditorViewModel() { Model = (TextualEditorModel)model };

            return new DocumentViewModel() { Model = model };
        }

        #region Model

        private DocumentModel m_Model;
        public DocumentModel Model
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

        #region CanClose

        private bool m_CanClose;
        public bool CanClose
        {
            get
            {
                return m_CanClose;
            }

            set
            {
                if (m_CanClose != value)
                {
                    m_CanClose = value;
                    OnPropertyChanged(nameof(CanClose));
                }
            }
        }

        #endregion

        #region CanFloat

        private bool m_CanFloat;
        public bool CanFloat
        {
            get
            {
                return m_CanFloat;
            }

            set
            {
                if (m_CanFloat != value)
                {
                    m_CanFloat = value;
                    OnPropertyChanged(nameof(CanFloat));
                }
            }
        }

        #endregion

        #region IsClosed
        
        public bool IsClosed
        {
            get
            {
                return Model.IsClosed;
            }

            set
            {
                if (Model.IsClosed != value)
                {
                    Model.IsClosed = value;
                    OnPropertyChanged(nameof(IsClosed));
                }
            }
        }

        #endregion

        #region IsSaved

        public bool IsSaved
        {
            get
            {
                return Model.IsSaved;
            }

            set
            {
                if (Model.IsSaved != value)
                {
                    Model.IsSaved = value;
                    OnPropertyChanged(nameof(IsSaved));
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
                if(Model.Title != value)
                {
                    Model.Title = value;
                    OnPropertyChanged(nameof(Title));
                }
            }
        }

        #endregion

        #region ToolTip

        public string ToolTip
        {
            get
            {
                return Model.ToolTip;
            }

            set
            {
                if (Model.ToolTip != value)
                {
                    Model.ToolTip = value;
                    OnPropertyChanged(nameof(ToolTip));
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

        #region Macro

        public Guid Macro
        {
            get
            {
                return Model.Macro;
            }

            set
            {
                if (Model.Macro != value)
                {
                    Model.Macro = value;
                    OnPropertyChanged(nameof(Macro));
                }
            }
        }

        #endregion
    }
}
