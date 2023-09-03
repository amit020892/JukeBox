using iTunesSearch.Library;
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

namespace JukeBoxSolutions.Pages
{
    /// <summary>
    /// Interaction logic for ImportAnalytics.xaml
    /// </summary>
    public partial class ImportAnalytics : Page
    {
        public ImportAnalytics()
        {
            InitializeComponent();
        }

        private void btnReload_Click(object sender, RoutedEventArgs e)
        {
            using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
            {
                //db.Database.ExecuteSqlCommand("EXECUTE sp_refreshview @viewname = 'LibraryView'");
                //Global.ViewLoaded = true;

                if (db.Playlists.Where(w => w.PlaylistId == Global.AutoUpdateTodoListPlaylistId).Any())
                    btnResumeItunesUpdate.IsEnabled = true;
                else
                    btnResumeItunesUpdate.IsEnabled = false;
            }

            stackSummary.Children.Clear();
            Label lbl = new Label() { Content = "Total Imported Tracks: " + Global.ImportAnalytics.DBAnalytics.Count(), Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255)), Padding = new Thickness(5) };
            stackSummary.Children.Add(lbl);

            Label lbl2 = new Label() { Content = "Total Database Tracks: " + Global.ImportAnalytics.DBAnalytics.Count(c => !c.TrackId.HasValue), Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255)), Padding = new Thickness(5) };
            stackSummary.Children.Add(lbl2);


            if (Global.ImportAnalytics.DBAnalytics != null)
            {
                var BreadCrumbList = Global.ImportAnalytics.DBAnalytics.SelectMany(s => s.Breadcrumbs).Distinct().ToList();
                Label lbl3 = new Label() { Content = "Import Analytics", Padding = new Thickness(5) };
                stackSummary.Children.Add(lbl3);

                foreach (string s in BreadCrumbList)
                {
                    Button btn3 = new Button() { Content = s + ": " + Global.ImportAnalytics.DBAnalytics.Where(c => c.Breadcrumbs.Contains(s)).Count() };
                    btn3.Tag = s;
                    btn3.Click += BtnLoadAnalyticBreadcrumb_Click;
                    //Label lbl3 = new Label() { Content = s + ": " + Global.ImportAnalytics.DBAnalytics.Where(c => c.BreadCrumbString == s).Count(), Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255)), Padding = new Thickness(5) };
                    stackSummary.Children.Add(btn3);
                }
            }
            // is imported

            //foreach (var a in Global.ImportAnalytics.DBAnalytics)
            //{

            //}
        }



        private void btnAlbumBuffer_Click(object sender, RoutedEventArgs e)
        {
            stackSummary.Children.Clear();
            using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
            {
                var songlist = Global.importFactory.sessionAlbums.Select(s => new { s.SetId, s.DBSongId, s.album.CollectionId });
                var a = from song in songlist
                        join b in db.SongLibraries on song.DBSongId equals b.SongId
                        join c in db.AlbumLibraries on song.CollectionId.ToString() equals c.iTunesId
                        select new { song.SetId, song.DBSongId, b.SongName, song.CollectionId, c.isHidden, c.AlbumName };

                stackResults.Children.Clear();
                foreach (var d in a)
                {
                    string s = string.Format("Set:{0}, ID:{1}, {2}, {3}, {4}, {5}", d.SetId, d.DBSongId, d.SongName, d.CollectionId, d.isHidden, d.AlbumName);
                    stackResults.Children.Add(new Label() { Content = s, Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255)), Padding = new Thickness(5) });
                }
            }
        }

        public class SetItem
        {
            public int SetID { get; set; }
            public AlbumLibrary Album { get; set; }
            public LibraryView Song { get; set; }
        }
        private List<SetItem> SetItems = new List<SetItem>();

        #region AlbumKey

        private void btnAlbumTree_Click(object sender, RoutedEventArgs e)
        {
            SetItems = new List<SetItem>();
            using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
            {
                // tracklist
                //var list = db.LibraryViews.Where(w => w.Type == "Video").ToList();
                var list2 = db.SongLibraries.ToList();

                foreach (var li in list2)
                {
                    // get all albums

                    // get all siblings

                    var result = SetItems.Where(a => li.AlbumLibraries.Select(s => s.AlbumId).Contains(a.Album.AlbumId)).Select(s1 => s1.SetID).Distinct();
                    List<int> setID = new List<int>();
                    if (SetItems.Any())
                        setID.Add(SetItems.Max(m => m.SetID) + 1);
                    else
                        setID.Add(1);

                    if (result.Any())
                    {
                        setID = result.ToList();
                    }


                    foreach (var albums in li.AlbumLibraries)
                    {
                        foreach (int i in setID)
                        {
                            SetItems.Add(new SetItem() { Album = albums, SetID = i, Song = db.LibraryViews.First(x => x.SongId == li.SongId) });
                        }
                    }
                }

                stackSummary.Children.Clear();
                foreach (var f in SetItems.Select(sx => sx.SetID).Distinct().OrderBy(o => o))
                {
                    Button b = new Button() { Content = "Set " + f, Tag = f };
                    b.Click += BtnChooseSet_Click;
                    stackSummary.Children.Add(b);
                }
            }

        }

        int SelectedSet = 0;
        private void BtnChooseSet_Click(object sender, RoutedEventArgs e)
        {
            SelectedSet = int.Parse(((Button)sender).Tag.ToString());
            var songlist = SetItems.Where(w1 => w1.SetID == SelectedSet).Select(s1 => s1.Song).Distinct().ToList();

            stackResults.Children.Clear();

            foreach (var f in songlist)
            {
                string s = "";
                foreach (var z in SetItems.Where(w => w.Song.SongId == f.SongId))
                {
                    if (z.Album.isHidden == false)
                        s += ", [*" + z.Album.AlbumName + "*]";
                    else
                        s += ", " + z.Album.AlbumName;
                }
                s = s.Substring(2);

                Label l = new Label() { Content = String.Format("{0} - {1}", f.SongName, s) };
                stackResults.Children.Add(l);
            }
        }


        #endregion AlbumKey

        #region ImportBreadCrumbs

        string ButtonMode = "";
        private void BtnLoadAnalyticBreadcrumb_Click(object sender, RoutedEventArgs e)
        {
            string crumb = ((Button)sender).Tag.ToString();
            LoadAnalyticStack(crumb);
        }
        private void LoadAnalyticStack(string BreadCrumb)
        {
            ButtonMode = BreadCrumb;
            stackResults.Children.Clear();

            // get items
            BufferAnalyticsStack = Global.ImportAnalytics.DBAnalytics.Where(w => w.Breadcrumbs.Contains(BreadCrumb)).Select((b, index) => new AnalyticStackItem { ID = index + 1, ContentKey = b.FilePath, Stack = b.Breadcrumbs }).ToList();
            foreach (var c in BufferAnalyticsStack)
            {
                stackResults.Children.Add(LoadMainBatchItem(c.ContentKey, false, c.ID.ToString()));
            }
        }

        List<AnalyticStackItem> BufferAnalyticsStack = new List<AnalyticStackItem>();
        public class AnalyticStackItem
        {
            public int ID { get; set; }
            public string ContentKey { get; set; }
            public List<string> Stack { get; set; }
        }

        private Label LoadMainBatchItem(string Content, bool isSelected, string IndexKey)
        {
            Label l = new Label();
            l.Content = Content;
            if (isSelected)
            {
                l.FontStyle = FontStyles.Italic;
                l.FontWeight = FontWeights.Bold;
                l.MouseLeftButtonDown += UnloadClickMainItem_MouseLeftButtonDown;
            }
            else
            {
                l.MouseLeftButtonDown += ClickMainItem_MouseLeftButtonDown;
            }
            l.Tag = IndexKey;
            l.MouseLeftButtonDown += ClickMainItem_MouseLeftButtonDown;

            return l;
        }

        private void UnloadClickMainItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            LoadAnalyticStack(ButtonMode);
            justclicked = true;
        }
        bool justclicked = false;
        private void ClickMainItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (justclicked == false)
            {
                int tag = int.Parse(((Label)sender).Tag.ToString());
                stackResults.Children.Clear();
                // get item
                //var a = Global.ImportAnalytics.DBAnalytics.First(f => f.FilePath == tag);
                var a = BufferAnalyticsStack.First(f => f.ID == tag);
                stackResults.Children.Add(LoadMainBatchItem(a.ContentKey, true, tag.ToString()));


                switch (ButtonMode)
                {
                    case "": break;
                    case "Loaded Albums to Session Buffer Albums":
                        using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                        {
                            // get albums for song
                            string fPath = BufferAnalyticsStack.Where(w => w.ID == tag).First().ContentKey;
                            SongLibrary selectedSong = db.TrackLibraries.First(t => t.FilePath == fPath).SongLibraries.First();


                            // Load selected album
                            //.AlbumLibraries.First(a => a.isHidden == false);
                            AlbumLibrary selectedAlbum = selectedSong.AlbumLibraries.First(ah => ah.isHidden == false);

                            bool thisisWrong = true;
                            if (selectedSong.AlbumLibraries.Count(c1 => c1.isHidden == false) > 1)
                                thisisWrong = false;

                            stackResults.Children.Add(LoadChildrenBatchItems("*" + selectedAlbum.AlbumName + " - " + selectedAlbum.SongLibraries.Count() + "*", true, selectedAlbum.AlbumId.ToString()));

                            // Load remaining albums
                            foreach (AlbumLibrary al in selectedSong.AlbumLibraries.Where(w1 => w1.isHidden == true))
                            {
                                stackResults.Children.Add(LoadChildrenBatchItems(al.AlbumName + " - " + al.SongLibraries.Count(), false, al.AlbumId.ToString()));
                            }
                        }
                        break;
                    case "iTunes:Could not find Artist":
                        foreach (var c in Global.ImportAnalytics.DBAnalytics.Where(w => w.FilePath == BufferAnalyticsStack.Find(id => id.ID == tag).ContentKey).First().SupportMetaData)
                        {
                            foreach (var d in c.Values)
                            {
                                stackResults.Children.Add(LoadChildrenBatchItems(c.Key + " - " + d, false, ""));
                            }
                        }
                        break;
                    case "iTunes:Found Artist, could not find Track":
                        foreach (var c in Global.ImportAnalytics.DBAnalytics.Where(w2 => w2.FilePath == BufferAnalyticsStack.Find(id2 => id2.ID == tag).ContentKey).First().SupportMetaData)
                        {
                            foreach (var d in c.Values)
                            {
                                stackResults.Children.Add(LoadChildrenBatchItems(c.Key + " - " + d, false, ""));
                            }
                        }
                        break;
                }

                foreach (var b in a.Stack)
                {

                }
            }
            else
                justclicked = false;
        }

        private Label LoadChildrenBatchItems(string Content, bool isSelected, string IndexKey)
        {
            Label l = new Label();
            l.Content = Content;
            l.Padding = new Thickness(25, 5, 5, 5);

            if (isSelected)
            {
                l.FontStyle = FontStyles.Italic;
                l.FontWeight = FontWeights.Bold;
            }
            l.Tag = IndexKey;
            l.MouseLeftButtonDown += ClickChildItem_MouseLeftButtonDown;

            return l;
        }

        private void ClickChildItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

            if (justclicked == false)
            {
                int tag = int.Parse(((Label)sender).Tag.ToString());
                stackResults.Children.Clear();
                // get item
                //var a = Global.ImportAnalytics.DBAnalytics.First(f => f.FilePath == tag);
                var a = BufferAnalyticsStack.First(f => f.ID == tag);
                stackResults.Children.Add(LoadMainBatchItem(a.ContentKey, true, tag.ToString()));

                foreach (var b in a.Stack)
                {
                    stackResults.Children.Add(LoadMainBatchItem(b, false, ""));
                }
            }
            else
                justclicked = false;
        }

        #endregion

        private void btnResumeItunesUpdate_Click(object sender, RoutedEventArgs e)
        {
            btnResumeItunesUpdate.IsEnabled = false;

            List<ImportFactory.ManagedFile> list = new List<ImportFactory.ManagedFile>();
            List<TrackLibrary> templist = new List<TrackLibrary>();

            using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
            {
                if (db.Playlists.Where(w => w.PlaylistId == Global.AutoUpdateTodoListPlaylistId).Any())
                {
                    // kick off import all
                    // create managed files

                    bool isBusyLoading = true;
                    // if in db, then add to playlist
                    templist = (from l in db.Playlists.Where(w => w.PlaylistId == Global.AutoUpdateTodoListPlaylistId)
                                select l.TrackLibrary).ToList();
                }

                foreach (var l in templist)
                {
                    list.Add(new ImportFactory.ManagedFile(l));
                }
            }

            if (list.Any())
            {
                // clear db
                Global.mPlayer.dbRemoveTrackFromPlaylist(templist.ToList(), Global.AutoUpdateTodoListPlaylistId);
                iTunesSearchManager iManager = new iTunesSearchManager();
                iManager.AutoUpdateTreadedTracksV2(list.ToList());
            }
        }

        private void btnITunesStatus_Click(object sender, RoutedEventArgs e)
        {
            if (Global.isOnline)
                btnITunesStatus.Content = "Online";
            else
                btnITunesStatus.Content = "Offline";
        }

        private void LoadDBAnalytics()
        {
            stackSummary.Children.Clear();
            Button b1 = new Button() { Content = "Check LibraryView", Tag = "LibraryView" };
            b1.Click += btnAnalyzeDatabase;
            stackSummary.Children.Add(b1);

            Button b2 = new Button() { Content = "Check Albums", Tag = "DbAlbums" };
            b2.Click += btnAnalyzeDatabase;
            stackSummary.Children.Add(b2);

            Button b3 = new Button() { Content = "Check Artists", Tag = "DbArtists" };
            b3.Click += btnAnalyzeDatabase;
            stackSummary.Children.Add(b3);

            Button b4 = new Button() { Content = "Check Tracks", Tag = "DbTracks" };
            b4.Click += btnAnalyzeDatabase;
            stackSummary.Children.Add(b4);
        }

        private void btnAnalyzeDatabase(object sender, RoutedEventArgs e)
        {
            string s = ((Button)sender).Tag.ToString();
            stackResults.Children.Clear();
            switch (s)
            {
                case "LibraryView":
                    using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                    {
                        int i1 = db.LibraryViews.Count(c1 => c1.AlbumId == null);
                        if (i1 > 0)
                            stackResults.Children.Add(LoadAlanyticLabels("NULL ALbum ID Count : " + i1, true, ""));
                        else
                            stackResults.Children.Add(LoadAlanyticLabels("NULL ALbum ID Count : " + i1, false, ""));
                        stackResults.Children.Add(LoadAlanyticLabels("Normal ALbum ID Count : " + db.LibraryViews.Count(c12 => c12.AlbumId != null), false, ""));

                        int i2 = db.LibraryViews.Count(c2 => c2.SongId == null);
                        if (i2 > 0)
                            stackResults.Children.Add(LoadAlanyticLabels("NULL Song ID Count : " + i2, true, ""));
                        else
                            stackResults.Children.Add(LoadAlanyticLabels("NULL Song ID Count : " + i2, false, ""));
                        stackResults.Children.Add(LoadAlanyticLabels("Normal Song ID Count : " + db.LibraryViews.Count(c22 => c22.SongId != null), false, ""));

                        int i3 = db.LibraryViews.Count(c3 => c3.Artists == null);
                        if (i3 > 0)
                            stackResults.Children.Add(LoadAlanyticLabels("NULL Artists Count : " + i3, true, ""));
                        else
                            stackResults.Children.Add(LoadAlanyticLabels("NULL Artists Count : " + i3, false, ""));
                        stackResults.Children.Add(LoadAlanyticLabels("Normal Artists Count : " + db.LibraryViews.Count(c12 => c12.Artists != null), false, ""));


                        foreach (string t in db.LibraryViews.Select(c4 => c4.Type).Distinct())
                        {
                            int i4 = db.LibraryViews.Count(c4 => c4.Type == t);
                            stackResults.Children.Add(LoadAlanyticLabels("Type " + t + " Count : " + i4, true, ""));
                        }
                    }
                    break;
                case "DbAlbums":
                    using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                    {
                        // Duplicates

                        // Album Count
                        var query = db.AlbumLibraries.GroupBy(x => x.AlbumName).Where(g => g.Count() > 1).Select(y => new { Element = y.Key, Counter = y.Count() }).ToList();

                        bufferDuplicateAlbums = query.Select(sel => sel.Element).ToList();
                        var a = LoadAlanyticLabels("Duplicate Album Count : " + query.Count(), false, "");
                        a.MouseLeftButtonDown += LoadAnalyticsExpand;
                        stackResults.Children.Add(a);
                        stackResults.Children.Add(LoadAlanyticLabels("Total Album Count : " + db.AlbumLibraries.Count(), false, ""));
                    }
                    break;
                case "DbArtists":
                    using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                    {
                        var queryArtist = db.Artists.GroupBy(x2 => x2.ArtistName).Where(g => g.Count() > 1).Select(y => new { Element = y.Key, Counter = y.Count(), Entries = y }).ToList();

                        stackResults.Children.Add(LoadAlanyticLabels("Duplicate Artist Count : " + queryArtist.Count(), false, ""));
                        stackResults.Children.Add(LoadAlanyticLabels("Total Artist Count : " + db.Artists.Count(), false, ""));

                        foreach (var a in queryArtist)
                        {
                            stackResults.Children.Add(LoadAlanyticLabels("Duplicate Artist : " + a.Counter + " - " + a.Element, false, ""));
                        }
                    }
                    break;
                case "DbTracks":
                    using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                    {
                        var queryArtist = db.TrackLibraries.GroupBy(x3 => x3.FilePath).Where(g => g.Count() > 1).Select(y => new { Element = y.Key, Counter = y.Count(), Entries = y }).ToList();

                        stackResults.Children.Add(LoadAlanyticLabels("Duplicate Physical Tracks Count : " + queryArtist.Count(), false, ""));
                        stackResults.Children.Add(LoadAlanyticLabels("Total Physical Tracks Count : " + db.TrackLibraries.Count(), false, ""));
                        stackResults.Children.Add(LoadAlanyticLabels("Total Tracks in Limbo : " + db.TrackLibraries.Where(w => w.Type.Contains("Import")).Count(), false, ""));

                        foreach (var a in queryArtist)
                        {
                            stackResults.Children.Add(LoadAlanyticLabels("Duplicate track : " + a.Counter + " - " + a.Element, false, ""));
                        }
                    }
                    break;
            }
        }

        List<string> bufferDuplicateAlbums = new List<string>();
        private void LoadAnalyticsExpand(object sender, MouseButtonEventArgs e)
        {
            stackResults.Children.Clear();
            int resultCounter = 0;

            using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
            {
                foreach (string b in bufferDuplicateAlbums)
                {
                    var a = db.AlbumLibraries.Where(w => w.AlbumName == b);
                    stackResults.Children.Add(LoadAlanyticLabels(b, true, ""));
                    resultCounter++;
                    foreach (AlbumLibrary c in a)
                    {
                        stackResults.Children.Add(LoadAlanyticLabels(string.Format("ID: {0} - SongCount: {1}, {2}", c.AlbumId, c.SongLibraries.Count(), c.SongLibraries.Count() == 0 ? "x" : String.Join(",", c.SongLibraries.Select(sx => sx.SongName))), false, ""));
                        resultCounter++;
                    }
                }
            }

            //if(resultCounter>20)

        }

        List<Label> bufferResultList = new List<Label>();


        private Label LoadAlanyticLabels(string Content, bool isSelected, string IndexKey)
        {
            Label l = new Label();
            l.Content = Content;
            l.Padding = new Thickness(25, 5, 5, 5);

            if (isSelected)
            {
                l.FontStyle = FontStyles.Italic;
                l.FontWeight = FontWeights.Bold;
            }
            l.Tag = IndexKey;
            return l;
        }

        private void btnLoadAnalytics_Click(object sender, RoutedEventArgs e)
        {
            LoadDBAnalytics();
        }
    }
}
