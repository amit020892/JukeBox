using iTunesSearch.Library.Models;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
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
using static JukeBoxSolutions.Library.PlayListSource2;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;

namespace JukeBoxSolutions.Controls
{
    /// <summary>
    /// Interaction logic for LibraryCardV1.xaml
    /// </summary>
    public partial class LibraryCardV1 : UserControl
    {
        Library parent { get; set; }
        public AlbumLibrary album { get; set; }
        List<SongLibrary> songs { get; set; }
        List<Library.PlayListSource2.AlbumTrack> songsMk2 { get; set; }

        internal BitmapImage AlbumCover = new BitmapImage();

        //private Image buttonImage;

        //public Image ButtonImage
        //{
        //    get
        //    {
        //        return buttonImage;
        //    }
        //}

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
            try
            {
                buttonImage2 = new ImageBrush();
                System.Drawing.Bitmap bmp;
                using (var ms = new MemoryStream(baseAlbum.CoverArt))
                {
                    bmp = new System.Drawing.Bitmap(ms);
                    bmp.MakeTransparent(bmp.GetPixel(0, 0));

                    // AlbumImage.Source = AlbumCover;
                    //buttonImage.Source = AlbumCover;
                    CoverImage.ImageSource = ToBitmapImage(bmp);
                    hasCover = true;
                }
            }
            catch (Exception)
            {

            }
            
        }

        public BitmapImage ToBitmapImage(Bitmap bitmap)
        {
            using (var memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Png);
                memory.Position = 0;

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();

                return bitmapImage;
            }
        }
        bool hasCover = false;

        ///TEST Library MK2
        public LibraryCardV1(AlbumLibrary alphaName, List<Library.PlayListSource2.AlbumTrack> albumTracks, string betaName, bool isSelected = false)
        {
            InitializeComponent();

            //parent = Parent;
            //lblNameBeta.Content = betaName;
            lblAlbum.Content = alphaName.AlbumName;
            if (albumTracks.Count > 0)
            {
                lblArtist.Content = albumTracks[0].ArtistName;
            }

            album = alphaName;
            songsMk2 = albumTracks;
            //ArtistName = betaName;

            if (Global.AdminSettings.ShowAlbumNumbers)
            {
                string countlabel = albumTracks.Count.ToString();
                if (alphaName.NumTracks.HasValue)
                    countlabel += @"\" + alphaName.NumTracks.Value.ToString();

                //lblContentCount.Content = countlabel;
            }
            else
                //lblContentCount.Content = "        ";

            if (alphaName.CoverArt != null)
            {
                try
                {
                    LoadCover(alphaName);
                }
                catch
                {

                }
            }

            if (hasCover)
            {
                //lblNameBeta.Visibility = Visibility.Collapsed;
                //lblNameAlpha.Visibility = Visibility.Collapsed;
            }
            else
            {
                foreach (string s in albumTracks.Select(s => s.SongName))
                {
                    Label l = new Label();
                    l.Content = s;
                    l.FontSize = 10;
                    //Height = "17.5" 
                    l.Padding = new Thickness(0);
                    l.HorizontalContentAlignment = HorizontalAlignment.Center;

                    //VerticalAlignment = "Center" HorizontalAlignment = "Center" Padding = "0" HorizontalContentAlignment = "Center" />

                    //stackTracks.Children.Add(l);
                }
            }

            SetAppActionMode();
            Selected = isSelected;
        }
        public LibraryCardV1(AlbumLibrary alphaName, List<Library.PlayListSource2.AlbumTrack> albumTracks, string betaName, Library Parent, bool isSelected = false)
        {
            InitializeComponent();

            parent = Parent;
            lblAlbum.Content = alphaName.AlbumName;
            if (albumTracks.Count > 0)
            {
                lblArtist.Content = albumTracks[0].ArtistName;
            }

            album = alphaName;
            songsMk2 = albumTracks;
            //ArtistName = betaName;

            if (Global.AdminSettings.ShowAlbumNumbers)
            {
                string countlabel = albumTracks.Count.ToString();
                if (alphaName.NumTracks.HasValue)
                    countlabel += @"\" + alphaName.NumTracks.Value.ToString();

                //lblContentCount.Content = countlabel;
            }
            else
                //lblContentCount.Content = "        ";

            if (alphaName.CoverArt != null)
            {
                try
                {
                    LoadCover(alphaName);
                }
                catch
                {

                }
            }

            if (hasCover)
            {
                //lblNameBeta.Visibility = Visibility.Collapsed;
                //lblNameAlpha.Visibility = Visibility.Collapsed;
            }
            else
            {
                foreach (string s in albumTracks.Select(s => s.SongName))
                {
                    Label l = new Label();
                    l.Content = s;
                    l.FontSize = 10;
                    //Height = "17.5" 
                    l.Padding = new Thickness(0);
                    l.HorizontalContentAlignment = HorizontalAlignment.Center;

                    //VerticalAlignment = "Center" HorizontalAlignment = "Center" Padding = "0" HorizontalContentAlignment = "Center" />

                   // stackTracks.Children.Add(l);
                }
            }

            SetAppActionMode();
            Selected = isSelected;
        }

        public LibraryCardV1(AlbumLibrary alphaName, List<SongLibrary> itemNames, string betaName, Library Parent, bool isSelected = false)
        {
            InitializeComponent();

            try
            {
                parent = Parent;
                //lblNameBeta.Content = betaName;
                lblAlbum.Content = alphaName.AlbumName;

                bool catcher = false;
                if (alphaName.AlbumName == "Old Skool")
                    catcher = true;


                album = alphaName;
                songs = itemNames;
                //ArtistName = betaName;
                lblArtist.Content = betaName;
                if (Global.AdminSettings.ShowAlbumNumbers)
                {
                    string countlabel = itemNames.Count.ToString();
                    if (alphaName.NumTracks.HasValue)
                        countlabel += @"\" + alphaName.NumTracks.Value.ToString();

                    //lblContentCount.Content = countlabel;
                }
                else
                //lblContentCount.Content = "        ";

                // If has complete album details, load the cover, not the tracks
                // If no cover, load the tracks instead
                if (alphaName.CoverArt != null)
                {
                    try
                    {
                        LoadCover(alphaName);
                    }
                    catch
                    {

                    }
                }

                if (hasCover)
                {
                    //lblNameBeta.Visibility = Visibility.Collapsed;
                    //lblNameAlpha.Visibility = Visibility.Collapsed;
                }
                else
                {
                    foreach (string s in itemNames.Select(s => s.SongName))
                    {
                        Label l = new Label();
                        l.Content = s;
                        l.FontSize = 10;
                        //Height = "17.5" 
                        l.Padding = new Thickness(0);
                        l.HorizontalContentAlignment = HorizontalAlignment.Center;

                        //VerticalAlignment = "Center" HorizontalAlignment = "Center" Padding = "0" HorizontalContentAlignment = "Center" />

                        //stackTracks.Children.Add(l);
                    }
                }

                SetAppActionMode();
                Selected = isSelected;
                if (album != null)
                {
                    try
                    {
                        using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                        {
                            //db.Database.ExecuteSqlCommand("EXECUTE sp_refreshview @viewname = 'LibraryView'");
                            var result = db.Database.SqlQuery<bool>($"EXEC [dbo].[sp_checkIfFaourite] @AlbumID = {album.AlbumId}").ToList();
                            if (result[0] == true)
                            {
                                FavIcon.Visibility = Visibility.Visible;
                                UnfavIcon.Visibility = Visibility.Collapsed;
                            }
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                }

                if (CheckIfAddedToPlaylist())
                {
                    AddIcon.Visibility = Visibility.Collapsed;
                    AddIconBG.Visibility = Visibility.Collapsed;
                    RemoveIcon.Visibility = Visibility.Visible;
                    RemoveIconBG.Visibility = Visibility.Visible;
                }
                else
                {
                    AddIcon.Visibility = Visibility.Visible;
                    AddIconBG.Visibility = Visibility.Visible;
                    RemoveIcon.Visibility = Visibility.Collapsed;
                    RemoveIconBG.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception)
            {

            }
            
        }



        private void Rectanngle_Expand_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //// Pop Up Library Item
                //Global.mPlayer.popUp_Grid.Visibility = Visibility.Visible;
                //Global.mPlayer.popUp_Frame.Content = new LibraryPopup(album, null, CheckIfAddedToPlaylist());
            }
            catch (Exception)
            {

            }
            
            //Global.mPlayer.popUp_Frame.Content = new LibraryPopup(album, songs, this);
            //parent.OpenOverlay(album, songs);
        }

        private bool CheckIfAddedToPlaylist()
        {
            try
            {
                using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                {
                    // Get tracks
                    var albumtracks = (from x in db.AlbumLibraries.Find(album.AlbumId).SongLibraries
                                   where x.TrackLibraries.Any(a => a.Type == Global.AppModeString)
                                   select x.TrackLibraries.First(a2 => a2.Type == Global.AppModeString)).ToList();

                    int count = 0;
                    foreach (var item in albumtracks)
                    {
                        if(db.Playlists.Count(x => x.TrackId == item.Id) > 0)
                        {
                            count++;
                        }
                    }

                    return albumtracks.Count==count;
                }
            }
            catch (Exception)
            {

            }

            return false;
        }

        public bool Selected
        {
            get { return _isAddAll; }
            set
            {
                if (value)
                {
                    //bgSelected.Visibility = Visibility.Visible;
                    //btnAddAll.Visibility = Visibility.Collapsed;

                    //btnRemoveAll.Visibility = Visibility.Visible;
                }
                else
                {
                    //bgSelected.Visibility = Visibility.Collapsed;
                    //btnAddAll.Visibility = Visibility.Visible;

                    //btnRemoveAll.Visibility = Visibility.Collapsed;
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
            //btnAddAll.Visibility = Visibility.Collapsed;
            //btnPlayAll.Visibility = Visibility.Collapsed;
            //btnRemoveAll.Visibility = Visibility.Visible;
        }
        private void Show_btnAddAll()
        {
            //btnAddAll.Visibility = Visibility.Visible;
            //btnPlayAll.Visibility = Visibility.Collapsed;
            //btnRemoveAll.Visibility = Visibility.Collapsed;
        }

        private void Show_btnPlayAll()
        {
            //btnAddAll.Visibility = Visibility.Collapsed;
            //btnPlayAll.Visibility = Visibility.Visible;
            //btnRemoveAll.Visibility = Visibility.Collapsed;
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

        private async void MakeFavourite(object sender, MouseButtonEventArgs e)
        {
            UnfavIcon.Visibility = Visibility.Collapsed;
            FavIcon.Visibility = Visibility.Visible;
            try
            {
                using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                {
                    //db.Database.ExecuteSqlCommand("EXECUTE sp_refreshview @viewname = 'LibraryView'");
                    db.Database.ExecuteSqlCommand($"EXEC [dbo].[sp_Make_Favourite_Unfavourite] @AlbumID = {album.AlbumId}, @isFavourite = 1");
                }
            }
            catch (Exception ex)
            {

            }
        }
        private void MakeUnfavourite(object sender, MouseButtonEventArgs e)
        {
            UnfavIcon.Visibility = Visibility.Visible;
            FavIcon.Visibility = Visibility.Collapsed;
            try
            {
                using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                {
                    db.Database.ExecuteSqlCommand($"EXEC [dbo].[sp_Make_Favourite_Unfavourite] @AlbumID = {album.AlbumId}, @isFavourite = 0");
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void AddToPlayList_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                AddIcon.Visibility = Visibility.Collapsed;
                AddIconBG.Visibility = Visibility.Collapsed;
                RemoveIcon.Visibility = Visibility.Visible;
                RemoveIconBG.Visibility = Visibility.Visible;
                Task.Run(() => RemoveAlbumFromPlaylist());
            }
            catch (Exception)
            {

            }
            
        }

        private void RemoveFromPlaylist_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                AddIcon.Visibility = Visibility.Visible;
                AddIconBG.Visibility = Visibility.Visible;
                RemoveIcon.Visibility = Visibility.Collapsed;
                RemoveIconBG.Visibility = Visibility.Collapsed;
                Task.Run(() => RemoveAlbumFromPlaylist());
            }
            catch (Exception)
            {

            }
            
        }

        private void RemoveAlbumFromPlaylist()
        {
            try
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

                    Global.mPlayer.dbRemoveTrackFromPlaylist(albumtracks, Global.AppPlaylistModeBufferID);
                    Global.mPlayer.dbRemoveTrackFromPlaylist(albumtracks, Global.BufferPlaylistID);

                    //Selected = true;
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

                        Global.mPlayer.dbRemoveTrackFromPlaylist(albumtracks, Global.AppPlaylistModeBufferID);
                        Global.mPlayer.dbRemoveTrackFromPlaylist(albumtracks, Global.BufferPlaylistID);
                        Global.mPlayer.RemoveFromPlaylist(albumtracks.ToList());

                        //Global.AppActionMode = Global.AppActionModeEnum.Playing;
                        //Selected = true;
                    }
                }

            }
            catch (Exception)
            {

            }
        }

        private void AddAlbumToPlaylist()
        {
            try
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

                    //Selected = true;
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

                        Global.mPlayer.dbAddTrackToPlaylist(albumtracks, Global.AppPlaylistModeBufferID);
                        Global.mPlayer.dbAddTrackToPlaylist(albumtracks, Global.BufferPlaylistID);
                        Global.mPlayer.AddToPlaylist(albumtracks.ToList());

                        Global.AppActionMode = Global.AppActionModeEnum.Playing;
                        //Selected = true;
                    }
                }

            }
            catch (Exception)
            {

            }
        }
    }
}
