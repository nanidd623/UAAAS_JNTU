using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace UAAAS.Models
{
    public class FacultyRegistration
    {
        public FacultyRegistration()
        {
            this.jntuh_registered_faculty_education = new HashSet<jntuh_registered_faculty_education>();
            //this.jntuh_registered_faculty_education_log=new HashSet<jntuh_registered_faculty_education_log>();
        }

        public int id { get; set; }
        public string Type { get; set; }
        public int UserId { get; set; }

        public int? CollegeId { get; set; }
        public string CollegeName { get; set; }
        public string CollegeCode { get; set; }

        #region Faculty Information 

        [Display(Name = "Faculty Registration ID")]
        public string RegistrationNumber { get; set; }

        [Display(Name = "Faculty Unique ID")]
        public string UniqueID { get; set; }

        [Required(ErrorMessage = "*")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "Must be 1-50 characters long.")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        //LastName-MiddleName
        [StringLength(50, ErrorMessage = "Maximum 50 characters")]
        [Display(Name = "Middle Name")]
        public string MiddleName { get; set; }

        //Surname-LastName
        [Required(ErrorMessage = "*")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "Must be 1-50 characters long.")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "*")]
        [Display(Name = "Gender")]
        public int? GenderId { get; set; }

        [Required(ErrorMessage = "*")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Must be 3-100 characters long.")]
        [Display(Name = "Father's Name")]
        public string FatherOrhusbandName { get; set; }

        [Required(ErrorMessage = "*")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Must be 3-50 characters long.")]
        [Display(Name = "Mother's Name")]
        public string MotherName { get; set; }

        [Display(Name = "Date of Birth")]
        //[DataType(DataType.Date)]
        //[DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? DateOfBirth { get; set; }

        [Required(ErrorMessage = "*")]
        [Display(Name = "Date of Birth")]
        public string facultyDateOfBirth { get; set; }

        
        [StringLength(10, ErrorMessage = "Must be 10 characters")]
        //[RegularExpression(@"\w{10}", ErrorMessage = "Invalid PAN number")]
        [Required(ErrorMessage = "*")]
        [RegularExpression(@"[A-Z]{3}[P][A-Z]{1}\d{4}[A-Z]{1}", ErrorMessage = "Invalid PAN number")]
        [Display(Name = "PAN Number")]
        [Remote("CheckPanNumber", "NewOnlineRegistration", HttpMethod = "POST", ErrorMessage = "PAN Number already registred.")]
        public string PANNumber { get; set; }

        public string PanStatus { get; set; }

         [Required(ErrorMessage = "*")]
        [StringLength(16, ErrorMessage = "Must be 12 characters")]
        [RegularExpression(@"\d{12}", ErrorMessage = "Invalid AADHAAR number")]
        [Display(Name = "Aadhaar Number")]
        [Remote("NewAadhaarNumberCheck", "NewOnlineRegistration", HttpMethod = "POST", ErrorMessage = "Aadhaar Number Already Exists.")]
        public string AadhaarNumber { get; set; }


        [StringLength(16, ErrorMessage = "Must be 12 characters")]
        [RegularExpression(@"\d{12}", ErrorMessage = "Invalid AADHAAR number")]
        [Display(Name = "Aadhaar Number")]
        [Remote("CheckAadhaarNumber", "OnlineRegistration", AdditionalFields = "RegistrationNumber", HttpMethod = "POST", ErrorMessage = "Aadhaar Number Already Exists.")]
        public string EditAadhaarNumber { get; set; }


        [Required(ErrorMessage = "*")]
        [StringLength(10, ErrorMessage = "Maximum 10 characters")]
        [RegularExpression(@"\d{10}", ErrorMessage = "Invalid mobile")]
        [Display(Name = "Mobile No")]
        [Remote("NewMobileNumberCheck", "NewOnlineRegistration",HttpMethod = "POST", ErrorMessage = "Mobile Number  Already Exists.")]
        public string Mobile { get; set; }


       // [Required(ErrorMessage = "*")]
        [StringLength(10, ErrorMessage = "Maximum 10 characters")]
        [RegularExpression(@"\d{10}", ErrorMessage = "Invalid mobile")]
        [Display(Name = "Mobile No")]
        [Remote("CheckMobileNumber", "OnlineRegistration", AdditionalFields = "RegistrationNumber", HttpMethod = "POST", ErrorMessage = "Mobile Number  Already Exists.")]
        public string EditMobile { get; set; }

        [Required(ErrorMessage = "*")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Must be 6-100 characters long.")]
        [DataType(DataType.EmailAddress)]
        [RegularExpression(@"[a-z0-9._%+-]+@[a-z0-9.-]+\.[a-z]{2,4}", ErrorMessage = "Invalid email")]
        [Display(Name = "Email Address")]
        [Remote("CheckEmail", "NewOnlineRegistration", HttpMethod = "POST", ErrorMessage = "Email is Already exists.")]
        public string Email { get; set; }

       // [Required(ErrorMessage = "*")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Must be 6-100 characters long.")]
        [DataType(DataType.EmailAddress)]
        [RegularExpression(@"[a-z0-9._%+-]+@[a-z0-9.-]+\.[a-z]{2,4}", ErrorMessage = "Invalid email")]
        [Display(Name = "Email Address")]
        // [Remote("EditCheckEmail", "OnlineRegistration",AdditionalFields = "RegistrationNumber",, HttpMethod = "POST", ErrorMessage = "Email is Already exists.")]
        public string EditEmail { get; set; }

        [Required(ErrorMessage = "*")]
        [Display(Name = "Photo")]
        //[ValidateFacultyImage(ErrorMessage = "Please select a JPEG image smaller than 50KB")]
        public HttpPostedFileBase Photo { get; set; }

        public string facultyPhoto { get; set; }

        [Required(ErrorMessage = "*")]
        [Display(Name = "PAN Card Document")]
        public HttpPostedFileBase PANCardDocument { get; set; }

        public string facultyPANCardDocument { get; set; }

        [Required(ErrorMessage = "*")]
        [Display(Name = "Aadhaar Card Document")]
        public HttpPostedFileBase AadhaarCardDocument { get; set; }

        public string facultyAadhaarCardDocument { get; set; }

        //[Required(ErrorMessage = "*")]
        [Display(Name = "Category")]
        public string facultyCategory { get; set; }

        #endregion
       

        //[Required(ErrorMessage = "*")]
        [Display(Name = "Presently Working(If 'YES' Upload NOC from parent organization)")]
        public bool? WorkingStatus { get; set; }

        //[Required(ErrorMessage = "*")]
        [StringLength(500, ErrorMessage = "Maximum 500 characters")]
        public string OrganizationName { get; set; }

        //[Required(ErrorMessage = "*")]
        [Display(Name = "Designation")]
        public int? DesignationId { get; set; }

        public string designation { get; set; }

        [Display(Name = "Other Designation")]
        public string OtherDesignation { get; set; }

        //[Required(ErrorMessage = "*")]
        [Display(Name = "Department")]
        public int? DepartmentId { get; set; }

        public string department { get; set; }

        public string Degree { get; set; }

        [Display(Name = "Other Department")]
        public string OtherDepartment { get; set; }

        [StringLength(100, ErrorMessage = "Maximum 100 characters")]
        [Display(Name = "Honorarium")]
        public string GrossSalary { get; set; }

        [Display(Name = "Date of Appointment")]
        //[DataType(DataType.Date)]
        //[DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? DateOfAppointment { get; set; }
      
        [Display(Name = "Date of Appointment")]
        public string facultyDateOfAppointment { get; set; }
        
        public string facultyAppointmentLetter { get; set; }
     
        [Display(Name = "Ratified by JNTUH")]
        public bool? isFacultyRatifiedByJNTU { get; set; }

        [Display(Name = "Date of Ratification")]
        //[DataType(DataType.Date)]
        //[DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? DateOfRatification { get; set; }

        [Display(Name = "Date of Ratification")]
        public string facultyDateOfRatification { get; set; }

        [StringLength(25, ErrorMessage = "Must be 25 characters")]
        [Display(Name = "Proceedings No")]
        public string ProceedingsNo { get; set; }

        [Display(Name = "Selection Committee Proceedings Document")]
        public HttpPostedFileBase SelectionCommitteeProceedingsDocument { get; set; }

        [Display(Name = "Selection Committee Proceedings")]
        public string SelectionCommitteeProcedings { get; set; }

        [StringLength(100, ErrorMessage = "Maximum 100 characters")]
        [Display(Name = "AICTE Faculty Id")]
        public string AICTEFacultyId { get; set; }

        [Display(Name = "Total Experience")]
        public int? TotalExperience { get; set; }
        public string showTotalExperience { get; set; }
     
        [Display(Name = "Experience Present Institution")]
        public int? TotalExperiencePresentCollege { get; set; }
        public string showPresentCollegeExperiance { get; set; }


        [StringLength(1000, ErrorMessage = "Maximum 1000 characters")]
        [Display(Name = "National")]
        public string National { get; set; }

        [StringLength(1000, ErrorMessage = "Maximum 1000 characters")]
        [Display(Name = "International")]
        public string InterNational { get; set; }

        [StringLength(1000, ErrorMessage = "Maximum 1000 characters")]
        [Display(Name = "Citation")]
        public string Citation { get; set; }

        [StringLength(1000, ErrorMessage = "Maximum 1000 characters")]
        [Display(Name = "Awards")]
        public string Awards { get; set; }

       
       // [Required(ErrorMessage = "*")]
        [Display(Name = "Status")]
        public bool isActive { get; set; }

        public string IsStatus { get; set; }

        //[Required(ErrorMessage = "*")]
        [Display(Name = "Status")]
        public bool? isApproved { get; set; }


        public Nullable<System.DateTime> createdOn { get; set; }
        public Nullable<int> createdBy { get; set; }
        public Nullable<System.DateTime> updatedOn { get; set; }
        public Nullable<int> updatedBy { get; set; }

        public string DeactivationReason { get; set; }

        public int? Deactivedby { get; set; }
        public DateTime? DeactivedOn { get; set; }

        #region Not Used Coloumns
       // [Required(ErrorMessage = "*")]
        public string WorkingType { get; set; }
        public string NOCFile { get; set; }
       // [Required(ErrorMessage = "*")]
        [Display(Name = "Roles")]
        public string PresentInstituteAssignedRole { get; set; }
       // [Required(ErrorMessage = "*")]
        [Display(Name = "Responsibilities")]
        public string PresentInstituteAssignedResponsebility { get; set; }
       // [Required(ErrorMessage = "*")]
        public string Accomplish1 { get; set; }
      //  [Required(ErrorMessage = "*")]
        public string Accomplish2 { get; set; }
       // [Required(ErrorMessage = "*")]
        public string Accomplish3 { get; set; }
        public string Accomplish4 { get; set; }
        public string Accomplish5 { get; set; }
        public string Professional { get; set; }
        public string Professional2 { get; set; }
        public string Professiona3 { get; set; }
        public string MembershipNo1 { get; set; }
        public string MembershipNo2 { get; set; }
        public string MembershipNo3 { get; set; }
        public string MembershipCertificate1 { get; set; }
        public string MembershipCertificate2 { get; set; }
        public string MembershipCertificate3 { get; set; }

        #endregion


        [Display(Name = "Designation in the Parent Organization")]
        public string AdjunctDesignation { get; set; }

        [Display(Name = "Parent Organization Department")]
        public string AdjunctDepartment { get; set; }

        public string PanVerificationStatus { get; set; }
        public string PanDeactivationReasion { get; set; }

        public bool Absent { get; set; }
        [RegularExpression(@"[A-Z]{3}[P][A-Z]{1}\d{4}[A-Z]{1}", ErrorMessage = "Invalid PAN number")]
        [Display(Name = "ModifiedPANNo")]
        public string ModifiedPANNo { get; set; }

        public bool InvalidPANNo { get; set; }

        public string NORelevantUG { get; set; }
        public string NORelevantPG { get; set; }
        public string NORelevantPHD { get; set; }
        public bool? NoSCM { get; set; }
        public bool NoSCMFlag { get; set; }
        public bool NOForm16 { get; set; }
        public bool NOForm26AS { get; set; }
        public bool Covid19 { get; set; }
        public bool Maternity { get; set; }
        public string facultyRelievingDate { get; set; }
        public DateTime MOdifiedDateofAppointment { get; set; }
        public string MOdifiedDateofAppointment1 { get; set; }
        public bool NOTQualifiedAsPerAICTE { get; set; }
        public bool PHDundertakingnotsubmitted { get; set; }
        public bool MultipleReginSamecoll { get; set; }
        public bool MultipleReginDiffcoll { get; set; }
        public bool SamePANUsedByMultipleFaculty { get; set; }
        public bool PhotocopyofPAN { get; set; }
        public bool AppliedPAN { get; set; }
        public bool LostPAN { get; set; }
        public bool OriginalsVerifiedUG { get; set; }
        public bool OriginalsVerifiedPG { get; set; }
        public bool OriginalsVerifiedPHD { get; set; }
        public bool? FacultyVerificationStatus { get; set; }
        public string appealsupportingdocument { get; set; }

        public string Others1 { get; set; }
        public string Others2 { get; set; }

        public bool InCompleteCeritificates { get; set; }
        public bool Noclassinugorpg { get; set; }

        public string PanStatusAfterDE { get; set; }
        public string PanReasonAfterDE { get; set; }

        public bool NOspecializationFlag { get; set; }
        public bool FalsePAN { get; set; }

        public bool? BlacklistFaculty { get; set; }

        public bool BlacklistFlag { get; set; }

        // [Required(ErrorMessage = "*")]
        public HttpPostedFileBase IncomeTaxFileUpload { get; set; }
        [Display(Name = "Income Tax 26 As / Form16")]
        public string IncomeTaxFileview { get; set; }

        public bool NOrelevantUgFlag { get; set; }
        public bool NOrelevantPgFlag { get; set; }
        public bool NOrelevantPhdFlag { get; set; }

        public bool BasStatusFlag { get; set; }
        public bool NophdUndertakingFlag { get; set; }
    

        public int? SpecializationId { get; set; }
        public string SpecializationName { get; set; }

       // [Required(ErrorMessage = "Select PG Specialization")]
        [Display(Name = "PG Specialization")]
        public int? PGSpecialization { get; set; }

        public string PGSpecializationName { get; set; }
        
        public HttpPostedFileBase PHDUndertakingDocument { get; set; }
        public string PHDUndertakingDocumentView { get; set; }

        public string Basstatus { get; set; }
        public string BasstatusOld { get; set; }

        public string DepartmentName { get; set; }
        public string OthersPGSpecilizationName { get; set; }

        public bool OriginalCertificatesnotshownFlag { get; set; }
        public bool XeroxcopyofcertificatesFlag { get; set; }
        public bool NotIdentityFiedForAnyProgramFlag { get; set; }
        public bool PhdDeskVerification { get; set; }
        public string PhdDeskVerCondition { get; set; }
        public bool NoSCM17Flag { get; set; }
        public bool NoForm16Verification { get; set; }
        public bool? VerificationStatus { get; set; }
        public bool VerificationStatusFlag { get; set; }
        public bool Phd2pages { get; set; }
        public bool PhdUndertakingDocumentstatus { get; set; }
        public string PhdUndertakingDocumentText { get; set; }

        public string IdentfiedFor { get; set; }
        public string SpecializationIdentfiedFor { get; set; }

        public bool NoClass { get; set; }
        public bool InvalidDegree { get; set; }
        public bool GenuinenessnotSubmitted { get; set; }
        public bool FakePhd { get; set; }
        public bool NotconsiderPhd { get; set; }
        public bool NoPgSpecialization { get; set; }
        public string InvalidAadhaar { get; set; }
        public bool InvalidAadhaarFlag { get; set; }

        #region Login Details
        //[Required(ErrorMessage = "*")]
        [StringLength(15, MinimumLength = 5, ErrorMessage = "Must be 5-15 characters long.")]
        [Display(Name = "Username")]
        [RegularExpression(@"[a-zA-Z0-9_]{1,15}", ErrorMessage = "Allowed characters : 'alphabets', 'numbers', '_'")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "*")]
        [StringLength(15, MinimumLength = 6, ErrorMessage = "Must be 6-15 characters long.")]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [System.Web.Mvc.Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        #endregion

        #region Faculty Education Details
      
        //SSC Education Details
        [Required(ErrorMessage = "*")]
        [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "Invalid HallTicket number")]
        [Display(Name = "HallTicket Number")]
        //[Remote("CheckHallTicket", "NewOnlineRegistration", HttpMethod = "POST", ErrorMessage = "HallTicket Number already registred.", AdditionalFields = "RegistrationNumber")]
        public string SSC_HallticketNo { get; set; }
        public int SSC_educationId { get; set; }
        public string SSC_educationName { get; set; }
        public string SSC_studiedEducation { get; set; }
        public Nullable<int> SSC_passedYear { get; set; }
        public Nullable<decimal> SSC_percentage { get; set; }
        public Nullable<int> SSC_division { get; set; }
        public string SSC_university { get; set; }
        public string SSC_place { get; set; }
        [Required(ErrorMessage = "*")]
        public HttpPostedFileBase SSC_certificate { get; set; }
        public string SSC_facultyCertificate { get; set; }
        public string SSC_specialization { get; set; }

        //UG Education Details
       [Required(ErrorMessage = "*")]
        [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "Invalid HallTicket number")]
        [Display(Name = "HallTicket Number")]
       // [Remote("CheckHallTicket", "NewOnlineRegistration", HttpMethod = "POST", ErrorMessage = "HallTicket Number already registred.", AdditionalFields = "RegistrationNumber")]
        public string UG_HallticketNo { get; set; }
        public int UG_educationId { get; set; }
        public string UG_educationName { get; set; }
        public string UG_studiedEducation { get; set; }
        public Nullable<int> UG_passedYear { get; set; }
        public Nullable<decimal> UG_percentage { get; set; }
        public Nullable<int> UG_division { get; set; }
        public string UG_university { get; set; }
        public string UG_place { get; set; }
        [Required(ErrorMessage = "*")]
        public HttpPostedFileBase UG_certificate { get; set; }
        public string UG_facultyCertificate { get; set; }
        public string UG_specialization { get; set; }

        //PG Education Details
       [Required(ErrorMessage = "*")]
        [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "Invalid HallTicket number")]
        [Display(Name = "HallTicket Number")]
        //[Remote("CheckHallTicket", "NewOnlineRegistration", HttpMethod = "POST", ErrorMessage = "HallTicket Number already registred.", AdditionalFields = "RegistrationNumber")]
        public string PG_HallticketNo { get; set; }
        public int PG_educationId { get; set; }
        public string PG_educationName { get; set; }
        public string PG_studiedEducation { get; set; }
        public Nullable<int> PG_passedYear { get; set; }
        public Nullable<decimal> PG_percentage { get; set; }
        public Nullable<int> PG_division { get; set; }
        public string PG_university { get; set; }
        public string PG_place { get; set; }
        [Required(ErrorMessage = "*")]
        public HttpPostedFileBase PG_certificate { get; set; }
        public string PG_facultyCertificate { get; set; }
        public string PG_specialization { get; set; }

        //M.Phil/Other PG Degree Education Details
        //[Required(ErrorMessage = "*")]
        [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "Invalid HallTicket number")]
        [Display(Name = "HallTicket Number")]
        //[Remote("CheckHallTicket", "NewOnlineRegistration", HttpMethod = "POST", ErrorMessage = "HallTicket Number already registred.", AdditionalFields = "RegistrationNumber")]
        public string MPhil_HallticketNo { get; set; }
        public int MPhil_educationId { get; set; }
        public string MPhil_educationName { get; set; }
        public string MPhil_studiedEducation { get; set; }
        public Nullable<int> MPhil_passedYear { get; set; }
        public Nullable<decimal> MPhil_percentage { get; set; }
        public Nullable<int> MPhil_division { get; set; }
        public string MPhil_university { get; set; }
        public string MPhil_place { get; set; }      
        public HttpPostedFileBase MPhil_certificate { get; set; }
        public string MPhil_facultyCertificate { get; set; }
        public string MPhil_specialization { get; set; }

        //Ph.D Education Details
        //[Required(ErrorMessage = "*")]
        [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "Invalid HallTicket number")]
        [Display(Name = "HallTicket Number")]
       // [Remote("CheckHallTicket", "NewOnlineRegistration", HttpMethod = "POST", ErrorMessage = "HallTicket Number already registred.", AdditionalFields = "RegistrationNumber")]
        public string PhD_HallticketNo { get; set; }
        public int PhD_educationId { get; set; }
        public string PhD_educationName { get; set; }
        public string PhD_studiedEducation { get; set; }
        public Nullable<int> PhD_passedYear { get; set; }
        public Nullable<decimal> PhD_percentage { get; set; }
        public Nullable<int> PhD_division { get; set; }
        public string PhD_university { get; set; }
        public string PhD_place { get; set; }
        public HttpPostedFileBase PhD_certificate { get; set; }
        public string PhD_facultyCertificate { get; set; }
        public string PhD_specialization { get; set; }

        //NET/SELT Education Details
        public string NET_HallticketNo { get; set; }
        public int NET_educationId { get; set; }
        public string NET_educationName { get; set; }
        public string NET_studiedEducation { get; set; }
        public Nullable<int> NET_passedYear { get; set; }
        public Nullable<decimal> NET_percentage { get; set; }
        public Nullable<int> NET_division { get; set; }
        public string NET_university { get; set; }
        public string NET_place { get; set; }
        public HttpPostedFileBase NET_certificate { get; set; }
        public string NET_facultyCertificate { get; set; }
        public string NET_specialization { get; set; }

        //All Certificates Details
        public int Others_educationId { get; set; }
        public HttpPostedFileBase All_Certificates { get; set; }
        public string faculty_AllCertificates { get; set; }
        public List<string> ug_specializations { get; set; }
        public List<string> pg_specializations { get; set; }
        public List<string> universitys { get; set; }
        public List<string> places { get; set; }
        public List<string> Courses { get; set; }
        #endregion

        public int DegreeId { get; set; }
        public int Eid { get; set; }   
        public string Principal { get; set; }
        public int? FIsApproved { get; set; }

        [StringLength(16, ErrorMessage = "Must be 12 characters")]
        [RegularExpression(@"\d{12}", ErrorMessage = "Invalid AADHAAR number")]
        [Display(Name = "Aadhaar Number")]
        [Remote("AdjunctCheckAadhaarNumber", "OnlineRegistration", AdditionalFields = "RegistrationNumber", HttpMethod = "POST", ErrorMessage = "Aadhaar Number Already Exists.")]
        public string AdjunctEditAadhaarNumber { get; set; }

        //[Required(ErrorMessage = "*")]
        [StringLength(10, ErrorMessage = "Maximum 10 characters")]
        [RegularExpression(@"\d{10}", ErrorMessage = "Invalid mobile")]
        [Display(Name = "Mobile No")]
        [Remote("AdjunctCheckMobileNumber", "OnlineRegistration", AdditionalFields = "RegistrationNumber", HttpMethod = "POST", ErrorMessage = "Mobile Number  Already Exists.")]
        public string AdjunctEditMobile { get; set; }

        public int? CUpdatedby { get; set; }
        public DateTime? CUpdatedOn { get; set; }

        [Display(Name = "ViewType")]
        public bool? isView { get; set; }     

        public int? SamePANNumberCount { get; set; }
        public int? SameAadhaarNumberCount { get; set; }

        public bool isVerified { get; set; }
        public bool isValid { get; set; }

      
        public string DeactivationNew { get; set; }
        public string SCMDocumentView { get; set; }
        public int PHDView { get; set; }
        public string AppealSCMDocumentView { get; set; }
        

        public HttpPostedFileBase NOCUploadFile { get; set; }
        public HttpPostedFileBase MembershipFile1 { get; set; }
        public HttpPostedFileBase MembershipFile2 { get; set; }
        public HttpPostedFileBase MembershipFile3 { get; set; }

        public virtual my_aspnet_users my_aspnet_users { get; set; }
        public virtual my_aspnet_users my_aspnet_users1 { get; set; }
        public virtual jntuh_designation jntuh_designation { get; set; }
        public virtual jntuh_department jntuh_department { get; set; }
        public virtual ICollection<jntuh_registered_faculty_education> jntuh_registered_faculty_education { get; set; }
       // public virtual ICollection<jntuh_reinspection_registered_faculty_education> jntuh_reinspection_registered_faculty_education { get; set; }
       // public virtual ICollection<jntuh_registered_faculty_education_log> jntuh_registered_faculty_education_log { get; set; }

        public List<RegisteredFacultyEducation> FacultyEducation { get; set; }
        public List<RegisteredfacultyExperience> RFExperience { get; set; }

        [StringLength(16, ErrorMessage = "Must be 12 characters")]
        [RegularExpression(@"\d{12}", ErrorMessage = "Invalid AADHAAR number")]
        [Display(Name = "ModifiedAadhaarNo")]
        public string ModifiedAadhaarNo { get; set; }

        public bool NONoc { get; set; }
        public bool NOProfessionalBodiesMembership { get; set; }
        public string SSC { get; set; }
        public string UG { get; set; }
        public string PG { get; set; }
        public string MTECH { get; set; }
        public string PHD { get; set; }



        public string GetPanNumber { get; set; }
        public string GetAadhaarNumber { get; set; }

        public string phdreson { get; set; }

        //public string HighestDegree { get; set; }
        public string DegreeName { get; set; }
        public string appealPrincipalSupportdocument { get; set; }
    }


    public class RegisteredFacultyEducation
    {
        public int educationId { get; set; }
        public string educationName { get; set; }
        public string studiedEducation { get; set; }
        public Nullable<int> passedYear { get; set; }
        public Nullable<decimal> percentage { get; set; }
        public Nullable<int> division { get; set; }
        public string university { get; set; }
        public string place { get; set; }
        public HttpPostedFileBase certificate { get; set; }
        public string facultyCertificate { get; set; }
        public string specialization { get; set; }
    }

    public class RegisteredfacultyExperience
    {
        public int? CollegeId { get; set; }
        public int? DesignationId { get; set; }
        public string CollegeName { get; set; }
        public string facultyDesignation { get; set; }

        public DateTime? facultyDateOfAppointment { get; set; }
        public DateTime? facultyDateOfResignation { get; set; }

        public string facultyDateOfAppointment1 { get; set; }
        public string facultyDateOfResignation1 { get; set; }
        public string RelievingLetter { get; set; }
        public string JoiningOrder { get; set; }
        public string Salary { get; set; }
        public int? IsApproved { get; set; }

    }

    public class facultyExperience
    {
        public int id { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Nullable<int> facultyId { get; set; }
        [Display(Name = "Faculty Registration ID")]
        public string RegistrationNumber { get; set; }
        public int? collegeId { get; set; }
        [Display(Name = "Previous College Name")]
        public string CollegeName { get; set; }

        public string CollegeCode { get; set; }

        [Display(Name = "College presently working in")]
        [Required(ErrorMessage = " Required")]
        public int PresentcollegeId { get; set; }
        [Display(Name = "Present College Name")]
        public string PresentCollegeName { get; set; }
        public int? PresentCollegeCode { get; set; }

        public Nullable<int> facultyDesignationId { get; set; }
        [Display(Name = "Current Designation")]
        public string facultyDesignation { get; set; }

        public int facultyNewDesignationId { get; set; }
        [Display(Name = "New Designation")]
        [Required(ErrorMessage = " Required")]
        public string facultyNewDesignation { get; set; }

        [Display(Name = "Date of Joining in the Present College")]
        public DateTime? facultyDateOfAppointment { get; set; }
        public string facultyDateOfAppointment1 { get; set; }

        [Display(Name = "Date Of Joining")]
        public string DateofJoining { get; set; }
        [Display(Name = "Date of Relieving in previous College")]
        public Nullable<System.DateTime> facultyDateOfResignation { get; set; }
        public string facultyDateOfResignation1 { get; set; }
        [Display(Name = "Upload Relieving Letter")]
        public HttpPostedFileBase facultyRelievingLetter { get; set; }
        public string RelievingLetter { get; set; }
        public string JoiningOrder { get; set; }
        [Display(Name = "Appointment Order")]
        public HttpPostedFileBase facultyJoiningOrder { get; set; }
        [Display(Name = "Gross Salary")]
        public string facultySalary { get; set; }
        public Nullable<bool> isActive { get; set; }
        public Nullable<int> createdBy { get; set; }
        public Nullable<System.DateTime> createdOn { get; set; }
        public Nullable<int> updatedBy { get; set; }
        public Nullable<System.DateTime> updatedOn { get; set; }
        public int? DepartmentId { get; set; }
        [Display(Name = "Department Name")]
        public string department { get; set; }

        [Required]
        public HttpPostedFileBase facultySCMDocument { get; set; }
        public string SCMDocument { get; set; }

    }

    public class DateWiseCounts
    {
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? Date { get; set; }
        public int? TotalFacultyRegistrations { get; set; }
    }

    public class PANNumberDuplicates
    {
        public string PanNumber { get; set; }
        public int? PanNumberDuplicatecount { get; set; }
    }

    public class AadhaarNumberDuplicates
    {
        public string AadhaarNumber { get; set; }
        public int? AadhaarNumberDuplicatecount { get; set; }
    }

    public class MobilityRequest
    {
        public int FacultyId { get; set; }
        public string FacultyName { get; set; }
        [Display(Name = "Previous Working College")]
        public string PresentWorkingCollege { get; set; }
        public int PresentWorkingCollegeId { get; set; }
        [Display(Name = "Department Name")]
        public string DepartmentName { get; set; }
        public int DeptId { get; set; }
        [Display(Name = "Previous College Date of Appointment")]
        public string PreviousDateofappointment { get; set; }
        [Display(Name = "Previous College Date of Releaving")]
        public string PreviousDateofReleving { get; set; }
        [Display(Name = "Previous College Appointment Letter")]
        public HttpPostedFileBase AppointmentOrder { get; set; }
        [Display(Name = "Previous College Releaving Letter")]
        public HttpPostedFileBase RelevingOrder { get; set; }
        public HttpPostedFileBase SalaryStatement { get; set; }
        [Display(Name = "Current College Date of Appointment")]
        public string Dateofappointment { get; set; }
        [Display(Name = "College Name")]
        public int CollegeId { get; set; }
        [Display(Name = "College Name")]
        public string CollegeName { get; set; }
        public string CollegeCode { get; set; }
        public string TicketId { get; set; }
        public HttpPostedFileBase SupportingDocument { get; set; }
        public string SupportingDocumentView { get; set; }
        [Display(Name = "Current College Appointment Letter")]
        public HttpPostedFileBase NewAppointmentOrder { get; set; }
        public string FacultyRegistration { get; set; }
        [Display(Name = "If you are presently working on any College")]
        public bool IsPresentlyWorking { get; set; }

    }

    public class ValidateFacultyImageAttribute : RequiredAttribute
    {
        public override bool IsValid(object value)
        {
            var file = value as HttpPostedFileBase;
            if (file == null)
            {
                return false;
            }

            if (file.ContentLength > 50 * 1024)
            {
                return false;
            }

            try
            {
                using (var img = Image.FromStream(file.InputStream))
                {
                    return img.RawFormat.Equals(ImageFormat.Jpeg);
                }
            }
            catch { }
            return false;
        }
    }

    public class KnowyourRegistrationNumbers
    {
        public string FirstName { get; set; }
        public string Lastname { get; set; }
        public string MiddleName { get; set; }
        public string RegistrationNumber { get; set; }
    }

    public class ScmUploadedData
    {
        public int SCMId { get; set; }
        public int SpecializationId { get; set; }
        public string Specialization { get; set; }
        public int DepartmentId { get; set; }
        public string Department { get; set; }
        public int DegreeId { get; set; }
        public string Degree { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string RegistrationNumber { get; set; }
        public bool Checked { get; set; }
        public int AuditorId { get; set; }
        public string AuditorName { get; set; }
        public int CollegeId { get; set; }
        public string CollegeName { get; set; }
        public HttpPostedFileBase ScmHardCopyBase { get; set; }
        public int AId { get; set; }

        public int DesignationId { get; set; }
        public string DesignationName { get; set; }

        public string SCMhardcopyview { get; set; }
        public string strSCMDate { get; set; }
        public DateTime? SCMDate { get; set; }
        public int facultyId { get; set; }
        public int ScmfacultyaddedId { get; set; }
        public bool? Approved { get; set; }
        public string Remarks { get; set; }
        public bool? Blacklist { get; set; }
        public bool? IsApproved { get; set; }
        public bool Noscm16 { get; set; }
        public string CollegeDepartment { get; set; }
        public int? CollegeDepartmentId { get; set; }
    }

    public class CollegeAssociatedFaculty
    {
        public string RegNo { get; set; }
        public int CollegeId { get; set; }
        public int DeptId { get; set; }
    }


    public class AddphdDocuments
    {
        public int Id { get; set; }
        public string RegistrationNumber { get; set; }
        public string Name { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        //[Required]
        public HttpPostedFileBase PhdDocumentfile { get; set; }
        public string PhdDocumentName { get; set; }

        [Display(Name = "Department")]
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public bool PhdUndertakingDocumentstatus { get; set; }

        public string PhdReason { get; set; }
    }

    public class FacultyPhdDeskVerification
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public string RegistrationNumber { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public int? GenderId { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string Email { get; set; }
        public string Mobile { get; set; }
        public string AadhaarNumber { get; set; }
        public bool isActive { get; set; }
        public bool? isApproved { get; set; }
        public string PhdDeskDocument { get; set; }
        public string PhdDeskReason { get; set; }
        public int? DeactivatedBy { get; set; }
        public DateTime? DeactivatedOn { get; set; }
        public bool PhdDeskVerification { get; set; }
        [Required]
        public string PhdDeskVerificationReason { get; set; }
        public string PANNumber { get; set; }
        public string facultyPhoto { get; set; }
        public int? DeptId { get; set; }
        public string DeptName { get; set; }
        public string OtherReson { get; set; }
    }


}