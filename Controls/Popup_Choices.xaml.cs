using iTunesSearch.Library.Models;
using System;
using System.Collections.Generic;
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
using static iTunesSearch.Library.iTunesSearchManager;

namespace JukeBoxSolutions.Controls
{
    /// <summary>
    /// Interaction logic for Popup_Choices.xaml
    /// </summary>
    public partial class Popup_Choices : UserControl
    {
        bool hasNotificationsA = false;
        bool hasNotificationsB = false;
        bool hasNotificationsC = false;
        public Popup_Choices()
        {
            InitializeComponent();

            notificationStack.Visibility = Visibility.Visible;
            instructionStack.Visibility = Visibility.Collapsed;
            notificationStack.Children.Clear();

            if (Global.importFactory.MismatchSongBuffer.Any())
            {
                //LoadNextBufferSong();
                Button a = new Button();
                a.Style = FindResource(Global.ControlStyle_Button) as Style;
                a.Content = string.Format("I need your attention on {0} song(s)", Global.importFactory.MismatchSongBuffer.Count());
                a.Click += LoadMismatchedSongs;

                notificationStack.Children.Add(a);
                hasNotificationsA = true;
            }

            if (Global.importFactory.MismatchAlbumBuffer.Any())
            {
                Button b = new Button();
                b.Style = FindResource(Global.ControlStyle_Button) as Style;
                b.Content = string.Format("There are {0} Albums that I'm not sure of..", Global.importFactory.MismatchAlbumBuffer.Count());
                b.Click += LoadMismatchedAlbums;

                notificationStack.Children.Add(b);
                //LoadNextAlbumBuffer();
                hasNotificationsB = true;
            }

            using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
            {
                var a = from b in db.TrackLibraries
                        where b.Type != "Music" && b.Type != "Karaoke" && b.Type != "Video" && b.Type != "Radio" && b.Type != "AlbumSongMismatch" && b.Type != "SystemVideo"
                        select b.Type;
                TrackTypes = a.Distinct().ToList();
                TrackTypeIndex = 0;
                if (TrackTypes.Any())
                {
                    Button c = new Button();
                    c.Style = FindResource(Global.ControlStyle_Button) as Style;
                    c.Content = string.Format("There are some import errors that need action");
                    c.Click += LoadDatabaseErrors;

                    notificationStack.Children.Add(c);

                    hasNotificationsC = true;
                    //LoadDatabaseTracks();
                }
            }
        }

        private void LoadDatabaseErrors(object sender, RoutedEventArgs e)
        {
            notificationStack.Visibility = Visibility.Collapsed;
            instructionStack.Visibility = Visibility.Visible;
            LoadDatabaseTracks();
        }

        private void LoadMismatchedAlbums(object sender, RoutedEventArgs e)
        {
            notificationStack.Visibility = Visibility.Collapsed;
            instructionStack.Visibility = Visibility.Visible;
            LoadNextAlbumBuffer();
        }

        private void LoadMismatchedSongs(object sender, RoutedEventArgs e)
        {
            notificationStack.Visibility = Visibility.Collapsed;
            instructionStack.Visibility = Visibility.Visible;
            LoadNextBufferSong();
        }

        ImportFactory.FactorySong bufferSong { get; set; }
        int bufferSongIndex = 0;
        private void btnTrackChoice(object sender, RoutedEventArgs e)
        {
            Button b = (Button)(sender);
            int index = int.Parse(b.Tag.ToString());

            iManagedTrack iTrack = new iManagedTrack(Global.importFactory.MismatchSongBuffer[0].ManagedFile);
            iTrack.SaveResultsData(Global.importFactory.MismatchSongBuffer[0].MismatchSongs[index], Global.importFactory.MismatchSongBuffer[0].ManagedFile, true);

            Global.importFactory.MismatchSongBuffer.Remove(bufferSong);

            if (bufferSongIndex <= Global.importFactory.MismatchSongBuffer.Count() - 1)
                LoadNextBufferSong();
            else
            {
                var lbl = lblMain;
                mainStack.Children.Clear();
                lbl.Content = "Done!";
                //mainStack.Children.Add(lbl);
            }
        }

        private void LoadNextBufferSong()
        {
            var lbl = lblMain;
            mainStack.Children.Clear();
            lbl.Content = Global.importFactory.MismatchSongBuffer[bufferSongIndex].ManagedFile.FileName + Global.importFactory.MismatchSongBuffer[bufferSongIndex].ManagedFile.FileExtention;
            //mainStack.Children.Add(lbl);

            bufferSong = Global.importFactory.MismatchSongBuffer[bufferSongIndex];

            int i = 0;
            foreach (var a in Global.importFactory.MismatchSongBuffer[bufferSongIndex].MismatchSongs)
            {
                Button b = new Button();
                b.Content = string.Format("{0} - {1} - {2}:{3}", a.ArtistName, a.CollectionName, a.TrackNumber, a.TrackName);
                b.Click += btnTrackChoice;
                b.Tag = i;

                mainStack.Children.Add(b);

                i++;
            }

            hasNotificationsA = false;
            CheckAlerts();
        }

        private void CheckAlerts()
        {
            if (hasNotificationsA == false && hasNotificationsB == false && hasNotificationsC == false)
                Global.AlertNotifications = false;
        }

        /// Database Entries
        List<string> TrackTypes = new List<string>();
        int TrackTypeIndex = 0;

        private void LoadDatabaseTracks()
        {
            using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
            {
                var a = from b in db.TrackLibraries
                        where b.Type != "Music" && b.Type != "Karaoke" && b.Type != "Video" && b.Type != "Radio" && b.Type != "SystemVideo"
                        select b.Type;
                TrackTypes = a.Distinct().ToList();
                TrackTypeIndex = 0;
                if (TrackTypes.Any())
                    LoadNextDBType(db);
                else
                    hasNotificationsC = false;
            }

            CheckAlerts();
        }

        private void LoadNextDBType()
        {
            using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
            {
                LoadNextDBType(db);
            }
        }
        private void LoadNextDBType(JukeboxBrainsDBEntities db)
        {

            //while(TrackTypes[TrackTypeIndex].StartsWith("AlbumSongMismatch"))
            //{
            //    TrackTypeIndex++;
            //    if (TrackTypeIndex >= TrackTypes.Count())
            //        break;
            //}

            if (TrackTypeIndex < TrackTypes.Count())
            {
                var lbl = lblMain;
                mainStack.Children.Clear();
                lbl.Content = TrackTypes[TrackTypeIndex];
                //mainStack.Children.Add(lbl);
                string t = TrackTypes[TrackTypeIndex];
                var songlist = (from w in db.TrackLibraries
                                where w.Type == t
                                select w).ToList();
                foreach (var a in songlist)
                {
                    Button b = new Button();
                    int songcount = a.SongLibraries == null ? 0 : a.SongLibraries.Count();
                    b.Content = string.Format("{0} - Song Count :{1}", a.FileName, songcount);
                    b.Click += loadSmartUpdater;
                    b.Tag = a.Id;
                    mainStack.Children.Add(b);
                }

                Button c = new Button();
                c.Content = "Load next set";
                c.Click += nextTrackType;
                mainStack.Children.Add(c);
            }
            else
            {
                hasNotificationsC = false;
                CheckAlerts();
            }
        }

        private void loadSmartUpdater(object sender, RoutedEventArgs e)
        {
            Button b = (Button)(sender);
            b.IsEnabled = false;
            int index = int.Parse(b.Tag.ToString());

            Global.mPlayer.popUp_Grid.Visibility = System.Windows.Visibility.Visible;
            Global.mPlayer.popUp_Frame.Content = new Popup_SmartUpdater(index);
        }
        private void nextTrackType(object sender, RoutedEventArgs e)
        {
            if (TrackTypeIndex + 1 < TrackTypes.Count)
            {
                TrackTypeIndex++;
            }
            else
                TrackTypeIndex = 0;

            LoadNextDBType();
        }



        /// Album Mismatch Buffer
        private ImportFactory.ManagedFile bufferAlbumSong;
        private ImportFactory.FactoryMismatchAlbum bufferAlbum;
        private int bufferAlbumIndex = 0;
        private int bufferAlbumSongIndex = 0;

        private void LoadNextAlbumBuffer()
        {
            var lbl = lblMain;
            mainStack.Children.Clear();
            for (int ix = bufferAlbumIndex; ix < Global.importFactory.MismatchAlbumBuffer.Count(); ix++)
            {
                if (!Global.importFactory.MismatchAlbumBuffer[ix].ManagedFiles.Any())
                {
                    bufferAlbumIndex++;
                }
                else
                {
                    break;
                }
            }


            if (bufferAlbumIndex < Global.importFactory.MismatchAlbumBuffer.Count)
            {


                bufferAlbum = Global.importFactory.MismatchAlbumBuffer[bufferAlbumIndex];
                bufferAlbumSong = Global.importFactory.MismatchAlbumBuffer[bufferAlbumIndex].ManagedFiles[bufferAlbumSongIndex];

                lbl.Content = bufferAlbumSong.FileName;
                //mainStack.Children.Add(lbl);

                int i = 0;
                foreach (var a in bufferAlbum.SongOptions)
                {
                    if (!string.IsNullOrEmpty(a.TrackName))
                    {
                        Button b = new Button();
                        b.Content = string.Format("{0} - {1} - {2}:{3}", a.ArtistName, a.CollectionName, a.TrackNumber, a.TrackName);
                        b.Click += btnSelectAlbumTrack;
                        b.Tag = i;

                        mainStack.Children.Add(b);
                    }
                    i++;
                }

                Button c = new Button();
                c.Content = "None";
                c.Click += btnSaveAlbumTrackAnyway;

                mainStack.Children.Add(c);

                if (bufferAlbum.ManagedFiles.Count > 1)
                {
                    Button d = new Button();
                    d.Content = "Next -->";
                    d.Click += btnNextAlbumTrack;

                    mainStack.Children.Add(d);
                }
            }
            else
            {
                var lblx = lblMain;
                mainStack.Children.Clear();
                lblx.Content = "Done!";
            }

            hasNotificationsB = false;
            CheckAlerts();
        }

        private void btnNextAlbumTrack(object sender, RoutedEventArgs e)
        {
            // 1 - 2
            if (bufferAlbumSongIndex + 1 == bufferAlbum.ManagedFiles.Count)
                bufferAlbumSongIndex = 0;
            else
                bufferAlbumSongIndex++;

            LoadNextAlbumBuffer();
        }
        private void btnSaveAlbumTrackAnyway(object sender, RoutedEventArgs e)
        {

            bufferAlbumIndex++;
            bufferAlbumSongIndex = 0;

            if (bufferAlbumSongIndex <= bufferAlbum.ManagedFiles.Count() - 1)
                LoadNextAlbumBuffer();
            else
            {
                var lbl = lblMain;
                mainStack.Children.Clear();
                lbl.Content = "Done!";
                //mainStack.Children.Add(lbl);
            }

            LoadNextAlbumBuffer();
        }
        private void btnSelectAlbumTrack(object sender, RoutedEventArgs e)
        {
            Button b = (Button)(sender);
            int index = int.Parse(b.Tag.ToString());

            iManagedTrack iTrack = new iManagedTrack(bufferAlbumSong);
            iTrack.SaveResultsData(bufferAlbum.SongOptions[index], bufferAlbumSong, true);

            bufferAlbum.ManagedFiles.Remove(bufferAlbumSong);
            bufferAlbum.SongOptions.Remove(bufferAlbum.SongOptions[index]);

            if (bufferAlbumSongIndex <= bufferAlbum.ManagedFiles.Count() - 1)
                LoadNextAlbumBuffer();
            else
            {
                var lbl = lblMain;
                mainStack.Children.Clear();
                lbl.Content = "Done!";
                //mainStack.Children.Add(lbl);
            }
        }

        private void btnDismissNotifications_Click(object sender, RoutedEventArgs e)
        {
            Global.MainWindow.hasUserAlerts = false;
            Global.mPlayer.TogglePopUp();
        }
    }
}
