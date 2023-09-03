using iTunesSearch.Library.Models;
using JukeBoxSolutions.Pages;
using JukeBoxSolutions.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static JukeBoxSolutions.Library.PlayListSource2;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;

namespace JukeBoxSolutions.Controls
{
    public class MainViewModelX
    {
        public String MyEmployee { get; set; } //In reality this should utilize INotifyPropertyChanged!
    }

    /// <summary>
    /// Interaction logic for LibraryCard.xaml
    /// </summary>
    public partial class LibraryCard : UserControl
    {
        public PlaylistServices PlaylistServices = new PlaylistServices();
        Library parent { get; set; }
        public AlbumLibrary album { get; set; }
        List<SongLibrary> songs { get; set; }
        List<Library.PlayListSource2.AlbumTrack> songsMk2 { get; set; }

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
                CoverImage.ImageSource = AlbumCover;
                hasCover = true;
            }
        }
        bool hasCover = false;

        ///TEST Library MK2
        public LibraryCard(AlbumLibrary alphaName, List<Library.PlayListSource2.AlbumTrack> albumTracks, string betaName, bool isSelected = false)
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
        public LibraryCard(AlbumLibrary alphaName, List<Library.PlayListSource2.AlbumTrack> albumTracks, string betaName, Library Parent, bool isSelected = false)
        {
            if (SystemParameters.PrimaryScreenWidth >= 1920)
            {
                ImageRow.Height = new GridLength(230);
                
            }
            else
            {
                ImageRow.Height = new GridLength(190);
            }
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

        public LibraryCard(AlbumLibrary alphaName, List<SongLibrary> itemNames, string betaName, Library Parent, bool isSelected = false)
        {
            InitializeComponent();
            if (SystemParameters.PrimaryScreenWidth >= 1920)
            {
                ImageRow.Height = new GridLength(230);
            }
            else
            {
                ImageRow.Height = new GridLength(174);
            }
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
                // Pop Up Library Item
                Global.mPlayer.popUp_Grid.Visibility = Visibility.Visible;
                Global.mPlayer.popUp_Frame.HorizontalAlignment = HorizontalAlignment.Stretch;
                var blurEffect = new BlurEffect() { Radius = 4, RenderingBias = RenderingBias.Quality };
                blurEffect.KernelType = KernelType.Box;
                Global.MainWindow.MusicLibraryFrame.Effect = blurEffect;
                Global.mPlayer.popUp_Frame.Content = new LibraryPopup(album, this, CheckIfAddedToPlaylist());
                Global.mPlayer.popUp_Frame.ContentRendered += PopUp_Frame_ContentRendered;
            }
            catch (Exception)
            {

            }

            //Global.mPlayer.popUp_Frame.Content = new LibraryPopup(album, songs, this);
            //parent.OpenOverlay(album, songs);
        }

        private void PopUp_Frame_ContentRendered(object sender, EventArgs e)
        {
            if (!Global.mPlayer.popUp_Frame.HasContent)
            {
                Global.MainWindow.MusicLibraryFrame.Effect = null;
            }
        }

        private void PopUp_Frame_MouseDown(object sender, MouseButtonEventArgs e)
        {
        }

        public bool CheckIfAddedToPlaylist()
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
                        var t = db.Playlists.ToList();
                        if (db.Playlists.Count(x => x.TrackId == item.Id && x.PlaylistId > 0) > 0)
                        {
                            count++;
                        }
                    }

                    return count>0;
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
            //if (Global.AppControlMode == Global.AppControlModeEnum.Playlist && Global.LibraryForceViewMode == false)
            //{
            //    List<TrackLibrary> albumtracks = new List<TrackLibrary>();
            //    using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
            //    {
            //        // Get tracks
            //        albumtracks = (from x in db.AlbumLibraries.Find(album.AlbumId).SongLibraries
            //                       where x.TrackLibraries.Any(a => a.Type == Global.AppModeString)
            //                       select x.TrackLibraries.First(a2 => a2.Type == Global.AppModeString)).ToList();
            //    }

            //    Global.mPlayer.dbAddTrackToPlaylist(albumtracks, Global.AppPlaylistModeBufferID);
            //    Global.mPlayer.dbAddTrackToPlaylist(albumtracks, Global.BufferPlaylistID);

            //    Selected = true;
            //}
            //else
            //{
            //    List<TrackLibrary> albumtracks = new List<TrackLibrary>();
            //    using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
            //    {
            //        // Get tracks
            //        albumtracks = (from x in db.AlbumLibraries.Find(album.AlbumId).SongLibraries
            //                       where x.TrackLibraries.Any(a => a.Type == Global.AppModeString)
            //                       select x.TrackLibraries.First(a2 => a2.Type == Global.AppModeString)).ToList();

            //        //Global.mPlayer.AddToPlaylist(albumtracks.ToList());

            //        //Global.AppActionMode = Global.AppActionModeEnum.Playing;
            //        Selected = true;
            //    }
            //}

            using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
            {
                // Get tracks
                var albumtracks = (from x in db.AlbumLibraries.Find(album.AlbumId).SongLibraries
                               where x.TrackLibraries.Any(a => a.Type == Global.AppModeString)
                               select x.TrackLibraries.First(a2 => a2.Type == Global.AppModeString)).ToList();

                PlaylistServices.AddToPlaylist(albumtracks.Select(x => x.Id));
                Selected = true;
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
            UnfavIcon.Visibility = Visibility.Visible;
            //FavIcon.Visibility = Visibility.Visible;
            try
            {
                //using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                //{
                //    //db.Database.ExecuteSqlCommand("EXECUTE sp_refreshview @viewname = 'LibraryView'");
                //    db.Database.ExecuteSqlCommand($"EXEC [dbo].[sp_Make_Favourite_Unfavourite] @AlbumID = {album.AlbumId}, @isFavourite = 1");
                //}

                AddFavBtnStack.Visibility = Visibility.Visible;
                UIElementCollection childs;
                if (Global.AdminSettings.IsMusicLibraryScrollHorizontal)
                {
                    childs = (parent.gridLibraryHorizontal.Children[0] as UniformGrid).Children;
                }
                else
                {
                    childs = (parent.gridLibraryVertical.Children[0] as UniformGrid).Children;
                }

                var currindex = childs.IndexOf(this);
                foreach (var item in childs)
                {
                    var card = item as LibraryCard;
                    if (childs.IndexOf((UIElement)item) != currindex)
                    {
                        if (card.AddFavBtnStack.Visibility == Visibility.Visible)
                        {
                            card.AddFavBtnStack.Visibility = Visibility.Collapsed;
                            card.UnfavIcon.Visibility = Visibility.Visible;
                        }
                    }
                }

            }
            catch (Exception ex)
            {

            }
        }
        private void MakeUnfavourite(object sender, MouseButtonEventArgs e)
        {
            //UnfavIcon.Visibility = Visibility.Visible;
            FavIcon.Visibility = Visibility.Visible;
            try
            {
                //using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                //{
                //    db.Database.ExecuteSqlCommand($"EXEC [dbo].[sp_Make_Favourite_Unfavourite] @AlbumID = {album.AlbumId}, @isFavourite = 0");
                //}

                RemoveFavBtnStack.Visibility = Visibility.Visible;
                UIElementCollection childs;
                if (Global.AdminSettings.IsMusicLibraryScrollHorizontal)
                {
                    childs = (parent.gridLibraryHorizontal.Children[0] as UniformGrid).Children;
                }
                else
                {
                    childs = (parent.gridLibraryVertical.Children[0] as UniformGrid).Children;
                }

                var currindex = childs.IndexOf(this);
                foreach (var item in childs)
                {
                    var card = item as LibraryCard;
                    if (childs.IndexOf((UIElement)item) != currindex)
                    {
                        if (card.RemoveFavBtnStack.Visibility == Visibility.Visible)
                        {
                            card.RemoveFavBtnStack.Visibility = Visibility.Collapsed;
                            card.FavIcon.Visibility = Visibility.Visible;
                        }
                    }
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
                RemoveIcon.Visibility = Visibility.Collapsed;
                RemoveIconBG.Visibility = Visibility.Collapsed;
                AddBtnStack.Visibility = Visibility.Visible;
                UIElementCollection childs = null;
                if (Global.AdminSettings.IsMusicLibraryScrollHorizontal)
                {
                    childs = (parent.gridLibraryHorizontal.Children[0] as UniformGrid).Children;
                }
                else
                {
                    childs = (parent.gridLibraryVertical.Children[0] as UniformGrid).Children;
                }

                var currindex = childs.IndexOf(this);
                foreach (var item in childs)
                {
                    var card = item as LibraryCard;
                    if (childs.IndexOf((UIElement)item) != currindex)
                    {
                        if (card.AddBtnStack.Visibility == Visibility.Visible)
                        {
                            card.AddBtnStack.Visibility = Visibility.Collapsed;
                            card.AddIcon.Visibility = Visibility.Visible;
                            card.AddIconBG.Visibility = Visibility.Visible;
                        }
                    }
                }
            }
            catch (Exception)
            {

            }

        }

        private void RemoveFromPlaylist_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                RemoveIcon.Visibility = Visibility.Collapsed;
                RemoveIconBG.Visibility = Visibility.Collapsed;
                RemoveBtnStack.Visibility = Visibility.Visible;
                UIElementCollection childs = null;
                if (Global.AdminSettings.IsMusicLibraryScrollHorizontal)
                {
                    childs = (parent.gridLibraryHorizontal.Children[0] as UniformGrid).Children;
                }
                else
                {
                    childs = (parent.gridLibraryVertical.Children[0] as UniformGrid).Children;
                }

                var currindex = childs.IndexOf(this);
                foreach (var item in childs)
                {
                    var card = item as LibraryCard;
                    if (childs.IndexOf((UIElement)item) != currindex)
                    {
                        if (card.RemoveBtnStack.Visibility == Visibility.Visible)
                        {
                            card.RemoveBtnStack.Visibility = Visibility.Collapsed;
                            card.RemoveIcon.Visibility = Visibility.Visible;
                            card.RemoveIconBG.Visibility = Visibility.Visible;
                        }
                    }
                }
            }
            catch (Exception)
            {

            }

        }

        public void RemoveAlbumFromPlaylist()
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
                    Global.mPlayer.dbRemoveTrackFromPlaylist(albumtracks, Global.NowPlayingId);

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

                        //Global.mPlayer.dbRemoveTrackFromPlaylist(albumtracks, Global.AppPlaylistModeBufferID);
                        //Global.mPlayer.dbRemoveTrackFromPlaylist(albumtracks, Global.BufferPlaylistID);
                        //Global.mPlayer.RemoveFromPlaylist(albumtracks.ToList());
                        //Global.AppActionMode = Global.AppActionModeEnum.Playing;
                        //Selected = true;
                        foreach (var item in albumtracks)
                        {
                            if (item.Playlists.Any())
                            {
                                var playlists = item.Playlists.ToList();
                                foreach (var t in playlists)
                                {
                                    var playlist = db.Playlists.FirstOrDefault(x => x.Id == t.Id);
                                    db.Playlists.Remove(playlist);
                                    db.SaveChanges();

                                }
                            }
                        }

                    }
                }

            }
            catch (Exception ex)
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

                        foreach (var baseTrack in albumtracks)
                        {
                            Global.mPlayer.dbAddTrackToPlaylist(baseTrack, Global.NowPlayingId);
                        }

                        if (Global.mPlayer.SongPlayingArtWork == null)
                        {
                            PlayPlaylist(db.Playlists.Select(x => x.TrackLibrary).ToList());
                        }                        
                    }
                }


            }
            catch (Exception)
            {

            }
        }

        private void PlayPlaylist(List<TrackLibrary> trackLibraries)
        {
            try
            {
                Global.MainWindow.SongPlayingArtworkFrame.Visibility = Visibility.Visible;
                Global.MainFrame.Visibility = Visibility.Collapsed;
                using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                {
                    Global.MainWindow.Dispatcher.InvokeAsync(() =>
                    {
                        Global.MainWindow.btnLogo.IsEnabled = false;
                        Global.MainWindow.btnKeyboard.IsEnabled = false;
                        Global.MainWindow.btnMediaPanel.IsEnabled = false;
                        var songPlayingArtworkcontrol = new SongPlayingArtwork(trackLibraries);
                        Global.MainWindow.SongPlayingArtworkFrame.Content = songPlayingArtworkcontrol;
                        Global.MainWindow.sParentPanel.Visibility = Visibility.Collapsed;
                        Global.MainWindow.btnKeyboard.Visibility = Visibility.Collapsed;
                        Global.MainWindow.btnMediaPanel.Visibility = Visibility.Collapsed;
                        Global.MainWindow.PlaylistControlPanel.Visibility = Visibility.Visible;
                        if (Global.AdminSettings.AudioVisualizerSize != Global.ClassAdminSettings.Quality.Off)
                        {
                            Global.MainWindow.MainBackgroundBG.Visibility = Visibility.Collapsed;
                        }
                        Global.MainWindow.ControlPanelBG.Visibility = Visibility.Collapsed;
                        Global.MainWindow.PlaylistControlPanel.Height = 120;
                        //Global.MainWindow.VolumeTracker.Value = 70;
                        Global.mPlayer.popUp_Grid.Visibility = Visibility.Collapsed;
                        Global.mPlayer.popUp_Frame.Content = null;
                        Global.MainWindow.btnPlay1.IsEnabled = true;
                        Global.MainWindow.btnPlay1.Visibility = Visibility.Collapsed;
                        Global.MainWindow.btnPause.IsEnabled = true;
                        Global.MainWindow.btnPause.Visibility = Visibility.Visible;
                        //Global.MainWindow.VolumnPanel.Visibility = Visibility.Visible;
                        Global.MainWindow.btnLogo.IsEnabled = true;
                        Global.MainWindow.btnKeyboard.IsEnabled = true;
                        Global.MainWindow.btnMediaPanel.IsEnabled = true;
                        Global.MainWindow.btnPreviousMenu.Visibility = Visibility.Collapsed;
                        Global.MainWindow.btnNextMenu.Visibility = Visibility.Collapsed;
                        Global.MainWindow.ControlPanelBG.Visibility = Visibility.Visible;
                        Global.MainWindow.btnInfo.IsEnabled = true;
                        Global.MainWindow.btnInfo.Opacity = 1;
                        //Global.MainWindow.MusicLibraryFrame.Visibility = Visibility.Collapsed;
                        //Global.MainWindow.KaraokeLibraryFrame.Visibility = Visibility.Collapsed;
                        //Global.MainWindow.VideoLibraryFrame.Visibility = Visibility.Collapsed;
                        //Global.MainWindow.RadioLibraryFrame.Visibility = Visibility.Collapsed;
                        //Global.MainWindow.LibraryButtonControlPanel.Visibility = Visibility.Collapsed;
                        //Global.MainWindow.InitializePlayer();
                        //Selected = true;
                    });
                }
            }
            catch (Exception)
            {

            }
        }

        public async void AddAlbumToPlaylist(object sender, MouseButtonEventArgs e)
        {
            try
            {
                AddAlbumToPlaylist();
                AddBtnStack.Visibility = Visibility.Collapsed;
                AddIcon.Visibility = Visibility.Collapsed;
                AddIconBG.Visibility = Visibility.Collapsed;
                RemoveIcon.Visibility = Visibility.Visible;
                RemoveIconBG.Visibility = Visibility.Visible;
                await Task.Delay(1000);
                using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                {                    
                    if (Global.mPlayer.SongPlayingArtWork == null)
                    {
                        PlayPlaylist(db.Playlists.Select(x => x.TrackLibrary).ToList());
                    }
                }
            }
            catch (Exception)
            {

            }
        }

        private void AddTrackToPlaylist(object sender, MouseButtonEventArgs e)
        {
            try
            {
                AddBtnStack.Visibility = Visibility.Collapsed;
                AddIcon.Visibility = Visibility.Visible;
                AddIconBG.Visibility = Visibility.Visible;
                Rectanngle_Expand_Click(null, null);
            }
            catch (Exception)
            {

            }
        }

        private void CloseBtnStack(object sender, MouseButtonEventArgs e)
        {
            AddBtnStack.Visibility = Visibility.Collapsed;
            AddIcon.Visibility = Visibility.Visible;
            AddIconBG.Visibility = Visibility.Visible;
        }

        public async void RemoveAlbumFromPlaylist(object sender, MouseButtonEventArgs e)
        {
            this.RemoveAlbumFromPlaylist();
            this.CloseRemoveBtnStack(sender, e);
            this.CloseBtnStack(sender, e);
            RemoveIcon.Visibility = Visibility.Collapsed;
            RemoveIconBG.Visibility = Visibility.Collapsed;
            await Task.Delay(500);

            using(JukeboxBrainsDBEntities db=new JukeboxBrainsDBEntities())
            {
                var list = db.Playlists.ToList();
                if (list.Any())
                {
                    PlayPlaylist(db.Playlists.Select(x => x.TrackLibrary).ToList());
                }
                else
                {
                    Global.mPlayer.Stop();
                    Global.mPlayer.SongPlayingArtWork = null;
                    Global.MainWindow.MainBackgroundBG.Visibility = Visibility.Visible;
                }
            }
        }

        private void RemoveTrackFromPlaylist(object sender, MouseButtonEventArgs e)
        {
            RemoveBtnStack.Visibility = Visibility.Collapsed;
            RemoveIcon.Visibility = Visibility.Visible;
            RemoveIconBG.Visibility = Visibility.Visible;
            Rectanngle_Expand_Click(null, null);
        }

        private void CloseRemoveBtnStack(object sender, MouseButtonEventArgs e)
        {
            AddIcon.Visibility = Visibility.Collapsed;
            AddIconBG.Visibility = Visibility.Collapsed;
            RemoveIcon.Visibility = Visibility.Visible;
            RemoveIconBG.Visibility = Visibility.Visible;
            RemoveBtnStack.Visibility = Visibility.Collapsed;
        }

        private void MakeFavourite(object sender, RoutedEventArgs e)
        {

        }

        public void AddAlbumToFavourite(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (FavIcon.Visibility == Visibility.Collapsed)
                {
                    using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                    {
                        db.Database.ExecuteSqlCommand($"EXEC [dbo].[sp_Make_Favourite_Unfavourite] @AlbumID = {album.AlbumId}, @isFavourite = 1");
                    }

                    AddFavBtnStack.Visibility = Visibility.Collapsed;
                    FavIcon.Visibility = Visibility.Visible;
                    UnfavIcon.Visibility = Visibility.Collapsed;
                    FavIcon.Width = 20;
                    FavIcon.Height = 20;
                    FavIcon.MouseDown -= MakeUnfavourite;
                    AddFavInfoButton.Visibility = Visibility.Visible;
                    var a = new DoubleAnimation
                    {
                        From = 1.0,
                        To = 0.0,
                        FillBehavior = FillBehavior.Stop,
                        BeginTime = TimeSpan.FromSeconds(2),
                        Duration = new Duration(TimeSpan.FromSeconds(0.3))
                    };
                    var storyboard = new Storyboard();

                    storyboard.Children.Add(a);
                    Storyboard.SetTarget(a, AddFavInfoButton);
                    Storyboard.SetTargetProperty(a, new PropertyPath(OpacityProperty));
                    storyboard.Completed += delegate
                    {
                        AddFavInfoButton.Visibility = System.Windows.Visibility.Collapsed;
                        FavIcon.Width = 24;
                        FavIcon.Height = 24;
                        FavIcon.MouseDown -= MakeUnfavourite;
                        FavIcon.MouseDown += MakeUnfavourite;
                    };
                    storyboard.Begin();
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void AddTrackToFavourite(object sender, MouseButtonEventArgs e)
        {
            try
            {
                AddFavBtnStack.Visibility = Visibility.Collapsed;
                Rectanngle_Expand_Click(null, null);
            }
            catch (Exception)
            {

            }
        }

        private void CloseAddFavBtnStack(object sender, MouseButtonEventArgs e)
        {
            AddFavBtnStack.Visibility = Visibility.Collapsed;
        }

        private void RemoveAlbumFromFav(object sender, MouseButtonEventArgs e)
        {
            try
            {
                RemoveFavBtnConfirmationStack.Visibility = Visibility.Visible;
                RemoveFavBtnStack.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {

            }
        }

        private void RemoveTrackFromFav(object sender, MouseButtonEventArgs e)
        {
            try
            {
                RemoveFavBtnStack.Visibility = Visibility.Collapsed;
                Rectanngle_Expand_Click(null, null);
            }
            catch (Exception)
            {

            }
        }

        private void CloseRemoveFavBtnStack(object sender, MouseButtonEventArgs e)
        {
            RemoveFavBtnStack.Visibility = Visibility.Collapsed;
        }

        public void RemoveYes(object sender, MouseButtonEventArgs e)
        {
            if (UnfavIcon.Visibility != Visibility.Collapsed) { return; }
            try
            {
                using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                {
                    db.Database.ExecuteSqlCommand($"EXEC [dbo].[sp_Make_Favourite_Unfavourite] @AlbumID = {album.AlbumId}, @isFavourite = 0");
                    var listA = db.LibraryViews.Where(w => w.AlbumId == album.AlbumId && w.Type == Global.AppModeString).ToList();
                    var listS = listA.Select(s => s.SongId);
                    var songList = db.SongLibraries.Where(w2 => listS.Contains(w2.SongId)).ToList();
                    var trackLibraryList = songList.Select(x => x.TrackLibraries.First()).ToList();
                    foreach (var item in trackLibraryList)
                    {
                        db.Database.ExecuteSqlCommand($"EXEC [dbo].[sp_MakeTrack_Favourite_Unfavourite] @TrackID = {item.Id}, @isFavourite = 0");
                    }
                }
            }
            catch (Exception)
            {

            }
            

            RemoveFavBtnStack.Visibility = Visibility.Collapsed;
            RemoveFavBtnConfirmationStack.Visibility = Visibility.Collapsed;
            FavIcon.Visibility = Visibility.Collapsed;
            UnfavIcon.Visibility = Visibility.Visible;
            UnfavIcon.Width = 20;
            UnfavIcon.Height = 20;

            UnfavIcon.MouseDown -= MakeFavourite;
            RemoveFavInfoButton.Visibility = Visibility.Visible;
            var a = new DoubleAnimation
            {
                From = 1.0,
                To = 0.0,
                FillBehavior = FillBehavior.Stop,
                BeginTime = TimeSpan.FromSeconds(1),
                Duration = new Duration(TimeSpan.FromSeconds(0.3))
            };
            var storyboard = new Storyboard();

            storyboard.Children.Add(a);
            Storyboard.SetTarget(a, AddFavInfoButton);
            Storyboard.SetTargetProperty(a, new PropertyPath(OpacityProperty));
            storyboard.Completed += delegate
            {
                RemoveFavInfoButton.Visibility = System.Windows.Visibility.Collapsed;
                UnfavIcon.Width = 24;
                UnfavIcon.Height = 24;
                UnfavIcon.MouseDown += MakeFavourite;
            };
            storyboard.Begin();
        }

        private void RemoveNo(object sender, MouseButtonEventArgs e)
        {
            RemoveFavBtnConfirmationStack.Visibility = Visibility.Collapsed;
        }
    }
}
