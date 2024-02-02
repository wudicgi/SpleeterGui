using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SpleeterGui.Util
{
    public static class FileSystemUtil
    {
        /// <summary>
        /// Checks whether the specified path is an absolute path
        /// </summary>
        /// <remarks>
        /// https://stackoverflow.com/questions/5565029/check-if-full-path-given
        /// </remarks>
        /// <param name="path">The path to check</param>
        /// <returns>true if the path is a absolute path; otherwise, false</returns>
        public static bool IsFullPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || path.IndexOfAny(Path.GetInvalidPathChars()) != -1 || !Path.IsPathRooted(path))
            {
                return false;
            }

            string pathRoot = Path.GetPathRoot(path);

            // Accepts X:\ and \\UNC\PATH, rejects empty string, \ and X:, but accepts / to support Linux
            if (pathRoot.Length <= 2 && pathRoot != "/")
            {
                return false;
            }

            // A UNC server name without a share name (e.g "\\NAME") is invalid
            if (pathRoot == path && pathRoot.StartsWith("\\\\") && pathRoot.IndexOf('\\', 2) == -1)
            {
                return false;
            }

            return true;
        }

        public static string GetProgramDirectoryFullPath()
        {
            string programFileFullPath = Assembly.GetExecutingAssembly().Location;

            string programDirectoryFullPath = Path.GetDirectoryName(programFileFullPath);

            return programDirectoryFullPath;
        }

        public static string GetFullPathBasedOnProgramFile(string path)
        {
            string fullPath;
            if (FileSystemUtil.IsFullPath(path))
            {
                fullPath = Path.GetFullPath(path);
            }
            else
            {
                string programDirectoryFullPath = FileSystemUtil.GetProgramDirectoryFullPath();

                fullPath = Path.GetFullPath(Path.Combine(programDirectoryFullPath, path));
            }

            return fullPath;
        }
    }
}
