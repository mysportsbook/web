using MySportsBook.Model;
using MySportsBook.Web.Areas.Transaction.Models;
using MySportsBook.Web.Controllers;
using MySportsBook.Web.Filters;
using MySportsBook.Web.Helper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace MySportsBook.Web.Areas.Transaction.Controllers
{
    [UserAuthentication]
    public class AttendanceController : BaseController
    {
        // GET: Transaction/Attendance
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

            collectionDataTable = clsDataControl.GetDetails(argstrQuery: "GetAttendanceByDate", IsParameter: true);

            if (collectionDataTable != null && collectionDataTable.Rows.Count > 0)
            {
                foreach (DataRow row in collectionDataTable.Rows)
                {
                    datas.Add(new AttendanceBatchViewModel()
                    {
                        BatchCode = ConvertHelper.ConvertToString(row["BatchCode"], string.Empty),
                        BatchName = ConvertHelper.ConvertToString(row["BatchName"]),
                        FK_BatchId = ConvertHelper.ConvertToString(row["FK_BatchId"]),
                        Date = ConvertHelper.ConvertToString(row["Date"]),
                        Fees = ConvertHelper.ConvertToString(row["Fee"])
                    });
                }
            }
            return View(datas);
        }

        public ActionResult Create()
        {
            //List<SelectListItem> ddlList = new List<SelectListItem>();
            //ddlList.Add(new SelectListItem
            //{
            //    Text = "--Select--",
            //    Value = ""
            //});

            //var Batch = (from b in dbContext.Master_Batch where b.FK_VenueId == currentUser.CurrentVenueId select new { b.BatchCode, b.PK_BatchId }).ToList();
            //ddlList.AddRange(Batch.Select(x => new SelectListItem { Value = x.PK_BatchId.ToString(), Text = x.BatchCode }));

            //ViewBag.BatchId = new SelectList(ddlList, "Value", "Text");
            GetBatchs();
            return View();
        }

        [HttpPost]
        public ActionResult Create(string[] Players, string BatchId, string Date)
        {
            GetBatchs("");
            List<Transaction_Attendance> _transaction_Attendances = new List<Transaction_Attendance>() { };
            foreach (var p in Players)
            {
                _transaction_Attendances.Add(new Transaction_Attendance()
                {
                    FK_BatchId = ConvertHelper.ConvertToInteger(BatchId),
                    FK_PlayerId = ConvertHelper.ConvertToInteger(p),
                    FK_VenueId = currentUser.CurrentVenueId,
                    Date = ConvertHelper.ConvertToDateTime(Date),
                    Present = true,
                    CreatedDate = DateTime.Now,
                    CreatedBy = currentUser.UserId

                });
            }
            if (_transaction_Attendances.Count() > 0)
            {
                dbContext.Transaction_Attendance.AddRange(_transaction_Attendances);
                dbContext.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        private void GetBatchs(string id = "0")
        {
            List<SelectListItem> ddlList = new List<SelectListItem>();
            ddlList.Add(new SelectListItem
            {
                Text = "--Select--",
                Value = "0"
            });

            var Batch = (from b in dbContext.Master_Batch where b.FK_VenueId == currentUser.CurrentVenueId select new { b.BatchCode, b.PK_BatchId }).ToList();
            ddlList.AddRange(Batch.Select(x => new SelectListItem { Value = x.PK_BatchId.ToString(), Text = x.BatchCode }));

            ViewBag.BatchId = new SelectList(ddlList, "Value", "Text", id);
        }
        public JsonResult GetAttendanceBatchPlayer(string date, string batchId)
        {
            List<AttendanceBatchPlayerViewModel> datas = new List<AttendanceBatchPlayerViewModel>() { };

            ConnectionDataControl clsDataControl = new ConnectionDataControl();
            DataTable collectionDataTable = new DataTable();
            clsDataControl.DynamicParameters.Clear();
            clsDataControl.DynamicParameters.Add("@VenueId", currentUser.CurrentVenueId);
            clsDataControl.DynamicParameters.Add("@BatchId", batchId);
            clsDataControl.DynamicParameters.Add("@Date", date);

            collectionDataTable = clsDataControl.GetDetails(argstrQuery: "GetAttendanceByDateandBatch", IsParameter: true);

            if (collectionDataTable != null && collectionDataTable.Rows.Count > 0)
            {
                foreach (DataRow row in collectionDataTable.Rows)
                {
                    if (ConvertHelper.ConvertToString(row["PK_AttendanceId"]) != "0")
                    {
                        datas.Add(new AttendanceBatchPlayerViewModel()
                        {
                            Email = ConvertHelper.ConvertToString(row["Email"], string.Empty),
                            FirstName = ConvertHelper.ConvertToString(row["FirstName"]),
                            LastName = ConvertHelper.ConvertToString(row["LastName"]),
                            Mobile = ConvertHelper.ConvertToString(row["Mobile"]),
                            AttendanceId = ConvertHelper.ConvertToString(row["PK_AttendanceId"]),
                            Present = ConvertHelper.ConvertToBoolean(row["Present"]),
                            BatchId = ConvertHelper.ConvertToString(row["BatchId"], string.Empty),
                            PlayerId = ConvertHelper.ConvertToString(row["PlayerId"])
                        });
                    }
                }
            }
            return Json(datas, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetBatchPlayersById(int id)
        {

            //var BatchUser = (from ps in dbContext.Transaction_PlayerSport
            //                 join ms in dbContext.Master_Sport on ps.FK_SportId equals ms.PK_SportId
            //                 join p in dbContext.Master_Player on ps.FK_PlayerId equals p.PK_PlayerId
            //                 where ps.FK_BatchId == id && ps.FK_VenueId == currentUser.CurrentVenueId && ps.FK_StatusId == 1
            //                 select new
            //                 {
            //                     BatchId = ps.FK_BatchId,
            //                     PlayerId = p.PK_PlayerId,
            //                     FirstName = p.FirstName,
            //                     LastName = p.LastName,
            //                     LastPaidMonth = ps.LastGeneratedMonth,
            //                     Mobile = p.Mobile

            //                 }).ToList();

            List<AttendanceBatchPlayerViewModel> datas = new List<AttendanceBatchPlayerViewModel>() { };

            ConnectionDataControl clsDataControl = new ConnectionDataControl();
            DataTable collectionDataTable = new DataTable();
            clsDataControl.DynamicParameters.Clear();
            clsDataControl.DynamicParameters.Add("@VenueId", currentUser.CurrentVenueId);
            clsDataControl.DynamicParameters.Add("@BatchId", id);

            collectionDataTable = clsDataControl.GetDetails(argstrQuery: "GetBatchPlayersById", IsParameter: true);

            if (collectionDataTable != null && collectionDataTable.Rows.Count > 0)
            {
                foreach (DataRow row in collectionDataTable.Rows)
                {

                    datas.Add(new AttendanceBatchPlayerViewModel()
                    {
                        FirstName = ConvertHelper.ConvertToString(row["FirstName"]),
                        LastName = ConvertHelper.ConvertToString(row["LastName"]),
                        PlayerId = ConvertHelper.ConvertToString(row["PK_PlayerId"]),
                        LastPaidMonth = ConvertHelper.ConvertToString(row["LastGeneratedMonth"]),
                        Mobile = ConvertHelper.ConvertToString(row["Mobile"]),
                        Image = ConvertHelper.ConvertToString(row["ProfileImg"], string.Empty),
                        BatchId = ConvertHelper.ConvertToString(row["FK_BatchId"])
                    });

                }
            }
            return new JsonResult()
            {
                ContentEncoding = Encoding.Default,
                ContentType = "application/json",
                Data = datas,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                MaxJsonLength = int.MaxValue
            };
        }

        public ActionResult Edit(int id, string date)
        {
            GetBatchs(ConvertHelper.ConvertToString(id));
            List<AttendanceBatchPlayerViewModel> datas = new List<AttendanceBatchPlayerViewModel>() { };

            ConnectionDataControl clsDataControl = new ConnectionDataControl();
            DataTable collectionDataTable = new DataTable();
            clsDataControl.DynamicParameters.Clear();
            clsDataControl.DynamicParameters.Add("@VenueId", currentUser.CurrentVenueId);
            clsDataControl.DynamicParameters.Add("@BatchId", id);
            clsDataControl.DynamicParameters.Add("@Date", date);

            collectionDataTable = clsDataControl.GetDetails(argstrQuery: "GetAttendanceByDateandBatch", IsParameter: true);

            if (collectionDataTable != null && collectionDataTable.Rows.Count > 0)
            {
                foreach (DataRow row in collectionDataTable.Rows)
                {
                    datas.Add(new AttendanceBatchPlayerViewModel()
                    {
                        Email = ConvertHelper.ConvertToString(row["Email"], string.Empty),
                        FirstName = ConvertHelper.ConvertToString(row["FirstName"]),
                        LastName = ConvertHelper.ConvertToString(row["LastName"]),
                        Mobile = ConvertHelper.ConvertToString(row["Mobile"]),
                        AttendanceId = ConvertHelper.ConvertToString(row["PK_AttendanceId"]),
                        Present = ConvertHelper.ConvertToBoolean(row["Present"]),
                        BatchId = ConvertHelper.ConvertToString(row["BatchId"], string.Empty),
                        PlayerId = ConvertHelper.ConvertToString(row["PlayerId"])
                    });
                }
            }
            return View(datas);
        }

        public JsonResult GetPlayers(string term)
        {

            return Json("", JsonRequestBehavior.AllowGet);
        }

    }
}