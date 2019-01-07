using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MySportsBook.Model.ViewModel
{
    public class ReceiptModel
    {
        public string ReceiptNumber { get; set; }
        public System.DateTime ReceiptDate { get; set; }
        public int VenueId { get; set; }
        public int InvoiceId { get; set; }
        public string InvoiceNumber { get; set; }
        public string Sport { get; set; }
        public string Month { get; set; }
        public decimal TotalFee { get; set; }
        public Nullable<decimal> TotalOtherAmount { get; set; }
        public Nullable<decimal> TotalDiscountAmount { get; set; }
        public decimal AmountPaid { get; set; }
        public string Description { get; set; }
        public int PaymentModeId { get; set; }
        public string PaymentMode { get; set; }
        public string TransactionNumber { get; set; }
        public Nullable<System.DateTime> TransactionDate { get; set; }
        public int FK_StatusId { get; set; }
        public int CreatedBy { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public Nullable<int> ModifiedBy { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        
        public Nullable<int> ReceivedBy { get; set; }
        public string Received { get; set; }
    }
}
