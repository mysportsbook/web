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
    public class HomeController : BaseController
    {

        // GET: Studio/Home
        public ActionResult Index()
        {
            var _Events = dbContext.Studio_Event.Where(x => x.CreatedDate.Month == DateTime.Now.Month && x.CreatedDate.Year == DateTime.Now.Year).Select(x => x.PK_EventId).ToListAsync().Result;
            var _Expenses = dbContext.Studio_ExpenseDetail.Where(x => x.CreatedDate.Month == DateTime.Now.Month && x.CreatedDate.Year == DateTime.Now.Year).Select(x => x).ToListAsync().Result;
            var _Income = dbContext.Studio_IncomeDetail.Where(x => x.CreatedDate.Month == DateTime.Now.Month && x.CreatedDate.Year == DateTime.Now.Year).Select(x => x).ToListAsync().Result;

            Studio_Home_Index _details = new Studio_Home_Index()
            {
                CurrentMonthEvents = _Events.Count().ToString(),
                CurrentMonthExpenses = _Expenses.Sum(x => x.Amount).ToString(),
                CurrentMonthIncome = _Income.Sum(x => x.Amount).ToString()
            };
            //var studio_Event = dbContext.Studio_Event.Include(s => s.Configuration_User).Include(s => s.Configuration_Status);
            return View(_details);
        }
    }
}
