using Python.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Python_Engine
{
    internal class PythonOutput
    {
        public void write(string line)
        {
            try
            {
                Console.Error.Write(line);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }
        }

        public void writelines(string[] lines)
        {
            try
            {
                foreach (string line in lines)
                    Console.Error.Write(line);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }
        }
        public void flush() { }
        public void close() { }
    }

    internal class PythonError
    {
        public void write(string line)
        {
            try
            {
                Console.Error.Write(line);
            }
            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }
        }

        public void writelines(string[] lines)
        {
            try
            {
                foreach (string line in lines)
                    Console.Error.Write(line);
            }
            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }
        }
        public void flush() { }
        public void close() { }
    }

    internal class PythonInput
    {
        public PyObject read()
        {
            try
            {
                return new PyString(new string(new char[] { (char)Console.In.Read() }));
            }
            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
                return new PyString("\n");
            }
        }

        public PyString readline()
        {
            try
            {
                return new PyString(Console.In.ReadLine());
            }
            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
                return new PyString("\n");
            }
        }

        public PyString readlines()
        {
            try
            {
                return new PyString(Console.In.ReadToEnd());
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
                return new PyString("\n");
            }
        }

        public void flush() { }
        public void close() { }
    }
}
