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
    
    public partial class Playlist
    {
        public int Id { get; set; }
        public int TrackId { get; set; }
        public int PlaylistId { get; set; }
        public int SequenceNumber { get; set; }
    
        public virtual PlayListDetail PlayListDetail { get; set; }
        public virtual TrackLibrary TrackLibrary { get; set; }
    }
}
