using MySportsBook.Model.ViewModel;
using MySportsBook.Web.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MySportsBook.Web.Areas.Report.Controllers
{
    public class InvoiceController : BaseController
    {
        // GET: Report/Invoice
        public ActionResult Index()
        {
            List<InvoiceReportModel> _invoiceReportModel = new List<InvoiceReportModel>();
            return View(_invoiceReportModel);
        }

        [HttpPost]
        public ActionResult Index(InvoiceReportSearchModel _invoiceReportSearchModel)
        {
            //DateTime _startDate = Convert.ToDateTime(StartDate);
            //DateTime _endDate = Convert.ToDateTime(EndDate);
            List<InvoiceReportModel> _invoiceReportModel = new List<InvoiceReportModel>();

            //var _Invoices = dbContext.Transaction_Invoice.Where(x => (x.InvoiceDate >= _invoiceReportSearchModel.StartDate) && (x.InvoiceDate <= _invoiceReportSearchModel.EndDate) && (x.PaidAmount==(_invoiceReportSearchModel.Collected == true? x.TotalFee+x.TotalDiscount: x.PaidAmount)) && (x.TotalFee + x.TotalDiscount != (_invoiceReportSearchModel.Pending == true? x.PaidAmount : x.TotalDiscount)) && x.FK_VenueId == currentUser.CurrentVenueId).ToList();
            //var _Invoices = dbContext.Transaction_Invoice.Where(x => (x.InvoiceDate >= _invoiceReportSearchModel.StartDate) && (x.InvoiceDate <= _invoiceReportSearchModel.EndDate) && x.FK_VenueId == currentUser.CurrentVenueId).ToList();
            //_Invoices.ForEach(invoice => {
            //var _PlayerSport = dbContext.Transaction_PlayerSport.Where(x => x.FK_PlayerId == invoice.FK_PlayerId).FirstOrDefault();
            //var player = dbContext.Master_Player.Where(x => x.PK_PlayerId == invoice.FK_PlayerId).FirstOrDefault();
            //        _invoiceReportModel.Add(new InvoiceReportModel()
            //        {
            //            Amount = invoice.TotalFee,
            //            Name = player.FirstName + " " + player.LastName,
            //            Mobile = player.Mobile,
            //            Status = invoice.TotalFee == invoice.PaidAmount + invoice.TotalDiscount ? "Collected" : "Pending",
            //            Batch = dbContext.Master_Batch.Where(x => x.PK_BatchId == _PlayerSport.FK_BatchId).Select(x => x.BatchName).SingleOrDefault()
            //        });
            //    });
           
            return View(_invoiceReportModel);
        }

    }
}