using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Security;
using UAAAS.Models;
using System.IO;
using System.Configuration;

namespace UAAAS.Controllers
{
    [ErrorHandling]
    public class CollegeLibraryDetailsController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();
        //
        // GET: /CollegeLibraryDetails/

        public ActionResult Index()
        {
            return View();
        }

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
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            int[] degree = db.jntuh_college_degree.Where(d => d.isActive == true && d.collegeId == userCollegeID).Select(d => d.degreeId).ToArray();
            int collegeLibraryId = db.jntuh_college_library_details.Where(a => a.collegeId == userCollegeID && degree.Contains(a.degreeId)).Select(a => a.id).FirstOrDefault();

            DateTime todayDate = DateTime.Now.Date;
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy =
                db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            int status = db.jntuh_college_edit_status.Where(editStatus => editStatus.academicyearId == prAy && editStatus.collegeId == userCollegeID &&
                                                                                      editStatus.IsCollegeEditable == true &&
                                                                                      editStatus.editFromDate <= todayDate &&
                                                                                      editStatus.editToDate >= todayDate)
                                                                 .Select(editStatus => editStatus.id)
                                                                 .FirstOrDefault(); if (userCollegeID == 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("Create", "CollegeInformation");
            }
            if (userCollegeID > 0 && collegeLibraryId > 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("View", "CollegeLibraryDetails");
            }

            if (userCollegeID > 0 && collegeLibraryId > 0 && (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry")))
            {
                return RedirectToAction("Edit", "CollegeLibraryDetails", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
            }
            if (userCollegeID == 0 && (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry")))
            {
                return RedirectToAction("Create", "CollegeInformation", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
            }

            if (collegeLibraryId == 0 && status == 0 && Roles.IsUserInRole("College"))
            {
                ViewBag.NotUpload = true;
            }
            else
            {
                ViewBag.NotUpload = false;
            }

            bool isPageEditable = db.jntuh_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("LB") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

            if (isPageEditable)
            {
                ViewBag.IsEditable = true;
            }
            else
            {
                ViewBag.IsEditable = false;
                return RedirectToAction("View", "CollegeLibraryDetails");
            }

            var cSpcIds =
               db.jntuh_college_intake_existing.Where(i => i.collegeId == userCollegeID && (i.academicYearId == prAy - 1 || i.academicYearId == prAy - 2 || i.academicYearId == prAy - 3) && (i.approvedIntake != 0 || i.aicteApprovedIntake != 0))
                //.GroupBy(r => new { r.specializationId })
                   .Select(s => s.specializationId)
                   .ToList();

            var DepartmentsData = db.jntuh_specialization.Where(e => e.isActive == true && cSpcIds.Contains(e.id)).Select(e => e.departmentId).Distinct().ToArray();

            var DegreeIds = db.jntuh_department.Where(e => e.isActive == true && DepartmentsData.Contains(e.id)).Select(e => e.degreeId).Distinct().ToList();

            List<LibraryDetails> library = (from collegeDegree in DegreeIds
                                            join degre in db.jntuh_degree on collegeDegree equals degre.id
                                            where (degre.isActive == true)
                                            orderby degre.degree
                                            select new LibraryDetails
                                                                    {
                                                                        degreeId = collegeDegree,
                                                                        degree = degre.degree,
                                                                        totalTitles = null,
                                                                        totalVolumes = null,
                                                                        totalNationalJournals = null,
                                                                        totalInternationalJournals = null,
                                                                        totalEJournals = null,
                                                                        collegeId = userCollegeID,
                                                                        newTitles = null,
                                                                        newVolumes = null,
                                                                        newNationalJournals = null,
                                                                        newInternationalJournals = null,
                                                                        newEJournals = null,
                                                                        EJournalsSubscriptionNumber = null
                                                                    }).ToList();

            ViewBag.Count = library.Count();
            return View(library);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Create(ICollection<LibraryDetails> libraryDetails, HttpPostedFileBase fileUploader)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                foreach (var item in libraryDetails)
                {
                    userCollegeID = item.collegeId;
                }
            }
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy =
                db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            SaveLibraryDetails(libraryDetails, fileUploader);

            var cSpcIds =
               db.jntuh_college_intake_existing.Where(i => i.collegeId == userCollegeID && (i.academicYearId == prAy - 1 || i.academicYearId == prAy - 2 || i.academicYearId == prAy - 3) && (i.approvedIntake != 0 || i.aicteApprovedIntake != 0))
                //.GroupBy(r => new { r.specializationId })
                   .Select(s => s.specializationId)
                   .ToList();

            var DepartmentsData = db.jntuh_specialization.Where(e => e.isActive == true && cSpcIds.Contains(e.id)).Select(e => e.departmentId).Distinct().ToArray();

            var DegreeIds = db.jntuh_department.Where(e => e.isActive == true && DepartmentsData.Contains(e.id)).Select(e => e.degreeId).Distinct().ToList();

            List<LibraryDetails> library = (from collegeDegree in DegreeIds
                                            join degre in db.jntuh_degree on collegeDegree equals degre.id
                                            where (degre.isActive == true)
                                            orderby degre.degree
                                            select new LibraryDetails
                                            {
                                                degreeId = collegeDegree,
                                                degree = degre.degree,
                                                totalTitles = null,
                                                totalVolumes = null,
                                                totalNationalJournals = null,
                                                totalInternationalJournals = null,
                                                totalEJournals = null,
                                                collegeId = userCollegeID,
                                                newTitles = null,
                                                newVolumes = null,
                                                newNationalJournals = null,
                                                newInternationalJournals = null,
                                                newEJournals = null,
                                                EJournalsSubscriptionNumber = null
                                            }).ToList();
            foreach (var item in library)
            {
                jntuh_college_library_details details = db.jntuh_college_library_details.Where(collegeLibrary => collegeLibrary.collegeId == userCollegeID &&
                                                                          collegeLibrary.degreeId == item.degreeId)
                                                                   .Select(collegeLibrary => collegeLibrary).FirstOrDefault();

                item.totalTitles = details.totalTitles;
                item.totalVolumes = details.totalVolumes;
                item.totalNationalJournals = details.totalNationalJournals;
                item.totalInternationalJournals = details.totalInternationalJournals;
                item.totalEJournals = details.totalEJournals;
                item.newTitles = details.newTitles;
                item.newVolumes = details.newVolumes;
                item.newNationalJournals = details.newNationalJournals;
                item.newInternationalJournals = details.newInternationalJournals;
                item.newEJournals = details.newEJournals;
                item.EJournalsSubscriptionNumber = details.subscription;
            }

            ViewBag.Count = library.Count();
            return View(library);
        }

        private void SaveLibraryDetails(ICollection<LibraryDetails> libraryDetails, HttpPostedFileBase fileUploader)
        {
            ModelState.Clear();
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            var message = string.Empty;

            if (userCollegeID == 0)
            {
                foreach (var item in libraryDetails)
                {
                    userCollegeID = item.collegeId;
                }
            }
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            int enclosureId = db.jntuh_enclosures.Where(e => e.documentName == "Library-Titles").Select(e => e.id).FirstOrDefault();
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int ay0 = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            var college_enclosures = db.jntuh_college_enclosures.Where(e => e.enclosureId == enclosureId && e.collegeID == userCollegeID && e.academicyearId == ay0).Select(e => e).FirstOrDefault();
            jntuh_college_enclosures jntuh_college_enclosures = new jntuh_college_enclosures();
            jntuh_college_enclosures.collegeID = userCollegeID;
            jntuh_college_enclosures.academicyearId = ay0;
            jntuh_college_enclosures.enclosureId = enclosureId;
            jntuh_college_enclosures.isActive = true;

            string fileName = string.Empty;
            string FilePath = "~/Content/Upload/CollegeEnclosures";
            if (fileUploader != null)
            {
                string ext = Path.GetExtension(fileUploader.FileName);
                fileName = db.jntuh_college.Where(c => c.id == userCollegeID).Select(c => c.collegeCode).FirstOrDefault() + "_BOOKS_" + enclosureId + ext;

                if (!Directory.Exists(Server.MapPath(FilePath)))
                {
                    Directory.CreateDirectory(Server.MapPath(FilePath));
                }
                fileUploader.SaveAs(string.Format("{0}/{1}", Server.MapPath(FilePath), fileName));
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
            ModelState.Clear();
            if (ModelState.IsValid)
            {
                foreach (LibraryDetails item in libraryDetails)
                {
                    jntuh_college_library_details details = new jntuh_college_library_details();
                    details.degreeId = item.degreeId;
                    details.collegeId = userCollegeID;
                    details.totalTitles = Convert.ToInt32(item.totalTitles);
                    details.totalVolumes = Convert.ToInt32(item.totalVolumes);
                    details.totalNationalJournals = Convert.ToInt32(item.totalNationalJournals);
                    details.totalInternationalJournals = Convert.ToInt32(item.totalInternationalJournals);
                    details.totalEJournals = Convert.ToInt32(item.totalEJournals);

                    details.newTitles = Convert.ToInt32(item.newTitles);
                    details.newVolumes = Convert.ToInt32(item.newVolumes);
                    details.newNationalJournals = Convert.ToInt32(item.newNationalJournals);
                    details.newInternationalJournals = Convert.ToInt32(item.newInternationalJournals);
                    details.newEJournals = Convert.ToInt32(item.newEJournals);
                    details.subscription = item.EJournalsSubscriptionNumber;

                    int collegeLibraryDetailsId = db.jntuh_college_library_details.Where(l => l.collegeId == userCollegeID && l.degreeId == item.degreeId).Select(l => l.id).FirstOrDefault();
                    if (collegeLibraryDetailsId == 0)
                    {
                        details.createdBy = userID;
                        details.createdOn = DateTime.Now;
                        db.jntuh_college_library_details.Add(details);
                        db.SaveChanges();
                        message = "Save";
                    }
                    else
                    {
                        details.id = collegeLibraryDetailsId;
                        details.createdOn = db.jntuh_college_library_details.Where(d => d.id == collegeLibraryDetailsId).Select(d => d.createdOn).FirstOrDefault();
                        details.createdBy = db.jntuh_college_library_details.Where(d => d.id == collegeLibraryDetailsId).Select(d => d.createdBy).FirstOrDefault();
                        details.updatedBy = userID;
                        details.updatedOn = DateTime.Now;
                        db.Entry(details).State = EntityState.Modified;
                        db.SaveChanges();
                        message = "Update";
                    }
                }
                if (message == "Update")
                {
                    TempData["Success"] = "Library Details are Updated successfully";
                }
                else
                {
                    TempData["Success"] = "Library Details are Added successfully";
                }
            }
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Edit(string collegeId)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            int enclosureId = db.jntuh_enclosures.Where(e => e.documentName == "Library-Titles").Select(e => e.id).FirstOrDefault();
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int ay0 = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            string Librarytitles = db.jntuh_college_enclosures.Where(e => e.enclosureId == enclosureId && e.academicyearId == ay0 && e.collegeID == userCollegeID).Select(e => e.path).FirstOrDefault();
            if (userCollegeID == 0 && collegeId != null)
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
            int collegeLibraryId = db.jntuh_college_library_details.Where(a => a.collegeId == userCollegeID && degree.Contains(a.degreeId)).Select(a => a.id).FirstOrDefault();

            if (userCollegeID == 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("Create", "CollegeInformation");
            }

            if (userCollegeID == 0 && (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry")))
            {
                return RedirectToAction("Create", "CollegeInformation", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
            }
            if (collegeLibraryId == 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("Create", "CollegeLibraryDetails");
            }
            if (collegeLibraryId == 0 && (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry")))
            {
                return RedirectToAction("Create", "CollegeLibraryDetails", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
            }
            DateTime todayDate = DateTime.Now.Date;
            int prAy =
                db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            int status = db.jntuh_college_edit_status.Where(editStatus => editStatus.academicyearId == prAy && editStatus.collegeId == userCollegeID &&
                                                                                      editStatus.IsCollegeEditable == true &&
                                                                                      editStatus.editFromDate <= todayDate &&
                                                                                      editStatus.editToDate >= todayDate)
                                                                 .Select(editStatus => editStatus.id)
                                                                 .FirstOrDefault();
            if (status == 0 && Roles.IsUserInRole("College"))
            {
                ViewBag.IsEditable = false;
                return RedirectToAction("View", "CollegeLibraryDetails");
            }
            else
            {
                bool isPageEditable = db.jntuh_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("LB") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

                if (isPageEditable)
                {
                    ViewBag.IsEditable = true;
                }
                else
                {
                    ViewBag.IsEditable = false;
                    return RedirectToAction("View", "CollegeLibraryDetails");
                }
            }

            var cSpcIds =
               db.jntuh_college_intake_existing.Where(i => i.collegeId == userCollegeID && (i.academicYearId == prAy - 1 || i.academicYearId == prAy - 2 || i.academicYearId == prAy - 3) && (i.approvedIntake != 0 || i.aicteApprovedIntake != 0))
                //.GroupBy(r => new { r.specializationId })
                   .Select(s => s.specializationId)
                   .ToList();

            var DepartmentsData = db.jntuh_specialization.Where(e => e.isActive == true && cSpcIds.Contains(e.id)).Select(e => e.departmentId).Distinct().ToArray();

            var DegreeIds = db.jntuh_department.Where(e => e.isActive == true && DepartmentsData.Contains(e.id)).Select(e => e.degreeId).Distinct().ToList();

            List<LibraryDetails> library = (from collegeDegree in DegreeIds
                                            join degre in db.jntuh_degree on collegeDegree equals degre.id
                                            where (degre.isActive == true)
                                            orderby degre.degree
                                            select new LibraryDetails
                                            {
                                                degreeId = collegeDegree,
                                                degree = degre.degree,
                                                totalTitles = null,
                                                totalVolumes = null,
                                                totalNationalJournals = null,
                                                totalInternationalJournals = null,
                                                totalEJournals = null,
                                                collegeId = userCollegeID,
                                                newTitles = null,
                                                newVolumes = null,
                                                newNationalJournals = null,
                                                newInternationalJournals = null,
                                                newEJournals = null,
                                                EJournalsSubscriptionNumber = null
                                            }).ToList();
            foreach (var item in library)
            {
                jntuh_college_library_details details = db.jntuh_college_library_details.Where(collegeLibrary => collegeLibrary.collegeId == userCollegeID &&
                                                                          collegeLibrary.degreeId == item.degreeId)
                                                                   .Select(collegeLibrary => collegeLibrary).FirstOrDefault();
                if (details != null)
                {
                    item.totalTitles = details.totalTitles;
                    item.totalVolumes = details.totalVolumes;
                    item.totalNationalJournals = details.totalNationalJournals;
                    item.totalInternationalJournals = details.totalInternationalJournals;
                    item.totalEJournals = details.totalEJournals;
                    item.newTitles = details.newTitles;
                    item.newVolumes = details.newVolumes;
                    item.newNationalJournals = details.newNationalJournals;
                    item.newInternationalJournals = details.newInternationalJournals;
                    item.newEJournals = details.newEJournals;
                    item.EJournalsSubscriptionNumber = details.subscription;
                    item.LibraryTitlesPath = Librarytitles;
                }

            }

            ViewBag.Count = library.Count();
            return View("Create", library);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Edit(ICollection<LibraryDetails> libraryDetails, HttpPostedFileBase fileUploader)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            int enclosureId = db.jntuh_enclosures.Where(e => e.documentName == "Library-Titles").Select(e => e.id).FirstOrDefault();
            var Librarytitles = db.jntuh_college_enclosures.Where(e => e.enclosureId == enclosureId && e.collegeID == userCollegeID).Select(e => e.path).FirstOrDefault();
            if (userCollegeID == 0)
            {
                foreach (var item in libraryDetails)
                {
                    userCollegeID = item.collegeId;
                }
            }
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            if (Request.Files.Count > 0)
            {
                SaveLibraryDetails(libraryDetails, Request.Files[0]);
            }

            List<LibraryDetails> library = (from collegeDegree in db.jntuh_college_degree
                                            join degre in db.jntuh_degree on collegeDegree.degreeId equals degre.id
                                            where (collegeDegree.collegeId == userCollegeID && collegeDegree.isActive == true)
                                            orderby degre.degree
                                            select new LibraryDetails
                                            {
                                                degreeId = collegeDegree.degreeId,
                                                degree = degre.degree,
                                                totalTitles = null,
                                                totalVolumes = null,
                                                totalNationalJournals = null,
                                                totalInternationalJournals = null,
                                                totalEJournals = null,
                                                collegeId = userCollegeID,
                                                newTitles = null,
                                                newVolumes = null,
                                                newNationalJournals = null,
                                                newInternationalJournals = null,
                                                newEJournals = null,
                                                EJournalsSubscriptionNumber = null
                                            }).ToList();


            foreach (var item in library)
            {
                jntuh_college_library_details details = db.jntuh_college_library_details.Where(collegeLibrary => collegeLibrary.collegeId == userCollegeID &&
                                                                          collegeLibrary.degreeId == item.degreeId)
                                                                   .Select(collegeLibrary => collegeLibrary).FirstOrDefault();


                if (details != null)
                {
                    item.totalTitles = details.totalTitles;
                    item.totalVolumes = details.totalVolumes;
                    item.totalNationalJournals = details.totalNationalJournals;
                    item.totalInternationalJournals = details.totalInternationalJournals;
                    item.totalEJournals = details.totalEJournals;
                    item.newTitles = details.newTitles;
                    item.newVolumes = details.newVolumes;
                    item.newNationalJournals = details.newNationalJournals;
                    item.newInternationalJournals = details.newInternationalJournals;
                    item.newEJournals = details.newEJournals;
                    item.EJournalsSubscriptionNumber = details.subscription;
                    item.LibraryTitlesPath = Librarytitles;
                }

            }

            ViewBag.Count = library.Count();
            // return View("Create", library);

            return View("View");
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
            int collegeLibraryId = db.jntuh_college_library_details.Where(a => a.collegeId == userCollegeID && degree.Contains(a.degreeId)).Select(a => a.id).FirstOrDefault();
            int enclosureId = db.jntuh_enclosures.Where(e => e.documentName == "Library-Titles").Select(e => e.id).FirstOrDefault();
            
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
                bool isPageEditable = db.jntuh_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("LB") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

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
            var Librarytitles = db.jntuh_college_enclosures.Where(e => e.enclosureId == enclosureId && e.collegeID == userCollegeID && e.academicyearId == prAy).Select(e => e.path).FirstOrDefault();
            var cSpcIds =
              db.jntuh_college_intake_existing.Where(i => i.collegeId == userCollegeID && (i.academicYearId == prAy - 1 || i.academicYearId == prAy - 2 || i.academicYearId == prAy - 3) && (i.approvedIntake != 0 || i.aicteApprovedIntake != 0))
                //.GroupBy(r => new { r.specializationId })
                  .Select(s => s.specializationId)
                  .ToList();

            var DepartmentsData = db.jntuh_specialization.Where(e => e.isActive == true && cSpcIds.Contains(e.id)).Select(e => e.departmentId).Distinct().ToArray();

            var DegreeIds = db.jntuh_department.Where(e => e.isActive == true && DepartmentsData.Contains(e.id)).Select(e => e.degreeId).Distinct().ToList();

            List<LibraryDetails> library = (from collegeDegree in DegreeIds
                                            join degre in db.jntuh_degree on collegeDegree equals degre.id
                                            where (degre.isActive == true)
                                            orderby degre.degree
                                            select new LibraryDetails
                                            {
                                                degreeId = collegeDegree,
                                                degree = degre.degree,
                                                totalTitles = null,
                                                totalVolumes = null,
                                                totalNationalJournals = null,
                                                totalInternationalJournals = null,
                                                totalEJournals = null,
                                                newTitles = null,
                                                newVolumes = null,
                                                newNationalJournals = null,
                                                newInternationalJournals = null,
                                                newEJournals = null,
                                                EJournalsSubscriptionNumber = null
                                            }).ToList();
            foreach (var item in library)
            {
                jntuh_college_library_details details = db.jntuh_college_library_details.Where(collegeLibrary => collegeLibrary.collegeId == userCollegeID &&
                                                                          collegeLibrary.degreeId == item.degreeId)
                                                                   .Select(collegeLibrary => collegeLibrary).FirstOrDefault();

                if (details != null)
                {
                    item.totalTitles = details.totalTitles;
                    item.totalVolumes = details.totalVolumes;
                    item.totalNationalJournals = details.totalNationalJournals;
                    item.totalInternationalJournals = details.totalInternationalJournals;
                    item.totalEJournals = details.totalEJournals;
                    item.newTitles = details.newTitles;
                    item.newVolumes = details.newVolumes;
                    item.newNationalJournals = details.newNationalJournals;
                    item.newInternationalJournals = details.newInternationalJournals;
                    item.newEJournals = details.newEJournals;
                    item.EJournalsSubscriptionNumber = details.subscription;
                    item.LibraryTitlesPath = Librarytitles;
                }
            }
            if (collegeLibraryId == 0)
            {
                ViewBag.Norecords = true;
            }
            else
            {
                ViewBag.Count = library.Count();

            }
            return View("View", library);
        }

        public ActionResult UserView(string id)
        {
            int userCollegeID = Convert.ToInt32(Utilities.DecryptString(id, WebConfigurationManager.AppSettings["CryptoKey"]));
            int[] degree = db.jntuh_college_degree.Where(d => d.isActive == true && d.collegeId == userCollegeID).Select(d => d.degreeId).ToArray();
            int collegeLibraryId = db.jntuh_college_library_details.Where(a => a.collegeId == userCollegeID && degree.Contains(a.degreeId)).Select(a => a.id).FirstOrDefault();
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy =
                db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            var cSpcIds =
              db.jntuh_college_intake_existing.Where(i => i.collegeId == userCollegeID && (i.academicYearId == prAy - 1 || i.academicYearId == prAy - 2 || i.academicYearId == prAy - 3) && (i.approvedIntake != 0 || i.aicteApprovedIntake != 0))
                //.GroupBy(r => new { r.specializationId })
                  .Select(s => s.specializationId)
                  .ToList();

            var DepartmentsData = db.jntuh_specialization.Where(e => e.isActive == true && cSpcIds.Contains(e.id)).Select(e => e.departmentId).Distinct().ToArray();

            var DegreeIds = db.jntuh_department.Where(e => e.isActive == true && DepartmentsData.Contains(e.id)).Select(e => e.degreeId).Distinct().ToList();
            List<LibraryDetails> library = (from collegeDegree in DegreeIds
                                            join degre in db.jntuh_degree on collegeDegree equals degre.id
                                            where (degre.isActive == true)
                                            orderby degre.degree
                                            select new LibraryDetails
                                            {
                                                degreeId = collegeDegree,
                                                degree = degre.degree,
                                                totalTitles = null,
                                                totalVolumes = null,
                                                totalNationalJournals = null,
                                                totalInternationalJournals = null,
                                                totalEJournals = null,
                                                newTitles = null,
                                                newVolumes = null,
                                                newNationalJournals = null,
                                                newInternationalJournals = null,
                                                newEJournals = null,
                                                EJournalsSubscriptionNumber = null
                                            }).ToList();
            foreach (var item in library)
            {
                jntuh_college_library_details details = db.jntuh_college_library_details.Where(collegeLibrary => collegeLibrary.collegeId == userCollegeID &&
                                                                           collegeLibrary.degreeId == item.degreeId)
                                                                    .Select(collegeLibrary => collegeLibrary).FirstOrDefault();

                item.totalTitles = details.totalTitles;
                item.totalVolumes = details.totalVolumes;
                item.totalNationalJournals = details.totalNationalJournals;
                item.totalInternationalJournals = details.totalInternationalJournals;
                item.totalEJournals = details.totalEJournals;
                item.newTitles = details.newTitles;
                item.newVolumes = details.newVolumes;
                item.newNationalJournals = details.newNationalJournals;
                item.newInternationalJournals = details.newInternationalJournals;
                item.newEJournals = details.newEJournals;
                item.EJournalsSubscriptionNumber = details.subscription;

            }
            if (collegeLibraryId == 0)
            {
                ViewBag.Norecords = true;
            }
            else
            {
                ViewBag.Count = library.Count();

            }
            return View("UserView", library);
        }
    }
}
