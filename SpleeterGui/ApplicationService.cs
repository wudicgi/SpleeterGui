using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpleeterGui
{
    internal sealed class ApplicationService
    {
        internal const string APPLICATION_TITLE = "Spleeter GUI";

        static ApplicationService()
        {

        }

        public static string GetCurrentVersion()
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            System.Diagnostics.FileVersionInfo versionInfo = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);

            // a.b.c.d
            // string version = versionInfo.FileVersion;

            // a.b.c
            string version = String.Format("{0}.{1}.{2}", versionInfo.FileMajorPart, versionInfo.FileMinorPart, versionInfo.FileBuildPart);

            return version;
        }

        public static string GetCopyrightInformation()
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            System.Diagnostics.FileVersionInfo versionInfo = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);

            string copyright = versionInfo.LegalCopyright;

            return copyright;
        }
    }
}
