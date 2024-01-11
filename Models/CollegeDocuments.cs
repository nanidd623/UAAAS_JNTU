using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace UAAAS.Models
{
    public class CollegeDocuments
    {
        public int id { get; set; }
        public int collegeId { get; set; }
        [Required(ErrorMessage = "Required")]
        public int documentId { get; set; }
        public string documentName { get; set; }
        public HttpPostedFileBase scannedDocument { get; set; }
        public string scannedCopy { get; set; }
        public Nullable<System.DateTime> createdOn { get; set; }
        public Nullable<int> createdBy { get; set; }
        public Nullable<System.DateTime> updatedOn { get; set; }
        public Nullable<int> updatedBy { get; set; }

        public virtual jntuh_college jntuh_college { get; set; }
        public virtual my_aspnet_users my_aspnet_users { get; set; }
        public virtual my_aspnet_users my_aspnet_users1 { get; set; }
    }
}