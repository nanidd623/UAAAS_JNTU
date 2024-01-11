using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Security;
using UAAAS.Models;

namespace UAAAS.Controllers.Reports.RTI_Reports
{
    public class RTI_SocietyInformationController : Controller
    {
        //
        // GET: /RTI_SocietyInformation/
        private uaaasDBContext db = new uaaasDBContext();

        [Authorize(Roles = "Admin")]
        public ActionResult Index(int? collegeid)
        {
            ViewBag.Colleges = db.jntuh_college.Where(c => c.isActive == true).Select(c => new { collegeId = c.id, collegeName = c.collegeCode + "-" + c.collegeName }).OrderBy(c => c.collegeName).ToList();
            SocietyInformation societyInformation = new SocietyInformation();
            var jntuh_department = db.jntuh_department.ToList();

            if (collegeid != null)
            { 
                int userSocietyID = db.jntuh_address.Where(address => address.collegeId == collegeid && address.addressTye.Equals("SOCIETY"))
                                                    .Select(address => address.id).FirstOrDefault();
                int userEstablishmentID = db.jntuh_college_establishment.Where(establishment => establishment.collegeId == collegeid)
                                                                        .Select(establishment => establishment.id).FirstOrDefault();

                if (userSocietyID == 0 && userEstablishmentID == 0)
                {
                    ViewBag.Norecords = true;
                }
                else
                {
                    ViewBag.Norecords = false;
                   /// societyInformation.addressTye = "SOCIETY";
                    jntuh_college_establishment jntuh_college_establishment = db.jntuh_college_establishment.Find(userEstablishmentID);
                    if (jntuh_college_establishment != null)
                    {
                        societyInformation.id = jntuh_college_establishment.id;
                        societyInformation.societyEstablishmentYear = jntuh_college_establishment.societyEstablishmentYear;
                        societyInformation.societyRegisterNumber = jntuh_college_establishment.societyRegisterNumber;
                        societyInformation.societyName = jntuh_college_establishment.societyName;
                        societyInformation.instituteEstablishedYear = jntuh_college_establishment.instituteEstablishedYear;
                        if (jntuh_college_establishment.firstApprovalDateByAICTE != null)
                        {
                            societyInformation.firstApprovalDateByAICTE = Utilities.MMDDYY2DDMMYY(jntuh_college_establishment.firstApprovalDateByAICTE.ToString());
                        }
                        else
                        {
                            societyInformation.firstApprovalDateByAICTE = string.Empty;
                        }
                        if (jntuh_college_establishment.firstAffiliationDateByJNTU != null)
                        {
                            societyInformation.firstAffiliationDateByJNTU = Utilities.MMDDYY2DDMMYY(jntuh_college_establishment.firstAffiliationDateByJNTU.ToString());
                        }
                        else
                        {
                            societyInformation.firstAffiliationDateByJNTU = string.Empty;
                        }
                        societyInformation.firstBatchCommencementYear = jntuh_college_establishment.firstBatchCommencementYear;
                        societyInformation.collegeId = jntuh_college_establishment.collegeId;
                        societyInformation.createdBy = jntuh_college_establishment.createdBy;
                        societyInformation.createdOn = jntuh_college_establishment.createdOn;
                        societyInformation.updatedBy = jntuh_college_establishment.updatedBy;
                        societyInformation.updatedOn = jntuh_college_establishment.updatedOn;
                    }

                    jntuh_address jntuh_address = db.jntuh_address.Where(a => a.collegeId == collegeid && a.addressTye.Equals("SOCIETY")).Select(a => a).ToList().FirstOrDefault();
                    if (jntuh_address != null)
                    {
                        societyInformation.address = jntuh_address.address;
                        societyInformation.addressTye = jntuh_address.addressTye;
                        societyInformation.townOrCity = jntuh_address.townOrCity;
                        societyInformation.mandal = jntuh_address.mandal;
                        societyInformation.stateId = jntuh_address.stateId;
                        societyInformation.districtId = jntuh_address.districtId;
                        societyInformation.pincode = jntuh_address.pincode;
                        societyInformation.fax = jntuh_address.fax;
                        societyInformation.landline = jntuh_address.landline;
                        societyInformation.mobile = jntuh_address.mobile;
                        societyInformation.email = jntuh_address.email;
                        societyInformation.website = jntuh_address.website;
                    }

                    ViewBag.State = db.jntuh_state.Where(state => state.isActive == true).ToList();
                    ViewBag.District = db.jntuh_district.Where(district => district.isActive == true).ToList();
                    ViewBag.StateName = db.jntuh_state.Where(s => s.id == societyInformation.stateId).Select(s => s.stateName).FirstOrDefault();
                    ViewBag.DistrictName = db.jntuh_district.Where(d => d.id == societyInformation.districtId).Select(d => d.districtName).FirstOrDefault();
                }
                return View(societyInformation); 
            }
            return View(societyInformation);
        }

        [Authorize(Roles = "Admin")]
        public ActionResult RTI_AllSocietiesInformationReport()
        {
            List<jntuh_college> colleges = db.jntuh_college.Where(c => c.isActive == true).Select(c => c).ToList();
            List<SocietyInformation> societyInformationList = new List<SocietyInformation>();

            var jntuh_department = db.jntuh_department.ToList();

            foreach (var societyInfo in colleges.ToList())
            {
                SocietyInformation societyInformation = new SocietyInformation(); 
                int collegeid = societyInfo.id;

                if (collegeid != 0)
                { 
                    int userSocietyID = db.jntuh_address.Where(address => address.collegeId == collegeid && address.addressTye.Equals("SOCIETY"))
                                                        .Select(address => address.id).FirstOrDefault();
                    int userEstablishmentID = db.jntuh_college_establishment.Where(establishment => establishment.collegeId == collegeid)
                                                                            .Select(establishment => establishment.id).FirstOrDefault();
                    if (userSocietyID == 0 && userEstablishmentID == 0)
                    {
                        ViewBag.Norecords = true;
                    }
                    else
                    {
                        ViewBag.Norecords = false; 
                        societyInformation.addressTye = "SOCIETY"; 

                        jntuh_college_establishment jntuh_college_establishment = db.jntuh_college_establishment.Find(userEstablishmentID);
                        if (jntuh_college_establishment != null)
                        {
                            societyInformation.id = jntuh_college_establishment.id;
                            societyInformation.societyEstablishmentYear = jntuh_college_establishment.societyEstablishmentYear;
                            societyInformation.societyRegisterNumber = jntuh_college_establishment.societyRegisterNumber;
                            societyInformation.societyName = jntuh_college_establishment.societyName;
                            societyInformation.instituteEstablishedYear = jntuh_college_establishment.instituteEstablishedYear;
                            if (jntuh_college_establishment.firstApprovalDateByAICTE != null)
                            {
                                societyInformation.firstApprovalDateByAICTE = Utilities.MMDDYY2DDMMYY(jntuh_college_establishment.firstApprovalDateByAICTE.ToString());
                            }
                            else
                            {
                                societyInformation.firstApprovalDateByAICTE = string.Empty;
                            }
                            if (jntuh_college_establishment.firstAffiliationDateByJNTU != null)
                            {
                                societyInformation.firstAffiliationDateByJNTU = Utilities.MMDDYY2DDMMYY(jntuh_college_establishment.firstAffiliationDateByJNTU.ToString());
                            }
                            else
                            {
                                societyInformation.firstAffiliationDateByJNTU = string.Empty;
                            }
                            societyInformation.firstBatchCommencementYear = jntuh_college_establishment.firstBatchCommencementYear;
                            societyInformation.collegeId = jntuh_college_establishment.collegeId;
                            societyInformation.createdBy = jntuh_college_establishment.createdBy;
                            societyInformation.createdOn = jntuh_college_establishment.createdOn;
                            societyInformation.updatedBy = jntuh_college_establishment.updatedBy;
                            societyInformation.updatedOn = jntuh_college_establishment.updatedOn;
                        }

                        jntuh_address jntuh_address = db.jntuh_address.Where(a => a.collegeId == collegeid && a.addressTye.Equals("SOCIETY")).Select(a => a).ToList().FirstOrDefault();
                        if (jntuh_address != null)
                        {
                            societyInformation.address = jntuh_address.address;
                            societyInformation.addressTye = jntuh_address.addressTye;
                            societyInformation.townOrCity = jntuh_address.townOrCity;
                            societyInformation.mandal = jntuh_address.mandal;
                            societyInformation.stateId = jntuh_address.stateId;
                            societyInformation.districtId = jntuh_address.districtId;
                            societyInformation.pincode = jntuh_address.pincode;
                            societyInformation.fax = jntuh_address.fax;
                            societyInformation.landline = jntuh_address.landline;
                            societyInformation.mobile = jntuh_address.mobile;
                            societyInformation.email = jntuh_address.email;
                            societyInformation.website = jntuh_address.website;
                            societyInformation.stateName = db.jntuh_state.Where(s => s.id == societyInformation.stateId).Select(s => s.stateName).FirstOrDefault();
                            societyInformation.districtName = db.jntuh_district.Where(d => d.id == societyInformation.districtId).Select(d => d.districtName).FirstOrDefault();
                        }

                        jntuh_college jntuh_college = db.jntuh_college.Where(a => a.id == societyInformation.collegeId).Select(a => a).ToList().FirstOrDefault();
                        if (jntuh_college != null)
                        {
                            societyInformation.collegeCode = jntuh_college.collegeCode;

                            societyInformation.collegeName = jntuh_college.collegeName;
                        }
                    }
                }
                societyInformationList.Add(societyInformation);
            }
            int Count = societyInformationList.Count();

            if (Count != 0)
            {
                Response.ClearContent();
                Response.Buffer = true;
                Response.AddHeader("content-disposition", "attachment; filename=RTI_AllSocietiesInformation.xls");
                Response.ContentType = "application//vnd.ms-excel";
                return PartialView("~/Views/RTI_SocietyInformation/RTI_AllSocietiesInformationReport.cshtml", societyInformationList);
            }
            else
            {
                return View("~/Views/RTI_SocietyInformation/Index.cshtml");
            }
        }

    }
}
