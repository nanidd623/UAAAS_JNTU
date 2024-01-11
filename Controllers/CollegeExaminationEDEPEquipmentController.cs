using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Data;
using UAAAS.Models;
using System.Web.Configuration;
using System.Configuration;

namespace UAAAS.Controllers
{
    [ErrorHandling]
    public class CollegeExaminationEDEPEquipmentController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();       

        [Authorize(Roles = "College")]
        public ActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Create(string collegeId)
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
            if (userCollegeID == 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("Create", "CollegeInformation");
            }
            if (userCollegeID == 0 && (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry")))
            {
                return RedirectToAction("Create", "CollegeInformation",new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
            }
            int[] EDEPEquipment = db.jntuh_edep_equipment.Where(equipment => equipment.isActive == true).Select(equipment => equipment.id).ToArray();
            int existId = db.jntuh_college_examination_branch_edep.Where(examinationEDEPEquipment => examinationEDEPEquipment.collegeId == userCollegeID &&
                                                                         EDEPEquipment.Contains(examinationEDEPEquipment.EDEPEquipmentId))
                                                                  .Select(examinationEDEPEquipment => examinationEDEPEquipment.id).FirstOrDefault();
            List<CollegeEDEPDetails> collegeEDEPDetails = db.jntuh_edep_equipment.Where(equipment => equipment.isActive == true)
                                                                                 .Select(equipment => new CollegeEDEPDetails
                                                                                 {
                                                                                     EDEPEquipmentId = equipment.id,
                                                                                     EDEPEquipment = equipment.equipmentName,
                                                                                     ActualValue = string.Empty,
                                                                                     collegeId=userCollegeID
                                                                                 }).ToList();

            ViewBag.Count = collegeEDEPDetails.Count();
            DateTime todayDate = DateTime.Now.Date;
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            int status = db.jntuh_college_edit_status.Where(editStatus => editStatus.academicyearId == prAy && editStatus.collegeId == userCollegeID &&
                                                                                      editStatus.IsCollegeEditable == true &&
                                                                                      editStatus.editFromDate <= todayDate &&
                                                                                      editStatus.editToDate >= todayDate)
                                                                 .Select(editStatus => editStatus.id)
                                                                 .FirstOrDefault();

            if (userCollegeID > 0 && existId > 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("View", "CollegeExaminationEDEPEquipment");
            }
            if (userCollegeID > 0 && existId > 0 && (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry")))
            {
                return RedirectToAction("Edit", "CollegeExaminationEDEPEquipment", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
            }

            if (existId == 0 && status == 0 && Roles.IsUserInRole("College"))
            {
                ViewBag.NotUpload = true;
            }
            else
            {
                ViewBag.NotUpload = false;
            }

            bool isPageEditable = db.jntuh_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("EE") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

            if (isPageEditable)
            {
                ViewBag.IsEditable = true;
            }
            else
            {
                ViewBag.IsEditable = false;
                return RedirectToAction("View", "CollegeExaminationEDEPEquipment");
            }

            return View(collegeEDEPDetails);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Create(ICollection<CollegeEDEPDetails> collegeEDEPDetails)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userID).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                foreach (var item in collegeEDEPDetails)
                {
                    userCollegeID = item.collegeId;
                }
            }
            SaveEDEPDetails(collegeEDEPDetails);

            List<CollegeEDEPDetails> examinationBanchEDEPDetails = db.jntuh_college_examination_branch_edep
                                                                     .Join(db.jntuh_edep_equipment, edepEquipment => edepEquipment.EDEPEquipmentId, examinationedep => examinationedep.id,
                                                                          (edepEquipment, examinationedep) => new
                                                                          {
                                                                              edepEquipment.EDEPEquipmentId,
                                                                              edepEquipment.ActualValue,
                                                                              edepEquipment.collegeId,
                                                                              examinationedep.equipmentName,
                                                                              examinationedep.isActive
                                                                          })
                                                                     .Where(equipment => equipment.isActive == true && equipment.collegeId == userCollegeID)
                                                                     .Select(equipment => new CollegeEDEPDetails
                                                                     {
                                                                         EDEPEquipmentId = equipment.EDEPEquipmentId,
                                                                         ActualValue = equipment.ActualValue,
                                                                         EDEPEquipment = equipment.equipmentName,
                                                                         collegeId=userCollegeID
                                                                     }).ToList();

            ViewBag.Count = examinationBanchEDEPDetails.Count();
            return View(examinationBanchEDEPDetails);
        }

        private void SaveEDEPDetails(ICollection<CollegeEDEPDetails> collegeEDEPDetails)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userID).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                foreach (var item in collegeEDEPDetails)
                {
                    userCollegeID = item.collegeId;
                }
            }

            var resultMessage = string.Empty;

            if (ModelState.IsValid)
            {
                foreach (CollegeEDEPDetails item in collegeEDEPDetails)
                {
                    jntuh_college_examination_branch_edep equipmentDetails = new jntuh_college_examination_branch_edep();
                    equipmentDetails.collegeId = userCollegeID;
                    equipmentDetails.EDEPEquipmentId = item.EDEPEquipmentId;
                    equipmentDetails.ActualValue = item.ActualValue;

                    int existId = db.jntuh_college_examination_branch_edep.Where(equipment => equipment.collegeId == userCollegeID &&
                                                                                 equipment.EDEPEquipmentId == item.EDEPEquipmentId)
                                                                          .Select(equipment => equipment.id).FirstOrDefault();
                    if (existId == 0)
                    {
                        equipmentDetails.createdBy = userID;
                        equipmentDetails.createdOn = DateTime.Now;
                        db.jntuh_college_examination_branch_edep.Add(equipmentDetails);
                        db.SaveChanges();
                        resultMessage = "Save";
                    }
                    else
                    {
                        equipmentDetails.id = existId;
                        equipmentDetails.createdOn = db.jntuh_college_examination_branch_edep.Where(equipment => equipment.id == existId)
                                                                                             .Select(equipment => equipment.createdOn)
                                                                                             .FirstOrDefault();
                        equipmentDetails.createdBy = db.jntuh_college_examination_branch_edep.Where(equipment => equipment.id == existId)
                                                                                             .Select(equipment => equipment.createdBy)
                                                                                             .FirstOrDefault();
                        equipmentDetails.updatedBy = userID;
                        equipmentDetails.updatedOn = DateTime.Now; 
                        db.Entry(equipmentDetails).State = EntityState.Modified;
                        db.SaveChanges();
                        resultMessage = "Update";
                    }
                }
                if (resultMessage == "Update")
                {
                    TempData["Success"] = "College Examination Branch EDEP Equipment Details are Updated successfully";
                }
                else
                {
                    TempData["Success"] = "College Examination Branch EDEP Equipment Details are Added successfully";
                }
            }
        }

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
            if (userCollegeID == 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("Create", "CollegeInformation");
            }
            int[] EDEPEquipment = db.jntuh_edep_equipment.Where(equipment => equipment.isActive == true).Select(equipment => equipment.id).ToArray();
            int existId = db.jntuh_college_examination_branch_edep.Where(examinationEDEPEquipment => examinationEDEPEquipment.collegeId == userCollegeID &&
                                                                         EDEPEquipment.Contains(examinationEDEPEquipment.EDEPEquipmentId))
                                                                  .Select(examinationEDEPEquipment => examinationEDEPEquipment.id).FirstOrDefault();
           

            if (existId == 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("Create", "CollegeExaminationEDEPEquipment");
            }
            if (existId == 0 && (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry")))
            {
                return RedirectToAction("Create", "CollegeExaminationEDEPEquipment", new { collegeId = Utilities.EncryptString(collegeId.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
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
                return RedirectToAction("View", "CollegeExaminationEDEPEquipment");
            }
            else
            {
                //ViewBag.IsEditable = true;

                ////RAMESH:To-DisableEdit
                //if (ConfigurationManager.AppSettings["ShouldFormsBeDisabled"].ToString() == "true")
                //{
                //    ViewBag.IsEditable = false;
                //    return RedirectToAction("View", "CollegeExaminationEDEPEquipment");
                //}
                //else
                //{
                //    ViewBag.IsEditable = true;
                //}

                bool isPageEditable = db.jntuh_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("EE") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

                if (isPageEditable)
                {
                    ViewBag.IsEditable = true;
                }
                else
                {
                    ViewBag.IsEditable = false;
                    return RedirectToAction("View", "CollegeExaminationEDEPEquipment");
                }
            }

            List<CollegeEDEPDetails> examinationBanchEDEPDetails = db.jntuh_edep_equipment.Where(equipment => equipment.isActive == true)
                                                                                 .Select(equipment => new CollegeEDEPDetails
                                                                                 {
                                                                                     EDEPEquipmentId = equipment.id,
                                                                                     EDEPEquipment = equipment.equipmentName,
                                                                                     ActualValue = string.Empty,
                                                                                     collegeId=userCollegeID
                                                                                 }).ToList();

            foreach (var item in examinationBanchEDEPDetails)
            {
                item.ActualValue = db.jntuh_college_examination_branch_edep.Where(e => e.collegeId == userCollegeID && e.EDEPEquipmentId == item.EDEPEquipmentId).Select(e => e.ActualValue).FirstOrDefault();
            }
            ViewBag.Update = true;
            ViewBag.Count = examinationBanchEDEPDetails.Count();
            return View("Create", examinationBanchEDEPDetails);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Edit(ICollection<CollegeEDEPDetails> collegeEDEPDetails)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userID).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                foreach (var item in collegeEDEPDetails)
                {
                    userCollegeID = item.collegeId;
                }
            }
            SaveEDEPDetails(collegeEDEPDetails);

            List<CollegeEDEPDetails> examinationBanchEDEPDetails = db.jntuh_edep_equipment.Where(equipment => equipment.isActive == true)
                                                                                 .Select(equipment => new CollegeEDEPDetails
                                                                                 {
                                                                                     EDEPEquipmentId = equipment.id,
                                                                                     EDEPEquipment = equipment.equipmentName,
                                                                                     ActualValue = string.Empty,
                                                                                     collegeId=userCollegeID
                                                                                 }).ToList();

            foreach (var item in examinationBanchEDEPDetails)
            {
                item.ActualValue = db.jntuh_college_examination_branch_edep.Where(e => e.collegeId == userCollegeID && e.EDEPEquipmentId == item.EDEPEquipmentId).Select(e => e.ActualValue).FirstOrDefault();
            }
            ViewBag.Update = true;

            ViewBag.Count = examinationBanchEDEPDetails.Count();

            return View("View");

            // Changed by Naushad Khan
           // return View("Create",examinationBanchEDEPDetails);
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
            int collegeEDEPId = db.jntuh_college_examination_branch_edep.Where(a => a.collegeId == userCollegeID).Select(a => a.id).FirstOrDefault();



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
            //    //RAMESH:To-DisableEdit
            //    if (ConfigurationManager.AppSettings["ShouldFormsBeDisabled"].ToString() == "true")
            //    {
            //        ViewBag.IsEditable = false;
            //    }
            //    else
            //    {
            //        ViewBag.IsEditable = true;
            //    }

            //    //ViewBag.IsEditable = true;

                bool isPageEditable = db.jntuh_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("EE") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

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

            List<CollegeEDEPDetails> examinationBanchEDEPDetails = db.jntuh_edep_equipment.Where(equipment => equipment.isActive == true)
                                                                                 .Select(equipment => new CollegeEDEPDetails
                                                                                 {
                                                                                     EDEPEquipmentId = equipment.id,
                                                                                     EDEPEquipment = equipment.equipmentName,
                                                                                     ActualValue = string.Empty,
                                                                                 }).ToList();
            if (collegeEDEPId == 0)
            {
                ViewBag.Norecords = true;
            }
            else
            {
                foreach (var item in examinationBanchEDEPDetails)
                {
                    item.ActualValue = db.jntuh_college_examination_branch_edep.Where(e => e.collegeId == userCollegeID && e.EDEPEquipmentId == item.EDEPEquipmentId).Select(e => e.ActualValue).FirstOrDefault();

                    if (item.ActualValue == string.Empty)
                    {
                        ViewBag.ActualValue = true;
                    }

                }
            }
            ViewBag.Count = examinationBanchEDEPDetails.Count();
            return View("View", examinationBanchEDEPDetails);
        }

        public ActionResult UserView(string id)
        {
            int userCollegeID = Convert.ToInt32(Utilities.DecryptString(id, WebConfigurationManager.AppSettings["CryptoKey"]));
            int collegeEDEPId = db.jntuh_college_examination_branch_edep.Where(a => a.collegeId == userCollegeID).Select(a => a.id).FirstOrDefault();
            List<CollegeEDEPDetails> examinationBanchEDEPDetails = db.jntuh_edep_equipment.Where(equipment => equipment.isActive == true)
                                                                                 .Select(equipment => new CollegeEDEPDetails
                                                                                 {
                                                                                     EDEPEquipmentId = equipment.id,
                                                                                     EDEPEquipment = equipment.equipmentName,
                                                                                     ActualValue = string.Empty,
                                                                                 }).ToList();
            if (collegeEDEPId == 0)
            {
                ViewBag.Norecords = true;
            }
            else
            {
                foreach (var item in examinationBanchEDEPDetails)
                {
                    item.ActualValue = db.jntuh_college_examination_branch_edep.Where(e => e.collegeId == userCollegeID && e.EDEPEquipmentId == item.EDEPEquipmentId).Select(e => e.ActualValue).FirstOrDefault();

                    if (item.ActualValue == string.Empty)
                    {
                        ViewBag.ActualValue = true;
                    }

                }
            }
            ViewBag.Count = examinationBanchEDEPDetails.Count();
            return View("UserView", examinationBanchEDEPDetails);
        }

    }
}
