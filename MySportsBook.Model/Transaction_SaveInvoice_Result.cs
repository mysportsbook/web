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
    
    public partial class Transaction_SaveInvoice_Result
    {
        public int PK_InvoiceId { get; set; }
        public string InvoiceNumber { get; set; }
        public int FK_VenueId { get; set; }
        public int FK_PlayerId { get; set; }
        public System.DateTime InvoiceDate { get; set; }
        public System.DateTime DueDate { get; set; }
        public decimal TotalFee { get; set; }
        public Nullable<decimal> TotalDiscount { get; set; }
        public Nullable<decimal> OtherAmount { get; set; }
        public Nullable<decimal> PaidAmount { get; set; }
        public string Comments { get; set; }
        public int FK_StatusId { get; set; }
        public int CreatedBy { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public Nullable<int> ModifiedBy { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
    }
}
