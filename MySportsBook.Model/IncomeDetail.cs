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
    
    public partial class IncomeDetail
    {
        public int PK_IncomeDetailId { get; set; }
        public System.DateTime ReceivedDate { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public string ReceivedBy { get; set; }
        public int CreatedBy { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public int FK_StatusId { get; set; }
        public int FK_EventId { get; set; }
    
        public virtual Configuration_Status Configuration_Status { get; set; }
        public virtual StudioEvent StudioEvent { get; set; }
    }
}
