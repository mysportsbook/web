using MySportsBook.Model;
using MySportsBook.Web.Controllers;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;


namespace MySportsBook.Web.Areas.Transaction.Controllers
{
    public class IncomeController : BaseController
    {

        // GET: Transaction/Income
        public async Task<ActionResult> Index()
        {
            return View(await dbContext.Transaction_Income.Include(x => x.Configuration_User).Include(x => x.Master_IncomeType).ToListAsync());
        }

        // GET: Transaction/Income/Create
        public ActionResult Create()
        {
            List<SelectListItem> ddlList = new List<SelectListItem>();
            ddlList.Add(new SelectListItem
            {
                Text = "--Select--",
                Value = ""
            });
            dbContext.Master_IncomeType.ToList().ForEach(x =>
            {
                ddlList.Add(new SelectListItem
                {
                    Text = x.IncomeDescription,
                    Value = x.PK_IncomeTypeId.ToString()
                });
            });
            ViewBag.IncomesType = ddlList;
            dbContext.Configuration_User.Where(x => dbContext.Master_UserVenue.Any(v => v.FK_UserId == x.PK_UserId && v.FK_VenueId == currentUser.CurrentVenueId) && x.PK_UserId != 0).ToList().ForEach(x =>
            {
                ddlList.Add(new SelectListItem
                {
                    Text = x.FirstName,
                    Value = x.PK_UserId.ToString()
                });
            });
            ViewBag.ReceivedUser = new SelectList(ddlList, "Value", "Text");

            return View();
        }

        // POST: Transaction/Income/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public ActionResult Create(Transaction_Income IncomeDetail)
        {
            try
            {
                IncomeDetail.FK_StatusId = 1;
                IncomeDetail.CreatedBy = currentUser.UserId;
                IncomeDetail.CreatedDate = DateTime.Now.ToLocalTime();

                dbContext.Transaction_Income.Add(IncomeDetail);
                dbContext.SaveChangesAsync().Wait();
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {

                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: Transaction/Income/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Transaction_Income IncomeDetail = await dbContext.Transaction_Income.FindAsync(id);
            if (IncomeDetail == null)
            {
                return HttpNotFound();
            }
            return View(IncomeDetail);
        }

        // POST: Transaction/Income/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "PK_IncomeId,ReceivedBy,Description,ReceivedDate")] Transaction_Income IncomeDetail)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    Transaction_Income _Expanses = await dbContext.Transaction_Income.Where(x => x.PK_IncomeId == IncomeDetail.PK_IncomeId).FirstOrDefaultAsync();
                    if (_Expanses == null)
                    {
                        return HttpNotFound();
                    }
                    _Expanses.ReceivedBy = IncomeDetail.ReceivedBy;
                    _Expanses.Description = IncomeDetail.Description;
                    _Expanses.ReceivedDate = IncomeDetail.ReceivedDate;
                    _Expanses.ModifiedBy = currentUser.UserId;
                    _Expanses.ModifiedDate = DateTime.Now;
                    dbContext.Entry(_Expanses).State = EntityState.Modified;
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


    }
}