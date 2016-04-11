using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;

namespace AdminArsenalFileWatcher
{
    /* ---------------------------------------------------------------------------------------------------------------------------------
     * 1:  The program takes 2 arguments, the directory to watch and a file pattern, example: program.exe "c:file folder" *.txt
     * 2:  The path may be an absolute path, relative to the current directory, or UNC.
     * 3:  Use the modified date of the file as a trigger that the file has changed.
     * 4:  Check for changes every 10 seconds.
     * 5:  When a file is created output a line to the console with its name and how many lines are in it.
     * 6:  When a file is modified output a line with its name and the change in number of lines (use a + or - to indicate more or less).
     * 7:  When a file is deleted output a line with its name.
     * 8:  Files will be ASCII or UTF-8 and will use Windows line separators (CR LF).
     * 9:  Multiple files may be changed at the same time, can be up to 2 GB in size, and may be locked for several seconds at a time.
     * 10: Use multiple threads so that the program doesn't block on a single large or locked file.
     * 11: Program will be run on Windows 7
     * 12: File names are case insensitive.
     * ---------------------------------------------------------------------------------------------------------------------------------*/

    class Program
    {
        static string _path;
        static string _filter = "*.*";
        static BackgroundWorker _bgw;

        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Please provide a folder path to watch.");
            }
            else
            {
                _path = Path.GetFullPath(args[0]);
                if (!Directory.Exists(_path))
                {
                    Console.WriteLine("Folder path to watch does not exist. Please check the path and try again.");
                }
                else
                {
                    // default filter is *.* unless set in argument path.
                    if (args.Length > 1)
                    {
                        _filter = args[1];
                    }

                    Console.WriteLine("Started watching for changes on path \"{0}\".", _path);
                    _bgw = new BackgroundWorker() { WorkerReportsProgress = true, WorkerSupportsCancellation = false };
                    _bgw.DoWork += _bgw_DoWork;
                    _bgw.ProgressChanged += _bgw_ProgressChanged;
                    _bgw.RunWorkerCompleted += _bgw_RunWorkerCompleted;
                    _bgw.RunWorkerAsync();
                }
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        static void _bgw_DoWork(object sender, DoWorkEventArgs e)
        {
            var bgw = sender as BackgroundWorker;
            
            // initialize watch folder - existing files will not be displayed until they are modified or delete.
            var watchedFolder = new WatchedFolder(_path, _filter);
            watchedFolder.AddedWatchFile += (s, a) =>
            {
                bgw.ReportProgress(0, string.Format("\"{0}\" : {1}", a.Name, a.LineCount));
            };
            watchedFolder.ModifiedWatchFile += (s, a) =>
            {
                // string formatter for displaying diff with plus sign (+) or minus (-). third format is for zero change in line count.
                bgw.ReportProgress(1, string.Format("\"{0}\" : {1:+0;-0;0}", a.Name, a.LineCountDiff));
            };
            
            var dir = new DirectoryInfo(_path);
            while (true)
            {
                dir.Refresh();
                if (!dir.Exists)
                {
                    throw new Exception("Folder path to watch does not exist. Please check the path and try again.");
                }
                var files = dir.EnumerateFiles(_filter);

                // files exist in the watched folder but are not being tracked.
                var newFiles = files.Where(x => watchedFolder[x.Name] == null);
                foreach (var file in newFiles)
                {
                    watchedFolder.Add(file.Name);
                }

                // tracking files that have a different last write time.
                var modFiles = files.Where(x => watchedFolder[x.Name] != null && watchedFolder[x.Name].LastWriteTime != x.LastWriteTime);
                foreach (var file in modFiles)
                {
                    watchedFolder.Modifiy(file.Name);
                }

                // tracking files that no longer exist in the watched folder.
                var rmvFiles = watchedFolder.OfType<WatchedFile>().Where(x => !files.Any(f => f.Name == x.Name)).ToArray();
                foreach (var file in rmvFiles)
                {
                    watchedFolder.Remove(file.Name);
                    bgw.ReportProgress(2, string.Format("\"{0}\"", file.Name));
                }

                Thread.Sleep(10000);
            }
        }

        static void _bgw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            ClearPreviousConsoleLine();
            Console.WriteLine(e.UserState);
            Console.WriteLine("Press any key to exit...");
        }

        static void _bgw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                ClearPreviousConsoleLine();
                Console.WriteLine(e.Error.Message);
                Console.WriteLine("Press any key to exit...");
            }
        }

        static void ClearPreviousConsoleLine()
        {
            Console.SetCursorPosition(0, Console.CursorTop - 1);
            for (int i = 0; i < Console.WindowWidth; i++)
            {
                Console.Write(" ");
            }
            Console.SetCursorPosition(0, Console.CursorTop - 1);
        }
    }
}
