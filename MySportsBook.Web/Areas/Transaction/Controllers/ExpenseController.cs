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
    public class ExpenseController : BaseController
    {

        // GET: Transaction/Expense
        public async Task<ActionResult> Index()
        {
            return View(await dbContext.Transaction_Expense.Include(x => x.Configuration_User).Include(x => x.Master_ExpenseType).ToListAsync());
        }

        // GET: Transaction/Expense/Create
        public ActionResult Create()
        {
            List<SelectListItem> ddlList = new List<SelectListItem>();
            ddlList.Add(new SelectListItem
            {
                Text = "--Select--",
                Value = ""
            });
            dbContext.Master_ExpenseType.ToList().ForEach(x =>
            {
                ddlList.Add(new SelectListItem
                {
                    Text = x.ExpenseDescription,
                    Value = x.PK_ExpenseTypeId.ToString()
                });
            });
            ViewBag.ExpensesType = new SelectList(ddlList, "Value", "Text"); ;
            ddlList = new List<SelectListItem>();
            dbContext.Configuration_User.Where(x => dbContext.Master_UserVenue.Any(v => v.FK_UserId == x.PK_UserId && v.FK_VenueId == currentUser.CurrentVenueId) && x.PK_UserId != 0).ToList().ForEach(x =>
            {
                ddlList.Add(new SelectListItem
                {
                    Text = x.FirstName,
                    Value = x.PK_UserId.ToString()
                });
            });
            ViewBag.SpendUser = new SelectList(ddlList, "Value", "Text", currentUser.UserId);

            return View();
        }

        // POST: Transaction/Expense/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public ActionResult Create(Transaction_Expense expenseDetail)
        {
            try
            {
                expenseDetail.FK_StatusId = 1;
                expenseDetail.CreatedBy = currentUser.UserId;
                expenseDetail.CreatedDate = DateTime.Now.ToLocalTime();

                dbContext.Transaction_Expense.Add(expenseDetail);
                dbContext.SaveChangesAsync().Wait();
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {

                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: Transaction/Expense/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Transaction_Expense expenseDetail = await dbContext.Transaction_Expense.FindAsync(id);
            if (expenseDetail == null)
            {
                return HttpNotFound();
            }
            List<SelectListItem> ddlList = new List<SelectListItem>();
            ddlList.Add(new SelectListItem
            {
                Text = "--Select--",
                Value = ""
            });
            dbContext.Master_ExpenseType.ToList().ForEach(x =>
            {
                ddlList.Add(new SelectListItem
                {
                    Text = x.ExpenseDescription,
                    Value = x.PK_ExpenseTypeId.ToString()
                });
            });
            ViewBag.ExpensesType = new SelectList(ddlList, "Value", "Text", expenseDetail.FK_ExpenseTypeId);
            dbContext.Configuration_User.Where(x => dbContext.Master_UserVenue.Any(v => v.FK_UserId == x.PK_UserId && v.FK_VenueId == currentUser.CurrentVenueId) && x.PK_UserId != 0).ToList().ForEach(x =>
            {
                ddlList.Add(new SelectListItem
                {
                    Text = x.FirstName,
                    Value = x.PK_UserId.ToString()
                });
            });
            ViewBag.SpendUser = new SelectList(ddlList, "Value", "Text", expenseDetail.SpendBy);

            return View(expenseDetail);
        }

        // POST: Transaction/Expense/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "PK_ExpenseId,FK_ExpenseTypeId,SpendBy,SpendDate,Description,Amount")] Transaction_Expense expenseDetail)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    Transaction_Expense _expense = await dbContext.Transaction_Expense.Where(x => x.PK_ExpenseId == expenseDetail.PK_ExpenseId).FirstOrDefaultAsync();
                    if (_expense == null)
                    {
                        return HttpNotFound();
                    }
                    _expense.FK_ExpenseTypeId = expenseDetail.FK_ExpenseTypeId;
                    _expense.SpendBy = expenseDetail.SpendBy;
                    _expense.Description = expenseDetail.Description;
                    _expense.SpendDate = expenseDetail.SpendDate;
                    _expense.ModifiedBy = currentUser.UserId;
                    _expense.ModifiedDate = DateTime.Now;
                    dbContext.Entry(_expense).State = EntityState.Modified;
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
