using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MySportsBook.Web.Areas.Transaction.Models
{
    public class PlayerReceiptViewModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string YearMonth { get; set; }
        public string MonthFormat { get; set; }
        public string PK_PlayerId { get; set; }
        public decimal AmountPaid { get; set; }
        public string Years { get; set; }
        public string Mobile { get; set; }
        public string Batch { get; set; }
        public int FK_StatusId { get; set; }
        public string Sport { get; set; }
        public string ReceiptNumber { get; set; }
    }
}