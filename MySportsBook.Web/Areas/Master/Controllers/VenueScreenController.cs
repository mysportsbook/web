using MySportsBook.Web.Controllers;
using MySportsBook.Web.Filters;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace MySportsBook.Web.Areas.Master.Controllers
{
    [UserAuthentication]
    public class VenueScreenController : BaseController
    {
        // GET: Master/VenueScreen
        public async Task<ActionResult> Index()
        {
            var ddlList = new List<SelectListItem>();
            ddlList.Add(new SelectListItem
            {
                Text = "--Select--",
                Value = "0"
            });
            dbContext.Configuration_Format.ToList().ForEach(x =>
            {
                ddlList.Add(new SelectListItem
                {
                    Text = $"{ x.NumberFormat}-{x.Divider}-{x.NumberLength}",
                    Value = x.PK_NumberFormatId.ToString()
                });
            });
            ViewBag.NumberFormat = ddlList;
            var master_VenueScreen = dbContext.Master_VenueScreen
                                        .Where(x => x.FK_StatusId == 1 && x.FK_VenueId == currentUser.CurrentVenueId).OrderByDescending(x => x.FK_ScreenId)
                                        .Include(m => m.Configuration_Status).Include(i => i.Configuration_Screen);
            return View(await master_VenueScreen.ToListAsync());
        }
    }
}