using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace UAAAS.Models
{
    public class CollegeNews
    {
        public int id { get; set; }

        public string collegeName { get; set; }
        public string type { get; set; }
        public string academicyear { get; set; }
        [Display(Name = "College Name")]
        [Required(ErrorMessage = "Required")]
        public int collegeId { get; set; }

        [Display(Name = "Title")]
        [Required(ErrorMessage = "Required")]
        [StringLength(500, ErrorMessage = "Must be under 500 characters")]
        public string title { get; set; }

        [Display(Name = "Navigate URL")]
        [StringLength(225, ErrorMessage = "Must be under 225 characters")]
        public string navigateURL { get; set; }

        [Display(Name = "Start Date")]
        public string startDate { get; set; }

        [Display(Name = "End Date")]
        public string endDate { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Status")]
        public Nullable<bool> isActive { get; set; }

        [Required(ErrorMessage = "Required")]
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

    public class CollegeList
    {
        public int collegeId { get; set; }

        public string collegeName { get; set; }
    }

    public class BulkCollegesNews
    {
        [Display(Name = "Title")]
        [Required(ErrorMessage = "Required")]
        [StringLength(500, ErrorMessage = "Must be under 500 characters")]
        public string title { get; set; }

        [Display(Name = "New Folder Name")]
        //[Required(ErrorMessage = "Required")]
        //[Remote("CheckFolderName", "CollegeNews", AdditionalFields = "NewsType,FolderType", HttpMethod = "POST", ErrorMessage = "Folder Name Path is already Exists. Try to Change The Folder Name.")]
        public string FolderName { get; set; }

        [Display(Name = "Select the Folder Type")]
        //[Required(ErrorMessage = "Required")]
        public string Folder { get; set; }

        [Display(Name = "Select the News Type")]
        [Required(ErrorMessage = "Required")]
        public string NewsType { get; set; }

        [Display(Name = "Select the Student Type")]       
        public string StudentBASNewsType { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Status")]
        public Nullable<bool> isActive { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Show Latest News Image")]
        public Nullable<bool> isLatest { get; set; }

        [Required(ErrorMessage = "Please select file.")]
        [Display(Name = "Browse File")]
        public HttpPostedFileBase[] files { get; set; }

        [Display(Name = "Browse File")]
        public HttpPostedFileBase ExcelFile { get; set; }
    }

    public class FailureClass
    {
        public string ItemName { get; set; }
        public string reason { get; set; }
    }
}