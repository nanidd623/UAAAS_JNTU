using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace UAAAS.Models
{
    [MetadataType(typeof(NewsEventsAttribs))]
    public partial class jntuh_newsevents
    {
        //leave it empty
    }
    public partial class NewsEventsAttribs
    {
        public int id { get; set; }

        [Display(Name = "Type")]
        [Required(ErrorMessage = "Required")]
        public bool isNews { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Title")]
        [StringLength(500, ErrorMessage = "Must be under 500 characters")]
        public string title { get; set; }

        [Display(Name = "Body")]
        [StringLength(1000, ErrorMessage = "Must be under 1000 characters")]
        public string body { get; set; }

        [Display(Name = "Navigate URL")]
        [StringLength(225, ErrorMessage = "Must be under 225 characters")]
        public string navigateURL { get; set; }

        [Display(Name = "Start Date")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public Nullable<System.DateTime> startDate { get; set; }

        [Display(Name = "End Date")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public Nullable<System.DateTime> endDate { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Status")]
        public bool isActive { get; set; }

        [Display(Name = "Upload Files")]
        public HttpPostedFileBase uploadFile { get; set; }

        [Display(Name = "Sort Order")]
        public int newsOrder { get; set; }
        public Nullable<System.DateTime> createdOn { get; set; }
        public Nullable<int> createdBy { get; set; }
        public Nullable<System.DateTime> updatedOn { get; set; }
        public Nullable<int> updatedBy { get; set; }

        public virtual my_aspnet_users my_aspnet_users { get; set; }
        public virtual my_aspnet_users my_aspnet_users1 { get; set; }
    }

    public partial class CollegeDashboardNews
    {
        public int? latest { get; set; }

        public string newstitle { get; set; }
        
        public DateTime? createdDate { get; set; }

        public string url { get; set; }

    }

    public partial class jntuh_newsevents
    {
        public HttpPostedFileBase uploadFile { get; set; }
    }
}