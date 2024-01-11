using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UAAAS.Models
{
    public class Governingbody
    {
        public int Id { get; set; }
        public int Academicyearid { get; set; }
        public int Collegeid { get; set; }
        public string Collegecode { get; set; }
        public string Collegename { get; set; }
        public string NameoftheGoverningBodyMember { get; set; }
        public short MemberDesignationId { get; set; }
        public string GoverningBodyMemberDesignation { get; set; }
        public string ParentOrganizationwhereworking { get; set; }
        public string DesignationofthememberwhereworkingatparentOrganization { get; set; }
        public string DateofappointmentasGoverningBodymember { get; set; }
        public HttpPostedFileBase SupportingDocument { get; set; }
        public string SupportingDocumentfile { get; set; }
        public HttpPostedFileBase SupportingDocument1 { get; set; }
        public string SupportingDocumentfile1 { get; set; }
        
        public bool Isactive { get; set; }
    }
}