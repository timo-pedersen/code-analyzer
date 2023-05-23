using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestProject
{
    public class SomeEvent2Args : EventArgs
    {
        public string Message { get; }

        public SomeEvent2Args(string msg)
        {
            Message = msg;
        }
    }
}
