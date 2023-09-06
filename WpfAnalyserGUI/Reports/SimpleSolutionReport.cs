namespace WpfAnalyserGUI.Reports
{
    public class SimpleSolutionReport
    {
        public string Project { get; set; } = string.Empty;
        public string DocumentName { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public bool IsRhino { get; set; }

        public override string ToString() => $"{Project}: {DocumentName}";
    }
}
