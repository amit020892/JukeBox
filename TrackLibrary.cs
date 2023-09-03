//------------------------------------------------------------------------------
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
    using System.Collections.Generic;
    
    public partial class TrackLibrary
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TrackLibrary()
        {
            this.Playlists = new HashSet<Playlist>();
            this.SongLibraries = new HashSet<SongLibrary>();
        }
    
        public int Id { get; set; }
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public string Type { get; set; }
        public string Extention { get; set; }
        public bool IsFavorite { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Playlist> Playlists { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SongLibrary> SongLibraries { get; set; }
    }
}
