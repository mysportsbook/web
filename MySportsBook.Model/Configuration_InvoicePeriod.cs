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
    
    public partial class Configuration_InvoicePeriod
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Configuration_InvoicePeriod()
        {
            this.Transaction_PlayerSport = new HashSet<Transaction_PlayerSport>();
        }
    
        public int PK_InvoicePeriodId { get; set; }
        public string InvoicePeriod { get; set; }
        public string InvoicePeriodCode { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Transaction_PlayerSport> Transaction_PlayerSport { get; set; }
    }
}
