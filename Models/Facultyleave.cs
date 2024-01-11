using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.Web;
using UAAAS.Controllers.OnlineRegistration;

namespace UAAAS.Models
{
    public class Facultyleave
    {
        public int id { get; set; }
        public int CollegeId { get; set; }
        public string CollegeName { get; set; }
        public string CollegeCode { get; set; }
        public string Leavetype { get; set; }
        public int Leavetypeid { get; set; }
        [Required(ErrorMessage = "*")]
        [Display(Name = "Registrationnumber")]
        public string RegistrationNumber { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string Middlename { get; set; }
        public int facultyId { get; set; }

        public DateTime Leavefromdate { get; set; }
        public DateTime Leavetodate { get; set; }
        [Required(ErrorMessage = "*")]
        [Display(Name = "FROM")]
        public string SLeavefromdate { get; set; }
        [Required(ErrorMessage = "*")]
        [Display(Name = "TO")]
        public string SLeavetodate { get; set; }
        public bool isDelete { get; set; }
        [Required(ErrorMessage = "*")]
        public HttpPostedFileBase Documentfile { get; set; }
        public int noofdays { get; set; }
        public Nullable<System.DateTime> createdOn { get; set; }
        public Nullable<int> createdBy { get; set; }
        public Nullable<System.DateTime> updatedOn { get; set; }
        public Nullable<int> updatedBy { get; set; }
        public string Documentview { get; set; }

        public List<Facultyleave> facultyleaveslist { get; set; }
    }
}