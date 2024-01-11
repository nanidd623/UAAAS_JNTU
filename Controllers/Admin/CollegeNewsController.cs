using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using UAAAS.Models;
using System.Data;
using System.Data.OleDb;
using System.Configuration;
using System.Text.RegularExpressions;

namespace UAAAS.Controllers
{
    [ErrorHandling]
    public class CollegeNewsController : BaseController
    {
        List<SelectListItem> NewsType = new List<SelectListItem>(){
                new SelectListItem() { Text = "CollegeNews", Value = "1" },
                new SelectListItem() { Text = "FacultyNews", Value = "2" }
            };

        List<SelectListItem> BASNewsType = new List<SelectListItem>(){
                new SelectListItem() { Text = "FacultyNews", Value = "1" },
                new SelectListItem() { Text = "StudentNews", Value = "2" }
            };

        List<SelectListItem> StudentBASNewsType = new List<SelectListItem>(){
                new SelectListItem() { Text = "FirstYear", Value = "1" },
                new SelectListItem() { Text = "SecondYear", Value = "2" },
                new SelectListItem() { Text = "PharmD", Value = "3" }
            };

        List<SelectListItem> FolderType = new List<SelectListItem>(){
                new SelectListItem() { Text = "New Folder", Value = "1" },
                new SelectListItem() { Text = "Existing Folder", Value = "2" }
            };

        private uaaasDBContext db = new uaaasDBContext();

        //
        // GET: /CollegeNews/CollegeNewsIndex
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult CollegeNewsIndex()
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            string collegeName = string.Empty;
            string collegeCode = string.Empty;
            List<CollegeNews> collegeNewsList = new List<CollegeNews>();
            var jntuh_college = db.jntuh_college.AsNoTracking().ToList();
            var collegeIds = jntuh_college.Where(i => i.isActive == true).Select(i => i.id).ToArray();
            var collegeNews = db.jntuh_college_news.Where(i => collegeIds.Contains(i.collegeId)).ToList();
            var jntuh_academic_year = db.jntuh_academic_year.ToList();
            foreach (var item in collegeNews)
            {
                CollegeNews news = new CollegeNews();
                news.id = item.id;
                collegeName = jntuh_college.Where(college => college.id == item.collegeId)
                                              .Select(college => college.collegeName)
                                              .FirstOrDefault();

                collegeCode = jntuh_college.Where(college => college.id == item.collegeId)
                                              .Select(college => college.collegeCode)
                                              .FirstOrDefault();
                if (collegeCode != string.Empty || collegeCode != null)
                {
                    //news.collegeName = collegeName + " " + "(" + collegeCode + ")";
                    news.collegeName = collegeCode + " - " + collegeName.Trim();
                }
                else
                {
                    news.collegeName = collegeCode;
                }
                news.title = item.title;
                if (item.startDate != null)
                {
                    news.startDate = Utilities.MMDDYY2DDMMYY(item.startDate.ToString());
                }
                else
                {
                    news.startDate = string.Empty;
                }
                if (item.endDate != null)
                {
                    news.endDate = Utilities.MMDDYY2DDMMYY(item.endDate.ToString());
                }
                else
                {
                    news.endDate = string.Empty;
                }
                news.navigateURL = item.navigateURL;
                news.isActive = item.isActive;
                news.isLatest = item.isLatest;
                collegeNewsList.Add(news);
            }
            //var collegebasreports = db.jntuh_college_monthlybasreports.Where(r => r.academicyearid == 11 && r.type == 1).ToList();
            //foreach (var basreport in collegebasreports)
            //{
            //    CollegeNews news = new CollegeNews();
            //    news.id = basreport.id;

            //    collegeName = jntuh_college.Where(college => college.id == basreport.collegeid)
            //                                  .Select(college => college.collegeName)
            //                                  .FirstOrDefault();

            //    collegeCode = jntuh_college.Where(college => college.id == basreport.collegeid)
            //                                  .Select(college => college.collegeCode)
            //                                  .FirstOrDefault();
            //    if (collegeCode != string.Empty || collegeCode != null)
            //    {
            //        //news.collegeName = collegeName + " " + "(" + collegeCode + ")";
            //        news.collegeName = collegeCode + " - " + collegeName.Trim();
            //    }
            //    else
            //    {
            //        news.collegeName = collegeCode;
            //    }
            //    news.title = basreport.title;
            //    news.startDate = string.Empty;
            //    news.endDate = string.Empty;
            //    news.academicyear =
            //        jntuh_academic_year.Where(r => r.id == basreport.academicyearid)
            //            .Select(s => s.academicYear)
            //            .FirstOrDefault();
            //    news.navigateURL = basreport.path;
            //    news.isActive = basreport.isactive;
            //    news.isLatest = basreport.islatest;
            //    collegeNewsList.Add(news);
            //}
            collegeNewsList = collegeNewsList.OrderBy(college => college.collegeName).ToList();
            return View("~/Views/Admin/CollegeNewsIndex.cshtml", collegeNewsList);
        }

        //
        // GET: /CollegeNews/CollegeNewsCreate
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult CollegeNewsCreate()
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            CollegeNews collegeNews = new CollegeNews();
            List<CollegeList> collegeDetails = new List<CollegeList>();
            string collegeName = string.Empty;
            string collegeCode = string.Empty;
            var collegeNameList = db.jntuh_college.AsNoTracking().Where(i => i.isActive == true).ToList();
            foreach (var item in collegeNameList)
            {
                CollegeList collegeList = new CollegeList();
                collegeList.collegeId = item.id;
                collegeName = item.collegeName;
                collegeCode = item.collegeCode;
                if (item.collegeCode != null || item.collegeCode != string.Empty)
                {
                    //collegeList.collegeName = (collegeName + " " + "(" + collegeCode + ")").Trim();
                    collegeList.collegeName = collegeCode + " - " + collegeName.Trim();
                }
                else
                {
                    collegeList.collegeName = collegeName.Trim();
                }
                collegeDetails.Add(collegeList);
            }
            ViewBag.Colleges = collegeDetails.OrderBy(college => college.collegeName).ToList();
            return View("~/Views/Admin/CollegeNewsCreate.cshtml", collegeNews);
        }

        //
        // POST: /CollegeNews/CollegeNewsCreate
        [HttpPost]
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult CollegeNewsCreate(CollegeNews collegeNews)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            SaveCollegeNews(collegeNews);
            List<CollegeList> collegeDetails = new List<CollegeList>();
            string collegeName = string.Empty;
            string collegeCode = string.Empty;
            var collegeNameList = db.jntuh_college.ToList();
            foreach (var item in collegeNameList)
            {
                CollegeList collegeList = new CollegeList();
                collegeList.collegeId = item.id;
                collegeName = item.collegeName;
                collegeCode = item.collegeCode;
                if (item.collegeCode != null || item.collegeCode != string.Empty)
                {
                    //collegeList.collegeName = (collegeName + " " + "(" + collegeCode + ")").Trim();
                    collegeList.collegeName = collegeCode + " - " + collegeName.Trim();
                }
                else
                {
                    collegeList.collegeName = collegeName.Trim();
                }
                collegeDetails.Add(collegeList);
            }
            ViewBag.Colleges = collegeDetails.OrderBy(college => college.collegeName).ToList();
            return View("~/Views/Admin/CollegeNewsCreate.cshtml", collegeNews);
        }

        private void SaveCollegeNews(CollegeNews collegeNews)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            if (collegeNews.uploadFile != null)
            {
                // int Id = db.jntuh_college_news.Count() > 0 ? db.jntuh_newsevents.Select(d => d.id).Max() : 0;
                // Id = Id + 1;
                // string RamdomCode = db.jntuh_college_randamcodes.Where(r => r.CollegeId == collegeNews.collegeId).Select(r => r.RandamCode).FirstOrDefault();                
                // Random rnd = new Random();
                //int RandomNumber = rnd.Next(1000, 9999);
                string randID = string.Empty;// RamdomCode; + Id;
                if (!Directory.Exists(Server.MapPath("~/Content/Upload/CollegeNews")))
                {
                    Directory.CreateDirectory(Server.MapPath("~/Content/Upload/CollegeNews"));
                }
                var ext = Path.GetExtension(collegeNews.uploadFile.FileName);
                var fileName = collegeNews.uploadFile.FileName;
                if (ext.ToUpper().Equals(".PDF"))
                {
                    collegeNews.uploadFile.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath("~/Content/Upload/CollegeNews"), randID, fileName));
                    collegeNews.navigateURL = string.Format("{0}/{1}{2}", "/Content/Upload/CollegeNews", randID, fileName);
                }

                if (ext.ToUpper().Equals(".DOC") || ext.ToUpper().Equals(".DOCX"))
                {
                    collegeNews.uploadFile.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath("~/Content/Upload/CollegeNews"), randID, fileName));
                    collegeNews.navigateURL = string.Format("{0}/{1}{2}", "/Content/Upload/CollegeNews", randID, fileName);
                }

                if (ext.ToUpper().Equals(".XLS") || ext.ToUpper().Equals(".XLSX"))
                {
                    collegeNews.uploadFile.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath("~/Content/Upload/CollegeNews"), randID, fileName));
                    collegeNews.navigateURL = string.Format("{0}/{1}{2}", "/Content/Upload/CollegeNews", randID, fileName);
                }
            }

            if (ModelState.IsValid)
            {
                string rowExists = string.Empty;
                if (collegeNews.id == 0)
                {
                    rowExists = db.jntuh_college_news.Where(news => news.title == collegeNews.title && news.collegeId == collegeNews.collegeId)
                                                         .Select(news => news.title)
                                                         .FirstOrDefault();
                }
                else
                {
                    rowExists = db.jntuh_college_news.Where(news => news.title == collegeNews.title && news.id != collegeNews.id && news.collegeId == collegeNews.collegeId)
                                                         .Select(news => news.title)
                                                         .FirstOrDefault();
                }
                if (rowExists == null || rowExists == string.Empty)
                {
                    jntuh_college_news newsDetails = new jntuh_college_news();
                    newsDetails.collegeId = collegeNews.collegeId;
                    newsDetails.title = collegeNews.title;
                    if (collegeNews.id == 0)
                    {
                        newsDetails.navigateURL = collegeNews.navigateURL;
                    }
                    else
                    {
                        if (collegeNews.uploadFile == null && collegeNews.navigateURL == null)
                        {
                            collegeNews.navigateURL = db.jntuh_college_news.Where(news => news.id == collegeNews.id)
                                                                              .Select(news => news.navigateURL)
                                                                              .FirstOrDefault();

                        }
                        newsDetails.navigateURL = collegeNews.navigateURL;

                    }
                    if (collegeNews.startDate != null)
                    {
                        newsDetails.startDate = Utilities.DDMMYY2MMDDYY(collegeNews.startDate);
                    }
                    if (collegeNews.endDate != null)
                    {
                        newsDetails.endDate = Utilities.DDMMYY2MMDDYY(collegeNews.endDate);
                    }
                    newsDetails.isActive = collegeNews.isActive;
                    newsDetails.isLatest = collegeNews.isLatest;

                    if (collegeNews.id == 0)
                    {
                        newsDetails.createdBy = userID;
                        newsDetails.createdOn = DateTime.Now;
                        db.jntuh_college_news.Add(newsDetails);
                        TempData["Success"] = "College News Details are Added successfully";
                    }
                    else
                    {
                        newsDetails.id = collegeNews.id;
                        newsDetails.createdBy = collegeNews.createdBy;
                        newsDetails.createdOn = collegeNews.createdOn;
                        newsDetails.updatedBy = userID;
                        newsDetails.updatedOn = DateTime.Now;
                        db.Entry(newsDetails).State = EntityState.Modified;
                        TempData["Success"] = "College News Details are Updated successfully";
                    }
                    db.SaveChanges();
                }
                else
                {
                    TempData["Error"] = "News Title is already exists. Please enter a different Title.";
                }
            }
        }

        //
        // GET: /CollegeNews/CollegeNewsEdit
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult CollegeNewsEdit(int id)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            CollegeNews collegeNews = new CollegeNews();
            jntuh_college_news collegeNewsDetails = db.jntuh_college_news.Where(news => news.id == id).FirstOrDefault();
            if (collegeNewsDetails != null)
            {
                collegeNews.id = collegeNewsDetails.id;
                collegeNews.collegeId = collegeNewsDetails.collegeId;
                collegeNews.title = collegeNewsDetails.title;
                collegeNews.createdBy = collegeNewsDetails.createdBy;
                collegeNews.createdOn = collegeNewsDetails.createdOn;
                if (collegeNewsDetails.startDate != null)
                {
                    collegeNews.startDate = Utilities.MMDDYY2DDMMYY(collegeNewsDetails.startDate.ToString());
                    string[] stratDate = collegeNews.startDate.Split('/');
                    string day = stratDate[0];
                    if (day.Length == 1)
                    {
                        day = "0" + day;
                    }
                    string month = stratDate[1];
                    if (month.Length == 1)
                    {
                        month = "0" + month;
                    }
                    collegeNews.startDate = day + "/" + month + "/" + stratDate[2];
                }
                else
                {
                    collegeNews.startDate = string.Empty;
                }
                if (collegeNewsDetails.endDate != null)
                {
                    collegeNews.endDate = Utilities.MMDDYY2DDMMYY(collegeNewsDetails.endDate.ToString());
                    string[] endDate = collegeNews.endDate.Split('/');
                    string day = endDate[0];
                    if (day.Length == 1)
                    {
                        day = "0" + day;
                    }
                    string month = endDate[1];
                    if (month.Length == 1)
                    {
                        month = "0" + month;
                    }
                    collegeNews.endDate = day + "/" + month + "/" + endDate[2];
                }
                else
                {
                    collegeNews.endDate = string.Empty;
                }
                collegeNews.navigateURL = collegeNewsDetails.navigateURL;
                if (collegeNewsDetails.navigateURL != null)
                {
                    if (collegeNewsDetails.navigateURL.Length >= 27)
                    {
                        string name = collegeNewsDetails.navigateURL.Substring(0, 28);
                        if (collegeNewsDetails.navigateURL.Substring(0, 28) == "/Content/Upload/CollegeNews/")
                        {
                            collegeNews.navigateURL = string.Empty;
                        }
                    }
                }
                collegeNews.isActive = collegeNewsDetails.isActive;
                collegeNews.isLatest = collegeNewsDetails.isLatest;
            }

            List<CollegeList> collegeDetails = new List<CollegeList>();
            string collegeName = string.Empty;
            string collegeCode = string.Empty;
            var collegeNameList = db.jntuh_college.ToList();
            foreach (var item in collegeNameList)
            {
                CollegeList collegeList = new CollegeList();
                collegeList.collegeId = item.id;
                collegeName = item.collegeName;
                collegeCode = item.collegeCode;
                if (item.collegeCode != null || item.collegeCode != string.Empty)
                {
                    //collegeList.collegeName = (collegeName + " " + "(" + collegeCode + ")").Trim();
                    collegeList.collegeName = collegeCode + " - " + collegeName.Trim();
                }
                else
                {
                    collegeList.collegeName = collegeName.Trim();
                }
                collegeDetails.Add(collegeList);
            }
            ViewBag.Colleges = collegeDetails.OrderBy(college => college.collegeName).ToList();
            return View("~/Views/Admin/CollegeNewsCreate.cshtml", collegeNews);
        }

        //
        // POST: /CollegeNews/CollegeNewsEdit
        [HttpPost]
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult CollegeNewsEdit(CollegeNews collegeNews)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            SaveCollegeNews(collegeNews);
            List<CollegeList> collegeDetails = new List<CollegeList>();
            string collegeName = string.Empty;
            string collegeCode = string.Empty;
            var collegeNameList = db.jntuh_college.ToList();
            foreach (var item in collegeNameList)
            {
                CollegeList collegeList = new CollegeList();
                collegeList.collegeId = item.id;
                collegeName = item.collegeName;
                collegeCode = item.collegeCode;
                if (item.collegeCode != null || item.collegeCode != string.Empty)
                {
                    //collegeList.collegeName = (collegeName + " " + "(" + collegeCode + ")").Trim();
                    collegeList.collegeName = collegeCode + " - " + collegeName.Trim();
                }
                else
                {
                    collegeList.collegeName = collegeName.Trim();
                }
                collegeDetails.Add(collegeList);
            }
            ViewBag.Colleges = collegeDetails.OrderBy(college => college.collegeName).ToList();
            return View("~/Views/Admin/CollegeNewsCreate.cshtml", collegeNews);
        }

        [Authorize(Roles = "Admin")]
        public ActionResult BulkNewsInsertion()
        {
            BulkCollegesNews multipleNews = new BulkCollegesNews();
            ViewBag.NewsType = NewsType;
            ViewBag.FolderType = FolderType;
            var Foldername = Directory.GetDirectories(Server.MapPath("~/Content/Upload/CollegeNews/")).Select(Path.GetFileName).ToArray();
            List<SelectListItem> SubDirectoryName = new List<SelectListItem>();
            foreach (var item in Foldername)
                SubDirectoryName.Add(new SelectListItem() { Text = item, Value = item });
            ViewBag.SubDirectoryFolderNames = SubDirectoryName;
            var FacultyFoldername = Directory.GetDirectories(Server.MapPath("~/Content/Upload/FacultyNews/")).Select(Path.GetFileName).ToArray();
            List<SelectListItem> FacultySubDirectoryName = new List<SelectListItem>();
            foreach (var item in FacultyFoldername)
                FacultySubDirectoryName.Add(new SelectListItem() { Text = item, Value = item });
            ViewBag.FacultySubDirectoryFolderNames = FacultySubDirectoryName;
            return View("~/Views/Admin/BulkNewsInsertion.cshtml", multipleNews);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult BulkNewsInsertion(BulkCollegesNews faculty)
        {
            int userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int SuccessCount = 0;
            ViewBag.NewsType = NewsType;
            ViewBag.FolderType = FolderType;

            var Foldername = Directory.GetDirectories(Server.MapPath("~/Content/Upload/CollegeNews/")).Select(Path.GetFileName).ToArray();
            List<SelectListItem> SubDirectoryName = new List<SelectListItem>();
            foreach (var item in Foldername)
                SubDirectoryName.Add(new SelectListItem() { Text = item, Value = item });
            ViewBag.SubDirectoryFolderNames = SubDirectoryName;
            var FacultyFoldername = Directory.GetDirectories(Server.MapPath("~/Content/Upload/FacultyNews/")).Select(Path.GetFileName).ToArray();
            List<SelectListItem> FacultySubDirectoryName = new List<SelectListItem>();
            foreach (var item in FacultyFoldername)
                FacultySubDirectoryName.Add(new SelectListItem() { Text = item, Value = item });
            ViewBag.FacultySubDirectoryFolderNames = FacultySubDirectoryName;

            if (ModelState.IsValid)
            {
                string FilePath = string.Empty;
                if (!string.IsNullOrEmpty(faculty.Folder) || !string.IsNullOrEmpty(faculty.FolderName))
                {
                    List<FailureClass> Failures = new List<FailureClass>();
                    if (faculty.NewsType == "1")
                    {
                        FilePath = "~/Content/Upload/CollegeNews";
                        if (!String.IsNullOrEmpty(faculty.Folder))
                            FilePath += "/" + faculty.Folder;
                        if (!String.IsNullOrEmpty(faculty.FolderName))
                            FilePath += "/" + faculty.FolderName;


                        if (!Directory.Exists(Server.MapPath(FilePath)))
                            Directory.CreateDirectory(Server.MapPath(FilePath));

                        foreach (HttpPostedFileBase file in faculty.files)
                        {
                            //Checking file is available to save.  
                            if (file != null)
                            {
                                try
                                {
                                    var InputFileName = Path.GetFileName(file.FileName);
                                    var ServerSavePath = string.Format("{0}/{1}", Server.MapPath(FilePath), InputFileName);
                                    string navigationUrl = FilePath + "/" + InputFileName;
                                    var navigationUrllength = navigationUrl.Length;

                                    string CC = InputFileName.Substring(0, 2);
                                    int collegeid = db.jntuh_college.Where(q => q.collegeCode == CC && q.isActive == true).Select(e => e.id).FirstOrDefault();

                                    if (collegeid != 0)
                                    {
                                        var rowsExists = db.jntuh_college_news.Where(q => q.title == faculty.title && q.collegeId == collegeid).Select(w => w.title).FirstOrDefault();
                                        if (string.IsNullOrEmpty(rowsExists))
                                        {
                                            //Save file to server folder  
                                            file.SaveAs(ServerSavePath);

                                            jntuh_college_news news = new jntuh_college_news();
                                            news.title = faculty.title.Trim();
                                            news.collegeId = collegeid;
                                            news.navigateURL = navigationUrl.Substring(1, (navigationUrllength - 1));
                                            news.isActive = faculty.isActive;
                                            news.isLatest = faculty.isLatest;
                                            news.createdOn = DateTime.Now;
                                            news.createdBy = userId;
                                            db.jntuh_college_news.Add(news);
                                            db.SaveChanges();
                                            SuccessCount++;
                                        }
                                        else
                                        {
                                            FailureClass FailureItem = new FailureClass();
                                            FailureItem.ItemName = file.FileName;
                                            FailureItem.reason = "Already Exists.";
                                            Failures.Add(FailureItem);
                                        }
                                    }
                                    else
                                    {
                                        FailureClass FailureItem = new FailureClass();
                                        FailureItem.ItemName = file.FileName;
                                        FailureItem.reason = "College doesn't Exists.";
                                        Failures.Add(FailureItem);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    FailureClass FailureItem = new FailureClass();
                                    FailureItem.ItemName = file.FileName;
                                    FailureItem.reason = ex.GetType().FullName.ToString();
                                    Failures.Add(FailureItem);
                                    continue;
                                }
                            }
                        }
                        //assigning file uploaded status to ViewBag for showing message to user.  
                        TempData["Success"] = SuccessCount.ToString() + " files uploaded successfully.";
                    }
                    else if (faculty.NewsType == "2")
                    {
                        FilePath = "~/Content/Upload/FacultyNews";
                        if (!String.IsNullOrEmpty(faculty.Folder))
                            FilePath += "/" + faculty.Folder;
                        if (!String.IsNullOrEmpty(faculty.FolderName))
                            FilePath += "/" + faculty.FolderName;

                        if (!Directory.Exists(Server.MapPath(FilePath)))
                            Directory.CreateDirectory(Server.MapPath(FilePath));

                        foreach (HttpPostedFileBase file in faculty.files)
                        {
                            //Checking file is available to save.  
                            if (file != null)
                            {
                                try
                                {
                                    var InputFileName = Path.GetFileName(file.FileName);
                                    var ServerSavePath = string.Format("{0}/{1}", Server.MapPath(FilePath), InputFileName);
                                    string navigationUrl = FilePath + "/" + InputFileName;
                                    var navigationUrllength = navigationUrl.Length;

                                    string substringregnos = InputFileName.Substring(0, 18);
                                    int AlphabentsCount = Regex.Matches(substringregnos, @"[a-zA-Z]").Count;
                                    substringregnos = AlphabentsCount == 0 ? substringregnos : InputFileName.Substring(0, 15);
                                    int AlphabentsCount2 = Regex.Matches(substringregnos, @"[a-zA-Z]").Count;
                                    substringregnos = AlphabentsCount2 == 0 ? substringregnos : InputFileName.Substring(0, 15);

                                    int Fid = db.jntuh_registered_faculty.Where(q => q.RegistrationNumber == substringregnos.Trim()).Select(e => e.id).FirstOrDefault();
                                    if (Fid != 0)
                                    {
                                        var rowsExists = db.jntuh_faculty_news.Where(q => q.title == faculty.title && q.facultyId == Fid).Select(w => w.title).FirstOrDefault();
                                        if (string.IsNullOrEmpty(rowsExists))
                                        {
                                            //Save file to server folder  
                                            file.SaveAs(ServerSavePath);

                                            jntuh_faculty_news news = new jntuh_faculty_news();
                                            news.isNews = false;
                                            news.title = faculty.title;
                                            news.facultyId = Fid;
                                            news.navigateURL = navigationUrl.Substring(1, (navigationUrllength - 1)); ;
                                            news.isActive = faculty.isActive;
                                            news.isLatest = faculty.isLatest;
                                            news.createdOn = DateTime.Now;
                                            news.createdBy = userId;
                                            db.jntuh_faculty_news.Add(news);
                                            db.SaveChanges();
                                            SuccessCount++;
                                        }
                                        else
                                        {
                                            FailureClass FailureItem = new FailureClass();
                                            FailureItem.ItemName = file.FileName;
                                            FailureItem.reason = "Already Exists";
                                            Failures.Add(FailureItem);
                                        }
                                    }
                                    else
                                    {
                                        FailureClass FailureItem = new FailureClass();
                                        FailureItem.ItemName = file.FileName;
                                        FailureItem.reason = "Registration Number is doesn't Exists.";
                                        Failures.Add(FailureItem);
                                    }

                                    TempData["Success"] = SuccessCount.ToString() + " files uploaded successfully.";
                                }
                                catch (Exception ex)
                                {
                                    FailureClass FailureItem = new FailureClass();
                                    FailureItem.ItemName = file.FileName;
                                    FailureItem.reason = ex.GetType().FullName.ToString();
                                    Failures.Add(FailureItem);
                                    continue;
                                }
                            }
                        }
                    }
                    ViewBag.FailureItems = Failures;
                }
                else
                {
                    TempData["Error"] = "Folder Name is Missed, Mention The Folder Name.";
                }
            }
            else
            {
                TempData["Error"] = "Fill the all Reqired Fields.";
            }

            return View("~/Views/Admin/BulkNewsInsertion.cshtml");
        }

        [HttpPost]
        public JsonResult CheckFolderName(string FolderName, string NewsType, string FolderType)
        {
            if (!string.IsNullOrEmpty(FolderName) && !string.IsNullOrEmpty(NewsType) && !string.IsNullOrEmpty(FolderType))
            {
                bool status = false;
                if (NewsType == "1" && FolderType == "1")
                    status = Directory.Exists(Server.MapPath("~/Content/Upload/CollegeNews/" + FolderName));
                else if (NewsType == "2" && FolderType == "1")
                    status = Directory.Exists(Server.MapPath("~/Content/Upload/FacultyNews/" + FolderName));

                if (status == true)
                    return Json("Folder Name is already Exists. Try to Change The Folder Name", JsonRequestBehavior.AllowGet);
                else
                    return Json(true);
            }
            else
            {
                return Json(true);
            }
        }

        [Authorize(Roles = "Admin")]
        public ActionResult BulkNewsInsertionWithoutFiles()
        {
            BulkCollegesNews multipleNews = new BulkCollegesNews();
            // ViewBag.NewsType = NewsType;
            // ViewBag.FolderType = FolderType;
            return View("~/Views/Admin/BulkNewsInsertionWithoutFiles.cshtml", multipleNews);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult BulkNewsInsertionWithoutFiles(BulkCollegesNews News)
        {
            int userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int SuccessCount = 0;
            string excelpath = string.Empty;
            ViewBag.NewsType = NewsType;
            ViewBag.FolderType = FolderType;
            if (News.ExcelFile != null)
            {
                if (News.NewsType == "1")
                    excelpath = "~/Content/Upload/CollegeNews";
                else if (News.NewsType == "2")
                    excelpath = "~/Content/Upload/FacultyNews";

                if (!Directory.Exists(Server.MapPath(excelpath)))
                    Directory.CreateDirectory(Server.MapPath(excelpath));

                var ext = Path.GetExtension(News.ExcelFile.FileName);
                if (ext.ToUpper().Equals(".XLS") || ext.ToUpper().Equals(".XLSX"))
                {
                    FileInfo fs = new FileInfo(Path.Combine(Server.MapPath(excelpath), News.ExcelFile.FileName));
                    if (fs.Exists)
                    {
                        fs.Delete();
                    }
                    News.ExcelFile.SaveAs(string.Format("{0}/{1}", Server.MapPath(excelpath), News.ExcelFile.FileName));
                }

                #region Excel File Code
                List<string> ColumnList = new List<string>();
                List<FailureClass> Failures = new List<FailureClass>();
                string extension = Path.GetExtension(News.ExcelFile.FileName);
                string conString = string.Empty;
                switch (extension)
                {
                    case ".xls":
                        conString = ConfigurationManager.ConnectionStrings["Excel03ConString"].ConnectionString;
                        break;
                    case ".xlsx":
                        conString = ConfigurationManager.ConnectionStrings["Excel07ConString"].ConnectionString;
                        break;
                }
                var filePath = string.Format("{0}/{1}", Server.MapPath(excelpath), News.ExcelFile.FileName);
                conString = string.Format(conString, filePath);
                using (OleDbConnection connection = new OleDbConnection(conString))
                {
                    connection.Open();
                    OleDbCommand command = new OleDbCommand("select * from [Sheet1$]", connection);
                    using (OleDbDataReader dr = command.ExecuteReader())
                    {
                        if (dr.HasRows)
                        {
                            while (dr.Read())
                            {
                                try
                                {
                                    if (!string.IsNullOrEmpty(dr[1].ToString()))
                                    {
                                        var row1Col0 = dr[1];
                                        ColumnList.Add(row1Col0.ToString().Trim());
                                        continue;
                                    }
                                }
                                catch (Exception)
                                {
                                    //FailureColumnList.Add(dr[1].ToString().Trim());
                                    continue;
                                }
                            }
                        }
                    }
                }
                #endregion

                if (News.NewsType == "1")
                {
                    // var CollegeIds = db.jntuh_college.Where(q => ColumnList.Contains(q.collegeCode) && q.isActive == true).Select(e => e.id).ToList();
                    foreach (var item in ColumnList)
                    {
                        int collegeid = db.jntuh_college.Where(q => q.collegeCode == item && q.isActive == true).Select(e => e.id).FirstOrDefault();
                        try
                        {
                            if (collegeid != 0)
                            {
                                var rowsExists = db.jntuh_college_news.Where(q => q.title == News.title && q.collegeId == collegeid).Select(w => w.title).FirstOrDefault();
                                if (string.IsNullOrEmpty(rowsExists))
                                {
                                    jntuh_college_news news = new jntuh_college_news();
                                    news.title = News.title.Trim();
                                    news.collegeId = collegeid;
                                    news.navigateURL = null;
                                    news.isActive = News.isActive;
                                    news.isLatest = News.isLatest;
                                    news.createdOn = DateTime.Now;
                                    news.createdBy = userId;
                                    db.jntuh_college_news.Add(news);
                                    db.SaveChanges();
                                    SuccessCount++;
                                }
                                else
                                {
                                    FailureClass FailureItem = new FailureClass();
                                    FailureItem.ItemName = item;
                                    FailureItem.reason = "Already Exists.";
                                    Failures.Add(FailureItem);
                                }
                            }
                            else
                            {
                                FailureClass FailureItem = new FailureClass();
                                FailureItem.ItemName = item;
                                FailureItem.reason = "College doesn't Exists.";
                                Failures.Add(FailureItem);
                            }
                        }
                        catch (Exception ex)
                        {
                            FailureClass FailureItem = new FailureClass();
                            FailureItem.ItemName = item;
                            FailureItem.reason = ex.GetType().FullName.ToString();
                            Failures.Add(FailureItem);
                            continue;
                        }
                    }
                    TempData["Success"] = SuccessCount.ToString() + "Rows are Added successfully.";
                }
                else if (News.NewsType == "2")
                {
                    foreach (var item in ColumnList)
                    {
                        try
                        {
                            int Fid = db.jntuh_registered_faculty.Where(q => q.RegistrationNumber == item.Trim()).Select(e => e.id).FirstOrDefault();
                            if (Fid != 0)
                            {
                                var rowsExists = db.jntuh_faculty_news.Where(q => q.title == News.title && q.facultyId == Fid).Select(w => w.title).FirstOrDefault();
                                if (string.IsNullOrEmpty(rowsExists))
                                {
                                    jntuh_faculty_news news = new jntuh_faculty_news();
                                    news.isNews = false;
                                    news.title = News.title;
                                    news.facultyId = Fid;
                                    news.navigateURL = null;
                                    news.isActive = News.isActive;
                                    news.isLatest = News.isLatest;
                                    news.createdOn = DateTime.Now;
                                    news.createdBy = userId;
                                    db.jntuh_faculty_news.Add(news);
                                    db.SaveChanges();
                                    SuccessCount++;
                                }
                                else
                                {
                                    FailureClass FailureItem = new FailureClass();
                                    FailureItem.ItemName = item;
                                    FailureItem.reason = "Already Exists";
                                    Failures.Add(FailureItem);
                                }
                            }
                            else
                            {
                                FailureClass FailureItem = new FailureClass();
                                FailureItem.ItemName = item;
                                FailureItem.reason = "Registration Number is doesn't Exists.";
                                Failures.Add(FailureItem);
                            }
                        }
                        catch (Exception ex)
                        {
                            FailureClass FailureItem = new FailureClass();
                            FailureItem.ItemName = item;
                            FailureItem.reason = ex.GetType().FullName.ToString();
                            Failures.Add(FailureItem);
                            continue;
                        }
                    }
                    TempData["Success"] = SuccessCount.ToString() + "Rows are Added successfully.";
                }
                ViewBag.FailureItems = Failures;
            }
            else
            {
                TempData["Error"] = "Uploaded File is Missed.Try Again.";
            }

            return View("~/Views/Admin/BulkNewsInsertionWithoutFiles.cshtml");
        }

        //[HttpPost]
        //[Authorize(Roles = "Admin")]
        //public ActionResult CopyFile(CopyFileClass faculty)
        //{
        //    if (faculty.RegNosExcel != null && !string.IsNullOrEmpty(faculty.ExcelFileFolderpath) && !string.IsNullOrEmpty(faculty.FilesCopiedpath))
        //    {
        //        string excelpath = "~/Content/Upload/LettersUploadfromAdmin";
        //        if (!Directory.Exists(Server.MapPath(excelpath)))
        //            Directory.CreateDirectory(Server.MapPath(excelpath));

        //        var ext = Path.GetExtension(faculty.RegNosExcel.FileName);
        //        if (ext.ToUpper().Equals(".XLS") || ext.ToUpper().Equals(".XLSX"))
        //        {
        //            FileInfo fs = new FileInfo(Path.Combine(Server.MapPath(excelpath), faculty.RegNosExcel.FileName));
        //            if (fs.Exists)
        //            {
        //                fs.Delete();
        //            }
        //            faculty.RegNosExcel.SaveAs(string.Format("{0}/{1}", Server.MapPath(excelpath), faculty.RegNosExcel.FileName));
        //        }


        //        //string excelConnectionString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + Path.Combine(Server.MapPath(excelpath), faculty.RegNosExcel.FileName) + ";Extended Properties=Excel 12.0;Persist Security Info=False";
        //        ////Create Connection to Excel work book                   
        //        //OleDbConnection excelConnection = new OleDbConnection(excelConnectionString);
        //        //excelConnection.Close();
        //        ////Create OleDbCommand to fetch data from Excel
        //        //excelConnection.Open();
        //        //OleDbCommand cmd1 = new OleDbCommand("select count(*) from [" + excelConnection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null).Rows[0]["Table_Name"].ToString() + "]", excelConnection);
        //        //var rows = (int)cmd1.ExecuteScalar();
        //        //OleDbCommand cmd2 = new OleDbCommand("select * from [" + excelConnection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null).Rows[0]["Table_Name"].ToString() + "]", excelConnection);

        //        //DataTable dt = new DataTable();
        //        //OleDbDataAdapter oleda = new OleDbDataAdapter();
        //        //oleda.SelectCommand = cmd2;
        //        //DataSet ds = new DataSet();
        //        //oleda.Fill(ds);
        //        //dt = ds.Tables[0];
        //        //List<string> FilesPathList = new List<string>();
        //        //List<string> FailureRegNos = new List<string>();
        //        //for (int i = 0; i < dt.Rows.Count; i++)
        //        //{
        //        //    try
        //        //    {
        //        //        if (!string.IsNullOrEmpty(dt.Rows[i][1].ToString()))
        //        //        {
        //        //            FilesPathList.Add(dt.Rows[i][1].ToString().Trim());
        //        //            continue;
        //        //        }
        //        //    }
        //        //    catch (Exception)
        //        //    {
        //        //        FailureRegNos.Add(dt.Rows[i][0].ToString().Trim());
        //        //        continue;
        //        //    }
        //        //}

        //        //excelConnection.Close();


        //        #region Excel File Code
        //        List<string> PathsList = new List<string>();
        //        List<string> FailurePathsList = new List<string>();
        //        string extension = Path.GetExtension(faculty.RegNosExcel.FileName);
        //        string conString = string.Empty;
        //        switch (extension)
        //        {
        //            case ".xls":
        //                conString = ConfigurationManager.ConnectionStrings["Excel03ConString"].ConnectionString;
        //                break;
        //            case ".xlsx":
        //                conString = ConfigurationManager.ConnectionStrings["Excel07ConString"].ConnectionString;
        //                break;
        //        }
        //        var filePath = Path.Combine(Server.MapPath(excelpath), faculty.RegNosExcel.FileName);
        //        conString = string.Format(conString, filePath);
        //        using (OleDbConnection connection = new OleDbConnection(conString))
        //        {
        //            connection.Open();
        //            OleDbCommand command = new OleDbCommand("select * from [Sheet1$]", connection);
        //            using (OleDbDataReader dr = command.ExecuteReader())
        //            {
        //                if (dr.HasRows)
        //                {
        //                    while (dr.Read())
        //                    {
        //                        try
        //                        {
        //                            if (!string.IsNullOrEmpty(dr[1].ToString()))
        //                            {
        //                                var row1Col0 = dr[1];
        //                                PathsList.Add(row1Col0.ToString());
        //                                continue;
        //                            }
        //                        }
        //                        catch (Exception)
        //                        {
        //                            FailurePathsList.Add(dr[1].ToString().Trim());
        //                            continue;
        //                        }
        //                    }
        //                }
        //            }
        //        }

        //        foreach (HttpPostedFileBase file in faculty.files)
        //        {
        //            //Checking file is available to save.  
        //            if (file != null)
        //            {
        //                var InputFileName = Path.GetFileName(file.FileName);
        //                var ServerSavePath = Path.Combine(Server.MapPath("~/Content/Upload/CollegeNews/") + InputFileName);
        //                //Save file to server folder  
        //                file.SaveAs(ServerSavePath);
        //                //assigning file uploaded status to ViewBag for showing message to user.  
        //                ViewBag.UploadStatus = faculty.files.Count().ToString() + " files uploaded successfully.";
        //            }

        //        } 

        //       // string IPAddress = GetIPAddress();
        //       //// string FileCopiedPath = @"\\10.10.10.5\D:\JNTUH\Prod\Content\Upload\FilesCopy";
        //       // string FileCopiedPath = "~/" + faculty.FilesCopiedpath;
        //       // if (!Directory.Exists(Server.MapPath(FileCopiedPath)))
        //       //     Directory.CreateDirectory(Server.MapPath(FileCopiedPath));
        //       // foreach (var item in PathsList)
        //       // {
        //       //     //var local = @"\\" + IPAddress + faculty.ExcelFileFolderpath + item;                 
        //       //     var local = faculty.ExcelFileFolderpath + @"\" + item;
        //       //    // local = @"\\10.10.10.91\C:\\Testforfilecopy\" + item;
        //       //     FileInfo f = new FileInfo(local);
        //       //     if (System.IO.File.Exists(local))
        //       //     {
        //       //         //FileCopiedPath = @"\\10.10.10.5\D:\JNTUH\Prod\Content\Upload\FilesCopy\";
        //       //         //string FileCopiedPathwithFilename = FileCopiedPath + item;
        //       //         string FileCopiedPathwithFilename = FileCopiedPath + "/" + item;
        //       //         using (System.IO.File.Create(Server.MapPath(FileCopiedPathwithFilename))) ;
        //       //         byte[] bytes = System.IO.File.ReadAllBytes(local);
        //       //         System.IO.File.WriteAllBytes(Server.MapPath(FileCopiedPathwithFilename), bytes);
        //       //     }
        //       // }
        //        TempData["Success"] = "Files Copied Process is Completed.";
        //        #endregion
        //    }
        //    TempData["Error"] = "Some thing Went Wrong.Please Try Again.";
        //    return View();
        //}

        [Authorize(Roles = "Admin")]
        public ActionResult BASBulkNewsInsertion()
        {
            BulkCollegesNews multipleNews = new BulkCollegesNews();
            return View("~/Views/Admin/BASBulkNewsInsertion.cshtml", multipleNews);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult BASBulkNewsInsertion(BulkCollegesNews faculty)
        {
            int userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int SuccessCount = 0;
            int Type = 0;
            ViewBag.BASNewsType = BASNewsType;
            ViewBag.StudentBASNewsType = StudentBASNewsType;
            ViewBag.FolderType = FolderType;
            if (ModelState.IsValid)
            {
                string FilePath = string.Empty;
                if (!string.IsNullOrEmpty(faculty.FolderName))
                {
                    List<FailureClass> Failures = new List<FailureClass>();
                    var jntuh_academic_year = db.jntuh_academic_year.ToList();
                    var ActualYear = jntuh_academic_year.Where(w => w.isActive == true && w.isPresentAcademicYear == true).Select(t => t.actualYear).FirstOrDefault();
                    var PresentYearId = jntuh_academic_year.Where(w => w.actualYear == (ActualYear + 1)).Select(t => t.id).FirstOrDefault();
                    var CurrentYear = jntuh_academic_year.Where(w => w.actualYear == (ActualYear + 1)).Select(t => t.actualYear).FirstOrDefault();

                    if (faculty.NewsType == "1")
                    {
                        FilePath = "~/Content/PDFReports/BASNotice/Faculty/" + CurrentYear + "/" + faculty.FolderName;
                        Type = 1;
                    }
                    else if (faculty.NewsType == "2")
                    {
                        if (faculty.StudentBASNewsType == "1")
                        {
                            FilePath = "~/Content/PDFReports/BASNotice/Student/" + CurrentYear + "/" + faculty.FolderName + "/" + "FirstYear";
                            Type = 3;
                        }
                        else if (faculty.StudentBASNewsType == "2")
                        {
                            FilePath = "~/Content/PDFReports/BASNotice/Student/" + CurrentYear + "/" + faculty.FolderName + "/" + "SecondYear";
                            Type = 2;
                        }
                        else if (faculty.StudentBASNewsType == "3")
                        {
                            FilePath = "~/Content/PDFReports/BASNotice/Student/" + CurrentYear + "/" + faculty.FolderName + "/" + "PharmD";
                            Type = 4;
                        }
                    }

                    if (!Directory.Exists(Server.MapPath(FilePath)))
                        Directory.CreateDirectory(Server.MapPath(FilePath));

                    foreach (HttpPostedFileBase file in faculty.files)
                    {
                        //Checking file is available to save.  
                        if (file != null)
                        {
                            try
                            {
                                var InputFileName = Path.GetFileName(file.FileName);
                                var ServerSavePath = string.Format("{0}/{1}", Server.MapPath(FilePath), InputFileName);
                                string navigationUrl = FilePath + "/" + InputFileName;
                                var navigationUrllength = navigationUrl.Length;

                                string CC = InputFileName.Substring(0, 2);
                                int collegeid = db.jntuh_college.Where(q => q.collegeCode == CC && q.isActive == true).Select(e => e.id).FirstOrDefault();

                                if (collegeid != 0)
                                {
                                    var rowsExists = db.jntuh_college_monthlybasreports.Where(q => q.academicyearid == PresentYearId && q.title == faculty.title && q.collegeid == collegeid).Select(w => w.title).FirstOrDefault();
                                    if (string.IsNullOrEmpty(rowsExists))
                                    {
                                        //Save file to server folder  
                                        file.SaveAs(ServerSavePath);

                                        jntuh_college_monthlybasreports news = new jntuh_college_monthlybasreports();
                                        news.collegeid = collegeid;
                                        news.academicyearid = PresentYearId;
                                        news.title = faculty.title.Trim();
                                        news.type = Type;
                                        news.path = navigationUrl.Substring(1, (navigationUrllength - 1));
                                        news.isactive = faculty.isActive;
                                        news.islatest = faculty.isLatest;
                                        news.createdon = DateTime.Now;
                                        news.createdby = userId;
                                        db.jntuh_college_monthlybasreports.Add(news);
                                        db.SaveChanges();
                                        SuccessCount++;
                                    }
                                    else
                                    {
                                        FailureClass FailureItem = new FailureClass();
                                        FailureItem.ItemName = file.FileName;
                                        FailureItem.reason = "Already Exists.";
                                        Failures.Add(FailureItem);
                                    }
                                }
                                else
                                {
                                    FailureClass FailureItem = new FailureClass();
                                    FailureItem.ItemName = file.FileName;
                                    FailureItem.reason = "College doesn't Exists.";
                                    Failures.Add(FailureItem);
                                }
                            }
                            catch (Exception ex)
                            {
                                FailureClass FailureItem = new FailureClass();
                                FailureItem.ItemName = file.FileName;
                                FailureItem.reason = ex.GetType().FullName.ToString();
                                Failures.Add(FailureItem);
                                continue;
                            }
                        }
                    }
                    //assigning file uploaded status to ViewBag for showing message to user.  
                    TempData["Success"] = SuccessCount.ToString() + " files uploaded successfully.";
                    ViewBag.FailureItems = Failures;
                }
                else
                {
                    TempData["Error"] = "Folder Name is Missed, Mention The Folder Name.";
                }
            }
            else
            {
                TempData["Error"] = "Fill the all Reqired Fields.";
            }

            return View("~/Views/Admin/BASBulkNewsInsertion.cshtml");
        }

    }
}
