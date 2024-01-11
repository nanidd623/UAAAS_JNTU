using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using UAAAS.Models;

namespace UAAAS.Controllers.College
{
    public class CollegeActiveInactiveFacultyController : BaseController
    {
        //
        // GET: /CollegeActiveInactiveFaculty/
        private uaaasDBContext db = new uaaasDBContext();
        [Authorize(Roles = "Admin")]
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult CollegewiseInactiveFaculty(int? collegeid)
        {
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy =
                db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            ViewBag.Colleges = db.jntuh_college.Join(db.jntuh_college_edit_status, co => co.id, e => e.collegeId, (co, e) => new { co = co, e = e }).Where(c => c.co.isActive == true&&c.e.academicyearId==prAy).Select(c => new { collegeId = c.co.id, collegeName = c.co.collegeCode + "-" + c.co.collegeName }).OrderBy(c => c.collegeName).ToList();
            ViewBag.collegeid = collegeid;
            var FacultyRegistrationList = new List<FacultyRegistration>();
            if (collegeid!=0)
            {
                var jntuh_college_faculty_registered = db.jntuh_college_faculty_registered.Where(cf => cf.collegeId == collegeid).ToList();
                string[] strRegnos = jntuh_college_faculty_registered.Select(cf => cf.RegistrationNumber).ToArray();


                var jntuh_college_faculty_registered_new = db.jntuh_college_principal_registered.Where(cf => cf.collegeId == collegeid).ToList();
                var principalRegno = jntuh_college_faculty_registered_new.Select(cf => cf.RegistrationNumber).FirstOrDefault();

                var registeredFaculty = principalRegno != null ? db.jntuh_registered_faculty.Where(rf => strRegnos.Contains(rf.RegistrationNumber.Trim())).ToList()
                   : db.jntuh_registered_faculty.Where(rf => strRegnos.Contains(rf.RegistrationNumber.Trim())).ToList();//&& rf.RegistrationNumber != principalRegno && rf.DepartmentId == departmentid


                //education categoryIds UG,PG,PHD...........
                var jntuh_education_category = db.jntuh_education_category.ToList();

                var jntuh_registered_faculty1 = registeredFaculty.Select(rf => new
                {
                    Id = rf.id,
                    type = rf.type,
                    Absent = rf.Absent != null ? (bool)rf.Absent : false,
                    OriginalCertificatesnotshownFlag = rf.OriginalCertificatesNotShown != null ? (bool)rf.OriginalCertificatesNotShown : false,
                    XeroxcopyofcertificatesFlag = rf.Xeroxcopyofcertificates != null ? (bool)rf.Xeroxcopyofcertificates : false,
                    NotQualifiedAsperAICTE = rf.NotQualifiedAsperAICTE != null ? (bool)rf.NotQualifiedAsperAICTE : false,
                    NoSCM = rf.NoSCM != null ? (bool)rf.NoSCM : false,
                    InCompleteCeritificates = rf.IncompleteCertificates != null ? (bool)rf.IncompleteCertificates : false,
                    BlacklistFaculty = rf.Blacklistfaculy != null ? (bool)rf.Blacklistfaculy : false,
                    NOrelevantUgFlag = rf.NoRelevantUG == "Yes" ? true : false,
                    NOrelevantPgFlag = rf.NoRelevantPG == "Yes" ? true : false,
                    NOrelevantPhdFlag = rf.NORelevantPHD == "Yes" ? true : false,
                    InvalidPANNo = rf.InvalidPANNumber != null ? (bool)rf.InvalidPANNumber : false,
                    OriginalsVerifiedUG = rf.OriginalsVerifiedUG == true ? true : false,
                    OriginalsVerifiedPHD = rf.OriginalsVerifiedPHD == true ? true : false,
                    Invaliddegree = rf.Invaliddegree != null ? (bool)(rf.Invaliddegree) : false,
                    BAS = rf.BAS,
                    InvalidAadhaar = rf.InvalidAadhaar,
                    Noclass = rf.Noclass == true ? true : false,
                    VerificationStatus = rf.AbsentforVerification != null ? (bool)rf.AbsentforVerification : false,
                    NotconsideredPHD = rf.NotconsideredPHD == true ? true : false,
                    NoPGspecialization = rf.NoPGspecialization == true ? true : false,
                    Genuinenessnotsubmitted = rf.Genuinenessnotsubmitted == true ? true : false,
                    PANNumber = rf.PANNumber,
                    AbsentforVerification = rf.AbsentforVerification,
                    Blacklistfaculy = rf.Blacklistfaculy,
                    NotIdentityFiedForAnyProgramFlag = rf.NotIdentityfiedForanyProgram != null ? (bool)rf.NotIdentityfiedForanyProgram : false,

                    //PhotocopyofPAN = rf.PhotoCopyofPAN != null ? (bool)rf.PhotoCopyofPAN : false,
                    PhdUndertakingDocumentstatus = rf.PhdUndertakingDocumentstatus != null ? (bool)(rf.PhdUndertakingDocumentstatus) : false,
                    PHDUndertakingDocumentView = rf.PHDUndertakingDocument,
                    PhdUndertakingDocumentText = rf.PhdUndertakingDocumentText,
                    //AppliedPAN = rf.AppliedPAN != null ? (bool)(rf.AppliedPAN) : false,
                    Notin116 = rf.Notin116,
                    
                    RegistrationNumber = rf.RegistrationNumber,
                    Department = rf.jntuh_department != null ? rf.jntuh_department.departmentName : "",
                    HighestDegreeID = rf.jntuh_registered_faculty_education.Count() != 0 ? rf.jntuh_registered_faculty_education.Where(r => r.educationId != 8).Select(e => e.educationId).Max() : 0,
                    IsApproved = rf.isApproved,
                    PanNumber = rf.PANNumber,
                    AadhaarNumber = rf.AadhaarNumber,
                    Photo = rf.Photo,
                    FullName = rf.FirstName + " " + rf.MiddleName + " " + rf.LastName,
                    FacultyEducation = rf.jntuh_registered_faculty_education,
                    DegreeId = rf.jntuh_registered_faculty_education.Count(e => e.facultyId == rf.id) > 0 ? rf.jntuh_registered_faculty_education.Where(e => e.facultyId == rf.id && e.educationId != 8).Select(e => e.educationId).Max() : 0,
                    DepartmentId = rf.DepartmentId

                }).ToList();

                //var RegistrationNumbersCleared = jntuh_registered_faculty1.Where(rf => rf.type != "Adjunct" && rf.Absent == false && ((rf.NotQualifiedAsperAICTE == false || rf.NotQualifiedAsperAICTE == null) && (rf.NoSCM == false || rf.NoSCM == null) && (rf.PANNumber != null) && (rf.InCompleteCeritificates == false || rf.InCompleteCeritificates == null) && (rf.BlacklistFaculty == false) &&
                //                                 rf.OriginalCertificatesnotshownFlag == false && rf.NOrelevantUgFlag == false && rf.NOrelevantPgFlag == false && rf.NOrelevantPhdFlag == false && (rf.InvalidPANNo == false || rf.InvalidPANNo == null) && (rf.XeroxcopyofcertificatesFlag == false || rf.XeroxcopyofcertificatesFlag == null) &&
                //                                 (rf.Invaliddegree == false || rf.Invaliddegree == null) && rf.NoPGspecialization == false && rf.Noclass == false && (rf.VerificationStatus == false || rf.VerificationStatus == null) && rf.Genuinenessnotsubmitted == false && rf.NotconsideredPHD == false && rf.BAS != "Yes" && rf.InvalidAadhaar != "Yes" && rf.OriginalsVerifiedUG == false && rf.OriginalsVerifiedPHD == false && (rf.DepartmentId != 61 || rf.DepartmentId != 27) && rf.HighestDegreeID >= 4)).Select(e => e.RegistrationNumber).ToArray();
                var RegistrationNumbersCleared = jntuh_registered_faculty1.Where(rf => rf.type != "Adjunct" && (rf.BAS != "Yes") && (rf.Noclass == false || rf.Noclass == null) && (rf.Absent == false || rf.Absent == null) &&
                                                         (rf.NoSCM == false || rf.NoSCM == null) && (rf.Blacklistfaculy == false) && (rf.AbsentforVerification == false || rf.AbsentforVerification == null) && (rf.DepartmentId != 61 || rf.DepartmentId != 27) && rf.HighestDegreeID >= 4).Select(e => e.RegistrationNumber).ToArray();

                var jntuh_registered_faculty = jntuh_registered_faculty1.Where(e => !RegistrationNumbersCleared.Contains(e.RegistrationNumber)).Select(rf => new
                {
                    Id = rf.Id,
                    type = rf.type,
                    Absent = rf.Absent,


                    NotQualifiedAsperAICTE = rf.NotQualifiedAsperAICTE,
                    rf.InCompleteCeritificates,
                    rf.InvalidPANNo,
                    rf.NOrelevantPgFlag,
                    rf.NOrelevantUgFlag,
                    rf.NOrelevantPhdFlag,
                    rf.VerificationStatus,
                    rf.XeroxcopyofcertificatesFlag,
                    rf.NoSCM,
                    rf.NotconsideredPHD,
                    rf.Genuinenessnotsubmitted,
                    rf.NoPGspecialization,
                    rf.Noclass,
                    rf.PANNumber,
                    rf.NotIdentityFiedForAnyProgramFlag,
                    rf.InvalidAadhaar,
                    rf.BAS,
                    rf.OriginalsVerifiedUG,
                    rf.OriginalsVerifiedPHD,
                    rf.Invaliddegree,
                    rf.OriginalCertificatesnotshownFlag,
                    rf.BlacklistFaculty,

                    PHDundertakingnotsubmitted = rf.PhdUndertakingDocumentstatus,
                    Notin116 = rf.Notin116,
                    RegistrationNumber = rf.RegistrationNumber,
                    Department = rf.Department,
                    HighestDegree = jntuh_education_category.Where(c => c.id == rf.HighestDegreeID).Select(c => c.educationCategoryName).FirstOrDefault(),
                    Recruitedfor = jntuh_college_faculty_registered.Where(c => c.RegistrationNumber == rf.RegistrationNumber).Select(c => c.IdentifiedFor).FirstOrDefault(),
                    SpecializationId = jntuh_college_faculty_registered.Where(c => c.RegistrationNumber == rf.RegistrationNumber).Select(c => c.SpecializationId).FirstOrDefault(),
                    DeptId = jntuh_college_faculty_registered.Where(c => c.RegistrationNumber == rf.RegistrationNumber).Select(c => c.DepartmentId).FirstOrDefault(),
                    PanNumber = rf.PanNumber,
                    AadhaarNumber = rf.AadhaarNumber,
                    Photo = rf.Photo,
                    FullName = rf.FullName,
                    faculty_education = rf.FacultyEducation,
                    HighestDegreeID = rf.HighestDegreeID
                }).ToList();


                foreach (var item in jntuh_registered_faculty)
                {
                    string Reason = null;
                    FacultyRegistration facultyregistered = new FacultyRegistration();
                    facultyregistered.id = item.Id;
                    facultyregistered.RegistrationNumber = item.RegistrationNumber;
                    facultyregistered.FirstName = item.FullName;
                    facultyregistered.department = item.Department;
                    facultyregistered.DepartmentId = item.DeptId;
                    facultyregistered.jntuh_registered_faculty_education = item.faculty_education;
                    facultyregistered.facultyPhoto = item.Photo;
                    facultyregistered.Absent = item.Absent != null && (bool)item.Absent;
                    facultyregistered.NOTQualifiedAsPerAICTE = item.NotQualifiedAsperAICTE != null && (bool)item.NotQualifiedAsperAICTE;
                    facultyregistered.NoSCM = item.NoSCM != null && (bool)item.NoSCM;
                    facultyregistered.PANNumber = item.PANNumber;
                    facultyregistered.PHDundertakingnotsubmitted = item.PHDundertakingnotsubmitted != null && (bool)item.PHDundertakingnotsubmitted;
                    facultyregistered.BlacklistFaculty = item.BlacklistFaculty != null && (bool)item.BlacklistFaculty;
                    facultyregistered.DegreeId = item.HighestDegreeID;
                    var principal =
                        jntuh_college_faculty_registered_new.Where(
                            r => r.RegistrationNumber == item.RegistrationNumber.Trim() && r.collegeId == collegeid)
                            .Select(s => s.RegistrationNumber)
                            .FirstOrDefault();
                    if (!String.IsNullOrEmpty(principal))
                        facultyregistered.Principal = "Principal";
                    else
                        facultyregistered.Principal = string.Empty;
                    if (item.Absent == true)
                        Reason += "Absent";
                    //if (item.type == "Adjunct")
                    //{
                    //    if (Reason != null)
                    //        Reason += ",Adjunct Faculty";
                    //    else
                    //        Reason += "Adjunct Faculty";
                    //}
                    //if (item.OriginalCertificatesnotshownFlag == true)
                    //{
                    //    if (Reason != null)
                    //        Reason += ",Orginal Certificates Not shown in College Inspection";
                    //    else
                    //        Reason += "Orginal Certificates Not shown in College Inspection";
                    //}
                    //if (item.XeroxcopyofcertificatesFlag == true)
                    //{
                    //    if (Reason != null)
                    //        Reason += ",Photo copy of Certificates";
                    //    else
                    //        Reason += "Photo copy of Certificates";
                    //}
                    //if (item.NotQualifiedAsperAICTE == true || item.HighestDegreeID < 4)
                    //{
                    //    if (Reason != null)
                    //        Reason += ",Not Qualified as per AICTE/PCI";
                    //    else
                    //        Reason += "Not Qualified as per AICTE/PCI";
                    //}
                    //if (item.NoSCM == true)
                    //{
                    //    if (Reason != null)
                    //        Reason += ",no/not valid SCM";
                    //    else
                    //        Reason += "no/not valid SCM";
                    //}
                    //if (item.PANNumber == null)
                    //{
                    //    if (Reason != null)
                    //        Reason += ",No PAN Number";
                    //    else
                    //        Reason += "No PAN Number";
                    //}
                    //if (item.InCompleteCeritificates == true)
                    //{
                    //    if (Reason != null)
                    //        Reason += ",IncompleteCertificates";
                    //    else
                    //        Reason += "IncompleteCertificates";
                    //}
                    //if (item.BlacklistFaculty == true)
                    //{
                    //    if (Reason != null)
                    //        Reason += ",Blacklisted Faculty";
                    //    else
                    //        Reason += "Blacklisted Faculty";
                    //}
                    //if (item.NOrelevantUgFlag == true)
                    //{
                    //    if (Reason != null)
                    //        Reason += ",NO Relevant UG";
                    //    else
                    //        Reason += "NO Relevant UG";
                    //}
                    //if (item.NOrelevantPgFlag == true)
                    //{
                    //    if (Reason != null)
                    //        Reason += ",NO Relevant PG";
                    //    else
                    //        Reason += "NO Relevant PG";
                    //}
                    //if (item.NOrelevantPhdFlag == true)
                    //{
                    //    if (Reason != null)
                    //        Reason += ",NO Relevant PHD";
                    //    else
                    //        Reason += "NO Relevant PHD";
                    //}
                    //if (item.InvalidPANNo == true)
                    //{
                    //    if (Reason != null)
                    //        Reason += ",InvalidPAN";
                    //    else
                    //        Reason += "InvalidPAN";
                    //}
                    //if (item.OriginalsVerifiedPHD == true)
                    //{
                    //    if (Reason != null)
                    //        Reason += ",No Guide Sign in PHD Thesis";
                    //    else
                    //        Reason += "No Guide Sign in PHD Thesis";
                    //}
                    //if (item.OriginalsVerifiedUG == true)
                    //{
                    //    if (Reason != null)
                    //        Reason += ",Complaint PHD Faculty";
                    //    else
                    //        Reason += "Complaint PHD Faculty";
                    //}
                    //if (item.Invaliddegree == true)
                    //{
                    //    if (Reason != null)
                    //        Reason += ",AICTE Not Approved University Degrees";
                    //    else
                    //        Reason += "AICTE Not Approved University Degrees";
                    //}
                    if (item.BAS == "Yes")
                    {
                        if (Reason != null)
                            Reason += ",Not having Sufficient Biometric Attendance";
                        else
                            Reason += "Not having Sufficient Biometric Attendance";
                    }
                    //if (item.InvalidAadhaar == "Yes")
                    //{
                    //    if (Reason != null)
                    //        Reason += ",Invalid/Blur Aadhaar";
                    //    else
                    //        Reason += "Invalid/Blur Aadhaar";
                    //}
                    //if (item.Noclass == true)
                    //{
                    //    if (Reason != null)
                    //        Reason += ",No Class in UG/PG";
                    //    else
                    //        Reason += "No Class in UG/PG";
                    //}
                    //if (item.VerificationStatus == true)
                    //{
                    //    if (Reason != null)
                    //        Reason += ",Absentfor Physical Verification";
                    //    else
                    //        Reason += "Absentfor Physical Verification";
                    //}
                    //if (item.NotconsideredPHD == true)
                    //{
                    //    if (Reason != null)
                    //        Reason += ",Cosidered as a faculty but not cosidered his/her P.hD";
                    //    else
                    //        Reason += "Cosidered as a faculty but not cosidered his/her P.hD";
                    //}
                    //if (item.NoPGspecialization == true)
                    //{
                    //    if (Reason != null)
                    //        Reason += ",no Specialization in PG";
                    //    else
                    //        Reason += "no Specialization in PG";
                    //}
                    //if (item.Genuinenessnotsubmitted == true)
                    //{
                    //    if (Reason != null)
                    //        Reason += ",PHD Genuinity not Submitted";
                    //    else
                    //        Reason += "PHD Genuinity not Submitted";
                    //}
                    facultyregistered.DeactivationReason = Reason;
                    FacultyRegistrationList.Add(facultyregistered);
                }
            }
            return View(FacultyRegistrationList);
        }

        [Authorize(Roles = "Admin")]
        public ActionResult CollegewiseInactiveFacultyExport(string collegeid)
        {
            int cid = Convert.ToInt32(collegeid);
            var FacultyRegistrationList = new List<FacultyRegistration>();
            jntuh_college jntuhcollege= new jntuh_college();
            if (cid!=0)
            {
                jntuhcollege = db.jntuh_college.Where(c => c.id == cid).FirstOrDefault();
                var jntuh_college_faculty_registered = db.jntuh_college_faculty_registered.Where(cf => cf.collegeId == cid).ToList();
                string[] strRegnos = jntuh_college_faculty_registered.Select(cf => cf.RegistrationNumber).ToArray();


                var jntuh_college_faculty_registered_new = db.jntuh_college_principal_registered.Where(cf => cf.collegeId == cid).ToList();
                var principalRegno = jntuh_college_faculty_registered_new.Select(cf => cf.RegistrationNumber).FirstOrDefault();

                var registeredFaculty = principalRegno != null ? db.jntuh_registered_faculty.Where(rf => strRegnos.Contains(rf.RegistrationNumber.Trim())).ToList()
                   : db.jntuh_registered_faculty.Where(rf => strRegnos.Contains(rf.RegistrationNumber.Trim())).ToList();//&& rf.RegistrationNumber != principalRegno && rf.DepartmentId == departmentid


                //education categoryIds UG,PG,PHD...........
                var jntuh_education_category = db.jntuh_education_category.ToList();

                var jntuh_registered_faculty1 = registeredFaculty.Select(rf => new
                {
                    Id = rf.id,
                    type = rf.type,
                    Absent = rf.Absent != null ? (bool)rf.Absent : false,
                    OriginalCertificatesnotshownFlag = rf.OriginalCertificatesNotShown != null ? (bool)rf.OriginalCertificatesNotShown : false,
                    XeroxcopyofcertificatesFlag = rf.Xeroxcopyofcertificates != null ? (bool)rf.Xeroxcopyofcertificates : false,
                    NotQualifiedAsperAICTE = rf.NotQualifiedAsperAICTE != null ? (bool)rf.NotQualifiedAsperAICTE : false,
                    NoSCM = rf.NoSCM != null ? (bool)rf.NoSCM : false,
                    InCompleteCeritificates = rf.IncompleteCertificates != null ? (bool)rf.IncompleteCertificates : false,
                    BlacklistFaculty = rf.Blacklistfaculy != null ? (bool)rf.Blacklistfaculy : false,
                    NOrelevantUgFlag = rf.NoRelevantUG == "Yes" ? true : false,
                    NOrelevantPgFlag = rf.NoRelevantPG == "Yes" ? true : false,
                    NOrelevantPhdFlag = rf.NORelevantPHD == "Yes" ? true : false,
                    InvalidPANNo = rf.InvalidPANNumber != null ? (bool)rf.InvalidPANNumber : false,
                    OriginalsVerifiedUG = rf.OriginalsVerifiedUG == true ? true : false,
                    OriginalsVerifiedPHD = rf.OriginalsVerifiedPHD == true ? true : false,
                    Invaliddegree = rf.Invaliddegree != null ? (bool)(rf.Invaliddegree) : false,
                    BAS = rf.BAS,
                    InvalidAadhaar = rf.InvalidAadhaar,
                    Noclass = rf.Noclass == true ? true : false,
                    VerificationStatus = rf.AbsentforVerification != null ? (bool)rf.AbsentforVerification : false,
                    NotconsideredPHD = rf.NotconsideredPHD == true ? true : false,
                    NoPGspecialization = rf.NoPGspecialization == true ? true : false,
                    Genuinenessnotsubmitted = rf.Genuinenessnotsubmitted == true ? true : false,
                    PANNumber = rf.PANNumber,
                    AbsentforVerification = rf.AbsentforVerification,
                    Blacklistfaculy = rf.Blacklistfaculy,

                    NotIdentityFiedForAnyProgramFlag = rf.NotIdentityfiedForanyProgram != null ? (bool)rf.NotIdentityfiedForanyProgram : false,

                    //PhotocopyofPAN = rf.PhotoCopyofPAN != null ? (bool)rf.PhotoCopyofPAN : false,
                    PhdUndertakingDocumentstatus = rf.PhdUndertakingDocumentstatus != null ? (bool)(rf.PhdUndertakingDocumentstatus) : false,
                    PHDUndertakingDocumentView = rf.PHDUndertakingDocument,
                    PhdUndertakingDocumentText = rf.PhdUndertakingDocumentText,
                    //AppliedPAN = rf.AppliedPAN != null ? (bool)(rf.AppliedPAN) : false,
                    Notin116 = rf.Notin116,

                    RegistrationNumber = rf.RegistrationNumber,
                    Department = rf.jntuh_department != null ? rf.jntuh_department.departmentName : "",
                    HighestDegreeID = rf.jntuh_registered_faculty_education.Count() != 0 ? rf.jntuh_registered_faculty_education.Where(r => r.educationId != 8).Select(e => e.educationId).Max() : 0,
                    IsApproved = rf.isApproved,
                    PanNumber = rf.PANNumber,
                    AadhaarNumber = rf.AadhaarNumber,
                    Photo = rf.Photo,
                    FullName = rf.FirstName + " " + rf.MiddleName + " " + rf.LastName,
                    FacultyEducation = rf.jntuh_registered_faculty_education,
                    DegreeId = rf.jntuh_registered_faculty_education.Count(e => e.facultyId == rf.id) > 0 ? rf.jntuh_registered_faculty_education.Where(e => e.facultyId == rf.id && e.educationId != 8).Select(e => e.educationId).Max() : 0,
                    DepartmentId = rf.DepartmentId

                }).ToList();

                //var RegistrationNumbersCleared = jntuh_registered_faculty1.Where(rf => rf.type != "Adjunct" && rf.Absent == false && ((rf.NotQualifiedAsperAICTE == false || rf.NotQualifiedAsperAICTE == null) && (rf.NoSCM == false || rf.NoSCM == null) && (rf.PANNumber != null) && (rf.InCompleteCeritificates == false || rf.InCompleteCeritificates == null) && (rf.BlacklistFaculty == false) &&
                //                                 rf.OriginalCertificatesnotshownFlag == false && rf.NOrelevantUgFlag == false && rf.NOrelevantPgFlag == false && rf.NOrelevantPhdFlag == false && (rf.InvalidPANNo == false || rf.InvalidPANNo == null) && (rf.XeroxcopyofcertificatesFlag == false || rf.XeroxcopyofcertificatesFlag == null) &&
                //                                 (rf.Invaliddegree == false || rf.Invaliddegree == null) && rf.NoPGspecialization == false && rf.Noclass == false && (rf.VerificationStatus == false || rf.VerificationStatus == null) && rf.Genuinenessnotsubmitted == false && rf.NotconsideredPHD == false && rf.BAS != "Yes" && rf.InvalidAadhaar != "Yes" && rf.OriginalsVerifiedUG == false && rf.OriginalsVerifiedPHD == false && (rf.DepartmentId != 61 || rf.DepartmentId != 27) && rf.HighestDegreeID >= 4)).Select(e => e.RegistrationNumber).ToArray();

                var RegistrationNumbersCleared = jntuh_registered_faculty1.Where(rf => rf.type != "Adjunct" && (rf.BAS != "Yes") && (rf.Noclass == false || rf.Noclass == null) && (rf.Absent == false || rf.Absent == null) &&
                                                         (rf.NoSCM == false || rf.NoSCM == null) && (rf.Blacklistfaculy == false) && (rf.AbsentforVerification == false || rf.AbsentforVerification == null) && (rf.DepartmentId != 61 || rf.DepartmentId != 27) && rf.HighestDegreeID >= 4).Select(e => e.RegistrationNumber).ToArray();


                var jntuh_registered_faculty = jntuh_registered_faculty1.Where(e => !RegistrationNumbersCleared.Contains(e.RegistrationNumber)).Select(rf => new
                {
                    Id = rf.Id,
                    type = rf.type,
                    Absent = rf.Absent,
                    NotQualifiedAsperAICTE = rf.NotQualifiedAsperAICTE,
                    rf.InCompleteCeritificates,
                    rf.InvalidPANNo,
                    rf.NOrelevantPgFlag,
                    rf.NOrelevantUgFlag,
                    rf.NOrelevantPhdFlag,
                    rf.VerificationStatus,
                    rf.XeroxcopyofcertificatesFlag,
                    rf.NoSCM,
                    rf.NotconsideredPHD,
                    rf.Genuinenessnotsubmitted,
                    rf.NoPGspecialization,
                    rf.Noclass,
                    rf.PANNumber,
                    rf.NotIdentityFiedForAnyProgramFlag,
                    rf.InvalidAadhaar,
                    rf.BAS,
                    rf.OriginalsVerifiedUG,
                    rf.OriginalsVerifiedPHD,
                    rf.Invaliddegree,
                    rf.OriginalCertificatesnotshownFlag,
                    rf.BlacklistFaculty,

                    PHDundertakingnotsubmitted = rf.PhdUndertakingDocumentstatus,
                    Notin116 = rf.Notin116,

                    RegistrationNumber = rf.RegistrationNumber,
                    Department = rf.Department,
                    HighestDegree = jntuh_education_category.Where(c => c.id == rf.HighestDegreeID).Select(c => c.educationCategoryName).FirstOrDefault(),
                    Recruitedfor = jntuh_college_faculty_registered.Where(c => c.RegistrationNumber == rf.RegistrationNumber).Select(c => c.IdentifiedFor).FirstOrDefault(),
                    SpecializationId = jntuh_college_faculty_registered.Where(c => c.RegistrationNumber == rf.RegistrationNumber).Select(c => c.SpecializationId).FirstOrDefault(),
                    DeptId = jntuh_college_faculty_registered.Where(c => c.RegistrationNumber == rf.RegistrationNumber).Select(c => c.DepartmentId).FirstOrDefault(),
                    PanNumber = rf.PanNumber,
                    AadhaarNumber = rf.AadhaarNumber,
                    Photo = rf.Photo,
                    FullName = rf.FullName,
                    faculty_education = rf.FacultyEducation,
                    HighestDegreeID = rf.HighestDegreeID
                }).ToList();             

                foreach (var item in jntuh_registered_faculty)
                {
                    string Reason = null;
                    FacultyRegistration facultyregistered = new FacultyRegistration();
                    facultyregistered.id = item.Id;
                    facultyregistered.RegistrationNumber = item.RegistrationNumber;
                    facultyregistered.FirstName = item.FullName;
                    facultyregistered.department = item.Department;
                    facultyregistered.DepartmentId = item.DeptId;
                    facultyregistered.jntuh_registered_faculty_education = item.faculty_education;
                    facultyregistered.facultyPhoto = item.Photo;
                    facultyregistered.Absent = item.Absent != null && (bool)item.Absent;
                    facultyregistered.NOTQualifiedAsPerAICTE = item.NotQualifiedAsperAICTE != null && (bool)item.NotQualifiedAsperAICTE;
                    facultyregistered.NoSCM = item.NoSCM != null && (bool)item.NoSCM;
                    facultyregistered.PANNumber = item.PANNumber;
                    facultyregistered.PHDundertakingnotsubmitted = item.PHDundertakingnotsubmitted != null && (bool)item.PHDundertakingnotsubmitted;
                    facultyregistered.BlacklistFaculty = item.BlacklistFaculty != null && (bool)item.BlacklistFaculty;
                    facultyregistered.DegreeId = item.HighestDegreeID;
                    var principal =
                        jntuh_college_faculty_registered_new.Where(
                            r => r.RegistrationNumber == item.RegistrationNumber.Trim() && r.collegeId == cid)
                            .Select(s => s.RegistrationNumber)
                            .FirstOrDefault();
                    if (!String.IsNullOrEmpty(principal))
                        facultyregistered.Principal = "Principal";
                    else
                        facultyregistered.Principal = string.Empty;
                    if (item.Absent == true)
                        Reason += "Absent";
                    //if (item.type == "Adjunct")
                    //{
                    //    if (Reason != null)
                    //        Reason += ",Adjunct Faculty";
                    //    else
                    //        Reason += "Adjunct Faculty";
                    //}
                    //if (item.OriginalCertificatesnotshownFlag == true)
                    //{
                    //    if (Reason != null)
                    //        Reason += ",Orginal Certificates Not shown in College Inspection";
                    //    else
                    //        Reason += "Orginal Certificates Not shown in College Inspection";
                    //}
                    //if (item.XeroxcopyofcertificatesFlag == true)
                    //{
                    //    if (Reason != null)
                    //        Reason += ",Photo copy of Certificates";
                    //    else
                    //        Reason += "Photo copy of Certificates";
                    //}
                    //if (item.NotQualifiedAsperAICTE == true || item.HighestDegreeID < 4)
                    //{
                    //    if (Reason != null)
                    //        Reason += ",Not Qualified as per AICTE/PCI";
                    //    else
                    //        Reason += "Not Qualified as per AICTE/PCI";
                    //}
                    //if (item.NoSCM == true)
                    //{
                    //    if (Reason != null)
                    //        Reason += ",no/not valid SCM";
                    //    else
                    //        Reason += "no/not valid SCM";
                    //}
                    //if (item.PANNumber == null)
                    //{
                    //    if (Reason != null)
                    //        Reason += ",No PAN Number";
                    //    else
                    //        Reason += "No PAN Number";
                    //}
                    //if (item.InCompleteCeritificates == true)
                    //{
                    //    if (Reason != null)
                    //        Reason += ",IncompleteCertificates";
                    //    else
                    //        Reason += "IncompleteCertificates";
                    //}
                    //if (item.BlacklistFaculty == true)
                    //{
                    //    if (Reason != null)
                    //        Reason += ",Blacklisted Faculty";
                    //    else
                    //        Reason += "Blacklisted Faculty";
                    //}
                    //if (item.NOrelevantUgFlag == true)
                    //{
                    //    if (Reason != null)
                    //        Reason += ",NO Relevant UG";
                    //    else
                    //        Reason += "NO Relevant UG";
                    //}
                    //if (item.NOrelevantPgFlag == true)
                    //{
                    //    if (Reason != null)
                    //        Reason += ",NO Relevant PG";
                    //    else
                    //        Reason += "NO Relevant PG";
                    //}
                    //if (item.NOrelevantPhdFlag == true)
                    //{
                    //    if (Reason != null)
                    //        Reason += ",NO Relevant PHD";
                    //    else
                    //        Reason += "NO Relevant PHD";
                    //}
                    //if (item.InvalidPANNo == true)
                    //{
                    //    if (Reason != null)
                    //        Reason += ",InvalidPAN";
                    //    else
                    //        Reason += "InvalidPAN";
                    //}
                    //if (item.OriginalsVerifiedPHD == true)
                    //{
                    //    if (Reason != null)
                    //        Reason += ",No Guide Sign in PHD Thesis";
                    //    else
                    //        Reason += "No Guide Sign in PHD Thesis";
                    //}
                    //if (item.OriginalsVerifiedUG == true)
                    //{
                    //    if (Reason != null)
                    //        Reason += ",Complaint PHD Faculty";
                    //    else
                    //        Reason += "Complaint PHD Faculty";
                    //}
                    //if (item.Invaliddegree == true)
                    //{
                    //    if (Reason != null)
                    //        Reason += ",AICTE Not Approved University Degrees";
                    //    else
                    //        Reason += "AICTE Not Approved University Degrees";
                    //}
                    if (item.BAS == "Yes")
                    {
                        if (Reason != null)
                            Reason += ",Not having Sufficient Biometric Attendance";
                        else
                            Reason += "Not having Sufficient Biometric Attendance";
                    }
                    //if (item.InvalidAadhaar == "Yes")
                    //{
                    //    if (Reason != null)
                    //        Reason += ",Invalid/Blur Aadhaar";
                    //    else
                    //        Reason += "Invalid/Blur Aadhaar";
                    //}
                    //if (item.Noclass == true)
                    //{
                    //    if (Reason != null)
                    //        Reason += ",No Class in UG/PG";
                    //    else
                    //        Reason += "No Class in UG/PG";
                    //}
                    //if (item.VerificationStatus == true)
                    //{
                    //    if (Reason != null)
                    //        Reason += ",Absentfor Physical Verification";
                    //    else
                    //        Reason += "Absentfor Physical Verification";
                    //}
                    //if (item.NotconsideredPHD == true)
                    //{
                    //    if (Reason != null)
                    //        Reason += ",Cosidered as a faculty but not cosidered his/her P.hD";
                    //    else
                    //        Reason += "Cosidered as a faculty but not cosidered his/her P.hD";
                    //}
                    //if (item.NoPGspecialization == true)
                    //{
                    //    if (Reason != null)
                    //        Reason += ",no Specialization in PG";
                    //    else
                    //        Reason += "no Specialization in PG";
                    //}
                    //if (item.Genuinenessnotsubmitted == true)
                    //{
                    //    if (Reason != null)
                    //        Reason += ",PHD Genuinity not Submitted";
                    //    else
                    //        Reason += "PHD Genuinity not Submitted";
                    //}
                    facultyregistered.DeactivationReason = Reason;
                    if (jntuhcollege!=null)
                    {
                        facultyregistered.CollegeCode = jntuhcollege.collegeCode;
                        facultyregistered.CollegeName = jntuhcollege.collegeName;
                    }
                    ViewBag.CollegeName = facultyregistered.CollegeCode + " - " + facultyregistered.CollegeName;
                    FacultyRegistrationList.Add(facultyregistered);
                }
            }
            string ReportHeader = string.Empty;
            if (jntuhcollege != null)
               ReportHeader = jntuhcollege .collegeCode+ "-InactiveFaculty.xls";
            else
                ReportHeader = "InactiveFaculty.xls";

            Response.ClearContent();
            Response.Buffer = true;
            Response.AddHeader("content-disposition", "attachment; filename=" + ReportHeader);
            Response.ContentType = "application/vnd.ms-excel";
            return PartialView("~/Views/CollegeActiveInactiveFaculty/CollegewiseInactiveFacultyExport.cshtml", FacultyRegistrationList);
           
        }

        [Authorize(Roles = "Admin,College")]
        public ActionResult CollegeInactiveFaculty()
        {
            return RedirectToAction("CollegeDashboard", "Dashboard");
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int Collegeid = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            //int Collegeid = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(CollegeId.ToString(), System.Web.Configuration.WebConfigurationManager.AppSettings["CryptoKey"])); ;
            if (Collegeid == 375)
            {
                Collegeid = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            var FacultyRegistrationList = new List<FacultyRegistration>();
            if (Collegeid != null)
            {
                var jntuh_college_faculty_registered = db.jntuh_college_faculty_registered.Where(cf => cf.collegeId == Collegeid).ToList();
                string[] strRegnos = jntuh_college_faculty_registered.Select(cf => cf.RegistrationNumber).ToArray();


                var jntuh_college_faculty_registered_new = db.jntuh_college_principal_registered.Where(cf => cf.collegeId == Collegeid).ToList();
                var principalRegno = jntuh_college_faculty_registered_new.Select(cf => cf.RegistrationNumber).FirstOrDefault();

                var registeredFaculty = principalRegno != null ? db.jntuh_registered_faculty.Where(rf => strRegnos.Contains(rf.RegistrationNumber.Trim())).ToList()
                   : db.jntuh_registered_faculty.Where(rf => strRegnos.Contains(rf.RegistrationNumber.Trim())).ToList();//&& rf.RegistrationNumber != principalRegno && rf.DepartmentId == departmentid


                //education categoryIds UG,PG,PHD...........
                var jntuh_education_category = db.jntuh_education_category.ToList();

                var jntuh_registered_faculty1 = registeredFaculty.Select(rf => new
                {
                    Id = rf.id,
                    type = rf.type,
                    rf.DeactivationReason,
                    Absent = rf.Absent != null ? (bool)rf.Absent : false,
                    OriginalCertificatesnotshownFlag = rf.OriginalCertificatesNotShown != null ? (bool)rf.OriginalCertificatesNotShown : false,
                    XeroxcopyofcertificatesFlag = rf.Xeroxcopyofcertificates != null ? (bool)rf.Xeroxcopyofcertificates : false,
                    NotQualifiedAsperAICTE = rf.NotQualifiedAsperAICTE != null ? (bool)rf.NotQualifiedAsperAICTE : false,
                    NoSCM = rf.NoSCM != null ? (bool)rf.NoSCM : false,
                    InCompleteCeritificates = rf.IncompleteCertificates != null ? (bool)rf.IncompleteCertificates : false,
                    BlacklistFaculty = rf.Blacklistfaculy != null ? (bool)rf.Blacklistfaculy : false,
                    NOrelevantUgFlag = rf.NoRelevantUG == "Yes" ? true : false,
                    NOrelevantPgFlag = rf.NoRelevantPG == "Yes" ? true : false,
                    NOrelevantPhdFlag = rf.NORelevantPHD == "Yes" ? true : false,
                    InvalidPANNo = rf.InvalidPANNumber != null ? (bool)rf.InvalidPANNumber : false,
                    OriginalsVerifiedUG = rf.OriginalsVerifiedUG == true ? true : false,
                    OriginalsVerifiedPHD = rf.OriginalsVerifiedPHD == true ? true : false,
                    Invaliddegree = rf.Invaliddegree != null ? (bool)(rf.Invaliddegree) : false,
                    BAS = rf.BAS,
                    InvalidAadhaar = rf.InvalidAadhaar,
                    Noclass = rf.Noclass == true ? true : false,
                    VerificationStatus = rf.AbsentforVerification != null ? (bool)rf.AbsentforVerification : false,
                    NotconsideredPHD = rf.NotconsideredPHD == true ? true : false,
                    NoPGspecialization = rf.NoPGspecialization == true ? true : false,
                    Genuinenessnotsubmitted = rf.Genuinenessnotsubmitted == true ? true : false,
                    PANNumber = rf.PANNumber,
                    AbsentforVerification = rf.AbsentforVerification,
                    Blacklistfaculy = rf.Blacklistfaculy,
                    
                    NotIdentityFiedForAnyProgramFlag = rf.NotIdentityfiedForanyProgram != null ? (bool)rf.NotIdentityfiedForanyProgram : false,                                       
                                                                                                                                                    
                    //PhotocopyofPAN = rf.PhotoCopyofPAN != null ? (bool)rf.PhotoCopyofPAN : false,
                    PhdUndertakingDocumentstatus = rf.PhdUndertakingDocumentstatus != null ? (bool)(rf.PhdUndertakingDocumentstatus) : false,
                    PHDUndertakingDocumentView = rf.PHDUndertakingDocument,
                    PhdUndertakingDocumentText = rf.PhdUndertakingDocumentText,
                    //AppliedPAN = rf.AppliedPAN != null ? (bool)(rf.AppliedPAN) : false,
                    Notin116 = rf.Notin116,
                    
                    RegistrationNumber = rf.RegistrationNumber,
                    Department = rf.jntuh_department != null ? rf.jntuh_department.departmentName : "",
                    HighestDegreeID = rf.jntuh_registered_faculty_education.Count() != 0 ? rf.jntuh_registered_faculty_education.Where(r=>r.educationId!=8).Select(e => e.educationId).Max() : 0,
                    IsApproved = rf.isApproved,
                    PanNumber = rf.PANNumber,
                    AadhaarNumber = rf.AadhaarNumber,
                    Photo = rf.Photo,
                    FullName = rf.FirstName+" "+ rf.MiddleName+" "+ rf.LastName,
                    FacultyEducation = rf.jntuh_registered_faculty_education,
                    DegreeId = rf.jntuh_registered_faculty_education.Count(e => e.facultyId == rf.id) > 0 ? rf.jntuh_registered_faculty_education.Where(e => e.facultyId == rf.id&&e.educationId!=8).Select(e => e.educationId).Max() : 0,
                    DepartmentId = rf.DepartmentId,
                    NoClass = rf.Noclass == true ? true : false,
                    NOTQualifiedAsPerAICTE = rf.NotQualifiedAsperAICTE != null ? (bool)rf.NotQualifiedAsperAICTE : false,
                    Type = rf.type

                }).ToList();

                //var RegistrationNumbersCleared = jntuh_registered_faculty1.Where(rf => rf.type != "Adjunct"  && ((rf.NotQualifiedAsperAICTE == false || rf.NotQualifiedAsperAICTE == null) && (rf.NoSCM == false || rf.NoSCM == null) && (rf.PANNumber != null) && (rf.InCompleteCeritificates == false || rf.InCompleteCeritificates == null) && (rf.BlacklistFaculty == false) &&
                //                                  rf.NOrelevantUgFlag == false && rf.NOrelevantPgFlag == false && rf.NOrelevantPhdFlag == false && (rf.InvalidPANNo == false || rf.InvalidPANNo == null) && (rf.XeroxcopyofcertificatesFlag == false || rf.XeroxcopyofcertificatesFlag == null) &&
                //                                 (rf.Invaliddegree == false || rf.Invaliddegree == null)&&rf.NoPGspecialization==false&&rf.Noclass==false && (rf.VerificationStatus == false || rf.VerificationStatus == null) && rf.Genuinenessnotsubmitted==false&&rf.NotconsideredPHD==false  && rf.InvalidAadhaar != "Yes" && rf.OriginalsVerifiedUG == false && rf.OriginalsVerifiedPHD == false && (rf.DepartmentId != 61 || rf.DepartmentId != 27) && rf.HighestDegreeID >= 4)).Select(e => e.RegistrationNumber).ToArray();
                //rf.OriginalCertificatesnotshownFlag == false &&&& rf.BAS != "Yes"&& rf.Absent == false

                //var RegistrationNumbersNotCleared = jntuh_registered_faculty1.Where(item => item.Absent == true || item.NoSCM == true ||
                //            item.NOrelevantUgFlag == true ||
                //            item.NOrelevantPgFlag == true || item.NOrelevantPhdFlag == true ||
                //            item.InvalidPANNo == true ||
                //            item.DegreeId < 4 ||
                //            item.BlacklistFaculty == true || item.Type == "Adjunct" ||
                //            item.VerificationStatus == true ||
                //            item.OriginalCertificatesnotshownFlag == true || item.NOTQualifiedAsPerAICTE == true || item.NoClass == true).Select(e => e.RegistrationNumber).ToArray();
                //var jntuh_registered_faculty = jntuh_registered_faculty1.Where(e => !RegistrationNumbersCleared.Contains(e.RegistrationNumber)).Select(rf => new
                var RegistrationNumbersCleared = jntuh_registered_faculty1.Where(rf => rf.type != "Adjunct" && (rf.BAS != "Yes") && (rf.Noclass == false || rf.Noclass == null) && (rf.Absent == false || rf.Absent == null) &&
                                                         (rf.NoSCM == false || rf.NoSCM == null) && (rf.Blacklistfaculy == false) && (rf.AbsentforVerification == false || rf.AbsentforVerification == null) && (rf.DepartmentId != 61 || rf.DepartmentId != 27) && rf.HighestDegreeID >= 4).Select(e => e.RegistrationNumber).ToArray();

                var jntuh_registered_faculty = jntuh_registered_faculty1.Where(e => !RegistrationNumbersCleared.Contains(e.RegistrationNumber)).Select(rf => new
                {
                    Id = rf.Id,
                    type = rf.type,
                    Absent = rf.Absent,


                    NotQualifiedAsperAICTE = rf.NotQualifiedAsperAICTE,
                    rf.InCompleteCeritificates,
                    rf.InvalidPANNo,
                    rf.NOrelevantPgFlag,
                    rf.NOrelevantUgFlag,
                    rf.NOrelevantPhdFlag,
                    rf.VerificationStatus,
                    rf.XeroxcopyofcertificatesFlag,
                    rf.NoSCM,
                    rf.DeactivationReason,
                    rf.NotconsideredPHD,
                    rf.Genuinenessnotsubmitted,
                    rf.NoPGspecialization,
                    rf.Noclass,
                    rf.PANNumber,
                    rf.NotIdentityFiedForAnyProgramFlag,
                    rf.InvalidAadhaar,
                    rf.BAS,
                    
                    rf.OriginalsVerifiedUG,
                    rf.OriginalsVerifiedPHD,
                    rf.Invaliddegree,
                    rf.OriginalCertificatesnotshownFlag,
                    rf.BlacklistFaculty,

                    PHDundertakingnotsubmitted = rf.PhdUndertakingDocumentstatus,
                    Notin116 = rf.Notin116,

                    RegistrationNumber = rf.RegistrationNumber,
                    Department = rf.Department,
                    HighestDegree = jntuh_education_category.Where(c => c.id == rf.HighestDegreeID).Select(c => c.educationCategoryName).FirstOrDefault(),
                    Recruitedfor = jntuh_college_faculty_registered.Where(c => c.RegistrationNumber == rf.RegistrationNumber).Select(c => c.IdentifiedFor).FirstOrDefault(),
                    SpecializationId = jntuh_college_faculty_registered.Where(c => c.RegistrationNumber == rf.RegistrationNumber).Select(c => c.SpecializationId).FirstOrDefault(),
                    DeptId = jntuh_college_faculty_registered.Where(c => c.RegistrationNumber == rf.RegistrationNumber).Select(c => c.DepartmentId).FirstOrDefault(),
                    PanNumber = rf.PanNumber,
                    AadhaarNumber = rf.AadhaarNumber,
                    Photo = rf.Photo,
                    FullName = rf.FullName,
                    faculty_education = rf.FacultyEducation,
                    HighestDegreeID = rf.HighestDegreeID
                }).ToList();


                foreach (var item in jntuh_registered_faculty)
                {
                    string Reason = null;
                    FacultyRegistration facultyregistered = new FacultyRegistration();
                    facultyregistered.id = item.Id;
                    facultyregistered.RegistrationNumber = item.RegistrationNumber;
                    facultyregistered.FirstName = item.FullName;
                    facultyregistered.department = item.Department;
                    facultyregistered.DepartmentId = item.DeptId;
                    facultyregistered.jntuh_registered_faculty_education = item.faculty_education;
                    facultyregistered.facultyPhoto = item.Photo;
                    facultyregistered.Absent = item.Absent != null && (bool)item.Absent;
                    facultyregistered.NOTQualifiedAsPerAICTE = item.NotQualifiedAsperAICTE != null && (bool)item.NotQualifiedAsperAICTE;
                    facultyregistered.NoSCM = item.NoSCM != null && (bool)item.NoSCM;
                    facultyregistered.PANNumber = item.PANNumber;
                    facultyregistered.PHDundertakingnotsubmitted = item.PHDundertakingnotsubmitted != null && (bool)item.PHDundertakingnotsubmitted;
                    facultyregistered.BlacklistFaculty = item.BlacklistFaculty != null && (bool)item.BlacklistFaculty;
                    facultyregistered.DegreeId = item.HighestDegreeID;
                    var principal =
                        jntuh_college_faculty_registered_new.Where(
                            r => r.RegistrationNumber == item.RegistrationNumber.Trim() && r.collegeId == Collegeid)
                            .Select(s => s.RegistrationNumber)
                            .FirstOrDefault();
                    if (!String.IsNullOrEmpty(principal))
                        facultyregistered.Principal = "Principal";
                    else
                        facultyregistered.Principal = string.Empty;
                    if (item.Absent == true)
                        Reason += "Absent";
                    //if (item.type == "Adjunct")
                    //{
                    //    if (Reason != null)
                    //        Reason += ",Adjunct Faculty";
                    //    else
                    //        Reason += "Adjunct Faculty";
                    //}
                    //if (item.OriginalCertificatesnotshownFlag == true)
                    //{
                    //    if (Reason != null)
                    //        Reason += ",Orginal Certificates Not shown in College Inspection";
                    //    else
                    //        Reason += "Orginal Certificates Not shown in College Inspection";
                    //}
                    //if (item.XeroxcopyofcertificatesFlag == true)
                    //{
                    //    if (Reason != null)
                    //        Reason += ",Photo copy of Certificates";
                    //    else
                    //        Reason += "Photo copy of Certificates";
                    //}
                    //if (item.NotQualifiedAsperAICTE == true || item.HighestDegreeID < 4)
                    //{
                    //    if (Reason != null)
                    //        Reason += ",Not Qualified as per AICTE/PCI";
                    //    else
                    //        Reason += "Not Qualified as per AICTE/PCI";
                    //}
                    //if (item.NoSCM == true)
                    //{
                    //    if (Reason != null)
                    //        Reason += ",no/not valid SCM";
                    //    else
                    //        Reason += "no/not valid SCM";
                    //}
                    //if (item.PANNumber == null)
                    //{
                    //    if (Reason != null)
                    //        Reason += ",No PAN Number";
                    //    else
                    //        Reason += "No PAN Number";
                    //}
                    //if (item.InCompleteCeritificates == true)
                    //{
                    //    if (Reason != null)
                    //        Reason += ",IncompleteCertificates";
                    //    else
                    //        Reason += "IncompleteCertificates";
                    //}
                    //if (item.BlacklistFaculty == true)
                    //{
                    //    if (Reason != null)
                    //        Reason += ",Blacklisted Faculty";
                    //    else
                    //        Reason += "Blacklisted Faculty";
                    //}
                    //if (item.NOrelevantUgFlag == true)
                    //{
                    //    if (Reason != null)
                    //        Reason += ",NO Relevant UG";
                    //    else
                    //        Reason += "NO Relevant UG";
                    //}
                    //if (item.NOrelevantPgFlag == true)
                    //{
                    //    if (Reason != null)
                    //        Reason += ",NO Relevant PG";
                    //    else
                    //        Reason += "NO Relevant PG";
                    //}
                    //if (item.NOrelevantPhdFlag == true)
                    //{
                    //    if (Reason != null)
                    //        Reason += ",NO Relevant PHD";
                    //    else
                    //        Reason += "NO Relevant PHD";
                    //}
                    //if (item.InvalidPANNo == true)
                    //{
                    //    if (Reason != null)
                    //        Reason += ",InvalidPAN";
                    //    else
                    //        Reason += "InvalidPAN";
                    //}
                    //if (item.OriginalsVerifiedPHD == true)
                    //{
                    //    if (Reason != null)
                    //        Reason += ",No Guide Sign in PHD Thesis";
                    //    else
                    //        Reason += "No Guide Sign in PHD Thesis";
                    //}
                    //if (item.OriginalsVerifiedUG == true)
                    //{
                    //    if (Reason != null)
                    //        Reason += ",Complaint PHD Faculty";
                    //    else
                    //        Reason += "Complaint PHD Faculty";
                    //}
                    //if (item.Invaliddegree == true)
                    //{
                    //    if (Reason != null)
                    //        Reason += ",AICTE Not Approved University Degrees";
                    //    else
                    //        Reason += "AICTE Not Approved University Degrees";
                    //}
                    if (item.BAS == "Yes")
                    {
                        if (Reason != null)
                            Reason += ",Not having Sufficient Biometric Attendance";
                        else
                            Reason += "Not having Sufficient Biometric Attendance";
                    }
                    //if (item.InvalidAadhaar == "Yes")
                    //{
                    //    if (Reason != null)
                    //        Reason += ",Invalid/Blur Aadhaar";
                    //    else
                    //        Reason += "Invalid/Blur Aadhaar";
                    //}
                    //if (item.Noclass == true)
                    //{
                    //    if (Reason != null)
                    //        Reason += ",No Class in UG/PG";
                    //    else
                    //        Reason += "No Class in UG/PG";
                    //}
                    //if (item.VerificationStatus == true)
                    //{
                    //    if (Reason != null)
                    //        Reason += ",Absentfor Physical Verification";
                    //    else
                    //        Reason += "Absentfor Physical Verification";
                    //}
                    //if (item.NotconsideredPHD == true)
                    //{
                    //    if (Reason != null)
                    //        Reason += ",Cosidered as a faculty but not cosidered his/her P.hD";
                    //    else
                    //        Reason += "Cosidered as a faculty but not cosidered his/her P.hD";
                    //}
                    //if (item.NoPGspecialization == true)
                    //{
                    //    if (Reason != null)
                    //        Reason += ",no Specialization in PG";
                    //    else
                    //        Reason += "no Specialization in PG";
                    //}
                    //if (item.Genuinenessnotsubmitted == true)
                    //{
                    //    if (Reason != null)
                    //        Reason += ",PHD Genuinity not Submitted";
                    //    else
                    //        Reason += "PHD Genuinity not Submitted";
                    //}  
               
                    //if(item.DeactivationReason!=null)
                    //    facultyregistered.DeactivationReason = item.DeactivationReason+","+ Reason;
                    //else
                        facultyregistered.DeactivationReason =  Reason;

                    
                    FacultyRegistrationList.Add(facultyregistered);
                }
            }
            return View(FacultyRegistrationList);
        }
    }
}
