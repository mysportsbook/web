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

        [HttpGet]
        public ActionResult GetInvoiceList(int id)
        {
            InvoiceModel invoiceModel = GetInvoiceDetailList(id);
            ViewBag.PaymentMode = new SelectList(dbContext.Confirguration_PaymentMode, "PK_PaymentModeId", "PaymentMode");
            ViewBag.ReceivedBy = new SelectList(dbContext.Configuration_User.Where(x => dbContext.Master_UserVenue.Any(v => v.FK_UserId == x.PK_UserId && v.FK_VenueId == currentUser.CurrentVenueId) && x.PK_UserId != 0), "PK_UserId", "UserName", currentUser.UserId.ToString());
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
                //Calculate the Total Fee
                invoice.TotalFee = invoice.InvoiceDetails.Sum(x => x.Fee);
                // Calculate the Total Extra Amount Paid 
                invoice.ExtraPaidAmount = invoice.TotalFee + invoice.TotalOtherAmount - invoice.TotalDiscount <= invoice.TotalPaidAmount ? 0 : invoice.TotalPaidAmount - invoice.TotalFee + invoice.TotalOtherAmount - invoice.TotalDiscount;
                //Get all the Month to be closed
                var ClosedInv = GetMonthToBeClosed(invoice);
                //Split the Amount in the Invoice Detail
                SplitTheAmountInDetail(ref invoice, invoice.TotalPaidAmount, invoice.TotalDiscount, invoice.TotalOtherAmount);
                //Close all the Month Automatically
                if (ClosedInv != null && ClosedInv.Count > 0)
                {
                    Close(new InvoiceModel
                    {
                        VenueId = currentUser.CurrentVenueId,
                        PlayerId = invoice.PlayerId,
                        Comments = "CLOSED AUTOMATICALLY BY THE SYSTEM!",
                        TotalFee = ClosedInv.Sum(x => x.Fee),
                        TotalDiscount = 0,
                        TotalOtherAmount = 0,
                        TotalPaidAmount = 0,
                        InvoiceDetails = ClosedInv
                    });
                }
                //Save the Invoice Header
                var _transInvoice = new Transaction_Invoice()
                {
                    FK_VenueId = currentUser.CurrentVenueId,
                    FK_PlayerId = invoice.PlayerId,
                    FK_StatusId = ((invoice.TotalFee + invoice.TotalOtherAmount - invoice.TotalDiscount) <= invoice.TotalPaidAmount) ? 4 : 3,
                    InvoiceDate = DateTime.Now.ToUniversalTime(),
                    InvoiceNumber = GenerateInvoiceNo(),
                    DueDate = DateTime.Now.AddDays(10).ToUniversalTime(),
                    TotalFee = (decimal)invoice.TotalFee,
                    TotalDiscount = (decimal)invoice.TotalDiscount,
                    OtherAmount = (decimal)invoice.TotalOtherAmount,
                    PaidAmount = (decimal)invoice.TotalPaidAmount,
                    Comments = invoice.Comments,
                    CreatedBy = currentUser.UserId,
                    CreatedDate = DateTime.Now.ToUniversalTime()
                };
                dbContext.Transaction_Invoice.Add(_transInvoice);
                dbContext.SaveChanges();
                invoice.InvoiceId = _transInvoice.PK_InvoiceId;
                //Save the Invoice Details
                invoice.InvoiceDetails.ForEach(d =>
                {
                    SaveDetail(_transInvoice.PK_InvoiceId, invoice.PlayerId, invoice.Comments, _transInvoice.FK_StatusId, d);
                });
                dbContext.SaveChanges();

                //Update the Last Generated Inv Month
                invoice.InvoiceDetails.ForEach(d =>
                {
                    if (d.Fee == d.PaidAmount)
                    {
                        UpdateLastInvGenerated(_transInvoice.PK_InvoiceId, invoice.PlayerId, d.BatchId, d.InvoicePeriod);
                    }
                });
                //CLOSE THE INVOICE
                ChangeStatus(invoice.PlayerId, invoice.InvoiceId);
                //Generate Receipt
                var _invoice = dbContext.Transaction_Invoice.Find(_transInvoice.PK_InvoiceId);
                dbContext.Transaction_Receipt.Add(new Transaction_Receipt()
                {
                    PK_ReceiptId = 0,
                    ReceiptNumber = GenerateInvoiceNo(),
                    ReceiptDate = DateTime.Now.ToUniversalTime(),
                    TotalFee = _invoice.TotalFee,
                    TotalOtherAmount = _invoice.OtherAmount,
                    TotalDiscountAmount = _invoice.TotalDiscount,
                    AmountPaid = (decimal)_invoice.PaidAmount,
                    TransactionDate = invoice.TransactionDate,
                    TransactionNumber = invoice.TransactionNo,
                    ReceivedBy = invoice.ReceivedBy,
                    FK_InvoiceId = _invoice.PK_InvoiceId,
                    FK_PaymentModeId = invoice.PaymentId,
                    FK_StatusId = 1,
                    FK_VenueId = currentUser.CurrentVenueId,
                    Description = invoice.Comments,
                    CreatedBy = currentUser.UserId,
                    CreatedDate = DateTime.Now.ToUniversalTime()
                });
                if (invoice.ExtraPaidAmount > 0)
                {
                    var _player = dbContext.Master_Player.Where(p => p.PK_PlayerId == invoice.PlayerId && p.FK_StatusId == 1).FirstOrDefault();
                    if (_player != null)
                    {
                        _player.CreditBalance = (decimal)invoice.ExtraPaidAmount;
                        dbContext.Entry(_player).State = EntityState.Modified;
                    }
                }
                dbContext.SaveChanges();
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
                    InvoiceDate = DateTime.Now.ToUniversalTime(),
                    InvoiceNumber = GenerateInvoiceNo(),
                    DueDate = DateTime.Now.ToUniversalTime(),
                    TotalFee = (decimal)invoice.TotalFee,
                    TotalDiscount = (decimal)invoice.TotalDiscount,
                    OtherAmount = (decimal)invoice.TotalOtherAmount,
                    PaidAmount = (decimal)invoice.TotalPaidAmount,
                    Comments = invoice.Comments,
                    CreatedBy = currentUser.UserId,
                    CreatedDate = DateTime.Now.ToUniversalTime()
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
            int _statusid = detail.Fee == detail.PaidAmount ? 4 : 3;
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
                CreatedDate = DateTime.Now.ToUniversalTime()
            });
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
                        d.ModifiedDate = DateTime.Now.ToUniversalTime();
                        dbContext.Entry(d).State = EntityState.Modified;

                    });
                }
            }
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
        [NonAction]
        InvoiceModel GetInvoiceDetailList(int playerId)
        {
            InvoiceModel invoiceModel = new InvoiceModel();
            invoiceModel.InvoiceDetails = new List<InvoiceDetailModel>();
            var batchdetails = dbContext.Transaction_PlayerSport.Include(c => c.Master_Sport).Include(c => c.Master_Batch).Where(x => x.FK_StatusId == 1 && x.FK_PlayerId == playerId);
            int _months = 0, _freq = 0, _totalmonths = 0, _sortorder = 0;
            string[] _listMonths = new string[] { };
            DateTime _date;
            batchdetails.ToList().ForEach(batch =>
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
                     .TakeWhile(e => e <= _date.AddMonths(1).AddMonths(_totalmonths).ToUniversalTime())
                     .Select(e => e.ToString("MMMyyyy").ToUpper()).ToArray();
                    for (int count = 0; count < _totalmonths; count += _freq)
                    {
                        invoiceModel.InvoiceDetails.Add(new InvoiceDetailModel()
                        {
                            BatchId = batch.FK_BatchId,
                            SportName = batch.Master_Sport.SportName,
                            Fee = (double)batch.Fee,
                            InvoicePeriod = (batch.FK_InvoicePeriodId == 1 ? _listMonths[count] : _listMonths[count] + "-" + _listMonths[count + (_freq - 1)]),
                            InvoicePeriodId = batch.FK_InvoicePeriodId,
                            PayOrder = _sortorder
                        });
                        _sortorder += 1;
                    }
                }

            });
            invoiceModel.PlayerId = playerId;
            var _CreditBalance = dbContext.Master_Player.Where(p => p.PK_PlayerId == playerId && p.FK_StatusId == 1).FirstOrDefault().CreditBalance;
            if (_CreditBalance > 0)
            {
                invoiceModel.InvoiceDetails.ForEach(x =>
                {
                    if (_CreditBalance > 0 && (double)_CreditBalance > x.Fee)
                    {
                        x.Fee = 0;
                        _CreditBalance -= (decimal)x.Fee;
                    }
                    else if (_CreditBalance > 0 && (double)_CreditBalance < x.Fee)
                    {
                        x.Fee -= (double)_CreditBalance;
                        _CreditBalance = 0;
                    }

                });
            }
            var _openInvoice = dbContext.Transaction_InvoiceDetail.Where(x => dbContext.Transaction_Invoice.Any(i => i.FK_PlayerId == playerId && i.FK_StatusId == 3 && i.PK_InvoiceId == x.FK_InvoiceId));
            if (_openInvoice != null)
            {
                invoiceModel.InvoiceDetails.ForEach(x =>
                {
                    if (_openInvoice.Any(o => o.InvoicePeriod == x.InvoicePeriod))
                    {
                        x.Fee -= (double)_openInvoice.Where(o => o.InvoicePeriod == x.InvoicePeriod).Sum(s => s.PaidAmount);
                    }
                });
            }
            return invoiceModel;
        }

        [NonAction]
        private List<InvoiceDetailModel> GetMonthToBeClosed(InvoiceModel invoice)
        {
            List<InvoiceDetailModel> toClosedinvoiceDetailModels = new List<InvoiceDetailModel>();
            var _selectedInvoiceDetails = invoice.InvoiceDetails.OrderBy(x => x.BatchId).ThenBy(x => x.PayOrder);
            var _selectedbatchIds = _selectedInvoiceDetails.Select(x => x.BatchId).Distinct();
            var _actualInvoiceDetails = GetInvoiceDetailList(invoice.PlayerId).InvoiceDetails.Where(x => _selectedbatchIds.Contains(x.BatchId)).Select(x => new InvoiceDetailModel
            {
                BatchId = x.BatchId,
                Fee = x.Fee,
                InvoicePeriod = x.InvoicePeriod,
                InvoicePeriodId = x.InvoicePeriodId,
                PayOrder = x.PayOrder
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
        private void SplitTheAmountInDetail(ref InvoiceModel invoice, double totalAmount, double discount, double otherAmount)
        {
            double _totalPaidAmount = totalAmount;
            int count = 0;
            invoice.InvoiceDetails.OrderByDescending(x => x.Fee).ToList().ForEach(x =>
            {
                if (count == 0)
                {
                    x.Fee += otherAmount - discount;
                    count += 1;
                }
                if (_totalPaidAmount > 0 && _totalPaidAmount >= x.Fee)
                {
                    x.PaidAmount = x.Fee;
                    _totalPaidAmount = _totalPaidAmount - x.Fee;
                }
                else if (_totalPaidAmount > 0 && _totalPaidAmount < x.Fee)
                {
                    x.PaidAmount = _totalPaidAmount;
                    _totalPaidAmount = 0;
                }

            });
            if (_totalPaidAmount > 0)
            {
                invoice.ExtraPaidAmount = _totalPaidAmount;
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
                              c.ModifiedDate = DateTime.Now.ToUniversalTime();
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
                            inv.ModifiedDate = DateTime.Now.ToUniversalTime();
                            dbContext.Entry(inv).State = EntityState.Modified;
                        }
                    });
                    dbContext.SaveChanges();
                }
            }
        }
        #endregion [ NonAction Methods ]
    }
}