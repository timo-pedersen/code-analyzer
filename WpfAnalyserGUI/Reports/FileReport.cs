using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfAnalyserGUI.Reports;

public class FileReport
{
    public string FileName { get; set; }
    public string Project { get; set; }
    public string vNextTargetsPath1 { get; set; } = string.Empty;
    public string NeoPath1 { get; set; } = string.Empty;
    public string vNextTargetsPath2 { get; set; } = string.Empty;
    public string NeoPath2 { get; set; } = string.Empty;
    public bool IsRhino1 { get; set; }
    public bool IsRhino2 { get; set; }
    //public bool FileMoved { get; set; }
    //public int Size1 { get; set; }
    //public int Size2 { get; set; }
    //public int SizeDiff { get; set; }
    public bool ExistsInvNextTargets1 { get; set; }
    public bool ExistsInNeo1 { get; set; }
    public bool ExistsInvNextTargets2 { get; set; }
    public bool ExistsInNeo2 { get; set; }
    public bool HasDuplicate { get; set; }
    public string WhatToDo { get; set; } = String.Empty;
    public string Comment { get; set; }
}
