using Macro_Engine;
using Macro_Editor.Model;
using Macro_Editor.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Macro_Editor.Utilities
{
    public class TextBoxReader : TextReader
    {
        private readonly ConsoleViewModel Model;

        //private TextBox m_TextBox;

        private int m_Index;
        private string m_Input;
        private bool m_HasInput;

        private bool m_Abort;

        /// <summary>
        /// Instantiate a TextBoxReader
        /// </summary>
        /// <param name="model">The ConsoleViewModel it is reading from</param>
        public TextBoxReader(ConsoleViewModel model)
        {
            Model = model;

            Events.SubscribeEvent("OnDestroyed", new Action(() => m_Abort = true));
            Events.SubscribeEvent("OnTerminateExecution", new Action<Action>((x) => m_Abort = true));
            m_Index = 0;

            m_Input = String.Empty;
            m_HasInput = false;
            m_Abort = false;
        }

        private void GetLineInput(string endLine)
        {
            Task<string> task = GetLineAsync(endLine);

            try
            {
                task.Wait();
                m_Input = task.Result;
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                m_Input = endLine;
            }

            m_Index = 0;
            m_HasInput = true;
        }

        private Task<string> GetLineAsync(string endLine)
        {
            m_Abort = false;

            return Task.Run(new Func<string>(() =>
            {
                try
                {
                    Model.BeginInput();

                    while (!Model.TextReadOnly)
                        if (m_Abort)
                            return endLine;

                    return Model.InputText + endLine;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);

                    Model.TextReadOnly = true;
                    return endLine;
                }
            }));
        }


        #region Sync ReadLine/ReadToEnd

        public override string ReadLine()
        {
            if (!m_HasInput)
                GetLineInput("\n");
            m_HasInput = false;

            return m_Input;
        }

        public override string ReadToEnd()
        {
            if (!m_HasInput)
                GetLineInput("\n");
            m_HasInput = false;

            return m_Input;
        }

        #endregion

        #region Async ReadLine/ReadToEnd

        public override Task<string> ReadLineAsync()
        {
            return GetLineAsync("\n");
        }

        public override Task<string> ReadToEndAsync()
        {
            return GetLineAsync("\n");
        }

        #endregion

        #region Read/Peek

        public override int Read()
        {
            if (!m_HasInput)
                GetLineInput("\n");

            if (m_Index < m_Input.Length)
                return (int)m_Input[m_Index++];

            m_HasInput = false;
            return -1;
        }

        public override int Peek()
        {
            if (!m_HasInput)
                GetLineInput("\n");

            if (m_Index + 1 < m_Input.Length)
                return (int)m_Input[m_Index + 1];

            m_HasInput = false;
            return -1;
        }

        #endregion
    }
}
