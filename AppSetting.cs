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
    
    public partial class AppSetting
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public string Subtype { get; set; }
        public string Value { get; set; }
    }

    public partial class PlaylistNG
    {
        public int Id { get; set; }
        public int TrackID { get; set; }
        public int PlaylistTypeID { get; set; }
    }
}
