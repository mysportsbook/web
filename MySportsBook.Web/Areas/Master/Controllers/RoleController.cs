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

namespace MySportsBook.Web.Areas.Master.Controllers
{
    public class RoleController : BaseController
    {
        // GET: Master/Role
        public async Task<ActionResult> Index()
        {
            var master_Role = dbContext.Master_Role.Include(m => m.Configuration_Status).Include(m => m.Configuration_User).Include(m => m.Configuration_User1).Include(m => m.Master_Venue);
            return View(await master_Role.ToListAsync());
        }

        // GET: Master/Role/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Master_Role master_Role = await dbContext.Master_Role.FindAsync(id);
            if (master_Role == null)
            {
                return HttpNotFound();
            }
            return View(master_Role);
        }

        // GET: Master/Role/Create
        public ActionResult Create()
        {
            ViewBag.FK_StatusId = new SelectList(dbContext.Configuration_Status, "PK_StatusId", "Status");
            ViewBag.CreatedBy = new SelectList(dbContext.Configuration_User, "PK_UserId", "UserName");
            ViewBag.ModifiedBy = new SelectList(dbContext.Configuration_User, "PK_UserId", "UserName");
            ViewBag.FK_VenueId = new SelectList(dbContext.Master_Venue, "PK_VenueId", "VenueName");
            return View();
        }

        // POST: Master/Role/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "PK_RoleId,FK_VenueId,RoleName,FK_StatusId,CreatedBy,CreatedDate,ModifiedBy,ModifiedDate")] Master_Role master_Role)
        {
            if (ModelState.IsValid)
            {
                dbContext.Master_Role.Add(master_Role);
                await dbContext.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.FK_StatusId = new SelectList(dbContext.Configuration_Status, "PK_StatusId", "Status", master_Role.FK_StatusId);
            ViewBag.CreatedBy = new SelectList(dbContext.Configuration_User, "PK_UserId", "UserName", master_Role.CreatedBy);
            ViewBag.ModifiedBy = new SelectList(dbContext.Configuration_User, "PK_UserId", "UserName", master_Role.ModifiedBy);
            ViewBag.FK_VenueId = new SelectList(dbContext.Master_Venue, "PK_VenueId", "VenueName", master_Role.FK_VenueId);
            return View(master_Role);
        }

        // GET: Master/Role/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Master_Role master_Role = await dbContext.Master_Role.FindAsync(id);
            if (master_Role == null)
            {
                return HttpNotFound();
            }
            ViewBag.FK_StatusId = new SelectList(dbContext.Configuration_Status, "PK_StatusId", "Status", master_Role.FK_StatusId);
            ViewBag.CreatedBy = new SelectList(dbContext.Configuration_User, "PK_UserId", "UserName", master_Role.CreatedBy);
            ViewBag.ModifiedBy = new SelectList(dbContext.Configuration_User, "PK_UserId", "UserName", master_Role.ModifiedBy);
            ViewBag.FK_VenueId = new SelectList(dbContext.Master_Venue, "PK_VenueId", "VenueName", master_Role.FK_VenueId);
            return View(master_Role);
        }

        // POST: Master/Role/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "PK_RoleId,FK_VenueId,RoleName,FK_StatusId,CreatedBy,CreatedDate,ModifiedBy,ModifiedDate")] Master_Role master_Role)
        {
            if (ModelState.IsValid)
            {
                dbContext.Entry(master_Role).State = EntityState.Modified;
                await dbContext.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.FK_StatusId = new SelectList(dbContext.Configuration_Status, "PK_StatusId", "Status", master_Role.FK_StatusId);
            ViewBag.CreatedBy = new SelectList(dbContext.Configuration_User, "PK_UserId", "UserName", master_Role.CreatedBy);
            ViewBag.ModifiedBy = new SelectList(dbContext.Configuration_User, "PK_UserId", "UserName", master_Role.ModifiedBy);
            ViewBag.FK_VenueId = new SelectList(dbContext.Master_Venue, "PK_VenueId", "VenueName", master_Role.FK_VenueId);
            return View(master_Role);
        }

        // GET: Master/Role/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Master_Role master_Role = await dbContext.Master_Role.FindAsync(id);
            if (master_Role == null)
            {
                return HttpNotFound();
            }
            return View(master_Role);
        }

        // POST: Master/Role/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Master_Role master_Role = await dbContext.Master_Role.FindAsync(id);
            dbContext.Master_Role.Remove(master_Role);
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
