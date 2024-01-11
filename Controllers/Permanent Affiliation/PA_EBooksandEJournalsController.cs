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
    public class PA_EBooksandEJournalsController : BaseController
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
                return RedirectToAction("View", "PA_EBooksandEJournals", new
                {
                    collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString(),
                    bookJournalId = ""
                });
            }
            if (userCollegeID > 0 && (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry")))
            {
                return RedirectToAction("Edit", "PA_EBooksandEJournals", new
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

            var status = GetPageEditableStatus(userCollegeID);

            if (status > 0 && Roles.IsUserInRole("College"))
            {
                bool isPageEditable = db.jntuh_pa_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("PEE") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

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

            var booksJournalsList = db.jntuh_college_booksandjournals.Where(c => c.collegeid == userCollegeID && c.isactive == true && c.essentialtype == dec_essentialTypeId).ToList();
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
                booksJournalsObj.NumberOfComputers = item.noofcomputers;

                booksJournalsListObj.Add(booksJournalsObj);
            }
            ViewBag.BooksJournalsList = booksJournalsListObj;
            ViewBag.TitleType = dec_essentialTypeId == 3 ? "List of e-Books" : "List of e-Journals";
            BookandJournalsModel bookjournalmodel = new BookandJournalsModel();
            bookjournalmodel.EssentialType = dec_essentialTypeId == 3 ? "e-Books" : "e-Journals";
            return View(bookjournalmodel);
        }

        [Authorize(Roles = "Admin,College")]
        public ActionResult EBookView(string collegeId, string essentialTypeId)
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

            var status = GetPageEditableStatus(userCollegeID);

            if (status > 0 && Roles.IsUserInRole("College"))
            {
                bool isPageEditable = db.jntuh_pa_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("PEE") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

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

            var booksJournalsList = db.jntuh_college_booksandjournals.Where(c => c.collegeid == userCollegeID && c.isactive == true && c.essentialtype == dec_essentialTypeId).ToList();
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
                booksJournalsObj.NumberOfComputers = item.noofcomputers;

                booksJournalsListObj.Add(booksJournalsObj);
            }
            ViewBag.BooksJournalsList = booksJournalsListObj;
            ViewBag.TitleType = dec_essentialTypeId == 3 ? "List of e-Books" : "List of e-Journals";
            BookandJournalsModel bookjournalmodel = new BookandJournalsModel();
            bookjournalmodel.EssentialType = dec_essentialTypeId == 3 ? "e-Books" : "e-Journals";
            return View("View", bookjournalmodel);
        }

        [Authorize(Roles = "Admin,College")]
        public ActionResult EJournalView(string collegeId, string essentialTypeId)
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

            var status = GetPageEditableStatus(userCollegeID);

            if (status > 0 && Roles.IsUserInRole("College"))
            {
                bool isPageEditable = db.jntuh_pa_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("PEE") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

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

            var booksJournalsList = db.jntuh_college_booksandjournals.Where(c => c.collegeid == userCollegeID && c.isactive == true && c.essentialtype == dec_essentialTypeId).ToList();
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
                booksJournalsObj.NumberOfComputers = item.noofcomputers;

                booksJournalsListObj.Add(booksJournalsObj);
            }
            ViewBag.BooksJournalsList = booksJournalsListObj;
            ViewBag.TitleType = dec_essentialTypeId == 3 ? "List of e-Books" : "List of e-Journals";
            BookandJournalsModel bookjournalmodel = new BookandJournalsModel();
            bookjournalmodel.EssentialType = dec_essentialTypeId == 3 ? "e-Books" : "e-Journals";
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
                return RedirectToAction("Create", "PA_EBooksandEJournals");
            }
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }

            var status = GetPageEditableStatus(userCollegeID);

            if (status == 0 && Roles.IsUserInRole("College"))
            {
                ViewBag.IsEditable = false;
                //return RedirectToAction("View", "BooksandJournals");
                return RedirectToAction("View", "PA_EBooksandEJournals", new
                {
                    collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString(),
                    essentialTypeId = essentialTypeId
                });
            }
            else
            {
                ViewBag.IsEditable = true;

                bool isPageEditable = db.jntuh_pa_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("PEE") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

                if (isPageEditable)
                {
                    ViewBag.IsEditable = true;
                }
                else
                {
                    ViewBag.IsEditable = false;
                    //return RedirectToAction("View", "BooksandJournals");
                    return RedirectToAction("View", "PA_EBooksandEJournals", new
                    {
                        collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString(),
                        essentialTypeId = essentialTypeId
                    });
                }
            }
            int dec_essentialTypeId = Convert.ToInt32(Utilities.DecryptString(essentialTypeId, System.Web.Configuration.WebConfigurationManager.AppSettings["CryptoKey"]));

            var booksJournalsList = db.jntuh_college_booksandjournals.Where(c => c.collegeid == userCollegeID && c.isactive == true && c.essentialtype == dec_essentialTypeId).ToList();
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
                booksJournalsObj.NumberOfComputers = item.noofcomputers;

                booksJournalsListObj.Add(booksJournalsObj);
            }
            ViewBag.BooksJournalsList = booksJournalsListObj;
            ViewBag.TitleType = dec_essentialTypeId == 3 ? "List of e-Books" : "List of e-Journals";

            ViewBag.AcademicYears = db.jntuh_academic_year.Where(a => a.isActive && a.actualYear > 2013).OrderByDescending(a => a.actualYear).ToList();

            var degreeId = db.jntuh_college_degree.Where(cd => cd.collegeId == userCollegeID && cd.isActive == true).Select(cd => cd.degreeId).ToList();

            ViewBag.Degrees = db.jntuh_degree.Where(d => degreeId.Contains(d.id)).ToList();

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
                bookJournalmodel.EssentialType = book_journal.essentialtype == 3 ? "e-Book" : "e-Journal";
            }
            bookJournalmodel.CollegeId = userCollegeID;
            bookJournalmodel.EssentialType = dec_essentialTypeId == 3 ? "e-Book" : "e-Journal";
            return View("Create", bookJournalmodel);
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult EBookEdit(string collegeId, string BookandJournalId)
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
                return RedirectToAction("Create", "PA_EBooksandEJournals");
            }
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }

            var status = GetPageEditableStatus(userCollegeID);

            if (status == 0 && Roles.IsUserInRole("College"))
            //if (true)
            {
                ViewBag.IsEditable = false;
                //return RedirectToAction("View", "BooksandJournals");
                return RedirectToAction("EBookView", "PA_EBooksandEJournals", new
                {
                    collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString(),
                    essentialTypeId = Utilities.EncryptString("3", WebConfigurationManager.AppSettings["CryptoKey"]).ToString()
                });
            }
            else
            {
                ViewBag.IsEditable = true;

                bool isPageEditable = db.jntuh_pa_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("PEE") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

                if (isPageEditable)
                {
                    ViewBag.IsEditable = true;
                }
                else
                {
                    ViewBag.IsEditable = false;
                    //return RedirectToAction("View", "BooksandJournals");
                    return RedirectToAction("EBookView", "PA_EBooksandEJournals", new
                    {
                        collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString(),
                        essentialTypeId = Utilities.EncryptString("3", WebConfigurationManager.AppSettings["CryptoKey"]).ToString()
                    });
                }
            }

            var booksJournalsList = db.jntuh_college_booksandjournals.Where(c => c.collegeid == userCollegeID && c.isactive == true && c.essentialtype == 3).ToList();
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
                booksJournalsObj.NumberOfComputers = item.noofcomputers;

                booksJournalsListObj.Add(booksJournalsObj);
            }
            ViewBag.BooksJournalsList = booksJournalsListObj;
            ViewBag.TitleType = "List of e-Books";

            ViewBag.AcademicYears = db.jntuh_academic_year.Where(a => a.isActive && a.actualYear > 2013).OrderByDescending(a => a.actualYear).ToList();

            var degreeId = db.jntuh_college_degree.Where(cd => cd.collegeId == userCollegeID && cd.isActive == true).Select(cd => cd.degreeId).ToList();

            ViewBag.Degrees = db.jntuh_degree.Where(d => degreeId.Contains(d.id)).ToList();

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
                bookJournalmodel.EssentialType = book_journal.essentialtype == 3 ? "e-Book" : "e-Journal";
                bookJournalmodel.NumberOfComputers = Convert.ToString(book_journal.noofcomputers);
            }
            bookJournalmodel.CollegeId = userCollegeID;
            bookJournalmodel.EssentialType = "e-Book";

            return View("Create", bookJournalmodel);
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult EJournalEdit(string collegeId, string BookandJournalId)
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
                return RedirectToAction("Create", "PA_EBooksandEJournals");
            }
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }

            var status = GetPageEditableStatus(userCollegeID);

            if (status == 0 && Roles.IsUserInRole("College"))
            //if (true)
            {
                ViewBag.IsEditable = false;
                //return RedirectToAction("View", "BooksandJournals");
                return RedirectToAction("EJournalView", "PA_EBooksandEJournals", new
                {
                    collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString(),
                    essentialTypeId = Utilities.EncryptString("4", WebConfigurationManager.AppSettings["CryptoKey"]).ToString()
                });
            }
            else
            {
                ViewBag.IsEditable = true;

                bool isPageEditable = db.jntuh_pa_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("PEE") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

                if (isPageEditable)
                {
                    ViewBag.IsEditable = true;
                }
                else
                {
                    ViewBag.IsEditable = false;
                    //return RedirectToAction("View", "BooksandJournals");
                    return RedirectToAction("EJournalView", "PA_EBooksandEJournals", new
                    {
                        collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString(),
                        essentialTypeId = Utilities.EncryptString("4", WebConfigurationManager.AppSettings["CryptoKey"]).ToString()
                    });
                }
            }

            var booksJournalsList = db.jntuh_college_booksandjournals.Where(c => c.collegeid == userCollegeID && c.isactive == true && c.essentialtype == 4).ToList();
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
                booksJournalsObj.NumberOfComputers = item.noofcomputers;

                booksJournalsListObj.Add(booksJournalsObj);
            }
            ViewBag.BooksJournalsList = booksJournalsListObj;
            ViewBag.TitleType = "List of e-Journals";

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
                bookJournalmodel.EssentialType = book_journal.essentialtype == 3 ? "e-Book" : "e-Journal";
                bookJournalmodel.NumberOfComputers = Convert.ToString(book_journal.noofcomputers);
            }
            bookJournalmodel.CollegeId = userCollegeID;
            bookJournalmodel.EssentialType = "e-Journal";

            return View("Create", bookJournalmodel);
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        [HttpPost]
        public ActionResult Edit(BookandJournalsModel bookjournalmodel)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            string collegeCode = db.jntuh_college.Where(u => u.id == userCollegeID).Select(u => u.collegeCode).FirstOrDefault();

            //Add
            jntuh_college_booksandjournals booksandjournals = new jntuh_college_booksandjournals();
            booksandjournals.collegeid = bookjournalmodel.CollegeId;
            booksandjournals.academicyearId = bookjournalmodel.AcademicYearId;
            booksandjournals.degreeid = bookjournalmodel.DegreeId;
            booksandjournals.numberofbooks = Convert.ToInt32(bookjournalmodel.NumberOfBooks);
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
            booksandjournals.noofcomputers = Convert.ToInt32(bookjournalmodel.NumberOfComputers);
            booksandjournals.essentialtype = bookjournalmodel.EssentialType == "e-Book" ? 3 : 4;
            booksandjournals.isactive = true;
            booksandjournals.createdby = userID;
            booksandjournals.createdon = DateTime.Now;

            db.jntuh_college_booksandjournals.Add(booksandjournals);
            db.SaveChanges();

            TempData["Success"] = "Added successfully";
            return RedirectToAction("Edit", "PA_EBooksandEJournals", new
            {
                collegeId = Utilities.EncryptString(bookjournalmodel.CollegeId.ToString(),
                    WebConfigurationManager.AppSettings["CryptoKey"]).ToString(),
                essentialTypeId = Utilities.EncryptString((bookjournalmodel.EssentialType == "e-Book" ? 3 : 4).ToString(),
                    WebConfigurationManager.AppSettings["CryptoKey"]).ToString(),
                BookandJournalId = ""
            });
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

            return RedirectToAction("Edit", "PA_EBooksandEJournals", new
            {
                collegeId = collegeId,
                essentialTypeId = essentialTypeId,
                BookandJournalId = ""
            });
        }
    }
}
