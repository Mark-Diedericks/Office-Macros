/*
 * Mark Diedericks
 * 02/08/2018
 * Version 1.0.4
 * Base item view models
 */

using Macro_Engine;
using Macro_Engine.Interop;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Macro_UI.ViewModel
{
    /// <summary>
    /// Custom Data Strcture for Displaying List View Items of aseemblies
    /// </summary>
    public class DisplayableListViewItem : Base.ViewModel
    {
        public DisplayableListViewItem(AssemblyDeclaration ad)
        {
            declaration = ad;
            Header = ad.displayname;
            ToolTip = ad.filepath;
            IsChecked = ad.enabled;
        }

        public AssemblyDeclaration declaration { get; internal set; }
        
        #region Header
        private string m_Header;
        public string Header
        {
            get
            {
                return m_Header;
            }
            set
            {
                if (m_Header != value)
                {
                    m_Header = value;
                    OnPropertyChanged(nameof(Header));
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
        #region IsChecked
        private bool m_IsChecked;
        public bool IsChecked
        {
            get
            {
                return m_IsChecked;
            }
            set
            {
                if (m_IsChecked != value)
                {
                    m_IsChecked = value;
                    declaration.enabled = value;
                    OnPropertyChanged(nameof(IsChecked));
                }
            }
        }
        #endregion
    }

    /// <summary>
    /// Custom Data Strcture for Displaying Tree View Items of macros
    /// </summary>
    public class DisplayableTreeViewItem : ViewModel.Base.ViewModel
    {
        public DisplayableTreeViewItem()
        {
            Header = "";
            IsExpanded = false;
            IsFolder = false;
            IsInputting = false;
            Root = "";
            ID = Guid.Empty;
            Parent = null;
            Items = new ObservableCollection<DisplayableTreeViewItem>();
        }

        public void Selected(object sender, RoutedEventArgs args)
        {
            SelectedEvent?.Invoke(sender, args);
        }

        public void DoubleClick(object sender, MouseButtonEventArgs args)
        {
            DoubleClickEvent?.Invoke(sender, args);
        }

        public void RightClick(object sender, MouseButtonEventArgs args)
        {
            RightClickEvent?.Invoke(sender, args);
        }

        public void FocusLost(object sender, RoutedEventArgs args)
        {
            FocusLostEvent?.Invoke(sender, args);
        }

        public void KeyUp(object sender, KeyEventArgs args)
        {
            KeyUpEvent?.Invoke(sender, args);
        }

        #region IsInputting & IsDisplaying
        private bool m_IsInputting;
        public bool IsInputting
        {
            get
            {
                return m_IsInputting;
            }
            set
            {
                if (m_IsInputting != value)
                {
                    m_IsInputting = value;

                    OnPropertyChanged(nameof(IsDisplaying));
                    OnPropertyChanged(nameof(IsInputting));
                }
            }
        }
        public bool IsDisplaying
        {
            get
            {
                return !m_IsInputting;
            }
            set
            {
                if ((!m_IsInputting) != value)
                {
                    m_IsInputting = !value;

                    OnPropertyChanged(nameof(IsDisplaying));
                    OnPropertyChanged(nameof(IsInputting));
                }
            }
        }
        #endregion

        #region Header
        private string m_Header;
        public string Header
        {
            get
            {
                return m_Header;
            }
            set
            {
                if (m_Header != value)
                {
                    m_Header = value;
                    RelativePath = Regex.Replace((Root + "/" + value).Replace('\\', '/'), @"/\/\/+/g", "/");
                    OnPropertyChanged(nameof(Header));
                }
            }
        }
        #endregion
        #region IsExpanded
        private bool m_IsExpaned;
        public bool IsExpanded
        {
            get
            {
                return m_IsExpaned;
            }
            set
            {
                if (m_IsExpaned != value)
                {
                    m_IsExpaned = value;
                    OnPropertyChanged(nameof(IsExpanded));
                }
            }
        }
        #endregion
        #region IsFolder
        private bool m_IsFolder;
        public bool IsFolder
        {
            get
            {
                return m_IsFolder;
            }
            set
            {
                if (m_IsFolder != value)
                {
                    m_IsFolder = value;
                    OnPropertyChanged(nameof(IsFolder));
                }
            }
        }
        #endregion
        #region IsMacro
        public bool IsMacro
        {
            get
            {
                return !IsFolder;
            }
            set
            {
                if ((!IsFolder) != value)
                {
                    IsFolder = !value;
                    OnPropertyChanged(nameof(IsMacro));
                }
            }
        }
        #endregion
        #region IsRibbonMacro
        public bool IsRibbonMacro
        {
            get
            {
                return MacroUI.GetInstance().IsRibbonMacro(ID);
            }
            set
            {
                if (ID == Guid.Empty)
                    return;

                if (value)
                    MacroUI.GetInstance().AddRibbonMacro(ID);
                else
                    MacroUI.GetInstance().RemoveRibbonMacro(ID);

                OnPropertyChanged(nameof(IsRibbonMacro));
            }
        }
        #endregion
        #region Root
        private string m_Root;
        public string Root
        {
            get
            {
                return m_Root;
            }
            set
            {
                if (m_Root != value)
                {
                    m_Root = value;
                    RelativePath = Regex.Replace((value + "/" + Header).Replace('\\', '/'), @"/\/\/+/g", "/");
                    OnPropertyChanged(nameof(Root));
                }
            }
        }
        #endregion
        #region RelativePath
        private string m_RelativePath;
        public string RelativePath
        {
            get
            {
                return m_RelativePath;
            }
            internal set
            {
                if (m_RelativePath != value)
                {
                    m_RelativePath = value;
                    OnPropertyChanged(RelativePath);
                }
            }
        }
        #endregion
        #region ID
        private Guid m_ID;
        public Guid ID
        {
            get
            {
                return m_ID;
            }
            set
            {
                if (m_ID != value)
                {
                    m_ID = value;
                    OnPropertyChanged(nameof(ID));
                }
            }
        }
        #endregion
        #region Parent
        private DisplayableTreeViewItem m_Parent;
        public DisplayableTreeViewItem Parent
        {
            get
            {
                return m_Parent;
            }
            set
            {
                if (m_Parent != value)
                {
                    m_Parent = value;
                    OnPropertyChanged(nameof(Parent));
                }
            }
        }
        #endregion
        #region Items
        private ObservableCollection<DisplayableTreeViewItem> m_Items;
        public ObservableCollection<DisplayableTreeViewItem> Items
        {
            get
            {
                return m_Items;
            }
            set
            {
                if (m_Items != value)
                {
                    m_Items = value;
                    OnPropertyChanged(nameof(Items));
                }
            }
        }
        #endregion

        #region SelectedEvent
        private Action<object, RoutedEventArgs> m_SelectedEvent;
        public Action<object, RoutedEventArgs> SelectedEvent
        {
            get
            {
                return m_SelectedEvent;
            }
            set
            {
                if (m_SelectedEvent != value)
                {
                    m_SelectedEvent = value;
                    OnPropertyChanged(nameof(SelectedEvent));
                }
            }
        }
        #endregion
        #region DoubleClickEvent
        private Action<object, MouseButtonEventArgs> m_DoubleClickEvent;
        public Action<object, MouseButtonEventArgs> DoubleClickEvent
        {
            get
            {
                return m_DoubleClickEvent;
            }
            set
            {
                if (m_DoubleClickEvent != value)
                {
                    m_DoubleClickEvent = value;
                    OnPropertyChanged(nameof(DoubleClickEvent));
                }
            }
        }
        #endregion
        #region RightClickEvent
        private Action<object, MouseButtonEventArgs> m_RightClickEvent;
        public Action<object, MouseButtonEventArgs> RightClickEvent
        {
            get
            {
                return m_RightClickEvent;
            }
            set
            {
                if (m_RightClickEvent != value)
                {
                    m_RightClickEvent = value;
                    OnPropertyChanged(nameof(RightClickEvent));
                }
            }
        }
        #endregion

        #region FocusLostEvent
        private Action<object, RoutedEventArgs> m_FocusLostEvent;
        public Action<object, RoutedEventArgs> FocusLostEvent
        {
            get
            {
                return m_FocusLostEvent;
            }
            set
            {
                if (m_FocusLostEvent != value)
                {
                    m_FocusLostEvent = value;
                    OnPropertyChanged(nameof(FocusLostEvent));
                }
            }
        }
        #endregion
        #region KeyUpEvent
        private Action<object, KeyEventArgs> m_KeyUpEvent;
        public Action<object, KeyEventArgs> KeyUpEvent
        {
            get
            {
                return m_KeyUpEvent;
            }
            set
            {
                if (m_KeyUpEvent != value)
                {
                    m_KeyUpEvent = value;
                    OnPropertyChanged(nameof(KeyUpEvent));
                }
            }
        }
        #endregion
    }
}
