using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Security;
using iTextSharp.text.html;
using iTextSharp.text.pdf;
using UAAAS.Models;
using System.Threading;
using iTextSharp.text;
using iTextSharp.text.html.simpleparser;

namespace UAAAS.Controllers.Permanent_Affiliation
{
    [ErrorHandling]
    public class PA_FacultyController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();

        [Authorize(Roles = "College")]
        public ActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Teaching(string collegeId)
        {
            //if (!User.IsInRole("Admin"))
            //{
            //    return RedirectToAction("Index", "UnderConstruction");
            //}        
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userID).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy =
                db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            if (userCollegeID == 0 && collegeId != null)
            {
                if (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry"))
                {
                    userCollegeID = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(collegeId, WebConfigurationManager.AppSettings["CryptoKey"]));
                }
            }

            ViewBag.collegeId = UAAAS.Models.Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString();
            DateTime todayDate = DateTime.Now.Date;

            var status = GetPageEditableStatus(userCollegeID);

            if (status == 0 && Roles.IsUserInRole("College"))
            {
                //return RedirectToAction("College", "Dashboard");
                return RedirectToAction("ViewTeaching");
            }
            bool isPageEditable = db.jntuh_pa_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("PFA") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

            if (!isPageEditable)
            {
                return RedirectToAction("ViewTeaching");
            }
            return View();
        }


        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        [HttpPost]
        public JsonResult TeachingJson(string collegeId)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userID).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();
            // userCollegeID = 263;
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            if (userCollegeID == 0 && collegeId != null)
            {
                if (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry"))
                {
                    userCollegeID = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(collegeId, WebConfigurationManager.AppSettings["CryptoKey"]));
                }
            }
            ViewBag.collegeId = UAAAS.Models.Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString();

            // Below Code is Added By Naushad Khan
            int type = db.jntuh_faculty_type.Where(t => t.facultyType == "Teaching").Select(t => t.id).FirstOrDefault();
            List<CollegeFaculty> newteachingFaculty = new List<CollegeFaculty>();


            #region
            //var TeachingFacultyData = (from t in db.jntuh_registered_faculty
            //                           join cf in db.jntuh_college_faculty_registered on t.RegistrationNumber equals cf.RegistrationNumber
            //                           join ds in db.jntuh_designation on t.DesignationId equals ds.id
            //                           join dp in db.jntuh_department on cf.DepartmentId equals dp.id
            //                           where cf.collegeId == userCollegeID && t.Notin116 != true
            //                           select new
            //                           {
            //                               t.FirstName,
            //                               t.MiddleName,
            //                               t.LastName,
            //                               t.RegistrationNumber,
            //                               t.id,
            //                               collegeid=cf.id,
            //                               depId =  cf.DepartmentId,
            //                               specId= cf.SpecializationId,
            //                               IdentfdFor = cf.IdentifiedFor,
            //                               t.DesignationId,
            //                               t.DepartmentId,
            //                               t.Absent,
            //                               t.NotQualifiedAsperAICTE,
            //                               t.PANNumber,
            //                               t.NoSCM,
            //                               t.PHDundertakingnotsubmitted,
            //                               t.Blacklistfaculy,
            //                               ds.designation,
            //                             depName=   dp.departmentName
            //                           }).ToList();

            #endregion


            var jntuh_departments = db.jntuh_department.AsNoTracking().ToList();
            var jntuh_designations = db.jntuh_designation.AsNoTracking().ToList();
            var jntuh_specializations = db.jntuh_specialization.AsNoTracking().ToList();









            //var TeachingFacultyData = (from cf in db.jntuh_college_faculty_registered.AsNoTracking()
            //                           join t in db.jntuh_registered_faculty.AsNoTracking() on cf.RegistrationNumber equals t.RegistrationNumber
            //                           where cf.collegeId == userCollegeID
            //                           select new
            //                           {
            //                               t.FirstName,
            //                               t.MiddleName,
            //                               t.LastName,
            //                               t.RegistrationNumber,
            //                               t.id,
            //                               collegeid = cf.id,
            //                               depId = cf.DepartmentId,
            //                               specId = cf.SpecializationId,
            //                               IdentfdFor = cf.IdentifiedFor,
            //                               t.DesignationId,
            //                               cf.DepartmentId,
            //                               t.Absent,
            //                               t.NotQualifiedAsperAICTE,
            //                               t.PANNumber,
            //                               t.NoSCM,
            //                               t.PHDundertakingnotsubmitted,
            //                               t.Blacklistfaculy,

            //                           }).ToList();


            var TeachingFacultyData = db.jntuh_college_faculty_registered.Join(db.jntuh_registered_faculty,
                    CLGREG => CLGREG.RegistrationNumber, REG => REG.RegistrationNumber,
                    (CLGREG, REG) => new { CLGREG = CLGREG, REG = REG }).Where(e => e.CLGREG.collegeId == userCollegeID).Select(e => new
                    {
                        e.REG.FirstName,
                        e.REG.MiddleName,
                        e.REG.LastName,
                        e.REG.RegistrationNumber,
                        e.REG.id,
                        collegeid = e.CLGREG.id,
                        depId = e.CLGREG.DepartmentId,
                        Aadhaarno = e.CLGREG.AadhaarNumber,
                        AadhaarDoc = e.CLGREG.AadhaarDocument,
                        specId = e.CLGREG.SpecializationId,
                        IdentfdFor = e.CLGREG.IdentifiedFor,
                        e.REG.DesignationId,
                        e.CLGREG.DepartmentId,
                        e.REG.Absent,
                        e.REG.NotQualifiedAsperAICTE,
                        e.REG.PANNumber,
                        e.REG.NoSCM,
                        e.REG.PHDundertakingnotsubmitted,
                        e.REG.Blacklistfaculy
                    }).ToList();








            foreach (var data in TeachingFacultyData)
            {
                CollegeFaculty newcollegeFaculty = new CollegeFaculty();
                newcollegeFaculty.FacultyRegistrationNumber = data.RegistrationNumber;
                newcollegeFaculty.facultyFirstName = data.FirstName + " " + data.MiddleName + " " + data.LastName;
                if (data.Blacklistfaculy == true)
                {
                    newcollegeFaculty.facultyFirstName = newcollegeFaculty.facultyFirstName + "<span style='color: red'>   (Blacklist)</span>";
                }
                newcollegeFaculty.facultyLastName = data.LastName;
                newcollegeFaculty.SpecializationId = data.specId;
                newcollegeFaculty.facultyAadhaarNumber = data.Aadhaarno;
                newcollegeFaculty.facultyAadharDocument = data.AadhaarDoc;
                newcollegeFaculty.facultyRecruitedFor = data.IdentfdFor;
                if (data.specId != null)
                {
                    int SpecId = (int)data.specId;
                    newcollegeFaculty.SpecializationName = jntuh_specializations.Where(e => e.id == SpecId).Select(e => e.specializationName).FirstOrDefault();
                }
                if (data.DepartmentId != null)
                {
                    int deptId = (int)data.DepartmentId;
                    newcollegeFaculty.department = jntuh_departments.Where(e => e.id == deptId).Select(e => e.departmentName).FirstOrDefault();
                }
                if (data.DesignationId != null)
                {
                    int desigId = (int)data.DesignationId;
                    newcollegeFaculty.designation = jntuh_designations.Where(e => e.id == desigId).Select(e => e.designation).FirstOrDefault();
                }
                newcollegeFaculty.facultyCategoryId = data.collegeid;
                newcollegeFaculty.facultyTypeId = type;
                newcollegeFaculty.id = data.id;
                if (data.Blacklistfaculy == null)
                {
                    newcollegeFaculty.BlackList = false;
                }
                else
                {
                    newcollegeFaculty.BlackList = (bool)data.Blacklistfaculy;
                }
                //newcollegeFaculty.BlackList = data.Blacklistfaculy != null ? (bool)data.Blacklistfaculy : data.Blacklistfaculy;
                #region this code for Reasons
                //string Reasons = "";
                //if (data.Absent == true)
                //{
                //    Reasons = "ABSENT" + ",";
                //}
                //if (data.NotQualifiedAsperAICTE == true)
                //{
                //    Reasons += "NOT QUALIFIED " + ",";
                //}
                //if (string.IsNullOrEmpty(data.PANNumber))
                //{
                //    Reasons += "NO PAN" + ",";
                //}
                //if (data.DepartmentId == null)
                //{
                //    Reasons += "NO DEPARTMENT" + ",";
                //}
                //if (data.NoSCM == true)
                //{
                //    Reasons += "NO SCM/RATIFICATION" + ",";
                //}
                //if (data.PHDundertakingnotsubmitted == true)
                //{
                //    Reasons += "NO UNDERTAKING" + ",";
                //}
                //if (data.Blacklistfaculy == true)
                //{
                //    Reasons += "BLACKLISTED" + ",";
                //}

                //if (Reasons != "")
                //{
                //    Reasons = Reasons.Substring(0, Reasons.Length - 1);
                //}


                //newcollegeFaculty.Reason = Reasons;
                #endregion

                newcollegeFaculty.sfacid = UAAAS.Models.Utilities.EncryptString(newcollegeFaculty.id.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]);
                newteachingFaculty.Add(newcollegeFaculty);

            }

            ViewBag.Count = newteachingFaculty.Count();
            TempData["InActive"] = newteachingFaculty.Where(s => s.department == null || s.facultyAadhaarNumber == null || s.facultyAadharDocument == null).Count();
            TempData["Active"] = newteachingFaculty.Where(s => s.department != null || s.facultyAadhaarNumber != null || s.facultyAadharDocument != null).Count();
            //  return View(newteachingFaculty);
            return Json(newteachingFaculty.OrderBy(s => s.facultyAadharDocument).ToList(), "application/json", JsonRequestBehavior.AllowGet);



            #region old code
            //[Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
            //[HttpGet]
            //public JsonResult TeachingJson(string collegeId)
            //{
            //    int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            //    int userCollegeID = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userID).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();
            //       int type = db.jntuh_faculty_type.Where(t => t.facultyType == "Teaching").Select(t => t.id).FirstOrDefault();
            //    List<jntuh_college_faculty> jntuh_college_faculty = db.jntuh_college_faculty.Where(f => f.facultyTypeId == type && f.collegeId == userCollegeID)
            //        .OrderBy(f => f.facultyDepartmentId).ThenBy(f => f.facultyDesignationId).ThenBy(f => f.facultyFirstName).ToList();

            //    List<CollegeFaculty> teachingFaculty = new List<CollegeFaculty>();

            //    //Commented By Srinivas.T   FacultyVerificationStatus
            //    //List<jntuh_college_faculty_registered> regFaculty = db.jntuh_college_faculty_registered.Where(r => r.collegeId == userCollegeID && r.existingFacultyId == null && r.createdBy != 63809).Select(r => r).ToList();
            //    List<jntuh_college_faculty_registered> regFaculty = db.jntuh_college_faculty_registered.Where(r => r.collegeId == userCollegeID && r.existingFacultyId == null).Select(r => r).ToList();

            //    var RegistredFacultyLog = db.jntuh_registered_faculty_log.Select(F => new { F.RegistrationNumber, F.FacultyApprovedStatus,F.Remarks }).ToList();
            //    var jntuh_registered_facultys = db.jntuh_registered_faculty.ToList();
            //    //List<jntuh_registered_faculty> rFaculty1 = jntuh_registered_facultys.Where(F => F.RegistrationNumber == "9562-150411-152607").ToList();
            //    int count = 0;
            //    var jntuh_designations = db.jntuh_designation.Select(d => new { d.id, d.designation }).ToList();
            //    foreach (var item in regFaculty)
            //    {
            //        CollegeFaculty collegeFaculty = new CollegeFaculty();
            //        jntuh_registered_faculty rFaculty = jntuh_registered_facultys.Where(f => f.RegistrationNumber == item.RegistrationNumber.Trim()).FirstOrDefault();

            //        //if (rFaculty.RegistrationNumber == "9893-150415-173751")
            //        //{

            //        //}


            //        //if (rFaculty.collegeId != null)
            //        //{
            //        //    collegeFaculty.collegeId = (int)rFaculty.collegeId;
            //        //}
            //        collegeFaculty.collegeId = userCollegeID;
            //        //collegeFaculty.facultyTypeId = rFaculty.facultyTypeId;
            //        collegeFaculty.facultyFirstName = rFaculty.FirstName;
            //        collegeFaculty.facultyLastName = rFaculty.MiddleName;
            //        collegeFaculty.facultySurname = rFaculty.LastName;
            //        collegeFaculty.facultyGenderId = rFaculty.GenderId;
            //        collegeFaculty.facultyFatherName = rFaculty.FatherOrHusbandName;
            //        //collegeFaculty.facultyCategoryId = rFaculty.facultyCategoryId;

            //        if (rFaculty.DateOfBirth != null)
            //            collegeFaculty.dateOfBirth = UAAAS.Models.Utilities.MMDDYY2DDMMYY(rFaculty.DateOfBirth.ToString());

            //        //if (rFaculty.WorkingStatus == true)
            //        //{

            //            // rFaculty.DesignationId!=null? (int)rFaculty.DesignationId:0;
            //            //collegeFaculty.facultyDesignationId = (int)rFaculty.DesignationId;
            //            collegeFaculty.facultyDesignationId = rFaculty.DesignationId != null ? (int)rFaculty.DesignationId : 0;
            //            collegeFaculty.designation = jntuh_designations.Where(d => d.id == collegeFaculty.facultyDesignationId).Select(d => d.designation).FirstOrDefault();
            //            collegeFaculty.facultyOtherDesignation = rFaculty.OtherDesignation;
            //            collegeFaculty.facultyDepartmentId = rFaculty.DepartmentId != null ? (int)rFaculty.DepartmentId : 0;
            //            collegeFaculty.department = db.jntuh_department.Where(d => d.id == collegeFaculty.facultyDepartmentId).Select(d => d.departmentName).FirstOrDefault();
            //            collegeFaculty.facultyOtherDepartment = rFaculty.OtherDepartment;
            //       // }

            //        collegeFaculty.facultyEmail = rFaculty.Email;
            //        collegeFaculty.facultyMobile = rFaculty.Mobile;
            //        collegeFaculty.facultyPANNumber = rFaculty.PANNumber;
            //        collegeFaculty.facultyAadhaarNumber = rFaculty.AadhaarNumber;
            //        collegeFaculty.FacultyRegistrationNumber = rFaculty.RegistrationNumber;
            //        collegeFaculty.facultyCount = RegistredFacultyLog.Count(F => F.RegistrationNumber == rFaculty.RegistrationNumber);

            //        collegeFaculty.FacultyVerificationStatus = RegistredFacultyLog.Where(F => F.RegistrationNumber == rFaculty.RegistrationNumber).Select(F => F.FacultyApprovedStatus).FirstOrDefault();
            //        collegeFaculty.Remarks = RegistredFacultyLog.Where(F => F.RegistrationNumber == rFaculty.RegistrationNumber).Select(F => F.Remarks).FirstOrDefault();
            //        teachingFaculty.Add(collegeFaculty);
            //    }
            //    var jntuh_college_faculty_registereds = db.jntuh_college_faculty_registered.Where(C => C.collegeId == userCollegeID).Select(C => C).ToList();

            //    jntuh_college_faculty_registered eFaculty = null;

            //    var jntuh_departments = db.jntuh_department.Select(d => new { d.id, d.departmentName }).ToList();
            //    foreach (var faculty in jntuh_college_faculty)
            //    {

            //        CollegeFaculty collegeFaculty = new CollegeFaculty();
            //        collegeFaculty.id = faculty.id;

            //        eFaculty = jntuh_college_faculty_registereds.Where(r => r.existingFacultyId == faculty.id).Select(r => r).FirstOrDefault();

            //        if (eFaculty != null)
            //        {
            //            collegeFaculty.FacultyRegistrationNumber = eFaculty.RegistrationNumber;
            //            //if (eFaculty.RegistrationNumber == "9893-150415-173751")
            //            //{

            //            //}
            //        }

            //        if (string.IsNullOrEmpty(collegeFaculty.FacultyRegistrationNumber))
            //        {
            //            if (eFaculty != null)
            //            {

            //                collegeFaculty.collegeId = faculty.collegeId;
            //                collegeFaculty.facultyTypeId = faculty.facultyTypeId;
            //                collegeFaculty.facultyFirstName = faculty.facultyFirstName;
            //                collegeFaculty.facultyLastName = faculty.facultyLastName;
            //                collegeFaculty.facultySurname = faculty.facultySurname;
            //                collegeFaculty.facultyGenderId = faculty.facultyGenderId;
            //                collegeFaculty.facultyFatherName = faculty.facultyFatherName;
            //                collegeFaculty.facultyCategoryId = faculty.facultyCategoryId;

            //                if (faculty.facultyDateOfBirth != null)
            //                    collegeFaculty.dateOfBirth = UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.facultyDateOfBirth.ToString());

            //                collegeFaculty.facultyDesignationId = faculty.facultyDesignationId;
            //                collegeFaculty.designation = jntuh_designations.Where(d => d.id == faculty.facultyDesignationId).Select(d => d.designation).FirstOrDefault();
            //                collegeFaculty.facultyOtherDesignation = faculty.facultyOtherDesignation;
            //                collegeFaculty.facultyDepartmentId = faculty.facultyDepartmentId;
            //                collegeFaculty.department = jntuh_departments.Where(d => d.id == faculty.facultyDepartmentId).Select(d => d.departmentName).FirstOrDefault();

            //                if (faculty.facultyDateOfAppointment != null)
            //                    collegeFaculty.dateOfAppointment = UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.facultyDateOfAppointment.ToString());

            //                if (faculty.facultyDateOfResignation != null)
            //                    collegeFaculty.dateOfResignation = UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.facultyDateOfResignation.ToString());

            //                collegeFaculty.isFacultyRatifiedByJNTU = faculty.isFacultyRatifiedByJNTU;

            //                if (faculty.facultyDateOfRatification != null)
            //                    collegeFaculty.dateOfRatification = UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.facultyDateOfRatification.ToString());

            //                collegeFaculty.facultyDurationOfRatification = faculty.facultyDurationOfRatification;
            //                collegeFaculty.facultySalary = faculty.facultySalary;
            //                collegeFaculty.facultyPreviousExperience = Convert.ToInt32(faculty.facultyPreviousExperience);
            //                collegeFaculty.facultyPayScale = faculty.facultyPayScale;
            //                collegeFaculty.facultyEmail = faculty.facultyEmail;
            //                collegeFaculty.facultyMobile = faculty.facultyMobile;
            //                collegeFaculty.facultyPANNumber = faculty.facultyPANNumber;
            //                collegeFaculty.facultyAadhaarNumber = faculty.facultyAadhaarNumber;
            //                collegeFaculty.isRelatedToExamBranch = faculty.isRelatedToExamBranch;
            //                collegeFaculty.isRelatedToPlacementCell = faculty.isRelatedToPlacementCell;
            //                collegeFaculty.facultyAchievements1 = faculty.facultyAchievements1;
            //                collegeFaculty.facultyAchievements2 = faculty.facultyAchievements2;
            //                collegeFaculty.facultyAchievements3 = faculty.facultyAchievements3;
            //                collegeFaculty.FacultyRegistrationNumber = string.Empty;
            //                collegeFaculty.facultyCount = RegistredFacultyLog.Count(F => F.RegistrationNumber == eFaculty.RegistrationNumber);
            //                collegeFaculty.Remarks = RegistredFacultyLog.Where(F => F.RegistrationNumber == eFaculty.RegistrationNumber).Select(F => F.Remarks).FirstOrDefault();
            //                collegeFaculty.FacultyVerificationStatus = RegistredFacultyLog.Where(F => F.RegistrationNumber == eFaculty.RegistrationNumber).Select(F => F.FacultyApprovedStatus).FirstOrDefault();
            //            }

            //        }
            //        else
            //        {
            //            if (eFaculty != null)
            //            {
            //                jntuh_registered_faculty rFaculty = jntuh_registered_facultys.Where(f => f.RegistrationNumber == eFaculty.RegistrationNumber.Trim()).FirstOrDefault();
            //                //if (rFaculty.RegistrationNumber == "9562-150411-152607")
            //                //{

            //                //}
            //                if (rFaculty.collegeId != null)
            //                {
            //                    collegeFaculty.collegeId = (int)rFaculty.collegeId;
            //                }

            //                //collegeFaculty.facultyTypeId = rFaculty.facultyTypeId;
            //                collegeFaculty.facultyFirstName = rFaculty.FirstName;
            //                collegeFaculty.facultyLastName = rFaculty.MiddleName;
            //                collegeFaculty.facultySurname = rFaculty.LastName;
            //                collegeFaculty.facultyGenderId = rFaculty.GenderId;
            //                collegeFaculty.facultyFatherName = rFaculty.FatherOrHusbandName;
            //                //collegeFaculty.facultyCategoryId = rFaculty.facultyCategoryId;

            //                if (rFaculty.DateOfBirth != null)
            //                    collegeFaculty.dateOfBirth = UAAAS.Models.Utilities.MMDDYY2DDMMYY(rFaculty.DateOfBirth.ToString());

            //                if (rFaculty.DesignationId != null)
            //                {
            //                    collegeFaculty.facultyDesignationId = (int)rFaculty.DesignationId;
            //                    collegeFaculty.designation = jntuh_designations.Where(d => d.id == collegeFaculty.facultyDesignationId).Select(d => d.designation).FirstOrDefault();
            //                }

            //                collegeFaculty.facultyOtherDesignation = rFaculty.OtherDesignation;

            //                if (rFaculty.DepartmentId != null)
            //                {
            //                    collegeFaculty.facultyDepartmentId = (int)rFaculty.DepartmentId;
            //                    collegeFaculty.department = jntuh_departments.Where(d => d.id == collegeFaculty.facultyDepartmentId).Select(d => d.departmentName).FirstOrDefault();
            //                }

            //                collegeFaculty.facultyOtherDepartment = rFaculty.OtherDepartment;
            //                collegeFaculty.facultyEmail = rFaculty.Email;
            //                collegeFaculty.facultyMobile = rFaculty.Mobile;
            //                collegeFaculty.facultyPANNumber = rFaculty.PANNumber;
            //                collegeFaculty.facultyAadhaarNumber = rFaculty.AadhaarNumber;
            //                collegeFaculty.FacultyRegistrationNumber = rFaculty.RegistrationNumber;

            //                collegeFaculty.facultyCount = RegistredFacultyLog.Count(F => F.RegistrationNumber == eFaculty.RegistrationNumber);
            //                collegeFaculty.Remarks = RegistredFacultyLog.Where(F => F.RegistrationNumber == eFaculty.RegistrationNumber).Select(F => F.Remarks).FirstOrDefault();
            //                collegeFaculty.FacultyVerificationStatus = RegistredFacultyLog.Where(F => F.RegistrationNumber == eFaculty.RegistrationNumber).Select(F => F.FacultyApprovedStatus).FirstOrDefault();
            //            }

            //            teachingFaculty.Add(collegeFaculty);
            //        }

            //    }

            //    ViewBag.Type = UAAAS.Models.Utilities.EncryptString(type.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]);
            //    //ViewBag.Type = 0;
            //    ViewBag.Id = UAAAS.Models.Utilities.EncryptString("0", WebConfigurationManager.AppSettings["CryptoKey"]);
            //    //ViewBag.Id = 0;
            //    ViewBag.Count = teachingFaculty.Count();
            //    count++;
            //    return View(teachingFaculty);
            //}
            #endregion
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        [HttpPost]
        public ActionResult EditFaculty(CollegeFaculty faculty)
        {
            //return RedirectToAction("Index", "UnderConstruction");
            //return RedirectToAction("Teaching", "Faculty");
            TempData["Error"] = null;
            //int id =
            //    db.jntuh_college_faculty_registered.Where(
            //        f => f.RegistrationNumber == faculty.FacultyRegistrationNumber && f.AadhaarDocument != null)
            //        .Select(s => s.id)
            //        .FirstOrDefault();
            //if (id!=0)
            //{
            //    return RedirectToAction("Teaching", "Faculty");
            //}
            //return RedirectToAction("Teaching", "Faculty");
            string aadhaarCardsPath = "~/Content/Upload/Faculty/AADHAARCARDS/CollegeFacultyAadhaar";
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int actualYear = db.jntuh_academic_year.Where(s => s.isActive == true && s.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int presentAyId = db.jntuh_academic_year.Where(s => s.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            //Aadhaar Verification
            var status = aadharcard.validateVerhoeff(faculty.facultyAadhaarNumber.Trim());
            var jntuh_college_faculty_registered =
                db.jntuh_college_faculty_registered.Where(
                    f => f.AadhaarNumber.Trim() == faculty.facultyAadhaarNumber.Trim() && f.RegistrationNumber.Trim() != faculty.FacultyRegistrationNumber.Trim())
                    .Select(e => e)
                    .Count();
            var jntuh_college_principal_registered =
          db.jntuh_college_principal_registered.Where(
              f => f.AadhaarNumber.Trim() == faculty.facultyAadhaarNumber.Trim() && f.RegistrationNumber != faculty.FacultyRegistrationNumber.Trim())
              .Select(e => e)
              .Count();
            if (status)
            {
                if (jntuh_college_faculty_registered != 0 || jntuh_college_principal_registered != 0)
                {
                    TempData["Error"] = "AadhaarNumber already Exists.";
                    return RedirectToAction("Teaching", "PA_Faculty");
                }
            }
            else
            {
                TempData["Error"] = "AadhaarNumber is not a validnumber.";
                return RedirectToAction("Teaching", "PA_Faculty");
            }

            jntuh_registered_faculty isRegisteredFaculty = db.jntuh_registered_faculty.Where(r => r.RegistrationNumber.Trim() == faculty.FacultyRegistrationNumber.Trim()).Select(r => r).FirstOrDefault();

            if (isRegisteredFaculty == null)
            {
                TempData["Error"] = "Invalid Faculty Registration Number.";
                return RedirectToAction("Teaching", "PA_Faculty");
            }

            if (isRegisteredFaculty.Blacklistfaculy == true)
            {
                TempData["Error"] = "Faculty Registration Number is in Blacklist.";
                return RedirectToAction("Teaching", "PA_Faculty");
            }
            if (isRegisteredFaculty.AbsentforVerification == true)
            {
                TempData["Error"] = "Faculty Registration Number is in Inactive.";
                return RedirectToAction("Teaching", "PA_Faculty");
            }

            FacultyAttedance jntuh_registered_facultys1 = new FacultyAttedance();
            var jntuh_registrared_faculty_log = db.jntuh_registered_faculty_log.Where(r => r.RegistrationNumber == faculty.FacultyRegistrationNumber).Select(r => r).FirstOrDefault();
            var jntuh_registered_faculty = db.jntuh_registered_faculty.Where(F => F.RegistrationNumber == faculty.FacultyRegistrationNumber && F.Blacklistfaculy != true).Select(F => F).FirstOrDefault();
            if (jntuh_registered_faculty != null)
            {
                if (jntuh_registrared_faculty_log != null)
                {
                    if (!string.IsNullOrEmpty(jntuh_registrared_faculty_log.AadhaarNumber))
                    {
                        jntuh_registered_facultys1 = db.jntuh_registered_faculty_log.Where(F => F.RegistrationNumber == faculty.FacultyRegistrationNumber).Select(F => new FacultyAttedance
                        {
                            RegistrationNumber = F.RegistrationNumber,
                            FirstName = F.FirstName,
                            LastName = F.LastName,
                            Mobile = F.Mobile,
                            Email = F.Email,
                            AadhaarNumber = F.AadhaarNumber,
                            GenderId = F.GenderId,
                            DateOfBirth = F.DateOfBirth,
                            id = F.RegFacultyId
                        }).FirstOrDefault();
                    }
                }
                else //if (!string.IsNullOrEmpty(jntuh_registered_faculty.AadhaarNumber))
                {
                    jntuh_registered_facultys1 = db.jntuh_registered_faculty.Where(F => F.RegistrationNumber == faculty.FacultyRegistrationNumber && F.Blacklistfaculy != true).Select(F => new FacultyAttedance
                    {
                        id = F.id,
                        RegistrationNumber = F.RegistrationNumber,
                        FirstName = F.FirstName,
                        LastName = F.LastName,
                        Mobile = F.Mobile,
                        Email = F.Email,
                        AadhaarNumber = F.AadhaarNumber,
                        GenderId = F.GenderId,
                        DateOfBirth = F.DateOfBirth
                    }).FirstOrDefault();

                }

            }


            if (jntuh_registered_facultys1 != null)
            {
                //if (string.IsNullOrEmpty(jntuh_registered_facultys1.AadhaarNumber))
                //{
                //    TempData["Error"] = "No Aadhaar Number For Faculty Registration Number.";
                //    return RedirectToAction("Teaching", "Faculty");
                //}
            }
            else
            {
                TempData["Error"] = "No Aadhaar Number For Faculty Registration Number.";
                return RedirectToAction("Teaching", "PA_Faculty");
            }

            int result = 0;

            jntuh_college_faculty_registered regFaculty = db.jntuh_college_faculty_registered.Where(F => F.RegistrationNumber.Trim() == faculty.FacultyRegistrationNumber.Trim()).Select(F => F).FirstOrDefault();

            regFaculty.IdentifiedFor = faculty.facultyRecruitedFor;

            if (regFaculty.IdentifiedFor == "UG")
            {
                if (faculty.facultyDepartmentId == 26)
                {
                    regFaculty.DepartmentId = faculty.facultyDepartmentId;
                    regFaculty.SpecializationId = null;
                    //regFaculty.FacultySpecializationId = faculty.FacultySpecalizationId;
                }
                else
                {
                    regFaculty.DepartmentId = faculty.facultyDepartmentId;
                    regFaculty.SpecializationId = null;
                    regFaculty.FacultySpecializationId = null;
                }

            }
            else if (faculty.facultyPGDepartmentId == 36 || faculty.facultyPGDepartmentId == 27 || faculty.facultyPGDepartmentId == 39)
            {
                regFaculty.DepartmentId = faculty.facultyPGDepartmentId;
                regFaculty.SpecializationId = faculty.SpecializationId;
                //regFaculty.FacultySpecializationId = faculty.FacultySpecalizationId;
            }
            else
            {
                regFaculty.DepartmentId = faculty.facultyPGDepartmentId;
                regFaculty.SpecializationId = faculty.SpecializationId;
                regFaculty.FacultySpecializationId = null;
            }
            regFaculty.isActive = faculty.isActive;
            regFaculty.existingFacultyId = jntuh_registered_faculty.id;
            regFaculty.createdBy = regFaculty.createdBy;
            regFaculty.createdOn = Convert.ToDateTime(regFaculty.createdOn);
            regFaculty.updatedOn = DateTime.Now;

            regFaculty.updatedBy = userID;
            regFaculty.AadhaarNumber = faculty.facultyAadhaarNumber.Trim();
            if (faculty.facultyAadharPhotoDocument != null)
            {
                if (!Directory.Exists(Server.MapPath(aadhaarCardsPath)))
                {
                    Directory.CreateDirectory(Server.MapPath(aadhaarCardsPath));
                }

                var ext = Path.GetExtension(faculty.facultyAadharPhotoDocument.FileName);

                if (ext.ToUpper().Equals(".JPEG") || ext.ToUpper().Equals(".JPG"))
                {
                    if (faculty.facultyAadharDocument == null)
                    {
                        string fileName = DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" +
                                          jntuh_registered_faculty.FirstName.Substring(0, 1) + "-" +
                                          jntuh_registered_faculty.LastName.Substring(0, 1);
                        faculty.facultyAadharPhotoDocument.SaveAs(string.Format("{0}/{1}{2}",
                            Server.MapPath(aadhaarCardsPath),
                            fileName, ext));
                        regFaculty.AadhaarDocument = string.Format("{0}{1}", fileName, ext);
                    }
                    else
                    {
                        //string fileName = faculty.facultyAadharDocument;
                        string fileName = DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" +
                                          jntuh_registered_faculty.FirstName.Substring(0, 1) + "-" +
                                          jntuh_registered_faculty.LastName.Substring(0, 1);
                        faculty.facultyAadharPhotoDocument.SaveAs(string.Format("{0}/{1}{2}",
                            Server.MapPath(aadhaarCardsPath),
                            fileName, ext));
                        regFaculty.AadhaarDocument = string.Format("{0}{1}", fileName, ext);
                        //regFaculty.AadhaarDocument = faculty.facultyAadharDocument; 
                    }
                }


            }
            else if (faculty.facultyAadharDocument != null)
            {

                regFaculty.AadhaarDocument = faculty.facultyAadharDocument;


            }
            else
            {
                TempData["Message"] = "File Upload is Missing.Please Fill the Details Again";
                return RedirectToAction("Teaching", "PA_Faculty",
                    new
                    {
                        collegeId =
                            UAAAS.Models.Utilities.EncryptString(faculty.collegeId.ToString(),
                                WebConfigurationManager.AppSettings["CryptoKey"]).ToString()
                    });
            }
            db.Entry(regFaculty).State = EntityState.Modified;
            db.SaveChanges();
            TempData["Success"] = "Faculty Updated Successfully.";

            //Faculty Experiance Details Saving
            jntuh_registered_faculty_experience updatefacultyexperiance =
                db.jntuh_registered_faculty_experience.Where(e => e.Id == faculty.experienceId)
                    .Select(s => s)
                    .FirstOrDefault();
            string facultyappointmentletters = "~/Content/Upload/College/Faculty/AppointmentLetters";
            string facultyrelevingletters = "~/Content/Upload/College/Faculty/RelevingLetters";
            string facultypreviouscollegescms = "~/Content/Upload/College/Faculty/PreviousSCMs";
            jntuh_registered_faculty_experience facultyexperiance = new jntuh_registered_faculty_experience();
            if (updatefacultyexperiance != null)
            {
                if (faculty.AppointmentOrderDocument != null)
                {
                    if (!Directory.Exists(Server.MapPath(facultyappointmentletters)))
                    {
                        Directory.CreateDirectory(Server.MapPath(facultyappointmentletters));
                    }

                    var ext = Path.GetExtension(faculty.AppointmentOrderDocument.FileName);

                    if (ext.ToUpper().Equals(".PDF"))
                    {
                        string fileName = DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" +
                        isRegisteredFaculty.FirstName.Substring(0, 1) + "-" + isRegisteredFaculty.LastName.Substring(0, 1);
                        faculty.AppointmentOrderDocument.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(facultyappointmentletters),
                            fileName, ext));
                        faculty.ViewAppointmentOrderDocument = string.Format("{0}{1}", fileName, ext);
                        updatefacultyexperiance.facultyJoiningOrder = faculty.ViewAppointmentOrderDocument;
                    }
                }
                if (faculty.RelivingDocument != null)
                {
                    if (!Directory.Exists(Server.MapPath(facultyrelevingletters)))
                    {
                        Directory.CreateDirectory(Server.MapPath(facultyrelevingletters));
                    }

                    var ext = Path.GetExtension(faculty.RelivingDocument.FileName);

                    if (ext.ToUpper().Equals(".PDF"))
                    {
                        string fileName = DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" +
                        isRegisteredFaculty.FirstName.Substring(0, 1) + "-" + isRegisteredFaculty.LastName.Substring(0, 1);
                        faculty.RelivingDocument.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(facultyrelevingletters),
                            fileName, ext));
                        faculty.ViewRelivingDocument = string.Format("{0}{1}", fileName, ext);
                        updatefacultyexperiance.facultyRelievingLetter = faculty.ViewRelivingDocument;
                    }
                }
                //Optional
                if (faculty.SelectionCommitteeDocument != null)
                {
                    if (!Directory.Exists(Server.MapPath(facultypreviouscollegescms)))
                    {
                        Directory.CreateDirectory(Server.MapPath(facultypreviouscollegescms));
                    }

                    var ext = Path.GetExtension(faculty.SelectionCommitteeDocument.FileName);

                    if (ext.ToUpper().Equals(".PDF"))
                    {
                        string fileName = DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" +
                        isRegisteredFaculty.FirstName.Substring(0, 1) + "-" + isRegisteredFaculty.LastName.Substring(0, 1);
                        faculty.SelectionCommitteeDocument.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(facultypreviouscollegescms),
                            fileName, ext));
                        faculty.ViewSelectionCommitteeDocument = string.Format("{0}{1}", fileName, ext);
                        updatefacultyexperiance.FacultySCMDocument = faculty.ViewSelectionCommitteeDocument;
                    }
                }
                if (faculty.Previouscollegeid == 0)
                {
                    updatefacultyexperiance.collegeId = faculty.collegeId;
                }
                else
                {
                    if (faculty.Previouscollegeid == 375)
                    {
                        updatefacultyexperiance.collegeId = faculty.Previouscollegeid;
                        updatefacultyexperiance.OtherCollege = faculty.Otherscollegename;
                    }
                    else
                    {
                        updatefacultyexperiance.collegeId = faculty.Previouscollegeid;
                        updatefacultyexperiance.OtherCollege = null;
                    }
                }
                if (faculty.facultyDesignationId == 4)
                {
                    updatefacultyexperiance.facultyDesignationId = faculty.facultyDesignationId;
                    updatefacultyexperiance.OtherDesignation = faculty.facultyOtherDesignation;
                }
                else
                {
                    updatefacultyexperiance.facultyDesignationId = faculty.facultyDesignationId;
                    updatefacultyexperiance.OtherDesignation = null;
                }
                if (faculty.facultySalary != null)
                    updatefacultyexperiance.facultySalary = faculty.facultySalary;


                if (!String.IsNullOrEmpty(faculty.dateOfAppointment))
                    updatefacultyexperiance.facultyDateOfAppointment = Models.Utilities.DDMMYY2MMDDYY(faculty.dateOfAppointment);
                if (!String.IsNullOrEmpty(faculty.dateOfResignation))
                    updatefacultyexperiance.facultyDateOfResignation =
                    Models.Utilities.DDMMYY2MMDDYY(faculty.dateOfResignation);

                updatefacultyexperiance.updatedBy = userID;
                updatefacultyexperiance.updatedOn = DateTime.Now;

                db.Entry(updatefacultyexperiance).State = EntityState.Modified;
                db.SaveChanges();

            }
            else
            {


                facultyexperiance.facultyId = isRegisteredFaculty.id;
                facultyexperiance.createdBycollegeId = faculty.collegeId;
                if (faculty.AppointmentOrderDocument != null)
                {
                    if (!Directory.Exists(Server.MapPath(facultyappointmentletters)))
                    {
                        Directory.CreateDirectory(Server.MapPath(facultyappointmentletters));
                    }

                    var ext = Path.GetExtension(faculty.AppointmentOrderDocument.FileName);

                    if (ext.ToUpper().Equals(".PDF"))
                    {
                        string fileName = DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" +
                        isRegisteredFaculty.FirstName.Substring(0, 1) + "-" + isRegisteredFaculty.LastName.Substring(0, 1);
                        faculty.AppointmentOrderDocument.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(facultyappointmentletters),
                            fileName, ext));
                        faculty.ViewAppointmentOrderDocument = string.Format("{0}{1}", fileName, ext);
                        facultyexperiance.facultyJoiningOrder = faculty.ViewAppointmentOrderDocument;
                    }
                }

                if (faculty.RelivingDocument != null)
                {
                    if (!Directory.Exists(Server.MapPath(facultyrelevingletters)))
                    {
                        Directory.CreateDirectory(Server.MapPath(facultyrelevingletters));
                    }

                    var ext = Path.GetExtension(faculty.RelivingDocument.FileName);

                    if (ext.ToUpper().Equals(".PDF"))
                    {
                        string fileName = DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" +
                        isRegisteredFaculty.FirstName.Substring(0, 1) + "-" + isRegisteredFaculty.LastName.Substring(0, 1);
                        faculty.RelivingDocument.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(facultyrelevingletters),
                            fileName, ext));
                        faculty.ViewRelivingDocument = string.Format("{0}{1}", fileName, ext);
                        facultyexperiance.facultyRelievingLetter = faculty.ViewRelivingDocument;
                    }
                }

                //Optional
                if (faculty.SelectionCommitteeDocument != null)
                {
                    if (!Directory.Exists(Server.MapPath(facultypreviouscollegescms)))
                    {
                        Directory.CreateDirectory(Server.MapPath(facultypreviouscollegescms));
                    }

                    var ext = Path.GetExtension(faculty.SelectionCommitteeDocument.FileName);

                    if (ext.ToUpper().Equals(".PDF"))
                    {
                        string fileName = DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" +
                        isRegisteredFaculty.FirstName.Substring(0, 1) + "-" + isRegisteredFaculty.LastName.Substring(0, 1);
                        faculty.SelectionCommitteeDocument.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(facultypreviouscollegescms),
                            fileName, ext));
                        faculty.ViewSelectionCommitteeDocument = string.Format("{0}{1}", fileName, ext);
                        facultyexperiance.FacultySCMDocument = faculty.ViewSelectionCommitteeDocument;
                    }
                }
                if (faculty.Previouscollegeid == 0)
                {
                    facultyexperiance.collegeId = faculty.collegeId;
                }
                else
                {
                    if (faculty.Previouscollegeid == 375)
                    {
                        facultyexperiance.collegeId = faculty.Previouscollegeid;
                        facultyexperiance.OtherCollege = faculty.Otherscollegename;
                    }
                    else
                    {
                        facultyexperiance.collegeId = faculty.Previouscollegeid;
                    }
                }


                if (faculty.facultyDesignationId == 4)
                {
                    facultyexperiance.facultyDesignationId = faculty.facultyDesignationId;
                    facultyexperiance.OtherDesignation = faculty.facultyOtherDesignation;
                }
                else
                {
                    facultyexperiance.facultyDesignationId = faculty.facultyDesignationId;
                }
                if (faculty.grossSalary != null)
                    facultyexperiance.facultySalary = faculty.grossSalary;
                if (!String.IsNullOrEmpty(faculty.dateOfAppointment))
                    facultyexperiance.facultyDateOfAppointment = Models.Utilities.DDMMYY2MMDDYY(faculty.dateOfAppointment);
                if (!String.IsNullOrEmpty(faculty.dateOfResignation))
                    facultyexperiance.facultyDateOfResignation =
                    Models.Utilities.DDMMYY2MMDDYY(faculty.dateOfResignation);

                facultyexperiance.createdBy = userID;
                facultyexperiance.createdOn = DateTime.Now;
                facultyexperiance.updatedBy = null;
                facultyexperiance.updatedOn = null;
                db.jntuh_registered_faculty_experience.Add(facultyexperiance);
                db.SaveChanges();
            }
            var objART = new jntuh_college_facultytracking();
            objART.academicYearId = presentAyId;
            objART.collegeId = regFaculty.collegeId;
            objART.RegistrationNumber = regFaculty.RegistrationNumber;
            objART.DepartmentId = regFaculty.DepartmentId;
            objART.SpecializationId = regFaculty.SpecializationId;
            objART.ActionType = 3;
            objART.FacultyType = "Faculty";
            objART.FacultyStatus = "Y";
            objART.Reasion = "Faculty Updated";
            //objART.FacultyJoinDate = regFaculty.createdOn;
            objART.previousworkingcollegeid = faculty.Previouscollegeid;
            objART.FacultyJoinDate = Models.Utilities.DDMMYY2MMDDYY(faculty.dateOfAppointment);
            objART.relevingdate = Models.Utilities.DDMMYY2MMDDYY(faculty.dateOfResignation);
            objART.aadhaarnumber = faculty.facultyAadhaarNumber.Trim();
            objART.aadhaardocument = regFaculty.AadhaarDocument;
            if (updatefacultyexperiance != null && updatefacultyexperiance.Id > 0)
            {
                objART.scmdocument = updatefacultyexperiance.FacultySCMDocument;
                objART.FacultyJoinDocument = updatefacultyexperiance.facultyJoiningOrder;
                objART.relevingdocumnt = updatefacultyexperiance.facultyRelievingLetter;
                objART.payscale = updatefacultyexperiance.facultySalary;
                objART.designation = updatefacultyexperiance.facultyDesignationId != null ? updatefacultyexperiance.facultyDesignationId.ToString() : null;
            }
            else
            {
                objART.scmdocument = facultyexperiance.FacultySCMDocument;
                objART.FacultyJoinDocument = facultyexperiance.facultyJoiningOrder;
                objART.relevingdocumnt = facultyexperiance.facultyRelievingLetter;
                objART.payscale = facultyexperiance.facultySalary;
                objART.designation = facultyexperiance.facultyDesignationId != null ? facultyexperiance.facultyDesignationId.ToString() : null;
            }
            objART.isActive = true;
            objART.Createdon = DateTime.Now;
            objART.CreatedBy = userID;
            objART.Updatedon = null;
            objART.UpdatedBy = null;
            db.jntuh_college_facultytracking.Add(objART);
            db.SaveChanges();
            return RedirectToAction("Teaching", "PA_Faculty");
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        [HttpGet]
        public ActionResult FacultyRegistrationNumber(string collegeId, string fid)
        {
            //return RedirectToAction("Index", "UnderConstruction");
            //return RedirectToAction("Teaching", "Faculty");
            if (Membership.GetUser() == null)
            {
                return RedirectToAction("LogOn", "Account");
            }
            int facultyId = 0;
            TempData["path"] = null;
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);

            int userCollegeID = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userID).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();

            if (userCollegeID == 0 && collegeId != null)
            {
                if (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry"))
                {
                    userCollegeID = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(collegeId, WebConfigurationManager.AppSettings["CryptoKey"]));
                }
            }

            if (!string.IsNullOrEmpty(fid))
            {
                //  facultyId = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(fid, WebConfigurationManager.AppSettings["CryptoKey"]));
                // Changed by Naushad Khan
                facultyId = Convert.ToInt32(fid);
            }
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            ViewBag.collegeId = UAAAS.Models.Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString();
            ViewBag.Institutions = db.jntuh_college.Where(c => c.isActive == true && c.id != 375).Select(c => new { CollegeId = c.id, CollegeName = c.collegeCode + "-" + c.collegeName }).OrderBy(c => c.CollegeName).ToList();
            CollegeFaculty faculty = new CollegeFaculty();
            jntuh_registered_faculty regfaculty = new jntuh_registered_faculty();
            if (facultyId != 0)
            {
                regfaculty = db.jntuh_registered_faculty.Find(facultyId);
                var jntuh_college_faculty_registered = db.jntuh_college_faculty_registered.Where(e => e.RegistrationNumber.Trim() == regfaculty.RegistrationNumber.Trim()).Select(e => e).FirstOrDefault();
                if (jntuh_college_faculty_registered == null)
                {
                    TempData["Error"] = "Faculty Data not found.";
                    return RedirectToAction("Teaching", "PA_Faculty");
                }
                faculty.id = jntuh_college_faculty_registered.id;
                faculty.collegeId = userCollegeID;
                faculty.facultySurname = regfaculty.FirstName;
                faculty.facultyFirstName = regfaculty.MiddleName;
                faculty.facultyLastName = regfaculty.LastName;
                faculty.isActive = regfaculty.isActive;
                // faculty.facultyAadhaarNumber = regfaculty.AadhaarNumber;
                faculty.facultyAadhaarNumber = jntuh_college_faculty_registered.AadhaarNumber;
                faculty.facultyAadharDocument = jntuh_college_faculty_registered.AadhaarDocument;
                //if (jntuh_college_faculty_registered.AadhaarNumber != null)
                //{

                //    TempData["path"] = true;
                //}
                //else
                //{
                //    faculty.facultyAadhaarNumberNew = regfaculty.AadhaarNumber;
                //    faculty.facultyAadharDocument = regfaculty.AadhaarDocument;
                //    TempData["path"] = null;
                //}

                faculty.facultyDesignationId = regfaculty.DesignationId;
                faculty.designation = db.jntuh_designation.Where(d => d.id == faculty.facultyDesignationId).Select(d => d.designation).FirstOrDefault();
                faculty.facultyOtherDesignation = regfaculty.OtherDesignation;
                faculty.FacultyRegistrationNumber = regfaculty.RegistrationNumber;
                ViewBag.id = jntuh_college_faculty_registered.id;
                faculty.facultyRecruitedFor = jntuh_college_faculty_registered.IdentifiedFor;

                if (jntuh_college_faculty_registered.IdentifiedFor == "UG")
                {
                    faculty.facultyDepartmentId = jntuh_college_faculty_registered.DepartmentId ?? 0;
                }
                else
                {
                    faculty.facultyPGDepartmentId = jntuh_college_faculty_registered.DepartmentId ?? 0;
                    faculty.SpecializationId = jntuh_college_faculty_registered.SpecializationId;
                }

                faculty.FacultySpecalizationId = jntuh_college_faculty_registered.FacultySpecializationId;
                // faculty.facultyDepartmentId = jntuh_college_faculty_registered.DepartmentId ?? 0;
                faculty.department = db.jntuh_department.Where(d => d.id == faculty.facultyDepartmentId).Select(d => d.departmentName).FirstOrDefault();

                int AadhaarCount = db.jntuh_college_faculty_registered_copy.Where(s => s.collegeId == faculty.collegeId && s.RegistrationNumber == faculty.FacultyRegistrationNumber).Select(e => e.collegeId).FirstOrDefault();
                //int CollegeFacultyAadhaarCount = db.jntuh_college_faculty_registered.Where(s => s.collegeId == faculty.collegeId && s.RegistrationNumber == faculty.FacultyRegistrationNumber).Select(e => e.id).FirstOrDefault();

                if ((AadhaarCount == 0 || AadhaarCount == null))
                {
                    TempData["AadhaarCount"] = null;
                }
                else
                {
                    TempData["AadhaarCount"] = AadhaarCount;
                }
            }

            ViewBag.facId = facultyId;
            var jntuh_specialization = db.jntuh_specialization.Where(s => s.isActive == true).Select(e => e).ToList();
            var jntuh_department = db.jntuh_department.Where(s => s.isActive == true).Select(e => e).ToList();
            var jntuh_degree = db.jntuh_degree.Where(s => s.isActive == true).Select(e => e).ToList();


            var jntuh_academic_year = db.jntuh_academic_year.ToList();
            int actualYear = jntuh_academic_year.Where(s => s.isActive == true && s.isPresentAcademicYear == true).Select(s => s.actualYear).FirstOrDefault();

            int AY0 = jntuh_academic_year.Where(s => s.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            int AY1 = jntuh_academic_year.Where(s => s.actualYear == actualYear).Select(s => s.id).FirstOrDefault();
            int AY2 = jntuh_academic_year.Where(s => s.actualYear == (actualYear - 1)).Select(s => s.id).FirstOrDefault();
            int AY3 = jntuh_academic_year.Where(s => s.actualYear == (actualYear - 2)).Select(s => s.id).FirstOrDefault();


            //var jntuh_college_intake_existing = db.jntuh_college_intake_existing.Where(s => s.collegeId == userCollegeID && (s.academicYearId == AY1 || s.academicYearId == AY2 || s.academicYearId == AY3) && s.shiftId == 1).GroupBy(s => new { s.specializationId, s.shiftId }).Select(s => s.FirstOrDefault()).ToList();
            var jntuh_college_intake_existing = db.jntuh_college_intake_existing.Where(s => s.collegeId == userCollegeID && (s.academicYearId == AY0 || s.academicYearId == AY1 || s.academicYearId == AY2 || s.academicYearId == AY3) && s.shiftId == 1).Select(s => s).ToList();
            jntuh_college_intake_existing =
                jntuh_college_intake_existing.GroupBy(s => new { s.specializationId, s.shiftId })
                    .Select(s => s.First())
                    .ToList();

            //Colleges and Designation Drop Dow Codding written by Narayana Reddy on 11-02-2020
            List<SelectListItem> colleges = new List<SelectListItem>();
            var colleges_list = db.jntuh_college.Where(c => c.isActive == true && c.id != 375).Select(c => new { CollegeId = c.id, CollegeName = c.collegeName + " (" + c.collegeCode + ")" }).OrderBy(c => c.CollegeName).ToList();
            colleges = colleges_list.Select(s => new SelectListItem { Value = s.CollegeId.ToString(), Text = s.CollegeName }).ToList();
            colleges.Add(new SelectListItem { Value = "375", Text = "Others" });
            ViewBag.Institutions = colleges;
            var Designation = db.jntuh_designation.Where(e => e.isActive == true).Select(a => new { Id = a.id, designation = a.designation }).Take(4).ToList();
            ViewBag.Designation = Designation;

            List<Departments> Dept = new List<Departments>();
            Dept = (from t in jntuh_college_intake_existing
                    join cf in db.jntuh_specialization on t.specializationId equals cf.id
                    join dd in db.jntuh_department on cf.departmentId equals dd.id
                    join deg in db.jntuh_degree on dd.degreeId equals deg.id
                    join type in db.jntuh_degree_type on deg.degreeTypeId equals type.id
                    where cf.isActive == true && dd.isActive == true && deg.isActive == true
                    //where t.collegeId == userCollegeID && t.academicYearId == 9 && t.shiftId==1
                    select new Departments()
                    {
                        DegreeTypeId = type.id,
                        DepartmentId = dd.id,
                        DepartmentName = deg.degree + "-" + dd.departmentName,
                        specid = cf.id,
                        Specializationname = cf.specializationName

                    }).ToList();


            Dept.Add(new Departments() { DegreeTypeId = 1, DepartmentId = 65, DepartmentName = "Others(CSE/IT)" });
            Dept.Add(new Departments() { DegreeTypeId = 1, DepartmentId = 66, DepartmentName = "Others(CIVIL/MECH)" });
            Dept.Add(new Departments() { DegreeTypeId = 1, DepartmentId = 67, DepartmentName = "Others(ECE/EEE)" });
            Dept.Add(new Departments() { DegreeTypeId = 1, DepartmentId = 68, DepartmentName = "Others(MNGT/H&S)" });
            Dept.Add(new Departments() { DegreeTypeId = 1, DepartmentId = 29, DepartmentName = "Physics" });
            Dept.Add(new Departments() { DegreeTypeId = 1, DepartmentId = 30, DepartmentName = "Mathematics" });
            Dept.Add(new Departments() { DegreeTypeId = 1, DepartmentId = 31, DepartmentName = "English" });
            Dept.Add(new Departments() { DegreeTypeId = 1, DepartmentId = 32, DepartmentName = "Chemistry" });


            ViewBag.departments = Dept.Where(d => d.DegreeTypeId == 1).Select(d => new Departments
            {
                DepartmentId = d.DepartmentId,
                DepartmentName = d.DepartmentName
            }).ToList();

            var PGdept = Dept.Where(d => d.DegreeTypeId == 2 || d.DegreeTypeId == 3).GroupBy(e => e.DepartmentId).Select(e => e.FirstOrDefault()).ToList();
            ViewBag.PGDepartments = PGdept.Select(d => new Departments
            {
                PGdeptid = d.DepartmentId,
                PGDeptname = d.DepartmentName
            }).ToList();

            ViewBag.PGSpecializations = Dept.Where(d => d.DegreeTypeId == 2 || d.DegreeTypeId == 3).Select(d => new Departments
            {
                specid = d.specid,
                Specializationname = d.Specializationname
            }).ToList();


            var MPharmacy_Spec = (from s in jntuh_specialization
                                  join d in jntuh_department on s.departmentId equals d.id
                                  join de in jntuh_degree on d.degreeId equals de.id
                                  where de.id == 2 || de.id == 9 || de.id == 10
                                  select new MpharmacySpec()
                                  {
                                      MPharmacyspecid = s.id,
                                      MPharmacyspecname = s.specializationName
                                  }).ToList();

            ViewBag.MpharmacySpec = MPharmacy_Spec;
            //if its Have Experiance Details

            jntuh_registered_faculty_experience facultyexperiance =
                db.jntuh_registered_faculty_experience.Where(
                    r => r.createdBycollegeId == userCollegeID && r.facultyId == regfaculty.id)
                    .Select(s => s).ToList()
                    .LastOrDefault();
            if (facultyexperiance != null)
            {
                faculty.experienceId = facultyexperiance.Id;
                faculty.facultyId = facultyexperiance.facultyId;
                faculty.Previouscollegeid = (int)facultyexperiance.collegeId;
                faculty.Otherscollegename = facultyexperiance.OtherCollege;
                faculty.facultyDesignationId = facultyexperiance.facultyDesignationId;
                faculty.facultyOtherDesignation = facultyexperiance.OtherDesignation;
                if (facultyexperiance.facultyDateOfAppointment != null)
                {
                    DateTime date = Convert.ToDateTime(facultyexperiance.facultyDateOfAppointment);
                    faculty.dateOfAppointment = date.ToString("dd/MM/yyyy").Split(' ')[0];
                }
                if (facultyexperiance.facultyDateOfResignation != null)
                {
                    DateTime date = Convert.ToDateTime(facultyexperiance.facultyDateOfResignation);
                    faculty.dateOfResignation = date.ToString("dd/MM/yyyy").Split(' ')[0];
                }
                faculty.facultySalary = facultyexperiance.facultySalary;
                //faculty.dateOfAppointment = facultyexperiance.facultyDateOfAppointment.ToString();
                //faculty.dateOfResignation = facultyexperiance.facultyDateOfResignation.ToString();
                faculty.ViewAppointmentOrderDocument = facultyexperiance.facultyJoiningOrder;
                faculty.ViewRelivingDocument = facultyexperiance.facultyRelievingLetter;
                if (faculty.ViewRelivingDocument == null)
                {
                    faculty.facultyfresherexperiance = "Fresher";
                }
                else
                {
                    faculty.facultyfresherexperiance = "Experienced";
                }

                faculty.ViewSelectionCommitteeDocument = facultyexperiance.FacultySCMDocument;
            }

            return PartialView("FacultyRegistrationNumber", faculty);
        }

        //Check Leaves from date and to date
        public ActionResult CheckExperianceDates(string facultyId, string facultyfromdate, string facultytodate)
        {
            MembershipUser userdata = Membership.GetUser(User.Identity.Name);

            if (userdata == null)
            {
                return RedirectToAction("LogOn", "Account");
            }

            var status = false;
            var message = string.Empty;
            var userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);

            var facultyid = db.jntuh_registered_faculty.Where(e => e.UserId == userId).Select(a => a.id).FirstOrDefault();

            if (facultyfromdate != null && facultytodate != null)
            {
                int Fromyear = Convert.ToInt32(facultyfromdate.Split('/')[2]);
                int Frommonth = Convert.ToInt32(facultyfromdate.Split('/')[1]);
                int Fromdate = Convert.ToInt32(facultyfromdate.Split('/')[0]);

                DateTime DOA = new DateTime(Fromyear, Frommonth, Fromdate);

                int Toyear = Convert.ToInt32(facultytodate.Split('/')[2]);
                int Tomonth = Convert.ToInt32(facultytodate.Split('/')[1]);
                int Todate = Convert.ToInt32(facultytodate.Split('/')[0]);

                DateTime DOR = new DateTime(Toyear, Tomonth, Todate);

                if (DOA < DOR)
                {
                    status = true;
                    message = "Date of Appointment should be less than to Date of Resignation";
                    return Json(new { status = status, message = message }, JsonRequestBehavior.AllowGet);
                }
                //var regno =
                //    db.jntuh_registered_faculty.Where(r => r.RegistrationNumber == facultyId)
                //        .Select(s => s.id)
                //        .FirstOrDefault();


                //var Experiance = db.jntuh_registered_faculty_experience.Where(s => s.facultyId == regno && s.createdBycollegeId != null).Select(e => e).ToList();

                //if (Experiance.Count != 0)
                //{
                //    foreach (var item in Experiance)
                //    {

                //        string From = item.facultyDateOfAppointment.ToString().Split(' ')[0];
                //        string To = item.facultyDateOfResignation.ToString().Split(' ')[0];

                //        int Appyear = Convert.ToInt32(From.Split('/')[2]);
                //        int Appmonth = Convert.ToInt32(From.Split('/')[0]);
                //        int Appdate = Convert.ToInt32(From.Split('/')[1]);

                //        DateTime DateOfApp = new DateTime(Appyear, Appmonth, Appdate);

                //        int Resignyear = Convert.ToInt32(To.Split('/')[2]);
                //        int Resignmonth = Convert.ToInt32(To.Split('/')[0]);
                //        int Resigndate = Convert.ToInt32(To.Split('/')[1]);

                //        DateTime DateOfResign = new DateTime(Resignyear, Resignmonth, Resigndate);

                //        if (DateOfApp >= DOA && DateOfResign <= DOR)
                //        {
                //            status = true;
                //            message = "You are already working in this peroid as per your claim";
                //            break;
                //        }
                //        if (DateOfApp >= DOA && DateOfResign >= DOR)
                //        {
                //            if (DOA == DOR)
                //            {
                //                //status = true;
                //                //message = "";
                //                // break;
                //            }
                //            //else if (DOA <= DateOfApp && DOR <= DateOfApp)
                //            //{
                //            //    status = false;
                //            //    message = "";
                //            //    // break;
                //            //}
                //            else if (DOA <= DateOfApp && DOR <= DateOfApp)
                //            {
                //                status = false;
                //                message = "";
                //                // break;
                //            }
                //            else
                //            {
                //                status = true;
                //                message = "You are already working in this peroid as per your claim";
                //                break;
                //            }
                //        }
                //        if (DateOfApp <= DOA && DateOfResign <= DOR)
                //        {
                //            if (DOA == DOR)
                //            {
                //                //status = true;
                //                //message = "";
                //                //break;
                //            }
                //            //else if (DOA >= DateOfResign && DOR > DateOfResign)
                //            //{
                //            //    status = false;
                //            //    message = "";
                //            //    // break;
                //            //}
                //            else if (DOA > DateOfResign && DOR > DateOfResign)
                //            {
                //                status = false;
                //                message = "";
                //                // break;
                //            }
                //            else
                //            {
                //                status = true;
                //                message = "You are already working in this peroid as per your claim";
                //                break;
                //            }
                //        }
                //        if (DateOfApp <= DOA && DateOfResign >= DOR)
                //        {
                //            status = true;
                //            message = "You are already working in this peroid as per your claim";
                //            break;
                //        }
                //    }
                //    return Json(new { status = status, message = message }, JsonRequestBehavior.AllowGet);
                //}
                return Json(new { status = false, message = message }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { status = true, message = "Please Select Date" }, JsonRequestBehavior.AllowGet);
        }

        //In Place Of Faculty
        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        [HttpGet]
        public ActionResult AdocFacultyRegistrationNumber(string collegeId, string fid)
        {
            //return RedirectToAction("Teaching", "Faculty");
            int facultyId = 0;
            TempData["path"] = null;
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);

            int userCollegeID = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userID).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();

            if (userCollegeID == 0 && collegeId != null)
            {
                if (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry"))
                {
                    userCollegeID = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(collegeId, WebConfigurationManager.AppSettings["CryptoKey"]));
                }
            }

            if (!string.IsNullOrEmpty(fid))
            {
                //  facultyId = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(fid, WebConfigurationManager.AppSettings["CryptoKey"]));

                facultyId = Convert.ToInt32(fid);
            }
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            ViewBag.collegeId = UAAAS.Models.Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString();

            CollegeFaculty faculty = new CollegeFaculty();

            if (facultyId != 0)
            {
                jntuh_registered_faculty regfaculty = db.jntuh_registered_faculty.Find(facultyId);

                var jntuh_college_faculty_registered = db.jntuh_college_faculty_registered.Where(e => e.RegistrationNumber.Trim() == regfaculty.RegistrationNumber.Trim()).Select(e => e).FirstOrDefault();
                faculty.id = jntuh_college_faculty_registered.id;
                faculty.collegeId = userCollegeID;
                faculty.facultySurname = regfaculty.FirstName;
                faculty.facultyFirstName = regfaculty.MiddleName;
                faculty.facultyLastName = regfaculty.LastName;
                faculty.isActive = regfaculty.isActive;
                // faculty.facultyAadhaarNumber = regfaculty.AadhaarNumber;
                //faculty.facultyAadhaarNumber = jntuh_college_faculty_registered.AadhaarNumber;
                //faculty.facultyAadharDocument = jntuh_college_faculty_registered.AadhaarDocument;
                //if (jntuh_college_faculty_registered.AadhaarNumber != null)
                //{

                //    TempData["path"] = true;
                //}
                //else
                //{
                //    faculty.facultyAadhaarNumberNew = regfaculty.AadhaarNumber;
                //    faculty.facultyAadharDocument = regfaculty.AadhaarDocument;
                //    TempData["path"] = null;
                //}

                faculty.AdocFacultyRegistrationNumber =
                    db.jntuh_college_faculty_replaceregistered.Where(
                        ad => ad.RegistrationNumber == regfaculty.RegistrationNumber.Trim())
                        .Select(s => s.AdhocRegistrationNumber)
                        .FirstOrDefault();
                faculty.facultyDesignationId = regfaculty.DesignationId;
                faculty.designation = db.jntuh_designation.Where(d => d.id == faculty.facultyDesignationId).Select(d => d.designation).FirstOrDefault();
                faculty.facultyOtherDesignation = regfaculty.OtherDesignation;
                faculty.FacultyRegistrationNumber = regfaculty.RegistrationNumber;
                ViewBag.id = jntuh_college_faculty_registered.id;
                faculty.facultyRecruitedFor = jntuh_college_faculty_registered.IdentifiedFor;

                if (jntuh_college_faculty_registered.IdentifiedFor == "UG")
                {
                    faculty.facultyDepartmentId = jntuh_college_faculty_registered.DepartmentId ?? 0;
                }
                else
                {
                    faculty.facultyPGDepartmentId = jntuh_college_faculty_registered.DepartmentId ?? 0;
                    faculty.SpecializationId = jntuh_college_faculty_registered.SpecializationId;
                }

                faculty.FacultySpecalizationId = jntuh_college_faculty_registered.FacultySpecializationId;
                // faculty.facultyDepartmentId = jntuh_college_faculty_registered.DepartmentId ?? 0;
                faculty.department = db.jntuh_department.Where(d => d.id == faculty.facultyDepartmentId).Select(d => d.departmentName).FirstOrDefault();

                int AadhaarCount = db.jntuh_college_faculty_registered_copy.Where(s => s.collegeId == faculty.collegeId && s.RegistrationNumber == faculty.FacultyRegistrationNumber).Select(e => e.collegeId).FirstOrDefault();
                //int CollegeFacultyAadhaarCount = db.jntuh_college_faculty_registered.Where(s => s.collegeId == faculty.collegeId && s.RegistrationNumber == faculty.FacultyRegistrationNumber).Select(e => e.id).FirstOrDefault();

                if ((AadhaarCount == 0 || AadhaarCount == null))
                {
                    TempData["AadhaarCount"] = null;
                }
                else
                {
                    TempData["AadhaarCount"] = AadhaarCount;
                }
            }

            ViewBag.facId = facultyId;


            var jntuh_specialization = db.jntuh_specialization.Where(s => s.isActive == true).Select(e => e).ToList();
            var jntuh_department = db.jntuh_department.Where(s => s.isActive == true).Select(e => e).ToList();
            var jntuh_degree = db.jntuh_degree.Where(s => s.isActive == true).Select(e => e).ToList();


            var jntuh_academic_year = db.jntuh_academic_year.ToList();
            int actualYear = jntuh_academic_year.Where(s => s.isActive == true && s.isPresentAcademicYear == true).Select(s => s.actualYear).FirstOrDefault();

            int AY1 = jntuh_academic_year.Where(s => s.actualYear == actualYear).Select(s => s.id).FirstOrDefault();
            int AY2 = jntuh_academic_year.Where(s => s.actualYear == (actualYear - 1)).Select(s => s.id).FirstOrDefault();
            int AY3 = jntuh_academic_year.Where(s => s.actualYear == (actualYear - 2)).Select(s => s.id).FirstOrDefault();


            var jntuh_college_intake_existing = db.jntuh_college_intake_existing.Where(s => s.collegeId == userCollegeID && (s.academicYearId == AY1 || s.academicYearId == AY2 || s.academicYearId == AY3) && s.shiftId == 1).GroupBy(s => new { s.specializationId, s.shiftId }).Select(s => s.FirstOrDefault()).ToList();
            List<Departments> Dept = new List<Departments>();
            Dept = (from t in jntuh_college_intake_existing
                    join cf in db.jntuh_specialization on t.specializationId equals cf.id
                    join dd in db.jntuh_department on cf.departmentId equals dd.id
                    join deg in db.jntuh_degree on dd.degreeId equals deg.id
                    join type in db.jntuh_degree_type on deg.degreeTypeId equals type.id
                    where cf.isActive == true && dd.isActive == true && deg.isActive == true
                    //where t.collegeId == userCollegeID && t.academicYearId == 9 && t.shiftId==1
                    select new Departments()
                    {
                        DegreeTypeId = type.id,
                        DepartmentId = dd.id,
                        DepartmentName = deg.degree + "-" + dd.departmentName,
                        specid = cf.id,
                        Specializationname = cf.specializationName

                    }).ToList();


            Dept.Add(new Departments() { DegreeTypeId = 1, DepartmentId = 65, DepartmentName = "Others(CSE/IT)" });
            Dept.Add(new Departments() { DegreeTypeId = 1, DepartmentId = 66, DepartmentName = "Others(CIVIL/MECH)" });
            Dept.Add(new Departments() { DegreeTypeId = 1, DepartmentId = 67, DepartmentName = "Others(ECE/EEE)" });
            Dept.Add(new Departments() { DegreeTypeId = 1, DepartmentId = 68, DepartmentName = "Others(MNGT/H&S)" });
            Dept.Add(new Departments() { DegreeTypeId = 1, DepartmentId = 29, DepartmentName = "Physics" });
            Dept.Add(new Departments() { DegreeTypeId = 1, DepartmentId = 30, DepartmentName = "Mathematics" });
            Dept.Add(new Departments() { DegreeTypeId = 1, DepartmentId = 31, DepartmentName = "English" });
            Dept.Add(new Departments() { DegreeTypeId = 1, DepartmentId = 32, DepartmentName = "Chemistry" });


            ViewBag.departments = Dept.Where(d => d.DegreeTypeId == 1).Select(d => new Departments
            {
                DepartmentId = d.DepartmentId,
                DepartmentName = d.DepartmentName
            }).ToList();

            var PGdept = Dept.Where(d => d.DegreeTypeId == 2 || d.DegreeTypeId == 3).GroupBy(e => e.DepartmentId).Select(e => e.FirstOrDefault()).ToList();
            ViewBag.PGDepartments = PGdept.Select(d => new Departments
            {
                PGdeptid = d.DepartmentId,
                PGDeptname = d.DepartmentName
            }).ToList();

            ViewBag.PGSpecializations = Dept.Where(d => d.DegreeTypeId == 2 || d.DegreeTypeId == 3).Select(d => new Departments
            {
                specid = d.specid,
                Specializationname = d.Specializationname
            }).ToList();


            var MPharmacy_Spec = (from s in jntuh_specialization
                                  join d in jntuh_department on s.departmentId equals d.id
                                  join de in jntuh_degree on d.degreeId equals de.id
                                  where de.id == 2 || de.id == 9 || de.id == 10
                                  select new MpharmacySpec()
                                  {
                                      MPharmacyspecid = s.id,
                                      MPharmacyspecname = s.specializationName
                                  }).ToList();

            ViewBag.MpharmacySpec = MPharmacy_Spec;

            return PartialView("AdocFacultyRegistrationNumber", faculty);
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        [HttpPost]
        public ActionResult AdocFacultyRegistrationNumber(CollegeFaculty faculty)
        {
            TempData["Error"] = null;
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeId =
                db.jntuh_college_users.Where(u => u.userID == userID).Select(s => s.collegeID).FirstOrDefault();
            string aadhaarCardsPath = "~/Content/Upload/Faculty/AADHAARCARDS/AdocCollegeFacultyAadhaar";

            //Black List Condition
            jntuh_registered_faculty isBlacklistFaculty = db.jntuh_registered_faculty.Where(r => r.RegistrationNumber == faculty.FacultyRegistrationNumber).Select(r => r).FirstOrDefault();
            if (isBlacklistFaculty == null)
            {
                TempData["Error"] = "Invalid Faculty Registration Number.";
                return RedirectToAction("ViewTeaching", "PA_Faculty");
            }
            if (isBlacklistFaculty.Blacklistfaculy == true)
            {
                TempData["Error"] = "Faculty Registration Number is in Blacklist.";
                return RedirectToAction("ViewTeaching", "PA_Faculty");
            }
            //
            int[] otherdepartments = new int[] { 65, 66, 67, 68 };
            jntuh_college_faculty_registered checkreg =
                db.jntuh_college_faculty_registered.Where(
                    cf => cf.RegistrationNumber == faculty.AdocFacultyRegistrationNumber).Select(s => s).FirstOrDefault();
            jntuh_college_principal_registered checkprireg =
                db.jntuh_college_principal_registered.Where(
                    pf => pf.RegistrationNumber == faculty.AdocFacultyRegistrationNumber).Select(s => s).FirstOrDefault();
            jntuh_college_faculty_replaceregistered checkreireg =
                db.jntuh_college_faculty_replaceregistered.Where(
                    pf => pf.RegistrationNumber == faculty.AdocFacultyRegistrationNumber).Select(s => s).FirstOrDefault();
            if (checkprireg != null || checkreireg != null)
            {
                TempData["Error"] = "Faculty Registration Number already Exist.";
                return RedirectToAction("ViewTeaching", "PA_Faculty");
            }
            if (checkreg != null)
            {
                int depId = Convert.ToInt32(checkreg.DepartmentId);
                if (!otherdepartments.Contains(depId))
                {
                    TempData["Error"] = "Faculty Registration Number already Exist.";
                    return RedirectToAction("ViewTeaching", "PA_Faculty");
                }
            }
            //Aadhaar Checking Condition
            var status = aadharcard.validateVerhoeff(faculty.AdocAadhaarNumber);

            var aajntuh_college_faculty_registered =
                db.jntuh_college_faculty_registered.Where(
                    f => f.AadhaarNumber == faculty.AdocAadhaarNumber && f.RegistrationNumber != faculty.AdocFacultyRegistrationNumber)
                    .Select(e => e)
                    .Count();
            var jaantuh_college_principal_registered =
          db.jntuh_college_principal_registered.Where(
              f => f.AadhaarNumber == faculty.AdocAadhaarNumber && f.RegistrationNumber != faculty.AdocFacultyRegistrationNumber)
              .Select(e => e)
              .Count();
            var aajntuh_college_faculty_replaceregistered =
          db.jntuh_college_faculty_replaceregistered.Where(
              f => f.AadhaarNumber == faculty.AdocAadhaarNumber && f.RegistrationNumber != faculty.AdocFacultyRegistrationNumber)
              .Select(e => e)
              .Count();
            if (status)
            {
                if (aajntuh_college_faculty_registered == 0 && jaantuh_college_principal_registered == 0 && aajntuh_college_faculty_replaceregistered == 0)
                {

                }

                else
                {
                    TempData["Error"] = "Aadhar Number exist.";
                    return RedirectToAction("ViewTeaching", "PA_Faculty");
                }

            }
            else
            {
                TempData["Error"] = "AadhaarNumber is not a validnumber";
                return RedirectToAction("ViewTeaching", "PA_Faculty");
                // return Json("AadhaarNumber is not a validnumber", JsonRequestBehavior.AllowGet);
            }
            jntuh_registered_faculty jntuh_registered_faculty =
                db.jntuh_registered_faculty.Where(rf => rf.RegistrationNumber == faculty.AdocFacultyRegistrationNumber)
                    .Select(s => s)
                    .FirstOrDefault();
            jntuh_college_faculty_registered jntuh_college_faculty_registered =
                db.jntuh_college_faculty_registered.Where(
                    cf => cf.RegistrationNumber == faculty.FacultyRegistrationNumber).Select(s => s).FirstOrDefault();
            if (jntuh_college_faculty_registered != null)
            {
                if (faculty.AdocAadhaarDocument != null)
                {
                    if (!Directory.Exists(Server.MapPath(aadhaarCardsPath)))
                    {
                        Directory.CreateDirectory(Server.MapPath(aadhaarCardsPath));
                    }

                    var ext = Path.GetExtension(faculty.AdocAadhaarDocument.FileName);

                    if (ext.ToUpper().Equals(".JPEG") || ext.ToUpper().Equals(".JPG"))
                    {
                        //if (faculty.facultyAadharDocument == null)
                        //{
                        string fileName = DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" +
                                          jntuh_registered_faculty.FirstName.Substring(0, 1) + "-" +
                                          jntuh_registered_faculty.LastName.Substring(0, 1);
                        faculty.AdocAadhaarDocument.SaveAs(string.Format("{0}/{1}{2}",
                            Server.MapPath(aadhaarCardsPath),
                            fileName, ext));
                        faculty.AdocAadhaarDocumentView = string.Format("{0}{1}", fileName, ext);
                        //}                      
                    }

                }
                jntuh_college_faculty_replaceregistered jntuh_college_faculty_replaceregistered = new jntuh_college_faculty_replaceregistered();
                jntuh_college_faculty_replaceregistered.collegeId = jntuh_college_faculty_registered.collegeId;
                jntuh_college_faculty_replaceregistered.RegistrationNumber = jntuh_college_faculty_registered.RegistrationNumber;
                jntuh_college_faculty_replaceregistered.existingFacultyId = jntuh_college_faculty_registered.existingFacultyId;
                jntuh_college_faculty_replaceregistered.IdentifiedFor = jntuh_college_faculty_registered.IdentifiedFor;
                jntuh_college_faculty_replaceregistered.SpecializationId = jntuh_college_faculty_registered.SpecializationId;
                jntuh_college_faculty_replaceregistered.DepartmentId = jntuh_college_faculty_registered.DepartmentId;
                jntuh_college_faculty_replaceregistered.FacultySpecializationId = faculty.FacultySpecalizationId;
                jntuh_college_faculty_replaceregistered.isActive = true;
                jntuh_college_faculty_replaceregistered.createdOn = DateTime.Now;
                jntuh_college_faculty_replaceregistered.createdBy = userID;
                //Adoc Faculy Details
                jntuh_college_faculty_replaceregistered.AdhocRegistrationNumber = faculty.AdocFacultyRegistrationNumber;
                jntuh_college_faculty_replaceregistered.AadhaarNumber = faculty.AdocAadhaarNumber;
                jntuh_college_faculty_replaceregistered.AadhaarDocument = faculty.AdocAadhaarDocumentView;

                db.jntuh_college_faculty_replaceregistered.Add(jntuh_college_faculty_replaceregistered);
                db.SaveChanges();
                TempData["Success"] = "Faculty Added Successfully.";
            }

            else
            {
                TempData["Error"] = "No Aadhaar Number For Faculty Registration Number.";
                return RedirectToAction("Teaching", "PA_Faculty");
            }
            return RedirectToAction("ViewTeaching", "PA_Faculty");
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        [HttpPost]
        public ActionResult FacultyRegistrationNumber(CollegeFaculty faculty)
        {
            //return RedirectToAction("Index", "UnderConstruction");
            TempData["Error"] = null;
            // return RedirectToAction("Teaching", "Faculty");
            if (Membership.GetUser() == null)
            {
                return RedirectToAction("LogOn", "Account");
            }
            string aadhaarCardsPath = "~/Content/Upload/Faculty/AADHAARCARDS/CollegeFacultyAadhaar";
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userID).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();
            //Aadhaar Number Validation
            bool status = false;
            if (!String.IsNullOrEmpty(faculty.facultyAadhaarNumber))
            {
                status = aadharcard.validateVerhoeff(faculty.facultyAadhaarNumber.Trim());
            }
            else
            {
                TempData["Error"] = "AadhaarNumber is not a validnumber.";
                return RedirectToAction("Teaching", "PA_Faculty");
            }
            var jntuh_college_faculty_registered =
                db.jntuh_college_faculty_registered.Where(
                    f => f.AadhaarNumber.Trim() == faculty.facultyAadhaarNumber.Trim() && f.RegistrationNumber.Trim() != faculty.FacultyRegistrationNumber.Trim())
                    .Select(e => e)
                    .Count();
            var jntuh_college_principal_registered =
          db.jntuh_college_principal_registered.Where(
              f => f.AadhaarNumber.Trim() == faculty.facultyAadhaarNumber.Trim() && f.RegistrationNumber != faculty.FacultyRegistrationNumber.Trim())
              .Select(e => e)
              .Count();
            if (status)
            {
                if (jntuh_college_faculty_registered != 0 || jntuh_college_principal_registered != 0)
                {
                    TempData["Error"] = "AadhaarNumber already Exists.";
                    return RedirectToAction("Teaching", "PA_Faculty");
                }
            }
            else
            {
                TempData["Error"] = "AadhaarNumber is not a validnumber.";
                return RedirectToAction("Teaching", "PA_Faculty");
            }

            // userCollegeID = 263;
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            int actualYear = db.jntuh_academic_year.Where(s => s.isActive == true && s.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int presentAyId = db.jntuh_academic_year.Where(s => s.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            jntuh_registered_faculty isRegisteredFaculty = db.jntuh_registered_faculty.Where(r => r.RegistrationNumber == faculty.FacultyRegistrationNumber.Trim()).Select(r => r).FirstOrDefault();
            if (isRegisteredFaculty == null)
            {
                TempData["Error"] = "Invalid Faculty Registration Number.";
                //return RedirectToAction("Teaching", "Faculty", new { collegeId = UAAAS.Models.Utilities.EncryptString(faculty.collegeId.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
                return RedirectToAction("Teaching", "PA_Faculty");
            }

            //jntuh_registered_faculty isBlacklistFaculty = db.jntuh_registered_faculty.Where(r => r.RegistrationNumber == faculty.FacultyRegistrationNumber && r.Blacklistfaculy == true).Select(r => r).FirstOrDefault();

            if (isRegisteredFaculty.Blacklistfaculy == true)
            {
                TempData["Error"] = "Faculty Registration Number is in Blacklist.";
                return RedirectToAction("Teaching", "PA_Faculty");
            }
            if (isRegisteredFaculty.AbsentforVerification == true)
            {
                TempData["Error"] = "Faculty Registration Number is in Inactive.";
                return RedirectToAction("Teaching", "PA_Faculty");
            }
            jntuh_college_faculty_registered isExistingFaculty = db.jntuh_college_faculty_registered.Where(r => r.RegistrationNumber.Trim() == faculty.FacultyRegistrationNumber.Trim()).Select(r => r).FirstOrDefault();
            //int id = db.jntuh_college_principal_registered.Where(r=>r.RegistrationNumber==faculty.FacultyRegistrationNumber && r.collegeId != faculty.collegeId).Select(e=>e.id).FirstOrDefault();
            int id = db.jntuh_college_principal_registered.Where(r => r.RegistrationNumber.Trim() == faculty.FacultyRegistrationNumber.Trim() && r.collegeId != userCollegeID).Select(e => e.id).FirstOrDefault();

            if (id != 0)
            {
                TempData["Error"] = "Faculty is already working in other JNTUH affiliated college.";
                return RedirectToAction("Teaching", "PA_Faculty");
            }



            if (isRegisteredFaculty.WorkingStatus == true && isExistingFaculty != null)
            {
                if (userCollegeID != isRegisteredFaculty.collegeId && isRegisteredFaculty.collegeId != null &&
                    isRegisteredFaculty.RegistrationNumber == isExistingFaculty.RegistrationNumber)
                {
                    if (userCollegeID == isExistingFaculty.collegeId)
                    {
                        TempData["Error"] = "Faculty is already working in your college.";
                    }
                    else
                    {
                        TempData["Error"] = "Faculty is already working in other JNTUH affiliated college.";
                    }

                    //  return RedirectToAction("Teaching", "Faculty", new { collegeId = UAAAS.Models.Utilities.EncryptString(faculty.collegeId.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
                    return RedirectToAction("Teaching", "PA_Faculty");
                }
            }

            FacultyAttedance jntuh_registered_facultys = new FacultyAttedance();
            var jntuh_registrared_faculty_log = db.jntuh_registered_faculty_log.Where(r => r.RegistrationNumber.Trim() == faculty.FacultyRegistrationNumber.Trim()).Select(r => r).FirstOrDefault();
            var jntuh_registered_facultyget = db.jntuh_registered_faculty.Where(F => F.RegistrationNumber.Trim() == faculty.FacultyRegistrationNumber.Trim() && F.Blacklistfaculy != true).Select(F => F).FirstOrDefault();
            if (jntuh_registered_facultyget != null)
            {
                if (jntuh_registrared_faculty_log != null)
                {
                    if (!string.IsNullOrEmpty(jntuh_registrared_faculty_log.AadhaarNumber))
                    {
                        jntuh_registered_facultys = db.jntuh_registered_faculty_log.Where(F => F.RegistrationNumber.Trim() == faculty.FacultyRegistrationNumber.Trim()).Select(F => new FacultyAttedance
                        {
                            RegistrationNumber = F.RegistrationNumber.Trim(),
                            FirstName = F.FirstName,
                            LastName = F.LastName,
                            Mobile = F.Mobile,
                            Email = F.Email,
                            AadhaarNumber = F.AadhaarNumber,
                            GenderId = F.GenderId,
                            DateOfBirth = F.DateOfBirth,
                            id = F.RegFacultyId
                        }).FirstOrDefault();
                    }
                }
                else //if (!string.IsNullOrEmpty(jntuh_registered_facultyget.AadhaarNumber))
                {
                    jntuh_registered_facultys = db.jntuh_registered_faculty.Where(F => F.RegistrationNumber.Trim() == faculty.FacultyRegistrationNumber.Trim() && F.Blacklistfaculy != true).Select(F => new FacultyAttedance
                    {
                        id = F.id,
                        RegistrationNumber = F.RegistrationNumber,
                        FirstName = F.FirstName,
                        LastName = F.LastName,
                        Mobile = F.Mobile,
                        Email = F.Email,
                        AadhaarNumber = F.AadhaarNumber,
                        GenderId = F.GenderId,
                        DateOfBirth = F.DateOfBirth
                    }).FirstOrDefault();

                }

            }

            if (jntuh_registered_facultys != null)
            {
                //if (string.IsNullOrEmpty(jntuh_registered_facultys.AadhaarNumber))
                //{
                //    TempData["Error"] = "No Aadhaar Number For Faculty Registration Number.";
                //    return RedirectToAction("Teaching", "Faculty");
                //}
            }
            else
            {
                TempData["Error"] = "No Aadhaar Number For Faculty Registration Number.";
                return RedirectToAction("Teaching", "PA_Faculty");
            }

            if (isExistingFaculty != null)
            {
                if (userCollegeID != isExistingFaculty.collegeId && isExistingFaculty.collegeId != null)
                {
                    TempData["Error"] = "Faculty is already working in other JNTUH affiliated college.";
                }
                else if (userCollegeID == isExistingFaculty.collegeId)
                {
                    TempData["Error"] = "Faculty is already working in your college";
                }
                //  return RedirectToAction("Teaching", "Faculty", new { collegeId = UAAAS.Models.Utilities.EncryptString(faculty.collegeId.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
                return RedirectToAction("Teaching", "PA_Faculty");
            }

            if (TempData["Error"] == null)
            {
                jntuh_college_faculty_registered eFaculty = new jntuh_college_faculty_registered();
                //var CollegeFacultydata = db.jntuh_college_faculty_registered.Where(r => r.AadhaarNumber.Trim() == faculty.facultyAadhaarNumber.Trim()).Select(r => r).FirstOrDefault();

                eFaculty.collegeId = userCollegeID;
                eFaculty.RegistrationNumber = faculty.FacultyRegistrationNumber.Trim();
                eFaculty.IdentifiedFor = faculty.facultyRecruitedFor;
                if (eFaculty.IdentifiedFor == "UG")
                {
                    eFaculty.DepartmentId = faculty.facultyDepartmentId;
                    eFaculty.SpecializationId = null;
                }
                else
                {
                    eFaculty.DepartmentId = faculty.facultyPGDepartmentId;
                    eFaculty.SpecializationId = faculty.SpecializationId;
                }
                eFaculty.FacultySpecializationId = null;
                eFaculty.existingFacultyId = jntuh_registered_facultys.id;
                eFaculty.createdBy = userID;
                eFaculty.createdOn = DateTime.Now;
                eFaculty.updatedBy = null;
                eFaculty.updatedOn = null;
                eFaculty.isActive = false;
                eFaculty.AadhaarNumber = faculty.facultyAadhaarNumber.Trim();

                if (faculty.facultyAadharPhotoDocument != null)
                {
                    if (!Directory.Exists(Server.MapPath(aadhaarCardsPath)))
                    {
                        Directory.CreateDirectory(Server.MapPath(aadhaarCardsPath));
                    }

                    var ext = Path.GetExtension(faculty.facultyAadharPhotoDocument.FileName);

                    if (ext.ToUpper().Equals(".JPEG") || ext.ToUpper().Equals(".JPG"))
                    {
                        string fileName = DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" +
                        jntuh_registered_facultyget.FirstName.Substring(0, 1) + "-" + jntuh_registered_facultyget.LastName.Substring(0, 1);
                        faculty.facultyAadharPhotoDocument.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(aadhaarCardsPath),
                            fileName, ext));
                        eFaculty.AadhaarDocument = string.Format("{0}{1}", fileName, ext);
                    }
                }
                else
                {
                    TempData["Message"] = "File Upload is Missing.Please Fill the Details Again";
                    return RedirectToAction("Teaching", "PA_Faculty", new { collegeId = UAAAS.Models.Utilities.EncryptString(faculty.collegeId.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
                }
                db.jntuh_college_faculty_registered.Add(eFaculty);
                db.SaveChanges();

                #region Faculty Experince Details Saving
                jntuh_registered_faculty_experience facultyexperiance = new jntuh_registered_faculty_experience();
                //
                string facultyappointmentletters = "~/Content/Upload/College/Faculty/AppointmentLetters";
                string facultyrelevingletters = "~/Content/Upload/College/Faculty/RelevingLetters";
                string facultypreviouscollegescms = "~/Content/Upload/College/Faculty/PreviousSCMs";

                facultyexperiance.facultyId = jntuh_registered_facultys.id;
                facultyexperiance.createdBycollegeId = userCollegeID;
                if (faculty.AppointmentOrderDocument != null)
                {
                    if (!Directory.Exists(Server.MapPath(facultyappointmentletters)))
                    {
                        Directory.CreateDirectory(Server.MapPath(facultyappointmentletters));
                    }

                    var ext = Path.GetExtension(faculty.AppointmentOrderDocument.FileName);

                    if (ext.ToUpper().Equals(".PDF"))
                    {
                        string fileName = DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" +
                        jntuh_registered_facultyget.FirstName.Substring(0, 1) + "-" + jntuh_registered_facultyget.LastName.Substring(0, 1);
                        faculty.AppointmentOrderDocument.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(facultyappointmentletters),
                            fileName, ext));
                        faculty.ViewAppointmentOrderDocument = string.Format("{0}{1}", fileName, ext);
                        facultyexperiance.facultyJoiningOrder = faculty.ViewAppointmentOrderDocument;
                    }
                }

                if (faculty.RelivingDocument != null)
                {
                    if (!Directory.Exists(Server.MapPath(facultyrelevingletters)))
                    {
                        Directory.CreateDirectory(Server.MapPath(facultyrelevingletters));
                    }

                    var ext = Path.GetExtension(faculty.RelivingDocument.FileName);

                    if (ext.ToUpper().Equals(".PDF"))
                    {
                        string fileName = DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" +
                        jntuh_registered_facultyget.FirstName.Substring(0, 1) + "-" + jntuh_registered_facultyget.LastName.Substring(0, 1);
                        faculty.RelivingDocument.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(facultyrelevingletters),
                            fileName, ext));
                        faculty.ViewRelivingDocument = string.Format("{0}{1}", fileName, ext);
                        facultyexperiance.facultyRelievingLetter = faculty.ViewRelivingDocument;
                    }
                }

                //Optional
                if (faculty.SelectionCommitteeDocument != null)
                {
                    if (!Directory.Exists(Server.MapPath(facultypreviouscollegescms)))
                    {
                        Directory.CreateDirectory(Server.MapPath(facultypreviouscollegescms));
                    }

                    var ext = Path.GetExtension(faculty.SelectionCommitteeDocument.FileName);

                    if (ext.ToUpper().Equals(".PDF"))
                    {
                        string fileName = DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" +
                        jntuh_registered_facultyget.FirstName.Substring(0, 1) + "-" + jntuh_registered_facultyget.LastName.Substring(0, 1);
                        faculty.SelectionCommitteeDocument.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(facultypreviouscollegescms),
                            fileName, ext));
                        faculty.ViewSelectionCommitteeDocument = string.Format("{0}{1}", fileName, ext);
                        facultyexperiance.FacultySCMDocument = faculty.ViewSelectionCommitteeDocument;
                    }
                }
                if (faculty.Previouscollegeid == 0)
                {
                    facultyexperiance.collegeId = userCollegeID;
                }
                else
                {
                    if (faculty.Previouscollegeid == 375)
                    {
                        facultyexperiance.collegeId = faculty.Previouscollegeid;
                        facultyexperiance.OtherCollege = faculty.Otherscollegename;
                    }
                    else
                    {
                        facultyexperiance.collegeId = faculty.Previouscollegeid;
                    }
                }


                if (faculty.facultyDesignationId == 4)
                {
                    facultyexperiance.facultyDesignationId = faculty.facultyDesignationId;
                    facultyexperiance.OtherDesignation = faculty.facultyOtherDesignation;
                }
                else
                {
                    facultyexperiance.facultyDesignationId = faculty.facultyDesignationId;
                }
                if (faculty.facultySalary != null)
                    facultyexperiance.facultySalary = faculty.facultySalary.Trim();

                if (!String.IsNullOrEmpty(faculty.dateOfAppointment))
                    facultyexperiance.facultyDateOfAppointment = Models.Utilities.DDMMYY2MMDDYY(faculty.dateOfAppointment);
                if (!String.IsNullOrEmpty(faculty.dateOfResignation))
                    facultyexperiance.facultyDateOfResignation =
                    Models.Utilities.DDMMYY2MMDDYY(faculty.dateOfResignation);

                facultyexperiance.createdBy = userID;
                facultyexperiance.createdOn = DateTime.Now;
                facultyexperiance.updatedBy = null;
                facultyexperiance.updatedOn = null;
                db.jntuh_registered_faculty_experience.Add(facultyexperiance);
                db.SaveChanges();

                #endregion


                jntuh_college_facultytracking objART = new jntuh_college_facultytracking();
                //jntuh_attendence_registrationnumberstracking objART = new jntuh_attendence_registrationnumberstracking();
                objART.academicYearId = presentAyId;
                objART.collegeId = eFaculty.collegeId;
                objART.RegistrationNumber = eFaculty.RegistrationNumber.Trim();
                objART.DepartmentId = eFaculty.DepartmentId;
                objART.SpecializationId = eFaculty.SpecializationId;
                objART.ActionType = 1;
                objART.FacultyType = "Faculty";
                objART.FacultyStatus = "Y";
                objART.Reasion = "Faculty Insert";
                // objART.FacultyStatus = "Y";
                //  objART.Reasion = "Faculty Deleted by College Successfully.";
                //objART.FacultyJoinDate = eFaculty.createdOn;
                objART.previousworkingcollegeid = faculty.Previouscollegeid;
                objART.scmdocument = facultyexperiance.FacultySCMDocument;
                objART.FacultyJoinDate = Models.Utilities.DDMMYY2MMDDYY(faculty.dateOfAppointment);
                objART.FacultyJoinDocument = facultyexperiance.facultyJoiningOrder;
                objART.relevingdate = Models.Utilities.DDMMYY2MMDDYY(faculty.dateOfResignation);
                objART.relevingdocumnt = facultyexperiance.facultyRelievingLetter;
                objART.aadhaarnumber = faculty.facultyAadhaarNumber.Trim();
                objART.aadhaardocument = eFaculty.AadhaarDocument;
                objART.payscale = facultyexperiance.facultySalary;
                objART.designation = facultyexperiance.facultyDesignationId != null ? facultyexperiance.facultyDesignationId.ToString() : null;
                objART.isActive = true;
                objART.Createdon = DateTime.Now;
                objART.CreatedBy = userID;
                objART.Updatedon = null;
                objART.UpdatedBy = null;
                db.jntuh_college_facultytracking.Add(objART);
                db.SaveChanges();
                //  ABAS.GetAffiliationDataSoapClient ObJabas = new ABAS.GetAffiliationDataSoapClient();
                //  ObJabas.Faculty(jntuh_registered_facultys.FirstName + "-" + jntuh_registered_facultys.LastName, jntuh_registered_facultys.RegistrationNumber, jntuh_registered_facultys.AadhaarNumber, userCollegeID.ToString(), faculty.facultyDepartmentId.ToString(), "", "", "", "", jntuh_registered_facultys.GenderId == 1 ? "Male" : "Female", jntuh_registered_facultys.DateOfBirth.ToString(), jntuh_registered_facultys.Mobile, jntuh_registered_facultys.Email, "", "1");

                TempData["Success"] = "Faculty Added Successfully.";
            }
            return RedirectToAction("Teaching", "PA_Faculty", new { collegeId = UAAAS.Models.Utilities.EncryptString(faculty.collegeId.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
            //    return RedirectToAction("Teaching", "Faculty", new { collegeId = UAAAS.Models.Utilities.EncryptString(faculty.collegeId.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
        }

        [HttpGet]
        public JsonResult GetSpecializations(string id)
        {
            object Spec = null;
            int Deptid = Convert.ToInt32(id);
            int userid = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userid).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();
            // userCollegeID = 318;
            var jntuh_academic_year = db.jntuh_academic_year.ToList();
            int actualYear = jntuh_academic_year.Where(s => s.isActive == true && s.isPresentAcademicYear == true).Select(s => s.actualYear).FirstOrDefault();

            int AY0 = jntuh_academic_year.Where(s => s.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            int AY1 = jntuh_academic_year.Where(s => s.actualYear == actualYear).Select(s => s.id).FirstOrDefault();
            int AY2 = jntuh_academic_year.Where(s => s.actualYear == (actualYear - 1)).Select(s => s.id).FirstOrDefault();
            int AY3 = jntuh_academic_year.Where(s => s.actualYear == (actualYear - 2)).Select(s => s.id).FirstOrDefault();

            var DeptNewData = (from d in db.jntuh_department
                               join s in db.jntuh_specialization on d.id equals s.departmentId
                               select new
                               {
                                   Deptid = d.id,
                                   spec = s.id,
                                   specname = s.specializationName

                               }).ToList();
            DeptNewData = DeptNewData.Where(s => s.Deptid == Deptid).ToList();
            var SpecalizationIds = DeptNewData.Select(s => s.spec).ToArray();
            var intake = db.jntuh_college_intake_existing.Where(e => e.collegeId == userCollegeID && (e.academicYearId == AY0 || e.academicYearId == AY1 || e.academicYearId == AY2 || e.academicYearId == AY3) && SpecalizationIds.Contains(e.specializationId)).Select(e => e.specializationId).ToArray();
            var Data = db.jntuh_specialization.Where(s => intake.Contains(s.id))
                .Select(e => new
                {
                    Specid = e.id,
                    Specname = e.specializationName
                }).ToList();
            ViewBag.PGSpecializations = Data;
            return Json(new { data = Data }, "application/json", JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetFacultySpecializations(int id)
        {
            if (id != 0)
            {
                var facultydata = db.jntuh_college_faculty_registered.Where(s => s.id == id).Select(s => s.SpecializationId).FirstOrDefault();
                facultydata = facultydata == null ? 0 : facultydata;
                var facultySpec = db.jntuh_specialization.Where(s => s.id == facultydata).Select(s => new { specid = s.id, specializationame = s.specializationName }).FirstOrDefault();
                return Json(new { data = facultySpec }, "application/json", JsonRequestBehavior.AllowGet);
            }
            else
            {

                return Json(new { data = "" }, "application/json", JsonRequestBehavior.AllowGet);
            }

        }

        public class Departments
        {
            public int DepartmentId { get; set; }
            public string DepartmentName { get; set; }
            public int specid { get; set; }
            public string Specializationname { get; set; }
            public int PGdeptid { get; set; }
            public string PGDeptname { get; set; }
            public int Totaldeptid { get; set; }
            public string Totaldeptname { get; set; }

            public int DegreeTypeId { get; set; }
        }

        public class MpharmacySpec
        {
            public int MPharmacyspecid { get; set; }
            public string MPharmacyspecname { get; set; }
        }

        // Added by Naushad Khan
        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        [HttpGet]
        public ActionResult AddFaculty(string collegeId, string fid)
        {
            //return RedirectToAction("Index", "UnderConstruction");
            //return RedirectToAction("Teaching", "Faculty");
            int facultyId = 0;
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userID).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();
            // userCollegeID = 263;
            if (userCollegeID == 0 && collegeId != null)
            {
                if (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry"))
                {
                    userCollegeID = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(collegeId, WebConfigurationManager.AppSettings["CryptoKey"]));
                }
            }
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            var jntuh_specialization = db.jntuh_specialization.Where(s => s.isActive == true).Select(e => e).ToList();
            var jntuh_department = db.jntuh_department.Where(s => s.isActive == true).Select(e => e).ToList();
            var jntuh_degree = db.jntuh_degree.Where(s => s.isActive == true).Select(e => e).ToList();

            //Colleges and Designation Drop Dow Codding written by Narayana Reddy on 11-02-2020
            List<SelectListItem> colleges = new List<SelectListItem>();
            var colleges_list = db.jntuh_college.Where(c => c.isActive == true && c.id != 375).Select(c => new { CollegeId = c.id, CollegeName = c.collegeName + " (" + c.collegeCode + ")" }).OrderBy(c => c.CollegeName).ToList();
            colleges = colleges_list.Select(s => new SelectListItem { Value = s.CollegeId.ToString(), Text = s.CollegeName }).ToList();
            colleges.Add(new SelectListItem { Value = "375", Text = "Others" });
            ViewBag.Institutions = colleges;
            var Designation = db.jntuh_designation.Where(e => e.isActive == true).Select(a => new { Id = a.id, designation = a.designation }).Take(4).ToList();
            ViewBag.Designation = Designation;



            var jntuh_academic_year = db.jntuh_academic_year.ToList();
            int actualYear = jntuh_academic_year.Where(s => s.isActive == true && s.isPresentAcademicYear == true).Select(s => s.actualYear).FirstOrDefault();

            int AY0 = jntuh_academic_year.Where(s => s.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            int AY1 = jntuh_academic_year.Where(s => s.actualYear == actualYear).Select(s => s.id).FirstOrDefault();
            int AY2 = jntuh_academic_year.Where(s => s.actualYear == (actualYear - 1)).Select(s => s.id).FirstOrDefault();
            int AY3 = jntuh_academic_year.Where(s => s.actualYear == (actualYear - 2)).Select(s => s.id).FirstOrDefault();



            // var jntuh_college_intake_existing = db.jntuh_college_intake_existing.Where(s => s.collegeId == userCollegeID && (s.academicYearId == AY1 || s.academicYearId == AY2 || s.academicYearId == AY3) && s.shiftId == 1).GroupBy(s => new { s.specializationId, s.shiftId }).Select(s => s.FirstOrDefault()).ToList();
            var jntuh_college_intake_existing = db.jntuh_college_intake_existing.Where(s => s.collegeId == userCollegeID && (s.academicYearId == AY0 || s.academicYearId == AY1 || s.academicYearId == AY2 || s.academicYearId == AY3) && s.shiftId == 1).Select(s => s).ToList();
            jntuh_college_intake_existing =
                jntuh_college_intake_existing.GroupBy(s => new { s.specializationId, s.shiftId }).Select(s => s.First()).ToList();
            List<Departments> Dept = new List<Departments>();
            Dept = (from t in jntuh_college_intake_existing
                    join cf in jntuh_specialization on t.specializationId equals cf.id
                    join dd in jntuh_department on cf.departmentId equals dd.id
                    join deg in jntuh_degree on dd.degreeId equals deg.id
                    join type in db.jntuh_degree_type on deg.degreeTypeId equals type.id
                    // where cf.isActive == true && dd.isActive == true && deg.isActive == true
                    //where t.collegeId == userCollegeID && t.academicYearId == 9 && t.shiftId==1
                    select new Departments()
                    {
                        DegreeTypeId = type.id,
                        DepartmentId = dd.id,
                        DepartmentName = deg.degree + "-" + dd.departmentName,
                        specid = cf.id,
                        Specializationname = cf.specializationName

                    }).ToList();
            //Dept = Dept.Distinct().ToList();


            Dept.Add(new Departments() { DegreeTypeId = 1, DepartmentId = 65, DepartmentName = "Others(CSE/IT)" });
            Dept.Add(new Departments() { DegreeTypeId = 1, DepartmentId = 66, DepartmentName = "Others(CIVIL/MECH)" });
            Dept.Add(new Departments() { DegreeTypeId = 1, DepartmentId = 67, DepartmentName = "Others(ECE/EEE)" });
            Dept.Add(new Departments() { DegreeTypeId = 1, DepartmentId = 68, DepartmentName = "Others(MNGT/H&S)" });
            Dept.Add(new Departments() { DegreeTypeId = 1, DepartmentId = 29, DepartmentName = "Physics" });
            Dept.Add(new Departments() { DegreeTypeId = 1, DepartmentId = 30, DepartmentName = "Mathematics" });
            Dept.Add(new Departments() { DegreeTypeId = 1, DepartmentId = 31, DepartmentName = "English" });
            Dept.Add(new Departments() { DegreeTypeId = 1, DepartmentId = 32, DepartmentName = "Chemistry" });


            ViewBag.departments = Dept.Where(d => d.DegreeTypeId == 1).Select(d => new Departments
            {
                DepartmentId = d.DepartmentId,
                DepartmentName = d.DepartmentName
            }).ToList();

            var PGdept = Dept.Where(d => d.DegreeTypeId == 2 || d.DegreeTypeId == 3).GroupBy(e => e.DepartmentId).Select(e => e.FirstOrDefault()).ToList();
            ViewBag.PGDepartments = PGdept.Select(d => new Departments
            {
                PGdeptid = d.DepartmentId,
                PGDeptname = d.DepartmentName
            }).ToList();

            ViewBag.PGSpecializations = Dept.Where(d => d.DegreeTypeId == 2 || d.DegreeTypeId == 3).Select(d => new Departments
            {
                specid = d.specid,
                Specializationname = d.Specializationname
            }).ToList();


            var MPharmacy_Spec = (from s in jntuh_specialization
                                  join d in jntuh_department on s.departmentId equals d.id
                                  join de in jntuh_degree on d.degreeId equals de.id
                                  where de.id == 2 || de.id == 9 || de.id == 10
                                  select new MpharmacySpec()
                                  {
                                      MPharmacyspecid = s.id,
                                      MPharmacyspecname = s.specializationName
                                  }).ToList();

            ViewBag.MpharmacySpec = MPharmacy_Spec;
            CollegeFaculty faculty = new CollegeFaculty();

            return PartialView("AddFaculty", faculty);
        }

        // new Action added by Naushad Khan
        //  [Authorize(Roles = "Admin,College")]
        [Authorize(Roles = "Admin,College")]
        public ActionResult ViewTeachingStaff(string id, string strType)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userID).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();
            // int userCollegeID = 214;
            if (userCollegeID == 0)
            {
                userCollegeID = userCollegeID = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(id, WebConfigurationManager.AppSettings["CryptoKey"]));
            }
            DateTime todayDate = DateTime.Now.Date;
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy =
                    db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();

            var status = GetPageEditableStatus(userCollegeID);

            if (status > 0 && Roles.IsUserInRole("College"))
            {

                bool isPageEditable = db.jntuh_pa_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("PFA") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

                if (isPageEditable)
                {
                    //  return RedirectToAction("Teaching");
                }
            }
            return View();
            #region Unused code

            //        collegeFaculty.facultyEmail = rFaculty.Email;
            //        collegeFaculty.facultyMobile = rFaculty.Mobile;
            //        collegeFaculty.facultyPANNumber = rFaculty.PANNumber;
            //        collegeFaculty.facultyAadhaarNumber = rFaculty.AadhaarNumber;
            //        collegeFaculty.FacultyRegistrationNumber = rFaculty.RegistrationNumber;
            //        collegeFaculty.facultyCount = RegistredFacultyLog.Count(F => F.RegistrationNumber == rFaculty.RegistrationNumber);

            //        collegeFaculty.FacultyVerificationStatus = RegistredFacultyLog.Where(F => F.RegistrationNumber == rFaculty.RegistrationNumber.Trim()).Select(F => F.FacultyApprovedStatus).FirstOrDefault();
            //        collegeFaculty.Remarks = RegistredFacultyLog.Where(F => F.RegistrationNumber == rFaculty.RegistrationNumber.Trim()).Select(F => F.Remarks).FirstOrDefault();
            //        string Reasons = "";
            //        //if (rFaculty.InvalidPANNumber == true)
            //        //{
            //        //    Reasons += "Invalid PANNumber" + ",";
            //        //}
            //        //if (rFaculty.DiscrepencyStatus == true)
            //        //{
            //        //    Reasons += "DiscrepencyStatus" + ",";
            //        //}
            //        //if (rFaculty.IncompleteCertificates == true)
            //        //{
            //        //    Reasons += "Incomplete Certificates" + ",";
            //        //}



            //        if (rFaculty.Absent == true)
            //        {
            //            Reasons = "ABSENT" + ",";
            //        }
            //        if (rFaculty.NotQualifiedAsperAICTE == true)
            //        {
            //            Reasons += "NOT QUALIFIED " + ",";
            //        }
            //        if (string.IsNullOrEmpty(rFaculty.PANNumber))
            //        {
            //            Reasons += "NO PAN" + ",";
            //        }
            //        if (rFaculty.DepartmentId == null)
            //        {
            //            Reasons += "NO DEPARTMENT" + ",";
            //        }
            //        if (rFaculty.NoSCM == true)
            //        {
            //            Reasons += "NO SCM/RATIFICATION" + ",";
            //        }
            //        if (rFaculty.PHDundertakingnotsubmitted == true)
            //        {
            //            Reasons += "NO UNDERTAKING" + ",";
            //        }
            //        if (rFaculty.Blacklistfaculy == true)
            //        {
            //            Reasons += "BLACKLISTED" + ",";
            //        }

            //        if (Reasons != "")
            //        {
            //            Reasons = Reasons.Substring(0, Reasons.Length - 1);
            //        }

            //        collegeFaculty.Reason = Reasons;


            //        teachingFaculty.Add(collegeFaculty);
            //    }
            //    var jntuh_college_faculty_registereds = db.jntuh_college_faculty_registered.Where(C => C.collegeId == userCollegeID).Select(C => C).ToList();
            //    jntuh_college_faculty_registered eFaculty = null;
            //    var jntuh_designations = db.jntuh_designation.Select(d => new { d.id, d.designation }).ToList();
            //    var jntuh_departments = db.jntuh_department.Select(d => new { d.id, d.departmentName }).ToList();
            //    foreach (var faculty in jntuh_college_faculty)
            //    {
            //        CollegeFaculty collegeFaculty = new CollegeFaculty();
            //        collegeFaculty.id = faculty.id;

            //        eFaculty = jntuh_college_faculty_registereds.Where(r => r.existingFacultyId == faculty.id).Select(r => r).FirstOrDefault();

            //        if (eFaculty != null)
            //        {
            //            collegeFaculty.FacultyRegistrationNumber = eFaculty.RegistrationNumber;
            //        }

            //        if (string.IsNullOrEmpty(collegeFaculty.FacultyRegistrationNumber))
            //        {
            //            if (eFaculty != null)
            //            {
            //                collegeFaculty.collegeId = faculty.collegeId;
            //                collegeFaculty.facultyTypeId = faculty.facultyTypeId;
            //                collegeFaculty.facultyFirstName = faculty.facultyFirstName;
            //                collegeFaculty.facultyLastName = faculty.facultyLastName;
            //                collegeFaculty.facultySurname = faculty.facultySurname;
            //                collegeFaculty.facultyGenderId = faculty.facultyGenderId;
            //                collegeFaculty.facultyFatherName = faculty.facultyFatherName;
            //                collegeFaculty.facultyCategoryId = faculty.facultyCategoryId;

            //                if (faculty.facultyDateOfBirth != null)
            //                    collegeFaculty.dateOfBirth = UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.facultyDateOfBirth.ToString());

            //                collegeFaculty.facultyDesignationId = faculty.facultyDesignationId;
            //                collegeFaculty.designation = jntuh_designations.Where(d => d.id == faculty.facultyDesignationId).Select(d => d.designation).FirstOrDefault();
            //                collegeFaculty.facultyOtherDesignation = faculty.facultyOtherDesignation;
            //                collegeFaculty.facultyDepartmentId = faculty.facultyDepartmentId;
            //                collegeFaculty.department = jntuh_departments.Where(d => d.id == faculty.facultyDepartmentId).Select(d => d.departmentName).FirstOrDefault();

            //                if (faculty.facultyDateOfAppointment != null)
            //                    collegeFaculty.dateOfAppointment = UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.facultyDateOfAppointment.ToString());

            //                if (faculty.facultyDateOfResignation != null)
            //                    collegeFaculty.dateOfResignation = UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.facultyDateOfResignation.ToString());

            //                collegeFaculty.isFacultyRatifiedByJNTU = faculty.isFacultyRatifiedByJNTU;

            //                if (faculty.facultyDateOfRatification != null)
            //                    collegeFaculty.dateOfRatification = UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.facultyDateOfRatification.ToString());

            //                collegeFaculty.facultyDurationOfRatification = faculty.facultyDurationOfRatification;
            //                collegeFaculty.facultySalary = faculty.facultySalary;
            //                collegeFaculty.facultyPreviousExperience = Convert.ToInt32(faculty.facultyPreviousExperience);
            //                collegeFaculty.facultyPayScale = faculty.facultyPayScale;
            //                collegeFaculty.facultyEmail = faculty.facultyEmail;
            //                collegeFaculty.facultyMobile = faculty.facultyMobile;
            //                collegeFaculty.facultyPANNumber = faculty.facultyPANNumber;
            //                collegeFaculty.facultyAadhaarNumber = faculty.facultyAadhaarNumber;
            //                collegeFaculty.isRelatedToExamBranch = faculty.isRelatedToExamBranch;
            //                collegeFaculty.isRelatedToPlacementCell = faculty.isRelatedToPlacementCell;
            //                collegeFaculty.facultyAchievements1 = faculty.facultyAchievements1;
            //                collegeFaculty.facultyAchievements2 = faculty.facultyAchievements2;
            //                collegeFaculty.facultyAchievements3 = faculty.facultyAchievements3;
            //                collegeFaculty.FacultyRegistrationNumber = string.Empty;

            //                collegeFaculty.facultyCount = RegistredFacultyLog.Count(F => F.RegistrationNumber == eFaculty.RegistrationNumber.Trim());
            //                collegeFaculty.Remarks = RegistredFacultyLog.Where(F => F.RegistrationNumber == eFaculty.RegistrationNumber.Trim()).Select(F => F.Remarks).FirstOrDefault();
            //                collegeFaculty.FacultyVerificationStatus = RegistredFacultyLog.Where(F => F.RegistrationNumber == eFaculty.RegistrationNumber.Trim()).Select(F => F.FacultyApprovedStatus).FirstOrDefault();

            //            }

            //        }
            //        else
            //        {
            //            if (eFaculty != null)
            //            {
            //                jntuh_registered_faculty rFaculty = jntuh_registered_facultys.Where(f => f.RegistrationNumber == eFaculty.RegistrationNumber.Trim()).Select(f => f).FirstOrDefault();

            //                if (rFaculty.collegeId != null)
            //                {
            //                    collegeFaculty.collegeId = (int)rFaculty.collegeId;
            //                }
            //                collegeFaculty.id = rFaculty.id;
            //                collegeFaculty.facultyType = rFaculty.type;
            //                //collegeFaculty.facultyTypeId = rFaculty.facultyTypeId;
            //                collegeFaculty.facultyFirstName = rFaculty.FirstName;
            //                collegeFaculty.facultyLastName = rFaculty.MiddleName;
            //                collegeFaculty.facultySurname = rFaculty.LastName;
            //                collegeFaculty.facultyGenderId = rFaculty.GenderId;
            //                collegeFaculty.facultyFatherName = rFaculty.FatherOrHusbandName;
            //                //collegeFaculty.facultyCategoryId = rFaculty.facultyCategoryId;

            //                if (rFaculty.DateOfBirth != null)
            //                    collegeFaculty.dateOfBirth = UAAAS.Models.Utilities.MMDDYY2DDMMYY(rFaculty.DateOfBirth.ToString());

            //                if (rFaculty.DesignationId != null)
            //                {
            //                    collegeFaculty.facultyDesignationId = (int)rFaculty.DesignationId;
            //                    collegeFaculty.designation = jntuh_designations.Where(d => d.id == collegeFaculty.facultyDesignationId).Select(d => d.designation).FirstOrDefault();
            //                }

            //                collegeFaculty.facultyOtherDesignation = rFaculty.OtherDesignation;

            //                if (rFaculty.DepartmentId != null)
            //                {
            //                    collegeFaculty.facultyDepartmentId = (int)rFaculty.DepartmentId;
            //                    collegeFaculty.department = jntuh_departments.Where(d => d.id == collegeFaculty.facultyDepartmentId).Select(d => d.departmentName).FirstOrDefault();
            //                }

            //                collegeFaculty.facultyOtherDepartment = rFaculty.OtherDepartment;
            //                collegeFaculty.facultyEmail = rFaculty.Email;
            //                collegeFaculty.facultyMobile = rFaculty.Mobile;
            //                collegeFaculty.facultyPANNumber = rFaculty.PANNumber;
            //                collegeFaculty.facultyAadhaarNumber = rFaculty.AadhaarNumber;
            //                collegeFaculty.FacultyRegistrationNumber = rFaculty.RegistrationNumber;
            //                collegeFaculty.facultyCount = RegistredFacultyLog.Count(F => F.RegistrationNumber == eFaculty.RegistrationNumber.Trim());
            //                collegeFaculty.Remarks = RegistredFacultyLog.Where(F => F.RegistrationNumber == eFaculty.RegistrationNumber.Trim()).Select(F => F.Remarks).FirstOrDefault();
            //                collegeFaculty.FacultyVerificationStatus = RegistredFacultyLog.Where(F => F.RegistrationNumber == eFaculty.RegistrationNumber.Trim()).Select(F => F.FacultyApprovedStatus).FirstOrDefault();

            //                string Reasons = "";
            //                if (rFaculty.InvalidPANNumber == true)
            //                {
            //                    Reasons += "Invalid PANNumber" + ",";
            //                }
            //                if (rFaculty.DiscrepencyStatus == true)
            //                {
            //                    Reasons += "Discrepency Status" + ",";
            //                }
            //                if (rFaculty.IncompleteCertificates == true)
            //                {
            //                    Reasons += "Incomplete Certificates" + ",";
            //                }
            //                if (Reasons != "")
            //                {
            //                    Reasons = Reasons.Substring(0, Reasons.Length - 1);
            //                }

            //                collegeFaculty.Reason = Reasons;

            //            }

            //            teachingFaculty.Add(collegeFaculty);
            //        }

            //    }

            //    ViewBag.Type = UAAAS.Models.Utilities.EncryptString(type.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]);
            //    //ViewBag.Type = 0;
            //    ViewBag.Id = UAAAS.Models.Utilities.EncryptString("0", WebConfigurationManager.AppSettings["CryptoKey"]);
            //    //ViewBag.Id = 0;
            //    ViewBag.Count = teachingFaculty.Count();
            //    return View(teachingFaculty);
            //    #endregion

            // ViewBag.Type = UAAAS.Models.Utilities.EncryptString(type.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]);
            //ViewBag.Type = 0;
            //  ViewBag.Id = UAAAS.Models.Utilities.EncryptString("0", WebConfigurationManager.AppSettings["CryptoKey"]);
            //ViewBag.Id = 0;
            #endregion
        }



        [HttpPost]
        [Authorize(Roles = "Admin,College")]
        public JsonResult ViewTeachingJson(string id, string strType)
        {

            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userID).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();
            //  int userCollegeID = 214;
            // userCollegeID = 23;
            if (userCollegeID == 0)
            {
                userCollegeID = userCollegeID = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(id, WebConfigurationManager.AppSettings["CryptoKey"]));
            }
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            int type = db.jntuh_faculty_type.Where(t => t.facultyType == "Teaching").Select(t => t.id).FirstOrDefault();
            int adhoctype = db.jntuh_faculty_type.Where(t => t.facultyType == "Teaching").Select(t => t.id).FirstOrDefault();
            List<CollegeFaculty> newteachingFaculty = new List<CollegeFaculty>();

            var jntuh_departments = db.jntuh_department.AsNoTracking().ToList();
            var jntuh_designations = db.jntuh_designation.AsNoTracking().ToList();
            var jntuh_specializations = db.jntuh_specialization.AsNoTracking().ToList();

            //var TeachingFacultyData = (from cf in db.jntuh_college_faculty_registered
            //                           join t in db.jntuh_registered_faculty on cf.RegistrationNumber.Trim() equals t.RegistrationNumber.Trim()
            //                           where cf.collegeId == userCollegeID 
            //                           //&& t.Notin116 != true
            //                           select new
            //                           {
            //                               t.FirstName,
            //                               t.MiddleName,
            //                               t.LastName,
            //                               t.RegistrationNumber,
            //                               t.id,
            //                               collegeid = cf.id,
            //                               depId = cf.DepartmentId,
            //                               specId = cf.SpecializationId,
            //                               IdentfdFor = cf.IdentifiedFor,
            //                               t.DesignationId,
            //                               cf.DepartmentId,
            //                               t.Absent,
            //                               t.NotQualifiedAsperAICTE,
            //                               t.PANNumber,
            //                               t.NoSCM,
            //                               t.PHDundertakingnotsubmitted,
            //                               t.Blacklistfaculy,
            //                              // ds.designation,
            //                              // depName = dp.departmentName
            //                           }).ToList();


            var TeachingFacultyData = db.jntuh_college_faculty_registered.Join(db.jntuh_registered_faculty,
                   CLGREG => CLGREG.RegistrationNumber, REG => REG.RegistrationNumber,
                   (CLGREG, REG) => new { CLGREG = CLGREG, REG = REG }).Where(e => e.CLGREG.collegeId == userCollegeID).Select(e => new
                   {
                       e.REG.FirstName,
                       e.REG.MiddleName,
                       e.REG.LastName,
                       e.REG.RegistrationNumber,
                       e.REG.id,
                       collegeid = e.CLGREG.id,

                       e.REG.DesignationId,
                       e.CLGREG.DepartmentId,
                       e.REG.Absent,
                       e.REG.NotQualifiedAsperAICTE,
                       e.REG.PANNumber,
                       depId = e.CLGREG.DepartmentId,
                       specId = e.CLGREG.SpecializationId,
                       IdentfdFor = e.CLGREG.IdentifiedFor,
                       e.REG.NoSCM,
                       e.REG.PHDundertakingnotsubmitted,
                       e.REG.Blacklistfaculy,
                       e.REG.AbsentforVerification
                   }).ToList();


            foreach (var data in TeachingFacultyData)
            {
                CollegeFaculty newcollegeFaculty = new CollegeFaculty();
                newcollegeFaculty.FacultyRegistrationNumber = data.RegistrationNumber;
                newcollegeFaculty.facultyFirstName = data.FirstName + " " + data.MiddleName + " " + data.LastName;
                if (data.Blacklistfaculy == true)
                {
                    newcollegeFaculty.BlackList = true;
                    newcollegeFaculty.facultyFirstName = newcollegeFaculty.facultyFirstName + "<span style='color: red;font-weight: bold'> &nbsp;&nbsp; (Blacklisted)</span>";
                }
                if (data.AbsentforVerification == true)
                {
                    newcollegeFaculty.BlackList = true;
                    newcollegeFaculty.facultyFirstName = newcollegeFaculty.facultyFirstName + "</br><span style='color: red;font-weight: bold'> &nbsp;&nbsp; (Absent for Physical Verification)</span>";
                }
                newcollegeFaculty.facultyLastName = data.LastName;
                newcollegeFaculty.SpecializationId = data.specId;
                jntuh_college_faculty_replaceregistered jntuh_college_faculty_replaceregistered =
                    db.jntuh_college_faculty_replaceregistered.Where(
                        rc => rc.RegistrationNumber == data.RegistrationNumber)
                        .Select(s => s).FirstOrDefault();
                if (jntuh_college_faculty_replaceregistered != null)
                {
                    newcollegeFaculty.AdocFacultyRegistrationNumber = jntuh_college_faculty_replaceregistered.AdhocRegistrationNumber;
                    newcollegeFaculty.adhocid = jntuh_college_faculty_replaceregistered.id;
                    newcollegeFaculty.AdhocfacultyTypeId = adhoctype;
                    newcollegeFaculty.AdhocfacultyCategoryId = jntuh_college_faculty_replaceregistered.id;
                }

                // newcollegeFaculty.facultyAadhaarNumber = data.Aadhaarno;
                // newcollegeFaculty.facultyAadharDocument = data.AadhaarDoc;
                //    newcollegeFaculty.facultyRecruitedFor = data.IdentfdFor;
                if (data.DepartmentId != null)
                {
                    int deptId = (int)data.DepartmentId;
                    newcollegeFaculty.department = jntuh_departments.Where(e => e.id == deptId).Select(e => e.departmentName).FirstOrDefault();
                }
                if (data.DesignationId != null)
                {
                    int desigId = (int)data.DesignationId;
                    newcollegeFaculty.designation = jntuh_designations.Where(e => e.id == desigId).Select(e => e.designation).FirstOrDefault();
                }
                if (data.specId != null)
                {
                    int SpecId = (int)data.specId;
                    newcollegeFaculty.SpecializationName = jntuh_specializations.Where(e => e.id == SpecId).Select(e => e.specializationName).FirstOrDefault();
                }
                newcollegeFaculty.facultyCategoryId = data.collegeid;
                newcollegeFaculty.facultyTypeId = type;
                newcollegeFaculty.id = data.id;

                newcollegeFaculty.sfacid = UAAAS.Models.Utilities.EncryptString(newcollegeFaculty.id.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]);

                //    newcollegeFaculty.id = data.id;
                #region Code for Reason
                //string Reasons = "";
                //if (data.Absent == true)
                //{
                //    Reasons = "ABSENT" + ",";
                //}
                //if (data.NotQualifiedAsperAICTE == true)
                //{
                //    Reasons += "NOT QUALIFIED " + ",";
                //}
                //if (string.IsNullOrEmpty(data.PANNumber))
                //{
                //    Reasons += "NO PAN" + ",";
                //}
                //if (data.DepartmentId == null)
                //{
                //    Reasons += "NO DEPARTMENT" + ",";
                //}
                //if (data.NoSCM == true)
                //{
                //    Reasons += "NO SCM/RATIFICATION" + ",";
                //}
                //if (data.PHDundertakingnotsubmitted == true)
                //{
                //    Reasons += "NO UNDERTAKING" + ",";
                //}
                //if (data.Blacklistfaculy == true)
                //{
                //    Reasons += "BLACKLISTED" + ",";
                //}

                //if (Reasons != "")
                //{
                //    Reasons = Reasons.Substring(0, Reasons.Length - 1);
                //}


                //newcollegeFaculty.Reason = Reasons;
                #endregion


                newteachingFaculty.Add(newcollegeFaculty);
            }

            ViewBag.Count = newteachingFaculty.Count();

            return Json(newteachingFaculty, "application/json", JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,College")]
        public JsonResult ViewAdhocTeachingJson(string id, string strType)
        {

            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userID).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();
            //  int userCollegeID = 214;
            // userCollegeID = 23;
            if (userCollegeID == 0)
            {
                userCollegeID = userCollegeID = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(id, WebConfigurationManager.AppSettings["CryptoKey"]));
            }
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            int type = db.jntuh_faculty_type.Where(t => t.facultyType == "Teaching").Select(t => t.id).FirstOrDefault();
            int adhoctype = db.jntuh_faculty_type.Where(t => t.facultyType == "Teaching").Select(t => t.id).FirstOrDefault();
            List<CollegeFaculty> newteachingFaculty = new List<CollegeFaculty>();

            var jntuh_departments = db.jntuh_department.AsNoTracking().ToList();
            var jntuh_designations = db.jntuh_designation.AsNoTracking().ToList();
            var jntuh_specializations = db.jntuh_specialization.AsNoTracking().ToList();

            var TeachingFacultyData = db.jntuh_college_faculty_replaceregistered.Join(db.jntuh_registered_faculty,
                   CLGREG => CLGREG.RegistrationNumber, REG => REG.RegistrationNumber,
                   (CLGREG, REG) => new { CLGREG = CLGREG, REG = REG }).Where(e => e.CLGREG.collegeId == userCollegeID).Select(e => new
                   {
                       e.REG.FirstName,
                       e.REG.MiddleName,
                       e.REG.LastName,
                       e.REG.RegistrationNumber,
                       e.REG.id,
                       collegeid = e.CLGREG.id,

                       e.REG.DesignationId,
                       e.CLGREG.DepartmentId,
                       e.REG.Absent,
                       e.REG.NotQualifiedAsperAICTE,
                       e.REG.PANNumber,
                       depId = e.CLGREG.DepartmentId,
                       specId = e.CLGREG.SpecializationId,
                       IdentfdFor = e.CLGREG.IdentifiedFor,
                       e.REG.NoSCM,
                       e.REG.PHDundertakingnotsubmitted,
                       e.REG.Blacklistfaculy
                   }).ToList();

            List<jntuh_college_faculty_replaceregistered> jntuh_college_faculty_replaceregisteredList =
                db.jntuh_college_faculty_replaceregistered.Where(s => s.collegeId == userCollegeID)
                    .Select(s => s)
                    .ToList();



            foreach (var data in TeachingFacultyData)
            {
                CollegeFaculty newcollegeFaculty = new CollegeFaculty();
                newcollegeFaculty.FacultyRegistrationNumber = data.RegistrationNumber;
                newcollegeFaculty.facultyFirstName = data.FirstName + " " + data.MiddleName + " " + data.LastName;
                newcollegeFaculty.facultyLastName = data.LastName;
                newcollegeFaculty.SpecializationId = data.specId;
                jntuh_college_faculty_replaceregistered jntuh_college_faculty_replaceregistered =
                    jntuh_college_faculty_replaceregisteredList.Where(
                        rc => rc.RegistrationNumber == data.RegistrationNumber)
                        .Select(s => s).FirstOrDefault();
                if (jntuh_college_faculty_replaceregistered != null)
                {
                    newcollegeFaculty.AdocFacultyRegistrationNumber = jntuh_college_faculty_replaceregistered.AdhocRegistrationNumber;
                    newcollegeFaculty.adhocid = jntuh_college_faculty_replaceregistered.id;
                    newcollegeFaculty.AdhocfacultyTypeId = adhoctype;
                    newcollegeFaculty.AdocFacultyId =
                        db.jntuh_college_faculty_registered.Where(
                            rf =>
                                rf.RegistrationNumber == jntuh_college_faculty_replaceregistered.AdhocRegistrationNumber)
                            .Select(s => s.id)
                            .FirstOrDefault();
                    newcollegeFaculty.adhocregid = UAAAS.Models.Utilities.EncryptString(data.id.ToString(),
                        System.Web.Configuration.WebConfigurationManager.AppSettings["CryptoKey"]);
                    newcollegeFaculty.AdhocfacultyCategoryId = jntuh_college_faculty_replaceregistered.id;
                }

                // newcollegeFaculty.facultyAadhaarNumber = data.Aadhaarno;
                // newcollegeFaculty.facultyAadharDocument = data.AadhaarDoc;
                //    newcollegeFaculty.facultyRecruitedFor = data.IdentfdFor;
                if (data.DepartmentId != null)
                {
                    int deptId = (int)data.DepartmentId;
                    newcollegeFaculty.department = jntuh_departments.Where(e => e.id == deptId).Select(e => e.departmentName).FirstOrDefault();
                }
                if (data.DesignationId != null)
                {
                    int desigId = (int)data.DesignationId;
                    newcollegeFaculty.designation = jntuh_designations.Where(e => e.id == desigId).Select(e => e.designation).FirstOrDefault();
                }
                if (data.specId != null)
                {
                    int SpecId = (int)data.specId;
                    newcollegeFaculty.SpecializationName = jntuh_specializations.Where(e => e.id == SpecId).Select(e => e.specializationName).FirstOrDefault();
                }
                newcollegeFaculty.facultyCategoryId = data.collegeid;
                newcollegeFaculty.facultyTypeId = type;
                newcollegeFaculty.id = data.id;


                //    newcollegeFaculty.id = data.id;
                #region Code for Reason
                //string Reasons = "";
                //if (data.Absent == true)
                //{
                //    Reasons = "ABSENT" + ",";
                //}
                //if (data.NotQualifiedAsperAICTE == true)
                //{
                //    Reasons += "NOT QUALIFIED " + ",";
                //}
                //if (string.IsNullOrEmpty(data.PANNumber))
                //{
                //    Reasons += "NO PAN" + ",";
                //}
                //if (data.DepartmentId == null)
                //{
                //    Reasons += "NO DEPARTMENT" + ",";
                //}
                //if (data.NoSCM == true)
                //{
                //    Reasons += "NO SCM/RATIFICATION" + ",";
                //}
                //if (data.PHDundertakingnotsubmitted == true)
                //{
                //    Reasons += "NO UNDERTAKING" + ",";
                //}
                //if (data.Blacklistfaculy == true)
                //{
                //    Reasons += "BLACKLISTED" + ",";
                //}

                //if (Reasons != "")
                //{
                //    Reasons = Reasons.Substring(0, Reasons.Length - 1);
                //}


                //newcollegeFaculty.Reason = Reasons;
                #endregion


                newteachingFaculty.Add(newcollegeFaculty);
            }

            ViewBag.Count = newteachingFaculty.Count();

            return Json(newteachingFaculty, "application/json", JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = "Admin,College")]
        public ActionResult ViewTeaching(string id, string strType)
        {
            //if (!User.IsInRole("Admin"))
            //{
            //    return RedirectToAction("Index", "UnderConstruction");
            //}        
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userID).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy =
                db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            if (userCollegeID == 0)
            {
                userCollegeID = userCollegeID = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(id, WebConfigurationManager.AppSettings["CryptoKey"]));
            }
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            DateTime todayDate = DateTime.Now.Date;

            var status = GetPageEditableStatus(userCollegeID);

            if (status > 0 && Roles.IsUserInRole("College"))
            {

                bool isPageEditable = db.jntuh_pa_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("PFA") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

                if (isPageEditable)
                {
                    return RedirectToAction("Teaching");
                }
            }

            #region
            //    List<CollegeFaculty> newteachingFaculty = new List<CollegeFaculty>();

            //var TeachingFacultyData = (from t in db.jntuh_registered_faculty
            //         join cf in db.jntuh_college_faculty_registered on t.RegistrationNumber equals cf.RegistrationNumber
            //         join ds in db.jntuh_designation on t.DesignationId equals ds.id
            //         join dp in db.jntuh_department on t.DepartmentId equals dp.id
            //         where t.collegeId == userCollegeID && t.Notin116!=true
            //         select new { t.FirstName, t.MiddleName, t.LastName, t.RegistrationNumber,t.id,t.DesignationId,t.DepartmentId, t.Absent, t.NotQualifiedAsperAICTE, t.PANNumber, t.NoSCM, t.PHDundertakingnotsubmitted, t.Blacklistfaculy,ds.designation, dp.departmentDescription}).ToList();

            //             //    ,concat(t.Absent case when t.Absent==true then 'Absent' end , t.NotQualifiedAsperAICTE case when t.NotQualifiedAsperAICTE== true then 'NOT QUALIFIED' end , t.PANNumbe case when t.PANNumber== true then 'NO PAN' end,t.NoSCM case when t.NoSCM==true then 'No SCM/ RATIFICATION' end, t.PHDundertakingnotsubmitted case when t.PHDundertakingnotsubmitted==true then 'NO UNDERTAKING' end, t.Blacklistfaculy case when t.Blacklistfaculy==true then 'BLACKLISTED'  end )


            //// Fname = Concat(t.FirstName,t.MiddleName,t.LastName),
            //foreach (var data in TeachingFacultyData)
            //{
            //    CollegeFaculty newcollegeFaculty = new CollegeFaculty();
            //    newcollegeFaculty.FacultyRegistrationNumber = data.RegistrationNumber;
            //    newcollegeFaculty.facultyFirstName = data.FirstName;
            //    newcollegeFaculty.facultyLastName = data.LastName;
            //    newcollegeFaculty.department = data.departmentDescription;
            //    newcollegeFaculty.designation = data.designation;


            //    newcollegeFaculty.id = data.id;

            //    string Reasons = "";
            //    if (data.Absent == true)
            //    {
            //        Reasons = "ABSENT" + ",";
            //    }
            //    if (data.NotQualifiedAsperAICTE == true)
            //    {
            //        Reasons += "NOT QUALIFIED " + ",";
            //    }
            //    if (string.IsNullOrEmpty(data.PANNumber))
            //    {
            //        Reasons += "NO PAN" + ",";
            //    }
            //    if (data.DepartmentId == null)
            //    {
            //        Reasons += "NO DEPARTMENT" + ",";
            //    }
            //    if (data.NoSCM == true)
            //    {
            //        Reasons += "NO SCM/RATIFICATION" + ",";
            //    }
            //    if (data.PHDundertakingnotsubmitted == true)
            //    {
            //        Reasons += "NO UNDERTAKING" + ",";
            //    }
            //    if (data.Blacklistfaculy == true)
            //    {
            //        Reasons += "BLACKLISTED" + ",";
            //    }

            //    if (Reasons != "")
            //    {
            //        Reasons = Reasons.Substring(0, Reasons.Length - 1);
            //    }


            //    newcollegeFaculty.Reason = Reasons;



            //    newteachingFaculty.Add(newcollegeFaculty);
            //}

            #endregion

            #region ViewPriviusCode
            //int type = db.jntuh_faculty_type.Where(t => t.facultyType == "Teaching").Select(t => t.id).FirstOrDefault();
            //List<jntuh_college_faculty> jntuh_college_faculty = db.jntuh_college_faculty.Where(f => f.facultyTypeId == type && f.collegeId == userCollegeID).ToList();

            //List<CollegeFaculty> teachingFaculty = new List<CollegeFaculty>();


            //foreach (var faculty in jntuh_college_faculty)
            //{
            //    CollegeFaculty collegeFaculty = new CollegeFaculty();
            //    collegeFaculty.id = faculty.id;
            //    collegeFaculty.collegeId = faculty.collegeId;
            //    collegeFaculty.facultyTypeId = faculty.facultyTypeId;
            //    collegeFaculty.facultyFirstName = faculty.facultyFirstName;
            //    collegeFaculty.facultyLastName = faculty.facultyLastName;
            //    collegeFaculty.facultySurname = faculty.facultySurname;
            //    collegeFaculty.facultyGenderId = faculty.facultyGenderId;
            //    collegeFaculty.facultyFatherName = faculty.facultyFatherName;
            //    collegeFaculty.facultyCategoryId = faculty.facultyCategoryId;

            //    if (faculty.facultyDateOfBirth != null)
            //        collegeFaculty.dateOfBirth = UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.facultyDateOfBirth.ToString());

            //    collegeFaculty.facultyDesignationId = faculty.facultyDesignationId;
            //    collegeFaculty.designation = db.jntuh_designation.Where(d => d.id == faculty.facultyDesignationId).Select(d => d.designation).FirstOrDefault();
            //    collegeFaculty.facultyOtherDesignation = faculty.facultyOtherDesignation;
            //    collegeFaculty.facultyDepartmentId = faculty.facultyDepartmentId;
            //    collegeFaculty.department = db.jntuh_department.Where(d => d.id == faculty.facultyDepartmentId).Select(d => d.departmentName).FirstOrDefault();

            //    if (faculty.facultyDateOfAppointment != null)
            //        collegeFaculty.dateOfAppointment = UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.facultyDateOfAppointment.ToString());

            //    if (faculty.facultyDateOfResignation != null)
            //        collegeFaculty.dateOfResignation = UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.facultyDateOfResignation.ToString());

            //    collegeFaculty.isFacultyRatifiedByJNTU = faculty.isFacultyRatifiedByJNTU;

            //    if (faculty.facultyDateOfRatification != null)
            //        collegeFaculty.dateOfRatification = UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.facultyDateOfRatification.ToString());

            //    collegeFaculty.facultyDurationOfRatification = faculty.facultyDurationOfRatification;
            //    collegeFaculty.facultySalary = faculty.facultySalary;
            //    collegeFaculty.facultyPreviousExperience = Convert.ToInt32(faculty.facultyPreviousExperience);
            //    collegeFaculty.facultyPayScale = faculty.facultyPayScale;
            //    collegeFaculty.facultyEmail = faculty.facultyEmail;
            //    collegeFaculty.facultyMobile = faculty.facultyMobile;
            //    collegeFaculty.facultyPANNumber = faculty.facultyPANNumber;
            //    collegeFaculty.facultyAadhaarNumber = faculty.facultyAadhaarNumber;
            //    collegeFaculty.isRelatedToExamBranch = faculty.isRelatedToExamBranch;
            //    collegeFaculty.isRelatedToPlacementCell = faculty.isRelatedToPlacementCell;
            //    collegeFaculty.facultyAchievements1 = faculty.facultyAchievements1;
            //    collegeFaculty.facultyAchievements2 = faculty.facultyAchievements2;
            //    collegeFaculty.facultyAchievements3 = faculty.facultyAchievements3;
            //    collegeFaculty.photo = faculty.facultyPhoto;


            //    //Getting faculty Education
            //    //collegeFaculty.FacultyEducation = db.jntuh_faculty_education.Where(fe => fe.facultyId == faculty.id).Select(fe => fe.courseStudied).ToList();

            //    collegeFaculty.FacultyEducation = (from fc in db.jntuh_education_category
            //                                       join fe in db.jntuh_faculty_education on fc.id equals fe.educationId into fec
            //                                       from ec in fec.Where(fe => fe.facultyId == faculty.id).DefaultIfEmpty()
            //                                       select new FacultyEducation
            //                                       {
            //                                           studiedEducation = ec.courseStudied
            //                                       }).ToList();

            //    teachingFaculty.Add(collegeFaculty);


            //}
            ////Export Teaching Faculty Details-16-09-2014
            //if (strType == "Export" && teachingFaculty.Count() > 0)
            //{
            //    Response.ClearContent();
            //    Response.Buffer = true;
            //    Response.AddHeader("content-disposition", "attachment; filename=Teaching Faculty Details.xls");
            //    Response.ContentType = "application/vnd.ms-excel";
            //    return PartialView("~/Views/Reports/_TeachingFacultyReport.cshtml", teachingFaculty);
            //}
            ////
            //ViewBag.Type = UAAAS.Models.Utilities.EncryptString(type.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]);
            ////ViewBag.Type = type;
            ////ViewBag.Id = UAAAS.Models.Utilities.Encrypt("0", WebConfigurationManager.AppSettings["CryptoKey"]);
            //ViewBag.Id = 0;
            //ViewBag.Count = teachingFaculty.Count();
            //if (Roles.IsUserInRole("Admin") == true)
            //{
            //    ViewBag.Admin = true;
            //}
            //return View("ViewTeaching", teachingFaculty); 
            #endregion

            #region Newcodeweneed to change
            //int type = db.jntuh_faculty_type.Where(t => t.facultyType == "Teaching").Select(t => t.id).FirstOrDefault();
            //List<jntuh_college_faculty> jntuh_college_faculty = db.jntuh_college_faculty.Where(f => f.facultyTypeId == type && f.collegeId == userCollegeID)
            //    .OrderBy(f => f.facultyDepartmentId).ThenBy(f => f.facultyDesignationId).ThenBy(f => f.facultyFirstName).ToList();

            //List<CollegeFaculty> teachingFaculty = new List<CollegeFaculty>();
            //// Commented BY Srinivas.T
            ////List<jntuh_college_faculty_registered> regFaculty = db.jntuh_college_faculty_registered.Where(r => r.collegeId == userCollegeID && r.existingFacultyId == null && r.createdBy != 63809).Select(r => r).ToList();
            //List<jntuh_college_faculty_registered> regFaculty = db.jntuh_college_faculty_registered.Where(r => r.collegeId == userCollegeID && r.existingFacultyId == null).Select(r => r).ToList();
            //var jntuh_registered_facultys = db.jntuh_registered_faculty.Select(f => f).ToList();
            //foreach (var item in regFaculty)
            //{
            //    CollegeFaculty collegeFaculty = new CollegeFaculty();
            //    jntuh_registered_faculty rFaculty = jntuh_registered_facultys.Where(f => f.RegistrationNumber == item.RegistrationNumber).Select(f => f).FirstOrDefault();

            //    //if (rFaculty.collegeId != null)
            //    //{
            //    //    collegeFaculty.collegeId = (int)rFaculty.collegeId;
            //    //}
            //    collegeFaculty.collegeId = userCollegeID;
            //    //collegeFaculty.facultyTypeId = rFaculty.facultyTypeId;
            //    collegeFaculty.facultyFirstName = rFaculty.FirstName;
            //    collegeFaculty.facultyLastName = rFaculty.MiddleName;
            //    collegeFaculty.facultySurname = rFaculty.LastName;
            //    collegeFaculty.facultyGenderId = rFaculty.GenderId;
            //    collegeFaculty.facultyFatherName = rFaculty.FatherOrHusbandName;
            //    //collegeFaculty.facultyCategoryId = rFaculty.facultyCategoryId;

            //    if (rFaculty.DateOfBirth != null)
            //        collegeFaculty.dateOfBirth = UAAAS.Models.Utilities.MMDDYY2DDMMYY(rFaculty.DateOfBirth.ToString());

            //    if (rFaculty.WorkingStatus == true)
            //    {
            //        collegeFaculty.facultyDesignationId = rFaculty.DesignationId!=null? (int)rFaculty.DesignationId:0;
            //        collegeFaculty.designation = db.jntuh_designation.Where(d => d.id == collegeFaculty.facultyDesignationId).Select(d => d.designation).FirstOrDefault();
            //        collegeFaculty.facultyOtherDesignation = rFaculty.OtherDesignation;
            //        collegeFaculty.facultyDepartmentId =rFaculty.DepartmentId !=null ? (int)rFaculty.DepartmentId:0;
            //        collegeFaculty.department = db.jntuh_department.Where(d => d.id == collegeFaculty.facultyDepartmentId).Select(d => d.departmentName).FirstOrDefault();
            //        collegeFaculty.facultyOtherDepartment = rFaculty.OtherDepartment;
            //    }

            //    collegeFaculty.facultyEmail = rFaculty.Email;
            //    collegeFaculty.facultyMobile = rFaculty.Mobile;
            //    collegeFaculty.facultyPANNumber = rFaculty.PANNumber;
            //    collegeFaculty.facultyAadhaarNumber = rFaculty.AadhaarNumber;
            //    collegeFaculty.FacultyRegistrationNumber = rFaculty.RegistrationNumber;
            //    teachingFaculty.Add(collegeFaculty);
            //}

            //foreach (var faculty in jntuh_college_faculty)
            //{
            //    CollegeFaculty collegeFaculty = new CollegeFaculty();
            //    collegeFaculty.id = faculty.id;

            //    jntuh_college_faculty_registered eFaculty = db.jntuh_college_faculty_registered.Where(r => r.existingFacultyId == faculty.id).Select(r => r).FirstOrDefault();

            //    if (eFaculty != null)
            //    {
            //        collegeFaculty.FacultyRegistrationNumber = eFaculty.RegistrationNumber;
            //    }

            //    if (string.IsNullOrEmpty(collegeFaculty.FacultyRegistrationNumber))
            //    {
            //        collegeFaculty.collegeId = faculty.collegeId;
            //        collegeFaculty.facultyTypeId = faculty.facultyTypeId;
            //        collegeFaculty.facultyFirstName = faculty.facultyFirstName;
            //        collegeFaculty.facultyLastName = faculty.facultyLastName;
            //        collegeFaculty.facultySurname = faculty.facultySurname;
            //        collegeFaculty.facultyGenderId = faculty.facultyGenderId;
            //        collegeFaculty.facultyFatherName = faculty.facultyFatherName;
            //        collegeFaculty.facultyCategoryId = faculty.facultyCategoryId;

            //        if (faculty.facultyDateOfBirth != null)
            //            collegeFaculty.dateOfBirth = UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.facultyDateOfBirth.ToString());

            //        collegeFaculty.facultyDesignationId = faculty.facultyDesignationId;
            //        collegeFaculty.designation = db.jntuh_designation.Where(d => d.id == faculty.facultyDesignationId).Select(d => d.designation).FirstOrDefault();
            //        collegeFaculty.facultyOtherDesignation = faculty.facultyOtherDesignation;
            //        collegeFaculty.facultyDepartmentId = faculty.facultyDepartmentId;
            //        collegeFaculty.department = db.jntuh_department.Where(d => d.id == faculty.facultyDepartmentId).Select(d => d.departmentName).FirstOrDefault();

            //        if (faculty.facultyDateOfAppointment != null)
            //            collegeFaculty.dateOfAppointment = UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.facultyDateOfAppointment.ToString());

            //        if (faculty.facultyDateOfResignation != null)
            //            collegeFaculty.dateOfResignation = UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.facultyDateOfResignation.ToString());

            //        collegeFaculty.isFacultyRatifiedByJNTU = faculty.isFacultyRatifiedByJNTU;

            //        if (faculty.facultyDateOfRatification != null)
            //            collegeFaculty.dateOfRatification = UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.facultyDateOfRatification.ToString());

            //        collegeFaculty.facultyDurationOfRatification = faculty.facultyDurationOfRatification;
            //        collegeFaculty.facultySalary = faculty.facultySalary;
            //        collegeFaculty.facultyPreviousExperience = Convert.ToInt32(faculty.facultyPreviousExperience);
            //        collegeFaculty.facultyPayScale = faculty.facultyPayScale;
            //        collegeFaculty.facultyEmail = faculty.facultyEmail;
            //        collegeFaculty.facultyMobile = faculty.facultyMobile;
            //        collegeFaculty.facultyPANNumber = faculty.facultyPANNumber;
            //        collegeFaculty.facultyAadhaarNumber = faculty.facultyAadhaarNumber;
            //        collegeFaculty.isRelatedToExamBranch = faculty.isRelatedToExamBranch;
            //        collegeFaculty.isRelatedToPlacementCell = faculty.isRelatedToPlacementCell;
            //        collegeFaculty.facultyAchievements1 = faculty.facultyAchievements1;
            //        collegeFaculty.facultyAchievements2 = faculty.facultyAchievements2;
            //        collegeFaculty.facultyAchievements3 = faculty.facultyAchievements3;
            //        collegeFaculty.FacultyRegistrationNumber = string.Empty;
            //    }
            //    else
            //    {
            //        jntuh_registered_faculty rFaculty = db.jntuh_registered_faculty.Where(f => f.RegistrationNumber == eFaculty.RegistrationNumber).Select(f => f).FirstOrDefault();

            //        if (rFaculty.collegeId != null)
            //        {
            //            collegeFaculty.collegeId = (int)rFaculty.collegeId;
            //        }

            //        //collegeFaculty.facultyTypeId = rFaculty.facultyTypeId;
            //        collegeFaculty.facultyFirstName = rFaculty.FirstName;
            //        collegeFaculty.facultyLastName = rFaculty.MiddleName;
            //        collegeFaculty.facultySurname = rFaculty.LastName;
            //        collegeFaculty.facultyGenderId = rFaculty.GenderId;
            //        collegeFaculty.facultyFatherName = rFaculty.FatherOrHusbandName;
            //        //collegeFaculty.facultyCategoryId = rFaculty.facultyCategoryId;

            //        if (rFaculty.DateOfBirth != null)
            //            collegeFaculty.dateOfBirth = UAAAS.Models.Utilities.MMDDYY2DDMMYY(rFaculty.DateOfBirth.ToString());

            //        if (rFaculty.DesignationId != null)
            //        {
            //            collegeFaculty.facultyDesignationId = (int)rFaculty.DesignationId;
            //            collegeFaculty.designation = db.jntuh_designation.Where(d => d.id == collegeFaculty.facultyDesignationId).Select(d => d.designation).FirstOrDefault();
            //        }

            //        collegeFaculty.facultyOtherDesignation = rFaculty.OtherDesignation;

            //        if (rFaculty.DepartmentId != null)
            //        {
            //            collegeFaculty.facultyDepartmentId = (int)rFaculty.DepartmentId;
            //            collegeFaculty.department = db.jntuh_department.Where(d => d.id == collegeFaculty.facultyDepartmentId).Select(d => d.departmentName).FirstOrDefault();
            //        }

            //        collegeFaculty.facultyOtherDepartment = rFaculty.OtherDepartment;
            //        collegeFaculty.facultyEmail = rFaculty.Email;
            //        collegeFaculty.facultyMobile = rFaculty.Mobile;
            //        collegeFaculty.facultyPANNumber = rFaculty.PANNumber;
            //        collegeFaculty.facultyAadhaarNumber = rFaculty.AadhaarNumber;
            //        collegeFaculty.FacultyRegistrationNumber = rFaculty.RegistrationNumber;
            //    }

            //    teachingFaculty.Add(collegeFaculty);
            //}

            //ViewBag.Type = UAAAS.Models.Utilities.EncryptString(type.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]);
            ////ViewBag.Type = 0;
            //ViewBag.Id = UAAAS.Models.Utilities.EncryptString("0", WebConfigurationManager.AppSettings["CryptoKey"]);
            ////ViewBag.Id = 0;
            //ViewBag.Count = teachingFaculty.Count();
            //return View(teachingFaculty);
            #endregion
            #region Some Colleges Getting Error So checking Purpose

            //    int type = db.jntuh_faculty_type.Where(t => t.facultyType == "Teaching").Select(t => t.id).FirstOrDefault();
            //    List<jntuh_college_faculty> jntuh_college_faculty = db.jntuh_college_faculty.Where(f => f.facultyTypeId == type && f.collegeId == userCollegeID)
            //        .OrderBy(f => f.facultyDepartmentId).ThenBy(f => f.facultyDesignationId).ThenBy(f => f.facultyFirstName).ToList();

            //    List<CollegeFaculty> teachingFaculty = new List<CollegeFaculty>();

            //    //Commented By Srinivas.T   FacultyVerificationStatus
            //    //List<jntuh_college_faculty_registered> regFaculty = db.jntuh_college_faculty_registered.Where(r => r.collegeId == userCollegeID && r.existingFacultyId == null && r.createdBy != 63809).Select(r => r).ToList();
            //    List<jntuh_college_faculty_registered> regFaculty = db.jntuh_college_faculty_registered.Where(r => r.collegeId == userCollegeID && r.existingFacultyId == null).Select(r => r).ToList();
            //    var RegistredFacultyLog = db.jntuh_registered_faculty_log.Where(F => F.FacultyApprovedStatus != 1).Select(F => new { F.RegistrationNumber, F.FacultyApprovedStatus, F.Remarks }).ToList();
            //    var jntuh_registered_facultys = db.jntuh_registered_faculty.AsNoTracking().Select(c => c).ToList();
            //    foreach (var item in regFaculty)
            //    {
            //        CollegeFaculty collegeFaculty = new CollegeFaculty();
            //        jntuh_registered_faculty rFaculty = jntuh_registered_facultys.Where(f => f.RegistrationNumber == item.RegistrationNumber.Trim()).Select(f => f).FirstOrDefault();

            //        //if (rFaculty.collegeId != null)
            //        //{
            //        //    collegeFaculty.collegeId = (int)rFaculty.collegeId;
            //        //}
            //        collegeFaculty.id = rFaculty.id;
            //        collegeFaculty.collegeId = userCollegeID;
            //        // collegeFaculty.facultyTypeId = rFaculty.facultyTypeId;
            //        //  regFaculty.Type = faculty.type;
            //        collegeFaculty.facultyType = rFaculty.type;
            //        collegeFaculty.facultyFirstName = rFaculty.FirstName;
            //        collegeFaculty.facultyLastName = rFaculty.MiddleName;
            //        collegeFaculty.facultySurname = rFaculty.LastName;
            //        collegeFaculty.facultyGenderId = rFaculty.GenderId;
            //        collegeFaculty.facultyFatherName = rFaculty.FatherOrHusbandName;
            //        //collegeFaculty.facultyCategoryId = rFaculty.facultyCategoryId;

            //        if (rFaculty.DateOfBirth != null)
            //            collegeFaculty.dateOfBirth = UAAAS.Models.Utilities.MMDDYY2DDMMYY(rFaculty.DateOfBirth.ToString());

            //        //if (rFaculty.WorkingStatus == true)
            //        //{

            //        // rFaculty.DesignationId!=null? (int)rFaculty.DesignationId:0;
            //        //collegeFaculty.facultyDesignationId = (int)rFaculty.DesignationId;
            //        collegeFaculty.facultyDesignationId = rFaculty.DesignationId != null ? (int)rFaculty.DesignationId : 0;
            //        collegeFaculty.designation = db.jntuh_designation.Where(d => d.id == collegeFaculty.facultyDesignationId).Select(d => d.designation).FirstOrDefault();
            //        collegeFaculty.facultyOtherDesignation = rFaculty.OtherDesignation;
            //        collegeFaculty.facultyDepartmentId = rFaculty.DepartmentId != null ? (int)rFaculty.DepartmentId : 0;
            //        collegeFaculty.department = db.jntuh_department.Where(d => d.id == collegeFaculty.facultyDepartmentId).Select(d => d.departmentName).FirstOrDefault();
            //        collegeFaculty.facultyOtherDepartment = rFaculty.OtherDepartment;
            //        //}

            //        collegeFaculty.facultyEmail = rFaculty.Email;
            //        collegeFaculty.facultyMobile = rFaculty.Mobile;
            //        collegeFaculty.facultyPANNumber = rFaculty.PANNumber;
            //        collegeFaculty.facultyAadhaarNumber = rFaculty.AadhaarNumber;
            //        collegeFaculty.FacultyRegistrationNumber = rFaculty.RegistrationNumber;
            //        collegeFaculty.facultyCount = RegistredFacultyLog.Count(F => F.RegistrationNumber == rFaculty.RegistrationNumber);

            //        collegeFaculty.FacultyVerificationStatus = RegistredFacultyLog.Where(F => F.RegistrationNumber == rFaculty.RegistrationNumber.Trim()).Select(F => F.FacultyApprovedStatus).FirstOrDefault();
            //        collegeFaculty.Remarks = RegistredFacultyLog.Where(F => F.RegistrationNumber == rFaculty.RegistrationNumber.Trim()).Select(F => F.Remarks).FirstOrDefault();
            //        string Reasons = "";
            //        //if (rFaculty.InvalidPANNumber == true)
            //        //{
            //        //    Reasons += "Invalid PANNumber" + ",";
            //        //}
            //        //if (rFaculty.DiscrepencyStatus == true)
            //        //{
            //        //    Reasons += "DiscrepencyStatus" + ",";
            //        //}
            //        //if (rFaculty.IncompleteCertificates == true)
            //        //{
            //        //    Reasons += "Incomplete Certificates" + ",";
            //        //}



            //        if (rFaculty.Absent == true)
            //        {
            //            Reasons = "ABSENT" + ",";
            //        }
            //        if (rFaculty.NotQualifiedAsperAICTE == true)
            //        {
            //            Reasons += "NOT QUALIFIED " + ",";
            //        }
            //        if (string.IsNullOrEmpty(rFaculty.PANNumber))
            //        {
            //            Reasons += "NO PAN" + ",";
            //        }
            //        if (rFaculty.DepartmentId == null)
            //        {
            //            Reasons += "NO DEPARTMENT" + ",";
            //        }
            //        if (rFaculty.NoSCM == true)
            //        {
            //            Reasons += "NO SCM/RATIFICATION" + ",";
            //        }
            //        if (rFaculty.PHDundertakingnotsubmitted == true)
            //        {
            //            Reasons += "NO UNDERTAKING" + ",";
            //        }
            //        if (rFaculty.Blacklistfaculy == true)
            //        {
            //            Reasons += "BLACKLISTED" + ",";
            //        }

            //        if (Reasons != "")
            //        {
            //            Reasons = Reasons.Substring(0, Reasons.Length - 1);
            //        }

            //        collegeFaculty.Reason = Reasons;


            //        teachingFaculty.Add(collegeFaculty);
            //    }
            //    var jntuh_college_faculty_registereds = db.jntuh_college_faculty_registered.Where(C => C.collegeId == userCollegeID).Select(C => C).ToList();
            //    jntuh_college_faculty_registered eFaculty = null;
            //    var jntuh_designations = db.jntuh_designation.Select(d => new { d.id, d.designation }).ToList();
            //    var jntuh_departments = db.jntuh_department.Select(d => new { d.id, d.departmentName }).ToList();
            //    foreach (var faculty in jntuh_college_faculty)
            //    {
            //        CollegeFaculty collegeFaculty = new CollegeFaculty();
            //        collegeFaculty.id = faculty.id;

            //        eFaculty = jntuh_college_faculty_registereds.Where(r => r.existingFacultyId == faculty.id).Select(r => r).FirstOrDefault();

            //        if (eFaculty != null)
            //        {
            //            collegeFaculty.FacultyRegistrationNumber = eFaculty.RegistrationNumber;
            //        }

            //        if (string.IsNullOrEmpty(collegeFaculty.FacultyRegistrationNumber))
            //        {
            //            if (eFaculty != null)
            //            {
            //                collegeFaculty.collegeId = faculty.collegeId;
            //                collegeFaculty.facultyTypeId = faculty.facultyTypeId;
            //                collegeFaculty.facultyFirstName = faculty.facultyFirstName;
            //                collegeFaculty.facultyLastName = faculty.facultyLastName;
            //                collegeFaculty.facultySurname = faculty.facultySurname;
            //                collegeFaculty.facultyGenderId = faculty.facultyGenderId;
            //                collegeFaculty.facultyFatherName = faculty.facultyFatherName;
            //                collegeFaculty.facultyCategoryId = faculty.facultyCategoryId;

            //                if (faculty.facultyDateOfBirth != null)
            //                    collegeFaculty.dateOfBirth = UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.facultyDateOfBirth.ToString());

            //                collegeFaculty.facultyDesignationId = faculty.facultyDesignationId;
            //                collegeFaculty.designation = jntuh_designations.Where(d => d.id == faculty.facultyDesignationId).Select(d => d.designation).FirstOrDefault();
            //                collegeFaculty.facultyOtherDesignation = faculty.facultyOtherDesignation;
            //                collegeFaculty.facultyDepartmentId = faculty.facultyDepartmentId;
            //                collegeFaculty.department = jntuh_departments.Where(d => d.id == faculty.facultyDepartmentId).Select(d => d.departmentName).FirstOrDefault();

            //                if (faculty.facultyDateOfAppointment != null)
            //                    collegeFaculty.dateOfAppointment = UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.facultyDateOfAppointment.ToString());

            //                if (faculty.facultyDateOfResignation != null)
            //                    collegeFaculty.dateOfResignation = UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.facultyDateOfResignation.ToString());

            //                collegeFaculty.isFacultyRatifiedByJNTU = faculty.isFacultyRatifiedByJNTU;

            //                if (faculty.facultyDateOfRatification != null)
            //                    collegeFaculty.dateOfRatification = UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.facultyDateOfRatification.ToString());

            //                collegeFaculty.facultyDurationOfRatification = faculty.facultyDurationOfRatification;
            //                collegeFaculty.facultySalary = faculty.facultySalary;
            //                collegeFaculty.facultyPreviousExperience = Convert.ToInt32(faculty.facultyPreviousExperience);
            //                collegeFaculty.facultyPayScale = faculty.facultyPayScale;
            //                collegeFaculty.facultyEmail = faculty.facultyEmail;
            //                collegeFaculty.facultyMobile = faculty.facultyMobile;
            //                collegeFaculty.facultyPANNumber = faculty.facultyPANNumber;
            //                collegeFaculty.facultyAadhaarNumber = faculty.facultyAadhaarNumber;
            //                collegeFaculty.isRelatedToExamBranch = faculty.isRelatedToExamBranch;
            //                collegeFaculty.isRelatedToPlacementCell = faculty.isRelatedToPlacementCell;
            //                collegeFaculty.facultyAchievements1 = faculty.facultyAchievements1;
            //                collegeFaculty.facultyAchievements2 = faculty.facultyAchievements2;
            //                collegeFaculty.facultyAchievements3 = faculty.facultyAchievements3;
            //                collegeFaculty.FacultyRegistrationNumber = string.Empty;

            //                collegeFaculty.facultyCount = RegistredFacultyLog.Count(F => F.RegistrationNumber == eFaculty.RegistrationNumber.Trim());
            //                collegeFaculty.Remarks = RegistredFacultyLog.Where(F => F.RegistrationNumber == eFaculty.RegistrationNumber.Trim()).Select(F => F.Remarks).FirstOrDefault();
            //                collegeFaculty.FacultyVerificationStatus = RegistredFacultyLog.Where(F => F.RegistrationNumber == eFaculty.RegistrationNumber.Trim()).Select(F => F.FacultyApprovedStatus).FirstOrDefault();

            //            }

            //        }
            //        else
            //        {
            //            if (eFaculty != null)
            //            {
            //                jntuh_registered_faculty rFaculty = jntuh_registered_facultys.Where(f => f.RegistrationNumber == eFaculty.RegistrationNumber.Trim()).Select(f => f).FirstOrDefault();

            //                if (rFaculty.collegeId != null)
            //                {
            //                    collegeFaculty.collegeId = (int)rFaculty.collegeId;
            //                }
            //                collegeFaculty.id = rFaculty.id;
            //                collegeFaculty.facultyType = rFaculty.type;
            //                //collegeFaculty.facultyTypeId = rFaculty.facultyTypeId;
            //                collegeFaculty.facultyFirstName = rFaculty.FirstName;
            //                collegeFaculty.facultyLastName = rFaculty.MiddleName;
            //                collegeFaculty.facultySurname = rFaculty.LastName;
            //                collegeFaculty.facultyGenderId = rFaculty.GenderId;
            //                collegeFaculty.facultyFatherName = rFaculty.FatherOrHusbandName;
            //                //collegeFaculty.facultyCategoryId = rFaculty.facultyCategoryId;

            //                if (rFaculty.DateOfBirth != null)
            //                    collegeFaculty.dateOfBirth = UAAAS.Models.Utilities.MMDDYY2DDMMYY(rFaculty.DateOfBirth.ToString());

            //                if (rFaculty.DesignationId != null)
            //                {
            //                    collegeFaculty.facultyDesignationId = (int)rFaculty.DesignationId;
            //                    collegeFaculty.designation = jntuh_designations.Where(d => d.id == collegeFaculty.facultyDesignationId).Select(d => d.designation).FirstOrDefault();
            //                }

            //                collegeFaculty.facultyOtherDesignation = rFaculty.OtherDesignation;

            //                if (rFaculty.DepartmentId != null)
            //                {
            //                    collegeFaculty.facultyDepartmentId = (int)rFaculty.DepartmentId;
            //                    collegeFaculty.department = jntuh_departments.Where(d => d.id == collegeFaculty.facultyDepartmentId).Select(d => d.departmentName).FirstOrDefault();
            //                }

            //                collegeFaculty.facultyOtherDepartment = rFaculty.OtherDepartment;
            //                collegeFaculty.facultyEmail = rFaculty.Email;
            //                collegeFaculty.facultyMobile = rFaculty.Mobile;
            //                collegeFaculty.facultyPANNumber = rFaculty.PANNumber;
            //                collegeFaculty.facultyAadhaarNumber = rFaculty.AadhaarNumber;
            //                collegeFaculty.FacultyRegistrationNumber = rFaculty.RegistrationNumber;
            //                collegeFaculty.facultyCount = RegistredFacultyLog.Count(F => F.RegistrationNumber == eFaculty.RegistrationNumber.Trim());
            //                collegeFaculty.Remarks = RegistredFacultyLog.Where(F => F.RegistrationNumber == eFaculty.RegistrationNumber.Trim()).Select(F => F.Remarks).FirstOrDefault();
            //                collegeFaculty.FacultyVerificationStatus = RegistredFacultyLog.Where(F => F.RegistrationNumber == eFaculty.RegistrationNumber.Trim()).Select(F => F.FacultyApprovedStatus).FirstOrDefault();

            //                string Reasons = "";
            //                if (rFaculty.InvalidPANNumber == true)
            //                {
            //                    Reasons += "Invalid PANNumber" + ",";
            //                }
            //                if (rFaculty.DiscrepencyStatus == true)
            //                {
            //                    Reasons += "Discrepency Status" + ",";
            //                }
            //                if (rFaculty.IncompleteCertificates == true)
            //                {
            //                    Reasons += "Incomplete Certificates" + ",";
            //                }
            //                if (Reasons != "")
            //                {
            //                    Reasons = Reasons.Substring(0, Reasons.Length - 1);
            //                }

            //                collegeFaculty.Reason = Reasons;

            //            }

            //            teachingFaculty.Add(collegeFaculty);
            //        }

            //    }

            //    ViewBag.Type = UAAAS.Models.Utilities.EncryptString(type.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]);
            //    //ViewBag.Type = 0;
            //    ViewBag.Id = UAAAS.Models.Utilities.EncryptString("0", WebConfigurationManager.AppSettings["CryptoKey"]);
            //    //ViewBag.Id = 0;
            //    ViewBag.Count = teachingFaculty.Count();
            //    return View(teachingFaculty);
            #endregion

            // ViewBag.Type = UAAAS.Models.Utilities.EncryptString(type.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]);
            //ViewBag.Type = 0;
            //  ViewBag.Id = UAAAS.Models.Utilities.EncryptString("0", WebConfigurationManager.AppSettings["CryptoKey"]);
            //ViewBag.Id = 0;
            //ViewBag.Count = newteachingFaculty.Count();
            //return View(newteachingFaculty);
            return View();

        }

        [Authorize(Roles = "Admin,College")]
        public ActionResult ViewAdhocTeaching(string id, string strType)
        {
            return RedirectToAction("College", "Dashboard");
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userID).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                userCollegeID = userCollegeID = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(id, WebConfigurationManager.AppSettings["CryptoKey"]));
            }
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            DateTime todayDate = DateTime.Now.Date;
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy =
                db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();

            var status = GetPageEditableStatus(userCollegeID);

            if (status > 0 && Roles.IsUserInRole("College"))
            {

                bool isPageEditable = db.jntuh_pa_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("PFA") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

                if (isPageEditable)
                {
                    return RedirectToAction("Teaching");
                }
            }
            return View();
        }


        [Authorize(Roles = "Admin,College")]
        public ActionResult PrintTeaching(string id)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userID).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                userCollegeID = userCollegeID = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(id, WebConfigurationManager.AppSettings["CryptoKey"]));
            }
            DateTime todayDate = DateTime.Now.Date;
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();

            var status = GetPageEditableStatus(userCollegeID);

            if (status > 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("Teaching");
            }
            int type = db.jntuh_faculty_type.Where(t => t.facultyType == "Teaching").Select(t => t.id).FirstOrDefault();
            List<jntuh_college_faculty> jntuh_college_faculty = db.jntuh_college_faculty.Where(f => f.facultyTypeId == type && f.collegeId == userCollegeID).ToList();

            List<CollegeFaculty> teachingFaculty = new List<CollegeFaculty>();

            foreach (var faculty in jntuh_college_faculty)
            {
                CollegeFaculty collegeFaculty = new CollegeFaculty();
                collegeFaculty.id = faculty.id;
                collegeFaculty.collegeId = faculty.collegeId;
                collegeFaculty.facultyTypeId = faculty.facultyTypeId;
                collegeFaculty.facultyFirstName = faculty.facultyFirstName;
                collegeFaculty.facultyLastName = faculty.facultyLastName;
                collegeFaculty.facultySurname = faculty.facultySurname;
                collegeFaculty.facultyGenderId = faculty.facultyGenderId;
                collegeFaculty.facultyFatherName = faculty.facultyFatherName;
                collegeFaculty.facultyCategoryId = faculty.facultyCategoryId;

                if (faculty.facultyDateOfBirth != null)
                    collegeFaculty.dateOfBirth = UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.facultyDateOfBirth.ToString());

                collegeFaculty.facultyDesignationId = faculty.facultyDesignationId;
                collegeFaculty.designation = db.jntuh_designation.Where(d => d.id == faculty.facultyDesignationId).Select(d => d.designation).FirstOrDefault();
                collegeFaculty.facultyOtherDesignation = faculty.facultyOtherDesignation;
                collegeFaculty.facultyDepartmentId = faculty.facultyDepartmentId;
                collegeFaculty.department = db.jntuh_department.Where(d => d.id == faculty.facultyDepartmentId).Select(d => d.departmentName).FirstOrDefault();

                if (faculty.facultyDateOfAppointment != null)
                    collegeFaculty.dateOfAppointment = UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.facultyDateOfAppointment.ToString());

                if (faculty.facultyDateOfResignation != null)
                    collegeFaculty.dateOfResignation = UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.facultyDateOfResignation.ToString());

                collegeFaculty.isFacultyRatifiedByJNTU = faculty.isFacultyRatifiedByJNTU;

                if (faculty.facultyDateOfRatification != null)
                    collegeFaculty.dateOfRatification = UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.facultyDateOfRatification.ToString());

                collegeFaculty.facultyDurationOfRatification = faculty.facultyDurationOfRatification;
                collegeFaculty.facultySalary = faculty.facultySalary;
                collegeFaculty.facultyPreviousExperience = Convert.ToInt32(faculty.facultyPreviousExperience);
                collegeFaculty.facultyPayScale = faculty.facultyPayScale;
                collegeFaculty.facultyEmail = faculty.facultyEmail;
                collegeFaculty.facultyMobile = faculty.facultyMobile;
                collegeFaculty.facultyPANNumber = faculty.facultyPANNumber;
                collegeFaculty.facultyAadhaarNumber = faculty.facultyAadhaarNumber;
                collegeFaculty.isRelatedToExamBranch = faculty.isRelatedToExamBranch;
                collegeFaculty.isRelatedToPlacementCell = faculty.isRelatedToPlacementCell;
                collegeFaculty.facultyAchievements1 = faculty.facultyAchievements1;
                collegeFaculty.facultyAchievements2 = faculty.facultyAchievements2;
                collegeFaculty.facultyAchievements3 = faculty.facultyAchievements3;
                collegeFaculty.photo = faculty.facultyPhoto;
                teachingFaculty.Add(collegeFaculty);
            }

            ViewBag.Type = UAAAS.Models.Utilities.EncryptString(type.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]);
            //ViewBag.Type = type;
            //ViewBag.Id = UAAAS.Models.Utilities.Encrypt("0", WebConfigurationManager.AppSettings["CryptoKey"]);
            ViewBag.Id = 0;
            ViewBag.Count = teachingFaculty.Count();
            return View("PrintTeaching", teachingFaculty);
        }


        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult NonTeaching(string collegeId)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userID).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();
            if (userCollegeID == 0 && collegeId != null)
            {
                if (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry"))
                {
                    userCollegeID = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(collegeId, WebConfigurationManager.AppSettings["CryptoKey"]));
                }
            }
            ViewBag.collegeId = UAAAS.Models.Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString();
            DateTime todayDate = DateTime.Now.Date;
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();

            var status = GetPageEditableStatus(userCollegeID);

            if (status == 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("ViewNonTeaching");
            }
            bool isPageEditable = db.jntuh_pa_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("PFA") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

            if (!isPageEditable)
            {
                return RedirectToAction("ViewNonTeaching");
            }
            int type = db.jntuh_faculty_type.Where(t => t.facultyType == "Non-Teaching").Select(t => t.id).FirstOrDefault();
            List<jntuh_college_faculty> jntuh_college_faculty = db.jntuh_college_faculty.Where(f => f.facultyTypeId == type && f.collegeId == userCollegeID).ToList();

            List<CollegeFaculty> teachingFaculty = new List<CollegeFaculty>();

            foreach (var faculty in jntuh_college_faculty)
            {
                CollegeFaculty collegeFaculty = new CollegeFaculty();
                collegeFaculty.id = faculty.id;
                collegeFaculty.collegeId = faculty.collegeId;
                collegeFaculty.facultyTypeId = faculty.facultyTypeId;
                collegeFaculty.facultyFirstName = faculty.facultyFirstName;
                collegeFaculty.facultyLastName = faculty.facultyLastName;
                collegeFaculty.facultySurname = faculty.facultySurname;
                collegeFaculty.facultyGenderId = faculty.facultyGenderId;
                collegeFaculty.facultyFatherName = faculty.facultyFatherName;
                collegeFaculty.facultyCategoryId = faculty.facultyCategoryId;

                if (faculty.facultyDateOfBirth != null)
                    collegeFaculty.dateOfBirth = UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.facultyDateOfBirth.ToString());

                collegeFaculty.facultyDesignationId = faculty.facultyDesignationId;
                collegeFaculty.designation = db.jntuh_designation.Where(d => d.id == faculty.facultyDesignationId).Select(d => d.designation).FirstOrDefault();
                collegeFaculty.facultyOtherDesignation = faculty.facultyOtherDesignation;
                collegeFaculty.facultyDepartmentId = faculty.facultyDepartmentId;
                collegeFaculty.department = db.jntuh_department.Where(d => d.id == faculty.facultyDepartmentId).Select(d => d.departmentName).FirstOrDefault();

                if (faculty.facultyDateOfAppointment != null)
                    collegeFaculty.dateOfAppointment = UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.facultyDateOfAppointment.ToString());

                if (faculty.facultyDateOfResignation != null)
                    collegeFaculty.dateOfResignation = UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.facultyDateOfResignation.ToString());

                collegeFaculty.isFacultyRatifiedByJNTU = faculty.isFacultyRatifiedByJNTU;

                if (faculty.facultyDateOfRatification != null)
                    collegeFaculty.dateOfRatification = UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.facultyDateOfRatification.ToString());

                collegeFaculty.facultyDurationOfRatification = faculty.facultyDurationOfRatification;
                collegeFaculty.facultySalary = faculty.facultySalary;
                collegeFaculty.facultyPreviousExperience = Convert.ToInt32(faculty.facultyPreviousExperience);
                collegeFaculty.facultyPayScale = faculty.facultyPayScale;
                collegeFaculty.facultyEmail = faculty.facultyEmail;
                collegeFaculty.facultyMobile = faculty.facultyMobile;
                collegeFaculty.facultyPANNumber = faculty.facultyPANNumber;
                collegeFaculty.facultyAadhaarNumber = faculty.facultyAadhaarNumber;
                collegeFaculty.isRelatedToExamBranch = faculty.isRelatedToExamBranch;
                collegeFaculty.isRelatedToPlacementCell = faculty.isRelatedToPlacementCell;
                collegeFaculty.facultyAchievements1 = faculty.facultyAchievements1;
                collegeFaculty.facultyAchievements2 = faculty.facultyAchievements2;
                collegeFaculty.facultyAchievements3 = faculty.facultyAchievements3;
                collegeFaculty.photo = faculty.facultyPhoto;

                teachingFaculty.Add(collegeFaculty);
            }

            ViewBag.Type = UAAAS.Models.Utilities.EncryptString(type.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]);
            // ViewBag.Type = type;
            ViewBag.Id = UAAAS.Models.Utilities.EncryptString("0", WebConfigurationManager.AppSettings["CryptoKey"]);
            //ViewBag.Id = 0;
            ViewBag.Count = teachingFaculty.Count();
            return View(teachingFaculty);
        }

        [Authorize(Roles = "Admin,College")]
        public ActionResult ViewNonTeaching(string id, string strType)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userID).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                userCollegeID = userCollegeID = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(id, WebConfigurationManager.AppSettings["CryptoKey"]));
            }

            DateTime todayDate = DateTime.Now.Date;
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();

            var status = GetPageEditableStatus(userCollegeID);

            if (status > 0 && Roles.IsUserInRole("College"))
            {
                bool isPageEditable = db.jntuh_pa_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("PFA") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

                if (isPageEditable)
                {
                    return RedirectToAction("NonTeaching");
                }

            }
            int type = db.jntuh_faculty_type.Where(t => t.facultyType == "Non-Teaching").Select(t => t.id).FirstOrDefault();
            List<jntuh_college_faculty> jntuh_college_faculty = db.jntuh_college_faculty.Where(f => f.facultyTypeId == type && f.collegeId == userCollegeID).ToList();

            List<CollegeFaculty> teachingFaculty = new List<CollegeFaculty>();

            foreach (var faculty in jntuh_college_faculty)
            {
                CollegeFaculty collegeFaculty = new CollegeFaculty();
                collegeFaculty.id = faculty.id;
                collegeFaculty.collegeId = faculty.collegeId;
                collegeFaculty.facultyTypeId = faculty.facultyTypeId;
                collegeFaculty.facultyFirstName = faculty.facultyFirstName;
                collegeFaculty.facultyLastName = faculty.facultyLastName;
                collegeFaculty.facultySurname = faculty.facultySurname;
                collegeFaculty.facultyGenderId = faculty.facultyGenderId;
                collegeFaculty.facultyFatherName = faculty.facultyFatherName;
                collegeFaculty.facultyCategoryId = faculty.facultyCategoryId;

                if (faculty.facultyDateOfBirth != null)
                    collegeFaculty.dateOfBirth = UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.facultyDateOfBirth.ToString());

                collegeFaculty.facultyDesignationId = faculty.facultyDesignationId;
                collegeFaculty.designation = db.jntuh_designation.Where(d => d.id == faculty.facultyDesignationId).Select(d => d.designation).FirstOrDefault();
                collegeFaculty.facultyOtherDesignation = faculty.facultyOtherDesignation;
                collegeFaculty.facultyDepartmentId = faculty.facultyDepartmentId;
                collegeFaculty.department = db.jntuh_department.Where(d => d.id == faculty.facultyDepartmentId).Select(d => d.departmentName).FirstOrDefault();

                if (faculty.facultyDateOfAppointment != null)
                    collegeFaculty.dateOfAppointment = UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.facultyDateOfAppointment.ToString());

                if (faculty.facultyDateOfResignation != null)
                    collegeFaculty.dateOfResignation = UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.facultyDateOfResignation.ToString());

                collegeFaculty.isFacultyRatifiedByJNTU = faculty.isFacultyRatifiedByJNTU;

                if (faculty.facultyDateOfRatification != null)
                    collegeFaculty.dateOfRatification = UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.facultyDateOfRatification.ToString());

                collegeFaculty.facultyDurationOfRatification = faculty.facultyDurationOfRatification;
                collegeFaculty.facultySalary = faculty.facultySalary;
                collegeFaculty.facultyPreviousExperience = Convert.ToInt32(faculty.facultyPreviousExperience);
                collegeFaculty.facultyPayScale = faculty.facultyPayScale;
                collegeFaculty.facultyEmail = faculty.facultyEmail;
                collegeFaculty.facultyMobile = faculty.facultyMobile;
                collegeFaculty.facultyPANNumber = faculty.facultyPANNumber;
                collegeFaculty.facultyAadhaarNumber = faculty.facultyAadhaarNumber;
                collegeFaculty.isRelatedToExamBranch = faculty.isRelatedToExamBranch;
                collegeFaculty.isRelatedToPlacementCell = faculty.isRelatedToPlacementCell;
                collegeFaculty.facultyAchievements1 = faculty.facultyAchievements1;
                collegeFaculty.facultyAchievements2 = faculty.facultyAchievements2;
                collegeFaculty.facultyAchievements3 = faculty.facultyAchievements3;
                collegeFaculty.photo = faculty.facultyPhoto;

                //Getting faculty Education
                collegeFaculty.FacultyEducation = (from fc in db.jntuh_education_category
                                                   join fe in db.jntuh_faculty_education on fc.id equals fe.educationId into fec
                                                   from ec in fec.Where(fe => fe.facultyId == faculty.id).DefaultIfEmpty()
                                                   select new FacultyEducation
                                                   {
                                                       studiedEducation = ec.courseStudied
                                                   }).ToList();

                teachingFaculty.Add(collegeFaculty);
            }
            //Export NonTeaching Faculty Details-16-09-2014
            if (strType == "Export" && teachingFaculty.Count() > 0)
            {
                Response.ClearContent();
                Response.Buffer = true;
                Response.AddHeader("content-disposition", "attachment; filename=NonTeaching Faculty Details.xls");
                Response.ContentType = "application/vnd.ms-excel";
                return PartialView("~/Views/Reports/_TeachingFacultyReport.cshtml", teachingFaculty);
            }
            //

            ViewBag.Type = UAAAS.Models.Utilities.EncryptString(type.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]);
            // ViewBag.Type = type;
            //ViewBag.Id = UAAAS.Models.Utilities.Encrypt("0", WebConfigurationManager.AppSettings["CryptoKey"]);
            ViewBag.Id = 0;
            ViewBag.Count = teachingFaculty.Count();
            if (Roles.IsUserInRole("Admin") == true)
            {
                ViewBag.Admin = true;
            }
            return View("ViewNonTeaching", teachingFaculty);
        }

        [Authorize(Roles = "Admin,College")]
        public ActionResult PrintNonTeaching(string id)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userID).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();

            if (userCollegeID == 0)
            {
                userCollegeID = userCollegeID = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(id, WebConfigurationManager.AppSettings["CryptoKey"]));
            }

            DateTime todayDate = DateTime.Now.Date;
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();

            var status = GetPageEditableStatus(userCollegeID);

            if (status > 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("NonTeaching");
            }
            int type = db.jntuh_faculty_type.Where(t => t.facultyType == "Non-Teaching").Select(t => t.id).FirstOrDefault();
            List<jntuh_college_faculty> jntuh_college_faculty = db.jntuh_college_faculty.Where(f => f.facultyTypeId == type && f.collegeId == userCollegeID).ToList();

            List<CollegeFaculty> teachingFaculty = new List<CollegeFaculty>();

            foreach (var faculty in jntuh_college_faculty)
            {
                CollegeFaculty collegeFaculty = new CollegeFaculty();
                collegeFaculty.id = faculty.id;
                collegeFaculty.collegeId = faculty.collegeId;
                collegeFaculty.facultyTypeId = faculty.facultyTypeId;
                collegeFaculty.facultyFirstName = faculty.facultyFirstName;
                collegeFaculty.facultyLastName = faculty.facultyLastName;
                collegeFaculty.facultySurname = faculty.facultySurname;
                collegeFaculty.facultyGenderId = faculty.facultyGenderId;
                collegeFaculty.facultyFatherName = faculty.facultyFatherName;
                collegeFaculty.facultyCategoryId = faculty.facultyCategoryId;

                if (faculty.facultyDateOfBirth != null)
                    collegeFaculty.dateOfBirth = UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.facultyDateOfBirth.ToString());

                collegeFaculty.facultyDesignationId = faculty.facultyDesignationId;
                collegeFaculty.designation = db.jntuh_designation.Where(d => d.id == faculty.facultyDesignationId).Select(d => d.designation).FirstOrDefault();
                collegeFaculty.facultyOtherDesignation = faculty.facultyOtherDesignation;
                collegeFaculty.facultyDepartmentId = faculty.facultyDepartmentId;
                collegeFaculty.department = db.jntuh_department.Where(d => d.id == faculty.facultyDepartmentId).Select(d => d.departmentName).FirstOrDefault();

                if (faculty.facultyDateOfAppointment != null)
                    collegeFaculty.dateOfAppointment = UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.facultyDateOfAppointment.ToString());

                if (faculty.facultyDateOfResignation != null)
                    collegeFaculty.dateOfResignation = UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.facultyDateOfResignation.ToString());

                collegeFaculty.isFacultyRatifiedByJNTU = faculty.isFacultyRatifiedByJNTU;

                if (faculty.facultyDateOfRatification != null)
                    collegeFaculty.dateOfRatification = UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.facultyDateOfRatification.ToString());

                collegeFaculty.facultyDurationOfRatification = faculty.facultyDurationOfRatification;
                collegeFaculty.facultySalary = faculty.facultySalary;
                collegeFaculty.facultyPreviousExperience = Convert.ToInt32(faculty.facultyPreviousExperience);
                collegeFaculty.facultyPayScale = faculty.facultyPayScale;
                collegeFaculty.facultyEmail = faculty.facultyEmail;
                collegeFaculty.facultyMobile = faculty.facultyMobile;
                collegeFaculty.facultyPANNumber = faculty.facultyPANNumber;
                collegeFaculty.facultyAadhaarNumber = faculty.facultyAadhaarNumber;
                collegeFaculty.isRelatedToExamBranch = faculty.isRelatedToExamBranch;
                collegeFaculty.isRelatedToPlacementCell = faculty.isRelatedToPlacementCell;
                collegeFaculty.facultyAchievements1 = faculty.facultyAchievements1;
                collegeFaculty.facultyAchievements2 = faculty.facultyAchievements2;
                collegeFaculty.facultyAchievements3 = faculty.facultyAchievements3;
                collegeFaculty.photo = faculty.facultyPhoto;

                teachingFaculty.Add(collegeFaculty);
            }

            ViewBag.Type = UAAAS.Models.Utilities.EncryptString(type.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]);
            // ViewBag.Type = type;
            //ViewBag.Id = UAAAS.Models.Utilities.Encrypt("0", WebConfigurationManager.AppSettings["CryptoKey"]);
            ViewBag.Id = 0;
            ViewBag.Count = teachingFaculty.Count();
            return View("PrintNonTeaching", teachingFaculty);
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Technical(string collegeId)
        {
            //return RedirectToAction("College", "Dashboard");
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userID).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();
            if (userCollegeID == 0 && collegeId != null)
            {
                if (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry"))
                {
                    userCollegeID = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(collegeId, WebConfigurationManager.AppSettings["CryptoKey"]));
                }
            }
            ViewBag.collegeId = UAAAS.Models.Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString();
            DateTime todayDate = DateTime.Now.Date;
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();

            var status = GetPageEditableStatus(userCollegeID);

            if (status == 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("ViewTechnical");
            }
            bool isPageEditable = db.jntuh_pa_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("PFA") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

            if (!isPageEditable)
            {
                return RedirectToAction("ViewTechnical");
            }

            int type = db.jntuh_faculty_type.Where(t => t.facultyType == "Technical").Select(t => t.id).FirstOrDefault();
            List<jntuh_college_faculty> jntuh_college_faculty = db.jntuh_college_faculty.Where(f => f.facultyTypeId == type && f.collegeId == userCollegeID).ToList();

            List<CollegeFaculty> teachingFaculty = new List<CollegeFaculty>();

            foreach (var faculty in jntuh_college_faculty)
            {
                CollegeFaculty collegeFaculty = new CollegeFaculty();
                collegeFaculty.id = faculty.id;
                collegeFaculty.collegeId = faculty.collegeId;
                collegeFaculty.facultyTypeId = faculty.facultyTypeId;
                collegeFaculty.facultyFirstName = faculty.facultyFirstName;
                collegeFaculty.facultyLastName = faculty.facultyLastName;
                collegeFaculty.facultySurname = faculty.facultySurname;
                collegeFaculty.facultyGenderId = faculty.facultyGenderId;
                collegeFaculty.facultyFatherName = faculty.facultyFatherName;
                collegeFaculty.facultyCategoryId = faculty.facultyCategoryId;

                if (faculty.facultyDateOfBirth != null)
                    collegeFaculty.dateOfBirth = UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.facultyDateOfBirth.ToString());

                collegeFaculty.facultyDesignationId = faculty.facultyDesignationId;
                collegeFaculty.designation = db.jntuh_designation.Where(d => d.id == faculty.facultyDesignationId).Select(d => d.designation).FirstOrDefault();
                collegeFaculty.facultyOtherDesignation = faculty.facultyOtherDesignation;
                collegeFaculty.facultyDepartmentId = faculty.facultyDepartmentId;
                collegeFaculty.department = db.jntuh_department.Where(d => d.id == faculty.facultyDepartmentId).Select(d => d.departmentName).FirstOrDefault();

                if (faculty.facultyDateOfAppointment != null)
                    collegeFaculty.dateOfAppointment = UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.facultyDateOfAppointment.ToString());

                if (faculty.facultyDateOfResignation != null)
                    collegeFaculty.dateOfResignation = UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.facultyDateOfResignation.ToString());

                collegeFaculty.isFacultyRatifiedByJNTU = faculty.isFacultyRatifiedByJNTU;

                if (faculty.facultyDateOfRatification != null)
                    collegeFaculty.dateOfRatification = UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.facultyDateOfRatification.ToString());

                collegeFaculty.facultyDurationOfRatification = faculty.facultyDurationOfRatification;
                collegeFaculty.facultySalary = faculty.facultySalary;
                collegeFaculty.facultyPreviousExperience = Convert.ToInt32(faculty.facultyPreviousExperience);
                collegeFaculty.facultyPayScale = faculty.facultyPayScale;
                collegeFaculty.facultyEmail = faculty.facultyEmail;
                collegeFaculty.facultyMobile = faculty.facultyMobile;
                collegeFaculty.facultyPANNumber = faculty.facultyPANNumber;
                collegeFaculty.facultyAadhaarNumber = faculty.facultyAadhaarNumber;
                collegeFaculty.isRelatedToExamBranch = faculty.isRelatedToExamBranch;
                collegeFaculty.isRelatedToPlacementCell = faculty.isRelatedToPlacementCell;
                collegeFaculty.facultyAchievements1 = faculty.facultyAchievements1;
                collegeFaculty.facultyAchievements2 = faculty.facultyAchievements2;
                collegeFaculty.facultyAchievements3 = faculty.facultyAchievements3;
                collegeFaculty.photo = faculty.facultyPhoto;

                teachingFaculty.Add(collegeFaculty);

            }

            ViewBag.Type = UAAAS.Models.Utilities.EncryptString(type.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]);
            //ViewBag.Type = type;
            ViewBag.Id = UAAAS.Models.Utilities.EncryptString("0", WebConfigurationManager.AppSettings["CryptoKey"]);
            //ViewBag.Id = 0;
            ViewBag.Count = teachingFaculty.Count();
            return View(teachingFaculty);
        }

        [Authorize(Roles = "Admin,College")]
        public ActionResult ViewTechnical(string id, string strType)
        {
            //return RedirectToAction("College", "Dashboard");
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userID).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                userCollegeID = userCollegeID = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(id, WebConfigurationManager.AppSettings["CryptoKey"]));
            }
            DateTime todayDate = DateTime.Now.Date;
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();

            var status = GetPageEditableStatus(userCollegeID);

            if (status > 0 && Roles.IsUserInRole("College"))
            {
                bool isPageEditable = db.jntuh_pa_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("PFA") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

                if (isPageEditable)
                {
                    return RedirectToAction("Technical");
                }
            }
            int type = db.jntuh_faculty_type.Where(t => t.facultyType == "Technical").Select(t => t.id).FirstOrDefault();
            List<jntuh_college_faculty> jntuh_college_faculty = db.jntuh_college_faculty.Where(f => f.facultyTypeId == type && f.collegeId == userCollegeID).ToList();

            List<CollegeFaculty> teachingFaculty = new List<CollegeFaculty>();

            foreach (var faculty in jntuh_college_faculty)
            {
                CollegeFaculty collegeFaculty = new CollegeFaculty();
                collegeFaculty.id = faculty.id;
                collegeFaculty.collegeId = faculty.collegeId;
                collegeFaculty.facultyTypeId = faculty.facultyTypeId;
                collegeFaculty.facultyFirstName = faculty.facultyFirstName;
                collegeFaculty.facultyLastName = faculty.facultyLastName;
                collegeFaculty.facultySurname = faculty.facultySurname;
                collegeFaculty.facultyGenderId = faculty.facultyGenderId;
                collegeFaculty.facultyFatherName = faculty.facultyFatherName;
                collegeFaculty.facultyCategoryId = faculty.facultyCategoryId;

                if (faculty.facultyDateOfBirth != null)
                    collegeFaculty.dateOfBirth = UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.facultyDateOfBirth.ToString());

                collegeFaculty.facultyDesignationId = faculty.facultyDesignationId;
                collegeFaculty.designation = db.jntuh_designation.Where(d => d.id == faculty.facultyDesignationId).Select(d => d.designation).FirstOrDefault();
                collegeFaculty.facultyOtherDesignation = faculty.facultyOtherDesignation;
                collegeFaculty.facultyDepartmentId = faculty.facultyDepartmentId;
                collegeFaculty.department = db.jntuh_department.Where(d => d.id == faculty.facultyDepartmentId).Select(d => d.departmentName).FirstOrDefault();

                if (faculty.facultyDateOfAppointment != null)
                    collegeFaculty.dateOfAppointment = UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.facultyDateOfAppointment.ToString());

                if (faculty.facultyDateOfResignation != null)
                    collegeFaculty.dateOfResignation = UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.facultyDateOfResignation.ToString());

                collegeFaculty.isFacultyRatifiedByJNTU = faculty.isFacultyRatifiedByJNTU;

                if (faculty.facultyDateOfRatification != null)
                    collegeFaculty.dateOfRatification = UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.facultyDateOfRatification.ToString());

                collegeFaculty.facultyDurationOfRatification = faculty.facultyDurationOfRatification;
                collegeFaculty.facultySalary = faculty.facultySalary;
                collegeFaculty.facultyPreviousExperience = Convert.ToInt32(faculty.facultyPreviousExperience);
                collegeFaculty.facultyPayScale = faculty.facultyPayScale;
                collegeFaculty.facultyEmail = faculty.facultyEmail;
                collegeFaculty.facultyMobile = faculty.facultyMobile;
                collegeFaculty.facultyPANNumber = faculty.facultyPANNumber;
                collegeFaculty.facultyAadhaarNumber = faculty.facultyAadhaarNumber;
                collegeFaculty.isRelatedToExamBranch = faculty.isRelatedToExamBranch;
                collegeFaculty.isRelatedToPlacementCell = faculty.isRelatedToPlacementCell;
                collegeFaculty.facultyAchievements1 = faculty.facultyAchievements1;
                collegeFaculty.facultyAchievements2 = faculty.facultyAchievements2;
                collegeFaculty.facultyAchievements3 = faculty.facultyAchievements3;
                collegeFaculty.photo = faculty.facultyPhoto;
                //Getting faculty Education
                collegeFaculty.FacultyEducation = (from fc in db.jntuh_education_category
                                                   join fe in db.jntuh_faculty_education on fc.id equals fe.educationId into fec
                                                   from ec in fec.Where(fe => fe.facultyId == faculty.id).DefaultIfEmpty()
                                                   select new FacultyEducation
                                                   {
                                                       studiedEducation = ec.courseStudied
                                                   }).ToList();

                teachingFaculty.Add(collegeFaculty);

            }
            //Export Technical Faculty Details-16-09-2014
            if (strType == "Export" && teachingFaculty.Count() > 0)
            {
                Response.ClearContent();
                Response.Buffer = true;
                Response.AddHeader("content-disposition", "attachment; filename=Technical Faculty Details.xls");
                Response.ContentType = "application/vnd.ms-excel";
                return PartialView("~/Views/Reports/_TeachingFacultyReport.cshtml", teachingFaculty);
            }
            //
            ViewBag.Type = UAAAS.Models.Utilities.EncryptString(type.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]);
            //ViewBag.Type = type;
            //ViewBag.Id = UAAAS.Models.Utilities.Encrypt("0", WebConfigurationManager.AppSettings["CryptoKey"]);
            ViewBag.Id = 0;
            ViewBag.Count = teachingFaculty.Count();
            if (Roles.IsUserInRole("Admin") == true)
            {
                ViewBag.Admin = true;
            }
            return View("ViewTechnical", teachingFaculty);
        }

        [Authorize(Roles = "Admin,College")]
        public ActionResult PrintTechnical(string id)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userID).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                userCollegeID = userCollegeID = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(id, WebConfigurationManager.AppSettings["CryptoKey"]));
            }
            DateTime todayDate = DateTime.Now.Date;
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();

            var status = GetPageEditableStatus(userCollegeID);

            if (status > 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("Technical");
            }
            int type = db.jntuh_faculty_type.Where(t => t.facultyType == "Technical").Select(t => t.id).FirstOrDefault();
            List<jntuh_college_faculty> jntuh_college_faculty = db.jntuh_college_faculty.Where(f => f.facultyTypeId == type && f.collegeId == userCollegeID).ToList();

            List<CollegeFaculty> teachingFaculty = new List<CollegeFaculty>();

            foreach (var faculty in jntuh_college_faculty)
            {
                CollegeFaculty collegeFaculty = new CollegeFaculty();
                collegeFaculty.id = faculty.id;
                collegeFaculty.collegeId = faculty.collegeId;
                collegeFaculty.facultyTypeId = faculty.facultyTypeId;
                collegeFaculty.facultyFirstName = faculty.facultyFirstName;
                collegeFaculty.facultyLastName = faculty.facultyLastName;
                collegeFaculty.facultySurname = faculty.facultySurname;
                collegeFaculty.facultyGenderId = faculty.facultyGenderId;
                collegeFaculty.facultyFatherName = faculty.facultyFatherName;
                collegeFaculty.facultyCategoryId = faculty.facultyCategoryId;

                if (faculty.facultyDateOfBirth != null)
                    collegeFaculty.dateOfBirth = UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.facultyDateOfBirth.ToString());

                collegeFaculty.facultyDesignationId = faculty.facultyDesignationId;
                collegeFaculty.designation = db.jntuh_designation.Where(d => d.id == faculty.facultyDesignationId).Select(d => d.designation).FirstOrDefault();
                collegeFaculty.facultyOtherDesignation = faculty.facultyOtherDesignation;
                collegeFaculty.facultyDepartmentId = faculty.facultyDepartmentId;
                collegeFaculty.department = db.jntuh_department.Where(d => d.id == faculty.facultyDepartmentId).Select(d => d.departmentName).FirstOrDefault();

                if (faculty.facultyDateOfAppointment != null)
                    collegeFaculty.dateOfAppointment = UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.facultyDateOfAppointment.ToString());

                if (faculty.facultyDateOfResignation != null)
                    collegeFaculty.dateOfResignation = UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.facultyDateOfResignation.ToString());

                collegeFaculty.isFacultyRatifiedByJNTU = faculty.isFacultyRatifiedByJNTU;

                if (faculty.facultyDateOfRatification != null)
                    collegeFaculty.dateOfRatification = UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.facultyDateOfRatification.ToString());

                collegeFaculty.facultyDurationOfRatification = faculty.facultyDurationOfRatification;
                collegeFaculty.facultySalary = faculty.facultySalary;
                collegeFaculty.facultyPreviousExperience = Convert.ToInt32(faculty.facultyPreviousExperience);
                collegeFaculty.facultyPayScale = faculty.facultyPayScale;
                collegeFaculty.facultyEmail = faculty.facultyEmail;
                collegeFaculty.facultyMobile = faculty.facultyMobile;
                collegeFaculty.facultyPANNumber = faculty.facultyPANNumber;
                collegeFaculty.facultyAadhaarNumber = faculty.facultyAadhaarNumber;
                collegeFaculty.isRelatedToExamBranch = faculty.isRelatedToExamBranch;
                collegeFaculty.isRelatedToPlacementCell = faculty.isRelatedToPlacementCell;
                collegeFaculty.facultyAchievements1 = faculty.facultyAchievements1;
                collegeFaculty.facultyAchievements2 = faculty.facultyAchievements2;
                collegeFaculty.facultyAchievements3 = faculty.facultyAchievements3;
                collegeFaculty.photo = faculty.facultyPhoto;

                teachingFaculty.Add(collegeFaculty);

            }

            ViewBag.Type = UAAAS.Models.Utilities.EncryptString(type.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]);
            //ViewBag.Type = type;
            //ViewBag.Id = UAAAS.Models.Utilities.Encrypt("0", WebConfigurationManager.AppSettings["CryptoKey"]);
            ViewBag.Id = 0;
            ViewBag.Count = teachingFaculty.Count();
            return View("PrintTechnical", teachingFaculty);
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        [HttpGet]
        public ActionResult Delete(string collegeId, string fid)
        {
            if (Membership.GetUser() == null)
            {
                return RedirectToAction("LogOn", "Account");
            }
            int facultyId = 0;
            TempData["path"] = null;
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);

            int userCollegeID = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userID).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();

            if (userCollegeID == 0 && collegeId != null)
            {
                if (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry"))
                {
                    userCollegeID = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(collegeId, WebConfigurationManager.AppSettings["CryptoKey"]));
                }
            }

            if (!string.IsNullOrEmpty(fid))
            {
                //  facultyId = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(fid, WebConfigurationManager.AppSettings["CryptoKey"]));
                // Changed by Naushad Khan
                facultyId = Convert.ToInt32(fid);
            }
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            ViewBag.collegeId = UAAAS.Models.Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString();
            ViewBag.Institutions = db.jntuh_college.Where(c => c.isActive == true && c.id != 375).Select(c => new { CollegeId = c.id, CollegeName = c.collegeCode + "-" + c.collegeName }).OrderBy(c => c.CollegeName).ToList();
            CollegeFaculty faculty = new CollegeFaculty();
            jntuh_registered_faculty regfaculty = new jntuh_registered_faculty();
            if (facultyId != 0)
            {
                regfaculty = db.jntuh_registered_faculty.Find(facultyId);
                var jntuh_college_faculty_registered = db.jntuh_college_faculty_registered.Where(e => e.RegistrationNumber.Trim() == regfaculty.RegistrationNumber.Trim()).Select(e => e).FirstOrDefault();
                if (jntuh_college_faculty_registered == null)
                {
                    TempData["Error"] = "Faculty Data not found.";
                    return RedirectToAction("Teaching", "PA_Faculty");
                }
                faculty.id = jntuh_college_faculty_registered.id;
                faculty.facultyId = regfaculty.id;
                faculty.collegeId = userCollegeID;
                faculty.facultySurname = regfaculty.FirstName;
                faculty.facultyFirstName = regfaculty.MiddleName;
                faculty.facultyLastName = regfaculty.LastName;
                faculty.isActive = regfaculty.isActive;
                // faculty.facultyAadhaarNumber = regfaculty.AadhaarNumber;
                faculty.facultyAadhaarNumber = jntuh_college_faculty_registered.AadhaarNumber;
                faculty.facultyAadharDocument = jntuh_college_faculty_registered.AadhaarDocument;


                faculty.facultyDesignationId = regfaculty.DesignationId;
                faculty.designation = db.jntuh_designation.Where(d => d.id == faculty.facultyDesignationId).Select(d => d.designation).FirstOrDefault();
                faculty.facultyOtherDesignation = regfaculty.OtherDesignation;
                faculty.FacultyRegistrationNumber = regfaculty.RegistrationNumber;
                ViewBag.id = jntuh_college_faculty_registered.id;
                faculty.facultyRecruitedFor = jntuh_college_faculty_registered.IdentifiedFor;

                if (jntuh_college_faculty_registered.IdentifiedFor == "UG")
                {
                    faculty.facultyDepartmentId = jntuh_college_faculty_registered.DepartmentId ?? 0;
                }
                else
                {
                    faculty.facultyPGDepartmentId = jntuh_college_faculty_registered.DepartmentId ?? 0;
                    faculty.SpecializationId = jntuh_college_faculty_registered.SpecializationId;
                }

                faculty.FacultySpecalizationId = jntuh_college_faculty_registered.FacultySpecializationId;
                // faculty.facultyDepartmentId = jntuh_college_faculty_registered.DepartmentId ?? 0;
                faculty.department = db.jntuh_department.Where(d => d.id == faculty.facultyDepartmentId).Select(d => d.departmentName).FirstOrDefault();

                int AadhaarCount = db.jntuh_college_faculty_registered_copy.Where(s => s.collegeId == faculty.collegeId && s.RegistrationNumber == faculty.FacultyRegistrationNumber).Select(e => e.collegeId).FirstOrDefault();
                //int CollegeFacultyAadhaarCount = db.jntuh_college_faculty_registered.Where(s => s.collegeId == faculty.collegeId && s.RegistrationNumber == faculty.FacultyRegistrationNumber).Select(e => e.id).FirstOrDefault();

                if ((AadhaarCount == 0 || AadhaarCount == null))
                {
                    TempData["AadhaarCount"] = null;
                }
                else
                {
                    TempData["AadhaarCount"] = AadhaarCount;
                }
            }

            ViewBag.facId = facultyId;
            var jntuh_specialization = db.jntuh_specialization.Where(s => s.isActive == true).Select(e => e).ToList();
            var jntuh_department = db.jntuh_department.Where(s => s.isActive == true).Select(e => e).ToList();
            var jntuh_degree = db.jntuh_degree.Where(s => s.isActive == true).Select(e => e).ToList();


            var jntuh_academic_year = db.jntuh_academic_year.ToList();
            int actualYear = jntuh_academic_year.Where(s => s.isActive == true && s.isPresentAcademicYear == true).Select(s => s.actualYear).FirstOrDefault();

            int AY0 = jntuh_academic_year.Where(s => s.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            int AY1 = jntuh_academic_year.Where(s => s.actualYear == actualYear).Select(s => s.id).FirstOrDefault();
            int AY2 = jntuh_academic_year.Where(s => s.actualYear == (actualYear - 1)).Select(s => s.id).FirstOrDefault();
            int AY3 = jntuh_academic_year.Where(s => s.actualYear == (actualYear - 2)).Select(s => s.id).FirstOrDefault();


            //var jntuh_college_intake_existing = db.jntuh_college_intake_existing.Where(s => s.collegeId == userCollegeID && (s.academicYearId == AY1 || s.academicYearId == AY2 || s.academicYearId == AY3) && s.shiftId == 1).GroupBy(s => new { s.specializationId, s.shiftId }).Select(s => s.FirstOrDefault()).ToList();
            var jntuh_college_intake_existing = db.jntuh_college_intake_existing.Where(s => s.collegeId == userCollegeID && (s.academicYearId == AY0 || s.academicYearId == AY1 || s.academicYearId == AY2 || s.academicYearId == AY3) && s.shiftId == 1).Select(s => s).ToList();
            jntuh_college_intake_existing =
                jntuh_college_intake_existing.GroupBy(s => new { s.specializationId, s.shiftId })
                    .Select(s => s.First())
                    .ToList();

            ////Colleges and Designation Drop Dow Codding written by Narayana Reddy on 11-02-2020
            //List<SelectListItem> colleges = new List<SelectListItem>();
            //var colleges_list = db.jntuh_college.Where(c => c.isActive == true && c.id != 375).Select(c => new { CollegeId = c.id, CollegeName = c.collegeName + " (" + c.collegeCode + ")" }).OrderBy(c => c.CollegeName).ToList();
            //colleges = colleges_list.Select(s => new SelectListItem { Value = s.CollegeId.ToString(), Text = s.CollegeName }).ToList();
            //colleges.Add(new SelectListItem { Value = "375", Text = "Others" });
            //ViewBag.Institutions = colleges;
            //var Designation = db.jntuh_designation.Where(e => e.isActive == true).Select(a => new { Id = a.id, designation = a.designation }).Take(4).ToList();
            //ViewBag.Designation = Designation;
            //jntuh_registered_faculty_experience facultyexperiance =
            //    db.jntuh_registered_faculty_experience.Where(
            //        r => r.createdBycollegeId == userCollegeID && r.facultyId == regfaculty.id)
            //        .Select(s => s)
            //        .FirstOrDefault();
            //if (facultyexperiance != null)
            //{
            //    faculty.experienceId = facultyexperiance.facultyId;
            //    faculty.Previouscollegeid = (int)facultyexperiance.collegeId;
            //    faculty.Otherscollegename = facultyexperiance.OtherCollege;
            //    faculty.facultyDesignationId = facultyexperiance.facultyDesignationId;
            //    faculty.facultyOtherDesignation = facultyexperiance.OtherDesignation;
            //    faculty.dateOfAppointment = facultyexperiance.facultyDateOfAppointment.ToString();
            //    faculty.dateOfResignation = facultyexperiance.facultyDateOfResignation.ToString();
            //    faculty.ViewAppointmentOrderDocument = facultyexperiance.facultyJoiningOrder;
            //    faculty.ViewRelivingDocument = facultyexperiance.facultyRelievingLetter;
            //    if (faculty.ViewRelivingDocument == null)
            //    {
            //        faculty.facultyfresherexperiance = "Fresher";
            //    }
            //    else
            //    {
            //        faculty.facultyfresherexperiance = "Experienced";
            //    }

            //    faculty.ViewSelectionCommitteeDocument = facultyexperiance.FacultySCMDocument;
            //}

            return PartialView("FacultyDelete", faculty);
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        [HttpPost]
        public ActionResult Delete(CollegeFaculty faculty)
        {
            //return RedirectToAction("Index", "UnderConstruction");
            //return RedirectToAction("Teaching", "Faculty");
            if (Membership.GetUser() == null)
            {
                return RedirectToAction("LogOn", "Account");
            }
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            int actualYear = db.jntuh_academic_year.Where(s => s.isActive == true && s.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int presentAyId = db.jntuh_academic_year.Where(s => s.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            if (faculty.facultyId == 0 || faculty.RelivingDocument == null || faculty.id == 0)
            {
                TempData["ERROR"] = "No data found";
                return RedirectToAction("Teaching", "PA_Faculty");
            }

            if (faculty.facultyId != 0 && faculty.RelivingDocument != null && faculty.id != 0)
            {
                //Before Delete Registration Number Save Faculty Experiance of Reliving Letter and Reliving Date
                var regfacultydata =
                    db.jntuh_registered_faculty.Where(r => r.id == faculty.facultyId).Select(s => s).FirstOrDefault();
                if (regfacultydata == null)
                {
                    TempData["ERROR"] = "No data found";
                    return RedirectToAction("Teaching", "PA_Faculty");
                }
                jntuh_registered_faculty_experience facultyexperiance = new jntuh_registered_faculty_experience();
                //
                string facultyappointmentletters = "~/Content/Upload/College/Faculty/AppointmentLetters";
                string facultyrelevingletters = "~/Content/Upload/College/Faculty/RelevingLetters";
                string facultypreviouscollegescms = "~/Content/Upload/College/Faculty/PreviousSCMs";

                facultyexperiance.facultyId = regfacultydata.id;
                facultyexperiance.createdBycollegeId = userCollegeID;
                if (faculty.AppointmentOrderDocument != null)
                {
                    if (!Directory.Exists(Server.MapPath(facultyappointmentletters)))
                    {
                        Directory.CreateDirectory(Server.MapPath(facultyappointmentletters));
                    }

                    var ext = Path.GetExtension(faculty.AppointmentOrderDocument.FileName);

                    if (ext.ToUpper().Equals(".PDF"))
                    {
                        string fileName = DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" +
                        regfacultydata.FirstName.Substring(0, 1) + "-" + regfacultydata.LastName.Substring(0, 1);
                        faculty.AppointmentOrderDocument.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(facultyappointmentletters),
                            fileName, ext));
                        faculty.ViewAppointmentOrderDocument = string.Format("{0}{1}", fileName, ext);
                        facultyexperiance.facultyJoiningOrder = faculty.ViewAppointmentOrderDocument;
                    }
                }

                if (faculty.RelivingDocument != null)
                {
                    if (!Directory.Exists(Server.MapPath(facultyrelevingletters)))
                    {
                        Directory.CreateDirectory(Server.MapPath(facultyrelevingletters));
                    }

                    var ext = Path.GetExtension(faculty.RelivingDocument.FileName);

                    if (ext.ToUpper().Equals(".PDF"))
                    {
                        string fileName = DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" +
                        regfacultydata.FirstName.Substring(0, 1) + "-" + regfacultydata.LastName.Substring(0, 1);
                        faculty.RelivingDocument.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(facultyrelevingletters),
                            fileName, ext));
                        faculty.ViewRelivingDocument = string.Format("{0}{1}", fileName, ext);
                        facultyexperiance.facultyRelievingLetter = faculty.ViewRelivingDocument;
                    }
                }

                //Optional
                if (faculty.SelectionCommitteeDocument != null)
                {
                    if (!Directory.Exists(Server.MapPath(facultypreviouscollegescms)))
                    {
                        Directory.CreateDirectory(Server.MapPath(facultypreviouscollegescms));
                    }

                    var ext = Path.GetExtension(faculty.SelectionCommitteeDocument.FileName);

                    if (ext.ToUpper().Equals(".PDF"))
                    {
                        string fileName = DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" +
                        regfacultydata.FirstName.Substring(0, 1) + "-" + regfacultydata.LastName.Substring(0, 1);
                        faculty.SelectionCommitteeDocument.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(facultypreviouscollegescms),
                            fileName, ext));
                        faculty.ViewSelectionCommitteeDocument = string.Format("{0}{1}", fileName, ext);
                        facultyexperiance.FacultySCMDocument = faculty.ViewSelectionCommitteeDocument;
                    }
                }
                if (faculty.Previouscollegeid == 0)
                {
                    facultyexperiance.collegeId = userCollegeID;
                }
                else
                {
                    if (faculty.Previouscollegeid == 375)
                    {
                        facultyexperiance.collegeId = faculty.Previouscollegeid;
                        facultyexperiance.OtherCollege = faculty.Otherscollegename;
                    }
                    else
                    {
                        facultyexperiance.collegeId = faculty.Previouscollegeid;
                    }
                }


                if (faculty.facultyDesignationId == 4)
                {
                    facultyexperiance.facultyDesignationId = faculty.facultyDesignationId;
                    facultyexperiance.OtherDesignation = faculty.facultyOtherDesignation;
                }
                else
                {
                    facultyexperiance.facultyDesignationId = faculty.facultyDesignationId;
                }
                if (!String.IsNullOrEmpty(faculty.dateOfAppointment))
                    facultyexperiance.facultyDateOfAppointment = Models.Utilities.DDMMYY2MMDDYY(faculty.dateOfAppointment);
                if (!String.IsNullOrEmpty(faculty.dateOfResignation))
                    facultyexperiance.facultyDateOfResignation =
                    Models.Utilities.DDMMYY2MMDDYY(faculty.dateOfResignation);

                facultyexperiance.createdBy = userID;
                facultyexperiance.createdOn = DateTime.Now;
                facultyexperiance.updatedBy = null;
                facultyexperiance.updatedOn = null;
                db.jntuh_registered_faculty_experience.Add(facultyexperiance);
                db.SaveChanges();


                jntuh_college_faculty_registered fToDelete = db.jntuh_college_faculty_registered.Where(f => f.id == faculty.id && f.collegeId == userCollegeID).Select(i => i).FirstOrDefault();
                if (fToDelete != null)
                {
                    db.jntuh_college_faculty_registered.Remove(fToDelete);
                    //This code written by Narayana Reddy because some times getting Error
                    try
                    {
                        db.SaveChanges();
                    }
                    catch (OptimisticConcurrencyException ex)
                    {
                        //db.Refresh(RefreshMode.ClientWins, db.jntuh_college_faculty_registered);
                        //db.SaveChanges();
                        const int DelayOnRetry = 1000;
                        Thread.Sleep(DelayOnRetry);
                        db.SaveChanges();
                    }

                    var jntuhFacultyTrackingData =
                        db.jntuh_college_facultytracking.AsNoTracking()
                            .Where(
                                i =>
                                    i.RegistrationNumber == fToDelete.RegistrationNumber &&
                                    i.collegeId == fToDelete.collegeId && i.isActive == true && i.ActionType != 2 && i.FacultyType == "Faculty")
                            .Select(e => e)
                            .OrderByDescending(i => i.Id)
                            .FirstOrDefault();

                    jntuh_college_facultytracking objART = new jntuh_college_facultytracking();
                    //jntuh_attendence_registrationnumberstracking objART = new jntuh_attendence_registrationnumberstracking();
                    objART.academicYearId = presentAyId;
                    objART.collegeId = fToDelete.collegeId;
                    objART.RegistrationNumber = fToDelete.RegistrationNumber;
                    objART.DepartmentId = fToDelete.DepartmentId;
                    objART.SpecializationId = fToDelete.SpecializationId;
                    objART.ActionType = 2;
                    objART.FacultyType = "Faculty";
                    objART.FacultyStatus = "Y";
                    objART.Reasion = "Faculty Deleted by College Successfully.";
                    // objART.FacultyStatus = "Y";
                    //  objART.Reasion = "Faculty Deleted by College Successfully.";
                    //objART.FacultyJoinDate = fToDelete.createdOn;
                    if (jntuhFacultyTrackingData != null)
                    {
                        objART.previousworkingcollegeid = jntuhFacultyTrackingData.previousworkingcollegeid;
                        objART.scmdocument = jntuhFacultyTrackingData.scmdocument;
                        objART.FacultyJoinDate = jntuhFacultyTrackingData.FacultyJoinDate;
                        objART.FacultyJoinDocument = jntuhFacultyTrackingData.FacultyJoinDocument;
                        objART.aadhaarnumber = jntuhFacultyTrackingData.aadhaarnumber;
                        objART.aadhaardocument = jntuhFacultyTrackingData.aadhaarnumber;
                        objART.payscale = jntuhFacultyTrackingData.payscale;
                        objART.designation = jntuhFacultyTrackingData.designation;
                    }
                    objART.relevingdate = Models.Utilities.DDMMYY2MMDDYY(faculty.dateOfResignation);
                    objART.relevingdocumnt = facultyexperiance.facultyRelievingLetter;
                    objART.isActive = true;
                    objART.Createdon = DateTime.Now;
                    objART.CreatedBy = userID;
                    objART.Updatedon = null;
                    objART.UpdatedBy = null;
                    db.jntuh_college_facultytracking.Add(objART);
                    db.SaveChanges();
                    TempData["success"] = "Faculty Deleted successfully";
                }
            }

            return RedirectToAction("Teaching", "PA_Faculty", new { collegeId = UAAAS.Models.Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
        }

        #region Old Delete Action on 14-02-2020
        //[Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        //[HttpPost]
        //public ActionResult Delete(string fid, string type, string rid)
        //{
        //    //return RedirectToAction("Index", "UnderConstruction");
        //    //return RedirectToAction("Teaching", "Faculty");
        //    int fID = 0;
        //    int fType = 0;
        //    int regId = 0;

        //    if (fid != "0")
        //    {
        //        fID = Convert.ToInt32(fid);
        //    }

        //    if (type != null)
        //    {
        //        fType = Convert.ToInt32(type);

        //    }

        //    if (rid != null)
        //    {
        //        regId = Convert.ToInt32(rid);

        //    }

        //    int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
        //    int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
        //    if (userCollegeID == 375)
        //    {
        //        userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
        //    }
        //    int actualYear = db.jntuh_academic_year.Where(s => s.isActive == true && s.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
        //    int presentAyId = db.jntuh_academic_year.Where(s => s.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();


        //    if (regId != 0 && regId != null)
        //    {
        //        jntuh_college_faculty_registered fToDelete = db.jntuh_college_faculty_registered.Where(f => f.id == regId && f.collegeId == userCollegeID).Select(i => i).FirstOrDefault();
        //        if (fToDelete != null)
        //        {
        //            db.jntuh_college_faculty_registered.Remove(fToDelete);
        //            //This code written by Narayana Reddy because some times getting Error
        //            try
        //            {
        //                db.SaveChanges();
        //            }
        //            catch (OptimisticConcurrencyException ex)
        //            {
        //                //db.Refresh(RefreshMode.ClientWins, db.jntuh_college_faculty_registered);
        //                //db.SaveChanges();
        //                const int DelayOnRetry = 1000;
        //                Thread.Sleep(DelayOnRetry);
        //                db.SaveChanges();
        //            }                  


        //            jntuh_college_facultytracking objART= new jntuh_college_facultytracking();
        //            //jntuh_attendence_registrationnumberstracking objART = new jntuh_attendence_registrationnumberstracking();
        //            objART.academicYearId = presentAyId;
        //            objART.collegeId = fToDelete.collegeId;
        //            objART.RegistrationNumber = fToDelete.RegistrationNumber;
        //            objART.DepartmentId = fToDelete.DepartmentId;
        //            objART.SpecializationId = fToDelete.SpecializationId;
        //            objART.ActionType = 2;
        //            objART.FacultyType = "Faculty";
        //            objART.FacultyStatus = "Y";
        //            objART.Reasion = "Faculty Deleted by College Successfully.";
        //           // objART.FacultyStatus = "Y";
        //          //  objART.Reasion = "Faculty Deleted by College Successfully.";
        //            objART.FacultyJoinDate = fToDelete.createdOn;
        //            objART.Createdon = DateTime.Now;
        //            objART.CreatedBy = userID;
        //            objART.Updatedon = null;
        //            objART.UpdatedBy = null;
        //            db.jntuh_college_facultytracking.Add(objART);
        //            db.SaveChanges();
        //            TempData["success"] = "Faculty Deleted successfully";
        //        }
        //    }

        //    string rtnAction = string.Empty;
        //    if (fType == 1)
        //        rtnAction = "Teaching";
        //    else if (fType == 2)
        //        rtnAction = "Technical";
        //    else if (fType == 3)
        //        rtnAction = "NonTeaching";

        //    return RedirectToAction(rtnAction, "Faculty", new { collegeId = UAAAS.Models.Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
        //}
        #endregion

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult AdocFacultyDelete(string fid, string type, string rid)
        {
            //return RedirectToAction("Teaching", "Faculty");
            int fID = 0;
            int fType = 0;
            int regId = 0;

            if (fid != "0")
            {
                fID = Convert.ToInt32(fid);
            }

            if (type != null)
            {
                fType = Convert.ToInt32(type);

            }

            if (rid != null)
            {
                regId = Convert.ToInt32(rid);

            }

            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            //if (userCollegeID == 375)
            //{
            //    userCollegeID = 196;
            //}           

            if (regId != 0 && regId != null)
            {
                jntuh_college_faculty_replaceregistered fToDelete = db.jntuh_college_faculty_replaceregistered.Where(f => f.id == regId && f.collegeId == userCollegeID).Select(i => i).FirstOrDefault();
                if (fToDelete != null)
                {
                    db.jntuh_college_faculty_replaceregistered.Remove(fToDelete);
                    db.SaveChanges();
                    TempData["success"] = "Faculty Deleted successfully";

                }
            }

            string rtnAction = string.Empty;
            if (fType == 1)
                rtnAction = "Teaching";
            else if (fType == 2)
                rtnAction = "Technical";
            else if (fType == 3)
                rtnAction = "NonTeaching";

            return RedirectToAction(rtnAction, "PA_Faculty", new { collegeId = UAAAS.Models.Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
        }
        public ActionResult UserViewTeaching(string id)
        {
            int userCollegeID = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(id, WebConfigurationManager.AppSettings["CryptoKey"]));
            int type = db.jntuh_faculty_type.Where(t => t.facultyType == "Teaching").Select(t => t.id).FirstOrDefault();
            List<jntuh_college_faculty> jntuh_college_faculty = db.jntuh_college_faculty.Where(f => f.facultyTypeId == type && f.collegeId == userCollegeID).ToList();

            List<CollegeFaculty> teachingFaculty = new List<CollegeFaculty>();

            foreach (var faculty in jntuh_college_faculty)
            {
                CollegeFaculty collegeFaculty = new CollegeFaculty();
                collegeFaculty.id = faculty.id;
                collegeFaculty.collegeId = faculty.collegeId;
                collegeFaculty.facultyTypeId = faculty.facultyTypeId;
                collegeFaculty.facultyFirstName = faculty.facultyFirstName;
                collegeFaculty.facultyLastName = faculty.facultyLastName;
                collegeFaculty.facultySurname = faculty.facultySurname;
                collegeFaculty.facultyGenderId = faculty.facultyGenderId;
                collegeFaculty.facultyFatherName = faculty.facultyFatherName;
                collegeFaculty.facultyCategoryId = faculty.facultyCategoryId;

                if (faculty.facultyDateOfBirth != null)
                    collegeFaculty.dateOfBirth = UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.facultyDateOfBirth.ToString());

                collegeFaculty.facultyDesignationId = faculty.facultyDesignationId;
                collegeFaculty.designation = db.jntuh_designation.Where(d => d.id == faculty.facultyDesignationId).Select(d => d.designation).FirstOrDefault();
                collegeFaculty.facultyOtherDesignation = faculty.facultyOtherDesignation;
                collegeFaculty.facultyDepartmentId = faculty.facultyDepartmentId;
                collegeFaculty.department = db.jntuh_department.Where(d => d.id == faculty.facultyDepartmentId).Select(d => d.departmentName).FirstOrDefault();

                if (faculty.facultyDateOfAppointment != null)
                    collegeFaculty.dateOfAppointment = UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.facultyDateOfAppointment.ToString());

                if (faculty.facultyDateOfResignation != null)
                    collegeFaculty.dateOfResignation = UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.facultyDateOfResignation.ToString());

                collegeFaculty.isFacultyRatifiedByJNTU = faculty.isFacultyRatifiedByJNTU;

                if (faculty.facultyDateOfRatification != null)
                    collegeFaculty.dateOfRatification = UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.facultyDateOfRatification.ToString());

                collegeFaculty.facultyDurationOfRatification = faculty.facultyDurationOfRatification;
                collegeFaculty.facultySalary = faculty.facultySalary;
                collegeFaculty.facultyPreviousExperience = Convert.ToInt32(faculty.facultyPreviousExperience);
                collegeFaculty.facultyPayScale = faculty.facultyPayScale;
                collegeFaculty.facultyEmail = faculty.facultyEmail;
                collegeFaculty.facultyMobile = faculty.facultyMobile;
                collegeFaculty.facultyPANNumber = faculty.facultyPANNumber;
                collegeFaculty.facultyAadhaarNumber = faculty.facultyAadhaarNumber;
                collegeFaculty.isRelatedToExamBranch = faculty.isRelatedToExamBranch;
                collegeFaculty.isRelatedToPlacementCell = faculty.isRelatedToPlacementCell;
                collegeFaculty.facultyAchievements1 = faculty.facultyAchievements1;
                collegeFaculty.facultyAchievements2 = faculty.facultyAchievements2;
                collegeFaculty.facultyAchievements3 = faculty.facultyAchievements3;
                collegeFaculty.photo = faculty.facultyPhoto;
                teachingFaculty.Add(collegeFaculty);
            }

            ViewBag.Type = UAAAS.Models.Utilities.EncryptString(type.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]);
            ViewBag.Id = 0;
            ViewBag.Count = teachingFaculty.Count();
            return View("UserViewTeaching", teachingFaculty);
        }

        public ActionResult UserViewNonTeaching(string id)
        {
            int userCollegeID = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(id, WebConfigurationManager.AppSettings["CryptoKey"]));
            int type = db.jntuh_faculty_type.Where(t => t.facultyType == "Non-Teaching").Select(t => t.id).FirstOrDefault();
            List<jntuh_college_faculty> jntuh_college_faculty = db.jntuh_college_faculty.Where(f => f.facultyTypeId == type && f.collegeId == userCollegeID).ToList();

            List<CollegeFaculty> teachingFaculty = new List<CollegeFaculty>();

            foreach (var faculty in jntuh_college_faculty)
            {
                CollegeFaculty collegeFaculty = new CollegeFaculty();
                collegeFaculty.id = faculty.id;
                collegeFaculty.collegeId = faculty.collegeId;
                collegeFaculty.facultyTypeId = faculty.facultyTypeId;
                collegeFaculty.facultyFirstName = faculty.facultyFirstName;
                collegeFaculty.facultyLastName = faculty.facultyLastName;
                collegeFaculty.facultySurname = faculty.facultySurname;
                collegeFaculty.facultyGenderId = faculty.facultyGenderId;
                collegeFaculty.facultyFatherName = faculty.facultyFatherName;
                collegeFaculty.facultyCategoryId = faculty.facultyCategoryId;

                if (faculty.facultyDateOfBirth != null)
                    collegeFaculty.dateOfBirth = UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.facultyDateOfBirth.ToString());

                collegeFaculty.facultyDesignationId = faculty.facultyDesignationId;
                collegeFaculty.designation = db.jntuh_designation.Where(d => d.id == faculty.facultyDesignationId).Select(d => d.designation).FirstOrDefault();
                collegeFaculty.facultyOtherDesignation = faculty.facultyOtherDesignation;
                collegeFaculty.facultyDepartmentId = faculty.facultyDepartmentId;
                collegeFaculty.department = db.jntuh_department.Where(d => d.id == faculty.facultyDepartmentId).Select(d => d.departmentName).FirstOrDefault();

                if (faculty.facultyDateOfAppointment != null)
                    collegeFaculty.dateOfAppointment = UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.facultyDateOfAppointment.ToString());

                if (faculty.facultyDateOfResignation != null)
                    collegeFaculty.dateOfResignation = UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.facultyDateOfResignation.ToString());

                collegeFaculty.isFacultyRatifiedByJNTU = faculty.isFacultyRatifiedByJNTU;

                if (faculty.facultyDateOfRatification != null)
                    collegeFaculty.dateOfRatification = UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.facultyDateOfRatification.ToString());

                collegeFaculty.facultyDurationOfRatification = faculty.facultyDurationOfRatification;
                collegeFaculty.facultySalary = faculty.facultySalary;
                collegeFaculty.facultyPreviousExperience = Convert.ToInt32(faculty.facultyPreviousExperience);
                collegeFaculty.facultyPayScale = faculty.facultyPayScale;
                collegeFaculty.facultyEmail = faculty.facultyEmail;
                collegeFaculty.facultyMobile = faculty.facultyMobile;
                collegeFaculty.facultyPANNumber = faculty.facultyPANNumber;
                collegeFaculty.facultyAadhaarNumber = faculty.facultyAadhaarNumber;
                collegeFaculty.isRelatedToExamBranch = faculty.isRelatedToExamBranch;
                collegeFaculty.isRelatedToPlacementCell = faculty.isRelatedToPlacementCell;
                collegeFaculty.facultyAchievements1 = faculty.facultyAchievements1;
                collegeFaculty.facultyAchievements2 = faculty.facultyAchievements2;
                collegeFaculty.facultyAchievements3 = faculty.facultyAchievements3;
                collegeFaculty.photo = faculty.facultyPhoto;

                teachingFaculty.Add(collegeFaculty);
            }

            ViewBag.Type = UAAAS.Models.Utilities.EncryptString(type.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]);
            ViewBag.Id = 0;
            ViewBag.Count = teachingFaculty.Count();
            return View("UserViewNonTeaching", teachingFaculty);
        }

        public ActionResult UserViewTechnical(string id)
        {
            int userCollegeID = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(id, WebConfigurationManager.AppSettings["CryptoKey"]));
            int type = db.jntuh_faculty_type.Where(t => t.facultyType == "Technical").Select(t => t.id).FirstOrDefault();
            List<jntuh_college_faculty> jntuh_college_faculty = db.jntuh_college_faculty.Where(f => f.facultyTypeId == type && f.collegeId == userCollegeID).ToList();

            List<CollegeFaculty> teachingFaculty = new List<CollegeFaculty>();

            foreach (var faculty in jntuh_college_faculty)
            {
                CollegeFaculty collegeFaculty = new CollegeFaculty();
                collegeFaculty.id = faculty.id;
                collegeFaculty.collegeId = faculty.collegeId;
                collegeFaculty.facultyTypeId = faculty.facultyTypeId;
                collegeFaculty.facultyFirstName = faculty.facultyFirstName;
                collegeFaculty.facultyLastName = faculty.facultyLastName;
                collegeFaculty.facultySurname = faculty.facultySurname;
                collegeFaculty.facultyGenderId = faculty.facultyGenderId;
                collegeFaculty.facultyFatherName = faculty.facultyFatherName;
                collegeFaculty.facultyCategoryId = faculty.facultyCategoryId;

                if (faculty.facultyDateOfBirth != null)
                    collegeFaculty.dateOfBirth = UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.facultyDateOfBirth.ToString());

                collegeFaculty.facultyDesignationId = faculty.facultyDesignationId;
                collegeFaculty.designation = db.jntuh_designation.Where(d => d.id == faculty.facultyDesignationId).Select(d => d.designation).FirstOrDefault();
                collegeFaculty.facultyOtherDesignation = faculty.facultyOtherDesignation;
                collegeFaculty.facultyDepartmentId = faculty.facultyDepartmentId;
                collegeFaculty.department = db.jntuh_department.Where(d => d.id == faculty.facultyDepartmentId).Select(d => d.departmentName).FirstOrDefault();

                if (faculty.facultyDateOfAppointment != null)
                    collegeFaculty.dateOfAppointment = UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.facultyDateOfAppointment.ToString());

                if (faculty.facultyDateOfResignation != null)
                    collegeFaculty.dateOfResignation = UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.facultyDateOfResignation.ToString());

                collegeFaculty.isFacultyRatifiedByJNTU = faculty.isFacultyRatifiedByJNTU;

                if (faculty.facultyDateOfRatification != null)
                    collegeFaculty.dateOfRatification = UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.facultyDateOfRatification.ToString());

                collegeFaculty.facultyDurationOfRatification = faculty.facultyDurationOfRatification;
                collegeFaculty.facultySalary = faculty.facultySalary;
                collegeFaculty.facultyPreviousExperience = Convert.ToInt32(faculty.facultyPreviousExperience);
                collegeFaculty.facultyPayScale = faculty.facultyPayScale;
                collegeFaculty.facultyEmail = faculty.facultyEmail;
                collegeFaculty.facultyMobile = faculty.facultyMobile;
                collegeFaculty.facultyPANNumber = faculty.facultyPANNumber;
                collegeFaculty.facultyAadhaarNumber = faculty.facultyAadhaarNumber;
                collegeFaculty.isRelatedToExamBranch = faculty.isRelatedToExamBranch;
                collegeFaculty.isRelatedToPlacementCell = faculty.isRelatedToPlacementCell;
                collegeFaculty.facultyAchievements1 = faculty.facultyAchievements1;
                collegeFaculty.facultyAchievements2 = faculty.facultyAchievements2;
                collegeFaculty.facultyAchievements3 = faculty.facultyAchievements3;
                collegeFaculty.photo = faculty.facultyPhoto;
                teachingFaculty.Add(collegeFaculty);
            }
            ViewBag.Type = UAAAS.Models.Utilities.EncryptString(type.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]);
            ViewBag.Id = 0;
            ViewBag.Count = teachingFaculty.Count();
            return View("UserViewTechnical", teachingFaculty);
        }


        public ActionResult printpdf(string htmldata)
        {
            Session["htmldata"] = htmldata;
            return Json("", JsonRequestBehavior.AllowGet);
        }

        public ActionResult printpdfdata()
        {
            string htmldata = Session["htmldata"].ToString();
            Response.AddHeader("Content-disposition", "attachment; filename=Teaching_Faculty_report.pdf");
            Response.ContentType = "application/octet-stream";
            String htmlText = htmldata.ToString();
            Document document = new Document(PageSize.A4);
            string filePath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            PdfWriter.GetInstance(document, Response.OutputStream);
            StyleSheet styles = new iTextSharp.text.html.simpleparser.StyleSheet();
            styles.LoadTagStyle("col21", "border", "1px solid #bbb");
            styles.LoadTagStyle(HtmlTags.TABLE, HtmlTags.FONTSIZE, "10");
            //styles.LoadTagStyle(HtmlTags.TD, HtmlTags.BGCOLOR, "#fcf1d1");
            document.Open();
            iTextSharp.text.html.simpleparser.HTMLWorker hw = new iTextSharp.text.html.simpleparser.HTMLWorker(document);
            hw.SetStyleSheet(styles);
            hw.Parse(new StringReader(htmlText));
            document.Close();
            Session["htmldata"] = "";
            List<CollegeFaculty> teachingFaculty = new List<CollegeFaculty>();
            return View("ViewTeaching", teachingFaculty);
        }

        [Authorize(Roles = "Admin,SuperAdmin,College")]
        public ActionResult ViewFacultyDetails(string fid)
        {
            return RedirectToAction("Teaching");
            FacultyRegistration regFaculty = new FacultyRegistration();
            int fID = 0;

            if (!string.IsNullOrEmpty(fid))
            {
                regFaculty.GenderId = null;
                regFaculty.isFacultyRatifiedByJNTU = null;
                // fID = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(fid, WebConfigurationManager.AppSettings["CryptoKey"]));
                // Above code commented by Naushad Khan Anbd added the below line.
                //fID = Convert.ToInt32(fid);
                fID = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(fid, WebConfigurationManager.AppSettings["CryptoKey"]));
                jntuh_registered_faculty faculty = db.jntuh_registered_faculty.Find(fID);
                if (faculty != null)
                {
                    regFaculty.id = fID;
                    regFaculty.Type = faculty.type;
                    regFaculty.RegistrationNumber = faculty.RegistrationNumber;
                    regFaculty.UserName =
                        db.my_aspnet_users.Where(u => u.id == faculty.UserId).Select(u => u.name).FirstOrDefault();
                    regFaculty.Email = faculty.Email;
                    regFaculty.UniqueID = faculty.UniqueID;
                    regFaculty.FirstName = faculty.FirstName;
                    regFaculty.MiddleName = faculty.MiddleName;
                    regFaculty.LastName = faculty.LastName;
                    regFaculty.FatherOrhusbandName = faculty.FatherOrHusbandName;
                    regFaculty.MotherName = faculty.MotherName;
                    regFaculty.GenderId = faculty.GenderId;
                    regFaculty.CollegeId =
                        db.jntuh_college_faculty_registered.Where(
                            f => f.RegistrationNumber == regFaculty.RegistrationNumber)
                            .Select(s => s.collegeId)
                            .FirstOrDefault();
                    if (faculty.DateOfBirth != null)
                    {
                        regFaculty.facultyDateOfBirth =
                            UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.DateOfBirth.ToString());
                    }
                    regFaculty.Mobile = faculty.Mobile;
                    regFaculty.facultyPhoto = faculty.Photo;
                    regFaculty.PANNumber = faculty.PANNumber;
                    regFaculty.facultyPANCardDocument = faculty.PANDocument;
                    regFaculty.AadhaarNumber = faculty.AadhaarNumber;
                    regFaculty.facultyAadhaarCardDocument = faculty.AadhaarDocument;
                    regFaculty.WorkingStatus = faculty.WorkingStatus;
                    regFaculty.TotalExperience = faculty.TotalExperience;
                    regFaculty.OrganizationName = faculty.OrganizationName;
                    if (regFaculty.CollegeId != 0)
                    {
                        regFaculty.CollegeName = db.jntuh_college.Find(regFaculty.CollegeId).collegeName;
                    }
                    //regFaculty.CollegeId = faculty.collegeId;
                    if (faculty.DepartmentId != null)
                    {
                        regFaculty.department = db.jntuh_department.Find(faculty.DepartmentId).departmentName;
                    }
                    regFaculty.DepartmentId = faculty.DepartmentId;
                    regFaculty.OtherDepartment = faculty.OtherDepartment;

                    if (faculty.DesignationId != null)
                    {
                        regFaculty.designation = db.jntuh_designation.Find(faculty.DesignationId).designation;
                    }
                    regFaculty.DesignationId = faculty.DesignationId;
                    regFaculty.OtherDesignation = faculty.OtherDesignation;

                    if (faculty.DateOfAppointment != null)
                    {
                        regFaculty.facultyDateOfAppointment =
                            UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.DateOfAppointment.ToString());
                    }
                    regFaculty.TotalExperiencePresentCollege = faculty.TotalExperiencePresentCollege;
                    regFaculty.isFacultyRatifiedByJNTU = faculty.isFacultyRatifiedByJNTU;
                    if (faculty.DateOfRatification != null)
                    {
                        regFaculty.facultyDateOfRatification =
                            UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.DateOfRatification.ToString());
                    }
                    regFaculty.ProceedingsNo = faculty.ProceedingsNumber;
                    regFaculty.SelectionCommitteeProcedings = faculty.ProceedingDocument;
                    regFaculty.AICTEFacultyId = faculty.AICTEFacultyId;
                    regFaculty.GrossSalary = faculty.grosssalary;
                    regFaculty.National = faculty.National;
                    regFaculty.InterNational = faculty.InterNational;
                    regFaculty.Citation = faculty.Citation;
                    regFaculty.Awards = faculty.Awards;
                    regFaculty.isActive = faculty.isActive;
                    regFaculty.isApproved = faculty.isApproved;
                    regFaculty.isView = true;
                    regFaculty.DeactivationReason = faculty.DeactivationReason;


                    regFaculty.FacultyEducation = db.jntuh_education_category.Where(e => e.isActive == true && (e.id == 1 || e.id == 3 || e.id == 4 || e.id == 5 || e.id == 6))
                            .Select(e => new RegisteredFacultyEducation
                            {
                                educationId = e.id,
                                educationName = e.educationCategoryName,
                                studiedEducation = db.jntuh_registered_faculty_education.Where(fe => fe.educationId == e.id && fe.facultyId == fID).Select(fe => fe.courseStudied).FirstOrDefault(),
                                specialization = db.jntuh_registered_faculty_education.Where(fe => fe.educationId == e.id && fe.facultyId == fID).Select(fe => fe.specialization).FirstOrDefault(),
                                passedYear = db.jntuh_registered_faculty_education.Where(fe => fe.educationId == e.id && fe.facultyId == fID).Select(fe => fe.passedYear).FirstOrDefault(),
                                percentage = db.jntuh_registered_faculty_education.Where(fe => fe.educationId == e.id && fe.facultyId == fID).Select(fe => fe.marksPercentage).FirstOrDefault(),
                                division = db.jntuh_registered_faculty_education.Where(fe => fe.educationId == e.id && fe.facultyId == fID).Select(fe => fe.division).FirstOrDefault(),
                                university = db.jntuh_registered_faculty_education.Where(fe => fe.educationId == e.id && fe.facultyId == fID).Select(fe => fe.boardOrUniversity).FirstOrDefault(),
                                place = db.jntuh_registered_faculty_education.Where(fe => fe.educationId == e.id && fe.facultyId == fID).Select(fe => fe.placeOfEducation).FirstOrDefault(),
                                facultyCertificate = db.jntuh_registered_faculty_education.Where(fe => fe.educationId == e.id && fe.facultyId == fID).Select(fe => fe.certificate).FirstOrDefault(),
                            }).ToList();

                    foreach (var item in regFaculty.FacultyEducation)
                    {
                        if (item.division == null)
                            item.division = 0;
                    }

                    string registrationNumber =
                        db.jntuh_registered_faculty.Where(of => of.id == fID)
                            .Select(of => of.RegistrationNumber)
                            .FirstOrDefault();
                    int facultyId =
                        db.jntuh_college_faculty_registered.Where(of => of.RegistrationNumber == registrationNumber)
                            .Select(of => of.id)
                            .FirstOrDefault();
                    //int[] verificationOfficers =
                    //    db.jntuh_college_faculty_verified.Where(v => v.FacultyId == facultyId)
                    //        .Select(v => v.VerificationOfficer)
                    //        .Distinct()
                    //        .ToArray();
                    int userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);

                    //bool isValid = ShowHideLink(fID);

                    //ViewBag.HideVerifyLink = isValid;

                    //if (verificationOfficers.Contains(userId))
                    //{
                    //    if (isValid)
                    //    {
                    //        ViewBag.HideVerifyLink = true;
                    //    }
                    //    else
                    //    {
                    //        ViewBag.HideVerifyLink = false;
                    //    }
                    //}

                    //if (verificationOfficers.Count() == 3)
                    //{
                    //    ViewBag.HideVerifyLink = true;
                    //}

                    ViewBag.HideVerifyLink = regFaculty.isApproved != null ? true : false;
                    return View(regFaculty);
                }
                else
                {
                    return RedirectToAction("Teaching", "PA_Faculty");
                }
            }
            else
            {
                return RedirectToAction("Teaching", "PA_Faculty");
            }
        }
        //New Action Due to Encripited Sting 
        [Authorize(Roles = "Admin,SuperAdmin,College")]
        public ActionResult ViewAdhocFacultyDetails(string fid)
        {
            FacultyRegistration regFaculty = new FacultyRegistration();
            int fID = 0;

            if (!string.IsNullOrEmpty(fid))
            {
                regFaculty.GenderId = null;
                regFaculty.isFacultyRatifiedByJNTU = null;
                // fID = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(fid, WebConfigurationManager.AppSettings["CryptoKey"]));
                // Above code commented by Naushad Khan Anbd added the below line.
                fID = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(fid, WebConfigurationManager.AppSettings["CryptoKey"]));
                jntuh_registered_faculty faculty = db.jntuh_registered_faculty.Find(fID);
                if (faculty != null)
                {
                    regFaculty.id = fID;
                    regFaculty.Type = faculty.type;
                    regFaculty.RegistrationNumber = faculty.RegistrationNumber;
                    regFaculty.UserName =
                        db.my_aspnet_users.Where(u => u.id == faculty.UserId).Select(u => u.name).FirstOrDefault();
                    regFaculty.Email = faculty.Email;
                    regFaculty.UniqueID = faculty.UniqueID;
                    regFaculty.FirstName = faculty.FirstName;
                    regFaculty.MiddleName = faculty.MiddleName;
                    regFaculty.LastName = faculty.LastName;
                    regFaculty.FatherOrhusbandName = faculty.FatherOrHusbandName;
                    regFaculty.MotherName = faculty.MotherName;
                    regFaculty.GenderId = faculty.GenderId;
                    regFaculty.CollegeId =
                        db.jntuh_college_faculty_registered.Where(
                            f => f.RegistrationNumber == regFaculty.RegistrationNumber)
                            .Select(s => s.collegeId)
                            .FirstOrDefault();
                    if (faculty.DateOfBirth != null)
                    {
                        regFaculty.facultyDateOfBirth =
                            UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.DateOfBirth.ToString());
                    }
                    regFaculty.Mobile = faculty.Mobile;
                    regFaculty.facultyPhoto = faculty.Photo;
                    regFaculty.PANNumber = faculty.PANNumber;
                    regFaculty.facultyPANCardDocument = faculty.PANDocument;
                    regFaculty.AadhaarNumber = faculty.AadhaarNumber;
                    regFaculty.facultyAadhaarCardDocument = faculty.AadhaarDocument;
                    regFaculty.WorkingStatus = faculty.WorkingStatus;
                    regFaculty.TotalExperience = faculty.TotalExperience;
                    regFaculty.OrganizationName = faculty.OrganizationName;
                    if (regFaculty.CollegeId != 0)
                    {
                        regFaculty.CollegeName = db.jntuh_college.Find(regFaculty.CollegeId).collegeName;
                    }
                    //regFaculty.CollegeId = faculty.collegeId;
                    if (faculty.DepartmentId != null)
                    {
                        regFaculty.department = db.jntuh_department.Find(faculty.DepartmentId).departmentName;
                    }
                    regFaculty.DepartmentId = faculty.DepartmentId;
                    regFaculty.OtherDepartment = faculty.OtherDepartment;

                    if (faculty.DesignationId != null)
                    {
                        regFaculty.designation = db.jntuh_designation.Find(faculty.DesignationId).designation;
                    }
                    regFaculty.DesignationId = faculty.DesignationId;
                    regFaculty.OtherDesignation = faculty.OtherDesignation;

                    if (faculty.DateOfAppointment != null)
                    {
                        regFaculty.facultyDateOfAppointment =
                            UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.DateOfAppointment.ToString());
                    }
                    regFaculty.TotalExperiencePresentCollege = faculty.TotalExperiencePresentCollege;
                    regFaculty.isFacultyRatifiedByJNTU = faculty.isFacultyRatifiedByJNTU;
                    if (faculty.DateOfRatification != null)
                    {
                        regFaculty.facultyDateOfRatification =
                            UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.DateOfRatification.ToString());
                    }
                    regFaculty.ProceedingsNo = faculty.ProceedingsNumber;
                    regFaculty.SelectionCommitteeProcedings = faculty.ProceedingDocument;
                    regFaculty.AICTEFacultyId = faculty.AICTEFacultyId;
                    regFaculty.GrossSalary = faculty.grosssalary;
                    regFaculty.National = faculty.National;
                    regFaculty.InterNational = faculty.InterNational;
                    regFaculty.Citation = faculty.Citation;
                    regFaculty.Awards = faculty.Awards;
                    regFaculty.isActive = faculty.isActive;
                    regFaculty.isApproved = faculty.isApproved;
                    regFaculty.isView = true;
                    regFaculty.DeactivationReason = faculty.DeactivationReason;


                    regFaculty.FacultyEducation = db.jntuh_education_category.Where(e => e.isActive == true && (e.id == 1 || e.id == 3 || e.id == 4 || e.id == 5 || e.id == 6))
                            .Select(e => new RegisteredFacultyEducation
                            {
                                educationId = e.id,
                                educationName = e.educationCategoryName,
                                studiedEducation = db.jntuh_registered_faculty_education.Where(fe => fe.educationId == e.id && fe.facultyId == fID).Select(fe => fe.courseStudied).FirstOrDefault(),
                                specialization = db.jntuh_registered_faculty_education.Where(fe => fe.educationId == e.id && fe.facultyId == fID).Select(fe => fe.specialization).FirstOrDefault(),
                                passedYear = db.jntuh_registered_faculty_education.Where(fe => fe.educationId == e.id && fe.facultyId == fID).Select(fe => fe.passedYear).FirstOrDefault(),
                                percentage = db.jntuh_registered_faculty_education.Where(fe => fe.educationId == e.id && fe.facultyId == fID).Select(fe => fe.marksPercentage).FirstOrDefault(),
                                division = db.jntuh_registered_faculty_education.Where(fe => fe.educationId == e.id && fe.facultyId == fID).Select(fe => fe.division).FirstOrDefault(),
                                university = db.jntuh_registered_faculty_education.Where(fe => fe.educationId == e.id && fe.facultyId == fID).Select(fe => fe.boardOrUniversity).FirstOrDefault(),
                                place = db.jntuh_registered_faculty_education.Where(fe => fe.educationId == e.id && fe.facultyId == fID).Select(fe => fe.placeOfEducation).FirstOrDefault(),
                                facultyCertificate = db.jntuh_registered_faculty_education.Where(fe => fe.educationId == e.id && fe.facultyId == fID).Select(fe => fe.certificate).FirstOrDefault(),
                            }).ToList();

                    foreach (var item in regFaculty.FacultyEducation)
                    {
                        if (item.division == null)
                            item.division = 0;
                    }

                    string registrationNumber =
                        db.jntuh_registered_faculty.Where(of => of.id == fID)
                            .Select(of => of.RegistrationNumber)
                            .FirstOrDefault();
                    int facultyId =
                        db.jntuh_college_faculty_registered.Where(of => of.RegistrationNumber == registrationNumber)
                            .Select(of => of.id)
                            .FirstOrDefault();
                    //int[] verificationOfficers =
                    //    db.jntuh_college_faculty_verified.Where(v => v.FacultyId == facultyId)
                    //        .Select(v => v.VerificationOfficer)
                    //        .Distinct()
                    //        .ToArray();
                    int userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);

                    //bool isValid = ShowHideLink(fID);

                    //ViewBag.HideVerifyLink = isValid;

                    //if (verificationOfficers.Contains(userId))
                    //{
                    //    if (isValid)
                    //    {
                    //        ViewBag.HideVerifyLink = true;
                    //    }
                    //    else
                    //    {
                    //        ViewBag.HideVerifyLink = false;
                    //    }
                    //}

                    //if (verificationOfficers.Count() == 3)
                    //{
                    //    ViewBag.HideVerifyLink = true;
                    //}

                    ViewBag.HideVerifyLink = regFaculty.isApproved != null ? true : false;
                    return View("~/Views/Faculty/ViewFacultyDetails.cshtml", regFaculty);
                }
                else
                {
                    return RedirectToAction("ViewAdhocTeaching", "PA_Faculty");
                }
            }
            else
            {
                return RedirectToAction("ViewAdhocTeaching", "PA_Faculty");
            }
        }

        [Authorize(Roles = "Admin,College")]
        public ActionResult ViewTeachingFlags(string id, string strType)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userID).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();
            // int userCollegeID = 423;
            if (userCollegeID == 0)
            {
                userCollegeID = userCollegeID = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(id, WebConfigurationManager.AppSettings["CryptoKey"]));
            }

            var pharmacyids = new[]{24,27,30,33,34,44,47,52,55,65,78,83,95,97,104,105,107,110,114,117,118,127,135,136,139,146,150,159,169,202,204,213,219,234,239,253,
                262,263,267,275,283,284,290,295,297,298,301,303,314,317,320,348,353,362,376,377,379,384,392,395,410,427,428,436,442,445,448,454,457,458,6,
                45,54,58,90,120,206,237,302,313,318,319,389,60,66,252,255,315,370};

            #region Some Colleges Getting Error So checking Purpose

            int type = db.jntuh_faculty_type.Where(t => t.facultyType == "Teaching").Select(t => t.id).FirstOrDefault();
            List<jntuh_college_faculty> jntuh_college_faculty = db.jntuh_college_faculty.Where(f => f.facultyTypeId == type && f.collegeId == userCollegeID)
                .OrderBy(f => f.facultyDepartmentId).ThenBy(f => f.facultyDesignationId).ThenBy(f => f.facultyFirstName).ToList();

            List<CollegeFaculty> teachingFaculty = new List<CollegeFaculty>();

            //Commented By Srinivas.T   FacultyVerificationStatus
            //List<jntuh_college_faculty_registered> regFaculty = db.jntuh_college_faculty_registered.Where(r => r.collegeId == userCollegeID && r.existingFacultyId == null && r.createdBy != 63809).Select(r => r).ToList();
            List<jntuh_college_faculty_registered> regFaculty = db.jntuh_college_faculty_registered.Where(r => r.collegeId == userCollegeID && r.existingFacultyId == null).Select(r => r).ToList();
            var RegistredFacultyLog = db.jntuh_registered_faculty_log.Where(F => F.FacultyApprovedStatus != 1).Select(F => new { F.RegistrationNumber, F.FacultyApprovedStatus, F.Remarks }).ToList();
            var jntuh_college_faculty_registereds = db.jntuh_college_faculty_registered.Where(C => C.collegeId == userCollegeID).Select(C => C.RegistrationNumber.Trim()).ToArray();
            var jntuh_faculty = db.jntuh_registered_faculty.Where(i => jntuh_college_faculty_registereds.Contains(i.RegistrationNumber.Trim()) && i.Notin116 != true).ToList();
            foreach (var rFaculty in jntuh_faculty)
            {
                CollegeFaculty collegeFaculty = new CollegeFaculty();
                collegeFaculty.id = rFaculty.id;


                if (rFaculty.collegeId != null)
                {
                    collegeFaculty.collegeId = (int)rFaculty.collegeId;
                }
                collegeFaculty.id = rFaculty.id;
                collegeFaculty.facultyType = rFaculty.type;
                //collegeFaculty.facultyTypeId = rFaculty.facultyTypeId;
                collegeFaculty.facultyFirstName = rFaculty.FirstName;
                collegeFaculty.facultyLastName = rFaculty.MiddleName;
                collegeFaculty.facultySurname = rFaculty.LastName;
                collegeFaculty.facultyGenderId = rFaculty.GenderId;
                collegeFaculty.facultyFatherName = rFaculty.FatherOrHusbandName;
                //collegeFaculty.facultyCategoryId = rFaculty.facultyCategoryId;

                if (rFaculty.DateOfBirth != null)
                    collegeFaculty.dateOfBirth = UAAAS.Models.Utilities.MMDDYY2DDMMYY(rFaculty.DateOfBirth.ToString());

                if (rFaculty.DesignationId != null)
                {
                    collegeFaculty.facultyDesignationId = (int)rFaculty.DesignationId;
                    collegeFaculty.designation = rFaculty.jntuh_designation.designation;
                }

                collegeFaculty.facultyOtherDesignation = rFaculty.OtherDesignation;

                if (rFaculty.DepartmentId != null)
                {
                    collegeFaculty.facultyDepartmentId = (int)rFaculty.DepartmentId;
                    collegeFaculty.department = rFaculty.jntuh_department.departmentName;
                }

                collegeFaculty.facultyOtherDepartment = rFaculty.OtherDepartment;
                collegeFaculty.facultyEmail = rFaculty.Email;
                collegeFaculty.facultyMobile = rFaculty.Mobile;
                collegeFaculty.facultyPANNumber = rFaculty.PANNumber;
                collegeFaculty.facultyAadhaarNumber = rFaculty.AadhaarNumber;
                collegeFaculty.FacultyRegistrationNumber = rFaculty.RegistrationNumber;
                collegeFaculty.facultyCount = RegistredFacultyLog.Count(F => F.RegistrationNumber.Trim() == rFaculty.RegistrationNumber.Trim());
                collegeFaculty.Remarks = RegistredFacultyLog.Where(F => F.RegistrationNumber.Trim() == rFaculty.RegistrationNumber.Trim()).Select(F => F.Remarks).FirstOrDefault();
                collegeFaculty.FacultyVerificationStatus = RegistredFacultyLog.Where(F => F.RegistrationNumber.Trim() == rFaculty.RegistrationNumber.Trim()).Select(F => F.FacultyApprovedStatus).FirstOrDefault();
                var educationid = rFaculty.jntuh_registered_faculty_education.Count(e => e.facultyId == rFaculty.id) > 0 ? rFaculty.jntuh_registered_faculty_education.Where(e => e.facultyId == rFaculty.id).Select(e => e.educationId).Max() : 0;
                var notqualified = rFaculty.NotQualifiedAsperAICTE != null ? (bool)rFaculty.NotQualifiedAsperAICTE : false;
                string Reasons = "";
                if (rFaculty.Absent == true)
                {
                    Reasons = "ABSENT" + ",";
                }
                if (notqualified == true || educationid < 4)
                {
                    Reasons += "NOT QUALIFIED " + ",";
                }
                if (string.IsNullOrEmpty(rFaculty.PANNumber))
                {
                    Reasons += "NO PAN" + ",";
                }
                if (rFaculty.DepartmentId == null && !pharmacyids.Contains(userCollegeID))
                {
                    Reasons += "NO DEPARTMENT" + ",";
                }
                if (rFaculty.NoSCM == true)
                {
                    Reasons += "NO SCM/RATIFICATION" + ",";
                }
                if (rFaculty.PHDundertakingnotsubmitted == true)
                {
                    Reasons += "UNDERTAKING NOT CONFIRMED BY FACULTY" + ",";
                }
                if (rFaculty.Blacklistfaculy == true)
                {
                    Reasons += "BLACKLISTED" + ",";
                }
                if (Reasons != "")
                {
                    Reasons = Reasons.Substring(0, Reasons.Length - 1);
                }

                collegeFaculty.Reason = Reasons;



                teachingFaculty.Add(collegeFaculty);


            }

            ViewBag.Type = UAAAS.Models.Utilities.EncryptString(type.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]);
            //ViewBag.Type = 0;
            ViewBag.Id = UAAAS.Models.Utilities.EncryptString("0", WebConfigurationManager.AppSettings["CryptoKey"]);
            //ViewBag.Id = 0;
            ViewBag.Count = teachingFaculty.Count();
            teachingFaculty = teachingFaculty.Where(e => e.Reason != "").ToList();
            return View(teachingFaculty);
            #endregion
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult DeleteNonTeachingAndTechnical(string fid, string type, string rid)
        {
            int fID = 0;
            int fType = 0;
            int regId = 0;

            if (fid != "0")
            {
                fID = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(fid, WebConfigurationManager.AppSettings["CryptoKey"]));

            }

            if (type != null)
            {

                fType = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(type, WebConfigurationManager.AppSettings["CryptoKey"]));
            }

            if (rid != null)
            {

                regId = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(rid, WebConfigurationManager.AppSettings["CryptoKey"]));
            }

            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();

            if (userCollegeID == 0)
            {
                userCollegeID = db.jntuh_college_faculty.Where(f => f.id == fID).Select(f => f.collegeId).FirstOrDefault();
            }

            //facultyId is foriegn Key in Table jntuh_faculty_education from jntuh_college_faculty so,First we delete Corresponding facultyId records in jntuh_faculty_education 

            int[] facultyId = db.jntuh_faculty_education.Where(f => f.facultyId == fID).Select(f => f.id).ToArray();

            foreach (var facid in facultyId)
            {
                jntuh_faculty_education jntuh_faculty_education = new jntuh_faculty_education();
                jntuh_faculty_education = db.jntuh_faculty_education.Where(f => f.id == facid).Select(f => f).FirstOrDefault();
                if (jntuh_faculty_education != null)
                {
                    db.jntuh_faculty_education.Remove(jntuh_faculty_education);
                    db.SaveChanges();
                }
            }

            //facultyId is foriegn Key in Table jntuh_faculty_subjects from jntuh_college_faculty so,First we delete Corresponding facultyId records in jntuh_faculty_subjects

            int[] facultySunjectId = db.jntuh_faculty_subjects.Where(f => f.facultyId == fID).Select(f => f.id).ToArray();

            foreach (var sid in facultySunjectId)
            {
                jntuh_faculty_subjects jntuh_faculty_subjects = new jntuh_faculty_subjects();

                jntuh_faculty_subjects = db.jntuh_faculty_subjects.Where(f => f.id == sid).Select(f => f).FirstOrDefault();
                if (jntuh_faculty_subjects != null)
                {
                    db.jntuh_faculty_subjects.Remove(jntuh_faculty_subjects);
                    db.SaveChanges();
                }
            }

            jntuh_college_faculty rowToDelete = new jntuh_college_faculty();
            rowToDelete = db.jntuh_college_faculty.Where(f => f.id == fID && f.collegeId == userCollegeID && f.facultyTypeId == fType).Select(i => i).FirstOrDefault();
            if (rowToDelete != null)
            {
                db.jntuh_college_faculty.Remove(rowToDelete);
                db.SaveChanges();
                TempData["success"] = "deleted successfully";
            }

            if (regId != 0 && regId != null)
            {
                jntuh_college_faculty_registered fToDelete = db.jntuh_college_faculty_registered.Where(f => f.id == regId && f.collegeId == userCollegeID).Select(i => i).FirstOrDefault();
                if (fToDelete != null)
                {
                    db.jntuh_college_faculty_registered.Remove(fToDelete);
                    db.SaveChanges();



                    TempData["success"] = "deleted successfully";
                }
            }

            string rtnAction = string.Empty;
            if (fType == 1)
                rtnAction = "Teaching";
            else if (fType == 2)
                rtnAction = "Technical";
            else if (fType == 3)
                rtnAction = "NonTeaching";

            return RedirectToAction(rtnAction, "PA_Faculty", new { collegeId = UAAAS.Models.Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });

        }



        //public ActionResult CheckAadharNumber()
        //{
        //    return View();
        //}
        //Commented on 13-04-2018 due to appeal data
        //[HttpPost]
        //public JsonResult CheckAadharNumber(string facultyAadhaarNumber, string FacultyRegistrationNumber)
        //{
        //    var status = aadharcard.validateVerhoeff(facultyAadhaarNumber);
        //   // string Regno = TempData["regno"].ToString()_
        //    var jntuh_college_faculty_registered =
        //        db.jntuh_college_faculty_registered.Where(
        //            f => f.AadhaarNumber == facultyAadhaarNumber && f.RegistrationNumber != FacultyRegistrationNumber)
        //            .Select(e => e)
        //            .Count();
        //   //var jntuh_registered_faculty= db.jntuh_registered_faculty.Where(
        //   //     f => f.AadhaarNumber == facultyAadhaarNumber && f.RegistrationNumber != FacultyRegistrationNumber)
        //   //     .Select(e => e)
        //   //     .Count();

        //    if (status)
        //    {
        //        if (jntuh_college_faculty_registered==0)
        //        {
        //            return Json(true);
        //        }
        //        else
        //        {
        //            return Json("AadhaarNumber already Exists", JsonRequestBehavior.AllowGet);
        //        }
        //        //if (jntuh_registered_faculty.Count == 0)
        //        //{
        //        //    if (jntuh_registered_faculty_new == 1)
        //        //    {
        //        //        return Json(true);
        //        //    }
        //        //    else
        //        //    {
        //        //        return Json(true);
        //        //    }

        //        //}
        //        //else
        //        //{
        //        //    if (jntuh_registered_faculty.Count == jntuh_registered_faculty_new)
        //        //    {
        //        //        return Json(true);
        //        //    }
        //        //    //else
        //        //   return Json("AadhaarNumber is already Registered", JsonRequestBehavior.AllowGet);
        //        //}
        //    }
        //    else
        //    {
        //        return Json("AadhaarNumber is not a validnumber", JsonRequestBehavior.AllowGet);
        //    }

        //}
        //New code to check aadhar & Appeal Table
        [Authorize(Roles = "Admin,SuperAdmin,College,DataEntry")]
        public JsonResult CheckAadharNumber(string facultyAadhaarNumber, string FacultyRegistrationNumber)
        {
            var status = aadharcard.validateVerhoeff(facultyAadhaarNumber.Trim());
            // string Regno = TempData["regno"].ToString()_
            var jntuh_college_faculty_registered =
                db.jntuh_college_faculty_registered.Where(
                    f => f.AadhaarNumber.Trim() == facultyAadhaarNumber.Trim() && f.RegistrationNumber.Trim() != FacultyRegistrationNumber.Trim())
                    .Select(e => e)
                    .Count();
            var jntuh_college_principal_registered =
          db.jntuh_college_principal_registered.Where(
              f => f.AadhaarNumber.Trim() == facultyAadhaarNumber.Trim() && f.RegistrationNumber != FacultyRegistrationNumber.Trim())
              .Select(e => e)
              .Count();
            //var jntuh_registered_faculty= db.jntuh_registered_faculty.Where(
            //     f => f.AadhaarNumber == facultyAadhaarNumber && f.RegistrationNumber != FacultyRegistrationNumber)
            //     .Select(e => e)
            //     .Count();

            //When Appela Faculty Adding At the Time uncommet this code on 419 29-04-2019
            //var jntuh_appeal_faculty_registered =
            //    db.jntuh_appeal_faculty_registered.Where(f => f.AadhaarNumber == facultyAadhaarNumber.Trim() && f.RegistrationNumber != FacultyRegistrationNumber.Trim()&&f.academicYearId==11)
            //    .Select(e => e)
            //    .Count();

            if (status)
            {
                //if (jntuh_college_faculty_registered == 0)
                //{
                //    return Json(true);
                //}
                //else
                //{
                //    return Json("AadhaarNumber already Exists", JsonRequestBehavior.AllowGet);
                //}

                if (jntuh_college_faculty_registered == 0 && jntuh_college_principal_registered == 0)
                {
                    return Json(true);
                    //if (jntuh_appeal_faculty_registered == 0)
                    //{
                    //    return Json(true);
                    //}
                    //else
                    //{
                    //    return Json("AadhaarNumber already Exists", JsonRequestBehavior.AllowGet);
                    //}

                }
                else
                {
                    return Json("AadhaarNumber already Exists.", JsonRequestBehavior.AllowGet);
                }
                //if (jntuh_registered_faculty.Count == 0)
                //{
                //    if (jntuh_registered_faculty_new == 1)
                //    {
                //        return Json(true);
                //    }
                //    else
                //    {
                //        return Json(true);
                //    }

                //}
                //else
                //{
                //    if (jntuh_registered_faculty.Count == jntuh_registered_faculty_new)
                //    {
                //        return Json(true);
                //    }
                //    //else
                //   return Json("AadhaarNumber is already Registered", JsonRequestBehavior.AllowGet);
                //}
            }
            else
            {
                return Json("AadhaarNumber is not a validnumber.", JsonRequestBehavior.AllowGet);
            }

        }
        /// <summary>
        /// Checking Registration

        /// </summary>
        /// <param name="FacultyRegistrationNumber"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin,College,DataEntry")]
        public JsonResult CheckRegistrationNumber(string FacultyRegistrationNumber)
        {
            var RegistrationNumber = db.jntuh_registered_faculty.Where(F => F.RegistrationNumber == FacultyRegistrationNumber.Trim()).Select(s => s).FirstOrDefault();

            if (RegistrationNumber != null)
            {
                var isExistingFaculty = db.jntuh_college_faculty_registered.Where(r => r.RegistrationNumber == FacultyRegistrationNumber.Trim()).Select(r => r.RegistrationNumber).FirstOrDefault();
                if (RegistrationNumber.Blacklistfaculy == true)
                {
                    return Json("Registration Number is in Blacklist.", JsonRequestBehavior.AllowGet);
                }
                else if (RegistrationNumber.AbsentforVerification == true)
                {
                    return Json("Registration Number is in Inactive.", JsonRequestBehavior.AllowGet);
                }
                else if (isExistingFaculty != null)
                {
                    return Json("Faculty is already working.", JsonRequestBehavior.AllowGet);
                }
                else
                    return Json(true);
            }
            else
                return Json("Invalid Registration.", JsonRequestBehavior.AllowGet);

        }
        //Adoc Aaadhaar Number Checking
        public JsonResult AdocCheckAadharNumber(string AdocAadhaarNumber, string AdocFacultyRegistrationNumber)
        {
            var status = aadharcard.validateVerhoeff(AdocAadhaarNumber);

            var jntuh_college_faculty_registered =
                db.jntuh_college_faculty_registered.Where(
                    f => f.AadhaarNumber == AdocAadhaarNumber && f.RegistrationNumber != AdocFacultyRegistrationNumber)
                    .Select(e => e)
                    .Count();
            var jntuh_college_principal_registered =
          db.jntuh_college_principal_registered.Where(
              f => f.AadhaarNumber == AdocAadhaarNumber && f.RegistrationNumber != AdocFacultyRegistrationNumber)
              .Select(e => e)
              .Count();
            var jntuh_college_faculty_replaceregistered =
          db.jntuh_college_faculty_replaceregistered.Where(
              f => f.AadhaarNumber == AdocAadhaarNumber && f.RegistrationNumber != AdocFacultyRegistrationNumber)
              .Select(e => e)
              .Count();
            if (status)
            {
                if (jntuh_college_faculty_registered == 0 && jntuh_college_principal_registered == 0 && jntuh_college_faculty_replaceregistered == 0)
                {
                    return Json(true);
                }

                else
                {
                    return Json("AadhaarNumber already Exists", JsonRequestBehavior.AllowGet);
                }

            }
            else
            {
                return Json("AadhaarNumber is not a validnumber", JsonRequestBehavior.AllowGet);
            }

        }

        //[HttpPost]
        //public JsonResult CheckAadharNumberNew(string facultyAadhaarNumber)
        //{
        //    var status = aadharcard.validateVerhoeff(facultyAadhaarNumber);

        //    var jntuh_registered_faculty =
        //        db.jntuh_registered_faculty.Where(f => f.AadhaarNumber == facultyAadhaarNumber).ToList();
        //    if (status)
        //    {
        //        return Json(true);
        //    }
        //    else
        //    {
        //        return Json("AadhaarNumber is not a validnumber", JsonRequestBehavior.AllowGet);
        //    }

        //}

        public JsonResult GetAadharrnobasedonregistrationno(string regno)
        {
            string data = db.jntuh_registered_faculty.Where(f => f.RegistrationNumber == regno).Select(f => f.AadhaarNumber).SingleOrDefault();
            TempData["regno"] = regno;
            //string data = null;
            return Json(data, JsonRequestBehavior.AllowGet);
        }

    }
}
