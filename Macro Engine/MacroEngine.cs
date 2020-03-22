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
    public class MacroEngine
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

        //IO Management
        private Dictionary<string, IExecutionEngineIO> m_IOManagers;

        /// <summary>
        /// Private constructor
        /// </summary>
        /// <param name="dispatcher">Host application UI dispatcher</param>
        private MacroEngine(Dispatcher dispatcher, HostState state)
        {
            s_Instance = this;
            m_HostDispatcher = dispatcher;

            Compose();

            //Events.OnHostExecute += ExecuteOnHost;
            Events.SubscribeEvent("OnHostEvent", (Action<DispatcherPriority, Action>)ExecuteOnHost);

            m_FileManager = new FileManager();

            m_Declarations = new Dictionary<Guid, MacroDeclaration>();
            m_Macros = new Dictionary<Guid, IMacro>();

            m_RibbonMacros = new HashSet<Guid>();
            m_RibbonMacroPaths = state.RibbonMacros;

            m_IOManagers = new Dictionary<string, IExecutionEngineIO>();
        }

        /// <summary>
        /// Public instantiation of MacroEngine
        /// </summary>
        /// <param name="hostDispatcher">Host application UI dispatcher</param>
        /// <param name="state">Host application state; ribbon macros, active macro, assemblies and workspaces</param>
        /// <param name="OnLoaded">Action to be fired once MacroEngine is fully initialized</param>
        /// <returns>The initialization task thread</returns>
        public static CancellationTokenSource Instantiate(Dispatcher hostDispatcher, HostState state, Action OnLoaded)
        {
            if (GetInstance() != null)
                System.Diagnostics.Debug.WriteLine("Overriding existing MacroEngine!");

            CancellationTokenSource cts = new CancellationTokenSource();
            Task init = Task.Run(() =>
            {
                MacroEngine me = new MacroEngine(hostDispatcher, state);

                //Load all macros in workspaces
                Dictionary<MacroDeclaration, IMacro> macros = new Dictionary<MacroDeclaration, IMacro>();//FileManager.LoadAllMacros(state.Workspaces);
                foreach(KeyValuePair<MacroDeclaration, IMacro> pair in macros)
                {
                    Guid id = Guid.NewGuid();

                    pair.Key.ID = id;
                    pair.Value.ID = id;

                    GetInstance().m_Declarations.Add(id, pair.Key);
                    GetInstance().m_Macros.Add(id, pair.Value);
                }

                Events.OnMacroCountChangedInvoke();

                //Get the active macro
                if (!String.IsNullOrEmpty(state.ActiveMacro))
                    GetInstance().m_ActiveMacro = GetIDFromRelativePath(state.ActiveMacro);
                else
                    GetInstance().m_ActiveMacro = GetInstance().m_Macros.Keys.FirstOrDefault<Guid>();

                //Get Assemblies
                if (state.Assemblies != null)
                    GetInstance().m_Assemblies = new HashSet<AssemblyDeclaration>(state.Assemblies);
                else
                    GetInstance().m_Assemblies = new HashSet<AssemblyDeclaration>();

                Events.OnAssembliesChangedInvoke();

                hostDispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => {
                    OnLoaded?.Invoke();
                    Events.OnLoadedInvoke();
                }));
            }, cts.Token);

            return cts;
        }

        /// <summary>
        /// Fires OnDestroyed event
        /// </summary>
        public static void Destroy()
        {
            Events.OnDestroyedInvoke();
        }

        #endregion



        #region Execution Engine & IO

        /// <summary>
        /// Get the appropriate IExecutionEngine implementation for the given language
        /// </summary>
        /// <param name="runtime">The specified runtime</param>
        /// <returns>IExecutionEngine of the langauge</returns>
        public static IExecutionEngine GetExecutionEngine(string runtime)
        {
            try
            {
                string value = runtime.ToLower().Trim();
                foreach (Lazy<IExecutionEngine, IExecutionEngineData> engine in GetInstance().m_ExecutionEngineImplementations)
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
        public static List<string> GetRuntimes(string language)
        {
            List<string> runtimes = new List<string>();

            try
            {
                string value = language.ToLower().Trim();
                foreach (Lazy<IExecutionEngine, IExecutionEngineData> engine in GetInstance().m_ExecutionEngineImplementations)
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
        public static string GetLangauge(string runtime)
        {
            try
            {
                string value = runtime.ToLower().Trim();
                foreach (Lazy<IExecutionEngine, IExecutionEngineData> engine in GetInstance().m_ExecutionEngineImplementations)
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
        public static string GetLangaugeFromFileExt(string fileExt)
        {
            try
            {
                string value = fileExt.ToLower().Trim();
                foreach (Lazy<IExecutionEngine, IExecutionEngineData> engine in GetInstance().m_ExecutionEngineImplementations)
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
        public static string GetFileExt(string runtime)
        {
            try
            {
                string value = runtime.ToLower().Trim();
                foreach (Lazy<IExecutionEngine, IExecutionEngineData> engine in GetInstance().m_ExecutionEngineImplementations)
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
        /// <returns>FileExt of the runtime</returns>
        public static string GetDefaultRuntime()
        {
            Lazy<IExecutionEngine, IExecutionEngineData> engine = GetInstance().m_ExecutionEngineImplementations.FirstOrDefault<Lazy<IExecutionEngine, IExecutionEngineData>>();
            if (engine.Value == null)
                return "";

            return GetFileExt(engine.Metadata.Runtime);
        }

        /// <summary>
        /// Sets the TextWriters for both output and error of the execution engines
        /// </summary>
        /// <param name="runtime">Runtime for which the IO is associated</param>
        /// <param name="output">TextWriter for ouput stream</param>
        /// <param name="error">TextWriter for error stream</param>
        /// <param name="input">TextReader for inputstream</param>
        public static void SetIOStreams(string runtime, TextWriter output, TextWriter error, TextReader input = null)
        {
            IExecutionEngineIO manager = new ExecutionEngineIO().SetStreams(output, error, input);

            if (string.IsNullOrEmpty(runtime))
                foreach (string key in GetInstance().m_IOManagers.Keys)
                    GetInstance().m_IOManagers[key] = manager;

            else if (GetInstance().m_IOManagers.ContainsKey(runtime))
                GetInstance().m_IOManagers[runtime] = manager;

            else
                GetInstance().m_IOManagers.Add(runtime, manager);

           Events.OnIOChangedInvoke(runtime, GetInstance().m_IOManagers[runtime]);
        }

        /// <summary>
        /// Gets the ExecutionEngineIO instance
        /// </summary>
        /// <param name="runtime">The runtime whose IO manager to get</param>
        /// <returns>IO Manager for specified language</returns>
        public static IExecutionEngineIO GetEngineIOManager(string runtime)
        {
            if (GetInstance().m_IOManagers.ContainsKey(runtime))
                return GetInstance().m_IOManagers[runtime];

            return null;
        }

        #endregion

        #region Ribbon & Active Macros

        /// <summary>
        /// Loads all ribbon macros from serialized list
        /// </summary>
        public static void LoadRibbonMacros()
        {
            GetInstance().m_RibbonMacros.Clear();

            foreach (string file in GetInstance().m_RibbonMacroPaths)
                AddRibbonMacro(GetIDFromRelativePath(file));
        }

        /// <summary>
        /// Gets the ID of the active macro
        /// </summary>
        /// <returns>ID of active macro</returns>
        public static Guid GetActiveMacro()
        {
            return s_Instance.m_ActiveMacro;
        }

        /// <summary>
        /// Sets the active macro
        /// </summary>
        /// <param name="macro">The macro's ID</param>
        public static void SetActiveMacro(Guid macro)
        {
            s_Instance.m_ActiveMacro = macro;
        }

        /// <summary>
        /// Checks if a macro is ribbon accessible 
        /// </summary>
        /// <param name="id">The macro's id</param>
        /// <returns></returns>
        public static bool IsRibbonMacro(Guid id)
        {
            return GetInstance().m_RibbonMacros.Contains(id);
        }

        /// <summary>
        /// Adds a macro to the ribbon
        /// </summary>
        /// <param name="id">The macro's id</param>
        public static void AddRibbonMacro(Guid id)
        {
            if (id == Guid.Empty || IsRibbonMacro(id))
                return;

            GetInstance().m_RibbonMacros.Add(id);

            MacroDeclaration md = GetInstance().m_Declarations[id];
            IMacro macro = GetInstance().m_Macros[id];

            Events.AddRibbonMacro(id, md.Name, md.RelativePath, () => macro.Execute(null, false));
        }

        /// <summary>
        /// Removes a macro from the ribbon
        /// </summary>
        /// <param name="id">The macro's id</param>
        public static void RemoveRibbonMacro(Guid id)
        {
            GetInstance().m_RibbonMacros.Remove(id);
            Events.RemoveRibbonMacro(id);
        }

        /// <summary>
        /// Renames a ribbon macro
        /// </summary>
        /// <param name="id">The macro's id</param>
        public static void RenameRibbonMacro(Guid id)
        {
            GetInstance().m_RibbonMacros.Add(id);

            MacroDeclaration md = GetInstance().m_Declarations[id];
            Events.RenameRibbonMacro(id, md.Name, md.RelativePath);
        }

        #endregion

        #region Assemblies

        /// <summary>
        /// Adds an assembly to the assembly list
        /// </summary>
        /// <param name="ad">AssemblyDeclaration of assembly</param>
        public static void AddAssembly(AssemblyDeclaration ad)
        {
            GetInstance().m_Assemblies.Add(ad);
            Events.OnAssembliesChangedInvoke();
        }

        /// <summary>
        /// Removes an assembly from the assembly list
        /// </summary>
        /// <param name="ad">AssemblyDeclaration of the assembly</param>
        public static void RemoveAssembly(AssemblyDeclaration ad)
        {
            GetInstance().m_Assemblies.Remove(ad);
            Events.OnAssembliesChangedInvoke();
        }

        /// <summary>
        /// Gets the list of Assemblies
        /// </summary>
        /// <returns>Gets list (HashSet) of assemblies</returns>
        public static HashSet<AssemblyDeclaration> GetAssemblies()
        {
            return GetInstance().m_Assemblies;
        }

        /// <summary>
        /// Gets an AssemblyDeclaration from its longname
        /// </summary>
        /// <param name="longname">An assembly's longname</param>
        /// <returns>The respective AssemblyDeclaration</returns>
        public static AssemblyDeclaration GetAssemblyByLongName(string longname)
        {
            foreach (AssemblyDeclaration ad in GetInstance().m_Assemblies)
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
        public static Dictionary<Guid, MacroDeclaration> GetDeclarations()
        {
            return GetInstance().m_Declarations;
        }

        /// <summary>
        /// Gets the list of macro objects
        /// </summary>
        /// <returns>Dictionary of Macros and their respective IDs</returns>
        public static Dictionary<Guid, IMacro> GetMacros()
        {
            return GetInstance().m_Macros;
        }

        /// <summary>
        /// Gets a macro by its id
        /// </summary>
        /// <param name="id">The macro's id</param>
        /// <returns>Macro of the given id</returns>
        public static IMacro GetMacro(Guid id)
        {
            if (!GetInstance().m_Macros.ContainsKey(id))
                return null;

            return GetInstance().m_Macros[id];
        }

        /// <summary>
        /// Gets a MacroDeclaration from a macro's id
        /// </summary>
        /// <param name="id">The macro's id</param>
        /// <returns>MacroDeclaration of the given id</returns>
        public static MacroDeclaration GetDeclaration(Guid id)
        {
            if (!GetInstance().m_Declarations.ContainsKey(id))
                return null;

            return GetInstance().m_Declarations[id];
        }

        /// <summary>
        /// Gets the ID of a macro from it's relative path
        /// </summary>
        /// <param name="relativepath">The macro's relative path</param>
        /// <returns>Guid of the macro</returns>
        public static Guid GetIDFromRelativePath(string relativepath)
        {
            string path = relativepath.ToLower().Trim();

            foreach (MacroDeclaration macro in GetInstance().m_Declarations.Values)
                if (macro.RelativePath.ToLower().Trim() == path)
                    return macro.ID;

            return Guid.Empty;
        }

        /// <summary>
        /// Sets the MacroDeclaration associated with an ID
        /// </summary>
        /// <param name="id">The macro's id</param>
        /// <param name="declaration">The macro's MacroDeclaration</param>
        public static void SetDeclaration(Guid id, MacroDeclaration declaration)
        {
            if (!GetInstance().m_Declarations.ContainsKey(id))
                GetInstance().m_Declarations.Add(id, declaration);
            else
                GetInstance().m_Declarations[id] = declaration;
        }

        #endregion

        #region File & Folder functions

        /// <summary>
        /// Adds a macro to the registry
        /// </summary>
        /// <param name="declaration">The macro's macro declaration</param>
        /// <param name="macro">The macro</param>
        /// <returns>The macro's assigned ID</returns>
        public static Guid AddMacro(MacroDeclaration declaration, IMacro macro)
        {
            Guid id = Guid.NewGuid();

            declaration.ID = id;
            macro.ID = id;

            GetInstance().m_Declarations.Add(id, declaration);
            GetInstance().m_Macros.Add(id, macro);
            Events.OnMacroCountChangedInvoke();

            return id;
        }

        /// <summary>
        /// Removes a macro from the registry
        /// </summary>
        /// <param name="id">The macro's id</param>
        public static void RemoveMacro(Guid id)
        {
            GetInstance().m_Macros.Remove(id);
            Events.OnMacroCountChangedInvoke();
        }

        /// <summary>
        /// Renames a macro
        /// </summary>
        /// <param name="id">The macro's id</param>
        /// <param name="newname">The macro's new name</param>
        public static void RenameMacro(Guid id, string newname)
        {
            if (!GetInstance().m_Macros.ContainsKey(id))
            {
                Messages.DisplayOkMessage("Could not find the macro: " + GetDeclaration(id).Name, "Rename Macro Error");
                return;
            }

            IMacro macro = GetInstance().m_Macros[id];

            macro.Save();
            macro.Rename(newname);
            macro.Save();

            Events.OnMacroRenamedInvoke(id);
        }

        /// <summary>
        /// Renames a folder
        /// </summary>
        /// <param name="olddir">The folder's current relative path</param>
        /// <param name="newdir">The folder's desired relative path</param>
        /// <returns>A list (HashSet) of ids of effected macros</returns>
        public static HashSet<Guid> RenameFolder(string olddir, string newdir)
        {
            HashSet<Guid> affectedMacros = new HashSet<Guid>();

            FileManager.RenameFolder(olddir, newdir);
            string relativepath = FileManager.CalculateRelativePath(FileManager.CalculateFullPath(olddir));

            foreach (Guid id in GetInstance().m_Declarations.Keys)
            {
                if (GetDeclaration(id).RelativePath.ToLower().Trim().StartsWith(relativepath.ToLower().Trim()))
                {
                    affectedMacros.Add(id);
                    GetInstance().m_Declarations[id].RelativePath = GetDeclaration(id).RelativePath.Replace(relativepath, FileManager.CalculateRelativePath(FileManager.CalculateFullPath(newdir)));
                }
            }

            return affectedMacros;
        }

        /// <summary>
        /// Deletes a folder
        /// </summary>
        /// <param name="directory">The relative path of the folder</param>
        /// <param name="OnReturn">The Action, a bool representing the operations success, to be fired when the task is completed</param>
        public static void DeleteFolder(string directory, Action<bool> OnReturn)
        {
            HashSet<Guid> affectedMacros = new HashSet<Guid>();

            FileManager.DeleteFolder(directory, new Action<bool>((result) =>
            {
                if (!result)
                    OnReturn?.Invoke(false);

                string relativepath = FileManager.CalculateRelativePath(FileManager.CalculateFullPath(directory)).ToLower().Trim();

                HashSet<Guid> toremove = new HashSet<Guid>();
                foreach (Guid id in GetInstance().m_Declarations.Keys)
                    if (GetDeclaration(id).RelativePath.ToLower().Trim().Contains(relativepath))
                        toremove.Add(id);

                foreach (Guid id in toremove)
                    GetInstance().m_Declarations.Remove(id);

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
            Events.OnShownInvoke();
        }

        /// <summary>
        /// Fires Focus event
        /// </summary>
        public static void FireFocusEvent()
        {
            Events.OnFocusedInvoke();
        }

        /// <summary>
        /// Fires Hide event
        /// </summary>
        public static void FireHideEvent()
        {
            Events.OnHiddenInvoke();
        }

        /// <summary>
        /// Sets host's interactivity state
        /// </summary>
        /// <param name="enabled">Whether or not the host should be set as interactive</param>
        public static void SetInteractive(bool enabled)
        {
            Events.SetInteractive(enabled);
        }

        #endregion
    }
}
