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
    public class CoachingLevelController : BaseController
    {

        // GET: Master/CoachingLevel
        public async Task<ActionResult> Index()
        {
            var master_CoachingLevel = dbContext.Master_CoachingLevel.Include(m => m.Configuration_Status).Include(m => m.Configuration_User).Include(m => m.Configuration_User1).Include(m => m.Master_Venue);
            return View(await master_CoachingLevel.ToListAsync());
        }

        // GET: Master/CoachingLevel/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Master_CoachingLevel master_CoachingLevel = await dbContext.Master_CoachingLevel.FindAsync(id);
            if (master_CoachingLevel == null)
            {
                return HttpNotFound();
            }
            return View(master_CoachingLevel);
        }

        // GET: Master/CoachingLevel/Create
        public ActionResult Create()
        {
            ViewBag.FK_StatusId = new SelectList(dbContext.Configuration_Status, "PK_StatusId", "Status");
            ViewBag.CreatedBy = new SelectList(dbContext.Configuration_User, "PK_UserId", "UserName");
            ViewBag.ModifiedBy = new SelectList(dbContext.Configuration_User, "PK_UserId", "UserName");
            ViewBag.FK_VenueId = new SelectList(dbContext.Master_Venue, "PK_VenueId", "VenueName");
            return View();
        }

        // POST: Master/CoachingLevel/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "PK_CoachingLevelId,FK_VenueId,LevelName,FK_StatusId,CreatedBy,CreatedDate,ModifiedBy,ModifiedDate")] Master_CoachingLevel master_CoachingLevel)
        {
            if (ModelState.IsValid)
            {
                dbContext.Master_CoachingLevel.Add(master_CoachingLevel);
                await dbContext.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.FK_StatusId = new SelectList(dbContext.Configuration_Status, "PK_StatusId", "Status", master_CoachingLevel.FK_StatusId);
            ViewBag.CreatedBy = new SelectList(dbContext.Configuration_User, "PK_UserId", "UserName", master_CoachingLevel.CreatedBy);
            ViewBag.ModifiedBy = new SelectList(dbContext.Configuration_User, "PK_UserId", "UserName", master_CoachingLevel.ModifiedBy);
            ViewBag.FK_VenueId = new SelectList(dbContext.Master_Venue, "PK_VenueId", "VenueName", master_CoachingLevel.FK_VenueId);
            return View(master_CoachingLevel);
        }

        // GET: Master/CoachingLevel/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Master_CoachingLevel master_CoachingLevel = await dbContext.Master_CoachingLevel.FindAsync(id);
            if (master_CoachingLevel == null)
            {
                return HttpNotFound();
            }
            ViewBag.FK_StatusId = new SelectList(dbContext.Configuration_Status, "PK_StatusId", "Status", master_CoachingLevel.FK_StatusId);
            ViewBag.CreatedBy = new SelectList(dbContext.Configuration_User, "PK_UserId", "UserName", master_CoachingLevel.CreatedBy);
            ViewBag.ModifiedBy = new SelectList(dbContext.Configuration_User, "PK_UserId", "UserName", master_CoachingLevel.ModifiedBy);
            ViewBag.FK_VenueId = new SelectList(dbContext.Master_Venue, "PK_VenueId", "VenueName", master_CoachingLevel.FK_VenueId);
            return View(master_CoachingLevel);
        }

        // POST: Master/CoachingLevel/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "PK_CoachingLevelId,FK_VenueId,LevelName,FK_StatusId,CreatedBy,CreatedDate,ModifiedBy,ModifiedDate")] Master_CoachingLevel master_CoachingLevel)
        {
            if (ModelState.IsValid)
            {
                dbContext.Entry(master_CoachingLevel).State = EntityState.Modified;
                await dbContext.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.FK_StatusId = new SelectList(dbContext.Configuration_Status, "PK_StatusId", "Status", master_CoachingLevel.FK_StatusId);
            ViewBag.CreatedBy = new SelectList(dbContext.Configuration_User, "PK_UserId", "UserName", master_CoachingLevel.CreatedBy);
            ViewBag.ModifiedBy = new SelectList(dbContext.Configuration_User, "PK_UserId", "UserName", master_CoachingLevel.ModifiedBy);
            ViewBag.FK_VenueId = new SelectList(dbContext.Master_Venue, "PK_VenueId", "VenueName", master_CoachingLevel.FK_VenueId);
            return View(master_CoachingLevel);
        }

        // GET: Master/CoachingLevel/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Master_CoachingLevel master_CoachingLevel = await dbContext.Master_CoachingLevel.FindAsync(id);
            if (master_CoachingLevel == null)
            {
                return HttpNotFound();
            }
            return View(master_CoachingLevel);
        }

        // POST: Master/CoachingLevel/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Master_CoachingLevel master_CoachingLevel = await dbContext.Master_CoachingLevel.FindAsync(id);
            dbContext.Master_CoachingLevel.Remove(master_CoachingLevel);
            await dbContext.SaveChangesAsync();
            return RedirectToAction("Index");
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
