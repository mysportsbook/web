using MySportsBook.Model;
using MySportsBook.Web.Filters;
using MySportsBook.Web.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MySportsBook.Web.Controllers
{
    [UserAuthentication]
    public class DropDownController : BaseController
    {
        // GET: DropDown
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult GetBatch(string sportid)
        {
            int _sportid;
            List<DropDownItem> selectListBatches = new List<DropDownItem>();
            if (!string.IsNullOrEmpty(sportid))
            {
                _sportid = Convert.ToInt32(sportid);
                var batches = dbContext.Master_Batch.Where(b => b.FK_StatusId == 1 && b.FK_VenueId == currentUser.CurrentVenueId)
                                              .Join(dbContext.Master_Court.Where(c => c.FK_StatusId == 1 && c.FK_VenueId == currentUser.CurrentVenueId), batch => batch.FK_CourtId, cou => cou.PK_CourtId, (batch, cou) => new { Batch = batch, Court = cou })
                                              .Join(dbContext.Master_Sport.Where(s => s.FK_StatusId == 1 && s.FK_VenueId == currentUser.CurrentVenueId), batch => batch.Court.FK_SportId, spo => spo.PK_SportId, (batch, sport) => new { Batch = batch, Sport = sport }).Where(b => b.Batch.Batch.FK_VenueId == currentUser.CurrentVenueId && b.Batch.Court.FK_VenueId == currentUser.CurrentVenueId && b.Sport.FK_VenueId == currentUser.CurrentVenueId && b.Sport.PK_SportId == _sportid && b.Sport.FK_StatusId == 1 && b.Batch.Batch.FK_StatusId == 1);

                if (batches != null)
                {
                    selectListBatches.Add(new DropDownItem { Text = "--Select--", Value = string.Empty, Fee = "0" });
                    batches.ToList().ForEach(x =>
                    {
                        selectListBatches.Add(new DropDownItem { Text = x.Batch.Batch.BatchName + " - " + x.Batch.Court.CourtName, Value = x.Batch.Batch.PK_BatchId.ToString(), Fee = x.Batch.Batch.Fee.ToString("0.00") });
                    });
                }
                else selectListBatches.Add(new DropDownItem { Text = "No Batch(s) Found.", Value = string.Empty });
            }
            return Json(selectListBatches, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetCourt(string sportid)
        {
            int _sportid;
            List<DropDownItem> selectListcourts = new List<DropDownItem>();
            if (!string.IsNullOrEmpty(sportid))
            {
                _sportid = Convert.ToInt32(sportid);
                var batches = dbContext.Master_Court.Where(c => c.FK_StatusId == 1 && c.FK_VenueId == currentUser.CurrentVenueId)
                                              .Join(dbContext.Master_Sport.Where(s => s.FK_StatusId == 1 && s.FK_VenueId == currentUser.CurrentVenueId), court => court.FK_SportId, spo => spo.PK_SportId, (court, sport) => new { Court = court, Sport = sport }).Where(b => b.Sport.PK_SportId == _sportid);

                if (batches != null)
                {
                    selectListcourts.Add(new DropDownItem { Text = "--Select--", Value = string.Empty, Fee = "0" });
                    batches.ToList().ForEach(x =>
                    {
                        selectListcourts.Add(new DropDownItem { Text = x.Court.CourtName, Value = x.Court.PK_CourtId.ToString(), Fee = x.Sport.Fee.ToString("0.00") });
                    });
                }
                else selectListcourts.Add(new DropDownItem { Text = "No Court(s) Found.", Value = string.Empty });
            }
            return Json(selectListcourts, JsonRequestBehavior.AllowGet);
        }
    }
}
