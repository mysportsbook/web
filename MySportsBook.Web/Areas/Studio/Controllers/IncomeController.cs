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
    public class IncomeController : BaseController
    {

        // GET: Studio/Income
        public async Task<ActionResult> Index()
        {
            return View();//await dbContext.Studio_IncomeDetail.ToListAsync());
        }

        // GET: Studio/Income/Create
        public ActionResult Create()
        {
            List<SelectListItem> ddlList = new List<SelectListItem>();
            ddlList.Add(new SelectListItem
            {
                Text = "--Select--",
                Value = ""
            });

            dbContext.StudioEvents.Where(x => x.FK_StatusId == 3).ToList().ForEach(x =>
            {
                ddlList.Add(new SelectListItem
                {
                    Text = x.OrderNumber + " - " + x.CustomerName,
                    Value = x.PK_EventId.ToString()
                });
            });
            ViewBag.Events = new SelectList(ddlList, "Value", "Text");

            ddlList = new List<SelectListItem>();
            ddlList.Add(new SelectListItem
            {
                Text = "--Select--",
                Value = ""
            });
            //dbContext.Configuration_StudioUser.Where(x => x.FK_StatusId == 1).ToList().ForEach(x =>
            //{
            //    ddlList.Add(new SelectListItem
            //    {
            //        Text = x.Name,
            //        Value = x.PK_StudioUserId.ToString()
            //    });
            //});
            ViewBag.StudioUser = new SelectList(ddlList, "Value", "Text");

            return View();
        }

        // POST: Studio/Income/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        //public ActionResult Create(StudioIncomeDetail incomeDetail)
        //{
        //    try
        //    {
        //        incomeDetail.FK_StatusId = 1;
        //        incomeDetail.CreatedBy = currentUser.UserId;
        //        incomeDetail.CreatedDate = DateTime.Now.ToLocalTime();

        //        //dbContext.Studio_IncomeDetail.Add(incomeDetail);
        //        dbContext.SaveChangesAsync().Wait();
        //        return Json(true, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {

        //        return Json(false, JsonRequestBehavior.AllowGet);
        //    }
        //}

        // GET: Studio/Income/Edit/5
        //public async Task<ActionResult> Edit(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    //Studio_IncomeDetail incomeDetail = await dbContext.Studio_IncomeDetail.FindAsync(id);
        //    //if (incomeDetail == null)
        //    //{
        //    //    return HttpNotFound();
        //    //}
        //    return View(incomeDetail);
        //}

        // POST: Studio/Income/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "IncomeDetailId,ReceivedDate,Description,Amount,ReceivedBy,CreatedBy,CreatedDate,Status")] StudioEvent incomeDetail)
        {
            if (ModelState.IsValid)
            {
                dbContext.Entry(incomeDetail).State = EntityState.Modified;
                await dbContext.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(incomeDetail);
        }

    }
}
