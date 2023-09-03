using iTunesSearch.Library.Models;
using System.IO;
using System.Net.Http;
using System.Runtime.Serialization.Json;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JukeBoxSolutions;
using JukeBoxSolutions.Controls;
using System.Diagnostics;
using System.Threading;
using System.Drawing;
using System.Net;
using static JukeBoxSolutions.ImportFactory;
using JukeBoxSolutions.Class;
using System.Text.RegularExpressions;
using JukeBoxSolutions.Pages;
using System.Windows;
using static JukeBoxSolutions.Library;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace iTunesSearch.Library
{
    /// <summary>
    /// A wrapper for the iTunes search API.  More information here:
    /// https://www.apple.com/itunes/affiliates/resources/documentation/itunes-store-web-service-search-api.html
    /// </summary>
    class iTunesSearchManager
    {
        /// <summary>
        /// The base API url for iTunes search
        /// </summary>
        private string _baseSearchUrl = "https://itunes.apple.com/search?{0}";

        /// <summary>
        /// The base API url for iTunes lookups
        /// </summary>
        private string _baseLookupUrl = "https://itunes.apple.com/lookup?{0}";


        internal class LookUpRef
        {
            internal string SearchText { get; set; }
            internal int AlbumYear { get; set; }
            internal Id3.Id3Tag bufferID3Tag { get; set; }
            internal Artist DBArtist { get; set; }
            internal AlbumLibrary DBAlbum { get; set; }
        }

        #region Session Memory





        #endregion Session Memory

        #region Threading Methods


        // New songs from Importer -- does not use Database, has it's own
        internal void FindTrackDetails(ref ImportFactory.ManagedFile mf)
        {
            //iManagedTrack iTrack = new iManagedTrack(song);
        }

        internal async void AttachNewAlbums(List<FactorySessionAlbum> factorySessionAlbums, List<long> unhideAlbumId)
        {
            //ThreadPool.QueueUserWorkItem(_ => UpdateSearchAlbumAsync(baseAlbum, libraryPopup));
            CreateAlbumsAsync(factorySessionAlbums, unhideAlbumId);
        }

        //internal void UpdateSearchAlbum(AlbumLibrary baseAlbum, LibraryPopup libraryPopup)
        internal async void UpdateSearchAlbum(AlbumLibrary baseAlbum, LibraryPopup libraryPopup)
        {
            // Get Search API
            //ThreadPool.QueueUserWorkItem(_ => UpdateSearchAlbumAsync(baseAlbum, libraryPopup));

            UpdateSearchAlbumAsync(baseAlbum, libraryPopup);
        }

        internal void UpdateTrack(SongLibrary baseSong, LibraryPopup libraryPopup)
        {
            ThreadPool.QueueUserWorkItem(_ => UpdateSearchTrackAsync(baseSong));
            // invoke update of control
        }

        internal void ImportTreadedTracksV2(List<ManagedFile> managedFiles, string playlistname = "", bool copyToDefaultFolder = false, int analyticBatchId = -1)
        {
            Global.importFactory.SessionOpen();
            ThreadPool.QueueUserWorkItem(_ => ImportTracksAsyncV2(managedFiles, false, playlistname, copyToDefaultFolder, analyticBatchId));
            //ImportTracksAsyncV2(managedFiles, playlistname, copyToDefaultFolder);
        }

        internal void AutoUpdateTreadedTracksV2(List<ManagedFile> managedFiles)
        {
            Global.importFactory.SessionOpen();
            Global.isAutoUpdating = true;
            ThreadPool.QueueUserWorkItem(_ => ImportTracksAsyncV2(managedFiles, true));
        }

        internal void ImportTreadedTracks(List<ManagedFile> managedFiles, string playlistname = "", bool copyToDefaultFolder = false)
        {
            //ThreadPool.QueueUserWorkItem(_ => ImportTracksAsync(managedFiles, playlistname));
            Global.importFactory.SessionOpen();
            ImportTracksAsync(managedFiles, playlistname, copyToDefaultFolder);
        }

        public class ResultDictionaryArtist
        {
            public SearchAnalysisRule Rule { get; set; }
            public SongArtist iTunesArtist { get; set; }
            public Artist dbArtist { get; set; }
        }

        private async void ImportTracksAsyncV2(List<ManagedFile> managedFiles, bool isUpdate = false, string playlistname = "", bool copyToDefaultFolder = false, int analyticBatchId = -1)
        {
            int LogID = LogSystem.StartProcess(LogSystem.Processes.iTunesImport, managedFiles.Select(s => s.FilePath).ToList(), "Import Media");
            LogSystem.AddEvent(LogID, "Starting Files Count: " + managedFiles.Count());
            LogSystem.AddDataDump(LogID, "Import Files", managedFiles.Select(s => s.FilePath).ToList());

            bool DisableCopyToFolder = false;
            bool DisableFolderAlbumSearch = false;
            bool DisableITunesUpdate = false;
            bool DisableSaveAsSong = false;
            int playlistID = -1;
            bool hasPlaylist = false;
            List<int> tempPlaylistBatch_trackID = new List<int>();


            if (Global.importFactory.DisableImportValidation)
            {
                LogSystem.AddEvent(LogID, "Skipping iTunes Search");
                Global.ForceOnlineTimeout();
            }

            // Playlist output : Saved xxx tracks to [new playlist], "[name]"
            string playlistType = "";
            if (!string.IsNullOrEmpty(playlistname))
            {
                playlistname = SuperTrim(playlistname);

                int sequencenumber = 0;
                using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                {
                    // can't add to system playlists
                    var p = db.PlayListDetails.Where(w => playlistname.ToLower() == w.Name.ToLower() && w.Type == 1);

                    if (p.Any())
                    {
                        // use existing playlist
                        playlistID = p.First().Id;
                        hasPlaylist = true;
                        playlistType = "existing playlist";

                        // update playlist type
                        if (p.First().isMusic == false) p.First().isMusic = managedFiles.Any(a1 => a1.isMusic == true);
                        if (p.First().isKaraoke == false) p.First().isKaraoke = managedFiles.Any(a1 => a1.isKaraoke == true);
                        if (p.First().isVideo == false) p.First().isVideo = managedFiles.Any(a1 => a1.isVideo == true);
                        db.Entry(p.First()).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                    }
                    else
                    {
                        // create new playlist
                        //LogSystem.UpdateFunction(functionID, "Save tracks to new playlist");
                        PlayListDetail pDetail = new PlayListDetail() { Name = playlistname, Type = 1 };
                        pDetail.isMusic = managedFiles.Any(a1 => a1.isMusic == true);
                        pDetail.isKaraoke = managedFiles.Any(a1 => a1.isKaraoke == true);
                        pDetail.isVideo = managedFiles.Any(a1 => a1.isVideo == true);

                        db.PlayListDetails.Add(pDetail);
                        db.SaveChanges();
                        playlistID = pDetail.Id;
                        hasPlaylist = true;
                        playlistType = "new playlist";
                    }
                }
            }


            /// Add existing tracks to Playlist (if needed)
            /// Remove existing tracks from this list
            /// Does not copy existing tracks to new location
            #region Files in DB Already + Playlist Add


            var dbList = managedFiles.Where(a => a.isDatabase == true).ToList();
            if (hasPlaylist)
            {
                var dblist2 = (from a in dbList select a.FilePath).ToList();

                using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                {
                    var dbTrackList = from a in db.TrackLibraries
                                      where dblist2.Contains(a.FilePath)
                                      select a;

                    int iSeq = db.Playlists.Where(w => w.PlaylistId == playlistID).Count();
                    foreach (var t in dbTrackList)
                    {
                        db.Playlists.Add(new Playlist() { TrackId = t.Id, PlaylistId = playlistID, SequenceNumber = iSeq });
                        iSeq++;
                    }
                    db.SaveChanges();
                    LogSystem.AddEvent(LogID, "Saved tracks (" + dbTrackList.Count() + ") to " + playlistType + ", " + playlistname);
                }
            }

            // why do you remove tracks from playlist???
            // wait.. so this automatically removes tracks that are in the db already?
            // how do we update then?
            managedFiles.RemoveAll(r => dbList.Contains(r));


            #endregion Files in DB Already + Playlist Add




            List<ManagedFile> WorkList = (from a in managedFiles select a).ToList();
            int trackcount = WorkList.Count();

            List<ResultDictionaryArtist> artistDictionary = new List<ResultDictionaryArtist>();

            /// Copy to Folder & Write to TRACK LIBRARY
            if (DisableCopyToFolder == false)
            {
                if (copyToDefaultFolder)
                {
                    // Copy to Default Folder
                    string destinationFolder = Global.DefaultImportDirectory;


                    // Set notification thingy
                    int iNotification = Global.mPlayer.Notifactions.NewNotification(string.Format("Copy files to destination {0} of {1}", 0, managedFiles.Count));
                    int iZ = 0;
                    using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                    {
                        // check directory
                        if (!System.IO.Directory.Exists(destinationFolder))
                            System.IO.Directory.CreateDirectory(destinationFolder);

                        foreach (var m in managedFiles)
                        {
                            try { System.IO.File.Copy(m.FilePath, destinationFolder + m.FullFileName); }
                            catch { }

                            string mp3FileName = System.IO.Path.GetFileName(m.SongMp3Path);
                            try { System.IO.File.Copy(m.SongMp3Path, destinationFolder + mp3FileName); }
                            catch { }

                            Global.mPlayer.Notifactions.UpdateNotification(iNotification, string.Format("Copy files to destination {0} of {1}", iZ, managedFiles.Count));
                            m.setFileName(destinationFolder + m.FullFileName);
                            // write to task db
                            m.SaveToTrackLibrary(db, "Import - " + m.ImportTypeString);

                            iZ++;
                        }

                    }

                    LogSystem.AddEvent(LogID, "Moved tracks (" + managedFiles.Count() + ") to Windows Folder, " + destinationFolder);
                    Global.mPlayer.Notifactions.RemoveNotification(iNotification);
                }
            }



            int iN2 = 0;
            int i2 = 0;



            //var id3 = from ix in managedFiles
            //          where ix.hasValidId3 == true
            //          select ix;



            List<string> AlbumArtUpdateList_iTunesID = new List<string>();
            if (isUpdate)
            {
                var unsaved = from m in managedFiles where m.hasDBTrackLibrary == false select m;
                int unsavedCount = unsaved.Count();

                using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                {
                    foreach (var m in managedFiles)
                    {
                        m.SaveToTrackLibrary(db, "Import - " + m.ImportTypeString);
                    }
                }

                LogSystem.AddEvent(LogID, "Update Process : Marked tracks (" + managedFiles.Count() + ") for Import / Update");
            }
            else
            {
                ///
                /// save all tracks to Database
                ///

                var unsavedKaraoke = from m in managedFiles where m.hasDBTrackLibrary == false && m.isKaraoke == true select m;
                var unsaved = from m in managedFiles where m.hasDBTrackLibrary == false && m.isKaraoke == false select m;

                int unsavedCount = unsaved.Count() + unsavedKaraoke.Count();
                iN2 = Global.mPlayer.Notifactions.NewNotification(string.Format("Saving tracks to Database {0} of {1}", 0, unsavedCount));
                int i = 1;
                using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                {
                    LogSystem.AddEvent(LogID, "Import Process : Saving tracks to DataBase (" + unsaved.Count() + ")");


                    //Unsaved - No database
                    var StartTime = DateTime.Now.ToLongTimeString();
                    foreach (var tUnsaved in unsaved)
                    {
                        Global.mPlayer.Notifactions.UpdateNotification(iN2, string.Format("Saving tracks to Database {0} of {1}", i, unsavedCount));
                        db.TrackLibraries.Add(new TrackLibrary() { Extention = tUnsaved.FileExtention, FileName = tUnsaved.FileName, FilePath = tUnsaved.FilePath, Type = "Import - " + tUnsaved.ImportTypeString });
                        i++;
                    }
                    db.SaveChanges();

                    foreach (var tUnsaved in unsavedKaraoke)
                    {
                        Global.mPlayer.Notifactions.UpdateNotification(iN2, string.Format("Saving tracks to Database {0} of {1}", i, unsavedCount));
                        db.TrackLibraries.Add(new TrackLibrary() { Extention = tUnsaved.FileExtention, FileName = tUnsaved.FileName, FilePath = tUnsaved.FilePath, Type = "Import - " + tUnsaved.ImportTypeString });
                        i++;
                    }
                    db.SaveChanges();


                    var LapTime = DateTime.Now.ToLongTimeString();

                    int iConuterPre = unsaved.Where(wfe2 => wfe2.hasDBTrackLibrary == true).Count();
                    unsaved.ToList().ForEach(fe => fe.hasDBTrackLibrary = true);

                    managedFiles.ForEach(fe5 => fe5.hasDBTrackLibrary = true);
                    int iConuter = unsaved.Where(wfe1 => wfe1.hasDBTrackLibrary == true).Count();
                    int iConuter3 = managedFiles.Where(wfe3 => wfe3.hasDBTrackLibrary == true).Count();
                    int iConuter4 = managedFiles.Where(wfe4 => wfe4.hasDBTrackLibrary == false).Count();



                    /// OLD CODE - 2.5x slower!

                    //var StartTime = DateTime.Now.ToLongTimeString();
                    //foreach (var m in unsaved)
                    //{
                    //    //Global.mPlayer.Notifactions.UpdateNotification(iN2, string.Format("Saving tracks to Database {0} of {1}", i, unsavedCount));
                    //    m.SaveToTrackLibrary(db, "Import - " + m.ImportTypeString);
                    //    i++;
                    //}
                    //var LapTime = DateTime.Now.ToLongTimeString();
                    //bool isdone = true;
                }

                LogSystem.AddEvent(LogID, "Import Process : Marked tracks (" + managedFiles.Count() + ") for Import");


                // check for export data
                // get unique folder list.. sort by length
                var importList = managedFiles.Select(s => s.FolderPath).Distinct().OrderBy(o => o);
                int startingCount = managedFiles.Count();
                foreach (var dir in importList)
                {
                    // check if jukebox file in folder
                    if (dir != null)
                    {
                        string filedir = dir + @"\albumData.jukebox";
                        if (System.IO.File.Exists(filedir))
                        {
                            var mBatch = managedFiles.Where(w => w.FolderPath == dir);


                            // pull data from file
                            ExportFactory xFactory = new ExportFactory();
                            xFactory.ImportBatch(filedir, ref mBatch);

                            var removelist = mBatch.Where(w2 => w2.Status == ManagedFileStatus.itunesImported).Select(s => s.FilePath).ToList();
                            var removeListID = mBatch.Where(w2 => w2.Status == ManagedFileStatus.itunesImported).Select(s => s.BaseDBTrackLibrary.Id).ToList();
                            if (hasPlaylist)
                                tempPlaylistBatch_trackID.AddRange(removeListID.ToList());

                            managedFiles.RemoveAll(r => removelist.Contains(r.FilePath));
                            WorkList.RemoveAll(r2 => removelist.Contains(r2.FilePath));


                            AlbumArtUpdateList_iTunesID.AddRange(xFactory.AlbumCoverDownloadList_iTunesID);
                            int tempcount = Global.importFactory.sessionAlbums.Count();
                        }
                    }
                }

                LogSystem.AddEvent(LogID, "Imported MetaData from exported data. Started with " + startingCount + ", " + managedFiles.Count() + " remaining for Import");

            }

            /// Goes hunting for albums
            #region has ID3


            List<AlbumBufferItem> AlbumBuffer = new List<AlbumBufferItem>();
            if (Global.isOnline)
            {
                Global.mPlayer.Notifactions.UpdateNotification(iN2, string.Format("Searching Album Info..."));
                LogSystem.AddEvent(LogID, "Scanning files with ID3 Tags for Albums (" + managedFiles.Where(w => w.hasValidId3 == true).Count() + ")");
                // Try to get the album results, don't worry about not finding an album now
                foreach (var i3 in managedFiles.Where(w => w.hasValidId3 == true))
                {
                    if (!AlbumBuffer.Where(w => w.AlbumName == i3.Id3Tag.Album).Any())
                    {
                        // Track Details
                        iManagedTrack iTrack = new iManagedTrack(i3);
                        iTrack.SetAlbumSearch();

                        // Search Album
                        while (iTrack.isSearching)
                        {
                            string artApi = getApi_SearchAlbum(iTrack.searchName.SearchTerm, 100, "");
                            AlbumResult apiResult;
                            try { apiResult = await MakeAPICall<AlbumResult>(artApi); }
                            catch
                            {
                                Global.SetOnlineTimeout(iTrack.BufferManagedFile.FilePath);
                                break;
                            }

                            iTrack.AlbumResults = apiResult;
                            iTrack.NextSearchTerm();
                        }

                        string noresults;

                        if (iTrack.HasAlbumResult)
                        {
                            AlbumBuffer.Add(new AlbumBufferItem() { BaseAlbum = iTrack.BaseAlbum, AlbumName = i3.Id3Tag.Album, ArtistName = i3.Id3Tag.Artists, SearchTerm = iTrack.searchName.SearchTerm });
                        }
                        else
                        {
                            AlbumBuffer.Add(new AlbumBufferItem() { BaseAlbum = iTrack.BaseAlbum, AlbumName = i3.Id3Tag.Album, ArtistName = i3.Id3Tag.Artists, SearchTerm = iTrack.searchName.SearchTerm, isFail = true });
                            noresults = "no albumresuults!";
                        }
                    }
                }

                /// ***************************
                /// Match Album Results with tracks
                List<ManagedFile> updatedTracks = new List<ManagedFile>();
                Global.mPlayer.Notifactions.UpdateNotification(iN2, string.Format("Updating Tracks with Album Info..."));
                foreach (AlbumBufferItem a in AlbumBuffer.Where(w => w.isFail == false))
                {
                    string albumApi = getApi_GetAlbumSongs(a.BaseAlbum.CollectionId.ToString(), 100, "");
                    SongResult apiResult;
                    try { apiResult = await MakeAPICall<SongResult>(albumApi); }
                    catch
                    {
                        Global.SetOnlineTimeout();
                        break;
                    }
                    var albumResults = apiResult;

                    //var albumResults = await MakeAPICall<SongResult>(albumApi);
                    List<Song> s = albumResults.Songs;

                    foreach (ManagedFile m in managedFiles)
                    {
                        try
                        {
                            if (!m.isiTunesReady && m.hasValidId3)
                            {
                                if (getTrackEqual(m.Id3Tag.Album, a.AlbumName) == CompareResults.Match)
                                {
                                    Regex rgx = new Regex("[^a-zA-Z0-9 ]");
                                    List<Song> x = new List<Song>();

                                    if (m.Id3Tag.Track.Value > 0)
                                        x = s.Where(w1 => w1.TrackName != null && rgx.Replace(w1.TrackName.ToLower(), "") == rgx.Replace(m.Id3Tag.Title.Value.ToLower(), "") && w1.TrackNumber == m.Id3Tag.Track.Value).ToList();
                                    else
                                        x = s.Where(w2 => w2.TrackName != null && rgx.Replace(w2.TrackName.ToLower(), "") == rgx.Replace(m.Id3Tag.Title.Value.ToLower(), "")).ToList();


                                    if (x.Any())
                                    {
                                        m.isiTunesReady = true;
                                        m.iTunesAlbum = a.BaseAlbum;
                                        m.iTunesSong = x.First();
                                        s.Remove(x.First());
                                    }
                                    else
                                    {
                                        //Regex rgx = new Regex("[^a-zA-Z0-9 -]");
                                        // Might be file name.. somehow
                                        string nameX = m.Id3Tag.Title.Value.ToLower();

                                        FileInfo f = new FileInfo(nameX);
                                        if (Global.musicformats.Contains(f.Extension) || Global.karaokeformats.Contains(f.Extension) || Global.videoformats.Contains(f.Extension))
                                        {
                                            nameX = f.Name;
                                            nameX = nameX.Replace(f.Extension, "").ToLower();
                                        }

                                        nameX = rgx.Replace(nameX, "");

                                        // same track, same album... ignore name?

                                        var x1 = s.Where(w => w.TrackName != null && rgx.Replace(w.TrackName.ToLower(), "").Contains(nameX)).ToList();
                                        var x2 = s.Where(w => w.TrackName != null && rgx.Replace(w.TrackName.ToLower(), "").Contains(nameX) && w.TrackNumber == m.Id3Tag.Track.Value).ToList();
                                        var x3 = s.Where(w => w.TrackName != null && w.TrackNumber == m.Id3Tag.Track.Value).ToList();

                                        // replace name
                                        if (x3.Any())
                                        {
                                            m.isiTunesReady = true;
                                            m.iTunesAlbum = a.BaseAlbum;
                                            m.iTunesSong = x3.First();
                                            s.Remove(x3.First());
                                        }
                                        else
                                        {
                                            m.Status = ManagedFileStatus.AlbumSongMismatch;
                                            m.iTunesAlbum = a.BaseAlbum;
                                            Global.importFactory.AddMismatchAlbumBuffer(a.BaseAlbum, m);
                                        }
                                    }
                                }
                            }
                        }
                        catch
                        {
                            using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                            {
                                m.SaveToTrackLibrary(db, "Error - " + m.ImportTypeString);
                                m.Status = ManagedFileStatus.ImportError;
                            }
                        }
                    }

                    /// remainder of the songs...
                    if (s.Any())
                    {
                        Global.importFactory.AddMismatchAlbumBuffer(a.BaseAlbum, s);
                    }

                    string done = "done!";
                }


                /// Save to DB
                List<ManagedFile> importReady = WorkList.Where(w => w.isiTunesReady).ToList();
                foreach (ManagedFile m in importReady)
                {
                    iManagedTrack iTrack = new iManagedTrack(m);
                    iTrack.SaveResultsData(m.iTunesSong, m, playlistID);
                    m.Status = ManagedFileStatus.itunesImported;
                    WorkList.Remove(m);
                }

                Global.mPlayer.Notifactions.UpdateNotification(iN2, string.Format("Saved {0} of {1} tracks", importReady.Count(), managedFiles.Count()));
                // trackcount = WorkList.Count();
            }

            #endregion has ID3

            #region Find Album From Folder Name


            if (DisableFolderAlbumSearch == false && Global.isOnline)
            {
                var gDir = from a in WorkList
                           group a by a.FolderName into g
                           select new { albumDir = g.Key, track = g.ToList() };

                AlbumBuffer = new List<AlbumBufferItem>();
                List<AlbumBufferItem> AlbumBufferFolder = new List<AlbumBufferItem>();
                foreach (var b in gDir)
                {
                    if (!AlbumBufferFolder.Where(w => w.FolderName == b.albumDir).Any())
                    {
                        // Track Details
                        iManagedTrack iTrack = new iManagedTrack();
                        iTrack.SetAlbumSearch(b.albumDir);

                        // Search Album
                        while (iTrack.isSearching)
                        {
                            string artApi = getApi_SearchAlbum(iTrack.searchName.SearchTerm, 100, "");
                            AlbumResult apiResult;
                            try { apiResult = await MakeAPICall<AlbumResult>(artApi); }
                            catch
                            {
                                if (iTrack.BufferManagedFile == null)
                                    Global.SetOnlineTimeout();
                                else
                                    Global.SetOnlineTimeout(iTrack.BufferManagedFile.FilePath);
                                break;
                            }
                            iTrack.AlbumResults = apiResult;

                            iTrack.NextSearchTerm();
                        }

                        string noresults;

                        if (iTrack.HasAlbumResult)
                            AlbumBufferFolder.Add(new AlbumBufferItem() { BaseAlbum = iTrack.BaseAlbum, AlbumName = iTrack.BaseAlbum.CollectionName, ArtistName = iTrack.BaseAlbum.ArtistName, FolderName = b.albumDir, SearchTerm = iTrack.searchName.SearchTerm });
                        else
                        {
                            AlbumBufferFolder.Add(new AlbumBufferItem() { BaseAlbum = iTrack.BaseAlbum, FolderName = "", SearchTerm = iTrack.searchName.SearchTerm, isFail = true });
                            noresults = "no albumresuults!";
                        }
                    }
                }

                // ***************************
                // Match Album Results with tracks

                // BROKEN!
                // Broken - Somehow the Album Buffer already has albums in, it shouldn't (Testing Music Video Imports)
                foreach (AlbumBufferItem aBufferItem in AlbumBufferFolder.Where(w => w.isFail == false))
                {
                    string albumApi = getApi_GetAlbumSongs(aBufferItem.BaseAlbum.CollectionId.ToString(), 100, "");
                    SongResult albumResults;
                    // TODO: Testing API Call!
                    try { albumResults = await MakeAPICall<SongResult>(albumApi); }
                    catch
                    {
                        // TODO: Testing API Call!
                        Global.SetOnlineTimeout();
                        break;
                    }
                    List<Song> s = albumResults.Songs;
                    int initialSongCount = s.Count();

                    List<ManagedFile> TempUnmatchedSongBuffer = new List<ManagedFile>();

                    foreach (ManagedFile m in WorkList.Where(smalllist => smalllist.FolderName == aBufferItem.FolderName))
                    {

                        if (!m.isiTunesReady && m.hasValidId3)
                        {
                            if (getTrackEqual(m.Id3Tag.Album, aBufferItem.AlbumName) == CompareResults.Match)
                            {
                                Regex rgx = new Regex("[^a-zA-Z0-9 ]");

                                var x = s.Where(w => w.TrackName != null && rgx.Replace(w.TrackName.ToLower(), "") == rgx.Replace(m.Id3Tag.Title.Value.ToLower(), "") && w.TrackNumber == m.Id3Tag.Track.Value);
                                if (x.Any())
                                {
                                    m.isiTunesReady = true;
                                    m.iTunesAlbum = aBufferItem.BaseAlbum;
                                    m.iTunesSong = x.First();
                                    s.Remove(x.First());
                                }
                                else
                                {
                                    //Regex rgx = new Regex("[^a-zA-Z0-9 -]");
                                    // Might be file name.. somehow
                                    string nameX = m.Id3Tag.Title.Value.ToLower();

                                    FileInfo f = new FileInfo(this.RemoveInvalidFilePathCharacters(nameX, ""));
                                    if (Global.musicformats.Contains(f.Extension) || Global.karaokeformats.Contains(f.Extension) || Global.videoformats.Contains(f.Extension))
                                    {
                                        nameX = f.Name;
                                        nameX = nameX.Replace(f.Extension, "").ToLower();
                                    }

                                    nameX = rgx.Replace(nameX, "");

                                    // same track, same album... ignore name?

                                    var x1 = s.Where(w => w.TrackName != null && rgx.Replace(w.TrackName.ToLower(), "").Contains(nameX)).ToList();
                                    var x2 = s.Where(w => w.TrackName != null && rgx.Replace(w.TrackName.ToLower(), "").Contains(nameX) && w.TrackNumber == m.Id3Tag.Track.Value).ToList();
                                    var x3 = s.Where(w => w.TrackName != null && w.TrackNumber == m.Id3Tag.Track.Value).ToList();

                                    // replace name
                                    if (x3.Any())
                                    {
                                        m.isiTunesReady = true;
                                        m.iTunesAlbum = aBufferItem.BaseAlbum;
                                        m.iTunesSong = x3.First();
                                        s.Remove(x3.First());
                                    }
                                }
                            }
                        }
                        else if (!m.isiTunesReady)
                        {

                            // filename only
                            string filename = m.FileName;
                            string start = filename.Substring(0, 1);
                            int startInt;
                            if (int.TryParse(start, out startInt))
                            {
                                if (string.IsNullOrEmpty(aBufferItem.FileNameFormat))
                                {
                                    foreach (string f in StringFileNameFormats)
                                    {
                                        var x1F = s.Where(w1F => getTrackEqual(filename, string.Format(f, startInt == 0 ? w1F.TrackNumber.ToString("00") : w1F.TrackNumber.ToString(), w1F.ArtistName, w1F.TrackName, w1F.CollectionName)) == CompareResults.Match);

                                        if (x1F.Any())
                                        {
                                            aBufferItem.FileNameFormat = f;
                                            m.isiTunesReady = true;
                                            m.iTunesAlbum = aBufferItem.BaseAlbum;
                                            m.iTunesSong = x1F.First();
                                            s.Remove(x1F.First());
                                            break;
                                        }
                                        else
                                        {
                                            var x1FEx = s.Where(w1FEx => getTrackEqual(filename, string.Format(f, startInt == 0 ? w1FEx.TrackNumber.ToString("00") : w1FEx.TrackNumber.ToString(), w1FEx.ArtistName, w1FEx.TrackName, w1FEx.CollectionName)) == CompareResults.MatchWithExtra);
                                            if (x1FEx.Any())
                                            {
                                                aBufferItem.FileNameFormat = f;
                                                m.isiTunesReady = true;
                                                m.iTunesAlbum = aBufferItem.BaseAlbum;
                                                m.iTunesSong = x1FEx.First();
                                                s.Remove(x1FEx.First());
                                                break;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    var x1 = s.Where(w => getTrackEqual(filename, string.Format(aBufferItem.FileNameFormat, startInt == 0 ? w.TrackNumber.ToString("00") : w.TrackNumber.ToString(), w.ArtistName, w.TrackName, w.CollectionName)) == CompareResults.Match);
                                    //bool hasTrack = s.Any(an => getTrackEqual(an.TrackName, "Elizabeth On The Bathroom Floor") == CompareResults.Match);
                                    if (x1.Any())
                                    {
                                        m.isiTunesReady = true;
                                        m.iTunesAlbum = aBufferItem.BaseAlbum;
                                        m.iTunesSong = x1.First();
                                        s.Remove(x1.First());
                                    }
                                    else
                                    {
                                        var x1Ex = s.Where(wEx => getTrackEqual(filename, string.Format(aBufferItem.FileNameFormat, startInt == 0 ? wEx.TrackNumber.ToString("00") : wEx.TrackNumber.ToString(), wEx.ArtistName, wEx.TrackName, wEx.CollectionName)) == CompareResults.MatchWithExtra);
                                        //bool hasTrack = s.Any(an => getTrackEqual(an.TrackName, "Elizabeth On The Bathroom Floor") == CompareResults.Match);
                                        if (x1Ex.Any())
                                        {
                                            m.isiTunesReady = true;
                                            m.iTunesAlbum = aBufferItem.BaseAlbum;
                                            m.iTunesSong = x1Ex.First();
                                            s.Remove(x1Ex.First());
                                        }
                                    }
                                }

                            }
                            else
                            {
                                //var x3 = s.Where(w => getTrackEqual(string.Format("{0} - {1} - {2}", w.TrackNumber.ToString("0"), w.ArtistName, w.TrackName), filename) == CompareResults.Match);

                                if (string.IsNullOrEmpty(aBufferItem.FileNameFormat))
                                {
                                    foreach (string f in StringFileNameFormats)
                                    {
                                        var x3 = s.Where(w1F => getTrackEqual(filename, string.Format(f, startInt == 0 ? w1F.TrackNumber.ToString("00") : w1F.TrackNumber.ToString(), w1F.ArtistName, w1F.TrackName, w1F.CollectionName)) == CompareResults.Match);
                                        if (x3.Any())
                                        {
                                            aBufferItem.FileNameFormat = f;
                                            m.isiTunesReady = true;
                                            m.iTunesAlbum = aBufferItem.BaseAlbum;
                                            m.iTunesSong = x3.First();
                                            s.Remove(x3.First());
                                            break;
                                        }
                                        else
                                        {
                                            var x3Ex = s.Where(w1FEx => getTrackEqual(filename, string.Format(f, startInt == 0 ? w1FEx.TrackNumber.ToString("00") : w1FEx.TrackNumber.ToString(), w1FEx.ArtistName, w1FEx.TrackName, w1FEx.CollectionName)) == CompareResults.MatchWithExtra);
                                            if (x3Ex.Any())
                                            {
                                                aBufferItem.FileNameFormat = f;
                                                m.isiTunesReady = true;
                                                m.iTunesAlbum = aBufferItem.BaseAlbum;
                                                m.iTunesSong = x3Ex.First();
                                                s.Remove(x3Ex.First());
                                                break;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    var x1 = s.Where(w => getTrackEqual(filename, string.Format(aBufferItem.FileNameFormat, startInt == 0 ? w.TrackNumber.ToString("00") : w.TrackNumber.ToString(), w.ArtistName, w.TrackName, w.CollectionName)) == CompareResults.Match);
                                    //bool hasTrack = s.Any(an => getTrackEqual(an.TrackName, "Elizabeth On The Bathroom Floor") == CompareResults.Match);
                                    if (x1.Any())
                                    {
                                        m.isiTunesReady = true;
                                        m.iTunesAlbum = aBufferItem.BaseAlbum;
                                        m.iTunesSong = x1.First();
                                        s.Remove(x1.First());
                                    }
                                    else
                                    {
                                        var x1Ex = s.Where(wEx => getTrackEqual(filename, string.Format(aBufferItem.FileNameFormat, startInt == 0 ? wEx.TrackNumber.ToString("00") : wEx.TrackNumber.ToString(), wEx.ArtistName, wEx.TrackName, wEx.CollectionName)) == CompareResults.MatchWithExtra);
                                        //bool hasTrack = s.Any(an => getTrackEqual(an.TrackName, "Elizabeth On The Bathroom Floor") == CompareResults.Match);
                                        if (x1Ex.Any())
                                        {
                                            m.isiTunesReady = true;
                                            m.iTunesAlbum = aBufferItem.BaseAlbum;
                                            m.iTunesSong = x1Ex.First();
                                            s.Remove(x1Ex.First());
                                        }
                                    }
                                }
                            }
                            //Regex rgx = new Regex("[^a-zA-Z0-9 ]");

                            // add to buffer
                            if (m.isiTunesReady == false)
                                TempUnmatchedSongBuffer.Add(m);
                        }

                    }



                    // skip buffer
                    if (TempUnmatchedSongBuffer.Any() && s.Count < initialSongCount)
                    {
                        foreach (var temp in TempUnmatchedSongBuffer)
                            Global.importFactory.AddBufferSongMismatch(temp, s);
                    }


                    string done = "done!";


                }


                int quickcount = WorkList.Count(cnt => cnt.isiTunesReady == true);
                // Save Results to DataBase

                if (quickcount > 0)
                {
                    foreach (ManagedFile wItem in WorkList.Where(wi => wi.isiTunesReady == true && wi.Status == ManagedFileStatus.Importing))
                    {
                        /// Save to DB
                        iManagedTrack iTrack = new iManagedTrack(wItem);
                        iTrack.SaveResultsData(wItem.iTunesSong, wItem, playlistID);
                        wItem.Status = ManagedFileStatus.itunesImported;
                        //WorkList.Remove(wItem);
                    }
                }

                // Remove found items from list
                // 


                // Save to DB
                //List<ManagedFile> importReady = managedFiles.Where(w => w.isiTunesReady).ToList();
                //foreach (ManagedFile m in importReady)
                //{
                //    WorkList.Remove(m);
                //    iManagedTrack iTrack = new iManagedTrack(m);
                //    iTrack.SaveResultsData(m.iTunesSong, m);
                //}

                trackcount = WorkList.Count();
            }




            #endregion Find Album From Folder Name









            /// NO ID3
            /// Search pattern DB
            /// Search pattern Batch
            if (Global.isOnline)
            {
                LogSystem.AddEvent(LogID, WorkList.Count(c => c.Status == ManagedFileStatus.Importing) + " tracks remaining. Manually searching content on iTunes Server");

                foreach (var m in WorkList.Where(w => w.Status == ManagedFileStatus.Importing))
                {
                    Global.mPlayer.Notifactions.UpdateNotification(iN2, string.Format("Downloading song info {0} of {1}", i2, WorkList.Count));
                    iManagedTrack iTrack = new iManagedTrack(m);
                    iTrack.ImportPlaylistId = playlistID;

                    /// This would be the place to do an album search
                    //Global.importFactory.AlbumBuffer.First().Rule.ArtistName.ToUpper....


                    //iTrack.SetAlbumSearch()
                    iTrack.SetArtistSearch(WorkList.Where(wfiles => wfiles.FolderPath == m.FolderPath).ToList());

                    // GetSearch API
                    // Find Artist First!
                    string artApi = "";
                    //string artApi = getApi_SearchArtist(iTrack.searchName.SearchTerm, 100, "");
                    int iCount = iTunesCallCounter;
                    //iTrack.ArtistResults = await MakeAPICall<SongArtistResult>(artApi);

                    if (!iTrack.HasArtist)
                        while (iTrack.isSearching)
                        {
                            // continue search for artist
                            artApi = getApi_SearchArtist(iTrack.searchName.SearchTerm, 100, "");
                            iCount = iTunesCallCounter;
                            SongArtistResult apiResult;
                            try { apiResult = await MakeAPICall<SongArtistResult>(artApi); }
                            catch
                            {
                                Global.SetOnlineTimeout(iTrack.BufferManagedFile.FilePath);
                                break;
                            }
                            iTrack.ArtistResults = apiResult;
                            // cycle index automatically
                            if (iTrack.HasArtist)
                                break;
                            else
                                iTrack.NextSearchTerm();
                        }

                    // Search Song
                    if (iTrack.HasArtist)
                    {
                        while (iTrack.isSearching)
                        {
                            string api = getApi_SearchSong(iTrack.searchName.SearchTerm, 100, "");
                            iCount = iTunesCallCounter;
                            SongResult apiResult;
                            try { apiResult = await MakeAPICall<SongResult>(api); }
                            catch
                            {
                                Global.SetOnlineTimeout(iTrack.BufferManagedFile.FilePath);
                                break;
                            }
                            iTrack.SongResults = apiResult;

                            // cycle index automatically
                            if (iTrack.HasTrack)
                                break;
                            else
                                iTrack.NextSearchTerm();
                        }

                        if (!iTrack.HasTrack)
                        {
                            // Could not find Artist
                            Global.ImportAnalytics.AddBreadcrumb(iTrack.BufferManagedFile.FilePath, "iTunes:Found Artist, could not find Track");
                            Global.ImportAnalytics.AddSupportingMetaData(iTrack.BufferManagedFile.FilePath, "iTunes:Found Artist, could not find Track", iTrack.searchNames.Select(s => s.SearchTerm).ToList());
                        }
                    }
                    else
                    {
                        // Could not find Artist
                        Global.ImportAnalytics.AddBreadcrumb(iTrack.BufferManagedFile.FilePath, "iTunes:Could not find Artist");
                        Global.ImportAnalytics.AddSupportingMetaData(iTrack.BufferManagedFile.FilePath, "iTunes:Could not find Artist", iTrack.searchNames.Select(s => s.SearchTerm).ToList());
                    }

                    if (Global.isOnline == false)
                        break;

                    m.NotBeenUpdated = false;
                    i2++;
                }
            }
            Global.mPlayer.Notifactions.RemoveNotification(iN2);



            #region save remainder to playlist and songs



            if (DisableSaveAsSong == false)
            {
                // Don't want online status to change at this point
                bool isOnline = Global.isOnline;
                List<int> autoupdate_trackID = new List<int>();

                using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                {
                    LogSystem.AddEvent(LogID, "-------------------------------------------------------");

                    if (Global.importFactory.MismatchSongBuffer.Any())
                    {
                        LogSystem.AddEvent(LogID, "Mismatch Songs: " + Global.importFactory.MismatchSongBuffer.Count()); ;
                        LogSystem.AddDataDump(LogID, "MismatchSongsBuffer", Global.importFactory.MismatchSongBuffer.Select(s => s.ManagedFile.FilePath).ToList());
                    }
                    if (Global.importFactory.MismatchAlbumBuffer.Any())
                    {
                        LogSystem.AddEvent(LogID, "Mismatch Albums: " + Global.importFactory.MismatchAlbumBuffer.Count()); ;

                        List<string> sBatch = Global.importFactory.MismatchAlbumBuffer.Select(s => s.BaseAlbum.CollectionName + " - " + s.BaseAlbum.ArtistName).ToList();
                        sBatch.Add("------------------------------------");
                        sBatch.AddRange(Global.importFactory.MismatchAlbumBuffer.SelectMany(s2 => s2.ManagedFiles.Select(sx => sx.FilePath)).ToList());

                        LogSystem.AddDataDump(LogID, "MismatchAlbumBuffer", sBatch);

                    }






                    // TODO: Saving all 'tracks' as actual songs

                    double persComplete = 0;
                    int iN3 = Global.mPlayer.Notifactions.NewNotification(string.Format("Adding songs to Library ... {0}%", persComplete));
                    int persCounter = 0;

                    #region Batch Save Songs to Library

                    // worklist
                    // if (a.Status != ManagedFileStatus.itunesImported)
                    //                   if (!a.isCompleteDB && a.Status == ManagedFileStatus.Importing)
                    //                  else if (!a.isCompleteDB)


                    /// List of ALL tracks not imported
                    var NOTstatusImported = WorkList.Where(w1 => w1.Status != ManagedFileStatus.itunesImported).ToList();

                    /// List of all the tracks no in the Database, and still importing
                    var NOTstatusImported_Importing = NOTstatusImported.Where(w1 => w1.isCompleteDB == false && w1.Status == ManagedFileStatus.Importing).ToList();


                    ///
                    /// Step 1 -    Extract ALL the artists and save them to the Database
                    ///             This will be used as a reference table
                    #region Batch Extract and save ARTISTS

                    // check if has artist
                    var aList = NOTstatusImported_Importing.SelectMany(sm => sm.searchArtist).Distinct().ToList();
                    List<LookUpRef> ArtistRefTable = new List<LookUpRef>();
                    List<LookUpRef> ArtistCreateTable = new List<LookUpRef>();

                    // check if in db
                    foreach (var art in aList)
                    {
                        if (art != null)
                        {
                            string tempart = SuperTrim(art);
                            if (string.IsNullOrEmpty(tempart))
                                continue;
                            var dbArt = db.Artists.Where(q1 => q1.ArtistName.Equals(tempart, StringComparison.OrdinalIgnoreCase)).ToList();
                            if (dbArt.Count() > 1)
                            {
                                // There should only be one result... not sure why there's more
                                bool wehaveTrouble = true;
                            }

                            if (dbArt.Any())
                            {
                                ArtistRefTable.Add(new LookUpRef() { SearchText = tempart, DBArtist = dbArt.First() });
                            }
                            else
                            {
                                if (!ArtistCreateTable.Any(an1 => an1.SearchText.Equals(tempart, StringComparison.OrdinalIgnoreCase)))
                                    ArtistCreateTable.Add(new LookUpRef() { SearchText = tempart });
                            }
                        }
                    }

                    // create new entries
                    foreach (var art2 in ArtistCreateTable)
                    {
                        Artist newArt = new Artist();
                        newArt.ArtistName = art2.SearchText;
                        db.Artists.Add(newArt);
                        ArtistRefTable.Add(new LookUpRef() { SearchText = art2.SearchText, DBArtist = newArt });
                    }

                    // Commit Artists to Database
                    db.SaveChanges();

                    #endregion Batch Extract and save ARTISTS

                    ///
                    /// Step 2 -    Extract ALL the albums and save them to the Database
                    ///             This will be used as a reference table
                    ///             First batches take priorit over lower batches
                    #region Batch Extract and save ALBUMS
                    // check if has Albums

                    // COUNTERS USED FOR TESTING
                    //List<ManagedFile> A1 = NOTstatusImportedA.Where(x1 => x1.hasValidId3 == true).ToList();
                    //List<ManagedFile> A2 = NOTstatusImported_Importing.Where(x2 => x2.hasValidId3 == true && x2.Id3Tag.Album != null).ToList();
                    //List<ManagedFile> A3 = NOTstatusImported_Importing.Where(x3 => x3.hasValidId3 == true && x3.Id3Tag.Album == null).ToList();
                    //List<ManagedFile> A4 = NOTstatusImported_Importing.Where(x4 => x4.hasValidId3 == true && x4.Id3Tag.Year != null).ToList();
                    //List<ManagedFile> A5 = NOTstatusImported_Importing.Where(x5 => x5.hasValidId3 == true && x5.Id3Tag.Year == null).ToList();
                    //List<ManagedFile> A6 = NOTstatusImported_Importing.Where(x6 => x6.hasValidId3 == true && x6.Id3Tag.Album != null && x6.Id3Tag.Year != null).ToList();

                    //var albumListID3 = NOTstatusImportedA.Where(sm1W => sm1W.hasId3).Select(sm1 => new { sm1.Id3Tag.Album, sm1.Id3Tag.Year }).Distinct().ToList();
                    var albumListID3B = NOTstatusImported_Importing.Where(sm3W => sm3W.hasValidId3 && sm3W.Id3Tag.Album != null && sm3W.Id3Tag.Year.Value != null).Select(sm3 => new { AlbumName = sm3.Id3Tag.Album.Value, AlbumYear = sm3.Id3Tag.Year.Value, ID3Tag = sm3.Id3Tag }).Distinct().ToList();
                    var albumListID3C = NOTstatusImported_Importing.Where(sm3W => sm3W.hasValidId3 && sm3W.Id3Tag.Album != null && sm3W.Id3Tag.Year.Value == null).Select(sm3 => new { AlbumName = sm3.Id3Tag.Album.Value, ID3Tag = sm3.Id3Tag }).Distinct().ToList();
                    var albumListID3D = NOTstatusImported_Importing.Where(sm3W => sm3W.hasValidId3 && sm3W.Id3Tag.Album == null).Select(sm3 => new { AlbumName = sm3.searchAlbum }).Distinct().ToList();
                    var albumListID3E = NOTstatusImported_Importing.Where(sm3W => !sm3W.hasValidId3).Select(sm3 => new { AlbumName = sm3.searchAlbum }).Distinct().ToList();
                    var albumListBlank = NOTstatusImported_Importing.Select(sm2 => sm2.searchAlbum).Distinct().ToList();

                    List<LookUpRef> AlbumRefTable = new List<LookUpRef>();
                    List<LookUpRef> AlbumCreateTable = new List<LookUpRef>();

                    // check if in db
                    // ID3 Album + ID3 Year
                    foreach (var albA in albumListID3B)
                    {
                        if (albA != null)
                        {
                            string tempalb = SuperTrim(albA.AlbumName);
                            if (string.IsNullOrEmpty(tempalb))
                                continue;
                            var dbAlb = db.AlbumLibraries.Where(q2 => q2.AlbumName.Equals(tempalb, StringComparison.OrdinalIgnoreCase) && q2.Year == albA.AlbumYear).ToList();
                            if (dbAlb.Count() > 1)
                            {
                                // There should only be one result... not sure why there's more
                                bool wehaveTrouble = true;
                            }

                            if (dbAlb.Any())
                            {
                                AlbumRefTable.Add(new LookUpRef() { SearchText = tempalb, AlbumYear = (int)albA.AlbumYear.Value, DBAlbum = dbAlb.First() });
                            }
                            else
                            {
                                if (!AlbumCreateTable.Any(an2 => an2.SearchText.Equals(tempalb, StringComparison.OrdinalIgnoreCase) && an2.AlbumYear == albA.AlbumYear.Value))
                                    AlbumCreateTable.Add(new LookUpRef() { SearchText = tempalb, AlbumYear = (int)albA.AlbumYear.Value, bufferID3Tag = albA.ID3Tag });
                            }
                        }
                    }

                    bool hasUnexpectedValues = false;


                    // ID3 Album - ID3 NO Year
                    foreach (var albAC in albumListID3C)
                    {
                        if (albAC != null)
                        {
                            string tempalb = SuperTrim(albAC.AlbumName);
                            if (string.IsNullOrEmpty(tempalb))
                                continue;

                            if (AlbumRefTable.Any(An1C => An1C.SearchText == tempalb))
                            {
                                hasUnexpectedValues = true;
                            }
                            else if (AlbumCreateTable.Any(An2C => An2C.SearchText == tempalb))
                            {
                                hasUnexpectedValues = true;
                            }
                            else
                            {
                                var dbAlb = db.AlbumLibraries.Where(q2C => q2C.AlbumName.Equals(tempalb, StringComparison.OrdinalIgnoreCase)).ToList();
                                if (dbAlb.Count() > 1)
                                {
                                    // There should only be one result... not sure why there's more
                                    bool wehaveTrouble = true;
                                }

                                if (dbAlb.Any())
                                {
                                    AlbumRefTable.Add(new LookUpRef() { SearchText = tempalb, DBAlbum = dbAlb.First() });
                                }
                                else
                                {
                                    if (!AlbumCreateTable.Any(an2C => an2C.SearchText.Equals(tempalb, StringComparison.OrdinalIgnoreCase)))
                                        AlbumCreateTable.Add(new LookUpRef() { SearchText = tempalb, bufferID3Tag = albAC.ID3Tag });
                                }
                            }
                        }
                    }

                    if (albumListID3D.Any())
                        hasUnexpectedValues = true;

                    // ID3 Album - ID3 NO Year
                    foreach (var albAD in albumListID3E)
                    {
                        if (albAD != null && albAD.AlbumName != null)
                        {
                            string tempalb = SuperTrim(albAD.AlbumName);
                            if (string.IsNullOrEmpty(tempalb))
                                continue;
                            if (AlbumRefTable.Any(An1E => An1E.SearchText == tempalb))
                            {
                                hasUnexpectedValues = true;
                            }
                            else if (AlbumCreateTable.Any(An1E => An1E.SearchText == tempalb))
                            {
                                hasUnexpectedValues = true;
                            }
                            else
                            {
                                var dbAlb = db.AlbumLibraries.Where(q2E => q2E.AlbumName.Equals(tempalb, StringComparison.OrdinalIgnoreCase)).ToList();
                                if (dbAlb.Count() > 1)
                                {
                                    // There should only be one result... not sure why there's more
                                    bool wehaveTrouble = true;
                                }

                                if (dbAlb.Any())
                                {
                                    AlbumRefTable.Add(new LookUpRef() { SearchText = tempalb, DBAlbum = dbAlb.First() });
                                }
                                else
                                {
                                    if (!AlbumCreateTable.Any(an2E => an2E.SearchText.Equals(tempalb, StringComparison.OrdinalIgnoreCase)))
                                        AlbumCreateTable.Add(new LookUpRef() { SearchText = tempalb });
                                }
                            }
                        }
                    }

                    bool stopHere = true;
                    // check if in db

                    //  create new entries
                    foreach (var alb2 in AlbumCreateTable)
                    {
                        AlbumLibrary newAlb = new AlbumLibrary();
                        newAlb.AlbumName = SuperTrim(alb2.SearchText);
                        if (string.IsNullOrEmpty(newAlb.AlbumName))
                            continue;

                        if (alb2.AlbumYear == 0)
                            stopHere = true;
                        else
                            newAlb.Year = alb2.AlbumYear;

                        if (alb2.bufferID3Tag != null)
                        {
                            if (alb2.bufferID3Tag.Pictures.Count() > 0) newAlb.CoverArt = alb2.bufferID3Tag.Pictures.First().PictureData;
                        }

                        db.AlbumLibraries.Add(newAlb);
                        AlbumRefTable.Add(new LookUpRef() { SearchText = alb2.SearchText, DBAlbum = newAlb });
                    }

                    // Commit Albums to Database
                    db.SaveChanges();


                    #endregion Batch Extract and save ALBUMS


                    // save to AutoUpdate
                    var NOTstatusImportedB = NOTstatusImported.Where(w1 => w1.isCompleteDB == false && w1.Status != ManagedFileStatus.Importing);


                    #endregion Batch Save Songs to Library


                    /// Batch Save Songs & Details
                    foreach (var a in WorkList.Where(wh => wh.isCompleteDB == false))
                    {
                        // log file 
                        if (a.Status == ManagedFileStatus.AlbumSongMismatch)
                        {
                            continue;
                            throw new ArgumentException("This import batch has AlbumSongMismatch Status");
                        }

                        if (a.Status == ManagedFileStatus.ImportError)
                        {
                            continue;
                            throw new ArgumentException("This import batch has ImportError Status");
                        }

                        if (a.Status == ManagedFileStatus.itunesImported)
                        {
                            continue;
                            throw new ArgumentException("This import batch has Imported Status (Should have been removed)");
                        }


                        persComplete = ((persCounter * 100) / WorkList.Count);
                        Global.mPlayer.Notifactions.UpdateNotification(iN3, string.Format("Adding songs to Library ... {0}%", persComplete));
                        persCounter++;


                        // reload track
                        TrackLibrary tl = new TrackLibrary();
                        if (a.BaseDBTrackLibrary == null && a.hasDBTrackLibrary)
                        {
                            if (a.isKaraoke)
                                tl = db.TrackLibraries.First(f => f.FilePath == a.FilePath);
                            else
                                tl = db.TrackLibraries.First(f => f.FilePath == a.FilePath);
                        }
                        else
                            tl = a.BaseDBTrackLibrary;

                        // change type back to normal
                        tl.Type = a.ImportTypeString;
                        db.Entry(tl).State = System.Data.Entity.EntityState.Modified;

                        a.BaseDBTrackLibrary = null;


                        bool hasSong = tl.SongLibraries.Any();
                        if (!hasSong)
                        {
                            SongLibrary sl = new SongLibrary();

                            // check if has artist
                            if (a.searchArtist.Distinct().Count() > 0)
                            {
                                foreach (var art in a.searchArtist.Distinct())
                                {
                                    //artistDictionary
                                    string tempArt = SuperTrim(art);
                                    var foundArt = ArtistRefTable.Find(f => f.SearchText.Equals(tempArt, StringComparison.OrdinalIgnoreCase));
                                    if (foundArt != null)
                                        sl.Artists.Add(foundArt.DBArtist);
                                }
                            }


                            // check if has albums
                            if (!string.IsNullOrEmpty(a.searchAlbum))
                            {
                                var foundAlb = AlbumRefTable.Find(f1 => f1.SearchText.Equals(SuperTrim(a.searchAlbum), StringComparison.OrdinalIgnoreCase));
                                if (foundAlb != null)
                                    sl.AlbumLibraries.Add(foundAlb.DBAlbum);
                            }


                            //if (!string.IsNullOrEmpty(playlistname))
                            //{
                            //    int sequencenumber = 0;
                            //    if (db.PlayListDetails.Any(a => a.Name == playlistname))
                            //    {
                            //        // use existing playlist
                            //        PlayListDetail pld = db.PlayListDetails.Where(w => w.Name == playlistname).First();
                            //        sequencenumber = pld.Playlists.Count();

                            //        tl.Playlists.Add(new Playlist() { SequenceNumber = sequencenumber, PlayListDetail = pld });
                            //    }
                            //    else
                            //    {
                            //        // create new playlist
                            //        tl.Playlists.Add(new Playlist() { SequenceNumber = sequencenumber, PlayListDetail = new PlayListDetail() { Name = playlistname, Type = 1 } });
                            //    }
                            //}


                            sl.TrackLibraries.Add(tl);

                            if (a.Id3Tag != null && !string.IsNullOrEmpty(a.Id3Tag.Title))
                            {
                                sl.SongName = a.Id3Tag.Title;
                            }
                            else
                            {
                                if (string.IsNullOrEmpty(a.searchTrack))
                                    sl.SongName = a.FileName;
                                else
                                    sl.SongName = a.searchTrack;
                            }

                            //if (mf.Id3Tag.Genre != null ) sl.Genre = mf.Id3Tag.Genre;
                            db.SongLibraries.Add(sl);
                        }
                        else
                        {
                            SongLibrary sl = tl.SongLibraries.First();

                        }



                        if (Global.importFactory.DisableImportValidation == false)
                            if (a.Status == ManagedFileStatus.Importing)
                                autoupdate_trackID.Add(tl.Id);

                        //playlistID = pDetail.Id;
                        if (hasPlaylist == true)
                            tempPlaylistBatch_trackID.Add(tl.Id);

                        ///
                        /// ORIGINAL CODE per work item
                        ///

                        if (a.Status != ManagedFileStatus.itunesImported && false)
                        {
                            //if (!a.isCompleteDB && a.Status == ManagedFileStatus.Importing)
                            //{
                            //    a.SaveFileToLibrary(db, playlistname);
                            //    Global.ImportAnalytics.AddBreadcrumb(a.FilePath, "Saved RAW to DB");
                            //    if (!isOnline)
                            //        Global.ImportAnalytics.AddBreadcrumb(a.FilePath, "iTunes OFFLINE");
                            //    // save to autoupdate todo list
                            //    if (a.NotBeenUpdated)
                            //        autoupdate.Add(a.BaseDBTrackLibrary);
                            //}
                            //else if (!a.isCompleteDB)
                            //{
                            //    Global.ImportAnalytics.AddBreadcrumb(a.FilePath, "Did not save to DB");
                            //    a.SaveToTrackLibrary(db, a.Status.ToString() + " - " + a.ImportTypeString);
                            //}
                        }
                    }

                    db.SaveChanges();





                    Global.mPlayer.Notifactions.RemoveNotification(iN3);

                    LogSystem.AddEvent(LogID, "Imported files with Clean Downloaded MetaData: " + WorkList.Count(c => c.Status == ManagedFileStatus.itunesImported && c.isCompleteDB == true));
                    //LogSystem.AddDataDump(LogID, "Clean Imports", WorkList.Where(c => c.Status == ManagedFileStatus.itunesImported && c.isCompleteDB == true).Select(s => s.FilePath).ToList());

                    LogSystem.AddEvent(LogID, "Imported files without Downloaded MetaData: " + WorkList.Count(c => c.Status == ManagedFileStatus.Importing && c.isCompleteDB == false));
                    //LogSystem.AddDataDump(LogID, "Files without Downloaded MetaData", WorkList.Where(c => c.Status == ManagedFileStatus.Importing && c.isCompleteDB == false).Select(s => s.FilePath).ToList());

                    LogSystem.AddEvent(LogID, "Files that require attention: " + WorkList.Count(c => (c.Status != ManagedFileStatus.Importing && c.Status != ManagedFileStatus.itunesImported) && c.isCompleteDB == false));
                    LogSystem.AddDataDump(LogID, "Files that require attention", WorkList.Where(c => (c.Status != ManagedFileStatus.Importing && c.Status != ManagedFileStatus.itunesImported) && c.isCompleteDB == false).Select(s => s.FilePath).ToList());

                    LogSystem.AddEvent(LogID, "Files left in limbo: " + db.TrackLibraries.Count(w => w.Type == "Import - Karaoke" || w.Type == "Import - Music" || w.Type == "Import - Video"));
                    LogSystem.AddDataDump(LogID, "Files left in limbo", db.TrackLibraries.Where(w => w.Type == "Import - Karaoke" || w.Type == "Import - Music" || w.Type == "Import - Video").Select(tl => tl.FilePath).ToList());

                    foreach (var a in db.TrackLibraries.Where(w => w.Type == "Import - Karaoke"))
                    {
                        // TODO: Doesn't change type back to normal...
                        // Also won't add back to autoupdate
                        bool removedThisForTesting = false;
                        if (!a.SongLibraries.Any())
                        {
                            a.SongLibraries.Add(new SongLibrary() { SongName = a.FileName });
                            db.Entry(a).State = System.Data.Entity.EntityState.Modified;
                        }
                    }
                    db.SaveChanges();
                } // End USING DB Clause

                if (autoupdate_trackID.Any())
                {
                    //Global.mPlayer.dbAddTrackToPlaylist(autoupdate_trackID, Global.AutoUpdateTodoListPlaylistId);
                }


                if (tempPlaylistBatch_trackID.Any())
                {
                    //Global.mPlayer.dbAddTrackToPlaylist(tempPlaylistBatch_trackID, playlistID);
                }


            }


            #endregion save remainder to playlist and songs

            // grab artist? 
            Global.importFactory.SessionSaveChanges();
            Global.mPlayer.ShowNotification("Finished Importing Files");
            Thread.Sleep(1000);
            Global.MainWindow.lblNowPlaying.Dispatcher.Invoke(new Action(() =>
            {
                Global.MainWindow.lblNowPlaying.Visibility = System.Windows.Visibility.Collapsed;
            }));

            if (isUpdate)
                Global.isAutoUpdating = false;


            // Important! Notify Import Factory that the next batch can start importing
            Global.ImportAnalytics.CloseBatch(analyticBatchId);
            Global.importFactory.ImportQue.FinishedBatch();

            //this.Dispatcher.Invoke(() =>
            Global.MainWindow.Dispatcher.Invoke(() =>
            {
                if (Global.MainFrame.Content != null && Global.MainFrame.Content.GetType().Name == "FileManagerV3")
                    ((FileManagerV3)Global.MainFrame.Content).isBusyLoading = false;

                if (Global.importFactory.MismatchSongBuffer.Any() || Global.importFactory.MismatchAlbumBuffer.Any())
                {
                    Global.MainWindow.hasUserAlerts = true;
                }
                else
                    Global.MainWindow.hasUserAlerts = false;
            });

            if (Global.importFactory.DisableImportValidation)
            {
                Global.ForceOnlineTimeout(true);
                Global.CheckOnlineStatus();
                Global.importFactory.DisableImportValidation = false;
            }
            else if (!Global.isOnline)
            {
                LogSystem.AddEvent(LogID, "Switched to Offline during Import Process");
            }

            //using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
            //{
            //    int libCount1 = db.LibraryViews.Count();
            //    int libCount2 = db.LibraryViews.Count();
            //}

            //CreateLibraryCarousel();
        }

        private void CreateLibraryCarousel()
        {
            try
            {
                using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                {
                    var songs = db.SongLibraries.Where(x => x.TrackLibraries.Any(c => c.Type.Contains(Global.AppModeString))).ToList();

                    var songClean = (from st2 in songs
                                     where st2.AlbumLibraries.Any() && st2.AlbumLibraries.All(w => w.isHidden == true) == false
                                     select st2);

                    // Get Albums...
                    var albums = (from st2 in songClean
                                  where st2.AlbumLibraries.Any()
                                  group st2 by st2.AlbumLibraries.Where(w => w.isHidden == false).First() into g
                                  select new { album = g.Key, tracks = g }).ToList().Distinct().ToList();
                    // add album tracks
                    var SourceTracks = (from a in albums
                                        select new PlayListSource() { SourceAlbumLibrary = a.album, SourceTracks = a.tracks.ToList(), isAlbumSelected = false }).ToList();



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

                    var FilterTracks = SourceTracks.ToList();
                    if (FilterTracks.Any())
                    {
                        var albumTracks = FilterTracks.Where(x => !x.isTracksOnly).ToList();
                        int i = 0;
                        bool hasMore = true;
                        Global.MainWindow.Dispatcher.Invoke(() =>
                        {
                            while (hasMore)
                            {
                                var uniformGrid = new UniformGrid { Rows = 2, Columns = 6 };
                                foreach (var al in albumTracks.Skip(12 * i).Take(12))
                                {
                                    var artists = (from x in db.AlbumLibraries.Find(al.SourceAlbumLibrary.AlbumId).SongLibraries
                                                   where x.TrackLibraries.Any(a => a.Type == Global.AppModeString)
                                                   select x.Artists.FirstOrDefault()).ToList();
                                    string artistname = artists.First().ArtistName;
                                    // is selected
                                    var isSelected = false;
                                    LibraryCard card = new LibraryCard(al.SourceAlbumLibrary, al.SourceTracks.ToList(), artistname, Global.MainWindow.CurrentLibraryInstnace, isSelected);
                                    card.Margin = new Thickness(5);

                                    uniformGrid.Children.Add(card);
                                }

                                i++;
                                hasMore = albumTracks.Skip(12 * i).Take(12).ToList().Count > 0;
                                Global.MainWindow.LibraryContentCarousel.Add(uniformGrid);

                            }
                            Global.MainWindow.MaxLibraryCarouselIndex = i;
                        });



                    }
                }

            }
            catch (Exception ex)
            {

            }
        }
        private async void ImportTracksAsync(List<ManagedFile> managedFiles, string playlistname = "", bool copyToDefaultFolder = false)
        {

            bool DisableFolderAlbumSearch = true;
            bool DisableITunesUpdate = true;

            // batch..
            // Check for ID3's
            // Some id3s have no data
            List<ManagedFile> WorkList = managedFiles;
            int trackcount = WorkList.Count();

            var noId3 = from i in managedFiles
                        where i.hasValidId3 == false
                        select i;

            int noId3Count = noId3.Count();







            var id3 = from i in managedFiles
                      where i.hasValidId3 == true
                      select i;
            int id3Count = id3.Count();

            #region has ID3


            List<AlbumBufferItem> AlbumBuffer = new List<AlbumBufferItem>();

            // Try to get the album results, don't worry about not finding an album now
            foreach (var i3 in id3)
            {
                Logs.StartActionSet();
                if (!AlbumBuffer.Where(w => w.AlbumName == i3.Id3Tag.Album).Any())
                {
                    // Track Details
                    Logs.WriteActionset(i3.FullFileName);
                    Logs.WriteActionset("Starting Album Search..");

                    iManagedTrack iTrack = new iManagedTrack(i3);
                    iTrack.SetAlbumSearch();

                    // Search Album
                    while (iTrack.isSearching)
                    {
                        string artApi = getApi_SearchAlbum(iTrack.searchName.SearchTerm, 100, "");
                        // TODO: Testing API Call!
                        try { iTrack.AlbumResults = await MakeAPICall<AlbumResult>(artApi); iTrack.NextSearchTerm(); }
                        catch
                        {
                            // TODO: Testing API Call!
                            Global.SetOnlineTimeout(iTrack.BufferManagedFile.FilePath);
                            break;
                        }
                    }

                    string noresults;

                    if (iTrack.HasAlbumResult)
                        AlbumBuffer.Add(new AlbumBufferItem() { BaseAlbum = iTrack.BaseAlbum, AlbumName = i3.Id3Tag.Album, ArtistName = i3.Id3Tag.Artists, SearchTerm = iTrack.searchName.SearchTerm });
                    else
                    {
                        AlbumBuffer.Add(new AlbumBufferItem() { BaseAlbum = iTrack.BaseAlbum, AlbumName = i3.Id3Tag.Album, ArtistName = i3.Id3Tag.Artists, SearchTerm = iTrack.searchName.SearchTerm, isFail = true });
                        noresults = "no albumresuults!";
                    }
                }
            }


            // ***************************
            // Match Album Results with tracks
            List<ManagedFile> updatedTracks = new List<ManagedFile>();
            foreach (AlbumBufferItem a in AlbumBuffer.Where(w => w.isFail == false))
            {
                string albumApi = getApi_GetAlbumSongs(a.BaseAlbum.CollectionId.ToString(), 100, "");
                SongResult albumResults;
                // TODO: Testing API Call!
                try { albumResults = await MakeAPICall<SongResult>(albumApi); }
                catch
                {
                    // TODO: Testing API Call!
                    Global.SetOnlineTimeout();
                    break;
                }
                List<Song> s = albumResults.Songs;

                foreach (ManagedFile m in managedFiles)
                {

                    if (!m.isiTunesReady && m.hasValidId3)
                    {
                        if (getTrackEqual(m.Id3Tag.Album, a.AlbumName) == CompareResults.Match)
                        {
                            Regex rgx = new Regex("[^a-zA-Z0-9 ]");

                            var x = s.Where(w => w.TrackName != null && rgx.Replace(w.TrackName.ToLower(), "") == rgx.Replace(m.Id3Tag.Title.Value.ToLower(), "") && w.TrackNumber == m.Id3Tag.Track.Value);
                            if (x.Any())
                            {
                                m.isiTunesReady = true;
                                m.iTunesAlbum = a.BaseAlbum;
                                m.iTunesSong = x.First();
                                s.Remove(x.First());
                            }
                            else
                            {
                                //Regex rgx = new Regex("[^a-zA-Z0-9 -]");
                                // Might be file name.. somehow
                                string nameX = m.Id3Tag.Title.Value.ToLower();

                                FileInfo f = new FileInfo(nameX);
                                if (Global.musicformats.Contains(f.Extension) || Global.karaokeformats.Contains(f.Extension) || Global.videoformats.Contains(f.Extension))
                                {
                                    nameX = f.Name;
                                    nameX = nameX.Replace(f.Extension, "").ToLower();
                                }

                                nameX = rgx.Replace(nameX, "");

                                // same track, same album... ignore name?

                                var x1 = s.Where(w => w.TrackName != null && rgx.Replace(w.TrackName.ToLower(), "").Contains(nameX)).ToList();
                                var x2 = s.Where(w => w.TrackName != null && rgx.Replace(w.TrackName.ToLower(), "").Contains(nameX) && w.TrackNumber == m.Id3Tag.Track.Value).ToList();
                                var x3 = s.Where(w => w.TrackName != null && w.TrackNumber == m.Id3Tag.Track.Value).ToList();

                                // replace name
                                if (x3.Any())
                                {
                                    m.isiTunesReady = true;
                                    m.iTunesAlbum = a.BaseAlbum;
                                    m.iTunesSong = x3.First();
                                    s.Remove(x3.First());
                                }
                            }
                        }
                    }
                }

                string done = "done!";
            }

            // Remove found items from list
            // 


            // are these the actual ones that didn't find albums?
            int notfound = managedFiles.Where(w => !w.isiTunesReady && w.hasValidId3).Count();
            List<ManagedFile> mX = managedFiles.Where(w => !w.isiTunesReady && w.hasValidId3).ToList();
            int iMX = mX.Count();

            // Save to DB
            List<ManagedFile> importReady = managedFiles.Where(w => w.isiTunesReady).ToList();
            foreach (ManagedFile m in importReady)
            {
                WorkList.Remove(m);
                iManagedTrack iTrack = new iManagedTrack(m);
                iTrack.SaveResultsData(m.iTunesSong, m);
            }

            trackcount = WorkList.Count();


            #endregion has ID3

            #region Find Album From Folder Name


            if (DisableFolderAlbumSearch == false)
            {
                var gDir = from a in WorkList
                           group a by a.FolderName into g
                           select new { albumDir = g.Key, track = g.ToList() };

                AlbumBuffer = new List<AlbumBufferItem>();
                foreach (var b in gDir)
                {
                    if (!AlbumBuffer.Where(w => w.FolderName == b.albumDir).Any())
                    {
                        // Track Details
                        iManagedTrack iTrack = new iManagedTrack();
                        iTrack.SetAlbumSearch(b.albumDir);

                        // Search Album
                        while (iTrack.isSearching)
                        {
                            string artApi = getApi_SearchAlbum(iTrack.searchName.SearchTerm, 100, "");
                            // TODO: Testing API Call!
                            try { iTrack.AlbumResults = await MakeAPICall<AlbumResult>(artApi); iTrack.NextSearchTerm(); }
                            catch
                            {
                                // TODO: Testing API Call!
                                Global.SetOnlineTimeout(iTrack.BufferManagedFile.FilePath);
                                break;
                            }
                        }

                        string noresults;

                        if (iTrack.HasAlbumResult)
                            AlbumBuffer.Add(new AlbumBufferItem() { BaseAlbum = iTrack.BaseAlbum, FolderName = "", SearchTerm = iTrack.searchName.SearchTerm });
                        else
                        {
                            AlbumBuffer.Add(new AlbumBufferItem() { BaseAlbum = iTrack.BaseAlbum, FolderName = "", SearchTerm = iTrack.searchName.SearchTerm, isFail = true });
                            noresults = "no albumresuults!";
                        }
                    }
                }

                // ***************************
                // Match Album Results with tracks

                // BROKEN!
                foreach (AlbumBufferItem a in AlbumBuffer.Where(w => w.isFail == false))
                {
                    string albumApi = getApi_GetAlbumSongs(a.BaseAlbum.CollectionId.ToString(), 100, "");
                    SongResult albumResults;
                    // TODO: Testing API Call!
                    try { albumResults = await MakeAPICall<SongResult>(albumApi); }
                    catch
                    {
                        // TODO: Testing API Call!
                        Global.SetOnlineTimeout();
                        break;
                    }

                    List<Song> s = albumResults.Songs;

                    foreach (ManagedFile m in WorkList)
                    {

                        if (!m.isiTunesReady && m.hasValidId3)
                        {
                            if (getTrackEqual(m.Id3Tag.Album, a.AlbumName) == CompareResults.Match)
                            {
                                Regex rgx = new Regex("[^a-zA-Z0-9 ]");

                                var x = s.Where(w => w.TrackName != null && rgx.Replace(w.TrackName.ToLower(), "") == rgx.Replace(m.Id3Tag.Title.Value.ToLower(), "") && w.TrackNumber == m.Id3Tag.Track.Value);
                                if (x.Any())
                                {
                                    m.isiTunesReady = true;
                                    m.iTunesAlbum = a.BaseAlbum;
                                    m.iTunesSong = x.First();
                                    s.Remove(x.First());
                                }
                                else
                                {
                                    //Regex rgx = new Regex("[^a-zA-Z0-9 -]");
                                    // Might be file name.. somehow
                                    string nameX = m.Id3Tag.Title.Value.ToLower();

                                    FileInfo f = new FileInfo(nameX);
                                    if (Global.musicformats.Contains(f.Extension) || Global.karaokeformats.Contains(f.Extension) || Global.videoformats.Contains(f.Extension))
                                    {
                                        nameX = f.Name;
                                        nameX = nameX.Replace(f.Extension, "").ToLower();
                                    }

                                    nameX = rgx.Replace(nameX, "");

                                    // same track, same album... ignore name?

                                    var x1 = s.Where(w => w.TrackName != null && rgx.Replace(w.TrackName.ToLower(), "").Contains(nameX)).ToList();
                                    var x2 = s.Where(w => w.TrackName != null && rgx.Replace(w.TrackName.ToLower(), "").Contains(nameX) && w.TrackNumber == m.Id3Tag.Track.Value).ToList();
                                    var x3 = s.Where(w => w.TrackName != null && w.TrackNumber == m.Id3Tag.Track.Value).ToList();

                                    // replace name
                                    if (x3.Any())
                                    {
                                        m.isiTunesReady = true;
                                        m.iTunesAlbum = a.BaseAlbum;
                                        m.iTunesSong = x3.First();
                                        s.Remove(x3.First());
                                    }
                                }
                            }
                        }
                    }

                    string done = "done!";
                }

                // Remove found items from list
                // 


                // Save to DB
                //List<ManagedFile> importReady = managedFiles.Where(w => w.isiTunesReady).ToList();
                //foreach (ManagedFile m in importReady)
                //{
                //    WorkList.Remove(m);
                //    iManagedTrack iTrack = new iManagedTrack(m);
                //    iTrack.SaveResultsData(m.iTunesSong, m);
                //}

                trackcount = WorkList.Count();
            }




            #endregion Find Album From Folder Name




            #region doesn't have ID3


            if (DisableITunesUpdate == true)
            {
                List<AlbumBufferItem> AlbumBuffer2 = new List<AlbumBufferItem>();

                // No real way to find the albums...
                foreach (var i3 in WorkList)
                {
                    string noid3here = "No id3";
                    iManagedTrack iTrack = new iManagedTrack(i3);
                    iTrack.SetArtistSearch();
                    // GetSearch API
                    // Find Artist First!
                    string artApi = getApi_SearchArtist(iTrack.searchName.SearchTerm, 100, "");

                    // TODO: Testing API Call!
                    try { iTrack.ArtistResults = await MakeAPICall<SongArtistResult>(artApi); }
                    catch
                    {
                        // TODO: Testing API Call!
                        Global.SetOnlineTimeout(iTrack.BufferManagedFile.FilePath);
                        break;
                    }

                    if (!iTrack.HasArtist)
                        while (iTrack.isSearching)
                        {
                            // continue search for artist
                            artApi = getApi_SearchArtist(iTrack.searchName.SearchTerm, 100, "");
                            // TODO: Testing API Call!
                            try { iTrack.ArtistResults = await MakeAPICall<SongArtistResult>(artApi); }
                            catch
                            {
                                // TODO: Testing API Call!
                                Global.SetOnlineTimeout(iTrack.BufferManagedFile.FilePath);
                                break;
                            }
                            // cycle index automatically
                            if (iTrack.HasTrack)
                                break;
                            else
                                iTrack.NextSearchTerm();
                        }

                    if (iTrack.HasArtist)
                    {
                        while (iTrack.isSearching)
                        {
                            string api = getApi_SearchSong(iTrack.searchName.SearchTerm, 100, "");
                            // TODO: Testing API Call!
                            try { iTrack.SongResults = await MakeAPICall<SongResult>(api); }
                            catch
                            {
                                // TODO: Testing API Call!
                                Global.SetOnlineTimeout(iTrack.BufferManagedFile.FilePath);
                                break;
                            }

                            // cycle index automatically
                            if (iTrack.HasTrack)
                                break;
                            else
                                iTrack.NextSearchTerm();
                        }

                        if (!iTrack.HasTrack)
                        {
                            // Could not find Artist
                            Global.ImportAnalytics.AddBreadcrumb(iTrack.BufferManagedFile.FilePath, "iTunes:Found Artist, could not find Track");
                            Global.ImportAnalytics.AddSupportingMetaData(iTrack.BufferManagedFile.FilePath, "iTunes:Found Artist, could not find Track", iTrack.searchNames.Select(s => s.SearchTerm).ToList());
                        }
                    }
                    else
                    {
                        // Could not find Artist
                        Global.ImportAnalytics.AddBreadcrumb(iTrack.BufferManagedFile.FilePath, "iTunes:Could not find Artist");
                        Global.ImportAnalytics.AddSupportingMetaData(iTrack.BufferManagedFile.FilePath, "iTunes:Could not find Artist", iTrack.searchNames.Select(s => s.SearchTerm).ToList());
                    }
                }
            }


            Global.importFactory.SessionSaveChanges();
            Global.isUpdatingLibrary = false;


            #endregion doesn't have ID3


            if (WorkList.Any())
            {
                // Popup Smart Updater
                Global.mPlayer.popUp_Grid.Visibility = System.Windows.Visibility.Visible;
                Global.mPlayer.popUp_Frame.Content = new Popup_SmartUpdater(WorkList);
            }

            // Import as is
            foreach (var i3 in WorkList)
            {

            }



            //Global.mPlayer.ShowNotification("Finished Importing Files");
        }

        private class AlbumBufferItem
        {
            public Album BaseAlbum { get; set; }
            public string SearchTerm { get; set; }
            public string AlbumName { get; set; }
            public string ArtistName { get; set; }

            public string FolderName { get; set; }

            public bool isFail = false;

            public string FileNameFormat { get; set; }
        }

        internal async void UpdateTrack2(SongLibrary baseSong)
        {
            // TODO: Activate Thread Pool
            //ThreadPool.QueueUserWorkItem(_ => UpdateAlbumTrackAsync(baseSong));

            // Open Factory Session
            Global.importFactory.SessionOpen();
            //Global.importFactory.SessionOpen(baseSong.SongId);

            UpdateSearchTrackAsync(baseSong);
            // Close Factory Session
        }

        private async void UpdateSearchTrackAsync(SongLibrary song)
        {
            Global.isUpdatingLibrary = true;


            //AnalysisFile analysisFile = Global.importFactory.NewAnalysisFile(song);

            //analysisFile.SetArtist();
            //analysisFile.AddArtistSearchBuffer("", "");


            iManagedTrack iTrack = new iManagedTrack(song);


            // GetSearch API
            // Find Artist First!
            string artApi = getApi_SearchArtist(iTrack.searchName.SearchTerm, 100, "");
            // TODO: Testing API Call!
            try { iTrack.ArtistResults = await MakeAPICall<SongArtistResult>(artApi); }
            catch
            {
                // TODO: Testing API Call!
                Global.SetOnlineTimeout(iTrack.BufferManagedFile.FilePath);
                //break;
            }

            while (iTrack.isSearching)
            {
                string api = getApi_SearchSong(iTrack.searchName.SearchTerm, 100, "");
                // TODO: Testing API Call!
                try { iTrack.SongResults = await MakeAPICall<SongResult>(api); }
                catch
                {
                    // TODO: Testing API Call!
                    Global.SetOnlineTimeout(iTrack.BufferManagedFile.FilePath);
                    break;
                }

                // cycle index automatically
                if (iTrack.HasTrack)
                    break;
                else
                    iTrack.NextSearchTerm();
            }

            if (!iTrack.HasTrack)
            {
                // Save Suggested Songs + Details to Buffer


            }

            Global.importFactory.SessionSaveChanges();
            Global.isUpdatingLibrary = false;
        }

        internal void SearchTrack(string track, SongArtist artist, ManagedFile mFile)
        {
            //ThreadPool.QueueUserWorkItem(_ => SearchArtistAsync(artist));
            SearchTrackAsync(track, artist, mFile);

        }
        private async void SearchTrackAsync(string track, SongArtist artist, ManagedFile mFile)
        {
            Global.importFactory.SessionOpen();

            iManagedTrack iTrack = new iManagedTrack(mFile);
            iTrack.BaseSongArtist = artist;

            if (artist == null)
                iTrack.SetTrackSearchVerbatum(track);
            else
            {
                iTrack.HasArtist = true;
                iTrack.SetTrackSearch(track);
            }

            iTrack.AutoSaveOff();

            // GetSearch API
            string trackApi = getApi_SearchSong(iTrack.searchName.SearchTerm, 100, "");

            // TODO: Testing API Call!
            try { iTrack.SongResults = await MakeAPICall<SongResult>(trackApi); }
            catch
            {
                // TODO: Testing API Call!
                Global.SetOnlineTimeout(iTrack.BufferManagedFile.FilePath);
                //   break;
            }

            if (iTrack.HasTrack)
            {
                var x = (Popup_SmartUpdater)Global.mPlayer.popUp_Frame.Content;

                // because it auto saves?
                x.NewBaseSong = iTrack.BaseSong;
            }
            else
            {
                var x = (Popup_SmartUpdater)Global.mPlayer.popUp_Frame.Content;
                x.CannotFindTrack();
            }
        }
        internal void SearchArtist(string artist)
        {
            //ThreadPool.QueueUserWorkItem(_ => SearchArtistAsync(artist));
            SearchArtistAsync(artist);
        }

        private async void SearchArtistAsync(string artist)
        {
            iManagedTrack iTrack = new iManagedTrack();
            iTrack.SetArtistSearch(artist);

            // GetSearch API
            string artApi = getApi_SearchArtist(iTrack.searchName.SearchTerm, 100, "");
            try { iTrack.ArtistResults = await MakeAPICall<SongArtistResult>(artApi); }
            catch
            {
                // TODO: Testing API Call!
                Global.SetOnlineTimeout();
                //  break;
            }

            if (iTrack.HasArtist)
            {
                var x = (Popup_SmartUpdater)Global.mPlayer.popUp_Frame.Content;
                x.NewBaseArtist = iTrack.BaseSongArtist;
            }
            else
            {
                var x = (Popup_SmartUpdater)Global.mPlayer.popUp_Frame.Content;
                x.CannotFindArtist();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="albumId">iTunes ID</param>
        internal void SearchAlbum(string albumId)
        {
            SearchAlbumAsync(albumId);
        }
        internal void SearchAlbum(ManagedFile mf, string albumName, string artistName, string artistId)
        {
            SearchAlbumAsync(mf, albumName, artistName, artistId);
        }

        private async void SearchAlbumAsync(string collectionId)
        {
            // pull all album songs
            string albumApi = getApi_GetAlbum(collectionId, 100, "");
            // TODO: Testing API Call!
            AlbumResult album = new AlbumResult();
            try { album = await MakeAPICall<AlbumResult>(albumApi); }
            catch
            {
                // TODO: Testing API Call!
                Global.SetOnlineTimeout();
                //  break;
            }

            albumApi = getApi_GetAlbumSongs(collectionId, 100, "");
            SongResult albumResults = new SongResult(); ;
            // TODO: Testing API Call!
            try { albumResults = await MakeAPICall<SongResult>(albumApi); }
            catch
            {
                // TODO: Testing API Call!
                Global.SetOnlineTimeout();
                //  break;
            }
            List<Song> s = albumResults.Songs;

            var x = (Popup_SmartUpdater)Global.mPlayer.popUp_Frame.Content;
            x.NewBaseAlbum = album.Albums.First();
            x.NewBaseAlbumResult = albumResults;
        }
        private async void SearchAlbumAsync(ManagedFile mf, string albumName, string artistName, string artistId)
        {
            iManagedTrack iTrack = new iManagedTrack(mf);

            if (string.IsNullOrEmpty(artistName))
                iTrack.SetAlbumSearch(albumName);
            else
                iTrack.SetAlbumSearch(artistName + " - " + albumName);

            // Search Album
            string artApi = getApi_SearchAlbum(iTrack.searchName.SearchTerm, 100, "");
            // TODO: Testing API Call!
            try { iTrack.AlbumResults = await MakeAPICall<AlbumResult>(artApi); }
            catch
            {
                // TODO: Testing API Call!
                Global.SetOnlineTimeout();
                //   break;
            }
            if (iTrack.HasAlbumResult)
            {
                // pull all album songs
                string albumApi = getApi_GetAlbumSongs(iTrack.BaseAlbum.CollectionId.ToString(), 100, "");
                SongResult albumResults = new SongResult();
                try { albumResults = await MakeAPICall<SongResult>(albumApi); }
                catch
                {
                    // TODO: Testing API Call!
                    Global.SetOnlineTimeout();
                    // break;
                }

                List<Song> s = albumResults.Songs;

                var x = (Popup_SmartUpdater)Global.mPlayer.popUp_Frame.Content;
                x.NewBaseAlbum = iTrack.BaseAlbum;
                x.NewBaseAlbumResult = albumResults;
            }
            else
            {
                artApi = getApi_GetArtistAlbums(artistId, 100, "");
                AlbumResult results = new AlbumResult();
                try { results = await MakeAPICall<AlbumResult>(artApi); }
                catch
                {
                    // TODO: Testing API Call!
                    Global.SetOnlineTimeout();
                    // break;
                }

                if (results.Count == 0)
                {
                    var x = (Popup_SmartUpdater)Global.mPlayer.popUp_Frame.Content;
                    x.CannotFindAlbum();
                }
                else
                {
                    bool hasAlbum = false;
                    foreach (var a in results.Albums)
                    {

                        switch (getTrackEqual(a.CollectionName, albumName))
                        {
                            case CompareResults.Match:
                                var s1 = a.CollectionName;

                                // pull all album songs
                                string albumApi = getApi_GetAlbumSongs(a.CollectionId.ToString(), 100, "");
                                SongResult albumResults = new SongResult();
                                try { albumResults = await MakeAPICall<SongResult>(albumApi); }
                                catch
                                {
                                    // TODO: Testing API Call!
                                    Global.SetOnlineTimeout();
                                    break;
                                }

                                List<Song> s = albumResults.Songs;

                                var x = (Popup_SmartUpdater)Global.mPlayer.popUp_Frame.Content;
                                x.NewBaseAlbum = a;
                                x.NewBaseAlbumResult = albumResults;
                                hasAlbum = true;
                                break;
                            case CompareResults.MatchWithExtra:
                                var s2 = a.CollectionName;
                                break;
                            case CompareResults.MatchSpellingError:
                                var s3 = a.CollectionName;
                                break;
                            case CompareResults.NoMatch:
                                break;
                        }

                        if (hasAlbum)
                            break;
                    }
                    if (hasAlbum == false)
                    {
                        var x = (Popup_SmartUpdater)Global.mPlayer.popUp_Frame.Content;
                        x.CannotFindAlbum();
                    }

                }
            }


        }

        public class CompareEngine
        {
            public CompareEngine()
            {

            }

            /// Where Tracknames Match
            public List<Song> MatchedTracks { get; set; }
            public static string ExpectedMatch { get; internal set; }
            public string ExtractedTrackName
            {
                get
                {

                    return "";
                }
            }

            public bool IsBroken { get; internal set; }

            internal static void AddAttributes(string v)
            {

            }

            bool HasArtist = false;
            string ArtistName = "";
            string TrackTerm = "";

            internal void CompareStringResult(string v, Song s)
            {
                // Check Artist Match
                if (!HasArtist || ArtistName == s.ArtistName) { }

                if (getTrackEqual(TrackTerm, s.TrackName) == CompareResults.Match) { }
                else if (getTrackContains(TrackTerm, s.TrackName)) { }
            }
        }


        public class TrackCompareResults
        {
            public bool isExactMatch { get; set; }
            public bool isPartialMatch { get; set; }

        }
        //public static TrackCompareResults getTrackCompare(string searchTerm, string trackName)
        //{
        //    TrackCompareResults c = new TrackCompareResults();
        //    string s1 = searchTerm.ToLower().Replace("  ", " ").Trim();
        //    string s2 = trackName.ToLower().Replace("  ", " ").Trim();

        //    c.isExactMatch = s1 == s2;
        //    if (!c.isExactMatch)
        //    {

        //    }

        //    return c;
        //}
        public string getlowercompare(string s)
        {
            return s.ToLower().Replace(" ", "");
        }
        public static byte[] getDBCoverArt(string ArtworkUrl100)
        {
            string artworkUrl = ArtworkUrl100.Replace("100x100bb", "500x500bb");
            try
            {
                byte[] imageData = new WebClient().DownloadData(artworkUrl);
                return imageData;
            }
            catch
            {
                return null;
            }
        }
        public static int getReleaseDateYear(string ReleaseDate)
        {
            // "2018-05-10T12:00:00Z"
            return int.Parse(ReleaseDate.Substring(0, 4));
        }
        public enum CompareResults
        {
            NoMatch,
            Match,
            MatchWithExtra,
            MatchSpellingError
        }
        /// <summary>
        /// Replaces all double spaces, new lines, tabs, ect. with single space
        /// </summary>
        /// <param name="String"></param>
        /// <returns></returns>

        /// TrackNumber - Artist - Track - Album
        public static List<string> StringFileNameFormats = new List<string>() { "{0} - {1} - {2}", "{0} - {2}", "{3} - {2}" };
        public static string SuperTrim(string String)
        {
            string myString = Regex.Replace(String, @"\s+", " ");
            myString = myString.Replace("\0", "");
            return myString.Trim();
        }
        public static CompareResults getTrackEqual(string searchTerm, string trackName)
        {
            Regex rgx = new Regex("[^a-zA-Z0-9 ]");
            if (string.IsNullOrEmpty(trackName))
                trackName = "";
            if (string.IsNullOrEmpty(searchTerm))
                searchTerm = "";
            string s1 = searchTerm.ToLower().Replace('-', ' ');
            s1 = rgx.Replace(s1, "").Replace(",", "").Replace(".", "");
            s1 = SuperTrim(s1);
            string s2 = trackName.ToLower();
            s2 = rgx.Replace(s2, "").Replace(",", "").Replace(".", "");
            s2 = SuperTrim(s2);
            string s3 = "";
            string s4 = trackName.ToLower().Replace('-', ' ').Trim();
            s4 = SuperTrim(s4);

            if (trackName.Contains("(") && trackName.Contains(")"))
            {
                int start = trackName.IndexOf("(");
                int end = trackName.LastIndexOf(")");
                string removebit = trackName.Substring(start, end - start + 1);
                s3 = trackName.Replace(removebit, "");
                s3 = s3.ToLower();
                s3 = rgx.Replace(s3, "");
                s3 = SuperTrim(s3);
            }

            if (s1 == s2 || s1 == s4)
                return CompareResults.Match;
            else if (s1 == s3 && string.IsNullOrEmpty(s3) == false)
                return CompareResults.MatchWithExtra;


            int s1N = 0;
            int s2N = 0;

            if (int.TryParse(s1, out s1N) && int.TryParse(s2, out s2N))
                if (s1N == s2N)
                    return CompareResults.Match;
                else
                    return CompareResults.NoMatch;


            bool isSpellingError = false;

            if (s1.Split(' ').Count() == s2.Split(' ').Count())
            {
                List<string> s1x = s1.Split(' ').ToList();
                List<string> s2x = s2.Split(' ').ToList();
                if (s1x.Count > 1)
                {
                    int match = 0;
                    int mismatch = 0;
                    int startmatch = 0;

                    for (int i = 0; i < s1x.Count; i++)
                    {
                        int maxi1 = s1x[i].Length - 1;
                        int maxi2 = s2x[i].Length - 1;

                        if (s1x[i] == s2x[i])
                            match++;
                        else if (s1x[i][0] == s2x[i][0] && s1x[i][maxi1] == s2x[i][maxi2])
                            startmatch++;
                        else
                            mismatch++;
                    }
                    if (mismatch == 0)
                        isSpellingError = true;
                    else if (match > mismatch + startmatch)
                        isSpellingError = true;
                    else if ((match > s1x.Count - 1) || (match > s1x.Count - 2 && match > 4))
                        isSpellingError = true;
                }
            }

            if (isSpellingError)
                return CompareResults.MatchSpellingError;
            else
                return CompareResults.NoMatch;
        }
        public static bool getTrackContains(string searchTerm, string trackName)
        {
            string s1 = searchTerm.ToLower().Replace("  ", " ").Trim();
            string s2 = trackName.ToLower().Replace("  ", " ").Trim();

            bool b1 = s2.Contains(s1);
            bool b2 = s1.Contains(s2);

            return b1 || b2;
        }
        public static CompareResults getArtistEqual(string searchTerm, string artistName)
        {
            return getTrackEqual(searchTerm, artistName);
        }
        private static AlbumLibrary ConvertToAlbum(Song song, bool isHidden = false)
        {
            AlbumLibrary album = new AlbumLibrary();

            album.AlbumName = song.CollectionName;
            album.iTunesId = song.CollectionId.ToString();
            album.NumTracks = song.TrackCount;
            if (song.ReleaseDate != null)
                album.Year = getReleaseDateYear(song.ReleaseDate);
            album.isVerified = true;

            if (isHidden)
            {
                album.isHidden = true;
            }
            else
            {
                album.CoverArt = getDBCoverArt(song.ArtworkUrl100);
                album.hasiTunesUpdate = true;
            }

            return album;
        }

        internal async void UpdateAlbumTrackAsync(SongLibrary song)
        {
            Global.isUpdatingLibrary = true;


            // GetSearch API
            string api = getApi_SearchSong(song.SongName, 100, "");

            // Search iTunes
            Debug.WriteLine("Starting API CALL to iTunes");
            SongResult results = new SongResult();
            try { results = await MakeAPICall<SongResult>(api); }
            catch
            {
                // TODO: Testing API Call!
                Global.SetOnlineTimeout();
                // break;
            }

            List<Song> filtered = new List<Song>();
            foreach (Song a in results.Songs)
            {
                if (getlowercompare(a.TrackName) == getlowercompare(song.SongName))
                {
                    filtered.Add(a);
                }
            }

            // Has single result ? Score!
            if (filtered.Count == 1)
            {
                Debug.WriteLine("iTunes : Found 1 Result");
                var i = filtered[0];
                using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                {
                    var s = db.SongLibraries.Find(song.SongId);
                    s.SongName = i.TrackName;


                    var art = s.Artists.First();
                    art.ArtistName = i.ArtistName;
                    art.iTunesId = i.ArtistId.ToString();
                    art.isVerified = true;
                    art.hasiTunesUpdate = true;


                    db.Entry(art).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();

                    db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    //a.Year = i.ReleaseDate;
                    //i.PrimaryGenreName;


                    // TODO : Pull album Art
                    //i.ArtworkUrl100

                    Global.mPlayer.ShowNotification("Updated track from iTunes");
                    Thread.Sleep(1000);
                    Global.MainWindow.lblNowPlaying.Dispatcher.Invoke(new Action(() => { Global.MainWindow.lblNowPlaying.Visibility = System.Windows.Visibility.Collapsed; }));
                }

                // TODO : Update Tracks + Save ID3's
                // TODO : Update Artist Details
            }
            else
            {
                Global.mPlayer.TogglePopUp();

                //libraryPopup.lblSubName.Dispatcher.Invoke(new Action(() => { libraryPopup.lblSubName.Content = "Erm... got weird stuff! Updated From Itunes!"; }));
                //libraryPopup.lblSubName.Content = "Erm... got weird stuff! Updated From Itunes!";
                Debug.WriteLine(String.Format("iTunes : Found other results {0} Result", results.Count));
            }

            // Has multiple results? Save them, create popup choice window

            // Has no results? Search via Tracks

            Global.isUpdatingLibrary = false;
        }


        // search album
        // filter album
        // update album

        // find tracks
        // update tracks
        // find lost tracks
        // 

        public class iManagedTrackFunctions
        {
            public SongResult SongResults
            {
                set
                {
                    _searchSongResults = value;
                    // Filter Results

                    //FilterResults();
                }
            }
            private SongResult _searchSongResults;

            public iManagedTrackFunctions(ImportFactory.ManagedFile mf)
            {
            }
        }

        public class iManagedTrack
        {
            public bool ImportToPlaylist { get { return (ImportPlaylistId != null && ImportPlaylistId > 0); } }
            public int ImportPlaylistId { get; set; }
            public SearchAnalysisRule PrimarySearchOption { get; set; }
            public bool ImportedRule { get; set; }

            public Artist DBArtist { get; set; }
            public SongLibrary DBSong { get; set; }
            public ManagedFile BufferManagedFile { get; set; }
            public bool HasTrack = false;
            public bool HasArtist = false;
            public bool HasAlbumResult = false;
            public Album BaseAlbum { get; set; }
            public SongArtist BaseSongArtist { get; set; }
            public Song BaseSong { get; set; }

            public int searchStartingIndex = 0;
            public SearchAnalysisRule searchName { get; set; }
            public int searchIndex = 0;
            public List<SearchAnalysisRule> searchNames = new List<SearchAnalysisRule>();
            public bool isSearching { get; set; }
            public bool isFail { get; set; }
            public AlbumResult AlbumResults
            {
                set
                {
                    _searchAlbumResults = value;
                    FilterAlbumResults();
                }
            }
            private AlbumResult _searchAlbumResults;

            public SongArtistResult ArtistResults
            {
                set
                {
                    _searchArtistResults = value;

                    // Filter Results
                    FilterArtistResults();
                }
            }
            private SongArtistResult _searchArtistResults;
            public SongResult SongResults
            {
                set
                {
                    _searchSongResults = value;
                    // Filter Results

                    FilterResults();
                }
            }
            private SongResult _searchSongResults;


            public iManagedTrack()
            {

            }
            public iManagedTrack(ManagedFile m)
            {
                BufferManagedFile = m;
                if (m.ImportToPlaylist)
                {
                    ImportPlaylistId = m.ImportPlaylistId;
                }
            }

            public void SetAlbumSearch(string folderDirectory)
            {
                isSearching = true;
                searchNames = new List<SearchAnalysisRule>();
                searchIndex = 0;

                if (!String.IsNullOrEmpty(folderDirectory))
                    searchNames.Add(new SearchAnalysisRule(FilterAlbumName(folderDirectory)));

                searchName = searchNames[searchIndex];
            }

            /// <summary>
            /// To be used with importing ManagedFiles
            /// </summary>
            public void SetAlbumSearch()
            {
                isSearching = true;
                searchNames = new List<SearchAnalysisRule>();
                searchIndex = 0;

                if (BufferManagedFile.hasValidId3)
                {
                    // grab details from id3 tag
                    searchNames.Add(new SearchAnalysisRule(BufferManagedFile.searchArtist.First() + " - " + FilterAlbumName(BufferManagedFile.searchAlbum)));
                    searchNames.Add(new SearchAnalysisRule(FilterAlbumName(BufferManagedFile.searchAlbum)));
                }

                if (!String.IsNullOrEmpty(BufferManagedFile.FolderName))
                    searchNames.Add(new SearchAnalysisRule(FilterAlbumName(BufferManagedFile.FolderName)));

                searchName = searchNames[searchIndex];
            }
            /// <summary>
            /// To be used with importing ManagedFiles
            /// </summary>


            public void SetArtistSearch(List<ManagedFile> files, JukeboxBrainsDBEntities db = null)
            {
                SetArtistSearch("", files, db);
            }
            public void SetArtistSearch(string artistName = "", List<ManagedFile> files = null, JukeboxBrainsDBEntities db = null)
            {
                PrimarySearchOption = new SearchAnalysisRule();

                if (artistName == "")
                    artistName = BufferManagedFile.FileName;

                if (db != null)
                {
                    ExtractSearchArtistList(artistName, files, db);
                }
                else if (files != null)
                {
                    ExtractSearchArtistList(artistName, files);
                }
                else
                {
                    ExtractSearchArtistList(artistName);
                }
            }

            public void SetTrackSearchVerbatum(string trackName)
            {
                PrimarySearchOption = new SearchAnalysisRule();
                if (BaseSongArtist != null)
                {
                    PrimarySearchOption.ArtistName = BaseSongArtist.ArtistName;
                    HasArtist = true;
                }

                PrimarySearchOption.Delimiter = "-";

                isSearching = true;
                searchNames = new List<SearchAnalysisRule>();
                searchIndex = 0;

                if (BaseSongArtist != null)
                    searchNames.Add(new SearchAnalysisRule(PrimarySearchOption.ArtistName + " - ", trackName));
                else
                    searchNames.Add(new SearchAnalysisRule(trackName));
                searchName = searchNames[searchIndex];
            }

            public void SetTrackSearch(string trackName = "")
            {
                PrimarySearchOption = new SearchAnalysisRule();
                PrimarySearchOption.ArtistName = BaseSongArtist.ArtistName;


                if (trackName == "")
                    ExtractSearchArtistList(BufferManagedFile.FileName);
                else
                {
                    isSearching = true;
                    searchNames = new List<SearchAnalysisRule>();
                    searchIndex = 0;

                    if (HasArtist)
                        searchNames = Global.importFactory.aEngine.GetTrackSearchStrings(trackName, true, true, BaseSongArtist.ArtistName);
                    else
                        searchNames = Global.importFactory.aEngine.GetTrackSearchStrings(trackName, true);

                    searchName = searchNames[searchIndex];
                }
            }

            private string FilterAlbumName(string album)
            {
                // xxxx Disk 1
                string a = album;

                List<string> words = album.Split(' ').ToList();
                if (words.Count > 2)
                {
                    string discX = words[words.Count - 2];
                    if (discX.ToLower() == "disc")
                        a = a.Replace(" " + discX + " " + words.Last(), "");
                }

                // Remove brackets
                if (a.Contains("(") && a.Contains(")"))
                {
                    int start = a.IndexOf("(");
                    int end = a.LastIndexOf(")");
                    string removebit = a.Substring(start, end - start + 1);
                    a = a.Replace(removebit, "");
                    a = a.ToLower().Replace("  ", " ").Trim();
                }

                if (a.Contains("[") && a.Contains("]"))
                {
                    int start = a.IndexOf("[");
                    int end = a.LastIndexOf("]");
                    string removebit = a.Substring(start, end - start + 1);
                    a = a.Replace(removebit, "");
                    a = a.ToLower().Replace("  ", " ").Trim();
                }

                return a;
            }

            public iManagedTrack(SongLibrary song)
            {
                DBSong = song;
                PrimarySearchOption = new SearchAnalysisRule();
                //ExtractSearchNameList(song.SongName);
                ExtractSearchArtistList(song.SongName);
            }

            public iManagedTrack(string filename)
            {
                PrimarySearchOption = new SearchAnalysisRule();
                ExtractSearchArtistList(filename);
            }

            #region Filters


            public void FilterAlbumResults()
            {
                List<Album> filteredAlbums = new List<Album>();
                if (_searchAlbumResults.Count == 1)
                {
                    BaseAlbum = _searchAlbumResults.Albums.First();
                    HasAlbumResult = true;
                    isSearching = false;
                }
                else
                {
                    List<Album> exactMatches = new List<Album>();
                    List<Album> exceptionMatches = new List<Album>();

                    foreach (Album a in _searchAlbumResults.Albums)
                    {
                        if (BufferManagedFile != null && BufferManagedFile.Id3Tag != null && BufferManagedFile.Id3Tag.Artists.Value.Count > 0)
                        {
                            foreach (var x in BufferManagedFile.Id3Tag.Artists.Value)
                            {
                                string collectionname = a.CollectionName;
                                if (searchName.SearchTerm.Contains(" - "))
                                    collectionname = x + " - " + collectionname;

                                switch (getTrackEqual(searchName.SearchTerm, collectionname))
                                {
                                    case CompareResults.Match:
                                        exactMatches.Add(a);
                                        break;
                                    case CompareResults.MatchWithExtra:
                                        exceptionMatches.Add(a);
                                        break;
                                }
                            }
                        }
                        else
                        {
                            string hasArtistCollectionNameA = a.ArtistName + " - " + a.CollectionName;
                            string hasArtistCollectionNameB = a.ArtistName + " " + a.CollectionName;
                            bool hasResult = false;

                            switch (getTrackEqual(searchName.SearchTerm, a.CollectionName))
                            {
                                case CompareResults.Match:
                                    exactMatches.Add(a);
                                    hasResult = true;
                                    break;
                                case CompareResults.MatchWithExtra:
                                    exceptionMatches.Add(a);
                                    hasResult = true;
                                    break;
                            }

                            if (!hasResult)
                                switch (getTrackEqual(searchName.SearchTerm, hasArtistCollectionNameA))
                                {
                                    case CompareResults.Match:
                                        exactMatches.Add(a);
                                        hasResult = true;
                                        break;
                                    case CompareResults.MatchWithExtra:
                                        exceptionMatches.Add(a);
                                        hasResult = true;
                                        break;
                                }

                            if (!hasResult)
                                switch (getTrackEqual(searchName.SearchTerm, hasArtistCollectionNameB))
                                {
                                    case CompareResults.Match:
                                        exactMatches.Add(a);
                                        hasResult = true;
                                        break;
                                    case CompareResults.MatchWithExtra:
                                        exceptionMatches.Add(a);
                                        hasResult = true;
                                        break;
                                }
                        }
                    }

                    if (!exactMatches.Any())
                        exactMatches = exceptionMatches;

                    if (exactMatches.Count() == 1)
                    {
                        BaseAlbum = exactMatches.First();
                        HasAlbumResult = true;
                        isSearching = false;
                    }
                    else if (exactMatches.Count() > 1)
                    {
                        // Fail! I'm not dealign with this shit
                        foreach (var eM in exactMatches)
                        {
                            //Logs.WriteActionset(eM.CollectionId + ": " + eM.CollectionName + ", " + eM.ArtistId + ": " + eM.ArtistName + ", Track Count :" + eM.TrackCount + ", Year :" + eM.ReleaseDate);
                        }
                        //Logs.ActionSetFail("More than one album!");
                        isFail = true;
                    }
                }
            }

            public void FilterArtistResults()
            {
                List<SongArtist> filteredArtists = new List<SongArtist>();
                bool keepSearching = true;

                // assuming the only result is the right one

                if (_searchArtistResults.Artists.Count == 1)
                {
                    filteredArtists.Add(_searchArtistResults.Artists.First());
                }
                else
                {
                    // get artistlist

                    foreach (SongArtist a in _searchArtistResults.Artists)
                    {
                        if (getTrackEqual(searchName.SearchTerm, a.ArtistName) == CompareResults.Match)
                            filteredArtists.Add(a);
                    }
                }

                if (filteredArtists.Count() > 1)
                {
                    List<string> uniqueNames = (from x in filteredArtists select x.ArtistName).Distinct().ToList();
                    List<long> artistIDs = (from x in filteredArtists select x.ArtistId).Distinct().ToList();

                    // fails on 1985 - Bolwing For Soup
                    if (uniqueNames.Count() == 1)
                    {
                        List<SongArtist> f2 = new List<SongArtist>();
                        f2.Add(filteredArtists.First());
                        filteredArtists = f2;
                    }
                }

                if (filteredArtists.Count() == 1)
                {
                    // Set Artist
                    BaseSongArtist = filteredArtists.First();
                    HasArtist = true;

                    // save primary search stuff, 
                    PrimarySearchOption = searchName;
                    PrimarySearchOption.ArtistName = BaseSongArtist.ArtistName;
                    PrimarySearchOption.ArtistPartIndex = searchName.ArtistPartIndex;
                    PrimarySearchOption.ArtistSearchTerm = searchName.SearchTerm;

                    // generate new search list for track
                    if (DBSong == null)
                    {
                        if (BufferManagedFile != null)
                            ExtractSearchNameList(BufferManagedFile.FileName);
                    }
                    else
                        ExtractSearchNameList(DBSong.SongName);
                }
                else if (filteredArtists.Count() == 0)
                {
                    // set to no result
                    searchNames[searchIndex].ResultVerdict = SearchAnalysisRule.Verdict.NoResults;
                    //NextSearchTerm();
                }
                else
                {
                    // set to inconclusive
                    searchNames[searchIndex].ResultVerdict = SearchAnalysisRule.Verdict.Inconclusive;
                    searchName.ResultVerdict = SearchAnalysisRule.Verdict.Inconclusive;
                    //NextSearchTerm();
                }
            }

            //CompareEngine ce = new CompareEngine();
            public void FilterResults()
            {
                List<Song> filteredSongs = new List<Song>();
                List<Song> filteredSongsX2 = new List<Song>();
                List<Song> filteredSongsX3 = new List<Song>();

                string artistNameInject = "";
                if (HasArtist)
                    artistNameInject = PrimarySearchOption.ArtistName + " " + PrimarySearchOption.Delimiter + " ";

                CompareEngine.ExpectedMatch = "";
                CompareEngine.AddAttributes("");

                //ce.IsBroken = true;
                foreach (Song s in _searchSongResults.Songs)
                {
                    // check if track name matches the search term
                    //if (getTrackEqual(searchName.TrackTerm, s.TrackName) == CompareResults.Match)
                    //{
                    //    if (!HasArtist || searchName.ArtistName == s.ArtistName)
                    //        filteredSongs.Add(s);
                    //    // check if track name includes search term
                    //    else if (getTrackContains(searchName.TrackTerm, s.TrackName))
                    //        filteredSongsX2.Add(s);
                    //}
                    //else if (getTrackEqual(searchName.TrackTerm, s.TrackName) == CompareResults.MatchWithExtra)
                    //{
                    //    filteredSongsX2.Add(s);
                    //}

                    switch (getTrackEqual(searchName.TrackTerm, s.TrackName))
                    {
                        case CompareResults.Match:
                            if (!HasArtist || searchName.ArtistName == s.ArtistName)
                                filteredSongs.Add(s);
                            // check if track name includes search term
                            else if (getTrackContains(searchName.TrackTerm, s.TrackName))
                                filteredSongsX2.Add(s);
                            break;
                        case CompareResults.MatchWithExtra:
                            filteredSongsX2.Add(s);
                            break;
                        case CompareResults.MatchSpellingError:
                            if (!HasArtist || searchName.ArtistName == s.ArtistName)
                                filteredSongsX3.Add(s);
                            break;
                    }

                    //ce.CompareStringResult(s.TrackName, s);
                    //ce.IsBroken = true;
                }




                // zero hits, next (do nothing)
                if (!filteredSongs.Any())
                {
                    if (_searchSongResults.Count < 100)
                    {
                        /// Partially mismatched
                        if (filteredSongsX3.Any())
                        {
                            Global.importFactory.AddBufferSongMismatch(BufferManagedFile, filteredSongsX3);
                        }
                        if (_searchSongResults.Count < 100 && filteredSongsX2.Any())
                        {
                            // likely not jibberish
                            filteredSongs = filteredSongsX2;
                        }
                    }
                }

                /// Extra filter to check if track number filter is available
                if (filteredSongs.Count() > 1)
                {
                    if (searchNames[PrimarySearchOption.TrackPartIndex].isPossibleTrackNumber)
                    {
                        int t = int.Parse(searchNames[PrimarySearchOption.TrackPartIndex].TrackTerm);
                        var f = filteredSongs.Where(w => w.TrackNumber == t).ToList();

                        filteredSongs = f;
                    }
                }

                /// Extra step to remove albums where track features more than one... yes, this actually happens Alan Walker - Faded
                if (filteredSongs.Count() > 1)
                {
                    var FirstHit = filteredSongs.First();
                    bool hitremoved = false;
                    List<long> albumIDs = filteredSongs.Select(a => a.CollectionId).Distinct().ToList();
                    foreach (long ai in albumIDs)
                    {
                        if (filteredSongs.Count(c => c.CollectionId == ai) > 1)
                        {
                            if (FirstHit.CollectionId == ai) hitremoved = true;
                            filteredSongs.RemoveAll(r => r.CollectionId == ai);
                        }
                    }
                    if (hitremoved)
                        filteredSongs.Insert(0, FirstHit);
                }

                // many hits, create popup
                if (filteredSongs.Count() > 1)
                {
                    // stop searching
                    isSearching = false;

                    // two spellings ... assume first one is right
                    string trackname = filteredSongs.First().TrackName;

                    List<Song> albums = filteredSongs.Where(w => w.TrackName == trackname).ToList();


                    //          albums.Add(ConvertToAlbum(s));

                    // Logic!
                    // Add all the albums, then hide them all. Unhiding the required album from the factory
                    // Factory.SetTrackBuffer... thingymabob
                    // Factory.AddAlbums to track / song
                    // Factory.Add more tracks in session
                    // Factory Commit Changes
                    // === > This is the wild part.. it chooses the most complete album and keeps the rest hidden.
                    // === > Also, we're dumping the iTunes id's for multitracks, if needed it can be gained from the album / artist id

                    var artistcount = (from artc in filteredSongs
                                       select artc.ArtistName).Distinct().OrderBy(ob => ob).ToList();

                    var artistcount2 = (from artc in filteredSongs
                                        select artc.ArtistId).Distinct().OrderBy(ob => ob).ToList();

                    if (isAutoSave)
                        SaveSongArtistData(filteredSongs.First(), albums);
                    else
                    {
                        BaseSong = filteredSongs.First();
                        HasTrack = true;
                    }


                    int trackcount = filteredSongs.Where(w => w.TrackName == trackname).Count();
                    var alt_tracknames = (from fs in filteredSongs
                                          where fs.TrackName != trackname
                                          select fs.TrackName).Distinct().OrderBy(ob => ob).ToList();

                    int alt_trackcount = alt_tracknames.Count();



                    var alt_albumnames = (from afs in filteredSongs
                                          where afs.TrackName != trackname
                                          select afs.CollectionName).Distinct().OrderBy(ob2 => ob2).ToList();

                    int albumcount = alt_albumnames.Count();

                    // TODO : save results for popup
                    //iSongs = filteredSongs;
                }
                // one hit, this is the track
                else if (filteredSongs.Count() == 1)
                {
                    // db Save Song Details
                    foreach (var s in searchNames.Where(w => w.isPossibleTrackNumber == true))
                    {
                        int track = filteredSongs.First().TrackNumber;
                        if (int.Parse(s.TrackTerm) == track)
                        {
                            searchName.TrackNumber = track;
                            searchName.TrackPartIndex = s.SearchPartIndex;
                        }
                    }

                    BaseSong = filteredSongs.First();

                    if (isAutoSave)
                        SaveResultsData(filteredSongs.First(), BufferManagedFile);

                    isSearching = false;
                    HasTrack = true;
                }
            }

            public void SaveSongArtistData(Song baseSong, List<Song> albums)
            {
                if (DBSong != null || BufferManagedFile != null)
                {
                    using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                    {
                        SongLibrary dbSong = new SongLibrary();
                        bool isNew = true;

                        // get fresh copy
                        if (DBSong != null)
                        {
                            dbSong = db.SongLibraries.Find(DBSong.SongId);
                            isNew = false;
                        }
                        else if (BufferManagedFile != null)
                        {
                            // double check if actually exists
                            if (BufferManagedFile.hasDBTrackLibrary && BufferManagedFile.BaseDBTrackLibrary == null)
                            {
                                if (BufferManagedFile.isKaraoke)
                                    BufferManagedFile.BaseDBTrackLibrary = db.TrackLibraries.First(fPath => fPath.FilePath == BufferManagedFile.FilePath);
                                else
                                    BufferManagedFile.BaseDBTrackLibrary = db.TrackLibraries.First(fPath => fPath.FilePath == BufferManagedFile.FilePath);
                            }

                            if (db.TrackLibraries.Find(BufferManagedFile.BaseDBTrackLibrary.Id).SongLibraries.Any())
                            {
                                dbSong = db.TrackLibraries.Find(BufferManagedFile.BaseDBTrackLibrary.Id).SongLibraries.First();
                                isNew = false;
                            }
                        }

                        // Update Song Details
                        dbSong.SongName = baseSong.TrackName;
                        dbSong.Genre = baseSong.PrimaryGenreName;

                        // -------------------
                        // Skips Adding Albums
                        // -------------------

                        DBAddArtist(baseSong, db, ref dbSong);

                        if (isNew)
                        {
                            // add track too
                            if (BufferManagedFile.hasDBTrackLibrary)
                            {
                                BufferManagedFile.BaseDBTrackLibrary = db.TrackLibraries.Find(BufferManagedFile.BaseDBTrackLibrary.Id);
                                BufferManagedFile.BaseDBTrackLibrary.Type = BufferManagedFile.isMusic ? "Music" : BufferManagedFile.isKaraoke ? "Karaoke" : "Video";
                                db.Entry(BufferManagedFile.BaseDBTrackLibrary).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();

                                dbSong.TrackLibraries.Add(BufferManagedFile.BaseDBTrackLibrary);

                                // add to 'new' playlist
                                //Global.mPlayer.dbAddTrackToPlaylist(BufferManagedFile.BaseDBTrackLibrary, Global.NewTracksPlaylistId, db);
                                if (ImportToPlaylist)
                                {
                                    //Global.mPlayer.dbAddTrackToPlaylist(BufferManagedFile.BaseDBTrackLibrary, ImportPlaylistId, db);
                                }

                            }
                            else
                            {

                                TrackLibrary track = new TrackLibrary()
                                {
                                    FilePath = BufferManagedFile.FilePath,
                                    FileName = BufferManagedFile.FileName,
                                    Extention = BufferManagedFile.FileExtention,
                                    Type = BufferManagedFile.isMusic ? "Music" : BufferManagedFile.isKaraoke ? "Karaoke" : "Video"
                                };
                                db.TrackLibraries.Add(track);
                                db.SaveChanges();

                                dbSong.TrackLibraries.Add(track);

                                // add to 'new' playlist
                                //Global.mPlayer.dbAddTrackToPlaylist(track, Global.NewTracksPlaylistId, db);
                                if (ImportToPlaylist)
                                {
                                    //Global.mPlayer.dbAddTrackToPlaylist(track, ImportPlaylistId, db);
                                }

                            }

                            db.SongLibraries.Add(dbSong);
                            db.SaveChanges();
                        }
                        else
                        {
                            db.Entry(dbSong).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();

                            if (BufferManagedFile.hasDBTrackLibrary)
                            {
                                // Change Track back to correct type
                                var tempTrack = db.TrackLibraries.Find(BufferManagedFile.BaseDBTrackLibrary.Id);
                                tempTrack.Type = BufferManagedFile.isMusic ? "Music" : BufferManagedFile.isKaraoke ? "Karaoke" : "Video";

                                db.Entry(tempTrack).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                            }

                        }
                        // ---------------
                        // SAVE TO BUFFER!
                        // ---------------
                        Global.importFactory.AddBufferArtist(searchName, dbSong.Artists.First());

                        Global.importFactory.SessionAddAlbums(albums, dbSong.SongId);
                        Global.ImportAnalytics.AddBreadcrumb(BufferManagedFile.FilePath, "Loaded Albums to Session Buffer Albums");

                        BufferManagedFile.Status = ManagedFileStatus.itunesImported;
                        BufferManagedFile.isCompleteDB = true;
                        isSearching = false;
                        HasTrack = true;
                        //Global.importFactory.AddBufferAlbum(searchName, dbSong.AlbumLibraries.First());
                        //Global.importFactory.AddBufferArtist(searchName, dbSong.Artists.First());
                        //Global.importFactory.AddBufferAnalysisRule(searchName);
                    }
                }
            }

            public void DBAddArtist(Song song, JukeboxBrainsDBEntities db, ref SongLibrary dbSong)
            {
                if (dbSong.Artists.Any())
                    Debug.WriteLine("HazArtist! Help!");
                else
                {
                    // Find in DB First!
                    var dbArt = db.Artists.Where(w => w.ArtistName == song.ArtistName);
                    if (dbArt.Any())
                    {
                        // there should only be one
                        int counter = dbArt.Count();


                        if (dbArt.First().isVerified)
                        {
                            dbSong.Artists.Add(dbArt.First());
                        }
                        else
                        {
                            Artist art = dbArt.First();

                            art.ArtistName = song.ArtistName;
                            art.hasiTunesUpdate = true;
                            art.isVerified = true;
                            art.iTunesId = song.ArtistId.ToString();

                            dbSong.Artists.Add(art);
                        }
                    }
                    else
                    {
                        Artist art = new Artist()
                        {
                            ArtistName = song.ArtistName,
                            hasiTunesUpdate = true,
                            isVerified = true,
                            iTunesId = song.ArtistId.ToString()
                        };
                        dbSong.Artists.Add(art);
                    }
                }
            }

            public void SaveResultsData(Song nSong, ManagedFile managedFile, bool checkDB)
            {
                if (checkDB)
                {
                    using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                    {
                        if (managedFile.BaseDBTrackLibrary == null) managedFile.BaseDBTrackLibrary = db.TrackLibraries.First(fnd => fnd.FilePath == managedFile.FilePath);
                        if (db.TrackLibraries.Find(managedFile.BaseDBTrackLibrary.Id).SongLibraries.Any())
                            DBSong = db.TrackLibraries.Find(managedFile.BaseDBTrackLibrary.Id).SongLibraries.First();
                    }
                }

                SaveResultsData(nSong, managedFile);
            }
            public void SaveResultsData(Song nSong, ManagedFile managedFile = null, int PlaylistId = -1)
            {
                if (DBSong != null || managedFile != null)
                    using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                    {
                        // get fresh copy
                        bool isNew = false;
                        SongLibrary dbSong;
                        if (DBSong != null)
                            dbSong = db.SongLibraries.Find(DBSong.SongId);
                        else
                        {
                            if (managedFile.BaseDBTrackLibrary == null) managedFile.BaseDBTrackLibrary = managedFile.isKaraoke ? db.TrackLibraries.First(f => f.FilePath == managedFile.SongMp3Path) : db.TrackLibraries.First(f => f.FilePath == managedFile.FilePath);
                            if (db.TrackLibraries.Find(managedFile.BaseDBTrackLibrary.Id).SongLibraries.Any())
                            {
                                isNew = false;
                                dbSong = db.TrackLibraries.Find(managedFile.BaseDBTrackLibrary.Id).SongLibraries.First();

                            }
                            else
                            {
                                dbSong = new SongLibrary();
                                isNew = true;
                            }
                        }

                        dbSong.SongName = nSong.TrackName;
                        dbSong.Genre = nSong.PrimaryGenreName;

                        if (dbSong.AlbumLibraries.Any())
                            Debug.WriteLine("HazAlbum! Help!");
                        else
                        {
                            // Find in DB First!
                            var dbAlb = db.AlbumLibraries.Where(wA => wA.AlbumName == nSong.CollectionName);
                            if (dbAlb.Any())
                            {
                                // there should only be one
                                int counter = dbAlb.Count();


                                if (dbAlb.First().isVerified)
                                {
                                    dbSong.AlbumLibraries.Add(dbAlb.First());
                                }
                                else
                                {
                                    AlbumLibrary album = dbAlb.First();

                                    album.AlbumName = nSong.CollectionName;
                                    album.CoverArt = getDBCoverArt(nSong.ArtworkUrl100);
                                    album.iTunesId = nSong.CollectionId.ToString();
                                    album.hasiTunesUpdate = true;
                                    album.isVerified = true;
                                    album.NumTracks = nSong.TrackCount;
                                    album.Year = getReleaseDateYear(nSong.ReleaseDate);

                                    dbSong.AlbumLibraries.Add(album);
                                }
                            }
                            else
                            {
                                AlbumLibrary album = new AlbumLibrary()
                                {
                                    AlbumName = nSong.CollectionName,
                                    CoverArt = getDBCoverArt(nSong.ArtworkUrl100),
                                    iTunesId = nSong.CollectionId.ToString(),
                                    hasiTunesUpdate = true,
                                    isVerified = true,
                                    NumTracks = nSong.TrackCount,
                                    Year = getReleaseDateYear(nSong.ReleaseDate)
                                };

                                dbSong.AlbumLibraries.Add(album);
                            }
                        }

                        DBAddArtist(nSong, db, ref dbSong);

                        // ------------------
                        if (isNew)
                        {
                            // add track too
                            if (managedFile.hasDBTrackLibrary)
                            {
                                var track = db.TrackLibraries.Find(managedFile.BaseDBTrackLibrary.Id);
                                //managedFile.BaseDBTrackLibrary.Type = managedFile.isMusic ? "Music" : managedFile.isKaraoke ? "Karaoke" : "Video";
                                track.Type = managedFile.isMusic ? "Music" : managedFile.isKaraoke ? "Karaoke" : "Video";
                                db.Entry(track).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();

                                dbSong.TrackLibraries.Add(track);
                                db.SaveChanges();

                                // add to 'new' playlist
                                //Global.mPlayer.dbAddTrackToPlaylist(track, Global.NewTracksPlaylistId, db);

                                // Save to Playlist
                                if (PlaylistId > -1)
                                {
                                    //Global.mPlayer.dbAddTrackToPlaylist(track, PlaylistId, db);
                                }
                                else if (ImportToPlaylist)
                                {
                                    //Global.mPlayer.dbAddTrackToPlaylist(track, ImportPlaylistId, db);
                                }
                            }
                            else
                            {
                                TrackLibrary track = new TrackLibrary()
                                {
                                    FilePath = managedFile.FilePath,
                                    FileName = managedFile.FileName,
                                    Extention = managedFile.FileExtention,
                                    Type = managedFile.isMusic ? "Music" : managedFile.isKaraoke ? "Karaoke" : "Video",
                                };

                                var t2 = db.TrackLibraries.Add(track);
                                db.SaveChanges();
                                dbSong.TrackLibraries.Add(track);
                                db.SaveChanges();

                                // add to 'new' playlist
                                //Global.mPlayer.dbAddTrackToPlaylist(track, Global.NewTracksPlaylistId, db);

                                // Save to Playlist
                                if (PlaylistId > -1)
                                {
                                    //Global.mPlayer.dbAddTrackToPlaylist(track, PlaylistId, db);
                                }
                                else if (ImportToPlaylist)
                                {
                                    //Global.mPlayer.dbAddTrackToPlaylist(track, ImportPlaylistId, db);
                                }
                            }

                            db.SongLibraries.Add(dbSong);

                            db.SaveChanges();
                        }
                        else
                        {
                            db.Entry(dbSong).State = System.Data.Entity.EntityState.Modified;

                            db.SaveChanges();

                            var t = db.TrackLibraries.Find(managedFile.BaseDBTrackLibrary.Id);
                            t.Type = managedFile.ImportTypeString;
                            db.Entry(t).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();

                            // Save to Playlist
                            if (PlaylistId > -1)
                            {
                                //Global.mPlayer.dbAddTrackToPlaylist(t, PlaylistId, db);
                            }
                            else if (ImportToPlaylist)
                            {
                                //Global.mPlayer.dbAddTrackToPlaylist(t, ImportPlaylistId, db);
                            }

                            managedFile.BaseDBTrackLibrary = t;
                        }



                        // SAVE TO BUFFER!
                        managedFile.isCompleteDB = true;
                        Global.importFactory.AddBufferAlbum(searchName, dbSong.AlbumLibraries.First());
                        Global.importFactory.AddBufferArtist(searchName, dbSong.Artists.First());
                        if (searchName != null)
                            Global.importFactory.AddBufferAnalysisRule(searchName);

                        Global.ImportAnalytics.AddBreadcrumb(managedFile.FilePath, "Saved Full iTunes");
                    }
            }

            #endregion Filters

            public void ExtractSearchArtistList(string filename, List<ManagedFile> files, JukeboxBrainsDBEntities db)
            {
                ExtractSearchArtistList(filename);
                CompareEngine c = new CompareEngine();

                // check buffer
                foreach (var ab in Global.importFactory.ArtistBuffer.Where(w => w.Rule != null))
                {
                    foreach (var name in searchNames)
                    {
                        if (getArtistEqual(ab.Rule.ArtistSearchTerm, name.SearchTerm) == CompareResults.Match && ab.Rule.ArtistPartIndex == name.SearchPartIndex)
                        {
                            // found!
                            isSearching = false;
                            HasArtist = true;
                            DBArtist = ab.Artist;

                            HasArtist = true;

                            // save primary search stuff, 
                            PrimarySearchOption = ab.Rule;
                            ImportedRule = true;
                            //PrimarySearchOption.ArtistName = BaseSongArtist.ArtistName;
                            //PrimarySearchOption.ArtistPartIndex = searchIndex;
                            //PrimarySearchOption.ArtistSearchTerm = searchName.SearchTerm;

                            // generate new search list for track
                            if (DBSong == null)
                            {
                                if (BufferManagedFile != null)
                                {
                                    ExtractSearchNameList(BufferManagedFile.FileName);
                                    /// Artist Name will be removed from the list, so compensating
                                    if (PrimarySearchOption.ArtistPartIndex < PrimarySearchOption.SearchPartIndex)
                                        searchIndex = PrimarySearchOption.SearchPartIndex - 1;
                                    else
                                        searchIndex = PrimarySearchOption.SearchPartIndex;

                                    // bypass above math to see if actually working
                                    if (searchIndex > searchNames.Count - 1)
                                        searchIndex = 0;
                                    searchName = searchNames[searchIndex];
                                }
                            }
                            else
                                ExtractSearchNameList(DBSong.SongName);

                            break;
                        }
                        if (HasArtist)
                            break;
                    }
                    if (HasArtist)
                        break;
                }

                if (!HasArtist)
                {
                    // check batch..
                    List<string> found = new List<string>();
                    SearchAnalysisRule rule = searchNames[0];
                    bool foundArtist = false;
                    int sindex = 0;

                    foreach (var name in searchNames)
                    {
                        // REMOVED DB SEARCH - FILES ONLY

                        //if (!name.isPossibleTrackNumber)
                        //{
                        //    foreach (var a in db.Artists.Select(sel => sel.ArtistName))
                        //    {
                        //        if (getArtistEqual(name.SearchTerm, a) != CompareResults.NoMatch)
                        //        {
                        //            rule = name;
                        //            found.Add(a);
                        //            break;
                        //        }
                        //    }
                        //}

                        if (found.Count >= 1)
                            break;

                        if (!name.isPossibleTrackNumber)
                        {
                            List<string> f = (from i in files where filename != i.FileName select i.FileName).ToList();
                            List<string> searchStrings = new List<string>();
                            if (f.Any())
                                searchStrings = name.GetSearchStrings(f);

                            foreach (var s in searchStrings)
                            {
                                // use same rule!
                                if (getArtistEqual(name.SearchTerm, s) != CompareResults.NoMatch)
                                {
                                    rule = name;
                                    found.Add(s);

                                }
                            }

                            if (found.Count >= 1)
                                break;
                        }
                        sindex++;
                    }

                    if (found.Count >= 1)
                    {
                        //   searchNames.Insert(0, rule);
                        // move record to front of que
                        var tempRule = searchNames[sindex];
                        tempRule.ArtistPartIndex = sindex;
                        searchNames.RemoveAt(sindex);
                        searchNames.Insert(0, tempRule);

                        searchIndex = 0;
                        searchName = searchNames[searchIndex];
                    }
                }
            }
            public void ExtractSearchArtistList(string filename, List<ManagedFile> files)
            {
                using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                {
                    ExtractSearchArtistList(filename, files, db);
                }
            }
            public void ExtractSearchArtistList(string filename)
            {
                isSearching = true;
                searchNames = new List<SearchAnalysisRule>();
                searchIndex = 0;

                searchNames = Global.importFactory.aEngine.GetArtistSearchStrings(filename);
                if (searchNames[searchIndex].isPossibleTrackNumber)
                    searchIndex++;
                searchName = searchNames[searchIndex];
            }

            private void a() { }
            private void b() { }
            private void c() { }

            public void ExtractSearchNameList(string filename)
            {
                isSearching = true;
                searchNames = new List<SearchAnalysisRule>();
                searchIndex = 0;

                if (!HasArtist)
                    searchNames = Global.importFactory.aEngine.GetTrackSearchStrings(filename);
                else
                    searchNames = Global.importFactory.aEngine.GetTrackSearchStrings(filename, true, true, "", PrimarySearchOption);

                if (searchNames[searchIndex].isPossibleTrackNumber)
                    searchIndex++;

                if (searchIndex >= searchNames.Count())
                    searchIndex = searchNames.Count() - 1;

                searchStartingIndex = searchIndex;
                searchName = searchNames[searchIndex];
            }

            public void NextSearchTerm()
            {
                // TODO: Add searchStartingIndex
                if (searchIndex < (searchNames.Count - 1))
                {
                    searchIndex++;
                    searchName = searchNames[searchIndex];

                }
                else if (searchIndex < (searchNames.Count - 1) + searchStartingIndex)
                {
                    searchIndex++;
                    searchName = searchNames[searchIndex - searchNames.Count];
                    //searchIndex--;
                }
                else
                {
                    isSearching = false;
                }
            }

            public bool isAutoSave = true;
            internal void AutoSaveOff()
            {
                isAutoSave = false;
            }


            #region Helpers



            #endregion
        }

        public class iManagedAlbum
        {
            public AlbumResult AlbumResults
            {
                set
                {
                    _searchAlbumResults = value;
                    filterResults();
                }
            }
            private AlbumResult _searchAlbumResults;

            public AlbumLibrary DBAlbum { get; set; }
            public Album iAlbum { get; set; }
            public bool NeedAction { get; internal set; }
            public bool Success { get; internal set; }




            public SongResult TrackResults
            {
                set
                {
                    _searchTrackResults = value;
                    if (_searchTrackResults.Songs.Where(w => w.TrackId > 0).Any())
                    {
                        filterTrackResults();
                    }
                }
            }
            private SongResult _searchTrackResults;
            public string iArtistId { get; set; }

            /// <summary>
            /// Used to search alternative options when first search failed
            /// </summary>
            public List<string> AltSearchOptions = new List<string>();

            public void filterTrackResults()
            {
                List<Song> filtered = new List<Song>();


                // filter for album ID
                foreach (Song s in _searchTrackResults.Songs)
                {
                    if (s.CollectionId.ToString() == DBAlbum.iTunesId)
                    {
                        filtered.Add(s);
                    }
                }

                using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                {
                    // get fresh copy
                    AlbumLibrary album = db.AlbumLibraries.Find(DBAlbum.AlbumId);
                    // Update track results
                    foreach (SongLibrary song in album.SongLibraries)
                    {
                        foreach (Song iS in filtered)
                        {
                            if (song.SongName == iS.TrackName)
                            {
                                filtered.Remove(iS);
                                break;
                            }
                        }
                    }

                    // remove songs
                    foreach (SongLibrary song in db.SongLibraries.Where(w => !w.AlbumLibraries.Any()))
                    {
                        foreach (Song iS in filtered)
                        {
                            int i1 = String.Compare(song.SongName, iS.TrackName);
                            int i2 = String.Compare(song.SongName, iS.TrackName, StringComparison.OrdinalIgnoreCase);
                            int i3 = String.Compare(song.SongName, iS.TrackName, StringComparison.InvariantCultureIgnoreCase);
                            int i4 = String.Compare(song.SongName, iS.TrackName, StringComparison.CurrentCultureIgnoreCase);

                            if (song.SongName == iS.TrackName)
                            {
                                song.SongName = iS.TrackName;
                                song.Genre = iS.PrimaryGenreName;
                                song.AlbumLibraries.Add(album);

                                db.Entry(song).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();

                                filtered.Remove(iS);
                                break;
                            }
                        }
                    }

                    // Find lost track results

                }
            }


            public void UpdateAlbumTracks()
            {


                //string tracksApi = getApi_GetAlbumSongs(DBAlbum.iTunesId, 100, "");
                //var resultTracks = await MakeAPICall<SongResult>(tracksApi);

                //foreach (var rSong in resultTracks.Songs)
                //{
                //    if (rSong.TrackId > 0)
                //    {
                //        // Search Connected Tracks first
                //        var x = a.SongLibraries.Where(w => getlowercompare(w.SongName) == getlowercompare(rSong.TrackName));

                //        if (x.Count() > 1)
                //        {

                //        }
                //        else if (x.Count() == 1)
                //        {
                //            // Found track
                //            var iSong = db.SongLibraries.Find(x.First().SongId);
                //            iSong.SongName = rSong.TrackName;
                //            iSong.Genre = rSong.PrimaryGenreName;
                //        }
                //        else
                //        {
                //            var x2 = a.SongLibraries.Where(w2 => getlowercompare(rSong.TrackName).Contains(getlowercompare(w2.SongName)));
                //            int iCount = x2.Count();
                //        }

                //        // Search 'lost' tracks
                //        var x3 = from s3 in db.SongLibraries
                //                 where s3.AlbumLibraries.Count() == 0
                //                 select s3;

                //        int iCount2 = x3.Count();
                //    }
                //}

            }

            private void filterResults()
            {
                string albumName = DBAlbum.AlbumName;
                List<Album> filtered = new List<Album>();
                foreach (Album a in _searchAlbumResults.Albums)
                {
                    int compare1 = string.Compare(albumName, a.CollectionName, StringComparison.OrdinalIgnoreCase);
                    int compare2 = string.Compare(albumName, a.CollectionName, StringComparison.InvariantCultureIgnoreCase);
                    int compare3 = string.Compare(albumName, a.CollectionName, StringComparison.CurrentCultureIgnoreCase);

                    if (a.CollectionName.Trim().ToLower().Replace(" ", "") == albumName.Trim().ToLower().Replace(" ", ""))
                    {
                        filtered.Add(a);
                    }
                }

                if (filtered.Count == 0)
                {
                    // No Result Found!
                    int i = 0;
                }
                else if (filtered.Count > 1)
                {
                    // many options, choose one
                    int i = 0;
                }
                else
                {
                    var i = filtered[0];
                    using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                    {
                        // get a fresh copy
                        var a = db.AlbumLibraries.Find(DBAlbum.AlbumId);
                        a.AlbumName = i.CollectionName;
                        a.NumTracks = i.TrackCount;
                        a.hasiTunesUpdate = true;
                        a.isVerified = true;
                        a.iTunesId = i.CollectionId.ToString();
                        iArtistId = i.ArtistId.ToString();

                        a.CoverArt = getDBCoverArt(i.ArtworkUrl100);
                        a.Year = getReleaseDateYear(i.ReleaseDate);

                        db.Entry(a).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                    }
                }
            }


            public iManagedAlbum(AlbumLibrary baseAlbum)
            {
                DBAlbum = baseAlbum;
            }

        }

        /// <summary>
        /// This is run per set, so all albums should have the same set number
        /// </summary>
        /// <param name="albums"></param>
        /// <param name="unhideAlbumId"></param>
        private async void CreateAlbumsAsync(List<FactorySessionAlbum> albums, List<long> unhideAlbumId)
        {
            Global.isUpdatingLibrary = true;

            // Adds songs to album, rather than album to songs
            using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
            {
                // get songs to attach
                //SongLibrary dbSong = db.SongLibraries.Find(dbSongId);

                //List<AlbumLibrary> dbAlbums = new List<AlbumLibrary>();

                foreach (var s in albums)
                {
                    // check if album exists via iTunes ID
                    AlbumLibrary album;
                    var existing = db.AlbumLibraries.Where(w => w.iTunesId == s.album.CollectionId.ToString());


                    if (existing.Any())
                    {
                        album = existing.Single();
                        if (unhideAlbumId.Contains(s.album.CollectionId))
                            album.isHidden = false;
                        else
                            album.isHidden = true;

                        // attach song
                        if (!album.SongLibraries.Any(a => a.SongId == s.DBSongId))
                        {
                            SongLibrary dbSong = db.SongLibraries.Find(s.DBSongId);
                            album.SongLibraries.Add(dbSong);
                        }

                        db.Entry(album).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                    }
                    else
                    {
                        // create new, one at a time
                        if (!unhideAlbumId.Contains(s.album.CollectionId))
                            // hides album
                            album = ConvertToAlbum(s.album, true);
                        else
                            album = ConvertToAlbum(s.album);

                        // attach song
                        SongLibrary dbSong = db.SongLibraries.Find(s.DBSongId);
                        album.SongLibraries.Add(dbSong);

                        db.AlbumLibraries.Add(album);
                        db.SaveChanges();
                    }
                }

                //db.Entry(dbSong).State = System.Data.Entity.EntityState.Modified;
                //db.SaveChanges();
            }


            Global.isUpdatingLibrary = false;
        }

        /// <summary>
        /// Updates the album from iTunes, searching for the name first
        /// </summary>
        /// <param name="baseAlbum"></param>
        /// <param name="libraryPopup"></param>
        private async void UpdateSearchAlbumAsync(AlbumLibrary baseAlbum, LibraryPopup libraryPopup)
        {
            Global.isUpdatingLibrary = true;
            // ------

            iManagedAlbum iAlbum = new iManagedAlbum(baseAlbum);

            #region Album Details


            // GetSearch API
            string api = getApi_SearchAlbum(baseAlbum.AlbumName, 100, "");
            // TODO: Testing API Call!
            try { iAlbum.AlbumResults = await MakeAPICall<AlbumResult>(api); }
            catch
            {
                // TODO: Testing API Call!
                Global.SetOnlineTimeout();
                // break;
            }

            // Has no results? Search via Tracks
            foreach (string s in iAlbum.AltSearchOptions)
            {
                // TODO: Testing API Call!
                try { iAlbum.AlbumResults = await MakeAPICall<AlbumResult>(api); }
                catch
                {
                    // TODO: Testing API Call!
                    Global.SetOnlineTimeout();
                    break;
                }
                if (iAlbum.Success)
                    break;
            }


            #endregion Album Details


            if (iAlbum.NeedAction)
            {
                // Has multiple results? Save them, create popup choice window

            }
            else
            {
                // Everything Okay, updating Tracks
                //string trackApi = getApi_GetAlbumSongs(iAlbum.DBAlbum.iTunesId, 100, "");
                //iAlbum.TrackResults = await MakeAPICall<SongResult>(trackApi);


                // failed, searching via Artist Instead
                //string trackArtistApi = getApi_GetArtistSongs(iAlbum.iArtistId, 100, "");
                //iAlbum.TrackResults = await MakeAPICall<SongResult>(trackArtistApi);
            }



            // ------
            Global.isUpdatingLibrary = false;
        }

        public async void UpdateAlbumTracksAsync(string iTunesAlbumId)
        {
            string api = getApi_GetAlbumSongs(iTunesAlbumId, 100, "");

            // Search iTunes
            Debug.WriteLine("Starting API CALL to iTunes");
            SongResult results = new SongResult();
            // TODO: Testing API Call!
            try { results = await MakeAPICall<SongResult>(api); }
            catch
            {
                // TODO: Testing API Call!
                Global.SetOnlineTimeout();
                // break;
            }

            using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
            {
                foreach (Song s in results.Songs)
                {
                    //s.ArtistId 
                    //s.ArtistName
                    //s.TrackNumber
                    //s.TrackName
                    //s.PrimaryGenreName


                }
                // update id3s

            }
            // 
        }

        #endregion Threading Methods



        #region Music Search


        public string getApi_SearchArtist(string artist, int resultLimit = 100, string countryCode = "us")
        {
            var nvc = HttpUtility.ParseQueryString(string.Empty);

            nvc.Add("term", artist);
            nvc.Add("media", "music");
            nvc.Add("entity", "musicArtist");
            //nvc.Add("attribute", "artistTerm");
            nvc.Add("limit", resultLimit.ToString());
            nvc.Add("country", countryCode);

            return string.Format(_baseSearchUrl, nvc.ToString());
        }
        public string getApi_SearchSong(string song, int resultLimit = 100, string countryCode = "us")
        {
            var nvc = HttpUtility.ParseQueryString(string.Empty);

            nvc.Add("term", song);
            nvc.Add("media", "music");
            nvc.Add("entity", "song");
            //nvc.Add("attribute", "artistTerm");
            nvc.Add("limit", resultLimit.ToString());
            nvc.Add("country", countryCode);

            return string.Format(_baseSearchUrl, nvc.ToString());
        }

        public string getApi_GetAlbum(string albumid, int resultLimit = 100, string countryCode = "us")
        {
            var nvc = HttpUtility.ParseQueryString(string.Empty);
            nvc.Add("id", albumid);
            nvc.Add("media", "music");
            nvc.Add("entity", "album");
            //nvc.Add("limit", resultLimit.ToString());

            return string.Format(_baseLookupUrl, nvc.ToString());
        }
        public string getApi_SearchAlbum(string album, int resultLimit = 100, string countryCode = "us")
        {
            var nvc = HttpUtility.ParseQueryString(string.Empty);

            nvc.Add("term", album);
            nvc.Add("media", "music");
            nvc.Add("entity", "album");
            //nvc.Add("attribute", "artistTerm");
            nvc.Add("limit", resultLimit.ToString());
            nvc.Add("country", countryCode);

            return string.Format(_baseSearchUrl, nvc.ToString());
        }

        public string getApi_GetArtistAlbums(string artistid, int resultLimit = 100, string countryCode = "us")
        {
            var nvc = HttpUtility.ParseQueryString(string.Empty);

            nvc.Add("id", artistid);
            //nvc.Add("media", "music");
            nvc.Add("entity", "album");
            //nvc.Add("attribute", "artistTerm");
            nvc.Add("limit", resultLimit.ToString());
            //nvc.Add("country", countryCode);

            return string.Format(_baseLookupUrl, nvc.ToString());
        }
        public string getApi_GetArtistSongs(string artistid, int resultLimit = 100, string countryCode = "us")
        {
            var nvc = HttpUtility.ParseQueryString(string.Empty);

            nvc.Add("id", artistid);
            //nvc.Add("media", "music");
            nvc.Add("entity", "song");
            //nvc.Add("attribute", "artistTerm");
            nvc.Add("limit", resultLimit.ToString());
            //nvc.Add("country", countryCode);

            return string.Format(_baseLookupUrl, nvc.ToString());
        }
        public string getApi_GetAlbumSongs(string albumid, int resultLimit = 100, string countryCode = "us")
        {
            var nvc = HttpUtility.ParseQueryString(string.Empty);

            nvc.Add("id", albumid);
            //nvc.Add("media", "music");
            nvc.Add("entity", "song");
            //nvc.Add("attribute", "artistTerm");
            nvc.Add("limit", resultLimit.ToString());
            //nvc.Add("country", countryCode);

            return string.Format(_baseLookupUrl, nvc.ToString());
        }


        /// <summary>
        /// Gets a list of artists for a given search term
        /// </summary>
        /// <param name="artist">The artist name to search for.</param>
        /// <param name="resultLimit">Limit the result count to this number.</param>
        /// <param name="countryCode">The two-letter country ISO code for the store you want to search.
        /// See http://en.wikipedia.org/wiki/%20ISO_3166-1_alpha-2 for a list of ISO country codes</param>
        /// <returns></returns>
        public async Task<SongArtistResult> GetSongArtistsAsync(string artist, int resultLimit = 100, string countryCode = "us")
        {
            var nvc = HttpUtility.ParseQueryString(string.Empty);

            nvc.Add("term", artist);
            nvc.Add("media", "music");
            nvc.Add("entity", "musicArtist");
            nvc.Add("attribute", "artistTerm");
            nvc.Add("limit", resultLimit.ToString());
            nvc.Add("country", countryCode);

            //  Construct the url:
            string apiUrl = string.Format(_baseSearchUrl, nvc.ToString());

            //  Get the list of episodes
            SongArtistResult result = new SongArtistResult();
            // TODO: Testing API Call!
            try { result = await MakeAPICall<SongArtistResult>(apiUrl); }
            catch
            {
                // TODO: Testing API Call!
                Global.SetOnlineTimeout();
                // break;
            }

            return result;
        }


        #endregion Music Search

        #region API helpers

        /// <summary>
        /// Makes an API call and deserializes return value to the specified type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="apiCall"></param>
        /// <returns></returns>
        private int iTunesCallCounter = 0;
        private int apiLock = 40;
        private int apiLockX = 1;

        private int iCounterID = -1;
        async private Task<T> MakeAPICall<T>(string apiCall, bool RemoveCollections = false)
        {
            if (apiLock * apiLockX <= iTunesCallCounter)
            {
                int i = Global.mPlayer.Notifactions.NewNotification("Sleeping...");
                Thread.Sleep(10000);
                Global.mPlayer.Notifactions.RemoveNotification(i);
                apiLockX++;
            }

            HttpClient client = new HttpClient();

            //  Make an async call to get the response
            var objString = await client.GetStringAsync(apiCall).ConfigureAwait(false);

            // remove 'collection' entry
            if (RemoveCollections)
                if (objString.Contains("\"wrapperType\":\"collection\""))
                {
                    int startIndex = objString.IndexOf("{\"wrapperType\":\"collection\"");
                    int endIndex = objString.IndexOf("}");
                    int len = endIndex - startIndex + 2;

                    string collection = objString.Substring(startIndex, len - 1);
                    objString = objString.Remove(startIndex, len);

                    string wrong = "";
                    if (objString.Contains("\"wrapperType\":\"collection\""))
                        wrong = "WRONG!";
                }

            iTunesCallCounter++;
            //if (iCounterID == -1)
            //    iCounterID = Global.mPlayer.Notifactions.NewNotification(string.Format("iTunes API Calls : {0}", iTunesCallCounter));
            //else
            //    Global.mPlayer.Notifactions.UpdateNotification(iCounterID, string.Format("iTunes API Calls : {0}", iTunesCallCounter));

            //  Deserialize and return
            return (T)DeserializeObject<T>(objString);
        }



        /// <summary>
        /// Deserializes the JSON string to the given type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objString"></param>
        /// <returns></returns>
        private T DeserializeObject<T>(string objString)
        {
            using (var stream = new MemoryStream(Encoding.Unicode.GetBytes(objString)))
            {
                var serializer = new DataContractJsonSerializer(typeof(T));
                return (T)serializer.ReadObject(stream);
            }
        }

        public string RemoveInvalidFilePathCharacters(string filename, string replaceChar)
        {
            string regexSearch = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            Regex r = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));
            return r.Replace(filename, replaceChar);
        }

        #endregion API helpers
    }
}
