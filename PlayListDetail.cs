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
    
    public partial class PlayListDetail
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public PlayListDetail()
        {
            this.Playlists = new HashSet<Playlist>();
        }
    
        public int Id { get; set; }
        public string Name { get; set; }
        public int Type { get; set; }
        public bool isVideo { get; set; }
        public bool isMusic { get; set; }
        public bool isKaraoke { get; set; }
        public bool isRadio { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Playlist> Playlists { get; set; }
    }
}
