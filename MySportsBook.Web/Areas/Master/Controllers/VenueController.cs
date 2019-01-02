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
    public class VenueController : BaseController
    {
        [UserAuthentication]
        // GET: Master/Venue
        public async Task<ActionResult> Index()
        {
            var master_Venue = dbContext.Master_Venue.Where(x => x.FK_StatusId == 1).OrderByDescending(x => x.CreatedDate).Include(m => m.Configuration_Status);
            return View(await master_Venue.ToListAsync());
        }

        // GET: Master/Venue/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Master/Venue/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public async Task<ActionResult> Create([Bind(Include = "VenueName,VenueCode,Address,Email,Mobile,EmailEnabled,SMSEnabled,WhatsappEnabled,ShouldAdvInvGenerate")] Master_Venue master_Venue)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    master_Venue.FK_StatusId = 1;
                    master_Venue.CreatedBy = currentUser.UserId;
                    master_Venue.CreatedDate = DateTime.Now.ToLocalTime();
                    dbContext.Master_Venue.Add(master_Venue);
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

        // GET: Master/Venue/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var master_Venue = dbContext.Master_Venue.Where(x => x.FK_StatusId == 1 && x.PK_VenueId == id);
            if (master_Venue == null || master_Venue.Count() == 0)
            {
                return HttpNotFound();
            }
            return View(master_Venue.FirstOrDefault());
        }

        // POST: Master/Venue/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public async Task<ActionResult> Edit([Bind(Include = "PK_VenueId,VenueName,VenueCode,Address,Email,Mobile,EmailEnabled,SMSEnabled,WhatsappEnabled,ShouldAdvInvGenerate")] Master_Venue master_Venue)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var _Venues = dbContext.Master_Venue.Where(x => x.FK_StatusId == 1 && x.PK_VenueId == master_Venue.PK_VenueId);
                    if (_Venues == null || _Venues.Count() == 0)
                    {
                        return HttpNotFound();
                    }
                    Master_Venue _venue = await dbContext.Master_Venue.FindAsync(master_Venue.PK_VenueId);
                    _venue.VenueCode = master_Venue.VenueCode;
                    _venue.VenueName = master_Venue.VenueName;
                    _venue.Mobile = master_Venue.Mobile;
                    _venue.Email = master_Venue.Email;
                    _venue.Address = master_Venue.Address;
                    _venue.EmailEnabled = master_Venue.EmailEnabled;
                    _venue.SMSEnabled = master_Venue.SMSEnabled;
                    _venue.WhatsappEnabled = master_Venue.WhatsappEnabled;
                    _venue.ShouldAdvInvGenerate = master_Venue.ShouldAdvInvGenerate;
                    _venue.FK_StatusId = 1;
                    _venue.ModifiedBy = currentUser.UserId;
                    _venue.ModifiedDate = DateTime.Now.ToLocalTime();
                    dbContext.Entry(_venue).State = EntityState.Modified;
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

        // GET: Master/Venue/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var _Venues = dbContext.Master_Venue.Where(x => x.FK_StatusId == 1 && x.PK_VenueId == id);
            if (_Venues == null || _Venues.Count() == 0)
            {
                return HttpNotFound();
            }
            Master_Venue master_Venue = _Venues.FirstOrDefault();
            if (master_Venue == null)
            {
                return HttpNotFound();
            }
            else
            {
                master_Venue.FK_StatusId = 2;
                master_Venue.ModifiedBy = currentUser.UserId;
                master_Venue.ModifiedDate = DateTime.Now.ToLocalTime();
            }
            dbContext.Entry(master_Venue).State = EntityState.Modified;
            await dbContext.SaveChangesAsync();
            return Json(true, JsonRequestBehavior.AllowGet);
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
