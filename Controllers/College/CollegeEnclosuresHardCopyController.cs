using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Security;
using UAAAS.Models;

namespace UAAAS.Controllers.College
{
    [ErrorHandling]
    public class CollegeEnclosuresHardCopyController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();
       
        [HttpGet]
        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Create(string collegeId)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0 && collegeId != null)
            {
                if (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry"))
                {
                    userCollegeID = Convert.ToInt32(Utilities.DecryptString(collegeId, WebConfigurationManager.AppSettings["CryptoKey"]));
                }
            }
            if (userCollegeID == 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("Create", "CollegeInformation");
            }
            if (userCollegeID == 0 && (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry")))
            {
                return RedirectToAction("Create", "CollegeInformation", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
            }
            //int[] enclosureIDs = db.jntuh_enclosures.Where(m => m.isActive == true).Select(m => m.id).ToArray();
           // int collegeenclosureID = db.jntuh_college_enclosures_hardcopy.Where(m => m.collegeID == userCollegeID && enclosureIDs.Contains(m.id))
             //                                                                     .Select(a => a.id).FirstOrDefault();
            int collegeenclosureID = db.jntuh_college_enclosures_hardcopy.Where(m => m.collegeID == userCollegeID).Select(m => m).ToList().Count();
            DateTime todayDate = DateTime.Now.Date;
            int status = db.jntuh_college_edit_status.Where(editStatus => editStatus.collegeId == userCollegeID &&
                                                                                     editStatus.IsCollegeEditable == true &&
                                                                                     editStatus.editFromDate <= todayDate &&
                                                                                     editStatus.editToDate >= todayDate)
                                                                .Select(editStatus => editStatus.id)
                                                                .FirstOrDefault();

            if (userCollegeID > 0 && collegeenclosureID > 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("View", "CollegeEnclosuresHardCopy");
            }
            if (userCollegeID > 0 && collegeenclosureID > 0 && (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry")))
            {
                return RedirectToAction("Edit", "CollegeEnclosuresHardCopy", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
            }

            if (collegeenclosureID == 0 && status == 0 && Roles.IsUserInRole("College"))
            {
                ViewBag.NotUpload = true;
            }
            else
            {
                ViewBag.NotUpload = false;
            }
            bool isPageEditable = db.jntuh_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("HC") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

            if (isPageEditable)
            {
                ViewBag.IsEditable = true;
            }
            else
            {
                ViewBag.IsEditable = false;
                return RedirectToAction("View", "CollegeEnclosuresHardCopy");
            }

            List<CollegeEnclosuresHardCopy> collegeEnclosuresHardCopy = db.jntuh_enclosures.Where(m => m.isActive == true)
                                                                          .Select(m => new CollegeEnclosuresHardCopy
                                                                          {
                                                                              id = m.id,
                                                                              documentName = m.documentName,
                                                                              collegeID = userCollegeID
                                                                          }).OrderBy(d=>d.id).ToList();
            ViewBag.Count = collegeEnclosuresHardCopy.Count();
            return View(collegeEnclosuresHardCopy);
        }

       
        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Create(ICollection<CollegeEnclosuresHardCopy> collegeEnclosuresHardCopy)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                foreach (var item in collegeEnclosuresHardCopy)
                {
                    userCollegeID = item.collegeID;
                }
            }
            SaveMiscellaneousParameters(collegeEnclosuresHardCopy);
            TempData["Success"] = "Added successfully";
            List<CollegeEnclosuresHardCopy> collegeMiscellaneousParameters = db.jntuh_enclosures.Where(m => m.isActive == true)
                                                                          .Select(m => new CollegeEnclosuresHardCopy
                                                                          {
                                                                              id = m.id,
                                                                              documentName = m.documentName,
                                                                              collegeID = userCollegeID
                                                                          }).ToList();
            ViewBag.Count = collegeMiscellaneousParameters.Count();
            return View(collegeMiscellaneousParameters);
        }

        private void SaveMiscellaneousParameters(ICollection<CollegeEnclosuresHardCopy> collegeEnclosuresHardCopy)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                foreach (var item in collegeEnclosuresHardCopy)
                {
                    userCollegeID = item.collegeID;
                }
            }
            var Meassage = string.Empty;
            if (ModelState.IsValid)
            {
                foreach (var item in collegeEnclosuresHardCopy)
                {
                    jntuh_college_enclosures_hardcopy collegeEnclosures = new jntuh_college_enclosures_hardcopy();
                    collegeEnclosures.collegeID = userCollegeID;
                    collegeEnclosures.enclosureId = item.id;
                    collegeEnclosures.isSelected =Convert.ToBoolean(item.isSelected);
                    collegeEnclosures.isActive = true;
                    int existId = db.jntuh_college_enclosures_hardcopy.Where(m => m.collegeID == userCollegeID && m.enclosureId == item.id)
                                                                           .Select(m => m.id)
                                                                           .FirstOrDefault();
                    if (existId == 0)
                    {
                        collegeEnclosures.createdBy = userID;
                        collegeEnclosures.createdOn = DateTime.Now;
                        db.jntuh_college_enclosures_hardcopy.Add(collegeEnclosures);
                    }
                    else
                    {
                        collegeEnclosures.id = existId;
                        collegeEnclosures.createdBy = db.jntuh_college_enclosures_hardcopy.Where(m => m.collegeID == userCollegeID && m.enclosureId == item.id)
                                                                           .Select(m => m.createdBy)
                                                                           .FirstOrDefault();
                        collegeEnclosures.createdOn = db.jntuh_college_enclosures_hardcopy.Where(m => m.collegeID == userCollegeID && m.enclosureId == item.id)
                                                                           .Select(m => m.createdOn)
                                                                           .FirstOrDefault();
                        collegeEnclosures.updatedBy = userID;
                        collegeEnclosures.updatedOn = DateTime.Now;
                        db.Entry(collegeEnclosures).State = EntityState.Modified;
                    }
                    db.SaveChanges();
                }
            }
        }

       
        [HttpGet]
        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Edit(string collegeId)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0 && collegeId != null)
            {
                if (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry"))
                {
                    userCollegeID = Convert.ToInt32(Utilities.DecryptString(collegeId, WebConfigurationManager.AppSettings["CryptoKey"]));
                }
            }
            //int[] enclosureIDs = db.jntuh_enclosures.Where(m => m.isActive == true).Select(m => m.id).ToArray();
            //int collegeenclosureID = db.jntuh_college_enclosures_hardcopy.Where(m => m.collegeID == userCollegeID && enclosureIDs.Contains(m.id))
            //                                                                      .Select(a => a.id).FirstOrDefault();
            int collegeenclosureID = db.jntuh_college_enclosures_hardcopy.Where(m => m.collegeID == userCollegeID).Select(m => m).ToList().Count();
            DateTime todayDate = DateTime.Now.Date;
            int status = db.jntuh_college_edit_status.Where(editStatus => editStatus.collegeId == userCollegeID &&
                                                                                     editStatus.IsCollegeEditable == true &&
                                                                                     editStatus.editFromDate <= todayDate &&
                                                                                     editStatus.editToDate >= todayDate)
                                                                .Select(editStatus => editStatus.id)
                                                                .FirstOrDefault();
            if (collegeenclosureID == 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("Create", "CollegeEnclosuresHardCopy");
            }
            if (status == 0 && Roles.IsUserInRole("College"))
            {
                ViewBag.IsEditable = false;
                return RedirectToAction("View", "CollegeEnclosuresHardCopy");
            }
            else
            {
                bool isPageEditable = db.jntuh_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("HC") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

                if (isPageEditable)
                {
                    ViewBag.IsEditable = true;
                }
                else
                {
                    ViewBag.IsEditable = false;
                    return RedirectToAction("View", "CollegeEnclosuresHardCopy");
                }
            }

            List<CollegeEnclosuresHardCopy> collegeEnclosuresHardCopy = (from r in db.jntuh_enclosures.Where(r => r.isActive == true)
                                                                     select new CollegeEnclosuresHardCopy
                                                                     {
                                                                         id = r.id,
                                                                         documentName = r.documentName,
                                                                         collegeID = userCollegeID
                                                                     }).OrderBy(d=>d.id).ToList();
            foreach (var item in collegeEnclosuresHardCopy)
            {
                var result = db.jntuh_college_enclosures_hardcopy.Where(d => d.collegeID == userCollegeID && d.enclosureId == item.id)
                                                                        .Select(d => new
                                                                        {
                                                                            enclosureId = d.enclosureId,
                                                                            isAvailable = d.isSelected,
                                                                            collegeId = userCollegeID
                                                                        }).FirstOrDefault();
                if (result != null)
                {
                    if (result.enclosureId == item.id && result.isAvailable == true)
                    {
                        item.isSelected = "True";
                    }
                    else
                    {
                        item.isSelected ="False";
                    }
                }
                else
                {
                    item.isSelected = "New";
                }
            }

            ViewBag.Count = collegeEnclosuresHardCopy.Count();
            return View("Create", collegeEnclosuresHardCopy);
        }

        
        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Edit(ICollection<CollegeEnclosuresHardCopy> collegeEnclosuresHardCopy)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                foreach (var item in collegeEnclosuresHardCopy)
                {
                    userCollegeID = item.collegeID;
                }

            }
            SaveMiscellaneousParameters(collegeEnclosuresHardCopy);
            TempData["Success"] = "Updated successfully";
            List<CollegeEnclosuresHardCopy> collegeMiscellaneousParameters = db.jntuh_enclosures.Where(m => m.isActive == true)
                                                                          .Select(m => new CollegeEnclosuresHardCopy
                                                                          {
                                                                              id = m.id,
                                                                              documentName = m.documentName,
                                                                              collegeID = userCollegeID
                                                                          }).ToList();
            ViewBag.Count = collegeMiscellaneousParameters.Count();            
            return View("Create", collegeMiscellaneousParameters);
        }


      
        [HttpGet]
        [Authorize(Roles = "Committee,DataEntry,College,Admin")]
        public ActionResult View(string id)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                userCollegeID = userCollegeID = Convert.ToInt32(Utilities.DecryptString(id, WebConfigurationManager.AppSettings["CryptoKey"]));
            }
            //int[] enclosureIDs = db.jntuh_enclosures.Where(m => m.isActive == true).Select(m => m.id).ToArray();
            //int collegeenclosureID = db.jntuh_college_enclosures_hardcopy.Where(m => m.collegeID == userCollegeID && enclosureIDs.Contains(m.id))
            //                                                                      .Select(a => a.id).FirstOrDefault();
            int collegeenclosureID = db.jntuh_college_enclosures_hardcopy.Where(m => m.collegeID == userCollegeID).Select(m => m).ToList().Count();
            DateTime todayDate = DateTime.Now.Date;
            int status = db.jntuh_college_edit_status.Where(editStatus => editStatus.collegeId == userCollegeID &&
                                                                          editStatus.IsCollegeEditable == true &&
                                                                          editStatus.editFromDate <= todayDate &&
                                                                          editStatus.editToDate >= todayDate)
                                                     .Select(editStatus => editStatus.id)
                                                     .FirstOrDefault();
            if (status > 0 && Roles.IsUserInRole("College"))
            {
                bool isPageEditable = db.jntuh_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("HC") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

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
            List<CollegeEnclosuresHardCopy> collegeEnclosuresHardCopy = (from r in db.jntuh_enclosures.Where(r => r.isActive == true)
                                                                     select new CollegeEnclosuresHardCopy
                                                                     {
                                                                         id = r.id,
                                                                         documentName = r.documentName
                                                                     }).OrderBy(d=>d.id).ToList();
            foreach (var item in collegeEnclosuresHardCopy)
            {
                var result = db.jntuh_college_enclosures_hardcopy.Where(d => d.collegeID == userCollegeID && d.enclosureId == item.id)
                                                                        .Select(d => new
                                                                        {
                                                                            enclosureId = d.enclosureId,
                                                                            isAvailable = d.isSelected
                                                                        }).FirstOrDefault();
                if (result != null)
                {
                    if (result.enclosureId == item.id && result.isAvailable == true)
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
            if (collegeenclosureID == 0)
            {
                ViewBag.Norecords = true;
            }
            else
            {
                ViewBag.Count = collegeEnclosuresHardCopy.Count();
            }
            return View(collegeEnclosuresHardCopy);
        }

    }
}
