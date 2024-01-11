using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace UAAAS.Models
{
    public class FacultyComplaints
    {
        [Display(Name = "complaintFile")]
        [Required(ErrorMessage = "*")]
        public string RegistrationNumber { get; set; }
        public string Facultyname { get; set; }
        public string Collegecode { get; set; }
        public string Collegename { get; set; }
        public int Facultyid { get; set; }
        public string fsid { get; set; }
        public int collegeId { get; set; }
        public string OthercollegeorRegno { get; set; }
        [Required(ErrorMessage = "*")]
        public string Facultycomplaintdate { get; set; }
        //[Required(ErrorMessage = "*")]
        [Display(Name = "complaintFile")]
        public HttpPostedFileBase FacultycomplaintFile { get; set; }
        public string FacultycomplaintFileview { get; set; }
        [Required(ErrorMessage = "*")]
        public int complaintid { get; set; }
        public string givenBy { get; set; }
        public int givenById { get; set; }
        public string OthergivenBy { get; set; }
        public string remarks { get; set; }
        public string complaintname { get; set; }
        public string otherscomplaint { get; set; }
        public string PhdUniversity { get; set; }
        public int Phdpassedyear { get; set; }
        public int Profiledepartmentid { get; set; }
        public string Profiledepartment { get; set; }

        [StringLength(100, MinimumLength = 6, ErrorMessage = "Must be 6-100 characters long.")]
        [DataType(DataType.EmailAddress)]
        [RegularExpression(@"[a-z0-9._%+-]+@[a-z0-9.-]+\.[a-z]{2,4}", ErrorMessage = "Invalid email")]
        public string Email { get; set; }

        [StringLength(10, ErrorMessage = "Maximum 10 characters")]
        [RegularExpression(@"\d{10}", ErrorMessage = "Invalid mobile")]
        public string Moblie { get; set; }

    }

    public class FacultyComplaintsClass : FacultyComplaints
    {
        #region New Properties Added by Siva..

        public int id { get; set; }
        public string TicketId { get; set; }

        [Required(ErrorMessage = "*")]
        [StringLength(500, ErrorMessage = "Maximum 500 characters are allowed for address.")]
        public string FacultyAddress { get; set; }

        [Required(ErrorMessage = "*")]
        [StringLength(1000, ErrorMessage = "Maximum 500 characters are allowed for Description.")]
        public string ComplaintDesc { get; set; }

        public string text { get; set; }

        [Required(ErrorMessage = "*")]
        public string District { get; set; }

        public string gender { get; set; }


        public int complaintStatus { get; set; }

        [Required(ErrorMessage = "*")]
        public int complaintStatusId { get; set; }
        public string complaintStatusName { get; set; }

        public string complaintFileview { get; set; }

        public string complaintdate { get; set; }
        public string Replaycomplaintsfile { get; set; }
        public string Replayremarks { get; set; }
        #endregion

        #region new properties added by rakesh

        public HttpPostedFileBase Collegesupportingdocument { get; set; }
        public string Collegesupportingdocumentname { get; set; }
        //[Required(ErrorMessage = "*")]
        [StringLength(1000, ErrorMessage = "Maximum 500 characters are allowed for Description.")]
        public string CollegeRemarks { get; set; }
        public string createddate { get; set; }
        #endregion
    }

    public class Collegecomplaints
    {
        public int id { get; set; }
        public int College_faculryid { get; set; }
        public int grievanceassiegnedto { get; set; }
        public int Grievancetype { get; set; }
        public int Grievanceid { get; set; }
        public int Subgrievanceid { get; set; }
        public string Ticketid { get; set; }
        public int Ticketstatus { get; set; }
        public string Collegeletter { get; set; }
        public string Collegesupportingdocument { get; set; }
        public string CollegeRemarks { get; set; }
        public string adminremarks { get; set; }
        public int ticketclosedby { get; set; }
        public DateTime ticketclosedon { get; set; }
        public DateTime admincreatedon { get; set; }
        public int admincreatedby { get; set; }
        public DateTime createdon { get; set; }
        public int createdby { get; set; }
        public int adminupdatedon { get; set; }
        public int adminupdatedby { get; set; }
        public DateTime updatedon { get; set; }
        public int updatedby { get; set; }
        public string RegistrationNumber { get; set; }
        public string Name { get; set; }
        public string Grievancename { get; set; }
        public string GrievanceDate { get; set; }

    }
    public class ComplaintStats
    {
        public int cmpltId { get; set; }
        public string cmpltName { get; set; }
    }
}