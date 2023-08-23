using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfAnalyserGUI.Reports;

public class FileReport
{
    public string FileName { get; set; }
    public string Path1 { get; set; }
    public string Path2 { get; set; }
    public string FileMoved { get; set; }
    public int Size1 { get; set; }
    public int Size2 { get; set; }
    public int SizeDiff { get; set; }
    public bool ExistsInNeo1 { get; set; }
    public bool ExistsInvNextTargets1 { get; set; }
    public bool FileIsRhino1 { get; set; }
    public bool ExistsInNeo2 { get; set; }
    public bool ExistsInvNextTargets2 { get; set; }
    public bool FileIsRhino2 { get; set; }
    public string Error { get; set; }
}
