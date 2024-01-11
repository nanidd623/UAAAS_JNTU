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
    public class PA_FacultyOppurtunitiesController : BaseController
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

                bool isPageEditable = db.jntuh_pa_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("PFO") && a.CollegeId == userCollegeId).Select(a => a.IsEditable).FirstOrDefault();

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
            var collgeSelfAppraisals = db.jntuh_college_selfappraisal.Where(i => i.isactive && i.collegeid == userCollegeId).ToList();
            var lstSelfAppraisals = db.jntuh_selfappraisal.AsNoTracking().Where(i => i.selfappraisaldescriptiontype == 10 && i.isactive == true).Select(i => new SelfAppraisalModel
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
            var facultyOppurtunityModel = new FacultyOppurtunityModel
            {
                Selfappraisalid = selfAppraisalId,
                Collegeid = userCollegeId
            };
            return PartialView("AddCollegeSelfAppraisal", facultyOppurtunityModel);
        }

        [Authorize(Roles = "Admin,SuperAdmin,College")]
        [HttpPost]
        public ActionResult AddCollegeSelfAppraisal(FacultyOppurtunityModel collegeSelfAppraisal)
        {
            var membershipUser = Membership.GetUser(User.Identity.Name);
            if (membershipUser == null) return RedirectToAction("LogOn", "Account");
            var userId = Convert.ToInt32(membershipUser.ProviderUserKey);
            var jntuhregisteredFaculty = db.jntuh_registered_faculty.AsNoTracking().FirstOrDefault(i => i.RegistrationNumber == collegeSelfAppraisal.FacultyRegistrationNumber.Trim());
            if (collegeSelfAppraisal.Selfappraisaltype == 1 && jntuhregisteredFaculty == null)
            {
                TempData["ERROR"] = collegeSelfAppraisal.FacultyRegistrationNumber + " Invalid Registration. Please try again with correct registration number..";
            }
            else
            {
                SaveCollegeSelfAppraisal(collegeSelfAppraisal, userId, jntuhregisteredFaculty);
            }
            return RedirectToAction("Index");
        }

        public void SaveCollegeSelfAppraisal(FacultyOppurtunityModel collegeAppraisal, int userId, jntuh_registered_faculty faculty)
        {
            const string selfAppraisalSupprotingPath = "~/Content/Upload/College/CollegeFacultyOpportunities";
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

                bool isPageEditable = db.jntuh_pa_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("PFO") && a.CollegeId == userCollegeId).Select(a => a.IsEditable).FirstOrDefault();

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
            var academicYears = db.jntuh_academic_year.Select(s => s).OrderByDescending(i => i.id).ToList();
            var collgeSelfAppraisals = db.jntuh_college_selfappraisal.Where(i => i.isactive && i.collegeid == userCollegeId && i.selfappraisalid == selfAppraisalId).ToList();
            var collegelstSelfAppraisals = collgeSelfAppraisals.Select(i =>
            {
                var jntuhAcademicYear = academicYears.FirstOrDefault(a => a.id == i.academicyearId);
                return jntuhAcademicYear != null ? new FacultyOppurtunityModel
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

        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin,College,DataEntry")]
        public JsonResult CheckRegistrationNumber(string FacultyRegistrationNumber)
        {
            var userdata = Membership.GetUser(User.Identity.Name);
            if (userdata == null)
            {
                return Json("Invalid Registration.", JsonRequestBehavior.AllowGet);
            }
            var userId = Convert.ToInt32(userdata.ProviderUserKey);
            var userCollegeId = db.jntuh_college_users.Where(u => u.userID == userId).Select(u => u.collegeID).FirstOrDefault();
            var RegistrationNumber = db.jntuh_registered_faculty.Where(F => F.RegistrationNumber == FacultyRegistrationNumber.Trim()).Select(s => s).FirstOrDefault();
            if (RegistrationNumber != null)
            {
                var isExistingFaculty = db.jntuh_college_faculty_registered.Where(r => r.RegistrationNumber == FacultyRegistrationNumber.Trim() && r.collegeId == userCollegeId).Select(r => r.RegistrationNumber).FirstOrDefault();
                if (isExistingFaculty == null)
                {
                    return Json("Faculty doesn't belong to your college.", JsonRequestBehavior.AllowGet);
                }
                else
                    return Json(true);
            }
            else
                return Json("Invalid Registration.", JsonRequestBehavior.AllowGet);
        }
    }
}
