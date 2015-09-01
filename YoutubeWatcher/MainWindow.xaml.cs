using System;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using GalaSoft.MvvmLight.Ioc;
using Google.Apis.YouTube.v3.Data;
using Youtube;
using YoutubeWatcher.ErrorHandler;
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
            Browser.Navigated += Browser_Navigated;
        }

        private void Browser_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            SilentHelper.SetSilent(Browser, true);
        }

        private async void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listBox = sender as Selector;
            if (listBox != null)
            {
                var subsEx = (listBox.SelectedItem as ChannelEx);

                if (subsEx != null)
                {
                    if (!subsEx.PlayListsAreLoaded)
                    {
                        await subsEx.RefreshPlayLists();
                    }
                }
            }
        }

        private async void PlaylistListBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listBox = sender as Selector;
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

            var listBox = sender as Selector;
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

        private void FrameworkElement_OnSourceUpdated(object sender, DataTransferEventArgs e)
        {
            var listBox = sender as Selector;
            if (listBox != null)
            {
                listBox.SelectedIndex = 0;
            }
        }

        private void MainWindow_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Right)
            {
                e.Handled = true;
                var mvm = SimpleIoc.Default.GetInstance<MainViewModel>();
                if (mvm.GetNextCommand.CanExecute(null))
                    mvm.GetNextCommand.Execute(null);
            }

        }
    }
}