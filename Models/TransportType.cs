﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace UAAAS.Models
{
    [MetadataType(typeof(TransportTypeAttribs))]
    public partial class jntuh_transport_type
    {
        //leave it empty
    }

    public class TransportTypeAttribs
    {
        public int id { get; set; }

        [Required(ErrorMessage = "Required")]
        [StringLength(50, ErrorMessage = "Must be under 50 characters")]
        [Display(Name = "Transport Type")]
        public string transportType { get; set; }

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
}