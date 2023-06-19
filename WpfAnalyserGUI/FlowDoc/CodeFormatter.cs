using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using static System.Net.Mime.MediaTypeNames;

namespace WpfAnalyserGUI.FlowDoc;

public static class CodeFormatter
{
    public static FlowDocument GenerateFlowDoc(string src, IEnumerable<CSharpSyntaxNode> nodes)
    {
        List<TextSpan> textRanges= nodes.Select(x => x.FullSpan).ToList();



        FlowDocument flowDoc = new FlowDocument();
        flowDoc.Blocks.Clear();
        flowDoc.Blocks.Add(new Paragraph(new Run(src)));

        return flowDoc;
    }
}
