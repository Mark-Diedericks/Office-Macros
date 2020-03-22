using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Macro_Engine
{
    public class Events
    {

        #region Instance

        private static Events s_Instance;
        private static Events Instance
        {
            get
            {
                if (s_Instance == null)
                    return new Events();

                return s_Instance;
            }
        }

        private Dictionary<string, Delegate> m_Events;

        private Events()
        {
            s_Instance = this;
            m_Events = new Dictionary<string, Delegate>();
        }

        #endregion

        public static void SubscribeEvent(string name, Delegate callback)
        {
            try
            {
                if (Instance.m_Events.ContainsKey(name))
                    Instance.m_Events[name] = Delegate.Combine(Instance.m_Events[name], callback);
                else
                    Instance.m_Events.Add(name, callback);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }
        public static void UnsubscribeEvent(string name, Delegate callback)
        {
            try
            {
                if (Instance.m_Events.ContainsKey(name))
                    Instance.m_Events[name] = Delegate.Remove(Instance.m_Events[name], callback);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        public static void InvokeEvent(string name, object[] args = null)
        {
            try
            {
                if (args == null)
                    args = new object[] { };

                if (Instance.m_Events.ContainsKey(name))
                    Instance.m_Events[name]?.DynamicInvoke(args);
                else
                    System.Diagnostics.Debug.WriteLine("Tried to invoke non-existent event: " + name);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }
    }
}
