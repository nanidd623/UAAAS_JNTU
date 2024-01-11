using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
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
    public class PA_NBAAccreditationController : BaseController
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
                return RedirectToAction("View", "PA_NBAAccreditation", new
                {
                    collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString()
                });
            }
            if (userCollegeID > 0 && (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry")))
            {
                return RedirectToAction("Edit", "PA_NBAAccreditation", new
                {
                    collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString()
                });
            }
            return View();
        }

        [Authorize(Roles = "Admin,College")]
        public ActionResult View(string collegeId)
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
                bool isPageEditable = db.jntuh_pa_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("PNA") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

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

            var NBAList = db.jntuh_college_nbaaccreditationdata.Where(c => c.collegeid == userCollegeID && c.isactive == true).OrderByDescending(c => c.nbafrom).ToList();
            List<NBA> nbaListObj = new List<NBA>();
            foreach (var item in NBAList)
            {
                NBA nbaObj = new NBA();
                nbaObj.CollegeId = item.collegeid;
                nbaObj.Sno = item.sno;
                nbaObj.AcademicYear = db.jntuh_academic_year.Where(a => a.id == item.accademicyear).Select(a => a.academicYear).FirstOrDefault();
                nbaObj.Specialization = db.jntuh_specialization.Where(a => a.id == item.specealizationid).Select(a => a.specializationName).FirstOrDefault();

                int deptId = db.jntuh_specialization.Where(a => a.id == item.specealizationid).Select(a => a.departmentId).FirstOrDefault();
                nbaObj.Department = db.jntuh_department.Where(a => a.id == deptId).Select(a => a.departmentName).FirstOrDefault();

                int degreeId = db.jntuh_department.Where(a => a.id == deptId).Select(a => a.degreeId).FirstOrDefault();
                nbaObj.Degree = db.jntuh_degree.Where(a => a.id == degreeId).Select(a => a.degree).FirstOrDefault();

                nbaObj.NbaFrom = string.Format("{0:dd/MM/yyyy}", item.nbafrom);
                nbaObj.NbaTo = string.Format("{0:dd/MM/yyyy}", item.nbato);
                nbaObj.NbaApprovalLetterPath = item.nbaapprovalletter;

                nbaListObj.Add(nbaObj);
            }
            ViewBag.NBAList = nbaListObj;
            NBAAccreditationModel nbamodel = new NBAAccreditationModel();
            return View(nbamodel);
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Edit(string collegeId, string sno)
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
                return RedirectToAction("Create", "PA_NBAAccreditation");
            }
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }

            var status = GetPageEditableStatus(userCollegeID);

            if (status == 0 && Roles.IsUserInRole("College"))
            {
                ViewBag.IsEditable = false;
                return RedirectToAction("View", "PA_NBAAccreditation");
            }
            else
            {
                ViewBag.IsEditable = true;

                bool isPageEditable = db.jntuh_pa_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("PNA") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

                if (isPageEditable)
                {
                    ViewBag.IsEditable = true;
                }
                else
                {
                    ViewBag.IsEditable = false;
                    return RedirectToAction("View", "PA_NBAAccreditation");
                }
            }
            var NBAList = db.jntuh_college_nbaaccreditationdata.Where(c => c.collegeid == userCollegeID && c.isactive == true).OrderByDescending(c => c.nbafrom).ToList();
            List<NBA> nbaListObj = new List<NBA>();
            foreach (var item in NBAList)
            {
                NBA nbaObj = new NBA();
                nbaObj.CollegeId = item.collegeid;
                nbaObj.Sno = item.sno;
                nbaObj.AcademicYear = db.jntuh_academic_year.Where(a => a.id == item.accademicyear).Select(a => a.academicYear).FirstOrDefault();
                nbaObj.Specialization = db.jntuh_specialization.Where(a => a.id == item.specealizationid).Select(a => a.specializationName).FirstOrDefault();

                int deptId = db.jntuh_specialization.Where(a => a.id == item.specealizationid).Select(a => a.departmentId).FirstOrDefault();
                nbaObj.Department = db.jntuh_department.Where(a => a.id == deptId).Select(a => a.departmentName).FirstOrDefault();

                int degreeId = db.jntuh_department.Where(a => a.id == deptId).Select(a => a.degreeId).FirstOrDefault();
                nbaObj.Degree = db.jntuh_degree.Where(a => a.id == degreeId).Select(a => a.degree).FirstOrDefault();

                nbaObj.NbaFrom = string.Format("{0:dd/MM/yyyy}", item.nbafrom);
                nbaObj.NbaTo = string.Format("{0:dd/MM/yyyy}", item.nbato);
                nbaObj.NbaApprovalLetterPath = item.nbaapprovalletter;

                nbaListObj.Add(nbaObj);
            }
            ViewBag.NBAList = nbaListObj;

            ViewBag.AcademicYears = db.jntuh_academic_year.Where(a => a.isActive && a.actualYear > 2013).OrderByDescending(a => a.actualYear).ToList();

            var degreeID = db.jntuh_college_degree.Where(cd => cd.collegeId == userCollegeID && cd.isActive == true).Select(cd => cd.degreeId).ToList();

            ViewBag.Degrees = db.jntuh_degree.Where(d => degreeID.Contains(d.id)).ToList();

            NBAAccreditationModel nbamodel = new NBAAccreditationModel();
            if (sno != null)
            {
                int dec_sno = Convert.ToInt32(Utilities.DecryptString(sno, WebConfigurationManager.AppSettings["CryptoKey"]));
                var nba = db.jntuh_college_nbaaccreditationdata.Find(dec_sno);
                nbamodel.CollegeId = nba.collegeid;
                nbamodel.Sno = nba.sno;
                nbamodel.AcademicYear = nba.accademicyear;
                //nbamodel.DegreeId = nba.degree;
                //nbamodel.DepartmentId = nba.department;
                nbamodel.SpecializationId = nba.specealizationid;
                //nbamodel.NbaFrom = nba.nbafrom;
                //nbamodel.NbaTo = nba.nbato;
                nbamodel.NbaApprovalLetterPath = nba.nbaapprovalletter;
            }
            nbamodel.CollegeId = userCollegeID;
            return View("Create", nbamodel);
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        [HttpPost]
        public ActionResult Edit(NBAAccreditationModel nbamodel)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            string collegeCode = db.jntuh_college.Where(u => u.id == userCollegeID).Select(u => u.collegeCode).FirstOrDefault();

            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }

            var NBAList = db.jntuh_college_nbaaccreditationdata.Where(a => a.collegeid == userCollegeID && a.isactive == true).OrderBy(a => a.sno).ToList();

            bool newAffiliation = false;
            bool returnStatus = false;
            if (NBAList.Count == 0)
            {
                newAffiliation = true;
            }
            else
            {
                var affFromDate = Convert.ToDateTime(DateTime.ParseExact(nbamodel.NbaFrom, "dd/MM/yyyy", CultureInfo.InvariantCulture));
                var affToDate = Convert.ToDateTime(DateTime.ParseExact(nbamodel.NbaTo, "dd/MM/yyyy", CultureInfo.InvariantCulture));

                foreach (var item in NBAList)
                {
                    newAffiliation = false;

                    returnStatus = IsBetween(affFromDate, item.nbafrom, item.nbato);

                    if (!returnStatus)
                    {
                        returnStatus = IsBetween(affToDate, item.nbafrom, item.nbato);

                        if (!returnStatus)
                        {
                            returnStatus = IsBetween(item.nbafrom, affFromDate, affToDate);

                            if (!returnStatus)
                            {
                                returnStatus = IsBetween(item.nbato, affFromDate, affToDate);

                                if (!returnStatus)
                                {
                                    newAffiliation = true;
                                }
                                else
                                {
                                    if (nbamodel.SpecializationId == item.specealizationid)
                                    {
                                        newAffiliation = false;
                                        break;
                                    }
                                    else
                                    {
                                        newAffiliation = true;
                                        continue;
                                    }
                                }
                            }
                            else
                            {
                                if (nbamodel.SpecializationId == item.specealizationid)
                                {
                                    newAffiliation = false;
                                    break;
                                }
                                else
                                {
                                    newAffiliation = true;
                                    continue;
                                }
                            }
                        }
                        else
                        {
                            if (nbamodel.SpecializationId == item.specealizationid)
                            {
                                newAffiliation = false;
                                break;
                            }
                            else
                            {
                                newAffiliation = true;
                                continue;
                            }
                        }
                    }
                    else
                    {
                        if (nbamodel.SpecializationId == item.specealizationid)
                        {
                            newAffiliation = false;
                            break;
                        }
                        else
                        {
                            newAffiliation = true;
                            continue;
                        }
                    }
                }
            }

            if (newAffiliation)
            {
                jntuh_college_nbaaccreditationdata nba = new jntuh_college_nbaaccreditationdata();
                nba.collegeid = nbamodel.CollegeId;
                nba.accademicyear = nbamodel.AcademicYear;
                nba.specealizationid = nbamodel.SpecializationId;
                nba.nbafrom = Convert.ToDateTime(DateTime.ParseExact(nbamodel.NbaFrom, "dd/MM/yyyy", CultureInfo.InvariantCulture)); //Convert.ToDateTime(nbamodel.NbaFrom);
                nba.nbato = Convert.ToDateTime(DateTime.ParseExact(nbamodel.NbaTo, "dd/MM/yyyy", CultureInfo.InvariantCulture)); //Convert.ToDateTime(nbamodel.NbaTo);

                if (nbamodel.NbaApprovalLetter != null)
                {
                    string SupportingDocumentfile = "~/Content/Upload/College/NBAAccredited_Latest";
                    if (!Directory.Exists(Server.MapPath(SupportingDocumentfile)))
                    {
                        Directory.CreateDirectory(Server.MapPath(SupportingDocumentfile));
                    }
                    var ext = Path.GetExtension(nbamodel.NbaApprovalLetter.FileName);
                    if (ext.ToUpper().Equals(".PDF") || ext.ToUpper().Equals(".PDF"))
                    {
                        if (nbamodel.NbaApprovalLetterPath == null)
                        {
                            string fileName = collegeCode + "-" + DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" + "JNTUH";
                            nbamodel.NbaApprovalLetter.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(SupportingDocumentfile), fileName, ext));
                            nbamodel.NbaApprovalLetterPath = string.Format("{0}{1}", fileName, ext);
                            nba.nbaapprovalletter = nbamodel.NbaApprovalLetterPath;
                        }
                        else
                        {
                            nbamodel.NbaApprovalLetter.SaveAs(string.Format("{0}/{1}", Server.MapPath(SupportingDocumentfile), nbamodel.NbaApprovalLetterPath));
                            nba.nbaapprovalletter = nbamodel.NbaApprovalLetterPath;
                        }
                    }
                }
                else
                {
                    nba.nbaapprovalletter = nbamodel.NbaApprovalLetterPath;
                }

                nba.isactive = true;
                nba.createdby = userID;
                nba.createdon = DateTime.Now;

                db.jntuh_college_nbaaccreditationdata.Add(nba);
                db.SaveChanges();

                TempData["Success"] = "Added successfully";
            }
            else
            {
                //Show that particular affiliation is already present.
                TempData["Error"] = "There is an NBA Accreditation already in the given Dates for the given Specialization.";
            }
            return RedirectToAction("Edit", "PA_NBAAccreditation", new
            {
                collegeId = Utilities.EncryptString(nbamodel.CollegeId.ToString(),
                    WebConfigurationManager.AppSettings["CryptoKey"]).ToString()
            });
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Delete(string collegeId, string sno)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);

            int dec_sno = Convert.ToInt32(Utilities.DecryptString(sno, WebConfigurationManager.AppSettings["CryptoKey"]));

            var nba = db.jntuh_college_nbaaccreditationdata.Where(a => a.sno == dec_sno).FirstOrDefault();
            db.Entry(nba).State = EntityState.Deleted;
            db.SaveChanges();

            TempData["Success"] = "Deleted successfully";

            return RedirectToAction("Edit", "PA_NBAAccreditation", new
            {
                collegeId = collegeId
            });
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult GetDepartments(string id)
        {
            if (Membership.GetUser() == null)
            {
                return Json("login again", JsonRequestBehavior.AllowGet);
            }
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userID).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();
            if (id == string.Empty)
            {
                id = "0";
            }
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            //New Code Return by Narayana Reddy on 05-03-2019.
            int did = Convert.ToInt32(id);
            var departments = (from cs in db.jntuh_approvedadmitted_intake
                               join s in db.jntuh_specialization on cs.SpecializationId equals s.id
                               join d in db.jntuh_department on s.departmentId equals d.id
                               join de in db.jntuh_degree on d.degreeId equals de.id
                               where de.id == did && cs.collegeId == userCollegeID
                               select new
                               {
                                   degreeId = de.id,
                                   degree = de.degree,
                                   DepartmentId = d.id,
                                   Department = d.departmentName,
                                   specializationId = s.id,
                                   specialization = s.specializationName
                               }).ToList();

            var departmentList = departments.GroupBy(g => g.DepartmentId).Select(s => s.FirstOrDefault()).ToList();
            var departmentsData = departmentList.Select(a => new SelectListItem()
            {
                Text = a.Department,
                Value = a.DepartmentId.ToString(),
            });
            return Json(departmentsData, JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult GetSpecialization(string id)
        {
            if (Membership.GetUser() == null)
            {
                return Json("login again", JsonRequestBehavior.AllowGet);
            }
            if (id == string.Empty)
            {
                id = "0";
            }
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userID).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            int deid = Convert.ToInt32(id);
            var secializationList = (from cs in db.jntuh_approvedadmitted_intake
                                     join s in db.jntuh_specialization on cs.SpecializationId equals s.id
                                     join d in db.jntuh_department on s.departmentId equals d.id
                                     join de in db.jntuh_degree on d.degreeId equals de.id
                                     where d.id == deid && cs.collegeId == userCollegeID
                                     select new
                                     {
                                         degreeId = de.id,
                                         degree = de.degree,
                                         DepartmentId = d.id,
                                         Department = d.departmentName,
                                         specializationId = s.id,
                                         specialization = s.specializationName
                                     }).ToList();
            secializationList =
                secializationList.GroupBy(g => g.specializationId).Select(e => e.FirstOrDefault()).ToList();
            var specializationdata = secializationList.Select(s => new SelectListItem()
            {
                Text = s.specialization,
                Value = s.specializationId.ToString(),
            });
            return Json(specializationdata, JsonRequestBehavior.AllowGet);
        }

        public bool IsBetween(DateTime? newDate, DateTime? fromDate, DateTime? toDate)
        {
            if (newDate == fromDate) return true;
            if (newDate == toDate) return true;

            if (fromDate <= toDate)
                return (newDate >= fromDate && newDate <= toDate);
            else
                return !(newDate >= toDate && newDate <= fromDate);
        }
    }

    public class NBA
    {
        public long Sno { get; set; }

        public int CollegeId { get; set; }

        public string AcademicYear { get; set; }

        public string Degree { get; set; }

        public string Department { get; set; }

        public string Specialization { get; set; }

        public string NbaFrom { get; set; }

        public string NbaTo { get; set; }

        public string NbaApprovalLetterPath { get; set; }
    }
}
