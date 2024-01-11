using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Web.Security;
using UAAAS.Models;
using System.Web.Configuration;

namespace UAAAS.Controllers
{
    [ErrorHandling]
    public class CampusHostelController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();       

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
            int[] hostel = db.jntuh_desirable_requirement_type.Where(r => r.isActive == true && r.isHostelRequirement == true).Select(r => r.id).ToArray();
            int otherDetailsId = db.jntuh_college_hostel_maintenance.Where(a => a.collegeId == userCollegeID && hostel.Contains(a.requirementTypeID)).Select(a => a.id).FirstOrDefault();

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
                return RedirectToAction("View", "CampusHostel");
            }
            if (userCollegeID > 0 && otherDetailsId > 0 && (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry")))
            {
                return RedirectToAction("Edit", "CampusHostel", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
            }

            if (otherDetailsId == 0 && status == 0 && Roles.IsUserInRole("College"))
            {
                ViewBag.NotUpload = true;
            }
            else
            {
                ViewBag.NotUpload = false;
            }
            bool isPageEditable = db.jntuh_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("CH") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

            if (isPageEditable)
            {
                ViewBag.IsEditable = true;
            }
            else
            {
                ViewBag.IsEditable = false;
                return RedirectToAction("View", "CampusHostel");
            }
            List<HostelRequirements> hostelRequirements = db.jntuh_desirable_requirement_type.Where(d => d.isActive == true &&
                                                                                                                    d.isHostelRequirement == true)
                                                                          .Select(d => new HostelRequirements
                                                                          {
                                                                              id = d.id,
                                                                              requirementType = d.requirementType,
                                                                              collegeId=userCollegeID
                                                                          }).ToList();
            ViewBag.Count = hostelRequirements.Count();
            return View(hostelRequirements);
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        [HttpPost]
        public ActionResult Create(ICollection<HostelRequirements> hostelRequirements)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                foreach (var item in hostelRequirements)
                {
                    userCollegeID = item.collegeId;
                }
            }
            SaveHostelRequirements(hostelRequirements);
            List<HostelRequirements> hostelRequirementTypes = db.jntuh_desirable_requirement_type.Where(d => d.isActive == true &&
                                                                                                                    d.isHostelRequirement == true)
                                                                          .Select(d => new HostelRequirements
                                                                          {
                                                                              id = d.id,
                                                                              requirementType = d.requirementType,
                                                                              collegeId=userCollegeID
                                                                          }).ToList();
            ViewBag.Count = hostelRequirementTypes.Count();
            TempData["Success"] = "Added successfully";
            return View(hostelRequirementTypes);
        }

        private void SaveHostelRequirements(ICollection<HostelRequirements> hostelRequirements)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                foreach (var item in hostelRequirements)
                {
                    userCollegeID = item.collegeId;
                }
            }
            if (ModelState.IsValid)
            {
                foreach (HostelRequirements item in hostelRequirements)
                {
                    jntuh_college_hostel_maintenance jntuh_college_hostel_maintenance = new jntuh_college_hostel_maintenance();
                    jntuh_college_hostel_maintenance.collegeId = userCollegeID;
                    jntuh_college_hostel_maintenance.requirementTypeID = item.id;
                    jntuh_college_hostel_maintenance.isAvaiable = Convert.ToBoolean(item.isSelected);

                    int hostelRequirementID = db.jntuh_college_hostel_maintenance.Where(a => a.collegeId == userCollegeID && a.requirementTypeID == item.id).Select(a => a.id).FirstOrDefault();
                    if (hostelRequirementID == 0)
                    {
                        jntuh_college_hostel_maintenance.createdBy = userID;
                        jntuh_college_hostel_maintenance.createdOn = DateTime.Now;
                        db.jntuh_college_hostel_maintenance.Add(jntuh_college_hostel_maintenance);
                        db.SaveChanges();
                    }
                    else
                    {
                        jntuh_college_hostel_maintenance.id = hostelRequirementID;
                        jntuh_college_hostel_maintenance.createdOn = db.jntuh_college_hostel_maintenance.Where(a => a.id == hostelRequirementID).Select(a => a.createdOn).FirstOrDefault();
                        jntuh_college_hostel_maintenance.createdBy = db.jntuh_college_hostel_maintenance.Where(a => a.id == hostelRequirementID).Select(a => a.createdBy).FirstOrDefault();
                        jntuh_college_hostel_maintenance.updatedBy = userID;
                        jntuh_college_hostel_maintenance.updatedOn = DateTime.Now;
                        db.Entry(jntuh_college_hostel_maintenance).State = EntityState.Modified;
                        db.SaveChanges();
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
            int[] hostel = db.jntuh_desirable_requirement_type.Where(h => h.isActive == true && h.isHostelRequirement == true).Select(h => h.id).ToArray();
            int HostelRequirementDesirableId = db.jntuh_college_hostel_maintenance.Where(a => a.collegeId == userCollegeID && hostel.Contains(a.requirementTypeID)).Select(a => a.id).FirstOrDefault();
            if (userCollegeID == 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("Create", "CollegeInformation");
            }

            if (HostelRequirementDesirableId == 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("Create", "CampusHostel");
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
                return RedirectToAction("View", "CampusHostel");
            }
            else
            {
                bool isPageEditable = db.jntuh_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("CH") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

                if (isPageEditable)
                {
                    ViewBag.IsEditable = true;
                }
                else
                {
                    ViewBag.IsEditable = false;
                    return RedirectToAction("View", "CampusHostel");
                }
            }

            List<HostelRequirements> hostelRequiremetns = (from d in db.jntuh_college_hostel_maintenance
                                                                           join r in db.jntuh_desirable_requirement_type on d.requirementTypeID equals r.id
                                                                           where (r.isActive == true && d.collegeId == userCollegeID && r.isHostelRequirement == true)

                                                                   select new HostelRequirements
                                                                           {
                                                                               id = d.requirementTypeID,
                                                                               requirementType = r.requirementType,
                                                                               isSelected = d.isAvaiable == true ? "true" : "false" ,
                                                                               collegeId=userCollegeID
                                                                           }).ToList();

            ViewBag.Count = hostelRequiremetns.Count();
            return View("Create", hostelRequiremetns);            
        }

        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Edit(ICollection<HostelRequirements> hostelRequiremetns)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                foreach (var item in hostelRequiremetns)
                {
                    userCollegeID = item.collegeId;
                }
            }
            SaveHostelRequirements(hostelRequiremetns);
            TempData["Success"] = "Updated successfully";

            List<HostelRequirements> hostelRequiremetnsType = db.jntuh_desirable_requirement_type.Where(d => d.isActive == true &&
                                                                                                                    d.isHostelRequirement == true)
                                                                          .Select(d => new HostelRequirements
                                                                          {
                                                                              id = d.id,
                                                                              requirementType = d.requirementType,
                                                                              collegeId=userCollegeID
                                                                          }).ToList();
            ViewBag.Count = hostelRequiremetnsType.Count();

            return View("Create", hostelRequiremetnsType);
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
            int[] hostel = db.jntuh_desirable_requirement_type.Where(h => h.isActive == true && h.isHostelRequirement == true).Select(h => h.id).ToArray();
            int HostelRequirementDesirableId = db.jntuh_college_hostel_maintenance.Where(a => a.collegeId == userCollegeID && hostel.Contains(a.requirementTypeID)).Select(a => a.id).FirstOrDefault();
            int[] area = db.jntuh_area_requirement.Where(r => r.isActive == true && r.areaType == "ADMINISTRATIVE").Select(r => r.id).ToArray();
            int collegeAreaId = db.jntuh_college_area.Where(a => a.collegeId == userCollegeID && area.Contains(a.areaRequirementId)).Select(a => a.id).FirstOrDefault();

           
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
                bool isPageEditable = db.jntuh_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("CH") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

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

            List<HostelRequirements> hostelRequiremetns = (from d in db.jntuh_college_hostel_maintenance
                                                           join r in db.jntuh_desirable_requirement_type on d.requirementTypeID equals r.id
                                                           where (r.isActive == true && d.collegeId == userCollegeID && r.isHostelRequirement == true)

                                                           select new HostelRequirements
                                                           {
                                                               id = d.requirementTypeID,
                                                               requirementType = r.requirementType,
                                                               isSelected = d.isAvaiable == true ? "true" : "false"
                                                           }).ToList();
            if (HostelRequirementDesirableId == 0)
            {
                ViewBag.Norecords = true;
            }
            else
            {              

                ViewBag.Count = hostelRequiremetns.Count();
            }
            return View("View", hostelRequiremetns);
        }

        public ActionResult UserView(string id)
        {
            int userCollegeID = Convert.ToInt32(Utilities.DecryptString(id, WebConfigurationManager.AppSettings["CryptoKey"]));
            int[] hostel = db.jntuh_desirable_requirement_type.Where(h => h.isActive == true && h.isHostelRequirement == true).Select(h => h.id).ToArray();
            int HostelRequirementDesirableId = db.jntuh_college_hostel_maintenance.Where(a => a.collegeId == userCollegeID && hostel.Contains(a.requirementTypeID)).Select(a => a.id).FirstOrDefault();
            int[] area = db.jntuh_area_requirement.Where(r => r.isActive == true && r.areaType == "ADMINISTRATIVE").Select(r => r.id).ToArray();
            int collegeAreaId = db.jntuh_college_area.Where(a => a.collegeId == userCollegeID && area.Contains(a.areaRequirementId)).Select(a => a.id).FirstOrDefault();
            List<HostelRequirements> hostelRequiremetns = (from d in db.jntuh_college_hostel_maintenance
                                                           join r in db.jntuh_desirable_requirement_type on d.requirementTypeID equals r.id
                                                           where (r.isActive == true && d.collegeId == userCollegeID && r.isHostelRequirement == true)

                                                           select new HostelRequirements
                                                           {
                                                               id = d.requirementTypeID,
                                                               requirementType = r.requirementType,
                                                               isSelected = d.isAvaiable == true ? "true" : "false"
                                                           }).ToList();
            if (HostelRequirementDesirableId == 0)
            {
                ViewBag.Norecords = true;
            }
            else
            {
                ViewBag.Count = hostelRequiremetns.Count();
            }
            return View("UserView", hostelRequiremetns);
        }
    }
}
