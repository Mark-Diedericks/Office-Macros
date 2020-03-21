/*
 * Mark Diedericks
 * 02/08/2018
 * Version 1.0.0
 * Base item model
 */

using Macro_Engine.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Macro_UI.Model
{
    /// <summary>
    /// Custom data structure for holding the temporary data of each tree view item fopr macros
    /// </summary>
    public class DataTreeViewItem
    {
        public int level;
        public bool folder;
        public string name;
        public Guid macro;
        public string root;
        public List<DataTreeViewItem> children;
    }
}
