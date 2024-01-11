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
    public class PA_CourtCasesController : BaseController
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
                return RedirectToAction("View", "PA_CourtCases", new
                {
                    collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString(),
                    courtCaseId = ""
                });
            }
            if (userCollegeID > 0 && (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry")))
            {
                return RedirectToAction("Edit", "PA_CourtCases", new
                {
                    collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString(),
                    courtCaseId = ""
                });
            }

            return View();
        }

        [Authorize(Roles = "Admin,College")]
        public ActionResult View(string collegeId, string courtCaseId)
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
                bool isPageEditable = db.jntuh_pa_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("PCC") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

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

            var courtCasesList = db.jntuh_college_courtcases.Where(c => c.collegeid == userCollegeID && c.isactive == true).OrderByDescending(c => c.yearoffilling).ToList();
            List<CourtCases> courtCasesListObj = new List<CourtCases>();
            foreach (var item in courtCasesList)
            {
                CourtCases courtCasesObj = new CourtCases();
                courtCasesObj.WporSlorOtherNo = item.wporslorotherno;
                courtCasesObj.YearofFilling = db.jntuh_academic_year.Where(a => a.id == item.yearoffilling).Select(a => a.academicYear).FirstOrDefault();
                courtCasesObj.PrayerofthePetitioner = item.prayerofthepetitioner;
                courtCasesObj.Respondents = item.respondents;
                courtCasesObj.WpPetitionerFilepath = item.wppetitionerdocument;
                courtCasesObj.OrderCopypath = item.ordercopy;
                courtCasesObj.InternOrderCopyPath = item.intermorder;
                courtCasesObj.courtCaseId = item.id;
                courtCasesObj.CollegeId = item.collegeid;

                courtCasesListObj.Add(courtCasesObj);
            }
            ViewBag.CourtCasesList = courtCasesListObj;
            return View();
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Edit(string collegeId, string courtCaseId)
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
                return RedirectToAction("Create", "PA_CourtCases");
            }
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }

            var status = GetPageEditableStatus(userCollegeID);

            if (status == 0 && Roles.IsUserInRole("College"))
            {
                ViewBag.IsEditable = false;
                return RedirectToAction("View", "PA_CourtCases");
            }
            else
            {
                ViewBag.IsEditable = true;

                bool isPageEditable = db.jntuh_pa_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("PCC") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

                if (isPageEditable)
                {
                    ViewBag.IsEditable = true;
                }
                else
                {
                    ViewBag.IsEditable = false;
                    return RedirectToAction("View", "PA_CourtCases");
                }
            }
            var courtCasesList = db.jntuh_college_courtcases.Where(c => c.collegeid == userCollegeID && c.isactive == true).OrderByDescending(c => c.yearoffilling).ToList();
            List<CourtCases> courtCasesListObj = new List<CourtCases>();
            foreach (var item in courtCasesList)
            {
                CourtCases courtCasesObj = new CourtCases();
                courtCasesObj.WporSlorOtherNo = item.wporslorotherno;
                courtCasesObj.YearofFilling = db.jntuh_academic_year.Where(a => a.id == item.yearoffilling).Select(a => a.academicYear).FirstOrDefault();
                courtCasesObj.PrayerofthePetitioner = item.prayerofthepetitioner;
                courtCasesObj.Respondents = item.respondents;
                courtCasesObj.WpPetitionerFilepath = item.wppetitionerdocument;
                courtCasesObj.OrderCopypath = item.ordercopy;
                courtCasesObj.InternOrderCopyPath = item.intermorder;
                courtCasesObj.courtCaseId = item.id;
                courtCasesObj.CollegeId = item.collegeid;

                courtCasesListObj.Add(courtCasesObj);
            }
            ViewBag.CourtCasesList = courtCasesListObj;
            ViewBag.YearofFilling = db.jntuh_academic_year.Where(a => a.isActive && a.actualYear > 2013).OrderByDescending(a => a.actualYear).ToList();
            List<SelectListItem> respondents = new List<SelectListItem>();
            for (int i = 1; i <= 10; i++)
            {
                respondents.Add(new SelectListItem { Text = i.ToString(), Value = i.ToString() });
            }
            ViewBag.Respondents = respondents;
            CourtCasesModel courtcasesmodel = new CourtCasesModel();
            if (courtCaseId != null)
            {
                int dec_courtCaseId = Convert.ToInt32(Utilities.DecryptString(courtCaseId, WebConfigurationManager.AppSettings["CryptoKey"]));
                var court_case = db.jntuh_college_courtcases.Find(dec_courtCaseId);
                courtcasesmodel.CollegeId = court_case.collegeid;
                courtcasesmodel.CourtCaseId = court_case.id;
                courtcasesmodel.WporSlorOtherNo = court_case.wporslorotherno;
                courtcasesmodel.YearofFilling = court_case.yearoffilling;
                courtcasesmodel.PrayerofthePetitioner = court_case.prayerofthepetitioner;
                courtcasesmodel.Respondents = court_case.respondents;
                courtcasesmodel.WpPetitionerFilepath = court_case.wppetitionerdocument;
                courtcasesmodel.OrderCopypath = court_case.ordercopy;
                courtcasesmodel.InternOrderCopyPath = court_case.intermorder;
            }
            courtcasesmodel.CollegeId = userCollegeID;
            return View("Create", courtcasesmodel);
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        [HttpPost]
        public ActionResult Edit(CourtCasesModel courtcasesmodel)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            string collegeCode = db.jntuh_college.Where(u => u.id == userCollegeID).Select(u => u.collegeCode).FirstOrDefault();

            if (courtcasesmodel.CourtCaseId > 0)
            {
                //Update
                var jntuh_courtcases = db.jntuh_college_courtcases.Where(a => a.id == courtcasesmodel.CourtCaseId).Select(a => a).FirstOrDefault();
                jntuh_courtcases.collegeid = courtcasesmodel.CollegeId;
                jntuh_courtcases.wporslorotherno = courtcasesmodel.WporSlorOtherNo;
                jntuh_courtcases.yearoffilling = courtcasesmodel.YearofFilling;
                jntuh_courtcases.prayerofthepetitioner = courtcasesmodel.PrayerofthePetitioner;
                jntuh_courtcases.respondents = courtcasesmodel.Respondents;

                if (courtcasesmodel.WpPetitionerFile != null)
                {
                    string wpPetitionerfile = "~/Content/Upload/College/CourtCases/WP Petitioner";
                    if (!Directory.Exists(Server.MapPath(wpPetitionerfile)))
                    {
                        Directory.CreateDirectory(Server.MapPath(wpPetitionerfile));
                    }
                    var ext = Path.GetExtension(courtcasesmodel.WpPetitionerFile.FileName);
                    if (ext.ToUpper().Equals(".PDF") || ext.ToUpper().Equals(".PDF"))
                    {
                        if (courtcasesmodel.WpPetitionerFilepath == null)
                        {
                            string fileName = collegeCode + "-" + DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" + "JNTUH";
                            courtcasesmodel.WpPetitionerFile.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(wpPetitionerfile), fileName, ext));
                            courtcasesmodel.WpPetitionerFilepath = string.Format("{0}{1}", fileName, ext);
                            jntuh_courtcases.wppetitionerdocument = courtcasesmodel.WpPetitionerFilepath;
                        }
                        else
                        {
                            courtcasesmodel.WpPetitionerFile.SaveAs(string.Format("{0}/{1}", Server.MapPath(wpPetitionerfile), courtcasesmodel.WpPetitionerFilepath));
                            jntuh_courtcases.wppetitionerdocument = courtcasesmodel.WpPetitionerFilepath;
                        }
                    }
                }

                if (courtcasesmodel.OrderCopy != null)
                {
                    string ordercopyfile = "~/Content/Upload/College/CourtCases/Order Copy";
                    if (!Directory.Exists(Server.MapPath(ordercopyfile)))
                    {
                        Directory.CreateDirectory(Server.MapPath(ordercopyfile));
                    }
                    var ext = Path.GetExtension(courtcasesmodel.OrderCopy.FileName);
                    if (ext.ToUpper().Equals(".PDF") || ext.ToUpper().Equals(".PDF"))
                    {
                        if (courtcasesmodel.OrderCopypath == null)
                        {
                            string fileName = collegeCode + "-" + DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" + "JNTUH";
                            courtcasesmodel.OrderCopy.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(ordercopyfile), fileName, ext));
                            courtcasesmodel.OrderCopypath = string.Format("{0}{1}", fileName, ext);
                            jntuh_courtcases.ordercopy = courtcasesmodel.OrderCopypath;
                        }
                        else
                        {
                            courtcasesmodel.OrderCopy.SaveAs(string.Format("{0}/{1}", Server.MapPath(ordercopyfile), courtcasesmodel.OrderCopypath));
                            jntuh_courtcases.ordercopy = courtcasesmodel.OrderCopypath;
                        }
                    }
                }

                if (courtcasesmodel.InternOrderCopy != null)
                {
                    string internOrdercopyfile = "~/Content/Upload/College/CourtCases/Intern Order Copy";
                    if (!Directory.Exists(Server.MapPath(internOrdercopyfile)))
                    {
                        Directory.CreateDirectory(Server.MapPath(internOrdercopyfile));
                    }
                    var ext = Path.GetExtension(courtcasesmodel.InternOrderCopy.FileName);
                    if (ext.ToUpper().Equals(".PDF") || ext.ToUpper().Equals(".PDF"))
                    {
                        if (courtcasesmodel.InternOrderCopyPath == null)
                        {
                            string fileName = collegeCode + "-" + DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" + "JNTUH";
                            courtcasesmodel.InternOrderCopy.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(internOrdercopyfile), fileName, ext));
                            courtcasesmodel.InternOrderCopyPath = string.Format("{0}{1}", fileName, ext);
                            jntuh_courtcases.intermorder = courtcasesmodel.InternOrderCopyPath;
                        }
                        else
                        {
                            courtcasesmodel.InternOrderCopy.SaveAs(string.Format("{0}/{1}", Server.MapPath(internOrdercopyfile), courtcasesmodel.InternOrderCopyPath));
                            jntuh_courtcases.intermorder = courtcasesmodel.InternOrderCopyPath;
                        }
                    }
                }
                jntuh_courtcases.updatedby = userID;
                jntuh_courtcases.updatedon = DateTime.Now;

                db.Entry(jntuh_courtcases).State = EntityState.Modified;
                db.SaveChanges();

                TempData["Success"] = "Updated successfully";
            }
            else
            {
                //Add
                jntuh_college_courtcases courtcases = new jntuh_college_courtcases();
                courtcases.collegeid = courtcasesmodel.CollegeId;
                courtcases.wporslorotherno = courtcasesmodel.WporSlorOtherNo;
                courtcases.yearoffilling = courtcasesmodel.YearofFilling;
                courtcases.prayerofthepetitioner = courtcasesmodel.PrayerofthePetitioner;
                courtcases.respondents = courtcasesmodel.Respondents;

                if (courtcasesmodel.WpPetitionerFile != null)
                {
                    string wpPetitionerfile = "~/Content/Upload/College/CourtCases/WP Petitioner";
                    if (!Directory.Exists(Server.MapPath(wpPetitionerfile)))
                    {
                        Directory.CreateDirectory(Server.MapPath(wpPetitionerfile));
                    }
                    var ext = Path.GetExtension(courtcasesmodel.WpPetitionerFile.FileName);
                    if (ext.ToUpper().Equals(".PDF") || ext.ToUpper().Equals(".PDF"))
                    {
                        if (courtcasesmodel.WpPetitionerFilepath == null)
                        {
                            string fileName = collegeCode + "-" + DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" + "JNTUH";
                            courtcasesmodel.WpPetitionerFile.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(wpPetitionerfile), fileName, ext));
                            courtcasesmodel.WpPetitionerFilepath = string.Format("{0}{1}", fileName, ext);
                            courtcases.wppetitionerdocument = courtcasesmodel.WpPetitionerFilepath;
                        }
                        else
                        {
                            courtcasesmodel.WpPetitionerFile.SaveAs(string.Format("{0}/{1}", Server.MapPath(wpPetitionerfile), courtcasesmodel.WpPetitionerFilepath));
                            courtcases.wppetitionerdocument = courtcasesmodel.WpPetitionerFilepath;
                        }
                    }
                }
                else
                {
                    courtcases.wppetitionerdocument = courtcasesmodel.WpPetitionerFilepath;
                }

                if (courtcasesmodel.OrderCopy != null)
                {
                    string ordercopyfile = "~/Content/Upload/College/CourtCases/Order Copy";
                    if (!Directory.Exists(Server.MapPath(ordercopyfile)))
                    {
                        Directory.CreateDirectory(Server.MapPath(ordercopyfile));
                    }
                    var ext = Path.GetExtension(courtcasesmodel.OrderCopy.FileName);
                    if (ext.ToUpper().Equals(".PDF") || ext.ToUpper().Equals(".PDF"))
                    {
                        if (courtcasesmodel.OrderCopypath == null)
                        {
                            string fileName = collegeCode + "-" + DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" + "JNTUH";
                            courtcasesmodel.OrderCopy.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(ordercopyfile), fileName, ext));
                            courtcasesmodel.OrderCopypath = string.Format("{0}{1}", fileName, ext);
                            courtcases.ordercopy = courtcasesmodel.OrderCopypath;
                        }
                        else
                        {
                            courtcasesmodel.OrderCopy.SaveAs(string.Format("{0}/{1}", Server.MapPath(ordercopyfile), courtcasesmodel.OrderCopypath));
                            courtcases.ordercopy = courtcasesmodel.OrderCopypath;
                        }
                    }
                }
                else
                {
                    courtcases.ordercopy = courtcasesmodel.OrderCopypath;
                }

                if (courtcasesmodel.InternOrderCopy != null)
                {
                    string internOrdercopyfile = "~/Content/Upload/College/CourtCases/Intern Order Copy";
                    if (!Directory.Exists(Server.MapPath(internOrdercopyfile)))
                    {
                        Directory.CreateDirectory(Server.MapPath(internOrdercopyfile));
                    }
                    var ext = Path.GetExtension(courtcasesmodel.InternOrderCopy.FileName);
                    if (ext.ToUpper().Equals(".PDF") || ext.ToUpper().Equals(".PDF"))
                    {
                        if (courtcasesmodel.InternOrderCopyPath == null)
                        {
                            string fileName = collegeCode + "-" + DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" + "JNTUH";
                            courtcasesmodel.InternOrderCopy.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(internOrdercopyfile), fileName, ext));
                            courtcasesmodel.InternOrderCopyPath = string.Format("{0}{1}", fileName, ext);
                            courtcases.intermorder = courtcasesmodel.InternOrderCopyPath;
                        }
                        else
                        {
                            courtcasesmodel.InternOrderCopy.SaveAs(string.Format("{0}/{1}", Server.MapPath(internOrdercopyfile), courtcasesmodel.InternOrderCopyPath));
                            courtcases.intermorder = courtcasesmodel.InternOrderCopyPath;
                        }
                    }
                }
                else
                {
                    courtcases.intermorder = courtcasesmodel.InternOrderCopyPath;
                }

                courtcases.isactive = true;
                courtcases.createdby = userID;
                courtcases.createdon = DateTime.Now;

                db.jntuh_college_courtcases.Add(courtcases);
                db.SaveChanges();

                TempData["Success"] = "Added successfully";
            }
            return RedirectToAction("Edit", "PA_CourtCases", new
            {
                collegeId = Utilities.EncryptString(courtcasesmodel.CollegeId.ToString(),
                    WebConfigurationManager.AppSettings["CryptoKey"]).ToString(),
                courtCaseId = ""
            });
            //return View("View");
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Delete(string collegeId, string courtCaseId)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);

            int dec_courtCaseId = Convert.ToInt32(Utilities.DecryptString(courtCaseId, WebConfigurationManager.AppSettings["CryptoKey"]));

            var jntuh_college_courtcase = db.jntuh_college_courtcases.Where(a => a.id == dec_courtCaseId).FirstOrDefault();
            db.Entry(jntuh_college_courtcase).State = EntityState.Deleted;
            db.SaveChanges();

            TempData["Success"] = "Deleted successfully";

            return RedirectToAction("Edit", "PA_CourtCases", new
            {
                collegeId = collegeId,
                courtCaseId = ""
            });
        }
    }

    public class CourtCases
    {
        public int CollegeId { get; set; }

        public int courtCaseId { get; set; }

        public string WporSlorOtherNo { get; set; }

        public string YearofFilling { get; set; }

        public string PrayerofthePetitioner { get; set; }

        public int? Respondents { get; set; }

        public string WpPetitionerFilepath { get; set; }

        public string OrderCopypath { get; set; }

        public string InternOrderCopyPath { get; set; }
    }
}
