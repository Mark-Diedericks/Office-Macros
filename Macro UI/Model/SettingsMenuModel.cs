/*
 * Mark Diedericks
 * 02/08/2018
 * Version 1.0.3
 * Settings menu model
 */

using Macro_Engine;
using Macro_UI.View;
using Macro_UI.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Macro_UI.Model
{
    /// <summary>
    /// Enum identifying the a settings menu page
    /// </summary>
    internal enum SettingsMenuPage
    {
        Style = 0,
        Library = 1,
        Ribbon = 2
    }

    public class SettingsMenuModel : Base.Model
    {
        private static SettingsMenuModel s_Instance;
        private SettingsMenuPage m_SettingsPage;

        /// <summary>
        /// Instantiation of SettingsMenuModel
        /// </summary>
        public SettingsMenuModel()
        {
            s_Instance = this;
            IsOpen = false;
            FunctionColor = "";
            DigitColor = "";
            CommentColor = "";
            StringColor = "";
            PairColor = "";
            ClassColor = "";
            StatementColor = "";
            BooleanColor = "";
            RibbonItems = new ObservableCollection<DisplayableTreeViewItem>();
            LibraryItems = new ObservableCollection<DisplayableListViewItem>();
            m_SettingsPage = SettingsMenuPage.Style;
        }

        /// <summary>
        /// Gets the instance of SettingsMenuModel
        /// </summary>
        /// <returns>SettingsMenuModel instance</returns>
        public SettingsMenuModel GetInstance()
        {
            return s_Instance;
        }

        /// <summary>
        /// Sets the RibbonItems collection
        /// </summary>
        /// <param name="items">Observable heirarchial collection</param>
        public static void SetRibbonItems(ObservableCollection<DisplayableTreeViewItem> items)
        {
            if (s_Instance == null)
                return;

            s_Instance.RibbonItems = items;
        }

        #region IsOpen

        private bool m_IsOpen;
        public bool IsOpen
        {
            get
            {
                return m_IsOpen;
            }
            set
            {
                if(m_IsOpen != value)
                {
                    m_IsOpen = value;
                    OnPropertyChanged(nameof(IsOpen));
                }
            }
        }

        #endregion

        #region FunctionColor

        private string m_FunctionColor;
        public string FunctionColor
        {
            get
            {
                return m_FunctionColor;
            }

            set
            {
                if (m_FunctionColor != value)
                {
                    m_FunctionColor = value;
                    OnPropertyChanged(nameof(FunctionColor));
                }
            }
        }

        #endregion

        #region DigitColor

        private string m_DigitColor;
        public string DigitColor
        {
            get
            {
                return m_DigitColor;
            }

            set
            {
                if (m_DigitColor != value)
                {
                    m_DigitColor = value;
                    OnPropertyChanged(nameof(DigitColor));
                }
            }
        }

        #endregion

        #region CommentColor

        private string m_CommentColor;
        public string CommentColor
        {
            get
            {
                return m_CommentColor;
            }

            set
            {
                if (m_CommentColor != value)
                {
                    m_CommentColor = value;
                    OnPropertyChanged(nameof(CommentColor));
                }
            }
        }

        #endregion

        #region StringColor

        private string m_StringColor;
        public string StringColor
        {
            get
            {
                return m_StringColor;
            }

            set
            {
                if (m_StringColor != value)
                {
                    m_StringColor = value;
                    OnPropertyChanged(nameof(StringColor));
                }
            }
        }

        #endregion

        #region PairColor

        private string m_PairColor;
        public string PairColor
        {
            get
            {
                return m_PairColor;
            }

            set
            {
                if (m_PairColor != value)
                {
                    m_PairColor = value;
                    OnPropertyChanged(nameof(PairColor));
                }
            }
        }

        #endregion

        #region ClassColor

        private string m_ClassColor;
        public string ClassColor
        {
            get
            {
                return m_ClassColor;
            }

            set
            {
                if (m_ClassColor != value)
                {
                    m_ClassColor = value;
                    OnPropertyChanged(nameof(ClassColor));
                }
            }
        }

        #endregion

        #region StatementColor

        private string m_StatementColor;
        public string StatementColor
        {
            get
            {
                return m_StatementColor;
            }

            set
            {
                if (m_StatementColor != value)
                {
                    m_StatementColor = value;
                }
            }
        }

        #endregion

        #region BooleanColor

        private string m_BooleanColor;
        public string BooleanColor
        {
            get
            {
                return m_BooleanColor;
            }

            set
            {
                if (m_BooleanColor != value)
                {
                    m_BooleanColor = value;
                    OnPropertyChanged(nameof(BooleanColor));
                }
            }
        }

        #endregion

        #region RibbonItems

        private ObservableCollection<DisplayableTreeViewItem> m_RibbonItems;
        public ObservableCollection<DisplayableTreeViewItem> RibbonItems
        {
            get
            {
                return m_RibbonItems;
            }
            set
            {
                if (m_RibbonItems != value)
                {
                    m_RibbonItems = value;
                    OnPropertyChanged(nameof(RibbonItems));
                    OnPropertyChanged(nameof(LabelVisible));
                }
            }
        }

        #endregion

        #region LibraryItems

        private ObservableCollection<DisplayableListViewItem> m_LibraryItems;
        public ObservableCollection<DisplayableListViewItem> LibraryItems
        {
            get
            {
                return m_LibraryItems;
            }
            set
            {
                if (m_LibraryItems != value)
                {
                    m_LibraryItems = value;
                    OnPropertyChanged(nameof(LibraryItems));
                    OnPropertyChanged(nameof(LabelVisible));
                }
            }
        }

        #endregion

        #region SelectedLibrary

        private DisplayableListViewItem m_SelectedLibrary;
        public DisplayableListViewItem SelectedLibrary
        {
            get
            {
                return m_SelectedLibrary;
            }
            set
            {
                if(m_SelectedLibrary != value)
                {
                    m_SelectedLibrary = value;
                    OnPropertyChanged(nameof(SelectedLibrary));
                }
            }
        }

        #endregion

        #region LabelVisible

        public bool LabelVisible
        {
            get
            {
                return RibbonActive ? RibbonItems.Count <= 0 : LibraryItems.Count <= 0;
            }
        }

        #endregion

        #region LightTheme

        private bool m_LightTheme;
        public bool LightTheme
        {
            get
            {
                return m_LightTheme;
            }
            set
            {
                if(m_LightTheme != value)
                {
                    if(value)
                        MainWindowViewModel.GetInstance().SetTheme("Light");

                    m_LightTheme = value;
                    OnPropertyChanged(nameof(LightTheme));
                    OnPropertyChanged(nameof(DarkTheme));
                }
            }
        }
        
        #endregion

        #region DarkTheme

        public bool DarkTheme
        {
            get
            {
                return !m_LightTheme;
            }
            set
            {
                if (m_LightTheme == value)
                {
                    if(value)
                        MainWindowViewModel.GetInstance().SetTheme("Dark");

                    LightTheme = !value;
                }
            }
        }

        #endregion

        #region StyleActive

        public bool StyleActive
        {
            get
            {
                return m_SettingsPage == SettingsMenuPage.Style;
            }
            set
            {
                if(m_SettingsPage != SettingsMenuPage.Style && value)
                {
                    m_SettingsPage = SettingsMenuPage.Style;
                    OnPropertyChanged(nameof(StyleActive));
                    OnPropertyChanged(nameof(LibraryActive));
                    OnPropertyChanged(nameof(RibbonActive));
                }
            }
        }

        #endregion

        #region LibraryActive
        
        public bool LibraryActive
        {
            get
            {
                return m_SettingsPage == SettingsMenuPage.Library;
            }
            set
            {
                if (m_SettingsPage != SettingsMenuPage.Library && value)
                {
                    m_SettingsPage = SettingsMenuPage.Library;
                    OnPropertyChanged(nameof(StyleActive));
                    OnPropertyChanged(nameof(LibraryActive));
                    OnPropertyChanged(nameof(RibbonActive));
                    OnPropertyChanged(nameof(LabelVisible));
                }
            }
        }

        #endregion

        #region RibbonActive
        
        public bool RibbonActive
        {
            get
            {
                return m_SettingsPage == SettingsMenuPage.Ribbon;
            }
            set
            {
                if (m_SettingsPage != SettingsMenuPage.Ribbon && value)
                {
                    m_SettingsPage = SettingsMenuPage.Ribbon;
                    OnPropertyChanged(nameof(StyleActive));
                    OnPropertyChanged(nameof(LibraryActive));
                    OnPropertyChanged(nameof(RibbonActive));
                    OnPropertyChanged(nameof(LabelVisible));
                }
            }
        }

        #endregion
    }
}
