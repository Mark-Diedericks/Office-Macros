using Macro_Engine.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Macro_Engine
{
    public class Utilities
    {
        private static Utilities s_Instance;
        private Dictionary<int, HighPrecisionTimer> m_DebugSessions;

        /// <summary>
        /// Private instatiation of Utilities 
        /// </summary>
        private Utilities()
        {
            s_Instance = this;
            m_DebugSessions = new Dictionary<int, HighPrecisionTimer>();
        }

        /// <summary>
        /// Gets the instance of Utilities
        /// </summary>
        /// <returns>Utilities instance</returns>
        public static Utilities GetInstance()
        {
            return s_Instance != null ? s_Instance : new Utilities();
        }

        /// <summary>
        /// Public instatiation of Utilities
        /// </summary>
        public static void Instantiate()
        {
            new Utilities();
        }

        /// <summary>
        /// Begins a new profiling session
        /// </summary>
        /// <returns>Profiling session identifier</returns>
        public static int BeginProfileSession()
        {
            int id = GetInstance().m_DebugSessions.Count;
            GetInstance().m_DebugSessions.Add(id, new HighPrecisionTimer());

            //Start debug timer
            GetInstance().m_DebugSessions[id].Start();

            return id;
        }

        /// <summary>
        /// Ends a profiling session
        /// </summary>
        /// <param name="id">Profiling session identifier</param>
        public static void EndProfileSession(int id)
        {
            if (id == -1)
                return;

            if (!GetInstance().m_DebugSessions.ContainsKey(id))
                return;

            //Stop debug timer
            GetInstance().m_DebugSessions[id].Stop();

            GetInstance().m_DebugSessions.Remove(id);
        }

        /// <summary>
        /// Gets the time interval of a profiling session
        /// </summary>
        /// <param name="id">Profiling session identifier</param>
        /// <returns>Time interval in milliseconds</returns>
        public static double GetTimeIntervalMilli(int id)
        {
            if (id == -1)
                return 0.00;

            GetInstance().m_DebugSessions[id].Stop();
            double duration = GetInstance().m_DebugSessions[id].Duration; //Convert from milliseconds to milliseconds
            GetInstance().m_DebugSessions[id].Start();

            return duration;
        }

        /// <summary>
        /// Gets the time interval of the profiling session
        /// </summary>
        /// <param name="id">Profiling session identifier</param>
        /// <returns>Time interval in seconds</returns>
        public static double GetTimeIntervalSeconds(int id)
        {
            if (id == -1)
                return 0.00;

            GetInstance().m_DebugSessions[id].Stop();
            double duration = GetInstance().m_DebugSessions[id].Duration / 1000.0f; //Convert from milliseconds to seconds
            GetInstance().m_DebugSessions[id].Start();

            return duration;
        }
    }
}
