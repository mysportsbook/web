using MySportsBook.Web.Controllers;
using MySportsBook.Web.Filters;
using System.Data;
using System.Web.Mvc;

namespace MySportsBook.Web.Areas.Report.Controllers
{
    [UserAuthentication]
    public class CustomController : BaseController
    {
        // GET: Report/Custom
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Defaulters()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Defaulters(string Month)
        {
            return View(CalltheCustomStoreProc(currentUser.CurrentVenueId, "Defaulter", Month));
        }

        [HttpPost]
        public ActionResult Index(string ReportType, string Month)
        {
            return View(CalltheCustomStoreProc(currentUser.CurrentVenueId, ReportType, Month));
        }

        [NonAction]
        private DataTable CalltheCustomStoreProc(int venueID, string storeProc, string parameters)
        {
            if (currentUser.UserId < 4 || storeProc == "Defaulter")
                return dbContext.GetResultReport(venueID, storeProc, parameters);
            return new DataTable();
        }

       
    }
}