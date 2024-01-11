using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace UAAAS.Models
{
    public class CollegeLibrary
    {
        public int id { get; set; }

        public int collegeId { get; set; }

        [Required(ErrorMessage = "Required")]
        [StringLength(100, ErrorMessage = "Must be under 100 characters")]
        [Display(Name = "Name of the Librarian")]
        public string librarianName { get; set; }

        [Required(ErrorMessage = "Required")]
        [StringLength(250, ErrorMessage = "Must be under 250 characters")]
        [Display(Name = "Qualifications of the Librarian")]
        public string librarianQualifications { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Library phone number (Landline/Mobile)")]
        [RegularExpression(@"^((\+)?(\d{2}[-])?(\d{10}){1})?(\d{11}){0,1}?$", ErrorMessage = "Please enter correct mobile number, ex: 9493929190,04012345678")]
        public string libraryPhoneNumber { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Number of Supporting Staff")]
        public int totalSupportingStaff { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Total Number of Titles")]
        public int totalTitles { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Total Number of Volumes")]
        public int totalVolumes { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Total Number of National Journals")]
        public int totalNationalJournals { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Total Number of International National Journals")]
        public int totalInternationalJournals { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "No. of e-journals")]
        public int totalEJournals { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Seating Capacity of Library	")]
        public int librarySeatingCapacity { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Working Hours of library From: ")]
        public int libraryWorkingHoursFrom { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "To :")]
        public int libraryWorkingHoursTo { get; set; }

        public Nullable<System.DateTime> createdOn { get; set; }
        public Nullable<int> createdBy { get; set; }
        public Nullable<System.DateTime> updatedOn { get; set; }
        public Nullable<int> updatedBy { get; set; }

        public virtual jntuh_college jntuh_college { get; set; }
        public virtual my_aspnet_users my_aspnet_users { get; set; }
        public virtual my_aspnet_users my_aspnet_users1 { get; set; }
    }
}