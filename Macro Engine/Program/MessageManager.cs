using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Macro_Engine
{
    public class MessageManager
    {
        private readonly EngineBase m_Engine;
        protected EngineBase GetEngine()
        {
            return m_Engine;
        }

        private MessageManager(EngineBase engine)
        {
            m_Engine = engine;
        }

        public static MessageManager Instantiate(EngineBase engine)
        {
            return new MessageManager(engine);
        }

        //VoidMessage event, for all Forms and GUIs
        public delegate void VoidMessageEvent(string content, string title);
        public event VoidMessageEvent DisplayOkMessageEvent;

        //ObjectMessage event, for all Forms and GUIs
        public delegate void ObjectMessageEvent(string content, string title, Action<bool> OnReturn);
        public event ObjectMessageEvent DisplayYesNoMessageEvent;

        //InputMessage event, for all Forms and GUIs
        public delegate void InputMessageEvent(string message, object title, object def, object left, object top, object helpFile, object helpContextID, object type, Action<object> OnResult);
        public event InputMessageEvent DisplayInputMessageEvent;

        //ObjectMessage event, for all Forms and GUIs
        public delegate bool ObjectMessageReturnEvent(string content, string title);
        public event ObjectMessageReturnEvent DisplayYesNoMessageReturnEvent;

        //InputMessage event, for all Forms and GUIs
        public delegate object InputMessageReturnEvent(string message, object title, object def, object left, object top, object helpFile, object helpContextID, object type);
        public event InputMessageReturnEvent DisplayInputMessageReturnEvent;

        /// <summary>
        /// Fires the DisplayOkMessage event
        /// </summary>
        /// <param name="content">The message to be displayed</param>
        /// <param name="title">The message's header</param>
        public void DisplayOkMessage(string content, string title)
        {
            DisplayOkMessageEvent?.Invoke(content, title);
        }

        /// <summary>
        /// Fires the DisplayYesNOMessage event
        /// </summary>
        /// <param name="content">The message to be displayed</param>
        /// <param name="title">The message's header</param>
        /// <param name="OnReturn">The Action, and bool representation of the yes/no result, to be fired when the user provides input</param>
        public void DisplayYesNoMessage(string content, string title, Action<bool> OnReturn)
        {
            DisplayYesNoMessageEvent?.Invoke(content, title, OnReturn);
        }

        /// <summary>
        /// Display input message asynchronously, forwarding Excel method; VBA Input Box
        /// </summary>
        /// <param name="message"></param>
        /// <param name="title"></param>
        /// <param name="def"></param>
        /// <param name="left"></param>
        /// <param name="top"></param>
        /// <param name="helpFile"></param>
        /// <param name="helpContextID"></param>
        /// <param name="type"></param>
        /// <param name="OnResult">The Action, and returning object, to be fired when the task is completed</param>
        public void DisplayInputMessage(string message, object title, object def, object left, object top, object helpFile, object helpContextID, object type, Action<object> OnResult)
        {
            DisplayInputMessageEvent?.Invoke(message, title, def, left, top, helpFile, helpContextID, type, OnResult);
        }

        /// <summary>
        /// Fires the DisplayYesNOMessageReturn event
        /// </summary>
        /// <param name="content">The message to be displayed</param>
        /// <param name="title">The message's header</param>
        /// <returns>The bool result of the user's action</returns>
        public bool DisplayYesNoMessage(string content, string title)
        {
            bool? res = DisplayYesNoMessageReturnEvent?.Invoke(content, title);
            return res.HasValue ? res.Value : false;
        }

        /// <summary>
        /// Display input message synchronously, forwarding Excel method; VBA Input Box
        /// </summary>
        /// <param name="message"></param>
        /// <param name="title"></param>
        /// <param name="def"></param>
        /// <param name="left"></param>
        /// <param name="top"></param>
        /// <param name="helpFile"></param>
        /// <param name="helpContextID"></param>
        /// <param name="type"></param>
        /// <returns>InputBox's resultant object</returns>
        public object DisplayInputMessage(string message, object title, object def, object left, object top, object helpFile, object helpContextID, object type)
        {
            return DisplayInputMessageReturnEvent?.Invoke(message, title, def, left, top, helpFile, helpContextID, type);
        }
    }
}
