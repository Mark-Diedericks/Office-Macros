using Macro_Engine.Macros;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Macro_Engine
{
    public class Files
    {
        public static readonly string ASSEMBLY_FILE_EXT = ".dll";
        public static readonly string ASSEMBLY_FILTER = "Assembly | *" + ASSEMBLY_FILE_EXT;

        #region Path Helpers

        public static readonly Regex REG_DIR = new Regex("[/]{2,}", RegexOptions.None);

        public static string CleanPath(string path)
        {
            path = path.Replace("\\", "/");
            path = REG_DIR.Replace(path, "/");
            return path;
        }

        public static string FullPath(string basepath, string relativepath)
        {
            return CleanPath(basepath + "/" + basepath);
        }

        public static string FullPathMacro(string relativepath)
        {
            return FullPath(MacroDirectory, relativepath);
        }
        public static string FullPathExtension(string relativepath)
        {
            return FullPath(ExtensionsDirectory, relativepath);
        }
        public static string FullPathAssembly(string relativepath)
        {
            return FullPath(AssemblyDirectory, relativepath);
        }

        #endregion

        #region Directories

        /// <summary>
        /// Gets the current AssemblyDirectory (working directory)
        /// </summary>
        public static string AssemblyDirectory
        {
            get
            {
                string codeBase = System.Reflection.Assembly.GetAssembly(typeof(Events)).CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }

        /// <summary>
        /// Gets the current AssemblyDirectory (working directory)
        /// </summary>
        public static string ExtensionsDirectory
        {
            get
            {
                string codeBase = System.Reflection.Assembly.GetAssembly(typeof(Events)).CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.Combine(Path.GetDirectoryName(path), "Extensions");
            }
        }

        /// <summary>
        /// Gets the directory in which the Macros are stored in the file system
        /// </summary>
        public static string MacroDirectory
        {
            get
            {
                return Path.GetFullPath(AssemblyDirectory + "/Macros/");
            }
        }

        #endregion


        #region File Discovery & Loading

        /// <summary>
        /// Gets all the files in the directory and it's subsequent directories
        /// </summary>
        /// <param name="directory">The directory in which to look</param>
        /// <returns>A list containing the fullpath to each file in the directory and subsequent directories</returns>
        public static List<string> GetFiles(string directory)
        {
            if (!Directory.Exists(directory))
                return new List<string>();

            List<string> result = new List<string>();
            string[] files = Directory.GetFiles(directory);
            string[] dirs = Directory.GetDirectories(directory);

            foreach (string file in files)
                result.Add(file);

            foreach (string dir in dirs)
                result.AddRange(GetFiles(dir));
            return result;
        }

        /// <summary>
        /// Identifies macros in an array of directories and loads them
        /// </summary>
        /// <param name="directories">An array of directories in which to search</param>
        /// <returns>A HashSet of MacroDeclarations of macros found</returns>
        public static HashSet<FileDeclaration> LoadAllFiles(string[] directories)
        {
            HashSet<FileDeclaration> declarations = new HashSet<FileDeclaration>();

            foreach (string directory in directories)
            {
                List<string> files = GetFiles(directory);

                foreach (string file in files)
                {
                    FileInfo fi = new FileInfo(file);

                    if (!fi.Exists)
                        continue;

                    declarations.Add(new FileDeclaration(fi, LoadFile(fi)));
                }
            }

            return declarations;
        }

        #endregion

        #region Assembly Utils

        /// <summary>
        /// Prompts the user to select an assembly and returns the file location
        /// </summary>
        /// <returns>Full file path of the selected assembly</returns>
        public static string ImportAssembly()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = Files.ASSEMBLY_FILTER;

            if (ofd.ShowDialog() == DialogResult.OK)
                return ofd.FileName;

            return String.Empty;
        }

        #endregion

        #region File Utils

        /// <summary>
        /// Saves a macro to it's respective file
        /// </summary>
        /// <param name="id">The id of the macro</param>
        /// <param name="source">The source code (python) to be saved</param>
        public static void SaveFile(FileInfo info, string source)
        {
            if (!info.Exists)
                return;

            try
            {
                File.WriteAllText(info.FullName, source);
            }
            catch (Exception e)
            {
                Messages.DisplayOkMessage("Could not save macro: \"" + info.Name + "\". \n\n" + e.Message, "Saving Error");
            }
        }

        /// <summary>
        /// Loads the macro file
        /// </summary>
        /// <param name="info">The macro file</param>
        /// <returns>Macro instance</returns>
        public static string LoadFile(FileInfo info)
        {
            try
            {
                if (!info.Exists)
                    return null;

                return File.ReadAllText(info.FullName);
            }
            catch (Exception e)
            {
                Messages.DisplayOkMessage("Could not open macro: \"" + info.Name + "\". \n\n" + e.Message, "Loading Error");
            }

            return String.Empty;
        }

        /// <summary>
        /// Exports a macro to a file, similar to saving a copy elsewhere
        /// </summary>
        /// <param name="info">The macro file</param>
        /// <param name="source">The source code (python) of the macro</param>
        public static void ExportFile(FileInfo info, string source)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.FileName = info.Name;
            sfd.Filter = "Macro | *" + info.Extension;

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                //MacroEngine.FireShowFocusEvent();
                Events.InvokeEvent("ShowFocusEvent");

                try
                {
                    File.WriteAllText(sfd.FileName, source);
                }
                catch (Exception e)
                {
                    Messages.DisplayOkMessage("Could not export macro: \"" + info.Name + "\". \n\n" + e.Message, "Saving Error");
                }
            }

            //MacroEngine.FireShowFocusEvent();
            Events.InvokeEvent("ShowFocusEvent");
        }

        /// <summary>
        /// Prompts the user to select an external macro and imports it into the local workspace
        /// </summary>
        /// <param name="info">The directory which the macro will be copied to</param>
        public static async Task<FileDeclaration> ImportFile(DirectoryInfo info)
        {
            OpenFileDialog ofd = new OpenFileDialog();

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                //MacroEngine.FireShowFocusEvent();
                Events.InvokeEvent("ShowFocusEvent");

                string fullpath = Files.FullPath(info.FullName, ofd.SafeFileName);

                FileInfo fi = new FileInfo(fullpath);

                if (fi.Exists)
                {
                    bool result = await Messages.DisplayYesNoMessage("This file already exists, would you like to replace it?", "File Overwrite");
                    if (!result)
                        return null;
                }

                if (!fi.Directory.Exists)
                    fi.Directory.Create();

                File.Copy(ofd.FileName, fullpath, true);

                FileDeclaration declaration = new FileDeclaration(fi, LoadFile(fi));

                //MacroEngine.GetInstance().AddMacro(declaration);
                return declaration;
            }

            //MacroEngine.FireShowFocusEvent();
            Events.InvokeEvent("ShowFocusEvent");
            return null;
        }

        /// <summary>
        /// Renames a macro on the file system
        /// </summary>
        /// <param name="md">The declaration of the macro</param>
        /// <param name="name">The new name of the macro</param>
        /// <returns>Bool identifying if the operation is successful</returns>
        public static bool RenameFile(FileInfo info, string name)
        {
            if (!info.Exists)
                return false;
            try
            {

                info.MoveTo(Files.FullPath(info.Directory.FullName, name));
                return true;
            }
            catch (Exception e)
            {
                Messages.DisplayOkMessage("Could not rename the macro file: " + info.Name + "\n" + e.Message, "Renaming Error");
            }

            return false;
        }

        /// <summary>
        /// Creates a new file at the specified location
        /// </summary>
        /// <param name="info">The macro file</param>
        /// <returns>The id of the newly created macro</returns>
        public static FileDeclaration CreateFile(FileInfo info)
        {
            try
            {
                if (info.Exists)
                    throw new ArgumentException("File with name '" + info.FullName + "' already exists.");

                if (!info.Directory.Exists)
                    info.Directory.Create();

                using (StreamWriter sw = info.CreateText())
                    sw.WriteLine("");

                FileDeclaration declaration = new FileDeclaration(info, LoadFile(info));

                //MacroEngine.GetInstance().AddMacro(declaration);
                return declaration;
            }
            catch (Exception e)
            {
                Messages.DisplayOkMessage("Could not create the macro file: " + info.Name + "\n" + e.Message, "Creation error");
            }

            return null;
        }

        /// <summary>
        /// Deletes the specified macro
        /// </summary>
        /// <param name="id">The id of the macro</param>
        /// <returns>If action is successful</returns>
        public static async Task<bool> DeleteFile(FileInfo info)
        {
            if (!info.Exists)
                return true;

            bool result = await Messages.DisplayYesNoMessage("'" + info.Name + "' Will be deleted permanently.", "Macro Deletion");

            if (!result)
                return false;

            try
            {
                System.Diagnostics.Debug.WriteLine("Deleting...");
                File.Delete(info.FullName);
                return true;
            }
            catch (Exception e)
            {
                Messages.DisplayOkMessage("Could not delete macro: \"" + info.Name + "\". \n\n" + e.Message, "Deletion Error");
            }

            return false;
        }

        #endregion

        #region Folders

        /// <summary>
        /// Creates a new folder from a relative path
        /// </summary>
        /// <param name="info">The expected folder</param>
        /// <returns>Bool idebtifying if the operation was successful</returns>
        public static bool CreateFolder(DirectoryInfo info)
        {
            try
            {
                if (!info.Exists)
                    info.Create();

                return true;
            }
            catch (Exception e)
            {
                Messages.DisplayOkMessage("Could not create the folder: " + info.Name + "\n" + e.Message, "Creation error");
            }

            return false;
        }

        /// <summary>
        /// Deletes the folder at the relative path
        /// </summary>
        /// <param name="info">The  folder</param>
        public static async Task<bool> DeleteFolder(DirectoryInfo info)
        {
            bool result = await Messages.DisplayYesNoMessage("'" + info.Name + "' Will be deleted permanently.", "Macro Deletion");

            if (!result)
                return false;

            try
            {
                Directory.Delete(info.FullName, true);
                return true;
            }
            catch (Exception e)
            {
                Messages.DisplayOkMessage("Could not delete the folder: " + info.Name + "\n" + e.Message, "Creation error");
            }


            return false;
        }

        /// <summary>
        /// Renames a folder from its previous name to the new name
        /// </summary>
        /// <param name="info">The current folder</param>
        /// <param name="newpath">Desired path of the folder</param>
        /// <returns></returns>
        public static bool RenameFolder(DirectoryInfo info, string newpath)
        {
            try
            {
                Directory.Move(info.FullName, newpath);
                return true;
            }
            catch (Exception e)
            {
                Messages.DisplayOkMessage("Could not rename the folder: " + info.Name + "\n" + e.Message, "Creation error");
            }

            return false;
        }

        #endregion
    }
}
