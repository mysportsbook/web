using MySportsBook.Model;
using MySportsBook.Model.ViewModel;
using MySportsBook.Web.Controllers;
using MySportsBook.Web.Filters;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace MySportsBook.Web.Areas.Transaction.Controllers
{
    [UserAuthentication]
    public class InvoiceController : BaseController
    {
        // GET: Transaction/Invoice
        public async Task<ActionResult> Index()
        {
            var master_Player = dbContext.Master_Player.Include(m => m.Configuration_PlayerType).Include(m => m.Configuration_Status).Where(x => x.FK_VenueId == currentUser.CurrentVenueId && x.FK_PlayerTypeId == 1);
            return View(await master_Player.Where(x => x.FK_StatusId == 1).ToListAsync());
        }

        public async Task<ActionResult> All()
        {
            return View();
        }

        [HttpGet]
        public ActionResult GetInvoiceList(int id)
        {
            InvoiceModel invoiceModel = new InvoiceModel();
            invoiceModel.InvoiceDetails = new List<InvoiceDetailModel>();
            var batchdetails = dbContext.Transaction_PlayerSport.Include(c => c.Master_Sport).Include(c => c.Master_Batch).Where(x => x.FK_StatusId == 1 && x.FK_PlayerId == id);
            int _months = 0, _freq = 0, _totalmonths = 0;
            string[] _listMonths = new string[] { };
            DateTime _date;
            batchdetails.ToList().ForEach(batch =>
            {
                _date = DateTime.ParseExact(batch.LastGeneratedMonth, "MMMyyyy", CultureInfo.CurrentCulture);
                _months = ((DateTime.Now.Year - _date.Year) * 12) + (DateTime.Now.Month + 1 - _date.Month);
                if (_months > 0)
                {
                    _freq = batch.FK_InvoicePeriodId == 1 ? 1 : batch.FK_InvoicePeriodId == 2 ? 3 : batch.FK_InvoicePeriodId == 3 ? 6 : 12;
                    _totalmonths = (int)Math.Ceiling(_months / (decimal)_freq) * _freq;
                    _listMonths = Enumerable.Range(0, Int32.MaxValue)
                     .Select(e => _date.AddMonths(e + 1))
                     .TakeWhile(e => e <= _date.AddMonths(1).AddMonths(_totalmonths).ToUniversalTime())
                     .Select(e => e.ToString("MMMyyyy").ToUpper()).ToArray();
                    for (int count = 0; count < _totalmonths; count += _freq)
                    {
                        invoiceModel.InvoiceDetails.Add(new InvoiceDetailModel()
                        {
                            BatchId = batch.FK_BatchId,
                            SportName = batch.Master_Sport.SportName,
                            Fee = (double)batch.Master_Batch.Fee * _freq,
                            InvoicePeriod = (batch.FK_InvoicePeriodId == 1 ? _listMonths[count] : _listMonths[count] + "-" + _listMonths[count + (_freq - 1)]),
                            InvoicePeriodId = batch.FK_InvoicePeriodId
                        });
                    }
                }

            });
            invoiceModel.PlayerId = id;
            ViewBag.PaymentMode = new SelectList(dbContext.Confirguration_PaymentMode, "PK_PaymentModeId", "PaymentMode");
            return PartialView("_Payment", (invoiceModel.InvoiceDetails != null && invoiceModel.InvoiceDetails.ToList().Count > 0) ? invoiceModel : new InvoiceModel() { NoDues = true });
        }

        [HttpPost]
        public ActionResult Payment(InvoiceModel invoiceModel)
        {
            try
            {
                Save(invoiceModel);
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {

                return Json(false, JsonRequestBehavior.AllowGet);
            }


        }

        #region [ NonAction Methods ]

        [NonAction]
        public ActionResult Save(InvoiceModel invoice)
        {
            var _playerinvoice = dbContext.Transaction_Invoice.Where(inv => inv.FK_StatusId.Equals(3) && inv.FK_VenueId == invoice.VenueId && inv.FK_PlayerId == invoice.PlayerId)
                   .Join(dbContext.Transaction_InvoiceDetail.Where(detail => detail.FK_StatusId.Equals(3)), inv => inv.PK_InvoiceId, detail => detail.FK_InvoiceId, (inv, detail) => new { inv, detail }).ToList();
            if (invoice.InvoiceDetails.Count > 0)
            {
                var _shouldupdate = false;
                if (_playerinvoice != null && _playerinvoice.Count > 0)
                {
                    invoice.InvoiceDetails.ForEach(d =>
                    {
                        if (!_shouldupdate)
                        {
                            _shouldupdate = _playerinvoice.Any(p => p.detail.InvoicePeriod.Equals(d.InvoicePeriod));
                        }
                    });
                }
                if (_shouldupdate)
                {
                    Update(invoice);
                }
                else
                {
                    //foreach (var item in invoice.InvoiceDetails)
                    //{
                    //    if (!CheckforPreviousInvoice(invoice.PlayerId, item.BatchId, item.InvoicePeriod, item.InvoicePeriodId))
                    //    {
                    //        return Json("Frist pay pending invoice for previous month", JsonRequestBehavior.AllowGet);
                    //    }
                    //}
                    invoice.TotalFee = invoice.InvoiceDetails.Sum(x => x.Fee);
                    var _transInvoice = new Transaction_Invoice()
                    {
                        FK_VenueId = currentUser.CurrentVenueId,
                        FK_PlayerId = invoice.PlayerId,
                        FK_StatusId = ((invoice.TotalFee + invoice.TotalFee - invoice.TotalDiscount) <= invoice.PaidAmount) ? 4 : 3,
                        InvoiceDate = DateTime.Now.ToUniversalTime(),
                        InvoiceNumber = GenerateInvoiceNo(),
                        DueDate = DateTime.Now.AddDays(10).ToUniversalTime(),
                        TotalFee = (decimal)invoice.TotalFee,
                        TotalDiscount = (decimal)invoice.TotalDiscount,
                        OtherAmount = (decimal)invoice.OtherAmount,
                        PaidAmount = (decimal)invoice.PaidAmount,
                        Comments = invoice.Comments,
                        CreatedBy = currentUser.UserId,
                        CreatedDate = DateTime.Now.ToUniversalTime()
                    };
                    dbContext.Transaction_Invoice.Add(_transInvoice);
                    dbContext.SaveChanges();
                    invoice.InvoiceId = _transInvoice.PK_InvoiceId;
                    invoice.InvoiceDetails.ForEach(d =>
                    {
                        SaveDetail(_transInvoice.PK_InvoiceId, invoice.PlayerId, invoice.Comments, _transInvoice.FK_StatusId, d);
                    });
                    dbContext.SaveChanges();
                    invoice.InvoiceDetails.ForEach(d =>
                    {
                        UpdateLastInvGenerated(_transInvoice.PK_InvoiceId, invoice.PlayerId, d.BatchId, d.InvoicePeriod);
                    });
                    //Generate Receipt
                    var _invoice = dbContext.Transaction_Invoice.Find(_transInvoice.PK_InvoiceId);
                    dbContext.Transaction_Receipt.Add(new Transaction_Receipt()
                    {
                        PK_ReceiptId = 0,
                        ReceiptNumber = GenerateInvoiceNo(),
                        ReceiptDate = DateTime.Now.ToUniversalTime(),
                        AmountTobePaid = _invoice.TotalFee,
                        OtherAmount = _invoice.OtherAmount,
                        DiscountAmount = _invoice.TotalDiscount,
                        AmountPaid = (decimal)_invoice.PaidAmount,
                        FK_InvoiceId = _invoice.PK_InvoiceId,
                        FK_PaymentModeId = invoice.PaymentId,
                        FK_StatusId = 1,
                        FK_VenueId = currentUser.CurrentVenueId,
                        Description = invoice.Comments,
                        CreatedBy = currentUser.UserId,
                        CreatedDate = DateTime.Now.ToUniversalTime()
                    });
                    dbContext.SaveChanges();
                }
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            return Json("Nothing to save", JsonRequestBehavior.AllowGet);
        }
        [NonAction]
        public void Update(InvoiceModel invoice)
        {
            //TODO:UPdate the Invoice
        }
        [NonAction]
        public void Close(InvoiceModel invoice)
        {
            if (invoice.InvoiceId == 0)
            {
                var _transInvoice = new Transaction_Invoice()
                {
                    FK_VenueId = invoice.VenueId,
                    FK_PlayerId = invoice.PlayerId,
                    FK_StatusId = 4,
                    InvoiceDate = invoice.InvoiceDate,
                    InvoiceNumber = GenerateInvoiceNo(),
                    DueDate = invoice.InvoiceDate,
                    TotalFee = (decimal)invoice.TotalFee,
                    TotalDiscount = (decimal)invoice.TotalDiscount,
                    OtherAmount = (decimal)invoice.OtherAmount,
                    PaidAmount = (decimal)invoice.PaidAmount,
                    Comments = invoice.Comments,
                    CreatedBy = currentUser.UserId,
                };
                dbContext.Transaction_Invoice.Add(_transInvoice);
                dbContext.SaveChanges();
                invoice.InvoiceId = _transInvoice.PK_InvoiceId;
                invoice.InvoiceDetails.ForEach(d =>
                {
                    SaveDetail(_transInvoice.PK_InvoiceId, invoice.PlayerId, invoice.Comments, 4, d);
                });
                dbContext.SaveChanges();
                invoice.InvoiceDetails.ForEach(d =>
                {
                    UpdateLastInvGenerated(_transInvoice.PK_InvoiceId, invoice.PlayerId, d.BatchId, d.InvoicePeriod);
                });

            }
            else
            {
                invoice.InvoiceDetails.ForEach(d =>
                {
                    var _details = dbContext.Transaction_InvoiceDetail.Find(d.InvoiceDetailssId);
                    if (_details != null)
                    {
                        _details.Comments = invoice.Comments;
                        _details.FK_StatusId = 4;
                        _details.ModifiedBy = currentUser.UserId;
                        _details.ModifiedDate = DateTime.Now.ToUniversalTime();
                        dbContext.Entry(_details).State = EntityState.Modified;
                    }
                    else
                    {
                        SaveDetail(invoice.InvoiceId, invoice.PlayerId, invoice.Comments, 4, d);
                    }
                });
                dbContext.SaveChanges();
                if (dbContext.Transaction_InvoiceDetail.All(i => i.FK_InvoiceId == invoice.InvoiceId && (i.FK_StatusId.Equals(4) || i.FK_StatusId.Equals(2))))
                {
                    var _invupdate = dbContext.Transaction_Invoice.Find(invoice.InvoiceId);
                    if (_invupdate != null)
                    {
                        _invupdate.FK_StatusId = 4;
                        dbContext.Entry(_invupdate).State = EntityState.Modified;
                        dbContext.SaveChanges();

                    }
                }
                invoice.InvoiceDetails.ForEach(d =>
                {
                    UpdateLastInvGenerated(invoice.InvoiceId, invoice.PlayerId, d.BatchId, d.InvoicePeriod);
                });
            }
            if (invoice.InvoiceId != 0)
            {
                if (dbContext.Transaction_InvoiceDetail.All(d => d.FK_InvoiceId == invoice.InvoiceId && d.FK_StatusId == 4))
                {
                    var _invoice = dbContext.Transaction_Invoice.Find(invoice.InvoiceId);
                    _invoice.FK_StatusId = 4;
                    _invoice.ModifiedBy = currentUser.UserId;
                    _invoice.ModifiedDate = DateTime.Now.ToUniversalTime();
                    dbContext.Entry(_invoice).State = EntityState.Modified;
                    dbContext.SaveChanges();
                }
            }
            dbContext.SaveChanges();
        }
        [NonAction]
        public void SaveDetail(int invoiceid, int playerid, string comments, int statusid, InvoiceDetailModel detail)
        {
            dbContext.Transaction_InvoiceDetail.Add(new Transaction_InvoiceDetail()
            {
                FK_BatchId = detail.BatchId,
                FK_InvoiceId = invoiceid,
                FK_StatusId = statusid,
                BatchAmount = (decimal)detail.Fee,
                InvoicePeriod = detail.InvoicePeriod,
                Comments = comments,
                CreatedBy = currentUser.UserId,
                CreatedDate = DateTime.Now.ToUniversalTime()
            });

        }
        [NonAction]
        public void UpdateLastInvGenerated(int invoiceid, int playerid, int BatchId, string InvoicePeriod)
        {
            var _playersport = dbContext.Transaction_PlayerSport.Where(s => s.FK_PlayerId == playerid && s.FK_BatchId == BatchId).FirstOrDefault();
            if (_playersport != null)
            {
                _playersport.LastGeneratedMonth = InvoicePeriod.IndexOf('-') > 0 ? InvoicePeriod.Split('-')[1].ToString().Trim() : InvoicePeriod;
                _playersport.ModifiedBy = currentUser.UserId;
                _playersport.ModifiedDate = DateTime.Now.ToUniversalTime();
                dbContext.Entry(_playersport).State = EntityState.Modified;
            }
            dbContext.SaveChanges();
        }
        [NonAction]
        public bool CheckforPreviousInvoice(int playerid, int batchid, string invperiod, int invperiodid)
        {
            //TODO:Need to check if all the previous invoice are generated
            var _playersport = dbContext.Transaction_PlayerSport.Where(s => s.FK_PlayerId == playerid && s.FK_BatchId == batchid).FirstOrDefault();
            if (invperiodid == 1)
            {
                var _currdate = DateTime.ParseExact(invperiod, "MMMyyyy", CultureInfo.CurrentCulture).AddMonths(-1);
                if (dbContext.Transaction_Invoice.Where(i => i.FK_PlayerId == playerid && i.FK_StatusId == 1)
                    .Join(dbContext.Transaction_InvoiceDetail.Where(d => d.InvoicePeriod == _currdate.ToString("MMMyyyy")), invoice => invoice.PK_InvoiceId, details => details.FK_InvoiceId, (invoice, details) => new { invoice, details })
                    .Any())
                {
                    return true;
                }
            }

            return true;
        }
        [NonAction]
        string GenerateInvoiceNo()
        {
            Random generator = new Random();
            return generator.Next(0, 999999).ToString("D6");
        }

        #endregion [ NonAction Methods ]
    }
}