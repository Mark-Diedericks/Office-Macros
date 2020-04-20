using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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
    }
}
