using MySportsBook.Model;
using MySportsBook.Model.ViewModel;
using MySportsBook.Web.Areas.Transaction.Models;
using MySportsBook.Web.Controllers;
using MySportsBook.Web.Filters;
using MySportsBook.Web.Helper;
using MySportsBook.Web.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace MySportsBook.Web.Areas.Master.Controllers
{
    [UserAuthentication]
    public class PlayerController : BaseController
    {
        private int[] status = { 1, 2 };
        // GET: Master/Player
        public async Task<ActionResult> Index()

        {
            //  List<PlayerModel> _playermodel = new List<PlayerModel>();
            //// var master_Player = dbContext.Master_Player.Where(x => x.FK_VenueId == currentUser.CurrentVenueId && x.FK_StatusId == 1).OrderByDescending(x => x.CreatedDate).Include(m => m.Configuration_PlayerType).Include(m => m.Configuration_Status);
            // master_Player.ToList().ForEach(p =>
            // {
            //     _playermodel.Add(new PlayerModel
            //     {
            //         Player = p,
            //         PlayerSports = dbContext.Transaction_PlayerSport.Include(i => i.Master_Sport).Include(i => i.Master_Batch).Where(s => s.FK_StatusId == 1 && s.FK_VenueId == currentUser.CurrentVenueId && s.FK_PlayerId == p.PK_PlayerId).ToList()
            //     });
            // });
            //
            //return View(master_Player);

            //var master_Player = dbContext.Master_Player
            //    .Include(m => m.Transaction_PlayerSport.Select(q => q.Master_Sport))
            //    .Include(m => m.Transaction_PlayerSport.Select(q => q.Master_Batch))
            //     .Include(m => m.Configuration_Status)
            //    .Where(x => x.FK_VenueId == currentUser.CurrentVenueId && x.FK_PlayerTypeId == 1 && status.Contains(x.FK_StatusId))
            //    .OrderByDescending(x => x.CreatedDate);
            List<PlayersViewMobel> datas = new List<PlayersViewMobel>() { };
            return View(datas);
        }
        [HttpPost]
        public ActionResult Index(string search = "")
        {
            List<PlayersViewMobel> datas = new List<PlayersViewMobel>() { };
            try
            {
                ConnectionDataControl clsDataControl = new ConnectionDataControl();
                DataTable collectionDataTable = new DataTable();
                clsDataControl.DynamicParameters.Clear();
                clsDataControl.DynamicParameters.Add("@VenueId", currentUser.CurrentVenueId);
                clsDataControl.DynamicParameters.Add("@Search", search);
                clsDataControl.DynamicParameters.Add("@IsInvoiceScreen", 0);

                collectionDataTable = clsDataControl.GetDetails(argstrQuery: "GetAllPlayersByVenueID", IsParameter: true);

                if (collectionDataTable != null && collectionDataTable.Rows.Count > 0)
                {
                    foreach (DataRow row in collectionDataTable.Rows)
                    {
                        datas.Add(new PlayersViewMobel()
                        {
                            FirstName = ConvertHelper.ConvertToString(row["FirstName"], string.Empty),
                            LastName = ConvertHelper.ConvertToString(row["LastName"]),
                            Mobile = ConvertHelper.ConvertToString(row["Mobile"]),
                            PK_PlayerId = ConvertHelper.ConvertToInteger(row["PK_PlayerId"]),
                            Batches = ConvertHelper.ConvertToString(row["BatchName"]),
                            FK_StatusId = ConvertHelper.ConvertToInteger(row["StatusId"]),
                            SportName = ConvertHelper.ConvertToString(row["SportName"]),
                            Fees = ConvertHelper.ConvertToDecimal(row["Fees"]),
                            ProfileImg = ConvertHelper.ConvertToString(row["ProfileImg"])

                        });
                    }
                }
            }
            catch
            {

            }
            return View(datas);
        }

        // GET: Master/Player/Create
        public ActionResult Create()
        {
            ViewBag.FK_PlayerTypeId = new SelectList(dbContext.Configuration_PlayerType.OrderByDescending(x => x.PlayerType), "PK_PlayerTypeId", "PlayerType").ToList();
            List<SelectListItem> ddlList = new List<SelectListItem>();
            ddlList.Add(new SelectListItem
            {
                Text = "--Select--",
                Value = ""
            });
            dbContext.Master_Sport.Where(s => s.FK_VenueId == currentUser.CurrentVenueId && s.FK_StatusId == 1).OrderBy(x => x.SportName).ToList().ForEach(x =>
              {
                  ddlList.Add(new SelectListItem
                  {
                      Text = x.SportName,
                      Value = x.PK_SportId.ToString()
                  });
              });
            ViewBag.Sport = ddlList;
            ViewBag.Batch = new SelectList((new SelectListItem[] { new SelectListItem { Text = "--Select--", Value = string.Empty } }).ToList(), "Value", "Text");
            ddlList = new List<SelectListItem>();
            ddlList.Add(new SelectListItem
            {
                Text = "--Select--",
                Value = ""
            });
            dbContext.Configuration_InvoicePeriod.ToList().ForEach(x =>
            {
                ddlList.Add(new SelectListItem
                {
                    Text = x.InvoicePeriod,
                    Value = x.PK_InvoicePeriodId.ToString()
                });
            });
            ViewBag.InvoicePeriod = ddlList;
            return View();
        }

        // POST: Master/Player/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [AcceptVerbs(HttpVerbs.Post)]
        //public async Task<ActionResult> Create(PlayerModel playerModel)
        public ActionResult Create(dataModel dataModel)
        {
            try
            {
                PlayerModel playermodel = JsonConvert.DeserializeObject<PlayerModel>(dataModel.playerModelobj);
                if (!dbContext.Master_Player.Where(x => x.FK_StatusId == 1 && x.FirstName + x.LastName + x.Mobile == playermodel.Player.FirstName + playermodel.Player.LastName + playermodel.Player.Mobile).Any())
                {
                    if (Request.Files.Count > 0)
                    {
                        try
                        {
                            //  Get all files from Request object  
                            HttpFileCollectionBase files = Request.Files;
                            HttpPostedFileBase file = files[0];
                            playermodel.Player.ProfileImg = GenerateThumbnails(0.5, file.InputStream);
                        }
                        catch
                        {

                        }

                    }

                    playermodel.Player.FK_StatusId = 1;
                    playermodel.Player.FK_VenueId = currentUser.CurrentVenueId;
                    playermodel.Player.CreatedBy = currentUser.UserId;
                    playermodel.Player.CreatedDate = DateTime.Now.ToLocalTime();
                    //playermodel.Player.ProfileImg = Image;
                    dbContext.Master_Player.Add(playermodel.Player);
                    dbContext.SaveChanges();
                    playermodel.PlayerSports.ForEach(ps =>
                    {
                        ps.FK_VenueId = currentUser.CurrentVenueId;
                        ps.FK_PlayerId = playermodel.Player.PK_PlayerId;
                        ps.FK_CourtId = dbContext.Master_Batch.Where(b => b.PK_BatchId == ps.FK_BatchId).FirstOrDefault().FK_CourtId;
                        ps.FK_StatusId = 1;
                        ps.LastGeneratedMonth = DateTime.Now.ToLocalTime().AddMonths(-1).ToString("MMMyyyy");
                        ps.CreatedBy = currentUser.UserId;
                        ps.CreatedDate = DateTime.Now.ToLocalTime();
                    });
                    dbContext.Transaction_PlayerSport.AddRange(playermodel.PlayerSports);
                    dbContext.SaveChanges();
                    return Json(true, JsonRequestBehavior.AllowGet);
                }
                return Json(false, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }

            // await dbContext.SaveChangesAsync();
        }

        // GET: Master/Player/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var master_Player = dbContext.Master_Player.Where(x => x.PK_PlayerId == id && x.FK_VenueId == currentUser.CurrentVenueId && status.Contains(x.FK_StatusId));
            if (master_Player == null || master_Player.Count() == 0)
            {
                return HttpNotFound();
            }
            if (master_Player.FirstOrDefault() == null)
            {
                return HttpNotFound();
            }
            PlayerModel _playermodel = new PlayerModel();
            _playermodel.Player = new Master_Player();
            _playermodel.Player = master_Player.FirstOrDefault();
            _playermodel.PlayerSports = new List<Transaction_PlayerSport>();
            _playermodel.PlayerSports = dbContext.Transaction_PlayerSport.Where(p => p.FK_StatusId == 1 && p.FK_VenueId == currentUser.CurrentVenueId && p.FK_PlayerId == id).ToList();

            ViewBag.FK_PlayerTypeId = new SelectList(dbContext.Configuration_PlayerType.OrderByDescending(x => x.PlayerType), "PK_PlayerTypeId", "PlayerType", master_Player.FirstOrDefault().FK_PlayerTypeId).ToList();
            List<SelectListItem> ddlList = new List<SelectListItem>();
            ddlList.Add(new SelectListItem
            {
                Text = "--Select--",
                Value = ""
            });
            dbContext.Master_Sport.Where(s => s.FK_VenueId == currentUser.CurrentVenueId && s.FK_StatusId == 1).ToList().ForEach(x =>
              {
                  ddlList.Add(new SelectListItem
                  {
                      Text = x.SportName,
                      Value = x.PK_SportId.ToString()
                  });
              });
            ViewBag.Sport = ddlList;
            ViewBag.Batch = new SelectList((new SelectListItem[] { new SelectListItem { Text = "--Select--", Value = string.Empty } }).ToList(), "Value", "Text");
            ddlList = new List<SelectListItem>();
            ddlList.Add(new SelectListItem
            {
                Text = "--Select--",
                Value = ""
            });
            dbContext.Configuration_InvoicePeriod.ToList().ForEach(x =>
            {
                ddlList.Add(new SelectListItem
                {
                    Text = x.InvoicePeriod,
                    Value = x.PK_InvoicePeriodId.ToString()
                });
            });
            ViewBag.InvoicePeriod = ddlList;
            return View(_playermodel);
        }

        // POST: Master/Player/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public ActionResult Edit(dataModel dataModel)
        {
            try
            {
                PlayerModel playermodel = JsonConvert.DeserializeObject<PlayerModel>(dataModel.playerModelobj);

                var _player = dbContext.Master_Player.Where(x => x.PK_PlayerId == playermodel.Player.PK_PlayerId && x.FK_VenueId == currentUser.CurrentVenueId && status.Contains(x.FK_StatusId));
                if (_player == null || _player.Count() == 0)
                {
                    return HttpNotFound();
                }
                if (_player.FirstOrDefault() == null)
                {
                    return HttpNotFound();
                }

                
                if (playermodel != null && playermodel.Player != null && playermodel.PlayerSports.Count > 0)
                {
                    if (!dbContext.Master_Player.Where(x => status.Contains(x.FK_StatusId) && x.PK_PlayerId != playermodel.Player.PK_PlayerId && x.FirstName + x.LastName + x.Mobile == playermodel.Player.FirstName + playermodel.Player.LastName + playermodel.Player.Mobile).Any())
                    {
                        Master_Player master_Player = _player.FirstOrDefault();
                        master_Player.FK_PlayerTypeId = playermodel.Player.FK_PlayerTypeId;
                        master_Player.FirstName = playermodel.Player.FirstName;
                        master_Player.LastName = playermodel.Player.LastName;
                        master_Player.Address = playermodel.Player.Address;
                        master_Player.Email = playermodel.Player.Email;
                        master_Player.Mobile = playermodel.Player.Mobile;
                        master_Player.EmailEnabled = playermodel.Player.EmailEnabled;
                        master_Player.SMSEnabled = playermodel.Player.SMSEnabled;
                        master_Player.WhatsupEnabled = playermodel.Player.WhatsupEnabled;
                        master_Player.LoginRequired = playermodel.Player.LoginRequired;
                        master_Player.FK_StatusId = 1;
                        master_Player.ModifiedBy = currentUser.UserId;
                        master_Player.ModifiedDate = DateTime.Now.ToLocalTime();
                        if (Request.Files.Count > 0)
                        {
                            try
                            {
                                //  Get all files from Request object  
                                HttpFileCollectionBase files = Request.Files;
                                HttpPostedFileBase file = files[0];
                                master_Player.ProfileImg = GenerateThumbnails(0.5, file.InputStream);
                            }
                            catch
                            {

                            }

                        }
                        dbContext.Entry(master_Player).State = EntityState.Modified;
                        var _playersport = dbContext.Transaction_PlayerSport.Where(x => x.FK_PlayerId == master_Player.PK_PlayerId && x.FK_StatusId == 1);
                        if (playermodel.PlayerSports.Count() > 0 && _playersport.Count() > 0)
                        {
                            //DELETE
                            var _deleteplayersport = _playersport.ToList().Where(o => !playermodel.PlayerSports.Any(n => n.FK_BatchId == o.FK_BatchId));
                            if (_deleteplayersport != null && _deleteplayersport.Count() > 0)
                            {
                                _deleteplayersport.ToList().ForEach(ps =>
                                {
                                    ps.FK_StatusId = 2;
                                    ps.ModifiedBy = currentUser.UserId;
                                    ps.ModifiedDate = DateTime.Now.ToLocalTime();
                                    dbContext.Entry(ps).State = EntityState.Modified;
                                });
                            }
                            //UPDATE
                            var _updateplayersport = _playersport.ToList().Where(o => playermodel.PlayerSports.Any(n => n.FK_BatchId == o.FK_BatchId));
                            if (_updateplayersport != null && _updateplayersport.Count() > 0)
                            {
                                _updateplayersport.ToList().ForEach(ps =>
                                {
                                    var _plysport = playermodel.PlayerSports.Find(s => s.FK_BatchId == ps.FK_BatchId);
                                    if (_plysport != null)
                                    {
                                        ps.Fee = _plysport.Fee;
                                        ps.FK_InvoicePeriodId = _plysport.FK_InvoicePeriodId;
                                        ps.ModifiedBy = currentUser.UserId;
                                        ps.ModifiedDate = DateTime.Now.ToLocalTime();
                                        dbContext.Entry(ps).State = EntityState.Modified;
                                    }
                                });
                            }
                            //INSERT
                            var _insertplayersport = playermodel.PlayerSports.Where(o => !_playersport.Any(n => n.FK_BatchId == o.FK_BatchId));
                            if (_insertplayersport != null && _insertplayersport.Count() > 0)
                            {
                                _insertplayersport.ToList().ForEach(ps =>
                                {
                                    ps.FK_VenueId = currentUser.CurrentVenueId;
                                    ps.FK_PlayerId = playermodel.Player.PK_PlayerId;
                                    ps.FK_CourtId = dbContext.Master_Batch.Where(b => b.PK_BatchId == ps.FK_BatchId).FirstOrDefault().FK_CourtId;
                                    ps.LastGeneratedMonth = DateTime.Now.ToLocalTime().AddMonths(-1).ToString("MMMyyyy");
                                    ps.FK_StatusId = 1;
                                    ps.CreatedBy = currentUser.UserId;
                                    ps.CreatedDate = DateTime.Now.ToLocalTime();
                                });
                                dbContext.Transaction_PlayerSport.AddRange(_insertplayersport);
                            }
                        }
                        else if (playermodel.PlayerSports.Count() == 0 && _playersport.Count() > 0)
                        {
                            //DELETE
                            dbContext.Transaction_PlayerSport.RemoveRange(_playersport);
                        }
                        else
                        {
                            if (playermodel.PlayerSports.Count() > 0)
                            {
                                //INSERT
                                playermodel.PlayerSports.ForEach(ps =>
                                {
                                    ps.FK_VenueId = currentUser.CurrentVenueId;
                                    ps.FK_PlayerId = playermodel.Player.PK_PlayerId;
                                    ps.FK_CourtId = dbContext.Master_Batch.Where(b => b.PK_BatchId == ps.FK_BatchId).FirstOrDefault().FK_CourtId;
                                    ps.FK_StatusId = 1;
                                    ps.LastGeneratedMonth = DateTime.Now.ToLocalTime().AddMonths(-1).ToString("MMMyyyy");
                                    ps.CreatedBy = currentUser.UserId;
                                    ps.CreatedDate = DateTime.Now.ToLocalTime();
                                });
                                dbContext.Transaction_PlayerSport.AddRange(playermodel.PlayerSports);
                            }
                        }
                        dbContext.SaveChanges();
                        return Json(true, JsonRequestBehavior.AllowGet);
                    }
                    return Json(false, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(false, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception)
            {

                return Json(false, JsonRequestBehavior.AllowGet);
            }


        }

        // GET: Master/Player/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {

            var _player = dbContext.Master_Player.Where(x => x.PK_PlayerId == id && x.FK_VenueId == currentUser.CurrentVenueId && status.Contains(x.FK_StatusId));
            if (_player == null || _player.Count() == 0)
            {
                return HttpNotFound();
            }
            if (_player.FirstOrDefault() == null)
            {
                return HttpNotFound();
            }
            Master_Player master_Player = _player.FirstOrDefault();
            master_Player.FK_StatusId = 5;
            master_Player.ModifiedBy = currentUser.UserId;
            master_Player.ModifiedDate = DateTime.Now.ToLocalTime();
            dbContext.Entry(master_Player).State = EntityState.Modified;
            //dbContext.Master_Player.Remove(master_Player);
            await dbContext.SaveChangesAsync();
            return RedirectToAction("Index");
        }


        [HttpGet, ActionName("Details")]
        public ActionResult GetDetails(int id)
        {
            var _playersports = dbContext.Transaction_PlayerSport.Where(p => p.FK_VenueId == currentUser.CurrentVenueId && p.FK_PlayerId == id && p.FK_StatusId == 1)
                                                     .Join(dbContext.Master_Batch.Where(p => p.FK_VenueId == currentUser.CurrentVenueId), playersport => playersport.FK_BatchId, batch => batch.PK_BatchId, (playersport, batch) => new { playersport, batch })
                                                     .Join(dbContext.Master_Court.Where(c => c.FK_StatusId == 1), playerbatchsport => playerbatchsport.batch.FK_CourtId, court => court.PK_CourtId, (playerbatchsport, court) => new { playerbatchsport, court })
                                                     .Join(dbContext.Master_Sport.Where(p => p.FK_VenueId == currentUser.CurrentVenueId), playersport => playersport.playerbatchsport.playersport.FK_SportId, sport => sport.PK_SportId, (playersport, sport) => new { playersport, sport })
                                                     .Join(dbContext.Configuration_InvoicePeriod, playersport => playersport.playersport.playerbatchsport.playersport.FK_InvoicePeriodId, inv => inv.PK_InvoicePeriodId, (playersport, inv) => new { playersport, inv })
                                                     .Select(ps => new PlayerSportModel()
                                                     {
                                                         SportId = ps.playersport.sport.PK_SportId,
                                                         Sport = ps.playersport.sport.SportName,
                                                         BatchId = ps.playersport.playersport.playerbatchsport.batch.PK_BatchId,
                                                         Batch = ps.playersport.playersport.playerbatchsport.batch.BatchName + " - " + ps.playersport.playersport.court.CourtName,
                                                         InvId = ps.inv.PK_InvoicePeriodId,
                                                         Inv = ps.inv.InvoicePeriod,
                                                         Fee = (double)ps.playersport.playersport.playerbatchsport.playersport.Fee
                                                     }).ToList();
            return Json(_playersports, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ActivateDeactivate(int id)
        {
            var _player = dbContext.Master_Player.Where(x => x.PK_PlayerId == id && x.FK_VenueId == currentUser.CurrentVenueId).FirstOrDefault();
            if (_player == null)
            {
                return HttpNotFound();
            }
            _player.FK_StatusId = _player.FK_StatusId == 1 ? 2 : 1;
            _player.ModifiedBy = currentUser.UserId;
            _player.ModifiedDate = DateTime.Now.ToLocalTime();
            dbContext.Entry(_player).State = EntityState.Modified;
            dbContext.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                dbContext.Dispose();
            }
            base.Dispose(disposing);
        }

        private string GenerateThumbnails(double scaleFactor, Stream sourcePath)
        {
            string _result = "";
            using (var image = Image.FromStream(sourcePath))
            {
                var newWidth = (int)(image.Width * scaleFactor);
                var newHeight = (int)(image.Height * scaleFactor);
                var thumbnailImg = new Bitmap(newWidth, newHeight);
                var thumbGraph = Graphics.FromImage(thumbnailImg);
                thumbGraph.CompositingQuality = CompositingQuality.HighQuality;
                thumbGraph.SmoothingMode = SmoothingMode.HighQuality;
                thumbGraph.InterpolationMode = InterpolationMode.HighQualityBicubic;
                var imageRectangle = new Rectangle(0, 0, newWidth, newHeight);
                thumbGraph.DrawImage(image, imageRectangle);
                using (MemoryStream m = new MemoryStream())
                {
                    image.Save(m, image.RawFormat);
                    byte[] imageBytes = m.ToArray();
                    // Convert byte[] to Base64 String
                    _result = Convert.ToBase64String(imageBytes);
                }
            }
            return _result;
        }


    }
}
