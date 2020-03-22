using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Macro_Engine.Engine
{
    public class HighPrecisionTimer
    {
        //
        // From:     CodeProject
        // Author:   Daniel Strigl
        // Link:     https://www.codeproject.com/Articles/2635/High-Performance-Timer-in-C
        // Lisence:  https://www.codeproject.com/info/cpol10.aspx
        //
        [DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceCounter(out long lpPerformanceCount);

        [DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceFrequency(out long lpFrequency);

        private long startTime, stopTime;
        private long freq;

        // Constructor
        public HighPrecisionTimer()
        {
            startTime = 0;
            stopTime = 0;

            if (QueryPerformanceFrequency(out freq) == false)
            {
                // high-performance counter not supported
                throw new Win32Exception();
            }
        }

        /// <summary>
        /// Start the current timer
        /// </summary>
        public void Start()
        {
            // lets do the waiting threads there work
            Thread.Sleep(0);

            QueryPerformanceCounter(out startTime);
        }

        /// <summary>
        /// Stop the current timer
        /// </summary>
        public void Stop()
        {
            QueryPerformanceCounter(out stopTime);
        }

        /// <summary>
        /// Returns the duration of the timer (in ms)
        /// </summary>
        public double Duration
        {
            get
            {
                return ((double)(stopTime - startTime) / (double)freq) * 1000.0D;
            }
        }
    }
}
