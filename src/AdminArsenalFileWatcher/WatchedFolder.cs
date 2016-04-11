using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace AdminArsenalFileWatcher
{
    public class WatchedFolder : IEnumerable<WatchedFile>
    {
        private Dictionary<string, WatchedFile> _files = new Dictionary<string, WatchedFile>();

        public string FolderPath { get; private set; }

        public event EventHandler<AddedWatchFileEventArgs> AddedWatchFile;
        public event EventHandler<ModifiedWatchFileEventArgs> ModifiedWatchFile;

        public WatchedFolder(string path)
        {
            FolderPath = path;
        }

        public WatchedFolder(string path, string filter)
            : this(path)
        {
            var files = Directory.EnumerateFiles(path, filter);
            foreach (var file in files)
            {
                Add(Path.GetFileName(file), report: false);
            }
            while (_files.Count < files.Count())
            {
                Thread.Sleep(200);
            }
        }

        public WatchedFile this[string name]
        {
            get
            {
                WatchedFile file;
                _files.TryGetValue(name.ToLower(), out file);
                return file;
            }
            private set
            {
                name = name.ToLower();
                lock (_files)
                {
                    if (value == null)
                    {
                        _files.Remove(name);
                    }
                    _files[name] = value;
                }
            }
        }

        public void Add(string name, bool report = true)
        {
            var watchedFile = this[name];
            if (watchedFile != null)
            {
                return;
            }

            var thread = new Thread(() =>
            {
                var filePath = Path.Combine(FolderPath, name);
                var file = new FileInfo(filePath);
                this[name] = new WatchedFile(name) { LastWriteTime = file.LastWriteTime };

                var lineCount = file.LineCount();
                while (!lineCount.HasValue)
                {
                    Thread.Sleep(200);
                    lineCount = file.LineCount();

                    // further consideration may need to be given to files being locked for an indeterminately long time.
                    // if this ever became a worry a time-out could be added here so that an indefinate\endless loop does not occur.
                }
                
                this[name].LineCount = lineCount.Value;
                if (report)
                {
                    OnAdded(Path.GetFileName(filePath), lineCount.Value);
                }
            }) { IsBackground = true, Name = string.Format("Add '{0}' Thread", name) };
            thread.Start();
        }

        public void Modifiy(string name, bool report = true)
        {
            var watchedFile = this[name];
            if (watchedFile == null)
            {
                return;
            }

            var thread = new Thread(() =>
            {
                var filePath = Path.Combine(FolderPath, name);
                var file = new FileInfo(filePath);
                watchedFile.LastWriteTime = file.LastWriteTime;

                var lineCount = file.LineCount();
                while (!lineCount.HasValue)
                {
                    Thread.Sleep(200);
                    lineCount = file.LineCount();

                    // further consideration may need to be given to files being locked for an indeterminately long time.
                    // if this ever became a worry a time-out could be added here so that an indefinate\endless loop does not occur.
                }

                // subtract the new line count from the tracked line count then multiply by -1 to get a number indicating the change in line count.
                // negative number: file now has fewer lines, positive number: file now has more lines than before.
                var diff = (watchedFile.LineCount - lineCount.Value) * -1;

                watchedFile.LineCount = lineCount.Value;
                if (report)
                {
                    OnModified(name, diff);
                }
            }) { IsBackground = true, Name = string.Format("Modify '{0}' Thread", name) };
            thread.Start();
        }

        public void Remove(string name)
        {
            this[name] = null;
        }

        public IEnumerator<WatchedFile> GetEnumerator()
        {
            return _files.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        private void OnAdded(string name, int lineCount)
        {
            if (AddedWatchFile != null)
            {
                AddedWatchFile(this, new AddedWatchFileEventArgs(name, lineCount));
            }
        }

        private void OnModified(string name, int lineCountDiff)
        {
            if (ModifiedWatchFile != null)
            {
                ModifiedWatchFile(this, new ModifiedWatchFileEventArgs(name, lineCountDiff));
            }
        }
    }
}
