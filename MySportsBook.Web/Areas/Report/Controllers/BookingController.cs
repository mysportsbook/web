using MySportsBook.Model.ViewModel;
using MySportsBook.Web.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MySportsBook.Web.Areas.Report.Controllers
{
    public class BookingController : BaseController
    {
        // GET: Report/Booking
        public ActionResult Index()
        {
            List<BookingModel> _bookingmodel = new List<BookingModel>();
            var otherBookings = dbContext.OtherBookings.Where(x => x.FK_VenueId == currentUser.CurrentVenueId).OrderByDescending(x => x.CreatedDate);
            otherBookings.ToList().ForEach(p =>
            {
            var _bookingDetail = dbContext.OtherBookingDetails.Where(s => s.FK_BookingId == p.PK_BookingId && s.FK_VenueId == currentUser.CurrentVenueId).Select(x => new { x.Court,x.Slot }).ToList();
                _bookingmodel.Add(new BookingModel
                {
                     Name = p.Name, Amount=p.Amount, BookingID=p.PK_BookingId, BookingNo=p.BookingNo,
                    Slots = string.Join(", ", _bookingDetail.Select(x=>x.Slot)),
                    Court = string.Join(", ", _bookingDetail.Select(x => x.Court))
                });
            });
            return View(_bookingmodel);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                dbContext.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}