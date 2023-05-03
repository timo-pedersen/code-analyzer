using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeAnalyzer.Data
{
    public class Project
    {
        public string Path { get; }
        public string Name { get => System.IO.Path.GetFileNameWithoutExtension(Path); }
        public ObservableCollection<Document> Documents = new();

        public int Matches
        {
            get
            {
                int count = 0;
                foreach (Document data in Documents)
                {
                    count += data.Matches;
                }

                return count;
            }
        }


        public Project(string path)
        {
            Path = path;
        }


    }
}
