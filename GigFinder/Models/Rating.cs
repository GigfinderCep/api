//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace GigFinder.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Rating
    {
        public int user_id { get; set; }
        public int event_id { get; set; }
        public byte avg_rating { get; set; }
        public string content { get; set; }
    
        public virtual Event Event { get; set; }
        public virtual User User { get; set; }
    }
}
