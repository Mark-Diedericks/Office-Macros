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

    }
}
