using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Security;
using UAAAS.Models;
using UAAAS.Models.Permanent_Affiliation;

namespace UAAAS.Controllers.Permanent_Affiliation
{
    [ErrorHandling]
    public class PA_SelfAppraisalController : BaseController
    {
        private readonly uaaasDBContext db = new uaaasDBContext();

        [Authorize(Roles = "Admin,SuperAdmin,College")]
        public ActionResult Index(string id)
        {
            var userdata = Membership.GetUser(User.Identity.Name);
            if (userdata == null)
            {
                return RedirectToAction("LogOn", "Account");
            }
            var userId = Convert.ToInt32(userdata.ProviderUserKey);
            var userCollegeId = db.jntuh_college_users.Where(u => u.userID == userId).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeId <= 0)
            {
                userCollegeId = Convert.ToInt32(Utilities.DecryptString(id, WebConfigurationManager.AppSettings["CryptoKey"]));
            }
            if (userCollegeId == 375)
            {
                userCollegeId = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            var status = GetPageEditableStatus(userCollegeId);
            if (status > 0 && Roles.IsUserInRole("College"))
            {
                ViewBag.IsEditable = true;
                var isPageEditable = db.jntuh_pa_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("PSA") && a.CollegeId == userCollegeId).Select(a => a.IsEditable).FirstOrDefault();
                ViewBag.IsEditable = isPageEditable;
            }
            else
            {
                ViewBag.IsEditable = false;
            }
            var collgeSelfAppraisals = db.jntuh_college_selfappraisal.Where(i => i.isactive && i.collegeid == userCollegeId).ToList();
            var lstSelfAppraisals = db.jntuh_selfappraisal.AsNoTracking().Where(i => i.selfappraisaldescriptiontype == 1 && i.isactive == true).Select(i => new SelfAppraisalModel
            {
                Id = i.id,
                Selfappraisaldescription = i.selfappraisaldescription,
                Selfappraisaldescriptiontype = (int)i.selfappraisaldescriptiontype,
            }).ToList();
            foreach (var lst in lstSelfAppraisals)
            {
                lst.CollegeSelfAppraisalsCount = collgeSelfAppraisals.Count(i => i.selfappraisalid == lst.Id);
            }
            //var academicYears = db.jntuh_academic_year.Where(i => i.id > 7 && i.id <= 11).Select(s => s).OrderByDescending(i => i.id).ToList();
            return View(lstSelfAppraisals);
        }

        [Authorize(Roles = "Admin,SuperAdmin,College")]
        [HttpGet]
        public ActionResult AddCollegeSelfAppraisal(int selfAppraisalId)
        {
            if (Membership.GetUser() == null)
            {
                return RedirectToAction("LogOn", "Account");
            }
            var membershipUser = Membership.GetUser(User.Identity.Name);
            var userCollegeId = 0;
            if (membershipUser != null)
            {
                var userId = Convert.ToInt32(membershipUser.ProviderUserKey);
                userCollegeId = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userId).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();
                if (userCollegeId == 375)
                {
                    userCollegeId = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
                }
                ViewBag.collegeId = Utilities.EncryptString(userCollegeId.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]);
            }
            ViewBag.AcademicYear = db.jntuh_academic_year.Where(i => i.actualYear > 2013).Select(s => s).OrderByDescending(i => i.id).ToList();
            if (userCollegeId == 0)
            {
                return RedirectToAction("LogOn", "Account");
            }
            var collegeSelfAppraisalModel = new CollegeSelfAppraisalModel
            {
                Selfappraisalid = selfAppraisalId,
                Collegeid = userCollegeId
            };
            return PartialView("AddCollegeSelfAppraisal", collegeSelfAppraisalModel);
        }

        [Authorize(Roles = "Admin,SuperAdmin,College")]
        [HttpPost]
        public ActionResult AddCollegeSelfAppraisal(CollegeSelfAppraisalModel collegeSelfAppraisal)
        {
            var membershipUser = Membership.GetUser(User.Identity.Name);
            if (membershipUser == null) return RedirectToAction("LogOn", "Account");
            var userId = Convert.ToInt32(membershipUser.ProviderUserKey);
            var jntuhregisteredFaculty = db.jntuh_registered_faculty.AsNoTracking().FirstOrDefault(i => collegeSelfAppraisal.Selfappraisaltype == 1 && i.RegistrationNumber == collegeSelfAppraisal.Registrationnumber.Trim());
            if (collegeSelfAppraisal.Selfappraisaltype == 1 && jntuhregisteredFaculty == null)
            {
                TempData["ERROR"] = collegeSelfAppraisal.Registrationnumber + " Invalid Registration. Please try again with correct registration number..";
            }
            else
            {
                SaveCollegeSelfAppraisal(collegeSelfAppraisal, userId, jntuhregisteredFaculty);
            }
            return RedirectToAction("Index");
        }

        public void SaveCollegeSelfAppraisal(CollegeSelfAppraisalModel collegeAppraisal, int userId, jntuh_registered_faculty faculty)
        {
            const string selfAppraisalSupprotingPath = "~/Content/Upload/College/CollegeSelfAppraisal";
            if (collegeAppraisal == null) return;
            var jntuhcollege = db.jntuh_college.AsNoTracking().FirstOrDefault(i => i.id == collegeAppraisal.Collegeid);
            var jntuhCollegeSelfappraisal = new jntuh_college_selfappraisal();
            if (collegeAppraisal.Suportingdocument != null)
            {
                if (!Directory.Exists(Server.MapPath(selfAppraisalSupprotingPath)))
                {
                    Directory.CreateDirectory(Server.MapPath(selfAppraisalSupprotingPath));
                }
                var ext = Path.GetExtension(collegeAppraisal.Suportingdocument.FileName);
                if (ext != null && ext.ToUpper().Equals(".PDF"))
                {
                    if (jntuhcollege != null)
                    {
                        var fileName = DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" + jntuhcollege.collegeCode + "_SelfAppraisal";
                        collegeAppraisal.Suportingdocument.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(selfAppraisalSupprotingPath),
                            fileName, ext));
                        collegeAppraisal.SuportingdocumentPath = string.Format("{0}{1}", fileName, ext);
                    }
                    jntuhCollegeSelfappraisal.suportingdocument = collegeAppraisal.SuportingdocumentPath;
                }
            }

            jntuhCollegeSelfappraisal = new jntuh_college_selfappraisal
            {
                academicyearId = collegeAppraisal.AcademicyearId,
                collegeid = collegeAppraisal.Collegeid,
                description = collegeAppraisal.Description,
                membername = (faculty != null && collegeAppraisal.Selfappraisaltype == 1) ? faculty.FirstName + " " + faculty.LastName : collegeAppraisal.Membername,
                registrationnumber = collegeAppraisal.Registrationnumber,
                selfappraisalid = collegeAppraisal.Selfappraisalid,
                selfappraisaltype = collegeAppraisal.Selfappraisaltype,
                suportingdocument = collegeAppraisal.SuportingdocumentPath,
                createdon = DateTime.Now,
                createdby = userId,
                updatedon = null,
                updatedby = null,
                isactive = true,
                remarks = null
            };
            try
            {
                db.jntuh_college_selfappraisal.Add(jntuhCollegeSelfappraisal);
                db.SaveChanges();
                TempData["SUCCESS"] = "Record Added Successfully";
            }
            catch (Exception e)
            {
                TempData["ERROR"] = e.Message;
                throw;
            }

        }

        [Authorize(Roles = "Admin,SuperAdmin,College")]
        [HttpGet]
        public ActionResult ViewCollegeSelfAppraisal(int selfAppraisalId)
        {
            var membershipUser = Membership.GetUser(User.Identity.Name);
            if (membershipUser == null) return RedirectToAction("LogOn", "Account");
            var userId = Convert.ToInt32(membershipUser.ProviderUserKey);
            var userCollegeId = db.jntuh_college_users.Where(u => u.userID == userId).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeId == 375)
            {
                userCollegeId = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            var status = GetPageEditableStatus(userCollegeId);
            if (status > 0 && Roles.IsUserInRole("College"))
            {
                ViewBag.IsEditable = true;
                var isPageEditable = db.jntuh_pa_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("PSA") && a.CollegeId == userCollegeId).Select(a => a.IsEditable).FirstOrDefault();
                ViewBag.IsEditable = isPageEditable;
            }
            else
            {
                ViewBag.IsEditable = false;
            }
            var academicYears = db.jntuh_academic_year.Select(s => s).OrderByDescending(i => i.id).ToList();
            var collgeSelfAppraisals = db.jntuh_college_selfappraisal.Where(i => i.isactive && i.collegeid == userCollegeId && i.selfappraisalid == selfAppraisalId).ToList();
            var collegelstSelfAppraisals = collgeSelfAppraisals.Select(i =>
            {
                var jntuhAcademicYear = academicYears.FirstOrDefault(a => a.id == i.academicyearId);
                return jntuhAcademicYear != null ? new CollegeSelfAppraisalModel
                {
                    Id = i.id,
                    AcademicyearId = i.academicyearId,
                    AcademicYear = jntuhAcademicYear.academicYear,
                    Registrationnumber = i.registrationnumber,
                    Collegeid = i.collegeid,
                    Description = i.description,
                    Membername = i.membername,
                    SuportingdocumentPath = i.suportingdocument,
                    Selfappraisaltype = i.selfappraisaltype,
                    Selfappraisalid = i.selfappraisalid,
                    Createdon = i.createdon
                } : null;
            }).ToList();
            return PartialView(collegelstSelfAppraisals);
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult DeleteSelfAppraisal(string id)
        {
            var userdata = Membership.GetUser(User.Identity.Name);
            if (userdata == null)
            {
                return RedirectToAction("LogOn", "Account");
            }
            if (!string.IsNullOrEmpty(id))
            {
                var facultytracking = db.jntuh_college_selfappraisal.Find(Convert.ToInt32(id));
                db.jntuh_college_selfappraisal.Remove(facultytracking);
                db.SaveChanges();
                if (true)
                {
                    TempData["SUCCESS"] = "Record Deleted Successfully";
                }
            }
            else
            {
                TempData["ERROR"] = "Self Appraisal Not Found.Please try again..";
            }
            return RedirectToAction("Index");
        }

        #region Self appraisal other than common type

        [Authorize(Roles = "Admin,SuperAdmin,College")]
        public ActionResult SelfOthersIndex(string id)
        {
            var userdata = Membership.GetUser(User.Identity.Name);
            if (userdata == null)
            {
                return RedirectToAction("LogOn", "Account");
            }
            var userId = Convert.ToInt32(userdata.ProviderUserKey);
            var userCollegeId = db.jntuh_college_users.Where(u => u.userID == userId).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeId <= 0)
            {
                userCollegeId = Convert.ToInt32(Utilities.DecryptString(id, WebConfigurationManager.AppSettings["CryptoKey"]));
            }
            if (userCollegeId == 375)
            {
                userCollegeId = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            var status = GetPageEditableStatus(userCollegeId);
            if (status > 0 && Roles.IsUserInRole("College"))
            {
                ViewBag.IsEditable = true;
                var isPageEditable = db.jntuh_pa_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("PSA") && a.CollegeId == userCollegeId).Select(a => a.IsEditable).FirstOrDefault();
                ViewBag.IsEditable = isPageEditable;
            }
            else
            {
                ViewBag.IsEditable = false;
            }
            var collgeSelfAppraisals = db.jntuh_college_selfappraisal.Where(i => i.isactive && i.collegeid == userCollegeId).ToList();
            var lstSelfAppraisals = db.jntuh_selfappraisal.AsNoTracking().Where(i => i.selfappraisaldescriptiontype == 2 && i.isactive == true).Select(i => new SelfAppraisalModel
            {
                Id = i.id,
                Selfappraisaldescription = i.selfappraisaldescription,
                Selfappraisaldescriptiontype = (int)i.selfappraisaldescriptiontype,
            }).ToList();
            foreach (var lst in lstSelfAppraisals)
            {
                lst.CollegeSelfAppraisalsCount = collgeSelfAppraisals.Count(i => i.selfappraisalid == lst.Id);
            }
            //var academicYears = db.jntuh_academic_year.Where(i => i.id > 7 && i.id <= 11).Select(s => s).OrderByDescending(i => i.id).ToList();
            return View(lstSelfAppraisals);
        }

        [Authorize(Roles = "Admin,SuperAdmin,College")]
        [HttpGet]
        public ActionResult AddCollegeOtherSelfAppraisal(int selfAppraisalId)
        {
            if (Membership.GetUser() == null)
            {
                return RedirectToAction("LogOn", "Account");
            }
            var membershipUser = Membership.GetUser(User.Identity.Name);
            var userCollegeId = 0;
            if (membershipUser != null)
            {
                var userId = Convert.ToInt32(membershipUser.ProviderUserKey);
                userCollegeId = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userId).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();
                if (userCollegeId == 375)
                {
                    userCollegeId = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
                }
                ViewBag.collegeId = Utilities.EncryptString(userCollegeId.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]);
            }
            ViewBag.AcademicYear = db.jntuh_academic_year.Where(i => i.actualYear > 2013).Select(s => s).OrderByDescending(i => i.id).ToList();
            if (userCollegeId == 0)
            {
                return RedirectToAction("LogOn", "Account");
            }
            var collegeSelfAppraisalModel = new OtherCollegeSelfAppraisalModel
            {
                Selfappraisalid = selfAppraisalId,
                Collegeid = userCollegeId
            };
            return PartialView("AddCollegeOtherSelfAppraisal", collegeSelfAppraisalModel);
        }

        [Authorize(Roles = "Admin,SuperAdmin,College")]
        [HttpPost]
        public ActionResult AddCollegeOtherSelfAppraisal(OtherCollegeSelfAppraisalModel collegeSelfAppraisal)
        {
            var membershipUser = Membership.GetUser(User.Identity.Name);
            if (membershipUser == null) return RedirectToAction("LogOn", "Account");
            var userId = Convert.ToInt32(membershipUser.ProviderUserKey);
            var jntuhregisteredFaculty = db.jntuh_registered_faculty.AsNoTracking().FirstOrDefault(i => i.RegistrationNumber == collegeSelfAppraisal.FacultyRegistrationNumber.Trim());
            if (jntuhregisteredFaculty == null)
            {
                TempData["ERROR"] = collegeSelfAppraisal.FacultyRegistrationNumber + " Invalid Registration. Please try again with correct registration number..";
            }
            else
            {
                SaveCollegeOtherSelfAppraisal(collegeSelfAppraisal, userId, jntuhregisteredFaculty);
            }
            return RedirectToAction("SelfOthersIndex");
        }

        public void SaveCollegeOtherSelfAppraisal(OtherCollegeSelfAppraisalModel collegeAppraisal, int userId, jntuh_registered_faculty faculty)
        {
            const string selfAppraisalSupprotingPath = "~/Content/Upload/College/CollegeResearchGrantSelfAppraisal";
            if (collegeAppraisal == null) return;
            var jntuhcollege = db.jntuh_college.AsNoTracking().FirstOrDefault(i => i.id == collegeAppraisal.Collegeid);
            var jntuhCollegeSelfappraisal = new jntuh_college_selfappraisal();
            if (collegeAppraisal.Suportingdocument != null)
            {
                if (!Directory.Exists(Server.MapPath(selfAppraisalSupprotingPath)))
                {
                    Directory.CreateDirectory(Server.MapPath(selfAppraisalSupprotingPath));
                }
                var ext = Path.GetExtension(collegeAppraisal.Suportingdocument.FileName);
                if (ext != null && ext.ToUpper().Equals(".PDF"))
                {
                    if (jntuhcollege != null)
                    {
                        var fileName = DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" + jntuhcollege.collegeCode + "_SelfAppraisal";
                        collegeAppraisal.Suportingdocument.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(selfAppraisalSupprotingPath),
                            fileName, ext));
                        collegeAppraisal.SuportingdocumentPath = string.Format("{0}{1}", fileName, ext);
                    }
                    jntuhCollegeSelfappraisal.suportingdocument = collegeAppraisal.SuportingdocumentPath;
                }
            }

            jntuhCollegeSelfappraisal = new jntuh_college_selfappraisal
            {
                academicyearId = collegeAppraisal.AcademicyearId,
                collegeid = collegeAppraisal.Collegeid,
                description = collegeAppraisal.Description,
                membername = (faculty != null) ? faculty.FirstName + " " + faculty.LastName : collegeAppraisal.Membername,
                registrationnumber = collegeAppraisal.FacultyRegistrationNumber,
                selfappraisalid = collegeAppraisal.Selfappraisalid,
                selfappraisaltype = 1, //Faculty
                suportingdocument = collegeAppraisal.SuportingdocumentPath,
                grantamount = !string.IsNullOrEmpty(collegeAppraisal.Grantamount) ? Convert.ToDecimal(collegeAppraisal.Grantamount) : 0,
                fundingagency = collegeAppraisal.Fundingagency,
                createdon = DateTime.Now,
                createdby = userId,
                updatedon = null,
                updatedby = null,
                isactive = true,
                remarks = null
            };
            try
            {
                db.jntuh_college_selfappraisal.Add(jntuhCollegeSelfappraisal);
                db.SaveChanges();
                TempData["SUCCESS"] = "Record Added Successfully";
            }
            catch (Exception e)
            {
                TempData["ERROR"] = e.Message;
                throw;
            }

        }

        [Authorize(Roles = "Admin,SuperAdmin,College")]
        [HttpGet]
        public ActionResult ViewCollegeOtherSelfAppraisal(int selfAppraisalId)
        {
            var membershipUser = Membership.GetUser(User.Identity.Name);
            if (membershipUser == null) return RedirectToAction("LogOn", "Account");
            var userId = Convert.ToInt32(membershipUser.ProviderUserKey);
            var userCollegeId = db.jntuh_college_users.Where(u => u.userID == userId).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeId == 375)
            {
                userCollegeId = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            var status = GetPageEditableStatus(userCollegeId);
            if (status > 0 && Roles.IsUserInRole("College"))
            {
                ViewBag.IsEditable = true;
                var isPageEditable = db.jntuh_pa_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("PSA") && a.CollegeId == userCollegeId).Select(a => a.IsEditable).FirstOrDefault();
                ViewBag.IsEditable = isPageEditable;
            }
            else
            {
                ViewBag.IsEditable = false;
            }
            var academicYears = db.jntuh_academic_year.Select(s => s).OrderByDescending(i => i.id).ToList();
            var collgeSelfAppraisals = db.jntuh_college_selfappraisal.Where(i => i.isactive && i.collegeid == userCollegeId && i.selfappraisalid == selfAppraisalId).ToList();
            var collegelstSelfAppraisals = collgeSelfAppraisals.Select(i =>
            {
                var jntuhAcademicYear = academicYears.FirstOrDefault(a => a.id == i.academicyearId);
                return jntuhAcademicYear != null ? new OtherCollegeSelfAppraisalModel
                {
                    Id = i.id,
                    AcademicyearId = i.academicyearId,
                    AcademicYear = jntuhAcademicYear.academicYear,
                    FacultyRegistrationNumber = i.registrationnumber,
                    Collegeid = i.collegeid,
                    Description = i.description,
                    Membername = i.membername,
                    SuportingdocumentPath = i.suportingdocument,
                    Selfappraisaltype = i.selfappraisaltype,
                    Selfappraisalid = i.selfappraisalid,
                    Grantamount = i.grantamount.ToString(),
                    Fundingagency = i.fundingagency,
                    Createdon = i.createdon
                } : null;
            }).ToList();
            return PartialView(collegelstSelfAppraisals);
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult DeleteOtherSelfAppraisal(string id)
        {
            var userdata = Membership.GetUser(User.Identity.Name);
            if (userdata == null)
            {
                return RedirectToAction("LogOn", "Account");
            }
            if (!string.IsNullOrEmpty(id))
            {
                var facultytracking = db.jntuh_college_selfappraisal.Find(Convert.ToInt32(id));
                db.jntuh_college_selfappraisal.Remove(facultytracking);
                db.SaveChanges();
                if (true)
                {
                    TempData["SUCCESS"] = "Record Deleted Successfully";
                }
            }
            else
            {
                TempData["ERROR"] = "Self Appraisal Not Found.Please try again..";
            }
            return RedirectToAction("SelfOthersIndex");
        }

        #endregion

        #region Self MoU

        [Authorize(Roles = "Admin,SuperAdmin,College")]
        public ActionResult SelfMoUIndex(string id)
        {
            var userdata = Membership.GetUser(User.Identity.Name);
            if (userdata == null)
            {
                return RedirectToAction("LogOn", "Account");
            }
            var userId = Convert.ToInt32(userdata.ProviderUserKey);
            var userCollegeId = db.jntuh_college_users.Where(u => u.userID == userId).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeId <= 0)
            {
                userCollegeId = Convert.ToInt32(Utilities.DecryptString(id, WebConfigurationManager.AppSettings["CryptoKey"]));
            }
            if (userCollegeId == 375)
            {
                userCollegeId = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            var status = GetPageEditableStatus(userCollegeId);
            if (status > 0 && Roles.IsUserInRole("College"))
            {
                ViewBag.IsEditable = true;
                var isPageEditable = db.jntuh_pa_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("PSA") && a.CollegeId == userCollegeId).Select(a => a.IsEditable).FirstOrDefault();
                ViewBag.IsEditable = isPageEditable;
            }
            else
            {
                ViewBag.IsEditable = false;
            }
            var collgeSelfAppraisals = db.jntuh_college_selfappraisal.Where(i => i.isactive && i.collegeid == userCollegeId).ToList();
            var lstSelfAppraisals = db.jntuh_selfappraisal.AsNoTracking().Where(i => i.selfappraisaldescriptiontype == 3 && i.isactive == true).Select(i => new SelfAppraisalModel
            {
                Id = i.id,
                Selfappraisaldescription = i.selfappraisaldescription,
                Selfappraisaldescriptiontype = (int)i.selfappraisaldescriptiontype,
            }).ToList();
            foreach (var lst in lstSelfAppraisals)
            {
                lst.CollegeSelfAppraisalsCount = collgeSelfAppraisals.Count(i => i.selfappraisalid == lst.Id);
            }
            //var academicYears = db.jntuh_academic_year.Where(i => i.id > 7 && i.id <= 11).Select(s => s).OrderByDescending(i => i.id).ToList();
            return View(lstSelfAppraisals);
        }

        [Authorize(Roles = "Admin,SuperAdmin,College")]
        [HttpGet]
        public ActionResult AddCollegeMoUSelfAppraisal(int selfAppraisalId)
        {
            if (Membership.GetUser() == null)
            {
                return RedirectToAction("LogOn", "Account");
            }
            var membershipUser = Membership.GetUser(User.Identity.Name);
            var userCollegeId = 0;
            if (membershipUser != null)
            {
                var userId = Convert.ToInt32(membershipUser.ProviderUserKey);
                userCollegeId = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userId).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();
                if (userCollegeId == 375)
                {
                    userCollegeId = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
                }
                ViewBag.collegeId = Utilities.EncryptString(userCollegeId.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]);
            }
            ViewBag.AcademicYear = db.jntuh_academic_year.Where(i => i.actualYear > 2013).Select(s => s).OrderByDescending(i => i.id).ToList();
            if (userCollegeId == 0)
            {
                return RedirectToAction("LogOn", "Account");
            }
            var collegeSelfAppraisalModel = new OtherCollegeSelfAppraisalModel
            {
                Selfappraisalid = selfAppraisalId,
                Collegeid = userCollegeId
            };
            return PartialView("AddCollegeMoUSelfAppraisal", collegeSelfAppraisalModel);
        }

        [Authorize(Roles = "Admin,SuperAdmin,College")]
        [HttpPost]
        public ActionResult AddCollegeMoUSelfAppraisal(OtherCollegeSelfAppraisalModel collegeSelfAppraisal)
        {
            var membershipUser = Membership.GetUser(User.Identity.Name);
            if (membershipUser == null) return RedirectToAction("LogOn", "Account");
            var userId = Convert.ToInt32(membershipUser.ProviderUserKey);
            var jntuhregisteredFaculty = db.jntuh_registered_faculty.AsNoTracking().FirstOrDefault(i => i.RegistrationNumber == collegeSelfAppraisal.FacultyRegistrationNumber.Trim());
            if (jntuhregisteredFaculty == null)
            {
                TempData["ERROR"] = collegeSelfAppraisal.FacultyRegistrationNumber + " Invalid Registration. Please try again with correct registration number..";
            }
            else
            {
                SaveCollegeOtherSelfAppraisal(collegeSelfAppraisal, userId, jntuhregisteredFaculty);
            }
            return RedirectToAction("SelfMoUIndex");
        }

        public void SaveCollegeMoUSelfAppraisal(OtherCollegeSelfAppraisalModel collegeAppraisal, int userId, jntuh_registered_faculty faculty)
        {
            const string selfAppraisalSupprotingPath = "~/Content/Upload/College/CollegeMoUSelfAppraisal";
            if (collegeAppraisal == null) return;
            var jntuhcollege = db.jntuh_college.AsNoTracking().FirstOrDefault(i => i.id == collegeAppraisal.Collegeid);
            var jntuhCollegeSelfappraisal = new jntuh_college_selfappraisal();
            if (collegeAppraisal.Suportingdocument != null)
            {
                if (!Directory.Exists(Server.MapPath(selfAppraisalSupprotingPath)))
                {
                    Directory.CreateDirectory(Server.MapPath(selfAppraisalSupprotingPath));
                }
                var ext = Path.GetExtension(collegeAppraisal.Suportingdocument.FileName);
                if (ext != null && ext.ToUpper().Equals(".PDF"))
                {
                    if (jntuhcollege != null)
                    {
                        var fileName = DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" + jntuhcollege.collegeCode + "_SelfAppraisal";
                        collegeAppraisal.Suportingdocument.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(selfAppraisalSupprotingPath),
                            fileName, ext));
                        collegeAppraisal.SuportingdocumentPath = string.Format("{0}{1}", fileName, ext);
                    }
                    jntuhCollegeSelfappraisal.suportingdocument = collegeAppraisal.SuportingdocumentPath;
                }
            }

            jntuhCollegeSelfappraisal = new jntuh_college_selfappraisal
            {
                academicyearId = collegeAppraisal.AcademicyearId,
                collegeid = collegeAppraisal.Collegeid,
                description = collegeAppraisal.Description,
                membername = (faculty != null) ? faculty.FirstName + " " + faculty.LastName : collegeAppraisal.Membername,
                registrationnumber = collegeAppraisal.FacultyRegistrationNumber,
                selfappraisalid = collegeAppraisal.Selfappraisalid,
                selfappraisaltype = 1, // Faculty
                suportingdocument = collegeAppraisal.SuportingdocumentPath,
                grantamount = !string.IsNullOrEmpty(collegeAppraisal.Grantamount) ? Convert.ToDecimal(collegeAppraisal.Grantamount) : 0,
                fundingagency = collegeAppraisal.Fundingagency,
                createdon = DateTime.Now,
                createdby = userId,
                updatedon = null,
                updatedby = null,
                isactive = true,
                remarks = null
            };
            try
            {
                db.jntuh_college_selfappraisal.Add(jntuhCollegeSelfappraisal);
                db.SaveChanges();
                TempData["SUCCESS"] = "Record Added Successfully";
            }
            catch (Exception e)
            {
                TempData["ERROR"] = e.Message;
                throw;
            }

        }

        [Authorize(Roles = "Admin,SuperAdmin,College")]
        [HttpGet]
        public ActionResult ViewCollegeMoUSelfAppraisal(int selfAppraisalId)
        {
            var membershipUser = Membership.GetUser(User.Identity.Name);
            if (membershipUser == null) return RedirectToAction("LogOn", "Account");
            var userId = Convert.ToInt32(membershipUser.ProviderUserKey);
            var userCollegeId = db.jntuh_college_users.Where(u => u.userID == userId).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeId == 375)
            {
                userCollegeId = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            var status = GetPageEditableStatus(userCollegeId);
            if (status > 0 && Roles.IsUserInRole("College"))
            {
                ViewBag.IsEditable = true;
                var isPageEditable = db.jntuh_pa_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("PSA") && a.CollegeId == userCollegeId).Select(a => a.IsEditable).FirstOrDefault();
                ViewBag.IsEditable = isPageEditable;
            }
            else
            {
                ViewBag.IsEditable = false;
            }
            var academicYears = db.jntuh_academic_year.Select(s => s).OrderByDescending(i => i.id).ToList();
            var collgeSelfAppraisals = db.jntuh_college_selfappraisal.Where(i => i.isactive && i.collegeid == userCollegeId && i.selfappraisalid == selfAppraisalId).ToList();
            var collegelstSelfAppraisals = collgeSelfAppraisals.Select(i =>
            {
                var jntuhAcademicYear = academicYears.FirstOrDefault(a => a.id == i.academicyearId);
                return jntuhAcademicYear != null ? new OtherCollegeSelfAppraisalModel
                {
                    Id = i.id,
                    AcademicyearId = i.academicyearId,
                    AcademicYear = jntuhAcademicYear.academicYear,
                    FacultyRegistrationNumber = i.registrationnumber,
                    Collegeid = i.collegeid,
                    Description = i.description,
                    Membername = i.membername,
                    SuportingdocumentPath = i.suportingdocument,
                    Selfappraisaltype = i.selfappraisaltype,
                    Selfappraisalid = i.selfappraisalid,
                    Grantamount = i.grantamount.ToString(),
                    Fundingagency = i.fundingagency,
                    Createdon = i.createdon
                } : null;
            }).ToList();
            return PartialView(collegelstSelfAppraisals);
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult DeleteMoUSelfAppraisal(string id)
        {
            var userdata = Membership.GetUser(User.Identity.Name);
            if (userdata == null)
            {
                return RedirectToAction("LogOn", "Account");
            }
            if (!string.IsNullOrEmpty(id))
            {
                var facultytracking = db.jntuh_college_selfappraisal.Find(Convert.ToInt32(id));
                db.jntuh_college_selfappraisal.Remove(facultytracking);
                db.SaveChanges();
                if (true)
                {
                    TempData["SUCCESS"] = "Record Deleted Successfully";
                }
            }
            else
            {
                TempData["ERROR"] = "Self Appraisal Not Found.Please try again..";
            }
            return RedirectToAction("SelfMoUIndex");
        }

        #endregion

    }
}
