using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Web.Mvc;
using System.Web.Security;

namespace UAAAS.Models
{

    public class ChangePasswordModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string OldPassword { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [System.Web.Mvc.Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class ResetPasswordModel
    {
        [Required(ErrorMessage = "Required")]
        //[StringLength(100, MinimumLength = 5, ErrorMessage = "Must be 5-100 characters long.")]
        [Display(Name = "Username")]
        //[RegularExpression(@"[a-zA-Z0-9_]{5,100}", ErrorMessage = "Valid username required")]
        public string UserName { get; set; }

        //[Required(ErrorMessage = "Required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Must be 6-15 characters long.")]
        [DataType(DataType.EmailAddress)]
        [RegularExpression(@"[a-z0-9._%+-]+@[a-z0-9.-]+\.[a-z]{2,4}", ErrorMessage = "Valid email address required.")]
        [Display(Name = "Email Address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Registration Number")]
        public string RegistrationNumber { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "PAN Number")]
        [RegularExpression(@"[A-Z]{3}[P][A-Z]{1}\d{4}[A-Z]{1}", ErrorMessage = "Invalid PAN number")]
        public string PanNumber { get; set; }

    }

    public class LogOnModel
    {
        [Required(ErrorMessage = "Required")]
        [Display(Name = "Username")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Required")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }

        public int UserId { get; set; }
    }

    public class RegisterModel
    {
        [Required(ErrorMessage = "Required")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Must be 3-100 characters long.")]
        [Display(Name = "Username")]
        [RegularExpression(@"[a-zA-Z0-9_]{3,100}", ErrorMessage = "Valid username required")]
        [Remote("DisallowUserName", "Account")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Must be 6-100 characters long.")]
        [DataType(DataType.EmailAddress)]
        [RegularExpression(@"[a-z0-9._%+-]+@[a-z0-9.-]+\.[a-z]{2,4}", ErrorMessage = "Valid email address required.")]
        [Display(Name = "Email Address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Required")]
        [StringLength(15, MinimumLength = 6, ErrorMessage = "Must be 6-15 characters long.")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        [RegularExpression(@"[a-zA-Z0-9.!#$&@]{5,25}", ErrorMessage = "Valid password required. Allowed characters: a-z A-Z 0-9 . ! # $ & @")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [System.Web.Mvc.Compare("Password", ErrorMessage = "Password & confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Role")]
        public int Roles { get; set; }

        [Required(ErrorMessage = "Required")]
        public string SelectedRole { get; set; }

        //[Required(ErrorMessage = "Required")]
        [Display(Name = "College")]
        public int collegeID { get; set; }

        public string selectedCollege { get; set; }

    }
    public class FacultyForgotEmail
    {

        [Required(ErrorMessage = "Please Enter RegistrationNumber")]
        [StringLength(20)]
        public string RegistrationNumber { get; set; }

        [Required(ErrorMessage = "Please Enter PAN Number")]
        [RegularExpression(@"[A-Z]{3}[P][A-Z]{1}\d{4}[A-Z]{1}", ErrorMessage = "Invalid PAN number")]
        [Display(Name = "PAN Number")]
        public string PanNumber { get; set; }

         [Required(ErrorMessage = "Please Enter Date of Birth")]
         [Display(Name = "Date Of Birth")]
        public DateTime? DateofBirth { get; set; }

    }
    public class GeneratePasswordModel
    {
        [Display(Name = "Faculty Registration Number")]
        public string RegistrationNumber { get; set; }

        [Display(Name = "Date of Birth")]
        //[DataType(DataType.Date)]
        //[DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? DateOfBirth { get; set; }

        //  [Required(ErrorMessage = "*")]
        [RegularExpression(@"[A-Z]{3}[P][A-Z]{1}\d{4}[A-Z]{1}", ErrorMessage = "Invalid PAN number")]
        [Display(Name = "PAN Number")]
        public string PANNumber { get; set; }

        // [Required(ErrorMessage = "")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Must be 6-100 characters long.")]
        [DataType(DataType.EmailAddress)]
        [RegularExpression(@"[a-z0-9._%+-]+@[a-z0-9.-]+\.[a-z]{2,4}", ErrorMessage = "Invalid email")]
        [Display(Name = "Email Address")]
        public string Email { get; set; }

        public int? CollegeId { get; set; }
        public string CollegeName { get; set; }
        public string CollegeCode { get; set; }

        // [Required(ErrorMessage = "*")]
        [StringLength(10, ErrorMessage = "Maximum 10 characters")]
        [RegularExpression(@"\d{10}", ErrorMessage = "Invalid mobile")]
        [Display(Name = "Mobile No")]
        public string Mobile { get; set; }

        [Display(Name = "Mother's Name")]
        public string MotherName { get; set; }

        [Display(Name = "10th Year of Passing")]
        public int TenthYear { get; set; }

        [Display(Name = "UG Year of Passing")]
        public int UGYear { get; set; }


    }


    public class ChangeuserName
    {
        [Display(Name = "Registration Number")]
        [Required(ErrorMessage = "*")]
        public string RegistrationNumber { get; set; }

        [Required(ErrorMessage = "*")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Must be 6-100 characters long.")]
        [DataType(DataType.EmailAddress)]
        [RegularExpression(@"[a-z0-9._%+-]+@[a-z0-9.-]+\.[a-z]{2,4}", ErrorMessage = "Invalid email")]
        [Display(Name = "Current Email Address")]
        public string OldEmailId { get; set; }

        [Required(ErrorMessage = "*")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Must be 6-100 characters long.")]
        [DataType(DataType.EmailAddress)]
        [RegularExpression(@"[a-z0-9._%+-]+@[a-z0-9.-]+\.[a-z]{2,4}", ErrorMessage = "Invalid email")]
        [Display(Name = "New Email Address")]
        [Remote("CheckEmail", "Account", HttpMethod = "POST", ErrorMessage = "Email Already exists.")]
        public string NewEmailId { get; set; }


       // [Required(ErrorMessage = "*")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Must be 6-100 characters long.")]
        [DataType(DataType.EmailAddress)]
        [RegularExpression(@"[a-z0-9._%+-]+@[a-z0-9.-]+\.[a-z]{2,4}", ErrorMessage = "Invalid email")]
        [System.Web.Mvc.Compare("NewEmailId", ErrorMessage = "The new email and confirmation email do not match.")]
        [Display(Name = "Confirm Email Address")]
        public string ConfirmEmailId { get; set; }


        [Required(ErrorMessage = "*")]
        [Display(Name = "Date of Birth")]
        public DateTime DateofBirth { get; set; }

        [Required(ErrorMessage = "*")]
        [RegularExpression(@"[A-Z]{3}[P][A-Z]{1}\d{4}[A-Z]{1}", ErrorMessage = "Invalid PAN number")]
        [Display(Name = "PAN Number")]
        public string PanNumber { get; set; }

        [Required(ErrorMessage = "*")]
        [StringLength(10, ErrorMessage = "Maximum 10 characters")]
        [RegularExpression(@"\d{10}", ErrorMessage = "Invalid mobile")]
        [Display(Name = "Mobile No")]
        public string MobileNo { get; set; }
    }


    public class ResteCollegePassword
    {
        [Required(ErrorMessage = "Required")]
        //[StringLength(100, MinimumLength = 5, ErrorMessage = "Must be 5-100 characters long.")]
        [Display(Name = "Username")]
        //[RegularExpression(@"[a-zA-Z0-9_]{5,100}", ErrorMessage = "Valid username required")]
        public string UserName { get; set; }
    }

}
