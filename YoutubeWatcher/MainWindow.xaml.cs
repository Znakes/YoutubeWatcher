using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Windows;
using System.Windows.Controls;
using Google.Apis.YouTube.v3.Data;
using Youtube;
using YoutubeWatcher.ViewModel;

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

        private async void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listBox = sender as ListBox;
            if (listBox != null)
            {
                var subsEx = (listBox.SelectedItem as SubscriptionEx);

                if (subsEx != null)
                {
                    if (!subsEx.PlayListsAreLoaded)
                        await subsEx.RefreshPlayLists();
                }
            }
        }
    }
}