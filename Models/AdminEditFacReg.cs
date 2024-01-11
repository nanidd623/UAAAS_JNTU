using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace UAAAS.Models
{
    public class AdminEditFacReg
    {
        public AdminEditFacReg()
        {
            this.jntuh_registered_faculty_education = new HashSet<jntuh_registered_faculty_education>();            
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

        //[Required(ErrorMessage = "*")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "Must be 1-50 characters long.")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        //LastName-MiddleName
        [StringLength(50, ErrorMessage = "Maximum 50 characters")]
        [Display(Name = "Middle Name")]
        public string MiddleName { get; set; }

        //Surname-LastName
        //[Required(ErrorMessage = "*")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "Must be 1-50 characters long.")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        //[Required(ErrorMessage = "*")]
        [Display(Name = "Gender")]
        public int? GenderId { get; set; }

        //[Required(ErrorMessage = "*")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Must be 3-100 characters long.")]
        [Display(Name = "Father's Name")]
        public string FatherOrhusbandName { get; set; }

        //[Required(ErrorMessage = "*")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Must be 3-50 characters long.")]
        [Display(Name = "Mother's Name")]
        public string MotherName { get; set; }

        [Display(Name = "Date of Birth")]
        //[DataType(DataType.Date)]
        //[DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? DateOfBirth { get; set; }

        //[Required(ErrorMessage = "*")]
        [Display(Name = "Date of Birth")]
        public string facultyDateOfBirth { get; set; }

        //[RegularExpression(@"\w{10}", ErrorMessage = "Invalid PAN number")]
        //[Required(ErrorMessage = "*")]
        [RegularExpression(@"[A-Z]{3}[P][A-Z]{1}\d{4}[A-Z]{1}", ErrorMessage = "Invalid PAN number")]
        [Remote("CheckPanNumber", "NewOnlineRegistration", AdditionalFields = "RegistrationNumber", HttpMethod = "POST", ErrorMessage = "PAN Number Not Valid.")]
        [Display(Name = "PAN Number")]
        public string EditPANNumber { get; set; }

        public string PanStatus { get; set; }

        [StringLength(16, ErrorMessage = "Must be 12 characters")]
        [RegularExpression(@"\d{12}", ErrorMessage = "Invalid AADHAAR number")]
        [Display(Name = "Aadhaar Number")]
        [Remote("EditCheckAadhaarNumber", "NewOnlineRegistration", AdditionalFields = "RegistrationNumber", HttpMethod = "POST", ErrorMessage = "Aadhaar Number Already Exists.")]
        public string EditAadhaarNumber { get; set; }


        //[Required(ErrorMessage = "*")]
        [StringLength(10, ErrorMessage = "Maximum 10 characters")]
        [RegularExpression(@"\d{10}", ErrorMessage = "Invalid mobile")]
        [Display(Name = "Mobile No")]
        [Remote("EditCheckMobileNumber", "NewOnlineRegistration", AdditionalFields = "RegistrationNumber", HttpMethod = "POST", ErrorMessage = "Mobile Number  Already Exists.")]
        public string EditMobile { get; set; }
      

        //[Required(ErrorMessage = "*")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Must be 6-100 characters long.")]
        [DataType(DataType.EmailAddress)]
        [RegularExpression(@"[a-z0-9._%+-]+@[a-z0-9.-]+\.[a-z]{2,4}", ErrorMessage = "Invalid email")]
        [Display(Name = "Email Address")]
        // [Remote("EditCheckEmail", "OnlineRegistration",AdditionalFields = "RegistrationNumber",, HttpMethod = "POST", ErrorMessage = "Email is Already exists.")]
        public string EditEmail { get; set; }

        //[Required(ErrorMessage = "*")]
        [Display(Name = "Photo")] 
        public HttpPostedFileBase Photo { get; set; }

        public string facultyPhoto { get; set; }

        [Display(Name = "PAN Card Document")]
        public HttpPostedFileBase PANCardDocument { get; set; }

        public string facultyPANCardDocument { get; set; }

        [Display(Name = "Aadhaar Card Document")]
        public HttpPostedFileBase AadhaarCardDocument { get; set; }

        public string facultyAadhaarCardDocument { get; set; }

        #endregion

       // [Required(ErrorMessage = "*")]
        public string WorkingType { get; set; }

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

        [Display(Name = "Other Department")]
        public string OtherDepartment { get; set; }

        [StringLength(100, ErrorMessage = "Maximum 100 characters")]
        [Display(Name = "Honorarium")]
        public string GrossSalary { get; set; }

        [Display(Name = "Date of Appointment")]
        //[DataType(DataType.Date)]
        //[DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? DateOfAppointment { get; set; }
        public string facultyAppointmentLetter { get; set; }
        public string showPresentCollegeExperiance { get; set; }
        public string showTotalExperience { get; set; }
      
        [Display(Name = "Date of Appointment")]
        public string facultyDateOfAppointment { get; set; }
     
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
     
        [Display(Name = "Experience Present Institution")]
        public int? TotalExperiencePresentCollege { get; set; }


        [StringLength(1000, ErrorMessage = "Maximum 500 characters")]
        [Display(Name = "National")]
        public string National { get; set; }

        [StringLength(1000, ErrorMessage = "Maximum 500 characters")]
        [Display(Name = "International")]
        public string InterNational { get; set; }

        [StringLength(1000, ErrorMessage = "Maximum 500 characters")]
        [Display(Name = "Citation")]
        public string Citation { get; set; }

        [StringLength(1000, ErrorMessage = "Maximum 500 characters")]
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

        // [Required(ErrorMessage = "*")]
        public HttpPostedFileBase IncomeTaxFileUpload { get; set; }
        
        [Display(Name = "Income Tax 26 As / Form16")]
        public string IncomeTaxFileview { get; set; }

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
        
        public bool PhdUndertakingDocumentstatus { get; set; }
        public string PhdUndertakingDocumentText { get; set; }

        public string IdentfiedFor { get; set; }
        public string SpecializationIdentfiedFor { get; set; }

        #region Login Details
        //[Required(ErrorMessage = "*")]
        [StringLength(15, MinimumLength = 5, ErrorMessage = "Must be 5-15 characters long.")]
        [Display(Name = "Username")]
        [RegularExpression(@"[a-zA-Z0-9_]{1,15}", ErrorMessage = "Allowed characters : 'alphabets', 'numbers', '_'")]
        public string UserName { get; set; }

       // [Required(ErrorMessage = "*")]
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
        public int SSC_educationId { get; set; }
        //[Required(ErrorMessage = "*")]
        [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "Invalid HallTicket number")]
        [Display(Name = "HallTicket Number")]
        //[Remote("CheckHallTicket", "NewOnlineRegistration", HttpMethod = "POST", ErrorMessage = "HallTicket Number already registred.", AdditionalFields = "RegistrationNumber")]
        public string SSC_HallticketNo { get; set; }
        public string SSC_educationName { get; set; }
        public string SSC_studiedEducation { get; set; }
        public Nullable<int> SSC_passedYear { get; set; }
        public Nullable<decimal> SSC_percentage { get; set; }
        public Nullable<int> SSC_division { get; set; }
        public string SSC_university { get; set; }
        public string SSC_place { get; set; }
        public HttpPostedFileBase SSC_certificate { get; set; }
        public string SSC_facultyCertificate { get; set; }
        public string SSC_specialization { get; set; }

        //UG Education Details
        public int UG_educationId { get; set; }
        //[Required(ErrorMessage = "*")]
        [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "Invalid HallTicket number")]
        [Display(Name = "HallTicket Number")]
        //[Remote("CheckHallTicket", "NewOnlineRegistration", HttpMethod = "POST", ErrorMessage = "HallTicket Number already registred.", AdditionalFields = "RegistrationNumber")]
        public string UG_HallticketNo { get; set; }
        public string UG_educationName { get; set; }
        public string UG_studiedEducation { get; set; }
        public Nullable<int> UG_passedYear { get; set; }
        public Nullable<decimal> UG_percentage { get; set; }
        public Nullable<int> UG_division { get; set; }
        public string UG_university { get; set; }
        public string UG_place { get; set; }
        public HttpPostedFileBase UG_certificate { get; set; }
        public string UG_facultyCertificate { get; set; }
        public string UG_specialization { get; set; }

        [Display(Name = "Category")]
        public string facultyCategory { get; set; }

        //PG Education Details
        public int PG_educationId { get; set; }
        //[Required(ErrorMessage = "*")]
        [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "Invalid HallTicket number")]
        [Display(Name = "HallTicket Number")]
        //[Remote("CheckHallTicket", "NewOnlineRegistration", HttpMethod = "POST", ErrorMessage = "HallTicket Number already registred.", AdditionalFields = "RegistrationNumber")]
        public string PG_HallticketNo { get; set; }
        public string PG_educationName { get; set; }
        public string PG_studiedEducation { get; set; }
        public Nullable<int> PG_passedYear { get; set; }
        public Nullable<decimal> PG_percentage { get; set; }
        public Nullable<int> PG_division { get; set; }
        public string PG_university { get; set; }
        public string PG_place { get; set; }
        public HttpPostedFileBase PG_certificate { get; set; }
        public string PG_facultyCertificate { get; set; }
        public string PG_specialization { get; set; }

        //M.Phil/Other PG Degree Education Details
        public int MPhil_educationId { get; set; }
        //[Required(ErrorMessage = "*")]
        //[RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "Invalid HallTicket number")]
        [Display(Name = "HallTicket Number")]
        //[Remote("CheckHallTicket", "NewOnlineRegistration", HttpMethod = "POST", ErrorMessage = "HallTicket Number already registred.", AdditionalFields = "RegistrationNumber")]
        public string MPhil_HallticketNo { get; set; }
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
        public int PhD_educationId { get; set; }
        //[Required(ErrorMessage = "*")]
        [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "Invalid HallTicket number")]
        [Display(Name = "HallTicket Number")]
        //[Remote("CheckHallTicket", "NewOnlineRegistration", HttpMethod = "POST", ErrorMessage = "HallTicket Number already registred.", AdditionalFields = "RegistrationNumber")]
        public string PhD_HallticketNo { get; set; }
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
        public string AppealSCMDocumentView { get; set; }
        
        public virtual my_aspnet_users my_aspnet_users { get; set; }
        public virtual my_aspnet_users my_aspnet_users1 { get; set; }
        public virtual jntuh_designation jntuh_designation { get; set; }
        public virtual jntuh_department jntuh_department { get; set; }
        public virtual ICollection<jntuh_registered_faculty_education> jntuh_registered_faculty_education { get; set; }
        public virtual ICollection<jntuh_registered_faculty_edit_fields> jntuh_registered_faculty_edit_fields { get; set; }

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
        public string InStatus { get; set; }
    }
}