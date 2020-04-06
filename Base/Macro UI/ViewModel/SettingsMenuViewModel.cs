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
            SaveSyntaxStyle(PreviousTheme == "Dark");
            PreviousTheme = MainWindowViewModel.GetInstance().ActiveTheme.Name;
            LoadColors();
        }

        /// <summary>
        /// Loads the colors of the syntax style;
        /// </summary>
        private void LoadColors()
        {
            LoadColorValues();
            string[] values = GetValues();

            FunctionColor = values[(int)SyntaxStyleColor.FUNCTION];
            DigitColor = values[(int)SyntaxStyleColor.DIGIT];
            CommentColor = values[(int)SyntaxStyleColor.COMMENT];
            StringColor = values[(int)SyntaxStyleColor.STRING];
            PairColor = values[(int)SyntaxStyleColor.PAIR];
            ClassColor = values[(int)SyntaxStyleColor.CLASS];
            StatementColor = values[(int)SyntaxStyleColor.STATEMENT];
            BooleanColor = values[(int)SyntaxStyleColor.BOOLEAN];
        }

        /// <summary>
        /// Sets the colors of the syntax style
        /// </summary>
        private void SetColors()
        {
            string[] values = new string[8];

            values[(int)SyntaxStyleColor.FUNCTION] = FunctionColor;
            values[(int)SyntaxStyleColor.DIGIT] = DigitColor;
            values[(int)SyntaxStyleColor.COMMENT] = CommentColor;
            values[(int)SyntaxStyleColor.STRING] = StringColor;
            values[(int)SyntaxStyleColor.PAIR] = PairColor;
            values[(int)SyntaxStyleColor.CLASS] = ClassColor;
            values[(int)SyntaxStyleColor.STATEMENT] = StatementColor;
            values[(int)SyntaxStyleColor.BOOLEAN] = BooleanColor;

            SetSyntaxStyle(values);
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
            Events.InvokeEvent("ImportAssembly", new Action<string>((path) => {
                if (path == String.Empty)
                    return;

                string name = System.Reflection.Assembly.LoadFrom(path).FullName;

                AssemblyDeclaration ad = new AssemblyDeclaration(name, path, false);

                MacroUI.GetInstance().AddAssembly(ad);
            }));
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

        #region FunctionColor

        public string FunctionColor
        {
            get
            {
                return Model.FunctionColor;
            }

            set
            {
                if (Model.FunctionColor != value)
                {
                    Model.FunctionColor = value;
                    OnPropertyChanged(nameof(FunctionColor));
                    SetColors();
                }
            }
        }

        #endregion

        #region DigitColor
        
        public string DigitColor
        {
            get
            {
                return Model.DigitColor;
            }

            set
            {
                if (Model.DigitColor != value)
                {
                    Model.DigitColor = value;
                    OnPropertyChanged(nameof(DigitColor));
                    SetColors();
                }
            }
        }

        #endregion

        #region CommentColor
        
        public string CommentColor
        {
            get
            {
                return Model.CommentColor;
            }

            set
            {
                if (Model.CommentColor != value)
                {
                    Model.CommentColor = value;
                    OnPropertyChanged(nameof(CommentColor));
                    SetColors();
                }
            }
        }

        #endregion

        #region StringColor
        
        public string StringColor
        {
            get
            {
                return Model.StringColor;
            }

            set
            {
                if (Model.StringColor != value)
                {
                    Model.StringColor = value;
                    OnPropertyChanged(nameof(StringColor));
                    SetColors();
                }
            }
        }

        #endregion

        #region PairColor
        
        public string PairColor
        {
            get
            {
                return Model.PairColor;
            }

            set
            {
                if (Model.PairColor != value)
                {
                    Model.PairColor = value;
                    OnPropertyChanged(nameof(PairColor));
                    SetColors();
                }
            }
        }

        #endregion

        #region ClassColor
        
        public string ClassColor
        {
            get
            {
                return Model.ClassColor;
            }

            set
            {
                if (Model.ClassColor != value)
                {
                    Model.ClassColor = value;
                    OnPropertyChanged(nameof(ClassColor));
                    SetColors();
                }
            }
        }

        #endregion

        #region StatementColor
        
        public string StatementColor
        {
            get
            {
                return Model.StatementColor;
            }

            set
            {
                if (Model.StatementColor != value)
                {
                    Model.StatementColor = value;
                    OnPropertyChanged(nameof(StatementColor));
                    SetColors();
                }
            }
        }

        #endregion

        #region BooleanColor
        
        public string BooleanColor
        {
            get
            {
                return Model.BooleanColor;
            }

            set
            {
                if (Model.BooleanColor != value)
                {
                    Model.BooleanColor = value;
                    OnPropertyChanged(nameof(BooleanColor));
                    SetColors();
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

        #region StyleActive

        public bool StyleActive
        {
            get
            {
                return Model.StyleActive;
            }
            set
            {
                if (Model.StyleActive != value)
                {
                    Model.StyleActive = value;
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
                return Model.LibraryActive;
            }
            set
            {
                if (Model.LibraryActive != value)
                {
                    Model.LibraryActive = value;
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
                return Model.RibbonActive;
            }
            set
            {
                if (Model.RibbonActive != value)
                {
                    Model.RibbonActive = value;
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
