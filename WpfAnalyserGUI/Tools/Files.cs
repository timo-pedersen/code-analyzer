using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Animation;

namespace WpfAnalyserGUI.Tools
{
    internal static class Files
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="documentPath">Path to doc. If sourceFolder provided, the absolute path, otherwise relative.</param>
        /// <param name="sourceFolder">Path to source folder</param>
        /// <returns>Emty string if not found.</returns>
        public static string FindCorrespondingReferenceDocument(string documentPath, string sourceFolder = "")
        {
            string referencePath = Path.Combine(Utils.Fs.ApplicationPath + Constants.Const.ReferenceFilesFolder);

            string docRelativePath = string.IsNullOrEmpty(sourceFolder) ? documentPath : Path.GetRelativePath(sourceFolder, documentPath);

            string ret = Path.Combine(referencePath, docRelativePath);

            if (!File.Exists(ret))
            {
                string basePath = docRelativePath.Split(Path.DirectorySeparatorChar)?[0] ?? "";

                var files = new List<string>();

                // Old ref repo may not have files
                try
                {
                    files = new DirectoryInfo(Path.Combine(referencePath, basePath))
                        .EnumerateFiles(Path.GetFileName(documentPath), SearchOption.AllDirectories)
                        .Select(f => f.FullName)
                        .ToList();
                }
                catch { /* ignored */ }

                if (!files.Any())
                    return "";

                ret = files[0];
            }

            return ret;
        }
    }
}
