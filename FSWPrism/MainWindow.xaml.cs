using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace FSWPrism
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public void InjectLogControl(Control control)
        {
            MainGrid.Children.Add(control);
            ScrollViewer.SetVerticalScrollBarVisibility(control, ScrollBarVisibility.Auto);
            ScrollViewer.SetPanningMode(control, PanningMode.VerticalOnly);
            control.Background = Brushes.Black;
            Grid.SetRow(control, 1);
        }
    }
}
