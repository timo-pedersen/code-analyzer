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
    public string PRPath { get; set; }
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
    public bool InPR { get; set; }
    public bool In_V1 { get; set; }
    public bool In_N1 { get; set; }
    public bool In_V2 { get; set; }
    public bool In_N2 { get; set; }
    public int Duplicates { get; set; } = 0;
    public bool HasDiff { get; set; }
    public bool Consider { get; set; }
    public string WhatToDo { get; set; } = String.Empty;
    public string Comment { get; set; }

    public override string ToString() => $"{FileName} ({Project})";
}
