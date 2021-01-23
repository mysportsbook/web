using MySportsBook.Web.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MySportsBook.Web.Areas.Master.Controllers
{
    public class ScreenController : BaseController
    {
        // GET: Master/Screen
        public ActionResult Index()
        {
            return View();
        }
    }
}