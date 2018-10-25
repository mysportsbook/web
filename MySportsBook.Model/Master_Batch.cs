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
    
    public partial class Master_Batch
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Master_Batch()
        {
            this.Master_BatchTiming = new HashSet<Master_BatchTiming>();
            this.Transaction_Attendance = new HashSet<Transaction_Attendance>();
            this.Transaction_InvoiceDetail = new HashSet<Transaction_InvoiceDetail>();
            this.Transaction_PlayerSport = new HashSet<Transaction_PlayerSport>();
        }
    
        public int PK_BatchId { get; set; }
        public int FK_VenueId { get; set; }
        public string BatchName { get; set; }
        public string BatchCode { get; set; }
        public int FK_CourtId { get; set; }
        public decimal Fee { get; set; }
        public int MaxPlayers { get; set; }
        public int FK_BatchTypeId { get; set; }
        public Nullable<int> FK_CoachId { get; set; }
        public Nullable<System.DateTime> StartDate { get; set; }
        public Nullable<System.DateTime> EndDate { get; set; }
        public bool IsAttendanceRequired { get; set; }
        public bool IsFull { get; set; }
        public int FK_StatusId { get; set; }
        public int CreatedBy { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public Nullable<int> ModifiedBy { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
    
        public virtual Configuration_BatchType Configuration_BatchType { get; set; }
        public virtual Configuration_Status Configuration_Status { get; set; }
        public virtual Configuration_User Configuration_User { get; set; }
        public virtual Configuration_User Configuration_User1 { get; set; }
        public virtual Master_Court Master_Court { get; set; }
        public virtual Master_Player Master_Player { get; set; }
        public virtual Master_Venue Master_Venue { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Master_BatchTiming> Master_BatchTiming { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Transaction_Attendance> Transaction_Attendance { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Transaction_InvoiceDetail> Transaction_InvoiceDetail { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Transaction_PlayerSport> Transaction_PlayerSport { get; set; }
    }
}
