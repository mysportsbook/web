using MySportsBook.Web.Controllers;
using MySportsBook.Web.Filters;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace MySportsBook.Web.Areas.Configuration.Controllers
{
    [UserAuthentication]
    public class UserAccessController : BaseController
    {
        // GET: Configuration/UserAccess
        public ActionResult Index()
        {
            return View();
        }

       
    }
}