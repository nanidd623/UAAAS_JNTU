using iTextSharp.text;
using iTextSharp.text.pdf;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using UAAAS.Models;
namespace UAAAS.Controllers
{
    [ErrorHandling]
    public class DatabaseReportsController : BaseController
    {
        static uaaasDBContext db = new uaaasDBContext();
        //
        // GET: /FacultyDetails/
        //int[] CurrentCollegesIds = new int[269] { 4, 7, 8, 9, 11, 12, 17, 20, 22, 23, 26, 29, 32, 34, 38, 40, 41, 48, 56, 59, 68, 69, 70, 72, 74, 77, 79, 80, 81, 84, 85, 86, 87, 88, 100, 103, 104, 106, 108, 109, 111, 113, 115, 116, 121, 123, 128, 129, 130, 132, 134, 137, 141, 143, 144, 145, 147, 148, 151, 152, 153, 155, 156, 157, 158, 159, 162, 163, 164, 165, 166, 168, 171, 172, 173, 175, 176, 177, 178, 179, 181, 182, 183, 184, 185, 186, 187, 188, 192, 193, 195, 196, 198, 201, 203, 207, 210, 211, 214, 215, 218, 222, 225, 227, 228, 236, 241, 242, 244, 245, 250, 254, 256, 260, 261, 264, 271, 273, 276, 282, 283, 287, 291, 292, 293, 299, 300, 304, 306, 307, 308, 309, 310, 315, 316, 321, 322, 324, 327, 329, 334, 335, 342, 349, 350, 352, 360, 365, 366, 367, 368, 369, 371, 373, 374, 376, 380, 382, 385, 391, 393, 395, 399, 400, 403, 414, 415, 420, 423, 424, 428, 429, 430, 6, 24, 27, 30, 44, 47, 52, 54, 55, 58, 60, 65, 78, 90, 95, 97, 105, 107, 110, 114, 117, 118, 120, 127, 135, 136, 139, 146, 150, 169, 202, 204, 206, 213, 219, 234, 237, 253, 262, 263, 267, 290, 295, 297, 298, 301, 302, 303, 313, 314, 317, 318, 319, 320, 348, 353, 370, 379, 384, 389, 392, 410, 5, 67, 246, 296, 343, 355, 386, 411, 421, 39, 42, 43, 75, 140, 180, 194, 217, 223, 266, 332, 364, 35, 50, 91, 174, 435, 436, 439, 441, 442, 443, 445, 449, 455 };
      
        [Authorize(Roles = "Admin")]
        public ActionResult GetAffilliatedColleges(int? AcademicYearId, string command)
        {
            List<AffiliatedColleges> CollegeList = new List<AffiliatedColleges>();
            var jntuh_academic_Year = db.jntuh_academic_year.ToList();
            var AcademicYears = jntuh_academic_Year.OrderByDescending(a => a.actualYear).Select(e => e).Take(5).ToList();
            ViewBag.AcademicYears = AcademicYears.Select(e => new
            {
                AcademicYearId = e.id,
                AcademicYear = e.academicYear
            }).ToList();

            var actualYear = jntuh_academic_Year.Where(e => e.isPresentAcademicYear == true && e.isActive == true).Select(e => e.actualYear).FirstOrDefault();
            int AY1 = jntuh_academic_Year.Where(e => e.actualYear == (actualYear + 1)).Select(e => e.id).FirstOrDefault();

            if (AcademicYearId != null && AcademicYearId != 0)
            {
                var Year = jntuh_academic_Year.Where(e => e.id == AcademicYearId).Select(e => e.academicYear).FirstOrDefault();
                var AffiliationCollegesIds = db.jntuh_college_intake_existing.Where(a => a.academicYearId == AcademicYearId && a.approvedIntake != 0).Select(z => z.collegeId).Distinct().ToList();
                var Address = db.jntuh_address.ToList();
                var CollegeIds = new List<int>();
                if (AcademicYearId==12)
                  CollegeIds = db.jntuh_college_edit_status.Where(a => a.academicyearId == AcademicYearId && a.IsCollegeEditable == false && a.collegeId != 375).Select(s => s.collegeId).ToList();
                else
                    CollegeIds = db.jntuh_college_edit_status.Where(a => a.academicyearId == AcademicYearId && a.isSubmitted == true && a.collegeId != 375).Select(s => s.collegeId).ToList();

                var Colleges = db.jntuh_college.Where(a => CollegeIds.Contains(a.id)).Select(q => q).ToList();
                var PrencipalData = db.jntuh_college_principal_registered.Where(p => CollegeIds.Contains(p.collegeId)).Select(p => new { p.RegistrationNumber ,p.collegeId}).ToList();
                string[] PrencipalReg = PrencipalData.Select(p => p.RegistrationNumber).ToArray();
                var RegData = db.jntuh_registered_faculty.Where(rf => PrencipalReg.Contains(rf.RegistrationNumber)).Select(rf => new { rf.RegistrationNumber, rf.Mobile, rf.Email, rf.FirstName, rf.LastName }).ToList();
                var Districts = db.jntuh_district.ToList();
                foreach (var item in Colleges)
                {
                    AffiliatedColleges College = new AffiliatedColleges();
                    College.CollegeId = item.id;
                    College.CollegeCode = item.collegeCode;
                    College.CollegeName = item.collegeName;
                    College.DistrictId = Address.Where(s => s.collegeId == College.CollegeId && s.addressTye == "COLLEGE").Select(z => z.districtId).FirstOrDefault();
                    College.CollegeAddress = Address.Where(s => s.collegeId == College.CollegeId && s.addressTye == "COLLEGE").Select(z => z.address).FirstOrDefault()+","+Address.Where(s => s.collegeId == College.CollegeId && s.addressTye == "COLLEGE").Select(z => z.townOrCity).FirstOrDefault()+","+Address.Where(s => s.collegeId == College.CollegeId && s.addressTye == "COLLEGE").Select(z => z.mandal).FirstOrDefault()+","+Districts.Where(d=>d.id==College.DistrictId).Select(d=>d.districtName).FirstOrDefault();
                    College.SubmissionDate = item.updatedOn.ToString();
                    College.AffiliatedStatus = AffiliationCollegesIds.Contains(item.id) ? "Yes" : "No";
                    College.CollegeEmail = Address.Where(s => s.collegeId == College.CollegeId && s.addressTye == "COLLEGE").Select(z => z.email).FirstOrDefault();
                    College.CollegeMobile = Address.Where(s => s.collegeId == College.CollegeId && s.addressTye == "COLLEGE").Select(z => z.mobile).FirstOrDefault();
                    College.SocietyEmail = Address.Where(s => s.collegeId == College.CollegeId && s.addressTye == "SOCIETY").Select(z => z.email).FirstOrDefault();
                    College.SocietyMobile = Address.Where(s => s.collegeId == College.CollegeId && s.addressTye == "SOCIETY").Select(z => z.mobile).FirstOrDefault();
                    College.SecretaryEmail = Address.Where(s => s.collegeId == College.CollegeId && s.addressTye == "SECRETARY").Select(z => z.email).FirstOrDefault();
                    College.SecretaryMobile = Address.Where(s => s.collegeId == College.CollegeId && s.addressTye == "SECRETARY").Select(z => z.mobile).FirstOrDefault();
                    College.PrincipalRegistrationNumber = PrencipalData.Where(p => p.collegeId == College.CollegeId).Select(p => p.RegistrationNumber).FirstOrDefault();
                    if (!string.IsNullOrEmpty(College.PrincipalRegistrationNumber))
                    {
                        College.PrincipalName = RegData.Where(p => p.RegistrationNumber == College.PrincipalRegistrationNumber).Select(p => p.FirstName.Trim()).FirstOrDefault() + " " + RegData.Where(p => p.RegistrationNumber == College.PrincipalRegistrationNumber).Select(p => p.LastName.Trim()).FirstOrDefault();
                        College.PrincipalMobile = RegData.Where(p => p.RegistrationNumber == College.PrincipalRegistrationNumber).Select(p => p.Mobile).FirstOrDefault(); College.PrincipalRegistrationNumber = PrencipalData.Where(p => p.collegeId == College.CollegeId).Select(p => p.RegistrationNumber).FirstOrDefault();
                        College.PrincipalEmail = RegData.Where(p => p.RegistrationNumber == College.PrincipalRegistrationNumber).Select(p => p.Email).FirstOrDefault(); College.PrincipalRegistrationNumber = PrencipalData.Where(p => p.collegeId == College.CollegeId).Select(p => p.RegistrationNumber).FirstOrDefault();

                    }
                    CollegeList.Add(College);
                }

                if (command == "Export")
                {
                    Response.ClearContent();
                    Response.Buffer = true;
                    Response.AddHeader("content-disposition", "attachment; filename=" + Year + "_Submission_Colleges.XLS");
                    Response.ContentType = "application/vnd.ms-excel";
                    return PartialView("~/Views/DatabaseReports/AssociatedCollegesExcel.cshtml", CollegeList.OrderBy(s => s.CollegeName).ToList());
                }              
            }
            return View(CollegeList.Where(a => a.CollegeId != 375).ToList());
        }

        [Authorize(Roles = "Admin")]
        public ActionResult GetCollegeFaculty(int? collegeId)
        {
            FacultyFieldsClass Faculty = new FacultyFieldsClass();
            var CollegesList = db.jntuh_college.Where(c => c.isActive == true && c.id != 375).Select(c => new { collegeId = c.id, collegeName = c.collegeCode + "-" + c.collegeName }).OrderBy(c => c.collegeName).ToList();

            CollegesList.Add(new { collegeId = -4, collegeName = "Engineering Colleges" });
            CollegesList.Add(new { collegeId = -3, collegeName = "Pharmacy Colleges" });
            CollegesList.Add(new { collegeId = -2, collegeName = "Total College Faculty" });
            CollegesList.Add(new { collegeId = -1, collegeName = "Active Login Colleges" });
            CollegesList.Add(new { collegeId = 0, collegeName = "All Submission Colleges" });

            ViewBag.Colleges = CollegesList.OrderBy(z => z.collegeId).ToList();
            Faculty.collegeId = collegeId;
            return View(Faculty);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult GetCollegeFaculty(FacultyFieldsClass faculty)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            List<string> Items = new List<string>();
            if (faculty.collegeId != null)
            {
                List<FacultyDetailsClass> AllFaculty = new List<FacultyDetailsClass>();
                var actualYear = db.jntuh_academic_year.Where(e => e.isPresentAcademicYear == true && e.isActive == true).Select(e => e.actualYear).FirstOrDefault();
                int AY1 = db.jntuh_academic_year.Where(e => e.actualYear == (actualYear + 1)).Select(e => e.id).FirstOrDefault();
                var jntuh_college = db.jntuh_college.Select(a => a).ToList();
                var ActiveLoginCollegeIds = jntuh_college.Where(a => a.isActive == true).Select(z => z.id).ToList();               
                var SubmittedCollegeIds = db.jntuh_college_edit_status.Where(a => a.academicyearId == AY1 && a.isSubmitted == true).Select(z => z.collegeId).ToList();                         
                var EngineeringCollegeIds = jntuh_college.Where(a => a.isActive == true && a.collegeTypeID == 1).Select(z => z.id).ToList();
                var PharmacyCollegeIds = jntuh_college.Where(a => a.isActive == true && a.collegeTypeID == 2).Select(z => z.id).ToList();
                var Single_jntuh_college = jntuh_college.Where(q => q.id == faculty.collegeId).Select(a => a).FirstOrDefault();

                var jntuh_Designation = db.jntuh_designation.ToList();
                var jntuh_department = db.jntuh_department.ToList();
                var jntuh_specialization = db.jntuh_specialization.ToList();

                List<jntuh_college_faculty_registered> CollegeFaculty = new List<jntuh_college_faculty_registered>();
                List<jntuh_college_principal_registered> CollegePrincipal = new List<jntuh_college_principal_registered>();

                if (faculty.collegeId == 0)
                {
                    CollegeFaculty = db.jntuh_college_faculty_registered.Where(w => SubmittedCollegeIds.Contains(w.collegeId)).Select(q => q).ToList();
                    CollegePrincipal = db.jntuh_college_principal_registered.Where(q => SubmittedCollegeIds.Contains(q.collegeId)).Select(w => w).ToList();
                }
                else if (faculty.collegeId == -1)
                {
                    CollegeFaculty = db.jntuh_college_faculty_registered.Where(w => ActiveLoginCollegeIds.Contains(w.collegeId)).Select(q => q).ToList();
                    CollegePrincipal = db.jntuh_college_principal_registered.Where(q => ActiveLoginCollegeIds.Contains(q.collegeId)).Select(w => w).ToList();
                }
                else if (faculty.collegeId == -2)
                {
                    CollegeFaculty = db.jntuh_college_faculty_registered.Select(q => q).ToList();
                    CollegePrincipal = db.jntuh_college_principal_registered.Select(w => w).ToList();
                }
                else if (faculty.collegeId == -3)
                {
                    var submissionIds = db.jntuh_college_edit_status.Where(q => q.academicyearId == AY1 && PharmacyCollegeIds.Contains(q.collegeId) && q.isSubmitted == true).Select(a => a.collegeId).ToList();
                    CollegeFaculty = db.jntuh_college_faculty_registered.Where(w => submissionIds.Contains(w.collegeId)).Select(q => q).ToList();
                    CollegePrincipal = db.jntuh_college_principal_registered.Where(q => submissionIds.Contains(q.collegeId)).Select(w => w).ToList();
                }
                else if (faculty.collegeId == -4)
                {
                    var submissionIds = db.jntuh_college_edit_status.Where(q => q.academicyearId == AY1 && EngineeringCollegeIds.Contains(q.collegeId) && q.isSubmitted == true).Select(a => a.collegeId).ToList();
                    CollegeFaculty = db.jntuh_college_faculty_registered.Where(w => submissionIds.Contains(w.collegeId)).Select(q => q).ToList();
                    CollegePrincipal = db.jntuh_college_principal_registered.Where(q => submissionIds.Contains(q.collegeId)).Select(w => w).ToList();
                }
                else
                {
                    CollegeFaculty = db.jntuh_college_faculty_registered.Where(w => w.collegeId == faculty.collegeId).Select(q => q).ToList();
                    CollegePrincipal = db.jntuh_college_principal_registered.Where(q => q.collegeId == faculty.collegeId).Select(w => w).ToList();
                }

                var RegNos = CollegeFaculty.Select(a => a.RegistrationNumber).ToList();
                string stringregnos = string.Empty;
                foreach (var item in RegNos)
                {
                    if (string.IsNullOrEmpty(stringregnos))
                        stringregnos += "'" + item + "'";
                    else
                        stringregnos += ",'" + item + "'";
                }

                List<FacultyRegistration> RegisteredFaculty = new List<FacultyRegistration>();
                string constr = "Data Source=10.10.10.16;user id=root;password=Jntu@123!@#123;database=uaaas207118;";
                MySqlConnection con = new MySqlConnection(constr);
                string query = "SELECT * FROM jntuh_registered_faculty where RegistrationNumber IN ( " + stringregnos + ")";
                con.Open();
                MySqlCommand cmd = new MySqlCommand(query, con);
                MySqlDataReader dr = cmd.ExecuteReader();
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {
                        FacultyRegistration facultyData = new FacultyRegistration();
                        facultyData.id = Convert.ToInt32(dr["id"]);
                        facultyData.FirstName = dr["FirstName"].ToString();
                        facultyData.MiddleName = dr["MiddleName"].ToString();
                        facultyData.LastName = dr["LastName"].ToString();
                        facultyData.RegistrationNumber = dr["RegistrationNumber"].ToString();
                        facultyData.FatherOrhusbandName = dr["FatherOrHusbandName"].ToString();
                        facultyData.MotherName = dr["FatherOrHusbandName"].ToString();
                        facultyData.GenderId = Convert.ToInt32(dr["GenderId"]);
                        facultyData.DateOfBirth = Convert.ToDateTime(dr["DateOfBirth"]);
                        facultyData.Email = dr["Email"].ToString();
                        facultyData.Mobile = dr["Mobile"].ToString();
                        facultyData.PANNumber = dr["PANNumber"].ToString();
                        facultyData.AadhaarNumber = dr["AadhaarNumber"].ToString();
                        facultyData.facultyPhoto = dr["Photo"].ToString();
                        facultyData.facultyPANCardDocument = dr["PANDocument"].ToString();
                        facultyData.facultyAadhaarCardDocument = dr["AadhaarDocument"].ToString();
                        var DesignationId = dr["DesignationId"].ToString();
                        facultyData.DesignationId = string.IsNullOrEmpty(DesignationId) ? 0 : Convert.ToInt32(DesignationId);
                        facultyData.OtherDesignation = dr["OtherDesignation"].ToString();
                        facultyData.GrossSalary = dr["grosssalary"].ToString();
                        facultyData.AICTEFacultyId = dr["AICTEFacultyId"].ToString();
                        var TotalExperience = dr["TotalExperience"].ToString();
                        facultyData.TotalExperience = string.IsNullOrEmpty(TotalExperience) ? 0 : Convert.ToInt32(TotalExperience);
                        var TotalExperiencePresentCollege = dr["TotalExperiencePresentCollege"].ToString();
                        facultyData.TotalExperiencePresentCollege = string.IsNullOrEmpty(TotalExperiencePresentCollege) ? 0 : Convert.ToInt32(TotalExperiencePresentCollege);
                        facultyData.facultyDateOfAppointment = dr["DateOfAppointment"].ToString();
                        facultyData.DeactivationReason = dr["DeactivationReason"].ToString();
                        facultyData.Others1 = dr["Others1"].ToString();
                        RegisteredFaculty.Add(facultyData);
                    }
                }
                con.Close();
                var FacultyIds = RegisteredFaculty.Select(q => q.id).ToList();

                foreach (var item in RegisteredFaculty)
                {
                    FacultyDetailsClass Faculty = new FacultyDetailsClass();
                    Faculty.collegeId = CollegeFaculty.Where(a => a.RegistrationNumber == item.RegistrationNumber).Select(z => z.collegeId).FirstOrDefault() == 0 ?
                            CollegePrincipal.Where(a => a.RegistrationNumber == item.RegistrationNumber).Select(z => z.collegeId).FirstOrDefault() : CollegeFaculty.Where(a => a.RegistrationNumber == item.RegistrationNumber).Select(z => z.collegeId).FirstOrDefault();

                    Faculty.collegeCode = jntuh_college.Where(a => a.id == Faculty.collegeId).Select(a => a.collegeCode).FirstOrDefault();
                    Faculty.collegeName = jntuh_college.Where(a => a.id == Faculty.collegeId).Select(a => a.collegeName).FirstOrDefault();
                    Faculty.FacultyRegistrationNumber = item.RegistrationNumber;
                    string principal_Regno = CollegePrincipal.Where(a => a.collegeId == Faculty.collegeId).Select(a => a.RegistrationNumber).FirstOrDefault();
                    if (String.IsNullOrEmpty(principal_Regno))
                        Faculty.FacultyPrincipalCase = principal_Regno == item.RegistrationNumber ? "Yes" : "No";
                    else
                        Faculty.FacultyPrincipalCase = "No";
                    Faculty.FacultyName = item.FirstName + " " + item.MiddleName + " " + item.LastName;
                    Faculty.FacultyFathersName = item.FatherOrhusbandName;
                    Faculty.FacultyMothersName = item.MotherName;
                    Faculty.FacultyGender = item.GenderId == 1 ? "Male" : "Female";
                    DateTime DOB = Convert.ToDateTime(item.DateOfBirth);
                    Faculty.FacultyDateofBirth = DOB == null ? null : DOB.ToString("dd-MM-yyyy");
                    Faculty.FacultyEmail = item.Email;
                    Faculty.FacultyMobile = item.Mobile;
                    Faculty.FacultyPANNumber = item.PANNumber;
                    Faculty.FacultyAadhaarNumber = item.AadhaarNumber;
                    Faculty.FacultyPhoto = item.facultyPhoto;
                    Faculty.FacultyPANDocument = item.facultyPANCardDocument;
                    Faculty.FacultyAadhaarDocument = item.facultyAadhaarCardDocument;
                    Faculty.IdentifiedFor = CollegeFaculty.Where(z => z.RegistrationNumber == item.RegistrationNumber).Select(q => q.IdentifiedFor).FirstOrDefault();
                    Faculty.FacultyDesignation = jntuh_Designation.Where(q => q.id == item.DesignationId).Select(z => z.designation).FirstOrDefault();
                    Faculty.FacultyOtherDesignation = item.OtherDesignation;
                    Faculty.DepartmentId = CollegeFaculty.Where(z => z.RegistrationNumber == item.RegistrationNumber).Select(q => q.DepartmentId).FirstOrDefault();
                    Faculty.FacultyDepartment = jntuh_department.Where(q => q.id == Faculty.DepartmentId).Select(z => z.departmentName).FirstOrDefault();
                    //Faculty.FacultyOtherDepartment = item.OtherDepartment;
                    Faculty.SpecializationId = CollegeFaculty.Where(z => z.RegistrationNumber == item.RegistrationNumber).Select(q => q.SpecializationId).FirstOrDefault();
                    Faculty.FacultySpecialization = Faculty.SpecializationId == null ? null : jntuh_specialization.Where(q => q.id == Faculty.SpecializationId).Select(z => z.specializationName).FirstOrDefault();
                    Faculty.FacultyGrossSalary = item.GrossSalary;
                    Faculty.FacultyAICTEFacultyId = item.AICTEFacultyId;
                    Faculty.FacultyTotalExperience = item.TotalExperience;
                    Faculty.FacultyPresentExperience = item.TotalExperiencePresentCollege;
                    DateTime DOA = Convert.ToDateTime(item.DateOfAppointment);
                    Faculty.FacultyDateofAppointment = DOA == null ? null : DOA.ToString("dd-MM-yyyy");
                    Faculty.FacultyDeactivationReason = item.DeactivationReason;
                    Faculty.FacultyPHDTwoPagesFormat = item.Others1;

                    List<jntuh_registered_faculty_education> jntuh_registered_faculty_education = db.jntuh_registered_faculty_education.Where(a => a.facultyId == item.id).Select(z => z).ToList();

                    var SSCEducation = jntuh_registered_faculty_education.Where(q => q.educationId == 1).Select(a => a).FirstOrDefault();
                    var UGEducation = jntuh_registered_faculty_education.Where(q => q.educationId == 3).Select(a => a).FirstOrDefault();
                    var PGEducation = jntuh_registered_faculty_education.Where(q => q.educationId == 4).Select(a => a).FirstOrDefault();
                    var MPhilEducation = jntuh_registered_faculty_education.Where(q => q.educationId == 5).Select(a => a).FirstOrDefault();
                    var PhDEducation = jntuh_registered_faculty_education.Where(q => q.educationId == 6).Select(a => a).FirstOrDefault();


                    if (SSCEducation != null)
                    {
                        Faculty.SSC_studiedEducation = String.IsNullOrEmpty(SSCEducation.courseStudied) ? SSCEducation.courseStudied : SSCEducation.courseStudied.Trim().ToString();
                        Faculty.SSC_specialization = String.IsNullOrEmpty(SSCEducation.specialization) ? SSCEducation.specialization : SSCEducation.specialization.Trim().ToString();
                        Faculty.SSC_passedYear = SSCEducation.passedYear;
                        Faculty.SSC_percentage = SSCEducation.marksPercentage;
                        Faculty.SSC_division = SSCEducation.division;
                        Faculty.SSC_university = String.IsNullOrEmpty(SSCEducation.boardOrUniversity) ? SSCEducation.boardOrUniversity : SSCEducation.boardOrUniversity.Trim().ToString();
                        Faculty.SSC_place = String.IsNullOrEmpty(SSCEducation.placeOfEducation) ? SSCEducation.placeOfEducation : SSCEducation.placeOfEducation.Trim().ToString();
                        Faculty.SSC_certificate = String.IsNullOrEmpty(SSCEducation.certificate) ? SSCEducation.certificate : SSCEducation.certificate.Trim().ToString();
                    }
                    if (UGEducation != null)
                    {
                        Faculty.UG_studiedEducation = String.IsNullOrEmpty(UGEducation.courseStudied) ? UGEducation.courseStudied : UGEducation.courseStudied.Trim().ToString();
                        Faculty.UG_specialization = String.IsNullOrEmpty(UGEducation.specialization) ? UGEducation.specialization : UGEducation.specialization.Trim().ToString();
                        Faculty.UG_passedYear = UGEducation.passedYear;
                        Faculty.UG_percentage = UGEducation.marksPercentage;
                        Faculty.UG_division = UGEducation.division;
                        Faculty.UG_university = String.IsNullOrEmpty(UGEducation.boardOrUniversity) ? UGEducation.boardOrUniversity : UGEducation.boardOrUniversity.Trim().ToString();
                        Faculty.UG_place = String.IsNullOrEmpty(UGEducation.placeOfEducation) ? UGEducation.placeOfEducation : UGEducation.placeOfEducation.Trim().ToString();
                        Faculty.UG_certificate = String.IsNullOrEmpty(UGEducation.certificate) ? UGEducation.certificate : UGEducation.certificate.Trim().ToString();
                    }

                    if (PGEducation != null)
                    {
                        Faculty.PG_studiedEducation = String.IsNullOrEmpty(PGEducation.courseStudied) ? PGEducation.courseStudied : PGEducation.courseStudied.Trim().ToString();
                        Faculty.PG_specialization = String.IsNullOrEmpty(PGEducation.specialization) ? PGEducation.specialization : PGEducation.specialization.Trim().ToString();
                        Faculty.PG_passedYear = PGEducation.passedYear;
                        Faculty.PG_percentage = PGEducation.marksPercentage;
                        Faculty.PG_division = PGEducation.division;
                        Faculty.PG_university = String.IsNullOrEmpty(PGEducation.boardOrUniversity) ? PGEducation.boardOrUniversity : PGEducation.boardOrUniversity.Trim().ToString();
                        Faculty.PG_place = String.IsNullOrEmpty(PGEducation.placeOfEducation) ? PGEducation.placeOfEducation : PGEducation.placeOfEducation.Trim().ToString();
                        Faculty.PG_certificate = String.IsNullOrEmpty(PGEducation.certificate) ? PGEducation.certificate : PGEducation.certificate.Trim().ToString();
                    }

                    if (MPhilEducation != null)
                    {
                        Faculty.MPhil_studiedEducation = String.IsNullOrEmpty(MPhilEducation.courseStudied) ? MPhilEducation.courseStudied : MPhilEducation.courseStudied.Trim().ToString();
                        Faculty.MPhil_specialization = String.IsNullOrEmpty(MPhilEducation.specialization) ? MPhilEducation.specialization : MPhilEducation.specialization.Trim().ToString();
                        Faculty.MPhil_passedYear = MPhilEducation.passedYear;
                        Faculty.MPhil_percentage = MPhilEducation.marksPercentage;
                        Faculty.MPhil_division = MPhilEducation.division;
                        Faculty.MPhil_university = String.IsNullOrEmpty(MPhilEducation.boardOrUniversity) ? MPhilEducation.boardOrUniversity : MPhilEducation.boardOrUniversity.Trim().ToString();
                        Faculty.MPhil_place = String.IsNullOrEmpty(MPhilEducation.placeOfEducation) ? MPhilEducation.placeOfEducation : MPhilEducation.placeOfEducation.Trim().ToString();
                        Faculty.MPhil_certificate = String.IsNullOrEmpty(MPhilEducation.certificate) ? MPhilEducation.certificate : MPhilEducation.certificate.Trim().ToString();
                    }

                    if (PhDEducation != null)
                    {
                        Faculty.PhD_studiedEducation = String.IsNullOrEmpty(PhDEducation.courseStudied) ? PhDEducation.courseStudied : PhDEducation.courseStudied.Trim().ToString();
                        Faculty.PhD_specialization = String.IsNullOrEmpty(PhDEducation.specialization) ? PhDEducation.specialization : PhDEducation.specialization.Trim().ToString();
                        Faculty.PhD_passedYear = PhDEducation.passedYear;
                        Faculty.PhD_percentage = PhDEducation.marksPercentage;
                        Faculty.PhD_division = PhDEducation.division;
                        Faculty.PhD_university = String.IsNullOrEmpty(PhDEducation.boardOrUniversity) ? PhDEducation.boardOrUniversity : PhDEducation.boardOrUniversity.Trim().ToString();
                        Faculty.PhD_place = String.IsNullOrEmpty(PhDEducation.placeOfEducation) ? PhDEducation.placeOfEducation : PhDEducation.placeOfEducation.Trim().ToString();
                        Faculty.PhD_certificate = String.IsNullOrEmpty(PhDEducation.certificate) ? PhDEducation.certificate : PhDEducation.certificate.Trim().ToString();
                    }

                    Faculty.All_certificate = jntuh_registered_faculty_education.Where(q => q.facultyId == item.id && q.educationId == 8).Select(a => a.certificate).FirstOrDefault();
                    AllFaculty.Add(Faculty);
                }

                faculty.FacultyDetails = AllFaculty;

                Response.ClearContent();
                Response.Buffer = true;
                if (faculty.collegeId == 0)
                    Response.AddHeader("content-disposition", "attachment; filename=Submission Colleges FacultyData.XLS");
                else if (faculty.collegeId == -1)
                    Response.AddHeader("content-disposition", "attachment; filename=Login College FacultyData.XLS");
                else if (faculty.collegeId == -2)
                    Response.AddHeader("content-disposition", "attachment; filename=All Colleges Teaching FacultyData.XLS");
                else if (faculty.collegeId == -3)
                    Response.AddHeader("content-disposition", "attachment; filename=All Pharmacy Colleges Teaching FacultyData.XLS");
                else if (faculty.collegeId == -4)
                    Response.AddHeader("content-disposition", "attachment; filename=All Engineering Colleges Teaching FacultyData.XLS");
                else
                    Response.AddHeader("content-disposition", "attachment; filename=" + Single_jntuh_college.collegeName + "(" + Single_jntuh_college.collegeCode + ")_CollegeFacultyData.XLS");

                Response.ContentType = "application/vnd.ms-excel";
                return PartialView("~/Views/DatabaseReports/DownloadCollegeFacultyExcel.cshtml", faculty);

            }
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin")]
        public ActionResult GetregisteredFaculty()
        {
            FacultyFieldsClass Faculty = new FacultyFieldsClass();
            return View(Faculty);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult GetregisteredFaculty(FacultyFieldsClass faculty)
        {
            if (faculty.RegNosExcel != null)
            {
                string excelpath = "~/Content/Upload/DatabaseReports";
                if (!Directory.Exists(Server.MapPath(excelpath)))
                    Directory.CreateDirectory(Server.MapPath(excelpath));

                var ext = Path.GetExtension(faculty.RegNosExcel.FileName);
                if (ext.ToUpper().Equals(".XLS") || ext.ToUpper().Equals(".XLSX"))
                    faculty.RegNosExcel.SaveAs(string.Format("{0}/{1}", Server.MapPath(excelpath), faculty.RegNosExcel.FileName));
                else
                    return RedirectToAction("GetregisteredFaculty");

                string excelConnectionString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + Path.Combine(Server.MapPath(excelpath), faculty.RegNosExcel.FileName) + ";Extended Properties=Excel 12.0;Persist Security Info=False";

                //Create Connection to Excel work book                   
                OleDbConnection excelConnection = new OleDbConnection(excelConnectionString);
                excelConnection.Close();
                //Create OleDbCommand to fetch data from Excel
                excelConnection.Open();
                OleDbCommand cmd1 = new OleDbCommand("select count(*) from [" + excelConnection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null).Rows[0]["Table_Name"].ToString() + "]", excelConnection);
                var rows = (int)cmd1.ExecuteScalar();
                OleDbCommand cmd2 = new OleDbCommand("select * from [" + excelConnection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null).Rows[0]["Table_Name"].ToString() + "]", excelConnection);

                DataTable dt = new DataTable();
                OleDbDataAdapter oleda = new OleDbDataAdapter();
                oleda.SelectCommand = cmd2;
                DataSet ds = new DataSet();
                oleda.Fill(ds);
                dt = ds.Tables[0];
                List<string> RegNos = new List<string>();
                List<string> FailureRegNos = new List<string>();
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(dt.Rows[i][1].ToString()))
                        {
                            RegNos.Add(dt.Rows[i][1].ToString().Trim());
                            continue;
                        }
                    }
                    catch (Exception)
                    {
                        FailureRegNos.Add(dt.Rows[i][0].ToString().Trim());
                        continue;
                    }
                }

                excelConnection.Close();

                if (RegNos.Count > 0)
                {
                    List<FacultyDetailsClass> AllFaculty = new List<FacultyDetailsClass>();
                    var jntuh_college = db.jntuh_college.ToList();
                    List<jntuh_registered_faculty> jntuh_registered_faculty = new List<Models.jntuh_registered_faculty>();
                    if (faculty.ColumnType == 1)
                        jntuh_registered_faculty = db.jntuh_registered_faculty.Where(z => RegNos.Contains(z.RegistrationNumber)).Select(a => a).Distinct().ToList();
                    else if (faculty.ColumnType == 2)
                        jntuh_registered_faculty = db.jntuh_registered_faculty.Where(z => RegNos.Contains(z.PANNumber)).Select(a => a).Distinct().ToList();
                    else
                        jntuh_registered_faculty = db.jntuh_registered_faculty.Where(z => RegNos.Contains(z.AadhaarNumber)).Select(a => a).Distinct().ToList();

                    var FacultyIds = jntuh_registered_faculty.Select(q => q.id).ToList();
                    var FacultyRegNos = jntuh_registered_faculty.Select(q => q.RegistrationNumber).ToList();
                    var college_Faculty = db.jntuh_college_faculty_registered.Where(z => FacultyRegNos.Contains(z.RegistrationNumber)).Select(a => a).ToList();
                    var CollegeRegNos = college_Faculty.Select(a => a.RegistrationNumber).ToList();

                    jntuh_registered_faculty = faculty.CollegeType == 1 ? jntuh_registered_faculty.Where(z => CollegeRegNos.Contains(z.RegistrationNumber)).Select(a => a).Distinct().ToList() :
                                                    faculty.CollegeType == 2 ? jntuh_registered_faculty.Where(z => !CollegeRegNos.Contains(z.RegistrationNumber)).Select(a => a).Distinct().ToList() :
                                                    jntuh_registered_faculty.ToList();

                    var jntuh_Designation = db.jntuh_designation.ToList();
                    var jntuh_department = db.jntuh_department.ToList();
                    var jntuh_specialization = db.jntuh_specialization.ToList();

                    var jntuh_registered_faculty_education = db.jntuh_registered_faculty_education.Where(a => FacultyIds.Contains(a.facultyId)).Select(z => z).ToList();

                    var principal = db.jntuh_college_principal_registered.Where(q => RegNos.Contains(q.RegistrationNumber)).Select(w => w).ToList();
                    foreach (var item in jntuh_registered_faculty)
                    {
                        FacultyDetailsClass Faculty = new FacultyDetailsClass();
                        Faculty.collegeId = college_Faculty.Where(a => a.RegistrationNumber == item.RegistrationNumber).Select(z => z.collegeId).FirstOrDefault() == 0 ?
                            principal.Where(a => a.RegistrationNumber == item.RegistrationNumber).Select(z => z.collegeId).FirstOrDefault() : college_Faculty.Where(a => a.RegistrationNumber == item.RegistrationNumber).Select(z => z.collegeId).FirstOrDefault();
                        Faculty.collegeCode = jntuh_college.Where(a => a.id == Faculty.collegeId).Select(x => x.collegeCode).FirstOrDefault();
                        Faculty.collegeName = jntuh_college.Where(a => a.id == Faculty.collegeId).Select(x => x.collegeName).FirstOrDefault();
                        Faculty.FacultyRegistrationNumber = item.RegistrationNumber;
                        if (principal.Where(a => a.RegistrationNumber == item.RegistrationNumber).Select(z => z.collegeId).FirstOrDefault() != 0)
                            Faculty.FacultyPrincipalCase = "Yes";
                        else
                            Faculty.FacultyPrincipalCase = "No";
                        Faculty.FacultyName = item.FirstName + " " + item.MiddleName + " " + item.LastName;
                        Faculty.FacultyFathersName = item.FatherOrHusbandName;
                        Faculty.FacultyMothersName = item.MotherName;
                        Faculty.FacultyGender = item.GenderId == 1 ? "Male" : "Female";
                        Faculty.FacultyDateofBirth = item.DateOfBirth.ToString("dd-MM-yyyy");
                        Faculty.FacultyEmail = item.Email;
                        Faculty.FacultyMobile = item.Mobile;
                        Faculty.FacultyPANNumber = item.PANNumber;
                        Faculty.FacultyAadhaarNumber = item.AadhaarNumber;
                        Faculty.FacultyPhoto = item.Photo;
                        Faculty.FacultyPANDocument = item.PANDocument;
                        Faculty.FacultyAadhaarDocument = item.AadhaarDocument;

                        Faculty.FacultyDesignation = jntuh_Designation.Where(q => q.id == item.DesignationId).Select(z => z.designation).FirstOrDefault();
                        Faculty.FacultyOtherDesignation = item.OtherDesignation;
                        Faculty.DepartmentId = college_Faculty.Where(z => z.RegistrationNumber == item.RegistrationNumber).Select(q => q.DepartmentId).FirstOrDefault();
                        Faculty.FacultyDepartment = jntuh_department.Where(q => q.id == Faculty.DepartmentId).Select(z => z.departmentName).FirstOrDefault();
                        Faculty.FacultyOtherDepartment = item.OtherDepartment;
                        Faculty.SpecializationId = college_Faculty.Where(z => z.RegistrationNumber == item.RegistrationNumber).Select(q => q.SpecializationId).FirstOrDefault();
                        Faculty.FacultySpecialization = Faculty.SpecializationId == null ? null : jntuh_specialization.Where(q => q.id == Faculty.SpecializationId).Select(z => z.specializationName).FirstOrDefault();
                        Faculty.FacultyGrossSalary = item.grosssalary;
                        Faculty.FacultyAICTEFacultyId = item.AICTEFacultyId;
                        Faculty.FacultyTotalExperience = item.TotalExperience;
                        Faculty.FacultyPresentExperience = item.TotalExperiencePresentCollege;
                        DateTime DOA = Convert.ToDateTime(item.DateOfAppointment);
                        Faculty.FacultyDateofAppointment = DOA == null ? null : DOA.ToString("dd-MM-yyyy");
                        Faculty.FacultyDeactivationReason = item.DeactivationReason;
                        Faculty.FacultyPHDTwoPagesFormat = item.Others1;

                        var SSCEducation = jntuh_registered_faculty_education.Where(q => q.facultyId == item.id && q.educationId == 1).Select(a => a).FirstOrDefault();
                        var UGEducation = jntuh_registered_faculty_education.Where(q => q.facultyId == item.id && q.educationId == 3).Select(a => a).FirstOrDefault();
                        var PGEducation = jntuh_registered_faculty_education.Where(q => q.facultyId == item.id && q.educationId == 4).Select(a => a).FirstOrDefault();
                        var MPhilEducation = jntuh_registered_faculty_education.Where(q => q.facultyId == item.id && q.educationId == 5).Select(a => a).FirstOrDefault();
                        var PhDEducation = jntuh_registered_faculty_education.Where(q => q.facultyId == item.id && q.educationId == 6).Select(a => a).FirstOrDefault();


                        if (SSCEducation != null)
                        {
                            Faculty.SSC_studiedEducation = String.IsNullOrEmpty(SSCEducation.courseStudied) ? SSCEducation.courseStudied : SSCEducation.courseStudied.Trim().ToString();
                            Faculty.SSC_specialization = String.IsNullOrEmpty(SSCEducation.specialization) ? SSCEducation.specialization : SSCEducation.specialization.Trim().ToString();
                            Faculty.SSC_passedYear = SSCEducation.passedYear;
                            Faculty.SSC_percentage = SSCEducation.marksPercentage;
                            Faculty.SSC_division = SSCEducation.division;
                            Faculty.SSC_university = String.IsNullOrEmpty(SSCEducation.boardOrUniversity) ? SSCEducation.boardOrUniversity : SSCEducation.boardOrUniversity.Trim().ToString();
                            Faculty.SSC_place = String.IsNullOrEmpty(SSCEducation.placeOfEducation) ? SSCEducation.placeOfEducation : SSCEducation.placeOfEducation.Trim().ToString();
                            Faculty.SSC_certificate = String.IsNullOrEmpty(SSCEducation.certificate) ? SSCEducation.certificate : SSCEducation.certificate.Trim().ToString();
                        }
                        if (UGEducation != null)
                        {
                            Faculty.UG_studiedEducation = String.IsNullOrEmpty(UGEducation.courseStudied) ? UGEducation.courseStudied : UGEducation.courseStudied.Trim().ToString();
                            Faculty.UG_specialization = String.IsNullOrEmpty(UGEducation.specialization) ? UGEducation.specialization : UGEducation.specialization.Trim().ToString();
                            Faculty.UG_passedYear = UGEducation.passedYear;
                            Faculty.UG_percentage = UGEducation.marksPercentage;
                            Faculty.UG_division = UGEducation.division;
                            Faculty.UG_university = String.IsNullOrEmpty(UGEducation.boardOrUniversity) ? UGEducation.boardOrUniversity : UGEducation.boardOrUniversity.Trim().ToString();
                            Faculty.UG_place = String.IsNullOrEmpty(UGEducation.placeOfEducation) ? UGEducation.placeOfEducation : UGEducation.placeOfEducation.Trim().ToString();
                            Faculty.UG_certificate = String.IsNullOrEmpty(UGEducation.certificate) ? UGEducation.certificate : UGEducation.certificate.Trim().ToString();
                        }

                        if (PGEducation != null)
                        {
                            Faculty.PG_studiedEducation = String.IsNullOrEmpty(PGEducation.courseStudied) ? PGEducation.courseStudied : PGEducation.courseStudied.Trim().ToString();
                            Faculty.PG_specialization = String.IsNullOrEmpty(PGEducation.specialization) ? PGEducation.specialization : PGEducation.specialization.Trim().ToString();
                            Faculty.PG_passedYear = PGEducation.passedYear;
                            Faculty.PG_percentage = PGEducation.marksPercentage;
                            Faculty.PG_division = PGEducation.division;
                            Faculty.PG_university = String.IsNullOrEmpty(PGEducation.boardOrUniversity) ? PGEducation.boardOrUniversity : PGEducation.boardOrUniversity.Trim().ToString();
                            Faculty.PG_place = String.IsNullOrEmpty(PGEducation.placeOfEducation) ? PGEducation.placeOfEducation : PGEducation.placeOfEducation.Trim().ToString();
                            Faculty.PG_certificate = String.IsNullOrEmpty(PGEducation.certificate) ? PGEducation.certificate : PGEducation.certificate.Trim().ToString();
                        }

                        if (MPhilEducation != null)
                        {
                            Faculty.MPhil_studiedEducation = String.IsNullOrEmpty(MPhilEducation.courseStudied) ? MPhilEducation.courseStudied : MPhilEducation.courseStudied.Trim().ToString();
                            Faculty.MPhil_specialization = String.IsNullOrEmpty(MPhilEducation.specialization) ? MPhilEducation.specialization : MPhilEducation.specialization.Trim().ToString();
                            Faculty.MPhil_passedYear = MPhilEducation.passedYear;
                            Faculty.MPhil_percentage = MPhilEducation.marksPercentage;
                            Faculty.MPhil_division = MPhilEducation.division;
                            Faculty.MPhil_university = String.IsNullOrEmpty(MPhilEducation.boardOrUniversity) ? MPhilEducation.boardOrUniversity : MPhilEducation.boardOrUniversity.Trim().ToString();
                            Faculty.MPhil_place = String.IsNullOrEmpty(MPhilEducation.placeOfEducation) ? MPhilEducation.placeOfEducation : MPhilEducation.placeOfEducation.Trim().ToString();
                            Faculty.MPhil_certificate = String.IsNullOrEmpty(MPhilEducation.certificate) ? MPhilEducation.certificate : MPhilEducation.certificate.Trim().ToString();
                        }

                        if (PhDEducation != null)
                        {
                            Faculty.PhD_studiedEducation = String.IsNullOrEmpty(PhDEducation.courseStudied) ? PhDEducation.courseStudied : PhDEducation.courseStudied.Trim().ToString();
                            Faculty.PhD_specialization = String.IsNullOrEmpty(PhDEducation.specialization) ? PhDEducation.specialization : PhDEducation.specialization.Trim().ToString();
                            Faculty.PhD_passedYear = PhDEducation.passedYear;
                            Faculty.PhD_percentage = PhDEducation.marksPercentage;
                            Faculty.PhD_division = PhDEducation.division;
                            Faculty.PhD_university = String.IsNullOrEmpty(PhDEducation.boardOrUniversity) ? PhDEducation.boardOrUniversity : PhDEducation.boardOrUniversity.Trim().ToString();
                            Faculty.PhD_place = String.IsNullOrEmpty(PhDEducation.placeOfEducation) ? PhDEducation.placeOfEducation : PhDEducation.placeOfEducation.Trim().ToString();
                            Faculty.PhD_certificate = String.IsNullOrEmpty(PhDEducation.certificate) ? PhDEducation.certificate : PhDEducation.certificate.Trim().ToString();
                        }

                        Faculty.All_certificate = jntuh_registered_faculty_education.Where(q => q.facultyId == item.id && q.educationId == 8).Select(a => a.certificate).FirstOrDefault();
                        AllFaculty.Add(Faculty);
                    }

                    faculty.FacultyDetails = AllFaculty;

                    Response.ClearContent();
                    Response.Buffer = true;
                    Response.AddHeader("content-disposition", "attachment; filename=RegisteredFacultyData.XLS");
                    Response.ContentType = "application/vnd.ms-excel";
                    return PartialView("~/Views/DatabaseReports/_DownloadRegFacultyExcel.cshtml", faculty);
                }

            }
            return RedirectToAction("GetregisteredFaculty");
        }

        [Authorize(Roles = "Admin")]
        public ActionResult GetApprovedAdmittedIntakeReport(int? collegeId)
        {
            var CollegesList = db.jntuh_college.Where(c => c.isActive == true && c.id != 375).Select(c => new { collegeId = c.id, collegeName = c.collegeCode + "-" + c.collegeName }).OrderBy(c => c.collegeName).ToList();
            CollegesList.Add(new { collegeId = 0, collegeName = "All Submission Colleges" });
            CollegesList.Add(new { collegeId = -1, collegeName = "Active Login Colleges" });

            ViewBag.Colleges = CollegesList.OrderBy(a => a.collegeId).ToList();
            var jntuh_academic_year = db.jntuh_academic_year.ToList();
            int actualYear = jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            ApprovedAdmittedIntake Intake = new ApprovedAdmittedIntake();
            Intake.CollegeId = collegeId;
            ViewBag.PresentYear = String.Format("{0}-{1}", (actualYear + 1).ToString(), (actualYear + 2).ToString().Substring(2, 2));
            ViewBag.FirstYear = String.Format("{0}-{1}", (actualYear).ToString(), (actualYear + 1).ToString().Substring(2, 2));
            ViewBag.SecondYear = String.Format("{0}-{1}", (actualYear - 1).ToString(), (actualYear).ToString().Substring(2, 2));
            ViewBag.ThirdYear = String.Format("{0}-{1}", (actualYear - 2).ToString(), (actualYear - 1).ToString().Substring(2, 2));
            ViewBag.FourthYear = String.Format("{0}-{1}", (actualYear - 3).ToString(), (actualYear - 2).ToString().Substring(2, 2));
            ViewBag.FifthYear = String.Format("{0}-{1}", (actualYear - 4).ToString(), (actualYear - 3).ToString().Substring(2, 2));
            ViewBag.SixYear = String.Format("{0}-{1}", (actualYear - 5).ToString(), (actualYear - 4).ToString().Substring(2, 2));
            return View(Intake);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult GetApprovedAdmittedIntakeReport(ApprovedAdmittedIntake obj, string command)
        {
            List<ApprovedAdmittedIntake> IntakeList = new List<ApprovedAdmittedIntake>();
            if (obj.CollegeId != null)
            {
                string ExcelFileName = string.Empty;
                List<CollegeIntakeExisting> collegeIntakeExisting = new List<CollegeIntakeExisting>();
                var jntuh_academic_year = db.jntuh_academic_year.ToList();
                int actualYear = jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
                int presentYear = jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(z => z.id).FirstOrDefault();
                var jntuh_college = db.jntuh_college.ToList();

                ViewBag.PresentYear = String.Format("{0}-{1}", (actualYear + 1).ToString().Substring(2, 2), (actualYear + 2).ToString().Substring(2, 2));
                ViewBag.FirstYear = String.Format("{0}-{1}", (actualYear).ToString().Substring(2, 2), (actualYear + 1).ToString().Substring(2, 2));
                ViewBag.SecondYear = String.Format("{0}-{1}", (actualYear - 1).ToString().Substring(2, 2), (actualYear).ToString().Substring(2, 2));
                ViewBag.ThirdYear = String.Format("{0}-{1}", (actualYear - 2).ToString().Substring(2, 2), (actualYear - 1).ToString().Substring(2, 2));
                ViewBag.FourthYear = String.Format("{0}-{1}", (actualYear - 3).ToString().Substring(2, 2), (actualYear - 2).ToString().Substring(2, 2));
                ViewBag.FifthYear = String.Format("{0}-{1}", (actualYear - 4).ToString().Substring(2, 2), (actualYear - 3).ToString().Substring(2, 2));
                ViewBag.SixYear = String.Format("{0}-{1}", (actualYear - 5).ToString().Substring(2, 2), (actualYear - 4).ToString().Substring(2, 2));

                int AY1 = jntuh_academic_year.Where(a => a.actualYear == (actualYear)).Select(a => a.id).FirstOrDefault();
                int AY2 = jntuh_academic_year.Where(a => a.actualYear == (actualYear - 1)).Select(a => a.id).FirstOrDefault();
                int AY3 = jntuh_academic_year.Where(a => a.actualYear == (actualYear - 2)).Select(a => a.id).FirstOrDefault();
                int AY4 = jntuh_academic_year.Where(a => a.actualYear == (actualYear - 3)).Select(a => a.id).FirstOrDefault();
                int AY5 = jntuh_academic_year.Where(a => a.actualYear == (actualYear - 4)).Select(a => a.id).FirstOrDefault();
                int AY6 = jntuh_academic_year.Where(a => a.actualYear == (actualYear - 5)).Select(a => a.id).FirstOrDefault();

                List<int> YearIds = new List<int>();
                if (obj.FirstYear == true)
                    YearIds.Add(AY1);
                if (obj.SecondYear == true)
                    YearIds.Add(AY2);
                if (obj.ThirdYear == true)
                    YearIds.Add(AY3);
                if (obj.FourYear == true)
                    YearIds.Add(AY4);
                if (obj.FifthYear == true)
                    YearIds.Add(AY5);
                if (obj.SixYear == true)
                    YearIds.Add(AY6);

                List<jntuh_college_intake_existing> intake = new List<jntuh_college_intake_existing>();
                var ActiveLoginCollegeIds = jntuh_college.Where(a => a.isActive == true).Select(z => z.id).ToList();
                var SubmittedCollegeIds = db.jntuh_college_edit_status.Where(a =>a.academicyearId == AY1 &&  a.isSubmitted == true).Select(z => z.collegeId).Distinct().ToList();
                if (obj.CollegeId == 0)
                {
                    intake = db.jntuh_college_intake_existing.Where(i => SubmittedCollegeIds.Contains(i.collegeId) && YearIds.Contains(i.academicYearId)).ToList();
                    ExcelFileName = "Submission Colleges";
                }
                else if (obj.CollegeId == -1)
                {
                    intake = db.jntuh_college_intake_existing.Where(i => ActiveLoginCollegeIds.Contains(i.collegeId) && YearIds.Contains(i.academicYearId)).ToList();
                    ExcelFileName = "Active Login Colleges";
                }
                else
                {
                    intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == obj.CollegeId && YearIds.Contains(i.academicYearId)).ToList();
                    ExcelFileName = jntuh_college.Where(a => a.id == obj.CollegeId).Select(z => z.collegeName + "(" + z.collegeCode + ")").FirstOrDefault();
                }

                foreach (var item in intake)
                {
                    CollegeIntakeExisting newIntake = new CollegeIntakeExisting();
                    newIntake.id = item.id;
                    newIntake.collegeId = item.collegeId;
                    newIntake.academicYearId = item.academicYearId;
                    newIntake.isActive = item.isActive;
                    newIntake.specializationId = item.specializationId;
                    newIntake.Specialization = item.jntuh_specialization.specializationName;
                    newIntake.DepartmentID = item.jntuh_specialization.jntuh_department.id;
                    newIntake.Department = item.jntuh_specialization.jntuh_department.departmentName;
                    newIntake.degreeID = item.jntuh_specialization.jntuh_department.jntuh_degree.id;
                    newIntake.Degree = item.jntuh_specialization.jntuh_department.jntuh_degree.degree;
                    newIntake.degreeDisplayOrder = item.jntuh_specialization.jntuh_department.jntuh_degree.degreeDisplayOrder;
                    newIntake.shiftId = item.shiftId;
                    newIntake.Shift = item.jntuh_shift.shiftName;
                    collegeIntakeExisting.Add(newIntake);
                }
                if (obj.CollegeId == 0 || obj.CollegeId == -1)
                    collegeIntakeExisting = collegeIntakeExisting.AsEnumerable().GroupBy(r => new { r.collegeId, r.specializationId, r.shiftId }).Select(r => r.First()).ToList();
                else
                    collegeIntakeExisting = collegeIntakeExisting.AsEnumerable().GroupBy(r => new { r.specializationId, r.shiftId }).Select(r => r.First()).ToList();

                foreach (var item2 in collegeIntakeExisting)
                {
                    ApprovedAdmittedIntake IntakeDetails = new ApprovedAdmittedIntake();
                    IntakeDetails.CollegeId = item2.collegeId;
                    IntakeDetails.CollegeCode = jntuh_college.Where(a => a.id == item2.collegeId).Select(a => a.collegeCode).FirstOrDefault();
                    IntakeDetails.CollegeName = jntuh_college.Where(a => a.id == item2.collegeId).Select(a => a.collegeName).FirstOrDefault();
                    IntakeDetails.Degree = item2.Degree;
                    IntakeDetails.Department = item2.Department;
                    IntakeDetails.Specialization = item2.Specialization;
                    IntakeDetails.ShiftId = item2.shiftId;
                    IntakeDetails.PresentYear = obj.PresentYear;
                    IntakeDetails.FirstYear = obj.FirstYear;
                    IntakeDetails.SecondYear = obj.SecondYear;
                    IntakeDetails.ThirdYear = obj.ThirdYear;
                    IntakeDetails.FourYear = obj.FourYear;
                    IntakeDetails.FifthYear = obj.FifthYear;
                    IntakeDetails.SixYear = obj.SixYear;

                    IntakeDetails.AICTEApprovedIntake = obj.AICTEApprovedIntake;
                    IntakeDetails.JntuApprovedIntake = obj.JntuApprovedIntake;
                    IntakeDetails.CollegeAdmittedIntake = obj.CollegeAdmittedIntake;
                    IntakeDetails.ExamsAdmittedIntakeR = obj.ExamsAdmittedIntakeR;
                    IntakeDetails.ExamsAdmittedIntakeL = obj.ExamsAdmittedIntakeL;

                    IntakeDetails.ProposedIntake = GetIntake(item2.collegeId, presentYear, item2.specializationId, item2.shiftId, 0);
                    IntakeDetails.PresentYearAICTEApproved = Convert.ToInt32(db.jntuh_college_intake_existing.Where(i => i.collegeId == item2.collegeId && i.academicYearId == presentYear && i.specializationId == item2.specializationId && i.shiftId == item2.shiftId).Select(i => i.aicteApprovedIntake).FirstOrDefault());
                    IntakeDetails.PresentYearJntuApproved = Convert.ToInt32(db.jntuh_college_intake_existing.Where(i => i.collegeId == item2.collegeId && i.academicYearId == presentYear && i.specializationId == item2.specializationId && i.shiftId == item2.shiftId).Select(i => i.approvedIntake).FirstOrDefault());
                    IntakeDetails.PresentYearExamsBranchRegularAdmitted = Convert.ToInt32(db.jntuh_college_intake_existing.Where(i => i.collegeId == item2.collegeId && i.academicYearId == presentYear && i.specializationId == item2.specializationId && i.shiftId == item2.shiftId).Select(i => i.admittedIntakeasperExambranch_R).FirstOrDefault());
                    IntakeDetails.PresentYearExamsBranchLateralAdmitted = Convert.ToInt32(db.jntuh_college_intake_existing.Where(i => i.collegeId == item2.collegeId && i.academicYearId == presentYear && i.specializationId == item2.specializationId && i.shiftId == item2.shiftId).Select(i => i.admittedIntakeasperExambranch_L).FirstOrDefault());
                    IntakeDetails.PresentYearCollegeAdmitted = Convert.ToInt32(db.jntuh_college_intake_existing.Where(i => i.collegeId == item2.collegeId && i.academicYearId == presentYear && i.specializationId == item2.specializationId && i.shiftId == item2.shiftId).Select(i => i.admittedIntake).FirstOrDefault());

                    if (obj.AICTEApprovedIntake == true)
                    {
                        IntakeDetails.FirstYearAICTEApproved = GetIntake(item2.collegeId, AY1, item2.specializationId, item2.shiftId, 1);
                        IntakeDetails.SecondYearAICTEApproved = GetIntake(item2.collegeId, AY2, item2.specializationId, item2.shiftId, 1);
                        IntakeDetails.ThirdYearAICTEApproved = GetIntake(item2.collegeId, AY3, item2.specializationId, item2.shiftId, 1);
                        IntakeDetails.FourYearAICTEApproved = GetIntake(item2.collegeId, AY4, item2.specializationId, item2.shiftId, 1);
                        IntakeDetails.FifthYearAICTEApproved = GetIntake(item2.collegeId, AY5, item2.specializationId, item2.shiftId, 1);
                        IntakeDetails.SixYearAICTEApproved = GetIntake(item2.collegeId, AY6, item2.specializationId, item2.shiftId, 1);
                    }

                    if (obj.JntuApprovedIntake == true)
                    {
                        IntakeDetails.FirstYearJntuApproved = GetIntake(item2.collegeId, AY1, item2.specializationId, item2.shiftId, 2);
                        IntakeDetails.SecondYearJntuApproved = GetIntake(item2.collegeId, AY2, item2.specializationId, item2.shiftId, 2);
                        IntakeDetails.ThirdYearJntuApproved = GetIntake(item2.collegeId, AY3, item2.specializationId, item2.shiftId, 2);
                        IntakeDetails.FourYearJntuApproved = GetIntake(item2.collegeId, AY4, item2.specializationId, item2.shiftId, 2);
                        IntakeDetails.FifthYearJntuApproved = GetIntake(item2.collegeId, AY5, item2.specializationId, item2.shiftId, 2);
                        IntakeDetails.SixYearJntuApproved = GetIntake(item2.collegeId, AY6, item2.specializationId, item2.shiftId, 2);
                    }

                    if (obj.ExamsAdmittedIntakeR == true)
                    {
                        IntakeDetails.FirstYearExamsBranchRegularAdmitted = GetIntake(item2.collegeId, AY1, item2.specializationId, item2.shiftId, 3);
                        IntakeDetails.SecondYearExamsBranchRegularAdmitted = GetIntake(item2.collegeId, AY2, item2.specializationId, item2.shiftId, 3);
                        IntakeDetails.ThirdYearExamsBranchRegularAdmitted = GetIntake(item2.collegeId, AY3, item2.specializationId, item2.shiftId, 3);
                        IntakeDetails.FourYearExamsBranchRegularAdmitted = GetIntake(item2.collegeId, AY4, item2.specializationId, item2.shiftId, 3);
                        IntakeDetails.FifthYearExamsBranchRegularAdmitted = GetIntake(item2.collegeId, AY5, item2.specializationId, item2.shiftId, 3);
                        IntakeDetails.SixYearExamsBranchRegularAdmitted = GetIntake(item2.collegeId, AY6, item2.specializationId, item2.shiftId, 3);
                    }

                    if (obj.ExamsAdmittedIntakeL == true)
                    {
                        IntakeDetails.FirstYearExamsBranchLateralAdmitted = GetIntake(item2.collegeId, AY1, item2.specializationId, item2.shiftId, 4);
                        IntakeDetails.SecondYearExamsBranchLateralAdmitted = GetIntake(item2.collegeId, AY2, item2.specializationId, item2.shiftId, 4);
                        IntakeDetails.ThirdYearExamsBranchLateralAdmitted = GetIntake(item2.collegeId, AY3, item2.specializationId, item2.shiftId, 4);
                        IntakeDetails.FourYearExamsBranchLateralAdmitted = GetIntake(item2.collegeId, AY4, item2.specializationId, item2.shiftId, 4);
                        IntakeDetails.FifthYearExamsBranchLateralAdmitted = GetIntake(item2.collegeId, AY5, item2.specializationId, item2.shiftId, 4);
                        IntakeDetails.SixYearExamsBranchLateralAdmitted = GetIntake(item2.collegeId, AY6, item2.specializationId, item2.shiftId, 4);
                    }

                    if (obj.CollegeAdmittedIntake == true)
                    {
                        IntakeDetails.FirstYearCollegeAdmitted = GetIntake(item2.collegeId, AY1, item2.specializationId, item2.shiftId, 5);
                        IntakeDetails.SecondYearCollegeAdmitted = GetIntake(item2.collegeId, AY2, item2.specializationId, item2.shiftId, 5);
                        IntakeDetails.ThirdYearCollegeAdmitted = GetIntake(item2.collegeId, AY3, item2.specializationId, item2.shiftId, 5);
                        IntakeDetails.FourYearCollegeAdmitted = GetIntake(item2.collegeId, AY4, item2.specializationId, item2.shiftId, 5);
                        IntakeDetails.FifthYearCollegeAdmitted = GetIntake(item2.collegeId, AY5, item2.specializationId, item2.shiftId, 5);
                        IntakeDetails.SixYearCollegeAdmitted = GetIntake(item2.collegeId, AY6, item2.specializationId, item2.shiftId, 5);
                    }

                    IntakeList.Add(IntakeDetails);
                }

                if (command == "Export")
                {
                    Response.ClearContent();
                    Response.Buffer = true;
                    Response.AddHeader("content-disposition", "attachment; filename=" + ExcelFileName + "_Intake.XLS");
                    Response.ContentType = "application/vnd.ms-excel";
                    return PartialView("~/Views/DatabaseReports/_DownloadApprovedIntakeReport.cshtml", IntakeList.OrderBy(s => s.Degree).ThenBy(s => s.Specialization).ThenBy(s => s.ShiftId).ToList());
                }
            }
            return View();

        }

        private int GetIntake(int collegeId, int academicYearId, int specializationId, int shiftId, int flag)
        {
            int intake = 0;

            if (flag == 0) //Proposed Intake
            {
                intake = Convert.ToInt32(db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId).Select(i => i.proposedIntake).FirstOrDefault());
            }
            else if (flag == 1) //AICTE approved Intake
            {
                intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId).Select(i => i.aicteApprovedIntake).FirstOrDefault();
            }
            else if (flag == 2)//Jntu approved Intake
            {
                intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId).Select(i => i.approvedIntake).FirstOrDefault();
            }
            else if (flag == 3) //Exam Branch Admitted Regular
            {
                intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId).Select(i => i.admittedIntakeasperExambranch_R).FirstOrDefault();
            }
            else if (flag == 4) //Exam Branch Admitted Lateral
            {
                intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId).Select(i => i.admittedIntakeasperExambranch_L).FirstOrDefault();
            }
            else //College Admitted Intake
            {
                intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId).Select(i => i.admittedIntake).FirstOrDefault();
            }
            return intake;
        }

        [Authorize(Roles = "Admin")]
        public ActionResult FlagsFaculty(int? collegeId)
        {
            FacultyFlags Faculty = new FacultyFlags();
            var CollegesList = db.jntuh_college.Where(c => c.isActive == true && c.id != 375).Select(c => new { collegeId = c.id, collegeName = c.collegeCode + "-" + c.collegeName }).OrderBy(c => c.collegeName).ToList();
            CollegesList.Add(new { collegeId = -4, collegeName = "Engineering Colleges" });
            CollegesList.Add(new { collegeId = -3, collegeName = "Pharmacy Colleges" });
            CollegesList.Add(new { collegeId = -2, collegeName = "Total College Faculty" });
            CollegesList.Add(new { collegeId = -1, collegeName = "Active Login Colleges" });
            CollegesList.Add(new { collegeId = 0, collegeName = "All Submission Colleges" });
            ViewBag.Colleges = CollegesList.OrderBy(z => z.collegeId).ToList();
            Faculty.collegeId = collegeId;
            return View(Faculty);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult FlagsFaculty(FacultyFlags facultyflags)
        {
            if (facultyflags.collegeId != null)
            {
                List<FlagsNames> AllFaculty = new List<FlagsNames>();
                var jntuh_college = db.jntuh_college.Select(a => a).ToList();
                var actualYear = db.jntuh_academic_year.Where(e => e.isPresentAcademicYear == true && e.isActive == true).Select(e => e.actualYear).FirstOrDefault();
                int AY1 = db.jntuh_academic_year.Where(e => e.actualYear == (actualYear + 1)).Select(e => e.id).FirstOrDefault();
                var ActiveLoginCollegeIds = jntuh_college.Where(a => a.isActive == true).Select(z => z.id).ToList();
                var SubmittedCollegeIds = db.jntuh_college_edit_status.Where(a => a.academicyearId == AY1 && a.isSubmitted == true).Select(z => z.collegeId).ToList();
                var EngineeringCollegeIds = jntuh_college.Where(a => a.isActive == true && a.collegeTypeID == 1).Select(z => z.id).ToList();
                var PharmacyCollegeIds = jntuh_college.Where(a => a.isActive == true && a.collegeTypeID == 2).Select(z => z.id).ToList();
                var Single_jntuh_college = jntuh_college.Where(q => q.id == facultyflags.collegeId).Select(a => a).FirstOrDefault();

                var jntuh_Designation = db.jntuh_designation.ToList();
                var jntuh_department = db.jntuh_department.ToList();
                var jntuh_specialization = db.jntuh_specialization.ToList();

                List<jntuh_college_faculty_registered> CollegeFaculty = new List<jntuh_college_faculty_registered>();
                List<jntuh_college_principal_registered> CollegePrincipal = new List<jntuh_college_principal_registered>();

                if (facultyflags.collegeId == 0)
                {
                    CollegeFaculty = db.jntuh_college_faculty_registered.Where(w => SubmittedCollegeIds.Contains(w.collegeId)).Select(q => q).ToList();
                    CollegePrincipal = db.jntuh_college_principal_registered.Where(q => SubmittedCollegeIds.Contains(q.collegeId)).Select(w => w).ToList();
                }
                else if (facultyflags.collegeId == -1)
                {
                    CollegeFaculty = db.jntuh_college_faculty_registered.Where(w => ActiveLoginCollegeIds.Contains(w.collegeId)).Select(q => q).ToList();
                    CollegePrincipal = db.jntuh_college_principal_registered.Where(q => ActiveLoginCollegeIds.Contains(q.collegeId)).Select(w => w).ToList();
                }
                else if (facultyflags.collegeId == -2)
                {
                    CollegeFaculty = db.jntuh_college_faculty_registered.Select(q => q).ToList();
                    CollegePrincipal = db.jntuh_college_principal_registered.Select(w => w).ToList();
                }
                else if (facultyflags.collegeId == -3)
                {
                    var submissionIds = db.jntuh_college_edit_status.Where(q => q.academicyearId == AY1 && PharmacyCollegeIds.Contains(q.collegeId) && q.isSubmitted == true).Select(a => a.collegeId).ToList();
                    CollegeFaculty = db.jntuh_college_faculty_registered.Where(w => submissionIds.Contains(w.collegeId)).Select(q => q).ToList();
                    CollegePrincipal = db.jntuh_college_principal_registered.Where(q => submissionIds.Contains(q.collegeId)).Select(w => w).ToList();
                }
                else if (facultyflags.collegeId == -4)
                {
                    var submissionIds = db.jntuh_college_edit_status.Where(q => q.academicyearId == AY1 && EngineeringCollegeIds.Contains(q.collegeId) && q.isSubmitted == true).Select(a => a.collegeId).ToList();
                    CollegeFaculty = db.jntuh_college_faculty_registered.Where(w => submissionIds.Contains(w.collegeId)).Select(q => q).ToList();
                    CollegePrincipal = db.jntuh_college_principal_registered.Where(q => submissionIds.Contains(q.collegeId)).Select(w => w).ToList();
                }
                else
                {
                    CollegeFaculty = db.jntuh_college_faculty_registered.Where(w => w.collegeId == facultyflags.collegeId).Select(q => q).ToList();
                    CollegePrincipal = db.jntuh_college_principal_registered.Where(q => q.collegeId == facultyflags.collegeId).Select(w => w).ToList();
                }

                var RegNos = CollegeFaculty.Select(a => a.RegistrationNumber).ToList();
                List<FacultyFlags> FacultyList = new List<FacultyFlags>();
                string constr = "Data Source=10.10.10.16;user id=root;password=Jntu@123!@#123;database=uaaas207118;";
                MySqlConnection con = new MySqlConnection(constr);
                string query = "";

                if (facultyflags.type == true)
                {
                    if (query != "")
                        query += " OR f.type='Adjunct'";
                    else
                        query += "SELECT * FROM jntuh_registered_faculty f where f.type='Adjunct'";
                }
                if (facultyflags.absent == true)
                {
                    if (query != "")
                        query += " OR f.Absent=true";
                    else
                        query += "SELECT * FROM jntuh_registered_faculty f where f.Absent=true";
                }
                if (facultyflags.OriginalCertificatesNotShown == true)
                {
                    if (query != "")
                        query += " OR f.OriginalCertificatesNotShown=true";
                    else
                        query += "SELECT * FROM jntuh_registered_faculty f where f.OriginalCertificatesNotShown=true";
                }
                if (facultyflags.Xeroxcopyofcertificates == true)
                {
                    if (query != "")
                        query += " OR f.Xeroxcopyofcertificates=true";
                    else
                        query += "SELECT * FROM jntuh_registered_faculty f where f.Xeroxcopyofcertificates=true";
                }
                if (facultyflags.NotQualifiedAsperAICTE == true)
                {
                    if (query != "")
                        query += " OR f.NotQualifiedAsperAICTE=true";
                    else
                        query += "SELECT * FROM jntuh_registered_faculty f where f.NotQualifiedAsperAICTE=true";
                }
                if (facultyflags.NoSCM == true)
                {
                    if (query != "")
                        query += " OR f.NoSCM=true";
                    else
                        query += "SELECT * FROM jntuh_registered_faculty f where f.NoSCM=true";
                }
                if (facultyflags.PANNumber == true)
                {
                    if (query != "")
                        query += " OR f.PANNumber IS NULL";
                    else
                        query += "SELECT * FROM jntuh_registered_faculty f where f.PANNumber IS NULL";
                }
                if (facultyflags.IncompleteCertificates == true)
                {
                    if (query != "")
                        query += " OR f.IncompleteCertificates=true";
                    else
                        query += "SELECT * FROM jntuh_registered_faculty f where f.IncompleteCertificates=true";
                }
                if (facultyflags.NoRelevantUG == true)
                {
                    if (query != "")
                        query += " OR f.NoRelevantUG='Yes'";
                    else
                        query += "SELECT * FROM jntuh_registered_faculty f where f.NoRelevantUG='Yes'";
                }
                if (facultyflags.NoRelevantPG == true)
                {
                    if (query != "")
                        query += " OR f.NoRelevantPG='Yes'";
                    else
                        query += "SELECT * FROM jntuh_registered_faculty f where f.NoRelevantPG='Yes'";
                }
                if (facultyflags.NORelevantPHD == true)
                {
                    if (query != "")
                        query += "OR f.NORelevantPHD='Yes'";
                    else
                        query += "SELECT * FROM jntuh_registered_faculty f where f.NORelevantPHD='Yes'";
                }
                if (facultyflags.InvalidPANNumber == true)
                {
                    if (query != "")
                        query += " OR f.InvalidPANNumber=true";
                    else
                        query += "SELECT * FROM jntuh_registered_faculty f where f.InvalidPANNumber=true";
                }
                if (facultyflags.OriginalsVerifiedPHD == true)
                {
                    if (query != "")
                        query += " OR f.OriginalsVerifiedPHD=true";
                    else
                        query += "SELECT * FROM jntuh_registered_faculty f where f.OriginalsVerifiedPHD=true";
                }
                if (facultyflags.OriginalsVerifiedUG == true)
                {
                    if (query != "")
                        query += " OR f.OriginalsVerifiedUG=true";
                    else
                        query += "SELECT * FROM jntuh_registered_faculty f where f.OriginalsVerifiedUG=true";
                }
                if (facultyflags.InvalidAadhaar == true)
                {
                    if (query != "")
                        query += " OR f.InvalidAadhaar='Yes'";
                    else
                        query += "SELECT * FROM jntuh_registered_faculty f where f.InvalidAadhaar='Yes'";
                }
                if (facultyflags.BAS == true)
                {
                    if (query != "")
                        query += " OR f.BAS='Yes'";
                    else
                        query += "SELECT * FROM jntuh_registered_faculty f where f.BAS='Yes'";
                }
                if (facultyflags.Blacklistfaculy == true)
                {
                    if (query != "")
                        query += " OR f.Blacklistfaculy=true";
                    else
                        query += "SELECT * FROM jntuh_registered_faculty f where f.Blacklistfaculy=true";
                }
                if (facultyflags.AbsentforVerification == true)
                {
                    if (query != "")
                        query += " OR f.AbsentforVerification=true";
                    else
                        query += "SELECT * FROM jntuh_registered_faculty f where f.AbsentforVerification=true";
                }
                if (facultyflags.Invaliddegree == true)
                {
                    if (query != "")
                        query += " OR f.Invaliddegree=true";
                    else
                        query += "SELECT * FROM jntuh_registered_faculty f where f.Invaliddegree=true";
                }
                if (facultyflags.Noclass == true)
                {
                    if (query != "")
                        query += " OR f.NoClass=true";
                    else
                        query += "SELECT * FROM jntuh_registered_faculty f where f.NoClass=true";
                }
                if (facultyflags.FakePhD == true)
                {
                    if (query != "")
                        query += " OR f.FakePHD=true";
                    else
                        query += "SELECT * FROM jntuh_registered_faculty f where f.FakePHD=true";
                }
                if (facultyflags.Genuinenessnotsubmitted == true)
                {
                    if (query != "")
                        query += " OR f.Genuinenessnotsubmitted=true";
                    else
                        query += "SELECT * FROM jntuh_registered_faculty f where f.Genuinenessnotsubmitted=true";
                }
                if (facultyflags.NotConsideredPhD == true)
                {
                    if (query != "")
                        query += " OR f.NotconsideredPHD=true";
                    else
                        query += "SELECT * FROM jntuh_registered_faculty f where f.NotconsideredPHD=true";
                }
                if (facultyflags.NoPGspecialization == true)
                {
                    if (query != "")
                        query += " OR f.NoPGspecialization=true";
                    else
                        query += "SELECT * FROM jntuh_registered_faculty f where f.NoPGspecialization=true";
                }
                if (facultyflags.NoForm16 == true)
                {
                    if (query != "")
                        query += " OR f.NoForm16=true";
                    else
                        query += "SELECT * FROM jntuh_registered_faculty f where f.NoForm16=true";
                }
                if (facultyflags.NoForm26AS == true)
                {
                    if (query != "")
                        query += " OR f.NoForm26AS=true";
                    else
                        query += "SELECT * FROM jntuh_registered_faculty f where f.NoForm26AS=true";
                }
                if (facultyflags.Covid19 == true)
                {
                    if (query != "")
                        query += " OR f.Covid19=true";
                    else
                        query += "SELECT * FROM jntuh_registered_faculty f where f.Covid19=true";
                }
                if (facultyflags.Maternity == true)
                {
                    if (query != "")
                        query += " OR f.Maternity=true";
                    else
                        query += "SELECT * FROM jntuh_registered_faculty f where f.Maternity=true";
                }
                con.Open();
                MySqlCommand cmd = new MySqlCommand(query, con);
                MySqlDataReader dr = cmd.ExecuteReader();
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {
                        FlagsNames facultyData = new FlagsNames();

                        var FirstName = dr["FirstName"].ToString();
                        var MiddleName = dr["MiddleName"].ToString();
                        var LastName = dr["LastName"].ToString();
                        facultyData.Name = FirstName + " " + MiddleName + " " + LastName;
                        facultyData.RegistrationNumber = dr["RegistrationNumber"].ToString();
                        facultyData.collegeId = CollegeFaculty.Where(a => a.RegistrationNumber == facultyData.RegistrationNumber).Select(z => z.collegeId).FirstOrDefault() == 0 ?
                            CollegePrincipal.Where(a => a.RegistrationNumber == facultyData.RegistrationNumber).Select(z => z.collegeId).FirstOrDefault() : CollegeFaculty.Where(a => a.RegistrationNumber == facultyData.RegistrationNumber).Select(z => z.collegeId).FirstOrDefault();

                        facultyData.collegeCode = jntuh_college.Where(a => a.id == facultyData.collegeId).Select(a => a.collegeCode).FirstOrDefault();
                        facultyData.collegeName = jntuh_college.Where(a => a.id == facultyData.collegeId).Select(a => a.collegeName).FirstOrDefault();
                        facultyData.typeFlag = dr["type"].ToString();
                        facultyData.absentFlag = String.IsNullOrEmpty(dr["Absent"].ToString()) ? "No" : Convert.ToBoolean(dr["Absent"]) == true ? "Yes" : "No";
                        facultyData.OriginalCertificatesNotShownFlag = String.IsNullOrEmpty(dr["OriginalCertificatesNotShown"].ToString()) ? "No" : Convert.ToBoolean(dr["OriginalCertificatesNotShown"]) == true ? "Yes" : "No";
                        facultyData.XeroxcopyofcertificatesFlag = String.IsNullOrEmpty(dr["Xeroxcopyofcertificates"].ToString()) ? "No" : Convert.ToBoolean(dr["Xeroxcopyofcertificates"]) == true ? "Yes" : "No";
                        facultyData.NotQualifiedAsperAICTEFlag = String.IsNullOrEmpty(dr["NotQualifiedAsperAICTE"].ToString()) ? "No" : Convert.ToBoolean(dr["NotQualifiedAsperAICTE"]) == true ? "Yes" : "No";
                        facultyData.NoSCMFlag = String.IsNullOrEmpty(dr["NoSCM"].ToString()) ? "No" : Convert.ToBoolean(dr["NoSCM"]) == true ? "Yes" : "No";
                        facultyData.PANNumberFlag = String.IsNullOrEmpty(dr["PANNumber"].ToString()) ? "No" : String.IsNullOrEmpty(dr["PANNumber"].ToString()) ? "Yes" : "No";
                        facultyData.IncompleteCertificatesFlag = String.IsNullOrEmpty(dr["IncompleteCertificates"].ToString()) ? "No" : Convert.ToBoolean(dr["IncompleteCertificates"]) == true ? "Yes" : "No";
                        facultyData.NoRelevantUGFlag = String.IsNullOrEmpty(dr["NoRelevantUG"].ToString()) ? "No" : dr["NoRelevantUG"].ToString() == "Yes" ? "Yes" : "No";
                        facultyData.NoRelevantPGFlag = String.IsNullOrEmpty(dr["NoRelevantPG"].ToString()) ? "No" : dr["NoRelevantPG"].ToString() == "Yes" ? "Yes" : "No";
                        facultyData.NORelevantPHDFlag = String.IsNullOrEmpty(dr["NoRelevantPHD"].ToString()) ? "No" : dr["NoRelevantPHD"].ToString() == "Yes" ? "Yes" : "No";
                        facultyData.InvalidPANNumberFlag = String.IsNullOrEmpty(dr["InvalidPANNumber"].ToString()) ? "No" : Convert.ToBoolean(dr["InvalidPANNumber"]) == true ? "Yes" : "No";
                        facultyData.OriginalsVerifiedPHDFlag = String.IsNullOrEmpty(dr["OriginalsVerifiedPHD"].ToString()) ? "No" : Convert.ToBoolean(dr["OriginalsVerifiedPHD"]) == true ? "Yes" : "No";
                        facultyData.OriginalsVerifiedUGFlag = String.IsNullOrEmpty(dr["OriginalsVerifiedUG"].ToString()) ? "No" : Convert.ToBoolean(dr["OriginalsVerifiedUG"]) == true ? "Yes" : "No";
                        facultyData.InvaliddegreeFlag = String.IsNullOrEmpty(dr["Invaliddegree"].ToString()) ? "No" : Convert.ToBoolean(dr["Invaliddegree"]) == true ? "Yes" : "No";
                        facultyData.InvalidAadhaarFlag = String.IsNullOrEmpty(dr["InvalidAadhaar"].ToString()) ? "No" : dr["InvalidAadhaar"].ToString() == "Yes" ? "Yes" : "No";
                        facultyData.BASFlag = String.IsNullOrEmpty(dr["BAS"].ToString()) ? "No" : dr["BAS"].ToString() == "Yes" ? "Yes" : "No";
                        facultyData.BlacklistfaculyFlag = String.IsNullOrEmpty(dr["Blacklistfaculy"].ToString()) ? "No" : Convert.ToBoolean(dr["Blacklistfaculy"]) == true ? "Yes" : "No";
                        facultyData.AbsentforVerificationFlag = String.IsNullOrEmpty(dr["AbsentforVerification"].ToString()) ? "No" : Convert.ToBoolean(dr["AbsentforVerification"]) == true ? "Yes" : "No";
                        facultyData.FakePhDFlag = String.IsNullOrEmpty(dr["FakePHD"].ToString()) ? "No" : Convert.ToBoolean(dr["FakePHD"]) == true ? "Yes" : "No";
                        facultyData.NoclassFlag = String.IsNullOrEmpty(dr["NoClass"].ToString()) ? "No" : Convert.ToBoolean(dr["NoClass"]) == true ? "Yes" : "No";
                        facultyData.GenuinenessnotsubmittedFlag = String.IsNullOrEmpty(dr["Genuinenessnotsubmitted"].ToString()) ? "No" : Convert.ToBoolean(dr["Genuinenessnotsubmitted"]) == true ? "Yes" : "No";
                        facultyData.NotConsideredPhDFlag = String.IsNullOrEmpty(dr["NotconsideredPHD"].ToString()) ? "No" : Convert.ToBoolean(dr["NotconsideredPHD"]) == true ? "Yes" : "No";
                        facultyData.NoPGspecializationFlag = String.IsNullOrEmpty(dr["NoPGspecialization"].ToString()) ? "No" : Convert.ToBoolean(dr["NoPGspecialization"]) == true ? "Yes" : "No";

                        facultyData.NoForm16 = String.IsNullOrEmpty(dr["NoForm16"].ToString()) ? "No" : Convert.ToBoolean(dr["NoForm16"]) == true ? "Yes" : "No";
                        facultyData.NoForm26AS = String.IsNullOrEmpty(dr["NoForm26AS"].ToString()) ? "No" : Convert.ToBoolean(dr["NoForm26AS"]) == true ? "Yes" : "No";
                        facultyData.Covid19 = String.IsNullOrEmpty(dr["Covid19"].ToString()) ? "No" : Convert.ToBoolean(dr["Covid19"]) == true ? "Yes" : "No";
                        facultyData.Maternity = String.IsNullOrEmpty(dr["Maternity"].ToString()) ? "No" : Convert.ToBoolean(dr["Maternity"]) == true ? "Yes" : "No";
                        AllFaculty.Add(facultyData);
                    }
                }

                AllFaculty = AllFaculty.Where(q => RegNos.Contains(q.RegistrationNumber)).OrderBy(q => q.collegeName).ToList();
                facultyflags.FacultyFlagsDetails = AllFaculty;

                Response.ClearContent();
                Response.Buffer = true;
                if (facultyflags.collegeId == 0)
                    Response.AddHeader("content-disposition", "attachment; filename=Submission Colleges Faculty Flags Data.XLS");
                else if (facultyflags.collegeId == -1)
                    Response.AddHeader("content-disposition", "attachment; filename=Login College Faculty Flags Data.XLS");
                else if (facultyflags.collegeId == -2)
                    Response.AddHeader("content-disposition", "attachment; filename=All Colleges Teaching Faculty Flags Data.XLS");
                else
                    Response.AddHeader("content-disposition", "attachment; filename=" + Single_jntuh_college.collegeName + "(" + Single_jntuh_college.collegeCode + ")_CollegeFaculty Flags Data.XLS");

                Response.ContentType = "application/vnd.ms-excel";
                return PartialView("~/Views/DatabaseReports/_DownloadFacultyFlagsExcel.cshtml", facultyflags);
            }
            return RedirectToAction("FlagsFaculty");
        }

        [Authorize(Roles = "Admin")]
        public ActionResult CopyFile()
        {
            CopyFileClass copy = new CopyFileClass();
            return View(copy);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult CopyFile(CopyFileClass faculty)
        {
            if (faculty.RegNosExcel != null && !string.IsNullOrEmpty(faculty.ExcelFileFolderpath) && !string.IsNullOrEmpty(faculty.FilesCopiedpath))
            {
                string excelpath = "~/Content/Upload/LettersUploadfromAdmin";
                if (!Directory.Exists(Server.MapPath(excelpath)))
                    Directory.CreateDirectory(Server.MapPath(excelpath));

                var ext = Path.GetExtension(faculty.RegNosExcel.FileName);
                if (ext.ToUpper().Equals(".XLS") || ext.ToUpper().Equals(".XLSX"))
                {
                    FileInfo fs = new FileInfo(Path.Combine(Server.MapPath(excelpath), faculty.RegNosExcel.FileName));
                    if (fs.Exists)
                    {
                        fs.Delete();
                    }
                    faculty.RegNosExcel.SaveAs(string.Format("{0}/{1}", Server.MapPath(excelpath), faculty.RegNosExcel.FileName));
                }


                //string excelConnectionString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + Path.Combine(Server.MapPath(excelpath), faculty.RegNosExcel.FileName) + ";Extended Properties=Excel 12.0;Persist Security Info=False";
                ////Create Connection to Excel work book                   
                //OleDbConnection excelConnection = new OleDbConnection(excelConnectionString);
                //excelConnection.Close();
                ////Create OleDbCommand to fetch data from Excel
                //excelConnection.Open();
                //OleDbCommand cmd1 = new OleDbCommand("select count(*) from [" + excelConnection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null).Rows[0]["Table_Name"].ToString() + "]", excelConnection);
                //var rows = (int)cmd1.ExecuteScalar();
                //OleDbCommand cmd2 = new OleDbCommand("select * from [" + excelConnection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null).Rows[0]["Table_Name"].ToString() + "]", excelConnection);

                //DataTable dt = new DataTable();
                //OleDbDataAdapter oleda = new OleDbDataAdapter();
                //oleda.SelectCommand = cmd2;
                //DataSet ds = new DataSet();
                //oleda.Fill(ds);
                //dt = ds.Tables[0];
                //List<string> FilesPathList = new List<string>();
                //List<string> FailureRegNos = new List<string>();
                //for (int i = 0; i < dt.Rows.Count; i++)
                //{
                //    try
                //    {
                //        if (!string.IsNullOrEmpty(dt.Rows[i][1].ToString()))
                //        {
                //            FilesPathList.Add(dt.Rows[i][1].ToString().Trim());
                //            continue;
                //        }
                //    }
                //    catch (Exception)
                //    {
                //        FailureRegNos.Add(dt.Rows[i][0].ToString().Trim());
                //        continue;
                //    }
                //}

                //excelConnection.Close();


                #region Excel File Code
                List<string> PathsList = new List<string>();
                List<string> FailurePathsList = new List<string>();
                string extension = Path.GetExtension(faculty.RegNosExcel.FileName);
                string conString = string.Empty;
                switch (extension)
                {
                    case ".xls":
                        conString = ConfigurationManager.ConnectionStrings["Excel03ConString"].ConnectionString;
                        break;
                    case ".xlsx":
                        conString = ConfigurationManager.ConnectionStrings["Excel07ConString"].ConnectionString;
                        break;
                }
                var filePath = Path.Combine(Server.MapPath(excelpath), faculty.RegNosExcel.FileName);
                conString = string.Format(conString, filePath);
                using (OleDbConnection connection = new OleDbConnection(conString))
                {
                    connection.Open();
                    OleDbCommand command = new OleDbCommand("select * from [Sheet1$]", connection);
                    using (OleDbDataReader dr = command.ExecuteReader())
                    {
                        if (dr.HasRows)
                        {
                            while (dr.Read())
                            {
                                try
                                {
                                    if (!string.IsNullOrEmpty(dr[1].ToString()))
                                    {
                                        var row1Col0 = dr[1];
                                        PathsList.Add(row1Col0.ToString());
                                        continue;
                                    }
                                }
                                catch (Exception)
                                {
                                    FailurePathsList.Add(dr[1].ToString().Trim());
                                    continue;
                                }
                            }
                        }
                    }
                }

                string IPAddress = GetIPAddress();
                string FileCopiedPath = "~/" + faculty.FilesCopiedpath;
                if (!Directory.Exists(Server.MapPath(FileCopiedPath)))
                    Directory.CreateDirectory(Server.MapPath(FileCopiedPath));
                foreach (var item in PathsList)
                {
                    var local = @"\\" + IPAddress + faculty.ExcelFileFolderpath + item;
                    //var local = faculty.ExcelFileFolderpath + @"\" + item;
                    FileInfo f = new FileInfo(local);
                    if (f.Exists)
                    {                        
                        string FileCopiedPathwithFilename = FileCopiedPath + "/" + item;
                        using (System.IO.File.Create(Server.MapPath(FileCopiedPathwithFilename))) ;
                        byte[] bytes = System.IO.File.ReadAllBytes(local);
                        System.IO.File.WriteAllBytes(Server.MapPath(FileCopiedPathwithFilename), bytes);
                    }
                }
                TempData["Success"] = "Files Copied Process is Completed.";
                #endregion
            }
            TempData["Error"] = "Some thing Went Wrong.Please Try Again.";
            return View();
        }

        public string GetIPAddress()
        {
            IPHostEntry Host = default(IPHostEntry);
            string Hostname = null;
            Hostname = System.Environment.MachineName;
            Host = Dns.GetHostEntry(Hostname);
            string IPAddress = string.Empty;
            foreach (var IP in Host.AddressList)
            {
                if (IP.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    IPAddress = Convert.ToString(IP);
                }
            }
            return IPAddress;
        }
    }


    public class CopyFileClass
    {
        public string ExcelFileFolderpath { get; set; }
        public string FilesCopiedpath { get; set; }
        public HttpPostedFileBase RegNosExcel { get; set; }
    }
}
