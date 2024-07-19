using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace FSWPrism.Extensions
{
    public static class FileSystemExtensions
    {
        public static Task<WaitForChangedResult> WaitForChangedAsync(
            this FileSystemWatcher fileSystemWatcher, 
            WatcherChangeTypes changeType,
            CancellationToken cancellationToken = default
            )
        {
            if (!fileSystemWatcher.EnableRaisingEvents)
                throw new InvalidOperationException($"{nameof(FileSystemWatcher.EnableRaisingEvents)} has to be true.");

            var tcs = new TaskCompletionSource<WaitForChangedResult>();
            FileSystemEventHandler? fseh = null;
            RenamedEventHandler? reh = null;

            if ((changeType & (WatcherChangeTypes.Created | WatcherChangeTypes.Deleted | WatcherChangeTypes.Changed)) != 0)
            {
                fseh = (s, e) =>
                {
                    if ((e.ChangeType & changeType) != 0)
                    {
                        if (reh != null)
                            fileSystemWatcher.Renamed -= reh;
                        if ((changeType & WatcherChangeTypes.Changed) != 0)
                            fileSystemWatcher.Changed -= fseh;
                        if ((changeType & WatcherChangeTypes.Deleted) != 0)
                            fileSystemWatcher.Deleted -= fseh;
                        if ((changeType & WatcherChangeTypes.Created) != 0)
                            fileSystemWatcher.Created -= fseh;

                        tcs.TrySetResult(new WaitForChangedResult()
                        {
                            ChangeType = e.ChangeType,
                            Name = e.Name,
                            OldName = null,
                            TimedOut = false
                        });
                    }
                };
                if ((changeType & WatcherChangeTypes.Created) != 0)
                    fileSystemWatcher.Created += fseh;
                if ((changeType & WatcherChangeTypes.Deleted) != 0)
                    fileSystemWatcher.Deleted += fseh;
                if ((changeType & WatcherChangeTypes.Changed) != 0)
                    fileSystemWatcher.Changed += fseh;
            }
            if ((changeType & WatcherChangeTypes.Renamed) != 0)
            {
                reh = (s, e) =>
                {
                    if ((e.ChangeType & changeType) != 0)
                    {
                        if (reh != null)
                            fileSystemWatcher.Renamed -= reh;
                        if ((changeType & WatcherChangeTypes.Changed) != 0)
                            fileSystemWatcher.Changed -= fseh;
                        if ((changeType & WatcherChangeTypes.Deleted) != 0)
                            fileSystemWatcher.Deleted -= fseh;
                        if ((changeType & WatcherChangeTypes.Created) != 0)
                            fileSystemWatcher.Created -= fseh;

                        tcs.TrySetResult(new WaitForChangedResult()
                        {
                            ChangeType = e.ChangeType,
                            Name = e.Name,
                            OldName = e.OldName,
                            TimedOut = false
                        });
                    }
                };
                fileSystemWatcher.Renamed += reh;
            }

            return tcs.Task.WaitAsync(cancellationToken);
        }

        // https://stackoverflow.com/a/56092132/20120008
        public static bool IsFileLocked(string filePath)
        {
            var ret = false;
            try
            {
                using (File.Open(filePath, FileMode.Open)) { }
            }
            catch (IOException e)
            {
                var errorCode = Marshal.GetHRForException(e) & ((1 << 16) - 1);
                ret = errorCode == 32 || errorCode == 33;
            }
            return ret;
        }
    }
}
