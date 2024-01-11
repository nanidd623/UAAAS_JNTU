using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Security;
using UAAAS.Models;

namespace UAAAS.Controllers
{
    [ErrorHandling]
    public class CollegeSportsController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();
        public string playgroundtype = string.Empty;
        public string transportType = string.Empty;
        public string paymentType = string.Empty;
        public PlayGroundTypeModel[] playGroundTypes = new[]
            {
                new PlayGroundTypeModel { id = "1", Name = "Square" },
                new PlayGroundTypeModel { id = "2", Name = "Rectangle" },
                new PlayGroundTypeModel { id = "3", Name = "Round" },
                new PlayGroundTypeModel { id = "4", Name = "Oval" },
                new PlayGroundTypeModel { id = "5", Name = "Cricket" },
                new PlayGroundTypeModel { id = "6", Name = "Other" }
            };
        public List<PlayGroundTypeModel> playGroundType = new List<PlayGroundTypeModel>();

        public ModeOfTransportModel[] transportMode = new[]
            {
                new ModeOfTransportModel { id = "1", Name = "College Transport" },
                new ModeOfTransportModel { id = "2", Name = "Public Transport" },
                new ModeOfTransportModel { id = "3", Name = "Other" }
            };
        public List<ModeOfTransportModel> transportModes = new List<ModeOfTransportModel>();

        public ModeOfPaymentModel[] paymentMode = new[]
            {
                new ModeOfPaymentModel { id = "1", Name = "Cash" },
                new ModeOfPaymentModel { id = "2", Name = "Cheque" },
                new ModeOfPaymentModel { id = "3", Name = "Bank Transfer" },
                new ModeOfPaymentModel { id = "4", Name = "Other" }
            };

        public List<ModeOfPaymentModel> paymentsModes = new List<ModeOfPaymentModel>();

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Create(string collegeId)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(collegeUsers => collegeUsers.userID == userID).Select(collegeUsers => collegeUsers.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                if (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry"))
                {
                    if (collegeId != null)
                    {
                        userCollegeID = Convert.ToInt32(Utilities.DecryptString(collegeId, WebConfigurationManager.AppSettings["CryptoKey"]));
                    }
                }
            }
            ViewBag.collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString();
            if (userCollegeID == 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("Create", "CollegeInformation");
            }
            if (userCollegeID == 0 && (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry")))
            {
                return RedirectToAction("Create", "CollegeInformation", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
            }
            int existId = db.jntuh_college_desirable_others.Where(otherDesirables => otherDesirables.collegeId == userCollegeID).Select(otherDesirables => otherDesirables.id).FirstOrDefault();
              
           
            CollegePlayground collegePlayground = new CollegePlayground();
            string[] strSelected = new string[] { };
            foreach (var type in playGroundTypes)
            {
                string strtype = type.id.ToString();
                playGroundType.Add(new PlayGroundTypeModel { id = type.id, Name = type.Name, Checked = strSelected.Contains(strtype) ? 1 : 0 });
            }

            collegePlayground.GroundTypes = playGroundType;

            var gamesList = db.jntuh_college_sports.Where(cs => cs.collegeId == userCollegeID).
                            Join(db.jntuh_sports_type.Where(s => s.isActive == true),
                            cs => cs.sportsTypeId, s => s.id,
                            (cs, s) => new CollegeSports
                            {
                                id = cs.id,
                                collegeId = cs.collegeId,
                                sportsType = s.sportType,
                                sportsFacility = cs.sportsFacility,
                                createdBy = cs.createdBy,
                                createdOn = cs.createdOn,
                                updatedOn = cs.updatedOn,
                                updatedBy = cs.updatedBy
                            }).ToList();
            ViewBag.Games = gamesList;

            foreach (var type in transportMode)
            {
                string strtype = type.id.ToString();
                transportModes.Add(new ModeOfTransportModel { id = type.id, Name = type.Name, Checked = strSelected.Contains(strtype) ? 1 : 0 });
            }

            collegePlayground.TransportModes = transportModes;


            foreach (var type in paymentMode)
            {
                string strtype = type.id.ToString();
                paymentsModes.Add(new ModeOfPaymentModel { id = type.id, Name = type.Name, Checked = strSelected.Contains(strtype) ? 1 : 0 });
            }

            collegePlayground.PaymentModes = paymentsModes;
            DateTime todayDate = DateTime.Now.Date;
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            int status = db.jntuh_college_edit_status.Where(editStatus => editStatus.academicyearId == prAy && editStatus.collegeId == userCollegeID &&
                                                                                      editStatus.IsCollegeEditable == true &&
                                                                                      editStatus.editFromDate <= todayDate &&
                                                                                      editStatus.editToDate >= todayDate)
                                                                 .Select(editStatus => editStatus.id)
                                                                 .FirstOrDefault();
            if (userCollegeID > 0 && existId > 0 && status == 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("View", "CollegeSports");
            }
            if (userCollegeID > 0 && existId > 0 && status > 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("Edit", "CollegeSports");
            }
            if (userCollegeID > 0 && existId > 0 && (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry")))
            {
                return RedirectToAction("Edit", "CollegeSports", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
            }
            if (existId == 0 && status == 0 && Roles.IsUserInRole("College"))
            {
                ViewBag.NotUpload = true;
            }
            else
            {
                ViewBag.NotUpload = false;
            }

            bool isPageEditable = db.jntuh_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("SG") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

            if (isPageEditable)
            {
                ViewBag.IsEditable = true;
            }
            else
            {
                ViewBag.IsEditable = false;
                return RedirectToAction("View", "CollegeSports");
            }
            ViewBag.Count = gamesList.Count();
            collegePlayground.collegeId = userCollegeID;
            return View(collegePlayground);
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        [HttpPost]
        public ActionResult Create(CollegePlayground collegePlayground)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                userCollegeID=collegePlayground.collegeId;
            }
            SaveCollegeSportsInformation(collegePlayground);
            return View(collegePlayground);
        }

        private void SaveCollegeSportsInformation(CollegePlayground collegePlayground)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(collegeUsers => collegeUsers.userID == userID).Select(collegeUsers => collegeUsers.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                userCollegeID = collegePlayground.collegeId;
            }
            var SportsMeassage = string.Empty;
            if (ModelState.IsValid)
            {
                //get current logged in user id
                collegePlayground.collegeId = userCollegeID;

                jntuh_college_desirable_others otherDesirablesDetails = new jntuh_college_desirable_others();
                otherDesirablesDetails.id = collegePlayground.id;
                otherDesirablesDetails.collegeId = userCollegeID;
                otherDesirablesDetails.totalPlaygrounds = collegePlayground.totalPlaygrounds;

                foreach (string groundtype in collegePlayground.PlayGroundTypeId.Where(s => !string.IsNullOrWhiteSpace(s)))
                {
                    playgroundtype += groundtype + "|";
                }
                otherDesirablesDetails.playgroundType = playgroundtype;

                foreach (string Transport in collegePlayground.modeOfTransportId.Where(s => !string.IsNullOrWhiteSpace(s)))
                {
                    transportType += Transport + "|";
                }

                otherDesirablesDetails.modeOfTransport = transportType;
                otherDesirablesDetails.numberOfBus = collegePlayground.numberOfBus;
                otherDesirablesDetails.numberOfOtherVehicles = collegePlayground.numberOfOtherVehicles;

                foreach (string Payment in collegePlayground.modeOfPaymentId.Where(s => !string.IsNullOrWhiteSpace(s)))
                {
                    paymentType += Payment + "|";
                }

                otherDesirablesDetails.modeOfPayment = paymentType;


                var id = db.jntuh_college_desirable_others.Where(d => d.collegeId == userCollegeID).Select(a => a.id).FirstOrDefault();

                if (id == 0)
                {
                    otherDesirablesDetails.createdBy = userID;
                    otherDesirablesDetails.createdOn = DateTime.Now;
                    db.jntuh_college_desirable_others.Add(otherDesirablesDetails);
                    SportsMeassage = "Save";
                }
                else
                {
                    otherDesirablesDetails.id = id;
                    otherDesirablesDetails.createdBy = db.jntuh_college_desirable_others.Where(d => d.id == id).Select(d => d.createdBy).FirstOrDefault();
                    otherDesirablesDetails.createdOn = db.jntuh_college_desirable_others.Where(d => d.id == id).Select(d => d.createdOn).FirstOrDefault();
                    otherDesirablesDetails.updatedOn = DateTime.Now;
                    otherDesirablesDetails.updatedBy = userID;
                    db.Entry(otherDesirablesDetails).State = EntityState.Modified;
                    SportsMeassage = "Update";
                }

                db.SaveChanges();

                if (SportsMeassage == "Update")
                {
                    TempData["Success"] = "Other Desirable Requirements Details are Updated successfully.";
                }
                else
                {
                    TempData["Success"] = "Other Desirable Requirements Details are Added successfully.";
                }

            }
            string[] selectedPlayGroundType = collegePlayground.PlayGroundTypeId.Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s).ToArray();

            foreach (var type in playGroundTypes)
            {
                string strtype = type.id.ToString();
                playGroundType.Add(new PlayGroundTypeModel { id = type.id, Name = type.Name, Checked = selectedPlayGroundType.Contains(strtype) ? 1 : 0 });
            }

            collegePlayground.GroundTypes = playGroundType;

            ViewBag.Games = db.jntuh_college_sports.Where(cs => cs.collegeId == userCollegeID).
                            Join(db.jntuh_sports_type.Where(s => s.isActive == true),
                            cs => cs.sportsTypeId, s => s.id,
                            (cs, s) => new CollegeSports
                            {
                                id = cs.id,
                                collegeId = cs.collegeId,
                                sportsType = s.sportType,
                                sportsFacility = cs.sportsFacility,
                                createdBy = cs.createdBy,
                                createdOn = cs.createdOn,
                                updatedOn = cs.updatedOn,
                                updatedBy = cs.updatedBy
                            }).ToList();

            string[] selectedTransportType = collegePlayground.modeOfTransportId.Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s).ToArray();

            foreach (var type in transportMode)
            {
                string strtype = type.id.ToString();
                transportModes.Add(new ModeOfTransportModel { id = type.id, Name = type.Name, Checked = selectedTransportType.Contains(strtype) ? 1 : 0 });
            }

            collegePlayground.TransportModes = transportModes;

            string[] selectedPaymentType = collegePlayground.modeOfPaymentId.Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s).ToArray();

            foreach (var type in paymentMode)
            {
                string strtype = type.id.ToString();
                paymentsModes.Add(new ModeOfPaymentModel { id = type.id, Name = type.Name, Checked = selectedPaymentType.Contains(strtype) ? 1 : 0 });
            }

            collegePlayground.PaymentModes = paymentsModes;
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Edit(string collegeId)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                if (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry"))
                {
                    if (collegeId != null)
                    {
                        userCollegeID = Convert.ToInt32(Utilities.DecryptString(collegeId, WebConfigurationManager.AppSettings["CryptoKey"]));
                    }                    
                }
            }
            ViewBag.collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString();
            int GroundTypeId = db.jntuh_college_desirable_others.Where(a => a.collegeId == userCollegeID).Select(a => a.id).FirstOrDefault();

            if (userCollegeID == 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("Create", "CollegeInformation");
            }
            if (GroundTypeId == 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("Create", "CollegeExaminationBranch");
            }
            DateTime todayDate = DateTime.Now.Date;
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            int status = db.jntuh_college_edit_status.Where(editStatus => editStatus.academicyearId == prAy && editStatus.collegeId == userCollegeID &&
                                                                                      editStatus.IsCollegeEditable == true &&
                                                                                      editStatus.editFromDate <= todayDate &&
                                                                                      editStatus.editToDate >= todayDate)
                                                                 .Select(editStatus => editStatus.id)
                                                                 .FirstOrDefault();
            if (status == 0 && Roles.IsUserInRole("College"))
            {
                ViewBag.IsEditable = false;
                return RedirectToAction("View", "CollegeSports");
            }
            else
            {
                bool isPageEditable = db.jntuh_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("SG") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

                if (isPageEditable)
                {
                    ViewBag.IsEditable = true;
                }
                else
                {
                    ViewBag.IsEditable = false;
                    return RedirectToAction("View", "CollegeSports");
                }
            }


            CollegePlayground collegePlayground = new CollegePlayground();
            collegePlayground.collegeId = userCollegeID;
            jntuh_college_desirable_others jntuh_college_desirable_others = db.jntuh_college_desirable_others.Find(GroundTypeId);
            if (jntuh_college_desirable_others != null)
            {
                collegePlayground.id = jntuh_college_desirable_others.id;
                collegePlayground.collegeId = jntuh_college_desirable_others.collegeId;
                collegePlayground.totalPlaygrounds = jntuh_college_desirable_others.totalPlaygrounds;

                string[] selectedPlayGroundType = jntuh_college_desirable_others.playgroundType.Split('|').ToArray();

                foreach (var type in playGroundTypes)
                {
                    string strtype = type.id.ToString();
                    playGroundType.Add(new PlayGroundTypeModel { id = type.id, Name = type.Name, Checked = selectedPlayGroundType.Contains(strtype) ? 1 : 0 });
                }

                collegePlayground.GroundTypes = playGroundType;

                string[] selectedTransportType = jntuh_college_desirable_others.modeOfTransport.Split('|').ToArray();

                foreach (var type in transportMode)
                {
                    string strtype = type.id.ToString();
                    transportModes.Add(new ModeOfTransportModel { id = type.id, Name = type.Name, Checked = selectedTransportType.Contains(strtype) ? 1 : 0 });
                }

                collegePlayground.TransportModes = transportModes;

                string[] selectedPaymentType = jntuh_college_desirable_others.modeOfPayment.Split('|').ToArray();

                foreach (var type in paymentMode)
                {
                    string strtype = type.id.ToString();
                    paymentsModes.Add(new ModeOfPaymentModel { id = type.id, Name = type.Name, Checked = selectedPaymentType.Contains(strtype) ? 1 : 0 });
                }

                collegePlayground.PaymentModes = paymentsModes;
                collegePlayground.numberOfBus = jntuh_college_desirable_others.numberOfBus;
                collegePlayground.numberOfOtherVehicles = jntuh_college_desirable_others.numberOfOtherVehicles;
                collegePlayground.createdBy = jntuh_college_desirable_others.createdBy;
                collegePlayground.createdOn = jntuh_college_desirable_others.createdOn;
                collegePlayground.updatedBy = jntuh_college_desirable_others.updatedBy;
                collegePlayground.updatedOn = jntuh_college_desirable_others.updatedOn;
            }
            ViewBag.Games = db.jntuh_college_sports.Where(cs => cs.collegeId == userCollegeID).
                            Join(db.jntuh_sports_type.Where(s => s.isActive == true),
                            cs => cs.sportsTypeId, s => s.id,
                            (cs, s) => new CollegeSports
                            {
                                id = cs.id,
                                collegeId = cs.collegeId,
                                sportsType = s.sportType,
                                sportsFacility = cs.sportsFacility,
                                createdBy = cs.createdBy,
                                createdOn = cs.createdOn,
                                updatedOn = cs.updatedOn,
                                updatedBy = cs.updatedBy
                            }).ToList();
            return View("Create", collegePlayground);            
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        [HttpPost]
        public ActionResult Edit(CollegePlayground collegePlayground)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                userCollegeID = collegePlayground.collegeId;
            }
            SaveCollegeSportsInformation(collegePlayground);
            return View("Create", collegePlayground);
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        [HttpGet]
        public ActionResult AddEditRecord(int? id,string collegeId)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                if (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry"))
                {
                    if (collegeId != null)
                    {
                        userCollegeID = Convert.ToInt32(Utilities.DecryptString(collegeId, WebConfigurationManager.AppSettings["CryptoKey"]));
                    }
                    else
                    {
                        userCollegeID = db.jntuh_college_sports.Where(cs => cs.id == id).Select(cs => cs.collegeId).FirstOrDefault();
                    }
                }
            }
            if (Request.IsAjaxRequest())
            {
                if (id != null)
                {
                    ViewBag.IsUpdate = true;
                    CollegeSports collegeSports = new CollegeSports();
                    jntuh_college_sports jntuh_college_sports = db.jntuh_college_sports.Where(s => s.collegeId == userCollegeID &&
                                                                                                   s.id == id).FirstOrDefault();
                    collegeSports.id = jntuh_college_sports.id;
                    collegeSports.collegeId = jntuh_college_sports.collegeId;
                    collegeSports.sportsTypeId = jntuh_college_sports.sportsTypeId;
                    collegeSports.sportsFacility = jntuh_college_sports.sportsFacility;
                    collegeSports.createdOn = jntuh_college_sports.createdOn;
                    collegeSports.createdBy = jntuh_college_sports.createdBy;
                    collegeSports.updatedBy = jntuh_college_sports.updatedBy;
                    collegeSports.updatedOn = jntuh_college_sports.updatedOn;
                    ViewBag.SportsType = db.jntuh_sports_type.Where(r => r.isActive == true);
                    return PartialView("_CreateCollegeSports", collegeSports);

                }
                else
                {
                    CollegeSports collegeSports = new CollegeSports();
                    collegeSports.collegeId = userCollegeID;
                    ViewBag.SportsType = db.jntuh_sports_type.Where(r => r.isActive == true);
                    return PartialView("_CreateCollegeSports", collegeSports);
                }
            }
            else
            {
                if (id != null)
                {
                    ViewBag.IsUpdate = true;
                    CollegeSports collegeSports = new CollegeSports();
                    jntuh_college_sports jntuh_college_sports = db.jntuh_college_sports.Where(s => s.collegeId == userCollegeID &&
                                                                                                   s.id == id).FirstOrDefault();
                    collegeSports.id = jntuh_college_sports.id;
                    collegeSports.collegeId = jntuh_college_sports.collegeId;
                    collegeSports.sportsTypeId = jntuh_college_sports.sportsTypeId;
                    collegeSports.sportsFacility = jntuh_college_sports.sportsFacility;
                    collegeSports.createdOn = jntuh_college_sports.createdOn;
                    collegeSports.createdBy = jntuh_college_sports.createdBy;
                    collegeSports.updatedBy = jntuh_college_sports.updatedBy;
                    collegeSports.updatedOn = jntuh_college_sports.updatedOn;
                    ViewBag.SportsType = db.jntuh_sports_type.Where(r => r.isActive == true);
                    return View("CreateCollegeSports", collegeSports);
                }
                else
                {
                    CollegeSports collegeSports = new CollegeSports();
                    collegeSports.collegeId = userCollegeID;
                    ViewBag.IsUpdate = false;
                    ViewBag.SportsType = db.jntuh_sports_type.Where(r => r.isActive == true);
                    return View("CreateCollegeSports", collegeSports);
                }
            }
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        [HttpPost]
        public ActionResult AddEditRecord(CollegeSports collegeSports, string cmd)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                userCollegeID = collegeSports.collegeId;
            }
            if (ModelState.IsValid)
            {
                collegeSports.collegeId = userCollegeID;
                if (cmd == "Add")
                {
                    var id = db.jntuh_college_sports.Where(s => s.collegeId == userCollegeID &&
                                                                s.sportsTypeId == collegeSports.sportsTypeId &&
                                                                s.sportsFacility == collegeSports.sportsFacility).Select(s => s.id).FirstOrDefault();

                    if (id > 0)
                    {
                        TempData["SportsError"] = "Sports Type And Sport Facility is already exists . Please enter a different Sports Type And Sport Facility";
                        ViewBag.SportsType = db.jntuh_sports_type.Where(r => r.isActive == true);
                        return RedirectToAction("Create", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
                    }
                    else
                    {
                        jntuh_college_sports jntuh_college_sports = new jntuh_college_sports();
                        jntuh_college_sports.id = collegeSports.id;
                        jntuh_college_sports.sportsTypeId = collegeSports.sportsTypeId;
                        jntuh_college_sports.collegeId = collegeSports.collegeId;
                        jntuh_college_sports.sportsFacility = collegeSports.sportsFacility;
                        jntuh_college_sports.createdBy = userID;
                        jntuh_college_sports.createdOn = DateTime.Now;
                        db.jntuh_college_sports.Add(jntuh_college_sports);
                        db.SaveChanges();
                        TempData["SportsSuccess"] = "Sports/Games Details Added successfully.";
                        ViewBag.SportsType = db.jntuh_sports_type.Where(r => r.isActive == true);
                        return RedirectToAction("Create", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
                    }
                }
                else
                {
                    var IdUpdate = db.jntuh_college_sports.Where(s => s.collegeId == userCollegeID &&
                                                                s.sportsTypeId == collegeSports.sportsTypeId &&
                                                                s.sportsFacility == collegeSports.sportsFacility &&
                                                                s.id != collegeSports.id).Select(s => s.id).FirstOrDefault();

                    if (IdUpdate > 0)
                    {
                        TempData["SportsError"] = "Sports Type And Sport Facility is already exists . Please enter a different Sports Type And Sport Facility";
                        ViewBag.SportsType = db.jntuh_sports_type.Where(r => r.isActive == true);
                        return RedirectToAction("Create", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
                    }
                    else
                    {
                        jntuh_college_sports jntuh_college_sports = new jntuh_college_sports();
                        jntuh_college_sports.id = collegeSports.id;
                        jntuh_college_sports.sportsTypeId = collegeSports.sportsTypeId;
                        jntuh_college_sports.collegeId = collegeSports.collegeId;
                        jntuh_college_sports.sportsFacility = collegeSports.sportsFacility;
                        jntuh_college_sports.createdBy = collegeSports.createdBy;
                        jntuh_college_sports.createdOn = collegeSports.createdOn;
                        jntuh_college_sports.updatedBy = userID;
                        jntuh_college_sports.updatedOn = DateTime.Now;
                        db.Entry(jntuh_college_sports).State = EntityState.Modified;
                        db.SaveChanges();
                        TempData["SportsSuccess"] = "Sports/Games Details Updated successfully.";
                        ViewBag.SportsType = db.jntuh_sports_type.Where(r => r.isActive == true);
                        return RedirectToAction("Create", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
                    }

                }

            }
            else
            {
                ViewBag.SportsType = db.jntuh_sports_type.Where(r => r.isActive == true);
                return RedirectToAction("Create", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
            }

        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult DeleteRecord(int id)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                userCollegeID = db.jntuh_college_sports.Where(s => s.id == id).Select(s=>s.collegeId).FirstOrDefault();
            }
            jntuh_college_sports jntuh_college_sports = db.jntuh_college_sports.Where(s => s.id == id).FirstOrDefault();
            if (jntuh_college_sports != null)
            {
                db.jntuh_college_sports.Remove(jntuh_college_sports);
                db.SaveChanges();
                TempData["SportsSuccess"] = "Sports/Games Details Are Deleted successfully.";
            }
            ViewBag.Games = db.jntuh_college_sports.Where(cs => cs.collegeId == userCollegeID).
                            Join(db.jntuh_sports_type.Where(s => s.isActive == true),
                            cs => cs.sportsTypeId, s => s.id,
                            (cs, s) => new CollegeSports
                            {
                                id = cs.id,
                                collegeId = cs.collegeId,
                                sportsType = s.sportType,
                                sportsFacility = cs.sportsFacility,
                                createdBy = cs.createdBy,
                                createdOn = cs.createdOn,
                                updatedOn = cs.updatedOn,
                                updatedBy = cs.updatedBy
                            }).ToList();
            ViewBag.SportsType = db.jntuh_sports_type.Where(r => r.isActive == true);
            return RedirectToAction("Create", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Details(int id)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                userCollegeID = db.jntuh_college_sports.Where(s => s.id == id).Select(s => s.collegeId).FirstOrDefault();
            }
            CollegeSports collegeSports = (from cp in db.jntuh_college_sports
                                           join s in db.jntuh_sports_type on cp.sportsTypeId equals s.id
                                           where (s.isActive == true && cp.id == id)
                                           orderby cp.id
                                           select new CollegeSports
                                           {
                                               id = cp.id,
                                               collegeId = cp.collegeId,
                                               sportsTypeId = cp.sportsTypeId,
                                               sportsType = s.sportType,
                                               sportsFacility = cp.sportsFacility,
                                               createdBy = cp.createdBy,
                                               createdOn = cp.createdOn,
                                               updatedOn = cp.updatedOn,
                                               updatedBy = cp.updatedBy
                                           }).FirstOrDefault();
            if (collegeSports != null)
            {
                if (Request.IsAjaxRequest())
                {
                    ViewBag.SportsType = db.jntuh_sports_type.Where(r => r.isActive == true);
                    return PartialView("_CreateCollegeSportsDetails", collegeSports);
                }
                else
                {
                    ViewBag.SportsType = db.jntuh_sports_type.Where(r => r.isActive == true);
                    return View("CreateCollegeSportsDetails", collegeSports);
                }
            }
           // return View("Create");
            return RedirectToAction("Create", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });

        }

        [Authorize(Roles = "Admin,College")]
        public ActionResult View(string id)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                userCollegeID = userCollegeID = Convert.ToInt32(Utilities.DecryptString(id, WebConfigurationManager.AppSettings["CryptoKey"]));          
            }
            int GroundTypeId = db.jntuh_college_desirable_others.Where(a => a.collegeId == userCollegeID).Select(a => a.id).FirstOrDefault();

          
            DateTime todayDate = DateTime.Now.Date;
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            int status = db.jntuh_college_edit_status.Where(editStatus => editStatus.academicyearId == prAy && editStatus.collegeId == userCollegeID &&
                                                                                      editStatus.IsCollegeEditable == true &&
                                                                                      editStatus.editFromDate <= todayDate &&
                                                                                      editStatus.editToDate >= todayDate)
                                                                 .Select(editStatus => editStatus.id)
                                                                 .FirstOrDefault();
            if (status > 0)
            {
                ViewBag.Status = true;
                bool isPageEditable = db.jntuh_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("EB") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

                if (isPageEditable)
                {
                    ViewBag.Status = true;
                }
                else
                {
                    ViewBag.Status = false;                   
                }
            }
            else
            {
                ViewBag.Status = false;
            }
            if (status > 0 && Roles.IsUserInRole("College"))
            {
                ViewBag.IsEditable = true;
                bool isPageEditable = db.jntuh_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("SG") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

                if (isPageEditable)
                {
                    ViewBag.IsEditable = true;
                }
                else
                {
                    ViewBag.IsEditable = false;
                }
            }
            else
            {
                ViewBag.IsEditable = false;
            }
             CollegePlayground collegePlayground = new CollegePlayground();
             if (GroundTypeId == 0)
             {
                 ViewBag.Norecords = true;
             }
             else
             {

                 jntuh_college_desirable_others jntuh_college_desirable_others = db.jntuh_college_desirable_others.Find(GroundTypeId);
                 if (jntuh_college_desirable_others != null)
                 {
                     collegePlayground.id = jntuh_college_desirable_others.id;
                     collegePlayground.collegeId = jntuh_college_desirable_others.collegeId;
                     collegePlayground.totalPlaygrounds = jntuh_college_desirable_others.totalPlaygrounds;

                     string[] selectedPlayGroundType = jntuh_college_desirable_others.playgroundType.Split('|').ToArray();

                     foreach (var type in playGroundTypes)
                     {
                         string strtype = type.id.ToString();
                         playGroundType.Add(new PlayGroundTypeModel { id = type.id, Name = type.Name, Checked = selectedPlayGroundType.Contains(strtype) ? 1 : 0 });
                     }

                     collegePlayground.GroundTypes = playGroundType;

                     string[] selectedTransportType = jntuh_college_desirable_others.modeOfTransport.Split('|').ToArray();

                     foreach (var type in transportMode)
                     {
                         string strtype = type.id.ToString();
                         transportModes.Add(new ModeOfTransportModel { id = type.id, Name = type.Name, Checked = selectedTransportType.Contains(strtype) ? 1 : 0 });
                     }

                     collegePlayground.TransportModes = transportModes;

                     string[] selectedPaymentType = jntuh_college_desirable_others.modeOfPayment.Split('|').ToArray();

                     foreach (var type in paymentMode)
                     {
                         string strtype = type.id.ToString();
                         paymentsModes.Add(new ModeOfPaymentModel { id = type.id, Name = type.Name, Checked = selectedPaymentType.Contains(strtype) ? 1 : 0 });
                     }

                     collegePlayground.PaymentModes = paymentsModes;
                     collegePlayground.numberOfBus = jntuh_college_desirable_others.numberOfBus;
                     collegePlayground.numberOfOtherVehicles = jntuh_college_desirable_others.numberOfOtherVehicles;
                     collegePlayground.createdBy = jntuh_college_desirable_others.createdBy;
                     collegePlayground.createdOn = jntuh_college_desirable_others.createdOn;
                     collegePlayground.updatedBy = jntuh_college_desirable_others.updatedBy;
                     collegePlayground.updatedOn = jntuh_college_desirable_others.updatedOn;
                 }
             }
            ViewBag.Games = db.jntuh_college_sports.Where(cs => cs.collegeId == userCollegeID).
                            Join(db.jntuh_sports_type.Where(s => s.isActive == true),
                            cs => cs.sportsTypeId, s => s.id,
                            (cs, s) => new CollegeSports
                            {
                                id = cs.id,
                                collegeId = cs.collegeId,
                                sportsType = s.sportType,
                                sportsFacility = cs.sportsFacility,
                                createdBy = cs.createdBy,
                                createdOn = cs.createdOn,
                                updatedOn = cs.updatedOn,
                                updatedBy = cs.updatedBy
                            }).ToList();            
            return View("View", collegePlayground);
        }

        public ActionResult UserView(string id)
        {
            int userCollegeID = Convert.ToInt32(Utilities.DecryptString(id, WebConfigurationManager.AppSettings["CryptoKey"]));
            int GroundTypeId = db.jntuh_college_desirable_others.Where(a => a.collegeId == userCollegeID).Select(a => a.id).FirstOrDefault();
            CollegePlayground collegePlayground = new CollegePlayground();
            if (GroundTypeId == 0)
            {
                ViewBag.Norecords = true;
            }
            else
            {
                jntuh_college_desirable_others jntuh_college_desirable_others = db.jntuh_college_desirable_others.Find(GroundTypeId);
                if (jntuh_college_desirable_others != null)
                {
                    collegePlayground.id = jntuh_college_desirable_others.id;
                    collegePlayground.collegeId = jntuh_college_desirable_others.collegeId;
                    collegePlayground.totalPlaygrounds = jntuh_college_desirable_others.totalPlaygrounds;

                    string[] selectedPlayGroundType = jntuh_college_desirable_others.playgroundType.Split('|').ToArray();

                    foreach (var type in playGroundTypes)
                    {
                        string strtype = type.id.ToString();
                        playGroundType.Add(new PlayGroundTypeModel { id = type.id, Name = type.Name, Checked = selectedPlayGroundType.Contains(strtype) ? 1 : 0 });
                    }

                    collegePlayground.GroundTypes = playGroundType;

                    string[] selectedTransportType = jntuh_college_desirable_others.modeOfTransport.Split('|').ToArray();

                    foreach (var type in transportMode)
                    {
                        string strtype = type.id.ToString();
                        transportModes.Add(new ModeOfTransportModel { id = type.id, Name = type.Name, Checked = selectedTransportType.Contains(strtype) ? 1 : 0 });
                    }

                    collegePlayground.TransportModes = transportModes;

                    string[] selectedPaymentType = jntuh_college_desirable_others.modeOfPayment.Split('|').ToArray();

                    foreach (var type in paymentMode)
                    {
                        string strtype = type.id.ToString();
                        paymentsModes.Add(new ModeOfPaymentModel { id = type.id, Name = type.Name, Checked = selectedPaymentType.Contains(strtype) ? 1 : 0 });
                    }
                    collegePlayground.PaymentModes = paymentsModes;
                    collegePlayground.numberOfBus = jntuh_college_desirable_others.numberOfBus;
                    collegePlayground.numberOfOtherVehicles = jntuh_college_desirable_others.numberOfOtherVehicles;
                    collegePlayground.createdBy = jntuh_college_desirable_others.createdBy;
                    collegePlayground.createdOn = jntuh_college_desirable_others.createdOn;
                    collegePlayground.updatedBy = jntuh_college_desirable_others.updatedBy;
                    collegePlayground.updatedOn = jntuh_college_desirable_others.updatedOn;
                }
            }
            List<CollegeSports> CollegeGames = db.jntuh_college_sports.Where(cs => cs.collegeId == userCollegeID).
                            Join(db.jntuh_sports_type.Where(s => s.isActive == true),
                            cs => cs.sportsTypeId, s => s.id,
                            (cs, s) => new CollegeSports
                            {
                                id = cs.id,
                                collegeId = cs.collegeId,
                                sportsType = s.sportType,
                                sportsFacility = cs.sportsFacility,
                                createdBy = cs.createdBy,
                                createdOn = cs.createdOn,
                                updatedOn = cs.updatedOn,
                                updatedBy = cs.updatedBy
                            }).ToList();
            ViewBag.Games = CollegeGames;
            ViewBag.Count = CollegeGames.Count();
            return View("UserView", collegePlayground);
        }

    }
}
