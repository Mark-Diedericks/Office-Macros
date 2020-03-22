﻿using Macro_Engine.Macros;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Macro_Engine
{
    public class FileManager
    {
        public static FileManager GetInstance()
        {
            return MacroEngine.GetFileManager();
        }

        public FileManager()
        {
            Events.SubscribeEvent("CreateFolder", new Action<Action<bool>, string>((r, p) =>
            {
                r.Invoke(CreateFolder(p));
            }));

            Events.SubscribeEvent("ImportAssembly", new Action<Action<string>>((r) =>
            {
                r.Invoke(ImportAssembly());
            }));

            Events.SubscribeEvent("CreateMacro", new Action<Action<Guid>, string>((r, p) =>
            {
                r.Invoke(CreateMacro(p));
            }));

            Events.SubscribeEvent("ImportMacro", new Action<Action<Guid>, string>((r, p) =>
            {
                ImportMacro(p, r);
            }));
        }

        #region MACRO_LOADING

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
        /// Identifies macros in an array of directories and creates MacroDeclarations for them
        /// </summary>
        /// <param name="directories">An array of directories in which to search</param>
        /// <returns>A list of MacroDeclarations of macros found</returns>
        public static List<MacroDeclaration> IdentifyAllMacros(string[] directories)
        {
            List<MacroDeclaration> declarations = new List<MacroDeclaration>();

            foreach (string directory in directories)
            {
                List<string> files = GetFiles(directory);

                foreach (string file in files)
                {
                    string relativepath = CalculateRelativePath(file);
                    string fullpath = CalculateFullPath(relativepath);

                    FileInfo fi = new FileInfo(fullpath);
                    if (!fi.Directory.Exists)
                        fi.Directory.Create();

                    string lang = MacroEngine.GetInstance().GetLangaugeFromFileExt(Path.GetExtension(file));

                    if (string.IsNullOrEmpty(lang))
                        continue;

                    declarations.Add(new MacroDeclaration(lang, Path.GetFileName(fullpath), relativepath));
                }
            }
            return declarations;
        }

        /// <summary>
        /// Identifies and loads all macros in a list of directories
        /// </summary>
        /// <param name="directories">An array of directories in which to search</param>
        /// <returns>A dictionary of MacroDeclarations and their respective Macros</returns>
        public static Dictionary<MacroDeclaration, IMacro> LoadAllMacros(string[] directories)
        {
            Dictionary<MacroDeclaration, IMacro> macros = new Dictionary<MacroDeclaration, IMacro>();
            List<MacroDeclaration> declarations = IdentifyAllMacros(directories);

            foreach (MacroDeclaration md in declarations)
            {
                IMacro macro = LoadMacro(md.Language, md.RelativePath);

                if (macro != null)
                    macros.Add(md, macro);
            }

            return macros;
        }

        #endregion

        #region GENERIC_FILE_UTIL

        /// <summary>
        /// Saves a macro to it's respective file
        /// </summary>
        /// <param name="id">The id of the macro</param>
        /// <param name="source">The source code (python) to be saved</param>
        public static void SaveMacro(Guid id, string source)
        {
            MacroDeclaration md = MacroEngine.GetInstance().GetDeclaration(id);
            if (md == null)
                return;

            try
            {
                string fullpath = CalculateFullPath(md.RelativePath);

                FileInfo fi = new FileInfo(fullpath);

                if (!Directory.Exists(fi.DirectoryName))
                    Directory.CreateDirectory(fi.DirectoryName);

                File.WriteAllText(fullpath, source);
            }
            catch (Exception e)
            {
                Messages.DisplayOkMessage("Could not save macro: \"" + md.Name + "\". \n\n" + e.Message, "Saving Error");
            }
        }

        /// <summary>
        /// Identifies which method to utilize to load a macro and returns the loaded macro
        /// </summary>
        /// <param name="type">The macro type of the macro</param>
        /// <param name="relativepath">The relative filepath of the macro</param>
        /// <returns>Macro instance</returns>
        public static IMacro LoadMacro(string language, string relativepath)
        {
            try
            {
                string fullpath = CalculateFullPath(relativepath);

                FileInfo fi = new FileInfo(fullpath);
                if (!fi.Exists)
                    return null;

                string source = File.ReadAllText(fullpath.Trim());
                return new Macro(language, source);
            }
            catch (Exception e)
            {
                Messages.DisplayOkMessage("Could not open macro: \"" + relativepath + "\". \n\n" + e.Message, "Loading Error");
            }

            return null;
        }

        /// <summary>
        /// Exports a macro to a file, similar to saving a copy elsewhere
        /// </summary>
        /// <param name="id">The id of the macro</param>
        /// <param name="source">The source code (python) of the macro</param>
        public static void ExportMacro(Guid id, string source)
        {
            MacroDeclaration md = MacroEngine.GetInstance().GetDeclaration(id);
            if (md == null)
                return;

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.FileName = md.Name;
            sfd.Filter = md.Language + " | *" + MacroEngine.GetInstance().GetMacro(md.ID).GetDefaultFileExtension();

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                MacroEngine.FireShowFocusEvent();

                try
                {
                    File.WriteAllText(sfd.FileName, source);
                }
                catch (Exception e)
                {
                    Messages.DisplayOkMessage("Could not export macro: \"" + md.Name + "\". \n\n" + e.Message, "Saving Error");
                }
            }

            MacroEngine.FireShowFocusEvent();
        }

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

        /// <summary>
        /// Prompts the user to select an external macro and imports it into the local workspace
        /// </summary>
        /// <param name="relativedir">The relative directory which the macro will be copied to</param>
        /// <param name="OnReturn">The Action, containing the macro's id, to be fired when the task is completed</param>
        public static void ImportMacro(string relativedir, Action<Guid> OnReturn)
        {
            OpenFileDialog ofd = new OpenFileDialog();

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                MacroEngine.FireShowFocusEvent();

                string fileExt = Path.GetExtension(ofd.FileName).ToLower().Trim();
                string lang = MacroEngine.GetInstance().GetLangaugeFromFileExt(fileExt);

                if(string.IsNullOrEmpty(lang))
                {
                    MacroEngine.FireShowFocusEvent();
                    OnReturn?.Invoke(Guid.Empty);
                }

                string newpath = CalculateFullPath(relativedir + ofd.SafeFileName);

                System.Diagnostics.Debug.WriteLine(newpath);

                string relativepath = CalculateRelativePath(newpath);
                string fullpath = CalculateFullPath(relativepath);

                FileInfo fi = new FileInfo(fullpath);
                if (!fi.Directory.Exists)
                    fi.Directory.Create();

                if (File.Exists(fullpath))
                {
                    Messages.DisplayYesNoMessage("This file already exists, would you like to replace it?", "File Overwrite", new Action<bool>((result) =>
                    {
                        if (!result)
                            OnReturn?.Invoke(Guid.Empty);
                    }));
                }

                File.Copy(ofd.FileName, fullpath, true);

                MacroDeclaration declaration = new MacroDeclaration(lang, ofd.SafeFileName, relativepath);
                IMacro macro = LoadMacro(lang, relativepath);

                OnReturn?.Invoke(MacroEngine.GetInstance().AddMacro(declaration, macro));
            }

            MacroEngine.FireShowFocusEvent();

            OnReturn?.Invoke(Guid.Empty);
        }

        /// <summary>
        /// Renames a macro on the file system
        /// </summary>
        /// <param name="id">The id of the macro</param>
        /// <param name="name">The new name of the macro</param>
        /// <returns>Bool identifying if the operation is successful</returns>
        public static bool RenameMacro(Guid id, string name)
        {
            MacroDeclaration md = MacroEngine.GetInstance().GetDeclaration(id);
            if (md == null)
                return false;

            try
            {
                string newpath = md.RelativePath.Replace(md.RelativePath, name);

                MacroDeclaration declaration = new MacroDeclaration(md.Language, name, newpath);
                declaration.ID = id;

                File.Move(CalculateFullPath(md.RelativePath), CalculateFullPath(declaration.RelativePath));
                MacroEngine.GetInstance().SetDeclaration(id, declaration);

                return true;
            }
            catch (Exception e)
            {
                Messages.DisplayOkMessage("Could not rename the macro file: " + md.Name + "\n" + e.Message, "Renaming Error");
            }

            return false;
        }

        /// <summary>
        /// Creates a new macro file at the specified relative path
        /// </summary>
        /// <param name="type">The type of the macro</param>
        /// <param name="relativepath">The relative filepath of the macro</param>
        /// <returns>The id of the newly created macro</returns>
        public static Guid CreateMacro(string relativepath)
        {
            try
            {
                string fullpath = CalculateFullPath(relativepath);

                FileInfo fi = new FileInfo(fullpath);
                if (!fi.Directory.Exists)
                    fi.Directory.Create();

                string lang = MacroEngine.GetInstance().GetLangaugeFromFileExt(fi.Extension);

                MacroDeclaration declaration = new MacroDeclaration(lang, Path.GetFileName(relativepath), relativepath);

                File.CreateText(fullpath).Close();

                IMacro macro = LoadMacro(lang, relativepath);
                macro.CreateBlankMacro();

                return MacroEngine.GetInstance().AddMacro(declaration, macro);
            }
            catch (Exception e)
            {
                Messages.DisplayOkMessage("Could not create the macro file: " + Path.GetFileName(relativepath) + "\n" + e.Message, "Creation error");
            }

            return Guid.Empty;
        }

        /// <summary>
        /// Creates a new folder from a relative path
        /// </summary>
        /// <param name="relativepath">The relative path of the folder</param>
        /// <returns>Bool idebtifying if the operation was successful</returns>
        public static bool CreateFolder(string relativepath)
        {
            try
            {
                if (!Directory.Exists(CalculateFullPath(relativepath)))
                    Directory.CreateDirectory(CalculateFullPath(relativepath));

                return true;
            }
            catch (Exception e)
            {
                Messages.DisplayOkMessage("Could not create the folder: " + relativepath.Replace('\\', '/').Replace("//", "/") + "\n" + e.Message, "Creation error");
            }

            return false;
        }

        /// <summary>
        /// Deletes the folder at the relative path
        /// </summary>
        /// <param name="relativepath">The relative path of the folder</param>
        /// <param name="OnReturn">The Action, and bool identifying the operations success, to be fired when the task is completed</param>
        public static void DeleteFolder(string relativepath, Action<bool> OnReturn)
        {
            Messages.DisplayYesNoMessage("'" + relativepath + "' Will be deleted permanently.", "Macro Deletion", new Action<bool>((result) => {
                if (result)
                {
                    try
                    {
                        Directory.Delete(CalculateFullPath(relativepath), true);
                        OnReturn?.Invoke(true);
                    }
                    catch (Exception e)
                    {
                        Messages.DisplayOkMessage("Could not delete the folder: " + relativepath + "\n" + e.Message, "Creation error");
                    }
                }
            }));


            OnReturn?.Invoke(false);
        }

        /// <summary>
        /// Renames a folder from its previous name to the new name
        /// </summary>
        /// <param name="oldpath">Current relative path of the folder</param>
        /// <param name="newpath">Desired relative path of the folder</param>
        /// <returns></returns>
        public static bool RenameFolder(string oldpath, string newpath)
        {
            try
            {
                Directory.Move(CalculateFullPath(oldpath), CalculateFullPath(newpath));
                return true;
            }
            catch (Exception e)
            {
                Messages.DisplayOkMessage("Could not rename the folder: " + oldpath + "\n" + e.Message, "Creation error");
            }

            return false;
        }

        /// <summary>
        /// Deletes the specified macro
        /// </summary>
        /// <param name="id">The id of the macro</param>
        /// <param name="OnReturn">The Action, and bool identifying if the operation was successful, to be fired when the task is completed</param>
        public static void DeleteMacro(Guid id, Action<bool> OnReturn)
        {
            MacroDeclaration md = MacroEngine.GetInstance().GetDeclaration(id);
            if (md == null)
                return;

            Messages.DisplayYesNoMessage("'" + md.Name + "' Will be deleted permanently.", "Macro Deletion", new Action<bool>(result =>
            {
                if (result)
                {
                    try
                    {
                        System.Diagnostics.Debug.WriteLine("Deleting...");
                        File.Delete(CalculateFullPath(md.RelativePath));
                        MacroEngine.GetInstance().RemoveMacro(id);

                        OnReturn?.Invoke(true);
                    }
                    catch (Exception e)
                    {
                        Messages.DisplayOkMessage("Could not delete macro: \"" + md.Name + "\". \n\n" + e.Message, "Deletion Error");
                    }
                }
            }));

            OnReturn?.Invoke(false);
        }

        /// <summary>
        /// Calculates the relative path from a fullpath
        /// </summary>
        /// <param name="fullpath">A fullpath</param>
        /// <returns>Relative path of the fullpath</returns>
        public static string CalculateRelativePath(string fullpath)
        {
            return fullpath.Remove(0, Files.MacroDirectory.Length);
        }

        /// <summary>
        /// Calculates the fullpath from a relative path
        /// </summary>
        /// <param name="relativepath">A relative path</param>
        /// <returns>Fullpath of the relative path</returns>
        public static string CalculateFullPath(string relativepath)
        {
            return Path.GetFullPath(Files.MacroDirectory + relativepath);
        }

        #endregion
    }
}
