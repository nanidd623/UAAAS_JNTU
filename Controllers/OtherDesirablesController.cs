using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Security;
using UAAAS.Models;

namespace UAAAS.Controllers
{
    [ErrorHandling]
    public class OtherDesirablesController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Index()
        {
            return View();
        }

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
            ViewBag.collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString();
            if (userCollegeID == 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("Create", "CollegeInformation");
            }
            if (userCollegeID == 0 && (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry")))
            {
                return RedirectToAction("Create", "CollegeInformation", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
            }
            int[] library = db.jntuh_desirable_requirement_type.Where(l => l.isActive == true && l.isHostelRequirement == false).Select(l => l.id).ToArray();
            int otherDetailsId = db.jntuh_college_desirable_requirement.Where(a => a.collegeId == userCollegeID && library.Contains(a.requirementTypeID)).Select(a => a.id).FirstOrDefault();

            DateTime todayDate = DateTime.Now.Date;
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            int status = db.jntuh_college_edit_status.Where(editStatus => editStatus.academicyearId == prAy && editStatus.collegeId == userCollegeID &&
                                                                                      editStatus.IsCollegeEditable == true &&
                                                                                      editStatus.editFromDate <= todayDate &&
                                                                                      editStatus.editToDate >= todayDate)
                                                                 .Select(editStatus => editStatus.id)
                                                                 .FirstOrDefault();

            if (userCollegeID > 0 && otherDetailsId > 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("View", "OtherDesirables");
            }
            if (userCollegeID > 0 && otherDetailsId > 0 && (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry")))
            {
                return RedirectToAction("Edit", "OtherDesirables", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
            }

            if (otherDetailsId == 0 && status == 0 && Roles.IsUserInRole("College"))
            {
                ViewBag.NotUpload = true;
            }
            else
            {
                ViewBag.NotUpload = false;
            }
            bool isPageEditable = db.jntuh_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("DR") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

            if (isPageEditable)
            {
                ViewBag.IsEditable = true;
            }
            else
            {
                ViewBag.IsEditable = false;
                return RedirectToAction("View", "OtherDesirables");
            }
            List<OtherDesirableRequirements> otherDesirableRequiremetns = db.jntuh_desirable_requirement_type.Where(d => d.isActive == true &&
                                                                                                                    d.isHostelRequirement == false)
                                                                          .Select(d => new OtherDesirableRequirements
                                                                          {
                                                                              id = d.id,
                                                                              requirementType = d.requirementType,
                                                                              collegeId=userCollegeID,
                                                                              governingBodymeetings=0
                                                                          }).ToList();
            ViewBag.Count = otherDesirableRequiremetns.Count();
            return View(otherDesirableRequiremetns);
           
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        [HttpPost]
        public ActionResult Create(ICollection<OtherDesirableRequirements> OtherDesirableRequirementTypes)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                foreach (var item in OtherDesirableRequirementTypes)
                {
                    userCollegeID = item.collegeId;
                }
            }
            SaveOtherDesirables(OtherDesirableRequirementTypes);
            List<OtherDesirableRequirements> otherDesirableRequiremetns = db.jntuh_desirable_requirement_type.Where(d => d.isActive == true &&
                                                                                                                    d.isHostelRequirement == false)
                                                                          .Select(d => new OtherDesirableRequirements
                                                                          {
                                                                              id = d.id,
                                                                              requirementType = d.requirementType,
                                                                              collegeId=userCollegeID
                                                                          }).ToList();
            ViewBag.Count = otherDesirableRequiremetns.Count();
            TempData["Success"] = "Added successfully";
            return View(otherDesirableRequiremetns);
        }

        private void SaveOtherDesirables(ICollection<OtherDesirableRequirements> OtherDesirableRequirementTypes)
        {
            int governingBodyMeetings = 0;
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                foreach (var item in OtherDesirableRequirementTypes)
                {
                    userCollegeID = item.collegeId;                    
                }
            }
            var Meassage = string.Empty;
            int rtypeid = db.jntuh_desirable_requirement_type.Where(rt => rt.isHostelRequirement == false && rt.requirementType == "No. of Governing Body meetings held in the past one academic year").Select(rt => rt.id).FirstOrDefault();
            if (ModelState.IsValid)
            {
                foreach (OtherDesirableRequirements item in OtherDesirableRequirementTypes)
                {
                    jntuh_college_desirable_requirement OtherDesirableRequirements = new jntuh_college_desirable_requirement();
                    OtherDesirableRequirements.collegeId = userCollegeID;
                    OtherDesirableRequirements.requirementTypeID = item.id;

                    if (item.id == rtypeid)
                    {
                        //if isselected=Oneormore than the value isSelected=false ,governingBodyMeetings=2
                        //if isselected=Nil than the value isSelected=true ,governingBodyMeetings=0
                        //if isselected=One than the value isSelected=false ,governingBodyMeetings=0
                        if (item.isSelected == "2")
                        {
                            governingBodyMeetings = Convert.ToInt32(item.isSelected);
                            OtherDesirableRequirements.isAvaiable = false;
                        }
                        else
                        {
                            OtherDesirableRequirements.isAvaiable = Convert.ToBoolean(item.isSelected);
                        }
                       
                    }
                    else
                    {
                        OtherDesirableRequirements.isAvaiable = Convert.ToBoolean(item.isSelected);
                    }
                    OtherDesirableRequirements.governingBodyMeetings = governingBodyMeetings;
                    int desirableID = db.jntuh_college_desirable_requirement.Where(a => a.collegeId == userCollegeID && a.requirementTypeID == item.id).Select(a => a.id).FirstOrDefault();
                    if (desirableID == 0)
                    {
                        OtherDesirableRequirements.createdBy = userID;
                        OtherDesirableRequirements.createdOn = DateTime.Now;                        
                        db.jntuh_college_desirable_requirement.Add(OtherDesirableRequirements);
                        db.SaveChanges();
                        Meassage = "Save";
                    }
                    else
                    {
                        OtherDesirableRequirements.id = desirableID;
                        OtherDesirableRequirements.createdOn = db.jntuh_college_desirable_requirement.Where(a => a.id == desirableID).Select(a => a.createdOn).FirstOrDefault();
                        OtherDesirableRequirements.createdBy = db.jntuh_college_desirable_requirement.Where(a => a.id == desirableID).Select(a => a.createdBy).FirstOrDefault();
                        OtherDesirableRequirements.updatedBy = userID;
                        OtherDesirableRequirements.updatedOn = DateTime.Now;
                        db.Entry(OtherDesirableRequirements).State = EntityState.Modified;
                        db.SaveChanges();
                        Meassage = "Update";
                    }
                }
            }

        }

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
            int[] library = db.jntuh_desirable_requirement_type.Where(l => l.isActive == true && l.isHostelRequirement == false).Select(l => l.id).ToArray();
            int otherDesirableId = db.jntuh_college_desirable_requirement.Where(a => a.collegeId == userCollegeID && library.Contains(a.requirementTypeID)).Select(a => a.id).FirstOrDefault();

            if (userCollegeID == 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("Create", "CollegeInformation");
            }

            if (otherDesirableId == 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("Create", "OtherDesirables");
            }
            DateTime todayDate = DateTime.Now.Date;
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            int status = db.jntuh_college_edit_status.Where(editStatus => editStatus.academicyearId == prAy && editStatus.collegeId == userCollegeID &&
                                                                                      editStatus.IsCollegeEditable == true &&
                                                                                      editStatus.editFromDate <= todayDate &&
                                                                                      editStatus.editToDate >= todayDate)
                                                                 .Select(editStatus => editStatus.id)
                                                                 .FirstOrDefault();
            if (status == 0 && Roles.IsUserInRole("College"))
            {
                ViewBag.IsEditable = false;
                return RedirectToAction("View", "OtherDesirables");
            }
            else
            {
                bool isPageEditable = db.jntuh_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("DR") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

                if (isPageEditable)
                {
                    ViewBag.IsEditable = true;
                }
                else
                {
                    ViewBag.IsEditable = false;
                    return RedirectToAction("View", "OtherDesirables");
                }
            }

            
            List<OtherDesirableRequirements> otherDesirableRequiremetns = (from r in db.jntuh_desirable_requirement_type.Where(r => r.isActive == true && r.isHostelRequirement == false)
                                                                           select new OtherDesirableRequirements
                                                                           {
                                                                               id = r.id,
                                                                               requirementType = r.requirementType,
                                                                               collegeId=userCollegeID                                                                               
                                                                           }).ToList();
            foreach (var item in otherDesirableRequiremetns)
            {
                var result = db.jntuh_college_desirable_requirement.Where(d => d.collegeId == userCollegeID && d.requirementTypeID == item.id)
                                                                        .Select(d => new
                                                                        {
                                                                            requirementTypeId = d.requirementTypeID,
                                                                            isAvailable = d.isAvaiable,
                                                                            governingBodymeetings = d.governingBodyMeetings
                                                                        }).FirstOrDefault();
                if (result != null)
                {
                    item.governingBodymeetings =(int)result.governingBodymeetings;
                    if (result.requirementTypeId == item.id && result.isAvailable == true)
                    {
                        item.isSelected = "true";
                    }
                    else
                    {
                        item.isSelected = "false";
                    }
                }
                else
                {
                    item.isSelected = null;
                }
            }

            ViewBag.Count = otherDesirableRequiremetns.Count();            
            return View("Create", otherDesirableRequiremetns);            
        }

        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Edit(ICollection<OtherDesirableRequirements> OtherDesirableRequirementTypes)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                foreach (var item in OtherDesirableRequirementTypes)
                {
                    userCollegeID = item.collegeId;
                }
            }
            SaveOtherDesirables(OtherDesirableRequirementTypes);
            TempData["Success"] = "Updated successfully";

            List<OtherDesirableRequirements> otherDesirableRequiremetns = db.jntuh_desirable_requirement_type.Where(d => d.isActive == true &&
                                                                                                                    d.isHostelRequirement == false)
                                                                          .Select(d => new OtherDesirableRequirements
                                                                          {
                                                                              id = d.id,
                                                                              requirementType = d.requirementType,
                                                                              collegeId=userCollegeID
                                                                          }).ToList();
            ViewBag.Count = otherDesirableRequiremetns.Count();

            return View("Create", otherDesirableRequiremetns);
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
            int[] library = db.jntuh_desirable_requirement_type.Where(l => l.isActive == true && l.isHostelRequirement == false).Select(l => l.id).ToArray();
            int otherDesirableId = db.jntuh_college_desirable_requirement.Where(a => a.collegeId == userCollegeID && library.Contains(a.requirementTypeID)).Select(a => a.id).FirstOrDefault();
                 
            DateTime todayDate = DateTime.Now.Date;
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            int status = db.jntuh_college_edit_status.Where(editStatus => editStatus.academicyearId == prAy && editStatus.collegeId == userCollegeID &&
                                                                                      editStatus.IsCollegeEditable == true &&
                                                                                      editStatus.editFromDate <= todayDate &&
                                                                                      editStatus.editToDate >= todayDate)
                                                                 .Select(editStatus => editStatus.id)
                                                                 .FirstOrDefault();
            if (status > 0 && Roles.IsUserInRole("College"))
            {
                bool isPageEditable = db.jntuh_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("DR") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

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
           
            List<OtherDesirableRequirements> otherDesirableRequiremetns = (from r in db.jntuh_desirable_requirement_type.Where(r => r.isActive == true && r.isHostelRequirement == false)
                                                                           select new OtherDesirableRequirements
                                                                           {
                                                                               id = r.id,
                                                                               requirementType = r.requirementType
                                                                           }).ToList();
            foreach (var item in otherDesirableRequiremetns)
            {
                var result = db.jntuh_college_desirable_requirement.Where(d => d.collegeId == userCollegeID && d.requirementTypeID == item.id)
                                                                        .Select(d => new
                                                                        {
                                                                            requirementTypeId = d.requirementTypeID,
                                                                            isAvailable = d.isAvaiable,
                                                                            governingBodymeetings=d.governingBodyMeetings
                                                                        })
                                                                            .FirstOrDefault();
                if (result != null)
                {
                    item.governingBodymeetings = (int)result.governingBodymeetings;
                    if (result.requirementTypeId == item.id && result.isAvailable == true)
                    {
                        item.isSelected = "True";
                    }
                    else
                    {
                        item.isSelected = "False";
                    }
                }
                else
                {
                    item.isSelected = "New";
                }
            }
            if (otherDesirableId == 0)
            {
                ViewBag.Norecords = true;
            }
            else
            {  

                ViewBag.Count = otherDesirableRequiremetns.Count();
            }
            return View("View", otherDesirableRequiremetns);
        }

        public ActionResult UserView(string id)
        {
            int userCollegeID = Convert.ToInt32(Utilities.DecryptString(id, WebConfigurationManager.AppSettings["CryptoKey"]));
            int[] library = db.jntuh_desirable_requirement_type.Where(l => l.isActive == true && l.isHostelRequirement == false).Select(l => l.id).ToArray();
            int otherDesirableId = db.jntuh_college_desirable_requirement.Where(a => a.collegeId == userCollegeID && library.Contains(a.requirementTypeID)).Select(a => a.id).FirstOrDefault();
            List<OtherDesirableRequirements> otherDesirableRequiremetns = (from r in db.jntuh_desirable_requirement_type.Where(r => r.isActive == true && r.isHostelRequirement == false)
                                                                           select new OtherDesirableRequirements
                                                                           {
                                                                               id = r.id,
                                                                               requirementType = r.requirementType
                                                                           }).ToList();
            foreach (var item in otherDesirableRequiremetns)
            {
                var result = db.jntuh_college_desirable_requirement.Where(d => d.collegeId == userCollegeID && d.requirementTypeID == item.id)
                                                                        .Select(d => new
                                                                        {
                                                                            requirementTypeId = d.requirementTypeID,
                                                                            isAvailable = d.isAvaiable,
                                                                            governingBodymeetings=d.governingBodyMeetings
                                                                        })
                                                                        .FirstOrDefault();
                if (result != null)
                {
                    item.governingBodymeetings = (int)result.governingBodymeetings;
                    if (result.requirementTypeId == item.id && result.isAvailable == true)
                    {
                        item.isSelected = "True";
                    }
                    else
                    {
                        item.isSelected = "False";
                    }
                }
                else
                {
                    item.isSelected = "New";
                }
            }
            if (otherDesirableId == 0)
            {
                ViewBag.Norecords = true;
            }
            else
            {

                ViewBag.Count = otherDesirableRequiremetns.Count();
            }
            return View("UserView", otherDesirableRequiremetns);
        }
    }
}
