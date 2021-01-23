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
    public class ExpenseController : BaseController
    {

        // GET: Studio/Expense
        public async Task<ActionResult> Index()
        {
            return View(await dbContext.Studio_ExpenseDetail.Include(u=>u.Configuration_User1).ToListAsync());
        }

        // GET: Studio/Expense/Create
        public ActionResult Create()
        {
            List<SelectListItem> ddlList = new List<SelectListItem>();
            ddlList.Add(new SelectListItem
            {
                Text = "--Select--",
                Value = ""
            });
            //dbContext.Studio_Event.Where(x => x.FK_StatusId == 1).ToList().ForEach(x =>
            //{
            //    ddlList.Add(new SelectListItem
            //    {
            //        Text = x.ExpenseType,
            //        Value = x.PK_ExpenseTypeId.ToString()
            //    });
            //});
            ViewBag.ExpensesType = ddlList;
            ddlList = new List<SelectListItem>();
            ddlList.Add(new SelectListItem
            {
                Text = "--Select--",
                Value = ""
            });
            dbContext.StudioEvents.Where(x => x.FK_StatusId == 3).ToList().ForEach(x =>
            {
                ddlList.Add(new SelectListItem
                {
                    Text = x.OrderNumber+" - "+x.CustomerName,
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
            dbContext.Configuration_User.Where(x => dbContext.Master_UserVenue.Any(v => v.FK_UserId == x.PK_UserId && v.FK_VenueId == currentUser.CurrentVenueId) && x.PK_UserId != 0).ToList().ForEach(x =>
            {
                ddlList.Add(new SelectListItem
                {
                    Text = x.FirstName,
                    Value = x.PK_UserId.ToString()
                });
            });
            ViewBag.StudioUser = new SelectList(ddlList, "Value", "Text");

            return View();
        }

        // POST: Studio/Expense/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public ActionResult Create(Studio_ExpenseDetail expenseDetail)
        {
                try
                {

                    expenseDetail.FK_StatusId = 1;
                    expenseDetail.CreatedBy = currentUser.UserId;
                    expenseDetail.CreatedDate = DateTime.Now.ToLocalTime();

                    dbContext.Studio_ExpenseDetail.Add(expenseDetail);
                    dbContext.SaveChangesAsync().Wait();
                    return Json(true, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {

                    return Json(false, JsonRequestBehavior.AllowGet);
                }
        }

        // GET: Studio/Expense/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Studio_ExpenseDetail expenseDetail = await dbContext.Studio_ExpenseDetail.FindAsync(id);
            if (expenseDetail == null)
            {
                return HttpNotFound();
            }


            List<SelectListItem>  ddlList = new List<SelectListItem>();
            ddlList.Add(new SelectListItem
            {
                Text = "--Select--",
                Value = ""
            });
            dbContext.Configuration_User.Where(x => dbContext.Master_UserVenue.Any(v => v.FK_UserId == x.PK_UserId && v.FK_VenueId == currentUser.CurrentVenueId) && x.PK_UserId != 0).ToList().ForEach(x =>
            {
                ddlList.Add(new SelectListItem
                {
                    Text = x.FirstName,
                    Value = x.PK_UserId.ToString()
                });
            });
            ViewBag.StudioUser = new SelectList(ddlList, "Value", "Text",expenseDetail.SpentBy);

            return View(expenseDetail);
        }

        // POST: Studio/Expense/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public async Task<ActionResult> Edit([Bind(Include = "PK_ExpenseDetailId,SpentBy,Description,SpentDate")] Studio_ExpenseDetail expenseDetail)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    Studio_ExpenseDetail _Expanses = await dbContext.Studio_ExpenseDetail.Where( x=>x.PK_ExpenseDetailId == expenseDetail.PK_ExpenseDetailId).FirstOrDefaultAsync();
                    if (_Expanses == null)
                    {
                        return HttpNotFound();
                    }
                    //_Expanses.FK_EventId = expenseDetail.FK_EventId;
                    //_Expanses.FK_ExpenseTypeId = expenseDetail.FK_ExpenseTypeId;
                    _Expanses.SpentBy = expenseDetail.SpentBy;
                    _Expanses.Description = expenseDetail.Description;
                    _Expanses.SpentDate = expenseDetail.SpentDate;
                    dbContext.Entry(_Expanses).State = EntityState.Modified;
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
