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

namespace MySportsBook.Web.Areas.Studio.Controllers
{
    [UserAuthentication]
    public class EventsController : BaseController
    {


        // GET: Studio/Events
        public async Task<ActionResult> Index()
        {
            return View(await dbContext.StudioEvents.ToListAsync());
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
        public ActionResult Create(StudioEvent studioEvent)
        {

            try
            {

                studioEvent.FK_StatusId = 3;
                studioEvent.CreatedBy = currentUser.UserId;
                studioEvent.CreatedDate = DateTime.Now.ToLocalTime();
                string OrderNumber = DateTime.Now.ToString("yyyyMMM").ToUpper();
                var LastOrderNumber = dbContext.StudioEvents.Where(x => x.OrderNumber.ToUpper().Contains(OrderNumber)).OrderByDescending(x => x.PK_EventId).Select(x => x.OrderNumber).Take(1).FirstOrDefault();

                if (LastOrderNumber == null)
                {
                    int LastId = dbContext.StudioEvents.OrderByDescending(x => x.PK_EventId).Select(x => x.PK_EventId).Take(1).FirstOrDefault();
                    studioEvent.OrderNumber = OrderNumber + String.Format("{0:000}", Increasecount(LastId == 0 ? "0" : LastId.ToString())) + "001";
                }
                else
                {
                    studioEvent.OrderNumber = OrderNumber + String.Format("{0:000}", Increasecount(LastOrderNumber.Replace(OrderNumber, "").Substring(0, 3))) + "" + String.Format("{0:000}", Increasecount(LastOrderNumber.Replace(OrderNumber, "").Substring(3)));
                }
                dbContext.StudioEvents.Add(studioEvent);
                dbContext.SaveChangesAsync().Wait();
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {

                return Json(false, JsonRequestBehavior.AllowGet);
            }

        }

        private int Increasecount(string numbers)
        {
            return (Convert.ToInt32(numbers) + 1);

        }

        // GET: Studio/Events/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            StudioEvent studioEvent = await dbContext.StudioEvents.FindAsync(id);

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
        public async Task<ActionResult> Edit(StudioEvent studioEvent)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    StudioEvent _Events = await dbContext.StudioEvents.Where(x => x.PK_EventId == studioEvent.PK_EventId).FirstOrDefaultAsync();
                    if (_Events == null)
                    {
                        return HttpNotFound();
                    }
                    _Events.CustomerName = studioEvent.CustomerName;
                    _Events.Mobile = studioEvent.Mobile;
                    _Events.EmailId = studioEvent.EmailId;
                    _Events.Description = studioEvent.Description;
                    //_Events.EventDate = studioEvent.EventDate;
                    _Events.Venue = studioEvent.Venue;
                    _Events.Remarks = studioEvent.Remarks;
                    _Events.Amount = studioEvent.Amount;
                    _Events.ModifiedBy = currentUser.UserId;
                    _Events.ModifiedDate = DateTime.Now.ToLocalTime();
                    dbContext.Entry(_Events).State = EntityState.Modified;
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
        [HttpPost]
        public ActionResult Index(StudioEvent events)
        {
            try
            {
                var _Events = dbContext.StudioEvents.Find(events.PK_EventId);
                if (_Events == null)
                {
                    return HttpNotFound();
                }

                _Events.FK_StatusId = _Events.FK_StatusId == 3 ? 4 : 3;
                _Events.ModifiedBy = currentUser.UserId;
                dbContext.Entry(_Events).State = EntityState.Modified;
                dbContext.SaveChanges();
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }

    }
}
