using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AdminArsenalFileWatcher
{
    public class AddedWatchFileEventArgs : EventArgs
    {
        public string Name { get; private set; }
        public int LineCount { get; private set; }

        public AddedWatchFileEventArgs(string name, int lineCount)
        {
            Name = name;
            LineCount = lineCount;
        }
    }

    public class ModifiedWatchFileEventArgs : EventArgs
    {
        public string Name { get; private set; }
        public int LineCountDiff { get; private set; }

        public ModifiedWatchFileEventArgs(string name, int lineCountDiff)
        {
            Name = name;
            LineCountDiff = lineCountDiff;
        }
    }
}
