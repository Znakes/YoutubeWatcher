using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Windows;
using Google.Apis.YouTube.v3.Data;
using Youtube;

namespace YoutubeWatcher
{
    /// <summary>
    ///     Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            DataContext = this;
            InitializeComponent();
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {

        }
    }
}