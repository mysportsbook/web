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
    
    public partial class Studio_ExpenseDetail
    {
        public int PK_ExpenseDetailId { get; set; }
        public int FK_EventId { get; set; }
        public int FK_ExpenseTypeId { get; set; }
        public string SpentBy { get; set; }
        public string Description { get; set; }
        public System.DateTime SpentDate { get; set; }
        public decimal Amount { get; set; }
        public int FK_StatusId { get; set; }
        public int CreatedBy { get; set; }
        public System.DateTime CreatedDate { get; set; }
    
        public virtual Configuration_Status Configuration_Status { get; set; }
        public virtual Configuration_User Configuration_User { get; set; }
        public virtual Studio_Event Studio_Event { get; set; }
        public virtual Studio_ExpenseType Studio_ExpenseType { get; set; }
    }
}
