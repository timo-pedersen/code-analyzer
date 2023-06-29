using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Documents;
using System.Windows.Media;
using FontFamily = System.Windows.Media.FontFamily;

namespace WpfAnalyserGUI.FlowDoc;

public static class CodeFormatter
{
    public static FlowDocument GenerateFlowDoc(string src, IEnumerable<CSharpSyntaxNode> nodes)
    {
        List<TextSpan> highlightSpans = nodes.Select(x => x.FullSpan).OrderBy(x => x.Start).ToList();
        List<(TextSpan Text, TextType Type)> allSpans = FillInEmptySpans(nodes.Select(x => x.FullSpan).OrderBy(x => x.Start).ToList(), src.Length);

        FlowDocument flowDoc = new FlowDocument();

        Paragraph paragraph = new Paragraph();
        paragraph.FontFamily = new FontFamily("Courier New");

        foreach (var span in allSpans)
        {
            Run run = new Run(src.Substring(span.Text.Start, span.Text.Length));
            if (span.Type == TextType.Highlight)
                run.Background = System.Windows.Media.Brushes.Pink;

            paragraph.Inlines.Add(run);
        }

        flowDoc.Blocks.Add(paragraph);

        return flowDoc;
    }

    public static List<(TextSpan, TextType)> FillInEmptySpans(IList<TextSpan> textSpans, int textLength)
    {
        if(textLength < 1) return new List<(TextSpan, TextType)>();

        var res = new List<(TextSpan Text, TextType Type)>();

        if (textSpans.Count() == 0)
        {
            TextSpan t = new TextSpan(0, textLength);
            return new List<(TextSpan Text, TextType Type)> {((TextSpan, TextType))(t, TextType.Normal)};
        }

        TextSpan prev = new TextSpan(0,0);
        foreach (var cur in textSpans)
        {
            if(prev.End < cur.Start) 
            {
                res.Add((new TextSpan(prev.End, cur.Start - prev.End), TextType.Normal));
            }
            res.Add((cur, TextType.Highlight));
            prev = cur;
        }

        if(prev.End < textLength)
        {
            res.Add((new TextSpan(prev.End, textLength - prev.End), TextType.Normal));
        }

        return res;
    }

    public static FlowDocument GeneratePlainFlowDoc(string src)
    {
        FlowDocument flowDoc = new FlowDocument();

        Paragraph paragraph = new Paragraph();
        paragraph.FontFamily = new FontFamily("Courier New");

        Run run = new Run(src);

        paragraph.Inlines.Add(run);

        flowDoc.Blocks.Add(paragraph);

        return flowDoc;
    }

    public enum TextType
    {
        Normal = 0,
        Highlight,
    }
}
