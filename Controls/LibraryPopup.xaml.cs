using iTunesSearch.Library;
using iTunesSearch.Library.Models;
using JukeBoxSolutions.Class;
using JukeBoxSolutions.Pages;
using JukeBoxSolutions.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using static JukeBoxSolutions.Library.PlayListSource2;

namespace JukeBoxSolutions.Controls
{
    /// <summary>
    /// Interaction logic for LibraryPopup.xaml
    /// </summary>
    public partial class LibraryPopup : UserControl
    {
        private PlaylistServices PlaylistServices = new PlaylistServices();
        internal class AlbumSong
        {
            public SongLibrary BaseSong { get; set; }
            public TrackLibrary BaseTrack { get; set; }
        }

        private List<string> _albumArtists = new List<string>();
        internal List<string> AlbumArtists
        {
            get { return _albumArtists; }
            set
            {
                if (value.Count > 1)
                {
                    lblSubName.Content = "Various Artists";
                    //lblSubNameB.Content = "Various Artists";
                    //btnSearchArtist.Visibility = Visibility.Visible;
                }
                else if (value.Any())
                {
                    lblSubName.Content = value.First();
                    //lblSubNameB.Content = value.First();
                    //btnSearchArtist.Visibility = Visibility.Visible;
                }
                else
                {
                    lblSubName.Content = "";
                    //lblSubNameB.Content = "";
                    //btnSearchArtist.Visibility = Visibility.Collapsed;
                }

                _albumArtists = value;
            }
        }


        internal BitmapImage AlbumCover = new BitmapImage();

        List<SongLibrary> SongList { get; set; }
        internal AlbumLibrary baseAlbum { get; set; }
        public LibraryCard baseControlCard { get; set; }
        private int activeSongsCounter = 0;
        internal void SongActiveCheckIn() { activeSongsCounter++; }
        internal bool SetBaseAlbumSelected
        {
            set
            {
                if (value == true)
                {
                    if (activeSongsCounter == 0 && baseControlCard!=null)
                    {
                        baseControlCard.SetAppActionMode();
                        baseControlCard.Selected = value;
                    }
                    activeSongsCounter++;
                }
                else
                {
                    activeSongsCounter--;
                    if (activeSongsCounter == 0 && baseControlCard != null)
                    {
                        baseControlCard.SetAppActionMode();
                        baseControlCard.Selected = value;
                    }
                }
            }
        }
        PlayListDetail basePlaylist { get; set; }
        bool isPlaylist = false;
        bool isAlbum = false;
        string filepath = "";

        public LibraryPopup(PlayListDetail playList)
        {
            InitializeComponent();
            // hide album stuff
            var height = this.ActualHeight;
            var t = from s in playList.Playlists.OrderBy(o => o.SequenceNumber)
                    select s.TrackLibrary;

            basePlaylist = playList;
            isPlaylist = true;
            SongList = (from a in t select a.SongLibraries.First()).ToList();
            //SongList = (from a in t select new AlbumSong { BaseSong = a.SongLibraries.First(), BaseTrack = a }).ToList();

            PopulateSonglist();
            lblAlubmName.Content = playList.Name;
            lblSubName.Content = "";

        }

        public void LoadCover()
        {
            try
            {
                System.Drawing.Bitmap bmp;
                using (var ms = new MemoryStream(baseAlbum.CoverArt))
                {
                    AlbumCover.BeginInit();
                    AlbumCover.CacheOption = BitmapCacheOption.OnLoad;
                    AlbumCover.StreamSource = ms;
                    AlbumCover.EndInit();

                    //AlbumImage.Source = AlbumCover;
                    AlbumImageInBackground.Source = AlbumCover;
                    RoundAlbumButtonImage.ImageSource = AlbumCover;

                }

            }
            catch
            {

            }
        }

        private int _playlistCount;
        public int PlaylistCount
        {
            get => _playlistCount;
            set
            {
                PlaylistTracksCount.Content= value;
                _playlistCount = value;
            }
        }

        public List<PlaylistWithActionTaken> PLaylists { get; set; }=new List<PlaylistWithActionTaken>();
        public List<PlaylistWithActionTaken> FavPLaylists { get; set; }=new List<PlaylistWithActionTaken>();
        public LibraryPopup(AlbumLibrary album, LibraryCard controlCard, bool IsAddedToPlaylist=false)
        {
            // Load from DB directly
            var height = this.Height;
            baseControlCard = controlCard;
            baseAlbum = album;
            isAlbum = true;

            InitializeComponent();
            if (album.CoverArt != null)
            {
                LoadCover();
            }
            if (IsAddedToPlaylist)
            {
                BtnAddToPlaylist.Visibility = Visibility.Collapsed;
                btnRemoveFromPlaylist.Visibility = Visibility.Visible;
            }
            
            using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
            {
                // Get correct songs from View...
                var listA = db.LibraryViews.Where(w => w.AlbumId == album.AlbumId && w.Type == Global.AppModeString).ToList();
                var listS = listA.Select(s => s.SongId);
                var listT = listA.Select(s => s.TrackId);

                SongList = db.SongLibraries.Where(w2 => listS.Contains(w2.SongId)).ToList();

                PlaylistCount = db.Playlists.ToList().GroupBy(x=>x.TrackId).Count();

                AlbumArtists = SongList.SelectMany(sm => sm.Artists).Select(s1 => s1.ArtistName).Distinct().ToList();

                var dirList = db.TrackLibraries.Where(w1 => listT.Contains(w1.Id)).Select(s1 => s1.FilePath).ToList();
                if(dirList!=null && dirList.Count() > 0)
                {
                    System.IO.DirectoryInfo di = new DirectoryInfo(dirList.First());

                    filepath = di.Parent.FullName;
                }
                


                //SongList = (from s in db.SongLibraries select new AlbumSong { BaseSong = s, BaseTrack = s.TrackLibraries.First(f => f.Type == Global.AppModeString) }).ToList();
            }
            //SongList = songs.ToList();
            
            PopulateSonglist();
            lblAlubmName.Content = album.AlbumName;
            //lblAlubmNameBg.Content = album.AlbumName;


            //if (Global.AppControlMode == Global.AppControlModeEnum.Admin)
            //    btnUpdateOnlineDetails.Visibility = Visibility.Visible;
            //else
            //    btnUpdateOnlineDetails.Visibility = Visibility.Collapsed;

            if (SongList.Count() > pageCount)
                btnNext.IsEnabled = true;

            //btnUpdateOnlineDetails.Visibility = Visibility.Collapsed;
            //btnEditAlbum.Visibility = Visibility.Collapsed;
        }

        private void PopulatePlaylistCount()
        {
            try
            {
                using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                {
                    // Get tracks
                    var albumtracks = (from x in db.AlbumLibraries.Find(baseAlbum.AlbumId).SongLibraries
                                       where x.TrackLibraries.Any(a => a.Type == Global.AppModeString)
                                       select x.TrackLibraries.First(a2 => a2.Type == Global.AppModeString)).ToList();

                    int count = 0;
                    foreach (var item in albumtracks)
                    {
                        var t = db.Playlists.ToList();
                        if (db.Playlists.Count(x => x.TrackId == item.Id && x.PlaylistId > 0) > 0)
                        {
                            PlaylistCount++;
                        }
                    }

                }
            }
            catch (Exception)
            {

            }
        }

        int page
        {
            get
            {
                return _page;
            }
            set
            {
                if (value == 1)
                    btnPrevious.IsEnabled = false;
                else
                    btnPrevious.IsEnabled = true;

                if (value * pageCount >= SongList.Count)
                    btnNext.IsEnabled = false;
                else
                    btnNext.IsEnabled = true;

                _page = value;
            }
        }

        internal void UpdateAppActionMode()
        {
            foreach (var c in stackSongItems.Children.OfType<LibraryCardSong>())
            {
                c.SetAppActionMode();
            }
        }

        int _page = 1;
        int pageCount = 8;
        public void PopulateSonglist()
        {
            stackSongItems.Children.Clear();            
            using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
            {
                int i = 1;
                BtnMusicCount.Content = SongList.Count;
                foreach (SongLibrary s in SongList)
                {
                    // 1 * 10 - 10 = 0 .. 10
                    // 2 * 10 - 10 = 10 .. 20
                    if (i > (page * pageCount) - pageCount && i <= (page * pageCount))
                    {
                        var sx = db.SongLibraries.Find(s.SongId);                        
                        stackSongItems.Children.Add(new LibraryCardSong(sx.TrackLibraries.First(), i.ToString(), this));
                    }

                    i++;
                }
            }
        }

        private void btnUpdateOnlineDetails_Click(object sender, RoutedEventArgs e)
        {
            if (isAlbum)
            {
                iTunesSearchManager ism = new iTunesSearchManager();

                // give label / control to update / reload
                ism.UpdateSearchAlbum(baseAlbum, this);
                //ism.UpdateSearchAlbumAsync(baseAlbum, this);

                //ExampleFactory.MusicBrainz online = new ExampleFactory.MusicBrainz();
                //    online.RunSongSearch(SongList.First().Artists.First().ArtistName, SongList.First().SongName, baseAlbum.AlbumName);
            }
        }

        private void btnPlayAll_Click(object sender, RoutedEventArgs e)
        {
            if (isPlaylist)
            {
                // play playlist
                Global.mPlayer.StartPlaylist(basePlaylist.Name, false, true);
            }
            else
            {
                Global.MainWindow.SongPlayingArtworkFrame.Visibility = Visibility.Visible;
                Global.MainFrame.Visibility = Visibility.Collapsed;
                List<TrackLibrary> albumtracks = new List<TrackLibrary>();
                using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                {
                    // Get tracks
                    albumtracks = (from x in db.AlbumLibraries.Find(baseAlbum.AlbumId).SongLibraries
                                   where x.TrackLibraries.Any(a => a.Type == Global.AppModeString)
                                   select x.TrackLibraries.First(a2 => a2.Type == Global.AppModeString)).ToList();
                    AddToPlaylistIfNotAdded(albumtracks);
                    Global.MainWindow.Dispatcher.InvokeAsync(() =>
                    {
                        Global.MainWindow.btnLogo.IsEnabled = false;
                        Global.MainWindow.btnKeyboard.IsEnabled = false;
                        Global.MainWindow.btnMediaPanel.IsEnabled = false;
                        var songPlayingArtworkcontrol = new SongPlayingArtwork(baseAlbum, albumtracks,baseControlCard);
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
                        Global.MainWindow.MusicLibraryFrame.Visibility = Visibility.Collapsed;
                        Global.MainWindow.KaraokeLibraryFrame.Visibility = Visibility.Collapsed;
                        Global.MainWindow.VideoLibraryFrame.Visibility = Visibility.Collapsed;
                        Global.MainWindow.RadioLibraryFrame.Visibility = Visibility.Collapsed;
                        Global.MainWindow.LibraryButtonControlPanel.Visibility = Visibility.Collapsed;
                        //Global.MainWindow.InitializePlayer();
                        //Selected = true;
                    });
                }
            }
        }

        private void AddToPlaylistIfNotAdded(List<TrackLibrary> albumtracks)
        {
            try
            {
                using (JukeboxBrainsDBEntities db=new JukeboxBrainsDBEntities())
                {
                    var playlists = db.Playlists.ToList();
                    int i = 0;
                    var Cards = new List<PlaylistCardSong>();
                    foreach (var item in playlists)
                    {
                        var trackLib = item.TrackLibrary;
                        var card = new PlaylistCardSong(trackLib, (i + 1).ToString(), null);
                        Cards.Add(card);
                        i++;
                    }

                    db.Database.ExecuteSqlCommand("delete from Playlists where [PlaylistId] <> 1");
                    db.Database.ExecuteSqlCommand("DBCC CHECKIDENT ([Playlists]);");
                    db.Database.ExecuteSqlCommand("DBCC CHECKIDENT ([PlayListDetails]);");
                    db.SaveChangesAsync();

                    foreach (var item in albumtracks)
                    {
                        var t = db.Playlists.ToList();
                        if (db.Playlists.Count(x => x.TrackId == item.Id && x.PlaylistId > 0) == 0)
                        {
                            Global.mPlayer.dbAddTrackToPlaylist(item, Global.NowPlayingId);
                        }
                    }

                    foreach (var item in Cards)
                    {
                        Global.mPlayer.dbAddTrackToPlaylist(item.baseTrack, Global.NowPlayingId);
                    }
                }
            }
            catch (Exception)
            {

            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // Previous
            page--;
            PopulateSonglist();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            page++;
            PopulateSonglist();
        }

        private void btnEditAlbum_Click(object sender, RoutedEventArgs e)
        {
            Global.mPlayer.BumpPopUp(new Popup_SmartUpdater(baseAlbum, baseControlCard));
            Global.mPlayer.popUp_Grid.Visibility = System.Windows.Visibility.Visible;
            //Global.mPlayer.popUp_Frame.Content = new Popup_SmartUpdater(baseTrack);
        }

        private void btnSearchArtist_Click(object sender, RoutedEventArgs e)
        {
            ((Library)Global.MainFrame.Content).SetSearchFilter(AlbumArtists, Library.SearchMode.Artist);
        }

        private void btnAutoUpdate_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnForceCoverLoad_Click(object sender, RoutedEventArgs e)
        {
            // Grab Album Art
            var d = System.IO.Directory.GetFiles(filepath);

            string coverFile = "";
            foreach (string s in d)
            {
                if (s.ToLower().Contains("cover"))
                {
                    coverFile = s;
                    break;
                }
                else if (s.ToLower().Contains("front"))
                {
                    coverFile = s;
                    break;
                }
            }

            if (!string.IsNullOrEmpty(coverFile))
            {
                System.Drawing.Image img = System.Drawing.Image.FromFile(coverFile);
                using (MemoryStream ms = new MemoryStream())
                {
                    if (coverFile.ToLower().Contains(".jpg") || coverFile.ToLower().Contains(".jpeg"))
                        img.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                    else if (coverFile.ToLower().Contains(".png"))
                        img.Save(ms, System.Drawing.Imaging.ImageFormat.Png);

                    baseAlbum.CoverArt = ms.ToArray();

                    using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                    {
                        var a = db.AlbumLibraries.Find(baseAlbum.AlbumId);
                        a.CoverArt = baseAlbum.CoverArt;
                        db.Entry(a).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                    }
                }
            }
            else
            {
                //btnForceCoverLoad.Content = "Not found";
            }

            //btnForceCoverLoad.IsEnabled = false;
        }

        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            ExportFactory f = new ExportFactory();
            //btnExport.Content = "Working";
            //btnExport.IsEnabled = false;
            using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
            {
                List<TrackLibrary> albumtracks = new List<TrackLibrary>();
                var id = (from ab in db.LibraryViews
                          where ab.AlbumId == baseAlbum.AlbumId && ab.Type == Global.AppModeString
                          select ab.TrackId).ToList();

                albumtracks = db.TrackLibraries.Where(tr => id.Contains(tr.Id)).ToList();

                f.ExportTracks(albumtracks);
            }
            //btnExport.Content = "Done!";
        }

        private void btnSearchAlbum_Click(object sender, RoutedEventArgs e)
        {
            //((Library)Global.MainFrame.Content).SetSearchFilter(AlbumArtists, Library.SearchMode.Artist);

        }

        private void btnAddAll_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Global.AppControlMode == Global.AppControlModeEnum.Playlist && Global.LibraryForceViewMode == false)
                {
                    List<TrackLibrary> albumtracks = new List<TrackLibrary>();
                    using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                    {
                        // Get tracks
                        albumtracks = (from x in db.AlbumLibraries.Find(baseAlbum.AlbumId).SongLibraries
                                       where x.TrackLibraries.Any(a => a.Type == Global.AppModeString)
                                       select x.TrackLibraries.First(a2 => a2.Type == Global.AppModeString)).ToList();
                    }

                    Global.mPlayer.dbAddTrackToPlaylist(albumtracks, Global.NowPlayingId);
                    Global.mPlayer.dbAddTrackToPlaylist(albumtracks, Global.BufferPlaylistID);
                    //PlaylistServices.AddToPlaylist(albumtracks.Select(x => x.Id));
                    //Selected = true;
                }
                else
                {
                    List<TrackLibrary> albumtracks = new List<TrackLibrary>();
                    using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                    {
                        // Get tracks
                        albumtracks = (from x in db.AlbumLibraries.Find(baseAlbum.AlbumId).SongLibraries
                                       where x.TrackLibraries.Any(a => a.Type == Global.AppModeString)
                                       select x.TrackLibraries.First(a2 => a2.Type == Global.AppModeString)).ToList();

                        Global.mPlayer.dbAddTrackToPlaylist(albumtracks, Global.NowPlayingId);
                    }
                }
                
                Global.mPlayer.popUp_Grid.Visibility = Visibility.Collapsed;
                Global.mPlayer.popUp_Frame.Content = null;
                Global.mPlayer.popUp_Frame.Content = new LibraryPopup(baseAlbum,baseControlCard,true);
                Global.mPlayer.popUp_Grid.Visibility=Visibility.Visible;
                
            }
            catch (Exception)
            {

            }            
        }

        private void btnRemoveAll_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //if (Global.AppControlMode == Global.AppControlModeEnum.Playlist && Global.LibraryForceViewMode == false)
                //{
                //    List<TrackLibrary> albumtracks = new List<TrackLibrary>();
                //    using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                //    {
                //        // Get tracks
                //        albumtracks = (from x in db.AlbumLibraries.Find(baseAlbum.AlbumId).SongLibraries
                //                       where x.TrackLibraries.Any(a => a.Type == Global.AppModeString)
                //                       select x.TrackLibraries.First(a2 => a2.Type == Global.AppModeString)).ToList();
                //    }

                //    Global.mPlayer.dbRemoveTrackFromPlaylist(albumtracks, Global.BufferPlaylistID);

                //    //Selected = true;
                //}
                //else
                //{
                //    List<TrackLibrary> albumtracks = new List<TrackLibrary>();
                //    using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                //    {
                //        // Get tracks
                //        albumtracks = (from x in db.AlbumLibraries.Find(baseAlbum.AlbumId).SongLibraries
                //                       where x.TrackLibraries.Any(a => a.Type == Global.AppModeString)
                //                       select x.TrackLibraries.First(a2 => a2.Type == Global.AppModeString)).ToList();

                //        Global.mPlayer.dbRemoveTrackFromPlaylist(albumtracks, Global.BufferPlaylistID);
                //    }
                //}

                //Global.mPlayer.popUp_Grid.Visibility = Visibility.Collapsed;
                //Global.mPlayer.popUp_Frame.Content = null;
                //Global.mPlayer.popUp_Frame.Content = new LibraryPopup(baseAlbum, baseControlCard, false);
                //Global.mPlayer.popUp_Grid.Visibility = Visibility.Visible;

                this.baseControlCard?.RemoveAlbumFromPlaylist(null, null);
                BtnAddToPlaylist.Visibility = Visibility.Visible;
                btnRemoveFromPlaylist.Visibility = Visibility.Collapsed;
                PopulateSonglist();
            }
            catch (Exception)
            {

            }
        }

        private void BtnCancelPopup(object sender, RoutedEventArgs e)
        {
            try
            {
                Global.mPlayer.popUp_Grid.Visibility = Visibility.Collapsed;
                Global.mPlayer.popUp_Frame.Content = null;
                if (baseControlCard != null)
                {
                    baseControlCard.AddBtnStack.Visibility = Visibility.Collapsed;
                    baseControlCard.RemoveBtnStack.Visibility = Visibility.Collapsed;
                }
                
                PLaylists = new List<PlaylistWithActionTaken>();
                FavPLaylists = new List<PlaylistWithActionTaken>();
            }
            catch (Exception)
            {

            }
        }

        private void ClosePopup(object sender, MouseButtonEventArgs e)
        {
            Global.mPlayer.popUp_Grid.Visibility = Visibility.Collapsed;
            Global.mPlayer.popUp_Frame.Content = null;
        }

        private void PlayPlaylist(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Global.MainWindow.SongPlayingArtworkFrame.Visibility = Visibility.Visible;
                Global.MainFrame.Visibility = Visibility.Collapsed;
                using (JukeboxBrainsDBEntities db=new JukeboxBrainsDBEntities())
                {
                    var albumtracks = (from x in db.AlbumLibraries.Find(baseAlbum.AlbumId).SongLibraries
                                   where x.TrackLibraries.Any(a => a.Type == Global.AppModeString) 
                                   select x.TrackLibraries.First(a2 => a2.Type == Global.AppModeString)).ToList();

                    this.baseControlCard.AddAlbumToPlaylist(null, null);
                    Global.MainWindow.Dispatcher.InvokeAsync(() =>
                    {
                        Global.MainWindow.btnLogo.IsEnabled = false;
                        Global.MainWindow.btnKeyboard.IsEnabled = false;
                        Global.MainWindow.btnMediaPanel.IsEnabled = false;
                        var songPlayingArtworkcontrol = new SongPlayingArtwork(baseAlbum, albumtracks, baseControlCard, isCarouselVisible:false);
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

        public void BtnDonePopup(object sender, RoutedEventArgs e)
        {
            try
            {
                Global.mPlayer.popUp_Grid.Visibility = Visibility.Collapsed;
                Global.mPlayer.popUp_Frame.Content = null;
                if (baseControlCard != null)
                {
                    baseControlCard.AddBtnStack.Visibility = Visibility.Collapsed;
                    baseControlCard.RemoveBtnStack.Visibility = Visibility.Collapsed;
                }
                
                if (PLaylists.Count > 0)
                {
                    var plList = new List<Playlist>();
                    using (JukeboxBrainsDBEntities db=new JukeboxBrainsDBEntities())
                    {
                        plList = db.Playlists.ToList();
                    }
                    foreach (var item in PLaylists)
                    {
                        if (item.ToAdd)
                        {         
                            if(plList.Count(x => x.TrackId == item.TrackLibrary.Id) == 0)
                            {
                                Global.mPlayer.dbAddTrackToPlaylist(item.TrackLibrary, Global.NowPlayingId);
                            }                            
                        }
                        else
                        {
                            if (plList.Count(x => x.TrackId == item.TrackLibrary.Id)> 0)
                            {
                                Global.mPlayer.dbRemoveTrackFromPlaylist(item.TrackLibrary, Global.NowPlayingId);
                            }
                                
                        }
                       
                    }

                }

                if (baseControlCard != null)
                {
                    if (baseControlCard.CheckIfAddedToPlaylist())
                    {
                        baseControlCard.AddIcon.Visibility = Visibility.Collapsed;
                        baseControlCard.AddIconBG.Visibility = Visibility.Collapsed;
                        baseControlCard.RemoveIcon.Visibility = Visibility.Visible;
                        baseControlCard.RemoveIconBG.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        baseControlCard.AddIcon.Visibility = Visibility.Visible;
                        baseControlCard.AddIconBG.Visibility = Visibility.Visible;
                        baseControlCard.RemoveIcon.Visibility = Visibility.Collapsed;
                        baseControlCard.RemoveIconBG.Visibility = Visibility.Collapsed;
                        Global.mPlayer.SongPlayingArtWork = null;
                        Global.mPlayer.Stop();
                        Global.MainWindow.MainBackgroundBG.Visibility = Visibility.Visible;
                    }
                }
                

                if (FavPLaylists.Count > 0)
                {
                    foreach (var item in FavPLaylists)
                    {
                        if (item.ToAdd)
                        {
                            using(JukeboxBrainsDBEntities db=new JukeboxBrainsDBEntities())
                            {
                                var track = db.TrackLibraries.FirstOrDefault(x => x.Id == item.TrackLibrary.Id);
                                if (track != null)
                                {
                                    db.Database.ExecuteSqlCommand($"EXEC [dbo].[sp_MakeTrack_Favourite_Unfavourite] @TrackID = {item.TrackLibrary.Id}, @isFavourite = 1");
                                }
                            }
                        }
                        else
                        {
                            using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                            {
                                var track = db.TrackLibraries.FirstOrDefault(x => x.Id == item.TrackLibrary.Id);
                                if (track != null)
                                {
                                    db.Database.ExecuteSqlCommand($"EXEC [dbo].[sp_MakeTrack_Favourite_Unfavourite] @TrackID = {item.TrackLibrary.Id}, @isFavourite = 0");
                                }
                            }
                        }

                    }

                    CheckIfAnySongFavourite();

                }

                using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                {
                    var list = db.Playlists.ToList();
                    if (list.Any())
                    {
                        if (Global.mPlayer.SongPlayingArtWork == null)
                        {
                            PlayPlaylist(db.Playlists.Select(x => x.TrackLibrary).ToList());
                        }
                        
                    }
                    else
                    {
                        Global.mPlayer.Stop();
                        Global.mPlayer.SongPlayingArtWork = null;
                        Global.MainWindow.MainBackgroundBG.Visibility = Visibility.Visible;
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }



        private void CheckIfAnySongFavourite()
        {
            try
            {
                int favCount = 0;
                using (JukeboxBrainsDBEntities db=new JukeboxBrainsDBEntities())
                {
                    var listA = db.LibraryViews.Where(w => w.AlbumId == baseAlbum.AlbumId && w.Type == Global.AppModeString).ToList();
                    var listS = listA.Select(s => s.SongId);

                    var list = db.SongLibraries.Where(w2 => listS.Contains(w2.SongId)).ToList();
                    foreach (var item in list)
                    {
                        if (favCount == 0)
                        {
                            if (item.TrackLibraries != null)
                            {
                                var tl = item.TrackLibraries.FirstOrDefault();
                                if (tl != null)
                                {
                                    var result = db.Database.SqlQuery<bool>($"EXEC [dbo].[sp_checkIfTrackFaourite] @TrackId = {tl.Id}").ToList();
                                    if (result[0] == true)
                                    {
                                        favCount++;
                                    }
                                }

                            }

                        }

                    }
                }

                if (baseControlCard != null)
                {
                    if (favCount > 0)
                    {
                        baseControlCard.AddAlbumToFavourite(null, null);
                    }
                    else
                    {
                        baseControlCard.RemoveYes(null, null);
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
    }

    public class PlaylistWithActionTaken
    {
        public TrackLibrary TrackLibrary { get; set; }
        public bool ToAdd { get; set; }
        public bool IsNewAdded { get; set; }
    }
}
