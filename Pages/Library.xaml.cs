using JukeBoxSolutions.Class;
using JukeBoxSolutions.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace JukeBoxSolutions
{
    /// <summary>
    /// Interaction logic for Library.xaml
    /// </summary>
    public partial class Library : Page
    {
        #region Enums





        #endregion Enums

        #region Properties

        public int TotalCardIndex { get; set; }
        public int CurrentCardIndex { get; set; }

        private PlayListHelper ActivePlaylistHelper = new PlayListHelper();
        public List<int> BufferSelectedAlbums { get; set; }
        private List<int> BufferSelectedSongs { get; set; }

        private Label AlbumCountLabel { get; set; }
        Frame mainFrame;
        private LibraryMode _libmode;
        string txtLibMode
        {
            set
            {
                lblLibraryMode.Content = value;
                lblLibraryModeB.Content = value;
            }
        }
        LibraryMode LibMode
        {
            get { return _libmode; }
            set
            {
                //Global.AppControlMode = Global.AppControlModeEnum.Playlist;
                //parent.isShowOnlyPlaylist = true;
                ////parent.SetPlaylistMode(PlaylistName);
                //parent.SetPlaylistMode(basePlaylistDetail, Library.PlaylistModeEnum.ViewPlaylist);
                switch (value)
                {
                    case LibraryMode.Library:
                        if (Global.AppPlaylistModeBufferID > -1 && isShowOnlyPlaylist == true)
                        {
                            // Do nothing
                            //txtLibMode = "Current Playlist, Already set";
                        }
                        else
                        {
                            if (Global.AppMode == Global.AppModeEnum.Music)
                                txtLibMode = "Music Library";
                            else if (Global.AppMode == Global.AppModeEnum.Video)
                                txtLibMode = "Video Library";
                            else if (Global.AppMode == Global.AppModeEnum.Karaoke)
                                txtLibMode = "Karaoke Library";
                            else if (Global.AppMode == Global.AppModeEnum.Radio)
                                txtLibMode = "Radio Library";
                        }
                        break;
                    case LibraryMode.Playlists:
                        txtLibMode = "Playlists";
                        break;
                }
                _libmode = value;
            }
        }
        private List<int> BufferPlaylistTrackIDs { get; set; }
        private bool hasBufferPlaylist
        {
            get
            {
                if (BufferPlaylistTrackIDs == null)
                    return false;
                else if (BufferPlaylistTrackIDs.Any() && Global.AppActionMode != Global.AppActionModeEnum.Idle)
                    return true;
                else
                    return false;
            }
        }

        // serves as databuffer
        private Queue<List<PlayListSource2>> QueueSourceTracks { get; set; }
        private bool isLibraryReady = false;
        private bool isLoadingLibrary = false;

        private void SourceTracksDBBatchController()
        {
            if (Global.BackgroundWorkerLibraryLoading == Global.BackgroundCheckout.GreenLight)
                Global.BackgroundWorkerLibraryLoading = Global.BackgroundCheckout.Abort;
            if (Global.AdminSettings.IsMusicLibraryScrollHorizontal)
            {
                cFactory.ResetGrid(ref gridLibraryHorizontal);
            }
            else
            {
                cFactory.ResetGrid(ref gridLibraryVertical);
            }

            if (LibMode == LibraryMode.Playlists)
            {
                Task task2 = Task.Run((Action)LoadPlaylists);
            }
            else
            {
                Task task1 = Task.Run((Action)LoadLibrary);
            }

            //Application.Current.Dispatcher.Invoke((Action)delegate
            //{
            //});
        }

        private void LoadPlaylists()
        {
            int batchCounter = 350;
            int trackbatchCounter = 30;

            isLoadingLibrary = true;
            int randomcounter = 0;
            int librarySkipIndex = 0;
            int trackCounter = -1;
            bool isAlbumsOnly = true;
            bool isAlbumEnd = false;
            bool isBatchEnd = false;
            bool hasAlbums = false;
            bool isFirstRun = true;
            cFactory.ResetGrid();
            // Load Batch

            // Create Controls


            ActivePlaylistHelper.SongIDs = new List<int>();
            SourceTracks3 = new List<PlayListSource3>();


            List<PlayListDetail> rawplaylists = new List<PlayListDetail>();

            using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
            {
                switch (Global.AppMode)
                {
                    case Global.AppModeEnum.Video:
                        rawplaylists = (from pl in db.PlayListDetails
                                        where pl.Type > 0 && pl.isVideo
                                        select pl).ToList();
                        break;
                    case Global.AppModeEnum.Music:
                        rawplaylists = (from pl in db.PlayListDetails
                                        where pl.Type > 0 && pl.isMusic
                                        select pl).ToList();
                        break;
                    case Global.AppModeEnum.Karaoke:
                        rawplaylists = (from pl in db.PlayListDetails
                                        where pl.Type > 0 && pl.isKaraoke
                                        select pl).ToList();
                        break;
                    case Global.AppModeEnum.Radio:
                        rawplaylists = (from pl in db.PlayListDetails
                                        where pl.Type > 0 && pl.isRadio
                                        select pl).ToList();
                        break;
                }
            }


            // Repeat
            foreach (PlayListDetail p in rawplaylists)
            {
                //List<PlayListSource2> TracksBatch = new List<PlayListSource2>();
                PlayListSource3 pList = new PlayListSource3() { SourcePlaylistDetails = p };

                using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                {
                    if (trackCounter == -1)
                        trackCounter = db.LibraryViews.Count(c => c.Type == Global.AppModeString);


                    //                var GroupedTags = playlist.Playlists.GroupBy(c => c.TrackLibrary.SongLibraries.First().Genre)
                    //.Select(g => new { name = g.Key, count = g.Count() }).OrderByDescending(c => c.count).First();


                    var plists = db.Playlists.Where(w => w.PlaylistId == p.Id).OrderBy(o => o.SequenceNumber);
                    var list = from a in plists
                               join b in db.LibraryViews
                               on a.TrackId equals b.TrackId
                               select b;
                    pList.LibraryTracks = list.ToList();


                    SourceTracks3.Add(pList);

                    if (!isShowOnlyPlaylist || (!Global.mPlayer.isIdle && Global.LibraryForceViewMode))
                    {
                        if (isFirstRun)
                        {
                            if (!Global.mPlayer.isIdle)
                            {
                                var tempIds = (from s in db.Playlists
                                               where s.PlaylistId == Global.NowPlayingId
                                               select s.TrackId).ToList();

                                ActivePlaylistHelper.SongIDs = db.LibraryViews.Where(w => w.Type == Global.AppModeString && tempIds.Contains(w.TrackId)).Select(s => s.SongId.Value).ToList();
                            }
                            ActivePlaylistHelper.AlbumIDs = new List<int>();
                            isFirstRun = false;
                        }

                    }

                    //TracksBatch = (from a in albums2
                    //               select new PlayListSource2() { SourcePlaylistDetails = db.PlayListDetails.Find(a.SourceAlbumId ?? -1), AlbumTracks = a.LibraryTracks.Select(s => new PlayListSource2.AlbumTrack() { SongID = (int)s.SongId, TrackID = s.TrackId, SongName = s.SongName }).ToList() }).ToList();
                }

                // Load Controls
                cFactory.AppendPlaylistRow(pList, this);

                isLibraryReady = true;
                if (isBatchEnd)
                {
                    if (!isAlbumEnd)
                    {
                        isAlbumEnd = true;
                        if (Global.AdminSettings.IsMusicLibraryScrollHorizontal)
                        {
                            cFactory.CloseAlbumGrid(ref gridLibraryHorizontal);
                        }
                        else
                        {
                            cFactory.CloseAlbumGrid(ref gridLibraryVertical);
                        }
                    }
                    else
                    {
                        if (Global.AdminSettings.IsMusicLibraryScrollHorizontal)
                        {
                            //cFactory.CloseSongsGrid(ref gridLibraryHorizontal);
                        }
                        else
                        {
                            //cFactory.CloseSongsGrid(ref gridLibraryVertical);
                        }
                        break;
                    }

                    isAlbumsOnly = false;
                    librarySkipIndex = 0;
                    isBatchEnd = false;
                }
            }
        }
        private void LoadLibrary()
        {
            while (Global.BackgroundWorkerLibraryLoading == Global.BackgroundCheckout.Abort)
            {
                // Wait until background process has stopped, should only be a moment
                //gridLoading.Dispatcher.Invoke(() => { gridLoading.Visibility = Visibility.Visible; });
            }

            //gridLoading.Dispatcher.Invoke(() => { gridLoading.Visibility = Visibility.Collapsed; });

            if (_loadFilterIndex)
            {
                FilterIndex();
                ApplyFilters3(true);
            }
            else
                ApplyFilters3(false);


            Global.BackgroundWorkerLibraryLoading = Global.BackgroundCheckout.GreenLight;

            int tempTrackCountBoost = 0;
            int batchCounter = 100;
            int trackbatchCounter = 30;

            isLoadingLibrary = true;
            int randomcounter = 0;
            int librarySkipIndex = 0;
            int librarySkipReset = 0;
            //int trackCounter = -1;
            bool isAlbumsOnly = true;
            bool isAlbumEnd = false;
            bool isBatchEnd = false;
            bool hasAlbums = false;
            bool isFirstRun = true;

            cFactory.ResetGrid();
            // Load Batch

            // Create Controls

            if (LibMode == LibraryMode.Library)
                ActivePlaylistHelper.SongIDs = new List<int>();

            bool isFreshLoad = false;
            if (SourceTracks3 == null)
            {
                SourceTracks3 = new List<PlayListSource3>();
                FilterTracks3 = null;

                isFreshLoad = true;
            }
            else
            {
                isFreshLoad = false;
                batchCounter = trackbatchCounter;
            }

            List<PlayListSource2> TracksBatch = new List<PlayListSource2>();

            // Repeat
            while (isLoadingLibrary)
            {
                using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                {
                    if (isFreshLoad)
                    {
                        List<LibraryView> libView = new List<LibraryView>();
                        if (Global.LibraryForceViewMode && Global.AppControlMode == Global.AppControlModeEnum.Playlist)
                        {
                            var p = db.Playlists.Where(w => w.PlaylistId == Global.AppPlaylistModeBufferID).Select(s => s.TrackId).ToList();

                            //if (trackCounter == -1)
                            //    trackCounter = db.LibraryViews.Count(c => c.Type == Global.AppModeString);
                            libView = db.LibraryViews.Where(lv => lv.Type == Global.AppModeString && (lv.AlbumId.HasValue == isAlbumsOnly) && p.Contains(lv.TrackId)).OrderBy(o1 => o1.Artists).ThenByDescending(o => o.AlbumId).Skip(librarySkipIndex).Take(batchCounter + tempTrackCountBoost).ToList();
                        }
                        else
                        {
                            //if (trackCounter == -1)
                            //    trackCounter = db.LibraryViews.Count(c => c.Type == Global.AppModeString);

                            libView = db.LibraryViews.Where(lv => lv.Type == Global.AppModeString && (lv.AlbumId.HasValue == isAlbumsOnly)).OrderBy(o1 => o1.Artists).ThenByDescending(o => o.AlbumId).Skip(librarySkipIndex).Take(batchCounter + tempTrackCountBoost).ToList();
                        }


                        if (libView.Count < batchCounter + tempTrackCountBoost)
                            isBatchEnd = true;

                        tempTrackCountBoost = 0;

                        if (libView.Any())
                        {
                            if (isAlbumsOnly)
                            {
                                hasAlbums = true;
                                if (isBatchEnd)
                                    batchCounter = trackbatchCounter;
                            }

                            // Remove Last Album
                            if (libView.Last().AlbumId != null && isBatchEnd == false)
                            {
                                int lastalbumid = libView.Last().AlbumId.Value;
                                int removeCount = libView.Count(cr => cr.AlbumId == lastalbumid);
                                if (removeCount == libView.Count)
                                {
                                    tempTrackCountBoost = batchCounter;
                                }

                                libView.RemoveAll(r => r.AlbumId == lastalbumid);
                            }

                            int skipcountadjustment = libView.Count();
                            librarySkipIndex += skipcountadjustment;
                            //var albums2 = (from v in libView
                            //               group v by v.AlbumId into g
                            //               select new { album = g.Key, tracks = g }).ToList();





                            var albums2 = (from v in libView
                                           group v by v.AlbumId into g
                                           select new PlayListSource3() { SourceAlbumId = g.Key, LibraryTracks = g.Select(s => (LibraryView)s).ToList() }).ToList();

                            SourceTracks3.AddRange(albums2);

                            if (!isShowOnlyPlaylist || (!Global.mPlayer.isIdle && Global.LibraryForceViewMode))
                            {
                                if (isFirstRun)
                                {
                                    if (!Global.mPlayer.isIdle && LibMode == LibraryMode.Library)
                                    {
                                        var tempIds = (from s in db.Playlists
                                                       where s.PlaylistId == Global.mPlayer.playList.PlayListId
                                                       select s.TrackId).ToList();

                                        ActivePlaylistHelper.SongIDs = db.LibraryViews.Where(w => w.Type == Global.AppModeString && tempIds.Contains(w.TrackId)).Select(s => s.SongId.Value).ToList();
                                    }
                                    ActivePlaylistHelper.AlbumIDs = new List<int>();
                                    isFirstRun = false;
                                }

                                var xxx = (from a in albums2
                                           where a.LibraryTracks.Any(c => ActivePlaylistHelper.SongIDs.Contains((int)c.SongId)) && a.SourceAlbumId.HasValue
                                           select (int)a.SourceAlbumId).ToList();
                                ActivePlaylistHelper.AlbumIDs.AddRange(xxx);
                            }
                            else
                            {
                                // Show Only Playlist
                            }

                            // Load Artists


                            TracksBatch = (from a in albums2
                                           select new PlayListSource2() { SourceAlbumLibrary = db.AlbumLibraries.Find(a.SourceAlbumId ?? -1), AlbumTracks = a.LibraryTracks.Select(s => new PlayListSource2.AlbumTrack() { SongID = (int)s.SongId, TrackID = s.TrackId, SongName = s.SongName, ArtistName = s.Artists }).ToList() }).ToList();
                        }
                    }
                    else
                    {
                        //var libView = db.LibraryViews.Where(lv => lv.Type == Global.AppModeString && (lv.AlbumId.HasValue == isAlbumsOnly)).OrderByDescending(o => o.AlbumId).Skip(librarySkipIndex).Take(batchCounter).ToList();

                        List<PlayListSource3> albums2 = new List<PlayListSource3>();
                        if (FilterTracks3 == null)
                            albums2 = SourceTracks3.Skip(librarySkipIndex).Take(batchCounter).ToList();
                        else
                            albums2 = FilterTracks3.Skip(librarySkipIndex).Take(batchCounter).ToList();

                        if (albums2.Count < batchCounter)
                        {
                            isBatchEnd = true;

                            // Grab albums only
                            int i = albums2.Count(c => c.SourceAlbumId != null);
                            albums2.RemoveRange(i, albums2.Count() - i);
                            librarySkipReset = i;
                            batchCounter = 1;
                        }
                        else
                        {
                            librarySkipIndex += batchCounter;
                        }

                        TracksBatch = (from a in albums2
                                       select new PlayListSource2() { SourceAlbumLibrary = db.AlbumLibraries.Find(a.SourceAlbumId ?? -1), AlbumTracks = a.LibraryTracks.Select(s => new PlayListSource2.AlbumTrack() { SongID = (int)s.SongId, TrackID = s.TrackId, SongName = s.SongName, ArtistName = s.Artists }).ToList() }).ToList();
                    }
                }

                // Load Controls
                // BuildAlbumCards
                if (Global.BackgroundWorkerLibraryLoading != Global.BackgroundCheckout.Abort)
                {
                    if (TracksBatch.Any(a => a.isTracksOnly))
                    {
                        AlbumsCountInfoLabel.Content = $"Songs ({TracksBatch.First().AlbumTracks.Count})";
                        cFactory.AppendSongsRows(TracksBatch.First(), this, hasAlbums, ActivePlaylistHelper.SongIDs, LogLibraryLoadTime);
                    }
                    else
                    {
                        cFactory.AppendAlbumRows(TracksBatch, this, ActivePlaylistHelper.AlbumIDs, LogLibraryLoadTime);
                    }
                }

                isLibraryReady = true;
                if (isBatchEnd)
                {
                    if (!isAlbumEnd)
                    {
                        isAlbumEnd = true;
                        if (Global.AdminSettings.IsMusicLibraryScrollHorizontal)
                        {
                            cFactory.CloseAlbumGrid(ref gridLibraryHorizontal);
                        }
                        else
                        {
                            cFactory.CloseAlbumGrid(ref gridLibraryVertical);
                        }
                    }
                    else
                    {
                        if (Global.AdminSettings.IsMusicLibraryScrollHorizontal)
                        {
                            cFactory.CloseSongsGrid(ref gridLibraryHorizontal);
                        }
                        else
                        {
                            cFactory.CloseSongsGrid(ref gridLibraryVertical);
                        }
                        break;
                    }

                    isAlbumsOnly = false;
                    librarySkipIndex = librarySkipReset;
                    isBatchEnd = false;
                }
            }

            Global.BackgroundWorkerLibraryLoading = Global.BackgroundCheckout.Idle;
        }

        private List<PlayListSource2> PreloaderTracks { get; set; }
        private List<PlayListSource3> SourceTracks3 { get; set; }
        private List<PlayListSource2> SourceTracks2 { get; set; }
        private List<PlayListSource> SourceTracks { get; set; }
        private List<PlayListSource3> FilterTracks3 { get; set; }
        public List<PlayListSource> FilterTracks { get; set; }



        private bool isNewFilter = false;
        private bool isUnsortedFilter = false;
        internal class SearchFilterClass { public string SearchText; public SearchMode SelectedMode; }
        private List<SearchFilterClass> SearchFilters = new List<SearchFilterClass>();
        private List<string> GenreFilters = new List<string>();
        private List<string> YearFilters = new List<string>();


        static PlayListDetail playlistModePlaylist { get; set; }

        // ----------------------------



        internal void UpdateAppActionMode(int AlbumId)
        {
            try
            {

                RefreshCardItems(AlbumId);
                //foreach (var a in gridLibrary.Children.OfType<System.Windows.Controls.Grid>())
                //{
                //    foreach (var b in a.Children.OfType<JukeBoxSolutions.Controls.LibraryCard>())
                //    {
                //        b.SetAppActionMode();
                //    }

                //    foreach (var c in a.Children.OfType<JukeBoxSolutions.Controls.LibraryCardSong>())
                //    {
                //        c.SetAppActionMode();
                //    }
                //}
            }
            catch (Exception)
            {

            }
        }


        #endregion Properties

        #region Classes


        internal class PlayListHelper
        {
            enum HelperPlayListModeEnum
            {
                Inactive,
                LivePlaylist,
                B
            }

            HelperPlayListModeEnum Mode = HelperPlayListModeEnum.Inactive;

            public bool HasActiveTracks
            {
                get
                {
                    if (SongIDs == null) return false;
                    else return SongIDs.Any();
                }
            }
            private bool _isModified = false;
            private bool _hasData = false;

            private List<int> _albumIDs = new List<int>();
            internal List<int> AlbumIDs
            {
                get
                {
                    //if (!_hasData)
                    //    Update();
                    return _albumIDs;
                }
                set { _albumIDs = value; }
            }

            public List<int> SongIDs { get; internal set; }

            internal void Update()
            {
                if (!_hasData)
                {
                    // Get Albums from Current Playlist
                    using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                    {
                        switch (Mode)
                        {
                            case HelperPlayListModeEnum.LivePlaylist:
                                var a = from b in db.TrackLibraries
                                        where b.Playlists.Any(w => w.PlaylistId == Global.mPlayer.playList.PlayListId)
                                        select b.SongLibraries.First().AlbumLibraries.Where(ab => ab.isHidden == false).First().AlbumId;

                                _albumIDs = a.ToList();
                                _hasData = true;
                                break;
                        }

                    }
                }

            }


        }

        internal class CardFactory
        {

            //int LibraryCardX = 200;
            //int LibraryCardY = 200;

            private double scrollViewLibraryWidth { get; set; }
            private double scrollViewLibraryHeight { get; set; }
            //private Grid libraryGrid { get; set; }

            public void ResetGrid(ref Grid LibraryGrid)
            {
                //var l = LibraryGrid;
                LibraryGrid.Children.Clear();
                LibraryGrid.RowDefinitions.Clear();
                LibraryGrid.RowDefinitions.Add(new RowDefinition() { MinHeight = 200 });
                LibraryGrid.RowDefinitions.Add(new RowDefinition() { MinHeight = 200 });

                //LibraryGrid.RowDefinitions.Clear();
                //// top blank row
                //LibraryGrid.RowDefinitions.Add(new RowDefinition() { MinHeight = 200 });
                //// bottom blank row
                //LibraryGrid.RowDefinitions.Add(new RowDefinition() { MinHeight = 200 });

                isAlbumRowLoaded = false;
                isPlaylistRowLoaded = false;
                isTracksRowLoaded = false;
            }

            public CardFactory(ref Grid LibraryGrid, double ScrollViewLibraryWidth, double ScrollViewLibraryHeight)
            {
                // Clear Library with new Factory
                // libraryGrid = LibraryGrid;
                //ResetGrid(ref LibraryGrid);

                scrollViewLibraryWidth = ScrollViewLibraryWidth;
                scrollViewLibraryHeight = ScrollViewLibraryHeight;
            }

            public void AddNewTitle()
            {

            }

            public class GridMath
            {
                public int CardPositionCount = 1;
                //cardsize = 200;
                int cardMargin = 10;
                int cardRes = 0;
                int cardCountX = 0;
                int cardIndexY = 0;

                int cardsizeX = 0;
                int cardsizeY = 0;

                double ControlWidth;


                public GridMath(double ViewLibraryWidth)
                {
                    // sets the grid layout and math setup
                    ControlWidth = ViewLibraryWidth;
                }

                private void CalcCardDistribution()
                {
                    cardCountX = Math.DivRem((int)ControlWidth, cardsizeX, out cardRes);

                    if ((cardCountX * cardsizeX) + ((cardCountX - 1) * cardMargin) > ControlWidth)
                    {
                        // else remove 1 x, -- > add rest to left margin
                        cardCountX--;
                        cardRes = ((cardCountX * cardsizeX) + ((cardCountX - 1) * cardMargin)) / 2;
                        cardRes = (int)(ControlWidth - ((cardCountX * cardsizeX) + ((cardCountX - 1) * cardMargin))) / 2;
                    }
                    else
                    {
                        // if x + (x-1 * margin) -- > add rest to left margin
                        cardRes = cardRes / 2;
                        cardRes = (int)(ControlWidth - ((cardCountX * cardsizeX) + ((cardCountX - 1) * cardMargin))) / 2;
                    }
                }

                public void ResetRowGrid(int cardWidth, int cardHeight)
                {
                    // Setup

                    CardPositionCount = 1;
                    cardsizeX = (int)cardWidth;
                    cardsizeY = (int)cardHeight;
                    cardMargin = 10;
                    cardRes = 0;
                    cardCountX = 0;
                    cardIndexY = 0;

                    // Math
                    CalcCardDistribution();
                }

                public Thickness GetCardPosition()
                {
                    if (CardPositionCount == 1)
                    {
                        //CalculateCardGrid();
                    }

                    //0
                    //10 + 200
                    if (CardPositionCount - (cardIndexY * cardCountX) > cardCountX)
                    {
                        cardIndexY++;
                    }

                    // 1 - 0 = 1
                    // 5 - 4 = 1;
                    // 8 - 8 = 1;
                    int cardinrow = CardPositionCount - (cardIndexY * cardCountX);
                    int x = ((cardinrow - 1) * (cardsizeX + cardMargin)) + cardRes;
                    int y = (cardIndexY * cardsizeY) + (cardMargin * cardIndexY);

                    Thickness m = new Thickness(x, y, 0, 0);

                    CardPositionCount++;

                    return m;
                }

                //private void CalculateCardGrid()
                //{
                //    // number of cards canLibrary.Width / 200
                //    // double ControlWidth = scrollViewLibrary.Width;

                //    cardCountX = Math.DivRem((int)ControlWidth, cardsize, out cardRes);
                //    //cardCountX = Math.DivRem((int)canLibrary.Width, cardsize, out cardRes);

                //    //if ((cardCountX * cardsize) + ((cardCountX - 1) * cardMargin) > canLibrary.Width)
                //    if ((cardCountX * cardsize) + ((cardCountX - 1) * cardMargin) > ControlWidth)
                //    {
                //        // else remove 1 x, -- > add rest to left margin

                //        cardCountX--;
                //        cardRes = ((cardCountX * cardsize) + ((cardCountX - 1) * cardMargin)) / 2;
                //        //cardRes = (int)(canLibrary.Width - ((cardCountX * cardsize) + ((cardCountX - 1) * cardMargin))) / 2;
                //        cardRes = (int)(ControlWidth - ((cardCountX * cardsize) + ((cardCountX - 1) * cardMargin))) / 2;
                //    }
                //    else
                //    {
                //        // if x + (x-1 * margin) -- > add rest to left margin
                //        cardRes = cardRes / 2;
                //        //cardRes = (int)(canLibrary.Width - ((cardCountX * cardsize) + ((cardCountX - 1) * cardMargin))) / 2;
                //        cardRes = (int)(ControlWidth - ((cardCountX * cardsize) + ((cardCountX - 1) * cardMargin))) / 2;
                //    }
                //}

            }

            public class GridManager
            {
                internal Grid SectionGrid { get; set; }
                internal Grid AlbumSectionGrid { get; set; }
                internal Grid TrackSectionGrid { get; set; }
                public void LoadSectionGrid(string tagName)
                {
                    Grid gridLibrary;
                    if (Global.AdminSettings.IsMusicLibraryScrollHorizontal)
                    {
                        gridLibrary = ((Library)Global.MainFrame.Content).gridLibraryHorizontal;
                    }
                    else
                    {
                        gridLibrary = ((Library)Global.MainFrame.Content).gridLibraryVertical;
                    }
                    if (tagName == "Albums")
                    {
                        foreach (var grid in gridLibrary.Children.OfType<Grid>())
                        {
                            if (grid.Tag != null && grid.Tag.ToString() == tagName)
                            {
                                AlbumSectionGrid = grid;
                            }

                        }
                    }
                    else if (tagName == "Tracks")
                    {
                        foreach (var grid in gridLibrary.Children.OfType<Grid>())
                        {
                            if (grid.Tag != null && grid.Tag.ToString() == tagName)
                            {
                                TrackSectionGrid = grid;
                            }

                        }
                    }
                    else
                    {
                        foreach (var grid in gridLibrary.Children.OfType<Grid>())
                        {
                            if (grid.Tag != null && grid.Tag.ToString() == tagName)
                            {
                                SectionGrid = grid;
                            }

                        }
                    }

                }

                public void UnLoadSectionGrid()
                {
                    SectionGrid = null;
                }
                public int CardPositionCount = 0;
                //cardsize = 200;
                int cardMargin = 10;
                int cardRes = 0;
                int cardCountX = 0;
                int cardIndexY = 0;

                int cardsizeX = 0;
                int cardsizeY = 0;

                double ControlWidth;

                public int CardCount { get; internal set; }

                public GridManager(double ViewLibraryWidth)
                {
                    // sets the grid layout and math setup
                    ControlWidth = ViewLibraryWidth;
                    //SectionGrid = sectionGrid;
                }

                public GridManager(double ViewLibraryWidth, ref Grid sectionGrid)
                {
                    // sets the grid layout and math setup
                    ControlWidth = ViewLibraryWidth;
                    SectionGrid = sectionGrid;
                }

                internal void ResetRowGrid(int cardWidth, int cardHeight)
                {
                    // Setup

                    CardPositionCount = 0;
                    cardsizeX = (int)cardWidth;
                    cardsizeY = (int)cardHeight;
                    cardMargin = 5;
                    cardRes = 0;
                    cardCountX = 0;
                    cardIndexY = 0;

                    // Math
                    CalcCardDistribution();

                    // GridSetup
                    if (SectionGrid != null)
                    {
                        SectionGrid.RowDefinitions.Add(new RowDefinition());
                        for (int i = 0; i < cardCountX; i++)
                        {
                            SectionGrid.ColumnDefinitions.Add(new ColumnDefinition());
                        }
                        SectionGrid.HorizontalAlignment = HorizontalAlignment.Center;
                    }
                    if (AlbumSectionGrid != null)
                    {
                        AlbumSectionGrid.RowDefinitions.Add(new RowDefinition());
                    }
                    if (TrackSectionGrid != null)
                    {
                        TrackSectionGrid.RowDefinitions.Add(new RowDefinition());
                    }
                }

                private void CalcCardDistribution()
                {
                    if (cardsizeX > 0)
                    {
                        cardCountX = Math.DivRem((int)ControlWidth, cardsizeX, out cardRes);

                        if ((cardCountX * cardsizeX) + ((cardCountX - 1) * cardMargin) > ControlWidth)
                        {
                            // else remove 1 x, -- > add rest to left margin
                            cardCountX--;
                            cardRes = ((cardCountX * cardsizeX) + ((cardCountX - 1) * cardMargin)) / 2;
                            cardRes = (int)(ControlWidth - ((cardCountX * cardsizeX) + ((cardCountX - 1) * cardMargin))) / 2;
                        }
                        else
                        {
                            // if x + (x-1 * margin) -- > add rest to left margin
                            cardRes = cardRes / 2;
                            cardRes = (int)(ControlWidth - ((cardCountX * cardsizeX) + ((cardCountX - 1) * cardMargin))) / 2;
                        }
                    }

                }
                //LibraryCardSong
                internal void AddCard(ref LibraryCardRadio card)
                {
                    SectionGrid.Children.Add(card);

                    Grid.SetRow(card, SectionGrid.RowDefinitions.Count - 1);
                    Grid.SetColumn(card, CardPositionCount);
                    AddCard();
                }
                internal void AddCard(ref LibraryCardSong card)
                {
                    if (SectionGrid.ColumnDefinitions.Count == 0)
                    {
                        SectionGrid.ColumnDefinitions.Add(new ColumnDefinition());
                        SectionGrid.ColumnDefinitions.Add(new ColumnDefinition());
                        SectionGrid.ColumnDefinitions.Add(new ColumnDefinition());
                        SectionGrid.ColumnDefinitions.Add(new ColumnDefinition());
                    }
                    maxRowCount = SectionGrid.RowDefinitions.Count;
                    SectionGrid.Children.Add(card);
                    SectionGrid.VerticalAlignment = VerticalAlignment.Top;
                    Grid.SetRow(card, rowCount);
                    Grid.SetColumn(card, CardPositionCount);
                    AddCard();
                }
                internal void AddCard(ref LibraryPlaylist card)
                {
                    SectionGrid.Children.Add(card);

                    Grid.SetRow(card, SectionGrid.RowDefinitions.Count - 1);
                    Grid.SetColumn(card, CardPositionCount);
                    AddCard();
                }
                internal void AddCard(ref LibraryCard card)
                {
                    SectionGrid.Children.Add(card);

                    Grid.SetRow(card, SectionGrid.RowDefinitions.Count - 1);
                    Grid.SetColumn(card, CardPositionCount);
                    AddCard();
                }

                int rowCount = 0;
                int maxRowCount = 0;
                private void AddCard()
                {
                    CardCount++;
                    rowCount++;
                    if (rowCount == maxRowCount - 1)
                    {
                        rowCount = 0;
                        CardPositionCount++;
                    }
                }

                internal Thickness GetCardMargin()
                {
                    return new Thickness(cardMargin);
                }
            }

            int LabelCounter = 0;
            string LabelBase = "";
            Controls.LibraryLabel LabelCardBase;
            private void UpdateLibraryRow(string title, int counter)
            {
                if (counter > 0 && LabelCardBase != null)
                {
                    LabelCounter += counter;
                    LabelCardBase.Dispatcher.Invoke(() => { LabelCardBase.Title = "Loading: " + LabelBase + " (" + LabelCounter + ")"; });
                }
            }
            private void AddLibraryRow(string title, int counter = 0)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (Global.MainFrame.Content != null && Global.MainFrame.Content.GetType().Name == "Library")
                    {
                        Library lib = ((Library)Global.MainFrame.Content);
                        Controls.LibraryLabel labelCard;
                        labelCard = new LibraryLabel();

                        if (counter > 0)
                        {
                            LabelCounter = counter;
                            LabelBase = title;
                            labelCard.Title = "Loading: " + title + " (" + counter + ")";
                        }
                        else
                            labelCard.Title = title;

                        //var r = new RowDefinition();
                        //r.MaxHeight = 50;
                        //lib.gridLibrary.RowDefinitions.Insert(lib.gridLibrary.RowDefinitions.Count - 1, r);
                        //lib.gridLibrary.Children.Add(labelCard);
                        //Grid.SetRow(labelCard, lib.gridLibrary.RowDefinitions.Count - 2);
                        //LabelCardBase = labelCard;
                    }
                }
  );


            }

            internal void ResetGrid()
            {
                try
                {
                    isAlbumRowLoaded = false;
                    isTracksRowLoaded = false;
                    Grid libGrid;
                    if (Global.AdminSettings.IsMusicLibraryScrollHorizontal)
                    {
                        libGrid = ((Library)Global.MainFrame.Content).gridLibraryHorizontal;
                    }
                    else
                    {
                        libGrid = ((Library)Global.MainFrame.Content).gridLibraryVertical;
                    }

                    libGrid.RowDefinitions.Clear();
                    libGrid.Children.Clear();
                    libGrid.RowDefinitions.Clear();
                    libGrid.RowDefinitions.Add(new RowDefinition());
                    libGrid.RowDefinitions.Add(new RowDefinition());
                    isAlbumRowLoaded = false;
                    isPlaylistRowLoaded = false;
                    isTracksRowLoaded = false;
                }
                catch (Exception)
                {

                }
            }
            internal void CloseAlbumGrid(ref Grid libraryGrid)
            {
                // Close Album Grid
                if (LabelCardBase != null)
                {
                    LabelCardBase.Dispatcher.Invoke(() => { LabelCardBase.Title = LabelBase + " (" + LabelCounter + ")"; });
                    LabelCardBase = null;
                    LabelCounter = 0;
                }
            }
            internal void CloseSongsGrid(ref Grid libraryGrid)
            {
                // Close Album Grid
                //libraryGrid.Children.Add(AlbumGrid);
                //Grid.SetRow(AlbumGrid, libraryGrid.RowDefinitions.Count - 2);
                if (LabelCardBase != null)
                {
                    LabelCardBase.Dispatcher.Invoke(() => { LabelCardBase.Title = LabelBase + " (" + LabelCounter + ")"; });
                    LabelCardBase = null;
                    LabelCounter = 0;
                }
            }

            private bool isAlbumRowLoaded = false;
            private bool isPlaylistRowLoaded = false;
            private bool isTracksRowLoaded = false;
            GridManager MainGridManager;
            bool isAlbumAvailable = false;

            internal void AppendAlbumRows(IEnumerable<PlayListSource2> filteredTracks, Library Parent, List<int> bufferPlaylistAlbums, int logid = -1)
            {
                isAlbumAvailable = true;
                if (isAlbumRowLoaded == false)
                {
                    // insert row label
                    //AddLibraryRow("Albums", filteredTracks.Count());

                    //insert new row
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Grid libGrid;
                        if (Global.AdminSettings.IsMusicLibraryScrollHorizontal)
                        {
                            libGrid = ((Library)Global.MainFrame.Content).gridLibraryHorizontal;
                        }
                        else
                        {
                            libGrid = ((Library)Global.MainFrame.Content).gridLibraryVertical;
                        }
                        libGrid.RowDefinitions.Add(new RowDefinition());

                        Grid AlbumGrid = new Grid();

                        MainGridManager = new GridManager(scrollViewLibraryWidth);
                        AlbumGrid.Tag = "Albums";

                        libGrid.Children.Add(AlbumGrid);
                        Grid.SetRow(AlbumGrid, libGrid.RowDefinitions.Count - 2);
                    });

                    isAlbumRowLoaded = true;
                }
                else
                {
                    UpdateLibraryRow("Albums", filteredTracks.Count());
                }

                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (Global.MainFrame.Content != null)
                        if (Global.MainFrame.Content.GetType().Name == "Library")
                        {
                            Grid libGrid;
                            if (Global.AdminSettings.IsMusicLibraryScrollHorizontal)
                            {
                                libGrid = ((Library)Global.MainFrame.Content).gridLibraryHorizontal;
                            }
                            else
                            {
                                libGrid = ((Library)Global.MainFrame.Content).gridLibraryVertical;
                            }
                            libGrid.RowDefinitions.Clear();
                            libGrid.RowDefinitions.Add(new RowDefinition());
                            bool isSelected = false;
                            MainGridManager.LoadSectionGrid("Albums");
                            // Add Card Items

                            var wrappanel = new WrapPanel { Orientation = Orientation.Horizontal };
                            wrappanel.MaxWidth = Global.MainFrame.ActualWidth;
                            foreach (var al in filteredTracks)
                            {
                                if (Global.BackgroundWorkerLibraryLoading == Global.BackgroundCheckout.Abort)
                                    break;

                                // is selected
                                isSelected = bufferPlaylistAlbums.Contains(al.SourceAlbumLibrary.AlbumId);

                                Controls.LibraryCard card = new Controls.LibraryCard(al.SourceAlbumLibrary, al.AlbumTracks, al.ArtistName, Parent, isSelected);
                                card.Margin = new Thickness(0, 0, 10, 0);
                                if (MainGridManager.CardCount == 0)
                                {
                                    MainGridManager.ResetRowGrid((int)card.MinWidth, (int)card.MinHeight);
                                }

                                wrappanel.HorizontalAlignment = HorizontalAlignment.Center;
                                wrappanel.Children.Add(card);
                            }
                            MainGridManager.AlbumSectionGrid.RowDefinitions.Add(new RowDefinition());
                            MainGridManager.AlbumSectionGrid.Children.Add(wrappanel);
                            Grid.SetRow(wrappanel, 0);
                            //MainGridManager.UnLoadSectionGrid();
                        }
                });
            }
            //internal void AddNewAlbumRowMK2(IEnumerable<PlayListSource2> filteredTracks, ref Grid libraryGrid, Library Parent, List<int> bufferPlaylistAlbums, int logid = -1)
            //{
            //    if (filteredTracks.Any())
            //    {
            //        bool isSelected = false;

            //        // insert row label
            //        AddLibraryRow("Albums (" + filteredTracks.Count() + ")");

            //        //insert new row
            //        libraryGrid.RowDefinitions.Insert(libraryGrid.RowDefinitions.Count - 1, new RowDefinition());

            //        Grid albumGrid = new Grid();
            //        GridManager gManager = new GridManager(scrollViewLibraryWidth, ref albumGrid);
            //        albumGrid.Tag = "Albums";
            //        // Album Tracks

            //        LogSystem.AddEvent(logid, "Start creating Album Cards for Library", true);

            //        using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
            //        {
            //            foreach (var al in filteredTracks)
            //            {

            //                // is selected
            //                isSelected = bufferPlaylistAlbums.Contains(al.SourceAlbumLibrary.AlbumId);

            //                Controls.LibraryCard card = new Controls.LibraryCard(al.SourceAlbumLibrary, al.AlbumTracks, al.ArtistName, Parent, isSelected);
            //                if (gManager.CardCount == 0)
            //                {
            //                    gManager.ResetRowGrid((int)card.MinWidth, (int)card.MinHeight);
            //                }


            //                card.Margin = gManager.GetCardMargin();

            //                gManager.AddCard(ref card);
            //            }
            //        }

            //        LogSystem.AddEvent(logid, "Add Album Cards to Library", true);

            //        libraryGrid.Children.Add(albumGrid);
            //        Grid.SetRow(albumGrid, libraryGrid.RowDefinitions.Count - 2);
            //    }
            //}

            private List<SongLibrary> songLibraries { get; set; }

            private int TotalIndex { get; set; }
            internal void AddNewAlbumRow(IEnumerable<PlayListSource> filteredTracks, ref Grid libraryGrid, Library Parent, List<int> bufferPlaylistAlbums)
            {
                Parent.UniGridHorz.Children.Clear();
                Parent.UniGridVert.Children.Clear();
                using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                {
                    var l1 = db.AppSettings.ToList();
                    var isMLSHorz = db.AppSettings.FirstOrDefault(x => x.Type == "IsMLSHorz");
                    foreach (var al in filteredTracks)
                    {
                        var artists = (from x in db.AlbumLibraries.Find(al.SourceAlbumLibrary.AlbumId).SongLibraries
                                       where x.TrackLibraries.Any(a => a.Type == Global.AppModeString)
                                       select x.Artists.FirstOrDefault()).ToList();
                        string artistname = artists.First().ArtistName;
                        // is selected
                        var isSelected = bufferPlaylistAlbums.Contains(al.SourceAlbumLibrary.AlbumId);
                        Controls.LibraryCard card = new Controls.LibraryCard(al.SourceAlbumLibrary, al.SourceTracks.ToList(), artistname, Parent, isSelected);
                        card.Margin = new Thickness(5);


                        if (isMLSHorz==null || isMLSHorz.Value=="true")
                        {
                            Parent.UniGridHorz.Children.Add(card);
                        }
                        else
                        {
                            Parent.UniGridVert.Children.Add(card);
                        }
                        
                    }                    
                }

                Global.MainWindow.LibraryButtonControlPanel.Visibility = Visibility.Visible;
            }

            internal void AppendPlaylistRow(PlayListSource3 filteredTracks, Library Parent)
            {
                if (isPlaylistRowLoaded == false)
                {
                    // insert row label
                    AddLibraryRow("Playlists");

                    //insert new row
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Grid libGrid;
                        if (Global.AdminSettings.IsMusicLibraryScrollHorizontal)
                        {
                            libGrid = ((Library)Global.MainFrame.Content).gridLibraryHorizontal;
                        }
                        else
                        {
                            libGrid = ((Library)Global.MainFrame.Content).gridLibraryVertical;
                        }
                        libGrid.RowDefinitions.Insert(libGrid.RowDefinitions.Count - 1, new RowDefinition());

                        Grid AlbumGrid = new Grid();

                        MainGridManager = new GridManager(scrollViewLibraryWidth);
                        AlbumGrid.Tag = "Playlists";

                        libGrid.Children.Add(AlbumGrid);
                        Grid.SetRow(AlbumGrid, libGrid.RowDefinitions.Count - 2);
                    });

                    isPlaylistRowLoaded = true;
                }

                Application.Current.Dispatcher.Invoke(() =>
                {
                    MainGridManager.LoadSectionGrid("Playlists");

                    // Add Card Items
                    Controls.LibraryPlaylist card = new Controls.LibraryPlaylist(filteredTracks.SourcePlaylistDetails, filteredTracks.LibraryTracks.Count(), Parent);
                    //Controls.LibraryCard card = new Controls.LibraryCard(al.SourceAlbumLibrary, al.AlbumTracks, al.ArtistName, Parent, isSelected);
                    if (MainGridManager.CardCount == 0)
                    {
                        MainGridManager.ResetRowGrid((int)card.MinWidth, (int)card.MinHeight);
                    }

                    card.Margin = MainGridManager.GetCardMargin();
                    MainGridManager.AddCard(ref card);

                    MainGridManager.UnLoadSectionGrid();

                });
            }

            internal void AddNewPlaylistRow(IEnumerable<PlayListSource> filteredTracks, ref Grid libraryGrid, Library Parent)
            {
                // insert row label
                AddLibraryRow("Playlists");

                // insert new row
                libraryGrid.Children.Clear();

                //GridMath gMath = new GridMath(scrollViewLibraryWidth);

                Grid albumGrid = new Grid();
                GridManager gManager = new GridManager(scrollViewLibraryWidth, ref albumGrid);
                albumGrid.Tag = "Playlists";

                // Album Tracks
                UniformGrid wrapPanel;
                wrapPanel = new UniformGrid { Rows = 2, Columns = 6 };
                foreach (var al in filteredTracks)
                {
                    Controls.LibraryPlaylist2 card = new Controls.LibraryPlaylist2(al.SourcePlaylistDetails, al.SourceTracks.Count(), Parent);
                    wrapPanel.Children.Add(card);                    
                }

                gManager.SectionGrid.Children.Add(wrapPanel);
                Grid.SetRow(wrapPanel, 0);

                libraryGrid.Children.Add(albumGrid);
                Grid.SetRow(albumGrid, 0);
            }

            Controls.LibraryCardSongSmall cardTemplate { get; set; }
            Controls.LibraryCardSongSmall.CardControlMode CardmodeBuffer { get; set; }

            internal void AppendSongsRows(PlayListSource2 filteredTracks, Library Parent, bool hasLibraryAlbums = false, List<int> playlistbuffer = null, int logid = -1)
            {
                if (isTracksRowLoaded == false)
                {
                    // insert row label
                    if (filteredTracks.AlbumTracks.Count() > 0)
                        AddLibraryRow("Loose Songs", filteredTracks.AlbumTracks.Count());
                    else if (!hasLibraryAlbums)
                        AddLibraryRow("This library is empty");

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        if (Global.MainFrame.Content != null && Global.MainFrame.Content.GetType().Name == "Library")
                        {

                            Grid libGrid;
                            if (Global.AdminSettings.IsMusicLibraryScrollHorizontal)
                            {
                                libGrid = ((Library)Global.MainFrame.Content).gridLibraryHorizontal;
                            }
                            else
                            {
                                libGrid = ((Library)Global.MainFrame.Content).gridLibraryVertical;
                            }
                            libGrid.RowDefinitions.Insert(libGrid.RowDefinitions.Count - 1, new RowDefinition());


                            Grid AlbumGrid = new Grid();

                            MainGridManager = new GridManager(scrollViewLibraryWidth);
                            AlbumGrid.Tag = "Tracks";

                            libGrid.Children.Add(AlbumGrid);
                            Grid.SetRow(AlbumGrid, libGrid.RowDefinitions.Count - 2);
                        }
                    });

                    isTracksRowLoaded = true;
                }
                else
                {
                    UpdateLibraryRow("Loose Songs", filteredTracks.AlbumTracks.Count());
                }

                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (Global.MainFrame.Content != null && Global.MainFrame.Content.GetType().Name == "Library")
                    {
                        Grid libGrid;
                        if (Global.AdminSettings.IsMusicLibraryScrollHorizontal)
                        {
                            libGrid = ((Library)Global.MainFrame.Content).gridLibraryHorizontal;
                        }
                        else
                        {
                            libGrid = ((Library)Global.MainFrame.Content).gridLibraryVertical;
                        }
                        if (MainGridManager.AlbumSectionGrid != null && MainGridManager.AlbumSectionGrid.Children.Count == 0)
                        {
                            libGrid.RowDefinitions.Clear();
                        }

                        libGrid.RowDefinitions.Add(new RowDefinition());
                        //libGrid.RowDefinitions.Add(new RowDefinition());
                        MainGridManager.LoadSectionGrid("Tracks");

                        //MainGridManager.SectionGrid.Children.Clear();
                        //MainGridManager.SectionGrid.RowDefinitions.Clear();
                        //MainGridManager.SectionGrid.ColumnDefinitions.Clear();

                        int itemRowCount = 0;
                        int rowCount = 0;
                        int colCount = 0;
                        int itemAddedInGrid = 0;
                        int pos = 0;
                        foreach (var al in filteredTracks.AlbumTracks.GroupBy(x => x.SongName.Substring(0, 1).ToUpper()))
                        {
                            MainGridManager.TrackSectionGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(56) });
                            var cont = new SongGroupLabelContainer(al.Key, al.Count());
                            MainGridManager.TrackSectionGrid.Children.Add(cont);
                            Grid.SetRow(cont, MainGridManager.TrackSectionGrid.RowDefinitions.Count - 1);
                            MainGridManager.TrackSectionGrid.RowDefinitions.Add(new RowDefinition());
                            rowCount++;
                            var itemsGrid = new Grid();
                            itemsGrid.RowDefinitions.Add(new RowDefinition());
                            foreach (var item in al)
                            {

                                if (Global.BackgroundWorkerLibraryLoading == Global.BackgroundCheckout.Abort)
                                    break;

                                if (Global.AppMode == Global.AppModeEnum.Radio)
                                {

                                }
                                else
                                {
                                    //Controls.LibraryCardSongSmall card = new Controls.LibraryCardSongSmall();
                                    //card.Margin = new Thickness(0, 5, 0, 5);
                                    //if (CardmodeBuffer == null || CardmodeBuffer.CardMode == LibraryCardSongSmall.CardModeEnum.RemoveFromPlaylist)
                                    //{
                                    //    card = new Controls.LibraryCardSongSmall(item);
                                    //    CardmodeBuffer = card.GetSetupMode();
                                    //}
                                    //else
                                    //{
                                    //    card = new LibraryCardSongSmall();
                                    //    card.SetupTrack(item, CardmodeBuffer);
                                    //    //card = XamlReader.Parse(XamlWriter.Save(cardTemplate)) as Controls.LibraryCardSong;
                                    //}


                                    ////if (MainGridManager.CardCount == 0)
                                    ////{
                                    ////    // doing this manually because it either gives a NaN value or Zero
                                    ////    MainGridManager.ResetRowGrid((int)card.MinWidth, (int)card.MinHeight);
                                    ////}

                                    //if (playlistbuffer != null)
                                    //{
                                    //    if (playlistbuffer.Contains(item.SongID))
                                    //        card.SetMode(LibraryCardSongSmall.CardModeEnum.RemoveFromPlaylist);
                                    //}

                                    //if (colCount < 4)
                                    //{
                                    //    colCount++;
                                    //    itemsGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
                                    //}

                                    //itemsGrid.Children.Add(card);
                                    //Grid.SetRow(card, itemRowCount);
                                    //Grid.SetColumn(card, pos);
                                    //itemAddedInGrid++;
                                    //pos++;
                                    //if (itemAddedInGrid == 4)
                                    //{
                                    //    itemsGrid.RowDefinitions.Add(new RowDefinition());
                                    //    itemAddedInGrid = 0;
                                    //    itemRowCount++;
                                    //    pos = 0;
                                    //}


                                }
                            }
                            MainGridManager.TrackSectionGrid.Children.Add(itemsGrid);
                            Grid.SetRow(itemsGrid, MainGridManager.TrackSectionGrid.RowDefinitions.Count - 1);
                        }
                        //MainGridManager.UnLoadSectionGrid();
                    }
                    //if(logid!=-1)
                    //LogSystem.AddEvent(logid, "Add Song Cards to Library", true);
                });
            }
            internal void AddNewSongsRowMK2(PlayListSource2 filteredTracks, ref Grid libraryGrid, Library Parent, List<int> playlistbuffer = null, int logid = -1)
            {
                //insert new row
                libraryGrid.RowDefinitions.Insert(libraryGrid.RowDefinitions.Count - 1, new RowDefinition());

                Grid albumGrid = new Grid();
                GridManager gManager = new GridManager(scrollViewLibraryWidth, ref albumGrid);
                albumGrid.Tag = "Tracks";
                LogSystem.AddEvent(logid, "Start Creating Song Cards for Library", true);
                foreach (var al in filteredTracks.AlbumTracks)
                {
                    if (Global.AppMode == Global.AppModeEnum.Radio)
                    {

                    }
                    else
                    {
                        Controls.LibraryCardSong card = new Controls.LibraryCardSong(al);
                        if (gManager.CardCount == 0)
                        {
                            // doing this manually because it either gives a NaN value or Zero
                            gManager.ResetRowGrid((int)card.MinWidth, (int)card.MinHeight);
                            //gMath.ResetRowGrid(600, 50);
                        }

                        if (playlistbuffer != null)
                        {
                            if (playlistbuffer.Contains(al.TrackID)) { }
                            //card.IsButtonActive = true;
                        }

                        card.Margin = gManager.GetCardMargin();
                        gManager.AddCard(ref card);
                    }
                }

                LogSystem.AddEvent(logid, "Add Song Cards to Library", true);
                libraryGrid.Children.Add(albumGrid);
                Grid.SetRow(albumGrid, libraryGrid.RowDefinitions.Count - 2);
            }
            internal void AddNewSongsRow(PlayListSource filteredTracks, ref Grid libraryGrid, Library Parent, List<int> playlistbuffer = null)
            {
                // insert row label
                if (filteredTracks.SourceTracks.Count() > 0)
                    AddLibraryRow("Unsorted Songs (" + filteredTracks.SourceTracks.Count() + ")");
                else
                    AddLibraryRow("This library is empty");
                libraryGrid.RowDefinitions.Add(new RowDefinition());

                //GridMath gMath = new GridMath(scrollViewLibraryWidth);
                Grid albumGrid = new Grid();
                GridManager gManager = new GridManager(scrollViewLibraryWidth, ref albumGrid);
                albumGrid.Tag = "Tracks";
                var grid = new Grid { Background = Brushes.Black, Opacity = 0.71 };
                if (!Global.AdminSettings.IsMusicLibraryScrollHorizontal)
                {
                    grid.Margin = new Thickness(0, 0, 56, 0);
                }
                libraryGrid.Children.Clear();
                libraryGrid.Children.Add(grid);
                var stackpanel = new StackPanel() { Margin = new Thickness(0, 10, 0, 10) };
                using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                {
                    var distinctList = filteredTracks.SourceTracks.GroupBy(s => s.SongName)
                                                 .Select(grp => grp.FirstOrDefault())
                                                 .OrderBy(s => s.SongName)
                                                 .ToList();
                    foreach (var item in distinctList.GroupBy(x => x.SongName.Substring(0, 1).ToUpper()))
                    {
                        var cont = new SongGroupLabelContainer(item.Key, item.Count());
                        if (!Global.AdminSettings.IsMusicLibraryScrollHorizontal)
                        {
                            cont.Margin = new Thickness(0, 5, 56, 0);
                        }

                        stackpanel.Children.Add(cont);
                        var wrapPanel = new UniformGrid { Columns = 4 };
                        if (!Global.AdminSettings.IsMusicLibraryScrollHorizontal)
                        {
                            wrapPanel.Margin = new Thickness(0, 0, 56, 0);
                        }
                        //if (!Global.AdminSettings.IsMusicLibraryScrollHorizontal)
                        //{
                        //    wrapPanel.Margin = new Thickness(40, 0, 0, 0);
                        //}
                        foreach (var al in item)
                        {
                            if (Global.AppMode == Global.AppModeEnum.Radio)
                            {
                                Controls.LibraryCardRadio card = new Controls.LibraryCardRadio(db.SongLibraries.Find(al.SongId).TrackLibraries.First());
                                if (gManager.CardCount == 0)
                                    gManager.ResetRowGrid((int)card.MinWidth, (int)card.MinHeight);

                                card.Margin = gManager.GetCardMargin();
                                gManager.AddCard(ref card);
                            }
                            else if (Global.AppMode == Global.AppModeEnum.Karaoke || Global.AppMode == Global.AppModeEnum.Video)
                            {
                                Controls.KaraokeLibraryCardSongSmall card = new Controls.KaraokeLibraryCardSongSmall(db.SongLibraries.Find(al.SongId).TrackLibraries.First(), Parent);
                                card.Margin = new Thickness(5, 0, 5, 0);
                                wrapPanel.Children.Add(card);
                            }
                            else
                            {
                                //db.SongLibraries.Find(al.SongId)
                                Controls.LibraryCardSongSmall card = new Controls.LibraryCardSongSmall(db.SongLibraries.Find(al.SongId).TrackLibraries.First());
                                wrapPanel.Children.Add(card);
                            }

                        }
                        stackpanel.Children.Add(wrapPanel);
                    }
                }

                libraryGrid.Children.Add(stackpanel);
                Grid.SetRow(stackpanel, 0);
            }
        }

        public class PlayListSource
        {
            public AlbumLibrary SourceAlbumLibrary { get; set; }
            //public List<PlayListDetail> SourcePlaylistDetails { get; set; }
            public PlayListDetail SourcePlaylistDetails { get; set; }
            public List<SongLibrary> SourceTracks { get; set; }
            //public List<SongLibrary> SourceTracks { get; set; }

            public bool isTracksOnly { get { return (SourceAlbumLibrary == null && SourcePlaylistDetails == null); } }
            public bool isAlbumSelected = false;
            public List<Playlist> LoadPlaylist
            {
                set
                {
                    var x = from a in value
                            select a.TrackLibrary;

                    SourceTracks = (from b in x
                                    select b.SongLibraries.First()).ToList();
                }
            }
        }

        public class PlayListSource2
        {
            public AlbumLibrary SourceAlbumLibrary { get; set; }
            //public List<PlayListDetail> SourcePlaylistDetails { get; set; }
            public PlayListDetail SourcePlaylistDetails { get; set; }
            public List<AlbumTrack> AlbumTracks { get; set; }

            public bool isTracksOnly { get { return (SourceAlbumLibrary == null && SourcePlaylistDetails == null); } }
            public bool isAlbumSelected = false;
            public List<Playlist> LoadPlaylist
            {
                set
                {
                    //TODO: Removed Code to make compile, obviously not working with Playlists at this time
                    var x = from a in value
                            select a.TrackLibrary;

                    //SourceTracks = (from b in x
                    //                select b.SongLibraries.First()).ToList();
                }
            }

            public string ArtistName { get; internal set; }

            public class AlbumTrack
            {
                public int TrackID { get; set; }
                public int SongID { get; set; }
                public string SongName { get; set; }
                public string ArtistName { get; set; }
                public bool hasArtist { get { if (string.IsNullOrEmpty(ArtistName)) return false; else return true; } }
            }
        }
        internal class PlayListSource3x
        {

        }
        internal class PlayListSource3
        {
            //public AlbumLibrary SourceAlbumLibrary { get; set; }
            public int? SourceAlbumId { get; set; }
            public int? SourcePlaylistId { get; set; }
            public List<LibraryView> LibraryTracks { get; set; }
            public PlayListDetail SourcePlaylistDetails { get; set; }
            //public bool isTracksOnly { get { return (SourceAlbumLibrary == null && SourcePlaylistDetails == null); } }
            public bool isAlbumSelected = false;
        }
        public class LibraryBackgroundWorker
        {
            // View Track Count
            int batchNumber = 1;
            public void LoadSourceTracks(int skipNumber)
            {
                // Create Batches

                using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                {
                    //var libview = (from a in db.LibraryViews.Where(lv => lv.Type == Global.AppModeString).Skip(skipNumber).Take(350) select a).ToList();
                    //// get last album
                    //int albumID = libview.Last().AlbumId.Value;
                    //// remove last album
                    //int result = libview.RemoveAll(r => r.AlbumId == albumID);


                    //var albums = (from v in libview.Take(350)
                    //               group v by v.AlbumId into g
                    //               select new { album = g.Key, tracks = g }).ToList();
                }

                // Add batch to SourceTracks


            }
        }

        #endregion Classes



        public List<ListBoxItem> getTestLiveDir()
        {
            List<ListBoxItem> items = new List<ListBoxItem>();
            //var d = (from p in db.AppSettings
            //         where p.Type == "LibraryDir"
            //         select p.Value).ToList();
            //foreach (string dir in d)
            //{
            //    foreach (string musicFilePath in Directory.EnumerateFiles(dir, "*", SearchOption.AllDirectories))
            //    {
            //        //System.IO.Path.GetFileName(musicFilePath);

            //        if (Global.AppMode == Global.AppModeEnum.Music)
            //        {
            //            if (Global.musicformats.Contains(System.IO.Path.GetExtension(musicFilePath)))
            //            {
            //                items.Add(new ListBoxItem() { ToolTip = musicFilePath, Content = System.IO.Path.GetFileNameWithoutExtension(musicFilePath), Tag = musicFilePath });
            //            }
            //        }
            //        if (Global.AppMode == Global.AppModeEnum.Karaoke)
            //        {
            //            //if (System.IO.Path.GetExtension(musicFilePath) == ".mp3")
            //            //{
            //            items.Add(new ListBoxItem() { ToolTip = musicFilePath, Content = System.IO.Path.GetFileNameWithoutExtension(musicFilePath), Tag = musicFilePath });
            //            //}
            //        }
            //        if (Global.AppMode == Global.AppModeEnum.Video)
            //        {
            //            if (Global.videoformats.Contains(System.IO.Path.GetExtension(musicFilePath)))
            //            {
            //                items.Add(new ListBoxItem() { ToolTip = musicFilePath, Content = System.IO.Path.GetFileNameWithoutExtension(musicFilePath), Tag = musicFilePath });
            //            }
            //        }
            //        else
            //        {
            //            if (Global.fileformats.Contains(System.IO.Path.GetExtension(musicFilePath)))
            //            {
            //                items.Add(new ListBoxItem() { ToolTip = musicFilePath, Content = System.IO.Path.GetFileNameWithoutExtension(musicFilePath), Tag = musicFilePath });
            //            }
            //        }
            //    }
            //}

            return items;
        }

        public List<ListBoxItem> getLibraryDir()
        {
            List<ListBoxItem> items = new List<ListBoxItem>();
            //if (Global.AppMode == Global.AppModeEnum.Karaoke)
            //{
            //    //if (System.IO.Path.GetExtension(musicFilePath) == ".mp3")
            //    //{
            //    var i = (from x in db.TrackLibraries
            //             where x.Type == "Karaoke" && x.FilePath.Contains(".mp3")
            //             select new ListBoxItem() { ToolTip = x.FilePath, Content = x.FileName, Tag = x.FilePath }).ToList<ListBoxItem>();

            //    items.AddRange(i);
            //    //}
            //}

            return items;
        }

        public ToggleButton GetNewIndexButton(char content)
        {
            //Button t = btnTemplate;

            ToggleButton b = new ToggleButton();
            b.Content = content;
            b.Click += new RoutedEventHandler(btnIndex_Click);

            b.Padding = new Thickness(0, 0, 0, 8);
            b.Margin = new Thickness(2, 0, 2, 0);

            b.Height = 81;
            b.Width = 40;
            b.MinWidth = 40;
            b.FontSize = 18;

            b.Style = FindResource(Global.ControlStyle_ToggleButton) as Style;
            b.IsEnabled = false;

            return b;
        }
        public ToggleButton GetEnabledNewIndexButton(char content)
        {
            //Button t = btnTemplate;

            ToggleButton b = new ToggleButton();
            b.Content = content;
            b.Click += new RoutedEventHandler(btnIndex_Click);

            b.Style = FindResource("EnabledToggleButtonStyle") as Style;
            b.IsEnabled = false;

            return b;
        }

        public ToggleButton GetDisabledNewIndexButton(char content)
        {
            //Button t = btnTemplate;

            ToggleButton b = new ToggleButton();
            b.Content = content;
            b.Click += new RoutedEventHandler(btnIndex_Click);

            b.Style = FindResource("DisabledToggleButtonStyle") as Style;
            b.IsEnabled = false;

            return b;
        }


        /// <summary>
        /// Loads the bottom index, defaulted to the artists
        /// </summary>
        public void LoadIndex()
        {
            Global.MainWindow.btnKeyboard.Visibility = Visibility.Visible;
            Global.MainWindow.sParentPanel.Visibility = Visibility.Visible;
            Global.MainWindow.btnKeyboard.Visibility = Visibility.Collapsed;
            Global.MainWindow.btnMediaPanel.Visibility = Visibility.Visible;
            Global.MainWindow.PlaylistControlPanel.Visibility = Visibility.Collapsed;          


            //Button t = btnTemplate;
            ToggleButton buttHash = GetDisabledNewIndexButton('#');

            List<string> list = new List<string>();
            using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
            {
                try
                {
                    if (db.LibraryViews!=null && db.LibraryViews.Any())
                    {
                        list = db.LibraryViews.Where(w => w.Type == Global.AppModeString).Select(s => s.SongIndex).Distinct().ToList();
                    }

                    Global.MainWindow.sPanel.Children.Clear();
                    for (int number = 0; number <= 9; number++)
                    {
                        if (list.Any(a => a != null && a.Contains(number.ToString())))
                        {
                            buttHash.IsEnabled = true;
                            buttHash.Style = FindResource("EnabledToggleButtonStyle") as Style;
                            break;
                        }
                    }

                    Global.MainWindow.sPanel.Children.Add(buttHash);

                    for (char letter = 'A'; letter <= 'Z'; letter++)
                    {
                        ToggleButton newB;
                        if (list.Any(a => a != null && a.Contains(letter)))
                        {
                            newB = GetEnabledNewIndexButton(letter);
                            newB.IsEnabled = true;
                        }
                        else
                        {
                            newB = GetDisabledNewIndexButton(letter);
                            newB.IsEnabled = false;
                        }


                        Global.MainWindow.sPanel.Children.Add(newB);
                    }

                }
                catch (Exception)
                {

                }

            }


            //for (int number = 0; number <= 9; number++)
            //{
            //    // code for song names
            //    //if (SourceTracks.Any(a => a.SourceTracks.Any(a2 => a2.SongName.ToString().StartsWith(number.ToString(), true, CultureInfo.CurrentCulture))))
            //    //{
            //    //    buttHash.IsEnabled = true;
            //    //    break;
            //    //}

            //    if (SourceTracks.Any(a => a.SourceTracks.Any(a2 => a2.Artists.Any(a3 => a3.ArtistName.StartsWith(number.ToString(), true, CultureInfo.CurrentCulture)))))
            //    {
            //        buttHash.IsEnabled = true;
            //        break;
            //    }
            //}

            ////sPanel.Children.Add(buttHash);

            //for (char letter = 'A'; letter <= 'Z'; letter++)
            //{
            //    //b.Content = letter;
            //    ToggleButton newB = GetNewIndexButton(letter);
            //    //if (SourceTracks.Any(a => a.SourceTracks.Any(a2 => a2.SongName.ToString().StartsWith(letter.ToString(), true, CultureInfo.CurrentCulture))))
            //    //    newB.IsEnabled = true;

            //    if (SourceTracks.Any(a => a.SourceTracks.Any(a2 => a2.Artists.Any(a3 => a3.ArtistName.ToString().StartsWith(letter.ToString(), true, CultureInfo.CurrentCulture)))))
            //        newB.IsEnabled = true;

            //    Global.MainWindow.sPanel.Children.Add(newB);
            //}
        }

        protected List<string> LoadYears()
        {
            // albums have years...
            List<string> Years = new List<string>();
            using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
            {
                var y = db.LibraryViews.Where(w => w.Type == Global.AppModeString).Select(s => s.Year).Distinct().OrderBy(o => o);
                Years = (from x in y where !YearFilters.Contains(x) select x).ToList();
            }

            Years.RemoveAll(r => r == "'s");

            //var a = (from s in SourceTracks.Where(w => w.SourceAlbumLibrary != null).OrderByDescending(o => o.SourceAlbumLibrary.Year)
            //         where s.SourceAlbumLibrary.Year != null
            //         select (Math.Floor((double)(s.SourceAlbumLibrary.Year.GetValueOrDefault() / 10)) * 10).ToString() + "'s").Distinct().ToList();

            //var b = (from c in a
            //         where !YearFilters.Contains(c)
            //         select c).ToList();

            return Years;
        }

        protected List<string> GenreList
        {
            get
            {
                if (_genreList == null)
                    _genreList = LoadGenres();

                return _genreList;
            }
            set { _genreList = value; }
        }
        private List<string> _genreList;
        protected List<string> LoadGenres()
        {
            List<string> g = new List<string>();
            using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
            {
                g = db.LibraryViews.Where(w => w.Type == Global.AppModeString).Select(s => s.Genre).Distinct().OrderBy(o => o).ToList();
            }

            g.RemoveAll(r => string.IsNullOrEmpty(r));
            //var g = (from s in SourceTracks
            //         select (from s2 in s.SourceTracks where s2.Genre != null select s2.Genre).Distinct().ToList()).Distinct().ToList();
            //List<string> g2 = new List<string>();

            //foreach (List<string> x in g)
            //{
            //    g2.AddRange(x);
            //}

            //g2 = g2.Distinct().ToList();

            //int i = g.Count();
            return g;
        }

        public enum LibraryMode
        {
            LastPlayed,
            Favourites,
            JustPlay,
            Playlists,
            NewPlaylist,
            EditPlaylist,
            Library
        }

        internal CardFactory cFactory { get; set; }

        public ToggleButton CreateSideToggleButtons(string content, bool invertColours = false)
        {
            ToggleButton b = new ToggleButton();
            b.Content = content;

            b.Style = FindResource(Global.ControlStyle_ToggleButton) as Style;
            if (invertColours)
            {
                var x = b.Background;
                b.Background = b.BorderBrush;
                b.BorderBrush = x;
            }
            b.FontSize = 18;
            return b;
        }
        public Button CreateSideButtons(string content, bool invertColours = false)
        {
            Button b = new Button();
            b.Content = content;
            //b.Padding = new Thickness(15);
            //b.Height = t.Height;
            //b.Width = t.Width;

            b.Style = FindResource(Global.ControlStyle_Button) as Style;
            if (invertColours)
            {
                var x = b.Background;
                b.Background = b.BorderBrush;
                b.BorderBrush = x;
            }
            b.FontSize = 18;

            return b;
        }

        protected void RemoveFilter(object sender, EventArgs e)
        {
            Button b = (Button)sender;
            switch (b.Tag.ToString())
            {
                case "Unsorted Songs":
                    isUnsortedFilter = false;
                    break;
                case "New Songs":
                    isNewFilter = false;
                    break;
                case "Search":
                    SearchFilters.RemoveAll(r => r.SearchText == b.Content.ToString() && r.SelectedMode == SearchMode.All);
                    break;
                case "Search Artist":
                    SearchFilters.RemoveAll(r => r.SearchText == b.Content.ToString() && r.SelectedMode == SearchMode.Artist);
                    break;
                case "Search Album":
                    SearchFilters.RemoveAll(r => r.SearchText == b.Content.ToString() && r.SelectedMode == SearchMode.Album);
                    break;
                case "Search Song":
                    SearchFilters.RemoveAll(r => r.SearchText == b.Content.ToString() && r.SelectedMode == SearchMode.Song);
                    break;
                case "Genre":
                    GenreFilters.Remove(b.Content.ToString());
                    break;
                case "Year":
                    YearFilters.Remove(b.Content.ToString());

                    if (bufferToggled != null && bufferToggled.Content == "Years")
                    {
                        //sLeftPanel.Children.Clear();

                        foreach (string s in LoadYears())
                        {
                            Button x = CreateSideButtons(s);
                            x.Click += new RoutedEventHandler(FilterYear);
                            //sLeftPanel.Children.Add(x);
                        }
                    }
                    break;
            }

            LoadRightStackPannel();
            if (Global.LibraryFlipFlop)
                SourceTracksDBBatchController();
            else
                ApplyFilters();
        }

        public void FilterYear(object sender, EventArgs e)
        {
            try
            {
                string filter = ((Button)sender).Content.ToString();
                YearFilters.Add(filter);
                LoadRightStackPannel();

                if (Global.LibraryFlipFlop)
                {
                    SourceTracksDBBatchController();
                }
                else
                {
                    ApplyFilters();
                }
            }
            catch (Exception)
            {

                throw;
            }



            // reload pannel
            //sLeftPanel.Children.Clear();

            //foreach (string s in LoadYears())
            //{
            //    Button x = CreateSideButtons(s);
            //    x.Click += new RoutedEventHandler(FilterYear);
            //    //sLeftPanel.Children.Add(x);
            //}
        }


        public void FilterGenre(object sender, EventArgs e)
        {
            try
            {
                string filter = ((Button)sender).Content.ToString();

                //var f = from t in SourceTracks
                //        where t.SourceTracks.Any(an => an.Genre != null && an.Genre.ToUpper() == filter.ToUpper())
                //        select new PlayListSource() { SourceAlbumLibrary = t.SourceAlbumLibrary, SourcePlaylistDetails = t.SourcePlaylistDetails, SourceTracks = t.SourceTracks.Where(s => s.Genre != null && s.Genre.ToUpper() == filter.ToUpper()).ToList() };
                //FilterTracks = f.ToList();
                //BuildCards();

                GenreFilters.Add(filter);
                //SearchFilters.Add(searchText);
                LoadRightStackPannel();
                if (Global.LibraryFlipFlop)
                    SourceTracksDBBatchController();

                else
                    ApplyFilters();
            }
            catch (Exception)
            {

            }
        }

        protected void NextGenre(object sender, EventArgs e)
        {
            string filter = ((Button)sender).Tag.ToString();

            //GenreFilters.Add(filter);
            if (filter == "Next")
                FiltersPage++;
            else
                FiltersPage--;

            LoadGenresFilters();
            //SearchFilters.Add(searchText);
        }

        //LoadUnsortedFilter
        protected void LoadUnsortedFilter(object sender, EventArgs e)
        {
            isUnsortedFilter = true;
            ////sLeftPanel.Children.Clear();

            // no artist, no album
            LoadRightStackPannel();
            SourceTracksDBBatchController();
            //ApplyFilters();
        }

        ToggleButton bufferToggled { get; set; }
        protected void LoadYearsFilter(object sender, EventArgs e)
        {
            if (((ToggleButton)sender).IsChecked.Value)
            {
                bufferToggled = (ToggleButton)sender;
                LoadRightStackPannel();

                //sLeftPanel.Children.Clear();

                foreach (string s in LoadYears())
                {
                    Button x = CreateSideButtons(s);
                    x.Click += new RoutedEventHandler(FilterYear);
                    //sLeftPanel.Children.Add(x);
                }
            }
            else
            {
                bufferToggled = null;
                //sLeftPanel.Children.Clear();
                foreach (var a in bufferElements)
                {
                    //sLeftPanel.Children.Add(a);
                }
            }
        }

        int maxCount = 5;
        int FiltersPage = 0;
        protected void LoadGenresFilter(object sender, EventArgs e)
        {
            if (((ToggleButton)sender).IsChecked.Value)
            {
                bufferToggled = (ToggleButton)sender;
                LoadGenresFilters();
                LoadRightStackPannel();
            }
            else
            {
                bufferToggled = null;
                //sLeftPanel.Children.Clear();
                foreach (var a in bufferElements)
                {
                    //sLeftPanel.Children.Add(a);
                }
            }
        }
        protected void LoadGenresFilters()
        {
            //sLeftPanel.Children.Clear();

            if (FiltersPage > 0)
            {
                Button up = CreateSideButtons(@"/\");
                up.Tag = "Prev";
                up.Click += new RoutedEventHandler(NextGenre);
                up.HorizontalAlignment = HorizontalAlignment.Left;
                var temp = up.Background;
                up.Background = up.BorderBrush;
                up.BorderBrush = temp;
                //sLeftPanel.Children.Add(up);
            }

            int buttonCounter = 1;
            int skipcount = FiltersPage * maxCount;
            // offset by one, because first page loads one extra
            if (FiltersPage > 0) skipcount++;

            foreach (string s in GenreList.Skip(skipcount))
            {
                Button x = CreateSideButtons(s);
                x.Click += new RoutedEventHandler(FilterGenre);
                //sLeftPanel.Children.Add(x);
                buttonCounter++;
                if (FiltersPage == 0 && buttonCounter == maxCount + 2 || FiltersPage > 0 && buttonCounter == maxCount + 1) break;
            }

            if ((GenreList.Count > (FiltersPage * maxCount) + 2) && maxCount < GenreList.Count)
            {
                Button dn = CreateSideButtons(@"\/");
                dn.Tag = "Next";
                dn.Click += new RoutedEventHandler(NextGenre);
                dn.HorizontalAlignment = HorizontalAlignment.Left;
                var temp2 = dn.Background;
                dn.Background = dn.BorderBrush;
                dn.BorderBrush = temp2;
                //sLeftPanel.Children.Add(dn);
            }

        }


        int LogLibraryLoadTime = -1;
        LibraryBackgroundWorker BackgroundWorker { get; set; }
        public Library(ref Frame MainFrame, LibraryMode libraryMode, int currentAlbumIndex)
        {
            InitializeComponent();
            Index = currentAlbumIndex;
            if (SystemParameters.PrimaryScreenWidth >= 1920)
            {
                HorizontalScrollViewContainer.Height = 600;
            }
            else
            {
                HorizontalScrollViewContainer.Height = 500;
            }
            HorizontalScrollViewContainer.Visibility = Visibility.Collapsed;
            VerticalScrollViewContainer.Visibility = Visibility.Collapsed;
            SongsHorizontalScrollViewContainer.Visibility = Visibility.Collapsed;
            SongsVerticalScrollViewContainer.Visibility = Visibility.Collapsed;
            this.AlbumCountLabel = AlbumsCountInfoLabel;
            Global.BackgroundWorkerLibraryLoading = Global.BackgroundCheckout.Abort;
            Global.BackgroundWorkerLibraryLoading = Global.BackgroundCheckout.Idle;
            LogLibraryLoadTime = LogSystem.StartProcess(LogSystem.Processes.Timer, "LibraryLoadingTime");
            Global.LibraryForceViewMode = false;
            mainFrame = MainFrame;
            LibMode = libraryMode;
            if (Global.AdminSettings.IsMusicLibraryScrollHorizontal)
            {
                cFactory = new CardFactory(ref gridLibraryHorizontal, HorizontalScrollViewLibrary.Width, HorizontalScrollViewLibrary.Height);
            }
            else
            {
                cFactory = new CardFactory(ref gridLibraryVertical, VerticalScrollViewLibrary.Width, VerticalScrollViewLibrary.Height);
            }

        }

        public List<UniformGrid> ContentCarousel = new List<UniformGrid>();
        public int MaxCarouselIndex { get; set; }
        public void createCarousel()
        {
            try
            {
                buildMusicLibrary(new JukeboxBrainsDBEntities());
                if (FilterTracks.Any())
                {
                    var albumTracks = FilterTracks.Where(x => !x.isTracksOnly).ToList();
                    int i = 0;
                    bool hasMore = true;
                    while (hasMore)
                    {
                        var uniformGrid = new UniformGrid { Rows = 2, Columns = 6};
                        using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                        {
                            foreach (var al in albumTracks.Skip(12*i).Take(12))
                            {
                                var artists = (from x in db.AlbumLibraries.Find(al.SourceAlbumLibrary.AlbumId).SongLibraries
                                               where x.TrackLibraries.Any(a => a.Type == Global.AppModeString)
                                               select x.Artists.FirstOrDefault()).ToList();
                                string artistname = artists.First().ArtistName;
                                // is selected
                                var isSelected = false;
                                Controls.LibraryCard card = new Controls.LibraryCard(al.SourceAlbumLibrary, al.SourceTracks.ToList(), artistname, this, isSelected);
                                card.Margin = new Thickness(5);

                                uniformGrid.Children.Add(card);
                            }
                        }

                        i++;
                        hasMore = albumTracks.Skip(12 * i).Take(12).ToList().Count > 0;
                        ContentCarousel.Add(uniformGrid);
                    }

                    if (Global.MainWindow.CurrentLibraryInstnace != null)
                    {
                        Global.MainWindow.CurrentLibraryInstnace.ContentCarousel = ContentCarousel;
                        Global.MainWindow.CurrentLibraryInstnace.MaxCarouselIndex = i;
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }
        public void LoadGrid(int index, List<UniformGrid> ContentCarousel)
        {
            try
            {
                if (ContentCarousel.Count > 0)
                {
                    if (!Global.AdminSettings.IsMusicLibraryScrollHorizontal)
                    {
                        Global.MainWindow.CurrentLibraryInstnace.gridLibraryVertical.Children.Clear();
                        Global.MainWindow.CurrentLibraryInstnace.gridLibraryVertical.Children.Add(ContentCarousel[index]);
                        Global.MainWindow.CurrentLibraryInstnace.VerticalScrollViewContainer.Visibility = Visibility.Visible;
                    }
                }
            }
            catch (Exception)
            {

            }
        }

        public void LoadData(LibraryMode libMode)
        {
            try
            {
                if (Global.LibraryFlipFlop)
                {
                    // New Library
                    switch (libMode)
                    {
                        case LibraryMode.Library:
                            if (Global.isAdminOverride)
                                Global.AppControlMode = Global.AppControlModeEnum.Admin;
                            else
                                Global.AppControlMode = Global.AppControlModeEnum.Normal;

                            LogSystem.AddEvent(LogLibraryLoadTime, "Start Library Setup", true);
                            SetupNormalLibrary();
                            LogSystem.AddEvent(LogLibraryLoadTime, "Build Music Library", true);
                            break;
                        case LibraryMode.Playlists:
                            Global.AppControlMode = Global.AppControlModeEnum.Playlist;
                            SetupPlaylists();
                            break;
                    }
                    SourceTracksDBBatchController();
                    LoadIndex();
                }
                else
                {
                    using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                    {
                        switch (libMode)
                        {
                            case LibraryMode.Library:
                                // Check for 'new' tracks

                                // NB! Removed!
                                //var n = (from n1 in db.Playlists where n1.PlaylistId == 3 select n1.TrackId).ToList();
                                //BufferPlaylistTrackIDs = n;
                                //LogSystem.AddEvent(LogLibraryLoadTime, "Start Library Setup", true);
                                //SetupNormalLibrary();
                                LogSystem.AddEvent(LogLibraryLoadTime, "Build Music Library", true);
                                buildMusicLibrary(db);
                                break;
                            case LibraryMode.Playlists:
                                SetupPlaylists();
                                buildPlaylistLibrary(db);
                                break;
                        }

                        // build library items
                        LogSystem.AddEvent(LogLibraryLoadTime, "Build Cards", true);
                        BuildCards();
                        // load index buttons
                        LogSystem.AddEvent(LogLibraryLoadTime, "Load Index", true);
                        this.Dispatcher.Invoke(() => LoadIndex());
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        private UIElementCollection _bufferChildren;
        UIElement[] bufferElements { get; set; }


        private void SetupNormalLibrary()
        {
            //sLeftPanel.Children.Clear();
            if (hasBufferPlaylist)
            {
                Button b = CreateSideButtons("Back");
                b.Click += new RoutedEventHandler(btnBack_Click);
                b.Background = FindResource("ControlGradientMain_Green") as Brush;
                b.BorderBrush = FindResource("ControlGradientMainHighlight_Green") as Brush;

                //sLeftPanel.Children.Add(b);
                //sLeftPanel.Children.Add(CreateSideButtons("Show Library"));
                //sLeftPanel.Children.Add(CreateSideButtons("Show Playlist"));
            }


            using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
            {
                if (db.Playlists.Where(w => w.PlaylistId == Global.NewTracksPlaylistId && w.TrackLibrary.Type == Global.AppModeString).Any())
                {
                    Button c = CreateSideButtons("New Songs");
                    c.Click += new RoutedEventHandler(btnLoadNewTracks);
                    //sLeftPanel.Children.Add(c);
                }
            }


            Button d = CreateSideButtons("Play All");
            d.Click += new RoutedEventHandler(btnPlayAll);
            //sLeftPanel.Children.Add(d);


            if (ActivePlaylistHelper.HasActiveTracks)
            {
                Button e = CreateSideButtons("Show Selected");
                e.Click += new RoutedEventHandler(btnShowSelectedOnly);
                //sLeftPanel.Children.Add(e);
            }



            //bufferElements = new UIElement[sLeftPanel.Children.Count];
            //sLeftPanel.Children.CopyTo(bufferElements, 0);
        }
        private void SetupPlaylistLibraryViewOnly()
        {
            Global.LibraryForceViewMode = true;
            //sLeftPanel.Children.Clear();

            Button b = CreateSideButtons("Back");
            b.Click += new RoutedEventHandler(btnBack_Click);
            //sLeftPanel.Children.Add(b);

            Button d = CreateSideButtons("Play");
            d.Click += btnPlaylistPlay;
            d.Background = FindResource("ControlGradientMain_Green") as Brush;
            d.BorderBrush = FindResource("ControlGradientMainHighlight_Green") as Brush;

            //sLeftPanel.Children.Add(d);

            //bufferElements = new UIElement[sLeftPanel.Children.Count];
            //sLeftPanel.Children.CopyTo(bufferElements, 0);
        }

        private void btnPlaylistPlay(object sender, RoutedEventArgs e)
        {
            int i = Global.AppPlaylistModeBufferID;
            Global.mPlayer.StartUserPlaylist(Global.AppPlaylistModeBufferID, Global.AdminSettings.isAutoPlayListShuffle);
            Global.MainWindow.HideMenu();
        }

        private void SetupPlaylists()
        {
            //sLeftPanel.Children.Clear();
            Button newPlaylist = CreateSideButtons("+ Playlist");
            newPlaylist.Click += new RoutedEventHandler(btnAddNew_Click);
            //sLeftPanel.Children.Add(newPlaylist);
        }
        private void SetupNewPlaylist()
        {
            //sLeftPanel.Children.Clear();

            Button btnSave = CreateSideButtons("Save", true);
            btnSave.Click += new RoutedEventHandler(btnSavePlaylistChanges_Click);
            //sLeftPanel.Children.Add(btnSave);

            Button btnCancel = CreateSideButtons("Cancel");
            btnCancel.Click += new RoutedEventHandler(btnCancelPlaylistChanges_Click);
            //sLeftPanel.Children.Add(btnCancel);

            Button btnAddAll = CreateSideButtons("Add All");
            btnAddAll.Click += BtnAddAll_Click;
            //sLeftPanel.Children.Add(btnAddAll);

            //bufferElements = new UIElement[sLeftPanel.Children.Count];
            //sLeftPanel.Children.CopyTo(bufferElements, 0);
        }

        private void BtnAddAll_Click(object sender, RoutedEventArgs e)
        {
            Global.mPlayer.dbAddTrackToPlaylist(GetTracks(FilterTracks), Global.AppPlaylistModeBufferID);
            RefreshCardItems(FilterTracks);
            ReloadLibrary();
        }


        private void RefreshCardItems(int albumId = -1)
        {
            if (albumId > -1)
            {
                //SourceTracks.Where(w => w.isTracksOnly == false && w.SourceAlbumLibrary.AlbumId == albumId).Select(c => { c.isAlbumSelected = true; return c; }).ToList();
                ActivePlaylistHelper.AlbumIDs.Add(albumId);
            }
            Grid gridLibrary;
            if (Global.AdminSettings.IsMusicLibraryScrollHorizontal)
            {
                gridLibrary = gridLibraryHorizontal;
            }
            else
            {
                gridLibrary = gridLibraryVertical;
            }
            foreach (var grid in gridLibrary.Children.OfType<Grid>())
            {
                if (grid.Tag != null)
                    switch (grid.Tag.ToString())
                    {
                        case "Albums":
                            foreach (var card in grid.Children.OfType<LibraryCard>())
                            {
                                card.SetAppActionMode();

                                if (card.album.AlbumId == albumId)
                                    card.Selected = true;
                            }
                            break;
                        case "Tracks":
                            foreach (var card in grid.Children.OfType<LibraryCardSong>())
                            {
                                card.SetAppActionMode();
                            }
                            break;
                        case "Playlists":
                            break;
                    }
            }
        }

        /// <summary>
        /// Will always only have the filtered songs in the grid
        /// </summary>
        /// <param name="filterTracks"></param>
        /// <param name="refreshAll"></param>
        private void RefreshCardItems(List<PlayListSource> filterTracks, bool refreshAll = false)
        {
            var albumList = (from a in filterTracks where a.isTracksOnly == false select a.SourceAlbumLibrary.AlbumId).ToList();

            //collection.Select(c => {c.PropertyToSet = value; return c;}).ToList();
            SourceTracks.Where(w => w.isTracksOnly == false && albumList.Contains(w.SourceAlbumLibrary.AlbumId)).Select(c => { c.isAlbumSelected = true; return c; }).ToList();
            ActivePlaylistHelper.AlbumIDs.AddRange(albumList);

            Grid gridLibrary;
            if (Global.AdminSettings.IsMusicLibraryScrollHorizontal)
            {
                gridLibrary = gridLibraryHorizontal;
            }
            else
            {
                gridLibrary = gridLibraryVertical;
            }
            foreach (var grid in gridLibrary.Children.OfType<Grid>())
            {
                if (grid.Tag != null)
                    switch (grid.Tag.ToString())
                    {
                        case "Albums":
                            foreach (var card in grid.Children.OfType<LibraryCard>())
                            {
                                card.SetAppActionMode();
                                card.Selected = true;
                            }
                            break;
                        case "Tracks":
                            foreach (var card in grid.Children.OfType<LibraryCardSong>())
                            {
                                //card.IsButtonActive = true;
                                card.SetAppActionMode();
                            }
                            break;
                        case "Playlists":
                            break;
                    }
            }
        }
        private void RefreshCardItems(List<PlayListSource3> filterTracks, bool refreshAll = false)
        {
            var albumList = (from a in filterTracks where a.SourceAlbumId.HasValue select a.SourceAlbumId.Value).ToList();

            //collection.Select(c => {c.PropertyToSet = value; return c;}).ToList();
            //SourceTracks3.Where(w => w.SourceAlbumId.HasValue && albumList.Contains(w.SourceAlbumId.Value)).Select(c => { c.isAlbumSelected = true; return c; }).ToList();
            ActivePlaylistHelper.AlbumIDs.AddRange(albumList);

            Grid gridLibrary;
            if (Global.AdminSettings.IsMusicLibraryScrollHorizontal)
            {
                gridLibrary = gridLibraryHorizontal;
            }
            else
            {
                gridLibrary = gridLibraryVertical;
            }
            foreach (var grid in gridLibrary.Children.OfType<Grid>())
            {
                if (grid.Tag != null)
                    switch (grid.Tag.ToString())
                    {
                        case "Albums":
                            foreach (var card in grid.Children.OfType<LibraryCard>())
                            {
                                card.SetAppActionMode();
                                card.Selected = true;
                            }
                            break;
                        case "Tracks":
                            foreach (var card in grid.Children.OfType<LibraryCardSong>())
                            {
                                //card.IsButtonActive = true;
                                card.SetAppActionMode();
                            }
                            break;
                        case "Playlists":
                            break;
                    }
            }
        }
        private void LoadRightStackPannel()
        {
            //sRightPanel.Children.Clear();
            if (isUnsortedFilter)
            {
                Button f = CreateSideButtons("Unsorted Songs");
                f.Height = 60;
                var x = f.Background;
                f.Background = f.BorderBrush;
                f.BorderBrush = x;

                f.Click += new RoutedEventHandler(RemoveFilter);
                f.Tag = "Unsorted Songs";
                //sRightPanel.Children.Add(f);
            }

            if (isNewFilter)
            {
                Button f = CreateSideButtons("New Songs");
                f.Height = 60;
                var x = f.Background;
                f.Background = f.BorderBrush;
                f.BorderBrush = x;

                f.Click += new RoutedEventHandler(RemoveFilter);
                f.Tag = "New Songs";
                //sRightPanel.Children.Add(f);
            }

            foreach (SearchFilterClass s in SearchFilters)
            {
                Button f = CreateSideButtons(s.SearchText);
                f.Height = 60;
                var x = f.Background;
                f.Background = f.BorderBrush;
                f.BorderBrush = x;

                f.Click += new RoutedEventHandler(RemoveFilter);
                switch (s.SelectedMode)
                {
                    case SearchMode.All:
                        f.Tag = "Search";
                        break;
                    case SearchMode.Artist:
                        f.Tag = "Search Artist";
                        break;
                    case SearchMode.Album:
                        f.Tag = "Search Album";
                        break;
                    case SearchMode.Song:
                        f.Tag = "Search Song";
                        break;
                }
                //sRightPanel.Children.Add(f);
            }

            foreach (string s in GenreFilters)
            {
                Button f = CreateSideButtons(s);
                f.Height = 60;
                var x = f.Background;
                f.Background = f.BorderBrush;
                f.BorderBrush = x;

                f.Click += new RoutedEventHandler(RemoveFilter);
                f.Tag = "Genre";
                //sRightPanel.Children.Add(f);
            }

            foreach (string s in YearFilters)
            {
                Button f = CreateSideButtons(s);
                f.Height = 60;
                var x = f.Background;
                f.Background = f.BorderBrush;
                f.BorderBrush = x;

                f.Click += new RoutedEventHandler(RemoveFilter);
                f.Tag = "Year";
                //sRightPanel.Children.Add(f);
            }

            //ToggleButton g = CreateSideToggleButtons("Genres");
            ////Button g = CreateSideButtons("Genres");
            //g.Click += new RoutedEventHandler(LoadGenresFilter);
            //if (bufferToggled != null && g.Content == bufferToggled.Content) g.IsChecked = true;
            ////sRightPanel.Children.Add(g);

            //ToggleButton y = CreateSideToggleButtons("Years");
            ////Button y = CreateSideButtons("Years");
            //y.Click += new RoutedEventHandler(LoadYearsFilter);
            //if (bufferToggled != null && y.Content == bufferToggled.Content) y.IsChecked = true;
            ////sRightPanel.Children.Add(y);

            // if Admin Mode
            if (Global.AppControlMode == Global.AppControlModeEnum.Admin)
            {
                Button u = CreateSideButtons("Unsorted");
                u.Click += new RoutedEventHandler(LoadUnsortedFilter);
                //sRightPanel.Children.Add(u);
            }
        }



        public void ReloadLibrary(LibraryMode libraryMode)
        {
            LibMode = libraryMode;
            ReloadLibrary(true);
        }
        public void ReloadLibrary(bool isDumpCache)
        {
            if (isDumpCache)
                SourceTracks3 = null;
            ReloadLibrary();
        }
        public void ReloadLibrary()
        {
            Global.LibraryForceViewMode = false;
            ActivePlaylistHelper = new PlayListHelper();
            if (Global.AdminSettings.IsMusicLibraryScrollHorizontal)
            {
                cFactory = new CardFactory(ref gridLibraryHorizontal, HorizontalScrollViewLibrary.Width, HorizontalScrollViewLibrary.Height);
            }
            else
            {
                cFactory = new CardFactory(ref gridLibraryVertical, VerticalScrollViewLibrary.Width, VerticalScrollViewLibrary.Height);
            }

            if (Global.LibraryFlipFlop)
            {
                switch (LibMode)
                {
                    case LibraryMode.Library:
                        LogSystem.AddEvent(LogLibraryLoadTime, "Start Library Setup", true);

                        if (Global.AppControlMode == Global.AppControlModeEnum.Playlist)
                            SetupPlaylistLibraryViewOnly();
                        else
                            SetupNormalLibrary();
                        LogSystem.AddEvent(LogLibraryLoadTime, "Build Music Library", true);
                        break;
                    case LibraryMode.Playlists:
                        SetupPlaylists();
                        break;
                    case LibraryMode.NewPlaylist:
                        SetupNewPlaylist();
                        using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                        {
                            var s = db.LibraryViews.Where(w => BufferPlaylistTrackIDs.Contains(w.TrackId)).Select(ss => ss.SongId.Value);
                            ActivePlaylistHelper.SongIDs = s.ToList();
                        }
                        break;
                    case LibraryMode.EditPlaylist:
                        SetupNewPlaylist();
                        using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                        {
                            var s = db.LibraryViews.Where(w => BufferPlaylistTrackIDs.Contains(w.TrackId)).Select(ss => ss.SongId.Value);
                            ActivePlaylistHelper.SongIDs = s.ToList();
                        }
                        break;
                }
                SourceTracksDBBatchController();
                LoadIndex();
            }
            else
            {
                using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                {
                    // if playlist not empty.. show only playlist entries
                    switch (LibMode)
                    {
                        case LibraryMode.Library:

                            if (Global.AppControlMode == Global.AppControlModeEnum.Playlist)
                                SetupPlaylistLibraryViewOnly();
                            else
                                SetupNormalLibrary();

                            // Show All
                            // Back
                            buildMusicLibrary(db);
                            break;
                        case LibraryMode.NewPlaylist:
                            SetupNewPlaylist();
                            buildMusicLibrary(db);
                            break;
                        case LibraryMode.EditPlaylist:
                            SetupNewPlaylist();
                            buildMusicLibrary(db);
                            break;
                        case LibraryMode.Playlists:
                            SetupPlaylists();
                            buildPlaylistLibrary(db);
                            break;
                    }

                    // build library items
                    BuildCards();

                    // load index buttons
                    LoadIndex();
                }
            }

        }

        public bool isShowOnlyPlaylist = false;

        private void buildMusicLibrary(JukeboxBrainsDBEntities db)
        {
            if (Global.LibraryFlipFlop)
                //buildMusicLibraryMk2(db);
                SourceTracksDBBatchController();
            else
                buildMusicLibraryMk1(db);
        }

        private void buildMusicLibraryMk2(JukeboxBrainsDBEntities db)
        {

            // refresh view
            if (Global.ViewLoaded == false)
            {
                db.Database.ExecuteSqlCommand("EXECUTE sp_refreshview @viewname = 'LibraryView'");
                Global.ViewLoaded = true;
            }

            var libView = db.LibraryViews.Where(lv => lv.Type == Global.AppModeString).ToList();

            var albums2 = (from v in libView.Take(350)
                           group v by v.AlbumId into g
                           select new { album = g.Key, tracks = g }).ToList();


            if (!isShowOnlyPlaylist || (!Global.mPlayer.isIdle && Global.LibraryForceViewMode))
            {
                ActivePlaylistHelper.SongIDs = new List<int>();
                var xxx = (from a in albums2
                           where a.tracks.Any(c => ActivePlaylistHelper.SongIDs.Contains((int)c.SongId))
                           select (int)a.album).ToList();
                ActivePlaylistHelper.AlbumIDs = xxx;
            }

            PreloaderTracks = (from a in albums2
                               select new PlayListSource2() { SourceAlbumLibrary = db.AlbumLibraries.Find(a.album ?? -1), AlbumTracks = a.tracks.Select(s => new PlayListSource2.AlbumTrack() { SongID = (int)s.SongId, TrackID = s.TrackId, SongName = s.SongName }).ToList() }).ToList();

            SourceTracks = new List<PlayListSource>();
            FilterTracks = SourceTracks;

            BackgroundWorker.LoadSourceTracks(2);
        }

        private void buildMusicLibraryMk1(JukeboxBrainsDBEntities db)
        {
            List<SongLibrary> songs = new List<SongLibrary>();

            if (isShowOnlyPlaylist)
            {
                int i = Global.AppPlaylistModeBufferID;

                songs = (from s in db.Playlists
                         where s.PlaylistId == Global.AppPlaylistModeBufferID
                         select s.TrackLibrary.SongLibraries.FirstOrDefault()).ToList();

                if (!Global.mPlayer.isIdle && Global.LibraryForceViewMode)
                {
                    // busy playing songs
                    ActivePlaylistHelper.SongIDs = (from s in db.Playlists
                                                    where s.PlaylistId == Global.NowPlayingId
                                                    select s.TrackLibrary.SongLibraries.FirstOrDefault().SongId).ToList();
                }

            }
            else
            {

                // Add album tracks
                // get tracks               
                songs = db.SongLibraries.Where(x => x.TrackLibraries.Any(c => c.Type.Contains(Global.AppModeString))).ToList();

                if (Global.AppControlMode == Global.AppControlModeEnum.Playlist)
                {
                    ActivePlaylistHelper.SongIDs = (from s in db.Playlists
                                                    where s.PlaylistId == Global.AppPlaylistModeBufferID
                                                    select s.TrackLibrary.SongLibraries.FirstOrDefault().SongId).ToList();
                }
                else if (Global.AppActionMode == Global.AppActionModeEnum.Playing)
                {
                    ActivePlaylistHelper.SongIDs = (from s in db.Playlists
                                                    where s.PlaylistId == Global.NowPlayingId
                                                    select s.TrackLibrary.SongLibraries.FirstOrDefault().SongId).ToList();
                }
                else
                    ActivePlaylistHelper.SongIDs = new List<int>();

            }

            var songClean = (from st2 in songs
                             where st2.AlbumLibraries.Any() && st2.AlbumLibraries.All(w => w.isHidden == true) == false
                             select st2);

            // Get Albums...

            var albums = (from st2 in songClean
                          where st2.AlbumLibraries.Any()
                          group st2 by st2.AlbumLibraries.Where(w => w.isHidden == false).First() into g
                          select new { album = g.Key, tracks = g }).ToList().Distinct().ToList();


            if (!isShowOnlyPlaylist || (!Global.mPlayer.isIdle && Global.LibraryForceViewMode))
            {
                var xxx = (from a in albums
                           where a.tracks.Any(c => ActivePlaylistHelper.SongIDs.Contains(c.SongId))
                           select a.album.AlbumId).ToList();
                ActivePlaylistHelper.AlbumIDs = xxx;
            }

            // add album tracks
            SourceTracks = (from a in albums
                            select new PlayListSource() { SourceAlbumLibrary = a.album, SourceTracks = a.tracks.ToList(), isAlbumSelected = a.album == null ? false : ActivePlaylistHelper.AlbumIDs.Contains(a.album.AlbumId) }).ToList();



            PlayListSource nonAlbums = new PlayListSource();
            nonAlbums.SourceTracks = (from a2 in songs
                                      where a2.AlbumLibraries.Count == 0
                                      select a2).ToList();

            bool isworking = nonAlbums.isTracksOnly;
            int beforecount = SourceTracks.Count();
            //SourceTracks = SourceTracks.Append(nonAlbums);
            SourceTracks.Add(nonAlbums);
            int aftercount = SourceTracks.Count();
            // add unsorted tracks too?




            // Add unsorted tracks ?

            FilterTracks = SourceTracks.ToList();
        }


        private void buildPlaylistLibrary(JukeboxBrainsDBEntities db)
        {
            var playlist = db.Playlists.ToList();
            var rawplaylists = (from pl in db.Playlists
                                where pl.TrackLibrary.Type == Global.AppModeString
                                select pl).ToList();

            var plists = (from pl2 in rawplaylists
                          group pl2.TrackLibrary.SongLibraries.First() by pl2.PlayListDetail into g
                          select new { playlist = g.Key, tracks = g }).ToList().Distinct().ToList();

            SourceTracks = (from a in plists
                            select new PlayListSource() { SourcePlaylistDetails = a.playlist, SourceTracks = a.tracks.ToList() }).ToList();

            //Broken!
            //        var results = s.GroupBy(
            //p => p.PlayListDetail,
            //(key, g) => new PlayListSource { SourcePlaylistDetails = key, SourceTracks = g.First()..ToList() });


            // results.First().Tracks.First().TrackLibrary.FileName

            // SourceTracks 
            FilterTracks = SourceTracks.ToList();
        }

        //private void BuildCards()
        //{
        //    ThreadPool.QueueUserWorkItem(_ => BuildCardsAsync());
        //}
        private void BuildCards()
        {

            switch (LibMode)
            {
                case LibraryMode.Library:

                    if (Global.AppModeString == "Karaoke" || Global.AppModeString == "Video" || Global.AppModeString == "Radio")
                    {
                        Global.MainWindow.LibraryButtonControlPanel.Visibility = Visibility.Collapsed;
                        BuildSongCards();
                    }
                    else
                    {
                        Global.MainWindow.LibraryButtonControlPanel.Visibility = Visibility.Collapsed;
                        BuildAlbumCards();
                    }

                    break;
                case LibraryMode.Playlists:
                    BuildPlaylistCards();
                    break;
                case LibraryMode.NewPlaylist:
                    BuildAlbumCards();
                    //BuildSongCards();
                    break;
                case LibraryMode.EditPlaylist:
                    BuildAlbumCards();
                    //BuildSongCards();
                    break;
            }
        }
        private void BuildSongCards()
        {

            if (Global.LibraryFlipFlop)
            {

            }

            // there can only be one
            int i = FilterTracks.Count();
            using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
            {
                if (Global.LibraryFlipFlop)
                {
                    var songTracksTemp = from ab in SourceTracks2 where ab.isTracksOnly == true select ab;
                    if (songTracksTemp.Any())
                    {
                        VerticalScrollViewContainer.Visibility = Visibility.Collapsed;
                        HorizontalScrollViewContainer.Visibility = Visibility.Collapsed;
                        var songTracksMK2 = songTracksTemp.First();
                        //if (Global.AdminSettings.IsMusicLibraryScrollHorizontal)
                        //{
                        //    SongsHorizontalScrollViewContainer.Visibility = Visibility.Visible;
                        //    SongsGridLibraryHorizontal.Visibility = Visibility.Visible;
                        //    SongsVerticalScrollViewContainer.Visibility = Visibility.Collapsed;
                        //    SongsGridLibraryVertical.Visibility = Visibility.Collapsed;
                        //    cFactory.AddNewSongsRowMK2(songTracksMK2, ref SongsGridLibraryHorizontal, this, null, LogLibraryLoadTime);
                        //}
                        //else
                        //{
                        //    SongsVerticalScrollViewContainer.Visibility = Visibility.Visible;
                        //    SongsGridLibraryVertical.Visibility = Visibility.Visible;
                        //    SongsHorizontalScrollViewContainer.Visibility = Visibility.Collapsed;
                        //    SongsGridLibraryHorizontal.Visibility = Visibility.Collapsed;
                        //    cFactory.AddNewSongsRowMK2(songTracksMK2, ref SongsGridLibraryVertical, this, null, LogLibraryLoadTime);
                        //}
                        SongsVerticalScrollViewContainer.Visibility = Visibility.Visible;
                        SongsGridLibraryVertical.Visibility = Visibility.Visible;
                        SongsHorizontalScrollViewContainer.Visibility = Visibility.Collapsed;
                        SongsGridLibraryHorizontal.Visibility = Visibility.Collapsed;
                        cFactory.AddNewSongsRowMK2(songTracksMK2, ref SongsGridLibraryVertical, this, null, LogLibraryLoadTime);
                    }
                }
                else
                {
                    var songTracks = (from ab in FilterTracks
                                      where ab.isTracksOnly == true && ab.SourceTracks.Count() > 0 && ab.SourceTracks.Count(x => x.AlbumLibraries == null || x.AlbumLibraries.Count == 0) > 0
                                      select ab);

                    if (songTracks.Any())
                    {
                        HorizontalScrollViewContainer.Visibility = Visibility.Collapsed;
                        VerticalScrollViewContainer.Visibility = Visibility.Collapsed;
                        AlbumsCountInfoLabel.Content = $"Songs ({songTracks.First().SourceTracks.Count})";
                        //if (Global.AppMode == Global.AppModeEnum.Karaoke || Global.AppMode == Global.AppModeEnum.Video)
                        //{
                        //    BottomButtonControlPanel.Visibility = Visibility.Collapsed;
                        //}
                        //else
                        //{
                        //    BottomButtonControlPanel.Visibility = Visibility.Visible;
                        //}

                        //if (Global.AdminSettings.IsMusicLibraryScrollHorizontal)
                        //{
                        //    SongsHorizontalScrollViewContainer.Visibility = Visibility.Visible;
                        //    SongsGridLibraryHorizontal.Visibility = Visibility.Visible;
                        //    SongsVerticalScrollViewContainer.Visibility = Visibility.Collapsed;
                        //    SongsGridLibraryVertical.Visibility = Visibility.Collapsed;
                        //    cFactory.AddNewSongsRow(songTracks.First(), ref SongsGridLibraryHorizontal, this);
                        //}
                        //else
                        //{
                        //    SongsHorizontalScrollViewContainer.Visibility = Visibility.Collapsed;
                        //    SongsGridLibraryHorizontal.Visibility = Visibility.Collapsed;
                        //    SongsVerticalScrollViewContainer.Visibility = Visibility.Visible;
                        //    SongsGridLibraryVertical.Visibility = Visibility.Visible;
                        //    cFactory.AddNewSongsRow(songTracks.First(), ref SongsGridLibraryVertical, this);
                        //}
                        SongsHorizontalScrollViewContainer.Visibility = Visibility.Collapsed;
                        SongsGridLibraryHorizontal.Visibility = Visibility.Collapsed;
                        SongsVerticalScrollViewContainer.Visibility = Visibility.Visible;
                        SongsGridLibraryVertical.Visibility = Visibility.Visible;
                        cFactory.AddNewSongsRow(songTracks.First(), ref SongsGridLibraryVertical, this);
                    }

                }
            }
        }

        public int totalCardCount { get; set; }
        public Dictionary<int, string> CountStack = new Dictionary<int, string>();
        public Dictionary<int, UniformGrid> LIbraryStacks = new Dictionary<int, UniformGrid>();
        private int MaxCardCount = 10;
        private void BuildAlbumCards()
        {
            if (Global.LibraryFlipFlop)
            {
                var albumTracksMK2 = (from ab in SourceTracks2
                                      where ab.isTracksOnly == false
                                      select ab);

                //cFactory.AddNewAlbumRowMK2(albumTracksMK2, ref gridLibrary, this, ActivePlaylistHelper.AlbumIDs, LogLibraryLoadTime);
            }
            else
            {

                var albumTracks = (from ab in FilterTracks
                                   where ab.isTracksOnly == false
                                   select ab);
                if (albumTracks.Count() > 0)
                {
                    SongsVerticalScrollViewContainer.Visibility = Visibility.Collapsed;
                    HorizontalScrollViewContainer.Visibility = Visibility.Collapsed;
                    var f = albumTracks.ToList();
                    totalCardCount = f.Count;
                    MaxIndex = (totalCardCount / MaxHorzCards);
                    var totalPage = MaxIndex + 1;
                    for (int i = 0; i < totalPage; i++)
                    {
                        if ((MaxHorzCards * (i + 1)) > f.Count)
                        {
                            CountStack.Add(i, $"Albums ({(MaxHorzCards * i) + 1}-{f.Count} of {f.Count})");
                        }
                        else
                        {
                            CountStack.Add(i, $"Albums ({(MaxHorzCards * i) + 1}-{MaxHorzCards * (i + 1)} of {f.Count})");
                        }

                    }

                    if (f.Count > MaxHorzCards)
                    {
                        AlbumsCountInfoLabel.Content = CountStack[0];
                    }
                    else
                    {
                        AlbumsCountInfoLabel.Content = $"Albums (1-{f.Count} of {f.Count})";
                    }

                    if (Global.AdminSettings.IsMusicLibraryScrollHorizontal)
                    {
                        VerticalScrollViewContainer.Visibility = Visibility.Collapsed;
                        HorizontalScrollViewContainer.Visibility = Visibility.Visible;
                        cFactory.AddNewAlbumRow(albumTracks.Take(MaxHorzCards), ref gridLibraryHorizontal, this, ActivePlaylistHelper.AlbumIDs);
                    }
                    else
                    {
                        VerticalScrollViewContainer.Visibility = Visibility.Visible;
                        HorizontalScrollViewContainer.Visibility = Visibility.Collapsed;                        
                        cFactory.AddNewAlbumRow(albumTracks.Take(MaxHorzCards), ref gridLibraryVertical, this, ActivePlaylistHelper.AlbumIDs);
                    }

                    //SetPreviousCardsPanel();
                    SetNextCardsPanel();
                }

            }
        }

        private void BuildPlaylistCards()
        {
            // Playlist Tracks
            if (Global.AdminSettings.IsMusicLibraryScrollHorizontal)
            {
                HorizontalScrollViewContainer.Visibility = Visibility.Visible;
                cFactory.AddNewPlaylistRow(FilterTracks, ref gridLibraryHorizontal, this);
            }
            else
            {
                cFactory.AddNewAlbumRow(FilterTracks, ref gridLibraryVertical, this,null);
            }


            //foreach (var al in FilterTracks)
            //{
            //    top += 20;
            //    Controls.LibraryPlaylist card = new Controls.LibraryPlaylist(al.SourcePlaylistDetails, this);

            //    //card.Margin = GetCardPosition();
            //    gridLibrary.Children.Add(card);

            //    Grid.SetRow(card, gridLibrary.RowDefinitions.Count - 2);
            //}
        }

        private void GetCardPosition(Button btn1)
        {

        }

        int CardPositionCount = 1;
        int cardsize = 200;
        int cardMargin = 10;
        int cardRes = 0;
        int cardCountX = 0;
        int cardIndexY = 0;

        internal void PlayNow()
        {
            // None Selected, Play Random
            //ListBoxItem lbi = (ListBoxItem)lbTrackList.SelectedItem;
            //string path = lbi.Tag.ToString();

            //mainFrame.Content = null;
        }

        #region Redundant


        //private void Button_Click(object sender, RoutedEventArgs e)
        //{
        //    string path = "D:\\Movies\\2CELLOS - Seven Nation Army.mp4";
        //    Global.mPlayer.PlayNow(path);
        //    mainFrame.Content = null;
        //}

        private void btnPlayNow_Click(object sender, RoutedEventArgs e)
        {
            // None Selected, Play Random
            //ListBoxItem lbi = (ListBoxItem)lbTrackList.SelectedItem;
            //string path = lbi.Tag.ToString();
            //Global.mPlayer.PlayNow(path);
            //mainFrame.Content = null;
        }

        private void btnPlayNext_Click(object sender, RoutedEventArgs e)
        {
            //ListBoxItem lbi = (ListBoxItem)lbTrackList.SelectedItem;
            //string path = lbi.Tag.ToString();
            //Global.mPlayer.CueNext(path);
            //// Note Added

            //refreshControls();
        }

        private void btnQue_Click(object sender, RoutedEventArgs e)
        {
            //ListBoxItem lbi = (ListBoxItem)lbTrackList.SelectedItem;
            //string path = lbi.Tag.ToString();
            //Global.mPlayer.AddToPlaylist(path);
            //// Note Added

            //refreshControls();
        }

        private void btnNav_NowPlaying_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnNav_History_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnNav_Top10_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnSpotify_Click(object sender, RoutedEventArgs e)
        {

        }


        #endregion Redundant



        private void refreshControls()
        {
            //lbHistory.ItemsSource = Global.mPlayer.getHistoryItems();
            //lbPlaylist.ItemsSource = Global.mPlayer.getNowPlayingItems();
        }

        ToggleButton btnToggleBuffer { get; set; }
        string btnToggle = "";
        private bool _loadFilterIndex = false;
        private void FilterIndex()
        {
            var f = from t in SourceTracks3
                    where t.LibraryTracks.Any(an => an.SongIndex == btnToggle)
                    select new PlayListSource3()
                    {
                        SourceAlbumId = t.SourceAlbumId,
                        SourcePlaylistDetails = t.SourcePlaylistDetails,
                        SourcePlaylistId = t.SourcePlaylistId,
                        LibraryTracks = t.LibraryTracks.Where(s => s.SongIndex == btnToggle).ToList()
                    };
            FilterTracks3 = f.ToList();
            //_loadFilterIndex = false;
        }
        private void btnIndex_Click(object sender, RoutedEventArgs e)
        {
            if (Global.BackgroundWorkerLibraryLoading != Global.BackgroundCheckout.Abort)
            {
                string a = ((ToggleButton)sender).Content.ToString();
                if (btnToggle == a)
                {
                    if (!Global.LibraryFlipFlop)
                        FilterTracks = SourceTracks.ToList();
                    btnToggle = "";
                    btnToggleBuffer = null;
                    if (Global.LibraryFlipFlop)
                    {
                        _loadFilterIndex = false;
                        FilterTracks3 = null;
                        SourceTracksDBBatchController();
                    }
                }
                else if (Global.LibraryFlipFlop)
                {
                    if (btnToggle != "" && btnToggleBuffer != null)
                    {
                        btnToggleBuffer.IsChecked = false;
                    }
                    btnToggle = a;
                    btnToggleBuffer = (ToggleButton)sender;
                    _loadFilterIndex = true;

                    SourceTracksDBBatchController();
                }
                else
                {
                    if (btnToggle != "" && btnToggleBuffer != null)
                    {
                        btnToggleBuffer.IsChecked = false;
                    }

                    var f = from t in SourceTracks
                            where t.SourceTracks.Any(an => an.SongName.ToUpper().StartsWith(a))
                            select new PlayListSource() { SourceAlbumLibrary = t.SourceAlbumLibrary, SourcePlaylistDetails = t.SourcePlaylistDetails, SourceTracks = t.SourceTracks.Where(s => s.SongName.ToUpper().StartsWith(a)).ToList() };
                    //var sourceTracks = SourceTracks.AsEnumerable();
                    //foreach (var item in sourceTracks)
                    //{
                    //    item.SourceTracks = item.SourceTracks.ToList();
                    //    foreach (var st in item.SourceTracks)
                    //    {
                    //        if (st.Artists.Any())
                    //        {
                    //            st.Artists = st.Artists.ToList();
                    //        }
                    //    }
                    //}
                    //var f = from t in sourceTracks
                    //        where t.SourceTracks.Any(an => an.Artists.Any(an2 => an2.ArtistName.ToUpper().StartsWith(a)))
                    //        select new PlayListSource()
                    //        {
                    //            SourceAlbumLibrary = t.SourceAlbumLibrary,
                    //            SourcePlaylistDetails = t.SourcePlaylistDetails,
                    //            SourceTracks = t.SourceTracks.Where(s => s.Artists.Any(s2 => s2.ArtistName.ToUpper().StartsWith(a))).ToList()
                    //        };

                    FilterTracks = f.ToList();
                    btnToggle = a;
                    btnToggleBuffer = (ToggleButton)sender;

                    BuildCards();

                }
            }
        }





        internal enum SearchMode
        {
            All,
            Artist,
            Album,
            Song
        }
        internal void SetSearchFilter(List<string> searchText, SearchMode mode)
        {
            foreach (string s in searchText)
            {
                SearchFilters.Add(new SearchFilterClass { SearchText = s, SelectedMode = mode });
            }

            LoadRightStackPannel();
            SourceTracksDBBatchController();
        }
        internal void SetSearchFilter(string searchText, SearchMode mode)
        {
            SearchFilters.Add(new SearchFilterClass { SearchText = searchText, SelectedMode = mode });
            LoadRightStackPannel();

            SourceTracksDBBatchController();
            //ApplyFilters3();
            //ApplyFilters();
        }

        internal void SearchbySearchFilter(string searchText, SearchMode mode)
        {
            try
            {
                SearchFilters.Add(new SearchFilterClass { SearchText = searchText, SelectedMode = mode });
                SourceTracksDBBatchController();
            }
            catch (Exception)
            {

            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            // lock all keyboard controls
            Global.isKeyboardFocus = true;
            Global.mPlayer.PopUp_Search();

            // show search thingy, focus control
            //btnSearchGo.Visibility = Visibility.Visible;
            //tbSearchText.Visibility = Visibility.Visible;

            //tbSearchText.Focus();
        }

        private void ApplyFilters3(bool isIndexFiltered)
        {
            if (!isIndexFiltered)
                FilterTracks3 = SourceTracks3;

            if (isUnsortedFilter)
            {
                // no artist, no album
                //var songlist = db.SongLibraries.Where(w => !w.Artists.Any() && !w.AlbumLibraries.Any()).Select(s => s.SongId).ToList();

                var x = FilterTracks3.Where(w2 => w2.SourceAlbumId == null).First();
                var y = x.LibraryTracks.Where(t => (t.Artists == null || t.Artists == "") && t.AlbumId == null).ToList();
                x.LibraryTracks = y;

                FilterTracks3 = new List<PlayListSource3>() { x };
            }

            if (isNewFilter)
            {
                using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                {
                    //var playSongs = (from a in db.Playlists
                    //                 where a.PlaylistId == Global.NewTracksPlaylistId && a.TrackLibrary.Type == Global.AppModeString
                    //                 select a.TrackLibrary.SongLibraries.FirstOrDefault().SongId).ToList();

                    var playSongs = db.Playlists.Where(a => a.PlaylistId == Global.NewTracksPlaylistId).Select(a2 => a2.TrackId).ToList();

                    var f = from t in FilterTracks3
                            where t.LibraryTracks.Any(an => playSongs.Contains(an.TrackId))
                            select new PlayListSource3() { SourceAlbumId = t.SourceAlbumId, SourcePlaylistDetails = t.SourcePlaylistDetails, LibraryTracks = t.LibraryTracks.Where(s => playSongs.Contains(s.TrackId)).ToList() };

                    FilterTracks3 = f.ToList();
                }
            }

            if (SearchFilters.Any())
            {
                List<string> lowerArtistFilters = (from fx in SearchFilters where fx.SelectedMode == SearchMode.Artist || fx.SelectedMode == SearchMode.All select fx.SearchText.ToLower()).ToList();
                List<string> lowerAlbumFilters = (from fx in SearchFilters where fx.SelectedMode == SearchMode.Album || fx.SelectedMode == SearchMode.All select fx.SearchText.ToLower()).ToList();
                List<string> lowerSongFilters = (from fx in SearchFilters where fx.SelectedMode == SearchMode.Song || fx.SelectedMode == SearchMode.All select fx.SearchText.ToLower()).ToList();

                //   string low = sFilter.ToLower();
                var list = new List<PlayListSource3>();
                if (FilterTracks3 != null && FilterTracks3.Any())
                {
                    list = (from t in FilterTracks3
                            where t.LibraryTracks.Any(an => lowerSongFilters.Any(an.SongName.ToLower().Contains))
                            select new PlayListSource3() { SourceAlbumId = t.SourceAlbumId, SourcePlaylistDetails = t.SourcePlaylistDetails, LibraryTracks = t.LibraryTracks.Where(s => lowerSongFilters.Any(s.SongName.ToLower().Contains)).ToList() }).ToList();
                }




                //var a1 = FilterTracks.SelectMany(s => s.SourceTracks.SelectMany(s1 => s1.Artists.Any()));
                using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                {
                    // Artist
                    var artistNameList = (from a in db.Artists select new { a.ArtistName, a }).Distinct().ToList();
                    var artFilteredSongs = artistNameList.Where(ab => lowerArtistFilters.Any(ab.ArtistName.ToLower().Contains)).SelectMany(s => s.a.SongLibraries.Select(s1 => s1.SongId)).Distinct().ToList();

                    if (FilterTracks3 != null && FilterTracks3.Any())
                    {
                        var fArt = (from t in FilterTracks3
                                    where artFilteredSongs.Any(t.LibraryTracks.Select(many => many.SongId.Value).Contains)
                                    select new PlayListSource3() { SourceAlbumId = t.SourceAlbumId, SourcePlaylistDetails = t.SourcePlaylistDetails, LibraryTracks = t.LibraryTracks.Where(s => artFilteredSongs.Contains(s.SongId.Value)).ToList() }).ToList();

                        list.AddRange(fArt.ToList());
                    }



                    // Album
                    var albumNameList = (from a in db.AlbumLibraries select new { a.AlbumName, a }).Distinct().ToList();
                    var albFilteredSongs = albumNameList.Where(ab => lowerAlbumFilters.Any(ab.AlbumName.ToLower().Contains)).SelectMany(s => s.a.SongLibraries.Select(s1 => s1.SongId)).ToList();

                    if (FilterTracks3 != null && FilterTracks3.Any())
                    {
                        var fAlb = (from t in FilterTracks3
                                    where albFilteredSongs.Any(t.LibraryTracks.Select(many => many.SongId.Value).Contains)
                                    select new PlayListSource3() { SourceAlbumId = t.SourceAlbumId, SourcePlaylistDetails = t.SourcePlaylistDetails, LibraryTracks = t.LibraryTracks.Where(s => albFilteredSongs.Contains(s.SongId.Value)).ToList() }).ToList();

                        list.AddRange(fAlb.ToList());
                    }


                }



                //var fAlb = from t in FilterTracks
                //        where t.SourceTracks.Any(an2 => lowerSongFilters.Any(an.SongName.ToLower().Contains))
                //        select new PlayListSource() { SourceAlbumLibrary = t.SourceAlbumLibrary, SourcePlaylistDetails = t.SourcePlaylistDetails, SourceTracks = t.SourceTracks.Where(s => lowerFilters.Any(s.SongName.ToLower().Contains)).ToList() };

                if (list.Any())
                {
                    FilterTracks3 = list;
                }

            }


            #region Genres

            if (GenreFilters.Any())
            {
                //string filter = ((Button)sender).Content.ToString();
                List<string> lowerFilters = (from fx in GenreFilters select fx.ToLower()).ToList();

                var f = from t in FilterTracks3
                        where t.LibraryTracks.Any(an => an.Genre != null && lowerFilters.Any(an.Genre.ToLower().Contains))
                        select new PlayListSource3() { SourceAlbumId = t.SourceAlbumId, SourcePlaylistDetails = t.SourcePlaylistDetails, LibraryTracks = t.LibraryTracks.Where(s => s.Genre != null && lowerFilters.Any(s.Genre.ToLower().Contains)).ToList() };

                FilterTracks3 = f.ToList();
            }

            #endregion Genres

            #region Years


            if (YearFilters.Any())
            {
                var f = from t in FilterTracks3
                        where t.SourceAlbumId.HasValue && t.LibraryTracks.Any(an => YearFilters.Any(an.Year.Contains))
                        select new PlayListSource3() { SourceAlbumId = t.SourceAlbumId, SourcePlaylistDetails = t.SourcePlaylistDetails, LibraryTracks = t.LibraryTracks };

                FilterTracks3 = f.ToList();
            }


            #endregion Years
        }
        private void ApplyFilters()
        {
            try
            {
                FilterTracks = SourceTracks;
                if (isUnsortedFilter)
                {
                    // no artist, no album
                    using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                    {
                        var songlist = db.SongLibraries.Where(w => !w.Artists.Any() && !w.AlbumLibraries.Any()).Select(s => s.SongId).ToList();

                        var f = from t in SourceTracks
                                where t.isTracksOnly == true
                                select new PlayListSource() { SourceAlbumLibrary = t.SourceAlbumLibrary, SourcePlaylistDetails = t.SourcePlaylistDetails, SourceTracks = t.SourceTracks.Where(sx => songlist.Contains(sx.SongId)).ToList() };

                        FilterTracks = f.ToList();
                    }
                }

                if (isNewFilter)
                {
                    using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                    {
                        var playSongs = (from a in db.Playlists
                                         where a.PlaylistId == Global.NewTracksPlaylistId && a.TrackLibrary.Type == Global.AppModeString
                                         select a.TrackLibrary.SongLibraries.FirstOrDefault().SongId).ToList();

                        var f = from t in SourceTracks
                                    //lowerFilters.Any(an.SongName.ToLower().Contains))

                                where t.SourceTracks.Any(an => playSongs.Contains(an.SongId))
                                //
                                select new PlayListSource() { SourceAlbumLibrary = t.SourceAlbumLibrary, SourcePlaylistDetails = t.SourcePlaylistDetails, SourceTracks = t.SourceTracks.Where(s => playSongs.Contains(s.SongId)).ToList() };

                        FilterTracks = f.ToList();
                    }
                }

                if (SearchFilters.Any())
                {
                    List<string> lowerArtistFilters = (from fx in SearchFilters where fx.SelectedMode == SearchMode.Artist || fx.SelectedMode == SearchMode.All select fx.SearchText.ToLower()).ToList();
                    List<string> lowerAlbumFilters = (from fx in SearchFilters where fx.SelectedMode == SearchMode.Album || fx.SelectedMode == SearchMode.All select fx.SearchText.ToLower()).ToList();
                    List<string> lowerSongFilters = (from fx in SearchFilters where fx.SelectedMode == SearchMode.Song || fx.SelectedMode == SearchMode.All select fx.SearchText.ToLower()).ToList();

                    //List<string> lowerAlbumFilters = (from fx in SearchFilters select fx.ToLower()).ToList();
                    //List<string> lowerSongFilters = (from fx in SearchFilters select fx.ToLower()).ToList();

                    //   string low = sFilter.ToLower();



                    var f = (from t in FilterTracks
                             where t.SourceTracks.Any(an => lowerSongFilters.Any(an.SongName.ToLower().Contains))
                             select new PlayListSource() { SourceAlbumLibrary = t.SourceAlbumLibrary, SourcePlaylistDetails = t.SourcePlaylistDetails, SourceTracks = t.SourceTracks.Where(s => lowerSongFilters.Any(s.SongName.ToLower().Contains)).ToList() }).ToList();

                    //var a1 = FilterTracks.SelectMany(s => s.SourceTracks.SelectMany(s1 => s1.Artists.Any()));
                    using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                    {
                        // Artist
                        var artistNameList = (from a in db.Artists select new { a.ArtistName, a }).Distinct().ToList();

                        var artFilteredSongs = artistNameList.Where(ab => lowerArtistFilters.Any(ab.ArtistName.ToLower().Contains)).SelectMany(s => s.a.SongLibraries.Select(s1 => s1.SongId)).ToList();


                        var fArt = (from t in FilterTracks
                                    where artFilteredSongs.Any(t.SourceTracks.Select(many => many.SongId).Contains)
                                    select new PlayListSource() { SourceAlbumLibrary = t.SourceAlbumLibrary, SourcePlaylistDetails = t.SourcePlaylistDetails, SourceTracks = t.SourceTracks.Where(s => artFilteredSongs.Contains(s.SongId)).ToList() }).ToList();

                        f.AddRange(fArt.ToList());


                        // Album
                        var albumNameList = (from a in db.AlbumLibraries select new { a.AlbumName, a }).Distinct().ToList();
                        var albFilteredSongs = albumNameList.Where(ab => lowerAlbumFilters.Any(ab.AlbumName.ToLower().Contains)).SelectMany(s => s.a.SongLibraries.Select(s1 => s1.SongId)).ToList();
                        var fAlb = (from t in FilterTracks
                                    where albFilteredSongs.Any(t.SourceTracks.Select(many => many.SongId).Contains)
                                    select new PlayListSource() { SourceAlbumLibrary = t.SourceAlbumLibrary, SourcePlaylistDetails = t.SourcePlaylistDetails, SourceTracks = t.SourceTracks.Where(s => albFilteredSongs.Contains(s.SongId)).ToList() }).ToList();

                        f.AddRange(fAlb.ToList());

                    }



                    //var fAlb = from t in FilterTracks
                    //        where t.SourceTracks.Any(an2 => lowerSongFilters.Any(an.SongName.ToLower().Contains))
                    //        select new PlayListSource() { SourceAlbumLibrary = t.SourceAlbumLibrary, SourcePlaylistDetails = t.SourcePlaylistDetails, SourceTracks = t.SourceTracks.Where(s => lowerFilters.Any(s.SongName.ToLower().Contains)).ToList() };

                    FilterTracks = f;
                }


                #region Genres


                if (GenreFilters.Any())
                {
                    //string filter = ((Button)sender).Content.ToString();
                    List<string> lowerFilters = (from fx in GenreFilters select fx.ToLower()).ToList();

                    var f = from t in FilterTracks
                            where t.SourceTracks.Any(an => an.Genre != null && lowerFilters.Any(an.Genre.ToLower().Contains))
                            select new PlayListSource() { SourceAlbumLibrary = t.SourceAlbumLibrary, SourcePlaylistDetails = t.SourcePlaylistDetails, SourceTracks = t.SourceTracks.Where(s => s.Genre != null && lowerFilters.Any(s.Genre.ToLower().Contains)).ToList() };

                    FilterTracks = f.ToList();
                }


                #endregion Genres

                #region Years


                if (YearFilters.Any())
                {
                    var f = from t in FilterTracks
                            where t.isTracksOnly == false && YearFilters.Contains((Math.Floor((double)(t.SourceAlbumLibrary.Year.GetValueOrDefault() / 10)) * 10).ToString() + "'s")
                            select new PlayListSource() { SourceAlbumLibrary = t.SourceAlbumLibrary, SourcePlaylistDetails = t.SourcePlaylistDetails, SourceTracks = t.SourceTracks.ToList() };

                    FilterTracks = f.ToList();
                }


                #endregion Years

                BuildCards();
            }
            catch (Exception)
            {

            }

        }

        private void btnSearchGo_Click(object sender, RoutedEventArgs e)
        {
            //string low = tbSearchText.Text.ToLower();
            //string high = tbSearchText.Text.ToUpper();

            //FilterTracks = from a in SourceTracks
            //               where a.SongName.ToLower().Contains(low)
            //               select a;

            //tbSearchText.Visibility = Visibility.Collapsed;
            //btnSearchGo.Visibility = Visibility.Collapsed;

            BuildCards();

        }

        #region Testing Tools


        private List<PlayListSource> GenerateTestData(int trackCount)
        {
            List<PlayListSource> TestSongs = new List<PlayListSource>();
            int TrackCounter = 0;
            int artistalbums = 0;
            Random r = new Random();

            Artist art = new Artist();
            AlbumLibrary album = new AlbumLibrary();

            while (TestSongs.Count < trackCount)
            {
                if (TrackCounter == 0)
                {
                    // assign random count for album tracks
                    TrackCounter = r.Next(5, 22);
                    // get album name
                    album = new AlbumLibrary();
                    album.AlbumName = GenerateName(r);

                    if (artistalbums == 0)
                    {
                        // get artist name
                        art = new Artist();
                        art.ArtistName = GenerateName(r);
                        artistalbums = r.Next(1, 3);
                    }
                    artistalbums -= 1;
                }

                // generate random track
                SongLibrary s = new SongLibrary();

                s.AlbumLibraries.Add(album);
                s.Artists.Add(art);

                s.SongName = GenerateName(r);

                // IT NEEDS THIS TO WORK, REMOVED BECAUSE NOT NEEDED NOW!
                //TestSongs.Add(s);

                TrackCounter -= 1;
            }

            return TestSongs;
        }

        private string GenerateName(Random r)
        {
            char[] chars = "abcdefghijklmnopqrstuvwxyz".ToArray();
            string n = "";
            int words = r.Next(1, 4);
            int length = r.Next(2, 8);

            // words
            for (int i = 0; i < words; i++)
            {
                for (int i2 = 0; i2 < length; i2++)
                {
                    n += chars[r.Next(0, 23)];
                }
                n += " ";
            }

            n = n.Trim().ToLowerInvariant();
            return n;
        }


        #endregion Testing Tools


        #region LibraryGridSetup










        #endregion LibraryGridSetup

        #region LibraryPopup



        //internal void OpenOverlay(AlbumLibrary album, List<SongLibrary> songs)
        //{
        //    Controls.LibraryPopup card = new Controls.LibraryPopup(album, songs);
        //    _openOverlay(card);
        //}

        internal void OpenOverlay(PlayListDetail playlist)
        {
            Controls.LibraryPopup card = new Controls.LibraryPopup(playlist);
            _openOverlay(card);
        }

        private void _openOverlay(Controls.LibraryPopup card)
        {
            //OverlayCanvas.Visibility = Visibility.Visible;
            Canvas.SetLeft(card, 500);
            Canvas.SetTop(card, 100);

            //OverlayCanvas.Children.Add(card);
        }

        private void CloseOverlay(object sender, KeyEventArgs e)
        {
            //OverlayCanvas.Children.Clear();
            //OverlayCanvas.Visibility = Visibility.Collapsed;
        }



        #endregion LibraryPopup

        public enum PlaylistModeEnum
        {
            ViewPlaylist,
            NewPlaylist,
            EditPlaylist
        }
        public void SetPlaylistMode(PlayListDetail playlistName, PlaylistModeEnum mode)
        {
            // Somehow the magic happens here
            txtLibMode = playlistName.Name;
            using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
            {
                var p = from a in db.Playlists
                        where a.PlaylistId == playlistName.Id
                        orderby a.SequenceNumber
                        select a.TrackId;


                BufferPlaylistTrackIDs = p.ToList();
            }
            switch (mode)
            {
                case PlaylistModeEnum.NewPlaylist:
                    isShowOnlyPlaylist = false;
                    this.ReloadLibrary(LibraryMode.NewPlaylist);
                    break;
                case PlaylistModeEnum.EditPlaylist:
                    isShowOnlyPlaylist = false;
                    this.ReloadLibrary(LibraryMode.EditPlaylist);
                    break;
                case PlaylistModeEnum.ViewPlaylist:
                    isShowOnlyPlaylist = true;
                    this.ReloadLibrary(LibraryMode.Library);
                    break;
            }
        }

        public void OpenPlaylist(object sender, RoutedEventArgs e)
        {
            LibraryPlaylist lp = (LibraryPlaylist)sender;

            Global.AppControlMode = Global.AppControlModeEnum.Playlist;
            Global.AppPlaylistModeBufferID = lp.PlaylistId;

            //SetPlaylistMode(lp.PlaylistName);
        }
        private void btnAddNew_Click(object sender, RoutedEventArgs e)
        {
            if (Global.AppControlMode != Global.AppControlModeEnum.Playlist)
            {
                Global.AppControlMode = Global.AppControlModeEnum.Normal;
                ReloadLibrary(LibraryMode.NewPlaylist);
            }
            else
                Global.mPlayer.PopUp_NewPlaylist();
        }

        private void btnSavePlaylistChanges_Click(object sender, RoutedEventArgs e)
        {
            Global.AppControlMode = Global.AppControlModeEnum.Playlist;
            Global.AppPlaylistModeBufferID = 0;

            ReloadLibrary(LibraryMode.Playlists);
        }
        private void btnCancelPlaylistChanges_Click(object sender, RoutedEventArgs e)
        {
            int i = Global.AppPlaylistModeBufferID;

            if (LibMode == LibraryMode.NewPlaylist)
            {

                using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                {
                    var p = from pl in db.Playlists
                            where pl.PlaylistId == i
                            select pl;

                    db.Playlists.RemoveRange(p);
                    db.SaveChanges();

                    var p1 = from pd in db.PlayListDetails
                             where pd.Id == i
                             select pd;

                    db.PlayListDetails.Remove(p1.FirstOrDefault());
                    db.SaveChanges();
                }
            }
            else if (LibMode == LibraryMode.EditPlaylist)
            {
                // Rollback Changes
                using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                {
                    var bufferA = db.PlayListDetails.Where(w => w.Name == "PlaylistBuffer" && w.Type == 0).FirstOrDefault().Playlists;
                    var bufferB = from a1 in bufferA
                                  select a1.TrackId;

                    var r = from b in db.Playlists
                            where b.PlaylistId == i && bufferB.Contains(b.TrackId)
                            select b;

                    db.Playlists.RemoveRange(r);
                    db.SaveChanges();

                    db.Playlists.RemoveRange(bufferA);
                    db.SaveChanges();
                }
            }

            Global.AppPlaylistModeBufferID = 0;

            Global.AppControlMode = Global.AppControlModeEnum.Playlist;
            ReloadLibrary(LibraryMode.Playlists);
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            ReloadLibrary(LibraryMode.Playlists);
        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            if (Global.AdminSettings.IsMusicLibraryScrollHorizontal)
            {
                HorizontalScrollViewLibrary.PageDown();
            }
            else
            {
                VerticalScrollViewLibrary.PageDown();
            }

        }

        private void btnPrevious_Click(object sender, RoutedEventArgs e)
        {
            if (Global.AdminSettings.IsMusicLibraryScrollHorizontal)
            {
                HorizontalScrollViewLibrary.PageUp();
            }
            else
            {
                VerticalScrollViewLibrary.PageUp();
            }
        }

        private void btnLoadNewTracks(object sender, EventArgs e)
        {
            isNewFilter = true;

            LoadRightStackPannel();
            SourceTracksDBBatchController();

            //ApplyFilters();

            //LoadIndex();
        }

        private void btnShowSelectedOnly(object sender, EventArgs e)
        {

            var btnToggle = (ToggleButton)sender;
            if (btnToggle.IsChecked.GetValueOrDefault())
            {
                FilterTracks = SourceTracks.ToList();
            }
            else
            {
                var f = from t in SourceTracks
                        where t.SourceTracks.Any(an => ActivePlaylistHelper.SongIDs.Contains(an.SongId))
                        select new PlayListSource() { SourceAlbumLibrary = t.SourceAlbumLibrary, SourcePlaylistDetails = t.SourcePlaylistDetails, SourceTracks = t.SourceTracks.Where(s => ActivePlaylistHelper.SongIDs.Contains(s.SongId)).ToList() };
                FilterTracks = f.ToList();
            }

            ApplyFilters();
            //BuildCards();
            LoadIndex();
        }

        private void btnPlayAll(object sender, EventArgs e)
        {
            if (Global.LibraryFlipFlop)
            {
                switch (LibMode)
                {
                    case LibraryMode.Library:
                        if (Global.AppActionMode == Global.AppActionModeEnum.Idle)
                        {
                            //if not playing
                            Global.AppActionMode = Global.AppActionModeEnum.Playing;
                            if (FilterTracks3 == null)
                            {
                                Global.mPlayer.PlayNow(GetTracks(SourceTracks3));
                                RefreshCardItems();
                            }
                            else
                            {
                                Global.mPlayer.PlayNow(GetTracks(FilterTracks3));
                                RefreshCardItems(FilterTracks3, true);
                            }
                        }
                        else
                        {
                            //if playing
                            Global.mPlayer.AddToPlaylist(GetTracks(FilterTracks3));
                            RefreshCardItems(FilterTracks3);
                        }
                        break;
                    case LibraryMode.EditPlaylist:
                        break;
                    case LibraryMode.Favourites:
                        break;
                    case LibraryMode.JustPlay:
                        break;
                    case LibraryMode.LastPlayed:
                        break;
                    case LibraryMode.NewPlaylist:
                        break;
                    case LibraryMode.Playlists:
                        break;
                }
            }
            else
            {
                switch (LibMode)
                {
                    case LibraryMode.Library:
                        if (Global.AppActionMode == Global.AppActionModeEnum.Idle)
                        {
                            //if not playing
                            Global.mPlayer.PlayNow(GetTracks(FilterTracks));
                            Global.AppActionMode = Global.AppActionModeEnum.Playing;
                            RefreshCardItems(FilterTracks, true);
                        }
                        else
                        {
                            //if playing
                            Global.mPlayer.AddToPlaylist(GetTracks(FilterTracks));
                            RefreshCardItems(FilterTracks);
                        }
                        break;
                    case LibraryMode.EditPlaylist:
                        break;
                    case LibraryMode.Favourites:
                        break;
                    case LibraryMode.JustPlay:
                        break;
                    case LibraryMode.LastPlayed:
                        break;
                    case LibraryMode.NewPlaylist:
                        break;
                    case LibraryMode.Playlists:
                        break;
                }
            }
        }

        #region Helpers

        private List<TrackLibrary> GetTracks(List<PlayListSource3> playlistSource)
        {
            List<TrackLibrary> t = new List<TrackLibrary>();

            using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
            {
                var x = playlistSource.SelectMany(s => s.LibraryTracks).ToList();
                //var x = FilterTracks.SelectMany(s => s.SourceTracks).ToList();
                var xi = x.Select(s => s.TrackId);

                var b = from a in db.TrackLibraries
                        where xi.Contains(a.Id)
                        select a;

                t = b.ToList(); ;
            }

            return t;
        }
        private List<TrackLibrary> GetTracks(List<PlayListSource> playlistSource)
        {
            List<TrackLibrary> t = new List<TrackLibrary>();

            using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
            {
                var x = FilterTracks.SelectMany(s => s.SourceTracks).ToList();
                var xi = x.Select(s => s.SongId);

                var b = from a in db.TrackLibraries
                        where a.Type == Global.AppModeString && xi.Contains(a.SongLibraries.FirstOrDefault().SongId)
                        select a;

                t = b.ToList(); ;
            }

            return t;
        }

        #endregion Helpers

        private void btnBack_Click_1(object sender, RoutedEventArgs e)
        {
            Global.MainWindow.GoToMainMenu();
            Global.MainWindow.MenuMinimalToggle(Visibility.Visible);
        }

        private void FilterYear(object sender, RoutedEventArgs e)
        {

        }

        public static bool IsLoading { get; set; }
        public void OpenFilterYearsPopup(object sender, RoutedEventArgs e)
        {
            try
            {
                Global.mPlayer.popUp_Frame.Content = null;
                Global.mPlayer.popUp_Grid.Visibility = Visibility.Visible;
                Global.mPlayer.popUp_Frame.Content = new YearsPopup(this.LoadYears(), AddYearFilter, RemoveYearFilter);
            }
            catch (Exception)
            {

            }
        }

        public void AddYearFilter(string selectedYear)
        {
            try
            {
                YearFilters.Add(selectedYear);
                LoadRightStackPannel();

                if (Global.LibraryFlipFlop)
                {
                    SourceTracksDBBatchController();
                }
                else
                {
                    ApplyFilters();
                }
            }
            catch (Exception)
            {

            }
        }


        public void RemoveYearFilter(string selectedYear)
        {
            try
            {
                YearFilters.Remove(selectedYear);
                LoadRightStackPannel();

                if (Global.LibraryFlipFlop)
                {
                    SourceTracksDBBatchController();
                }
                else
                {
                    ApplyFilters();
                }
            }
            catch (Exception)
            {

            }
        }

        private void ExpandStack(object sender, MouseButtonEventArgs e)
        {
            try
            {
                UnexpandedStack.Visibility = Visibility.Collapsed;
                ExpandedStack.Visibility = Visibility.Visible;
            }
            catch (Exception)
            {

            }
        }

        private void CollapseStack(object sender, MouseButtonEventArgs e)
        {
            try
            {
                UnexpandedStack.Visibility = Visibility.Visible;
                ExpandedStack.Visibility = Visibility.Collapsed;
            }
            catch (Exception)
            {

            }
        }

        public static bool ShowFavouritesOnly { get; set; }
        public void ShowFavourites(object sender, RoutedEventArgs e)
        {
            try
            {
                var albumTracks = (from ab in FilterTracks
                                   where ab.isTracksOnly == false
                                   select ab);
                Global.mPlayer.popUp_Grid.Visibility = Visibility.Visible;
                Global.mPlayer.popUp_Frame.Content = new favouriteItemsPopup(albumTracks.ToList());
            }
            catch (Exception)
            {

            }
        }
        public static int MaxHorzCards = 12;
        public static int Index = 0;
        public static int MaxIndex { get; set; }
        private void btnHorPrev_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Index > 0)
                {
                    Index--;
                }

                if (Index < 0)
                {
                    return;
                }



                var currentVisibleAlbumTracks = (from ab in FilterTracks.Skip(MaxHorzCards * Index).Take(MaxHorzCards).ToList()
                                                 where ab.isTracksOnly == false
                                                 select ab);
                if (currentVisibleAlbumTracks.Count() > 0)
                {
                    AlbumsCountInfoLabel.Content = CountStack[Index];
                    if (Global.AdminSettings.IsMusicLibraryScrollHorizontal)
                    {
                        cFactory.AddNewAlbumRow(currentVisibleAlbumTracks, ref gridLibraryHorizontal, this, ActivePlaylistHelper.AlbumIDs);
                        SetPreviousCardsPanel();
                        SetNextCardsPanel();
                    }
                    else
                    {
                        cFactory.AddNewAlbumRow(currentVisibleAlbumTracks, ref gridLibraryVertical, this, ActivePlaylistHelper.AlbumIDs);
                    }
                }


            }
            catch (Exception)
            {
            }

        }

        private void SetPreviousCardsPanel()
        {
            try
            {
                if (Global.AdminSettings.IsMusicLibraryScrollHorizontal)
                {
                    PreviousCardsInfoContainerPanel.Children.Clear();
                    IEnumerable<PlayListSource> previousCards = new List<PlayListSource>();
                    if (Index > 0)
                    {
                        previousCards = (from ab in FilterTracks.Skip(MaxHorzCards * (Index - 1)).Take(2).ToList()
                                         where ab.isTracksOnly == false
                                         select ab);

                        using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                        {
                            //System.Data.Linq
                            var wrapPanel = new WrapPanel { HorizontalAlignment = HorizontalAlignment.Center };
                            wrapPanel.MaxWidth = 200;
                            foreach (var al in previousCards.ToList())
                            {
                                string artistname = "";
                                var tempArt = db.SongLibraries.Find(al.SourceTracks.First().SongId);
                                if (tempArt.Artists.Any())
                                {
                                    artistname = tempArt.Artists.First().ArtistName;
                                }


                                Controls.LibraryCardV1 card = new Controls.LibraryCardV1(al.SourceAlbumLibrary, al.SourceTracks.ToList(), artistname, this, false);
                                card.Opacity = 0.3;
                                wrapPanel.Children.Add(card);
                            }

                            PreviousCardsInfoContainerPanel.Children.Add(wrapPanel);
                            Grid.SetRow(wrapPanel, 0);
                        }
                    }
                    else
                    {
                        PreviousCardsInfoContainerPanel.Children.Clear();
                    }
                }
                else
                {
                    gridLibraryVerticalPrevious.Children.Clear();
                    IEnumerable<PlayListSource> previousCards = new List<PlayListSource>();
                    if (Index > 0)
                    {
                        previousCards = (from ab in FilterTracks.Skip(MaxHorzCards * (Index - 1)).Take(12).ToList()
                                         where ab.isTracksOnly == false
                                         select ab);

                        using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                        {
                            //System.Data.Linq
                            var wrapPanel = new UniformGrid { Rows = 2, Columns = 6 };
                            foreach (var al in previousCards.ToList())
                            {
                                string artistname = "";
                                var tempArt = db.SongLibraries.Find(al.SourceTracks.First().SongId);
                                if (tempArt.Artists.Any())
                                {
                                    artistname = tempArt.Artists.First().ArtistName;
                                }


                                Controls.LibraryCard card = new Controls.LibraryCard(al.SourceAlbumLibrary, al.SourceTracks.ToList(), artistname, this, false);
                                wrapPanel.Children.Add(card);
                            }

                            gridLibraryVerticalPrevious.Children.Add(wrapPanel);
                            Grid.SetRow(wrapPanel, 0);
                        }
                    }
                    else
                    {
                        gridLibraryVerticalPrevious.Children.Clear();
                    }
                }
            }
            catch (Exception)
            {

            }

        }

        private void SetNextCardsPanel()
        {
            try
            {
                if (!Global.AdminSettings.IsMusicLibraryScrollHorizontal)
                {
                    gridLibraryVerticalNext.Children.Clear();
                    IEnumerable<PlayListSource> previousCards = new List<PlayListSource>();
                    previousCards = (from ab in FilterTracks.Skip(MaxHorzCards * (Index + 1)).Take(12).ToList()
                                     where ab.isTracksOnly == false
                                     select ab);

                    using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                    {
                        //System.Data.Linq
                        var wrapPanel = new UniformGrid { Rows = 2, Columns = 6 };
                        foreach (var al in previousCards.ToList())
                        {
                            string artistname = "";
                            var tempArt = db.SongLibraries.Find(al.SourceTracks.First().SongId);
                            if (tempArt.Artists.Any())
                            {
                                artistname = tempArt.Artists.First().ArtistName;
                            }


                            Controls.LibraryCard card = new Controls.LibraryCard(al.SourceAlbumLibrary, al.SourceTracks.ToList(), artistname, this, false);
                            card.Margin = new Thickness(5);
                            wrapPanel.Children.Add(card);
                        }

                        gridLibraryVerticalNext.Children.Add(wrapPanel);
                        Grid.SetRow(wrapPanel, 0);
                    }
                }
                else
                {
                    NextCardsInfoContainerPanel.Children.Clear();
                    IEnumerable<PlayListSource> nextCards = new List<PlayListSource>();
                    if (Index < MaxIndex)
                    {
                        nextCards = (from ab in FilterTracks.Skip(MaxHorzCards * (Index + 1)).Take(2).ToList()
                                     where ab.isTracksOnly == false
                                     select ab);

                        using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                        {
                            //System.Data.Linq
                            var wrapPanel = new WrapPanel { HorizontalAlignment = HorizontalAlignment.Center };
                            wrapPanel.MaxWidth = 200;
                            foreach (var al in nextCards.ToList())
                            {
                                string artistname = "";
                                var tempArt = db.SongLibraries.Find(al.SourceTracks.First().SongId);
                                if (tempArt.Artists.Any())
                                {
                                    artistname = tempArt.Artists.First().ArtistName;
                                }


                                Controls.LibraryCardV1 card = new Controls.LibraryCardV1(al.SourceAlbumLibrary, al.SourceTracks.ToList(), artistname, this, false);
                                card.Opacity = 0.3;
                                wrapPanel.Children.Add(card);
                            }

                            NextCardsInfoContainerPanel.Children.Add(wrapPanel);
                            PreviousCardsInfoContainerPanel.Children.Add(wrapPanel);
                            Grid.SetRow(wrapPanel, 0);
                        }
                    }
                    else
                    {
                        NextCardsInfoContainerPanel.Children.Clear();
                    }
                }
            }
            catch (Exception)
            {

            }
        }

        private void btnHorNext_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Index == Math.Ceiling(Convert.ToDouble(totalCardCount / MaxHorzCards)))
                {
                    Index = 0;
                }
                else
                {
                    Index++;
                }

                AlbumsCountInfoLabel.Content = CountStack[Index];
                var albumTracks = (from ab in FilterTracks.Skip(MaxHorzCards * Index).Take(MaxHorzCards).ToList()
                                   where ab.isTracksOnly == false
                                   select ab);
                if (albumTracks.Count() > 0)
                {
                    if (Global.AdminSettings.IsMusicLibraryScrollHorizontal)
                    {
                        cFactory.AddNewAlbumRow(albumTracks, ref gridLibraryHorizontal, this, ActivePlaylistHelper.AlbumIDs);
                    }
                    else
                    {
                        cFactory.AddNewAlbumRow(albumTracks, ref gridLibraryVertical, this, ActivePlaylistHelper.AlbumIDs);
                    }
                }

                SetPreviousCardsPanel();
                SetNextCardsPanel();
            }
            catch (Exception ex)
            {
            }
        }

        private void DeleteStack(object sender, MouseButtonEventArgs e)
        {

        }

        private void EditStack(object sender, MouseButtonEventArgs e)
        {

        }

        private void btnScrollUp_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Index > 0)
                {
                    Index--;
                }

                if (Index < 0)
                {
                    return;
                }



                var currentVisibleAlbumTracks = (from ab in FilterTracks.Skip(MaxHorzCards * Index).Take(MaxHorzCards).ToList()
                                                 where ab.isTracksOnly == false
                                                 select ab);
                if (currentVisibleAlbumTracks.Count() > 0)
                {
                    AlbumsCountInfoLabel.Content = CountStack[Index];
                    cFactory.AddNewAlbumRow(currentVisibleAlbumTracks, ref gridLibraryVertical, this, ActivePlaylistHelper.AlbumIDs);                    
                    //if (gridLibraryVerticalPrevious.Children.Count > 0)
                    //{
                    //    var childToRemove = gridLibraryVertical.Children[0] as UniformGrid;
                    //    gridLibraryVertical.Children.Clear();
                    //    var childToAdd = gridLibraryVerticalPrevious.Children[0] as UniformGrid;
                    //    childToAdd.Opacity = 1;
                    //    gridLibraryVerticalPrevious.Children.Remove(childToAdd);
                    //    gridLibraryVertical.Children.Add(childToAdd);
                    //    gridLibraryVerticalNext.Children.Add(childToRemove);
                    //}
                }

                SetPreviousCardsPanel();
                SetNextCardsPanel();
            }
            catch (Exception)
            {

            }
        }

        private void btnScrollDown_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Index == Math.Ceiling(Convert.ToDouble(totalCardCount / MaxHorzCards)))
                {
                    Index = 0;
                }
                else
                {
                    Index++;
                }

                AlbumsCountInfoLabel.Content = CountStack[Index];
                var albumTracks = (from ab in FilterTracks.Skip(MaxHorzCards * Index).Take(MaxHorzCards).ToList()
                                   where ab.isTracksOnly == false
                                   select ab);
                if (albumTracks.Count() > 0)
                {
                    cFactory.AddNewAlbumRow(albumTracks, ref gridLibraryVertical, this, ActivePlaylistHelper.AlbumIDs);
                    //var childToremove = gridLibraryVertical.Children[0] as UniformGrid;
                    //gridLibraryVertical.Children.Clear();
                    //var childToAdd = gridLibraryVerticalNext.Children[0] as UniformGrid;
                    //childToAdd.Opacity = 1;
                    //gridLibraryVerticalNext.Children.Remove(childToAdd);
                    //gridLibraryVertical.Children.Add(childToAdd);

                    //gridLibraryVerticalPrevious.Children.Add(childToremove);
                }

                SetPreviousCardsPanel();
                SetNextCardsPanel();
            }
            catch (Exception ex)
            {

            }
        }
    }
}
