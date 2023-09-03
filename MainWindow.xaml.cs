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

using System.Configuration;
using System.Collections.Specialized;
using Vlc.DotNet.Wpf;
using Vlc.DotNet.Core;
using System.Reflection;
using System.Collections;
using System.Threading;
using JukeBoxSolutions.Controls;
using System.Diagnostics;
using JukeBoxSolutions.Class;
using System.Net.NetworkInformation;
using JukeBoxSolutions.Pages;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Animation;
using Vlc.DotNet.Core;
using iTunesSearch.Library;
using System.Windows.Threading;
using System.ComponentModel;
using Id3;
using System.Data.SqlClient;
using iTunesSearch.Library.Models;
using System.Windows.Media.Effects;
using static JukeBoxSolutions.Library.PlayListSource2;

namespace JukeBoxSolutions
{
    internal static class Global
    {
        internal static ImportAnalyticsClass ImportAnalytics = new ImportAnalyticsClass();
        public class ImportAnalyticsClass
        {
            // Batch
            public List<ImportAnalyticsFile> DBAnalytics = new List<ImportAnalyticsFile>();
            internal List<ImportAnalyticsBatch> DBAnalyticsBatch = new List<ImportAnalyticsBatch>();

            // Track Id
            // Breadcrumbs
            //internal void NewBatch(IEnumerable<string> enumerable, string v, Func<string> toShortDateString, Func<string> toShortTimeString,int rawCount, int musicCount, int videoCount, int karaokeCount, string playlistName)
            internal int NewBatch(int rawCount, int musicCount, int videoCount, int karaokeCount, string playlistName)
            {
                //  throw new NotImplementedException();
                int ID = DBAnalyticsBatch.Count();
                DBAnalyticsBatch.Add(new ImportAnalyticsBatch() { BatchID = ID, DirectPlaylistName = playlistName, KaraokeFilesCount = karaokeCount, MusicFilesCount = musicCount, VideoFilesCount = videoCount, RawFilesCount = rawCount });
                return ID;
            }

            internal void SaveToDatabase(ref JukeboxBrainsDBEntities db, string filePath, int trackId)
            {
                if (DBAnalytics == null)
                    DBAnalytics = new List<ImportAnalyticsFile>();
                DBAnalytics.Add(new ImportAnalyticsFile() { FilePath = filePath, TrackId = trackId });
            }

            internal void AddBreadcrumb(string filePath, string Breadcrumb)
            {
                var a = DBAnalytics.Find(f => f.FilePath == filePath);
                if (a != null)
                {
                    //if (string.IsNullOrEmpty(a.Breadcrumbs))
                    //    a.Breadcrumbs = Breadcrumb;
                    //else
                    //    a.Breadcrumbs += ", " + Breadcrumb;
                    if (a.Breadcrumbs == null)
                        a.Breadcrumbs = new List<string>();

                    a.Breadcrumbs.Add(Breadcrumb);
                }
            }

            internal void AddSupportingMetaData(string filePath, string Breadcrumb, List<string> MetaData)
            {
                var a = DBAnalytics.Find(f => f.FilePath == filePath);
                if (a != null)
                {
                    if (a.SupportMetaData == null)
                        a.SupportMetaData = new List<SupportingInfo>();

                    a.SupportMetaData.Add(new SupportingInfo() { Key = Breadcrumb, Values = MetaData });
                }
            }

            internal void CloseBatch(int analyticBatchId)
            {
                //throw new NotImplementedException();
            }
        }

        public class ImportAnalyticsBatch
        {
            internal int BatchID { get; set; }
            internal int RawFilesCount { get; set; }
            internal int SuccessRawFilesCount { get; set; }
            internal int MusicFilesCount { get; set; }
            internal int SuccessMusicFilesCount { get; set; }
            internal int KaraokeFilesCount { get; set; }
            internal int SuccessKaraokeFilesCount { get; set; }
            internal int VideoFilesCount { get; set; }
            internal int SuccessVideoFilesCount { get; set; }
            internal string DirectPlaylistName { get; set; }
            internal int NewArtistsCount { get; set; }
            internal int NewAlbumsCount { get; set; }
            internal int SystemPlaylist1Count { get; set; }
            internal int SystemPlaylist2Count { get; set; }
            internal int SystemPlaylist3Count { get; set; }
            internal int SystemPlaylist4Count { get; set; }

            internal List<string> GetReport()
            {
                List<string> l = new List<string>();
                l.Add(string.Format("{0}", BatchID));
                return l;
            }
        }
        public class ImportAnalyticsFile
        {
            public string FilePath { get; set; }
            public string BreadCrumbString { get { return string.Join(", ", Breadcrumbs); } }
            public List<string> Breadcrumbs { get; set; }
            public int? TrackId { get; set; }

            public List<SupportingInfo> SupportMetaData { get; set; }
        }

        public class SupportingInfo
        {
            public string Key { get; set; }
            public List<string> Values { get; set; }
        }

        internal static BackgroundCheckout _backgroundWorkerLibraryLoading = BackgroundCheckout.Idle;
        internal static BackgroundCheckout BackgroundWorkerLibraryLoading
        {
            get { return _backgroundWorkerLibraryLoading; }
            set
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (Global.MainFrame.Content != null && Global.MainFrame.Content.GetType().Name == "Library")
                    {
                        //switch (value)
                        //{
                        //    case BackgroundCheckout.Abort:
                        //        ((Library)Global.MainFrame.Content).statusAbort.Visibility = Visibility.Visible;
                        //        ((Library)Global.MainFrame.Content).statusWorking.Visibility = Visibility.Collapsed;
                        //        ((Library)Global.MainFrame.Content).statusIdle.Visibility = Visibility.Collapsed;
                        //        ((Library)Global.MainFrame.Content).statusAmber.Visibility = Visibility.Collapsed;
                        //        break;
                        //    case BackgroundCheckout.GreenLight:
                        //        ((Library)Global.MainFrame.Content).statusAbort.Visibility = Visibility.Collapsed;
                        //        ((Library)Global.MainFrame.Content).statusWorking.Visibility = Visibility.Visible;
                        //        ((Library)Global.MainFrame.Content).statusIdle.Visibility = Visibility.Collapsed;
                        //        ((Library)Global.MainFrame.Content).statusAmber.Visibility = Visibility.Collapsed;
                        //        break;
                        //    case BackgroundCheckout.Idle:
                        //        ((Library)Global.MainFrame.Content).statusAbort.Visibility = Visibility.Collapsed;
                        //        ((Library)Global.MainFrame.Content).statusWorking.Visibility = Visibility.Collapsed;
                        //        ((Library)Global.MainFrame.Content).statusIdle.Visibility = Visibility.Visible;
                        //        ((Library)Global.MainFrame.Content).statusAmber.Visibility = Visibility.Collapsed;
                        //        break;
                        //}
                    }
                });
                _backgroundWorkerLibraryLoading = value;
            }
        }
        internal enum BackgroundCheckout
        {
            Idle,
            GreenLight,
            Abort
        }
        internal static bool ViewLoaded = false;
        internal static bool LibraryFlipFlop = false;

        #region Styling

        // FindResource(Global.ControlStyle_Button) as Style;
        internal static string ControlStyle = "StyleAlpha";
        internal static string ControlStyle_Button = "Button" + ControlStyle;
        internal static string ControlStyle_ToggleButton = "ToggleButton" + ControlStyle;
        internal static string ControlStyle_FolderButton = "FolderButtonStyle2";
        internal static string ControlStyle_InitialPathButton = "InitialPathButtonStyle";
        internal static string ControlStyle_AlbumButton = "AlbumButtonStyle";
        internal static string ControlStyle_FolderPathButton = "FolderPathButtonStyle";

        //FileListingStyleAlpha FileListingToggleStyleAlpha
        internal static string ControlStyle_FileListing = "FileListing" + ControlStyle;
        internal static string ControlStyle_FileListingToggle = "FileListingToggle" + ControlStyle;

        internal static void StopButtonFlashing(ref Button button)
        {
            Storyboard s = new Storyboard();
            button.BeginAnimation(Button.BackgroundProperty, null);
        }

        internal static void SetButtonFlashing(ref Button button)
        {
            ObjectAnimationUsingKeyFrames ani = new ObjectAnimationUsingKeyFrames();
            ani.Duration = new Duration(TimeSpan.FromSeconds(1));
            ani.RepeatBehavior = RepeatBehavior.Forever;

            ObjectKeyFrame o = new DiscreteObjectKeyFrame();
            o.Value = button.Background;
            o.KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0));

            ani.KeyFrames.Add(o);

            ObjectKeyFrame o2 = new DiscreteObjectKeyFrame();
            o2.Value = button.BorderBrush;
            o2.KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.5));

            ani.KeyFrames.Add(o2);

            Storyboard s = new Storyboard();
            s.RepeatBehavior = RepeatBehavior.Forever;
            s.Children.Add(ani);

            button.BeginAnimation(Button.BackgroundProperty, ani);
        }

        internal static void StopButtonFlashing(ref ToggleButton toggleButton)
        {
            Storyboard s = new Storyboard();
            toggleButton.BeginAnimation(Button.BackgroundProperty, null);
        }
        internal static void SetButtonFlashing(ref ToggleButton toggleButton)
        {
            ObjectAnimationUsingKeyFrames ani = new ObjectAnimationUsingKeyFrames();
            ani.Duration = new Duration(TimeSpan.FromSeconds(1));
            ani.RepeatBehavior = RepeatBehavior.Forever;

            ObjectKeyFrame o = new DiscreteObjectKeyFrame();
            o.Value = toggleButton.Background;
            o.KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0));

            ani.KeyFrames.Add(o);

            ObjectKeyFrame o2 = new DiscreteObjectKeyFrame();
            o2.Value = toggleButton.BorderBrush;
            o2.KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.5));

            ani.KeyFrames.Add(o2);

            Storyboard s = new Storyboard();
            s.RepeatBehavior = RepeatBehavior.Forever;
            s.Children.Add(ani);

            toggleButton.BeginAnimation(Button.BackgroundProperty, ani);
        }




        #endregion Styling

        #region UserSettings

        internal static bool PinTrackInfo = false;
        internal static string DefaultImportDirectory = "";
        internal static string PopupTextInput = "";
        public static ClassAdminSettings AdminSettings = new ClassAdminSettings();

        public class ClassAdminSettings
        {
            private int _KeyboardButtonSize = 50;
            public int KeyboardButtonSize
            {
                get { return _KeyboardButtonSize; }
                set
                {
                    _KeyboardButtonSize = value; setDBValue("KeyboardButtonSize", value);
                    Application.Current.Resources["TouchButtonHeight"] = value;
                }
            }
            private string _StartupVersion = "";
            public string StartupVersion
            {
                get { return _StartupVersion; }
                set { _StartupVersion = value; setDBValue("StartupVersion", value); }
            }
            private bool _isAutoPlayListShuffle = true;
            internal bool isAutoPlayListShuffle
            {
                get { return _isAutoPlayListShuffle; }
                set { _isAutoPlayListShuffle = value; setDBValue("isAutoPlayListShuffle", value); }
            }
            private bool _isAutoPlayShuffle = true;
            internal bool isAutoPlayShuffle
            {
                get { return _isAutoPlayShuffle; }
                set { _isAutoPlayShuffle = value; setDBValue("isAutoPlayShuffle", value); }
            }
            private int _VolumeIncrement = 40;
            internal int VolumeIncrement
            {
                get { return _VolumeIncrement; }
                set { _VolumeIncrement = value; setDBValue("VolumeIncrement", value); }
            }
            private bool _isTextButtonOverlay = false;
            internal bool isTextButtonOverlay
            {
                get { return _isTextButtonOverlay; }
                set { _isTextButtonOverlay = value; setDBValue("isTextButtonOverlay", value); }
            }
            private bool _ShowKeyboard = false;
            internal bool ShowKeyboard
            {
                get { return _ShowKeyboard; }
                set { _ShowKeyboard = value; setDBValue("ShowKeyboard", value); }
            }
            private bool _ShowAlbumNumbers = false;
            internal bool ShowAlbumNumbers
            {
                get { return _ShowAlbumNumbers; }
                set { _ShowAlbumNumbers = value; setDBValue("ShowAlbumNumbers", value); }
            }
            private bool _isMusicLibraryScrollHorizontal = false;
            internal bool IsMusicLibraryScrollHorizontal
            {
                get { return _isMusicLibraryScrollHorizontal; }
                set { _isMusicLibraryScrollHorizontal = value; setDBValue("IsMLSHorz", value); }
            }

            public int currentThemeSequence { get; set; } = 1;
            public int FirstTimeLoad { get; set; } = 0;
            public enum Quality { High, Medium, Low, Off }
            public Quality _audioVisualizerSize = Quality.Medium;
            public Quality AudioVisualizerSize { get { return _audioVisualizerSize; } internal set { if ((int)value >= Enum.GetNames(typeof(Quality)).Length) value = 0; _audioVisualizerSize = value; setDBValue("AudioVisualizerSize", (int)value); } }

            public void LoadValues()
            {
                using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                {
                    _isAutoPlayListShuffle = getValue(db, "isAutoPlayListShuffle", isAutoPlayListShuffle);
                    _isAutoPlayShuffle = getValue(db, "isAutoPlayShuffle", isAutoPlayShuffle);
                    _VolumeIncrement = getValue(db, "VolumeIncrement", VolumeIncrement);
                    _isTextButtonOverlay = getValue(db, "isTextButtonOverlay", isTextButtonOverlay);
                    _ShowKeyboard = getValue(db, "ShowKeyboard", ShowKeyboard);
                    _ShowAlbumNumbers = getValue(db, "ShowAlbumNumbers", ShowAlbumNumbers);
                    _StartupVersion = getStringValue(db, "StartupVersion", StartupVersion);
                    _KeyboardButtonSize = getValue(db, "KeyboardButtonSize", KeyboardButtonSize);
                    Application.Current.Resources["TouchButtonHeight"] = _KeyboardButtonSize;
                    _audioVisualizerSize = (Quality)getValue(db, "AudioVisualizerSize", (int)AudioVisualizerSize);
                    //isTextButtonOverlay = getValue(db, "isTextButtonOverlay");
                    //isTextButtonOverlay = getValue(db, "isTextButtonOverlay");




                }
            }


            private string getStringValue(JukeboxBrainsDBEntities db, string v, string defaultValue = "")
            {
                var a = db.AppSettings.Where(w => w.Type == v);
                if (a.Any())
                {
                    return a.First().Value;
                }
                else
                    return defaultValue;
            }
            private void setDBValue(string parameter, string value)
            {
                using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                {
                    var a = db.AppSettings.Where(w => w.Type == parameter);
                    if (a.Any())
                    {
                        var setting = a.First();
                        setting.Value = value;
                        db.Entry(setting).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                    }
                    else
                    {
                        // has no value
                        var setting = new AppSetting()
                        {
                            Type = parameter,
                            Subtype = "",
                            Value = value
                        };

                        db.AppSettings.Add(setting);
                        db.SaveChanges();
                    }
                }
            }

            private void setDBValue(string parameter, bool value)
            {
                using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                {
                    var a = db.AppSettings.Where(w => w.Type == parameter);
                    if (a.Any())
                    {
                        var setting = a.First();
                        setting.Value = value == true ? "true" : "false";
                        db.Entry(setting).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                    }
                    else
                    {
                        try
                        {
                            // has no value
                            var setting = new AppSetting()
                            {
                                Type = "IsMLSHorz",
                                Subtype = "",
                                Value = value == true ? "true" : "false"
                            };

                            db.AppSettings.Add(setting);
                            db.SaveChanges();
                        }
                        catch (Exception ex)
                        {

                        }

                    }
                }
            }
            private bool getValue(JukeboxBrainsDBEntities db, string v, bool defaultValue = false)
            {
                var a = db.AppSettings.Where(w => w.Type == v);
                if (a.Any())
                {
                    if (a.First().Value == "true")
                        return true;
                    else
                        return false;
                }
                else
                    return defaultValue;
            }

            private void setDBValue(string parameter, int value)
            {
                using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                {
                    var a = db.AppSettings.Where(w => w.Type == parameter);
                    if (a.Any())
                    {
                        var setting = a.First();
                        setting.Value = value.ToString();
                        db.Entry(setting).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                    }
                    else
                    {
                        // has no value
                        var setting = new AppSetting()
                        {
                            Type = parameter,
                            Subtype = "",
                            Value = value.ToString()
                        };

                        db.AppSettings.Add(setting);
                        db.SaveChanges();
                    }
                }
            }
            private int getValue(JukeboxBrainsDBEntities db, string v, int defaultValue = 0)
            {
                var a = db.AppSettings.Where(w => w.Type == v);
                if (a.Any())
                {
                    return int.Parse(a.First().Value);
                }
                else
                    return defaultValue;
            }
        }


        #endregion UserSettings


        #region AppModes


        public enum AppModeEnum
        {
            Karaoke,
            Music,
            Video,
            Radio
        }

        public enum AppControlModeEnum
        {
            Normal,
            Admin,
            Playlist
        }
        public enum AppActionModeEnum
        {
            Idle,
            Playing
        }

        internal static AppActionModeEnum AppActionMode = AppActionModeEnum.Idle;
        internal static AppModeEnum AppMode { get; set; }
        internal static bool isAdminOverride = false;
        internal static AppControlModeEnum AppControlMode = AppControlModeEnum.Normal;
        /// <summary>
        /// Default Buffer for Playlist being worked on
        /// </summary>
        internal static int AppPlaylistModeBufferID = -1;
        internal static string AppModeString
        {
            get
            {
                switch (AppMode)
                {
                    case AppModeEnum.Karaoke:
                        return "Karaoke";
                    case AppModeEnum.Music:
                        return "Music";
                    case AppModeEnum.Video:
                        return "Video";
                    case AppModeEnum.Radio:
                        return "Radio";
                }
                return "";
            }
        }

        /// <summary>
        /// Used when forcing the library to load as normal
        /// </summary>
        internal static bool LibraryForceViewMode = false;


        #endregion AppModes



        #region UIWidgets

        private static Timer ActiveButtonTimer { get; set; }
        internal static void setExpandedSongButtonBuffer(LibraryCardSong songcard)
        {
            expandedSongButtonBuffer = songcard;
            buttonHideTime = DateTime.Now.AddSeconds(5);
            buttonOpenTime = DateTime.Now;

            var startTimeSpan = TimeSpan.Zero;
            var periodTimeSpan = TimeSpan.FromSeconds(15);

            ActiveButtonTimer = new System.Threading.Timer((e) =>
            {
                CollapseSong();
            }, null, 5000, -1);
        }
        private static LibraryCardSong expandedSongButtonBuffer { get; set; }
        private static DateTime buttonHideTime { get; set; }
        private static DateTime buttonOpenTime { get; set; }

        private static void CollapseSong()
        {
            if (DateTime.Now >= buttonHideTime)
            {
                expandedSongButtonBuffer.Dispatcher.Invoke(new Action(() => { expandedSongButtonBuffer.hideButtons(); }));
            }
        }


        #endregion UIWidgets


        internal static string LastImportDirectory
        {
            get
            {
                if (_lastImportDirectory == null)
                {
                    using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                    {
                        var l = db.AppSettings.Where(w => w.Type == "LastImportDir");
                        if (!l.Any())
                        {
                            _lastImportDirectory = new AppSetting()
                            {
                                Type = "LastImportDir",
                                Subtype = "",
                                Value = @"C:\"
                            };

                            db.AppSettings.Add(_lastImportDirectory);
                            db.SaveChanges();
                        }
                        else
                        {
                            _lastImportDirectory = l.First();
                        }
                    }
                }

                return _lastImportDirectory.Value;
            }
            set
            {
                if (_lastImportDirectory == null)
                {
                    using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                    {
                        var l = db.AppSettings.Where(w => w.Type == "LastImportDir");
                        if (!l.Any())
                        {
                            _lastImportDirectory = new AppSetting()
                            {
                                Type = "LastImportDir",
                                Subtype = "",
                                Value = value
                            };

                            db.AppSettings.Add(_lastImportDirectory);
                            db.SaveChanges();
                        }
                        else
                        {
                            _lastImportDirectory = l.First();
                            _lastImportDirectory.Value = value;
                            db.Entry(_lastImportDirectory).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                        }
                    }
                }
                else
                {
                    using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                    {
                        var l = db.AppSettings.Where(w => w.Type == "LastImportDir");
                        if (!l.Any())
                        {
                            _lastImportDirectory = new AppSetting()
                            {
                                Type = "LastImportDir",
                                Subtype = "",
                                Value = value
                            };

                            db.AppSettings.Add(_lastImportDirectory);
                            db.SaveChanges();
                        }
                        else
                        {
                            _lastImportDirectory.Value = value;
                            db.Entry(_lastImportDirectory).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                        }

                    }
                }
            }
        }
        private static AppSetting _lastImportDirectory { get; set; }


        internal static ImportFactory importFactory = new ImportFactory();

        internal static Frame MainFrame { get; set; }
        internal static FileManagerV3 OpenFileManager { get { if (MainFrame.Content.GetType().Name == "FileManagerV3") return ((FileManagerV3)MainFrame.Content); else return null; } }

        internal static MainWindow MainWindow { get; set; }
        internal static MainPlayer mPlayer { get; set; }
        //internal static string[] fileformats = { ".mp3", ".mp4", ".mkv" };
        internal static string[] musicformats = { ".mp3", ".wav", ".flac", ".m4a", ".wma" };
        internal static string[] videoformats = { ".mp4", ".mkv",".avi", ".webm", ".mpv", ".mpe", ".mpeg",".mp2",".mpg",".m4p",".m4v",".wmv",".mov",".flv",".swf" };
        internal static string[] karaokeformats = { ".cdg" };

        internal static bool isMusicFormat(string path)
        {
            bool result = false;

            foreach (string s in musicformats)
                if (path.Contains(s))
                    result = true;

            return result;
        }


        internal static bool isKeyboardFocus = false;

        /// <summary>
        /// Used as a bridge buffer to undo changes if needed
        /// </summary>
        internal static int SystemPlaylistID = 1;
        /// <summary>
        /// SystemPlaylist - Used to handle live playlist changes
        /// </summary>
        internal static int BufferPlaylistID = 4;
        internal static string NowPlayingName = "NowPlaying";
        internal static int NewTracksPlaylistId = 3;
        internal static int NowPlayingId = 2;
        internal static int AutoUpdateTodoListPlaylistId = 5;

        #region Background Updating System


        internal static bool isAutoUpdating = false;
        internal static bool isUpdatingLibrary = false;
        internal static StackPanel ButtonBuffer_StackPannel;
        internal static Rectangle ButtonBuffer_CoverButton;

        internal static void LibraryUpdateActionMode(int AlbumId = -1)
        {
            try
            {

                ((Library)Global.MainWindow.MusicLibraryFrame.Content).UpdateAppActionMode(AlbumId);
            }
            catch (Exception)
            {

            }
        }


        #endregion Background Updating System

        #region Notification System

        //             ((Library)Global.MainFrame.Content).SetPlaylistMode(basePlaylistDetail, Library.PlaylistModeEnum.NewPlaylist);

        internal static bool AlertNotifications
        {
            get { return Global.MainWindow.hasUserAlerts; }
            set { Global.MainWindow.hasUserAlerts = value; }
        }


        #endregion Notification System

        #region Online Server Things

        static bool _isForcedOffline = false;
        internal static void ForceOnlineTimeout(bool ResumeOnline = false)
        {
            if (ResumeOnline)
            {
                _isForcedOffline = false;
            }
            else
            {
                onlineTimeOut = DateTime.Now.AddMinutes(3);
                onlineLastChecked = DateTime.Now;
                _isOnline = false;
                _isForcedOffline = true;
            }
        }

        internal static void SetOnlineTimeout(string FilePathKey = "")
        {
            if (!string.IsNullOrEmpty(FilePathKey))
            {
                using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                {
                    Global.ImportAnalytics.AddBreadcrumb(FilePathKey, "Interrupted iTunes import");
                }
            }
            onlineTimeOut = DateTime.Now.AddMinutes(3);
            onlineLastChecked = DateTime.Now;
            _isOnline = false;


            var startTimeSpan = TimeSpan.Zero;
            var periodTimeSpan = TimeSpan.FromMinutes(3);

            var timer = new System.Threading.Timer((e) =>
            {
                CheckOnlineStatus();
            }, null, periodTimeSpan, periodTimeSpan);
        }

        internal static bool isOnline
        {
            get
            {
                if (_isOnline == false && onlineTimeOut < DateTime.Now && _isForcedOffline == false)
                {
                    // check again if online yet
                    CheckOnlineStatus();
                }

                if (onlineLastChecked == null || onlineLastChecked == blankOnlineLastChecked)
                {
                    _isOnline = GetPingResponse("www.apple.com", 2000);
                    onlineLastChecked = DateTime.Now;
                }
                else
                {
                }
                return _isOnline;
            }
        }

        private static bool _isOnline = false;
        private static DateTime blankOnlineLastChecked { get; set; }
        private static DateTime onlineLastChecked { get; set; }
        private static DateTime onlineTimeOut { get; set; }

        internal static void CheckOnlineStatus()
        {
            _isOnline = GetPingResponse("www.apple.com");
            onlineLastChecked = DateTime.Now;
        }

        public static bool GetPingResponse(string IpAddress, int timeout = 3000)
        {
            var ping = new Ping();
            try
            {
                var reply = ping.Send(IpAddress, timeout);
                return (reply.Status == IPStatus.Success);
            }
            catch
            {
                return (false);
            }
        }


        #endregion Online Server Things
    }

    public class MainPlayer
    {
        // playlist logic
        // Any play / que action voids last played if last played isn't loaded first

        /// <summary>
        /// 
        /// </summary>
        /// <returns>False if there are no last tracks</returns>
        public bool hasLibraryItems(string type)
        {
            try
            {

                using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                {
                    return db.LibraryViews.Any(a => a.Type == type);
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
        public bool hasLoadLastPlaylist(string type)
        {
            using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
            {
                var lastTracks = db.Playlists.Where(w => w.PlaylistId == Global.NowPlayingId).Select(s => s.TrackId).ToList();
                if (lastTracks.Any())
                {
                    return db.TrackLibraries.Any(a => lastTracks.Contains(a.Id) && a.Type == type);
                }
                else
                    return false;
            }
        }
        public bool LoadLastPlaylist()
        {
            bool hasLastPlayingTracks = false;
            using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
            {
                // there can only be one
                // Important to include system setting!
                hasLastPlayingTracks = db.PlayListDetails.Where(w => w.Name == Global.NowPlayingName && w.Type == 0).First().Playlists.Count > 0;
            }

            if (hasLastPlayingTracks)
            {
                isIdle = false;
                // Last playing is a system playlist
                StartPlaylist(Global.NowPlayingName, true);
                Global.AppActionMode = Global.AppActionModeEnum.Playing;
            }
            else
            {
                NowPlaying.ShowNotification("No songs to load. Here's the Library instead");
            }

            return hasLastPlayingTracks;
        }

        // idle settings
        public PlayList playList = new PlayList();
        public Queue<PlayItem> playQueue = new Queue<PlayItem>();
        public Label LabelCurrentTrackName { get; set; }
        public PopupNotifications Notifactions { get; set; }

        //public int playIndex = 0;
        private bool isNewNowPlaying = false;
        public bool isIdle = false;
        public bool isRepeat = false;


        internal DirectoryInfo vlcLibDirectory;

        private VlcControl _vlcControllerAudio;
        private VlcControl _vlcControllerVideo;
        private bool _isVLCVideoPlayer = true;
        private bool _settings_isCopySettings = false;
        private bool _settings_mute;
        private int _settings_volume;
        private bool isVLCVideoPlayer
        {
            get { return _isVLCVideoPlayer; }
            set
            {
                _settings_isCopySettings = false;   //reset

                // only stop when actually changing
                if (value != _isVLCVideoPlayer)
                {
                    // Stop current player
                    if (vlcController.SourceProvider.MediaPlayer != null)
                    {
                        ThreadPool.QueueUserWorkItem(_ => vlcController.SourceProvider.MediaPlayer.Play(""));
                        vlcController.Dispatcher.Invoke(() => { vlcController.Visibility = Visibility.Collapsed; });


                        //vlcController.SourceProvider.MediaPlayer.Audio.ToggleMute()
                        _settings_isCopySettings = true;
                        _settings_mute = vlcController.SourceProvider.MediaPlayer.Audio.IsMute;
                        _settings_volume = vlcController.SourceProvider.MediaPlayer.Audio.Volume;
                    }

                    _isVLCVideoPlayer = value;
                    //vlcController.Visibility = Visibility.Visible;
                    vlcController.Dispatcher.Invoke(() => { vlcController.Visibility = Visibility.Visible; });
                    //vlcController.Visibility = Visibility.Visible;
                }
            }
        }
        public VlcControl vlcController
        {
            get
            {
                if (isVLCVideoPlayer) return _vlcControllerVideo;
                else return _vlcControllerAudio;
            }
            set
            {
                if (isVLCVideoPlayer) _vlcControllerVideo = value;
                else _vlcControllerAudio = value;
            }
        }

        internal Slider sliderTrackBar { get; set; }
        internal TimeSpan time1 { get; set; }
        internal TimeSpan time2 { get; set; }

        public Frame popUp_Frame { get; set; }
        public Grid popUp_Grid { get; set; }

        private void EventPositionChanged(object sender, EventArgs e)
        {
            var i = vlcController.SourceProvider.MediaPlayer.Position * 10;
            time1 = new TimeSpan(0, 0, 0, 0, ((int)(vlcController.SourceProvider.MediaPlayer.Time)));
            time2 = new TimeSpan(0, 0, 0, 0, ((int)vlcController.SourceProvider.MediaPlayer.Length));

            sliderTrackBar.Dispatcher.Invoke(() =>
            {
                if (sliderTrackBar.Tag == null)
                    sliderTrackBar.Tag = "";

                if (sliderTrackBar.Tag.ToString() != "wait")
                {
                    sliderTrackBar.Value = i;
                    sliderTrackBar.Tag = "vlc";
                }
            });
        }
        public void SetTrackbar(double newPosition)
        {
            if (vlcController.SourceProvider.MediaPlayer != null)
            {
                //if (vlcController.SourceProvider.MediaPlayer.IsPausable())
                //    vlcController.SourceProvider.MediaPlayer.Pause();
                //else

                var i = (int)(time2.TotalSeconds * newPosition) * 100;

                ThreadPool.QueueUserWorkItem(_ => vlcController.SourceProvider.MediaPlayer.Time = i);
                //float f = (float)newPosition / 10;
                //ThreadPool.QueueUserWorkItem(_ => vlcController.SourceProvider.MediaPlayer.Position = f);

            }
        }


        #region PopUp
        //
        //
        //
        public object PopUpBackPocket { get; set; }
        public bool hasPopUpBackPocket { get { return PopUpBackPocket != null; } }

        internal string PopUpTextBuffer { get; set; }
        internal void BumpPopUp(Popup_Confirm popup_Confirm)
        {
            if (popUp_Grid.Visibility == System.Windows.Visibility.Visible)
            {
                PopUpBackPocket = Global.mPlayer.popUp_Frame.Content;
                Global.mPlayer.popUp_Frame.Content = popup_Confirm;
            }
            else
            {
                popUp_Grid.Visibility = System.Windows.Visibility.Visible;
                Global.mPlayer.popUp_Frame.Content = popup_Confirm;
            }
        }
        internal void BumpPopUp(Popup_TextInput popup_TextInput)
        {
            if (popUp_Grid.Visibility == System.Windows.Visibility.Visible)
            {
                PopUpBackPocket = Global.mPlayer.popUp_Frame.Content;
                Global.mPlayer.popUp_Frame.Content = popup_TextInput;
            }
            else
            {
                popUp_Grid.Visibility = System.Windows.Visibility.Visible;
                Global.mPlayer.popUp_Frame.Content = popup_TextInput;
            }
        }

        internal void BumpPopUp(Popup_SmartUpdater popup_SmartUpdater)
        {
            if (popUp_Grid.Visibility == System.Windows.Visibility.Visible)
            {
                PopUpBackPocket = Global.mPlayer.popUp_Frame.Content;
                Global.mPlayer.popUp_Frame.Content = popup_SmartUpdater;
            }
            else
            {
                popUp_Grid.Visibility = System.Windows.Visibility.Visible;
                Global.mPlayer.popUp_Frame.Content = popup_SmartUpdater;
            }
        }

        public void TogglePopUp()
        {
            if (hasPopUpBackPocket)
            {

                if (!string.IsNullOrEmpty(PopUpTextBuffer))
                {
                    switch (PopUpBackPocket.GetType().Name)
                    {
                        case "Popup_SmartUpdater":
                            ((Popup_SmartUpdater)PopUpBackPocket).setTextInput = PopUpTextBuffer;
                            break;
                    }
                }

                popUp_Frame.Content = PopUpBackPocket;

                PopUpBackPocket = null;
            }
            else
            {
                popUp_Frame.Content = null;
                popUp_Grid.Visibility = Visibility.Collapsed;
                Global.isKeyboardFocus = false;
            }

            //popUp_StackPannel.Children.Clear();
            //popUpController.IsOpen = !popUpController.IsOpen;
        }

        public void PopUp_Search()
        {
            Global.isKeyboardFocus = true;
            popUp_Grid.Visibility = Visibility.Visible;
            popUp_Frame.Content = new Popup_TextInput(Popup_TextInput.ControlMode.Search);
        }

        public void PopUp_NewPlaylist(bool bumpPopUp = false)
        {
            if (bumpPopUp)
            {
                PopUpBackPocket = popUp_Frame.Content;
            }

            Global.isKeyboardFocus = true;
            popUp_Grid.Visibility = Visibility.Visible;
            popUp_Frame.Content = new Popup_TextInput(Popup_TextInput.ControlMode.NewPlaylist);
        }

        public void PopUp_NewPlaylist(Library controlLibrary)
        {
            Global.isKeyboardFocus = true;
            popUp_Grid.Visibility = Visibility.Visible;
            popUp_Frame.Content = new Popup_TextInput(Popup_TextInput.ControlMode.NewPlaylist, controlLibrary);
        }
        //
        //
        //
        #endregion PopUp




        public PlayerNotificationBar NowPlaying { get; set; }
        public SongInfoBar NowPlayingSongInfo { get; set; }

        public MainPlayer(ref VlcControl vlcContainerVideo, ref VlcControl vlcContainerAudio, ref PlayerNotificationBar lblNowPlaying, ref Label lblCurrentTrackName, ref Grid grid, ref Frame frame, ref SongInfoBar songInfo)
        {
            LabelCurrentTrackName = lblCurrentTrackName;
            _vlcControllerVideo = vlcContainerVideo;
            _vlcControllerAudio = vlcContainerAudio;
            //vlcController = vlcContainer;

            NowPlaying = lblNowPlaying;
            NowPlayingSongInfo = songInfo;

            var currentAssembly = Assembly.GetEntryAssembly();
            var currentDirectory = new FileInfo(currentAssembly.Location).DirectoryName;
            vlcLibDirectory = new DirectoryInfo(System.IO.Path.Combine(currentDirectory, "libvlc", IntPtr.Size == 4 ? "win-x86" : "win-x64"));

            //PlayCue = new List<PlayItem>();
            //PlayHistory = new List<PlayItem>();

            //backgroundfile = new PlayItem();
            //backgroundfile.SetPath("C:\\Users\\Johan\\source\\repos\\foozball3000\\JukeBoxSolutions\\Media\\Particle tests (15) 3D Music Visualizer - Full HD.mp4");

            popUp_Grid = grid;
            popUp_Frame = frame;

            Global.DefaultImportDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic) + "\\Jukebox\\";


        }

        private void EventEndOfTrack(object sender, EventArgs e)
        {
            OnEventEndOfTrack(EventArgs.Empty);
        }

        private void OnEventEndOfTrack(EventArgs e)
        {
            // Code Here
            Next();
        }

        public void RunVlCSetup()
        {
            // get current app path
            //var vlcLibDirectory = new DirectoryInfo(System.IO.Path.Combine(currentpath, "libvlc", IntPtr.Size == 4? "win-x86":"win-x64"));
            //var vlcLibDirectory = new DirectoryInfo(System.IO.Path.Combine(currentpath, "libvlc"));


            //vControl.SourceProvider.CreatePlayer(vlcLibDirectory, options);
        }

        public void SetIdle()
        {
            isIdle = true;
            isVLCVideoPlayer = true;
            StartPlaylist("SystemBackground", true, true);
            Global.AppActionMode = Global.AppActionModeEnum.Idle;
        }

        public void xPlayNow(string path)
        {
            // PlayNow(new PlayItem(path));
        }

        public class PlayList : List<TrackLibrary>
        {
            private bool isVideo = false;
            private bool isMusic = false;
            private bool isKaraoke = false;
            private bool isRadio = false;

            private List<int> randomhistory = new List<int>();
            private bool isShuffle = false;
            private int ShuffleIndex
            {
                get
                {
                    if (randomhistory.Count != this.Count)
                        Shuffle = true;
                    return randomhistory[PlayIndex];
                }
            }

            public bool Shuffle
            {
                get
                {
                    return isShuffle;
                }
                set
                {
                    isShuffle = value;
                    if (value == true)
                    {
                        var r = new Random();
                        randomhistory = new List<int>();

                        for (int i = 0; i < this.Count; i++)
                        {
                            if (randomhistory.Any())
                            {
                                randomhistory.Insert(r.Next(0, randomhistory.Count), i);
                            }
                            else
                                randomhistory.Add(i);
                        }
                    }
                }
            }

            /// used to force switching to modified mode 
            public bool isLocked = false;
            public bool isModified
            {
                get { return _isModified; }
                set
                {
                    // copy over to nowplaying
                    if (_isModified == false && value == true)
                    {
                        Global.mPlayer.dbCloneToBufferPlaylist(PlayListId);
                        _bufferPlayListId = PlayListId;
                        PlayListId = Global.BufferPlaylistID;
                    }
                    _isModified = value;
                }
            }
            private bool _isModified = false;
            public int PlayListId { get; set; }
            public int _bufferPlayListId;
            private int _activePlayListId;

            public int PlayIndex { get; set; }
            //defaults to 'NowPlaying'
            public PlayList(int _playlistID = 2)
            {
                PlayListId = _playlistID;
                PlayIndex = 0;
            }

            //defaults to 'NowPlaying'
            public PlayList(PlayListDetail _playlist, List<TrackLibrary> trackList)
            {
                if (_playlist.Id != Global.NowPlayingId)
                    isLocked = true;
                else
                    isLocked = false;
                PlayListId = _playlist.Id;
                isVideo = _playlist.isVideo;
                isMusic = _playlist.isMusic;
                isKaraoke = _playlist.isKaraoke;
                isRadio = _playlist.isRadio;

                PlayIndex = 0;
                base.Clear();
                base.AddRange(trackList);
            }

            public void SetCurrentTrack(int i)
            {
                PlayIndex = i;
            }

            public TrackLibrary GetCurrentTrack(bool noShuffle = false)
            {
                if (isShuffle && noShuffle == false)
                    return this.Count == 0 ? null : this[ShuffleIndex];
                else
                    return this.Count == 0 ? null : this[PlayIndex];
            }

            public TrackLibrary GetNextTrack()
            {
                PlayIndex++;
                if (PlayIndex == this.Count)
                {
                    PlayIndex = 0;
                    if (isShuffle)
                        Shuffle = true;
                }

                if (isShuffle)
                    return this.Count == 0 ? null : this[ShuffleIndex];
                else
                    return this.Count == 0 ? null : this[PlayIndex];
            }

            public TrackLibrary GetPreviousTrack()
            {
                PlayIndex--;
                if (PlayIndex < 0)
                    PlayIndex = this.Count - 1;

                if (isShuffle)
                    return this.Count == 0 ? null : this[ShuffleIndex];
                else
                    return this.Count == 0 ? null : this[PlayIndex];
            }

            /// <summary>
            /// Auto Injects record into current position on playlist
            /// </summary>
            /// <param name="playItem"></param>
            /// <param name="db"></param>
            public void InsertNowPlayingRange(List<TrackLibrary> playItems)
            {
                if (isLocked)
                {
                    isModified = true;
                }

                bool isChanged = false;
                if (this.Count == 0)
                {
                    base.AddRange(playItems);
                    PlayIndex = 0;
                    isChanged = CheckType(playItems);


                    // save to db
                    Global.mPlayer.dbAddTrackToPlaylist(playItems, PlayListId);
                    if (isChanged) Global.mPlayer.dbUpdatePlaylistDetailsType(PlayListId, isVideo, isMusic, isKaraoke, isRadio);
                }
                else
                {
                    this.InsertRange(PlayIndex, playItems);
                    isChanged = CheckType(playItems);

                    // save to db
                    Global.mPlayer.dbInsertTracksToPlaylist(playItems, PlayIndex, PlayListId);
                    if (isChanged) Global.mPlayer.dbUpdatePlaylistDetailsType(PlayListId, isVideo, isMusic, isKaraoke, isRadio);
                    PlayIndex++;
                }


                if (PlayListId != Global.BufferPlaylistID)
                {
                    Global.mPlayer.dbInsertTracksToPlaylist(playItems, PlayIndex, Global.BufferPlaylistID);
                    if (isChanged) Global.mPlayer.dbUpdatePlaylistDetailsType(Global.BufferPlaylistID, isVideo, isMusic, isKaraoke, isRadio);
                }
            }

            public void InsertNowPlaying(TrackLibrary playItem)
            {
                if (isLocked)
                {
                    isModified = true;
                }

                if (this.Count == 0)
                {
                    base.Add(playItem);
                    PlayIndex = 0;
                }
                else
                {
                    this.Insert(PlayIndex + 1, playItem);
                    PlayIndex++;
                }
                bool isChanged = CheckType(playItem);

                // save to db
                if (isChanged) Global.mPlayer.dbUpdatePlaylistDetailsType(PlayListId, isVideo, isMusic, isKaraoke, isRadio);
                Global.mPlayer.dbInsertTrackToPlaylist(playItem, PlayIndex, PlayListId);

                if (PlayListId != Global.BufferPlaylistID)
                {
                    if (isChanged) Global.mPlayer.dbUpdatePlaylistDetailsType(Global.BufferPlaylistID, isVideo, isMusic, isKaraoke, isRadio);
                    Global.mPlayer.dbInsertTrackToPlaylist(playItem, PlayIndex, Global.BufferPlaylistID);
                }

            }

            public void InsertNext(TrackLibrary playItem)
            {
                if (isLocked)
                {
                    isModified = true;
                }

                if (this.Count == 0)
                {
                    base.Add(playItem);
                    PlayIndex = 0;
                    bool ischanged = CheckType(playItem);

                    // save to db
                    Global.mPlayer.dbInsertTrackToPlaylist(playItem, PlayIndex, PlayListId);
                    if (ischanged) Global.mPlayer.dbUpdatePlaylistDetailsType(PlayListId, isVideo, isMusic, isKaraoke, isRadio);
                    if (PlayListId != Global.BufferPlaylistID)
                    {
                        Global.mPlayer.dbInsertTrackToPlaylist(playItem, PlayIndex, Global.BufferPlaylistID);
                        if (ischanged) Global.mPlayer.dbUpdatePlaylistDetailsType(Global.BufferPlaylistID, isVideo, isMusic, isKaraoke, isRadio);
                    }
                }
                else
                {
                    this.Insert(PlayIndex + 1, playItem);
                    bool ischanged = CheckType(playItem);

                    // save to db
                    Global.mPlayer.dbInsertTrackToPlaylist(playItem, PlayIndex + 1, PlayListId);
                    if (ischanged) Global.mPlayer.dbUpdatePlaylistDetailsType(PlayListId, isVideo, isMusic, isKaraoke, isRadio);
                    if (PlayListId != Global.BufferPlaylistID)
                    {
                        Global.mPlayer.dbInsertTrackToPlaylist(playItem, PlayIndex + 1, Global.BufferPlaylistID);
                        if (ischanged) Global.mPlayer.dbUpdatePlaylistDetailsType(Global.BufferPlaylistID, isVideo, isMusic, isKaraoke, isRadio);
                    }
                }


            }

            public new void Add(TrackLibrary item)
            {
                if (isLocked)
                {
                    isModified = true;
                }

                base.Add(item);
                bool isChanged = CheckType(item);

                // save to db
                Global.mPlayer.dbAddTrackToPlaylist(item, PlayListId);
                if (isChanged) Global.mPlayer.dbUpdatePlaylistDetailsType(PlayListId, isVideo, isMusic, isKaraoke, isRadio);
                if (PlayListId != Global.BufferPlaylistID)
                {
                    Global.mPlayer.dbAddTrackToPlaylist(item, Global.BufferPlaylistID);
                    if (isChanged) Global.mPlayer.dbUpdatePlaylistDetailsType(Global.BufferPlaylistID, isVideo, isMusic, isKaraoke, isRadio);
                }
            }

            private bool CheckType(IEnumerable<TrackLibrary> collection)
            {
                var types = collection.Select(s => s.Type).Distinct();
                bool isChanged = false;
                foreach (var t in types)
                {
                    switch (t) { case "Video": if (!isVideo) { isVideo = true; isChanged = true; } break; case "Karaoke": if (!isKaraoke) { isKaraoke = true; isChanged = true; } break; case "Music": if (!isMusic) { isMusic = true; isChanged = true; } break; case "Radio": if (!isRadio) { isRadio = true; isChanged = true; } break; }
                }

                return isChanged;
            }
            private bool CheckType(TrackLibrary item)
            {
                bool isChanged = false;
                if (item != null)
                    switch (item.Type) { case "Video": if (!isVideo) { isVideo = true; isChanged = true; } break; case "Karaoke": if (!isKaraoke) { isKaraoke = true; isChanged = true; } break; case "Music": if (!isMusic) { isMusic = true; isChanged = true; } break; case "Radio": if (!isRadio) { isRadio = true; isChanged = true; } break; }
                return isChanged;
            }
            private bool RemoveType(TrackLibrary item)
            {
                if (item != null && item.Type != null)
                {

                    if (!base.Exists(x => x.Type == item.Type))
                    {
                        switch (item.Type) { case "Video": isVideo = false; break; case "Karaoke": isKaraoke = false; break; case "Music": isMusic = false; break; case "Radio": isRadio = false; break; }
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                    return false;
            }
            public new void AddRange(IEnumerable<TrackLibrary> collection)
            {
                if (isLocked)
                {
                    isModified = true;
                }

                base.AddRange(collection);
                bool isChanged = CheckType(collection);


                // save to db
                Global.mPlayer.dbAddTrackToPlaylist(collection, PlayListId);
                if (isChanged) Global.mPlayer.dbUpdatePlaylistDetailsType(PlayListId, isVideo, isMusic, isKaraoke, isRadio);
                if (PlayListId != Global.BufferPlaylistID)
                {
                    Global.mPlayer.dbAddTrackToPlaylist(collection, Global.BufferPlaylistID);
                    if (isChanged) Global.mPlayer.dbUpdatePlaylistDetailsType(Global.BufferPlaylistID, isVideo, isMusic, isKaraoke, isRadio);
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="item"></param>
            /// <returns>If App Now Idle</returns>
            public new bool Remove(TrackLibrary item)
            {
                if (isLocked)
                {
                    isModified = true;
                }

                base.Remove(item);
                bool isChanged = RemoveType(item);

                // remove from db
                Global.mPlayer.dbRemoveTrackFromPlaylist(item, PlayListId);
                if (isChanged) Global.mPlayer.dbUpdatePlaylistDetailsType(PlayListId, isVideo, isMusic, isKaraoke, isRadio);
                if (PlayListId != Global.BufferPlaylistID)
                {
                    Global.mPlayer.dbRemoveTrackFromPlaylist(item, Global.BufferPlaylistID);
                    if (isChanged) Global.mPlayer.dbUpdatePlaylistDetailsType(Global.BufferPlaylistID, isVideo, isMusic, isKaraoke, isRadio);
                }

                if (Count == 0)
                {
                    Global.mPlayer.SetIdle();
                    return true;
                }
                else
                    return false;
            }
            public new void RemoveRange(IEnumerable<TrackLibrary> collection)
            {
                if (isLocked)
                {
                    isModified = true;
                }

                //base.RemoveAll(collection);

                foreach (var c in collection)
                {
                    Remove(c);
                }
            }

            public void Unload()
            {
                Global.mPlayer.dbClearNowPlaying();
                base.Clear();
                this.PlayListId = Global.NowPlayingId;
                isLocked = false;
                isModified = false;
                PlayIndex = 0;
            }

            public void CancelChanges()
            {
                Global.mPlayer.dbClearBufferPlaylist();
                if (isModified)
                    PlayListId = _bufferPlayListId;
                isModified = false;
            }
            public void SaveChanges()
            {
                Global.mPlayer.dbApplyBufferPlaylistChanges(_bufferPlayListId);
                PlayListId = _bufferPlayListId;
                isModified = false;
            }

            internal void CopyTypeSettings(ref PlayListDetail newPlaylist)
            {
                newPlaylist.isVideo = isVideo;
                newPlaylist.isMusic = isMusic;
                newPlaylist.isKaraoke = isKaraoke;
                newPlaylist.isRadio = isRadio;
            }
        }

        public void PlayNext(TrackLibrary track)
        {
            // Create/Load NowPlayingList
            if (isIdle)
            {
                // Lets just create a new list anyway
                playList = new PlayList();
                isIdle = false;
                if (!isNewNowPlaying)
                {
                    isNewNowPlaying = true;
                    dbClearNowPlaying();
                }
            }
            playList.InsertNext(track);
        }
        public void PlayNow(TrackLibrary track)
        {
            // Create/Load NowPlayingList
            if (isIdle)
            {
                // Lets just create a new list anyway
                playList = new PlayList();
                isIdle = false;
                if (!isNewNowPlaying)
                {
                    isNewNowPlaying = true;
                    dbClearNowPlaying();
                }
            }

            if (playList.Count(x => x.FileName == track.FileName) == 0)
            {
                playList.InsertNowPlaying(track);
            }

            Play(playList.GetCurrentTrack());
        }

        public TrackLibrary CurrentTrack { get; set; }
        public SongPlayingArtwork SongPlayingArtWork { get; set; }
        public VolumnControlPanel VolumeControlPanel { get; set; }
        public CurrentTrackInfo CurrentTrackInfo { get; set; }
        public void PlayNow(List<TrackLibrary> tracks)
        {
            // Create/Load NowPlayingList
            if (isIdle)
            {
                // Lets just create a new list anyway
                playList = new PlayList();
                isIdle = false;
                if (!isNewNowPlaying)
                {
                    isNewNowPlaying = true;
                    dbClearNowPlaying();
                }
            }
            playList.InsertNowPlayingRange(tracks);
            CurrentTrack = playList.GetCurrentTrack();
            if (Global.mPlayer.SongPlayingArtWork != null)
            {

                Global.mPlayer.SongPlayingArtWork.CarouselUserControl._carouselDABRadioStations.SelectedItem = (Global.mPlayer.SongPlayingArtWork.CarouselUserControl._carouselDABRadioStations.ItemsSource as List<RadioStation>)[playList.PlayIndex];
            }
            Play(CurrentTrack);
        }

        public void HideNowPlayingInfo()
        {
            NowPlaying.Dispatcher.Invoke(new Action(() => { NowPlaying.ClearMessage(); }));
        }
        public void ShowNowPlayingInfo()
        {
            NowPlayingSongInfo.Dispatcher.Invoke(new Action(() => { NowPlayingSongInfo.PinNowPlaying(); }));
        }

        public void ShowNotification(string message, bool shouldLoadLibrary = false)
        {
            NowPlaying.Dispatcher.Invoke(new Action(() => { NowPlaying.ShowNotification(message); }));
        }

        public void HideNowPlayingSongInfo()
        {
            NowPlayingSongInfo.Dispatcher.Invoke(new Action(() => { NowPlayingSongInfo.ClearMessage(); }));
        }
        public void ShowNowPlayingSongInfo()
        {
            NowPlayingSongInfo.Dispatcher.Invoke(new Action(() => { NowPlayingSongInfo.PinNowPlaying(); }));
        }

        public void Play(TrackLibrary track)
        {
            if (track != null)
            {
                if (!isIdle)
                {
                    // load from database
                    using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                    {
                        TrackLibrary t = db.TrackLibraries.Find(track.Id);
                        SongLibrary sl;
                        var list = t.SongLibraries.ToList();
                        sl = t.SongLibraries.First();

                        //  LabelCurrentTrackName.Content = sl.SongName;

                        LabelCurrentTrackName.Dispatcher.Invoke(new Action(() => { LabelCurrentTrackName.Content = sl.SongName; }));

                        //NowPlaying.Dispatcher.Invoke(new Action(() => { NowPlaying.ShowNowPlaying(sl); }));
                        track = t;
                    }
                    //NowPlaying.Visibility = Visibility.Visible;
                }

                Play(track.FilePath, track.Type == "SystemVideo" || track.Type == "Video" || track.Type == "Karaoke");
            }
        }

        private void Play(string path, bool isVideoMode)
        {
            vlcController.Dispatcher.InvokeAsync(() =>
            {
                List<string> settings = new List<string>() { "Input Path = " + path, "isVideoMode = " + isVideoMode };
                int logid = -1;
                if (!isIdle)
                    logid = LogSystem.StartProcess(LogSystem.Processes.VLCCorePlayer, settings, "VLC Settings");



                // remove when successful



                //PlayItem p = new PlayItem(fileInfo);
                bool isStreaming = false;
                bool enableVisualisation = false;

                //if (path.Contains(".cdg"))
                //    path = path.Replace(".cdg", ".mp3");

                Uri stream = new Uri(path);
                FileInfo fileInfo = null;

                if (path.Contains("https://") || path.Contains("http://"))
                {
                    stream = new Uri(path);
                    isStreaming = true;
                }
                else
                {
                    fileInfo = new FileInfo(path);
                }



                // Switch between Audio and Video
                if (!isVideoMode || isStreaming)
                    isVLCVideoPlayer = false;
                else
                    isVLCVideoPlayer = true;



                //audio test
                if (path.Contains(".mp3") && enableVisualisation == true)
                {

                }
                else
                {
                    // check if busy playing
                    if (vlcController.SourceProvider.MediaPlayer != null)
                    {
                        if (!isIdle)
                            LogSystem.AddEvent(logid, "VLC Controller has Player, Play new media");
                        if (isStreaming)
                        {
                            // HTTP input (http)
                            // string[] options = new string[] { "--http-continuous", "--audio-visual=goom", "--goom-height=1080", "--goom-width=1920" };

                            //options[1] = "--sout-keep";
                            ThreadPool.QueueUserWorkItem(_ => vlcController.SourceProvider.MediaPlayer.Play(stream));
                        }
                        else
                        {
                            ThreadPool.QueueUserWorkItem(_ => vlcController.SourceProvider.MediaPlayer.Play(fileInfo));
                        }

                        if (_settings_isCopySettings)
                        {
                            if (!isIdle)
                                LogSystem.AddEvent(logid, "VLC Controller has Player, copy settings between Music & Video Player");
                            vlcController.SourceProvider.MediaPlayer.Audio.IsMute = _settings_mute;
                            vlcController.SourceProvider.MediaPlayer.Audio.Volume = _settings_volume;
                            
                            _settings_isCopySettings = false;
                        }
                    }
                    else
                    {
                        if (!isIdle)
                            LogSystem.AddEvent(logid, "No Player in VLC Controller, Creating New Player");

                        string[] options;
                        if (isVLCVideoPlayer)
                            options = new string[] { "--aout=opensles", "--http-reconnect", "--http-continuous", "--audio-time-stretch", "--network-caching=1500" };
                        //options = new string[] { "--aout=opensles", "--http-reconnect", "--http-continuous", "--audio-time-stretch", "--network-caching=1500", "-vvv" };
                        else
                        {
                            List<string> tempOptions = new List<string>() { "--aout=opensles", "--http-reconnect", "--http-continuous", "--audio-time-stretch", "--network-caching=1500" };
                            //options = new string[] { "--aout=opensles", "--http-reconnect", "--http-continuous", "--audio-time-stretch", "--audio-visual=goom", "--network-caching=1500", "-vvv" };

                            switch (Global.AdminSettings.AudioVisualizerSize)
                            {
                                case Global.ClassAdminSettings.Quality.High: tempOptions.Add("--audio-visual=goom"); tempOptions.Add("--goom-height=1080"); tempOptions.Add("--goom-width=1920"); break;
                                case Global.ClassAdminSettings.Quality.Medium: tempOptions.Add("--audio-visual=goom"); tempOptions.Add("--goom-height=720"); tempOptions.Add("--goom-width=1280"); break;
                                case Global.ClassAdminSettings.Quality.Low: tempOptions.Add("--audio-visual=goom"); tempOptions.Add("--goom-height=540"); tempOptions.Add("--goom-width=960"); break;
                                case Global.ClassAdminSettings.Quality.Off: break;
                            }
                            //  options = new string[] { "--aout=opensles", "--http-reconnect", "--http-continuous", "--audio-time-stretch", "--audio-visual=goom", "--goom-height=1080", "--goom-width=1920", "--network-caching=1500", "-vvv" };
                            options = tempOptions.ToArray();
                        }

                        vlcController.SourceProvider.CreatePlayer(this.vlcLibDirectory, options);
                        vlcController.SourceProvider.MediaPlayer.EndReached += this.EventEndOfTrack;
                        vlcController.SourceProvider.MediaPlayer.PositionChanged += this.EventPositionChanged;
                        // This can also be called before EndInit
                        vlcController.SourceProvider.MediaPlayer.Log += (_, args) =>
                        {
                            string message = $"libVlc : {args.Level} {args.Message} @ {args.Module}";
                            System.Diagnostics.Debug.WriteLine(message);
                        };

                        if (isStreaming)
                            vlcController.SourceProvider.MediaPlayer.Play(stream);
                        else
                            vlcController.SourceProvider.MediaPlayer.Play(fileInfo);

                        if (_settings_isCopySettings)
                        {
                            vlcController.SourceProvider.MediaPlayer.Audio.IsMute = _settings_mute;
                            vlcController.SourceProvider.MediaPlayer.Audio.Volume = _settings_volume;
                            _settings_isCopySettings = false;
                        }
                    }

                    if (!isIdle)
                        LogSystem.EndProcess(logid, new List<string>());
                }
            });

            //NowPlaying.Content = "Now Playing : " + PlayCurrentTrack.Name;
            //NowPlaying.Visibility = Visibility.Visible;
        }

        public void PlayRandomSongs()
        {

            ThreadPool.QueueUserWorkItem(_ => playRandomSongsAsync());

        }
        private async void playRandomSongsAsync()
        {
            int topTakeAmount = 20;

            using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
            {
                var ex = from a in db.Playlists where a.PlaylistId == 1 select a.TrackLibrary;
                //var t = db.TrackLibraries.Where(w => w.Type == Global.AppModeString).Except(ex).OrderBy(a => Guid.NewGuid()).Take(topTakeAmount+5).ToList();
                var t = db.TrackLibraries.Where(w => w.Type == Global.AppModeString).OrderBy(a => Guid.NewGuid()).Take(topTakeAmount + 5).ToList();
                t = t.Except(ex).Take(topTakeAmount).ToList();

                playList.Unload();
                //playList = new PlayList();
                //dbClearNowPlaying();
                isNewNowPlaying = true;
                if (t.Any())
                {
                    Global.AppActionMode = Global.AppActionModeEnum.Playing;
                    isIdle = false;
                    Global.MainWindow.Dispatcher.InvokeAsync(() => Global.MainWindow.InitializePlayer());
                    //ThreadPool.QueueUserWorkItem(_ => Global.MainWindow.HideMenu());
                }
                playList.AddRange(t);
            }

            Play();
        }

        public void StartUserPlaylist(int playlistId, bool playShuffle = false)
        {
            using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
            {
                PlayListDetail _pList = new PlayListDetail();
                isIdle = false;

                _pList = db.PlayListDetails.Find(playlistId);

                bool thisShouldBeImposstible = false;
                if (_pList.Type == 0)
                    thisShouldBeImposstible = true;

                var tracks = from tr in _pList.Playlists.OrderBy(o => o.SequenceNumber)
                             select tr.TrackLibrary;

                playList = new PlayList(_pList, tracks.ToList());

                if (playShuffle)
                {
                    playList.Shuffle = true;
                }
            }

            Play(playList.GetCurrentTrack());
        }

        public void StartPlaylist(string playlistName, bool isSystem, bool shuffle = false)
        {
            using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
            {
                Debug.WriteLine("Loading Playlist : " + playlistName);
                PlayListDetail _pList = new PlayListDetail();

                // index 0 for system playlists
                // added check to see if there are any tracks to play

                //w.Type == 0
                //w.Name == playlistName 
                //isSystem

                //SystemBackground
                //if (isSystem)
                //{
                //    if (db.PlayListDetails.Where(w => w.Name == playlistName && w.Type == 0).Any())
                //        _pList = db.PlayListDetails.Where(w => w.Name == playlistName && w.Type == 0).First();
                //    else
                //    {
                //        isIdle = false;
                //        if (db.PlayListDetails.Where(w => w.Name == playlistName && w.Type > 0).Any())
                //            _pList = db.PlayListDetails.Where(w => w.Name == playlistName && w.Type > 0).First();
                //    }
                //}
                //else
                //{
                //    isIdle = false;
                //    if (db.PlayListDetails.Where(w => w.Name == playlistName && w.Type > 0).Any())
                //    {
                //        _pList = db.PlayListDetails.Where(w => w.Name == playlistName && w.Type > 0).First();
                //    }
                //}

                isIdle = false;
                if (isSystem)
                {
                    if (db.PlayListDetails.Where(w => w.Name == playlistName && w.Type == 0).Any())
                    {
                        if (playlistName == "SystemBackground")
                        {
                            isIdle = true;
                        }
                        _pList = db.PlayListDetails.Where(w => w.Name == playlistName && w.Type == 0).First();
                    }
                }
                else
                {
                    if (db.PlayListDetails.Where(w => w.Name == playlistName && w.Type > 0).Any())
                        _pList = db.PlayListDetails.Where(w => w.Name == playlistName && w.Type > 0).First();
                }


                var tracks = from tr in _pList.Playlists.OrderBy(o => o.SequenceNumber)
                             select tr.TrackLibrary;

                if (shuffle)
                {
                    var sorted = tracks.OrderBy(a => Guid.NewGuid()).ToList();
                    playList = new PlayList(_pList, sorted);
                }
                else
                {
                    playList = new PlayList(_pList, tracks.ToList());
                }
            }

            Play(playList.GetCurrentTrack());
        }

        private void AddHistory(string path, int i = -1)
        {
            //PlayItem pi = new PlayItem();
            //pi.SetPath(path);
            //pi.hasPlayed = true;
            //if (i == -1) { PlayHistory.Add(pi); }
            //else { PlayHistory.Insert(i, pi); }
        }

        private void AddPlayListx(string path, int i = -1)
        {
            PlayItem pi = new PlayItem();
            pi.SetPath(path);
            //if (i == -1)
            //    PlayCue.Add(pi);
            //else
            //    PlayCue.Insert(i, pi);
        }

        public class PlayItem
        {
            public string Path { get; set; }
            public string Name { get; set; }
            public string Extension { get; set; }
            public FileInfo fileInfo { get; set; }
            public bool hasPlayed { get; set; }

            public PlayItem()
            {
                hasPlayed = false;
            }

            public PlayItem(string setPath)
            {
                hasPlayed = false;
                SetPath(setPath);
            }

            public void SetPath(string path)
            {
                Path = path;
                Name = System.IO.Path.GetFileNameWithoutExtension(path);
                Extension = System.IO.Path.GetFileName(path);
                fileInfo = new FileInfo(path);
                hasPlayed = false;
            }
        }

        //internal IEnumerable getNowPlayingItems()
        //{
        //    //var n = (from l in PlayCue
        //    //         select new ListBoxItem() { Tag = l.Path, Content = l.Name, ToolTip = l.Path }).ToList();
        //    //return n;
        //}

        //internal IEnumerable getHistoryItems()
        //{
        //    var n = (from l in PlayHistory
        //             select new ListBoxItem() { Tag = l.Path, Content = l.Name, ToolTip = l.Path }).ToList();
        //    return n;
        //}

        internal void AddToPlaylistx(string fullPath)
        {
            if (isIdle)
            {
                // Lets just create a new list anyway
                playList = new PlayList();
                isIdle = false;
            }
            //playList.Add(new PlayItem(fullPath));
        }

        internal void AddToPlaylist(TrackLibrary trackLibrary)
        {
            if (isIdle)
            {
                // Lets just create a new list anyway
                playList = new PlayList();
                isIdle = false;
                if (!isNewNowPlaying)
                {
                    isNewNowPlaying = true;
                    dbClearNowPlaying();
                }
            }
            playList.Add(trackLibrary);
            NowPlaying.ShowNotification(String.Format("Added track"));
        }
        internal void AddToPlaylist(IEnumerable<TrackLibrary> trackLibrary)
        {
            if (isIdle)
            {
                // Lets just create a new list anyway
                playList = new PlayList();
                isIdle = false;
                if (!isNewNowPlaying)
                {
                    isNewNowPlaying = true;
                    dbClearNowPlaying();
                }
            }
            playList.AddRange(trackLibrary);
            //NowPlaying.ShowNotification(String.Format("Added tracks"));
        }

        internal void RemoveFromPlaylist(TrackLibrary trackLibrary)
        {
            playList.Remove(trackLibrary);
            //NowPlaying.ShowNotification(String.Format("Removed track"));
        }
        internal void RemoveFromPlaylist(IEnumerable<TrackLibrary> trackLibrary)
        {
            playList.RemoveRange(trackLibrary);
            //NowPlaying.ShowNotification(String.Format("Removed tracks"));
        }

        internal void CueNextx(string fullPath)
        {
            if (isIdle)
            {
                // Lets just create a new list anyway
                playList = new PlayList();
                isIdle = false;
            }
            //playList.InsertNext(new PlayItem(fullPath));
        }



        #region Controls

        internal void Next()
        {
            try
            {
                if (Global.mPlayer.SongPlayingArtWork != null)
                {
                    Global.mPlayer.SongPlayingArtWork.Dispatcher.InvokeAsync(() =>
                    {
                        Global.mPlayer.SongPlayingArtWork.PlayNext();
                    });
                }
                else
                {
                    Play(playList.GetNextTrack());
                }
            }
            catch (Exception)
            {

            }
        }

        internal void Previous()
        {
            Play(playList.GetPreviousTrack());
        }

        internal void Pause()
        {
            if (vlcController.SourceProvider.MediaPlayer != null)
            {
                if (vlcController.SourceProvider.MediaPlayer.IsPausable())
                    vlcController.SourceProvider.MediaPlayer.Pause();
                else
                    Play();
            }
            else
            {
                Play();
            }
        }

        internal void Play()
        {
            Play(playList.GetCurrentTrack());
        }

        internal void PlayPlaylistIndex(int PlaylistSequence)
        {
            playList.SetCurrentTrack(PlaylistSequence);
            Play(playList.GetCurrentTrack(true));
        }

        internal void Resume()
        {
            if (vlcController.SourceProvider.MediaPlayer != null)
            {
                vlcController.SourceProvider.MediaPlayer.Play();
            }
        }

        internal void Stop()
        {
            if (vlcController.SourceProvider.MediaPlayer != null)
            {
                ThreadPool.QueueUserWorkItem(_ => vlcController.SourceProvider.MediaPlayer.Stop());
                //vlcController.SourceProvider.Dispose();
            }
        }

        internal void Mute()
        {
            if (vlcController.SourceProvider.MediaPlayer != null)
            {
                //ThreadPool.QueueUserWorkItem(_ => vlcController.SourceProvider.MediaPlayer.Audio.ToggleMute());
                //vlcController.SourceProvider.Dispose();
                vlcController.SourceProvider.MediaPlayer.Audio.ToggleMute();
            }
        }

        internal int VolumeUp(int volume)
        {
            if (vlcController.SourceProvider.MediaPlayer != null)
            {
                //int vol = vlcController.SourceProvider.MediaPlayer.Audio.Volume;
                //if (vol < 200)
                //{
                //    vol += Global.AdminSettings.VolumeIncrement;

                //    ThreadPool.QueueUserWorkItem(_ => vlcController.SourceProvider.MediaPlayer.Audio.Volume = vol);
                //}
                ThreadPool.QueueUserWorkItem(_ => vlcController.SourceProvider.MediaPlayer.Audio.Volume = volume);
                return volume;
            }

            return volume;
        }

        internal int VolumeDown()
        {
            if (vlcController.SourceProvider.MediaPlayer != null)
            {
                int vol = vlcController.SourceProvider.MediaPlayer.Audio.Volume;
                if (vol > 0)
                {
                    vol -= Global.AdminSettings.VolumeIncrement;

                    ThreadPool.QueueUserWorkItem(_ => vlcController.SourceProvider.MediaPlayer.Audio.Volume = vol);
                }
                return vol;
            }
            return 100;

        }

        internal bool IsPlaying()
        {
            if (vlcController.SourceProvider.MediaPlayer != null)
            {
                return vlcController.SourceProvider.MediaPlayer.IsPlaying();
            }
            else
                return false;
        }

        #endregion Controls

        #region DatabaseControls

        public bool dbRenamePlaylist(int playlistID, string newName)
        {
            using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
            {
                // check if name is in use
                if (db.PlayListDetails.Any(w => w.Name == newName.Trim()))
                    return false;

                var play = db.PlayListDetails.Find(playlistID);
                play.Name = newName.Trim();

                db.Entry(play).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
            }
            return true;
        }
        public void dbDeletePlaylist(int playlistID)
        {
            // make sure no system playlists can be deleted
            using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
            {
                var resultA = db.Database.ExecuteSqlCommand(string.Format("DELETE FROM [Playlists] WHERE [PlaylistId] = {0}", playlistID));
                var resultB = db.Database.ExecuteSqlCommand(string.Format("DELETE FROM [PlayListDetails] WHERE [Id] = {0}", playlistID));
                //var result = db.Database.ExecuteSqlCommand(string.Format("UPDATE [Playlists] SET [SequenceNumber] = [SequenceNumber] + {2} WHERE [SequenceNumber] >= {0} AND [PlaylistId] = {1}", newInsertedRecordSeq, playlistID, RecordCountOffset));
            }
        }
        public void dbClearNewImporterPlaylist()
        {
            using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
            {
                // NewImporter
                var result = db.Database.ExecuteSqlCommand(string.Format("DELETE FROM [Playlists] WHERE [PlaylistId] = 3"));
            }
        }
        public void dbClearBufferPlaylist()
        {
            using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
            {
                int i = Global.BufferPlaylistID;
                var result = db.Database.ExecuteSqlCommand(string.Format("DELETE FROM [Playlists] WHERE [PlaylistId] = {0}", i));
            }
        }
        public void dbClearNowPlayingPlaylist()
        {
            using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
            {
                int i = Global.NowPlayingId;
                var result = db.Database.ExecuteSqlCommand(string.Format("DELETE FROM [Playlists] WHERE [PlaylistId] = {0}", i));
            }
        }

        public void dbRemoveTrackFromPlaylist(TrackLibrary track, int playlistID)
        {
            using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
            {
                // removes ALL instances
                var playTracks = db.Playlists.Where(w => w.PlaylistId == playlistID && w.TrackId == track.Id).ToList();

                foreach (var p in playTracks)
                {
                    // recalculate sequence
                    dbPullPlaylistSequence(p.SequenceNumber, playlistID, db);

                    // 2
                    db.Playlists.Remove(p);
                    db.SaveChanges();
                }
            }
        }

        public void dbRemoveTrackFromPlaylist(List<TrackLibrary> tracks, int playlistID)
        {
            using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
            {
                foreach (var t in tracks)
                {
                    // removes ALL instances
                    var playTracks = db.Playlists.Where(w => w.PlaylistId == playlistID && w.TrackId == t.Id).ToList();

                    foreach (var p in playTracks)
                    {
                        // recalculate sequence
                        dbPullPlaylistSequence(p.SequenceNumber, playlistID, db);

                        // 2
                        db.Playlists.Remove(p);
                        db.SaveChanges();
                    }
                }
            }
        }

        public void dbApplyBufferPlaylistChanges(int playlistID)
        {
            // Global.NewTracksPlaylistId
            using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
            {
                int i = Global.BufferPlaylistID;
                var result = db.Database.ExecuteSqlCommand(string.Format("DELETE FROM [Playlists] WHERE [PlaylistId] = {0}", playlistID));
                var result2 = db.Database.ExecuteSqlCommand(string.Format("INSERT INTO [Playlists] ([TrackId],[PlaylistId],[SequenceNumber]) SELECT [TrackId], {1}, [SequenceNumber] FROM [Playlists] WHERE [PlaylistId] = {0}", i, playlistID));
                db.SaveChanges();
            }
        }
        public void dbCloneToBufferPlaylist(int playlistID)
        {
            // Global.NewTracksPlaylistId
            using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
            {
                int i = Global.BufferPlaylistID;
                var result = db.Database.ExecuteSqlCommand(string.Format("DELETE FROM [Playlists] WHERE [PlaylistId] = {0}", i));
                var result2 = db.Database.ExecuteSqlCommand(string.Format("INSERT INTO [Playlists] ([TrackId],[PlaylistId],[SequenceNumber]) SELECT [TrackId], {1}, [SequenceNumber] FROM [Playlists] WHERE [PlaylistId] = {0}", playlistID, i));
                db.SaveChanges();
            }
        }
        public void dbMoveTracksFromNowPlaying(int newPlaylistID, bool deleteExisting = false)
        {
            // Global.NewTracksPlaylistId
            using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
            {
                if (deleteExisting)
                    db.Database.ExecuteSqlCommand(string.Format("delete from [Playlists] WHERE [PlaylistId] = {0}", newPlaylistID));

                var result = db.Database.ExecuteSqlCommand(string.Format("UPDATE [Playlists] SET [PlaylistId] = {0} WHERE [PlaylistId] = {1}", newPlaylistID, Global.NowPlayingId));
                db.SaveChanges();
            }
        }
        public void dbAddTrackToPlaylist(TrackLibrary track, int playlistID, JukeboxBrainsDBEntities db)
        {
            // 1
            // get new sequence
            int i = db.Playlists.Where(w => w.PlaylistId == playlistID).Count();

            // 2
            db.Playlists.Add(new Playlist() { TrackId = track.Id, PlaylistId = playlistID, SequenceNumber = i });
            db.SaveChanges();
        }
        public void dbAddTrackToPlaylist(TrackLibrary track, int playlistID)
        {
            using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
            {
                // 1
                // get new sequence
                int i = db.Playlists.Where(w => w.PlaylistId == playlistID).Count();

                // 2
                db.Playlists.Add(new Playlist() { TrackId = track.Id, PlaylistId = playlistID});
                db.SaveChanges();
            }
        }

        public void dbAddTrackToPlaylist(List<int> trackIDs, int playlistID)
        {
            try
            {
                using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                {
                    // 1
                    // get new sequence
                    int i = db.Playlists.Where(w => w.PlaylistId == playlistID).Count();

                    // 2
                    foreach (int t in trackIDs)
                    {
                        db.Playlists.Add(new Playlist() { TrackId = t, PlaylistId = playlistID, SequenceNumber = i });
                        i++;
                    }
                    db.SaveChanges();
                    //db.SaveChangesAsync();
                }
            }
            catch (Exception)
            {

            }

        }
        public void dbAddTrackToPlaylist(IEnumerable<TrackLibrary> tracks, int playlistID)
        {
            try
            {

                using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                {
                    // 1
                    // get new sequence
                    int i = db.Playlists.Where(w => w.PlaylistId == playlistID).Count();

                    // 2
                    foreach (TrackLibrary t in tracks)
                    {
                        db.Playlists.Add(new Playlist() { TrackId = t.Id, PlaylistId = playlistID, SequenceNumber = i });
                        i++;
                    }
                    db.SaveChanges();
                    //db.SaveChangesAsync();
                    var playlist = db.Playlists.ToList();
                }
            }
            catch (Exception ex)
            {

            }
        }

        public void dbInsertTracksToPlaylist(List<TrackLibrary> tracks, int playIndex, int playlistID)
        {
            try
            {
                using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                {
                    // 1
                    dbBumpPlaylistSequence(playIndex, playlistID, db, tracks.Count);

                    List<Playlist> p = new List<Playlist>();
                    foreach (var t in tracks)
                    {
                        p.Add(new Playlist() { TrackId = t.Id, PlaylistId = playlistID, SequenceNumber = playIndex + p.Count });
                    }

                    db.Playlists.AddRange(p);
                    db.SaveChanges();
                }
            }
            catch (Exception)
            {

            }

        }

        public void dbInsertTrackToPlaylist(TrackLibrary track, int playIndex, int playlistID)
        {
            try
            {
                using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                {
                    // 1

                    var playListDetail = db.Playlists.FirstOrDefault(x => x.TrackId == track.Id);
                    if (playListDetail != null)
                    {
                        playlistID = playListDetail.PlaylistId;
                    }

                    dbBumpPlaylistSequence(playIndex, playlistID, db);
                    // 2
                    db.Playlists.Add(new Playlist() { TrackId = track.Id, PlaylistId = playlistID, SequenceNumber = playIndex });
                    db.SaveChanges();
                }
            }
            catch (Exception)
            {

            }
        }

        public void dbUpdatePlaylistDetailsType(int playlistID, bool isVideo, bool isMusic, bool isKaraoke, bool isRadio)
        {
            try
            {
                using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                {
                    var p = db.PlayListDetails.Find(playlistID);
                    if (p != null && p.Type > 1)
                    {
                        p.isVideo = isVideo;
                        p.isMusic = isMusic;
                        p.isKaraoke = isKaraoke;
                        p.isRadio = isRadio;
                        db.Entry(p).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                    }
                }
            }
            catch (Exception)
            {

            }

        }
        public void dbShufflePlaylist(int playlistID)
        {
            using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
            {
                // 1
                // get count
                var numbers = (from a in db.Playlists
                               where a.PlaylistId == playlistID
                               select a.SequenceNumber).ToList();

                // shuffle count
                var rand = new Random();
                var randomList = numbers.OrderBy(x => rand.Next()).ToList();

                // Update Values
                var playlist = db.Playlists.Where(w => w.PlaylistId == playlistID).OrderBy(o => o.SequenceNumber);
                int i = 0;
                foreach (var p in playlist)
                {
                    p.SequenceNumber = randomList[i];
                    i++;
                    db.Entry(p).State = System.Data.Entity.EntityState.Modified;
                }

                db.SaveChanges();

                var tracklist = (from t in db.Playlists.OrderBy(o => o.SequenceNumber)
                                 where t.PlaylistId == playlistID
                                 select t.TrackLibrary).ToList();
                var _plist = db.PlayListDetails.Find(playlistID);
                playList = new PlayList(_plist, tracklist);
            }
        }

        /// <summary>
        /// Do this BEFORE inserting new record!
        /// </summary>
        /// <param name="newInsertedRecordSeq"></param>
        /// <param name="playlistID"></param>
        /// <param name="db"></param>
        public void dbBumpPlaylistSequence(int newInsertedRecordSeq, int playlistID, JukeboxBrainsDBEntities db, int RecordCountOffset = 1)
        {
            var result = db.Database.ExecuteSqlCommand(string.Format("UPDATE [Playlists] SET [SequenceNumber] = [SequenceNumber] + {2} WHERE [SequenceNumber] >= {0} AND [PlaylistId] = {1}", newInsertedRecordSeq, playlistID, RecordCountOffset));
            foreach (var x in db.Playlists.Where(pi => pi.PlaylistId == playlistID))
            {
                Debug.WriteLine(x.SequenceNumber.ToString());
            }
        }

        /// <summary>
        /// Do this BEFORE removing the record!
        /// </summary>
        /// <param name="newInsertedRecordSeq"></param>
        /// <param name="playlistID"></param>
        /// <param name="db"></param>
        public void dbPullPlaylistSequence(int removeRecordSeq, int playlistID, JukeboxBrainsDBEntities db, int RecordCountOffset = 1)
        {
            var result = db.Database.ExecuteSqlCommand(string.Format("UPDATE [Playlists] SET [SequenceNumber] = [SequenceNumber] - {2} WHERE [SequenceNumber] > {0} AND [PlaylistId] = {1}", removeRecordSeq, playlistID, RecordCountOffset));
            foreach (var x in db.Playlists.Where(pi => pi.PlaylistId == playlistID))
            {
                Debug.WriteLine(x.SequenceNumber.ToString());
            }
        }

        private void dbAddTracksNowPlaying(List<TrackLibrary> tracks, int positionindex)
        {
            using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
            {

            }
        }

        private void dbClearNowPlaying()
        {
            using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
            {
                var result = db.Database.ExecuteSqlCommand(string.Format("DELETE FROM [Playlists] WHERE [PlaylistId] = {0}", 2));
                var result2 = db.Database.ExecuteSqlCommand(string.Format("DELETE FROM [Playlists] WHERE [PlaylistId] = {0}", Global.BufferPlaylistID));
            }
        }

        private void dbSaveNowPlaying()
        {
            using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
            {
                //var p = db.PlayListDetails.Where(w => w.Name == Global.NowPlayingName && w.Type == 0).FirstOrDefault();
                //foreach (var pi in playList)
                //{
                //    p.Playlists.Add(pi);
                //    db.SaveChanges();
                //}
            }
        }






        #endregion DatabaseControls
    }

    public class CurrentTrackInfo
    {
        public string SongName { get; set; }
        public string ArtistName { get; set; }
        public string AlbumName { get; set; }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //D:\Movies
        //string MusicDir = "C:\\Users\\Johan\\Music";
        //string MusicDir = ConfigurationManager.AppSettings.Get("TestDirectory");
        //string TestPlay = ConfigurationManager.AppSettings.Get("TestPlay");


        public List<Page> MenuCarosel { get; set; }
        int SelectedMenu;
        public bool isMenuOpen;
        public bool isMinimalMode;

        private bool isInitialMenuLoad = true;

        // import folder?
        // Settings File

        public static bool YearFilterPopupOpen { get; set; }
        public MainWindow()
        {
            InitializeComponent();
            string errormessage = "";
            try
            {
                WriteLog("Entered to constructor");
                bool hasDB = false;
                using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                {
                    WriteLog("Entered to using block");
                    hasDB = db.Database.Exists();
                    WriteLog("Entered to using block=> hasDB:" + hasDB);
                    if (hasDB)
                    {
                        WriteLog("Entered to using block=> hasDB: if block");
                        // Add Radion Channels
                        if (!db.TrackLibraries.Any(a => a.Type == "Radio"))
                        {
                            WriteLog("no tracklibrary radio found");
                            TrackLibrary track = new TrackLibrary() { FilePath = "http://stream.hive365.co.uk:8088/live", FileName = "hive365", Type = "Radio" };
                            SongLibrary song = new SongLibrary() { SongName = "Hive 365" };

                            track.SongLibraries.Add(song);

                            db.TrackLibraries.Add(track);
                            db.SaveChanges();
                        }

                        if (db.TrackLibraries.Any(a => a.Type == "SystemVideo"))
                        {
                            WriteLog("tracklibrary system video");
                            if (!Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\JukeboxSolutions"))
                            {
                                Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\JukeboxSolutions");
                            }
                            var d = System.IO.Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\JukeboxSolutions", "*.mp4");
                            if (d?.Length > db.TrackLibraries.Count(c => c.Type == "SystemVideo"))
                            {
                                foreach (string f in d)
                                {
                                    FileInfo fi = new FileInfo(f);
                                    if (!db.TrackLibraries.Any(an => an.FilePath == f && an.Type == "SystemVideo"))
                                    {
                                        TrackLibrary track = new TrackLibrary() { Extention = fi.Extension, FileName = fi.Name.Replace(fi.Extension, ""), FilePath = fi.FullName, Type = "SystemVideo" };
                                        db.TrackLibraries.Add(track);
                                        db.SaveChanges();

                                        // 1
                                        // get new sequence
                                        int i = db.Playlists.Where(w => w.PlaylistId == Global.SystemPlaylistID).Count();

                                        // 2
                                        db.Playlists.Add(new Playlist() { TrackId = track.Id, PlaylistId = Global.SystemPlaylistID, SequenceNumber = i });
                                        db.SaveChanges();
                                    }
                                }
                            }
                        }

                        Global.AdminSettings.LoadValues();

                        if (db.AppSettings.Any())
                        {
                            var appSetting = db.AppSettings.FirstOrDefault(x => x.Type == "CurrentThemeSequnce");
                            if (appSetting != null)
                            {
                                Global.AdminSettings.currentThemeSequence = appSetting.Value == string.Empty ? 1 : int.Parse(appSetting.Value);
                            }
                            else
                            {
                                Global.AdminSettings.currentThemeSequence = 1;
                                db.AppSettings.Add(new AppSetting
                                {
                                    Type = "CurrentThemeSequnce",
                                    Subtype = "",
                                    Value = "1"
                                });
                                db.SaveChanges();
                            }

                            var appSetting1 = db.AppSettings.FirstOrDefault(x => x.Type == "FirstTimeLoad");
                            if (appSetting1 != null)
                            {
                                Global.AdminSettings.FirstTimeLoad = appSetting1.Value == string.Empty ? 1 : int.Parse(appSetting1.Value);
                            }
                            else
                            {
                                Global.AdminSettings.FirstTimeLoad = 1;
                                db.AppSettings.Add(new AppSetting
                                {
                                    Type = "FirstTimeLoad",
                                    Subtype = "",
                                    Value = "1"
                                });
                                db.SaveChanges();

                                //var menu2 = new Menu2(ref MainFrame);
                                //menu2.btnResetDatabase_Click(null, null);
                            }
                        }
                    }
                }

                if (hasDB)
                {
                    Global.mPlayer = new MainPlayer(ref vlcControlVideo, ref vlcControlAudio, ref lblNowPlaying, ref lblCurrentTrack, ref OverlayGrid, ref OverlayFrame, ref SongInfo);
                    Global.mPlayer.sliderTrackBar = sliderTrackBar;

                    
                    MenuCarosel = new List<Page>() { new Menu1(ref MainFrame, ref btnLogo), new Settings(), new Pages.ImportModeSelection(), new Settings_Page1(), new Menu2(ref MainFrame), new ImportAnalytics(), new Pages.ImportModeSelection() };
                    SelectedMenu = 0;

                    ShowMenu();

                    Global.mPlayer.SetIdle();
                    Global.MainFrame = MainFrame;

                    Logs.ResetLog();
                    Global.MainWindow = this;
                    Global.mPlayer.Notifactions = popupNotifications;

                    // TODO: THIS NEEDS TO BE ACTIVE!
                    Global.mPlayer.dbClearNewImporterPlaylist();

#if (DEBUG)
                    Global.mPlayer.Mute();
#endif

                    bool SkipAutoUpdate = true;
                    if (SkipAutoUpdate == false)
                    {
                        using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                        {
                            if (db.Playlists.Any() && db.Playlists.Where(w => w.PlaylistId == Global.AutoUpdateTodoListPlaylistId).Any())
                            {
                                // kick off import all
                                // create managed files

                                bool isBusyLoading = true;
                                // if in db, then add to playlist
                                var templist = (from l in db.Playlists.Where(w => w.PlaylistId == Global.AutoUpdateTodoListPlaylistId)
                                                select l.TrackLibrary).ToList();

                                List<ImportFactory.ManagedFile> list = new List<ImportFactory.ManagedFile>();

                                foreach (var l in templist)
                                {
                                    list.Add(new ImportFactory.ManagedFile(l));
                                }

                                if (list.Any())
                                {
                                    // clear db
                                    Global.mPlayer.dbRemoveTrackFromPlaylist(templist, Global.AutoUpdateTodoListPlaylistId);
                                    iTunesSearchManager iManager = new iTunesSearchManager();
                                    iManager.AutoUpdateTreadedTracksV2(list.ToList());
                                }
                            }
                        }
                    }

                    if (errormessage != "")
                        Global.mPlayer.ShowNotification(errormessage);

                }

                Assembly assembly = Assembly.GetExecutingAssembly();
                FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
                string version = fileVersionInfo.ProductVersion;
                if (Global.AdminSettings.StartupVersion != version)
                {
                    if (Global.mPlayer != null)
                    {
                        Global.mPlayer.popUp_Grid.Visibility = Visibility.Visible;
                        Global.mPlayer.popUp_Frame.Content = new ChangeLog_PopUp();
                    }
                }

                btnKeyboard.Visibility = Visibility.Visible;
                ToggleButton buttHash = GetDisabledNewIndexButton('#');
                for (char letter = 'A'; letter <= 'Z'; letter++)
                {
                    ToggleButton newB;
                    newB = GetDisabledNewIndexButton(letter);
                    newB.IsEnabled = false;

                    sPanel.Children.Add(newB);
                }

                sParentPanel.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                Write(ex);
            }
        }
        public ToggleButton GetEnabledNewIndexButton(char content)
        {
            //Button t = btnTemplate;

            ToggleButton b = new ToggleButton();
            b.Content = content;
            //b.Click += new RoutedEventHandler(btnIndex_Click);

            b.Style = FindResource("EnabledToggleButtonStyle") as Style;
            b.IsEnabled = false;

            return b;
        }

        public ToggleButton GetDisabledNewIndexButton(char content)
        {
            //Button t = btnTemplate;

            ToggleButton b = new ToggleButton();
            b.Content = content;
            //b.Click += new RoutedEventHandler(btnIndex_Click);

            b.Style = FindResource("DisabledToggleButtonStyle") as Style;
            b.IsEnabled = false;

            return b;
        }

        public void Write(Exception exception)
        {
            try
            {

                using (StreamWriter sr = File.AppendText("C:\\Log\\result.txt")) //new StreamWriter("result.txt", Encoding. ))
                {
                    // Get stack trace for the exception with source file information
                    var st = new StackTrace(exception, true);
                    // Get the top stack frame
                    var frame = st.GetFrame(0);
                    // Get the line number from the stack frame
                    var line = frame.GetFileLineNumber();
                    sr.WriteLine("=>" + DateTime.Now + " " + " An Error occurred: " + exception.StackTrace +
                        " Message: " + exception.Message + "\n\n" + exception.InnerException);
                    sr.Flush();
                }
            }

            catch (Exception e)
            {

            }

        }

        public void WriteLog(string text)
        {
            try
            {

                using (StreamWriter sr = File.AppendText("C:\\Log\\LogT.txt")) //new StreamWriter("result.txt", Encoding. ))
                {
                    sr.WriteLine("=>" + DateTime.Now + " " + text);
                    sr.Flush();
                }
            }

            catch (Exception e)
            {

            }

        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // Next

        }

        private Key keyPlay = Key.S;
        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            // Play / Pause

            doPlayPause();
            btnPlayToggle();
        }

        //private void btnPlayReset()
        //{
        //    btnPlay1.Visibility = Visibility.Visible;
        //    btnPause.Visibility = Visibility.Collapsed;
        //}

        private bool isPlaying = false;
        private void btnPlayToggle()
        {
            if (!isPlaying)
            {
                btnPlay1.Visibility = Visibility.Visible;
                btnPause.Visibility = Visibility.Collapsed;
            }
            else
            {
                btnPlay1.Visibility = Visibility.Collapsed;
                btnPause.Visibility = Visibility.Visible;
            }
        }

        private void doPlayPause()
        {
            if (Global.mPlayer.IsPlaying())
            {
                Global.mPlayer.Pause();
                isPlaying = false;
            }
            else
            {
                Global.mPlayer.Pause();
                isPlaying = true;
            }
        }

        private Key keyNext = Key.S;
        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            if (Global.mPlayer.SongPlayingArtWork != null)
            {
                Global.mPlayer.SongPlayingArtWork.PlayNext();
            }
            else
            {
                Global.mPlayer.Next();
                isPlaying = true;
                btnPlayToggle();
            }
        }

        private Key keyPrevious = Key.S;
        private void btnPrev_Click(object sender, RoutedEventArgs e)
        {
            if (Global.mPlayer.SongPlayingArtWork != null)
            {
                Global.mPlayer.SongPlayingArtWork.PlayPrevious();
            }
            else
            {
                Global.mPlayer.Previous();
                isPlaying = true;
                btnPlayToggle();
            }
            // Prev

        }

        private Key keyStop = Key.S;
        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            // Prev
            Global.mPlayer.Stop();
            isPlaying = false;
            btnPlayToggle();
        }

        private void getDirectoryListings2()
        {
            List<ListBoxItem> items = new List<ListBoxItem>();
            string[] fileformats = { ".mp3", ".mp4", ".mkv" };
            foreach (string musicFilePath in Directory.EnumerateFiles("", "*", SearchOption.AllDirectories))
            {
                //System.IO.Path.GetFileName(musicFilePath);

                //if(true)
                if (fileformats.Contains(System.IO.Path.GetExtension(musicFilePath)))
                {
                    items.Add(new ListBoxItem() { ToolTip = musicFilePath, Content = System.IO.Path.GetFileNameWithoutExtension(musicFilePath), Tag = musicFilePath });
                    //items.Add(new ListBoxItem() { ToolTip = musicFilePath, Content = System.IO.Path.GetExtension(musicFilePath), Tag = musicFilePath });
                }

            }

            // lvLibrary.ItemsSource = items;
        }

        private void btnPreviousMenu_Click(object sender, RoutedEventArgs e)
        {
            // if library mode
            MainFrame.Content = getPrevMenu();
            //btnKeyboard.Visibility = Visibility.Collapsed;
            //btnMediaPanel.Visibility = Visibility.Collapsed;
            sParentPanel.Visibility = Visibility.Collapsed;
            PlaylistControlPanel.Visibility = Visibility.Visible;
            ControlPanelBG.Visibility = Visibility.Visible;
            //VolumnPanel.Visibility = Visibility.Visible;
            if (SelectedMenu != 0)
            {
                btnSearch.IsEnabled = false;
            }
            else
            {
                btnKeyboard.Visibility = Visibility.Visible;
            }
        }

        private void btnNextMenu_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Content = getNextMenu();
            //btnKeyboard.Visibility = Visibility.Collapsed;
            //btnMediaPanel.Visibility = Visibility.Collapsed;
            sParentPanel.Visibility = Visibility.Collapsed;
            PlaylistControlPanel.Visibility = Visibility.Visible;
            ControlPanelBG.Visibility = Visibility.Visible;
            //VolumnPanel.Visibility = Visibility.Visible;
            if (SelectedMenu != 0)
            {
                btnSearch.IsEnabled = false;
            }
            else
            {
                //VolumnPanel.Visibility = Visibility.Visible;
                PlaylistControlPanel.Visibility = Visibility.Visible;
                btnSearch.Visibility = Visibility.Visible;
                ControlPanelBG.Visibility = Visibility.Visible;
                btnKeyboard.Visibility = Visibility.Visible;
            }
        }






        #region Menu Controls


        private Page getNextMenu()
        {
            if (SelectedMenu == MenuCarosel.Count() - 1)
                SelectedMenu = 0;
            else
                SelectedMenu += 1;

            return MenuCarosel[SelectedMenu];
        }

        private Page getPrevMenu()
        {
            if (SelectedMenu == 0)
                SelectedMenu = MenuCarosel.Count() - 1;
            else
                SelectedMenu -= 1;

            return MenuCarosel[SelectedMenu];
        }

        public void GoToMainMenu()
        {
            isInitialMenuLoad = false;
            ShowMenu();
        }

        public void ShowMenu()
        {
            SelectedMenu = 0;
            MainFrame.Content = MenuCarosel[SelectedMenu];

            isMenuOpen = true;

            btnShuffle.IsChecked = Global.mPlayer.playList.Shuffle;
            if (Global.mPlayer.isIdle == false)
            {
                if (Global.mPlayer.IsPlaying())
                    isPlaying = true;
                else
                    isPlaying = false;

                btnPlayToggle();
            }

            MenuToggle(Visibility.Visible);
        }

        public void HideMenu()
        {
            MainFrame.Content = null;

            isMenuOpen = false;
            lblNowPlaying.Visibility = Visibility.Collapsed;

            MenuToggle(Visibility.Collapsed);
            MainBackgroundBG.Visibility = Visibility.Collapsed;
            ControlPanelBG.Visibility = Visibility.Collapsed;
            //btnMenuMinimize.Visibility = Visibility.Collapsed;
        }

        private bool _isExpandedMode = false;
        public void MenuMinimalToggle(Visibility mode)
        {
            if (mode == Visibility.Visible)
            {
                if (Global.AdminSettings.isTextButtonOverlay)
                    //textOverlay.Visibility = Visibility.Visible;
                    isMinimalMode = false;
                //btnExpandControls.Visibility = Visibility.Collapsed;
                if (_isExpandedMode)
                    rectControlsExpandBG.Visibility = Visibility.Visible;
                else
                    rectControlsExpandBG.Visibility = Visibility.Collapsed;
            }
            else
            {
                //textOverlay.Visibility = Visibility.Collapsed;
                isMinimalMode = true;

                if (!Global.mPlayer.isIdle && isMenuOpen == true)
                    //btnExpandControls.Visibility = Visibility.Visible;
                    if (_isExpandedMode)
                    {
                        rectControlsExpandBG.Visibility = Visibility.Collapsed;
                        //btnExpandControls.Visibility = Visibility.Visible;
                        _isExpandedMode = false;
                    }
            }



            //btnShuffle.Visibility = mode;
            //btnRepeat.Visibility = mode;
            //btnRWD.Visibility = mode;
            //btnPlaylist.Visibility = mode;
            ////btnStop.Visibility = mode;
            //btnFFD.Visibility = mode;
            //btnNextMenu.Visibility = mode;
            //btnPreviousMenu.Visibility = mode;

            //VolumnPanel.Visibility = mode;
            //lblVolume.Visibility = mode;

            //btnMute.Visibility = mode;


            //sliderTrackBar.Visibility = mode;
            //lblTime1.Visibility = mode;
            //lblTime2.Visibility = mode;

            //if (Global.mPlayer.isIdle || isInitialMenuLoad == true)
            //{
            //    sliderTrackBar.Visibility = Visibility.Collapsed;
            //    lblTime1.Visibility = Visibility.Collapsed;
            //    lblTime2.Visibility = Visibility.Collapsed;
            //}

        }

        public void MenuToggle(Visibility mode)
        {
            btnNextMenu.Visibility = mode;
            btnPreviousMenu.Visibility = mode;

            //if (mode == Visibility.Collapsed)
            //{
            //    btnPause.Visibility = mode;
            //    btnPlay1.Visibility = mode;
            //    //btnExpandControls.Visibility = mode;
            //}

            MenuMinimalToggle(mode);


            if (mode == Visibility.Visible)
            {
                btnLogo.Opacity = 1;
                PlaylistControlPanel.Visibility = Visibility.Visible;
                btnSearch.Visibility = mode;
                btnInfo.Opacity = 0.71;
                btnInfo.IsEnabled = false;
                if (Global.mPlayer.isIdle || isInitialMenuLoad == true)
                {
                    //sliderTrackBar.Visibility = Visibility.Collapsed;
                    //lblTime1.Visibility = Visibility.Collapsed;
                    //lblTime2.Visibility = Visibility.Collapsed;

                    //btnPlaylist.IsEnabled = false;
                    //btnPlay1.IsEnabled = false;
                    //btnPlay1.Visibility = Visibility.Collapsed;
                    //btnPause.IsEnabled = false;
                    //btnStop.IsEnabled = false;
                    //btnFFD.IsEnabled = false;
                    //btnRWD.IsEnabled = false;
                    //btnRepeat.IsEnabled = false;
                    //btnShuffle.IsEnabled = false;
                    //btnInfo.Visibility = Visibility.Visible;
                }
                else
                {
                    //btnMenuMinimize.Visibility = Visibility.Visible;
                    MainFrame.Visibility = Visibility.Collapsed;
                    btnPreviousMenu.Visibility = Visibility.Collapsed;
                    btnNextMenu.Visibility = Visibility.Collapsed;

                    btnPlaylist.IsEnabled = true;
                    btnPlay1.IsEnabled = true;
                    btnPause.IsEnabled = true;
                    //btnStop.IsEnabled = true;
                    btnFFD.IsEnabled = true;
                    btnRWD.IsEnabled = true;
                    btnRepeat.IsEnabled = true;
                    btnShuffle.IsEnabled = true;
                    btnInfo.Visibility = Visibility.Visible;
                    PlaylistControlPanel.Visibility = Visibility.Visible;
                }
            }
            else
            {
                if (Global.mPlayer.IsPlaying())
                {
                    btnLogo.Opacity = 0.25;
                    PlaylistControlPanel.Visibility = Visibility.Collapsed;
                    btnSearch.Visibility = Visibility.Collapsed;
                    btnInfo.Visibility = Visibility.Visible;
                }


                //if (Global.PinTrackInfo)
                //{
                //    btnInfo.IsChecked = true;
                //    btnInfo.Opacity = 1;
                //    Global.mPlayer.ShowNowPlayingInfo();
                //}
                //else
                //{
                //    btnInfo.IsChecked = false;
                //    btnInfo.Opacity = 1;
                //}
            }
            //btnFFD.Visibility = mode;
            //btnRWD.Visibility = mode;
            //btnPlaylist.Visibility = mode;
            //btnStop.Visibility = mode;

        }


        #endregion Menu Controls


        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (!Global.isKeyboardFocus)
            {
                switch (e.Key)
                {
                    case Key.M:
                        if (isMenuOpen)
                            HideMenu();
                        else
                            ShowMenu();
                        break;

                    case Key.Space:
                        doPlayPause();
                        break;
                    case Key.S:
                        Global.mPlayer.Stop();
                        break;
                    case Key.N:
                        Global.mPlayer.Next();
                        break;
                    case Key.P:
                        Global.mPlayer.Previous();
                        break;
                }
            }
        }

        private bool isCurrentContentArtWork = false;
        public void btnLogo_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Dispatcher.InvokeAsync(() =>
            {
                MusicLibraryFrame.Visibility = Visibility.Collapsed;
                KaraokeLibraryFrame.Visibility = Visibility.Collapsed;
                RadioLibraryFrame.Visibility = Visibility.Collapsed;
                VideoLibraryFrame.Visibility = Visibility.Collapsed;
                MainFrame.Visibility = Visibility.Visible;
                var mainFrame = Global.MainFrame;
                var menu1 = new Menu1(ref mainFrame, ref btnLogo);
                MenuCarosel[0] = menu1;
                //LogoOnCLick();
                btnKeyboard.Visibility = Visibility.Visible;
                sParentPanel.Visibility = Visibility.Collapsed;
                btnMediaPanel.Visibility = Visibility.Collapsed;
                PlaylistControlPanel.Visibility = Visibility.Visible;
                SelectedMenu = 0;
                MainFrame.Content = MenuCarosel[SelectedMenu];
                btnPreviousMenu.Visibility = Visibility.Visible;
                btnNextMenu.Visibility = Visibility.Visible;
                LibraryButtonControlPanel.Visibility = Visibility.Collapsed;
            });
        }

        public void CreateLibraryInstances()
        {
            try
            {
                var mainFrame = Global.MainFrame;

                Global.MainWindow.CurrentKaraokeLibraryInstnace.Dispatcher.InvokeAsync(() =>
                {
                    Global.AppMode = Global.AppModeEnum.Karaoke;
                    Global.MainWindow.CurrentKaraokeLibraryInstnace = new Library(ref mainFrame, Library.LibraryMode.Library, currentAlbumIndex: 0);
                });
                Global.MainWindow.CurrentVideoLibraryInstnace.Dispatcher.InvokeAsync(() =>
                {
                    Global.AppMode = Global.AppModeEnum.Video;
                    Global.MainWindow.CurrentVideoLibraryInstnace = new Library(ref mainFrame, Library.LibraryMode.Library, currentAlbumIndex: 0);
                });
                Global.MainWindow.CurrentRadioLibraryInstnace.Dispatcher.InvokeAsync(() =>
                {
                    Global.AppMode = Global.AppModeEnum.Radio;
                    Global.MainWindow.CurrentRadioLibraryInstnace = new Library(ref mainFrame, Library.LibraryMode.Library, currentAlbumIndex: 0);
                });
                Global.MainWindow.CurrentLibraryInstnace.Dispatcher.InvokeAsync(() =>
                {
                    Global.AppMode = Global.AppModeEnum.Music;
                    Global.MainWindow.CurrentLibraryInstnace = new Library(ref mainFrame, Library.LibraryMode.Library, currentAlbumIndex: 0);
                });
            }
            catch (Exception)
            {

            }
        }
        private void LogoOnCLick()
        {
            try
            {
                Global.MainFrame.Visibility = Visibility.Visible;
                Global.mPlayer.popUp_Grid.Visibility = Visibility.Collapsed;
                Global.mPlayer.popUp_Frame.Content = null;
                Library.IsLoading = false;
                btnKeyboard.Visibility = Visibility.Visible;
                btnMediaPanel.Visibility = Visibility.Collapsed;
                btnMediaPanel_Click(null, null);
                SelectedMenu = 0;
                MainFrame.Content = MenuCarosel[SelectedMenu];
                btnPreviousMenu.Visibility = Visibility.Visible;
                btnNextMenu.Visibility = Visibility.Visible;
                if (!Global.mPlayer.IsPlaying())
                {
                    btnInfo.Opacity = 0.71;
                    btnInfo.IsEnabled = false;
                    btnPlay1.Visibility = Visibility.Visible;
                    btnShuffle.Visibility = Visibility.Visible;
                    btnRepeat.Visibility = Visibility.Visible;
                    btnRWD.Visibility = Visibility.Visible;
                    btnFFD.Visibility = Visibility.Visible;
                    btnPlaylist.Visibility = Visibility.Visible;
                    PlaylistControlPanel.Visibility = Visibility.Visible;
                    btnPreviousMenu.Visibility = Visibility.Visible;
                    btnNextMenu.Visibility = Visibility.Visible;
                    //VolumnPanel.Visibility = Visibility.Visible;
                    btnSearch.Visibility = Visibility.Visible;
                    ControlPanelBG.Visibility = Visibility.Visible;
                    //VolumnPanel.Visibility = Visibility.Visible;

                    //if (Global.mPlayer.IsPlaying())
                    //{
                    //    btnPlay1.Visibility = Visibility.Collapsed;
                    //    btnPause.IsEnabled = true;
                    //    btnPlay1.IsEnabled = true;
                    //    btnPause.Visibility = Visibility.Visible;
                    //    sliderTrackBar.Visibility = Visibility.Visible;
                    //    ControlPanelBG.Visibility = Visibility.Collapsed;
                    //    Global.MainWindow.PlaylistControlPanel.Height = 120;
                    //}
                    //else
                    //{
                    //    btnPlay1.Visibility = Visibility.Visible;
                    //    btnPause.Visibility = Visibility.Collapsed;
                    //    ControlPanelBG.Visibility = Visibility.Visible;
                    //    //sliderTrackBar.Visibility = Visibility.Collapsed;
                    //}

                    btnLogo.Opacity = 1;
                }
                else
                {
                    if (Global.mPlayer.IsPlaying())
                    {
                        btnInfo.Opacity = 1;
                        btnInfo.IsEnabled = true;
                        if (Global.mPlayer.SongPlayingArtWork != null && !isCurrentContentArtWork)
                        {
                            MainFrame.Content = Global.mPlayer.SongPlayingArtWork;
                            isCurrentContentArtWork = true;
                            Global.MainWindow.sParentPanel.Visibility = Visibility.Collapsed;
                            Global.MainWindow.btnKeyboard.Visibility = Visibility.Collapsed;
                            Global.MainWindow.btnMediaPanel.Visibility = Visibility.Collapsed;
                            Global.MainWindow.PlaylistControlPanel.Visibility = Visibility.Visible;
                            Global.MainWindow.ControlPanelBG.Visibility = Visibility.Visible;
                            btnPreviousMenu.Visibility = Visibility.Collapsed;
                            btnNextMenu.Visibility = Visibility.Collapsed;
                            Global.MainWindow.PlaylistControlPanel.Height = 120;
                        }
                        else
                        {
                            isCurrentContentArtWork = false;
                            //VolumnPanel.Visibility = Visibility.Visible;
                            Global.MainWindow.btnPlay1.IsEnabled = true;
                            Global.MainWindow.btnPlay1.Visibility = Visibility.Collapsed;
                            Global.MainWindow.btnPause.IsEnabled = true;
                            Global.MainWindow.btnPause.Visibility = Visibility.Visible;
                            //Global.MainWindow.VolumnPanel.Visibility = Visibility.Visible;
                            Global.MainWindow.ControlPanelBG.Visibility = Visibility.Visible;
                        }


                    }
                }
            }
            catch (Exception)
            {

            }
        }

        private void btnPlaylist_Click(object sender, RoutedEventArgs e)
        {
            SelectedMenu = 0;
            //Global.mPlayer.TogglePopUp();

            Global.isKeyboardFocus = true;
            //if (Global.mPlayer.SongPlayingArtWork != null)
            //{
            //    Global.mPlayer.popUp_Grid.Visibility = Visibility.Visible;
            //    Global.mPlayer.popUp_Frame.HorizontalAlignment = HorizontalAlignment.Stretch;
            //    var blurEffect = new BlurEffect() { Radius = 4, RenderingBias = RenderingBias.Quality };
            //    blurEffect.KernelType = KernelType.Box;
            //    Global.mPlayer.SongPlayingArtWork.Effect = blurEffect;
            //    Global.mPlayer.popUp_Frame.Content = new LibraryPopup(Global.mPlayer.SongPlayingArtWork.AlbumLibrary, Global.mPlayer.SongPlayingArtWork.BaseControlCard, false);
            //    Global.mPlayer.popUp_Frame.ContentRendered += PopUp_Frame_ContentRendered;
            //}            
            
            isMenuOpen = true;
            Global.mPlayer.popUp_Grid.Visibility = Visibility.Visible;
            Global.mPlayer.popUp_Frame.HorizontalAlignment = HorizontalAlignment.Stretch;
            Global.mPlayer.popUp_Grid.Visibility = Visibility.Visible;
            Global.mPlayer.popUp_Frame.HorizontalAlignment = HorizontalAlignment.Stretch;
            //var blurEffect = new BlurEffect() { Radius = 4, RenderingBias = RenderingBias.Quality };
            //blurEffect.KernelType = KernelType.Box;
            //Global.mPlayer.SongPlayingArtWork.Effect = blurEffect;
            Global.mPlayer.popUp_Frame.Content = new PlaylistPopup();
            Global.mPlayer.popUp_Frame.ContentRendered += PopUp_Frame_ContentRendered;
            //MenuToggle(Visibility.Visible);
        }

        private void PopUp_Frame_ContentRendered(object sender, EventArgs e)
        {
            try
            {
                if (!Global.mPlayer.popUp_Frame.HasContent)
                {
                    if (Global.mPlayer.SongPlayingArtWork != null)
                    {
                        Global.mPlayer.SongPlayingArtWork.Effect = null;
                    }
                }
            }
            catch (Exception)
            {

            }
           
        }

        private void Rectangle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Global.mPlayer.TogglePopUp();
        }


        private void btnVolDown_Click(object sender, RoutedEventArgs e)
        {
            //lblVolume.Content = Global.mPlayer.VolumeDown() + "%";
        }

        private void btnVolUp_Click(object sender, RoutedEventArgs e)
        {
            //lblVolume.Content = Global.mPlayer.VolumeUp() + "%";
        }

        bool isWait = false;
        private void sliderTrackBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                // load values?
                if (isWait)
                {
                    if (e.NewValue == e.OldValue)
                        isWait = false;
                }
                else
                {
                    if (sliderTrackBar.Tag.ToString() == "vlc")
                    {
                        lblTime1.Content = String.Format("{0:D2}:{1:D2}", Global.mPlayer.time1.Minutes, Global.mPlayer.time1.Seconds);
                        lblTime2.Content = String.Format("{0:D2}:{1:D2}", Global.mPlayer.time2.Minutes, Global.mPlayer.time2.Seconds);
                        sliderTrackBar.Tag = "";
                    }
                    else if (sliderTrackBar.Tag.ToString() == "")
                    {
                        Global.mPlayer.SetTrackbar(e.OldValue);
                        sliderTrackBar.Tag = "wait";
                        //isWait = true;
                    }
                }
            }
            catch (Exception)
            {

            }
            
        }

        #region AlertNotifications

        private bool _hasAlerts = false;
        public bool hasUserAlerts
        {
            get { return _hasAlerts; }
            set
            {
                _hasAlerts = value;
                //if (value == true)
                //{
                //    AlertNotifications.Visibility = Visibility.Visible;
                //}
                //else
                //{
                //    AlertNotifications.Visibility = Visibility.Collapsed;
                //}

                //if (MainFrame.Content != null)
                //    switch (MainFrame.Content.GetType().Name)
                //    {
                //        case "Menu1":
                //            AlertNotifications.HorizontalAlignment = HorizontalAlignment.Center;
                //            break;
                //        case "Menu2":
                //            AlertNotifications.HorizontalAlignment = HorizontalAlignment.Center;
                //            break;
                //        default:
                //            AlertNotifications.HorizontalAlignment = HorizontalAlignment.Left;
                //            break;
                //    }
            }
        }

        public VolumnControlPanel VolumeControlPanel { get; set; }

        public bool KaraokeLibraryInstancesLoaded { get; set; }
        public bool MusicLibraryInstancesLoaded { get; set; }
        public bool VideoLibraryInstancesLoaded { get; set; }
        public bool RadioLibraryInstancesLoaded { get; set; }
        public List<int> BufferSelectedAlbums { get; set; }
        public List<UniformGrid> LibraryContentCarousel { get; set; } = new List<UniformGrid>();
        public Library CurrentLibraryInstnace { get; set; }
        public Library CurrentKaraokeLibraryInstnace { get; set; }
        public Library CurrentVideoLibraryInstnace { get; set; }
        public Library CurrentRadioLibraryInstnace { get; set; }
        public int MaxLibraryCarouselIndex { get; set; }

        private void AlertNotifications_Click(object sender, RoutedEventArgs e)
        {
            Global.mPlayer.popUp_Grid.Visibility = System.Windows.Visibility.Visible;
            Global.mPlayer.popUp_Frame.Content = new Popup_Choices();
        }

        private void btnMenuMinimize_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Visibility = Visibility.Visible;

            btnPreviousMenu.Visibility = Visibility.Visible;
            btnNextMenu.Visibility = Visibility.Visible;

            //btnMenuMinimize.Visibility = Visibility.Collapsed;
        }

        private void btnShuffle_Checked(object sender, RoutedEventArgs e)
        {
            Global.mPlayer.playList.Shuffle = true;
        }

        private void btnShuffle_Unchecked(object sender, RoutedEventArgs e)
        {
            Global.mPlayer.playList.Shuffle = false;
        }

        private void btnRepeat_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void btnRepeat_Unchecked(object sender, RoutedEventArgs e)
        {

        }

        private void btnMute_Checked(object sender, RoutedEventArgs e)
        {
            Global.mPlayer.Mute();
        }

        private void btnMute_Unchecked(object sender, RoutedEventArgs e)
        {
            Global.mPlayer.Mute();
        }


        internal void ReloadControls()
        {
            //System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
            //Application.Current.Shutdown();
        }


        private ToggleButton CopyToggleButton(ToggleButton tOld)
        {
            ToggleButton t = new ToggleButton();

            t.IsChecked = tOld.IsChecked;
            t.Visibility = tOld.Visibility;
            t.Height = tOld.Height;
            t.Width = tOld.Width;
            t.Content = tOld.Content;
            t.Style = tOld.Style;
            t.Margin = tOld.Margin;
            t.Padding = tOld.Padding;
            t.IsEnabled = tOld.IsEnabled;
            t.Name = tOld.Name;

            return t;
        }



        private void btnExpandControls_Click(object sender, RoutedEventArgs e)
        {
            _isExpandedMode = true;
            MenuMinimalToggle(Visibility.Visible);
        }

        private void rectControlsExpandBG_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MenuMinimalToggle(Visibility.Collapsed);
            _isExpandedMode = false;
        }

        private void btnInfo_Click(object sender, RoutedEventArgs e)
        {
            Global.PinTrackInfo = true;
            Global.mPlayer.ShowNowPlayingSongInfo();
            btnInfo.Visibility = Visibility.Collapsed;
            CloseInfoButton.Visibility = Visibility.Visible;
        }
        private void btnCloseInfo_Click(object sender, RoutedEventArgs e)
        {
            btnInfo.Visibility = Visibility.Visible;
            CloseInfoButton.Visibility = Visibility.Collapsed;
            Global.PinTrackInfo = false;
            Global.mPlayer.HideNowPlayingSongInfo();
            Global.MainWindow.SongInfo.Dispatcher.Invoke(new Action(() => { Global.MainWindow.SongInfo.Visibility = System.Windows.Visibility.Collapsed; }));
        }

        private void OnVolumeChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            //if (lblVolume != null)
            //{
            //    lblVolume.Content = Global.mPlayer.VolumeUp(Convert.ToInt32(e.NewValue)) + "%";
            //}

        }

        private void BtnMuteClick(object sender, RoutedEventArgs e)
        {
            Global.mPlayer.Mute();
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            btnLogo.IsEnabled = false;
            Global.mPlayer.popUp_Grid.Visibility = Visibility.Visible;
            Global.mPlayer.popUp_Frame.Content = new SearchPopup();
        }

        internal void InitializePlayer()
        {
            MainFrame.Content = null;
            btnLogo.Opacity = 0.25;
            MainFrame.Visibility = Visibility.Visible;
            PlaylistControlPanel.Visibility = Visibility.Collapsed;
            //VolumnPanel.Visibility = Visibility.Collapsed;
            btnSearch.Visibility = Visibility.Collapsed;
            btnPreviousMenu.Visibility = Visibility.Collapsed;
            btnNextMenu.Visibility = Visibility.Collapsed;
            MainBackgroundBG.Visibility = Visibility.Collapsed;
            ControlPanelBG.Visibility = Visibility.Collapsed;
            btnKeyboard.Visibility = Visibility.Collapsed;
            btnMediaPanel.Visibility = Visibility.Collapsed;
            sParentPanel.Visibility = Visibility.Collapsed;
        }

        private void btnAdminMenu_click(object sender, RoutedEventArgs e)
        {
            //if (stackAdminMenu.Visibility == Visibility.Collapsed)
            //{
            //    stackAdminMenu.Visibility = Visibility.Visible;
            //}
            //else
            //{
            //    stackAdminMenu.Visibility = Visibility.Collapsed;
            //}

        }

        private void btnKeyBoard_Click(object sender, RoutedEventArgs e)
        {
            btnMediaPanel.Visibility = Visibility.Visible;
            btnKeyboard.Visibility = Visibility.Collapsed;
            PlaylistControlPanel.Visibility = Visibility.Collapsed;
            sParentPanel.Visibility = Visibility.Visible;
        }

        private void btnMediaPanel_Click(object sender, RoutedEventArgs e)
        {
            btnMediaPanel.Visibility = Visibility.Collapsed;
            btnKeyboard.Visibility = Visibility.Visible;
            PlaylistControlPanel.Visibility = Visibility.Visible;
            sParentPanel.Visibility = Visibility.Collapsed;
        }

        private void CloseInfo(object sender, MouseButtonEventArgs e)
        {
            btnInfo.Visibility = Visibility.Visible;
            CloseInfoButton.Visibility = Visibility.Collapsed;
            Global.PinTrackInfo = false;
            Global.mPlayer.HideNowPlayingSongInfo();
            Global.MainWindow.SongInfo.Dispatcher.Invoke(new Action(() => { Global.MainWindow.SongInfo.Visibility = System.Windows.Visibility.Collapsed; }));
        }

        private void btnNext10_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Global.mPlayer.SongPlayingArtWork != null)
                {
                    Global.mPlayer.SongPlayingArtWork._buttonRightArrow_Click(null, null);
                }
            }
            catch (Exception)
            {

            }
        }






        #endregion AlertNotifications


        // Play...
        // next in row
        // loop
        // shuffle
        // add to now playing

        // drag and drop import

    }
}
