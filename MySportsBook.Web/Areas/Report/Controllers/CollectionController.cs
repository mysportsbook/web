using MySportsBook.Model;
using MySportsBook.Web.Controllers;
using MySportsBook.Web.Filters;
using MySportsBook.Web.Helper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Web.Mvc;

namespace MySportsBook.Web.Areas.Report.Controllers
{
    [UserAuthentication]
    public class CollectionController : BaseController
    {
        // GET: Report/Collection
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Daily()
        {
            List<rp_COLLECTIONDETAIL_Result> result = GetCollection(null, "DAILY");
            return View(result);
        }
        [HttpPost]
        public ActionResult Daily(string Date)
        {
            var _date = DateTime.ParseExact((string.IsNullOrEmpty(Date) ? DateTime.Now.ToString("dd/MM/yyyy") : Date), "dd/MM/yyyy", null);
            return View(GetCollection(_date, "DAILY"));
        }

        public ActionResult Player()
        {
            return View(GetCollection(null, "PLAYER"));
        }

        public ActionResult Monthly()
        {
            return View(GetCollection(null, "MONTHLY"));
        }

        public ActionResult Split()
        {
            return View(GetCollection(null, "SPLIT"));
        }

        [HttpPost]
        public ActionResult Split(DateTime? Month)
        {
            return View(GetCollection(Month, "SPLIT"));
        }

        [HttpPost]
        public ActionResult Player(DateTime? Month)
        {
            return View(GetCollection(Month, "PLAYER"));
        }

        [HttpPost]
        public ActionResult Monthly(DateTime? Month)
        {
            return View(GetCollection(Month, "PLAYER"));
        }

        [NonAction]
        private List<rp_COLLECTIONDETAIL_Result> GetCollection(DateTime? month, string Type)
        {
            List<rp_COLLECTIONDETAIL_Result> datas = new List<rp_COLLECTIONDETAIL_Result>() { };
            ConnectionDataControl clsDataControl = new ConnectionDataControl();
            DataTable collectionDataTable = new DataTable();
            clsDataControl.DynamicParameters.Clear();
            clsDataControl.DynamicParameters.Add("@VENUEID", currentUser.CurrentVenueId);
            clsDataControl.DynamicParameters.Add("@MONTH", month.HasValue ? month.Value : DateTime.Now);
            clsDataControl.DynamicParameters.Add("@TYPE", Type);

            collectionDataTable = clsDataControl.GetDetails("rp_COLLECTIONDETAIL", IsParameter: true);

            if (collectionDataTable != null && collectionDataTable.Rows.Count > 0)
            {
                foreach (DataRow row in collectionDataTable.Rows)
                {
                    datas.Add(new rp_COLLECTIONDETAIL_Result()
                    {
                        Name = ConvertHelper.ConvertToString(row["Name"], string.Empty),
                        InvoicePeriod = ConvertHelper.ConvertToString(row["InvoicePeriod"]),
                        CollectedDate = ConvertHelper.ConvertToString(row["CollectedDate"]),
                        PaymentMode = ConvertHelper.ConvertToString(row["PaymentMode"]),
                        SportName = ConvertHelper.ConvertToString(row["SportName"]),
                        BatchCode = ConvertHelper.ConvertToString(row["BatchCode"]),
                        TotalFee = ConvertHelper.ConvertToDecimal(row["TotalFee"]),
                        TotalOtherAmount = ConvertHelper.ConvertToDecimal(row["TotalOtherAmount"]),
                        TotalDiscountAmount = ConvertHelper.ConvertToDecimal(row["TotalDiscountAmount"]),
                        AmountPaid = ConvertHelper.ConvertToDecimal(row["AmountPaid"]),
                        Description = ConvertHelper.ConvertToString(row["Description"]),
                        TransactionNumber = ConvertHelper.ConvertToString(row["TransactionNumber"]),
                        TransactionDate = ConvertHelper.ConvertToString(row["TransactionDate"]),
                        ReceivedBy = ConvertHelper.ConvertToString(row["ReceivedBy"]),
                        ReceiptNumber = ConvertHelper.ConvertToString(row["ReceiptNumber"]),
                        ReceiptDate = ConvertHelper.ConvertToDateTime(row["ReceiptDate"])
                    });
                }
            }

            return datas;
        }
    }
}