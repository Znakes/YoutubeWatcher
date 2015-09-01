using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Interactivity;

namespace YoutubeWatcher.Behaviours
{
    class SelectFirstLine : Behavior<ComboBox>
    {
        protected override void OnAttached()
        {
            //this.AssociatedObject.ItemsSource as ObservableCollection<Channels>
        }
    }
}
