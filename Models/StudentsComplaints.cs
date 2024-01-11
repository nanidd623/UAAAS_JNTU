using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace UAAAS.Models
{
    public class StudentsComplaints
    {
        public int id { get; set; }
        public int academicyearid { get; set; }
        public string TicketId { get; set; }
        public int collegeId { get; set; }
        public string Collegecode { get; set; }
        public string Collegename { get; set; }

        [Required(ErrorMessage = "*")]
        public int complaintid { get; set; }
        public string complaintname { get; set; }

        public string otherscomplaint { get; set; }

        [Required(ErrorMessage = "*")]
        public string complaintdate { get; set; }

        [Required(ErrorMessage = "*")]
        [StringLength(1000, ErrorMessage = "Maximum 500 characters are allowed for Description.")]
        public string ComplaintDesc { get; set; }

        [Display(Name = "complaintFile")]
        public HttpPostedFileBase complaintFile { get; set; }
        public string complaintFileview { get; set; }

        [Display(Name = "Student IDProof")]
        public HttpPostedFileBase IDProofFile { get; set; }
        public string IDProof { get; set; }

        [Display(Name = "HallticketNo")]
        [StringLength(10, MinimumLength = 10,ErrorMessage="Maximum 10 Characters")]
        [Required(ErrorMessage = "*")]
        public string HallticketNo { get; set; }
        [Required(ErrorMessage = "*")]
        public string studentfirstname { get; set; }
        [Required(ErrorMessage = "*")]
        public string studentlastname { get; set; }
        public string studentfullname { get; set; }
        [Required(ErrorMessage = "*")]
        public string studentgender { get; set; }
        [Required(ErrorMessage = "*")]
        public string studentDOB { get; set; }

        [Required(ErrorMessage = "*")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Must be 6-100 characters long.")]
        [DataType(DataType.EmailAddress)]
        [RegularExpression(@"[a-z0-9._%+-]+@[a-z0-9.-]+\.[a-z]{2,4}", ErrorMessage = "Invalid email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "*")]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "Maximum 10 characters")]
        [RegularExpression(@"\d{10}", ErrorMessage = "Invalid mobile")]
        public string Moblie { get; set; }

        [Required(ErrorMessage = "*")]
        [StringLength(500, ErrorMessage = "Maximum 500 characters are allowed for address.")]
        public string Address { get; set; }
        [Required(ErrorMessage = "*")]
        public string city { get; set; }
        [Required(ErrorMessage = "*")]
        public string District { get; set; }
        
        [Required(ErrorMessage = "*")]
        public string Guardianname { get; set; }

        [Required(ErrorMessage = "*")]
        [StringLength(10, MinimumLength = 10 , ErrorMessage = "Maximum 10 characters")]
        [RegularExpression(@"\d{10}", ErrorMessage = "Invalid mobile")]
        public string Guardianmobile { get; set; }

        [Required(ErrorMessage = "*")]
        public int coursestudiedId { get; set; }
        public string coursestudied { get; set; }
        [Required(ErrorMessage = "*")]
        public int courseId { get; set; }
        public string course { get; set; }
        //[Required(ErrorMessage = "*")]
        public int departmentId { get; set; }
        public string department { get; set; }
        public int yearstudy { get; set; }

        public string replystatus { get; set; }
        public string replydate { get; set; }
        public string replyfile { get; set; }
        public string remarks { get; set; }

        public string text { get; set; }
        public int complaintStatus { get; set; }
        public string complaintStatusName { get; set; }
        public string Replaycomplaintsfile { get; set; }
        public string Replayremarks { get; set; }
    }
}