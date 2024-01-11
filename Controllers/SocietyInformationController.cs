using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using UAAAS.Models;
using System.Web.Security;
using System.Data;
using System.Web.Configuration;
using System.Configuration;
using System.IO;

namespace UAAAS.Controllers
{
    [ErrorHandling]
    public class SocietyInformationController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Create(string collegeId)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userID).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                if (collegeId != null)
                {
                    if (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry"))
                    {
                        userCollegeID = Convert.ToInt32(Utilities.DecryptString(collegeId, WebConfigurationManager.AppSettings["CryptoKey"]));
                    }
                }
            }
            if (userCollegeID == 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("Create", "CollegeInformation");
            }
            if (userCollegeID == 0 && (Roles.IsUserInRole("DataEntry") || Roles.IsUserInRole("Admin")))
            {
                return RedirectToAction("Create", "CollegeInformation", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
            }
            int userSocietyID = db.jntuh_address.Where(address => address.collegeId == userCollegeID && address.addressTye.Equals("SOCIETY")).Select(address => address.id).FirstOrDefault();
            int userEstablishmentID = db.jntuh_college_establishment.Where(establishment => establishment.collegeId == userCollegeID).Select(establishment => establishment.id).FirstOrDefault();
            DateTime todayDate = DateTime.Now.Date;
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy =
                db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            int status = db.jntuh_college_edit_status.Where(editStatus => editStatus.academicyearId == prAy && editStatus.collegeId == userCollegeID &&
                                                                                      editStatus.IsCollegeEditable == true &&
                                                                                      editStatus.editFromDate <= todayDate &&
                                                                                      editStatus.editToDate >= todayDate)
                                                                 .Select(editStatus => editStatus.id)
                                                                 .FirstOrDefault();

            if (userCollegeID > 0 && (userSocietyID > 0 || userEstablishmentID > 0) && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("View", "SocietyInformation");
            }
            if (userCollegeID > 0 && userSocietyID > 0 && userEstablishmentID > 0 && (Roles.IsUserInRole("DataEntry") || Roles.IsUserInRole("Admin")))
            {
                return RedirectToAction("Edit", "SocietyInformation", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
            }

            if (userSocietyID == 0 && userEstablishmentID == 0 && status == 0 && Roles.IsUserInRole("College"))
            {
                ViewBag.NotUpload = true;
            }
            else
            {
                ViewBag.NotUpload = false;
            }

            bool isPageEditable = db.jntuh_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("ES") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

            if (isPageEditable)
            {
                ViewBag.IsEditable = true;
            }
            else
            {
                ViewBag.IsEditable = false;
                return RedirectToAction("View", "SocietyInformation");
            }
            SocietyInformation societyInformation = new SocietyInformation();
            societyInformation.collegeId = userCollegeID;
            ViewBag.State = db.jntuh_state.Where(state => state.isActive == true).ToList();
            ViewBag.District = db.jntuh_district.Where(district => district.isActive == true).ToList();
            return View(societyInformation);
        }

        private List<jntuh_district> GetDistricts(int id)
        {
            return db.jntuh_district.Where(district => district.stateId == id && district.isActive == true).ToList();
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult GetDistrictList(string id)
        {
            if (id == string.Empty)
            {
                id = "0";
            }
            var districtList = this.GetDistricts(Convert.ToInt32(id));

            var myData = districtList.Select(district => new SelectListItem()
            {
                Text = district.districtName,
                Value = district.id.ToString(),
            });
            return Json(myData, JsonRequestBehavior.AllowGet);
        }

        // POST: /SocietyInformation/Create
        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        [HttpPost]
        public ActionResult Create(SocietyInformation societyInformation)
        {
            SaveSocietyInformation(societyInformation);
            return View(societyInformation);
        }

        private void SaveSocietyInformation(SocietyInformation societyInformation)
        {
            var collegeAddressMessage = string.Empty;
            var membershipUser = Membership.GetUser(User.Identity.Name);
            if (membershipUser != null)
            {
                var userId = Convert.ToInt32(membershipUser.ProviderUserKey);
                var userCollegeId = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userId).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();
                if (userCollegeId == 0)
                {
                    userCollegeId = societyInformation.collegeId;
                }
                if (userCollegeId == 375)
                {
                    userCollegeId = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
                }

                if (!ModelState.IsValid)
                {
                    var jntuhCollege = db.jntuh_college.Where(r => r.id == userCollegeId).Select(s => s).FirstOrDefault();
                    var collegeEstablishmentDetails = new jntuh_college_establishment();

                    //Registration and Members Documents Saving on 07-02-2020

                    const string societydocuments = "~/Content/Upload/College/SocietyDocument";
                    if (!Directory.Exists(Server.MapPath(societydocuments)))
                    {
                        Directory.CreateDirectory(Server.MapPath(societydocuments));
                    }
                    if (societyInformation.RegistrationDocument != null)
                    {
                        var ext = Path.GetExtension(societyInformation.RegistrationDocument.FileName);
                        if (ext != null && ext.ToUpper().Equals(".PDF"))
                        {
                            if (jntuhCollege != null)
                            {
                                var filename = jntuhCollege.collegeCode + "-" +
                                                  DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "_RD";
                                societyInformation.RegistrationDocument.SaveAs(string.Format("{0}/{1}{2}",
                                    Server.MapPath(societydocuments), filename, ext));
                                societyInformation.RegistrationDocumentfile = string.Format("{0}{1}", filename, ext);
                            }
                        }
                    }
                    else if (!String.IsNullOrEmpty(societyInformation.RegistrationDocumentfile))
                    {
                        collegeEstablishmentDetails.registrationDocument = societyInformation.RegistrationDocumentfile;
                    }
                    if (societyInformation.MembersDetailsDocument != null)
                    {
                        var ext = Path.GetExtension(societyInformation.MembersDetailsDocument.FileName);
                        if (ext != null && ext.ToUpper().Equals(".PDF"))
                        {
                            if (jntuhCollege != null)
                            {
                                var filename = jntuhCollege.collegeCode + "-" + DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "_AM";
                                societyInformation.MembersDetailsDocument.SaveAs(string.Format("{0}/{1}{2}",
                                    Server.MapPath(societydocuments), filename, ext));
                                societyInformation.MembersDetailsDocumentfile = string.Format("{0}{1}", filename, ext);
                            }
                        }
                    }
                    else if (!String.IsNullOrEmpty(societyInformation.MembersDetailsDocumentfile))
                    {
                        //After EDMX Update Write Code
                        collegeEstablishmentDetails.membersDetailsDocument = societyInformation.MembersDetailsDocumentfile;
                    }
                    else
                    {
                        TempData["Success"] = "Added successfully.";
                    }
                    collegeEstablishmentDetails.collegeId = userCollegeId;
                    collegeEstablishmentDetails.registrationDocument = societyInformation.RegistrationDocumentfile;
                    collegeEstablishmentDetails.membersDetailsDocument = societyInformation.MembersDetailsDocumentfile;
                    collegeEstablishmentDetails.societyEstablishmentYear = societyInformation.societyEstablishmentYear;
                    collegeEstablishmentDetails.societyRegisterNumber = societyInformation.societyRegisterNumber;
                    collegeEstablishmentDetails.societyName = societyInformation.societyName;
                    collegeEstablishmentDetails.instituteEstablishedYear = societyInformation.instituteEstablishedYear;
                    collegeEstablishmentDetails.oldsocityname = societyInformation.OldSocietyName;
                    #region New file uploads for permanent affiliation

                    //New file uploads for permanent affiliation
                    if (societyInformation.Societymoudoc != null)
                    {
                        const string societymoudocuments = "~/Content/Upload/College/SocietyMOU";
                        if (!Directory.Exists(Server.MapPath(societymoudocuments)))
                        {
                            Directory.CreateDirectory(Server.MapPath(societymoudocuments));
                        }
                        if (societyInformation.Societymoudoc != null)
                        {
                            var ext = Path.GetExtension(societyInformation.Societymoudoc.FileName);
                            if (ext != null && ext.ToUpper().Equals(".PDF"))
                            {
                                if (jntuhCollege != null)
                                {
                                    var filename = jntuhCollege.collegeCode + "-" +
                                                   DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "_MOU";
                                    societyInformation.Societymoudoc.SaveAs(string.Format("{0}/{1}{2}",
                                        Server.MapPath(societymoudocuments), filename, ext));
                                    societyInformation.Societymoufile = string.Format("{0}{1}", filename, ext);
                                }
                            }
                        }
                    }
                    else
                    {
                        collegeEstablishmentDetails.socitymou = societyInformation.Societybylawsfile;
                    }

                    if (societyInformation.Societybyelawsdoc != null)
                    {
                        const string societybylawdocuments = "~/Content/Upload/College/SocietyByeLaws";
                        if (!Directory.Exists(Server.MapPath(societybylawdocuments)))
                        {
                            Directory.CreateDirectory(Server.MapPath(societybylawdocuments));
                        }
                        if (societyInformation.Societybyelawsdoc != null)
                        {
                            var ext = Path.GetExtension(societyInformation.Societybyelawsdoc.FileName);
                            if (ext != null && ext.ToUpper().Equals(".PDF"))
                            {
                                if (jntuhCollege != null)
                                {
                                    var filename = jntuhCollege.collegeCode + "-" +
                                                   DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "_BL";
                                    societyInformation.Societybyelawsdoc.SaveAs(string.Format("{0}/{1}{2}",
                                        Server.MapPath(societybylawdocuments), filename, ext));
                                    societyInformation.Societybylawsfile = string.Format("{0}{1}", filename, ext);
                                }
                            }

                        }
                    }
                    else
                    {
                        collegeEstablishmentDetails.socitybyelaws = societyInformation.Societybylawsfile;
                    }

                    collegeEstablishmentDetails.socitymou = societyInformation.Societymoufile;
                    collegeEstablishmentDetails.socitybyelaws = societyInformation.Societybylawsfile;
                    #endregion

                    if (societyInformation.firstApprovalDateByAICTE != null)
                    {
                        collegeEstablishmentDetails.firstApprovalDateByAICTE = Utilities.DDMMYY2MMDDYY(societyInformation.firstApprovalDateByAICTE);
                    }
                    else
                    {
                        collegeEstablishmentDetails.firstApprovalDateByAICTE = null;
                    }
                    if (societyInformation.firstAffiliationDateByJNTU != null)
                    {
                        collegeEstablishmentDetails.firstAffiliationDateByJNTU = Utilities.DDMMYY2MMDDYY(societyInformation.firstAffiliationDateByJNTU);
                    }
                    else
                    {
                        societyInformation.firstAffiliationDateByJNTU = null;
                    }
                    collegeEstablishmentDetails.firstBatchCommencementYear = societyInformation.firstBatchCommencementYear;

                    var existEstablishmentId = db.jntuh_college_establishment.Where(establishment => establishment.collegeId == userCollegeId)
                        .Select(establishment => establishment.id)
                        .FirstOrDefault();

                    string collegeEstablishmentMessage;
                    if (existEstablishmentId == 0) //if CollegeEstablishmentID =0 ; then insert the college establishment record
                    {
                        collegeEstablishmentDetails.createdBy = userId;
                        collegeEstablishmentDetails.createdOn = DateTime.Now;
                        db.jntuh_college_establishment.Add(collegeEstablishmentDetails);
                        collegeEstablishmentMessage = "Save";
                    }
                    else //if CollegeEstablishmentID is Exists then Modify the college establishment record
                    {
                        collegeEstablishmentDetails.id = existEstablishmentId;
                        collegeEstablishmentDetails.createdBy = db.jntuh_college_establishment.Where(establishment => establishment.id == existEstablishmentId)
                            .Select(establishment => establishment.createdBy)
                            .FirstOrDefault();
                        collegeEstablishmentDetails.createdOn = db.jntuh_college_establishment.Where(establishment => establishment.id == existEstablishmentId)
                            .Select(establishment => establishment.createdOn)
                            .FirstOrDefault();
                        collegeEstablishmentDetails.updatedBy = userId;
                        collegeEstablishmentDetails.updatedOn = DateTime.Now;
                        db.Entry(collegeEstablishmentDetails).State = EntityState.Modified;
                        collegeEstablishmentMessage = "Update";
                    }
                    db.SaveChanges();

                    //Get the CollegeEstablishmentID after inserting the College Establishment record
                    var collegeEstablishmentId = collegeEstablishmentDetails.id;

                    //if CollegeEstablishmentID exists, insert other records into other tables
                    if (collegeEstablishmentId != 0)
                    {
                        jntuh_address addressDetails = new jntuh_address();
                        addressDetails.collegeId = userCollegeId;
                        addressDetails.addressTye = societyInformation.addressTye;
                        addressDetails.address = societyInformation.address;
                        addressDetails.townOrCity = societyInformation.townOrCity;
                        addressDetails.mandal = societyInformation.mandal;
                        addressDetails.districtId = societyInformation.districtId;
                        addressDetails.stateId = societyInformation.stateId;
                        addressDetails.pincode = societyInformation.pincode;
                        addressDetails.fax = societyInformation.fax;
                        addressDetails.landline = societyInformation.landline;
                        addressDetails.mobile = societyInformation.mobile;
                        addressDetails.email = societyInformation.email;
                        addressDetails.website = societyInformation.website;
                        var addressId = db.jntuh_address.Where(address => address.collegeId == userCollegeId &&
                                                                          address.addressTye == societyInformation.addressTye)
                            .Select(address => address.id)
                            .FirstOrDefault();

                        if (addressId == 0) //if addressID==0 then insert the college establishment address
                        {
                            addressDetails.createdBy = userId;
                            addressDetails.createdOn = DateTime.Now;
                            db.jntuh_address.Add(addressDetails);
                            collegeAddressMessage = "Save";
                        }
                        else //if addressID is exists then Modify the existing college establishment address
                        {
                            addressDetails.id = addressId;
                            addressDetails.createdBy = db.jntuh_address.Where(address => address.id == addressId && address.addressTye == societyInformation.addressTye)
                                .Select(address => address.createdBy)
                                .FirstOrDefault();
                            addressDetails.createdOn = db.jntuh_address.Where(address => address.id == addressId && address.addressTye == societyInformation.addressTye)
                                .Select(address => address.createdOn)
                                .FirstOrDefault();
                            addressDetails.updatedBy = userId;
                            addressDetails.updatedOn = DateTime.Now;
                            db.Entry(addressDetails).State = EntityState.Modified;
                            collegeAddressMessage = "Update";
                        }
                        db.SaveChanges();
                    }
                    if (collegeEstablishmentMessage == "Update" && collegeAddressMessage == "Update")
                    {
                        TempData["Success"] = "Updated successfully.";
                    }
                    else
                    {
                        TempData["Success"] = "Added successfully.";
                    }
                }
            }
            ViewBag.State = db.jntuh_state.Where(state => state.isActive).ToList();
            ViewBag.District = db.jntuh_district.Where(district => district.isActive).ToList();
        }

        // GET: /SocietyInformation/Edit/9
        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Edit(string collegeId)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userID).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                if (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry"))
                {
                    if (collegeId != null)
                    {
                        userCollegeID = Convert.ToInt32(Utilities.DecryptString(collegeId, WebConfigurationManager.AppSettings["CryptoKey"]));
                    }
                }
            }
            int userSocietyID = db.jntuh_address.Where(address => address.collegeId == userCollegeID && address.addressTye.Equals("SOCIETY"))
                                                .Select(address => address.id).FirstOrDefault();
            int userEstablishmentID = db.jntuh_college_establishment.Where(establishment => establishment.collegeId == userCollegeID)
                                                                    .Select(establishment => establishment.id).FirstOrDefault();

            if (userCollegeID == 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("Create", "CollegeInformation");
            }

            if (userSocietyID == 0 && userEstablishmentID == 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("Create", "SocietyInformation");
            }

            if (userSocietyID == 0 && userEstablishmentID == 0 && (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry")))
            {
                return RedirectToAction("Create", "SocietyInformation", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
            }
            DateTime todayDate = DateTime.Now.Date;
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy =
                db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            int status = db.jntuh_college_edit_status.Where(editStatus => editStatus.academicyearId == prAy && editStatus.collegeId == userCollegeID &&
                                                                                      editStatus.IsCollegeEditable == true &&
                                                                                      editStatus.editFromDate <= todayDate &&
                                                                                      editStatus.editToDate >= todayDate)
                                                                 .Select(editStatus => editStatus.id)
                                                                 .FirstOrDefault();
            if (status == 0 && Roles.IsUserInRole("College"))
            {
                ViewBag.IsEditable = false;
                return RedirectToAction("View", "SocietyInformation");
            }
            else
            {
                ViewBag.IsEditable = true;

                ////RAMESH:To-DisableEdit
                //if (ConfigurationManager.AppSettings["ShouldFormsBeDisabled"].ToString() == "true")
                //{
                //    ViewBag.IsEditable = false;
                //    return RedirectToAction("View", "SocietyInformation");
                //}
                //else
                //{
                //    ViewBag.IsEditable = true;
                //}

                bool isPageEditable = db.jntuh_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("ES") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

                if (isPageEditable)
                {
                    ViewBag.IsEditable = true;
                }
                else
                {
                    ViewBag.IsEditable = false;
                    return RedirectToAction("View", "SocietyInformation");
                }
            }

            SocietyInformation societyInformation = new SocietyInformation();
            societyInformation.addressTye = "SOCIETY";
            jntuh_college_establishment jntuh_college_establishment = db.jntuh_college_establishment.Find(userEstablishmentID);
            if (jntuh_college_establishment != null)
            {
                societyInformation.collegeId = userCollegeID;
                societyInformation.id = jntuh_college_establishment.id;
                societyInformation.societyEstablishmentYear = jntuh_college_establishment.societyEstablishmentYear;
                societyInformation.societyRegisterNumber = jntuh_college_establishment.societyRegisterNumber;
                societyInformation.societyName = jntuh_college_establishment.societyName;
                societyInformation.OldSocietyName = jntuh_college_establishment.oldsocityname;
                societyInformation.RegistrationDocumentfile = jntuh_college_establishment.registrationDocument;
                societyInformation.MembersDetailsDocumentfile = jntuh_college_establishment.membersDetailsDocument;
                societyInformation.Societymoufile = jntuh_college_establishment.socitymou;
                societyInformation.Societybylawsfile = jntuh_college_establishment.socitybyelaws;
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

            jntuh_address jntuh_address = db.jntuh_address.Where(a => a.collegeId == userCollegeID && a.addressTye == societyInformation.addressTye).Select(a => a).ToList().FirstOrDefault();
            if (jntuh_address != null)
            {
                societyInformation.address = jntuh_address.address;
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
            return View("Create", societyInformation);

        }

        // POST: /SocietyInformation/Edit
        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        [HttpPost]
        public ActionResult Edit(SocietyInformation societyInformation)
        {
            SaveSocietyInformation(societyInformation);
            return View("View");

            // Commnented & return view changed by Naushad Khan
            // return View("Create", societyInformation);
        }

        [Authorize(Roles = "Admin,College")]
        public ActionResult View(string id)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userID).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                userCollegeID = Convert.ToInt32(Utilities.DecryptString(id, WebConfigurationManager.AppSettings["CryptoKey"]));
            }
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            int userSocietyID = db.jntuh_address.Where(address => address.collegeId == userCollegeID && address.addressTye.Equals("SOCIETY"))
                                                .Select(address => address.id).FirstOrDefault();
            int userEstablishmentID = db.jntuh_college_establishment.Where(establishment => establishment.collegeId == userCollegeID)
                                                                    .Select(establishment => establishment.id).FirstOrDefault();
            DateTime todayDate = DateTime.Now.Date;
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy =
               db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            int status = db.jntuh_college_edit_status.Where(editStatus => editStatus.academicyearId == prAy && editStatus.collegeId == userCollegeID &&
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
                bool isPageEditable = db.jntuh_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("ES") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

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
            SocietyInformation societyInformation = new SocietyInformation();
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
                    societyInformation.OldSocietyName = jntuh_college_establishment.oldsocityname;
                    societyInformation.RegistrationDocumentfile = jntuh_college_establishment.registrationDocument;
                    societyInformation.MembersDetailsDocumentfile = jntuh_college_establishment.membersDetailsDocument;
                    societyInformation.Societymoufile = jntuh_college_establishment.socitymou;
                    societyInformation.Societybylawsfile = jntuh_college_establishment.socitybyelaws;
                    //societyInformation.instituteEstablishedYear = jntuh_college_establishment.instituteEstablishedYear;
                    //if (jntuh_college_establishment.firstApprovalDateByAICTE != null)
                    //{
                    //    societyInformation.firstApprovalDateByAICTE = Utilities.MMDDYY2DDMMYY(jntuh_college_establishment.firstApprovalDateByAICTE.ToString());
                    //}
                    //else
                    //{
                    //    societyInformation.firstApprovalDateByAICTE = string.Empty;
                    //}
                    //if (jntuh_college_establishment.firstAffiliationDateByJNTU != null)
                    //{
                    //    societyInformation.firstAffiliationDateByJNTU = Utilities.MMDDYY2DDMMYY(jntuh_college_establishment.firstAffiliationDateByJNTU.ToString());
                    //}
                    //else
                    //{
                    //    societyInformation.firstAffiliationDateByJNTU = string.Empty;
                    //}
                    //societyInformation.firstBatchCommencementYear = jntuh_college_establishment.firstBatchCommencementYear;
                    societyInformation.collegeId = jntuh_college_establishment.collegeId;
                    societyInformation.createdBy = jntuh_college_establishment.createdBy;
                    societyInformation.createdOn = jntuh_college_establishment.createdOn;
                    societyInformation.updatedBy = jntuh_college_establishment.updatedBy;
                    societyInformation.updatedOn = jntuh_college_establishment.updatedOn;
                }

                jntuh_address jntuh_address = db.jntuh_address.Where(a => a.collegeId == userCollegeID && a.addressTye == societyInformation.addressTye).Select(a => a).ToList().FirstOrDefault();
                if (jntuh_address != null)
                {
                    societyInformation.address = jntuh_address.address;
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
            return View("View", societyInformation);
        }

        //[Authorize(Roles = "Admin,College")]
        //public ActionResult Print(string id)
        //{
        //    int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
        //    int userCollegeID = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userID).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();
        //    if (userCollegeID == 0)
        //    {
        //        userCollegeID = Convert.ToInt32(Utilities.DecryptString(id, WebConfigurationManager.AppSettings["CryptoKey"]));
        //    }
        //    int userSocietyID = db.jntuh_address.Where(address => address.collegeId == userCollegeID && address.addressTye.Equals("SOCIETY"))
        //                                        .Select(address => address.id).FirstOrDefault();
        //    int userEstablishmentID = db.jntuh_college_establishment.Where(establishment => establishment.collegeId == userCollegeID)
        //                                                            .Select(establishment => establishment.id).FirstOrDefault();


        //    SocietyInformation societyInformation = new SocietyInformation();
        //    societyInformation.addressTye = "SOCIETY";
        //    jntuh_college_establishment jntuh_college_establishment = db.jntuh_college_establishment.Find(userEstablishmentID);
        //    if (jntuh_college_establishment != null)
        //    {
        //        societyInformation.id = jntuh_college_establishment.id;
        //        societyInformation.societyEstablishmentYear = jntuh_college_establishment.societyEstablishmentYear;
        //        societyInformation.societyRegisterNumber = jntuh_college_establishment.societyRegisterNumber;
        //        societyInformation.societyName = jntuh_college_establishment.societyName;
        //        societyInformation.instituteEstablishedYear = jntuh_college_establishment.instituteEstablishedYear;
        //        if (jntuh_college_establishment.firstApprovalDateByAICTE != null)
        //        {
        //            societyInformation.firstApprovalDateByAICTE = Utilities.MMDDYY2DDMMYY(jntuh_college_establishment.firstApprovalDateByAICTE.ToString());
        //        }
        //        else
        //        {
        //            societyInformation.firstApprovalDateByAICTE = string.Empty;
        //        }
        //        if (jntuh_college_establishment.firstAffiliationDateByJNTU != null)
        //        {
        //            societyInformation.firstAffiliationDateByJNTU = Utilities.MMDDYY2DDMMYY(jntuh_college_establishment.firstAffiliationDateByJNTU.ToString());
        //        }
        //        else
        //        {
        //            societyInformation.firstAffiliationDateByJNTU = string.Empty;
        //        }
        //        societyInformation.firstBatchCommencementYear = jntuh_college_establishment.firstBatchCommencementYear;
        //        societyInformation.collegeId = jntuh_college_establishment.collegeId;
        //        societyInformation.createdBy = jntuh_college_establishment.createdBy;
        //        societyInformation.createdOn = jntuh_college_establishment.createdOn;
        //        societyInformation.updatedBy = jntuh_college_establishment.updatedBy;
        //        societyInformation.updatedOn = jntuh_college_establishment.updatedOn;
        //    }

        //    jntuh_address jntuh_address = db.jntuh_address.Where(a => a.collegeId == userCollegeID && a.addressTye == societyInformation.addressTye).Select(a => a).ToList().FirstOrDefault();
        //    if (jntuh_address != null)
        //    {
        //        societyInformation.address = jntuh_address.address;
        //        societyInformation.townOrCity = jntuh_address.townOrCity;
        //        societyInformation.mandal = jntuh_address.mandal;
        //        societyInformation.stateId = jntuh_address.stateId;
        //        societyInformation.districtId = jntuh_address.districtId;
        //        societyInformation.pincode = jntuh_address.pincode;
        //        societyInformation.fax = jntuh_address.fax;
        //        societyInformation.landline = jntuh_address.landline;
        //        societyInformation.mobile = jntuh_address.mobile;
        //        societyInformation.email = jntuh_address.email;
        //        societyInformation.website = jntuh_address.website;
        //    }

        //    ViewBag.State = db.jntuh_state.Where(state => state.isActive == true).ToList();
        //    ViewBag.District = db.jntuh_district.Where(district => district.isActive == true).ToList();
        //    ViewBag.StateName = db.jntuh_state.Where(s => s.id == societyInformation.stateId).Select(s => s.stateName).FirstOrDefault();
        //    ViewBag.DistrictName = db.jntuh_district.Where(d => d.id == societyInformation.districtId).Select(d => d.districtName).FirstOrDefault();            
        //    string url = string.Format("{0}://{1}{2}", Request.Url.Scheme, Request.Url.Authority, Url.Content("~"));
        //    societyInformation.jntuh_state = db.jntuh_state.Where(s => s.id == societyInformation.stateId).Select(s => s).FirstOrDefault();
        //    societyInformation.jntuh_district = db.jntuh_district.Where(d => d.id == societyInformation.districtId).Select(d => d).FirstOrDefault();
        //    return this.ViewPdf("SocietyInformation", "Print", societyInformation);
        //}

        public ActionResult UserView(string id)
        {
            int userCollegeID = userCollegeID = Convert.ToInt32(Utilities.DecryptString(id, WebConfigurationManager.AppSettings["CryptoKey"]));
            int userSocietyID = db.jntuh_address.Where(address => address.collegeId == userCollegeID && address.addressTye.Equals("SOCIETY"))
                                                .Select(address => address.id).FirstOrDefault();
            int userEstablishmentID = db.jntuh_college_establishment.Where(establishment => establishment.collegeId == userCollegeID)
                                                                    .Select(establishment => establishment.id).FirstOrDefault();
            SocietyInformation societyInformation = new SocietyInformation();
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

            jntuh_address jntuh_address = db.jntuh_address.Where(a => a.collegeId == userCollegeID && a.addressTye == societyInformation.addressTye).Select(a => a).ToList().FirstOrDefault();
            if (jntuh_address != null)
            {
                societyInformation.address = jntuh_address.address;
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
                ViewBag.StateName = db.jntuh_state.Where(s => s.id == jntuh_address.stateId).Select(s => s.stateName).FirstOrDefault();
                ViewBag.DistrictName = db.jntuh_district.Where(d => d.id == jntuh_address.districtId).Select(d => d.districtName).FirstOrDefault();
            }

            if (jntuh_college_establishment == null && jntuh_address == null)
            {
                ViewBag.NoRecords = true;
            }
            return View("UserView", societyInformation);
        }
    }
}
