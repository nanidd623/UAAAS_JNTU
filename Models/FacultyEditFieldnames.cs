using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace UAAAS.Models
{
    public class FacultyEditFieldnames
    {
        public string ticketId { get; set; }
        public int facultyId { get; set; }
        public string FatherOrhusbandName { get; set; }
        public string MotherName { get; set; }
        public int? GenderId { get; set; }
        public string FacultyDateOfBirth { get; set; }
        public string PanNumber { get; set; }
        public string Mobile { get; set; }
        public string AadhaarNumber { get; set; }
        [Required]
        public HttpPostedFileBase AllSuportdocument { get; set; }
        public string AllSuportdocumentView { get; set; }
        public List<EditFields> EditCheckboxs { get; set; }
        public List<EditEducationFields> EditEducationCheckboxs { get; set; }

        public List<string> Depts { get; set; }
        public List<string> Design { get; set; }
        public List<string> ug_specializations { get; set; }
        public List<string> pg_specializations { get; set; }
        public List<string> universitys { get; set; }
        public List<string> places { get; set; }
        public List<string> Courses { get; set; }
    }
    public class EditFields
    {
        public int Id { get; set; }
        public string ticketId { get; set; }
        public int FieldId { get; set; }
        public string Field { get; set; }
        public string FieldDesc { get; set; }
        public string Fieldorginal { get; set; }
        public HttpPostedFileBase Suportdocument { get; set; }
        public string SuportdocumentView { get; set; }
        public bool? isEditable { get; set; }
        public bool isSelect { get; set; }
        public string requestReason { get; set; }
        public short? isApproved { get; set; }
        public string createddate { get; set; }
    }

    public class EditEducationFields
    {
        public int Id { get; set; }
        public string ticketId { get; set; }
        public int facultyId { get; set; }
        public int FieldId { get; set; }
        public string Field { get; set; }
        public string FieldDesc { get; set; }
        public int Educationid { get; set; }
        public string Coursestudied { get; set; }
        public string Specialization { get; set; }
        public int PassedYear { get; set; }
        public decimal? MarkasPercentage { get; set; }
        public int? Division { get; set; }
        public string BoardorUniversity { get; set; }
        public string PlaceofEducation { get; set; }
        public HttpPostedFileBase Educationcertificate { get; set; }
        public string EducationcertificateView { get; set; }
        public string AllSupportFilesView { get; set; }
        public bool isSelect { get; set; }
        public string requestReason { get; set; }
        public short? isApproved { get; set; }
        public string createddate { get; set; }
    }

    public class AdminRequestsClass
    {
        public int Id { get; set; }
        public string type { get; set; }
        public int FacultyId { get; set; }
        public int FieldId { get; set; }
        public string Field { get; set; }
        public string FieldDesc { get; set; }
        public string RegistrationNumber { get; set; }
        public string Fieldorginal { get; set; }
        public string requestReason { get; set; }
        public short? isApproved { get; set; }
        public string rejectReason { get; set; }
        public string SuportdocumentView { get; set; }
    }
    public class AdminFacultyEduRequestsClass
    {
        public int Id { get; set; }
        public int FacultyId { get; set; }
        public int FieldId { get; set; }
        public string Field { get; set; }
        public string FieldDesc { get; set; }
        public string RegistrationNumber { get; set; }
        public string Fieldorginal { get; set; }
        public string requestReason { get; set; }
        public short? isApproved { get; set; }
        public string rejectReason { get; set; }
        public string SuportdocumentView { get; set; }
    }

    public class RequestsListBasedonRegNumbers
    {
        public int? FieldId { get; set; }
        public List<FacultyData> FacultyList { get; set; }
    }

    public class FacultyData
    {
        public int CollegeId { get; set; }
        public string CollegeName { get; set; }
        public string ticketId { get; set; }
        public int FacultyId { get; set; }
        public string RegistrationNumber { get; set; }
        public string Name { get; set; }
        public string Gender { get; set; }
        public int RequestsCount { get; set; }
        public int EduRequestsCount { get; set; }
    }

    public class FacultySuppDocsClass
    {
        public int Id { get; set; }
        public int academicyearId { get; set; }
        public int FacultyId { get; set; }
        [Required(ErrorMessage = "*")]
        public int CertificateTypeId { get; set; }
        public string CertificateType { get; set; }
        [Required(ErrorMessage = "*")]
        public int DegreeId { get; set; }
        public string Degree { get; set; }
        [Required(ErrorMessage = "*")]
        public string Specialization { get; set; }
        [Required(ErrorMessage = "*")]
        [Display(Name = "Certificate")]
        public HttpPostedFileBase Certificate { get; set; }

        public string facultyCertificate { get; set; }

        [Display(Name = "Date of Birth")]
        public DateTime? IssuedDate { get; set; }

        [Required(ErrorMessage = "*")]
        [Display(Name = "Date of Birth")]
        public string facultyCertificateIssuedDate { get; set; }

        [Required(ErrorMessage = "*")]
        public string AwardedUniversity { get; set; }

        [Required(ErrorMessage = "*")]
        public int? AwardedYear { get; set; }

        [Required(ErrorMessage = "*")]
        public string PlaceOfEducation { get; set; }

        public string remarks { get; set; }
        public short? isApproved { get; set; }

        public Nullable<System.DateTime> createdOn { get; set; }
        public Nullable<int> createdBy { get; set; }
        public Nullable<System.DateTime> updatedOn { get; set; }
        public Nullable<int> updatedBy { get; set; }

        public int? Verifiedby { get; set; }
        public DateTime? VerifiedOn { get; set; }
        public List<string> ugspecializations { get; set; } 
    }
}