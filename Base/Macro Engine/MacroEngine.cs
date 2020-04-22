/* 
 * Mark Diedericks
 * 18/03/2020
 * Base macro engine
 */

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using Macro_Engine.Engine;
using Macro_Engine.Interop;
using Macro_Engine.Macros;

namespace Macro_Engine
{
    public class MacroEngine : IMacroEngine
    {

        #region Extensibility

        [ImportMany(typeof(IExecutionEngine))]
        private IEnumerable<Lazy<IExecutionEngine, IExecutionEngineData>> m_ExecutionEngineImplementations;

        private CompositionContainer m_Container;
        private bool m_HasExtensions;

        private void Compose()
        {
            m_HasExtensions = false;

            AggregateCatalog catalog = new AggregateCatalog();

            if (Directory.Exists(Files.ExtensionsDirectory))
            {
                catalog.Catalogs.Add(new DirectoryCatalog(Files.ExtensionsDirectory));
                foreach(string addin_dir in Directory.GetDirectories(Files.ExtensionsDirectory))
                    catalog.Catalogs.Add(new DirectoryCatalog(addin_dir));

                m_HasExtensions = true;
            }

            m_Container = new CompositionContainer(catalog);
            m_Container.ComposeParts(this);
        }

        #endregion

        #region Dispatchers & Threading

        private readonly Executor m_HostExecutor;

        /// <summary>
        /// Execute a task on the host dispatcher with a given priority
        /// </summary>
        /// <param name="task">Task to be executed</param>
        private static async Task<bool> ExecuteOnHost(Func<bool> task)
        {
            return await GetInstance().m_HostExecutor?.ExecuteAction(task);
        }

        #endregion

        #region Instance Getters

        private static MacroEngine s_Instance = null;

        /// <summary>
        /// Gets instance of MacroEngine
        /// </summary>
        /// <returns>MacroEngine instance</returns>
        public static MacroEngine GetInstance()
        {
            return s_Instance;
        }

        #endregion

        #region Initialization & Destruction
        //Macros
        private HashSet<FileDeclaration> m_Files;
        private FileDeclaration m_ActiveFile;

        //User Included Assemblies
        private HashSet<AssemblyDeclaration> m_Assemblies;

        //All runtimes
        private HashSet<string> m_Runtimes;

        /// <summary>
        /// Private constructor
        /// </summary>
        /// <param name="executor">Host application UI executor</param>
        private MacroEngine(Executor executor)
        {
            s_Instance = this;

            m_HostExecutor = executor;

            Compose();

            m_Runtimes = new HashSet<string>();
            foreach (Lazy<IExecutionEngine, IExecutionEngineData> pair in GetInstance().m_ExecutionEngineImplementations)
            {
                m_Runtimes.Add(pair.Metadata.Runtime);
                pair.Value.Initialize();
            }

            //Events.SubscribeEvent("OnHostExecute", (Action<Action>)ExecuteOnHost);
            Events.ExecuteOnHostEvent = ExecuteOnHost;

            Events.SubscribeEvent("SetIO", (Action<string, TextWriter, TextWriter, TextReader>)SetIOStreams);

            Events.SubscribeEvent("OnTerminateExecution", new Action(() => {
                foreach (Lazy<IExecutionEngine, IExecutionEngineData> pair in GetInstance().m_ExecutionEngineImplementations)
                    pair.Value.TerminateExecution();
            }));

            m_Files = new HashSet<FileDeclaration>();
        }

        /// <summary>
        /// Public instantiation of MacroEngine
        /// </summary>
        /// <param name="hostDispatcher">Host application UI dispatcher</param>
        /// <param name="state">Host application state; ribbon macros, active macro, assemblies and workspaces</param>
        /// <param name="OnLoaded">Action to be fired once MacroEngine is fully initialized</param>
        /// <returns>The initialization task thread</returns>
        public void Instantiate(HostState state)
        {

            //Load all macros in workspaces
            m_Files = Files.LoadAllFiles(state.Workspaces);

            //Events.OnMacroCountChangedInvoke();
            Events.InvokeEvent("OnMacroCountChanged");

            //Get the active macro
            if (!String.IsNullOrEmpty(state.ActiveMacro))
                m_ActiveFile = GetDeclarationFromFullname(state.ActiveMacro);
            else
                m_ActiveFile = m_Files.FirstOrDefault<FileDeclaration>();


            //Get Assemblies
            if (state.Assemblies != null)
                m_Assemblies = new HashSet<AssemblyDeclaration>(state.Assemblies);
            else
                m_Assemblies = new HashSet<AssemblyDeclaration>();

            //Events.OnAssembliesChangedInvoke();
            Events.InvokeEvent("OnAssembliesChanged");
        }

        /// <summary>
        /// Fires OnDestroyed event
        /// </summary>
        public void Destroy()
        {
            Events.InvokeEvent("OnDestroyed");
            foreach (Lazy<IExecutionEngine, IExecutionEngineData> pair in GetInstance().m_ExecutionEngineImplementations)
                pair.Value.Destroy();
        }

        public static MacroEngine CreateApplicationInstance(Executor executor)
        {
            return new MacroEngine(executor);
        }

        #endregion



        #region Execution Engine & IO

        /// <summary>
        /// Execute the macro using the source saved in macro
        /// </summary>
        /// <param name="async">Bool identifying if the macro should be execute asynchronously or not (synchronous)</param>
        /// <param name="runtime">Runtime tag identifying which execution engine to use, if empty, a default will be chosen</param>
        public async Task<bool> TryExecuteFile(FileDeclaration d, bool async, string runtime = "")
        {
            d.Save();

            if (string.IsNullOrEmpty(runtime))
                runtime = GetDefaultRuntime();

            IExecutionEngine engine = GetExecutionEngine(runtime);

            if (engine == null)
                engine = GetExecutionEngine(GetDefaultRuntime());

            if (engine == null)
                return false;

            return await engine.ExecuteMacro(d.Content, async, d.Info != null ? d.Info.Directory.FullName : "");
        }

        /// <summary>
        /// Get the appropriate IExecutionEngine implementation for the given language
        /// </summary>
        /// <param name="runtime">The specified runtime</param>
        /// <returns>IExecutionEngine of the langauge</returns>
        public IExecutionEngine GetExecutionEngine(string runtime)
        {
            try
            {
                string value = runtime.ToLower().Trim();
                foreach (Lazy<IExecutionEngine, IExecutionEngineData> engine in m_ExecutionEngineImplementations)
                    if (engine.Metadata.Runtime.Equals(value, StringComparison.OrdinalIgnoreCase))
                        return engine.Value;
            }
            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }

            return null;
        }

        /// <summary>
        /// Get the appropriate IExecutionEngine implementation for the given language
        /// </summary>
        /// <param name="language">The specified language</param>
        /// <returns>Runtime(s) of the language</returns>
        public HashSet<string> GetRuntimes(string language = "")
        {
            if (string.IsNullOrEmpty(language))
                return m_Runtimes;

            HashSet<string> runtimes = new HashSet<string>();
            try
            {
                string value = language.ToLower().Trim();
                foreach (Lazy<IExecutionEngine, IExecutionEngineData> engine in m_ExecutionEngineImplementations)
                    if (engine.Metadata.Language.Equals(value, StringComparison.OrdinalIgnoreCase))
                        runtimes.Add(engine.Metadata.Runtime);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }

            return runtimes;
        }

        /// <summary>
        /// Get the appropriate IExecutionEngine implementation for the given language
        /// </summary>
        /// <param name="runtime">The specified runtime</param>
        /// <returns>Language of the runtime</returns>
        public string GetLangauge(string runtime)
        {
            try
            {
                string value = runtime.ToLower().Trim();
                foreach (Lazy<IExecutionEngine, IExecutionEngineData> engine in m_ExecutionEngineImplementations)
                    if (engine.Metadata.Runtime.Equals(value, StringComparison.OrdinalIgnoreCase))
                        return engine.Metadata.Language;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }

            return String.Empty;
        }

        /// <summary>
        /// Get the appropriate IExecutionEngine implementation for the given language
        /// </summary>
        /// <param name="fileExt">The specified fileExt</param>
        /// <returns>Language of the File Extension</returns>
        public string GetLangaugeFromFileExt(string fileExt)
        {
            try
            {
                string value = fileExt.ToLower().Trim();
                foreach (Lazy<IExecutionEngine, IExecutionEngineData> engine in m_ExecutionEngineImplementations)
                    if (engine.Metadata.FileExt.Equals(value, StringComparison.OrdinalIgnoreCase))
                        return engine.Metadata.Language;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }

            return "Plain Text";
        }

        /// <summary>
        /// Get the appropriate IExecutionEngine implementation for the given language
        /// </summary>
        /// <param name="runtime">The specified runtime</param>
        /// <returns>FileExt of the runtime</returns>
        public string GetFileExt(string runtime)
        {
            try
            {
                string value = runtime.ToLower().Trim();
                foreach (Lazy<IExecutionEngine, IExecutionEngineData> engine in m_ExecutionEngineImplementations)
                    if (engine.Metadata.Runtime.Equals(value, StringComparison.OrdinalIgnoreCase))
                        return engine.Metadata.FileExt;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }

            return ".txt";
        }

        /// <summary>
        /// Get the appropriate default IExecutionEngine runtime
        /// </summary>
        /// <param name="runtime">The specified runtime</param>
        /// <returns>Defaulr runtime</returns>
        public string GetDefaultRuntime()
        {
            Lazy<IExecutionEngine, IExecutionEngineData> engine = m_ExecutionEngineImplementations.FirstOrDefault<Lazy<IExecutionEngine, IExecutionEngineData>>();
            if (engine.Value == null)
                return "";

            return GetFileExt(engine.Metadata.Runtime);
        }

        /// <summary>
        /// Get the appropriate default runtime file extension
        /// </summary>
        /// <returns>FileExt of the default runtime</returns>
        public string GetDefaultFileExtension()
        {
            return GetFileExt(GetDefaultRuntime());
        }

        /// <summary>
        /// Sets the TextWriters for both output and error of the execution engines
        /// </summary>
        /// <param name="runtime">Runtime for which the IO is associated</param>
        /// <param name="output">TextWriter for ouput stream</param>
        /// <param name="error">TextWriter for error stream</param>
        /// <param name="input">TextReader for inputstream</param>
        public void SetIOStreams(string runtime, TextWriter output, TextWriter error, TextReader input)
        {
            IExecutionEngineIO manager = new ExecutionEngineIO().SetStreams(output, error, input);

            if (string.IsNullOrEmpty(runtime))
                foreach (Lazy<IExecutionEngine, IExecutionEngineData> pair in m_ExecutionEngineImplementations)
                    pair.Value?.SetIO(manager);
            else
                GetExecutionEngine(runtime)?.SetIO(manager);
        }

        /// <summary>
        /// Assign a variable a value in each of the execution engine scopes
        /// </summary>
        /// <param name="name">Variable name</param>
        /// <param name="value">Variable value</param>
        public void SetExecutionValue(string name, object value)
        {
            foreach (Lazy<IExecutionEngine, IExecutionEngineData> pair in m_ExecutionEngineImplementations)
                pair.Value?.SetValue(name, value);
        }

        /// <summary>
        /// Remove a variable from each of the execution engine scopes.
        /// </summary>
        /// <param name="name">Variable name</param>
        public void RemoveExecutionValue(string name)
        {
            foreach (Lazy<IExecutionEngine, IExecutionEngineData> pair in m_ExecutionEngineImplementations)
                pair.Value?.RemoveValue(name);
        }

        #endregion

        #region Active Macro

        /// <summary>
        /// Gets the active macro
        /// </summary>
        /// <returns>Active macro</returns>
        public FileDeclaration GetActiveFile()
        {
            return s_Instance.m_ActiveFile;
        }

        /// <summary>
        /// Sets the active macro
        /// </summary>
        /// <param name="macro">The macro</param>
        public void SetActiveFile(FileDeclaration d)
        {
            s_Instance.m_ActiveFile = d;
            Events.InvokeEvent("ActiveMacroChanged");
        }

        #endregion

        #region Assemblies

        /// <summary>
        /// Adds an assembly to the assembly list
        /// </summary>
        /// <param name="ad">AssemblyDeclaration of assembly</param>
        public void AddAssembly(AssemblyDeclaration ad)
        {
            m_Assemblies.Add(ad);
            Events.InvokeEvent("OnAssembliesChanged");

            foreach (Lazy<IExecutionEngine, IExecutionEngineData> pair in m_ExecutionEngineImplementations)
                pair.Value?.AddAssembly(ad);
        }

        /// <summary>
        /// Removes an assembly from the assembly list
        /// </summary>
        /// <param name="ad">AssemblyDeclaration of the assembly</param>
        public void RemoveAssembly(AssemblyDeclaration ad)
        {
            m_Assemblies.Remove(ad);
            Events.InvokeEvent("OnAssembliesChanged");

            foreach (Lazy<IExecutionEngine, IExecutionEngineData> pair in m_ExecutionEngineImplementations)
                pair.Value?.RemoveAssembly(ad);


        }

        /// <summary>
        /// Gets the list of Assemblies
        /// </summary>
        /// <returns>Gets list (HashSet) of assemblies</returns>
        public HashSet<AssemblyDeclaration> GetAssemblies()
        {
            return m_Assemblies;
        }

        /// <summary>
        /// Gets an AssemblyDeclaration from its longname
        /// </summary>
        /// <param name="longname">An assembly's longname</param>
        /// <returns>The respective AssemblyDeclaration</returns>
        public AssemblyDeclaration GetAssemblyByName(string name)
        {
            foreach (AssemblyDeclaration ad in m_Assemblies)
                if (ad.Name == name)
                    return ad;

            return null;
        }

        #endregion

        #region Get FileDeclarations

        /// <summary>
        /// Gets the list of macro objects
        /// </summary>
        /// <returns>Dictionary of Macros and their respective IDs</returns>
        public HashSet<FileDeclaration> GetFileDeclarations()
        {
            return m_Files;
        }

        /// <summary>
        /// Gets the macro for the given fullname
        /// </summary>
        /// <param name="fullname">The macro's fullname (fullpath)</param>
        /// <returns>Th macro</returns>
        public FileDeclaration GetDeclarationFromFullname(string fullname)
        {
            foreach (FileDeclaration macro in m_Files)
                if (fullname.Trim().Equals(macro.Info.FullName.Trim(), StringComparison.OrdinalIgnoreCase))
                    return macro;

            return null;
        }

        #endregion

        #region File & Folder functions

        /// <summary>
        /// Adds a macro to the hashset
        /// </summary>
        /// <param name="declaration">The macro to be added</param>
        public void AddFile(FileDeclaration d)
        {
            m_Files.Add(d);

            Events.InvokeEvent("OnMacroCountChanged");
        }

        /// <summary>
        /// Removes a macro from the hashset
        /// </summary>
        /// <param name="declaration">The macro the be removed</param>
        public void RemoveFile(FileDeclaration d)
        {
            m_Files.Remove(d);
            d.Remove();

            Events.InvokeEvent("OnMacroCountChanged");
        }

        /// <summary>
        /// Renames a macro
        /// </summary>
        /// <param name="declaration">The macro</param>
        /// <param name="newname">The macro's new name</param>
        public void RenameFile(FileDeclaration d, string newname)
        {
            d.Save();
            d.Rename(newname);
            d.Save();

            Events.InvokeEvent("OnMacroRenamed", d);
        }

        /// <summary>
        /// Renames a folder
        /// </summary>
        /// <param name="info">The folder to be renamed</param>
        /// <param name="newdir">The folder's dname</param>
        /// <returns>A list (HashSet) of affected macros</returns>
        public HashSet<FileDeclaration> RenameFolder(DirectoryInfo info, string newdir)
        {
            IEnumerable<string> oldPaths = info.GetFiles().Select(x => x.FullName);
            bool result = Files.RenameFolder(info, newdir);

            if (!result)
                return new HashSet<FileDeclaration>();

            IEnumerable<string> newPaths = info.GetFiles().Select(x => x.FullName);
            IEnumerable<FileDeclaration> macros = m_Files.Where(x => oldPaths.Contains(x.Info.FullName));

            //foreach(MacroDeclaration md in files)
            //    md.Info = new FileInfo(

            // TODO

            return macros.ToHashSet<FileDeclaration>();
        }

        /// <summary>
        /// Deletes a folder
        /// </summary>
        /// <param name="directory">The directroy</param>
        public async Task<bool> DeleteFolder(DirectoryInfo info)
        {
            IEnumerable<string> oldPaths = info.GetFiles().Select(x => x.FullName);

            bool result = await Files.DeleteFolder(info);

            if (!result)
                return false;

            IEnumerable<FileDeclaration> files = m_Files.Where(x => oldPaths.Contains(x.Info.FullName));
            foreach (FileDeclaration md in files)
                md.Remove();

            return true;
        }

        #endregion

        /*
        #region Event Invocations

        /// <summary>
        /// Fires Show and Focus events
        /// </summary>
        public static void FireShowFocusEvent()
        {
            FireShowEvent();
            FireFocusEvent();
        }

        /// <summary>
        /// Fires Show event
        /// </summary>
        public static void FireShowEvent()
        {
            //Events.OnShownInvoke();
            Events.InvokeEvent("OnShown");
        }

        /// <summary>
        /// Fires Focus event
        /// </summary>
        public static void FireFocusEvent()
        {
            //Events.OnFocusedInvoke();
            Events.InvokeEvent("OnFocused");
        }

        /// <summary>
        /// Fires Hide event
        /// </summary>
        public static void FireHideEvent()
        {
            //Events.OnHiddenInvoke();
            Events.InvokeEvent("OnHidden");
        }

        /// <summary>
        /// Sets host's interactivity state
        /// </summary>
        /// <param name="enabled">Whether or not the host should be set as interactive</param>
        public static void SetInteractive(bool enabled)
        {
            //Events.SetInteractive(enabled);
            Events.InvokeEvent("SetInteractive", enabled);
        }

        #endregion
        */
    }
}
