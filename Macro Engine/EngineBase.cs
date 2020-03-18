/* 
 * Mark Diedericks
 * 18/03/2020
 * Entry point of python engine
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using Macro_Engine.Engine;
using Macro_Engine.Interop;

namespace Macro_Engine
{
    public abstract class EngineBase
    {

        #region Office Application

        private readonly object m_OfficeApplication;
        private readonly Dispatcher m_OfficeDispatcher;

        /// <summary>
        /// Gets host office application
        /// </summary>
        /// <returns>Office application</returns>
        public object GetOfficeApplication()
        {
            return m_OfficeApplication;
        }

        /// <summary>
        /// Gets host office application UI dispatcher
        /// </summary>
        /// <returns>Office application UI dispatcher</returns>
        public Dispatcher GetOfficeDispatcher()
        {
            return m_OfficeDispatcher;
        }

        #endregion

        #region Initialization & Getters

        private readonly FileManager m_FileManager;
        private readonly MessageManager m_MessageManager;
        private readonly EventManager m_EventManager;
        private readonly IExecutionEngine m_ExecutionEngine;


        private EngineBase(object application, Dispatcher dispatcher, IExecutionEngine executionEngine)
        {
            m_OfficeApplication = application;
            m_OfficeDispatcher = dispatcher;

            m_FileManager = FileManager.Instantiate(this);
            m_MessageManager = MessageManager.Instantiate(this);
            m_EventManager = EventManager.Instantiate(this);
            m_ExecutionEngine = executionEngine;
        }

        public FileManager GetFileManager()
        {
            return m_FileManager;
        }

        public MessageManager GetMessageManager()
        {
            return m_MessageManager;
        }

        public EventManager GetEventManager()
        {
            return m_EventManager;
        }

        public IExecutionEngine GetExecutionEngine()
        {
            return m_ExecutionEngine;
        }

        #endregion


    }
}
