using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
namespace UAAAS.Models.Admin
{
    public class FacultyNews
    {
        public int id { get; set; }

        [Display(Name = "Faculty RegistartionNumber")]
        [Remote("CheckRegistration", "NewOnlineRegistration", HttpMethod = "POST" ,ErrorMessage="Registration Number is Not Exists")]
        public string RegistartionNumber { get; set; }

       // [Display(Name = "Faculty Id")]
        //[Required(ErrorMessage = "Required")]
        public int? FacultyId { get; set; }

        [Display(Name = "Title")]
        [Required(ErrorMessage = "Required")]
        [StringLength(500, ErrorMessage = "Must be under 500 characters")]
        public string title { get; set; }

        [Display(Name = "Navigate URL")]
        [StringLength(225, ErrorMessage = "Must be under 225 characters")]
        public string navigateURL { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Status")]
        public Nullable<bool> isActive { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Status")]
        public bool isNews { get; set; }

       // [Required(ErrorMessage = "Required")]
        [Display(Name = "Show Latest News Image")]
        public Nullable<bool> isLatest { get; set; }

        [Display(Name = "Upload Files")]
        public HttpPostedFileBase uploadFile { get; set; }

        public Nullable<System.DateTime> createdOn { get; set; }

        public Nullable<int> createdBy { get; set; }

        public Nullable<System.DateTime> updatedOn { get; set; }

        public Nullable<int> updatedBy { get; set; }

        public virtual my_aspnet_users my_aspnet_users { get; set; }

        public virtual my_aspnet_users my_aspnet_users1 { get; set; }
    }
}