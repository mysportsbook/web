using MySportsBook.Model;
using MySportsBook.Model.ViewModel;
using System.Collections.Generic;
using System.Linq;
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
                if (currentUser.CurrentVenueId > 0)
                {
                    GetMenu(1);
                }
            }
            else
            {
                currentUser = new CurrentUser();
                baseuser = new Configuration_User();
            }
        }

        private List<string> GetMenu(int RoleId)
        {
            List<string> _menu = new List<string>();
            var _userscreen = dbContext.Master_UserRole.Where(x => x.FK_StatusId == 1 && x.FK_RoleId == RoleId && x.FK_UserId == currentUser.UserId && x.FK_VenueId == currentUser.CurrentVenueId)
                  .Join(dbContext.Master_RoleScreen.Where(x => x.FK_StatusId == 1 && x.FK_VenueId == currentUser.CurrentVenueId), ur => ur.FK_RoleId, rs => rs.FK_RoleId, (userrole, rolescreen) => new { userrole, rolescreen })
                  .Join(dbContext.Master_VenueScreen.Where(x => x.FK_StatusId == 1 && x.FK_VenueId == currentUser.CurrentVenueId), urs => urs.rolescreen.FK_VenueScreenId, vss => vss.PK_VenueScreenId, (userrolescreen, venuescreen) => new { userrolescreen, venuescreen })
                  .Join(dbContext.Configuration_Screen, cs => cs.venuescreen.FK_ScreenId, s => s.PK_ScreenId, (userrolevenuescreen, conscreen) => new { userrolevenuescreen, conscreen })
                  .Join(dbContext.Configuration_ScreenType, cs => cs.conscreen.FK_ScreenTypeId, s => s.PK_ScreenTypeId, (userrolevenuescreencon, screentype) => new { userrolevenuescreencon, screentype }).ToList();
            if (_userscreen != null && _userscreen.Count > 0)
            {
                foreach (var _divider in _userscreen.OrderBy(x => x.screentype.PK_ScreenTypeId).GroupBy(x => x.screentype.ScreenType))
                {
                    var _tem = _divider;
                    _menu.Add($"<li class='nav-title'>{_divider.Key}</li>");
                    foreach (var item in _tem.Select(x => x.userrolevenuescreencon.conscreen))
                    {
                        _menu.Add($"<li class='nav-item'>"
                                + $"<a class='nav-link' href='@Url.Action('', '{item.Controller}', new {{ Area = '{item.Area}' }})'>"
                                + $" <i class='nav-icon fa fa-file-text-o'></i> {item.Screen}"
                                + "</a>"
                                + "</li>");
                    }
                }
            }
            return _menu;
        }
    }
}