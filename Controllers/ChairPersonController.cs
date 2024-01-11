using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Security;
using UAAAS.Models;

namespace UAAAS.Controllers
{
    [ErrorHandling]
    public class ChairPersonController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();
        //
        // GET: /ChairPerson/
        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Index()
        {
            return View();
        }

        // GET: /CollegeInformation/Create
        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Create(string collegeId)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();

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
            if (userCollegeID == 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("Create", "CollegeInformation");
            }
            if (userCollegeID == 0 && (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry")))
            {
                return RedirectToAction("Edit", "CollegeInformation", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
            }   
            int userAddressID = db.jntuh_address.Where(a => a.collegeId == userCollegeID && a.addressTye.Equals("SECRETARY")).Select(a => a.id).FirstOrDefault();
            int userChairPersonID = db.jntuh_college_chairperson.Where(e => e.collegeId == userCollegeID).Select(e => e.id).FirstOrDefault();

            DateTime todayDate = DateTime.Now.Date;
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy =
                db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            int status = db.jntuh_college_edit_status.Where(editStatus => editStatus.collegeId == userCollegeID &&
                                                                                      editStatus.IsCollegeEditable == true &&
                                                                                      editStatus.editFromDate <= todayDate &&
                                                                                      editStatus.editToDate >= todayDate)
                                                                 .Select(editStatus => editStatus.id)
                                                                 .FirstOrDefault();
            if (userCollegeID > 0 && userChairPersonID > 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("View", "ChairPerson");
            }
            if (userCollegeID > 0 && userChairPersonID > 0 && (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry")))
            {
                return RedirectToAction("Edit", "ChairPerson", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
            }

            if (userChairPersonID == 0 && status == 0 && Roles.IsUserInRole("College"))
            {
                ViewBag.NotUpload = true;
            }
            else
            {
                ViewBag.NotUpload = false;
            }

            bool isPageEditable = db.jntuh_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("CC") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

            if (isPageEditable)
            {
                ViewBag.IsEditable = true;
            }
            else
            {
                ViewBag.IsEditable = false;
                return RedirectToAction("View", "ChairPerson");
            }
            
            ChairPerson chairPerson = new ChairPerson();
            chairPerson.collegeId = userCollegeID;
            ViewBag.State = db.jntuh_state.Where(s => s.isActive == true).ToList();
            ViewBag.District = db.jntuh_district.Where(s => s.isActive == true).ToList();
            ViewBag.Designation = db.jntuh_chairperson_designation.Where(s => s.isActive == true).ToList();
            chairPerson.pincode = null;
            return View(chairPerson);
        }    
    
        // POST: /CollegeInformation/Create
        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        [HttpPost]
        public ActionResult Create(ChairPerson chairPerson)
        {
            SaveInformation(chairPerson);
            var designation = db.jntuh_chairperson_designation.Where(d => d.id == chairPerson.designationId).Select(d => d.designationName).FirstOrDefault();
            if (designation == "Other Designation")
            {
                ViewBag.OtherDesignation = "OtherDesignation";
            }
            return View(chairPerson);
        }

        private void SaveInformation(ChairPerson chairPerson)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                userCollegeID = chairPerson.collegeId;
            }
            var resultMessage = string.Empty;
            if (!ModelState.IsValid)
            {
                int createdBy = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
                jntuh_college_chairperson jntuh_college_chairperson = new jntuh_college_chairperson();
                jntuh_college_chairperson.collegeId = userCollegeID;
                jntuh_college_chairperson.firstName = chairPerson.firstName;
                jntuh_college_chairperson.lastName = chairPerson.lastName == null ? string.Empty : chairPerson.lastName;
                jntuh_college_chairperson.surname = chairPerson.surname;
                jntuh_college_chairperson.designationId = chairPerson.designationId;
                var designation = db.jntuh_chairperson_designation.Where(d => d.id == chairPerson.designationId).Select(d => d.designationName).FirstOrDefault();
                if (designation == "Other Designation")
                {
                    jntuh_college_chairperson.otherDesignation = chairPerson.otherDesignation;
                }
                else
                {
                    jntuh_college_chairperson.otherDesignation = null;
                }
                jntuh_college_chairperson.createdBy = createdBy;
                jntuh_college_chairperson.createdOn = DateTime.Now;
               string strCollegeCode = db.jntuh_college.Where(c => c.id == userCollegeID).Select(c => c.collegeCode).FirstOrDefault();
               string photoPath = "~/Content/Upload/College/ChairPerson";
                if (chairPerson.ChairpersionPhoto != null)
                {
                    if (!Directory.Exists(Server.MapPath(photoPath)))
                    {
                        Directory.CreateDirectory(Server.MapPath(photoPath));
                    }

                    var ext = Path.GetExtension(chairPerson.ChairpersionPhoto.FileName);

                    if (ext.ToUpper().Equals(".JPEG") || ext.ToUpper().Equals(".JPG"))
                    {
                        string fileName = "C-" + strCollegeCode + "-" + DateTime.Now.ToString("yyyyMMdd-HHmmssffffff");
                        chairPerson.ChairpersionPhoto.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(photoPath), fileName, ext));
                        jntuh_college_chairperson.Photo = string.Format("{0}{1}", fileName, ext);
                    }
                }
                int chairPersonID = db.jntuh_college_chairperson.Where(p => p.collegeId == userCollegeID).Select(p => p.id).FirstOrDefault();
                if (chairPersonID == 0)     //if college id = 0; then insert the chairPerson record
                {
                    db.jntuh_college_chairperson.Add(jntuh_college_chairperson);
                }
                else                    //if college id exists then modify the existing college record
                {
                    jntuh_college_chairperson.id = chairPersonID;
                    jntuh_college_chairperson.createdBy = db.jntuh_college_chairperson.Where(c => c.id == chairPersonID).Select(c => c.createdBy).FirstOrDefault();
                    jntuh_college_chairperson.createdOn = db.jntuh_college_chairperson.Where(c => c.id == chairPersonID).Select(c => c.createdOn).FirstOrDefault();
                    jntuh_college_chairperson.updatedBy = createdBy;
                    jntuh_college_chairperson.updatedOn = DateTime.Now;
                    db.Entry(jntuh_college_chairperson).State = EntityState.Modified;
                    resultMessage = "Update";
                }
                db.SaveChanges();

                jntuh_address jntuh_address = new jntuh_address();
                jntuh_address.collegeId = userCollegeID;
                jntuh_address.addressTye = chairPerson.addressTye;
                jntuh_address.address = chairPerson.address;
                jntuh_address.townOrCity = chairPerson.townOrCity;
                jntuh_address.mandal = chairPerson.mandal;
                jntuh_address.districtId = chairPerson.districtId;
                jntuh_address.stateId = chairPerson.stateId;
                jntuh_address.pincode = (chairPerson.pincode != null) ? (int)chairPerson.pincode : 0;
                jntuh_address.fax = chairPerson.fax;
                jntuh_address.landline = chairPerson.landline;
                jntuh_address.mobile = chairPerson.mobile;
                jntuh_address.email = chairPerson.email;
                jntuh_address.website = DBNull.Value.ToString();
                jntuh_address.createdBy = createdBy;
                jntuh_address.createdOn = DateTime.Now;
                var addressID = db.jntuh_address.Where(a => a.collegeId == userCollegeID && a.addressTye == chairPerson.addressTye).Select(a => a.id).FirstOrDefault();

                if (addressID == 0)      //if address id = 0; then insert the college address record
                {
                    db.jntuh_address.Add(jntuh_address);
                }
                else                        //if address id exists then modify the existing college address record
                {
                    jntuh_address.id = addressID;
                    jntuh_address.createdBy = db.jntuh_address.Where(a => a.id == addressID).Select(a => a.createdBy).FirstOrDefault();
                    jntuh_address.createdOn = db.jntuh_address.Where(a => a.id == addressID).Select(a => a.createdOn).FirstOrDefault();
                    jntuh_address.updatedBy = createdBy;
                    jntuh_address.updatedOn = DateTime.Now;
                    db.Entry(jntuh_address).State = EntityState.Modified;
                    resultMessage = "Update";
                }

                db.SaveChanges();

                //after postback
                ViewBag.State = db.jntuh_state.Where(s => s.isActive == true).ToList();
                ViewBag.District = db.jntuh_district.Where(s => s.isActive == true).ToList();
                ViewBag.Designation = db.jntuh_chairperson_designation.Where(s => s.isActive == true).ToList();
                if (resultMessage == "Update")
                {
                    TempData["Success"] = "Updated Successfully.";
                }
                else
                {
                    TempData["Success"] = "Added Successfully.";
                }
            }
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult GetDistrictList(string id)
        {
            if (id == string.Empty)
            {
                id = "0";
            }
            var districtList = this.GetDistricts(Convert.ToInt32(id));

            var myData = districtList.Select(a => new SelectListItem()
            {
                Text = a.districtName,
                Value = a.id.ToString(),
            });

            return Json(myData, JsonRequestBehavior.AllowGet);
        }

        private IList<jntuh_district> GetDistricts(int id)
        {
            return db.jntuh_district.Where(d => d.stateId == id).ToList();
        }

        // GET: /SocietyInformation/Edit/9
        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Edit(string collegeId)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
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
            int userAddressID = db.jntuh_address.Where(a => a.collegeId == userCollegeID && a.addressTye.Equals("SECRETARY")).Select(a => a.id).FirstOrDefault();
            int userChairPersonID = db.jntuh_college_chairperson.Where(e => e.collegeId == userCollegeID).Select(e => e.id).FirstOrDefault();
            if (userCollegeID == 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("Create", "CollegeInformation");
            }

            if (userChairPersonID == 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("Create", "ChairPerson");
            }
            if (userCollegeID == 0 && (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry")))
            {
                return RedirectToAction("Create", "CollegeInformation", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
            }

            if (userChairPersonID == 0 && (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry")))
            {
                return RedirectToAction("Create", "ChairPerson", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
            }
            DateTime todayDate = DateTime.Now.Date;
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy =
                db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            int status = db.jntuh_college_edit_status.Where(editStatus => editStatus.collegeId == userCollegeID &&
                                                                                      editStatus.IsCollegeEditable == true &&
                                                                                      editStatus.editFromDate <= todayDate &&
                                                                                      editStatus.editToDate >= todayDate)
                                                                 .Select(editStatus => editStatus.id)
                                                                 .FirstOrDefault();
            if (status == 0 && Roles.IsUserInRole("College"))
            {
                ViewBag.IsEditable = false;
                return RedirectToAction("View", "ChairPerson");
            }
            else
            {
                ViewBag.IsEditable = true;

                ////RAMESH:To-DisableEdit
                //if (ConfigurationManager.AppSettings["ShouldFormsBeDisabled"].ToString() == "true")
                //{
                //    ViewBag.IsEditable = false;
                //    return RedirectToAction("View", "ChairPerson");
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
                    return RedirectToAction("View", "ChairPerson");
                }
            }
            
            ChairPerson chairPerson = new ChairPerson();
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
                chairPerson.ChairpersionPhotoview = jntuh_college_chairperson.Photo;
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
                ViewBag.DistrictName = db.jntuh_district.Where(d => d.id == chairPerson.districtId && d.isActive==true).Select(d => d.districtName).FirstOrDefault();
                chairPerson.pincode = jntuh_address.pincode;
                chairPerson.fax = jntuh_address.fax;
                chairPerson.landline = jntuh_address.landline;
                chairPerson.mobile = jntuh_address.mobile;
                chairPerson.email = jntuh_address.email;
            }
            ViewBag.State = db.jntuh_state.Where(s => s.isActive == true).ToList();
            ViewBag.District = db.jntuh_district.Where(d => d.isActive == true).ToList();
            ViewBag.Designation = db.jntuh_chairperson_designation.Where(s => s.isActive == true).ToList();            
            return View("Create", chairPerson);
           
        }

        // POST: /SocietyInformation/Edit
        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        [HttpPost]
        public ActionResult Edit(ChairPerson chairPerson)
        {
            SaveInformation(chairPerson);
            var designation = db.jntuh_chairperson_designation.Where(d => d.id == chairPerson.designationId).Select(d => d.designationName).FirstOrDefault();
            if (designation == "Other Designation")
            {
                ViewBag.OtherDesignation = "OtherDesignation";
            }

            //int ChairPersionId = db.jntuh_college_users.Where(C => chairPerson.collegeId == chairPerson.collegeId).Select(c => c.id).FirstOrDefault();


            //jntuh_college_chairperson jntuh_college_chairperson = new jntuh_college_chairperson();
            //jntuh_college_chairperson.id = ChairPersionId;
            //jntuh_college_chairperson.collegeId = chairPerson.collegeId;
            //jntuh_college_chairperson.firstName = chairPerson.firstName;
            //jntuh_college_chairperson.lastName = chairPerson.lastName;
            //jntuh_college_chairperson.surname = chairPerson.surname;
            //jntuh_college_chairperson.designationId = chairPerson.designationId;
            //jntuh_college_chairperson.createdOn = chairPerson.createdOn;
            //jntuh_college_chairperson.createdBy = chairPerson.createdBy;
            //jntuh_college_chairperson.updatedOn = DateTime.Now;
            //jntuh_college_chairperson.updatedBy = chairPerson.UserID;
            //db.Entry(jntuh_college_chairperson).State = EntityState.Modified;
            //db.SaveChanges();
            ViewBag.State = db.jntuh_state.Where(s => s.isActive == true).ToList();
            ViewBag.District = db.jntuh_district.Where(d => d.isActive == true).ToList();
            ViewBag.Designation = db.jntuh_chairperson_designation.Where(s => s.isActive == true).ToList();
            return View("View");
        }

        [Authorize(Roles = "Admin,College")]
        public ActionResult View(string id)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                userCollegeID = userCollegeID = Convert.ToInt32(Utilities.DecryptString(id, WebConfigurationManager.AppSettings["CryptoKey"]));
            }
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            int userAddressID = db.jntuh_address.Where(a => a.collegeId == userCollegeID && a.addressTye.Equals("SECRETARY")).Select(a => a.id).FirstOrDefault();
            int userChairPersonID = db.jntuh_college_chairperson.Where(e => e.collegeId == userCollegeID).Select(e => e.id).FirstOrDefault();
            
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

            ChairPerson chairPerson = new ChairPerson();

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
                    chairPerson.ChairpersionPhotoview = jntuh_college_chairperson.Photo;
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
                    ViewBag.DistrictName = db.jntuh_district.Where(d => d.id == chairPerson.districtId && d.isActive==true).Select(d => d.districtName).FirstOrDefault();
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
            return View("View", chairPerson);
        }

        public ActionResult UserView(string id)
        {
            int userCollegeID = Convert.ToInt32(Utilities.DecryptString(id, WebConfigurationManager.AppSettings["CryptoKey"]));
            int userAddressID = db.jntuh_address.Where(a => a.collegeId == userCollegeID && a.addressTye.Equals("SECRETARY")).Select(a => a.id).FirstOrDefault();
            int userChairPersonID = db.jntuh_college_chairperson.Where(e => e.collegeId == userCollegeID).Select(e => e.id).FirstOrDefault();
                      
            ChairPerson chairPerson = new ChairPerson();
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
                ViewBag.DistrictName = db.jntuh_district.Where(d => d.id == chairPerson.districtId && d.isActive==true).Select(d => d.districtName).FirstOrDefault();
                chairPerson.pincode = jntuh_address.pincode;
                chairPerson.fax = jntuh_address.fax;
                chairPerson.landline = jntuh_address.landline;
                chairPerson.mobile = jntuh_address.mobile;
                chairPerson.email = jntuh_address.email;

                ViewBag.State = db.jntuh_state.Where(s => s.isActive == true).ToList();
                ViewBag.District = db.jntuh_district.Where(d => d.isActive == true).ToList();
                ViewBag.Designation = db.jntuh_chairperson_designation.Where(s => s.isActive == true).ToList();
            }
            if (jntuh_college_chairperson == null && jntuh_address == null)
            {
                ViewBag.NoRecords = true;
            }
            return View("UserView", chairPerson);
        }
    }
}
