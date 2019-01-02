using MySportsBook.Model;
using MySportsBook.Web.Controllers;
using MySportsBook.Web.Filters;
using System;
using System.Data.Entity;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace MySportsBook.Web.Areas.Configuration.Controllers
{
    [UserAuthentication]
    public class UserController : BaseController
    {
        private MySportsBookEntities db = new MySportsBookEntities();

        // GET: Configuration/User
        public async Task<ActionResult> Index()
        {
            var configuration_User = db.Configuration_User.Include(c => c.Configuration_Status);
            return View(await configuration_User.ToListAsync());
        }

        // GET: Configuration/User/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Configuration_User configuration_User = await db.Configuration_User.FindAsync(id);
            if (configuration_User == null)
            {
                return HttpNotFound();
            }
            return View(configuration_User);
        }

        // GET: Configuration/User/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Configuration/User/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "UserName,FirstName,LastName,Email,Mobile")] Configuration_User configuration_User)
        {
            if (ModelState.IsValid)
            {
                var pass = new Cryptography("winners@456");
                configuration_User.PasswordSalt = pass.Salt;
                configuration_User.PasswordHash = pass.Hash;
                configuration_User.FK_StatusId = 1;
                configuration_User.CreatedBy = currentUser.UserId;
                configuration_User.CreatedDate = DateTime.Now.ToLocalTime();
                db.Configuration_User.Add(configuration_User);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(configuration_User);
        }

        // GET: Configuration/User/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Configuration_User configuration_User = await db.Configuration_User.FindAsync(id);
            if (configuration_User == null)
            {
                return HttpNotFound();
            }
            configuration_User.PasswordSalt = string.Empty;
            configuration_User.PasswordHash = string.Empty;
            ViewBag.FK_StatusId = new SelectList(db.Configuration_Status, "PK_StatusId", "Status", configuration_User.FK_StatusId);
            return View(configuration_User);
        }

        // POST: Configuration/User/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "PK_UserId,UserName,FirstName,LastName,Email,Mobile,PasswordHash")] Configuration_User configuration_User)
        {
            if (ModelState.IsValid)
            {
                var _user = db.Configuration_User.Find(configuration_User.PK_UserId);
                if (_user != null)
                {
                    if (configuration_User.PasswordHash != string.Empty)
                    {
                        var pass = new Cryptography(configuration_User.PasswordHash);
                        _user.PasswordHash = pass.Hash;
                        _user.PasswordSalt = pass.Salt;
                        _user.ModifiedBy = currentUser.UserId;
                        _user.ModifiedDate = DateTime.Now.ToLocalTime();
                    }
                    db.Entry(_user).State = EntityState.Modified;
                    await db.SaveChangesAsync();
                    return RedirectToAction("Index");
                }

            }
            ViewBag.FK_StatusId = new SelectList(db.Configuration_Status, "PK_StatusId", "Status", configuration_User.FK_StatusId);
            return View(configuration_User);
        }

        // GET: Configuration/User/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Configuration_User configuration_User = await db.Configuration_User.FindAsync(id);
            if (configuration_User == null)
            {
                return HttpNotFound();
            }
            return View(configuration_User);
        }

        // POST: Configuration/User/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Configuration_User configuration_User = await db.Configuration_User.FindAsync(id);
            db.Configuration_User.Remove(configuration_User);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
