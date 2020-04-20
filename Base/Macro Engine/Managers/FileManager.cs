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
                    FileInfo fi = new FileInfo(file);
                    if (!fi.Directory.Exists)
                        fi.Directory.Create();

                    string lang = MacroEngine.GetInstance().GetLangaugeFromFileExt(Path.GetExtension(file));

                    if (string.IsNullOrEmpty(lang))
                        continue;

                    IMacro macro = LoadMacro(lang, fi);
                    declarations.Add(new MacroDeclaration(lang, fi, macro));
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
                FileInfo info = md.Info;

                if(!info.Exists)
                {
                    System.Diagnostics.Debug.WriteLine("Could not find file: " + info.FullName);
                    continue;
                }

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
                if (!Directory.Exists(md.Info.DirectoryName))
                    Directory.CreateDirectory(md.Info.DirectoryName);

                File.WriteAllText(md.Info.FullName, source);
            }
            catch (Exception e)
            {
                Messages.DisplayOkMessage("Could not save macro: \"" + md.Info.Name + "\". \n\n" + e.Message, "Saving Error");
            }
        }

        /// <summary>
        /// Identifies which method to utilize to load a macro and returns the loaded macro
        /// </summary>
        /// <param name="type">The macro type of the macro</param>
        /// <param name="info">The macro file</param>
        /// <returns>Macro instance</returns>
        public static IMacro LoadMacro(string language, FileInfo info)
        {
            try
            {
                if (!info.Exists)
                    return null;

                string source = File.ReadAllText(info.FullName);
                return new Macro(language, source);
            }
            catch (Exception e)
            {
                Messages.DisplayOkMessage("Could not open macro: \"" + info.Name + "\". \n\n" + e.Message, "Loading Error");
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
            sfd.FileName = md.Info.Name;
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
                    Messages.DisplayOkMessage("Could not export macro: \"" + md.Info.Name + "\". \n\n" + e.Message, "Saving Error");
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
        /// <param name="info">The directory which the macro will be copied to</param>
        public static async Task<Guid> ImportMacro(DirectoryInfo info)
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
                    return Guid.Empty;
                }

                string fullpath = Files.FullPath(info.FullName, ofd.SafeFileName);

                FileInfo fi = new FileInfo(fullpath);

                if (fi.Exists)
                {
                    bool result = await Messages.DisplayYesNoMessage("This file already exists, would you like to replace it?", "File Overwrite");
                    if (!result)
                        return Guid.Empty;
                }

                if (!fi.Directory.Exists)
                    fi.Directory.Create();

                File.Copy(ofd.FileName, fullpath, true);

                IMacro macro = LoadMacro(lang, fi);
                MacroDeclaration declaration = new MacroDeclaration(lang, fi, macro);

                return MacroEngine.GetInstance().AddMacro(declaration);
            }

            MacroEngine.FireShowFocusEvent();
            return Guid.Empty;
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
                if (!md.Info.Exists)
                    return false;

                md.Info.MoveTo(Files.FullPath(md.Info.Directory.FullName, name));

                //MacroEngine.GetInstance().SetDeclaration(id, new MacroDeclaration(md.Language, info.Name, info.FullName, md.ID));

                return true;
            }
            catch (Exception e)
            {
                Messages.DisplayOkMessage("Could not rename the macro file: " + md.Info.Name + "\n" + e.Message, "Renaming Error");
            }

            return false;
        }

        /// <summary>
        /// Creates a new file at the specified location
        /// </summary>
        /// <param name="info">The macro file</param>
        /// <returns>The id of the newly created macro</returns>
        public static async Task<Guid> CreateFile(FileInfo info)
        {
            string lang = MacroEngine.GetInstance().GetLangaugeFromFileExt(info.Extension);

            if (info.Exists)
                return LoadMacro(lang, info);

            try
            {
                if (!info.Directory.Exists)
                    info.Directory.Create();

                using (StreamWriter sw = info.CreateText())
                    sw.WriteLine("");

                IMacro macro = LoadMacro(lang, info);
                macro.CreateBlankMacro();

                MacroDeclaration declaration = new MacroDeclaration(lang, info, macro);

                Guid macroID = MacroEngine.GetInstance().AddMacro(declaration);
                return macroID;
            }
            catch (Exception e)
            {
                Messages.DisplayOkMessage("Could not create the macro file: " + info.Name + "\n" + e.Message, "Creation error");
            }

            return Guid.Empty;
        }

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

        /// <summary>
        /// Deletes the specified macro
        /// </summary>
        /// <param name="id">The id of the macro</param>
        /// <returns>If action is successful</returns>
        public static async Task<bool> DeleteMacro(Guid id)
        {
            MacroDeclaration md = MacroEngine.GetInstance().GetDeclaration(id);
            if (md == null)
                return false;

            bool result = await Messages.DisplayYesNoMessage("'" + md.Info.Name + "' Will be deleted permanently.", "Macro Deletion");

            if (!result)
                return false;

            try
            {
                System.Diagnostics.Debug.WriteLine("Deleting...");
                File.Delete(md.Info.FullName);
                MacroEngine.GetInstance().RemoveMacro(id);

                return true;
            }
            catch (Exception e)
            {
                Messages.DisplayOkMessage("Could not delete macro: \"" + md.Info.Name + "\". \n\n" + e.Message, "Deletion Error");
            }

            return false;
        }

        #endregion
    }
}
