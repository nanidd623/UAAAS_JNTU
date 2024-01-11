using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace UAAAS.Models
{
    [MetadataType(typeof(UserAttribs))]
    public partial class my_aspnet_users
    {
        //leave it empty
    }

    public class UserAttribs
    {
        public int id { get; set; }
        public int applicationId { get; set; }

        [Required(ErrorMessage = "Required")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Must be 3-100 characters long.")]
        [Display(Name = "Username")]
        [RegularExpression(@"[a-zA-Z0-9_]{3,100}", ErrorMessage = "Valid username required")]
        [Remote("DisallowEditUserName", "Account", AdditionalFields = "id")]
        public string name { get; set; }

        public bool isAnonymous { get; set; }

        [Required(ErrorMessage = "Required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Must be 6-15 characters long.")]
        [DataType(DataType.EmailAddress)]
        [RegularExpression(@"[a-z0-9._%+-]+@[a-z0-9.-]+\.[a-z]{2,4}", ErrorMessage = "Valid email address required.")]
        [Display(Name = "Email Address")]
        public string email { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Status")]
        public bool isApproved { get; set; }

        //[Required(ErrorMessage = "Required")]
        //[StringLength(15, MinimumLength = 6, ErrorMessage = "Must be 6-15 characters long.")]
        //[DataType(DataType.Password)]
        //[Display(Name = "Old Password")]
        //public string OldPassword { get; set; }

        //[Required(ErrorMessage = "Required")]
        //[StringLength(15, MinimumLength = 6, ErrorMessage = "Must be 6-15 characters long.")]
        //[DataType(DataType.Password)]
        //[Display(Name = "Password")]
        //[RegularExpression(@"[a-zA-Z0-9.!#$&]{5,25}", ErrorMessage = "Valid password required. Allowed characters: a-z A-Z 0-9 . ! # $ &")]
        //public string Password { get; set; }

        //[DataType(DataType.Password)]
        //[Display(Name = "Confirm Password")]
        //[System.Web.Mvc.Compare("Password", ErrorMessage = "Password & confirmation password do not match.")]
        //public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Role")]
        public int Roles { get; set; }

        [Required(ErrorMessage = "Required")]
        public string SelectedRole { get; set; }

        public Nullable<System.DateTime> lastActivityDate { get; set; }

    }

    public partial class my_aspnet_users
    {
        public string email { get; set; }
        public bool isApproved { get; set; }
        public int Roles { get; set; }
        public string SelectedRole { get; set; }
    }
}