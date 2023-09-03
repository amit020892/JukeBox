using System.Collections.Generic;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace JukeBoxSolutions.Controls
{
    /// <summary>
    /// Interaction logic for SongPlayingAlbumCard.xaml
    /// </summary>
    public partial class SongPlayingAlbumCard : UserControl
    {
        public AlbumLibrary album { get; set; }
        List<Library.PlayListSource2.AlbumTrack> AlbumTracks { get; set; }
        internal BitmapImage AlbumCover = new BitmapImage();
        public SongPlayingAlbumCard()
        {
            InitializeComponent();
        }

        public void LoadCover(AlbumLibrary baseAlbum)
        {
            System.Drawing.Bitmap bmp;
            using (var ms = new MemoryStream(baseAlbum.CoverArt))
            {
                AlbumCover.BeginInit();
                AlbumCover.CacheOption = BitmapCacheOption.OnLoad;
                AlbumCover.StreamSource = ms;
                AlbumCover.EndInit();

                // AlbumImage.Source = AlbumCover;
                //buttonImage.Source = AlbumCover;
                CoverImage.Source = AlbumCover;
            }

            lblAlbum.Content = baseAlbum.AlbumName;
            if (AlbumTracks.Count > 0)
            {
                lblArtist.Content = AlbumTracks[0].ArtistName;
            }
        }
    }
}
