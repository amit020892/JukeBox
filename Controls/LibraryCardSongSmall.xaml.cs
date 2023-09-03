using System;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using iTunesSearch.Library;
using JukeBoxSolutions.Pages;
using static JukeBoxSolutions.Library.PlayListSource2;

namespace JukeBoxSolutions.Controls
{
    /// <summary>
    /// Interaction logic for LibraryCardSongSmall.xaml
    /// </summary>
    public partial class LibraryCardSongSmall : UserControl
    {
        //SongLibrary song { get; set; }
        //public bool? IsButtonActive
        //{
        //    get { return baseButton.IsChecked; }
        //    set { baseButton.IsChecked = value; }
        //}

        TrackLibrary baseTrack { get; set; }
        int baseTrackID { get; set; }
        int PlaylistId { get; set; }
        private int? PlaylistSequenceNumber { get; set; }
        private LibraryPopup ParentAlbum { get; set; }

        //public LibraryCardSongSmall(TrackLibrary trackLibrary, string trackNumber = "", LibraryPopup parentAlbum = null)
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
        public LibraryCardSongSmall(TrackLibrary trackLibrary)
        {
            InitializeComponent();
            this.baseTrack = trackLibrary;
            this.baseTrackID = trackLibrary.Id;
            if (trackLibrary.SongLibraries.Any())
            {
                var f = trackLibrary.SongLibraries.ToList().First();
                lblTrackName.Text = f.SongName;
                if (f.Artists!=null && f.Artists.Any())
                {
                    lblArtistName.Visibility = Visibility.Visible;
                    lblArtistName.Content= f.Artists.First().ArtistName;
                }
                else
                {
                    lblArtistName.Visibility = Visibility.Visible;
                }
            }
           
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
        public LibraryCardSongSmall(AlbumTrack track, string trackNumber = "", LibraryPopup parentAlbum = null, bool isSelected = false, bool isInPlaylist = false)
        {
            InitializeComponent();

            lblTrackName.Text = track.SongName;
            if (track.hasArtist)
                lblArtistName.Content = track.ArtistName;
            baseTrackID = track.TrackID;

            SetAppActionMode(isSelected, Global.LibraryForceViewMode);

            bool isNowPlaying = isSelected;

            if (isNowPlaying && ParentAlbum != null)
                ParentAlbum.SongActiveCheckIn();

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
                    btnAddToPlaylistx.Visibility = Visibility.Visible;
                }
                else
                {
                    btnAddToPlaylistx.Visibility = Visibility.Collapsed;
                    btnAddToPlaylistx.Tag = "hide";
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


        public LibraryCardSongSmall(TrackLibrary trackLibrary, string trackNumber = "", LibraryPopup parentAlbum = null)
        {
            InitializeComponent();
            ParentAlbum = parentAlbum;
            //lblTrackNo.Content = trackNumber;
            //BtnMusicCount.Content = trackNumber;


            baseTrack = trackLibrary;
            lblTrackName.Text = trackLibrary.SongLibraries.First().SongName;
            if (trackLibrary.SongLibraries.First().Artists.Any())
                lblArtistName.Content = trackLibrary.SongLibraries.First().Artists.First().ArtistName;

            bool isNowPlaying = trackLibrary.Playlists.Where(w => w.PlaylistId == Global.mPlayer.playList.PlayListId).Any();
            SetAppActionMode(isNowPlaying, Global.LibraryForceViewMode);

            if (isNowPlaying && ParentAlbum != null)
                ParentAlbum.SongActiveCheckIn();

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
        public LibraryCardSongSmall(TrackLibrary trackLibrary, bool isPlaying, int playlistSequence)
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
                //IsButtonActive = true;

                if (ParentAlbum != null)
                {
                    ParentAlbum.UpdateAppActionMode();
                    Global.LibraryUpdateActionMode(ParentAlbum.baseAlbum.AlbumId);
                }
                else
                {
                    Global.LibraryUpdateActionMode();
                }


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
                if (ParentAlbum != null)
                    ParentAlbum.SetBaseAlbumSelected = true;
            }
            else
            {
                Global.mPlayer.playList.Add(baseTrack);
                if (ParentAlbum != null)
                    ParentAlbum.SetBaseAlbumSelected = true;
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
            // Glow to indicate playlist thingy
            Global.mPlayer.dbAddTrackToPlaylist(baseTrack, PlaylistId);
            Global.mPlayer.dbAddTrackToPlaylist(baseTrack, Global.BufferPlaylistID);
            btnAddToPlaylistx.Visibility = Visibility.Collapsed;
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
            if (baseTrack == null)
                using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                {
                    baseTrack = db.TrackLibraries.Find(baseTrackID);
                }

            if (Global.mPlayer.playList.Remove(baseTrack))
            {
                //IsButtonActive = false;
                Global.LibraryUpdateActionMode();
            }
            else
            {
                CardMode = CardModeEnum.AddToPlaylist;
                setMode();
                if (ParentAlbum != null)
                    ParentAlbum.SetBaseAlbumSelected = false;
            }
        }


        public void ShowButtons()
        {
            //btnAddToPlaylistx.Visibility = Visibility.Visible;

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
                bool refreshLib = false;
                if (baseTrack == null)
                    using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                    {
                        baseTrack = db.TrackLibraries.Find(baseTrackID);
                    }
                Global.mPlayer.PlayNow(baseTrack);
                Global.AppActionMode = Global.AppActionModeEnum.Playing;
                Global.MainWindow.btnPreviousMenu.Visibility = Visibility.Collapsed;
                Global.MainWindow.btnNextMenu.Visibility = Visibility.Collapsed;
                Global.MainWindow.sParentPanel.Visibility = Visibility.Collapsed;
                Global.MainWindow.btnMediaPanel.Visibility = Visibility.Collapsed;
                Global.MainWindow.PlaylistControlPanel.Visibility = Visibility.Visible;
                Global.MainWindow.MainBackgroundBG.Visibility = Visibility.Collapsed;
                Global.MainWindow.KaraokeLibraryFrame.Visibility = Visibility.Collapsed;
                Global.MainWindow.MusicLibraryFrame.Visibility = Visibility.Collapsed;
                Global.MainWindow.VideoLibraryFrame.Visibility = Visibility.Collapsed;
                Global.MainWindow.RadioLibraryFrame.Visibility = Visibility.Collapsed;
                Global.mPlayer.popUp_Grid.Visibility = Visibility.Collapsed;
                Global.mPlayer.popUp_Frame.Content = null;

                Global.MainWindow.MainFrame.Content = null;
                Global.MainWindow.btnPlay1.IsEnabled = true;
                Global.MainWindow.btnPlay1.Visibility = Visibility.Collapsed;
                Global.MainWindow.btnPause.IsEnabled = true;
                Global.MainWindow.btnPause.Visibility = Visibility.Visible;
                Global.MainWindow.PlaylistControlPanel.Height = 120;
                Global.MainWindow.btnInfo.IsEnabled = true;
                if (Global.AppActionMode == Global.AppActionModeEnum.Playing)
                    refreshLib = false;
                else
                {
                    refreshLib = true;
                    Global.AppActionMode = Global.AppActionModeEnum.Playing;
                }

                if (ParentAlbum != null)
                    ParentAlbum.UpdateAppActionMode();

                CardMode = CardModeEnum.RemoveFromPlaylist;
                //IsButtonActive = true;
                hideButtons();
                if (refreshLib)
                    Global.LibraryUpdateActionMode();
                else
                    setMode();
            }
            catch (Exception)
            {

            }
            
        }

        private void btnQueNow_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (baseTrack == null)
                    using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                    {
                        baseTrack = db.TrackLibraries.Find(baseTrackID);
                    }

                if (PlaylistSequenceNumber == null)
                {
                    Global.mPlayer.PlayNext(baseTrack);
                    CardMode = CardModeEnum.RemoveFromPlaylist;
                    //IsButtonActive = true;
                    setMode();
                    hideButtons();
                    Global.mPlayer.ShowNotification("Next Up: " + lblTrackName.Text.ToString());
                }
                else
                {
                    Global.mPlayer.RemoveFromPlaylist(baseTrack);
                    Global.mPlayer.PlayNext(baseTrack);

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

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            try
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
            catch (Exception)
            {

                throw;
            }
            
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
                // Reload Playlist
                string s = Global.mPlayer.popUp_Frame.Content.GetType().Name;

                //if (Global.mPlayer.popUp_Frame.Content.GetType().Name == "PlayListView")
                //{
                //    PlayListView p = (PlayListView)Global.mPlayer.popUp_Frame.Content;
                //    p.LoadPlaylist();
                //}
            }
            catch (Exception)
            {

                throw;
            }
            
        }

        private void BtnTrackMenu_Click(object sender, RoutedEventArgs e)
        {
            if (Global.AppControlMode == Global.AppControlModeEnum.Admin)
            {
                NormalModeActiveBtnContainer.Visibility = Visibility.Collapsed;
                NormalModeActiveBtnPanel.Visibility = Visibility.Collapsed;
                AdminActiveBtnContainer.Visibility = Visibility.Visible;
                AdminActiveBtnPanels.Visibility = Visibility.Visible;
            }
            else
            {
                AdminActiveBtnContainer.Visibility = Visibility.Collapsed;
                AdminActiveBtnPanels.Visibility = Visibility.Collapsed;
                NormalModeActiveBtnContainer.Visibility = Visibility.Visible;
                NormalModeActiveBtnPanel.Visibility = Visibility.Visible;
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
            btnAddToPlaylistx_Click(null, null);
        }

        private void RemoveFromPlaylistX(object sender, MouseButtonEventArgs e)
        {
            btnRemove_Click(null, null);
        }

        private void QeueueNext(object sender, MouseButtonEventArgs e)
        {
            btnQueNow_Click(null, null);
        }
    }
}
