using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
namespace UAAAS.Models
{
    public class CollgeDirector
    {
        public bool IsDirectorChecked { get; set; }

        public int id { get; set; }
        public int collegeId { get; set; }
        public string type { get; set; }

        [Display(Name = "First Name")]
        public string firstName { get; set; }

        [Display(Name = "Last Name")]
        public string lastName { get; set; }

        [Display(Name = "Surname")]
        public string surname { get; set; }

        [Display(Name = "Qualification")]
        public int qualificationId { get; set; }

        //[Display(Name = "Qualification")]
        //public string qualification { get; set; }

        [Display(Name = "Date Of Appointment")]
        public string dateOfAppointment { get; set; }

        [Display(Name = "Date Of Resignation")]
        public Nullable<System.DateTime> dateOfResignation { get; set; }

        [Display(Name = "Date Of Birth")]
        public string dateOfBirth { get; set; }

        [StringLength(11, ErrorMessage = "Must be 11 digits, ex: 04012345678")]
        [Display(Name = "Fax")]
        [RegularExpression(@"\d{11}", ErrorMessage = "Please enter correct fax number, ex: 04012345678")]
        public string fax { get; set; }

        [StringLength(11, ErrorMessage = "Must be 11 digits, ex: 04012345678")]
        [Display(Name = "Landline")]
        [RegularExpression(@"\d{11}", ErrorMessage = "Please enter correct landline number, ex: 04012345678")]
        public string landline { get; set; }

        [StringLength(10, ErrorMessage = "Must be 10 digits, ex: 9493929190")]
        [Display(Name = "Mobile")]
        [RegularExpression(@"\d{10}", ErrorMessage = "Please enter correct mobile number, ex: 9493929190")]
        public string mobile { get; set; }

        [StringLength(100, ErrorMessage = "Must be under 100 characters")]
        [RegularExpression(@"[a-z0-9._%+-]+@[a-z0-9.-]+\.[a-z]{2,4}", ErrorMessage = "Please enter correct email")]
        [Display(Name = "Email")]
        public string email { get; set; }

        [Display(Name = "Department")]
        public Nullable<int> departmentId { get; set; }
        [Display(Name = "Department")]
        public string department { get; set; }

        [Display(Name = "Ph.D Subject")]
        public Nullable<int> phdId { get; set; }
        [Display(Name = "Phd")]
        public string phd { get; set; }

        [Display(Name = "Ph.D Awarded From")]
        public string phdFromUniversity { get; set; }

        [RegularExpression(@"\d{4}", ErrorMessage = "Must be 4 digits, ex: 2014")]
        [Display(Name = "Ph.D Year")]
        public Nullable<int> phdYear { get; set; }

        //[Required(ErrorMessage = "*")]
        [Display(Name = "Photo")]
        public HttpPostedFileBase DirectorPhotoDocument { get; set; }
        public string DirectorPhoto { get; set; }

        public Nullable<System.DateTime> createdOn { get; set; }
        public Nullable<int> createdBy { get; set; }
        public Nullable<System.DateTime> updatedOn { get; set; }
        public Nullable<int> updatedBy { get; set; }

        public virtual jntuh_college jntuh_college { get; set; }
        public virtual jntuh_qualification jntuh_qualification { get; set; }
        public virtual jntuh_department jntuh_department { get; set; }
        public virtual jntuh_phd_subject jntuh_phd_subject { get; set; }
        public virtual my_aspnet_users my_aspnet_users { get; set; }
        public virtual my_aspnet_users my_aspnet_users1 { get; set; }
    }
}