using FSWPrism.Interfaces;
using FSWPrism.Services;
using Prism.Events;
using Prism.Ioc;
using Prism.Unity;
using Serilog;
using System.Windows;

namespace FSWPrism
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : PrismApplication
    {
        private System.Windows.Controls.RichTextBox? _logRichTextBox;

        protected override Window CreateShell()
        {
            var w = new MainWindow();
            if (_logRichTextBox is not null)
                w.InjectLogControl(_logRichTextBox);
            return w;
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.Register<ILogger>(() => Log.Logger);
            containerRegistry.RegisterSingleton<IEventAggregator, EventAggregator>();
            containerRegistry.RegisterInstance<IFSWService>(
                new FSWService(
                    Container.Resolve<IEventAggregator>(),
                    Container.Resolve<ILogger>()
                    ));
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            _logRichTextBox = new System.Windows.Controls.RichTextBox();

            Log.Logger = new LoggerConfiguration()
               .MinimumLevel.Verbose()
               .WriteTo.RichTextBox(_logRichTextBox)
               .CreateLogger();

            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Log.CloseAndFlush();
            base.OnExit(e);
        }
    }
}
