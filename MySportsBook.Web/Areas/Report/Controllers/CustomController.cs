using MySportsBook.Web.Controllers;
using MySportsBook.Web.Filters;
using MySportsBook.Web.Helper;
using System.Collections.Generic;
using System.Data;
using System.Web.Mvc;

namespace MySportsBook.Web.Areas.Report.Controllers
{
    [UserAuthentication]
    public class CustomController : BaseController
    {
        // GET: Report/Custom
        public ActionResult Index()
        {
            GetReportType("");
            return View();
        }

        public ActionResult Defaulters()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Defaulters(string Month)
        {
            return View(CalltheCustomStoreProc(currentUser.CurrentVenueId, "Defaulter", Month));
        }

        [HttpPost]
        public ActionResult Index(string ReportType, string Month)
        {
            GetReportType(ReportType);
            return View(CalltheCustomStoreProc(currentUser.CurrentVenueId, ReportType, Month));
        }

        private void GetReportType(string ReportType)
        {
            List<SelectListItem> ddlList = new List<SelectListItem>();
            ddlList.Add(new SelectListItem
            {
                Text = "--Select--",
                Value = ""
            });
            ConnectionDataControl clsDataControl = new ConnectionDataControl();
            DataTable collectionDataTable = new DataTable();
            clsDataControl.IsSp = false;
            clsDataControl.DynamicParameters.Clear();

            collectionDataTable = clsDataControl.GetDetails("select ReportName,DisplayName from ReportType order by OrderBy Asc", false);

            if (collectionDataTable != null && collectionDataTable.Rows.Count > 0)
            {
                foreach (DataRow row in collectionDataTable.Rows)
                {
                    ddlList.Add(new SelectListItem
                    {
                        Text = ConvertHelper.ConvertToString(row["DisplayName"], string.Empty),
                        Value = ConvertHelper.ConvertToString(row["ReportName"], string.Empty),
                    });
                }
            }


            ViewBag.ReportType = new SelectList(ddlList, "Value", "Text", ReportType);
        }

        [NonAction]
        private DataTable CalltheCustomStoreProc(int venueID, string storeProc, string parameters)
        {
            if (currentUser.UserId < 4 || storeProc == "Defaulter")
                return dbContext.GetResultReport(venueID, storeProc, parameters);
            return new DataTable();
        }

       
    }
}