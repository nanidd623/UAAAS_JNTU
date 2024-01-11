using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using UAAAS.Models;
using System.Web.Security;
using System.Data;
using System.Web.Configuration;
using System.IO;
using System.Configuration;

namespace UAAAS.Controllers
{
    [ErrorHandling]
    public class CollegeInternetBandwidthController : BaseController
    {       
        private uaaasDBContext db = new uaaasDBContext();

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Create(string collegeId)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            
            if (userCollegeID == 0 && collegeId!=null)
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
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            int[] degree = db.jntuh_college_degree.Where(d => d.isActive == true && d.collegeId == userCollegeID).Select(d => d.degreeId).ToArray();
            int internetSpeedId = db.jntuh_college_internet_bandwidth.Where(a => a.collegeId == userCollegeID && degree.Contains(a.degreeId)).Select(a => a.id).FirstOrDefault();
            DateTime todayDate = DateTime.Now.Date;
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy =
                db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            int status = db.jntuh_college_edit_status.Where(editStatus => editStatus.academicyearId == prAy && editStatus.academicyearId == prAy && editStatus.collegeId == userCollegeID &&
                                                                                      editStatus.IsCollegeEditable == true &&
                                                                                      editStatus.editFromDate <= todayDate &&
                                                                                      editStatus.editToDate >= todayDate)
                                                                 .Select(editStatus => editStatus.id)
                                                                 .FirstOrDefault();

            if (userCollegeID > 0 && internetSpeedId > 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("View", "CollegeInternetBandwidth");
            }

            if (userCollegeID > 0 && internetSpeedId > 0 && (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry")))
            {
                return RedirectToAction("Edit", "CollegeInternetBandwidth", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
            }

            if (internetSpeedId == 0 && status == 0 && Roles.IsUserInRole("College"))
            {
                ViewBag.NotUpload = true;
            }
            else
            {
                ViewBag.NotUpload = false;
            }

            bool isPageEditable = db.jntuh_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("IB") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

            if (isPageEditable)
            {
                ViewBag.IsEditable = true;
            }
            else
            {
                ViewBag.IsEditable = false;
                return RedirectToAction("View", "CollegeInternetBandwidth");
            }

            //List<jntuh_college_degree> collegeDegree = db.jntuh_college_degree.Where(d => d.collegeId == userCollegeID && d.isActive == true).ToList();

            var cSpcIds =
                db.jntuh_college_intake_existing.Where(i => i.collegeId == userCollegeID && (i.academicYearId == prAy - 1 || i.academicYearId == prAy - 2 || i.academicYearId == prAy - 3) && (i.approvedIntake != 0 || i.aicteApprovedIntake != 0))
                //.GroupBy(r => new { r.specializationId })
                    .Select(s => s.specializationId)
                    .ToList();

            var DepartmentsData = db.jntuh_specialization.Where(e => e.isActive == true && cSpcIds.Contains(e.id)).Select(e => e.departmentId).Distinct().ToArray();

            var DegreeIds = db.jntuh_department.Where(e => e.isActive == true && DepartmentsData.Contains(e.id)).Select(e => e.degreeId).Distinct().ToList();

            int enclosureId =
                db.jntuh_enclosures.Where(e => e.documentName == "Internet bandwidth certificate")
                    .Select(e => e.id)
                    .FirstOrDefault();
            var InternetbandwidthLetter =
                db.jntuh_college_enclosures.Where(
                    e => e.enclosureId == enclosureId && e.collegeID == userCollegeID && e.academicyearId == prAy)
                    .OrderByDescending(a => a.id)
                    .Select(e => e.path)
                    .FirstOrDefault();
            ViewBag.InternetbandwidthLetter = InternetbandwidthLetter;
            List<InternetBandwidthDetails> internetBandwidthDetails = new List<InternetBandwidthDetails>();

            foreach (var item in DegreeIds)
            {
                InternetBandwidthDetails details = new InternetBandwidthDetails();
                details.id = 0;
                details.degreeId = item;
                details.degree = db.jntuh_degree.Where(d => d.isActive == true && d.id == item).Select(d => d.degree).FirstOrDefault();
                details.totalIntake = GetIntake(item, userCollegeID);
                details.availableInternetSpeed = 0;
                details.collegeId = userCollegeID;
                internetBandwidthDetails.Add(details);
            }
            ViewBag.Count = internetBandwidthDetails.Count();
            return View(internetBandwidthDetails);
        }

        private int GetIntake(int degreeId, int collegeId)
        {
            int totalIntake = 0;
            int duration = Convert.ToInt32(db.jntuh_degree.Where(d => d.id == degreeId).Select(d => d.degreeDuration).FirstOrDefault());
            int presentAcademicYearId = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.id).FirstOrDefault();
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.id == presentAcademicYearId).Select(a => a.actualYear).FirstOrDefault();
            int AcademicYearId1 = db.jntuh_academic_year.Where(a => a.isActive == true && a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            int AcademicYearId2 = db.jntuh_academic_year.Where(a => a.isActive == true && a.actualYear == (actualYear - 1)).Select(a => a.id).FirstOrDefault();
            int AcademicYearId3 = db.jntuh_academic_year.Where(a => a.isActive == true && a.actualYear == (actualYear - 2)).Select(a => a.id).FirstOrDefault();
            int AcademicYearId4 = db.jntuh_academic_year.Where(a => a.isActive == true && a.actualYear == (actualYear - 3)).Select(a => a.id).FirstOrDefault();
            int[] specializationsId = (from d in db.jntuh_college_degree
                                       join de in db.jntuh_department on d.degreeId equals de.degreeId
                                       join s in db.jntuh_specialization on de.id equals s.departmentId
                                       join ProposedIntakeExisting in db.jntuh_college_intake_proposed on s.id equals ProposedIntakeExisting.specializationId
                                       where (d.degreeId == degreeId && d.isActive == true && d.collegeId == collegeId && ProposedIntakeExisting.collegeId == collegeId)
                                       select ProposedIntakeExisting.specializationId).Distinct().ToArray();
            //int[] specializations = specializationsId.Distinct().ToArray();
            foreach (var specializationId in specializationsId)
            {
                int totalIntake1 = 0;
                int totalIntake2 = 0;
                int totalIntake3 = 0;
                int totalIntake4 = 0;
                int totalIntake5 = 0;
                int[] shiftId1 = db.jntuh_college_intake_proposed.Where(e => e.collegeId == collegeId && e.specializationId == specializationId && e.academicYearId == AcademicYearId1).Select(e => e.shiftId).ToArray();
                foreach (var sId1 in shiftId1)
                {
                    totalIntake1 += db.jntuh_college_intake_proposed.Where(e => e.academicYearId == AcademicYearId1 && e.collegeId == collegeId && e.specializationId == specializationId && e.shiftId == sId1).Select(a => a.proposedIntake).FirstOrDefault();
                    totalIntake2 += db.jntuh_college_intake_existing.Where(e => e.academicYearId == presentAcademicYearId && e.collegeId == collegeId && e.specializationId == specializationId && e.shiftId == sId1).Select(a => a.approvedIntake).FirstOrDefault();
                    totalIntake3 += db.jntuh_college_intake_existing.Where(e => e.academicYearId == AcademicYearId2 && e.collegeId == collegeId && e.specializationId == specializationId && e.shiftId == sId1).Select(a => a.approvedIntake).FirstOrDefault();
                    totalIntake4 += db.jntuh_college_intake_existing.Where(e => e.academicYearId == AcademicYearId3 && e.collegeId == collegeId && e.specializationId == specializationId && e.shiftId == sId1).Select(a => a.approvedIntake).FirstOrDefault();
                    totalIntake5 += db.jntuh_college_intake_existing.Where(e => e.academicYearId == AcademicYearId4 && e.collegeId == collegeId && e.specializationId == specializationId && e.shiftId == sId1).Select(a => a.approvedIntake).FirstOrDefault();
                }
                if (duration >= 5)
                {
                    totalIntake += totalIntake1 + totalIntake2 + totalIntake3 + totalIntake4 + totalIntake5;
                }
                if (duration == 4)
                {
                    totalIntake += totalIntake1 + totalIntake2 + totalIntake3 + totalIntake4;
                }
                if (duration == 3)
                {
                    totalIntake += totalIntake1 + totalIntake2 + totalIntake3;
                }
                if (duration == 2)
                {
                    totalIntake += totalIntake1 + totalIntake2;
                }
                if (duration == 1)
                {
                    totalIntake += totalIntake1;
                }
            }

            return totalIntake;
        }

        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Create(ICollection<InternetBandwidthDetails> internetBandwidthDetails)
        {
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int ay0 = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                foreach (var item in internetBandwidthDetails)
                {
                    userCollegeID = item.collegeId;
                }
            }
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            SaveInternetBandwidthDetails(internetBandwidthDetails);
           
            //List<jntuh_college_degree> collegeDegree = db.jntuh_college_degree.Where(d => d.collegeId == userCollegeID && d.isActive == true).ToList();

            var cSpcIds =
                db.jntuh_college_intake_existing.Where(i => i.collegeId == userCollegeID && (i.academicYearId == ay0 - 1 || i.academicYearId == ay0 - 2 || i.academicYearId == ay0 - 3) && (i.approvedIntake != 0 || i.aicteApprovedIntake != 0))
                //.GroupBy(r => new { r.specializationId })
                    .Select(s => s.specializationId)
                    .ToList();

            var DepartmentsData = db.jntuh_specialization.Where(e => e.isActive == true && cSpcIds.Contains(e.id)).Select(e => e.departmentId).Distinct().ToArray();

            var DegreeIds = db.jntuh_department.Where(e => e.isActive == true && DepartmentsData.Contains(e.id)).Select(e => e.degreeId).Distinct().ToList();

            List<InternetBandwidthDetails> internetDetails = new List<InternetBandwidthDetails>();

            foreach (var item in DegreeIds)
            {
                InternetBandwidthDetails details = new InternetBandwidthDetails();
                details.id = 0;
                details.degreeId = item;
                details.degree = db.jntuh_degree.Where(d => d.isActive == true && d.id == item).Select(d => d.degree).FirstOrDefault();
                details.totalIntake = GetIntake(item, userCollegeID);
                details.collegeId = userCollegeID;
                details.availableInternetSpeed = db.jntuh_college_internet_bandwidth.Where(internetbandwidth => internetbandwidth.collegeId == userCollegeID &&
                                                                                        internetbandwidth.degreeId == item)
                                                                                 .Select(internetbandwidth => internetbandwidth.availableInternetSpeed)
                                                                                 .FirstOrDefault();
                internetDetails.Add(details);
            }
            ViewBag.Count = internetDetails.Count();
            return View(internetDetails);
        }

        private void SaveInternetBandwidthDetails(ICollection<InternetBandwidthDetails> internetBandwidthDetails)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                foreach (var item in internetBandwidthDetails)
                {
                    userCollegeID = item.collegeId;
                }
            }
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            var message = string.Empty;
            if (ModelState.IsValid)
            {
                foreach (InternetBandwidthDetails item in internetBandwidthDetails)
                {
                    jntuh_college_internet_bandwidth details = new jntuh_college_internet_bandwidth();
                    details.degreeId = item.degreeId;
                    details.availableInternetSpeed = item.availableInternetSpeed;
                    details.collegeId = userCollegeID;

                    int existId = db.jntuh_college_internet_bandwidth.Where(InternetBandwidth => InternetBandwidth.collegeId == userCollegeID &&
                                                                            InternetBandwidth.degreeId == item.degreeId).Select(InternetBandwidth => InternetBandwidth.id).FirstOrDefault();

                    if (existId == 0)
                    {
                        details.createdBy = userID;
                        details.createdOn = DateTime.Now;
                        db.jntuh_college_internet_bandwidth.Add(details);
                        db.SaveChanges();
                        message = "Save";
                    }
                    else
                    {
                        details.id = existId; ;
                        details.createdOn = db.jntuh_college_internet_bandwidth.Where(d => d.id == existId).Select(d => d.createdOn).FirstOrDefault();
                        details.createdBy = db.jntuh_college_internet_bandwidth.Where(d => d.id == existId).Select(d => d.createdBy).FirstOrDefault();
                        details.updatedBy = userID;
                        details.updatedOn = DateTime.Now;
                        db.Entry(details).State = EntityState.Modified;
                        db.SaveChanges();
                        message = "Update";
                    }
                }
                if (message == "Update")
                {
                    TempData["Success"] = "Internet bandwidth Details are Updated successfully";
                }
                else
                {
                    TempData["Success"] = "Internet bandwidth Details are Added successfully";
                }
            }
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Edit(string collegeId)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            
            if (userCollegeID == 0 && collegeId!=null)
            {
                if (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry"))
                {
                    userCollegeID = Convert.ToInt32(Utilities.DecryptString(collegeId, WebConfigurationManager.AppSettings["CryptoKey"]));
                }
            }
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            int[] degree = db.jntuh_college_degree.Where(d => d.isActive == true && d.collegeId == userCollegeID).Select(d => d.degreeId).ToArray();
            int internetSpeedId = db.jntuh_college_internet_bandwidth.Where(a => a.collegeId == userCollegeID && degree.Contains(a.degreeId)).Select(a => a.id).FirstOrDefault();
            
            if (userCollegeID == 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("Create", "CollegeInformation");
            }

            if (internetSpeedId == 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("Create", "CollegeInternetBandwidth");
            }
            
            DateTime todayDate = DateTime.Now.Date;
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy =
                db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            int status = db.jntuh_college_edit_status.Where(editStatus => editStatus.academicyearId == prAy && editStatus.academicyearId == prAy && editStatus.collegeId == userCollegeID &&
                                                                                      editStatus.IsCollegeEditable == true &&
                                                                                      editStatus.editFromDate <= todayDate &&
                                                                                      editStatus.editToDate >= todayDate)
                                                                 .Select(editStatus => editStatus.id)
                                                                 .FirstOrDefault();
            if (status == 0 && Roles.IsUserInRole("College"))
            {
                ViewBag.IsEditable = false;
                return RedirectToAction("View", "CollegeInternetBandwidth");
            }
            else
            {
                bool isPageEditable = db.jntuh_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("IB") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

                if (isPageEditable)
                {
                    ViewBag.IsEditable = true;
                }
                else
                {
                    ViewBag.IsEditable = false;
                    return RedirectToAction("View", "CollegeInternetBandwidth");
                }
                //ViewBag.IsEditable = true;
            }

           
            //List<jntuh_college_degree> collegeDegree = db.jntuh_college_degree.Where(d => d.collegeId == userCollegeID && d.isActive == true).ToList();

            var cSpcIds =
                db.jntuh_college_intake_existing.Where(i => i.collegeId == userCollegeID && (i.academicYearId == prAy - 1 || i.academicYearId == prAy - 2 || i.academicYearId == prAy - 3) && (i.approvedIntake != 0 || i.aicteApprovedIntake != 0))
                //.GroupBy(r => new { r.specializationId })
                    .Select(s => s.specializationId)
                    .ToList();

            var DepartmentsData = db.jntuh_specialization.Where(e => e.isActive == true && cSpcIds.Contains(e.id)).Select(e => e.departmentId).Distinct().ToArray();

            var DegreeIds = db.jntuh_department.Where(e => e.isActive == true && DepartmentsData.Contains(e.id)).Select(e => e.degreeId).Distinct().ToList();

            List<InternetBandwidthDetails> internetDetails = new List<InternetBandwidthDetails>();

            int enclosureId =
                db.jntuh_enclosures.Where(e => e.documentName == "Internet bandwidth certificate")
                    .Select(e => e.id)
                    .FirstOrDefault();
            var InternetbandwidthLetter =
                db.jntuh_college_enclosures.Where(
                    e => e.enclosureId == enclosureId && e.collegeID == userCollegeID && e.academicyearId == prAy)
                    .OrderByDescending(a => a.id)
                    .Select(e => e.path)
                    .FirstOrDefault();
            ViewBag.InternetbandwidthLetter = InternetbandwidthLetter;

            foreach (var item in DegreeIds)
            {
                InternetBandwidthDetails details = new InternetBandwidthDetails();
                details.id = 0;
                details.degreeId = item;
                details.degree = db.jntuh_degree.Where(d => d.isActive == true && d.id == item).Select(d => d.degree).FirstOrDefault();
                details.totalIntake = GetIntake(item, userCollegeID);
                details.collegeId = userCollegeID;
                details.availableInternetSpeed = db.jntuh_college_internet_bandwidth.Where(internetbandwidth => internetbandwidth.collegeId == userCollegeID &&
                                                                                        internetbandwidth.degreeId == item)
                                                                                 .Select(internetbandwidth => internetbandwidth.availableInternetSpeed)
                                                                                 .FirstOrDefault();
                internetDetails.Add(details);
            }
            ViewBag.Count = internetDetails.Count();
            return View("Create",internetDetails);            
        }

        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Edit(ICollection<InternetBandwidthDetails> internetBandwidthDetails)
        {
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int ay0 = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                foreach (var item in internetBandwidthDetails)
                {
                    userCollegeID = item.collegeId;
                }
            }
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            SaveInternetBandwidthDetails(internetBandwidthDetails);           
            //List<jntuh_college_degree> collegeDegree = db.jntuh_college_degree.Where(d => d.collegeId == userCollegeID && d.isActive == true).ToList();

            var cSpcIds =
                db.jntuh_college_intake_existing.Where(i => i.collegeId == userCollegeID && (i.academicYearId == ay0 - 1 || i.academicYearId == ay0 - 2 || i.academicYearId == ay0 - 3) && (i.approvedIntake != 0 || i.aicteApprovedIntake != 0))
                //.GroupBy(r => new { r.specializationId })
                    .Select(s => s.specializationId)
                    .ToList();

            var DepartmentsData = db.jntuh_specialization.Where(e => e.isActive == true && cSpcIds.Contains(e.id)).Select(e => e.departmentId).Distinct().ToArray();

            var DegreeIds = db.jntuh_department.Where(e => e.isActive == true && DepartmentsData.Contains(e.id)).Select(e => e.degreeId).Distinct().ToList();

            int enclosureId =
                db.jntuh_enclosures.Where(e => e.documentName == "Internet bandwidth certificate")
                    .Select(e => e.id)
                    .FirstOrDefault();
            var InternetbandwidthLetter =
                db.jntuh_college_enclosures.Where(
                    e => e.enclosureId == enclosureId && e.collegeID == userCollegeID && e.academicyearId == ay0)
                    .OrderByDescending(a => a.id)
                    .Select(e => e.path)
                    .FirstOrDefault();
            ViewBag.InternetbandwidthLetter = InternetbandwidthLetter;

            List<InternetBandwidthDetails> internetDetails = new List<InternetBandwidthDetails>();

            foreach (var item in DegreeIds)
            {
                InternetBandwidthDetails details = new InternetBandwidthDetails();
                details.id = 0;
                details.degreeId = item;
                details.degree = db.jntuh_degree.Where(d => d.isActive == true && d.id == item).Select(d => d.degree).FirstOrDefault();
                details.totalIntake = GetIntake(item, userCollegeID);
                details.collegeId = userCollegeID;
                details.availableInternetSpeed = db.jntuh_college_internet_bandwidth.Where(internetbandwidth => internetbandwidth.collegeId == userCollegeID &&
                                                                                        internetbandwidth.degreeId == item)
                                                                                 .Select(internetbandwidth => internetbandwidth.availableInternetSpeed)
                                                                                 .FirstOrDefault();
                internetDetails.Add(details);
            }
            ViewBag.Count = internetDetails.Count();
            return View("View");
           // return View("Create", internetDetails); 
        }

        public ActionResult FileUpload(HttpPostedFileBase fileUploader, string collegeId)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID =
                db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            int actualYear =
                db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true)
                    .Select(a => a.actualYear)
                    .FirstOrDefault();
            int ay0 =
                db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            if (userCollegeID == 0)
            {
                userCollegeID = Convert.ToInt32(collegeId);
            }
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            string fileName = string.Empty;
            int presentAY =
                db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true)
                    .Select(a => a.actualYear)
                    .FirstOrDefault();
            int academicyearId =
                db.jntuh_academic_year.Where(a => a.actualYear == (presentAY + 1))
                    .Select(a => a.actualYear)
                    .FirstOrDefault();
            int enclosureId =
                db.jntuh_enclosures.Where(e => e.documentName == "Internet bandwidth certificate")
                    .Select(e => e.id)
                    .FirstOrDefault();
            var college_enclosures =
                db.jntuh_college_enclosures.Where(
                    e => e.enclosureId == enclosureId && e.academicyearId == ay0 && e.collegeID == userCollegeID)
                    .Select(e => e)
                    .FirstOrDefault();
            jntuh_college_enclosures jntuh_college_enclosures = new jntuh_college_enclosures();
            jntuh_college_enclosures.collegeID = userCollegeID;
            jntuh_college_enclosures.academicyearId = ay0;
            jntuh_college_enclosures.enclosureId = enclosureId;
            jntuh_college_enclosures.isActive = true;
            if (fileUploader != null)
            {
                string ext = Path.GetExtension(fileUploader.FileName);
                //DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" + faculty.FirstName.Substring(0, 1) + "-" + faculty.LastName.Substring(0, 1)
                fileName =
                    db.jntuh_college.Where(c => c.id == userCollegeID).Select(c => c.collegeCode).FirstOrDefault() +
                    "_APL_" + enclosureId + "-" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + ext;
                fileUploader.SaveAs(string.Format("{0}/{1}", Server.MapPath("~/Content/Upload/CollegeEnclosures/"),
                    fileName));
                jntuh_college_enclosures.path = fileName;
            }
            else if (!string.IsNullOrEmpty(college_enclosures.path))
            {
                fileName = college_enclosures.path;
                jntuh_college_enclosures.path = fileName;
            }

            if (college_enclosures == null)
            {
                jntuh_college_enclosures.createdBy = userID;
                jntuh_college_enclosures.createdOn = DateTime.Now;
                db.jntuh_college_enclosures.Add(jntuh_college_enclosures);
                db.SaveChanges();
            }
            else
            {
                college_enclosures.path = fileName;
                college_enclosures.updatedBy = userID;
                college_enclosures.updatedOn = DateTime.Now;
                db.Entry(college_enclosures).State = EntityState.Modified;
                db.SaveChanges();
            }
            return RedirectToAction("Create",
                new
                {
                    collegeId =
                        Utilities.EncryptString(userCollegeID.ToString(),
                            WebConfigurationManager.AppSettings["CryptoKey"]).ToString()
                });
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
            int[] degree = db.jntuh_college_degree.Where(d => d.isActive == true && d.collegeId == userCollegeID).Select(d => d.degreeId).ToArray();
            int internetSpeedId = db.jntuh_college_internet_bandwidth.Where(a => a.collegeId == userCollegeID && degree.Contains(a.degreeId)).Select(a => a.id).FirstOrDefault();
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
                bool isPageEditable = db.jntuh_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("IB") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

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

            int enclosureId =
                db.jntuh_enclosures.Where(e => e.documentName == "Internet bandwidth certificate")
                    .Select(e => e.id)
                    .FirstOrDefault();
            var InternetbandwidthLetter =
                db.jntuh_college_enclosures.Where(
                    e => e.enclosureId == enclosureId && e.collegeID == userCollegeID && e.academicyearId == prAy)
                    .OrderByDescending(a => a.id)
                    .Select(e => e.path)
                    .FirstOrDefault();
            ViewBag.InternetbandwidthLetter = InternetbandwidthLetter;

            //int academicYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.id).FirstOrDefault();
            //List<InternetBandwidthDetails> internetDetails = (from d in db.jntuh_college_degree
            //                                                  join di in db.jntuh_degree on d.degreeId equals di.id
            //                                                  orderby di.degree
            //                                                  where (d.collegeId == userCollegeID && d.isActive == true && di.isActive == true)
            //                                                  select new InternetBandwidthDetails
            //                                                  {
            //                                                      id = 0,
            //                                                      degreeId = d.degreeId,
            //                                                      degree = di.degree,
            //                                                      totalIntake = 0,
            //                                                      availableInternetSpeed = 0
            //                                                  }).ToList();
            //foreach (var item in internetDetails)
            //{
            //    int[] specializations = (from d in db.jntuh_college_degree
            //                             join de in db.jntuh_department on d.degreeId equals de.degreeId
            //                             join s in db.jntuh_specialization on de.id equals s.departmentId
            //                             where (d.collegeId == userCollegeID && d.degreeId == item.degreeId && d.isActive == true)
            //                             select s.id).ToArray();
            //    item.totalIntake = 0;
            //    foreach (var specializationId in specializations)
            //    {
            //        item.totalIntake += db.jntuh_college_intake_existing.Where(ei => ei.specializationId == specializationId &&
            //                                                                  ei.collegeId == userCollegeID &&
            //                                                                  ei.academicYearId == academicYear).Select(ei => ei.approvedIntake).FirstOrDefault();
            //    }
            //    item.availableInternetSpeed = db.jntuh_college_internet_bandwidth.Where(internetbandwidth => internetbandwidth.collegeId == userCollegeID &&
            //                                                                            internetbandwidth.degreeId == item.degreeId)
            //                                                                     .Select(internetbandwidth => internetbandwidth.availableInternetSpeed)
            //                                                                     .FirstOrDefault();
            //}
            //internetDetails = internetDetails.AsEnumerable().GroupBy(c => new { c.degreeId, c.degree })
            //                                                         .Select(cs => new InternetBandwidthDetails
            //                                                         {
            //                                                             degreeId = cs.Key.degreeId,
            //                                                             degree = cs.Key.degree,
            //                                                             availableInternetSpeed = cs.FirstOrDefault().availableInternetSpeed,
            //                                                             totalIntake = cs.Sum(s => s.totalIntake)
            //                                                         }).ToList();
            //List<jntuh_college_degree> collegeDegree = db.jntuh_college_degree.Where(d => d.collegeId == userCollegeID && d.isActive == true).ToList();

            var cSpcIds =
                db.jntuh_college_intake_existing.Where(i => i.collegeId == userCollegeID && (i.academicYearId == prAy - 1 || i.academicYearId == prAy - 2 || i.academicYearId == prAy - 3) && (i.approvedIntake != 0 || i.aicteApprovedIntake != 0))
                //.GroupBy(r => new { r.specializationId })
                    .Select(s => s.specializationId)
                    .ToList();

            var DepartmentsData = db.jntuh_specialization.Where(e => e.isActive == true && cSpcIds.Contains(e.id)).Select(e => e.departmentId).Distinct().ToArray();

            var DegreeIds = db.jntuh_department.Where(e => e.isActive == true && DepartmentsData.Contains(e.id)).Select(e => e.degreeId).Distinct().ToList();

            List<InternetBandwidthDetails> internetDetails = new List<InternetBandwidthDetails>();

            foreach (var item in DegreeIds)
            {
                InternetBandwidthDetails details = new InternetBandwidthDetails();
                details.id = 0;
                details.degreeId = item;
                details.degree = db.jntuh_degree.Where(d => d.isActive == true && d.id == item).Select(d => d.degree).FirstOrDefault();
                details.totalIntake = GetIntake(item, userCollegeID);
                details.availableInternetSpeed = db.jntuh_college_internet_bandwidth.Where(internetbandwidth => internetbandwidth.collegeId == userCollegeID &&
                                                                                        internetbandwidth.degreeId == item)
                                                                                 .Select(internetbandwidth => internetbandwidth.availableInternetSpeed)
                                                                                 .FirstOrDefault();
                internetDetails.Add(details);
            }
            if (internetSpeedId == 0)
            {
                ViewBag.Norecords = true;
            }
            else
            {                
                ViewBag.Update = true;
                ViewBag.Count = internetDetails.Count();
            }
            return View("View", internetDetails);
        }

        public ActionResult UserView(string id)
        {
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int ay0 = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            int userCollegeID = Convert.ToInt32(Utilities.DecryptString(id, WebConfigurationManager.AppSettings["CryptoKey"]));
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            int[] degree = db.jntuh_college_degree.Where(d => d.isActive == true && d.collegeId == userCollegeID).Select(d => d.degreeId).ToArray();
            int internetSpeedId = db.jntuh_college_internet_bandwidth.Where(a => a.collegeId == userCollegeID && degree.Contains(a.degreeId)).Select(a => a.id).FirstOrDefault();
            //List<jntuh_college_degree> collegeDegree = db.jntuh_college_degree.Where(d => d.collegeId == userCollegeID && d.isActive == true).ToList();

            var cSpcIds =
                db.jntuh_college_intake_existing.Where(i => i.collegeId == userCollegeID && (i.academicYearId == ay0 - 1 || i.academicYearId == ay0 - 2 || i.academicYearId == ay0 - 3) && (i.approvedIntake != 0 || i.aicteApprovedIntake != 0))
                //.GroupBy(r => new { r.specializationId })
                    .Select(s => s.specializationId)
                    .ToList();

            var DepartmentsData = db.jntuh_specialization.Where(e => e.isActive == true && cSpcIds.Contains(e.id)).Select(e => e.departmentId).Distinct().ToArray();

            var DegreeIds = db.jntuh_department.Where(e => e.isActive == true && DepartmentsData.Contains(e.id)).Select(e => e.degreeId).Distinct().ToList();

            List<InternetBandwidthDetails> internetDetails = new List<InternetBandwidthDetails>();

            foreach (var item in DegreeIds)
            {
                InternetBandwidthDetails details = new InternetBandwidthDetails();
                details.id = 0;
                details.degreeId = item;
                details.degree = db.jntuh_degree.Where(d => d.isActive == true && d.id == item).Select(d => d.degree).FirstOrDefault();
                details.totalIntake = GetIntake(item, userCollegeID);
                details.availableInternetSpeed = db.jntuh_college_internet_bandwidth.Where(internetbandwidth => internetbandwidth.collegeId == userCollegeID &&
                                                                                        internetbandwidth.degreeId == item)
                                                                                 .Select(internetbandwidth => internetbandwidth.availableInternetSpeed)
                                                                                 .FirstOrDefault();
                internetDetails.Add(details);
            }
            if (internetSpeedId == 0)
            {
                ViewBag.Norecords = true;
            }
            else
            {
                ViewBag.Update = true;
                ViewBag.Count = internetDetails.Count();
            }
            return View("UserView", internetDetails);
        }
    }
}
