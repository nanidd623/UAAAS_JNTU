using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Security;
using UAAAS.Models;
using UAAAS.Models.Permanent_Affiliation;

namespace UAAAS.Controllers.Permanent_Affiliation
{
    [ErrorHandling]
    public class BooksandJournalsController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();

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

            if (userCollegeID > 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("View", "BooksandJournals", new
                {
                    collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString(),
                    bookJournalId = ""
                });
            }
            if (userCollegeID > 0 && (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry")))
            {
                return RedirectToAction("Edit", "BooksandJournals", new
                {
                    collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString(),
                    bookJournalId = ""
                });
            }

            return View();
        }

        [Authorize(Roles = "Admin,College")]
        public ActionResult View(string collegeId, string essentialTypeId)
        {
            //return RedirectToAction("Index", "UnderConstruction");
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();

            if (userCollegeID == 0)
            {
                userCollegeID = Convert.ToInt32(Utilities.DecryptString(collegeId, System.Web.Configuration.WebConfigurationManager.AppSettings["CryptoKey"]));
            }
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }

            var status = GetOnlineAppPageEditableStatus(userCollegeID);

            if (status > 0 && Roles.IsUserInRole("College"))
            {
                bool isPageEditable = db.jntuh_pa_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("PBJ") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

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
            int dec_essentialTypeId = Convert.ToInt32(Utilities.DecryptString(essentialTypeId, System.Web.Configuration.WebConfigurationManager.AppSettings["CryptoKey"]));

            var booksJournalsList = db.jntuh_college_booksandjournals.Where(c => c.collegeid == userCollegeID && c.isactive == true && c.essentialtype == dec_essentialTypeId).OrderByDescending(c => c.academicyearId).ToList();
            List<BooksJournals> booksJournalsListObj = new List<BooksJournals>();
            foreach (var item in booksJournalsList)
            {
                BooksJournals booksJournalsObj = new BooksJournals();
                booksJournalsObj.CollegeId = item.collegeid;
                booksJournalsObj.BookandJournalId = item.id;
                booksJournalsObj.DegreeId = db.jntuh_degree.Where(a => a.id == item.degreeid).Select(a => a.degree).FirstOrDefault();
                booksJournalsObj.AcademicYearId = db.jntuh_academic_year.Where(a => a.id == item.academicyearId).Select(a => a.academicYear).FirstOrDefault();
                booksJournalsObj.NumberOfBooks = item.numberofbooks;
                booksJournalsObj.AmountSpent = item.amountspent;
                booksJournalsObj.SupportingDocumentPath = item.supporingdocument;
                booksJournalsObj.EssentialType = Convert.ToString(item.essentialtype);

                booksJournalsListObj.Add(booksJournalsObj);
            }
            ViewBag.BooksJournalsList = booksJournalsListObj;
            ViewBag.TitleType = dec_essentialTypeId == 1 ? "List of Books" : "List of Journals";
            BookandJournalsModel bookjournalmodel = new BookandJournalsModel();
            bookjournalmodel.EssentialType = dec_essentialTypeId == 1 ? "Books" : "Journals";

            ViewBag.ActivityDescription = db.jntuh_extracurricularactivities.Where(e => e.activitytype == 14).Select(e => e.activitydescription).FirstOrDefault();
            var masteractivities = db.jntuh_extracurricularactivities.Where(i => i.isactive && i.activitytype == 14).Select(i => i.sno).ToArray();
            if (masteractivities.Count() > 0)
            {
                var collegeExtracurricularactivities = db.jntuh_college_extracurricularactivities.FirstOrDefault(e => masteractivities.Contains(e.activityid) && e.collegeid == 375);
                if (collegeExtracurricularactivities != null)
                {
                    bookjournalmodel.ActivitySelected = collegeExtracurricularactivities.activitystatus;
                    bookjournalmodel.ActivityDocumentPath = collegeExtracurricularactivities.supportingdocuments;
                    bookjournalmodel.Remarks = collegeExtracurricularactivities.remarks;
                }
            }
            return View(bookjournalmodel);
        }

        [Authorize(Roles = "Admin,College")]
        public ActionResult BookView(string collegeId, string essentialTypeId)
        {
            //return RedirectToAction("Index", "UnderConstruction");
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();

            if (userCollegeID == 0)
            {
                userCollegeID = Convert.ToInt32(Utilities.DecryptString(collegeId, System.Web.Configuration.WebConfigurationManager.AppSettings["CryptoKey"]));
            }
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }

            var status = GetOnlineAppPageEditableStatus(userCollegeID);

            if (status > 0 && Roles.IsUserInRole("College"))
            {
                bool isPageEditable = db.jntuh_pa_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("PBJ") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

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
            int dec_essentialTypeId = Convert.ToInt32(Utilities.DecryptString(essentialTypeId, System.Web.Configuration.WebConfigurationManager.AppSettings["CryptoKey"]));

            var booksJournalsList = db.jntuh_college_booksandjournals.Where(c => c.collegeid == userCollegeID && c.isactive == true && c.essentialtype == dec_essentialTypeId).OrderByDescending(c => c.academicyearId).ToList();
            List<BooksJournals> booksJournalsListObj = new List<BooksJournals>();
            foreach (var item in booksJournalsList)
            {
                BooksJournals booksJournalsObj = new BooksJournals();
                booksJournalsObj.CollegeId = item.collegeid;
                booksJournalsObj.BookandJournalId = item.id;
                booksJournalsObj.DegreeId = db.jntuh_degree.Where(a => a.id == item.degreeid).Select(a => a.degree).FirstOrDefault();
                booksJournalsObj.AcademicYearId = db.jntuh_academic_year.Where(a => a.id == item.academicyearId).Select(a => a.academicYear).FirstOrDefault();
                booksJournalsObj.NumberOfBooks = item.numberofbooks;
                booksJournalsObj.AmountSpent = item.amountspent;
                booksJournalsObj.SupportingDocumentPath = item.supporingdocument;
                booksJournalsObj.EssentialType = Convert.ToString(item.essentialtype);

                booksJournalsListObj.Add(booksJournalsObj);
            }
            ViewBag.BooksJournalsList = booksJournalsListObj;
            ViewBag.TitleType = dec_essentialTypeId == 1 ? "List of Books" : "List of Journals";
            BookandJournalsModel bookjournalmodel = new BookandJournalsModel();
            bookjournalmodel.EssentialType = dec_essentialTypeId == 1 ? "Books" : "Journals";

            ViewBag.ActivityDescription = db.jntuh_extracurricularactivities.Where(e => e.activitytype == 14).Select(e => e.activitydescription).FirstOrDefault();
            var masteractivities = db.jntuh_extracurricularactivities.Where(i => i.isactive && i.activitytype == 14).Select(i => i.sno).ToArray();
            if (masteractivities.Count() > 0)
            {
                var collegeExtracurricularactivities = db.jntuh_college_extracurricularactivities.FirstOrDefault(e => masteractivities.Contains(e.activityid) && e.collegeid == 375);
                if (collegeExtracurricularactivities != null)
                {
                    bookjournalmodel.ActivitySelected = collegeExtracurricularactivities.activitystatus;
                    bookjournalmodel.ActivityDocumentPath = collegeExtracurricularactivities.supportingdocuments;
                    bookjournalmodel.Remarks = collegeExtracurricularactivities.remarks;
                }
            }
            return View("View", bookjournalmodel);
        }

        [Authorize(Roles = "Admin,College")]
        public ActionResult JournalView(string collegeId, string essentialTypeId)
        {
            //return RedirectToAction("Index", "UnderConstruction");
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();

            if (userCollegeID == 0)
            {
                userCollegeID = Convert.ToInt32(Utilities.DecryptString(collegeId, System.Web.Configuration.WebConfigurationManager.AppSettings["CryptoKey"]));
            }
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }

            var status = GetOnlineAppPageEditableStatus(userCollegeID);

            if (status > 0 && Roles.IsUserInRole("College"))
            {
                bool isPageEditable = db.jntuh_pa_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("PBJ") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

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
            int dec_essentialTypeId = Convert.ToInt32(Utilities.DecryptString(essentialTypeId, System.Web.Configuration.WebConfigurationManager.AppSettings["CryptoKey"]));

            var booksJournalsList = db.jntuh_college_booksandjournals.Where(c => c.collegeid == userCollegeID && c.isactive == true && c.essentialtype == dec_essentialTypeId).OrderByDescending(c => c.academicyearId).ToList();
            List<BooksJournals> booksJournalsListObj = new List<BooksJournals>();
            foreach (var item in booksJournalsList)
            {
                BooksJournals booksJournalsObj = new BooksJournals();
                booksJournalsObj.CollegeId = item.collegeid;
                booksJournalsObj.BookandJournalId = item.id;
                booksJournalsObj.DegreeId = db.jntuh_degree.Where(a => a.id == item.degreeid).Select(a => a.degree).FirstOrDefault();
                booksJournalsObj.AcademicYearId = db.jntuh_academic_year.Where(a => a.id == item.academicyearId).Select(a => a.academicYear).FirstOrDefault();
                booksJournalsObj.NumberOfBooks = item.numberofbooks;
                booksJournalsObj.AmountSpent = item.amountspent;
                booksJournalsObj.SupportingDocumentPath = item.supporingdocument;
                booksJournalsObj.EssentialType = Convert.ToString(item.essentialtype);

                booksJournalsListObj.Add(booksJournalsObj);
            }
            ViewBag.BooksJournalsList = booksJournalsListObj;
            ViewBag.TitleType = dec_essentialTypeId == 1 ? "List of Books" : "List of Journals";
            BookandJournalsModel bookjournalmodel = new BookandJournalsModel();
            bookjournalmodel.EssentialType = dec_essentialTypeId == 1 ? "Books" : "Journals";

            ViewBag.ActivityDescription = db.jntuh_extracurricularactivities.Where(e => e.activitytype == 14).Select(e => e.activitydescription).FirstOrDefault();
            var masteractivities = db.jntuh_extracurricularactivities.Where(i => i.isactive && i.activitytype == 14).Select(i => i.sno).ToArray();
            if (masteractivities.Count() > 0)
            {
                var collegeExtracurricularactivities = db.jntuh_college_extracurricularactivities.FirstOrDefault(e => masteractivities.Contains(e.activityid) && e.collegeid == 375);
                if (collegeExtracurricularactivities != null)
                {
                    bookjournalmodel.ActivitySelected = collegeExtracurricularactivities.activitystatus;
                    bookjournalmodel.ActivityDocumentPath = collegeExtracurricularactivities.supportingdocuments;
                    bookjournalmodel.Remarks = collegeExtracurricularactivities.remarks;
                }
            }
            return View("View", bookjournalmodel);
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Edit(string collegeId, string essentialTypeId, string BookandJournalId)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
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
                return RedirectToAction("Create", "BooksandJournals");
            }
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }

            var status = GetOnlineAppPageEditableStatus(userCollegeID);

            if (status == 0 && Roles.IsUserInRole("College"))
            {
                ViewBag.IsEditable = false;
                //return RedirectToAction("View", "BooksandJournals");
                return RedirectToAction("View", "BooksandJournals", new
                {
                    collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString(),
                    essentialTypeId = essentialTypeId
                });
            }
            else
            {
                ViewBag.IsEditable = true;

                bool isPageEditable = db.jntuh_pa_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("PBJ") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

                if (isPageEditable)
                {
                    ViewBag.IsEditable = true;
                }
                else
                {
                    ViewBag.IsEditable = false;
                    //return RedirectToAction("View", "BooksandJournals");
                    return RedirectToAction("View", "BooksandJournals", new
                    {
                        collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString(),
                        essentialTypeId = essentialTypeId
                    });
                }
            }
            int dec_essentialTypeId = Convert.ToInt32(Utilities.DecryptString(essentialTypeId, System.Web.Configuration.WebConfigurationManager.AppSettings["CryptoKey"]));

            var booksJournalsList = db.jntuh_college_booksandjournals.Where(c => c.collegeid == userCollegeID && c.isactive == true && c.essentialtype == dec_essentialTypeId).OrderByDescending(c => c.academicyearId).ToList();
            List<BooksJournals> booksJournalsListObj = new List<BooksJournals>();
            foreach (var item in booksJournalsList)
            {
                BooksJournals booksJournalsObj = new BooksJournals();
                booksJournalsObj.CollegeId = item.collegeid;
                booksJournalsObj.BookandJournalId = item.id;
                booksJournalsObj.DegreeId = db.jntuh_degree.Where(a => a.id == item.degreeid).Select(a => a.degree).FirstOrDefault();
                booksJournalsObj.AcademicYearId = db.jntuh_academic_year.Where(a => a.id == item.academicyearId).Select(a => a.academicYear).FirstOrDefault();
                booksJournalsObj.NumberOfBooks = item.numberofbooks;
                booksJournalsObj.AmountSpent = item.amountspent;
                booksJournalsObj.SupportingDocumentPath = item.supporingdocument;
                booksJournalsObj.EssentialType = Convert.ToString(item.essentialtype);

                booksJournalsListObj.Add(booksJournalsObj);
            }
            ViewBag.BooksJournalsList = booksJournalsListObj;
            ViewBag.TitleType = dec_essentialTypeId == 1 ? "List of Books" : "List of Journals";

            ViewBag.AcademicYears = db.jntuh_academic_year.Where(a => a.isActive && a.actualYear > 2013).OrderByDescending(a => a.actualYear).ToList();

            var degreeId = db.jntuh_college_degree.Where(cd => cd.collegeId == userCollegeID && cd.isActive == true).Select(cd => cd.degreeId).ToList();

            ViewBag.Degrees = db.jntuh_degree.Where(d => degreeId.Contains(d.id)).ToList();

            ViewBag.ActivityDescription = db.jntuh_extracurricularactivities.Where(e => e.activitytype == 14).Select(e => e.activitydescription).FirstOrDefault();

            BookandJournalsModel bookJournalmodel = new BookandJournalsModel();
            if (BookandJournalId != null)
            {
                int dec_bookandJournalId = Convert.ToInt32(Utilities.DecryptString(BookandJournalId, WebConfigurationManager.AppSettings["CryptoKey"]));
                var book_journal = db.jntuh_college_booksandjournals.Find(dec_bookandJournalId);
                bookJournalmodel.CollegeId = book_journal.collegeid;
                bookJournalmodel.BookandJournalId = book_journal.id;
                bookJournalmodel.AcademicYearId = book_journal.academicyearId;
                bookJournalmodel.DegreeId = book_journal.degreeid;
                bookJournalmodel.NumberOfBooks = Convert.ToString(book_journal.numberofbooks);
                bookJournalmodel.AmountSpent = Convert.ToString(book_journal.amountspent);
                bookJournalmodel.SupportingDocumentPath = book_journal.supporingdocument;
                bookJournalmodel.EssentialType = book_journal.essentialtype == 1 ? "Book" : "Journal";
            }
            bookJournalmodel.CollegeId = userCollegeID;
            bookJournalmodel.EssentialType = dec_essentialTypeId == 1 ? "Book" : "Journal";
            var masteractivities = db.jntuh_extracurricularactivities.Where(i => i.isactive && i.activitytype == 14).Select(i => i.sno).ToArray();
            if (masteractivities.Count() > 0)
            {
                var collegeExtracurricularactivities = db.jntuh_college_extracurricularactivities.FirstOrDefault(e => masteractivities.Contains(e.activityid) && e.collegeid == 375);
                bookJournalmodel.ActivityId = 18;
                if (collegeExtracurricularactivities != null)
                {
                    bookJournalmodel.ActivitySelected = collegeExtracurricularactivities.activitystatus;
                    bookJournalmodel.ActivityDocumentPath = collegeExtracurricularactivities.supportingdocuments;
                    bookJournalmodel.Remarks = collegeExtracurricularactivities.remarks;
                }
            }
            return View("Create", bookJournalmodel);
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult BookEdit(string collegeId, string BookandJournalId)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
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
                return RedirectToAction("Create", "BooksandJournals");
            }
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }

            var status = GetOnlineAppPageEditableStatus(userCollegeID);

            if (status == 0 && Roles.IsUserInRole("College"))
            //if (true)
            {
                ViewBag.IsEditable = false;
                //return RedirectToAction("View", "BooksandJournals");
                return RedirectToAction("BookView", "BooksandJournals", new
                {
                    collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString(),
                    essentialTypeId = Utilities.EncryptString("1", WebConfigurationManager.AppSettings["CryptoKey"]).ToString()
                });
            }
            else
            {
                ViewBag.IsEditable = true;

                bool isPageEditable = db.jntuh_pa_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("PBJ") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

                if (isPageEditable)
                {
                    ViewBag.IsEditable = true;
                }
                else
                {
                    ViewBag.IsEditable = false;
                    //return RedirectToAction("View", "BooksandJournals");
                    return RedirectToAction("BookView", "BooksandJournals", new
                    {
                        collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString(),
                        essentialTypeId = Utilities.EncryptString("1", WebConfigurationManager.AppSettings["CryptoKey"]).ToString()
                    });
                }
            }

            var booksJournalsList = db.jntuh_college_booksandjournals.Where(c => c.collegeid == userCollegeID && c.isactive == true && c.essentialtype == 1).OrderByDescending(c => c.academicyearId).ToList();
            List<BooksJournals> booksJournalsListObj = new List<BooksJournals>();
            foreach (var item in booksJournalsList)
            {
                BooksJournals booksJournalsObj = new BooksJournals();
                booksJournalsObj.CollegeId = item.collegeid;
                booksJournalsObj.BookandJournalId = item.id;
                booksJournalsObj.DegreeId = db.jntuh_degree.Where(a => a.id == item.degreeid).Select(a => a.degree).FirstOrDefault();
                booksJournalsObj.AcademicYearId = db.jntuh_academic_year.Where(a => a.id == item.academicyearId).Select(a => a.academicYear).FirstOrDefault();
                booksJournalsObj.NumberOfBooks = item.numberofbooks;
                booksJournalsObj.AmountSpent = item.amountspent;
                booksJournalsObj.SupportingDocumentPath = item.supporingdocument;
                booksJournalsObj.EssentialType = Convert.ToString(item.essentialtype);

                booksJournalsListObj.Add(booksJournalsObj);
            }
            ViewBag.BooksJournalsList = booksJournalsListObj;
            ViewBag.TitleType = "List of Books";

            ViewBag.AcademicYears = db.jntuh_academic_year.Where(a => a.isActive && a.actualYear > 2013).OrderByDescending(a => a.actualYear).ToList();

            var degreeId = db.jntuh_college_degree.Where(cd => cd.collegeId == userCollegeID && cd.isActive == true).Select(cd => cd.degreeId).ToList();

            ViewBag.Degrees = db.jntuh_degree.Where(d => degreeId.Contains(d.id)).ToList();

            ViewBag.ActivityDescription = db.jntuh_extracurricularactivities.Where(e => e.activitytype == 14).Select(e => e.activitydescription).FirstOrDefault();

            BookandJournalsModel bookJournalmodel = new BookandJournalsModel();
            if (BookandJournalId != null)
            {
                int dec_bookandJournalId = Convert.ToInt32(Utilities.DecryptString(BookandJournalId, WebConfigurationManager.AppSettings["CryptoKey"]));
                var book_journal = db.jntuh_college_booksandjournals.Find(dec_bookandJournalId);
                bookJournalmodel.CollegeId = book_journal.collegeid;
                bookJournalmodel.BookandJournalId = book_journal.id;
                bookJournalmodel.AcademicYearId = book_journal.academicyearId;
                bookJournalmodel.DegreeId = book_journal.degreeid;
                bookJournalmodel.NumberOfBooks = Convert.ToString(book_journal.numberofbooks);
                bookJournalmodel.AmountSpent = Convert.ToString(book_journal.amountspent);
                bookJournalmodel.SupportingDocumentPath = book_journal.supporingdocument;
                bookJournalmodel.EssentialType = book_journal.essentialtype == 1 ? "Book" : "Journal";
            }
            bookJournalmodel.CollegeId = userCollegeID;
            bookJournalmodel.EssentialType = "Book";
            var masteractivities = db.jntuh_extracurricularactivities.Where(i => i.isactive && i.activitytype == 14).Select(i => i.sno).ToArray();
            if (masteractivities.Count() > 0)
            {
                var collegeExtracurricularactivities = db.jntuh_college_extracurricularactivities.FirstOrDefault(e => masteractivities.Contains(e.activityid) && e.collegeid == 375);
                bookJournalmodel.ActivityId = 18;
                if (collegeExtracurricularactivities != null)
                {
                    bookJournalmodel.ActivitySelected = collegeExtracurricularactivities.activitystatus;
                    bookJournalmodel.ActivityDocumentPath = collegeExtracurricularactivities.supportingdocuments;
                    bookJournalmodel.Remarks = collegeExtracurricularactivities.remarks;
                }
            }
            return View("Create", bookJournalmodel);
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult JournalEdit(string collegeId, string BookandJournalId)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
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
                return RedirectToAction("Create", "BooksandJournals");
            }
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }

            var status = GetOnlineAppPageEditableStatus(userCollegeID);

            if (status == 0 && Roles.IsUserInRole("College"))
            //if (true)
            {
                ViewBag.IsEditable = false;
                //return RedirectToAction("View", "BooksandJournals");
                return RedirectToAction("JournalView", "BooksandJournals", new
                {
                    collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString(),
                    essentialTypeId = Utilities.EncryptString("2", WebConfigurationManager.AppSettings["CryptoKey"]).ToString()
                });
            }
            else
            {
                ViewBag.IsEditable = true;

                bool isPageEditable = db.jntuh_pa_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("PBJ") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

                if (isPageEditable)
                {
                    ViewBag.IsEditable = true;
                }
                else
                {
                    ViewBag.IsEditable = false;
                    //return RedirectToAction("View", "BooksandJournals");
                    return RedirectToAction("JournalView", "BooksandJournals", new
                    {
                        collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString(),
                        essentialTypeId = Utilities.EncryptString("2", WebConfigurationManager.AppSettings["CryptoKey"]).ToString()
                    });
                }
            }

            var booksJournalsList = db.jntuh_college_booksandjournals.Where(c => c.collegeid == userCollegeID && c.isactive == true && c.essentialtype == 2).OrderByDescending(c => c.academicyearId).ToList();
            List<BooksJournals> booksJournalsListObj = new List<BooksJournals>();
            foreach (var item in booksJournalsList)
            {
                BooksJournals booksJournalsObj = new BooksJournals();
                booksJournalsObj.CollegeId = item.collegeid;
                booksJournalsObj.BookandJournalId = item.id;
                booksJournalsObj.DegreeId = db.jntuh_degree.Where(a => a.id == item.degreeid).Select(a => a.degree).FirstOrDefault();
                booksJournalsObj.AcademicYearId = db.jntuh_academic_year.Where(a => a.id == item.academicyearId).Select(a => a.academicYear).FirstOrDefault();
                booksJournalsObj.NumberOfBooks = item.numberofbooks;
                booksJournalsObj.AmountSpent = item.amountspent;
                booksJournalsObj.SupportingDocumentPath = item.supporingdocument;
                booksJournalsObj.EssentialType = Convert.ToString(item.essentialtype);

                booksJournalsListObj.Add(booksJournalsObj);
            }
            ViewBag.BooksJournalsList = booksJournalsListObj;
            ViewBag.TitleType = "List of Journals";

            ViewBag.AcademicYears = db.jntuh_academic_year.Where(a => a.isActive && a.actualYear > 2013).OrderByDescending(a => a.actualYear).ToList();

            var degreeId = db.jntuh_college_degree.Where(cd => cd.collegeId == userCollegeID && cd.isActive == true).Select(cd => cd.degreeId).ToList();

            ViewBag.Degrees = db.jntuh_degree.Where(d => degreeId.Contains(d.id)).ToList();

            ViewBag.ActivityDescription = db.jntuh_extracurricularactivities.Where(e => e.activitytype == 14).Select(e => e.activitydescription).FirstOrDefault();

            BookandJournalsModel bookJournalmodel = new BookandJournalsModel();
            if (BookandJournalId != null)
            {
                int dec_bookandJournalId = Convert.ToInt32(Utilities.DecryptString(BookandJournalId, WebConfigurationManager.AppSettings["CryptoKey"]));
                var book_journal = db.jntuh_college_booksandjournals.Find(dec_bookandJournalId);
                bookJournalmodel.CollegeId = book_journal.collegeid;
                bookJournalmodel.BookandJournalId = book_journal.id;
                bookJournalmodel.AcademicYearId = book_journal.academicyearId;
                bookJournalmodel.DegreeId = book_journal.degreeid;
                bookJournalmodel.NumberOfBooks = Convert.ToString(book_journal.numberofbooks);
                bookJournalmodel.AmountSpent = Convert.ToString(book_journal.amountspent);
                bookJournalmodel.SupportingDocumentPath = book_journal.supporingdocument;
                bookJournalmodel.EssentialType = book_journal.essentialtype == 1 ? "Book" : "Journal";
            }
            bookJournalmodel.CollegeId = userCollegeID;
            bookJournalmodel.EssentialType = "Journal";
            var masteractivities = db.jntuh_extracurricularactivities.Where(i => i.isactive && i.activitytype == 14).Select(i => i.sno).ToArray();
            if (masteractivities.Count() > 0)
            {
                var collegeExtracurricularactivities = db.jntuh_college_extracurricularactivities.FirstOrDefault(e => masteractivities.Contains(e.activityid) && e.collegeid == 375);
                bookJournalmodel.ActivityId = 18;
                if (collegeExtracurricularactivities != null)
                {
                    bookJournalmodel.ActivitySelected = collegeExtracurricularactivities.activitystatus;
                    bookJournalmodel.ActivityDocumentPath = collegeExtracurricularactivities.supportingdocuments;
                    bookJournalmodel.Remarks = collegeExtracurricularactivities.remarks;
                }
            }
            return View("Create", bookJournalmodel);
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        [HttpPost]
        public ActionResult Edit(BookandJournalsModel bookjournalmodel)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            string collegeCode = db.jntuh_college.Where(u => u.id == userCollegeID).Select(u => u.collegeCode).FirstOrDefault();

            if (bookjournalmodel.BookandJournalId > 0)
            {
                //Update
                var jntuh_bookJournal = db.jntuh_college_booksandjournals.Where(a => a.id == bookjournalmodel.BookandJournalId).Select(a => a).FirstOrDefault();
                jntuh_bookJournal.collegeid = bookjournalmodel.CollegeId;
                jntuh_bookJournal.academicyearId = bookjournalmodel.AcademicYearId;
                jntuh_bookJournal.degreeid = bookjournalmodel.DegreeId;
                jntuh_bookJournal.numberofbooks = Convert.ToInt32(bookjournalmodel.NumberOfBooks);

                //if ((bookjournalmodel.AmountSpent % 1) > 0)
                //    jntuh_bookJournal.amountspent = Convert.ToDecimal(bookjournalmodel.AmountSpent.ToString().Replace(".", ","));
                //else
                //    jntuh_bookJournal.amountspent = bookjournalmodel.AmountSpent;

                jntuh_bookJournal.amountspent = Convert.ToDecimal(bookjournalmodel.AmountSpent);

                if (bookjournalmodel.SupportingDocument != null)
                {
                    string SupportingDocumentfile = "~/Content/Upload/College/BooksandJournals";
                    if (!Directory.Exists(Server.MapPath(SupportingDocumentfile)))
                    {
                        Directory.CreateDirectory(Server.MapPath(SupportingDocumentfile));
                    }
                    var ext = Path.GetExtension(bookjournalmodel.SupportingDocument.FileName);
                    if (ext.ToUpper().Equals(".PDF") || ext.ToUpper().Equals(".PDF"))
                    {
                        if (bookjournalmodel.SupportingDocumentPath == null)
                        {
                            string fileName = collegeCode + "-" + DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" + "JNTUH";
                            bookjournalmodel.SupportingDocument.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(SupportingDocumentfile), fileName, ext));
                            bookjournalmodel.SupportingDocumentPath = string.Format("{0}{1}", fileName, ext);
                            jntuh_bookJournal.supporingdocument = bookjournalmodel.SupportingDocumentPath;
                        }
                        else
                        {
                            bookjournalmodel.SupportingDocument.SaveAs(string.Format("{0}/{1}", Server.MapPath(SupportingDocumentfile), bookjournalmodel.SupportingDocumentPath));
                            jntuh_bookJournal.supporingdocument = bookjournalmodel.SupportingDocumentPath;
                        }
                    }
                }

                jntuh_bookJournal.updatedby = userID;
                jntuh_bookJournal.updatedon = DateTime.Now;

                db.Entry(jntuh_bookJournal).State = EntityState.Modified;
                db.SaveChanges();

                TempData["Success"] = "Updated successfully";
            }
            else
            {
                //Add
                jntuh_college_booksandjournals booksandjournals = new jntuh_college_booksandjournals();
                booksandjournals.collegeid = bookjournalmodel.CollegeId;
                booksandjournals.academicyearId = bookjournalmodel.AcademicYearId;
                booksandjournals.degreeid = bookjournalmodel.DegreeId;
                booksandjournals.numberofbooks = Convert.ToInt32(bookjournalmodel.NumberOfBooks);

                //if ((bookjournalmodel.AmountSpent % 1) > 0)
                //    booksandjournals.amountspent = Convert.ToDecimal(bookjournalmodel.AmountSpent.ToString().Replace(".", ","));
                //else
                //    booksandjournals.amountspent = bookjournalmodel.AmountSpent;
                booksandjournals.amountspent = Convert.ToDecimal(bookjournalmodel.AmountSpent);

                if (bookjournalmodel.SupportingDocument != null)
                {
                    string SupportingDocumentfile = "~/Content/Upload/College/BooksandJournals";
                    if (!Directory.Exists(Server.MapPath(SupportingDocumentfile)))
                    {
                        Directory.CreateDirectory(Server.MapPath(SupportingDocumentfile));
                    }
                    var ext = Path.GetExtension(bookjournalmodel.SupportingDocument.FileName);
                    if (ext.ToUpper().Equals(".PDF") || ext.ToUpper().Equals(".PDF"))
                    {
                        if (bookjournalmodel.SupportingDocumentPath == null)
                        {
                            string fileName = collegeCode + "-" + DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" + "JNTUH";
                            bookjournalmodel.SupportingDocument.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(SupportingDocumentfile), fileName, ext));
                            bookjournalmodel.SupportingDocumentPath = string.Format("{0}{1}", fileName, ext);
                            booksandjournals.supporingdocument = bookjournalmodel.SupportingDocumentPath;
                        }
                        else
                        {
                            bookjournalmodel.SupportingDocument.SaveAs(string.Format("{0}/{1}", Server.MapPath(SupportingDocumentfile), bookjournalmodel.SupportingDocumentPath));
                            booksandjournals.supporingdocument = bookjournalmodel.SupportingDocumentPath;
                        }
                    }
                }
                else
                {
                    booksandjournals.supporingdocument = bookjournalmodel.SupportingDocumentPath;
                }
                booksandjournals.essentialtype = bookjournalmodel.EssentialType == "Book" ? 1 : 2;
                booksandjournals.isactive = true;
                booksandjournals.createdby = userID;
                booksandjournals.createdon = DateTime.Now;

                db.jntuh_college_booksandjournals.Add(booksandjournals);
                db.SaveChanges();

                TempData["Success"] = "Added successfully";
            }
            return RedirectToAction("Edit", "BooksandJournals", new
            {
                collegeId = Utilities.EncryptString(bookjournalmodel.CollegeId.ToString(),
                    WebConfigurationManager.AppSettings["CryptoKey"]).ToString(),
                essentialTypeId = Utilities.EncryptString((bookjournalmodel.EssentialType == "Book" ? 1 : 2).ToString(),
                    WebConfigurationManager.AppSettings["CryptoKey"]).ToString(),
                BookandJournalId = ""
            });
            //return View("View");
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Delete(string collegeId, string essentialTypeId, string BookandJournalId)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);

            int dec_bookJournalId = Convert.ToInt32(Utilities.DecryptString(BookandJournalId, WebConfigurationManager.AppSettings["CryptoKey"]));

            var jntuh_college_bookandjournal = db.jntuh_college_booksandjournals.Where(a => a.id == dec_bookJournalId).FirstOrDefault();
            db.Entry(jntuh_college_bookandjournal).State = EntityState.Deleted;
            db.SaveChanges();

            TempData["Success"] = "Deleted successfully";

            return RedirectToAction("Edit", "BooksandJournals", new
            {
                collegeId = collegeId,
                essentialTypeId = essentialTypeId,
                BookandJournalId = ""
            });
        }

        public ActionResult FileUpload(HttpPostedFileBase fileUploader, BookandJournalsModel bookjournalmodel)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            string collegeCode = db.jntuh_college.Where(u => u.id == userCollegeID).Select(u => u.collegeCode).FirstOrDefault();
            int actualYear =
                db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true)
                    .Select(a => a.actualYear)
                    .FirstOrDefault();
            int ay0 = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            var masteractivities = db.jntuh_extracurricularactivities.Where(i => i.isactive && i.activitytype == 14).Select(i => i.sno).ToArray();
            var collegeExtracurricularactivities = db.jntuh_college_extracurricularactivities.Where(e => masteractivities.Contains(e.activityid) && e.collegeid == 375).FirstOrDefault();

            if (collegeExtracurricularactivities != null && collegeExtracurricularactivities.sno > 0)
            {
                //update
                collegeExtracurricularactivities.academicyear = ay0;
                collegeExtracurricularactivities.activitystatus = bookjournalmodel.ActivitySelected;

                if (bookjournalmodel.ActivitySelected)
                {
                    if (bookjournalmodel.ActivityDocument != null)
                    {
                        string SupportingDocumentfile = "~/Content/Upload/College/BooksandJournals";
                        if (!Directory.Exists(Server.MapPath(SupportingDocumentfile)))
                        {
                            Directory.CreateDirectory(Server.MapPath(SupportingDocumentfile));
                        }
                        var ext = Path.GetExtension(bookjournalmodel.ActivityDocument.FileName);
                        if (ext.ToUpper().Equals(".PDF") || ext.ToUpper().Equals(".PDF"))
                        {
                            if (bookjournalmodel.ActivityDocumentPath == null)
                            {
                                string fileName = collegeCode + "-" + DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" + "feedback" + "-" + "JNTUH";
                                bookjournalmodel.ActivityDocument.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(SupportingDocumentfile), fileName, ext));
                                bookjournalmodel.ActivityDocumentPath = string.Format("{0}{1}", fileName, ext);
                                collegeExtracurricularactivities.supportingdocuments = bookjournalmodel.ActivityDocumentPath;
                            }
                            else
                            {
                                bookjournalmodel.ActivityDocument.SaveAs(string.Format("{0}/{1}", Server.MapPath(SupportingDocumentfile), bookjournalmodel.ActivityDocumentPath));
                                collegeExtracurricularactivities.supportingdocuments = bookjournalmodel.ActivityDocumentPath;
                            }
                        }
                    }
                    collegeExtracurricularactivities.remarks = bookjournalmodel.Remarks;
                }
                else
                {
                    collegeExtracurricularactivities.supportingdocuments = null;
                    collegeExtracurricularactivities.remarks = null;
                }
                collegeExtracurricularactivities.updatedby = userID;
                collegeExtracurricularactivities.updatedon = DateTime.Now;

                db.Entry(collegeExtracurricularactivities).State = EntityState.Modified;

                db.SaveChanges();

                TempData["Success"] = "Updated successfully";
            }
            else
            {
                //add
                jntuh_college_extracurricularactivities clgExtraCurr = new jntuh_college_extracurricularactivities();
                clgExtraCurr.activityid = bookjournalmodel.ActivityId;
                clgExtraCurr.collegeid = userCollegeID;
                clgExtraCurr.academicyear = ay0;
                clgExtraCurr.activitystatus = bookjournalmodel.ActivitySelected;
                clgExtraCurr.isactive = true;
                if (bookjournalmodel.ActivitySelected)
                {
                    if (bookjournalmodel.ActivityDocument != null)
                    {
                        string SupportingDocumentfile = "~/Content/Upload/College/BooksandJournals";
                        if (!Directory.Exists(Server.MapPath(SupportingDocumentfile)))
                        {
                            Directory.CreateDirectory(Server.MapPath(SupportingDocumentfile));
                        }
                        var ext = Path.GetExtension(bookjournalmodel.ActivityDocument.FileName);
                        if (ext.ToUpper().Equals(".PDF") || ext.ToUpper().Equals(".PDF"))
                        {
                            if (bookjournalmodel.ActivityDocumentPath == null)
                            {
                                string fileName = collegeCode + "-" + DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" + "feedback" + "-" + "JNTUH";
                                bookjournalmodel.ActivityDocument.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(SupportingDocumentfile), fileName, ext));
                                bookjournalmodel.ActivityDocumentPath = string.Format("{0}{1}", fileName, ext);
                                clgExtraCurr.supportingdocuments = bookjournalmodel.ActivityDocumentPath;
                            }
                            else
                            {
                                bookjournalmodel.ActivityDocument.SaveAs(string.Format("{0}/{1}", Server.MapPath(SupportingDocumentfile), bookjournalmodel.ActivityDocumentPath));
                                clgExtraCurr.supportingdocuments = bookjournalmodel.ActivityDocumentPath;
                            }
                        }
                    }
                    else
                    {
                        clgExtraCurr.supportingdocuments = bookjournalmodel.ActivityDocumentPath;
                    }

                    clgExtraCurr.remarks = bookjournalmodel.Remarks;
                }
                clgExtraCurr.createdon = DateTime.Now;
                clgExtraCurr.createdby = userID;
                db.jntuh_college_extracurricularactivities.Add(clgExtraCurr);
                db.SaveChanges();

                TempData["Success"] = "Added successfully";
            }

            return RedirectToAction("Edit", "BooksandJournals", new
            {
                collegeId = Utilities.EncryptString(bookjournalmodel.CollegeId.ToString(),
                    WebConfigurationManager.AppSettings["CryptoKey"]).ToString(),
                essentialTypeId = Utilities.EncryptString((bookjournalmodel.EssentialType == "Book" ? 1 : 2).ToString(),
                    WebConfigurationManager.AppSettings["CryptoKey"]).ToString(),
                BookandJournalId = ""
            });
        }

        [Authorize(Roles = "Admin,College")]
        public ActionResult BooksandJournalsView(string id)
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
                bool isPageEditable = db.jntuh_pa_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("PBJ") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

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
            return View(library);
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult EditLibrary(string collegeId)
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
                return RedirectToAction("Create", "BooksandJournals");
            }
            if (collegeLibraryId == 0 && (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry")))
            {
                return RedirectToAction("Create", "BooksandJournals", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
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
                return RedirectToAction("View", "BooksandJournals");
            }
            else
            {
                bool isPageEditable = db.jntuh_pa_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("PBJ") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

                if (isPageEditable)
                {
                    ViewBag.IsEditable = true;
                }
                else
                {
                    ViewBag.IsEditable = false;
                    return RedirectToAction("View", "BooksandJournals");
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
            return View("EditLibrary", library);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult EditLibrary(ICollection<LibraryDetails> libraryDetails, HttpPostedFileBase fileUploader)
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

            return RedirectToAction("BooksandJournalsView");
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
    }

    public class BooksJournals
    {
        public int CollegeId { get; set; }

        public int BookandJournalId { get; set; }

        public string EssentialType { get; set; }

        public string DegreeId { get; set; }

        public string AcademicYearId { get; set; }

        public int NumberOfBooks { get; set; }

        public decimal AmountSpent { get; set; }

        public string SupportingDocumentPath { get; set; }

        public int? NumberOfComputers { get; set; }
    }
}
