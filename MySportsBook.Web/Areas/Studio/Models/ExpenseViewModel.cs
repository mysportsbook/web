using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MySportsBook.Web.Areas.Studio.Models
{
    public class ExpenseViewModel
    {
        public string Description { get; set; }
        public string FirstName { get; set; }
        public DateTime SpentDate { get; set; }
        public Decimal Amount { get; set; }
        public int PK_ExpenseDetailId { get; set; }
    }
}