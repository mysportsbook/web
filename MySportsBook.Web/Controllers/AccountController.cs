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

namespace MySportsBook.Web.Controllers
{
    public class AccountController : BaseController
    {
        // GET: Account
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(Configuration_User configuration_User)
        {
            Configuration_User _configuration_User = new Configuration_User();
            if (Verify(configuration_User.UserName, configuration_User.PasswordHash))
            {
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            return Json(false, JsonRequestBehavior.AllowGet);
        }

        // GET: Account
        public ActionResult Home()
        {
            return View();
        }

        [UserAuthentication]
        // GET: Account
        public ActionResult SelectVenue()
        {
            if (dbContext.Master_UserVenue.Where(v => v.FK_UserId == currentUser.UserId && v.FK_StatusId == 1).Count() > 1) { return View(); }
            else
            {
                var _userVenue = dbContext.Master_UserVenue.Where(v => v.FK_UserId == currentUser.UserId && v.FK_StatusId == 1);
                if (_userVenue != null && _userVenue.Count() > 0)
                {
                    currentUser.CurrentVenueId = Convert.ToInt32(_userVenue.FirstOrDefault().FK_VenueId);
                    var _venue = dbContext.Master_Venue.Where(x => x.FK_StatusId == 1).ToList().Find(v => v.PK_VenueId == Convert.ToInt32(currentUser.CurrentVenueId));
                    currentUser.CurrentVenueName = _venue != null ? _venue.VenueName : string.Empty;
                    System.Web.HttpContext.Current.Session["CURRENTUSER"] = currentUser;
                    return RedirectToAction("Home");
                }
                return View();
            }
        }


        // GET: Account
        public ActionResult SetVenue(string id)
        {
            if (Convert.ToInt32(id) > 0)
            {
                currentUser.CurrentVenueId = Convert.ToInt32(id);
                var _venue = dbContext.Master_Venue.Where(x => x.FK_StatusId == 1).ToList().Find(v => v.PK_VenueId == Convert.ToInt32(id));
                currentUser.CurrentVenueName = _venue != null ? _venue.VenueName : string.Empty;
                System.Web.HttpContext.Current.Session["CURRENTUSER"] = currentUser;
                return RedirectToAction("Home");
            }
            return RedirectToAction("Login");
        }

        [HttpGet]
        public ActionResult GetVenueList()
        {
            return PartialView("_VenueList", dbContext.Master_UserVenue.Include(c => c.Master_Venue).ToList().FindAll(v => v.FK_UserId == currentUser.UserId && v.FK_StatusId == 1));
        }



        [NonAction]
        private bool Verify(string Username, string Password)
        {
            var user = dbContext.Configuration_User.Include(c => c.Master_UserVenue).Where(u => u.FK_StatusId == 1).ToList().Find(u => u.UserName.ToLower().Equals(Username.ToLower()) || u.Email.ToLower().Equals(Username.ToLower()) || u.Mobile.ToLower().Equals(Username.ToLower()));
            if (user != null)
            {
                baseuser = user;
                return Cryptography.Verify(user.PasswordSalt, user.PasswordHash, Password) ? SetSession(user) : false;
            }
            return false;
        }

        [NonAction]
        private bool SetSession(Configuration_User configuration_User)
        {
            System.Web.HttpContext.Current.Session["CURRENTUSER"] = new CurrentUser()
            {
                UserId = configuration_User.PK_UserId,
                FirstName = configuration_User.FirstName,
                LastName = configuration_User.LastName,
                Email = configuration_User.Email,
                Mobile = configuration_User.Mobile
            };
            return true;
        }

        // GET: Account/Logout
        public ActionResult Logout()
        {
            System.Web.HttpContext.Current.Session["CURRENTUSER"] = null;
            return RedirectToAction("Login");
        }
    }
}