using MySportsBook.Web.Controllers;
using MySportsBook.Web.Filters;
using System;
using System.Web.Mvc;

namespace MySportsBook.Web.Areas.Report.Controllers
{
    [UserAuthentication]
    public class CollectionController : BaseController
    {
        // GET: Report/Collection
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Daily()
        {
            return View(GetCollection(null, "DAILY"));
        }
        [HttpPost]
        public ActionResult Daily(DateTime Date)
        {
            return View(GetCollection(Date, "DAILY"));
        }

        public ActionResult Player()
        {
            return View(GetCollection(null, "PLAYER"));
        }
        [HttpPost]
        public ActionResult Player(DateTime Month)
        {
            return View(GetCollection(Month, "PLAYER"));
        }

        [NonAction]
        private dynamic GetCollection(DateTime? month, string Type)
        {
           return  dbContext.rp_COLLECTIONDETAIL(currentUser.CurrentVenueId, month.HasValue ? month.Value : DateTime.Now, Type);
        }
    }
}