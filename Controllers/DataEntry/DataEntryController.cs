using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using UAAAS.Models;
using System.Data;
using System.Web.Security;
using System.IO;
using System.Drawing;
using System.Drawing.Text;
using System.Drawing.Drawing2D;
using System.Web.Configuration;

namespace UAAAS.Controllers
{
    [ErrorHandling]
    public class DataEntryController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();

        public ActionResult Index()
        {
            //return RedirectToAction("DataEntryAssignedColleges", "DataEntryAssignedColleges");
            return View();
        }
        [Authorize(Roles = "Admin,Operations")]
        public ActionResult DataEntryColleges(int? PhaseId)
        {
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy =
                db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            //get old inspection phases
            ViewBag.oldInspectionPhases = (from s in db.jntuh_ffc_schedule
                                           join p in db.jntuh_inspection_phase on s.InspectionPhaseId equals p.id
                                           join a in db.jntuh_academic_year on p.academicYearId equals a.id
                                           where p.isActive == true
                                           select new OldInspectionPhaseIds { name = a.academicYear + " (" + p.inspectionPhase + ")", id = (int)s.InspectionPhaseId }).Distinct().ToList();

            //int InspectionPhaseId = 0;
            //if (PhaseId == null)
            //    InspectionPhaseId = db.jntuh_inspection_phase.Where(p => p.isActive == true).Select(p => p.id).SingleOrDefault();
            //else
            //    InspectionPhaseId = (int)PhaseId;
            int[] InspectionPhaseIds =
                db.jntuh_inspection_phase.Where(
                    a => a.academicYearId == prAy && a.isActive == true)
                    .Select(s => s.id)
                    .ToArray();
            List<DataEntryColleges> dataEntryCollegesList = (from dcl in db.jntuh_dataentry_allotment
                                                             join u in db.my_aspnet_users on dcl.userID equals u.id
                                                             join c in db.jntuh_college on dcl.collegeID equals c.id
                                                             join p in db.jntuh_inspection_phase on dcl.InspectionPhaseId equals p.id
                                                             where (c.isActive == true && dcl.isActive == true)
                                                             select new DataEntryColleges
                                                             {
                                                                 id = dcl.id,
                                                                 userId = dcl.userID,
                                                                 collegeId = dcl.collegeID,
                                                                 isActive = dcl.isActive,
                                                                 isCompleted = dcl.isCompleted,
                                                                 isVerified = dcl.isVerified,
                                                                 userName = u.name,
                                                                 phasename = p.inspectionPhase,
                                                                 collegeName = c.collegeName,
                                                                 collegeCode = c.collegeCode,
                                                                 createdBy = dcl.createdBy,
                                                                 createdon = dcl.createdOn
                                                             }).ToList();

            ViewBag.DataEntryList = dataEntryCollegesList;
            return View("~/Views/DataEntry/DataEntryColleges.cshtml", dataEntryCollegesList);
        }
        [AcceptVerbs(HttpVerbs.Get)]
        [Authorize(Roles = "Admin,Operations")]
        public JsonResult GetUsers()
        {
            var usersList = (from u in db.my_aspnet_users
                             join ur in db.my_aspnet_usersinroles on u.id equals ur.userId
                             join r in db.my_aspnet_roles on ur.roleId equals r.id
                             join ra in db.my_aspnet_membership on ur.id equals ra.userId
                             where r.name == "DataEntry" && ra.IsLockedOut == false
                             select new
                             {
                                 u.id,
                                 u.name
                             }).ToList();
            var usersData = usersList.Select(a => new SelectListItem()
            {
                Text = a.name,
                Value = a.id.ToString(),
            });
            return Json(usersData.OrderBy(a => a.Value), JsonRequestBehavior.AllowGet);
        }
        [AcceptVerbs(HttpVerbs.Get)]
        [Authorize(Roles = "Admin,Operations")]
        public JsonResult GetColleges()
        {
            int AcademicYear = db.jntuh_academic_year.Where(a => a.isPresentAcademicYear == true).Select(a => a.id).FirstOrDefault();
            //int inspectionPhaseId = db.jntuh_inspection_phase.Where(p => p.isActive == true).Select(p => p.id).SingleOrDefault();
            ////Not in sub Query            
            //var collegesList = (from c in db.jntuh_college
            //                    //join cs in db.jntuh_dataentry_allotment on c.id equals cs.collegeID
            //                    let dcs = from dc in db.jntuh_dataentry_allotment
            //                              where dc.isActive == true
            //                              select dc.collegeID
            //                    where c.isActive == true && !dcs.Contains(c.id)
            //                    select new
            //                    {
            //                        c.id,
            //                        c.collegeCode,
            //                        c.collegeName
            //                    }).OrderBy(c => c.collegeCode).ToList();
            //var collegesList = db.jntuh_college.Where(c => c.isActive == true).Select(s =>s).ToList();
            var collegesList =
                db.jntuh_college.Join(db.jntuh_college_edit_status, co => co.id, e => e.collegeId,
                    (co, e) => new { co = co, e = e })
                    .Where(c => c.e.IsCollegeEditable == false && c.e.academicyearId == AcademicYear + 1)
                    .Select(c => new { id = c.co.id, collegeCode = c.co.collegeCode, collegeName = c.co.collegeName })
                    .OrderBy(c => c.collegeName)
                    .ToList();
            var collegesData = collegesList.Select(a => new SelectListItem()
            {
                Text = a.collegeCode + "-" + a.collegeName,
                Value = a.id.ToString(),
            });
            return Json(collegesData.OrderBy(a => a.Text), JsonRequestBehavior.AllowGet);
        }
        [Authorize(Roles = "Admin,Operations")]
        public JsonResult GetPhases()
        {
            var phaseslist = db.jntuh_inspection_phase.Where(r => r.isActive == true).ToList();
            var phases = phaseslist.Select(a => new SelectListItem()
            {
                Text = a.inspectionPhase,
                Value = a.id.ToString(),
            });
            return Json(phases.OrderBy(a => a.Text), JsonRequestBehavior.AllowGet);
        }
        //[Authorize(Roles = "Admin")]
        [HttpGet]
        [Authorize(Roles = "Admin,Operations")]
        public ActionResult DataEntryCreateColleges(int? id)
        {
            if (id != null)
            {
                ViewBag.IsUpdate = true;
                jntuh_dataentry_allotment dataEntry = db.jntuh_dataentry_allotment.Where(d => d.id == id).Select(d => d).FirstOrDefault();
                DataEntryColleges dataEntryCollegesList = new DataEntryColleges();
                if (dataEntry != null)
                {
                    dataEntryCollegesList.userId = dataEntry.userID;
                    dataEntryCollegesList.collegeId = dataEntry.collegeID;
                    dataEntryCollegesList.isActive = dataEntry.isActive;
                    dataEntryCollegesList.isCompleted = dataEntry.isCompleted;
                    dataEntryCollegesList.isVerified = dataEntry.isVerified;
                    dataEntryCollegesList.createdBy = dataEntry.createdBy;
                    dataEntryCollegesList.createdon = dataEntry.createdOn;
                    dataEntryCollegesList.InspectionPhaseId = dataEntry.InspectionPhaseId;
                    List<jntuh_college> Colleges = db.jntuh_college.Where(c => c.isActive == true).ToList();
                    foreach (var item in Colleges)
                    {
                        if (item.collegeCode != null || item.collegeName != null)
                        {
                            item.collegeName = item.collegeCode + "-" + item.collegeName;
                        }
                    }
                    ViewBag.colleges = Colleges.ToList();

                    ViewBag.inspectionphase = db.jntuh_inspection_phase.Where(r => r.isActive == true).Select(s => s).FirstOrDefault();

                    ViewBag.users = (from u in db.my_aspnet_users
                                     join ur in db.my_aspnet_usersinroles on u.id equals ur.userId
                                     join r in db.my_aspnet_roles on ur.roleId equals r.id
                                     where r.name == "DataEntry"
                                     select new
                                     {
                                         u.id,
                                         u.name
                                     }).ToList();
                }
                return PartialView("~/Views/DataEntry/DataEntryCreateColleges.cshtml", dataEntryCollegesList);
            }
            else
            {
                DataEntryColleges dataEntryCollegesList = new DataEntryColleges();
                return PartialView("~/Views/DataEntry/DataEntryCreateColleges.cshtml", dataEntryCollegesList);
            }
        }
        //[Authorize(Roles = "Admin")]
        [HttpPost]
        [Authorize(Roles = "Admin,Operations")]
        public ActionResult DataEntryCreateColleges(DataEntryColleges dataEntryCollegesList, string cmd)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            if (cmd == "Update")
            {
                if (ModelState.IsValid == true)
                {

                    int activecollegeCount = db.jntuh_dataentry_allotment.Where(d => d.collegeID == dataEntryCollegesList.collegeId && d.isActive == true && d.InspectionPhaseId == dataEntryCollegesList.InspectionPhaseId).Select(d => d.collegeID).Count();
                    int inativecollegeCount = db.jntuh_dataentry_allotment.Where(d => d.collegeID == dataEntryCollegesList.collegeId && d.isActive == false && d.InspectionPhaseId == dataEntryCollegesList.InspectionPhaseId).Select(d => d.collegeID).Count();
                    if (activecollegeCount != 0 && inativecollegeCount != 0)
                    {
                        if (inativecollegeCount != 0)
                        {
                            var activestatus = db.jntuh_dataentry_allotment.Where(d => d.collegeID == dataEntryCollegesList.collegeId && d.id == dataEntryCollegesList.id && d.InspectionPhaseId == dataEntryCollegesList.InspectionPhaseId).Select(d => d.isActive).FirstOrDefault();
                            if (activestatus == false)
                            {
                                TempData["Error"] = "This college already assigned anthor user you can not update.";
                            }
                            else
                            {
                                jntuh_dataentry_allotment dataEntry = new jntuh_dataentry_allotment();
                                dataEntry.id = dataEntryCollegesList.id;
                                dataEntry.userID = dataEntryCollegesList.userId;
                                dataEntry.collegeID = dataEntryCollegesList.collegeId;
                                dataEntry.isActive = dataEntryCollegesList.isActive;
                                dataEntry.isCompleted = dataEntryCollegesList.isCompleted;
                                dataEntry.isVerified = dataEntryCollegesList.isVerified;
                                dataEntry.createdBy = dataEntryCollegesList.createdBy;
                                dataEntry.createdOn = dataEntryCollegesList.createdon;
                                dataEntry.updatedBy = userID;
                                dataEntry.updatedOn = DateTime.Now;
                                dataEntry.InspectionPhaseId = dataEntryCollegesList.InspectionPhaseId;
                                db.Entry(dataEntry).State = EntityState.Modified;
                                db.SaveChanges();
                                TempData["Success"] = "Data Updated successfully.";
                            }
                        }
                    }
                    else
                    {

                        jntuh_dataentry_allotment dataEntry = new jntuh_dataentry_allotment();
                        dataEntry.id = dataEntryCollegesList.id;
                        dataEntry.userID = dataEntryCollegesList.userId;
                        dataEntry.collegeID = dataEntryCollegesList.collegeId;
                        dataEntry.isActive = dataEntryCollegesList.isActive;
                        dataEntry.isCompleted = dataEntryCollegesList.isCompleted;
                        dataEntry.isVerified = dataEntryCollegesList.isVerified;
                        dataEntry.createdBy = dataEntryCollegesList.createdBy;
                        dataEntry.createdOn = dataEntryCollegesList.createdon;
                        dataEntry.updatedBy = userID;
                        dataEntry.updatedOn = DateTime.Now;
                        dataEntry.InspectionPhaseId = dataEntryCollegesList.InspectionPhaseId;
                        db.Entry(dataEntry).State = EntityState.Modified;
                        db.SaveChanges();
                        TempData["Success"] = "Data Updated successfully.";

                    }
                    return RedirectToAction("DataEntryColleges");
                }
            }
            else
            {
                if (ModelState.IsValid == true)
                {
                    //int InspectionPhaseId = db.jntuh_inspection_phase.Where(p => p.isActive == true).Select(p => p.id).SingleOrDefault();

                    int committeeId = db.jntuh_dataentry_allotment.Where(c => c.collegeID == dataEntryCollegesList.collegeId && c.isActive == true && c.InspectionPhaseId == dataEntryCollegesList.InspectionPhaseId).Select(c => c.id).FirstOrDefault();
                    if (committeeId == 0)
                    {
                        jntuh_dataentry_allotment dataEntry = new jntuh_dataentry_allotment();
                        dataEntry.userID = dataEntryCollegesList.userId;
                        dataEntry.collegeID = dataEntryCollegesList.collegeId;
                        dataEntry.isActive = true;
                        dataEntry.isCompleted = false;
                        dataEntry.isVerified = false;
                        dataEntry.createdBy = userID;
                        dataEntry.createdOn = DateTime.Now;
                        dataEntry.InspectionPhaseId = dataEntryCollegesList.InspectionPhaseId;
                        db.jntuh_dataentry_allotment.Add(dataEntry);
                        db.SaveChanges();
                        TempData["Success"] = "Data Added successfully";
                        return RedirectToAction("DataEntryColleges");
                    }
                    else
                    {
                        TempData["Error"] = "College alredy  exist";
                        return RedirectToAction("DataEntryColleges");
                    }
                }

            }
            return View();
        }

        [Authorize(Roles = "Admin,Operations")]
        public ActionResult DataEntryDeleteColleges(int? id)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            DataEntryColleges dataEntryCollegesList = new DataEntryColleges();
            if (ModelState.IsValid == true)
            {
                jntuh_dataentry_allotment dataEntry = db.jntuh_dataentry_allotment.Where(d => d.id == id).Select(d => d).FirstOrDefault();
                dataEntry.id = dataEntry.id;
                dataEntry.userID = dataEntry.userID;
                dataEntry.collegeID = dataEntry.collegeID;
                dataEntry.isActive = false;
                dataEntry.isCompleted = dataEntry.isCompleted;
                dataEntry.isVerified = dataEntry.isVerified;
                dataEntry.createdBy = dataEntry.createdBy;
                dataEntry.createdOn = dataEntry.createdOn;
                dataEntry.updatedBy = userID;
                dataEntry.updatedOn = DateTime.Now;
                dataEntry.InspectionPhaseId = dataEntry.InspectionPhaseId;
                db.Entry(dataEntry).State = EntityState.Modified;
                db.SaveChanges();
                TempData["Success"] = "Data Deleted successfully.";

            }
            return RedirectToAction("DataEntryColleges");
        }


        //Faculty Data Entry Screens Written by Narayana Reddy on 12-03-2019
        [Authorize(Roles = "Admin,SuperAdmin,DataEntry")]
        public ActionResult FacultyDataEntry(int? collegeid)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            List<int> userRoles = db.my_aspnet_usersinroles.Where(u => u.userId == userID).Select(u => u.roleId).ToList();
            if (userRoles.Contains(
                                        db.my_aspnet_roles.Where(r => r.name.Equals("Admin"))
                                            .Select(r => r.id)
                                            .FirstOrDefault()))
            {
                ViewBag.Colleges =
                   db.jntuh_college.Join(db.jntuh_college_edit_status, co => co.id, e => e.collegeId,
                       (co, e) => new { co = co, e = e })
                       .Where(c => c.e.IsCollegeEditable == false)
                       .Select(c => new { collegeId = c.co.id, collegeName = c.co.collegeCode + "-" + c.co.collegeName })
                       .GroupBy(c => new { c.collegeId, c.collegeName }).Select(i => new { collegeId = i.Key.collegeId, collegeName = i.Key.collegeName })
                       .OrderBy(c => c.collegeName)
                       .ToList();
            }
            else
            {
                int AcademicYear = db.jntuh_academic_year.Where(a => a.isPresentAcademicYear == true).Select(a => a.id).FirstOrDefault();
                int InspectionPhaseId = db.jntuh_inspection_phase.Where(p => p.isActive == true && p.inspectionPhase == "Data Entry").Select(p => p.id).SingleOrDefault();
                int[] assignedcollegeslist =
                    db.jntuh_dataentry_allotment.Where(
                        d =>
                            d.InspectionPhaseId == InspectionPhaseId && d.userID == userID && d.isActive == true &&
                            d.isCompleted == false).Select(s => s.collegeID).ToArray();
                ViewBag.Colleges =
                    db.jntuh_college.Join(db.jntuh_college_edit_status, co => co.id, e => e.collegeId,
                        (co, e) => new { co = co, e = e })
                        .Where(c => c.e.IsCollegeEditable == false && c.e.academicyearId == AcademicYear + 1 && assignedcollegeslist.Contains(c.e.collegeId))
                        .Select(c => new { collegeId = c.co.id, collegeName = c.co.collegeCode + "-" + c.co.collegeName })
                        .GroupBy(c => new { c.collegeId, c.collegeName }).Select(i => new { collegeId = i.Key.collegeId, collegeName = i.Key.collegeName })
                        .OrderBy(c => c.collegeName)
                        .ToList();

            }
            ViewBag.collegeid = collegeid;
            var jntuhDepartment = db.jntuh_department.ToList();
            List<FacultyRegistration> teachingFaculty = new List<FacultyRegistration>();

            if (collegeid != null)
            {
                //string[] notshowregistation = { "71150401-160927", "45150402-123537", "26150403-103955" };
                //&&!notshowregistation.Contains(cf.RegistrationNumber)
                //string[] notshowregistation =
                //    db.jntuh_college_faculty_registered_copy.Where(fr => fr.collegeId == collegeid)
                //        .Select(s => s.RegistrationNumber)
                //        .ToArray();
                var jntuhDegree = db.jntuh_degree.ToList();
                var jntuhSpecialization = db.jntuh_specialization.ToList();
                List<jntuh_college_faculty_registered> jntuh_college_faculty_registered =
                    db.jntuh_college_faculty_registered.Where(cf => cf.collegeId == collegeid).Select(cf => cf).ToList();
                string[] strRegNoS = jntuh_college_faculty_registered.Select(cf => cf.RegistrationNumber).ToArray();
                string[] PrincipalstrRegNoS =
                    db.jntuh_college_principal_registered.Where(P => P.collegeId == collegeid)
                        .Select(P => P.RegistrationNumber.Trim())
                        .ToArray();
                List<jntuh_registered_faculty> jntuh_registered_faculty = new List<jntuh_registered_faculty>();
                jntuh_registered_faculty =
                    db.jntuh_registered_faculty.Where(
                        rf => strRegNoS.Contains(rf.RegistrationNumber) && rf.Blacklistfaculy != true)
                    //&& (rf.collegeId == null || rf.collegeId == collegeid)//&& rf.Notin116 != true
                        .ToList();

                //  var jntuh_notin415facultys = db.jntuh_notin415faculty.Where(F => F.CollegeId == collegeid).ToList();
                //  string[] strREG = jntuh_notin415facultys.Select(F => F.RegistrationNumber.Trim()).ToArray();
                string RegNumber = "";
                int? Specializationid = 0;
                foreach (var a in jntuh_registered_faculty)
                {
                    string Reason = String.Empty;
                    Specializationid =
                        jntuh_college_faculty_registered.Where(
                            C => C.RegistrationNumber.Trim() == a.RegistrationNumber.Trim())
                            .Select(C => C.SpecializationId)
                            .FirstOrDefault();
                    var faculty = new FacultyRegistration();
                    faculty.id = a.id;
                    faculty.Type = a.type;
                    faculty.CollegeId = collegeid;
                    faculty.RegistrationNumber = a.RegistrationNumber;
                    faculty.UniqueID = a.UniqueID;
                    faculty.FirstName = a.FirstName;
                    faculty.MiddleName = a.MiddleName;
                    faculty.LastName = a.LastName;
                    faculty.Basstatus = a.InvalidAadhaar;
                    if (PrincipalstrRegNoS.Contains(a.RegistrationNumber.Trim()))
                        faculty.Principal = "Principal";
                    else
                        faculty.Principal = "";


                    faculty.GenderId = a.GenderId;
                    faculty.Email = a.Email;
                    faculty.facultyPhoto = a.Photo;
                    faculty.Mobile = a.Mobile;
                    faculty.PANNumber = a.PANNumber;
                    //faculty.AadhaarNumber = a.AadhaarNumber;
                    faculty.AadhaarNumber =
                        jntuh_college_faculty_registered.Where(f => f.RegistrationNumber == a.RegistrationNumber)
                            .Select(s => s.AadhaarNumber)
                            .FirstOrDefault();
                    faculty.isActive = a.isActive;
                    faculty.isApproved = a.isApproved;
                    faculty.IdentfiedFor =
                        jntuh_college_faculty_registered.Where(
                            f => f.RegistrationNumber.Trim() == a.RegistrationNumber.Trim())
                            .Select(f => f.IdentifiedFor)
                            .FirstOrDefault();
                    if (faculty.IdentfiedFor == "UG")
                    {
                        a.DepartmentId =
                            jntuh_college_faculty_registered.Where(c => c.RegistrationNumber == a.RegistrationNumber)
                                .Select(s => s.DepartmentId)
                                .FirstOrDefault();
                        faculty.department =
                            jntuhDepartment.Where(d => d.id == a.DepartmentId)
                                .Select(d => d.departmentName)
                                .FirstOrDefault();
                        faculty.DegreeId =
                            jntuhDepartment.Where(d => d.id == a.DepartmentId).Select(d => d.degreeId).FirstOrDefault();
                        faculty.DegreeName =
                            jntuhDegree.Where(d => d.id == faculty.DegreeId).Select(s => s.degree).FirstOrDefault();
                        faculty.SpecializationName = string.Empty;
                    }
                    else
                    {
                        a.DepartmentId =
                            jntuh_college_faculty_registered.Where(c => c.RegistrationNumber == a.RegistrationNumber)
                                .Select(s => s.DepartmentId)
                                .FirstOrDefault();
                        faculty.department =
                            jntuhDepartment.Where(d => d.id == a.DepartmentId)
                                .Select(d => d.departmentName)
                                .FirstOrDefault();
                        faculty.SpecializationId =
                            jntuh_college_faculty_registered.Where(c => c.RegistrationNumber == a.RegistrationNumber)
                                .Select(s => s.SpecializationId)
                                .FirstOrDefault();
                        faculty.SpecializationName =
                            jntuhSpecialization.Where(s => s.id == faculty.SpecializationId)
                                .Select(s => s.specializationName)
                                .FirstOrDefault();
                    }


                    faculty.SamePANNumberCount =
                        jntuh_registered_faculty.Where(f => f.PANNumber == a.PANNumber).ToList().Count;
                    faculty.SameAadhaarNumberCount =
                        jntuh_registered_faculty.Where(f => f.AadhaarNumber == a.AadhaarNumber).ToList().Count;
                    faculty.SpecializationIdentfiedFor = Specializationid > 0
                        ? jntuhSpecialization.Where(S => S.id == Specializationid)
                            .Select(S => S.specializationName)
                            .FirstOrDefault()
                        : "";
                    faculty.isVerified = isFacultyVerified(a.id);
                    faculty.DeactivationReason = a.DeactivationReason;
                    faculty.FacultyVerificationStatus = a.FacultyVerificationStatus;
                    faculty.updatedOn = a.updatedOn;
                    faculty.createdOn =
                        jntuh_college_faculty_registered.Where(f => f.RegistrationNumber == a.RegistrationNumber)
                            .Select(f => f.createdOn)
                            .FirstOrDefault();

                    faculty.jntuh_registered_faculty_education = a.jntuh_registered_faculty_education;
                    faculty.DegreeId = a.jntuh_registered_faculty_education.Count(e => e.facultyId == a.id) > 0 ? a.jntuh_registered_faculty_education.Where(e => e.facultyId == a.id).Select(e => e.educationId).Max() : 0;
                    faculty.PanDeactivationReasion = !string.IsNullOrEmpty(a.PanDeactivationReason)
                        ? a.PanDeactivationReason
                        : "";
                    faculty.Absent = a.Absent != null ? (bool)a.Absent : false;
                    faculty.BlacklistFaculty = a.Blacklistfaculy != null ? (bool)a.Blacklistfaculy : false;
                    faculty.PHDundertakingnotsubmitted = a.PHDundertakingnotsubmitted != null
                        ? (bool)a.PHDundertakingnotsubmitted
                        : false;
                    faculty.NOTQualifiedAsPerAICTE = a.NotQualifiedAsperAICTE != null
                        ? (bool)a.NotQualifiedAsperAICTE
                        : false;
                    faculty.InvalidPANNo = a.InvalidPANNumber != null ? (bool)a.InvalidPANNumber : false;
                    faculty.InCompleteCeritificates = a.IncompleteCertificates != null
                        ? (bool)a.IncompleteCertificates
                        : false;
                    faculty.OriginalCertificatesnotshownFlag = a.OriginalCertificatesNotShown != null
                        ? (bool)a.OriginalCertificatesNotShown
                        : false;
                    faculty.FalsePAN = a.FalsePAN != null ? (bool)a.FalsePAN : false;
                    faculty.NOForm16 = a.NoForm16 != null ? (bool)a.NoForm16 : false;
                    faculty.ModifiedPANNo = a.ModifiedPANNumber;
                    faculty.PanStatusAfterDE = a.PanStatusAfterDE;
                    faculty.NoSCM = a.NoSCM != null ? (bool)a.NoSCM : false;
                    faculty.NoClass = a.Noclass != null ? (bool)a.Noclass : false;
                    //faculty.facultyAadhaarCardDocument = a.AadhaarDocument;
                    faculty.facultyAadhaarCardDocument =
                        jntuh_college_faculty_registered.Where(f => f.RegistrationNumber == a.RegistrationNumber)
                            .Select(s => s.AadhaarDocument)
                            .FirstOrDefault();
                    //faculty.updatedOn = jntuh_college_faculty_registered.Where(f => f.RegistrationNumber == a.RegistrationNumber).Select(f => f.updatedOn).FirstOrDefault();
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

                    //if (strREG.Contains(a.RegistrationNumber.Trim()))
                    //{
                    //   // faculty.SelectionCommitteeProcedings = string.IsNullOrEmpty(a.ProceedingDocument) ? "No" : "";
                    //    faculty.NoSCM = a.NoSCM == null ? false : (bool)a.NoSCM;
                    //}

                    if (Reason != "")
                    {
                        Reason = Reason.Substring(0, Reason.Length - 1);
                    }

                    faculty.DeactivationNew = Reason;
                    teachingFaculty.Add(faculty);
                }

                teachingFaculty =
                    teachingFaculty.Where(m => m.isActive == true)
                        .OrderBy(f => f.updatedOn)
                        .ThenBy(f => f.department)
                        .ToList();
                ViewBag.TotalFaculty = teachingFaculty.Count();
                ViewBag.AbsentFacultyCount = teachingFaculty.Where(a => a.Absent == true).ToList().Count();
                ViewBag.ClearFaculty = ViewBag.TotalFaculty - ViewBag.AbsentFacultyCount;
                ViewBag.notshowcertiCount =
                    teachingFaculty.Where(a => a.OriginalCertificatesnotshownFlag == true).ToList().Count();
                ViewBag.Modifiedpancount = teachingFaculty.Where(a => a.ModifiedPANNo != null).ToList().Count();
                ViewBag.Modifiedaadhaarcount = teachingFaculty.Where(a => a.PanStatusAfterDE != null).ToList().Count();
                ViewBag.FlagTotalFaculty = teachingFaculty.Where(item =>
                            item.Absent == true || item.NoSCM == true ||
                                //item.NOrelevantUgFlag == true ||
                                //item.NOrelevantPgFlag == true || item.NOrelevantPhdFlag == true ||
                                //item.InvalidPANNo == true ||
                            item.DegreeId < 4 ||
                            item.BlacklistFaculty == true || item.Type == "Adjunct" ||
                            item.VerificationStatus == true ||
                                //item.OriginalCertificatesnotshownFlag == true || 
                                //item.NOTQualifiedAsPerAICTE == true || 
                            item.NoClass == true
                            ).Select(e => e).Count();
            }
            return View(teachingFaculty.OrderBy(d => d.department).ThenBy(d => d.DegreeName).ThenBy(d => d.id).ToList());
        }
        public bool isFacultyVerified(int fid)
        {
            bool isVerified = false;

            var faculty = db.jntuh_registered_faculty.Find(fid);

            if (faculty.isApproved != null)
            {
                isVerified = true;
            }

            return isVerified;
        }


        //Faculty Verification View
        [Authorize(Roles = "Admin,DataEntry")]
        public ActionResult FacultyVerification(string fid)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            FacultyRegistration regFaculty = new FacultyRegistration();
            int fID = 0;

            if (fid != null)
            {
                fID =
                    Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(fid,
                        WebConfigurationManager.AppSettings["CryptoKey"]));
            }
            else
            {
                return RedirectToAction("College", "Dashboard");
            }

            // fID = 9155;
            if (!string.IsNullOrEmpty(fid))
            {
                regFaculty.GenderId = null;
                regFaculty.isFacultyRatifiedByJNTU = null;
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

                    if (regFaculty.CollegeId == 0 || regFaculty.CollegeId == null)
                    {
                        regFaculty.CollegeId =
                                  db.jntuh_college_principal_registered.Where(
                                      f => f.RegistrationNumber == regFaculty.RegistrationNumber)
                                      .Select(s => s.collegeId)
                                      .FirstOrDefault();
                    }


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
                    regFaculty.IncomeTaxFileview = faculty.IncometaxDocument;
                    //  regFaculty.faculty_AllCertificates = faculty.OrganizationName;
                    regFaculty.WorkingStatus = faculty.WorkingStatus;
                    regFaculty.TotalExperience = faculty.TotalExperience;
                    regFaculty.TotalExperiencePresentCollege = faculty.TotalExperiencePresentCollege;
                    // regFaculty.OrganizationName = faculty.OrganizationName;
                    if (regFaculty.CollegeId != 0)
                    {
                        regFaculty.CollegeName = db.jntuh_college.Where(a => a.id == regFaculty.CollegeId).Select(z => z.collegeName + " (" + z.collegeCode + ")").FirstOrDefault();
                    }

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

                    regFaculty.isFacultyRatifiedByJNTU = faculty.isFacultyRatifiedByJNTU;
                    if (faculty.DateOfRatification != null)
                    {
                        regFaculty.facultyDateOfRatification =
                            UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.DateOfRatification.ToString());
                    }

                    regFaculty.ProceedingsNo = faculty.ProceedingsNumber;
                    regFaculty.SelectionCommitteeProcedings = faculty.ProceedingDocument;

                    //Get the SCM Document from scmupload table uploaded by College.
                    // regFaculty.SelectionCommitteeProcedings = db.jntuh_scmupload.Where(e => e.RegistrationNumber.Trim() == regFaculty.RegistrationNumber.Trim()).Select(e => e.SCMDocument).FirstOrDefault();

                    regFaculty.AICTEFacultyId = faculty.AICTEFacultyId;
                    regFaculty.GrossSalary = faculty.grosssalary;
                    regFaculty.National = faculty.National;
                    regFaculty.InterNational = faculty.InterNational;
                    regFaculty.Citation = faculty.Citation;
                    regFaculty.Awards = faculty.Awards;
                    regFaculty.isActive = faculty.isActive;
                    regFaculty.isApproved = faculty.isApproved;
                    regFaculty.isView = true;
                    regFaculty.BlacklistFaculty = faculty.Blacklistfaculy;
                    regFaculty.VerificationStatus = faculty.AbsentforVerification;
                    regFaculty.DeactivationReason = faculty.DeactivationReason;

                    #region Faculty Education Data Getting

                    // var jntuh_education_category = db.jntuh_education_category.Where(e => e.isActive == true).ToList();
                    var registeredFacultyEducation = db.jntuh_registered_faculty_education.Where(e => e.facultyId == fID).ToList();

                    if (registeredFacultyEducation.Count != 0)
                    {
                        foreach (var item in registeredFacultyEducation)
                        {
                            if (item.educationId == 1)
                            {
                                regFaculty.SSC_educationId = 1;
                                regFaculty.SSC_studiedEducation = item.courseStudied;
                                regFaculty.SSC_specialization = item.specialization;
                                regFaculty.SSC_passedYear = item.passedYear;
                                regFaculty.SSC_percentage = item.marksPercentage;
                                regFaculty.SSC_division = item.division == null ? 0 : item.division;
                                regFaculty.SSC_university = item.boardOrUniversity;
                                regFaculty.SSC_place = item.placeOfEducation;
                                regFaculty.SSC_facultyCertificate = item.certificate;
                            }
                            else if (item.educationId == 3)
                            {
                                regFaculty.UG_educationId = 3;
                                regFaculty.UG_studiedEducation = item.courseStudied;
                                regFaculty.UG_specialization = item.specialization;
                                regFaculty.UG_passedYear = item.passedYear;
                                regFaculty.UG_percentage = item.marksPercentage;
                                regFaculty.UG_division = item.division == null ? 0 : item.division;
                                regFaculty.UG_university = item.boardOrUniversity;
                                regFaculty.UG_place = item.placeOfEducation;
                                regFaculty.UG_facultyCertificate = item.certificate;
                            }
                            else if (item.educationId == 4)
                            {
                                regFaculty.PG_educationId = 4;
                                regFaculty.PG_studiedEducation = item.courseStudied;
                                regFaculty.PG_specialization = item.specialization;
                                regFaculty.PG_passedYear = item.passedYear;
                                regFaculty.PG_percentage = item.marksPercentage;
                                regFaculty.PG_division = item.division == null ? 0 : item.division;
                                regFaculty.PG_university = item.boardOrUniversity;
                                regFaculty.PG_place = item.placeOfEducation;
                                regFaculty.PG_facultyCertificate = item.certificate;
                            }
                            else if (item.educationId == 5)
                            {
                                regFaculty.MPhil_educationId = 5;
                                regFaculty.MPhil_studiedEducation = item.courseStudied;
                                regFaculty.MPhil_specialization = item.specialization;
                                regFaculty.MPhil_passedYear = item.passedYear;
                                regFaculty.MPhil_percentage = item.marksPercentage;
                                regFaculty.MPhil_division = item.division == null ? 0 : item.division;
                                regFaculty.MPhil_university = item.boardOrUniversity;
                                regFaculty.MPhil_place = item.placeOfEducation;
                                regFaculty.MPhil_facultyCertificate = item.certificate;
                            }
                            else if (item.educationId == 6)
                            {
                                regFaculty.PhD_educationId = 6;
                                regFaculty.PhD_studiedEducation = item.courseStudied;
                                regFaculty.PhD_specialization = item.specialization;
                                regFaculty.PhD_passedYear = item.passedYear;
                                regFaculty.PhD_percentage = item.marksPercentage;
                                regFaculty.PhD_division = item.division == null ? 0 : item.division;
                                regFaculty.PhD_university = item.boardOrUniversity;
                                regFaculty.PhD_place = item.placeOfEducation;
                                regFaculty.PhD_facultyCertificate = item.certificate;
                            }
                            else if (item.educationId == 8)
                            {
                                regFaculty.Others_educationId = 8;
                                regFaculty.faculty_AllCertificates = item.certificate;
                            }
                        }
                    }

                    #endregion

                    var currentDate = DateTime.Now;
                    return View(regFaculty);
                }
                else
                {
                    return RedirectToAction("FacultyDataEntry", "DataEntry");
                }
            }
            else
            {
                return RedirectToAction("FacultyDataEntry", "DataEntry");
            }
        }

        //Faculty Verification adding flags view
        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification,DataEntry")]
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
                regFaculty.Absent = faculty.Absent ?? false;
                regFaculty.NOForm16 = faculty.NoForm16 ?? false;
                regFaculty.OriginalCertificatesnotshownFlag = faculty.OriginalCertificatesNotShown ?? false;
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
                //Commented on 18-06-2018 by Narayana Reddy
                //int[] verificationOfficers = db.jntuh_college_faculty_verified.Where(v => v.FacultyId == facultyId).Select(v => v.VerificationOfficer).Distinct().ToArray();
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

        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification")]
        public ActionResult FacultyVerificationNoEdit(string fid, string collegeid)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int fID = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(fid, WebConfigurationManager.AppSettings["CryptoKey"]));
            var facultydetails = db.jntuh_registered_faculty.Find(fID);
            if (facultydetails != null)
            {
                facultydetails.Absent = false;
                facultydetails.OriginalCertificatesNotShown = false;
                facultydetails.ModifiedPANNumber = null;
                facultydetails.PanStatusAfterDE = null;
                facultydetails.NoForm16 = false;
                facultydetails.NoForm26AS = false;
                facultydetails.Covid19 = false;
                facultydetails.Maternity = false;
                facultydetails.FacultyVerificationStatus = false;
                //facultydetails.updatedBy = userID;
                //facultydetails.updatedOn = DateTime.Now;
                db.Entry(facultydetails).State = EntityState.Modified;
                db.SaveChanges();
            }
            return RedirectToAction("FacultyDataEntry", "DataEntry", new { collegeid = collegeid });
        }

        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification,DataEntry")]
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
                jntuh_registered_faculty faculty = db.jntuh_registered_faculty.Find(fID);
                regFaculty.id = fID;
                regFaculty.Type = faculty.type;
                regFaculty.RegistrationNumber = faculty.RegistrationNumber;
                regFaculty.Email = faculty.Email;
                regFaculty.FirstName = faculty.FirstName;
                regFaculty.MiddleName = faculty.MiddleName;
                regFaculty.LastName = faculty.LastName;
                regFaculty.facultyPhoto = faculty.Photo;
                //regFaculty.Absent = faculty.Absent != false ? true : false;
                regFaculty.Absent = faculty.Absent ?? false;
                regFaculty.InvalidPANNo = faculty.InvalidPANNumber != false ? true : false;
                regFaculty.NoSCM = faculty.NoSCM != false ? true : false;
                regFaculty.NORelevantPG = faculty.NoRelevantPG;
                //regFaculty.NOForm16 = faculty.NoForm16 != false ? true : false;
                regFaculty.NOForm16 = faculty.NoForm16 ?? false;
                regFaculty.NOForm26AS = faculty.NoForm26AS ?? false;
                regFaculty.Covid19 = faculty.Covid19 ?? false;
                regFaculty.Maternity = faculty.Maternity ?? false;
                regFaculty.NOTQualifiedAsPerAICTE = faculty.NotQualifiedAsperAICTE ?? false;
                regFaculty.ModifiedPANNo = faculty.ModifiedPANNumber;
                //PanStatusAfterDE Column is Considered as ModifiedAadhaarNo from on Data Entery 05-03-2018
                regFaculty.ModifiedAadhaarNo = faculty.PanStatusAfterDE;
                regFaculty.InCompleteCeritificates = faculty.IncompleteCertificates ?? false;
                regFaculty.OriginalCertificatesnotshownFlag = faculty.OriginalCertificatesNotShown ?? false;
                regFaculty.DeactivationReason = faculty.DeactivationReason;
                regFaculty.PanDeactivationReasion = faculty.PanDeactivationReason;
                regFaculty.PanVerificationStatus = faculty.PanVerificationStatus;
            }
            return PartialView("_FacultyVerificationCheck", regFaculty);
        }

        [Authorize(Roles = "Admin,SuperAdmin,Operations")]
        public ActionResult DataEntryLogins()
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);

            int[] roleIDs = db.my_aspnet_roles.Where(r => r.name == "DataEntry" || r.name == "FacultyVerification").Select(r => r.id).ToArray();
            var list = db.my_aspnet_users.Join(db.my_aspnet_usersinroles, u => u.id, r => r.userId, (u, r) => new { u, r })
                   .Where(a => roleIDs.Contains(a.r.roleId)).Select(a => a.u).OrderBy(a => a.name).ToList();
            List<usersdata> userlist = new List<usersdata>();
            foreach (var item in list)
            {
                usersdata data = new usersdata();
                data.userid = item.id;
                data.username = item.name;
                data.usermail = db.my_aspnet_membership.Where(a => a.userId == item.id).Select(s => s.Email).FirstOrDefault();
                data.islockout = db.my_aspnet_membership.Where(a => a.userId == item.id).Select(s => s.IsLockedOut).FirstOrDefault();
                userlist.Add(data);
            }
            return View(userlist);
        }

        [Authorize(Roles = "Admin,SuperAdmin,Operations")]
        public ActionResult Edit(int id)
        {
            my_aspnet_users my_aspnet_users = db.my_aspnet_users.Find(id);
            //GetAllRoles();
            usersdata udata = new usersdata();
            udata.username = my_aspnet_users.name;
            udata.userid = my_aspnet_users.id;
            udata.usermail = db.my_aspnet_membership.Where(m => m.userId == id).Select(m => m.Email).FirstOrDefault();
            udata.userroleid = db.my_aspnet_usersinroles.Where(ur => ur.userId == id).Select(ur => ur.roleId).FirstOrDefault().ToString();
            udata.islockout = db.my_aspnet_membership.Where(m => m.userId == id).Select(m => (m.IsLockedOut.HasValue ? m.IsLockedOut.Value : false)).FirstOrDefault();
            return View(udata);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin,Operations")]
        public ActionResult Edit(usersdata udata)
        {
            if (udata.username == null)
            {
                udata.username = db.my_aspnet_users.Where(u => u.id == udata.userid).Select(u => u.name).FirstOrDefault();
            }

            if (ModelState.IsValid)
            {
                my_aspnet_membership my_aspnet_membership = db.my_aspnet_membership.Find(udata.userid);

                my_aspnet_membership.IsLockedOut = udata.islockout;
                db.Entry(my_aspnet_membership).State = EntityState.Modified;
                db.SaveChanges();
                TempData["Success"] = "User credentials updated successfully";

            }
            return RedirectToAction("DataEntryLogins");
        }

        //Faculty SCM Verification by College Wise Written by Narayana Reddy
        [Authorize(Roles = "Admin,SuperAdmin,DataEntry")]
        public ActionResult FacultySCMVerification(int? collegeid)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            List<int> userRoles = db.my_aspnet_usersinroles.Where(u => u.userId == userID).Select(u => u.roleId).ToList();
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy =
                db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            if (userRoles.Contains(
                                        db.my_aspnet_roles.Where(r => r.name.Equals("Admin"))
                                            .Select(r => r.id)
                                            .FirstOrDefault()))
            {
                ViewBag.Colleges =
                   db.jntuh_college.Join(db.jntuh_college_edit_status, co => co.id, e => e.collegeId,
                       (co, e) => new { co = co, e = e })
                       .Where(c => c.e.IsCollegeEditable == false && c.e.academicyearId == prAy)
                       .Select(c => new { collegeId = c.co.id, collegeName = c.co.collegeCode + "-" + c.co.collegeName })
                       .GroupBy(c => new { c.collegeId, c.collegeName }).Select(i => new { collegeId = i.Key.collegeId, collegeName = i.Key.collegeName })
                       .OrderBy(c => c.collegeName)
                       .ToList();
            }
            else
            {
                int InspectionPhaseId = db.jntuh_inspection_phase.Where(p => p.isActive == true && p.inspectionPhase == "Faculty SCM").Select(p => p.id).SingleOrDefault();
                int[] assignedcollegeslist =
                    db.jntuh_dataentry_allotment.Where(
                        d =>
                            d.InspectionPhaseId == InspectionPhaseId && d.userID == userID && d.isActive == true &&
                            d.isCompleted == false).Select(s => s.collegeID).ToArray();
                ViewBag.Colleges =
                    db.jntuh_college.Join(db.jntuh_college_edit_status, co => co.id, e => e.collegeId,
                        (co, e) => new { co = co, e = e })
                        .Where(c => c.e.IsCollegeEditable == false && assignedcollegeslist.Contains(c.e.collegeId))
                        .Select(c => new { collegeId = c.co.id, collegeName = c.co.collegeCode + "-" + c.co.collegeName })
                        .GroupBy(c => new { c.collegeId, c.collegeName }).Select(i => new { collegeId = i.Key.collegeId, collegeName = i.Key.collegeName })
                        .OrderBy(c => c.collegeName)
                        .ToList();

            }
            ViewBag.collegeid = collegeid;
            var jntuhDepartment = db.jntuh_department.ToList();
            List<FacultyRegistration> teachingFaculty = new List<FacultyRegistration>();
            if (collegeid != null)
            {
                var jntuhDegree = db.jntuh_degree.ToList();
                var jntuhSpecialization = db.jntuh_specialization.ToList();
                List<jntuh_college_faculty_registered> jntuh_college_faculty_registered =
                    db.jntuh_college_faculty_registered.Where(cf => cf.collegeId == collegeid).Select(cf => cf).ToList();
                string[] strRegNoS = jntuh_college_faculty_registered.Select(cf => cf.RegistrationNumber).ToArray();
                string[] PrincipalstrRegNoS =
                    db.jntuh_college_principal_registered.Where(P => P.collegeId == collegeid)
                        .Select(P => P.RegistrationNumber.Trim())
                        .ToArray();
                var jntuholdfaculty =
                    db.jntuh_college_previous_academic_faculty.Where(a => a.collegeId == collegeid)
                        .Select(s => s)
                        .ToList();

                List<jntuh_registered_faculty> jntuh_registered_faculty = new List<jntuh_registered_faculty>();
                jntuh_registered_faculty =
                    db.jntuh_registered_faculty.Where(
                        rf => strRegNoS.Contains(rf.RegistrationNumber) && rf.Blacklistfaculy != true)
                        .ToList();

                DateTime scmuploadstartdate = new DateTime(2022, 07, 05);
                var jntuh_scmupload =
                    db.jntuh_scmupload.Where(r => r.CollegeId == collegeid && (r.CreatedOn >= scmuploadstartdate || r.UpdatedOn >= scmuploadstartdate))
                        .Select(s => s)
                        .ToList();
                var jntuhcollegeClearfaculty =
                    db.jntuh_college_faculty_registered_copy.Where(c => c.collegeId == 1419).Select(s => s).ToList();
                var facultyexperiance =
                    db.jntuh_registered_faculty_experience.Where(
                    r => r.createdBycollegeId == collegeid).Select(s => s).ToList();
                string RegNumber = "";
                int? Specializationid = 0;
                foreach (var a in jntuh_registered_faculty)
                {
                    string Reason = String.Empty;
                    Specializationid =
                        jntuh_college_faculty_registered.Where(
                            C => C.RegistrationNumber.Trim() == a.RegistrationNumber.Trim())
                            .Select(C => C.SpecializationId)
                            .FirstOrDefault();

                    var faculty = new FacultyRegistration();
                    faculty.id = a.id;
                    faculty.Type = a.type;
                    faculty.CollegeId = collegeid;
                    faculty.RegistrationNumber = a.RegistrationNumber;
                    faculty.UniqueID = a.UniqueID;
                    faculty.FirstName = a.FirstName;
                    faculty.MiddleName = a.MiddleName;
                    faculty.LastName = a.LastName;
                    faculty.Basstatus = a.InvalidAadhaar;
                    if (PrincipalstrRegNoS.Contains(a.RegistrationNumber.Trim()))
                        faculty.Principal = "Principal";
                    else
                        faculty.Principal = "";

                    faculty.GenderId = a.GenderId;
                    faculty.Email = a.Email;
                    faculty.facultyPhoto = a.Photo;
                    faculty.Mobile = a.Mobile;
                    faculty.PANNumber = a.PANNumber;
                    //faculty.AadhaarNumber = a.AadhaarNumber;
                    faculty.AadhaarNumber =
                        jntuh_college_faculty_registered.Where(f => f.RegistrationNumber == a.RegistrationNumber)
                            .Select(s => s.AadhaarNumber)
                            .FirstOrDefault();
                    faculty.isActive = a.isActive;
                    faculty.isApproved = a.isApproved;
                    faculty.IdentfiedFor =
                        jntuh_college_faculty_registered.Where(
                            f => f.RegistrationNumber.Trim() == a.RegistrationNumber.Trim())
                            .Select(f => f.IdentifiedFor)
                            .FirstOrDefault();
                    if (faculty.IdentfiedFor == "UG")
                    {
                        a.DepartmentId =
                            jntuh_college_faculty_registered.Where(c => c.RegistrationNumber == a.RegistrationNumber)
                                .Select(s => s.DepartmentId)
                                .FirstOrDefault();
                        faculty.department =
                            jntuhDepartment.Where(d => d.id == a.DepartmentId)
                                .Select(d => d.departmentName)
                                .FirstOrDefault();
                        faculty.DegreeId =
                            jntuhDepartment.Where(d => d.id == a.DepartmentId).Select(d => d.degreeId).FirstOrDefault();
                        faculty.DegreeName =
                            jntuhDegree.Where(d => d.id == faculty.DegreeId).Select(s => s.degree).FirstOrDefault();
                        faculty.SpecializationName = string.Empty;
                    }
                    else
                    {
                        a.DepartmentId =
                            jntuh_college_faculty_registered.Where(c => c.RegistrationNumber == a.RegistrationNumber)
                                .Select(s => s.DepartmentId)
                                .FirstOrDefault();
                        faculty.department =
                            jntuhDepartment.Where(d => d.id == a.DepartmentId)
                                .Select(d => d.departmentName)
                                .FirstOrDefault();
                        faculty.SpecializationId =
                            jntuh_college_faculty_registered.Where(c => c.RegistrationNumber == a.RegistrationNumber)
                                .Select(s => s.SpecializationId)
                                .FirstOrDefault();
                        faculty.SpecializationName =
                            jntuhSpecialization.Where(s => s.id == faculty.SpecializationId)
                                .Select(s => s.specializationName)
                                .FirstOrDefault();
                    }
                    bool isnewfaculty = false;
                    bool isinActiveFaculty = false;
                    bool isscmupdated = false;
                    //New Faculty Checking
                    if (jntuholdfaculty.Where(j => j.RegistrationNumber.Trim() == j.RegistrationNumber.Trim() && j.DepartmentId == a.DepartmentId).Select(s => s).FirstOrDefault() == null)
                    {
                        isnewfaculty = true;
                    }
                    //Faculty Have Flags Checking
                    if (jntuhcollegeClearfaculty.Where(c => c.collegeId == 1419 && c.RegistrationNumber == a.RegistrationNumber.Trim()).Select(s => s).FirstOrDefault() == null)
                    {
                        isinActiveFaculty = true;
                    }
                    var scmupload = jntuh_scmupload.Where(
                        c => c.CollegeId == collegeid && c.RegistrationNumber == a.RegistrationNumber.Trim())
                        .Select(s => s)
                        .FirstOrDefault();
                    //Faculty updated or Created SCM document Recent
                    if (scmupload != null)
                    {
                        isscmupdated = true;
                    }

                    faculty.SamePANNumberCount =
                        jntuh_registered_faculty.Where(f => f.PANNumber == a.PANNumber).ToList().Count;
                    faculty.SameAadhaarNumberCount =
                        jntuh_registered_faculty.Where(f => f.AadhaarNumber == a.AadhaarNumber).ToList().Count;
                    faculty.SpecializationIdentfiedFor = Specializationid > 0
                        ? jntuhSpecialization.Where(S => S.id == Specializationid)
                            .Select(S => S.specializationName)
                            .FirstOrDefault()
                        : "";
                    faculty.isVerified = isFacultyVerified(a.id);
                    faculty.DeactivationReason = a.DeactivationReason;
                    faculty.FacultyVerificationStatus = a.FacultyVerificationStatus;
                    faculty.updatedOn = a.updatedOn;
                    faculty.createdOn =
                        jntuh_college_faculty_registered.Where(f => f.RegistrationNumber == a.RegistrationNumber)
                            .Select(f => f.createdOn)
                            .FirstOrDefault();

                    faculty.jntuh_registered_faculty_education = a.jntuh_registered_faculty_education;
                    //faculty.DegreeId = a.jntuh_registered_faculty_education.Count(e => e.facultyId == a.id) > 0 ? a.jntuh_registered_faculty_education.Where(e => e.facultyId == a.id).Select(e => e.educationId).Max() : 0;
                    faculty.PanDeactivationReasion = !string.IsNullOrEmpty(a.PanDeactivationReason)
                        ? a.PanDeactivationReason
                        : "";
                    faculty.Absent = a.Absent != null ? (bool)a.Absent : false;
                    faculty.BlacklistFaculty = a.Blacklistfaculy != null ? (bool)a.Blacklistfaculy : false;
                    faculty.PHDundertakingnotsubmitted = a.PHDundertakingnotsubmitted != null
                        ? (bool)a.PHDundertakingnotsubmitted
                        : false;
                    faculty.NOTQualifiedAsPerAICTE = a.NotQualifiedAsperAICTE != null
                        ? (bool)a.NotQualifiedAsperAICTE
                        : false;
                    faculty.InvalidPANNo = a.InvalidPANNumber != null ? (bool)a.InvalidPANNumber : false;
                    faculty.InCompleteCeritificates = a.IncompleteCertificates != null
                        ? (bool)a.IncompleteCertificates
                        : false;
                    faculty.OriginalCertificatesnotshownFlag = a.OriginalCertificatesNotShown != null
                        ? (bool)a.OriginalCertificatesNotShown
                        : false;
                    faculty.FalsePAN = a.FalsePAN != null ? (bool)a.FalsePAN : false;
                    faculty.NOForm16 = a.NoForm16 != null ? (bool)a.NoForm16 : false;
                    //faculty.NoSCM = a.NoSCM != null ? (bool)a.NoSCM : false;
                    faculty.NoSCM = a.NoSCM;
                    faculty.ModifiedPANNo = a.ModifiedPANNumber;
                    faculty.PanStatusAfterDE = a.PanStatusAfterDE;

                    faculty.facultyAadhaarCardDocument =
                        jntuh_college_faculty_registered.Where(f => f.RegistrationNumber == a.RegistrationNumber)
                            .Select(s => s.AadhaarDocument)
                            .FirstOrDefault();
                    var facultyexp = facultyexperiance.Where(r => r.facultyId == a.id).Select(s => s).ToList().LastOrDefault();
                    if (facultyexp != null)
                    {
                        if (facultyexp.facultyDateOfResignation != null)
                        {
                            DateTime date = Convert.ToDateTime(facultyexp.facultyDateOfResignation);
                            faculty.facultyRelievingDate = date.ToString("dd/MM/yyyy").Split(' ')[0];
                        }
                    }

                    //Show Faculty SCM Document checking allconditions
                    //if (isnewfaculty == true || isinActiveFaculty == true || isscmupdated == true)
                    if (isinActiveFaculty == true || isscmupdated == true)
                    {
                        if (scmupload == null)
                        {
                            var scmdocument =
                            db.jntuh_scmupload.Where(
                                s => s.RegistrationNumber == a.RegistrationNumber.Trim() && s.CollegeId == collegeid)
                                .Select(s => s)
                                .FirstOrDefault();
                            if (scmdocument != null)
                            {
                                faculty.SCMDocumentView = scmdocument.SCMDocument;
                                faculty.facultyDateOfRatification =
                                UAAAS.Models.Utilities.MMDDYY2DDMMYY(scmdocument.SCMdate.ToShortDateString());
                                //faculty.facultyDateOfRatification = scmdocument.SCMdate.ToShortDateString();
                            }
                        }
                        else
                        {
                            faculty.SCMDocumentView = scmupload.SCMDocument;
                            //faculty.facultyDateOfRatification = scmupload.SCMdate.ToShortDateString();
                            faculty.facultyDateOfRatification =
                                UAAAS.Models.Utilities.MMDDYY2DDMMYY(scmupload.SCMdate.ToShortDateString());
                        }
                        faculty.DeactivationNew = Reason;
                        teachingFaculty.Add(faculty);
                    }
                }

                teachingFaculty =
                    teachingFaculty.Where(m => m.isActive == true)
                        .OrderBy(f => f.updatedOn)
                        .ThenBy(f => f.department)
                        .ToList();
                ViewBag.TotalFaculty = teachingFaculty.Count();
                ViewBag.SCMFlagCount = teachingFaculty.Where(a => a.NoSCM == true).ToList().Count();
                ViewBag.ClearFaculty = ViewBag.TotalFaculty - ViewBag.SCMFlagCount;
                //ViewBag.notshowcertiCount =
                //    teachingFaculty.Where(a => a.OriginalCertificatesnotshownFlag == true).ToList().Count();
                //ViewBag.Modifiedpancount = teachingFaculty.Where(a => a.ModifiedPANNo != null).ToList().Count();
                //ViewBag.Modifiedaadhaarcount = teachingFaculty.Where(a => a.PanStatusAfterDE != null).ToList().Count();

            }
            return View(teachingFaculty.OrderBy(d => d.department).ThenBy(d => d.DegreeName).ThenBy(d => d.id).ToList());
        }

        [Authorize(Roles = "Admin,SuperAdmin,DataEntry")]
        public ActionResult ApproveFacultyScm(string fid, string collegeid)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            if (fid != null)
            {
                int fID = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(fid, WebConfigurationManager.AppSettings["CryptoKey"]));
                var facultydetails = db.jntuh_registered_faculty.Where(i => i.id == fID).Select(s => s).FirstOrDefault();
                if (facultydetails != null)
                {
                    facultydetails.NoSCM = false;
                    db.Entry(facultydetails).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Success"] = facultydetails.RegistrationNumber + " SCM Approved Successfully.";
                }
            }
            return RedirectToAction("FacultySCMVerification", new { collegeid = collegeid });
        }

        [Authorize(Roles = "Admin,SuperAdmin,DataEntry")]
        public ActionResult NotApproveFacultyScm(string fid, string collegeid)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            if (fid != null)
            {
                int fID = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(fid, WebConfigurationManager.AppSettings["CryptoKey"]));
                var facultydetails = db.jntuh_registered_faculty.Where(i => i.id == fID).Select(s => s).FirstOrDefault();
                if (facultydetails != null)
                {
                    facultydetails.NoSCM = true;
                    db.Entry(facultydetails).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Success"] = facultydetails.RegistrationNumber + " SCM not Approved Successfully.";
                }
            }
            return RedirectToAction("FacultySCMVerification", new { collegeid = collegeid });
        }

        [Authorize(Roles = "Admin")]
        public ActionResult Scmflagrollback(string fid, string collegeid)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            if (fid != null)
            {
                int fID = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(fid, WebConfigurationManager.AppSettings["CryptoKey"]));
                var facultydetails = db.jntuh_registered_faculty.Where(i => i.id == fID).Select(s => s).FirstOrDefault();
                if (facultydetails != null)
                {
                    facultydetails.NoSCM = null;
                    db.Entry(facultydetails).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Success"] = facultydetails.RegistrationNumber + " Rollback Successfully.";
                }
            }
            return RedirectToAction("FacultySCMVerification", new { collegeid = collegeid });
        }

        public ActionResult FacultySCMVerificationExportAllColleges()
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            List<int> userRoles = db.my_aspnet_usersinroles.Where(u => u.userId == userID).Select(u => u.roleId).ToList();
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy =
                db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            if (userRoles.Contains(
                                        db.my_aspnet_roles.Where(r => r.name.Equals("Admin"))
                                            .Select(r => r.id)
                                            .FirstOrDefault()))
            {
                ViewBag.Colleges =
                   db.jntuh_college.Join(db.jntuh_college_edit_status, co => co.id, e => e.collegeId,
                       (co, e) => new { co = co, e = e })
                       .Where(c => c.e.IsCollegeEditable == false && c.e.academicyearId == prAy)
                       .Select(c => new { collegeId = c.co.id, collegeName = c.co.collegeCode + "-" + c.co.collegeName })
                       .GroupBy(c => new { c.collegeId, c.collegeName }).Select(i => new { collegeId = i.Key.collegeId, collegeName = i.Key.collegeName })
                       .OrderBy(c => c.collegeName)
                       .ToList();
            }
            var colleges = db.jntuh_college.AsNoTracking().Where(i => i.isActive == true).ToList();
            var jntuhDepartment = db.jntuh_department.ToList();
            List<FacultyRegistration> teachingFaculty = new List<FacultyRegistration>();
            foreach (var item in ViewBag.Colleges)
            {
                int collegeid = item.collegeId;
                if (collegeid != null)
                {
                    var jntuhDegree = db.jntuh_degree.ToList();
                    var jntuhSpecialization = db.jntuh_specialization.ToList();
                    List<jntuh_college_faculty_registered> jntuh_college_faculty_registered =
                        db.jntuh_college_faculty_registered.Where(cf => cf.collegeId == collegeid).Select(cf => cf).ToList();
                    string[] strRegNoS = jntuh_college_faculty_registered.Select(cf => cf.RegistrationNumber).ToArray();
                    string[] PrincipalstrRegNoS =
                        db.jntuh_college_principal_registered.Where(P => P.collegeId == collegeid)
                            .Select(P => P.RegistrationNumber.Trim())
                            .ToArray();
                    var jntuholdfaculty =
                        db.jntuh_college_previous_academic_faculty.Where(a => a.collegeId == collegeid)
                            .Select(s => s)
                            .ToList();

                    List<jntuh_registered_faculty> jntuh_registered_faculty = new List<jntuh_registered_faculty>();
                    jntuh_registered_faculty =
                        db.jntuh_registered_faculty.Where(
                            rf => strRegNoS.Contains(rf.RegistrationNumber) && rf.Blacklistfaculy != true)
                            .ToList();

                    DateTime scmuploadstartdate = new DateTime(2021, 08, 06);
                    var jntuh_scmupload =
                        db.jntuh_scmupload.Where(r => r.CollegeId == collegeid && (r.CreatedOn >= scmuploadstartdate || r.UpdatedOn >= scmuploadstartdate))
                            .Select(s => s)
                            .ToList();
                    var jntuhcollegeClearfaculty =
                        db.jntuh_college_faculty_registered_copy.Where(c => c.collegeId == 1419).Select(s => s).ToList();
                    string RegNumber = "";
                    int? Specializationid = 0;
                    foreach (var a in jntuh_registered_faculty)
                    {
                        string Reason = String.Empty;
                        Specializationid =
                            jntuh_college_faculty_registered.Where(
                                C => C.RegistrationNumber.Trim() == a.RegistrationNumber.Trim())
                                .Select(C => C.SpecializationId)
                                .FirstOrDefault();

                        var faculty = new FacultyRegistration();
                        faculty.id = a.id;
                        faculty.Type = a.type;
                        faculty.CollegeId = collegeid;
                        faculty.RegistrationNumber = a.RegistrationNumber;
                        faculty.UniqueID = a.UniqueID;
                        faculty.FirstName = a.FirstName;
                        faculty.MiddleName = a.MiddleName;
                        faculty.LastName = a.LastName;
                        faculty.Basstatus = a.InvalidAadhaar;
                        if (PrincipalstrRegNoS.Contains(a.RegistrationNumber.Trim()))
                            faculty.Principal = "Principal";
                        else
                            faculty.Principal = "";

                        faculty.GenderId = a.GenderId;
                        faculty.Email = a.Email;
                        faculty.facultyPhoto = a.Photo;
                        faculty.Mobile = a.Mobile;
                        faculty.PANNumber = a.PANNumber;
                        //faculty.AadhaarNumber = a.AadhaarNumber;
                        faculty.AadhaarNumber =
                            jntuh_college_faculty_registered.Where(f => f.RegistrationNumber == a.RegistrationNumber)
                                .Select(s => s.AadhaarNumber)
                                .FirstOrDefault();
                        faculty.isActive = a.isActive;
                        faculty.isApproved = a.isApproved;
                        faculty.IdentfiedFor =
                            jntuh_college_faculty_registered.Where(
                                f => f.RegistrationNumber.Trim() == a.RegistrationNumber.Trim())
                                .Select(f => f.IdentifiedFor)
                                .FirstOrDefault();
                        if (faculty.IdentfiedFor == "UG")
                        {
                            a.DepartmentId =
                                jntuh_college_faculty_registered.Where(c => c.RegistrationNumber == a.RegistrationNumber)
                                    .Select(s => s.DepartmentId)
                                    .FirstOrDefault();
                            faculty.department =
                                jntuhDepartment.Where(d => d.id == a.DepartmentId)
                                    .Select(d => d.departmentName)
                                    .FirstOrDefault();
                            faculty.DegreeId =
                                jntuhDepartment.Where(d => d.id == a.DepartmentId).Select(d => d.degreeId).FirstOrDefault();
                            faculty.DegreeName =
                                jntuhDegree.Where(d => d.id == faculty.DegreeId).Select(s => s.degree).FirstOrDefault();
                            faculty.SpecializationName = string.Empty;
                        }
                        else
                        {
                            a.DepartmentId =
                                jntuh_college_faculty_registered.Where(c => c.RegistrationNumber == a.RegistrationNumber)
                                    .Select(s => s.DepartmentId)
                                    .FirstOrDefault();
                            faculty.department =
                                jntuhDepartment.Where(d => d.id == a.DepartmentId)
                                    .Select(d => d.departmentName)
                                    .FirstOrDefault();
                            faculty.SpecializationId =
                                jntuh_college_faculty_registered.Where(c => c.RegistrationNumber == a.RegistrationNumber)
                                    .Select(s => s.SpecializationId)
                                    .FirstOrDefault();
                            faculty.SpecializationName =
                                jntuhSpecialization.Where(s => s.id == faculty.SpecializationId)
                                    .Select(s => s.specializationName)
                                    .FirstOrDefault();
                        }
                        bool isnewfaculty = false;
                        bool isinActiveFaculty = false;
                        bool isscmupdated = false;
                        //New Faculty Checking
                        if (jntuholdfaculty.Where(j => j.RegistrationNumber.Trim() == j.RegistrationNumber.Trim() && j.DepartmentId == a.DepartmentId).Select(s => s).FirstOrDefault() == null)
                        {
                            isnewfaculty = true;
                        }
                        //Faculty Have Flags Checking
                        if (jntuhcollegeClearfaculty.Where(c => c.collegeId == 1419 && c.RegistrationNumber == a.RegistrationNumber.Trim()).Select(s => s).FirstOrDefault() == null)
                        {
                            isinActiveFaculty = true;
                        }
                        var scmupload = jntuh_scmupload.Where(
                            c => c.CollegeId == collegeid && c.RegistrationNumber == a.RegistrationNumber.Trim())
                            .Select(s => s)
                            .FirstOrDefault();
                        //Faculty updated or Created SCM document Recent
                        if (scmupload != null)
                        {
                            isscmupdated = true;
                        }

                        faculty.SamePANNumberCount =
                            jntuh_registered_faculty.Where(f => f.PANNumber == a.PANNumber).ToList().Count;
                        faculty.SameAadhaarNumberCount =
                            jntuh_registered_faculty.Where(f => f.AadhaarNumber == a.AadhaarNumber).ToList().Count;
                        faculty.SpecializationIdentfiedFor = Specializationid > 0
                            ? jntuhSpecialization.Where(S => S.id == Specializationid)
                                .Select(S => S.specializationName)
                                .FirstOrDefault()
                            : "";
                        faculty.isVerified = isFacultyVerified(a.id);
                        faculty.DeactivationReason = a.DeactivationReason;
                        faculty.FacultyVerificationStatus = a.FacultyVerificationStatus;
                        faculty.updatedOn = a.updatedOn;
                        faculty.createdOn =
                            jntuh_college_faculty_registered.Where(f => f.RegistrationNumber == a.RegistrationNumber)
                                .Select(f => f.createdOn)
                                .FirstOrDefault();

                        faculty.jntuh_registered_faculty_education = a.jntuh_registered_faculty_education;
                        //faculty.DegreeId = a.jntuh_registered_faculty_education.Count(e => e.facultyId == a.id) > 0 ? a.jntuh_registered_faculty_education.Where(e => e.facultyId == a.id).Select(e => e.educationId).Max() : 0;
                        faculty.PanDeactivationReasion = !string.IsNullOrEmpty(a.PanDeactivationReason)
                            ? a.PanDeactivationReason
                            : "";
                        faculty.Absent = a.Absent != null ? (bool)a.Absent : false;
                        faculty.BlacklistFaculty = a.Blacklistfaculy != null ? (bool)a.Blacklistfaculy : false;
                        faculty.PHDundertakingnotsubmitted = a.PHDundertakingnotsubmitted != null
                            ? (bool)a.PHDundertakingnotsubmitted
                            : false;
                        faculty.NOTQualifiedAsPerAICTE = a.NotQualifiedAsperAICTE != null
                            ? (bool)a.NotQualifiedAsperAICTE
                            : false;
                        faculty.InvalidPANNo = a.InvalidPANNumber != null ? (bool)a.InvalidPANNumber : false;
                        faculty.InCompleteCeritificates = a.IncompleteCertificates != null
                            ? (bool)a.IncompleteCertificates
                            : false;
                        faculty.OriginalCertificatesnotshownFlag = a.OriginalCertificatesNotShown != null
                            ? (bool)a.OriginalCertificatesNotShown
                            : false;
                        faculty.FalsePAN = a.FalsePAN != null ? (bool)a.FalsePAN : false;
                        faculty.NOForm16 = a.NoForm16 != null ? (bool)a.NoForm16 : false;
                        //faculty.NoSCM = a.NoSCM != null ? (bool)a.NoSCM : false;
                        faculty.NoSCM = a.NoSCM;
                        faculty.ModifiedPANNo = a.ModifiedPANNumber;
                        faculty.PanStatusAfterDE = a.PanStatusAfterDE;
                        var college = colleges.Where(i => i.id == collegeid).FirstOrDefault();
                        if (college != null)
                        {
                            faculty.CollegeCode = college.collegeCode;
                            faculty.CollegeName = college.collegeName;
                        }
                        faculty.facultyAadhaarCardDocument =
                            jntuh_college_faculty_registered.Where(f => f.RegistrationNumber == a.RegistrationNumber)
                                .Select(s => s.AadhaarDocument)
                                .FirstOrDefault();


                        //Show Faculty SCM Document checking allconditions
                        if (isnewfaculty == true || isinActiveFaculty == true || isscmupdated == true)
                        {
                            if (scmupload == null)
                            {
                                var scmdocument =
                                db.jntuh_scmupload.Where(
                                    s => s.RegistrationNumber == a.RegistrationNumber.Trim() && s.CollegeId == collegeid)
                                    .Select(s => s)
                                    .FirstOrDefault();
                                if (scmdocument != null)
                                {
                                    faculty.SCMDocumentView = scmdocument.SCMDocument;
                                    faculty.facultyDateOfRatification =
                                    UAAAS.Models.Utilities.MMDDYY2DDMMYY(scmdocument.SCMdate.ToShortDateString());
                                    //faculty.facultyDateOfRatification = scmdocument.SCMdate.ToShortDateString();
                                }
                            }
                            else
                            {
                                faculty.SCMDocumentView = scmupload.SCMDocument;
                                //faculty.facultyDateOfRatification = scmupload.SCMdate.ToShortDateString();
                                faculty.facultyDateOfRatification =
                                    UAAAS.Models.Utilities.MMDDYY2DDMMYY(scmupload.SCMdate.ToShortDateString());
                            }
                            faculty.DeactivationNew = Reason;
                            teachingFaculty.Add(faculty);
                        }
                    }
                }
            }
            teachingFaculty =
                        teachingFaculty.Where(m => m.isActive == true)
                            .OrderBy(f => f.updatedOn)
                            .ThenBy(f => f.department)
                            .ToList();
            string ReportHeader = "SCM_Faculty Flags_" + DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + ".xls";
            Response.ClearContent();
            Response.Buffer = true;
            Response.AddHeader("content-disposition", "attachment; filename=" + ReportHeader);
            Response.ContentType = "application/vnd.ms-excel";
            return PartialView("~/Views/DataEntry/FacultySCMVerificationExport.cshtml", teachingFaculty);
        }

        [Authorize(Roles = "Admin,SuperAdmin,DataEntry")]
        public ActionResult AdministrativeAreaDataEntry(int? collegeid)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            List<int> userRoles = db.my_aspnet_usersinroles.Where(u => u.userId == userID).Select(u => u.roleId).ToList();
            if (userRoles.Contains(
                                        db.my_aspnet_roles.Where(r => r.name.Equals("Admin"))
                                            .Select(r => r.id)
                                            .FirstOrDefault()))
            {
                ViewBag.Colleges =
                   db.jntuh_college.Join(db.jntuh_college_edit_status, co => co.id, e => e.collegeId,
                       (co, e) => new { co = co, e = e })
                       .Where(c => c.e.IsCollegeEditable == false)
                       .Select(c => new { collegeId = c.co.id, collegeName = c.co.collegeCode + "-" + c.co.collegeName })
                       .GroupBy(c => new { c.collegeId, c.collegeName }).Select(i => new { collegeId = i.Key.collegeId, collegeName = i.Key.collegeName })
                       .OrderBy(c => c.collegeName)
                       .ToList();
            }
            else
            {
                int AcademicYear = db.jntuh_academic_year.Where(a => a.isPresentAcademicYear == true).Select(a => a.id).FirstOrDefault();
                int InspectionPhaseId = db.jntuh_inspection_phase.Where(p => p.isActive == true && p.inspectionPhase == "Data Entry").Select(p => p.id).SingleOrDefault();
                int[] assignedcollegeslist =
                    db.jntuh_dataentry_allotment.Where(
                        d =>
                            d.InspectionPhaseId == InspectionPhaseId && d.userID == userID && d.isActive == true &&
                            d.isCompleted == false).Select(s => s.collegeID).ToArray();
                ViewBag.Colleges =
                    db.jntuh_college.Join(db.jntuh_college_edit_status, co => co.id, e => e.collegeId,
                        (co, e) => new { co = co, e = e })
                        .Where(c => c.e.IsCollegeEditable == false && c.e.academicyearId == AcademicYear + 1 && assignedcollegeslist.Contains(c.e.collegeId))
                        .Select(c => new { collegeId = c.co.id, collegeName = c.co.collegeCode + "-" + c.co.collegeName })
                        .GroupBy(c => new { c.collegeId, c.collegeName }).Select(i => new { collegeId = i.Key.collegeId, collegeName = i.Key.collegeName })
                        .OrderBy(c => c.collegeName)
                        .ToList();

            }
            ViewBag.collegeid = collegeid;
            var land = new List<AdminLand>();
            if (collegeid > 0)
            {
                land = db.jntuh_area_requirement.Where(r => r.isActive == true && r.areaType == "ADMINISTRATIVE").OrderBy(r => r.areaTypeDisplayOrder)
                                     .Select(r => new AdminLand
                                     {
                                         id = r.id,
                                         requirementType = r.requirementType,
                                         programId = r.programId,
                                         requiredRooms = r.requiredRooms,
                                         requiredRoomsCalculation = r.requiredRoomsCalculation,
                                         requiredArea = r.requiredArea,
                                         requiredAreaCalculation = r.requiredAreaCalculation,
                                         areaTypeDescription = r.areaTypeDescription,
                                         areaTypeDisplayOrder = r.areaTypeDisplayOrder,
                                         jntuh_program_type = db.jntuh_program_type.Where(p => p.id == r.programId).FirstOrDefault(),
                                         availableRooms = db.jntuh_college_area.Where(a => a.collegeId == collegeid && a.areaRequirementId == r.id).Select(a => a.availableRooms).FirstOrDefault(),
                                         availableArea = db.jntuh_college_area.Where(a => a.collegeId == collegeid && a.areaRequirementId == r.id).Select(a => a.availableArea).FirstOrDefault(),
                                         cfRooms = db.jntuh_college_area.Where(a => a.collegeId == collegeid && a.areaRequirementId == r.id).Select(a => a.cfrooms).FirstOrDefault(),
                                         cfArea = db.jntuh_college_area.Where(a => a.collegeId == collegeid && a.areaRequirementId == r.id).Select(a => a.cfarea).FirstOrDefault(),
                                         collegeId = collegeid
                                     }).Where(g => g.availableRooms != null && g.availableRooms != 0).ToList();
            }

            ViewBag.Count = land.Count();

            return View(land);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin,DataEntry")]
        public ActionResult AdministrativeAreaDataEntry(ICollection<AdminLand> adminLand)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = 0;
            if (userCollegeID == 0)
            {
                if (adminLand != null)
                {
                    foreach (var item in adminLand)
                    {
                        userCollegeID = (int)item.collegeId;
                    }
                }
            }
            List<AdminLand> land = new List<AdminLand>();
            if (userCollegeID > 0)
            {
                SaveArea(adminLand, userCollegeID);
                TempData["Success"] = "Added successfully";

                land = db.jntuh_area_requirement.Where(r => r.isActive == true && r.areaType == "ADMINISTRATIVE").OrderBy(r => r.areaTypeDisplayOrder)
                                         .Select(r => new AdminLand
                                         {
                                             id = r.id,
                                             requirementType = r.requirementType,
                                             programId = r.programId,
                                             requiredRooms = r.requiredRooms,
                                             requiredRoomsCalculation = r.requiredRoomsCalculation,
                                             requiredArea = r.requiredArea,
                                             requiredAreaCalculation = r.requiredAreaCalculation,
                                             areaTypeDescription = r.areaTypeDescription,
                                             areaTypeDisplayOrder = r.areaTypeDisplayOrder,
                                             jntuh_program_type = db.jntuh_program_type.Where(p => p.id == r.programId).FirstOrDefault(),
                                             availableRooms = db.jntuh_college_area.Where(a => a.collegeId == userCollegeID && a.areaRequirementId == r.id).Select(a => a.availableRooms).FirstOrDefault(),
                                             availableArea = db.jntuh_college_area.Where(a => a.collegeId == userCollegeID && a.areaRequirementId == r.id).Select(a => a.availableArea).FirstOrDefault(),
                                             cfRooms = db.jntuh_college_area.Where(a => a.collegeId == userCollegeID && a.areaRequirementId == r.id).Select(a => a.cfrooms).FirstOrDefault(),
                                             cfArea = db.jntuh_college_area.Where(a => a.collegeId == userCollegeID && a.areaRequirementId == r.id).Select(a => a.cfarea).FirstOrDefault(),
                                             collegeId = userCollegeID
                                         }).ToList();

                ViewBag.Count = land.Count();
            }

            return RedirectToAction("AdministrativeAreaDataEntry", "DataEntry", new { @collegeid = userCollegeID });
        }

        private void SaveArea(ICollection<AdminLand> adminLand, int userCollegeID)
        {
            try
            {
                int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
                if (userCollegeID > 0)
                {
                    if (ModelState.IsValid)
                    {
                        foreach (AdminLand item in adminLand)
                        {
                            jntuh_college_area area = new jntuh_college_area();
                            area.areaRequirementId = item.id;
                            if (userCollegeID == 0)
                            {
                                area.collegeId = (int)item.collegeId;
                            }
                            else
                            {
                                area.collegeId = userCollegeID;
                            }
                            area.availableRooms = item.availableRooms;
                            area.availableArea = item.availableArea;
                            area.cfrooms = item.cfRooms;
                            area.cfarea = item.cfArea;

                            int collegeAreaId = db.jntuh_college_area.Where(a => a.collegeId == userCollegeID && a.areaRequirementId == item.id).Select(a => a.id).FirstOrDefault();
                            if (collegeAreaId == 0)
                            {
                                area.createdBy = userID;
                                area.createdOn = DateTime.Now;

                                if ((item.cfRooms != null) && (item.cfArea != null))
                                {
                                    area.cfupdatedBy = userID;
                                    area.cfupdatedOn = DateTime.Now;
                                    db.jntuh_college_area.Add(area);
                                    db.SaveChanges();
                                }
                            }
                            else
                            {
                                int createdBy = Convert.ToInt32(db.jntuh_college_area.Where(a => a.collegeId == userCollegeID && a.areaRequirementId == item.id).Select(a => a.createdBy).FirstOrDefault());
                                DateTime createdon = Convert.ToDateTime(db.jntuh_college_area.Where(a => a.collegeId == userCollegeID && a.areaRequirementId == item.id).Select(a => a.createdOn).FirstOrDefault());

                                if (createdBy != 0)
                                {
                                    area.createdBy = createdBy;
                                    area.createdOn = createdon;
                                }

                                area.id = collegeAreaId;
                                area.cfupdatedBy = userID;
                                area.cfupdatedOn = DateTime.Now;
                                db.Entry(area).State = EntityState.Modified;
                                db.SaveChanges();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Authorize(Roles = "Admin,SuperAdmin,DataEntry")]
        public ActionResult InstructionalAreaDataEntry(int? collegeid)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            List<int> userRoles = db.my_aspnet_usersinroles.Where(u => u.userId == userID).Select(u => u.roleId).ToList();
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            if (userRoles.Contains(
                                        db.my_aspnet_roles.Where(r => r.name.Equals("Admin"))
                                            .Select(r => r.id)
                                            .FirstOrDefault()))
            {
                ViewBag.Colleges =
                   db.jntuh_college.Join(db.jntuh_college_edit_status, co => co.id, e => e.collegeId,
                       (co, e) => new { co = co, e = e })
                       .Where(c => c.e.IsCollegeEditable == false)
                       .Select(c => new { collegeId = c.co.id, collegeName = c.co.collegeCode + "-" + c.co.collegeName })
                       .GroupBy(c => new { c.collegeId, c.collegeName }).Select(i => new { collegeId = i.Key.collegeId, collegeName = i.Key.collegeName })
                       .OrderBy(c => c.collegeName)
                       .ToList();
            }
            else
            {
                int AcademicYear = db.jntuh_academic_year.Where(a => a.isPresentAcademicYear == true).Select(a => a.id).FirstOrDefault();
                int InspectionPhaseId = db.jntuh_inspection_phase.Where(p => p.isActive == true && p.inspectionPhase == "Data Entry").Select(p => p.id).SingleOrDefault();
                int[] assignedcollegeslist =
                    db.jntuh_dataentry_allotment.Where(
                        d =>
                            d.InspectionPhaseId == InspectionPhaseId && d.userID == userID && d.isActive == true &&
                            d.isCompleted == false).Select(s => s.collegeID).ToArray();
                ViewBag.Colleges =
                    db.jntuh_college.Join(db.jntuh_college_edit_status, co => co.id, e => e.collegeId,
                        (co, e) => new { co = co, e = e })
                        .Where(c => c.e.IsCollegeEditable == false && c.e.academicyearId == AcademicYear + 1 && assignedcollegeslist.Contains(c.e.collegeId))
                        .Select(c => new { collegeId = c.co.id, collegeName = c.co.collegeCode + "-" + c.co.collegeName })
                        .GroupBy(c => new { c.collegeId, c.collegeName }).Select(i => new { collegeId = i.Key.collegeId, collegeName = i.Key.collegeName })
                        .OrderBy(c => c.collegeName)
                        .ToList();

            }
            ViewBag.collegeid = collegeid;
            //List<string> collegeDegrees = db.jntuh_college_degree
            //                             .Join(db.jntuh_degree, cd => cd.degreeId, d => d.id, (cd, d) => new { cd, d })
            //                             .Where(s => s.d.isActive == true && s.cd.collegeId == collegeid)
            //                             .OrderBy(s => s.d.degreeDisplayOrder)
            //                             .Select(s => s.d.degree).ToList();

            List<string> collegeDegrees = (from ie in db.jntuh_college_intake_existing
                                           join s in db.jntuh_specialization on ie.specializationId equals s.id
                                           join d in db.jntuh_department on s.departmentId equals d.id
                                           join de in db.jntuh_degree on d.degreeId equals de.id
                                           //where ie.academicYearId == (prAy - 1) && (ie.aicteApprovedIntake != 0 || ie.approvedIntake != 0) && ie.collegeId == userCollegeID
                                           where ie.academicYearId == (prAy) && (ie.proposedIntake != 0) && ie.collegeId == collegeid
                                           orderby de.degreeDisplayOrder
                                           select de.degree
                ).Distinct().ToList();

            var land = new List<AdminLand>();
            if (collegeid > 0)
            {
                foreach (string degree in collegeDegrees)
                {
                    var programType = db.jntuh_program_type.Where(p => p.programType == degree).FirstOrDefault();
                    var areaRequirements = db.jntuh_area_requirement.Where(r => r.isActive == true && r.areaType == "INSTRUCTIONAL" && r.programId == programType.id).OrderBy(r => r.areaTypeDisplayOrder)
                                     .Select(r => r).ToList();
                    foreach (var areaRequirement in areaRequirements)
                    {
                        AdminLand newLand = new AdminLand();
                        newLand.id = areaRequirement.id;
                        newLand.requirementType = areaRequirement.requirementType;
                        newLand.programId = areaRequirement.programId;
                        newLand.requiredRooms = areaRequirement.requiredRooms;
                        newLand.requiredRoomsCalculation = areaRequirement.requiredRoomsCalculation;
                        newLand.requiredArea = areaRequirement.requiredArea;
                        newLand.requiredAreaCalculation = areaRequirement.requiredAreaCalculation;
                        newLand.areaTypeDescription = areaRequirement.areaTypeDescription;
                        newLand.areaTypeDisplayOrder = areaRequirement.areaTypeDisplayOrder;
                        newLand.jntuh_program_type = programType;
                        newLand.availableRooms = db.jntuh_college_area.Where(a => a.collegeId == collegeid && a.areaRequirementId == areaRequirement.id && a.specializationID == 0).Select(a => a.availableRooms).FirstOrDefault();
                        newLand.availableArea = db.jntuh_college_area.Where(a => a.collegeId == collegeid && a.areaRequirementId == areaRequirement.id && a.specializationID == 0).Select(a => a.availableArea).FirstOrDefault();
                        newLand.cfRooms = db.jntuh_college_area.Where(a => a.collegeId == collegeid && a.areaRequirementId == areaRequirement.id && a.specializationID == 0).Select(a => a.cfrooms).FirstOrDefault();
                        newLand.cfArea = db.jntuh_college_area.Where(a => a.collegeId == collegeid && a.areaRequirementId == areaRequirement.id && a.specializationID == 0).Select(a => a.cfarea).FirstOrDefault();
                        newLand.collegeId = collegeid;
                        land.Add(newLand);
                    }
                }
            }

            ViewBag.Count = land.Count();
            return View(land);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin,DataEntry")]
        public ActionResult InstructionalAreaDataEntry(ICollection<AdminLand> adminLand)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = 0;
            if (userCollegeID == 0)
            {
                if (adminLand != null)
                {
                    foreach (var item in adminLand)
                    {
                        userCollegeID = (int)item.collegeId;
                    }
                }
            }
            List<AdminLand> land = new List<AdminLand>();
            if (userCollegeID > 0)
            {
                SaveInstructionalArea(adminLand, userCollegeID);
                TempData["Success"] = "Added successfully";
            }

            return RedirectToAction("InstructionalAreaDataEntry", "DataEntry", new { @collegeid = userCollegeID });
        }

        private void SaveInstructionalArea(ICollection<AdminLand> adminLand, int userCollegeID)
        {
            try
            {
                int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
                if (userCollegeID > 0)
                {
                    if (ModelState.IsValid)
                    {
                        foreach (AdminLand item in adminLand)
                        {
                            jntuh_college_area area = new jntuh_college_area();
                            area.areaRequirementId = item.id;
                            if (userCollegeID == 0)
                            {
                                area.collegeId = (int)item.collegeId;
                            }
                            else
                            {
                                area.collegeId = userCollegeID;
                            }
                            area.availableRooms = item.availableRooms;
                            area.availableArea = item.availableArea;
                            area.cfrooms = item.cfRooms;
                            area.cfarea = item.cfArea;

                            if (item.specializationID == null)
                            {
                                area.specializationID = 0;
                            }
                            else
                            {
                                area.specializationID = item.specializationID;
                            }

                            jntuh_college_area collegeArea = db.jntuh_college_area.AsNoTracking().Where(a => a.collegeId == userCollegeID && a.areaRequirementId == item.id && a.specializationID == item.specializationID).Select(a => a).FirstOrDefault();
                            if (collegeArea == null)
                            {
                                area.createdBy = userID;
                                area.createdOn = DateTime.Now;

                                if ((item.availableRooms != null) && (item.availableArea != null))
                                {
                                    collegeArea = area;
                                    area.cfupdatedBy = userID;
                                    area.cfupdatedOn = DateTime.Now;
                                    db.jntuh_college_area.Add(collegeArea);
                                    db.SaveChanges();
                                }
                            }
                            else
                            {
                                int createdBy = Convert.ToInt32(db.jntuh_college_area.AsNoTracking().Where(a => a.collegeId == userCollegeID && a.areaRequirementId == item.id && a.specializationID == item.specializationID).Select(a => a.createdBy).FirstOrDefault());
                                DateTime createdon = Convert.ToDateTime(db.jntuh_college_area.AsNoTracking().Where(a => a.collegeId == userCollegeID && a.areaRequirementId == item.id && a.specializationID == item.specializationID).Select(a => a.createdOn).FirstOrDefault());

                                if (createdBy != 0)
                                {
                                    area.createdBy = createdBy;
                                    area.createdOn = createdon;
                                }
                                area.id = collegeArea.id;
                                area.cfupdatedBy = userID;
                                area.cfupdatedOn = DateTime.Now;
                                collegeArea = area;
                                db.Entry(collegeArea).State = EntityState.Modified;
                                //db.Entry(area).CurrentValues.SetValues(area);
                                db.SaveChanges();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }

    public class usersdata
    {
        public int userid { get; set; }
        public string username { get; set; }
        public string userroleid { get; set; }
        public string usermail { get; set; }
        public bool? islockout { get; set; }
    }
}
