using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace UAAAS.Models
{
    public class CollegFacultyVerification
    {
        public int Id { get; set; }
        public int FacultyId { get; set; }
        public int LabelId { get; set; }
        public string LabelName { get; set; }
        [Required(ErrorMessage="Required")]
        public Nullable<bool> IsValid { get; set; }
        public int VerificationOfficer { get; set; }
        public int CreatedBy { get; set; }
        public System.DateTime CreatedOn { get; set; }
        public Nullable<int> UpdatedBy { get; set; }
        public Nullable<System.DateTime> UpdatedOn { get; set; }
        public List<int> VerificationOfficers { get; set; }
        public int loggedinUserId { get; set; }

        public virtual jntuh_college_faculty_registered jntuh_college_faculty_registered { get; set; }
       // public virtual jntuh_registered_faculty_labels jntuh_registered_faculty_labels { get; set; }
        public virtual my_aspnet_users my_aspnet_users { get; set; }
    }
}