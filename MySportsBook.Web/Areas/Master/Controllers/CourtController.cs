using MySportsBook.Model;
using MySportsBook.Web.Controllers;
using MySportsBook.Web.Filters;
using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace MySportsBook.Web.Areas.Master.Controllers
{
    [UserAuthentication]
    public class CourtController : BaseController
    {

        // GET: Master/Court
        public async Task<ActionResult> Index()
        {
            var master_Court = dbContext.Master_Court.Where(x => x.FK_VenueId == currentUser.CurrentVenueId && x.FK_StatusId == 1).OrderByDescending(x => x.CreatedDate).Include(m => m.Configuration_Status).Include(m => m.Master_Sport);
            return View(await master_Court.ToListAsync());
        }


        // GET: Master/Court/Create
        public ActionResult Create()
        {
            ViewBag.FK_SportId = new SelectList(dbContext.Master_Sport.Where(x => x.FK_VenueId == currentUser.CurrentVenueId && x.FK_StatusId == 1), "PK_SportId", "SportName");
            return View();
        }

        // POST: Master/Court/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public async Task<ActionResult> Create([Bind(Include = "CourtName,CourtCode,FK_SportId")] Master_Court master_Court)
        {
            if (ModelState.IsValid)
            {
                try
                {

                    if (dbContext.Master_Court.Any(x => x.CourtName == master_Court.CourtName && x.FK_StatusId == 1 && x.FK_VenueId == currentUser.CurrentVenueId))
                    {
                        return Json(false, JsonRequestBehavior.AllowGet);
                    }

                    if (dbContext.Master_Court.Any(x => x.CourtCode == master_Court.CourtCode && x.FK_StatusId == 1 && x.FK_VenueId == currentUser.CurrentVenueId))
                    {
                        return Json(false, JsonRequestBehavior.AllowGet);
                    }

                    master_Court.FK_VenueId = currentUser.CurrentVenueId;
                    master_Court.FK_StatusId = 1;
                    master_Court.CreatedBy = currentUser.UserId;
                    master_Court.CreatedDate = DateTime.Now.ToLocalTime();
                    dbContext.Master_Court.Add(master_Court);
                    await dbContext.SaveChangesAsync();
                    return Json(true, JsonRequestBehavior.AllowGet);
                }
                catch (Exception)
                {

                    return Json(false, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(false, JsonRequestBehavior.AllowGet);
        }

        // GET: Master/Court/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var master_Court = dbContext.Master_Court.Where(x => x.FK_VenueId == currentUser.CurrentVenueId && x.PK_CourtId == id && x.FK_StatusId == 1);
            if (master_Court == null || master_Court.Count() == 0)
            {
                return HttpNotFound();
            }
            ViewBag.FK_SportId = new SelectList(dbContext.Master_Sport.Where(x => x.FK_StatusId == 1 && x.FK_VenueId == currentUser.CurrentVenueId), "PK_SportId", "SportName", master_Court.FirstOrDefault().FK_SportId);
            return View(master_Court.FirstOrDefault());
        }

        // POST: Master/Court/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public async Task<ActionResult> Edit([Bind(Include = "PK_CourtId,CourtName,CourtCode,FK_SportId")] Master_Court master_Court)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var _Courts = dbContext.Master_Court.Where(x => x.FK_VenueId == currentUser.CurrentVenueId && x.PK_CourtId == master_Court.PK_CourtId && x.FK_StatusId == 1);
                    if (_Courts == null || _Courts.Count() == 0)
                    {
                        return HttpNotFound();
                    }
                    if (dbContext.Master_Court.Any(x => x.CourtName == master_Court.CourtName && x.FK_StatusId == 1 && x.FK_VenueId == currentUser.CurrentVenueId && x.PK_CourtId != master_Court.PK_CourtId))
                    {
                        return Json(false, JsonRequestBehavior.AllowGet);
                    }

                    if (dbContext.Master_Court.Any(x => x.CourtCode == master_Court.CourtCode && x.FK_StatusId == 1 && x.FK_VenueId == currentUser.CurrentVenueId && x.PK_CourtId != master_Court.PK_CourtId))
                    {
                        return Json(false, JsonRequestBehavior.AllowGet);
                    }

                    var _court = _Courts.FirstOrDefault();
                    if (_court == null)
                    {
                        return HttpNotFound();
                    }
                    _court.CourtCode = master_Court.CourtCode;
                    _court.CourtName = master_Court.CourtName;
                    _court.FK_SportId = master_Court.FK_SportId;
                    _court.FK_StatusId = 1;
                    _court.ModifiedBy = currentUser.UserId;
                    _court.ModifiedDate = DateTime.Now.ToLocalTime();
                    dbContext.Entry(_court).State = EntityState.Modified;
                    await dbContext.SaveChangesAsync();
                    return Json(true, JsonRequestBehavior.AllowGet);
                }
                catch (Exception)
                {

                    return Json(false, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(false, JsonRequestBehavior.AllowGet);
        }

        // GET: Master/Court/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            try
            {
                var _courts = dbContext.Master_Court.Where(x => x.FK_VenueId == currentUser.CurrentVenueId && x.PK_CourtId == id && x.FK_StatusId == 1);
                if (_courts == null || _courts.Count() == 0)
                {
                    return HttpNotFound();
                }
                var _court = _courts.FirstOrDefault();
                if (_court == null)
                {
                    return HttpNotFound();
                }
                _court.FK_StatusId = 2;
                _court.ModifiedBy = currentUser.UserId;
                _court.ModifiedDate = DateTime.Now.ToLocalTime();
                dbContext.Entry(_court).State = EntityState.Modified;
                await dbContext.SaveChangesAsync();
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {

                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                dbContext.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
