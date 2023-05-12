using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestProject
{
    public class SomeEventArgs : EventArgs
    {
        public string Message { get; }

        public SomeEventArgs(string msg)
        {
            Message = msg;
        }
    }
}
