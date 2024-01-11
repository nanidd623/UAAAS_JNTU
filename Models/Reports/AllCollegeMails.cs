using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace UAAAS.Models
{
    public class CollegeMails
    {
        public string Subject { get; set; }

       // [UIHint("tinymce_full"), AllowHtml]
        public string Message { get; set; }

        public string SMS { get; set; }
        public bool Select { get; set; }
        public List<AllCollegeMails> collegeMails { get; set; }
    }
    public class CollegeBulkMails
    {
        [Required(ErrorMessage = "Please enter mail subject.")]
        public string Subject { get; set; }

        [Required(ErrorMessage = "Please enter mail body.")]
        public string Message { get; set; }
        public string SMS { get; set; }
        public bool Select { get; set; }
        [Required(ErrorMessage = "Please select file.")]
        [Display(Name = "Browse File")]
        public HttpPostedFileBase[] files { get; set; }

        public List<BulkCollegeMails> Bulkemails { get; set; }
    }
    public class BulkCollegeMails
    {
        public int collegeId { get; set; }
        public string collegeCode { get; set; }
        public string collegeName { get; set; }
        //public string collegeDistrict { get; set; }
        //public string collegeMandal { get; set; }
        public string Collegeemail { get; set; }
        public string Societyemail { get; set; }
        public string Secretaryemail { get; set; }
        public string principalemail { get; set; }
        public string FileName { get; set; }
        public string Mobile { get; set; }

    }
    public class AllCollegeMails
    {
        public int collegeId { get; set; }
        public string collegeCode { get; set; }
        public string collegeName { get; set; }
        public int collegeTypeId { get; set; }
        //public string collegeMandal { get; set; }
        public string email { get; set; }
        public string mobileNo { get; set; }
        public bool isSelect { get; set; }
    }
    //CommitteePendingSubmissionColleges Report
    public class CommitteePendingSubmissionColleges
    {
        public int id { get; set; }
        public int collegeid { get; set; }
        public string auditorname { get; set; }
        public string departmentname { get; set; }
        public string designation { get; set; }
        public string auditorpreferreddesignation { get; set; }
        public string auditoremail1 { get; set; }
        public string auditoremail2 { get; set; }
        public string auditormobile1 { get; set; }
        public string auditormobile2 { get; set; }
        public string auditorplace { get; set; }
    }
    //Colleges Report
    public class CollegesReport
    {
        public int collegeid { get; set; }
        public string collegeCode { get; set; }
        public string collegeName { get; set; }
        public string mandal { get; set; }
        public string address { get; set; }
        public string townorCity { get; set; }
        public int? pincode { get; set; }
        public string landline { get; set; }
        [RegularExpression(@"\d{10}", ErrorMessage = "Please enter correct mobile number, ex: 9493929190")]
        public string mobile { get; set; }
        [RegularExpression(@"[a-z0-9._%+-]+@[a-z0-9.-]+\.[a-z]{2,4}", ErrorMessage = "Please enter correct email")]
        public string email { get; set; }
        public int districtId { get; set; }
        public string districtName { get; set; }
        public int collegeTypeId { get; set; }
        public string collegeType { get; set; }
        public bool isCollegeEditable { get; set; }
        public int collegestatusId { get; set; }


       // [Required(ErrorMessage = "*")]
        public string subject { get; set; }
        //[Required(ErrorMessage = "*")]
        [AllowHtml]
        public string message { get; set; }
        //[Required(ErrorMessage = "*")]
        public string smstext { get; set; }      
       
    }
    //AcademicPerformanceReport
    public class AcademicPerformanceReport
    {
        public int districtId { get; set; }
        public int collegeTypeId { get; set; }
        public bool isCollegeEditable { get; set; }
        public int collegeid { get; set; }
        public string collegeCode { get; set; }
        public string collegeName { get; set; }
        public int specializationId { get; set; }
        public string specializationName { get; set; }
        public string academicyear { get; set; }
        public int appearedstudents { get; set; }
        public int passedstudents { get; set; }
        public decimal? passPercentage { get; set; }
        public int collegestatusId { get; set; }
    }
    //PlacementReport
    public class PlacementReport
    {
        public int districtId { get; set; }
        public int collegeTypeId { get; set; }
        public bool isCollegeEditable { get; set; }
        public int collegeid { get; set; }
        public string collegeCode { get; set; }
        public string collegeName { get; set; }
        public int specializationId { get; set; }
        public string specializationName { get; set; }
        public string academicyear { get; set; }
        public int? totalStudentsPassed { get; set; }
        public int? totalStudentsPlaced { get; set; }
        public decimal? PlacedPercentage { get; set; }
        public int collegestatusId { get; set; }
    }
    //InfrastructureReport
    public class InfrastructureReport
    {
        public int districtId { get; set; }
        public int collegeTypeId { get; set; }
        public bool isCollegeEditable { get; set; }
        public int collegeid { get; set; }
        public string collegeCode { get; set; }
        public string collegeName { get; set; }
        public decimal? NoofClassrooms { get; set; }
        public decimal? NoofLaboratorys { get; set; }
        public int collegestatusId { get; set; }
    }
    //SatffStrengthReport
    public class SatffStrengthReport
    {
        public int collegeid { get; set; }
        public int districtId { get; set; }
        public int collegeTypeId { get; set; }
        public bool isCollegeEditable { get; set; }
        public string collegeCode { get; set; }
        public string collegeName { get; set; }
        public int collegestatusId { get; set; }

        public int intake { get; set; }
        public int specializationId { get; set; }
        public string specialization { get; set; }
        public int departmentID { get; set; }
        public string department { get; set; }
        public int degreeID { get; set; }
        public string degree { get; set; }
        public string shift { get; set; }
        public int shiftId { get; set; }
        public int professors { get; set; }
        public int assoProfessors { get; set; }
        public int assisProfessors { get; set; }
        public string ratio { get; set; }


    }

    public class UpdateCollegeStatus
    {
        public int collegeId { get; set; }
        public string collegeCode { get; set; }
        public string collegeName { get; set; }
        public DateTime ? fromDate { get; set; }
        public DateTime? toDate { get; set; }
        public string remarks { get; set; }
        public string collegeType { get; set; }
        public string file { get; set; }
        public bool Active { get; set; }
    }


    public class EmailSendingModel
    {
        [AllowHtml]
        public string Body { get; set; }
        public HttpPostedFileBase EmailIdsFile { get; set; }
        public HttpPostedFileBase Attachment { get; set; }
        public string Subject { get; set; }
    }

}