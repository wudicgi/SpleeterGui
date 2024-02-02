using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpleeterGui.Processor
{
    public static class ProcessManager
    {
        public static event EventHandler<ExternalProgram> ItemAdded;

        public static event EventHandler<ExternalProgram> ItemRemoved;

        private static List<ExternalProgram> _list = new List<ExternalProgram>();

        public static void Add(ExternalProgram ep)
        {
            if (_list.Contains(ep))
            {
                return;
            }

            _list.Add(ep);

            ItemAdded?.Invoke(null, ep);
        }

        public static void Remove(ExternalProgram ep)
        {
            if (!_list.Contains(ep))
            {
                return;
            }

            _list.Remove(ep);

            ItemRemoved?.Invoke(null, ep);
        }

        public static void KillAll()
        {
            List<ExternalProgram> tempList = new List<ExternalProgram>();

            foreach (ExternalProgram process in _list)
            {
                tempList.Add(process);
            }

            foreach (ExternalProgram process in tempList)
            {
                if (!process.HasExited)
                {
                    process.Kill();
                }

                Remove(process);
            }
        }

        public static void TryKillExisted(string processName)
        {
            Process[] processList = Process.GetProcessesByName(processName);

            foreach (Process process in processList)
            {
                process.Kill();
            }
        }
    }
}
