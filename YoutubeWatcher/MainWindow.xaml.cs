using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Google.Apis.YouTube.v3.Data;

namespace YoutubeWatcher
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {

            DataContext = this;
            InitializeComponent();
        }

        public ObservableCollection<Channel> PlayListItem { get; set; } 

        private async void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {

            RetrieveChannels terminal = new RetrieveChannels();

            await terminal.GetPlaylist();
            //RetrieveChannels terminal = new RetrieveChannels();

            //var playlist = terminal.GetPlaylist();
        }
    }
}
