using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpleeterGui.DataStructure
{
    public class ProcessingStats
    {
        public DateTime TotalBeginDateTime { get; set; }

        public int TotalFileCount { get; set; }

        public int CurrentFileIndex { get; set; }

        public DateTime CurrentFileBeginDateTime { get; set; }
    }
}
