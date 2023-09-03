
using System;
using System.Collections.Generic;
using System.Linq;

namespace JukeBoxSolutions.Services
{
    public class PlaylistServices
    {
        public PlaylistServices() { }
        public void AddToPlaylist(int trackID)
        {
            try
            {
                using (JukeboxBrainsDBEntities db=new JukeboxBrainsDBEntities())
                {
                    db.Database.ExecuteSqlCommand($"EXEC [dbo].[sp_addToPlaylist] @TrackID = {trackID}, @PlaylistTypeID = {(int)PlaylistType.BufferPlaylist}");
                }   
            }
            catch (System.Exception)
            {

            }
        }
        public void RemoveFromPlaylist(int trackID)
        {
            try
            {
                using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                {
                    db.Database.ExecuteSqlCommand($"EXEC [dbo].[sp_removeFromPlaylist] @TrackID = {trackID}");
                }
            }
            catch (System.Exception)
            {

            }
        }
        public void QueueNextPlaylist(int trackID)
        {

        }

        internal void AddToPlaylist(IEnumerable<int> listOfTrackIds)
        {
            try
            {
                using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                {
                    listOfTrackIds.ToList().ForEach(x =>
                    {
                        db.Database.ExecuteSqlCommand($"EXEC [dbo].[sp_addToPlaylist] @TrackID = {x}, @PlaylistTypeID = {(int)PlaylistType.BufferPlaylist}");
                    });
                    
                }
            }
            catch (System.Exception)
            {

            }
        }
    }

    public enum PlaylistType
    {
        BufferPlaylist=1,
        NowPlaying=2
    }
}
