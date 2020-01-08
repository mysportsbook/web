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
        private string CloseMessage = "CLOSED AUTOMATICALLY BY THE SYSTEM!";
        // GET: Transaction/Invoice
        public async Task<ActionResult> Index()
        // public ActionResult Index()
        {
            var master_Player = dbContext.Master_Player
                .Include(m => m.Transaction_PlayerSport).Include(m => m.Transaction_PlayerSport.Select(b => b.Master_Batch))
              .Where(x => x.FK_VenueId == currentUser.CurrentVenueId && x.FK_PlayerTypeId == 1 && x.FK_StatusId == 1);
            return View(await master_Player.ToListAsync());
        }

        [HttpGet]
        public ActionResult GetInvoiceList(int id)
        {
            InvoiceModel invoiceModel = GetInvoiceDetailList(id);
            ViewBag.PaymentMode = new SelectList(dbContext.Confirguration_PaymentMode, "PK_PaymentModeId", "PaymentMode");
            ViewBag.ReceivedBy = new SelectList(dbContext.Configuration_User.Where(x => dbContext.Master_UserVenue.Any(v => v.FK_UserId == x.PK_UserId && v.FK_VenueId == currentUser.CurrentVenueId) && x.PK_UserId != 0), "PK_UserId", "UserName", currentUser.UserId.ToString());
            return PartialView("_Payment", (invoiceModel.InvoiceDetails != null && invoiceModel.InvoiceDetails.ToList().Count > 0) ? invoiceModel : new InvoiceModel() { NoDues = true });
        }

        [HttpGet]
        public ActionResult GetPaymentHistory(int id)
        {
            List<ReceiptModel> receiptModels = GetPaymentHistoryList(id);
            return PartialView("_PaymentHistory", receiptModels);
        }

        [HttpPost]
        public ActionResult Payment(InvoiceModel invoiceModel)
        {
            try
            {
                return Save(invoiceModel);
            }
            catch (Exception)
            {

                return Json(false, JsonRequestBehavior.AllowGet);
            }


        }

        #region [ NonAction Methods ]


        [NonAction]
        public void Update(InvoiceModel invoice)
        {
            //TODO:UPdate the Invoice
        }

        [NonAction]
        public void SaveDetail(int invoiceid, int playerid, string comments, int statusid, InvoiceDetailModel detail)
        {
            int _statusid = statusid != 4 ? (detail.Fee == detail.PaidAmount ? 4 : 3) : 4;
            if (statusid == 4 && dbContext.Transaction_InvoiceDetail.Any(i => i.FK_StatusId == 3 && i.FK_BatchId == detail.BatchId && i.InvoicePeriod == detail.InvoicePeriod))
            {
                var _details = dbContext.Transaction_InvoiceDetail.Where(i => i.FK_StatusId == 3 && i.FK_BatchId == detail.BatchId && i.InvoicePeriod == detail.InvoicePeriod);
                if (_details?.Count() > 0)
                {
                    _details.ToList().ForEach(d =>
                    {
                        d.Comments += comments;
                        //CLOSE THE INVOICE DETAIL IF PAID
                        d.FK_StatusId = 4;
                        d.ModifiedBy = currentUser.UserId;
                        d.ModifiedDate = DateTime.Now.ToLocalTime();
                        dbContext.Entry(d).State = EntityState.Modified;

                    });
                }
            }
            else
            {
                dbContext.Transaction_InvoiceDetail.Add(new Transaction_InvoiceDetail()
                {
                    FK_BatchId = detail.BatchId,
                    FK_InvoiceId = invoiceid,
                    FK_StatusId = _statusid,
                    BatchAmount = (decimal)detail.Fee,
                    InvoicePeriod = detail.InvoicePeriod,
                    PaidAmount = (decimal)detail.PaidAmount,
                    Comments = comments,
                    CreatedBy = currentUser.UserId,
                    CreatedDate = DateTime.Now.ToLocalTime()
                });

            }
            if (_statusid == 4 && dbContext.Transaction_InvoiceDetail.Any(d => d.FK_StatusId == 3 && d.InvoicePeriod == detail.InvoicePeriod && dbContext.Transaction_Invoice.Any(i => i.FK_StatusId == 3 && i.PK_InvoiceId == d.FK_InvoiceId && i.FK_PlayerId == playerid)))
            {
                var _details = dbContext.Transaction_InvoiceDetail.Where(d => d.FK_StatusId == 3 && d.InvoicePeriod == detail.InvoicePeriod && dbContext.Transaction_Invoice.Any(i => i.FK_StatusId == 3 && i.PK_InvoiceId == d.FK_InvoiceId && i.FK_PlayerId == playerid));
                if (_details?.Count() > 0)
                {
                    _details.ToList().ForEach(d =>
                    {
                        //CLOSE THE INVOICE DETAIL IF PAID
                        d.FK_StatusId = 4;
                        d.ModifiedBy = currentUser.UserId;
                        d.ModifiedDate = DateTime.Now.ToLocalTime();
                        dbContext.Entry(d).State = EntityState.Modified;

                    });
                }
            }
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

        [NonAction]
        InvoiceModel GetInvoiceDetailList(int playerId)
        {
            InvoiceModel invoiceModel = new InvoiceModel();
            invoiceModel.InvoiceDetails = new List<InvoiceDetailModel>();
            var batchdetails = dbContext.Transaction_PlayerSport.Include(c => c.Master_Sport).Include(c => c.Master_Batch).Where(x => x.FK_StatusId == 1 && x.FK_PlayerId == playerId);
            if (dbContext.Master_Venue.Find(currentUser.CurrentVenueId).GuestPlayerId != playerId)
            {
                var _openInvoice = dbContext.Transaction_InvoiceDetail.Where(x => x.FK_StatusId == 3 && dbContext.Transaction_Invoice.Any(i => i.FK_PlayerId == playerId && i.FK_StatusId == 3 && i.PK_InvoiceId == x.FK_InvoiceId));
                var _creditBalance = dbContext.Master_Player.Where(p => p.PK_PlayerId == playerId && p.FK_StatusId == 1).FirstOrDefault().CreditBalance;
                invoiceModel.ExtraPaidAmount = (double)(_creditBalance.HasValue ? _creditBalance : 0);
                invoiceModel.InvoiceDetails.AddRange(GetDueList(playerId, batchdetails.ToList(), _openInvoice.ToList(), false));
            }
            else
            {
                batchdetails.ToList().ForEach(batch =>
                {
                    invoiceModel.InvoiceDetails.Add(new InvoiceDetailModel()
                    {
                        BatchId = batch.FK_BatchId,
                        BatchName = batch.Master_Batch.BatchName,
                        SportName = batch.Master_Sport.SportName,
                        Fee = (double)batch.Fee,
                        InvoicePeriod = DateTime.Now.ToString("MMMyyyy").ToUpper(),
                        InvoicePeriodId = batch.FK_InvoicePeriodId,
                        PayOrder = 1
                    });

                });
            }
            invoiceModel.PlayerId = playerId;
            return invoiceModel;
        }

        [NonAction]
        private void GetDueList(ref InvoiceModel invoiceModel, IQueryable<Transaction_PlayerSport> batchdetails)
        {
            int _months = 0, _freq = 0, _totalmonths = 0, _sortorder = 0;
            string[] _listMonths = new string[] { };
            DateTime _date;
            foreach (var batch in batchdetails.ToList())
            {
                _date = DateTime.ParseExact(batch.LastGeneratedMonth, "MMMyyyy", CultureInfo.CurrentCulture);
                _months = ((DateTime.Now.Year - _date.Year) * 12) + (DateTime.Now.Month + 1 - _date.Month);
                if (_months > 0)
                {
                    _sortorder = 1;
                    _freq = batch.FK_InvoicePeriodId == 1 ? 1 : batch.FK_InvoicePeriodId == 2 ? 3 : batch.FK_InvoicePeriodId == 3 ? 6 : 12;
                    _totalmonths = (int)Math.Ceiling(_months / (decimal)_freq) * _freq;
                    _listMonths = Enumerable.Range(0, Int32.MaxValue)
                     .Select(e => _date.AddMonths(e + 1))
                     .TakeWhile(e => e <= _date.AddMonths(1).AddMonths(_totalmonths).ToLocalTime())
                     .Select(e => e.ToString("MMMyyyy").ToUpper()).ToArray();
                    for (int count = 0; count < _totalmonths; count += _freq)
                    {
                        invoiceModel.InvoiceDetails.Add(new InvoiceDetailModel()
                        {
                            BatchId = batch.FK_BatchId,
                            BatchName = batch.Master_Batch.BatchName,
                            SportName = batch.Master_Sport.SportName,
                            Fee = (double)batch.Fee,
                            InvoicePeriod = (batch.FK_InvoicePeriodId == 1 ? _listMonths[count] : _listMonths[count] + "-" + _listMonths[count + (_freq - 1)]),
                            InvoicePeriodId = batch.FK_InvoicePeriodId,
                            PayOrder = _sortorder
                        });
                        _sortorder += 1;
                    }
                }
            }
        }

        [NonAction]
        private void ChangeStatus(int PlayerId, int InvoiceId)
        {
            //CLOSE THE INVOICE IF PAID
            var _invoiceDetails = dbContext.Transaction_InvoiceDetail
                                                            .Where(d => d.FK_StatusId == 4 && d.FK_InvoiceId == InvoiceId &&
                                                                        dbContext.Transaction_Invoice.Any(i => i.FK_PlayerId == PlayerId && i.PK_InvoiceId == d.FK_InvoiceId));
            if (_invoiceDetails?.Count() > 0)
            {
                _invoiceDetails.ToList().ForEach(d =>
                {
                    if (dbContext.Transaction_InvoiceDetail
                                    .Any(x => x.FK_StatusId == 3 && x.InvoicePeriod == d.InvoicePeriod &&
                                    dbContext.Transaction_Invoice.Any(i => i.FK_PlayerId == PlayerId &&
                                                                        i.PK_InvoiceId == x.FK_InvoiceId)))
                    {
                        dbContext.Transaction_InvoiceDetail
                                        .Where(x => x.FK_StatusId == 3 && x.InvoicePeriod == d.InvoicePeriod &&
                                        dbContext.Transaction_Invoice.Any(i => i.FK_PlayerId == PlayerId &&
                                                                            i.PK_InvoiceId == x.FK_InvoiceId)).ToList().ForEach(c =>
                          {
                              c.FK_StatusId = 4;
                              c.ModifiedBy = currentUser.UserId;
                              c.ModifiedDate = DateTime.Now.ToLocalTime();
                              dbContext.Entry(c).State = EntityState.Modified;
                          });
                        dbContext.SaveChanges();
                    }
                });
                if (dbContext.Transaction_Invoice.Any(x => x.FK_PlayerId == PlayerId && x.FK_StatusId == 3))
                {
                    dbContext.Transaction_Invoice.Where(x => x.FK_PlayerId == PlayerId && x.FK_StatusId == 3).ToList().ForEach(inv =>
                    {
                        if (!dbContext.Transaction_InvoiceDetail.Any(d => d.FK_StatusId == 3 && d.FK_InvoiceId == inv.PK_InvoiceId))
                        {
                            inv.FK_StatusId = 4;
                            inv.ModifiedBy = currentUser.UserId;
                            inv.ModifiedDate = DateTime.Now.ToLocalTime();
                            dbContext.Entry(inv).State = EntityState.Modified;
                        }
                    });
                    dbContext.SaveChanges();
                }
            }
        }

        [NonAction]
        List<ReceiptModel> GetPaymentHistoryList(int PlayerId)
        {
            var master_Receipt = dbContext.Transaction_Receipt
                                    .Join(dbContext.Confirguration_PaymentMode,rec=>rec.FK_PaymentModeId,pay=>pay.PK_PaymentModeId,(receipt,payment)=>new { receipt, payment })
                                    .Join(dbContext.Configuration_User, rec => rec.receipt.ReceivedBy, user => user.PK_UserId, (receiptpay, user) => new { receiptpay, user })
                                    .Join(dbContext.Transaction_Invoice, rec => rec.receiptpay.receipt.FK_InvoiceId, inv => inv.PK_InvoiceId, (receiptpayuser, invoice) => new { receiptpayuser, invoice })
                                    .Join(dbContext.Transaction_InvoiceDetail, recinv => recinv.invoice.PK_InvoiceId, detail => detail.FK_InvoiceId, (receiptinvoice, details) => new { receiptinvoice, details })
                                    .Join(dbContext.Master_Batch, recinv => recinv.details.FK_BatchId, batch => batch.PK_BatchId, (receiptinvoice, batch) => new { receiptinvoice, batch })
                                    .Join(dbContext.Master_Court, recinvbat => recinvbat.batch.FK_CourtId, cou => cou.PK_CourtId, (receiptinvoicebat, court) => new { receiptinvoicebat, court })
                                    .Join(dbContext.Master_Sport, recinvbatcou => recinvbatcou.court.FK_SportId, sport => sport.PK_SportId, (receiptinvoicebatcou, sport) => new { receiptinvoicebatcou, sport })
                                    .Where(p => p.receiptinvoicebatcou.receiptinvoicebat.receiptinvoice.receiptinvoice.invoice.FK_PlayerId == PlayerId)
                                    .GroupBy(x => new { x.sport.SportName, x.receiptinvoicebatcou.receiptinvoicebat.batch.BatchName, x.receiptinvoicebatcou.receiptinvoicebat.receiptinvoice.receiptinvoice.receiptpayuser.receiptpay.receipt.AmountPaid, x.receiptinvoicebatcou.receiptinvoicebat.receiptinvoice.receiptinvoice.receiptpayuser.receiptpay.receipt.ReceiptDate, x.receiptinvoicebatcou.receiptinvoicebat.receiptinvoice.receiptinvoice.receiptpayuser.receiptpay.receipt.ReceiptNumber }).ToList()
                                    .Select(s => new ReceiptModel
                                    {
                                        ReceiptNumber = s.FirstOrDefault().receiptinvoicebatcou.receiptinvoicebat.receiptinvoice.receiptinvoice.receiptpayuser.receiptpay.receipt.ReceiptNumber,
                                        ReceiptDate = s.FirstOrDefault().receiptinvoicebatcou.receiptinvoicebat.receiptinvoice.receiptinvoice.receiptpayuser.receiptpay.receipt.ReceiptDate,
                                        AmountPaid = s.FirstOrDefault().receiptinvoicebatcou.receiptinvoicebat.receiptinvoice.receiptinvoice.receiptpayuser.receiptpay.receipt.AmountPaid,
                                        Sport = String.Join(",", s.Select(c => c.sport.SportName).Distinct()),
                                        Month = String.Join(",", s.Select(b => b.receiptinvoicebatcou.receiptinvoicebat.receiptinvoice.details.InvoicePeriod).Distinct()),
                                        PaymentMode = s.FirstOrDefault().receiptinvoicebatcou.receiptinvoicebat.receiptinvoice.receiptinvoice.receiptpayuser.receiptpay.payment.PaymentMode,
                                        Received = s.FirstOrDefault().receiptinvoicebatcou.receiptinvoicebat.receiptinvoice.receiptinvoice.receiptpayuser.user.FirstName
                                    });
            //.ToList().Select(x => new ReceiptModel
            //{
            //    ReceiptNumber = x.receiptinvoicebatcou.receiptinvoicebat.receiptinvoice.receiptinvoice.receipt.ReceiptNumber,
            //    ReceiptDate = x.receiptinvoicebatcou.receiptinvoicebat.receiptinvoice.receiptinvoice.receipt.ReceiptDate,
            //    AmountPaid = x.receiptinvoicebatcou.receiptinvoicebat.receiptinvoice.receiptinvoice.receipt.AmountPaid,
            //    Sport = String.Join(",", x.sport.SportName),
            //    Month = String.Join(",", x.receiptinvoicebatcou.receiptinvoicebat.receiptinvoice.details.InvoicePeriod)
            //});
            return master_Receipt.OrderByDescending(x => x.ReceiptDate).ToList();
        }

        [NonAction]
        private ActionResult Save(InvoiceModel invoice)
        {
            var _guestId = dbContext.Master_Venue.Find(currentUser.CurrentVenueId).GuestPlayerId;
            if (_guestId == invoice.PlayerId && invoice.TotalFee + invoice.TotalOtherAmount - invoice.TotalDiscount != invoice.TotalPaidAmount)
            {
                return Json("Paid amount Should be equal to the total amount!", JsonRequestBehavior.AllowGet);
            }
            var batchdetails = dbContext.Transaction_PlayerSport.Include(c => c.Master_Sport).Include(c => c.Master_Batch).Where(x => x.FK_StatusId == 1 && x.FK_PlayerId == invoice.PlayerId);
            var _openInvoice = dbContext.Transaction_InvoiceDetail.Where(x => x.FK_StatusId == 3 && dbContext.Transaction_Invoice.Any(i => i.FK_PlayerId == invoice.PlayerId && i.FK_StatusId == 3 && i.PK_InvoiceId == x.FK_InvoiceId));
            var _creditbalance = dbContext.Master_Player.Where(x => x.PK_PlayerId == invoice.PlayerId).FirstOrDefault().CreditBalance;
            invoice.ExtraPaidAmount = (double)(_creditbalance.HasValue ? _creditbalance : 0);
            if (_guestId != invoice.PlayerId)
            {
                var _allInvoiceDetailList = GetDueList(invoice.PlayerId, batchdetails.ToList(), _openInvoice.ToList(), false);
                var _closedInvoiceDetails = GetMonthToBeClosed(invoice.PlayerId, invoice.InvoiceDetails, _allInvoiceDetailList);
                if (_closedInvoiceDetails != null && _closedInvoiceDetails.Count > 0)
                {
                    _closedInvoiceDetails.Where(x => _allInvoiceDetailList.Any(d => d.BatchId == x.BatchId && d.InvoicePeriod == x.InvoicePeriod && d.InvoiceDetailssId > 0)).ToList().ForEach(x =>
                    {
                        GenerateInvoiceDetail(new InvoiceDetailModel() { PlayerId = invoice.PlayerId, BatchId = x.BatchId, InvoicePeriod = x.InvoicePeriod, Comments = CloseMessage, StatusId = 4 }, true);
                        UpdateLastInvGeneratedClose(invoice.PlayerId, x.BatchId, x.InvoicePeriod);
                    });
                    if (_closedInvoiceDetails.Where(x => _allInvoiceDetailList.Any(d => d.BatchId == x.BatchId && d.InvoicePeriod == x.InvoicePeriod && d.InvoiceDetailssId == 0)).ToList().Count > 0)
                    {
                        var _invoiceModel = new InvoiceModel
                        {
                            VenueId = currentUser.CurrentVenueId,
                            PlayerId = invoice.PlayerId,
                            Comments = CloseMessage,
                            TotalFee = 0,
                            TotalDiscount = 0,
                            TotalOtherAmount = 0,
                            TotalPaidAmount = 0,

                        };
                        GenerateInvoice(ref _invoiceModel);
                        _closedInvoiceDetails.Where(x => _allInvoiceDetailList.Any(d => d.BatchId == x.BatchId && d.InvoicePeriod == x.InvoicePeriod && d.InvoiceDetailssId == 0)).ToList().ForEach(x =>
                        {
                            x.PlayerId = invoice.PlayerId;
                            x.Comments = CloseMessage;
                            x.InvoiceId = _invoiceModel.InvoiceId;
                            x.StatusId = 4;
                            x.PaidAmount = 0;
                            GenerateInvoiceDetail(x, false);
                            UpdateLastInvGeneratedClose(invoice.PlayerId, x.BatchId, x.InvoicePeriod);
                        });
                        dbContext.SaveChanges();
                        CloseOthersInvoices(_invoiceModel);
                    }
                }
                SplitTheAmountInDetail(ref invoice);
            }
            GenerateInvoice(ref invoice);
            invoice.InvoiceDetails.ForEach(x =>
            {
                x.PlayerId = invoice.PlayerId;
                x.InvoiceId = invoice.InvoiceId;
                x.Comments = invoice.Comments;
                x.StatusId = (x.Fee == x.PaidAmount ? 4 : 3);
                GenerateInvoiceDetail(x, false);
            });
            dbContext.SaveChanges();
            CloseOthersInvoices(invoice);
            GenerateReceipt(invoice);
            UpdateLastInvGenerated(invoice);
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        [NonAction]
        private void CloseOthersInvoices(InvoiceModel invoice)
        {
            var _invoices = dbContext.Transaction_Invoice.Where(c => c.FK_PlayerId == invoice.PlayerId && c.FK_StatusId == 3);

            _invoices.ToList().ForEach(x =>
            {
                if (dbContext.Transaction_InvoiceDetail.Where(d => d.FK_InvoiceId == x.PK_InvoiceId).All(d => d.FK_StatusId == 4))
                {
                    x.FK_StatusId = 4;
                    x.ModifiedBy = currentUser.UserId;
                    x.ModifiedDate = DateTime.Now;
                    dbContext.Entry(x).State = EntityState.Modified;
                }
            });
            dbContext.SaveChanges();
            //invoice.InvoiceDetails.GroupBy(x => new { x.BatchId, x.InvoicePeriod }).Select(x => x.FirstOrDefault()).ToList().ForEach(x =>
            // {
            //     var _invoice = dbContext.Transaction_Invoice.Where(s => dbContext.Transaction_InvoiceDetail.All(i => i.FK_BatchId == x.BatchId && i.InvoicePeriod == x.InvoicePeriod && i.FK_StatusId == 4 && i.FK_InvoiceId == s.PK_InvoiceId) && s.FK_PlayerId == invoice.PlayerId && s.FK_StatusId == 3);
            //     if (_invoice != null)
            //     {
            //         var _results = _invoice.ToList();
            //         _results.ForEach(c =>
            //          {
            //              if (dbContext.Transaction_InvoiceDetail.All(xd => xd.FK_StatusId == 4 && xd.FK_InvoiceId == c.PK_InvoiceId))
            //              {
            //                  c.FK_StatusId = 4;
            //                  c.ModifiedBy = currentUser.UserId;
            //                  c.ModifiedDate = DateTime.Now;
            //                  dbContext.Entry(c).State = EntityState.Modified;
            //              }
            //          });
            //         dbContext.SaveChanges();
            //     }

            // });
        }

        [NonAction]
        private List<InvoiceDetailModel> GetDueList(int PlayerId, List<Transaction_PlayerSport> batchdetails, List<Transaction_InvoiceDetail> OpenInvoice, bool IsGuestplayer)
        {
            var _invoicedetailModel = new List<InvoiceDetailModel>();
            if (!IsGuestplayer)
            {
                int _months = 0, _freq = 0, _totalmonths = 0, _sortorder = 0;
                string[] _listMonths = new string[] { };
                DateTime _date;
                foreach (var batch in batchdetails)
                {
                    _date = DateTime.ParseExact(batch.LastGeneratedMonth, "MMMyyyy", CultureInfo.CurrentCulture);
                    _months = ((DateTime.Now.Year - _date.Year) * 12) + (DateTime.Now.Month + 1 - _date.Month);
                    if (_months > 0)
                    {
                        _sortorder = 1;
                        _freq = batch.FK_InvoicePeriodId == 1 ? 1 : batch.FK_InvoicePeriodId == 2 ? 3 : batch.FK_InvoicePeriodId == 3 ? 6 : 12;
                        _totalmonths = (int)Math.Ceiling(_months / (decimal)_freq) * _freq;
                        _listMonths = Enumerable.Range(0, Int32.MaxValue)
                         .Select(e => _date.AddMonths(e + 1))
                         .TakeWhile(e => e <= _date.AddMonths(1).AddMonths(_totalmonths).ToLocalTime())
                         .Select(e => e.ToString("MMMyyyy").ToUpper()).ToArray();
                        for (int count = 0; count < _totalmonths; count += _freq)
                        {
                            _invoicedetailModel.Add(new InvoiceDetailModel()
                            {
                                BatchId = batch.FK_BatchId,
                                BatchName = batch.Master_Batch.BatchName,
                                SportName = batch.Master_Sport.SportName,
                                Fee = (double)batch.Fee,
                                InvoicePeriod = (batch.FK_InvoicePeriodId == 1 ? _listMonths[count] : _listMonths[count] + "-" + _listMonths[count + (_freq - 1)]),
                                InvoicePeriodId = batch.FK_InvoicePeriodId,
                                PayOrder = _sortorder
                            });
                            _sortorder += 1;
                        }
                    }
                }
                if (OpenInvoice != null)
                {
                    _invoicedetailModel.ForEach(x =>
                    {
                        if (OpenInvoice.Any(o => o.InvoicePeriod == x.InvoicePeriod))
                        {
                            x.InvoiceDetailssId = OpenInvoice.Where(o => o.InvoicePeriod == x.InvoicePeriod).FirstOrDefault().PK_InvoiceDetailId;
                            x.Fee -= (double)OpenInvoice.Where(o => o.InvoicePeriod == x.InvoicePeriod).Sum(s => s.PaidAmount);
                        }
                    });
                }
            }
            else
            {
                foreach (var batch in batchdetails)
                {
                    _invoicedetailModel.Add(new InvoiceDetailModel()
                    {
                        BatchId = batch.FK_BatchId,
                        BatchName = batch.Master_Batch.BatchName,
                        SportName = batch.Master_Sport.SportName,
                        Fee = (double)batch.Fee,
                        InvoicePeriod = DateTime.Now.ToString("MMMyyyy").ToUpper(),
                        InvoicePeriodId = batch.FK_InvoicePeriodId,
                        PayOrder = 1
                    });

                }
            }
            return _invoicedetailModel;
        }

        [NonAction]
        private List<InvoiceDetailModel> GetMonthToBeClosed(int PlayerId, List<InvoiceDetailModel> Selecteddetails, List<InvoiceDetailModel> CompletedInvoiceDetails)
        {
            List<InvoiceDetailModel> toClosedinvoiceDetailModels = new List<InvoiceDetailModel>();
            var _selectedInvoiceDetails = Selecteddetails.OrderBy(x => x.BatchId).ThenBy(x => x.PayOrder);
            var _selectedbatchIds = _selectedInvoiceDetails.Select(x => x.BatchId).Distinct();
            var _actualInvoiceDetails = CompletedInvoiceDetails.Where(x => _selectedbatchIds.Contains(x.BatchId)).Select(x => new InvoiceDetailModel
            {
                BatchId = x.BatchId,
                Fee = x.Fee,
                InvoicePeriod = x.InvoicePeriod,
                InvoicePeriodId = x.InvoicePeriodId,
                PayOrder = x.PayOrder,
            }).OrderBy(x => x.BatchId).ThenBy(x => x.PayOrder);

            var groupedByBatchId = _selectedInvoiceDetails.GroupBy(c => c.BatchId);
            foreach (var batch in groupedByBatchId)
            {
                int maxpayOrder = batch.Max(g => g.PayOrder);
                int _batchId = batch.Key;
                var _result = _actualInvoiceDetails.Where(x => x.BatchId == _batchId && x.PayOrder < maxpayOrder).ToList().Where(x => !_selectedInvoiceDetails.Any(s => s.InvoicePeriodId == x.InvoicePeriodId && s.PayOrder == x.PayOrder && s.BatchId == _batchId)).ToList();
                if (_result != null && _result.Count > 0)
                {
                    toClosedinvoiceDetailModels.AddRange(_result);
                }
            }
            return toClosedinvoiceDetailModels;
        }

        [NonAction]
        public bool Close1(InvoiceModel invoice)
        {
            // dbContext.Transaction_InvoiceDetail.Where(x=> invoice.InvoiceDetails.Any(i=>i.BatchId==x.FK_BatchId && i.InvoicePeriod==x.InvoicePeriod) )
            //secondList.Where(p1 => initialList.Any(p2 => p1.Value == p2.Value)).ToList();
            var _openInvoice = dbContext.Transaction_InvoiceDetail.Where(x => x.FK_StatusId == 3 && dbContext.Transaction_Invoice.Any(i => i.FK_PlayerId == invoice.PlayerId && i.FK_StatusId == 3 && i.PK_InvoiceId == x.FK_InvoiceId));
            if (_openInvoice != null && _openInvoice.Where(x => invoice.InvoiceDetails.All(i => i.InvoicePeriod == x.InvoicePeriod && i.BatchId == x.FK_BatchId)).Count() > 0)
            {
                _openInvoice.Where(x => invoice.InvoiceDetails.All(i => i.InvoicePeriod == x.InvoicePeriod && i.BatchId == x.FK_BatchId));
            }
            var _transInvoice = new Transaction_Invoice()
            {
                FK_VenueId = invoice.VenueId,
                FK_PlayerId = invoice.PlayerId,
                FK_StatusId = 4,
                InvoiceDate = invoice.InvoiceDate.HasValue ? invoice.InvoiceDate.Value.ToLocalTime() : DateTime.Now.ToLocalTime(),
                InvoiceNumber = GenerateInvoiceNo(),
                DueDate = invoice.InvoiceDate.HasValue ? invoice.InvoiceDate.Value.AddDays(10).ToLocalTime() : DateTime.Now.AddDays(10).ToLocalTime(),
                TotalFee = 0,
                TotalDiscount = 0,
                OtherAmount = 0,
                PaidAmount = 0,
                Comments = invoice.Comments,
                CreatedBy = currentUser.UserId,
                CreatedDate = DateTime.Now.ToLocalTime()
            };
            dbContext.Transaction_Invoice.Add(_transInvoice);
            dbContext.SaveChanges();
            invoice.InvoiceId = _transInvoice.PK_InvoiceId;
            invoice.InvoiceDetails.ForEach(detail =>
            {
                GenerateInvoiceDetail(detail, false);
            });

            return true;
        }

        [NonAction]
        private bool SplitTheAmountInDetail(ref InvoiceModel invoice)
        {
            double _totalPaidAmount = invoice.TotalPaidAmount + invoice.ExtraPaidAmount;
            double _totalAmount = invoice.TotalFee - invoice.TotalDiscount + invoice.TotalOtherAmount;
            double _discount = invoice.TotalDiscount;
            double _otherAmount = invoice.TotalOtherAmount;
            int _index = 0;
            int _count = invoice.InvoiceDetails.Count - 1;
            invoice.InvoiceDetails.OrderBy(x => x.PayOrder).ToList().ForEach(x =>
            {
                if (_index == 0)
                {
                    if ((x.Fee + _otherAmount) > _discount)
                    {
                        x.PaidAmount = (_totalPaidAmount - (x.Fee + _otherAmount - _discount)) <= 0 ? _totalPaidAmount : (x.Fee + _otherAmount - _discount);
                        x.Fee = x.Fee + _otherAmount - _discount;
                        _totalPaidAmount = _totalPaidAmount - x.PaidAmount;
                        _discount = 0;
                    }
                    else
                    {
                        _discount = _discount - (x.Fee + _otherAmount);
                        x.PaidAmount = 0;
                        x.Fee = 0;
                    }
                }
                else
                {
                    if (_discount < x.Fee || _discount == 0)
                    {
                        x.PaidAmount = (_totalPaidAmount - (x.Fee - _discount)) <= 0 ? _totalPaidAmount : (x.Fee - _discount);
                        x.Fee = x.Fee - _discount;
                        _totalPaidAmount = _totalPaidAmount - (x.PaidAmount);
                        _discount = 0;
                    }
                    else
                    {
                        _discount = _discount - x.Fee;
                        x.PaidAmount = x.Fee = 0;
                    }
                }
                _index++;
            });
            if (_totalPaidAmount > 0)
            {
                invoice.ExtraPaidAmount = _totalPaidAmount;
            }
            else
            {
                invoice.ExtraPaidAmount = 0;
            }
            return true;
        }

        [NonAction]
        private bool GenerateInvoice(ref InvoiceModel invoice)
        {
            var _transInvoice = new Transaction_Invoice()
            {
                FK_VenueId = currentUser.CurrentVenueId,
                FK_PlayerId = invoice.PlayerId,
                FK_StatusId = 3,
                InvoiceDate = invoice.InvoiceDate.HasValue ? invoice.InvoiceDate.Value.ToLocalTime() : DateTime.Now.ToLocalTime(),
                InvoiceNumber = GenerateInvoiceNo(),
                DueDate = invoice.InvoiceDate.HasValue ? invoice.InvoiceDate.Value.AddDays(10).ToLocalTime() : DateTime.Now.AddDays(10).ToLocalTime(),
                TotalFee = (decimal)invoice.TotalFee,
                TotalDiscount = (decimal)invoice.TotalDiscount,
                OtherAmount = (decimal)invoice.TotalOtherAmount,
                PaidAmount = (decimal)invoice.TotalPaidAmount,
                Comments = invoice.Comments,
                CreatedBy = currentUser.UserId,
                CreatedDate = DateTime.Now.ToLocalTime()
            };

            dbContext.Transaction_Invoice.Add(_transInvoice);
            dbContext.SaveChanges();
            invoice.InvoiceId = _transInvoice.PK_InvoiceId;
            return true;
        }

        [NonAction]
        private bool UpdateStatusInvoice(InvoiceModel invoice)
        {

            var _transInvoice = dbContext.Transaction_Invoice.Include(x => x.Transaction_InvoiceDetail).Where(x => x.PK_InvoiceId == invoice.InvoiceId).First();
            if (_transInvoice != null)
            {
                _transInvoice.FK_StatusId = _transInvoice.Transaction_InvoiceDetail.All(x => x.FK_StatusId == 4) ? 4 : 3;
                _transInvoice.ModifiedBy = currentUser.UserId;
                _transInvoice.ModifiedDate = DateTime.Now.ToLocalTime();
                dbContext.Entry(_transInvoice).State = EntityState.Modified; return true;
            }
            return false;
        }

        [NonAction]
        private bool GenerateInvoiceDetail(InvoiceDetailModel detail, bool isUpdate)
        {
            if (!isUpdate)
            {
                dbContext.Transaction_InvoiceDetail.Add(new Transaction_InvoiceDetail()
                {
                    FK_BatchId = detail.BatchId,
                    FK_InvoiceId = detail.InvoiceId,
                    FK_StatusId = detail.StatusId,
                    BatchAmount = (decimal)detail.Fee,
                    InvoicePeriod = detail.InvoicePeriod,
                    PaidAmount = (decimal)detail.PaidAmount,
                    Comments = detail.Comments,
                    CreatedBy = currentUser.UserId,
                    CreatedDate = DateTime.Now.ToLocalTime()
                });
            }
            if (detail.StatusId == 4)
            {
                var _details = dbContext.Transaction_InvoiceDetail.Where(d => d.FK_StatusId == 3 && d.InvoicePeriod == detail.InvoicePeriod && dbContext.Transaction_Invoice.Any(i => i.FK_StatusId == 3 && i.PK_InvoiceId == d.FK_InvoiceId && i.FK_PlayerId == detail.PlayerId));
                var _result = _details.ToList();
                _result.ForEach(d =>
                {
                    //CLOSE THE INVOICE DETAIL IF PAID
                    d.Comments += detail.Comments;
                    d.FK_StatusId = detail.StatusId;
                    d.ModifiedBy = currentUser.UserId;
                    d.ModifiedDate = DateTime.Now.ToLocalTime();
                    dbContext.Entry(d).State = EntityState.Modified;

                });
            }
            return true;
        }

        [NonAction]
        private bool GenerateReceipt(InvoiceModel invoice)
        {
            var _playerid = invoice.PlayerId;
            var _invoice = dbContext.Transaction_Invoice.Find(invoice.InvoiceId);
            var _player = dbContext.Master_Player.Where(p => p.PK_PlayerId == _playerid && p.FK_StatusId == 1).FirstOrDefault();
            dbContext.Transaction_Receipt.Add(new Transaction_Receipt()
            {
                PK_ReceiptId = 0,
                ReceiptNumber = GenerateInvoiceNo(),
                ReceiptDate = invoice.InvoiceDate.HasValue ? invoice.InvoiceDate.Value.ToLocalTime() : DateTime.Now.ToLocalTime(),
                TotalFee = _invoice.TotalFee - (_player.CreditBalance.HasValue ? _player.CreditBalance.Value : 0),
                TotalOtherAmount = _invoice.OtherAmount,
                TotalDiscountAmount = _invoice.TotalDiscount,
                AmountPaid = (decimal)invoice.TotalPaidAmount,
                TransactionDate = invoice.TransactionDate,
                TransactionNumber = invoice.TransactionNo,
                ReceivedBy = invoice.ReceivedBy,
                FK_InvoiceId = _invoice.PK_InvoiceId,
                FK_PaymentModeId = invoice.PaymentId,
                FK_StatusId = 1,
                FK_VenueId = currentUser.CurrentVenueId,
                Description = invoice.Comments,
                CreatedBy = currentUser.UserId,
                CreatedDate = DateTime.Now.ToLocalTime(),
                CreditAmount = (_player.CreditBalance.HasValue ? _player.CreditBalance.Value : 0)
            });
            dbContext.SaveChanges();
            if (invoice.ExtraPaidAmount != (double)(_player.CreditBalance.HasValue ? _player.CreditBalance.Value : 0))
            {
                _player.CreditBalance = (decimal)invoice.ExtraPaidAmount;
                _player.ModifiedBy = currentUser.UserId;
                _player.ModifiedDate = DateTime.Now.ToLocalTime();
                dbContext.Entry(_player).State = EntityState.Modified;
            }
            return true;
        }

        [NonAction]
        private bool UpdateLastInvGenerated(InvoiceModel invoiceModel)
        {
            var _receipt = dbContext.Transaction_Receipt.FirstOrDefault(x => x.FK_InvoiceId == invoiceModel.InvoiceId);
            if (_receipt != null)
            {
                var _selectedbatchIds = invoiceModel.InvoiceDetails.Select(x => x.BatchId).Distinct();
                foreach (var batch in _selectedbatchIds)
                {
                    if (invoiceModel.InvoiceDetails.Any(x => x.BatchId == batch && x.StatusId == 4))
                    {
                        var _payorder = invoiceModel.InvoiceDetails.Where(x => x.BatchId == batch && x.StatusId == 4).Max(xs => xs.PayOrder);
                        if (_payorder != 0)
                        {
                            var _detail = invoiceModel.InvoiceDetails.FirstOrDefault(x => x.BatchId == batch && x.PayOrder == _payorder && x.StatusId == 4);
                            if (_detail != null)
                            {
                                var _playersport = dbContext.Transaction_PlayerSport.FirstOrDefault(s => s.FK_PlayerId == invoiceModel.PlayerId && s.FK_BatchId == batch && s.FK_StatusId == 1);
                                if (_playersport != null)
                                {
                                    _playersport.LastGeneratedMonth = _detail.InvoicePeriod.IndexOf('-') > 0 ? _detail.InvoicePeriod.Split('-')[1].ToString().Trim() : _detail.InvoicePeriod;
                                    _playersport.ModifiedBy = currentUser.UserId;
                                    _playersport.ModifiedDate = DateTime.Now.ToLocalTime();
                                    dbContext.Entry(_playersport).State = EntityState.Modified;
                                }
                                _detail.PlayerId = invoiceModel.PlayerId;
                                GenerateInvoiceDetail(_detail, true);
                                dbContext.SaveChanges();
                            }
                        }
                    }
                }
            }
            return true;
        }
        [NonAction]
        public void UpdateLastInvGeneratedClose(int playerid, int BatchId, string InvoicePeriod)
        {
            var _playersport = dbContext.Transaction_PlayerSport.Where(s => s.FK_PlayerId == playerid && s.FK_BatchId == BatchId && s.FK_StatusId == 1).FirstOrDefault();
            if (_playersport != null)
            {
                _playersport.LastGeneratedMonth = InvoicePeriod.IndexOf('-') > 0 ? InvoicePeriod.Split('-')[1].ToString().Trim() : InvoicePeriod;
                _playersport.ModifiedBy = currentUser.UserId;
                _playersport.ModifiedDate = DateTime.Now.ToLocalTime();
                dbContext.Entry(_playersport).State = EntityState.Modified;
            }
            dbContext.SaveChanges();
        }
        #endregion [ NonAction Methods ]


        #region
        //[NonAction]
        //public ActionResult Save(InvoiceModel invoice)
        //{
        //    var _guestId = dbContext.Master_Venue.Find(currentUser.CurrentVenueId).GuestPlayerId;
        //    if (_guestId == invoice.PlayerId && invoice.TotalFee + invoice.TotalOtherAmount - invoice.TotalDiscount != invoice.TotalPaidAmount)
        //    {
        //        return Json("Paid amount Should be equal to the total amount!", JsonRequestBehavior.AllowGet);
        //    }
        //    if (invoice.InvoiceDetails.Count > 0)
        //    {
        //        if (_guestId != invoice.PlayerId)
        //        {
        //            InvoiceModel allinvoice = new InvoiceModel();
        //            allinvoice.InvoiceDetails = new List<InvoiceDetailModel>();
        //            var batchdetails = dbContext.Transaction_PlayerSport.Include(c => c.Master_Sport).Include(c => c.Master_Batch).Where(x => x.FK_StatusId == 1 && x.FK_PlayerId == invoice.PlayerId);
        //            GetDueList(ref allinvoice, batchdetails);
        //            //Calculate the Total Fee
        //            invoice.TotalFee = invoice.InvoiceDetails.Sum(x => x.Fee);
        //            // Calculate the Total Extra Amount Paid 
        //            invoice.ExtraPaidAmount = (invoice.TotalFee + invoice.TotalOtherAmount - invoice.TotalDiscount) >= invoice.TotalPaidAmount ? 0 : invoice.TotalPaidAmount - (invoice.TotalFee + invoice.TotalOtherAmount - invoice.TotalDiscount);
        //            //Get all the Month to be closed
        //            var ClosedInv = GetMonthToBeClosed(invoice);
        //            //Close all the Month Automatically
        //            if (ClosedInv != null && ClosedInv.Count > 0)
        //            {
        //                Close(new InvoiceModel
        //                {
        //                    VenueId = currentUser.CurrentVenueId,
        //                    PlayerId = invoice.PlayerId,
        //                    Comments = "CLOSED AUTOMATICALLY BY THE SYSTEM!",
        //                    TotalFee = ClosedInv.Sum(x => x.Fee),
        //                    TotalDiscount = 0,
        //                    TotalOtherAmount = 0,
        //                    TotalPaidAmount = 0,
        //                    InvoiceDetails = ClosedInv
        //                });
        //            }
        //        }
        //        //Split the Amount in the Invoice Detail
        //        SplitTheAmountInDetail(ref invoice, invoice.TotalPaidAmount, invoice.TotalDiscount, invoice.TotalOtherAmount);
        //        //Save the Invoice Header
        //        var _transInvoice = new Transaction_Invoice()
        //        {
        //            FK_VenueId = currentUser.CurrentVenueId,
        //            FK_PlayerId = invoice.PlayerId,
        //            FK_StatusId = ((invoice.TotalFee + invoice.TotalOtherAmount - invoice.TotalDiscount) <= invoice.TotalPaidAmount) ? 4 : 3,
        //            InvoiceDate = invoice.InvoiceDate.HasValue ? invoice.InvoiceDate.Value.ToLocalTime() : DateTime.Now.ToLocalTime(),
        //            InvoiceNumber = GenerateInvoiceNo(),
        //            DueDate = invoice.InvoiceDate.HasValue ? invoice.InvoiceDate.Value.AddDays(10).ToLocalTime() : DateTime.Now.AddDays(10).ToLocalTime(),
        //            TotalFee = (decimal)invoice.TotalFee,
        //            TotalDiscount = (decimal)invoice.TotalDiscount,
        //            OtherAmount = (decimal)invoice.TotalOtherAmount,
        //            PaidAmount = (decimal)invoice.TotalPaidAmount,
        //            Comments = invoice.Comments,
        //            CreatedBy = currentUser.UserId,
        //            CreatedDate = DateTime.Now.ToLocalTime()
        //        };
        //        dbContext.Transaction_Invoice.Add(_transInvoice);
        //        dbContext.SaveChanges();
        //        invoice.InvoiceId = _transInvoice.PK_InvoiceId;
        //        //Save the Invoice Details
        //        invoice.InvoiceDetails.ForEach(d =>
        //        {
        //            SaveDetail(_transInvoice.PK_InvoiceId, invoice.PlayerId, invoice.Comments, _transInvoice.FK_StatusId, d);
        //        });
        //        dbContext.SaveChanges();

        //        //Update the Last Generated Inv Month
        //        invoice.InvoiceDetails.ForEach(d =>
        //        {
        //            if (d.Fee == d.PaidAmount)
        //            {
        //                UpdateLastInvGenerated(_transInvoice.PK_InvoiceId, invoice.PlayerId, d.BatchId, d.InvoicePeriod);
        //            }
        //        });
        //        //CLOSE THE INVOICE
        //        ChangeStatus(invoice.PlayerId, invoice.InvoiceId);
        //        //Generate Receipt
        //        var _invoice = dbContext.Transaction_Invoice.Find(_transInvoice.PK_InvoiceId);
        //        dbContext.Transaction_Receipt.Add(new Transaction_Receipt()
        //        {
        //            PK_ReceiptId = 0,
        //            ReceiptNumber = GenerateInvoiceNo(),
        //            ReceiptDate = invoice.InvoiceDate.HasValue ? invoice.InvoiceDate.Value.ToLocalTime() : DateTime.Now.ToLocalTime(),
        //            TotalFee = _invoice.TotalFee,
        //            TotalOtherAmount = _invoice.OtherAmount,
        //            TotalDiscountAmount = _invoice.TotalDiscount,
        //            AmountPaid = (decimal)invoice.TotalPaidAmount,
        //            TransactionDate = invoice.TransactionDate,
        //            TransactionNumber = invoice.TransactionNo,
        //            ReceivedBy = invoice.ReceivedBy,
        //            FK_InvoiceId = _invoice.PK_InvoiceId,
        //            FK_PaymentModeId = invoice.PaymentId,
        //            FK_StatusId = 1,
        //            FK_VenueId = currentUser.CurrentVenueId,
        //            Description = invoice.Comments,
        //            CreatedBy = currentUser.UserId,
        //            CreatedDate = DateTime.Now.ToLocalTime()
        //        });
        //        if (invoice.ExtraPaidAmount > 0)
        //        {
        //            var _player = dbContext.Master_Player.Where(p => p.PK_PlayerId == invoice.PlayerId && p.FK_StatusId == 1).FirstOrDefault();
        //            if (_player != null)
        //            {
        //                _player.CreditBalance = (decimal)invoice.ExtraPaidAmount;
        //                dbContext.Entry(_player).State = EntityState.Modified;
        //            }
        //        }
        //        dbContext.SaveChanges();
        //        return Json(true, JsonRequestBehavior.AllowGet);
        //    }
        //    return Json("Nothing to save", JsonRequestBehavior.AllowGet);
        //}

        //[NonAction]
        //private List<InvoiceDetailModel> GetMonthToBeClosed(InvoiceModel invoice)
        //{
        //    List<InvoiceDetailModel> toClosedinvoiceDetailModels = new List<InvoiceDetailModel>();
        //    var _selectedInvoiceDetails = invoice.InvoiceDetails.OrderBy(x => x.BatchId).ThenBy(x => x.PayOrder);
        //    var _selectedbatchIds = _selectedInvoiceDetails.Select(x => x.BatchId).Distinct();
        //    var _actualInvoiceDetails = GetInvoiceDetailList(invoice.PlayerId).InvoiceDetails.Where(x => _selectedbatchIds.Contains(x.BatchId)).Select(x => new InvoiceDetailModel
        //    {
        //        BatchId = x.BatchId,
        //        Fee = x.Fee,
        //        InvoicePeriod = x.InvoicePeriod,
        //        InvoicePeriodId = x.InvoicePeriodId,
        //        PayOrder = x.PayOrder,
        //    }).OrderBy(x => x.BatchId).ThenBy(x => x.PayOrder);

        //    var groupedByBatchId = _selectedInvoiceDetails.GroupBy(c => c.BatchId);
        //    foreach (var batch in groupedByBatchId)
        //    {
        //        int maxpayOrder = batch.Max(g => g.PayOrder);
        //        int _batchId = batch.Key;

        //        var _result = _actualInvoiceDetails.Where(x => x.BatchId == _batchId && x.PayOrder < maxpayOrder).ToList().Where(x => !_selectedInvoiceDetails.Any(s => s.InvoicePeriodId == x.InvoicePeriodId && s.PayOrder == x.PayOrder && s.BatchId == _batchId)).ToList();
        //        if (_result != null && _result.Count > 0)
        //        {
        //            toClosedinvoiceDetailModels.AddRange(_result);
        //        }
        //    }
        //    return toClosedinvoiceDetailModels;
        //}
        //[NonAction]
        //private void SplitTheAmountInDetail(ref InvoiceModel invoice, double totalAmount, double discount, double otherAmount)
        //{
        //    double _totalPaidAmount = totalAmount - invoice.TotalOtherAmount + invoice.TotalDiscount;
        //    invoice.InvoiceDetails.OrderBy(x => x.PayOrder).ToList().ForEach(x =>
        //    {
        //        if (_totalPaidAmount > 0 && _totalPaidAmount >= x.Fee)
        //        {
        //            x.PaidAmount = x.Fee;
        //            _totalPaidAmount = _totalPaidAmount - x.Fee;
        //        }
        //        else if (_totalPaidAmount > 0 && _totalPaidAmount < x.Fee)
        //        {
        //            x.PaidAmount = _totalPaidAmount;
        //            _totalPaidAmount = 0;
        //        }

        //    });
        //}
        //[NonAction]
        //public void Close(InvoiceModel invoice)
        //{
        //    if (invoice.InvoiceId == 0)
        //    {
        //        var _transInvoice = new Transaction_Invoice()
        //        {
        //            FK_VenueId = invoice.VenueId,
        //            FK_PlayerId = invoice.PlayerId,
        //            FK_StatusId = 4,
        //            InvoiceDate = invoice.InvoiceDate.HasValue ? invoice.InvoiceDate.Value.ToLocalTime() : DateTime.Now.ToLocalTime(),
        //            InvoiceNumber = GenerateInvoiceNo(),
        //            DueDate = invoice.InvoiceDate.HasValue ? invoice.InvoiceDate.Value.AddDays(10).ToLocalTime() : DateTime.Now.AddDays(10).ToLocalTime(),
        //            TotalFee = 0,
        //            TotalDiscount = 0,
        //            OtherAmount = 0,
        //            PaidAmount = 0,
        //            Comments = invoice.Comments,
        //            CreatedBy = currentUser.UserId,
        //            CreatedDate = DateTime.Now.ToLocalTime()
        //        };
        //        dbContext.Transaction_Invoice.Add(_transInvoice);
        //        dbContext.SaveChanges();
        //        invoice.InvoiceId = _transInvoice.PK_InvoiceId;
        //        invoice.InvoiceDetails.ForEach(d =>
        //        {
        //            SaveDetail(_transInvoice.PK_InvoiceId, invoice.PlayerId, invoice.Comments, 4, d);
        //        });
        //        dbContext.SaveChanges();
        //        invoice.InvoiceDetails.ForEach(d =>
        //        {
        //            UpdateLastInvGenerated(_transInvoice.PK_InvoiceId, invoice.PlayerId, d.BatchId, d.InvoicePeriod);
        //        });

        //    }
        //    else
        //    {
        //        invoice.InvoiceDetails.ForEach(d =>
        //        {
        //            var _details = dbContext.Transaction_InvoiceDetail.Find(d.InvoiceDetailssId);
        //            if (_details != null)
        //            {
        //                _details.Comments = invoice.Comments;
        //                _details.FK_StatusId = 4;
        //                _details.ModifiedBy = currentUser.UserId;
        //                _details.ModifiedDate = DateTime.Now.ToLocalTime();
        //                dbContext.Entry(_details).State = EntityState.Modified;
        //            }
        //            else
        //            {
        //                SaveDetail(invoice.InvoiceId, invoice.PlayerId, invoice.Comments, 4, d);
        //            }
        //        });
        //        dbContext.SaveChanges();
        //        if (dbContext.Transaction_InvoiceDetail.All(i => i.FK_InvoiceId == invoice.InvoiceId && (i.FK_StatusId.Equals(4) || i.FK_StatusId.Equals(2))))
        //        {
        //            var _invupdate = dbContext.Transaction_Invoice.Find(invoice.InvoiceId);
        //            if (_invupdate != null)
        //            {
        //                _invupdate.FK_StatusId = 4;
        //                dbContext.Entry(_invupdate).State = EntityState.Modified;
        //                dbContext.SaveChanges();

        //            }
        //        }
        //        invoice.InvoiceDetails.ForEach(d =>
        //        {
        //            UpdateLastInvGenerated(invoice.InvoiceId, invoice.PlayerId, d.BatchId, d.InvoicePeriod);
        //        });
        //    }
        //    if (invoice.InvoiceId != 0)
        //    {
        //        if (dbContext.Transaction_InvoiceDetail.All(d => d.FK_InvoiceId == invoice.InvoiceId && d.FK_StatusId == 4))
        //        {
        //            var _invoice = dbContext.Transaction_Invoice.Find(invoice.InvoiceId);
        //            _invoice.FK_StatusId = 4;
        //            _invoice.ModifiedBy = currentUser.UserId;
        //            _invoice.ModifiedDate = DateTime.Now.ToLocalTime();
        //            dbContext.Entry(_invoice).State = EntityState.Modified;
        //            dbContext.SaveChanges();
        //        }
        //    }
        //    dbContext.SaveChanges();
        //}
        //[NonAction]
        //public void UpdateLastInvGenerated(int invoiceid, int playerid, int BatchId, string InvoicePeriod)
        //{
        //    var _playersport = dbContext.Transaction_PlayerSport.Where(s => s.FK_PlayerId == playerid && s.FK_BatchId == BatchId).FirstOrDefault();
        //    if (_playersport != null)
        //    {
        //        _playersport.LastGeneratedMonth = InvoicePeriod.IndexOf('-') > 0 ? InvoicePeriod.Split('-')[1].ToString().Trim() : InvoicePeriod;
        //        _playersport.ModifiedBy = currentUser.UserId;
        //        _playersport.ModifiedDate = DateTime.Now.ToLocalTime();
        //        dbContext.Entry(_playersport).State = EntityState.Modified;
        //    }
        //    dbContext.SaveChanges();
        //}
        #endregion
    }
}