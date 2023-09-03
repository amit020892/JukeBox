using System;
using System.Collections.Generic;
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

namespace JukeBoxSolutions.Controls
{
    /// <summary>
    /// Interaction logic for LibraryPopupItem.xaml
    /// </summary>
    public partial class LibraryPopupItem : UserControl
    {
        SongLibrary song { get; set; }
        TrackLibrary baseTrack { get; set; }
        string SongName { get; set; }

        public LibraryPopupItem(TrackLibrary t)
        {
            InitializeComponent();
            baseTrack = t;
            SongName = baseTrack.SongLibraries.First().SongName;
            lblTrackName.Content = SongName;
        }

        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            Global.mPlayer.PlayNow(baseTrack);
        }

        private void btnAddToPlaylist_Click(object sender, RoutedEventArgs e)
        {
            Global.mPlayer.AddToPlaylist(baseTrack);
        }
    }
}
