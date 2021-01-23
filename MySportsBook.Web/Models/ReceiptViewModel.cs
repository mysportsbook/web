using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MySportsBook.Web.Models
{
    public class ReceiptViewModel
    {

        public string ReceiptNumber { get; set; }
        public System.DateTime ReceiptDate { get; set; }
        public string ReceiptPeriod { get; set; }
        public string InvoiceNumber { get; set; }
        public string Sport { get; set; }
        public string Batch { get; set; }
        public decimal TotalFee { get; set; }
        public decimal AmountPaid { get; set; }
        public string Description { get; set; }
        public string PaymentMode { get; set; }
        public string ReceivedBy { get; set; }
        public string PlayerId { get; set; }
        public bool IsFullPaymentHistory { get; set; }
    }
}