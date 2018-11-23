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
using MySportsBook.Web.Filters;

namespace MySportsBook.Web.Areas.Master.Controllers
{
    [UserAuthentication]
    public class SportController : BaseController
    {
        // GET: Master/Sport
        public async Task<ActionResult> Index()
        {
            var master_Sport = dbContext.Master_Sport.Include(m => m.Configuration_Status).Where(x => x.FK_VenueId == currentUser.CurrentVenueId && x.FK_StatusId == 1).OrderByDescending(x => x.CreatedDate);
            return View(await master_Sport.ToListAsync());
        }


        // GET: Master/Sport/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Master/Sport/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public async Task<ActionResult> Create([Bind(Include = "SportName,SportCode,FK_VenueId,Fee,AllowCoaching")] Master_Sport master_Sport)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (dbContext.Master_Sport.Any(x => x.SportName == master_Sport.SportName && x.FK_StatusId == 1 && x.FK_VenueId == currentUser.CurrentVenueId))
                        return Json(false, JsonRequestBehavior.AllowGet);
                    if (dbContext.Master_Sport.Any(x => x.SportCode == master_Sport.SportCode && x.FK_StatusId == 1 && x.FK_VenueId == currentUser.CurrentVenueId))
                        return Json(false, JsonRequestBehavior.AllowGet);
                    master_Sport.FK_StatusId = 1;
                    master_Sport.FK_VenueId = currentUser.CurrentVenueId;
                    master_Sport.CreatedBy = currentUser.UserId;
                    master_Sport.CreatedDate = DateTime.Now.ToUniversalTime();
                    dbContext.Master_Sport.Add(master_Sport);
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

        // GET: Master/Sport/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var master_Sport = dbContext.Master_Sport.Where(x => x.FK_VenueId == currentUser.CurrentVenueId && x.PK_SportId == id && x.FK_StatusId == 1);
            if (master_Sport == null || master_Sport.Count() == 0)
            {
                return HttpNotFound();
            }
            return View(master_Sport.FirstOrDefault());
        }

        // POST: Master/Sport/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public async Task<ActionResult> Edit([Bind(Include = "PK_SportId,SportName,SportCode,FK_VenueId,Fee,AllowCoaching")] Master_Sport master_Sport)
        {
            if (ModelState.IsValid)
            {
                var _Sports = dbContext.Master_Sport.Where(x => x.FK_VenueId == currentUser.CurrentVenueId && x.PK_SportId == master_Sport.PK_SportId && x.FK_StatusId == 1);
                if (_Sports == null || _Sports.Count() == 0)
                {
                    return HttpNotFound();
                }
                if (dbContext.Master_Sport.Any(x => x.SportName == master_Sport.SportName && x.FK_StatusId == 1 && x.FK_VenueId == currentUser.CurrentVenueId && x.PK_SportId != master_Sport.PK_SportId)
                    || dbContext.Master_Sport.Any(x => x.SportCode == master_Sport.SportCode && x.FK_StatusId == 1 && x.FK_VenueId == currentUser.CurrentVenueId && x.PK_SportId != master_Sport.PK_SportId))
                    return Json(false, JsonRequestBehavior.AllowGet);
                var _sport = _Sports.FirstOrDefault();
                if (_sport == null)
                    return Json(false, JsonRequestBehavior.AllowGet);
                _sport.SportCode = master_Sport.SportCode;
                _sport.SportName = master_Sport.SportName;
                _sport.AllowCoaching = master_Sport.AllowCoaching;
                _sport.Fee = master_Sport.Fee;
                _sport.FK_StatusId = 1;
                _sport.ModifiedBy = currentUser.UserId;
                _sport.ModifiedDate = DateTime.Now.ToUniversalTime();
                try
                {
                    dbContext.Entry(_sport).State = EntityState.Modified;
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

        // GET: Master/Sport/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var _sport = dbContext.Master_Sport.ToList().Find(x => x.FK_VenueId == currentUser.CurrentVenueId && x.PK_SportId == id && x.FK_StatusId == 1);
            if (_sport == null)
            {
                return HttpNotFound();
            }
            _sport.FK_StatusId = 2;
            _sport.ModifiedBy = currentUser.UserId;
            _sport.ModifiedDate = DateTime.Now.ToUniversalTime();
            try
            {
                dbContext.Entry(_sport).State = EntityState.Modified;
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
