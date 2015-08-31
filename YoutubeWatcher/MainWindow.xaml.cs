using System;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Threading;
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

        private async void PlaylistListBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listBox = sender as ListBox;
            if (listBox != null)
            {
                var plEx = (listBox.SelectedItem as PlaylistEx);

                if (plEx != null)
                {
                    if (!plEx.PlayListsItemsAreLoaded)
                        await plEx.RefreshPlayListItems(CancellationToken.None);
                }
            }
        }

        private void PlaylistItem_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var baseUrl = @"http://www.youtube.com/watch?v=";

            var listBox = sender as ListBox;
            if (listBox != null)
            {
                var plItem = (listBox.SelectedItem as PlaylistItem);

                if (plItem != null)
                {
                    var appendix = plItem.Snippet.ResourceId.VideoId;
                    this.Browser.Source = new Uri(string.Format(@"{0}{1}", baseUrl, appendix));
                }
            }

            

        }
    }
}