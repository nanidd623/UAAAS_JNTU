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
    public class CollegeExaminationBranchController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Create(string collegeId)
        {
            CollegeExaminationBranch collegeExaminationBranch = new CollegeExaminationBranch();

            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(collegeUsers => collegeUsers.userID == userID).Select(collegeUsers => collegeUsers.collegeID).FirstOrDefault();
            if (userCollegeID == 0 && collegeId!=null)
            {
                if (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry"))
                {
                    userCollegeID = Convert.ToInt32(Utilities.DecryptString(collegeId, WebConfigurationManager.AppSettings["CryptoKey"]));
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

            int existsExaminationBranchId = db.jntuh_college_examination_branch.Where(examinationBranch => examinationBranch.collegeId == userCollegeID)
                                                                               .Select(examinationBranch => examinationBranch.id).FirstOrDefault();

            int existsExaminationBranchSecurityId = db.jntuh_college_examination_branch_security.Where(examinationBranchSecurity => examinationBranchSecurity.collegeId == userCollegeID)
                                                                                     .Select(examinationBranchSecurity => examinationBranchSecurity.id).FirstOrDefault();


            DateTime todayDate = DateTime.Now.Date;
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            int status = db.jntuh_college_edit_status.Where(editStatus => editStatus.academicyearId == prAy && editStatus.collegeId == userCollegeID &&
                                                                                      editStatus.IsCollegeEditable == true &&
                                                                                      editStatus.editFromDate <= todayDate &&
                                                                                      editStatus.editToDate >= todayDate)
                                                                 .Select(editStatus => editStatus.id)
                                                                 .FirstOrDefault();
            if (userCollegeID > 0 && existsExaminationBranchId > 0 && existsExaminationBranchSecurityId > 0 && status == 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("View", "CollegeExaminationBranch");
            }
            if (userCollegeID > 0 && existsExaminationBranchId > 0 && existsExaminationBranchSecurityId > 0 && status > 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("Edit", "CollegeExaminationBranch");
            }

            if (userCollegeID > 0 && existsExaminationBranchId > 0 && existsExaminationBranchSecurityId > 0  &&  (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry")))
            {
                return RedirectToAction("Edit", "CollegeExaminationBranch", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
            }
            if (existsExaminationBranchId == 0 && existsExaminationBranchSecurityId == 0 && status == 0 && Roles.IsUserInRole("College"))
            {
                ViewBag.NotUpload = true;
            }
            else
            {
                ViewBag.NotUpload = false;
            }  

            var list = db.jntuh_college_examination_branch_staff.Where(staffMembers => staffMembers.collegeId == userCollegeID).ToList();
            if (list.Count() == 0 && status == 0 && Roles.IsUserInRole("College"))
            {
                ViewBag.StaffNotUpload = true;
            }
            else
            {
                ViewBag.StaffNotUpload = false;
            }

            bool isPageEditable = db.jntuh_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("EB") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

            if (isPageEditable)
            {
                ViewBag.IsEditable = true;
            }
            else
            {
                ViewBag.IsEditable = false;
                return RedirectToAction("View", "CollegeExaminationBranch");
            }

            ViewBag.StaffMembers = list;
            ViewBag.Count = list.Count();
            collegeExaminationBranch.collegeId = userCollegeID;
            return View(collegeExaminationBranch);
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        [HttpPost]
        public ActionResult Create(CollegeExaminationBranch collegeExaminationBranch)
        {

            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(collegeUsers => collegeUsers.userID == userID).Select(collegeUsers => collegeUsers.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                userCollegeID = collegeExaminationBranch.collegeId;
            }
            SaveExaminationBranchDetails(collegeExaminationBranch);

            ViewBag.StaffMembers = db.jntuh_college_examination_branch_staff.Where(staffMembers => staffMembers.collegeId == userCollegeID).ToList();
            return View(collegeExaminationBranch);
        }

        private void SaveExaminationBranchDetails(CollegeExaminationBranch collegeExaminationBranch)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(collegeUsers => collegeUsers.userID == userID).Select(collegeUsers => collegeUsers.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                userCollegeID = collegeExaminationBranch.collegeId;
            }
            var resultMessage = string.Empty;
            if (ModelState.IsValid)
            {
                jntuh_college_examination_branch examinationBranchDetails = new jntuh_college_examination_branch();
                examinationBranchDetails.collegeId = userCollegeID;
                examinationBranchDetails.examinationBranchArea = collegeExaminationBranch.examinationBranchArea;
                examinationBranchDetails.isConfidenatialRoomExists = collegeExaminationBranch.isConfidenatialRoomExists;
                examinationBranchDetails.isAdjacentPrincipalRoom = collegeExaminationBranch.isAdjacentPrincipalRoom;

                int existsExaminationBranchId = db.jntuh_college_examination_branch.Where(examinationBranch => examinationBranch.collegeId == userCollegeID)
                                                                                   .Select(examinationBranch => examinationBranch.id).FirstOrDefault();

                if (existsExaminationBranchId == 0)
                {
                    examinationBranchDetails.createdBy = userID;
                    examinationBranchDetails.createdOn = DateTime.Now;
                    db.jntuh_college_examination_branch.Add(examinationBranchDetails);
                    db.SaveChanges();
                    resultMessage = "Save";
                }
                else
                {
                    examinationBranchDetails.id = existsExaminationBranchId;
                    examinationBranchDetails.createdOn = db.jntuh_college_examination_branch.Where(examinationBranch => examinationBranch.id == existsExaminationBranchId)
                                                                                            .Select(examinationBranch => examinationBranch.createdOn)
                                                                                            .FirstOrDefault();
                    examinationBranchDetails.createdBy = db.jntuh_college_examination_branch.Where(examinationBranch => examinationBranch.id == existsExaminationBranchId)
                                                                                            .Select(examinationBranch => examinationBranch.createdBy)
                                                                                            .FirstOrDefault();
                    examinationBranchDetails.updatedBy = userID;
                    examinationBranchDetails.updatedOn = DateTime.Now;
                    db.Entry(examinationBranchDetails).State = EntityState.Modified;
                    db.SaveChanges();
                    resultMessage = "Update";
                }

                int existId = examinationBranchDetails.id;
                if (existId != 0)
                {
                    jntuh_college_examination_branch_security examinationBranchSecurityDetails = new jntuh_college_examination_branch_security();
                    examinationBranchSecurityDetails.collegeId = userCollegeID;
                    examinationBranchSecurityDetails.securityMesearesTaken1 = collegeExaminationBranch.securityMesearesTaken1;
                    examinationBranchSecurityDetails.securityMesearesTaken2 = collegeExaminationBranch.securityMesearesTaken2;
                    examinationBranchSecurityDetails.securityMesearesTaken3 = collegeExaminationBranch.securityMesearesTaken3;

                    int existsExaminationBranchSecurityId = db.jntuh_college_examination_branch_security.Where(examinationBranchSecurity => examinationBranchSecurity.collegeId == userCollegeID)
                                                                               .Select(examinationBranchSecurity => examinationBranchSecurity.id).FirstOrDefault();

                    if (existsExaminationBranchSecurityId == 0)
                    {
                        examinationBranchSecurityDetails.createdBy = userID;
                        examinationBranchSecurityDetails.createdOn = DateTime.Now;
                        db.jntuh_college_examination_branch_security.Add(examinationBranchSecurityDetails);
                        db.SaveChanges();
                        resultMessage = "Save";
                    }
                    else
                    {
                        examinationBranchSecurityDetails.id = existsExaminationBranchSecurityId;
                        examinationBranchSecurityDetails.createdOn = db.jntuh_college_examination_branch_security.Where(examinationBranchSecurity => examinationBranchSecurity.id == existsExaminationBranchSecurityId)
                                                                                            .Select(examinationBranchSecurity => examinationBranchSecurity.createdOn)
                                                                                            .FirstOrDefault();
                        examinationBranchSecurityDetails.createdBy = db.jntuh_college_examination_branch_security.Where(examinationBranchSecurity => examinationBranchSecurity.id == existsExaminationBranchSecurityId)
                                                                                                .Select(examinationBranchSecurity => examinationBranchSecurity.createdBy)
                                                                                                .FirstOrDefault();
                        examinationBranchSecurityDetails.updatedBy = userID;
                        examinationBranchSecurityDetails.updatedOn = DateTime.Now;
                        db.Entry(examinationBranchSecurityDetails).State = EntityState.Modified;
                        db.SaveChanges();
                        resultMessage = "Update";
                    }
                }

                if (resultMessage == "Update")
                {
                    TempData["Success"] = "College Examination Branch Details are Updated successfully";
                }
                else
                {
                    TempData["Success"] = "College Examination Branch Details are Added successfully";
                }
            }
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Edit(string collegeId)
        {
            CollegeExaminationBranch collegeExaminationBranch = new CollegeExaminationBranch();

            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(collegeUsers => collegeUsers.userID == userID).Select(collegeUsers => collegeUsers.collegeID).FirstOrDefault();

            if (userCollegeID == 0 && collegeId != null)
            {
                if (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry"))
                {
                    userCollegeID = Convert.ToInt32(Utilities.DecryptString(collegeId, WebConfigurationManager.AppSettings["CryptoKey"]));
                }
            }
            ViewBag.collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString();
            if (userCollegeID == 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("Create", "CollegeInformation");
            }

            int existsExaminationBranchId = db.jntuh_college_examination_branch.Where(examinationBranch => examinationBranch.collegeId == userCollegeID)
                                                                               .Select(examinationBranch => examinationBranch.id).FirstOrDefault();

            int existsExaminationBranchSecurityId = db.jntuh_college_examination_branch_security.Where(examinationBranchSecurity => examinationBranchSecurity.collegeId == userCollegeID)
                                                                               .Select(examinationBranchSecurity => examinationBranchSecurity.id).FirstOrDefault();

            if (existsExaminationBranchId == 0 && existsExaminationBranchSecurityId == 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("Create", "CollegeExaminationBranch");
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
                return RedirectToAction("View", "CollegeExaminationBranch");
            }
            else
            {
                bool isPageEditable = db.jntuh_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("EB") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

                if (isPageEditable)
                {
                    ViewBag.IsEditable = true;
                }
                else
                {
                    ViewBag.IsEditable = false;
                    return RedirectToAction("View", "CollegeExaminationBranch");
                }
            }

            jntuh_college_examination_branch examinationBranchDetails = db.jntuh_college_examination_branch.Find(existsExaminationBranchId);

            if (examinationBranchDetails != null)
            {
                collegeExaminationBranch.collegeId = examinationBranchDetails.collegeId;
                collegeExaminationBranch.examinationBranchArea = examinationBranchDetails.examinationBranchArea;
                collegeExaminationBranch.isConfidenatialRoomExists = examinationBranchDetails.isConfidenatialRoomExists;
                collegeExaminationBranch.isAdjacentPrincipalRoom = examinationBranchDetails.isAdjacentPrincipalRoom;
                collegeExaminationBranch.createdOn = examinationBranchDetails.createdOn;
                collegeExaminationBranch.createdBy = examinationBranchDetails.createdBy;
            }

            jntuh_college_examination_branch_security examinationBranchSecurityDetails = db.jntuh_college_examination_branch_security.Find(existsExaminationBranchSecurityId);

            if (examinationBranchSecurityDetails != null)
            {
                collegeExaminationBranch.collegeId = examinationBranchSecurityDetails.collegeId;
                collegeExaminationBranch.securityMesearesTaken1 = examinationBranchSecurityDetails.securityMesearesTaken1;
                collegeExaminationBranch.securityMesearesTaken2 = examinationBranchSecurityDetails.securityMesearesTaken2;
                collegeExaminationBranch.securityMesearesTaken3 = examinationBranchSecurityDetails.securityMesearesTaken3;
                collegeExaminationBranch.createdOn = examinationBranchSecurityDetails.createdOn;
                collegeExaminationBranch.createdBy = examinationBranchSecurityDetails.createdBy;
            }
            collegeExaminationBranch.collegeId = userCollegeID;
            ViewBag.StaffMembers = db.jntuh_college_examination_branch_staff.Where(staffMembers => staffMembers.collegeId == userCollegeID).ToList();
            return View("Create", collegeExaminationBranch);            
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        [HttpPost]
        public ActionResult Edit(CollegeExaminationBranch collegeExaminationBranch)
        {

            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(collegeUsers => collegeUsers.userID == userID).Select(collegeUsers => collegeUsers.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                userCollegeID = collegeExaminationBranch.collegeId;
            }
            SaveExaminationBranchDetails(collegeExaminationBranch);

            ViewBag.StaffMembers = db.jntuh_college_examination_branch_staff.Where(staffMembers => staffMembers.collegeId == userCollegeID).ToList();
            return View("Create", collegeExaminationBranch);
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        [HttpGet]
        public ActionResult AddOrEditStaffMembers(int? id,string collegeId)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(collegeUsers => collegeUsers.userID == userID).Select(collegeUsers => collegeUsers.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                if (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry"))
                {
                    if (collegeId != null)
                    {
                        userCollegeID = Convert.ToInt32(Utilities.DecryptString(collegeId, WebConfigurationManager.AppSettings["CryptoKey"]));
                    }
                    else
                    {
                        userCollegeID = db.jntuh_college_examination_branch_staff.Where(e => e.id == id).Select(e => e.collegeId).FirstOrDefault();
                    }
                }
            }
            if (Request.IsAjaxRequest())
            {
                if (id != null)
                {
                    ViewBag.IsUpdate = true;
                    CollegeExaminationBranchstaffDetails staffDetails = new CollegeExaminationBranchstaffDetails();

                    jntuh_college_examination_branch_staff ExaminationBranchstaffDetails = db.jntuh_college_examination_branch_staff.Where(staff => staff.collegeId == userCollegeID &&
                                                                                                             staff.id == id).FirstOrDefault();

                    staffDetails.id = ExaminationBranchstaffDetails.id;
                    staffDetails.collegeId = ExaminationBranchstaffDetails.collegeId;
                    staffDetails.staffName = ExaminationBranchstaffDetails.staffName;
                    staffDetails.staffDesignation = ExaminationBranchstaffDetails.staffDesignation;
                    staffDetails.isTeachingStaff = ExaminationBranchstaffDetails.isTeachingStaff;
                    return PartialView("_CreateStaff", staffDetails);

                }
                else
                {

                    CollegeExaminationBranchstaffDetails staffDetails = new CollegeExaminationBranchstaffDetails();
                    staffDetails.collegeId = userCollegeID;
                    ViewBag.IsUpdate = false;
                    return PartialView("_CreateStaff",staffDetails);
                }
            }
            else
            {
                if (id != null)
                {
                    ViewBag.IsUpdate = true;
                    CollegeExaminationBranchstaffDetails staffDetails = new CollegeExaminationBranchstaffDetails();

                    jntuh_college_examination_branch_staff ExaminationBranchstaffDetails = db.jntuh_college_examination_branch_staff.Where(staff => staff.collegeId == userCollegeID &&
                                                                                                             staff.id == id).FirstOrDefault();

                    staffDetails.id = ExaminationBranchstaffDetails.id;
                    staffDetails.collegeId = ExaminationBranchstaffDetails.collegeId;
                    staffDetails.staffName = ExaminationBranchstaffDetails.staffName;
                    staffDetails.staffDesignation = ExaminationBranchstaffDetails.staffDesignation;
                    staffDetails.isTeachingStaff = ExaminationBranchstaffDetails.isTeachingStaff;
                    return View("CreateStaff", staffDetails);
                }
                else
                {

                    CollegeExaminationBranchstaffDetails staffDetails = new CollegeExaminationBranchstaffDetails();
                    staffDetails.collegeId = userCollegeID;
                    ViewBag.IsUpdate = false;
                    return View("CreateStaff", staffDetails);
                }
            }
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        [HttpPost]
        public ActionResult AddOrEditStaffMembers(CollegeExaminationBranchstaffDetails examinationBranchstaffDetails, string cmd)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(collegeUsers => collegeUsers.userID == userID).Select(collegeUsers => collegeUsers.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                userCollegeID = examinationBranchstaffDetails.collegeId;
            }
            if (ModelState.IsValid)
            {
                if (cmd == "Add")
                {
                    jntuh_college_examination_branch_staff staffDetails = new jntuh_college_examination_branch_staff();
                    staffDetails.id = examinationBranchstaffDetails.id;
                    staffDetails.collegeId = userCollegeID;
                    staffDetails.staffName = examinationBranchstaffDetails.staffName;
                    staffDetails.staffDesignation = examinationBranchstaffDetails.staffDesignation;
                    staffDetails.isTeachingStaff = examinationBranchstaffDetails.isTeachingStaff;
                    staffDetails.createdOn = DateTime.Now;
                    staffDetails.createdBy = userID;
                    db.jntuh_college_examination_branch_staff.Add(staffDetails);
                    db.SaveChanges();
                    TempData["StaffDetailsSuccess"] = "Staff Details are Added successfully.";

                    return RedirectToAction("Create", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
                }
                else
                {
                    jntuh_college_examination_branch_staff staffDetails = new jntuh_college_examination_branch_staff();
                    staffDetails.id = examinationBranchstaffDetails.id;
                    staffDetails.collegeId = userCollegeID;
                    staffDetails.staffName = examinationBranchstaffDetails.staffName;
                    staffDetails.staffDesignation = examinationBranchstaffDetails.staffDesignation;
                    staffDetails.isTeachingStaff = examinationBranchstaffDetails.isTeachingStaff;
                    staffDetails.createdOn = db.jntuh_college_examination_branch_staff.Where(staff => staff.id == examinationBranchstaffDetails.id)
                                                                                      .Select(staff => staff.createdOn).FirstOrDefault();
                    staffDetails.createdBy = db.jntuh_college_examination_branch_staff.Where(staff => staff.id == examinationBranchstaffDetails.id)
                                                                                      .Select(staff => staff.createdBy).FirstOrDefault();
                    staffDetails.updatedOn = DateTime.Now;
                    staffDetails.updatedBy = userID;
                    db.Entry(staffDetails).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["StaffDetailsSuccess"] = "Staff Details are Updated successfully.";

                    return RedirectToAction("Create", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
                }
            }
            else
            {
                return RedirectToAction("Create", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
            }
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult DeleteStaffMembers(int id)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(collegeUsers => collegeUsers.userID == userID).Select(collegeUsers => collegeUsers.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                userCollegeID = db.jntuh_college_examination_branch_staff.Where(e => e.id == id).Select(e => e.collegeId).FirstOrDefault();
            }
            jntuh_college_examination_branch_staff staffDetails = db.jntuh_college_examination_branch_staff.Where(staff => staff.id == id).FirstOrDefault();
            if (staffDetails != null)
            {
                db.jntuh_college_examination_branch_staff.Remove(staffDetails);
                db.SaveChanges();
                TempData["StaffDetailsSuccess"] = "Staff Details are Deleted successfully.";
            }

            return RedirectToAction("Create", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult StaffMembersDetails(int id)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(collegeUsers => collegeUsers.userID == userID).Select(collegeUsers => collegeUsers.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                userCollegeID = db.jntuh_college_examination_branch_staff.Where(e => e.id == id).Select(e => e.collegeId).FirstOrDefault();
            }
            CollegeExaminationBranchstaffDetails staffDetails = db.jntuh_college_examination_branch_staff.Where(staff => staff.id == id)
                                                                                                         .Select(staff => new CollegeExaminationBranchstaffDetails
                                                                                                        {
                                                                                                            id = staff.id,
                                                                                                            collegeId = staff.collegeId,
                                                                                                            staffName = staff.staffName,
                                                                                                            staffDesignation = staff.staffDesignation,
                                                                                                            isTeachingStaff = staff.isTeachingStaff
                                                                                                        }).FirstOrDefault();
                

            if (staffDetails != null)
            {
                if (Request.IsAjaxRequest())
                {
                    return PartialView("_StaffDetails", staffDetails);
                }
                else
                {
                    return View("StaffDetails", staffDetails);
                }
            }
            return RedirectToAction("Create", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });

        }

        [Authorize(Roles = "Admin,College")]
        public ActionResult View(string id)
        {
            CollegeExaminationBranch collegeExaminationBranch = new CollegeExaminationBranch();

            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(collegeUsers => collegeUsers.userID == userID).Select(collegeUsers => collegeUsers.collegeID).FirstOrDefault();

            if (userCollegeID == 0)
            {
                userCollegeID = userCollegeID = Convert.ToInt32(Utilities.DecryptString(id, WebConfigurationManager.AppSettings["CryptoKey"]));          
            }

            int existsExaminationBranchId = db.jntuh_college_examination_branch.Where(examinationBranch => examinationBranch.collegeId == userCollegeID)
                                                                               .Select(examinationBranch => examinationBranch.id).FirstOrDefault();

            int existsExaminationBranchSecurityId = db.jntuh_college_examination_branch_security.Where(examinationBranchSecurity => examinationBranchSecurity.collegeId == userCollegeID)
                                                                               .Select(examinationBranchSecurity => examinationBranchSecurity.id).FirstOrDefault();

            DateTime todayDate = DateTime.Now.Date;
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            int status = db.jntuh_college_edit_status.Where(editStatus => editStatus.academicyearId == prAy && editStatus.collegeId == userCollegeID &&
                                                                                      editStatus.IsCollegeEditable == true &&
                                                                                      editStatus.editFromDate <= todayDate &&
                                                                                      editStatus.editToDate >= todayDate)
                                                                 .Select(editStatus => editStatus.id)
                                                                 .FirstOrDefault();
            if (status > 0)
            {
                ViewBag.Status = true;
                bool isPageEditable = db.jntuh_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("EB") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

                if (isPageEditable)
                {
                    ViewBag.Status = true;
                }
                else
                {
                    ViewBag.Status = false;                    
                }
            }
            else
            {
                ViewBag.Status = false;
            }
            if (status > 0 && Roles.IsUserInRole("College"))
            {
                bool isPageEditable = db.jntuh_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("EB") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

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

            if (existsExaminationBranchId == 0 && existsExaminationBranchSecurityId == 0)
            {
                ViewBag.Norecords = true;
            }
            else
            {
                jntuh_college_examination_branch examinationBranchDetails = db.jntuh_college_examination_branch.Find(existsExaminationBranchId);

                if (examinationBranchDetails != null)
                {
                    collegeExaminationBranch.collegeId = examinationBranchDetails.collegeId;
                    collegeExaminationBranch.examinationBranchArea = examinationBranchDetails.examinationBranchArea;
                    collegeExaminationBranch.isConfidenatialRoomExists = examinationBranchDetails.isConfidenatialRoomExists;
                    collegeExaminationBranch.isAdjacentPrincipalRoom = examinationBranchDetails.isAdjacentPrincipalRoom;
                    collegeExaminationBranch.createdOn = examinationBranchDetails.createdOn;
                    collegeExaminationBranch.createdBy = examinationBranchDetails.createdBy;
                }

                jntuh_college_examination_branch_security examinationBranchSecurityDetails = db.jntuh_college_examination_branch_security.Find(existsExaminationBranchSecurityId);

                if (examinationBranchSecurityDetails != null)
                {
                    collegeExaminationBranch.collegeId = examinationBranchSecurityDetails.collegeId;
                    collegeExaminationBranch.securityMesearesTaken1 = examinationBranchSecurityDetails.securityMesearesTaken1;
                    collegeExaminationBranch.securityMesearesTaken2 = examinationBranchSecurityDetails.securityMesearesTaken2;
                    collegeExaminationBranch.securityMesearesTaken3 = examinationBranchSecurityDetails.securityMesearesTaken3;
                    collegeExaminationBranch.createdOn = examinationBranchSecurityDetails.createdOn;
                    collegeExaminationBranch.createdBy = examinationBranchSecurityDetails.createdBy;
                }
            }
            ViewBag.StaffMembers = db.jntuh_college_examination_branch_staff.Where(staffMembers => staffMembers.collegeId == userCollegeID).ToList();
            ViewBag.Count = db.jntuh_college_examination_branch_staff.Where(staffMembers => staffMembers.collegeId == userCollegeID).ToList().Count();
            return View("View", collegeExaminationBranch);
        }

        public ActionResult UserView(string id)
        {
            CollegeExaminationBranch collegeExaminationBranch = new CollegeExaminationBranch();
            int userCollegeID = Convert.ToInt32(Utilities.DecryptString(id, WebConfigurationManager.AppSettings["CryptoKey"]));
            int existsExaminationBranchId = db.jntuh_college_examination_branch.Where(examinationBranch => examinationBranch.collegeId == userCollegeID)
                                                                               .Select(examinationBranch => examinationBranch.id).FirstOrDefault();

            int existsExaminationBranchSecurityId = db.jntuh_college_examination_branch_security.Where(examinationBranchSecurity => examinationBranchSecurity.collegeId == userCollegeID)
                                                                               .Select(examinationBranchSecurity => examinationBranchSecurity.id).FirstOrDefault();

            if (existsExaminationBranchId == 0 && existsExaminationBranchSecurityId == 0)
            {
                ViewBag.Norecords = true;
            }
            else
            {
                jntuh_college_examination_branch examinationBranchDetails = db.jntuh_college_examination_branch.Find(existsExaminationBranchId);

                if (examinationBranchDetails != null)
                {
                    collegeExaminationBranch.collegeId = examinationBranchDetails.collegeId;
                    collegeExaminationBranch.examinationBranchArea = examinationBranchDetails.examinationBranchArea;
                    collegeExaminationBranch.isConfidenatialRoomExists = examinationBranchDetails.isConfidenatialRoomExists;
                    collegeExaminationBranch.isAdjacentPrincipalRoom = examinationBranchDetails.isAdjacentPrincipalRoom;
                    collegeExaminationBranch.createdOn = examinationBranchDetails.createdOn;
                    collegeExaminationBranch.createdBy = examinationBranchDetails.createdBy;
                }

                jntuh_college_examination_branch_security examinationBranchSecurityDetails = db.jntuh_college_examination_branch_security.Find(existsExaminationBranchSecurityId);

                if (examinationBranchSecurityDetails != null)
                {
                    collegeExaminationBranch.collegeId = examinationBranchSecurityDetails.collegeId;
                    collegeExaminationBranch.securityMesearesTaken1 = examinationBranchSecurityDetails.securityMesearesTaken1;
                    collegeExaminationBranch.securityMesearesTaken2 = examinationBranchSecurityDetails.securityMesearesTaken2;
                    collegeExaminationBranch.securityMesearesTaken3 = examinationBranchSecurityDetails.securityMesearesTaken3;
                    collegeExaminationBranch.createdOn = examinationBranchSecurityDetails.createdOn;
                    collegeExaminationBranch.createdBy = examinationBranchSecurityDetails.createdBy;
                }
            }
            ViewBag.StaffMembers = db.jntuh_college_examination_branch_staff.Where(staffMembers => staffMembers.collegeId == userCollegeID).ToList();
            ViewBag.Count = db.jntuh_college_examination_branch_staff.Where(staffMembers => staffMembers.collegeId == userCollegeID).ToList().Count();
            return View("UserView", collegeExaminationBranch);
        }
    }
}
