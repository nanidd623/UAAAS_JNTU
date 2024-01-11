using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using Org.BouncyCastle.Asn1.X509;
using UAAAS.Models;


namespace UAAAS.Controllers
{
    [ErrorHandling]
    public class FacultyVerificationDENewController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();

        #region

        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification,DataEntry")]
        public ActionResult Index(int? collegeid)
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
                       .Where(c => c.e.IsCollegeEditable == false && c.co.isActive == true && c.e.academicyearId == prAy)
                       .Select(c => new { collegeId = c.co.id, collegeName = c.co.collegeCode + "-" + c.co.collegeName })
                       .OrderBy(c => c.collegeName)
                       .ToList();
            }
            else
            {
                int InspectionPhaseId = db.jntuh_inspection_phase.Where(p => p.isActive == true && p.inspectionPhase == "Aadhar Verification").Select(p => p.id).SingleOrDefault();
                int[] assignedcollegeslist =
                    db.jntuh_dataentry_allotment.Where(
                        d =>
                            d.InspectionPhaseId == InspectionPhaseId && d.userID == userID && d.isActive == true &&
                            d.isCompleted == false).Select(s => s.collegeID).ToArray();
                ViewBag.Colleges =
                    db.jntuh_college.Join(db.jntuh_college_edit_status, co => co.id, e => e.collegeId,
                        (co, e) => new { co = co, e = e })
                        .Where(c => c.e.IsCollegeEditable == false && c.co.isActive == true && c.e.academicyearId == prAy && assignedcollegeslist.Contains(c.e.collegeId))
                        .Select(c => new { collegeId = c.co.id, collegeName = c.co.collegeCode + "-" + c.co.collegeName })
                        .OrderBy(c => c.collegeName)
                        .ToList();

            }
            // ViewBag.Colleges = db.jntuh_college.Join(db.jntuh_college_edit_status, co => co.id, e => e.collegeId, (co, e) => new { co = co, e = e }).Where(c => c.co.isActive == true && c.e.IsCollegeEditable==false).Select(c => new { collegeId = c.co.id, collegeName = c.co.collegeCode + "-" + c.co.collegeName }).OrderBy(c => c.collegeName).ToList();
            ViewBag.collegeid = collegeid;

            var jntuh_department = db.jntuh_department.ToList();
            List<FacultyRegistration> teachingFaculty = new List<FacultyRegistration>();

            if (collegeid != null)
            {
                //string[] notshowregistation = { "71150401-160927", "45150402-123537", "26150403-103955" };
                //&&!notshowregistation.Contains(cf.RegistrationNumber)
                string[] notshowregistation =
                    db.jntuh_college_faculty_registered_copy.Where(fr => fr.collegeId == collegeid)
                        .Select(s => s.RegistrationNumber)
                        .ToArray();
                List<jntuh_college_faculty_registered> jntuh_college_faculty_registered = db.jntuh_college_faculty_registered.Where(cf => cf.collegeId == collegeid && !notshowregistation.Contains(cf.RegistrationNumber) && cf.AadhaarDocument != null).Select(cf => cf).ToList();
                string[] strRegNoS = jntuh_college_faculty_registered.Select(cf => cf.RegistrationNumber).ToArray();
                string[] PrincipalstrRegNoS = db.jntuh_college_principal_registered.Where(P => P.collegeId == collegeid).Select(P => P.RegistrationNumber.Trim()).ToArray();
                List<jntuh_registered_faculty> jntuh_registered_faculty = new List<jntuh_registered_faculty>();
                jntuh_registered_faculty = db.jntuh_registered_faculty.Where(rf => strRegNoS.Contains(rf.RegistrationNumber) && rf.Blacklistfaculy != true)  //&& (rf.collegeId == null || rf.collegeId == collegeid)//&& rf.Notin116 != true
                                             .ToList();

                //  var jntuh_notin415facultys = db.jntuh_notin415faculty.Where(F => F.CollegeId == collegeid).ToList();
                var Specializations = db.jntuh_specialization.ToList();
                //  string[] strREG = jntuh_notin415facultys.Select(F => F.RegistrationNumber.Trim()).ToArray();
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
                    //Aadhaar Flag
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
                    faculty.AadhaarNumber = jntuh_college_faculty_registered.Where(f => f.RegistrationNumber.Trim() == a.RegistrationNumber.Trim()).Select(s => s.AadhaarNumber).FirstOrDefault();
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
                    faculty.InvalidPANNo = a.InvalidPANNumber != null ? (bool)a.InvalidPANNumber : false;
                    faculty.InCompleteCeritificates = a.IncompleteCertificates != null ? (bool)a.IncompleteCertificates : false;
                    faculty.OriginalCertificatesnotshownFlag = a.OriginalCertificatesNotShown != null ? (bool)a.OriginalCertificatesNotShown : false;
                    faculty.FalsePAN = a.FalsePAN != null ? (bool)a.FalsePAN : false;
                    faculty.NOForm16 = a.NoForm16 != null ? (bool)a.NoForm16 : false;
                    //faculty.facultyAadhaarCardDocument = a.AadhaarDocument;
                    faculty.facultyAadhaarCardDocument = jntuh_college_faculty_registered.Where(f => f.RegistrationNumber == a.RegistrationNumber.Trim()).Select(s => s.AadhaarDocument).FirstOrDefault();
                    faculty.updatedOn = jntuh_college_faculty_registered.Where(f => f.RegistrationNumber.Trim() == a.RegistrationNumber.Trim()).Select(f => f.updatedOn).FirstOrDefault();
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
                teachingFaculty = teachingFaculty.Where(m => m.isActive == true).OrderBy(f => f.updatedOn).ThenBy(f => f.department).ThenBy(f => f.id).ToList();
                ViewBag.TotalFaculty = teachingFaculty.Count();
                ViewBag.AFlagTotalFaculty = teachingFaculty.Where(a => a.Basstatus == "Yes").Count();
                return View(teachingFaculty);
            }

            return View(teachingFaculty);
        }

        //Aadhaar Verification Approve Screen
        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification,DataEntry")]
        public ActionResult AadhaarApprove(string fid, string collegeid)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            if (fid != null)
            {
                int fID = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(fid, WebConfigurationManager.AppSettings["CryptoKey"]));
                var facultydetails = db.jntuh_registered_faculty.Where(i => i.id == fID).Select(s => s).FirstOrDefault();
                if (facultydetails != null)
                {
                    //facultydetails.BASStatus = "No";
                    db.Entry(facultydetails).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Success"] = facultydetails.RegistrationNumber + " Aadhar Approved Successfully.";
                }
            }
            return RedirectToAction("Index", new { collegeid = collegeid });
        }

        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification,DataEntry")]
        public ActionResult AadhaarNotApprove(string fid, string collegeid)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            if (fid != null)
            {
                int fID = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(fid, WebConfigurationManager.AppSettings["CryptoKey"]));
                var facultydetails = db.jntuh_registered_faculty.Where(i => i.id == fID).Select(s => s).FirstOrDefault();
                if (facultydetails != null)
                {
                    //facultydetails.BASStatus = "Yes";
                    db.Entry(facultydetails).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Success"] = facultydetails.RegistrationNumber + " Aadhaar not Approved Successfully.";
                }
            }
            return RedirectToAction("Index", new { collegeid = collegeid });
        }
        [Authorize(Roles = "Admin")]
        public ActionResult Aadhaarflagrollback(string fid, string collegeid)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            if (fid != null)
            {
                int fID = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(fid, WebConfigurationManager.AppSettings["CryptoKey"]));
                var facultydetails = db.jntuh_registered_faculty.Where(i => i.id == fID).Select(s => s).FirstOrDefault();
                if (facultydetails != null)
                {
                    //facultydetails.BASStatus =null;
                    db.Entry(facultydetails).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Success"] = facultydetails.RegistrationNumber + " Rollback Successfully.";
                }
            }
            return RedirectToAction("Index", new { collegeid = collegeid });
        }

        //New Action on 03-03-2018
        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification,DataEntry")]
        public ActionResult FacultyDataEntry(int? collegeid)
        {
            //ViewBag.Colleges = db.jntuh_college.Join(db.jntuh_college_edit_status, co => co.id, e => e.collegeId, (co, e) => new { co = co, e = e }).Where(c => c.co.isActive == true && c.e.IsCollegeEditable == false).Select(c => new { collegeId = c.co.id, collegeName = c.co.collegeCode + "-" + c.co.collegeName }).OrderBy(c => c.collegeName).ToList();
            //int[] jntuh_college_list = { 2, 4, 7, 8, 9, 11, 12, 17, 20, 22, 23, 26, 29, 32, 34, 38, 40, 41, 46, 48, 56, 59, 68, 69, 70, 72, 74, 77, 79, 80, 81, 84, 85, 86, 87, 88, 100, 102, 103, 104, 106, 108, 109, 111, 113, 115, 116, 119, 121, 122, 123, 124, 125, 128, 129, 130, 132, 134, 137, 138, 141, 143, 144, 145, 147, 148, 151, 152, 153, 155, 156, 157, 158, 159, 161, 162, 163, 164, 165, 166, 168, 170, 171, 172, 173, 175, 176, 177, 178, 179, 181, 182, 183, 184, 185, 186, 187, 188, 189, 192, 193, 195, 196, 197, 198, 201, 203, 207, 210, 211, 214, 215, 218, 222, 225, 227, 228, 229, 236, 238, 241, 242, 243, 244, 245, 247, 249, 250, 254, 256, 259, 260, 261, 264, 269, 271, 273, 276, 282, 283, 286, 287, 291, 292, 293, 299, 300, 304, 305, 306, 307, 308, 309, 310, 315, 316, 321, 322, 324, 326, 327, 329, 330, 334, 335, 336, 342, 349, 350, 352, 360, 365, 366, 367, 368, 369, 371, 373, 374, 376, 380, 382, 385, 391, 393, 394, 395, 399, 400, 401, 402, 403, 414, 415, 416, 419, 420, 422, 423, 424, 428, 429, 430, 6, 24, 27, 30, 44, 45, 47, 52, 54, 55, 58, 60, 65, 66, 78, 90, 95, 97, 105, 107, 110, 114, 117, 118, 120, 127, 135, 136, 139, 146, 150, 169, 202, 204, 206, 213, 219, 234, 237, 239, 252, 253, 262, 263, 267, 290, 295, 297, 298, 301, 302, 303, 313, 314, 317, 318, 319, 320, 348, 353, 362, 370, 379, 384, 389, 392, 410, 427, 5, 67, 246, 279, 296, 325, 343, 355, 386, 411, 421, 39, 42, 43, 75, 140, 180, 194, 217, 223, 230, 235, 266, 332, 364, 35, 50, 91, 174, 435, 436, 439, 441, 442, 443, 445, 447, 448, 452, 454, 455 };
            //int[] jntuh_college_list = { 2, 4, 7, 8, 9, 12, 17, 20, 23, 26, 32, 29, 34, 38, 41, 46, 48, 56, 59, 69, 72, 74, 77, 80, 81, 85, 86, 88, 100, 103, 108, 109, 104, 79, 116, 123, 128, 129, 132, 134, 137, 111, 115, 121, 122, 119, 138, 141, 143, 144, 147, 152, 155, 157, 158, 163, 164, 159, 170, 171, 172, 173, 175, 176, 177, 178, 184, 185, 181, 182, 187, 188, 192, 193, 195, 196, 198, 203, 211, 218, 222, 229, 238, 242, 243, 249, 225, 250, 256, 260, 261, 264, 271, 273, 286, 282, 283, 287, 292, 293, 299, 300, 304, 306, 307, 309, 310, 316, 291, 327, 330, 335, 342, 349, 350, 352, 360, 365, 366, 367, 321, 322, 373, 380, 385, 393, 400, 423, 391, 369, 376, 395, 394, 429, 428, 6, 24, 27, 30, 44, 47, 52, 54, 58, 60, 65, 90, 95, 97, 105, 110, 114, 117, 118, 120, 135, 136, 139, 146, 150, 169, 424, 430, 39, 42, 202, 204, 213, 219, 234, 237, 262, 263, 267, 290, 297, 298, 301, 302, 303, 313, 314, 317, 318, 319, 320, 348, 353, 384, 389, 392, 5, 67, 296, 343, 355, 386, 411, 413, 140, 180, 217, 43, 230, 235, 194, 266, 364, 435, 332, 455, 443, 442, 448, 454, 174, 449, 452 };
            //int[] jntuh_college_list = { 11, 22, 40, 68, 70, 84, 87, 106, 113, 125, 130, 145, 148, 151, 153, 156, 161, 162, 165, 166, 168, 179, 183, 186, 189, 197, 201, 207, 214, 215, 227, 228, 236, 241, 244, 245, 254, 276, 308, 315, 324, 329, 334, 336, 368, 371, 374, 382, 399, 403, 414, 416, 420, 422, 55, 78, 107, 127, 206, 253, 295, 370, 379, 410, 246, 421, 75, 223, 35, 50, 91, 436, 439, 441, 445 };
            //var jntuh_college_edit_status_list = db.jntuh_college_edit_status.Where(s => jntuh_college_list.Contains(s.collegeId)).ToList();
            //ViewBag.Colleges = db.jntuh_college.Join(jntuh_college_edit_status, co => co.id, e => e.collegeId, (co, e) => new { co = co, e = e }).Where(c => c.co.isActive == true).Select(c => new { collegeId = c.co.id, collegeName = c.co.collegeCode + "-" + c.co.collegeName }).OrderBy(c => c.collegeName).ToList();
            ViewBag.Colleges = db.jntuh_college.Join(db.jntuh_college_edit_status, co => co.id, e => e.collegeId, (co, e) => new { co = co, e = e }).Where(c => c.e.IsCollegeEditable == true).Select(c => new { collegeId = c.co.id, collegeName = c.co.collegeCode + "-" + c.co.collegeName }).OrderBy(c => c.collegeName).ToList();
            ViewBag.collegeid = collegeid;

            var jntuh_department = db.jntuh_department.ToList();
            List<FacultyRegistration> teachingFaculty = new List<FacultyRegistration>();

            if (collegeid != null)
            {
                //string[] notshowregistation = { "71150401-160927", "45150402-123537", "26150403-103955" };
                //&&!notshowregistation.Contains(cf.RegistrationNumber)
                //string[] notshowregistation =
                //    db.jntuh_college_faculty_registered_copy.Where(fr => fr.collegeId == collegeid)
                //        .Select(s => s.RegistrationNumber)
                //        .ToArray();
                List<jntuh_college_faculty_registered> jntuh_college_faculty_registered = db.jntuh_college_faculty_registered.Where(cf => cf.collegeId == collegeid).Select(cf => cf).ToList();
                string[] strRegNoS = jntuh_college_faculty_registered.Select(cf => cf.RegistrationNumber).ToArray();
                string[] PrincipalstrRegNoS = db.jntuh_college_principal_registered.Where(P => P.collegeId == collegeid).Select(P => P.RegistrationNumber.Trim()).ToArray();
                List<jntuh_registered_faculty> jntuh_registered_faculty = new List<jntuh_registered_faculty>();
                jntuh_registered_faculty = db.jntuh_registered_faculty.Where(rf => strRegNoS.Contains(rf.RegistrationNumber) && rf.Blacklistfaculy != true)  //&& (rf.collegeId == null || rf.collegeId == collegeid)//&& rf.Notin116 != true
                                             .ToList();

                //  var jntuh_notin415facultys = db.jntuh_notin415faculty.Where(F => F.CollegeId == collegeid).ToList();
                var Specializations = db.jntuh_specialization.ToList();
                //  string[] strREG = jntuh_notin415facultys.Select(F => F.RegistrationNumber.Trim()).ToArray();
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
                    //faculty.Basstatus = a.BASStatus;
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
                    faculty.AadhaarNumber = jntuh_college_faculty_registered.Where(f => f.RegistrationNumber == a.RegistrationNumber).Select(s => s.AadhaarNumber).FirstOrDefault();
                    faculty.isActive = a.isActive;
                    faculty.isApproved = a.isApproved;
                    faculty.IdentfiedFor = jntuh_college_faculty_registered.Where(f => f.RegistrationNumber.Trim() == a.RegistrationNumber.Trim()).Select(f => f.IdentifiedFor).FirstOrDefault();
                    if (faculty.IdentfiedFor == "UG")
                    {
                        a.DepartmentId =
                       jntuh_college_faculty_registered.Where(c => c.RegistrationNumber == a.RegistrationNumber)
                           .Select(s => s.DepartmentId)
                           .FirstOrDefault();
                        faculty.department = jntuh_department.Where(d => d.id == a.DepartmentId).Select(d => d.departmentName).FirstOrDefault();
                        faculty.DegreeId = jntuh_department.Where(d => d.id == a.DepartmentId).Select(d => d.degreeId).FirstOrDefault();
                        faculty.DegreeName = db.jntuh_degree.Where(d => d.id == faculty.DegreeId).Select(s => s.degree).FirstOrDefault();
                        faculty.SpecializationName = string.Empty;
                    }
                    else
                    {
                        a.DepartmentId =
                        jntuh_college_faculty_registered.Where(c => c.RegistrationNumber == a.RegistrationNumber)
                            .Select(s => s.DepartmentId)
                            .FirstOrDefault();
                        faculty.department = jntuh_department.Where(d => d.id == a.DepartmentId).Select(d => d.departmentName).FirstOrDefault();
                        faculty.SpecializationId = jntuh_college_faculty_registered.Where(c => c.RegistrationNumber == a.RegistrationNumber)
                           .Select(s => s.SpecializationId)
                           .FirstOrDefault();
                        faculty.SpecializationName =
                            db.jntuh_specialization.Where(s => s.id == faculty.SpecializationId)
                                .Select(s => s.specializationName)
                                .FirstOrDefault();
                    }


                    faculty.SamePANNumberCount = jntuh_registered_faculty.Where(f => f.PANNumber == a.PANNumber).ToList().Count;
                    faculty.SameAadhaarNumberCount = jntuh_registered_faculty.Where(f => f.AadhaarNumber == a.AadhaarNumber).ToList().Count;
                    faculty.SpecializationIdentfiedFor = Specializationid > 0 ? Specializations.Where(S => S.id == Specializationid).Select(S => S.specializationName).FirstOrDefault() : "";
                    faculty.isVerified = isFacultyVerified(a.id);
                    faculty.DeactivationReason = a.DeactivationReason;
                    faculty.FacultyVerificationStatus = a.FacultyVerificationStatus;
                    faculty.updatedOn = a.updatedOn;
                    faculty.createdOn = jntuh_college_faculty_registered.Where(f => f.RegistrationNumber == a.RegistrationNumber).Select(f => f.createdOn).FirstOrDefault();

                    faculty.jntuh_registered_faculty_education = a.jntuh_registered_faculty_education;
                    //faculty.DegreeId = a.jntuh_registered_faculty_education.Count(e => e.facultyId == a.id) > 0 ? a.jntuh_registered_faculty_education.Where(e => e.facultyId == a.id).Select(e => e.educationId).Max() : 0;
                    faculty.PanDeactivationReasion = !string.IsNullOrEmpty(a.PanDeactivationReason) ? a.PanDeactivationReason : "";
                    faculty.Absent = a.Absent != null ? (bool)a.Absent : false;
                    faculty.BlacklistFaculty = a.Blacklistfaculy != null ? (bool)a.Blacklistfaculy : false;
                    faculty.PHDundertakingnotsubmitted = a.PHDundertakingnotsubmitted != null ? (bool)a.PHDundertakingnotsubmitted : false;
                    faculty.NOTQualifiedAsPerAICTE = a.NotQualifiedAsperAICTE != null ? (bool)a.NotQualifiedAsperAICTE : false;
                    faculty.InvalidPANNo = a.InvalidPANNumber != null ? (bool)a.InvalidPANNumber : false;
                    faculty.InCompleteCeritificates = a.IncompleteCertificates != null ? (bool)a.IncompleteCertificates : false;
                    faculty.OriginalCertificatesnotshownFlag = a.OriginalCertificatesNotShown != null ? (bool)a.OriginalCertificatesNotShown : false;
                    faculty.FalsePAN = a.FalsePAN != null ? (bool)a.FalsePAN : false;
                    faculty.NOForm16 = a.NoForm16 != null ? (bool)a.NoForm16 : false;
                    faculty.ModifiedPANNo = a.ModifiedPANNumber;
                    faculty.PanStatusAfterDE = a.PanStatusAfterDE;
                    //faculty.facultyAadhaarCardDocument = a.AadhaarDocument;
                    faculty.facultyAadhaarCardDocument = jntuh_college_faculty_registered.Where(f => f.RegistrationNumber == a.RegistrationNumber).Select(s => s.AadhaarDocument).FirstOrDefault();
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
                ViewBag.TotalFaculty = teachingFaculty.Count();
                ViewBag.AbsentFacultyCount = teachingFaculty.Where(a => a.Absent == true).ToList().Count();
                ViewBag.ClearFaculty = ViewBag.TotalFaculty - ViewBag.AbsentFacultyCount;
                ViewBag.notshowcertiCount = teachingFaculty.Where(a => a.OriginalCertificatesnotshownFlag == true).ToList().Count();
                ViewBag.Modifiedpancount = teachingFaculty.Where(a => a.ModifiedPANNo != null).ToList().Count();
                ViewBag.Modifiedaadhaarcount = teachingFaculty.Where(a => a.PanStatusAfterDE != null).ToList().Count();
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
                return View(teachingFaculty.OrderBy(d => d.department).ThenBy(d => d.DegreeName).ToList());
            }

            return View(teachingFaculty.OrderBy(d => d.department).ThenBy(d => d.DegreeName).ToList());
        }

        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification,DataEntry")]
        public ActionResult FacultyCertificatesVerification(int? collegeid)
        {
            ViewBag.Colleges = db.jntuh_college.Where(c => c.isActive == true).Select(c => new { collegeId = c.id, collegeName = c.collegeCode + "-" + c.collegeName }).OrderBy(c => c.collegeName).ToList();

            var jntuh_department = db.jntuh_department.ToList();
            List<FacultyRegistration> teachingFaculty = new List<FacultyRegistration>();

            if (collegeid != null)
            {
                List<jntuh_college_faculty_registered> jntuh_college_faculty_registered = db.jntuh_college_faculty_registered.Where(cf => cf.collegeId == collegeid).Select(cf => cf).ToList();
                string[] strRegNoS = db.jntuh_college_faculty_registered.Where(cf => cf.collegeId == collegeid).Select(cf => cf.RegistrationNumber).ToArray();
                string[] clearRegNos =
                    db.jntuh_college_faculty_registered_copy.Where(
                        c => c.collegeId == 375 && strRegNoS.Contains(c.RegistrationNumber))
                        .Select(s => s.RegistrationNumber)
                        .ToArray();
                //string[] notclearRegNos =
                //    db.jntuh_college_faculty_registered_copy.Where(
                //        c => c.collegeId == 375 && !strRegNoS.Contains(c.RegistrationNumber))
                //        .Select(s => s.RegistrationNumber)
                //        .ToArray();
                List<jntuh_registered_faculty> jntuh_registered_faculty = new List<jntuh_registered_faculty>();
                jntuh_registered_faculty = db.jntuh_registered_faculty
                                             .Where(rf => clearRegNos.Contains(rf.RegistrationNumber))  //&& (rf.collegeId == null || rf.collegeId == collegeid)
                                             .ToList();
                int[] REGIDS = jntuh_registered_faculty.Select(F => F.id).ToArray();
                var FacultyEducation = db.jntuh_registered_faculty_education.Where(E => REGIDS.Contains(E.facultyId)).ToList();
                // string FDateOfAppointment = null;
                var data = jntuh_registered_faculty.Select(a => new FacultyRegistration
                {
                    id = a.id,
                    Type = a.type,
                    CollegeId = collegeid,
                    RegistrationNumber = a.RegistrationNumber,
                    UniqueID = a.UniqueID,
                    FirstName = a.FirstName,
                    MiddleName = a.MiddleName,
                    LastName = a.LastName,
                    GenderId = a.GenderId,
                    Email = a.Email,
                    facultyPhoto = a.Photo,
                    Mobile = a.Mobile,
                    PANNumber = a.PANNumber,
                    AadhaarNumber = a.AadhaarNumber,
                    TotalExperience = a.TotalExperience > 0 ? a.TotalExperience : 0,
                    facultyDateOfAppointment = UAAAS.Models.Utilities.MMDDYY2DDMMYY(a.DateOfAppointment.ToString()),
                    SelectionCommitteeProcedings = a.ProceedingDocument,
                    SSC = FacultyEducation.Where(E => E.educationId == 1 && E.facultyId == a.id).Select(E => E.certificate).FirstOrDefault(),
                    UG = FacultyEducation.Where(E => E.educationId == 3 && E.facultyId == a.id).Select(E => E.certificate).FirstOrDefault(),
                    PG = FacultyEducation.Where(E => E.educationId == 4 && E.facultyId == a.id).Select(E => E.certificate).FirstOrDefault(),
                    PHD = FacultyEducation.Where(E => E.educationId == 5 && E.facultyId == a.id).Select(E => E.certificate).FirstOrDefault(),
                    InCompleteCeritificates = a.IncompleteCertificates != null ? (bool)a.IncompleteCertificates : false,
                    //isActive = a.isActive,
                    //isApproved = a.isApproved,
                    //department = jntuh_department.Where(d => d.id == a.DepartmentId).Select(d => d.departmentName).FirstOrDefault(),
                    //SamePANNumberCount = jntuh_registered_faculty.Where(f => f.PANNumber == a.PANNumber).ToList().Count(),
                    //SameAadhaarNumberCount = jntuh_registered_faculty.Where(f => f.AadhaarNumber == a.AadhaarNumber).ToList().Count(),
                    //isVerified = isFacultyVerified(a.id),
                    //DeactivationReason = a.DeactivationReason,
                    //FacultyVerificationStatus = a.FacultyVerificationStatus,
                    //updatedOn = a.updatedOn,
                    //createdOn = jntuh_college_faculty_registered.Where(f => f.RegistrationNumber == a.RegistrationNumber).Select(f => f.createdOn).FirstOrDefault(),
                    //jntuh_registered_faculty_education = a.jntuh_registered_faculty_education,
                    //PanDeactivationReasion = a.PanDeactivationReason,
                    //Absent = a.Absent != null ? (bool)a.Absent : false,
                    //NOTQualifiedAsPerAICTE = a.NotQualifiedAsperAICTE != null ? (bool)a.NotQualifiedAsperAICTE : false,

                }).ToList();

                teachingFaculty.AddRange(data);
                //teachingFaculty = teachingFaculty.Where(m => m.isActive == true).OrderBy(f => f.updatedOn).ThenBy(f => f.department).ToList();
                return View(teachingFaculty);
            }

            return View(teachingFaculty);
        }

        public ActionResult FacultyDeactivation(int fid, string RegNo, int? collegeid)
        {
            var jntuh_registered_facultys = db.jntuh_registered_faculty.Where(F => F.id == fid).FirstOrDefault();

            if (jntuh_registered_facultys != null)
            {
                string DeactivationReasion = jntuh_registered_facultys.DeactivationReason;
                jntuh_registered_facultys.updatedOn = DateTime.Now;
                jntuh_registered_facultys.updatedBy = 375;
                //if (!string.IsNullOrEmpty(DeactivationReasion))
                //    jntuh_registered_facultys.DeactivationReason = "In Complete Certificates" + "," + DeactivationReasion;
                //else
                //jntuh_registered_facultys.DeactivationReason = "In Complete Certificates";
                jntuh_registered_facultys.IncompleteCertificates = true;
                db.Entry(jntuh_registered_facultys).State = EntityState.Modified;
                db.SaveChanges();
            }

            return RedirectToAction("FacultyCertificatesVerification", "FacultyVerificationDENew", new { collegeid = collegeid });
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
                regFaculty.CollegeId = db.jntuh_college_faculty_registered.Where(f => f.RegistrationNumber == regFaculty.RegistrationNumber).Select(s => s.collegeId).FirstOrDefault();
                if (regFaculty.CollegeId != 0)
                {
                    regFaculty.CollegeName = db.jntuh_college.Find(regFaculty.CollegeId).collegeName;
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
                regFaculty.PhdUndertakingDocumentstatus = faculty.PhdUndertakingDocumentstatus != null ? (bool)faculty.PhdUndertakingDocumentstatus : true;
                regFaculty.PHDUndertakingDocumentView = faculty.PHDUndertakingDocument;
                regFaculty.PhdUndertakingDocumentText = faculty.PhdUndertakingDocumentText;


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

        [HttpGet]
        public ActionResult EditFacultyDetails(string fid)
        {
            ViewBag.fid = string.IsNullOrEmpty(fid) ? "0" : fid;
            int fID = 0;
            if (!string.IsNullOrEmpty(User.Identity.Name))
            {
                int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
                int facultyId = db.jntuh_registered_faculty.Where(f => f.UserId == userID).Select(f => f.id).FirstOrDefault();

                if (fid != null)
                {
                    fID = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(fid, WebConfigurationManager.AppSettings["CryptoKey"]));
                }

                //if (facultyId != fID && !Roles.IsUserInRole("Admin"))
                //{
                //    fID = facultyId;
                //}

            }
            else
            {
                if (fid != null)
                {
                    fID = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(fid, WebConfigurationManager.AppSettings["CryptoKey"]));
                }
            }

            if (fid != null)
            {
                TempData["fid"] = fid;
            }

            ViewBag.Id = fid;
            ViewBag.FacultyID = fID;

            DateTime todayDate = DateTime.Now.Date;

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
            ViewBag.designation = db.jntuh_designation.Where(c => c.isActive == true).ToList();
            ViewBag.Institutions = db.jntuh_college.Where(c => c.isActive == true).Select(c => new { CollegeId = c.id, CollegeName = c.collegeCode + "-" + c.collegeName }).OrderBy(c => c.CollegeName).ToList();



            FacultyRegistration regFaculty = new FacultyRegistration();

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

            jntuh_registered_faculty faculty = db.jntuh_registered_faculty.Find(fID);
            regFaculty.id = fID;
            regFaculty.Type = faculty.type;
            regFaculty.NewPassword = "TEMP@PWD";
            regFaculty.ConfirmPassword = "TEMP@PWD";
            int facultyUserId = db.jntuh_registered_faculty.Find(regFaculty.id).UserId;
            regFaculty.UserName = db.my_aspnet_users.Where(u => u.id == facultyUserId).Select(u => u.name).FirstOrDefault();
            regFaculty.RegistrationNumber = faculty.RegistrationNumber;
            regFaculty.UniqueID = faculty.UniqueID;
            regFaculty.FirstName = faculty.FirstName;
            regFaculty.MiddleName = faculty.MiddleName;
            regFaculty.LastName = faculty.LastName;
            regFaculty.GenderId = faculty.GenderId;
            regFaculty.FatherOrhusbandName = faculty.FatherOrHusbandName;
            regFaculty.MotherName = faculty.MotherName;
            if (faculty.DateOfBirth != null)
                regFaculty.facultyDateOfBirth = UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.DateOfBirth.ToString());
            regFaculty.OrganizationName = faculty.OrganizationName;
            regFaculty.DesignationId = faculty.DesignationId;
            if (faculty.DesignationId != null)
            {
                regFaculty.designation = db.jntuh_designation.Find(faculty.DesignationId).designation;
            }
            regFaculty.DepartmentId = faculty.DepartmentId;
            if (faculty.DepartmentId != null)
            {
                regFaculty.department = db.jntuh_department.Find(faculty.DepartmentId).departmentName;
            }
            if (faculty.collegeId != null)
            {
                regFaculty.CollegeName = db.jntuh_college.Find(faculty.collegeId).collegeName;
            }

            regFaculty.CollegeId = faculty.collegeId;
            regFaculty.WorkingStatus = faculty.WorkingStatus;
            regFaculty.OtherDepartment = faculty.OtherDepartment;
            regFaculty.OtherDesignation = faculty.OtherDesignation;
            if (faculty.DateOfAppointment != null)
                regFaculty.facultyDateOfAppointment = UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.DateOfAppointment.ToString());
            regFaculty.isFacultyRatifiedByJNTU = faculty.isFacultyRatifiedByJNTU;
            if (faculty.DateOfRatification != null)
                regFaculty.facultyDateOfRatification = UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.DateOfRatification.ToString());
            regFaculty.ProceedingsNo = faculty.ProceedingsNumber;
            regFaculty.SelectionCommitteeProcedings = faculty.ProceedingDocument;
            regFaculty.ProceedingsNo = faculty.ProceedingsNumber;
            regFaculty.OrganizationName = faculty.OrganizationName;
            regFaculty.AICTEFacultyId = faculty.AICTEFacultyId;
            regFaculty.GrossSalary = faculty.grosssalary;
            regFaculty.TotalExperience = faculty.TotalExperience;
            regFaculty.TotalExperiencePresentCollege = faculty.TotalExperiencePresentCollege;
            //regFaculty.EditPANNumber = faculty.PANNumber;
            regFaculty.PANNumber = faculty.PANNumber;
            regFaculty.AadhaarNumber = faculty.AadhaarNumber;
            regFaculty.Email = faculty.Email;
            regFaculty.Mobile = faculty.Mobile;
            regFaculty.National = faculty.National;
            regFaculty.InterNational = faculty.InterNational;
            regFaculty.Citation = faculty.Citation;
            regFaculty.Awards = faculty.Awards;
            regFaculty.facultyPhoto = faculty.Photo;
            regFaculty.facultyPANCardDocument = faculty.PANDocument;
            regFaculty.facultyAadhaarCardDocument = faculty.AadhaarDocument;
            regFaculty.SelectionCommitteeProcedings = faculty.ProceedingDocument;

            regFaculty.isActive = faculty.isActive;
            regFaculty.isApproved = faculty.isApproved;
            regFaculty.isView = true;
            ViewBag.FacultyDetails = regFaculty;
            List<SelectListItem> ratifiedDuration = new List<SelectListItem>();
            for (int i = 1; i <= 10; i++)
            {
                ratifiedDuration.Add(new SelectListItem { Text = i.ToString(), Value = i.ToString() });
            }
            ViewBag.duration = ratifiedDuration;

            List<SelectListItem> prevExperience = new List<SelectListItem>();
            for (int i = 0; i <= 40; i++)
            {
                prevExperience.Add(new SelectListItem { Text = i.ToString(), Value = i.ToString() });
            }
            ViewBag.prevExperience = prevExperience;

            List<SelectListItem> years = new List<SelectListItem>();
            for (int i = 1940; i <= DateTime.Now.Year; i++)
            {
                years.Add(new SelectListItem { Text = i.ToString(), Value = i.ToString() });
            }
            ViewBag.years = years;

            List<SelectListItem> division = new List<SelectListItem>();
            for (int i = 1; i <= 5; i++)
            {
                division.Add(new SelectListItem { Text = i.ToString(), Value = i.ToString() });
            }
            ViewBag.division = division;

            return View(regFaculty);
        }

        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification,DataEntry")]
        [HttpGet]
        public ActionResult NotApprovedInformation(string fid, string Command, bool pan, string collegeid, string facultyid)
        {
            var notapproved = new PANReasons();
            List<PANReasons> NewPanReasons = new List<PANReasons>();
            NewPanReasons.Add(new PANReasons { Id = 1, Reason = "PAN Modifications" });
            NewPanReasons.Add(new PANReasons { Id = 2, Reason = "Absent" });
            NewPanReasons.Add(new PANReasons { Id = 3, Reason = "Certificates Incomplete" });
            NewPanReasons.Add(new PANReasons { Id = 4, Reason = "Not Qualified as per AICTE" });
            NewPanReasons.Add(new PANReasons { Id = 5, Reason = "No Selection committee" });
            NewPanReasons.Add(new PANReasons { Id = 6, Reason = "No Form16" });

            ViewBag.notapproved = NewPanReasons;
            TempData["collegeid"] = collegeid;
            TempData["facultyid"] = facultyid;
            TempData["fid"] = fid;
            return PartialView("NotApprovedInformationPAN", NewPanReasons);
        }

        //[Authorize(Roles = "Admin,SuperAdmin,FacultyVerification")]
        //[HttpPost]
        //public ActionResult NotApprovedFacultyInformation(string[] remarks, string fid, int facultyid, string others, string collegeid)
        //{
        //    int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
        //    var uid = db.jntuh_registered_faculty.Where(i => i.id == facultyid).FirstOrDefault();
        //    //var faculty_log = db.jntuh_registered_faculty_log.Where(i => i.UserId == uid.UserId && i.RegistrationNumber == uid.RegistrationNumber).FirstOrDefault();

        //    if (uid.UserId != null)
        //    {
        //        uid.updatedOn = DateTime.Now;
        //        uid.updatedBy = userID;
        //        uid.PanStatusAfterDE = "Not Approved";
        //        uid.isApproved = false;
        //        if (remarks != null)
        //        {
        //            uid.PanReasonAfterDE = string.Join(",", remarks) + (!string.IsNullOrEmpty(others) ? "," + others : null);

        //        }
        //        else
        //        {
        //            uid.PanReasonAfterDE = others;
        //        }
        //        db.SaveChanges();
        //    }

        //    //var education = db.jntuh_registered_faculty_education.Where(i => i.facultyId == facultyid).ToList();

        //    //foreach (var item in education)
        //    //{
        //    //    var log = education.Where(i => i.educationId == item.educationId && i.facultyId == facultyid).ToList();
        //    //    if (log.Count >= 1)
        //    //    {
        //    //        log.FirstOrDefault().courseStudied = item.courseStudied;
        //    //        log.FirstOrDefault().specialization = item.specialization;
        //    //        log.FirstOrDefault().passedYear = item.passedYear;
        //    //        log.FirstOrDefault().marksPercentage = item.marksPercentage;
        //    //        log.FirstOrDefault().division = item.division;
        //    //        log.FirstOrDefault().boardOrUniversity = item.boardOrUniversity;
        //    //        log.FirstOrDefault().placeOfEducation = item.placeOfEducation;
        //    //        log.FirstOrDefault().certificate = item.certificate;
        //    //        log.FirstOrDefault().updatedOn = DateTime.Now;
        //    //        log.FirstOrDefault().updatedBy = userID;
        //    //        // db.SaveChanges();
        //    //    }
        //    //}

        //    return RedirectToAction("Index", new { collegeid = collegeid });
        //}

        //[Authorize(Roles = "Admin,SuperAdmin,FacultyVerification")]

        //public ActionResult ApproveFaculty(string fid, int facultyid, bool pan, string collegeid)
        //{
        //    int userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);

        //    if (fid != null)
        //    {
        //        int fID = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(fid, WebConfigurationManager.AppSettings["CryptoKey"]));

        //        jntuh_registered_faculty jntuh_registered_faculty = db.jntuh_registered_faculty.Find(fID);
        //        jntuh_registered_faculty.updatedBy = userId;
        //        jntuh_registered_faculty.updatedOn = DateTime.Now;
        //        jntuh_registered_faculty.PanStatusAfterDE = "Approved";
        //        //jntuh_registered_faculty.DeactivatedOn = DateTime.Now;
        //        jntuh_registered_faculty.isApproved = true;


        //        db.Entry(jntuh_registered_faculty).State = EntityState.Modified;
        //        db.SaveChanges();
        //    }

        //    return RedirectToAction("Index", new { collegeid = collegeid });
        //}

        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification,DataEntry")]
        public ActionResult FacultyDataEntryExportExcel(int? collegeid, string type)
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
                jntuh_registered_faculty = db.jntuh_registered_faculty
                                             .Where(rf => strRegNoS.Contains(rf.RegistrationNumber) && rf.Notin116 != true)  //&& (rf.collegeId == null || rf.collegeId == collegeid)
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
                teachingFaculty = teachingFaculty.Where(m => m.isActive == true).OrderBy(f => f.department).ToList();//(f => f.updatedOn).ThenBy
                if (type == "Excel")
                {
                    Response.ClearContent();
                    Response.Buffer = true;
                    Response.AddHeader("content-disposition", "attachment; filename= '" + jntuh_collegeCode + "'-College Wise Faculty Details.xls");
                    Response.ContentType = "application/vnd.ms-excel";
                    return PartialView("~/Views/Reports/_FacultyDataEntryExportExcel.cshtml", teachingFaculty);

                }
                // return View(teachingFaculty);
            }
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification,DataEntry")]
        public ActionResult ConsideredFacultyDataEntryExportExcel(int? collegeid, string type)
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
                jntuh_registered_faculty = db.jntuh_registered_faculty
                                              .Where(rf => strRegNoS.Contains(rf.RegistrationNumber) && rf.Notin116 != true).ToList();

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
                    Response.AddHeader("content-disposition", "attachment; filename='" + jntuh_collegeCode + "'-Considered Faculty Details.xls");
                    Response.ContentType = "application/vnd.ms-excel";
                    return PartialView("~/Views/Reports/_ConsideredFacultyDataEntryExportExcel.cshtml", teachingFaculty);

                }
                // return View(teachingFaculty);
            }
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification,DataEntry")]
        public ActionResult NotConsideredFacultyDataEntryExportExcel(int? collegeid, string type)
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
                jntuh_registered_faculty = db.jntuh_registered_faculty
                                             .Where(rf => strRegNoS.Contains(rf.RegistrationNumber) && rf.Notin116 != true).ToList();

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
                teachingFaculty = teachingFaculty.Where(rf => rf.DeactivationNew != "" && rf.isActive == true).OrderBy(f => f.department).ToList();//(f => f.updatedOn).ThenBy
                if (type == "Excel")
                {
                    Response.ClearContent();
                    Response.Buffer = true;
                    Response.AddHeader("content-disposition", "attachment; filename='" + jntuh_collegeCode + "'-Not Considered Faculty Details.xls");
                    Response.ContentType = "application/vnd.ms-excel";
                    return PartialView("~/Views/Reports/_NotConsideredFacultyDataEntryExportExcel.cshtml", teachingFaculty);

                }
                // return View(teachingFaculty);
            }
            return RedirectToAction("Index");
        }
        #endregion

        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public ActionResult EditFacultyDetails(string fid, string collegeid, string Command, FacultyRegistration facultydetails)
        {
            ViewBag.fid = string.IsNullOrEmpty(fid) ? "0" : fid;
            int fID = 0;
            //int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);

            if (fid != null)
            {
                fID = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(fid, WebConfigurationManager.AppSettings["CryptoKey"]));
            }

            jntuh_registered_faculty faculty = db.jntuh_registered_faculty.Find(fID);
            if (faculty != null)
            {
                faculty.PANNumber = facultydetails.PANNumber;
                faculty.AadhaarNumber = facultydetails.AadhaarNumber;
                var regfaculty = faculty.jntuh_registered_faculty_education.Where(i => i.educationId == 4).FirstOrDefault();
                var modifiedfaculty = facultydetails.FacultyEducation.Where(i => i.educationId == 4).FirstOrDefault();
                regfaculty.specialization = modifiedfaculty.specialization;
                db.SaveChanges();
            }

            return RedirectToAction("FacultyVerification", "FacultyVerificationDENew", new { fid = fid });
        }

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

        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification,DataEntry")]
        public ActionResult FacultyVerificationPostDENew(FacultyRegistration faculty)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            var facultydetails = db.jntuh_registered_faculty.Where(i => i.RegistrationNumber == faculty.RegistrationNumber).FirstOrDefault();
            if (facultydetails != null)
            {
                if (faculty.Absent == true)
                    facultydetails.Absent = faculty.Absent;

                if (!String.IsNullOrEmpty(faculty.ModifiedPANNo))
                    facultydetails.ModifiedPANNumber = faculty.ModifiedPANNo.Trim();
                //PanStatusAfterDE column is Considered as ModifiedAadhaarNo from Date on 05-03-2018
                if (!String.IsNullOrEmpty(faculty.ModifiedAadhaarNo))
                    facultydetails.PanStatusAfterDE = faculty.ModifiedAadhaarNo.Trim();
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
                //if (faculty.NOForm16 == true)
                facultydetails.NoForm16 = faculty.NOForm16;
                facultydetails.NoForm26AS = faculty.NOForm26AS;
                facultydetails.Covid19 = faculty.Covid19;
                facultydetails.Maternity = faculty.Maternity;
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
                if (faculty.OriginalCertificatesnotshownFlag == true)
                    facultydetails.OriginalCertificatesNotShown = faculty.OriginalCertificatesnotshownFlag;



                if (faculty.Absent == true || faculty.OriginalCertificatesnotshownFlag == true || !String.IsNullOrEmpty(faculty.ModifiedPANNo) || !String.IsNullOrEmpty(faculty.ModifiedAadhaarNo)
                    || facultydetails.NoForm16 == true || facultydetails.NoForm26AS == true || facultydetails.Covid19 == true || facultydetails.Maternity == true)
                {
                    facultydetails.FacultyVerificationStatus = true;
                    facultydetails.updatedBy = userID;
                    facultydetails.updatedOn = DateTime.Now;
                    db.Entry(facultydetails).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }

            return RedirectToAction("FacultyDataEntry", "DataEntry", new { collegeid = faculty.CollegeId });
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
                facultydetails.updatedBy = userID;
                facultydetails.updatedOn = DateTime.Now;
                //facultydetails.InvalidPANNumber = false;
                //facultydetails.NoRelevantUG = string.Empty;
                //facultydetails.NoRelevantPG = string.Empty;
                //facultydetails.NORelevantPHD = string.Empty;
                //facultydetails.NoSCM = false;
                //facultydetails.NoForm16 = false;
                //facultydetails.NotQualifiedAsperAICTE = false;
                //facultydetails.MultipleRegInSameCollege = false;
                //facultydetails.MultipleRegInDiffCollege = false;
                //facultydetails.SamePANUsedByMultipleFaculty = false;
                //facultydetails.PhotoCopyofPAN = false;
                //facultydetails.AppliedPAN = false;
                //facultydetails.LostPAN = false;
                //facultydetails.OriginalsVerifiedUG = false;
                //facultydetails.OriginalsVerifiedPG = false;
                //facultydetails.OriginalsVerifiedPHD = false;
                //facultydetails.FacultyVerificationStatus = false;
                //facultydetails.IncompleteCertificates = false;
                //facultydetails.DeactivatedBy = userID;
                //facultydetails.DeactivatedOn = DateTime.Now;
                db.Entry(facultydetails).State = EntityState.Modified;
                db.SaveChanges();
            }
            return RedirectToAction("FacultyDataEntry", "FacultyVerificationDENew", new { collegeid = collegeid });
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
                jntuh_registered_faculty faculty = db.jntuh_registered_faculty.Find(fID);
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


        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification")]
        public ActionResult AdjunctIndex(int? collegeid)
        {

            var reg_adjunct_colleges = db.jntuh_registered_faculty.AsNoTracking().Where(i => i.type == "Adjunct").Select(i => i.collegeId).Distinct().ToArray();
            ViewBag.Colleges = db.jntuh_college.Where(c => c.isActive == true && reg_adjunct_colleges.Contains(c.id)).Select(c => new { collegeId = c.id, collegeName = c.collegeCode + "-" + c.collegeName }).OrderBy(c => c.collegeName).ToList();

            var jntuh_department = db.jntuh_department.ToList();
            List<FacultyRegistration> teachingFaculty = new List<FacultyRegistration>();

            if (collegeid != null)
            {
                List<jntuh_college_faculty_registered> jntuh_college_faculty_registered = db.jntuh_college_faculty_registered.Where(cf => cf.collegeId == collegeid).Select(cf => cf).ToList();
                string[] strRegNoS = db.jntuh_college_faculty_registered.Where(cf => cf.collegeId == collegeid).Select(cf => cf.RegistrationNumber).ToArray();
                List<jntuh_registered_faculty> jntuh_registered_faculty = new List<jntuh_registered_faculty>();
                jntuh_registered_faculty = db.jntuh_registered_faculty.AsNoTracking().Where(rf => strRegNoS.Contains(rf.RegistrationNumber) && rf.type == "Adjunct")  //&& (rf.collegeId == null || rf.collegeId == collegeid)
                                             .ToList();

                var data = jntuh_registered_faculty.Select(a => new FacultyRegistration
                {
                    id = a.id,
                    Type = a.type,
                    CollegeId = collegeid,
                    RegistrationNumber = a.RegistrationNumber,
                    UniqueID = a.UniqueID,
                    FirstName = a.FirstName,
                    MiddleName = a.MiddleName,
                    LastName = a.LastName,
                    GenderId = a.GenderId,
                    Email = a.Email,
                    facultyPhoto = a.Photo,
                    Mobile = a.Mobile,
                    PANNumber = a.PANNumber,
                    AadhaarNumber = a.AadhaarNumber,
                    isActive = a.isActive,
                    isApproved = a.isApproved,
                    department = jntuh_department.Where(d => d.id == a.DepartmentId).Select(d => d.departmentName).FirstOrDefault(),
                    SamePANNumberCount = jntuh_registered_faculty.Where(f => f.PANNumber == a.PANNumber).ToList().Count(),
                    SameAadhaarNumberCount = jntuh_registered_faculty.Where(f => f.AadhaarNumber == a.AadhaarNumber).ToList().Count(),
                    isVerified = isFacultyVerified(a.id),
                    DeactivationReason = a.DeactivationReason,
                    updatedOn = a.updatedOn,
                    FacultyVerificationStatus = a.FacultyVerificationStatus,
                    createdOn = jntuh_college_faculty_registered.Where(f => f.RegistrationNumber == a.RegistrationNumber).Select(f => f.createdOn).FirstOrDefault(),
                    jntuh_registered_faculty_education = a.jntuh_registered_faculty_education
                }).ToList();

                teachingFaculty.AddRange(data);
                teachingFaculty = teachingFaculty.Where(m => m.isActive == true).OrderBy(f => f.updatedOn).ThenBy(f => f.department).ToList();
                return View(teachingFaculty);
            }

            return View(teachingFaculty);

        }

        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification")]
        public ActionResult AdjunctFacultyVerification(string fid)
        {
            FacultyRegistration vFaculty = new FacultyRegistration();
            if (TempData["FACULTY"] != null)
            {
                vFaculty = (FacultyRegistration)TempData["FACULTY"];
            }

            int fID = 0;
            if (!string.IsNullOrEmpty(User.Identity.Name))
            {
                int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
                int facultyId = db.jntuh_registered_faculty.Where(f => f.UserId == userID).Select(f => f.id).FirstOrDefault();

                if (fid != null)
                {
                    fID = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(fid, WebConfigurationManager.AppSettings["CryptoKey"]));
                }

                //if (facultyId != fID && !Roles.IsUserInRole("Admin"))
                //{
                //    fID = facultyId;
                //}

            }
            else if (fid != null)
            {
                string fUser = ""; string fPwd = "";

                if (TempData["FUserName"] != null)
                {
                    fUser = TempData["FUserName"].ToString();
                }

                if (TempData["FPassword"] != null)
                {
                    fPwd = TempData["FPassword"].ToString();
                }

                if (Membership.ValidateUser(fUser.TrimEnd(' '), fPwd.TrimEnd(' ')))
                {
                    FormsAuthentication.SetAuthCookie(fUser, false);
                    //int facultyId = db.jntuh_registered_faculty.Where(f => f.UserId == userID).Select(f => f.id).FirstOrDefault();
                    //string fid = Utilities.EncryptString(facultyId.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]);

                    return RedirectToAction("AdjunctFacty", "OnlineRegistration", new { fid = fid });
                }

                //return RedirectToAction("Logon", "Account");
            }

            ViewBag.Id = fid;
            ViewBag.FacultyID = fID;

            DateTime todayDate = DateTime.Now.Date;

            List<DistinctDepartment> depts = new List<DistinctDepartment>();
            string existingDepts = string.Empty;
            int[] notRequiredIds = { 25, 26, 27, 33, 34, 36, 37, 38, 39, 53, 54, 55, 56 };
            foreach (var item in db.jntuh_department.Where(s => !notRequiredIds.Contains(s.id)).OrderBy(s => s.departmentName).ToList())
            {
                if (!existingDepts.Split(',').Contains(item.departmentName))
                {
                    depts.Add(new DistinctDepartment { id = item.id, departmentName = item.departmentName });
                    existingDepts = existingDepts + "," + item.departmentName;
                }
            }

            ViewBag.department = depts;
            ViewBag.designation = db.jntuh_designation.Where(c => c.isActive == true).ToList();
            ViewBag.Institutions = db.jntuh_college.Where(c => c.isActive == true).Select(c => new { CollegeId = c.id, CollegeName = c.collegeCode + "-" + c.collegeName }).OrderBy(c => c.CollegeName).ToList();

            List<SelectListItem> ratifiedDuration = new List<SelectListItem>();
            for (int i = 1; i <= 10; i++)
            {
                ratifiedDuration.Add(new SelectListItem { Text = i.ToString(), Value = i.ToString() });
            }
            ViewBag.duration = ratifiedDuration;

            List<SelectListItem> prevExperience = new List<SelectListItem>();
            for (int i = 0; i <= 40; i++)
            {
                prevExperience.Add(new SelectListItem { Text = i.ToString(), Value = i.ToString() });
            }
            ViewBag.prevExperience = prevExperience;

            List<SelectListItem> years = new List<SelectListItem>();
            for (int i = 1940; i <= DateTime.Now.Year; i++)
            {
                years.Add(new SelectListItem { Text = i.ToString(), Value = i.ToString() });
            }
            ViewBag.years = years;

            List<SelectListItem> division = new List<SelectListItem>();
            for (int i = 1; i <= 5; i++)
            {
                division.Add(new SelectListItem { Text = i.ToString(), Value = i.ToString() });
            }
            ViewBag.division = division;

            FacultyRegistration regFaculty = new FacultyRegistration();

            if (fID == 0)
            {
                regFaculty.FacultyEducation = db.jntuh_education_category.Where(e => e.isActive == true && (e.id == 1 || e.id == 3 || e.id == 4 || e.id == 5 || e.id == 6 || e.id == 8))
                                                            .Select(e => new RegisteredFacultyEducation
                                                            {
                                                                educationId = e.id,
                                                                educationName = e.educationCategoryName,
                                                                studiedEducation = string.Empty,
                                                                specialization = string.Empty,
                                                                passedYear = 0,
                                                                percentage = 0,
                                                                division = 0,
                                                                university = string.Empty,
                                                                place = string.Empty,
                                                                facultyCertificate = string.Empty,
                                                            }).ToList();
            }
            else
            {
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
            }

            regFaculty.GenderId = null;
            regFaculty.isFacultyRatifiedByJNTU = null;

            if (fID > 0)
            {
                jntuh_registered_faculty faculty = db.jntuh_registered_faculty.Find(fID);
                regFaculty.id = fID;
                regFaculty.Type = faculty.type;
                regFaculty.NewPassword = "TEMP@PWD";
                regFaculty.ConfirmPassword = "TEMP@PWD";
                int facultyUserId = db.jntuh_registered_faculty.Find(regFaculty.id).UserId;
                regFaculty.UserName = db.my_aspnet_users.Where(u => u.id == facultyUserId).Select(u => u.name).FirstOrDefault();
                regFaculty.RegistrationNumber = faculty.RegistrationNumber;
                regFaculty.UniqueID = faculty.UniqueID;
                regFaculty.FirstName = faculty.FirstName;
                regFaculty.MiddleName = faculty.MiddleName;
                regFaculty.LastName = faculty.LastName;
                regFaculty.GenderId = faculty.GenderId;
                regFaculty.FatherOrhusbandName = faculty.FatherOrHusbandName;
                regFaculty.MotherName = faculty.MotherName;
                if (faculty.DateOfBirth != null)
                    regFaculty.facultyDateOfBirth = UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.DateOfBirth.ToString());
                regFaculty.OrganizationName = faculty.OrganizationName;
                regFaculty.DesignationId = faculty.DesignationId;
                if (faculty.DesignationId != null)
                {
                    regFaculty.designation = db.jntuh_designation.Find(faculty.DesignationId).designation;
                }
                regFaculty.DepartmentId = faculty.DepartmentId;
                if (faculty.DepartmentId != null)
                {
                    regFaculty.department = db.jntuh_department.Find(faculty.DepartmentId).departmentName;
                }
                if (faculty.collegeId != null)
                {
                    regFaculty.CollegeName = db.jntuh_college.Find(faculty.collegeId).collegeName;
                }

                regFaculty.CollegeId = faculty.collegeId;
                regFaculty.WorkingStatus = faculty.WorkingStatus;
                regFaculty.OtherDepartment = faculty.OtherDepartment;
                regFaculty.OtherDesignation = faculty.OtherDesignation;
                if (faculty.DateOfAppointment != null)
                    regFaculty.facultyDateOfAppointment = UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.DateOfAppointment.ToString());
                regFaculty.isFacultyRatifiedByJNTU = faculty.isFacultyRatifiedByJNTU;
                if (faculty.DateOfRatification != null)
                    regFaculty.facultyDateOfRatification = UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.DateOfRatification.ToString());
                regFaculty.ProceedingsNo = faculty.ProceedingsNumber;
                regFaculty.SelectionCommitteeProcedings = faculty.ProceedingDocument;
                regFaculty.AICTEFacultyId = faculty.AICTEFacultyId;
                regFaculty.GrossSalary = faculty.grosssalary;
                regFaculty.TotalExperience = faculty.TotalExperience;
                regFaculty.TotalExperiencePresentCollege = faculty.TotalExperiencePresentCollege;
                regFaculty.PANNumber = faculty.PANNumber;
                regFaculty.AadhaarNumber = faculty.AadhaarNumber;
                regFaculty.Email = faculty.Email;
                regFaculty.Mobile = faculty.Mobile;
                regFaculty.National = faculty.National;
                regFaculty.InterNational = faculty.InterNational;
                regFaculty.Citation = faculty.Citation;
                regFaculty.Awards = faculty.Awards;
                regFaculty.facultyPhoto = faculty.Photo;
                regFaculty.facultyPANCardDocument = faculty.PANDocument;
                regFaculty.facultyAadhaarCardDocument = faculty.AadhaarDocument;
                regFaculty.isActive = faculty.isActive;
                regFaculty.isApproved = faculty.isApproved;
                regFaculty.isView = true;
                regFaculty.NOCFile = faculty.NOCFile;
                regFaculty.PresentInstituteAssignedRole = faculty.PresentInstituteAssignedRole;
                regFaculty.PresentInstituteAssignedResponsebility = faculty.PresentInstituteAssignedResponsebility;
                regFaculty.Accomplish1 = faculty.Accomplish1;
                regFaculty.Accomplish2 = faculty.Accomplish2;
                regFaculty.Accomplish3 = faculty.Accomplish3;
                regFaculty.Accomplish4 = faculty.Accomplish4;
                regFaculty.Accomplish5 = faculty.Accomplish5;
                regFaculty.Professional = faculty.Professional;
                regFaculty.Professional2 = faculty.Professional2;
                regFaculty.Professiona3 = faculty.Professiona3;
                regFaculty.MembershipNo1 = faculty.MembershipNo1;
                regFaculty.MembershipNo2 = faculty.MembershipNo2;
                regFaculty.MembershipNo3 = faculty.MembershipNo3;
                regFaculty.MembershipCertificate1 = faculty.MembershipCertificate1;
                regFaculty.MembershipCertificate2 = faculty.MembershipCertificate2;
                regFaculty.MembershipCertificate3 = faculty.MembershipCertificate3;
                regFaculty.AdjunctDepartment = faculty.AdjunctDepartment;
                regFaculty.AdjunctDesignation = faculty.AdjunctDesignation;
                regFaculty.WorkingType = faculty.WorkingType;

                TempData["FacultyDetails"] = regFaculty;
                ViewBag.id = regFaculty.id;
            }
            else
            {
                if (vFaculty.isView != null)
                {
                    regFaculty = vFaculty;

                    if (vFaculty.CollegeId != null)
                    {
                        regFaculty.CollegeName = db.jntuh_college.Find(vFaculty.CollegeId).collegeName;
                    }

                    if (vFaculty.DesignationId != null)
                    {
                        regFaculty.designation = db.jntuh_designation.Find(vFaculty.DesignationId).designation;
                    }

                    if (vFaculty.DepartmentId != null)
                    {
                        regFaculty.department = db.jntuh_department.Find(vFaculty.DepartmentId).departmentName;
                    }

                    regFaculty.OtherDepartment = vFaculty.OtherDepartment;
                    regFaculty.OtherDesignation = vFaculty.OtherDesignation;
                    regFaculty.isFacultyRatifiedByJNTU = vFaculty.isFacultyRatifiedByJNTU;


                }
            }
            ViewBag.HideVerifyLink = regFaculty.isApproved != null ? true : false;
            return View(regFaculty);
        }


        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification,DataEntry")]
        [HttpGet]
        public ActionResult OldNotApprovedInformation(string fid, string Command, bool pan, string collegeid)
        {

            var notapproved = db.jntuh_faculty_deactivation_reason.ToList();
            ViewBag.notapproved = notapproved;
            TempData["collegeid"] = collegeid;
            TempData["fid"] = fid;
            return PartialView("OldNotApprovedInformation", notapproved);
        }

        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification")]
        [HttpGet]
        public ActionResult Approve(string fid, int collegeId, bool pan, CollegFacultyVerification faculty)
        {
            int userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);

            if (fid != null)
            {
                int fID = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(fid, WebConfigurationManager.AppSettings["CryptoKey"]));

                jntuh_registered_faculty jntuh_registered_faculty = db.jntuh_registered_faculty.Find(fID);
                jntuh_registered_faculty.updatedBy = userId;
                jntuh_registered_faculty.updatedOn = DateTime.Now;

                jntuh_registered_faculty.isApproved = true;

                if (pan == false)
                {
                    jntuh_registered_faculty.DeactivationReason = "PAN NUMBER APPROVAL PENDING";
                }

                db.Entry(jntuh_registered_faculty).State = EntityState.Modified;
                db.SaveChanges();
            }

            return RedirectToAction("Index", new { collegeid = collegeId });

            //CollegFacultyVerification faculty = new CollegFacultyVerification();

            //if (fid != null)
            //{
            //    int fID = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(fid, WebConfigurationManager.AppSettings["CryptoKey"]));
            //    string regNo = db.jntuh_registered_faculty.Where(of => of.id == fID).Select(of => of.RegistrationNumber).FirstOrDefault();
            //    int facultyId = db.jntuh_college_faculty_registered.Where(of => of.RegistrationNumber == regNo).Select(of => of.id).FirstOrDefault();

            //    faculty.FacultyId = facultyId;
            //}

            //return View(faculty);
        }


        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification")]
        public ActionResult AdjunctFacultyVerificationEdit(string fid, string collegeid)
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
            return PartialView("_AdjunctFacultyVerificationEdit", regFaculty);
        }

        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification")]
        public ActionResult AdjunctFacultyVerificationNoEdit(string fid, string collegeid)
        {
            int fID = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(fid, WebConfigurationManager.AppSettings["CryptoKey"]));
            var facultydetails = db.jntuh_registered_faculty.Find(fID);
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
                //facultydetails.MultipleRegInSameCollege = false;
                //facultydetails.MultipleRegInDiffCollege = false;
                //facultydetails.SamePANUsedByMultipleFaculty = false;
                //facultydetails.PhotoCopyofPAN = false;
                //facultydetails.AppliedPAN = false;
                //facultydetails.LostPAN = false;
                facultydetails.OriginalsVerifiedUG = false;
                facultydetails.OriginalsVerifiedPG = false;
                facultydetails.OriginalsVerifiedPHD = false;
                facultydetails.FacultyVerificationStatus = false;
                facultydetails.Others1 = null;
                facultydetails.Others2 = null;
                facultydetails.IncompleteCertificates = false;
                db.SaveChanges();
            }
            return RedirectToAction("AdjunctIndex", "FacultyVerificationDENew", new { collegeid = collegeid });
        }

        [Authorize(Roles = "Admin,SuperAdmin")]
        public ActionResult AdjunctFacultyVerificationCheck(string fid, string collegeid)
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
                regFaculty.Absent = faculty.Absent != false ? true : false;
                regFaculty.InvalidPANNo = faculty.InvalidPANNumber != false ? true : false;
                regFaculty.NoSCM = faculty.NoSCM != false ? true : false;
                regFaculty.NORelevantPG = faculty.NoRelevantPG;
                regFaculty.NOForm16 = faculty.NoForm16 != false ? true : false;
                regFaculty.NOTQualifiedAsPerAICTE = faculty.NotQualifiedAsperAICTE != false ? true : false;
                regFaculty.ModifiedPANNo = faculty.ModifiedPANNumber;
                regFaculty.InCompleteCeritificates = faculty.IncompleteCertificates != false ? true : false;
                regFaculty.NONoc = faculty.Others1 != null ? true : false;
                regFaculty.NOProfessionalBodiesMembership = faculty.Others2 != null ? true : false;
            }
            return PartialView("_AdjunctFacultyVerificationCheck", regFaculty);
        }
        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification")]


        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification")]
        public ActionResult AdjunctFacultyVerificationPostDENew(FacultyRegistration faculty)
        {
            var facultydetails = db.jntuh_registered_faculty.Where(i => i.RegistrationNumber == faculty.RegistrationNumber).FirstOrDefault();
            if (facultydetails != null)
            {
                facultydetails.Absent = faculty.Absent;
                if (faculty.ModifiedPANNo != null)
                {
                    facultydetails.PANNumber = faculty.ModifiedPANNo;
                }
                if (faculty.MOdifiedDateofAppointment1 != null)
                {
                    facultydetails.DateOfAppointment = Convert.ToDateTime(faculty.MOdifiedDateofAppointment1);
                }
                if (faculty.DepartmentId != null)
                {
                    facultydetails.DepartmentId = faculty.DepartmentId;
                }
                if (faculty.NONoc == true)
                {
                    facultydetails.Others1 = "NONOC";
                }
                if (faculty.NOProfessionalBodiesMembership == true)
                {
                    facultydetails.Others2 = "NOProfessionalBodiesMembership";
                }


                facultydetails.InvalidPANNumber = faculty.InvalidPANNo;
                facultydetails.NoRelevantUG = faculty.NORelevantUG;
                facultydetails.NoRelevantPG = faculty.NORelevantPG;
                facultydetails.NORelevantPHD = faculty.NORelevantPHD;
                facultydetails.NoSCM = faculty.NoSCM;
                facultydetails.NoForm16 = faculty.NOForm16;
                facultydetails.NotQualifiedAsperAICTE = faculty.NOTQualifiedAsPerAICTE;
                //facultydetails.MultipleRegInSameCollege = faculty.MultipleReginSamecoll;
                //facultydetails.MultipleRegInDiffCollege = faculty.MultipleReginDiffcoll;
                //facultydetails.SamePANUsedByMultipleFaculty = faculty.SamePANUsedByMultipleFaculty;
                //facultydetails.PhotoCopyofPAN = faculty.PhotocopyofPAN;
                //facultydetails.AppliedPAN = faculty.AppliedPAN;
                //facultydetails.LostPAN = faculty.LostPAN;
                facultydetails.OriginalsVerifiedUG = faculty.OriginalsVerifiedUG;
                facultydetails.OriginalsVerifiedPG = faculty.OriginalsVerifiedPG;
                facultydetails.OriginalsVerifiedPHD = faculty.OriginalsVerifiedPHD;
                facultydetails.IncompleteCertificates = faculty.InCompleteCeritificates;
                facultydetails.FacultyVerificationStatus = true;
                db.SaveChanges();
            }

            return RedirectToAction("AdjunctIndex", "FacultyVerificationDENew", new { collegeid = faculty.CollegeId });
        }
        public class PANReasons
        {
            public int Id { get; set; }
            public string Reason { get; set; }
        }


        #region HighestDegreeEducation

        [HttpGet]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public ActionResult HighestDegreeEducation()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public ActionResult HighestDegreeEducation(FacultyRegistration regno)
        {
            if (regno != null)
            {
                var facultyId = db.jntuh_registered_faculty.Where(e => e.RegistrationNumber.Trim() == regno.RegistrationNumber.Trim()).Select(e => e.id).FirstOrDefault();

                if (facultyId != 0)
                {
                    var facultyeducationdetails = db.jntuh_registered_faculty_education.Where(i => i.facultyId == facultyId).ToList();
                    var jntuhRegisteredFacultyEducation = facultyeducationdetails.LastOrDefault();
                    if (jntuhRegisteredFacultyEducation != null && (facultyeducationdetails.Count > 0 && jntuhRegisteredFacultyEducation.educationId != 4))
                    {

                        var regfaculty = db.jntuh_registered_faculty.Find(facultyId);
                        regfaculty.NotQualifiedAsperAICTE = false;
                        db.Entry(regfaculty).State = EntityState.Modified;
                        db.SaveChanges();


                        var fdetails = jntuhRegisteredFacultyEducation;
                        fdetails.facultyId = facultyId;
                        fdetails.educationId = 4;
                        fdetails.courseStudied = "Test";
                        fdetails.specialization = "Test";
                        fdetails.passedYear = 2015;
                        fdetails.marksPercentage = 10;
                        fdetails.division = 1;
                        fdetails.boardOrUniversity = "Test";
                        fdetails.placeOfEducation = "Test";
                        fdetails.certificate = "Test";
                        fdetails.isActive = true;
                        fdetails.createdOn = DateTime.Now;
                        fdetails.createdBy = 1;
                        fdetails.updatedOn = DateTime.Now;
                        fdetails.updatedBy = 1;
                        db.jntuh_registered_faculty_education.Add(fdetails);
                        db.Entry(fdetails).State = EntityState.Added;
                        db.SaveChanges();
                        TempData["Success"] = "Highest Degree Added Successfully";
                        return RedirectToAction("HighestDegreeEducation");
                    }

                    else
                    {
                        TempData["Error"] = "Already has Highest Degree";
                    }
                }
                else
                {
                    TempData["Error"] = "Registration Number Not Found...";
                }
            }
            return RedirectToAction("HighestDegreeEducation");
        }

        #endregion

        #region Add PAN Number
        public class FacultyRegistractionaddPan
        {
            [Required(ErrorMessage = "This field is required")]
            [Display(Name = "Faculty Registration ID")]
            public string RegistrationNumber { get; set; }
            [Required(ErrorMessage = "This field is required")]
            [RegularExpression(@"[A-Z]{3}[P][A-Z]{1}\d{4}[A-Z]{1}", ErrorMessage = "Invalid PAN number")]
            [Display(Name = "PAN Number")]

            public string PANNumber { get; set; }

        }
        [HttpGet]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public ActionResult AddPANNumber()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public ActionResult AddPANNumber(FacultyRegistractionaddPan regno)
        {
            if (regno != null)
            {
                var facultyId = db.jntuh_registered_faculty.Where(e => e.RegistrationNumber.Trim() == regno.RegistrationNumber.Trim()).Select(e => e.id).FirstOrDefault();

                if (facultyId != 0)
                {
                    var facultydetails = db.jntuh_registered_faculty.Find(facultyId);
                    facultydetails.PANNumber = regno.PANNumber;
                    db.Entry(facultydetails).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Success"] = "PAN Number Added Successfully";
                    return RedirectToAction("AddPANNumber");
                }
                else
                {
                    TempData["Error"] = "Registration Number Not Found...";
                }
            }
            return RedirectToAction("AddPANNumber");
        }
        #endregion



        #region For Specialization adding for college faculty registered

        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification,DataEntry")]
        public ActionResult CollegeFacultySpecializationScreen(int? pageNumber)
        {
            var teachingFaculty = new List<FacultyRegistration>();


            var jntuhcollege = db.jntuh_college.AsNoTracking().ToList();
            var jntuhCollegeFacultyRegistered =
                db.jntuh_college_faculty_registered.Where(
                    cf => cf.SpecializationId == null && (cf.IdentifiedFor == "PG" || cf.IdentifiedFor == "UG&PG"))
                    .Select(cf => cf)
                    .ToList();
            string[] strRegNoS = jntuhCollegeFacultyRegistered.Select(cf => cf.RegistrationNumber).ToArray();
            int Collegeid, Collegeid1 = 0;
            var jntuh_registered_faculty = new List<jntuh_registered_faculty>();
            jntuh_registered_faculty = db.jntuh_registered_faculty
                .Where(rf => strRegNoS.Contains(rf.RegistrationNumber) && rf.NoSpecialization != true)
                //&& (rf.collegeId == null || rf.collegeId == collegeid)
                .ToList();
            int[] REGIDS = jntuh_registered_faculty.Select(F => F.id).ToArray();
            var FacultyEducation =
                db.jntuh_registered_faculty_education.Where(E => REGIDS.Contains(E.facultyId)).ToList();
            //var CollegeFaculty = db.jntuh_college_faculty_registered.Select(C => new { C.collegeId, C.RegistrationNumber }).ToList();
            Collegeid1 = jntuhCollegeFacultyRegistered.Where(C => C.RegistrationNumber.Trim() == "7791-150426-124035").Select(C => C.collegeId).FirstOrDefault();
            var data = jntuh_registered_faculty.Select(a => new FacultyRegistration
            {
                id = a.id,
                Type = a.type,
                RegistrationNumber = a.RegistrationNumber,
                UniqueID = a.UniqueID,
                FirstName = a.FirstName,
                MiddleName = a.MiddleName,
                LastName = a.LastName,
                GenderId = a.GenderId,
                Email = a.Email,
                facultyPhoto = a.Photo,
                Mobile = a.Mobile,
                PANNumber = a.PANNumber,
                AadhaarNumber = a.AadhaarNumber,
                SSC =
                    FacultyEducation.Where(E => E.educationId == 1 && E.facultyId == a.id)
                        .Select(E => E.certificate)
                        .FirstOrDefault(),
                UG =
                    FacultyEducation.Where(E => E.educationId == 3 && E.facultyId == a.id)
                        .Select(E => E.certificate)
                        .FirstOrDefault(),
                PG =
                    FacultyEducation.Where(E => E.educationId == 4 && E.facultyId == a.id)
                        .Select(E => E.certificate)
                        .FirstOrDefault(),
                PHD =
                    FacultyEducation.Where(E => E.educationId == 5 && E.facultyId == a.id)
                        .Select(E => E.certificate)
                        .FirstOrDefault(),
                CollegeId = Collegeid = jntuhCollegeFacultyRegistered.Where(C => C.RegistrationNumber.Trim() == a.RegistrationNumber).Select(C => C.collegeId).FirstOrDefault(),
                CollegeName = jntuhcollege.Where(i => i.id == Collegeid).Select(i => i.collegeName).FirstOrDefault(),



            }).ToList();

            teachingFaculty.AddRange(data);
            var collegeid = teachingFaculty.Where(C => C.RegistrationNumber == "7791-150426-124035").ToList();
            #region Paging
            int totalPages = 1;
            int totalLabs = teachingFaculty.Count();
            int First = 0;
            int second = 0;
            if (totalLabs > 20)
            {
                totalPages = totalLabs / 20;
                if (totalLabs > 20 * totalPages)
                {
                    totalPages = totalPages + 1;
                }
            }
            if (pageNumber == null || pageNumber == 1)
            {
                First = 0;
                second = 20;
            }
            else if (pageNumber > 1)
            {
                First = ((pageNumber ?? default(int)) * 20) - 20;

                //second = (pageNumber ?? default(int)) * 100;
                second = 20;
                int Total = teachingFaculty.Count;
                if (Total <= ((pageNumber ?? default(int)) * 20))
                {

                    second = Total - First;
                }
            }
            ViewBag.Pages = totalPages;
            ViewBag.count = teachingFaculty.Count;
            //teachingFaculty = teachingFaculty.GetRange(First, totalLabs > 20 ? second : totalLabs);
            #endregion
            return View(teachingFaculty);
        }

        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification,DataEntry")]
        [HttpGet]
        public ActionResult FacultySpecializationView(int collegeId, string regno)
        {
            var faculty = new CollegeFaculty();
            var existingfaculty = db.jntuh_registered_faculty.FirstOrDefault(i => i.RegistrationNumber.Trim() == regno.Trim()); //&& i.collegeId == collegeId
            if (existingfaculty != null)
            {
                faculty.collegeId = collegeId;
                faculty.id = existingfaculty.id;
                faculty.facultyFirstName = existingfaculty.FirstName;
                faculty.facultyLastName = existingfaculty.LastName;
                faculty.facultySurname = existingfaculty.MiddleName;
                faculty.facultyDesignationId = existingfaculty.DesignationId;
                faculty.designation = db.jntuh_designation.Where(d => d.id == faculty.facultyDesignationId).Select(d => d.designation).FirstOrDefault();
                faculty.facultyOtherDesignation = existingfaculty.OtherDesignation;
                if (existingfaculty.DepartmentId != null)
                    faculty.facultyDepartmentId = (int)existingfaculty.DepartmentId;
                faculty.department = db.jntuh_department.Where(d => d.id == faculty.facultyDepartmentId).Select(d => d.departmentName).FirstOrDefault();
                faculty.FacultyRegistrationNumber = regno;
                faculty.facultyRecruitedFor =
                    db.jntuh_college_faculty_registered.Where(i => i.RegistrationNumber == regno)
                        .Select(i => i.IdentifiedFor)
                        .FirstOrDefault();
            }

            var pgSpecializations = db.jntuh_college_intake_existing
                                         .Where(e => e.jntuh_specialization.jntuh_department.jntuh_degree.id != 4 && e.jntuh_specialization.jntuh_department.jntuh_degree.id != 5)
                                         .Select(e => new { id = e.jntuh_specialization.id, spec = e.jntuh_specialization.specializationName })
                                         .GroupBy(e => new { e.id, e.spec })
                                         .OrderBy(e => e.Key.spec)
                                         .Select(e => new { id = e.Key.id, spec = e.Key.spec }).ToList();
            ViewBag.PGSpecializations = pgSpecializations;
            return PartialView(faculty);
        }

        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification,DataEntry")]
        [HttpPost]
        public ActionResult FacultySpecializationCollege(CollegeFaculty faculty)
        {
            TempData["Error"] = null;
            //int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            //var isRegisteredFaculty = db.jntuh_registered_faculty.Where(r => r.RegistrationNumber == faculty.FacultyRegistrationNumber).Select(r => r).FirstOrDefault();
            var isExistingFaculty = db.jntuh_college_faculty_registered.FirstOrDefault(r => r.RegistrationNumber == faculty.FacultyRegistrationNumber);
            if (isExistingFaculty != null)
            {
                isExistingFaculty.IdentifiedFor = faculty.facultyRecruitedFor;
                isExistingFaculty.SpecializationId = faculty.SpecializationId;
                isExistingFaculty.updatedBy = 375;
                isExistingFaculty.updatedOn = DateTime.Now;
                db.Entry(isExistingFaculty).State = EntityState.Modified;
                db.SaveChanges();
                TempData["Success"] = "Faculty Specialization (" + faculty.FacultyRegistrationNumber + " ) Successfully Updated ..";
                TempData["Error"] = null;
            }

            return RedirectToAction("CollegeFacultySpecializationScreen", "FacultyVerificationDENew");
        }

        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification,DataEntry")]
        public ActionResult FacultyNOSpecialization(int collegeId, string regno)
        {
            TempData["Error"] = null;
            var isExistingFaculty = db.jntuh_registered_faculty.FirstOrDefault(r => r.RegistrationNumber == regno && r.collegeId == collegeId);
            if (isExistingFaculty != null)
            {
                isExistingFaculty.NoSpecialization = true;
                isExistingFaculty.updatedOn = DateTime.Now;
                isExistingFaculty.updatedBy = 375;
                db.SaveChanges();
                TempData["Success"] = "Specialization Inactivated Successfully for (" + regno + ") ..";
            }
            else
            {
                TempData["Error"] = "Registration number doesn't exist.";
            }
            return RedirectToAction("CollegeFacultySpecializationScreen", "FacultyVerificationDENew");

        }


        #endregion


        #region For pharmacy ug faculty specialization adding

        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification,DataEntry")]
        public ActionResult PharmacyFacultySpecializationScreen(int? collegeId)
        {
            //Written By siva
            //Pharmacy Colleges 
            var CollegeIds = new int[] { 448, 180, 159, 376, 428, 445, 24, 117, 202, 213, 395, 364, 27, 30, 6, 34, 55, 58, 54, 52, 9, 60, 65, 78, 150, 204, 219, 90, 97, 104, 110, 139, 105, 107, 114, 379, 118, 39, 120, 389, 127, 237, 135, 136, 47, 252, 169, 253, 262, 442, 301, 436, 370, 263, 267, 206, 140, 44, 290, 454, 42, 95, 297, 384, 75, 348, 298, 295, 303, 302, 283, 332, 410, 392, 234, 146, 315, 320, 317, 314, 318, 319, 353, 313, 235, 375 };
            // var CollegeIds = new int[] { 6, 24, 27, 30, 34, 44, 47, 52, 54, 55, 58, 60, 65, 78, 90, 95, 97, 104, 105, 107, 110, 114, 117, 118, 120, 127, 135, 136, 139, 146, 150, 159, 169, 202, 204, 206, 213, 219, 234, 237, 252, 253, 262, 263, 267, 283, 290, 295, 297, 298, 301, 302, 303, 313, 314, 315, 317, 318, 319, 320, 348, 353, 370, 376, 379, 384, 389, 392, 395, 410, 428, 436, 442, 445, 448, 454 };
            var jntuhcollege = db.jntuh_college.AsNoTracking().Where(e => CollegeIds.Contains(e.id)).ToList();

            ViewBag.PharmacyCollegeList = jntuhcollege.Select(e => new
            {
                collegeId = e.id,
                collegeName = e.collegeCode + "-" + e.collegeName
            }).ToList();

            if (collegeId != null)
            {

                TempData["collegeId"] = collegeId;
                var jntuh_college_faculty_registered_copy = db.jntuh_college_faculty_registered_copy.Where(e => e.collegeId == 375).Select(e => e).ToList();

                var jntuh_specialization = db.jntuh_specialization.Where(e => e.isActive == true).ToList();

                var jntuh_college = jntuhcollege.Where(e => e.id == collegeId).Select(e => e).FirstOrDefault();
                var teachingFaculty = new List<FacultyRegistration>();
                var jntuh_college_faculty_registered = db.jntuh_college_faculty_registered.Where(e => e.collegeId == collegeId && (e.DepartmentId == 26 || e.DepartmentId == 36 || e.DepartmentId == 27 || e.DepartmentId == 39 || e.DepartmentId == 61)).Select(e => e).ToList();
                var strRegnos = jntuh_college_faculty_registered.Select(e => e.RegistrationNumber).ToList();

                var jntuh_registered_faculty = db.jntuh_registered_faculty.Where(e => strRegnos.Contains(e.RegistrationNumber.Trim())).Select(e => e).ToList();
                var FacultyIds = jntuh_registered_faculty.Select(e => e.id).ToList();

                var jntuh_registered_faculty_education = db.jntuh_registered_faculty_education.Where(e => FacultyIds.Contains(e.facultyId)).Select(e => e).ToList();

                var jntuh_scmupload = db.jntuh_scmupload.Where(e => e.CollegeId == collegeId).Select(e => e).ToList();


                List<FacultyRegistration> data = new List<FacultyRegistration>();
                foreach (var item in jntuh_registered_faculty)
                {
                    FacultyRegistration Faculty = new FacultyRegistration();
                    Faculty.id = item.id;
                    Faculty.Type = item.type;
                    Faculty.RegistrationNumber = item.RegistrationNumber;
                    Faculty.UniqueID = item.UniqueID;
                    Faculty.FirstName = item.FirstName;
                    Faculty.MiddleName = item.MiddleName;
                    Faculty.LastName = item.LastName;
                    Faculty.GenderId = item.GenderId;
                    Faculty.Email = item.Email;
                    Faculty.facultyPhoto = item.Photo;
                    Faculty.Mobile = item.Mobile;
                    Faculty.PANNumber = item.PANNumber;
                    Faculty.AadhaarNumber = item.AadhaarNumber;
                    Faculty.DegreeId = jntuh_registered_faculty_education.Count(e => e.facultyId == item.id) > 0 ? item.jntuh_registered_faculty_education.Where(e => e.facultyId == item.id).Select(e => e.educationId).Max() : 0;
                    Faculty.PANNumber = item.PANNumber;
                    Faculty.XeroxcopyofcertificatesFlag = item.Xeroxcopyofcertificates != null ? (bool)item.Xeroxcopyofcertificates : false;
                    Faculty.NOrelevantUgFlag = item.NoRelevantUG == "No" ? false : true;
                    Faculty.NOrelevantPgFlag = item.NoRelevantPG == "No" ? false : true;
                    Faculty.NOrelevantPhdFlag = item.NORelevantPHD == "No" ? false : true;
                    Faculty.NOTQualifiedAsPerAICTE = item.NotQualifiedAsperAICTE != null ? (bool)item.NotQualifiedAsperAICTE : false;
                    Faculty.InvalidPANNo = item.InvalidPANNumber != null ? (bool)item.InvalidPANNumber : false;
                    Faculty.InCompleteCeritificates = item.IncompleteCertificates != null ? (bool)item.IncompleteCertificates : false;
                    Faculty.NoSCM = item.NoSCM != null ? (bool)item.NoSCM : false;
                    Faculty.Deactivedby = item.DeactivatedBy;
                    Faculty.DeactivedOn = item.DeactivatedOn;

                    Faculty.SpecializationId = jntuh_college_faculty_registered.Where(e => e.RegistrationNumber == Faculty.RegistrationNumber).Select(e => e.SpecializationId).FirstOrDefault();
                    Faculty.SpecializationName = Faculty.SpecializationId != null ? jntuh_specialization.Where(e => e.id == Faculty.SpecializationId).Select(e => e.specializationName).FirstOrDefault() : null;
                    Faculty.IdentfiedFor = jntuh_college_faculty_registered.Where(e => e.RegistrationNumber == Faculty.RegistrationNumber).Select(e => e.IdentifiedFor).FirstOrDefault();
                    Faculty.PGSpecialization = jntuh_registered_faculty.Where(e => e.RegistrationNumber.Trim() == Faculty.RegistrationNumber.Trim()).Select(e => e.Jntu_PGSpecializationId).FirstOrDefault();
                    Faculty.PGSpecializationName = Faculty.PGSpecialization != null ? jntuh_specialization.Where(e => e.id == Faculty.PGSpecialization).Select(e => e.specializationName).FirstOrDefault() : null;
                    Faculty.CUpdatedby = jntuh_college_faculty_registered.Where(e => e.RegistrationNumber == Faculty.RegistrationNumber).Select(e => e.updatedBy).FirstOrDefault();
                    Faculty.CUpdatedOn = jntuh_college_faculty_registered.Where(e => e.RegistrationNumber == Faculty.RegistrationNumber).Select(e => e.updatedOn).FirstOrDefault();
                    //Checking Cleared Faculty
                    Faculty.Eid = jntuh_college_faculty_registered_copy.Where(e => e.RegistrationNumber == Faculty.RegistrationNumber).Count() == 1 ? 1 : 0;

                    Faculty.SSC = jntuh_registered_faculty_education.Where(E => E.educationId == 1 && E.facultyId == item.id).Select(E => E.certificate).FirstOrDefault();
                    Faculty.UG = jntuh_registered_faculty_education.Where(E => E.educationId == 3 && E.facultyId == item.id).Select(E => E.certificate).FirstOrDefault();
                    Faculty.PG = jntuh_registered_faculty_education.Where(E => E.educationId == 4 && E.facultyId == item.id).Select(E => E.certificate).FirstOrDefault();
                    Faculty.PHD = jntuh_registered_faculty_education.Where(E => E.educationId == 6 && E.facultyId == item.id).Select(E => E.certificate).FirstOrDefault();
                    Faculty.SCMDocumentView = jntuh_scmupload.Where(e => e.RegistrationNumber == Faculty.RegistrationNumber && e.SpecializationId != 0 && e.DepartmentId != 0).Select(e => e.SCMDocument).FirstOrDefault();
                    Faculty.CollegeId = collegeId;
                    Faculty.CollegeName = jntuh_college.collegeName;
                    teachingFaculty.Add(Faculty);
                }



                #region old Code
                //var jntuhregfaculty = db.jntuh_registered_faculty.Where(rf => rf.DepartmentId == 61).Select(cf => cf.RegistrationNumber).ToArray();
                //var jntuhCollegeFacultyRegistered =
                //    db.jntuh_college_faculty_registered.Where(cf => jntuhregfaculty.Contains(cf.RegistrationNumber) && cf.IdentifiedFor == "UG" && cf.SpecializationId == null)
                //        .Select(cf => cf.RegistrationNumber)
                //        .ToArray();

                //var jntuh_registered_faculty = new List<jntuh_registered_faculty>();
                //jntuh_registered_faculty = db.jntuh_registered_faculty
                //    .Where(rf => jntuhCollegeFacultyRegistered.Contains(rf.RegistrationNumber) && rf.NoSpecialization != true).ToList();
                //int[] REGIDS = jntuh_registered_faculty.Select(F => F.id).ToArray();
                //var FacultyEducation =
                //    db.jntuh_registered_faculty_education.Where(E => REGIDS.Contains(E.facultyId)).ToList();

                //var data = jntuh_registered_faculty.Select(a => new FacultyRegistration
                //{
                //    id = a.id,
                //    Type = a.type,
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
                //    SSC =
                //        FacultyEducation.Where(E => E.educationId == 1 && E.facultyId == a.id)
                //            .Select(E => E.certificate)
                //            .FirstOrDefault(),
                //    UG =
                //        FacultyEducation.Where(E => E.educationId == 3 && E.facultyId == a.id)
                //            .Select(E => E.certificate)
                //            .FirstOrDefault(),
                //    PG =
                //        FacultyEducation.Where(E => E.educationId == 4 && E.facultyId == a.id)
                //            .Select(E => E.certificate)
                //            .FirstOrDefault(),
                //    PHD =
                //        FacultyEducation.Where(E => E.educationId == 5 && E.facultyId == a.id)
                //            .Select(E => E.certificate)
                //            .FirstOrDefault(),
                //    CollegeName = jntuhcollege.Where(i => i.id == a.collegeId).Select(i => i.collegeName).FirstOrDefault(),
                //    CollegeId = a.collegeId

                //}).ToList();

                //teachingFaculty.AddRange(data);

                //var data = jntuh_registered_faculty.Select(a => new FacultyRegistration
                //{
                //    SpecId = 
                //    id = a.id,
                //    Type = a.type,
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


                //    SSC =
                //        jntuh_registered_faculty_education.Where(E => E.educationId == 1 && E.facultyId == a.id)
                //            .Select(E => E.certificate)
                //            .FirstOrDefault(),
                //    UG =
                //        jntuh_registered_faculty_education.Where(E => E.educationId == 3 && E.facultyId == a.id)
                //            .Select(E => E.certificate)
                //            .FirstOrDefault(),
                //    PG =
                //        jntuh_registered_faculty_education.Where(E => E.educationId == 4 && E.facultyId == a.id)
                //            .Select(E => E.certificate)
                //            .FirstOrDefault(),
                //    PHD =
                //        jntuh_registered_faculty_education.Where(E => (E.educationId == 6 || E.educationId ==5 ) && E.facultyId == a.id)
                //            .Select(E => E.certificate)
                //            .FirstOrDefault(),
                //    CollegeName = jntuhcollege.Where(i => i.id == a.collegeId).Select(i => i.collegeName).FirstOrDefault(),
                //    CollegeId = a.collegeId

                //}).ToList();
                #endregion


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
                //ViewBag.count = teachingFaculty.Count;
                //teachingFaculty = teachingFaculty.GetRange(First, totalLabs > 20 ? second : totalLabs);
                #endregion
                return View(teachingFaculty);
            }
            else
            {
                return View();
            }


        }

        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification,DataEntry")]
        [HttpGet]
        public ActionResult PharmacyFacultySpecializationView(int collegeId, string regno)
        {
            var faculty = new CollegeFaculty();
            var existingfaculty = db.jntuh_registered_faculty.FirstOrDefault(i => i.RegistrationNumber == regno);
            if (existingfaculty != null)
            {
                var jntuh_college_faculty_registered = db.jntuh_college_faculty_registered.Where(i => i.RegistrationNumber == regno).Select(e => e).FirstOrDefault();
                faculty.collegeId = collegeId;
                faculty.id = existingfaculty.id;
                faculty.facultyFirstName = existingfaculty.FirstName;
                faculty.facultyLastName = existingfaculty.LastName;
                faculty.facultySurname = existingfaculty.MiddleName;
                faculty.facultyDesignationId = existingfaculty.DesignationId;
                faculty.designation = db.jntuh_designation.Where(d => d.id == faculty.facultyDesignationId).Select(d => d.designation).FirstOrDefault();
                faculty.facultyOtherDesignation = existingfaculty.OtherDesignation;
                if (existingfaculty.DepartmentId != null)
                    faculty.facultyDepartmentId = (int)existingfaculty.DepartmentId;
                faculty.department = db.jntuh_department.Where(d => d.id == faculty.facultyDepartmentId).Select(d => d.departmentName).FirstOrDefault();
                faculty.FacultyRegistrationNumber = regno;

                faculty.facultyRecruitedFor = jntuh_college_faculty_registered.IdentifiedFor;
                faculty.SpecializationId = existingfaculty.Jntu_PGSpecializationId == null ? null : existingfaculty.Jntu_PGSpecializationId;


            }

            // var jntuh_college_intake_existing = db.jntuh_college_intake_existing.Where(e => e.collegeId == collegeId).Select(e => e).ToList();
            var jntuh_specialization = db.jntuh_specialization.Where(s => s.isActive == true).Select(e => e).ToList();
            var jntuh_department = db.jntuh_department.Where(s => s.isActive == true).Select(e => e).ToList();
            var jntuh_degree = db.jntuh_degree.Where(s => s.isActive == true).Select(e => e).ToList();
            var Data = //(from e in jntuh_college_intake_existing
                        (from s in jntuh_specialization
                         join d in jntuh_department on s.departmentId equals d.id
                         join de in jntuh_degree on d.degreeId equals de.id
                         where de.id == 2 || de.id == 9 || de.id == 10
                         select new
                         {
                             id = s.id,
                             spec = s.specializationName
                         }).Distinct().ToList();


            //var pgSpecializations = db.jntuh_college_intake_existing
            //                             .Where(e => e.jntuh_specialization.jntuh_department.jntuh_degree.id != 4 && e.jntuh_specialization.jntuh_department.jntuh_degree.id != 5)
            //                             .Select(e => new { id = e.jntuh_specialization.id, spec = e.jntuh_specialization.specializationName })
            //                             .GroupBy(e => new { e.id, e.spec })
            //                             .OrderBy(e => e.Key.spec)
            //                             .Select(e => new { id = e.Key.id, spec = e.Key.spec }).ToList();
            ViewBag.PGSpecializations = Data;
            return PartialView(faculty);
        }


        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification,DataEntry")]
        [HttpPost]
        public ActionResult PharmacyFacultySpecializationCollege(CollegeFaculty faculty)
        {
            TempData["Error"] = null;
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            var isExistingFaculty = db.jntuh_registered_faculty.Where(r => r.RegistrationNumber == faculty.FacultyRegistrationNumber).Select(r => r).FirstOrDefault();
            //var isExistingFaculty = db.jntuh_college_faculty_registered.FirstOrDefault(r => r.RegistrationNumber == faculty.FacultyRegistrationNumber);
            //if (isExistingFaculty != null)
            //{
            //    // isExistingFaculty.IdentifiedFor = faculty.facultyRecruitedFor;
            //    isExistingFaculty.FacultySpecializationId = faculty.SpecializationId;
            //    isExistingFaculty.updatedBy = userID;
            //    isExistingFaculty.updatedOn = DateTime.Now;
            //    db.Entry(isExistingFaculty).State = EntityState.Modified;
            //    db.SaveChanges();
            //    TempData["Success"] = "Faculty Specialization (" + faculty.FacultyRegistrationNumber + " ) Successfully Updated ..";
            //    TempData["Error"] = null;
            //}
            if (isExistingFaculty != null && faculty.SpecializationId != 0)
            {
                isExistingFaculty.Jntu_PGSpecializationId = faculty.SpecializationId;
                //isExistingFaculty.updatedBy = userID;
                //isExistingFaculty.updatedOn = DateTime.Now;
                db.Entry(isExistingFaculty).State = EntityState.Modified;
                db.SaveChanges();
                TempData["Success"] = "Faculty Specialization (" + faculty.FacultyRegistrationNumber + " ) Successfully Updated ..";
                TempData["Error"] = null;
            }
            else
            {
                TempData["Error"] = "No data found.";
            }
            return RedirectToAction("PharmacyFacultySpecializationScreen", "FacultyVerificationDENew", new { collegeId = faculty.collegeId });
        }

        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification,DataEntry")]
        public ActionResult PharmacyFacultyNOSpecialization(int collegeId, string regno)
        {
            TempData["Error"] = null;
            var isExistingFaculty = db.jntuh_registered_faculty.FirstOrDefault(r => r.RegistrationNumber == regno && r.collegeId == collegeId);
            if (isExistingFaculty != null)
            {
                isExistingFaculty.NoSpecialization = true;
                db.SaveChanges();
                TempData["Success"] = "Specialization Inactivated Successfully for (" + regno + ") ..";
            }
            else
            {
                TempData["Error"] = "Registration number doesn't exist.";
            }
            return RedirectToAction("PharmacyFacultySpecializationScreen", "FacultyVerificationDENew", new { collegeId = collegeId });

        }


        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification,DataEntry")]
        public ActionResult UpdatePharmacyFacultyFlags(string fid, string command)
        {
            int userid = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int fID = 0;
            int collegeid = 0;
            TempData["Success"] = null;
            TempData["Error"] = null;

            if (fid != null)
            {
                fID = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(fid, System.Web.Configuration.WebConfigurationManager.AppSettings["CryptoKey"]));
            }

            //if(TempData["collegeId"] != null)
            //{
            //    collegeid =Convert.ToInt32(TempData["collegeId"]);
            //}

            if (fID != 0)
            {
                if (command == "Approved")
                {
                    var jntuh_registered_faculty = db.jntuh_registered_faculty.Where(s => s.id == fID).Select(e => e).FirstOrDefault();
                    jntuh_registered_faculty facultyData = new jntuh_registered_faculty();
                    if (jntuh_registered_faculty != null)
                    {
                        jntuh_registered_faculty.NotQualifiedAsperAICTE = false;
                        jntuh_registered_faculty.IncompleteCertificates = false;
                        jntuh_registered_faculty.NoRelevantUG = "No";
                        jntuh_registered_faculty.NoRelevantPG = "No";
                        jntuh_registered_faculty.NORelevantPHD = "No";
                        //jntuh_registered_faculty.NoSCM = false;
                        jntuh_registered_faculty.InvalidPANNumber = false;
                        jntuh_registered_faculty.Xeroxcopyofcertificates = false;


                        jntuh_registered_faculty.DeactivatedBy = userid;
                        jntuh_registered_faculty.DeactivatedOn = DateTime.Now;
                        db.Entry(jntuh_registered_faculty).State = EntityState.Modified;
                        db.SaveChanges();
                        TempData["Success"] = jntuh_registered_faculty.RegistrationNumber + " Faculty Flags are Cleared";
                        // return RedirectToAction("PharmacyFacultySpecializationScreen", "FacultyVerificationDENew", new { collegeId = collegeid });
                        return RedirectToAction("PharmacyViewFacultyDetails", "FacultyVerificationDENew", new { fid = UAAAS.Models.Utilities.EncryptString(fID.ToString(), System.Web.Configuration.WebConfigurationManager.AppSettings["CryptoKey"]) });
                    }
                    else
                    {
                        TempData["Error"] = "No Data Found";
                        // return RedirectToAction("PharmacyFacultySpecializationScreen", "FacultyVerificationDENew", new { collegeId = collegeid });
                        return RedirectToAction("PharmacyViewFacultyDetails", "FacultyVerificationDENew", new { fid = UAAAS.Models.Utilities.EncryptString(fID.ToString(), System.Web.Configuration.WebConfigurationManager.AppSettings["CryptoKey"]) });
                    }
                }
                //else if(command=="NotApproved")
                //{
                //    var jntuh_registered_faculty = db.jntuh_registered_faculty.Where(s => s.id == fID).Select(e => e).FirstOrDefault();
                //    jntuh_registered_faculty facultyData = new jntuh_registered_faculty();
                //    if (jntuh_registered_faculty != null)
                //    {
                //        jntuh_registered_faculty.NotQualifiedAsperAICTE = true;
                //        jntuh_registered_faculty.IncompleteCertificates = true;
                //        jntuh_registered_faculty.NoRelevantUG = "Yes";
                //        jntuh_registered_faculty.NoRelevantPG = "Yes";
                //        jntuh_registered_faculty.NORelevantPHD = "Yes";
                //        jntuh_registered_faculty.NoSCM17 = true;
                //        jntuh_registered_faculty.InvalidPANNumber = true;
                //        jntuh_registered_faculty.NotIdentityfiedForanyProgram = true;
                //        jntuh_registered_faculty.PhdUndertakingDocumentstatus = false;
                //        db.Entry(jntuh_registered_faculty).State = EntityState.Modified;
                //        db.SaveChanges();
                //        TempData["Success"] = "Flags are Cleared";
                //        return RedirectToAction("ViewFacultyDetails", "FacultyVerificationDENew", new { fid = UAAAS.Models.Utilities.EncryptString(fID.ToString(), System.Web.Configuration.WebConfigurationManager.AppSettings["CryptoKey"]) });
                //    }
                //    else
                //    {
                //        TempData["Error"] = "No Data Found";
                //        return RedirectToAction("ViewFacultyDetails", "FacultyVerificationDENew", new { fid = UAAAS.Models.Utilities.EncryptString(fID.ToString(), System.Web.Configuration.WebConfigurationManager.AppSettings["CryptoKey"]) });
                //    }
                //}
                else
                {
                    return RedirectToAction("PharmacyViewFacultyDetails", "FacultyVerificationDENew", new { fid = UAAAS.Models.Utilities.EncryptString(fID.ToString(), System.Web.Configuration.WebConfigurationManager.AppSettings["CryptoKey"]) });
                }
            }
            else
            {
                TempData["Error"] = "No Data Found";
                return RedirectToAction("PharmacyViewFacultyDetails", "FacultyVerificationDENew", new { fid = UAAAS.Models.Utilities.EncryptString(fID.ToString(), System.Web.Configuration.WebConfigurationManager.AppSettings["CryptoKey"]) });
            }
        }

        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification,DataEntry")]
        public ActionResult PharmacyFacultyVerificationFlagsEdit(string fid, string collegeid)
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



                regFaculty.PanDeactivationReasion = !string.IsNullOrEmpty(faculty.PanDeactivationReason) ? faculty.PanDeactivationReason : "";
                regFaculty.Absent = faculty.Absent != null ? (bool)faculty.Absent : false;
                regFaculty.BlacklistFaculty = faculty.Blacklistfaculy != null ? (bool)faculty.Blacklistfaculy : false;
                regFaculty.PHDundertakingnotsubmitted = faculty.PHDundertakingnotsubmitted != null ? (bool)faculty.PHDundertakingnotsubmitted : false;
                regFaculty.NOTQualifiedAsPerAICTE = faculty.NotQualifiedAsperAICTE != null ? (bool)faculty.NotQualifiedAsperAICTE : false;
                if (regFaculty.NOTQualifiedAsPerAICTE == false)
                {

                }
                regFaculty.InvalidPANNo = faculty.InvalidPANNumber != null ? (bool)faculty.InvalidPANNumber : false;
                regFaculty.InCompleteCeritificates = faculty.IncompleteCertificates != null ? (bool)faculty.IncompleteCertificates : false;
                regFaculty.OriginalCertificatesnotshownFlag = faculty.OriginalCertificatesNotShown != null ? (bool)faculty.OriginalCertificatesNotShown : false;
                regFaculty.FalsePAN = faculty.FalsePAN != null ? (bool)faculty.FalsePAN : false;
                regFaculty.NOForm16 = faculty.NoForm16 != null ? (bool)faculty.NoForm16 : false;
                //regFaculty.MultipleReginSamecoll = faculty.MultipleRegInSameCollege != null ? (bool)faculty.MultipleRegInSameCollege : false;
                regFaculty.XeroxcopyofcertificatesFlag = faculty.Xeroxcopyofcertificates != null ? (bool)faculty.Xeroxcopyofcertificates : false;
                regFaculty.NotIdentityFiedForAnyProgramFlag = faculty.NotIdentityfiedForanyProgram != null ? (bool)faculty.NotIdentityfiedForanyProgram : false;
                regFaculty.NOrelevantUgFlag = faculty.NoRelevantUG == "No" ? false : true;
                regFaculty.NOrelevantPgFlag = faculty.NoRelevantPG == "No" ? false : true;
                regFaculty.NOrelevantPhdFlag = faculty.NORelevantPHD == "No" ? false : true;
                //regFaculty.NoForm16Verification = faculty.Noform16Verification != null ? (bool)faculty.Noform16Verification : false;

                //regFaculty.NoSCM = faculty.NoSCM != null ? (bool)faculty.NoSCM : false;
                regFaculty.NoSCM17Flag = faculty.NoSCM17 != null ? (bool)faculty.NoSCM17 : false;

                //regFaculty.PhotocopyofPAN = faculty.PhotoCopyofPAN != null ? (bool)faculty.PhotoCopyofPAN : false;
                regFaculty.PhdUndertakingDocumentstatus = faculty.PhdUndertakingDocumentstatus != null ? (bool)(faculty.PhdUndertakingDocumentstatus) : false;
                regFaculty.PHDUndertakingDocumentView = faculty.PHDUndertakingDocument;
                regFaculty.PhdUndertakingDocumentText = faculty.PhdUndertakingDocumentText;
                //regFaculty.AppliedPAN = faculty.AppliedPAN != null ? (bool)(faculty.AppliedPAN) : false;
                //regFaculty.SamePANUsedByMultipleFaculty = faculty.SamePANUsedByMultipleFaculty != null ? (bool)(faculty.SamePANUsedByMultipleFaculty) : false;




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
            return PartialView("_PharmacyFacultyVerificationFlagsEdit", regFaculty);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification,DataEntry")]
        public ActionResult PharmcyFacultyVerificationFlagsPostDENew(FacultyRegistration faculty)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            var facultydetails = db.jntuh_registered_faculty.Where(i => i.RegistrationNumber == faculty.RegistrationNumber).FirstOrDefault();
            if (facultydetails != null)
            {

                facultydetails.NotQualifiedAsperAICTE = faculty.NOTQualifiedAsPerAICTE;
                facultydetails.IncompleteCertificates = faculty.InCompleteCeritificates;
                if (faculty.NOrelevantUgFlag == true)
                    facultydetails.NoRelevantUG = "Yes";
                else if (faculty.NOrelevantUgFlag == false)
                    facultydetails.NoRelevantUG = "No";



                if (faculty.NOrelevantPgFlag == true)
                    facultydetails.NoRelevantPG = "Yes";
                else if (faculty.NOrelevantPgFlag == false)
                    facultydetails.NoRelevantPG = "No";


                if (faculty.NOrelevantPhdFlag == true)
                    facultydetails.NORelevantPHD = "Yes";
                else if (faculty.NOrelevantPhdFlag == false)
                    facultydetails.NORelevantPHD = "No";

                //facultydetails.NoSCM = faculty.NoSCM;

                facultydetails.InvalidPANNumber = faculty.InvalidPANNo;
                facultydetails.Xeroxcopyofcertificates = faculty.XeroxcopyofcertificatesFlag;

                facultydetails.DeactivatedBy = userID;
                facultydetails.DeactivatedOn = DateTime.Now;

                db.Entry(facultydetails).State = EntityState.Modified;
                db.SaveChanges();
                TempData["Success"] = facultydetails.RegistrationNumber + " Faculty Flags are Changed";

            }

            // return RedirectToAction("FacultyVerificationIndex", "FacultyVerificationDENew", new { collegeid = faculty.CollegeId });
            return RedirectToAction("PharmacyViewFacultyDetails", "FacultyVerificationDENew", new { fid = UAAAS.Models.Utilities.EncryptString(faculty.id.ToString(), System.Web.Configuration.WebConfigurationManager.AppSettings["CryptoKey"]) });
        }

        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification,DataEntry")]
        public ActionResult PharmacyViewFacultyDetails(string fid)
        {
            FacultyRegistration regFaculty = new FacultyRegistration();
            int fID = 0;

            if (!string.IsNullOrEmpty(fid))
            {
                regFaculty.GenderId = null;
                regFaculty.isFacultyRatifiedByJNTU = null;
                fID = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(fid, WebConfigurationManager.AppSettings["CryptoKey"]));
                // Above code commented by Naushad Khan Anbd added the below line.
                // fID = Convert.ToInt32(fid);
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
                    regFaculty.DeactivedOn = faculty.DeactivatedOn;
                    regFaculty.Deactivedby = faculty.DeactivatedBy;
                    DateTime verificationstartdate = new DateTime(2018, 03, 21);
                    ViewBag.Isdone = true;
                    if (verificationstartdate > regFaculty.DeactivedOn || regFaculty.DeactivedOn == null)
                    {
                        ViewBag.Isdone = false;
                    }
                    //regFaculty.Deactivedby = faculty.DeactivatedBy;
                    //regFaculty.DeactivedOn = faculty.DeactivatedOn;

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
                    //Commented on 18-06-2018 by Narayana Reddy
                    //int[] verificationOfficers =
                    //    db.jntuh_college_faculty_verified.Where(v => v.FacultyId == facultyId)
                    //        .Select(v => v.VerificationOfficer)
                    //        .Distinct()
                    //        .ToArray();
                    int userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);

                    ViewBag.HideVerifyLink = regFaculty.isApproved != null ? true : false;
                    return View(regFaculty);
                }
                else
                {
                    return RedirectToAction("FacultyVerificationIndex", "FacultyVerificationDENew");
                }
            }
            else
            {
                return RedirectToAction("FacultyVerificationIndex", "FacultyVerificationDENew");
            }
        }

        #endregion


        #region College Faculty With No Departmetn Screenn
        //Commented on 14-06-2018 by Narayana Reddy
        //[Authorize(Roles = "Admin")]
        //public ActionResult CollegeFacultyNoDepartmentScreen()
        //{
        //    var teachingFaculty = new List<FacultyRegistration>();
        //    var jntuhcollege = db.jntuh_college.AsNoTracking().ToList();
        //    var jntuhcollegeFacultyRegisterd = db.jntuh_college_faculty_registered.ToList();
        //    var jntuhCollegeFacultyNoDepartment =
        //        db.jntuh_college_nodepartments.Where(r => r.DepartmentId == null && r.IsActive == false)
        //            .Select(r => r).ToList();



        //    string[] strRegNoS = jntuhCollegeFacultyNoDepartment.Select(cf => cf.RegistrationNumber).Distinct().ToArray();
        //    int Collegeid, Collegeid1 = 0;
        //    var jntuh_registered_faculty = new List<jntuh_registered_faculty>();



        //    jntuh_registered_faculty = db.jntuh_registered_faculty.AsNoTracking().Where(rf => strRegNoS.Contains(rf.RegistrationNumber) && rf.DepartmentId == null && rf.isActive == true).Distinct().ToList();





        //    int[] REGIDS = jntuh_registered_faculty.Select(F => F.id).ToArray();
        //    var FacultyEducation =
        //        db.jntuh_registered_faculty_education.Where(E => REGIDS.Contains(E.facultyId)).ToList();
        //    //var CollegeFaculty = db.jntuh_college_faculty_registered.Select(C => new { C.collegeId, C.RegistrationNumber }).ToList();
        //    //  Collegeid1 = jntuhCollegeFacultyRegistered.Where(C => C.RegistrationNumber.Trim() == "7791-150426-124035").Select(C => C.collegeId).FirstOrDefault();
        //    var data = jntuh_registered_faculty.Select(a => new FacultyRegistration
        //    {
        //        id = a.id,
        //        // Type = a.type,
        //        RegistrationNumber = a.RegistrationNumber,
        //        UniqueID = a.UniqueID,
        //        FirstName = a.FirstName,
        //        MiddleName = a.MiddleName,
        //        LastName = a.LastName,
        //        //   GenderId = a.GenderId,
        //        Email = a.Email,
        //        facultyPhoto = a.Photo,
        //        // Mobile = a.Mobile,
        //        // PANNumber = a.PANNumber,
        //        //  AadhaarNumber = a.AadhaarNumber,
        //        //SSC =
        //        //    FacultyEducation.Where(E => E.educationId == 1 && E.facultyId == a.id)
        //        //        .Select(E => E.certificate)
        //        //        .FirstOrDefault(),
        //        UG =
        //            FacultyEducation.Where(E => E.educationId == 3 && E.facultyId == a.id)
        //                .Select(E => E.specialization)
        //                .FirstOrDefault(),
        //        PG =
        //            FacultyEducation.Where(E => E.educationId == 4 && E.facultyId == a.id)
        //                .Select(E => E.specialization)
        //                .FirstOrDefault(),
        //        PHD =
        //            FacultyEducation.Where(E => E.educationId == 5 && E.facultyId == a.id)
        //                .Select(E => E.specialization)
        //                .FirstOrDefault(),
        //        CollegeId = Collegeid = jntuhcollegeFacultyRegisterd.Where(C => C.RegistrationNumber.Trim() == a.RegistrationNumber).Select(C => C.collegeId).FirstOrDefault(),
        //        CollegeName = jntuhcollege.Where(i => i.id == Collegeid).Select(i => i.collegeName).FirstOrDefault(),



        //    }).ToList();

        //    teachingFaculty.AddRange(data);

        //    return View(teachingFaculty);
        //}
        [Authorize(Roles = "Admin")]
        public ActionResult FacultyDepartmentPartialView(int fid, int collegeId, string regno)
        {
            var faculty = new CollegeFaculty();
            //  var existingfaculty = db.jntuh_registered_faculty.FirstOrDefault(i => i.RegistrationNumber.Trim() == regno.Trim()); //&& i.collegeId == collegeId
            var existingfaculty = db.jntuh_registered_faculty.FirstOrDefault(i => i.id == fid);
            if (existingfaculty != null)
            {
                faculty.collegeId = collegeId;
                faculty.id = existingfaculty.id;
                faculty.facultyFirstName = existingfaculty.FirstName;
                faculty.facultyLastName = existingfaculty.LastName;
                faculty.facultySurname = existingfaculty.MiddleName;
                faculty.facultyDesignationId = existingfaculty.DesignationId;
                faculty.designation = db.jntuh_designation.Where(d => d.id == faculty.facultyDesignationId).Select(d => d.designation).FirstOrDefault();
                faculty.facultyOtherDesignation = existingfaculty.OtherDesignation;
                //if (existingfaculty.DepartmentId != null)
                //    faculty.facultyDepartmentId = (int)existingfaculty.DepartmentId;
                //faculty.department = db.jntuh_department.Where(d => d.id == faculty.facultyDepartmentId).Select(d => d.departmentName).FirstOrDefault();
                faculty.FacultyRegistrationNumber = regno;
                faculty.facultyRecruitedFor =
                    db.jntuh_college_faculty_registered.Where(i => i.RegistrationNumber == regno)
                        .Select(i => i.IdentifiedFor)
                        .FirstOrDefault();
            }

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

            return PartialView(faculty);
        }
        //Commented on 14-06-2018 by Narayana Reddy
        //[Authorize(Roles = "Admin")]
        //[HttpPost]
        //public ActionResult FacultyDepartmentCollege(CollegeFaculty faculty)
        //{
        //    int userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
        //    TempData["Error"] = null;
        //    var jntuh_registered_faculty = db.jntuh_registered_faculty.AsNoTracking().ToList();
        //    var jntuh_college_nodepartments = db.jntuh_college_nodepartments.AsNoTracking().ToList();

        //    var isExistingFaculty = jntuh_registered_faculty.FirstOrDefault(r => r.RegistrationNumber == faculty.FacultyRegistrationNumber);
        //    if (isExistingFaculty != null)
        //    {
        //        jntuh_registered_faculty jntuhRegistered = db.jntuh_registered_faculty.Find(isExistingFaculty.id);
        //        jntuhRegistered.DepartmentId = faculty.facultyDepartmentId;
        //        db.Entry(jntuhRegistered).State = EntityState.Modified;
        //        db.SaveChanges();

        //        int id = jntuh_college_nodepartments.Where(i => i.RegistrationNumber == isExistingFaculty.RegistrationNumber).Select(i => i.Id).FirstOrDefault();

        //        if (id != 0)
        //        {
        //            jntuh_college_nodepartments jntuhCollegeNodepartments = db.jntuh_college_nodepartments.Find(id);
        //            jntuhCollegeNodepartments.DepartmentId = faculty.facultyDepartmentId;
        //            jntuhCollegeNodepartments.IsActive = true;
        //            jntuhCollegeNodepartments.UpdatedOn = DateTime.Now;
        //            jntuhCollegeNodepartments.UpdatedBy = userId;
        //            db.Entry(jntuhCollegeNodepartments).State = EntityState.Modified;
        //            db.SaveChanges();
        //        }

        //        TempData["Success"] = "Faculty Specialization (" + faculty.FacultyRegistrationNumber + " ) Successfully Updated ..";
        //        TempData["Error"] = null;

        //    }
        //    return RedirectToAction("CollegeFacultyNoDepartmentScreen", "FacultyVerificationDENew");
        //}

        #endregion


        #region PharmacyFaculty Specilizations adding

        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification")]
        public ActionResult PharmacyCollegeFacultySpecializationScreen(int? collegeid)
        {
            ViewBag.Colleges = db.jntuh_college.Where(c => c.isActive == true).Select(c => new { collegeId = c.id, collegeName = c.collegeCode + "-" + c.collegeName }).OrderBy(c => c.collegeName).ToList();
            ViewBag.collegeid = collegeid;
            #region new
            var teachingFaculty = new List<FacultyRegistration>();


            var jntuhcollege = db.jntuh_college.AsNoTracking().ToList();
            var jntuhCollegeFacultyRegistered =
                db.jntuh_college_faculty_registered.Where(
                    cf => cf.collegeId == collegeid).Select(cf => cf).ToList();
            string[] strRegNoS = jntuhCollegeFacultyRegistered.Select(cf => cf.RegistrationNumber).ToArray();
            int Collegeid, Collegeid1 = 0;
            var jntuh_registered_faculty = new List<jntuh_registered_faculty>();
            jntuh_registered_faculty = db.jntuh_registered_faculty.Where(rf => strRegNoS.Contains(rf.RegistrationNumber)).ToList();

            var REGIDS = jntuh_registered_faculty.Select(F => F.id).ToArray();
            var FacultyEducation =
                db.jntuh_registered_faculty_education.Where(E => REGIDS.Contains(E.facultyId)).ToList();

            var jntuh_education_category = db.jntuh_education_category.ToList();

            var jntuh_registered_faculty1 = jntuh_registered_faculty.Where(rf => rf.DepartmentId != null && ((rf.Absent == false || rf.Absent == null) && (rf.NotQualifiedAsperAICTE == false || rf.NotQualifiedAsperAICTE == null)
                                                          ) && (rf.PANNumber != null))
                                                 .Select(rf => new
                                                 {
                                                     id = rf.id,
                                                     type = rf.type,
                                                     FirstName = rf.FirstName,
                                                     MiddleName = rf.MiddleName,
                                                     LastName = rf.LastName,
                                                     GenderId = rf.GenderId,
                                                     Email = rf.Email,
                                                     facultyPhoto = rf.Photo,
                                                     Mobile = rf.Mobile,
                                                     PANNumber = rf.PANNumber,
                                                     RegistrationNumber = rf.RegistrationNumber,
                                                     Department = rf.jntuh_department.departmentName,
                                                     HighestDegreeID = rf.jntuh_registered_faculty_education.Count() != 0 ? rf.jntuh_registered_faculty_education.Select(e => e.educationId).Max() : 0,
                                                     IsApproved = rf.isApproved,
                                                     PanNumber = rf.PANNumber,
                                                     AadhaarNumber = rf.AadhaarNumber,
                                                     jntuh_registered_faculty_education = rf.jntuh_registered_faculty_education,

                                                 }).ToList();
            jntuh_registered_faculty1 = jntuh_registered_faculty1.Where(e => e.HighestDegreeID >= 4).ToList();
            var jntuh_registered = jntuh_registered_faculty1.Select(rf => new
            {
                id = rf.id,
                type = rf.type,
                FirstName = rf.FirstName,
                MiddleName = rf.MiddleName,
                LastName = rf.LastName,
                GenderId = rf.GenderId,
                Email = rf.Email,
                facultyPhoto = rf.facultyPhoto,
                Mobile = rf.Mobile,
                PANNumber = rf.PANNumber,
                RegistrationNumber = rf.RegistrationNumber,
                Department = rf.Department,
                HighestDegree = jntuh_education_category.Where(c => c.id == rf.HighestDegreeID).Select(c => c.educationCategoryName).FirstOrDefault(),
                Recruitedfor = jntuhCollegeFacultyRegistered.Where(c => c.RegistrationNumber == rf.RegistrationNumber).Select(c => c.IdentifiedFor).FirstOrDefault(),
                SpecializationId = jntuhCollegeFacultyRegistered.Where(c => c.RegistrationNumber == rf.RegistrationNumber).Select(c => c.SpecializationId).FirstOrDefault(),
                PanNumber = rf.PanNumber,
                AadhaarNumber = rf.AadhaarNumber,
                registered_faculty_specialization = rf.jntuh_registered_faculty_education.Where(e => e.jntuh_education_category.educationCategoryName == "PG").FirstOrDefault().specialization != null ? rf.jntuh_registered_faculty_education.Where(e => e.jntuh_education_category.educationCategoryName == "PG").Select(i => i.specialization).FirstOrDefault().ToUpper().Trim() : "",
                jntuh_registered_faculty_education = rf.jntuh_registered_faculty_education,
            }).Where(e => e.Department != null).ToList();

            var specs = jntuh_registered.Select(i => i.registered_faculty_specialization).Distinct().ToArray();

            var withoutspecs =
                jntuh_registered.Where(i => i.registered_faculty_specialization != "Pharmaceutics".ToUpper() && i.registered_faculty_specialization != "Industrial Pharmacy".ToUpper() &&
                               i.registered_faculty_specialization != "Pharmacy BioTechnology".ToUpper() && i.registered_faculty_specialization != "Pharmaceutical Chemistry".ToUpper() &&
                               i.registered_faculty_specialization != "Pharmacy Analysis".ToUpper() && i.registered_faculty_specialization != "PAQA".ToUpper() &&
                                i.registered_faculty_specialization != "Pharmacology".ToUpper() && i.registered_faculty_specialization != "Pharma D".ToUpper() &&
                                i.registered_faculty_specialization != "Pharmacognosy".ToUpper() && i.registered_faculty_specialization != "English".ToUpper() &&
                                i.registered_faculty_specialization != "Mathematics".ToUpper() && i.registered_faculty_specialization != "Computers".ToUpper() &&
                                i.registered_faculty_specialization != "Zoology".ToUpper() &&
                                i.registered_faculty_specialization != "Pharmacy Practice".ToUpper() &&
                                i.registered_faculty_specialization != "Pharm D".ToUpper() &&
                                i.registered_faculty_specialization != "PharmD".ToUpper() &&
                                i.registered_faculty_specialization != "Pharm.D".ToUpper() &&
                                i.registered_faculty_specialization != "PA & QA".ToUpper() &&
                                i.registered_faculty_specialization != "Quality Assurance".ToUpper() &&
                                i.registered_faculty_specialization != "Hospital & ClinicalPharmacy".ToUpper() &&
                                i.registered_faculty_specialization != "Hospital  Clinical Pharmacy".ToUpper() &&
                                !i.registered_faculty_specialization.Contains("Hospital".ToUpper()) &&
                                !i.registered_faculty_specialization.Contains("QAPRA".ToUpper())).ToList();

            var data = withoutspecs.Select(a => new FacultyRegistration
            {
                id = a.id,
                Type = a.type,
                RegistrationNumber = a.RegistrationNumber,
                FirstName = a.FirstName,
                MiddleName = a.MiddleName,
                LastName = a.LastName,
                GenderId = a.GenderId,
                Email = a.Email,
                facultyPhoto = a.facultyPhoto,
                Mobile = a.Mobile,
                PANNumber = a.PANNumber,
                AadhaarNumber = a.AadhaarNumber,
                SSC =
                    FacultyEducation.Where(E => E.educationId == 1 && E.facultyId == a.id)
                        .Select(E => E.certificate)
                        .FirstOrDefault(),
                UG =
                    FacultyEducation.Where(E => E.educationId == 3 && E.facultyId == a.id)
                        .Select(E => E.certificate)
                        .FirstOrDefault(),
                PG =
                    FacultyEducation.Where(E => E.educationId == 4 && E.facultyId == a.id)
                        .Select(E => E.certificate)
                        .FirstOrDefault(),
                PHD =
                    FacultyEducation.Where(E => E.educationId == 5 && E.facultyId == a.id)
                        .Select(E => E.certificate)
                        .FirstOrDefault(),
                CollegeId = Collegeid = jntuhCollegeFacultyRegistered.Where(C => C.RegistrationNumber.Trim() == a.RegistrationNumber).Select(C => C.collegeId).FirstOrDefault(),
                CollegeName = jntuhcollege.Where(i => i.id == Collegeid).Select(i => i.collegeName).FirstOrDefault(),
                jntuh_registered_faculty_education = a.jntuh_registered_faculty_education,
                updatedOn = a.jntuh_registered_faculty_education.Where(i => i.educationId == 4).Select(i => i.updatedOn).FirstOrDefault()

            }).ToList();

            teachingFaculty.AddRange(data);
            #endregion

            return View(teachingFaculty);
        }

        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification,DataEntry")]
        [HttpGet]
        public ActionResult Pharmacyfacultyview(int collegeId, string regno)
        {
            var faculty = new CollegeFaculty();
            var existingfaculty = db.jntuh_registered_faculty.FirstOrDefault(i => i.RegistrationNumber.Trim() == regno.Trim()); //&& i.collegeId == collegeId
            if (existingfaculty != null)
            {
                faculty.collegeId = collegeId;
                faculty.id = existingfaculty.id;
                faculty.facultyFirstName = existingfaculty.FirstName;
                faculty.facultyLastName = existingfaculty.LastName;
                faculty.facultySurname = existingfaculty.MiddleName;
                faculty.facultyDesignationId = existingfaculty.DesignationId;
                faculty.designation = db.jntuh_designation.Where(d => d.id == faculty.facultyDesignationId).Select(d => d.designation).FirstOrDefault();
                faculty.facultyOtherDesignation = existingfaculty.OtherDesignation;
                if (existingfaculty.DepartmentId != null)
                    faculty.facultyDepartmentId = (int)existingfaculty.DepartmentId;
                faculty.department = db.jntuh_department.Where(d => d.id == faculty.facultyDepartmentId).Select(d => d.departmentName).FirstOrDefault();
                faculty.FacultyRegistrationNumber = regno;
                faculty.facultyRecruitedFor =
                    db.jntuh_college_faculty_registered.Where(i => i.RegistrationNumber == regno)
                        .Select(i => i.IdentifiedFor)
                        .FirstOrDefault();
            }

            var pgSpecializations = db.jntuh_college_intake_existing
                                         .Where(e => e.jntuh_specialization.jntuh_department.jntuh_degree.id != 4 && e.jntuh_specialization.jntuh_department.jntuh_degree.id != 5)
                                         .Select(e => new { id = e.jntuh_specialization.id, spec = e.jntuh_specialization.specializationName })
                                         .GroupBy(e => new { e.id, e.spec })
                                         .OrderBy(e => e.Key.spec)
                                         .Select(e => new { id = e.Key.id, spec = e.Key.spec }).ToList();
            pgSpecializations.Add(new { id = 100, spec = "English" });
            pgSpecializations.Add(new { id = 100, spec = "Mathematics" });
            pgSpecializations.Add(new { id = 100, spec = "Computers" });
            ViewBag.PGSpecializations = pgSpecializations;
            return PartialView(faculty);
        }


        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification,DataEntry")]
        [HttpPost]
        public ActionResult FacultySpecializationPharmacyCollege(CollegeFaculty faculty)
        {
            TempData["Error"] = null;
            var userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            var isRegisteredFaculty = db.jntuh_registered_faculty.Where(r => r.RegistrationNumber == faculty.FacultyRegistrationNumber).Select(r => r).FirstOrDefault();
            var faculty_education = isRegisteredFaculty.jntuh_registered_faculty_education.FirstOrDefault(e => e.jntuh_education_category.educationCategoryName == "PG");

            var education = db.jntuh_registered_faculty_education.FirstOrDefault(i => i.facultyId == isRegisteredFaculty.id && i.educationId == 4);
            // var isExistingFaculty = db.jntuh_college_faculty_registered.FirstOrDefault(r => r.RegistrationNumber == faculty.FacultyRegistrationNumber);
            if (education != null)
            {
                education.specialization = faculty.SpecializationName;
                education.updatedBy = userID;
                education.updatedOn = DateTime.Now;
                db.Entry(education).State = EntityState.Modified;
                db.SaveChanges();
                TempData["Success"] = "Faculty Specialization (" + faculty.FacultyRegistrationNumber + " ) Successfully Updated ..";
                TempData["Error"] = null;
            }

            return RedirectToAction("PharmacyCollegeFacultySpecializationScreen", "FacultyVerificationDENew", new { collegeid = faculty.collegeId });
        }
        #endregion

        #region FacultyNoSCM's View

        //Commented on 14-06-2018 by Narayana Reddy
        //[Authorize(Roles = "Admin,SuperAdmin,FacultyVerification")]
        //public ActionResult FacultyFlagsReactivated(int? collegeid)
        //{
        //    var collegeids = new[] { 301, 376, 442, 230, 249, 300, 443, 81, 129, 254, 380, 403 };
        //    ViewBag.Colleges = db.jntuh_college.Where(c => collegeids.Contains(c.id)).Select(c => new { collegeId = c.id, collegeName = c.collegeCode + "-" + c.collegeName }).OrderBy(c => c.collegeName).ToList();
        //    ViewBag.collegeid = collegeid;
        //    var jntuh_department = db.jntuh_department.ToList();
        //    List<FacultyRegistration> teachingFaculty = new List<FacultyRegistration>();

        //    if (collegeid != null)
        //    {
        //        var jntuh_college_faculty_registered = db.jntuh_college_faculty_registered.Where(cf => cf.collegeId == collegeid).Select(cf => cf).ToList();
        //        var strRegNoS = db.jntuh_college_faculty_registered.Where(cf => cf.collegeId == collegeid).Select(cf => cf.RegistrationNumber.Trim()).ToArray();

        //        var jntuhfaculty = db.jntuh_registered_faculty
        //                                     .Where(rf => strRegNoS.Contains(rf.RegistrationNumber.Trim()) && rf.Notin116 != true)  //&& (rf.collegeId == null || rf.collegeId == collegeid)
        //                                     .ToList();

        //        var jntuh_registered_faculty = new List<jntuh_registered_faculty_noscm>();
        //        jntuh_registered_faculty = db.jntuh_registered_faculty_noscm
        //                                     .Where(rf => strRegNoS.Contains(rf.RegistrationNumber.Trim()) && rf.Notin116 != true)  //&& (rf.collegeId == null || rf.collegeId == collegeid)
        //                                     .ToList();

        //        var jntuh_notin415facultys = db.jntuh_notin415faculty.Where(F => F.CollegeId == collegeid).ToList();
        //        var Specializations = db.jntuh_specialization.ToList();
        //        string[] strREG = jntuh_notin415facultys.Select(F => F.RegistrationNumber.Trim()).ToArray();
        //        string RegNumber = "";
        //        int? Specializationid = 0;
        //        foreach (var a in jntuh_registered_faculty)
        //        {
        //            var Reason = string.Empty;
        //            Specializationid = jntuh_college_faculty_registered.Where(C => C.RegistrationNumber.Trim() == a.RegistrationNumber.Trim()).Select(C => C.SpecializationId).FirstOrDefault();
        //            var faculty = new FacultyRegistration();
        //            var jntuhRegisteredFaculty = jntuhfaculty.FirstOrDefault(i => i.RegistrationNumber.Trim() == a.RegistrationNumber.Trim());
        //            if (jntuhRegisteredFaculty != null)
        //            {
        //                var facultyeducation = jntuhRegisteredFaculty.jntuh_registered_faculty_education;
        //                faculty.id = a.id;
        //                faculty.Type = a.type;
        //                faculty.CollegeId = collegeid;
        //                faculty.RegistrationNumber = a.RegistrationNumber;
        //                faculty.UniqueID = a.UniqueID;
        //                faculty.FirstName = a.FirstName;
        //                faculty.MiddleName = a.MiddleName;
        //                faculty.LastName = a.LastName;
        //                faculty.GenderId = a.GenderId;
        //                faculty.Email = a.Email;
        //                faculty.facultyPhoto = a.Photo;
        //                faculty.Mobile = a.Mobile;
        //                faculty.PANNumber = a.PANNumber;
        //                faculty.AadhaarNumber = a.AadhaarNumber;
        //                faculty.isActive = a.isActive;
        //                faculty.isApproved = a.isApproved;
        //                faculty.department = jntuh_department.Where(d => d.id == a.DepartmentId).Select(d => d.departmentName).FirstOrDefault();
        //                faculty.SamePANNumberCount = jntuh_registered_faculty.Where(f => f.PANNumber == a.PANNumber).ToList().Count;
        //                faculty.SameAadhaarNumberCount = jntuh_registered_faculty.Where(f => f.AadhaarNumber == a.AadhaarNumber).ToList().Count;
        //                faculty.SpecializationIdentfiedFor = Specializationid > 0 ? Specializations.Where(S => S.id == Specializationid).Select(S => S.specializationName).FirstOrDefault() : "";
        //                faculty.isVerified = isFacultyVerified(a.id);
        //                faculty.DeactivationReason = a.DeactivationReason;
        //                faculty.FacultyVerificationStatus = a.FacultyVerificationStatus;
        //                faculty.updatedOn = a.updatedOn;
        //                faculty.createdOn = jntuh_college_faculty_registered.Where(f => f.RegistrationNumber == a.RegistrationNumber).Select(f => f.createdOn).FirstOrDefault();
        //                faculty.IdentfiedFor = jntuh_college_faculty_registered.Where(f => f.RegistrationNumber.Trim() == a.RegistrationNumber.Trim()).Select(f => f.IdentifiedFor).FirstOrDefault();
        //                faculty.jntuh_registered_faculty_education = facultyeducation;
        //                faculty.DegreeId = facultyeducation.Count(e => e.facultyId == a.id) > 0 ? facultyeducation.Where(e => e.facultyId == a.id).Select(e => e.educationId).Max() : 0;
        //            }
        //            faculty.PanDeactivationReasion = !string.IsNullOrEmpty(a.PanDeactivationReason) ? a.PanDeactivationReason : "";
        //            faculty.Absent = a.Absent != null ? (bool)a.Absent : false;
        //            faculty.BlacklistFaculty = a.Blacklistfaculy != null ? (bool)a.Blacklistfaculy : false;
        //            faculty.PHDundertakingnotsubmitted = a.PHDundertakingnotsubmitted != null ? (bool)a.PHDundertakingnotsubmitted : false;
        //            faculty.NOTQualifiedAsPerAICTE = a.NotQualifiedAsperAICTE != null ? (bool)a.NotQualifiedAsperAICTE : false;
        //            faculty.InvalidPANNo = a.InvalidPANNumber != null ? (bool)a.InvalidPANNumber : false; ;
        //            faculty.InCompleteCeritificates = a.IncompleteCertificates != null ? (bool)a.IncompleteCertificates : false; ;
        //            faculty.FalsePAN = a.FalsePAN != null ? (bool)a.FalsePAN : false;
        //            if (faculty.Absent == true)
        //            {
        //                Reason = "Absent" + ",";
        //            }


        //            if (faculty.NOTQualifiedAsPerAICTE == true)
        //            {
        //                Reason += "Not Qualified as AICTE" + ",";
        //            }
        //            if (faculty.InCompleteCeritificates == true)
        //            {
        //                Reason += "Incomplete Certificates(UG/PG/PHD/SCM)" + ",";
        //            }

        //            if (strREG.Contains(a.RegistrationNumber.Trim()))
        //            {
        //                // faculty.SelectionCommitteeProcedings = string.IsNullOrEmpty(a.ProceedingDocument) ? "No" : "";
        //                faculty.NoSCM = a.NoSCM == null ? false : (bool)a.NoSCM;
        //            }

        //            if (Reason != "")
        //            {
        //                Reason = Reason.Substring(0, Reason.Length - 1);
        //            }

        //            faculty.DeactivationNew = Reason;
        //            teachingFaculty.Add(faculty);
        //        }

        //        teachingFaculty = teachingFaculty.Where(m => m.isActive == true).OrderBy(f => f.updatedOn).ThenBy(f => f.department).ToList();

        //        return View(teachingFaculty);
        //    }

        //    return View(teachingFaculty);
        //}


        #endregion

        #region For Pharmacy Faculty Deficiency DataEntry

        //Code Written By Siva
        [Authorize(Roles = "Admin,SuperAdmin,DataEntry,FacultyVerification")]
        public ActionResult PharmacyFacultyGroupingbasedonColleges(int? collegeId, int? DegreeId, int? GroupId, int? TotalRequiredFaculty, int? SpecializationWiseFaculty)
        {
            int userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);

            var CollegeIds = new int[] { 448, 180, 159, 376, 428, 445, 24, 117, 202, 213, 395, 364, 27, 30, 6, 34, 55, 58, 54, 52, 9, 60, 65, 78, 150, 204, 219, 90, 97, 104, 110, 139, 105, 107, 114, 379, 118, 39, 120, 389, 127, 237, 135, 136, 47, 252, 169, 253, 262, 442, 301, 436, 370, 263, 267, 206, 140, 44, 290, 454, 42, 95, 297, 384, 75, 348, 298, 295, 303, 302, 283, 332, 410, 392, 234, 146, 315, 320, 317, 314, 318, 319, 353, 313, 235, 375 };
            var jntuhcollege = db.jntuh_college.AsNoTracking().Where(e => CollegeIds.Contains(e.id)).ToList();

            ViewBag.PharmacyCollegeList = jntuhcollege.Select(e => new
            {
                collegeId = e.id,
                collegeName = e.collegeCode + "-" + e.collegeName
            }).ToList();

            if (collegeId == null)
                ViewBag.checkcollegeId = null;
            else
                ViewBag.checkcollegeId = collegeId;

            ViewBag.AfterCollegeSelect = null;

            string StringCollegeId = collegeId == null ? "0" : collegeId.ToString();

            List<SelectListItem> DegreesFirst = new List<SelectListItem>();
            DegreesFirst.Add(new SelectListItem { Text = "---Select---", Value = "1" });
            ViewBag.Degrees = DegreesFirst;

            List<SelectListItem> GroupsFirst = new List<SelectListItem>();
            GroupsFirst.Add(new SelectListItem { Text = "---Select---", Value = "1" });
            ViewBag.Groups = GroupsFirst;

            TempData["ViewCollegeId"] = null;

            string Group1 = "1";
            string Group2 = "2";
            string Group3 = "3";
            string Group4 = "4";

            var jntuh_academic_year = db.jntuh_academic_year.ToList();
            int presentYear = jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int AY1 = jntuh_academic_year.Where(a => a.actualYear == (presentYear + 1)).Select(a => a.id).FirstOrDefault();
            int AY2 = jntuh_academic_year.Where(a => a.actualYear == (presentYear)).Select(a => a.id).FirstOrDefault();
            int AY3 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 1)).Select(a => a.id).FirstOrDefault();
            int AY4 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 2)).Select(a => a.id).FirstOrDefault();
            int AY5 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 3)).Select(a => a.id).FirstOrDefault();
            int AY6 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 4)).Select(a => a.id).FirstOrDefault();

            var jntuh_appeal_pharmacy_data = db.jntuh_appeal_pharmacydata.Where(e => e.CollegeCode == StringCollegeId).Select(e => e).ToList();

            ViewBag.Group1Required = jntuh_appeal_pharmacy_data.Where(e => e.PharmacySpecialization == Group1).Select(e => e.SpecializationWiseRequiredFaculty).FirstOrDefault() == null ? 0 : jntuh_appeal_pharmacy_data.Where(e => e.PharmacySpecialization == Group1).Select(e => e.SpecializationWiseRequiredFaculty).FirstOrDefault();
            ViewBag.Group2Required = jntuh_appeal_pharmacy_data.Where(e => e.PharmacySpecialization == Group2).Select(e => e.SpecializationWiseRequiredFaculty).FirstOrDefault() == null ? 0 : jntuh_appeal_pharmacy_data.Where(e => e.PharmacySpecialization == Group2).Select(e => e.SpecializationWiseRequiredFaculty).FirstOrDefault(); ;
            ViewBag.Group3Required = jntuh_appeal_pharmacy_data.Where(e => e.PharmacySpecialization == Group3).Select(e => e.SpecializationWiseRequiredFaculty).FirstOrDefault() == null ? 0 : jntuh_appeal_pharmacy_data.Where(e => e.PharmacySpecialization == Group3).Select(e => e.SpecializationWiseRequiredFaculty).FirstOrDefault(); ;
            ViewBag.Group4Required = jntuh_appeal_pharmacy_data.Where(e => e.PharmacySpecialization == Group4).Select(e => e.SpecializationWiseRequiredFaculty).FirstOrDefault() == null ? 0 : jntuh_appeal_pharmacy_data.Where(e => e.PharmacySpecialization == Group4).Select(e => e.SpecializationWiseRequiredFaculty).FirstOrDefault(); ;

            var Group1Available = jntuh_appeal_pharmacy_data.Where(e => e.PharmacySpecialization == Group1).Select(e => e).ToList();
            var Group2Available = jntuh_appeal_pharmacy_data.Where(e => e.PharmacySpecialization == Group2).Select(e => e).ToList();
            var Group3Available = jntuh_appeal_pharmacy_data.Where(e => e.PharmacySpecialization == Group3).Select(e => e).ToList();
            var Group4Available = jntuh_appeal_pharmacy_data.Where(e => e.PharmacySpecialization == Group4).Select(e => e).ToList();

            ViewBag.Group1Available = Group1Available.Where(e => e.Deficiency != null).Select(e => e.Deficiency).Count();
            ViewBag.Group2Available = Group2Available.Where(e => e.Deficiency != null).Select(e => e.Deficiency).Count();
            ViewBag.Group3Available = Group3Available.Where(e => e.Deficiency != null).Select(e => e.Deficiency).Count();
            ViewBag.Group4Available = Group4Available.Where(e => e.Deficiency != null).Select(e => e.Deficiency).Count();


            if (collegeId != null && GroupId == null)
            {
                TempData["ViewCollegeId"] = collegeId;
                string CollegeidNew1 = collegeId.ToString();

                var jntuh_specialization = db.jntuh_specialization.Where(e => e.isActive == true).ToList();

                var jntuh_college_intake_existing = db.jntuh_college_intake_existing.Where(e => e.collegeId == collegeId).Select(e => e).ToList();
                var jntuh_college_intake_existing_Data = jntuh_college_intake_existing.Where(e => e.academicYearId == AY1 && e.proposedIntake != 0 && e.courseStatus != "Closure").Select(w => w).ToList();
                var SpecializationIds = jntuh_college_intake_existing_Data.Select(e => e.specializationId).Distinct().ToArray();
                var Degrees = (from e in jntuh_college_intake_existing_Data
                               join s in db.jntuh_specialization on e.specializationId equals s.id
                               join d in db.jntuh_department on s.departmentId equals d.id
                               join de in db.jntuh_degree on d.degreeId equals de.id
                               where SpecializationIds.Contains(s.id) && (de.id == 2 || de.id == 5 || de.id == 9 || de.id == 10)
                               select new
                               {
                                   DegreeId = de.id,
                                   Degreename = de.degree
                               }).Distinct().ToList();

                ViewBag.Degrees = Degrees;

                List<SelectListItem> Groups = new List<SelectListItem>();
                Groups.Add(new SelectListItem { Text = "---Select---", Value = "1" });
                ViewBag.Groups = Groups;

                //BpharmacyIntake
                int? BpharmacyProposedintake = jntuh_college_intake_existing.Where(e => e.academicYearId == AY1 && e.specializationId == 12).Select(e => e.proposedIntake).FirstOrDefault();
                int? Bpharmacyintake1 = jntuh_college_intake_existing.Where(e => e.academicYearId == AY2 && e.specializationId == 12).Select(e => e.approvedIntake).FirstOrDefault();
                int? Bpharmacyintake2 = jntuh_college_intake_existing.Where(e => e.academicYearId == AY3 && e.specializationId == 12).Select(e => e.approvedIntake).FirstOrDefault();
                int? Bpharmacyintake3 = jntuh_college_intake_existing.Where(e => e.academicYearId == AY4 && e.specializationId == 12).Select(e => e.approvedIntake).FirstOrDefault();
                int? Bpharmacyintake4 = jntuh_college_intake_existing.Where(e => e.academicYearId == AY5 && e.specializationId == 12).Select(e => e.approvedIntake).FirstOrDefault();
                int? Bpharmacyintake5 = jntuh_college_intake_existing.Where(e => e.academicYearId == AY6 && e.specializationId == 12).Select(e => e.approvedIntake).FirstOrDefault();

                ViewBag.BpharmacyProposedInatke = BpharmacyProposedintake;
                ViewBag.BpharmcySecondInatke = (Bpharmacyintake1);
                ViewBag.BpharmcythirdInatke = (Bpharmacyintake2);
                ViewBag.BpharmcyfouthInatke = (Bpharmacyintake3);
                ViewBag.BpharmcyfifthInatke = (Bpharmacyintake4);
                ViewBag.BpharmcySixthInatke = (Bpharmacyintake5);

                //Pharm D Intake
                int? PharmDProposedintake = jntuh_college_intake_existing.Where(e => e.academicYearId == AY1 && e.specializationId == 18).Select(e => e.proposedIntake).FirstOrDefault();
                int? PharmDintake1 = jntuh_college_intake_existing.Where(e => e.academicYearId == AY2 && e.specializationId == 18).Select(e => e.approvedIntake).FirstOrDefault();
                int? PharmDintake2 = jntuh_college_intake_existing.Where(e => e.academicYearId == AY3 && e.specializationId == 18).Select(e => e.approvedIntake).FirstOrDefault();
                int? PharmDintake3 = jntuh_college_intake_existing.Where(e => e.academicYearId == AY4 && e.specializationId == 18).Select(e => e.approvedIntake).FirstOrDefault();
                int? PharmDintake4 = jntuh_college_intake_existing.Where(e => e.academicYearId == AY5 && e.specializationId == 18).Select(e => e.approvedIntake).FirstOrDefault();
                int? PharmDintake5 = jntuh_college_intake_existing.Where(e => e.academicYearId == AY6 && e.specializationId == 18).Select(e => e.approvedIntake).FirstOrDefault();

                ViewBag.PharmDProposedInatke = PharmDProposedintake;
                ViewBag.PharmDySecondInatke = (PharmDintake1);
                ViewBag.PharmDthirdInatke = (PharmDintake2);
                ViewBag.PharmDfouthInatke = (PharmDintake3);
                ViewBag.PharmDfifthInatke = (PharmDintake4);
                ViewBag.PharmDSixthInatke = (PharmDintake5);

                //Pharm D.PB Intake
                int? PharmDPBProposedintake = jntuh_college_intake_existing.Where(e => e.academicYearId == AY1 && e.specializationId == 19).Select(e => e.proposedIntake).FirstOrDefault();
                int? PharmDPBintake1 = jntuh_college_intake_existing.Where(e => e.academicYearId == AY2 && e.specializationId == 19).Select(e => e.approvedIntake).FirstOrDefault();
                int? PharmDPBintake2 = jntuh_college_intake_existing.Where(e => e.academicYearId == AY3 && e.specializationId == 19).Select(e => e.approvedIntake).FirstOrDefault();
                int? PharmDPBintake3 = jntuh_college_intake_existing.Where(e => e.academicYearId == AY4 && e.specializationId == 19).Select(e => e.approvedIntake).FirstOrDefault();
                int? PharmDPBintake4 = jntuh_college_intake_existing.Where(e => e.academicYearId == AY5 && e.specializationId == 19).Select(e => e.approvedIntake).FirstOrDefault();
                int? PharmDPBintake5 = jntuh_college_intake_existing.Where(e => e.academicYearId == AY6 && e.specializationId == 19).Select(e => e.approvedIntake).FirstOrDefault();

                ViewBag.PharmDPBProposedInatke = PharmDPBProposedintake;
                ViewBag.PharmDPBySecondInatke = (PharmDPBintake1);
                ViewBag.PharmDPBthirdInatke = (PharmDPBintake2);
                ViewBag.PharmDPBfouthInatke = (PharmDPBintake3);
                ViewBag.PharmDPBfifthInatke = (PharmDPBintake4);
                ViewBag.PharmDPBSixthInatke = (PharmDPBintake5);

                var jntuh_college = jntuhcollege.Where(e => e.id == collegeId).Select(e => e).FirstOrDefault();
                var teachingFaculty = new List<FacultyRegistration>();
                var jntuh_college_faculty_registered = db.jntuh_college_faculty_registered.Where(e => e.collegeId == collegeId && (e.DepartmentId == 26 || e.DepartmentId == 36 || e.DepartmentId == 27 || e.DepartmentId == 39 || e.DepartmentId == 61)).Select(e => e).ToList();
                var jntuh_college_faculty_registered_new = jntuh_college_faculty_registered;
                var jntuh_college_prinicipal_registered = db.jntuh_college_principal_registered.Where(c => c.collegeId == collegeId).Select(e => e.RegistrationNumber).FirstOrDefault();

                ViewBag.Prinicipal = jntuh_college_prinicipal_registered;

                var strRegnos = jntuh_college_faculty_registered.Select(e => e.RegistrationNumber).ToList();

                var jntuh_registered_faculty_New = db.jntuh_registered_faculty.Where(e => strRegnos.Contains(e.RegistrationNumber.Trim())).Select(e => e).ToList();

                var jntuh_registered_faculty = jntuh_registered_faculty_New.Where(rf => rf.type != "Adjunct" && ((rf.Absent == false || rf.Absent == null) && (rf.OriginalCertificatesNotShown == false || rf.OriginalCertificatesNotShown == null) && (rf.Xeroxcopyofcertificates == false || rf.Xeroxcopyofcertificates == null) && (rf.NotQualifiedAsperAICTE == false || rf.NotQualifiedAsperAICTE == null)
                                                       && (rf.NoSCM == false || rf.NoSCM == null) && (rf.PANNumber != null) && (rf.IncompleteCertificates == false || rf.IncompleteCertificates == null) && (rf.Blacklistfaculy == false) && rf.NoRelevantUG != "Yes" && rf.NoRelevantPG != "Yes" && rf.NORelevantPHD != "Yes"
                                                       && (rf.InvalidPANNumber == false || rf.InvalidPANNumber == null) && (rf.OriginalsVerifiedPHD != true) && (rf.OriginalsVerifiedUG != true) && (rf.Noclass == false || rf.Noclass == null) && (rf.FakePHD == false || rf.FakePHD == null) && (rf.Genuinenessnotsubmitted == false || rf.Genuinenessnotsubmitted == null) && (rf.NoPGspecialization == false || rf.NoPGspecialization == null) && (rf.Invaliddegree == false || rf.Invaliddegree == null) && rf.BAS != "Yes" && (rf.AbsentforVerification == false || rf.AbsentforVerification == null)) && rf.InvalidAadhaar != "Yes").Select(rf => rf).ToList();

                string CollegeidNew = collegeId.ToString();
                var jntuh_appeal_pharmacydata = db.jntuh_appeal_pharmacydata.Where(e => e.CollegeCode == CollegeidNew).Select(e => e).ToList();
                var RegNumbers = jntuh_appeal_pharmacydata.Select(e => e.Deficiency).ToList();

                jntuh_college_faculty_registered_new = jntuh_college_faculty_registered_new.Where(e => !RegNumbers.Contains(e.RegistrationNumber)).Select(e => e).ToList();

                jntuh_registered_faculty = jntuh_registered_faculty.Where(e => !RegNumbers.Contains(e.RegistrationNumber)).Select(e => e).ToList();

                //var Group1RemainingRegnos = jntuh_college_faculty_registered_new.Where(e => e.FacultySpecializationId == 115 || e.FacultySpecializationId == 119 || e.FacultySpecializationId == 172 || e.FacultySpecializationId == 120).Select(e => e.RegistrationNumber).ToList();
                //ViewBag.Group1RemainingFaculty = jntuh_registered_faculty.Where(e => Group1RemainingRegnos.Contains(e.RegistrationNumber)).Select(e => e.RegistrationNumber).Count();

                //var Group2RemainingRegnos = jntuh_college_faculty_registered_new.Where(e => e.FacultySpecializationId == 116 || e.FacultySpecializationId == 167 || e.FacultySpecializationId == 123 || e.FacultySpecializationId == 170 || e.FacultySpecializationId == 117).Select(e => e.RegistrationNumber).ToList();
                //ViewBag.Group2RemainingFaculty = jntuh_registered_faculty.Where(e => Group2RemainingRegnos.Contains(e.RegistrationNumber)).Select(e => e.RegistrationNumber).Count();

                //var Group3RemainingRegnos = jntuh_college_faculty_registered_new.Where(e => e.FacultySpecializationId == 169 || e.FacultySpecializationId == 18 || e.FacultySpecializationId == 19 || e.FacultySpecializationId == 114 || e.FacultySpecializationId == 122).Select(e => e.RegistrationNumber).ToList();
                //ViewBag.Group3RemainingFaculty = jntuh_registered_faculty.Where(e => Group3RemainingRegnos.Contains(e.RegistrationNumber)).Select(e => e.RegistrationNumber).Count();

                //var Group4RemainingRegnos = jntuh_college_faculty_registered_new.Where(e => e.FacultySpecializationId == 172 || e.FacultySpecializationId == 117 || e.FacultySpecializationId == 121).Select(e => e.RegistrationNumber).ToList();
                //ViewBag.Group4RemainingFaculty = jntuh_registered_faculty.Where(e => Group4RemainingRegnos.Contains(e.RegistrationNumber)).Select(e => e.RegistrationNumber).Count();

                var Group1RemainingRegnos = jntuh_registered_faculty.Where(e => e.Jntu_PGSpecializationId == 115 || e.Jntu_PGSpecializationId == 119 || e.Jntu_PGSpecializationId == 172 || e.Jntu_PGSpecializationId == 120).Select(e => e.RegistrationNumber).ToList();
                ViewBag.Group1RemainingFaculty = jntuh_registered_faculty.Where(e => Group1RemainingRegnos.Contains(e.RegistrationNumber)).Select(e => e.RegistrationNumber).Count();

                var Group2RemainingRegnos = jntuh_registered_faculty.Where(e => e.Jntu_PGSpecializationId == 116 || e.Jntu_PGSpecializationId == 167 || e.Jntu_PGSpecializationId == 123 || e.Jntu_PGSpecializationId == 170 || e.Jntu_PGSpecializationId == 117).Select(e => e.RegistrationNumber).ToList();
                ViewBag.Group2RemainingFaculty = jntuh_registered_faculty.Where(e => Group2RemainingRegnos.Contains(e.RegistrationNumber)).Select(e => e.RegistrationNumber).Count();

                var Group3RemainingRegnos = jntuh_registered_faculty.Where(e => e.Jntu_PGSpecializationId == 169 || e.Jntu_PGSpecializationId == 18 || e.Jntu_PGSpecializationId == 19 || e.Jntu_PGSpecializationId == 114 || e.Jntu_PGSpecializationId == 122).Select(e => e.RegistrationNumber).ToList();
                ViewBag.Group3RemainingFaculty = jntuh_registered_faculty.Where(e => Group3RemainingRegnos.Contains(e.RegistrationNumber)).Select(e => e.RegistrationNumber).Count();

                var Group4RemainingRegnos = jntuh_registered_faculty.Where(e => e.Jntu_PGSpecializationId == 172 || e.Jntu_PGSpecializationId == 117 || e.Jntu_PGSpecializationId == 121).Select(e => e.RegistrationNumber).ToList();
                ViewBag.Group4RemainingFaculty = jntuh_registered_faculty.Where(e => Group4RemainingRegnos.Contains(e.RegistrationNumber)).Select(e => e.RegistrationNumber).Count();



                var FacultyIds = jntuh_registered_faculty.Select(e => e.id).ToList();

                var jntuh_registered_faculty_education = db.jntuh_registered_faculty_education.Where(e => FacultyIds.Contains(e.facultyId)).Select(e => e).ToList();

                List<FacultyRegistration> data = new List<FacultyRegistration>();
                foreach (var item in jntuh_registered_faculty)
                {
                    string Reason = null;
                    FacultyRegistration Faculty = new FacultyRegistration();
                    Faculty.id = item.id;
                    Faculty.Type = item.type;
                    Faculty.RegistrationNumber = item.RegistrationNumber;
                    Faculty.UniqueID = item.UniqueID;
                    Faculty.FirstName = item.FirstName;
                    Faculty.MiddleName = item.MiddleName;
                    Faculty.LastName = item.LastName;
                    Faculty.GenderId = item.GenderId;
                    Faculty.Email = item.Email;
                    Faculty.facultyPhoto = item.Photo;
                    Faculty.Mobile = item.Mobile;
                    Faculty.PANNumber = item.PANNumber;
                    Faculty.AadhaarNumber = item.AadhaarNumber;
                    Faculty.DegreeId = jntuh_registered_faculty_education.Count(e => e.facultyId == item.id) > 0 ? item.jntuh_registered_faculty_education.Where(e => e.facultyId == item.id).Select(e => e.educationId).Max() : 0;
                    Faculty.PANNumber = item.PANNumber;

                    Faculty.Absent = item.Absent != null ? (bool)item.Absent : false;
                    Faculty.XeroxcopyofcertificatesFlag = item.Xeroxcopyofcertificates != null ? (bool)item.Xeroxcopyofcertificates : false;
                    Faculty.NOrelevantUgFlag = item.NoRelevantUG == "Yes" ? true : false;
                    Faculty.NOrelevantPgFlag = item.NoRelevantPG == "Yes" ? true : false;
                    Faculty.NOrelevantPhdFlag = item.NORelevantPHD == "Yes" ? true : false;
                    Faculty.NOTQualifiedAsPerAICTE = item.NotQualifiedAsperAICTE != null ? (bool)item.NotQualifiedAsperAICTE : false;
                    Faculty.InvalidPANNo = item.InvalidPANNumber != null ? (bool)item.InvalidPANNumber : false;
                    Faculty.InCompleteCeritificates = item.IncompleteCertificates == true ? true : false;
                    Faculty.NoSCM = item.NoSCM != null ? (bool)item.NoSCM : false;
                    Faculty.OriginalCertificatesnotshownFlag = item.OriginalCertificatesNotShown != null ? (bool)item.OriginalCertificatesNotShown : false;
                    Faculty.NotIdentityFiedForAnyProgramFlag = item.NotIdentityfiedForanyProgram != null ? (bool)item.NotIdentityfiedForanyProgram : false;
                    Faculty.Basstatus = item.InvalidAadhaar;
                    Faculty.BasstatusOld = item.BAS;
                    Faculty.OriginalsVerifiedUG = item.OriginalsVerifiedUG == true ? true : false;
                    Faculty.OriginalsVerifiedPHD = item.OriginalsVerifiedPHD == true ? true : false;
                    Faculty.BlacklistFaculty = item.Blacklistfaculy == true ? true : false;
                    Faculty.VerificationStatus = item.AbsentforVerification == true ? true : false;

                    //New Flags 
                    Faculty.InvalidDegree = item.Invaliddegree == true ? true : false;
                    Faculty.NoClass = item.Noclass == true ? true : false;
                    Faculty.GenuinenessnotSubmitted = item.Genuinenessnotsubmitted == true ? true : false;
                    Faculty.FakePhd = item.FakePHD == true ? true : false;
                    Faculty.NoPgSpecialization = item.NoPGspecialization == true ? true : false;
                    Faculty.NotconsiderPhd = item.NotconsideredPHD == true ? true : false;

                    Faculty.NoSCM17Flag = item.NoSCM17 != null ? (bool)item.NoSCM17 : false;
                    Faculty.PhdUndertakingDocumentstatus = item.PhdUndertakingDocumentstatus != null ? (bool)(item.PhdUndertakingDocumentstatus) : false;
                    Faculty.PHDUndertakingDocumentView = item.PHDUndertakingDocument;
                    Faculty.PhdUndertakingDocumentText = item.PhdUndertakingDocumentText;

                    Faculty.Deactivedby = item.DeactivatedBy;
                    Faculty.DeactivedOn = item.DeactivatedOn;

                    Faculty.DegreeId = jntuh_registered_faculty_education.Count(E => E.facultyId == item.id) > 0 ? jntuh_registered_faculty_education.Where(E => E.facultyId == item.id).Select(E => E.educationId).Max() : 0;

                    if (Faculty.Absent == true)
                        Reason += "Absent";

                    if (Faculty.Type == "Adjunct")
                    {
                        if (Reason != null)
                            Reason += ",Adjunct Faculty";
                        else
                            Reason += "Adjunct Faculty";
                    }

                    if (Faculty.XeroxcopyofcertificatesFlag == true)
                    {
                        if (Reason != null)
                            Reason += ",Xerox copyof certificates";
                        else
                            Reason += "Xerox copyof certificates";
                    }

                    if (Faculty.NOrelevantUgFlag == true)
                    {
                        if (Reason != null)
                            Reason += ",NO Relevant UG";
                        else
                            Reason += "NO Relevant UG";
                    }

                    if (Faculty.NOrelevantPgFlag == true)
                    {
                        if (Reason != null)
                            Reason += ",NO Relevant PG";
                        else
                            Reason += "NO Relevant PG";
                    }

                    if (Faculty.NOrelevantPhdFlag == true)
                    {
                        if (Reason != null)
                            Reason += ",NO Relevant PHD";
                        else
                            Reason += "NO Relevant PHD";
                    }

                    if (Faculty.NOTQualifiedAsPerAICTE == true)
                    {
                        if (Reason != null)
                            Reason += ",NOT Qualified AsPerAICTE";
                        else
                            Reason += "NOT Qualified AsPerAICTE";
                    }

                    if (Faculty.InvalidPANNo == true)
                    {
                        if (Reason != null)
                            Reason += ",InvalidPANNumber";
                        else
                            Reason += "InvalidPANNumber";
                    }

                    if (Faculty.InCompleteCeritificates == true)
                    {
                        if (Reason != null)
                            Reason += ",InComplete Ceritificates";
                        else
                            Reason += "InComplete Ceritificates";
                    }

                    if (Faculty.NoSCM == true)
                    {
                        if (Reason != null)
                            Reason += ",NoSCM";
                        else
                            Reason += "NoSCM";
                    }

                    if (Faculty.OriginalCertificatesnotshownFlag == true)
                    {
                        if (Reason != null)
                            Reason += ",Original Certificates notshown";
                        else
                            Reason += "Original Certificates notshown";
                    }

                    if (Faculty.PANNumber == null)
                    {
                        if (Reason != null)
                            Reason += ",No PANNumber";
                        else
                            Reason += "No PANNumber";
                    }

                    if (Faculty.NotIdentityFiedForAnyProgramFlag == true)
                    {
                        if (Reason != null)
                            Reason += ",NotIdentityFied ForAnyProgram";
                        else
                            Reason += "NotIdentityFied ForAnyProgram";
                    }

                    if (Faculty.SamePANUsedByMultipleFaculty == true)
                    {
                        if (Reason != null)
                            Reason += ",SamePANUsedByMultipleFaculty";
                        else
                            Reason += "SamePANUsedByMultipleFaculty";
                    }

                    if (Faculty.MultipleReginSamecoll == true)
                    {
                        if (Reason != null)
                            Reason += ",No Class in UG/PG";
                        else
                            Reason += "No Class in UG/PG";
                    }

                    if (Faculty.Basstatus == "Yes")
                    {
                        if (Reason != null)
                            Reason += ",No/Invalid Aadhaar Document";
                        else
                            Reason += "No/Invalid Aadhaar Document";
                    }

                    if (Faculty.BasstatusOld == "Yes")
                    {
                        if (Reason != null)
                            Reason += ",BAS Flag";
                        else
                            Reason += "BAS Flag";
                    }

                    if (Faculty.OriginalsVerifiedUG == true)
                    {
                        if (Reason != null)
                            Reason += ",Complaint PHD Faculty";
                        else
                            Reason += "Complaint PHD Faculty";
                    }

                    if (Faculty.OriginalsVerifiedPHD == true)
                    {
                        if (Reason != null)
                            Reason += ",No Guide Sign in PHD Thesis";
                        else
                            Reason += "No Guide Sign in PHD Thesis";
                    }

                    if (Faculty.BlacklistFaculty == true)
                    {
                        if (Reason != null)
                            Reason += ",BlackList";
                        else
                            Reason += "BlackList";
                    }

                    if (Faculty.VerificationStatus == true)
                    {
                        if (Reason != null)
                            Reason += ",Not Attend For Physical Verification";
                        else
                            Reason += "Not Attend For Physical Verification";
                    }

                    Faculty.DeactivationReason = Reason == null ? null : Reason;

                    Faculty.SpecializationId = jntuh_college_faculty_registered.Where(e => e.RegistrationNumber == Faculty.RegistrationNumber).Select(e => e.SpecializationId).FirstOrDefault();
                    Faculty.SpecializationName = Faculty.SpecializationId != null ? jntuh_specialization.Where(e => e.id == Faculty.SpecializationId).Select(e => e.specializationName).FirstOrDefault() : null;
                    Faculty.IdentfiedFor = jntuh_college_faculty_registered.Where(e => e.RegistrationNumber == Faculty.RegistrationNumber).Select(e => e.IdentifiedFor).FirstOrDefault();
                    Faculty.PGSpecialization = item.Jntu_PGSpecializationId;
                    Faculty.PGSpecializationName = Faculty.PGSpecialization != null ? jntuh_specialization.Where(e => e.id == Faculty.PGSpecialization).Select(e => e.specializationName).FirstOrDefault() : null;

                    Faculty.PHD = jntuh_registered_faculty_education.Where(E => E.educationId == 6 && E.facultyId == item.id).Select(E => E.courseStudied).FirstOrDefault();
                    Faculty.CollegeId = collegeId;
                    Faculty.CollegeName = jntuh_college.collegeName;
                    Faculty.isVerified = false;

                    teachingFaculty.Add(Faculty);

                }
                return View(teachingFaculty.Where(e => e.DeactivationReason == null).ToList());
            }
            else if (collegeId != null && DegreeId != null && GroupId != null)
            {
                TempData["ViewCollegeId"] = collegeId;
                string CollegeidNew1 = collegeId.ToString();

                int?[] Group_Specializationids = new int?[] { };
                if (GroupId == 1)
                {
                    Group_Specializationids = new int?[] { 115, 119, 172, 120 };
                }
                else if (GroupId == 2)
                {
                    Group_Specializationids = new int?[] { 116, 167, 123, 170, 117, 124 };
                }
                else if (GroupId == 3)
                {
                    Group_Specializationids = new int?[] { 18, 19, 114, 169, 122 };
                }
                else if (GroupId == 4)
                {
                    Group_Specializationids = new int?[] { 172, 117, 121 };
                }

                var jntuh_specialization = db.jntuh_specialization.Where(e => e.isActive == true).ToList();

                var jntuh_college_intake_existing = db.jntuh_college_intake_existing.Where(e => e.collegeId == collegeId).Select(e => e).ToList();
                var jntuh_college_intake_existing_Data = jntuh_college_intake_existing.Where(e => e.academicYearId == AY1 && e.proposedIntake != 0 && e.courseStatus != "Closure").Select(w => w).ToList();
                var SpecializationIds = jntuh_college_intake_existing_Data.Select(e => e.specializationId).Distinct().ToArray();
                var Degrees = (from e in jntuh_college_intake_existing_Data
                               join s in db.jntuh_specialization on e.specializationId equals s.id
                               join d in db.jntuh_department on s.departmentId equals d.id
                               join de in db.jntuh_degree on d.degreeId equals de.id
                               where SpecializationIds.Contains(s.id) && (de.id == 2 || de.id == 5 || de.id == 9 || de.id == 10)
                               select new
                               {
                                   DegreeId = de.id,
                                   Degreename = de.degree
                               }).Distinct().ToList();

                ViewBag.Degrees = Degrees;

                //BpharmacyIntake
                int? BpharmacyProposedintake = jntuh_college_intake_existing.Where(e => e.academicYearId == AY1 && e.specializationId == 12).Select(e => e.proposedIntake).FirstOrDefault();
                int? Bpharmacyintake1 = jntuh_college_intake_existing.Where(e => e.academicYearId == AY2 && e.specializationId == 12).Select(e => e.approvedIntake).FirstOrDefault();
                int? Bpharmacyintake2 = jntuh_college_intake_existing.Where(e => e.academicYearId == AY3 && e.specializationId == 12).Select(e => e.approvedIntake).FirstOrDefault();
                int? Bpharmacyintake3 = jntuh_college_intake_existing.Where(e => e.academicYearId == AY4 && e.specializationId == 12).Select(e => e.approvedIntake).FirstOrDefault();
                int? Bpharmacyintake4 = jntuh_college_intake_existing.Where(e => e.academicYearId == AY5 && e.specializationId == 12).Select(e => e.approvedIntake).FirstOrDefault();
                int? Bpharmacyintake5 = jntuh_college_intake_existing.Where(e => e.academicYearId == AY6 && e.specializationId == 12).Select(e => e.approvedIntake).FirstOrDefault();

                ViewBag.BpharmacyProposedInatke = BpharmacyProposedintake;
                ViewBag.BpharmcySecondInatke = (Bpharmacyintake1);
                ViewBag.BpharmcythirdInatke = (Bpharmacyintake2);
                ViewBag.BpharmcyfouthInatke = (Bpharmacyintake3);
                ViewBag.BpharmcyfifthInatke = (Bpharmacyintake4);
                ViewBag.BpharmcySixthInatke = (Bpharmacyintake5);

                //Pharm D Intake
                int? PharmDProposedintake = jntuh_college_intake_existing.Where(e => e.academicYearId == AY1 && e.specializationId == 18).Select(e => e.proposedIntake).FirstOrDefault();
                int? PharmDintake1 = jntuh_college_intake_existing.Where(e => e.academicYearId == AY2 && e.specializationId == 18).Select(e => e.approvedIntake).FirstOrDefault();
                int? PharmDintake2 = jntuh_college_intake_existing.Where(e => e.academicYearId == AY3 && e.specializationId == 18).Select(e => e.approvedIntake).FirstOrDefault();
                int? PharmDintake3 = jntuh_college_intake_existing.Where(e => e.academicYearId == AY4 && e.specializationId == 18).Select(e => e.approvedIntake).FirstOrDefault();
                int? PharmDintake4 = jntuh_college_intake_existing.Where(e => e.academicYearId == AY5 && e.specializationId == 18).Select(e => e.approvedIntake).FirstOrDefault();
                int? PharmDintake5 = jntuh_college_intake_existing.Where(e => e.academicYearId == AY6 && e.specializationId == 18).Select(e => e.approvedIntake).FirstOrDefault();

                ViewBag.PharmDProposedInatke = PharmDProposedintake;
                ViewBag.PharmDySecondInatke = (PharmDintake1);
                ViewBag.PharmDthirdInatke = (PharmDintake2);
                ViewBag.PharmDfouthInatke = (PharmDintake3);
                ViewBag.PharmDfifthInatke = (PharmDintake4);
                ViewBag.PharmDSixthInatke = (PharmDintake5);

                //Pharm D.PB Intake
                int? PharmDPBProposedintake = jntuh_college_intake_existing.Where(e => e.academicYearId == AY1 && e.specializationId == 19).Select(e => e.proposedIntake).FirstOrDefault();
                int? PharmDPBintake1 = jntuh_college_intake_existing.Where(e => e.academicYearId == AY2 && e.specializationId == 19).Select(e => e.approvedIntake).FirstOrDefault();
                int? PharmDPBintake2 = jntuh_college_intake_existing.Where(e => e.academicYearId == AY3 && e.specializationId == 19).Select(e => e.approvedIntake).FirstOrDefault();
                int? PharmDPBintake3 = jntuh_college_intake_existing.Where(e => e.academicYearId == AY4 && e.specializationId == 19).Select(e => e.approvedIntake).FirstOrDefault();
                int? PharmDPBintake4 = jntuh_college_intake_existing.Where(e => e.academicYearId == AY5 && e.specializationId == 19).Select(e => e.approvedIntake).FirstOrDefault();
                int? PharmDPBintake5 = jntuh_college_intake_existing.Where(e => e.academicYearId == AY6 && e.specializationId == 19).Select(e => e.approvedIntake).FirstOrDefault();

                ViewBag.PharmDPBProposedInatke = PharmDPBProposedintake;
                ViewBag.PharmDPBySecondInatke = (PharmDPBintake1);
                ViewBag.PharmDPBthirdInatke = (PharmDPBintake2);
                ViewBag.PharmDPBfouthInatke = (PharmDPBintake3);
                ViewBag.PharmDPBfifthInatke = (PharmDPBintake4);
                ViewBag.PharmDPBSixthInatke = (PharmDPBintake5);


                var jntuh_college = jntuhcollege.Where(e => e.id == collegeId).Select(e => e).FirstOrDefault();
                var teachingFaculty = new List<FacultyRegistration>();
                var jntuh_college_faculty_registered = db.jntuh_college_faculty_registered.Where(e => e.collegeId == collegeId && (e.DepartmentId == 26 || e.DepartmentId == 36 || e.DepartmentId == 27 || e.DepartmentId == 39 || e.DepartmentId == 61)).Select(e => e).ToList();
                var jntuh_college_faculty_registered_new = jntuh_college_faculty_registered;
                var jntuh_college_prinicipal_registered = db.jntuh_college_principal_registered.Where(c => c.collegeId == collegeId).Select(e => e.RegistrationNumber).FirstOrDefault();

                ViewBag.Prinicipal = jntuh_college_prinicipal_registered;
                if (DegreeId == 5)
                {

                    //jntuh_college_faculty_registered = jntuh_college_faculty_registered.Where(e => Group_Specializationids.Contains(e.FacultySpecializationId)).Select(e => e).ToList();

                    List<SelectListItem> Groups = new List<SelectListItem>();
                    Groups.Add(new SelectListItem { Text = "Group1", Value = "1" });
                    Groups.Add(new SelectListItem { Text = "Group2", Value = "2" });
                    Groups.Add(new SelectListItem { Text = "Group3", Value = "3" });
                    Groups.Add(new SelectListItem { Text = "Group4", Value = "4" });
                    ViewBag.Groups = Groups;
                    if (TotalRequiredFaculty != null)
                        TempData["RequiredFaculty"] = TotalRequiredFaculty;
                    else
                        TempData["RequiredFaculty"] = null;

                    // if (SpecializationWiseFaculty != null)
                    TempData["SpecializationFaculty"] = null;
                    //else
                    //    TempData["SpecializationFaculty"] = null;
                }
                else
                {
                    //jntuh_college_faculty_registered = jntuh_college_faculty_registered.Select(e => e).ToList();

                    var jntuh_college_intake_existing_new = db.jntuh_college_intake_existing.Where(e => e.collegeId == collegeId && e.academicYearId == AY1 && e.proposedIntake != 0 && e.courseStatus != "Closure").Select(e => e).ToList();
                    int[] SpecializationIdsNew = jntuh_college_intake_existing_new.Select(e => e.specializationId).Distinct().ToArray();
                    var jntuh_degree = db.jntuh_degree.Where(e => e.isActive == true).ToList();
                    var jntuh_department = db.jntuh_department.Where(e => e.isActive == true).ToList();
                    var jntuh_specialization_New = db.jntuh_specialization.Where(e => e.isActive == true).ToList();

                    var Spec = (from e in jntuh_college_intake_existing_new
                                join s in jntuh_specialization on e.specializationId equals s.id
                                join d in jntuh_department on s.departmentId equals d.id
                                join de in jntuh_degree on d.degreeId equals de.id
                                where SpecializationIdsNew.Contains(s.id) && de.id == DegreeId
                                select new
                                {
                                    GroupId = s.id,
                                    GroupName = s.specializationName
                                }).Distinct().ToList();
                    ViewBag.Groups = Spec;

                    TempData["RequiredFaculty"] = 2;

                    TempData["SpecializationFaculty"] = 2;

                    ViewBag.AfterCollegeSelect = GroupId;
                }

                var strRegnos = jntuh_college_faculty_registered.Select(e => e.RegistrationNumber).ToList();

                var jntuh_registered_faculty_New = db.jntuh_registered_faculty.Where(e => strRegnos.Contains(e.RegistrationNumber.Trim())).Select(e => e).ToList();

                var jntuh_registered_faculty = jntuh_registered_faculty_New.Where(rf => rf.type != "Adjunct" && ((rf.Absent == false || rf.Absent == null) && (rf.OriginalCertificatesNotShown == false || rf.OriginalCertificatesNotShown == null) && (rf.Xeroxcopyofcertificates == false || rf.Xeroxcopyofcertificates == null) && (rf.NotQualifiedAsperAICTE == false || rf.NotQualifiedAsperAICTE == null)
                                                       && (rf.NoSCM == false || rf.NoSCM == null) && (rf.PANNumber != null) && (rf.IncompleteCertificates == false || rf.IncompleteCertificates == null) && (rf.Blacklistfaculy == false) && rf.NoRelevantUG != "Yes" && rf.NoRelevantPG != "Yes" && rf.NORelevantPHD != "Yes"
                                                       && (rf.InvalidPANNumber == false || rf.InvalidPANNumber == null) && (rf.OriginalsVerifiedPHD != true) && (rf.OriginalsVerifiedUG != true) && (rf.Noclass == false || rf.Noclass == null) && (rf.FakePHD == false || rf.FakePHD == null) && (rf.Genuinenessnotsubmitted == false || rf.Genuinenessnotsubmitted == null) && (rf.NoPGspecialization == false || rf.NoPGspecialization == null) && (rf.Invaliddegree == false || rf.Invaliddegree == null) && rf.BAS != "Yes" && (rf.AbsentforVerification == false || rf.AbsentforVerification == null)) && rf.InvalidAadhaar != "Yes").Select(rf => rf).ToList();

                string CollegeidNew = collegeId.ToString();
                var jntuh_appeal_pharmacydata = db.jntuh_appeal_pharmacydata.Where(e => e.CollegeCode == CollegeidNew).Select(e => e).ToList();
                var RegNumbers = jntuh_appeal_pharmacydata.Select(e => e.Deficiency).ToList();

                jntuh_college_faculty_registered_new = jntuh_college_faculty_registered_new.Where(e => !RegNumbers.Contains(e.RegistrationNumber)).Select(e => e).ToList();

                if (DegreeId == 5)
                {
                    jntuh_registered_faculty = jntuh_registered_faculty.Where(q => Group_Specializationids.Contains(q.Jntu_PGSpecializationId)).Select(a => a).ToList();
                }
                jntuh_registered_faculty = jntuh_registered_faculty.Where(e => !RegNumbers.Contains(e.RegistrationNumber)).Select(e => e).ToList();

                //var Group1RemainingRegnos = jntuh_college_faculty_registered_new.Where(e => e.FacultySpecializationId == 115 || e.FacultySpecializationId == 119 || e.FacultySpecializationId == 172 || e.FacultySpecializationId == 120).Select(e => e.RegistrationNumber).ToList();
                //ViewBag.Group1RemainingFaculty = jntuh_registered_faculty.Where(e => Group1RemainingRegnos.Contains(e.RegistrationNumber)).Select(e => e.RegistrationNumber).Count();

                //var Group2RemainingRegnos = jntuh_college_faculty_registered_new.Where(e => e.FacultySpecializationId == 116 || e.FacultySpecializationId == 167 || e.FacultySpecializationId == 123 || e.FacultySpecializationId == 170 || e.FacultySpecializationId == 117).Select(e => e.RegistrationNumber).ToList();
                //ViewBag.Group2RemainingFaculty = jntuh_registered_faculty.Where(e => Group2RemainingRegnos.Contains(e.RegistrationNumber)).Select(e => e.RegistrationNumber).Count();

                //var Group3RemainingRegnos = jntuh_college_faculty_registered_new.Where(e => e.FacultySpecializationId == 169 || e.FacultySpecializationId == 18 || e.FacultySpecializationId == 19 || e.FacultySpecializationId == 114 || e.FacultySpecializationId == 122).Select(e => e.RegistrationNumber).ToList();
                //ViewBag.Group3RemainingFaculty = jntuh_registered_faculty.Where(e => Group3RemainingRegnos.Contains(e.RegistrationNumber)).Select(e => e.RegistrationNumber).Count();

                //var Group4RemainingRegnos = jntuh_college_faculty_registered_new.Where(e => e.FacultySpecializationId == 172 || e.FacultySpecializationId == 117 || e.FacultySpecializationId == 121).Select(e => e.RegistrationNumber).ToList();
                //ViewBag.Group4RemainingFaculty = jntuh_registered_faculty.Where(e => Group4RemainingRegnos.Contains(e.RegistrationNumber)).Select(e => e.RegistrationNumber).Count();

                var Group1RemainingRegnos = jntuh_registered_faculty.Where(e => e.Jntu_PGSpecializationId == 115 || e.Jntu_PGSpecializationId == 119 || e.Jntu_PGSpecializationId == 172 || e.Jntu_PGSpecializationId == 120).Select(e => e.RegistrationNumber).ToList();
                ViewBag.Group1RemainingFaculty = jntuh_registered_faculty.Where(e => Group1RemainingRegnos.Contains(e.RegistrationNumber)).Select(e => e.RegistrationNumber).Count();

                var Group2RemainingRegnos = jntuh_registered_faculty.Where(e => e.Jntu_PGSpecializationId == 116 || e.Jntu_PGSpecializationId == 167 || e.Jntu_PGSpecializationId == 123 || e.Jntu_PGSpecializationId == 170 || e.Jntu_PGSpecializationId == 117).Select(e => e.RegistrationNumber).ToList();
                ViewBag.Group2RemainingFaculty = jntuh_registered_faculty.Where(e => Group2RemainingRegnos.Contains(e.RegistrationNumber)).Select(e => e.RegistrationNumber).Count();

                var Group3RemainingRegnos = jntuh_registered_faculty.Where(e => e.Jntu_PGSpecializationId == 169 || e.Jntu_PGSpecializationId == 18 || e.Jntu_PGSpecializationId == 19 || e.Jntu_PGSpecializationId == 114 || e.Jntu_PGSpecializationId == 122).Select(e => e.RegistrationNumber).ToList();
                ViewBag.Group3RemainingFaculty = jntuh_registered_faculty.Where(e => Group3RemainingRegnos.Contains(e.RegistrationNumber)).Select(e => e.RegistrationNumber).Count();

                var Group4RemainingRegnos = jntuh_registered_faculty.Where(e => e.Jntu_PGSpecializationId == 172 || e.Jntu_PGSpecializationId == 117 || e.Jntu_PGSpecializationId == 121).Select(e => e.RegistrationNumber).ToList();
                ViewBag.Group4RemainingFaculty = jntuh_registered_faculty.Where(e => Group4RemainingRegnos.Contains(e.RegistrationNumber)).Select(e => e.RegistrationNumber).Count();



                var FacultyIds = jntuh_registered_faculty.Select(e => e.id).ToList();

                var jntuh_registered_faculty_education = db.jntuh_registered_faculty_education.Where(e => FacultyIds.Contains(e.facultyId)).Select(e => e).ToList();
                List<FacultyRegistration> data = new List<FacultyRegistration>();
                foreach (var item in jntuh_registered_faculty)
                {
                    string Reason = null;
                    FacultyRegistration Faculty = new FacultyRegistration();
                    Faculty.id = item.id;
                    Faculty.Type = item.type;
                    Faculty.RegistrationNumber = item.RegistrationNumber;
                    Faculty.UniqueID = item.UniqueID;
                    Faculty.FirstName = item.FirstName;
                    Faculty.MiddleName = item.MiddleName;
                    Faculty.LastName = item.LastName;
                    Faculty.GenderId = item.GenderId;
                    Faculty.Email = item.Email;
                    Faculty.facultyPhoto = item.Photo;
                    Faculty.Mobile = item.Mobile;
                    Faculty.PANNumber = item.PANNumber;
                    Faculty.AadhaarNumber = item.AadhaarNumber;
                    Faculty.DegreeId = jntuh_registered_faculty_education.Count(e => e.facultyId == item.id) > 0 ? item.jntuh_registered_faculty_education.Where(e => e.facultyId == item.id).Select(e => e.educationId).Max() : 0;
                    Faculty.PANNumber = item.PANNumber;

                    Faculty.Absent = item.Absent != null ? (bool)item.Absent : false;
                    Faculty.XeroxcopyofcertificatesFlag = item.Xeroxcopyofcertificates != null ? (bool)item.Xeroxcopyofcertificates : false;
                    Faculty.NOrelevantUgFlag = item.NoRelevantUG == "Yes" ? true : false;
                    Faculty.NOrelevantPgFlag = item.NoRelevantPG == "Yes" ? true : false;
                    Faculty.NOrelevantPhdFlag = item.NORelevantPHD == "Yes" ? true : false;
                    Faculty.NOTQualifiedAsPerAICTE = item.NotQualifiedAsperAICTE != null ? (bool)item.NotQualifiedAsperAICTE : false;
                    Faculty.InvalidPANNo = item.InvalidPANNumber != null ? (bool)item.InvalidPANNumber : false;
                    Faculty.InCompleteCeritificates = item.IncompleteCertificates == true ? true : false;
                    Faculty.NoSCM = item.NoSCM != null ? (bool)item.NoSCM : false;
                    Faculty.OriginalCertificatesnotshownFlag = item.OriginalCertificatesNotShown != null ? (bool)item.OriginalCertificatesNotShown : false;
                    Faculty.NotIdentityFiedForAnyProgramFlag = item.NotIdentityfiedForanyProgram != null ? (bool)item.NotIdentityfiedForanyProgram : false;
                    Faculty.Basstatus = item.InvalidAadhaar;
                    Faculty.BasstatusOld = item.BAS;
                    Faculty.OriginalsVerifiedUG = item.OriginalsVerifiedUG == true ? true : false;
                    Faculty.OriginalsVerifiedPHD = item.OriginalsVerifiedPHD == true ? true : false;
                    Faculty.BlacklistFaculty = item.Blacklistfaculy == true ? true : false;
                    Faculty.VerificationStatus = item.AbsentforVerification == true ? true : false;

                    //New Flags 
                    Faculty.InvalidDegree = item.Invaliddegree == true ? true : false;
                    Faculty.NoClass = item.Noclass == true ? true : false;
                    Faculty.GenuinenessnotSubmitted = item.Genuinenessnotsubmitted == true ? true : false;
                    Faculty.FakePhd = item.FakePHD == true ? true : false;
                    Faculty.NoPgSpecialization = item.NoPGspecialization == true ? true : false;
                    Faculty.NotconsiderPhd = item.NotconsideredPHD == true ? true : false;

                    Faculty.NoSCM17Flag = item.NoSCM17 != null ? (bool)item.NoSCM17 : false;
                    Faculty.PhdUndertakingDocumentstatus = item.PhdUndertakingDocumentstatus != null ? (bool)(item.PhdUndertakingDocumentstatus) : false;
                    Faculty.PHDUndertakingDocumentView = item.PHDUndertakingDocument;
                    Faculty.PhdUndertakingDocumentText = item.PhdUndertakingDocumentText;

                    Faculty.Deactivedby = item.DeactivatedBy;
                    Faculty.DeactivedOn = item.DeactivatedOn;

                    Faculty.DegreeId = jntuh_registered_faculty_education.Count(e => e.facultyId == item.id) > 0 ? item.jntuh_registered_faculty_education.Where(e => e.facultyId == item.id).Select(e => e.educationId).Max() : 0;

                    if (Faculty.Absent == true)
                        Reason += "Absent";

                    if (Faculty.Type == "Adjunct")
                    {
                        if (Reason != null)
                            Reason += ",Adjunct Faculty";
                        else
                            Reason += "Adjunct Faculty";
                    }

                    if (Faculty.XeroxcopyofcertificatesFlag == true)
                    {
                        if (Reason != null)
                            Reason += ",Xerox copyof certificates";
                        else
                            Reason += "Xerox copyof certificates";
                    }

                    if (Faculty.NOrelevantUgFlag == true)
                    {
                        if (Reason != null)
                            Reason += ",NO Relevant UG";
                        else
                            Reason += "NO Relevant UG";
                    }

                    if (Faculty.NOrelevantPgFlag == true)
                    {
                        if (Reason != null)
                            Reason += ",NO Relevant PG";
                        else
                            Reason += "NO Relevant PG";
                    }

                    if (Faculty.NOrelevantPhdFlag == true)
                    {
                        if (Reason != null)
                            Reason += ",NO Relevant PHD";
                        else
                            Reason += "NO Relevant PHD";
                    }

                    if (Faculty.NOTQualifiedAsPerAICTE == true)
                    {
                        if (Reason != null)
                            Reason += ",NOT Qualified AsPerAICTE";
                        else
                            Reason += "NOT Qualified AsPerAICTE";
                    }

                    if (Faculty.InvalidPANNo == true)
                    {
                        if (Reason != null)
                            Reason += ",InvalidPANNumber";
                        else
                            Reason += "InvalidPANNumber";
                    }

                    if (Faculty.InCompleteCeritificates == true)
                    {
                        if (Reason != null)
                            Reason += ",InComplete Ceritificates";
                        else
                            Reason += "InComplete Ceritificates";
                    }

                    if (Faculty.NoSCM == true)
                    {
                        if (Reason != null)
                            Reason += ",NoSCM";
                        else
                            Reason += "NoSCM";
                    }

                    if (Faculty.OriginalCertificatesnotshownFlag == true)
                    {
                        if (Reason != null)
                            Reason += ",Original Certificates notshown";
                        else
                            Reason += "Original Certificates notshown";
                    }

                    if (Faculty.PANNumber == null)
                    {
                        if (Reason != null)
                            Reason += ",No PANNumber";
                        else
                            Reason += "No PANNumber";
                    }

                    if (Faculty.NotIdentityFiedForAnyProgramFlag == true)
                    {
                        if (Reason != null)
                            Reason += ",NotIdentityFied ForAnyProgram";
                        else
                            Reason += "NotIdentityFied ForAnyProgram";
                    }

                    if (Faculty.SamePANUsedByMultipleFaculty == true)
                    {
                        if (Reason != null)
                            Reason += ",SamePANUsedByMultipleFaculty";
                        else
                            Reason += "SamePANUsedByMultipleFaculty";
                    }

                    if (Faculty.MultipleReginSamecoll == true)
                    {
                        if (Reason != null)
                            Reason += ",No Class in UG/PG";
                        else
                            Reason += "No Class in UG/PG";
                    }

                    if (Faculty.Basstatus == "Yes")
                    {
                        if (Reason != null)
                            Reason += ",No/Invalid Aadhaar Document";
                        else
                            Reason += "No/Invalid Aadhaar Document";
                    }

                    if (Faculty.BasstatusOld == "Yes")
                    {
                        if (Reason != null)
                            Reason += ",BAS Flag";
                        else
                            Reason += "BAS Flag";
                    }

                    if (Faculty.OriginalsVerifiedUG == true)
                    {
                        if (Reason != null)
                            Reason += ",Complaint PHD Faculty";
                        else
                            Reason += "Complaint PHD Faculty";
                    }

                    if (Faculty.OriginalsVerifiedPHD == true)
                    {
                        if (Reason != null)
                            Reason += ",No Guide Sign in PHD Thesis";
                        else
                            Reason += "No Guide Sign in PHD Thesis";
                    }

                    if (Faculty.BlacklistFaculty == true)
                    {
                        if (Reason != null)
                            Reason += ",BlackList";
                        else
                            Reason += "BlackList";
                    }

                    if (Faculty.VerificationStatus == true)
                    {
                        if (Reason != null)
                            Reason += ",Not Attend For Physical Verification";
                        else
                            Reason += "Not Attend For Physical Verification";
                    }

                    Faculty.DeactivationReason = Reason;

                    Faculty.SpecializationId = jntuh_college_faculty_registered.Where(e => e.RegistrationNumber == Faculty.RegistrationNumber).Select(e => e.SpecializationId).FirstOrDefault();
                    Faculty.SpecializationName = Faculty.SpecializationId != null ? jntuh_specialization.Where(e => e.id == Faculty.SpecializationId).Select(e => e.specializationName).FirstOrDefault() : null;
                    Faculty.IdentfiedFor = jntuh_college_faculty_registered.Where(e => e.RegistrationNumber == Faculty.RegistrationNumber).Select(e => e.IdentifiedFor).FirstOrDefault();
                    Faculty.PGSpecialization = item.Jntu_PGSpecializationId;
                    Faculty.PGSpecializationName = Faculty.PGSpecialization != null ? jntuh_specialization.Where(e => e.id == Faculty.PGSpecialization).Select(e => e.specializationName).FirstOrDefault() : null;

                    Faculty.PHD = jntuh_registered_faculty_education.Where(E => E.educationId == 6 && E.facultyId == item.id).Select(E => E.courseStudied).FirstOrDefault();
                    Faculty.CollegeId = collegeId;
                    Faculty.CollegeName = jntuh_college.collegeName;
                    Faculty.isVerified = false;

                    teachingFaculty.Add(Faculty);
                }
                return View(teachingFaculty.Where(e => e.DeactivationReason == null).ToList());
            }
            else
            {
                return View();
            }
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin,DataEntry,FacultyVerification")]
        public ActionResult PharmacyFacultyGroupingbasedonColleges(ICollection<FacultyRegistration> Faculty, int? collegeId, int? DegreeId, int? GroupId, int? TotalRequiredFaculty, int? SpecializationWiseFaculty)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            // int? CollegeID = collegeId;
            var CollegeId = collegeId;
            if (DegreeId != null && GroupId != null && TotalRequiredFaculty != null && SpecializationWiseFaculty != null)
            {
                TempData["RequiredFaculty"] = TotalRequiredFaculty;
                var FacultyData = new List<FacultyRegistration>();
                if (Faculty == null)
                {

                }
                else
                {
                    FacultyData = Faculty.Where(e => e.isVerified == true).Select(e => e).ToList();
                }

                var jntuh_department = db.jntuh_department.Where(e => e.isActive == true).Select(e => e).ToList();
                var jntuh_specialization = db.jntuh_specialization.Where(e => e.isActive == true).Select(e => e).ToList();
                var jntuh_college_intake_existing = db.jntuh_college_intake_existing.Where(e => e.collegeId == CollegeId && e.proposedIntake != 0).Select(e => e).ToList();

                int BPharmacyDepartmentId = 26;
                int BPharmacySpecializationId = 12;
                int? Proposedintake = 0;
                int? intake1 = 0;
                int? intake2 = 0;
                int? intake3 = 0;
                int? intake4 = 0;
                int? intake5 = 0;
                int? intake6 = 0;
                var jntuh_academic_year = db.jntuh_academic_year.ToList();

                int presentYear = jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
                int AY1 = jntuh_academic_year.Where(a => a.actualYear == (presentYear + 1)).Select(a => a.id).FirstOrDefault();
                int AY2 = jntuh_academic_year.Where(a => a.actualYear == (presentYear)).Select(a => a.id).FirstOrDefault();
                int AY3 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 1)).Select(a => a.id).FirstOrDefault();
                int AY4 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 2)).Select(a => a.id).FirstOrDefault();
                int AY5 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 3)).Select(a => a.id).FirstOrDefault();
                int AY6 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 4)).Select(a => a.id).FirstOrDefault();

                if (DegreeId == 5)
                {
                    Proposedintake = jntuh_college_intake_existing.Where(e => e.academicYearId == AY1 && e.specializationId == 12).Select(e => e.proposedIntake).FirstOrDefault();
                    intake1 = jntuh_college_intake_existing.Where(e => e.academicYearId == AY2 && e.specializationId == 12).Select(e => e.approvedIntake).FirstOrDefault();
                    intake2 = jntuh_college_intake_existing.Where(e => e.academicYearId == AY3 && e.specializationId == 12).Select(e => e.approvedIntake).FirstOrDefault();
                    intake3 = jntuh_college_intake_existing.Where(e => e.academicYearId == AY4 && e.specializationId == 12).Select(e => e.approvedIntake).FirstOrDefault();
                    intake4 = 0;
                    intake5 = 0;
                    intake6 = 0;
                }
                else
                {
                    if (GroupId == 18)
                    {
                        Proposedintake = jntuh_college_intake_existing.Where(e => e.academicYearId == AY1 && e.specializationId == GroupId).Select(e => e.proposedIntake).FirstOrDefault();
                        intake1 = jntuh_college_intake_existing.Where(e => e.academicYearId == AY2 && e.specializationId == GroupId).Select(e => e.approvedIntake).FirstOrDefault();
                        intake2 = jntuh_college_intake_existing.Where(e => e.academicYearId == AY3 && e.specializationId == GroupId).Select(e => e.approvedIntake).FirstOrDefault();
                        intake3 = jntuh_college_intake_existing.Where(e => e.academicYearId == AY4 && e.specializationId == GroupId).Select(e => e.approvedIntake).FirstOrDefault();
                        intake4 = jntuh_college_intake_existing.Where(e => e.academicYearId == AY5 && e.specializationId == GroupId).Select(e => e.approvedIntake).FirstOrDefault();
                        intake5 = 0;
                        intake6 = 0;
                    }
                    else if (GroupId == 19)
                    {
                        Proposedintake = jntuh_college_intake_existing.Where(e => e.academicYearId == AY1 && e.specializationId == GroupId).Select(e => e.proposedIntake).FirstOrDefault();
                        intake1 = jntuh_college_intake_existing.Where(e => e.academicYearId == AY2 && e.specializationId == GroupId).Select(e => e.approvedIntake).FirstOrDefault();
                        intake2 = jntuh_college_intake_existing.Where(e => e.academicYearId == AY3 && e.specializationId == GroupId).Select(e => e.approvedIntake).FirstOrDefault();
                        intake3 = jntuh_college_intake_existing.Where(e => e.academicYearId == AY4 && e.specializationId == GroupId).Select(e => e.approvedIntake).FirstOrDefault();
                        intake4 = jntuh_college_intake_existing.Where(e => e.academicYearId == AY5 && e.specializationId == GroupId).Select(e => e.approvedIntake).FirstOrDefault();
                        intake5 = jntuh_college_intake_existing.Where(e => e.academicYearId == AY6 && e.specializationId == GroupId).Select(e => e.approvedIntake).FirstOrDefault();
                        intake6 = 0;
                    }
                    else
                    {
                        Proposedintake = jntuh_college_intake_existing.Where(e => e.academicYearId == AY1 && e.specializationId == GroupId).Select(e => e.proposedIntake).FirstOrDefault();
                        intake1 = jntuh_college_intake_existing.Where(e => e.academicYearId == AY2 && e.specializationId == GroupId).Select(e => e.approvedIntake).FirstOrDefault();
                        intake2 = 0;
                        intake3 = 0;
                        intake4 = 0;
                        intake5 = 0;
                        intake6 = 0;
                    }


                }

                if (FacultyData.Count() == 0 || FacultyData == null)
                {
                    jntuh_appeal_pharmacydata pharmacy = new jntuh_appeal_pharmacydata();
                    pharmacy.CollegeCode = CollegeId.ToString();
                    pharmacy.Department = jntuh_department.Where(e => e.degreeId == DegreeId).Select(e => e.id).FirstOrDefault().ToString();
                    pharmacy.NoOfFacultyRequired = TotalRequiredFaculty;
                    pharmacy.SpecializationWiseRequiredFaculty = SpecializationWiseFaculty;
                    if (DegreeId == 5)
                    {
                        pharmacy.PharmacySpecialization = GroupId.ToString();
                        pharmacy.Specialization = BPharmacySpecializationId.ToString();
                        pharmacy.TotalIntake = (Proposedintake + intake1 + intake2 + intake3);
                    }
                    else
                    {
                        if (GroupId == 18)
                        {
                            pharmacy.TotalIntake = (Proposedintake + intake1 + intake2 + intake3 + intake4);
                        }
                        else if (GroupId == 19)
                        {
                            pharmacy.TotalIntake = (Proposedintake + intake1 + intake2 + intake3 + intake4 + intake5);
                        }
                        else
                        {
                            pharmacy.TotalIntake = (Proposedintake + intake1);
                        }
                        pharmacy.PharmacySpecialization = null;
                        pharmacy.Specialization = GroupId.ToString();

                    }
                    pharmacy.ProposedIntake = Proposedintake;
                    //pharmacy.TotalIntake = (Proposedintake + intake1 + intake2 + intake3 + intake4 + intake5 + intake6);
                    pharmacy.Deficiency = null;
                    pharmacy.CreatedOn = DateTime.Now;
                    pharmacy.IsActive = true;
                    db.jntuh_appeal_pharmacydata.Add(pharmacy);
                    db.SaveChanges();
                    TempData["Success"] = "Course Details Are Added Without Faculty";
                }
                else
                {
                    foreach (var item in FacultyData)
                    {

                        jntuh_appeal_pharmacydata pharmacy = new jntuh_appeal_pharmacydata();
                        pharmacy.CollegeCode = CollegeId.ToString();
                        pharmacy.Department = jntuh_department.Where(e => e.degreeId == DegreeId).Select(e => e.id).FirstOrDefault().ToString();
                        pharmacy.NoOfFacultyRequired = TotalRequiredFaculty;
                        pharmacy.SpecializationWiseRequiredFaculty = SpecializationWiseFaculty;
                        if (DegreeId == 5)
                        {
                            pharmacy.PharmacySpecialization = GroupId.ToString();
                            pharmacy.Specialization = BPharmacySpecializationId.ToString();
                            pharmacy.TotalIntake = (Proposedintake + intake1 + intake2 + intake3);
                        }
                        else
                        {
                            if (GroupId == 18)
                            {
                                pharmacy.TotalIntake = (Proposedintake + intake1 + intake2 + intake3 + intake4);
                            }
                            else if (GroupId == 19)
                            {
                                pharmacy.TotalIntake = (Proposedintake + intake1 + intake2 + intake3 + intake4 + intake5);
                            }
                            else
                            {
                                pharmacy.TotalIntake = (Proposedintake + intake1);
                            }
                            pharmacy.PharmacySpecialization = null;
                            pharmacy.Specialization = GroupId.ToString();

                        }
                        pharmacy.ProposedIntake = Proposedintake;
                        //  pharmacy.TotalIntake = (intake1 + intake2 + intake3 + intake4 + intake5 + intake6);
                        pharmacy.Deficiency = item.RegistrationNumber;
                        pharmacy.CreatedOn = DateTime.Now;
                        pharmacy.IsActive = true;
                        db.jntuh_appeal_pharmacydata.Add(pharmacy);
                        db.SaveChanges();
                        TempData["Success"] = "Registration Details Are Added";
                    }
                }

                //if (FacultyData.Count() == 0)
                //{
                //    TempData["Error"] = "Please Select The Faculty";
                //}
                return RedirectToAction("PharmacyFacultyGroupingbasedonColleges", new { collegeId = CollegeId, DegreeId = DegreeId, GroupId = GroupId, totalRequiredFaculty = TotalRequiredFaculty });
            }

            return RedirectToAction("PharmacyFacultyGroupingbasedonColleges", new { collegeId = CollegeId });
        }

        [Authorize(Roles = "Admin,SuperAdmin,DataEntry,FacultyVerification")]
        public ActionResult ViewAddedFaculty(int? collegeId)
        {

            int userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            var CollegeIds = new int[] { 448, 180, 159, 376, 428, 445, 24, 117, 202, 213, 395, 364, 27, 30, 6, 34, 55, 58, 54, 52, 9, 60, 65, 78, 150, 204, 219, 90, 97, 104, 110, 139, 105, 107, 114, 379, 118, 39, 120, 389, 127, 237, 135, 136, 47, 252, 169, 253, 262, 442, 301, 436, 370, 263, 267, 206, 140, 44, 290, 454, 42, 95, 297, 384, 75, 348, 298, 295, 303, 302, 283, 332, 410, 392, 234, 146, 315, 320, 317, 314, 318, 319, 353, 313, 235, 375 };
            var jntuhcollege = db.jntuh_college.AsNoTracking().Where(e => CollegeIds.Contains(e.id)).ToList();

            ViewBag.PharmacyCollegeList = jntuhcollege.Select(e => new
            {
                collegeId = e.id,
                collegeName = e.collegeCode + "-" + e.collegeName
            }).ToList();
            if (collegeId != null)
            {
                string collegeid = collegeId.ToString();
                var jntuh_appeal_pharmacydata = db.jntuh_appeal_pharmacydata.Where(e => e.CollegeCode == collegeid).Select(e => e).ToList();
                var StrRegNos = jntuh_appeal_pharmacydata.Where(e => e.Deficiency != null).Select(e => e.Deficiency.Trim()).ToList();

                var jntuh_college_prinicipal_registered = db.jntuh_college_principal_registered.Where(e => e.collegeId == collegeId).Select(e => e.RegistrationNumber).FirstOrDefault();
                ViewBag.Prinicipal = jntuh_college_prinicipal_registered;

                var jntuh_college_faculty_registered = db.jntuh_college_faculty_registered.Where(e => StrRegNos.Contains(e.RegistrationNumber.Trim())).Select(e => e).ToList();
                var jntuh_registered_faculty = db.jntuh_registered_faculty.Where(e => StrRegNos.Contains(e.RegistrationNumber.Trim())).Select(e => e).ToList();
                var FacultyIds = jntuh_registered_faculty.Select(e => e.id).ToList();
                var jntuh_faculty_registered_education = db.jntuh_registered_faculty_education.Where(e => FacultyIds.Contains(e.facultyId)).Select(e => e).ToList();

                var jntuh_specialization = db.jntuh_specialization.Where(e => e.isActive == true).ToList();
                var jntuh_department = db.jntuh_department.Where(e => e.isActive == true).ToList();
                List<FacultyRegistration> Faculty = new List<FacultyRegistration>();

                foreach (var item in jntuh_appeal_pharmacydata)
                {
                    FacultyRegistration eachfaculty = new FacultyRegistration();
                    if (item.Deficiency == null)
                    {
                        eachfaculty.id = item.Sno;
                        eachfaculty.RegistrationNumber = item.Deficiency; ;
                        eachfaculty.FirstName = null;
                        eachfaculty.MiddleName = null;
                        eachfaculty.LastName = null;

                        eachfaculty.SpecializationId = null;
                        eachfaculty.SpecializationName = null;
                        eachfaculty.IdentfiedFor = null;
                        eachfaculty.PGSpecialization = null;
                        eachfaculty.PGSpecializationName = null;

                        eachfaculty.DepartmentId = Convert.ToInt32(item.Department);
                        eachfaculty.department = eachfaculty.DepartmentId != null ? jntuh_department.Where(e => e.id == eachfaculty.DepartmentId).Select(e => e.departmentName).FirstOrDefault() : null;
                        eachfaculty.DesignationId = Convert.ToInt32(item.Specialization);
                        eachfaculty.designation = eachfaculty.DesignationId == null ? null : jntuh_specialization.Where(e => e.id == eachfaculty.DesignationId).Select(e => e.specializationName).FirstOrDefault();
                        eachfaculty.Eid = item.PharmacySpecialization == null ? 0 : Convert.ToInt32(item.PharmacySpecialization);

                        eachfaculty.PHD = null;
                    }
                    else
                    {
                        //FacultyRegistration eachfaculty = new FacultyRegistration();
                        eachfaculty.id = item.Sno;
                        eachfaculty.RegistrationNumber = item.Deficiency.Trim();
                        eachfaculty.FirstName = jntuh_registered_faculty.Where(e => e.RegistrationNumber == item.Deficiency.Trim()).Select(e => e.FirstName).FirstOrDefault();
                        eachfaculty.MiddleName = jntuh_registered_faculty.Where(e => e.RegistrationNumber == item.Deficiency.Trim()).Select(e => e.MiddleName).FirstOrDefault();
                        eachfaculty.LastName = jntuh_registered_faculty.Where(e => e.RegistrationNumber == item.Deficiency.Trim()).Select(e => e.LastName).FirstOrDefault();

                        eachfaculty.SpecializationId = jntuh_college_faculty_registered.Where(e => e.RegistrationNumber.Trim() == item.Deficiency.Trim()).Select(e => e.SpecializationId).FirstOrDefault();
                        eachfaculty.SpecializationName = eachfaculty.SpecializationId == null ? null : jntuh_specialization.Where(e => e.id == eachfaculty.SpecializationId).Select(e => e.specializationName).FirstOrDefault();
                        eachfaculty.IdentfiedFor = jntuh_college_faculty_registered.Where(e => e.RegistrationNumber.Trim() == item.Deficiency.Trim()).Select(e => e.IdentifiedFor).FirstOrDefault();
                        eachfaculty.PGSpecialization = jntuh_registered_faculty.Where(e => e.RegistrationNumber.Trim() == item.Deficiency.Trim()).Select(e => e.Jntu_PGSpecializationId).FirstOrDefault();
                        eachfaculty.PGSpecializationName = eachfaculty.PGSpecialization == null ? null : jntuh_specialization.Where(e => e.id == eachfaculty.PGSpecialization).Select(e => e.specializationName).FirstOrDefault();

                        eachfaculty.DepartmentId = Convert.ToInt32(item.Department);
                        eachfaculty.department = eachfaculty.DepartmentId == null ? null : jntuh_department.Where(e => e.id == eachfaculty.DepartmentId).Select(e => e.departmentName).FirstOrDefault();
                        eachfaculty.DesignationId = Convert.ToInt32(item.Specialization);
                        eachfaculty.designation = eachfaculty.DesignationId == null ? null : jntuh_specialization.Where(e => e.id == eachfaculty.DesignationId).Select(e => e.specializationName).FirstOrDefault();
                        eachfaculty.Eid = item.PharmacySpecialization == null ? 0 : Convert.ToInt32(item.PharmacySpecialization);

                        var Notconsiderphd = jntuh_registered_faculty.Where(e => e.RegistrationNumber.Trim() == item.Deficiency.Trim()).Select(e => e.NotconsideredPHD).FirstOrDefault();
                        eachfaculty.NotconsiderPhd = Notconsiderphd == true ? true : false;
                        eachfaculty.PHD = jntuh_faculty_registered_education.Where(e => e.educationId == 6 && e.facultyId == jntuh_registered_faculty.Where(s => s.RegistrationNumber == item.Deficiency).Select(q => q.id).FirstOrDefault()).Select(w => w.courseStudied).FirstOrDefault();
                    }

                    Faculty.Add(eachfaculty);
                }
                return View(Faculty.Where(e => e.RegistrationNumber != null).ToList());
            }
            else
            {
                return View();
            }
            return View();
        }

        [Authorize(Roles = "Admin,SuperAdmin,DataEntry,FacultyVerification")]
        public ActionResult EditPharmacyIntake(int? collegeId, int? DegreeId, int? GroupId)
        {
            var CollegeIds = new int[] { 448, 180, 159, 376, 428, 445, 24, 117, 202, 213, 395, 364, 27, 30, 6, 34, 55, 58, 54, 52, 9, 60, 65, 78, 150, 204, 219, 90, 97, 104, 110, 139, 105, 107, 114, 379, 118, 39, 120, 389, 127, 237, 135, 136, 47, 252, 169, 253, 262, 442, 301, 436, 370, 263, 267, 206, 140, 44, 290, 454, 42, 95, 297, 384, 75, 348, 298, 295, 303, 302, 283, 332, 410, 392, 234, 146, 315, 320, 317, 314, 318, 319, 353, 313, 235, 375 };
            var jntuhcollege = db.jntuh_college.AsNoTracking().Where(e => CollegeIds.Contains(e.id)).ToList();

            ViewBag.PharmacyCollegeList = jntuhcollege.Select(e => new
            {
                collegeId = e.id,
                collegeName = e.collegeCode + "-" + e.collegeName
            }).ToList();

            List<SelectListItem> DegreesFirst = new List<SelectListItem>();
            DegreesFirst.Add(new SelectListItem { Text = "---Select---", Value = "1" });
            ViewBag.Degrees = DegreesFirst.Select(w => new
            {
                DegreeId = w.Value,
                Degreename = w.Text
            }).ToList();

            List<SelectListItem> GroupsFirst = new List<SelectListItem>();
            GroupsFirst.Add(new SelectListItem { Text = "---Select---", Value = "1" });
            ViewBag.Groups = GroupsFirst.Select(e => new
            {
                GroupId = e.Value,
                GroupName = e.Text
            }).ToList();

            int presentYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int AY1 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear + 1)).Select(a => a.id).FirstOrDefault();

            var jntuh_department = db.jntuh_department.Where(e => e.isActive == true).Select(e => e).ToList();
            var jntuh_specialization = db.jntuh_specialization.Where(e => e.isActive == true).Select(e => e).ToList();
            if (collegeId != null && DegreeId == null && GroupId == null)
            {
                ViewBag.AfterCollegeSelect = null;

                var jntuh_college_intake_existing = db.jntuh_college_intake_existing.Where(e => e.collegeId == collegeId).Select(e => e).ToList();
                var jntuh_college_intake_existing_Data = jntuh_college_intake_existing.Where(e => e.academicYearId == AY1 && e.courseStatus != "Closure").Select(w => w).ToList();
                var SpecializationIds = jntuh_college_intake_existing_Data.Select(e => e.specializationId).Distinct().ToArray();
                var Degrees = (from e in jntuh_college_intake_existing_Data
                               join s in jntuh_specialization on e.specializationId equals s.id
                               join d in jntuh_department on s.departmentId equals d.id
                               join de in db.jntuh_degree on d.degreeId equals de.id
                               where SpecializationIds.Contains(s.id) && (de.id == 2 || de.id == 5 || de.id == 9 || de.id == 10)
                               select new
                               {
                                   DegreeId = de.id,
                                   Degreename = de.degree
                               }).Distinct().ToList();

                ViewBag.Degrees = Degrees;

                List<SelectListItem> Groups = new List<SelectListItem>();
                Groups.Add(new SelectListItem { Text = "Group1", Value = "1" });
                Groups.Add(new SelectListItem { Text = "Group2", Value = "2" });
                Groups.Add(new SelectListItem { Text = "Group3", Value = "3" });
                Groups.Add(new SelectListItem { Text = "Group4", Value = "4" });
                ViewBag.Groups = Groups.Select(e => new
                {
                    GroupId = e.Value,
                    GroupName = e.Text
                }).ToList();

                string CollegeID = collegeId.ToString();
                var jntuh_appeal_pharmacyData = db.jntuh_appeal_pharmacydata.Where(e => e.CollegeCode == CollegeID).Select(e => e).ToList();

                List<PharmacyIntakeFaculty> PharmacyList = new List<PharmacyIntakeFaculty>();

                if (jntuh_appeal_pharmacyData.Count() == 0 || jntuh_appeal_pharmacyData.Count() == null)
                {
                    return View(PharmacyList);
                }
                else
                {
                    var jntuh_appeal_pharmacyData_group = jntuh_appeal_pharmacyData.GroupBy(e => new { e.Department, e.Specialization, e.PharmacySpecialization }).Select(e => e.First()).ToList();
                    foreach (var item in jntuh_appeal_pharmacyData_group)
                    {
                        PharmacyIntakeFaculty Pharmacy = new PharmacyIntakeFaculty();
                        Pharmacy.collegeId = Convert.ToInt32(item.CollegeCode);
                        Pharmacy.DegreeId = Convert.ToInt32(item.Department);
                        Pharmacy.Department = jntuh_department.Where(e => e.id == Pharmacy.DegreeId).Select(e => e.departmentName).FirstOrDefault();
                        Pharmacy.SpecializationId = Convert.ToInt32(item.Specialization);
                        Pharmacy.Specialization = jntuh_specialization.Where(e => e.id == Pharmacy.SpecializationId).Select(e => e.specializationName).FirstOrDefault();
                        Pharmacy.GroupId = item.PharmacySpecialization;
                        // Pharmacy.TotalIntake = jntuh_college_intake_existing_Data.Where(e => e.specializationId == Pharmacy.SpecializationId).Select(e => e.proposedIntake).FirstOrDefault();
                        Pharmacy.TotalIntake = item.TotalIntake;
                        Pharmacy.ProposedIntake = item.ProposedIntake;
                        Pharmacy.TotalRequiredFaculty = item.NoOfFacultyRequired;
                        Pharmacy.SpecializationWiseFaculty = item.SpecializationWiseRequiredFaculty;
                        Pharmacy.FacultyCount = jntuh_appeal_pharmacyData.Where(e => e.Department == item.Department && e.Specialization == item.Specialization && e.PharmacySpecialization == item.PharmacySpecialization && e.Deficiency != null).Select(e => e.Deficiency).Count();
                        PharmacyList.Add(Pharmacy);
                    }
                }
                return View(PharmacyList);
            }
            else if (collegeId != null && DegreeId != null && GroupId == null)
            {
                ViewBag.AfterCollegeSelect = GroupId;

                var jntuh_college_intake_existing = db.jntuh_college_intake_existing.Where(e => e.collegeId == collegeId).Select(e => e).ToList();
                var jntuh_college_intake_existing_Data = jntuh_college_intake_existing.Where(e => e.academicYearId == AY1 && e.courseStatus != "Closure").Select(w => w).ToList();
                var SpecializationIds = jntuh_college_intake_existing_Data.Select(e => e.specializationId).Distinct().ToArray();
                var Degrees = (from e in jntuh_college_intake_existing_Data
                               join s in jntuh_specialization on e.specializationId equals s.id
                               join d in jntuh_department on s.departmentId equals d.id
                               join de in db.jntuh_degree on d.degreeId equals de.id
                               where SpecializationIds.Contains(s.id) && (de.id == 2 || de.id == 5 || de.id == 9 || de.id == 10)
                               select new
                               {
                                   DegreeId = de.id,
                                   Degreename = de.degree
                               }).Distinct().ToList();

                ViewBag.Degrees = Degrees;

                string Departmentid = jntuh_department.Where(e => e.degreeId == DegreeId).Select(e => e.id).FirstOrDefault().ToString();
                string collegeID = collegeId.ToString();
                string DegreeID = DegreeId.ToString();
                string GroupID = GroupId.ToString();
                var pharmacyDatabasedonGroup = new jntuh_appeal_pharmacydata();
                if (DegreeId == 5)
                {
                    pharmacyDatabasedonGroup = db.jntuh_appeal_pharmacydata.Where(e => e.CollegeCode == collegeID && e.Department == Departmentid).Select(e => e).FirstOrDefault();
                    List<SelectListItem> Groups = new List<SelectListItem>();
                    Groups.Add(new SelectListItem { Text = "Group1", Value = "1" });
                    Groups.Add(new SelectListItem { Text = "Group2", Value = "2" });
                    Groups.Add(new SelectListItem { Text = "Group3", Value = "3" });
                    Groups.Add(new SelectListItem { Text = "Group4", Value = "4" });
                    ViewBag.Groups = Groups.Select(e => new
                    {
                        GroupId = e.Value,
                        GroupName = e.Text
                    }).ToList();
                }
                if (pharmacyDatabasedonGroup == null)
                {
                    TempData["TotalRequiredFaculty"] = null;
                    TempData["SpecializationWiseFaculty"] = null;
                }
                else
                {
                    TempData["TotalRequiredFaculty"] = pharmacyDatabasedonGroup.NoOfFacultyRequired;
                    int specializationID = Convert.ToInt32(pharmacyDatabasedonGroup.Specialization);
                    TempData["ProposedIntake"] = jntuh_college_intake_existing_Data.Where(e => e.specializationId == specializationID).Select(e => e.proposedIntake).FirstOrDefault();
                    //TempData["SpecializationWiseFaculty"] = pharmacyDatabasedonGroup.SpecializationWiseRequiredFaculty;
                }

                string CollegeID = collegeId.ToString();
                var jntuh_appeal_pharmacyData = db.jntuh_appeal_pharmacydata.Where(e => e.CollegeCode == CollegeID).Select(e => e).ToList();

                List<PharmacyIntakeFaculty> PharmacyList = new List<PharmacyIntakeFaculty>();

                if (jntuh_appeal_pharmacyData.Count() == 0 || jntuh_appeal_pharmacyData.Count() == null)
                {
                    return View(PharmacyList);
                }
                else
                {
                    var jntuh_appeal_pharmacyData_group = jntuh_appeal_pharmacyData.GroupBy(e => new { e.Department, e.Specialization, e.PharmacySpecialization }).Select(e => e.First()).ToList();
                    foreach (var item in jntuh_appeal_pharmacyData_group)
                    {
                        PharmacyIntakeFaculty Pharmacy = new PharmacyIntakeFaculty();
                        Pharmacy.collegeId = Convert.ToInt32(item.CollegeCode);
                        Pharmacy.DegreeId = Convert.ToInt32(item.Department);
                        Pharmacy.Department = jntuh_department.Where(e => e.id == Pharmacy.DegreeId).Select(e => e.departmentName).FirstOrDefault();
                        Pharmacy.SpecializationId = Convert.ToInt32(item.Specialization);
                        Pharmacy.Specialization = jntuh_specialization.Where(e => e.id == Pharmacy.SpecializationId).Select(e => e.specializationName).FirstOrDefault();
                        Pharmacy.GroupId = item.PharmacySpecialization;
                        // Pharmacy.TotalIntake = jntuh_college_intake_existing_Data.Where(e => e.specializationId == Pharmacy.SpecializationId).Select(e => e.proposedIntake).FirstOrDefault();
                        Pharmacy.TotalIntake = item.TotalIntake;
                        Pharmacy.ProposedIntake = item.ProposedIntake;
                        Pharmacy.TotalRequiredFaculty = item.NoOfFacultyRequired;
                        Pharmacy.SpecializationWiseFaculty = item.SpecializationWiseRequiredFaculty;
                        Pharmacy.FacultyCount = jntuh_appeal_pharmacyData.Where(e => e.Department == item.Department && e.Specialization == item.Specialization && e.PharmacySpecialization == item.PharmacySpecialization && e.Deficiency != null).Select(e => e.Deficiency).Count();

                        PharmacyList.Add(Pharmacy);
                    }
                }
                return View(PharmacyList);
            }
            else if (collegeId != null && DegreeId != null && GroupId != null)
            {

                ViewBag.AfterCollegeSelect = GroupId;

                var jntuh_college_intake_existing = db.jntuh_college_intake_existing.Where(e => e.collegeId == collegeId).Select(e => e).ToList();
                var jntuh_college_intake_existing_Data = jntuh_college_intake_existing.Where(e => e.academicYearId == AY1 && e.courseStatus != "Closure").Select(w => w).ToList();
                var SpecializationIds = jntuh_college_intake_existing_Data.Select(e => e.specializationId).Distinct().ToArray();
                var Degrees = (from e in jntuh_college_intake_existing_Data
                               join s in jntuh_specialization on e.specializationId equals s.id
                               join d in jntuh_department on s.departmentId equals d.id
                               join de in db.jntuh_degree on d.degreeId equals de.id
                               where SpecializationIds.Contains(s.id) && (de.id == 2 || de.id == 5 || de.id == 9 || de.id == 10)
                               select new
                               {
                                   DegreeId = de.id,
                                   Degreename = de.degree
                               }).Distinct().ToList();

                ViewBag.Degrees = Degrees;

                string Departmentid = jntuh_department.Where(e => e.degreeId == DegreeId).Select(e => e.id).FirstOrDefault().ToString();
                string collegeID = collegeId.ToString();
                string DegreeID = DegreeId.ToString();
                string GroupID = GroupId.ToString();
                var pharmacyDatabasedonGroup = new jntuh_appeal_pharmacydata();
                if (DegreeId == 5)
                {
                    pharmacyDatabasedonGroup = db.jntuh_appeal_pharmacydata.Where(e => e.CollegeCode == collegeID && e.Department == Departmentid && e.PharmacySpecialization == GroupID).Select(e => e).FirstOrDefault();
                    List<SelectListItem> Groups = new List<SelectListItem>();
                    Groups.Add(new SelectListItem { Text = "Group1", Value = "1" });
                    Groups.Add(new SelectListItem { Text = "Group2", Value = "2" });
                    Groups.Add(new SelectListItem { Text = "Group3", Value = "3" });
                    Groups.Add(new SelectListItem { Text = "Group4", Value = "4" });
                    ViewBag.Groups = Groups.Select(e => new
                    {
                        GroupId = e.Value,
                        GroupName = e.Text
                    }).ToList();
                }
                else
                {
                    pharmacyDatabasedonGroup = db.jntuh_appeal_pharmacydata.Where(e => e.CollegeCode == collegeID && e.Department == Departmentid && e.Specialization == GroupID).Select(e => e).FirstOrDefault();

                    var Spec = (from e in jntuh_college_intake_existing_Data
                                join s in jntuh_specialization on e.specializationId equals s.id
                                join d in jntuh_department on s.departmentId equals d.id
                                join de in db.jntuh_degree on d.degreeId equals de.id
                                where SpecializationIds.Contains(s.id) && de.id == DegreeId
                                select new
                                {
                                    GroupId = s.id,
                                    GroupName = s.specializationName
                                }).Distinct().ToList();
                    ViewBag.Groups = Spec;
                }

                if (pharmacyDatabasedonGroup == null)
                {
                    TempData["TotalRequiredFaculty"] = null;
                    TempData["SpecializationWiseFaculty"] = null;
                }
                else
                {
                    TempData["TotalRequiredFaculty"] = pharmacyDatabasedonGroup.NoOfFacultyRequired;
                    TempData["SpecializationWiseFaculty"] = pharmacyDatabasedonGroup.SpecializationWiseRequiredFaculty;
                    int specializationID = Convert.ToInt32(pharmacyDatabasedonGroup.Specialization);
                    TempData["ProposedIntake"] = jntuh_college_intake_existing_Data.Where(e => e.specializationId == specializationID).Select(e => e.proposedIntake).FirstOrDefault();
                }

                string CollegeID = collegeId.ToString();
                var jntuh_appeal_pharmacyData = db.jntuh_appeal_pharmacydata.Where(e => e.CollegeCode == CollegeID).Select(e => e).ToList();

                List<PharmacyIntakeFaculty> PharmacyList = new List<PharmacyIntakeFaculty>();

                if (jntuh_appeal_pharmacyData.Count() == 0 || jntuh_appeal_pharmacyData.Count() == null)
                {
                    return View(PharmacyList);
                }
                else
                {
                    var jntuh_appeal_pharmacyData_group = jntuh_appeal_pharmacyData.GroupBy(e => new { e.Department, e.Specialization, e.PharmacySpecialization }).Select(e => e.First()).ToList();
                    foreach (var item in jntuh_appeal_pharmacyData_group)
                    {
                        PharmacyIntakeFaculty Pharmacy = new PharmacyIntakeFaculty();
                        Pharmacy.collegeId = Convert.ToInt32(item.CollegeCode);
                        Pharmacy.DegreeId = Convert.ToInt32(item.Department);
                        Pharmacy.Department = jntuh_department.Where(e => e.id == Pharmacy.DegreeId).Select(e => e.departmentName).FirstOrDefault();
                        Pharmacy.SpecializationId = Convert.ToInt32(item.Specialization);
                        Pharmacy.Specialization = jntuh_specialization.Where(e => e.id == Pharmacy.SpecializationId).Select(e => e.specializationName).FirstOrDefault();
                        Pharmacy.GroupId = item.PharmacySpecialization;
                        // Pharmacy.TotalIntake = jntuh_college_intake_existing_Data.Where(e => e.specializationId == Pharmacy.SpecializationId).Select(e => e.proposedIntake).FirstOrDefault();
                        Pharmacy.TotalIntake = item.TotalIntake;
                        Pharmacy.ProposedIntake = item.ProposedIntake;
                        Pharmacy.TotalRequiredFaculty = item.NoOfFacultyRequired;
                        Pharmacy.SpecializationWiseFaculty = item.SpecializationWiseRequiredFaculty;
                        Pharmacy.FacultyCount = jntuh_appeal_pharmacyData.Where(e => e.Department == item.Department && e.Specialization == item.Specialization && e.PharmacySpecialization == item.PharmacySpecialization && e.Deficiency != null).Select(e => e.Deficiency).Count();

                        PharmacyList.Add(Pharmacy);
                    }
                }
                return View(PharmacyList);
            }
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin,DataEntry,FacultyVerification")]
        public ActionResult EditPharmacyIntake(int? collegeId, int? DegreeId, int? GroupId, int? TotalRequiredFaculty, int? SpecializationWiseFaculty, int? ProposedIntake)
        {
            var CollegeIds = new int[] { 448, 180, 159, 376, 428, 445, 24, 117, 202, 213, 395, 364, 27, 30, 6, 34, 55, 58, 54, 52, 9, 60, 65, 78, 150, 204, 219, 90, 97, 104, 110, 139, 105, 107, 114, 379, 118, 39, 120, 389, 127, 237, 135, 136, 47, 252, 169, 253, 262, 442, 301, 436, 370, 263, 267, 206, 140, 44, 290, 454, 42, 95, 297, 384, 75, 348, 298, 295, 303, 302, 283, 332, 410, 392, 234, 146, 315, 320, 317, 314, 318, 319, 353, 313, 235, 375 };
            var jntuhcollege = db.jntuh_college.AsNoTracking().Where(e => CollegeIds.Contains(e.id)).ToList();

            ViewBag.PharmacyCollegeList = jntuhcollege.Select(e => new
            {
                collegeId = e.id,
                collegeName = e.collegeCode + "-" + e.collegeName
            }).ToList();

            if (collegeId != null && DegreeId != null && GroupId != null && TotalRequiredFaculty != null && SpecializationWiseFaculty != null)
            {
                string collegeID = collegeId.ToString();
                string DegreeID = DegreeId.ToString();
                string GroupID = GroupId.ToString();
                var PharmacySpecializationData = new List<jntuh_appeal_pharmacydata>();

                if (DegreeId == 5)
                {
                    string DepartmentId = db.jntuh_department.Where(e => e.degreeId == DegreeId).Select(e => e.id).FirstOrDefault().ToString();
                    PharmacySpecializationData = db.jntuh_appeal_pharmacydata.Where(e => e.CollegeCode == collegeID && e.Department == DepartmentId && e.PharmacySpecialization == GroupID).Select(e => e).ToList();
                }
                else
                {
                    string DepartmentId = db.jntuh_department.Where(e => e.degreeId == DegreeId).Select(e => e.id).FirstOrDefault().ToString();
                    PharmacySpecializationData = db.jntuh_appeal_pharmacydata.Where(e => e.CollegeCode == collegeID && e.Department == DepartmentId && e.Specialization == GroupID).Select(e => e).ToList();
                }

                foreach (var item in PharmacySpecializationData)
                {
                    var data = item;
                    jntuh_appeal_pharmacydata PharmacyData = new jntuh_appeal_pharmacydata();
                    PharmacyData.Sno = data.Sno;
                    PharmacyData.CollegeCode = data.CollegeCode;
                    PharmacyData.Department = data.Department;
                    PharmacyData.Specialization = data.Specialization;
                    PharmacyData.TotalIntake = data.TotalIntake;
                    PharmacyData.ProposedIntake = ProposedIntake;
                    // PharmacyData.ProposedIntake = data.ProposedIntake;
                    PharmacyData.NoOfFacultyRequired = TotalRequiredFaculty;
                    PharmacyData.NoOfFacultyAvilabli = data.NoOfFacultyAvilabli;
                    PharmacyData.SpecializationWiseRequiredFaculty = SpecializationWiseFaculty;
                    PharmacyData.SpecializationWiseAvilableFaculty = data.SpecializationWiseAvilableFaculty;
                    PharmacyData.PharmacySpecialization = data.PharmacySpecialization;
                    PharmacyData.Deficiency = data.Deficiency;
                    PharmacyData.PhDFaculty = data.PhDFaculty;
                    PharmacyData.CreatedOn = DateTime.Now;
                    PharmacyData.IsActive = data.IsActive;
                    db.Entry(data).CurrentValues.SetValues(PharmacyData);
                    db.SaveChanges();

                }
                TempData["Success"] = "Data is Updated Successfully";
                return RedirectToAction("EditPharmacyIntake", new { CollegeId = collegeId, DegreeID = DegreeId, GroupID = GroupId });
            }
            else if (collegeId != null && DegreeId != null && GroupId == null && TotalRequiredFaculty != null)
            {
                string collegeID = collegeId.ToString();
                string DegreeID = DegreeId.ToString();
                string GroupID = GroupId.ToString();
                var PharmacySpecializationData = new List<jntuh_appeal_pharmacydata>();

                string DepartmentId = db.jntuh_department.Where(e => e.degreeId == DegreeId).Select(e => e.id).FirstOrDefault().ToString();
                PharmacySpecializationData = db.jntuh_appeal_pharmacydata.Where(e => e.CollegeCode == collegeID && e.Department == DepartmentId).Select(e => e).ToList();

                foreach (var item in PharmacySpecializationData)
                {
                    var data = item;
                    jntuh_appeal_pharmacydata PharmacyData = new jntuh_appeal_pharmacydata();
                    PharmacyData.Sno = data.Sno;
                    PharmacyData.CollegeCode = data.CollegeCode;
                    PharmacyData.Department = data.Department;
                    PharmacyData.Specialization = data.Specialization;
                    PharmacyData.TotalIntake = data.TotalIntake;
                    PharmacyData.ProposedIntake = ProposedIntake;
                    // PharmacyData.ProposedIntake = data.ProposedIntake;
                    PharmacyData.NoOfFacultyRequired = TotalRequiredFaculty;
                    PharmacyData.NoOfFacultyAvilabli = data.NoOfFacultyAvilabli;
                    PharmacyData.SpecializationWiseRequiredFaculty = data.SpecializationWiseRequiredFaculty;
                    PharmacyData.SpecializationWiseAvilableFaculty = data.SpecializationWiseAvilableFaculty;
                    PharmacyData.PharmacySpecialization = data.PharmacySpecialization;
                    PharmacyData.Deficiency = data.Deficiency;
                    PharmacyData.PhDFaculty = data.PhDFaculty;
                    PharmacyData.CreatedOn = DateTime.Now;
                    PharmacyData.IsActive = data.IsActive;
                    db.Entry(data).CurrentValues.SetValues(PharmacyData);
                    db.SaveChanges();

                }
                TempData["Success"] = "Data is Updated Successfully";
                return RedirectToAction("EditPharmacyIntake", new { CollegeId = collegeId, DegreeID = DegreeId, GroupID = GroupId });
            }
            else
            {
                return RedirectToAction("EditPharmacyIntake");
            }

            //return View();
        }

        [Authorize(Roles = "Admin,SuperAdmin,DataEntry,FacultyVerification")]
        public ActionResult DeleteAddedFaculty(int? Id)
        {
            if (Id != null)
            {
                var PharmacyFaculty = db.jntuh_appeal_pharmacydata.Where(e => e.Sno == Id).Select(e => e).FirstOrDefault();
                int collegeid = Convert.ToInt32(PharmacyFaculty.CollegeCode);
                if (PharmacyFaculty != null)
                {
                    db.jntuh_appeal_pharmacydata.Remove(PharmacyFaculty);
                    db.SaveChanges();
                    TempData["Success"] = PharmacyFaculty.Deficiency + "is Deleted Successfully";
                }
                return RedirectToAction("ViewAddedFaculty", new { collegeId = collegeid });
            }
            else
            {
                TempData["Error"] = "Data is Not Found";
                return RedirectToAction("ViewAddedFaculty");
            }
            return View();
        }

        [Authorize(Roles = "Admin,SuperAdmin,DataEntry,FacultyVerification")]
        public ActionResult DeleteAllAddedFaculty(int? collegeId, string Status)
        {
            if (Status == "DeleteAll")
            {
                int collegeID = Convert.ToInt32(collegeId);
                string collegeid = collegeId.ToString();
                var PharmacyFaculty = db.jntuh_appeal_pharmacydata.Where(e => e.CollegeCode == collegeid).Select(e => e).ToList();
                if (PharmacyFaculty.Count() > 0)
                {
                    db.jntuh_appeal_pharmacydata.Where(d => d.CollegeCode == collegeid).ToList().ForEach(d => db.jntuh_appeal_pharmacydata.Remove(d));
                    db.SaveChanges();
                }
                else
                {
                    TempData["Error"] = "Data is Not Found";
                    return RedirectToAction("ViewAddedFaculty", new { collegeId = collegeID });
                }
                TempData["Success"] = "Data is Deleted Successfully";
                return RedirectToAction("ViewAddedFaculty", new { collegeId = collegeID });
            }
            else
            {
                TempData["Error"] = "Data is Not Found";
                return RedirectToAction("ViewAddedFaculty");
            }
            return View();
        }

        public ActionResult GetGroups(int? DegreeId)
        {
            List<SelectListItem> Groups = new List<SelectListItem>();
            if (DegreeId != null)
            {

                Groups.Add(new SelectListItem { Text = "Group1", Value = "1" });
                Groups.Add(new SelectListItem { Text = "Group2", Value = "2" });
                Groups.Add(new SelectListItem { Text = "Group3", Value = "3" });
                Groups.Add(new SelectListItem { Text = "Group4", Value = "4" });
                ViewBag.Groups = Groups;
                return Json(new { data = Groups }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { data = Groups }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetSpecializations(int? DegreeId, int? collegeId)
        {

            var jntuh_college_intake_existing = db.jntuh_college_intake_existing.Where(e => e.collegeId == collegeId && e.academicYearId == 10 && e.courseStatus != "Closure").Select(e => e).ToList();
            int[] SpecializationIds = jntuh_college_intake_existing.Select(e => e.specializationId).Distinct().ToArray();
            var jntuh_degree = db.jntuh_degree.Where(e => e.isActive == true).ToList();
            var jntuh_department = db.jntuh_department.Where(e => e.isActive == true).ToList();
            var jntuh_specialization = db.jntuh_specialization.Where(e => e.isActive == true).ToList();

            var data = (from e in jntuh_college_intake_existing
                        join s in jntuh_specialization on e.specializationId equals s.id
                        join d in jntuh_department on s.departmentId equals d.id
                        join de in jntuh_degree on d.degreeId equals de.id
                        where SpecializationIds.Contains(s.id) && de.id == DegreeId
                        select new
                        {
                            id = s.id,
                            Specname = s.specializationName
                        }).Distinct().ToList();
            TempData["Required"] = "Empty";
            return Json(new { data = data }, JsonRequestBehavior.AllowGet);
        }

        public class PharmacyIntakeFaculty
        {
            public int? collegeId { get; set; }
            public int? DegreeId { get; set; }
            public string Department { get; set; }
            public string GroupId { get; set; }
            public int? SpecializationId { get; set; }
            public string Specialization { get; set; }
            public int? TotalIntake { get; set; }
            public int? ProposedIntake { get; set; }
            public int? TotalRequiredFaculty { get; set; }
            public int? SpecializationWiseFaculty { get; set; }
            public int? FacultyCount { get; set; }
        }

        #endregion

        [HttpGet]
        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification,DataEntry")]
        public ActionResult SCMFacultyCertitficatesVerification(int? rid)
        {
            //ViewBag.Colleges = db.jntuh_college.Join(db.jntuh_college_edit_status, Clg => Clg.id, Edit => Edit.collegeId, (Clg, Edit) => new { Clg = Clg, Edit = Edit }).Where(c => c.Clg.isActive == true && c.Edit.IsCollegeEditable==false).Select(c => new { collegeId = c.Clg.id, collegeName = c.Clg.collegeCode + "-" + c.Clg.collegeName }).OrderBy(c => c.collegeName).ToList();
            int[] CollegeIds = db.jntuh_college.Join(db.jntuh_college_edit_status, Clg => Clg.id, Edit => Edit.collegeId, (Clg, Edit) => new { Clg = Clg, Edit = Edit }).Where(c => c.Clg.isActive == true && c.Edit.IsCollegeEditable == false).Select(c => c.Clg.id).ToArray();
            ViewBag.Colleges = db.jntuh_college_randamcodes.Where(e => e.IsActive == true && CollegeIds.Contains(e.CollegeId)).Select(e => new { rid = e.Id, RandamCode = e.RandamCode }).OrderBy(e => e.RandamCode).ToList();


            var jntuh_specialization = db.jntuh_specialization.Where(e => e.isActive == true).Select(e => e).ToList();
            List<FacultyRegistration> teachingFaculty = new List<FacultyRegistration>();

            if (rid != null)
            {
                int collegeid = db.jntuh_college_randamcodes.Find(rid).CollegeId;
                List<jntuh_college_faculty_registered> jntuh_college_faculty_registered = db.jntuh_college_faculty_registered.Where(cf => cf.collegeId == collegeid).Select(cf => cf).ToList();
                string[] strRegNoS = jntuh_college_faculty_registered.Where(cf => cf.collegeId == collegeid).Select(cf => cf.RegistrationNumber).ToArray();

                List<jntuh_registered_faculty> jntuh_registered_faculty = new List<jntuh_registered_faculty>();

                // string[] SCMRequestRegNos =db.jntuh_auditors_dataentry.Where(e => e.CollegeId == collegeid).Select(e => e.RegistrationNo).Distinct().ToArray();

                jntuh_registered_faculty = db.jntuh_registered_faculty.Where(rf => strRegNoS.Contains(rf.RegistrationNumber) && rf.Blacklistfaculy != true && rf.isActive != false).ToList();


                int[] REGIDS = jntuh_registered_faculty.Select(F => F.id).ToArray();
                //  var regEducationids = db.jntuh_registered_faculty_education.AsNoTracking().Where(i => REGIDS.Contains(i.facultyId) && i.educationId == 6).Select(i => i.facultyId).ToArray();
                var FacultyEducation = db.jntuh_registered_faculty_education.Where(E => REGIDS.Contains(E.facultyId)).ToList();

                var DegreeData = (from Deg in db.jntuh_degree
                                  join Dept in db.jntuh_department on Deg.id equals Dept.degreeId
                                  join Spec in db.jntuh_specialization on Dept.id equals Spec.departmentId
                                  select new
                                  {
                                      Id = Spec.id,
                                      specName = Deg.degree + "-" + Spec.specializationName,
                                      DeptId = Dept.id
                                  }).ToList();



                var data = jntuh_registered_faculty.Select(a => new FacultyRegistration
                {
                    id = a.id,

                    Type = a.type,
                    CollegeId = collegeid,
                    RegistrationNumber = a.RegistrationNumber,
                    UniqueID = a.UniqueID,
                    FirstName = a.FirstName,
                    MiddleName = a.MiddleName,
                    LastName = a.LastName,
                    GenderId = a.GenderId,
                    Email = a.Email,
                    facultyPhoto = a.Photo,
                    Mobile = a.Mobile,
                    NORelevantUG = a.NoRelevantUG,
                    NORelevantPG = a.NoRelevantPG,
                    NORelevantPHD = a.NORelevantPHD,
                    PANNumber = a.PANNumber,
                    NOTQualifiedAsPerAICTE = a.NotQualifiedAsperAICTE ?? false,
                    XeroxcopyofcertificatesFlag = a.Xeroxcopyofcertificates ?? false,
                    //MultipleReginSamecoll = a.MultipleRegInSameCollege??false,
                    AadhaarNumber = a.AadhaarNumber,
                    TotalExperience = a.TotalExperience > 0 ? a.TotalExperience : 0,
                    facultyDateOfAppointment = UAAAS.Models.Utilities.MMDDYY2DDMMYY(a.DateOfAppointment.ToString()),
                    SelectionCommitteeProcedings = a.ProceedingDocument,
                    SSC = FacultyEducation.Where(E => E.educationId == 1 && E.facultyId == a.id).Select(E => E.certificate).FirstOrDefault(),
                    UG = FacultyEducation.Where(E => E.educationId == 3 && E.facultyId == a.id).Select(E => E.certificate).FirstOrDefault(),
                    PG = FacultyEducation.Where(E => E.educationId == 4 && E.facultyId == a.id).Select(E => E.certificate).FirstOrDefault(),
                    PHD = FacultyEducation.Where(E => E.educationId == 6 && E.facultyId == a.id).Select(E => E.certificate).FirstOrDefault(),
                    FacultyVerificationStatus = a.FacultyVerificationStatus,
                    InCompleteCeritificates = a.IncompleteCertificates != null ? (bool)a.IncompleteCertificates : false,
                    NotIdentityFiedForAnyProgramFlag = a.NotIdentityfiedForanyProgram ?? false,
                    PGSpecializationName = a.PGSpecialization != null ? jntuh_specialization.Where(e => e.id == a.PGSpecialization).Select(e => e.specializationName).FirstOrDefault() : "",
                    SpecializationIdentfiedFor = jntuh_college_faculty_registered.Where(e => e.RegistrationNumber.Trim() == a.RegistrationNumber.Trim()).Select(e => e.SpecializationId).FirstOrDefault() != null ? DegreeData.Where(s => s.Id == jntuh_college_faculty_registered.Where(e => e.RegistrationNumber.Trim() == a.RegistrationNumber.Trim()).Select(e => e.SpecializationId).FirstOrDefault()).Select(s => s.specName).FirstOrDefault() : "",
                    IdentfiedFor = jntuh_college_faculty_registered.Where(e => e.RegistrationNumber.Trim() == a.RegistrationNumber.Trim()).Select(e => e.IdentifiedFor).FirstOrDefault(),
                    DepartmentName = jntuh_college_faculty_registered.Where(e => e.RegistrationNumber.Trim() == a.RegistrationNumber.Trim()).Select(e => e.DepartmentId).FirstOrDefault() != null ? DegreeData.Where(s => s.DeptId == jntuh_college_faculty_registered.Where(e => e.RegistrationNumber.Trim() == a.RegistrationNumber.Trim()).Select(e => e.DepartmentId).FirstOrDefault()).Select(s => s.specName).FirstOrDefault() : ""

                }).ToList();

                teachingFaculty.AddRange(data);
                return View(teachingFaculty);
            }

            return View(teachingFaculty);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification,DataEntry")]
        public ActionResult SCMFacultyVerificationEdit(string fid, int collegeid)
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
                var departments = db.jntuh_department.Where(e => e.isActive == true).Select(e => e).ToList();
                var specializatons = db.jntuh_specialization.Where(e => e.isActive == true).Select(e => e).ToList();




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
                regFaculty.PGSpecialization = faculty.PGSpecialization;
                regFaculty.Absent = faculty.Absent ?? false;
                regFaculty.NoSCM = faculty.NoSCM ?? false;
                regFaculty.NOForm16 = faculty.NoForm16 ?? false;
                regFaculty.NOTQualifiedAsPerAICTE = faculty.NotQualifiedAsperAICTE ?? false;
                //regFaculty.MultipleReginSamecoll = faculty.MultipleRegInSameCollege ?? false;
                regFaculty.InCompleteCeritificates = faculty.IncompleteCertificates ?? false;
                regFaculty.BlacklistFlag = faculty.Blacklistfaculy ?? false;
                regFaculty.NOrelevantPgFlag = faculty.NoPhdUndertakingNew ?? false;
                regFaculty.NOspecializationFlag = faculty.NoSpecialization ?? false;
                regFaculty.XeroxcopyofcertificatesFlag = faculty.Xeroxcopyofcertificates ?? false;
                regFaculty.NotIdentityFiedForAnyProgramFlag = faculty.NotIdentityfiedForanyProgram ?? false;


                if (faculty.NoRelevantUG != "No")
                {
                    regFaculty.NOrelevantUgFlag = true;
                }
                if (faculty.NoRelevantPG != "No")
                {
                    regFaculty.NOrelevantPgFlag = true;
                }
                if (faculty.NORelevantPHD != "No")
                {
                    regFaculty.NOrelevantPhdFlag = true;
                }

                if (faculty.collegeId != null)
                {
                    regFaculty.CollegeName = db.jntuh_college.Find(faculty.collegeId).collegeName;
                }
                regFaculty.CollegeId = collegeid;
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
                regFaculty.OthersPGSpecilizationName = faculty.Others1;


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

                //   string registrationNumber = db.jntuh_registered_faculty.Where(of => of.id == fID).Select(of => of.RegistrationNumber).FirstOrDefault();
                //  int facultyId = db.jntuh_college_faculty_registered.Where(of => of.RegistrationNumber == registrationNumber).Select(of => of.id).FirstOrDefault();
                // int[] verificationOfficers = db.jntuh_college_faculty_verified.Where(v => v.FacultyId == facultyId).Select(v => v.VerificationOfficer).Distinct().ToArray();
                // int userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);

                int[] ugids = departments.Where(i => i.degreeId == 4 || i.degreeId == 5).Select(i => i.id).ToArray();
                int[] pgids = departments.Where(i => i.degreeId != 4 && i.degreeId != 5).Select(i => i.id).ToArray();
                //List<DistinctDepartment> depts = new List<DistinctDepartment>();
                //string existingDepts = string.Empty;
                //int[] notRequiredIds = { 25, 26, 27, 33, 34, 36, 37, 38, 39, 53, 54, 55, 56 };
                //foreach (var item in db.jntuh_department.Where(s => !notRequiredIds.Contains(s.id)).OrderBy(s => s.departmentName))
                //{
                //    if (!existingDepts.Split(',').Contains(item.departmentName))
                //    {
                //        depts.Add(new DistinctDepartment { id = item.id, departmentName = item.departmentName });
                //        existingDepts = existingDepts + "," + item.departmentName;
                //    }
                //}

                //ViewBag.department = depts;

                var ugcoures = specializatons.Where(i => ugids.Contains(i.departmentId)).ToList();
                var pgcoures = specializatons.Where(i => pgids.Contains(i.departmentId) || i.id == 154).ToList();
                //var phdcoures = db.jntuh_specialization.Where(i => phdids.Contains(i.departmentId)).Select(i => i.specializationName).ToList();

                ViewBag.ugcourses = ugcoures;
                ViewBag.pgcourses = pgcoures;
                //ViewBag.phdcourses = phdcoures;
                ViewBag.FacultyDetails = regFaculty;
                TempData["FacultyDetails"] = regFaculty;
                ViewBag.HideVerifyLink = regFaculty.isApproved != null ? true : false;
            }
            return PartialView("_SCMFacultyVerificationEdit", regFaculty);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification,DataEntry")]
        public ActionResult SCMFacultyVerificationPostEdit(FacultyRegistration faculty)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            var facultydetails = db.jntuh_registered_faculty.FirstOrDefault(i => i.RegistrationNumber == faculty.RegistrationNumber);
            var rid = db.jntuh_college_randamcodes.Where(e => e.IsActive == true && e.CollegeId == faculty.CollegeId).Select(e => e.Id).FirstOrDefault();
            if (facultydetails != null)
            {
                //if (faculty.Absent == true)
                //    facultydetails.Absent = faculty.Absent;
                if (faculty.NOrelevantUgFlag == true)
                    facultydetails.NoRelevantUG = "Yes";
                if (faculty.NOrelevantPgFlag == true)
                    facultydetails.NoRelevantPG = "Yes";
                if (faculty.NOrelevantPhdFlag == true)
                    facultydetails.NORelevantPHD = "Yes";
                //if (faculty.PGSpecialization != null)
                //{

                //    if (faculty.PGSpecialization != 154)
                //    {
                //        facultydetails.PGSpecialization = faculty.PGSpecialization;
                //    }
                //    else
                //    {
                //        facultydetails.PGSpecialization = faculty.PGSpecialization;
                //        facultydetails.Others1 = faculty.OthersPGSpecilizationName;
                //    }
                //}

                //if (faculty.NoSCM == true)
                //    facultydetails.NoSCM = faculty.NoSCM;
                //if (faculty.NOForm16 == true)//No form16
                //    facultydetails.NoForm16 = faculty.NOForm16;
                if (faculty.InCompleteCeritificates == true)//Orgs Certificates not Shown
                    facultydetails.IncompleteCertificates = faculty.InCompleteCeritificates;
                if (faculty.NOTQualifiedAsPerAICTE == true)
                    facultydetails.NotQualifiedAsperAICTE = faculty.NOTQualifiedAsPerAICTE;
                if (faculty.MultipleReginSamecoll == true)
                    //facultydetails.MultipleRegInSameCollege = faculty.MultipleReginSamecoll;
                    if (faculty.XeroxcopyofcertificatesFlag == true)
                        facultydetails.Xeroxcopyofcertificates = faculty.XeroxcopyofcertificatesFlag;
                if (faculty.NotIdentityFiedForAnyProgramFlag == true)
                    facultydetails.NotIdentityfiedForanyProgram = faculty.NotIdentityFiedForAnyProgramFlag;
                //if (faculty.BlacklistFlag == true)
                //    facultydetails.NoSpecialization = faculty.NOspecializationFlag;
                //if (faculty.NophdUndertakingFlag == true)
                //    facultydetails.NoPhdUndertakingNew = faculty.NophdUndertakingFlag;

                #region oldCode
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

                //facultydetails.NotQualifiedAsperAICTE = faculty.NOTQualifiedAsPerAICTE;
                //
                //facultydetails.MultipleRegInDiffCollege = faculty.MultipleReginDiffcoll;
                //facultydetails.SamePANUsedByMultipleFaculty = faculty.SamePANUsedByMultipleFaculty;
                //facultydetails.PhotoCopyofPAN = faculty.PhotocopyofPAN;
                //facultydetails.AppliedPAN = faculty.AppliedPAN;
                //facultydetails.LostPAN = faculty.LostPAN;
                //facultydetails.OriginalsVerifiedUG = faculty.OriginalsVerifiedUG;
                //facultydetails.OriginalsVerifiedPG = faculty.OriginalsVerifiedPG;
                //facultydetails.OriginalsVerifiedPHD = faculty.OriginalsVerifiedPHD;
                #endregion




                facultydetails.FacultyVerificationStatus = true;
                facultydetails.DeactivatedBy = userID;
                facultydetails.DeactivatedOn = DateTime.Now;
                db.Entry(facultydetails).State = EntityState.Modified;
                db.SaveChanges();

            }

            return RedirectToAction("SCMFacultyCertitficatesVerification", "FacultyVerificationDENew", new { rid = rid });
        }


        [HttpGet]
        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification,DataEntry")]
        public ActionResult AddPGSpecialization(string fid, int collegeId)
        {
            var faculty = new CollegeFaculty();
            int fID = 0;
            if (fid != null)
            {
                fID = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(fid, WebConfigurationManager.AppSettings["CryptoKey"]));

                var existingfaculty = db.jntuh_registered_faculty.Find(fID); //&& i.collegeId == collegeId
                if (existingfaculty != null)
                {
                    faculty.collegeId = collegeId;
                    faculty.id = existingfaculty.id;
                    faculty.facultyFirstName = existingfaculty.FirstName;
                    faculty.facultyLastName = existingfaculty.LastName;
                    faculty.facultySurname = existingfaculty.MiddleName;
                    faculty.facultyDesignationId = existingfaculty.DesignationId;
                    faculty.facultyOtherDesignation = existingfaculty.OtherDesignation;
                    faculty.FacultyRegistrationNumber = existingfaculty.RegistrationNumber;
                    faculty.SpecializationId = existingfaculty.PGSpecialization;
                    faculty.SpecializationName = existingfaculty.PGSpecializationRemarks;
                }
            }
            int[] pgids = db.jntuh_department.Where(i => i.degreeId != 4 && i.degreeId != 5).Select(i => i.id).ToArray();
            var pgcoures = db.jntuh_specialization.Where(i => pgids.Contains(i.departmentId) || i.id == 154).ToList();
            ViewBag.PGSpecializations = pgcoures;

            return PartialView("_AddPGSpecialization", faculty);
        }

        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification,DataEntry")]
        [HttpPost]
        public ActionResult AddPGSpecializationPost(CollegeFaculty faculty)
        {
            TempData["Error"] = null;
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int rid = db.jntuh_college_randamcodes.Where(e => e.CollegeId == faculty.collegeId).Select(e => e.Id).FirstOrDefault();
            //var isRegisteredFaculty = db.jntuh_registered_faculty.Where(r => r.RegistrationNumber == faculty.FacultyRegistrationNumber).Select(r => r).FirstOrDefault();
            var isExistingFaculty = db.jntuh_registered_faculty.FirstOrDefault(r => r.RegistrationNumber == faculty.FacultyRegistrationNumber);
            if (isExistingFaculty != null)
            {
                isExistingFaculty.PGSpecialization = faculty.SpecializationId;
                if (faculty.SpecializationId == 154)
                {
                    isExistingFaculty.PGSpecializationRemarks = faculty.SpecializationName;
                }

                isExistingFaculty.updatedBy = userID;
                isExistingFaculty.updatedOn = DateTime.Now;
                db.Entry(isExistingFaculty).State = EntityState.Modified;
                db.SaveChanges();
                TempData["Success"] = "Faculty Specialization (" + faculty.FacultyRegistrationNumber + " ) Successfully Updated ..";
                TempData["Error"] = null;
            }

            return RedirectToAction("SCMFacultyCertitficatesVerification", "FacultyVerificationDENew", new { rid = rid });
        }


        [HttpGet]
        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification,DataEntry")]
        public ActionResult PhramcyAddPGSpecialization(int? rid)
        {

            var PhramcyCollegeIds = (from a in db.jntuh_college_intake_existing
                                     join b in db.jntuh_college_edit_status on a.collegeId equals b.collegeId
                                     join c in db.jntuh_specialization on a.specializationId equals c.id
                                     join d in db.jntuh_department on c.departmentId equals d.id
                                     join e in db.jntuh_degree on d.degreeId equals e.id
                                     where b.IsCollegeEditable == false && a.academicYearId == 9 && e.id == 5
                                     select a.collegeId).GroupBy(e => e).Select(e => e.Key).ToArray();



            int[] CollegeIds = db.jntuh_college.Join(db.jntuh_college_edit_status, Clg => Clg.id, Edit => Edit.collegeId, (Clg, Edit) => new { Clg = Clg, Edit = Edit }).Where(c => c.Clg.isActive == true && c.Edit.IsCollegeEditable == false).Select(c => c.Clg.id).ToArray();
            ViewBag.Colleges = db.jntuh_college_randamcodes.Where(e => e.IsActive == true && CollegeIds.Contains(e.CollegeId) && PhramcyCollegeIds.Contains(e.CollegeId)).Select(e => new { rid = e.Id, RandamCode = e.RandamCode }).OrderBy(e => e.RandamCode).ToList();


            var jntuh_specialization = db.jntuh_specialization.Where(e => e.isActive == true).Select(e => e).ToList();
            List<FacultyRegistration> teachingFaculty = new List<FacultyRegistration>();

            if (rid != null)
            {
                int collegeid = db.jntuh_college_randamcodes.Find(rid).CollegeId;
                List<jntuh_college_faculty_registered> jntuh_college_faculty_registered = db.jntuh_college_faculty_registered.Where(cf => cf.collegeId == collegeid).Select(cf => cf).ToList();
                string[] strRegNoS = jntuh_college_faculty_registered.Where(cf => cf.collegeId == collegeid).Select(cf => cf.RegistrationNumber).ToArray();

                List<jntuh_registered_faculty> jntuh_registered_faculty = new List<jntuh_registered_faculty>();

                //  string[] SCMRequestRegNos = db.jntuh_auditors_dataentry.Where(e => e.CollegeId == collegeid).Select(e => e.RegistrationNo).Distinct().ToArray();

                jntuh_registered_faculty = db.jntuh_registered_faculty.Where(rf => strRegNoS.Contains(rf.RegistrationNumber) && rf.Blacklistfaculy != true && rf.isActive != false).ToList();


                int[] REGIDS = jntuh_registered_faculty.Select(F => F.id).ToArray();
                //  var regEducationids = db.jntuh_registered_faculty_education.AsNoTracking().Where(i => REGIDS.Contains(i.facultyId) && i.educationId == 6).Select(i => i.facultyId).ToArray();
                var FacultyEducation = db.jntuh_registered_faculty_education.Where(E => REGIDS.Contains(E.facultyId)).ToList();

                var DegreeData = (from Deg in db.jntuh_degree
                                  join Dept in db.jntuh_department on Deg.id equals Dept.degreeId
                                  join Spec in db.jntuh_specialization on Dept.id equals Spec.departmentId
                                  select new
                                  {
                                      Id = Spec.id,
                                      specName = Deg.degree + "-" + Spec.specializationName,
                                      DeptId = Dept.id
                                  }).ToList();



                var data = jntuh_registered_faculty.Select(a => new FacultyRegistration
                {
                    id = a.id,

                    Type = a.type,
                    CollegeId = collegeid,
                    RegistrationNumber = a.RegistrationNumber,
                    UniqueID = a.UniqueID,
                    FirstName = a.FirstName,
                    MiddleName = a.MiddleName,
                    LastName = a.LastName,
                    GenderId = a.GenderId,
                    Email = a.Email,
                    facultyPhoto = a.Photo,
                    Mobile = a.Mobile,
                    NORelevantUG = a.NoRelevantUG,
                    NORelevantPG = a.NoRelevantPG,
                    NORelevantPHD = a.NORelevantPHD,
                    PANNumber = a.PANNumber,
                    NOTQualifiedAsPerAICTE = a.NotQualifiedAsperAICTE ?? false,
                    XeroxcopyofcertificatesFlag = a.Xeroxcopyofcertificates ?? false,
                    //MultipleReginSamecoll = a.MultipleRegInSameCollege ?? false,
                    AadhaarNumber = a.AadhaarNumber,
                    TotalExperience = a.TotalExperience > 0 ? a.TotalExperience : 0,
                    facultyDateOfAppointment = UAAAS.Models.Utilities.MMDDYY2DDMMYY(a.DateOfAppointment.ToString()),
                    SelectionCommitteeProcedings = a.ProceedingDocument,
                    SSC = FacultyEducation.Where(E => E.educationId == 1 && E.facultyId == a.id).Select(E => E.certificate).FirstOrDefault(),
                    UG = FacultyEducation.Where(E => E.educationId == 3 && E.facultyId == a.id).Select(E => E.certificate).FirstOrDefault(),
                    PG = FacultyEducation.Where(E => E.educationId == 4 && E.facultyId == a.id).Select(E => E.certificate).FirstOrDefault(),
                    PHD = FacultyEducation.Where(E => E.educationId == 5 && E.facultyId == a.id).Select(E => E.certificate).FirstOrDefault(),
                    FacultyVerificationStatus = a.FacultyVerificationStatus,
                    InCompleteCeritificates = a.IncompleteCertificates != null ? (bool)a.IncompleteCertificates : false,
                    NotIdentityFiedForAnyProgramFlag = a.NotIdentityfiedForanyProgram ?? false,
                    PGSpecializationName = a.PGSpecialization != null ? jntuh_specialization.Where(e => e.id == a.PGSpecialization).Select(e => e.specializationName).FirstOrDefault() : "",
                    SpecializationIdentfiedFor = jntuh_college_faculty_registered.Where(e => e.RegistrationNumber.Trim() == a.RegistrationNumber.Trim()).Select(e => e.SpecializationId).FirstOrDefault() != null ? DegreeData.Where(s => s.Id == jntuh_college_faculty_registered.Where(e => e.RegistrationNumber.Trim() == a.RegistrationNumber.Trim()).Select(e => e.SpecializationId).FirstOrDefault()).Select(s => s.specName).FirstOrDefault() : "",
                    IdentfiedFor = jntuh_college_faculty_registered.Where(e => e.RegistrationNumber.Trim() == a.RegistrationNumber.Trim()).Select(e => e.IdentifiedFor).FirstOrDefault(),
                    DepartmentName = jntuh_college_faculty_registered.Where(e => e.RegistrationNumber.Trim() == a.RegistrationNumber.Trim()).Select(e => e.DepartmentId).FirstOrDefault() != null ? DegreeData.Where(s => s.DeptId == jntuh_college_faculty_registered.Where(e => e.RegistrationNumber.Trim() == a.RegistrationNumber.Trim()).Select(e => e.DepartmentId).FirstOrDefault()).Select(s => s.specName).FirstOrDefault() : ""

                }).ToList();

                teachingFaculty.AddRange(data);
                return View(teachingFaculty);
            }

            return View(teachingFaculty);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification,DataEntry")]
        public ActionResult PharmacyAddPGSpecialization(string fid, int collegeId)
        {
            var faculty = new CollegeFaculty();
            int fID = 0;
            if (fid != null)
            {
                fID = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(fid, WebConfigurationManager.AppSettings["CryptoKey"]));

                var existingfaculty = db.jntuh_registered_faculty.Find(fID); //&& i.collegeId == collegeId
                if (existingfaculty != null)
                {
                    faculty.collegeId = collegeId;
                    faculty.id = existingfaculty.id;
                    faculty.facultyFirstName = existingfaculty.FirstName;
                    faculty.facultyLastName = existingfaculty.LastName;
                    faculty.facultySurname = existingfaculty.MiddleName;
                    faculty.facultyDesignationId = existingfaculty.DesignationId;
                    faculty.facultyOtherDesignation = existingfaculty.OtherDesignation;
                    faculty.FacultyRegistrationNumber = existingfaculty.RegistrationNumber;
                    faculty.SpecializationId = existingfaculty.PGSpecialization;
                    faculty.SpecializationName = existingfaculty.PGSpecializationRemarks;
                }
            }
            int[] pgids = db.jntuh_department.Where(i => i.degreeId != 4 && i.degreeId != 5).Select(i => i.id).ToArray();
            var pgcoures = db.jntuh_specialization.Where(i => pgids.Contains(i.departmentId) || i.id == 154).ToList();
            ViewBag.PGSpecializations = pgcoures;

            return PartialView("_PharmacyAddPGSpecialization", faculty);
        }

        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification,DataEntry")]
        [HttpPost]
        public ActionResult PharmacyAddPGSpecializationPost(CollegeFaculty faculty)
        {
            TempData["Error"] = null;
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int rid = db.jntuh_college_randamcodes.Where(e => e.CollegeId == faculty.collegeId).Select(e => e.Id).FirstOrDefault();
            //var isRegisteredFaculty = db.jntuh_registered_faculty.Where(r => r.RegistrationNumber == faculty.FacultyRegistrationNumber).Select(r => r).FirstOrDefault();
            var isExistingFaculty = db.jntuh_registered_faculty.FirstOrDefault(r => r.RegistrationNumber == faculty.FacultyRegistrationNumber);
            if (isExistingFaculty != null)
            {
                isExistingFaculty.PGSpecialization = faculty.SpecializationId;
                if (faculty.SpecializationId == 154)
                {
                    isExistingFaculty.PGSpecializationRemarks = faculty.SpecializationName;
                }

                isExistingFaculty.updatedBy = userID;
                isExistingFaculty.updatedOn = DateTime.Now;
                db.Entry(isExistingFaculty).State = EntityState.Modified;
                db.SaveChanges();
                TempData["Success"] = "Faculty Specialization (" + faculty.FacultyRegistrationNumber + " ) Successfully Updated ..";
                TempData["Error"] = null;
            }

            return RedirectToAction("PhramcyAddPGSpecialization", "FacultyVerificationDENew", new { rid = rid });
        }

        #region Faculty Flags VerificationView

        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification")]
        public ActionResult FacultyFlagsVerificationView(int? collegeid)
        {
            //ViewBag.Colleges = db.jntuh_college.Join(db.jntuh_college_edit_status, co => co.id, e => e.collegeId, (co, e) => new { co = co, e = e }).Where(c => c.co.isActive == true && c.e.IsCollegeEditable == false).Select(c => new { collegeId = c.co.id, collegeName = c.co.collegeCode + "-" + c.co.collegeName }).OrderBy(c => c.collegeName).ToList();
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy =
                db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            ViewBag.Colleges = db.jntuh_college.Join(db.jntuh_college_edit_status, co => co.id, e => e.collegeId, (co, e) => new { co = co, e = e }).Where(c => c.co.isActive == true && c.e.academicyearId == prAy).Select(c => new { collegeId = c.co.id, collegeName = c.co.collegeCode + "-" + c.co.collegeName }).OrderBy(c => c.collegeName).ToList();
            ViewBag.collegeid = collegeid;
            var jntuh_department = db.jntuh_department.Where(e => e.isActive == true).ToList();
            var jntuh_specialization = db.jntuh_specialization.Where(s => s.isActive == true).ToList();
            List<FacultyRegistration> teachingFaculty = new List<FacultyRegistration>();

            if (collegeid != null)
            {

                var DegreeIdNameBasedOnSpecialization = (from a in db.jntuh_department
                                                         join b in db.jntuh_specialization on a.id equals b.departmentId
                                                         join c in db.jntuh_degree on a.degreeId equals c.id
                                                         select new
                                                         {
                                                             DegreeId = c.id,
                                                             DegreeName = c.degree,
                                                             SpcializationName = b.specializationName,
                                                             Specid = b.id
                                                         }).ToList();


                //string[] Copy_RegNos = db.jntuh_college_faculty_registered_copy.Where(c => c.collegeId == 375).Select(e => e.RegistrationNumber).Distinct().ToArray();
                // List<jntuh_college_faculty_registered> jntuh_college_faculty_registered_College_Portal = db.jntuh_college_faculty_registered.Where(cf => cf.collegeId == collegeid).Select(cf => cf).ToList();

                List<jntuh_college_faculty_registered> jntuh_college_faculty_registered = db.jntuh_college_faculty_registered.Where(cf => cf.collegeId == collegeid).Select(cf => cf).ToList();

                // jntuh_college_faculty_registered = jntuh_college_faculty_registered.Where(c => c.RegistrationNumber).ToList();

                //  List<jntuh_college_faculty_registered> jntuh_college_faculty_registered = new List<Models.jntuh_college_faculty_registered>();

                // jntuh_college_faculty_registered.AddRange(Faculty_Registered_copy_data);
                //  jntuh_college_faculty_registered.AddRange(jntuh_college_faculty_registered_College_Portal);

                // jntuh_college_faculty_registered = jntuh_college_faculty_registered.GroupBy(s => new { s.RegistrationNumber }).Select(s => s.First()).ToList();

                string[] strRegNoS = jntuh_college_faculty_registered.Select(cf => cf.RegistrationNumber).ToArray();
                string[] PrincipalstrRegNoS = db.jntuh_college_principal_registered.Where(P => P.collegeId == collegeid).Select(P => P.RegistrationNumber.Trim()).ToArray();
                List<jntuh_registered_faculty> jntuh_registered_faculty = new List<jntuh_registered_faculty>();
                jntuh_registered_faculty = db.jntuh_registered_faculty.Where(rf => strRegNoS.Contains(rf.RegistrationNumber))  //&& (&& rf.Blacklistfaculy != truerf.collegeId == null || rf.collegeId == collegeid)//&& rf.Notin116 != true
                                             .ToList();

                var Specializations = db.jntuh_specialization.AsNoTracking().ToList();
                var jntuh_phdfaculty = db.jntuh_faculty_phddetails.AsNoTracking().ToList();

                string RegNumber = "";
                int? Specializationid = 0;
                int? CollegeDepartmentId = 0;
                foreach (var a in jntuh_registered_faculty)
                {
                    string Reason = String.Empty;
                    Specializationid = jntuh_college_faculty_registered.Where(C => C.RegistrationNumber.Trim() == a.RegistrationNumber.Trim()).Select(C => C.SpecializationId).FirstOrDefault();
                    //a.PGSpecialization = jntuh_college_faculty_registered.Where(C => C.RegistrationNumber.Trim() == a.RegistrationNumber.Trim()).Select(C => C.FacultySpecializationId).FirstOrDefault();
                    CollegeDepartmentId = jntuh_college_faculty_registered.Where(C => C.RegistrationNumber.Trim() == a.RegistrationNumber.Trim()).Select(C => C.DepartmentId).FirstOrDefault();
                    var faculty = new FacultyRegistration();
                    faculty.id = a.id;
                    faculty.Type = a.type;
                    faculty.CollegeId = collegeid;
                    faculty.RegistrationNumber = a.RegistrationNumber;
                    faculty.UniqueID = a.UniqueID;
                    faculty.FirstName = a.FirstName;
                    faculty.MiddleName = a.MiddleName;
                    faculty.LastName = a.LastName;
                    //faculty.Basstatus = a.InvalidAadhaar;
                    if (PrincipalstrRegNoS.Contains(a.RegistrationNumber.Trim()))
                        faculty.Principal = "Principal";
                    else
                        faculty.Principal = "";

                    faculty.DepartmentId = jntuh_college_faculty_registered.Where(e => e.RegistrationNumber == a.RegistrationNumber).Select(e => e.DepartmentId).FirstOrDefault() == null ? null : jntuh_college_faculty_registered.Where(e => e.RegistrationNumber == a.RegistrationNumber).Select(e => e.DepartmentId).FirstOrDefault();
                    faculty.SpecializationId = jntuh_college_faculty_registered.Where(e => e.RegistrationNumber == a.RegistrationNumber).Select(e => e.SpecializationId).FirstOrDefault() == null ? null : jntuh_college_faculty_registered.Where(e => e.RegistrationNumber == a.RegistrationNumber).Select(e => e.SpecializationId).FirstOrDefault();
                    faculty.SpecializationName = jntuh_specialization.Where(e => e.id == faculty.SpecializationId).Select(e => e.specializationName).FirstOrDefault() == null ? null : jntuh_specialization.Where(e => e.id == faculty.SpecializationId).Select(e => e.specializationName).FirstOrDefault();
                    //faculty.PGSpecialization = jntuh_college_faculty_registered.Where(e => e.RegistrationNumber == a.RegistrationNumber).Select(e => e.FacultySpecializationId).FirstOrDefault() == null ? null : jntuh_college_faculty_registered.Where(e => e.RegistrationNumber == a.RegistrationNumber).Select(e => e.FacultySpecializationId).FirstOrDefault();
                    faculty.PGSpecialization = a.Jntu_PGSpecializationId == null ? null : a.Jntu_PGSpecializationId;
                    faculty.PGSpecializationName = faculty.PGSpecialization != 0 ? DegreeIdNameBasedOnSpecialization.Where(e => e.Specid == faculty.PGSpecialization).Select(e => e.DegreeName + "-" + e.SpcializationName).FirstOrDefault() : "";
                    faculty.GenderId = a.GenderId;
                    faculty.Email = a.Email;
                    faculty.facultyPhoto = a.Photo;
                    faculty.Mobile = a.Mobile;
                    faculty.PANNumber = a.PANNumber;
                    //faculty.AadhaarNumber = a.AadhaarNumber;
                    faculty.isActive = a.isActive;
                    faculty.isApproved = a.isApproved;
                    faculty.department = CollegeDepartmentId > 0 ? jntuh_department.Where(d => d.id == CollegeDepartmentId).Select(d => d.departmentName).FirstOrDefault() : "";
                    faculty.SamePANNumberCount = jntuh_registered_faculty.Where(f => f.PANNumber == a.PANNumber).ToList().Count;
                    faculty.SameAadhaarNumberCount = jntuh_registered_faculty.Where(f => f.AadhaarNumber == a.AadhaarNumber).ToList().Count;
                    faculty.SpecializationIdentfiedFor = Specializationid > 0 ? DegreeIdNameBasedOnSpecialization.Where(S => S.Specid == Specializationid).Select(S => S.DegreeName + "-" + S.SpcializationName).FirstOrDefault() : "";
                    faculty.isVerified = isFacultyVerified(a.id);
                    faculty.DeactivationReason = a.DeactivationReason;
                    faculty.FacultyVerificationStatus = a.FacultyVerificationStatus;
                    faculty.updatedOn = a.updatedOn;
                    faculty.createdOn = jntuh_college_faculty_registered.Where(f => f.RegistrationNumber == a.RegistrationNumber).Select(f => f.createdOn).FirstOrDefault();
                    faculty.IdentfiedFor = jntuh_college_faculty_registered.Where(f => f.RegistrationNumber.Trim() == a.RegistrationNumber.Trim()).Select(f => f.IdentifiedFor).FirstOrDefault();
                    faculty.jntuh_registered_faculty_education = a.jntuh_registered_faculty_education;
                    faculty.DegreeId = a.jntuh_registered_faculty_education.Where(r => r.educationId != 8).Count(e => e.facultyId == a.id) > 0 ? a.jntuh_registered_faculty_education.Where(e => e.facultyId == a.id && e.educationId != 8).Select(e => e.educationId).Max() : 0;
                    faculty.PanDeactivationReasion = !string.IsNullOrEmpty(a.PanDeactivationReason) ? a.PanDeactivationReason : "";

                    faculty.Absent = a.Absent != null ? (bool)a.Absent : false;
                    faculty.BlacklistFaculty = a.Blacklistfaculy != null ? (bool)a.Blacklistfaculy : false;
                    faculty.PHDundertakingnotsubmitted = a.PHDundertakingnotsubmitted != null ? (bool)a.PHDundertakingnotsubmitted : false;
                    faculty.NOTQualifiedAsPerAICTE = a.NotQualifiedAsperAICTE != null ? (bool)a.NotQualifiedAsperAICTE : false;
                    faculty.InvalidPANNo = a.InvalidPANNumber != null ? (bool)a.InvalidPANNumber : false;
                    faculty.InCompleteCeritificates = a.IncompleteCertificates != null ? (bool)a.IncompleteCertificates : false;
                    faculty.OriginalCertificatesnotshownFlag = a.OriginalCertificatesNotShown != null ? (bool)a.OriginalCertificatesNotShown : false;
                    faculty.FalsePAN = a.FalsePAN != null ? (bool)a.FalsePAN : false;
                    faculty.NOForm16 = a.NoForm16 != null ? (bool)a.NoForm16 : false;
                    //faculty.MultipleReginSamecoll = a.MultipleRegInSameCollege != null ? (bool)a.MultipleRegInSameCollege : false;
                    faculty.XeroxcopyofcertificatesFlag = a.Xeroxcopyofcertificates != null ? (bool)a.Xeroxcopyofcertificates : false;
                    faculty.NotIdentityFiedForAnyProgramFlag = a.NotIdentityfiedForanyProgram != null ? (bool)a.NotIdentityfiedForanyProgram : false;
                    faculty.NOrelevantUgFlag = a.NoRelevantUG == "Yes" ? true : false;
                    faculty.NOrelevantPgFlag = a.NoRelevantPG == "Yes" ? true : false;
                    faculty.NOrelevantPhdFlag = a.NORelevantPHD == "Yes" ? true : false;
                    //faculty.NoForm16Verification = a.Noform16Verification != null ? (bool)a.Noform16Verification : false;
                    faculty.NoSCM17Flag = a.NoSCM != null ? (bool)a.NoSCM : false;
                    faculty.NoSCM = a.NoSCM != null ? (bool)a.NoSCM : false;
                    //faculty.PhotocopyofPAN = a.PhotoCopyofPAN != null ? (bool)a.PhotoCopyofPAN : false;
                    faculty.PhdUndertakingDocumentstatus = a.PhdUndertakingDocumentstatus != null ? (bool)(a.PhdUndertakingDocumentstatus) : false;
                    faculty.PHDUndertakingDocumentView = a.PHDUndertakingDocument;
                    faculty.PhdUndertakingDocumentText = a.PhdUndertakingDocumentText;
                    //faculty.AppliedPAN = a.AppliedPAN != null ? (bool)(a.AppliedPAN) : false;
                    //faculty.SamePANUsedByMultipleFaculty = a.SamePANUsedByMultipleFaculty != null ? (bool)(a.SamePANUsedByMultipleFaculty) : false;
                    faculty.Basstatus = a.BAS;
                    //Basstatus Column Consider as Aadhaar Flag 
                    faculty.InvalidAadhaar = a.InvalidAadhaar;
                    if (faculty.InvalidAadhaar == "Yes")
                    {
                        faculty.AadhaarNumber = jntuh_college_faculty_registered.Where(e => e.RegistrationNumber == a.RegistrationNumber).Select(e => e.AadhaarNumber).FirstOrDefault() == null ? null : jntuh_college_faculty_registered.Where(e => e.RegistrationNumber == a.RegistrationNumber).Select(e => e.AadhaarNumber).FirstOrDefault();
                    }
                    faculty.Deactivedby = a.DeactivatedBy;
                    faculty.DeactivedOn = a.DeactivatedOn;
                    faculty.SCMDocumentView =
                        db.jntuh_scmupload.Where(u => u.RegistrationNumber.Trim() == faculty.RegistrationNumber.Trim() && u.DepartmentId != 0)
                            .Select(s => s.SCMDocument)
                            .FirstOrDefault();
                    faculty.OriginalsVerifiedPHD = a.OriginalsVerifiedPHD == true ? true : false;
                    faculty.OriginalsVerifiedUG = a.OriginalsVerifiedUG == true ? true : false;
                    //faculty.BlacklistFlag = a.Blacklistfaculy == true ? true : false;
                    faculty.VerificationStatus = a.AbsentforVerification != null ? (bool)a.AbsentforVerification : false;
                    faculty.Phd2pages = jntuh_phdfaculty.Where(i => i.Facultyid == a.id).Count() > 0 ? true : false;
                    faculty.NoClass = a.Noclass == true ? true : false;
                    faculty.InvalidDegree = a.Invaliddegree == true ? true : false;
                    faculty.GenuinenessnotSubmitted = a.Genuinenessnotsubmitted == true ? true : false;
                    faculty.FakePhd = a.FakePHD == true ? true : false;
                    faculty.NotconsiderPhd = a.NotconsideredPHD == true ? true : false;
                    faculty.NoPgSpecialization = a.NoPGspecialization == true ? true : false;
                    faculty.Maternity = a.Maternity == true ? true : false;
                    faculty.Covid19 = a.Covid19 == true ? true : false;
                    faculty.NOForm26AS = a.NoForm26AS == true ? true : false;
                    teachingFaculty.Add(faculty);
                }
                teachingFaculty = teachingFaculty.Where(m => m.isActive == true).OrderBy(f => f.updatedOn).ThenBy(f => f.department).ToList();
                //|| item.NoForm16Verification == true ||item.NOForm16 == true ||
                ViewBag.TotalFaculty = teachingFaculty.Count();
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
                ViewBag.ClearFaculty = ViewBag.TotalFaculty - ViewBag.FlagTotalFaculty;
                return View(teachingFaculty);
            }

            return View(teachingFaculty);
        }

        public ActionResult FacultyFlagsVerificationExport(int collegeid)
        {
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy =
                db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            ViewBag.Colleges = db.jntuh_college.Join(db.jntuh_college_edit_status, co => co.id, e => e.collegeId, (co, e) => new { co = co, e = e }).Where(c => c.co.isActive == true && c.e.academicyearId == prAy).Select(c => new { collegeId = c.co.id, collegeName = c.co.collegeCode + "-" + c.co.collegeName }).OrderBy(c => c.collegeName).ToList();
            ViewBag.collegeid = collegeid;
            var jntuh_department = db.jntuh_department.Where(e => e.isActive == true).ToList();
            var jntuh_specialization = db.jntuh_specialization.Where(s => s.isActive == true).ToList();

            string collegeCode = db.jntuh_college.Where(c => c.id == collegeid).Select(c => c.collegeCode).FirstOrDefault();

            List<FacultyRegistration> teachingFaculty = new List<FacultyRegistration>();

            var DegreeIdNameBasedOnSpecialization = (from a in db.jntuh_department
                                                     join b in db.jntuh_specialization on a.id equals b.departmentId
                                                     join c in db.jntuh_degree on a.degreeId equals c.id
                                                     select new
                                                     {
                                                         DegreeId = c.id,
                                                         DegreeName = c.degree,
                                                         SpcializationName = b.specializationName,
                                                         Specid = b.id
                                                     }).ToList();

            List<jntuh_college_faculty_registered> jntuh_college_faculty_registered = db.jntuh_college_faculty_registered.Where(cf => cf.collegeId == collegeid).Select(cf => cf).ToList();

            string[] strRegNoS = jntuh_college_faculty_registered.Select(cf => cf.RegistrationNumber).ToArray();
            string[] PrincipalstrRegNoS = db.jntuh_college_principal_registered.Where(P => P.collegeId == collegeid).Select(P => P.RegistrationNumber.Trim()).ToArray();
            List<jntuh_registered_faculty> jntuh_registered_faculty = new List<jntuh_registered_faculty>();
            jntuh_registered_faculty = db.jntuh_registered_faculty.Where(rf => strRegNoS.Contains(rf.RegistrationNumber))
                                         .ToList();

            //var Specializations = db.jntuh_specialization.ToList();
            var jntuh_phdfaculty = db.jntuh_faculty_phddetails.AsNoTracking().ToList();
            string RegNumber = "";
            int? Specializationid = 0;
            int? CollegeDepartmentId = 0;
            foreach (var a in jntuh_registered_faculty)
            {
                string Reason = String.Empty;
                Specializationid = jntuh_college_faculty_registered.Where(C => C.RegistrationNumber.Trim() == a.RegistrationNumber.Trim()).Select(C => C.SpecializationId).FirstOrDefault();
                CollegeDepartmentId = jntuh_college_faculty_registered.Where(C => C.RegistrationNumber.Trim() == a.RegistrationNumber.Trim()).Select(C => C.DepartmentId).FirstOrDefault();
                var faculty = new FacultyRegistration();
                //faculty.id = a.id;
                //faculty.Type = a.type;
                //faculty.CollegeId = collegeid;
                faculty.RegistrationNumber = a.RegistrationNumber;
                //faculty.UniqueID = a.UniqueID;
                faculty.FirstName = a.FirstName;
                faculty.MiddleName = a.MiddleName;
                faculty.LastName = a.LastName;
                if (PrincipalstrRegNoS.Contains(a.RegistrationNumber.Trim()))
                    faculty.Principal = "Principal";
                else
                    faculty.Principal = "";

                faculty.DegreeName = Specializationid > 0 ? DegreeIdNameBasedOnSpecialization.Where(S => S.Specid == Specializationid).Select(S => S.DegreeName).FirstOrDefault() : "";
                //faculty.DepartmentId = jntuh_college_faculty_registered.Where(e => e.RegistrationNumber == a.RegistrationNumber).Select(e => e.DepartmentId).FirstOrDefault() == null ? null : jntuh_college_faculty_registered.Where(e => e.RegistrationNumber == a.RegistrationNumber).Select(e => e.DepartmentId).FirstOrDefault();
                faculty.SpecializationId = jntuh_college_faculty_registered.Where(e => e.RegistrationNumber == a.RegistrationNumber).Select(e => e.SpecializationId).FirstOrDefault() == null ? null : jntuh_college_faculty_registered.Where(e => e.RegistrationNumber == a.RegistrationNumber).Select(e => e.SpecializationId).FirstOrDefault();
                //faculty.SpecializationName = jntuh_specialization.Where(e => e.id == faculty.SpecializationId).Select(e => e.specializationName).FirstOrDefault() == null ? null : jntuh_specialization.Where(e => e.id == faculty.SpecializationId).Select(e => e.specializationName).FirstOrDefault();
                faculty.PGSpecialization = a.Jntu_PGSpecializationId == null ? null : a.Jntu_PGSpecializationId;
                faculty.PGSpecializationName = faculty.PGSpecialization != 0 ? DegreeIdNameBasedOnSpecialization.Where(e => e.Specid == faculty.PGSpecialization).Select(e => e.DegreeName + "-" + e.SpcializationName).FirstOrDefault() : "";
                //faculty.GenderId = a.GenderId;
                //faculty.Email = a.Email;
                //faculty.facultyPhoto = a.Photo;
                //faculty.Mobile = a.Mobile;
                //faculty.PANNumber = a.PANNumber;
                faculty.isActive = a.isActive;
                //faculty.isApproved = a.isApproved;
                faculty.department = CollegeDepartmentId > 0 ? jntuh_department.Where(d => d.id == CollegeDepartmentId).Select(d => d.departmentName).FirstOrDefault() : "";
                //faculty.SamePANNumberCount = jntuh_registered_faculty.Where(f => f.PANNumber == a.PANNumber).ToList().Count;
                //faculty.SameAadhaarNumberCount = jntuh_registered_faculty.Where(f => f.AadhaarNumber == a.AadhaarNumber).ToList().Count;
                faculty.SpecializationIdentfiedFor = Specializationid > 0 ? DegreeIdNameBasedOnSpecialization.Where(S => S.Specid == Specializationid).Select(S => S.DegreeName + "-" + S.SpcializationName).FirstOrDefault() : "";
                //faculty.isVerified = isFacultyVerified(a.id);
                //faculty.DeactivationReason = a.DeactivationReason;
                //faculty.FacultyVerificationStatus = a.FacultyVerificationStatus;
                faculty.updatedOn = a.updatedOn;
                //faculty.createdOn = jntuh_college_faculty_registered.Where(f => f.RegistrationNumber == a.RegistrationNumber).Select(f => f.createdOn).FirstOrDefault();
                faculty.IdentfiedFor = jntuh_college_faculty_registered.Where(f => f.RegistrationNumber.Trim() == a.RegistrationNumber.Trim()).Select(f => f.IdentifiedFor).FirstOrDefault();
                faculty.jntuh_registered_faculty_education = a.jntuh_registered_faculty_education;
                faculty.DegreeId = a.jntuh_registered_faculty_education.Where(r => r.educationId != 8).Count(e => e.facultyId == a.id) > 0 ? a.jntuh_registered_faculty_education.Where(e => e.facultyId == a.id && e.educationId != 8).Select(e => e.educationId).Max() : 0;
                //faculty.PanDeactivationReasion = !string.IsNullOrEmpty(a.PanDeactivationReason) ? a.PanDeactivationReason : "";
                faculty.Phd2pages = jntuh_phdfaculty.Where(i => i.Facultyid == a.id).Count() > 0 ? true : false;
                faculty.Absent = a.Absent != null ? (bool)a.Absent : false;
                if (faculty.Absent)
                    Reason += "Absent,";
                //faculty.NOForm26AS = a.NoForm26AS != null ? (bool)a.NoForm26AS : false;
                //if (faculty.NOForm26AS)
                //    Reason += "NOForm26AS,";
                //faculty.Covid19 = a.Covid19 != null ? (bool)a.Covid19 : false;
                //if (faculty.Covid19)
                //    Reason += "Covid19,";
                //faculty.Maternity = a.Maternity != null ? (bool)a.Maternity : false;
                //if (faculty.Maternity)
                //    Reason += "Maternity,";
                //faculty.BlacklistFaculty = a.Blacklistfaculy != null ? (bool)a.Blacklistfaculy : false;
                //faculty.PHDundertakingnotsubmitted = a.PHDundertakingnotsubmitted != null ? (bool)a.PHDundertakingnotsubmitted : false;
                //faculty.NOTQualifiedAsPerAICTE = a.NotQualifiedAsperAICTE != null ? (bool)a.NotQualifiedAsperAICTE : false;
                //if (faculty.NOTQualifiedAsPerAICTE == true || faculty.DegreeId < 4)
                //    Reason += " Not Qualified as Per AICTE/ PCI,";
                faculty.InvalidPANNo = a.InvalidPANNumber != null ? (bool)a.InvalidPANNumber : false;
                if (faculty.InvalidPANNo)
                    Reason += " Invalid PAN,";
                //faculty.InCompleteCeritificates = a.IncompleteCertificates != null ? (bool)a.IncompleteCertificates : false;
                //if (faculty.InCompleteCeritificates)
                //    Reason += " Incomplete Ceritificates,";
                faculty.OriginalCertificatesnotshownFlag = a.OriginalCertificatesNotShown != null ? (bool)a.OriginalCertificatesNotShown : false;
                if (faculty.OriginalCertificatesnotshownFlag)
                    Reason += " Original Certificates Not Shown,";
                //faculty.FalsePAN = a.FalsePAN != null ? (bool)a.FalsePAN : false;
                //faculty.NOForm16 = a.NoForm16 != null ? (bool)a.NoForm16 : false;
                //faculty.XeroxcopyofcertificatesFlag = a.Xeroxcopyofcertificates != null ? (bool)a.Xeroxcopyofcertificates : false;
                //if (faculty.XeroxcopyofcertificatesFlag)
                //    Reason += " Photo copy of Certificates,";
                //faculty.NotIdentityFiedForAnyProgramFlag = a.NotIdentityfiedForanyProgram != null ? (bool)a.NotIdentityfiedForanyProgram : false;
                faculty.NOrelevantUgFlag = a.NoRelevantUG == "Yes" ? true : false;
                if (faculty.NOrelevantUgFlag)
                    Reason += " No Relevant UG,";
                faculty.NOrelevantPgFlag = a.NoRelevantPG == "Yes" ? true : false;
                if (faculty.NOrelevantPgFlag)
                    Reason += " No Relevant PG,";
                faculty.NOrelevantPhdFlag = a.NORelevantPHD == "Yes" ? true : false;
                if (faculty.NOrelevantPhdFlag)
                    Reason += " No Relevant Ph.d,";
                faculty.NoSCM = a.NoSCM != null ? (bool)a.NoSCM : false;
                if ((bool)faculty.NoSCM == true)
                    Reason += " No SCM,";
                faculty.BlacklistFaculty = a.Blacklistfaculy != null ? (bool)a.Blacklistfaculy : false;
                if ((bool)faculty.BlacklistFaculty == true)
                    Reason += " BlackList,";
                //faculty.PhdUndertakingDocumentstatus = a.PhdUndertakingDocumentstatus != null ? (bool)(a.PhdUndertakingDocumentstatus) : false;
                //faculty.PHDUndertakingDocumentView = a.PHDUndertakingDocument;
                //faculty.PhdUndertakingDocumentText = a.PhdUndertakingDocumentText;
                //faculty.Basstatus = a.BAS;
                //if (faculty.Basstatus == "Yes")
                //    Reason += " BAS Flag,";
                //Basstatus Column Consider as Aadhaar Flag 
                //faculty.InvalidAadhaar = a.InvalidAadhaar;
                //if (faculty.InvalidAadhaar == "Yes")
                //{
                //    faculty.AadhaarNumber = jntuh_college_faculty_registered.Where(e => e.RegistrationNumber == a.RegistrationNumber).Select(e => e.AadhaarNumber).FirstOrDefault() == null ? null : jntuh_college_faculty_registered.Where(e => e.RegistrationNumber == a.RegistrationNumber).Select(e => e.AadhaarNumber).FirstOrDefault();
                //    Reason += " Invalid Aadhaar,";
                //}
                //faculty.Deactivedby = a.DeactivatedBy;
                //faculty.DeactivedOn = a.DeactivatedOn;
                //faculty.SCMDocumentView =
                //    db.jntuh_scmupload.Where(u => u.RegistrationNumber.Trim() == faculty.RegistrationNumber.Trim() && u.DepartmentId != 0)
                //        .Select(s => s.SCMDocument)
                //        .FirstOrDefault();
                //faculty.OriginalsVerifiedPHD = a.OriginalsVerifiedPHD == true ? true : false;
                //if (faculty.OriginalsVerifiedPHD)
                //    Reason += " No Guide Sign in PHD Thesis,";
                //faculty.OriginalsVerifiedUG = a.OriginalsVerifiedUG == true ? true : false;
                //if (faculty.OriginalsVerifiedUG)
                //    Reason += " Complaint PHD Faculty,";
                faculty.VerificationStatus = a.AbsentforVerification != null ? (bool)a.AbsentforVerification : false;
                if (faculty.VerificationStatus == true)
                    Reason += " Absent for Physical Verification,";
                //faculty.NoClass = a.Noclass == true ? true : false;
                //if (faculty.NoClass)
                //    Reason += " No Class in UG/PG,";
                //faculty.InvalidDegree = a.Invaliddegree == true ? true : false;
                //if (faculty.InvalidDegree)
                //    Reason += " Invalid Degree,";
                //faculty.GenuinenessnotSubmitted = a.Genuinenessnotsubmitted == true ? true : false;
                //if (faculty.GenuinenessnotSubmitted)
                //    Reason += " Genuineness not Submitted,";
                //faculty.FakePhd = a.FakePHD == true ? true : false;
                //faculty.NotconsiderPhd = a.NotconsideredPHD == true ? true : false;
                //if (faculty.NotconsiderPhd)
                //    Reason += " Not considered Phd,";
                //faculty.NoPgSpecialization = a.NoPGspecialization == true ? true : false;
                //if (faculty.NoPgSpecialization)
                //    Reason += " No Pg Specialization,";

                Reason.Trim();

                if (Reason.Length > 0)
                    Reason = Reason.Remove(Reason.Length - 1, 1);

                faculty.DeactivationReason = Reason;

                teachingFaculty.Add(faculty);
            }
            teachingFaculty = teachingFaculty.Where(m => m.isActive == true).OrderBy(f => f.updatedOn).ThenBy(f => f.department).ToList();

            string ReportHeader = collegeCode + "_Faculty Flags_" + DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + ".xls";
            Response.ClearContent();
            Response.Buffer = true;
            Response.AddHeader("content-disposition", "attachment; filename=" + ReportHeader);
            Response.ContentType = "application/vnd.ms-excel";
            return PartialView("~/Views/FacultyVerificationDENew/FacultyFlagsVerificationExport.cshtml", teachingFaculty);
        }

        public ActionResult AllCollegesFacultyFlagsVerificationExport()
        {
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy =
                db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            ViewBag.Colleges = db.jntuh_college.Join(db.jntuh_college_edit_status, co => co.id, e => e.collegeId, (co, e) => new { co = co, e = e }).Where(c => c.co.isActive == true && c.e.academicyearId == prAy).Select(c => new { collegeId = c.co.id, collegeName = c.co.collegeCode + "-" + c.co.collegeName }).OrderBy(c => c.collegeName).ToList();
            var jntuh_department = db.jntuh_department.Where(e => e.isActive == true).ToList();
            var jntuh_specialization = db.jntuh_specialization.Where(s => s.isActive == true).ToList();

            List<FacultyRegistration> teachingFaculty = new List<FacultyRegistration>();

            var DegreeIdNameBasedOnSpecialization = (from a in db.jntuh_department
                                                     join b in db.jntuh_specialization on a.id equals b.departmentId
                                                     join c in db.jntuh_degree on a.degreeId equals c.id
                                                     select new
                                                     {
                                                         DegreeId = c.id,
                                                         DegreeName = c.degree,
                                                         SpcializationName = b.specializationName,
                                                         Specid = b.id
                                                     }).ToList();
            foreach (var item in ViewBag.Colleges)
            {
                int collegeid = item.collegeId;

                List<jntuh_college_faculty_registered> jntuh_college_faculty_registered = db.jntuh_college_faculty_registered.Where(cf => cf.collegeId == collegeid).Select(cf => cf).ToList();

                string[] strRegNoS = jntuh_college_faculty_registered.Select(cf => cf.RegistrationNumber).ToArray();
                string[] PrincipalstrRegNoS = db.jntuh_college_principal_registered.Where(P => P.collegeId == collegeid).Select(P => P.RegistrationNumber.Trim()).ToArray();
                List<jntuh_registered_faculty> jntuh_registered_faculty = new List<jntuh_registered_faculty>();
                jntuh_registered_faculty = db.jntuh_registered_faculty.Where(rf => strRegNoS.Contains(rf.RegistrationNumber))
                                             .ToList();

                string RegNumber = "";
                int? Specializationid = 0;
                int? CollegeDepartmentId = 0;
                foreach (var a in jntuh_registered_faculty)
                {
                    string Reason = String.Empty;
                    Specializationid = jntuh_college_faculty_registered.Where(C => C.RegistrationNumber.Trim() == a.RegistrationNumber.Trim()).Select(C => C.SpecializationId).FirstOrDefault();
                    CollegeDepartmentId = jntuh_college_faculty_registered.Where(C => C.RegistrationNumber.Trim() == a.RegistrationNumber.Trim()).Select(C => C.DepartmentId).FirstOrDefault();
                    var faculty = new FacultyRegistration();
                    //faculty.id = a.id;
                    //faculty.Type = a.type;
                    //faculty.CollegeId = collegeid;
                    faculty.CollegeName = item.collegeName;
                    faculty.RegistrationNumber = a.RegistrationNumber;
                    //faculty.UniqueID = a.UniqueID;
                    faculty.FirstName = a.FirstName;
                    faculty.MiddleName = a.MiddleName;
                    faculty.LastName = a.LastName;
                    if (PrincipalstrRegNoS.Contains(a.RegistrationNumber.Trim()))
                        faculty.Principal = "Principal";
                    else
                        faculty.Principal = "";

                    faculty.DegreeName = Specializationid > 0 ? DegreeIdNameBasedOnSpecialization.Where(S => S.Specid == Specializationid).Select(S => S.DegreeName).FirstOrDefault() : "";
                    //faculty.DepartmentId = jntuh_college_faculty_registered.Where(e => e.RegistrationNumber == a.RegistrationNumber).Select(e => e.DepartmentId).FirstOrDefault() == null ? null : jntuh_college_faculty_registered.Where(e => e.RegistrationNumber == a.RegistrationNumber).Select(e => e.DepartmentId).FirstOrDefault();
                    faculty.SpecializationId = jntuh_college_faculty_registered.Where(e => e.RegistrationNumber == a.RegistrationNumber).Select(e => e.SpecializationId).FirstOrDefault() == null ? null : jntuh_college_faculty_registered.Where(e => e.RegistrationNumber == a.RegistrationNumber).Select(e => e.SpecializationId).FirstOrDefault();
                    //faculty.SpecializationName = jntuh_specialization.Where(e => e.id == faculty.SpecializationId).Select(e => e.specializationName).FirstOrDefault() == null ? null : jntuh_specialization.Where(e => e.id == faculty.SpecializationId).Select(e => e.specializationName).FirstOrDefault();
                    faculty.PGSpecialization = a.Jntu_PGSpecializationId == null ? null : a.Jntu_PGSpecializationId;
                    faculty.PGSpecializationName = faculty.PGSpecialization != 0 ? DegreeIdNameBasedOnSpecialization.Where(e => e.Specid == faculty.PGSpecialization).Select(e => e.DegreeName + "-" + e.SpcializationName).FirstOrDefault() : "";
                    //faculty.GenderId = a.GenderId;
                    //faculty.Email = a.Email;
                    //faculty.facultyPhoto = a.Photo;
                    //faculty.Mobile = a.Mobile;
                    //faculty.PANNumber = a.PANNumber;
                    faculty.isActive = a.isActive;
                    //faculty.isApproved = a.isApproved;
                    faculty.department = CollegeDepartmentId > 0 ? jntuh_department.Where(d => d.id == CollegeDepartmentId).Select(d => d.departmentName).FirstOrDefault() : "";
                    //faculty.SamePANNumberCount = jntuh_registered_faculty.Where(f => f.PANNumber == a.PANNumber).ToList().Count;
                    //faculty.SameAadhaarNumberCount = jntuh_registered_faculty.Where(f => f.AadhaarNumber == a.AadhaarNumber).ToList().Count;
                    faculty.SpecializationIdentfiedFor = Specializationid > 0 ? DegreeIdNameBasedOnSpecialization.Where(S => S.Specid == Specializationid).Select(S => S.DegreeName + "-" + S.SpcializationName).FirstOrDefault() : "";
                    //faculty.isVerified = isFacultyVerified(a.id);
                    //faculty.DeactivationReason = a.DeactivationReason;
                    //faculty.FacultyVerificationStatus = a.FacultyVerificationStatus;
                    faculty.updatedOn = a.updatedOn;
                    //faculty.createdOn = jntuh_college_faculty_registered.Where(f => f.RegistrationNumber == a.RegistrationNumber).Select(f => f.createdOn).FirstOrDefault();
                    faculty.IdentfiedFor = jntuh_college_faculty_registered.Where(f => f.RegistrationNumber.Trim() == a.RegistrationNumber.Trim()).Select(f => f.IdentifiedFor).FirstOrDefault();
                    faculty.jntuh_registered_faculty_education = a.jntuh_registered_faculty_education;
                    faculty.DegreeId = a.jntuh_registered_faculty_education.Where(r => r.educationId != 8).Count(e => e.facultyId == a.id) > 0 ? a.jntuh_registered_faculty_education.Where(e => e.facultyId == a.id && e.educationId != 8).Select(e => e.educationId).Max() : 0;
                    //faculty.PanDeactivationReasion = !string.IsNullOrEmpty(a.PanDeactivationReason) ? a.PanDeactivationReason : "";

                    faculty.Absent = a.Absent != null ? (bool)a.Absent : false;
                    if (faculty.Absent)
                        Reason += "Absent,";
                    faculty.NOForm26AS = a.NoForm26AS != null ? (bool)a.NoForm26AS : false;
                    if (faculty.NOForm26AS)
                        Reason += "NOForm26AS,";
                    faculty.Covid19 = a.Covid19 != null ? (bool)a.Covid19 : false;
                    if (faculty.Covid19)
                        Reason += "Covid19,";
                    faculty.Maternity = a.Maternity != null ? (bool)a.Maternity : false;
                    if (faculty.Maternity)
                        Reason += "Maternity,";
                    //faculty.BlacklistFaculty = a.Blacklistfaculy != null ? (bool)a.Blacklistfaculy : false;
                    //faculty.PHDundertakingnotsubmitted = a.PHDundertakingnotsubmitted != null ? (bool)a.PHDundertakingnotsubmitted : false;
                    faculty.NOTQualifiedAsPerAICTE = a.NotQualifiedAsperAICTE != null ? (bool)a.NotQualifiedAsperAICTE : false;
                    if (faculty.NOTQualifiedAsPerAICTE == true || faculty.DegreeId < 4)
                        Reason += " Not Qualified as Per AICTE/ PCI,";
                    faculty.InvalidPANNo = a.InvalidPANNumber != null ? (bool)a.InvalidPANNumber : false;
                    if (faculty.InvalidPANNo)
                        Reason += " Invalid PAN,";
                    faculty.InCompleteCeritificates = a.IncompleteCertificates != null ? (bool)a.IncompleteCertificates : false;
                    if (faculty.InCompleteCeritificates)
                        Reason += " Incomplete Ceritificates,";
                    faculty.OriginalCertificatesnotshownFlag = a.OriginalCertificatesNotShown != null ? (bool)a.OriginalCertificatesNotShown : false;
                    if (faculty.OriginalCertificatesnotshownFlag)
                        Reason += " Original Certificates Not Shown,";
                    //faculty.FalsePAN = a.FalsePAN != null ? (bool)a.FalsePAN : false;
                    //faculty.NOForm16 = a.NoForm16 != null ? (bool)a.NoForm16 : false;
                    faculty.XeroxcopyofcertificatesFlag = a.Xeroxcopyofcertificates != null ? (bool)a.Xeroxcopyofcertificates : false;
                    if (faculty.XeroxcopyofcertificatesFlag)
                        Reason += " Photo copy of Certificates,";
                    //faculty.NotIdentityFiedForAnyProgramFlag = a.NotIdentityfiedForanyProgram != null ? (bool)a.NotIdentityfiedForanyProgram : false;
                    faculty.NOrelevantUgFlag = a.NoRelevantUG == "Yes" ? true : false;
                    if (faculty.NOrelevantUgFlag)
                        Reason += " No Relevant UG,";
                    faculty.NOrelevantPgFlag = a.NoRelevantPG == "Yes" ? true : false;
                    if (faculty.NOrelevantPgFlag)
                        Reason += " No Relevant PG,";
                    faculty.NOrelevantPhdFlag = a.NORelevantPHD == "Yes" ? true : false;
                    if (faculty.NOrelevantPhdFlag)
                        Reason += " No Relevant Ph.d,";
                    faculty.NoSCM17Flag = a.NoSCM != null ? (bool)a.NoSCM : false;
                    if (faculty.NoSCM17Flag)
                        Reason += " No SCM,";
                    //faculty.PhdUndertakingDocumentstatus = a.PhdUndertakingDocumentstatus != null ? (bool)(a.PhdUndertakingDocumentstatus) : false;
                    //faculty.PHDUndertakingDocumentView = a.PHDUndertakingDocument;
                    //faculty.PhdUndertakingDocumentText = a.PhdUndertakingDocumentText;
                    faculty.Basstatus = a.BAS;
                    if (faculty.Basstatus == "Yes")
                        Reason += " BAS Flag,";
                    //Basstatus Column Consider as Aadhaar Flag 
                    faculty.InvalidAadhaar = a.InvalidAadhaar;
                    if (faculty.InvalidAadhaar == "Yes")
                    {
                        faculty.AadhaarNumber = jntuh_college_faculty_registered.Where(e => e.RegistrationNumber == a.RegistrationNumber).Select(e => e.AadhaarNumber).FirstOrDefault() == null ? null : jntuh_college_faculty_registered.Where(e => e.RegistrationNumber == a.RegistrationNumber).Select(e => e.AadhaarNumber).FirstOrDefault();
                        Reason += " Invalid Aadhaar,";
                    }
                    //faculty.Deactivedby = a.DeactivatedBy;
                    //faculty.DeactivedOn = a.DeactivatedOn;
                    //faculty.SCMDocumentView =
                    //    db.jntuh_scmupload.Where(u => u.RegistrationNumber.Trim() == faculty.RegistrationNumber.Trim() && u.DepartmentId != 0)
                    //        .Select(s => s.SCMDocument)
                    //        .FirstOrDefault();
                    faculty.OriginalsVerifiedPHD = a.OriginalsVerifiedPHD == true ? true : false;
                    if (faculty.OriginalsVerifiedPHD)
                        Reason += " No Guide Sign in PHD Thesis,";
                    faculty.OriginalsVerifiedUG = a.OriginalsVerifiedUG == true ? true : false;
                    if (faculty.OriginalsVerifiedUG)
                        Reason += " Complaint PHD Faculty,";
                    faculty.VerificationStatus = a.AbsentforVerification != null ? (bool)a.AbsentforVerification : false;
                    if (faculty.VerificationStatus == true)
                        Reason += " Absent for Physical Verification,";
                    faculty.NoClass = a.Noclass == true ? true : false;
                    if (faculty.NoClass)
                        Reason += " No Class in UG/PG,";
                    faculty.InvalidDegree = a.Invaliddegree == true ? true : false;
                    if (faculty.InvalidDegree)
                        Reason += " Invalid Degree,";
                    faculty.GenuinenessnotSubmitted = a.Genuinenessnotsubmitted == true ? true : false;
                    if (faculty.GenuinenessnotSubmitted)
                        Reason += " Genuineness not Submitted,";
                    //faculty.FakePhd = a.FakePHD == true ? true : false;
                    faculty.NotconsiderPhd = a.NotconsideredPHD == true ? true : false;
                    if (faculty.NotconsiderPhd)
                        Reason += " Not considered Phd,";
                    faculty.NoPgSpecialization = a.NoPGspecialization == true ? true : false;
                    if (faculty.NoPgSpecialization)
                        Reason += " No Pg Specialization,";

                    Reason.Trim();

                    if (Reason.Length > 0)
                        Reason = Reason.Remove(Reason.Length - 1, 1);

                    faculty.DeactivationReason = Reason;

                    teachingFaculty.Add(faculty);
                }
            }
            teachingFaculty = teachingFaculty.Where(m => m.isActive == true).OrderBy(f => f.CollegeName).ThenBy(f => f.updatedOn).ThenBy(f => f.department).ToList();

            string ReportHeader = "All Colleges Faculty Flags_" + DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + ".xls";
            Response.ClearContent();
            Response.Buffer = true;
            Response.AddHeader("content-disposition", "attachment; filename=" + ReportHeader);
            Response.ContentType = "application/vnd.ms-excel";
            return PartialView("~/Views/FacultyVerificationDENew/AllCollegesFacultyFlagsVerificationExport.cshtml", teachingFaculty);
        }

        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification")]
        public ActionResult DepartmentWiseFacultyView(string departmentname)
        {
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy =
                db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            ViewBag.Departments = db.jntuh_department.Where(d => d.isActive == true && d.degreeId == 4 && !d.departmentName.Contains("Others")).Select(d => new { deptName = d.departmentName }).Distinct().OrderBy(d => d.deptName).ToList();
            var jntuh_department = db.jntuh_department.Where(e => e.isActive == true).ToList();
            //var jntuh_specialization = db.jntuh_specialization.Where(s => s.isActive == true).ToList();
            var jntuh_college = db.jntuh_college.ToList();
            List<FacultyRegistration> teachingFaculty = new List<FacultyRegistration>();

            if (departmentname != null)
            {

                var DegreeIdNameBasedOnSpecialization = (from a in db.jntuh_department
                                                         join b in db.jntuh_specialization on a.id equals b.departmentId
                                                         join c in db.jntuh_degree on a.degreeId equals c.id
                                                         select new
                                                         {
                                                             DegreeId = c.id,
                                                             DegreeName = c.degree,
                                                             SpcializationName = b.specializationName,
                                                             Specid = b.id
                                                         }).ToList();

                int[] departmentsId = jntuh_department.Where(d => d.departmentName == departmentname).Select(d => d.id).ToArray();

                List<jntuh_college_faculty_registered> jntuh_college_faculty_registered = db.jntuh_college_faculty_registered.Where(cf => departmentsId.Contains((int)cf.DepartmentId)).Select(cf => cf).ToList();

                string[] strRegNoS = jntuh_college_faculty_registered.Select(cf => cf.RegistrationNumber).ToArray();
                string[] PrincipalstrRegNoS = db.jntuh_college_principal_registered.Select(P => P.RegistrationNumber.Trim()).ToArray();
                List<jntuh_registered_faculty> jntuh_registered_faculty = new List<jntuh_registered_faculty>();
                jntuh_registered_faculty = db.jntuh_registered_faculty.Where(rf => strRegNoS.Contains(rf.RegistrationNumber)).ToList();

                int? Specializationid = 0;
                int? CollegeDepartmentId = 0;
                foreach (var a in jntuh_registered_faculty)
                {
                    string Reason = String.Empty;
                    Specializationid = jntuh_college_faculty_registered.Where(C => C.RegistrationNumber.Trim() == a.RegistrationNumber.Trim()).Select(C => C.SpecializationId).FirstOrDefault();
                    CollegeDepartmentId = jntuh_college_faculty_registered.Where(C => C.RegistrationNumber.Trim() == a.RegistrationNumber.Trim()).Select(C => C.DepartmentId).FirstOrDefault();
                    var faculty = new FacultyRegistration();
                    faculty.id = a.id;
                    //faculty.Type = a.type;
                    faculty.CollegeId = a.collegeId;
                    faculty.CollegeName = jntuh_college.Where(c => c.id == faculty.CollegeId).Select(c => c.collegeCode + "-" + c.collegeName).FirstOrDefault();
                    faculty.DegreeName = Specializationid > 0 ? DegreeIdNameBasedOnSpecialization.Where(S => S.Specid == Specializationid).Select(S => S.DegreeName).FirstOrDefault() : "";
                    faculty.RegistrationNumber = a.RegistrationNumber;
                    //faculty.UniqueID = a.UniqueID;
                    faculty.FirstName = a.FirstName;
                    faculty.MiddleName = a.MiddleName;
                    faculty.LastName = a.LastName;
                    //faculty.Basstatus = a.InvalidAadhaar;
                    if (PrincipalstrRegNoS.Contains(a.RegistrationNumber.Trim()))
                        faculty.Principal = "Principal";
                    else
                        faculty.Principal = "";

                    //faculty.DepartmentId = jntuh_college_faculty_registered.Where(e => e.RegistrationNumber == a.RegistrationNumber).Select(e => e.DepartmentId).FirstOrDefault() == null ? null : jntuh_college_faculty_registered.Where(e => e.RegistrationNumber == a.RegistrationNumber).Select(e => e.DepartmentId).FirstOrDefault();
                    //faculty.SpecializationId = jntuh_college_faculty_registered.Where(e => e.RegistrationNumber == a.RegistrationNumber).Select(e => e.SpecializationId).FirstOrDefault() == null ? null : jntuh_college_faculty_registered.Where(e => e.RegistrationNumber == a.RegistrationNumber).Select(e => e.SpecializationId).FirstOrDefault();
                    //faculty.SpecializationName = jntuh_specialization.Where(e => e.id == faculty.SpecializationId).Select(e => e.specializationName).FirstOrDefault() == null ? null : jntuh_specialization.Where(e => e.id == faculty.SpecializationId).Select(e => e.specializationName).FirstOrDefault();
                    //faculty.PGSpecialization = jntuh_college_faculty_registered.Where(e => e.RegistrationNumber == a.RegistrationNumber).Select(e => e.FacultySpecializationId).FirstOrDefault() == null ? null : jntuh_college_faculty_registered.Where(e => e.RegistrationNumber == a.RegistrationNumber).Select(e => e.FacultySpecializationId).FirstOrDefault();
                    faculty.PGSpecialization = a.Jntu_PGSpecializationId == null ? null : a.Jntu_PGSpecializationId;
                    faculty.PGSpecializationName = faculty.PGSpecialization != 0 ? DegreeIdNameBasedOnSpecialization.Where(e => e.Specid == faculty.PGSpecialization).Select(e => e.DegreeName + "-" + e.SpcializationName).FirstOrDefault() : "";
                    //faculty.GenderId = a.GenderId;
                    faculty.Email = a.Email;
                    //faculty.facultyPhoto = a.Photo;
                    faculty.Mobile = a.Mobile;
                    //faculty.PANNumber = a.PANNumber;
                    //faculty.AadhaarNumber = a.AadhaarNumber;
                    faculty.isActive = a.isActive;
                    //faculty.isApproved = a.isApproved;
                    faculty.department = CollegeDepartmentId > 0 ? jntuh_department.Where(d => d.id == CollegeDepartmentId).Select(d => d.departmentName).FirstOrDefault() : "";
                    //faculty.SamePANNumberCount = jntuh_registered_faculty.Where(f => f.PANNumber == a.PANNumber).ToList().Count;
                    //faculty.SameAadhaarNumberCount = jntuh_registered_faculty.Where(f => f.AadhaarNumber == a.AadhaarNumber).ToList().Count;
                    faculty.SpecializationIdentfiedFor = Specializationid > 0 ? DegreeIdNameBasedOnSpecialization.Where(S => S.Specid == Specializationid).Select(S => S.DegreeName + "-" + S.SpcializationName).FirstOrDefault() : "";
                    //faculty.isVerified = isFacultyVerified(a.id);
                    //faculty.DeactivationReason = a.DeactivationReason;
                    //faculty.FacultyVerificationStatus = a.FacultyVerificationStatus;
                    faculty.updatedOn = a.updatedOn;
                    //faculty.createdOn = jntuh_college_faculty_registered.Where(f => f.RegistrationNumber == a.RegistrationNumber).Select(f => f.createdOn).FirstOrDefault();
                    faculty.IdentfiedFor = jntuh_college_faculty_registered.Where(f => f.RegistrationNumber.Trim() == a.RegistrationNumber.Trim()).Select(f => f.IdentifiedFor).FirstOrDefault();
                    faculty.jntuh_registered_faculty_education = a.jntuh_registered_faculty_education;
                    //faculty.DegreeId = a.jntuh_registered_faculty_education.Where(r => r.educationId != 8).Count(e => e.facultyId == a.id) > 0 ? a.jntuh_registered_faculty_education.Where(e => e.facultyId == a.id && e.educationId != 8).Select(e => e.educationId).Max() : 0;
                    //faculty.PanDeactivationReasion = !string.IsNullOrEmpty(a.PanDeactivationReason) ? a.PanDeactivationReason : "";

                    //faculty.Absent = a.Absent != null ? (bool)a.Absent : false;
                    //faculty.BlacklistFaculty = a.Blacklistfaculy != null ? (bool)a.Blacklistfaculy : false;
                    //faculty.PHDundertakingnotsubmitted = a.PHDundertakingnotsubmitted != null ? (bool)a.PHDundertakingnotsubmitted : false;
                    //faculty.NOTQualifiedAsPerAICTE = a.NotQualifiedAsperAICTE != null ? (bool)a.NotQualifiedAsperAICTE : false;
                    //faculty.InvalidPANNo = a.InvalidPANNumber != null ? (bool)a.InvalidPANNumber : false;
                    //faculty.InCompleteCeritificates = a.IncompleteCertificates != null ? (bool)a.IncompleteCertificates : false;
                    //faculty.OriginalCertificatesnotshownFlag = a.OriginalCertificatesNotShown != null ? (bool)a.OriginalCertificatesNotShown : false;
                    //faculty.FalsePAN = a.FalsePAN != null ? (bool)a.FalsePAN : false;
                    //faculty.NOForm16 = a.NoForm16 != null ? (bool)a.NoForm16 : false;
                    //faculty.MultipleReginSamecoll = a.MultipleRegInSameCollege != null ? (bool)a.MultipleRegInSameCollege : false;
                    //faculty.XeroxcopyofcertificatesFlag = a.Xeroxcopyofcertificates != null ? (bool)a.Xeroxcopyofcertificates : false;
                    //faculty.NotIdentityFiedForAnyProgramFlag = a.NotIdentityfiedForanyProgram != null ? (bool)a.NotIdentityfiedForanyProgram : false;
                    //faculty.NOrelevantUgFlag = a.NoRelevantUG == "Yes" ? true : false;
                    //faculty.NOrelevantPgFlag = a.NoRelevantPG == "Yes" ? true : false;
                    //faculty.NOrelevantPhdFlag = a.NORelevantPHD == "Yes" ? true : false;
                    //faculty.NoForm16Verification = a.Noform16Verification != null ? (bool)a.Noform16Verification : false;
                    //faculty.NoSCM17Flag = a.NoSCM != null ? (bool)a.NoSCM : false;
                    //faculty.PhotocopyofPAN = a.PhotoCopyofPAN != null ? (bool)a.PhotoCopyofPAN : false;
                    //faculty.PhdUndertakingDocumentstatus = a.PhdUndertakingDocumentstatus != null ? (bool)(a.PhdUndertakingDocumentstatus) : false;
                    //faculty.PHDUndertakingDocumentView = a.PHDUndertakingDocument;
                    //faculty.PhdUndertakingDocumentText = a.PhdUndertakingDocumentText;
                    //faculty.AppliedPAN = a.AppliedPAN != null ? (bool)(a.AppliedPAN) : false;
                    //faculty.SamePANUsedByMultipleFaculty = a.SamePANUsedByMultipleFaculty != null ? (bool)(a.SamePANUsedByMultipleFaculty) : false;
                    //faculty.Basstatus = a.BAS;
                    //Basstatus Column Consider as Aadhaar Flag 
                    //faculty.InvalidAadhaar = a.InvalidAadhaar;
                    //if (faculty.InvalidAadhaar == "Yes")
                    //{
                    //    faculty.AadhaarNumber = jntuh_college_faculty_registered.Where(e => e.RegistrationNumber == a.RegistrationNumber).Select(e => e.AadhaarNumber).FirstOrDefault() == null ? null : jntuh_college_faculty_registered.Where(e => e.RegistrationNumber == a.RegistrationNumber).Select(e => e.AadhaarNumber).FirstOrDefault();
                    //}
                    //faculty.Deactivedby = a.DeactivatedBy;
                    //faculty.DeactivedOn = a.DeactivatedOn;
                    //faculty.OriginalsVerifiedPHD = a.OriginalsVerifiedPHD == true ? true : false;
                    //faculty.OriginalsVerifiedUG = a.OriginalsVerifiedUG == true ? true : false;
                    //faculty.BlacklistFlag = a.Blacklistfaculy == true ? true : false;
                    //faculty.VerificationStatus = a.AbsentforVerification != null ? (bool)a.AbsentforVerification : false;

                    //faculty.NoClass = a.Noclass == true ? true : false;
                    //faculty.InvalidDegree = a.Invaliddegree == true ? true : false;
                    //faculty.GenuinenessnotSubmitted = a.Genuinenessnotsubmitted == true ? true : false;
                    //faculty.FakePhd = a.FakePHD == true ? true : false;
                    //faculty.NotconsiderPhd = a.NotconsideredPHD == true ? true : false;
                    //faculty.NoPgSpecialization = a.NoPGspecialization == true ? true : false;

                    teachingFaculty.Add(faculty);
                }
                teachingFaculty = teachingFaculty.Where(m => m.isActive == true).OrderBy(f => f.CollegeName).ThenBy(f => f.updatedOn).ToList();
                return View(teachingFaculty);
            }

            return View(teachingFaculty);
        }

        public ActionResult DepartmentWiseFacultyExport(string departmentname)
        {
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy =
                db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            ViewBag.Departments = db.jntuh_department.Where(d => d.isActive == true && d.degreeId == 4 && !d.departmentName.Contains("Others")).Select(d => new { deptName = d.departmentName }).Distinct().OrderBy(d => d.deptName).ToList();
            var jntuh_department = db.jntuh_department.Where(e => e.isActive == true).ToList();
            //var jntuh_specialization = db.jntuh_specialization.Where(s => s.isActive == true).ToList();
            var jntuh_college = db.jntuh_college.ToList();
            List<FacultyRegistration> teachingFaculty = new List<FacultyRegistration>();

            if (departmentname != null)
            {

                var DegreeIdNameBasedOnSpecialization = (from a in db.jntuh_department
                                                         join b in db.jntuh_specialization on a.id equals b.departmentId
                                                         join c in db.jntuh_degree on a.degreeId equals c.id
                                                         select new
                                                         {
                                                             DegreeId = c.id,
                                                             DegreeName = c.degree,
                                                             SpcializationName = b.specializationName,
                                                             Specid = b.id
                                                         }).ToList();

                int[] departmentsId = jntuh_department.Where(d => d.departmentName == departmentname).Select(d => d.id).ToArray();

                List<jntuh_college_faculty_registered> jntuh_college_faculty_registered = db.jntuh_college_faculty_registered.Where(cf => departmentsId.Contains((int)cf.DepartmentId)).Select(cf => cf).ToList();

                string[] strRegNoS = jntuh_college_faculty_registered.Select(cf => cf.RegistrationNumber).ToArray();
                string[] PrincipalstrRegNoS = db.jntuh_college_principal_registered.Select(P => P.RegistrationNumber.Trim()).ToArray();
                List<jntuh_registered_faculty> jntuh_registered_faculty = new List<jntuh_registered_faculty>();
                jntuh_registered_faculty = db.jntuh_registered_faculty.Where(rf => strRegNoS.Contains(rf.RegistrationNumber)).ToList();

                int? Specializationid = 0;
                int? CollegeDepartmentId = 0;
                foreach (var a in jntuh_registered_faculty)
                {
                    string Reason = String.Empty;
                    Specializationid = jntuh_college_faculty_registered.Where(C => C.RegistrationNumber.Trim() == a.RegistrationNumber.Trim()).Select(C => C.SpecializationId).FirstOrDefault();
                    CollegeDepartmentId = jntuh_college_faculty_registered.Where(C => C.RegistrationNumber.Trim() == a.RegistrationNumber.Trim()).Select(C => C.DepartmentId).FirstOrDefault();
                    var faculty = new FacultyRegistration();
                    faculty.id = a.id;
                    //faculty.Type = a.type;
                    faculty.CollegeId = a.collegeId;
                    faculty.CollegeName = jntuh_college.Where(c => c.id == faculty.CollegeId).Select(c => c.collegeCode + "-" + c.collegeName).FirstOrDefault();
                    faculty.DegreeName = Specializationid > 0 ? DegreeIdNameBasedOnSpecialization.Where(S => S.Specid == Specializationid).Select(S => S.DegreeName).FirstOrDefault() : "";
                    faculty.RegistrationNumber = a.RegistrationNumber;
                    //faculty.UniqueID = a.UniqueID;
                    faculty.FirstName = a.FirstName;
                    faculty.MiddleName = a.MiddleName;
                    faculty.LastName = a.LastName;
                    //faculty.Basstatus = a.InvalidAadhaar;
                    if (PrincipalstrRegNoS.Contains(a.RegistrationNumber.Trim()))
                        faculty.Principal = "Principal";
                    else
                        faculty.Principal = "";

                    //faculty.DepartmentId = jntuh_college_faculty_registered.Where(e => e.RegistrationNumber == a.RegistrationNumber).Select(e => e.DepartmentId).FirstOrDefault() == null ? null : jntuh_college_faculty_registered.Where(e => e.RegistrationNumber == a.RegistrationNumber).Select(e => e.DepartmentId).FirstOrDefault();
                    //faculty.SpecializationId = jntuh_college_faculty_registered.Where(e => e.RegistrationNumber == a.RegistrationNumber).Select(e => e.SpecializationId).FirstOrDefault() == null ? null : jntuh_college_faculty_registered.Where(e => e.RegistrationNumber == a.RegistrationNumber).Select(e => e.SpecializationId).FirstOrDefault();
                    //faculty.SpecializationName = jntuh_specialization.Where(e => e.id == faculty.SpecializationId).Select(e => e.specializationName).FirstOrDefault() == null ? null : jntuh_specialization.Where(e => e.id == faculty.SpecializationId).Select(e => e.specializationName).FirstOrDefault();
                    //faculty.PGSpecialization = jntuh_college_faculty_registered.Where(e => e.RegistrationNumber == a.RegistrationNumber).Select(e => e.FacultySpecializationId).FirstOrDefault() == null ? null : jntuh_college_faculty_registered.Where(e => e.RegistrationNumber == a.RegistrationNumber).Select(e => e.FacultySpecializationId).FirstOrDefault();
                    faculty.PGSpecialization = a.Jntu_PGSpecializationId == null ? null : a.Jntu_PGSpecializationId;
                    faculty.PGSpecializationName = faculty.PGSpecialization != 0 ? DegreeIdNameBasedOnSpecialization.Where(e => e.Specid == faculty.PGSpecialization).Select(e => e.DegreeName + "-" + e.SpcializationName).FirstOrDefault() : "";
                    //faculty.GenderId = a.GenderId;
                    faculty.Email = a.Email;
                    //faculty.facultyPhoto = a.Photo;
                    faculty.Mobile = a.Mobile;
                    //faculty.PANNumber = a.PANNumber;
                    //faculty.AadhaarNumber = a.AadhaarNumber;
                    faculty.isActive = a.isActive;
                    //faculty.isApproved = a.isApproved;
                    faculty.department = CollegeDepartmentId > 0 ? jntuh_department.Where(d => d.id == CollegeDepartmentId).Select(d => d.departmentName).FirstOrDefault() : "";
                    //faculty.SamePANNumberCount = jntuh_registered_faculty.Where(f => f.PANNumber == a.PANNumber).ToList().Count;
                    //faculty.SameAadhaarNumberCount = jntuh_registered_faculty.Where(f => f.AadhaarNumber == a.AadhaarNumber).ToList().Count;
                    faculty.SpecializationIdentfiedFor = Specializationid > 0 ? DegreeIdNameBasedOnSpecialization.Where(S => S.Specid == Specializationid).Select(S => S.DegreeName + "-" + S.SpcializationName).FirstOrDefault() : "";
                    //faculty.isVerified = isFacultyVerified(a.id);
                    //faculty.DeactivationReason = a.DeactivationReason;
                    //faculty.FacultyVerificationStatus = a.FacultyVerificationStatus;
                    faculty.updatedOn = a.updatedOn;
                    //faculty.createdOn = jntuh_college_faculty_registered.Where(f => f.RegistrationNumber == a.RegistrationNumber).Select(f => f.createdOn).FirstOrDefault();
                    faculty.IdentfiedFor = jntuh_college_faculty_registered.Where(f => f.RegistrationNumber.Trim() == a.RegistrationNumber.Trim()).Select(f => f.IdentifiedFor).FirstOrDefault();
                    faculty.jntuh_registered_faculty_education = a.jntuh_registered_faculty_education;
                    //faculty.DegreeId = a.jntuh_registered_faculty_education.Where(r => r.educationId != 8).Count(e => e.facultyId == a.id) > 0 ? a.jntuh_registered_faculty_education.Where(e => e.facultyId == a.id && e.educationId != 8).Select(e => e.educationId).Max() : 0;
                    //faculty.PanDeactivationReasion = !string.IsNullOrEmpty(a.PanDeactivationReason) ? a.PanDeactivationReason : "";

                    //faculty.Absent = a.Absent != null ? (bool)a.Absent : false;
                    //faculty.BlacklistFaculty = a.Blacklistfaculy != null ? (bool)a.Blacklistfaculy : false;
                    //faculty.PHDundertakingnotsubmitted = a.PHDundertakingnotsubmitted != null ? (bool)a.PHDundertakingnotsubmitted : false;
                    //faculty.NOTQualifiedAsPerAICTE = a.NotQualifiedAsperAICTE != null ? (bool)a.NotQualifiedAsperAICTE : false;
                    //faculty.InvalidPANNo = a.InvalidPANNumber != null ? (bool)a.InvalidPANNumber : false;
                    //faculty.InCompleteCeritificates = a.IncompleteCertificates != null ? (bool)a.IncompleteCertificates : false;
                    //faculty.OriginalCertificatesnotshownFlag = a.OriginalCertificatesNotShown != null ? (bool)a.OriginalCertificatesNotShown : false;
                    //faculty.FalsePAN = a.FalsePAN != null ? (bool)a.FalsePAN : false;
                    //faculty.NOForm16 = a.NoForm16 != null ? (bool)a.NoForm16 : false;
                    //faculty.MultipleReginSamecoll = a.MultipleRegInSameCollege != null ? (bool)a.MultipleRegInSameCollege : false;
                    //faculty.XeroxcopyofcertificatesFlag = a.Xeroxcopyofcertificates != null ? (bool)a.Xeroxcopyofcertificates : false;
                    //faculty.NotIdentityFiedForAnyProgramFlag = a.NotIdentityfiedForanyProgram != null ? (bool)a.NotIdentityfiedForanyProgram : false;
                    //faculty.NOrelevantUgFlag = a.NoRelevantUG == "Yes" ? true : false;
                    //faculty.NOrelevantPgFlag = a.NoRelevantPG == "Yes" ? true : false;
                    //faculty.NOrelevantPhdFlag = a.NORelevantPHD == "Yes" ? true : false;
                    //faculty.NoForm16Verification = a.Noform16Verification != null ? (bool)a.Noform16Verification : false;
                    //faculty.NoSCM17Flag = a.NoSCM != null ? (bool)a.NoSCM : false;
                    //faculty.PhotocopyofPAN = a.PhotoCopyofPAN != null ? (bool)a.PhotoCopyofPAN : false;
                    //faculty.PhdUndertakingDocumentstatus = a.PhdUndertakingDocumentstatus != null ? (bool)(a.PhdUndertakingDocumentstatus) : false;
                    //faculty.PHDUndertakingDocumentView = a.PHDUndertakingDocument;
                    //faculty.PhdUndertakingDocumentText = a.PhdUndertakingDocumentText;
                    //faculty.AppliedPAN = a.AppliedPAN != null ? (bool)(a.AppliedPAN) : false;
                    //faculty.SamePANUsedByMultipleFaculty = a.SamePANUsedByMultipleFaculty != null ? (bool)(a.SamePANUsedByMultipleFaculty) : false;
                    //faculty.Basstatus = a.BAS;
                    //Basstatus Column Consider as Aadhaar Flag 
                    //faculty.InvalidAadhaar = a.InvalidAadhaar;
                    //if (faculty.InvalidAadhaar == "Yes")
                    //{
                    //    faculty.AadhaarNumber = jntuh_college_faculty_registered.Where(e => e.RegistrationNumber == a.RegistrationNumber).Select(e => e.AadhaarNumber).FirstOrDefault() == null ? null : jntuh_college_faculty_registered.Where(e => e.RegistrationNumber == a.RegistrationNumber).Select(e => e.AadhaarNumber).FirstOrDefault();
                    //}
                    //faculty.Deactivedby = a.DeactivatedBy;
                    //faculty.DeactivedOn = a.DeactivatedOn;
                    //faculty.OriginalsVerifiedPHD = a.OriginalsVerifiedPHD == true ? true : false;
                    //faculty.OriginalsVerifiedUG = a.OriginalsVerifiedUG == true ? true : false;
                    //faculty.BlacklistFlag = a.Blacklistfaculy == true ? true : false;
                    //faculty.VerificationStatus = a.AbsentforVerification != null ? (bool)a.AbsentforVerification : false;

                    //faculty.NoClass = a.Noclass == true ? true : false;
                    //faculty.InvalidDegree = a.Invaliddegree == true ? true : false;
                    //faculty.GenuinenessnotSubmitted = a.Genuinenessnotsubmitted == true ? true : false;
                    //faculty.FakePhd = a.FakePHD == true ? true : false;
                    //faculty.NotconsiderPhd = a.NotconsideredPHD == true ? true : false;
                    //faculty.NoPgSpecialization = a.NoPGspecialization == true ? true : false;

                    teachingFaculty.Add(faculty);
                }
                teachingFaculty = teachingFaculty.Where(m => m.isActive == true).OrderBy(f => f.CollegeName).ThenBy(f => f.updatedOn).ToList();

                string ReportHeader = "Department Wise Faculty_" + DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + ".xls";
                Response.ClearContent();
                Response.Buffer = true;
                Response.AddHeader("content-disposition", "attachment; filename=" + ReportHeader);
                Response.ContentType = "application/vnd.ms-excel";
                return PartialView("~/Views/FacultyVerificationDENew/DepartmentWiseFacultyExport.cshtml", teachingFaculty);
            }

            return View(teachingFaculty);
        }

        public ActionResult AllDepartmentsFacultyExport()
        {
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy =
                db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            ViewBag.Departments = db.jntuh_department.Where(d => d.isActive == true && d.degreeId == 4 && !d.departmentName.Contains("Others")).Select(d => new { deptName = d.departmentName }).Distinct().OrderBy(d => d.deptName).ToList();
            var jntuh_department = db.jntuh_department.Where(e => e.isActive == true).ToList();
            //var jntuh_specialization = db.jntuh_specialization.Where(s => s.isActive == true).ToList();
            var jntuh_college = db.jntuh_college.ToList();
            List<FacultyRegistration> teachingFaculty = new List<FacultyRegistration>();

            var DegreeIdNameBasedOnSpecialization = (from a in db.jntuh_department
                                                     join b in db.jntuh_specialization on a.id equals b.departmentId
                                                     join c in db.jntuh_degree on a.degreeId equals c.id
                                                     select new
                                                     {
                                                         DegreeId = c.id,
                                                         DegreeName = c.degree,
                                                         SpcializationName = b.specializationName,
                                                         Specid = b.id
                                                     }).ToList();
            foreach (var item in ViewBag.Departments)
            {
                string departmentname = item.deptName;

                int[] departmentsId = jntuh_department.Where(d => d.departmentName == departmentname).Select(d => d.id).ToArray();

                List<jntuh_college_faculty_registered> jntuh_college_faculty_registered = db.jntuh_college_faculty_registered.Where(cf => departmentsId.Contains((int)cf.DepartmentId)).Select(cf => cf).ToList();

                string[] strRegNoS = jntuh_college_faculty_registered.Select(cf => cf.RegistrationNumber).ToArray();
                string[] PrincipalstrRegNoS = db.jntuh_college_principal_registered.Select(P => P.RegistrationNumber.Trim()).ToArray();
                List<jntuh_registered_faculty> jntuh_registered_faculty = new List<jntuh_registered_faculty>();
                jntuh_registered_faculty = db.jntuh_registered_faculty.Where(rf => strRegNoS.Contains(rf.RegistrationNumber)).ToList();

                int? Specializationid = 0;
                int? CollegeDepartmentId = 0;
                foreach (var a in jntuh_registered_faculty)
                {
                    string Reason = String.Empty;
                    Specializationid = jntuh_college_faculty_registered.Where(C => C.RegistrationNumber.Trim() == a.RegistrationNumber.Trim()).Select(C => C.SpecializationId).FirstOrDefault();
                    CollegeDepartmentId = jntuh_college_faculty_registered.Where(C => C.RegistrationNumber.Trim() == a.RegistrationNumber.Trim()).Select(C => C.DepartmentId).FirstOrDefault();
                    var faculty = new FacultyRegistration();
                    faculty.id = a.id;
                    //faculty.Type = a.type;
                    faculty.CollegeId = a.collegeId;
                    faculty.CollegeName = jntuh_college.Where(c => c.id == faculty.CollegeId).Select(c => c.collegeCode + "-" + c.collegeName).FirstOrDefault();
                    faculty.DegreeName = Specializationid > 0 ? DegreeIdNameBasedOnSpecialization.Where(S => S.Specid == Specializationid).Select(S => S.DegreeName).FirstOrDefault() : "";
                    faculty.RegistrationNumber = a.RegistrationNumber;
                    //faculty.UniqueID = a.UniqueID;
                    faculty.FirstName = a.FirstName;
                    faculty.MiddleName = a.MiddleName;
                    faculty.LastName = a.LastName;
                    //faculty.Basstatus = a.InvalidAadhaar;
                    if (PrincipalstrRegNoS.Contains(a.RegistrationNumber.Trim()))
                        faculty.Principal = "Principal";
                    else
                        faculty.Principal = "";

                    //faculty.DepartmentId = jntuh_college_faculty_registered.Where(e => e.RegistrationNumber == a.RegistrationNumber).Select(e => e.DepartmentId).FirstOrDefault() == null ? null : jntuh_college_faculty_registered.Where(e => e.RegistrationNumber == a.RegistrationNumber).Select(e => e.DepartmentId).FirstOrDefault();
                    //faculty.SpecializationId = jntuh_college_faculty_registered.Where(e => e.RegistrationNumber == a.RegistrationNumber).Select(e => e.SpecializationId).FirstOrDefault() == null ? null : jntuh_college_faculty_registered.Where(e => e.RegistrationNumber == a.RegistrationNumber).Select(e => e.SpecializationId).FirstOrDefault();
                    //faculty.SpecializationName = jntuh_specialization.Where(e => e.id == faculty.SpecializationId).Select(e => e.specializationName).FirstOrDefault() == null ? null : jntuh_specialization.Where(e => e.id == faculty.SpecializationId).Select(e => e.specializationName).FirstOrDefault();
                    //faculty.PGSpecialization = jntuh_college_faculty_registered.Where(e => e.RegistrationNumber == a.RegistrationNumber).Select(e => e.FacultySpecializationId).FirstOrDefault() == null ? null : jntuh_college_faculty_registered.Where(e => e.RegistrationNumber == a.RegistrationNumber).Select(e => e.FacultySpecializationId).FirstOrDefault();
                    faculty.PGSpecialization = a.Jntu_PGSpecializationId == null ? null : a.Jntu_PGSpecializationId;
                    faculty.PGSpecializationName = faculty.PGSpecialization != 0 ? DegreeIdNameBasedOnSpecialization.Where(e => e.Specid == faculty.PGSpecialization).Select(e => e.DegreeName + "-" + e.SpcializationName).FirstOrDefault() : "";
                    //faculty.GenderId = a.GenderId;
                    faculty.Email = a.Email;
                    //faculty.facultyPhoto = a.Photo;
                    faculty.Mobile = a.Mobile;
                    //faculty.PANNumber = a.PANNumber;
                    //faculty.AadhaarNumber = a.AadhaarNumber;
                    faculty.isActive = a.isActive;
                    //faculty.isApproved = a.isApproved;
                    faculty.department = CollegeDepartmentId > 0 ? jntuh_department.Where(d => d.id == CollegeDepartmentId).Select(d => d.departmentName).FirstOrDefault() : "";
                    //faculty.SamePANNumberCount = jntuh_registered_faculty.Where(f => f.PANNumber == a.PANNumber).ToList().Count;
                    //faculty.SameAadhaarNumberCount = jntuh_registered_faculty.Where(f => f.AadhaarNumber == a.AadhaarNumber).ToList().Count;
                    faculty.SpecializationIdentfiedFor = Specializationid > 0 ? DegreeIdNameBasedOnSpecialization.Where(S => S.Specid == Specializationid).Select(S => S.DegreeName + "-" + S.SpcializationName).FirstOrDefault() : "";
                    //faculty.isVerified = isFacultyVerified(a.id);
                    //faculty.DeactivationReason = a.DeactivationReason;
                    //faculty.FacultyVerificationStatus = a.FacultyVerificationStatus;
                    faculty.updatedOn = a.updatedOn;
                    //faculty.createdOn = jntuh_college_faculty_registered.Where(f => f.RegistrationNumber == a.RegistrationNumber).Select(f => f.createdOn).FirstOrDefault();
                    faculty.IdentfiedFor = jntuh_college_faculty_registered.Where(f => f.RegistrationNumber.Trim() == a.RegistrationNumber.Trim()).Select(f => f.IdentifiedFor).FirstOrDefault();
                    faculty.jntuh_registered_faculty_education = a.jntuh_registered_faculty_education;
                    //faculty.DegreeId = a.jntuh_registered_faculty_education.Where(r => r.educationId != 8).Count(e => e.facultyId == a.id) > 0 ? a.jntuh_registered_faculty_education.Where(e => e.facultyId == a.id && e.educationId != 8).Select(e => e.educationId).Max() : 0;
                    //faculty.PanDeactivationReasion = !string.IsNullOrEmpty(a.PanDeactivationReason) ? a.PanDeactivationReason : "";

                    //faculty.Absent = a.Absent != null ? (bool)a.Absent : false;
                    //faculty.BlacklistFaculty = a.Blacklistfaculy != null ? (bool)a.Blacklistfaculy : false;
                    //faculty.PHDundertakingnotsubmitted = a.PHDundertakingnotsubmitted != null ? (bool)a.PHDundertakingnotsubmitted : false;
                    //faculty.NOTQualifiedAsPerAICTE = a.NotQualifiedAsperAICTE != null ? (bool)a.NotQualifiedAsperAICTE : false;
                    //faculty.InvalidPANNo = a.InvalidPANNumber != null ? (bool)a.InvalidPANNumber : false;
                    //faculty.InCompleteCeritificates = a.IncompleteCertificates != null ? (bool)a.IncompleteCertificates : false;
                    //faculty.OriginalCertificatesnotshownFlag = a.OriginalCertificatesNotShown != null ? (bool)a.OriginalCertificatesNotShown : false;
                    //faculty.FalsePAN = a.FalsePAN != null ? (bool)a.FalsePAN : false;
                    //faculty.NOForm16 = a.NoForm16 != null ? (bool)a.NoForm16 : false;
                    //faculty.MultipleReginSamecoll = a.MultipleRegInSameCollege != null ? (bool)a.MultipleRegInSameCollege : false;
                    //faculty.XeroxcopyofcertificatesFlag = a.Xeroxcopyofcertificates != null ? (bool)a.Xeroxcopyofcertificates : false;
                    //faculty.NotIdentityFiedForAnyProgramFlag = a.NotIdentityfiedForanyProgram != null ? (bool)a.NotIdentityfiedForanyProgram : false;
                    //faculty.NOrelevantUgFlag = a.NoRelevantUG == "Yes" ? true : false;
                    //faculty.NOrelevantPgFlag = a.NoRelevantPG == "Yes" ? true : false;
                    //faculty.NOrelevantPhdFlag = a.NORelevantPHD == "Yes" ? true : false;
                    //faculty.NoForm16Verification = a.Noform16Verification != null ? (bool)a.Noform16Verification : false;
                    //faculty.NoSCM17Flag = a.NoSCM != null ? (bool)a.NoSCM : false;
                    //faculty.PhotocopyofPAN = a.PhotoCopyofPAN != null ? (bool)a.PhotoCopyofPAN : false;
                    //faculty.PhdUndertakingDocumentstatus = a.PhdUndertakingDocumentstatus != null ? (bool)(a.PhdUndertakingDocumentstatus) : false;
                    //faculty.PHDUndertakingDocumentView = a.PHDUndertakingDocument;
                    //faculty.PhdUndertakingDocumentText = a.PhdUndertakingDocumentText;
                    //faculty.AppliedPAN = a.AppliedPAN != null ? (bool)(a.AppliedPAN) : false;
                    //faculty.SamePANUsedByMultipleFaculty = a.SamePANUsedByMultipleFaculty != null ? (bool)(a.SamePANUsedByMultipleFaculty) : false;
                    //faculty.Basstatus = a.BAS;
                    //Basstatus Column Consider as Aadhaar Flag 
                    //faculty.InvalidAadhaar = a.InvalidAadhaar;
                    //if (faculty.InvalidAadhaar == "Yes")
                    //{
                    //    faculty.AadhaarNumber = jntuh_college_faculty_registered.Where(e => e.RegistrationNumber == a.RegistrationNumber).Select(e => e.AadhaarNumber).FirstOrDefault() == null ? null : jntuh_college_faculty_registered.Where(e => e.RegistrationNumber == a.RegistrationNumber).Select(e => e.AadhaarNumber).FirstOrDefault();
                    //}
                    //faculty.Deactivedby = a.DeactivatedBy;
                    //faculty.DeactivedOn = a.DeactivatedOn;
                    //faculty.OriginalsVerifiedPHD = a.OriginalsVerifiedPHD == true ? true : false;
                    //faculty.OriginalsVerifiedUG = a.OriginalsVerifiedUG == true ? true : false;
                    //faculty.BlacklistFlag = a.Blacklistfaculy == true ? true : false;
                    //faculty.VerificationStatus = a.AbsentforVerification != null ? (bool)a.AbsentforVerification : false;

                    //faculty.NoClass = a.Noclass == true ? true : false;
                    //faculty.InvalidDegree = a.Invaliddegree == true ? true : false;
                    //faculty.GenuinenessnotSubmitted = a.Genuinenessnotsubmitted == true ? true : false;
                    //faculty.FakePhd = a.FakePHD == true ? true : false;
                    //faculty.NotconsiderPhd = a.NotconsideredPHD == true ? true : false;
                    //faculty.NoPgSpecialization = a.NoPGspecialization == true ? true : false;

                    teachingFaculty.Add(faculty);
                }
            }
            teachingFaculty = teachingFaculty.Where(m => m.isActive == true).OrderBy(f => f.CollegeName).ThenBy(f => f.updatedOn).ToList();

            string ReportHeader = "All Departmenst Faculty_" + DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + ".xls";
            Response.ClearContent();
            Response.Buffer = true;
            Response.AddHeader("content-disposition", "attachment; filename=" + ReportHeader);
            Response.ContentType = "application/vnd.ms-excel";
            return PartialView("~/Views/FacultyVerificationDENew/DepartmentWiseFacultyExport.cshtml", teachingFaculty);
        }

        //Adding Faculty Departmet
        [Authorize(Roles = "Admin")]
        public ActionResult ClearFacultyAddDepartment(string fid, int collegeid)//collegeid
        {
            var faculty = new CollegeFaculty();
            var fId = 0;
            if (!string.IsNullOrEmpty(fid))
            {
                fId = Convert.ToInt32(Utilities.DecryptString(fid, WebConfigurationManager.AppSettings["CryptoKey"]));
            }
            var facultyregistered = db.jntuh_registered_faculty.Find(fId);
            if (facultyregistered != null)
            {
                faculty.collegeId = collegeid;
                faculty.id = facultyregistered.id;
                faculty.facultyFirstName = facultyregistered.FirstName;
                faculty.facultyLastName = facultyregistered.LastName;
                faculty.facultySurname = facultyregistered.MiddleName;
                faculty.facultyDesignationId = facultyregistered.DesignationId;
                faculty.designation = db.jntuh_designation.Where(d => d.id == faculty.facultyDesignationId).Select(d => d.designation).FirstOrDefault();
                faculty.facultyOtherDesignation = facultyregistered.OtherDesignation;
                faculty.FacultyRegistrationNumber = facultyregistered.RegistrationNumber;

            }


            List<int> deptIds = new List<int>() { 71, 72, 73, 74, 75, 76, 77, 78 };
            var depts = db.jntuh_department.Join(db.jntuh_degree, dept => dept.degreeId, Deg => Deg.id, (dept, Deg) => new { dept = dept, Deg = Deg }).Where(e => e.dept.isActive == true && !deptIds.Contains(e.dept.id)).Select(e => new { id = e.dept.id, departmentName = e.Deg.degree + "-" + e.dept.departmentName }).ToList();
            ViewBag.department = depts;

            var Specialization = (from de in db.jntuh_degree
                                  join dep in db.jntuh_department on de.id equals dep.degreeId
                                  join spec in db.jntuh_specialization on dep.id equals spec.departmentId
                                  where dep.isActive == true && !deptIds.Contains(dep.id)
                                  select new
                                  {
                                      id = spec.id,
                                      spec = de.degree + "-" + spec.specializationName
                                  }).ToList();

            var pgSpecializations = db.jntuh_specialization.Select(e => new { id = e.id, spec = e.specializationName }).ToList();


            ViewBag.PGSpecializations = Specialization;

            return PartialView(faculty);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult ClearFacultyAddDepartment(CollegeFaculty faculty)
        {
            int userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            TempData["Error"] = null;
            //var jntuh_registered_faculty = db.jntuh_registered_faculty.AsNoTracking().ToList();
            //var jntuh_college_faculty_registered = db.jntuh_college_faculty_registered.AsNoTracking().ToList();
            // var jntuh_college_nodepartments = db.jntuh_college_nodepartments.AsNoTracking().ToList();

            var isExistingFaculty = db.jntuh_registered_faculty.FirstOrDefault(r => r.RegistrationNumber == faculty.FacultyRegistrationNumber);
            if (isExistingFaculty != null)
            {
                var iscollegefacultyexist = db.jntuh_college_faculty_registered.FirstOrDefault(r => r.RegistrationNumber.Trim() == faculty.FacultyRegistrationNumber.Trim());
                if (iscollegefacultyexist != null)
                {
                    jntuh_college_faculty_registered collegefaculty = db.jntuh_college_faculty_registered.Find(iscollegefacultyexist.id);
                    collegefaculty.DepartmentId = faculty.facultyDepartmentId;
                    int degreeid =
                        db.jntuh_department.Where(d => d.id == collegefaculty.DepartmentId)
                            .Select(s => s.degreeId)
                            .FirstOrDefault();
                    if (degreeid == 4)
                    {
                        collegefaculty.IdentifiedFor = "UG";
                        collegefaculty.SpecializationId = null;
                    }
                    else
                    {
                        collegefaculty.IdentifiedFor = "PG";
                        collegefaculty.SpecializationId = faculty.SpecializationId;
                    }

                    db.Entry(collegefaculty).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Success"] = "Faculty Department & Specialization (" + faculty.FacultyRegistrationNumber + " ) Successfully Updated ..";
                    TempData["Error"] = null;
                }
                else
                {
                    TempData["Success"] = null;
                    TempData["Error"] = "";
                }
            }
            return RedirectToAction("FacultyFlagsVerificationView", "FacultyVerificationDENew", new { @collegeid = faculty.collegeId });
        }

        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification,DataEntry")]
        [HttpGet]
        public ActionResult PharmacyFacultySpecializationAdd(int collegeId, string regno)
        {
            var faculty = new CollegeFaculty();
            var existingfaculty = db.jntuh_registered_faculty.FirstOrDefault(i => i.RegistrationNumber == regno.Trim());
            if (existingfaculty != null)
            {
                var jntuh_college_faculty_registered = db.jntuh_college_faculty_registered.Where(i => i.RegistrationNumber == regno).Select(e => e).FirstOrDefault();
                var jntuh_registredfaculty = db.jntuh_registered_faculty.Where(r => r.RegistrationNumber == regno.Trim()).FirstOrDefault();
                faculty.collegeId = collegeId;
                faculty.id = existingfaculty.id;
                faculty.facultyFirstName = existingfaculty.FirstName;
                faculty.facultyLastName = existingfaculty.LastName;
                faculty.facultySurname = existingfaculty.MiddleName;
                faculty.facultyDesignationId = existingfaculty.DesignationId;
                faculty.designation = db.jntuh_designation.Where(d => d.id == faculty.facultyDesignationId).Select(d => d.designation).FirstOrDefault();
                faculty.facultyOtherDesignation = existingfaculty.OtherDesignation;
                if (existingfaculty.DepartmentId != null)
                    faculty.facultyDepartmentId = (int)existingfaculty.DepartmentId;
                faculty.department = db.jntuh_department.Where(d => d.id == faculty.facultyDepartmentId).Select(d => d.departmentName).FirstOrDefault();
                faculty.FacultyRegistrationNumber = regno;

                faculty.facultyRecruitedFor = jntuh_college_faculty_registered.IdentifiedFor;
                faculty.SpecializationId = existingfaculty.Jntu_PGSpecializationId == null ? null : existingfaculty.Jntu_PGSpecializationId;
            }


            var jntuh_specialization = db.jntuh_specialization.Where(s => s.isActive == true).Select(e => e).ToList();
            var jntuh_department = db.jntuh_department.Where(s => s.isActive == true).Select(e => e).ToList();
            var jntuh_degree = db.jntuh_degree.Where(s => s.isActive == true).Select(e => e).ToList();
            var Data =
                        (from s in jntuh_specialization
                         join d in jntuh_department on s.departmentId equals d.id
                         join de in jntuh_degree on d.degreeId equals de.id
                         where de.id == 2 || de.id == 9 || de.id == 10
                         select new
                         {
                             id = s.id,
                             spec = s.specializationName
                         }).Distinct().ToList();



            ViewBag.PGSpecializations = Data;
            return PartialView(faculty);
        }

        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification,DataEntry")]
        [HttpPost]
        public ActionResult PharmacyFacultySpecializationAdd(CollegeFaculty faculty)
        {
            TempData["Error"] = null;
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int collegeid = db.jntuh_college_faculty_registered.Where(r => r.RegistrationNumber == faculty.FacultyRegistrationNumber.Trim()).Select(s => s.collegeId).FirstOrDefault();
            var isExistingFaculty = db.jntuh_registered_faculty.FirstOrDefault(r => r.RegistrationNumber == faculty.FacultyRegistrationNumber.Trim());
            if (isExistingFaculty != null && faculty.SpecializationId != 0)
            {
                isExistingFaculty.Jntu_PGSpecializationId = faculty.SpecializationId;
                //isExistingFaculty.updatedBy = userID;
                //isExistingFaculty.updatedOn = DateTime.Now;
                db.Entry(isExistingFaculty).State = EntityState.Modified;
                db.SaveChanges();
                TempData["Success"] = "Faculty Specialization (" + faculty.FacultyRegistrationNumber + " ) Successfully Updated ..";
                TempData["Error"] = null;
            }
            else
            {
                TempData["Error"] = "No data found.";
            }
            return RedirectToAction("FacultyFlagsVerificationView", new { collegeid = collegeid });
            //return RedirectToAction("ViewFacultyDetails", "FacultyVerificationDENew", new { fid = UAAAS.Models.Utilities.EncryptString(faculty.id.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]) });
        }


        #endregion

        [Authorize(Roles = "Admin,DataEntry,FacultyVerification")]
        public ActionResult GetFacultyBASDetailsView(string RegistarationNumber)
        {
            int userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int collegeID = db.jntuh_college_faculty_registered.Where(r => r.RegistrationNumber == RegistarationNumber).Select(e => e.collegeId).FirstOrDefault();
            if (collegeID == 0)
            {
                collegeID = db.jntuh_college_principal_registered.Where(r => r.RegistrationNumber == RegistarationNumber).Select(e => e.collegeId).FirstOrDefault();
            }
            if (RegistarationNumber != null)
            {
                GetFacultyBASDetails Faculty = new GetFacultyBASDetails();
                var FacultyBASData = db.jntuh_college_basreport.Where(e => e.RegistrationNumber == RegistarationNumber && e.collegeId == collegeID && e.monthId == 15).Select(e => e).ToList();
                if (FacultyBASData.Count() != 0 && FacultyBASData != null)
                {
                    Faculty.RegistarationNumber = RegistarationNumber;
                    Faculty.FacultyName = FacultyBASData.Select(e => e.name).LastOrDefault().ToString();
                    string date = FacultyBASData.Select(e => e.joiningDate).LastOrDefault().ToString();
                    Faculty.BasJoiningDate = date == null ? null : Convert.ToDateTime(date).ToString("dd-MM-yyyy");
                    Faculty.TotalWorkingDays = FacultyBASData.Select(e => e.totalworkingDays).Sum();
                    Faculty.TotalPresentDays = FacultyBASData.Select(e => e.NoofPresentDays).Sum();
                    List<GetFacultyBASDetails> facultylist = new List<GetFacultyBASDetails>();
                    foreach (var item in FacultyBASData)
                    {
                        GetFacultyBASDetails newFaculty = new GetFacultyBASDetails();
                        newFaculty.RegistarationNumber = item.RegistrationNumber.Trim();
                        newFaculty.Month = item.month.Trim() + "-" + item.year;
                        newFaculty.MonthPresentDays = item.NoofPresentDays;
                        newFaculty.MonthWorkingDays = item.totalworkingDays;
                        newFaculty.MonthHolidays = item.NoofHolidays;
                        newFaculty.MonthHolidays = item.NoofHolidays;
                        newFaculty.Createdon = item.createdOn;
                        facultylist.Add(newFaculty);

                        //if (item.month == "July")
                        //{
                        //    Faculty.JulyTotalDays = item.totalworkingDays;
                        //    Faculty.JulyPresentDays = item.NoofPresentDays;
                        //}

                        //if (item.month == "August" && item.year == 2018)
                        //{
                        //    Faculty.AugustTotalDays = item.totalworkingDays;
                        //    Faculty.AugustPresentDays = item.NoofPresentDays;
                        //    Faculty.TotalWorkingDays = Faculty.TotalWorkingDays + item.totalworkingDays;
                        //}

                        //else if (item.month == "September" && item.year == 2018)
                        //{
                        //    Faculty.SeptemberTotalDays = item.totalworkingDays;
                        //    Faculty.SeptemberPresentDays = item.NoofPresentDays;
                        //    Faculty.TotalWorkingDays = Faculty.TotalWorkingDays + item.totalworkingDays;
                        //}

                        //else if (item.month == "October" && item.year == 2018)
                        //{
                        //    Faculty.OctoberTotalDays = item.totalworkingDays;
                        //    Faculty.OctoberPresentDays = item.NoofPresentDays;
                        //    Faculty.TotalWorkingDays = Faculty.TotalWorkingDays + item.totalworkingDays;
                        //}

                        //else if (item.month == "November" && item.year == 2018)
                        //{
                        //    Faculty.NovemberTotalDays = item.totalworkingDays;
                        //    Faculty.NovemberPresentDays = item.NoofPresentDays;
                        //    Faculty.TotalWorkingDays = Faculty.TotalWorkingDays + item.totalworkingDays;
                        //}

                        //else if (item.month == "December" && item.year == 2018)
                        //{
                        //    Faculty.DecemberTotalDays = item.totalworkingDays;
                        //    Faculty.DecemberPresentDays = item.NoofPresentDays;
                        //    Faculty.TotalWorkingDays = Faculty.TotalWorkingDays + item.totalworkingDays;
                        //}

                        //else if (item.month == "January" && item.year == 2019)
                        //{
                        //    Faculty.JanuaryTotalDays = item.totalworkingDays;
                        //    Faculty.JanuaryPresentDays = item.NoofPresentDays;
                        //    Faculty.TotalWorkingDays = Faculty.TotalWorkingDays + item.totalworkingDays;
                        //}

                        //else if (item.month == "February" && item.year == 2019)
                        //{
                        //    Faculty.FebruaryTotalDays = item.totalworkingDays;
                        //    Faculty.FebruaryPresentDays = item.NoofPresentDays;
                        //    Faculty.TotalWorkingDays = Faculty.TotalWorkingDays + item.totalworkingDays;
                        //}

                        //else if (item.month == "March" && item.year == 2019)
                        //{
                        //    Faculty.MarchTotalDays = item.totalworkingDays;
                        //    Faculty.MarchPresentDays = item.NoofPresentDays;
                        //    Faculty.TotalWorkingDays = Faculty.TotalWorkingDays + item.totalworkingDays;
                        //}
                        //else if (item.month == "March" && item.year == 2019)
                        //{
                        //    Faculty.MarchTotalDays = item.totalworkingDays;
                        //    Faculty.MarchPresentDays = item.NoofPresentDays;
                        //    Faculty.TotalWorkingDays = Faculty.TotalWorkingDays + item.totalworkingDays;
                        //}

                    }
                    Faculty.FacultyBASDetails = facultylist;
                    return PartialView("~/Views/FacultyVerificationDENew/_GetFacultyBASDetails.cshtml", Faculty);
                }
                else
                {
                    return PartialView("~/Views/FacultyVerificationDENew/_GetFacultyBASDetails.cshtml", Faculty);
                }
            }
            else
            {
                return RedirectToAction("FacultyFlagsVerificationView", new { collegeId = collegeID });
            }
            // return View();
        }

        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification")]
        public ActionResult FacultyVerificationIndex(int? collegeid)
        {
            return RedirectToAction("LogOn", "Account");
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy =
                db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            ViewBag.Colleges = db.jntuh_college.Join(db.jntuh_college_edit_status, co => co.id, e => e.collegeId, (co, e) => new { co = co, e = e }).Where(c => c.co.isActive == true && c.e.IsCollegeEditable == false && c.e.academicyearId == prAy).Select(c => new { collegeId = c.co.id, collegeName = c.co.collegeCode + "-" + c.co.collegeName }).OrderBy(c => c.collegeName).ToList();
            //ViewBag.Colleges = db.jntuh_college.Join(db.jntuh_college_edit_status, co => co.id, e => e.collegeId, (co, e) => new { co = co, e = e }).Where(c => c.co.isActive == true).Select(c => new { collegeId = c.co.id, collegeName = c.co.collegeCode + "-" + c.co.collegeName }).OrderBy(c => c.collegeName).ToList();

            //int[] EditFacultycollegeIds = {4,5,7,8,9,12,20,22,23,24,26,27,29,30,32,35,38,39,40,41,42,43,44,47,48,55,56,58,68,69,70,72,74,75,77,79,80,81,84,85,86,87,88,91,97,100,103,105,106,107,108,109,110,111,116,117,118,119,121,123,127,128,129,130,132,134,135,137,138,139,141,143,144,145,147,148,150,152,153,155,156,157,158,161,162,163,164,165,166,168,171,172,173,174,175,176,177,178,179,180,181,183,184,185,187,189,192,193,195,196,198,202,203,206,210,211,213,214,215,217,218,222,223,225,227,228,234,235,236,241,242,244,245,246,249,250,254,256,260,261,262,264,266,267,271,273,276,282,287,290,291,292,295,296,298,299,300,302,304,307,309,310,313,315,316,318,319,321,322,324,327,329,334,335,336,349,350,352,353,355,360,364,365,366,367,368,369,370,373,374,376,380,382,384,385,389,391,392,393,399,400,411,414,420,421,423,424,428,435,436,439,441,455,375};
            //ViewBag.Colleges =
            //    db.jntuh_college.Where(c => EditFacultycollegeIds.Contains(c.id))
            //        .Select(a => new {collegeId = a.id, collegeName = a.collegeCode + "-" + a.collegeName})
            //        .OrderBy(r => r.collegeName)
            //        .ToList();
            ViewBag.collegeid = collegeid;
            var jntuh_department = db.jntuh_department.ToList();
            List<FacultyRegistration> teachingFaculty = new List<FacultyRegistration>();

            if (collegeid != null)
            {

                var DegreeIdNameBasedOnSpecialization = (from a in db.jntuh_department
                                                         join b in db.jntuh_specialization on a.id equals b.departmentId
                                                         join c in db.jntuh_degree on a.degreeId equals c.id
                                                         select new
                                                         {
                                                             DegreeId = c.id,
                                                             DegreeName = c.degree,
                                                             SpcializationName = b.specializationName,
                                                             Specid = b.id
                                                         }).ToList();

                //418 Faculty Verification Condition 
                //string[] Copy_RegNos = db.jntuh_college_faculty_registered_copy.Where(c => c.collegeId == 375).Select(e => e.RegistrationNumber).Distinct().ToArray();

                // List<jntuh_college_faculty_registered> jntuh_college_faculty_registered_College_Portal = db.jntuh_college_faculty_registered.Where(cf => cf.collegeId == collegeid).Select(cf => cf).ToList();
                //committed by siva due to only edit option faculty is comming case....
                // jntuh_college_faculty_registered = jntuh_college_faculty_registered.Where(c => !Copy_RegNos.Contains(c.RegistrationNumber)).ToList();


                List<jntuh_college_faculty_registered> jntuh_college_faculty_registered = db.jntuh_college_faculty_registered.Where(cf => cf.collegeId == collegeid).Select(cf => cf).ToList();

                //419 Faculty Verifi
                string[] collegeRegnos =
                    jntuh_college_faculty_registered.Select(s => s.RegistrationNumber).Distinct().ToArray();
                string[] Copy_RegNos = db.jntuh_college_faculty_registered_copy.Where(c => c.collegeId == 1419 && collegeRegnos.Contains(c.RegistrationNumber)).Select(e => e.RegistrationNumber).Distinct().ToArray();

                string[] strRegNoS = jntuh_college_faculty_registered.Where(c => !Copy_RegNos.Contains(c.RegistrationNumber)).Select(cf => cf.RegistrationNumber).ToArray();
                string[] PrincipalstrRegNoS = db.jntuh_college_principal_registered.Where(P => P.collegeId == collegeid).Select(P => P.RegistrationNumber.Trim()).ToArray();
                List<jntuh_registered_faculty> jntuh_registered_faculty = new List<jntuh_registered_faculty>();

                //committed by siva due to only edit option faculty is comming case....
                //jntuh_registered_faculty = db.jntuh_registered_faculty.Where(rf => strRegNoS.Contains(rf.RegistrationNumber) && rf.Blacklistfaculy != true)  //&& (rf.collegeId == null || rf.collegeId == collegeid)//&& rf.Notin116 != true
                //                             .ToList();

                //New Code written by siva due to only edit option faculty is comming case....
                DateTime editoptionStartdate = new DateTime(2018, 08, 30);
                if (collegeid == 375)
                {
                    string[] phdnotcollegeassociate = new string[]
                    {
                        "26150328-102440", "63150328-184136", "74150329-123802", "07150330-011802", "21150330-130127",
                        "45150330-135118", "24150330-154256", "86150330-214902", "24150331-105215", "94150331-122435",
                        "72150401-095414", "63150401-151206", "17150401-154407", "96150401-170945", "30150401-172054",
                        "17150401-192409", "82150401-193318", "02150402-111353", "90150402-113500", "29150402-115939",
                        "44150402-125710", "02150402-134808", "66150402-143755", "60150402-151814", "71150402-153329",
                        "14150402-155349", "76150402-165017", "94150402-165951", "68150403-015716", "89150403-074443",
                        "74150403-101252", "79150403-133318", "50150403-154325", "02150403-220700", "85150403-230538",
                        "31150404-102907", "59150404-113124", "23150404-113644", "64150404-113919", "13150404-123131",
                        "43150404-143444", "16150404-155121", "69150404-162418", "78150404-162446", "04150404-164306",
                        "29150404-175402", "66150404-195728", "34150405-111326", "61150405-121514", "58150405-142724",
                        "70150405-151757", "78150405-203601", "35150405-223509", "48150406-111721", "39150406-112650",
                        "58150406-112805", "75150406-114801", "39150406-124038", "40150406-124742", "00150406-124851",
                        "37150406-133923", "65150406-144550", "81150406-145946", "66150406-154147", "88150406-163814",
                        "95150406-190716", "98150406-191738", "26150407-101433", "74150407-104707", "00150407-105515",
                        "06150407-110409", "78150407-121739", "73150407-124535", "39150407-125323", "85150407-125912",
                        "49150407-132629", "07150407-134148", "46150407-144715", "42150407-154522", "13150407-160822",
                        "13150407-162554", "84150407-164929", "86150407-165203", "28150407-193405", "6979-150408-070114",
                        "3786-150408-101716", "6945-150408-105112", "1666-150408-110342", "9352-150408-112832",
                        "9588-150408-114822", "2809-150408-115117", "3046-150408-123016", "7525-150408-124513",
                        "5043-150408-125708", "8792-150408-140040", "9130-150408-153601", "6082-150408-155301",
                        "0457-150408-215923", "4802-150409-000442", "7860-150409-093946", "7624-150409-103723",
                        "9811-150409-103922", "3556-150409-105506", "8608-150409-110811", "3743-150409-112440",
                        "8748-150409-122037", "2802-150409-125545", "5685-150409-131541", "3037-150409-131714",
                        "0377-150409-133622", "5801-150409-143524", "9091-150409-165116", "6206-150409-165306",
                        "8835-150409-215651", "0123-150410-101600", "2468-150410-103024", "0574-150410-111014",
                        "3151-150410-125404", "9946-150410-132838", "9708-150410-144632", "9927-150411-101844",
                        "4758-150411-123622", "7490-150411-125727", "7362-150412-131129", "3453-150412-185020",
                        "2080-150413-104303", "8441-150413-113304", "8741-150413-125346", "5777-150413-125906",
                        "7118-150413-130756", "0861-150413-142903", "9165-150413-144341", "3285-150413-145210",
                        "6344-150413-170725", "9692-150413-174620", "1324-150413-200901", "2177-150414-063346",
                        "7598-150414-111151", "6100-150414-111354", "7301-150414-112533", "4854-150414-125905",
                        "2215-150414-145324", "8720-150414-153720", "1191-150414-182314", "4024-150414-201804",
                        "1268-150415-140422", "2639-150415-144606", "8636-150416-130230", "4590-150416-143638",
                        "5417-150416-173107", "4699-150417-132914", "1546-150417-152331", "9369-150417-214628",
                        "3425-150418-120359", "4549-150418-133640", "7730-150418-150918", "8184-150418-160647",
                        "3992-150419-080217", "3668-150419-121933", "5218-150420-155112", "5614-150420-155913",
                        "5574-150422-123317", "5453-150424-111832", "1897-150425-151950", "6283-150425-165254",
                        "1339-150425-223809", "3119-150426-160711", "0091-150427-145248", "8599-150428-123056",
                        "9127-150429-155346", "8544-150429-203133", "3100-150430-084713", "9828-150501-044104",
                        "8640-150504-132831", "2916-150505-111623", "7794-150506-111930", "2705-150506-170154",
                        "7149-150507-192126", "1693-150508-154732", "7233-150519-175453", "4896-150619-162217",
                        "1244-150625-160443", "2625-151218-121808", "3428-151219-150017", "2357-160105-123059",
                        "6879-160127-141445", "6672-160128-161546", "2530-160130-131822", "7516-160130-171629",
                        "3738-160130-172215", "8187-160201-143225", "8500-160203-162140", "9193-160204-123623",
                        "7783-160204-153400", "8388-160209-104546", "8771-160209-123747", "1082-160210-102657",
                        "6278-160210-150833", "6456-160213-103822", "4874-160215-165007", "3431-160219-135556",
                        "0653-160220-110651", "6222-160220-124116", "3832-160221-111516", "9683-160221-140840",
                        "1098-160223-134532", "8787-160223-142327", "5772-160224-120936", "9257-160226-122157",
                        "2772-160226-193511", "4865-160229-143431", "1334-160302-111355", "5132-160302-122835",
                        "6161-160302-130228", "7391-160304-125258", "2310-160304-201247", "3068-160305-201328",
                        "3695-160306-065942", "2185-160306-125348", "1061-160306-221302", "3525-160307-141516",
                        "3654-160310-153344", "0452-160310-154804", "7708-160311-153502", "4454-160315-024113",
                        "7613-160315-162203", "5408-160315-171742", "9805-160316-211552", "8052-160317-163026",
                        "4497-160318-164821", "1592-160320-134143", "2892-160320-155417", "1778-161022-132131",
                        "6213-161022-184405", "7016-161024-152425", "6391-161101-112549", "8777-161128-190236",
                        "9905-161202-142028", "1194-161205-171645", "1821-161220-155939", "3259-161221-153152",
                        "7984-170103-155512", "6532-170104-113621", "4374-170104-125349", "7173-170104-134234",
                        "3745-170105-143145", "5118-170106-120828", "7030-170107-122049", "5214-170107-151312",
                        "2773-170109-102839", "7111-170111-113028", "6981-170112-104726", "6076-170112-142605",
                        "4473-170115-183328", "3050-170116-142118", "2453-170118-113454", "9918-170122-172510",
                        "2999-170122-190110", "1062-170123-140150", "0987-170125-114032", "4464-170126-024023",
                        "8631-170126-075712", "7138-170127-100321", "4364-170129-111927", "7953-170131-025801",
                        "3659-170131-043726", "1235-170131-182202", "2355-170131-191226", "9213-170131-224323",
                        "1332-170201-103806", "8423-170201-112533", "8585-170201-120216", "4388-170205-120331",
                        "8972-170205-151326", "9568-170207-150317", "1089-170208-175418", "8860-170210-174656",
                        "3855-170213-144041", "0340-170213-144245", "5798-170213-181815", "6756-170520-123238",
                        "6954-170520-130548", "5237-170520-164625", "5255-170520-175435", "0647-170521-101358",
                        "3919-170521-115934", "8397-170522-122330", "9281-170523-112402", "9242-170523-172200",
                        "9348-170523-173650", "0228-170523-212632", "8004-170911-225045", "3487-170912-102813",
                        "0978-170913-201423", "3028-170914-111305", "0012-170914-122955", "7770-170915-144641",
                        "1493-170915-144826", "3976-170915-180353", "6907-170916-103001", "9760-170916-124258",
                        "9857-170916-131945", "5784-171106-142610", "8276-171207-141230", "5089-171223-141800",
                        "1938-171227-190729", "6366-150408-203914", "7227-180106-110314", "9814-180108-153949",
                        "6112-180109-135003", "0479-180110-220632", "5916-180127-115342", "7802-180130-105715",
                        "1258-180130-143059", "3138-180131-233327", "1704-180201-132810", "8235-180201-145327",
                        "4032-180201-151029", "8166-180201-155956", "4872-180202-103956", "2823-180202-111019",
                        "5308-180202-115849", "5672-180202-153808", "4179-180202-163132", "4248-180202-171805",
                        "5905-180203-132313", "9882-180203-163139", "5056-180207-170351", "7727-180208-134717",
                        "5503-180208-144157", "1747-180208-144342", "2425-180208-152024", "2011-180208-160959",
                        "0755-180208-162637", "8667-180208-205639", "9966-180209-140843", "2164-180219-145739",
                        "1127-180417-133316", "9150-180417-144644", "8584-180417-155254", "6072-180419-143930",
                        "4427-180421-131451", "6023-180629-145052", "6954-180703-140314", "8772-180703-154939",
                        "7948-180710-121316", "0467-180716-115522", "6877-180723-180939", "2916-180811-104805",
                        "2122-180811-132049", "6247-180813-234819", "4935-180820-115112", "9725-180831-130259",
                        "69150326-205400", "07150330-011802", "64150401-171554", "59150404-113124", "95150406-190716",
                        "42150407-094907", "0937-150408-120422", "0141-150408-170937", "7676-150409-150707",
                        "4446-150410-182834", "1628-150410-202050", "3642-150413-114149", "5218-150413-161057",
                        "2631-150416-225714", "3660-150417-155532", "8978-150420-142807", "2716-150421-134851",
                        "2388-150425-155251", "7276-150508-125833", "1244-150625-160443", "5596-160227-132758",
                        "0189-160529-182017", "4181-161216-164008", "7477-170101-063341", "0170-170108-153931",
                        "0606-170203-094716", "8215-170207-113049", "5565-170520-164515", "9850-170520-185454",
                        "6070-170520-193830", "3260-170521-073322", "5847-170522-121603", "8397-170522-122330",
                        "9281-170523-112402", "1837-170523-171255", "5065-170913-161739", "9208-170914-120608",
                        "0385-170914-131203", "5176-171223-143546", "9415-171227-160635", "4704-150409-212359",
                        "9814-180108-153949", "5528-180111-103141", "6031-180202-141035", "7787-180416-235700",
                        "6023-180629-145052", "2916-180811-104805"
                    };
                    jntuh_registered_faculty = db.jntuh_registered_faculty.Where(rf => phdnotcollegeassociate.Contains(rf.RegistrationNumber) && rf.Blacklistfaculy != true && rf.updatedOn >= editoptionStartdate)  //&& (rf.collegeId == null || rf.collegeId == collegeid)//&& rf.Notin116 != true
                                           .ToList();
                }
                else
                {
                    jntuh_registered_faculty = db.jntuh_registered_faculty.Where(rf => strRegNoS.Contains(rf.RegistrationNumber) && rf.Blacklistfaculy != true)  //&& (rf.collegeId == null || rf.collegeId == collegeid)//&& rf.Notin116 != true
                                            .ToList();
                }


                //var jntuh_faculty = db.jntuh_registered_faculty.Where(rf => rf.Blacklistfaculy != true && rf.updatedOn >= editoptionStartdate)  //&& (rf.collegeId == null || rf.collegeId == collegeid)//&& rf.Notin116 != true
                //                            .ToList();

                var Specializations = db.jntuh_specialization.ToList();

                string RegNumber = "";
                int? Specializationid = 0;
                int? CollegeDepartmentId = 0;
                foreach (var a in jntuh_registered_faculty)
                {
                    int educationid =
                        db.jntuh_registered_faculty_education.Where(e => e.facultyId == a.id && e.educationId == 6)
                            .Select(s => s.educationId)
                            .FirstOrDefault();
                    string Reason = String.Empty;
                    if (collegeid == 375)
                    {
                        Specializationid =
                          jntuh_college_faculty_registered.Where(
                              C => C.RegistrationNumber.Trim() == a.RegistrationNumber.Trim())
                              .Select(C => C.SpecializationId)
                              .FirstOrDefault();
                        CollegeDepartmentId =
                            jntuh_college_faculty_registered.Where(
                                C => C.RegistrationNumber.Trim() == a.RegistrationNumber.Trim())
                                .Select(C => C.DepartmentId)
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

                        faculty.PGSpecializationName = a.PGSpecialization != 0
                            ? DegreeIdNameBasedOnSpecialization.Where(e => e.Specid == a.PGSpecialization)
                                .Select(e => e.DegreeName + "-" + e.SpcializationName)
                                .FirstOrDefault()
                            : "";
                        faculty.GenderId = a.GenderId;
                        faculty.Email = a.Email;
                        faculty.facultyPhoto = a.Photo;
                        faculty.Mobile = a.Mobile;
                        faculty.PANNumber = a.PANNumber;
                        faculty.AadhaarNumber = a.AadhaarNumber;
                        faculty.isActive = a.isActive;
                        faculty.isApproved = a.isApproved;
                        faculty.department = CollegeDepartmentId > 0
                            ? jntuh_department.Where(d => d.id == CollegeDepartmentId)
                                .Select(d => d.departmentName)
                                .FirstOrDefault()
                            : "";
                        faculty.SamePANNumberCount =
                            jntuh_registered_faculty.Where(f => f.PANNumber == a.PANNumber).ToList().Count;
                        faculty.SameAadhaarNumberCount =
                            jntuh_registered_faculty.Where(f => f.AadhaarNumber == a.AadhaarNumber).ToList().Count;
                        faculty.SpecializationIdentfiedFor = Specializationid > 0
                            ? DegreeIdNameBasedOnSpecialization.Where(S => S.Specid == Specializationid)
                                .Select(S => S.DegreeName + "-" + S.SpcializationName)
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
                        //faculty.MultipleReginSamecoll = a.MultipleRegInSameCollege != null
                        //    ? (bool)a.MultipleRegInSameCollege
                        //    : false;
                        faculty.XeroxcopyofcertificatesFlag = a.Xeroxcopyofcertificates != null
                            ? (bool)a.Xeroxcopyofcertificates
                            : false;
                        faculty.NotIdentityFiedForAnyProgramFlag = a.NotIdentityfiedForanyProgram != null
                            ? (bool)a.NotIdentityfiedForanyProgram
                            : false;
                        faculty.NOrelevantUgFlag = a.NoRelevantUG == "No" ? false : true;
                        faculty.NOrelevantPgFlag = a.NoRelevantPG == "No" ? false : true;
                        faculty.NOrelevantPhdFlag = a.NORelevantPHD == "No" ? false : true;
                        //faculty.NoForm16Verification = a.Noform16Verification !=null ? (bool)a.Noform16Verification : false;
                        faculty.NoSCM17Flag = a.NoSCM17 != null ? (bool)a.NoSCM17 : false;
                        //faculty.PhotocopyofPAN = a.PhotoCopyofPAN != null ? (bool)a.PhotoCopyofPAN : false;
                        faculty.PhdUndertakingDocumentstatus = a.PhdUndertakingDocumentstatus != null
                            ? (bool)(a.PhdUndertakingDocumentstatus)
                            : false;
                        faculty.PHDUndertakingDocumentView = a.PHDUndertakingDocument;
                        faculty.PhdUndertakingDocumentText = a.PhdUndertakingDocumentText;
                        //faculty.AppliedPAN = a.AppliedPAN != null ? (bool)(a.AppliedPAN) : false;
                        //faculty.SamePANUsedByMultipleFaculty = a.SamePANUsedByMultipleFaculty != null
                        //    ? (bool)(a.SamePANUsedByMultipleFaculty)
                        //    : false;
                        //faculty.Basstatus = a.BASStatusOld;
                        faculty.Deactivedby = a.DeactivatedBy;
                        faculty.DeactivedOn = a.DeactivatedOn;
                        teachingFaculty.Add(faculty);
                    }
                    else if (educationid != 6)
                    {
                        Specializationid =
                            jntuh_college_faculty_registered.Where(
                                C => C.RegistrationNumber.Trim() == a.RegistrationNumber.Trim())
                                .Select(C => C.SpecializationId)
                                .FirstOrDefault();
                        CollegeDepartmentId =
                            jntuh_college_faculty_registered.Where(
                                C => C.RegistrationNumber.Trim() == a.RegistrationNumber.Trim())
                                .Select(C => C.DepartmentId)
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
                        //faculty.Basstatus = a.BASStatus;
                        if (PrincipalstrRegNoS.Contains(a.RegistrationNumber.Trim()))
                            faculty.Principal = "Principal";
                        else
                            faculty.Principal = "";

                        faculty.PGSpecializationName = a.PGSpecialization != 0
                            ? DegreeIdNameBasedOnSpecialization.Where(e => e.Specid == a.PGSpecialization)
                                .Select(e => e.DegreeName + "-" + e.SpcializationName)
                                .FirstOrDefault()
                            : "";
                        faculty.GenderId = a.GenderId;
                        faculty.Email = a.Email;
                        faculty.facultyPhoto = a.Photo;
                        faculty.Mobile = a.Mobile;
                        faculty.PANNumber = a.PANNumber;
                        faculty.AadhaarNumber = a.AadhaarNumber;
                        faculty.isActive = a.isActive;
                        faculty.isApproved = a.isApproved;
                        faculty.department = CollegeDepartmentId > 0
                            ? jntuh_department.Where(d => d.id == CollegeDepartmentId)
                                .Select(d => d.departmentName)
                                .FirstOrDefault()
                            : "";
                        faculty.SamePANNumberCount =
                            jntuh_registered_faculty.Where(f => f.PANNumber == a.PANNumber).ToList().Count;
                        faculty.SameAadhaarNumberCount =
                            jntuh_registered_faculty.Where(f => f.AadhaarNumber == a.AadhaarNumber).ToList().Count;
                        faculty.SpecializationIdentfiedFor = Specializationid > 0
                            ? DegreeIdNameBasedOnSpecialization.Where(S => S.Specid == Specializationid)
                                .Select(S => S.DegreeName + "-" + S.SpcializationName)
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
                        //faculty.MultipleReginSamecoll = a.MultipleRegInSameCollege != null
                        //    ? (bool) a.MultipleRegInSameCollege
                        //    : false;
                        faculty.XeroxcopyofcertificatesFlag = a.Xeroxcopyofcertificates != null
                            ? (bool)a.Xeroxcopyofcertificates
                            : false;
                        faculty.NotIdentityFiedForAnyProgramFlag = a.NotIdentityfiedForanyProgram != null
                            ? (bool)a.NotIdentityfiedForanyProgram
                            : false;
                        faculty.NOrelevantUgFlag = a.NoRelevantUG == "No" ? false : true;
                        faculty.NOrelevantPgFlag = a.NoRelevantPG == "No" ? false : true;
                        faculty.NOrelevantPhdFlag = a.NORelevantPHD == "No" ? false : true;
                        //faculty.NoForm16Verification = a.Noform16Verification !=null ? (bool)a.Noform16Verification : false;
                        faculty.NoSCM17Flag = a.NoSCM17 != null ? (bool)a.NoSCM17 : false;
                        //faculty.PhotocopyofPAN = a.PhotoCopyofPAN != null ? (bool) a.PhotoCopyofPAN : false;
                        faculty.PhdUndertakingDocumentstatus = a.PhdUndertakingDocumentstatus != null
                            ? (bool)(a.PhdUndertakingDocumentstatus)
                            : false;
                        faculty.PHDUndertakingDocumentView = a.PHDUndertakingDocument;
                        faculty.PhdUndertakingDocumentText = a.PhdUndertakingDocumentText;
                        //faculty.AppliedPAN = a.AppliedPAN != null ? (bool) (a.AppliedPAN) : false;
                        //faculty.SamePANUsedByMultipleFaculty = a.SamePANUsedByMultipleFaculty != null
                        //  ? (bool) (a.SamePANUsedByMultipleFaculty)
                        //: false;
                        //faculty.Basstatus = a.BASStatusOld;
                        faculty.Deactivedby = a.DeactivatedBy;
                        faculty.DeactivedOn = a.DeactivatedOn;
                        teachingFaculty.Add(faculty);
                    }
                }
                teachingFaculty = teachingFaculty.Where(m => m.isActive == true).OrderBy(f => f.updatedOn).ThenBy(f => f.department).ToList();
                //|| item.NoForm16Verification == true ||item.NOForm16 == true ||
                ViewBag.TotalFaculty = teachingFaculty.Count();
                ViewBag.FlagTotalFaculty = teachingFaculty.Where(item =>
                            item.Absent == true ||
                            item.NOTQualifiedAsPerAICTE == true ||
                            item.InCompleteCeritificates == true || item.MultipleReginSamecoll == true ||
                             item.NOrelevantUgFlag == true ||
                            item.NOrelevantPgFlag == true || item.NOrelevantPhdFlag == true ||
                            item.NotIdentityFiedForAnyProgramFlag == true ||
                            item.NoSCM17Flag == true || item.InvalidPANNo == true ||
                            item.DegreeId < 4 || item.PANNumber == null ||
                            item.BlacklistFaculty == true || item.Type == "Adjunct" ||
                            item.AppliedPAN == true || item.SamePANUsedByMultipleFaculty == true || item.XeroxcopyofcertificatesFlag == true
                           || (item.PhdUndertakingDocumentstatus == false)
                            || item.Basstatus == "N").Select(e => e).Count();
                ViewBag.ClearFaculty = ViewBag.TotalFaculty - ViewBag.FlagTotalFaculty;
                return View(teachingFaculty);
            }

            return View(teachingFaculty);
        }

        #region 420 Faculty Verification for clearance added by Narayana Reddy on 31-12-2019

        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification,DataEntry")]
        public ActionResult Facultyverificationfouretwozero(int? collegeid)
        {
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            //int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            //List<int> userRoles = db.my_aspnet_usersinroles.Where(u => u.userId == userID).Select(u => u.roleId).ToList();
            //if (userRoles.Contains(
            //                            db.my_aspnet_roles.Where(r => r.name.Equals("Admin"))
            //                                .Select(r => r.id)
            //                                .FirstOrDefault()))
            //{
            //    ViewBag.Colleges =
            //       db.jntuh_college.Join(db.jntuh_college_edit_status, co => co.id, e => e.collegeId,
            //           (co, e) => new { co = co, e = e })
            //           .Where(c => c.e.IsCollegeEditable == false)
            //           .Select(c => new { collegeId = c.co.id, collegeName = c.co.collegeCode + "-" + c.co.collegeName })
            //           .GroupBy(c => new { c.collegeId, c.collegeName }).Select(i => new { collegeId = i.Key.collegeId, collegeName = i.Key.collegeName })
            //           .OrderBy(c => c.collegeName)
            //           .ToList();
            //}
            //else
            //{
            //    int AcademicYear = db.jntuh_academic_year.Where(a => a.isPresentAcademicYear == true).Select(a => a.id).FirstOrDefault();
            //    int InspectionPhaseId = db.jntuh_inspection_phase.Where(p => p.isActive == true && p.inspectionPhase == "Data Entry").Select(p => p.id).SingleOrDefault();
            //    int[] assignedcollegeslist =
            //        db.jntuh_dataentry_allotment.Where(
            //            d =>
            //                d.InspectionPhaseId == InspectionPhaseId && d.userID == userID && d.isActive == true &&
            //                d.isCompleted == false).Select(s => s.collegeID).ToArray();
            //    ViewBag.Colleges =
            //        db.jntuh_college.Join(db.jntuh_college_edit_status, co => co.id, e => e.collegeId,
            //            (co, e) => new { co = co, e = e })
            //            .Where(c => c.e.IsCollegeEditable == false && c.e.academicyearId == AcademicYear + 1 && assignedcollegeslist.Contains(c.e.collegeId))
            //            .Select(c => new { collegeId = c.co.id, collegeName = c.co.collegeCode + "-" + c.co.collegeName })
            //            .GroupBy(c => new { c.collegeId, c.collegeName }).Select(i => new { collegeId = i.Key.collegeId, collegeName = i.Key.collegeName })
            //            .OrderBy(c => c.collegeName)
            //            .ToList();

            //}



            var colleges = db.jntuh_college.Join(db.jntuh_college_edit_status, co => co.id, e => e.collegeId, (co, e) => new { co = co, e = e }).Where(c => c.co.isActive == true && c.e.academicyearId == prAy && c.e.IsCollegeEditable == false).Select(c => new { collegeId = c.co.id, collegeName = c.co.collegeCode + "-" + c.co.collegeName }).OrderBy(c => c.collegeName).ToList();

            colleges.Add(new { collegeId = 375, collegeName = "TEST COLLEGE" });
            //colleges.Add(new { collegeId = 0, collegeName = "Not Clear PHD Faculty" });
            ViewBag.Colleges = colleges;
            //ViewBag.Colleges = db.jntuh_college.Join(db.jntuh_college_edit_status, co => co.id, e => e.collegeId, (co, e) => new { co = co, e = e }).Where(c => c.co.isActive == true).Select(c => new { collegeId = c.co.id, collegeName = c.co.collegeCode + "-" + c.co.collegeName }).OrderBy(c => c.collegeName).ToList();

            //int[] EditFacultycollegeIds = {4,5,7,8,9,12,20,22,23,24,26,27,29,30,32,35,38,39,40,41,42,43,44,47,48,55,56,58,68,69,70,72,74,75,77,79,80,81,84,85,86,87,88,91,97,100,103,105,106,107,108,109,110,111,116,117,118,119,121,123,127,128,129,130,132,134,135,137,138,139,141,143,144,145,147,148,150,152,153,155,156,157,158,161,162,163,164,165,166,168,171,172,173,174,175,176,177,178,179,180,181,183,184,185,187,189,192,193,195,196,198,202,203,206,210,211,213,214,215,217,218,222,223,225,227,228,234,235,236,241,242,244,245,246,249,250,254,256,260,261,262,264,266,267,271,273,276,282,287,290,291,292,295,296,298,299,300,302,304,307,309,310,313,315,316,318,319,321,322,324,327,329,334,335,336,349,350,352,353,355,360,364,365,366,367,368,369,370,373,374,376,380,382,384,385,389,391,392,393,399,400,411,414,420,421,423,424,428,435,436,439,441,455,375};
            //ViewBag.Colleges =
            //    db.jntuh_college.Where(c => EditFacultycollegeIds.Contains(c.id))
            //        .Select(a => new {collegeId = a.id, collegeName = a.collegeCode + "-" + a.collegeName})
            //        .OrderBy(r => r.collegeName)
            //        .ToList();
            ViewBag.collegeid = collegeid;
            var jntuh_department = db.jntuh_department.ToList();
            List<FacultyRegistration> teachingFaculty = new List<FacultyRegistration>();

            List<jntuh_appeal_faculty_registered> appealphdfaculty = new List<jntuh_appeal_faculty_registered>();

            if (collegeid != null)
            {
                var jntuh_appeal_faculty_registered =
                    db.jntuh_appeal_faculty_registered.Where(r => r.academicYearId == prAy && r.collegeId == collegeid).Select(s => s).ToList();

                var DegreeIdNameBasedOnSpecialization = (from a in db.jntuh_department
                                                         join b in db.jntuh_specialization on a.id equals b.departmentId
                                                         join c in db.jntuh_degree on a.degreeId equals c.id
                                                         select new
                                                         {
                                                             DegreeId = c.id,
                                                             DegreeName = c.degree,
                                                             SpcializationName = b.specializationName,
                                                             Specid = b.id
                                                         }).ToList();

                var phDCSEFacultyRegnoS = new string[] { "3586-220517-222453", "0437-230414-125856", "0847-160306-111850", "2448-150408-160802", "1067-150412-120929", "31150406-132701", "38150407-100552", "14150406-115825", "63150407-122903", "81150406-145946", "34150407-114655", "1048-230413-095805", "2788-190830-155540", "40150407-124112", "2265-160303-152641", "62150401-111449", "70150406-140728", "8075-150421-103605", "37150404-195953", "3312-150413-135700", "9456-201222-150724", "3113-150409-121939", "4046-150422-161638", "1260-170102-141915", "8284-150408-103817", "4854-180201-161447", "5629-150408-152930", "1858-150408-111415", "20150406-160630", "4667-150409-132641", "5026-150408-103332", "7247-160125-111447", "88150406-111907", "4388-230418-013203", "6524-150409-125614", "9633-230415-120056", "7194-220616-153456", "7674-220616-214557", "4189-180201-202900", "2054-200307-145918", "5778-220705-164630", "8226-230418-015938", "7624-170203-154510", "8348-170202-174618", "6797-150424-233740", "4886-180417-153108", "7309-210704-195107", "9033-220708-093419", "2557-230420-230320", "3908-230420-233916", "3290-221212-174655", "7286-180210-122930", "8683-220709-123511", "5628-230413-141006", "4275-200226-063531", "2874-221214-233021", "5140-220711-124204", "9768-220711-123037", "0625-220708-154749", "0126-230418-112659", "90150406-144953", "6932-150410-121534", "8111-150410-141910", "2893-161208-165311", "8160-160224-102406", "8700-180421-163805", "7556-180710-001234", "9446-150521-080533", "6476-180420-152801", "0909-220623-233842", "04150403-181817", "3657-211027-150514", "2919-170120-152602", "9425-210409-110217", "6193-220404-144046", "2223-180820-104921", "09150328-171552", "98150403-213601", "40150405-171110", "07150407-000328", "7202-160213-113619", "7246-150413-122929", "8283-230413-170720", "5006-200306-123244", "1401-220707-095101", "9313-150425-170758", "0180-230413-111326", "4004-200229-125904", "5034-220713-143929", "5592-150506-144923", "0803-190117-174335", "1149-230219-153945", "0640-190827-152900", "3009-220712-165639", "7280-190111-141821", "9620-190828-014552", "6360-190111-210943", "5943-160311-155419", "8000-220307-154232", "1917-161221-152404", "6719-180703-130904", "1916-210702-094641", "2767-170523-225515", "6515-230207-112044", "1985-220712-114709", "0916-230419-162226", "9610-150420-152409", "5581-150416-130218", "4256-230419-183126", "42150406-152311", "8487-150506-145736", "7376-150502-153240", "86150404-182337", "2024-230408-115442", "1878-150413-133952", "9455-170914-160054", "8992-150413-104644", "7558-150408-191150", "2489-210408-153400", "9349-160310-130516", "9926-220708-115754", "7100-220721-163844", "7349-210702-144407", "8184-151216-193044", "5874-220721-161205", "5504-220708-143008", "1157-150416-114851", "01150331-125001", "8707-170913-114035", "47150404-105431", "5279-150413-112800", "7060-221213-094845", "4945-170115-145425", "71150402-153329", "2793-150410-105925", "1575-180708-000900", "0930-230324-184630", "49150407-163413", "1409-230413-145610", "9412-230106-201847", "15150407-154347", "87150407-152353", "7624-160311-164633", "31150406-195402", "2334-201104-191233", "1578-150505-135926", "4101-150504-170027", "9146-160227-100628", "26150404-152131", "8859-220509-121014", "68150404-113848", "4342-170111-091417", "3825-160313-150058", "3432-170129-072851", "1423-150425-160847", "0593-220309-121855", "8963-150416-154655", "5766-180711-111724", "0467-180716-115522", "4303-200312-165950", "85150407-144437", "9314-230310-195744", "2982-170523-172149", "8823-230324-133213", "8184-150418-160647", "9675-220212-135822", "7380-150416-113540", "2077-190830-153940", "3191-230413-095416", "1708-211231-142710", "47150406-142655", "8238-230127-103655", "5193-150411-173339", "56150406-152601", "5924-170520-140225", "3559-160225-133612", "0522-221213-121701", "1553-200201-164756", "9090-210908-101101", "3240-160226-113058", "2529-161028-120912", "0058-220118-105740", "6834-220511-130050", "0682-230331-114743", "10150404-110423", "8066-230421-151454", "8404-150408-185212", "65150402-093629", "9871-210401-144301", "5845-220125-150803", "3417-230415-122309", "9764-150420-120719", "8146-220124-151730", "4000-191108-124634", "1778-161022-132131", "9807-150412-180800", "7490-170110-120834", "9725-210407-100721", "1435-150408-162555", "3471-150416-145955", "0408-150408-135149", "3707-150416-164020", "8143-171221-170212", "1979-210415-205309", "1196-150409-212536", "72150404-140928", "2938-220408-131445", "5410-150409-093231", "15150407-113619", "6771-150408-123607", "08150407-114657", "5233-150408-152752", "1382-170521-153441", "6286-171204-120854", "51150404-160046", "5436-170915-110802", "2057-221215-153046", "6393-190118-112312", "65150331-131547", "75150406-114801", "5411-150409-114451", "0229-150508-113304", "3366-210503-145402", "4274-150408-160332", "41150407-120751", "7211-161230-103239", "2015-150408-165022", "48150406-143321", "2674-220524-112545", "73150407-174313", "4351-150414-115355", "3834-150408-150824", "9208-150408-104650", "2061-150408-171446", "92150402-163535", "9493-170103-124912", "7669-150408-131725", "7068-150504-162456", "05150331-195447", "8421-150408-165831", "61150327-133556", "0807-150408-125629", "32150405-110127", "1417-150408-163905", "98150406-154041", "7272-230415-194256", "7775-160220-122024", "1840-221215-175536", "8253-221215-153249", "8071-160311-115646", "8620-210706-224234", "5794-180707-111324", "3113-190831-091134", "8451-150415-192421", "8636-150410-094744", "3999-150413-154939", "2274-230416-174558", "2658-210702-135822", "3027-150412-221354", "3767-230413-122421", "8159-230415-223719", "0101-190309-173648", "2753-221213-143724", "5620-210706-170547", "1739-230416-230544", "0147-210702-145314", "1089-150409-125700", "2233-230318-122847", "4969-150417-194423", "2099-230419-115159", "6457-221213-140914", "2062-230416-195537", "3933-230416-184019", "2962-230417-224341", "03150401-155434", "0518-210702-120726", "40150406-150901", "70150407-142347", "8054-150408-104547", "5065-161231-130124", "06150407-131623", "5409-230413-150657", "08150407-134210", "47150402-104501", "20150401-191342", "84150407-122120", "8938-211021-102416", "79150401-140136", "32150406-121437", "76150404-141308", "72150404-171147", "8136-170120-132321", "8007-150413-153051", "4594-170120-121702", "54150407-114314", "90150407-123018", "6030-220707-123348", "6093-220708-133646", "0810-180117-120758", "3329-180117-113104", "0672-170120-151323", "40150402-170443", "5256-150420-121814", "0930-170522-161350", "5483-170213-152810", "0492-150409-004339", "7348-220927-104735", "9265-150408-170256", "1348-200722-122624", "1264-160129-123953", "6360-220817-153752", "78150406-160752", "01150406-161824", "8982-211206-130127", "6508-210201-130534", "6576-200302-114816", "18150406-165741", "9213-170131-224323", "9056-210707-143302", "1793-150407-210245", "7742-150417-153242", "1400-150407-222024", "5456-171113-110854", "5423-211227-140404", "2892-160320-155417", "42150406-160745", "7900-200703-114413", "9210-160213-101450", "5585-150417-144825", "62150406-101224", "1330-190108-123748", "07150330-103619", "9749-221029-111653", "5422-190104-115119", "26150407-115032", "0083-150414-175701", "9127-220124-192005", "43150331-121811", "27150331-122933", "85150331-125201", "25150331-123716", "21150330-130127", "17150401-154407", "94150331-225018", "56150331-142915", "9459-150424-091752", "01150330-195657", "97150407-113543", "55150330-122417", "2705-150506-170154", "24150330-154256", "2801-170129-084632", "4590-160223-102817", "9770-160122-191115", "48150403-102432", "9624-150409-144855", "57150330-225202", "4591-210702-004559", "23150406-114252", "4135-211216-110738", "3630-161103-141908", "8702-221215-132950", "3649-150409-130542", "36150404-145345", "18150407-163929", "2600-160213-141859", "5856-220707-110421", "2080-170213-111855", "8199-150412-110326", "9091-150409-165116", "3659-210907-113751", "6018-230417-132811", "8139-150408-053912", "0404-230412-210912", "3769-150506-162037", "2739-150411-161718", "0804-170913-180210", "7907-210701-155848", "94150406-181726", "5559-150408-203132", "3843-230329-125756", "25150407-125834", "5985-211211-145502", "84150404-231555", "07150404-164617", "6848-160123-103023", "9237-220708-155641", "8198-221213-154207", "9031-230401-102420", "22150404-160405", "5542-150411-135735", "5525-230329-151915", "4545-210130-152711", "5146-230412-203459", "3628-230404-131800", "1207-220506-111527", "5880-230214-103556", "97150404-122618", "2580-230329-120633", "5084-220708-121847", "0977-150412-170443", "3570-230412-194922", "4451-230412-184744", "5757-170126-090739", "6219-160308-144721", "4893-180417-191036", "3565-150418-233552", "49150406-153408", "59150407-120930", "2110-220708-152431", "8815-210703-100909", "0750-150424-182028", "5803-150418-161742", "90150402-113500", "0033-150419-144018", "0192-220709-114509", "8220-220125-120219", "2991-220707-143512", "7280-230414-143452", "9098-230421-120237", "4352-220704-155101", "5709-150409-170617", "9414-221227-111029", "5507-211204-141447", "72150407-124749", "64150407-132853", "24150406-210028", "4538-170106-121618", "2143-220401-124105", "41150403-225919", "6855-150408-162214", "4600-211218-095011", "0767-211220-212927", "4799-220409-114718", "52150403-142324", "3725-220418-124625", "6046-220708-143555", "97150403-125404", "4284-230415-165934", "1025-210702-183829", "3954-220430-130712", "0655-220326-065311", "11150403-115927", "34150328-164129", "0918-221206-174620", "9280-150407-230817", "9080-210707-143045", "08150330-130321", "5362-210216-150833", "45150331-131933", "65150330-234021", "6869-220325-115118", "6078-210804-122121", "3950-211031-234506", "8249-220503-142045", "83150331-223450", "1239-220908-102721", "19150403-074232", "0037-150418-151850", "2315-150414-144832", "5331-220615-161458", "6098-230328-135550", "9907-150408-093545", "8388-160209-104546", "7915-220704-131830", "5363-150508-174515", "50150405-134030", "1942-230115-100858", "5954-221018-220006", "88150404-175659", "9860-190202-110439", "0139-230331-143824", "3213-150412-193500", "2374-190608-132310", "29150404-122402", "0640-150412-202441", "6270-211018-134149", "2054-160219-143542", "96150406-213444", "8127-150409-121259", "9755-210702-142525", "09150405-095951", "0889-181117-154851", "5108-211013-152200", "3855-150409-103518", "2857-190727-154209", "4284-210715-140835", "3417-211127-133608", "9372-220706-143954", "3532-170208-183343", "2690-230415-061029", "9839-230415-125052", "1483-150407-211331", "0667-230413-225655", "4883-160216-152019", "01150407-105020", "09150407-095439", "4830-181106-124446", "64150406-195102", "5953-170916-153156", "43150407-021708", "0679-230303-214451", "7370-230414-111809", "5414-150414-135918", "1201-150408-143445", "3837-150428-105553", "4044-230414-124956", "5522-230415-111314", "0013-150506-152758", "6260-170520-125955", "92150402-123218", "45150402-114200", "57150406-141835", "8148-170523-140155", "04150406-151001", "9671-160227-114642", "14150406-122433", "6412-160306-212653", "6490-150430-104902", "3997-230413-114647", "34150407-114017", "72150407-114851", "8519-150410-102508", "2611-170207-130847", "4918-211123-220422", "9473-230412-212534", "54150402-145851", "71150404-150809", "3817-150415-142151", "87150406-111335", "1492-230417-190657", "8878-150412-135054", "59150407-184559", "5624-230210-165435", "6833-150412-134117", "5603-150410-113109", "3149-170109-134623", "62150404-132410", "8122-220511-135628", "7025-221125-120015", "52150404-154809", "20150407-101409", "2339-230413-111603", "17150404-174505", "9810-210707-105819", "34150402-143615", "66150403-201447", "8201-170111-130936", "0423-220125-113115", "9258-170522-150215", "4950-201119-193912", "0791-150410-131235", "0563-210805-132955", "31150404-215301", "3337-221213-131359", "5274-210707-123921", "3486-230420-172350", "4339-170201-151843", "7310-190506-151820", "9375-220425-170131", "4072-150412-122633", "2601-150504-153704", "50150406-095500", "44150407-122156", "27150402-172445", "1605-230420-120359", "42150406-162206", "53150404-202509", "1150-210330-134313", "6742-150425-173453", "1638-220328-154131", "61150405-225858", "1791-150417-215632", "39150406-191623", "47150406-130646", "5370-180804-121011", "60150406-160447", "1332-170201-103806", "78150406-161537", "92150406-110955", "9956-230420-132738", "94150406-002509", "2325-190116-162356", "0329-150409-130505", "04150407-044411", "5118-170106-120828", "67150406-000144", "8968-161217-114914", "4898-150409-150843", "11150406-151423", "4215-161223-144848", "8299-150408-125903", "93150403-181342", "95150402-110343", "5486-200307-101919", "30150406-153413", "2487-150414-161444", "3011-150506-143432", "86150326-210611", "4973-180827-130605", "30150331-124825", "8943-160311-151341", "3176-210706-130626", "7048-160122-120440", "1251-150409-123859", "8026-150422-153205", "41150404-145739", "53150402-155157", "7930-150408-171351", "7065-200307-104237", "8396-220125-135852", "8947-160301-111330", "7314-220704-170801", "2257-150428-055331", "7950-210316-152955", "3273-230401-152123", "1824-170915-142214", "3177-230418-154012", "41150331-122508", "5323-230119-104413", "3939-221212-154538", "55150404-103154", "7218-230413-133707", "4838-230417-143337", "8985-230328-122701", "4257-230413-142052", "8810-230413-112757", "7788-230419-124813", "31150406-154834", "4748-180201-165122", "15150406-154237", "92150405-131642", "1331-170916-134133", "00150401-175140", "2262-150411-165819", "3809-220630-155627", "95150403-121234", "13150402-131801", "0033-150619-222823", "6713-180702-185243", "91150402-151621", "53150402-102135", "78150402-143344", "2986-170914-154133", "7915-170104-140515", "97150405-044942", "43150402-132102", "02150403-220700", "1151-190827-130616", "1227-230413-104158", "9780-170210-000443", "92150405-183642", "8852-150416-112530", "0819-180208-214939", "66150404-121248", "1752-150410-144814", "23150404-113644", "44150404-122732", "2658-220516-122407", "2838-180208-162017", "8985-150415-160711", "8023-220711-232747", "5077-230415-103704", "5092-230316-231723", "7748-230420-193358", "0887-200307-112312", "3371-230413-204535", "8223-160127-135325", "1778-230419-142351", "7565-160105-105513", "5593-230417-121506", "67150407-152411", "1825-150415-215719", "4054-160304-104549", "1241-161228-144032", "3127-170126-034042", "5678-211218-113536", "8183-150409-115412", "0404-220705-115823", "72150407-154904", "5567-150506-162147", "3706-220627-144854", "9680-230415-225855", "35150402-133204", "6562-150410-163233", "8344-220705-134405", "6546-221130-142909", "3469-191113-143254", "0332-230413-151641", "1147-211117-110857", "5113-230418-082637", "0986-180204-074254", "31150407-153724", "1887-170201-164245", "7467-150420-112157", "1020-180802-123119", "3427-180421-145309", "4522-220719-121321", "41150406-010905", "15150402-121149", "44150330-151942", "9552-171124-165313", "6618-220620-160041", "8094-210704-224540", "3261-220707-113855", "4728-180202-082643", "0136-230413-153657", "2784-161027-112616", "2659-150507-124116", "4481-200307-154131", "6213-161022-184405", "5554-150417-164101", "4943-191221-155334", "4346-210917-161144", "4682-220523-153543", "1009-230419-033636", "9162-221214-134254", "74150406-164908", "5249-230413-163325", "3404-190807-141310", "8004-160217-114406", "7499-160217-160855", "1411-221221-075658", "3938-210705-110108", "3214-220506-161520", "3652-230413-195408", "96150401-142235", "8520-230314-153528", "9475-221215-142819", "0717-210811-110645", "4553-210503-125023", "7685-220915-131909", "9767-230413-134109", "3482-221215-164646", "8656-210811-121918", "4569-230413-161540", "6077-230413-110517", "0431-230413-174722", "3112-230413-124332", "6361-170913-133058", "7759-211218-102946", "92150406-102807", "2290-230413-150404", "3475-220615-140526", "0648-210707-153814", "1239-160307-230546", "7236-220324-124530", "5028-230413-125632", "2963-220607-144053", "0593-220701-094149", "9731-170208-114736", "2164-220401-141419", "2335-210308-175207", "3162-180415-193439", "8792-211220-125548", "30150406-124528", "3110-150410-151304", "1636-170117-152946", "3062-220701-145530", "4729-210326-125312", "1567-150409-140203", "4476-170208-192134", "5526-150422-175939", "4304-150411-114558", "4678-220706-111500", "7420-150412-131817", "2831-150420-104922", "6495-190117-135331", "3153-191118-161528", "0389-210921-192038", "84150404-154037", "3510-170111-130947", "52150331-225205", "0728-151223-143240", "4877-170116-111735", "02150404-110529", "1060-190505-122821", "4019-190220-143933", "42150331-103847", "6641-150419-140603", "7086-160106-175848", "2773-170109-102839", "4096-150408-140216", "0446-220708-150430", "0748-230413-144010", "4776-230421-101758", "8368-171216-095047", "7666-160312-135345", "9704-161210-115658", "3454-210301-164918", "2097-150413-125436", "3865-150420-172621", "8242-220120-162405", "1975-220708-111331", "9206-190827-135738", "0255-150418-113623", "1974-150409-111257", "5664-221213-095646", "33150407-170334", "8819-220122-141422", "31150406-122204", "4581-230415-180505", "71150407-123429", "2493-150427-171413", "9840-160309-121032", "7424-180131-172714", "9656-230417-191412", "61150406-114556", "5368-220720-150402", "4369-150411-134640", "66150406-154147", "8442-150409-152908", "3659-150418-093303", "2084-230413-170047", "09150407-104042", "3649-220926-141746", "5196-150408-124819", "7227-190119-144342", "6284-230203-164917", "0384-220917-122756", "5761-230415-160424", "3819-220325-112843", "5203-230415-131806", "23150401-141529", "3563-220701-101042", "7485-220326-152830", "5592-170914-103508", "64150406-155958", "6907-161214-151021", "9616-210702-095306", "3732-220702-123330", "3131-170126-100506", "9526-170211-114843", "0875-170125-122325", "6111-170128-114618", "21150406-131425", "3315-150413-155000", "40150401-151126", "5951-220121-115109", "2160-220328-160930", "1726-210213-223338", "1745-150419-203504", "6515-220514-142318", "0506-150413-163849", "3094-171013-095909", "5375-230415-142118", "1908-230413-101827", "5562-230415-123329", "9459-230413-105131", "8479-230415-154829", "8698-221215-161718", "4127-200227-150406", "0414-230413-112338", "92150407-131451", "31150407-114759", "1044-230413-155315", "7295-230413-152607", "8245-230415-112420", "2994-150505-150403", "49150404-131205", "9999-230304-101345", "83150404-105506", "8456-150408-111033", "4536-150420-162837", "0306-230420-195437", "5304-230420-125837", "8365-160223-154042", "3603-230420-142719", "6042-230420-133335", "5979-180102-143239", "0513-220706-111540", "2131-220701-153341", "5076-220610-112954", "0631-211103-123839", "0096-211218-104109", "5080-230415-161505", "5869-181122-215853", "9339-220715-092955", "0123-230415-111239", "9696-220709-154322", "1396-230414-162604", "5182-220713-101445", "1476-230421-145757", "7632-230414-150201", "1746-211213-153634", "2358-230415-115449", "4927-150412-150421", "9454-150411-155023", "0402-211209-130241", "1735-221213-170430", "1666-150506-114739", "05150407-113010", "2284-220706-142327", "0147-211207-132515", "7378-230413-132627", "6019-211207-133222", "77150405-171116", "7080-230415-134814", "5573-230403-155348", "4162-230413-182003", "8368-150410-144017", "5483-150410-132548", "6775-170916-094129", "3014-170521-204708", "41150407-182401", "16150401-123810", "9914-150414-143319", "6945-230418-114803", "0848-150411-171128", "3685-160302-190451", "3603-220705-061009", "6302-170120-134008", "00150404-110336", "5357-221213-181525", "08150407-120355", "9321-160311-163716", "2699-150418-110601", "41150401-140203", "9887-230413-101915", "2320-170120-140821", "02150406-115356", "19150331-095841", "9075-210701-154408", "7880-230411-121609", "8174-230411-142908", "0645-230413-094742", "6498-150424-105331", "1489-200303-001845", "1887-211102-183840", "7051-220707-101242", "9693-220706-162933", "07150407-145644", "0238-160206-162519", "9190-220705-171631", "1168-150413-124139", "7074-150507-151336", "4382-150427-200333", "6930-211217-215458", "5950-150409-134403", "38150406-074116", "8197-150412-144118", "0009-150410-223407", "5721-191108-151313", "7997-220707-181058", "0939-150410-114808", "1005-150407-195347", "0716-150414-114048", "5027-150422-193636", "71150402-130245", "7269-190314-115515", "4182-220708-105359", "8206-220705-121158", "4264-221122-131130", "1723-220516-103443", "92150407-151844", "46150403-175919", "4408-220702-153043", "9715-220724-201244", "17150404-114628", "14150406-160403", "5218-150420-155112", "5244-150411-130734", "06150402-153954", "7948-150623-175333", "4853-150408-130020", "1218-150410-131137", "6582-150413-195902", "7331-211213-113625", "1183-220117-125159", "9224-150414-155523", "6549-230414-134152", "38150406-222536", "2266-200307-145643", "3013-220708-113623", "5418-221215-144233", "76150404-143402", "3014-221215-112708", "6277-150413-145628", "29150407-193525", "4553-160527-080305", "2497-230421-053841", "9708-220703-191840", "4246-221213-130244", "4153-180818-150148", "82150405-193542", "90150406-150306", "90150406-143740", "8391-150410-144417", "4680-161104-140308", "4232-220513-134844", "7278-170101-130913", "8594-170914-131547", "05150407-141345", "6447-161229-061433", "5050-150409-132929", "5740-161130-143527", "3002-161130-102007", "18150407-103916", "05150406-162148", "3554-180202-123255", "65150406-145928", "2726-150408-010119", "85150403-230538", "02150406-222518", "7577-210706-125910", "96150402-173609", "6791-230404-143412", "7788-161025-163652", "6434-220705-111833", "28150404-123835", "41150331-164228", "47150406-130751", "1577-220620-142249", "76150330-131131", "3713-230401-122201", "4114-160219-170157", "3152-150416-124600", "44150401-111032", "1718-150411-170214", "6790-150426-113713", "4858-150418-102518", "22150407-162731", "2774-150501-112023", "12150331-233017", "0032-150420-203917", "19150331-203000", "3332-230413-125546", "8229-150411-123257", "9850-230415-144930", "9332-191105-133824", "5315-160218-125111", "7581-230329-113815", "0076-211216-212102", "3160-160105-183526", "6014-220222-152741", "7695-160212-144358", "13150330-110833", "8406-210707-144629", "8427-230404-140128", "9968-211220-144430", "90150407-001154", "79150401-145846", "30150406-113957", "1358-170201-193230", "8962-230404-140515", "5165-230327-183423", "78150331-134528", "4649-150414-120316", "2729-200302-105146", "8097-170112-011609", "8570-220410-225835", "4649-230404-194122", "50150404-132622", "2198-150416-133736", "96150402-120237", "45150407-144352", "18150406-124222", "1264-220707-132821", "4724-150418-180434", "5500-230404-142324", "2370-210703-144144", "0545-180416-150913", "6935-220306-135610", "6425-230223-154020", "2869-151223-112911", "0486-210327-115103", "49150407-130505", "10150407-100312", "4088-170201-144307", "3942-190504-193853", "2488-160314-230836", "5954-150413-154957", "2134-220118-113354", "0595-160205-123208", "6186-220416-155246", "0963-211128-155429", "6758-220707-153406", "8605-161215-145006", "4235-230418-111024", "1442-160130-104102", "38150407-103331", "2811-180201-095512", "7420-180208-163241", "2988-160304-161607", "29150406-105912", "8546-150410-121030", "4052-160220-140239", "6320-150411-113145", "4507-160219-103918", "2587-160220-113810", "7686-211220-124008", "3929-150409-111821", "9210-150411-155733", "2414-150428-153457", "2115-160222-153033", "9486-211101-123857", "2216-150413-110009", "7677-180709-141517", "4013-150413-183454", "95150404-173213", "2221-210705-210619", "9840-150412-102941", "9581-160208-123530", "7106-150413-110709", "9066-150412-145957", "2653-150427-182613", "5508-210707-102625", "88150407-123130", "8585-220621-131524", "6021-150428-113036", "2021-221018-150248", "6232-230308-121650", "8354-150408-150515", "6052-220712-124259", "1881-160307-111846", "2798-170110-134150", "96150405-212107", "8166-180201-155956", "5493-190723-122200", "97150401-132313", "7066-190220-174538", "4639-210707-153934", "6472-210702-161340", "3025-190821-102751", "8039-210702-105321", "0877-190310-100805", "5305-230419-182012", "6943-150408-124330", "0562-230419-132510", "8585-150410-171752", "2311-230419-161327", "2528-150408-161603", "6283-150413-140008", "5865-191108-152439", "2041-170129-123934", "5566-150408-114948", "2324-160129-123352", "0002-150413-182843", "5814-180814-140252", "8962-171227-145054", "8384-220106-133658", "4268-210403-133310", "8986-180424-135114", "67150406-190847", "2212-171009-175947", "7563-170914-154458", "85150404-150450", "7112-150408-105413", "2618-210401-120734", "3511-220707-151903", "06150405-143206", "2997-150409-125929", "5165-150409-103527", "7126-200306-235646", "2519-230416-090413", "9442-150408-153023", "5980-230416-085009", "2927-150409-091855", "3337-150413-142359", "7552-210702-164257", "2357-200307-122902", "3052-230419-164152", "2890-150409-150716", "1716-160306-110039", "1950-211204-124118", "1622-150418-161710", "92150401-162534", "1528-160216-144536", "2879-220708-154236", "7553-170204-164547", "9637-150506-114345", "03150406-113248", "7177-220707-171620", "9251-150414-062519", "5350-211217-131632", "1219-210702-123734", "6276-220106-153951", "2884-220708-170452", "50150401-194631", "62150404-125408", "7063-230411-124056", "2271-230411-120810", "3984-200225-095516", "9492-160310-111216", "8633-210704-233607", "7395-150412-152735", "6853-211220-165925", "47150330-145627", "7760-150408-162330", "1011-150408-160814", "8934-220629-151507", "2479-230321-153419", "8612-210707-140157", "2439-200306-160524", "4196-150506-140959", "9268-210709-153035", "8426-150416-141206", "6294-150410-121458", "1669-210702-155419", "4019-160303-152651", "3298-220411-154442", "0277-150410-140710", "8175-220314-130819", "7000-150409-233654", "2151-230413-142239", "9443-220707-143412", "3036-230413-222123", "8668-220416-135131", "7218-230413-124929", "9810-221130-143013", "9611-150413-141808", "1659-160226-144252", "3733-220913-123007", "2386-150620-152649", "7172-230225-162609", "2834-200307-105539", "1581-150408-103840", "6936-150421-180539", "39150406-124038", "7576-230420-150554", "3133-150410-105936", "5378-150413-144042", "0262-211220-175412", "6286-230403-145350", "2331-150419-160011", "7323-220521-150130", "9854-230415-155940", "2091-230421-151753", "9031-230413-101035", "9817-230413-121716", "0533-230421-121432", "01150331-201207", "1353-190829-100358", "2887-220309-114521", "8682-220704-132201", "0404-220623-143423", "6818-220326-113807", "41150403-200513", "68150404-200734", "5632-150428-084201", "0508-150410-150833", "5039-230415-172404", "7197-150420-160310", "7339-220315-143958", "4379-220630-110143", "9160-170208-154839", "0324-230413-151732", "8049-211203-112031", "6640-220707-175611", "1807-230414-123539", "0868-220722-115136", "5415-220729-120128", "6994-230413-130224", "7910-150407-224557", "6527-230414-161815", "0225-150427-134156", "7663-170520-152039", "8083-150422-100415", "7196-160216-154415", "6980-150421-120201", "0956-180813-145408", "4205-200307-143722", "2579-150421-142614", "5544-220124-153951", "6097-210701-155159", "7544-220625-122726", "9861-150414-163817", "8741-150413-125346", "87150404-141052", "5213-150422-102149", "9959-180713-002900", "1320-230417-184122", "5374-190814-113459", "0474-150413-144451", "2077-150414-110113", "08150407-181138", "19150406-124310", "0833-150413-121641", "2934-220707-110300", "10150407-105549", "8643-230412-193954", "81150402-090727", "6246-150425-165730", "9330-150419-140146", "46150330-215312", "1811-220905-191726", "9716-220715-104902", "66150407-162639", "4419-161202-124849", "8628-230416-063536", "0303-230415-164021", "0405-230416-110652", "5105-230416-054744", "3634-150428-100300", "6710-230416-112114", "1912-230415-170355", "4953-220505-162024", "57150330-132408", "8989-220810-163229", "5344-211211-110656", "3267-150408-114729", "4614-200107-163009", "6543-230321-142101", "3667-220505-130518", "14150404-151146", "8755-150410-154152", "98150401-160025", "9730-160305-173544", "0141-150408-170937", "95150404-145819", "3205-150508-142938", "05150406-121220", "1625-180223-170629", "4921-220620-152150", "48150406-123316", "34150330-174403", "9691-150409-160527", "89150407-160112", "09150406-160711", "51150402-113910", "6267-210706-124118", "08150403-000542", "1287-190828-111638", "0812-220714-150212", "4929-150409-144539", "27150402-152525", "0930-220708-094544", "2470-150414-114544", "9159-200306-152110", "0125-150413-170311", "1332-191109-143627", "8983-200304-161105", "4681-220713-144339", "71150331-142633", "2999-230411-163744", "1465-150408-165446", "7700-150420-161952", "9729-220531-152627", "2074-150428-154341", "5948-220331-133235", "0792-180421-121557", "0568-220531-142628", "6122-150504-165200", "7557-191105-151233", "07150404-151424", "6371-150425-152129", "5657-150408-131643", "4543-150426-142608", "9824-150426-190619", "8661-161215-111800", "3321-161228-121552", "50150406-113223", "0562-150507-130733", "8536-151222-152431", "8412-170213-000212", "2210-170120-122301", "94150402-154930", "16150407-122941", "29150402-125950", "76150404-172259", "70150405-151757", "14150402-115407", "47150404-190607", "36150404-150808", "63150401-200449", "6878-190702-150200", "97150402-154036", "7525-150409-131915", "9175-161210-122028", "2166-180417-123901", "8601-150407-215242", "8015-150409-113940", "2610-220719-213956", "0609-230413-210854", "5746-150418-143340", "9299-161215-112342", "25150402-171632", "7603-220518-151322", "8827-160314-204132", "30150404-203404", "0489-230412-114356" };

                //418 Faculty Verification Condition 
                //string[] Copy_RegNos = db.jntuh_college_faculty_registered_copy.Where(c => c.collegeId == 375).Select(e => e.RegistrationNumber).Distinct().ToArray();

                // List<jntuh_college_faculty_registered> jntuh_college_faculty_registered_College_Portal = db.jntuh_college_faculty_registered.Where(cf => cf.collegeId == collegeid).Select(cf => cf).ToList();
                //committed by siva due to only edit option faculty is comming case....
                // jntuh_college_faculty_registered = jntuh_college_faculty_registered.Where(c => !Copy_RegNos.Contains(c.RegistrationNumber)).ToList();
                List<jntuh_college_faculty_registered> jntuh_college_faculty_registered_forphd = new List<jntuh_college_faculty_registered>();

                List<jntuh_college_faculty_registered> jntuh_college_faculty_registered = db.jntuh_college_faculty_registered.Where(cf => phDCSEFacultyRegnoS.Contains(cf.RegistrationNumber) && cf.collegeId == collegeid).Select(cf => cf).ToList();

                //419 Faculty Verifi
                string[] collegeRegnos =
                    jntuh_college_faculty_registered.Select(s => s.RegistrationNumber).Distinct().ToArray();
                //string[] Copy_RegNos = db.jntuh_college_faculty_registered_copy.Where(c => c.collegeId == 1420 && collegeRegnos.Contains(c.RegistrationNumber)).Select(e => e.RegistrationNumber).Distinct().ToArray();
                //string[] Copy_RegNos = db.jntuh_college_faculty_registered_copy.Where(c => c.collegeId == 1419 && collegeRegnos.Contains(c.RegistrationNumber)).Select(e => e.RegistrationNumber).Distinct().ToArray();
                //string[] strRegNoS = jntuh_college_faculty_registered.Where(c => !Copy_RegNos.Contains(c.RegistrationNumber)).Select(cf => cf.RegistrationNumber).ToArray();
                string[] strRegNoS = jntuh_college_faculty_registered.Select(cf => cf.RegistrationNumber).ToArray();
                string[] PrincipalstrRegNoS = db.jntuh_college_principal_registered.Where(P => P.collegeId == collegeid).Select(P => P.RegistrationNumber.Trim()).ToArray();
                List<jntuh_registered_faculty> jntuh_registered_faculty = new List<jntuh_registered_faculty>();
                ViewBag.TotalClearedFaculty = collegeRegnos.Count() - strRegNoS.Count();
                ViewBag.TotalCollegeFaculty = collegeRegnos.Count();


                if (collegeid == 375)
                {
                    string[] phdnotclearcollegeassociate = new string[]
                    {
                       "85150330-182657","06150331-111246","47150331-113435","94150331-122435","08150331-164950","75150331-181534","32150401-121708","85150401-152830","17150401-154407","82150401-193318","81150402-122531","84150402-135106","97150402-154036","76150402-165017","07150402-165852","74150403-101252","79150403-133318","73150403-160105","48150403-190656","91150403-203812","41150404-113040","12150404-154539","16150404-155121","04150404-164306","32150404-172740","29150404-175402","28150404-175504","31150404-195745","21150405-011701","51150405-112628","89150405-153610","01150405-154759","58150406-100658","92150406-102807","31150406-132701","83150406-133331","80150406-145457","66150406-154147","67150406-154925","47150406-163118","35150406-172430","12150407-102002","88150407-105745","06150407-110409","21150407-114517","39150407-121907","26150407-125656","77150407-135637","38150407-141045","39150407-142712","24150407-144012","13150407-160822","6945-150408-105112","7525-150408-124513","0254-150408-125530","5043-150408-125708","1439-150408-145337","5233-150408-152752","4587-150408-154522","0973-150408-164314","7860-150409-093946","7258-150409-102513","9420-150409-104558","3556-150409-105506","8677-150409-122457","2802-150409-125545","5685-150409-131541","0377-150409-133622","5763-150409-155228","2468-150410-103024","3591-150410-125429","0791-150410-131235","9946-150410-132838","7490-150411-125727","1955-150411-132435","9288-150413-125630","0861-150413-142903","9165-150413-144341","5958-150413-154246","1291-150413-162334","7331-150414-124517","4854-150414-125905","0104-150414-142722","8720-150414-153720","1191-150414-182314","8713-150415-223120","1898-150416-103606","5417-150416-173107","6858-150417-153743","8184-150418-160647","1136-150419-010704","8382-150419-134710","9377-150419-145913","5256-150420-121814","5614-150420-155913","6931-150421-013104","7479-150421-105533","7186-150422-130450","3119-150426-160711","3969-150427-114410","8599-150428-123056","9127-150429-155346","9828-150501-044104","1804-150504-223903","2916-150505-111623","2705-150506-170154","2351-150507-075609","9233-150507-194931","5307-150605-143306","9225-150619-210159","0183-150623-110537","9189-160112-211310","6672-160128-161546","8795-160203-175048","7783-160204-153400","1082-160210-102657","6661-160216-162733","1688-160217-160734","4744-160220-155006","5020-160223-110315","0948-160223-154330","2582-160225-154756","4320-160228-133043","4531-160229-113726","4984-160303-115900","0124-160303-185802","6631-160305-114438","2185-160306-125348","2718-160307-011834","7715-160307-094837","1881-160307-111846","7008-160307-141820","2232-160312-110412","4712-160313-143055","1581-160313-152759","3592-160313-222024","0879-160314-164629","2841-160315-000843","8250-160315-111108","3370-160315-134028","9805-160316-211552","4497-160318-164821","4825-160529-193005","7691-160804-172710","1778-161022-132131","2971-161024-163038","2770-161028-140111","6391-161101-112549","4213-161128-093740","7595-161207-140122","0089-161209-145730","7984-170103-155512","4374-170104-125349","3745-170105-143145","5118-170106-120828","4779-170106-123241","7757-170109-141614","0767-170111-102126","7111-170111-113028","0846-170111-120917","9510-170112-102057","5203-170122-115917","3830-170127-055112","8402-170130-042424","4731-170130-143850","7999-170131-184403","7050-170131-192946","1286-170131-201713","2639-170201-061450","1423-170201-231848","2951-170202-052356","8348-170202-174618","4300-170208-163059","1255-170208-172403","8673-170209-001735","2704-170520-130953","0697-170521-115623","4499-170522-103724","1539-170522-113513","5670-170522-125002","9348-170523-173650","5914-170523-202326","3040-170915-115017","3792-170915-171037","9760-170916-124258","4865-171016-113947","7148-171125-120143","9255-171220-162451","6404-171221-160659","0895-171222-113335","5089-171223-141800","7837-171225-073920","7329-171227-111349","9084-180131-155141","1603-180131-191237","4463-180201-140846","2117-180201-163235","4189-180201-202900","5990-180202-173614","9882-180203-163139","9122-180204-153030","7617-180207-150550","1747-180208-144342","0755-180208-162637","0961-180208-210002","2182-180208-233008","9447-180212-120756","1076-180220-234151","6846-180417-113033","3130-180417-121250","7594-180421-150604","9537-180421-154149","3135-180421-155425","5776-180421-155710","5702-180421-160738","9715-180703-001326","6481-180703-075013","9418-180711-145210","9959-180713-002900","1744-180713-010015","0602-180713-161550","0467-180716-115522","8650-180719-124256","3929-180721-133143","6877-180723-180939","2326-180728-155220","1451-180808-161540","4474-180809-203945","2439-180810-111213","2122-180811-132049","3747-180811-214805","5995-180811-222347","5981-180812-093328","7572-180818-112007","9455-180818-115900","5106-180818-124347","6560-180818-133117","3010-180818-140606","6570-180820-121747","2319-180820-142857","2417-180820-144805","2373-180820-194644","9887-181105-140926","4830-181106-124446","5071-181122-173530","4703-181223-121930","3568-190110-142247","7610-190119-095241","5633-190119-142303","8766-190119-161519","6370-190131-113843","6881-190201-230301","7990-190202-160519","5221-190212-125443","2442-190214-133013","6397-190216-121426","5076-190218-161729","2784-190219-171631","7679-190220-121213","3537-190220-144812","9560-190220-170729","8563-190221-154413","1040-190221-160040","9744-190221-161632","8881-190309-143540","0337-190309-154816"
                    };

                    jntuh_registered_faculty = db.jntuh_registered_faculty.Where(rf => phdnotclearcollegeassociate.Contains(rf.RegistrationNumber) && rf.Blacklistfaculy != true)
                                           .ToList();
                    jntuh_college_faculty_registered_forphd = db.jntuh_college_faculty_registered.Where(cf => phdnotclearcollegeassociate.Contains(cf.RegistrationNumber)).Select(cf => cf).ToList();
                    appealphdfaculty =
                        db.jntuh_appeal_faculty_registered.Where(
                            r => phdnotclearcollegeassociate.Contains(r.RegistrationNumber) && r.academicYearId == prAy)
                            .Select(a => a)
                            .ToList();
                    //jntuh_registered_faculty = db.jntuh_registered_faculty.Where(rf => phdnotcollegeassociate.Contains(rf.RegistrationNumber) && rf.Blacklistfaculy != true && rf.updatedOn >= editoptionStartdate)
                    //                       .ToList();
                }
                else
                {
                    jntuh_registered_faculty = db.jntuh_registered_faculty.Where(rf => strRegNoS.Contains(rf.RegistrationNumber) && rf.Blacklistfaculy != true)  //&& (rf.collegeId == null || rf.collegeId == collegeid)//&& rf.Notin116 != true
                                            .ToList();
                }



                //var jntuh_faculty = db.jntuh_registered_faculty.Where(rf => rf.Blacklistfaculy != true && rf.updatedOn >= editoptionStartdate)  //&& (rf.collegeId == null || rf.collegeId == collegeid)//&& rf.Notin116 != true
                //                            .ToList();

                var Specializations = db.jntuh_specialization.ToList();

                string RegNumber = "";
                int? Specializationid = 0;
                int? CollegeDepartmentId = 0;
                int inactivephdfaculty = 0;
                foreach (var a in jntuh_registered_faculty)
                {

                    if (collegeid == 375)
                    {

                        int educationid =
                            db.jntuh_registered_faculty_education.Where(e => e.facultyId == a.id && e.educationId == 6)
                                .Select(s => s.educationId)
                                .FirstOrDefault();

                        if (collegeid == 375)
                        {
                            string Reason = null;
                            Specializationid =
                              jntuh_college_faculty_registered_forphd.Where(
                                  C => C.RegistrationNumber.Trim() == a.RegistrationNumber.Trim())
                                  .Select(C => C.SpecializationId)
                                  .FirstOrDefault();
                            CollegeDepartmentId =
                                jntuh_college_faculty_registered_forphd.Where(
                                    C => C.RegistrationNumber.Trim() == a.RegistrationNumber.Trim())
                                    .Select(C => C.DepartmentId)
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
                            //Hear showing JNTU Specilazation only Pharmacy Faculty
                            faculty.PGSpecializationName = a.Jntu_PGSpecializationId != null
                                ? DegreeIdNameBasedOnSpecialization.Where(e => e.Specid == a.Jntu_PGSpecializationId)
                                    .Select(e => e.DegreeName + "-" + e.SpcializationName)
                                    .FirstOrDefault()
                                : "";
                            faculty.GenderId = a.GenderId;
                            faculty.Email = a.Email;
                            faculty.facultyPhoto = a.Photo;
                            faculty.Mobile = a.Mobile;
                            faculty.PANNumber = a.PANNumber;
                            faculty.AadhaarNumber = a.AadhaarNumber;
                            faculty.isActive = a.isActive;
                            faculty.isApproved = a.isApproved;
                            faculty.department = CollegeDepartmentId > 0
                                ? jntuh_department.Where(d => d.id == CollegeDepartmentId)
                                    .Select(d => d.departmentName)
                                    .FirstOrDefault()
                                : "";
                            faculty.SamePANNumberCount =
                                jntuh_registered_faculty.Where(f => f.PANNumber == a.PANNumber).ToList().Count;
                            faculty.SameAadhaarNumberCount =
                                jntuh_registered_faculty.Where(f => f.AadhaarNumber == a.AadhaarNumber).ToList().Count;
                            faculty.SpecializationIdentfiedFor = Specializationid > 0
                                ? DegreeIdNameBasedOnSpecialization.Where(S => S.Specid == Specializationid)
                                    .Select(S => S.DegreeName + "-" + S.SpcializationName)
                                    .FirstOrDefault()
                                : "";
                            faculty.isVerified = isFacultyVerified(a.id);
                            faculty.DeactivationReason = a.DeactivationReason;
                            faculty.VerificationStatus = a.AbsentforVerification;
                            faculty.updatedOn = a.updatedOn;
                            faculty.createdOn =
                                jntuh_college_faculty_registered_forphd.Where(f => f.RegistrationNumber == a.RegistrationNumber)
                                    .Select(f => f.createdOn)
                                    .FirstOrDefault();
                            faculty.IdentfiedFor =
                                jntuh_college_faculty_registered_forphd.Where(
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

                            faculty.NoClass = a.Noclass != null
                                ? (bool)a.Noclass
                                : false;
                            faculty.NotconsiderPhd = a.NotconsideredPHD != null
                               ? (bool)a.NotconsideredPHD
                               : false;
                            faculty.NoPgSpecialization = a.NoPGspecialization != null
                            ? (bool)a.NoPGspecialization
                            : false;
                            faculty.GenuinenessnotSubmitted = a.Genuinenessnotsubmitted != null
                         ? (bool)a.Genuinenessnotsubmitted
                         : false;
                            faculty.InvalidDegree = a.Invaliddegree != null
                              ? (bool)a.Invaliddegree
                              : false;
                            faculty.XeroxcopyofcertificatesFlag = a.Xeroxcopyofcertificates != null
                                ? (bool)a.Xeroxcopyofcertificates
                                : false;
                            faculty.NotIdentityFiedForAnyProgramFlag = a.NotIdentityfiedForanyProgram != null
                                ? (bool)a.NotIdentityfiedForanyProgram
                                : false;
                            faculty.InvalidAadhaar = a.InvalidAadhaar;
                            faculty.NOrelevantUgFlag = a.NoRelevantUG == "No" ? false : true;
                            faculty.NOrelevantPgFlag = a.NoRelevantPG == "No" ? false : true;
                            faculty.NOrelevantPhdFlag = a.NORelevantPHD == "No" ? false : true;
                            faculty.NoSCM = a.NoSCM != null ? (bool)a.NoSCM : false;
                            faculty.NoClass = a.Noclass != null ? (bool)a.Noclass : false;
                            faculty.InvalidDegree = a.Invaliddegree != null ? (bool)a.Invaliddegree : false;
                            faculty.NotconsiderPhd = a.NotconsideredPHD != null ? (bool)a.NotconsideredPHD : false;
                            faculty.NoPgSpecialization = a.NoPGspecialization != null ? (bool)a.NoPGspecialization : false;
                            faculty.Noclassinugorpg = a.Noclass != null
                                ? (bool)a.Noclass
                                : false;
                            faculty.GenuinenessnotSubmitted = a.Genuinenessnotsubmitted != null ? (bool)a.Genuinenessnotsubmitted : false;
                            faculty.OriginalsVerifiedUG = a.OriginalsVerifiedUG != null ? (bool)a.OriginalsVerifiedUG : false;
                            faculty.OriginalsVerifiedPHD = a.OriginalsVerifiedPHD != null ? (bool)a.OriginalsVerifiedPHD : false;
                            faculty.PhdUndertakingDocumentstatus = a.PhdUndertakingDocumentstatus != null
                                ? (bool)(a.PhdUndertakingDocumentstatus)
                                : false;
                            faculty.PHDUndertakingDocumentView = a.PHDUndertakingDocument;
                            faculty.PhdUndertakingDocumentText = a.PhdUndertakingDocumentText;
                            faculty.appealsupportingdocument =
                                appealphdfaculty.Where(r => r.RegistrationNumber == a.RegistrationNumber)
                                    .Select(s => s.AppealReverificationSupportingDocument)
                                    .FirstOrDefault();
                            if (String.IsNullOrEmpty(faculty.appealsupportingdocument))
                            {
                                faculty.appealsupportingdocument =
                               appealphdfaculty.Where(r => r.RegistrationNumber == a.RegistrationNumber)
                                   .Select(s => s.AppealReverificationScreenshot)
                                   .FirstOrDefault();
                            }

                            if (faculty.Absent == true)
                                Reason += "Absent";
                            if (faculty.Type == "Adjunct")
                            {
                                if (Reason != null)
                                    Reason += ",Adjunct Faculty";
                                else
                                    Reason += "Adjunct Faculty";
                            }
                            if (faculty.OriginalCertificatesnotshownFlag == true)
                            {
                                if (Reason != null)
                                    Reason += ",Orginal Certificates Not shown in College Inspection";
                                else
                                    Reason += "Orginal Certificates Not shown in College Inspection";
                            }
                            if (faculty.XeroxcopyofcertificatesFlag == true)
                            {
                                if (Reason != null)
                                    Reason += ",Photo copy of Certificates";
                                else
                                    Reason += "Photo copy of Certificates";
                            }
                            if (faculty.NOTQualifiedAsPerAICTE == true || faculty.DegreeId < 4)
                            {
                                if (Reason != null)
                                    Reason += ",Not Qualified as per AICTE/PCI";
                                else
                                    Reason += "Not Qualified as per AICTE/PCI";
                            }
                            if (faculty.NoSCM == true)
                            {
                                if (Reason != null)
                                    Reason += ",no/not valid SCM";
                                else
                                    Reason += "no/not valid SCM";
                            }
                            if (faculty.PANNumber == null)
                            {
                                if (Reason != null)
                                    Reason += ",No PAN Number";
                                else
                                    Reason += "No PAN Number";
                            }
                            if (faculty.InCompleteCeritificates == true)
                            {
                                if (Reason != null)
                                    Reason += ",IncompleteCertificates";
                                else
                                    Reason += "IncompleteCertificates";
                            }
                            if (faculty.BlacklistFaculty == true)
                            {
                                if (Reason != null)
                                    Reason += ",Blacklisted Faculty";
                                else
                                    Reason += "Blacklisted Faculty";
                            }
                            if (faculty.NOrelevantUgFlag == true)
                            {
                                if (Reason != null)
                                    Reason += ",NO Relevant UG";
                                else
                                    Reason += "NO Relevant UG";
                            }
                            if (faculty.NOrelevantPgFlag == true)
                            {
                                if (Reason != null)
                                    Reason += ",NO Relevant PG";
                                else
                                    Reason += "NO Relevant PG";
                            }
                            if (faculty.NOrelevantPhdFlag == true)
                            {
                                if (Reason != null)
                                    Reason += ",NO Relevant PHD";
                                else
                                    Reason += "NO Relevant PHD";
                            }
                            if (faculty.InvalidPANNo == true)
                            {
                                if (Reason != null)
                                    Reason += ",InvalidPAN";
                                else
                                    Reason += "InvalidPAN";
                            }
                            if (faculty.OriginalsVerifiedPHD == true)
                            {
                                if (Reason != null)
                                    Reason += ",No Guide Sign in PHD Thesis";
                                else
                                    Reason += "No Guide Sign in PHD Thesis";
                            }
                            if (faculty.OriginalsVerifiedUG == true)
                            {
                                if (Reason != null)
                                    Reason += ",Complaint PHD Faculty";
                                else
                                    Reason += "Complaint PHD Faculty";
                            }
                            if (faculty.InvalidDegree == true)
                            {
                                if (Reason != null)
                                    Reason += ",AICTE Not Approved University Degrees";
                                else
                                    Reason += "AICTE Not Approved University Degrees";
                            }
                            //if (faculty.BAS == "Yes")
                            //{
                            //    if (Reason != null)
                            //        Reason += ",BAS Flag";
                            //    else
                            //        Reason += "BAS Flag";
                            //}
                            if (faculty.InvalidAadhaar == "Yes")
                            {
                                if (Reason != null)
                                    Reason += ",Invalid/Blur Aadhaar";
                                else
                                    Reason += "Invalid/Blur Aadhaar";
                            }
                            if (faculty.NoClass == true)
                            {
                                if (Reason != null)
                                    Reason += ",No Class in UG/PG";
                                else
                                    Reason += "No Class in UG/PG";
                            }
                            if (faculty.VerificationStatus == true)
                            {
                                if (Reason != null)
                                    Reason += ",Absentfor Physical Verification";
                                else
                                    Reason += "Absentfor Physical Verification";
                            }
                            if (faculty.NotconsiderPhd == true)
                            {
                                if (Reason != null)
                                    Reason += ",Cosidered as a faculty but not cosidered his/her P.hD";
                                else
                                    Reason += "Cosidered as a faculty but not cosidered his/her P.hD";
                            }
                            if (faculty.NOspecializationFlag == true)
                            {
                                if (Reason != null)
                                    Reason += ",no Specialization in PG";
                                else
                                    Reason += "no Specialization in PG";
                            }
                            if (faculty.GenuinenessnotSubmitted == true)
                            {
                                if (Reason != null)
                                    Reason += ",PHD Genuinity not Submitted";
                                else
                                    Reason += "PHD Genuinity not Submitted";
                            }
                            if (faculty.DeactivationReason != null)
                                faculty.DeactivationReason += "," + Reason;
                            else
                                faculty.DeactivationReason = Reason;
                            faculty.Deactivedby = a.DeactivatedBy;
                            faculty.DeactivedOn = a.DeactivatedOn;
                            teachingFaculty.Add(faculty);
                        }
                        else if (educationid != 6)
                        {
                            string Reason = null;
                            Specializationid =
                                jntuh_college_faculty_registered.Where(
                                    C => C.RegistrationNumber.Trim() == a.RegistrationNumber.Trim())
                                    .Select(C => C.SpecializationId)
                                    .FirstOrDefault();
                            CollegeDepartmentId =
                                jntuh_college_faculty_registered.Where(
                                    C => C.RegistrationNumber.Trim() == a.RegistrationNumber.Trim())
                                    .Select(C => C.DepartmentId)
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
                            //faculty.Basstatus = a.BASStatus;
                            if (PrincipalstrRegNoS.Contains(a.RegistrationNumber.Trim()))
                                faculty.Principal = "Principal";
                            else
                                faculty.Principal = "";

                            //Hear showing JNTU Specilazation only Pharmacy Faculty
                            faculty.PGSpecializationName = a.Jntu_PGSpecializationId != null
                                ? DegreeIdNameBasedOnSpecialization.Where(e => e.Specid == a.Jntu_PGSpecializationId)
                                    .Select(e => e.DegreeName + "-" + e.SpcializationName)
                                    .FirstOrDefault()
                                : "";
                            faculty.GenderId = a.GenderId;
                            faculty.Email = a.Email;
                            faculty.facultyPhoto = a.Photo;
                            faculty.Mobile = a.Mobile;
                            faculty.PANNumber = a.PANNumber;
                            faculty.AadhaarNumber = a.AadhaarNumber;
                            faculty.isActive = a.isActive;
                            faculty.isApproved = a.isApproved;
                            faculty.department = CollegeDepartmentId > 0
                                ? jntuh_department.Where(d => d.id == CollegeDepartmentId)
                                    .Select(d => d.departmentName)
                                    .FirstOrDefault()
                                : "";
                            faculty.SamePANNumberCount =
                                jntuh_registered_faculty.Where(f => f.PANNumber == a.PANNumber).ToList().Count;
                            faculty.SameAadhaarNumberCount =
                                jntuh_registered_faculty.Where(f => f.AadhaarNumber == a.AadhaarNumber).ToList().Count;
                            faculty.SpecializationIdentfiedFor = Specializationid > 0
                                ? DegreeIdNameBasedOnSpecialization.Where(S => S.Specid == Specializationid)
                                    .Select(S => S.DegreeName + "-" + S.SpcializationName)
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
                            faculty.IdentfiedFor =
                                jntuh_college_faculty_registered.Where(
                                    f => f.RegistrationNumber.Trim() == a.RegistrationNumber.Trim())
                                    .Select(f => f.IdentifiedFor)
                                    .FirstOrDefault();
                            faculty.jntuh_registered_faculty_education = a.jntuh_registered_faculty_education;
                            faculty.DegreeId = a.jntuh_registered_faculty_education.Count(e => e.facultyId == a.id && e.educationId != 8) > 0
                                ? a.jntuh_registered_faculty_education.Where(e => e.facultyId == a.id && e.educationId != 8)
                                    .Select(e => e.educationId)
                                    .Max()
                                : 0;
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
                            faculty.GenuinenessnotSubmitted = a.Genuinenessnotsubmitted != null ? (bool)a.Genuinenessnotsubmitted : false;
                            faculty.NoClass = a.Noclass != null ? (bool)a.Noclass : false;
                            faculty.InvalidDegree = a.Invaliddegree != null ? (bool)a.Invaliddegree : false;
                            faculty.NotconsiderPhd = a.NotconsideredPHD != null ? (bool)a.NotconsideredPHD : false;
                            faculty.NoPgSpecialization = a.NoPGspecialization != null ? (bool)a.NoPGspecialization : false;
                            faculty.Noclassinugorpg = a.Noclass != null
                                ? (bool)a.Noclass
                                : false;
                            faculty.NoSCM = a.NoSCM != null ? (bool)a.NoSCM : false;

                            faculty.XeroxcopyofcertificatesFlag = a.Xeroxcopyofcertificates != null
                                ? (bool)a.Xeroxcopyofcertificates
                                : false;
                            faculty.NotIdentityFiedForAnyProgramFlag = a.NotIdentityfiedForanyProgram != null
                                ? (bool)a.NotIdentityfiedForanyProgram
                                : false;
                            faculty.InvalidAadhaar = a.InvalidAadhaar;
                            faculty.VerificationStatus = a.AbsentforVerification;
                            faculty.NOrelevantUgFlag = a.NoRelevantUG == "No" ? false : true;
                            faculty.NOrelevantUgFlag = a.NoRelevantUG == "No" ? false : true;
                            faculty.NOrelevantPgFlag = a.NoRelevantPG == "No" ? false : true;
                            faculty.NOrelevantPhdFlag = a.NORelevantPHD == "No" ? false : true;
                            faculty.PhdUndertakingDocumentstatus = a.PhdUndertakingDocumentstatus != null
                                ? (bool)(a.PhdUndertakingDocumentstatus)
                                : false;
                            faculty.GenuinenessnotSubmitted = a.Genuinenessnotsubmitted != null ? (bool)a.Genuinenessnotsubmitted : false;
                            faculty.OriginalsVerifiedUG = a.OriginalsVerifiedUG != null ? (bool)a.OriginalsVerifiedUG : false;
                            faculty.OriginalsVerifiedPHD = a.OriginalsVerifiedPHD != null ? (bool)a.OriginalsVerifiedPHD : false;
                            faculty.PHDUndertakingDocumentView = a.PHDUndertakingDocument;
                            faculty.PhdUndertakingDocumentText = a.PhdUndertakingDocumentText;
                            faculty.Deactivedby = a.DeactivatedBy;
                            faculty.DeactivedOn = a.DeactivatedOn;
                            faculty.appealsupportingdocument =
                                jntuh_appeal_faculty_registered.Where(r => r.RegistrationNumber == a.RegistrationNumber)
                                    .Select(s => s.AppealReverificationSupportingDocument)
                                    .FirstOrDefault();
                            if (String.IsNullOrEmpty(faculty.appealsupportingdocument))
                            {
                                faculty.appealsupportingdocument =
                               jntuh_appeal_faculty_registered.Where(r => r.RegistrationNumber == a.RegistrationNumber)
                                   .Select(s => s.AppealReverificationScreenshot)
                                   .FirstOrDefault();
                            }

                            if (faculty.Absent == true)
                                Reason += "Absent";
                            if (faculty.Type == "Adjunct")
                            {
                                if (Reason != null)
                                    Reason += ",Adjunct Faculty";
                                else
                                    Reason += "Adjunct Faculty";
                            }
                            if (faculty.OriginalCertificatesnotshownFlag == true)
                            {
                                if (Reason != null)
                                    Reason += ",Orginal Certificates Not shown in College Inspection";
                                else
                                    Reason += "Orginal Certificates Not shown in College Inspection";
                            }
                            if (faculty.XeroxcopyofcertificatesFlag == true)
                            {
                                if (Reason != null)
                                    Reason += ",Photo copy of Certificates";
                                else
                                    Reason += "Photo copy of Certificates";
                            }
                            if (faculty.NOTQualifiedAsPerAICTE == true || faculty.DegreeId < 4)
                            {
                                if (Reason != null)
                                    Reason += ",Not Qualified as per AICTE/PCI";
                                else
                                    Reason += "Not Qualified as per AICTE/PCI";
                            }
                            if (faculty.NoSCM == true)
                            {
                                if (Reason != null)
                                    Reason += ",no/not valid SCM";
                                else
                                    Reason += "no/not valid SCM";
                            }
                            if (faculty.PANNumber == null)
                            {
                                if (Reason != null)
                                    Reason += ",No PAN Number";
                                else
                                    Reason += "No PAN Number";
                            }
                            if (faculty.InCompleteCeritificates == true)
                            {
                                if (Reason != null)
                                    Reason += ",IncompleteCertificates";
                                else
                                    Reason += "IncompleteCertificates";
                            }
                            if (faculty.BlacklistFaculty == true)
                            {
                                if (Reason != null)
                                    Reason += ",Blacklisted Faculty";
                                else
                                    Reason += "Blacklisted Faculty";
                            }
                            if (faculty.NOrelevantUgFlag == true)
                            {
                                if (Reason != null)
                                    Reason += ",NO Relevant UG";
                                else
                                    Reason += "NO Relevant UG";
                            }
                            if (faculty.NOrelevantPgFlag == true)
                            {
                                if (Reason != null)
                                    Reason += ",NO Relevant PG";
                                else
                                    Reason += "NO Relevant PG";
                            }
                            if (faculty.NOrelevantPhdFlag == true)
                            {
                                if (Reason != null)
                                    Reason += ",NO Relevant PHD";
                                else
                                    Reason += "NO Relevant PHD";
                            }
                            if (faculty.InvalidPANNo == true)
                            {
                                if (Reason != null)
                                    Reason += ",InvalidPAN";
                                else
                                    Reason += "InvalidPAN";
                            }
                            if (faculty.OriginalsVerifiedPHD == true)
                            {
                                if (Reason != null)
                                    Reason += ",No Guide Sign in PHD Thesis";
                                else
                                    Reason += "No Guide Sign in PHD Thesis";
                            }
                            if (faculty.OriginalsVerifiedUG == true)
                            {
                                if (Reason != null)
                                    Reason += ",Complaint PHD Faculty";
                                else
                                    Reason += "Complaint PHD Faculty";
                            }
                            if (faculty.InvalidDegree == true)
                            {
                                if (Reason != null)
                                    Reason += ",AICTE Not Approved University Degrees";
                                else
                                    Reason += "AICTE Not Approved University Degrees";
                            }
                            //if (faculty.BAS == "Yes")
                            //{
                            //    if (Reason != null)
                            //        Reason += ",BAS Flag";
                            //    else
                            //        Reason += "BAS Flag";
                            //}
                            if (faculty.InvalidAadhaar == "Yes")
                            {
                                if (Reason != null)
                                    Reason += ",Invalid/Blur Aadhaar";
                                else
                                    Reason += "Invalid/Blur Aadhaar";
                            }
                            if (faculty.NoClass == true)
                            {
                                if (Reason != null)
                                    Reason += ",No Class in UG/PG";
                                else
                                    Reason += "No Class in UG/PG";
                            }
                            if (faculty.VerificationStatus == true)
                            {
                                if (Reason != null)
                                    Reason += ",Absentfor Physical Verification";
                                else
                                    Reason += "Absentfor Physical Verification";
                            }
                            if (faculty.NotconsiderPhd == true)
                            {
                                if (Reason != null)
                                    Reason += ",Cosidered as a faculty but not cosidered his/her P.hD";
                                else
                                    Reason += "Cosidered as a faculty but not cosidered his/her P.hD";
                            }
                            if (faculty.NOspecializationFlag == true)
                            {
                                if (Reason != null)
                                    Reason += ",no Specialization in PG";
                                else
                                    Reason += "no Specialization in PG";
                            }
                            if (faculty.GenuinenessnotSubmitted == true)
                            {
                                if (Reason != null)
                                    Reason += ",PHD Genuinity not Submitted";
                                else
                                    Reason += "PHD Genuinity not Submitted";
                            }

                            faculty.IsStatus = a.InStatus;
                            faculty.DeactivationReason = Reason;
                            teachingFaculty.Add(faculty);
                        }
                        if (collegeid != 375 && educationid == 6)
                        {
                            inactivephdfaculty++;
                        }
                    }
                    else
                    {
                        string Reason = null;
                        Specializationid =
                            jntuh_college_faculty_registered.Where(
                                C => C.RegistrationNumber.Trim() == a.RegistrationNumber.Trim())
                                .Select(C => C.SpecializationId)
                                .FirstOrDefault();
                        CollegeDepartmentId =
                            jntuh_college_faculty_registered.Where(
                                C => C.RegistrationNumber.Trim() == a.RegistrationNumber.Trim())
                                .Select(C => C.DepartmentId)
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
                        //faculty.Basstatus = a.BASStatus;
                        if (PrincipalstrRegNoS.Contains(a.RegistrationNumber.Trim()))
                            faculty.Principal = "Principal";
                        else
                            faculty.Principal = "";

                        //Hear showing JNTU Specilazation only Pharmacy Faculty
                        faculty.PGSpecializationName = a.Jntu_PGSpecializationId != null
                            ? DegreeIdNameBasedOnSpecialization.Where(e => e.Specid == a.Jntu_PGSpecializationId)
                                .Select(e => e.DegreeName + "-" + e.SpcializationName)
                                .FirstOrDefault()
                            : "";
                        faculty.GenderId = a.GenderId;
                        faculty.Email = a.Email;
                        faculty.facultyPhoto = a.Photo;
                        faculty.Mobile = a.Mobile;
                        faculty.PANNumber = a.PANNumber;
                        faculty.AadhaarNumber = a.AadhaarNumber;
                        faculty.isActive = a.isActive;
                        faculty.isApproved = a.isApproved;
                        faculty.department = CollegeDepartmentId > 0
                            ? jntuh_department.Where(d => d.id == CollegeDepartmentId)
                                .Select(d => d.departmentName)
                                .FirstOrDefault()
                            : "";
                        faculty.SamePANNumberCount =
                            jntuh_registered_faculty.Where(f => f.PANNumber == a.PANNumber).ToList().Count;
                        faculty.SameAadhaarNumberCount =
                            jntuh_registered_faculty.Where(f => f.AadhaarNumber == a.AadhaarNumber).ToList().Count;
                        faculty.SpecializationIdentfiedFor = Specializationid > 0
                            ? DegreeIdNameBasedOnSpecialization.Where(S => S.Specid == Specializationid)
                                .Select(S => S.DegreeName + "-" + S.SpcializationName)
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
                        //faculty.IdentfiedFor =
                        //    jntuh_college_faculty_registered.Where(
                        //        f => f.RegistrationNumber.Trim() == a.RegistrationNumber.Trim())
                        //        .Select(f => f.IdentifiedFor)
                        //        .FirstOrDefault();
                        //faculty.jntuh_registered_faculty_education = a.jntuh_registered_faculty_education;
                        //faculty.DegreeId = a.jntuh_registered_faculty_education.Count(e => e.facultyId == a.id && e.educationId != 8) > 0
                        //    ? a.jntuh_registered_faculty_education.Where(e => e.facultyId == a.id && e.educationId != 8)
                        //        .Select(e => e.educationId)
                        //        .Max()
                        //    : 0;
                        //faculty.PanDeactivationReasion = !string.IsNullOrEmpty(a.PanDeactivationReason)
                        //    ? a.PanDeactivationReason
                        //    : "";
                        //faculty.Absent = a.Absent != null ? (bool)a.Absent : false;
                        //faculty.BlacklistFaculty = a.Blacklistfaculy != null ? (bool)a.Blacklistfaculy : false;
                        //faculty.PHDundertakingnotsubmitted = a.PHDundertakingnotsubmitted != null
                        //    ? (bool)a.PHDundertakingnotsubmitted
                        //    : false;
                        //faculty.NOTQualifiedAsPerAICTE = a.NotQualifiedAsperAICTE != null
                        //    ? (bool)a.NotQualifiedAsperAICTE
                        //    : false;
                        //faculty.InvalidPANNo = a.InvalidPANNumber != null ? (bool)a.InvalidPANNumber : false;
                        //faculty.InCompleteCeritificates = a.IncompleteCertificates != null
                        //    ? (bool)a.IncompleteCertificates
                        //    : false;
                        //faculty.OriginalCertificatesnotshownFlag = a.OriginalCertificatesNotShown != null
                        //    ? (bool)a.OriginalCertificatesNotShown
                        //    : false;
                        //faculty.GenuinenessnotSubmitted = a.Genuinenessnotsubmitted != null ? (bool)a.Genuinenessnotsubmitted : false;
                        //faculty.NoClass = a.Noclass != null ? (bool)a.Noclass : false;
                        //faculty.InvalidDegree = a.Invaliddegree != null ? (bool)a.Invaliddegree : false;
                        //faculty.NotconsiderPhd = a.NotconsideredPHD != null ? (bool)a.NotconsideredPHD : false;
                        //faculty.NoPgSpecialization = a.NoPGspecialization != null ? (bool)a.NoPGspecialization : false;
                        //faculty.Noclassinugorpg = a.Noclass != null
                        //    ? (bool)a.Noclass
                        //    : false;
                        //faculty.NoSCM = a.NoSCM != null ? (bool)a.NoSCM : false;

                        //faculty.XeroxcopyofcertificatesFlag = a.Xeroxcopyofcertificates != null
                        //    ? (bool)a.Xeroxcopyofcertificates
                        //    : false;
                        //faculty.NotIdentityFiedForAnyProgramFlag = a.NotIdentityfiedForanyProgram != null
                        //    ? (bool)a.NotIdentityfiedForanyProgram
                        //    : false;
                        //faculty.InvalidAadhaar = a.InvalidAadhaar;
                        //faculty.VerificationStatus = a.AbsentforVerification;
                        //faculty.NOrelevantUgFlag = a.NoRelevantUG == "No" ? false : true;
                        //faculty.NOrelevantUgFlag = a.NoRelevantUG == "No" ? false : true;
                        //faculty.NOrelevantPgFlag = a.NoRelevantPG == "No" ? false : true;
                        //faculty.NOrelevantPhdFlag = a.NORelevantPHD == "No" ? false : true;
                        faculty.NotIdentityFiedForAnyProgramFlag = a.NotIdentityfiedForanyProgram != null ? (bool)a.NotIdentityfiedForanyProgram : false;
                        //faculty.PhdUndertakingDocumentstatus = a.PhdUndertakingDocumentstatus != null
                        //    ? (bool)(a.PhdUndertakingDocumentstatus)
                        //    : false;
                        //faculty.GenuinenessnotSubmitted = a.Genuinenessnotsubmitted != null ? (bool)a.Genuinenessnotsubmitted : false;
                        //faculty.OriginalsVerifiedUG = a.OriginalsVerifiedUG != null ? (bool)a.OriginalsVerifiedUG : false;
                        //faculty.OriginalsVerifiedPHD = a.OriginalsVerifiedPHD != null ? (bool)a.OriginalsVerifiedPHD : false;
                        //faculty.PHDUndertakingDocumentView = a.PHDUndertakingDocument;
                        //faculty.PhdUndertakingDocumentText = a.PhdUndertakingDocumentText;
                        //faculty.Deactivedby = a.DeactivatedBy;
                        //faculty.DeactivedOn = a.DeactivatedOn;
                        //faculty.appealsupportingdocument =
                        //    jntuh_appeal_faculty_registered.Where(r => r.RegistrationNumber == a.RegistrationNumber)
                        //        .Select(s => s.AppealReverificationSupportingDocument)
                        //        .FirstOrDefault();
                        //if (String.IsNullOrEmpty(faculty.appealsupportingdocument))
                        //{
                        //    faculty.appealsupportingdocument =
                        //   jntuh_appeal_faculty_registered.Where(r => r.RegistrationNumber == a.RegistrationNumber)
                        //       .Select(s => s.AppealReverificationScreenshot)
                        //       .FirstOrDefault();
                        //}

                        //if (faculty.Absent == true)
                        //    Reason += "Absent";
                        //if (faculty.NotIdentityFiedForAnyProgramFlag == true)
                        //{
                        //    if (Reason != null)
                        //        Reason += ",Adjunct Faculty";
                        //    else
                        //        Reason += "Adjunct Faculty";
                        //}

                        //faculty.IsStatus = a.InStatus;
                        //faculty.DeactivationReason = Reason;
                        teachingFaculty.Add(faculty);
                    }
                }
                teachingFaculty = teachingFaculty.Where(m => m.isActive == true).OrderBy(f => f.updatedOn).ThenBy(f => f.department).ToList();
                //|| item.NoForm16Verification == true ||item.NOForm16 == true ||
                ViewBag.TotalFaculty = teachingFaculty.Count();
                ViewBag.Inactivephdfaculty = inactivephdfaculty;

                string[] data = teachingFaculty.Where(item =>
                            item.NotconsiderPhd == true ||
                            item.NOTQualifiedAsPerAICTE == true ||
                            item.InCompleteCeritificates == true || item.NoPgSpecialization == true ||
                             item.NOrelevantUgFlag == true ||
                            item.NOrelevantPgFlag == true || item.NOrelevantPhdFlag == true ||
                            item.NotIdentityFiedForAnyProgramFlag == true ||
                            item.NoSCM == true || item.InvalidPANNo == true ||
                            item.DegreeId < 4 || item.PANNumber == null ||
                            item.BlacklistFaculty == true || item.Type == "Adjunct" ||
                            item.AppliedPAN == true || item.NoClass == true || item.XeroxcopyofcertificatesFlag == true
                           || (item.PhdUndertakingDocumentstatus == false) || item.InvalidAadhaar == "Yes"
                            ).Select(e => e.RegistrationNumber).ToArray();
                ViewBag.FlagTotalFaculty = teachingFaculty.Where(item =>
                            item.NotconsiderPhd == true ||
                            item.NOTQualifiedAsPerAICTE == true ||
                            item.InCompleteCeritificates == true || item.NoPgSpecialization == true ||
                             item.NOrelevantUgFlag == true ||
                            item.NOrelevantPgFlag == true || item.NOrelevantPhdFlag == true ||
                            item.NotIdentityFiedForAnyProgramFlag == true ||
                            item.NoSCM == true || item.InvalidPANNo == true ||
                            item.DegreeId < 4 || item.PANNumber == null ||
                            item.BlacklistFaculty == true || item.Type == "Adjunct" ||
                            item.AppliedPAN == true || item.NoClass == true || item.XeroxcopyofcertificatesFlag == true
                           || (item.PhdUndertakingDocumentstatus == false) || item.InvalidAadhaar == "Yes"
                            ).Select(e => e).Count();
                ViewBag.ClearFaculty = ViewBag.TotalFaculty - ViewBag.FlagTotalFaculty;
                return View("FacultyVerificationIndex", teachingFaculty.OrderBy(t => t.id));
            }

            return View("FacultyVerificationIndex", teachingFaculty.OrderBy(t => t.id));
        }
        #endregion
        #region 419 Faculty Verification for clearance added by Narayana Reddy
        //419 Faculty Verification new action Added by Narayana Reddy on 25-03-2019
        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification")]
        public ActionResult Facultyverificationfouronenine(int? collegeid)
        {
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy =
                db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            var colleges = db.jntuh_college.Join(db.jntuh_college_edit_status, co => co.id, e => e.collegeId, (co, e) => new { co = co, e = e }).Where(c => c.co.isActive == true && c.e.academicyearId == prAy && c.e.IsCollegeEditable == false).Select(c => new { collegeId = c.co.id, collegeName = c.co.collegeCode + "-" + c.co.collegeName }).OrderBy(c => c.collegeName).ToList();

            colleges.Add(new { collegeId = 375, collegeName = "TEST COLLEGE" });
            //colleges.Add(new { collegeId = 0, collegeName = "Not Clear PHD Faculty" });
            ViewBag.Colleges = colleges;
            //ViewBag.Colleges = db.jntuh_college.Join(db.jntuh_college_edit_status, co => co.id, e => e.collegeId, (co, e) => new { co = co, e = e }).Where(c => c.co.isActive == true).Select(c => new { collegeId = c.co.id, collegeName = c.co.collegeCode + "-" + c.co.collegeName }).OrderBy(c => c.collegeName).ToList();

            //int[] EditFacultycollegeIds = {4,5,7,8,9,12,20,22,23,24,26,27,29,30,32,35,38,39,40,41,42,43,44,47,48,55,56,58,68,69,70,72,74,75,77,79,80,81,84,85,86,87,88,91,97,100,103,105,106,107,108,109,110,111,116,117,118,119,121,123,127,128,129,130,132,134,135,137,138,139,141,143,144,145,147,148,150,152,153,155,156,157,158,161,162,163,164,165,166,168,171,172,173,174,175,176,177,178,179,180,181,183,184,185,187,189,192,193,195,196,198,202,203,206,210,211,213,214,215,217,218,222,223,225,227,228,234,235,236,241,242,244,245,246,249,250,254,256,260,261,262,264,266,267,271,273,276,282,287,290,291,292,295,296,298,299,300,302,304,307,309,310,313,315,316,318,319,321,322,324,327,329,334,335,336,349,350,352,353,355,360,364,365,366,367,368,369,370,373,374,376,380,382,384,385,389,391,392,393,399,400,411,414,420,421,423,424,428,435,436,439,441,455,375};
            //ViewBag.Colleges =
            //    db.jntuh_college.Where(c => EditFacultycollegeIds.Contains(c.id))
            //        .Select(a => new {collegeId = a.id, collegeName = a.collegeCode + "-" + a.collegeName})
            //        .OrderBy(r => r.collegeName)
            //        .ToList();
            ViewBag.collegeid = collegeid;
            var jntuh_department = db.jntuh_department.ToList();
            List<FacultyRegistration> teachingFaculty = new List<FacultyRegistration>();

            if (collegeid != null)
            {

                var DegreeIdNameBasedOnSpecialization = (from a in db.jntuh_department
                                                         join b in db.jntuh_specialization on a.id equals b.departmentId
                                                         join c in db.jntuh_degree on a.degreeId equals c.id
                                                         select new
                                                         {
                                                             DegreeId = c.id,
                                                             DegreeName = c.degree,
                                                             SpcializationName = b.specializationName,
                                                             Specid = b.id
                                                         }).ToList();

                //418 Faculty Verification Condition 
                //string[] Copy_RegNos = db.jntuh_college_faculty_registered_copy.Where(c => c.collegeId == 375).Select(e => e.RegistrationNumber).Distinct().ToArray();

                // List<jntuh_college_faculty_registered> jntuh_college_faculty_registered_College_Portal = db.jntuh_college_faculty_registered.Where(cf => cf.collegeId == collegeid).Select(cf => cf).ToList();
                //committed by siva due to only edit option faculty is comming case....
                // jntuh_college_faculty_registered = jntuh_college_faculty_registered.Where(c => !Copy_RegNos.Contains(c.RegistrationNumber)).ToList();


                List<jntuh_college_faculty_registered> jntuh_college_faculty_registered = db.jntuh_college_faculty_registered.Where(cf => cf.collegeId == collegeid).Select(cf => cf).ToList();

                //419 Faculty Verifi
                string[] collegeRegnos =
                    jntuh_college_faculty_registered.Select(s => s.RegistrationNumber).Distinct().ToArray();
                string[] Copy_RegNos = db.jntuh_college_faculty_registered_copy.Where(c => c.collegeId == 1419 && collegeRegnos.Contains(c.RegistrationNumber)).Select(e => e.RegistrationNumber).Distinct().ToArray();

                string[] strRegNoS = jntuh_college_faculty_registered.Where(c => !Copy_RegNos.Contains(c.RegistrationNumber)).Select(cf => cf.RegistrationNumber).ToArray();
                string[] PrincipalstrRegNoS = db.jntuh_college_principal_registered.Where(P => P.collegeId == collegeid).Select(P => P.RegistrationNumber.Trim()).ToArray();
                List<jntuh_registered_faculty> jntuh_registered_faculty = new List<jntuh_registered_faculty>();
                ViewBag.TotalClearedFaculty = collegeRegnos.Count() - strRegNoS.Count();
                ViewBag.TotalCollegeFaculty = collegeRegnos.Count();

                //committed by siva due to only edit option faculty is comming case....
                //jntuh_registered_faculty = db.jntuh_registered_faculty.Where(rf => strRegNoS.Contains(rf.RegistrationNumber) && rf.Blacklistfaculy != true)  //&& (rf.collegeId == null || rf.collegeId == collegeid)//&& rf.Notin116 != true
                //                             .ToList();

                //New Code written by siva due to only edit option faculty is comming case....
                DateTime editoptionStartdate = new DateTime(2018, 08, 30);
                if (collegeid == 375)
                {
                    string[] phdnotclearcollegeassociate = new string[]
                    {
                       "26150328-102440","74150329-123802","07150330-011802","94150330-113934","21150330-130127","45150330-135118","24150330-154256","43150330-161340","34150330-174403","85150330-182657","47150331-113435","94150331-122435","85150331-143942","37150331-201236","53150331-204441","72150401-095414","32150401-121708","22150401-125835","78150401-132922","63150401-151206","17150401-154407","72150401-171750","17150401-192409","82150401-193318","53150401-204835","66150401-210720","03150401-222957","02150402-111353","68150402-113151","90150402-113500","29150402-115939","81150402-122531","44150402-125710","48150402-130357","02150402-134808","84150402-135106","90150402-140326","66150402-143755","68150402-144056","60150402-151814","71150402-153329","14150402-155349","76150402-165017","94150402-165951","68150403-015716","89150403-074443","74150403-101252","36150403-130305","79150403-133318","77150403-144321","70150403-145538","50150403-154325","26150403-155145","12150403-161135","48150403-190656","29150403-194155","02150403-220700","85150403-230538","20150404-015659","41150404-111625","23150404-113644","64150404-113919","13150404-123131","07150404-125837","76150404-132522","14150404-132654","06150404-132713","74150404-132947","98150404-151104","72150404-151751","12150404-154539","86150404-154907","16150404-155121","83150404-160632","67150404-161438","83150404-162428","04150404-162859","61150404-163123","91150404-163458","04150404-164306","15150404-165417","32150404-172740","29150404-175402","28150404-175504","66150404-195728","50150405-080016","63150405-105142","58150405-142724","70150405-151757","01150405-154759","78150405-203601","48150405-232624","91150406-023716","58150406-100658","50150406-110631","48150406-111721","39150406-112650","58150406-112805","75150406-114801",
                       "93150406-121602","16150406-121938","16150406-123557","39150406-124038","00150406-124851","97150406-124918","46150406-125500","78150406-131451","31150406-132701","84150406-134458","73150406-141234","80150406-145457","81150406-145946","38150406-151849","71150406-154014","81150406-154016","66150406-154147","51150406-154756","53150406-161513","88150406-161753","47150406-163118","97150406-163314","35150406-172430","69150406-174533","83150406-190942","98150406-191738","52150406-194954","77150407-100906","26150407-101433","12150407-102002","00150407-105515","88150407-105745","74150407-110315","06150407-110409","08150407-111738","60150407-112825","54150407-113636","49150407-114156","97150407-114851","92150407-114946","94150407-115002","78150407-121739","73150407-124535","39150407-125323","26150407-125656","73150407-125907","85150407-125912","49150407-132629","16150407-133453","77150407-135637","72150407-140037","38150407-141045","39150407-142712","24150407-144012","93150407-151138","67150407-152411","52150407-152647","42150407-154522","13150407-160822","91150407-164824","86150407-165203","66150407-165442","04150407-173505","6979-150408-070114","9714-150408-101414","3786-150408-101716","6945-150408-105112","1666-150408-110342","9352-150408-112832","9588-150408-114822","7663-150408-120134","3046-150408-123016","5382-150408-123033","7525-150408-124513","0254-150408-125530","5043-150408-125708","3539-150408-131514","4245-150408-132734","2411-150408-134853","8792-150408-140040","5826-150408-151029","6082-150408-155301","7167-150408-170456","2724-150408-171821","6405-150408-172409","0149-150408-213453","0457-150408-215923","4802-150409-000442","7860-150409-093946","5346-150409-103615","7624-150409-103723","9811-150409-103922",
                       "9420-150409-104558","1608-150409-104757","3556-150409-105506","0055-150409-105615","3743-150409-112440","8127-150409-121259",
                       "3521-150409-121615","8748-150409-122037","3345-150409-122730","2802-150409-125545","5685-150409-131541","3624-150409-132438","0377-150409-133622","5801-150409-143524","0816-150409-144756","2890-150409-150716","5763-150409-155228","1710-150409-162323","9091-150409-165116","8690-150409-173054","8835-150409-215651","4681-150410-101558","0123-150410-101600","0209-150410-101942","9079-150410-103817","3133-150410-105936","4822-150410-114424","3509-150410-115632","7534-150410-120811","3151-150410-125404","9946-150410-132838","5294-150410-135203","2051-150410-135522","9708-150410-144632","4610-150410-155151","7490-150411-125727","1955-150411-132435","1034-150411-145802","9454-150411-155023","4370-150411-182858","7362-150412-131129","0076-150412-140821","3453-150412-185020","8441-150413-113304","8921-150413-122236","1019-150413-125218","5751-150413-125449","9288-150413-125630","8223-150413-130432","7118-150413-130756","2810-150413-132540","2748-150413-133433","0861-150413-142903","9165-150413-144341","3285-150413-145210","0622-150413-150200","5958-150413-154246","5931-150413-155104","1291-150413-162334","5286-150413-164156","1818-150413-164538","4114-150413-164740","6344-150413-170725","9692-150413-174620","2876-150413-192246","4523-150413-200655","1324-150413-200901","0321-150413-212744","2177-150414-063346","7598-150414-111151","4854-150414-125905","6441-150414-142344","1191-150414-182314","4024-150414-201804","5985-150415-112044","6619-150415-115130","2697-150415-115426","1268-150415-140422","2639-150415-144606","0459-150415-162506","8713-150415-223120","4310-150415-235250","0343-150416-102005","1898-150416-103606","8636-150416-130230","8818-150416-131333","5417-150416-173107","1354-150417-115731","2518-150417-130617","0920-150417-131040","4699-150417-132914","5498-150417-133552","1546-150417-152331","6858-150417-153743","9369-150417-214628","9695-150418-094635","0255-150418-113623","4549-150418-133640","1057-150418-141941","7730-150418-150918","0037-150418-151850","8184-150418-160647","1499-150418-162700","3992-150419-080217","8162-150419-091405","2759-150419-112924","8382-150419-134710","3918-150420-101926","9585-150420-105758","0936-150420-114612","5218-150420-155112","5614-150420-155913","6931-150421-013104","7479-150421-105533","2644-150421-125943","2274-150421-163320","0004-150422-121538","5574-150422-123317","7164-150422-204237","3961-150424-120438","1897-150425-151950","6283-150425-165254","1339-150425-223809","9839-150426-103701","3119-150426-160711","3969-150427-114410","2701-150427-155527","8285-150427-160311","2787-150427-172144","2257-150428-055331","8599-150428-123056","4897-150428-162757","9127-150429-155346","8544-150429-203133","1965-150430-110931","0330-150430-152111","9828-150501-044104","0004-150502-142242","8640-150504-132831","4571-150504-181413","1804-150504-223903","2916-150505-111623","7794-150506-111930","2705-150506-170154","2351-150507-075609","6386-150507-161158","7149-150507-192126","9233-150507-194931","1693-150508-154732","5617-150513-152145","5307-150605-143306","4896-150619-162217","8276-150620-203646","2694-150622-111010","0183-150623-110537","0224-150623-130257","6281-150626-112130","8008-150702-132923","2831-151216-143314","3428-151219-150017","2468-151222-143411","4716-151222-144026","2098-151228-160426","2488-160108-160128","9189-160112-211310","4999-160118-121910","8710-160123-150322","6879-160127-141445","9785-160127-143316",
                       "6672-160128-161546","2530-160130-131822","7516-160130-171629","3738-160130-172215","8187-160201-143225","8795-160203-175048","7783-160204-153400","3930-160204-155854","8388-160209-104546","9263-160209-165332","8313-160209-172511","1082-160210-102657","6278-160210-150833","2439-160211-123708","8343-160212-153801","6456-160213-103822","4874-160215-165007","6661-160216-162733","0143-160217-160943","1127-160218-105422","2880-160218-223616","3431-160219-135556","0653-160220-110651","3832-160221-111516","9683-160221-140840","6775-160223-100217","8787-160223-142327","7447-160223-160623","5772-160224-120936","0562-160224-172550","1840-160225-112218","2582-160225-154756","9257-160226-122157","4763-160226-172443","2772-160226-193511","7805-160227-114301","8994-160227-143825","4320-160228-133043","8506-160228-150736","1932-160229-073352","4865-160229-143431","6161-160302-130228","1959-160303-111849","4984-160303-115900","0124-160303-185802","7767-160304-114226","6364-160304-122123","9340-160304-135148","8686-160304-154330","6012-160304-160526","6631-160305-114438","0462-160305-131508","3068-160305-201328","7957-160305-202129","3695-160306-065942","2185-160306-125348","1061-160306-221302","7715-160307-094837","1881-160307-111846","2875-160307-112908","1097-160307-135134","3525-160307-141516","7008-160307-141820","9178-160307-144617","0663-160308-152723","2949-160309-110305","2397-160310-142830","3654-160310-153344","7206-160311-141220","0012-160311-144543","2232-160312-110412","4712-160313-143055","1581-160313-152759","1716-160313-155737","3592-160313-222024","9853-160314-115912","2841-160315-000843","4454-160315-024113","1303-160315-103412","8250-160315-111108","5152-160315-120421","3370-160315-134028","0398-160315-143304","0871-160315-145356","7613-160315-162203","7900-160315-170924","4841-160315-184911","7440-160315-210929","0687-160316-122027","7356-160316-133849","9805-160316-211552","4497-160318-164821","1889-160319-164942","6387-160319-165638","9268-160320-150307","2892-160320-155417","2051-160524-153249","6614-160525-111651","6266-160526-122437","0568-160529-145949","1959-160529-184524","4825-160529-193005","7691-160804-172710","1778-161022-132131","6213-161022-184405","5675-161024-151653","3962-161024-194724","4847-161024-210125","7684-161024-224045","3077-161025-092554","3448-161025-195900","2784-161027-112616","0661-161031-001234","6785-161031-114837","6391-161101-112549","3055-161125-201020","9905-161202-142028","1194-161205-171645","9665-161207-131759","7595-161207-140122","1736-161207-142623","0089-161209-145730","4357-161210-150550","1988-161213-123229","1886-161215-110840","3259-161221-153152","0368-161227-154357","6447-161229-061433","5963-161230-134120","7984-170103-155512","6532-170104-113621","4374-170104-125349","3745-170105-143145","5118-170106-120828","4779-170106-123241","8084-170106-141141","3439-170106-143940","7030-170107-122049","4843-170108-135437","2773-170109-102839","7757-170109-141614","9801-170110-065949","9614-170110-095637","0767-170111-102126","7111-170111-113028","0846-170111-120917","6217-170112-095414","9510-170112-102057","6981-170112-104726","2167-170112-111334","4973-170113-202737","4473-170115-183328","3050-170116-142118","7353-170117-121726","9202-170118-101846","9661-170119-095107","8446-170119-141054","3169-170121-103040","5203-170122-115917","2999-170122-190110","0311-170122-203935","1932-170122-210432","0987-170125-114032","4464-170126-024023","8631-170126-075712","3573-170126-082633","4801-170126-125032","3830-170127-055112","2054-170127-102825","6108-170128-041206","4364-170129-111927","8402-170130-042424","6283-170130-062336","4731-170130-143850","7953-170131-025801","7999-170131-184403","7050-170131-192946","1286-170131-201713","9213-170131-224323","2639-170201-061450","6590-170201-091037","6553-170201-092800","1332-170201-103806","8585-170201-120216","7401-170201-135706","1423-170201-231848","7735-170202-021128","2951-170202-052356","8348-170202-174618","5992-170203-094437","4054-170204-110401","0617-170204-180432","7893-170206-154526","9568-170207-150317","3573-170207-162957","8236-170207-202324","9550-170208-114920","6641-170208-130829","4300-170208-163059","1255-170208-172403","1089-170208-175418","8673-170209-001735","4717-170212-193526","0340-170213-144245","0386-170213-173208","5798-170213-181815","2413-170520-123446","2704-170520-130953","0398-170521-100904","0647-170521-101358","0697-170521-115623","3919-170521-115934","8488-170521-214919","8423-170522-001654","5730-170522-100531","1539-170522-113513","4265-170522-122220","0043-170522-131138","4232-170522-141133","1803-170522-170602","8348-170522-171005","9020-170522-171446","1799-170523-145346","2982-170523-172149","9242-170523-172200","9348-170523-173650","0228-170523-212632","1336-170911-162939","8004-170911-225045","5826-170913-103020","4439-170913-131636","9504-170913-132727","0012-170914-122955","3993-170914-140249","6336-170914-140628","3040-170915-115017","7770-170915-144641","0897-170915-161247","4373-170915-165932","3792-170915-171037","6907-170916-103001","9760-170916-124258","9857-170916-131945","0861-170916-141035","7136-170918-135738","7220-170918-153900","7454-171009-094302","5057-171021-105147","4917-171022-202121","5784-171106-142610","0191-171129-114721","2324-171208-105321","2460-171220-144420","6404-171221-160659","6053-171221-173151","0895-171222-113335","5089-171223-141800","7837-171225-073920","9324-171225-083429","0553-171226-122921","4273-171226-143405","7329-171227-111349","3286-171227-145359","1938-171227-190729","6366-150408-203914","0520-180102-220148","1416-180104-125600","7227-180106-110314","2166-180109-173401","0105-180110-112802","0479-180110-220632","8470-150427-103627","5916-180127-115342","6887-180127-160016","5252-180127-160522","3908-180129-122453","1624-180129-143004","5582-180131-143419","4460-180131-172933","1704-180201-132810","4463-180201-140846","4032-180201-151029","8166-180201-155956","3109-180201-163126","2117-180201-163235","8993-180201-172018","1878-180201-172209","4539-180201-172827","5025-180201-191319","4189-180201-202900","5921-180202-095859","5308-180202-115849","9562-180202-121510","3554-180202-123255","3367-180202-132237","6557-180202-145448","6324-180202-145631","9285-180202-153742","8741-180202-162322","8361-180202-165230","4248-180202-171805","8196-180202-172813","5990-180202-173614","7893-180202-182120","5905-180203-132313","9882-180203-163139","9122-180204-153030","9662-180204-211536","7860-180207-125814","7617-180207-150550","0704-180208-133828","7727-180208-134717","1747-180208-144342","2425-180208-152024","3887-180208-154154","0755-180208-162637","8667-180208-205639","0961-180208-210002","1834-180208-210350","6307-180208-211616","2182-180208-233008","9966-180209-140843","9447-180212-120756","3697-180216-140031","2164-180219-145739","1076-180220-234151","0275-180413-225756","9375-180414-103858","5603-180414-154024","3162-180415-193439","7990-180416-105441","2754-180416-124114","3529-180416-125522","5838-180416-133809","0545-180416-150913","5778-180416-152538","7707-180416-160218","0123-180416-162437","4607-180416-164009","8593-180416-164709","5837-180416-170400","8852-180416-175025","0704-180416-194821","7228-180416-212014","3554-180416-224025","2909-180417-105850","6846-180417-113033","3130-180417-121250","1949-180417-122625","4120-180417-123136","4053-180417-132839","1127-180417-133316","7341-180417-143139","9150-180417-144644","6243-180417-151242","4380-180417-151524","6826-180417-152947","7662-180417-154826","8584-180417-155254","1078-180417-171508","3049-180417-171953","6669-180417-180422","3173-180417-180447","2738-180417-181119","1593-180417-185823","4250-180417-185913","2149-180419-104552","3674-180419-135113","6072-180419-143930","7026-180419-150643","4502-180419-155035","6759-180420-105440","6503-180420-154506","2609-180420-181501","5098-180421-091752","2754-180421-105428","2182-180421-114834","2277-180421-130154","4427-180421-131451","4938-180421-141422","7867-180421-143302","4540-180421-150439","7594-180421-150604","7372-180421-150919","5980-180421-152003","9481-180421-153750","9537-180421-154149","5885-180421-154309","3135-180421-155425","5776-180421-155710","1535-180421-160729","5702-180421-160738","1154-180421-161510","7089-180421-161804","1468-180421-165652","9807-180421-171303","5420-180421-181108","1924-180421-195320","8173-180421-200729","6432-180502-122013","0485-180702-171940","9715-180703-001326","6481-180703-075013","3511-180703-123706","2605-180703-133913","6954-180703-140314","8772-180703-154939","9392-180704-001253","7095-180705-111442","7814-180707-121720","4802-180709-150750","2231-180710-102723","7948-180710-121316","9418-180711-145210","7203-180711-153007","0294-180712-110352","9959-180713-002900","1744-180713-010015","5651-180713-124217","0602-180713-161550","0467-180716-115522","2227-180716-160856","5694-180717-164555","0003-180718-111543","8650-180719-124256","5267-180719-151738","3929-180721-133143","6877-180723-180939","4480-180725-152021","1689-180726-094624","9302-180727-141317","1147-180727-144227","4812-180730-105523","4472-180730-152624","5116-180802-153604","0081-180803-123759","5431-180805-232946","7184-180807-114846","5422-180808-112307","3861-180808-115346","1451-180808-161540","4474-180809-203945","2695-180810-140035","9669-180810-143812","5281-180810-144406","8167-180810-151024","4969-180811-131709","2122-180811-132049","7456-180811-155448","0621-180811-193222","0743-180811-212548","3747-180811-214805","5995-180811-222347","5981-180812-093328","4834-180813-111346","3067-180813-115313","0532-180813-124715","7790-180813-132221","8596-180813-153655","6247-180813-234819","6147-180814-133656","1470-180816-135632","9367-180816-145749","1818-180816-165806","5104-180817-121912","5972-180818-104334","5653-180818-111615","7572-180818-112007","9018-180818-121645","8124-180818-122041","6166-180818-122732","4636-180818-123716","5106-180818-124347","5296-180818-132231","6560-180818-133117","3010-180818-140606","9286-180818-151917","0213-180818-173746","1232-180819-222940","1294-180820-114846","4935-180820-115112","6570-180820-121747","2319-180820-142857","2417-180820-144805","2171-180820-144953","8275-180820-152356","0077-180820-161226","8639-180820-161745","1215-180820-170112","9739-180820-182445","1004-180820-191254","2373-180820-194644","8347-180824-213250","4323-180827-115910","1684-180827-134553","6147-180914-121328","6446-180914-122927","5806-180914-144231","3010-180918-101949","5612-181013-105443","6680-181015-151313","8783-181016-162700","8556-181024-121524","9531-181025-122843","9887-181105-140926","4830-181106-124446","2591-181112-174044","6187-181114-121259","1312-181115-120913","5851-190118-142407","7694-190118-144316","5629-190118-144514","7733-190118-145753","5720-190118-150450","8727-190118-201047","7610-190119-095241","5633-190119-142303","8766-190119-161519","1172-190119-225243","2803-190120-143634","9878-190121-202106","6980-190124-165329","2837-190128-143758","6370-190131-113843","9860-190202-110439","6894-190203-111227","5186-190204-133550","1381-190206-115752","0068-190207-153803","3716-190209-131236","2688-190211-154811","5221-190212-125443","5163-190212-160516","1436-190213-130310","2442-190214-133013","4796-190215-095908","6452-190215-103755","4350-190215-111559","3663-190215-141957","6397-190216-121426","9192-190218-011007","3607-190218-014535","2381-190218-111934","9525-190218-132152","1883-190218-155920","5076-190218-161729","5661-190218-175703","2399-190219-101751","2493-190219-123238","6433-190219-145341","2225-190219-152148","0739-190219-155159","2784-190219-171631","9366-190219-175153","0698-190219-224953","7389-190220-114326","0824-190220-115619","7679-190220-121213","3784-190220-121328","8670-190220-124032","3537-190220-144812","7983-190220-152408","5398-190220-152508","8379-190220-161816","6170-190220-163500","9560-190220-170729","4695-190220-171822","0086-190220-180221","4852-190220-185751","4313-190221-105212","5447-190221-142836","6937-190221-150359","3078-190221-152149","6815-190221-152826","8563-190221-154413","1040-190221-160040","5016-190221-160822","9744-190221-161632","5254-190221-163148"
                    };

                    jntuh_registered_faculty = db.jntuh_registered_faculty.Where(rf => phdnotclearcollegeassociate.Contains(rf.RegistrationNumber) && rf.Blacklistfaculy != true)
                                           .ToList();
                    //jntuh_registered_faculty = db.jntuh_registered_faculty.Where(rf => phdnotcollegeassociate.Contains(rf.RegistrationNumber) && rf.Blacklistfaculy != true && rf.updatedOn >= editoptionStartdate)
                    //                       .ToList();
                }
                else
                {
                    jntuh_registered_faculty = db.jntuh_registered_faculty.Where(rf => strRegNoS.Contains(rf.RegistrationNumber) && rf.Blacklistfaculy != true)  //&& (rf.collegeId == null || rf.collegeId == collegeid)//&& rf.Notin116 != true
                                            .ToList();
                }



                //var jntuh_faculty = db.jntuh_registered_faculty.Where(rf => rf.Blacklistfaculy != true && rf.updatedOn >= editoptionStartdate)  //&& (rf.collegeId == null || rf.collegeId == collegeid)//&& rf.Notin116 != true
                //                            .ToList();

                var Specializations = db.jntuh_specialization.ToList();

                string RegNumber = "";
                int? Specializationid = 0;
                int? CollegeDepartmentId = 0;
                int inactivephdfaculty = 0;
                foreach (var a in jntuh_registered_faculty)
                {
                    int educationid =
                        db.jntuh_registered_faculty_education.Where(e => e.facultyId == a.id && e.educationId == 6)
                            .Select(s => s.educationId)
                            .FirstOrDefault();
                    string Reason = String.Empty;
                    if (collegeid == 375)
                    {
                        Specializationid =
                          jntuh_college_faculty_registered.Where(
                              C => C.RegistrationNumber.Trim() == a.RegistrationNumber.Trim())
                              .Select(C => C.SpecializationId)
                              .FirstOrDefault();
                        CollegeDepartmentId =
                            jntuh_college_faculty_registered.Where(
                                C => C.RegistrationNumber.Trim() == a.RegistrationNumber.Trim())
                                .Select(C => C.DepartmentId)
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
                        //Hear showing JNTU Specilazation only Pharmacy Faculty
                        faculty.PGSpecializationName = a.Jntu_PGSpecializationId != null
                            ? DegreeIdNameBasedOnSpecialization.Where(e => e.Specid == a.Jntu_PGSpecializationId)
                                .Select(e => e.DegreeName + "-" + e.SpcializationName)
                                .FirstOrDefault()
                            : "";
                        faculty.GenderId = a.GenderId;
                        faculty.Email = a.Email;
                        faculty.facultyPhoto = a.Photo;
                        faculty.Mobile = a.Mobile;
                        faculty.PANNumber = a.PANNumber;
                        faculty.AadhaarNumber = a.AadhaarNumber;
                        faculty.isActive = a.isActive;
                        faculty.isApproved = a.isApproved;
                        faculty.department = CollegeDepartmentId > 0
                            ? jntuh_department.Where(d => d.id == CollegeDepartmentId)
                                .Select(d => d.departmentName)
                                .FirstOrDefault()
                            : "";
                        faculty.SamePANNumberCount =
                            jntuh_registered_faculty.Where(f => f.PANNumber == a.PANNumber).ToList().Count;
                        faculty.SameAadhaarNumberCount =
                            jntuh_registered_faculty.Where(f => f.AadhaarNumber == a.AadhaarNumber).ToList().Count;
                        faculty.SpecializationIdentfiedFor = Specializationid > 0
                            ? DegreeIdNameBasedOnSpecialization.Where(S => S.Specid == Specializationid)
                                .Select(S => S.DegreeName + "-" + S.SpcializationName)
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
                        //faculty.Noclassinugorpg = a.MultipleRegInSameCollege != null
                        //    ? (bool)a.MultipleRegInSameCollege
                        //    : false;
                        faculty.XeroxcopyofcertificatesFlag = a.Xeroxcopyofcertificates != null
                            ? (bool)a.Xeroxcopyofcertificates
                            : false;
                        faculty.NotIdentityFiedForAnyProgramFlag = a.NotIdentityfiedForanyProgram != null
                            ? (bool)a.NotIdentityfiedForanyProgram
                            : false;
                        faculty.NOrelevantUgFlag = a.NoRelevantUG == "No" ? false : true;
                        faculty.NOrelevantPgFlag = a.NoRelevantPG == "No" ? false : true;
                        faculty.NOrelevantPhdFlag = a.NORelevantPHD == "No" ? false : true;
                        //faculty.NoForm16Verification = a.Noform16Verification !=null ? (bool)a.Noform16Verification : false;
                        faculty.NoSCM17Flag = a.NoSCM17 != null ? (bool)a.NoSCM17 : false;
                        //faculty.PhotocopyofPAN = a.PhotoCopyofPAN != null ? (bool)a.PhotoCopyofPAN : false;
                        faculty.PhdUndertakingDocumentstatus = a.PhdUndertakingDocumentstatus != null
                            ? (bool)(a.PhdUndertakingDocumentstatus)
                            : false;
                        faculty.PHDUndertakingDocumentView = a.PHDUndertakingDocument;
                        faculty.PhdUndertakingDocumentText = a.PhdUndertakingDocumentText;
                        //faculty.AppliedPAN = a.AppliedPAN != null ? (bool)(a.AppliedPAN) : false;
                        //faculty.SamePANUsedByMultipleFaculty = a.SamePANUsedByMultipleFaculty != null
                        //    ? (bool)(a.SamePANUsedByMultipleFaculty)
                        //    : false;
                        //faculty.Basstatus = a.BASStatusOld;
                        faculty.Deactivedby = a.DeactivatedBy;
                        faculty.DeactivedOn = a.DeactivatedOn;
                        teachingFaculty.Add(faculty);
                    }
                    else if (educationid != 6)
                    {
                        Specializationid =
                            jntuh_college_faculty_registered.Where(
                                C => C.RegistrationNumber.Trim() == a.RegistrationNumber.Trim())
                                .Select(C => C.SpecializationId)
                                .FirstOrDefault();
                        CollegeDepartmentId =
                            jntuh_college_faculty_registered.Where(
                                C => C.RegistrationNumber.Trim() == a.RegistrationNumber.Trim())
                                .Select(C => C.DepartmentId)
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
                        //faculty.Basstatus = a.BASStatus;
                        if (PrincipalstrRegNoS.Contains(a.RegistrationNumber.Trim()))
                            faculty.Principal = "Principal";
                        else
                            faculty.Principal = "";

                        //Hear showing JNTU Specilazation only Pharmacy Faculty
                        faculty.PGSpecializationName = a.Jntu_PGSpecializationId != null
                            ? DegreeIdNameBasedOnSpecialization.Where(e => e.Specid == a.Jntu_PGSpecializationId)
                                .Select(e => e.DegreeName + "-" + e.SpcializationName)
                                .FirstOrDefault()
                            : "";
                        faculty.GenderId = a.GenderId;
                        faculty.Email = a.Email;
                        faculty.facultyPhoto = a.Photo;
                        faculty.Mobile = a.Mobile;
                        faculty.PANNumber = a.PANNumber;
                        faculty.AadhaarNumber = a.AadhaarNumber;
                        faculty.isActive = a.isActive;
                        faculty.isApproved = a.isApproved;
                        faculty.department = CollegeDepartmentId > 0
                            ? jntuh_department.Where(d => d.id == CollegeDepartmentId)
                                .Select(d => d.departmentName)
                                .FirstOrDefault()
                            : "";
                        faculty.SamePANNumberCount =
                            jntuh_registered_faculty.Where(f => f.PANNumber == a.PANNumber).ToList().Count;
                        faculty.SameAadhaarNumberCount =
                            jntuh_registered_faculty.Where(f => f.AadhaarNumber == a.AadhaarNumber).ToList().Count;
                        faculty.SpecializationIdentfiedFor = Specializationid > 0
                            ? DegreeIdNameBasedOnSpecialization.Where(S => S.Specid == Specializationid)
                                .Select(S => S.DegreeName + "-" + S.SpcializationName)
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
                        faculty.Noclassinugorpg = a.Noclass != null
                            ? (bool)a.Noclass
                            : false;
                        faculty.XeroxcopyofcertificatesFlag = a.Xeroxcopyofcertificates != null
                            ? (bool)a.Xeroxcopyofcertificates
                            : false;
                        faculty.NotIdentityFiedForAnyProgramFlag = a.NotIdentityfiedForanyProgram != null
                            ? (bool)a.NotIdentityfiedForanyProgram
                            : false;
                        faculty.NOrelevantUgFlag = a.NoRelevantUG == "No" ? false : true;
                        faculty.NOrelevantPgFlag = a.NoRelevantPG == "No" ? false : true;
                        faculty.NOrelevantPhdFlag = a.NORelevantPHD == "No" ? false : true;
                        //faculty.NoForm16Verification = a.Noform16Verification !=null ? (bool)a.Noform16Verification : false;
                        faculty.NoSCM17Flag = a.NoSCM17 != null ? (bool)a.NoSCM17 : false;
                        //faculty.PhotocopyofPAN = a.PhotoCopyofPAN != null ? (bool)a.PhotoCopyofPAN : false;
                        faculty.PhdUndertakingDocumentstatus = a.PhdUndertakingDocumentstatus != null
                            ? (bool)(a.PhdUndertakingDocumentstatus)
                            : false;
                        faculty.PHDUndertakingDocumentView = a.PHDUndertakingDocument;
                        faculty.PhdUndertakingDocumentText = a.PhdUndertakingDocumentText;
                        //faculty.AppliedPAN = a.AppliedPAN != null ? (bool)(a.AppliedPAN) : false;
                        //faculty.SamePANUsedByMultipleFaculty = a.SamePANUsedByMultipleFaculty != null
                        //    ? (bool)(a.SamePANUsedByMultipleFaculty)
                        //    : false;
                        //faculty.Basstatus = a.BASStatusOld;
                        faculty.Deactivedby = a.DeactivatedBy;
                        faculty.DeactivedOn = a.DeactivatedOn;
                        teachingFaculty.Add(faculty);
                    }
                    if (collegeid != 375 && educationid == 6)
                    {
                        inactivephdfaculty++;
                    }
                }
                teachingFaculty = teachingFaculty.Where(m => m.isActive == true).OrderBy(f => f.updatedOn).ThenBy(f => f.department).ToList();
                //|| item.NoForm16Verification == true ||item.NOForm16 == true ||
                ViewBag.TotalFaculty = teachingFaculty.Count();
                ViewBag.Inactivephdfaculty = inactivephdfaculty;
                ViewBag.FlagTotalFaculty = teachingFaculty.Where(item =>
                            item.Absent == true ||
                            item.NOTQualifiedAsPerAICTE == true ||
                            item.InCompleteCeritificates == true || item.MultipleReginSamecoll == true ||
                             item.NOrelevantUgFlag == true ||
                            item.NOrelevantPgFlag == true || item.NOrelevantPhdFlag == true ||
                            item.NotIdentityFiedForAnyProgramFlag == true ||
                            item.NoSCM17Flag == true || item.InvalidPANNo == true ||
                            item.DegreeId < 4 || item.PANNumber == null ||
                            item.BlacklistFaculty == true || item.Type == "Adjunct" ||
                            item.AppliedPAN == true || item.SamePANUsedByMultipleFaculty == true || item.XeroxcopyofcertificatesFlag == true
                           || (item.PhdUndertakingDocumentstatus == false)
                            || item.Basstatus == "N").Select(e => e).Count();
                ViewBag.ClearFaculty = ViewBag.TotalFaculty - ViewBag.FlagTotalFaculty;
                return View("FacultyVerificationIndex", teachingFaculty.OrderBy(t => t.id));
            }

            return View("FacultyVerificationIndex", teachingFaculty.OrderBy(t => t.id));
        }

        //419 Faculty Verification Faculty View
        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification,DataEntry")]
        public ActionResult ViewFacultyDetails(string fid)
        {
            FacultyRegistration regFaculty = new FacultyRegistration();
            int fID = 0;

            if (!string.IsNullOrEmpty(fid))
            {
                regFaculty.GenderId = null;
                regFaculty.isFacultyRatifiedByJNTU = null;
                fID = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(fid, WebConfigurationManager.AppSettings["CryptoKey"]));
                // Above code commented by Naushad Khan Anbd added the below line.
                // fID = Convert.ToInt32(fid);
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
                    regFaculty.WorkingStatus = faculty.WorkingStatus;
                    regFaculty.TotalExperience = faculty.TotalExperience;
                    regFaculty.OrganizationName = faculty.OrganizationName;
                    if (regFaculty.CollegeId != 0)
                    {
                        regFaculty.CollegeName = db.jntuh_college.Find(regFaculty.CollegeId).collegeCode + " - " + db.jntuh_college.Find(regFaculty.CollegeId).collegeName;
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
                    regFaculty.DeactivedOn = faculty.DeactivatedOn;
                    regFaculty.Deactivedby = faculty.DeactivatedBy;
                    DateTime verificationstartdate = new DateTime(2019, 12, 31);
                    ViewBag.Isdone = true;
                    if (verificationstartdate > regFaculty.DeactivedOn || regFaculty.DeactivedOn == null)
                    {
                        ViewBag.Isdone = false;
                    }
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

                    string registrationNumber =
                        db.jntuh_registered_faculty.Where(of => of.id == fID)
                            .Select(of => of.RegistrationNumber)
                            .FirstOrDefault();
                    int[] pharmacydepartmetids = { 26, 36, 27, 36, 39, 61 };

                    int specializationid =
                        db.jntuh_college_faculty_registered.Where(of => of.RegistrationNumber == registrationNumber && of.FacultySpecializationId == null)
                            .Select(of => of.DepartmentId)
                            .FirstOrDefault() == null ? 0 : Convert.ToInt32(db.jntuh_college_faculty_registered.Where(of => of.RegistrationNumber == registrationNumber && of.FacultySpecializationId == null)
                            .Select(of => of.DepartmentId)
                            .FirstOrDefault());
                    ViewBag.shospecilizationlink = false;
                    regFaculty.PHDView = db.jntuh_faculty_phddetails.Where(i => i.Facultyid == faculty.id).Count();
                    //Commented on 18-06-2018 by Narayana Reddy
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
                    return RedirectToAction("FacultyVerificationIndex", "FacultyVerificationDENew");
                }
            }
            else
            {
                return RedirectToAction("FacultyVerificationIndex", "FacultyVerificationDENew");
            }
        }

        //419 faculty Verification Flags Update means clear flags
        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification")]
        public ActionResult UpdateFacultyFlags(string fid, string command)
        {
            int userid = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int fID = 0;
            TempData["Success"] = null;
            TempData["Error"] = null;
            if (fid != null)
            {
                fID = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(fid, System.Web.Configuration.WebConfigurationManager.AppSettings["CryptoKey"]));
            }

            if (fID != 0)
            {
                if (command == "Approved")
                {
                    var jntuh_registered_faculty = db.jntuh_registered_faculty.Where(s => s.id == fID).Select(e => e).FirstOrDefault();
                    jntuh_registered_faculty facultyData = new jntuh_registered_faculty();
                    if (jntuh_registered_faculty != null)
                    {
                        int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
                        int ay0 = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
                        //Faculty Flages are added for Tracking perpose by Narayana Reddy in 419 only Faculty Verification flags
                        jntuh_college_facultyverification_tracking updatefacultytracking =
                            db.jntuh_college_facultyverification_tracking.Where(
                                r => r.registrationNumber == jntuh_registered_faculty.RegistrationNumber.Trim())
                                .Select(s => s)
                                .FirstOrDefault();
                        if (updatefacultytracking == null)
                        {
                            jntuh_college_facultyverification_tracking facultyTracking = new jntuh_college_facultyverification_tracking();
                            facultyTracking.academicyearId = ay0;
                            facultyTracking.registrationNumber = jntuh_registered_faculty.RegistrationNumber.Trim();
                            facultyTracking.collegeId = db.jntuh_college_faculty_registered.Where(c => c.RegistrationNumber == jntuh_registered_faculty.RegistrationNumber.Trim()).Select(s => s.collegeId).FirstOrDefault();
                            facultyTracking.NotQualifiedAsperAICTE = jntuh_registered_faculty.NotQualifiedAsperAICTE;
                            facultyTracking.IncompleteCertificates = jntuh_registered_faculty.IncompleteCertificates;
                            facultyTracking.InvalidPANNumber = jntuh_registered_faculty.InvalidPANNumber;
                            facultyTracking.Xeroxcopyofcertificates = jntuh_registered_faculty.Xeroxcopyofcertificates;
                            if (!String.IsNullOrEmpty(jntuh_registered_faculty.NoRelevantUG))
                                facultyTracking.NoRelevantUG = jntuh_registered_faculty.NoRelevantUG == "Yes"
                                    ? true
                                    : false;
                            if (!String.IsNullOrEmpty(jntuh_registered_faculty.NoRelevantPG))
                                facultyTracking.NoRelevantPG = jntuh_registered_faculty.NoRelevantPG == "Yes"
                                    ? true
                                    : false;
                            if (!String.IsNullOrEmpty(jntuh_registered_faculty.NORelevantPHD))
                                facultyTracking.NoRelevantPHD = jntuh_registered_faculty.NORelevantPHD == "Yes"
                                    ? true
                                    : false;
                            facultyTracking.createdBy = userid;
                            facultyTracking.createdOn = DateTime.Now;
                            facultyTracking.updatedOn = jntuh_registered_faculty.updatedOn;
                            facultyTracking.updatedBy = jntuh_registered_faculty.updatedBy;
                            db.jntuh_college_facultyverification_tracking.Add(facultyTracking);
                            db.SaveChanges();
                        }
                        else
                        {
                            updatefacultytracking.collegeId = db.jntuh_college_faculty_registered.Where(c => c.RegistrationNumber == jntuh_registered_faculty.RegistrationNumber.Trim()).Select(s => s.collegeId).FirstOrDefault();
                            updatefacultytracking.NotQualifiedAsperAICTE = jntuh_registered_faculty.NotQualifiedAsperAICTE;
                            updatefacultytracking.IncompleteCertificates = jntuh_registered_faculty.IncompleteCertificates;
                            updatefacultytracking.InvalidPANNumber = jntuh_registered_faculty.InvalidPANNumber;
                            updatefacultytracking.Xeroxcopyofcertificates = jntuh_registered_faculty.Xeroxcopyofcertificates;
                            if (!String.IsNullOrEmpty(jntuh_registered_faculty.NoRelevantUG))
                                updatefacultytracking.NoRelevantUG = jntuh_registered_faculty.NoRelevantUG == "Yes"
                                    ? true
                                    : false;
                            if (!String.IsNullOrEmpty(jntuh_registered_faculty.NoRelevantPG))
                                updatefacultytracking.NoRelevantPG = jntuh_registered_faculty.NoRelevantPG == "Yes"
                                    ? true
                                    : false;
                            if (!String.IsNullOrEmpty(jntuh_registered_faculty.NORelevantPHD))
                                updatefacultytracking.NoRelevantPHD = jntuh_registered_faculty.NORelevantPHD == "Yes"
                                    ? true
                                    : false;
                            updatefacultytracking.updatedBy = userid;
                            updatefacultytracking.updatedOn = DateTime.Now;
                            db.Entry(updatefacultytracking).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                        jntuh_registered_faculty.NotQualifiedAsperAICTE = false;
                        jntuh_registered_faculty.IncompleteCertificates = false;
                        jntuh_registered_faculty.NoRelevantUG = "No";
                        jntuh_registered_faculty.NoRelevantPG = "No";
                        jntuh_registered_faculty.NORelevantPHD = "No";
                        //jntuh_registered_faculty.NoSCM17 = false;
                        jntuh_registered_faculty.InvalidPANNumber = false;
                        //jntuh_registered_faculty.MultipleRegInSameCollege = false;
                        // jntuh_registered_faculty.NotIdentityfiedForanyProgram = false;
                        jntuh_registered_faculty.Xeroxcopyofcertificates = false;
                        // jntuh_registered_faculty.PhdUndertakingDocumentstatus = true;

                        jntuh_registered_faculty.DeactivatedBy = userid;
                        jntuh_registered_faculty.DeactivatedOn = DateTime.Now;
                        db.Entry(jntuh_registered_faculty).State = EntityState.Modified;
                        db.SaveChanges();
                        TempData["Success"] = jntuh_registered_faculty.RegistrationNumber + " Faculty Flags are Cleared";
                        return RedirectToAction("ViewFacultyDetails", "FacultyVerificationDENew", new { fid = UAAAS.Models.Utilities.EncryptString(fID.ToString(), System.Web.Configuration.WebConfigurationManager.AppSettings["CryptoKey"]) });
                    }
                    else
                    {
                        TempData["Error"] = "No Data Found";
                        return RedirectToAction("ViewFacultyDetails", "FacultyVerificationDENew", new { fid = UAAAS.Models.Utilities.EncryptString(fID.ToString(), System.Web.Configuration.WebConfigurationManager.AppSettings["CryptoKey"]) });
                    }
                }
                //else if(command=="NotApproved")
                //{
                //    var jntuh_registered_faculty = db.jntuh_registered_faculty.Where(s => s.id == fID).Select(e => e).FirstOrDefault();
                //    jntuh_registered_faculty facultyData = new jntuh_registered_faculty();
                //    if (jntuh_registered_faculty != null)
                //    {
                //        jntuh_registered_faculty.NotQualifiedAsperAICTE = true;
                //        jntuh_registered_faculty.IncompleteCertificates = true;
                //        jntuh_registered_faculty.NoRelevantUG = "Yes";
                //        jntuh_registered_faculty.NoRelevantPG = "Yes";
                //        jntuh_registered_faculty.NORelevantPHD = "Yes";
                //        jntuh_registered_faculty.NoSCM17 = true;
                //        jntuh_registered_faculty.InvalidPANNumber = true;
                //        jntuh_registered_faculty.NotIdentityfiedForanyProgram = true;
                //        jntuh_registered_faculty.PhdUndertakingDocumentstatus = false;
                //        db.Entry(jntuh_registered_faculty).State = EntityState.Modified;
                //        db.SaveChanges();
                //        TempData["Success"] = "Flags are Cleared";
                //        return RedirectToAction("ViewFacultyDetails", "FacultyVerificationDENew", new { fid = UAAAS.Models.Utilities.EncryptString(fID.ToString(), System.Web.Configuration.WebConfigurationManager.AppSettings["CryptoKey"]) });
                //    }
                //    else
                //    {
                //        TempData["Error"] = "No Data Found";
                //        return RedirectToAction("ViewFacultyDetails", "FacultyVerificationDENew", new { fid = UAAAS.Models.Utilities.EncryptString(fID.ToString(), System.Web.Configuration.WebConfigurationManager.AppSettings["CryptoKey"]) });
                //    }
                //}
                else
                {
                    return RedirectToAction("ViewFacultyDetails", "FacultyVerificationDENew", new { fid = UAAAS.Models.Utilities.EncryptString(fID.ToString(), System.Web.Configuration.WebConfigurationManager.AppSettings["CryptoKey"]) });
                }
            }
            else
            {
                TempData["Error"] = "No Data Found";
                return RedirectToAction("ViewFacultyDetails", "FacultyVerificationDENew", new { fid = UAAAS.Models.Utilities.EncryptString(fID.ToString(), System.Web.Configuration.WebConfigurationManager.AppSettings["CryptoKey"]) });
            }
        }

        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification")]
        public ActionResult FacultyVerificationFlagsEdit(string fid, string collegeid)
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



                regFaculty.PanDeactivationReasion = !string.IsNullOrEmpty(faculty.PanDeactivationReason) ? faculty.PanDeactivationReason : "";
                regFaculty.Absent = faculty.Absent != null ? (bool)faculty.Absent : false;
                regFaculty.BlacklistFaculty = faculty.Blacklistfaculy != null ? (bool)faculty.Blacklistfaculy : false;
                regFaculty.PHDundertakingnotsubmitted = faculty.PHDundertakingnotsubmitted != null ? (bool)faculty.PHDundertakingnotsubmitted : false;
                regFaculty.NOTQualifiedAsPerAICTE = faculty.NotQualifiedAsperAICTE != null ? (bool)faculty.NotQualifiedAsperAICTE : false;
                if (regFaculty.NOTQualifiedAsPerAICTE == false)
                {

                }
                regFaculty.InvalidPANNo = faculty.InvalidPANNumber != null ? (bool)faculty.InvalidPANNumber : false;
                regFaculty.InCompleteCeritificates = faculty.IncompleteCertificates != null ? (bool)faculty.IncompleteCertificates : false;
                regFaculty.OriginalCertificatesnotshownFlag = faculty.OriginalCertificatesNotShown != null ? (bool)faculty.OriginalCertificatesNotShown : false;
                regFaculty.FalsePAN = faculty.FalsePAN != null ? (bool)faculty.FalsePAN : false;
                regFaculty.NOForm16 = faculty.NoForm16 != null ? (bool)faculty.NoForm16 : false;
                //regFaculty.MultipleReginSamecoll = faculty.MultipleRegInSameCollege != null ? (bool)faculty.MultipleRegInSameCollege : false;
                regFaculty.XeroxcopyofcertificatesFlag = faculty.Xeroxcopyofcertificates != null ? (bool)faculty.Xeroxcopyofcertificates : false;
                regFaculty.NotIdentityFiedForAnyProgramFlag = faculty.NotIdentityfiedForanyProgram != null ? (bool)faculty.NotIdentityfiedForanyProgram : false;
                regFaculty.NOrelevantUgFlag = faculty.NoRelevantUG == "No" ? false : true;
                regFaculty.NOrelevantPgFlag = faculty.NoRelevantPG == "No" ? false : true;
                regFaculty.NOrelevantPhdFlag = faculty.NORelevantPHD == "No" ? false : true;
                //regFaculty.NoForm16Verification = faculty.Noform16Verification != null ? (bool)faculty.Noform16Verification : false;
                regFaculty.NoSCM17Flag = faculty.NoSCM17 != null ? (bool)faculty.NoSCM17 : false;
                //regFaculty.PhotocopyofPAN = faculty.PhotoCopyofPAN != null ? (bool)faculty.PhotoCopyofPAN : false;
                regFaculty.PhdUndertakingDocumentstatus = faculty.PhdUndertakingDocumentstatus != null ? (bool)(faculty.PhdUndertakingDocumentstatus) : false;
                regFaculty.PHDUndertakingDocumentView = faculty.PHDUndertakingDocument;
                regFaculty.PhdUndertakingDocumentText = faculty.PhdUndertakingDocumentText;
                //regFaculty.AppliedPAN = faculty.AppliedPAN != null ? (bool)(faculty.AppliedPAN) : false;
                //regFaculty.SamePANUsedByMultipleFaculty = faculty.SamePANUsedByMultipleFaculty != null ? (bool)(faculty.SamePANUsedByMultipleFaculty) : false;
                //This Flag for No Class in Profile faculty flag Name No Class in Ug/PG
                //regFaculty.Noclassinugorpg = faculty.MultipleRegInSameCollege != null ? (bool)faculty.MultipleRegInSameCollege : false;

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
            return PartialView("_FacultyVerificationFlagsEdit", regFaculty);
        }
        //419 Faculty Verifiaction adding flages
        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification")]
        public ActionResult FacultyVerificationFlagsPostDENew(FacultyRegistration faculty)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            var facultydetails = db.jntuh_registered_faculty.Where(i => i.RegistrationNumber == faculty.RegistrationNumber).FirstOrDefault();
            if (facultydetails != null)
            {
                //Faculty Flages are added for Tracking perpose by Narayana Reddy in 419 only Faculty Verification flags
                jntuh_college_facultyverification_tracking updatefacultytracking =
                    db.jntuh_college_facultyverification_tracking.Where(
                        r => r.registrationNumber == facultydetails.RegistrationNumber.Trim())
                        .Select(s => s)
                        .FirstOrDefault();
                if (updatefacultytracking == null)
                {
                    jntuh_college_facultyverification_tracking facultyTracking = new jntuh_college_facultyverification_tracking();
                    facultyTracking.academicyearId = 11;
                    facultyTracking.registrationNumber = facultydetails.RegistrationNumber.Trim();
                    facultyTracking.collegeId = db.jntuh_college_faculty_registered.Where(c => c.RegistrationNumber == facultydetails.RegistrationNumber.Trim()).Select(s => s.collegeId).FirstOrDefault();
                    facultyTracking.NotQualifiedAsperAICTE = facultydetails.NotQualifiedAsperAICTE;
                    facultyTracking.IncompleteCertificates = facultydetails.IncompleteCertificates;
                    facultyTracking.InvalidPANNumber = facultydetails.InvalidPANNumber;
                    facultyTracking.Xeroxcopyofcertificates = facultydetails.Xeroxcopyofcertificates;


                    if (!String.IsNullOrEmpty(facultydetails.NoRelevantUG))
                        facultyTracking.NoRelevantUG = facultydetails.NoRelevantUG == "Yes"
                            ? true
                            : false;
                    if (!String.IsNullOrEmpty(facultydetails.NoRelevantPG))
                        facultyTracking.NoRelevantPG = facultydetails.NoRelevantPG == "Yes"
                            ? true
                            : false;
                    if (!String.IsNullOrEmpty(facultydetails.NORelevantPHD))
                        facultyTracking.NoRelevantPHD = facultydetails.NORelevantPHD == "Yes"
                            ? true
                            : false;
                    facultyTracking.createdBy = userID;
                    facultyTracking.createdOn = DateTime.Now;
                    facultyTracking.deactivatedBy = facultydetails.DeactivatedBy;
                    facultyTracking.deactivatedOn = facultydetails.DeactivatedOn;
                    db.jntuh_college_facultyverification_tracking.Add(facultyTracking);
                    db.SaveChanges();
                }
                else
                {
                    updatefacultytracking.collegeId = db.jntuh_college_faculty_registered.Where(c => c.RegistrationNumber == facultydetails.RegistrationNumber.Trim()).Select(s => s.collegeId).FirstOrDefault();
                    updatefacultytracking.NotQualifiedAsperAICTE = facultydetails.NotQualifiedAsperAICTE;
                    updatefacultytracking.IncompleteCertificates = facultydetails.IncompleteCertificates;
                    updatefacultytracking.InvalidPANNumber = facultydetails.InvalidPANNumber;
                    updatefacultytracking.Xeroxcopyofcertificates = facultydetails.Xeroxcopyofcertificates;
                    if (!String.IsNullOrEmpty(facultydetails.NoRelevantUG))
                        updatefacultytracking.NoRelevantUG = facultydetails.NoRelevantUG == "Yes"
                            ? true
                            : false;
                    if (!String.IsNullOrEmpty(facultydetails.NoRelevantPG))
                        updatefacultytracking.NoRelevantPG = facultydetails.NoRelevantPG == "Yes"
                            ? true
                            : false;
                    if (!String.IsNullOrEmpty(facultydetails.NORelevantPHD))
                        updatefacultytracking.NoRelevantPHD = facultydetails.NORelevantPHD == "Yes"
                            ? true
                            : false;
                    updatefacultytracking.updatedBy = userID;
                    updatefacultytracking.updatedOn = DateTime.Now;
                    updatefacultytracking.deactivatedBy = facultydetails.DeactivatedBy;
                    updatefacultytracking.deactivatedOn = facultydetails.DeactivatedOn;
                    db.Entry(updatefacultytracking).State = EntityState.Modified;
                    db.SaveChanges();
                }

                facultydetails.NotQualifiedAsperAICTE = faculty.NOTQualifiedAsPerAICTE;
                facultydetails.IncompleteCertificates = faculty.InCompleteCeritificates;
                //This Flag for No Class in Profile faculty flag Name No Class in Ug/PG
                //facultydetails.MultipleRegInSameCollege = faculty.Noclassinugorpg;
                if (faculty.NOrelevantUgFlag == true)
                    facultydetails.NoRelevantUG = "Yes";
                else if (faculty.NOrelevantUgFlag == false)
                    facultydetails.NoRelevantUG = "No";



                if (faculty.NOrelevantPgFlag == true)
                    facultydetails.NoRelevantPG = "Yes";
                else if (faculty.NOrelevantPgFlag == false)
                    facultydetails.NoRelevantPG = "No";


                if (faculty.NOrelevantPhdFlag == true)
                    facultydetails.NORelevantPHD = "Yes";
                else if (faculty.NOrelevantPhdFlag == false)
                    facultydetails.NORelevantPHD = "No";

                //facultydetails.NoSCM17 = faculty.NoSCM17Flag;

                facultydetails.InvalidPANNumber = faculty.InvalidPANNo;
                // facultydetails.NotIdentityfiedForanyProgram = faculty.NotIdentityFiedForAnyProgramFlag;
                //  facultydetails.PhdUndertakingDocumentstatus = faculty.PhdUndertakingDocumentstatus;
                facultydetails.Xeroxcopyofcertificates = faculty.XeroxcopyofcertificatesFlag;

                facultydetails.DeactivatedBy = userID;
                facultydetails.DeactivatedOn = DateTime.Now;

                db.Entry(facultydetails).State = EntityState.Modified;
                db.SaveChanges();
                TempData["Success"] = facultydetails.RegistrationNumber + " Faculty Flags are Changed";

            }

            // return RedirectToAction("FacultyVerificationIndex", "FacultyVerificationDENew", new { collegeid = faculty.CollegeId });
            return RedirectToAction("ViewFacultyDetails", "FacultyVerificationDENew", new { fid = UAAAS.Models.Utilities.EncryptString(faculty.id.ToString(), System.Web.Configuration.WebConfigurationManager.AppSettings["CryptoKey"]) });
        }

        //Admin Rollback the faculty flags from Faculty tracking table
        [Authorize(Roles = "Admin")]
        public ActionResult Facultyflagsrollback(string fid, string collegeid)
        {
            int userid = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int fID = 0;
            TempData["Success"] = null;
            TempData["Error"] = null;
            if (fid != null)
            {
                fID = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(fid, System.Web.Configuration.WebConfigurationManager.AppSettings["CryptoKey"]));
            }
            if (fID != 0)
            {
                var jntuh_registered_faculty = db.jntuh_registered_faculty.Where(s => s.id == fID).Select(e => e).FirstOrDefault();
                if (jntuh_registered_faculty != null)
                {
                    //var facultyflags =
                    //    db.jntuh_college_facultyverification_tracking.Where(
                    //        r => r.registrationNumber == jntuh_registered_faculty.RegistrationNumber.Trim())
                    //        .Select(s => s)
                    //        .FirstOrDefault();
                    //if (facultyflags != null)
                    //{
                    //    jntuh_registered_faculty.NotQualifiedAsperAICTE = facultyflags.NotQualifiedAsperAICTE;
                    //    jntuh_registered_faculty.IncompleteCertificates = facultyflags.IncompleteCertificates;
                    //    jntuh_registered_faculty.InvalidPANNumber = facultyflags.InvalidPANNumber;
                    //    jntuh_registered_faculty.Xeroxcopyofcertificates = facultyflags.Xeroxcopyofcertificates;
                    //    jntuh_registered_faculty.NoRelevantPG = facultyflags.NoRelevantPG == true ? "Yes" : "No";
                    //    jntuh_registered_faculty.NoRelevantUG = facultyflags.NoRelevantUG == true ? "Yes" : "No";
                    //    jntuh_registered_faculty.NORelevantPHD = facultyflags.NoRelevantPHD == true ? "Yes" : "No";

                    //    //jntuh_registered_faculty.MultipleRegInSameCollege = false;
                    //    jntuh_registered_faculty.DeactivatedBy = facultyflags.deactivatedBy;
                    //    jntuh_registered_faculty.DeactivatedOn = facultyflags.deactivatedOn;
                    //    db.Entry(jntuh_registered_faculty).State = EntityState.Modified;
                    //    db.SaveChanges();
                    //    TempData["Success"] = jntuh_registered_faculty.RegistrationNumber + " Faculty Flags Rollback.";
                    //}
                    //else
                    //{
                    //    TempData["Error"] = "No record found in tracking.";
                    //}

                    jntuh_registered_faculty.NotIdentityfiedForanyProgram = null;
                    db.Entry(jntuh_registered_faculty).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Success"] = jntuh_registered_faculty.RegistrationNumber + " Faculty Flags Rollback.";
                }
                else
                    TempData["Error"] = "No Data found.";
            }

            return RedirectToAction("Facultyverificationfouretwozero", new { collegeid = collegeid });
        }

        //419 PHD Faculty Verification Screen
        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification")]
        public ActionResult FacultyverificationPHD(int? collegeid)
        {
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy =
                db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            var colleges = db.jntuh_college.Join(db.jntuh_college_edit_status, co => co.id, e => e.collegeId, (co, e) => new { co = co, e = e }).Where(c => c.co.isActive == true && c.e.academicyearId == prAy && c.e.IsCollegeEditable == false).Select(c => new { collegeId = c.co.id, collegeName = c.co.collegeCode + "-" + c.co.collegeName }).OrderBy(c => c.collegeName).ToList();

            colleges.Add(new { collegeId = 375, collegeName = "TEST COLLEGE" });
            //colleges.Add(new { collegeId = 0, collegeName = "Not Clear PHD Faculty" });
            ViewBag.Colleges = colleges;
            //ViewBag.Colleges = db.jntuh_college.Join(db.jntuh_college_edit_status, co => co.id, e => e.collegeId, (co, e) => new { co = co, e = e }).Where(c => c.co.isActive == true).Select(c => new { collegeId = c.co.id, collegeName = c.co.collegeCode + "-" + c.co.collegeName }).OrderBy(c => c.collegeName).ToList();

            //int[] EditFacultycollegeIds = {4,5,7,8,9,12,20,22,23,24,26,27,29,30,32,35,38,39,40,41,42,43,44,47,48,55,56,58,68,69,70,72,74,75,77,79,80,81,84,85,86,87,88,91,97,100,103,105,106,107,108,109,110,111,116,117,118,119,121,123,127,128,129,130,132,134,135,137,138,139,141,143,144,145,147,148,150,152,153,155,156,157,158,161,162,163,164,165,166,168,171,172,173,174,175,176,177,178,179,180,181,183,184,185,187,189,192,193,195,196,198,202,203,206,210,211,213,214,215,217,218,222,223,225,227,228,234,235,236,241,242,244,245,246,249,250,254,256,260,261,262,264,266,267,271,273,276,282,287,290,291,292,295,296,298,299,300,302,304,307,309,310,313,315,316,318,319,321,322,324,327,329,334,335,336,349,350,352,353,355,360,364,365,366,367,368,369,370,373,374,376,380,382,384,385,389,391,392,393,399,400,411,414,420,421,423,424,428,435,436,439,441,455,375};
            //ViewBag.Colleges =
            //    db.jntuh_college.Where(c => EditFacultycollegeIds.Contains(c.id))
            //        .Select(a => new {collegeId = a.id, collegeName = a.collegeCode + "-" + a.collegeName})
            //        .OrderBy(r => r.collegeName)
            //        .ToList();
            ViewBag.collegeid = collegeid;
            var jntuh_department = db.jntuh_department.ToList();
            List<FacultyRegistration> teachingFaculty = new List<FacultyRegistration>();

            if (collegeid != null)
            {

                var DegreeIdNameBasedOnSpecialization = (from a in db.jntuh_department
                                                         join b in db.jntuh_specialization on a.id equals b.departmentId
                                                         join c in db.jntuh_degree on a.degreeId equals c.id
                                                         select new
                                                         {
                                                             DegreeId = c.id,
                                                             DegreeName = c.degree,
                                                             SpcializationName = b.specializationName,
                                                             Specid = b.id
                                                         }).ToList();

                //418 Faculty Verification Condition 
                //string[] Copy_RegNos = db.jntuh_college_faculty_registered_copy.Where(c => c.collegeId == 375).Select(e => e.RegistrationNumber).Distinct().ToArray();

                // List<jntuh_college_faculty_registered> jntuh_college_faculty_registered_College_Portal = db.jntuh_college_faculty_registered.Where(cf => cf.collegeId == collegeid).Select(cf => cf).ToList();
                //committed by siva due to only edit option faculty is comming case....
                // jntuh_college_faculty_registered = jntuh_college_faculty_registered.Where(c => !Copy_RegNos.Contains(c.RegistrationNumber)).ToList();


                List<jntuh_college_faculty_registered> jntuh_college_faculty_registered = db.jntuh_college_faculty_registered.Where(cf => cf.collegeId == collegeid).Select(cf => cf).ToList();

                //419 Faculty Verifi
                string[] collegeRegnos =
                    jntuh_college_faculty_registered.Select(s => s.RegistrationNumber).Distinct().ToArray();
                string[] Copy_RegNos = db.jntuh_college_faculty_registered_copy.Where(c => c.collegeId == 1419 && collegeRegnos.Contains(c.RegistrationNumber)).Select(e => e.RegistrationNumber).Distinct().ToArray();

                string[] strRegNoS = jntuh_college_faculty_registered.Where(c => !Copy_RegNos.Contains(c.RegistrationNumber)).Select(cf => cf.RegistrationNumber).ToArray();
                string[] PrincipalstrRegNoS = db.jntuh_college_principal_registered.Where(P => P.collegeId == collegeid).Select(P => P.RegistrationNumber.Trim()).ToArray();
                List<jntuh_registered_faculty> jntuh_registered_faculty = new List<jntuh_registered_faculty>();
                ViewBag.TotalClearedFaculty = collegeRegnos.Count() - strRegNoS.Count();
                ViewBag.TotalCollegeFaculty = collegeRegnos.Count();

                //committed by siva due to only edit option faculty is comming case....
                //jntuh_registered_faculty = db.jntuh_registered_faculty.Where(rf => strRegNoS.Contains(rf.RegistrationNumber) && rf.Blacklistfaculy != true)  //&& (rf.collegeId == null || rf.collegeId == collegeid)//&& rf.Notin116 != true
                //                             .ToList();

                //New Code written by siva due to only edit option faculty is comming case....
                DateTime editoptionStartdate = new DateTime(2018, 08, 30);
                if (collegeid == 375)
                {
                    string[] phdnotclearcollegeassociate = new string[]
                    {
                       "26150328-102440","74150329-123802","07150330-011802","94150330-113934","21150330-130127","45150330-135118","24150330-154256","43150330-161340","34150330-174403","85150330-182657","47150331-113435","94150331-122435","85150331-143942","37150331-201236","53150331-204441","72150401-095414","32150401-121708","22150401-125835","78150401-132922","63150401-151206","17150401-154407","72150401-171750","17150401-192409","82150401-193318","53150401-204835","66150401-210720","03150401-222957","02150402-111353","68150402-113151","90150402-113500","29150402-115939","81150402-122531","44150402-125710","48150402-130357","02150402-134808","84150402-135106","90150402-140326","66150402-143755","68150402-144056","60150402-151814","71150402-153329","14150402-155349","76150402-165017","94150402-165951","68150403-015716","89150403-074443","74150403-101252","36150403-130305","79150403-133318","77150403-144321","70150403-145538","50150403-154325","26150403-155145","12150403-161135","48150403-190656","29150403-194155","02150403-220700","85150403-230538","20150404-015659","41150404-111625","23150404-113644","64150404-113919","13150404-123131","07150404-125837","76150404-132522","14150404-132654","06150404-132713","74150404-132947","98150404-151104","72150404-151751","12150404-154539","86150404-154907","16150404-155121","83150404-160632","67150404-161438","83150404-162428","04150404-162859","61150404-163123","91150404-163458","04150404-164306","15150404-165417","32150404-172740","29150404-175402","28150404-175504","66150404-195728","50150405-080016","63150405-105142","58150405-142724","70150405-151757","01150405-154759","78150405-203601","48150405-232624","91150406-023716","58150406-100658","50150406-110631","48150406-111721","39150406-112650","58150406-112805","75150406-114801",
                       "93150406-121602","16150406-121938","16150406-123557","39150406-124038","00150406-124851","97150406-124918","46150406-125500","78150406-131451","31150406-132701","84150406-134458","73150406-141234","80150406-145457","81150406-145946","38150406-151849","71150406-154014","81150406-154016","66150406-154147","51150406-154756","53150406-161513","88150406-161753","47150406-163118","97150406-163314","35150406-172430","69150406-174533","83150406-190942","98150406-191738","52150406-194954","77150407-100906","26150407-101433","12150407-102002","00150407-105515","88150407-105745","74150407-110315","06150407-110409","08150407-111738","60150407-112825","54150407-113636","49150407-114156","97150407-114851","92150407-114946","94150407-115002","78150407-121739","73150407-124535","39150407-125323","26150407-125656","73150407-125907","85150407-125912","49150407-132629","16150407-133453","77150407-135637","72150407-140037","38150407-141045","39150407-142712","24150407-144012","93150407-151138","67150407-152411","52150407-152647","42150407-154522","13150407-160822","91150407-164824","86150407-165203","66150407-165442","04150407-173505","6979-150408-070114","9714-150408-101414","3786-150408-101716","6945-150408-105112","1666-150408-110342","9352-150408-112832","9588-150408-114822","7663-150408-120134","3046-150408-123016","5382-150408-123033","7525-150408-124513","0254-150408-125530","5043-150408-125708","3539-150408-131514","4245-150408-132734","2411-150408-134853","8792-150408-140040","5826-150408-151029","6082-150408-155301","7167-150408-170456","2724-150408-171821","6405-150408-172409","0149-150408-213453","0457-150408-215923","4802-150409-000442","7860-150409-093946","5346-150409-103615","7624-150409-103723","9811-150409-103922",
                       "9420-150409-104558","1608-150409-104757","3556-150409-105506","0055-150409-105615","3743-150409-112440","8127-150409-121259",
                       "3521-150409-121615","8748-150409-122037","3345-150409-122730","2802-150409-125545","5685-150409-131541","3624-150409-132438","0377-150409-133622","5801-150409-143524","0816-150409-144756","2890-150409-150716","5763-150409-155228","1710-150409-162323","9091-150409-165116","8690-150409-173054","8835-150409-215651","4681-150410-101558","0123-150410-101600","0209-150410-101942","9079-150410-103817","3133-150410-105936","4822-150410-114424","3509-150410-115632","7534-150410-120811","3151-150410-125404","9946-150410-132838","5294-150410-135203","2051-150410-135522","9708-150410-144632","4610-150410-155151","7490-150411-125727","1955-150411-132435","1034-150411-145802","9454-150411-155023","4370-150411-182858","7362-150412-131129","0076-150412-140821","3453-150412-185020","8441-150413-113304","8921-150413-122236","1019-150413-125218","5751-150413-125449","9288-150413-125630","8223-150413-130432","7118-150413-130756","2810-150413-132540","2748-150413-133433","0861-150413-142903","9165-150413-144341","3285-150413-145210","0622-150413-150200","5958-150413-154246","5931-150413-155104","1291-150413-162334","5286-150413-164156","1818-150413-164538","4114-150413-164740","6344-150413-170725","9692-150413-174620","2876-150413-192246","4523-150413-200655","1324-150413-200901","0321-150413-212744","2177-150414-063346","7598-150414-111151","4854-150414-125905","6441-150414-142344","1191-150414-182314","4024-150414-201804","5985-150415-112044","6619-150415-115130","2697-150415-115426","1268-150415-140422","2639-150415-144606","0459-150415-162506","8713-150415-223120","4310-150415-235250","0343-150416-102005","1898-150416-103606","8636-150416-130230","8818-150416-131333","5417-150416-173107","1354-150417-115731","2518-150417-130617","0920-150417-131040","4699-150417-132914","5498-150417-133552","1546-150417-152331","6858-150417-153743","9369-150417-214628","9695-150418-094635","0255-150418-113623","4549-150418-133640","1057-150418-141941","7730-150418-150918","0037-150418-151850","8184-150418-160647","1499-150418-162700","3992-150419-080217","8162-150419-091405","2759-150419-112924","8382-150419-134710","3918-150420-101926","9585-150420-105758","0936-150420-114612","5218-150420-155112","5614-150420-155913","6931-150421-013104","7479-150421-105533","2644-150421-125943","2274-150421-163320","0004-150422-121538","5574-150422-123317","7164-150422-204237","3961-150424-120438","1897-150425-151950","6283-150425-165254","1339-150425-223809","9839-150426-103701","3119-150426-160711","3969-150427-114410","2701-150427-155527","8285-150427-160311","2787-150427-172144","2257-150428-055331","8599-150428-123056","4897-150428-162757","9127-150429-155346","8544-150429-203133","1965-150430-110931","0330-150430-152111","9828-150501-044104","0004-150502-142242","8640-150504-132831","4571-150504-181413","1804-150504-223903","2916-150505-111623","7794-150506-111930","2705-150506-170154","2351-150507-075609","6386-150507-161158","7149-150507-192126","9233-150507-194931","1693-150508-154732","5617-150513-152145","5307-150605-143306","4896-150619-162217","8276-150620-203646","2694-150622-111010","0183-150623-110537","0224-150623-130257","6281-150626-112130","8008-150702-132923","2831-151216-143314","3428-151219-150017","2468-151222-143411","4716-151222-144026","2098-151228-160426","2488-160108-160128","9189-160112-211310","4999-160118-121910","8710-160123-150322","6879-160127-141445","9785-160127-143316",
                       "6672-160128-161546","2530-160130-131822","7516-160130-171629","3738-160130-172215","8187-160201-143225","8795-160203-175048","7783-160204-153400","3930-160204-155854","8388-160209-104546","9263-160209-165332","8313-160209-172511","1082-160210-102657","6278-160210-150833","2439-160211-123708","8343-160212-153801","6456-160213-103822","4874-160215-165007","6661-160216-162733","0143-160217-160943","1127-160218-105422","2880-160218-223616","3431-160219-135556","0653-160220-110651","3832-160221-111516","9683-160221-140840","6775-160223-100217","8787-160223-142327","7447-160223-160623","5772-160224-120936","0562-160224-172550","1840-160225-112218","2582-160225-154756","9257-160226-122157","4763-160226-172443","2772-160226-193511","7805-160227-114301","8994-160227-143825","4320-160228-133043","8506-160228-150736","1932-160229-073352","4865-160229-143431","6161-160302-130228","1959-160303-111849","4984-160303-115900","0124-160303-185802","7767-160304-114226","6364-160304-122123","9340-160304-135148","8686-160304-154330","6012-160304-160526","6631-160305-114438","0462-160305-131508","3068-160305-201328","7957-160305-202129","3695-160306-065942","2185-160306-125348","1061-160306-221302","7715-160307-094837","1881-160307-111846","2875-160307-112908","1097-160307-135134","3525-160307-141516","7008-160307-141820","9178-160307-144617","0663-160308-152723","2949-160309-110305","2397-160310-142830","3654-160310-153344","7206-160311-141220","0012-160311-144543","2232-160312-110412","4712-160313-143055","1581-160313-152759","1716-160313-155737","3592-160313-222024","9853-160314-115912","2841-160315-000843","4454-160315-024113","1303-160315-103412","8250-160315-111108","5152-160315-120421","3370-160315-134028","0398-160315-143304","0871-160315-145356","7613-160315-162203","7900-160315-170924","4841-160315-184911","7440-160315-210929","0687-160316-122027","7356-160316-133849","9805-160316-211552","4497-160318-164821","1889-160319-164942","6387-160319-165638","9268-160320-150307","2892-160320-155417","2051-160524-153249","6614-160525-111651","6266-160526-122437","0568-160529-145949","1959-160529-184524","4825-160529-193005","7691-160804-172710","1778-161022-132131","6213-161022-184405","5675-161024-151653","3962-161024-194724","4847-161024-210125","7684-161024-224045","3077-161025-092554","3448-161025-195900","2784-161027-112616","0661-161031-001234","6785-161031-114837","6391-161101-112549","3055-161125-201020","9905-161202-142028","1194-161205-171645","9665-161207-131759","7595-161207-140122","1736-161207-142623","0089-161209-145730","4357-161210-150550","1988-161213-123229","1886-161215-110840","3259-161221-153152","0368-161227-154357","6447-161229-061433","5963-161230-134120","7984-170103-155512","6532-170104-113621","4374-170104-125349","3745-170105-143145","5118-170106-120828","4779-170106-123241","8084-170106-141141","3439-170106-143940","7030-170107-122049","4843-170108-135437","2773-170109-102839","7757-170109-141614","9801-170110-065949","9614-170110-095637","0767-170111-102126","7111-170111-113028","0846-170111-120917","6217-170112-095414","9510-170112-102057","6981-170112-104726","2167-170112-111334","4973-170113-202737","4473-170115-183328","3050-170116-142118","7353-170117-121726","9202-170118-101846","9661-170119-095107","8446-170119-141054","3169-170121-103040","5203-170122-115917","2999-170122-190110","0311-170122-203935","1932-170122-210432","0987-170125-114032","4464-170126-024023","8631-170126-075712","3573-170126-082633","4801-170126-125032","3830-170127-055112","2054-170127-102825","6108-170128-041206","4364-170129-111927","8402-170130-042424","6283-170130-062336","4731-170130-143850","7953-170131-025801","7999-170131-184403","7050-170131-192946","1286-170131-201713","9213-170131-224323","2639-170201-061450","6590-170201-091037","6553-170201-092800","1332-170201-103806","8585-170201-120216","7401-170201-135706","1423-170201-231848","7735-170202-021128","2951-170202-052356","8348-170202-174618","5992-170203-094437","4054-170204-110401","0617-170204-180432","7893-170206-154526","9568-170207-150317","3573-170207-162957","8236-170207-202324","9550-170208-114920","6641-170208-130829","4300-170208-163059","1255-170208-172403","1089-170208-175418","8673-170209-001735","4717-170212-193526","0340-170213-144245","0386-170213-173208","5798-170213-181815","2413-170520-123446","2704-170520-130953","0398-170521-100904","0647-170521-101358","0697-170521-115623","3919-170521-115934","8488-170521-214919","8423-170522-001654","5730-170522-100531","1539-170522-113513","4265-170522-122220","0043-170522-131138","4232-170522-141133","1803-170522-170602","8348-170522-171005","9020-170522-171446","1799-170523-145346","2982-170523-172149","9242-170523-172200","9348-170523-173650","0228-170523-212632","1336-170911-162939","8004-170911-225045","5826-170913-103020","4439-170913-131636","9504-170913-132727","0012-170914-122955","3993-170914-140249","6336-170914-140628","3040-170915-115017","7770-170915-144641","0897-170915-161247","4373-170915-165932","3792-170915-171037","6907-170916-103001","9760-170916-124258","9857-170916-131945","0861-170916-141035","7136-170918-135738","7220-170918-153900","7454-171009-094302","5057-171021-105147","4917-171022-202121","5784-171106-142610","0191-171129-114721","2324-171208-105321","2460-171220-144420","6404-171221-160659","6053-171221-173151","0895-171222-113335","5089-171223-141800","7837-171225-073920","9324-171225-083429","0553-171226-122921","4273-171226-143405","7329-171227-111349","3286-171227-145359","1938-171227-190729","6366-150408-203914","0520-180102-220148","1416-180104-125600","7227-180106-110314","2166-180109-173401","0105-180110-112802","0479-180110-220632","8470-150427-103627","5916-180127-115342","6887-180127-160016","5252-180127-160522","3908-180129-122453","1624-180129-143004","5582-180131-143419","4460-180131-172933","1704-180201-132810","4463-180201-140846","4032-180201-151029","8166-180201-155956","3109-180201-163126","2117-180201-163235","8993-180201-172018","1878-180201-172209","4539-180201-172827","5025-180201-191319","4189-180201-202900","5921-180202-095859","5308-180202-115849","9562-180202-121510","3554-180202-123255","3367-180202-132237","6557-180202-145448","6324-180202-145631","9285-180202-153742","8741-180202-162322","8361-180202-165230","4248-180202-171805","8196-180202-172813","5990-180202-173614","7893-180202-182120","5905-180203-132313","9882-180203-163139","9122-180204-153030","9662-180204-211536","7860-180207-125814","7617-180207-150550","0704-180208-133828","7727-180208-134717","1747-180208-144342","2425-180208-152024","3887-180208-154154","0755-180208-162637","8667-180208-205639","0961-180208-210002","1834-180208-210350","6307-180208-211616","2182-180208-233008","9966-180209-140843","9447-180212-120756","3697-180216-140031","2164-180219-145739","1076-180220-234151","0275-180413-225756","9375-180414-103858","5603-180414-154024","3162-180415-193439","7990-180416-105441","2754-180416-124114","3529-180416-125522","5838-180416-133809","0545-180416-150913","5778-180416-152538","7707-180416-160218","0123-180416-162437","4607-180416-164009","8593-180416-164709","5837-180416-170400","8852-180416-175025","0704-180416-194821","7228-180416-212014","3554-180416-224025","2909-180417-105850","6846-180417-113033","3130-180417-121250","1949-180417-122625","4120-180417-123136","4053-180417-132839","1127-180417-133316","7341-180417-143139","9150-180417-144644","6243-180417-151242","4380-180417-151524","6826-180417-152947","7662-180417-154826","8584-180417-155254","1078-180417-171508","3049-180417-171953","6669-180417-180422","3173-180417-180447","2738-180417-181119","1593-180417-185823","4250-180417-185913","2149-180419-104552","3674-180419-135113","6072-180419-143930","7026-180419-150643","4502-180419-155035","6759-180420-105440","6503-180420-154506","2609-180420-181501","5098-180421-091752","2754-180421-105428","2182-180421-114834","2277-180421-130154","4427-180421-131451","4938-180421-141422","7867-180421-143302","4540-180421-150439","7594-180421-150604","7372-180421-150919","5980-180421-152003","9481-180421-153750","9537-180421-154149","5885-180421-154309","3135-180421-155425","5776-180421-155710","1535-180421-160729","5702-180421-160738","1154-180421-161510","7089-180421-161804","1468-180421-165652","9807-180421-171303","5420-180421-181108","1924-180421-195320","8173-180421-200729","6432-180502-122013","0485-180702-171940","9715-180703-001326","6481-180703-075013","3511-180703-123706","2605-180703-133913","6954-180703-140314","8772-180703-154939","9392-180704-001253","7095-180705-111442","7814-180707-121720","4802-180709-150750","2231-180710-102723","7948-180710-121316","9418-180711-145210","7203-180711-153007","0294-180712-110352","9959-180713-002900","1744-180713-010015","5651-180713-124217","0602-180713-161550","0467-180716-115522","2227-180716-160856","5694-180717-164555","0003-180718-111543","8650-180719-124256","5267-180719-151738","3929-180721-133143","6877-180723-180939","4480-180725-152021","1689-180726-094624","9302-180727-141317","1147-180727-144227","4812-180730-105523","4472-180730-152624","5116-180802-153604","0081-180803-123759","5431-180805-232946","7184-180807-114846","5422-180808-112307","3861-180808-115346","1451-180808-161540","4474-180809-203945","2695-180810-140035","9669-180810-143812","5281-180810-144406","8167-180810-151024","4969-180811-131709","2122-180811-132049","7456-180811-155448","0621-180811-193222","0743-180811-212548","3747-180811-214805","5995-180811-222347","5981-180812-093328","4834-180813-111346","3067-180813-115313","0532-180813-124715","7790-180813-132221","8596-180813-153655","6247-180813-234819","6147-180814-133656","1470-180816-135632","9367-180816-145749","1818-180816-165806","5104-180817-121912","5972-180818-104334","5653-180818-111615","7572-180818-112007","9018-180818-121645","8124-180818-122041","6166-180818-122732","4636-180818-123716","5106-180818-124347","5296-180818-132231","6560-180818-133117","3010-180818-140606","9286-180818-151917","0213-180818-173746","1232-180819-222940","1294-180820-114846","4935-180820-115112","6570-180820-121747","2319-180820-142857","2417-180820-144805","2171-180820-144953","8275-180820-152356","0077-180820-161226","8639-180820-161745","1215-180820-170112","9739-180820-182445","1004-180820-191254","2373-180820-194644","8347-180824-213250","4323-180827-115910","1684-180827-134553","6147-180914-121328","6446-180914-122927","5806-180914-144231","3010-180918-101949","5612-181013-105443","6680-181015-151313","8783-181016-162700","8556-181024-121524","9531-181025-122843","9887-181105-140926","4830-181106-124446","2591-181112-174044","6187-181114-121259","1312-181115-120913","5851-190118-142407","7694-190118-144316","5629-190118-144514","7733-190118-145753","5720-190118-150450","8727-190118-201047","7610-190119-095241","5633-190119-142303","8766-190119-161519","1172-190119-225243","2803-190120-143634","9878-190121-202106","6980-190124-165329","2837-190128-143758","6370-190131-113843","9860-190202-110439","6894-190203-111227","5186-190204-133550","1381-190206-115752","0068-190207-153803","3716-190209-131236","2688-190211-154811","5221-190212-125443","5163-190212-160516","1436-190213-130310","2442-190214-133013","4796-190215-095908","6452-190215-103755","4350-190215-111559","3663-190215-141957","6397-190216-121426","9192-190218-011007","3607-190218-014535","2381-190218-111934","9525-190218-132152","1883-190218-155920","5076-190218-161729","5661-190218-175703","2399-190219-101751","2493-190219-123238","6433-190219-145341","2225-190219-152148","0739-190219-155159","2784-190219-171631","9366-190219-175153","0698-190219-224953","7389-190220-114326","0824-190220-115619","7679-190220-121213","3784-190220-121328","8670-190220-124032","3537-190220-144812","7983-190220-152408","5398-190220-152508","8379-190220-161816","6170-190220-163500","9560-190220-170729","4695-190220-171822","0086-190220-180221","4852-190220-185751","4313-190221-105212","5447-190221-142836","6937-190221-150359","3078-190221-152149","6815-190221-152826","8563-190221-154413","1040-190221-160040","5016-190221-160822","9744-190221-161632","5254-190221-163148"
                    };

                    jntuh_registered_faculty = db.jntuh_registered_faculty.Where(rf => phdnotclearcollegeassociate.Contains(rf.RegistrationNumber) && rf.Blacklistfaculy != true)
                                           .ToList();
                    //jntuh_registered_faculty = db.jntuh_registered_faculty.Where(rf => phdnotcollegeassociate.Contains(rf.RegistrationNumber) && rf.Blacklistfaculy != true && rf.updatedOn >= editoptionStartdate)
                    //                       .ToList();
                }
                else
                {
                    jntuh_registered_faculty = db.jntuh_registered_faculty.Where(rf => strRegNoS.Contains(rf.RegistrationNumber) && rf.Blacklistfaculy != true)  //&& (rf.collegeId == null || rf.collegeId == collegeid)//&& rf.Notin116 != true
                                            .ToList();
                }



                //var jntuh_faculty = db.jntuh_registered_faculty.Where(rf => rf.Blacklistfaculy != true && rf.updatedOn >= editoptionStartdate)  //&& (rf.collegeId == null || rf.collegeId == collegeid)//&& rf.Notin116 != true
                //                            .ToList();

                var Specializations = db.jntuh_specialization.ToList();

                string RegNumber = "";
                int? Specializationid = 0;
                int? CollegeDepartmentId = 0;
                int inactivephdfaculty = 0;
                foreach (var a in jntuh_registered_faculty)
                {
                    int educationid =
                        db.jntuh_registered_faculty_education.Where(e => e.facultyId == a.id && e.educationId == 6)
                            .Select(s => s.educationId)
                            .FirstOrDefault();
                    string Reason = String.Empty;
                    if (collegeid == 375)
                    {
                        Specializationid =
                          jntuh_college_faculty_registered.Where(
                              C => C.RegistrationNumber.Trim() == a.RegistrationNumber.Trim())
                              .Select(C => C.SpecializationId)
                              .FirstOrDefault();
                        CollegeDepartmentId =
                            jntuh_college_faculty_registered.Where(
                                C => C.RegistrationNumber.Trim() == a.RegistrationNumber.Trim())
                                .Select(C => C.DepartmentId)
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
                        //Hear showing JNTU Specilazation only Pharmacy Faculty
                        faculty.PGSpecializationName = a.Jntu_PGSpecializationId != null
                            ? DegreeIdNameBasedOnSpecialization.Where(e => e.Specid == a.Jntu_PGSpecializationId)
                                .Select(e => e.DegreeName + "-" + e.SpcializationName)
                                .FirstOrDefault()
                            : "";
                        faculty.GenderId = a.GenderId;
                        faculty.Email = a.Email;
                        faculty.facultyPhoto = a.Photo;
                        faculty.Mobile = a.Mobile;
                        faculty.PANNumber = a.PANNumber;
                        faculty.AadhaarNumber = a.AadhaarNumber;
                        faculty.isActive = a.isActive;
                        faculty.isApproved = a.isApproved;
                        faculty.department = CollegeDepartmentId > 0
                            ? jntuh_department.Where(d => d.id == CollegeDepartmentId)
                                .Select(d => d.departmentName)
                                .FirstOrDefault()
                            : "";
                        faculty.SamePANNumberCount =
                            jntuh_registered_faculty.Where(f => f.PANNumber == a.PANNumber).ToList().Count;
                        faculty.SameAadhaarNumberCount =
                            jntuh_registered_faculty.Where(f => f.AadhaarNumber == a.AadhaarNumber).ToList().Count;
                        faculty.SpecializationIdentfiedFor = Specializationid > 0
                            ? DegreeIdNameBasedOnSpecialization.Where(S => S.Specid == Specializationid)
                                .Select(S => S.DegreeName + "-" + S.SpcializationName)
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
                        faculty.Noclassinugorpg = a.Noclass != null
                            ? (bool)a.Noclass
                            : false;
                        faculty.XeroxcopyofcertificatesFlag = a.Xeroxcopyofcertificates != null
                            ? (bool)a.Xeroxcopyofcertificates
                            : false;
                        faculty.NotIdentityFiedForAnyProgramFlag = a.NotIdentityfiedForanyProgram != null
                            ? (bool)a.NotIdentityfiedForanyProgram
                            : false;
                        faculty.NOrelevantUgFlag = a.NoRelevantUG == "No" ? false : true;
                        faculty.NOrelevantPgFlag = a.NoRelevantPG == "No" ? false : true;
                        faculty.NOrelevantPhdFlag = a.NORelevantPHD == "No" ? false : true;
                        //faculty.NoForm16Verification = a.Noform16Verification !=null ? (bool)a.Noform16Verification : false;
                        faculty.NoSCM17Flag = a.NoSCM17 != null ? (bool)a.NoSCM17 : false;
                        //faculty.PhotocopyofPAN = a.PhotoCopyofPAN != null ? (bool)a.PhotoCopyofPAN : false;
                        faculty.PhdUndertakingDocumentstatus = a.PhdUndertakingDocumentstatus != null
                            ? (bool)(a.PhdUndertakingDocumentstatus)
                            : false;
                        faculty.PHDUndertakingDocumentView = a.PHDUndertakingDocument;
                        faculty.PhdUndertakingDocumentText = a.PhdUndertakingDocumentText;
                        //faculty.AppliedPAN = a.AppliedPAN != null ? (bool)(a.AppliedPAN) : false;
                        //faculty.SamePANUsedByMultipleFaculty = a.SamePANUsedByMultipleFaculty != null
                        //? (bool)(a.SamePANUsedByMultipleFaculty)
                        //: false;
                        //faculty.Basstatus = a.BASStatusOld;
                        faculty.Deactivedby = a.DeactivatedBy;
                        faculty.DeactivedOn = a.DeactivatedOn;
                        teachingFaculty.Add(faculty);
                    }
                    else if (educationid == 6)
                    {
                        Specializationid =
                            jntuh_college_faculty_registered.Where(
                                C => C.RegistrationNumber.Trim() == a.RegistrationNumber.Trim())
                                .Select(C => C.SpecializationId)
                                .FirstOrDefault();
                        CollegeDepartmentId =
                            jntuh_college_faculty_registered.Where(
                                C => C.RegistrationNumber.Trim() == a.RegistrationNumber.Trim())
                                .Select(C => C.DepartmentId)
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

                        //Hear showing JNTU Specilazation only Pharmacy Faculty
                        faculty.PGSpecializationName = a.Jntu_PGSpecializationId != null
                            ? DegreeIdNameBasedOnSpecialization.Where(e => e.Specid == a.Jntu_PGSpecializationId)
                                .Select(e => e.DegreeName + "-" + e.SpcializationName)
                                .FirstOrDefault()
                            : "";
                        faculty.GenderId = a.GenderId;
                        faculty.Email = a.Email;
                        faculty.facultyPhoto = a.Photo;
                        faculty.Mobile = a.Mobile;
                        faculty.PANNumber = a.PANNumber;
                        faculty.AadhaarNumber = a.AadhaarNumber;
                        faculty.isActive = a.isActive;
                        faculty.isApproved = a.isApproved;
                        faculty.department = CollegeDepartmentId > 0
                            ? jntuh_department.Where(d => d.id == CollegeDepartmentId)
                                .Select(d => d.departmentName)
                                .FirstOrDefault()
                            : "";
                        faculty.SamePANNumberCount =
                            jntuh_registered_faculty.Where(f => f.PANNumber == a.PANNumber).ToList().Count;
                        faculty.SameAadhaarNumberCount =
                            jntuh_registered_faculty.Where(f => f.AadhaarNumber == a.AadhaarNumber).ToList().Count;
                        faculty.SpecializationIdentfiedFor = Specializationid > 0
                            ? DegreeIdNameBasedOnSpecialization.Where(S => S.Specid == Specializationid)
                                .Select(S => S.DegreeName + "-" + S.SpcializationName)
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
                        faculty.Noclassinugorpg = a.Noclass != null
                            ? (bool)a.Noclass
                            : false;
                        faculty.XeroxcopyofcertificatesFlag = a.Xeroxcopyofcertificates != null
                            ? (bool)a.Xeroxcopyofcertificates
                            : false;
                        faculty.NotIdentityFiedForAnyProgramFlag = a.NotIdentityfiedForanyProgram != null
                            ? (bool)a.NotIdentityfiedForanyProgram
                            : false;
                        faculty.NOrelevantUgFlag = a.NoRelevantUG == "No" ? false : true;
                        faculty.NOrelevantPgFlag = a.NoRelevantPG == "No" ? false : true;
                        faculty.NOrelevantPhdFlag = a.NORelevantPHD == "No" ? false : true;
                        //faculty.NoForm16Verification = a.Noform16Verification !=null ? (bool)a.Noform16Verification : false;
                        faculty.NoSCM17Flag = a.NoSCM17 != null ? (bool)a.NoSCM17 : false;
                        //faculty.PhotocopyofPAN = a.PhotoCopyofPAN != null ? (bool)a.PhotoCopyofPAN : false;
                        faculty.PhdUndertakingDocumentstatus = a.PhdUndertakingDocumentstatus != null
                            ? (bool)(a.PhdUndertakingDocumentstatus)
                            : false;
                        faculty.PHDUndertakingDocumentView = a.PHDUndertakingDocument;
                        faculty.PhdUndertakingDocumentText = a.PhdUndertakingDocumentText;
                        //faculty.AppliedPAN = a.AppliedPAN != null ? (bool)(a.AppliedPAN) : false;
                        faculty.SamePANUsedByMultipleFaculty = a.Noclass != null
                            ? (bool)(a.Noclass)
                            : false;
                        faculty.Basstatus = a.BAS;
                        faculty.Deactivedby = a.DeactivatedBy;
                        faculty.DeactivedOn = a.DeactivatedOn;
                        teachingFaculty.Add(faculty);
                    }
                    if (collegeid != 375 && educationid == 6)
                    {
                        inactivephdfaculty++;
                    }
                }
                teachingFaculty = teachingFaculty.Where(m => m.isActive == true).OrderBy(f => f.updatedOn).ThenBy(f => f.department).ToList();
                //|| item.NoForm16Verification == true ||item.NOForm16 == true ||
                ViewBag.TotalFaculty = teachingFaculty.Count();
                ViewBag.Inactivephdfaculty = inactivephdfaculty;
                ViewBag.FlagTotalFaculty = teachingFaculty.Where(item =>
                            item.Absent == true ||
                            item.NOTQualifiedAsPerAICTE == true ||
                            item.InCompleteCeritificates == true || item.MultipleReginSamecoll == true ||
                             item.NOrelevantUgFlag == true ||
                            item.NOrelevantPgFlag == true || item.NOrelevantPhdFlag == true ||
                            item.NotIdentityFiedForAnyProgramFlag == true ||
                            item.NoSCM17Flag == true || item.InvalidPANNo == true ||
                            item.DegreeId < 4 || item.PANNumber == null ||
                            item.BlacklistFaculty == true || item.Type == "Adjunct" ||
                            item.AppliedPAN == true || item.SamePANUsedByMultipleFaculty == true || item.XeroxcopyofcertificatesFlag == true
                           || (item.PhdUndertakingDocumentstatus == false)
                            || item.Basstatus == "N").Select(e => e).Count();
                ViewBag.ClearFaculty = ViewBag.TotalFaculty - ViewBag.FlagTotalFaculty;
                return View(teachingFaculty.OrderBy(t => t.id));
            }

            return View(teachingFaculty.OrderBy(t => t.id));
        }

        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification")]
        public ActionResult AdminFacultyVerificationFlagsEdit(string fid, string collegeid)
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




                regFaculty.Absent = faculty.Absent != null ? (bool)faculty.Absent : false;
                regFaculty.OriginalCertificatesnotshownFlag = faculty.OriginalCertificatesNotShown != null ? (bool)faculty.OriginalCertificatesNotShown : false;
                regFaculty.XeroxcopyofcertificatesFlag = faculty.Xeroxcopyofcertificates != null ? (bool)faculty.Xeroxcopyofcertificates : false;
                regFaculty.NOTQualifiedAsPerAICTE = faculty.NotQualifiedAsperAICTE != null ? (bool)faculty.NotQualifiedAsperAICTE : false;
                regFaculty.NoSCMFlag = faculty.NoSCM != null ? (bool)faculty.NoSCM : false;
                regFaculty.InCompleteCeritificates = faculty.IncompleteCertificates != null ? (bool)faculty.IncompleteCertificates : false;
                regFaculty.BlacklistFaculty = faculty.Blacklistfaculy != null ? (bool)faculty.Blacklistfaculy : false;
                regFaculty.NotIdentityFiedForAnyProgramFlag = faculty.NotIdentityfiedForanyProgram != null ? (bool)faculty.NotIdentityfiedForanyProgram : false;
                regFaculty.NOrelevantUgFlag = faculty.NoRelevantUG == "No" ? false : true;
                regFaculty.NOrelevantPgFlag = faculty.NoRelevantPG == "No" ? false : true;
                regFaculty.NOrelevantPhdFlag = faculty.NORelevantPHD == "No" ? false : true;
                regFaculty.InvalidPANNo = faculty.InvalidPANNumber != null ? (bool)faculty.InvalidPANNumber : false;
                regFaculty.OriginalsVerifiedUG = faculty.OriginalsVerifiedUG != null ? (bool)faculty.OriginalsVerifiedUG : false;
                regFaculty.OriginalsVerifiedPHD = faculty.OriginalsVerifiedPHD != null ? (bool)faculty.OriginalsVerifiedPHD : false;
                regFaculty.InvalidDegree = faculty.Invaliddegree != null ? (bool)faculty.Invaliddegree : false;
                regFaculty.InvalidAadhaarFlag = faculty.InvalidAadhaar == "Yes" ? true : false;
                regFaculty.BasStatusFlag = faculty.BAS == "Yes" ? true : false;
                regFaculty.NoClass = faculty.Noclass != null ? (bool)faculty.Noclass : false;
                regFaculty.GenuinenessnotSubmitted = faculty.Genuinenessnotsubmitted != null ? (bool)faculty.Genuinenessnotsubmitted : false;
                regFaculty.VerificationStatusFlag = faculty.AbsentforVerification != null ? (bool)faculty.AbsentforVerification : false;
                regFaculty.NotconsiderPhd = faculty.NotconsideredPHD != null ? (bool)faculty.NotconsideredPHD : false;
                regFaculty.NoPgSpecialization = faculty.NoPGspecialization != null ? (bool)faculty.NoPGspecialization : false;
                regFaculty.NOForm26AS = faculty.NoForm26AS != null ? (bool)faculty.NoForm26AS : false;
                regFaculty.Covid19 = faculty.Covid19 != null ? (bool)faculty.Covid19 : false;
                regFaculty.Maternity = faculty.Maternity != null ? (bool)faculty.Maternity : false;
                regFaculty.isApproved = faculty.isApproved;
                regFaculty.isView = true;
                regFaculty.DeactivationReason = faculty.DeactivationReason;
            }
            return PartialView("_AdminFacultyVerificationFlagsEdit", regFaculty);
        }
        //419 Faculty Verifiaction adding flages
        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification")]
        public ActionResult AdminFacultyVerificationFlagsPostDENew(FacultyRegistration faculty)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            var facultydetails = db.jntuh_registered_faculty.Where(i => i.RegistrationNumber == faculty.RegistrationNumber).FirstOrDefault();
            if (facultydetails != null)
            {
                //Faculty Flages are added for Tracking perpose by Narayana Reddy in 419 only Faculty Verification flags
                jntuh_college_facultyverification_tracking updatefacultytracking =
                    db.jntuh_college_facultyverification_tracking.Where(
                        r => r.registrationNumber == facultydetails.RegistrationNumber.Trim())
                        .Select(s => s)
                        .FirstOrDefault();
                if (updatefacultytracking == null)
                {
                    jntuh_college_facultyverification_tracking facultyTracking = new jntuh_college_facultyverification_tracking();
                    facultyTracking.academicyearId = 11;
                    facultyTracking.registrationNumber = facultydetails.RegistrationNumber.Trim();
                    facultyTracking.collegeId = db.jntuh_college_faculty_registered.Where(c => c.RegistrationNumber == facultydetails.RegistrationNumber.Trim()).Select(s => s.collegeId).FirstOrDefault();
                    facultyTracking.NotQualifiedAsperAICTE = facultydetails.NotQualifiedAsperAICTE;
                    facultyTracking.IncompleteCertificates = facultydetails.IncompleteCertificates;
                    facultyTracking.InvalidPANNumber = facultydetails.InvalidPANNumber;
                    facultyTracking.Xeroxcopyofcertificates = facultydetails.Xeroxcopyofcertificates;
                    facultyTracking.NoSCM = facultydetails.NoSCM;
                    facultyTracking.AadhaarFlag = facultydetails.InvalidAadhaar == "Yes" ? true : false;
                    facultyTracking.BasFlag = facultydetails.BAS == "Yes" ? true : false;
                    facultyTracking.Absent = facultydetails.Absent;
                    facultyTracking.Blacklistfaculy = facultydetails.Blacklistfaculy;
                    facultyTracking.VerificationStatus = facultydetails.AbsentforVerification;

                    if (!String.IsNullOrEmpty(facultydetails.NoRelevantUG))
                        facultyTracking.NoRelevantUG = facultydetails.NoRelevantUG == "Yes"
                            ? true
                            : false;
                    if (!String.IsNullOrEmpty(facultydetails.NoRelevantPG))
                        facultyTracking.NoRelevantPG = facultydetails.NoRelevantPG == "Yes"
                            ? true
                            : false;
                    if (!String.IsNullOrEmpty(facultydetails.NORelevantPHD))
                        facultyTracking.NoRelevantPHD = facultydetails.NORelevantPHD == "Yes"
                            ? true
                            : false;
                    facultyTracking.createdBy = userID;
                    facultyTracking.createdOn = DateTime.Now;
                    db.jntuh_college_facultyverification_tracking.Add(facultyTracking);
                    db.SaveChanges();
                }
                else
                {
                    updatefacultytracking.collegeId = db.jntuh_college_faculty_registered.Where(c => c.RegistrationNumber == facultydetails.RegistrationNumber.Trim()).Select(s => s.collegeId).FirstOrDefault();
                    updatefacultytracking.NotQualifiedAsperAICTE = facultydetails.NotQualifiedAsperAICTE;
                    updatefacultytracking.IncompleteCertificates = facultydetails.IncompleteCertificates;
                    updatefacultytracking.InvalidPANNumber = facultydetails.InvalidPANNumber;
                    updatefacultytracking.Xeroxcopyofcertificates = facultydetails.Xeroxcopyofcertificates;
                    updatefacultytracking.NoSCM = facultydetails.NoSCM;
                    updatefacultytracking.AadhaarFlag = facultydetails.InvalidAadhaar == "Yes" ? true : false; ;
                    updatefacultytracking.BasFlag = facultydetails.BAS == "Yes" ? true : false; ;
                    updatefacultytracking.Absent = facultydetails.Absent;
                    updatefacultytracking.Blacklistfaculy = facultydetails.Blacklistfaculy;
                    updatefacultytracking.VerificationStatus = facultydetails.AbsentforVerification;


                    if (!String.IsNullOrEmpty(facultydetails.NoRelevantUG))
                        updatefacultytracking.NoRelevantUG = facultydetails.NoRelevantUG == "Yes"
                            ? true
                            : false;
                    if (!String.IsNullOrEmpty(facultydetails.NoRelevantPG))
                        updatefacultytracking.NoRelevantPG = facultydetails.NoRelevantPG == "Yes"
                            ? true
                            : false;
                    if (!String.IsNullOrEmpty(facultydetails.NORelevantPHD))
                        updatefacultytracking.NoRelevantPHD = facultydetails.NORelevantPHD == "Yes"
                            ? true
                            : false;
                    updatefacultytracking.updatedBy = userID;
                    updatefacultytracking.updatedOn = DateTime.Now;
                    db.Entry(updatefacultytracking).State = EntityState.Modified;
                    db.SaveChanges();
                }

                facultydetails.Absent = faculty.Absent;
                facultydetails.OriginalCertificatesNotShown = faculty.OriginalCertificatesnotshownFlag;
                facultydetails.Xeroxcopyofcertificates = faculty.XeroxcopyofcertificatesFlag;
                facultydetails.NoSCM = faculty.NoSCMFlag;
                facultydetails.NotQualifiedAsperAICTE = faculty.NOTQualifiedAsPerAICTE;
                facultydetails.IncompleteCertificates = faculty.InCompleteCeritificates;
                //This Flag for No Class in Profile faculty flag Name No Class in Ug/PG

                if (faculty.NOrelevantUgFlag == true)
                    facultydetails.NoRelevantUG = "Yes";
                else if (faculty.NOrelevantUgFlag == false)
                    facultydetails.NoRelevantUG = "No";

                if (faculty.NOrelevantPgFlag == true)
                    facultydetails.NoRelevantPG = "Yes";
                else if (faculty.NOrelevantPgFlag == false)
                    facultydetails.NoRelevantPG = "No";

                if (faculty.NOrelevantPhdFlag == true)
                    facultydetails.NORelevantPHD = "Yes";
                else if (faculty.NOrelevantPhdFlag == false)
                    facultydetails.NORelevantPHD = "No";

                facultydetails.InvalidPANNumber = faculty.InvalidPANNo;
                facultydetails.OriginalsVerifiedPHD = faculty.OriginalsVerifiedPHD;
                facultydetails.OriginalsVerifiedUG = faculty.OriginalsVerifiedUG;
                facultydetails.Invaliddegree = faculty.InvalidDegree;
                facultydetails.BAS = faculty.BasStatusFlag == true ? "Yes" : null;
                facultydetails.InvalidAadhaar = faculty.InvalidAadhaarFlag == true ? "Yes" : null;
                facultydetails.Noclass = faculty.NoClass;
                facultydetails.Genuinenessnotsubmitted = faculty.GenuinenessnotSubmitted;
                facultydetails.NotconsideredPHD = faculty.NotconsiderPhd;
                facultydetails.NoPGspecialization = faculty.NoPgSpecialization;
                facultydetails.AbsentforVerification = faculty.VerificationStatusFlag;
                facultydetails.NoForm26AS = faculty.NOForm26AS;
                facultydetails.Covid19 = faculty.Covid19;
                facultydetails.Maternity = faculty.Maternity;
                facultydetails.DeactivatedBy = userID;
                facultydetails.DeactivatedOn = DateTime.Now;

                db.Entry(facultydetails).State = EntityState.Modified;
                db.SaveChanges();
                TempData["Success"] = facultydetails.RegistrationNumber + " Faculty Flags are Changed";

            }

            return RedirectToAction("FacultyFlagsVerificationView", "FacultyVerificationDENew", new { collegeid = faculty.CollegeId });
            // return RedirectToAction("ViewFacultyDetails", "FacultyVerificationDENew", new { fid = UAAAS.Models.Utilities.EncryptString(faculty.id.ToString(), System.Web.Configuration.WebConfigurationManager.AppSettings["CryptoKey"]) });
        }


        #endregion

        //[Authorize(Roles = "Admin,SuperAdmin,FacultyVerification")]
        //public ActionResult FacultyPhdVerification()
        //{
        //    List<FacultyPhdDeskVerification> teachingFaculty = new List<FacultyPhdDeskVerification>();
        //    List<jntuh_registered_faculty> jntuh_registered_faculty = new List<jntuh_registered_faculty>();
        //    jntuh_registered_faculty = db.jntuh_registered_faculty.Where(rf => rf.Blacklistfaculy != true &&( rf.Others1!=null || rf.Others2!=null) && rf.isActive==true).ToList();
        //    teachingFaculty = jntuh_registered_faculty.Select(a => new FacultyPhdDeskVerification()
        //    {
        //       Id = a.id,
        //        Type = a.type,
        //        RegistrationNumber = a.RegistrationNumber,
        //       // UniqueID = a.UniqueID,
        //        FirstName = a.FirstName,
        //        MiddleName = a.MiddleName,
        //        LastName = a.LastName,

        //       GenderId = a.GenderId,
        //        Email = a.Email,
        //        facultyPhoto = a.Photo,
        //       Mobile = a.Mobile,
        //       PANNumber = a.PANNumber,
        //        AadhaarNumber = a.AadhaarNumber,
        //      //  isActive = a.isActive,
        //      //  isApproved = a.isApproved,
        //      //  DeactivatedBy = (int)a.DeactivatedBy,
        //      //  DeactivatedOn = a.DeactivatedOn,
        //      //PhdDeskDocument = a.Others1,
        //      //PhdDeskReason = a.Others2,
        //      DateOfBirth = a.DateOfBirth,
        //      //PhdDeskVerification = 
        //      //PhdDeskVerificationReason = =
        //    }).ToList();





        //    //foreach (var a in jntuh_registered_faculty)
        //    //{
        //    //    string Reason = String.Empty;
        //    //    var faculty = new FacultyRegistration();
        //    //    faculty.id = a.id;
        //    //    faculty.Type = a.type;
        //    //    faculty.RegistrationNumber = a.RegistrationNumber;
        //    //    faculty.UniqueID = a.UniqueID;
        //    //    faculty.FirstName = a.FirstName;
        //    //    faculty.MiddleName = a.MiddleName;
        //    //    faculty.LastName = a.LastName;
        //    //    faculty.Basstatus = a.BASStatus;
        //    //    faculty.GenderId = a.GenderId;
        //    //    faculty.Email = a.Email;
        //    //    faculty.facultyPhoto = a.Photo;
        //    //    faculty.Mobile = a.Mobile;
        //    //    faculty.PANNumber = a.PANNumber;
        //    //    faculty.AadhaarNumber = a.AadhaarNumber;
        //    //    faculty.isActive = a.isActive;
        //    //    faculty.isApproved = a.isApproved;

        //    //    teachingFaculty.Add(faculty);
        //    //}
        //    //teachingFaculty = teachingFaculty.Where(m => m.isActive == true).OrderBy(f => f.updatedOn).ThenBy(f => f.department).ToList();
        //    return View(teachingFaculty);
        //}


        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification")]
        public ActionResult FacultyPhdVerification()
        {
            List<FacultyPhdDeskVerification> teachingFaculty = new List<FacultyPhdDeskVerification>();
            List<jntuh_registered_faculty> jntuh_registered_faculty = new List<jntuh_registered_faculty>();
            jntuh_registered_faculty = db.jntuh_registered_faculty.Where(rf => rf.Blacklistfaculy != true && (rf.Others1 != null || rf.Others2 != null) && rf.isActive == true && rf.PhdDeskVerification != true && string.IsNullOrEmpty(rf.PhdDeskReason)).ToList();
            teachingFaculty = jntuh_registered_faculty.Select(a => new FacultyPhdDeskVerification()
            {
                Id = a.id,
                Type = a.type,
                RegistrationNumber = a.RegistrationNumber,
                // UniqueID = a.UniqueID,
                FirstName = a.FirstName,
                MiddleName = a.MiddleName,
                LastName = a.LastName,

                GenderId = a.GenderId,
                Email = a.Email,
                facultyPhoto = a.Photo == null ? string.Empty : a.Photo,
                Mobile = a.Mobile,
                PANNumber = a.PANNumber,
                AadhaarNumber = a.AadhaarNumber,
                DeptId = (int)a.DepartmentId,

                //  isActive = a.isActive,
                //  isApproved = a.isApproved,
                //  DeactivatedBy = (int)a.DeactivatedBy,
                //  DeactivatedOn = a.DeactivatedOn,
                //PhdDeskDocument = a.Others1,
                //PhdDeskReason = a.Others2,
                DateOfBirth = a.DateOfBirth,
                PhdDeskDocument = a.Others1 == null ? string.Empty : a.Others1,
                PhdDeskReason = a.Others2 == null ? string.Empty : a.Others2,
                PhdDeskVerification = (bool)a.PhdDeskVerification,
                PhdDeskVerificationReason = a.PhdDeskReason
            }).ToList();
            return View(teachingFaculty);
        }
        //PHD Approved
        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification")]
        public ActionResult PHDApprove(string fid)
        {
            try
            {
                int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
                int Fid = 0;
                if (fid != null)
                {
                    Fid = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(fid, WebConfigurationManager.AppSettings["CryptoKey"]));
                }
                if (Fid != null && Fid != 0)
                {
                    jntuh_registered_faculty registered_faculty = db.jntuh_registered_faculty.Where(f => f.id == Fid).FirstOrDefault();
                    registered_faculty.DeactivatedBy = userID;
                    registered_faculty.DeactivatedOn = DateTime.Now;
                    registered_faculty.PhdDeskVerification = true;
                    if (registered_faculty != null)
                    {
                        db.Entry(registered_faculty).State = EntityState.Modified;
                        db.SaveChanges();
                        TempData["SUCCESS"] = "PHD Approved For " + registered_faculty.FirstName + " " + registered_faculty.MiddleName + " " + registered_faculty.LastName;
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction("FacultyPhdVerification");
        }

        [HttpGet]
        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification")]
        public ActionResult PHDNOTApprove(string fid)
        {
            FacultyPhdDeskVerification PhdDeskVerification = new FacultyPhdDeskVerification();
            try
            {
                int Fid = 0;
                if (fid != null)
                {
                    Fid = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(fid, WebConfigurationManager.AppSettings["CryptoKey"]));
                }
                PhdDeskVerification.Id = Fid;
                ViewBag.Resons = ResonsList();
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return PartialView("PHDNOTApprove", PhdDeskVerification);
        }
        //Resons List
        private static List<SelectListItem> ResonsList()
        {
            List<SelectListItem> selectResons = new List<SelectListItem>();
            selectResons.Add(new SelectListItem { Value = "1", Text = "InComplete Certificates" });
            selectResons.Add(new SelectListItem { Value = "2", Text = "Photo Copyes of Certificates" });
            selectResons.Add(new SelectListItem { Value = "3", Text = "Thesis Copy Not Uploaded" });
            selectResons.Add(new SelectListItem { Value = "4", Text = "Pre Phd not Uploaded" });
            selectResons.Add(new SelectListItem { Value = "5", Text = "No PHD/PC/OD" });
            selectResons.Add(new SelectListItem { Value = "6", Text = "Others" });
            return selectResons;
        }

        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification")]
        public ActionResult PHDNOTApprove(FacultyPhdDeskVerification phdverification)
        {
            try
            {
                int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
                if (phdverification.Id != null && phdverification.Id != 0 && phdverification.PhdDeskVerificationReason != null)
                {
                    jntuh_registered_faculty registered_faculty = db.jntuh_registered_faculty.Where(f => f.id == phdverification.Id).FirstOrDefault();
                    List<SelectListItem> list = ResonsList();
                    if (phdverification.PhdDeskVerificationReason != "6")
                    {
                        registered_faculty.PhdDeskReason = list.Where(a => a.Value == phdverification.PhdDeskVerificationReason).Select(s => s.Text).FirstOrDefault();
                    }
                    else
                    {
                        registered_faculty.PhdDeskReason = phdverification.OtherReson;
                    }
                    registered_faculty.DeactivatedBy = userID;
                    registered_faculty.DeactivatedOn = DateTime.Now;
                    registered_faculty.PhdDeskVerification = false;
                    if (registered_faculty != null)
                    {
                        db.Entry(registered_faculty).State = EntityState.Modified;
                        db.SaveChanges();
                        TempData["SUCCESS"] = "PHD Not Approved For " + registered_faculty.FirstName + " " + registered_faculty.MiddleName + " " + registered_faculty.LastName;
                    }
                }
                else
                {
                    return RedirectToAction("PHDNOTApprove");
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction("FacultyPhdVerification");
        }


        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification,DataEntry")]
        public ActionResult CSEVerificationApprove(string fid, string collegeid)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            var fId = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(fid, WebConfigurationManager.AppSettings["CryptoKey"]));
            var facultydetails = db.jntuh_registered_faculty.Find(fId);
            if (facultydetails != null)
            {
                facultydetails.NotIdentityfiedForanyProgram = false;
                db.Entry(facultydetails).State = EntityState.Modified;
                db.SaveChanges();
                TempData["Success"] = facultydetails.RegistrationNumber + " Successfully Approved..";
            }
            return RedirectToAction("Facultyverificationfouretwozero", "FacultyVerificationDENew", new { collegeid = collegeid });
        }

        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification,DataEntry")]
        public ActionResult CSEVerificationNotApprove(string fid, string collegeid)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            var fId = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(fid, WebConfigurationManager.AppSettings["CryptoKey"]));
            var facultydetails = db.jntuh_registered_faculty.Find(fId);
            if (facultydetails != null)
            {
                facultydetails.NotIdentityfiedForanyProgram = true;
                db.Entry(facultydetails).State = EntityState.Modified;
                db.SaveChanges();
                TempData["Success"] = facultydetails.RegistrationNumber + " Successfully Not Approved..";
            }
            return RedirectToAction("Facultyverificationfouretwozero", "FacultyVerificationDENew", new { collegeid = collegeid });
        }

        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification,DataEntry")]
        public ActionResult FacultyCSEPHDverification(int? collegeid)
        {
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            //int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            //List<int> userRoles = db.my_aspnet_usersinroles.Where(u => u.userId == userID).Select(u => u.roleId).ToList();
            //if (userRoles.Contains(
            //                            db.my_aspnet_roles.Where(r => r.name.Equals("Admin"))
            //                                .Select(r => r.id)
            //                                .FirstOrDefault()))
            //{
            //    ViewBag.Colleges =
            //       db.jntuh_college.Join(db.jntuh_college_edit_status, co => co.id, e => e.collegeId,
            //           (co, e) => new { co = co, e = e })
            //           .Where(c => c.e.IsCollegeEditable == false)
            //           .Select(c => new { collegeId = c.co.id, collegeName = c.co.collegeCode + "-" + c.co.collegeName })
            //           .GroupBy(c => new { c.collegeId, c.collegeName }).Select(i => new { collegeId = i.Key.collegeId, collegeName = i.Key.collegeName })
            //           .OrderBy(c => c.collegeName)
            //           .ToList();
            //}
            //else
            //{
            //    int AcademicYear = db.jntuh_academic_year.Where(a => a.isPresentAcademicYear == true).Select(a => a.id).FirstOrDefault();
            //    int InspectionPhaseId = db.jntuh_inspection_phase.Where(p => p.isActive == true && p.inspectionPhase == "Data Entry").Select(p => p.id).SingleOrDefault();
            //    int[] assignedcollegeslist =
            //        db.jntuh_dataentry_allotment.Where(
            //            d =>
            //                d.InspectionPhaseId == InspectionPhaseId && d.userID == userID && d.isActive == true &&
            //                d.isCompleted == false).Select(s => s.collegeID).ToArray();
            //    ViewBag.Colleges =
            //        db.jntuh_college.Join(db.jntuh_college_edit_status, co => co.id, e => e.collegeId,
            //            (co, e) => new { co = co, e = e })
            //            .Where(c => c.e.IsCollegeEditable == false && c.e.academicyearId == AcademicYear + 1 && assignedcollegeslist.Contains(c.e.collegeId))
            //            .Select(c => new { collegeId = c.co.id, collegeName = c.co.collegeCode + "-" + c.co.collegeName })
            //            .GroupBy(c => new { c.collegeId, c.collegeName }).Select(i => new { collegeId = i.Key.collegeId, collegeName = i.Key.collegeName })
            //            .OrderBy(c => c.collegeName)
            //            .ToList();

            //}
            var colleges = db.jntuh_college.Join(db.jntuh_college_edit_status, co => co.id, e => e.collegeId, (co, e) => new { co = co, e = e }).Where(c => c.co.isActive == true && c.e.academicyearId == prAy && c.e.IsCollegeEditable == false).Select(c => new { collegeId = c.co.id, collegeName = c.co.collegeCode + "-" + c.co.collegeName }).OrderBy(c => c.collegeName).ToList();

            colleges.Add(new { collegeId = 375, collegeName = "TEST COLLEGE" });
            //colleges.Add(new { collegeId = 0, collegeName = "Not Clear PHD Faculty" });
            ViewBag.Colleges = colleges;
            //ViewBag.Colleges = db.jntuh_college.Join(db.jntuh_college_edit_status, co => co.id, e => e.collegeId, (co, e) => new { co = co, e = e }).Where(c => c.co.isActive == true).Select(c => new { collegeId = c.co.id, collegeName = c.co.collegeCode + "-" + c.co.collegeName }).OrderBy(c => c.collegeName).ToList();

            //int[] EditFacultycollegeIds = {4,5,7,8,9,12,20,22,23,24,26,27,29,30,32,35,38,39,40,41,42,43,44,47,48,55,56,58,68,69,70,72,74,75,77,79,80,81,84,85,86,87,88,91,97,100,103,105,106,107,108,109,110,111,116,117,118,119,121,123,127,128,129,130,132,134,135,137,138,139,141,143,144,145,147,148,150,152,153,155,156,157,158,161,162,163,164,165,166,168,171,172,173,174,175,176,177,178,179,180,181,183,184,185,187,189,192,193,195,196,198,202,203,206,210,211,213,214,215,217,218,222,223,225,227,228,234,235,236,241,242,244,245,246,249,250,254,256,260,261,262,264,266,267,271,273,276,282,287,290,291,292,295,296,298,299,300,302,304,307,309,310,313,315,316,318,319,321,322,324,327,329,334,335,336,349,350,352,353,355,360,364,365,366,367,368,369,370,373,374,376,380,382,384,385,389,391,392,393,399,400,411,414,420,421,423,424,428,435,436,439,441,455,375};
            //ViewBag.Colleges =
            //    db.jntuh_college.Where(c => EditFacultycollegeIds.Contains(c.id))
            //        .Select(a => new {collegeId = a.id, collegeName = a.collegeCode + "-" + a.collegeName})
            //        .OrderBy(r => r.collegeName)
            //        .ToList();
            ViewBag.collegeid = collegeid;
            var jntuh_department = db.jntuh_department.ToList();
            List<FacultyRegistration> teachingFaculty = new List<FacultyRegistration>();

            List<jntuh_appeal_faculty_registered> appealphdfaculty = new List<jntuh_appeal_faculty_registered>();

            if (collegeid != null)
            {
                var jntuh_appeal_faculty_registered =
                    db.jntuh_appeal_faculty_registered.Where(r => r.academicYearId == prAy && r.collegeId == collegeid).Select(s => s).ToList();

                var DegreeIdNameBasedOnSpecialization = (from a in db.jntuh_department
                                                         join b in db.jntuh_specialization on a.id equals b.departmentId
                                                         join c in db.jntuh_degree on a.degreeId equals c.id
                                                         select new
                                                         {
                                                             DegreeId = c.id,
                                                             DegreeName = c.degree,
                                                             SpcializationName = b.specializationName,
                                                             Specid = b.id
                                                         }).ToList();

                var phDCSEFacultyRegnoS = new string[] { "92150401-202528", "3326-210703-215239", "22150403-202837", "81150406-145946", "5569-150423-175518", "4046-150422-161638", "1260-170102-141915", "7317-220711-125149", "5629-150408-152930", "50150405-080016", "0627-171208-145014", "2054-200307-145918", "6524-150409-125614", "1476-210819-151210", "7674-220616-214557", "7194-220616-153456", "4189-180201-202900", "0078-230414-174500", "7675-190505-115629", "02150406-180132", "1520-180421-094650", "4886-180417-153108", "9116-220709-173126", "8397-170522-122330", "4275-200226-063531", "7523-150430-160517", "9446-150521-080533", "3434-160602-132427", "3446-150419-095020", "1696-150413-145115", "90150404-115955", "77150402-155219", "56150404-161904", "69150404-153211", "8993-150414-125558", "98150403-213601", "9085-221212-173924", "3421-190220-101706", "5533-160227-120854", "17150406-104943", "59150404-104244", "2919-170120-152602", "55150406-110725", "6454-160305-130424", "7578-150427-115924", "07150407-000328", "27150404-095936", "5592-150506-144923", "6957-150408-111738", "5006-200306-123244", "6853-220427-151816", "2637-150408-165918", "2767-170523-225515", "8000-220307-154232", "7506-150413-143553", "6719-180703-130904", "42150406-152311", "2709-170520-111218", "7376-150502-153240", "7558-150408-191150", "5504-220708-143008", "3349-150409-135337", "1994-220829-135619", "1223-160307-085216", "8577-220706-161735", "2793-150410-105925", "71150402-153329", "7111-170111-113028", "7081-170915-123807", "5409-210127-121303", "1994-150409-142348", "68150404-113848", "9146-160227-100628", "14150404-113552", "7624-160311-164633", "02150404-150446", "3432-170129-072851", "1423-150425-160847", "4342-170111-091417", "1371-150408-155648", "5678-161026-134337", "0441-210318-175421", "8999-210706-160423", "0837-161129-122004", "2334-201104-191233", "3825-160313-150058", "4101-150504-170027", "6689-230310-134356", "1519-171223-161429", "78150404-132559", "0467-180716-115522", "2122-180811-132049", "8184-150418-160647", "2982-170523-172149", "3158-220530-122953", "9150-180417-144644", "3611-201104-161318", "8628-160306-231747", "1972-150408-152900", "74150404-143917", "7905-150408-164415", "2778-190717-100933", "8631-170126-075712", "70150404-132355", "59150402-155551", "24150403-091523", "75150404-132847", "5924-170520-140225", "4040-150413-142705", "5468-210301-112021", "2836-190716-153037", "2954-150416-103048", "0058-220118-105740", "10150406-141754", "31150407-130909", "9807-150412-180800", "90150404-133929", "3947-150501-121041", "9355-210503-092839", "7885-160318-191918", "10150404-110423", "0884-160315-001044", "01150402-162427", "4523-230323-094357", "6786-210812-154233", "7669-150408-131725", "7762-160311-153730", "9804-150420-202301", "0415-210503-123242", "7507-161201-112124", "5104-180817-121912", "8351-150416-151821", "48150406-143321", "09150407-101903", "2001-220122-145528", "0229-150508-113304", "15150407-113619", "6741-161022-121423", "1862-150411-113042", "51150404-160046", "8724-160220-114604", "5411-150409-114451", "85150405-220535", "6341-211216-124244", "6393-190118-112312", "3366-210503-145402", "0807-150408-125629", "4383-200228-142340", "7690-170119-120804", "5233-150408-152752", "75150406-114801", "1196-150409-212536", "7804-150410-150606", "6377-150409-103655", "7128-150408-115856", "0034-180705-102815", "8236-171224-222547", "88150402-110935", "2674-220524-112545", "1382-170521-153441", "8017-150408-125254", "72150404-140928", "8563-210503-125619", "8036-150408-092450", "78150403-130225", "6286-171204-120854", "09150406-121910", "4274-150408-160332", "3113-190831-091134", "3558-171129-141433", "3908-180129-122453", "43150402-162415", "1089-150409-125700", "3999-150413-154939", "92150401-140923", "20150401-191342", "08150407-134210", "38150406-190205", "06150407-131623", "4425-160302-150829", "1740-171115-111337", "70150407-142347", "47150402-104501", "8016-191101-102442", "5065-161231-130124", "7691-160302-093216", "78150406-160752", "5456-171113-110854", "2892-160320-155417", "4612-151217-230709", "4147-150415-121607", "62150406-101224", "46150406-155903", "61150407-100731", "33150406-151737", "3420-191107-161934", "9213-170131-224323", "40150406-151446", "67150407-110820", "1793-150407-210245", "0540-150425-145853", "1348-200722-122624", "07150330-103619", "1709-220715-071615", "0083-150414-175701", "26150404-140949", "21150330-130127", "43150331-121811", "56150331-142915", "2514-190817-120841", "57150330-234818", "25150331-123716", "4402-180704-135820", "97150407-113543", "1624-200229-021327", "01150330-195657", "85150331-125201", "8908-210628-151857", "8502-181101-152559", "09150330-233548", "5507-161120-154946", "6490-150417-210934", "6253-171201-152124", "3180-210628-155615", "0750-220715-095508", "94150331-225018", "5754-220124-133006", "01150407-151833", "54150331-085207", "25150406-152025", "18150330-232502", "4994-150408-112901", "1100-160222-132952", "63150331-105631", "27150331-122933", "7990-180416-105441", "24150330-154256", "9371-160302-155838", "72150330-203208", "5550-210702-114314", "5958-150409-125234", "4591-210702-004559", "6503-170201-145903", "2600-160213-141859", "3359-210301-120929", "9091-150409-165116", "3659-210907-113751", "3677-151228-155128", "56150407-104220", "3407-150410-145259", "9914-150522-154121", "0804-170913-180210", "97150404-122618", "67150403-111913", "9031-230401-102420", "8198-221213-154207", "7588-220122-100232", "4893-180417-191036", "4135-150424-152211", "1207-220506-111527", "3898-150415-145415", "0750-150424-182028", "3480-150409-144311", "90150402-113500", "22150402-122908", "4538-170106-121618", "4284-230415-165934", "0655-220326-065311", "2141-160311-173346", "96150405-090312", "3725-220418-124625", "11150403-115927", "08150330-130321", "4066-170116-153454", "3516-230413-092630", "19150403-074232", "5426-150408-235052", "9010-150408-121616", "2374-190608-132310", "7777-150408-171502", "9860-190202-110439", "7915-220704-131830", "6869-220325-115118", "8388-160209-104546", "8249-220503-142045", "8127-150409-121259", "2589-160315-121543", "2605-160310-131957", "50150405-134030", "0037-150418-151850", "43150407-102654", "2054-160219-143542", "0640-150412-202441", "2957-190116-135456", "0001-211018-151302", "9907-150408-093545", "5331-220615-161458", "1195-170915-155908", "5363-150508-174515", "5108-211013-152200", "7113-160312-112254", "2857-190727-154209", "65150330-234021", "83150331-223450", "0889-181117-154851", "9755-210702-142525", "96150406-213444", "8423-151219-150129", "3132-150426-111616", "5414-150414-135918", "43150403-210708", "4830-181106-124446", "9375-180414-103858", "5836-190104-100307", "0013-150506-152758", "6260-170520-125955", "6726-150426-140606", "57150406-141835", "4427-180421-131451", "6092-190430-163915", "2611-170207-130847", "8878-150412-135054", "3234-150411-160116", "2517-150411-154733", "8111-150412-173810", "3503-190116-151859", "5603-150410-113109", "3149-170109-134623", "4282-150409-114804", "62150404-132410", "42150406-121024", "52150404-154809", "07150402-104325", "4950-201119-193912", "5000-150422-132711", "90150407-101404", "7132-230112-162402", "7310-190506-151820", "8968-161217-114914", "1332-170201-103806", "5486-200307-101919", "44150407-122156", "1468-150422-132056", "5118-170106-120828", "4215-161223-144848", "3011-150506-143432", "0791-150410-131235", "5274-210707-123921", "1791-150417-215632", "8874-161218-133022", "04150407-044411", "23150405-193508", "86150326-210611", "1465-160305-162202", "73150403-160105", "30150331-124825", "5568-190503-151217", "6375-160229-130729", "50150406-133509", "7314-220704-170801", "8396-220125-135852", "2591-170918-124640", "2257-150428-055331", "4257-230413-142052", "60150405-003701", "13150402-131801", "41150402-112015", "8261-210915-225715", "92150405-183642", "31150402-125941", "5475-160305-123021", "1151-190827-130616", "86150402-121020", "1336-170911-162939", "60150405-201023", "97150404-203443", "02150403-220700", "0840-150409-125439", "0819-180208-214939", "1752-150410-144814", "5270-170208-012320", "5256-171220-130151", "4046-150408-180421", "7202-190501-124420", "5198-150410-113217", "0887-200307-112312", "6557-180202-145448", "8985-150415-160711", "92150407-143242", "6546-221130-142909", "72150407-154904", "0424-220705-123623", "3127-170126-034042", "2477-150413-174417", "0398-170521-100904", "0261-190504-111404", "67150407-152411", "0957-160106-112053", "0986-180204-074254", "95150403-181521", "7727-150408-151444", "41150406-151110", "8617-150507-162025", "8324-170117-123623", "59150406-114819", "3611-171205-150253", "7341-210701-192551", "7565-160105-105513", "2694-150622-111010", "6746-150413-123836", "88150407-152004", "5252-180127-160522", "41150406-010905", "4479-170117-130507", "53150406-150814", "9552-171124-165313", "0136-230413-153657", "8234-170105-124758", "74150406-164908", "4221-190827-181122", "69150402-091811", "5002-211025-115335", "4682-220523-153543", "6213-161022-184405", "1694-160309-134309", "1395-171129-121318", "0013-221214-180707", "9162-221214-134254", "2784-161027-112616", "5108-221215-174639", "3404-190807-141310", "7685-220915-131909", "78150402-021601", "9475-221215-142819", "6899-230225-125708", "9086-150408-141048", "7869-150417-160702", "1016-200229-221341", "0867-210312-221421", "6920-150420-160903", "5747-220409-145022", "5012-220315-140130", "53150407-120415", "3214-220506-161520", "88150407-145551", "1239-160307-230546", "2921-171009-152053", "4258-160118-170905", "0438-171122-122044", "92150406-102807", "91150406-134727", "3162-180415-193439", "4587-150408-154522", "88150331-142803", "0855-150409-144220", "0013-191111-163137", "2164-220401-141419", "22150406-111129", "4230-210707-101058", "7443-170523-213227", "3855-210702-163625", "0499-210329-164556", "58150407-103605", "1039-160313-141051", "0725-150408-161536", "0332-150409-115134", "5526-150422-175939", "6945-170201-074733", "1578-210309-103243", "79150403-161308", "5812-211215-144625", "2041-160215-133636", "73150404-182934", "9139-160113-104601", "6641-150419-140603", "1636-170117-152946", "19150406-144202", "5163-160224-123838", "4877-170116-111735", "1060-190505-122821", "3510-170111-130947", "4019-190220-143933", "7264-160225-131221", "4847-161024-210125", "0856-160225-120043", "8936-210818-104747", "60150402-141644", "2326-180728-155220", "9704-161210-115658", "2639-150415-144606", "3121-220226-110655", "6180-211105-122713", "3454-210301-164918", "0255-150418-113623", "62150404-101335", "1975-220708-111331", "7095-180705-111442", "0483-220920-140048", "4149-201109-120743", "6457-201109-153520", "1774-191109-140114", "4369-150411-134640", "5761-230415-160424", "0899-150419-090852", "66150406-154147", "32150401-182932", "5112-150408-194704", "3732-220702-123330", "2299-230412-185013", "4162-230417-163920", "0875-170125-122325", "7131-230421-145340", "5570-180827-134124", "9993-220707-120459", "3563-220701-101042", "2829-150413-153459", "0301-220704-132825", "4017-150408-133755", "3315-150413-155000", "93150407-124825", "5375-230415-142118", "0948-160223-154330", "4127-200227-150406", "2994-150505-150403", "8456-150408-111033", "2103-190506-163410", "8603-220709-145938", "9139-200311-155327", "9665-161207-131759", "7149-150426-114210", "87150406-174454", "11150406-114427", "57150403-082409", "68150331-154919", "0264-150507-160118", "0096-211218-104109", "9454-150411-155023", "0816-150409-144756", "3827-160311-182024", "6775-170916-094129", "1800-191108-144157", "2711-150413-125111", "4464-170126-024023", "41150401-140203", "6945-230418-114803", "28150406-134111", "19150331-095841", "62150403-100019", "41150407-182401", "1650-150413-133959", "1583-150413-163325", "16150401-123810", "8488-170521-214919", "2488-170914-144209", "1097-220711-123712", "5950-150409-134403", "6534-191107-152025", "8206-220705-121158", "0904-190221-161321", "4348-150414-123031", "4182-220708-105359", "4382-150427-200333", "18150404-115829", "5218-150420-155112", "7948-150623-175333", "09150403-140857", "4806-150420-111136", "06150407-165250", "1127-180417-133316", "4648-150408-212254", "05150407-141345", "2726-150408-010119", "6447-161229-061433", "23150407-162202", "8893-190502-165322", "8594-170914-131547", "2244-180908-113833", "3554-180202-123255", "90150406-150306", "85150403-230538", "4680-161104-140308", "65150406-145928", "02150406-222518", "6791-230404-143412", "49150406-112603", "2556-210224-103108", "7788-161025-163652", "8771-160209-123747", "9332-191105-133824", "2178-150421-061659", "1718-150411-170214", "3752-210701-122037", "7953-210702-102210", "4858-150418-102518", "8276-221214-142639", "9015-150408-150952", "8455-210702-103129", "5366-220223-120043", "1098-160223-134532", "2901-191104-152721", "13150330-110833", "4450-230313-160939", "2023-230414-100601", "2932-230419-175251", "7635-220223-134818", "90150407-001154", "8275-150410-104004", "1235-170131-182202", "8097-170112-011609", "30150406-113957", "0545-180416-150913", "94150407-124045", "4649-150414-120316", "79150401-145846", "0695-160209-105750", "1358-170201-193230", "45150402-143940", "9377-160220-135938", "92150404-153015", "3942-190504-193853", "5833-150419-140948", "2134-220118-113354", "5954-150413-154957", "1374-150408-102944", "0122-190309-140459", "0486-210327-115103", "0595-160205-123208", "2499-200108-163842", "0971-201109-124331", "7013-210327-125552", "1035-191104-111048", "2988-160304-161607", "4101-150411-105236", "3929-150409-111821", "4495-210812-133218", "8928-160203-162708", "6092-150413-164143", "4897-150428-162757", "85150404-120222", "9828-150420-172726", "1394-170915-133940", "7355-150413-180225", "9840-150412-102941", "5508-210707-102625", "8168-150426-122831", "44150402-125710", "4010-210707-141319", "2767-221018-155935", "3564-150407-235348", "9428-210922-093221", "5493-190723-122200", "1409-200214-121449", "8166-180201-155956", "0894-220608-095337", "88150407-123130", "2528-150408-161603", "2368-170104-151452", "82150404-130543", "0766-191108-150321", "4792-210706-110724", "7392-211219-091245", "4024-190119-131500", "3337-150413-142359", "1622-150418-161710", "92150401-162534", "7553-170204-164547", "2890-150409-150716", "8160-150410-100558", "2116-190505-183023", "4383-170127-111456", "61150330-162109", "2879-220708-154236", "50150401-194631", "68150402-124351", "4974-210701-155827", "7395-150412-152735", "3984-200225-095516", "6276-220106-153951", "1483-190502-153355", "8633-210704-233607", "0138-150408-131246", "84150407-164929", "67150405-130233", "7332-171214-192349", "9251-150414-062519", "8612-210707-140157", "9196-150409-232438", "7000-150409-233654", "9894-160113-202323", "3746-150408-155908", "2876-150413-192246", "0708-150418-220656", "9268-210709-153035", "2386-150620-152649", "6097-170127-072854", "39150406-124038", "1659-160226-144252", "0354-190828-113106", "5378-150413-144042", "22150407-141424", "2166-191118-145311", "2331-150419-160011", "7197-150420-160310", "8049-200311-121139", "7339-220315-143958", "01150331-201207", "10150406-151442", "2887-220309-114521", "2254-211210-145210", "2004-230201-153232", "12150330-214646", "1712-191109-172832", "2886-161117-110449", "9160-170208-154839", "1353-210314-114813", "6373-190829-142431", "07150331-132917", "8049-211203-112031", "94150403-133940", "6264-191110-102309", "0080-220319-210104", "7978-150416-144352", "54150331-143516", "9062-211123-123905", "2461-211126-160501", "1353-190829-100358", "6641-211216-115405", "0406-150418-131001", "6791-201106-121208", "7021-160307-014720", "92150331-142157", "6097-210701-155159", "6934-190117-161100", "4205-200307-143722", "9959-180713-002900", "2694-161223-112956", "3138-230413-150405", "0065-161128-150056", "1282-150418-182637", "8741-150413-125346", "0474-150413-144451", "2500-160307-125857", "4768-160219-134349", "76150405-161531", "2989-220707-160023", "3593-220508-105651", "81150402-090727", "9283-170102-130548", "61150407-120333", "3634-150428-100300", "81150402-142527", "54150407-113636", "27150331-114003", "3267-150408-114729", "4655-150415-193029", "2191-220430-142239", "2387-150413-103809", "0141-150408-170937", "49150404-142008", "79150402-120446", "3114-200110-125654", "5716-161220-161902", "08150403-000542", "4929-150409-144539", "2470-150414-114544", "1287-190828-111638", "0125-150413-170311", "66150406-155949", "6122-150504-165200", "7700-150420-161952", "4085-210702-163054", "7557-191105-151233", "4088-150414-103734", "1919-150504-115005", "5657-150408-131643", "8601-150407-215242", "0664-161118-151901", "4481-150413-002634", "16150406-161926", "34150405-102222", "9175-161210-122028", "18150406-133332", "8946-150428-143903", "1958-150417-101629", "8536-151222-152431", "0124-150408-165221", "2541-150419-130601", "8412-170213-000212", "14150402-120027", "53150407-173532", "8015-150409-113940", "28150405-194151", "4482-161125-142436", "2625-220308-111834", "5957-150413-135745", "1212-201103-154657", "16150407-122941", "55150404-164801", "6597-190128-170916", "8664-160112-104621", "97150402-154036", "70150405-151757", "00150404-163546", "9556-150408-110528", "78150407-162814", "14150402-115407", "75150406-222443", "6575-160227-133435", "8924-150407-231234", "16150404-113719", "47150404-190607", "6678-161119-152720", "0733-150409-100036", "27150402-231225", "5540-150409-151342", "", "1067-150412-120929", "31150406-132701", "92150331-160512", "8741-180202-162322", "8232-160309-131947", "8348-170202-174618", "6797-150424-233740", "1466-150419-190407", "9349-160310-130516", "80150331-104357", "01150331-125001", "04150404-162859", "7490-170110-120834", "7392-170911-165820", "1778-161022-132131", "5651-180713-124217", "6419-150416-160033", "2115-150415-093549", "3540-180208-183821", "9768-171227-113501", "76150404-141308", "3139-161024-163729", "1551-160302-115953", "5256-150420-121814", "40150402-170443", "2644-160219-154922", "17150401-154407", "2705-150506-170154", "32150404-172740", "7534-150410-120811", "8148-170523-140155", "6641-170208-130829", "5576-150410-101336", "94150330-113934", "3404-170119-172558", "35150402-133204", "4728-180202-082643", "02150404-110529", "2773-170109-102839", "3591-150410-125429", "35150406-172430", "1426-150411-125711", "1745-150419-203504", "22150407-112351", "1828-150420-120347", "2336-160123-161124", "8779-161025-113529", "60150406-102734", "7393-170210-175855", "0009-150410-223407", "0716-150414-114048", "46150403-175919", "9550-150413-074527", "7993-160123-141250", "4684-150420-135135", "5916-180127-115342", "6950-180201-112952", "5826-150413-164352", "3022-150408-105758", "9191-150408-103715", "5050-150409-132929", "37150404-142550", "76150330-131131", "2382-170131-200402", "8349-151216-215143", "2488-160314-230836", "5454-161126-134359", "3040-170915-115017", "1881-160307-111846", "3826-170107-145702", "9157-160203-163715", "3141-160312-215029", "6283-150413-140008", "85150404-150450", "3133-150410-105936", "34150330-174403" };

                //418 Faculty Verification Condition 
                //string[] Copy_RegNos = db.jntuh_college_faculty_registered_copy.Where(c => c.collegeId == 375).Select(e => e.RegistrationNumber).Distinct().ToArray();

                // List<jntuh_college_faculty_registered> jntuh_college_faculty_registered_College_Portal = db.jntuh_college_faculty_registered.Where(cf => cf.collegeId == collegeid).Select(cf => cf).ToList();
                //committed by siva due to only edit option faculty is comming case....
                // jntuh_college_faculty_registered = jntuh_college_faculty_registered.Where(c => !Copy_RegNos.Contains(c.RegistrationNumber)).ToList();
                List<jntuh_college_faculty_registered> jntuh_college_faculty_registered_forphd = new List<jntuh_college_faculty_registered>();

                List<jntuh_college_faculty_registered> jntuh_college_faculty_registered = db.jntuh_college_faculty_registered.Where(cf => phDCSEFacultyRegnoS.Contains(cf.RegistrationNumber) && cf.collegeId == collegeid).Select(cf => cf).ToList();

                //419 Faculty Verifi
                string[] collegeRegnos =
                    jntuh_college_faculty_registered.Select(s => s.RegistrationNumber).Distinct().ToArray();
                //string[] Copy_RegNos = db.jntuh_college_faculty_registered_copy.Where(c => c.collegeId == 1420 && collegeRegnos.Contains(c.RegistrationNumber)).Select(e => e.RegistrationNumber).Distinct().ToArray();
                //string[] Copy_RegNos = db.jntuh_college_faculty_registered_copy.Where(c => c.collegeId == 1419 && collegeRegnos.Contains(c.RegistrationNumber)).Select(e => e.RegistrationNumber).Distinct().ToArray();
                //string[] strRegNoS = jntuh_college_faculty_registered.Where(c => !Copy_RegNos.Contains(c.RegistrationNumber)).Select(cf => cf.RegistrationNumber).ToArray();
                string[] strRegNoS = jntuh_college_faculty_registered.Select(cf => cf.RegistrationNumber).ToArray();
                string[] PrincipalstrRegNoS = db.jntuh_college_principal_registered.Where(P => P.collegeId == collegeid).Select(P => P.RegistrationNumber.Trim()).ToArray();
                List<jntuh_registered_faculty> jntuh_registered_faculty = new List<jntuh_registered_faculty>();
                ViewBag.TotalClearedFaculty = collegeRegnos.Count() - strRegNoS.Count();
                ViewBag.TotalCollegeFaculty = collegeRegnos.Count();


                if (collegeid == 375)
                {
                    string[] phdnotclearcollegeassociate = new string[]
                    {
                       "85150330-182657","06150331-111246","47150331-113435","94150331-122435","08150331-164950","75150331-181534","32150401-121708","85150401-152830","17150401-154407","82150401-193318","81150402-122531","84150402-135106","97150402-154036","76150402-165017","07150402-165852","74150403-101252","79150403-133318","73150403-160105","48150403-190656","91150403-203812","41150404-113040","12150404-154539","16150404-155121","04150404-164306","32150404-172740","29150404-175402","28150404-175504","31150404-195745","21150405-011701","51150405-112628","89150405-153610","01150405-154759","58150406-100658","92150406-102807","31150406-132701","83150406-133331","80150406-145457","66150406-154147","67150406-154925","47150406-163118","35150406-172430","12150407-102002","88150407-105745","06150407-110409","21150407-114517","39150407-121907","26150407-125656","77150407-135637","38150407-141045","39150407-142712","24150407-144012","13150407-160822","6945-150408-105112","7525-150408-124513","0254-150408-125530","5043-150408-125708","1439-150408-145337","5233-150408-152752","4587-150408-154522","0973-150408-164314","7860-150409-093946","7258-150409-102513","9420-150409-104558","3556-150409-105506","8677-150409-122457","2802-150409-125545","5685-150409-131541","0377-150409-133622","5763-150409-155228","2468-150410-103024","3591-150410-125429","0791-150410-131235","9946-150410-132838","7490-150411-125727","1955-150411-132435","9288-150413-125630","0861-150413-142903","9165-150413-144341","5958-150413-154246","1291-150413-162334","7331-150414-124517","4854-150414-125905","0104-150414-142722","8720-150414-153720","1191-150414-182314","8713-150415-223120","1898-150416-103606","5417-150416-173107","6858-150417-153743","8184-150418-160647","1136-150419-010704","8382-150419-134710","9377-150419-145913","5256-150420-121814","5614-150420-155913","6931-150421-013104","7479-150421-105533","7186-150422-130450","3119-150426-160711","3969-150427-114410","8599-150428-123056","9127-150429-155346","9828-150501-044104","1804-150504-223903","2916-150505-111623","2705-150506-170154","2351-150507-075609","9233-150507-194931","5307-150605-143306","9225-150619-210159","0183-150623-110537","9189-160112-211310","6672-160128-161546","8795-160203-175048","7783-160204-153400","1082-160210-102657","6661-160216-162733","1688-160217-160734","4744-160220-155006","5020-160223-110315","0948-160223-154330","2582-160225-154756","4320-160228-133043","4531-160229-113726","4984-160303-115900","0124-160303-185802","6631-160305-114438","2185-160306-125348","2718-160307-011834","7715-160307-094837","1881-160307-111846","7008-160307-141820","2232-160312-110412","4712-160313-143055","1581-160313-152759","3592-160313-222024","0879-160314-164629","2841-160315-000843","8250-160315-111108","3370-160315-134028","9805-160316-211552","4497-160318-164821","4825-160529-193005","7691-160804-172710","1778-161022-132131","2971-161024-163038","2770-161028-140111","6391-161101-112549","4213-161128-093740","7595-161207-140122","0089-161209-145730","7984-170103-155512","4374-170104-125349","3745-170105-143145","5118-170106-120828","4779-170106-123241","7757-170109-141614","0767-170111-102126","7111-170111-113028","0846-170111-120917","9510-170112-102057","5203-170122-115917","3830-170127-055112","8402-170130-042424","4731-170130-143850","7999-170131-184403","7050-170131-192946","1286-170131-201713","2639-170201-061450","1423-170201-231848","2951-170202-052356","8348-170202-174618","4300-170208-163059","1255-170208-172403","8673-170209-001735","2704-170520-130953","0697-170521-115623","4499-170522-103724","1539-170522-113513","5670-170522-125002","9348-170523-173650","5914-170523-202326","3040-170915-115017","3792-170915-171037","9760-170916-124258","4865-171016-113947","7148-171125-120143","9255-171220-162451","6404-171221-160659","0895-171222-113335","5089-171223-141800","7837-171225-073920","7329-171227-111349","9084-180131-155141","1603-180131-191237","4463-180201-140846","2117-180201-163235","4189-180201-202900","5990-180202-173614","9882-180203-163139","9122-180204-153030","7617-180207-150550","1747-180208-144342","0755-180208-162637","0961-180208-210002","2182-180208-233008","9447-180212-120756","1076-180220-234151","6846-180417-113033","3130-180417-121250","7594-180421-150604","9537-180421-154149","3135-180421-155425","5776-180421-155710","5702-180421-160738","9715-180703-001326","6481-180703-075013","9418-180711-145210","9959-180713-002900","1744-180713-010015","0602-180713-161550","0467-180716-115522","8650-180719-124256","3929-180721-133143","6877-180723-180939","2326-180728-155220","1451-180808-161540","4474-180809-203945","2439-180810-111213","2122-180811-132049","3747-180811-214805","5995-180811-222347","5981-180812-093328","7572-180818-112007","9455-180818-115900","5106-180818-124347","6560-180818-133117","3010-180818-140606","6570-180820-121747","2319-180820-142857","2417-180820-144805","2373-180820-194644","9887-181105-140926","4830-181106-124446","5071-181122-173530","4703-181223-121930","3568-190110-142247","7610-190119-095241","5633-190119-142303","8766-190119-161519","6370-190131-113843","6881-190201-230301","7990-190202-160519","5221-190212-125443","2442-190214-133013","6397-190216-121426","5076-190218-161729","2784-190219-171631","7679-190220-121213","3537-190220-144812","9560-190220-170729","8563-190221-154413","1040-190221-160040","9744-190221-161632","8881-190309-143540","0337-190309-154816"
                    };

                    jntuh_registered_faculty = db.jntuh_registered_faculty.Where(rf => phdnotclearcollegeassociate.Contains(rf.RegistrationNumber) && rf.Blacklistfaculy != true)
                                           .ToList();
                    jntuh_college_faculty_registered_forphd = db.jntuh_college_faculty_registered.Where(cf => phdnotclearcollegeassociate.Contains(cf.RegistrationNumber)).Select(cf => cf).ToList();
                    appealphdfaculty =
                        db.jntuh_appeal_faculty_registered.Where(
                            r => phdnotclearcollegeassociate.Contains(r.RegistrationNumber) && r.academicYearId == prAy)
                            .Select(a => a)
                            .ToList();
                    //jntuh_registered_faculty = db.jntuh_registered_faculty.Where(rf => phdnotcollegeassociate.Contains(rf.RegistrationNumber) && rf.Blacklistfaculy != true && rf.updatedOn >= editoptionStartdate)
                    //                       .ToList();
                }
                else
                {
                    jntuh_registered_faculty = db.jntuh_registered_faculty.Where(rf => strRegNoS.Contains(rf.RegistrationNumber) && rf.Blacklistfaculy != true)  //&& (rf.collegeId == null || rf.collegeId == collegeid)//&& rf.Notin116 != true
                                            .ToList();
                }



                //var jntuh_faculty = db.jntuh_registered_faculty.Where(rf => rf.Blacklistfaculy != true && rf.updatedOn >= editoptionStartdate)  //&& (rf.collegeId == null || rf.collegeId == collegeid)//&& rf.Notin116 != true
                //                            .ToList();

                var Specializations = db.jntuh_specialization.ToList();

                string RegNumber = "";
                int? Specializationid = 0;
                int? CollegeDepartmentId = 0;
                int inactivephdfaculty = 0;
                foreach (var a in jntuh_registered_faculty)
                {
                    string Reason = null;
                    Specializationid =
                        jntuh_college_faculty_registered.Where(
                            C => C.RegistrationNumber.Trim() == a.RegistrationNumber.Trim())
                            .Select(C => C.SpecializationId)
                            .FirstOrDefault();
                    CollegeDepartmentId =
                        jntuh_college_faculty_registered.Where(
                            C => C.RegistrationNumber.Trim() == a.RegistrationNumber.Trim())
                            .Select(C => C.DepartmentId)
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
                    //faculty.Basstatus = a.BASStatus;
                    if (PrincipalstrRegNoS.Contains(a.RegistrationNumber.Trim()))
                        faculty.Principal = "Principal";
                    else
                        faculty.Principal = "";

                    //Hear showing JNTU Specilazation only Pharmacy Faculty
                    faculty.PGSpecializationName = a.Jntu_PGSpecializationId != null
                        ? DegreeIdNameBasedOnSpecialization.Where(e => e.Specid == a.Jntu_PGSpecializationId)
                            .Select(e => e.DegreeName + "-" + e.SpcializationName)
                            .FirstOrDefault()
                        : "";
                    faculty.GenderId = a.GenderId;
                    faculty.Email = a.Email;
                    faculty.facultyPhoto = a.Photo;
                    faculty.Mobile = a.Mobile;
                    faculty.PANNumber = a.PANNumber;
                    faculty.AadhaarNumber = a.AadhaarNumber;
                    faculty.isActive = a.isActive;
                    faculty.isApproved = a.isApproved;
                    faculty.department = CollegeDepartmentId > 0
                        ? jntuh_department.Where(d => d.id == CollegeDepartmentId)
                            .Select(d => d.departmentName)
                            .FirstOrDefault()
                        : "";
                    faculty.SamePANNumberCount =
                        jntuh_registered_faculty.Where(f => f.PANNumber == a.PANNumber).ToList().Count;
                    faculty.SameAadhaarNumberCount =
                        jntuh_registered_faculty.Where(f => f.AadhaarNumber == a.AadhaarNumber).ToList().Count;
                    faculty.SpecializationIdentfiedFor = Specializationid > 0
                        ? DegreeIdNameBasedOnSpecialization.Where(S => S.Specid == Specializationid)
                            .Select(S => S.DegreeName + "-" + S.SpcializationName)
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
                    //faculty.IdentfiedFor =
                    //    jntuh_college_faculty_registered.Where(
                    //        f => f.RegistrationNumber.Trim() == a.RegistrationNumber.Trim())
                    //        .Select(f => f.IdentifiedFor)
                    //        .FirstOrDefault();
                    //faculty.jntuh_registered_faculty_education = a.jntuh_registered_faculty_education;
                    //faculty.DegreeId = a.jntuh_registered_faculty_education.Count(e => e.facultyId == a.id && e.educationId != 8) > 0
                    //    ? a.jntuh_registered_faculty_education.Where(e => e.facultyId == a.id && e.educationId != 8)
                    //        .Select(e => e.educationId)
                    //        .Max()
                    //    : 0;
                    //faculty.PanDeactivationReasion = !string.IsNullOrEmpty(a.PanDeactivationReason)
                    //    ? a.PanDeactivationReason
                    //    : "";
                    //faculty.Absent = a.Absent != null ? (bool)a.Absent : false;
                    //faculty.BlacklistFaculty = a.Blacklistfaculy != null ? (bool)a.Blacklistfaculy : false;
                    //faculty.PHDundertakingnotsubmitted = a.PHDundertakingnotsubmitted != null
                    //    ? (bool)a.PHDundertakingnotsubmitted
                    //    : false;
                    //faculty.NOTQualifiedAsPerAICTE = a.NotQualifiedAsperAICTE != null
                    //    ? (bool)a.NotQualifiedAsperAICTE
                    //    : false;
                    //faculty.InvalidPANNo = a.InvalidPANNumber != null ? (bool)a.InvalidPANNumber : false;
                    //faculty.InCompleteCeritificates = a.IncompleteCertificates != null
                    //    ? (bool)a.IncompleteCertificates
                    //    : false;
                    //faculty.OriginalCertificatesnotshownFlag = a.OriginalCertificatesNotShown != null
                    //    ? (bool)a.OriginalCertificatesNotShown
                    //    : false;
                    //faculty.GenuinenessnotSubmitted = a.Genuinenessnotsubmitted != null ? (bool)a.Genuinenessnotsubmitted : false;
                    //faculty.NoClass = a.Noclass != null ? (bool)a.Noclass : false;
                    //faculty.InvalidDegree = a.Invaliddegree != null ? (bool)a.Invaliddegree : false;
                    //faculty.NotconsiderPhd = a.NotconsideredPHD != null ? (bool)a.NotconsideredPHD : false;
                    //faculty.NoPgSpecialization = a.NoPGspecialization != null ? (bool)a.NoPGspecialization : false;
                    //faculty.Noclassinugorpg = a.Noclass != null
                    //    ? (bool)a.Noclass
                    //    : false;
                    //faculty.NoSCM = a.NoSCM != null ? (bool)a.NoSCM : false;

                    //faculty.XeroxcopyofcertificatesFlag = a.Xeroxcopyofcertificates != null
                    //    ? (bool)a.Xeroxcopyofcertificates
                    //    : false;
                    //faculty.NotIdentityFiedForAnyProgramFlag = a.NotIdentityfiedForanyProgram != null
                    //    ? (bool)a.NotIdentityfiedForanyProgram
                    //    : false;
                    //faculty.InvalidAadhaar = a.InvalidAadhaar;
                    //faculty.VerificationStatus = a.AbsentforVerification;
                    //faculty.NOrelevantUgFlag = a.NoRelevantUG == "No" ? false : true;
                    //faculty.NOrelevantUgFlag = a.NoRelevantUG == "No" ? false : true;
                    //faculty.NOrelevantPgFlag = a.NoRelevantPG == "No" ? false : true;
                    //faculty.NOrelevantPhdFlag = a.NORelevantPHD == "No" ? false : true;
                    faculty.PhdDeskVerification = a.PhdDeskVerification != null ? (bool)a.PhdDeskVerification : false;
                    //faculty.PhdUndertakingDocumentstatus = a.PhdUndertakingDocumentstatus != null
                    //    ? (bool)(a.PhdUndertakingDocumentstatus)
                    //    : false;
                    //faculty.GenuinenessnotSubmitted = a.Genuinenessnotsubmitted != null ? (bool)a.Genuinenessnotsubmitted : false;
                    //faculty.OriginalsVerifiedUG = a.OriginalsVerifiedUG != null ? (bool)a.OriginalsVerifiedUG : false;
                    //faculty.OriginalsVerifiedPHD = a.OriginalsVerifiedPHD != null ? (bool)a.OriginalsVerifiedPHD : false;
                    //faculty.PHDUndertakingDocumentView = a.PHDUndertakingDocument;
                    //faculty.PhdUndertakingDocumentText = a.PhdUndertakingDocumentText;
                    //faculty.Deactivedby = a.DeactivatedBy;
                    //faculty.DeactivedOn = a.DeactivatedOn;
                    //faculty.appealsupportingdocument =
                    //    jntuh_appeal_faculty_registered.Where(r => r.RegistrationNumber == a.RegistrationNumber)
                    //        .Select(s => s.AppealReverificationSupportingDocument)
                    //        .FirstOrDefault();
                    //if (String.IsNullOrEmpty(faculty.appealsupportingdocument))
                    //{
                    //    faculty.appealsupportingdocument =
                    //   jntuh_appeal_faculty_registered.Where(r => r.RegistrationNumber == a.RegistrationNumber)
                    //       .Select(s => s.AppealReverificationScreenshot)
                    //       .FirstOrDefault();
                    //}

                    //if (faculty.Absent == true)
                    //    Reason += "Absent";
                    //if (faculty.NotIdentityFiedForAnyProgramFlag == true)
                    //{
                    //    if (Reason != null)
                    //        Reason += ",Adjunct Faculty";
                    //    else
                    //        Reason += "Adjunct Faculty";
                    //}

                    //faculty.IsStatus = a.InStatus;
                    //faculty.DeactivationReason = Reason;
                    teachingFaculty.Add(faculty);
                }
                teachingFaculty = teachingFaculty.Where(m => m.isActive == true).OrderBy(f => f.updatedOn).ThenBy(f => f.department).ToList();
                //|| item.NoForm16Verification == true ||item.NOForm16 == true ||
                ViewBag.TotalFaculty = teachingFaculty.Count();
                ViewBag.Inactivephdfaculty = inactivephdfaculty;

                string[] data = teachingFaculty.Where(item =>
                            item.NotconsiderPhd == true ||
                            item.NOTQualifiedAsPerAICTE == true ||
                            item.InCompleteCeritificates == true || item.NoPgSpecialization == true ||
                             item.NOrelevantUgFlag == true ||
                            item.NOrelevantPgFlag == true || item.NOrelevantPhdFlag == true ||
                            item.NotIdentityFiedForAnyProgramFlag == true ||
                            item.NoSCM == true || item.InvalidPANNo == true ||
                            item.DegreeId < 4 || item.PANNumber == null ||
                            item.BlacklistFaculty == true || item.Type == "Adjunct" ||
                            item.AppliedPAN == true || item.NoClass == true || item.XeroxcopyofcertificatesFlag == true
                           || (item.PhdUndertakingDocumentstatus == false) || item.InvalidAadhaar == "Yes"
                            ).Select(e => e.RegistrationNumber).ToArray();
                ViewBag.FlagTotalFaculty = teachingFaculty.Where(item =>
                            item.NotconsiderPhd == true ||
                            item.NOTQualifiedAsPerAICTE == true ||
                            item.InCompleteCeritificates == true || item.NoPgSpecialization == true ||
                             item.NOrelevantUgFlag == true ||
                            item.NOrelevantPgFlag == true || item.NOrelevantPhdFlag == true ||
                            item.NotIdentityFiedForAnyProgramFlag == true ||
                            item.NoSCM == true || item.InvalidPANNo == true ||
                            item.DegreeId < 4 || item.PANNumber == null ||
                            item.BlacklistFaculty == true || item.Type == "Adjunct" ||
                            item.AppliedPAN == true || item.NoClass == true || item.XeroxcopyofcertificatesFlag == true
                           || (item.PhdUndertakingDocumentstatus == false) || item.InvalidAadhaar == "Yes"
                            ).Select(e => e).Count();
                ViewBag.ClearFaculty = ViewBag.TotalFaculty - ViewBag.FlagTotalFaculty;
                return View(teachingFaculty.OrderBy(t => t.id));
            }

            return View(teachingFaculty.OrderBy(t => t.id));
        }

        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification,DataEntry")]
        public ActionResult ViewCSEFacultyDetails(string fid)
        {
            FacultyRegistration regFaculty = new FacultyRegistration();
            int fID = 0;

            if (!string.IsNullOrEmpty(fid))
            {
                regFaculty.GenderId = null;
                regFaculty.isFacultyRatifiedByJNTU = null;
                fID = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(fid, WebConfigurationManager.AppSettings["CryptoKey"]));
                // Above code commented by Naushad Khan Anbd added the below line.
                // fID = Convert.ToInt32(fid);
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
                    regFaculty.WorkingStatus = faculty.WorkingStatus;
                    regFaculty.TotalExperience = faculty.TotalExperience;
                    regFaculty.OrganizationName = faculty.OrganizationName;
                    if (regFaculty.CollegeId != 0)
                    {
                        regFaculty.CollegeName = db.jntuh_college.Find(regFaculty.CollegeId).collegeCode + " - " + db.jntuh_college.Find(regFaculty.CollegeId).collegeName;
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
                    regFaculty.DeactivedOn = faculty.DeactivatedOn;
                    regFaculty.Deactivedby = faculty.DeactivatedBy;
                    regFaculty.PhdDeskVerification = faculty.PhdDeskVerification != null ? (bool)faculty.PhdDeskVerification : false;
                    regFaculty.PhdDeskVerCondition = faculty.PhdDeskVerification == null ? "" : faculty.PhdDeskVerification.ToString();
                    DateTime verificationstartdate = new DateTime(2019, 12, 31);
                    ViewBag.Isdone = true;
                    if (verificationstartdate > regFaculty.DeactivedOn || regFaculty.DeactivedOn == null)
                    {
                        ViewBag.Isdone = false;
                    }
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

                    string registrationNumber =
                        db.jntuh_registered_faculty.Where(of => of.id == fID)
                            .Select(of => of.RegistrationNumber)
                            .FirstOrDefault();
                    int[] pharmacydepartmetids = { 26, 36, 27, 36, 39, 61 };

                    int specializationid =
                        db.jntuh_college_faculty_registered.Where(of => of.RegistrationNumber == registrationNumber && of.FacultySpecializationId == null)
                            .Select(of => of.DepartmentId)
                            .FirstOrDefault() == null ? 0 : Convert.ToInt32(db.jntuh_college_faculty_registered.Where(of => of.RegistrationNumber == registrationNumber && of.FacultySpecializationId == null)
                            .Select(of => of.DepartmentId)
                            .FirstOrDefault());
                    ViewBag.shospecilizationlink = false;
                    regFaculty.PHDView = db.jntuh_faculty_phddetails.Where(i => i.Facultyid == faculty.id).Count();
                    //Commented on 18-06-2018 by Narayana Reddy
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
                    return RedirectToAction("FacultyVerificationIndex", "FacultyVerificationDENew");
                }
            }
            else
            {
                return RedirectToAction("FacultyVerificationIndex", "FacultyVerificationDENew");
            }
        }

        [Authorize(Roles = "Admin")]
        public ActionResult FacultyCSEflagsrollback(string fid, string collegeid)
        {
            int userid = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int fID = 0;
            TempData["Success"] = null;
            TempData["Error"] = null;
            if (fid != null)
            {
                fID = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(fid, System.Web.Configuration.WebConfigurationManager.AppSettings["CryptoKey"]));
            }
            if (fID != 0)
            {
                var jntuh_registered_faculty = db.jntuh_registered_faculty.Where(s => s.id == fID).Select(e => e).FirstOrDefault();
                if (jntuh_registered_faculty != null)
                {
                    //var facultyflags =
                    //    db.jntuh_college_facultyverification_tracking.Where(
                    //        r => r.registrationNumber == jntuh_registered_faculty.RegistrationNumber.Trim())
                    //        .Select(s => s)
                    //        .FirstOrDefault();
                    //if (facultyflags != null)
                    //{
                    //    jntuh_registered_faculty.NotQualifiedAsperAICTE = facultyflags.NotQualifiedAsperAICTE;
                    //    jntuh_registered_faculty.IncompleteCertificates = facultyflags.IncompleteCertificates;
                    //    jntuh_registered_faculty.InvalidPANNumber = facultyflags.InvalidPANNumber;
                    //    jntuh_registered_faculty.Xeroxcopyofcertificates = facultyflags.Xeroxcopyofcertificates;
                    //    jntuh_registered_faculty.NoRelevantPG = facultyflags.NoRelevantPG == true ? "Yes" : "No";
                    //    jntuh_registered_faculty.NoRelevantUG = facultyflags.NoRelevantUG == true ? "Yes" : "No";
                    //    jntuh_registered_faculty.NORelevantPHD = facultyflags.NoRelevantPHD == true ? "Yes" : "No";

                    //    //jntuh_registered_faculty.MultipleRegInSameCollege = false;
                    //    jntuh_registered_faculty.DeactivatedBy = facultyflags.deactivatedBy;
                    //    jntuh_registered_faculty.DeactivatedOn = facultyflags.deactivatedOn;
                    //    db.Entry(jntuh_registered_faculty).State = EntityState.Modified;
                    //    db.SaveChanges();
                    //    TempData["Success"] = jntuh_registered_faculty.RegistrationNumber + " Faculty Flags Rollback.";
                    //}
                    //else
                    //{
                    //    TempData["Error"] = "No record found in tracking.";
                    //}

                    jntuh_registered_faculty.PhdDeskVerification = null;
                    db.Entry(jntuh_registered_faculty).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Success"] = jntuh_registered_faculty.RegistrationNumber + " Faculty Flags Rollback.";
                }
                else
                    TempData["Error"] = "No Data found.";
            }

            return RedirectToAction("FacultyCSEPHDverification", new { collegeid = collegeid });
        }

        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification,DataEntry")]
        public ActionResult CSEPHDVerificationApprove(string fid, string collegeid)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            var fId = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(fid, WebConfigurationManager.AppSettings["CryptoKey"]));
            var facultydetails = db.jntuh_registered_faculty.Find(fId);
            if (facultydetails != null)
            {
                facultydetails.PhdDeskVerification = false;
                db.Entry(facultydetails).State = EntityState.Modified;
                db.SaveChanges();
                TempData["Success"] = facultydetails.RegistrationNumber + " Successfully Approved..";
            }
            return RedirectToAction("FacultyCSEPHDverification", "FacultyVerificationDENew", new { collegeid = collegeid });
        }

        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification,DataEntry")]
        public ActionResult CSEPHDVerificationNotApprove(string fid, string collegeid)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            var fId = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(fid, WebConfigurationManager.AppSettings["CryptoKey"]));
            var facultydetails = db.jntuh_registered_faculty.Find(fId);
            if (facultydetails != null)
            {
                facultydetails.PhdDeskVerification = true;
                db.Entry(facultydetails).State = EntityState.Modified;
                db.SaveChanges();
                TempData["Success"] = facultydetails.RegistrationNumber + " Successfully Not Approved..";
            }
            return RedirectToAction("FacultyCSEPHDverification", "FacultyVerificationDENew", new { collegeid = collegeid });
        }
    }
}
