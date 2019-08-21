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
    
    public partial class V_InvoiceDetails
    {
        public string InvoiceNumber { get; set; }
        public int VenueId { get; set; }
        public int PlayerId { get; set; }
        public int BatchId { get; set; }
        public decimal BatchAmount { get; set; }
        public string InvoicePeriod { get; set; }
        public System.DateTime InvoiceDate { get; set; }
        public string ReceivedBy { get; set; }
        public System.DateTime DueDate { get; set; }
        public decimal TotalFee { get; set; }
        public Nullable<decimal> TotalDiscount { get; set; }
        public Nullable<decimal> OtherAmount { get; set; }
        public Nullable<decimal> InvoicePaidAmount { get; set; }
        public decimal DetailsPaidAmount { get; set; }
        public decimal ReceiptPaidAmount { get; set; }
        public string Comments { get; set; }
    }
}
