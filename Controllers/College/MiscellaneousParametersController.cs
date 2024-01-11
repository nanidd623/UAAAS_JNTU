using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Web.Security;
using System.Web.Configuration;
using UAAAS.Models;

namespace UAAAS.Controllers
{
    [ErrorHandling]
    public class MiscellaneousParametersController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();

        //
        // GET: /MiscellaneousParameters/MiscellaneousParametersCreate
        [HttpGet]
        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult MiscellaneousParametersCreate(string collegeId)
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
            int[] miscellaneousId = db.jntuh_miscellaneous_parameters.Where(m => m.isActive == true).Select(m => m.id).ToArray();
            int collegeMiscellaneousId = db.jntuh_college_miscellaneous_parameters.Where(m => m.collegeId == userCollegeID && miscellaneousId.Contains(m.typeId))
                                                                                  .Select(a => a.id).FirstOrDefault();
            DateTime todayDate = DateTime.Now.Date;
            int status = db.jntuh_college_edit_status.Where(editStatus => editStatus.collegeId == userCollegeID &&
                                                                                     editStatus.IsCollegeEditable == true &&
                                                                                     editStatus.editFromDate <= todayDate &&
                                                                                     editStatus.editToDate >= todayDate)
                                                                .Select(editStatus => editStatus.id)
                                                                .FirstOrDefault();

            if (userCollegeID > 0 && collegeMiscellaneousId > 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("MiscellaneousParametersView", "MiscellaneousParameters");
            }
            if (userCollegeID > 0 && collegeMiscellaneousId > 0 && (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry")))
            {
                return RedirectToAction("MiscellaneousParametersEdit", "MiscellaneousParameters", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
            }

            if (collegeMiscellaneousId == 0 && status == 0 && Roles.IsUserInRole("College"))
            {
                ViewBag.NotUpload = true;
            }
            else
            {
                ViewBag.NotUpload = false;
            }
            List<MiscellaneousParameters> miscellaneousParameters = db.jntuh_miscellaneous_parameters.Where(m => m.isActive == true)
                                                                          .Select(m => new MiscellaneousParameters
                                                                          {
                                                                              id = m.id,
                                                                              type = m.type,
                                                                              collegeId=userCollegeID
                                                                          }).ToList();
            ViewBag.Count = miscellaneousParameters.Count();
            return View("~/Views/College/MiscellaneousParametersCreate.cshtml", miscellaneousParameters);
        }

        //
        // POST: /MiscellaneousParameters/MiscellaneousParametersCreate
        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult MiscellaneousParametersCreate(ICollection<MiscellaneousParameters> miscellaneousParameters)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                foreach (var item in miscellaneousParameters)
                {
                    userCollegeID = item.collegeId;
                }
            }
            SaveMiscellaneousParameters(miscellaneousParameters);
            TempData["Success"] = "Added successfully";
            List<MiscellaneousParameters> collegeMiscellaneousParameters = db.jntuh_miscellaneous_parameters.Where(m => m.isActive == true)
                                                                          .Select(m => new MiscellaneousParameters
                                                                          {
                                                                              id = m.id,
                                                                              type = m.type,
                                                                              collegeId=userCollegeID
                                                                          }).ToList();
            ViewBag.Count = collegeMiscellaneousParameters.Count();
            return View("~/Views/College/MiscellaneousParametersCreate.cshtml", collegeMiscellaneousParameters);
        }

        private void SaveMiscellaneousParameters(ICollection<MiscellaneousParameters> miscellaneousParameters)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                foreach (var item in miscellaneousParameters)
                {
                    userCollegeID = item.collegeId;
                }
            }
            var Meassage = string.Empty;
            if (ModelState.IsValid)
            {
                foreach (MiscellaneousParameters item in miscellaneousParameters)
                {
                    jntuh_college_miscellaneous_parameters collegeMiscellaneousParameters = new jntuh_college_miscellaneous_parameters();
                    collegeMiscellaneousParameters.collegeId = userCollegeID;
                    collegeMiscellaneousParameters.typeId = item.id;
                    collegeMiscellaneousParameters.isSelected = Convert.ToBoolean(item.isSelected);

                    int existId = db.jntuh_college_miscellaneous_parameters.Where(m => m.collegeId == userCollegeID && m.typeId == item.id)
                                                                           .Select(m => m.id)
                                                                           .FirstOrDefault();
                    if (existId == 0)
                    {
                        collegeMiscellaneousParameters.createdBy = userID;
                        collegeMiscellaneousParameters.createdOn = DateTime.Now;
                        db.jntuh_college_miscellaneous_parameters.Add(collegeMiscellaneousParameters);
                    }
                    else
                    {
                        collegeMiscellaneousParameters.id = existId;
                        collegeMiscellaneousParameters.createdBy = db.jntuh_college_miscellaneous_parameters.Where(m => m.collegeId == userCollegeID && m.typeId == item.id)
                                                                           .Select(m => m.createdBy)
                                                                           .FirstOrDefault();
                        collegeMiscellaneousParameters.createdOn = db.jntuh_college_miscellaneous_parameters.Where(m => m.collegeId == userCollegeID && m.typeId == item.id)
                                                                           .Select(m => m.createdOn)
                                                                           .FirstOrDefault();
                        collegeMiscellaneousParameters.updatedBy = userID;
                        collegeMiscellaneousParameters.updatedOn = DateTime.Now;
                        db.Entry(collegeMiscellaneousParameters).State = EntityState.Modified;
                    }
                    db.SaveChanges();
                }
            }
        }

        //
        // GET: /MiscellaneousParameters/MiscellaneousParametersEdit
        [HttpGet]
        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult MiscellaneousParametersEdit(string collegeId)
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
            int[] miscellaneousId = db.jntuh_miscellaneous_parameters.Where(m => m.isActive == true).Select(m => m.id).ToArray();
            int collegeMiscellaneousId = db.jntuh_college_miscellaneous_parameters.Where(m => m.collegeId == userCollegeID && miscellaneousId.Contains(m.typeId))
                                                                                  .Select(a => a.id).FirstOrDefault();
            DateTime todayDate = DateTime.Now.Date;
            int status = db.jntuh_college_edit_status.Where(editStatus => editStatus.collegeId == userCollegeID &&
                                                                                     editStatus.IsCollegeEditable == true &&
                                                                                     editStatus.editFromDate <= todayDate &&
                                                                                     editStatus.editToDate >= todayDate)
                                                                .Select(editStatus => editStatus.id)
                                                                .FirstOrDefault();
            if (collegeMiscellaneousId == 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("MiscellaneousParametersCreate", "MiscellaneousParameters");
            }
            if (status == 0 && Roles.IsUserInRole("College"))
            {
                ViewBag.IsEditable = false;
                return RedirectToAction("MiscellaneousParametersView", "MiscellaneousParameters");
            }
            else
            {
                ViewBag.IsEditable = true;
            }

            List<MiscellaneousParameters> miscellaneousParameters = (from r in db.jntuh_miscellaneous_parameters.Where(r => r.isActive == true)
                                                                     select new MiscellaneousParameters
                                                                           {
                                                                               id = r.id,
                                                                               type = r.type,
                                                                               collegeId=userCollegeID
                                                                           }).ToList();
            foreach (var item in miscellaneousParameters)
            {
                var result = db.jntuh_college_miscellaneous_parameters.Where(d => d.collegeId == userCollegeID && d.typeId == item.id)
                                                                        .Select(d => new
                                                                        {
                                                                            typeId = d.typeId,
                                                                            isAvailable = d.isSelected,
                                                                            collegeId=userCollegeID
                                                                        }).FirstOrDefault();
                if (result != null)
                {
                    if (result.typeId == item.id && result.isAvailable == true)
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

            ViewBag.Count = miscellaneousParameters.Count();
            return View("~/Views/College/MiscellaneousParametersCreate.cshtml", miscellaneousParameters);
        }

        //
        // POST: /MiscellaneousParameters/MiscellaneousParametersEdit
        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult MiscellaneousParametersEdit(ICollection<MiscellaneousParameters> miscellaneousParameters)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                foreach (var item in miscellaneousParameters)
                {
                    userCollegeID = item.collegeId;
                }

            }
            SaveMiscellaneousParameters(miscellaneousParameters);
            TempData["Success"] = "Updated successfully";
            List<MiscellaneousParameters> collegeMiscellaneousParameters = db.jntuh_miscellaneous_parameters.Where(m => m.isActive == true)
                                                                          .Select(m => new MiscellaneousParameters
                                                                          {
                                                                              id = m.id,
                                                                              type = m.type,
                                                                              collegeId=userCollegeID
                                                                          }).ToList();
            ViewBag.Count = collegeMiscellaneousParameters.Count();
            return View("~/Views/College/MiscellaneousParametersCreate.cshtml", collegeMiscellaneousParameters);
        }


        //
        // GET: /MiscellaneousParameters/MiscellaneousParametersView
        [HttpGet]
        [Authorize(Roles = "Committee,DataEntry,College,Admin")]
        public ActionResult MiscellaneousParametersView(string id)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                userCollegeID = userCollegeID = Convert.ToInt32(Utilities.DecryptString(id, WebConfigurationManager.AppSettings["CryptoKey"]));
            }
            int[] miscellaneousId = db.jntuh_miscellaneous_parameters.Where(m => m.isActive == true).Select(m => m.id).ToArray();
            int collegeMiscellaneousId = db.jntuh_college_miscellaneous_parameters.Where(m => m.collegeId == userCollegeID && miscellaneousId.Contains(m.typeId))
                                                                                  .Select(a => a.id).FirstOrDefault();
            DateTime todayDate = DateTime.Now.Date;
            int status = db.jntuh_college_edit_status.Where(editStatus => editStatus.collegeId == userCollegeID &&
                                                                          editStatus.IsCollegeEditable == true &&
                                                                          editStatus.editFromDate <= todayDate &&
                                                                          editStatus.editToDate >= todayDate)
                                                     .Select(editStatus => editStatus.id)
                                                     .FirstOrDefault();
            if (status > 0 && Roles.IsUserInRole("College"))
            {
                ViewBag.IsEditable = true;
            }
            else
            {
                ViewBag.IsEditable = false;
            }
            List<MiscellaneousParameters> miscellaneousParameters = (from r in db.jntuh_miscellaneous_parameters.Where(r => r.isActive == true)
                                                                     select new MiscellaneousParameters
                                                                           {
                                                                               id = r.id,
                                                                               type = r.type
                                                                           }).ToList();
            foreach (var item in miscellaneousParameters)
            {
                var result = db.jntuh_college_miscellaneous_parameters.Where(d => d.collegeId == userCollegeID && d.typeId == item.id)
                                                                        .Select(d => new
                                                                        {
                                                                            typeId = d.typeId,
                                                                            isAvailable = d.isSelected
                                                                        }).FirstOrDefault();
                if (result != null)
                {
                    if (result.typeId == item.id && result.isAvailable == true)
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
            if (collegeMiscellaneousId == 0)
            {
                ViewBag.Norecords = true;
            }
            else
            {
                ViewBag.Count = miscellaneousParameters.Count();
            }
            return View("~/Views/College/MiscellaneousParametersView.cshtml", miscellaneousParameters);
        }

    }
}
