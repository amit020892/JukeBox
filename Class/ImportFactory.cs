using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Id3;
using Hqub.MusicBrainz.API;
using Hqub.MusicBrainz.API.Entities;
using Hqub.MusicBrainz.Client;
using System.Reflection;
using System.Net;
using System.Diagnostics;
using iTunesSearch.Library;
using System.Threading;

namespace JukeBoxSolutions
{
    public class ImportFactory
    {
        #region FileManagerV3


        // load dir listing


        public void ImportSelectedFiles(List<string> filePaths, bool includeAudio = true, bool includeVideo = true, bool includeKaraoke = true)
        {
            ImportVideoFile(new ManagedFile());

        }


        #endregion FileManagerV3


        public bool DisableImportValidation = false;

        private static List<Id3Tag> AlbumsTrackShelf = new List<Id3Tag>();
        OnlineFactory online = new OnlineFactory();


        //public static List<string> ArtistBuffer { get; set; }
        public List<ManagedFile> ImportList { get; private set; }
        public List<ManagedFile> OrderedImportList { get; private set; }

        public List<ManagedFile> UnmatchedFiles { get; private set; }
        public List<ManagedFile> MatchedFiles { get; private set; }
        public AnalisisEngine aEngine = new AnalisisEngine();

        IGrouping<string, List<ManagedFile>> ImportListGrouped { get; set; }


        // Global Session

        public List<FactoryArtist> ArtistBuffer = new List<FactoryArtist>();
        public void AddBufferArtist(SearchAnalysisRule rule, Artist artist)
        {
            // TODO: check duplicates
            if (!ArtistBuffer.Any(c => c.Artist == artist))
                ArtistBuffer.Add(new FactoryArtist() { Rule = rule, Artist = artist });
        }
        public void AddBufferAnalysisRule(SearchAnalysisRule rule) { }

        public List<FactoryAlbum> AlbumBuffer = new List<FactoryAlbum>();

        public void AddBufferAlbum(SearchAnalysisRule rule, AlbumLibrary dbAlbum)
        {
            // TODO: check duplicates

            AlbumBuffer.Add(new FactoryAlbum() { Rule = rule, Album = dbAlbum });
        }

        public List<FactorySong> MismatchSongBuffer = new List<FactorySong>();
        public void AddBufferSongMismatch(ManagedFile managedFile, List<iTunesSearch.Library.Models.Song> songMatches)
        {
            MismatchSongBuffer.Add(new FactorySong() { ManagedFile = managedFile, MismatchSongs = songMatches });
        }

        public List<FactoryMismatchAlbum> MismatchAlbumBuffer = new List<FactoryMismatchAlbum>();
        public void AddMismatchAlbumBuffer(iTunesSearch.Library.Models.Album baseAlbum, ManagedFile missmachedSong)
        {
            if (MismatchAlbumBuffer.Where(w => w.BaseAlbum == baseAlbum).Any())
            {
                MismatchAlbumBuffer.Where(w => w.BaseAlbum == baseAlbum).First().ManagedFiles.Add(missmachedSong);
            }
            else
                MismatchAlbumBuffer.Add(new FactoryMismatchAlbum() { BaseAlbum = baseAlbum, ManagedFile = missmachedSong });

        }
        public void AddMismatchAlbumBuffer(iTunesSearch.Library.Models.Album baseAlbum, List<iTunesSearch.Library.Models.Song> remainingSongs)
        {
            if (MismatchAlbumBuffer.Where(w => w.BaseAlbum == baseAlbum).Any())
            {
                MismatchAlbumBuffer.Where(w => w.BaseAlbum == baseAlbum).First().SongOptions = remainingSongs;
            }
            else
            {
                MismatchAlbumBuffer.Add(new FactoryMismatchAlbum() { BaseAlbum = baseAlbum, SongOptions = remainingSongs });
            }
        }

        // Local Session
        public void SessionOpen()
        {
            //sessionDBSongId = new List<int>();
            sessionAlbums = new List<FactorySessionAlbum>();
        }

        public void AddSessionSongId(int dbSongId)
        {
            //sessionDBSongId.Add(dbSongId);
        }

        //private List<int> sessionDBSongId { get; set; }
        internal List<FactorySessionAlbum> sessionAlbums = new List<FactorySessionAlbum>();
        /// <summary>
        /// If Collection Id is in a different set, merge with set. Otherwise create new set
        /// </summary>
        /// <param name="albums"></param>
        /// <param name="dbSongId"></param>
        public void SessionAddAlbums(List<iTunesSearch.Library.Models.Song> albums, int dbSongId)
        {
            int i = 1;
            List<int> iDup = new List<int>();

            Dictionary<int, int> lookupIndex = new Dictionary<int, int>();

            if (sessionAlbums.Any())
            {
                // get new collection ids
                var cid = (from a in albums select a.CollectionId).Distinct();
                var sid = (from a2 in sessionAlbums
                           where cid.Contains(a2.album.CollectionId)
                           select a2.SetId).Distinct().ToList();

                if (!sid.Any())
                {
                    // create new set id
                    i = sessionAlbums.Max(m => m.SetId) + 1;
                }
                else if (sid.Count() == 1)
                    // attach to existing album set
                    i = sid.First();
                else
                // Features in more than one group, making this a bridge group
                // Merges the two groups and joins that group
                {
                    // OPTION 1 : Merge / Bridge Groups
                    bool isOPTION1 = false;
                    if (isOPTION1)
                    {
                        int firstgroup = sid.Min();
                        int counter1 = sessionAlbums.Count(c => c.SetId == firstgroup);
                        foreach (int set in sid)
                        {
                            if (set != firstgroup)
                            {
                                sessionAlbums.Where(w => w.SetId == set).ToList().ForEach(u => u.SetId = firstgroup);

                                int counter2 = sessionAlbums.Count(c => c.SetId == set);
                                int counter3 = sessionAlbums.Count(c => c.SetId == firstgroup);
                            }
                        }
                        i = firstgroup;
                    }
                    // OPTION 2 : Duplicate into found groups
                    else
                    {
                        iDup = sid;
                        i = 0;
                    }
                }
            }

            if (i != 0)
            {
                foreach (var a in albums)
                {
                    sessionAlbums.Add(new FactorySessionAlbum() { album = a, DBSongId = dbSongId, SetId = i });
                }
            }
            else if (iDup.Any())
            {
                foreach (int i2 in iDup)
                    foreach (var a in albums)
                    {
                        sessionAlbums.Add(new FactorySessionAlbum() { album = a, DBSongId = dbSongId, SetId = i2 });
                    }
            }
        }
        public class AlbumSetGroup
        {
            public long CollectionId { get; internal set; }
            public List<FactorySessionAlbum> Tracks { get; internal set; }
            public int Count { get; internal set; }
            public int ArtistCount { get; internal set; }
        }
        public void SessionSaveChanges()
        {
            if (sessionAlbums.Count > 0)
            {
                // Commits Data to DB
                // Creates new iTunesThread for AlbumArt and things


                // get set numbers
                var sets = (from setX in sessionAlbums select setX.SetId).Distinct();


                foreach (int i in sets)
                {
                    //Dictionary<int, long> listOfDBEntries = new Dictionary<int, long>();

                    // First, get the right album
                    bool setcatcher = false;
                    if (i == 6)
                        setcatcher = true;
                    var LikelyAlbum = sessionAlbums.Where(w => w.SetId == i).First();
                    List<AlbumSetGroup> AlbumGroups = (from x in sessionAlbums.Distinct()
                                                       where x.SetId == i
                                                       group x by x.album.CollectionId into g
                                                       select new AlbumSetGroup { CollectionId = g.Key, Tracks = g.Distinct().ToList(), Count = g.Distinct().Count(), ArtistCount = g.Distinct().Select(cA => cA.album.ArtistId).Distinct().Count() }).ToList();

                    // Load DB saved entries to adjust results
                    using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                    {
                        var DBAlbumGroups = (from aDB in db.AlbumLibraries
                                             where aDB.hasiTunesUpdate
                                             select new { CollectionId = aDB.iTunesId, Tracks = aDB.SongLibraries.Select(z => z.SongId).ToList(), Count = aDB.SongLibraries.Count() }).ToList();

                        // update count value
                        AlbumGroups.Select(upD => { upD.Count = upD.Count + (DBAlbumGroups.Any(f => f.CollectionId == upD.CollectionId.ToString()) ? DBAlbumGroups.Find(f => f.CollectionId == upD.CollectionId.ToString()).Count : 0); return upD; }).ToList();
                    }

                    //AlbumGroups.Select(upD => { upD.Count = upD.Count + 5; return upD; }).ToList();

                    AlbumGroups = AlbumGroups.OrderBy(o1 => o1.ArtistCount).ThenByDescending(o2 => o2.Count).ToList();




                    int minTrackHitCount = AlbumGroups.Max(m => m.Count);


                    // Grouped by MaxCount


                    // number of tracks
                    // list of assigned tracks




                    // removes invalid albums, with single tracks, add all albums
                    // hide all albums, unhide the most correct one

                    //long tempUnhideId = LikelyAlbum.album.CollectionId;
                    List<long> unhideId = new List<long>();

                    int unhideAlbumTrackCount = LikelyAlbum.album.TrackCount;
                    int unhideTrackCount = AlbumGroups.Where(w => w.CollectionId == LikelyAlbum.album.CollectionId).First().Count;
                    long artistID = LikelyAlbum.album.ArtistId;

                    double MaxCompletepers = 0;


                    // Saves the index for mixed albums
                    List<int> albumGroupMixList = new List<int>();
                    List<int> bufferDBLists = new List<int>();
                    for (int iCounter = 0; iCounter < AlbumGroups.Count(); iCounter++)
                    {
                        var al = AlbumGroups[iCounter];

                        //
                        List<int> batchIDs = al.Tracks.Select(s1 => s1.DBSongId).ToList();
                        int batchCount = bufferDBLists.Count(c => batchIDs.Contains(c));

                        if (batchCount == 0)
                        {
                            unhideId.Add(al.CollectionId);
                            bufferDBLists.AddRange(al.Tracks.Select(s => s.DBSongId).Distinct());
                        }
                        else if (batchIDs.Count() == batchCount)
                        {
                            // all tracks already in buffer, ignore
                        }
                        else
                        {
                            albumGroupMixList.Add(iCounter);
                        }
                    }

                    // grab ALL dbIDs and see if all is assigned
                    List<int> AllDBIds = AlbumGroups.SelectMany(sM => sM.Tracks.Select(sN => sN.DBSongId)).Distinct().ToList();
                    List<int> remaingingDBIds = AllDBIds.Where(w => !bufferDBLists.Contains(w)).ToList();

                    List<int> albumGroupMixList2 = new List<int>();
                    foreach (int iMixC in albumGroupMixList)
                    {
                        var alMix = AlbumGroups[iMixC];

                        List<int> batchIDs = alMix.Tracks.Select(sM1 => sM1.DBSongId).ToList();
                        int batchCount = bufferDBLists.Count(c => batchIDs.Contains(c));

                        if (batchCount == 0)
                        {
                            unhideId.Add(alMix.CollectionId);
                            bufferDBLists.AddRange(alMix.Tracks.Select(s => s.DBSongId).Distinct());
                        }
                        else if (batchIDs.Count() == batchCount)
                        {
                            // all tracks already in buffer, ignore
                        }
                        else
                        {
                            albumGroupMixList2.Add(iMixC);
                        }
                    }


                    //unhideId.Add(tempUnhideId);
                    iTunesSearchManager sm = new iTunesSearchManager();
                    sm.AttachNewAlbums(sessionAlbums.Where(w => w.SetId == i).ToList(), unhideId);
                    // End sets loop
                }
            }
        }


        public class FactoryAlbum
        {
            public SearchAnalysisRule Rule { get; set; }
            public AlbumLibrary Album { get; set; }
        }

        public class FactorySessionAlbum
        {
            public int SetId { get; set; }
            public int DBSongId { get; set; }
            public iTunesSearch.Library.Models.Song album { get; set; }
        }

        public class FactoryArtist
        {
            public SearchAnalysisRule Rule { get; set; }
            public Artist Artist { get; set; }
        }

        public class FactorySong
        {
            public ManagedFile ManagedFile { get; set; }
            public List<iTunesSearch.Library.Models.Song> MismatchSongs { get; set; }
        }

        public class FactoryMismatchAlbum
        {
            public ManagedFile ManagedFile
            {
                set
                {
                    ManagedFiles.Add(value);
                }
            }
            public iTunesSearch.Library.Models.Album BaseAlbum { get; internal set; }

            public List<iTunesSearch.Library.Models.Song> SongOptions = new List<iTunesSearch.Library.Models.Song>();
            public List<ManagedFile> ManagedFiles = new List<ManagedFile>();
        }

        public ImportFactory()
        {
            ImportList = new List<ManagedFile>();
            UnmatchedFiles = new List<ManagedFile>();
            MatchedFiles = new List<ManagedFile>();
            ArtistBuffer = new List<FactoryArtist>();
        }


        #region OnlineSearches

        public void SearchOnline(ManagedFile mf)
        {
            PullMusicBrainzData(mf);
        }

        internal void PullMusicBrainzData(ManagedFile mf)
        {
            // Workbench
            //int ImportListMarker = 0;
            //List<WorkbenchFile> ComplexShelf = new List<WorkbenchFile>();
            //List<ManagedFile> Inbox = ImportList;
            //List<ManagedFile> Failbox = new List<ManagedFile>();
            //List<ManagedFile> Outbox = new List<ManagedFile>();


            // set up MusicBrainz
            var location = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var client = new MusicBrainzClient()
            {
                Cache = new FileRequestCache(Path.Combine(location, "cache"))
            };


            // pull working file data
            // get possible search names
            //foreach (ManagedFile inbox in Inbox)

            //ManagedFile inbox = Inbox.First();

            WorkbenchFile WorkingFile = new WorkbenchFile(mf);

            // Look if track is possibly in Album Shelf
            //if (WorkingFile.SearchShelf(AlbumsTrackShelf))
            //{
            //    //found result m
            //    ManagedFile m2 = inbox;
            //    m2.setId3Tag(WorkingFile.FoundTrack);
            //    Outbox.Add(m2);
            //    Inbox.Remove(inbox);
            //    // remove track from workbench shelf
            //    AlbumsTrackShelf.Remove(WorkingFile.FoundTrack);
            //}


            // search existing albums
            //var task = online.RunTrackSearch(client, mf.searchTrack, mf.searchArtist);
            OnlineFactory2.Run(client);
            //var task = online.RunTrackSearch(client, mf.searchTrack);
            //int timeout = 1000;
            //  task.Wait(timeout);

            string s = "Something went wrong";
            //isSimple = false;
            //isComplex = false;
            //TrackSearchResults = new List<MusicBrainzHelper>();

            //if (online.TrackSearchResults.Count() == 1)
            //{
            //    mf.setId3Tag(online.TrackSearchResults.First().RootTag);
            //}

            //AlbumsTrackShelf.AddRange(online.TrackSearchResults.First().AlbumTracks);
        }


        public class WorkbenchFile
        {
            public bool keepseaching = true;
            public string testname { get { return testnames[splitindex]; } }
            public List<string> testnames = new List<string>();
            public int splitindex = 0;

            // internal List<MusicBrainzHelper> ResultsSearchBuffer = new List<MusicBrainzHelper>();


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

            //internal void AttachSearchResults(List<MusicBrainzHelper> results)
            //{
            //    ResultsSearchBuffer.AddRange(results);
            //}

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



        #endregion OnlineSearches

        #region File Analysis


        internal ImportFactory.AnalysisFile AnalyseFilename(ManagedFile mf, bool pullArtist)
        {
            if (pullArtist)
            {
                foreach (var a in mf.Id3Tag.Artists.Value)
                {
                    aEngine.Buffer_AddArtist(a);
                }
            }

            return aEngine.AnalizeString(mf.FileName);
        }



        public class AnalysisFile
        {
            public bool hasAlbum { get; set; }
            public bool hasArtist { get; set; }
            public bool hasTrack { get; set; }
            public List<Info> additionalInfo { get; set; }

            public List<AnalysisPart> NamePortions { get; set; }
            public bool hasTrackNumber { get; internal set; }

            public AnalysisFile()
            {
                hasAlbum = false;
                hasArtist = false;
                hasTrack = false;
                hasTrackNumber = false;
                additionalInfo = new List<Info>();
                NamePortions = new List<AnalysisPart>();
            }

            public class Info
            {
                public string key { get; set; }
                public string Value { get; set; }
            }
        }

        public class SearchAnalysisRule
        {
            public string fileSearchPattern { get; internal set; }

            public Verdict ResultVerdict = Verdict.Untouched;
            public enum Verdict
            {
                Untouched,
                NoResults,
                Inconclusive,
                Conclusive
            }
            public iTunesSearch.Library.Models.SongArtistResult SongArtistResult { get; set; }
            public string SearchTerm { get; set; }
            public string TrackTerm { get; set; }
            public string ArtistSearchTerm { get; set; }
            public bool isDelimiter
            {
                get
                {
                    if (String.IsNullOrEmpty(Delimiter)) return false;
                    else return true;
                }
            }
            public string Delimiter { get; set; }

            /// <summary>
            /// The default index used for the position of the text part in the string
            /// </summary>
            public int SearchPartIndex { get; set; }
            public int ArtistPartIndex { get; set; }
            public int TrackPartIndex { get; set; }
            public string ArtistName { get; set; }
            public int TrackNumber { get; set; }
            public bool isPossibleTrackNumber = false;
            internal bool isPatternMatch = false;

            public SearchAnalysisRule()
            { }
            public SearchAnalysisRule(string artistInsert, string trackTerm)
            {
                SearchTerm = artistInsert + trackTerm;
                TrackTerm = trackTerm;
            }
            public SearchAnalysisRule(string searchTerm)
            {
                SearchTerm = searchTerm;
            }

            internal string GetTrackString(string filename)
            {
                if (isDelimiter)
                {
                    char d = Delimiter.ToCharArray()[0];
                    string f = filename;

                    if (filename.Contains(Delimiter) && filename.Split(d).Count() > SearchPartIndex)
                        f = ("" + filename.Split(d)[SearchPartIndex]).Trim();

                    return f;
                }
                else
                {
                    return filename;
                }
            }
            internal List<string> GetSearchStrings(List<string> v)
            {
                if (isDelimiter)
                {
                    char d = Delimiter.ToCharArray()[0];
                    var a = (from name in v where name.Contains(Delimiter) && name.Split(d).Count() > SearchPartIndex select ("" + name.Split(d)[SearchPartIndex]).Trim()).ToList();
                    if (a.Any())
                    {
                        var x = a.First();
                        return a.ToList();
                    }
                    else
                    {
                        return new List<string>();
                    }
                }
                else
                {
                    return new List<string>();
                }
            }

            internal SearchAnalysisRule Clone(string filename)
            {
                SearchAnalysisRule r = this;

                if (isDelimiter)
                {
                    char d = Delimiter.ToCharArray()[0];
                    var arr = filename.Split(d);

                    //string f = filename;

                    //if (filename.Contains(Delimiter) && filename.Split(d).Count() > SearchPartIndex)
                    //    f = ("" + filename.Split(d)[SearchPartIndex]).Trim();

                    if (isPossibleTrackNumber)
                    {
                        var x = TrackTerm;
                        r.TrackTerm = arr[TrackPartIndex];
                        int t = 0;
                        r.TrackNumber = 0;
                    }

                    if (arr.Count() > SearchPartIndex)
                        r.SearchTerm = arr[SearchPartIndex].Trim();

                    return r;
                }
                else if (isPatternMatch)
                {
                    SearchTerm = filename.ToLower().Replace("%2f", " ");
                    ArtistSearchTerm = "";
                    return r;
                }
                else
                {
                    if (isPossibleTrackNumber)
                    {
                        string p1 = filename.Split(' ')[0];
                        string p2 = filename.Substring(p1.Length);

                        r.TrackNumber = int.Parse(p1);
                        r.SearchTerm = p2.Trim();
                        return r;
                    }

                    return r;
                }
            }
        }
        public class AnalysisPart
        {
            public string NamePortion { get; set; }
            public string SlotMode { get; set; }
        }

        public class AnalisisEngine
        {
            AnalysisFile an = new AnalysisFile();

            internal static char[] delims = { '.', '-', '|', '¦' };
            internal static char[] bracketsOpen = { '(', '[', '{' };
            internal static char[] bracketsClose = { ')', ']', '}' };
            // formula?
            // A|S|
            // 

            public AnalysisFile AnalizeString(string filename)
            {
                // pull brackets
                AnalysisFile af = new AnalysisFile();
                string s = filename;
                string middle = "";

                for (int i = 0; i < bracketsOpen.Count(); i++)
                {
                    if (filename.Contains(bracketsOpen[i]))
                    {
                        int iStart = filename.IndexOf(bracketsOpen[i]);
                        int iEnd = filename.LastIndexOf(bracketsClose[i]);
                        middle = filename.Substring(iStart, iEnd - iStart + 1);
                        s = filename.Remove(iStart, iEnd - iStart + 1).Trim();
                    }
                }

                //starts with tracknumber
                // .. 30 seconds to mars?

                string tracknumber = "";
                int iTrackNumber;
                if (int.TryParse(s.Split(' ').First(), out iTrackNumber))
                {
                    tracknumber = s.Split(' ').First();
                    s = s.Substring(tracknumber.Length);
                }

                af.NamePortions = new List<AnalysisPart>();
                // split up words
                foreach (char d in delims)
                {
                    if (s.Contains(d))
                    {
                        af.NamePortions = (from w in s.Split(new char[] { d }, StringSplitOptions.RemoveEmptyEntries)
                                           where !string.IsNullOrEmpty(w.Trim())
                                           select new AnalysisPart() { NamePortion = w.Trim(), SlotMode = "Open" }).ToList();
                    }
                }

                // check for artist
                bool foundartist = false;
                if (af.NamePortions.Count() > 0)
                {
                    foreach (var port in af.NamePortions)
                    {
                        //foreach (string art in ArtistBuffer)
                        //{
                        //    if (art.Equals(port.NamePortion, StringComparison.OrdinalIgnoreCase))
                        //    {
                        //        port.SlotMode = "Artist";
                        //        af.hasArtist = true;
                        //        foundartist = true;
                        //    }
                        //}
                    }
                }
                else
                {
                    af.NamePortions.Add(new AnalysisPart() { NamePortion = s, SlotMode = "Open" });
                }

                if (tracknumber != "")
                    af.NamePortions.Insert(0, new AnalysisPart() { NamePortion = tracknumber, SlotMode = "TrackNumber" });

                if (foundartist && af.NamePortions.Count == 2)
                {
                    // assuming the other one is then the track
                    af.NamePortions.Where(w => w.SlotMode == "Open").First().SlotMode = "Track";
                    af.hasTrack = true;
                }

                return af;
            }

            // Add Artist
            public void Buffer_AddArtist(string artist)
            {
                //ArtistBuffer.Add(artist);
            }

            /// <summary>
            /// Same as get track strings, but skips the starting tracknumber option
            /// </summary>
            /// <param name="filename">The raw filename string to pull from</param>
            /// <returns></returns>


            private bool KeepCapitalization = false;
            public List<SearchAnalysisRule> GetArtistSearchStrings(string filename, bool keepCapitalization = false)
            {
                KeepCapitalization = keepCapitalization;
                var x = GetTrackSearchStrings(filename, true);
                x.ForEach(fe => fe.ArtistPartIndex = fe.SearchPartIndex);
                return x;
            }

            string[] verbs = new string[] { "cover" };
            string[] artistCombos = new string[] { "&" };

            public List<string> GetTrackSplitStrings(string filename)
            {
                List<SearchAnalysisRule> searchNames = new List<SearchAnalysisRule>();
                string s = filename;

                for (int i = 0; i < bracketsOpen.Count(); i++)
                {
                    if (s.Contains(bracketsOpen[i]))
                    {
                        int iStart = s.IndexOf(bracketsOpen[i]);
                        int iEnd = s.LastIndexOf(bracketsClose[i]);

                        string pre = s.Substring(0, iStart).Trim();
                        string post = s.Substring(iEnd + 1).Trim();

                        string bracketContents = s.Substring(iStart, iEnd - iStart + 1);

                        // override filename with removed brackets
                        s = pre + "|" + bracketContents + "|" + post;
                    }
                }

                foreach (char d in delims)
                {
                    if (s.Contains(d))
                    {
                        s = s.Replace(d, '|');
                    }
                }

                foreach (var v in verbs)
                {
                    if (s.ToLower().Contains(v))
                    {

                        int iStart = s.ToLower().IndexOf(v);
                        int iEnd = s.ToLower().LastIndexOf(v);

                        string pre = s.Substring(0, iStart).Trim();
                        string post = s.Substring(iEnd + v.Length).Trim();

                        string verbContents = s.Substring(iStart, iEnd - iStart + v.Length);

                        // override filename with removed brackets
                        s = pre + "|" + verbContents + "|" + post;
                    }
                }


                foreach (var ac in artistCombos)
                {
                    if (s.Contains(ac))
                    {
                        s = s.Replace(ac, "|" + ac + "|");
                    }
                }

                string tracknumber = "";
                int iTrackNumber;
                if (int.TryParse(s.Split(' ').First(), out iTrackNumber))
                {
                    tracknumber = s.Split(' ').First();
                    string notrack = s.Substring(tracknumber.Length);
                    s = tracknumber + "|" + notrack;
                }

                return s.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries).ToList();
            }
            public List<SearchAnalysisRule> GetTrackSearchStrings(string filename, bool skipTrackStart = false, bool hasArtist = false, string artistName = "", SearchAnalysisRule PrimaryAnalysisRule = null)
            {
                List<SearchAnalysisRule> searchNames = new List<SearchAnalysisRule>();

                // 1985 (Bowling for Soup) - Jonathan Young & Travis Carte COVER
                // S (OA) - A & A V
                // S |OA| | A | A |V
                // Aerosmith - Girls Of Summer - YouTube
                // A - S - X


                string artistNameInject = "";
                if (hasArtist)
                {
                    if (artistName == "")
                        artistNameInject = PrimaryAnalysisRule.ArtistName + " " + PrimaryAnalysisRule.Delimiter + " ";
                    else
                        artistNameInject = artistName + " - ";
                }

                string s = filename;
                List<string> bracketContents = new List<string>();

                for (int i = 0; i < bracketsOpen.Count(); i++)
                {
                    if (filename.Contains(bracketsOpen[i]))
                    {
                        int iStart = filename.IndexOf(bracketsOpen[i]);
                        int iEnd = filename.LastIndexOf(bracketsClose[i]);
                        if (iEnd == -1)
                        {
                            bracketContents.Add(filename.Substring(iStart));
                            s = filename.Remove(iStart).Trim();
                        }
                        else
                        {
                            bracketContents.Add(filename.Substring(iStart, iEnd - iStart + 1));
                            // override filename with removed brackets
                            s = filename.Remove(iStart, iEnd - iStart + 1).Trim();
                        }
                    }
                }

                // Option 1
                // Split if delims are present
                // SearchAnalysisRule - The 'search rule template' {artist name} - {track name} - youtube.mp4
                foreach (char d in delims)
                {
                    if (s.Contains(d))
                    {
                        int partindex = 0;
                        foreach (string sPart in s.Split(new char[] { d }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            if (!string.IsNullOrEmpty(sPart.Trim()))
                            {
                                string cleanstring = sPart.ToLower();
                                if (KeepCapitalization)
                                    cleanstring = sPart;

                                int possibleTrackNo = -1;
                                bool isPossibleTrackNumber = false;
                                if (int.TryParse(cleanstring, out possibleTrackNo))
                                    if (possibleTrackNo < 1000)
                                        isPossibleTrackNumber = true;

                                //List<WebContentsImage> Images = await db.WebContentsImages
                                //.Where(t => t.Tags.Any(queryTags.Contains))
                                //.ToListAsync()
                                //.ConfigureAwait(false);
                                foreach (var v in verbs)
                                    if (cleanstring.Contains(v))
                                    {
                                        //StringSmartTrim();

                                        //string sentence = "This is a sentence with multiple    spaces";
                                        //RegexOptions options = RegexOptions.None;
                                        //Regex regex = new Regex("[ ]{2,}", options);
                                        //sentence = regex.Replace(sentence, " ");

                                        cleanstring = cleanstring.Replace(v, "").Trim();
                                    }

                                if (hasArtist)
                                {
                                    // skips part if this part is the artist name
                                    // TODO: cleanstring?????
                                    if (PrimaryAnalysisRule.ArtistPartIndex != partindex)
                                        searchNames.Add(new SearchAnalysisRule()
                                        {
                                            SearchTerm = artistNameInject + sPart.Trim(),
                                            TrackTerm = sPart.Trim(),
                                            Delimiter = d.ToString(),
                                            SearchPartIndex = partindex,
                                            ArtistPartIndex = PrimaryAnalysisRule.ArtistPartIndex,
                                            ArtistName = PrimaryAnalysisRule.ArtistName,
                                            ArtistSearchTerm = PrimaryAnalysisRule.ArtistSearchTerm,
                                            isPossibleTrackNumber = isPossibleTrackNumber
                                        });
                                }
                                else
                                    searchNames.Add(new SearchAnalysisRule() { SearchTerm = cleanstring.Trim(), Delimiter = d.ToString(), SearchPartIndex = partindex, isPossibleTrackNumber = isPossibleTrackNumber });

                            }
                            partindex++;
                        }
                    }
                }


                // Option 2
                // No delims?
                // Try using the start number as track number
                string s_notrack = s;
                if (!skipTrackStart)
                {
                    string tracknumber = "";
                    int iTrackNumber;
                    if (int.TryParse(s.Split(' ').First(), out iTrackNumber))
                        if (iTrackNumber < 1000)
                        {
                            tracknumber = s.Split(' ').First();
                            s_notrack = s.Substring(tracknumber.Length).Trim();
                            searchNames.Add(new SearchAnalysisRule() { SearchTerm = artistNameInject + s_notrack, TrackTerm = s_notrack, TrackNumber = iTrackNumber, isPossibleTrackNumber = true });
                        }
                }


                // Option 3
                // song name as is
                if (searchNames.Count == 0)
                    searchNames.Add(new SearchAnalysisRule(artistNameInject, s));


                return searchNames;
            }
        }




        #endregion File Analysis

        List<string> MusicFilePaths = new List<string>();
        public void LoadDirlisting(string TargetDir, bool includeFolders = false)
        {
            try
            {
                SearchOption s = SearchOption.TopDirectoryOnly;
                if (includeFolders)
                    s = SearchOption.AllDirectories;

                var m = Directory.EnumerateFiles(TargetDir, "*", s).ToList();
                LoadDirFiles(m);
            }
            catch (Exception ex)
            {

            }

        }

        /// <summary>
        /// Creates and Populates ImportList with files from DIR
        /// </summary>
        /// <param name="TargetDir"></param>
        /// <param name="includeFolders"></param>
        public void LoadDirFiles(List<string> musicFilePaths)
        {
            ImportList.Clear();
            MatchedFiles.Clear();
            UnmatchedFiles.Clear();

            using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
            {
                foreach (string musicFilePath in musicFilePaths)
                {
                    ManagedFile m = new ManagedFile();
                    string cleanFilePath = musicFilePath.Replace("\\\\", "\\");

                    // 1. Check DB Entry
                    if (db.TrackLibraries.Any(a => a.FilePath == cleanFilePath))
                        m.isDatabase = true;

                    if (Global.karaokeformats.Contains(System.IO.Path.GetExtension(cleanFilePath).ToLower()))
                    {
                        m.isKaraoke = true;
                        CheckKaraokePair(cleanFilePath, m);
                    }
                    else if (Global.musicformats.Contains(System.IO.Path.GetExtension(cleanFilePath).ToLower()))
                    {
                        if (hasID3v1(cleanFilePath, ref m))
                        {
                            m.hasId3 = true;

                        }
                        if (hasID3v2(cleanFilePath, ref m))
                        {
                            m.hasId3 = true;
                        }

                        // all mp3s go into unmatched pile
                        m.isMusic = true;
                        CheckKaraokePair(cleanFilePath, m);
                    }
                    else if (Global.videoformats.Contains(System.IO.Path.GetExtension(cleanFilePath).ToLower()))
                    {
                        m.isVideo = true;
                        m.setFileName(cleanFilePath);
                        ImportList.Add(m);
                    }
                }

                // adds a single entry for the pair of karaoke files
                ImportList.AddRange(MatchedFiles);

                // no karaoke files, dump remainder mp3s in import list
                // dumps incomplete karaoke files
                ImportList.AddRange(UnmatchedFiles.Where(w => w.isMusic == true));
            }

            // filters to ensure pure ID3 Tags
            // SortFilterImportList();
        }

        public void AutoDBSaveMusic()
        {
            var list = from l in ImportList
                       where l.hasId3 == true && l.isMusic == true && l.isDatabase == false
                       select l;

            using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
            {
                foreach (var track in list)
                {
                    if (track.isImportReady)
                    {
                        ImportMusicFile(track, db);
                    }
                }
            }
        }

        public bool ImportAllKaraokeFiles(bool copyToDefaultFolder = false, string playlistname = "")
        {
            bool isBusyLoading = true;

            var list = (from l in ImportList
                        where l.isKaraoke == true && l.isDatabase == false
                        select l).ToList();

            if (list.Any())
            {
                ImportQue.Import(list.ToList(), playlistname, copyToDefaultFolder);

                foreach (var m in list)
                    ImportList.Remove(m);
            }


            if (list.Count() == 0)
            {
                //Global.mPlayer.NowPlaying.ShowNotification(String.Format("Warning! Didn't add any tracks to Library", list.Count()));
                isBusyLoading = false;
            }
            //else if (list.Count() == 1)
            //Global.mPlayer.NowPlaying.ShowNotification(String.Format("Added {0} track to Library", list.Count()));
            //if (list.Count() > 1)
            //Global.mPlayer.NowPlaying.ShowNotification(String.Format("Added {0} tracks to Library", list.Count()));

            return isBusyLoading;

        }

        internal ManagedFileQue ImportQue = new ManagedFileQue();

        internal class ManagedFileQue : List<ManagedFile>
        {
            private QueOrderBatch Limbo = new QueOrderBatch();
            internal List<QueOrderBatch> ImportQue = new List<QueOrderBatch>();

            public bool isLimbo { get { return Limbo.managedFiles.Count > 0; } }


            internal void Import(List<ManagedFile> managedFiles, string playlistname, bool copyToDefaultFolder)
            {
                if (isLimbo)
                {
                    ImportQue.Add(new QueOrderBatch() { managedFiles = managedFiles, playlistname = playlistname, copyToDefaultFolder = copyToDefaultFolder });
                    Global.mPlayer.NowPlaying.ShowNotification("Added songs to the import que");
                }
                else
                {
                    Limbo = new QueOrderBatch() { managedFiles = managedFiles, playlistname = playlistname, copyToDefaultFolder = copyToDefaultFolder };

                    iTunesSearchManager iManager = new iTunesSearchManager();
                    //Global.ImportAnalytics.NewBatch( list.Select(s => s.FilePath), "Music", DateTime.Now.ToShortDateString, DateTime.Now.ToShortTimeString);

                    int analyticBatchId = Global.ImportAnalytics.NewBatch(managedFiles.Count(), managedFiles.Count(c => c.isMusic), managedFiles.Count(c => c.isVideo), managedFiles.Count(c => c.isKaraoke), playlistname);

                    if (managedFiles.Count() == 1)
                        Global.mPlayer.NowPlaying.ShowNotification(String.Format("Importing {0} track to Library", managedFiles.Count()));
                    if (managedFiles.Count() > 1)
                        Global.mPlayer.NowPlaying.ShowNotification(String.Format("Importing {0} tracks to Library", managedFiles.Count()));

                    iManager.ImportTreadedTracksV2(managedFiles, playlistname, copyToDefaultFolder, analyticBatchId);
                }
                BufferDirectories = new List<string>();
            }

            internal void FinishedBatch()
            {
                // Limbo Batch is done, bump next batch up and repeat
                Limbo = new QueOrderBatch();

                if (ImportQue.Any())
                {
                    var newLimbo = ImportQue.First();
                    Limbo = newLimbo;

                    ImportQue.RemoveAt(0);

                    iTunesSearchManager iManager = new iTunesSearchManager();
                    int analyticBatchId = Global.ImportAnalytics.NewBatch(Limbo.managedFiles.Count(), Limbo.managedFiles.Count(c => c.isMusic), Limbo.managedFiles.Count(c => c.isVideo), Limbo.managedFiles.Count(c => c.isKaraoke), Limbo.playlistname);

                    if (Limbo.managedFiles.Count() == 1)
                        Global.mPlayer.NowPlaying.ShowNotification(String.Format("Importing {0} track to Library", Limbo.managedFiles.Count()));
                    if (Limbo.managedFiles.Count() > 1)
                        Global.mPlayer.NowPlaying.ShowNotification(String.Format("Importing {0} tracks to Library", Limbo.managedFiles.Count()));

                    iManager.ImportTreadedTracksV2(Limbo.managedFiles, Limbo.playlistname, Limbo.copyToDefaultFolder, analyticBatchId);
                }
                else
                {
                    // import done, reload view buffers
                    using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                    {
                        //db.Database.ExecuteSqlCommand("EXECUTE sp_refreshview @viewname = 'LibraryView'");
                        db.Database.ExecuteSqlCommand("exec BuildLibraryView");
                        Global.ViewLoaded = true;
                    }
                }

                BufferDirectories = new List<string>();
            }

            private List<string> BufferDirectories = new List<string>();
            internal bool ContainsDir(string s)
            {
                if (!BufferDirectories.Any())
                {
                    if (Limbo.managedFiles.Any())
                    {
                        var xA = from a in Limbo.managedFiles
                                 select a.FilePath;

                        BufferDirectories.AddRange(xA);
                    }

                    if (ImportQue.Any())
                    {
                        foreach (var i in ImportQue)
                        {
                            var xB = from a in i.managedFiles
                                     select a.FilePath;

                            BufferDirectories.AddRange(xB);
                        }
                    }
                }

                return BufferDirectories.Contains(s);
            }
        }

        internal class QueOrderBatch
        {
            internal List<ManagedFile> managedFiles = new List<ManagedFile>();
            internal string playlistname = "";
            internal bool copyToDefaultFolder = false;
        }

        public bool ImportAllMusicFiles(bool copyToDefaultFolder = false, string playlistname = "")
        {
            bool isBusyLoading = true;
            try
            {
                // if in db, then add to playlist
                var list = (from l in ImportList
                            where l.isMusic == true && l.isDatabase == false
                            select l).ToList();


                if (list.Any())
                {
                    ImportQue.Import(list.ToList(), playlistname, copyToDefaultFolder);

                    foreach (var m in list)
                        ImportList.Remove(m);
                }


                if (list.Count() == 0)
                {
                    Global.mPlayer.NowPlaying.ShowNotification(String.Format("Warning! Didn't add any tracks to Library", list.Count()));
                    Thread.Sleep(1000);
                    Global.MainWindow.lblNowPlaying.Dispatcher.Invoke(new Action(() => { Global.MainWindow.lblNowPlaying.Visibility = System.Windows.Visibility.Collapsed; }));
                    isBusyLoading = false;
                }
                else if (list.Count() == 1)
                {
                    Global.mPlayer.NowPlaying.ShowNotification(String.Format("Added {0} track to Library", list.Count()));
                    Thread.Sleep(1000);
                    Global.MainWindow.lblNowPlaying.Dispatcher.Invoke(new Action(() => { Global.MainWindow.lblNowPlaying.Visibility = System.Windows.Visibility.Collapsed; }));
                }
                if (list.Count() > 1)
                {
                    Global.mPlayer.NowPlaying.ShowNotification(String.Format("Added {0} tracks to Library", list.Count()));
                    Thread.Sleep(1000);
                    Global.MainWindow.lblNowPlaying.Dispatcher.Invoke(new Action(() => { Global.MainWindow.lblNowPlaying.Visibility = System.Windows.Visibility.Collapsed; }));
                }

            }
            catch (Exception)
            {

            }



            return isBusyLoading;
        }


        public bool ResumeImportKaraokeFiles()
        {
            bool isBusyLoading = true;
            List<ManagedFile> list = new List<ManagedFile>();

            using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
            {
                var listA = (from a in db.TrackLibraries
                             where a.Type == "Import - Karaoke"
                             select a).ToList();

                foreach (var b in listA)
                {
                    list.Add(new ManagedFile(b));
                }
            }

            if (list.Any())
            {
                iTunesSearchManager iManager = new iTunesSearchManager();
                iManager.ImportTreadedTracksV2(list);
            }

            if (list.Count() == 0)
            {
                Global.mPlayer.NowPlaying.ShowNotification(String.Format("Warning! Didn't add any Karaoke to Library", list.Count()));
                isBusyLoading = false;
                Thread.Sleep(1000);
                Global.MainWindow.lblNowPlaying.Dispatcher.Invoke(new Action(() => { Global.MainWindow.lblNowPlaying.Visibility = System.Windows.Visibility.Collapsed; }));
            }

            return isBusyLoading;
        }
        public bool ResumeImportMusicFiles()
        {
            bool isBusyLoading = true;
            List<ManagedFile> list = new List<ManagedFile>();

            using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
            {
                var listA = (from a in db.TrackLibraries
                             where a.Type == "Import - Music"
                             select a).ToList();

                foreach (var b in listA)
                {
                    list.Add(new ManagedFile(b));
                }
            }

            if (list.Any())
            {
                iTunesSearchManager iManager = new iTunesSearchManager();
                iManager.ImportTreadedTracksV2(list);
            }

            if (list.Count() == 0)
            {
                Global.mPlayer.NowPlaying.ShowNotification(String.Format("Warning! Didn't add any Music to Library", list.Count()));
                isBusyLoading = false;
                Thread.Sleep(1000);
                Global.MainWindow.lblNowPlaying.Dispatcher.Invoke(new Action(() => { Global.MainWindow.lblNowPlaying.Visibility = System.Windows.Visibility.Collapsed; }));
            }

            return isBusyLoading;
        }
        public bool ResumeImportVideoFiles()
        {
            bool isBusyLoading = true;
            List<ManagedFile> list = new List<ManagedFile>();

            using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
            {
                var listA = (from a in db.TrackLibraries
                             where a.Type == "Import - Video"
                             select a).ToList();

                foreach (var b in listA)
                {
                    list.Add(new ManagedFile(b));
                }
            }

            if (list.Any())
            {
                iTunesSearchManager iManager = new iTunesSearchManager();
                iManager.ImportTreadedTracksV2(list);
            }

            if (list.Count() == 0)
            {
                Global.mPlayer.NowPlaying.ShowNotification(String.Format("Warning! Didn't add any videos to Library", list.Count()));
                isBusyLoading = false;
                Thread.Sleep(1000);
                Global.MainWindow.lblNowPlaying.Dispatcher.Invoke(new Action(() => { Global.MainWindow.lblNowPlaying.Visibility = System.Windows.Visibility.Collapsed; }));
            }

            return isBusyLoading;
        }

        public bool ImportAllVideoFiles(bool copyToDefaultFolder = false, string playlistname = "")
        {
            bool isBusyLoading = true;

            // if in db, then add to playlist
            //var list = (from l in ImportList
            //            where l.isVideo == true && l.isDatabase == false
            //            select l).ToList();

            // if in db, then add to playlist
            var list = (from l in ImportList
                        where l.isVideo == true && l.isDatabase == false
                        select l).ToList();

            if (list.Any())
            {

                ImportQue.Import(list.ToList(), playlistname, copyToDefaultFolder);

                foreach (var m in list)
                    ImportList.Remove(m);
            }

            if (list.Count() == 0)
            {
                Global.mPlayer.NowPlaying.ShowNotification(String.Format("Warning! Didn't add any videos to Library", list.Count()));
                isBusyLoading = false;
                Thread.Sleep(1000);
                Global.MainWindow.lblNowPlaying.Dispatcher.Invoke(new Action(() => { Global.MainWindow.lblNowPlaying.Visibility = System.Windows.Visibility.Collapsed; }));
            }
            else if (list.Count() == 1)
            {
                Global.mPlayer.NowPlaying.ShowNotification(String.Format("Added {0} video to Library", list.Count()));
                Thread.Sleep(1000);
                Global.MainWindow.lblNowPlaying.Dispatcher.Invoke(new Action(() => { Global.MainWindow.lblNowPlaying.Visibility = System.Windows.Visibility.Collapsed; }));
            }
            if (list.Count() > 1)
            {
                Global.mPlayer.NowPlaying.ShowNotification(String.Format("Added {0} videos to Library", list.Count()));
                Thread.Sleep(1000);
                Global.MainWindow.lblNowPlaying.Dispatcher.Invoke(new Action(() => { Global.MainWindow.lblNowPlaying.Visibility = System.Windows.Visibility.Collapsed; }));
            }


            return isBusyLoading;
        }

        public void ImportAddToPlaylistOnly(string playlistname = "")
        {
            var list = from l in ImportList
                       where l.isDatabase == true
                       select l;

            using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
            {
                foreach (var track in list)
                {
                    ImportAddToPlaylistOnly(track, db, playlistname);
                }
            }

            if (list.Count() == 0)
            {
                Global.mPlayer.NowPlaying.ShowNotification(String.Format("Warning! Didn't add any videos to Library", list.Count()));
                Thread.Sleep(1000);
                Global.MainWindow.lblNowPlaying.Dispatcher.Invoke(new Action(() => { Global.MainWindow.lblNowPlaying.Visibility = System.Windows.Visibility.Collapsed; }));
            }
            else if (list.Count() == 1)
            {
                Global.mPlayer.NowPlaying.ShowNotification(String.Format("Added {0} video to Library", list.Count()));
                Thread.Sleep(1000);
                Global.MainWindow.lblNowPlaying.Dispatcher.Invoke(new Action(() => { Global.MainWindow.lblNowPlaying.Visibility = System.Windows.Visibility.Collapsed; }));
            }
            if (list.Count() > 1)
            {
                Global.mPlayer.NowPlaying.ShowNotification(String.Format("Added {0} videos to Library", list.Count()));
                Thread.Sleep(1000);
                Global.MainWindow.lblNowPlaying.Dispatcher.Invoke(new Action(() => { Global.MainWindow.lblNowPlaying.Visibility = System.Windows.Visibility.Collapsed; }));
            }

        }

        internal void ImportAddToPlaylistOnly(ImportFactory.ManagedFile mf, JukeboxBrainsDBEntities db, string playlistname = "")
        {
            TrackLibrary tl = db.TrackLibraries.Where(w => w.FilePath == mf.FilePath).First();

            if (!string.IsNullOrEmpty(playlistname))
            {
                int sequencenumber = 0;
                if (db.PlayListDetails.Any(a => a.Name == playlistname))
                {
                    // use existing playlist
                    PlayListDetail pld = db.PlayListDetails.Where(w => w.Name == playlistname).First();
                    sequencenumber = pld.Playlists.Count();

                    tl.Playlists.Add(new Playlist() { SequenceNumber = sequencenumber, PlayListDetail = pld });
                }
                else
                {
                    // create new playlist
                    tl.Playlists.Add(new Playlist() { SequenceNumber = sequencenumber, PlayListDetail = new PlayListDetail() { Name = playlistname, Type = 1 } });
                }
            }

            db.Entry(tl).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();
        }


        internal void ImportDiryFile(ImportFactory.ManagedFile mf, JukeboxBrainsDBEntities db, string playlistname = "")
        {
            SongLibrary sl = new SongLibrary();

            // check if has artist
            if (mf.searchArtist.Count > 0)
            {
                foreach (var art in mf.searchArtist)
                {
                    Artist a = new Artist();

                    if (db.Artists.Any(q1 => q1.ArtistName.Equals(art, StringComparison.OrdinalIgnoreCase)))
                    {
                        a = db.Artists.Where(q1 => q1.ArtistName.Equals(art, StringComparison.OrdinalIgnoreCase)).First();
                    }
                    else
                    {
                        a.ArtistName = art;
                    }

                    sl.Artists.Add(a);
                }
            }

            // check if has albums
            if (!string.IsNullOrEmpty(mf.searchAlbum))
            {
                AlbumLibrary al = new AlbumLibrary();
                if (db.AlbumLibraries.Any(q2 => q2.AlbumName.Equals(mf.searchAlbum, StringComparison.OrdinalIgnoreCase)))
                {
                    al = db.AlbumLibraries.Where(q2 => q2.AlbumName.Equals(mf.searchAlbum, StringComparison.OrdinalIgnoreCase)).First();
                }
                else
                {
                    al.AlbumName = mf.searchAlbum;
                    //if(!string.IsNullOrEmpty(mf.Id3Tag.Genre.Value))
                    if (mf.Id3Tag.Year != null) al.Year = mf.Id3Tag.Year.Value ?? null;
                    if (mf.Id3Tag.Pictures.Count() > 0) al.CoverArt = mf.Id3Tag.Pictures.First().PictureData;
                }
                sl.AlbumLibraries.Add(al);
            }

            TrackLibrary tl = new TrackLibrary();
            tl.FilePath = mf.FilePath;
            tl.FileName = mf.FileName;
            tl.Extention = mf.FileExtention;

            if (mf.isKaraoke)
                tl.Type = "Karaoke";
            else if (mf.isVideo)
                tl.Type = "Video";
            else if (mf.isMusic)
                tl.Type = "Music";

            if (!string.IsNullOrEmpty(playlistname))
            {
                int sequencenumber = 0;
                if (db.PlayListDetails.Any(a => a.Name == playlistname))
                {
                    // use existing playlist
                    PlayListDetail pld = db.PlayListDetails.Where(w => w.Name == playlistname).First();
                    sequencenumber = pld.Playlists.Count();

                    tl.Playlists.Add(new Playlist() { SequenceNumber = sequencenumber, PlayListDetail = pld });
                }
                else
                {
                    // create new playlist
                    tl.Playlists.Add(new Playlist() { SequenceNumber = sequencenumber, PlayListDetail = new PlayListDetail() { Name = playlistname, Type = 1 } });
                }
            }

            sl.TrackLibraries.Add(tl);

            if (string.IsNullOrEmpty(mf.searchTrack))
                sl.SongName = mf.FileName;
            else
                sl.SongName = mf.searchTrack;

            //if (mf.Id3Tag.Genre != null ) sl.Genre = mf.Id3Tag.Genre;
            db.SongLibraries.Add(sl);
            db.SaveChanges();
            Debug.WriteLine("Added new Record");
        }

        internal void ImportVideoFile(string filepath)
        {

        }

        /// <summary>
        /// Imports Video File and Searches Details on iTunes
        /// </summary>
        /// <param name="mf"></param>
        internal void ImportVideoFile(ImportFactory.ManagedFile mf)
        {
            // try to find artist first
            int songid;
            SongLibrary baseSong;
            // HACK!: just save details to db, then run update!
            using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
            {
                TrackLibrary track = new TrackLibrary()
                {
                    Extention = mf.FileExtention,
                    FileName = mf.FileName,
                    FilePath = mf.FilePath,
                    Type = "video"
                };

                SongLibrary song = new SongLibrary()
                {
                    SongName = mf.FileName
                };

                track.SongLibraries.Add(song);
                db.TrackLibraries.Add(track);
                db.SaveChanges();

                songid = song.SongId;
                baseSong = song;
            }

            iTunesSearchManager iManager = new iTunesSearchManager();
            iManager.UpdateTrack2(baseSong);
        }

        internal void ImportMusicFile(ImportFactory.ManagedFile mf, JukeboxBrainsDBEntities db)
        {
            // has all 3
            SongLibrary sl = new SongLibrary();
            foreach (var art in mf.searchArtist)
            {
                Artist a = new Artist();

                if (db.Artists.Any(q1 => q1.ArtistName.Equals(art, StringComparison.OrdinalIgnoreCase)))
                {
                    a = db.Artists.Where(q1 => q1.ArtistName.Equals(art, StringComparison.OrdinalIgnoreCase)).First();
                }
                else
                {
                    a.ArtistName = art;
                }

                sl.Artists.Add(a);
            }

            AlbumLibrary al = new AlbumLibrary();

            if (db.AlbumLibraries.Any(q2 => q2.AlbumName.Equals(mf.searchAlbum, StringComparison.OrdinalIgnoreCase)))
            {
                al = db.AlbumLibraries.Where(q2 => q2.AlbumName.Equals(mf.searchAlbum, StringComparison.OrdinalIgnoreCase)).First();
            }
            else
            {
                al.AlbumName = mf.searchAlbum;
                //if(!string.IsNullOrEmpty(mf.Id3Tag.Genre.Value))
                if (mf.Id3Tag.Year != null) al.Year = mf.Id3Tag.Year.Value ?? null;
                if (mf.Id3Tag.Pictures.Count() > 0) al.CoverArt = mf.Id3Tag.Pictures.First().PictureData;
            }

            TrackLibrary tl = new TrackLibrary();
            tl.FilePath = mf.FilePath;
            tl.FileName = mf.FileName;
            tl.Extention = mf.FileExtention;
            tl.Type = "Music";

            sl.AlbumLibraries.Add(al);
            sl.TrackLibraries.Add(tl);

            sl.SongName = mf.searchTrack;
            //if (mf.Id3Tag.Genre != null ) sl.Genre = mf.Id3Tag.Genre;
            db.SongLibraries.Add(sl);
            db.SaveChanges();
            Debug.WriteLine("Added new Record");
        }

        public void SortFilterImportList()
        {
            // string album = (from a in ImportList where a.isId3 == true && a.isMusic == true && a.searchAlbum != null select a.searchAlbum).First();
            //List<ManagedFile> newList = (from a in ImportList where a.isId3 == true && a.isMusic == true select a).OrderBy(i => i.searchAlbum).ThenBy(i2 => i2.searchAlbumTrackPosition).ToList();

            List<ManagedFile> newList = (from a in ImportList where a.hasId3 == true && a.isMusic == true select a).OrderBy(i => i.FolderPath).ToList();

            ImportList = newList;
        }

        internal bool hasID3v1(string filepath, ref ManagedFile m)
        {
            using (var mp3 = new Mp3(filepath))
            {
                foreach (Id3Version iv in mp3.AvailableTagVersions)
                {
                    if (iv == Id3Version.V1X)
                    {
                        try
                        {
                            Id3Tag tag = mp3.GetTag(Id3TagFamily.Version1X);
                            m.Id3Tag = tag;
                            m.CheckImportStatus();
                        }
                        catch
                        {
                            return false;
                        }
                        return true;
                    }
                }
            }
            return false;
        }

        internal bool hasID3v2(string filepath, ref ManagedFile m)
        {
            using (var mp3 = new Mp3(filepath))
            {
                foreach (Id3Version iv in mp3.AvailableTagVersions)
                {
                    if (iv == Id3Version.V23)
                    {
                        try
                        {
                            Id3Tag tag = mp3.GetTag(Id3TagFamily.Version2X);
                            m.Id3Tag = tag;
                            m.CheckImportStatus();
                        }
                        catch
                        {
                            return false;
                        }
                        return true;
                    }
                }
            }
            return false;
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
                k2.Id3Tag = pullId3Tag(dir);
                MatchedFiles.Add(k2);
            }
            else
            {
                k2.setKaraokeDir(dir);
                k2.CloneValues(m);
                k2.Id3Tag = pullId3Tag(dir);
                UnmatchedFiles.Add(k2);
            }
        }

        private Id3Tag pullId3Tag(string s)
        {
            Id3Tag buffertag = null;
            using (var mp3 = new Mp3(s))
            {
                foreach (Id3Version iv in mp3.AvailableTagVersions)
                {
                    if (iv == Id3Version.V1X)
                    {
                        try
                        {
                            buffertag = mp3.GetTag(Id3TagFamily.Version1X);
                        }
                        catch
                        {
                            buffertag = null;
                        }
                    }
                    if (iv == Id3Version.V23)
                    {
                        try
                        {
                            return mp3.GetTag(Id3TagFamily.Version2X);
                        }
                        catch
                        {
                            return buffertag;
                        }
                    }
                }
            }
            return buffertag;
        }

        public enum ManagedFileStatus
        {
            Importing,
            itunesImported,
            AlbumSongMismatch,
            ImportError
        }
        public class ManagedFile
        {
            public TrackLibrary BaseDBTrackLibrary { get; set; }
            public bool hasDBTrackLibrary = false;
            public int ImportPlaylistId = -1;
            public bool ImportToPlaylist { get { return (ImportPlaylistId != null && ImportPlaylistId > 0); } }

            public string FileName { get; set; }
            public string FileExtention { get; set; }
            public string FullFileName { get { return FileName + FileExtention; } }
            public string FolderPath { get; set; }
            public string FolderName { get; set; }

            public string FilePath { get; set; }
            public string SongMp3Path { get; set; }

            /// <summary>
            /// Checks if the ID3 Tag actually contains any data
            /// </summary>
            public bool hasValidId3
            {
                get
                {
                    if (hasId3 && Id3Tag == null)
                        return false;
                    else if (hasId3)
                    {
                        if (Id3Tag.Album == null || String.IsNullOrEmpty(Id3Tag.Album.Value)) return false;
                        else if (Id3Tag.Artists == null || Id3Tag.Artists.Value.Count() == 0) return false;
                        else if (Id3Tag.Title == null || String.IsNullOrEmpty(Id3Tag.Title.Value)) return false;
                        else return true;
                    }
                    else
                        return false;
                }
            }
            public bool hasId3 = false;
            public bool isDatabase = false;
            public bool isKaraoke = false;
            public bool isMusic = false;
            public bool isVideo = false;

            public bool isImportReady = false;
            public bool isiTunesReady = false;
            public iTunesSearch.Library.Models.Album iTunesAlbum { get; set; }
            public iTunesSearch.Library.Models.Song iTunesSong { get; set; }

            /// <summary>
            /// Pulls Data From ID3 Tag
            /// </summary>
            public void CheckImportStatus()
            {
                bool isReady = true;
                // Album
                if (string.IsNullOrEmpty(Id3Tag.Album.Value))
                    isReady = false;
                else
                    searchAlbum = Id3Tag.Album.Value;

                // Track
                if (string.IsNullOrEmpty(Id3Tag.Album.Value))
                    isReady = false;
                else
                    searchTrack = Id3Tag.Title.Value;

                // Artist(s)
                if (Id3Tag.Artists != null && Id3Tag.Artists.Value.Count > 0)
                {
                    searchArtistCombined = "";
                    foreach (var a in Id3Tag.Artists.Value)
                    {
                        searchArtist.Add(a);
                        searchArtistCombined += ", " + a;
                    }
                    searchArtistCombined = searchArtistCombined.Remove(0, 2);
                }
                else
                {
                    searchArtist = new List<string>();
                    isReady = false;
                }

                // TrackNumbers
                searchAlbumTrackCount = Id3Tag.Track.TrackCount;
                searchAlbumTrackPosition = Id3Tag.Track.Value;

                isImportReady = isReady;
            }

            public List<string> searchArtist { get; set; }
            public string searchArtistCombined { get; set; }
            public string searchTrack { get; set; }
            public string searchAlbum { get; set; }
            public int searchAlbumTrackCount { get; set; }
            public int searchAlbumTrackPosition { get; set; }

            public Id3Tag Id3Tag { get; set; }
            public string SetTypeString
            {
                set
                {
                    if (value.Contains("Karaoke"))
                        isKaraoke = true;
                    else if (value.Contains("Video"))
                        isVideo = true;
                    else if (value.Contains("Music"))
                        isMusic = true;
                    else
                    {
                        string thisiswrong = "";
                    }
                }
            }
            public string ImportTypeString
            {
                get
                {
                    if (isKaraoke)
                        return "Karaoke";
                    else if (isVideo)
                        return "Video";
                    else if (isMusic)
                        return "Music";
                    else return "";
                }
            }

            public ManagedFileStatus Status { get; internal set; }

            public bool isCompleteDB = false;
            /// <summary>
            /// Set to True by default, set to False after NO ID3 Online Update
            /// </summary>
            internal bool NotBeenUpdated = true;

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
                searchArtist = new List<string>();
                searchAlbumTrackCount = -1;
                searchAlbumTrackPosition = -1;
            }

            public ManagedFile(string dir)
            {
                searchArtist = new List<string>();
                searchAlbumTrackCount = -1;
                searchAlbumTrackPosition = -1;
                setFileName(dir);
            }

            public ManagedFile(TrackLibrary track)
            {
                searchArtist = new List<string>();
                searchAlbumTrackCount = -1;
                searchAlbumTrackPosition = -1;
                setFileName(track.FilePath);

                BaseDBTrackLibrary = track;
                hasDBTrackLibrary = true;
                SetTypeString = track.Type;

                using (var mp3 = new Mp3(track.FilePath))
                {
                    foreach (Id3Version iv in mp3.AvailableTagVersions)
                    {
                        if (iv == Id3Version.V1X)
                        {
                            try
                            {
                                Id3Tag tag = mp3.GetTag(Id3TagFamily.Version1X);
                                this.Id3Tag = tag;
                                this.CheckImportStatus();
                                hasId3 = true;
                            }
                            catch
                            {
                                hasId3 = false;
                            }
                        }
                        if (iv == Id3Version.V23)
                        {
                            try
                            {
                                Id3Tag tag = mp3.GetTag(Id3TagFamily.Version2X);
                                this.Id3Tag = tag;
                                this.CheckImportStatus();
                                hasId3 = true;
                            }
                            catch
                            {
                                hasId3 = false;
                            }
                        }
                    }
                }
            }

            public void setFileName(string dir)
            {
                FileName = System.IO.Path.GetFileNameWithoutExtension(dir);
                FileExtention = System.IO.Path.GetExtension(dir);
                FilePath = dir;
                FolderPath = System.IO.Path.GetDirectoryName(dir);
                FolderName = FolderPath.Split('\\').Last();
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
                if (m.hasId3)
                    hasId3 = m.hasId3;
                if (m.isDatabase)
                    isDatabase = m.isDatabase;
                if (m.isMusic && !isKaraoke)
                    isMusic = m.isMusic;
                if (m.isKaraoke)
                {
                    isKaraoke = m.isKaraoke;
                    isMusic = false;
                }

                if (m.searchArtist.Count > 0)
                    searchArtist = m.searchArtist;
                searchTrack = string.IsNullOrEmpty(m.searchTrack) ? searchTrack : m.searchTrack;
                searchAlbum = string.IsNullOrEmpty(m.searchAlbum) ? searchAlbum : m.searchAlbum;
                searchAlbumTrackCount = m.searchAlbumTrackCount == -1 ? searchAlbumTrackCount : m.searchAlbumTrackCount;
                searchAlbumTrackPosition = m.searchAlbumTrackPosition == -1 ? searchAlbumTrackPosition : m.searchAlbumTrackPosition;
                if (m.isImportReady)
                    isImportReady = m.isImportReady;
            }

            internal void setId3Tag(Id3Tag i3)
            {
                Id3Tag = i3;
                hasId3 = true;
            }

            internal void ResetMetaData()
            {
                RemoveDataFromLibrary();

                // Reload This Card

            }

            internal void DeleteTrack()
            {
                RemoveDataFromLibrary(true, true);

            }

            private void RemoveDataFromLibrary(bool isDelete = false, bool removeTrackReferences = false)
            {
                if (BaseDBTrackLibrary == null)
                {

                }

                using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                {
                    bool isNew = false;
                    var s = db.SongLibraries.Find(BaseDBTrackLibrary.SongLibraries.First().SongId);

                    // isSongShared -- new song if need to preserve old song
                    if (s.TrackLibraries.Count > 1) { s = new SongLibrary(); isNew = true; }

                    if (!isNew)
                    {
                        #region Albums


                        // delete albums that aren't shared
                        var a = (from ab in s.AlbumLibraries
                                 where ab.SongLibraries.Count == 1
                                 select ab).ToList();

                        foreach (var ab in a)
                        {
                            s.AlbumLibraries.Remove(ab);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(ab).State = System.Data.Entity.EntityState.Deleted;
                            db.SaveChanges();
                        }

                        var albums = s.AlbumLibraries.ToList();
                        foreach (var ab in albums)
                        {
                            s.AlbumLibraries.Remove(ab);
                        }

                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();


                        #endregion Albums

                        #region Artists


                        // delete albums that aren't shared
                        var art = (from ab in s.Artists
                                   where ab.SongLibraries.Count == 1
                                   select ab).ToList();

                        foreach (var ab in art)
                        {
                            s.Artists.Remove(ab);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(ab).State = System.Data.Entity.EntityState.Deleted;
                            db.SaveChanges();
                        }

                        var artists = s.Artists.ToList();
                        foreach (var ab in artists)
                        {
                            s.Artists.Remove(ab);
                        }

                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();


                        #endregion Artists
                    }

                    #region Song Entry


                    if (isDelete && !isNew)
                    {
                        db.Entry(s).State = System.Data.Entity.EntityState.Deleted;
                        db.SaveChanges();
                    }
                    else
                    {
                        if (Id3Tag != null && !string.IsNullOrEmpty(Id3Tag.Title))
                        {
                            s.SongName = Id3Tag.Title;

                            // check if has artist
                            if (searchArtist.Count > 0)
                            {
                                foreach (var sArt in searchArtist)
                                {
                                    Artist newArt = new Artist();

                                    if (db.Artists.Any(q1 => q1.ArtistName.Equals(sArt, StringComparison.OrdinalIgnoreCase)))
                                    {
                                        newArt = db.Artists.Where(q1 => q1.ArtistName.Equals(sArt, StringComparison.OrdinalIgnoreCase)).First();
                                    }
                                    else
                                    {
                                        newArt.ArtistName = sArt;
                                    }
                                    s.Artists.Add(newArt);
                                }
                            }
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(searchTrack))
                                s.SongName = FileName;
                            else
                                s.SongName = searchTrack;
                        }

                        s.Genre = null;

                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                    }

                    #endregion Song Entry

                    if (removeTrackReferences)
                    {
                        var playlists = from p in db.Playlists where p.TrackId == BaseDBTrackLibrary.Id select p.PlaylistId;
                        foreach (var i in playlists)
                        {
                            Global.mPlayer.dbRemoveTrackFromPlaylist(BaseDBTrackLibrary, i);
                        }
                    }

                    if (isDelete)
                    {
                        var t = db.TrackLibraries.Find(BaseDBTrackLibrary.Id);
                        db.Entry(t).State = System.Data.Entity.EntityState.Deleted;
                        db.SaveChanges();
                    }
                }
            }


            internal void SaveToTrackLibrary(JukeboxBrainsDBEntities db, string type)
            {
                if (BaseDBTrackLibrary == null)
                {
                    // Karaoke Files need to save mp3
                    if (isKaraoke)
                    {
                        string xFileName = System.IO.Path.GetFileNameWithoutExtension(SongMp3Path);
                        string xFileExtention = System.IO.Path.GetExtension(SongMp3Path);
                        string xFilePath = SongMp3Path;

                        BaseDBTrackLibrary = db.TrackLibraries.Add(new TrackLibrary() { Extention = xFileExtention, FileName = xFileName, FilePath = xFilePath, Type = type });
                    }
                    else
                        BaseDBTrackLibrary = db.TrackLibraries.Add(new TrackLibrary() { Extention = FileExtention, FileName = FileName, FilePath = FilePath, Type = type });

                    db.SaveChanges();

                    //Global.ImportAnalytics.SaveToDatabase(ref db, FilePath, BaseDBTrackLibrary.Id);
                }
                else
                {
                    var track = db.TrackLibraries.Find(BaseDBTrackLibrary.Id);
                    track.Type = type;
                    db.Entry(track).State = System.Data.Entity.EntityState.Modified;

                    db.SaveChanges();
                    BaseDBTrackLibrary = track;
                    //Global.ImportAnalytics.SaveToDatabase(ref db, FilePath, BaseDBTrackLibrary.Id);
                }

                hasDBTrackLibrary = true;
            }


            internal void SaveFileToLibrary(JukeboxBrainsDBEntities db, string playlistname = "")
            {
                // Sort out track first

                TrackLibrary tl = new TrackLibrary();
                if (BaseDBTrackLibrary == null && hasDBTrackLibrary)
                    if (isKaraoke)
                        BaseDBTrackLibrary = db.TrackLibraries.First(f => f.FilePath == SongMp3Path);
                    else
                        BaseDBTrackLibrary = db.TrackLibraries.First(f => f.FilePath == FilePath);

                if (BaseDBTrackLibrary != null)
                {
                    tl = BaseDBTrackLibrary;
                }
                tl.FilePath = FilePath;
                tl.FileName = FileName;
                tl.Extention = FileExtention;

                if (isKaraoke)
                    tl.Type = "Karaoke";
                else if (isVideo)
                    tl.Type = "Video";
                else if (isMusic)
                    tl.Type = "Music";


                db.Entry(tl).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                bool hasSong = BaseDBTrackLibrary.SongLibraries.Any();
                if (!hasSong)
                {


                    SongLibrary sl = new SongLibrary();

                    // check if has artist
                    if (searchArtist.Distinct().Count() > 0)
                    {
                        foreach (var art in searchArtist.Distinct())
                        {
                            Artist a = new Artist();

                            if (db.Artists.Any(q1 => q1.ArtistName.Equals(art, StringComparison.OrdinalIgnoreCase)))
                            {
                                a = db.Artists.Where(q1 => q1.ArtistName.Equals(art, StringComparison.OrdinalIgnoreCase)).First();
                            }
                            else
                            {
                                a.ArtistName = art;
                            }

                            sl.Artists.Add(a);
                        }
                    }

                    // check if has albums
                    if (!string.IsNullOrEmpty(searchAlbum))
                    {
                        AlbumLibrary al = new AlbumLibrary();
                        if (db.AlbumLibraries.Any(q2 => q2.AlbumName.Equals(searchAlbum, StringComparison.OrdinalIgnoreCase)))
                        {
                            al = db.AlbumLibraries.Where(q2 => q2.AlbumName.Equals(searchAlbum, StringComparison.OrdinalIgnoreCase)).First();
                        }
                        else
                        {
                            al.AlbumName = searchAlbum;
                            //if(!string.IsNullOrEmpty(mf.Id3Tag.Genre.Value))
                            if (Id3Tag != null)
                            {
                                if (Id3Tag.Year != null) al.Year = Id3Tag.Year.Value ?? null;
                                if (Id3Tag.Pictures.Count() > 0) al.CoverArt = Id3Tag.Pictures.First().PictureData;
                            }
                        }
                        sl.AlbumLibraries.Add(al);
                    }




                    if (!string.IsNullOrEmpty(playlistname))
                    {
                        int sequencenumber = 0;
                        if (db.PlayListDetails.Any(a => a.Name == playlistname))
                        {
                            // use existing playlist
                            PlayListDetail pld = db.PlayListDetails.Where(w => w.Name == playlistname).First();
                            sequencenumber = pld.Playlists.Count();

                            tl.Playlists.Add(new Playlist() { SequenceNumber = sequencenumber, PlayListDetail = pld });
                        }
                        else
                        {
                            // create new playlist
                            tl.Playlists.Add(new Playlist() { SequenceNumber = sequencenumber, PlayListDetail = new PlayListDetail() { Name = playlistname, Type = 1 } });
                        }
                    }

                    sl.TrackLibraries.Add(tl);


                    if (Id3Tag != null && !string.IsNullOrEmpty(Id3Tag.Title))
                    {
                        sl.SongName = Id3Tag.Title;
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(searchTrack))
                            sl.SongName = FileName;
                        else
                            sl.SongName = searchTrack;
                    }

                    //if (mf.Id3Tag.Genre != null ) sl.Genre = mf.Id3Tag.Genre;
                    db.SongLibraries.Add(sl);
                    db.SaveChanges();
                }
            }
        }

        internal List<string> getID3Details(string filepath)
        {
            List<string> values = new List<string>();

            using (var mp3 = new Mp3(filepath))
            {
                foreach (Id3Version iv in mp3.AvailableTagVersions)
                {
                    if (iv == Id3Version.V1X)
                    {
                        try
                        {
                            values.Add("Id3 Tag version: 1x");
                            Id3Tag tag = mp3.GetTag(Id3TagFamily.Version1X);

                            //values.Add("Album: " + tag.Album);
                            if (tag.Title.Value != null) values.Add("Title: " + tag.Title);
                            if (tag.Album.Value != null) values.Add("Album: " + tag.Album);
                            if (tag.Artists.Value.Any()) values.Add("Artists: " + String.Join(", ", tag.Artists.Value));
                            if (tag.Genre.Value != null) values.Add("Genre: " + tag.Genre);
                            if (tag.Year.Value != null) values.Add("Year: " + tag.Year);
                        }
                        catch
                        {
                            values.Add("Error! Detected Id3 Tag version 1x, but cannot load Id3 Tag");
                        }
                    }
                    if (iv == Id3Version.V23)
                    {
                        try
                        {
                            values.Add("Id3 Tag version: 2x");

                            Id3Tag tag = mp3.GetTag(Id3TagFamily.Version2X);
                            if (tag.Title.Value != null) values.Add("Title: " + tag.Title);
                            if (tag.Album.Value != null) values.Add("Album: " + tag.Album);
                            if (tag.Artists.Value.Any()) values.Add("Artists: " + String.Join(", ", tag.Artists.Value));
                            if (tag.Genre.Value != null) values.Add("Genre: " + tag.Genre);
                            if (tag.Year.Value != null) values.Add("Year: " + tag.Year);
                        }
                        catch
                        {
                            values.Add("Error! Detected Id3 Tag version 2x, but cannot load Id3 Tag");
                        }
                    }
                }
            }

            return values;
        }
    }

    public class OnlineFactory
    {
        public static List<MusicBrainzHelper> TrackSearchResults { get; set; }

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

        public static async Task RunTrackSearch(MusicBrainzClient client, string trackname, string artistname)
        {
            string AlbumID = "";
            //TrackSearchResults = new List<MusicBrainzHelper>();

            QueryParameters<Recording> query = new QueryParameters<Recording>();
            query.Add("artistname", artistname);
            query.Add("recording", trackname);

            var recordings = await client.Recordings.SearchAsync(query, 20);
            //var recordings = await client.Recordings.SearchAsync(trackname, 20);

            if (recordings.Count > 0)
            {
                foreach (var a1 in recordings)
                {
                    MusicBrainzHelper mb = new MusicBrainzHelper();
                    mb.SearchString = artistname + "|" + trackname;
                    Id3Tag i3 = new Id3Tag { Title = a1.Title };

                    // Found Artists
                    foreach (NameCredit nc in a1.Credits)
                    {
                        //artist = nc.Artist.Name;
                        i3.Artists.Value.Add(nc.Name);
                    }

                    #region Albums


                    List<MusicBrainzHelper> tempAlbums = new List<MusicBrainzHelper>();
                    foreach (Release r in a1.Releases)
                    {
                        MusicBrainzHelper temp_mb = new MusicBrainzHelper();
                        Id3Tag i3A = i3;

                        //AlbumName = r.Title;
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
                        //isComplex = true;
                    }

                    //TrackSearchResults.Add(mb);


                    #endregion Albums

                }
            }
        }

        public static async Task RunTrackSearch(MusicBrainzClient client, string searchname)
        {
            TrackSearchResults = new List<MusicBrainzHelper>();
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
                        //  isComplex = true;
                    }

                    //  TrackSearchResults.Add(mb);
                }

                //is simple
                //if (TrackSearchResults.Count == 1)
                //    isSimple = true;
                //if (TrackSearchResults.Count > 1)
                //    isComplex = true;


            }
        }

        public static async Task RunSongSearch(MusicBrainzClient client)
        {
            await SongSearch(client, "Massive Attack", "Mezzanine", "Teardrop");
        }

        public static async Task SongSearch(MusicBrainzClient client, string artist, string album, string song)
        {
            // Build an advanced query to search for the recording.
            var query = new QueryParameters<Recording>()
            {
                { "artist", artist },
                { "release", album },
                { "recording", song }
            };

            // Search for a recording by title.
            var recordings = await client.Recordings.SearchAsync(query);
            string s = "Stop here!";
        }
    }

    public class OnlineFactory2
    {
        public static async Task Run(MusicBrainzClient client)
        {
            await Search(client, "Amanda Palmer");
        }

        public static async Task RunSongSearch(MusicBrainzClient client, string song, string artist)
        {
            await SongSearch(client, song, artist);
        }

        public static async Task SongSearch(MusicBrainzClient client, string song, string artist)
        {
            // Build an advanced query to search for the recording.
            var query = new QueryParameters<Recording>()
            {
                { "artist", artist },
                { "recording", song }
            };

            // Search for a recording by title.
            var recordings = await client.Recordings.SearchAsync(query);
            int count = recordings.Count();
        }

        public static async Task Search(MusicBrainzClient client, string band)
        {
            var artists = await client.Artists.SearchAsync(band.Quote());
            var artist = artists.Items.First();

        }
    }

    public class ExampleFactory
    {
        public class MusicBrainz
        {
            public MusicBrainzClient client { get; set; }
            int timeout = 1000;

            public MusicBrainz()
            {
                var location = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                client = new MusicBrainzClient()
                {
                    Cache = new FileRequestCache(System.IO.Path.Combine(location, "cache"))
                };

                ///OnlineFactory.RunSongSearch(client);
            }


            public void UpdateAlbumDetails(SongLibrary song, AlbumLibrary album, Artist artist)
            {
                var task = SongSearch(client, artist.ArtistName, album.AlbumName, song.SongName);
                int timeout = 1000;
                task.Wait(timeout);
            }

            private async Task AlbumSearch(MusicBrainzClient client, string artist, string album, string song)
            {
                var query = new QueryParameters<Release>()
            {
                { "artist", artist },
                { "release", album },
                { "recording", song }
            };

                // Search for a recording by title.
                var albums = await client.Releases.SearchAsync(query);
                string s = "Stop here!";
            }

            public void RunSongSearch(string artistname, string trackname, string albumname)
            {
                var task = SongSearch(client, artistname, albumname, trackname);
                task.Wait(timeout);
            }

            public void RunSongSearch(string artistname, string trackname)
            {
                var task = SongSearch(client, artistname, trackname);
                task.Wait(timeout);
            }


            //var recordings = await client.Recordings.SearchAsync(query, 20);
            private async Task SongSearch(MusicBrainzClient client, string artist, string song)
            {
                QueryParameters<Recording> query = new QueryParameters<Recording>();
                query.Add("artistname", artist);
                query.Add("recording", song);

                var recordings = await client.Recordings.SearchAsync(query);
                int i = recordings.Count();

                string s = "Stop here!";
            }

            private async Task SongSearch(MusicBrainzClient client, string artist, string album, string song)
            {
                // Build an advanced query to search for the recording.
                var query = new QueryParameters<Recording>()
            {
                { "artist", artist },
                { "release", album },
                { "recording", song }
            };

                // Search for a recording by title.
                var recordings = await client.Recordings.SearchAsync(query);
                int i = recordings.Count();
                string s = "Stop here!";
            }
        }
    }
}
