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

        [HttpPost]
        public ActionResult Index(string StoreProc,string parameters)
        {
            return View(CalltheCustomStoreProc(StoreProc, parameters));
        }

        [NonAction]
        private DataTable CalltheCustomStoreProc(string StoreProc, string parameters)
        {
            return dbContext.GetResultReport(StoreProc,parameters);
           
        }
    }
}