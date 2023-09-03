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
using System.Web;

namespace JukeBoxSolutions.Controls
{
    /// <summary>
    /// Interaction logic for LibraryCardRadio.xaml
    /// </summary>
    public partial class LibraryCardRadio : UserControl
    {
        TrackLibrary baseTrack { get; set; }
        bool isPlaying = false;
        public LibraryCardRadio(TrackLibrary trackLibrary)
        {
            InitializeComponent();

            Uri url = new Uri(trackLibrary.FilePath);
            string s =string.Format( "{0}://{1}/favicon.ico",url.Scheme,url.Host);
            ImageSourceConverter ic = new ImageSourceConverter();

            imgFavicon.Source = (ImageSource)ic.ConvertFromString(s);
            baseTrack = trackLibrary;
            lblTrackName.Content = trackLibrary.SongLibraries.First().SongName;
        }

        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            Global.mPlayer.PlayNow(baseTrack);
            Global.AppActionMode = Global.AppActionModeEnum.Playing;

            isPlaying = true;

            Global.mPlayer.PlayNow(baseTrack);
            Global.AppActionMode = Global.AppActionModeEnum.Playing;

            //CardMode = CardModeEnum.RemoveFromPlaylist;
            //IsButtonActive = true;
            Global.LibraryUpdateActionMode();
        }
    }
}