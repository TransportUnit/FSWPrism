using FSWPrism.Events;
using FSWPrism.Extensions;
using FSWPrism.Interfaces;
using Prism.Events;
using Serilog;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FSWPrism.Services
{
    public class FSWService : IFSWService
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly ILogger _logger;
        private CancellationTokenSource? _tokenSource;
        private Task? _workerTask;
        private readonly object _pathLock = new object();
        private string _path = string.Empty;
        public string Path
        {
            get { lock (_pathLock) { return _path; } }
            private set 
            {
                lock (_pathLock) 
                {
                    _path = value; 
                }
            }
        }

        public FSWService(
            IEventAggregator eventAggregator,
            ILogger logger
            )
        {
            Path = string.Empty;
            _eventAggregator = eventAggregator;
            _logger = logger;
        }

        private async Task CheckFileSystem(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    using (var fileSystemWatcher = new FileSystemWatcher(Path) { EnableRaisingEvents = true })
                    {
                        var result = await fileSystemWatcher.WaitForChangedAsync(WatcherChangeTypes.All, cancellationToken);
                        _eventAggregator?
                            .GetEvent<FileSystemEvent>()
                            .Publish(
                                new FileChangedResult()
                                { ChangeType = result.ChangeType, Name = result.Name, OldName = result.OldName });
                    }
                }
            }
            catch (Exception ex) 
            {
                //throw;
                _logger?.Error(ex.Message, ex);
            }
            finally
            {
                _eventAggregator?.GetEvent<FSWServiceStateEvent>().Publish(false);
            }
        }

        public async Task SetDirectoryPathAsync(string path)
        {
            if (path == Path)
                return;

            await StopAsync();

            Path = path;

            _eventAggregator?.GetEvent<DirectoryPathChangedEvent>().Publish(Path);
        }

        public async Task StartAsync()
        {
            await StopAsync();

            _tokenSource = new CancellationTokenSource();

            var token = _tokenSource.Token;

            _workerTask = Task.Run(async () =>
            {
                await CheckFileSystem(_tokenSource.Token);
            }, _tokenSource.Token);

            _eventAggregator?.GetEvent<FSWServiceStateEvent>().Publish(true);
        }

        public async Task StopAsync()
        {
            try
            {
                _tokenSource?.Cancel();
            }
            catch (ObjectDisposedException) { }

            try
            {
                if (_workerTask is not null)
                    await _workerTask;
            }
            catch (OperationCanceledException) { }
            catch { throw; }
            finally
            {
                _tokenSource?.Dispose();
                _eventAggregator?.GetEvent<FSWServiceStateEvent>().Publish(false);
            }
        }
    }
}
