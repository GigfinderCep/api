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
    
    public partial class ChatRooms
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public ChatRooms()
        {
            this.Messages = new HashSet<Messages>();
        }
    
        public int id { get; set; }
        public int user_id1 { get; set; }
        public int user_id2 { get; set; }
    
        public virtual Users Users { get; set; }
        public virtual Users Users1 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Messages> Messages { get; set; }
    }
}
