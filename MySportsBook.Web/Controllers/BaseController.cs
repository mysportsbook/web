using MySportsBook.Model;
using MySportsBook.Model.ViewModel;
using System.Web.Mvc;

namespace MySportsBook.Web.Controllers
{
    public class BaseController : Controller
    {
        public MySportsBookEntities dbContext;
        public CurrentUser currentUser;
        public Configuration_User baseuser;
        public BaseController()
        {
            dbContext = new MySportsBookEntities();
            if (System.Web.HttpContext.Current.Session != null && System.Web.HttpContext.Current.Session["CURRENTUSER"] != null)
            {
                currentUser = System.Web.HttpContext.Current.Session["CURRENTUSER"] as CurrentUser;
                if (currentUser.UserId == 0)
                {
                    ViewBag.IsAdmin = true;
                }
                ViewBag.Name = $"{currentUser.FirstName}";
                ViewBag.Venue = currentUser.CurrentVenueName;
            }
            else
            {
                currentUser = new CurrentUser();
                baseuser = new Configuration_User();
            }
        }
    }
}