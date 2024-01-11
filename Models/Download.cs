using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace UAAAS.Models
{
    [MetadataType(typeof(DownloadAttribs))]
    public partial class jntuh_download
    {
        //leave it empty
    }

    public class DownloadAttribs
    {
        public int id { get; set; }

        [Required(ErrorMessage = "Required")]
        [StringLength(100, ErrorMessage = "Must be under 100 characters")]
        [Display(Name = "Title")]
        public string downloadTitle { get; set; }

        [StringLength(500, ErrorMessage = "Must be under 500 characters")]
        [Display(Name = "Url (if any)")]
        public string downloadUrl { get; set; }

        [Display(Name = "Word Document (if any)")]
        public string downloadWord { get; set; }

        [Display(Name = "Excel Document (if any)")]
        public string downloadExcel { get; set; }

        [Display(Name = "PDF Document (if any)")]
        public string downloadPDF { get; set; }

        [Display(Name = "From Date")]
        public Nullable<System.DateTime> downloadFromDate { get; set; }

        [Display(Name = "To Date")]
        public Nullable<System.DateTime> downloadToDate { get; set; }

        [Required(ErrorMessage = "Please Upload File")]
        [Display(Name = "Upload Files")]
        [ValidateFile]
        public HttpPostedFileBase uploadFile1 { get; set; }
        public HttpPostedFileBase uploadFile2 { get; set; }
        public HttpPostedFileBase uploadFile3 { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Status")]
        public bool isActive { get; set; }

        public Nullable<System.DateTime> createdOn { get; set; }
        public Nullable<int> createdBy { get; set; }
        public Nullable<System.DateTime> updatedOn { get; set; }
        public Nullable<int> updatedBy { get; set; }

        public virtual my_aspnet_users my_aspnet_users { get; set; }
        public virtual my_aspnet_users my_aspnet_users1 { get; set; }
    }

    public partial class jntuh_download
    {
        public HttpPostedFileBase uploadFile1 { get; set; }
        public HttpPostedFileBase uploadFile2 { get; set; }
        public HttpPostedFileBase uploadFile3 { get; set; }
    }
    //Customized data annotation validator for uploading file
    public class ValidateFileAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            int MaxContentLength = 1024 * 1024 * 3; //3 MB
            string[] AllowedFileExtensions = new string[] { ".pdf", ".doc", ".docx", ".xls", ".xlsx" };

            var file = value as HttpPostedFileBase;

            if (file == null)
                return false;
            else if (!AllowedFileExtensions.Contains(file.FileName.Substring(file.FileName.LastIndexOf('.')).ToLower()))
            {
                ErrorMessage = "Please upload Your Photo of type: " + string.Join(", ", AllowedFileExtensions);
                return false;
            }
            else if (file.ContentLength > MaxContentLength)
            {
                ErrorMessage = "Your Photo is too large, maximum allowed size is : " + (MaxContentLength / 1024).ToString() + "MB";
                return false;
            }
            else
                return true;
        }
    }
}