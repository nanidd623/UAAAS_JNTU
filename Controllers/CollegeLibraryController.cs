using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using UAAAS.Models;
using System.Web.Security;
using System.Data;
using System.Web.Configuration;
using System.Configuration;
//using ReportManagement;

namespace UAAAS.Controllers
{
    [ErrorHandling]
    public class CollegeLibraryController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();
        //
        // GET: /CollegeLibrary/

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
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
            ViewBag.collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString();
            int userLibraryInformationID = db.jntuh_college_library.Where(e => e.collegeId == userCollegeID).Select(e => e.id).FirstOrDefault();

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
            if (userCollegeID == 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("Create", "CollegeInformation");
            }
            if (userCollegeID == 0 && (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry")))
            {
                return RedirectToAction("Create", "CollegeInformation", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
            }

            if (userCollegeID > 0 && userLibraryInformationID > 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("View", "CollegeLibrary");
            }
            if (userCollegeID > 0 && userLibraryInformationID > 0 && (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry")))
            {
                return RedirectToAction("Edit", "CollegeLibrary", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
            }

            if (userLibraryInformationID == 0 && status == 0 && Roles.IsUserInRole("College"))
            {
                ViewBag.NotUpload = true;
            }
            else
            {
                ViewBag.NotUpload = false;
            }

            bool isPageEditable = db.jntuh_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("LI") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

            if (isPageEditable)
            {
                ViewBag.IsEditable = true;
            }
            else
            {
                ViewBag.IsEditable = false;
                return RedirectToAction("View", "CollegeLibrary");
            }

            CollegeLibrary collegeLibrary = new CollegeLibrary();
            collegeLibrary.collegeId = userCollegeID;
            return View(collegeLibrary);
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        [HttpPost]
        public ActionResult Create(CollegeLibrary collegeLibrary)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                userCollegeID = collegeLibrary.collegeId;
            }
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            SaveCollegeLibrary(collegeLibrary);
            return View("Create", collegeLibrary);
        }

        public void SaveCollegeLibrary(CollegeLibrary collegeLibrary)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                userCollegeID = collegeLibrary.collegeId;
            }
            var message = string.Empty;
            if (ModelState.IsValid)
            {
                jntuh_college_library libraryDetails = new jntuh_college_library();
                libraryDetails.collegeId = userCollegeID;
                libraryDetails.librarianName = collegeLibrary.librarianName;
                libraryDetails.librarianQualifications = collegeLibrary.librarianQualifications;
                libraryDetails.libraryPhoneNumber = collegeLibrary.libraryPhoneNumber;
                libraryDetails.totalSupportingStaff = collegeLibrary.totalSupportingStaff;
                libraryDetails.totalTitles = collegeLibrary.totalTitles;
                libraryDetails.totalVolumes = collegeLibrary.totalVolumes;
                libraryDetails.totalNationalJournals = collegeLibrary.totalNationalJournals;
                libraryDetails.totalInternationalJournals = collegeLibrary.totalInternationalJournals;
                libraryDetails.totalEJournals = collegeLibrary.totalEJournals;
                libraryDetails.librarySeatingCapacity = collegeLibrary.librarySeatingCapacity;
                libraryDetails.libraryWorkingHoursFrom = collegeLibrary.libraryWorkingHoursFrom;
                libraryDetails.libraryWorkingHoursTo = collegeLibrary.libraryWorkingHoursTo;

                var libraryId = db.jntuh_college_library.Where(l => l.collegeId == userCollegeID).Select(l => l.id).FirstOrDefault();
                if (libraryId == 0)
                {
                    libraryDetails.createdBy = userID;
                    libraryDetails.createdOn = DateTime.Now;
                    db.jntuh_college_library.Add(libraryDetails);
                    message = "Save";
                    db.SaveChanges();
                }
                else
                {
                    libraryDetails.id = libraryId;
                    libraryDetails.createdOn = db.jntuh_college_library.Where(l => l.id == libraryId).Select(l => l.createdOn).FirstOrDefault();
                    libraryDetails.createdBy = db.jntuh_college_library.Where(l => l.id == libraryId).Select(l => l.createdBy).FirstOrDefault();
                    libraryDetails.updatedBy = userID;
                    libraryDetails.updatedOn = DateTime.Now;
                    db.Entry(libraryDetails).State = EntityState.Modified;
                    db.SaveChanges();
                    message = "Update";
                }

                if (message == "Update")
                {
                    TempData["Success"] = "Updated successfully.";
                }
                else
                {
                    TempData["Success"] = "Added successfully.";
                }
            }
        }

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
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            int collegeLibraryId = db.jntuh_college_library.Where(l => l.collegeId == userCollegeID).Select(l => l.id).FirstOrDefault();
            if (userCollegeID == 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("Create", "CollegeInformation");
            }
            if (userCollegeID == 0 && (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry")))
            {
                return RedirectToAction("Edit", "CollegeInformation", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
            }

            if (collegeLibraryId == 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("Create", "CollegeLibrary");
            }
            if (collegeLibraryId == 0 && (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry")))
            {
                return RedirectToAction("Create", "CollegeLibrary", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
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
            if (status == 0 && Roles.IsUserInRole("College"))
            {
                ViewBag.IsEditable = false;
                return RedirectToAction("View", "CollegeLibrary");
            }
            else
            {
                ViewBag.IsEditable = true;
                bool isPageEditable = db.jntuh_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("LI") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

                if (isPageEditable)
                {
                    ViewBag.IsEditable = true;
                }
                else
                {
                    ViewBag.IsEditable = false;
                    return RedirectToAction("View", "CollegeLibrary");
                }
            }

            CollegeLibrary collegeLibrary = new CollegeLibrary();
            collegeLibrary.collegeId = userCollegeID;
            jntuh_college_library jntuh_college_library = db.jntuh_college_library.Find(collegeLibraryId);
            collegeLibrary.id = jntuh_college_library.id;
            collegeLibrary.collegeId = jntuh_college_library.collegeId;
            collegeLibrary.librarianName = jntuh_college_library.librarianName;
            collegeLibrary.librarianQualifications = jntuh_college_library.librarianQualifications;
            collegeLibrary.libraryPhoneNumber = jntuh_college_library.libraryPhoneNumber;
            collegeLibrary.totalSupportingStaff = jntuh_college_library.totalSupportingStaff;
            collegeLibrary.totalTitles = jntuh_college_library.totalTitles;
            collegeLibrary.totalVolumes = jntuh_college_library.totalVolumes;
            collegeLibrary.totalNationalJournals = jntuh_college_library.totalNationalJournals;
            collegeLibrary.totalInternationalJournals = jntuh_college_library.totalInternationalJournals;
            collegeLibrary.totalEJournals = jntuh_college_library.totalEJournals;
            collegeLibrary.librarySeatingCapacity = jntuh_college_library.librarySeatingCapacity;
            collegeLibrary.libraryWorkingHoursFrom = jntuh_college_library.libraryWorkingHoursFrom;
            collegeLibrary.libraryWorkingHoursTo = jntuh_college_library.libraryWorkingHoursTo;
            collegeLibrary.createdBy = jntuh_college_library.createdBy;
            collegeLibrary.createdOn = jntuh_college_library.createdOn;
            collegeLibrary.updatedBy = jntuh_college_library.updatedBy;
            collegeLibrary.updatedOn = jntuh_college_library.updatedOn;
            return View("Create", collegeLibrary);
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        [HttpPost]
        public ActionResult Edit(CollegeLibrary collegeLibrary)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                userCollegeID = collegeLibrary.collegeId;
            }
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            SaveCollegeLibrary(collegeLibrary);
            //return View("Create", collegeLibrary);
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
            int collegeLibraryId = db.jntuh_college_library.Where(l => l.collegeId == userCollegeID).Select(l => l.id).FirstOrDefault();
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
                bool isPageEditable = db.jntuh_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("LI") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

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

            CollegeLibrary collegeLibrary = new CollegeLibrary();
            if (collegeLibraryId == 0)
            {
                ViewBag.Norecords = true;
            }
            else
            {
                ViewBag.Norecords = false;

                //List<LibraryDetails> library = (from collegeDegree in db.jntuh_college_degree
                //                                join degre in db.jntuh_degree on collegeDegree.degreeId equals degre.id
                //                                where (collegeDegree.collegeId == userCollegeID && collegeDegree.isActive == true)
                //                                orderby degre.degree
                //                                select new LibraryDetails
                //                                {
                //                                    degreeId = collegeDegree.degreeId,
                //                                    degree = degre.degree,
                //                                    totalTitles = null,
                //                                    totalVolumes = null,
                //                                    totalNationalJournals = null,
                //                                    totalInternationalJournals = null,
                //                                    totalEJournals = null,
                //                                    newTitles = null,
                //                                    newVolumes = null,
                //                                    newNationalJournals = null,
                //                                    newInternationalJournals = null,
                //                                    newEJournals = null,
                //                                    EJournalsSubscriptionNumber = null
                //                                }).ToList();
                //foreach (var item in library)
                //{
                //    jntuh_college_library_details details = db.jntuh_college_library_details.Where(l => l.collegeId == userCollegeID && l.degreeId == item.degreeId).Select(l => l).FirstOrDefault();

                //    item.totalTitles = details.totalTitles;
                //    item.totalVolumes = details.totalVolumes;
                //    item.totalNationalJournals = details.totalNationalJournals;
                //    item.totalInternationalJournals = details.totalInternationalJournals;
                //    item.totalEJournals = details.totalEJournals;
                //    item.newTitles = details.newTitles;
                //    item.newVolumes = details.newVolumes;
                //    item.newNationalJournals = details.newNationalJournals;
                //    item.newInternationalJournals = details.newInternationalJournals;
                //    item.newEJournals = details.newEJournals;
                //    item.EJournalsSubscriptionNumber = details.subscription;

                //}

                jntuh_college_library jntuh_college_library = db.jntuh_college_library.Find(collegeLibraryId);
                collegeLibrary.id = jntuh_college_library.id;
                collegeLibrary.collegeId = jntuh_college_library.collegeId;
                collegeLibrary.librarianName = jntuh_college_library.librarianName;
                collegeLibrary.librarianQualifications = jntuh_college_library.librarianQualifications;
                collegeLibrary.libraryPhoneNumber = jntuh_college_library.libraryPhoneNumber;
                collegeLibrary.totalSupportingStaff = jntuh_college_library.totalSupportingStaff;
                collegeLibrary.totalTitles = jntuh_college_library.totalTitles;
                collegeLibrary.totalVolumes = jntuh_college_library.totalVolumes;
                collegeLibrary.totalNationalJournals = jntuh_college_library.totalNationalJournals;
                collegeLibrary.totalInternationalJournals = jntuh_college_library.totalInternationalJournals;
                collegeLibrary.totalEJournals = jntuh_college_library.totalEJournals;
                collegeLibrary.librarySeatingCapacity = jntuh_college_library.librarySeatingCapacity;
                collegeLibrary.libraryWorkingHoursFrom = jntuh_college_library.libraryWorkingHoursFrom;
                collegeLibrary.libraryWorkingHoursTo = jntuh_college_library.libraryWorkingHoursTo;
                collegeLibrary.createdBy = jntuh_college_library.createdBy;
                collegeLibrary.createdOn = jntuh_college_library.createdOn;
                collegeLibrary.updatedBy = jntuh_college_library.updatedBy;
                collegeLibrary.updatedOn = jntuh_college_library.updatedOn;
            }
            return View("View", collegeLibrary);
        }

        public ActionResult UserView(string id)
        {
            int userCollegeID = Convert.ToInt32(Utilities.DecryptString(id, WebConfigurationManager.AppSettings["CryptoKey"]));
            int collegeLibraryId = db.jntuh_college_library.Where(l => l.collegeId == userCollegeID).Select(l => l.id).FirstOrDefault();
            CollegeLibrary collegeLibrary = new CollegeLibrary();
            if (collegeLibraryId == 0)
            {
                ViewBag.Norecords = true;
            }
            else
            {
                ViewBag.Norecords = false;
                jntuh_college_library jntuh_college_library = db.jntuh_college_library.Find(collegeLibraryId);
                collegeLibrary.id = jntuh_college_library.id;
                collegeLibrary.collegeId = jntuh_college_library.collegeId;
                collegeLibrary.librarianName = jntuh_college_library.librarianName;
                collegeLibrary.librarianQualifications = jntuh_college_library.librarianQualifications;
                collegeLibrary.libraryPhoneNumber = jntuh_college_library.libraryPhoneNumber;
                collegeLibrary.totalSupportingStaff = jntuh_college_library.totalSupportingStaff;
                collegeLibrary.totalTitles = jntuh_college_library.totalTitles;
                collegeLibrary.totalVolumes = jntuh_college_library.totalVolumes;
                collegeLibrary.totalNationalJournals = jntuh_college_library.totalNationalJournals;
                collegeLibrary.totalInternationalJournals = jntuh_college_library.totalInternationalJournals;
                collegeLibrary.totalEJournals = jntuh_college_library.totalEJournals;
                collegeLibrary.librarySeatingCapacity = jntuh_college_library.librarySeatingCapacity;
                collegeLibrary.libraryWorkingHoursFrom = jntuh_college_library.libraryWorkingHoursFrom;
                collegeLibrary.libraryWorkingHoursTo = jntuh_college_library.libraryWorkingHoursTo;
            }
            return View("UserView", collegeLibrary);
        }
    }
}
