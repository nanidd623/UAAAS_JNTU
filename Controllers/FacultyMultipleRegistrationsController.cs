using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Web.Security;
using System.Web.Services.Description;
using PANAPIMVC.Controllers;
using UAAAS.Models;
using UAAAS.Mailers;
using System.Threading;
using System.Text.RegularExpressions;
using System.Drawing.Imaging;
using System.ComponentModel.DataAnnotations;
using UAAAS.Models.Admin;
using iTextSharp.text.html;
using System.Configuration;
using System;
using System.IO;
using System.Data;
using System.Globalization;
using MySql.Data.MySqlClient;
using System.Collections.Generic;

namespace UAAAS.Controllers
{
    [ErrorHandling]
    public class FacultyMultipleRegistrationsController : BaseController
    {
        //
        // GET: /FacultyMultipleRegistrations/
        uaaasDBContext db = new uaaasDBContext();
        private string filePath = System.Configuration.ConfigurationManager.AppSettings["PANCertificate"];
        public ActionResult Index()
        {
            return View();
        }

        //Getting Faculty
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public ActionResult GetRegistation()
        {
            FacultyRegistration FacultyRegistration = new FacultyRegistration();
            return View(FacultyRegistration);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult GetRegistation(FacultyRegistration regno)
        {
            if (String.IsNullOrEmpty(regno.RegistrationNumber))
            {
                ViewBag.Error = "Please Enter Registration Number.";
                return View(regno);
            }
            int facultyId =
                db.jntuh_registered_faculty.Where(f => f.RegistrationNumber == regno.RegistrationNumber)
                    .Select(s => s.id)
                    .FirstOrDefault();
            if (facultyId == 0)
            {
                ViewBag.Error = "Faculty Registration Number is Not Exists.";
                return View(regno);
            }
            //int facultyId = 0;
            int newUserId = 0;
            int newfacultyId = 0;
            string FacultyData = string.Empty;
            FacultyRegistration FacultyRegistration = new FacultyRegistration();

            jntuh_registered_faculty RegisteredFaculty = db.jntuh_registered_faculty.Where(f => f.RegistrationNumber == regno.RegistrationNumber).Select(s => s).FirstOrDefault();

            FacultyRegistration.id = RegisteredFaculty.id;
            FacultyRegistration.UniqueID = RegisteredFaculty.UniqueID;
            FacultyRegistration.UserId = RegisteredFaculty.UserId;
            FacultyRegistration.Type = RegisteredFaculty.type;
            if (db.jntuh_college_faculty_registered.Where(s => s.RegistrationNumber == RegisteredFaculty.RegistrationNumber).Count() != 0)
            {
                FacultyRegistration.CollegeId = db.jntuh_college_faculty_registered.Where(s => s.RegistrationNumber == RegisteredFaculty.RegistrationNumber).Select(q => q.collegeId).FirstOrDefault();
                FacultyData = "As Faculty";
            }
            if (db.jntuh_college_principal_registered.Where(s => s.RegistrationNumber == RegisteredFaculty.RegistrationNumber).Count() != 0)
            {
                FacultyRegistration.CollegeId = db.jntuh_college_principal_registered.Where(s => s.RegistrationNumber == RegisteredFaculty.RegistrationNumber).Select(q => q.collegeId).FirstOrDefault();
                FacultyData = string.IsNullOrEmpty(FacultyData) ? "As Principal" : FacultyData + " && As Principal";
            }

            if (FacultyRegistration.CollegeId != null && FacultyRegistration.CollegeId != 0)
            {
                jntuh_college collegeData = db.jntuh_college.Find(FacultyRegistration.CollegeId);
                FacultyRegistration.CollegeCode = collegeData.collegeCode;
                FacultyRegistration.CollegeName = collegeData.collegeName;
                FacultyRegistration.Type = FacultyData;
            }
            FacultyRegistration.RegistrationNumber = RegisteredFaculty.RegistrationNumber;
            FacultyRegistration.FirstName = RegisteredFaculty.FirstName;
            FacultyRegistration.MiddleName = RegisteredFaculty.MiddleName;
            FacultyRegistration.LastName = RegisteredFaculty.LastName;
            FacultyRegistration.GenderId = RegisteredFaculty.GenderId;
            FacultyRegistration.FatherOrhusbandName = RegisteredFaculty.FatherOrHusbandName;
            FacultyRegistration.MotherName = RegisteredFaculty.MotherName;
            FacultyRegistration.DateOfBirth = RegisteredFaculty.DateOfBirth;
            FacultyRegistration.PANNumber = RegisteredFaculty.PANNumber;
            FacultyRegistration.AadhaarNumber = RegisteredFaculty.AadhaarNumber;
            FacultyRegistration.IsStatus = regno.IsStatus;
            #region OldCode
            //string constr = "Data Source=10.10.10.5;user id=verification;password=jntu@123456;database=uaaas207118;";
            //string query = "SELECT * FROM jntuh_registered_faculty WHERE RegistrationNumber='" + regno.RegistrationNumber.Trim() + "'";
            //MySqlConnection con = new MySqlConnection(constr);
            //con.Open();
            //MySqlCommand cmd = new MySqlCommand(query, con);
            ////if(con.State==true)
            ////con
            //MySqlDataReader dr = cmd.ExecuteReader();
            //jntuh_registered_faculty oldjntuh_registered_faculty = new jntuh_registered_faculty();
            //if (dr.HasRows)
            //{
            //    while (dr.Read())
            //    {
            //        oldjntuh_registered_faculty.UniqueID = string.Empty;
            //        oldjntuh_registered_faculty.UserId = dr["UserId"] == DBNull.Value ? oldjntuh_registered_faculty.UserId : Convert.ToInt32(dr["UserId"]);
            //        facultyId = dr["Id"] == DBNull.Value ? facultyId : Convert.ToInt32(dr["Id"]);
            //        oldjntuh_registered_faculty.type = Convert.ToString(dr["type"]);
            //        oldjntuh_registered_faculty.collegeId = dr["collegeId"] == DBNull.Value ? oldjntuh_registered_faculty.collegeId : Convert.ToInt32(dr["collegeId"]);
            //        oldjntuh_registered_faculty.RegistrationNumber = Convert.ToString(dr["RegistrationNumber"]);
            //        //oldjntuh_registered_faculty.UserId =;
            //        oldjntuh_registered_faculty.FirstName = Convert.ToString(dr["FirstName"]);
            //        oldjntuh_registered_faculty.MiddleName = Convert.ToString(dr["MiddleName"]);
            //        oldjntuh_registered_faculty.LastName = Convert.ToString(dr["LastName"]);
            //        oldjntuh_registered_faculty.GenderId = dr["GenderId"] == DBNull.Value ? oldjntuh_registered_faculty.GenderId : Convert.ToInt32(dr["GenderId"]);
            //        oldjntuh_registered_faculty.FatherOrHusbandName = Convert.ToString(dr["FatherOrHusbandName"]);
            //        oldjntuh_registered_faculty.MotherName = Convert.ToString(dr["MotherName"]);
            //        oldjntuh_registered_faculty.DateOfBirth = dr["DateOfBirth"] == DBNull.Value ? oldjntuh_registered_faculty.DateOfBirth : Convert.ToDateTime(dr["DateOfBirth"]);
            //        oldjntuh_registered_faculty.WorkingStatus = dr["WorkingStatus"] == DBNull.Value ? oldjntuh_registered_faculty.WorkingStatus : Convert.ToBoolean(dr["WorkingStatus"]);
            //        oldjntuh_registered_faculty.OrganizationName = Convert.ToString(dr["OrganizationName"]);
            //        oldjntuh_registered_faculty.DesignationId = dr["DesignationId"] == DBNull.Value ? oldjntuh_registered_faculty.DesignationId : Convert.ToInt32(dr["DesignationId"]);
            //        oldjntuh_registered_faculty.OtherDesignation = Convert.ToString(dr["OtherDesignation"]);
            //        oldjntuh_registered_faculty.DepartmentId = dr["DepartmentId"] == DBNull.Value ? oldjntuh_registered_faculty.DepartmentId : Convert.ToInt32(dr["DepartmentId"]);
            //        oldjntuh_registered_faculty.OtherDepartment = Convert.ToString(dr["OtherDepartment"]);
            //        oldjntuh_registered_faculty.grosssalary = Convert.ToString(dr["grosssalary"]);
            //        oldjntuh_registered_faculty.DateOfAppointment = Convert.ToDateTime(dr["DateOfAppointment"]);
            //        oldjntuh_registered_faculty.isFacultyRatifiedByJNTU = dr["isFacultyRatifiedByJNTU"] == DBNull.Value ? oldjntuh_registered_faculty.isFacultyRatifiedByJNTU : Convert.ToBoolean(dr["isFacultyRatifiedByJNTU"]);
            //        oldjntuh_registered_faculty.DateOfRatification = dr["DateOfRatification"] == DBNull.Value ? oldjntuh_registered_faculty.DateOfRatification : Convert.ToDateTime(dr["DateOfRatification"]);
            //        oldjntuh_registered_faculty.ProceedingsNumber = Convert.ToString(dr["ProceedingsNumber"]);
            //        oldjntuh_registered_faculty.ProceedingDocument = Convert.ToString(dr["ProceedingDocument"]);
            //        oldjntuh_registered_faculty.AICTEFacultyId = Convert.ToString(dr["AICTEFacultyId"]);
            //        oldjntuh_registered_faculty.TotalExperience = dr["TotalExperience"] == DBNull.Value ? oldjntuh_registered_faculty.TotalExperience : Convert.ToInt32(dr["TotalExperience"]);
            //        oldjntuh_registered_faculty.TotalExperiencePresentCollege = dr["TotalExperiencePresentCollege"] == DBNull.Value ? oldjntuh_registered_faculty.TotalExperiencePresentCollege : Convert.ToInt32(dr["TotalExperiencePresentCollege"]);
            //        oldjntuh_registered_faculty.PANNumber = Convert.ToString(dr["PANNumber"]);
            //        oldjntuh_registered_faculty.PanStatus = Convert.ToString(dr["PanStatus"]);
            //        oldjntuh_registered_faculty.AadhaarNumber = Convert.ToString(dr["AadhaarNumber"]);
            //        oldjntuh_registered_faculty.Mobile = Convert.ToString(dr["Mobile"]);
            //        oldjntuh_registered_faculty.Email = Convert.ToString(dr["Email"]);
            //        oldjntuh_registered_faculty.National = Convert.ToString(dr["National"]);
            //        oldjntuh_registered_faculty.InterNational = Convert.ToString(dr["InterNational"]);
            //        oldjntuh_registered_faculty.Citation = Convert.ToString(dr["Citation"]);
            //        oldjntuh_registered_faculty.Awards = Convert.ToString(dr["Awards"]);
            //        oldjntuh_registered_faculty.Photo = Convert.ToString(dr["Photo"]);
            //        oldjntuh_registered_faculty.PANDocument = Convert.ToString(dr["PANDocument"]);
            //        oldjntuh_registered_faculty.AadhaarDocument = Convert.ToString(dr["AadhaarDocument"]);
            //        oldjntuh_registered_faculty.isActive = dr["isActive"] == DBNull.Value ? oldjntuh_registered_faculty.isActive : Convert.ToBoolean(dr["isActive"]);
            //        oldjntuh_registered_faculty.InStatus = Convert.ToString(dr["InStatus"]);

            //        oldjntuh_registered_faculty.isApproved = dr["isApproved"] == DBNull.Value ? oldjntuh_registered_faculty.isApproved : Convert.ToBoolean(dr["isApproved"]);

            //        oldjntuh_registered_faculty.createdOn = dr["createdOn"] == DBNull.Value ? oldjntuh_registered_faculty.createdOn : Convert.ToDateTime(dr["createdOn"]);
            //        oldjntuh_registered_faculty.createdBy = dr["createdBy"] == DBNull.Value ? oldjntuh_registered_faculty.createdBy : Convert.ToInt32(dr["createdBy"]);
            //        oldjntuh_registered_faculty.updatedOn = dr["updatedOn"] == DBNull.Value ? oldjntuh_registered_faculty.updatedOn : Convert.ToDateTime(dr["updatedOn"]);
            //        oldjntuh_registered_faculty.updatedBy = dr["updatedBy"] == DBNull.Value ? oldjntuh_registered_faculty.updatedBy : Convert.ToInt32(dr["updatedBy"]);
            //        oldjntuh_registered_faculty.DeactivationReason = dr["DeactivationReason"] == DBNull.Value ? oldjntuh_registered_faculty.DeactivationReason : Convert.ToString(dr["DeactivationReason"]);

            //        oldjntuh_registered_faculty.DeactivatedOn = dr["DeactivatedOn"] == DBNull.Value ? oldjntuh_registered_faculty.DeactivatedOn : Convert.ToDateTime(dr["DeactivatedOn"]);

            //        oldjntuh_registered_faculty.DeactivatedBy = dr["DeactivatedBy"] == DBNull.Value ? oldjntuh_registered_faculty.DeactivatedBy : Convert.ToInt32(dr["DeactivatedBy"]);
            //        oldjntuh_registered_faculty.WorkingType = Convert.ToString(dr["WorkingType"]);
            //        oldjntuh_registered_faculty.NOCFile = Convert.ToString(dr["NOCFile"]);
            //        oldjntuh_registered_faculty.PresentInstituteAssignedRole = Convert.ToString(dr["PresentInstituteAssignedRole"]);
            //        oldjntuh_registered_faculty.PresentInstituteAssignedResponsebility = Convert.ToString(dr["PresentInstituteAssignedResponsebility"]);
            //        oldjntuh_registered_faculty.Accomplish1 = Convert.ToString(dr["Accomplish1"]);
            //        oldjntuh_registered_faculty.Accomplish2 = Convert.ToString(dr["Accomplish2"]);
            //        oldjntuh_registered_faculty.Accomplish3 = Convert.ToString(dr["Accomplish3"]);
            //        oldjntuh_registered_faculty.Accomplish4 = Convert.ToString(dr["Accomplish4"]);
            //        oldjntuh_registered_faculty.Accomplish5 = Convert.ToString(dr["Accomplish5"]);
            //        oldjntuh_registered_faculty.Professional = Convert.ToString(dr["Professional"]);
            //        oldjntuh_registered_faculty.Professional2 = Convert.ToString(dr["Professional2"]);
            //        oldjntuh_registered_faculty.Professiona3 = Convert.ToString(dr["Professiona3"]);
            //        oldjntuh_registered_faculty.MembershipNo1 = Convert.ToString(dr["MembershipNo1"]);
            //        oldjntuh_registered_faculty.MembershipNo2 = Convert.ToString(dr["MembershipNo2"]);
            //        oldjntuh_registered_faculty.MembershipNo3 = Convert.ToString(dr["MembershipNo3"]);
            //        oldjntuh_registered_faculty.MembershipCertificate1 = Convert.ToString(dr["MembershipCertificate1"]);
            //        oldjntuh_registered_faculty.MembershipCertificate2 = Convert.ToString(dr["MembershipCertificate2"]);
            //        oldjntuh_registered_faculty.MembershipCertificate3 = Convert.ToString(dr["MembershipCertificate3"]);
            //        oldjntuh_registered_faculty.AdjunctDesignation = Convert.ToString(dr["AdjunctDesignation"]);
            //        oldjntuh_registered_faculty.AdjunctDepartment = Convert.ToString(dr["AdjunctDepartment"]);
            //        oldjntuh_registered_faculty.PanVerificationStatus = Convert.ToString(dr["PanVerificationStatus"]);
            //        oldjntuh_registered_faculty.PanDeactivationReason = Convert.ToString(dr["PanDeactivationReason"]);

            //        oldjntuh_registered_faculty.Absent = dr["Absent"] == DBNull.Value ? oldjntuh_registered_faculty.Absent : Convert.ToBoolean(dr["Absent"]);


            //        oldjntuh_registered_faculty.ModifiedPANNumber = Convert.ToString(dr["ModifiedPANNumber"]);
            //        oldjntuh_registered_faculty.InvalidPANNumber = dr["InvalidPANNumber"] == DBNull.Value ? oldjntuh_registered_faculty.InvalidPANNumber : Convert.ToBoolean(dr["InvalidPANNumber"]);
            //        oldjntuh_registered_faculty.NoRelevantUG = Convert.ToString(dr["NoRelevantUG"]);
            //        oldjntuh_registered_faculty.NoRelevantPG = Convert.ToString(dr["NoRelevantPG"]);
            //        oldjntuh_registered_faculty.NORelevantPHD = Convert.ToString(dr["NORelevantPHD"]);
            //        oldjntuh_registered_faculty.NoSCM = dr["NoSCM"] == DBNull.Value ? oldjntuh_registered_faculty.NoSCM : Convert.ToBoolean(dr["NoSCM"]);
            //        oldjntuh_registered_faculty.NoForm16 = dr["NoForm16"] == DBNull.Value ? oldjntuh_registered_faculty.NoForm16 : Convert.ToBoolean(dr["NoForm16"]);
            //        oldjntuh_registered_faculty.ModifiedDateofAppointment = dr["ModifiedDateofAppointment"] == DBNull.Value ? oldjntuh_registered_faculty.ModifiedDateofAppointment : Convert.ToDateTime(dr["ModifiedDateofAppointment"]);
            //        oldjntuh_registered_faculty.NotQualifiedAsperAICTE = dr["NotQualifiedAsperAICTE"] == DBNull.Value ? oldjntuh_registered_faculty.NotQualifiedAsperAICTE : Convert.ToBoolean(dr["NotQualifiedAsperAICTE"]);
            //        oldjntuh_registered_faculty.MultipleRegInSameCollege = dr["MultipleRegInSameCollege"] == DBNull.Value ? oldjntuh_registered_faculty.MultipleRegInSameCollege : Convert.ToBoolean(dr["MultipleRegInSameCollege"]);
            //        oldjntuh_registered_faculty.MultipleRegInDiffCollege = dr["MultipleRegInDiffCollege"] == DBNull.Value ? oldjntuh_registered_faculty.MultipleRegInDiffCollege : Convert.ToBoolean(dr["MultipleRegInDiffCollege"]);
            //        oldjntuh_registered_faculty.SamePANUsedByMultipleFaculty = dr["SamePANUsedByMultipleFaculty"] == DBNull.Value ? oldjntuh_registered_faculty.SamePANUsedByMultipleFaculty : Convert.ToBoolean(dr["SamePANUsedByMultipleFaculty"]);
            //        oldjntuh_registered_faculty.PhotoCopyofPAN = dr["PhotoCopyofPAN"] == DBNull.Value ? oldjntuh_registered_faculty.PhotoCopyofPAN : Convert.ToBoolean(dr["PhotoCopyofPAN"]);
            //        oldjntuh_registered_faculty.AppliedPAN = dr["AppliedPAN"] == DBNull.Value ? oldjntuh_registered_faculty.AppliedPAN : Convert.ToBoolean(dr["AppliedPAN"]);
            //        oldjntuh_registered_faculty.LostPAN = dr["LostPAN"] == DBNull.Value ? oldjntuh_registered_faculty.LostPAN : Convert.ToBoolean(dr["LostPAN"]);
            //        oldjntuh_registered_faculty.OriginalsVerifiedUG = dr["OriginalsVerifiedUG"] == DBNull.Value ? oldjntuh_registered_faculty.OriginalsVerifiedUG : Convert.ToBoolean(dr["OriginalsVerifiedUG"]);
            //        oldjntuh_registered_faculty.OriginalsVerifiedPG = dr["OriginalsVerifiedPG"] == DBNull.Value ? oldjntuh_registered_faculty.OriginalsVerifiedPG : Convert.ToBoolean(dr["OriginalsVerifiedPG"]);
            //        oldjntuh_registered_faculty.OriginalsVerifiedPHD = dr["OriginalsVerifiedPHD"] == DBNull.Value ? oldjntuh_registered_faculty.OriginalsVerifiedPHD : Convert.ToBoolean(dr["OriginalsVerifiedPHD"]);
            //        oldjntuh_registered_faculty.FacultyVerificationStatus = dr["FacultyVerificationStatus"] == DBNull.Value ? oldjntuh_registered_faculty.FacultyVerificationStatus : Convert.ToBoolean(dr["FacultyVerificationStatus"]);
            //        oldjntuh_registered_faculty.Others1 = Convert.ToString(dr["Others1"]);
            //        oldjntuh_registered_faculty.Others2 = Convert.ToString(dr["Others2"]);
            //        oldjntuh_registered_faculty.IncompleteCertificates = dr["IncompleteCertificates"] == DBNull.Value ? oldjntuh_registered_faculty.IncompleteCertificates : Convert.ToBoolean(dr["IncompleteCertificates"]);
            //        oldjntuh_registered_faculty.PanStatusAfterDE = Convert.ToString(dr["PanStatusAfterDE"]);
            //        oldjntuh_registered_faculty.PanReasonAfterDE = Convert.ToString(dr["PanReasonAfterDE"]);
            //        oldjntuh_registered_faculty.NoSpecialization = dr["NoSpecialization"] == DBNull.Value ? oldjntuh_registered_faculty.NoSpecialization : Convert.ToBoolean(dr["NoSpecialization"]);
            //        oldjntuh_registered_faculty.FalsePAN = dr["FalsePAN"] == DBNull.Value ? oldjntuh_registered_faculty.FalsePAN : Convert.ToBoolean(dr["FalsePAN"]);
            //        oldjntuh_registered_faculty.Notin116 = dr["Notin116"] == DBNull.Value ? oldjntuh_registered_faculty.Notin116 : Convert.ToBoolean(dr["Notin116"]);
            //        oldjntuh_registered_faculty.PHDundertakingnotsubmitted = dr["PHDundertakingnotsubmitted"] == DBNull.Value ? oldjntuh_registered_faculty.PHDundertakingnotsubmitted : Convert.ToBoolean(dr["PHDundertakingnotsubmitted"]);
            //        oldjntuh_registered_faculty.Blacklistfaculy = dr["Blacklistfaculy"] == DBNull.Value ? oldjntuh_registered_faculty.Blacklistfaculy : Convert.ToBoolean(dr["Blacklistfaculy"]);
            //        oldjntuh_registered_faculty.DiscrepencyStatus = dr["DiscrepencyStatus"] == DBNull.Value ? oldjntuh_registered_faculty.DiscrepencyStatus : Convert.ToBoolean(dr["DiscrepencyStatus"]);
            //        oldjntuh_registered_faculty.NoPhdUndertakingNew = dr["NoPhdUndertakingNew"] == DBNull.Value ? oldjntuh_registered_faculty.NoPhdUndertakingNew : Convert.ToBoolean(dr["NoPhdUndertakingNew"]);
            //        oldjntuh_registered_faculty.IncometaxDocument = Convert.ToString(dr["IncometaxDocument"]);
            //        oldjntuh_registered_faculty.PGSpecialization = dr["PGSpecialization"] == DBNull.Value ? oldjntuh_registered_faculty.PGSpecialization : Convert.ToInt32(dr["PGSpecialization"]);
            //        oldjntuh_registered_faculty.PHDUndertakingDocument = Convert.ToString(dr["PHDUndertakingDocument"]);
            //        oldjntuh_registered_faculty.BASStatus = Convert.ToString(dr["BASStatus"]);
            //        oldjntuh_registered_faculty.BASStatusOld = Convert.ToString(dr["BASStatusOld"]);
            //        oldjntuh_registered_faculty.OriginalCertificatesNotShown = dr["OriginalCertificatesNotShown"] == DBNull.Value ? oldjntuh_registered_faculty.OriginalCertificatesNotShown : Convert.ToBoolean(dr["OriginalCertificatesNotShown"]);
            //        oldjntuh_registered_faculty.PGSpecializationRemarks = Convert.ToString(dr["PGSpecializationRemarks"]);
            //        oldjntuh_registered_faculty.Xeroxcopyofcertificates = dr["Xeroxcopyofcertificates"] == DBNull.Value ? oldjntuh_registered_faculty.Xeroxcopyofcertificates : Convert.ToBoolean(dr["Xeroxcopyofcertificates"]);
            //        oldjntuh_registered_faculty.PhdUndertakingDocumentstatus = dr["PhdUndertakingDocumentstatus"] == DBNull.Value ? oldjntuh_registered_faculty.PhdUndertakingDocumentstatus : Convert.ToBoolean(dr["PhdUndertakingDocumentstatus"]);
            //        oldjntuh_registered_faculty.PhdUndertakingDocumentText = Convert.ToString(dr["PhdUndertakingDocumentText"]);
            //        oldjntuh_registered_faculty.NotIdentityfiedForanyProgram = dr["NotIdentityfiedForanyProgram"] == DBNull.Value ? oldjntuh_registered_faculty.NotIdentityfiedForanyProgram : Convert.ToBoolean(dr["NotIdentityfiedForanyProgram"]);
            //        oldjntuh_registered_faculty.NoSCM17 = dr["NoSCM17"] == DBNull.Value ? oldjntuh_registered_faculty.NoSCM17 : Convert.ToBoolean(dr["NoSCM17"]);
            //        //oldjntuh_registered_faculty.Noform16Verification =dr["Noform16Verification"]==DBNull.Value?oldjntuh_registered_faculty.Noform16Verification: Convert.ToBoolean(dr["Noform16Verification"]);
            //        oldjntuh_registered_faculty.PhdDeskVerification = dr["PhdDeskVerification"] == DBNull.Value ? oldjntuh_registered_faculty.PhdDeskVerification : Convert.ToBoolean(dr["PhdDeskVerification"]);
            //        oldjntuh_registered_faculty.PhdDeskReason = Convert.ToString(dr["PhdDeskReason"]);
            //        oldjntuh_registered_faculty.ACollegeId = dr["ACollegeId"] == DBNull.Value ? oldjntuh_registered_faculty.ACollegeId : Convert.ToInt32(dr["ACollegeId"]);
            //        oldjntuh_registered_faculty.ADepartmentId = dr["ADepartmentId"] == DBNull.Value ? oldjntuh_registered_faculty.ADepartmentId : Convert.ToInt32(dr["ADepartmentId"]);
            //        oldjntuh_registered_faculty.ASpecializationId = dr["ASpecializationId"] == DBNull.Value ? oldjntuh_registered_faculty.ASpecializationId : Convert.ToInt32(dr["ASpecializationId"]);
            //        oldjntuh_registered_faculty.AIdentifiedFor = Convert.ToString(dr["AIdentifiedFor"]);

            //    }
            //}
            //con.Close();

            //con.Open();
            //List<jntuh_registered_faculty_education> jntuh_registered_faculty_educationList = new List<jntuh_registered_faculty_education>();
            //query = string.Empty;
            //query = "SELECT * FROM jntuh_registered_faculty_education WHERE facultyId=" + facultyId + "";
            //MySqlCommand cmdmembership = new MySqlCommand(query, con);
            //MySqlDataReader drm = cmdmembership.ExecuteReader();
            //if (drm.HasRows)
            //{
            //    while (drm.Read())
            //    {
            //        jntuh_registered_faculty_education oldjntuh_registered_faculty_education = new jntuh_registered_faculty_education();
            //        oldjntuh_registered_faculty_education.facultyId = drm["facultyId"] == DBNull.Value ? oldjntuh_registered_faculty_education.facultyId : Convert.ToInt32(drm["facultyId"]);
            //        oldjntuh_registered_faculty_education.educationId = drm["educationId"] == DBNull.Value ? oldjntuh_registered_faculty_education.educationId : Convert.ToInt32(drm["educationId"]);
            //        oldjntuh_registered_faculty_education.courseStudied = Convert.ToString(drm["courseStudied"].ToString());
            //        oldjntuh_registered_faculty_education.specialization = Convert.ToString(drm["specialization"]);
            //        oldjntuh_registered_faculty_education.passedYear = drm["passedYear"] == DBNull.Value ? oldjntuh_registered_faculty_education.passedYear : Convert.ToInt32(drm["passedYear"]);
            //        oldjntuh_registered_faculty_education.marksPercentage = drm["marksPercentage"] == DBNull.Value ? oldjntuh_registered_faculty_education.marksPercentage : Convert.ToDecimal(drm["marksPercentage"]);
            //        oldjntuh_registered_faculty_education.division = drm["division"] == DBNull.Value ? oldjntuh_registered_faculty_education.division : Convert.ToInt32(drm["division"]);
            //        oldjntuh_registered_faculty_education.boardOrUniversity = Convert.ToString(drm["boardOrUniversity"]);
            //        oldjntuh_registered_faculty_education.placeOfEducation = Convert.ToString(drm["placeOfEducation"]);
            //        oldjntuh_registered_faculty_education.certificate = Convert.ToString(drm["certificate"]);
            //        oldjntuh_registered_faculty_education.isActive = drm["isActive"] == DBNull.Value ? oldjntuh_registered_faculty_education.isActive : Convert.ToBoolean(drm["isActive"]);
            //        oldjntuh_registered_faculty_education.createdOn = drm["createdOn"] == DBNull.Value ? oldjntuh_registered_faculty_education.createdOn : Convert.ToDateTime(drm["createdOn"]);
            //        oldjntuh_registered_faculty_education.createdBy = 1;
            //        oldjntuh_registered_faculty_education.updatedOn = drm["updatedOn"] == DBNull.Value ? oldjntuh_registered_faculty_education.createdOn : Convert.ToDateTime(drm["createdOn"]);
            //        oldjntuh_registered_faculty_education.createdBy = 1;
            //        jntuh_registered_faculty_educationList.Add(oldjntuh_registered_faculty_education);
            //    }
            //}
            //con.Close();
            //if (oldjntuh_registered_faculty.UserId == 0 && oldjntuh_registered_faculty != null)
            //{
            //    ViewBag.Error = "No Data in This Registration Number";
            //    return View(regno);
            //}
            //if (jntuh_registered_faculty_educationList.Count() == 0)
            //{
            //    ViewBag.Error = "No Education Details in This Registration Number";
            //    return View(regno);
            //}
            //MembershipCreateStatus createStatus;
            //try
            //{
            //    if (!String.IsNullOrEmpty(oldjntuh_registered_faculty.Email))
            //    {
            //        Membership.CreateUser(oldjntuh_registered_faculty.Email, "loveindia@123",
            //            oldjntuh_registered_faculty.Email, null, null, true, out createStatus);
            //        if (createStatus == MembershipCreateStatus.Success)
            //        {
            //            my_aspnet_usersinroles roleModel = new my_aspnet_usersinroles();
            //            roleModel.roleId = 7; // 7 = Faculty Role

            //            roleModel.userId =
            //                db.my_aspnet_users.Where(u => u.name == oldjntuh_registered_faculty.Email)
            //                    .Select(u => u.id)
            //                    .FirstOrDefault();
            //            newUserId = roleModel.userId;
            //            db.my_aspnet_usersinroles.Add(roleModel);
            //            db.SaveChanges();
            //        }
            //        if (newUserId != 0 && oldjntuh_registered_faculty != null &&
            //            jntuh_registered_faculty_educationList.Count() > 0)
            //        {
            //            oldjntuh_registered_faculty.UserId = newUserId;
            //            db.jntuh_registered_faculty.Add(oldjntuh_registered_faculty);
            //            db.SaveChanges();
            //        }
            //        List<jntuh_registered_faculty_education> neweducation =
            //            new List<jntuh_registered_faculty_education>();
            //        newfacultyId =
            //            db.jntuh_registered_faculty.Where(
            //                c => c.UserId == newUserId && c.Email == oldjntuh_registered_faculty.Email)
            //                .Select(s => s.id)
            //                .FirstOrDefault();
            //        if (jntuh_registered_faculty_educationList.Count() > 0)
            //        {
            //            foreach (var item in jntuh_registered_faculty_educationList)
            //            {
            //                jntuh_registered_faculty_education education = new jntuh_registered_faculty_education();
            //                education.facultyId = newfacultyId;
            //                education.educationId = item.educationId;
            //                education.courseStudied = item.courseStudied;
            //                education.specialization = item.specialization;
            //                education.passedYear = item.passedYear == null ? 0 : (int)item.passedYear;
            //                education.marksPercentage = item.marksPercentage == null
            //                    ? 0
            //                    : (decimal)item.marksPercentage;
            //                education.division = item.division == null ? 0 : (int)item.division;
            //                education.boardOrUniversity = item.boardOrUniversity;
            //                education.placeOfEducation = item.placeOfEducation;
            //                education.createdBy = item.createdBy;
            //                education.createdOn = item.createdOn;
            //                education.updatedOn = item.updatedOn;
            //                education.updatedBy = item.updatedBy;
            //                education.certificate = null;
            //                if (item.certificate != null)
            //                {
            //                    //System.IO.File.Move(string.Format("{0}\\{1}", Server.MapPath(certificatesPath), item.facultyCertificate), string.Format("{0}\\{1}", Server.MapPath(certificatesPath), regFaculty.RegistrationNumber + "-" + item.educationId + Path.GetExtension(item.facultyCertificate)));
            //                    //education.certificate = regFaculty.RegistrationNumber + "-" + item.educationId + Path.GetExtension(item.facultyCertificate);
            //                    education.certificate = item.certificate;
            //                }
            //                neweducation.Add(education);
            //            }
            //        }
            //        neweducation.ForEach(d => db.jntuh_registered_faculty_education.Add(d));
            //        db.SaveChanges();
            //        ViewBag.Success = "Faculty Added Successfuly";
            //    }
            //}
            //catch (Exception ex)
            //{
            //    ViewBag.Error = ex.ToString();
            //}
            #endregion
            return View(FacultyRegistration);
        }

        //Adding Faculty
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public ActionResult AddFaculty(int? fid, string isstatus)
        {
            if (fid != 0 && fid != null)
            {
                string photoPath = "~/Content/Upload/Faculty/PHOTOS";
                string panCardsPath = "~/Content/Upload/Faculty/PANCARDS";
                string aadhaarCardsPath = "~/Content/Upload/Faculty/AADHAARCARDS";
                string proceedingsPath = "~/Content/Upload/Faculty/PROCEEDINGS";
                string certificatesPath = "~/Content/Upload/Faculty/CERTIFICATES";
                string IncomeTaxcertificatesPath = "~/Content/Upload/Faculty/INCOMETAX";
                string CollegeFacultyaadhaarCardsPath = "~/Content/Upload/Faculty/AADHAARCARDS/CollegeFacultyAadhaar";
                string CollegePrincipalaadhaarCardsPath = "~/Content/Upload/Faculty/AADHAARCARDS/CollegePrincipalAadhaar";              
                string AllcertificatesPath = "~/Content/Upload/Faculty/CERTIFICATES/CertificatesPDF";
                string facultyexperiencedocumentspath = "~/Content/Upload/Faculty/ExperienceDocuments";
                string facultyNewspath = "~/Content/Upload/FacultyNews";
                string facultyTwoPagesFormatPath = "~/Content/Upload/Faculty/PhdDocument";

                jntuh_registered_faculty Faculty = db.jntuh_registered_faculty.Where(a => a.id == fid).Select(e => e).FirstOrDefault();
                
                #region This is Written By Narayana Reddy Black and Absent for Verification can't Delete
                if (Faculty.Blacklistfaculy == true || Faculty.AbsentforVerification == true)
                {
                    TempData["ERROR"] = "Faculty Registration can't Delete due to Blacklist/Absent for Verification.";
                    return RedirectToAction("GetRegistation");
                }
                if (!String.IsNullOrEmpty(Faculty.PANNumber))
                {
                    var registratedfacultywithpan = db.jntuh_registered_faculty.Where(a => a.PANNumber == Faculty.PANNumber.Trim()).Select(e => e).ToList();
                    if (registratedfacultywithpan.Count!=0)
                    {
                        foreach (var panreg in registratedfacultywithpan)
                        {
                            if (panreg.Blacklistfaculy == true || panreg.AbsentforVerification == true)
                            {
                                TempData["ERROR"] =Faculty.RegistrationNumber+" This faculty Have Multiple Registrations can't Delete due to Blacklist/Absent for Verification.";
                                return RedirectToAction("GetRegistation");
                            }
                        }
                    }
                }
                var collegefaculty =
                    db.jntuh_college_faculty_registered.Where(
                        r => r.RegistrationNumber == Faculty.RegistrationNumber.Trim())
                        .Select(s => s.RegistrationNumber)
                        .FirstOrDefault();
                if (!String.IsNullOrEmpty(collegefaculty))
                {
                    TempData["ERROR"] =collegefaculty+ " Faculty Registration can't Delete due to College Assocation.";
                    return RedirectToAction("GetRegistation");
                }
                #endregion  

                string constr = "Data Source=10.10.10.16;user id=root;password=Jntu@123!@#123;database=deletefaculty;";
                //string constr = "Data Source=10.10.10.91;user id=root;password=jntu123;database=deletefaculty;";
                MySqlConnection con = new MySqlConnection(constr);

                string Usersquery = "SELECT id FROM my_aspnet_usersdelete WHERE id='" + Faculty.UserId + "'";
                string Membershipquery = "SELECT userId FROM my_aspnet_membershipdelete WHERE userId='" + Faculty.UserId + "'";
                string Rolesquery = "SELECT roleId FROM my_aspnet_usersinrolesdelete WHERE userId='" + Faculty.UserId + "'";
                string Registrationquery = "SELECT RegistrationNumber FROM jntuh_registered_facultydelete WHERE RegistrationNumber='" + Faculty.RegistrationNumber.Trim() + "'";
                string Educationquery = "SELECT count(facultyId) FROM jntuh_registered_faculty_educationdelete WHERE facultyId='" + Faculty.id + "'";
                string EducationLogquery = "SELECT count(facultyId) FROM jntuh_registered_faculty_education_logdelete WHERE facultyId='" + Faculty.id + "'";
                string Experiencequery = "SELECT count(facultyId) FROM jntuh_registered_faculty_experiencedelete WHERE facultyId='" + Faculty.id + "'";
                string SubjectTaughtquery = "SELECT count(facultyId) FROM jntuh_registered_faculty_subjectstaughtdelete WHERE facultyId='" + Faculty.id + "'";
                string Newsquery = "SELECT count(facultyId) FROM jntuh_faculty_newsdelete WHERE facultyId='" + Faculty.id + "'";

                con.Open();
                MySqlCommand Userscmd = new MySqlCommand(Usersquery, con);
                var Users = Userscmd.ExecuteScalar();
                MySqlCommand Membershipcmd = new MySqlCommand(Membershipquery, con);
                var Membership = Membershipcmd.ExecuteScalar();
                MySqlCommand Rolescmd = new MySqlCommand(Rolesquery, con);
                var Roles = Rolescmd.ExecuteScalar();
                MySqlCommand Registrationcmd = new MySqlCommand(Registrationquery, con);
                var Registration = Registrationcmd.ExecuteScalar();
                MySqlCommand Educationcmd = new MySqlCommand(Educationquery, con);
                int Education = Convert.ToInt32(Educationcmd.ExecuteScalar());
                MySqlCommand EducationLogcmd = new MySqlCommand(EducationLogquery, con);
                int EducationLog = Convert.ToInt32(EducationLogcmd.ExecuteScalar());
                MySqlCommand Experiencecmd = new MySqlCommand(Experiencequery, con);
                int Experience = Convert.ToInt32(Experiencecmd.ExecuteScalar());
                MySqlCommand Subjectscmd = new MySqlCommand(SubjectTaughtquery, con);
                int Subjects = Convert.ToInt32(Subjectscmd.ExecuteScalar());
                MySqlCommand Newscmd = new MySqlCommand(Newsquery, con);
                int News = Convert.ToInt32(Newscmd.ExecuteScalar());
                con.Close();

                //Users Table Inserting
                my_aspnet_users Faculty_Users = db.my_aspnet_users.Where(a => a.id == Faculty.UserId).Select(e => e).FirstOrDefault();
                if (Users == null)
                {
                    if (Faculty_Users != null)
                    {
                        DateTime dt = DateTime.ParseExact(Faculty_Users.lastActivityDate.ToString(), "MM/dd/yyyy HH:mm:ss", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None);
                        string ActiveDate = dt.ToString("yyyy-MM-dd HH:mm:ss");

                        con.Open();
                        string UsersQuery = "INSERT INTO my_aspnet_usersdelete (id,applicationId,NAME,isAnonymous,lastActivityDate)";
                        UsersQuery += " values ('" + Faculty_Users.id + "','" + Faculty_Users.applicationId + "','" + Faculty_Users.name + "'," + Faculty_Users.isAnonymous + ",'" + ActiveDate + "')";
                        MySqlCommand UsersTablecmd = new MySqlCommand(UsersQuery, con);
                        UsersTablecmd.ExecuteNonQuery();
                        con.Close();
                    }
                }

                //membership Table Inserting
                my_aspnet_membership Faculty_Membership = db.my_aspnet_membership.Where(a => a.userId == Faculty.UserId).Select(e => e).FirstOrDefault();
                if (Membership == null)
                {
                    if (Faculty_Membership != null)
                    {

                        DateTime dt = DateTime.ParseExact(Faculty_Membership.LastActivityDate.ToString(), "MM/dd/yyyy HH:mm:ss", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None);
                        string LastActivityDate = dt.ToString("yyyy-MM-dd HH:mm:ss");
                        DateTime dt1 = DateTime.ParseExact(Faculty_Membership.LastLoginDate.ToString(), "MM/dd/yyyy HH:mm:ss", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None);
                        string LastLoginDate = dt1.ToString("yyyy-MM-dd HH:mm:ss");
                        DateTime dt2 = DateTime.ParseExact(Faculty_Membership.LastPasswordChangedDate.ToString(), "MM/dd/yyyy HH:mm:ss", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None);
                        string LastPasswordChangedDate = dt2.ToString("yyyy-MM-dd HH:mm:ss");
                        DateTime dt3 = DateTime.ParseExact(Faculty_Membership.CreationDate.ToString(), "MM/dd/yyyy HH:mm:ss", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None);
                        string CreationDate = dt3.ToString("yyyy-MM-dd HH:mm:ss");
                        DateTime dt4 = DateTime.ParseExact(Faculty_Membership.LastLockedOutDate.ToString(), "MM/dd/yyyy HH:mm:ss", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None);
                        string LastLockedOutDate = dt4.ToString("yyyy-MM-dd HH:mm:ss");
                        DateTime dt5 = DateTime.ParseExact(Faculty_Membership.FailedPasswordAttemptWindowStart.ToString(), "MM/dd/yyyy HH:mm:ss", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None);
                        string FailedPasswordAttemptWindowStart = dt5.ToString("yyyy-MM-dd HH:mm:ss");
                        DateTime dt6 = DateTime.ParseExact(Faculty_Membership.FailedPasswordAnswerAttemptWindowStart.ToString(), "MM/dd/yyyy HH:mm:ss", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None);
                        string FailedPasswordAnswerAttemptWindowStart = dt6.ToString("yyyy-MM-dd HH:mm:ss");

                        con.Open();
                        string MembershipQuery = "INSERT INTO my_aspnet_membershipdelete (userId,Email,COMMENT,PASSWORD,PasswordKey,PasswordFormat,PasswordQuestion,PasswordAnswer,IsApproved,LastActivityDate,LastLoginDate,LastPasswordChangedDate,CreationDate,IsLockedOut,LastLockedOutDate,FailedPasswordAttemptCount,FailedPasswordAttemptWindowStart,FailedPasswordAnswerAttemptCount,FailedPasswordAnswerAttemptWindowStart)";
                        MembershipQuery += " values ('" + Faculty_Membership.userId + "','" + Faculty_Membership.Email + "','" + Faculty_Membership.Comment + "','" + Faculty_Membership.Password + "','" + Faculty_Membership.PasswordKey + "','" + Faculty_Membership.PasswordFormat + "','" + Faculty_Membership.PasswordQuestion + "','" + Faculty_Membership.PasswordAnswer + "'," + Faculty_Membership.IsApproved + ",'" + LastActivityDate + "','" + LastLoginDate + "','" + LastPasswordChangedDate + "','" + CreationDate + "'," + Faculty_Membership.IsLockedOut + ",'" + LastLockedOutDate + "','" + Faculty_Membership.FailedPasswordAttemptCount + "','" + FailedPasswordAttemptWindowStart + "','" + Faculty_Membership.FailedPasswordAnswerAttemptCount + "','" + FailedPasswordAnswerAttemptWindowStart + "')";
                        MySqlCommand MembershipTablecmd = new MySqlCommand(MembershipQuery, con);
                        MembershipTablecmd.ExecuteNonQuery();
                        con.Close();
                    }
                }

                //usersinRole Table Inserting
                my_aspnet_usersinroles Faculty_UserInRole = db.my_aspnet_usersinroles.Where(a => a.userId == Faculty.UserId).Select(e => e).FirstOrDefault();
                if (Roles == null)
                {
                    if (Faculty_UserInRole != null)
                    {
                        con.Open();
                        string UserInRoleQuery = "INSERT INTO my_aspnet_usersinrolesdelete (userId,roleId,id)";
                        UserInRoleQuery += " values ('" + Faculty_UserInRole.userId + "','" + Faculty_UserInRole.roleId + "','" + Faculty_UserInRole.id + "')";
                        MySqlCommand UserInRolecmd = new MySqlCommand(UserInRoleQuery, con);
                        UserInRolecmd.ExecuteNonQuery();
                        con.Close();
                    }
                }

                //Registation Table Inserting
                if (Registration == null)
                {
                    if (Faculty != null)
                    {
                        int? RegCollegeId = Faculty.collegeId == null ? 0 : Faculty.collegeId;

                        DateTime dt7 = DateTime.ParseExact(Faculty.DateOfBirth.ToString(), "MM/dd/yyyy HH:mm:ss", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None);
                        string DateOfBirth = dt7.ToString("yyyy-MM-dd HH:mm:ss");
                        string DateOfAppointment = string.Empty;
                        if (Faculty.DateOfAppointment != null)
                        {
                            DateTime dt8 = DateTime.ParseExact(Faculty.DateOfAppointment.ToString(), "MM/dd/yyyy HH:mm:ss", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None);
                            DateOfAppointment = dt8.ToString("yyyy-MM-dd HH:mm:ss");
                        }
                        string DateOfRatification = string.Empty;
                        if (Faculty.DateOfRatification != null)
                        {
                            DateTime dt9 = DateTime.ParseExact(Faculty.DateOfRatification.ToString(), "MM/dd/yyyy HH:mm:ss", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None);
                            DateOfRatification = dt9.ToString("yyyy-MM-dd HH:mm:ss");
                        }
                        DateTime dt10 = DateTime.ParseExact(Faculty.createdOn.ToString(), "MM/dd/yyyy HH:mm:ss", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None);
                        string createdOn = dt10.ToString("yyyy-MM-dd HH:mm:ss");
                        string updatedOn = string.Empty;
                        if (Faculty.updatedOn != null)
                        {
                            DateTime dt11 = DateTime.ParseExact(Faculty.updatedOn.ToString(), "MM/dd/yyyy HH:mm:ss", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None);
                            updatedOn = dt11.ToString("yyyy-MM-dd HH:mm:ss");
                        }
                        string DeactivatedOn = string.Empty;
                        if (Faculty.DeactivatedOn != null)
                        {
                            DateTime dt12 = DateTime.ParseExact(Faculty.DeactivatedOn.ToString(), "MM/dd/yyyy HH:mm:ss", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None);
                            DeactivatedOn = dt12.ToString("yyyy-MM-dd HH:mm:ss");
                        }
                        string ModifiedDateofAppointment = string.Empty;
                        if (Faculty.ModifiedDateofAppointment != null)
                        {
                            DateTime dt13 = DateTime.ParseExact(Faculty.ModifiedDateofAppointment.ToString(), "MM/dd/yyyy HH:mm:ss", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None);
                            ModifiedDateofAppointment = dt13.ToString("yyyy-MM-dd HH:mm:ss");
                        }

                        string Orgname = string.Empty;
                        if (!string.IsNullOrEmpty(Faculty.OrganizationName))
                            Orgname = Faculty.OrganizationName.Replace("'", " ");
                        string National = string.Empty;
                        if (!string.IsNullOrEmpty(Faculty.National))
                            National = Faculty.National.Replace("'", " ");
                        string InterNational = string.Empty;
                        if (!string.IsNullOrEmpty(Faculty.InterNational))
                            InterNational = Faculty.InterNational.Replace("'", " ");
                        string Citation = string.Empty;
                        if (!string.IsNullOrEmpty(Faculty.Citation))
                            Citation = Faculty.Citation.Replace("'", " ");
                        string Awards = string.Empty;
                        if (!string.IsNullOrEmpty(Faculty.Awards))
                            Awards = Faculty.Awards.Replace("'", " ");
                        string DeactivationReason = string.Empty;
                        if (!string.IsNullOrEmpty(Faculty.DeactivationReason))
                            DeactivationReason = Faculty.DeactivationReason.Replace("'", " ");
                        string PanDeactivationReason = string.Empty;
                        if (!string.IsNullOrEmpty(Faculty.PanDeactivationReason))
                            PanDeactivationReason = Faculty.PanDeactivationReason.Replace("'", " ");
                        string PhdDeskReason = string.Empty;
                        if (!string.IsNullOrEmpty(Faculty.PhdDeskReason))
                            PhdDeskReason = Faculty.PhdDeskReason.Replace("'", " ");

                        string Insertquery = "INSERT INTO jntuh_registered_facultydelete (id,type,UserId,collegeId,RegistrationNumber,UniqueID,FirstName,MiddleName,LastName,GenderId,FatherOrHusbandName,MotherName,DateOfBirth,WorkingStatus,OrganizationName,DesignationId,OtherDesignation,DepartmentId,OtherDepartment,grosssalary,DateOfAppointment,isFacultyRatifiedByJNTU,DateOfRatification,ProceedingsNumber,ProceedingDocument,AICTEFacultyId,TotalExperience,TotalExperiencePresentCollege,PANNumber,PanStatus,AadhaarNumber,Mobile,Email,National,InterNational,Citation,Awards,Photo,PANDocument,AadhaarDocument,isActive,InStatus,isApproved,createdOn,createdBy,updatedOn,updatedBy,DeactivationReason,DeactivatedOn,DeactivatedBy,WorkingType,NOCFile,PresentInstituteAssignedRole,PresentInstituteAssignedResponsebility,Accomplish1,Accomplish2,Accomplish3,Accomplish4,Accomplish5,Professional,Professional2,Professiona3,MembershipNo1,MembershipNo2,MembershipNo3,MembershipCertificate1,MembershipCertificate2,MembershipCertificate3,AdjunctDesignation,AdjunctDepartment,PanVerificationStatus,PanDeactivationReason,Absent,ModifiedPANNumber,InvalidPANNumber,NoRelevantUG,NoRelevantPG,NORelevantPHD,NoSCM,NoForm16,ModifiedDateofAppointment,NotQualifiedAsperAICTE,Noclass,Invaliddegree,Genuinenessnotsubmitted,NotconsideredPHD,FakePHD,NoPGspecialization,OriginalsVerifiedUG,OriginalsVerifiedPG,OriginalsVerifiedPHD,FacultyVerificationStatus,Others1,Others2,IncompleteCertificates,PanStatusAfterDE,PanReasonAfterDE,NoSpecialization,FalsePAN,Notin116,PHDundertakingnotsubmitted,Blacklistfaculy,DiscrepencyStatus,NoPhdUndertakingNew,IncometaxDocument,PGSpecialization,PHDUndertakingDocument,InvalidAadhaar,BAS,OriginalCertificatesNotShown,PGSpecializationRemarks,Xeroxcopyofcertificates,PhdUndertakingDocumentstatus,PhdUndertakingDocumentText,NotIdentityfiedForanyProgram,NoSCM17,AbsentforVerification,PhdDeskVerification,PhdDeskReason,ACollegeId,ADepartmentId,Jntu_PGSpecializationId,AIdentifiedFor)";
                        Insertquery += " values ('" + Faculty.id + "','" + Faculty.type + "','" + Faculty.UserId + "','" + RegCollegeId + "','" + Faculty.RegistrationNumber + "','" + Faculty.UniqueID + "','" + Faculty.FirstName + "','" + Faculty.MiddleName + "','" + Faculty.LastName + "','" + Faculty.GenderId + "','" + Faculty.FatherOrHusbandName + "','" + Faculty.MotherName + "','" + DateOfBirth + "'," + Faculty.WorkingStatus + ",'" + Orgname + "','" + Faculty.DesignationId + "','" + Faculty.OtherDesignation + "','" + Faculty.DepartmentId + "','" + Faculty.OtherDepartment + "','" + Faculty.grosssalary + "','" + DateOfAppointment + "'," + Faculty.isFacultyRatifiedByJNTU + ",'" + DateOfRatification + "','" + Faculty.ProceedingsNumber + "','" + Faculty.ProceedingDocument + "','" + Faculty.AICTEFacultyId + "','" + Faculty.TotalExperience + "','" + Faculty.TotalExperiencePresentCollege + "','" + Faculty.PANNumber + "','" + Faculty.PanStatus + "','" + Faculty.AadhaarNumber + "','" + Faculty.Mobile + "','" + Faculty.Email + "','" + National + "','" + InterNational + "','" + Citation + "','" + Awards + "','" + Faculty.Photo + "','" + Faculty.PANDocument + "','" + Faculty.AadhaarDocument + "'," + Faculty.isActive + ",'" + isstatus.Replace("'", " ") + "'," + Faculty.isApproved + ",'" + createdOn + "','" + Faculty.createdBy + "','" + updatedOn + "','" + Faculty.updatedBy + "','" + DeactivationReason + "','" + DeactivatedOn + "','" + Faculty.DeactivatedBy + "','" + Faculty.WorkingType + "','" + Faculty.NOCFile + "','" + Faculty.PresentInstituteAssignedRole + "','" + Faculty.PresentInstituteAssignedResponsebility + "','" + Faculty.Accomplish1 + "','" + Faculty.Accomplish2 + "','" + Faculty.Accomplish3 + "','" + Faculty.Accomplish4 + "','" + Faculty.Accomplish5 + "','" + Faculty.Professional + "','" + Faculty.Professional2 + "','" + Faculty.Professiona3 + "','" + Faculty.MembershipNo1 + "','" + Faculty.MembershipNo2 + "','" + Faculty.MembershipNo3 + "','" + Faculty.MembershipCertificate1 + "','" + Faculty.MembershipCertificate2 + "','" + Faculty.MembershipCertificate3 + "','" + Faculty.AdjunctDesignation + "','" + Faculty.AdjunctDepartment + "','" + Faculty.PanVerificationStatus + "','" + PanDeactivationReason + "'," + Faculty.Absent + ",'" + Faculty.ModifiedPANNumber + "'," + Faculty.InvalidPANNumber + ",'" + Faculty.NoRelevantUG + "','" + Faculty.NoRelevantPG + "','" + Faculty.NORelevantPHD + "'," + Faculty.NoSCM + "," + Faculty.NoForm16 + ",'" + ModifiedDateofAppointment + "'," + Faculty.NotQualifiedAsperAICTE + "," + Faculty.Noclass + "," + Faculty.Invaliddegree + "," + Faculty.Genuinenessnotsubmitted + "," + Faculty.NotconsideredPHD + "," + Faculty.FakePHD + "," + Faculty.NoPGspecialization + "," + Faculty.OriginalsVerifiedUG + "," + Faculty.OriginalsVerifiedPG + "," + Faculty.OriginalsVerifiedPHD + "," + Faculty.FacultyVerificationStatus + ",'" + Faculty.Others1 + "','" + Faculty.Others2 + "'," + Faculty.IncompleteCertificates + ",'" + Faculty.PanStatusAfterDE + "','" + Faculty.PanReasonAfterDE + "'," + Faculty.NoSpecialization + "," + Faculty.FalsePAN + "," + Faculty.Notin116 + "," + Faculty.PHDundertakingnotsubmitted + "," + Faculty.Blacklistfaculy + "," + Faculty.DiscrepencyStatus + "," + Faculty.NoPhdUndertakingNew + ",'" + Faculty.IncometaxDocument + "','" + Faculty.PGSpecialization + "','" + Faculty.PHDUndertakingDocument + "','" + Faculty.InvalidAadhaar + "','" + Faculty.BAS + "'," + Faculty.OriginalCertificatesNotShown + ",'" + Faculty.PGSpecializationRemarks + "'," + Faculty.Xeroxcopyofcertificates + "," + Faculty.PhdUndertakingDocumentstatus + ",'" + Faculty.PhdUndertakingDocumentText + "'," + Faculty.NotIdentityfiedForanyProgram + "," + Faculty.NoSCM17 + "," + Faculty.AbsentforVerification + "," + Faculty.PhdDeskVerification + ",'" + PhdDeskReason + "'," + Faculty.ACollegeId + "," + Faculty.ADepartmentId + "," + Faculty.Jntu_PGSpecializationId + ",'" + Faculty.AIdentifiedFor + "')";
                        Insertquery = Insertquery.Replace(",,", ",(NULL),");
                        Insertquery = Insertquery.Replace(",,", ",(NULL),");
                        Insertquery = Insertquery.Replace("''", "(NULL)");
                        con.Open();
                        MySqlCommand Insertcmd = new MySqlCommand(Insertquery, con);
                        Insertcmd.ExecuteNonQuery();
                        con.Close();
                    }
                }

                //Education Table Inserting
                List<jntuh_registered_faculty_education> FacultyEducation = db.jntuh_registered_faculty_education.Where(a => a.facultyId == fid).Select(e => e).ToList();
                if (Education == 0)
                {
                    if (FacultyEducation.Count != 0 && FacultyEducation.Count != null)
                    {
                        con.Open();

                        foreach (var item in FacultyEducation)
                        {
                            string EducationQuery = "INSERT INTO jntuh_registered_faculty_educationdelete (id,facultyId,educationId,courseStudied,specialization,passedYear,marksPercentage,division,boardOrUniversity,placeOfEducation,certificate,isActive,createdOn,createdBy,updatedOn,updatedBy)";
                            string EducationcreatedOn = string.Empty;
                            if (item.createdOn.ToString() != null)
                            {
                                DateTime dt14 = DateTime.ParseExact(item.createdOn.ToString(), "MM/dd/yyyy HH:mm:ss", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None);
                                EducationcreatedOn = dt14.ToString("yyyy-MM-dd HH:mm:ss");
                            }
                            string EducationupdatedOn = string.Empty;
                            if (item.updatedOn != null)
                            {
                                DateTime dt15 = DateTime.ParseExact(item.updatedOn.ToString(), "MM/dd/yyyy HH:mm:ss", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None);
                                EducationupdatedOn = dt15.ToString("yyyy-MM-dd HH:mm:ss");
                            }

                            string courseStudied = string.Empty;
                            if (!string.IsNullOrEmpty(item.courseStudied))
                                courseStudied = item.courseStudied.Replace("'", " ");
                            string specialization = string.Empty;
                            if (!string.IsNullOrEmpty(item.specialization))
                                specialization = item.specialization.Replace("'", " ");
                            string boardOrUniversity = string.Empty;
                            if (!string.IsNullOrEmpty(item.boardOrUniversity))
                                boardOrUniversity = item.boardOrUniversity.Replace("'", " ");
                            string placeOfEducation = string.Empty;
                            if (!string.IsNullOrEmpty(item.placeOfEducation))
                                placeOfEducation = item.placeOfEducation.Replace("'", " ");

                            EducationQuery += " values ('" + item.id + "','" + item.facultyId + "','" + item.educationId + "','" + courseStudied + "','" + specialization + "','" + item.passedYear + "','" + item.marksPercentage + "','" + item.division + "','" + boardOrUniversity + "','" + placeOfEducation + "','" + item.certificate + "'," + item.isActive + ",'" + EducationcreatedOn + "','" + item.createdBy + "','" + EducationupdatedOn + "','" + item.updatedBy + "')";
                            EducationQuery = EducationQuery.Replace("''","(NULL)");
                            MySqlCommand EducationInsertcmd = new MySqlCommand(EducationQuery, con);
                            EducationInsertcmd.ExecuteNonQuery();
                            EducationQuery = string.Empty;
                        }
                        con.Close();
                    }
                }

                //EducationLog Table Inserting
                List<jntuh_registered_faculty_education_log> FacultyEducationLog = db.jntuh_registered_faculty_education_log.Where(a => a.facultyId == fid).Select(e => e).ToList();
                if (EducationLog == 0)
                {
                    if (FacultyEducationLog.Count != 0 && FacultyEducationLog.Count != null)
                    {
                        con.Open();

                        foreach (var item in FacultyEducationLog)
                        {
                            string EducationLogQuery = "INSERT INTO jntuh_registered_faculty_education_logdelete (id,hallticketnumber,facultyId,educationId,courseStudied,specialization,passedYear,marksPercentage,division,boardOrUniversity,placeOfEducation,certificate,isActive,createdOn,createdBy,updatedOn,updatedBy)";
                            string EducationLogcreatedOn = string.Empty;
                            if (item.createdOn.ToString() != null)
                            {
                                DateTime dt14 = DateTime.ParseExact(item.createdOn.ToString(), "MM/dd/yyyy HH:mm:ss", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None);
                                EducationLogcreatedOn = dt14.ToString("yyyy-MM-dd HH:mm:ss");
                            }
                            string EducationLogupdatedOn = string.Empty;
                            if (item.updatedOn != null)
                            {
                                DateTime dt15 = DateTime.ParseExact(item.updatedOn.ToString(), "MM/dd/yyyy HH:mm:ss", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None);
                                EducationLogupdatedOn = dt15.ToString("yyyy-MM-dd HH:mm:ss");
                            }

                            string courseStudied = string.Empty;
                            if (!string.IsNullOrEmpty(item.courseStudied))
                                courseStudied = item.courseStudied.Replace("'", " ");
                            string specialization = string.Empty;
                            if (!string.IsNullOrEmpty(item.specialization))
                                specialization = item.specialization.Replace("'", " ");
                            string boardOrUniversity = string.Empty;
                            if (!string.IsNullOrEmpty(item.boardOrUniversity))
                                boardOrUniversity = item.boardOrUniversity.Replace("'", " ");
                            string placeOfEducation = string.Empty;
                            if (!string.IsNullOrEmpty(item.placeOfEducation))
                                placeOfEducation = item.placeOfEducation.Replace("'", " ");

                            EducationLogQuery += " values ('" + item.id + "','" + item.hallticketnumber + "','" + item.facultyId + "','" + item.educationId + "','" + courseStudied + "','" + specialization + "','" + item.passedYear + "','" + item.marksPercentage + "','" + item.division + "','" + boardOrUniversity + "','" + placeOfEducation + "','" + item.certificate + "'," + item.isActive + ",'" + EducationLogcreatedOn + "','" + item.createdBy + "','" + EducationLogupdatedOn + "','" + item.updatedBy + "')";
                            EducationLogQuery = EducationLogQuery.Replace("''", "(NULL)");
                            MySqlCommand EducationLogInsertcmd = new MySqlCommand(EducationLogQuery, con);
                            EducationLogInsertcmd.ExecuteNonQuery();
                            EducationLogQuery = string.Empty;
                        }
                        con.Close();
                    }
                }

                //Experience Table Inserting
                List<jntuh_registered_faculty_experience> FacultyExperience = db.jntuh_registered_faculty_experience.Where(a => a.facultyId == fid).Select(e => e).ToList();
                if (Experience == 0)
                {
                    if (FacultyExperience.Count != 0 && FacultyExperience.Count != null)
                    {
                        con.Open();

                        foreach (var item in FacultyExperience)
                        {
                            string ExperienceQuery = "INSERT INTO jntuh_registered_faculty_experiencedelete (id,facultyId,collegeId,otherCollege,facultyDesignationId,OtherDesignation,facultyDateOfAppointment,facultyDateOfResignation,facultyJoiningOrder,facultyRelievingLetter,facultySalary,FacultySCMDocument,isActive,createdOn,createdBy,updatedOn,updatedBy)";
                            string ExperiencecreatedOn = string.Empty;
                            if (item.createdOn.ToString() != null)
                            {
                                DateTime dt16 = DateTime.ParseExact(item.createdOn.ToString(), "MM/dd/yyyy HH:mm:ss", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None);
                                ExperiencecreatedOn = dt16.ToString("yyyy-MM-dd HH:mm:ss");
                            }

                            string ExperienceupdatedOn = string.Empty;
                            if (item.updatedOn != null)
                            {
                                DateTime dt17 = DateTime.ParseExact(item.updatedOn.ToString(), "MM/dd/yyyy HH:mm:ss", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None);
                                ExperienceupdatedOn = dt17.ToString("yyyy-MM-dd HH:mm:ss");
                            }

                            string ExperienceDOJ = string.Empty;
                            if (item.facultyDateOfAppointment.ToString() != null)
                            {
                                DateTime dt18 = DateTime.ParseExact(item.facultyDateOfAppointment.ToString(), "MM/dd/yyyy HH:mm:ss", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None);
                                ExperienceDOJ = dt18.ToString("yyyy-MM-dd HH:mm:ss");
                            }

                            string ExperienceDOR = string.Empty;
                            if (item.facultyDateOfResignation != null)
                            {
                                DateTime dt19 = DateTime.ParseExact(item.facultyDateOfResignation.ToString(), "MM/dd/yyyy HH:mm:ss", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None);
                                ExperienceDOR = dt19.ToString("yyyy-MM-dd HH:mm:ss");
                            }

                            ExperienceQuery += " values ('" + item.Id + "','" + item.facultyId + "','" + item.collegeId + "','" + item.OtherCollege + "','" + item.facultyDesignationId + "','" + item.OtherDesignation + "','" + ExperienceDOJ + "','" + ExperienceDOR + "','" + item.facultyJoiningOrder + "','" + item.facultyRelievingLetter + "','" + item.facultySalary + "','" + item.FacultySCMDocument + "'," + item.isActive + ",'" + ExperiencecreatedOn + "','" + item.createdBy + "','" + ExperienceupdatedOn + "','" + item.updatedBy + "')";
                            ExperienceQuery = ExperienceQuery.Replace("''", "(NULL)");
                            ExperienceQuery = ExperienceQuery.Replace(",,", ",(NULL),");
                            MySqlCommand ExperienceInsertcmd = new MySqlCommand(ExperienceQuery, con);
                            ExperienceInsertcmd.ExecuteNonQuery();
                            ExperienceQuery = string.Empty;
                        }
                        con.Close();
                    }
                }

                //SubjectsTaught Table Inserting
                List<jntuh_registered_faculty_subjectstaught> Facultysubjectstaught = db.jntuh_registered_faculty_subjectstaught.Where(a => a.facultyId == fid).Select(e => e).ToList();
                if (Subjects == 0)
                {
                    if (Facultysubjectstaught.Count != 0 && Facultysubjectstaught.Count != null)
                    {
                        con.Open();

                        foreach (var item in Facultysubjectstaught)
                        {
                            string subjectstaughtQuery = "INSERT INTO jntuh_registered_faculty_subjectstaughtdelete (id,facultyId,facultysubjectsTaught,NoOfTimesSubjectTaught,remarks,IsAcitve,CreatedOn,CreatedBy,UpdatedOn,UpdatedBy)";
                            string subjectstaughtcreatedOn = string.Empty;
                            if (item.CreatedOn.ToString() != null)
                            {
                                DateTime dt20 = DateTime.ParseExact(item.CreatedOn.ToString(), "MM/dd/yyyy HH:mm:ss", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None);
                                subjectstaughtcreatedOn = dt20.ToString("yyyy-MM-dd HH:mm:ss");
                            }
                            string subjectstaughtupdatedOn = string.Empty;
                            if (item.UpdatedOn != null)
                            {
                                DateTime dt21 = DateTime.ParseExact(item.UpdatedOn.ToString(), "MM/dd/yyyy HH:mm:ss", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None);
                                subjectstaughtupdatedOn = dt21.ToString("yyyy-MM-dd HH:mm:ss");
                            }

                            string facultysubjectsTaught = string.Empty;
                            if (!string.IsNullOrEmpty(item.facultysubjectsTaught))
                                facultysubjectsTaught = item.facultysubjectsTaught.Replace("'", " ");

                            subjectstaughtQuery += " values ('" + item.Id + "','" + item.facultyId + "','" + facultysubjectsTaught + "','" + item.NoOfTimesSubjectTaught + "','" + item.remarks + "'," + item.IsAcitve + ",'" + subjectstaughtcreatedOn + "','" + item.CreatedBy + "','" + subjectstaughtupdatedOn + "','" + item.UpdatedBy + "')";
                            subjectstaughtQuery = subjectstaughtQuery.Replace("''", "(NULL)");
                            subjectstaughtQuery = subjectstaughtQuery.Replace(",,", ",(NULL),");
                            MySqlCommand subjectstaughtInsertcmd = new MySqlCommand(subjectstaughtQuery, con);
                            subjectstaughtInsertcmd.ExecuteNonQuery();
                            subjectstaughtQuery = string.Empty;
                        }
                        con.Close();
                    }
                }

                //FacultyNews Table Inserting
                List<jntuh_faculty_news> FacultyNews = db.jntuh_faculty_news.Where(a => a.facultyId == fid).Select(e => e).ToList();
                if (News == 0)
                {
                    if (FacultyNews.Count != 0 && FacultyNews.Count != null)
                    {
                        con.Open();

                        foreach (var item in FacultyNews)
                        {
                            string FacultyNewsQuery = "INSERT INTO jntuh_faculty_newsdelete (id,isNews,facultyId,title,navigateURL,isActive,isLatest,createdOn,createdBy,updatedOn,updatedBy)";
                            string FacultyNewscreatedOn = string.Empty;
                            if (item.createdOn.ToString() != null)
                            {
                                DateTime dt22 = DateTime.ParseExact(item.createdOn.ToString(), "MM/dd/yyyy HH:mm:ss", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None);
                                FacultyNewscreatedOn = dt22.ToString("yyyy-MM-dd HH:mm:ss");
                            }
                            string FacultyNewsupdatedOn = string.Empty;
                            if (item.updatedOn != null)
                            {
                                DateTime dt23 = DateTime.ParseExact(item.updatedOn.ToString(), "MM/dd/yyyy HH:mm:ss", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None);
                                FacultyNewsupdatedOn = dt23.ToString("yyyy-MM-dd HH:mm:ss");
                            }

                            FacultyNewsQuery += " values ('" + item.id + "'," + item.isNews + ",'" + item.facultyId + "','" + item.title.Replace("'", " ") + "','" + item.navigateURL + "'," + item.isActive + "," + item.isLatest + ",'" + FacultyNewscreatedOn + "','" + item.createdBy + "','" + FacultyNewsupdatedOn + "','" + item.updatedBy + "')";
                            FacultyNewsQuery = FacultyNewsQuery.Replace("''", "(NULL)");
                            FacultyNewsQuery = FacultyNewsQuery.Replace(",,", ",(NULL),");
                            MySqlCommand FacultyNewsInsertcmd = new MySqlCommand(FacultyNewsQuery, con);
                            FacultyNewsInsertcmd.ExecuteNonQuery();
                            FacultyNewsQuery = string.Empty;
                        }
                        con.Close();
                    }
                }


                con.Open();
                MySqlCommand AfterUserscmd = new MySqlCommand(Usersquery, con);
                var AfterUsers = AfterUserscmd.ExecuteScalar();
                MySqlCommand AfterMembershipcmd = new MySqlCommand(Membershipquery, con);
                var AfterMembership = AfterMembershipcmd.ExecuteScalar();
                MySqlCommand AfterRolescmd = new MySqlCommand(Rolesquery, con);
                var AfterRoles = AfterRolescmd.ExecuteScalar();
                MySqlCommand AfterRegistrationcmd = new MySqlCommand(Registrationquery, con);
                var AfterRegistration = AfterRegistrationcmd.ExecuteScalar();
                MySqlCommand AfterEducationcmd = new MySqlCommand(Educationquery, con);
                int AfterEducation = Convert.ToInt32(AfterEducationcmd.ExecuteScalar());
                MySqlCommand AfterEducationLogcmd = new MySqlCommand(EducationLogquery, con);
                int AfterEducationLog = Convert.ToInt32(AfterEducationLogcmd.ExecuteScalar());
                MySqlCommand AfterExperiencecmd = new MySqlCommand(Experiencequery, con);
                int AfterExperience = Convert.ToInt32(AfterExperiencecmd.ExecuteScalar());
                MySqlCommand AfterSubjectscmd = new MySqlCommand(SubjectTaughtquery, con);
                int AfterSubjects = Convert.ToInt32(AfterSubjectscmd.ExecuteScalar());
                MySqlCommand AfterNewscmd = new MySqlCommand(Newsquery, con);
                int AfterNews = Convert.ToInt32(AfterNewscmd.ExecuteScalar());
                con.Close();

                //Delete FacultyNews
                if (FacultyNews.Count != 0 && FacultyNews.Count != null && AfterNews != 0)
                {
                    foreach (var item in FacultyNews)
                    {
                         if(item.navigateURL != null && item.navigateURL != "")
                        {
                            System.IO.File.Delete(string.Format("{0}\\{1}", Server.MapPath(facultyNewspath), item.navigateURL));
                        }
                        db.jntuh_faculty_news.Remove(item);
                        db.SaveChanges();
                    }
                }
                //Delete Faculty Subjects
                if (Facultysubjectstaught.Count != 0 && Facultysubjectstaught.Count != null && AfterSubjects != 0)
                {
                    foreach (var item in Facultysubjectstaught)
                    {
                        db.jntuh_registered_faculty_subjectstaught.Remove(item);
                        db.SaveChanges();
                    }
                }
                //Delete Faculty Experience
                if (FacultyExperience.Count != 0 && FacultyExperience.Count != null && AfterExperience != 0)
                {
                    foreach (var item in FacultyExperience)
                    {
                        if(item.facultyJoiningOrder != null && item.facultyJoiningOrder != "")
                        {
                            System.IO.File.Delete(string.Format("{0}\\{1}", Server.MapPath(facultyexperiencedocumentspath), item.facultyJoiningOrder));
                        }
                        if(item.facultyRelievingLetter != null && item.facultyRelievingLetter != "")
                        {
                            System.IO.File.Delete(string.Format("{0}\\{1}", Server.MapPath(facultyexperiencedocumentspath), item.facultyRelievingLetter));
                        }
                        
                        db.jntuh_registered_faculty_experience.Remove(item);
                        db.SaveChanges();
                    }
                }
                //Delete Faculty Education Log
                if (FacultyEducationLog.Count != 0 && FacultyEducationLog.Count != null && AfterEducationLog != 0)
                {
                    foreach (var item in FacultyEducationLog)
                    {
                        if(item.educationId == 8 && item.certificate != null && item.certificate != "")
                        {
                            System.IO.File.Delete(string.Format("{0}\\{1}", Server.MapPath(AllcertificatesPath), item.certificate));
                        }                     
                        db.jntuh_registered_faculty_education_log.Remove(item);
                        db.SaveChanges();
                    }
                }
                //Delete Faculty Education
                if (FacultyEducation.Count != 0 && FacultyEducation.Count != null && AfterEducation !=0)
                {
                    foreach (var item in FacultyEducation)
                    {
                        if (item.certificate != null && item.certificate != "")
                        {
                            System.IO.File.Delete(string.Format("{0}\\{1}", Server.MapPath(certificatesPath), item.certificate));
                        } 
                        db.jntuh_registered_faculty_education.Remove(item);
                        db.SaveChanges();
                    }
                }
                //Delete Faculty && Principal Association
                var jntuh_college_faculty_registered = db.jntuh_college_faculty_registered.Where(a => a.RegistrationNumber == Faculty.RegistrationNumber).Select(q => q).FirstOrDefault();
                var jntuh_college_principal_registered = db.jntuh_college_principal_registered.Where(a => a.RegistrationNumber == Faculty.RegistrationNumber).Select(q => q).FirstOrDefault();
                if (jntuh_college_faculty_registered != null)
                {
                    if (jntuh_college_faculty_registered.AadhaarDocument != null && jntuh_college_faculty_registered.AadhaarDocument != "")
                    {
                        System.IO.File.Delete(string.Format("{0}\\{1}", Server.MapPath(CollegeFacultyaadhaarCardsPath), jntuh_college_faculty_registered.AadhaarDocument));
                    }                   
                    db.jntuh_college_faculty_registered.Remove(jntuh_college_faculty_registered);
                    db.SaveChanges();
                }
                if (jntuh_college_principal_registered != null)
                {
                    if (jntuh_college_principal_registered.AadhaarDocument != null && jntuh_college_principal_registered.AadhaarDocument != "")
                    {
                        System.IO.File.Delete(string.Format("{0}\\{1}", Server.MapPath(CollegePrincipalaadhaarCardsPath), jntuh_college_principal_registered.AadhaarDocument));
                    }
                    db.jntuh_college_principal_registered.Remove(jntuh_college_principal_registered);
                    db.SaveChanges();
                }

                //Delete Faculty Registration
                if (Faculty != null && AfterRegistration != null)
                {
                    if (Faculty.Photo != null && Faculty.Photo != "")
                    {
                        System.IO.File.Delete(string.Format("{0}\\{1}", Server.MapPath(photoPath), Faculty.Photo));
                    }
                    if (Faculty.PANDocument != null && Faculty.PANDocument != "")
                    {
                        System.IO.File.Delete(string.Format("{0}\\{1}", Server.MapPath(panCardsPath), Faculty.PANDocument));
                    }
                    if (Faculty.AadhaarDocument != null && Faculty.AadhaarDocument != "")
                    {
                        System.IO.File.Delete(string.Format("{0}\\{1}", Server.MapPath(aadhaarCardsPath), Faculty.AadhaarDocument));
                    }
                    if (Faculty.ProceedingDocument != null && Faculty.ProceedingDocument != "")
                    {
                        System.IO.File.Delete(string.Format("{0}\\{1}", Server.MapPath(proceedingsPath), Faculty.ProceedingDocument));
                    }
                    if (Faculty.IncometaxDocument != null && Faculty.IncometaxDocument != "")
                    {
                        System.IO.File.Delete(string.Format("{0}\\{1}", Server.MapPath(IncomeTaxcertificatesPath), Faculty.IncometaxDocument));
                    }
                    if (Faculty.Others1 != null && Faculty.Others1 != "")
                    {
                        System.IO.File.Delete(string.Format("{0}\\{1}", Server.MapPath(facultyTwoPagesFormatPath), Faculty.Others1));
                    }
                    db.jntuh_registered_faculty.Remove(Faculty);
                    db.SaveChanges();
                }
                //Delete Faculty Roles
                if (Faculty_UserInRole != null && AfterRoles != null)
                {
                    db.my_aspnet_usersinroles.Remove(Faculty_UserInRole);
                    db.SaveChanges();
                }
                //Delete Faculty Membership
                if (Faculty_Membership != null && AfterMembership != null)
                {
                    db.my_aspnet_membership.Remove(Faculty_Membership);
                    db.SaveChanges();
                }
                //Delete Faculty Errors list
                var jntuh_error_log = db.jntuh_error_log.Where(a => a.createdBy == Faculty_Users.id).Select(q => q).ToList();
                foreach (var error in jntuh_error_log)
                {
                    db.jntuh_error_log.Remove(error);
                    db.SaveChanges();
                }
                //Delete Faculty User
                if (Faculty_Users != null && AfterUsers != null)
                {
                    db.my_aspnet_users.Remove(Faculty_Users);
                    db.SaveChanges();
                }

                TempData["Success"] = "Faculty Registration Number is Deleted Successfully.";
               // return RedirectToAction("GetRegistation");

            }
            return RedirectToAction("GetRegistation");
        }

        [Authorize(Roles = "Admin")]
        public ActionResult DeleteCollgeRegistredFaculty(string fregno)
        {
            TempData["nocollege"] = null;
            TempData["facultydeletesuccess"] = null;
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            if (!String.IsNullOrEmpty(fregno))
            {
                jntuh_college_faculty_registered jntuh_college_faculty_registered =
                    db.jntuh_college_faculty_registered.Where(r => r.RegistrationNumber == fregno.Trim())
                        .Select(s => s)
                        .FirstOrDefault();
                if (jntuh_college_faculty_registered != null)
                {
                    db.jntuh_college_faculty_registered.Remove(jntuh_college_faculty_registered);
                    db.SaveChanges();
                    //Tracking
                    //jntuh_attendence_registrationnumberstracking jntuh_attendence_registrationnumberstracking=new jntuh_attendence_registrationnumberstracking();
                    //jntuh_attendence_registrationnumberstracking.RegistrationNumber = fregno.Trim();
                    //jntuh_attendence_registrationnumberstracking.DepartmentId = jntuh_college_faculty_registered.DepartmentId;
                    //jntuh_attendence_registrationnumberstracking.FacultyType = userID.ToString();
                    //jntuh_attendence_registrationnumberstracking.Reasion = "Faculty Request for Delete";
                    //jntuh_attendence_registrationnumberstracking.ActionType = "DeletebyAdmin";
                    //jntuh_attendence_registrationnumberstracking.FacultyStatus = "Y";
                    //jntuh_attendence_registrationnumberstracking.Createdon =DateTime.Now;
                    //db.jntuh_attendence_registrationnumberstracking.Add(jntuh_attendence_registrationnumberstracking);
                    //db.SaveChanges();
                    jntuh_college_facultytracking objART = new jntuh_college_facultytracking();
                    //jntuh_attendence_registrationnumberstracking objART = new jntuh_attendence_registrationnumberstracking();
                    objART.academicYearId = 10;
                    objART.collegeId = jntuh_college_faculty_registered.collegeId;
                    objART.RegistrationNumber = fregno.Trim();
                    objART.DepartmentId = jntuh_college_faculty_registered.DepartmentId;
                    objART.SpecializationId = jntuh_college_faculty_registered.SpecializationId;
                    objART.ActionType = 2;
                    objART.FacultyType = "Faculty";
                    objART.FacultyStatus = "Y";
                    objART.Reasion = null;
                    // objART.FacultyStatus = "Y";
                    //  objART.Reasion = "Faculty Deleted by College Successfully.";
                    objART.FacultyJoinDate = jntuh_college_faculty_registered.createdOn;
                    objART.Createdon = DateTime.Now;
                    objART.CreatedBy = userID;
                    objART.Updatedon = null;
                    objART.UpdatedBy = null;
                    db.jntuh_college_facultytracking.Add(objART);
                    db.SaveChanges();
                    TempData["facultydeletesuccess"] = fregno.Trim() + " College Assocation Deleted Successfully..";
                    return RedirectToAction("FindRegistrationNumber");
                }
            }
            TempData["nocollege"] = "No College Assocation...";
            return RedirectToAction("FindRegistrationNumber");
        }
        //Delete Registration Faculty
        [Authorize(Roles = "Admin")]
        public ActionResult DeleteRegistratedFaculty()
        {
            FacultyRegistration FacultyRegistration = new FacultyRegistration();
            return View(FacultyRegistration);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult DeleteRegistratedFaculty(FacultyRegistration regno)
        {
            //FacultyRegistration FacultyRegistration = new FacultyRegistration();
            List<jntuh_registered_faculty> FacultyRegistrationlist =
                db.jntuh_registered_faculty.Where(d => d.RegistrationNumber.Trim() == regno.RegistrationNumber.Trim())
                    .Select(s => s)
                    .ToList();
            jntuh_registered_faculty jntuh_registered_faculty = new jntuh_registered_faculty();

            ViewBag.GetfacultyDetails = FacultyRegistrationlist;
            return View(regno);
        }
        [Authorize(Roles = "Admin")]
        public ActionResult DeleteRegFaculty(string fregno)
        {
            //FacultyRegistration FacultyRegistration = new FacultyRegistration();
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            jntuh_registered_faculty jntuh_registered_facultydata =
                db.jntuh_registered_faculty.Where(d => d.RegistrationNumber.Trim() == fregno.Trim())
                    .Select(s => s)
                    .FirstOrDefault();
            List<jntuh_registered_faculty_education> facultyeducationdata = db.jntuh_registered_faculty_education.Where(d => d.facultyId == jntuh_registered_facultydata.id)
                    .Select(s => s)
                    .ToList();
            int facultyuserid = jntuh_registered_facultydata.UserId;
            my_aspnet_usersinroles my_aspnet_usersinroles =
            db.my_aspnet_usersinroles.Where(u => u.userId == facultyuserid).Select(s => s).FirstOrDefault();
            my_aspnet_membership my_aspnet_membership =
                db.my_aspnet_membership.Where(m => m.userId == facultyuserid).Select(m => m).FirstOrDefault();
            my_aspnet_users my_aspnet_users =
                db.my_aspnet_users.Where(m => m.id == facultyuserid).Select(m => m).FirstOrDefault();
            if (facultyeducationdata.Count != 0)
            {
                foreach (var item in facultyeducationdata)
                {
                    db.jntuh_registered_faculty_education.Remove(item);
                }
                db.SaveChanges();
                if (jntuh_registered_facultydata != null)
                {
                    db.jntuh_registered_faculty.Remove(jntuh_registered_facultydata);
                    db.SaveChanges();
                    if (my_aspnet_usersinroles != null)
                    {
                        db.my_aspnet_usersinroles.Remove(my_aspnet_usersinroles);
                        db.SaveChanges();
                        if (my_aspnet_membership != null)
                        {
                            db.my_aspnet_membership.Remove(my_aspnet_membership);
                            db.SaveChanges();
                            if (my_aspnet_users != null)
                            {
                                db.my_aspnet_users.Remove(my_aspnet_users);
                                db.SaveChanges();
                                ViewBag.Success = "Faculty Delete Success.";
                            }
                        }
                    }
                }
            }
            return RedirectToAction("DeleteRegistratedFaculty");
        }
        [Authorize(Roles = "Admin")]
        public ActionResult CheckEditOptionFaculty()
        {
            return View();
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult CheckEditOptionFaculty(FacultyRegistration regno)
        {
            if (!String.IsNullOrEmpty(regno.RegistrationNumber))
            {
                int userID =
                    db.jntuh_registered_faculty.Where(u => u.RegistrationNumber == regno.RegistrationNumber.Trim())
                        .Select(s => s.UserId)
                        .FirstOrDefault();
                int facultyId = db.jntuh_registered_faculty.Where(f => f.UserId == userID)
                                         .Select(f => f.id)
                                         .FirstOrDefault();
                string FacultyType =
                    db.jntuh_registered_faculty.Where(f => f.UserId == userID)
                        .Select(f => f.type)
                        .FirstOrDefault();
                string fid = Utilities.EncryptString(facultyId.ToString(),
                    WebConfigurationManager.AppSettings["CryptoKey"]);

                if (FacultyType != "Adjunct")
                    return RedirectToAction("FacultyNew", "OnlineRegistration", new { fid = fid });
                else
                    return RedirectToAction("AdjunctFacty", "OnlineRegistration",
                        new { fid = fid });

            }
            else
            {
                TempData["Error"] = "Invalid Registration Number";
                return View();
            }
            return View();
        }

        #region PAN Number Changeing

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public ActionResult Pannumberchange()
        {
            pannumberchangeing panchange = new pannumberchangeing();
            return View(panchange);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult Pannumberchange(pannumberchangeing panchange)
        {
            int userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            var orginalpandata =
                db.jntuh_registered_faculty.Where(r => r.PANNumber == panchange.correctpannumber)
                    .Select(s => s)
                    .FirstOrDefault();
            string panCardsPath = "~/Content/Upload/Faculty/PANCARDS";
            jntuh_registered_faculty registratedfaculty =
                db.jntuh_registered_faculty.Where(
                    r =>
                        r.RegistrationNumber == panchange.RegistrationNumber.Trim() &&
                        r.PANNumber == panchange.worngpannumbe.Trim()).Select(s => s).FirstOrDefault();
            if (registratedfaculty==null)
            {
                TempData["ERROR"] = "No data found with this " + panchange.RegistrationNumber + " and " + panchange.worngpannumbe;
                return RedirectToAction("Pannumberchange");
            }
            if (registratedfaculty.Blacklistfaculy == true || registratedfaculty.AbsentforVerification == true)
            {
                TempData["ERROR"] = "we can't change update the Data because Blacklistfaculy/AbsentforVerification.";
                return RedirectToAction("Pannumberchange");
            }
            //PAN Card Document Saving
            if (panchange.pandocument != null && registratedfaculty!=null)
            {
                if (!Directory.Exists(Server.MapPath(panCardsPath)))
                {
                    Directory.CreateDirectory(Server.MapPath(panCardsPath));
                }
                var ext = Path.GetExtension(panchange.pandocument.FileName);

                if (ext.ToUpper().Equals(".JPEG") || ext.ToUpper().Equals(".JPG"))
                {
                    string fileName = DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" +
                                      registratedfaculty.FirstName.Substring(0, 1) + "-" + registratedfaculty.LastName.Substring(0, 1);
                    panchange.pandocument.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(panCardsPath), fileName, ext));
                    registratedfaculty.PANDocument = string.Format("{0}{1}", fileName, ext);
                    registratedfaculty.updatedBy = userId;
                    registratedfaculty.updatedOn = DateTime.Now;
                    if (String.IsNullOrEmpty(registratedfaculty.InStatus))
                    {
                        registratedfaculty.InStatus = panchange.Reson.Trim();
                    }
                    else
                    {
                        registratedfaculty.InStatus += "," + panchange.Reson.Trim();
                    }
                    db.Entry(registratedfaculty).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Success"] = "PAN Document Changed successfully";
                    return RedirectToAction("Pannumberchange");
                }
            }
            if (orginalpandata!=null)
            {
                TempData["ERROR"] = orginalpandata.PANNumber + "This PAN Number have" + orginalpandata .RegistrationNumber + "Registaration Number";
                return RedirectToAction("Pannumberchange");
            }
            else
            {
                string PANNumber = panchange.correctpannumber.Trim();
                string SendPansToAPI = string.Empty;
                string CheckingPANNumber =
                    db.jntuh_registered_faculty.Where(F => F.PANNumber == PANNumber.Trim())
                        .Select(F => F.PANNumber)
                        .FirstOrDefault();
                if (CheckingPANNumber != null && CheckingPANNumber != "")
                {
                    if (CheckingPANNumber.Trim() == PANNumber.Trim())
                    {
                        TempData["ERROR"] = "PAN Number already registred.";
                        return RedirectToAction("Pannumberchange");
                    }
                }
                if (!string.IsNullOrEmpty(PANNumber))
                {
                    var panstatusdb =
                       db.jntuh_pan_status.Where(p => p.PANNumber.Trim() == PANNumber.Trim() && p.PANStatus == "E")
                           .Select(s => s)
                           .FirstOrDefault();
                    if (panstatusdb!=null&&panstatusdb.PANStatus == "E")
                    {
                        registratedfaculty.FirstName = panstatusdb.FirstName;
                        registratedfaculty.LastName = panstatusdb.LastName;
                        registratedfaculty.MiddleName = panstatusdb.MiddleName;
                        registratedfaculty.PANNumber = PANNumber.Trim();
                        registratedfaculty.PanStatus = "E";
                        registratedfaculty.updatedOn =DateTime.Now;
                        registratedfaculty.updatedBy = userId;
                        if (String.IsNullOrEmpty(registratedfaculty.InStatus))
                        {
                            registratedfaculty.InStatus = panchange.Reson.Trim();
                        }
                        else
                        {
                            registratedfaculty.InStatus += "," + panchange.Reson.Trim();
                        }
                        db.Entry(registratedfaculty).State = EntityState.Modified;
                        db.SaveChanges();
                        TempData["Success"] = "PAN Number Changed successfully";
                        return RedirectToAction("Pannumberchange");
                    }
                    else
                    {
                        var UserID = "V0131101";
                        SendPansToAPI = UserID + "^" + PANNumber;
                        var pandetails = SendPanRequest1(SendPansToAPI);
                        string[] pandetails1 = pandetails.Split(',');
                        jntuh_pan_status PANStatus = new jntuh_pan_status();
                        PANStatus.PANNumber = pandetails1[0].ToString();
                        PANStatus.PANStatus = pandetails1[1].ToString();
                        PANStatus.FirstName = pandetails1[3].ToString();
                        PANStatus.LastName = pandetails1[2].ToString();
                        PANStatus.MiddleName = pandetails1[4].ToString();
                        // PANStatus.MiddleName = "";
                        PANStatus.Title = pandetails1[5].ToString();
                        if (!string.IsNullOrEmpty(pandetails1[6]))
                        {
                            PANStatus.LastUpdated = UAAAS.Models.Utilities.DDMMYY2MMDDYY(pandetails1[6]);
                            //PANStatus.LastUpdated = Convert.ToDateTime(pandetails1[6]);
                        }
                        PANStatus.IsActive = true;
                        PANStatus.createdby = 1;
                        PANStatus.CreateOn = DateTime.Now;
                        db.jntuh_pan_status.Add(PANStatus);
                        db.SaveChanges();
                        var panStatus = pandetails1[1].ToString();
                        if (panStatus != "E")
                        {
                              TempData["ERROR"] = "PAN Number Not Valid";
                              return RedirectToAction("Pannumberchange");
                        }
                        else
                        {
                            registratedfaculty.FirstName = PANStatus.FirstName;
                            registratedfaculty.LastName = PANStatus.LastName;
                            registratedfaculty.MiddleName = PANStatus.MiddleName;
                            registratedfaculty.PANNumber = PANNumber.Trim();
                            registratedfaculty.PanStatus = "E";
                            registratedfaculty.updatedOn = DateTime.Now;
                            registratedfaculty.updatedBy = userId;
                            if (String.IsNullOrEmpty(registratedfaculty.InStatus))
                            {
                                registratedfaculty.InStatus = panchange.Reson.Trim();
                            }
                            else
                            {
                                registratedfaculty.InStatus += "," + panchange.Reson.Trim();
                            }
                            db.Entry(registratedfaculty).State = EntityState.Modified;
                            db.SaveChanges();
                            TempData["Success"] = "PAN Number Changed successfully";
                            return RedirectToAction("Pannumberchange");
                        }
                    }

                }
            }
            TempData["ERROR"] = "PAN Number Changed Failed";
            return RedirectToAction("Pannumberchange");
        }

        protected string SendPanRequest1(string Data)
        {
            string Details = string.Empty;
            System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();
            byte[] bytes = encoding.GetBytes(Data);
            var path = filePath;
            //var path = Server.MapPath("jntuh_Sign.p12");
            X509Certificate2 uidCert = new X509Certificate2(filePath, "123", X509KeyStorageFlags.MachineKeySet);
            byte[] sig = Sign(bytes, uidCert);
            String Signature = Convert.ToBase64String(sig);
            ServicePointManager.ServerCertificateValidationCallback = delegate(object s, X509Certificate certificate,
                X509Chain chain, SslPolicyErrors sslPolicyErrors)
            {
                return true;
            };
            string post_data = "data=" + Data + "&signature=" +
                               System.Web.HttpContext.Current.Server.UrlEncode(Signature);
            string url = "https://59.163.46.2/TIN/PanInquiryBackEnd";
            //string url = "https://121.240.9.19/TIN/PanInquiryBackEnd";
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            // create a request
            HttpWebRequest request = (HttpWebRequest)
                WebRequest.Create(url);
            request.KeepAlive = false;
            request.ProtocolVersion = HttpVersion.Version10;
            request.Method = "POST";
            // turn our request string into a byte stream
            byte[] postBytes = Encoding.UTF8.GetBytes(post_data);
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = postBytes.Length;
            Stream requestStream = request.GetRequestStream();
            // now send it
            requestStream.Write(postBytes, 0, postBytes.Length);
            requestStream.Close();
            HttpWebResponse res = (HttpWebResponse)request.GetResponse();
            StreamReader sr = new StreamReader(res.GetResponseStream(), System.Text.Encoding.Default);
            string backstr = sr.ReadToEnd();
            Logger Log = new Logger();
            Log.Log(backstr);
            string panRes = backstr;
            string Pan1 = string.Empty;
            string[] resp = panRes.Split('^');
            if (resp[0] == "1")
            {
                int strLength = resp.Length;
                int LastRowCount = 0;
                if (strLength == 11)
                {
                    LastRowCount = 2;

                }
                else if (strLength == 21)
                {
                    LastRowCount = 3;

                }
                else if (strLength == 31)
                {
                    LastRowCount = 4;
                }
                else if (strLength == 41)
                {
                    LastRowCount = 5;
                }
                else if (strLength == 51)
                {
                    LastRowCount = 6;
                }
                int j = 0;
                for (int k = 1; k < LastRowCount; k++)
                {
                    Details = resp[j + 1] + "," + resp[j + 2] + "," + resp[j + 3] + "," + resp[j + 4] + "," +
                              resp[j + 5] + "," + resp[j + 6] + "," + resp[j + 7] + "," + resp[j + 8] + "," +
                              resp[j + 9] + "," + resp[j + 10];



                    j = j + k;
                    j = k * 10;
                }

            }
            string[] Strcount = Details.Split(',');
            return Details;
        }
        public static byte[] Sign(byte[] data, X509Certificate2 certificate)
        {
            if (data == null)
                throw new ArgumentNullException("data");
            if (certificate == null)
                throw new ArgumentNullException("certificate");

            // setup the data to sign
            ContentInfo content = new ContentInfo(data);
            SignedCms signedCms = new SignedCms(content, false);
            CmsSigner signer = new CmsSigner(SubjectIdentifierType.IssuerAndSerialNumber, certificate);
            // create the signature
            signedCms.ComputeSignature(signer);
            return signedCms.Encode();
        }
        #endregion
    }

    public class pannumberchangeing
    {
        public string RegistrationNumber { get; set; }
        public string worngpannumbe { get; set; }
        public string correctpannumber { get; set; }
        public HttpPostedFileBase pandocument { get; set; }
        public string pandocumentfile { get; set; }
        public string Reson { get; set; }
    }
    //public class CheckFacultyData
    //{
    //    public bool SubjectsTaughtTable { get; set; }
    //    public bool ExperienceTable { get; set; }
    //    public bool EducationTable { get; set; }
    //    public bool RegistrationTable { get; set; }
    //    public bool RolesinUsersTable { get; set; }
    //    public bool MembershipTable { get; set; }
    //    public bool UsersTable { get; set; }
    //}
}
