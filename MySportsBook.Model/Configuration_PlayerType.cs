//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MySportsBook.Model
{
    using System;
    using System.Collections.Generic;
    
    public partial class Configuration_PlayerType
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Configuration_PlayerType()
        {
            this.Master_Player = new HashSet<Master_Player>();
        }
    
        public int PK_PlayerTypeId { get; set; }
        public string PlayerType { get; set; }
        public string PlayerTypeCode { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Master_Player> Master_Player { get; set; }
    }
}
