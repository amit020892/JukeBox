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
    /// Interaction logic for LibraryCard.xaml
    /// </summary>
    public partial class LibraryCard2 : UserControl
    {
        Library parent { get; set; }
        public AlbumLibrary album { get; set; }
        List<SongLibrary> songs { get; set; }

        internal BitmapImage AlbumCover = new BitmapImage();

        private Image buttonImage;

        public Image ButtonImage
        {
            get
            {
                return buttonImage;
            }
        }

        private ImageBrush buttonImage2;
        public ImageBrush ButtonImage2
        {
            get
            {
                return buttonImage2;
            }
        }

        public void LoadCover(AlbumLibrary baseAlbum)
        {
            buttonImage2 = new ImageBrush();
            System.Drawing.Bitmap bmp;
            using (var ms = new MemoryStream(baseAlbum.CoverArt))
            {
                AlbumCover.BeginInit();
                AlbumCover.CacheOption = BitmapCacheOption.OnLoad;
                AlbumCover.StreamSource = ms;
                AlbumCover.EndInit();

                // AlbumImage.Source = AlbumCover;
                //buttonImage.Source = AlbumCover;
                buttonImage2.ImageSource = AlbumCover;

                bgRectangle2.Fill = buttonImage2;
                hasCover = true;
            }
        }
        bool hasCover = false;

        public LibraryCard2(AlbumLibrary alphaName, List<SongLibrary> itemNames, string betaName, Library Parent, bool isSelected = false)
        {
            InitializeComponent();

            //parent = Parent;
            //lblNameBeta.Content = betaName;
            //lblNameAlpha.Content = alphaName.AlbumName;

            //album = alphaName;
            //songs = itemNames;


            //string countlabel = itemNames.Count.ToString();
            //if (alphaName.NumTracks.HasValue)
            //    countlabel += @"\" + alphaName.NumTracks.Value.ToString();

            //lblContentCount.Content = countlabel;


            //// If has complete album details, load the cover, not the tracks
            //// If no cover, load the tracks instead
            //if (alphaName.CoverArt != null)
            //{
            //    try
            //    {
            //        //LoadCover(alphaName);
            //    }
            //    catch
            //    {

            //    }
            //}

            //if (hasCover)
            //{
            //    lblNameBeta.Visibility = Visibility.Collapsed;
            //    lblNameAlpha.Visibility = Visibility.Collapsed;
            //}
            //else
            //{
            //    if (false)
            //    {
            //        foreach (string s in itemNames.Select(s => s.SongName))
            //        {
            //            Label l = new Label();
            //            l.Content = s;
            //            l.FontSize = 10;
            //            //Height = "17.5" 
            //            l.Padding = new Thickness(0);
            //            l.HorizontalContentAlignment = HorizontalAlignment.Center;

            //            //VerticalAlignment = "Center" HorizontalAlignment = "Center" Padding = "0" HorizontalContentAlignment = "Center" />

            //            stackTracks.Children.Add(l);
            //        }
            //    }
            //}

            ////SetAppActionMode();
            //Selected = isSelected;
        }


        private void Rectanngle_Expand_Click(object sender, RoutedEventArgs e)
        {
            // Pop Up Library Item
            Global.mPlayer.popUp_Grid.Visibility = Visibility.Visible;
            KillMeNow();
            // This is removed for testing, needed to actually run
            //Global.mPlayer.popUp_Frame.Content = new LibraryPopup(album, songs, this);


            //parent.OpenOverlay(album, songs);
        }

        private void KillMeNow()
        {
            throw new NotImplementedException();
        }

        public bool Selected
        {
            get { return _isAddAll; }
            set
            {
                if (value)
                {
                    bgSelected.Visibility = Visibility.Visible;
                    btnAddAll.Visibility = Visibility.Collapsed;

                    btnRemoveAll.Visibility = Visibility.Visible;
                }
                else
                {
                    bgSelected.Visibility = Visibility.Collapsed;
                    btnAddAll.Visibility = Visibility.Visible;

                    btnRemoveAll.Visibility = Visibility.Collapsed;
                }
                _isAddAll = value;
            }
        }
        private bool _isAddAll = true;


        #region ModeSettings


        public void SetAppActionMode()
        {
            var x = Global.AppControlMode;

            //default
            bool showPlay = true;

            if (Global.AppActionMode == Global.AppActionModeEnum.Idle)
            {
                Show_btnPlayAll();
            }
            else
            {
                Show_btnAddAll();
                //btnAddAll.Visibility = Visibility.Collapsed;
            }

            if (Global.AppControlMode == Global.AppControlModeEnum.Playlist && Global.LibraryForceViewMode == false)
            {
                Show_btnAddAll();
            }

        }

        private void Show_btnRemoveAll()
        {
            btnAddAll.Visibility = Visibility.Collapsed;
            btnPlayAll.Visibility = Visibility.Collapsed;
            btnRemoveAll.Visibility = Visibility.Visible;
        }
        private void Show_btnAddAll()
        {
            btnAddAll.Visibility = Visibility.Visible;
            btnPlayAll.Visibility = Visibility.Collapsed;
            btnRemoveAll.Visibility = Visibility.Collapsed;
        }

        private void Show_btnPlayAll()
        {
            btnAddAll.Visibility = Visibility.Collapsed;
            btnPlayAll.Visibility = Visibility.Visible;
            btnRemoveAll.Visibility = Visibility.Collapsed;
        }



        #endregion ModeSettings

        #region events


        private void btnAddAll_Click(object sender, RoutedEventArgs e)
        {
            if (Global.AppControlMode == Global.AppControlModeEnum.Playlist && Global.LibraryForceViewMode == false)
            {
                List<TrackLibrary> albumtracks = new List<TrackLibrary>();
                using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                {
                    // Get tracks
                    albumtracks = (from x in db.AlbumLibraries.Find(album.AlbumId).SongLibraries
                                   where x.TrackLibraries.Any(a => a.Type == Global.AppModeString)
                                   select x.TrackLibraries.First(a2 => a2.Type == Global.AppModeString)).ToList();
                }

                Global.mPlayer.dbAddTrackToPlaylist(albumtracks, Global.AppPlaylistModeBufferID);
                Global.mPlayer.dbAddTrackToPlaylist(albumtracks, Global.BufferPlaylistID);

                Selected = true;
            }
            else
            {
                List<TrackLibrary> albumtracks = new List<TrackLibrary>();
                using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                {
                    // Get tracks
                    albumtracks = (from x in db.AlbumLibraries.Find(album.AlbumId).SongLibraries
                                   where x.TrackLibraries.Any(a => a.Type == Global.AppModeString)
                                   select x.TrackLibraries.First(a2 => a2.Type == Global.AppModeString)).ToList();

                    Global.mPlayer.AddToPlaylist(albumtracks.ToList());

                    Global.AppActionMode = Global.AppActionModeEnum.Playing;
                    Selected = true;
                }
            }
        }

        private void btnPlayAll_Click(object sender, RoutedEventArgs e)
        {
            List<TrackLibrary> albumtracks = new List<TrackLibrary>();
            using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
            {
                // Get tracks
                albumtracks = (from x in db.AlbumLibraries.Find(album.AlbumId).SongLibraries
                               where x.TrackLibraries.Any(a => a.Type == Global.AppModeString)
                               select x.TrackLibraries.First(a2 => a2.Type == Global.AppModeString)).ToList();

                Global.mPlayer.PlayNow(albumtracks.ToList());

                Global.AppActionMode = Global.AppActionModeEnum.Playing;
                Selected = true;
            }


            //var tracks = from t in songs select t.TrackLibraries.Where(w => w.Type == Global.AppModeString).First();
            //Global.mPlayer.PlayNow(tracks.ToList());
            // play album

            Global.LibraryUpdateActionMode();
            Selected = true;
        }


        #endregion events

        private void btnRemoveAll_Click(object sender, RoutedEventArgs e)
        {
            if (Global.AppControlMode == Global.AppControlModeEnum.Playlist)
            {
                List<TrackLibrary> albumtracks = new List<TrackLibrary>();
                using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                {
                    // Get tracks
                    albumtracks = (from x in db.AlbumLibraries.Find(album.AlbumId).SongLibraries
                                   where x.TrackLibraries.Any(a => a.Type == Global.AppModeString)
                                   select x.TrackLibraries.First(a2 => a2.Type == Global.AppModeString)).ToList();
                }

                Global.mPlayer.dbRemoveTrackFromPlaylist(albumtracks, Global.AppPlaylistModeBufferID);
                //Not from the buffer, the buffer is to restore changes
                //Global.mPlayer.dbRemoveTrackFromPlaylist(albumtracks, Global.BufferPlaylistID);

                Selected = false;
            }
            else
            {
                List<TrackLibrary> albumtracks = new List<TrackLibrary>();
                using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                {
                    // Get tracks
                    albumtracks = (from x in db.AlbumLibraries.Find(album.AlbumId).SongLibraries
                                   where x.TrackLibraries.Any(a => a.Type == Global.AppModeString)
                                   select x.TrackLibraries.First(a2 => a2.Type == Global.AppModeString)).ToList();
                }

                // Adds all tracks to current playlist
                Global.mPlayer.RemoveFromPlaylist(albumtracks);
                Selected = false;
            }
        }
    }
}
