using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Security;
using UAAAS.Models;


namespace UAAAS.Controllers.Reports
{
    [ErrorHandling]
    public class RTI_ChairPersonController : Controller
    {
        //
        // GET: /RTI_ChairPerson/

        private uaaasDBContext db = new uaaasDBContext();

       [Authorize(Roles = "Admin,College")]
        public ActionResult Index(string collegeid)
        {
            ViewBag.Colleges = db.jntuh_college.Where(c => c.isActive == true).Select(c => new { collegeId = c.id, collegeName = c.collegeCode + "-" + c.collegeName }).OrderBy(c => c.collegeName).ToList();
            ChairPerson chairPerson = new ChairPerson(); 
            var jntuh_department = db.jntuh_department.ToList();

             if (collegeid != null)
             {
                 int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
                 int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
                 if (userCollegeID == 0)
                 {
                     userCollegeID = userCollegeID = Convert.ToInt32(Utilities.DecryptString(collegeid, WebConfigurationManager.AppSettings["CryptoKey"]));
                 }
                 int userAddressID = db.jntuh_address.Where(a => a.collegeId == userCollegeID && a.addressTye.Equals("SECRETARY")).Select(a => a.id).FirstOrDefault();
                 int userChairPersonID = db.jntuh_college_chairperson.Where(e => e.collegeId == userCollegeID).Select(e => e.id).FirstOrDefault();

                 DateTime todayDate = DateTime.Now.Date;
                 int status = db.jntuh_college_edit_status.Where(editStatus => editStatus.collegeId == userCollegeID &&
                                                                               editStatus.IsCollegeEditable == true &&
                                                                               editStatus.editFromDate <= todayDate &&
                                                                               editStatus.editToDate >= todayDate)
                                                          .Select(editStatus => editStatus.id)
                                                          .FirstOrDefault();
                 if (status > 0 && Roles.IsUserInRole("College"))
                 {
                     //if (ConfigurationManager.AppSettings["ShouldFormsBeDisabled"].ToString() == "true")
                     //{
                     //    ViewBag.IsEditable = false;
                     //}
                     //else
                     //{
                     //    ViewBag.IsEditable = true;
                     //}
                     bool isPageEditable = db.jntuh_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("CC") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

                     if (isPageEditable)
                     {
                         ViewBag.IsEditable = true;
                     }
                     else
                     {
                         ViewBag.IsEditable = false;
                     }
                 }
                 else
                 {
                     ViewBag.IsEditable = false;
                 }

                /// ChairPerson chairPerson = new ChairPerson();

                 if (userChairPersonID == 0)
                 {
                     ViewBag.Norecords = true;
                 }
                 else
                 {
                     chairPerson.collegeId = userCollegeID;
                     chairPerson.addressTye = "SECRETARY";
                     jntuh_college_chairperson jntuh_college_chairperson = db.jntuh_college_chairperson.Find(userChairPersonID);
                     if (jntuh_college_chairperson != null)
                     {
                         chairPerson.id = jntuh_college_chairperson.id;
                         chairPerson.collegeId = jntuh_college_chairperson.collegeId;
                         chairPerson.firstName = jntuh_college_chairperson.firstName;
                         chairPerson.lastName = jntuh_college_chairperson.lastName;
                         chairPerson.surname = jntuh_college_chairperson.surname;
                         chairPerson.designationId = jntuh_college_chairperson.designationId;
                         var designation = db.jntuh_chairperson_designation.Where(d => d.id == chairPerson.designationId).Select(d => d.designationName).FirstOrDefault();
                         ViewBag.designationName = designation;
                         if (designation == "Other Designation")
                         {
                             ViewBag.OtherDesignation = "OtherDesignation";
                         }
                         chairPerson.otherDesignation = jntuh_college_chairperson.otherDesignation;
                         chairPerson.createdBy = jntuh_college_chairperson.createdBy;
                         chairPerson.createdOn = jntuh_college_chairperson.createdOn;
                     }
                     jntuh_address jntuh_address = db.jntuh_address.Where(a => a.collegeId == userCollegeID && a.addressTye == chairPerson.addressTye).Select(a => a).ToList().FirstOrDefault();
                     if (jntuh_address != null)
                     {
                         chairPerson.address = jntuh_address.address;
                         chairPerson.townOrCity = jntuh_address.townOrCity;
                         chairPerson.mandal = jntuh_address.mandal;
                         chairPerson.stateId = jntuh_address.stateId;
                         chairPerson.districtId = jntuh_address.districtId;
                         ViewBag.StateName = db.jntuh_state.Where(s => s.id == chairPerson.stateId).Select(s => s.stateName).FirstOrDefault();
                         ViewBag.DistrictName = db.jntuh_district.Where(d => d.id == chairPerson.districtId && d.isActive == true).Select(d => d.districtName).FirstOrDefault();
                         chairPerson.pincode = jntuh_address.pincode;
                         chairPerson.fax = jntuh_address.fax;
                         chairPerson.landline = jntuh_address.landline;
                         chairPerson.mobile = jntuh_address.mobile;
                         chairPerson.email = jntuh_address.email;

                         ViewBag.State = db.jntuh_state.Where(s => s.isActive == true).ToList();
                         ViewBag.District = db.jntuh_district.Where(d => d.isActive == true).ToList();
                         ViewBag.Designation = db.jntuh_chairperson_designation.Where(s => s.isActive == true).ToList();
                     }
                 }
                 return View(chairPerson);
             }
            return View(chairPerson);
        }

    }
}
