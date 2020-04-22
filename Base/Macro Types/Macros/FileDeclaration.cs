using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Macro_Engine.Macros
{
    /// <summary>
    /// Data strcture containing info on a macro
    /// </summary>
    public class FileDeclaration : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        #endregion

        #region Properties

        public FileInfo Info { get; set; }
        public string Content { get; set; }

        public string Name
        {
            get
            {
                return Path.GetFileNameWithoutExtension(Info.Name);
            }
        }

        #endregion

        public delegate void RemoveEvent();
        public event RemoveEvent OnRemoved;

        public FileDeclaration(FileInfo info, string content)
        {
            Info = info;
            Content = content;

            OnPropertyChanged(nameof(Info));
            OnPropertyChanged(nameof(Name));
            OnPropertyChanged(nameof(Content));
        }

        public void Remove()
        {
            OnRemoved?.Invoke();
        }

        /// <summary>
        /// Renames the macro file
        /// </summary>
        /// <param name="name">The new name for the macro</param>
        public void Rename(string name)
        {
            if (Info == null)
                return;

            Files.RenameFile(Info, name);

            OnPropertyChanged(nameof(Info));
            OnPropertyChanged(nameof(Name));
        }

        /// <summary>
        /// Save the macro to it's respective file
        /// </summary>
        public void Save()
        {
            if (Info == null)
                return;

            Files.SaveFile(Info, Content);
        }

        /// <summary>
        /// Export the macro to a different file -> Save Copy As.
        /// </summary>
        public void Export()
        {
            if (Info == null)
                return;

            Files.ExportFile(Info, Content);
        }

        /// <summary>
        /// Delete the macro and it's respective file
        /// </summary>
        public async Task<bool> Delete()
        {
            if (Info == null)
                return true;

            bool result = await Files.DeleteFile(Info);

            if(result)
                OnRemoved?.Invoke();

            return result;
        }
    }
}
