using iTunesSearch.Library;
using iTunesSearch.Library.Models;
using System;
using System.Collections.Generic;
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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static JukeBoxSolutions.ImportFactory;
using static iTunesSearch.Library.iTunesSearchManager;
using System.Windows.Media.Animation;
using System.Text.RegularExpressions;
using System.IO;
using System.Diagnostics;

namespace JukeBoxSolutions.Controls
{
    /// <summary>
    /// Interaction logic for Popup_SmartUpdater.xaml
    /// </summary>
    public partial class Popup_SmartUpdater : UserControl
    {

        private managedArtist PageArtist = new managedArtist();
        private class managedArtist
        {
            internal void SetVerified(SongArtist value)
            {

                throw new NotImplementedException();
            }
        }

        private managedTrack PageTrack = new managedTrack();
        private class managedTrack
        {

        }
        private managedTrackNumber PageTrackNumber = new managedTrackNumber();
        private class managedTrackNumber
        {

        }
        private managedAlbum PageAlbum = new managedAlbum();
        private class managedAlbum
        {

        }



        // Info Messages
        private enum InfoMessage
        {
            NoMetaData,
            ArtistFound,
            Searching,
            CannotFindAlbum,
            MissingAlbum,
            MissingArtist,
            MissingTrack,
            None
        }
        private InfoMessage currentInfoMessage = InfoMessage.None;
        private void SetInfoMessage(InfoMessage info)
        {
            currentInfoMessage = info;
            //gridInstruction.Visibility = Visibility.Visible;

            switch (info)
            {
                case InfoMessage.NoMetaData:
                    lblInstructionHeader.Content = "No Meta-Data available";
                    lblInstruction.Content = "Start by finding the Artist";
                    break;
                case InfoMessage.ArtistFound:
                    lblInstructionHeader.Content = "Found the Artist";
                    lblInstruction.Content = ArtistName;
                    break;
                case InfoMessage.Searching:
                    lblInstructionHeader.Content = "Connecting to servers..";
                    lblInstruction.Content = "Searching..";
                    break;
                case InfoMessage.CannotFindAlbum:
                    lblInstructionHeader.Content = "No results";
                    lblInstruction.Content = "Cannot find Album";
                    break;
                case InfoMessage.MissingAlbum:
                    lblInstructionHeader.Content = "Missing MetaData";
                    lblInstruction.Content = "Cannot find Album Details";
                    break;
                case InfoMessage.MissingArtist:
                    lblInstructionHeader.Content = "Missing MetaData";
                    lblInstruction.Content = "Cannot find Artist Details";
                    break;
                case InfoMessage.MissingTrack:
                    lblInstructionHeader.Content = "Missing MetaData";
                    lblInstruction.Content = "Please set the Song / Track Name";
                    break;
                case InfoMessage.None:
                    lblInstructionHeader.Content = "";
                    lblInstruction.Content = "";
                    //gridInstruction.Visibility = Visibility.Collapsed;
                    break;
            }
        }



        public string Title
        {
            set
            {
                if (string.IsNullOrEmpty(value))
                {

                }
                else
                {
                    lblMainTrackName.Content = value;
                    //btnTitle.BorderBrush = btnGreenTemplate.BorderBrush;
                    //btnTitle.Background = btnGreenTemplate.Background;
                }
            }
        }
        public string ArtistName
        {
            get { return _artistName; }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                }
                else
                {
                    lblMainArtistName.Content = value;

                    //btnArtist.BorderBrush = btnGreenTemplate.BorderBrush;
                    //btnArtist.Background = btnGreenTemplate.Background;
                    Global.StopButtonFlashing(ref btnArtist);
                    SetButtonGreen(ref btnArtist);
                }
                _artistName = value;
                ResetAll();
            }
        }
        private string _artistName = "";
        private string _albumName = "";
        public string AlbumName
        {
            get { return _albumName; }
            set
            {
                if (string.IsNullOrEmpty(value))
                {

                }
                else
                {
                    _albumName = value;
                    lblMainAlbumName.Content = value;
                    //btnAlbum.BorderBrush = btnGreenTemplate.BorderBrush;
                    //btnAlbum.Background = btnGreenTemplate.Background;
                }
            }
        }

        List<ManagedFile> TroublesomeSongs { get; set; }
        List<TrackLibrary> TroublesomeTracks { get; set; }
        bool isTracks = false;
        bool hasID3 = false;

        Artist bufferBaseDBArtist { get; set; }
        AlbumLibrary bufferBaseDBAlbum { get; set; }
        LibraryCard bufferBaseDBAlbumCard { get; set; }
        SongLibrary bufferBaseDBSong { get; set; }

        private string bufferImportTypeString { get; set; }
        ManagedFile bufferFile { get; set; }
        ToggleButton bufferButton { get; set; }
        Song bufferSong { get; set; }
        SearchAnalysisRule bufferRule { get; set; }

        string searchString
        {
            get
            {
                return baseString;
            }
            set
            {
                baseString = value;
                lblSelectedName.Text = baseString;

                if (string.IsNullOrEmpty(baseString))
                {
                    btnSearchAlbum.IsEnabled = false;
                    btnSearchArtist.IsEnabled = false;
                    btnSearchTrack.IsEnabled = false;
                }
                else
                {
                    btnSearchAlbum.IsEnabled = true;
                    btnSearchArtist.IsEnabled = true;
                    btnSearchTrack.IsEnabled = true;

                    if (btnSetAlbum.Visibility == Visibility.Visible)
                    {
                        btnSearchAlbum.Visibility = Visibility.Visible;
                        btnSetAlbum.Visibility = Visibility.Collapsed;
                    }
                }
            }
        }
        string baseString = "";
        int baseIndex = -1;
        bool hasArtist = false;

        string SetArtist
        {
            set
            {
                btnArtist.Content = value;
            }
        }

        private void SetVerifiedArtist()
        {
            hasArtist = true;
            //lblMainArtistName.Foreground = btnGreenTemplate.BorderBrush;
            btnSearchArtist.IsEnabled = false;
        }

        public SongArtist NewBaseArtist
        {
            set
            {
                // wrong code, but need something to happen here
                PageArtist = new managedArtist();

                bufferBaseArtist = value;
                ArtistName = bufferBaseArtist.ArtistName;

                SetVerifiedArtist();
                SetInfoMessage(InfoMessage.ArtistFound);

                // disable button
                if (bufferButton != null)
                    bufferButton.IsEnabled = false;

                // The next step
                //Global.SetButtonFlashing(ref btnAlbum);

                //btnAlbum.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            }
        }

        private SongArtist bufferBaseArtist { get; set; }
        public bool NewImported
        {
            set
            {
                if (value == true)
                {
                    btnSearchTrack.Content = "Imported!";
                    btnSearchTrack.IsEnabled = false;

                    bufferButton.IsEnabled = false;

                    btnNextTrack.Visibility = Visibility.Visible;
                    btnNextTrack.IsEnabled = true;
                }
            }
        }

        private Song bufferBaseSong { get; set; }
        public Song NewBaseSong
        {
            get { return bufferBaseSong; }
            set
            {
                SetButtonGreen(ref btnTitle);
                bufferFile.iTunesSong = value;
                bufferBaseSong = value;
                hasTrack = true;
                lblMainTrackName.Content = bufferBaseSong.TrackName;

                iTunesSearchManager iManager = new iTunesSearchManager();
                iManager.SearchAlbum(bufferBaseSong.CollectionId.ToString());
            }
        }

        public SongResult NewBaseAlbumResult
        {
            get { return bufferAlbumSongs; }
            internal set
            {
                bufferAlbumSongs = value;

                Global.mPlayer.BumpPopUp(new Popup_Confirm(bufferBaseAlbum, bufferAlbumSongs));
            }
        }

        private string baseFileSearchPattern = "";
        public void SetAlbumConfirm(bool isConfirmed)
        {
            if (isConfirmed)
            {
                hasAlbum = true;
                btnSearchAlbum.Content = bufferBaseAlbum.CollectionName;
                AlbumName = bufferBaseAlbum.CollectionName;
                lblMainAlbumName.Foreground = btnSearchAlbum.BorderBrush;
                gridInstruction.Visibility = Visibility.Collapsed;

                ResetAll();

                if (hasTrack)
                {
                    //btnSaveDB.Visibility = Visibility.Visible;

                    // Extract search pattern
                    string pattern = bufferFile.FileName.ToLower();

                    if (pattern.Contains(bufferBaseSong.ArtistName.ToLower()))
                    {
                        pattern = pattern.Replace(bufferBaseSong.ArtistName.ToLower(), "{0}");
                    }
                    if (pattern.Contains(bufferBaseSong.TrackName.ToLower()))
                    {
                        pattern = pattern.Replace(bufferBaseSong.TrackName.ToLower(), "{1}");
                    }
                    if (pattern.Contains("0" + bufferBaseSong.TrackNumber.ToString()))
                    {
                        pattern = pattern.Replace("0" + bufferBaseSong.TrackNumber.ToString(), "{2}");
                    }
                    else if (pattern.Contains(bufferBaseSong.TrackNumber.ToString()))
                        pattern = pattern.Replace(bufferBaseSong.TrackNumber.ToString(), "{2}");

                    if (pattern.Contains(bufferBaseSong.CollectionName.ToLower()))
                    {
                        pattern = pattern.Replace(bufferBaseSong.CollectionName.ToLower(), "{3}");
                    }

                    baseFileSearchPattern = pattern;
                    SongAlbumMatch(pattern);
                }
                else
                {
                    // Match Song with records
                    SongAlbumMatch();
                }
            }
            else
            {
                bufferBaseAlbum = null;
                ResetAll();
                SetButtonRed(ref btnAlbum);
            }
        }

        private void SongAlbumMatch(string searchPattern)
        {
            SearchAnalysisRule s = new SearchAnalysisRule();
            s.isPatternMatch = true;
            s.fileSearchPattern = baseFileSearchPattern;

            bufferSong = bufferBaseSong;
            bufferRule = s;
            Title = bufferSong.TrackName;
            hasTrack = true;
            ConfirmUpdate = true;

            //lblMainTrackName.Foreground = btnGreenTemplate.BorderBrush;

            bufferAlbumSongs.Songs.Remove(bufferSong);
        }

        private void SongAlbumMatch()
        {
            // has ID3?
            if (bufferFile.hasId3)
            {

            }

            // has artist name?
            // has track number?

            var searchNames = Global.importFactory.aEngine.GetTrackSearchStrings(bufferFile.FileName);
            //foreach (var s in searchNames)
            for (int i = searchNames.Count - 1; i >= 0; i--)
            {
                var s = searchNames[i];
                if (s.isPossibleTrackNumber)
                {
                    if (bufferAlbumSongs.Songs.Where(w => w.TrackNumber == s.TrackNumber).Any())
                    {
                        bufferRule = s;
                        bufferSong = bufferAlbumSongs.Songs.Where(w => w.TrackNumber == s.TrackNumber).First();
                        Title = bufferSong.TrackName;
                        hasTrack = true;
                        ConfirmUpdate = true;
                        bufferAlbumSongs.Songs.Remove(bufferSong);
                        break;
                    }
                }
                else
                {
                    if (bufferAlbumSongs.Songs.Where(w => w.TrackName == s.SearchTerm).Any())
                    {
                        bufferSong = bufferAlbumSongs.Songs.Where(w => w.TrackName == s.SearchTerm).First();
                        bufferRule = s;
                        Title = bufferSong.TrackName;
                        hasTrack = true;
                        ConfirmUpdate = true;
                        bufferAlbumSongs.Songs.Remove(bufferSong);
                    }
                    else
                    {
                        List<Song> directMatches = new List<Song>();
                        List<Song> extraMatches = new List<Song>();
                        List<Song> spellingMatches = new List<Song>();

                        foreach (var song in bufferAlbumSongs.Songs)
                        {
                            switch (getTrackEqual(song.TrackName, s.SearchTerm))
                            {
                                case CompareResults.Match:
                                    directMatches.Add(song);
                                    break;
                                case CompareResults.MatchWithExtra:
                                    extraMatches.Add(song);
                                    break;
                                case CompareResults.MatchSpellingError:
                                    spellingMatches.Add(song);
                                    break;
                            }

                            if (hasTrack)
                                break;
                        }

                        Song y;
                        var x = bufferAlbumSongs.Songs.Where(w => w.TrackName != null && w.TrackName.Contains("rulan")).Any();
                        if (x)
                        {
                            y = bufferAlbumSongs.Songs.Where(w => w.TrackName != null && w.TrackName.Contains("rulan")).First();
                            getTrackEqual(y.TrackName, s.SearchTerm);
                        }

                        if (directMatches.Any())
                        {
                            foreach (var song in directMatches)
                            {
                                bufferSong = song;
                                bufferRule = s;
                                Title = bufferSong.TrackName;
                                hasTrack = true;
                                ConfirmUpdate = true;
                                bufferAlbumSongs.Songs.Remove(bufferSong);
                            }
                        }
                        else if (extraMatches.Any())
                        {
                            foreach (var song in extraMatches)
                            {
                                bufferSong = song;
                                bufferRule = s;
                                Title = bufferSong.TrackName;
                                hasTrack = true;
                                ConfirmUpdate = true;
                                bufferAlbumSongs.Songs.Remove(bufferSong);
                            }
                        }
                        else if (spellingMatches.Any())
                        {
                            foreach (var song in spellingMatches)
                            {
                                bufferSong = song;
                                bufferRule = s;
                                Title = bufferSong.TrackName;
                                hasTrack = true;
                                ConfirmUpdate = true;
                                bufferAlbumSongs.Songs.Remove(bufferSong);
                            }
                        }
                    }
                }
                if (hasTrack)
                    break;
            }
        }

        private SongResult bufferAlbumSongs;
        private bool ConfirmUpdate
        {
            set
            {
                if (value == true)
                {
                    _confirmUpdate = value;
                    btnSave.Visibility = Visibility.Visible;

                    stackWordLists.Visibility = Visibility.Collapsed;
                    //stackMainToggleButtons.Visibility = Visibility.Collapsed;
                    stackSaveUpdate.Visibility = Visibility.Visible;
                }
            }
        }
        private bool _confirmUpdate = false;
        private bool hasTrack = false;
        private bool hasAlbum = false;
        public void CannotFindArtist()
        {
            hasArtist = false;

            //btnSearchTrack.Content = "No Track Found";
            //btnSearchTrack.IsEnabled = false;

            btnSearchArtist.Visibility = Visibility.Collapsed;
            btnSetArtist.Visibility = Visibility.Visible;
            SetButtonRed(ref btnSetArtist);
        }
        public void CannotFindTrack()
        {
            hasTrack = false;

            //btnSearchTrack.Content = "No Track Found";
            //btnSearchTrack.IsEnabled = false;

            btnSearchTrack.Visibility = Visibility.Collapsed;
            btnSetTrack.Visibility = Visibility.Visible;
            SetButtonRed(ref btnSetTrack);
            //SetButtonRed(ref btnSearchTrack);
        }
        public void CannotFindAlbum()
        {
            hasAlbum = false;
            SetInfoMessage(InfoMessage.CannotFindAlbum);

            gridEdit.Visibility = Visibility.Visible;

            //btnSearchAlbum.Content = "No Album Found";
            //btnSearchAlbum.IsEnabled = false;

            btnSearchAlbum.Visibility = Visibility.Collapsed;
            btnSetAlbum.Visibility = Visibility.Visible;

            btnSetAlbum.Content = "Create new Album";

            SetButtonRed(ref btnSetAlbum);
            //Global.SetButtonFlashing(ref btnSetAlbum);
        }

        public Album NewBaseAlbum
        {
            get { return bufferBaseAlbum; }
            internal set { bufferBaseAlbum = value; bufferFile.iTunesAlbum = value; }
        }
        private Album bufferBaseAlbum;


        private void GetStackID3()
        {
            // Set Album for option
            //var l3 = btnID3Tag;
            //stackWordsA.Children.Clear();
            //stackWordsA.Children.Add(l3);
        }
        bool hasStackFolderName = false;
        private void GetStackFolderName(string folderName)
        {
            hasStackFolderName = true;

            // Set Album for option
            var l = btnFolderName;
            l.Tag = folderName;

            lblFolderName.Content = folderName;

            //stackWordsB.Children.Clear();
            //stackWordsB.Children.Add(l);

            var searchNames = Global.importFactory.aEngine.GetArtistSearchStrings(folderName, true);
            int index = 0;
            foreach (var s in searchNames)
            {
                //if (index > 0)
                //{
                //    //ToggleButton d = getBreadCrumbButton(s.Delimiter, -1);
                //    AlbumButton.Click += ActivateAlbumButton;

                //    stackWordsB.Children.Add(d);
                //}

                //ToggleButton b = getBreadCrumbButton(s.SearchTerm, index);
                //b.Click += ActivateAlbumButton;

                //stackWordsB.Children.Add(b);
                //index++;
                AlbumButton.Content = s.SearchTerm;
                AlbumButton.Click += ActivateAlbumButton;
            }

            stackWordsB.Visibility = Visibility.Visible;
        }



        public Popup_SmartUpdater()
        {
            InitializeComponent();
            using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
            {
                var t = (from a in db.TrackLibraries
                         where a.Type == "Import - Karaoke"
                         select a).ToList();

                TroublesomeTracks = t.ToList();
                isTracks = true;
                LoadNextSong();
            }
        }

        /// <summary>
        /// Used for tracks that don't have songs yet
        /// </summary>
        /// <param name="trackId"></param>
        public Popup_SmartUpdater(int trackId)
        {
            InitializeComponent();
            using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
            {
                var track = db.TrackLibraries.Find(trackId);

                ManagedFile mf = new ManagedFile(track.FilePath);
                bufferImportTypeString = track.Type;
                mf.SetTypeString = track.Type;
                GetStackFolderName(mf.FolderName);

                hasID3 = mf.hasId3;
                if (hasID3)
                {
                    GetStackID3();
                }

                mf.BaseDBTrackLibrary = db.TrackLibraries.Find(track.Id);
                mf.hasDBTrackLibrary = true;

                if (track.SongLibraries.Any())
                {
                    var song = track.SongLibraries.First();
                    bufferBaseDBSong = song;

                    Title = song.SongName;

                    if (song.Artists.Any())
                    {
                        //mf.song
                        ArtistName = song.Artists.First().ArtistName;

                        if (song.Artists.First().isVerified)
                            ArtistName += " (v)";

                        if (!string.IsNullOrEmpty(song.Artists.First().iTunesId))
                            ArtistName += " (" + song.Artists.First().iTunesId + ")";
                    }
                    else
                    {
                        SetButtonRed(ref btnArtist, true);
                    }
                }
                else
                {
                    Title = track.FileName;
                    SetButtonRed(ref btnArtist, true);
                    AlbumName = "";
                }

                lblFileName.Content = track.FileName + track.Extention;


                TroublesomeSongs = new List<ManagedFile>();
                TroublesomeSongs.Add(mf);
            }

            LoadNextSong();
        }
        public Popup_SmartUpdater(TrackLibrary track)
        {
            InitializeComponent();
            StartupSetUpTrackLibrary(track);
        }

        bool showDetailsButton = false;
        private void StartupSetUpTrackLibrary(TrackLibrary track)
        {
            showDetailsButton = true;

            ResetAll();
            SetInfoMessage(InfoMessage.NoMetaData);

            // Create New Managed File

            //ManagedFile mf = new ManagedFile(track.FilePath);
            ManagedFile mf = new ManagedFile(track);
            bufferImportTypeString = track.Type;
            mf.SetTypeString = track.Type;

            GetStackFolderName(mf.FolderName);

            hasID3 = mf.hasId3;
            if (hasID3)
            {
                GetStackID3();
                //string a =  mf.Id3Tag.Album.Value;
            }


            using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
            {
                // get song stuffs
                mf.BaseDBTrackLibrary = db.TrackLibraries.Find(track.Id);
                mf.hasDBTrackLibrary = true;

                var song = db.TrackLibraries.Find(track.Id).SongLibraries.First();
                bufferBaseDBSong = song;


                Title = song.SongName;
                lblFileName.Content = track.FileName + track.Extention;


                if (song.Artists.Any())
                {
                    SetInfoMessage(InfoMessage.MissingAlbum);
                    //mf.song
                    ArtistName = song.Artists.First().ArtistName;
                    hasArtist = true;
                    bufferBaseDBArtist = song.Artists.First();

                    if (song.Artists.First().isVerified)
                    {
                        ArtistName += " (v)";
                        bufferBaseArtist = new SongArtist() { ArtistId = long.Parse(song.Artists.First().iTunesId), ArtistName = song.Artists.First().ArtistName };
                    }

                    if (!string.IsNullOrEmpty(song.Artists.First().iTunesId))
                        ArtistName += " (" + song.Artists.First().iTunesId + ")";
                }
                else
                {
                    SetButtonRed(ref btnArtist, true);
                }

                if (song.AlbumLibraries.Any())
                {
                    if (currentInfoMessage == InfoMessage.MissingAlbum)
                        SetInfoMessage(InfoMessage.None);
                    else
                        SetInfoMessage(InfoMessage.MissingArtist);

                    string s = song.AlbumLibraries.Count().ToString() + " - ";
                    s += song.AlbumLibraries.First().AlbumName;
                    AlbumName = s;
                }
                else
                {
                    AlbumName = "";
                    SetButtonRed(ref btnAlbum);
                }
            }



            TroublesomeSongs = new List<ManagedFile>();
            TroublesomeSongs.Add(mf);
            LoadNextSong();

            if (Global.AppControlMode != Global.AppControlModeEnum.Admin)
            {
                btnArtist.IsChecked = true;
                btnArtist_Checked(btnArtist, new RoutedEventArgs(ButtonBase.ClickEvent));
            }
            else
            {
                gridAdditionalCommands.Visibility = Visibility.Visible;
            }
            
        }


        public Popup_SmartUpdater(List<ManagedFile> troublesomeSongs)
        {
            InitializeComponent();
            TroublesomeSongs = troublesomeSongs.Select(s => s).ToList();
            LoadNextSong();
        }

        public Popup_SmartUpdater(AlbumLibrary baseAlbum, LibraryCard baseControlCard)
        {
            InitializeComponent();
            hideTrack = true;

            //GetStackFolderName();

            bufferBaseDBAlbum = baseAlbum;
            bufferBaseDBAlbumCard = baseControlCard;
            AlbumName = baseAlbum.AlbumName;

            btnAlbum.IsChecked = true;
            btnAlbum_Checked(btnAlbum, new RoutedEventArgs(ButtonBase.ClickEvent));
        }
        private void LoadNextSong()
        {
            if (baseIndex == -1)
                baseIndex = 0;
            else
                baseIndex++;

            btnNextTrack.IsEnabled = true;

            if (isTracks)
            {
                if (TroublesomeTracks.Any())
                {
                    if (baseIndex + 1 == TroublesomeTracks.Count)
                        btnNextTrack.IsEnabled = false;

                    bufferFile = new ManagedFile(TroublesomeTracks[baseIndex].FilePath) { BaseDBTrackLibrary = TroublesomeTracks[baseIndex] };
                    stackMainWords.Children.Clear();

                    // split into words

                    List<string> searchNames = new List<string>();
                    int searchIndex = 0;

                    searchNames = Global.importFactory.aEngine.GetTrackSplitStrings(bufferFile.FileName);
                    lblBreadCrumbFileName.Content = bufferFile.FileName;

                    string searchName = searchNames[searchIndex];


                    int i = 0;
                    foreach (string s in searchNames)
                    {
                        var wordButton = getWordButton(s, i, i == 0 || i < searchNames.Count - 1 ? "AlbumButtonStyleV1" : "AlbumButtonStyle");
                        wordButton.Margin = new Thickness(0, 0, 5, 0);
                        stackMainWords.Children.Add(wordButton);
                        i++;
                    }
                }
            }
            else
            {
                if (TroublesomeSongs.Any())
                {
                    if (baseIndex + 1 == TroublesomeSongs.Count)
                        btnNextTrack.IsEnabled = false;

                    bufferFile = TroublesomeSongs[baseIndex];
                    stackMainWords.Children.Clear();

                    // split into words

                    List<string> searchNames = new List<string>();
                    int searchIndex = 0;

                    searchNames = Global.importFactory.aEngine.GetTrackSplitStrings(bufferFile.FileName);
                    lblBreadCrumbFileName.Content = bufferFile.FileName;

                    string searchName = searchNames[searchIndex];


                    int i = 0;
                    foreach (string s in searchNames)
                    {
                        var wordButton = getWordButton(s, i, i == 0 || i < searchNames.Count - 1 ? "AlbumButtonStyleV1" : "AlbumButtonStyle");
                        wordButton.Margin = new Thickness(0, 0, 5, 0);
                        stackMainWords.Children.Add(wordButton);
                        i++;
                    }
                }
            }
        }

        private ToggleButton getWordButton(string name, int index,string styleName)
        {
            ToggleButton b = getBreadCrumbButton(name, index);
            //b.Tag = index;
            //b.Content = name;
            b.Click += new RoutedEventHandler(ActivateButton);

            //b.Style = FindResource(Global.ControlStyle_ToggleButton) as Style;
            b.Style = FindResource(styleName) as Style;
            b.FontSize = 16;
            b.Height = 40;
            b.MinWidth = 40;
            //b.MinHeight = 50;
            b.Padding = new Thickness(10, 0, 10, 0);
            return b;
        }

        private void Save()
        {

        }

        private void SelectWholeFolder(object sender, RoutedEventArgs e)
        {
            ToggleButton s = (ToggleButton)sender;
            string tag = s.Tag.ToString();

            if (s.IsChecked.Value)
                searchString = (searchString + " " + tag).Trim();
            else
                searchString = searchString.Replace(tag, "").Trim();
        }
        private void ActivateAlbumButton(object sender, EventArgs e)
        {
            ToggleButton s = (ToggleButton)sender;
            if (s.Tag != null)
                baseIndex = (int)s.Tag;
            string c = s.Content.ToString();

            if (s.IsChecked.Value && !searchString.Contains(c.Trim()))
                searchString = (searchString + " " + c).Trim();
            else
                searchString = searchString.Replace(c, "").Trim();
        }
        private void ActivateButton(object sender, EventArgs e)
        {
            ToggleButton s = (ToggleButton)sender;
            if (s.Tag != null)
                baseIndex = (int)s.Tag;
            string c = s.Content.ToString();

            if (s.IsChecked.Value && !searchString.Contains(c.Trim()))
            {
                searchString = (searchString.Trim() + " " + c.Trim()).Trim();
            }
            else
            {
                searchString = searchString.Replace(c, "").Trim();
            }

            bufferButton = s;
        }

        private void btnSearchTrack_Click(object sender, RoutedEventArgs e)
        {
            btnSearchTrack.IsEnabled = false;
            iTunesSearchManager iManager = new iTunesSearchManager();
            iManager.SearchTrack(baseString, bufferBaseArtist, bufferFile);
            //iManager.SearchArtist("Jonathan Young");
        }

        private void btnSearchArtist_Click(object sender, RoutedEventArgs e)
        {
            btnSearchArtist.IsEnabled = false;
            SetInfoMessage(InfoMessage.Searching);
            gridEdit.Visibility = Visibility.Hidden;

            iTunesSearchManager iManager = new iTunesSearchManager();
            iManager.SearchArtist(baseString);
            //iManager.SearchArtist("Jonathan Young");


        }

        private void btnFuseLeft(object sender, RoutedEventArgs e)
        {

        }

        private void btnFuseRight(object sender, RoutedEventArgs e)
        {

        }

        private void btnNextTrack_Click(object sender, RoutedEventArgs e)
        {
            LoadNextSong();
        }

        private void btnEditName_Click(object sender, RoutedEventArgs e)
        {
            //tbSelectedName.Visibility = Visibility.Visible;
            lblSelectedName.Visibility = Visibility.Collapsed;
        }

        private void btnSearchAlbum_Click(object sender, RoutedEventArgs e)
        {
            btnSearchAlbum.IsEnabled = false;
            SetInfoMessage(InfoMessage.Searching);
            gridEdit.Visibility = Visibility.Hidden;

            iTunesSearchManager iManager = new iTunesSearchManager();
            //iManager.SearchArtist(baseString);
            string s = "";
            string sId = "";
            if (hasArtist)
            {
                s = bufferBaseArtist.ArtistName;
                sId = bufferBaseArtist.ArtistId.ToString();
            }
            iManager.SearchAlbum(bufferFile, baseString, s, sId);
        }

        private void btnTitle_Checked(object sender, RoutedEventArgs e)
        {
            btnArtist.IsChecked = false;
            btnAlbum.IsChecked = false;
            btnDetails.IsChecked = false;
            ResetAll();
            gridEdit.Visibility = Visibility.Visible;
            gridAdditionalCommands.Visibility = Visibility.Collapsed;


            if (hasID3)
                stackWordsA.Visibility = Visibility.Visible;
            stackWordsB.Visibility = Visibility.Visible;
            stackMainWords.Visibility = Visibility.Visible;
            lblSelectedName.Visibility = Visibility.Visible;

            btnSearchTrack.Visibility = Visibility.Visible;

            searchString = "";
        }

        private void btnArtist_Checked(object sender, RoutedEventArgs e)
        {
            btnTitle.IsChecked = false;
            btnAlbum.IsChecked = false;
            btnDetails.IsChecked = false;
            ResetAll();

            gridEdit.Visibility = Visibility.Visible;
            gridAdditionalCommands.Visibility = Visibility.Collapsed;

            stackMainWords.Visibility = Visibility.Visible;
            lblSelectedName.Visibility = Visibility.Visible;
            btnSearchArtist.Visibility = Visibility.Visible;
            btnSearchArtist.IsEnabled = true;

            if (hasID3)
                stackWordsA.Visibility = Visibility.Visible;
            stackWordsB.Visibility = Visibility.Visible;

            searchString = "";
        }

        private void btnAlbum_Checked(object sender, RoutedEventArgs e)
        {
            btnArtist.IsChecked = false;
            btnTitle.IsChecked = false;
            btnDetails.IsChecked = false;
            ResetAll();
            gridEdit.Visibility = Visibility.Visible;
            gridAdditionalCommands.Visibility = Visibility.Collapsed;

            stackMainWords.Visibility = Visibility.Visible;
            lblSelectedName.Visibility = Visibility.Visible;
            if (hasID3)
                stackWordsA.Visibility = Visibility.Visible;
            stackWordsB.Visibility = Visibility.Visible;

            btnSearchAlbum.Visibility = Visibility.Visible;
            searchString = "";
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            // doesn't have a db song..

            iTunesSearchManager.iManagedTrack iTrack = new iTunesSearchManager.iManagedTrack();
            if (bufferBaseDBSong != null)
            {
                iTrack.DBSong = bufferBaseDBSong;
                iTrack.PrimarySearchOption = new SearchAnalysisRule();
                //ExtractSearchNameList(song.SongName);
                iTrack.ExtractSearchArtistList(bufferBaseDBSong.SongName);
            }
            else
            {
                iTrack.PrimarySearchOption = bufferRule;

            }

            if (bufferSong == null)
            {
                // Save data to DB
                ManualSaveToDB();
            }
            else
            {
                iTrack.SaveResultsData(bufferSong, bufferFile);
                bufferAlbumSongs.Songs.Remove(bufferSong);
            }

            gridTrackInfo.Visibility = Visibility.Collapsed;
            //stackMainToggleButtons.Visibility = Visibility.Collapsed;
            stackSaveUpdate.Visibility = Visibility.Collapsed;
            gridSaveApply.Visibility = Visibility.Visible;
            stackSaveApply.Visibility = Visibility.Visible;

            SetUpToApplyResults();
        }

        private void ManualSaveToDB(ReplacementSong rSong = null)
        {
            // Save data to DB
            using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
            {
                SongLibrary s = new SongLibrary();

                // SAVE DB! NEED TO BUFFER FOR OTHER SONGS
                // Get updated version
                if (rSong != null)
                {
                    var a1 = db.TrackLibraries.Find(rSong.DBTrack.Id);
                    s = a1.SongLibraries.First();
                }
                else
                {
                    // Should only kick in with the main save
                    s = db.SongLibraries.Find(bufferBaseDBSong.SongId);
                }


                AlbumLibrary a = new AlbumLibrary();
                if (bufferBaseDBAlbum == null)
                {
                    // CREATE NEW ALBUM
                    a.AlbumName = AlbumName;
                    a.hasiTunesUpdate = false;
                    a.hasMusicBrainzUpdate = false;
                    a.isHidden = false;
                    a.isVerified = false;
                    //a.SongLibraries.Add(bufferBaseDBSong);
                    db.AlbumLibraries.Add(a);
                    db.SaveChanges();

                    bufferBaseDBAlbum = a;
                }
                else
                {
                    // reload data
                    var aB = db.AlbumLibraries.Find(bufferBaseDBAlbum.AlbumId);
                    bufferBaseDBAlbum = aB;
                }

                // CREATE ARTIST
                //bufferBaseArtist.
                if (bufferBaseDBArtist == null)
                {
                    Artist ar = new Artist();
                    ar.ArtistName = ArtistName;
                    ar.hasiTunesUpdate = false;
                    ar.isVerified = false;
                    db.Artists.Add(ar);
                    db.SaveChanges();

                    bufferBaseDBArtist = ar;
                }
                else
                {
                    var arB = db.Artists.Find(bufferBaseDBArtist.Id);
                    bufferBaseDBArtist = arB;
                }

                if (rSong == null)
                    s.SongName = lblMainTrackName.Content.ToString();
                else
                {
                    var r = bufferRule.Clone(rSong.DBTrack.FileName);
                    s.SongName = r.SearchTerm;
                }

                s.AlbumLibraries.Add(bufferBaseDBAlbum);
                s.Artists.Add(bufferBaseDBArtist);


                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
            }
        }

        private void SetButtonGreen(ref Button button)
        {
            //button.Background = btnGreenTemplate.Background;
            //button.BorderBrush = btnGreenTemplate.BorderBrush;
        }
        private void SetButtonGreen(ref ToggleButton toggleButton)
        {
            //toggleButton.Background = btnGreenTemplate.Background;
            //toggleButton.BorderBrush = btnGreenTemplate.BorderBrush;
        }
        private void SetButtonRed(ref Button button, bool isFlashing = false)
        {
            //button.Background = btnRedTemplate.Background;
            //button.BorderBrush = btnRedTemplate.BorderBrush;


            if (isFlashing)
            {
                //Global.SetButtonFlashing(ref button);
            }
            //button.Triggers.Add()
        }
        private void SetButtonRed(ref ToggleButton toggleButton, bool isFlashing = false)
        {
            //toggleButton.Background = btnRedTemplate.Background;
            //toggleButton.BorderBrush = btnRedTemplate.BorderBrush;

            if (isFlashing)
            {
                //Global.SetButtonFlashing(ref toggleButton);
            }
        }
        private void SetButtonNormal(ref Button button)
        {
            button.Background = btnTitle.Background;
            button.BorderBrush = btnTitle.BorderBrush;
        }
        private void SetButtonNormal(ref ToggleButton toggleButton)
        {
            toggleButton.Background = btnTitle.Background;
            toggleButton.BorderBrush = btnTitle.BorderBrush;
        }

        public class ReplacementSong
        {
            public bool isBruteRuleVersion { get { return NewSong == null; } }
            public TrackLibrary DBTrack { get; set; }
            //public SearchAnalysisRule NewRule { get; set; }
            public Song NewSong { get; set; }
            public TrackLibrary OldTrack { get; internal set; }
        }
        List<ReplacementSong> replacementSongs = new List<ReplacementSong>();
        int replacementSongsPage = 0;
        int replacementSongsPageCount = 8;
        private void SetUpToApplyResults()
        {
            /// stackSaveApply

            // reloads the library underneath
            bool isReLoadtime = false;
            bool isReadingFromDir = false;

            if (bufferAlbumSongs == null)
                isReadingFromDir = true;
            // list and match all remaining songs... code from last night...
            // we'll wing it as we go.. so for now, using the folder name

            //public Popup_Choices(string folderName, string Type, List<Song> songs, string trackFormat)

            using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
            {
                // Folder Name
                // File Structure

                string folderName = bufferFile.FolderPath + "\\";
                int driveIndex = folderName.IndexOf("\\");
                string drive = folderName.Substring(0, driveIndex + 1);

                string folderNameB = folderName.Replace(drive, drive + "\\");
                //folderName = folderName.Replace(drive, drive + "\\");
                string Type = bufferImportTypeString;
                //string Type = bufferFile.ImportTypeString;

                string AlbumId = "";
                if (bufferBaseAlbum == null)
                {
                }
                else
                {
                    AlbumId = bufferBaseAlbum.CollectionId.ToString();
                }


                var a = (from b in db.TrackLibraries
                         where b.Type == Type && ((folderName + b.FileName + b.Extention) == b.FilePath || (folderNameB + b.FileName + b.Extention) == b.FilePath)
                         select b).ToList();

                if (!a.Any())
                    isReLoadtime = true;

                foreach (var a1 in a)
                {
                    if (isReadingFromDir)
                    {
                        if (!(a1.SongLibraries.First().Artists.Any() && a1.SongLibraries.First().AlbumLibraries.Any()))
                        {
                            var searchName = bufferRule.Clone(a1.FileName);
                            replacementSongs.Add(new ReplacementSong() { DBTrack = a1, OldTrack = a1 });
                        }
                    }
                    else
                    {
                        bool hasThisAlbum = (a1.SongLibraries.Any() && a1.SongLibraries.First().AlbumLibraries.Where(w => w.iTunesId == AlbumId).Any());
                        if (hasThisAlbum)
                        {
                            //SearchAnalysisRule searchName = bufferRule.Clone(a1.FileName);
                            if (bufferAlbumSongs.Songs.Where(w => w.TrackName == a1.SongLibraries.First().SongName).Any())
                            {
                                var song = bufferAlbumSongs.Songs.Where(w => w.TrackName == a1.SongLibraries.First().SongName).First();
                                bufferAlbumSongs.Songs.Remove(song);
                            }
                        }
                        else
                        {
                            //var searchName = Global.importFactory.aEngine.GetTrackSearchStrings(a1.FileName).First();
                            var searchName = bufferRule.Clone(a1.FileName);

                            if (searchName.isPatternMatch)
                            {

                                foreach (var song in bufferAlbumSongs.Songs)
                                {
                                    if (song.TrackName != null)
                                    {
                                        bool found = false;
                                        string searchMatch = string.Format(baseFileSearchPattern, song.ArtistName.ToLower(), song.TrackName.ToLower(), song.TrackNumber.ToString().PadLeft(2, '0'), song.CollectionName.ToLower());

                                        string f = baseFileSearchPattern.Replace("{0}", "");
                                        if (f.Contains('{'))
                                        {
                                            f = f.Substring(f.IndexOf('{'));
                                            f = f.Substring(0, f.LastIndexOf('}') + 1);
                                        }
                                        string searchMatch2 = string.Format(f, song.ArtistName.ToLower(), song.TrackName.ToLower(), song.TrackNumber.ToString().PadLeft(2, '0'), song.CollectionName.ToLower());

                                        switch (getTrackEqual(searchName.SearchTerm, searchMatch))
                                        {
                                            case CompareResults.Match:
                                                replacementSongs.Add(new ReplacementSong() { NewSong = song, OldTrack = a1 });
                                                bufferAlbumSongs.Songs.Remove(song);
                                                found = true;
                                                break;
                                            case CompareResults.MatchWithExtra:
                                                replacementSongs.Add(new ReplacementSong() { NewSong = song, OldTrack = a1 });
                                                bufferAlbumSongs.Songs.Remove(song);
                                                found = true;
                                                break;
                                            case CompareResults.NoMatch:
                                                break;
                                        }

                                        if (found == false)
                                        {
                                            Regex rgx = new Regex("[^a-zA-Z0-9 ]");

                                            string s1 = searchName.SearchTerm.ToLower().Replace('-', ' ');
                                            s1 = rgx.Replace(s1, "").Replace(",", "").Replace(".", "");
                                            s1 = SuperTrim(s1);

                                            string s2 = searchMatch2.ToLower();
                                            s2 = rgx.Replace(s2, "").Replace(",", "").Replace(".", "");
                                            s2 = SuperTrim(s2);

                                            if (s1.Contains(s2))
                                            {
                                                replacementSongs.Add(new ReplacementSong() { NewSong = song, OldTrack = a1 });
                                                bufferAlbumSongs.Songs.Remove(song);
                                                found = true;
                                            }
                                        }
                                        if (found)
                                            break;
                                    }
                                }
                            }
                            /// check if starts with number
                            else if (searchName.isPossibleTrackNumber)
                            {
                                if (bufferAlbumSongs.Songs.Where(w => w.TrackNumber == searchName.TrackNumber).Any())
                                {
                                    var song = bufferAlbumSongs.Songs.Where(w => w.TrackNumber == searchName.TrackNumber).First();
                                    int iCount = bufferAlbumSongs.Songs.Where(w => w.TrackNumber == searchName.TrackNumber).Count();
                                    if (a1.SongLibraries.First().AlbumLibraries.Any())
                                    {

                                    }
                                    else
                                    {
                                        switch (getTrackEqual(searchName.SearchTerm, song.TrackName))
                                        {
                                            case CompareResults.Match:
                                                replacementSongs.Add(new ReplacementSong() { NewSong = song, OldTrack = a1 });
                                                bufferAlbumSongs.Songs.Remove(song);
                                                break;
                                            case CompareResults.MatchWithExtra:
                                                replacementSongs.Add(new ReplacementSong() { NewSong = song, OldTrack = a1 });
                                                bufferAlbumSongs.Songs.Remove(song);
                                                break;
                                            case CompareResults.MatchSpellingError:
                                                replacementSongs.Add(new ReplacementSong() { NewSong = song, OldTrack = a1 });
                                                bufferAlbumSongs.Songs.Remove(song);
                                                break;
                                            case CompareResults.NoMatch:
                                                break;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                bool found = false;
                                foreach (var song in bufferAlbumSongs.Songs)
                                {
                                    switch (getTrackEqual(searchName.SearchTerm, song.TrackName))
                                    {
                                        case CompareResults.Match:
                                            replacementSongs.Add(new ReplacementSong() { NewSong = song, OldTrack = a1 });
                                            bufferAlbumSongs.Songs.Remove(song);
                                            found = true;
                                            break;
                                        case CompareResults.MatchWithExtra:
                                            replacementSongs.Add(new ReplacementSong() { NewSong = song, OldTrack = a1 });
                                            bufferAlbumSongs.Songs.Remove(song);
                                            found = true;
                                            break;
                                        case CompareResults.NoMatch:
                                            break;
                                    }
                                    if (found)
                                        break;
                                }
                            }
                        }
                    }
                }

                //var lbl = lblMain;
                //mainStack.Children.Clear();
                //lbl.Content = TrackTypes[TrackTypeIndex];
                //mainStack.Children.Add(lbl);
                //string t = TrackTypes[TrackTypeIndex];
                //var songlist = (from w in db.TrackLibraries
                //                where w.Type == t
                //                select w).ToList();



                //foreach (var ax in songlist)
                //{
                //Button b = new Button();
                //int songcount = a.SongLibraries == null ? 0 : a.SongLibraries.Count();
                //b.Content = string.Format("{0} - Song Count :{1}", a.FileName, songcount);
                //b.Click += loadSmartUpdater;
                //b.Tag = a.Id;
                //mainStack.Children.Add(b);
                //}

                //Button c = new Button();
                //c.Content = "Load next set";
                //c.Click += nextTrackType;
                //mainStack.Children.Add(c);
            }

            if (replacementSongs.Any())
            {
                Button bDone = getApplyChangesTrackButton("Skip", "skip");
                bDone.Click += ApplyResultsToTrack;
                bDone.HorizontalAlignment = HorizontalAlignment.Center;
                SetButtonGreen(ref bDone);
                stackSaveApply.Children.Add(bDone);

                foreach (var r in replacementSongs.Skip(replacementSongsPage * replacementSongsPageCount).Take(replacementSongsPageCount))
                {
                    Button b = getApplyChangesTrackButton("", r.OldTrack.Id.ToString());
                    if (r.isBruteRuleVersion)
                        b.Content = r.OldTrack.FileName;
                    else
                        b.Content = r.OldTrack.FileName + " --> " + r.NewSong.TrackName;

                    b.Click += ApplyResultsToTrack;

                    stackSaveApply.Children.Add(b);
                }

                if (replacementSongs.Count() > replacementSongsPageCount)
                {
                    Button bNext = getApplyChangesTrackButton("Next >>", "next");
                    bNext.Click += btnPageNext_ReplacementSongsClick;
                    bNext.HorizontalAlignment = HorizontalAlignment.Center;
                    SetButtonRed(ref bNext);
                    stackSaveApply.Children.Add(bNext);
                }
            }
            else
            {
                ResetAll();
                ((Library)Global.MainFrame.Content).ReloadLibrary(true);
            }


            if (isReLoadtime)
                ((Library)Global.MainFrame.Content).ReloadLibrary(true);
        }



        private void btnPageNext_ReplacementSongsClick(object sender, RoutedEventArgs e)
        {
            stackSaveApply.Children.Clear();
            //Button bDone = getApplyChangesTrackButton("Skip", "skip");
            //bDone.Click += ApplyResultsToTrack;
            //bDone.HorizontalAlignment = HorizontalAlignment.Center;
            //SetButtonGreen(ref bDone);
            //stackSaveApply.Children.Add(bDone);
            replacementSongsPage++;

            foreach (var r in replacementSongs.Skip(replacementSongsPage * replacementSongsPageCount).Take(replacementSongsPageCount))
            {
                Button b = getApplyChangesTrackButton("", r.OldTrack.Id.ToString());
                if (r.isBruteRuleVersion)
                    b.Content = r.OldTrack.FileName;
                else
                    b.Content = r.OldTrack.FileName + " --> " + r.NewSong.TrackName;

                b.Click += ApplyResultsToTrack;

                stackSaveApply.Children.Add(b);
            }

            if (replacementSongs.Count() > replacementSongsPageCount)
            {
                Button bNext = getApplyChangesTrackButton("Next >>", "next");
                bNext.Click += btnPageNext_ReplacementSongsClick;
                bNext.HorizontalAlignment = HorizontalAlignment.Center;
                SetButtonRed(ref bNext);
                stackSaveApply.Children.Add(bNext);
            }
        }

        private void ApplyResultsToTrack(object sender, RoutedEventArgs e)
        {
            Button b = (Button)sender;
            string command = b.Tag.ToString();
            int id = 0;

            if (int.TryParse(b.Tag.ToString(), out id))
            {
                var r = replacementSongs.Where(w => w.OldTrack.Id == id).First();

                ManagedFile man = new ManagedFile(r.OldTrack);

                if (r.isBruteRuleVersion)
                {
                    ManualSaveToDB(r);
                }
                else
                {
                    iTunesSearchManager.iManagedTrack iTrack = new iTunesSearchManager.iManagedTrack();
                    iTrack.SaveResultsData(r.NewSong, man, true);
                }

                stackSaveApply.Children.Remove((UIElement)sender);
            }



            // added increment because of present skip button
            if (stackSaveApply.Children.Count == 2 || command == "skip")
            {
                ResetAll();
                //((Library)Global.MainFrame.Content).ReloadLibrary();
                ((Library)Global.MainFrame.Content).ReloadLibrary(true);
            }
        }

        bool hideTrack = false;
        public void ResetAll()
        {
            gridDetails.Visibility = Visibility.Collapsed;
            gridInstruction.Visibility = Visibility.Collapsed;
            baseString = "";
            baseIndex = -1;

            //stackMainToggleButtons.Visibility = Visibility.Visible;
            gridTrackInfo.Visibility = Visibility.Visible;

            gridEdit.Visibility = Visibility.Collapsed;

            btnSetAlbum.Visibility = Visibility.Collapsed;
            btnSetArtist.Visibility = Visibility.Collapsed;
            btnSetTrack.Visibility = Visibility.Collapsed;

            btnSearchAlbum.Visibility = Visibility.Collapsed;
            btnSearchArtist.Visibility = Visibility.Collapsed;
            btnSearchTrack.Visibility = Visibility.Collapsed;
            btnSave.Visibility = Visibility.Collapsed;

            gridSaveApply.Visibility = Visibility.Collapsed;
            stackSaveApply.Visibility = Visibility.Collapsed;
            stackSaveUpdate.Visibility = Visibility.Collapsed;

            if (hideTrack)
            {
                btnTitle.Visibility = Visibility.Collapsed;
                lblMainTrackName.Visibility = Visibility.Collapsed;
            }

            if (Global.AppControlMode == Global.AppControlModeEnum.Admin)
            {
                gridAdditionalCommands.Visibility = Visibility.Visible;
            }
            else
            {
                gridAdditionalCommands.Visibility = Visibility.Collapsed;
            }

            if (showDetailsButton) btnDetails.Visibility = Visibility.Visible;
            else btnDetails.Visibility = Visibility.Collapsed;
        }



        private ToggleButton getBreadCrumbButton(string Content, int tag)
        {
            return getBreadCrumbButton(Content, tag.ToString());
        }
        private ToggleButton getBreadCrumbButton(string Content, string tag)
        {
            ToggleButton t = new ToggleButton();
            t.Style = FindResource(Global.ControlStyle_AlbumButton) as Style;
            t.Content = Content;
            //t.Padding = new Thickness(0, 0, 0, 8);
            //t.Margin = new Thickness(2, 0, 2, 0);

            t.Height = 50;
            //b.Width = 40;
            t.MinWidth = 40;
            t.FontSize = 20;

            t.Style = FindResource(Global.ControlStyle_ToggleButton) as Style;

            return t;
        }

        private Button getApplyChangesTrackButton(string Content, string tag)
        {
            Button t = new Button();

            t.Content = Content;
            t.Tag = tag;
            //t.Padding = new Thickness(0, 0, 0, 8);
            //t.Margin = new Thickness(2, 0, 2, 0);

            t.Height = 50;
            //b.Width = 40;
            t.MinWidth = 40;
            t.FontSize = 20;

            t.Style = FindResource(Global.ControlStyle_Button) as Style;

            return t;
        }


        private void btnArtist_Unchecked(object sender, RoutedEventArgs e)
        {
            ResetAll();
        }

        private void btnAlbumDetails_Click(object sender, RoutedEventArgs e)
        {
            btnAlbum.IsChecked = true;
            btnAlbum_Checked(btnAlbum, new RoutedEventArgs(ButtonBase.ClickEvent));
        }

        private void btnArtistDetails_Click(object sender, RoutedEventArgs e)
        {
            btnArtist.IsChecked = true;
            btnArtist_Checked(btnArtist, new RoutedEventArgs(ButtonBase.ClickEvent));
        }

        private void btnTrackDetails_Click(object sender, RoutedEventArgs e)
        {
            btnTitle.IsChecked = true;
            btnTitle_Checked(btnTitle, new RoutedEventArgs(ButtonBase.ClickEvent));
        }

        internal string setTextInput
        {
            set { searchString = value; }
        }
        private void btnTextEdit_Click(object sender, RoutedEventArgs e)
        {
            Global.mPlayer.BumpPopUp(new Popup_TextInput(Popup_TextInput.ControlMode.TextEdit, searchString));
        }

        private void btnSetTrack_Click(object sender, RoutedEventArgs e)
        {
            hasTrack = true;
            Global.StopButtonFlashing(ref btnTitle);
            //AlbumName = searchString;

            Title = searchString;
            //lblMainTrackName.Foreground = btnGreenTemplate.BorderBrush;
            ImportFactory.SearchAnalysisRule rule = new SearchAnalysisRule(ArtistName, searchString);

            // Can't remember if there's automated code.. but this will do. See if format starts with track number
            var possibleTrackStart = bufferFile.FileName.ToLower().Replace(searchString.ToLower(), "");
            int i = 0;
            if (int.TryParse(possibleTrackStart, out i))
            {
                rule.isPossibleTrackNumber = true;
                rule.TrackNumber = i;
            }

            bufferRule = rule;

            ResetAll();
            ConfirmUpdate = true;
        }

        private void btnSetArtist_Click(object sender, RoutedEventArgs e)
        {
            hasArtist = true;
            ArtistName = searchString;
            lblMainAlbumName.BorderBrush = btnSearchArtist.BorderBrush;
            lblMainAlbumName.Background = btnSearchArtist.Background;
        }

        private void btnSetAlbum_Click(object sender, RoutedEventArgs e)
        {
            Global.StopButtonFlashing(ref btnAlbum);
            bool bypassActualSave = true;

            hasAlbum = true;
            AlbumName = searchString;
            //lblMainAlbumName.Foreground = btnGreenTemplate.BorderBrush;


            if (bufferBaseAlbum == null)
            {

            }
            else
            {
                btnSearchAlbum.Content = bufferBaseAlbum.CollectionName;
                AlbumName = bufferBaseAlbum.CollectionName;
            }

            gridInstruction.Visibility = Visibility.Collapsed;

            if (!bypassActualSave)
                using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                {
                    AlbumLibrary a = new AlbumLibrary();
                    a.AlbumName = AlbumName;
                    a.hasiTunesUpdate = false;
                    a.hasMusicBrainzUpdate = false;
                    a.isHidden = false;
                    a.isVerified = false;
                    a.SongLibraries.Add(bufferBaseDBSong);
                    db.AlbumLibraries.Add(a);
                    db.SaveChanges();

                    bufferBaseDBAlbum = a;
                }

            ResetAll();

            //Global.SetButtonFlashing(ref btnTitle);
            SetInfoMessage(InfoMessage.MissingTrack);

            if (hasTrack)
            {
                //btnSaveDB.Visibility = Visibility.Visible;

                // Extract search pattern
                string pattern = bufferFile.FileName.ToLower();

                if (pattern.Contains(bufferBaseSong.ArtistName.ToLower()))
                {
                    pattern = pattern.Replace(bufferBaseSong.ArtistName.ToLower(), "{0}");
                }
                if (pattern.Contains(bufferBaseSong.TrackName.ToLower()))
                {
                    pattern = pattern.Replace(bufferBaseSong.TrackName.ToLower(), "{1}");
                }
                if (pattern.Contains("0" + bufferBaseSong.TrackNumber.ToString()))
                {
                    pattern = pattern.Replace("0" + bufferBaseSong.TrackNumber.ToString(), "{2}");
                }
                else if (pattern.Contains(bufferBaseSong.TrackNumber.ToString()))
                    pattern = pattern.Replace(bufferBaseSong.TrackNumber.ToString(), "{2}");

                if (pattern.Contains(bufferBaseSong.CollectionName.ToLower()))
                {
                    pattern = pattern.Replace(bufferBaseSong.CollectionName.ToLower(), "{3}");
                }

                baseFileSearchPattern = pattern;
                SongAlbumMatch(pattern);
            }
            else
            {
                // Match Song with records
                //SongAlbumMatch();


            }


            //lblMainAlbumName.BorderBrush = btnSearchAlbum.BorderBrush;
            //lblMainAlbumName.Background = btnSearchAlbum.Background;
        }

        private void btnSaveDB_Click(object sender, RoutedEventArgs e)
        {
            iTunesSearchManager.iManagedTrack iTrack = new iManagedTrack(bufferFile);
            iTrack.SaveResultsData(bufferBaseSong, bufferFile);
        }

        // Analysis Rule..
        // 


        private void DeleteMetadata()
        {
            ManagedFile mf = TroublesomeSongs.First();

            // Add Confirm Notification
            mf.ResetMetaData();

            // Reload this card
            var track = mf.BaseDBTrackLibrary;
            StartupSetUpTrackLibrary(track);
            ((Library)Global.MainFrame.Content).ReloadLibrary(true);
        }

        private void DeleteRecord_AreYouSure()
        {
            //ManagedFile mf = new ManagedFile(TroublesomeTracks.First());
            ManagedFile mf = TroublesomeSongs.First();

            // Add Confirm Notification
            mf.DeleteTrack();
            // reload library
            ((Library)Global.MainFrame.Content).ReloadLibrary(true);
            Global.mPlayer.TogglePopUp();
        }
        private void DeleteRecord()
        {
            // check if only on track
            if (TroublesomeSongs.Count == 1)
            {
                using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                {
                    var t = TroublesomeSongs.First().BaseDBTrackLibrary;
                    var s = t.SongLibraries.First();
                    // check if album needs deleting
                    int i2 = s.AlbumLibraries.Count();

                    // check if artist needs deleting
                    int i3 = s.Artists.Count();
                    var a = s.Artists.First();

                    int ai = a.SongLibraries.Count();
                    s.Artists.Remove(a);

                    db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();


                    // remove song
                    // remove track from playlists
                    // remove track fom db
                }
            }
        }

        private void btnDeleteTrack_Click(object sender, RoutedEventArgs e)
        {
            DeleteRecord_AreYouSure();
        }

        private void btnResetMetaData_Click(object sender, RoutedEventArgs e)
        {
            DeleteMetadata();
        }

        private void btnDetails_Checked(object sender, RoutedEventArgs e)
        {
            btnTitle.IsChecked = false;
            btnAlbum.IsChecked = false;
            btnArtist.IsChecked = false;

            ResetAll();

            gridInstruction.Visibility = Visibility.Collapsed;
            gridDetails.Visibility = Visibility.Visible;

            //gridEdit.Visibility = Visibility.Visible;
            //gridAdditionalCommands.Visibility = Visibility.Collapsed;

            //stackMainWords.Visibility = Visibility.Visible;
            //lblSelectedName.Visibility = Visibility.Visible;
            //btnSearchArtist.Visibility = Visibility.Visible;
            //btnSearchArtist.IsEnabled = true;

            //if (hasID3)
            //    stackWordsA.Visibility = Visibility.Visible;
            //stackWordsB.Visibility = Visibility.Visible;

            //searchString = "";
        }

        private void btnDetails_Unchecked(object sender, RoutedEventArgs e)
        {
            ResetAll();
        }

        private void btnDetailsID3_Click(object sender, RoutedEventArgs e)
        {

            //TroublesomeSongs.First().FilePath;
            string filepath = bufferFile.FilePath;
            List<string> s = Global.importFactory.getID3Details(filepath);

            stackDetails.Children.Clear();
            foreach (var s2 in s)
            {
                stackDetails.Children.Add(new Label() { Content = s2, Foreground = Brushes.White });
            }
        }

        private void btnDetailsSiblings_Click(object sender, RoutedEventArgs e)
        {
            stackDetails.Children.Clear();

            // Get Folder
            string foldername = bufferFile.FolderName;
            string filename = bufferFile.FileName + bufferFile.FileExtention;
            stackDetails.Children.Add(new Label() { Content = "Folder Name: " + foldername, Foreground = Brushes.White });
            // Show THIS File
            stackDetails.Children.Add(new Label() { Content = "This file: " + filename, Foreground = Brushes.White });
            stackDetails.Children.Add(new Label() { Content = "----------------------------------", Foreground = Brushes.White });

            SearchOption s = SearchOption.TopDirectoryOnly;
            string TargetDir = bufferFile.FolderPath;
            var m = Directory.EnumerateFiles(TargetDir, "*", s).ToList();

            using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
            {
                var dblistings = db.TrackLibraries.Where(w => m.Contains(w.FilePath) && w.FilePath != bufferFile.FilePath).ToList();
                var dblistingsPath = db.TrackLibraries.Where(w => m.Contains(w.FilePath) && w.FilePath != bufferFile.FilePath).Select(sx => sx.FilePath).ToList();

                List<string> unlisted = m.ToList();
                unlisted.RemoveAll(r => dblistingsPath.Contains(r));
                unlisted.Remove(bufferFile.FilePath);

                foreach (var l in dblistings)
                {
                    // Check siblings in DB 
                    if (l.SongLibraries.Any())
                    {
                        var a = l.SongLibraries.First().AlbumLibraries.Select(sx => sx.AlbumName);
                        if (a.Any())
                        {
                            foreach (var ax in a)
                                stackDetails.Children.Add(new Label() { Content = string.Format("{0} -- {1}", l.FileName + l.Extention, ax), Foreground = Brushes.White });
                        }
                    }
                    else
                        stackDetails.Children.Add(new Label() { Content = string.Format("{0} -- No Album", l.FileName + l.Extention), Foreground = Brushes.White });
                }

                foreach (var u in unlisted.Take(10))
                {
                    stackDetails.Children.Add(new Label() { Content = string.Format("Not in Database -- {0}", new DirectoryInfo(u).Name),Foreground=Brushes.White });
                }

                if (unlisted.Count > 10)
                {
                    stackDetails.Children.Add(new Label() { Content = string.Format("... + {0} more", unlisted.Count-10), Foreground = Brushes.White });
                }
            }
        }

        private void btnOpenFolder_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(bufferFile.FolderPath);
        }

        private void btnResetData_Click(object sender, MouseButtonEventArgs e)
        {
            btnResetMetaDataExpanded.Visibility = btnResetMetaDataExpanded.Visibility==Visibility.Collapsed?Visibility.Visible:Visibility.Collapsed;
            if (btnResetMetaDataExpanded.Visibility == Visibility.Visible)
            {
                btnDeleteExpanded.Visibility = Visibility.Collapsed;
                btnDeleteTrack.Visibility = Visibility.Visible;
            }
        }
        private void btnDeleteData_Click(object sender, MouseButtonEventArgs e)
        {
            btnDeleteExpanded.Visibility = btnDeleteExpanded.Visibility==Visibility.Collapsed?Visibility.Visible:Visibility.Collapsed;
            if (btnDeleteExpanded.Visibility == Visibility.Visible)
            {
                btnResetMetaDataExpanded.Visibility = Visibility.Collapsed;
                btnResetMetaData.Visibility = Visibility.Visible;
            }
        }

        private void ClosePopup(object sender, MouseButtonEventArgs e)
        {
            Global.mPlayer.popUp_Grid.Visibility = Visibility.Collapsed;
            Global.mPlayer.popUp_Frame.Content = null;
            Global.MainWindow.btnLogo.IsEnabled = true;
        }
    }
}
