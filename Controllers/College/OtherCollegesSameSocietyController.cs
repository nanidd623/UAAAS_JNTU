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
    public class OtherCollegesSameSocietyController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Index(string collegeId)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();

            if (userCollegeID == 0)
            {
                if (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry"))
                {
                    userCollegeID = Convert.ToInt32(Utilities.DecryptString(collegeId, WebConfigurationManager.AppSettings["CryptoKey"]));
                }
            }
            List<OtherCollegesSameSociety> otherCollegesSameSociety = db.jntuh_society_other_locations_colleges.Where(a => a.collegeId == userCollegeID).Select(a =>
                                           new OtherCollegesSameSociety
                                           {
                                               id = a.id,
                                               collegeId = a.collegeId,
                                               collegeName = a.collegeName,
                                               collegeLocation = a.collegeLocation,
                                               affiliatedUniversityId = a.affiliatedUniversityId,
                                               otherUniversityName = a.otherUniversityName,
                                               jntuh_college = a.jntuh_college,
                                               jntuh_university = a.jntuh_university,
                                               my_aspnet_users = a.my_aspnet_users,
                                               my_aspnet_users1 = a.my_aspnet_users1,
                                               universityName = a.jntuh_university.universityName,
                                               yearOfEstablishment = a.yearOfEstablishment
                                           }).ToList();
            return View();
        }

        [HttpGet]
        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult AddEditRecord(int? id, string collegeId)
        {
            ViewBag.University = db.jntuh_university.Where(u => u.isActive == true).Select(u => u).ToList();
            if (Request.IsAjaxRequest())
            {
                if (id != null)
                {
                    ViewBag.IsUpdate = true;
                    OtherCollegesSameSociety otherCollegesSameSociety = db.jntuh_society_other_locations_colleges.Where(oc => oc.id == id).Select(a =>
                                              new OtherCollegesSameSociety
                                              {
                                                  id = a.id,
                                                  collegeId = a.collegeId,
                                                  collegeName = a.collegeName,
                                                  collegeLocation=a.collegeLocation,
                                                  affiliatedUniversityId = a.affiliatedUniversityId,
                                                  otherUniversityName = a.otherUniversityName,
                                                  createdBy = a.createdBy,
                                                  createdOn = a.createdOn,
                                                  updatedBy = a.updatedBy,
                                                  updatedOn = a.updatedOn,
                                                  jntuh_college = a.jntuh_college,
                                                  jntuh_university = a.jntuh_university,
                                                  my_aspnet_users = a.my_aspnet_users,
                                                  my_aspnet_users1 = a.my_aspnet_users1,
                                                  yearOfEstablishment = a.yearOfEstablishment
                                              }).FirstOrDefault();
                    return PartialView("_OtherCollegesSameSociety", otherCollegesSameSociety);
                }
                else
                {
                    OtherCollegesSameSociety otherCollegesSameSociety = new OtherCollegesSameSociety();
                    if (collegeId != null)
                    {
                        int userCollegeID = Convert.ToInt32(Utilities.DecryptString(collegeId, WebConfigurationManager.AppSettings["CryptoKey"]));
                        otherCollegesSameSociety.collegeId = userCollegeID;
                    }
                    ViewBag.IsUpdate = false;
                    return PartialView("_OtherCollegesSameSociety", otherCollegesSameSociety);
                }
            }
            else
            {
                if (id != null)
                {
                    ViewBag.IsUpdate = true;
                    OtherCollegesSameSociety otherCollegesSameSociety = db.jntuh_society_other_locations_colleges.Where(oc => oc.id == id).Select(a =>
                                              new OtherCollegesSameSociety
                                              {
                                                  id = a.id,
                                                  collegeId = a.collegeId,
                                                  collegeName = a.collegeName,
                                                  collegeLocation=a.collegeLocation,
                                                  affiliatedUniversityId = a.affiliatedUniversityId,
                                                  otherUniversityName = a.otherUniversityName,
                                                  createdBy = a.createdBy,
                                                  createdOn = a.createdOn,
                                                  updatedBy = a.updatedBy,
                                                  updatedOn = a.updatedOn,
                                                  jntuh_college = a.jntuh_college,
                                                  jntuh_university = a.jntuh_university,
                                                  my_aspnet_users = a.my_aspnet_users,
                                                  my_aspnet_users1 = a.my_aspnet_users1,
                                                  yearOfEstablishment = a.yearOfEstablishment
                                              }).FirstOrDefault();
                    return View("OtherCollegesSameSociety", otherCollegesSameSociety);
                }
                else
                {
                    OtherCollegesSameSociety otherCollegesSameSociety = new OtherCollegesSameSociety();
                    if (collegeId != null)
                    {
                        int userCollegeID = Convert.ToInt32(Utilities.DecryptString(collegeId, WebConfigurationManager.AppSettings["CryptoKey"]));
                        otherCollegesSameSociety.collegeId = userCollegeID;
                    }
                    ViewBag.IsUpdate = false;
                    return View("OtherCollegesSameSociety", otherCollegesSameSociety);
                }
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult AddEditRecord(OtherCollegesSameSociety otherCollegesSameSociety, string cmd)
        {
            if (ModelState.IsValid)
            {
                int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
                int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
                if (userCollegeID == 0)
                {
                    userCollegeID = otherCollegesSameSociety.collegeId;
                }
                if (cmd == "Save")
                {
                    try
                    {
                        jntuh_society_other_locations_colleges jntuh_society_other_college = new jntuh_society_other_locations_colleges();
                        jntuh_society_other_college.id = otherCollegesSameSociety.id;
                        jntuh_society_other_college.collegeId = userCollegeID;
                        jntuh_society_other_college.collegeName = otherCollegesSameSociety.collegeName;
                        jntuh_society_other_college.collegeLocation = otherCollegesSameSociety.collegeLocation;
                        jntuh_society_other_college.affiliatedUniversityId = otherCollegesSameSociety.affiliatedUniversityId;
                        var otherUniversity = db.jntuh_university.Where(u => u.id == otherCollegesSameSociety.affiliatedUniversityId).Select(u => u.universityName).FirstOrDefault();
                        if (otherUniversity == "Other")
                        {
                            jntuh_society_other_college.otherUniversityName = otherCollegesSameSociety.otherUniversityName;
                        }
                        else
                        {
                            jntuh_society_other_college.otherUniversityName = null;
                        }
                        jntuh_society_other_college.isActive = true;
                        jntuh_society_other_college.createdBy = userID;
                        jntuh_society_other_college.createdOn = DateTime.Now;
                        jntuh_society_other_college.yearOfEstablishment = otherCollegesSameSociety.yearOfEstablishment;
                        db.jntuh_society_other_locations_colleges.Add(jntuh_society_other_college);
                        db.SaveChanges();
                        TempData["CollegesSameSocitySuccess"] = "Other College Same Socity Details are Added Successfully.";
                        return RedirectToAction("Index", "OtherColleges", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
                    }
                    catch { }
                }
                else
                {
                    try
                    {
                        jntuh_society_other_locations_colleges oCollege = new jntuh_society_other_locations_colleges();

                        if (oCollege != null)
                        {
                            oCollege.id = otherCollegesSameSociety.id;
                            oCollege.collegeId = userCollegeID;
                            oCollege.collegeName = otherCollegesSameSociety.collegeName;
                            oCollege.collegeLocation = otherCollegesSameSociety.collegeLocation;
                            oCollege.affiliatedUniversityId = otherCollegesSameSociety.affiliatedUniversityId;
                            var otherUniversity = db.jntuh_university.Where(u => u.id == otherCollegesSameSociety.affiliatedUniversityId).Select(u => u.universityName).FirstOrDefault();
                            if (otherUniversity == "Other")
                            {
                                oCollege.otherUniversityName = otherCollegesSameSociety.otherUniversityName;
                            }
                            else
                            {
                                oCollege.otherUniversityName = null;
                            }
                            oCollege.createdBy = otherCollegesSameSociety.createdBy;
                            oCollege.createdOn = otherCollegesSameSociety.createdOn;
                            oCollege.updatedBy = userID;
                            oCollege.updatedOn = DateTime.Now;
                            oCollege.yearOfEstablishment = otherCollegesSameSociety.yearOfEstablishment;
                            db.Entry(oCollege).State = EntityState.Modified;
                            db.SaveChanges();
                            TempData["CollegesSameSocitySuccess"] = "Other College Same Socity Details are Updated Successfully.";
                        }
                        return RedirectToAction("Index", "OtherColleges", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
                    }
                    catch { }
                }
            }

            if (Request.IsAjaxRequest())
            {
                return PartialView("_OtherCollegesSameSociety", otherCollegesSameSociety);
            }
            else
            {
                return View("OtherCollegesSameSociety", otherCollegesSameSociety);
            }
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult DeleteRecord(int id)
        {
            jntuh_society_other_locations_colleges otherCollegesSameSociety = db.jntuh_society_other_locations_colleges.Where(oc => oc.id == id).FirstOrDefault();
            int usercollegeId = db.jntuh_society_other_locations_colleges.Where(oc => oc.id == id).Select(oc => oc.collegeId).FirstOrDefault();
            if (otherCollegesSameSociety != null)
            {
                try
                {
                    db.jntuh_society_other_locations_colleges.Remove(otherCollegesSameSociety);
                    db.SaveChanges();
                    TempData["CollegesSameSocitySuccess"] = "Other College Same Socity Details are Deleted Successfully.";
                }
                catch { }
            }
            return RedirectToAction("Index", "OtherColleges", new { collegeId = Utilities.EncryptString(usercollegeId.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Details(int id)
        {
            OtherCollegesSameSociety otherCollegesSameSociety = db.jntuh_society_other_locations_colleges.Where(oc => oc.id == id).Select(a =>
                                              new OtherCollegesSameSociety
                                              {
                                                  id = a.id,
                                                  collegeId = a.collegeId,
                                                  collegeName = a.collegeName,
                                                  collegeLocation = a.collegeLocation,
                                                  affiliatedUniversityId = a.affiliatedUniversityId,
                                                  otherUniversityName = a.otherUniversityName,
                                                  jntuh_college = a.jntuh_college,
                                                  jntuh_university = a.jntuh_university,
                                                  my_aspnet_users = a.my_aspnet_users,
                                                  my_aspnet_users1 = a.my_aspnet_users1,
                                                  yearOfEstablishment = a.yearOfEstablishment
                                              }).FirstOrDefault();
            if (otherCollegesSameSociety != null)
            {
                if (Request.IsAjaxRequest())
                {
                    return PartialView("_OtherCollegesSameSocietyDetails", otherCollegesSameSociety);
                }
                else
                {
                    return View("OtherCollegesSameSocietyDetails", otherCollegesSameSociety);
                }
            }
            return RedirectToAction("Index", "OtherColleges", new { collegeId = Utilities.EncryptString(otherCollegesSameSociety.collegeId.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
        }

    }
}
