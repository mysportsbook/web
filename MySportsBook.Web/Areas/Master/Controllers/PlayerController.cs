using MySportsBook.Model;
using MySportsBook.Model.ViewModel;
using MySportsBook.Web.Controllers;
using MySportsBook.Web.Filters;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace MySportsBook.Web.Areas.Master.Controllers
{
    [UserAuthentication]
    public class PlayerController : BaseController
    {
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

            var master_Player = dbContext.Master_Player
               .Include(m => m.Transaction_PlayerSport).Include(m => m.Master_Batch)
             .Where(x => x.FK_VenueId == currentUser.CurrentVenueId && x.FK_PlayerTypeId == 1 && x.FK_StatusId == 1)
             .OrderByDescending(x => x.CreatedDate);

            return View(await master_Player.ToListAsync());
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
        //public async Task<ActionResult> Create(PlayerModel playerModel)
        public ActionResult Create(PlayerModel playermodel)
        {
            try
            {
                if (!dbContext.Master_Player.Where(x => x.FK_StatusId == 1 && x.FirstName + x.LastName + x.Mobile == playermodel.Player.FirstName + playermodel.Player.LastName + playermodel.Player.Mobile).Any())
                {
                    playermodel.Player.FK_StatusId = 1;
                    playermodel.Player.FK_VenueId = currentUser.CurrentVenueId;
                    playermodel.Player.CreatedBy = currentUser.UserId;
                    playermodel.Player.CreatedDate = DateTime.Now.ToLocalTime();
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
            var master_Player = dbContext.Master_Player.Where(x => x.PK_PlayerId == id && x.FK_VenueId == currentUser.CurrentVenueId && x.FK_StatusId == 1);
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
        public ActionResult Edit(PlayerModel playermodel)
        {
            try
            {
                var _player = dbContext.Master_Player.Where(x => x.PK_PlayerId == playermodel.Player.PK_PlayerId && x.FK_VenueId == currentUser.CurrentVenueId && x.FK_StatusId == 1);
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
                    if (!dbContext.Master_Player.Where(x => x.FK_StatusId == 1 && x.PK_PlayerId != playermodel.Player.PK_PlayerId && x.FirstName + x.LastName + x.Mobile == playermodel.Player.FirstName + playermodel.Player.LastName + playermodel.Player.Mobile).Any())
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
            var _player = dbContext.Master_Player.Where(x => x.PK_PlayerId == id && x.FK_VenueId == currentUser.CurrentVenueId && x.FK_StatusId == 1);
            if (_player == null || _player.Count() == 0)
            {
                return HttpNotFound();
            }
            if (_player.FirstOrDefault() == null)
            {
                return HttpNotFound();
            }
            Master_Player master_Player = _player.FirstOrDefault();
            master_Player.FK_StatusId = 2;
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
