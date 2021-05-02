using MySportsBook.Web.Areas.Transaction.Models;
using MySportsBook.Web.Controllers;
using MySportsBook.Web.Filters;
using MySportsBook.Web.Helper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MySportsBook.Web.Areas.Report.Controllers
{
    [UserAuthentication]
    public class AttendanceReportController : BaseController
    {
        // GET: Report/AttendanceReport
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(string FromDate, string ToDate)
        {
            List<AttendanceBatchViewModel> datas = new List<AttendanceBatchViewModel>() { };

            ConnectionDataControl clsDataControl = new ConnectionDataControl();
            DataTable collectionDataTable = new DataTable();
            clsDataControl.DynamicParameters.Clear();
            clsDataControl.DynamicParameters.Add("@VenueId", currentUser.CurrentVenueId);
            clsDataControl.DynamicParameters.Add("@FromDate", FromDate);
            clsDataControl.DynamicParameters.Add("@ToDate", ToDate);

            collectionDataTable = clsDataControl.GetDetails(argstrQuery: "GetAttendanceDetailByDate", IsParameter: true);

            if (collectionDataTable != null && collectionDataTable.Rows.Count > 0)
            {
                foreach (DataRow row in collectionDataTable.Rows)
                {
                    datas.Add(new AttendanceBatchViewModel()
                    {
                        FirstName = ConvertHelper.ConvertToString(row["FirstName"], string.Empty),
                        LastName = ConvertHelper.ConvertToString(row["LastName"], string.Empty),
                        Mobile = ConvertHelper.ConvertToString(row["PK_PlayerId"], string.Empty),
                        PK_PlayerId = ConvertHelper.ConvertToString(row["PK_PlayerId"], string.Empty),
                        AttendanceBatchCode = ConvertHelper.ConvertToString(row["AttendanceBatchCode"], string.Empty),
                        AttendanceBatchName = ConvertHelper.ConvertToString(row["AttendanceBatchName"], string.Empty),
                        BatchName = ConvertHelper.ConvertToString(row["BatchName"]),
                        FK_BatchId = ConvertHelper.ConvertToString(row["FK_BatchId"]),
                        Date = ConvertHelper.ConvertToString(row["AttendanceDate"]),
                        Fees = ConvertHelper.ConvertToString(row["Fee"])
                    });
                }
            }
            return View(datas);
        }


    }
}