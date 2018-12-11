using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using MySportsBook.Model;
using MySportsBook.Web.Controllers;
using MySportsBook.Model.ViewModel;
using MySportsBook.Web.Filters;

namespace MySportsBook.Web.Areas.Studio.Controllers
{
    [UserAuthentication]
    public class ReportsController : BaseController
    {

        // GET: Studio/Reports
        public async Task<ActionResult> Index()
        {
            Transaction_Details _transactionDetails = new Transaction_Details();
            return View(_transactionDetails);
        }

        [HttpPost]
        public ActionResult Index(Transaction_Details transaction_Details)
        {
            Transaction_Details _transactionDetails = new Transaction_Details();
            List<Transactions> _transactions = new List<Transactions>();
            _transactions = dbContext.Studio_IncomeDetail.Where(x => DbFunctions.TruncateTime(x.CreatedDate) >= DbFunctions.TruncateTime(transaction_Details.StartDate) && DbFunctions.TruncateTime(x.CreatedDate) <= DbFunctions.TruncateTime(transaction_Details.EndDate))
                                                         .Select(x => new Transactions { Amount = x.Amount, Description = x.Description, TransactionId = x.PK_IncomeDetailId, EventId = x.FK_EventId, CustomerName = x.Studio_Event.CustomerName, TransactionType = "Credit", CreatedDate = x.CreatedDate, SpendBy = x.ReceivedBy }).ToList();
            var _Expenses = dbContext.Studio_ExpenseDetail.Where(x => DbFunctions.TruncateTime(x.CreatedDate) >= DbFunctions.TruncateTime(transaction_Details.StartDate) && DbFunctions.TruncateTime(x.CreatedDate) <= DbFunctions.TruncateTime(transaction_Details.EndDate))
                                                          .Select(x => new Transactions { Amount = x.Amount, CustomerName = x.Studio_Event.CustomerName, Description = x.Description, EventId = x.FK_EventId, TransactionId = x.PK_ExpenseDetailId, TransactionType = "Debit", SpendBy = x.SpentBy, CreatedDate = x.CreatedDate }).ToList();
            _transactions.AddRange(_Expenses);
            _transactions.OrderByDescending(x => x.CreatedDate);
            _transactionDetails.TransactionList = _transactions;
            _transactionDetails.TotalIncome = _transactions.Where(x => x.TransactionType == "Credit").Sum(x => x.Amount).ToString();
            _transactionDetails.TotalExpanses= _transactions.Where(x => x.TransactionType == "Debit").Sum(x => x.Amount).ToString();

            return View(_transactionDetails);
        }

    }
}
