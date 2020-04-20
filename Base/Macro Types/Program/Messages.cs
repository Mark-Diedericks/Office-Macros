using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Macro_Engine
{
    public class Messages
    {
        
        //VoidMessage event, for all Forms and GUIs
        public delegate void VoidMessageEvent(string content, string title);
        public static event VoidMessageEvent DisplayOkMessageEvent;

        //ObjectMessage event, for all Forms and GUIs
        public delegate bool ObjectMessageReturnEvent(string content, string title);
        public static event ObjectMessageReturnEvent DisplayYesNoMessageReturnEvent;

        //InputMessage event, for all Forms and GUIs
        public delegate object InputMessageReturnEvent(string message, object title, object def, object left, object top, object helpFile, object helpContextID, object type);
        public static event InputMessageReturnEvent DisplayInputMessageReturnEvent;

        /// <summary>
        /// Fires the DisplayOkMessage event
        /// </summary>
        /// <param name="content">The message to be displayed</param>
        /// <param name="title">The message's header</param>
        public static void DisplayOkMessage(string content, string title)
        {
            DisplayOkMessageEvent?.Invoke(content, title);
        }

        /// <summary>
        /// Fires the DisplayYesNOMessageReturn event
        /// </summary>
        /// <param name="content">The message to be displayed</param>
        /// <param name="title">The message's header</param>
        /// <returns>The bool result of the user's action</returns>
        public static Task<bool> DisplayYesNoMessage(string content, string title)
        {
            return Task.Run(new Func<bool>(() =>
            {
                bool? res = DisplayYesNoMessageReturnEvent?.Invoke(content, title);
                return res.HasValue ? res.Value : false;
            }));
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
        public static Task<object> DisplayInputMessage(string message, object title, object def, object left, object top, object helpFile, object helpContextID, object type)
        {
            return Task.Run(new Func<object>(() =>
            {
                return DisplayInputMessageReturnEvent?.Invoke(message, title, def, left, top, helpFile, helpContextID, type);
            }));
        }
    }
}
