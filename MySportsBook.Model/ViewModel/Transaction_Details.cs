using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MySportsBook.Model.ViewModel
{
    public class Transactions
    {
        public string CustomerName { get; set; }
        public int EventId { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public string TransactionType { get; set; }
        public int TransactionId { get; set; }
        public string SpendBy { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class Transaction_Details
    {
        public string TotalIncome { get; set; }
        public string TotalExpanses { get; set; }
        public string OpeningBalance { get; set; }
        public string ClosingBalance { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<Transactions> TransactionList { get; set; }
    }
}
