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

namespace UAAAS.Controllers.Reports.RTI_Reports
{
    public class RTI_ChairPersonInformationController : Controller
    {
        private uaaasDBContext db = new uaaasDBContext();
        //
        // GET: /RTI_ChairPersonInformation/
        [Authorize(Roles = "Admin")]
        public ActionResult Index(int? collegeid)
        {
            ViewBag.Colleges = db.jntuh_college.Where(c => c.isActive == true).Select(c => new { collegeId = c.id, collegeName = c.collegeCode + "-" + c.collegeName }).OrderBy(c => c.collegeName).ToList();

            int userAddressID = db.jntuh_address.Where(a => a.collegeId == collegeid && a.addressTye.Equals("SECRETARY")).Select(a => a.id).FirstOrDefault();
            int userChairPersonID = db.jntuh_college_chairperson.Where(e => e.collegeId == collegeid).Select(e => e.id).FirstOrDefault();

            DateTime todayDate = DateTime.Now.Date;
          

            ChairPerson chairPerson = new ChairPerson();

            if (collegeid != null)
            { 
                chairPerson.collegeId = Convert.ToInt32(collegeid);
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
                    chairPerson.designationName = db.jntuh_chairperson_designation.Where(d => d.id == chairPerson.designationId).Select(d => d.designationName).FirstOrDefault();

                    if (chairPerson.designationName == "Other Designation")
                    {
                        chairPerson.otherDesignation = jntuh_college_chairperson.otherDesignation;
                    } 
                    chairPerson.createdBy = jntuh_college_chairperson.createdBy;
                    chairPerson.createdOn = jntuh_college_chairperson.createdOn;
                }
                jntuh_address jntuh_address = db.jntuh_address.Where(a => a.collegeId == collegeid && a.addressTye == chairPerson.addressTye).Select(a => a).ToList().FirstOrDefault();
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
                    chairPerson.website = jntuh_address.website;
                }

            }
            return View(chairPerson); 
        }
        public ActionResult RTI_AllChairPersonsInformationReport()
        {
            List<jntuh_college> colleges = db.jntuh_college.Where(c => c.isActive == true).Select(c => c).ToList();
            List<ChairPerson> chairPersonInformationList = new List<ChairPerson>();

            foreach (var clgInfo in colleges.ToList())
            {
                ChairPerson chairPerson = new ChairPerson();

                int collegeid = clgInfo.id;

                if (collegeid != 0)
                {
                    int userAddressID = db.jntuh_address.Where(a => a.collegeId == collegeid && a.addressTye.Equals("SECRETARY")).Select(a => a.id).FirstOrDefault();
                    int userChairPersonID = db.jntuh_college_chairperson.Where(e => e.collegeId == collegeid).Select(e => e.id).FirstOrDefault();

                    chairPerson.collegeId = Convert.ToInt32(collegeid);
                    chairPerson.addressTye = "SECRETARY";
                   
                    jntuh_college_chairperson jntuh_college_chairperson = db.jntuh_college_chairperson.Find(userChairPersonID);
                    if (jntuh_college_chairperson == null) { ViewBag.Norecords = true; }
                    if (jntuh_college_chairperson != null)
                    {
                        ViewBag.Norecords = false;
                        chairPerson.id = jntuh_college_chairperson.id;
                        chairPerson.collegeId = jntuh_college_chairperson.collegeId;
                        chairPerson.firstName = jntuh_college_chairperson.firstName;
                        chairPerson.lastName = jntuh_college_chairperson.lastName;
                        chairPerson.surname = jntuh_college_chairperson.surname;
                        chairPerson.designationId = jntuh_college_chairperson.designationId;
                        chairPerson.designationName = db.jntuh_chairperson_designation.Where(d => d.id == chairPerson.designationId).Select(d => d.designationName).FirstOrDefault();
                        if (chairPerson.designationName == "Other Designation")
                        {
                            chairPerson.otherDesignation = jntuh_college_chairperson.otherDesignation;
                        } 
                        chairPerson.createdBy = jntuh_college_chairperson.createdBy;
                        chairPerson.createdOn = jntuh_college_chairperson.createdOn;
                    }
                    jntuh_address jntuh_address = db.jntuh_address.Where(a => a.collegeId == collegeid && a.addressTye == chairPerson.addressTye).Select(a => a).ToList().FirstOrDefault();
                    if (jntuh_address != null)
                    {
                        chairPerson.address = jntuh_address.address;
                        chairPerson.townOrCity = jntuh_address.townOrCity;
                        chairPerson.mandal = jntuh_address.mandal;
                        chairPerson.stateId = jntuh_address.stateId;
                        chairPerson.districtId = jntuh_address.districtId;
                        chairPerson.stateName = db.jntuh_state.Where(s => s.id == chairPerson.stateId).Select(s => s.stateName).FirstOrDefault();
                        chairPerson.districtName = db.jntuh_district.Where(d => d.id == chairPerson.districtId && d.isActive == true).Select(d => d.districtName).FirstOrDefault();
                        chairPerson.pincode = jntuh_address.pincode;
                        chairPerson.fax = jntuh_address.fax;
                        chairPerson.landline = jntuh_address.landline;
                        chairPerson.mobile = jntuh_address.mobile;
                        chairPerson.email = jntuh_address.email; 
                    }
                    jntuh_college jntuhcollege = db.jntuh_college.Find(chairPerson.collegeId);
                    if (jntuhcollege != null)
                    {
                        chairPerson.collegeCode = jntuhcollege.collegeCode;
                        chairPerson.collegeName = jntuhcollege.collegeName;
                    } 
                }
                chairPersonInformationList.Add(chairPerson);
            }
            int Count = chairPersonInformationList.Count();

            if (Count != 0)
            {
                Response.ClearContent();
                Response.Buffer = true;
                Response.AddHeader("content-disposition", "attachment; filename=RTI_AllChairPersonssInformation.xls");
                Response.ContentType = "application//vnd.ms-excel";
                return PartialView("~/Views/RTI_ChairPersonInformation/RTI_AllChairPersonsInformationReport.cshtml", chairPersonInformationList);
            }
            else
            {
                return View("~/Views/RTI_ChairPersonInformation/Index.cshtml");
            }
        }

    }
}
