using MySportsBook.Model;
using MySportsBook.Web.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MySportsBook.Web.Areas.Report.Controllers
{
    public class EnquiryController : BaseController
    {
        // GET: Report/Enquiry
        public ActionResult Index()
        {
            List<Master_Enquiry> _master_Enquiry = dbContext.Master_Enquiry.Where(x => x.FK_VenueId == currentUser.CurrentVenueId).OrderByDescending(x => x.CreatedDate).ToList();
            return View(_master_Enquiry);
        }

        [HttpGet]
        public ActionResult GetEnquiryComments(int id)
        {
            List<Transaction_Enquiry_Comments> _transaction_Enquiry_Comments = (dbContext.Transaction_Enquiry_Comments.Where(x => x.FK_EnquiryId == id)).ToList();
            return PartialView("_enquiryComments", _transaction_Enquiry_Comments);
        }
    }
}