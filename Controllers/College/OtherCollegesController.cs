using System;
using System.Collections.Generic;
using System.Configuration;
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
    public class OtherCollegesController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Index(string collegeId)
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
            ViewBag.collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString();
            if (userCollegeID == 0)
            {
                return RedirectToAction("Create", "CollegeInformation");
            }
            List<OtherCollege> otherCollege = db.jntuh_society_other_colleges.Where(a => a.collegeId == userCollegeID).Select(a =>
                                              new OtherCollege
                                              {
                                                  id = a.id,
                                                  collegeId = a.collegeId,
                                                  collegeName = a.collegeName,
                                                  affiliatedUniversityId = a.affiliatedUniversityId,
                                                  otherUniversityName = a.otherUniversityName,
                                                  jntuh_college = a.jntuh_college,
                                                  jntuh_university = a.jntuh_university,
                                                  my_aspnet_users = a.my_aspnet_users,
                                                  my_aspnet_users1 = a.my_aspnet_users1,
                                                  universityName = a.jntuh_university.universityName,
                                                  yearOfEstablishment = a.yearOfEstablishment
                                              }).ToList();

            List<OtherCourse> otherCourse = db.jntuh_college_other_university_courses.Where(a => a.collegeId == userCollegeID).Select(a =>
                                              new OtherCourse
                                              {
                                                  id = a.id,
                                                  collegeId = a.collegeId,
                                                  courseName = a.courseName,
                                                  affiliatedUniversityId = a.affiliatedUniversityId,
                                                  otherUniversityName = a.otherUniversityName,
                                                  jntuh_college = a.jntuh_college,
                                                  jntuh_university = a.jntuh_university,
                                                  my_aspnet_users = a.my_aspnet_users,
                                                  my_aspnet_users1 = a.my_aspnet_users1,
                                                  universityName = a.jntuh_university.universityName
                                              }).ToList();


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

            ViewBag.Colleges = otherCollege;
            ViewBag.Courses = otherCourse;
            ViewBag.otherCollegesSameSociety = otherCollegesSameSociety;
            ViewBag.CollegeCount = otherCollege.Count();
            ViewBag.CourseCount = otherCourse.Count();
            ViewBag.otherCollegesSameSocietyCount = otherCollegesSameSociety.Count();
            ViewBag.University = db.jntuh_university.Where(u => u.isActive == true).Select(u => u).ToList();
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
            if (otherCollege.Count() == 0 && status == 0 && Roles.IsUserInRole("College"))
            {
                ViewBag.OtherCollegeNotUpload = true;
            }
            else
            {
                ViewBag.OtherCollegeNotUpload = false;
            }

            if (otherCourse.Count() == 0 && status == 0 && Roles.IsUserInRole("College"))
            {
                ViewBag.OtherCourseNotUpload = true;
            }
            else
            {
                ViewBag.OtherCourseNotUpload = false;
            }

            if (otherCollegesSameSociety.Count() == 0 && status == 0 && Roles.IsUserInRole("College"))
            {
                ViewBag.otherCollegesSameSocietyNotUpload = true;
            }
            else
            {
                ViewBag.otherCollegesSameSocietyNotUpload = false;
            }

            ////RAMESH:To-DisableEdit
            //if (ConfigurationManager.AppSettings["ShouldFormsBeDisabled"].ToString() == "true")
            //{
            //    ViewBag.IsEditable = false;
            //}
            //else
            //{
            //    ViewBag.IsEditable = true;
            //}

            if (status == 0 && Roles.IsUserInRole("College") && (otherCollege.Count() > 0 || otherCourse.Count() > 0 || otherCollegesSameSociety.Count() > 0))
            {
                return RedirectToAction("View", "OtherColleges");
            }

            bool isPageEditable = db.jntuh_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("OO") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

            if (!isPageEditable)
            {
                return RedirectToAction("View", "OtherColleges");
            }
            
            return View();

        }

        [HttpGet]
        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult AddEditRecord(int? id, string collegeId)
        {
            ////RAMESH:To-DisableEdit
            //if (ConfigurationManager.AppSettings["ShouldFormsBeDisabled"].ToString() == "true")
            //{
            //    ViewBag.IsEditable = false;
            //    return RedirectToAction("Index", "OtherColleges");
            //}
            //else
            //{
            //    ViewBag.IsEditable = true;
            //}
            ViewBag.University = db.jntuh_university.Where(u => u.isActive == true).Select(u => u).ToList();
            if (Request.IsAjaxRequest())
            {
                if (id != null)
                {
                    ViewBag.IsUpdate = true;
                    OtherCollege otherCollege = db.jntuh_society_other_colleges.Where(oc => oc.id == id).Select(a =>
                                              new OtherCollege
                                              {
                                                  id = a.id,
                                                  collegeId = a.collegeId,
                                                  collegeName = a.collegeName,
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
                    return PartialView("_OtherCollegeData", otherCollege);
                }
                else
                {
                    OtherCollege otherCollege = new OtherCollege();
                    if (collegeId != null)
                    {
                        int userCollegeID = Convert.ToInt32(Utilities.DecryptString(collegeId, WebConfigurationManager.AppSettings["CryptoKey"]));
                        otherCollege.collegeId = userCollegeID;
                    }
                    ViewBag.IsUpdate = false;
                    return PartialView("_OtherCollegeData", otherCollege);
                }
            }
            else
            {
                if (id != null)
                {
                    ViewBag.IsUpdate = true;
                    OtherCollege otherCollege = db.jntuh_society_other_colleges.Where(oc => oc.id == id).Select(a =>
                                              new OtherCollege
                                              {
                                                  id = a.id,
                                                  collegeId = a.collegeId,
                                                  collegeName = a.collegeName,
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
                    return View("OtherCollegeData", otherCollege);
                }
                else
                {
                    OtherCollege otherCollege = new OtherCollege();
                    if (collegeId != null)
                    {
                        int userCollegeID = Convert.ToInt32(Utilities.DecryptString(collegeId, WebConfigurationManager.AppSettings["CryptoKey"]));
                        otherCollege.collegeId = userCollegeID;
                    }
                    ViewBag.IsUpdate = false;
                    return View("OtherCollegeData", otherCollege);
                }
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult AddEditRecord(OtherCollege otherCollege, string cmd)
        {
            if (ModelState.IsValid)
            {
                int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
                int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
                 ViewBag.University = db.jntuh_university.Where(u => u.isActive == true).Select(u => u).ToList();
                if (userCollegeID == 0)
                {
                    userCollegeID = otherCollege.collegeId;
                }
                if (cmd == "Save")
                {
                    try
                    {
                        jntuh_society_other_colleges jntuh_society_other_college = new jntuh_society_other_colleges();
                        jntuh_society_other_college.id = otherCollege.id;
                        jntuh_society_other_college.collegeId = userCollegeID;
                        jntuh_society_other_college.collegeName = otherCollege.collegeName;
                        jntuh_society_other_college.affiliatedUniversityId = otherCollege.affiliatedUniversityId;
                        var otherUniversity = db.jntuh_university.Where(u => u.id == otherCollege.affiliatedUniversityId).Select(u => u.universityName).FirstOrDefault();
                        if (otherUniversity == "Other")
                        {
                            jntuh_society_other_college.otherUniversityName = otherCollege.otherUniversityName;
                        }
                        else
                        {
                            jntuh_society_other_college.otherUniversityName = null;
                        }
                        jntuh_society_other_college.isActive = true;
                        jntuh_society_other_college.createdBy = userID;
                        jntuh_society_other_college.createdOn = DateTime.Now;
                        jntuh_society_other_college.yearOfEstablishment = otherCollege.yearOfEstablishment;
                        db.jntuh_society_other_colleges.Add(jntuh_society_other_college);
                        db.SaveChanges();
                        TempData["CollegesSuccess"] = "Other College Details are Added Successfully.";
                        //return RedirectToAction("Index");
                        return RedirectToAction("Index", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
                       
                    }
                    catch { }
                }
                else
                {
                    try
                    {
                        jntuh_society_other_colleges oCollege = new jntuh_society_other_colleges();
                        oCollege = db.jntuh_society_other_colleges.Where(c => c.id == otherCollege.id).Select(c => c).FirstOrDefault();

                        if (oCollege != null)
                        {
                            oCollege.id = otherCollege.id;
                            oCollege.collegeId = userCollegeID;
                            oCollege.collegeName = otherCollege.collegeName;
                            oCollege.affiliatedUniversityId = otherCollege.affiliatedUniversityId;
                            String otherUniversity = db.jntuh_university.Where(u => u.id == otherCollege.affiliatedUniversityId).Select(u => u.universityName).FirstOrDefault();
                            if (otherUniversity == "Other")
                            {
                                oCollege.otherUniversityName = otherCollege.otherUniversityName;
                                //oCollege.otherUniversityName = otherUniversity;
                            }
                            else
                            {
                                oCollege.otherUniversityName =null;
                            }
                            oCollege.createdBy = otherCollege.createdBy;
                            oCollege.createdOn = otherCollege.createdOn;
                            oCollege.updatedBy = userID;
                            oCollege.updatedOn = DateTime.Now;
                            oCollege.yearOfEstablishment = otherCollege.yearOfEstablishment;
                            db.Entry(oCollege).State = EntityState.Modified;
                            db.SaveChanges();
                            TempData["CollegesSuccess"] = "Other College Details are Updated Successfully.";

                        }
                        //return RedirectToAction("Index");
                        return RedirectToAction("Index", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
                    }
                    catch (Exception ex )
                    {
                        
                    }
                }
            }

            if (Request.IsAjaxRequest())
            {
                return PartialView("_OtherCollegeData", otherCollege);
            }
            else
            {
                return View("OtherCollegeData", otherCollege);
            }
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult DeleteRecord(int id)
        {
            jntuh_society_other_colleges otherCollege = db.jntuh_society_other_colleges.Where(oc => oc.id == id).FirstOrDefault();
            int usercollegeId = db.jntuh_society_other_colleges.Where(oc => oc.id == id).Select(oc => oc.collegeId).FirstOrDefault();
            if (otherCollege != null)
            {
                try
                {
                    db.jntuh_society_other_colleges.Remove(otherCollege);
                    db.SaveChanges();
                    TempData["CollegesSuccess"] = "Other College Details are Deleted Successfully.";
                }
                catch { }
            }
            return RedirectToAction("Index", new { collegeId = Utilities.EncryptString(usercollegeId.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Details(int id)
        {
            OtherCollege otherCollege = db.jntuh_society_other_colleges.Where(oc => oc.id == id).Select(a =>
                                              new OtherCollege
                                              {
                                                  id = a.id,
                                                  collegeId = a.collegeId,
                                                  collegeName = a.collegeName,
                                                  affiliatedUniversityId = a.affiliatedUniversityId,
                                                  otherUniversityName = a.otherUniversityName,
                                                  jntuh_college = a.jntuh_college,
                                                  jntuh_university = a.jntuh_university,
                                                  my_aspnet_users = a.my_aspnet_users,
                                                  my_aspnet_users1 = a.my_aspnet_users1,
                                                  yearOfEstablishment = a.yearOfEstablishment
                                              }).FirstOrDefault();
            if (otherCollege != null)
            {
                if (Request.IsAjaxRequest())
                {
                    return PartialView("_OtherCollegeDetails", otherCollege);
                }
                else
                {
                    return View("OtherCollegeDetails", otherCollege);
                }
            }
            return RedirectToAction("Index", new { collegeId = Utilities.EncryptString(otherCollege.collegeId.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
        }

        [Authorize(Roles = "Admin,College")]
        public ActionResult View(string id)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();

            if (userCollegeID == 0)
            {
                userCollegeID = userCollegeID = Convert.ToInt32(Utilities.DecryptString(id, WebConfigurationManager.AppSettings["CryptoKey"]));
                //return RedirectToAction("Create", "CollegeInformation");
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
            if (status > 0 && Roles.IsUserInRole("College"))
            {
                
                bool isPageEditable = db.jntuh_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("OO") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

                if (isPageEditable)
                {
                    return RedirectToAction("Index");
                }
            }
            List<OtherCollege> otherCollege = db.jntuh_society_other_colleges.Where(a => a.collegeId == userCollegeID).Select(a =>
                                              new OtherCollege
                                              {
                                                  id = a.id,
                                                  collegeId = a.collegeId,
                                                  collegeName = a.collegeName,
                                                  affiliatedUniversityId = a.affiliatedUniversityId,
                                                  otherUniversityName = a.otherUniversityName,
                                                  jntuh_college = a.jntuh_college,
                                                  jntuh_university = a.jntuh_university,
                                                  my_aspnet_users = a.my_aspnet_users,
                                                  my_aspnet_users1 = a.my_aspnet_users1,
                                                  universityName = a.jntuh_university.universityName,
                                                  yearOfEstablishment = a.yearOfEstablishment
                                              }).ToList();

            List<OtherCourse> otherCourse = db.jntuh_college_other_university_courses.Where(a => a.collegeId == userCollegeID).Select(a =>
                                              new OtherCourse
                                              {
                                                  id = a.id,
                                                  collegeId = a.collegeId,
                                                  courseName = a.courseName,
                                                  affiliatedUniversityId = a.affiliatedUniversityId,
                                                  otherUniversityName = a.otherUniversityName,
                                                  jntuh_college = a.jntuh_college,
                                                  jntuh_university = a.jntuh_university,
                                                  my_aspnet_users = a.my_aspnet_users,
                                                  my_aspnet_users1 = a.my_aspnet_users1,
                                                  universityName = a.jntuh_university.universityName
                                              }).ToList();

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

            ViewBag.Colleges = otherCollege;
            ViewBag.Courses = otherCourse;
            ViewBag.otherCollegesSameSociety = otherCollegesSameSociety;
            ViewBag.CollegeCount = otherCollege.Count();
            ViewBag.CourseCount = otherCourse.Count();
            ViewBag.otherCollegesSameSocietyCount = otherCollegesSameSociety.Count();
            return View();
        }

        public ActionResult UserView(string id)
        {
            int userCollegeID = Convert.ToInt32(Utilities.DecryptString(id, WebConfigurationManager.AppSettings["CryptoKey"]));

            List<OtherCollege> otherCollege = db.jntuh_society_other_colleges.Where(a => a.collegeId == userCollegeID).Select(a =>
                                              new OtherCollege
                                              {
                                                  id = a.id,
                                                  collegeId = a.collegeId,
                                                  collegeName = a.collegeName,
                                                  affiliatedUniversityId = a.affiliatedUniversityId,
                                                  otherUniversityName = a.otherUniversityName,
                                                  jntuh_college = a.jntuh_college,
                                                  jntuh_university = a.jntuh_university,
                                                  my_aspnet_users = a.my_aspnet_users,
                                                  my_aspnet_users1 = a.my_aspnet_users1,
                                                  universityName = a.jntuh_university.universityName,
                                                  yearOfEstablishment = a.yearOfEstablishment
                                              }).ToList();

            List<OtherCourse> otherCourse = db.jntuh_college_other_university_courses.Where(a => a.collegeId == userCollegeID).Select(a =>
                                              new OtherCourse
                                              {
                                                  id = a.id,
                                                  collegeId = a.collegeId,
                                                  courseName = a.courseName,
                                                  affiliatedUniversityId = a.affiliatedUniversityId,
                                                  otherUniversityName = a.otherUniversityName,
                                                  jntuh_college = a.jntuh_college,
                                                  jntuh_university = a.jntuh_university,
                                                  my_aspnet_users = a.my_aspnet_users,
                                                  my_aspnet_users1 = a.my_aspnet_users1,
                                                  universityName = a.jntuh_university.universityName
                                              }).ToList();

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

            ViewBag.Colleges = otherCollege;
            ViewBag.Courses = otherCourse;
            ViewBag.otherCollegesSameSociety = otherCollegesSameSociety;
            ViewBag.CollegeCount = otherCollege.Count();
            ViewBag.CourseCount = otherCourse.Count();
            ViewBag.otherCollegesSameSocietyCount = otherCollegesSameSociety.Count();
            return View();
        }
    }
}
