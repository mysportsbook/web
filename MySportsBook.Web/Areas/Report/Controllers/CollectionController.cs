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
        public ActionResult Daily(string Date)
        {
            var _date = DateTime.ParseExact((string.IsNullOrEmpty(Date) ? DateTime.Now.ToString("dd/MM/yyyy") : Date),"dd/MM/yyyy", null);
            return View(GetCollection(_date, "DAILY"));
        }

        public ActionResult Player()
        {
            return View(GetCollection(null, "PLAYER"));
        }

        public ActionResult Split()
        {
            return View(GetCollection(null, "SPLIT"));
        }

        [HttpPost]
        public ActionResult Split(DateTime? Month)
        {
            return View(GetCollection(Month, "SPLIT"));
        }

        [HttpPost]
        public ActionResult Player(DateTime? Month)
        {
            return View(GetCollection(Month, "PLAYER"));
        }

        [NonAction]
        private dynamic GetCollection(DateTime? month, string Type)
        {
            return dbContext.rp_COLLECTIONDETAIL(currentUser.CurrentVenueId, month.HasValue ? month.Value : DateTime.Now, Type);
        }
    }
}