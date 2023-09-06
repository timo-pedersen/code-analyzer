using System;
// ReSharper disable InconsistentNaming

namespace WpfAnalyserGUI.Reports;

public class FileReport
{
    public string DocumentName { get; set; } = string.Empty;
    public string Project { get; set; } = string.Empty;
    public string PRPath { get; set; } = string.Empty;
    public string vNextTargetsPath1 { get; set; } = string.Empty;
    public string NeoPath1 { get; set; } = string.Empty;
    public string vNextTargetsPath2 { get; set; } = string.Empty;
    public string NeoPath2 { get; set; } = string.Empty;
    public bool IsRhino1 { get; set; }
    public bool IsRhino2 { get; set; }
    public bool InPR { get; set; }
    public bool In_V1 { get; set; }
    public bool In_N1 { get; set; }
    public bool In_V2 { get; set; }
    public bool In_N2 { get; set; }
    public int Duplicates { get; set; } = 0;
    public bool HasDiff { get; set; }
    public bool Consider { get; set; }
    public string WhatToDo { get; set; } = string.Empty;
    public string Comment { get; set; } = string.Empty;

    public override string ToString() => $"{DocumentName} ({Project})";
}
