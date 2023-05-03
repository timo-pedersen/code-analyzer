using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeAnalyzer.Data
{
    public class Document
    {
        public string Path { get; }
        public string Name { get => System.IO.Path.GetFileNameWithoutExtension(Path); }
        public int Matches { get; set; }
        public List<CSharpSyntaxNode> SyntaxNodes { get; } = new ();

        public Document(string path)
        {
            Path = path;
        }

    }
}
