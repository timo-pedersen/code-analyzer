using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestProject
{
    // A class to be mocked
    public class Class2 : IClass2
    {
        public string Name1 { get; set; }
        public string Name2 { get; set; }
        public int SomeEventCallCount { get; set; } = 0;

        public event EventHandler<SomeEventArgs> SomeEvent;
        public void OnSomeEvent()
        {
            SomeEvent?.Invoke(this, new SomeEventArgs("Class2: Some event happened"));
            SomeEventCallCount++;
        }



        public Class2(string name1, string name2)
        {
            Name1 = name1;
            Name2 = name2;
        }

        public string GetFullName1()
        {
            return Name1 + " " + Name2;
        }

        public string GetFullName2()
        {
            return Name2 + ", " + Name1;
        }
    }
}
