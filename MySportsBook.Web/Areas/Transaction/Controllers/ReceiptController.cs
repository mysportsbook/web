using MySportsBook.Model;
using MySportsBook.Model.ViewModel;
using MySportsBook.Web.Controllers;
using MySportsBook.Web.Filters;
using System.Collections.Generic;
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
            var master_Receipt = dbContext.Transaction_Receipt.Include(m => m.Confirguration_PaymentMode).Include(m => m.Transaction_Invoice).Include(m => m.Transaction_Invoice.Master_Player).Include(m => m.Configuration_Status).Where(x => x.FK_VenueId == currentUser.CurrentVenueId);
            return View(await master_Receipt.Where(x => x.FK_StatusId == 1).ToListAsync());
        }

        public ActionResult Edit(int? id)
        {
            //var master_Receipt = dbContext.Transaction_Receipt.Include(m => m.Confirguration_PaymentMode).Include(m => m.Transaction_Invoice.Transaction_InvoiceDetail).Include(m => m.Transaction_Invoice.Master_Player).Include(m => m.Configuration_Status).Where(x => x.FK_VenueId == currentUser.CurrentVenueId && x.Transaction_Invoice.FK_PlayerId == id).OrderByDescending(s => s.CreatedDate).Take(2);

            var _transactionInvoices = dbContext.Transaction_Invoice.Include(x => x.Transaction_InvoiceDetail).Where(x => x.FK_VenueId == currentUser.CurrentVenueId && x.FK_PlayerId == id).OrderByDescending(s => s.CreatedDate).Take(2);
            if (_transactionInvoices != null)
            {
                var master_Receipt = new Transaction_Receipt();
                List<InvoiceDetailModel> obj = new List<InvoiceDetailModel>();
                if (_transactionInvoices.All(x => dbContext.Transaction_Receipt.Any(r => r.FK_InvoiceId == x.PK_InvoiceId)))
                {
                    var _transactionInvoice = _transactionInvoices.FirstOrDefault();
                    if (_transactionInvoice != null)
                    {
                        _transactionInvoice.Transaction_InvoiceDetail.FirstOrDefault().BatchAmount = (decimal)(_transactionInvoice.Transaction_InvoiceDetail.FirstOrDefault().BatchAmount + _transactionInvoice.TotalDiscount - _transactionInvoice.OtherAmount);
                        _transactionInvoice.Transaction_InvoiceDetail.FirstOrDefault().PaidAmount = (decimal)(_transactionInvoice.Transaction_InvoiceDetail.FirstOrDefault().PaidAmount + _transactionInvoice.TotalDiscount - _transactionInvoice.OtherAmount);
                        master_Receipt = dbContext.Transaction_Receipt.Include(m => m.Confirguration_PaymentMode).Include(m => m.Transaction_Invoice.Transaction_InvoiceDetail).Include(m => m.Transaction_Invoice.Master_Player).Include(m => m.Configuration_Status).Where(x => x.FK_VenueId == currentUser.CurrentVenueId && x.Transaction_Invoice.FK_PlayerId == id && _transactionInvoice.PK_InvoiceId == x.FK_InvoiceId && x.FK_StatusId == 1).ToList().FirstOrDefault();
                    }
                    else
                        return Json(false, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var _transactionInvoiceReceipt = _transactionInvoices.FirstOrDefault();
                    var _transactionInvoiceClose = _transactionInvoices.ToList().LastOrDefault();
                    _transactionInvoiceReceipt.Transaction_InvoiceDetail.FirstOrDefault().BatchAmount = (decimal)(_transactionInvoiceReceipt.Transaction_InvoiceDetail.FirstOrDefault().BatchAmount + _transactionInvoiceReceipt.TotalDiscount - _transactionInvoiceReceipt.OtherAmount);
                    _transactionInvoiceReceipt.Transaction_InvoiceDetail.FirstOrDefault().PaidAmount = (decimal)(_transactionInvoiceReceipt.Transaction_InvoiceDetail.FirstOrDefault().PaidAmount + _transactionInvoiceReceipt.TotalDiscount - _transactionInvoiceReceipt.OtherAmount);

                    master_Receipt = dbContext.Transaction_Receipt.Include(m => m.Confirguration_PaymentMode).Include(m => m.Transaction_Invoice.Transaction_InvoiceDetail).Include(m => m.Transaction_Invoice.Master_Player).Include(m => m.Configuration_Status).Where(x => x.FK_VenueId == currentUser.CurrentVenueId && x.Transaction_Invoice.FK_PlayerId == id && _transactionInvoiceReceipt.PK_InvoiceId == x.FK_InvoiceId && x.FK_StatusId == 1).ToList().FirstOrDefault();
                    if (master_Receipt != null)
                    {
                        if (_transactionInvoices.Count() > 1)
                        {

                            _transactionInvoiceClose.Transaction_InvoiceDetail.ToList().ForEach(x =>
                            {
                                //master_Receipt.Transaction_Invoice.Transaction_InvoiceDetail.Add(x);
                                obj.Add(new InvoiceDetailModel()
                                {
                                    InvoicePeriod = x.InvoicePeriod,
                                    Fee = (double)x.BatchAmount,
                                    PaidAmount = (double)x.PaidAmount

                                });
                            });


                        }

                    }
                }
                //Reordering the Display List
                if (master_Receipt != null)
                {
                    master_Receipt.Transaction_Invoice.Transaction_InvoiceDetail.ToList().ForEach(x =>
                    {
                        obj.Add(new InvoiceDetailModel()
                        {
                            InvoicePeriod = x.InvoicePeriod,
                            Fee = (double)x.BatchAmount,
                            PaidAmount = (double)x.PaidAmount
                        });
                    });
                    master_Receipt.Transaction_Invoice.Transaction_InvoiceDetail.Clear();
                    obj.OrderByDescending(x => x.InvoicePeriodDate).ToList().ForEach(x =>
                    {
                        master_Receipt.Transaction_Invoice.Transaction_InvoiceDetail.Add(new Transaction_InvoiceDetail() { InvoicePeriod = x.InvoicePeriod, BatchAmount = (decimal)x.Fee, PaidAmount = (decimal)x.PaidAmount });
                    });
                }
                ViewBag.PaymentMode = new SelectList(dbContext.Confirguration_PaymentMode, "PK_PaymentModeId", "PaymentMode", master_Receipt.FK_PaymentModeId);
                ViewBag.ReceivedBy = new SelectList(dbContext.Configuration_User.Where(x => dbContext.Master_UserVenue.Any(v => v.FK_UserId == x.PK_UserId && v.FK_VenueId == currentUser.CurrentVenueId) && x.PK_UserId != 0), "PK_UserId", "UserName", master_Receipt.ReceivedBy);
                return View(master_Receipt);
            }
            return View();
        }

        [HttpPost]
        public ActionResult Edit(InvoiceModel invoiceModel)
        {
            try
            {
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch (System.Exception)
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }
        // GET: Master/Player
        public async Task<ActionResult> Player()
        {
            var master_Player = dbContext.Master_Player
                .Include(m => m.Transaction_PlayerSport.Select(q => q.Master_Sport))
                .Include(m => m.Transaction_PlayerSport.Select(q => q.Master_Batch))
                .Include(m => m.Configuration_Status)
                .Where(x => x.FK_VenueId == currentUser.CurrentVenueId && x.FK_PlayerTypeId == 1 && x.FK_StatusId == 1)
                .OrderByDescending(x => x.CreatedDate);
            return View(await master_Player.ToListAsync());
        }
    }
}