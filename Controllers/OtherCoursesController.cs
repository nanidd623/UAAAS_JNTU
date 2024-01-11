using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using UAAAS.Models;

namespace UAAAS.Controllers
{
    [ErrorHandling]
    public class OtherCoursesController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();

        [Authorize(Roles = "Admin, SuperAdmin, College")]
        public ActionResult Index()
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();

            List<OtherCollege> otherCollege = db.jntuh_society_other_colleges.Where(a => a.collegeId == userCollegeID).Select(a =>
                                              new OtherCollege
                                              {
                                                  id = a.id,
                                                  //rowNumber = index + 1,
                                                  collegeId = a.collegeId,
                                                  collegeName = a.collegeName,
                                                  affiliatedUniversityId = a.affiliatedUniversityId,
                                                  otherUniversityName = a.otherUniversityName,
                                                  isActive = a.isActive,
                                                  jntuh_college = a.jntuh_college,
                                                  jntuh_university = a.jntuh_university,
                                                  my_aspnet_users = a.my_aspnet_users,
                                                  my_aspnet_users1 = a.my_aspnet_users1,
                                                  universityName = a.jntuh_university.universityName
                                              }).ToList();
            return View(otherCollege);
        }

        //[HttpGet]
        [Authorize(Roles = "Admin, SuperAdmin, College")]
        public ActionResult AddEditRecord(int? id)
        {
            ViewBag.University = db.jntuh_university.Where(u => u.isActive == true).Select(u => u).ToList();

            if (Request.IsAjaxRequest())
            {
                if (id != null)
                {
                    ViewBag.IsUpdate = true;
                    OtherCourse otherCourse = db.jntuh_college_other_university_courses.Where(oc => oc.id == id).Select(a =>
                                              new OtherCourse
                                              {
                                                  id = a.id,
                                                  collegeId = a.collegeId,
                                                  courseName = a.courseName,
                                                  affiliatedUniversityId = a.affiliatedUniversityId,
                                                  otherUniversityName = a.otherUniversityName,
                                                  isActive = a.isActive,
                                                  createdBy = a.createdBy,
                                                  createdOn = a.createdOn,
                                                  updatedBy = a.updatedBy,
                                                  updatedOn = a.updatedOn,
                                                  jntuh_college = a.jntuh_college,
                                                  jntuh_university = a.jntuh_university,
                                                  my_aspnet_users = a.my_aspnet_users,
                                                  my_aspnet_users1 = a.my_aspnet_users1
                                              }).FirstOrDefault();
                    return PartialView("_OtherCourseData", otherCourse);
                }
                ViewBag.IsUpdate = false;
                return PartialView("_OtherCourseData");
            }
            else
            {
                if (id != null)
                {
                    ViewBag.IsUpdate = true;
                    OtherCourse otherCourse = db.jntuh_college_other_university_courses.Where(oc => oc.id == id).Select(a =>
                                              new OtherCourse
                                              {
                                                  id = a.id,
                                                  collegeId = a.collegeId,
                                                  courseName = a.courseName,
                                                  affiliatedUniversityId = a.affiliatedUniversityId,
                                                  otherUniversityName = a.otherUniversityName,
                                                  isActive = a.isActive,
                                                  createdBy = a.createdBy,
                                                  createdOn = a.createdOn,
                                                  updatedBy = a.updatedBy,
                                                  updatedOn = a.updatedOn,
                                                  jntuh_college = a.jntuh_college,
                                                  jntuh_university = a.jntuh_university,
                                                  my_aspnet_users = a.my_aspnet_users,
                                                  my_aspnet_users1 = a.my_aspnet_users1
                                              }).FirstOrDefault();
                    return View("OtherCourseData", otherCourse);
                }
                ViewBag.IsUpdate = false;
                return View("OtherCourseData");
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin, SuperAdmin, College")]
        public ActionResult AddEditRecord(OtherCourse otherCourse, string cmd)
        {
            if (ModelState.IsValid)
            {
                int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
                int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();

                if (cmd == "Save")
                {
                    try
                    {
                        jntuh_college_other_university_courses jntuh_college_other_university_course = new jntuh_college_other_university_courses();
                        jntuh_college_other_university_course.id = otherCourse.id;
                        jntuh_college_other_university_course.collegeId = userCollegeID;
                        jntuh_college_other_university_course.courseName = otherCourse.courseName;
                        jntuh_college_other_university_course.affiliatedUniversityId = otherCourse.affiliatedUniversityId;
                        jntuh_college_other_university_course.otherUniversityName = otherCourse.otherUniversityName;
                        jntuh_college_other_university_course.isActive = otherCourse.isActive;
                        jntuh_college_other_university_course.createdBy = userID;
                        jntuh_college_other_university_course.createdOn = DateTime.Now;

                        db.jntuh_college_other_university_courses.Add(jntuh_college_other_university_course);
                        db.SaveChanges();
                        return RedirectToAction("Index", "OtherColleges");
                    }
                    catch { }
                }
                else
                {
                    try
                    {
                        jntuh_college_other_university_courses oCourse = new jntuh_college_other_university_courses();

                        if (oCourse != null)
                        {
                            oCourse.id = otherCourse.id;
                            oCourse.collegeId = userCollegeID;
                            oCourse.courseName = otherCourse.courseName;
                            oCourse.affiliatedUniversityId = otherCourse.affiliatedUniversityId;
                            oCourse.otherUniversityName = otherCourse.otherUniversityName;
                            oCourse.isActive = otherCourse.isActive;
                            oCourse.createdBy = otherCourse.createdBy;
                            oCourse.createdOn = otherCourse.createdOn;
                            oCourse.updatedBy = userID;
                            oCourse.updatedOn = DateTime.Now;
                            db.Entry(oCourse).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                        return RedirectToAction("Index", "OtherColleges");
                    }
                    catch { }
                }
            }

            if (Request.IsAjaxRequest())
            {
                return PartialView("_OtherCourseData", otherCourse);
            }
            else
            {
                return View("OtherCourseData", otherCourse);
            }
        }

        [Authorize(Roles = "Admin, SuperAdmin, College")]
        public ActionResult DeleteRecord(int id)
        {
            jntuh_college_other_university_courses otherCourse = db.jntuh_college_other_university_courses.Where(oc => oc.id == id).FirstOrDefault();
            if (otherCourse != null)
            {
                try
                {
                    db.jntuh_college_other_university_courses.Remove(otherCourse);
                    db.SaveChanges();
                }
                catch { }
            }
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin, SuperAdmin, College")]
        public ActionResult Details(int id)
        {
            OtherCourse otherCourse = db.jntuh_college_other_university_courses.Where(oc => oc.id == id).Select(a =>
                                              new OtherCourse
                                              {
                                                  id = a.id,
                                                  collegeId = a.collegeId,
                                                  courseName = a.courseName,
                                                  affiliatedUniversityId = a.affiliatedUniversityId,
                                                  otherUniversityName = a.otherUniversityName,
                                                  isActive = a.isActive,
                                                  jntuh_college = a.jntuh_college,
                                                  jntuh_university = a.jntuh_university,
                                                  my_aspnet_users = a.my_aspnet_users,
                                                  my_aspnet_users1 = a.my_aspnet_users1
                                              }).FirstOrDefault();
            if (otherCourse != null)
            {
                if (Request.IsAjaxRequest())
                {
                    return PartialView("_OtherCourseDetails", otherCourse);
                }
                else
                {
                    return View("OtherCourseDetails", otherCourse);
                }
            }
            return View("Index");
        }

    }
}
