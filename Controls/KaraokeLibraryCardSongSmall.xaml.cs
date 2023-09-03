using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using iTunesSearch.Library;
using JukeBoxSolutions.Pages;
using static JukeBoxSolutions.Library.PlayListSource2;

namespace JukeBoxSolutions.Controls
{
    /// <summary>
    /// Interaction logic for KaraokeLibraryCardSongSmall.xaml
    /// </summary>
    public partial class KaraokeLibraryCardSongSmall : UserControl
    {
        //SongLibrary song { get; set; }
        //public bool? IsButtonActive
        //{
        //    get { return baseButton.IsChecked; }
        //    set { baseButton.IsChecked = value; }
        //}

        public TrackLibrary baseTrack { get; set; }
        int baseTrackID { get; set; }
        int PlaylistId { get; set; }
        private int? PlaylistSequenceNumber { get; set; }
        private Library ParentAlbum { get; set; }

        //public KaraokeLibraryCardSongSmall(TrackLibrary trackLibrary, string trackNumber = "", LibraryPopup parentAlbum = null)
        public CardControlMode GetSetupMode()
        {
            return new CardControlMode() { CardMode = CardMode };
        }
        public class CardControlMode
        {
            public bool ShowAddToPlaylistx { get; set; }
            public bool ShowSettingsAdmin { get; set; }
            public CardModeEnum CardMode { get; set; }
        }
        public KaraokeLibraryCardSongSmall(TrackLibrary trackLibrary, Library parent)
        {
            InitializeComponent();
            this.baseTrackID = trackLibrary.Id;
            this.ParentAlbum = parent;
            if (trackLibrary.SongLibraries.Any())
            {
                var f = trackLibrary.SongLibraries.ToList().First();
                lblTrackName.Text = f.SongName;
                if (f.Artists != null && f.Artists.Any())
                {
                    lblArtistName.Visibility = Visibility.Visible;
                    lblArtistName.Content = f.Artists.First().ArtistName;
                }
                else
                {
                    lblArtistName.Visibility = Visibility.Visible;
                }
            }

            baseTrack = trackLibrary;
            PlaylistId = Global.NowPlayingId;
            if (Global.AppControlMode == Global.AppControlModeEnum.Playlist && Global.LibraryForceViewMode == false)
            {
                PlaylistId = Global.AppPlaylistModeBufferID;
            }

            showAddToPlaylistx = !CheckIfAddedInPlaylist(trackLibrary);
        }

        public bool CheckIfAddedInPlaylist(TrackLibrary trackLibrary)
        {
            try
            {
                using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                {
                    return db.Playlists.Count(x => x.TrackId == trackLibrary.Id) > 0;
                }
            }
            catch (Exception)
            {

            }

            return false;
        }

        public void SetupTrack(AlbumTrack track, CardControlMode setupMode)
        {
            CardMode = setupMode.CardMode;

            lblTrackName.Text = track.SongName;
            if (track.hasArtist)
                lblArtistName.Content = track.ArtistName;
            baseTrackID = track.TrackID;

            showAddToPlaylistx = setupMode.ShowAddToPlaylistx;
            showSettingsAdmin = Global.AppControlMode == Global.AppControlModeEnum.Admin;

            setMode();
        }
        public KaraokeLibraryCardSongSmall(AlbumTrack track, string trackNumber = "", LibraryPopup parentAlbum = null, bool isSelected = false, bool isInPlaylist = false)
        {
            InitializeComponent();

            lblTrackName.Text = track.SongName;
            if (track.hasArtist)
                lblArtistName.Content = track.ArtistName;
            baseTrackID = track.TrackID;

            SetAppActionMode(isSelected, Global.LibraryForceViewMode);

            bool isNowPlaying = isSelected;

            PlaylistId = Global.NowPlayingId;

            // Organize Buttons... can this be grouped?
            // Playlist Mode & ForceViewmode


            if (Global.AppControlMode == Global.AppControlModeEnum.Playlist && Global.LibraryForceViewMode == false)
            {
                PlaylistId = Global.AppPlaylistModeBufferID;

                if (isInPlaylist)
                {
                    showAddToPlaylistx = false;
                }
                else
                    showAddToPlaylistx = true;
            }
            else
            {
                showAddToPlaylistx = false;
            }

            if (Global.AppControlMode == Global.AppControlModeEnum.Admin)
            {
                showSettingsAdmin = true;
            }
            else
            {
                showSettingsAdmin = false;
            }
        }

        private bool _isAddToPlaylistx;
        public bool showAddToPlaylistx
        {
            get { return _isAddToPlaylistx; }
            set
            {
                if (value)
                {
                    AddToPlaylistxNormal.Visibility = Visibility.Visible;
                    RemoveFromPlaylistxNormal.Visibility = Visibility.Collapsed;
                    btnAddToPlaylistx.Visibility = Visibility.Visible;
                    btnRemove.Visibility = Visibility.Collapsed;
                }
                else
                {
                    AddToPlaylistxNormal.Visibility = Visibility.Collapsed;
                    RemoveFromPlaylistxNormal.Visibility = Visibility.Visible;
                    btnAddToPlaylistx.Visibility = Visibility.Collapsed;
                    btnAddToPlaylistx.Tag = "hide";
                    btnRemove.Visibility = Visibility.Visible;
                }
                _isAddToPlaylistx = value;
            }
        }

        private bool _isSettingsAdmin;
        public bool showSettingsAdmin
        {
            get { return _isSettingsAdmin; }
            set
            {
                if (value)
                {
                    btnEdit.Visibility = Visibility.Visible;
                    btnRemove.Visibility = Visibility.Visible;
                }
                else
                {
                    btnEdit.Visibility = Visibility.Collapsed;
                    btnEdit.Tag = "hide";
                    btnRemove.Visibility = Visibility.Collapsed;
                    btnRemove.Tag = "hide";
                }
                _isSettingsAdmin = value;
            }
        }


        public KaraokeLibraryCardSongSmall(TrackLibrary trackLibrary, string trackNumber = "", LibraryPopup parentAlbum = null)
        {
            InitializeComponent();
            //lblTrackNo.Content = trackNumber;
            //BtnMusicCount.Content = trackNumber;


            baseTrack = trackLibrary;
            lblTrackName.Text = trackLibrary.SongLibraries.First().SongName;
            if (trackLibrary.SongLibraries.First().Artists.Any())
                lblArtistName.Content = trackLibrary.SongLibraries.First().Artists.First().ArtistName;

            bool isNowPlaying = trackLibrary.Playlists.Where(w => w.PlaylistId == Global.mPlayer.playList.PlayListId).Any();
            SetAppActionMode(isNowPlaying, Global.LibraryForceViewMode);

            PlaylistId = Global.NowPlayingId;
            if (Global.AppControlMode == Global.AppControlModeEnum.Playlist && Global.LibraryForceViewMode == false)
            {
                PlaylistId = Global.AppPlaylistModeBufferID;

                if (trackLibrary.Playlists.Where(w => w.PlaylistId == PlaylistId).Any())
                {
                    btnAddToPlaylistx.Visibility = Visibility.Collapsed;
                    btnAddToPlaylistx.Tag = "hide";
                }
                else
                {
                    btnAddToPlaylistx.Visibility = Visibility.Visible;
                }

            }
            else
            {
                btnAddToPlaylistx.Visibility = Visibility.Collapsed;
                btnAddToPlaylistx.Tag = "hide";
            }

            if (Global.AppControlMode == Global.AppControlModeEnum.Admin)
            {
                btnEdit.Visibility = Visibility.Visible;
                btnRemove.Visibility = Visibility.Visible;
            }
            else
            {
                btnEdit.Visibility = Visibility.Collapsed;
                btnEdit.Tag = "hide";
            }

            btnRemove.Visibility = Visibility.Collapsed;
            btnRemove.Tag = "hide";


            //Manual Application Mode
            //btnPlayTrack.Visibility = Visibility.Visible;
            btnRemove.Visibility = Visibility.Visible;
        }



        /// <summary>
        /// Used for Playlist Mode
        /// </summary>
        /// <param name="trackLibrary"></param>
        /// <param name="isPlaying"></param>
        /// <param name="trackNumber"></param>
        public KaraokeLibraryCardSongSmall(TrackLibrary trackLibrary, bool isPlaying, int playlistSequence)
        {
            bool showPlaylistIndexNumber = true;
            InitializeComponent();
            baseTrack = trackLibrary;
            if (showPlaylistIndexNumber)
                lblTrackName.Text = playlistSequence.ToString() + " : " + trackLibrary.SongLibraries.First().SongName;
            else
                lblTrackName.Text = trackLibrary.SongLibraries.First().SongName;
            if (trackLibrary.SongLibraries.First().Artists.Any())
                lblArtistName.Content = trackLibrary.SongLibraries.First().Artists.First().ArtistName;
            else
                lblArtistName.Content = "";
            //BtnMusicCount.Content = (playlistSequence + 1).ToString();
            PlaylistSequenceNumber = playlistSequence;
            //IsButtonActive = isPlaying;

            // Manual Application Mode
            //btnPlayTrack.Visibility = Visibility.Visible;
            btnRemove.Visibility = Visibility.Visible;

            //btnAddToPlaylist.Visibility = Visibility.Collapsed;
            //btnAddToPlaylist.Tag = "hide";
            //btnEdit.Visibility = Visibility.Collapsed;
            //btnEdit.Tag = "hide";
            //btnPlayNow.Visibility = Visibility.Collapsed;
            //btnPlayNow.Tag = "hide";
            btnAddToPlaylistx.Visibility = Visibility.Collapsed;
            btnAddToPlaylistx.Tag = "hide";
        }

        public void SetAppActionMode(bool isNowPlaying = false, bool forceLibraryViewMode = false)
        {
            //default
            CardMode = CardModeEnum.NormalPlay;

            if (Global.AppControlMode == Global.AppControlModeEnum.Playlist && forceLibraryViewMode == false)
            {
                //if (IsButtonActive.Value)
                //    CardMode = CardModeEnum.RemoveFromPlaylist;
                //else
                //    CardMode = CardModeEnum.AddToPlaylist;
            }
            else if (Global.AppActionMode == Global.AppActionModeEnum.Idle)
            {
                CardMode = CardModeEnum.NormalPlay;
            }
            else
            {
                //if (IsButtonActive.Value || isNowPlaying == true)
                //    CardMode = CardModeEnum.RemoveFromPlaylist;
                //else
                //    CardMode = CardModeEnum.AddToPlaylist;
            }

            setMode();
        }

        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            if (PlaylistSequenceNumber == null)
            {
                if (baseTrack == null)
                {
                    using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                    {
                        baseTrack = db.TrackLibraries.Find(baseTrackID);
                    }
                }

                Global.mPlayer.PlayNow(baseTrack);

                Global.AppActionMode = Global.AppActionModeEnum.Playing;

                CardMode = CardModeEnum.RemoveFromPlaylist;
            }
            else
            {
                // Index vs Sequence
                Global.mPlayer.PlayPlaylistIndex(PlaylistSequenceNumber.Value);
            }
        }

        private void btnAddToPlaylist_Click(object sender, RoutedEventArgs e)
        {

            int i = Global.AppPlaylistModeBufferID;
            int i2 = Global.BufferPlaylistID;

            if (baseTrack == null)
                using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                {
                    baseTrack = db.TrackLibraries.Find(baseTrackID);
                }

            if (Global.AppPlaylistModeBufferID > 0 && Global.LibraryForceViewMode == false)
            {
                int i3 = PlaylistId;
                Global.mPlayer.dbAddTrackToPlaylist(baseTrack, Global.AppPlaylistModeBufferID);
                Global.mPlayer.dbAddTrackToPlaylist(baseTrack, Global.BufferPlaylistID);
                
            }
            else
            {
                Global.mPlayer.playList.Add(baseTrack);
                
            }

            //var x = Global.mPlayer.playList.PlayListId;
            Global.mPlayer.dbAddTrackToPlaylist(baseTrack, PlaylistId);
            Global.mPlayer.dbAddTrackToPlaylist(baseTrack, Global.BufferPlaylistID);
            CardMode = CardModeEnum.RemoveFromPlaylist;
            setMode();
        }
        public void SetMode(CardModeEnum cardMode)
        {
            CardMode = cardMode;
            setMode();
        }
        private void setMode()
        {
            switch (CardMode)
            {
                case CardModeEnum.NormalPlay:
                    // Set Play Mode
                    //btnPlayTrack.Visibility = Visibility.Visible;
                    //btnAddToPlaylist.Visibility = Visibility.Collapsed;
                    //btnRemoveFromPlaylist.Visibility = Visibility.Collapsed;
                    break;
                case CardModeEnum.AddToPlaylist:
                    // Set Add / Append Mode
                    //btnPlayTrack.Visibility = Visibility.Collapsed;
                    //btnAddToPlaylist.Visibility = Visibility.Visible;
                    //btnRemoveFromPlaylist.Visibility = Visibility.Collapsed;
                    break;
                case CardModeEnum.RemoveFromPlaylist:
                    //btnPlayTrack.Visibility = Visibility.Collapsed;
                    //btnAddToPlaylist.Visibility = Visibility.Collapsed;
                    //btnRemoveFromPlaylist.Visibility = Visibility.Visible;
                    break;
            }

            btnAddToPlaylistx.Visibility = Visibility.Collapsed;
            btnAddToPlaylistx.Tag = "hide";
        }

        public CardModeEnum CardMode = CardModeEnum.NormalPlay;
        public enum CardModeEnum
        {
            NormalPlay,
            AddToPlaylist,
            RemoveFromPlaylist
        }

        private void btnUpdateSong_Click(object sender, RoutedEventArgs e)
        {
            iTunesSearchManager ism = new iTunesSearchManager();

            ism.UpdateTrack2(baseTrack.SongLibraries.First());
            //ism.UpdateAlbumTrackAsync(song);
        }

        private void btnAddToPlaylistx_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (JukeboxBrainsDBEntities db=new JukeboxBrainsDBEntities())
                {
                    if (db.Playlists != null)
                    {
                        var playlist=db.Playlists.FirstOrDefault(x=>x.TrackId==baseTrack.Id);
                        if (playlist == null)
                        {
                            Global.mPlayer.dbAddTrackToPlaylist(baseTrack, PlaylistId);
                            showAddToPlaylistx = false;
                            NormalModeActiveBtnContainer.Visibility = Visibility.Collapsed;
                            AdminActiveBtnContainer.Visibility = Visibility.Collapsed;
                            AddedToPlaylistInfoButton.Visibility = Visibility.Visible;
                            var a = new DoubleAnimation
                            {
                                From = 1.0,
                                To = 0.0,
                                FillBehavior = FillBehavior.Stop,
                                BeginTime = TimeSpan.FromSeconds(2),
                                Duration = new Duration(TimeSpan.FromSeconds(0.5))
                            };
                            var storyboard = new Storyboard();

                            storyboard.Children.Add(a);
                            Storyboard.SetTarget(a, AddedToPlaylistInfoButton);
                            Storyboard.SetTargetProperty(a, new PropertyPath(OpacityProperty));
                            storyboard.Completed += delegate { AddedToPlaylistInfoButton.Visibility = System.Windows.Visibility.Collapsed; };
                            storyboard.Begin();
                        }
                    }
                }                
            }
            catch (Exception)
            {

            }
            
        }

        private void btnExpand_ClickX(object sender, RoutedEventArgs e)
        {
            //btnExpand.Visibility = Visibility.Collapsed;
        }

        public void BuildBButtons()
        {

        }

        private void btnExpand_Click(object sender, MouseButtonEventArgs e)
        {
            //btnExpand.Visibility = Visibility.Collapsed;
            //btnAddToPlaylistx.Visibility = Visibility.Visible;
            //stackInfo.Visibility = Visibility.Visible;

            //if (Global.ButtonBuffer_StackPannel != null)
            //    Global.ButtonBuffer_StackPannel.Visibility = Visibility.Collapsed;
            //if (Global.ButtonBuffer_CoverButton != null)
            //    Global.ButtonBuffer_CoverButton.Visibility = Visibility.Visible;

            //Global.ButtonBuffer_StackPannel = stackInfo;
            //Global.ButtonBuffer_CoverButton = btnExpand;
            ShowButtons();
        }

        private void btnRemoveFromPlaylist_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                {
                    if (db.Playlists != null)
                    {
                        var playlist = db.Playlists.FirstOrDefault(x => x.TrackId == baseTrack.Id);
                        if (playlist != null)
                        {
                            Global.mPlayer.dbRemoveTrackFromPlaylist(baseTrack, PlaylistId);
                            showAddToPlaylistx = true;
                            NormalModeActiveBtnContainer.Visibility = Visibility.Collapsed;
                            AdminActiveBtnContainer.Visibility = Visibility.Collapsed;
                            RemovedFromPlaylistInfoButton.Visibility = Visibility.Visible;
                            var a = new DoubleAnimation
                            {
                                From = 1.0,
                                To = 0.0,
                                FillBehavior = FillBehavior.Stop,
                                BeginTime = TimeSpan.FromSeconds(2),
                                Duration = new Duration(TimeSpan.FromSeconds(0.5))
                            };
                            var storyboard = new Storyboard();

                            storyboard.Children.Add(a);
                            Storyboard.SetTarget(a, RemovedFromPlaylistInfoButton);
                            Storyboard.SetTargetProperty(a, new PropertyPath(OpacityProperty));
                            storyboard.Completed += delegate { RemovedFromPlaylistInfoButton.Visibility = System.Windows.Visibility.Collapsed; };
                            storyboard.Begin();
                        }
                    }
                }
            }
            catch (Exception)
            {

            }
        }


        public void ShowButtons()
        {
            btnAddToPlaylistx.Visibility = Visibility.Visible;

            //btnExpand.Visibility = Visibility.Collapsed;
            //stackInfo.Visibility = Visibility.Visible;
            //foreach (var b in stackInfo.Children.OfType<Button>())
            //{
            //    b.Visibility = Visibility.Collapsed;
            //}

            if (Global.ButtonBuffer_StackPannel != null)
                Global.ButtonBuffer_StackPannel.Visibility = Visibility.Collapsed;
            if (Global.ButtonBuffer_CoverButton != null)
                Global.ButtonBuffer_CoverButton.Visibility = Visibility.Visible;

            //Global.ButtonBuffer_StackPannel = stackInfo;
            //Global.ButtonBuffer_CoverButton = btnExpand;

            showbutton();
        }

        public void unpackButtons(Visibility v, int buttoncount)
        {

            for (int i = buttoncount - 1; i >= 0; i--)
            {
                Thread.Sleep(30);
                //string s = ((Button)stackInfo.Children[i]).Tag == null ? "" : ((Button)stackInfo.Children[i]).Tag.ToString();

                //this.Dispatcher.Invoke(() =>
                //{
                //    string s = ((Button)stackInfo.Children[i]).Tag == null ? "" : ((Button)stackInfo.Children[i]).Tag.ToString();
                //    if (s == "hide")
                //    {
                //        ((Button)stackInfo.Children[i]).Visibility = Visibility.Collapsed;
                //    }
                //    else
                //    {
                //        ((Button)stackInfo.Children[i]).Visibility = v;
                //    }
                //});
            }

        }

        private void showbutton()
        {
            //int i = stackInfo.Children.Count;

            //new Task(() => { unpackButtons(Visibility.Visible, i); }).Start();

            //Global.setExpandedSongButtonBuffer(this);
        }

        public void hideButtons()
        {
            //int i = stackInfo.Children.Count;
            //new Task(() => { unpackButtons(Visibility.Collapsed, i); }).Start();
        }

        #region INotifyPropertyChanged Members


        public static void Delay(int milliseconds, Action action)
        {
            var t = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(milliseconds) };
            t.Tick += (o, e) => { t.Stop(); action.Invoke(); };
            t.Start();
        }


        #endregion

        private void btnPlayNow_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (baseTrack == null)
                    using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                    {
                        baseTrack = db.TrackLibraries.Find(baseTrackID);
                    }

                AddToPlaylistIfNotExist(baseTrack);
                PlayPlaylist(null, null);
            }
            catch (Exception)
            {

            }
        }
        private void PlayPlaylist(object sender, MouseButtonEventArgs e)
        {
            Play(new List<TrackLibrary> { baseTrack });
        }
        private void Play(List<TrackLibrary> trackLibraries)
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
                        var songPlayingArtworkcontrol = new SongPlayingArtwork(new List<TrackLibrary> { baseTrack });
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

        private void AddToPlaylistIfNotExist(TrackLibrary baseTrack)
        {
            try
            {
                using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
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
                    if (Cards.Count(x => x.baseTrack.Id == baseTrack.Id) > 0)
                    {
                        var cardIndex = Cards.IndexOf(Cards.First(x => x.baseTrack.Id == baseTrack.Id));
                        Cards.Remove(Cards[cardIndex]);
                    }

                    Global.mPlayer.dbAddTrackToPlaylist(baseTrack, Global.NowPlayingId);

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

        private void btnQueNow_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                NormalModeActiveBtnContainer.Visibility = Visibility.Collapsed;
                AdminActiveBtnContainer.Visibility = Visibility.Collapsed;
                AddedToPlaylistInfoButton.Visibility = Visibility.Visible;
                var a = new DoubleAnimation
                {
                    From = 1.0,
                    To = 0.0,
                    FillBehavior = FillBehavior.Stop,
                    BeginTime = TimeSpan.FromSeconds(2),
                    Duration = new Duration(TimeSpan.FromSeconds(0.5))
                };
                var storyboard = new Storyboard();

                storyboard.Children.Add(a);
                Storyboard.SetTarget(a, AddedToPlaylistInfoButton);
                Storyboard.SetTargetProperty(a, new PropertyPath(OpacityProperty));
                storyboard.Completed += delegate { AddedToPlaylistInfoButton.Visibility = System.Windows.Visibility.Collapsed; };
                storyboard.Begin();
                using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                {
                    Global.mPlayer.dbAddTrackToPlaylist(baseTrack, PlaylistId);
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
                    if (Cards.Count > 0)
                    {
                        var playlistItem = Cards.FirstOrDefault(x => x.baseTrack.Id == baseTrack.Id);
                        var indexOfCurrentItem = Cards.IndexOf(playlistItem);
                        if (indexOfCurrentItem > 1)
                        {
                            Cards.Remove(Cards[indexOfCurrentItem]);
                            if (Global.mPlayer.SongPlayingArtWork != null)
                            {
                                var currentTrack = Global.mPlayer.SongPlayingArtWork.CurrentTrack;
                                if (currentTrack != null)
                                {
                                    var card = Cards.First(x => x.baseTrack.Id == currentTrack.Id);
                                    var cardIndex = Cards.IndexOf(card);
                                    if (cardIndex != -1)
                                    {
                                        Cards.Insert(cardIndex + 1, playlistItem);
                                    }
                                }
                            }
                            else
                            {
                                Cards.Insert(1, playlistItem);
                            }
                        }
                    }

                    db.Database.ExecuteSqlCommand("delete from Playlists where [PlaylistId] <> 1");
                    db.Database.ExecuteSqlCommand("DBCC CHECKIDENT ([Playlists]);");
                    db.Database.ExecuteSqlCommand("DBCC CHECKIDENT ([PlayListDetails]);");
                    db.SaveChangesAsync();
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

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            // Editor?
            // Smart Updater?
            // Popup Smart Updater
            Global.mPlayer.popUp_Grid.Visibility = System.Windows.Visibility.Visible;

            if (baseTrack == null)
                using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                {
                    baseTrack = db.TrackLibraries.Find(baseTrackID);
                }

            Global.mPlayer.popUp_Frame.Content = new Popup_SmartUpdater(baseTrack);
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            // Confirm?
        }

        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Global.mPlayer.RemoveFromPlaylist(baseTrack);
                showAddToPlaylistx = true;
                btnRemove.Visibility = Visibility.Collapsed;
                if (Global.mPlayer?.popUp_Frame?.Content != null)
                {
                    // Reload Playlist
                    string s = Global.mPlayer.popUp_Frame.Content.GetType().Name;

                    if (Global.mPlayer.popUp_Frame.Content.GetType().Name == "PlayListView")
                    {
                        PlayListView p = (PlayListView)Global.mPlayer.popUp_Frame.Content;
                        p.LoadPlaylist();
                    }
                }                
            }
            catch (Exception)
            {

            }
            
        }

        private void BtnTrackMenu_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Global.AppControlMode == Global.AppControlModeEnum.Admin)
                {
                    NormalModeActiveBtnContainer.Visibility = Visibility.Collapsed;
                    NormalModeActiveBtnPanel.Visibility = Visibility.Collapsed;
                    AdminActiveBtnContainer.Visibility = Visibility.Visible;
                    AdminActiveBtnPanels.Visibility = Visibility.Visible;
                    var childs = this.ParentAlbum.SongsGridLibraryVertical.Children;
                    if (childs != null && childs.Count > 0)
                    {
                        var panel = childs[1] as StackPanel;
                        if (panel != null && panel.Children?.Count > 0)
                        {
                            foreach (var item in panel.Children.OfType<UniformGrid>())
                            {
                                foreach (var item1 in item.Children)
                                {
                                    var cardSong = item1 as KaraokeLibraryCardSongSmall;
                                    if (cardSong != this)
                                    {
                                        if (cardSong.AdminActiveBtnContainer.Visibility == Visibility.Visible)
                                        {
                                            cardSong.AdminActiveBtnContainer.Visibility = Visibility.Collapsed;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    AdminActiveBtnContainer.Visibility = Visibility.Collapsed;
                    AdminActiveBtnPanels.Visibility = Visibility.Collapsed;
                    NormalModeActiveBtnContainer.Visibility = Visibility.Visible;
                    NormalModeActiveBtnPanel.Visibility = Visibility.Visible;
                    var childs = this.ParentAlbum.SongsGridLibraryVertical.Children;
                    if(childs!=null && childs.Count > 0)
                    {
                        var panel = childs[1] as StackPanel;
                        if (panel != null && panel.Children?.Count > 0)
                        {
                            foreach (var item in panel.Children.OfType<UniformGrid>())
                            {
                                foreach (var item1 in item.Children)
                                {
                                    var cardSong = item1 as KaraokeLibraryCardSongSmall;
                                    if (cardSong != this)
                                    {
                                        if (cardSong.NormalModeActiveBtnContainer.Visibility == Visibility.Visible)
                                        {
                                            cardSong.NormalModeActiveBtnContainer.Visibility = Visibility.Collapsed;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    
                }
            }
            catch (Exception)
            {

            }
            

        }
        private void BtnTrackMenuActive_Click(object sender, RoutedEventArgs e)
        {
            NormalModeActiveBtnContainer.Visibility = Visibility.Collapsed;
            NormalModeActiveBtnPanel.Visibility = Visibility.Collapsed;
        }

        private void AdminModeBtnTrackMenuActive_Click(object sender, RoutedEventArgs e)
        {
            AdminActiveBtnContainer.Visibility = Visibility.Collapsed;
            AdminActiveBtnPanels.Visibility = Visibility.Collapsed;
        }

        private void AddToPlaylistX(object sender, MouseButtonEventArgs e)
        {
            try
            {
                btnAddToPlaylistx_Click(null, null);

                using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                {
                    if (Global.mPlayer.SongPlayingArtWork == null)
                    {
                        Play(db.Playlists.Select(x => x.TrackLibrary).ToList());
                    }
                }
            }
            catch (Exception)
            {

            }
            
        }

        private void RemoveFromPlaylistX(object sender, MouseButtonEventArgs e)
        {
            try
            {
                btnRemoveFromPlaylist_Click(null, null);
                using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                {
                    var list = db.Playlists.ToList();
                    if (list.Any())
                    {
                        Play(db.Playlists.Select(x => x.TrackLibrary).ToList());
                    }
                    else
                    {
                        Global.mPlayer.Stop();
                        Global.mPlayer.SongPlayingArtWork = null;
                        Global.MainWindow.MainBackgroundBG.Visibility = Visibility.Visible;
                    }
                }
            }
            catch (Exception)
            {

            }
            
        }

        private void QueueNext(object sender, MouseButtonEventArgs e)
        {
            NormalModeActiveBtnContainer.Visibility = Visibility.Collapsed;
            AdminActiveBtnContainer.Visibility = Visibility.Collapsed;
            btnQueNow_Click(null, null);            
        }
    }
}
