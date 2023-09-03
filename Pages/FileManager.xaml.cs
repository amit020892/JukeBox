using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
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
//using System.Windows.Shapes;
using Id3;
using Hqub.MusicBrainz.API;
using Hqub.MusicBrainz.API.Entities;
using Hqub.MusicBrainz.Client;
using System.Net;
using System.Threading;

namespace JukeBoxSolutions.Pages
{
    /// <summary>
    /// Interaction logic for FileManager.xaml
    /// </summary>
    public partial class FileManager : Page
    {
        public class ImportManager
        {
            public List<ManagedFile> ImportList { get; set; }

            public List<ManagedFile> UnmatchedFiles { get; set; }
            public List<ManagedFile> MatchedFiles { get; set; }

            public ImportManager()
            {
                ImportList = new List<ManagedFile>();
                UnmatchedFiles = new List<ManagedFile>();
                MatchedFiles = new List<ManagedFile>();
            }

            public void LoadDirlisting(string TargetDir, bool includeFolders = false)
            {
                SearchOption s = SearchOption.TopDirectoryOnly;
                //JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities();
                using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                {

                    if (includeFolders)
                        s = SearchOption.AllDirectories;

                    foreach (string musicFilePath in Directory.EnumerateFiles(TargetDir, "*", s))
                    {
                        ManagedFile m = new ManagedFile();
                        if (db.TrackLibraries.Any(a => a.FilePath == musicFilePath))
                            m.isDatabase = true;

                        if (Global.karaokeformats.Contains(System.IO.Path.GetExtension(musicFilePath)))
                        {
                            m.isKaraoke = true;
                            CheckKaraokePair(musicFilePath, m);
                        }
                        else if (Global.musicformats.Contains(System.IO.Path.GetExtension(musicFilePath)))
                        {
                            if (hasID3v2(musicFilePath))
                                m.isId3 = true;

                            // all mp3s go into unmatched pile
                            m.isMusic = true;
                            CheckKaraokePair(musicFilePath, m);
                        }
                        else if (Global.videoformats.Contains(System.IO.Path.GetExtension(musicFilePath)))
                        {
                            m.isVideo = true;
                            m.setFileName(musicFilePath);
                        }
                    }

                    // adds a single entry for the pair of karaoke files
                    ImportList.AddRange(MatchedFiles);

                    // no karaoke files, dump remainder mp3s in import list
                    // dumps incomplete karaoke files
                    ImportList.AddRange(UnmatchedFiles.Where(w => w.isMusic == true));
                }
            }

            public void CheckKaraokePair(string dir, ManagedFile m)
            {
                ManagedFile k2 = new ManagedFile();
                bool isFound = false;

                // see if other half is recorded
                foreach (ManagedFile k in UnmatchedFiles)
                {
                    if (k.checkFileName(dir))
                    {
                        // found
                        isFound = true;
                        k2 = k;
                        break;
                    }
                }

                if (isFound)
                {
                    UnmatchedFiles.Remove(k2);
                    k2.AddKaraokeValue(dir);
                    k2.CloneValues(m);
                    MatchedFiles.Add(k2);
                }
                else
                {
                    k2.setKaraokeDir(dir);
                    k2.CloneValues(m);
                    UnmatchedFiles.Add(k2);
                }
            }

            private bool hasID3v2(string filepath)
            {
                using (var mp3 = new Mp3(filepath))
                {
                    foreach (Id3Version iv in mp3.AvailableTagVersions)
                    {
                        if (iv == Id3Version.V23)
                        {
                            Id3Tag tag = mp3.GetTag(Id3TagFamily.Version2X);
                            return true;
                        }
                    }
                }
                return false;
            }


            public List<ListBoxItem> getListBox_All()
            {
                return (from i in ImportList
                        select new ListBoxItem() { Content = i.FullFileName, ToolTip = i.FilePath, Tag = i.FilePath }).ToList();
            }

            public List<ListBoxItem> getListBox_NoData()
            {
                return (from i in ImportList
                        where i.isId3 == false && i.isDatabase == false
                        select new ListBoxItem() { Content = i.FullFileName, ToolTip = i.FilePath, Tag = i.FilePath }).ToList();

            }

            public List<ListBoxItem> getListBox_Id3()
            {
                return (from i in ImportList
                        where i.isId3 == true
                        select new ListBoxItem() { Content = i.FullFileName, ToolTip = i.FilePath, Tag = i.FilePath }).ToList();
            }

            #region classes

            public class ManagedFile
            {
                public string FileName { get; set; }
                public string FileExtention { get; set; }
                public string FullFileName { get { return FileName + FileExtention; } }

                public string FilePath { get; set; }
                public string SongMp3Path { get; set; }

                public bool isId3 = false;
                public bool isDatabase = false;
                public bool isKaraoke = false;
                public bool isMusic = false;
                public bool isVideo = false;

                public Id3Tag Id3Tag { get; set; }

                public void setKaraokeDir(string dir)
                {
                    if (Global.musicformats.Contains(System.IO.Path.GetExtension(dir)))
                    {
                        SongMp3Path = dir;
                    }
                    setFileName(dir);
                }

                public ManagedFile()
                {

                }

                public ManagedFile(string dir)
                {
                    setFileName(dir);
                }

                public void setFileName(string dir)
                {
                    FileName = System.IO.Path.GetFileNameWithoutExtension(dir);
                    FileExtention = System.IO.Path.GetExtension(dir);
                    FilePath = dir;
                }

                public void AddKaraokeValue(string dir)
                {
                    if (Global.karaokeformats.Contains(System.IO.Path.GetExtension(dir)))
                    {
                        FilePath = dir;
                        FileExtention = System.IO.Path.GetExtension(dir);
                    }
                    if (Global.musicformats.Contains(System.IO.Path.GetExtension(dir)))
                        SongMp3Path = dir;
                }

                public bool checkFileName(string dir)
                {
                    return FileName == System.IO.Path.GetFileNameWithoutExtension(dir);
                }

                internal void CloneValues(ManagedFile m)
                {
                    if (m.isId3)
                        isId3 = m.isId3;
                    if (m.isDatabase)
                        isDatabase = m.isDatabase;
                    if (m.isMusic && !isKaraoke)
                        isMusic = m.isMusic;
                    if (m.isKaraoke)
                    {
                        isKaraoke = m.isKaraoke;
                        isMusic = false;
                    }
                }

                internal void setId3Tag(Id3Tag i3)
                {
                    Id3Tag = i3;
                }
            }

            public class WorkbenchFile
            {
                public bool keepseaching = true;
                public string testname { get { return testnames[splitindex]; } }
                public List<string> testnames = new List<string>();
                public int splitindex = 0;

                internal List<MusicBrainzHelper> ResultsSearchBuffer = new List<MusicBrainzHelper>();


                public WorkbenchFile(ManagedFile m)
                {
                    // extract possible names
                    testnames = m.FileName.Split('-').ToList();
                }

                public void NextName()
                {
                    // maxed out
                    if (testnames.Count == splitindex + 1)
                        keepseaching = false;
                    else
                    {
                        splitindex++;
                    }
                }

                internal bool isTrackFamily(string fileName, string trackname)
                {
                    // using seach format, check if file names match
                    string n1 = fileName.Split('-')[splitindex].Trim().ToLower();
                    string n2 = trackname.ToLower();
                    return n1 == n2;
                }

                internal void AttachSearchResults(List<MusicBrainzHelper> results)
                {
                    ResultsSearchBuffer.AddRange(results);
                }

                public Id3Tag FoundTrack { get; set; }
                internal bool SearchShelf(List<Id3Tag> albumsShelf)
                {
                    string artistname = "";

                    for (int i = 0; i < testnames.Count(); i++)
                    {
                        string tinyname = testnames[i].ToString().ToLower().Trim();
                        foreach (var a in albumsShelf)
                        {
                            // possible artist name?
                            if (a.Artists.Value.First().ToLower() == tinyname)
                            {
                                artistname = a.Artists.Value.First();
                                break;
                            }
                            if (a.Title.ToString().ToLower() == tinyname)
                            {
                                // found name
                                keepseaching = false;
                                splitindex = i;
                                FoundTrack = a;
                                return true;
                            }
                        }
                    }

                    return false;
                }
            }

            public List<ManagedFile> Outbox = new List<ManagedFile>();

            internal void PullMusicBrainzData()
            {
                // Workbench
                int ImportListMarker = 0;
                List<WorkbenchFile> ComplexShelf = new List<WorkbenchFile>();
                List<ManagedFile> Inbox = ImportList;
                List<ManagedFile> Failbox = new List<ManagedFile>();
                List<ManagedFile> Outbox = new List<ManagedFile>();
                List<Id3Tag> AlbumsTrackShelf = new List<Id3Tag>();


                // set up MusicBrainz
                var location = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                var client = new MusicBrainzClient()
                {
                    Cache = new FileRequestCache(Path.Combine(location, "cache"))
                };


                // pull working file data
                // get possible search names
                //foreach (ManagedFile inbox in Inbox)

                while (Inbox.Count > 0)
                {
                    ManagedFile inbox = Inbox.First();

                    WorkbenchFile WorkingFile = new WorkbenchFile(inbox);
                    if (WorkingFile.SearchShelf(AlbumsTrackShelf))
                    {
                        //found result m
                        ManagedFile m2 = inbox;
                        m2.setId3Tag(WorkingFile.FoundTrack);
                        Outbox.Add(m2);
                        Inbox.Remove(inbox);
                        // remove track from workbench shelf
                        AlbumsTrackShelf.Remove(WorkingFile.FoundTrack);
                    }


                    while (WorkingFile.keepseaching)
                    {
                        // search existing albums
                        try
                        {
                            var task = RunTrackSearch(client, WorkingFile.testname.Trim());
                            int timeout = 1000;
                            task.Wait(timeout);
                        }
                        catch
                        {
                            isSimple = false;
                            isComplex = false;
                            TrackSearchResults = new List<MusicBrainzHelper>();
                        }

                        // attach search results to track

                        if (isComplex)
                        {
                            // saves the search name, and results for later
                            WorkingFile.AttachSearchResults(TrackSearchResults);
                            ComplexShelf.Add(WorkingFile);
                            isSimple = false;
                            isComplex = false;
                            TrackSearchResults = new List<MusicBrainzHelper>();
                        }
                        if (isSimple)
                        {
                            // TODO: No album?
                            // Cycle through Album to find other Tracks
                            int i = 0;
                            List<Id3Tag> foundtracks = new List<Id3Tag>();
                            foreach (Id3Tag i3 in TrackSearchResults.First().AlbumTracks)
                            {
                                // need search params
                                foreach (ManagedFile m in Inbox)
                                {
                                    // TODO Add root album track back.. the clones kept being related
                                    if (WorkingFile.isTrackFamily(m.FileName, i3.Title))
                                    {
                                        ManagedFile m2 = m;
                                        m2.setId3Tag(i3);
                                        Outbox.Add(m2);
                                        Inbox.Remove(m);
                                        // remove track from workbench shelf
                                        foundtracks.Add(i3);
                                        break;
                                    }
                                }

                                i++;

                                // This track???
                            }
                            // remove saved tracks from workbench shelf
                            foreach (var f in foundtracks)
                            {
                                TrackSearchResults.First().AlbumTracks.Remove(f);
                            }

                            // Save Album
                            // TrackSearchResults.CopyTo(AlbumsShelf);
                            AlbumsTrackShelf.AddRange(TrackSearchResults.First().AlbumTracks);
                            isSimple = false;
                            isComplex = false;
                            TrackSearchResults = new List<MusicBrainzHelper>();
                            break;
                        }
                        WorkingFile.NextName();
                        //reset results

                    }

                    // Movingon
                    Failbox.Add(inbox);
                    Inbox.Remove(inbox);
                }

            }

            internal void PullSingleBrainzData(ManagedFile selectedTrack)
            {
                // Workbench
                List<WorkbenchFile> ComplexShelf = new List<WorkbenchFile>();
                List<ManagedFile> Failbox = new List<ManagedFile>();
                List<Id3Tag> AlbumsTrackShelf = new List<Id3Tag>();


                // set up MusicBrainz
                var location = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                var client = new MusicBrainzClient()
                {
                    Cache = new FileRequestCache(Path.Combine(location, "cache"))
                };


                // pull working file data
                // get possible search names
                //foreach (ManagedFile inbox in Inbox)

                ManagedFile inbox = selectedTrack;
                WorkbenchFile WorkingFile = new WorkbenchFile(inbox);

                while (WorkingFile.keepseaching)
                {
                    // search existing albums
                    try
                    {
                        var task = RunTrackSearch(client, WorkingFile.testname.Trim());
                        int timeout = 1000;
                        task.Wait(timeout);
                    }
                    catch
                    {
                        isSimple = false;
                        isComplex = false;
                        TrackSearchResults = new List<MusicBrainzHelper>();
                    }

                    // attach search results to track

                    if (isComplex)
                    {
                        // saves the search name, and results for later
                        WorkingFile.AttachSearchResults(TrackSearchResults);
                        ComplexShelf.Add(WorkingFile);
                        isSimple = false;
                        isComplex = false;
                        TrackSearchResults = new List<MusicBrainzHelper>();
                    }
                    if (isSimple)
                    {
                        // TODO: No album?
                        // Cycle through Album to find other Tracks
                        int i = 0;
                        List<Id3Tag> foundtracks = new List<Id3Tag>();
                        foreach (Id3Tag i3 in TrackSearchResults.First().AlbumTracks)
                        {
                            if (WorkingFile.isTrackFamily(inbox.FileName, i3.Title))
                            {
                                inbox.setId3Tag(i3);
                                Outbox.Add(inbox);
                                // remove track from workbench shelf
                                foundtracks.Add(i3);
                                break;
                            }

                            // This track???
                        }
                        // remove saved tracks from workbench shelf
                        foreach (var f in foundtracks)
                        {
                            TrackSearchResults.First().AlbumTracks.Remove(f);
                        }

                        // Save Album
                        // TrackSearchResults.CopyTo(AlbumsShelf);
                        AlbumsTrackShelf.AddRange(TrackSearchResults.First().AlbumTracks);
                        isSimple = false;
                        isComplex = false;
                        TrackSearchResults = new List<MusicBrainzHelper>();
                        break;
                    }
                    WorkingFile.NextName();
                    //reset results

                }
            }


            #endregion classes

            #region MusicBrainz

            internal bool isSimple = false;
            internal bool isComplex = false;
            internal List<MusicBrainzHelper> TrackSearchResults = new List<MusicBrainzHelper>();

            public class AlbumHelperX
            {
                public Id3Tag RootTag = new Id3Tag();
                public List<Id3Tag> Tracks { get; set; }

                public void SetTracksId3Tags(List<Track> tracks)
                {
                    int i = 1;
                    foreach (Track r in tracks)
                    {
                        Id3Tag tag = RootTag;
                        tag.Title = r.Recording.Title;
                        tag.Track = new Id3.Frames.TrackFrame(r.Position, tracks.Count);
                        int i2 = r.Length.GetValueOrDefault(0);
                        //tag.Length = new Id3.Frames.LengthFrame(r.Length);
                        Tracks.Add(tag);
                        i++;
                        //tag.Length = new Id3.Frames.LengthFrame().Value.Add(new TimeSpan())
                        //Album = "Waking up the neighbors",
                        //Track = new Id3.Frames.TrackFrame(9, 15),
                        //Year = 1991,
                        //Genre = a1.Genres[0].Name
                        //Publisher = "A&M",
                        //RecordingDate = new DateTime(1991, 03, 01)
                        //  i3A.Track = new Id3.Frames.TrackFrame(r.Media[0].Position, r.Media[0].TrackCount);
                    }
                }
            }

            public class MusicBrainzHelper
            {
                public bool isSimple = false;
                public bool isComplex = false;

                public string SearchString { get; set; }

                public Id3Tag RootTag = new Id3Tag();
                public List<Id3Tag> AlbumTracks { get; set; }

                public List<MusicBrainzHelper> PossibleAlbums { get; set; }

                public MusicBrainzHelper()
                {

                }

                public MusicBrainzHelper(Id3Tag i3)
                {
                    RootTag = i3;
                }

                public Id3Tag CloneRootTag(Id3Tag NewTagValues)
                {
                    Id3Tag newTag = NewTagValues;

                    newTag.Artists = RootTag.Artists;
                    newTag.Album = RootTag.Album;
                    newTag.Year = RootTag.Year;
                    newTag.RecordingDate = RootTag.RecordingDate;
                    newTag.Pictures.Add(RootTag.Pictures.First());

                    return newTag;
                }

                public void SetTracksId3Tags(List<Track> tracks)
                {
                    AlbumTracks = new List<Id3Tag>();
                    int i = 1;
                    foreach (Track r in tracks)
                    {
                        Id3Tag tag = new Id3Tag();

                        tag.Title = r.Recording.Title;
                        tag.Track = new Id3.Frames.TrackFrame(r.Position, tracks.Count);
                        AlbumTracks.Add(CloneRootTag(tag));
                        i++;
                        //tag.Length = new Id3.Frames.LengthFrame().Value.Add(new TimeSpan())
                        //Album = "Waking up the neighbors",
                        //Track = new Id3.Frames.TrackFrame(9, 15),
                        //Year = 1991,
                        //Genre = a1.Genres[0].Name
                        //Publisher = "A&M",
                        //RecordingDate = new DateTime(1991, 03, 01)
                        //  i3A.Track = new Id3.Frames.TrackFrame(r.Media[0].Position, r.Media[0].TrackCount);
                    }
                }
            }

            static readonly CancellationTokenSource s_cts = new CancellationTokenSource();

            public async Task RunTrackSearch(MusicBrainzClient client, string searchname)
            {
                var recordings = await client.Recordings.SearchAsync(searchname.Quote(), 20);

                string artist = "";
                string AlbumName = "";
                string AlbumID = "";


                if (recordings.Count > 0)
                {
                    // Found Track(s)
                    foreach (var a1 in recordings)
                    {
                        MusicBrainzHelper mb = new MusicBrainzHelper();
                        mb.SearchString = searchname;

                        Id3Tag i3 = new Id3Tag
                        {
                            Title = a1.Title
                            //Album = "Waking up the neighbors",
                            //Track = new Id3.Frames.TrackFrame(9, 15),
                            //Year = 1991,
                            //Genre = a1.Genres[0].Name
                            //Publisher = "A&M",
                            //RecordingDate = new DateTime(1991, 03, 01)
                        };

                        // Found Artists
                        foreach (NameCredit nc in a1.Credits)
                        {
                            artist = nc.Artist.Name;
                            i3.Artists.Value.Add(nc.Name);
                        }

                        List<MusicBrainzHelper> tempAlbums = new List<MusicBrainzHelper>();
                        foreach (Release r in a1.Releases)
                        {
                            MusicBrainzHelper temp_mb = new MusicBrainzHelper();
                            Id3Tag i3A = i3;

                            AlbumName = r.Title;
                            AlbumID = r.Id;


                            i3A.Album = r.Title;
                            i3A.Track = new Id3.Frames.TrackFrame(r.Media[0].Position, r.Media[0].TrackCount);

                            if (!String.IsNullOrEmpty(r.Date))
                            {
                                string[] rdate = r.Date.Split('-');
                                i3A.Year = int.Parse(rdate[0]);
                                i3A.RecordingDate = new DateTime(int.Parse(rdate[0]), int.Parse(rdate[1]), int.Parse(rdate[2]));
                            }

                            // get album art
                            Uri uri = CoverArtArchive.GetCoverArtUri(AlbumID);
                            byte[] imageBytes;
                            using (var webClient = new WebClient())
                            {
                                imageBytes = webClient.DownloadData(uri);
                            }
                            i3A.Pictures.Add(new Id3.Frames.PictureFrame() { Description = "Front Cover", PictureData = imageBytes, PictureType = Id3.Frames.PictureType.FrontCover });

                            // Still needs tracklist
                            var tracklist = await client.Releases.GetAsync(AlbumID, "artists", "recordings");

                            temp_mb.RootTag = i3A;
                            temp_mb.SetTracksId3Tags(tracklist.Media.First().Tracks);
                            tempAlbums.Add(temp_mb);
                        }

                        if (tempAlbums.Count == 1)
                        {
                            mb = tempAlbums.First();
                            mb.isSimple = true;
                        }
                        else
                        {
                            mb.RootTag = i3;
                            mb.PossibleAlbums = tempAlbums;
                            mb.isComplex = true;
                            isComplex = true;
                        }

                        TrackSearchResults.Add(mb);
                    }

                    //is simple
                    if (TrackSearchResults.Count == 1)
                        isSimple = true;
                    if (TrackSearchResults.Count > 1)
                        isComplex = true;


                }
            }

            #endregion
        }

        public ImportManager im = new ImportManager();



        //public List<ListBoxItem> FullList = new List<ListBoxItem>();
        //public List<ListBoxItem> RawList = new List<ListBoxItem>();
        //public List<ListBoxItem> Id3List = new List<ListBoxItem>();
        //public List<ListBoxItem> Id3CompleteList = new List<ListBoxItem>();
        //public List<ListBoxItem> DatabaseList = new List<ListBoxItem>();

        public FileManager()
        {
            InitializeComponent();
        }

        private void btnImport_Click(object sender, RoutedEventArgs e)
        {
            //ispathvalid
            if (!string.IsNullOrEmpty(tbImportPath.Text))
            {
                im = new ImportManager();

                // List songs
                im.LoadDirlisting(tbImportPath.Text);

                //update button counts
                btnFilterRaw.Content = "Raw (" + im.getListBox_NoData().Count() + ")";
                btnFilterID3Tags.Content = "ID3 Tags (" + im.getListBox_Id3().Count() + ")";
                btnFilterDatabase.Content = "Database (" + "0" + ")";
                btnFilterReset.Content = "Reset (" + im.getListBox_All().Count() + ")";

            }
        }

        private bool hasID3v1(string filepath)
        {
            using (var mp3 = new Mp3(filepath))
            {
                foreach (Id3Version iv in mp3.AvailableTagVersions)
                {
                    if (iv == Id3Version.V1X)
                    {
                        Id3Tag tag = mp3.GetTag(Id3TagFamily.Version1X);
                        return true;
                    }
                }
            }
            return false;
        }




        private void btnImportSelected_Click(object sender, RoutedEventArgs e)
        {
            if (listDir.SelectedItems.Count > 0)
            {

                //ListBoxItem lb = (ListBoxItem)listDir.SelectedItem;
                ImportSelected(true);
            }
        }

        private void btnImportAll_Click(object sender, RoutedEventArgs e)
        {
            string TestPlay = ConfigurationManager.AppSettings.Get("BaseMusicDir");

            ImportSelected();


            //JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities();

            //// take every track and add it
            //// log files
            //foreach (ListBoxItem l in listDir.Items)
            //{
            //    string s = "";
            //    //check if in database

            //    if (!db.TrackLibraries.Any(x => x.FileName + x.Extention == l.Content.ToString()))
            //    {
            //        // find karaoke first

            //        //  items.Add(new ListBoxItem() { ToolTip = musicFilePath, Content = System.IO.Path.GetFileName(musicFilePath), Tag = musicFilePath });
            //        if (Global.musicformats.Contains(System.IO.Path.GetExtension(l.ToolTip.ToString())))
            //        {
            //            s = "Music";
            //        }
            //        else if (Global.videoformats.Contains(System.IO.Path.GetExtension(l.ToolTip.ToString())))
            //        {
            //            s = "Video";
            //        }
            //        else if (Global.karaokeformats.Contains(System.IO.Path.GetExtension(l.ToolTip.ToString())))
            //        {
            //            s = "Karaoke";
            //        }

            //        TrackLibrary tl = new TrackLibrary() { FilePath = l.ToolTip.ToString(), FileName = System.IO.Path.GetFileNameWithoutExtension(l.Content.ToString()), Type = s };
            //        db.TrackLibraries.Add(tl);
            //    }
            //}

            //db.SaveChanges();

            //var list = (from t in db.TrackLibraries
            //            select new ListBoxItem() { Tag = t.FilePath, Content = t.FileName }).ToList();
            //listDir.ItemsSource = list;
            // find possible matches
        }

        private void ImportSelected(bool isSingleImport = false)
        {
            List<ListBoxItem> list = new List<ListBoxItem>();

            if (isSingleImport)
                list.Add((ListBoxItem)listDir.SelectedItem);
            else
            {
                foreach (ListBoxItem l in listDir.Items)
                {
                    list.Add(l);
                }
            }

            foreach (ListBoxItem lb in list)
            {
                JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities();
                // TODO: check if in database
                string songname = System.IO.Path.GetFileNameWithoutExtension(lb.Content.ToString());
                SongLibrary sl = new SongLibrary() { SongName = songname };

                if (db.SongLibraries.Any(w => w.SongName == songname))
                {
                    sl = db.SongLibraries.First(w2 => w2.SongName == songname);
                }

                if (!db.TrackLibraries.Any(w => w.FileName == songname))

                    if (Global.karaokeformats.Contains(System.IO.Path.GetExtension(lb.ToolTip.ToString())))
                    {
                        // is Karaoke Mode
                        TrackLibrary tl_k = new TrackLibrary() { FilePath = lb.ToolTip.ToString(), FileName = System.IO.Path.GetFileNameWithoutExtension(lb.Content.ToString()), Type = "Karaoke", Extention = System.IO.Path.GetExtension(lb.ToolTip.ToString()) };
                        TrackLibrary tl_a = new TrackLibrary() { FilePath = lb.Tag.ToString(), FileName = System.IO.Path.GetFileNameWithoutExtension(lb.Content.ToString()), Type = "Karaoke", Extention = System.IO.Path.GetExtension(lb.Tag.ToString()) };
                        sl.TrackLibraries.Add(tl_k);
                        sl.TrackLibraries.Add(tl_a);

                        db.SongLibraries.Add(sl);
                        db.SaveChanges();
                    }
                    else if (Global.musicformats.Contains(System.IO.Path.GetExtension(lb.ToolTip.ToString())))
                    {
                        // if music
                        TrackLibrary tl = new TrackLibrary() { FilePath = lb.ToolTip.ToString(), FileName = System.IO.Path.GetFileNameWithoutExtension(lb.Content.ToString()), Type = "Music" };
                        sl.TrackLibraries.Add(tl);

                        db.SongLibraries.Add(sl);
                        db.SaveChanges();
                    }
                    else if (Global.videoformats.Contains(System.IO.Path.GetExtension(lb.ToolTip.ToString())))
                    {
                        // if video
                        TrackLibrary tl = new TrackLibrary() { FilePath = lb.ToolTip.ToString(), FileName = System.IO.Path.GetFileNameWithoutExtension(lb.Content.ToString()), Type = "Video" };
                        sl.TrackLibraries.Add(tl);

                        db.SongLibraries.Add(sl);
                        db.SaveChanges();
                    }
            }
        }

        private void btnFilterRaw_Click(object sender, RoutedEventArgs e)
        {
            listDir.ItemsSource = im.getListBox_NoData();
        }

        private void btnFilterID3Tags_Click(object sender, RoutedEventArgs e)
        {
            listDir.ItemsSource = im.getListBox_Id3();
            btnLoadID3.Visibility = Visibility.Visible;
        }

        private void btnFilterID3Comp_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnFilterDatabase_Click(object sender, RoutedEventArgs e)
        {
            listDir.ItemsSource = im.getListBox_All();
        }

        private void btnFilterReset_Click(object sender, RoutedEventArgs e)
        {
            listDir.ItemsSource = im.getListBox_All();
        }


        private void AutoImportStep1()
        {
            var location = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var client = new MusicBrainzClient()
            {
                Cache = new FileRequestCache(Path.Combine(location, "cache"))
            };

            //buffer.Filepath = ((ListBoxItem)listDir.SelectedItems[0]).Tag.ToString();
            //buffer.Filename = Path.GetFileNameWithoutExtension(((ListBoxItem)listDir.SelectedItems[0]).Tag.ToString());

            //foreach (string s in buffer.Filename.Split('-'))
            // position save
            // delim save
            string s = "";
            //var task = RunTrackSearch(client, s.Trim());
            //task.Wait();


            // Cycle through album track lists
            foreach (Recording r in bAlbum.Tracks)
            {
                //r.Title == 
            }
        }

        private void AutoImportAll()
        {
            var location = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var client = new MusicBrainzClient()
            {
                Cache = new FileRequestCache(Path.Combine(location, "cache"))
            };

            //TrackTodoList = new List<qwert>();
            foreach (ListBoxItem li in listDir.Items)
            {
                qwert q = new qwert() { lbi = li };
                string Filename = Path.GetFileNameWithoutExtension(((ListBoxItem)listDir.SelectedItems[0]).Tag.ToString());
                foreach (string s in Filename.Split('-'))
                {
                    // check in album tracks
                    foreach (var x in Al)
                    {
                        foreach (var y in x.Tracks)
                        {
                            if (y.Title == s.Trim())
                            {
                                q.setId3(x.GetId3Tag(y));
                                q.removeTrack(y);

                            }
                        }
                    }


                    // position save
                    // delim save
                    //var task = RunTrackSearch(client, s.Trim());
                    //task.Wait();
                }


            }

            // TrackTodoList
            bufferAlbums b = new bufferAlbums();
            foreach (var b1 in b.Tracks)
            {

            }
        }

        class qwert
        {
            public ListBoxItem lbi { get; set; }
            public bool hasID3 { get; set; }
            public Id3Tag Id3 { get; set; }

            public int array_position { get; set; }
            public char saved_delim { get; set; }

            public int TrackCount { get; set; }


            internal void removeTrack(Recording y)
            {

            }

            internal void setId3(Id3Tag id3Tag)
            {
                Id3 = id3Tag;
                hasID3 = true;
            }
        }

        public async Task RunTrackSeachOnly(MusicBrainzClient client, string searchname)
        {
            var recordings = await client.Recordings.SearchAsync(searchname.Quote(), 20);
            if (recordings.Count > 0)
            {
                // id3
                // database

                foreach (var rec in recordings)
                {
                    //Al.Add(rec);
                    bufferAlbums al = new bufferAlbums();

                    // save artists
                    foreach (var a2 in rec.Credits)
                    {
                        al.Artist.Add(a2);
                    }

                    // save albums
                    foreach (var a3 in rec.Releases)
                    {
                        al.GetAlbumDetails(a3);
                    }

                    if (rec.Releases.Count() == 1)
                    {
                        // Find Other Albums

                    }
                }

            }
        }

        //public List<qwert> TrackTodoList { get; set; }

        public bool isSingleAlbum = false;
        public bufferAlbums bAlbum = new bufferAlbums();

        public List<bufferAlbums> Al = new List<bufferAlbums>();
        public class bufferAlbums
        {
            public List<Recording> Tracks { get; set; }
            public Release Albums { get; set; }
            public List<NameCredit> Artist { get; set; }
            public Id3Tag i3 { get; set; }
            public bool isDepleted { get; set; }

            public Id3Tag GetId3Tag(Recording rec)
            {
                //i3.Track = new Id3.Frames.TrackFrame(r.Media[0].Position, r.Media[0].TrackCount);
                //Title = a1.Title
                //        x.Artist.Add(nc);
                //artist = nc.Artist.Name;
                //i3.Artists.Value.Add(nc.Name);
                //a.Add(new ListBoxItem() { Content = a1.Title + " - " + nc.Name });
                return i3;
            }

            public void GetAlbumDetails(Release rel)
            {
                i3.Album = rel.Title;
                //AlbumName = r.Title;
                //AlbumID = r.Id;
                //i3.Track = new Id3.Frames.TrackFrame(r.Media[0].Position, r.Media[0].TrackCount);

                string[] rdate = rel.Date.Split('-');
                i3.Year = int.Parse(rdate[0]);
                i3.RecordingDate = new DateTime(int.Parse(rdate[0]), int.Parse(rdate[1]), int.Parse(rdate[2]));

                // get album art
                Uri uri = CoverArtArchive.GetCoverArtUri(rel.Id);
                byte[] imageBytes;
                using (var webClient = new WebClient())
                {
                    imageBytes = webClient.DownloadData(uri);
                }
                i3.Pictures.Add(new Id3.Frames.PictureFrame() { Description = "Front Cover", PictureData = imageBytes, PictureType = Id3.Frames.PictureType.FrontCover });
            }
        }



        public async Task Run(MusicBrainzClient client, string searchname)
        {
            string album = "A Is for Accident";
            string name = "Amanda Palmer";

            //var x = Collections.ArtistList;
            //var y = Artist;

            var albums = await client.Artists.SearchAsync(name.Quote(), 20);

            var query = new QueryParameters<Release>()
            {
                { "release", album },
                { "type", "album" }
            };

            var r = await client.Releases.SearchAsync(query);

        }

        private void btnMusicBrainz_Click(object sender, RoutedEventArgs e)
        {
            AutoImportStep1();
        }

        private void btnWriteID3_Click(object sender, RoutedEventArgs e)
        {
            //Mp3 ix = new Mp3(buffer.Filepath, Mp3Permissions.ReadWrite);
            //ix.WriteTag(buffer.Id3Tags[0], Id3Version.V23, WriteConflictAction.Replace);
        }

        private void btnTestMusicBrainz_Click(object sender, RoutedEventArgs e)
        {
            // force to selected only
            string filepath = ((ListBoxItem)listDir.SelectedItem).Tag.ToString();
            var i = (from a in im.ImportList
                     where a.FilePath == filepath
                     select a).First();

            im.PullSingleBrainzData(i);

            var x = from o in im.Outbox
                    select new ListBoxItem() { Content = "" + o.Id3Tag.Track.Value + " " + o.Id3Tag.Title + " - " + o.Id3Tag.Artists.Value.First() + " - " + o.Id3Tag.Album.Value, Tag = o.FilePath, ToolTip = o.FilePath };
            listDir.ItemsSource = x;
        }

        private void LoadID3Tags()
        {

        }

        private void btnLoadID3_Click(object sender, RoutedEventArgs e)
        {
            string filepath = ((ListBoxItem)listDir.SelectedItem).ToolTip.ToString();
            id3Listbox.Items.Clear();
            using (var mp3 = new Mp3(filepath))
            {
                foreach (Id3Version iv in mp3.AvailableTagVersions)
                {
                    if (iv == Id3Version.V1X)
                    {
                        Id3Tag tag = mp3.GetTag(Id3TagFamily.Version1X);
                        id3Listbox.Items.Add(tag.Title.Value);
                    }
                    if (iv == Id3Version.V23)
                    {
                        Id3Tag tag = mp3.GetTag(Id3TagFamily.Version2X);
                        //tag.Pictures.First().PictureData.Length;
                        id3Listbox.Items.Add(tag.Artists.Value.First().ToString());
                        id3Listbox.Items.Add(tag.Album.Value);
                        id3Listbox.Items.Add(tag.Title.Value);
                        id3Listbox.Items.Add(tag.Genre.Value);
                        id3Listbox.Items.Add(tag.Length.Value.TotalMinutes.ToString());
                        id3Listbox.Items.Add(tag.Year.Value.ToString());

                    }
                }
            }
        }
    }
}
