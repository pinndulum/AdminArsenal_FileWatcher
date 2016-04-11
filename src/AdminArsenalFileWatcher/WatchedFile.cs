using System;

namespace AdminArsenalFileWatcher
{
    public class WatchedFile
    {
        public string Name { get; private set; }
        
        public DateTime LastWriteTime { get; set; }
        public int LineCount { get; set; }

        public WatchedFile(string name)
        {
            Name = name;
        }
    }
}
