using MySportsBook.Model;
using MySportsBook.Web.Controllers;
using MySportsBook.Web.Filters;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace MySportsBook.Web.Areas.Master.Controllers
{
    [UserAuthentication]
    public class BatchController : BaseController
    {

        // GET: Master/Batch
        public ActionResult Index()
        {
            var master_Batch = dbContext.Master_Batch.Include(b => b.Master_Court.Master_Sport).Include(b => b.Master_Court).Include(b => b.Configuration_Status).Where(x => x.FK_VenueId == currentUser.CurrentVenueId && x.FK_StatusId == 1).OrderByDescending(x => x.CreatedDate);
            return View(master_Batch.ToList());
        }

        // GET: Master/Batch/Create
        public ActionResult Create()
        {
            ViewBag.BatchType = new SelectList(dbContext.Configuration_BatchType, "PK_BatchTypeId", "BatchType");

            List<SelectListItem> ddlList = new List<SelectListItem>();
            ddlList.Add(new SelectListItem
            {
                Text = "--Select--",
                Value = ""
            });
            dbContext.Master_Player.Where(x => x.FK_PlayerTypeId == 2 && x.FK_VenueId == currentUser.CurrentVenueId).ToList().ForEach(x =>
            {
                ddlList.Add(new SelectListItem
                {
                    Text = x.FirstName,
                    Value = x.PK_PlayerId.ToString()
                });
            });
            ViewBag.Coach = ddlList;
            ddlList = new List<SelectListItem>();
            ddlList.Add(new SelectListItem
            {
                Text = "--Select--",
                Value = ""
            });
            dbContext.Master_Sport.Where(s => s.FK_VenueId == currentUser.CurrentVenueId).ToList().ForEach(x =>
            {
                ddlList.Add(new SelectListItem
                {
                    Text = x.SportName,
                    Value = x.PK_SportId.ToString()
                });
            });
            ViewBag.Sport = ddlList;
            ViewBag.Court = new SelectList((new SelectListItem[] { new SelectListItem { Text = "--Select--", Value = string.Empty } }).ToList(), "Value", "Text");
            return View();
        }

        // POST: Master/Batch/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public ActionResult Create([Bind(Include = "BatchName,BatchCode,FK_CourtId,StartTime,EndTime,Fee,MaxPlayers,FK_BatchTypeId,StartDate,EndDate,Monday,Tuesday,Wednesday,Thursday,Friday,Saturday,Sunday")] Master_Batch master_Batch)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (dbContext.Master_Batch.Any(x => x.BatchName == master_Batch.BatchName && x.FK_StatusId == 1 && x.FK_VenueId == currentUser.CurrentVenueId))
                    {
                        return Json(false, JsonRequestBehavior.AllowGet);
                    }

                    if (dbContext.Master_Batch.Any(x => x.BatchCode == master_Batch.BatchCode && x.FK_StatusId == 1 && x.FK_VenueId == currentUser.CurrentVenueId))
                    {
                        return Json(false, JsonRequestBehavior.AllowGet);
                    }

                    master_Batch.FK_StatusId = 1;
                    master_Batch.FK_VenueId = currentUser.CurrentVenueId;
                    master_Batch.CreatedBy = currentUser.UserId;
                    master_Batch.CreatedDate = DateTime.Now.ToUniversalTime();
                    dbContext.Master_Batch.Add(master_Batch);
                    dbContext.SaveChanges();
                    return Json(true, JsonRequestBehavior.AllowGet);
                }
                return Json(false, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: Master/Batch/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Master_Batch master_Batch = dbContext.Master_Batch.ToList().Find(x => x.FK_VenueId == currentUser.CurrentVenueId && x.PK_BatchId == id && x.FK_StatusId == 1);
            if (master_Batch == null)
            {
                return HttpNotFound();
            }
            ViewBag.BatchType = new SelectList(dbContext.Configuration_BatchType, "PK_BatchTypeId", "BatchType", master_Batch.FK_BatchTypeId);

            List<SelectListItem> ddlList = new List<SelectListItem>();
            ddlList.Add(new SelectListItem
            {
                Text = "--Select--",
                Value = ""
            });
            dbContext.Master_Player.Where(x => x.FK_PlayerTypeId == 2 && x.FK_VenueId == currentUser.CurrentVenueId && x.FK_StatusId == 1).ToList().ForEach(x =>
              {
                  ddlList.Add(new SelectListItem
                  {
                      Text = x.FirstName,
                      Value = x.PK_PlayerId.ToString()
                  });
              });
            ViewBag.Coach = new SelectList(ddlList, "Value", "Text", master_Batch.FK_CoachId);
            ddlList = new List<SelectListItem>();
            ddlList.Add(new SelectListItem
            {
                Text = "--Select--",
                Value = ""
            });
            dbContext.Master_Sport.Where(s => s.FK_VenueId == currentUser.CurrentVenueId && s.FK_StatusId == 1).ToList().ForEach(x =>
            {
                ddlList.Add(new SelectListItem
                {
                    Text = x.SportName,
                    Value = x.PK_SportId.ToString()
                });
            });
            var _court = dbContext.Master_Court.Where(x => x.FK_VenueId == currentUser.CurrentVenueId && x.FK_StatusId == 1 && x.PK_CourtId == master_Batch.FK_CourtId);
            ViewBag.Sport = (_court != null && _court.Count() > 0) ? new SelectList(ddlList, "Value", "Text", _court.FirstOrDefault().FK_SportId) : new SelectList(ddlList, "Value", "Text");
            if (_court != null && _court.Count() > 0)
            {
                ddlList = new List<SelectListItem>();
                dbContext.Master_Court.Where(x => x.FK_VenueId == currentUser.CurrentVenueId && x.FK_StatusId == 1 && x.FK_SportId == _court.FirstOrDefault().FK_SportId).ToList().ForEach(x =>
                {
                    ddlList.Add(new SelectListItem
                    {
                        Text = x.CourtName,
                        Value = x.PK_CourtId.ToString()
                    });
                });
                ViewBag.Court = new SelectList(ddlList, "Value", "Text", master_Batch.FK_CourtId);
            }
            else
            {
                ViewBag.Court = new SelectList((new SelectListItem[] { new SelectListItem { Text = "--Select--", Value = string.Empty } }).ToList(), "Value", "Text");
            }
            return View(master_Batch);
        }

        // POST: Master/Batch/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public ActionResult Edit([Bind(Include = "PK_BatchId,BatchName,BatchCode,FK_CourtId,StartTime,EndTime,Fee,MaxPlayers,FK_BatchTypeId,StartDate,EndDate,Monday,Tuesday,Wednesday,Thursday,Friday,Saturday,Sunday,FK_StatusId,CreatedBy,CreatedDate,ModifiedBy,ModifiedDate")] Master_Batch master_Batch)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    Master_Batch _batch = dbContext.Master_Batch.ToList().Find(x => x.FK_VenueId == currentUser.CurrentVenueId && x.PK_BatchId == master_Batch.PK_BatchId && x.FK_StatusId == 1);
                    if (_batch == null)
                        return HttpNotFound();
                    if (dbContext.Master_Batch.Any(x => x.BatchName == master_Batch.BatchName && x.FK_StatusId == 1 && x.FK_VenueId == currentUser.CurrentVenueId && x.PK_BatchId != _batch.PK_BatchId))
                        return Json(false, JsonRequestBehavior.AllowGet);
                    if (dbContext.Master_Batch.Any(x => x.BatchCode == master_Batch.BatchCode && x.FK_StatusId == 1 && x.FK_VenueId == currentUser.CurrentVenueId && x.PK_BatchId != _batch.PK_BatchId))
                        return Json(false, JsonRequestBehavior.AllowGet);
                    _batch.BatchCode = master_Batch.BatchCode;
                    _batch.BatchName = master_Batch.BatchName;
                    _batch.FK_CourtId = master_Batch.FK_CourtId;
                    _batch.StartTime = master_Batch.StartTime;
                    _batch.EndTime = master_Batch.EndTime;
                    _batch.StartDate = master_Batch.StartDate;
                    _batch.EndDate = master_Batch.EndDate;
                    _batch.MaxPlayers = master_Batch.MaxPlayers;
                    _batch.Fee = master_Batch.Fee;
                    _batch.FK_BatchTypeId = master_Batch.FK_BatchTypeId;
                    _batch.FK_CoachId = master_Batch.FK_CoachId;
                    _batch.Sunday = master_Batch.Sunday;
                    _batch.Monday = master_Batch.Monday;
                    _batch.Tuesday = master_Batch.Tuesday;
                    _batch.Wednesday = master_Batch.Wednesday;
                    _batch.Thursday = master_Batch.Thursday;
                    _batch.Friday = master_Batch.Friday;
                    _batch.Saturday = master_Batch.Saturday;
                    _batch.FK_StatusId = 1;
                    _batch.ModifiedBy = currentUser.UserId;
                    _batch.ModifiedDate = DateTime.Now.ToUniversalTime();
                    //_batch. = master_Batch.BatchCode;
                    //_batch.BatchCode = master_Batch.BatchCode;
                    dbContext.Entry(_batch).State = EntityState.Modified;
                    dbContext.SaveChanges();
                    return Json(true, JsonRequestBehavior.AllowGet);
                }
                return Json(false, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: Master/Batch/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            try
            {
                var _batchs = dbContext.Master_Batch.Where(x => x.FK_VenueId == currentUser.CurrentVenueId && x.PK_BatchId == id);
                if (_batchs == null || _batchs.Count() == 0)
                {
                    return HttpNotFound();
                }
                var _batch = _batchs.FirstOrDefault();
                if (_batch == null)
                {
                    return HttpNotFound();
                }
                _batch.FK_StatusId = 2;
                _batch.ModifiedBy = currentUser.UserId;
                _batch.ModifiedDate = DateTime.Now.ToUniversalTime();
                dbContext.Entry(_batch).State = EntityState.Modified;
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