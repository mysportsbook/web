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

namespace MySportsBook.Web.Areas.Studio.Controllers
{
    [UserAuthentication]
    public class EventsController : BaseController
    {
      

        // GET: Studio/Events
        public async Task<ActionResult> Index()
        {
            return View(await dbContext.Studio_Event.ToListAsync());
        }

        

        // GET: Studio/Events/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Studio/Events/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public ActionResult Create(Studio_Event studioEvent)
        {
            
                try
                {

                    studioEvent.FK_StatusId = 3;
                    studioEvent.CreatedBy = currentUser.UserId;
                    studioEvent.CreatedDate = DateTime.Now.ToUniversalTime();
                string OrderNumber = DateTime.Now.ToString("MMM");
                var LastOrderNumber = dbContext.Studio_Event.Where(x => OrderNumber.Contains(x.OrderNumber)).OrderByDescending(x=>x.PK_EventId).Select(x => x.OrderNumber).FirstOrDefault();
                if (LastOrderNumber == null)
                {
                    studioEvent.OrderNumber = OrderNumber + "000001";
                }
                else
                {
                    studioEvent.OrderNumber = String.Format("{0:000000}", LastOrderNumber);
                }
                dbContext.Studio_Event.Add(studioEvent);
                    dbContext.SaveChangesAsync().Wait();
                    return Json(true, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {

                    return Json(false, JsonRequestBehavior.AllowGet);
                }
           
        }

        // GET: Studio/Events/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Studio_Event studioEvent = await dbContext.Studio_Event.FindAsync(id);
            
            if (studioEvent == null)
            {
                return HttpNotFound();
            }
            return View(studioEvent);
        }

        // POST: Studio/Events/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "PK_EventId,OrderNumber,CustomerName,Mobile,EmailId,Description,EventDate,Venue,Remarks,Amount")] Studio_Event studioEvent)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    Studio_Event _Events = await dbContext.Studio_Event.Where(x =>  x.PK_EventId == studioEvent.PK_EventId).FirstOrDefaultAsync();
                    if (_Events == null)
                    {
                        return HttpNotFound();
                    }
                    _Events.CustomerName = studioEvent.CustomerName;
                    _Events.Mobile = studioEvent.Mobile;
                    _Events.EmailId = studioEvent.EmailId;
                    _Events.Description = studioEvent.Description;
                    _Events.EventDate = studioEvent.EventDate;
                    _Events.Venue = studioEvent.Venue;
                    _Events.Remarks = studioEvent.Remarks;
                    _Events.Amount = studioEvent.Amount;
                    _Events.ModifiedBy = currentUser.UserId;
                    _Events.ModifiedDate = DateTime.Now.ToUniversalTime();
                    dbContext.Entry(_Events).State = EntityState.Modified;
                    await dbContext.SaveChangesAsync();
                    return Json(true, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {

                    return Json(false, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(false, JsonRequestBehavior.AllowGet);
        }

    }
}
