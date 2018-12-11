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
    
    public partial class Studio_Event
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Studio_Event()
        {
            this.Studio_ExpenseDetail = new HashSet<Studio_ExpenseDetail>();
            this.Studio_IncomeDetail = new HashSet<Studio_IncomeDetail>();
        }
    
        public int PK_EventId { get; set; }
        public string OrderNumber { get; set; }
        public string CustomerName { get; set; }
        public string Mobile { get; set; }
        public string EmailId { get; set; }
        public string Description { get; set; }
        public System.DateTime EventDate { get; set; }
        public string Venue { get; set; }
        public string Remarks { get; set; }
        public Nullable<decimal> Amount { get; set; }
        public int FK_StatusId { get; set; }
        public int CreatedBy { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public Nullable<int> ModifiedBy { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
    
        public virtual Configuration_Status Configuration_Status { get; set; }
        public virtual Configuration_User Configuration_User { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Studio_ExpenseDetail> Studio_ExpenseDetail { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Studio_IncomeDetail> Studio_IncomeDetail { get; set; }
    }
}
