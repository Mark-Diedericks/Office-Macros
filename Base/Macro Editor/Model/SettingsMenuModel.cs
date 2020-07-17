/*
 * Mark Diedericks
 * 02/08/2018
 * Version 1.0.3
 * Settings menu model
 */

using Macro_Engine;
using Macro_Editor.Utilities;
using Macro_Editor.View;
using Macro_Editor.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Macro_Editor.Utilities.SyntaxStyleLoader;

namespace Macro_Editor.Model
{
    /// <summary>
    /// Enum identifying the a settings menu page
    /// </summary>
    internal enum SettingsMenuPage
    {
        AppStyle = 0,
        Environment = 1,
        Macros = 2
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
            SyntaxStyle = new SyntaxStyleValues();
            RibbonItems = new ObservableCollection<DisplayableTreeViewItem>();
            LibraryItems = new ObservableCollection<DisplayableListViewItem>();
            m_SettingsPage = SettingsMenuPage.AppStyle;
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

        #region SyntaxStyle

        private SyntaxStyleValues m_SyntaxStyle;
        public SyntaxStyleValues SyntaxStyle
        {
            get
            {
                return m_SyntaxStyle;
            }

            set
            {
                if (m_SyntaxStyle != value)
                {
                    m_SyntaxStyle = value;
                    OnPropertyChanged(nameof(SyntaxStyle));
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
                return MacrosActive ? RibbonItems.Count <= 0 : LibraryItems.Count <= 0;
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

        #region AppStyleActive

        public bool AppStyleActive
        {
            get
            {
                return m_SettingsPage == SettingsMenuPage.AppStyle;
            }
            set
            {
                if(m_SettingsPage != SettingsMenuPage.AppStyle && value)
                {
                    m_SettingsPage = SettingsMenuPage.AppStyle;
                    OnPropertyChanged(nameof(AppStyleActive));
                    OnPropertyChanged(nameof(EnvironmentActive));
                    OnPropertyChanged(nameof(MacrosActive));
                }
            }
        }

        #endregion

        #region EnvironmentActive

        public bool EnvironmentActive
        {
            get
            {
                return m_SettingsPage == SettingsMenuPage.Environment;
            }
            set
            {
                if (m_SettingsPage != SettingsMenuPage.Environment && value)
                {
                    m_SettingsPage = SettingsMenuPage.Environment;
                    OnPropertyChanged(nameof(AppStyleActive));
                    OnPropertyChanged(nameof(EnvironmentActive));
                    OnPropertyChanged(nameof(MacrosActive));
                    OnPropertyChanged(nameof(LabelVisible));
                }
            }
        }

        #endregion

        #region MacrosActive

        public bool MacrosActive
        {
            get
            {
                return m_SettingsPage == SettingsMenuPage.Macros;
            }
            set
            {
                if (m_SettingsPage != SettingsMenuPage.Macros && value)
                {
                    m_SettingsPage = SettingsMenuPage.Macros;
                    OnPropertyChanged(nameof(AppStyleActive));
                    OnPropertyChanged(nameof(EnvironmentActive));
                    OnPropertyChanged(nameof(MacrosActive));
                    OnPropertyChanged(nameof(LabelVisible));
                }
            }
        }

        #endregion
    }
}
