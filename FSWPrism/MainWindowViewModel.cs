using FSWPrism.Events;
using FSWPrism.Extensions;
using FSWPrism.Interfaces;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Serilog;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FSWPrism
{
    public class MainWindowViewModel : BindableBase
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IFSWService _fSWService;
        private readonly ILogger _logger;

        private ICommand? _startCommand;
        public ICommand StartCommand => _startCommand ?? 
            (_startCommand = new DelegateCommand(async() => await _fSWService.StartAsync(), () => !string.IsNullOrEmpty(CurrentPath))
                                    .ObservesProperty(() => CurrentPath));

        private ICommand? _stopCommand;
        public ICommand StopCommand => _stopCommand ?? (_stopCommand = new DelegateCommand(async() => await _fSWService.StopAsync()));

        private ICommand? _selectPathCommand;
        public ICommand SelectPathCommand => _selectPathCommand ?? (_selectPathCommand = new DelegateCommand(async() => await SelectPathAsync()));

        private string _currentPath = string.Empty;
        public string CurrentPath
        {
            get { return _currentPath; }
            private set { SetProperty(ref _currentPath, value); }
        }

        private bool _fSWServiceRunning = false;
        public bool FSWServiceRunning
        {
            get { return _fSWServiceRunning; }
            set { SetProperty(ref _fSWServiceRunning, value); }
        }

        public MainWindowViewModel(
            IEventAggregator eventAggregator,
            IFSWService fSWService,
            ILogger logger
            )
        {
            _eventAggregator = eventAggregator;
            _fSWService = fSWService;
            _logger = logger;
            _eventAggregator
                .GetEvent<FileSystemEvent>()
                .Subscribe(HandleFileSystemEvent, ThreadOption.UIThread);
            _eventAggregator
                .GetEvent<DirectoryPathChangedEvent>()
                .Subscribe((p) => CurrentPath = p, ThreadOption.UIThread);
            _eventAggregator
                .GetEvent<FSWServiceStateEvent>()
                .Subscribe((r) => FSWServiceRunning = r, ThreadOption.UIThread);
        }

        private void HandleFileSystemEvent(FileChangedResult fileChangedResult)
        {
            var (action, additional) = fileChangedResult.ChangeType switch
            {
                WatcherChangeTypes.Renamed => ("renamed", fileChangedResult.OldName),
                WatcherChangeTypes.Created => ("created", string.Empty),
                WatcherChangeTypes.Deleted => ("deleted", string.Empty),
                WatcherChangeTypes.Changed => ("changed", string.Empty),
                _ => ("<unknown action>", string.Empty)
            };
            _logger.Information($"File was {action}");
            _logger.Information($"File name: \"{fileChangedResult.Name}\"");
            if (!string.IsNullOrEmpty(additional))
                _logger.Information(additional);
        }

        private async Task SelectPathAsync()
        {
            var dialog = new FolderPicker();
            if (dialog.ShowDialog() == true)
            {
                await _fSWService.SetDirectoryPathAsync(dialog.ResultPath);
            }
        }
    }
}
