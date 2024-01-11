using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Security;
using UAAAS.Models;

namespace UAAAS.Controllers
{
     [ErrorHandling]
    public class FacultyVerificationDENewReportsController : BaseController
    {
         private uaaasDBContext db = new uaaasDBContext();

         [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification")]
         public ActionResult Index(int? collegeid,string deptId)
         {

             ViewBag.Colleges = db.jntuh_college.Where(c => c.isActive == true).Select(c => new { collegeId = c.id, collegeName = c.collegeCode + "-" + c.collegeName }).OrderBy(c => c.collegeName).ToList();
             ViewBag.collegeid = collegeid;
             var jntuh_department = db.jntuh_department.ToList();
             List<FacultyRegistration> teachingFaculty = new List<FacultyRegistration>();
             ViewBag.Departments = jntuh_department.Where(e=>e.isActive==true).Select(e => new { deptId = e.id, deptName = e.departmentName }).OrderBy(e => e.deptName).ToList();
             if (collegeid != null)
             {
                 var SpecliazationIds = db.jntuh_college_intake_existing.Where(e => e.academicYearId == 8 && e.collegeId == collegeid).Select(e => e.specializationId).ToArray();

                 var DeptIds = db.jntuh_specialization.Where(e => SpecliazationIds.Contains(e.id)).Select(e => e.departmentId).ToArray();

                


                 ViewBag.Departments = jntuh_department.Where(e => DeptIds.Contains(e.id) && e.degreeId!=1).Select(e => new { deptId = e.id, deptName = e.departmentName }).OrderBy(e => e.deptName).ToList();


               

                 List<jntuh_college_faculty_registered> jntuh_college_faculty_registered = db.jntuh_college_faculty_registered.Where(cf => cf.collegeId == collegeid).Select(cf => cf).ToList();
                 string[] strRegNoS = db.jntuh_college_faculty_registered.Where(cf => cf.collegeId == collegeid).Select(cf => cf.RegistrationNumber).ToArray();

                 List<jntuh_registered_faculty> jntuh_registered_faculty = new List<jntuh_registered_faculty>();

                 if (deptId != null)
                 {

                     var DeptIds1 = jntuh_department.Where(e => e.departmentName.Trim() == deptId.Trim()).Select(e => e.id).ToArray();
                     jntuh_registered_faculty = db.jntuh_registered_faculty
                                                .Where(rf => strRegNoS.Contains(rf.RegistrationNumber) && DeptIds1.Contains((int)rf.DepartmentId))  //&& (rf.collegeId == null || rf.collegeId == collegeid)
                                                .ToList();
                 
                 }
                 else
                 {
                     jntuh_registered_faculty = db.jntuh_registered_faculty
                                             .Where(rf => strRegNoS.Contains(rf.RegistrationNumber))  //&& (rf.collegeId == null || rf.collegeId == collegeid)
                                             .ToList();
                 
                 }
                

               



                

                
               

                 var jntuh_notin415facultys = db.jntuh_notin415faculty.Where(F => F.CollegeId == collegeid).ToList();
                 var Specializations = db.jntuh_specialization.ToList();
                 string[] strREG = jntuh_notin415facultys.Select(F => F.RegistrationNumber.Trim()).ToArray();
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
                     faculty.GenderId = a.GenderId;
                     faculty.Email = a.Email;
                     faculty.facultyPhoto = a.Photo;
                     faculty.Mobile = a.Mobile;
                     faculty.PANNumber = a.PANNumber;
                     faculty.AadhaarNumber = a.AadhaarNumber;
                     faculty.isActive = a.isActive;
                     faculty.isApproved = a.isApproved;
                     faculty.department =
                         jntuh_department.Where(d => d.id == a.DepartmentId)
                             .Select(d => d.departmentName)
                             .FirstOrDefault();
                     faculty.SamePANNumberCount =
                         jntuh_registered_faculty.Where(f => f.PANNumber == a.PANNumber).ToList().Count;
                     faculty.SameAadhaarNumberCount =
                         jntuh_registered_faculty.Where(f => f.AadhaarNumber == a.AadhaarNumber).ToList().Count;
                     faculty.SpecializationIdentfiedFor = Specializationid > 0
                         ? Specializations.Where(S => S.id == Specializationid)
                             .Select(S => S.specializationName)
                             .FirstOrDefault()
                         : "";
                     //faculty.isVerified = isFacultyVerified(a.id);
                     faculty.DeactivationReason = a.DeactivationReason;
                     faculty.FacultyVerificationStatus = a.FacultyVerificationStatus;
                     faculty.updatedOn = a.updatedOn;
                     faculty.createdOn =
                         jntuh_college_faculty_registered.Where(f => f.RegistrationNumber == a.RegistrationNumber)
                             .Select(f => f.createdOn)
                             .FirstOrDefault();
                     faculty.IdentfiedFor =
                         jntuh_college_faculty_registered.Where(
                             f => f.RegistrationNumber.Trim() == a.RegistrationNumber.Trim())
                             .Select(f => f.IdentifiedFor)
                             .FirstOrDefault();
                     faculty.jntuh_registered_faculty_education = a.jntuh_registered_faculty_education;
                     faculty.DegreeId = a.jntuh_registered_faculty_education.Count(e => e.facultyId == a.id) > 0
                         ? a.jntuh_registered_faculty_education.Where(e => e.facultyId == a.id)
                             .Select(e => e.educationId)
                             .Max()
                         : 0;
                     faculty.PanDeactivationReasion = !string.IsNullOrEmpty(a.PanDeactivationReason)
                         ? a.PanDeactivationReason
                         : "";
                     faculty.Absent = a.Absent != null ? (bool) a.Absent : false;
                     faculty.BlacklistFaculty = a.Blacklistfaculy != null ? (bool) a.Blacklistfaculy : false;
                     faculty.PHDundertakingnotsubmitted = a.PHDundertakingnotsubmitted != null
                         ? (bool) a.PHDundertakingnotsubmitted
                         : false;
                     faculty.NOTQualifiedAsPerAICTE = a.NotQualifiedAsperAICTE != null
                         ? (bool) a.NotQualifiedAsperAICTE
                         : false;
                     faculty.InvalidPANNo = a.InvalidPANNumber != null ? (bool) a.InvalidPANNumber : false;
                     ;
                     faculty.InCompleteCeritificates = a.IncompleteCertificates != null
                         ? (bool) a.IncompleteCertificates
                         : false;
                     faculty.XeroxcopyofcertificatesFlag = a.Xeroxcopyofcertificates != null
                         ? (bool)a.Xeroxcopyofcertificates
                         : false;
                     faculty.FalsePAN = a.FalsePAN != null ? (bool) a.FalsePAN : false;
                     faculty.NORelevantPHD = a.NORelevantPHD;
                     faculty.NORelevantPG = a.NoRelevantPG;
                     faculty.NORelevantUG = a.NoRelevantUG;
                     faculty.NoSCM = a.NoSCM != null ? (bool)a.NoSCM : false;
                     faculty.NotIdentityFiedForAnyProgramFlag = a.NotIdentityfiedForanyProgram != null ? (bool)a.NotIdentityfiedForanyProgram : false;
                     //faculty.SamePANUsedByMultipleFaculty = a.SamePANUsedByMultipleFaculty != null ? (bool)a.SamePANUsedByMultipleFaculty : false;
                     //faculty.Basstatus = a.BASStatus;
                     //faculty.BasstatusOld = a.BASStatusOld;
                     faculty.OriginalsVerifiedUG = a.OriginalsVerifiedUG != null ? (bool)a.OriginalsVerifiedUG : false;
                     faculty.OriginalsVerifiedPG = a.OriginalsVerifiedPG != null ? (bool)a.OriginalsVerifiedPG : false;
                     faculty.BlacklistFaculty = a.Blacklistfaculy != null ? (bool)a.Blacklistfaculy : false;
                     if (faculty.Absent == true)
                         Reason += "Absent";
                     if (faculty.Type == "Adjunct")
                     {
                         if (Reason != string.Empty)
                             Reason += ",Adjunct Faculty";
                         else
                             Reason += "Adjunct Faculty";
                     }

                     if (faculty.XeroxcopyofcertificatesFlag == true)
                     {
                         if (Reason != string.Empty)
                             Reason += ",Xerox copyof certificates";
                         else
                             Reason += "Xerox copyof certificates";
                     }

                     if (faculty.NORelevantPHD == "Yes")
                     {
                         if (Reason != string.Empty)
                             Reason += ",NO Relevant UG";
                         else
                             Reason += "NO Relevant UG";
                     }

                     if (faculty.NORelevantPG == "Yes")
                     {
                         if (Reason != string.Empty)
                             Reason += ",NO Relevant PG";
                         else
                             Reason += "NO Relevant PG";
                     }

                     if (faculty.NORelevantPHD == "Yes")
                     {
                         if (Reason != string.Empty)
                             Reason += ",NO Relevant PHD";
                         else
                             Reason += "NO Relevant PHD";
                     }

                     if (faculty.NOTQualifiedAsPerAICTE == true)
                     {
                         if (Reason != string.Empty)
                             Reason += ",NOT Qualified AsPerAICTE";
                         else
                             Reason += "NOT Qualified AsPerAICTE";
                     }

                     if (faculty.InvalidPANNo == true)
                     {
                         if (Reason != string.Empty)
                             Reason += ",InvalidPANNumber";
                         else
                             Reason += "InvalidPANNumber";
                     }

                     if (faculty.InCompleteCeritificates == true)
                     {
                         if (Reason != string.Empty)
                             Reason += ",InComplete Ceritificates";
                         else
                             Reason += "InComplete Ceritificates";
                     }

                     if (faculty.NoSCM == true)
                     {
                         if (Reason != string.Empty)
                             Reason += ",NoSCM";
                         else
                             Reason += "NoSCM";
                     }

                     if (faculty.OriginalCertificatesnotshownFlag == true)
                     {
                         if (Reason != string.Empty)
                             Reason += ",Original Certificates notshown";
                         else
                             Reason += "Original Certificates notshown";
                     }

                     if (faculty.PANNumber == null)
                     {
                         if (Reason != string.Empty)
                             Reason += ",No PANNumber";
                         else
                             Reason += "No PANNumber";
                     }

                     if (faculty.NotIdentityFiedForAnyProgramFlag == true)
                     {
                         if (Reason != string.Empty)
                             Reason += ",NotIdentityFied ForAnyProgram";
                         else
                             Reason += "NotIdentityFied ForAnyProgram";
                     }

                     if (faculty.SamePANUsedByMultipleFaculty == true)
                     {
                         if (Reason != string.Empty)
                             Reason += ",SamePANUsedByMultipleFaculty";
                         else
                             Reason += "SamePANUsedByMultipleFaculty";
                     }

                     if (faculty.Basstatus == "Yes")
                     {
                         if (Reason != string.Empty)
                             Reason += ",No/Invalid Aadhaar Document";
                         else
                             Reason += "No/Invalid Aadhaar Document";
                     }

                     if (faculty.BasstatusOld == "Yes")
                     {
                         if (Reason != string.Empty)
                             Reason += ",BAS Flag";
                         else
                             Reason += "BAS Flag";
                     }

                     if (faculty.OriginalsVerifiedUG == true)
                     {
                         if (Reason != string.Empty)
                             Reason += ",Complaint PHD Faculty";
                         else
                             Reason += "Complaint PHD Faculty";
                     }

                     if (faculty.OriginalsVerifiedPHD == true)
                     {
                         if (Reason != string.Empty)
                             Reason += ",No Guide Sign in PHD Thesis";
                         else
                             Reason += "No Guide Sign in PHD Thesis";
                     }
                     if (faculty.BlacklistFaculty == true)
                     {
                         if (Reason != string.Empty)
                             Reason += ",Blacklistfaculy";
                         else
                             Reason += "Blacklistfaculy";
                     }
                     //if (faculty.Absent == true)
                         //{
                         //    Reason = "Absent" + ",";
                         //}


                         //if (faculty.NOTQualifiedAsPerAICTE == true)
                         //{
                         //    Reason += "Not Qualified as AICTE" + ",";
                         //}
                         //if (faculty.InCompleteCeritificates == true)
                         //{
                         //    Reason += "Incomplete Certificates(UG/PG/PHD/SCM)" + ",";
                         //}

                         //if (strREG.Contains(a.RegistrationNumber.Trim()))
                         //{
                         //    // faculty.SelectionCommitteeProcedings = string.IsNullOrEmpty(a.ProceedingDocument) ? "No" : "";
                         //    faculty.NoSCM = a.NoSCM == null ? false : (bool)a.NoSCM;
                         //}

                         if (Reason != "")
                         {
                             Reason = Reason.Substring(0, Reason.Length - 1);
                         }

                         faculty.DeactivationNew = Reason;
                         teachingFaculty.Add(faculty);
                     }
               
                 //var data = jntuh_registered_faculty.Select(a => new FacultyRegistration
                 //{
                 //    id = a.id,
                 //    Type = a.type,
                 //    CollegeId = collegeid,
                 //    RegistrationNumber = a.RegistrationNumber,
                 //    UniqueID = a.UniqueID,
                 //    FirstName = a.FirstName,
                 //    MiddleName = a.MiddleName,
                 //    LastName = a.LastName,
                 //    GenderId = a.GenderId,
                 //    Email = a.Email,
                 //    facultyPhoto = a.Photo,
                 //    Mobile = a.Mobile,
                 //    PANNumber = a.PANNumber,
                 //    AadhaarNumber = a.AadhaarNumber,
                 //   isActive = a.isActive,
                 //    isApproved = a.isApproved,
                 //    department = jntuh_department.Where(d => d.id == a.DepartmentId).Select(d => d.departmentName).FirstOrDefault(),
                 //    SamePANNumberCount = jntuh_registered_faculty.Where(f => f.PANNumber == a.PANNumber).ToList().Count(),
                 //    SameAadhaarNumberCount = jntuh_registered_faculty.Where(f => f.AadhaarNumber == a.AadhaarNumber).ToList().Count(),
                 //    isVerified = isFacultyVerified(a.id),
                 //    DeactivationReason = a.DeactivationReason,
                 //    FacultyVerificationStatus = a.FacultyVerificationStatus,
                 //    updatedOn = a.updatedOn,
                 //    createdOn = jntuh_college_faculty_registered.Where(f => f.RegistrationNumber == a.RegistrationNumber).Select(f => f.createdOn).FirstOrDefault(),
                 //    jntuh_registered_faculty_education = a.jntuh_registered_faculty_education,
                 //    DegreeId = a.jntuh_registered_faculty_education.Where(e => e.facultyId == a.id).Count()> 0 ? a.jntuh_registered_faculty_education.Where(e => e.facultyId == a.id).Select(e => e.educationId).Max() : 0,
                 //    PanDeactivationReasion = !string.IsNullOrEmpty(a.PanDeactivationReason)?a.PanDeactivationReason:"",
                 //    Absent=a.Absent!=null?(bool)a.Absent:false,
                 //    NOTQualifiedAsPerAICTE = a.NotQualifiedAsperAICTE !=null?(bool)a.NotQualifiedAsperAICTE:false,


                 //}).AsParallel().ToList();

                 //teachingFaculty.AddRange(data);
                 teachingFaculty = teachingFaculty.Where(m => m.isActive == true).OrderBy(f => f.updatedOn).ThenBy(f => f.department).ToList();
                 #region Paging
                 //int totalPages = 1;
                 //int totalLabs = teachingFaculty.Count();
                 //int First = 0;
                 //int second = 0;
                 //if (totalLabs > 20)
                 //{
                 //    totalPages = totalLabs / 20;
                 //    if (totalLabs > 20 * totalPages)
                 //    {
                 //        totalPages = totalPages + 1;
                 //    }
                 //}
                 //if (pageNumber == null || pageNumber == 1)
                 //{
                 //    First = 0;
                 //    second = 20;
                 //}
                 //else if (pageNumber > 1)
                 //{
                 //    First = ((pageNumber ?? default(int)) * 20) - 20;

                 //    //second = (pageNumber ?? default(int)) * 100;
                 //    second = 20;
                 //    int Total = teachingFaculty.Count;
                 //    if (Total <= ((pageNumber ?? default(int)) * 20))
                 //    {

                 //        second = Total - First;
                 //    }
                 //}
                 //ViewBag.Pages = totalPages;

                 //teachingFaculty = teachingFaculty.GetRange(First, totalLabs > 20 ? second : totalLabs);
                 #endregion
                 return View(teachingFaculty);
             }

             return View(teachingFaculty);
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



         [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification,DataEntry")]
         public ActionResult FacultyDataEntryExportExcel(int? collegeid, string type, string deptId)
         {
             var jntuh_department = db.jntuh_department.ToList();
             var jntuh_college = db.jntuh_college.ToList();
             var jntuh_collegeCode = db.jntuh_college.Where(C => C.id == collegeid).Select(C => C.collegeCode).FirstOrDefault();
             List<FacultyRegistration> teachingFaculty = new List<FacultyRegistration>();

             if (collegeid != null)
             {
                 List<jntuh_college_faculty_registered> jntuh_college_faculty_registered = db.jntuh_college_faculty_registered.Where(cf => cf.collegeId == collegeid).Select(cf => cf).ToList();
                 string[] strRegNoS = db.jntuh_college_faculty_registered.Where(cf => cf.collegeId == collegeid).Select(cf => cf.RegistrationNumber).ToArray();

                 List<jntuh_registered_faculty> jntuh_registered_faculty = new List<jntuh_registered_faculty>();


                 if (deptId != null)
                 {
                     var DeptIds1 = jntuh_department.Where(e => e.departmentName.Trim() == deptId.Trim()).Select(e => e.id).ToArray();
                     jntuh_registered_faculty = db.jntuh_registered_faculty
                                                .Where(rf => strRegNoS.Contains(rf.RegistrationNumber) && rf.Notin116 != true && DeptIds1.Contains((int)rf.DepartmentId))  //&& (rf.collegeId == null || rf.collegeId == collegeid)
                                                .ToList();

                 }
                 else
                 {
                     jntuh_registered_faculty = db.jntuh_registered_faculty
                                             .Where(rf => strRegNoS.Contains(rf.RegistrationNumber) && rf.Notin116 != true)  //&& (rf.collegeId == null || rf.collegeId == collegeid)
                                             .ToList();

                 }

                 //jntuh_registered_faculty = db.jntuh_registered_faculty
                 //                             .Where(rf => strRegNoS.Contains(rf.RegistrationNumber) && rf.Notin116 != true)  //&& (rf.collegeId == null || rf.collegeId == collegeid)
                 //                             .ToList();







                 var jntuh_notin415facultys = db.jntuh_notin415faculty.Where(F => F.CollegeId == collegeid).ToList();
                 var Specializations = db.jntuh_specialization.ToList();
                 string[] strREG = jntuh_notin415facultys.Select(F => F.RegistrationNumber.Trim()).ToArray();
                 string RegNumber = "";
                 int? Specializationid = 0;
                 foreach (var a in jntuh_registered_faculty)
                 {
                     string Reason = String.Empty;
                     Specializationid = jntuh_college_faculty_registered.Where(C => C.RegistrationNumber.Trim() == a.RegistrationNumber.Trim()).Select(C => C.SpecializationId).FirstOrDefault();
                     var faculty = new FacultyRegistration();
                     faculty.CollegeCode = jntuh_college.FirstOrDefault(i => i.id == collegeid).collegeCode;
                     faculty.CollegeName = jntuh_college.FirstOrDefault(i => i.id == collegeid).collegeName;
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
                     faculty.SamePANNumberCount = jntuh_registered_faculty.Where(f => f.PANNumber == a.PANNumber).ToList().Count;
                     faculty.SameAadhaarNumberCount = jntuh_registered_faculty.Where(f => f.AadhaarNumber == a.AadhaarNumber).ToList().Count;
                     faculty.SpecializationIdentfiedFor = Specializationid > 0 ? Specializations.Where(S => S.id == Specializationid).Select(S => S.specializationName).FirstOrDefault() : "";
                     faculty.isVerified = isFacultyVerified(a.id);
                     faculty.DeactivationReason = a.DeactivationReason;
                     faculty.FacultyVerificationStatus = a.FacultyVerificationStatus;
                     faculty.updatedOn = a.updatedOn;
                     faculty.createdOn = jntuh_college_faculty_registered.Where(f => f.RegistrationNumber == a.RegistrationNumber).Select(f => f.createdOn).FirstOrDefault();
                     faculty.IdentfiedFor = jntuh_college_faculty_registered.Where(f => f.RegistrationNumber.Trim() == a.RegistrationNumber.Trim()).Select(f => f.IdentifiedFor).FirstOrDefault();
                     faculty.jntuh_registered_faculty_education = a.jntuh_registered_faculty_education;
                     faculty.DegreeId = a.jntuh_registered_faculty_education.Count(e => e.facultyId == a.id) > 0 ? a.jntuh_registered_faculty_education.Where(e => e.facultyId == a.id).Select(e => e.educationId).Max() : 0;
                     faculty.PanDeactivationReasion = !string.IsNullOrEmpty(a.PanDeactivationReason) ? a.PanDeactivationReason : "";
                     faculty.Absent = a.Absent != null ? (bool)a.Absent : false;
                     faculty.BlacklistFaculty = a.Blacklistfaculy != null ? (bool)a.Blacklistfaculy : false;
                     faculty.PHDundertakingnotsubmitted = a.PHDundertakingnotsubmitted != null ? (bool)a.PHDundertakingnotsubmitted : false;
                     faculty.NOTQualifiedAsPerAICTE = a.NotQualifiedAsperAICTE != null ? (bool)a.NotQualifiedAsperAICTE : false;
                     faculty.InvalidPANNo = a.InvalidPANNumber != null ? (bool)a.InvalidPANNumber : false; ;
                     faculty.InCompleteCeritificates = a.IncompleteCertificates != null ? (bool)a.IncompleteCertificates : false; ;
                     faculty.FalsePAN = a.FalsePAN != null ? (bool)a.FalsePAN : false;
                     //if (faculty.Absent == true)
                     //{
                     //    Reason = "Absent" + ",";
                     //}


                     //if (faculty.NOTQualifiedAsPerAICTE == true)
                     //{
                     //    Reason += "Not Qualified as AICTE" + ",";
                     //}
                     //if (faculty.InCompleteCeritificates == true)
                     //{
                     //    Reason += "Incomplete Certificates(UG/PG/PHD/SCM)" + ",";
                     //}

                     if (strREG.Contains(a.RegistrationNumber.Trim()))
                     {
                         // faculty.SelectionCommitteeProcedings = string.IsNullOrEmpty(a.ProceedingDocument) ? "No" : "";
                         faculty.NoSCM = a.NoSCM == null ? false : (bool)a.NoSCM;
                     }

                     //if (Reason != "")
                     //{
                     //    Reason = Reason.Substring(0, Reason.Length - 1);
                     //}

                     //faculty.DeactivationNew = Reason;
                     teachingFaculty.Add(faculty);
                 }
                 teachingFaculty = teachingFaculty.Where(m => m.isActive == true).OrderBy(f => f.updatedOn).ThenBy(f => f.department).ToList();
                 if (type == "Excel")
                 {
                     Response.ClearContent();
                     Response.Buffer = true;
                     Response.AddHeader("content-disposition", "attachment; filename=" + jntuh_collegeCode + "- " + deptId + "-College Wise Faculty Details.xls");
                     Response.ContentType = "application/vnd.ms-excel";
                     return PartialView("~/Views/Reports/_FacultyDataEntryExportExcel.cshtml", teachingFaculty);

                 }
                 // return View(teachingFaculty);
             }
             return RedirectToAction("Index");
         }



         [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification,DataEntry")]
         public ActionResult ConsideredFacultyDataEntryExportExcel(int? collegeid, string type, string deptId)
         {
             var jntuh_department = db.jntuh_department.ToList();
             var jntuh_college = db.jntuh_college.ToList();
             var jntuh_collegeCode = db.jntuh_college.Where(C => C.id == collegeid).Select(C => C.collegeCode).FirstOrDefault();
             List<FacultyRegistration> teachingFaculty = new List<FacultyRegistration>();

             if (collegeid != null)
             {
                 List<jntuh_college_faculty_registered> jntuh_college_faculty_registered = db.jntuh_college_faculty_registered.Where(cf => cf.collegeId == collegeid).Select(cf => cf).ToList();
                 string[] strRegNoS = db.jntuh_college_faculty_registered.Where(cf => cf.collegeId == collegeid).Select(cf => cf.RegistrationNumber).ToArray();

                 List<jntuh_registered_faculty> jntuh_registered_faculty = new List<jntuh_registered_faculty>();


                 if (deptId != null)
                 {
                     var DeptIds1 = jntuh_department.Where(e => e.departmentName.Trim() == deptId.Trim()).Select(e => e.id).ToArray();
                     jntuh_registered_faculty = db.jntuh_registered_faculty
                                                .Where(rf => strRegNoS.Contains(rf.RegistrationNumber) && rf.Notin116 != true && DeptIds1.Contains((int)rf.DepartmentId))  //&& (rf.collegeId == null || rf.collegeId == collegeid)
                                                .ToList();

                 }
                 else
                 {
                     jntuh_registered_faculty = db.jntuh_registered_faculty
                                             .Where(rf => strRegNoS.Contains(rf.RegistrationNumber) && rf.Notin116 != true)  //&& (rf.collegeId == null || rf.collegeId == collegeid)
                                             .ToList();

                 }
                 //jntuh_registered_faculty = db.jntuh_registered_faculty
                 //                              .Where(rf => strRegNoS.Contains(rf.RegistrationNumber) && rf.Notin116 != true).ToList();

                 var jntuh_notin415facultys = db.jntuh_notin415faculty.Where(F => F.CollegeId == collegeid).ToList();
                 var Specializations = db.jntuh_specialization.ToList();
                 string[] strREG = jntuh_notin415facultys.Select(F => F.RegistrationNumber.Trim()).ToArray();
                 string RegNumber = "";
                 int? Specializationid = 0;
                 string Reason1 = "";
                 foreach (var a in jntuh_registered_faculty)
                 {
                     string Reason = String.Empty;
                     Reason1 = "";
                     Specializationid = jntuh_college_faculty_registered.Where(C => C.RegistrationNumber.Trim() == a.RegistrationNumber.Trim()).Select(C => C.SpecializationId).FirstOrDefault();
                     var faculty = new FacultyRegistration();
                     faculty.CollegeCode = jntuh_college.FirstOrDefault(i => i.id == collegeid).collegeCode;
                     faculty.CollegeName = jntuh_college.FirstOrDefault(i => i.id == collegeid).collegeName;
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
                     faculty.SamePANNumberCount = jntuh_registered_faculty.Where(f => f.PANNumber == a.PANNumber).ToList().Count;
                     faculty.SameAadhaarNumberCount = jntuh_registered_faculty.Where(f => f.AadhaarNumber == a.AadhaarNumber).ToList().Count;
                     faculty.SpecializationIdentfiedFor = Specializationid > 0 ? Specializations.Where(S => S.id == Specializationid).Select(S => S.specializationName).FirstOrDefault() : "";
                     faculty.isVerified = isFacultyVerified(a.id);
                     faculty.DeactivationReason = a.DeactivationReason;
                     faculty.FacultyVerificationStatus = a.FacultyVerificationStatus;
                     faculty.updatedOn = a.updatedOn;
                     faculty.createdOn = jntuh_college_faculty_registered.Where(f => f.RegistrationNumber == a.RegistrationNumber).Select(f => f.createdOn).FirstOrDefault();
                     faculty.IdentfiedFor = jntuh_college_faculty_registered.Where(f => f.RegistrationNumber.Trim() == a.RegistrationNumber.Trim()).Select(f => f.IdentifiedFor).FirstOrDefault();
                     faculty.jntuh_registered_faculty_education = a.jntuh_registered_faculty_education;
                     faculty.DegreeId = a.jntuh_registered_faculty_education.Count(e => e.facultyId == a.id) > 0 ? a.jntuh_registered_faculty_education.Where(e => e.facultyId == a.id).Select(e => e.educationId).Max() : 0;
                     faculty.PanDeactivationReasion = !string.IsNullOrEmpty(a.PanDeactivationReason) ? a.PanDeactivationReason : "";
                     faculty.Absent = a.Absent != null ? (bool)a.Absent : false;
                     faculty.BlacklistFaculty = a.Blacklistfaculy != null ? (bool)a.Blacklistfaculy : false;
                     faculty.PHDundertakingnotsubmitted = a.PHDundertakingnotsubmitted != null ? (bool)a.PHDundertakingnotsubmitted : false;
                     faculty.NOTQualifiedAsPerAICTE = a.NotQualifiedAsperAICTE != null ? (bool)a.NotQualifiedAsperAICTE : false;
                     faculty.InvalidPANNo = a.InvalidPANNumber != null ? (bool)a.InvalidPANNumber : false; ;
                     faculty.InCompleteCeritificates = a.IncompleteCertificates != null ? (bool)a.IncompleteCertificates : false; ;
                     faculty.FalsePAN = a.FalsePAN != null ? (bool)a.FalsePAN : false;
                     faculty.NoSCM = a.NoSCM != null ? (bool)a.NoSCM : false;
                     faculty.Eid = a.jntuh_registered_faculty_education.Where(e => e.facultyId == a.id).Select(e => e.educationId).Count() > 0 ? a.jntuh_registered_faculty_education.Where(e => e.facultyId == a.id).Select(e => e.educationId).Max() : 0;
                     if (faculty.Absent == true)
                     {
                         Reason1 = "Absent" + ",";
                     }
                     if (faculty.NOTQualifiedAsPerAICTE == true || faculty.DegreeId < 4)
                     {
                         Reason1 += "NOT QUALIFIED " + ",";
                     }
                     if (faculty.NoSCM == true)
                     {
                         Reason1 += "NO SCM" + ",";
                     }
                     if (string.IsNullOrEmpty(faculty.PANNumber))
                     {
                         Reason1 += "NO PAN" + ",";
                     }
                     if (faculty.department == null)
                     {
                         Reason1 += "No Department" + ",";
                     }
                     if (faculty.PHDundertakingnotsubmitted == true)
                     {
                         Reason1 += "No Undertaking" + ",";
                     }
                     if (faculty.BlacklistFaculty == true)
                     {
                         Reason1 += "Blacklisted" + ",";
                     }
                     if (faculty.Type == "Adjunct")
                     {
                         Reason1 += "Adjunct Faculty" + ",";
                     }

                     if (Reason1 != "")
                     {
                         Reason1 = Reason1.Substring(0, Reason1.Length - 1);
                         faculty.DeactivationNew = Reason1;
                     }
                     else
                         faculty.DeactivationNew = "";

                     teachingFaculty.Add(faculty);
                 }
                 teachingFaculty = teachingFaculty.Where(rf => rf.DeactivationNew == "" && rf.isActive == true).OrderBy(f => f.department).ToList();//(f => f.updatedOn).ThenBy
                 if (type == "Excel")
                 {
                     Response.ClearContent();
                     Response.Buffer = true;
                     Response.AddHeader("content-disposition", "attachment; filename=" + jntuh_collegeCode + "- " + deptId + "-Considered Faculty Details.xls");
                     Response.ContentType = "application/vnd.ms-excel";
                     return PartialView("~/Views/Reports/_ConsideredFacultyDataEntryExportExcel.cshtml", teachingFaculty);

                 }
                 // return View(teachingFaculty);
             }
             return RedirectToAction("Index");
         }

         [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification,DataEntry")]
         public ActionResult NotConsideredFacultyDataEntryExportExcel(int? collegeid, string type, string deptId)
         {
             var jntuh_department = db.jntuh_department.ToList();
             var jntuh_college = db.jntuh_college.ToList();
             var jntuh_collegeCode = db.jntuh_college.Where(C => C.id == collegeid).Select(C => C.collegeCode).FirstOrDefault();
             List<FacultyRegistration> teachingFaculty = new List<FacultyRegistration>();

             if (collegeid != null)
             {
                 List<jntuh_college_faculty_registered> jntuh_college_faculty_registered = db.jntuh_college_faculty_registered.Where(cf => cf.collegeId == collegeid).Select(cf => cf).ToList();
                 string[] strRegNoS = db.jntuh_college_faculty_registered.Where(cf => cf.collegeId == collegeid).Select(cf => cf.RegistrationNumber).ToArray();



                 var ComplianceFaculty = db.jntuh_appeal_faculty_registered.Where(e => e.NOtificationReport != null && e.isActive == false && e.collegeId == collegeid).Select(e => e).ToList();

                 var ComplianceFaculty2 = db.jntuh_appeal_faculty_registered.Where(e => e.NOtificationReport != null && e.isActive == false && e.collegeId == collegeid).Select(e => e.RegistrationNumber.Trim()).ToArray();
                 //var Regino = strRegNoS.Union(ComplianceFaculty).ToArray();
                 List<jntuh_registered_faculty> jntuh_registered_faculty = new List<jntuh_registered_faculty>();
                 if (deptId != null)
                 {
                     var DeptIds1 = jntuh_department.Where(e => e.departmentName.Trim() == deptId.Trim()).Select(e => e.id).ToArray();

                     var ComplianceFaculty1 = ComplianceFaculty.Where(e => DeptIds1.Contains((int)e.DepartmentId)).Select(e => e.RegistrationNumber).ToArray();
                     var Regino = strRegNoS.Union(ComplianceFaculty1).ToArray();

                     jntuh_registered_faculty = db.jntuh_registered_faculty
                                                .Where(rf => Regino.Contains(rf.RegistrationNumber) && rf.Notin116 != true && DeptIds1.Contains((int)rf.DepartmentId))  //&& (rf.collegeId == null || rf.collegeId == collegeid)
                                                .ToList();

                 }
                 else
                 {
                     var Regino1 = strRegNoS.Union(ComplianceFaculty2).ToArray();
                     jntuh_registered_faculty = db.jntuh_registered_faculty
                                             .Where(rf => Regino1.Contains(rf.RegistrationNumber) && rf.Notin116 != true)  //&& (rf.collegeId == null || rf.collegeId == collegeid)
                                             .ToList();

                 }
                 //jntuh_registered_faculty = db.jntuh_registered_faculty
                 //                             .Where(rf => strRegNoS.Contains(rf.RegistrationNumber) && rf.Notin116 != true).ToList();

                 var jntuh_notin415facultys = db.jntuh_notin415faculty.Where(F => F.CollegeId == collegeid).ToList();
                 var Specializations = db.jntuh_specialization.ToList();
                 string[] strREG = jntuh_notin415facultys.Select(F => F.RegistrationNumber.Trim()).ToArray();
                 string RegNumber = "";
                 int? Specializationid = 0;
                 string Reason1 = "";
                 foreach (var a in jntuh_registered_faculty)
                 {
                     string Reason = String.Empty;
                     Reason1 = "";
                     Specializationid = jntuh_college_faculty_registered.Where(C => C.RegistrationNumber.Trim() == a.RegistrationNumber.Trim()).Select(C => C.SpecializationId).FirstOrDefault();
                     var faculty = new FacultyRegistration();
                     faculty.CollegeCode = jntuh_college.FirstOrDefault(i => i.id == collegeid).collegeCode;
                     faculty.CollegeName = jntuh_college.FirstOrDefault(i => i.id == collegeid).collegeName;
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
                     faculty.SamePANNumberCount = jntuh_registered_faculty.Where(f => f.PANNumber == a.PANNumber).ToList().Count;
                     faculty.SameAadhaarNumberCount = jntuh_registered_faculty.Where(f => f.AadhaarNumber == a.AadhaarNumber).ToList().Count;
                     faculty.SpecializationIdentfiedFor = Specializationid > 0 ? Specializations.Where(S => S.id == Specializationid).Select(S => S.specializationName).FirstOrDefault() : "";
                     faculty.isVerified = isFacultyVerified(a.id);
                     faculty.DeactivationReason = a.DeactivationReason;
                     faculty.FacultyVerificationStatus = a.FacultyVerificationStatus;
                     faculty.updatedOn = a.updatedOn;
                     faculty.createdOn = jntuh_college_faculty_registered.Where(f => f.RegistrationNumber == a.RegistrationNumber).Select(f => f.createdOn).FirstOrDefault();
                     faculty.IdentfiedFor = jntuh_college_faculty_registered.Where(f => f.RegistrationNumber.Trim() == a.RegistrationNumber.Trim()).Select(f => f.IdentifiedFor).FirstOrDefault();
                     faculty.jntuh_registered_faculty_education = a.jntuh_registered_faculty_education;
                     faculty.DegreeId = a.jntuh_registered_faculty_education.Count(e => e.facultyId == a.id) > 0 ? a.jntuh_registered_faculty_education.Where(e => e.facultyId == a.id).Select(e => e.educationId).Max() : 0;
                     faculty.PanDeactivationReasion = !string.IsNullOrEmpty(a.PanDeactivationReason) ? a.PanDeactivationReason : "";
                     faculty.Absent = a.Absent != null ? (bool)a.Absent : false;
                     faculty.BlacklistFaculty = a.Blacklistfaculy != null ? (bool)a.Blacklistfaculy : false;
                     faculty.PHDundertakingnotsubmitted = a.PHDundertakingnotsubmitted != null ? (bool)a.PHDundertakingnotsubmitted : false;
                     faculty.NOTQualifiedAsPerAICTE = a.NotQualifiedAsperAICTE != null ? (bool)a.NotQualifiedAsperAICTE : false;
                     faculty.InvalidPANNo = a.InvalidPANNumber != null ? (bool)a.InvalidPANNumber : false; ;
                     faculty.InCompleteCeritificates = a.IncompleteCertificates != null ? (bool)a.IncompleteCertificates : false; ;
                     faculty.FalsePAN = a.FalsePAN != null ? (bool)a.FalsePAN : false;
                     faculty.NoSCM = a.NoSCM != null ? (bool)a.NoSCM : false;
                     faculty.Eid = a.jntuh_registered_faculty_education.Where(e => e.facultyId == a.id).Select(e => e.educationId).Count() > 0 ? a.jntuh_registered_faculty_education.Where(e => e.facultyId == a.id).Select(e => e.educationId).Max() : 0;
                     if (faculty.Absent == true)
                     {
                         Reason1 = "Absent" + ",";
                     }
                     if (faculty.NOTQualifiedAsPerAICTE == true || faculty.DegreeId < 4)
                     {
                         Reason1 += "NOT QUALIFIED " + ",";
                     }
                     if (faculty.NoSCM == true)
                     {
                         Reason1 += "NO SCM" + ",";
                     }
                     if (string.IsNullOrEmpty(faculty.PANNumber))
                     {
                         Reason1 += "NO PAN" + ",";
                     }
                     if (faculty.department == null)
                     {
                         Reason1 += "No Department" + ",";
                     }
                     if (faculty.PHDundertakingnotsubmitted == true)
                     {
                         Reason1 += "No Undertaking" + ",";
                     }
                     if (faculty.BlacklistFaculty == true)
                     {
                         Reason1 += "Blacklisted" + ",";
                     }
                     if (faculty.Type == "Adjunct")
                     {
                         Reason1 += "Adjunct Faculty" + ",";
                     }

                     if (Reason1 != "")
                     {
                         Reason1 = Reason1.Substring(0, Reason1.Length - 1);
                         faculty.DeactivationNew = Reason1;
                     }
                     else
                     {
                         if (ComplianceFaculty2.Contains(a.RegistrationNumber.Trim()))
                             faculty.DeactivationNew = "Compliance";
                         else
                             faculty.DeactivationNew = "";
                     }


                     teachingFaculty.Add(faculty);
                 }
                 teachingFaculty = teachingFaculty.Where(rf => rf.DeactivationNew != "" && rf.isActive == true).OrderBy(f => f.department).ToList();//(f => f.updatedOn).ThenBy
                 if (type == "Excel")
                 {
                     Response.ClearContent();
                     Response.Buffer = true;
                     Response.AddHeader("content-disposition", "attachment; filename=" + jntuh_collegeCode + "- " + deptId + "-Not Considered Faculty Details.xls");
                     Response.ContentType = "application/vnd.ms-excel";
                     return PartialView("~/Views/Reports/_NotConsideredFacultyDataEntryExportExcel.cshtml", teachingFaculty);

                 }
                 // return View(teachingFaculty);
             }
             return RedirectToAction("Index");
         }


         [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification")]
         public ActionResult MinorityColleges(int? collegeid)
         {

             ViewBag.Colleges = db.jntuh_college.Join(db.jntuh_college_edit_status,C=>C.id,ES=>ES.collegeId,(C,ES)=>new{C=C,ES=ES}).Where(e=>e.C.isActive==true && e.C.collegeStatusID==1 && e.ES.IsCollegeEditable==false).Select(c => new { collegeId = c.C.id, collegeName = c.C.collegeCode + "-" + c.C.collegeName }).OrderBy(c => c.collegeName).ToList();
             ViewBag.collegeid = collegeid;
             var jntuh_department = db.jntuh_department.ToList();
             List<FacultyRegistration> teachingFaculty = new List<FacultyRegistration>();

             if (collegeid != null)
             {
                 List<jntuh_college_faculty_registered> jntuh_college_faculty_registered = db.jntuh_college_faculty_registered.Where(cf => cf.collegeId == collegeid && cf.createdOn.Value.Year==2017).Select(cf => cf).ToList();
                 string[] strRegNoS = db.jntuh_college_faculty_registered.Where(cf => cf.collegeId == collegeid &&  cf.createdOn.Value.Year==2017).Select(cf => cf.RegistrationNumber).ToArray();
                 string[] PrincipalstrRegNoS = db.jntuh_college_principal_registered.Where(P => P.collegeId == collegeid).Select(P => P.RegistrationNumber.Trim()).ToArray();
                 List<jntuh_registered_faculty> jntuh_registered_faculty = new List<jntuh_registered_faculty>();
                 jntuh_registered_faculty = db.jntuh_registered_faculty.Where(rf => strRegNoS.Contains(rf.RegistrationNumber))  //&& (rf.collegeId == null || rf.collegeId == collegeid)//&& rf.Notin116 != true
                                              .ToList();

                 var jntuh_notin415facultys = db.jntuh_notin415faculty.Where(F => F.CollegeId == collegeid).ToList();
                 var Specializations = db.jntuh_specialization.ToList();
                 string[] strREG = jntuh_notin415facultys.Select(F => F.RegistrationNumber.Trim()).ToArray();
                 string RegNumber = "";
                 int? Specializationid = 0;
                 foreach (var a in jntuh_registered_faculty)
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
                     if (PrincipalstrRegNoS.Contains(a.RegistrationNumber.Trim()))
                         faculty.Principal = "Principal";
                     else
                         faculty.Principal = "";


                     faculty.GenderId = a.GenderId;
                     faculty.Email = a.Email;
                     faculty.facultyPhoto = a.Photo;
                     faculty.Mobile = a.Mobile;
                     faculty.PANNumber = a.PANNumber;
                     faculty.AadhaarNumber = a.AadhaarNumber;
                     faculty.isActive = a.isActive;
                     faculty.isApproved = a.isApproved;
                     faculty.department = jntuh_department.Where(d => d.id == a.DepartmentId).Select(d => d.departmentName).FirstOrDefault();
                     faculty.SamePANNumberCount = jntuh_registered_faculty.Where(f => f.PANNumber == a.PANNumber).ToList().Count;
                     faculty.SameAadhaarNumberCount = jntuh_registered_faculty.Where(f => f.AadhaarNumber == a.AadhaarNumber).ToList().Count;
                     faculty.SpecializationIdentfiedFor = Specializationid > 0 ? Specializations.Where(S => S.id == Specializationid).Select(S => S.specializationName).FirstOrDefault() : "";
                     faculty.isVerified = isFacultyVerified(a.id);
                     faculty.DeactivationReason = a.DeactivationReason;
                     faculty.FacultyVerificationStatus = a.FacultyVerificationStatus;
                     faculty.updatedOn = a.updatedOn;
                     faculty.createdOn = jntuh_college_faculty_registered.Where(f => f.RegistrationNumber == a.RegistrationNumber).Select(f => f.createdOn).FirstOrDefault();
                     faculty.IdentfiedFor = jntuh_college_faculty_registered.Where(f => f.RegistrationNumber.Trim() == a.RegistrationNumber.Trim()).Select(f => f.IdentifiedFor).FirstOrDefault();
                     faculty.jntuh_registered_faculty_education = a.jntuh_registered_faculty_education;
                     faculty.DegreeId = a.jntuh_registered_faculty_education.Count(e => e.facultyId == a.id) > 0 ? a.jntuh_registered_faculty_education.Where(e => e.facultyId == a.id).Select(e => e.educationId).Max() : 0;
                     faculty.PanDeactivationReasion = !string.IsNullOrEmpty(a.PanDeactivationReason) ? a.PanDeactivationReason : "";
                     faculty.Absent = a.Absent != null ? (bool)a.Absent : false;
                     faculty.BlacklistFaculty = a.Blacklistfaculy != null ? (bool)a.Blacklistfaculy : false;
                     faculty.PHDundertakingnotsubmitted = a.PHDundertakingnotsubmitted != null ? (bool)a.PHDundertakingnotsubmitted : false;
                     faculty.NOTQualifiedAsPerAICTE = a.NotQualifiedAsperAICTE != null ? (bool)a.NotQualifiedAsperAICTE : false;
                     faculty.InvalidPANNo = a.InvalidPANNumber != null ? (bool)a.InvalidPANNumber : false; ;
                     faculty.InCompleteCeritificates = a.IncompleteCertificates != null ? (bool)a.IncompleteCertificates : false; ;
                     faculty.FalsePAN = a.FalsePAN != null ? (bool)a.FalsePAN : false;
                     faculty.NOForm16 = a.NoForm16 != null ? (bool)a.NoForm16 : false;


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


         [HttpPost]
         [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification")]
         public ActionResult FacultyVerificationPostDENew(FacultyRegistration faculty)
         {
             int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
             var facultydetails = db.jntuh_registered_faculty.Where(i => i.RegistrationNumber == faculty.RegistrationNumber).FirstOrDefault();
             if (facultydetails != null)
             {
                 if (faculty.Absent == true)
                     facultydetails.Absent = faculty.Absent;
                 //if (faculty.ModifiedPANNo != null)
                 //{
                 //    facultydetails.PANNumber = faculty.ModifiedPANNo;
                 //    facultydetails.ModifiedPANNumber = faculty.ModifiedPANNo;
                 //}
                 //if (faculty.MOdifiedDateofAppointment1 != null)
                 //{
                 //    facultydetails.DateOfAppointment = Convert.ToDateTime(faculty.MOdifiedDateofAppointment1);
                 //}
                 //if (faculty.DepartmentId != null)
                 //{
                 //    facultydetails.DepartmentId = faculty.DepartmentId;
                 //}
                 //facultydetails.InvalidPANNumber = faculty.InvalidPANNo;
                 //facultydetails.NoRelevantUG = faculty.NORelevantUG;
                 //facultydetails.NoRelevantPG = faculty.NORelevantPG;
                 //facultydetails.NORelevantPHD = faculty.NORelevantPHD;
                 // facultydetails.NoSCM = faculty.NoSCM;
                 if (faculty.NOForm16 == true)
                     facultydetails.NoForm16 = faculty.NOForm16;
                 //facultydetails.NotQualifiedAsperAICTE = faculty.NOTQualifiedAsPerAICTE;
                 //facultydetails.MultipleRegInSameCollege = faculty.MultipleReginSamecoll;
                 //facultydetails.MultipleRegInDiffCollege = faculty.MultipleReginDiffcoll;
                 //facultydetails.SamePANUsedByMultipleFaculty = faculty.SamePANUsedByMultipleFaculty;
                 //facultydetails.PhotoCopyofPAN = faculty.PhotocopyofPAN;
                 //facultydetails.AppliedPAN = faculty.AppliedPAN;
                 //facultydetails.LostPAN = faculty.LostPAN;
                 //facultydetails.OriginalsVerifiedUG = faculty.OriginalsVerifiedUG;
                 //facultydetails.OriginalsVerifiedPG = faculty.OriginalsVerifiedPG;
                 //facultydetails.OriginalsVerifiedPHD = faculty.OriginalsVerifiedPHD;
                 if (faculty.InCompleteCeritificates == true)
                     facultydetails.IncompleteCertificates = faculty.InCompleteCeritificates;

                 facultydetails.FacultyVerificationStatus = true;
                 facultydetails.DeactivatedBy = userID;
                 facultydetails.DeactivatedOn = DateTime.Now;
                 db.Entry(facultydetails).State = EntityState.Modified;
                 db.SaveChanges();

             }

             return RedirectToAction("MinorityColleges", "FacultyVerificationDENewReports", new { collegeid = faculty.CollegeId });
         }


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
                 jntuh_registered_faculty faculty = db.jntuh_registered_faculty.Find(fID);
                 regFaculty.id = fID;
                 regFaculty.Type = faculty.type;
                 regFaculty.RegistrationNumber = faculty.RegistrationNumber;
                 regFaculty.Email = faculty.Email;
                 regFaculty.FirstName = faculty.FirstName;
                 regFaculty.MiddleName = faculty.MiddleName;
                 regFaculty.LastName = faculty.LastName;
                 regFaculty.facultyPhoto = faculty.Photo;
                 regFaculty.Absent = faculty.Absent ?? false;
                 regFaculty.InvalidPANNo = faculty.InvalidPANNumber != false ? true : false;
                 regFaculty.NoSCM = faculty.NoSCM != false ? true : false;
                 regFaculty.NORelevantPG = faculty.NoRelevantPG;
                 regFaculty.NOForm16 = faculty.NoForm16 ?? false;
                 regFaculty.NOTQualifiedAsPerAICTE = faculty.NotQualifiedAsperAICTE ?? false;
                 regFaculty.ModifiedPANNo = faculty.ModifiedPANNumber;
                 regFaculty.InCompleteCeritificates = faculty.IncompleteCertificates ?? false;
                 regFaculty.DeactivationReason = faculty.DeactivationReason;
                 regFaculty.PanDeactivationReasion = faculty.PanDeactivationReason;
                 regFaculty.PanVerificationStatus = faculty.PanVerificationStatus;
             }
             return PartialView("_FacultyVerificationCheck", regFaculty);
         }


    }
}
