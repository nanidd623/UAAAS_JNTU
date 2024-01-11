using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Security;
using UAAAS.Models;

namespace UAAAS.Controllers
{
     [ErrorHandling]
    public class ReInspectionFacultyVerificationController : BaseController
    {
         private uaaasDBContext db = new uaaasDBContext();
        // GET: /ReInspectionFacultyVerification/
        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification")]
         public ActionResult Index(int? collegeid)
        {
            int[] collegeIds = new int[] {48,62,125,140,143,157,161,162,252,292,305,370,371,401,415,416,422,424,447,20,33,83,107,338};
            ViewBag.Colleges =
                db.jntuh_college.Where(c => c.isActive == true && collegeIds.Contains(c.id))
                    .Select(c => new {collegeId = c.id, collegeName = c.collegeCode + "-" + c.collegeName})
                    .OrderBy(c => c.collegeName)
                    .ToList();
            ViewBag.collegeid = collegeid;
            var jntuh_department = db.jntuh_department.ToList();
            List<FacultyRegistration> teachingFaculty = new List<FacultyRegistration>();

            if (collegeid != null)
            {
                List<jntuh_college_faculty_registered> jntuh_college_faculty_registered = db.jntuh_college_faculty_registered.Where(cf => cf.collegeId == collegeid).Select(cf => cf).ToList();
                string[] strRegNoS = db.jntuh_college_faculty_registered.Where(cf => cf.collegeId == collegeid).Select(cf => cf.RegistrationNumber).ToArray();

                List<jntuh_reinspection_registered_faculty> jntuh_reinspection_registered_faculty = new List<jntuh_reinspection_registered_faculty>();
                jntuh_reinspection_registered_faculty = db.jntuh_reinspection_registered_faculty
                                             .Where(rf => strRegNoS.Contains(rf.RegistrationNumber) && rf.Notin116 != true)  //&& (rf.collegeId == null || rf.collegeId == collegeid)
                                             .ToList();

                var jntuh_notin415facultys = db.jntuh_notin415faculty.Where(F => F.CollegeId == collegeid).ToList();
                var Specializations = db.jntuh_specialization.ToList();
                string[] strREG = jntuh_notin415facultys.Select(F => F.RegistrationNumber.Trim()).ToArray();
                string RegNumber = "";
                int? Specializationid = 0;
                foreach (var a in jntuh_reinspection_registered_faculty)
                {
                    string Reason = String.Empty;
                    Specializationid = jntuh_college_faculty_registered.Where(C => C.RegistrationNumber.Trim() == a.RegistrationNumber.Trim()).Select(C => C.SpecializationId).FirstOrDefault();
                    var faculty = new FacultyRegistration();
                    faculty.id = a.id;
                    faculty.Type = a.type;
                    faculty.CollegeId = collegeid;
                    faculty.RegistrationNumber = a.RegistrationNumber;
                    faculty.UniqueID = a.UniqueID;
                    faculty.FirstName = a.FirstName;
                    faculty.MiddleName = a.MiddleName;
                    faculty.LastName = a.LastName;
                    faculty.GenderId = a.GenderId;
                    faculty.Email = a.Email;
                    faculty.facultyPhoto = a.Photo;
                    faculty.Mobile = a.Mobile;
                    faculty.PANNumber = a.PANNumber;
                    faculty.AadhaarNumber = a.AadhaarNumber;
                    faculty.isActive = a.isActive;
                    faculty.isApproved = a.isApproved;
                    faculty.department = jntuh_department.Where(d => d.id == a.DepartmentId).Select(d => d.departmentName).FirstOrDefault();
                    faculty.SamePANNumberCount = jntuh_reinspection_registered_faculty.Where(f => f.PANNumber == a.PANNumber).ToList().Count;
                    faculty.SameAadhaarNumberCount = jntuh_reinspection_registered_faculty.Where(f => f.AadhaarNumber == a.AadhaarNumber).ToList().Count;
                    faculty.SpecializationIdentfiedFor = Specializationid > 0 ? Specializations.Where(S => S.id == Specializationid).Select(S => S.specializationName).FirstOrDefault() : "";
                    faculty.isVerified = isFacultyVerified(a.id);
                    faculty.DeactivationReason = a.DeactivationReason;
                    faculty.FacultyVerificationStatus = a.FacultyVerificationStatus;
                    faculty.updatedOn = a.updatedOn;
                    faculty.createdOn = jntuh_college_faculty_registered.Where(f => f.RegistrationNumber == a.RegistrationNumber).Select(f => f.createdOn).FirstOrDefault();
                    faculty.IdentfiedFor = jntuh_college_faculty_registered.Where(f => f.RegistrationNumber.Trim() == a.RegistrationNumber.Trim()).Select(f => f.IdentifiedFor).FirstOrDefault();
                    faculty.jntuh_reinspection_registered_faculty_education = a.jntuh_reinspection_registered_faculty_education;
                    faculty.DegreeId = a.jntuh_reinspection_registered_faculty_education.Count(e => e.facultyId == a.id) > 0 ? a.jntuh_reinspection_registered_faculty_education.Where(e => e.facultyId == a.id).Select(e => e.educationId).Max() : 0;
                    faculty.PanDeactivationReasion = !string.IsNullOrEmpty(a.PanDeactivationReason) ? a.PanDeactivationReason : "";
                    faculty.Absent = a.Absent != null ? (bool)a.Absent : false;
                    faculty.BlacklistFaculty = a.Blacklistfaculy != null ? (bool)a.Blacklistfaculy : false;
                    faculty.PHDundertakingnotsubmitted = a.PHDundertakingnotsubmitted != null ? (bool)a.PHDundertakingnotsubmitted : false;
                    faculty.NOTQualifiedAsPerAICTE = a.NotQualifiedAsperAICTE != null ? (bool)a.NotQualifiedAsperAICTE : false;
                    faculty.InvalidPANNo = a.InvalidPANNumber != null ? (bool)a.InvalidPANNumber : false; ;
                    faculty.InCompleteCeritificates = a.IncompleteCertificates != null ? (bool)a.IncompleteCertificates : false; ;
                    faculty.FalsePAN = a.FalsePAN != null ? (bool)a.FalsePAN : false;
                    if (faculty.Absent == true)
                    {
                        Reason = "Absent" + ",";
                    }


                    if (faculty.NOTQualifiedAsPerAICTE == true)
                    {
                        Reason += "Not Qualified as AICTE" + ",";
                    }
                    if (faculty.InCompleteCeritificates == true)
                    {
                        Reason += "Incomplete Certificates(UG/PG/PHD/SCM)" + ",";
                    }

                    if (strREG.Contains(a.RegistrationNumber.Trim()))
                    {
                        
                        faculty.NoSCM = a.NoSCM == null ? false : (bool)a.NoSCM;
                    }

                    if (Reason != "")
                    {
                        Reason = Reason.Substring(0, Reason.Length - 1);
                    }

                    faculty.DeactivationNew = Reason;
                    teachingFaculty.Add(faculty);
                }



               
                teachingFaculty = teachingFaculty.Where(m => m.isActive == true).OrderBy(f => f.updatedOn).ThenBy(f => f.department).ToList();
            
                return View(teachingFaculty);
            }

            return View(teachingFaculty);
        }

        public bool isFacultyVerified(int fid)
        {
            bool isVerified = false;

            var faculty = db.jntuh_reinspection_registered_faculty.Find(fid);

            if (faculty.isApproved != null)
            {
                isVerified = true;
            }

            return isVerified;
        }

        //Edit Dialog Partial view
        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification")]
        public ActionResult FacultyVerificationEdit(string fid, string collegeid)
        {
            FacultyRegistration regFaculty = new FacultyRegistration();
            int fID = 0;

            if (fid != null)
            {
                regFaculty.GenderId = null;
                regFaculty.isFacultyRatifiedByJNTU = null;
                fID = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(fid, WebConfigurationManager.AppSettings["CryptoKey"]));
                ViewBag.FacultyID = fID;
                ViewBag.collegeid = collegeid;
                ViewBag.fid = fid;
                jntuh_reinspection_registered_faculty faculty = db.jntuh_reinspection_registered_faculty.Find(fID);

                regFaculty.id = fID;
                regFaculty.Type = faculty.type;
                regFaculty.RegistrationNumber = faculty.RegistrationNumber;
                regFaculty.UserName = db.my_aspnet_users.Where(u => u.id == faculty.UserId).Select(u => u.name).FirstOrDefault();
                regFaculty.Email = faculty.Email;
                regFaculty.UniqueID = faculty.UniqueID;
                regFaculty.FirstName = faculty.FirstName;
                regFaculty.MiddleName = faculty.MiddleName;
                regFaculty.LastName = faculty.LastName;
                regFaculty.FatherOrhusbandName = faculty.FatherOrHusbandName;
                regFaculty.MotherName = faculty.MotherName;
                regFaculty.GenderId = faculty.GenderId;
                if (faculty.DateOfBirth != null)
                {
                    regFaculty.facultyDateOfBirth = UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.DateOfBirth.ToString());
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
                if (faculty.collegeId != null)
                {
                    regFaculty.CollegeName = db.jntuh_college.Find(faculty.collegeId).collegeName;
                }
                regFaculty.CollegeId = faculty.collegeId;
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
                    regFaculty.facultyDateOfAppointment = UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.DateOfAppointment.ToString());
                }
                regFaculty.TotalExperiencePresentCollege = faculty.TotalExperiencePresentCollege;
                regFaculty.isFacultyRatifiedByJNTU = faculty.isFacultyRatifiedByJNTU;
                if (faculty.DateOfRatification != null)
                {
                    regFaculty.facultyDateOfRatification = UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.DateOfRatification.ToString());
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

                string registrationNumber = db.jntuh_reinspection_registered_faculty.Where(of => of.id == fID).Select(of => of.RegistrationNumber).FirstOrDefault();
                int facultyId = db.jntuh_college_faculty_registered.Where(of => of.RegistrationNumber == registrationNumber).Select(of => of.id).FirstOrDefault();
                int[] verificationOfficers = db.jntuh_college_faculty_verified.Where(v => v.FacultyId == facultyId).Select(v => v.VerificationOfficer).Distinct().ToArray();
                int userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
                var departments = db.jntuh_department.ToList();
                var specializatons = db.jntuh_specialization.ToList();
                int[] ugids = departments.Where(i => i.degreeId == 4 || i.degreeId == 5).Select(i => i.id).ToArray();
                int[] pgids = departments.Where(i => i.degreeId != 4 && i.degreeId != 5).Select(i => i.id).ToArray();
                List<DistinctDepartment> depts = new List<DistinctDepartment>();
                string existingDepts = string.Empty;
                int[] notRequiredIds = { 25, 26, 27, 33, 34, 36, 37, 38, 39, 53, 54, 55, 56 };
                foreach (var item in db.jntuh_department.Where(s => !notRequiredIds.Contains(s.id)).OrderBy(s => s.departmentName))
                {
                    if (!existingDepts.Split(',').Contains(item.departmentName))
                    {
                        depts.Add(new DistinctDepartment { id = item.id, departmentName = item.departmentName });
                        existingDepts = existingDepts + "," + item.departmentName;
                    }
                }

                ViewBag.department = depts;

                var ugcoures = specializatons.Where(i => ugids.Contains(i.departmentId)).ToList();
                var pgcoures = specializatons.Where(i => pgids.Contains(i.departmentId)).ToList();
                //var phdcoures = db.jntuh_specialization.Where(i => phdids.Contains(i.departmentId)).Select(i => i.specializationName).ToList();

                ViewBag.ugcourses = ugcoures;
                ViewBag.pgcourses = pgcoures;
                //ViewBag.phdcourses = phdcoures;
                ViewBag.FacultyDetails = regFaculty;
                TempData["FacultyDetails"] = regFaculty;
                ViewBag.HideVerifyLink = regFaculty.isApproved != null ? true : false;
            }
            return PartialView("_FacultyVerificationEdit", regFaculty);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification")]
        public ActionResult FacultyVerificationPostDENew(FacultyRegistration faculty)
        {
            var facultydetails = db.jntuh_reinspection_registered_faculty.FirstOrDefault(i => i.RegistrationNumber == faculty.RegistrationNumber);
            if (facultydetails != null)
            {
                facultydetails.Absent = faculty.Absent;
                if (faculty.ModifiedPANNo != null)
                {
                    facultydetails.PANNumber = faculty.ModifiedPANNo;
                    facultydetails.ModifiedPANNumber = faculty.ModifiedPANNo;
                }
                if (faculty.MOdifiedDateofAppointment1 != null)
                {
                    facultydetails.DateOfAppointment = Convert.ToDateTime(faculty.MOdifiedDateofAppointment1);
                }
                if (faculty.DepartmentId != null)
                {
                    facultydetails.DepartmentId = faculty.DepartmentId;
                }
                facultydetails.InvalidPANNumber = faculty.InvalidPANNo;
                facultydetails.NoRelevantUG = faculty.NORelevantUG;
                facultydetails.NoRelevantPG = faculty.NORelevantPG;
                facultydetails.NORelevantPHD = faculty.NORelevantPHD;
                facultydetails.NoSCM = faculty.NoSCM;
                facultydetails.NoForm16 = faculty.NOForm16;
                facultydetails.NotQualifiedAsperAICTE = faculty.NOTQualifiedAsPerAICTE;
                facultydetails.MultipleRegInSameCollege = faculty.MultipleReginSamecoll;
                facultydetails.MultipleRegInDiffCollege = faculty.MultipleReginDiffcoll;
                facultydetails.SamePANUsedByMultipleFaculty = faculty.SamePANUsedByMultipleFaculty;
                facultydetails.PhotoCopyofPAN = faculty.PhotocopyofPAN;
                facultydetails.AppliedPAN = faculty.AppliedPAN;
                facultydetails.LostPAN = faculty.LostPAN;
                facultydetails.OriginalsVerifiedUG = faculty.OriginalsVerifiedUG;
                facultydetails.OriginalsVerifiedPG = faculty.OriginalsVerifiedPG;
                facultydetails.OriginalsVerifiedPHD = faculty.OriginalsVerifiedPHD;
                facultydetails.IncompleteCertificates = faculty.InCompleteCeritificates;
                facultydetails.FacultyVerificationStatus = true;
                db.SaveChanges();
            }

            return RedirectToAction("Index", "ReInspectionFacultyVerification", new { collegeid = faculty.CollegeId });
        }

         //Reactive Faculty
        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification")]
        public ActionResult FacultyVerificationNoEdit(string fid, string collegeid)
        {
            int fID = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(fid, WebConfigurationManager.AppSettings["CryptoKey"]));
            var facultydetails = db.jntuh_reinspection_registered_faculty.Find(fID);
            if (facultydetails != null)
            {
                facultydetails.Absent = false;
                facultydetails.InvalidPANNumber = false;
                facultydetails.NoRelevantUG = string.Empty;
                facultydetails.NoRelevantPG = string.Empty;
                facultydetails.NORelevantPHD = string.Empty;
                facultydetails.NoSCM = false;
                facultydetails.NoForm16 = false;
                facultydetails.NotQualifiedAsperAICTE = false;
                facultydetails.MultipleRegInSameCollege = false;
                facultydetails.MultipleRegInDiffCollege = false;
                facultydetails.SamePANUsedByMultipleFaculty = false;
                facultydetails.PhotoCopyofPAN = false;
                facultydetails.AppliedPAN = false;
                facultydetails.LostPAN = false;
                facultydetails.OriginalsVerifiedUG = false;
                facultydetails.OriginalsVerifiedPG = false;
                facultydetails.OriginalsVerifiedPHD = false;
                facultydetails.FacultyVerificationStatus = false;
                facultydetails.IncompleteCertificates = false;
                db.SaveChanges();
            }
            return RedirectToAction("Index", "ReInspectionFacultyVerification", new { collegeid = collegeid });
        }

        //Check Details
        [Authorize(Roles = "Admin,SuperAdmin")]
        public ActionResult FacultyVerificationCheck(string fid, string collegeid)
        {
            FacultyRegistration regFaculty = new FacultyRegistration();
            int fID = 0;

            if (fid != null)
            {
                regFaculty.GenderId = null;
                regFaculty.isFacultyRatifiedByJNTU = null;
                fID = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(fid, WebConfigurationManager.AppSettings["CryptoKey"]));
                ViewBag.FacultyID = fID;
                ViewBag.collegeid = collegeid;
                ViewBag.fid = fid;
                jntuh_reinspection_registered_faculty faculty = db.jntuh_reinspection_registered_faculty.Find(fID);
                regFaculty.id = fID;
                regFaculty.Type = faculty.type;
                regFaculty.RegistrationNumber = faculty.RegistrationNumber;
                regFaculty.Email = faculty.Email;
                regFaculty.FirstName = faculty.FirstName;
                regFaculty.MiddleName = faculty.MiddleName;
                regFaculty.LastName = faculty.LastName;
                regFaculty.facultyPhoto = faculty.Photo;
                regFaculty.Absent = faculty.Absent != false ? true : false;
                regFaculty.InvalidPANNo = faculty.InvalidPANNumber != false ? true : false;
                regFaculty.NoSCM = faculty.NoSCM != false ? true : false;
                regFaculty.NORelevantPG = faculty.NoRelevantPG;
                regFaculty.NOForm16 = faculty.NoForm16 != false ? true : false;
                regFaculty.NOTQualifiedAsPerAICTE = faculty.NotQualifiedAsperAICTE != false ? true : false;
                regFaculty.ModifiedPANNo = faculty.ModifiedPANNumber;
                regFaculty.InCompleteCeritificates = faculty.IncompleteCertificates != false ? true : false;
                regFaculty.DeactivationReason = faculty.DeactivationReason;
                regFaculty.PanDeactivationReasion = faculty.PanDeactivationReason;
                regFaculty.PanVerificationStatus = faculty.PanVerificationStatus;
            }
            return PartialView("_FacultyVerificationCheck", regFaculty);
        }

         //Re_check Status  Details

        [Authorize(Roles = "Admin,SuperAdmin")]
        public ActionResult FacultyVerificationStatusCheck(string fid, string collegeid)
        {
            FacultyRegistration regFaculty = new FacultyRegistration();
            int fID = 0;

            if (fid != null)
            {
                regFaculty.GenderId = null;
                regFaculty.isFacultyRatifiedByJNTU = null;
                fID = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(fid, WebConfigurationManager.AppSettings["CryptoKey"]));
                ViewBag.FacultyID = fID;
                ViewBag.collegeid = collegeid;
                ViewBag.fid = fid;
                jntuh_reinspection_registered_faculty faculty = db.jntuh_reinspection_registered_faculty.Find(fID);
                regFaculty.id = fID;
                regFaculty.Type = faculty.type;
                regFaculty.RegistrationNumber = faculty.RegistrationNumber;
                regFaculty.Email = faculty.Email;
                regFaculty.FirstName = faculty.FirstName;
                regFaculty.MiddleName = faculty.MiddleName;
                regFaculty.LastName = faculty.LastName;
                regFaculty.facultyPhoto = faculty.Photo;
                regFaculty.Absent = faculty.Absent != false ? true : false;
                regFaculty.InvalidPANNo = faculty.InvalidPANNumber != false ? true : false;
                regFaculty.NoSCM = faculty.NoSCM != false ? true : false;
                regFaculty.NORelevantPG = faculty.NoRelevantPG;
                regFaculty.NOForm16 = faculty.NoForm16 != false ? true : false;
                regFaculty.NOTQualifiedAsPerAICTE = faculty.NotQualifiedAsperAICTE != false ? true : false;
                regFaculty.ModifiedPANNo = faculty.ModifiedPANNumber;
                regFaculty.InCompleteCeritificates = faculty.IncompleteCertificates != false ? true : false;
                regFaculty.DeactivationReason = faculty.DeactivationReason;
                regFaculty.PanDeactivationReasion = faculty.PanDeactivationReason;
                regFaculty.PanVerificationStatus = faculty.PanVerificationStatus;
            }
            return PartialView("_FacultyVerificationStatusCheck", regFaculty);
        }

         //Faculty Verification
        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification")]
        public ActionResult FacultyVerification(string fid)
        {

            FacultyRegistration regFaculty = new FacultyRegistration();
            int fID = 0;

            if (fid != null)
            {
                regFaculty.GenderId = null;
                regFaculty.isFacultyRatifiedByJNTU = null;
                fID = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(fid, WebConfigurationManager.AppSettings["CryptoKey"]));
                ViewBag.FacultyID = fID;
                jntuh_registered_faculty faculty = db.jntuh_registered_faculty.Find(fID);

                regFaculty.id = fID;
                regFaculty.Type = faculty.type;
                regFaculty.RegistrationNumber = faculty.RegistrationNumber;
                regFaculty.UserName = db.my_aspnet_users.Where(u => u.id == faculty.UserId).Select(u => u.name).FirstOrDefault();
                regFaculty.Email = faculty.Email;
                regFaculty.UniqueID = faculty.UniqueID;
                regFaculty.FirstName = faculty.FirstName;
                regFaculty.MiddleName = faculty.MiddleName;
                regFaculty.LastName = faculty.LastName;
                regFaculty.FatherOrhusbandName = faculty.FatherOrHusbandName;
                regFaculty.MotherName = faculty.MotherName;
                regFaculty.GenderId = faculty.GenderId;
                if (faculty.DateOfBirth != null)
                {
                    regFaculty.facultyDateOfBirth = UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.DateOfBirth.ToString());
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
                if (faculty.collegeId != null)
                {
                    regFaculty.CollegeName = db.jntuh_college.Find(faculty.collegeId).collegeName;
                }
                regFaculty.CollegeId = faculty.collegeId;
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
                    regFaculty.facultyDateOfAppointment = UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.DateOfAppointment.ToString());
                }
                regFaculty.TotalExperiencePresentCollege = faculty.TotalExperiencePresentCollege;
                regFaculty.isFacultyRatifiedByJNTU = faculty.isFacultyRatifiedByJNTU;
                if (faculty.DateOfRatification != null)
                {
                    regFaculty.facultyDateOfRatification = UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.DateOfRatification.ToString());
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

                string registrationNumber = db.jntuh_registered_faculty.Where(of => of.id == fID).Select(of => of.RegistrationNumber).FirstOrDefault();
                int facultyId = db.jntuh_college_faculty_registered.Where(of => of.RegistrationNumber == registrationNumber).Select(of => of.id).FirstOrDefault();
                int[] verificationOfficers = db.jntuh_college_faculty_verified.Where(v => v.FacultyId == facultyId).Select(v => v.VerificationOfficer).Distinct().ToArray();
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
                ViewBag.FacultyDetails = regFaculty;
                TempData["FacultyDetails"] = regFaculty;
                ViewBag.HideVerifyLink = regFaculty.isApproved != null ? true : false;
            }

            return View(regFaculty);
        }

         //Lab Deficiency Screen for Reinspection
        [Authorize(Roles = "Admin,DataEntry,FacultyVerification")]
        [HttpGet]
        public ActionResult AllCollegesLabsDeficiency(int? rid)
        {
            List<Lab> lstlaboratories = new List<Lab>();
            int[] collegeIds = new int[] { 48, 62, 125, 140, 143, 157, 161, 162, 252, 292, 305, 370, 371, 401, 415, 416, 422, 424, 447 };
            var colleges = db.jntuh_college_randamcodes.Where(c => c.IsActive == true&&collegeIds.Contains(c.CollegeId)).Select(c => new
            {
                rid = c.Id,
                RandamCode = c.RandamCode
            }).OrderBy(c => c.RandamCode).ToList();
            ViewBag.Colleges = colleges;
            //colleges.Add(new { collegeId = 0, collegeName = "00-ALL Colleges" });
            if (rid != null)
            {
                int cid = db.jntuh_college_randamcodes.Find(rid).CollegeId;
                ViewBag.display = true;
                int PGEquipmentCount = Convert.ToInt32(WebConfigurationManager.AppSettings["PGEquipmentCount"]);

                int[] collegeIDs = db.jntuh_college.Where(c => c.isActive == true && c.id == cid).Select(c => c.id).Take(1).ToArray();
                var jntuh_reinspection_college_laboratories_deficiency = db.jntuh_reinspection_college_laboratories_deficiency.Where(c => c.CollegeId == cid).ToList();
                string CollegeAffiliationStatus = db.jntuh_college_affiliation.Where(u => u.collegeId == cid && u.affiliationTypeId == 7).Select(u => u.affiliationStatus).FirstOrDefault();
                List<Lab> collegeLabMaster = null;
                foreach (var collegeId in collegeIDs)
                {
                    string strcollegecode = db.jntuh_college_randamcodes.Where(r => r.CollegeId == collegeId).Select(r => r.RandamCode).FirstOrDefault();
                    int[] specializationIds = db.jntuh_college_intake_existing.Where(e => e.collegeId == collegeId).Select(e => e.specializationId).Distinct().ToArray();
                    int[] DegreeIDs = db.jntuh_lab_master.AsNoTracking().Where(l => l.DegreeID == 4 && specializationIds.Contains(l.SpecializationID)).Select(l => l.DegreeID).ToArray();
                    if (CollegeAffiliationStatus == "Yes")
                    {

                        if (DegreeIDs.Contains(4))
                        {
                            collegeLabMaster = db.jntuh_lab_master.AsNoTracking()
                                                             .Where(l => specializationIds.Contains(l.SpecializationID) || l.SpecializationID == 39)
                                                             .Select(l => new Lab
                                                             {
                                                                 ////// EquipmentID=l.id,                                                               
                                                                 degreeId = l.DegreeID,
                                                                 degree = l.jntuh_degree.degree,
                                                                 degreeDisplayOrder = l.jntuh_degree.degreeDisplayOrder,
                                                                 departmentId = l.DepartmentID,
                                                                 department = l.jntuh_department.departmentName,
                                                                 specializationId = l.SpecializationID,
                                                                 specializationName = l.jntuh_specialization.specializationName,
                                                                 year = l.Year,
                                                                 Semester = l.Semester,
                                                                 Labcode = l.Labcode,
                                                                 //////LabName = l.LabName
                                                             })
                                                             .OrderBy(l => l.degreeDisplayOrder)
                                                             .ThenBy(l => l.department)
                                                             .ThenBy(l => l.specializationName)
                                                             .ThenBy(l => l.year).ThenBy(l => l.Semester).Distinct()
                                                             .ToList();
                        }
                        else
                        {
                            collegeLabMaster = db.jntuh_lab_master.AsNoTracking()
                                                             .Where(l => specializationIds.Contains(l.SpecializationID))
                                                             .Select(l => new Lab
                                                             {
                                                                 ////// EquipmentID=l.id,                                                               
                                                                 degreeId = l.DegreeID,
                                                                 degree = l.jntuh_degree.degree,
                                                                 degreeDisplayOrder = l.jntuh_degree.degreeDisplayOrder,
                                                                 departmentId = l.DepartmentID,
                                                                 department = l.jntuh_department.departmentName,
                                                                 specializationId = l.SpecializationID,
                                                                 specializationName = l.jntuh_specialization.specializationName,
                                                                 year = l.Year,
                                                                 Semester = l.Semester,
                                                                 Labcode = l.Labcode,
                                                                 //////LabName = l.LabName
                                                             })
                                                             .OrderBy(l => l.degreeDisplayOrder)
                                                             .ThenBy(l => l.department)
                                                             .ThenBy(l => l.specializationName)
                                                             .ThenBy(l => l.year).ThenBy(l => l.Semester).Distinct()
                                                             .ToList();
                        }


                    }
                    else if (CollegeAffiliationStatus == "No" || CollegeAffiliationStatus == null)
                    {

                        if (DegreeIDs.Contains(4))
                        {
                            collegeLabMaster = db.jntuh_lab_master.AsNoTracking()
                                                              .Where(l => (l.SpecializationID == 39 || specializationIds.Contains(l.SpecializationID)) && l.Labcode != "TMP-CL")
                                                              .Select(l => new Lab
                                                              {
                                                                  ////// EquipmentID=l.id,                                                               
                                                                  degreeId = l.DegreeID,
                                                                  degree = l.jntuh_degree.degree,
                                                                  degreeDisplayOrder = l.jntuh_degree.degreeDisplayOrder,
                                                                  departmentId = l.DepartmentID,
                                                                  department = l.jntuh_department.departmentName,
                                                                  specializationId = l.SpecializationID,
                                                                  specializationName = l.jntuh_specialization.specializationName,
                                                                  year = l.Year,
                                                                  Semester = l.Semester,
                                                                  Labcode = l.Labcode,
                                                                  LabName = l.LabName
                                                              })
                                                              .OrderBy(l => l.degreeDisplayOrder)
                                                              .ThenBy(l => l.department)
                                                              .ThenBy(l => l.specializationName)
                                                              .ThenBy(l => l.year).ThenBy(l => l.Semester).Distinct()
                                                              .ToList();
                        }
                        else
                        {

                            collegeLabMaster = db.jntuh_lab_master.AsNoTracking()
                                                              .Where(l => specializationIds.Contains(l.SpecializationID) && l.Labcode != "TMP-CL")
                                                              .Select(l => new Lab
                                                              {
                                                                  ////// EquipmentID=l.id,                                                               
                                                                  degreeId = l.DegreeID,
                                                                  degree = l.jntuh_degree.degree,
                                                                  degreeDisplayOrder = l.jntuh_degree.degreeDisplayOrder,
                                                                  departmentId = l.DepartmentID,
                                                                  department = l.jntuh_department.departmentName,
                                                                  specializationId = l.SpecializationID,
                                                                  specializationName = l.jntuh_specialization.specializationName,
                                                                  year = l.Year,
                                                                  Semester = l.Semester,
                                                                  Labcode = l.Labcode,
                                                                  LabName = l.LabName
                                                              })
                                                              .OrderBy(l => l.degreeDisplayOrder)
                                                              .ThenBy(l => l.department)
                                                              .ThenBy(l => l.specializationName)
                                                              .ThenBy(l => l.year).ThenBy(l => l.Semester).Distinct()
                                                              .ToList();
                        }


                    }


                    foreach (var item in collegeLabMaster)
                    {
                        Lab lstlabs = new Lab();
                        lstlabs.collegeId = collegeId;
                        lstlabs.EquipmentID = item.EquipmentID;
                        lstlabs.degree = item.degree;
                        lstlabs.department = item.department;
                        lstlabs.specializationName = item.specializationName;
                        lstlabs.specializationId = item.specializationId;
                        lstlabs.Semester = item.Semester;
                        lstlabs.year = item.year;
                        lstlabs.Labcode = item.Labcode;
                        lstlabs.RandomId = (int)rid;
                        //////lstlabs.LabName = item.LabName;
                        lstlabs.EquipmentNo = 1;
                        lstlabs.RandomCode = strcollegecode;
                        lstlabs.degreeDisplayOrder = item.degreeDisplayOrder;
                        //lstlabs.id = jntuh_college_laboratories_deficiency.Where(ld => ld.LabCode == item.Labcode && ld.CollegeId == collegeId).Select(ld => ld.Id).FirstOrDefault();
                        //if (lstlabs.id != 0)
                        //{
                        //    lstlabs.deficiency = jntuh_college_laboratories_deficiency.Where(ld => ld.LabCode == item.Labcode && ld.CollegeId == collegeId).Select(ld => ld.Deficiency).FirstOrDefault();
                        //    //lstlabs.id = jntuh_college_laboratories_deficiency.Where(ld => ld.LabCode == item.Labcode && ld.CollegeId == collegeId).Select(ld => ld.Id).FirstOrDefault();
                        //}
                        lstlabs.id = jntuh_reinspection_college_laboratories_deficiency.Where(ld => ld.LabCode == item.Labcode && ld.CollegeId == collegeId && ld.Year == item.year && ld.Semister == item.Semester && ld.SpecializationId == item.specializationId).Select(ld => ld.Id).FirstOrDefault();
                        lstlabs.deficiencyStatus = jntuh_reinspection_college_laboratories_deficiency.Where(ld => ld.LabCode == item.Labcode && ld.CollegeId == collegeId && ld.Year == item.year && ld.Semister == item.Semester && ld.SpecializationId == item.specializationId).Select(ld => ld.DeficiencyStatus).FirstOrDefault();
                        if (lstlabs.id != 0)
                        {
                            lstlabs.deficiency = jntuh_reinspection_college_laboratories_deficiency.Where(ld => ld.LabCode == item.Labcode && ld.CollegeId == collegeId && ld.Year == item.year && ld.Semister == item.Semester && ld.SpecializationId == item.specializationId).Select(ld => ld.Deficiency).FirstOrDefault();
                        }
                        else
                        {
                            lstlabs.deficiency = null;
                            lstlabs.id = 0;
                        }
                        lstlaboratories.Add(lstlabs);
                    }
                }
                lstlaboratories = lstlaboratories.OrderBy(l => l.degreeDisplayOrder)
                                                                .ThenBy(l => l.department)
                                                                .ThenBy(l => l.specializationName)
                                                                .ThenBy(l => l.year).ThenBy(l => l.Semester).Distinct()
                                                                .ToList();
            }
            else
            {
                ViewBag.display = false;
            }

            return View(lstlaboratories);
        }

        [Authorize(Roles = "Admin,DataEntry,FacultyVerification")]
        [HttpPost]
        public ActionResult AllCollegesLabsDeficiency(List<Lab> labs)
        {
            int RandomId = labs.Select(c => c.RandomId).FirstOrDefault();
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            // var labsItems = labs.Where(ld => ld.deficiency != null).ToList();
            var labsItems = labs.ToList();
            if (labsItems.Count() > 0)
            {

                foreach (var item in labsItems)
                {
                    jntuh_reinspection_college_laboratories_deficiency labsDeficiency = new jntuh_reinspection_college_laboratories_deficiency();
                    labsDeficiency.CollegeId = item.collegeId;
                    labsDeficiency.LabCode = item.Labcode ?? string.Empty;
                    labsDeficiency.IsActive = true;
                    labsDeficiency.SpecializationId = item.specializationId;
                    labsDeficiency.Year = item.year;
                    labsDeficiency.Semister = (int)item.Semester;

                    if (item.deficiency == null)
                    {
                        labsDeficiency.Deficiency = true;
                        labsDeficiency.DeficiencyStatus = true;

                    }
                    else
                    {
                        labsDeficiency.Deficiency = (bool)item.deficiency;
                        labsDeficiency.DeficiencyStatus = false;
                    }
                    if (item.id == 0)
                    {
                        labsDeficiency.CreatedBy = userID;
                        labsDeficiency.CreatedOn = DateTime.Now;
                        db.jntuh_reinspection_college_laboratories_deficiency.Add(labsDeficiency);
                        try
                        {
                            db.SaveChanges();
                        }
                        catch (DbEntityValidationException dbEx)
                        {
                            foreach (var validationErrors in dbEx.EntityValidationErrors)
                            {
                                foreach (var validationError in validationErrors.ValidationErrors)
                                {
                                    Trace.TraceInformation("Property: {0} Error: {1}",
                                                            validationError.PropertyName,
                                                            validationError.ErrorMessage);
                                }
                            }
                        }
                    }
                    else
                    {
                        jntuh_reinspection_college_laboratories_deficiency labsDeficiencyupdate = db.jntuh_reinspection_college_laboratories_deficiency.Find(item.id);
                        labsDeficiencyupdate.LabCode = item.Labcode ?? string.Empty;
                        labsDeficiencyupdate.Deficiency = (bool)item.deficiency;
                        if (item.deficiency == false)
                            labsDeficiencyupdate.DeficiencyStatus = false;

                        labsDeficiencyupdate.UpdatedBy = userID;
                        labsDeficiencyupdate.UpdatedOn = DateTime.Now;
                        db.Entry(labsDeficiencyupdate).State = EntityState.Modified;
                        try
                        {
                            db.SaveChanges();
                        }
                        catch (DbEntityValidationException dbEx)
                        {
                            foreach (var validationErrors in dbEx.EntityValidationErrors)
                            {
                                foreach (var validationError in validationErrors.ValidationErrors)
                                {
                                    Trace.TraceInformation("Property: {0} Error: {1}",
                                                            validationError.PropertyName,
                                                            validationError.ErrorMessage);
                                }
                            }
                        }
                    }
                }

                TempData["Success"] = "Data Saved";


                //TempData["Error"] = "Invalid Data";

            }
            return RedirectToAction("AllCollegesLabsDeficiency", "ReInspectionFacultyVerification", new { rid = RandomId });
        }



    }
}
