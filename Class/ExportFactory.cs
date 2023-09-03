using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JukeBoxSolutions.Class
{
    class ExportFactory
    {
        private class IndexKey
        {
            internal int ExportIndex { get; set; }
            internal int DBIndex { get; set; }
        }

        internal class ReverseIndexKey
        {
            internal int ExportIndex { get; set; }
            internal AlbumLibrary DbAlbum { get; set; }
            internal Artist DbArtist { get; set; }
        }

        private List<IndexKey> AlbumIndex { get; set; }
        private List<IndexKey> ArtistIndex { get; set; }

        internal void ExportFolder(string s)
        {

        }

        internal void ExportTracks(List<TrackLibrary> cluster)
        {
            var folders = cluster.Select(s => new { folder = s.FilePath.Replace(s.FileName + s.Extention, ""), DbTrack = s });
            var foldersD = folders.Select(s => s.folder).Distinct();

            foreach (var f in foldersD)
            {
                ExportBatch(folders.Where(w => w.folder == f).Select(s => s.DbTrack.Id).ToList(), f);
            }
        }

        private void ExportAlbumArt(byte[] CoverArt, int AlbumBatchId, string MusicFilesRoot)
        {
            string exportDir = String.Format(@"{0}AlbumCovers\\", MusicFilesRoot).Replace("\\\\", "\\");

            if (!System.IO.Directory.Exists(exportDir))
                System.IO.Directory.CreateDirectory(exportDir);

            using (var ms = new MemoryStream(CoverArt))
            {
                System.Drawing.Image img = System.Drawing.Image.FromStream(ms);
                img.Save(String.Format(@"{0}{1}.jpg", exportDir, AlbumBatchId.ToString()), System.Drawing.Imaging.ImageFormat.Jpeg);
            }
        }

        private void ExportBatch(List<int> batch, string folderName)
        {
            var buffer = new StringBuilder();


            using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
            {
                var refreshBatch = db.TrackLibraries.Where(w => batch.Contains(w.Id)).ToList();


                // Write Albums to batch
                var flatbuffer = refreshBatch.SelectMany(sLib => sLib.SongLibraries).Distinct();
                int counter1 = flatbuffer.Count();
                var flatalbumbuffer = flatbuffer.SelectMany(alb => alb.AlbumLibraries).Where(w => w.isHidden == false).Distinct();
                counter1 = flatalbumbuffer.Count();
                List<ReverseIndexKey> albumbuffer = flatalbumbuffer.Select((ab, i) => new ReverseIndexKey() { DbAlbum = ab, ExportIndex = i }).Distinct().ToList();

                var songalbum = albumbuffer.SelectMany(ReverseIndexKey => ReverseIndexKey.DbAlbum.SongLibraries.Select(SongLibrary => new { ReverseIndexKey, SongLibrary })).ToList();
                counter1 = albumbuffer.Count();
                counter1 = songalbum.Count();

                // Write Artists to batch
                //          var artistbuffer = minifolder.SelectMany(art => art.artists).Distinct();
                var flatArtistBuffer = flatbuffer.SelectMany(alb => alb.Artists).Distinct();
                List<ReverseIndexKey> artistbuffer = flatArtistBuffer.Select((ar, i) => new ReverseIndexKey() { DbArtist = ar, ExportIndex = i }).Distinct().ToList();


                //
                var tbatch = from first in refreshBatch
                             join last in songalbum on first.SongLibraries.First().SongId equals last.SongLibrary.SongId into temp
                             from last1 in temp
                             select new
                             {
                                 FilePath = first.FileName + first.Extention,
                                 SongId = first.SongLibraries.First().SongId,
                                 SongName = first.SongLibraries.First().SongName,
                                 Genre = first.SongLibraries.First().Genre,
                                 AlbumID = last1.ReverseIndexKey.ExportIndex,
                                 ArtistDBId = first.SongLibraries.First().Artists.First().Id
                             };

                var trackbatch = from first2 in tbatch
                                 join last2 in artistbuffer on first2.ArtistDBId equals last2.DbArtist.Id into temp2
                                 from last3 in temp2
                                 select new
                                 {
                                     FilePath = first2.FilePath,
                                     SongId = first2.SongId,
                                     SongName = first2.SongName,
                                     Genre = first2.Genre,
                                     AlbumID = first2.AlbumID,
                                     ArtistID = last3.ExportIndex
                                 };

                // Music Vids... track path

                foreach (var a in albumbuffer)
                {
                    buffer.AppendLine(String.Format("AlbumDetails[{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}|{8}|{9}]", a.ExportIndex, a.DbAlbum.AlbumName, a.DbAlbum.MusicBrainzId, a.DbAlbum.Year, a.DbAlbum.NumTracks, a.DbAlbum.isVerified, a.DbAlbum.hasMusicBrainzUpdate, a.DbAlbum.iTunesId, a.DbAlbum.hasiTunesUpdate, a.DbAlbum.isHidden));
                    if (a.DbAlbum.CoverArt != null) ExportAlbumArt(a.DbAlbum.CoverArt, a.ExportIndex, folderName);
                }

                foreach (var a in artistbuffer)
                {
                    buffer.AppendLine(String.Format("ArtistDetails[{0}|{1}|{2}|{3}|{4}]", a.ExportIndex, a.DbArtist.ArtistName, a.DbArtist.iTunesId, a.DbArtist.isVerified, a.DbArtist.hasiTunesUpdate));
                }

                // Write Tracks to batch
                trackbatch.ToList().ForEach(item => buffer.AppendLine(String.Format("TrackDetails[{0}|{1}|{2}|{3}|{4}]", item.FilePath, item.SongName, item.Genre, item.AlbumID, item.ArtistID)));

            }

            // Write File
            File.WriteAllText(folderName + "albumData.jukebox", buffer.ToString());
        }

        private void ExportAlbums(List<AlbumLibrary> albums)
        {
            foreach (var a in albums)
            {
                int i = 0;
                string s = string.Format("AlbumDetails[{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}|{8}|{9}]", i, a.AlbumName, a.MusicBrainzId, a.Year, a.NumTracks, a.isVerified, a.hasMusicBrainzUpdate, a.iTunesId, a.hasiTunesUpdate, a.isHidden);
                WriteLine(s);
            }
        }
        internal ReverseIndexKey ImportAlbum(string s)
        {
            ReverseIndexKey r = new ReverseIndexKey();

            s = s.Substring(s.IndexOf('['));
            s = s.Substring(0, s.IndexOf(']'));

            string[] parts = s.Split('|');
            r.ExportIndex = int.Parse(parts[0]);

            AlbumLibrary a = new AlbumLibrary()
            {
                AlbumName = parts[1],
                MusicBrainzId = parts[2],
                Year = int.Parse(parts[3]),
                NumTracks = int.Parse(parts[3]),
                isVerified = bool.Parse(parts[5]),
                hasMusicBrainzUpdate = bool.Parse(parts[6]),
                iTunesId = parts[7],
                hasiTunesUpdate = bool.Parse(parts[8]),
                isHidden = bool.Parse(parts[9]),
                isFavourite=false
            };

            r.DbAlbum = a;
            return r;
        }

        private void ExportArtists(List<Artist> artists)
        {
            foreach (var a in artists)
            {
                int i = 0;
                string s = string.Format("ArtistDetails[{0}|{1}|{2}|{3}|{4}]", i, a.ArtistName, a.iTunesId, a.isVerified, a.hasiTunesUpdate);
                WriteLine(s);
            }
        }
        internal ReverseIndexKey ImportArtist(string s)
        {
            ReverseIndexKey r = new ReverseIndexKey();

            s = s.Substring(s.IndexOf('['));
            s = s.Substring(0, s.IndexOf(']'));

            string[] parts = s.Split('|');
            r.ExportIndex = int.Parse(parts[0]);

            Artist a = new Artist()
            {
                ArtistName = parts[1],
                iTunesId = parts[2],
                isVerified = bool.Parse(parts[3]),
                hasiTunesUpdate = bool.Parse(parts[4]),
            };

            r.DbArtist = a;
            return r;
        }
        private void ExportTracks2(List<TrackLibrary> tracks)
        {
            // MISSING ALBUM
            foreach (var t in tracks)
            {
                SongLibrary song = t.SongLibraries.First();
                int i = 0;
                string s = string.Format("TrackDetails[{0}|{1}|{2}|{3}|{4}]", t.FileName + t.Extention, song.SongName, song.Genre, 0, 0);
                WriteLine(s);
            }
        }

        private void WriteLine(string s)
        {

        }



        internal List<string> AlbumCoverDownloadList_iTunesID = new List<string>();
        internal void ImportBatch(string filePath, ref IEnumerable<ImportFactory.ManagedFile> mBatch)
        {
            var sBatch = File.ReadAllLines(filePath);
            List<string[]> albumArray = new List<string[]>();
            List<string[]> artistArray = new List<string[]>();
            List<string[]> trackArray = new List<string[]>();

            foreach (string s in sBatch)
            {
                if (s.StartsWith("AlbumDetails"))
                {
                    albumArray.Add(s.Replace("AlbumDetails", "").Trim('[', ']').Split('|'));
                }
                else if (s.StartsWith("ArtistDetails"))
                {
                    artistArray.Add(s.Replace("ArtistDetails", "").Trim('[', ']').Split('|'));
                }
                else if (s.StartsWith("TrackDetails"))
                {
                    trackArray.Add(s.Replace("TrackDetails", "").Trim('[', ']').Split('|'));
                }
            }


            // "TrackDetails[{0}|{1}|{2}|{3}|{4}]",
            var tracks = from t in trackArray
                         select new
                         {
                             FilePath = t[0],
                             SongName = t[1],
                             Genre = t[2],
                             AlbumID = t[3],
                             ArtistID = t[4]
                         };

            foreach (var mb in mBatch)
            {
                //mb.FullFileName
                List<string> tempAlbums = new List<string>();
                List<string> tempArtists = new List<string>();
                if (tracks.Where(w => mb.FullFileName.Contains(w.FilePath)).Any())
                {
                    var tempSong = tracks.Where(w => mb.FullFileName.Contains(w.FilePath)).First();

                    foreach (var x in tracks.Where(w => mb.FullFileName.Contains(w.FilePath)))
                    {
                        if (x.AlbumID != "")
                        {
                            tempAlbums.Add(x.AlbumID);
                        }

                        if (x.ArtistID != "")
                        {
                            tempArtists.Add(x.ArtistID);
                        }
                    }

                    // Write DB
                    if (AlbumIndex == null)
                        AlbumIndex = new List<IndexKey>();
                    if (ArtistIndex == null)
                        ArtistIndex = new List<IndexKey>();

                    using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                    {
                        // save albums
                        // "AlbumDetails[{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}|{8}|{9}]", a.ExportIndex, a.DbAlbum.AlbumName, a.DbAlbum.MusicBrainzId, a.DbAlbum.Year, a.DbAlbum.NumTracks, a.DbAlbum.isVerified, a.DbAlbum.hasMusicBrainzUpdate, a.DbAlbum.iTunesId, a.DbAlbum.hasiTunesUpdate, a.DbAlbum.isHidden));
                        foreach (string i in tempAlbums)
                        {
                            if (!AlbumIndex.Any(a => a.ExportIndex.ToString() == i))
                            {


                                string[] album = albumArray.Where(w => w[0] == i).First();
                                string tempItunesID = album[7];
                                if (db.AlbumLibraries.Where(w2 => w2.iTunesId == tempItunesID).Any())
                                {
                                    // Has album
                                    if (AlbumIndex == null)
                                        AlbumIndex = new List<IndexKey>();
                                    AlbumIndex.Add(new IndexKey() { ExportIndex = int.Parse(i), DBIndex = db.AlbumLibraries.Where(w3 => w3.iTunesId == tempItunesID).First().AlbumId });
                                }
                                else
                                {
                                    // No album

                                    // Grab Album Art
                                    byte[] cArt = null;
                                    string imgpath = String.Format(@"{0}\AlbumCovers\{1}.jpg", mb.FolderPath, i);
                                    if (System.IO.File.Exists(imgpath))
                                    {
                                        System.Drawing.Image img = System.Drawing.Image.FromFile(imgpath);
                                        using (MemoryStream ms = new MemoryStream())
                                        {
                                            img.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                                            cArt = ms.ToArray();
                                        }
                                    }



                                    AlbumLibrary dbAlbum = new AlbumLibrary()
                                    {
                                        AlbumName = album[1],
                                        MusicBrainzId = album[2],
                                        Year = int.Parse(album[3]),
                                        NumTracks = int.Parse(album[4]),
                                        isVerified = Boolean.Parse(album[5]),
                                        hasMusicBrainzUpdate = Boolean.Parse(album[6]),
                                        iTunesId = album[7],
                                        hasiTunesUpdate = Boolean.Parse(album[8]),
                                        isHidden = Boolean.Parse(album[9]),
                                        CoverArt = cArt
                                    };

                                    db.AlbumLibraries.Add(dbAlbum);
                                    db.SaveChanges();

                                    if (AlbumIndex == null)
                                        AlbumIndex = new List<IndexKey>();
                                    AlbumIndex.Add(new IndexKey() { ExportIndex = int.Parse(i), DBIndex = dbAlbum.AlbumId });
                                    //AlbumCoverDownloadList_iTunesID = new List<int>();
                                    AlbumCoverDownloadList_iTunesID.Add(dbAlbum.iTunesId);
                                }
                            }
                        }

                        // save artists
                        // "ArtistDetails[{0}|{1}|{2}|{3}|{4}]", a.ExportIndex, a.DbArtist.ArtistName, a.DbArtist.iTunesId, a.DbArtist.isVerified, a.DbArtist.hasiTunesUpdate));
                        foreach (string i in tempArtists)
                        {
                            if (!ArtistIndex.Any(a => a.ExportIndex.ToString() == i))
                            {
                                string[] artist = artistArray.Where(w => w[0] == i).First();
                                string tempItunesID = artist[2];
                                if (db.Artists.Where(w2 => w2.iTunesId == tempItunesID).Any())
                                {
                                    // Has artist
                                    ArtistIndex.Add(new IndexKey() { ExportIndex = int.Parse(i), DBIndex = db.Artists.Where(w2 => w2.iTunesId == tempItunesID).First().Id });


                                }
                                else
                                {
                                    // No artist
                                    Artist dbArtist = new Artist()
                                    {
                                        ArtistName = artist[1],
                                        iTunesId = artist[2],
                                        isVerified = Boolean.Parse(artist[3]),
                                        hasiTunesUpdate = Boolean.Parse(artist[4])
                                    };

                                    db.Artists.Add(dbArtist);
                                    db.SaveChanges();

                                    if (ArtistIndex == null)
                                        ArtistIndex = new List<IndexKey>();
                                    ArtistIndex.Add(new IndexKey() { ExportIndex = int.Parse(i), DBIndex = dbArtist.Id });
                                }
                            }
                        }

                        // save tracks
                        // get fresh copy

                        if (mb.hasDBTrackLibrary)
                        {
                            if (mb.BaseDBTrackLibrary == null)
                                mb.BaseDBTrackLibrary = db.TrackLibraries.Where(w => w.FilePath == mb.FilePath).First();

                            SongLibrary song = new SongLibrary();
                            bool isNewSong = true;

                            if (mb.BaseDBTrackLibrary.SongLibraries.Any())
                            {
                                song = db.TrackLibraries.Where(w => w.Id == mb.BaseDBTrackLibrary.Id).First().SongLibraries.First();
                                isNewSong = false;
                            }

                            song.Genre = tempSong.Genre;
                            song.SongName = tempSong.SongName;

                            foreach (var art in tempArtists)
                            {
                                var a = db.Artists.Find(ArtistIndex.Where(w => w.ExportIndex.ToString() == art).First().DBIndex);
                                song.Artists.Add(a);
                            }

                            foreach (var alb in tempAlbums)
                            {
                                var a = db.AlbumLibraries.Find(AlbumIndex.Where(w => w.ExportIndex.ToString() == alb).First().DBIndex);
                                song.AlbumLibraries.Add(a);
                            }
                            db.SaveChanges();


                            // get fresh track
                            var track = db.TrackLibraries.Find(mb.BaseDBTrackLibrary.Id);
                            track.Type = mb.ImportTypeString;
                            if (isNewSong)
                                track.SongLibraries.Add(song);

                            db.Entry(track).State = System.Data.Entity.EntityState.Modified;

                            db.SaveChanges();

                            mb.BaseDBTrackLibrary = track;
                            mb.Status = ImportFactory.ManagedFileStatus.itunesImported;
                            mb.isCompleteDB = true;
                            Global.ImportAnalytics.AddBreadcrumb(track.FilePath, "Loaded data from Export");
                        }
                        else
                        {
                            bool hasDatabaseForSomeReason = true;
                        }


                        // save track / song
                    }
                    //

                }
            }
        }

    }
}
