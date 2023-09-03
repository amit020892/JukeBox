using iTunesSearch.Library;
using iTunesSearch.Library.Models;
using System;
using System.Collections.Generic;
using System.IO;
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
    /// Interaction logic for Popup_Confirm.xaml
    /// </summary>
    public partial class Popup_Confirm : UserControl
    {
        private Album baseAlbum { get; set; }
        public Popup_Confirm(Album album, SongResult albumSongs)
        {
            InitializeComponent();

            // Load album details
            baseAlbum = album;

            LoadCover();

            lblAlubmName.Content = album.CollectionName;
            lblAlubmNameBg.Content = album.CollectionName;
            lblArtistName.Content = album.ArtistName;
            lblTrackCount.Content = lblTrackCount.Content + " " + albumSongs.Count;
        }

        internal BitmapImage AlbumCover = new BitmapImage();
        public void LoadCover()
        {
            try
            {
                byte[] a = iTunesSearchManager.getDBCoverArt(baseAlbum.ArtworkUrl100);
                System.Drawing.Bitmap bmp;
                using (var ms = new MemoryStream(a))
                {
                    AlbumCover.BeginInit();
                    AlbumCover.CacheOption = BitmapCacheOption.OnLoad;
                    AlbumCover.StreamSource = ms;
                    AlbumCover.EndInit();

                    AlbumImage.Source = AlbumCover;

                }

            }
            catch
            {

            }
        }

        private void Rectangle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Exit Control
            Global.mPlayer.TogglePopUp();
        }

        private void btnYes_Click(object sender, RoutedEventArgs e)
        {
            // Set as actual album for Track
            var x = (Popup_SmartUpdater)Global.mPlayer.PopUpBackPocket;
            x.SetAlbumConfirm(true);
            Global.mPlayer.TogglePopUp();
        }

        private void btnNo_Click(object sender, RoutedEventArgs e)
        {
            // Save album as not relevant to this track
            // Album Not found
            var x = (Popup_SmartUpdater)Global.mPlayer.PopUpBackPocket;
            x.SetAlbumConfirm(false);
            Global.mPlayer.TogglePopUp();
        }
    }
}
