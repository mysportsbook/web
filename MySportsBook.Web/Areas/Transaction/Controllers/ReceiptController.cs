using MySportsBook.Web.Controllers;
using MySportsBook.Web.Filters;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace MySportsBook.Web.Areas.Transaction.Controllers
{
    [UserAuthentication]
    public class ReceiptController : BaseController
    {
        // GET: Transaction/Invoice
        public async Task<ActionResult> Index()
        {
            var master_Receipt = dbContext.Transaction_Receipt.Include(m=>m.Confirguration_PaymentMode).Include(m => m.Transaction_Invoice).Include(m => m.Transaction_Invoice.Master_Player).Include(m => m.Configuration_Status).Where(x => x.FK_VenueId == currentUser.CurrentVenueId);
            return View(await master_Receipt.Where(x => x.FK_StatusId == 1).ToListAsync());
        }
    }
}