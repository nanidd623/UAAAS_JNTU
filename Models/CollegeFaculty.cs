using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace UAAAS.Models
{
    public class CollegeFaculty
    {
        public int id { get; set; }
        public int facultyId { get; set; }
        public string sfacid { get; set; }
        public int adhocid { get; set; }
        public string adhocregid { get; set; }
        public int collegeId { get; set; }
        public int? FacultyVerificationStatus { get; set; }
        public string Remarks { get; set; }
        public string facultyType { get; set; }
        public string PANVerificationStatuis { get; set; }
        public string PANDeactivationReasion { get; set; }
        public string FacultyDeactivationReasion { get; set; }
        public int? TotalExperience { get; set; }
        public bool InvalidPan { get; set; }
        public bool DiscrepencyStatus { get; set; }
        public bool InCompleteCertificates { get; set; }
        public string Reason { get; set; }
        public string NewReason { get; set; }
        //[Required(ErrorMessage = "Required")]
        [Display(Name = "Photo")]
        [ValidateFile]
        public HttpPostedFileBase facultyPhoto { get; set; }

        public string photo { get; set; }
        public int facultyCount { get; set; }

        [Required(ErrorMessage = "Required")]
        [StringLength(25, ErrorMessage = "Max 25 characters")]
        [Display(Name = "First Name")]
        public string facultyFirstName { get; set; }

        [StringLength(25, ErrorMessage = "Max 25 characters")]
        [Display(Name = "Last Name")]
        public string facultyLastName { get; set; }

        [Required(ErrorMessage = "Required")]
        [StringLength(25, ErrorMessage = "Max 25 characters")]
        [Display(Name = "Surname")]
        public string facultySurname { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Faculty Type")]
        public int facultyTypeId { get; set; }
        public int AdhocfacultyTypeId { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Gender")]
        public Nullable<int> facultyGenderId { get; set; }

        [Required(ErrorMessage = "Required")]
        [StringLength(100, ErrorMessage = "Max 100 characters")]
        [Display(Name = "Father's Name")]
        public string facultyFatherName { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Category")]
        public int facultyCategoryId { get; set; }

        public int AdhocfacultyCategoryId { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Date of Birth")]
        public string dateOfBirth { get; set; }

        //[Required(ErrorMessage = "Required")]
        [Display(Name = "Date of Ratification")]
        public string dateOfRatification { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Designation")]
        public int? facultyDesignationId { get; set; }

        public string designation { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Designation")]
        public int? facultyPresentDesignationId { get; set; }

        public string facultyPresentOtherDesignation { get; set; }

        public string presentdesignation { get; set; }

        [Display(Name = "Additional Duty if any")]
        public string facultyOtherDesignation { get; set; }


        //[Display(Name = "Additional Duty if any")]
        public string BasstatusOld { get; set; }


        [Display(Name = "Other Department")]
        public string facultyOtherDepartment { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Department")]
        public int facultyDepartmentId { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Department")]
        public int facultyPGDepartmentId { get; set; }

        [Required(ErrorMessage = "Select specialization")]
        [Display(Name = "Faculty specialization")]
        public int? FacultySpecalizationId { get; set; }




        public string department { get; set; }
        public string IdentifiedFor { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Date of Appointment")]
        public string dateOfAppointment { get; set; }

        [Display(Name = "Date of Resignation")]
        public string dateOfResignation { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Date of Birth")]
        public System.DateTime facultyDateOfBirth { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Date of Appointment")]
        public System.DateTime facultyDateOfAppointment { get; set; }

        [Display(Name = "Date of Resignation")]
        public Nullable<System.DateTime> facultyDateOfResignation { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Ratified by JNTUH")]
        public Nullable<bool> isFacultyRatifiedByJNTU { get; set; }

        [Display(Name = "Date of Ratification")]
        public Nullable<System.DateTime> facultyDateOfRatification { get; set; }

        [Display(Name = "Duration")]
        public Nullable<int> facultyDurationOfRatification { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Past Experience")]
        public Nullable<int> facultyPreviousExperience { get; set; }

        [Required(ErrorMessage = "Required")]
        // [RegularExpression(@"^\d+.\d{0,2}$", ErrorMessage = "Invalid, ex: 17350.50")]
        [Display(Name = "Salary Drawn (Rs.)")]
        public string facultySalary { get; set; }

        public string facultyGrossSalary { get; set; }

        public HttpPostedFileBase facultyForm16 { get; set; }
        public string ViewfacultyForm16 { get; set; }
        [Required(ErrorMessage = "Required")]
        [StringLength(100, ErrorMessage = "Max 100 characters")]
        [Display(Name = "Scale of Pay")]
        public string facultyPayScale { get; set; }

        //[Required(ErrorMessage = "Required")]
        [StringLength(10, ErrorMessage = "Max 10 characters")]
        [RegularExpression(@"\w{10}", ErrorMessage = "Invalid PAN number")]
        [Display(Name = "PAN Number")]
        public string facultyPANNumber { get; set; }

        [Required(ErrorMessage = "Required")]
        [StringLength(12, ErrorMessage = "Must be 12 characters")]
        [RegularExpression(@"\d{12}", ErrorMessage = "Invalid AADHAAR number")]
        [Display(Name = "Aadhaar Number")]
        [Remote("CheckAadharNumber", "Faculty", AdditionalFields = "FacultyRegistrationNumber", HttpMethod = "POST", ErrorMessage = "Aadhar Number is Not Correct.")]
        public string facultyAadhaarNumber { get; set; }

        [Required(ErrorMessage = "Required")]
        [StringLength(12, ErrorMessage = "Must be 12 characters")]
        [RegularExpression(@"\d{12}", ErrorMessage = "Invalid AADHAAR number")]
        [Display(Name = "Aadhaar Number")]
        //[Remote("CheckAadharNumber", "Faculty", AdditionalFields = "FacultyRegistrationNumber", HttpMethod = "POST", ErrorMessage = "Aadhar Number is Not Correct.")]
        public int? intfacultyAadhaarNumber { get; set; }

        [Required(ErrorMessage = "Required")]
        [StringLength(12, ErrorMessage = "Must be 12 characters")]
        [RegularExpression(@"\d{12}", ErrorMessage = "Invalid AADHAAR number")]
        [Display(Name = "Aadhaar Number")]
        [Remote("CheckAadharNumberNew", "Faculty", HttpMethod = "POST", ErrorMessage = "Aadhar Number is Not Correct.")]
        public string facultyAadhaarNumberNew { get; set; }

        [Display(Name = "facultyAadharPhotoDocument")]
        public HttpPostedFileBase facultyAadharPhotoDocument { get; set; }
        public string facultyAadharDocument { get; set; }


        [Required(ErrorMessage = "Required")]
        [Display(Name = "Is the faculty associated with Examination Branch?")]
        public Nullable<bool> isRelatedToExamBranch { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Is the faculty associated with Placement Cell?")]
        public Nullable<bool> isRelatedToPlacementCell { get; set; }

        [Required(ErrorMessage = "Required")]
        [StringLength(15, ErrorMessage = "Max 15 characters")]
        [RegularExpression(@"\d{10}", ErrorMessage = "Invalid mobile")]
        [Display(Name = "Mobile")]
        public string facultyMobile { get; set; }

        [Required(ErrorMessage = "Required")]
        [StringLength(250, ErrorMessage = "Max 250 characters")]
        [RegularExpression(@"[a-z0-9._%+-]+@[a-z0-9.-]+\.[a-z]{2,4}", ErrorMessage = "Invalid email")]
        [Display(Name = "Email")]
        public string facultyEmail { get; set; }

        [StringLength(500, ErrorMessage = "Max 500 characters")]
        [Display(Name = "Achievement")]
        public string facultyAchievements1 { get; set; }

        [StringLength(500, ErrorMessage = "Max 500 characters")]
        [Display(Name = "Achievement")]
        public string facultyAchievements2 { get; set; }

        [StringLength(500, ErrorMessage = "Max 500 characters")]
        [Display(Name = "Achievement")]
        public string facultyAchievements3 { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Status")]
        public bool isActive { get; set; }

        [StringLength(50, ErrorMessage = "Max 50 characters")]
        //[RegularExpression(@"\d{12}", ErrorMessage = "Invalid A/c No")]
        [Display(Name = "Salary Bank A/c No")]
        public string salaryAccountNumber { get; set; }

        [StringLength(50, ErrorMessage = "Max 50 characters")]
        [Display(Name = "Bank")]
        public string salaryBankName { get; set; }

        [StringLength(50, ErrorMessage = "Max 50 characters")]
        [Display(Name = "Branch")]
        public string salaryBranchName { get; set; }

        [StringLength(10, ErrorMessage = "Max 10 characters")]
        [Display(Name = "Faculty Recruited For")]
        public string facultyRecruitedFor { get; set; }

        [Required(ErrorMessage = "*")]
        public string facultyfresherexperiance { get; set; }

        [Required(ErrorMessage = "Required")]
        [StringLength(20, ErrorMessage = "Max 20 characters")]
        [RegularExpression(@"^\d+$", ErrorMessage = "Invalid")]
        [Display(Name = "Gross Salary")]
        public string grossSalary { get; set; }





        [Required(ErrorMessage = "Required")]
        [StringLength(20, ErrorMessage = "Max 20 characters")]
        [RegularExpression(@"^\d+$", ErrorMessage = "Invalid")]
        [Display(Name = "Net Salary")]
        public string netSalary { get; set; }

        //[StringLength(15, ErrorMessage = "Max 15 characters")]
        [Display(Name = "Registration ID")]
        [Required(ErrorMessage = "Required")]
        [Remote("CheckRegistrationNumber", "Faculty", HttpMethod = "POST", ErrorMessage = "*")]
        public string FacultyRegistrationNumber { get; set; }

        public string AdocFacultyRegistrationNumber { get; set; }
        public int AdocFacultyId { get; set; }

        [Required(ErrorMessage = "Required")]
        [StringLength(12, ErrorMessage = "Must be 12 characters")]
        [RegularExpression(@"\d{12}", ErrorMessage = "Invalid AADHAAR number")]
        [Display(Name = "Aadhaar Number")]
        [Remote("AdocCheckAadharNumber", "Faculty", AdditionalFields = "AdocFacultyRegistrationNumber", HttpMethod = "POST", ErrorMessage = "Aadhar Number is Not Correct.")]
        public string AdocAadhaarNumber { get; set; }
        public HttpPostedFileBase AdocAadhaarDocument { get; set; }
        public string AdocAadhaarDocumentView { get; set; }

        [Required(ErrorMessage = "Select specialization")]
        [Display(Name = "Specialization ID")]
        public int? SpecializationId { get; set; }
        public string SpecializationName { get; set; }
        public Nullable<System.DateTime> createdOn { get; set; }
        public Nullable<int> createdBy { get; set; }
        public Nullable<System.DateTime> updatedOn { get; set; }
        public Nullable<int> updatedBy { get; set; }
        public bool Phd2pages { get; set; }
        public bool PrevoiusYearFaculty { get; set; }
        public string facultygrosssalA21 { get; set; }
        public int HighestQualification { get; set; }

        public virtual jntuh_college jntuh_college { get; set; }
        public virtual my_aspnet_users my_aspnet_users { get; set; }
        public virtual my_aspnet_users my_aspnet_users1 { get; set; }
        public virtual jntuh_faculty_type jntuh_faculty_type { get; set; }
        public virtual jntuh_faculty_category jntuh_faculty_category { get; set; }
        public virtual jntuh_designation jntuh_designation { get; set; }
        public virtual jntuh_department jntuh_department { get; set; }
        public virtual ICollection<jntuh_faculty_education> jntuh_faculty_education { get; set; }
        public virtual ICollection<jntuh_faculty_subjects> jntuh_faculty_subjects { get; set; }

        public List<FacultyEducation> FacultyEducation { get; set; }
        public List<FacultySubject> FacultySubject { get; set; }
        public int Facultydeficencycount { get; set; }
        public int? DegreeId { get; set; }
        public int FacultyDegreeId { get; set; }
        public string DegreeName { get; set; }

        //New docs for appeal
        public HttpPostedFileBase NotificationDocument { get; set; }
        public HttpPostedFileBase SelectionCommitteeDocument { get; set; }
        public HttpPostedFileBase JoiningReportDocument { get; set; }
        public HttpPostedFileBase AppointmentOrderDocument { get; set; }
        public HttpPostedFileBase PhysicalPresenceDocument { get; set; }
        public HttpPostedFileBase AppealReverificationSupportDoc { get; set; }
        public HttpPostedFileBase AppealReverificationScreenShot { get; set; }
        public HttpPostedFileBase PhdUndertakingDocument { get; set; }
        // public HttpPostedFileBase  AadharDocument { get; set; }

        public HttpPostedFileBase RelivingDocument { get; set; }

        //view appeal docs
        public string ViewNotificationDocument { get; set; }
        public string ViewSelectionCommitteeDocument { get; set; }
        public string Collegescmdocument { get; set; }
        public string ViewJoiningReportDocument { get; set; }
        public string ViewAppointmentOrderDocument { get; set; }
        public string ViewPhysicalPresenceDocument { get; set; }
        public string ViewAppealReverificationSupportDoc { get; set; }
        public string ViewPhdUndertakingDocument { get; set; }
        public string ViewAppealReverificationScreenShot { get; set; }

        public string ViewRelivingDocument { get; set; }
        [Required(ErrorMessage = "*")]
        public int Previouscollegeid { get; set; }
        public int experienceId { get; set; }
        public string Previouscollegename { get; set; }
        public string Otherscollegename { get; set; }
        public string CollegeName { get; set; }
        public string CollegeCode { get; set; }
        public List<CollegeFaculty> CollegeFacultiesliList { get; set; }
        public bool BlackList { get; set; }
        public bool IsFacultyTracking { get; set; }
        public string RegNum { get; set; }
        public string FacName { get; set; }
        public string AadhaarFlag { get; set; }

        //Appeal Reverification Faculty flags
        public string FacultyReson { get; set; }
        public int FacultyAddedSpecializationId { get; set; }
        public int RegisteredFacultyPGSpecializationId { get; set; }
        public string RegisteredFacultyPGSpecializationName { get; set; }

    }

    public class FacultyEducation
    {
        public int educationId { get; set; }
        public string educationName { get; set; }
        public string studiedEducation { get; set; }
        public Nullable<int> passedYear { get; set; }
        public Nullable<decimal> percentage { get; set; }
        public Nullable<int> division { get; set; }
        public string university { get; set; }
        public string place { get; set; }
    }

    public class FacultySubject
    {
        public Nullable<int> degreeId { get; set; }
        public string degreeName { get; set; }
        public Nullable<int> departmentId { get; set; }
        public string departmentName { get; set; }
        public Nullable<int> specializationId { get; set; }
        public string specializationName { get; set; }
        public int shiftId { get; set; }
        public string shiftName { get; set; }
        public string subject { get; set; }
        public Nullable<int> duration { get; set; }
    }

    public class DistinctDepartment
    {
        public int id { get; set; }
        public string departmentName { get; set; }
    }


    public class FacultyAttedance
    {
        public string RegistrationNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }
        public string AadhaarNumber { get; set; }
        public int GenderId { get; set; }
        public DateTime DateOfBirth { get; set; }
        public int id { get; set; }
        public int DepartmentId { get; set; }
    }

    public class PhysicalLabMaster
    {
        public int NoofAvailable { get; set; }
        public int NoofRequeried { get; set; }
        public string DepartmentName { get; set; }
        public string Labname { get; set; }
        public string PhysicalLabUploadingview { get; set; }
        public HttpPostedFileBase PhysicalLabsuploading { get; set; }
    }

    public class SpecializationList
    {
        public int SpecializationId { get; set; }
        public string SpecializationName { get; set; }
    }

    public class registeredfacultylist
    {
        public string Collegecode { get; set; }
        public string collegename { get; set; }
        public string RegistrationNumber { get; set; }
        public string FacultyName { get; set; }
        public string identifierfor { get; set; }
        public string departmentName { get; set; }
        public int? specializationId { get; set; }
        public string specialization { get; set; }
    }
}