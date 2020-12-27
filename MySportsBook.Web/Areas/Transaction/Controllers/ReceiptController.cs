using MySportsBook.Model;
using MySportsBook.Model.ViewModel;
using MySportsBook.Web.Controllers;
using MySportsBook.Web.Filters;
using System;
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
        private string CloseMessage = "CLOSED AUTOMATICALLY BY THE SYSTEM!";
        Transaction_Receipt _master_Receipt;
        List<InvoiceDetailModel> _invoiceDetailModels;
        List<Transaction_Invoice> _transactionInvoices;
        Transaction_Invoice _transactionInvoiceReceipt, _transactionInvoiceClose;
        // GET: Transaction/Invoice
        public async Task<ActionResult> Index()
        {
            var master_Receipt = dbContext.Transaction_Receipt.Include(m => m.Confirguration_PaymentMode).Include(m => m.Transaction_Invoice).Include(m => m.Transaction_Invoice.Master_Player).Include(m => m.Configuration_Status).Where(x => x.FK_VenueId == currentUser.CurrentVenueId);
            return View(await master_Receipt.Where(x => x.FK_StatusId == 1).ToListAsync());
        }

        public ActionResult Edit(int? id)
        {
            GetInvoices(id.Value);
            ViewBag.PaymentMode = new SelectList(dbContext.Confirguration_PaymentMode, "PK_PaymentModeId", "PaymentMode", _master_Receipt.FK_PaymentModeId);
            ViewBag.ReceivedBy = new SelectList(dbContext.Configuration_User.Where(x => dbContext.Master_UserVenue.Any(v => v.FK_UserId == x.PK_UserId && v.FK_VenueId == currentUser.CurrentVenueId) && x.PK_UserId != 0), "PK_UserId", "UserName", _master_Receipt.ReceivedBy);
            return View(_master_Receipt);

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

        [HttpGet]
        public ActionResult Delete(int? id)
        {
            try
            {
                return Json(DeleteReceiptInvoice(id), JsonRequestBehavior.AllowGet);
            }
            catch (System.Exception)
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }
        // GET: Master/Player
        public ActionResult Player()
        {
            var master_Player = dbContext.Master_Player
                .Include(m => m.Transaction_PlayerSport.Select(q => q.Master_Sport))
                .Include(m => m.Transaction_PlayerSport.Select(q => q.Master_Batch))
                .Include(m => m.Configuration_Status)
                .Include(m => m.Transaction_Invoice.Select(q => q.Transaction_Receipt))
                .Where(x => x.FK_VenueId == currentUser.CurrentVenueId && x.FK_PlayerTypeId == 1 && x.FK_StatusId == 1)
                .OrderByDescending(x => x.CreatedDate).ToList()
                .Select(x => new Master_Player()
                {
                    PK_PlayerId=x.PK_PlayerId,
                    FirstName = x.FirstName,
                    LastName = x.LastName,
                    Mobile = x.Mobile,
                    FK_StatusId = x.FK_StatusId,
                    Transaction_PlayerSport = x.Transaction_PlayerSport,
                    Configuration_Status = x.Configuration_Status,
                    Transaction_Invoice = x.Transaction_Invoice.OrderByDescending(s => s.PK_InvoiceId).Take(1).ToList()

                }).ToList();
            return View(master_Player.ToList());
        }


        #region[ Non Action Methods]
        private bool Save(InvoiceModel invoiceModel)
        {
            GetInvoices(invoiceModel.PlayerId);
            if (_master_Receipt.PK_ReceiptId == invoiceModel.ReceiptId)
            {
                CloseInvoices(invoiceModel);
                return true;
            }
            else
            {
                return false;
            }
        }

        bool DeleteReceiptInvoice(int? id)
        {
            var _result = dbContext.DeleteReceipt(id).ToList();
            return (_result.FirstOrDefault().HasValue && _result.FirstOrDefault().Value == 1);
        }

        void CloseInvoices(InvoiceModel invoiceModel)
        {
            List<string> _oldclosedMonths = _transactionInvoiceClose.Transaction_InvoiceDetail.Select(x => x.InvoicePeriod).ToList();
            List<string> _newclosedMonths = invoiceModel.InvoiceDetails.Where(x => x.ShouldClose).Select(x => x.InvoicePeriod).ToList();
            var _deleteMonths = _oldclosedMonths.Except(_newclosedMonths).ToList();
            var _addMonths = _newclosedMonths.Except(_oldclosedMonths).ToList();
            if (_deleteMonths?.Count > 0)
            {
                List<int> _ids = new List<int>();
                _transactionInvoiceClose.Transaction_InvoiceDetail.ToList().FindAll(x => _deleteMonths.Any(m => m == x.InvoicePeriod)).ForEach(c =>
                {
                    _ids.Add(c.PK_InvoiceDetailId);
                });
                var _details = dbContext.Transaction_InvoiceDetail
                    .Where(p => p.FK_InvoiceId == _transactionInvoiceClose.PK_InvoiceId && _ids.Contains(p.PK_InvoiceDetailId)).ToList();
                dbContext.Transaction_InvoiceDetail.RemoveRange(_details);
            }
            if (_addMonths?.Count > 0)
            {
                invoiceModel.InvoiceDetails.FindAll(x => _addMonths.Any(m => m == x.InvoicePeriod)).ForEach(c =>
                {
                    dbContext.Transaction_InvoiceDetail.Add(new Transaction_InvoiceDetail()
                    {
                        FK_BatchId = c.BatchId,
                        FK_InvoiceId = invoiceModel.InvoiceId,
                        FK_StatusId = 4,
                        BatchAmount = (decimal)c.Fee,
                        InvoicePeriod = c.InvoicePeriod,
                        PaidAmount = 0,
                        Comments = CloseMessage,
                        CreatedBy = currentUser.UserId,
                        CreatedDate = DateTime.Now.ToLocalTime()
                    });
                });
            }
            dbContext.SaveChanges();

        }

        void GetInvoices(int PlayerId)
        {
            _transactionInvoices = new List<Transaction_Invoice>();
            _transactionInvoiceReceipt = new Transaction_Invoice();
            _master_Receipt = new Transaction_Receipt();
            _invoiceDetailModels = new List<InvoiceDetailModel>();
            _transactionInvoices = dbContext.Transaction_Invoice
               .Include(x => x.Transaction_InvoiceDetail)
               .Where(x => x.FK_VenueId == currentUser.CurrentVenueId && x.FK_PlayerId == PlayerId)
               .OrderByDescending(s => s.CreatedDate).Take(2).ToList();
            _transactionInvoiceReceipt = _transactionInvoices.FirstOrDefault();
            _master_Receipt = dbContext.Transaction_Receipt.Include(m => m.Confirguration_PaymentMode).Include(m => m.Transaction_Invoice.Transaction_InvoiceDetail).Include(m => m.Transaction_Invoice.Master_Player).Include(m => m.Configuration_Status).Where(x => x.FK_VenueId == currentUser.CurrentVenueId && x.Transaction_Invoice.FK_PlayerId == PlayerId && _transactionInvoiceReceipt.PK_InvoiceId == x.FK_InvoiceId && x.FK_StatusId == 1).ToList().FirstOrDefault();
            if (!_transactionInvoices.All(x => dbContext.Transaction_Receipt.Any(r => r.FK_InvoiceId == x.PK_InvoiceId)))
            {
                var _transactionInvoiceClose = _transactionInvoices.ToList().LastOrDefault();
                if (_transactionInvoices.Count() > 1)
                {
                    _transactionInvoiceClose.Transaction_InvoiceDetail.ToList().ForEach(x =>
                    {
                        _invoiceDetailModels.Add(new InvoiceDetailModel()
                        {
                            InvoicePeriod = x.InvoicePeriod,
                            Fee = (double)x.BatchAmount,
                            PaidAmount = (double)x.PaidAmount

                        });
                    });
                }
            }
            //Reordering the Display List
            if (_master_Receipt != null)
            {
                _master_Receipt.Transaction_Invoice.Transaction_InvoiceDetail.ToList().ForEach(x =>
                {
                    _invoiceDetailModels.Add(new InvoiceDetailModel()
                    {
                        InvoicePeriod = x.InvoicePeriod,
                        Fee = (double)x.BatchAmount,
                        PaidAmount = (double)x.PaidAmount
                    });
                });
                _master_Receipt.Transaction_Invoice.Transaction_InvoiceDetail.Clear();
                _invoiceDetailModels.OrderByDescending(x => x.InvoicePeriodDate).ToList().ForEach(x =>
                {
                    _master_Receipt.Transaction_Invoice.Transaction_InvoiceDetail.Add(new Transaction_InvoiceDetail() { InvoicePeriod = x.InvoicePeriod, BatchAmount = (decimal)x.Fee, PaidAmount = (decimal)x.PaidAmount });
                });
            }
        }

        #endregion[ Non Action Methods]
    }
}