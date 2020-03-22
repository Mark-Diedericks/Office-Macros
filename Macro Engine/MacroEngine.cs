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

        private readonly Dispatcher m_HostDispatcher;

        /// <summary>
        /// Gets host office application UI dispatcher
        /// </summary>
        /// <returns>Office application UI dispatcher</returns>
        public static Dispatcher GetHostDispatcher()
        {
            return GetInstance().m_HostDispatcher;
        }

        /// <summary>
        /// Execute a task on the host dispatcher with a given priority
        /// </summary>
        /// <param name="priority">The disptacher priority to use</param>
        /// <param name="task">Task to be executed</param>
        private static void ExecuteOnHost(DispatcherPriority priority, Action task)
        {
            GetHostDispatcher()?.BeginInvoke(priority, task);
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

        private readonly FileManager m_FileManager;

        public static FileManager GetFileManager()
        {
            return GetInstance().m_FileManager;
        }

        #endregion

        #region Initialization & Destruction

        //Temporary path storage
        private string[] m_RibbonMacroPaths;

        //Macros
        private Dictionary<Guid, MacroDeclaration> m_Declarations;
        private Dictionary<Guid, IMacro> m_Macros;
        private HashSet<Guid> m_RibbonMacros;
        private Guid m_ActiveMacro;

        //User Included Assemblies
        private HashSet<AssemblyDeclaration> m_Assemblies;

        //All runtimes
        private HashSet<string> m_Runtimes;

        /// <summary>
        /// Private constructor
        /// </summary>
        /// <param name="dispatcher">Host application UI dispatcher</param>
        private MacroEngine(Dispatcher dispatcher)
        {
            s_Instance = this;
            m_HostDispatcher = dispatcher;

            Compose();

            m_Runtimes = new HashSet<string>();
            foreach (Lazy<IExecutionEngine, IExecutionEngineData> pair in GetInstance().m_ExecutionEngineImplementations)
                m_Runtimes.Add(pair.Metadata.Runtime);

            Events.SubscribeEvent("OnHostExecute", (Action<DispatcherPriority, Action>)ExecuteOnHost);
            Events.SubscribeEvent("SetIO", (Action<string, TextWriter, TextWriter, TextReader>)SetIOStreams);
            Events.SubscribeEvent("RibbonLoaded", (Action)LoadRibbonMacros);
            Events.SubscribeEvent("LoadRibbonMacros", (Action)LoadRibbonMacros);

            Events.SubscribeEvent("OnTerminateExecution", new Action(() => {
                foreach (Lazy<IExecutionEngine, IExecutionEngineData> pair in GetInstance().m_ExecutionEngineImplementations)
                    pair.Value?.TerminateExecution();
            }));

            m_FileManager = new FileManager();

            m_Declarations = new Dictionary<Guid, MacroDeclaration>();
            m_Macros = new Dictionary<Guid, IMacro>();

            m_RibbonMacros = new HashSet<Guid>();
            m_RibbonMacroPaths = new string[] { };
        }

        private void SetState(HostState state)
        {
            m_ActiveMacro = GetIDFromRelativePath(state.ActiveMacro);

            m_RibbonMacroPaths = state.RibbonMacros;

            //Get Assemblies
            if (state.Assemblies != null)
                m_Assemblies = new HashSet<AssemblyDeclaration>(state.Assemblies);
            else
                m_Assemblies = new HashSet<AssemblyDeclaration>();
        }

        /// <summary>
        /// Public instantiation of MacroEngine
        /// </summary>
        /// <param name="hostDispatcher">Host application UI dispatcher</param>
        /// <param name="state">Host application state; ribbon macros, active macro, assemblies and workspaces</param>
        /// <param name="OnLoaded">Action to be fired once MacroEngine is fully initialized</param>
        /// <returns>The initialization task thread</returns>
        public CancellationTokenSource Instantiate(HostState state, Action OnLoaded)
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            Task init = Task.Run(() =>
            {
                SetState(state);

                //Load all macros in workspaces
                Dictionary<MacroDeclaration, IMacro> macros = FileManager.LoadAllMacros(state.Workspaces);
                foreach(KeyValuePair<MacroDeclaration, IMacro> pair in macros)
                {
                    Guid id = Guid.NewGuid();

                    pair.Key.ID = id;
                    pair.Value.ID = id;

                    m_Declarations.Add(id, pair.Key);
                    m_Macros.Add(id, pair.Value);
                }

                //Events.OnMacroCountChangedInvoke();
                Events.InvokeEvent("OnMacroCountChanged");

                //Get the active macro
                if (!String.IsNullOrEmpty(state.ActiveMacro))
                    m_ActiveMacro = GetIDFromRelativePath(state.ActiveMacro);
                else
                    m_ActiveMacro = m_Macros.Keys.FirstOrDefault<Guid>();


                //Events.OnAssembliesChangedInvoke();
                Events.InvokeEvent("OnAssembliesChanged");

                GetHostDispatcher().BeginInvoke(DispatcherPriority.Normal, new Action(() => {
                    OnLoaded?.Invoke();
                }));
            }, cts.Token);

            return cts;
        }

        /// <summary>
        /// Fires OnDestroyed event
        /// </summary>
        public void Destroy()
        {
            Events.InvokeEvent("OnDestroyed");
            foreach (Lazy<IExecutionEngine, IExecutionEngineData> pair in GetInstance().m_ExecutionEngineImplementations)
                pair.Value?.Destroy();
        }

        public static MacroEngine CreateApplicationInstance(Dispatcher dispatcher)
        {
            return new MacroEngine(dispatcher);
        }

        #endregion



        #region Execution Engine & IO

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

        #endregion

        #region Ribbon & Active Macros

        /// <summary>
        /// Loads all ribbon macros from serialized list
        /// </summary>
        public void LoadRibbonMacros()
        {
            m_RibbonMacros.Clear();

            foreach (string file in m_RibbonMacroPaths)
                AddRibbonMacro(GetIDFromRelativePath(file));
        }

        /// <summary>
        /// Gets the ID of the active macro
        /// </summary>
        /// <returns>ID of active macro</returns>
        public Guid GetActiveMacro()
        {
            return s_Instance.m_ActiveMacro;
        }

        /// <summary>
        /// Sets the active macro
        /// </summary>
        /// <param name="macro">The macro's ID</param>
        public void SetActiveMacro(Guid macro)
        {
            s_Instance.m_ActiveMacro = macro;
            Events.InvokeEvent("ActiveMacroChanged");
        }

        /// <summary>
        /// Checks if a macro is ribbon accessible 
        /// </summary>
        /// <param name="id">The macro's id</param>
        /// <returns></returns>
        public bool IsRibbonMacro(Guid id)
        {
            return m_RibbonMacros.Contains(id);
        }

        /// <summary>
        /// Adds a macro to the ribbon
        /// </summary>
        /// <param name="id">The macro's id</param>
        public void AddRibbonMacro(Guid id)
        {
            if (id == Guid.Empty || IsRibbonMacro(id))
                return;

            m_RibbonMacros.Add(id);

            MacroDeclaration md = m_Declarations[id];
            IMacro macro = m_Macros[id];

            //Events.AddRibbonMacro(id, md.Name, md.RelativePath, () => macro.Execute(null, false));
            Events.InvokeEvent("AddRibbonMacro", new object[] { id, md.Name, md.RelativePath, new Action(() => macro.Execute(null, false)) });
        }

        /// <summary>
        /// Removes a macro from the ribbon
        /// </summary>
        /// <param name="id">The macro's id</param>
        public void RemoveRibbonMacro(Guid id)
        {
            m_RibbonMacros.Remove(id);
            //Events.RemoveRibbonMacro(id);
            Events.InvokeEvent("RemoveRibbonMacro", new object[] { id });
        }

        /// <summary>
        /// Renames a ribbon macro
        /// </summary>
        /// <param name="id">The macro's id</param>
        public void RenameRibbonMacro(Guid id)
        {
            m_RibbonMacros.Add(id);

            MacroDeclaration md = m_Declarations[id];
            //Events.RenameRibbonMacro(id, md.Name, md.RelativePath);
            Events.InvokeEvent("RenameRibbonMacro", new object[] { id, md.Name, md.RelativePath });
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
            //Events.OnAssembliesChangedInvoke();
            Events.InvokeEvent("OnAssembliesChanged");
        }

        /// <summary>
        /// Removes an assembly from the assembly list
        /// </summary>
        /// <param name="ad">AssemblyDeclaration of the assembly</param>
        public void RemoveAssembly(AssemblyDeclaration ad)
        {
            m_Assemblies.Remove(ad);
            //Events.OnAssembliesChangedInvoke();
            Events.InvokeEvent("OnAssembliesChanged");
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
        public AssemblyDeclaration GetAssemblyByLongName(string longname)
        {
            foreach (AssemblyDeclaration ad in m_Assemblies)
                if (ad.filepath == longname)
                    return ad;

            return null;
        }

        #endregion

        #region Macro & Declarations

        /// <summary>
        /// Gets the list of macro declarations
        /// </summary>
        /// <returns>Dictionary of MacroDeclarations to their respective IDs</returns>
        public Dictionary<Guid, MacroDeclaration> GetDeclarations()
        {
            return m_Declarations;
        }

        /// <summary>
        /// Gets the list of macro objects
        /// </summary>
        /// <returns>Dictionary of Macros and their respective IDs</returns>
        public Dictionary<Guid, IMacro> GetMacros()
        {
            return m_Macros;
        }

        /// <summary>
        /// Gets a macro by its id
        /// </summary>
        /// <param name="id">The macro's id</param>
        /// <returns>Macro of the given id</returns>
        public IMacro GetMacro(Guid id)
        {
            if (!m_Macros.ContainsKey(id))
                return null;

            return m_Macros[id];
        }

        /// <summary>
        /// Gets a MacroDeclaration from a macro's id
        /// </summary>
        /// <param name="id">The macro's id</param>
        /// <returns>MacroDeclaration of the given id</returns>
        public MacroDeclaration GetDeclaration(Guid id)
        {
            if (!m_Declarations.ContainsKey(id))
                return null;

            return m_Declarations[id];
        }

        /// <summary>
        /// Gets the ID of a macro from it's relative path
        /// </summary>
        /// <param name="relativepath">The macro's relative path</param>
        /// <returns>Guid of the macro</returns>
        public Guid GetIDFromRelativePath(string relativepath)
        {
            string path = relativepath.ToLower().Trim();

            foreach (MacroDeclaration macro in m_Declarations.Values)
                if (macro.RelativePath.ToLower().Trim() == path)
                    return macro.ID;

            return Guid.Empty;
        }

        /// <summary>
        /// Sets the MacroDeclaration associated with an ID
        /// </summary>
        /// <param name="id">The macro's id</param>
        /// <param name="declaration">The macro's MacroDeclaration</param>
        public void SetDeclaration(Guid id, MacroDeclaration declaration)
        {
            if (!m_Declarations.ContainsKey(id))
                m_Declarations.Add(id, declaration);
            else
                m_Declarations[id] = declaration;
        }

        #endregion

        #region File & Folder functions

        /// <summary>
        /// Adds a macro to the registry
        /// </summary>
        /// <param name="declaration">The macro's macro declaration</param>
        /// <param name="macro">The macro</param>
        /// <returns>The macro's assigned ID</returns>
        public Guid AddMacro(MacroDeclaration declaration, IMacro macro)
        {
            Guid id = Guid.NewGuid();

            declaration.ID = id;
            macro.ID = id;

            m_Declarations.Add(id, declaration);
            m_Macros.Add(id, macro);
            //Events.OnMacroCountChangedInvoke();
            Events.InvokeEvent("OnMacroCountChanged");

            return id;
        }

        /// <summary>
        /// Removes a macro from the registry
        /// </summary>
        /// <param name="id">The macro's id</param>
        public void RemoveMacro(Guid id)
        {
            m_Macros.Remove(id);
            //Events.OnMacroCountChangedInvoke();
            Events.InvokeEvent("OnMacroCountChanged");
        }

        /// <summary>
        /// Renames a macro
        /// </summary>
        /// <param name="id">The macro's id</param>
        /// <param name="newname">The macro's new name</param>
        public void RenameMacro(Guid id, string newname)
        {
            if (!m_Macros.ContainsKey(id))
            {
                Messages.DisplayOkMessage("Could not find the macro: " + GetDeclaration(id).Name, "Rename Macro Error");
                return;
            }

            IMacro macro = m_Macros[id];

            macro.Save();
            macro.Rename(newname);
            macro.Save();

            //Events.OnMacroRenamedInvoke(id);
            Events.InvokeEvent("OnMacroRenamed", new object[] { id });
        }

        /// <summary>
        /// Renames a folder
        /// </summary>
        /// <param name="olddir">The folder's current relative path</param>
        /// <param name="newdir">The folder's desired relative path</param>
        /// <returns>A list (HashSet) of ids of effected macros</returns>
        public HashSet<Guid> RenameFolder(string olddir, string newdir)
        {
            HashSet<Guid> affectedMacros = new HashSet<Guid>();

            FileManager.RenameFolder(olddir, newdir);
            string relativepath = FileManager.CalculateRelativePath(FileManager.CalculateFullPath(olddir));

            foreach (Guid id in m_Declarations.Keys)
            {
                if (GetDeclaration(id).RelativePath.ToLower().Trim().StartsWith(relativepath.ToLower().Trim()))
                {
                    affectedMacros.Add(id);
                    m_Declarations[id].RelativePath = GetDeclaration(id).RelativePath.Replace(relativepath, FileManager.CalculateRelativePath(FileManager.CalculateFullPath(newdir)));
                }
            }

            return affectedMacros;
        }

        /// <summary>
        /// Deletes a folder
        /// </summary>
        /// <param name="directory">The relative path of the folder</param>
        /// <param name="OnReturn">The Action, a bool representing the operations success, to be fired when the task is completed</param>
        public void DeleteFolder(string directory, Action<bool> OnReturn)
        {
            HashSet<Guid> affectedMacros = new HashSet<Guid>();

            FileManager.DeleteFolder(directory, new Action<bool>((result) =>
            {
                if (!result)
                    OnReturn?.Invoke(false);

                string relativepath = FileManager.CalculateRelativePath(FileManager.CalculateFullPath(directory)).ToLower().Trim();

                HashSet<Guid> toremove = new HashSet<Guid>();
                foreach (Guid id in m_Declarations.Keys)
                    if (GetDeclaration(id).RelativePath.ToLower().Trim().Contains(relativepath))
                        toremove.Add(id);

                foreach (Guid id in toremove)
                    m_Declarations.Remove(id);

                OnReturn?.Invoke(true);
            }));
        }

        #endregion

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
            Events.InvokeEvent("SetInteractive", new object[] { enabled });
        }

        #endregion
    }
}
