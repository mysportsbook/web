using System;
using System.Collections.Generic;
namespace MySportsBook.Model.ViewModel
{
    public class InvoiceModel
    {
        public int PaymentId { get; set; }
        public int InvoiceId { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public int PlayerId { get; set; }
        public int VenueId { get; set; }
        public double TotalFee { get; set; }
        public double TotalOtherAmount { get; set; }
        public double TotalDiscount { get; set; }
        public double TotalPaidAmount { get; set; }
        public double ExtraPaidAmount { get; set; }
        public string Comments { get; set; }
        public string TransactionNo { get; set; }
        public DateTime? TransactionDate { get; set; }
        public int ReceivedBy { get; set; }
        public bool NoDues { get; set; }
        public List<InvoiceDetailModel> InvoiceDetails { get; set; }
        public bool IsGuestPlayer { get; set; }
        /*Receipt*/
        public string ReceiptNumber { get; set; }
        public int ReceiptId { get; set; }
    }
    public class InvoiceDetailModel
    {
        public int InvoiceId { get; set; }
        public int InvoiceDetailssId { get; set; }
        public int InvoicePeriodId { get; set; }
        public int BatchId { get; set; }
        public string BatchName { get; set; }
        public string SportName { get; set; }
        public string InvoicePeriod { get; set; }
        public double Fee { get; set; }
        public int PayOrder { get; set; }
        public double PaidAmount { get; set; }
        public int StatusId { get; set; }
        public int PlayerId { get; set; }
        public string Comments { get; set; }
        public bool ShouldClose { get; set; }
        public string LastPaidDetails { get; set; }
        public DateTime InvoicePeriodDate
        {
            get
            {
               return InvoicePeriod.Split('-').Length > 1 ? DateTime.ParseExact(InvoicePeriod.Split('-')[1], "MMMyyyy", System.Globalization.CultureInfo.InvariantCulture): DateTime.ParseExact(InvoicePeriod, "MMMyyyy", System.Globalization.CultureInfo.InvariantCulture);
            }
        }
       
    }


    public class InvoiceReportModel
    {
        public string Name { get; set; }
        public string Batch { get; set; }
        public string Mobile { get; set; }
        public string Month { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; }
    }

    public class InvoiceReportSearchModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool Collected { get; set; }
        public bool Pending { get; set; }

    }

}
