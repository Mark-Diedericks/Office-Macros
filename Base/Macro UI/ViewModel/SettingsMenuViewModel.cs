/*
 * Mark Diedericks
 * 02/08/2018
 * Version 1.0.3
 * Settings menu view model
 */

using Macro_Engine;
using Macro_Engine.Interop;
using Macro_UI.Model;
using Macro_UI.Routing;
using Macro_UI.Utilities;
using Macro_UI.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using static Macro_UI.Utilities.SyntaxStyleLoader;

namespace Macro_UI.ViewModel
{
    public class SettingsMenuViewModel : Base.ViewModel
    {
        private string PreviousTheme;
        private static SettingsMenuViewModel s_Instance;

        /// <summary>
        /// Instantiation of SettingsMenuViewModel
        /// </summary>
        public SettingsMenuViewModel()
        {
            s_Instance = this;
            Model = new SettingsMenuModel();

            Events.SubscribeEvent("ThemeChanged", (Action)ThemeChanged);
            //Routing.EventManager.ThemeChangedEvent += ThemeChanged;

            PreviousTheme = MainWindowViewModel.GetInstance().ActiveTheme.Name;
            LoadColors();

            //Events.OnAssembliesChanged += LoadAssemblies;
            Events.SubscribeEvent("OnAssembliesChanged", (Action)LoadAssemblies);

            LoadAssemblies();
        }

        /// <summary>
        /// Produces a collection of displable instances of assemblies
        /// </summary>
        private void LoadAssemblies()
        {
            LibraryItems.Clear();

            foreach (AssemblyDeclaration ad in MacroUI.GetInstance().GetAssemblies())
                LibraryItems.Add(new DisplayableListViewItem(ad));

            OnPropertyChanged(nameof(LabelVisible));
        }

        /// <summary>
        /// ThemeChanged event callback, updates the theme settings
        /// </summary>
        private void ThemeChanged()
        {
            //SaveSyntaxStyle(PreviousTheme == "Dark");
            PreviousTheme = MainWindowViewModel.GetInstance().ActiveTheme.Name;
            LoadColors();
        }

        /// <summary>
        /// Loads the colors of the syntax style;
        /// </summary>
        private void LoadColors()
        {
            LoadColorValues();
            SyntaxStyle = GetValues();

            /*FunctionColor = values[(int)SyntaxStyleColor.FUNCTION];
            DigitColor = values[(int)SyntaxStyleColor.DIGIT];
            CommentColor = values[(int)SyntaxStyleColor.COMMENT];
            StringColor = values[(int)SyntaxStyleColor.STRING];
            PairColor = values[(int)SyntaxStyleColor.PAIR];
            ClassColor = values[(int)SyntaxStyleColor.CLASS];
            StatementColor = values[(int)SyntaxStyleColor.STATEMENT];
            BooleanColor = values[(int)SyntaxStyleColor.BOOLEAN];*/
        }

        #region Model

        private SettingsMenuModel m_Model;
        public SettingsMenuModel Model
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

        #region AddLibraryCommand

        private ICommand m_AddLibraryCommand;
        public ICommand AddLibraryCommand
        {
            get
            {
                if (m_AddLibraryCommand == null)
                    m_AddLibraryCommand = new RelayCommand(call => AddLibrary());
                return m_AddLibraryCommand;
            }
        }

        /// <summary>
        /// Adds an assembly to the registry
        /// </summary>
        private void AddLibrary()
        {
            string path = Files.ImportAssembly();

            if (string.IsNullOrEmpty(path))
                return;

            string name = System.Reflection.Assembly.LoadFrom(path).FullName;

            AssemblyDeclaration ad = new AssemblyDeclaration(name, path, false);

            MacroUI.GetInstance().AddAssembly(ad);
        }

        #endregion

        #region RemoveLibraryCommand

        private ICommand m_RemoveLibraryCommand;
        public ICommand RemoveLibraryCommand
        {
            get
            {
                if (m_RemoveLibraryCommand == null)
                    m_RemoveLibraryCommand = new RelayCommand(call => RemoveLibrary());
                return m_RemoveLibraryCommand;
            }
        }

        /// <summary>
        /// Removes an assembly from the registry
        /// </summary>
        private void RemoveLibrary()
        {
            MacroUI.GetInstance().RemoveAssembly(SelectedLibrary.declaration);
        }

        #endregion

        #region IsOpen

        public bool IsOpen
        {
            get
            {
                return Model.IsOpen;
            }
            set
            {
                if(Model.IsOpen != value)
                {
                    Model.IsOpen = value;
                    OnPropertyChanged(nameof(IsOpen));
                }
            }
        }

        #endregion

        #region SyntaxStyle

        public SyntaxStyleValues SyntaxStyle
        {
            get
            {
                return Model.SyntaxStyle;
            }

            set
            {
                if (Model.SyntaxStyle != value)
                {
                    Model.SyntaxStyle = value;
                    OnPropertyChanged(nameof(SyntaxStyle));

                    SetSyntaxStyle(Model.SyntaxStyle);
                }
            }
        }

        #endregion

        #region RibbonItems

        public ObservableCollection<DisplayableTreeViewItem> RibbonItems
        {
            get
            {
                return Model.RibbonItems;
            }
            set
            {
                if (Model.RibbonItems != value)
                {
                    Model.RibbonItems = value;
                    OnPropertyChanged(nameof(RibbonItems));
                    OnPropertyChanged(nameof(LabelVisible));
                }
            }
        }

        #endregion

        #region LibraryItems

        public ObservableCollection<DisplayableListViewItem> LibraryItems
        {
            get
            {
                return Model.LibraryItems;
            }
            set
            {
                if (Model.LibraryItems != value)
                {
                    Model.LibraryItems = value;
                    OnPropertyChanged(nameof(LibraryItems));
                    OnPropertyChanged(nameof(LabelVisible));
                }
            }
        }

        #endregion

        #region LabelVisible

        public bool LabelVisible
        {
            get
            {
                return Model.LabelVisible;
            }
        }

        #endregion

        #region SelectedLibrary

        public DisplayableListViewItem SelectedLibrary
        {
            get
            {
                return Model.SelectedLibrary;
            }
            set
            {
                if(Model.SelectedLibrary != value)
                {
                    Model.SelectedLibrary = value;
                    OnPropertyChanged(nameof(SelectedLibrary));
                }
            }
        }

        #endregion

        #region LightTheme

        public bool LightTheme
        {
            get
            {
                return Model.LightTheme;
            }
            set
            {
                if (Model.LightTheme != value)
                {
                    Model.LightTheme = value;
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
                return Model.DarkTheme;
            }
            set
            {
                if (Model.DarkTheme != value)
                {
                    Model.DarkTheme = value;
                    OnPropertyChanged(nameof(LightTheme));
                    OnPropertyChanged(nameof(DarkTheme));
                }
            }
        }

        #endregion

        #region AppStyleActive

        public bool AppStyleActive
        {
            get
            {
                return Model.AppStyleActive;
            }
            set
            {
                if (Model.AppStyleActive != value)
                {
                    Model.AppStyleActive = value;
                    OnPropertyChanged(nameof(AppStyleActive));
                    OnPropertyChanged(nameof(EnvironmentActive));
                    OnPropertyChanged(nameof(MacrosActive));
                }
            }
        }

        #endregion

        #region LibraryActive

        public bool EnvironmentActive
        {
            get
            {
                return Model.EnvironmentActive;
            }
            set
            {
                if (Model.EnvironmentActive != value)
                {
                    Model.EnvironmentActive = value;
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
                return Model.MacrosActive;
            }
            set
            {
                if (Model.MacrosActive != value)
                {
                    Model.MacrosActive = value;
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
