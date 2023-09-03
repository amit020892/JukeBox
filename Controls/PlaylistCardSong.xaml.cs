using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using iTunesSearch.Library;
using iTunesSearch.Library.Models;
using JukeBoxSolutions.Pages;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;
using static JukeBoxSolutions.Library.PlayListSource2;

namespace JukeBoxSolutions.Controls
{
    /// <summary>
    /// Interaction logic for PlaylistCardSong.xaml
    /// </summary>
    public partial class PlaylistCardSong : System.Windows.Controls.UserControl
    {
        public TrackLibrary baseTrack { get; set; }
        int baseTrackID { get; set; }
        int PlaylistId { get; set; }
        private int? PlaylistSequenceNumber { get; set; }
        private LibraryPopup ParentAlbum { get; set; }
        private PlaylistPopup ParentPopup { get; set; }
        public class CardControlMode
        {
            public bool ShowAddToPlaylistx { get; set; }
            public bool ShowSettingsAdmin { get; set; }
            public CardModeEnum CardMode { get; set; }
        }
        public PlaylistCardSong(TrackLibrary trackLib)
        {
            InitializeComponent();
        }
        public void SetupTrack(AlbumTrack track, CardControlMode setupMode)
        {
            CardMode = setupMode.CardMode;

            lblTrackName.Content = track.SongName;
            if (track.hasArtist)
                lblArtistName.Content = track.ArtistName;
            baseTrackID = track.TrackID;

            //showAddToPlaylistx = setupMode.ShowAddToPlaylistx;
            //showSettingsAdmin = setupMode.ShowSettingsAdmin;

            setMode();
        }
        public PlaylistCardSong(TrackLibrary trackLib, AlbumTrack track, string trackNumber = "")
        {
            InitializeComponent();
            lblTrackName.Content = track.SongName;
            if (track.hasArtist)
                lblArtistName.Content = track.ArtistName;
            baseTrackID = track.TrackID;

            PlaylistId = Global.NowPlayingId;
            showAddToPlaylistx = false;
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
                    AddToPlaylistxNormal.Visibility = Visibility.Visible;
                    btnRemove.Visibility = Visibility.Collapsed;
                    RemoveFromPlaylistxNormal.Visibility = Visibility.Collapsed;
                    //AddedToPlaylistVector.Visibility = Visibility.Collapsed;
                    //AddedToPlaylistInfoButton.Visibility = Visibility.Collapsed;
                }
                else
                {
                    btnAddToPlaylistx.Visibility = Visibility.Collapsed;
                    AddToPlaylistxNormal.Visibility = Visibility.Collapsed;
                    btnAddToPlaylistx.Tag = "hide";
                    btnRemove.Visibility = Visibility.Visible;
                    RemoveFromPlaylistxNormal.Visibility = Visibility.Visible;
                    //AddedToPlaylistVector.Visibility = Visibility.Visible;
                    //AddedToPlaylistInfoButton.Visibility = Visibility.Visible;
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


        public PlaylistCardSong(TrackLibrary trackLibrary, string trackNumber = "", LibraryPopup parentAlbum = null,PlaylistPopup parent=null)
        {
            InitializeComponent();
            ParentAlbum = parentAlbum;
            ParentPopup=parent;
            //lblTrackNo.Content = trackNumber;
            BtnMusicCount.Content = trackNumber;

            baseTrack = trackLibrary;
            lblTrackName.Content = trackLibrary.SongLibraries.First().SongName;
            if (trackLibrary.SongLibraries.First().Artists.Any())
                lblArtistName.Content = trackLibrary.SongLibraries.First().Artists.First().ArtistName;

            showAddToPlaylistx = !CheckIfAddedInPlaylist(trackLibrary);

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
                    AddToPlaylistxNormal.Visibility = Visibility.Collapsed;
                    btnAddToPlaylistx.Tag = "hide";
                    btnRemove.Visibility= Visibility.Visible;
                    RemoveFromPlaylistxNormal.Visibility= Visibility.Visible;
                }
                else
                {
                    btnAddToPlaylistx.Visibility = Visibility.Visible;
                    AddToPlaylistxNormal.Visibility = Visibility.Visible;
                    btnRemove.Visibility = Visibility.Collapsed;
                    RemoveFromPlaylistxNormal.Visibility = Visibility.Collapsed;
                }
                   
            }
            else
            {
                btnAddToPlaylistx.Visibility = Visibility.Collapsed;
                AddToPlaylistxNormal.Visibility = Visibility.Collapsed;
                btnAddToPlaylistx.Tag = "hide";
            }

            if (Global.AppControlMode == Global.AppControlModeEnum.Admin)
            {
                btnEdit.Visibility = Visibility.Visible;
            }
            else
            {
                btnEdit.Visibility = Visibility.Collapsed;
                btnEdit.Tag = "hide";
            }

            //btnRemove.Visibility = Visibility.Collapsed;
            //btnRemove.Tag = "hide";


            // Manual Application Mode
            //btnPlayTrack.Visibility = Visibility.Visible;
            //btnRemove.Visibility = Visibility.Visible;

            CheckIfFavorited(trackLibrary);
        }

        private void CheckIfFavorited(TrackLibrary trackLibrary)
        {
            try
            {
                using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                {
                    var result = db.Database.SqlQuery<bool>($"EXEC [dbo].[sp_checkIfTrackFaourite] @TrackId = {trackLibrary.Id}").ToList();
                    if (result[0] == true)
                    {
                        FavDotButton.Visibility = Visibility.Visible;
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        private bool CheckIfAddedInPlaylist(TrackLibrary trackLibrary, bool shouldPlaylistCountChange=true)
        {
            try
            {
                using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                {
                    var result= db.Playlists.Count(x => x.TrackId == trackLibrary.Id && x.PlaylistId>0) > 0;
                    if (result)
                    {
                        AddedToPlaylistVector.Visibility = Visibility.Visible;
                        //if (shouldPlaylistCountChange)
                        //{
                        //    ParentAlbum.PlaylistCount++;
                        //}                        
                    }
                    return result;
                }
            }
            catch (Exception ex)
            {

            }

            return false;
        }

        /// <summary>
        /// Used for Playlist Mode
        /// </summary>
        /// <param name="trackLibrary"></param>
        /// <param name="isPlaying"></param>
        /// <param name="trackNumber"></param>
        public PlaylistCardSong(TrackLibrary trackLibrary, bool isPlaying, int playlistSequence)
        {
            bool showPlaylistIndexNumber = true;
            InitializeComponent();
            baseTrack = trackLibrary;
            if (showPlaylistIndexNumber)
                lblTrackName.Content = playlistSequence.ToString() + " : " + trackLibrary.SongLibraries.First().SongName;
            else
                lblTrackName.Content = trackLibrary.SongLibraries.First().SongName;
            if (trackLibrary.SongLibraries.First().Artists.Any())
                lblArtistName.Content = trackLibrary.SongLibraries.First().Artists.First().ArtistName;
            else
                lblArtistName.Content = "";
            BtnMusicCount.Content = (playlistSequence + 1).ToString();
            PlaylistSequenceNumber = playlistSequence;
            //IsButtonActive = isPlaying;

            // Manual Application Mode
            //btnPlayTrack.Visibility = Visibility.Visible;
            //btnRemove.Visibility = Visibility.Visible;

            //btnAddToPlaylist.Visibility = Visibility.Collapsed;
            //btnAddToPlaylist.Tag = "hide";
            //btnEdit.Visibility = Visibility.Collapsed;
            //btnEdit.Tag = "hide";
            //btnPlayNow.Visibility = Visibility.Collapsed;
            //btnPlayNow.Tag = "hide";
            //btnAddToPlaylistx.Visibility = Visibility.Collapsed;
            //btnAddToPlaylistx.Tag = "hide";
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

            //btnAddToPlaylistx.Visibility = Visibility.Collapsed;
            //btnAddToPlaylistx.Tag = "hide";
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
                // Glow to indicate playlist thingy
                //Global.mPlayer.dbAddTrackToPlaylist(baseTrack, PlaylistId);
                //Global.mPlayer.dbAddTrackToPlaylist(baseTrack, Global.BufferPlaylistID);
                showAddToPlaylistx = false;
                NormalModeActiveBtnContainer.Visibility = Visibility.Collapsed;
                AdminActiveBtnContainer.Visibility = Visibility.Collapsed;
                AdminActiveBtnPanels.Visibility = Visibility.Collapsed;
                AddedToPlaylistVector.Visibility = Visibility.Visible;
                AddedToPlaylistInfoButton.Visibility=Visibility.Visible;
                ParentAlbum.PlaylistCount++;
                ParentAlbum.PLaylists.Add(new PlaylistWithActionTaken
                {
                    TrackLibrary=baseTrack,
                    ToAdd=true,
                    IsNewAdded=true
                });
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
                PlayPlaylist(null, null);
            }
            catch (Exception)
            {

            }
        }

        private void PlayPlaylist(object sender, MouseButtonEventArgs e)
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
                        var songPlayingArtworkcontrol = new SongPlayingArtwork(new List<TrackLibrary> { baseTrack});
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
                        Global.MainWindow.LibraryButtonControlPanel.Visibility = Visibility.Collapsed;
                        Global.mPlayer.popUp_Grid.Visibility = Visibility.Collapsed;
                        //Global.MainWindow.InitializePlayer();
                        //Selected = true;
                    });
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
                NormalModeActiveBtnContainer.Visibility=Visibility.Collapsed;
                AdminActiveBtnContainer.Visibility=Visibility.Collapsed;
                var index = this.ParentPopup.Cards.IndexOf(this);
                this.ParentPopup.Cards.Remove(this.ParentPopup.Cards[index]);
                if (Global.mPlayer.SongPlayingArtWork != null)
                {
                    var currentTrack = Global.mPlayer.SongPlayingArtWork.CurrentTrack;
                    if (currentTrack != null)
                    {
                        var card = this.ParentPopup.Cards.First(x => x.baseTrack.Id == currentTrack.Id);
                        var cardIndex = this.ParentPopup.Cards.IndexOf(card);
                        if(cardIndex != -1)
                        {
                            this.ParentPopup.Cards.Insert(cardIndex+1, this);
                        }
                    }
                }
                else
                {
                    this.ParentPopup.Cards.Insert(1, this);
                }
                

                int t = 0;
                foreach (var item in this.ParentPopup.Cards)
                {
                    (item as PlaylistCardSong).BtnMusicCount.Content = $"{t + 1}";
                    t++;
                }

                var playlists = this.ParentPopup.Cards.Skip(10 * this.ParentPopup.currentPageIndex).Take(10).ToList();
                if (playlists.Count > 0)
                {
                    this.ParentPopup.stackSongItems.Children.Clear();
                    foreach (var item in playlists)
                    {
                        this.ParentPopup.stackSongItems.Children.Add(item);
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

                var content = new Popup_SmartUpdater(baseTrack);
                if (ParentAlbum != null)
                {
                    content.AlbumImageInBackground.ImageSource = ParentAlbum.AlbumImageInBackground.Source;
                }

                Global.mPlayer.popUp_Frame.Content = content;
            }
            catch (Exception)
            {

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
                //Global.mPlayer.RemoveFromPlaylist(baseTrack);
                showAddToPlaylistx = true;

                NormalModeActiveBtnContainer.Visibility = Visibility.Collapsed;
                AdminActiveBtnContainer.Visibility = Visibility.Collapsed;
                AdminActiveBtnPanels.Visibility = Visibility.Collapsed;
                AddedToPlaylistVector.Visibility = Visibility.Collapsed;
                RemovedFromPlaylistInfoButton.Visibility = Visibility.Visible;
                
                var a = new DoubleAnimation
                {
                    From = 1.0,
                    To = 0.0,
                    FillBehavior = FillBehavior.Stop,
                    BeginTime = TimeSpan.FromSeconds(1),
                    Duration = new Duration(TimeSpan.FromSeconds(0.5))
                };

                var storyboard = new Storyboard();
                storyboard.Children.Add(a);
                Storyboard.SetTarget(a, RemovedFromPlaylistInfoButton);
                Storyboard.SetTargetProperty(a, new PropertyPath(OpacityProperty));
                storyboard.Completed += delegate {
                    RemovedFromPlaylistInfoButton.Visibility = System.Windows.Visibility.Collapsed;
                    AddedToPlaylistVector.Visibility = Visibility.Collapsed;
                };
                storyboard.Begin();
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
                    if (!CheckIfAddedInPlaylist(baseTrack, shouldPlaylistCountChange: false))
                    {
                        btnAddToPlaylistx.Visibility = Visibility.Visible;
                        AddToPlaylistxNormal.Visibility = Visibility.Visible;
                        btnRemove.Visibility = Visibility.Collapsed;
                        RemoveFromPlaylistxNormal.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        btnAddToPlaylistx.Visibility = Visibility.Collapsed;
                        AddToPlaylistxNormal.Visibility = Visibility.Collapsed;
                        btnRemove.Visibility = Visibility.Visible;
                        RemoveFromPlaylistxNormal.Visibility = Visibility.Visible;
                    }

                    if (this.ParentAlbum == null) return;
                    var childs = this.ParentAlbum.stackSongItems.Children;
                    var currIndex = childs.IndexOf(this);
                    foreach (var item in childs)
                    {
                        var cardSong = item as PlaylistCardSong;
                        if (childs.IndexOf((UIElement)item) != currIndex)
                        {
                            if (cardSong.AdminActiveBtnContainer.Visibility == Visibility.Visible)
                            {
                                cardSong.AdminActiveBtnContainer.Visibility = Visibility.Collapsed;
                                cardSong.AdminActiveBtnPanels.Visibility = Visibility.Collapsed;
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
                    if (!CheckIfAddedInPlaylist(baseTrack, shouldPlaylistCountChange: false))
                    {
                        if(this.ParentAlbum.PLaylists.Count(x => x.TrackLibrary.Id == baseTrack.Id && x.ToAdd) == 0)
                        {
                            AddToPlaylistxNormal.Visibility = Visibility.Visible;
                            RemoveFromPlaylistxNormal.Visibility = Visibility.Collapsed;
                        }
                        else
                        {
                            AddToPlaylistxNormal.Visibility = Visibility.Collapsed;
                            RemoveFromPlaylistxNormal.Visibility = Visibility.Visible;
                        }
                        
                    }
                    else
                    {
                        AddToPlaylistxNormal.Visibility = Visibility.Collapsed;
                        RemoveFromPlaylistxNormal.Visibility = Visibility.Visible;
                    }
                    if (this.ParentAlbum != null)
                    {
                        var childs = this.ParentAlbum.stackSongItems.Children;
                        var currIndex = childs.IndexOf(this);
                        foreach (var item in childs)
                        {
                            var cardSong = item as PlaylistCardSong;
                            if (childs.IndexOf((UIElement)item) != currIndex)
                            {
                                if (cardSong.NormalModeActiveBtnContainer.Visibility == Visibility.Visible)
                                {
                                    cardSong.NormalModeActiveBtnContainer.Visibility = Visibility.Collapsed;
                                    cardSong.NormalModeActiveBtnPanel.Visibility = Visibility.Collapsed;
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

        private void AddFavourtie(object sender, RoutedEventArgs e)
        {
            try
            {
                if(FavDotButton.Visibility != Visibility.Collapsed) { return; }
                FavDotButton.Visibility = Visibility.Visible;
                NormalModeActiveBtnContainer.Visibility = Visibility.Collapsed;
                AdminActiveBtnContainer.Visibility = Visibility.Collapsed;
                FavInfoButton.Visibility = System.Windows.Visibility.Visible;
                this.ParentAlbum.FavPLaylists.Add(new PlaylistWithActionTaken
                {
                    TrackLibrary=baseTrack,
                    ToAdd=true
                });
                var a = new DoubleAnimation
                {
                    From = 1.0,
                    To = 0.0,
                    FillBehavior = FillBehavior.Stop,
                    BeginTime = TimeSpan.FromSeconds(1),
                    Duration = new Duration(TimeSpan.FromSeconds(0.1))
                };
                var storyboard = new Storyboard();

                storyboard.Children.Add(a);
                Storyboard.SetTarget(a, FavInfoButton);
                Storyboard.SetTargetProperty(a, new PropertyPath(OpacityProperty));
                storyboard.Completed += delegate
                {
                    FavInfoButton.Visibility = System.Windows.Visibility.Collapsed;
                    FavDotButton.Visibility = Visibility.Visible;
                };
                storyboard.Begin();
            }
            catch (Exception)
            {

            }
        }

        private void RemoveFavourtie(object sender, RoutedEventArgs e)
        {
            try
            {
                FavDotButton.Visibility = Visibility.Collapsed;
                RemoveFavBtnContainer.Visibility = Visibility.Visible;                
            }
            catch (Exception)
            {

            }
        }

        private void RemoveYes(object sender, MouseButtonEventArgs e)
        {
            try
            {
                RemoveFavBtnContainer.Visibility = Visibility.Collapsed;
                FavInfoText.Content = "Removed";
                FavInfoButton.Visibility = Visibility.Visible;
                FavDotButton.Visibility = Visibility.Collapsed;
                this.ParentAlbum.FavPLaylists.Add(new PlaylistWithActionTaken
                {
                    TrackLibrary = baseTrack,
                    ToAdd = false
                });
                var a = new DoubleAnimation
                {
                    From = 1.0,
                    To = 0.0,
                    FillBehavior = FillBehavior.Stop,
                    BeginTime = TimeSpan.FromSeconds(1),
                    Duration = new Duration(TimeSpan.FromSeconds(0.2))
                };
                var storyboard = new Storyboard();

                storyboard.Children.Add(a);
                Storyboard.SetTarget(a, FavInfoButton);
                Storyboard.SetTargetProperty(a, new PropertyPath(OpacityProperty));
                storyboard.Completed += delegate
                {
                    FavInfoButton.Visibility = System.Windows.Visibility.Collapsed;
                };
                storyboard.Begin();
                
            }
            catch (Exception)
            {

            }
        }

        private void RemoveNo(object sender, MouseButtonEventArgs e)
        {
            try
            {
                RemoveFavBtnContainer.Visibility = Visibility.Collapsed;
            }
            catch (Exception)
            {

            }
        }

        private void BtnFavActive_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                RemoveFavBtnContainer.Visibility = Visibility.Visible;
                var childs = this.ParentAlbum.stackSongItems.Children;
                var currIndex = childs.IndexOf(this);
                foreach (var item in childs)
                {
                    var cardSong = item as PlaylistCardSong;
                    if (childs.IndexOf((UIElement)item) != currIndex)
                    {
                        if (cardSong.NormalModeActiveBtnContainer.Visibility == Visibility.Visible)
                        {
                            cardSong.NormalModeActiveBtnContainer.Visibility = Visibility.Collapsed;
                            cardSong.NormalModeActiveBtnPanel.Visibility = Visibility.Collapsed;
                        }
                        if (cardSong.RemoveFavBtnContainer.Visibility == Visibility.Visible)
                        {
                            cardSong.RemoveFavBtnContainer.Visibility = Visibility.Collapsed;
                        }
                    }
                }
            }
            catch (Exception)
            {

            }
        }

        private void AddToPlaylistX(object sender, MouseButtonEventArgs e)
        {
            if (this.ParentPopup.idToRemove.Count(x => x == baseTrack.Id) > 0)
            {
                this.ParentPopup.idToRemove.Remove(this.baseTrack.Id);
            }
            btnAddToPlaylistx_Click(sender, e);
        }

        private void RemoveFromPlaylistX(object sender, MouseButtonEventArgs e)
        {
            this.ParentPopup.idToRemove.Add(this.baseTrack.Id);
            btnRemove_Click(sender, e);
        }

        private void QeueueNext(object sender, MouseButtonEventArgs e)
        {
            btnQueNow_Click(sender, e);
        }
    }
}
