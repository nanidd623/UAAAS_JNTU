//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace UAAAS.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class jntuh_college_complaints
    {
        public int id { get; set; }
        public int academicyearId { get; set; }
        public string TicketId { get; set; }
        public int roleId { get; set; }
        public int college_faculty_Id { get; set; }
        public Nullable<int> collegeId { get; set; }
        public Nullable<int> complaintId { get; set; }
        public Nullable<int> subcomplaintId { get; set; }
        public string otherComplaint { get; set; }
        public Nullable<System.DateTime> complaintDate { get; set; }
        public string complaintDescription { get; set; }
        public Nullable<int> complaintStatus { get; set; }
        public string email { get; set; }
        public string mobile { get; set; }
        public string facultyAddress { get; set; }
        public Nullable<bool> replayStatus { get; set; }
        public Nullable<System.DateTime> replayDate { get; set; }
        public string complaintFile { get; set; }
        public string replayFile { get; set; }
        public string givenBy { get; set; }
        public Nullable<int> nooftimes { get; set; }
        public string remarks { get; set; }
        public Nullable<bool> isActive { get; set; }
        public Nullable<System.DateTime> createdOn { get; set; }
        public Nullable<int> createdBy { get; set; }
        public Nullable<System.DateTime> updatedOn { get; set; }
        public Nullable<int> updatedBy { get; set; }
        public string complaintOn { get; set; }
    }
}