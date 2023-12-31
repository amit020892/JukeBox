﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace JukeBoxSolutions
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Core.Objects;
    using System.Linq;
    
    public partial class JukeboxBrainsDBEntities : DbContext
    {
        public JukeboxBrainsDBEntities()
            : base("name=JukeboxBrainsDBEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<AppSetting> AppSettings { get; set; }
        public virtual DbSet<AlbumLibrary> AlbumLibraries { get; set; }
        public virtual DbSet<TrackLibrary> TrackLibraries { get; set; }
        public virtual DbSet<PlayListDetail> PlayListDetails { get; set; }
        public virtual DbSet<Playlist> Playlists { get; set; }
        public virtual DbSet<LibraryView> LibraryViews { get; set; }
        public virtual DbSet<Artist> Artists { get; set; }
        public virtual DbSet<SongLibrary> SongLibraries { get; set; }
    
        public virtual int BuildLibraryView()
        {
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("BuildLibraryView");
        }
    }
}
